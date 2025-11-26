using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace CyberApp_FIA.Services
{
    /// <summary>
    /// Event catalog service for University Admins.
    /// - Stores events in ~/App_Data/eventCatalog.xml
    /// - Allows editing event details and soft-deleting events.
    /// - Soft-deleted events are marked isDeleted=true and should not be shown
    ///   in participant-facing catalogs.
    /// - Writes all changes into the central audit log with category="Catalog".
    /// </summary>
    public class EventCatalogService
    {
        private readonly string _path;

        public EventCatalogService()
        {
            var ctx = HttpContext.Current ?? throw new InvalidOperationException("No HttpContext.");
            var appData = ctx.Server.MapPath("~/App_Data");
            Directory.CreateDirectory(appData);
            _path = Path.Combine(appData, "eventCatalog.xml");
            EnsureStore();
        }

        private void EnsureStore()
        {
            if (File.Exists(_path)) return;

            var doc = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                new XElement("events",
                    new XAttribute("version", "1")
                )
            );
            doc.Save(_path);
        }

        public IList<EventRecord> GetEventsForUniversity(string universityId)
        {
            var doc = XDocument.Load(_path);
            var root = doc.Root;
            if (root == null) return new List<EventRecord>();

            return root.Elements("event")
                .Where(e => !string.Equals((string)e.Attribute("isDeleted"), "true", StringComparison.OrdinalIgnoreCase))
                .Where(e => string.Equals((string)e.Attribute("universityId"), universityId ?? "", StringComparison.OrdinalIgnoreCase))
                .Select(ToRecord)
                .OrderBy(e => e.StartUtc)
                .ToList();
        }

        public EventRecord GetById(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;

            var doc = XDocument.Load(_path);
            var root = doc.Root;
            if (root == null) return null;

            var node = root.Elements("event").FirstOrDefault(e => (string)e.Attribute("id") == id);
            return node == null ? null : ToRecord(node);
        }

        /// <summary>
        /// Update event details (time, room, enabled flag, etc.).
        /// University Admins can call this from their event editing page.
        /// </summary>
        public void Save(EventRecord record)
        {
            if (record == null) throw new ArgumentNullException(nameof(record));
            if (string.IsNullOrWhiteSpace(record.Id)) throw new InvalidOperationException("Event ID is required.");

            var doc = XDocument.Load(_path);
            var root = doc.Root;
            if (root == null) throw new InvalidOperationException("Invalid eventCatalog.xml.");

            var node = root.Elements("event").FirstOrDefault(e => (string)e.Attribute("id") == record.Id);
            if (node == null)
            {
                // If not found, we do nothing here. This keeps behavior predictable.
                return;
            }

            node.SetAttributeValue("isEnabled", record.IsEnabled ? "true" : "false");
            node.SetAttributeValue("room", record.Room ?? string.Empty);

            node.SetElementValue("title", record.Title ?? string.Empty);
            node.SetElementValue("startUtc", record.StartUtc.ToString("o"));
            node.SetElementValue("endUtc", record.EndUtc.ToString("o"));
            node.SetElementValue("updatedUtc", DateTime.UtcNow.ToString("o"));

            doc.Save(_path);

            var audit = new AuditLogService();
            audit.AppendFromSession(
                category: "Catalog",
                actionType: "UpdateEvent",
                targetType: "Event",
                targetId: record.Id,
                targetLabel: record.Title ?? string.Empty,
                notes: "University Admin updated event details.",
                meta: new Dictionary<string, string>
                {
                    { "eventId", record.Id },
                    { "courseId", record.CourseId ?? string.Empty },
                    { "universityId", record.UniversityId ?? string.Empty },
                    { "isEnabled", record.IsEnabled ? "true" : "false" }
                });
        }

        /// <summary>
        /// Soft-delete an event so it stops appearing in participant catalogs.
        /// </summary>
        public void SoftDelete(string id, string reasonNote = null)
        {
            if (string.IsNullOrWhiteSpace(id)) return;

            var doc = XDocument.Load(_path);
            var root = doc.Root;
            if (root == null) return;

            var node = root.Elements("event").FirstOrDefault(e => (string)e.Attribute("id") == id);
            if (node == null) return;

            var title = (string)node.Element("title") ?? "";
            var courseId = (string)node.Attribute("courseId") ?? "";
            var universityId = (string)node.Attribute("universityId") ?? "";

            node.SetAttributeValue("isDeleted", "true");
            node.SetAttributeValue("isEnabled", "false");
            node.SetElementValue("deletedUtc", DateTime.UtcNow.ToString("o"));

            doc.Save(_path);

            var audit = new AuditLogService();
            audit.AppendFromSession(
                category: "Catalog",
                actionType: "DeleteEvent",
                targetType: "Event",
                targetId: id,
                targetLabel: title,
                notes: string.IsNullOrWhiteSpace(reasonNote)
                    ? "University Admin deleted an event."
                    : reasonNote,
                meta: new Dictionary<string, string>
                {
                    { "eventId", id },
                    { "courseId", courseId },
                    { "universityId", universityId }
                });
        }

        private static EventRecord ToRecord(XElement e)
        {
            var record = new EventRecord
            {
                Id = (string)e.Attribute("id") ?? "",
                CourseId = (string)e.Attribute("courseId") ?? "",
                UniversityId = (string)e.Attribute("universityId") ?? "",
                Room = (string)e.Attribute("room") ?? "",
                IsEnabled = string.Equals((string)e.Attribute("isEnabled"), "true", StringComparison.OrdinalIgnoreCase),
                Title = (string)e.Element("title") ?? ""
            };

            DateTime start;
            if (DateTime.TryParse((string)e.Element("startUtc"), out start))
            {
                record.StartUtc = start;
            }

            DateTime end;
            if (DateTime.TryParse((string)e.Element("endUtc"), out end))
            {
                record.EndUtc = end;
            }

            return record;
        }
    }

    /// <summary>
    /// Small event DTO used by EventCatalogService and UI pages.
    /// </summary>
    public class EventRecord
    {
        public string Id { get; set; }
        public string CourseId { get; set; }
        public string UniversityId { get; set; }
        public string Title { get; set; }
        public string Room { get; set; }
        public bool IsEnabled { get; set; }
        public DateTime StartUtc { get; set; }
        public DateTime EndUtc { get; set; }
    }
}
