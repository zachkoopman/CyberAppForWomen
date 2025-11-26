using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace CyberApp_FIA.Services
{
    /// <summary>
    /// Records security-relevant events (login attempts, denied access, impersonation)
    /// and raises simple alerts when thresholds are exceeded.
    /// Alerts are visible to Super Admins through a read-only UI.
    /// </summary>
    public class SecurityMonitoringService
    {
        private readonly string _eventsPath;
        private readonly string _alertsPath;

        public SecurityMonitoringService()
        {
            var ctx = HttpContext.Current ?? throw new InvalidOperationException("No HttpContext.");
            var appData = ctx.Server.MapPath("~/App_Data");
            Directory.CreateDirectory(appData);

            _eventsPath = Path.Combine(appData, "securityEvents.xml");
            _alertsPath = Path.Combine(appData, "securityAlerts.xml");

            EnsureStore(_eventsPath, "securityEvents");
            EnsureStore(_alertsPath, "securityAlerts");
        }

        private void EnsureStore(string path, string rootName)
        {
            if (File.Exists(path)) return;

            var doc = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                new XElement(rootName,
                    new XAttribute("version", "1")
                )
            );
            doc.Save(path);
        }

        /// <summary>
        /// Record a single login attempt and evaluate if alerts are needed.
        /// Call this from your login code when you are ready.
        /// </summary>
        public void RecordLoginAttempt(
            string email,
            string userId,
            string role,
            bool success,
            string clientIp,
            bool isImpersonation,
            string reason)
        {
            var doc = XDocument.Load(_eventsPath);
            var root = doc.Root;
            if (root == null) return;

            var evtId = "login-" + Guid.NewGuid().ToString("N");
            var now = DateTime.UtcNow;

            var element = new XElement("event",
                new XAttribute("id", evtId),
                new XAttribute("tsUtc", now.ToString("o")),
                new XAttribute("type", "LoginAttempt"),
                new XAttribute("email", email ?? string.Empty),
                new XAttribute("userId", userId ?? string.Empty),
                new XAttribute("role", role ?? string.Empty),
                new XAttribute("success", success ? "true" : "false"),
                new XAttribute("clientIp", clientIp ?? string.Empty),
                new XAttribute("isImpersonation", isImpersonation ? "true" : "false"),
                new XAttribute("reason", reason ?? string.Empty)
            );

            root.Add(element);
            doc.Save(_eventsPath);

            if (!success || isImpersonation)
            {
                EvaluateForAlerts(email, clientIp);
            }
        }

        /// <summary>
        /// Record a generic denied access event (for example, an access control failure).
        /// </summary>
        public void RecordDeniedAccess(string email, string userId, string role, string clientIp, string reason)
        {
            var doc = XDocument.Load(_eventsPath);
            var root = doc.Root;
            if (root == null) return;

            var evtId = "denied-" + Guid.NewGuid().ToString("N");
            var now = DateTime.UtcNow;

            var element = new XElement("event",
                new XAttribute("id", evtId),
                new XAttribute("tsUtc", now.ToString("o")),
                new XAttribute("type", "DeniedAccess"),
                new XAttribute("email", email ?? string.Empty),
                new XAttribute("userId", userId ?? string.Empty),
                new XAttribute("role", role ?? string.Empty),
                new XAttribute("clientIp", clientIp ?? string.Empty),
                new XAttribute("reason", reason ?? string.Empty)
            );

            root.Add(element);
            doc.Save(_eventsPath);

            EvaluateForAlerts(email, clientIp);
        }

        /// <summary>
        /// Evaluate recent events for suspicious patterns.
        /// This uses simple thresholds to keep behavior predictable.
        /// </summary>
        private void EvaluateForAlerts(string email, string clientIp)
        {
            var eventsDoc = XDocument.Load(_eventsPath);
            var eventsRoot = eventsDoc.Root;
            if (eventsRoot == null) return;

            var alertsDoc = XDocument.Load(_alertsPath);
            var alertsRoot = alertsDoc.Root;
            if (alertsRoot == null) return;

            var now = DateTime.UtcNow;
            var windowStart = now.AddMinutes(-15); // look at the last 15 minutes

            var recentEvents = eventsRoot.Elements("event")
                .Where(e =>
                {
                    var tsStr = (string)e.Attribute("tsUtc") ?? "";
                    DateTime ts;
                    return DateTime.TryParse(tsStr, out ts) && ts >= windowStart;
                })
                .ToList();

            // Pattern 1: repeated failed logins for the same account
            if (!string.IsNullOrWhiteSpace(email))
            {
                var failedForUser = recentEvents
                    .Where(e => (string)e.Attribute("type") == "LoginAttempt")
                    .Where(e => string.Equals((string)e.Attribute("email"), email, StringComparison.OrdinalIgnoreCase))
                    .Where(e => string.Equals((string)e.Attribute("success"), "false", StringComparison.OrdinalIgnoreCase))
                    .Count();

                if (failedForUser >= 5 && !HasRecentAlert(alertsRoot, "RepeatedFailedLogins", "email", email, windowStart))
                {
                    CreateAlert(alertsRoot, "RepeatedFailedLogins", "Warning",
                        $"Repeated failed logins for {email}",
                        $"Detected {failedForUser} failed login attempts for {email} in the last 15 minutes.",
                        new Dictionary<string, string> { { "email", email } });
                }
            }

            // Pattern 2: spike in denied access from same IP
            if (!string.IsNullOrWhiteSpace(clientIp))
            {
                var deniedFromIp = recentEvents
                    .Where(e => (string)e.Attribute("type") == "DeniedAccess")
                    .Where(e => string.Equals((string)e.Attribute("clientIp"), clientIp, StringComparison.OrdinalIgnoreCase))
                    .Count();

                if (deniedFromIp >= 10 && !HasRecentAlert(alertsRoot, "DeniedAccessSpike", "clientIp", clientIp, windowStart))
                {
                    CreateAlert(alertsRoot, "DeniedAccessSpike", "Warning",
                        $"Spike in denied access from IP {clientIp}",
                        $"Detected {deniedFromIp} denied access events from {clientIp} in the last 15 minutes.",
                        new Dictionary<string, string> { { "clientIp", clientIp } });
                }
            }

            alertsDoc.Save(_alertsPath);
        }

        private bool HasRecentAlert(XElement alertsRoot, string type, string keyName, string keyValue, DateTime windowStart)
        {
            var alerts = alertsRoot.Elements("alert")
                .Where(a => (string)a.Attribute("type") == type)
                .Where(a => (string)a.Attribute(keyName) == keyValue);

            foreach (var a in alerts)
            {
                var tsStr = (string)a.Attribute("tsUtc") ?? "";
                if (DateTime.TryParse(tsStr, out var ts) && ts >= windowStart)
                {
                    // There is a recent alert, so we do not raise another yet.
                    return true;
                }
            }
            return false;
        }

        private void CreateAlert(
            XElement alertsRoot,
            string type,
            string severity,
            string title,
            string description,
            IDictionary<string, string> meta)
        {
            var now = DateTime.UtcNow;
            var id = "alert-" + Guid.NewGuid().ToString("N");

            var alert = new XElement("alert",
                new XAttribute("id", id),
                new XAttribute("tsUtc", now.ToString("o")),
                new XAttribute("type", type),
                new XAttribute("severity", severity),
                new XAttribute("title", title),
                new XAttribute("description", description)
            );

            if (meta != null)
            {
                foreach (var kvp in meta)
                {
                    alert.SetAttributeValue(kvp.Key, kvp.Value ?? string.Empty);
                }
            }

            alertsRoot.Add(alert);

            // Also log to the main audit log so Super Admins see it in their Activity Log.
            var audit = new AuditLogService();
            audit.AppendFromSession(
                category: "Security",
                actionType: "SecurityAlertRaised",
                targetType: "SecurityAlert",
                targetId: id,
                targetLabel: title,
                notes: description,
                meta: meta ?? new Dictionary<string, string>(),
                severity: severity);
        }

        /// <summary>
        /// Load all alerts for display.
        /// </summary>
        public IList<SecurityAlertRecord> GetAllAlerts()
        {
            var doc = XDocument.Load(_alertsPath);
            var root = doc.Root;
            if (root == null) return new List<SecurityAlertRecord>();

            return root.Elements("alert")
                .Select(a => new SecurityAlertRecord
                {
                    Id = (string)a.Attribute("id") ?? "",
                    TimestampUtc = SafeParseUtc((string)a.Attribute("tsUtc")),
                    Type = (string)a.Attribute("type") ?? "",
                    Severity = (string)a.Attribute("severity") ?? "Info",
                    Title = (string)a.Attribute("title") ?? "",
                    Description = (string)a.Attribute("description") ?? ""
                })
                .OrderByDescending(a => a.TimestampUtc)
                .ToList();
        }

        private static DateTime SafeParseUtc(string value)
        {
            if (DateTime.TryParse(value, out var dt))
            {
                return dt;
            }
            return DateTime.UtcNow;
        }
    }

    public class SecurityAlertRecord
    {
        public string Id { get; set; }
        public DateTime TimestampUtc { get; set; }
        public string Type { get; set; }
        public string Severity { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }
}
