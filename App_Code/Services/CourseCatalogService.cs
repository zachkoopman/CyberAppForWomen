using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace CyberApp_FIA.Services
{
    /// <summary>
    /// Simple course catalog service for Super Admin use.
    /// - Stores courses in ~/App_Data/courseCatalog.xml
    /// - Prevents duplicate IDs and titles.
    /// - Writes catalog changes into the central audit log.
    /// 
    /// This does NOT change existing catalog pages; it is a shared helper
    /// that those pages can call when you are ready.
    /// </summary>
    public class CourseCatalogService
    {
        private readonly string _path;

        public CourseCatalogService()
        {
            var ctx = HttpContext.Current ?? throw new InvalidOperationException("No HttpContext.");
            var appData = ctx.Server.MapPath("~/App_Data");
            Directory.CreateDirectory(appData);
            _path = Path.Combine(appData, "courseCatalog.xml");
            EnsureStore();
        }

        private void EnsureStore()
        {
            if (File.Exists(_path)) return;

            var doc = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                new XElement("courses",
                    new XAttribute("version", "1")
                )
            );
            doc.Save(_path);
        }

        public IList<CourseRecord> GetAll()
        {
            var doc = XDocument.Load(_path);
            var root = doc.Root;
            if (root == null) return new List<CourseRecord>();

            return root.Elements("course")
                .Select(ToRecord)
                .OrderBy(c => c.Title)
                .ToList();
        }

        public CourseRecord GetById(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;

            var doc = XDocument.Load(_path);
            var root = doc.Root;
            if (root == null) return null;

            var node = root.Element("course");
            // For simplicity, this version expects only one course element at a time in this example file.
            // If you add more courses, you can switch to root.Elements("course").FirstOrDefault(...).
            node = root.Elements("course").FirstOrDefault(x => (string)x.Attribute("id") == id);

            return node == null ? null : ToRecord(node);
        }

        /// <summary>
        /// Creates or updates a course.
        /// - If record.Id is empty, a new course is created.
        /// - If record.Id is present, that course is updated.
        /// - Titles must be unique (case-insensitive) across active courses.
        /// - Writes a Catalog entry into the audit log.
        /// </summary>
        public CourseRecord Save(CourseRecord record)
        {
            if (record == null) throw new ArgumentNullException(nameof(record));

            var doc = XDocument.Load(_path);
            var root = doc.Root;
            if (root == null) throw new InvalidOperationException("Invalid courseCatalog.xml.");

            var now = DateTime.UtcNow;
            var isNew = string.IsNullOrWhiteSpace(record.Id);
            var id = isNew ? Guid.NewGuid().ToString("N") : record.Id.Trim();

            // Ensure unique title across not-deleted courses.
            var existingTitles = root.Elements("course")
                .Where(c => !Equals((string)c.Attribute("isDeleted"), "true"))
                .Where(c => !string.Equals((string)c.Attribute("id"), id, StringComparison.OrdinalIgnoreCase))
                .Select(c => ((string)c.Element("title") ?? "").Trim())
                .ToList();

            if (existingTitles.Any(t => t.Equals(record.Title.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException("A course with this title already exists.");
            }

            XElement node;
            if (isNew)
            {
                node = new XElement("course",
                    new XAttribute("id", id),
                    new XAttribute("ownerUniversity", record.OwnerUniversity ?? string.Empty),
                    new XAttribute("isPublished", record.IsPublished ? "true" : "false"),
                    new XAttribute("isDeleted", "false"),
                    new XElement("title", record.Title ?? string.Empty),
                    new XElement("shortCode", record.ShortCode ?? string.Empty),
                    new XElement("description", record.Description ?? string.Empty),
                    new XElement("createdUtc", now.ToString("o")),
                    new XElement("updatedUtc", now.ToString("o"))
                );
                root.Add(node);
            }
            else
            {
                node = root.Elements("course")
                    .FirstOrDefault(c => (string)c.Attribute("id") == id);

                if (node == null)
                {
                    // If the course is missing, treat this as a new one for safety.
                    node = new XElement("course",
                        new XAttribute("id", id),
                        new XAttribute("ownerUniversity", record.OwnerUniversity ?? string.Empty),
                        new XAttribute("isPublished", record.IsPublished ? "true" : "false"),
                        new XAttribute("isDeleted", "false"),
                        new XElement("title", record.Title ?? string.Empty),
                        new XElement("shortCode", record.ShortCode ?? string.Empty),
                        new XElement("description", record.Description ?? string.Empty),
                        new XElement("createdUtc", now.ToString("o")),
                        new XElement("updatedUtc", now.ToString("o"))
                    );
                    root.Add(node);
                }
                else
                {
                    node.SetAttributeValue("ownerUniversity", record.OwnerUniversity ?? string.Empty);
                    node.SetAttributeValue("isPublished", record.IsPublished ? "true" : "false");

                    node.SetElementValue("title", record.Title ?? string.Empty);
                    node.SetElementValue("shortCode", record.ShortCode ?? string.Empty);
                    node.SetElementValue("description", record.Description ?? string.Empty);
                    node.SetElementValue("updatedUtc", now.ToString("o"));
                }
            }

            doc.Save(_path);

            // Write catalog change to the shared audit log.
            var audit = new AuditLogService();
            audit.AppendFromSession(
                category: "Catalog",
                actionType: isNew ? "CreateCourse" : "UpdateCourse",
                targetType: "Course",
                targetId: id,
                targetLabel: record.Title ?? string.Empty,
                notes: isNew ? "Super Admin created a new course." : "Super Admin updated an existing course.",
                meta: new Dictionary<string, string>
                {
                    { "courseId", id },
                    { "title", record.Title ?? string.Empty },
                    { "ownerUniversity", record.OwnerUniversity ?? string.Empty },
                    { "isPublished", record.IsPublished ? "true" : "false" }
                });

            record.Id = id;
            record.UpdatedUtc = now;
            if (isNew)
            {
                record.CreatedUtc = now;
            }

            return record;
        }

        private static CourseRecord ToRecord(XElement course)
        {
            var record = new CourseRecord
            {
                Id = (string)course.Attribute("id") ?? "",
                OwnerUniversity = (string)course.Attribute("ownerUniversity") ?? "",
                IsPublished = string.Equals((string)course.Attribute("isPublished"), "true", StringComparison.OrdinalIgnoreCase),
                Title = (string)course.Element("title") ?? "",
                ShortCode = (string)course.Element("shortCode") ?? "",
                Description = (string)course.Element("description") ?? ""
            };

            DateTime created;
            if (DateTime.TryParse((string)course.Element("createdUtc"), out created))
            {
                record.CreatedUtc = created;
            }

            DateTime updated;
            if (DateTime.TryParse((string)course.Element("updatedUtc"), out updated))
            {
                record.UpdatedUtc = updated;
            }

            return record;
        }
    }

    /// <summary>
    /// Small course DTO used by CourseCatalogService and UI pages.
    /// </summary>
    public class CourseRecord
    {
        public string Id { get; set; }
        public string OwnerUniversity { get; set; }
        public string Title { get; set; }
        public string ShortCode { get; set; }
        public string Description { get; set; }
        public bool IsPublished { get; set; }
        public DateTime CreatedUtc { get; set; }
        public DateTime UpdatedUtc { get; set; }
    }
}
