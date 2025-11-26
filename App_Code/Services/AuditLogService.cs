using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Xml.Linq;

namespace CyberApp_FIA.Services
{
    /// <summary>
    /// Central, file-backed audit log helper.
    /// Writes entries into ~/App_Data/auditLog.xml and supports basic querying.
    /// This service does not change existing flows – it just provides helpers
    /// that other pages can call when you are ready to hook them in.
    /// </summary>
    public class AuditLogService
    {
        private readonly string _auditPath;
        private readonly int _retentionDays;

        public AuditLogService()
        {
            var ctx = HttpContext.Current ?? throw new InvalidOperationException("No HttpContext.");
            var appData = ctx.Server.MapPath("~/App_Data");
            Directory.CreateDirectory(appData);

            _auditPath = Path.Combine(appData, "auditLog.xml");
            _retentionDays = 1095; // ~3 years of history by default

            EnsureStore();
        }

        private void EnsureStore()
        {
            if (File.Exists(_auditPath))
            {
                return;
            }

            var doc = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                new XElement("auditLog",
                    new XAttribute("version", "1"),
                    new XAttribute("retentionDays", _retentionDays)
                )
            );

            doc.Save(_auditPath);
        }

        /// <summary>
        /// Append a new audit record to the XML store.
        /// </summary>
        public void Append(AuditLogEntry entry)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));

            EnsureStore();

            var doc = XDocument.Load(_auditPath);
            var root = doc.Root;
            if (root == null) return;

            var id = string.IsNullOrWhiteSpace(entry.Id)
                ? Guid.NewGuid().ToString("N")
                : entry.Id;

            var element = new XElement("entry",
                new XAttribute("id", id),
                new XAttribute("timestampUtc", entry.TimestampUtc.ToString("o")),
                new XAttribute("actorUserId", entry.ActorUserId ?? string.Empty),
                new XAttribute("actorRole", entry.ActorRole ?? string.Empty),
                new XAttribute("actorEmail", entry.ActorEmail ?? string.Empty),
                new XAttribute("actorDisplayName", entry.ActorDisplayName ?? string.Empty),
                new XAttribute("actorUniversity", entry.ActorUniversity ?? string.Empty),
                new XAttribute("category", entry.Category ?? string.Empty),
                new XAttribute("actionType", entry.ActionType ?? string.Empty),
                new XAttribute("targetType", entry.TargetType ?? string.Empty),
                new XAttribute("targetId", entry.TargetId ?? string.Empty),
                new XAttribute("targetLabel", entry.TargetLabel ?? string.Empty),
                new XAttribute("clientIp", entry.ClientIp ?? string.Empty),
                new XAttribute("userAgentHash", SafeHash(entry.UserAgent ?? string.Empty)),
                new XAttribute("consentVersion", entry.ConsentVersion ?? string.Empty),
                new XAttribute("severity", string.IsNullOrWhiteSpace(entry.Severity) ? "Info" : entry.Severity)
            );

            if (!string.IsNullOrWhiteSpace(entry.Notes))
            {
                element.Add(new XElement("notes", entry.Notes));
            }

            if (entry.Metadata != null && entry.Metadata.Count > 0)
            {
                var metaElement = new XElement("meta");
                foreach (var kvp in entry.Metadata)
                {
                    // Only store simple, non-sensitive values here.
                    metaElement.Add(
                        new XElement("item",
                            new XAttribute("key", kvp.Key),
                            kvp.Value ?? string.Empty));
                }
                element.Add(metaElement);
            }

            root.Add(element);

            PruneOldEntries(root);

            doc.Save(_auditPath);
        }

        /// <summary>
        /// Convenience helper that tries to fill actor information from the current session.
        /// Use this from pages that have UserId/Role/Email in Session.
        /// </summary>
        public void AppendFromSession(string category, string actionType, string targetType, string targetId,
            string targetLabel, string notes = null, IDictionary<string, string> meta = null,
            string severity = "Info", string consentVersion = null)
        {
            var ctx = HttpContext.Current;
            var session = ctx?.Session;

            var entry = new AuditLogEntry
            {
                Category = category,
                ActionType = actionType,
                TargetType = targetType,
                TargetId = targetId,
                TargetLabel = targetLabel,
                Notes = notes,
                Metadata = meta != null ? new Dictionary<string, string>(meta) : new Dictionary<string, string>(),
                Severity = severity,
                ConsentVersion = consentVersion,
                TimestampUtc = DateTime.UtcNow,
                ActorUserId = session?["UserId"] as string ?? "",
                ActorRole = session?["Role"] as string ?? "",
                ActorEmail = session?["Email"] as string ?? "",
                ActorDisplayName = session?["DisplayName"] as string ?? "", // optional
                ActorUniversity = session?["University"] as string ?? "",
                ClientIp = ctx?.Request?.UserHostAddress ?? "",
                UserAgent = ctx?.Request?.UserAgent ?? ""
            };

            Append(entry);
        }

        /// <summary>
        /// Basic query function for viewer pages (search, filter, date range).
        /// This is intentionally simple and in-memory for now.
        /// </summary>
        public IList<AuditLogEntry> Query(AuditLogQuery query)
        {
            EnsureStore();

            var doc = XDocument.Load(_auditPath);
            var root = doc.Root;
            if (root == null)
            {
                return new List<AuditLogEntry>();
            }

            var entries = root.Elements("entry");

            if (!string.IsNullOrWhiteSpace(query.Category))
            {
                entries = entries.Where(e =>
                    (string)e.Attribute("category") ??
                    string.Empty).Equals(query.Category, StringComparison.OrdinalIgnoreCase);
            }

            if (!string.IsNullOrWhiteSpace(query.Role))
            {
                entries = entries.Where(e =>
                    (string)e.Attribute("actorRole") ??
                    string.Empty).Equals(query.Role, StringComparison.OrdinalIgnoreCase);
            }

            if (!string.IsNullOrWhiteSpace(query.University))
            {
                entries = entries.Where(e =>
                    (string)e.Attribute("actorUniversity") ??
                    string.Empty).Equals(query.University, StringComparison.OrdinalIgnoreCase);
            }

            if (query.FromUtc.HasValue)
            {
                entries = entries.Where(e =>
                {
                    var tsStr = (string)e.Attribute("timestampUtc") ?? "";
                    if (!DateTime.TryParse(tsStr, null, DateTimeStyles.AdjustToUniversal, out var ts)) return false;
                    return ts >= query.FromUtc.Value;
                });
            }

            if (query.ToUtc.HasValue)
            {
                entries = entries.Where(e =>
                {
                    var tsStr = (string)e.Attribute("timestampUtc") ?? "";
                    if (!DateTime.TryParse(tsStr, null, DateTimeStyles.AdjustToUniversal, out var ts)) return false;
                    return ts <= query.ToUtc.Value;
                });
            }

            if (!string.IsNullOrWhiteSpace(query.SearchText))
            {
                var term = query.SearchText.Trim();
                entries = entries.Where(e =>
                {
                    string GetAttr(string name) => (string)e.Attribute(name) ?? "";
                    string GetElem(string name) => (string)e.Element(name) ?? "";

                    var haystack = string.Join(" ",
                        GetAttr("actorEmail"),
                        GetAttr("actorDisplayName"),
                        GetAttr("actorRole"),
                        GetAttr("category"),
                        GetAttr("actionType"),
                        GetAttr("targetType"),
                        GetAttr("targetId"),
                        GetAttr("targetLabel"),
                        GetElem("notes"));

                    return haystack.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0;
                });
            }

            var list = entries
                .Select(ToEntry)
                .OrderByDescending(e => e.TimestampUtc)
                .ToList();

            if (query.MaxRows.HasValue)
            {
                list = list.Take(query.MaxRows.Value).ToList();
            }

            return list;
        }

        private static AuditLogEntry ToEntry(XElement e)
        {
            var entry = new AuditLogEntry
            {
                Id = (string)e.Attribute("id") ?? "",
                ActorUserId = (string)e.Attribute("actorUserId") ?? "",
                ActorRole = (string)e.Attribute("actorRole") ?? "",
                ActorEmail = (string)e.Attribute("actorEmail") ?? "",
                ActorDisplayName = (string)e.Attribute("actorDisplayName") ?? "",
                ActorUniversity = (string)e.Attribute("actorUniversity") ?? "",
                Category = (string)e.Attribute("category") ?? "",
                ActionType = (string)e.Attribute("actionType") ?? "",
                TargetType = (string)e.Attribute("targetType") ?? "",
                TargetId = (string)e.Attribute("targetId") ?? "",
                TargetLabel = (string)e.Attribute("targetLabel") ?? "",
                ClientIp = (string)e.Attribute("clientIp") ?? "",
                UserAgent = "", // we only store the hash on disk
                ConsentVersion = (string)e.Attribute("consentVersion") ?? "",
                Severity = (string)e.Attribute("severity") ?? "Info",
                Notes = (string)e.Element("notes") ?? string.Empty
            };

            var tsStr = (string)e.Attribute("timestampUtc") ?? "";
            if (DateTime.TryParse(tsStr, null, DateTimeStyles.AdjustToUniversal, out var ts))
            {
                entry.TimestampUtc = ts;
            }

            var meta = new Dictionary<string, string>();
            var metaElem = e.Element("meta");
            if (metaElem != null)
            {
                foreach (var item in metaElem.Elements("item"))
                {
                    var key = (string)item.Attribute("key") ?? "";
                    var val = item.Value ?? "";
                    if (!string.IsNullOrWhiteSpace(key))
                    {
                        meta[key] = val;
                    }
                }
            }

            entry.Metadata = meta;
            return entry;
        }

        private void PruneOldEntries(XElement root)
        {
            var now = DateTime.UtcNow;

            var retention = _retentionDays;
            var attr = root.Attribute("retentionDays")?.Value;
            if (int.TryParse(attr, out var parsed))
            {
                retention = parsed;
            }

            var cutoff = now.AddDays(-retention);

            var oldEntries = root.Elements("entry").Where(e =>
            {
                var tsStr = (string)e.Attribute("timestampUtc") ?? "";
                return DateTime.TryParse(tsStr, null, DateTimeStyles.AdjustToUniversal, out var ts)
                       && ts < cutoff;
            }).ToList();

            foreach (var e in oldEntries)
            {
                e.Remove();
            }
        }

        /// <summary>
        /// Hash user-agent to avoid storing the full string in the log.
        /// Very small helper to protect privacy while still giving us device grouping.
        /// </summary>
        private static string SafeHash(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;

            using (var sha = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(value);
                var hash = sha.ComputeHash(bytes);
                return "sha256:" + BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }
    }

    /// <summary>
    /// DTO for log entries used by the service and viewer pages.
    /// </summary>
    public class AuditLogEntry
    {
        public string Id { get; set; }
        public DateTime TimestampUtc { get; set; }
        public string ActorUserId { get; set; }
        public string ActorRole { get; set; }
        public string ActorEmail { get; set; }
        public string ActorDisplayName { get; set; }
        public string ActorUniversity { get; set; }
        public string Category { get; set; }
        public string ActionType { get; set; }
        public string TargetType { get; set; }
        public string TargetId { get; set; }
        public string TargetLabel { get; set; }
        public string ClientIp { get; set; }
        public string UserAgent { get; set; }
        public string ConsentVersion { get; set; }
        public string Severity { get; set; }
        public string Notes { get; set; }
        public IDictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
    }

    /// <summary>
    /// Simple query object to let pages filter the log.
    /// </summary>
    public class AuditLogQuery
    {
        public string Category { get; set; }
        public string Role { get; set; }
        public string University { get; set; }
        public string SearchText { get; set; }
        public DateTime? FromUtc { get; set; }
        public DateTime? ToUtc { get; set; }
        public int? MaxRows { get; set; }
    }
}
