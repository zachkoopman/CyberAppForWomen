using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace CyberApp_FIA.Services
{
    /// <summary>
    /// Small helper for spot-checking Helper logs.
    /// - Reads samples from helperDeliveries.xml and helperHelpNotes.xml.
    /// - Records Verify/Question/Skip decisions in helperVerifications.xml.
    /// - Writes each decision into the central audit log for a durable trail.
    /// 
    /// This does not change existing Helper logging flows.
    /// </summary>
    public class HelperVerificationService
    {
        private readonly string _appData;
        private readonly string _deliveriesPath;
        private readonly string _notesPath;
        private readonly string _verificationsPath;
        private readonly string _helperProgressPath;

        public HelperVerificationService()
        {
            var ctx = HttpContext.Current ?? throw new InvalidOperationException("No HttpContext.");
            _appData = ctx.Server.MapPath("~/App_Data");

            _deliveriesPath = Path.Combine(_appData, "helperDeliveries.xml");
            _notesPath = Path.Combine(_appData, "helperHelpNotes.xml");
            _verificationsPath = Path.Combine(_appData, "helperVerifications.xml");
            _helperProgressPath = Path.Combine(_appData, "helperProgress.xml");

            EnsureVerificationsStore();
        }

        private void EnsureVerificationsStore()
        {
            if (File.Exists(_verificationsPath)) return;

            Directory.CreateDirectory(_appData);
            var doc = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                new XElement("helperVerifications",
                    new XAttribute("version", "1")
                )
            );
            doc.Save(_verificationsPath);
        }

        /// <summary>
        /// Returns a light sample of helper logs to spot-check.
        /// For now this simply returns the latest N entries across deliveries and help notes.
        /// </summary>
        public IList<HelperLogSample> GetLatestSamples(int maxCount)
        {
            var samples = new List<HelperLogSample>();

            if (File.Exists(_deliveriesPath))
            {
                var doc = XDocument.Load(_deliveriesPath);
                foreach (var d in doc.Root.Elements("delivery"))
                {
                    var ts = SafeParseUtc((string)d.Attribute("tsUtc"));
                    samples.Add(new HelperLogSample
                    {
                        Source = "TeachingDelivery",
                        HelperId = (string)d.Attribute("helperId") ?? "",
                        HelperName = (string)d.Attribute("helperName") ?? "",
                        CourseId = (string)d.Attribute("courseId") ?? "",
                        CourseTitle = (string)d.Attribute("courseTitle") ?? "",
                        TimestampUtc = ts,
                        Key = BuildKey("delivery", (string)d.Attribute("helperId"), ts, (string)d.Attribute("courseId"))
                    });
                }
            }

            if (File.Exists(_notesPath))
            {
                var doc = XDocument.Load(_notesPath);
                foreach (var n in doc.Root.Elements("note"))
                {
                    var ts = SafeParseUtc((string)n.Attribute("tsUtc"));
                    samples.Add(new HelperLogSample
                    {
                        Source = "OneToOneNote",
                        HelperId = (string)n.Attribute("helperId") ?? "",
                        HelperName = "", // name is optional here; logs may not store it
                        CourseId = (string)n.Attribute("courseId") ?? "",
                        CourseTitle = (string)n.Attribute("courseTitle") ?? "",
                        TimestampUtc = ts,
                        Key = BuildKey("note", (string)n.Attribute("helperId"), ts, (string)n.Attribute("courseId")),
                        Preview = TrimPreview(n.Element("text")?.Value)
                    });
                }
            }

            // Sort newest first and take the requested number.
            return samples
                .OrderByDescending(s => s.TimestampUtc)
                .Take(maxCount)
                .ToList();
        }

        /// <summary>
        /// Records a verification decision and writes it into the main audit log.
        /// </summary>
        public void RecordDecision(HelperVerificationDecision decision)
        {
            EnsureVerificationsStore();

            var doc = XDocument.Load(_verificationsPath);
            var root = doc.Root;
            if (root == null) return;

            var element = new XElement("verification",
                new XAttribute("key", decision.LogKey ?? ""),
                new XAttribute("helperId", decision.HelperId ?? ""),
                new XAttribute("helperName", decision.HelperName ?? ""),
                new XAttribute("source", decision.Source ?? ""),
                new XAttribute("decision", decision.Decision ?? ""),
                new XAttribute("adminEmail", decision.AdminEmail ?? ""),
                new XAttribute("adminUniversity", decision.AdminUniversity ?? ""),
                new XAttribute("tsUtc", DateTime.UtcNow.ToString("o"))
            );

            if (!string.IsNullOrWhiteSpace(decision.Note))
            {
                element.Add(new XElement("note", decision.Note));
            }

            root.Add(element);
            doc.Save(_verificationsPath);

            // Update helper progress in a simple way – bump a counter we can hook into later.
            UpdateHelperProgressCounters(decision);

            // Also write to central audit log so Super Admins can see the trail.
            var audit = new AuditLogService();
            audit.AppendFromSession(
                category: "Helper",
                actionType: $"HelperLog{decision.Decision}",
                targetType: "HelperLog",
                targetId: decision.LogKey,
                targetLabel: $"{decision.Source} for helper {decision.HelperId}",
                notes: decision.Note,
                meta: new Dictionary<string, string>
                {
                    {"helperId", decision.HelperId ?? ""},
                    {"helperName", decision.HelperName ?? ""},
                    {"source", decision.Source ?? ""}
                },
                severity: decision.Decision == "Questioned" ? "Warning" : "Info");
        }

        private void UpdateHelperProgressCounters(HelperVerificationDecision decision)
        {
            // Simple implementation: we only track total verified/questioned counts by helper.
            // This keeps behavior predictable and lets the existing certification progress
            // page pick up new counters later once you wire that in.
            if (!File.Exists(_helperProgressPath)) return;

            var doc = XDocument.Load(_helperProgressPath);
            var root = doc.Root;
            if (root == null) return;

            var helperNode = root.Element("helper");
            if (helperNode == null) return; // for now we only maintain one example helper

            var totals = helperNode.Element("totals");
            if (totals == null)
            {
                totals = new XElement("totals");
                helperNode.Add(totals);
            }

            var verElem = totals.Element("totalSpotCheckedLogs");
            if (verElem == null)
            {
                verElem = new XElement("totalSpotCheckedLogs", "0");
                totals.Add(verElem);
            }

            if (!int.TryParse(verElem.Value, out var total))
            {
                total = 0;
            }

            total++;
            verElem.Value = total.ToString();
            doc.Save(_helperProgressPath);
        }

        private static DateTime SafeParseUtc(string value)
        {
            if (DateTime.TryParse(value, null, System.Globalization.DateTimeStyles.AdjustToUniversal, out var dt))
            {
                return dt;
            }
            return DateTime.UtcNow;
        }

        private static string BuildKey(string kind, string helperId, DateTime ts, string courseId)
        {
            // A small stable key so we can link a verification back to a log row.
            return $"{kind}:{helperId}:{courseId}:{ts:yyyyMMddHHmmss}";
        }

        private static string TrimPreview(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;
            if (text.Length <= 140) return text;
            return text.Substring(0, 140) + "…";
        }

        public sealed class HelperLogSample
        {
            public string Key { get; set; }
            public string Source { get; set; }
            public string HelperId { get; set; }
            public string HelperName { get; set; }
            public string CourseId { get; set; }
            public string CourseTitle { get; set; }
            public DateTime TimestampUtc { get; set; }
            public string Preview { get; set; }
        }

        public sealed class HelperVerificationDecision
        {
            public string LogKey { get; set; }
            public string HelperId { get; set; }
            public string HelperName { get; set; }
            public string Source { get; set; }
            public string Decision { get; set; } // "Verified", "Questioned", "Skip"
            public string Note { get; set; }     // short, non-sensitive explanation
            public string AdminEmail { get; set; }
            public string AdminUniversity { get; set; }
        }
    }
}
