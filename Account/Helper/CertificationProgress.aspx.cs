using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;

namespace CyberApp_FIA.Helper
{
    /// <summary>
    /// Helper-facing certification progress page.
    /// - Loads current microcourses from ~/App_Data/microcourses.xml
    /// - Looks up their certification rules via <requiredRules><rule id="..."/>
    ///   in ~/App_Data/certificationRules.xml (id or rule name)
    /// - Shows module-level Certified / Eligible / Not certified status and per-requirement progress.
    /// - Lets Helpers open resources in a viewer and confirm they’ve completed them
    ///   (which marks the quiz requirement as met).
    /// - Ensures helperProgress.xml contains an entry for this Helper
    ///   for every current microcourse, with rule metadata and zero progress.
    /// - Automatically updates <isEligible> and <isCertified> when the Helper meets requirements.
    /// </summary>
    public partial class CertificationProgress : Page
    {
        private string RulesXmlPath => Server.MapPath("~/App_Data/certificationRules.xml");
        private string MicrocoursesXmlPath => Server.MapPath("~/App_Data/microcourses.xml");
        private string HelperProgressXmlPath => Server.MapPath("~/App_Data/helperProgress.xml");

        protected void Page_Load(object sender, EventArgs e)
        {
            // Gate: Helpers only
            var role = (string)Session["Role"];
            if (!string.Equals(role, "Helper", StringComparison.OrdinalIgnoreCase))
            {
                Response.Redirect("~/Account/Login.aspx");
                return;
            }

            var helperId = Session["UserId"] as string ?? string.Empty;

            if (!IsPostBack)
            {
                HelperName.Text = GetHelperDisplayName();

                // Seed helperProgress.xml for this helper if needed
                EnsureHelperProgressSeeded(helperId, HelperName.Text);

                BindCertificationView();
            }
        }

        private string GetHelperDisplayName()
        {
            var name = Session["HelperName"] as string;
            if (!string.IsNullOrWhiteSpace(name))
            {
                return Server.HtmlEncode(name);
            }

            var email = Session["Email"] as string;
            if (!string.IsNullOrWhiteSpace(email) && email.Contains("@"))
            {
                return Server.HtmlEncode(email.Split('@')[0]);
            }

            return "Peer Helper";
        }

        private void EnsureRulesXml()
        {
            if (File.Exists(RulesXmlPath)) return;
            Directory.CreateDirectory(Path.GetDirectoryName(RulesXmlPath));
            File.WriteAllText(RulesXmlPath, "<?xml version='1.0' encoding='utf-8'?><certRules version='1'></certRules>");
        }

        private void EnsureMicrocoursesXml()
        {
            if (File.Exists(MicrocoursesXmlPath)) return;
            Directory.CreateDirectory(Path.GetDirectoryName(MicrocoursesXmlPath));
            File.WriteAllText(MicrocoursesXmlPath, "<?xml version='1.0' encoding='utf-8'?><microcourses version='1'></microcourses>");
        }

        private void EnsureHelperProgressXml()
        {
            if (File.Exists(HelperProgressXmlPath)) return;
            Directory.CreateDirectory(Path.GetDirectoryName(HelperProgressXmlPath));
            File.WriteAllText(HelperProgressXmlPath, "<?xml version='1.0' encoding='utf-8'?><helperProgress version='1'></helperProgress>");
        }

        private sealed class RuleInfo
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public bool RequireQuiz { get; set; }
            public int PassScore { get; set; }
            public int MinSessionsTaught { get; set; }
            public int MinHelpSessions { get; set; }
            public int ExpiryDays { get; set; }
        }

        private sealed class MicrocourseInfo
        {
            public string Id { get; set; }
            public string Title { get; set; }
            public string Summary { get; set; }
            public string ExternalLink { get; set; }

            /// <summary>
            /// Value from <requiredRules><rule id="..."/></requiredRules>.
            /// In your current XML, this is the RULE NAME ("Quiz", "Easy Rule", etc.),
            /// not the numeric rule id ("3", "4", "5"), so we support both.
            /// </summary>
            public string RuleId { get; set; }
        }

        private sealed class HelperProgressInfo
        {
            public int QuizScore { get; set; }
            public int TeachingSessions { get; set; }
            public int HelpSessions { get; set; }
        }

        private sealed class ModuleStatus
        {
            public string CourseId { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public bool HasRule { get; set; }

            public string StatusLabel { get; set; }
            public string StatusCssClass { get; set; }

            public string HeaderStatusLabel { get; set; }
            public string HeaderStatusCss { get; set; }

            public string QuizRequirementText { get; set; }
            public string QuizProgressText { get; set; }
            public string QuizStatusText { get; set; }
            public string QuizStatusCss { get; set; }

            public string TeachingRequirementText { get; set; }
            public string TeachingProgressText { get; set; }
            public string TeachingStatusText { get; set; }
            public string TeachingStatusCss { get; set; }

            public string HelpRequirementText { get; set; }
            public string HelpProgressText { get; set; }
            public string HelpStatusText { get; set; }
            public string HelpStatusCss { get; set; }

            public string ExpiryText { get; set; }
            public string ExpiryMetaText { get; set; }

            public string RuleMetaText { get; set; }

            public bool IsCertified { get; set; }
            public bool IsEligible { get; set; }
            public int SortKey { get; set; }

            // Resources UI
            public string ExternalLink { get; set; }
            public bool HasExternalLink { get; set; }
            public string ResourceViewerUrl { get; set; }
        }

        /// <summary>
        /// Ensure this helper has a course entry for every current microcourse,
        /// with rule metadata and zeroed progress.
        /// Called on first load after sign-in for Helpers.
        /// </summary>
        private void EnsureHelperProgressSeeded(string helperId, string helperDisplayName)
        {
            if (string.IsNullOrWhiteSpace(helperId))
            {
                return;
            }

            EnsureHelperProgressXml();
            EnsureMicrocoursesXml();
            EnsureRulesXml();

            var courses = LoadMicrocourses();
            var rulesByKey = LoadRulesByKey();

            var doc = new XmlDocument();
            doc.Load(HelperProgressXmlPath);

            var root = doc.DocumentElement;
            if (root == null)
            {
                root = doc.CreateElement("helperProgress");
                root.SetAttribute("version", "1");
                doc.AppendChild(root);
            }

            var helperEl = (XmlElement)root.SelectSingleNode($"./helper[@id='{helperId}']");
            if (helperEl == null)
            {
                helperEl = doc.CreateElement("helper");
                helperEl.SetAttribute("id", helperId);
                root.AppendChild(helperEl);
            }

            // Store / refresh a simple display name snapshot
            var displayNameEl = helperEl["displayName"] ?? doc.CreateElement("displayName");
            displayNameEl.InnerText = helperDisplayName ?? string.Empty;
            if (displayNameEl.ParentNode == null) helperEl.AppendChild(displayNameEl);

            foreach (var course in courses)
            {
                var courseEl = (XmlElement)helperEl.SelectSingleNode($"./course[@id='{course.Id}']");
                if (courseEl == null)
                {
                    courseEl = doc.CreateElement("course");
                    courseEl.SetAttribute("id", course.Id);
                    helperEl.AppendChild(courseEl);
                }

                XmlElement EnsureChild(XmlElement parent, string name, string defaultValue)
                {
                    var n = parent[name] ?? doc.CreateElement(name);
                    if (n.ParentNode == null) parent.AppendChild(n);
                    if (string.IsNullOrEmpty(n.InnerText))
                    {
                        n.InnerText = defaultValue;
                    }
                    return n;
                }

                // Snapshot title so the XML is readable
                EnsureChild(courseEl, "title", course.Title ?? string.Empty);

                // Rule metadata from requiredRules -> certRules
                RuleInfo rule = null;
                if (!string.IsNullOrWhiteSpace(course.RuleId))
                {
                    rulesByKey.TryGetValue(course.RuleId.Trim(), out rule);
                }

                if (rule != null)
                {
                    EnsureChild(courseEl, "ruleKey", course.RuleId ?? string.Empty);   // e.g., "Quiz", "Easy Rule"
                    EnsureChild(courseEl, "ruleId", rule.Id ?? string.Empty);         // e.g., "3", "4", "5"
                    EnsureChild(courseEl, "ruleName", rule.Name ?? string.Empty);
                    EnsureChild(courseEl, "requireQuiz", rule.RequireQuiz ? "true" : "false");
                    EnsureChild(courseEl, "passScore", rule.PassScore.ToString());
                    EnsureChild(courseEl, "minSessionsTaught", rule.MinSessionsTaught.ToString());
                    EnsureChild(courseEl, "minHelpSessions", rule.MinHelpSessions.ToString());
                    EnsureChild(courseEl, "expiryDays", rule.ExpiryDays.ToString());
                }
                else
                {
                    // No known rule; still seed the node
                    EnsureChild(courseEl, "ruleKey", string.Empty);
                }

                // Progress values: zero by default
                EnsureChild(courseEl, "quizScore", "0");
                EnsureChild(courseEl, "teachingSessions", "0");
                EnsureChild(courseEl, "helpSessions", "0");
                EnsureChild(courseEl, "isEligible", "false");
                EnsureChild(courseEl, "isCertified", "false");
                EnsureChild(courseEl, "resourcesConfirmed", "false");

                var updatedEl = courseEl["lastUpdatedUtc"] ?? doc.CreateElement("lastUpdatedUtc");
                if (string.IsNullOrEmpty(updatedEl.InnerText))
                {
                    updatedEl.InnerText = DateTime.UtcNow.ToString("o");
                }
                if (updatedEl.ParentNode == null) courseEl.AppendChild(updatedEl);
            }

            // Recalculate "isEligible" / "isCertified" per course
            RecalculateCertificationForHelper(doc, helperEl);

            // Helper-level totals (all zero initially, but computed generically)
            UpdateHelperTotals(doc, helperEl);

            doc.Save(HelperProgressXmlPath);
        }

        private void BindCertificationView()
        {
            EnsureRulesXml();
            EnsureMicrocoursesXml();
            EnsureHelperProgressXml();

            // dictionary keyed by both rule ID *and* rule Name
            var rulesByKey = LoadRulesByKey();
            var courses = LoadMicrocourses();

            var helperId = Session["UserId"] as string ?? string.Empty;
            var modules = new List<ModuleStatus>();

            foreach (var course in courses)
            {
                RuleInfo rule = null;
                if (!string.IsNullOrWhiteSpace(course.RuleId))
                {
                    // course.RuleId might be "3" (id) OR "Quiz" (name)
                    rulesByKey.TryGetValue(course.RuleId.Trim(), out rule);
                }

                var progress = GetHelperProgressForModule(helperId, course.Id, course.Title);
                var moduleStatus = BuildModuleStatus(course, rule, progress);
                modules.Add(moduleStatus);
            }

            var sorted = modules
                .OrderBy(m => m.SortKey)
                .ToList();

            var certified = sorted.Where(m => m.IsCertified).ToList();
            var eligible = sorted.Where(m => m.IsEligible && !m.IsCertified).ToList();
            var notCertified = sorted.Where(m => !m.IsEligible && !m.IsCertified).ToList();

            NotCertifiedRepeater.DataSource = notCertified;
            NotCertifiedRepeater.DataBind();
            NotCertifiedPH.Visible = notCertified.Count > 0;

            EligibleRepeater.DataSource = eligible;
            EligibleRepeater.DataBind();
            EligiblePH.Visible = eligible.Count > 0;

            CertifiedRepeater.DataSource = certified;
            CertifiedRepeater.DataBind();
            CertifiedPH.Visible = certified.Count > 0;

            RequirementsRepeater.DataSource = sorted;
            RequirementsRepeater.DataBind();
        }

        /// <summary>
        /// Loads rules and returns a dictionary keyed by BOTH:
        /// - rule id (e.g., "3"), and
        /// - rule name (e.g., "Quiz").
        /// That way, microcourses can reference either one in <requiredRules><rule id="..."/>.
        /// </summary>
        private Dictionary<string, RuleInfo> LoadRulesByKey()
        {
            var doc = new XmlDocument();
            doc.Load(RulesXmlPath);

            var dict = new Dictionary<string, RuleInfo>(StringComparer.OrdinalIgnoreCase);

            foreach (XmlElement r in doc.SelectNodes("/certRules/rule"))
            {
                var id = r.GetAttribute("id");
                var name = r["name"]?.InnerText ?? string.Empty;

                if (string.IsNullOrWhiteSpace(id) && string.IsNullOrWhiteSpace(name))
                    continue;

                var info = new RuleInfo
                {
                    Id = id,
                    Name = name,
                    Description = r["description"]?.InnerText ?? string.Empty,
                    RequireQuiz = (r["requireQuiz"]?.InnerText ?? "false")
                        .Equals("true", StringComparison.OrdinalIgnoreCase),
                    PassScore = ParseIntSafe(r["passScore"]?.InnerText, 0),
                    MinSessionsTaught = ParseIntSafe(r["minSessionsTaught"]?.InnerText, 0),
                    MinHelpSessions = ParseIntSafe(r["minHelpSessions"]?.InnerText, 0),
                    ExpiryDays = ParseIntSafe(r["expiryDays"]?.InnerText, 0)
                };

                // Key by numeric id if present
                if (!string.IsNullOrWhiteSpace(info.Id))
                {
                    dict[info.Id] = info;
                }

                // Also key by rule name so microcourses can reference "Quiz", "Easy Rule", etc.
                if (!string.IsNullOrWhiteSpace(info.Name) && !dict.ContainsKey(info.Name))
                {
                    dict[info.Name] = info;
                }
            }

            return dict;
        }

        private List<MicrocourseInfo> LoadMicrocourses()
        {
            var doc = new XmlDocument();
            doc.Load(MicrocoursesXmlPath);

            var list = new List<MicrocourseInfo>();

            foreach (XmlElement c in doc.SelectNodes("/microcourses/course[@status='Published' or @status='Draft']"))
            {
                var id = c.GetAttribute("id");
                var title = c["title"]?.InnerText ?? "(Untitled microcourse)";
                var summary = c["summary"]?.InnerText ?? string.Empty;
                var externalLink = (c["externalLink"]?.InnerText ?? string.Empty).Trim();

                string ruleId = null;
                var rContainer = c.SelectSingleNode("./requiredRules");
                if (rContainer != null)
                {
                    // Assumption: at most one main cert rule per microcourse; if multiple, we take the first.
                    var firstRule = rContainer.SelectSingleNode("./rule") as XmlElement;
                    if (firstRule != null)
                    {
                        ruleId = firstRule.GetAttribute("id");
                    }
                }

                list.Add(new MicrocourseInfo
                {
                    Id = id,
                    Title = title,
                    Summary = summary,
                    ExternalLink = externalLink,
                    RuleId = ruleId
                });
            }

            return list;
        }

        private static int ParseIntSafe(string text, int fallback)
        {
            if (int.TryParse(text ?? string.Empty, out var value))
            {
                if (value < 0) return 0;
                return value;
            }
            return fallback;
        }

        /// <summary>
        /// Reads the Helper's stored progress for a given microcourse from helperProgress.xml.
        /// </summary>
        private HelperProgressInfo GetHelperProgressForModule(string helperId, string courseId, string courseTitle)
        {
            EnsureHelperProgressXml();

            if (string.IsNullOrWhiteSpace(helperId) || string.IsNullOrWhiteSpace(courseId))
            {
                return new HelperProgressInfo { QuizScore = 0, TeachingSessions = 0, HelpSessions = 0 };
            }

            var doc = new XmlDocument();
            doc.Load(HelperProgressXmlPath);

            var node = (XmlElement)doc.SelectSingleNode(
                $"/helperProgress/helper[@id='{helperId}']/course[@id='{courseId}']");

            if (node == null)
            {
                return new HelperProgressInfo
                {
                    QuizScore = 0,
                    TeachingSessions = 0,
                    HelpSessions = 0
                };
            }

            return new HelperProgressInfo
            {
                QuizScore = ParseIntSafe(node["quizScore"]?.InnerText, 0),
                TeachingSessions = ParseIntSafe(node["teachingSessions"]?.InnerText, 0),
                HelpSessions = ParseIntSafe(node["helpSessions"]?.InnerText, 0)
            };
        }

        private ModuleStatus BuildModuleStatus(MicrocourseInfo course, RuleInfo rule, HelperProgressInfo progress)
        {
            var hasRule = rule != null;

            var requireQuiz = hasRule && rule.RequireQuiz;
            var passScore = hasRule ? rule.PassScore : 0;
            var minTeach = hasRule ? rule.MinSessionsTaught : 0;
            var minHelp = hasRule ? rule.MinHelpSessions : 0;
            var expiryDays = hasRule ? rule.ExpiryDays : 0;

            var extLink = course.ExternalLink ?? string.Empty;
            var hasExternal = !string.IsNullOrWhiteSpace(extLink);
            var viewerUrl = hasExternal
                ? ResolveUrl("~/Account/Helper/ResourceViewer.aspx?courseId="
                             + Server.UrlEncode(course.Id) + "&url="
                             + Server.UrlEncode(extLink))
                : string.Empty;

            // Quiz req + progress
            bool quizMet = requireQuiz
                ? (passScore > 0 && progress.QuizScore >= passScore)
                : true;

            string quizReqText = requireQuiz
                ? (quizMet
                    ? "Passed"
                    : (passScore > 0 ? $"Score ≥ {passScore}%" : "Quiz required (score TBD)"))
                : "No quiz required";

            string quizProgressText = requireQuiz
                ? $"{progress.QuizScore}%"
                : "—";

            string quizStatusText = requireQuiz
                ? (quizMet ? "Met" : "Not met")
                : "Not required";

            string quizStatusCssInner = requireQuiz
                ? (quizMet ? "met" : "notmet")
                : "met";

            // Teaching sessions
            string teachReqText = minTeach > 0 ? $"{minTeach}+" : "No minimum";
            string teachProgressText = progress.TeachingSessions.ToString();
            bool teachMet = progress.TeachingSessions >= minTeach;
            string teachStatusText = teachMet ? "Met" : "Not met";
            string teachStatusCssInner = teachMet ? "met" : "notmet";

            // 1:1 help
            string helpReqText = minHelp > 0 ? $"{minHelp}+" : "No minimum";
            string helpProgressText = progress.HelpSessions.ToString();
            bool helpMet = progress.HelpSessions >= minHelp;
            string helpStatusText = helpMet ? "Met" : "Not met";
            string helpStatusCssInner = helpMet ? "met" : "notmet";

            // Expiry
            string expiryText = expiryDays > 0
                ? $"{expiryDays} days (re-certify after this)"
                : "No expiry set";

            string expiryMetaText = expiryDays > 0
                ? "Once certified, this module will eventually expire to keep skills fresh."
                : "This certification does not currently expire.";

            // Eligibility / Certification
            bool anyRequirementConfigured = requireQuiz || minTeach > 0 || minHelp > 0;

            bool isCertified = hasRule && anyRequirementConfigured && quizMet && teachMet && helpMet;

            // Eligible to teach when quiz requirement is met, but not yet certified
            bool isEligible = hasRule && anyRequirementConfigured && quizMet && !isCertified;

            string statusLabel;
            string statusCss;
            string headerStatusCss;

            if (isCertified)
            {
                statusLabel = "Certified";
                statusCss = "status-certified";
                headerStatusCss = "req-status-certified";
            }
            else if (isEligible)
            {
                statusLabel = "Eligible";
                statusCss = "status-eligible";
                headerStatusCss = "req-status-eligible";
            }
            else
            {
                statusLabel = "Not certified";
                statusCss = "status-notcert";
                headerStatusCss = "req-status-not";
            }

            string desc;
            if (hasRule && !string.IsNullOrWhiteSpace(rule.Description))
            {
                desc = rule.Description;
            }
            else if (!string.IsNullOrWhiteSpace(course.Summary))
            {
                desc = course.Summary;
            }
            else if (hasRule)
            {
                desc = "No detailed description set yet for this certification rule.";
            }
            else
            {
                desc = "No certification rule has been configured yet for this microcourse.";
            }

            string ruleMeta;
            if (!hasRule)
            {
                ruleMeta = "This microcourse is available, but a certification rule has not been set up yet.";
            }
            else
            {
                ruleMeta = $"Rule ID: {rule.Id} • Require quiz: {(requireQuiz ? "Yes" : "No")}";
            }

            // Difficulty metric for sorting
            int sortKey = (requireQuiz ? 1 : 0) * 100000
                          + passScore * 100
                          + minTeach * 10
                          + minHelp;

            return new ModuleStatus
            {
                CourseId = course.Id,
                Title = course.Title,
                Description = desc,
                HasRule = hasRule,

                StatusLabel = statusLabel,
                StatusCssClass = statusCss,

                HeaderStatusLabel = statusLabel,
                HeaderStatusCss = headerStatusCss,

                QuizRequirementText = quizReqText,
                QuizProgressText = quizProgressText,
                QuizStatusText = quizStatusText,
                QuizStatusCss = "req-item-status " + quizStatusCssInner,

                TeachingRequirementText = teachReqText,
                TeachingProgressText = teachProgressText,
                TeachingStatusText = teachStatusText,
                TeachingStatusCss = "req-item-status " + teachStatusCssInner,

                HelpRequirementText = helpReqText,
                HelpProgressText = helpProgressText,
                HelpStatusText = helpStatusText,
                HelpStatusCss = "req-item-status " + helpStatusCssInner,

                ExpiryText = expiryText,
                ExpiryMetaText = expiryMetaText,

                RuleMetaText = ruleMeta,
                IsCertified = isCertified,
                IsEligible = isEligible,
                SortKey = sortKey,

                ExternalLink = extLink,
                HasExternalLink = hasExternal,
                ResourceViewerUrl = viewerUrl
            };
        }

        /// <summary>
        /// Handles "I’ve reviewed the resources and passed the quiz" button clicks.
        /// Marks the Helper's quizScore for that course as at least the rule's passScore.
        /// Also recalculates eligibility/certification and totals.
        /// </summary>
        protected void RequirementsRepeater_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName != "confirmResources") return;

            var courseId = e.CommandArgument as string;
            var helperId = Session["UserId"] as string ?? string.Empty;

            if (string.IsNullOrWhiteSpace(courseId) || string.IsNullOrWhiteSpace(helperId))
            {
                BindCertificationView();
                return;
            }

            EnsureRulesXml();
            EnsureMicrocoursesXml();
            EnsureHelperProgressXml();

            var rulesByKey = LoadRulesByKey();
            var courses = LoadMicrocourses();
            var course = courses.FirstOrDefault(c =>
                string.Equals(c.Id, courseId, StringComparison.OrdinalIgnoreCase));

            RuleInfo rule = null;
            if (course != null && !string.IsNullOrWhiteSpace(course.RuleId))
            {
                rulesByKey.TryGetValue(course.RuleId.Trim(), out rule);
            }

            var passScore = rule?.PassScore ?? 0;
            if (passScore <= 0)
            {
                // Fallback in case a rule accidentally has 0; treat as 80%.
                passScore = 80;
            }

            SaveQuizConfirmed(helperId, courseId, passScore);

            // Refresh UI with updated progress.
            BindCertificationView();
        }

        private void SaveQuizConfirmed(string helperId, string courseId, int passScore)
        {
            EnsureHelperProgressXml();

            var doc = new XmlDocument();
            doc.Load(HelperProgressXmlPath);
            var root = doc.DocumentElement;

            var helperEl = (XmlElement)root.SelectSingleNode($"./helper[@id='{helperId}']");
            if (helperEl == null)
            {
                helperEl = doc.CreateElement("helper");
                helperEl.SetAttribute("id", helperId);
                root.AppendChild(helperEl);
            }

            var courseEl = (XmlElement)helperEl.SelectSingleNode($"./course[@id='{courseId}']");
            if (courseEl == null)
            {
                courseEl = doc.CreateElement("course");
                courseEl.SetAttribute("id", courseId);
                helperEl.AppendChild(courseEl);
            }

            XmlElement EnsureChild(string name, string valueIfMissing)
            {
                var n = courseEl[name] ?? doc.CreateElement(name);
                if (n.ParentNode == null) courseEl.AppendChild(n);
                if (string.IsNullOrEmpty(n.InnerText)) n.InnerText = valueIfMissing;
                return n;
            }

            // Mark quiz as passed at least at the minimum score.
            var quizScoreNode = courseEl["quizScore"] ?? doc.CreateElement("quizScore");
            quizScoreNode.InnerText = passScore.ToString();
            if (quizScoreNode.ParentNode == null) courseEl.AppendChild(quizScoreNode);

            // Optional: flag that resources were confirmed.
            var confirmedNode = courseEl["resourcesConfirmed"] ?? doc.CreateElement("resourcesConfirmed");
            confirmedNode.InnerText = "true";
            if (confirmedNode.ParentNode == null) courseEl.AppendChild(confirmedNode);

            var updatedNode = courseEl["lastUpdatedUtc"] ?? doc.CreateElement("lastUpdatedUtc");
            updatedNode.InnerText = DateTime.UtcNow.ToString("o");
            if (updatedNode.ParentNode == null) courseEl.AppendChild(updatedNode);

            // Ensure other fields exist (even if still 0).
            EnsureChild("teachingSessions", "0");
            EnsureChild("helpSessions", "0");
            EnsureChild("isEligible", "false");
            EnsureChild("isCertified", "false");

            // Recalculate eligibility/certifications for all this helper's courses
            RecalculateCertificationForHelper(doc, helperEl);

            // Keep helper-level totals in sync (teaching/help + certified count)
            UpdateHelperTotals(doc, helperEl);

            doc.Save(HelperProgressXmlPath);
        }

        /// <summary>
        /// Recalculate <isEligible> and <isCertified> for each course for this helper
        /// based on stored rule metadata and current progress.
        /// </summary>
        private void RecalculateCertificationForHelper(XmlDocument doc, XmlElement helperEl)
        {
            foreach (XmlElement c in helperEl.SelectNodes("./course"))
            {
                int passScore = ParseIntSafe(c["passScore"]?.InnerText, 0);
                int minTeach = ParseIntSafe(c["minSessionsTaught"]?.InnerText, 0);
                int minHelp = ParseIntSafe(c["minHelpSessions"]?.InnerText, 0);
                bool requireQuiz = (c["requireQuiz"]?.InnerText ?? "false")
                    .Equals("true", StringComparison.OrdinalIgnoreCase);

                int quizScore = ParseIntSafe(c["quizScore"]?.InnerText, 0);
                int teach = ParseIntSafe(c["teachingSessions"]?.InnerText, 0);
                int help = ParseIntSafe(c["helpSessions"]?.InnerText, 0);

                bool quizMet = requireQuiz
                    ? (passScore > 0 && quizScore >= passScore)
                    : true;

                bool teachMet = teach >= minTeach;
                bool helpMet = help >= minHelp;

                bool anyRequirementConfigured = requireQuiz || minTeach > 0 || minHelp > 0;

                bool certified = anyRequirementConfigured && quizMet && teachMet && helpMet;
                bool eligible = anyRequirementConfigured && quizMet && !certified;

                var isCertEl = c["isCertified"] ?? doc.CreateElement("isCertified");
                isCertEl.InnerText = certified ? "true" : "false";
                if (isCertEl.ParentNode == null) c.AppendChild(isCertEl);

                var isEligEl = c["isEligible"] ?? doc.CreateElement("isEligible");
                isEligEl.InnerText = eligible ? "true" : "false";
                if (isEligEl.ParentNode == null) c.AppendChild(isEligEl);
            }
        }

        /// <summary>
        /// Sums course-level progress into helper-level totals and stores them in /helper/totals.
        /// This keeps an easy snapshot of total sessions taught, total 1:1s, and total certified modules.
        /// </summary>
        private void UpdateHelperTotals(XmlDocument doc, XmlElement helperEl)
        {
            int totalTeach = 0;
            int totalHelp = 0;
            int totalCertified = 0;

            foreach (XmlElement c in helperEl.SelectNodes("./course"))
            {
                totalTeach += ParseIntSafe(c["teachingSessions"]?.InnerText, 0);
                totalHelp += ParseIntSafe(c["helpSessions"]?.InnerText, 0);

                var isCertText = c["isCertified"]?.InnerText ?? string.Empty;
                if (isCertText.Equals("true", StringComparison.OrdinalIgnoreCase))
                {
                    totalCertified++;
                }
            }

            var totalsEl = helperEl["totals"] ?? doc.CreateElement("totals");

            XmlElement EnsureTotalsChild(string name, string value)
            {
                var n = totalsEl[name] ?? doc.CreateElement(name);
                n.InnerText = value;
                if (n.ParentNode == null) totalsEl.AppendChild(n);
                return n;
            }

            EnsureTotalsChild("totalTeachingSessions", totalTeach.ToString());
            EnsureTotalsChild("totalHelpSessions", totalHelp.ToString());
            EnsureTotalsChild("totalCoursesCertified", totalCertified.ToString());
            EnsureTotalsChild("lastUpdatedUtc", DateTime.UtcNow.ToString("o"));

            if (totalsEl.ParentNode == null) helperEl.AppendChild(totalsEl);
        }
    }
}

