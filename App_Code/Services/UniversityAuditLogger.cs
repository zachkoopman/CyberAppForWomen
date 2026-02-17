using System;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Xml;

namespace CyberApp_FIA.Services
{
    /// <summary>
    /// Central helper for appending rows into ~/App_Data/Audit_Log/UnvAdminAudit.xml
    /// for the university admin audit log viewer.
    /// </summary>
    public static class UniversityAuditLogger
    {
        private static readonly object LockObj = new object();

        /// <summary>
        /// Convenience wrapper: pulls University / Role / Email / UserId from Session
        /// and looks up firstName in users.xml, then appends an entry.
        /// </summary>
        public static void AppendForCurrentUser(Page page, string type, string details)
        {
            if (page == null) return;

            var role = Convert.ToString(page.Session["Role"] ?? "");
            var email = Convert.ToString(page.Session["Email"] ?? "");
            var uni = Convert.ToString(page.Session["University"] ?? "");
            var userId = Convert.ToString(page.Session["UserId"] ?? "");

            var firstName = LookupFirstNameByUserId(page.Server, userId);

            AppendEntry(page.Server, uni, role, type, email, firstName, details);
        }

        /// <summary>
        /// Low-level append API if you want to pass all fields explicitly.
        /// </summary>
        public static void AppendEntry(
            HttpServerUtility server,
            string university,
            string role,
            string type,
            string email,
            string firstName,
            string details)
        {
            if (server == null) return;

            var path = server.MapPath("~/App_Data/Audit_Log/UnvAdminAudit.xml");

            lock (LockObj)
            {
                var doc = new XmlDocument();

                if (File.Exists(path))
                {
                    doc.Load(path);
                }
                else
                {
                    doc.LoadXml("<?xml version='1.0' encoding='utf-8'?><auditLog version='1'></auditLog>");
                }

                var root = doc.DocumentElement ?? (XmlElement)doc.AppendChild(doc.CreateElement("auditLog"));

                var entry = doc.CreateElement("entry");
                entry.SetAttribute("id", "log-" + Guid.NewGuid().ToString("N"));
                entry.SetAttribute("university", university ?? string.Empty);
                entry.SetAttribute("role", role ?? string.Empty);
                entry.SetAttribute("type", type ?? string.Empty);
                entry.SetAttribute("timestamp", DateTime.UtcNow.ToString("o"));
                entry.SetAttribute("email", email ?? string.Empty);
                entry.SetAttribute("firstName", firstName ?? string.Empty);

                var d = doc.CreateElement("details");
                d.InnerText = details ?? string.Empty;
                entry.AppendChild(d);

                root.AppendChild(entry);
                doc.Save(path);
            }
        }

        private static string LookupFirstNameByUserId(HttpServerUtility server, string userId)
        {
            try
            {
                if (server == null || string.IsNullOrWhiteSpace(userId)) return string.Empty;

                var usersPath = server.MapPath("~/App_Data/users.xml");
                if (!File.Exists(usersPath)) return string.Empty;

                var doc = new XmlDocument();
                doc.Load(usersPath);
                var node = doc.SelectSingleNode($"/users/user[@id='{userId}']/firstName");
                return node?.InnerText ?? string.Empty;
            }
            catch
            {
                // Never break the main flow if audit logging fails.
                return string.Empty;
            }
        }
    }
}
