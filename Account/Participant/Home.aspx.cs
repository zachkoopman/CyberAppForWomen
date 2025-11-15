using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;

namespace CyberApp_FIA.Participant
{
    public partial class Home : Page
    {
        // ---------- XML file paths ----------
        private string EventsXmlPath => Server.MapPath("~/App_Data/events.xml");
        private string MicrocoursesXmlPath => Server.MapPath("~/App_Data/microcourses.xml");
        private string EventCoursesXmlPath => Server.MapPath("~/App_Data/eventCourses.xml");
        private string EventSessionsXmlPath => Server.MapPath("~/App_Data/eventSessions.xml");
        private string EnrollmentsXmlPath => Server.MapPath("~/App_Data/enrollments.xml");

        private string CompletionsXmlPath => Server.MapPath("~/App_Data/completions.xml");

        private static readonly object EnrollmentsLock = new object();
        private static readonly object CompletionsLock = new object();

        private sealed class FilterState
        {
            public DateTime? From { get; set; }
            public DateTime? To { get; set; }
            public HashSet<string> Tags { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            public string Query { get; set; } = "";
        }

        private sealed class Interval
        {
            public DateTime StartUtc { get; set; }
            public DateTime EndUtc { get; set; }
            public string SessionId { get; set; }
            public string Title { get; set; }
        }

        private static bool Overlaps(Interval a, Interval b) => a.StartUtc < b.EndUtc && b.StartUtc < a.EndUtc;

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            SessionsRepeater.ItemCommand += SessionsRepeater_ItemCommand;
            MySessionsRepeater.ItemCommand += SessionsRepeater_ItemCommand;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                var role = (string)Session["Role"];
                if (!string.Equals(role, "Participant", StringComparison.OrdinalIgnoreCase))
                {
                    Response.Redirect("~/Account/Login.aspx");
                    return;
                }

                try
                {
                    var userId = CurrentUserId();
                    if (string.IsNullOrEmpty(userId))
                    {
                        Response.Redirect("~/Account/Login.aspx");
                        return;
                    }

                    var qs = new CyberApp_FIA.Services.QuizService(Server);
                    var rules = qs.CurrentRulesetVersion();
                    if (!qs.IsCompleted(userId, rules))
                    {
                        Response.Redirect("~/Account/Participant/Quiz.aspx");
                        return;
                    }
                }
                catch { }

                var eventId = (string)Session["EventId"];
                if (string.IsNullOrWhiteSpace(eventId))
                {
                    Response.Redirect("~/Account/Participant/SelectEvent.aspx");
                    return;
                }

                LoadEventHeader(eventId);

                // NEW: load assigned Helper pill in the header (if any)
                LoadAssignedHelperChip(CurrentUserId());

                EnsureXmlDoc(EnrollmentsXmlPath, "enrollments");
                EnsureXmlDoc(CompletionsXmlPath, "completions");

                var allCourseTags = LoadCourseTags();
                var allTagValues = allCourseTags.Values.SelectMany(v => v).Distinct(StringComparer.OrdinalIgnoreCase);
                LoadTagOptionsIntoUi(allTagValues);

                var filters = ReadFiltersFromQuery();

                if (filters.From.HasValue) FilterFrom.Text = filters.From.Value.ToString("yyyy-MM-ddTHH:mm");
                if (filters.To.HasValue) FilterTo.Text = filters.To.Value.ToString("yyyy-MM-ddTHH:mm");
                foreach (ListItem li in FilterTags.Items) li.Selected = filters.Tags.Contains(li.Value);
                FilterQuery.Text = filters.Query;

                BindSessions(eventId, filters);
                BindMySessions(eventId, CurrentUserId());
            }
        }

        private string CurrentUserId() => Session["UserId"] as string;

        private void LoadEventHeader(string eventId)
        {
            var uni = (string)Session["University"] ?? "";
            University.Text = string.IsNullOrWhiteSpace(uni) ? "(not set)" : uni;

            if (!File.Exists(EventsXmlPath)) { EventName.Text = "(unknown)"; return; }
            var doc = new XmlDocument(); doc.Load(EventsXmlPath);
            var ev = (XmlElement)doc.SelectSingleNode($"/events/event[@id='{eventId}']");
            EventName.Text = ev?["name"]?.InnerText ?? "(unknown)";
        }

        /// <summary>
        /// Looks up the participant's assigned Helper in users.xml and, if found,
        /// shows a pill in the header with the Helper's first name.
        /// </summary>
        private void LoadAssignedHelperChip(string userId)
        {
            // Default to hidden / empty if anything fails.
            HelperPill.Visible = false;
            HelperName.Text = string.Empty;

            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                    return;

                var usersPath = Server.MapPath("~/App_Data/users.xml");
                if (!File.Exists(usersPath))
                    return;

                var doc = new XmlDocument();
                doc.Load(usersPath);

                // Find the current participant by id.
                var me = (XmlElement)doc.SelectSingleNode($"/users/user[@id='{userId}']");
                if (me == null)
                    return;

                var helperId = me.GetAttribute("assignedHelperId");
                if (string.IsNullOrWhiteSpace(helperId))
                    return;

                // Find the Helper row.
                var helper = (XmlElement)doc.SelectSingleNode($"/users/user[@id='{helperId}' and @role='Helper']");
                if (helper == null)
                    return;

                var firstName = helper["firstName"]?.InnerText ?? string.Empty;
                if (string.IsNullOrWhiteSpace(firstName))
                {
                    // Fallbacks if firstName is missing for some reason.
                    firstName = helper["name"]?.InnerText
                                ?? helper["email"]?.InnerText
                                ?? string.Empty;
                }

                if (string.IsNullOrWhiteSpace(firstName))
                    return;

                HelperName.Text = Server.HtmlEncode(firstName);
                HelperPill.Visible = true;
            }
            catch
            {
                // Soft-fail: header helper pill is non-critical.
                HelperPill.Visible = false;
                HelperName.Text = string.Empty;
            }
        }

        private Dictionary<string, HashSet<string>> LoadCourseTags()
        {
            var map = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
            if (!File.Exists(MicrocoursesXmlPath)) return map;

            var doc = new XmlDocument(); doc.Load(MicrocoursesXmlPath);
            foreach (XmlElement c in doc.SelectNodes("/microcourses/course[@status='Published']"))
            {
                var id = c.GetAttribute("id");
                if (string.IsNullOrWhiteSpace(id)) continue;

                var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (XmlElement t in c.SelectNodes("tags/tag"))
                {
                    var v = (t.InnerText ?? "").Trim();
                    if (!string.IsNullOrWhiteSpace(v)) set.Add(v);
                }
                map[id] = set;
            }
            return map;
        }

        private void LoadTagOptionsIntoUi(IEnumerable<string> allTags)
        {
            FilterTags.Items.Clear();
            foreach (var t in allTags.OrderBy(x => x))
                FilterTags.Items.Add(new ListItem(t, t));
        }

        private static DateTime? ParseLocal(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            if (DateTime.TryParse(s, out var dt)) return dt;
            return null;
        }

        private FilterState ReadFiltersFromQuery()
        {
            var f = new FilterState();
            f.From = ParseLocal(Request.QueryString["from"]);
            f.To = ParseLocal(Request.QueryString["to"]);
            var tags = (Request.QueryString["tags"] ?? "").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var t in tags) f.Tags.Add(t);
            f.Query = (Request.QueryString["q"] ?? "").Trim();
            return f;
        }

        private string BuildFilterQueryString(FilterState f)
        {
            var qs = HttpUtility.ParseQueryString(string.Empty);
            if (f.From.HasValue) qs["from"] = f.From.Value.ToString("yyyy-MM-ddTHH:mm");
            if (f.To.HasValue) qs["to"] = f.To.Value.ToString("yyyy-MM-ddTHH:mm");
            if (f.Tags.Count > 0) qs["tags"] = string.Join(",", f.Tags);
            if (!string.IsNullOrWhiteSpace(f.Query)) qs["q"] = f.Query;
            return qs.ToString();
        }

        private sealed class IntervalList : List<Interval> { }

        private List<Interval> GetUserEnrolledIntervals(string eventId, string userId)
        {
            var list = new List<Interval>();
            if (string.IsNullOrWhiteSpace(userId)) return list;
            if (!File.Exists(EnrollmentsXmlPath) || !File.Exists(EventSessionsXmlPath)) return list;

            var enr = new XmlDocument(); enr.Load(EnrollmentsXmlPath);
            var ses = new XmlDocument(); ses.Load(EventSessionsXmlPath);

            foreach (XmlElement s in enr.SelectNodes($"/enrollments/session[@eventId='{eventId}']"))
            {
                var sid = s.GetAttribute("id");
                if (s.SelectSingleNode($"enrolled/user[@id='{userId}']") == null) continue;

                var sn = (XmlElement)ses.SelectSingleNode($"/eventSessions/session[@eventId='{eventId}' and @id='{sid}']");
                if (sn == null) continue;

                var startIso = sn["start"]?.InnerText ?? "";
                var endIso = sn["end"]?.InnerText ?? "";
                if (!DateTime.TryParse(startIso, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var startUtc)) continue;
                if (!DateTime.TryParse(endIso, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var endUtc)) continue;

                var courseId = sn["courseId"]?.InnerText ?? "";
                var title = LoadCourseTitles().TryGetValue(courseId, out var t) ? t : "(untitled)";

                list.Add(new Interval { StartUtc = startUtc, EndUtc = endUtc, SessionId = sid, Title = title });
            }

            return list;
        }

        private bool SessionOverlapsUser(string eventId, string userId, string sessionId, out Interval conflictWith)
        {
            conflictWith = null;
            if (!File.Exists(EventSessionsXmlPath)) return false;

            var ses = new XmlDocument(); ses.Load(EventSessionsXmlPath);
            var s = (XmlElement)ses.SelectSingleNode($"/eventSessions/session[@eventId='{eventId}' and @id='{sessionId}']");
            if (s == null) return false;

            var startIso = s["start"]?.InnerText ?? "";
            var endIso = s["end"]?.InnerText ?? "";
            if (!DateTime.TryParse(startIso, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var startUtc)) return false;
            if (!DateTime.TryParse(endIso, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var endUtc)) return false;

            var target = new Interval { StartUtc = startUtc, EndUtc = endUtc, SessionId = sessionId };
            var mine = GetUserEnrolledIntervals(eventId, CurrentUserId());
            foreach (var m in mine)
            {
                if (Overlaps(target, m)) { conflictWith = m; return true; }
            }
            return false;
        }

        private (DateTime startUtc, DateTime endUtc)? GetSessionTimes(string eventId, string sessionId)
        {
            if (!File.Exists(EventSessionsXmlPath)) return null;
            var ses = new XmlDocument(); ses.Load(EventSessionsXmlPath);
            var s = (XmlElement)ses.SelectSingleNode($"/eventSessions/session[@eventId='{eventId}' and @id='{sessionId}']");
            if (s == null) return null;
            if (!DateTime.TryParse(s["start"]?.InnerText ?? "", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var startUtc)) return null;
            if (!DateTime.TryParse(s["end"]?.InnerText ?? "", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var endUtc)) return null;
            return (startUtc, endUtc);
        }

        // NEW: 1-based position of the current user in a session's waitlist (0 if not present)
        private int GetWaitlistPosition(string eventId, string sessionId, string userId)
        {
            if (string.IsNullOrWhiteSpace(userId)) return 0;
            var doc = LoadEnrollmentsDoc();
            var ses = (XmlElement)doc.SelectSingleNode($"/enrollments/session[@eventId='{eventId}' and @id='{sessionId}']");
            if (ses == null) return 0;

            var list = ses.SelectNodes("waitlist/user");
            if (list == null) return 0;

            int idx = 1;
            foreach (XmlElement u in list)
            {
                if (string.Equals(u.GetAttribute("id"), userId, StringComparison.OrdinalIgnoreCase))
                    return idx;
                idx++;
            }
            return 0;
        }

        private void BindSessions(string eventId, FilterState filters)
        {
            var visibleCourseIds = LoadEventVisibleCourseIds(eventId);
            var idToTitle = LoadCourseTitles();
            var titleToId = LoadTitleToIdMap();
            var prereqsByCourseId = LoadPrereqMapMultiple(idToTitle, titleToId);
            var completedSet = LoadUserCompletedSet(CurrentUserId());
            var courseTags = LoadCourseTags();

            var rows = LoadUpcomingSessionRows(
                eventId, visibleCourseIds, idToTitle,
                prereqsByCourseId, completedSet, CurrentUserId(),
                filters, courseTags);

            EmptySessionsPH.Visible = rows.Count == 0;
            SessionsRepeater.DataSource = rows;
            SessionsRepeater.DataBind();
        }

        private HashSet<string> LoadEventVisibleCourseIds(string eventId)
        {
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (!File.Exists(EventCoursesXmlPath)) return set;

            var doc = new XmlDocument(); doc.Load(EventCoursesXmlPath);
            foreach (XmlElement c in doc.SelectNodes($"/eventCourses/event[@id='{eventId}']/course[@enabled='true']"))
            {
                var id = c.GetAttribute("id");
                if (!string.IsNullOrEmpty(id)) set.Add(id);
            }
            return set;
        }

        private Dictionary<string, string> LoadCourseTitles()
        {
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (!File.Exists(MicrocoursesXmlPath)) return map;

            var doc = new XmlDocument(); doc.Load(MicrocoursesXmlPath);
            foreach (XmlElement c in doc.SelectNodes("/microcourses/course[@status='Published']"))
            {
                var id = c.GetAttribute("id");
                if (string.IsNullOrWhiteSpace(id)) continue;
                var title = c["title"]?.InnerText ?? "(untitled)";
                map[id] = title;
            }
            return map;
        }

        private Dictionary<string, string> LoadTitleToIdMap()
        {
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (!File.Exists(MicrocoursesXmlPath)) return map;

            var doc = new XmlDocument(); doc.Load(MicrocoursesXmlPath);
            foreach (XmlElement c in doc.SelectNodes("/microcourses/course[@status='Published']"))
            {
                var id = c.GetAttribute("id");
                var title = c["title"]?.InnerText;
                if (!string.IsNullOrWhiteSpace(id) && !string.IsNullOrWhiteSpace(title))
                {
                    map[title] = id;
                }
            }
            return map;
        }

        private string ResolveCourseIdentifier(string ident, Dictionary<string, string> idToTitle, Dictionary<string, string> titleToId)
        {
            if (string.IsNullOrWhiteSpace(ident)) return null;
            if (idToTitle.ContainsKey(ident)) return ident;
            if (titleToId.TryGetValue(ident, out var idFromTitle)) return idFromTitle;
            return null;
        }

        private Dictionary<string, List<string>> LoadPrereqMapMultiple(Dictionary<string, string> idToTitle, Dictionary<string, string> titleToId)
        {
            var map = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            if (!File.Exists(MicrocoursesXmlPath)) return map;

            var doc = new XmlDocument(); doc.Load(MicrocoursesXmlPath);
            foreach (XmlElement c in doc.SelectNodes("/microcourses/course"))
            {
                var courseId = c.GetAttribute("id");
                if (string.IsNullOrWhiteSpace(courseId)) continue;

                var list = new List<string>();
                foreach (XmlElement p in c.SelectNodes("prerequisites/course"))
                {
                    var ident = p.GetAttribute("id");
                    var resolved = ResolveCourseIdentifier(ident, idToTitle, titleToId);
                    if (!string.IsNullOrWhiteSpace(resolved)) list.Add(resolved);
                }
                var singleChild = (XmlElement)c.SelectSingleNode("prerequisite");
                if (singleChild != null)
                {
                    var ident = singleChild.GetAttribute("id");
                    var resolved = ResolveCourseIdentifier(ident, idToTitle, titleToId);
                    if (!string.IsNullOrWhiteSpace(resolved) && !list.Contains(resolved, StringComparer.OrdinalIgnoreCase))
                        list.Add(resolved);
                }
                var attrIdent = c.GetAttribute("prerequisiteId");
                if (!string.IsNullOrWhiteSpace(attrIdent))
                {
                    var resolved = ResolveCourseIdentifier(attrIdent, idToTitle, titleToId);
                    if (!string.IsNullOrWhiteSpace(resolved) && !list.Contains(resolved, StringComparer.OrdinalIgnoreCase))
                        list.Add(resolved);
                }
                if (list.Count > 0) map[courseId] = list;
            }
            return map;
        }

        private void EnsureXmlDoc(string path, string rootName)
        {
            if (File.Exists(path)) return;
            var doc = new XmlDocument();
            doc.AppendChild(doc.CreateElement(rootName));
            doc.Save(path);
        }

        private HashSet<string> LoadUserCompletedSet(string userId)
        {
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(userId)) return set;
            if (!File.Exists(CompletionsXmlPath)) return set;

            var doc = new XmlDocument(); doc.Load(CompletionsXmlPath);
            foreach (XmlElement c in doc.SelectNodes($"/completions/user[@id='{userId}']/course"))
            {
                var id = c.GetAttribute("id");
                if (!string.IsNullOrWhiteSpace(id)) set.Add(id);
            }
            return set;
        }

        private void MarkCourseCompleted(string userId, string courseId)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(courseId)) return;

            lock (CompletionsLock)
            {
                EnsureXmlDoc(CompletionsXmlPath, "completions");
                var doc = new XmlDocument(); doc.Load(CompletionsXmlPath);

                var user = (XmlElement)doc.SelectSingleNode($"/completions/user[@id='{userId}']");
                if (user == null)
                {
                    user = doc.CreateElement("user");
                    user.SetAttribute("id", userId);
                    doc.DocumentElement.AppendChild(user);
                }

                var existing = (XmlElement)user.SelectSingleNode($"course[@id='{courseId}']");
                if (existing == null)
                {
                    var c = doc.CreateElement("course");
                    c.SetAttribute("id", courseId);
                    c.SetAttribute("completedOn", DateTime.UtcNow.ToString("yyyy-MM-dd"));
                    user.AppendChild(c);
                }
                else
                {
                    if (!existing.HasAttribute("completedOn"))
                        existing.SetAttribute("completedOn", DateTime.UtcNow.ToString("yyyy-MM-dd"));
                }

                doc.Save(CompletionsXmlPath);
            }
        }

        private List<object> LoadUpcomingSessionRows(
            string eventId,
            HashSet<string> visibleCourseIds,
            Dictionary<string, string> idToTitle,
            Dictionary<string, List<string>> prereqsByCourseId,
            HashSet<string> completedSet,
            string userId,
            FilterState filters,
            Dictionary<string, HashSet<string>> courseTags)
        {
            var rows = new List<object>();
            if (!File.Exists(EventSessionsXmlPath)) return rows;

            var doc = new XmlDocument(); doc.Load(EventSessionsXmlPath);
            var myIntervals = GetUserEnrolledIntervals(eventId, userId);

            foreach (XmlElement s in doc.SelectNodes($"/eventSessions/session[@eventId='{eventId}']"))
            {
                var courseId = s["courseId"]?.InnerText ?? "";
                if (string.IsNullOrEmpty(courseId)) continue;
                if (visibleCourseIds.Count > 0 && !visibleCourseIds.Contains(courseId)) continue;

                var startIso = s["start"]?.InnerText ?? "";
                var endIso = s["end"]?.InnerText ?? "";
                if (!DateTime.TryParse(endIso, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var endUtc))
                {
                    if (!DateTime.TryParse(endIso, out endUtc)) continue;
                }
                if (endUtc < DateTime.UtcNow) continue;

                DateTime startUtc;
                if (!DateTime.TryParse(startIso, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out startUtc))
                {
                    DateTime.TryParse(startIso, out startUtc);
                }
                var startLocal = DateTime.SpecifyKind(startUtc, DateTimeKind.Utc).ToLocalTime();

                if (filters.From.HasValue && startLocal < filters.From.Value) continue;
                if (filters.To.HasValue && startLocal > filters.To.Value) continue;

                var sessionId = s.GetAttribute("id");
                if (string.IsNullOrWhiteSpace(sessionId))
                    sessionId = $"{courseId}|{startUtc:o}";

                var room = s["room"]?.InnerText ?? "";
                var helperName = s["helper"]?.InnerText ?? "";
                var capStr = s["capacity"]?.InnerText ?? "0";
                var capacity = int.TryParse(capStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out var capVal) ? capVal : 0;

                idToTitle.TryGetValue(courseId, out var title);
                var microcourseTitle = string.IsNullOrWhiteSpace(title) ? "(untitled)" : title;

                if (!string.IsNullOrWhiteSpace(filters.Query))
                {
                    var q = filters.Query.Trim();
                    var hay = $"{microcourseTitle}|{helperName}|{room}";
                    if (hay.IndexOf(q, StringComparison.OrdinalIgnoreCase) < 0) continue;
                }

                if (filters.Tags.Count > 0)
                {
                    courseTags.TryGetValue(courseId, out var tagsForCourse);
                    if (tagsForCourse == null || !tagsForCourse.Overlaps(filters.Tags))
                        continue;
                }

                var enrolledCount = GetEnrolledCount(eventId, sessionId);
                var remaining = Math.Max(0, capacity - enrolledCount);
                var isFull = remaining == 0;
                var isEnrolled = IsUserEnrolled(eventId, sessionId, userId);
                var isWaitlisted = IsUserWaitlisted(eventId, sessionId, userId);

                var waitlistPosition = isWaitlisted ? GetWaitlistPosition(eventId, sessionId, userId) : 0; // NEW

                var prereqsByCourse = prereqsByCourseId;
                prereqsByCourse.TryGetValue(courseId, out var prereqList);
                prereqList = prereqList ?? new List<string>();

                var completedSetLocal = completedSet;
                var unmet = prereqList.Where(p => !completedSetLocal.Contains(p)).ToList();

                var prereqMet = unmet.Count == 0;
                var missingPrereqId = prereqMet ? null : unmet[0];
                string missingPrereqTitle = null;
                if (!string.IsNullOrWhiteSpace(missingPrereqId))
                {
                    idToTitle.TryGetValue(missingPrereqId, out missingPrereqTitle);
                    if (string.IsNullOrWhiteSpace(missingPrereqTitle)) missingPrereqTitle = missingPrereqId;
                }

                var hasConflict = false;
                string conflictMessage = "";
                string replacementUrl = "";
                if (!isEnrolled && !isWaitlisted)
                {
                    var target = new Interval { StartUtc = startUtc, EndUtc = endUtc, SessionId = sessionId, Title = microcourseTitle };
                    foreach (var m in myIntervals)
                    {
                        if (Overlaps(target, m))
                        {
                            hasConflict = true;
                            var mLocalStart = DateTime.SpecifyKind(m.StartUtc, DateTimeKind.Utc).ToLocalTime();
                            var mLocalEnd = DateTime.SpecifyKind(m.EndUtc, DateTimeKind.Utc).ToLocalTime();
                            conflictMessage = $"It overlaps with your existing session “{m.Title}” ({mLocalStart:ddd, MMM d • h:mm tt}–{mLocalEnd:h:mm tt}).";
                            var returnQs = Request.Url.Query ?? "";
                            replacementUrl = ResolveUrl($"~/Account/Participant/Replacements.aspx?eventId={HttpUtility.UrlEncode(eventId)}&sessionId={HttpUtility.UrlEncode(sessionId)}&return={HttpUtility.UrlEncode(returnQs)}");
                            break;
                        }
                    }
                }

                rows.Add(new
                {
                    microcourseTitle,
                    helperName,
                    remainingSeats = remaining,
                    capacity,
                    room,
                    startLocal,
                    sessionId,
                    isFull,
                    isEnrolled,
                    isWaitlisted,
                    waitlistPosition,  // NEW
                    courseId,
                    prereqMet,
                    missingPrereqId,
                    missingPrereqTitle,
                    hasConflict,
                    conflictMessage,
                    replacementUrl
                });
            }

            return rows.OrderBy(r => (DateTime)r.GetType().GetProperty("startLocal").GetValue(r, null)).ToList();
        }

        private void BindMySessions(string eventId, string userId)
        {
            var rows = new List<object>();
            if (string.IsNullOrWhiteSpace(userId)) { MySessionsWrap.Visible = false; return; }
            if (!File.Exists(EventSessionsXmlPath)) { MySessionsWrap.Visible = false; return; }

            var docSess = new XmlDocument(); docSess.Load(EventSessionsXmlPath);
            var docEnr = LoadEnrollmentsDoc();

            foreach (XmlElement sesNode in docEnr.SelectNodes($"/enrollments/session[@eventId='{eventId}']"))
            {
                var sid = sesNode.GetAttribute("id");

                // include waitlisted sessions, not only enrolled
                bool isEnrolled = sesNode.SelectSingleNode($"enrolled/user[@id='{userId}']") != null;
                bool isWaitlisted = !isEnrolled && sesNode.SelectSingleNode($"waitlist/user[@id='{userId}']") != null;
                if (!isEnrolled && !isWaitlisted) continue;

                var s = (XmlElement)docSess.SelectSingleNode($"/eventSessions/session[@eventId='{eventId}' and @id='{sid}']");
                if (s == null) continue;

                var courseId = s["courseId"]?.InnerText ?? "";
                var startIso = s["start"]?.InnerText ?? "";
                DateTime.TryParse(startIso, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var startUtc);
                var startLocal = DateTime.SpecifyKind(startUtc, DateTimeKind.Utc).ToLocalTime();
                var room = s["room"]?.InnerText ?? "";
                var helperName = s["helper"]?.InnerText ?? "";

                var titles = LoadCourseTitles();
                titles.TryGetValue(courseId, out var title);
                var microcourseTitle = string.IsNullOrWhiteSpace(title) ? "(untitled)" : title;

                // compute waitlist position for "My Sessions" when applicable
                var waitlistPosition = isWaitlisted ? GetWaitlistPosition(eventId, sid, userId) : 0;

                // NEW: Only participants the Helper has "admitted" see the room link.
                bool isAdmitted = false;
                if (isEnrolled)
                {
                    var thisUserNode = sesNode.SelectSingleNode($"enrolled/user[@id='{userId}']") as XmlElement;
                    if (thisUserNode != null &&
                        string.Equals(thisUserNode.GetAttribute("admitted"), "true", StringComparison.OrdinalIgnoreCase))
                    {
                        isAdmitted = true;
                    }
                }

                bool canSeeRoom = !string.IsNullOrWhiteSpace(room) && isEnrolled && isAdmitted;

                rows.Add(new
                {
                    microcourseTitle,
                    helperName,
                    room,
                    startLocal,
                    sessionId = sid,
                    isEnrolled,
                    isWaitlisted,
                    waitlistPosition,
                    canSeeRoom
                });
            }

            MySessionsWrap.Visible = rows.Count > 0;
            MySessionsRepeater.DataSource = rows
                .OrderBy(r => (DateTime)r.GetType().GetProperty("startLocal").GetValue(r, null))
                .ToList();
            MySessionsRepeater.DataBind();
        }

        protected void SessionsRepeater_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            var userId = CurrentUserId();
            var eventId = (string)Session["EventId"] ?? "";
            var sessionId = Convert.ToString(e.CommandArgument);

            if (string.IsNullOrWhiteSpace(userId)) { ShowFormInfo(false, "Please sign in again."); return; }
            if (string.IsNullOrWhiteSpace(eventId) || string.IsNullOrWhiteSpace(sessionId)) { ShowFormInfo(false, "Missing event or session."); return; }

            try
            {
                string msg; bool ok = false;

                if (string.Equals(e.CommandName, "enroll", StringComparison.OrdinalIgnoreCase))
                {
                    if (SessionOverlapsUser(eventId, userId, sessionId, out var conflict))
                    {
                        var sLocal = DateTime.SpecifyKind(conflict.StartUtc, DateTimeKind.Utc).ToLocalTime();
                        var eLocal = DateTime.SpecifyKind(conflict.EndUtc, DateTimeKind.Utc).ToLocalTime();
                        var returnQs = Request.Url.Query ?? "";
                        var replUrl = ResolveUrl($"~/Account/Participant/Replacements.aspx?eventId={Uri.EscapeDataString(eventId)}&sessionId={Uri.EscapeDataString(sessionId)}&return={HttpUtility.UrlEncode(returnQs)}");
                        ShowFormInfo(false, $"Time conflict with “{conflict.Title}” ({sLocal:ddd, MMM d • h:mm tt}–{eLocal:h:mm tt}). You can look for alternate times here: {replUrl}");
                        return;
                    }

                    ok = TryEnroll(eventId, sessionId, userId, out msg);
                    if (ok)
                    {
                        var returnQs = Request.Url.Query;
                        var url = ResolveUrl($"~/Account/Participant/EnrollSuccess.aspx?eventId={Uri.EscapeDataString(eventId)}&sessionId={Uri.EscapeDataString(sessionId)}&return={HttpUtility.UrlEncode(returnQs)}");
                        Response.Redirect(url, endResponse: false);
                        HttpContext.Current.ApplicationInstance.CompleteRequest();
                        return;
                    }
                    ShowFormInfo(false, msg);
                    return;
                }
                else if (string.Equals(e.CommandName, "waitlist", StringComparison.OrdinalIgnoreCase))
                {
                    ok = TryWaitlist(eventId, sessionId, userId, out msg);
                }
                else if (string.Equals(e.CommandName, "complete", StringComparison.OrdinalIgnoreCase))
                {
                    ok = TryMarkComplete(eventId, sessionId, userId, out msg);
                }
                else if (string.Equals(e.CommandName, "unenroll", StringComparison.OrdinalIgnoreCase)) // NEW
                {
                    ok = TryUnenroll(eventId, sessionId, userId, out msg); // NEW
                }
                else
                {
                    msg = "Unknown action.";
                }

                ShowFormInfo(ok, msg);
            }
            catch (Exception ex)
            {
                ShowFormInfo(false, "Action failed: " + Server.HtmlEncode(ex.Message));
            }
            finally
            {
                if (!string.IsNullOrWhiteSpace(eventId))
                {
                    var filters = ReadFiltersFromQuery();
                    BindSessions(eventId, filters);
                    BindMySessions(eventId, userId);
                }
            }
        }

        protected void ApplyFilters_Click(object sender, EventArgs e)
        {
            var f = new FilterState
            {
                From = ParseLocal(FilterFrom.Text),
                To = ParseLocal(FilterTo.Text),
                Query = (FilterQuery.Text ?? "").Trim()
            };
            foreach (ListItem li in FilterTags.Items) if (li.Selected) f.Tags.Add(li.Value);

            var qs = BuildFilterQueryString(f);
            var url = ResolveUrl("~/Account/Participant/Home.aspx");
            Response.Redirect(string.IsNullOrEmpty(qs) ? url : $"{url}?{qs}", endResponse: true);
        }

        protected void ClearFilters_Click(object sender, EventArgs e)
        {
            var url = ResolveUrl("~/Account/Participant/Home.aspx");
            Response.Redirect(url, endResponse: true);
        }

        private void ShowFormInfo(bool ok, string msg)
        {
            FormMessage.Text = ok
                ? $"<span style='color:#0a6b4f'>{Server.HtmlEncode(msg)}</span>"
                : $"<span style='color:#b00020'>{Server.HtmlEncode(msg)}</span>";
        }

        private XmlDocument LoadEnrollmentsDoc()
        {
            EnsureXmlDoc(EnrollmentsXmlPath, "enrollments");
            var doc = new XmlDocument();
            doc.Load(EnrollmentsXmlPath);
            return doc;
        }

        private void SaveEnrollmentsDoc(XmlDocument doc)
        {
            lock (EnrollmentsLock) { doc.Save(EnrollmentsXmlPath); }
        }

        private XmlElement EnsureSessionNode(XmlDocument doc, string eventId, string sessionId)
        {
            var ses = (XmlElement)doc.SelectSingleNode($"/enrollments/session[@eventId='{eventId}' and @id='{sessionId}']");
            if (ses == null)
            {
                ses = doc.CreateElement("session");
                ses.SetAttribute("eventId", eventId);
                ses.SetAttribute("id", sessionId);
                var enrolled = doc.CreateElement("enrolled");
                var waitlist = doc.CreateElement("waitlist");
                ses.AppendChild(enrolled);
                ses.AppendChild(waitlist);
                doc.DocumentElement.AppendChild(ses);
            }
            return ses;
        }

        protected void BtnLogout_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Response.Redirect("~/Welcome_Page.aspx");
        }

        private int GetEnrolledCount(string eventId, string sessionId)
        {
            var doc = LoadEnrollmentsDoc();
            var node = doc.SelectNodes($"/enrollments/session[@eventId='{eventId}' and @id='{sessionId}']/enrolled/user");
            return node?.Count ?? 0;
        }

        private bool IsUserEnrolled(string eventId, string sessionId, string userId)
        {
            var doc = LoadEnrollmentsDoc();
            var node = doc.SelectSingleNode($"/enrollments/session[@eventId='{eventId}' and @id='{sessionId}']/enrolled/user[@id='{userId}']");
            return node != null;
        }

        private bool IsUserWaitlisted(string eventId, string sessionId, string userId)
        {
            var doc = LoadEnrollmentsDoc();
            var node = doc.SelectSingleNode($"/enrollments/session[@eventId='{eventId}' and @id='{sessionId}']/waitlist/user[@id='{userId}']");
            return node != null;
        }

        private int GetCapacity(string eventId, string sessionId)
        {
            if (!File.Exists(EventSessionsXmlPath)) return 0;
            var doc = new XmlDocument(); doc.Load(EventSessionsXmlPath);
            var s = (XmlElement)doc.SelectSingleNode($"/eventSessions/session[@eventId='{eventId}' and @id='{sessionId}']");
            if (s == null) return 0;
            return int.TryParse(s["capacity"]?.InnerText, out var v) ? v : 0;
        }

        private string GetCourseIdForSession(string eventId, string sessionId)
        {
            if (!File.Exists(EventSessionsXmlPath)) return null;
            var doc = new XmlDocument(); doc.Load(EventSessionsXmlPath);
            var s = (XmlElement)doc.SelectSingleNode($"/eventSessions/session[@eventId='{eventId}' and @id='{sessionId}']");
            return s?["courseId"]?.InnerText ?? null;
        }

        private bool TryEnroll(string eventId, string sessionId, string userId, out string message)
        {
            lock (EnrollmentsLock)
            {
                var cap = GetCapacity(eventId, sessionId);
                var doc = LoadEnrollmentsDoc();
                var ses = EnsureSessionNode(doc, eventId, sessionId);

                if (ses.SelectSingleNode($"enrolled/user[@id='{userId}']") != null) { message = "You’re already enrolled in this session."; return true; }
                if (ses.SelectSingleNode($"waitlist/user[@id='{userId}']") != null) { message = "You’re already on the waitlist for this session."; return false; }

                if (SessionOverlapsUser(eventId, userId, sessionId, out var conflict))
                {
                    var sLocal = DateTime.SpecifyKind(conflict.StartUtc, DateTimeKind.Utc).ToLocalTime();
                    var eLocal = DateTime.SpecifyKind(conflict.EndUtc, DateTimeKind.Utc).ToLocalTime();
                    message = $"Time conflict with “{conflict.Title}” ({sLocal:ddd, MMM d • h:mm tt}–{eLocal:h:mm tt}).";
                    return false;
                }

                var currentEnrolled = ses.SelectNodes("enrolled/user")?.Count ?? 0;
                var remaining = Math.Max(0, cap - currentEnrolled);
                if (remaining <= 0) { message = "This session is full. You can join the waitlist instead."; return false; }

                var u = doc.CreateElement("user");
                u.SetAttribute("id", userId);
                u.SetAttribute("ts", DateTime.UtcNow.ToString("o"));
                ses.SelectSingleNode("enrolled").AppendChild(u);

                SaveEnrollmentsDoc(doc);
                message = "Enrolled successfully!";
                return true;
            }
        }

        private bool TryWaitlist(string eventId, string sessionId, string userId, out string message)
        {
            lock (EnrollmentsLock)
            {
                var cap = GetCapacity(eventId, sessionId);
                var doc = LoadEnrollmentsDoc();
                var ses = EnsureSessionNode(doc, eventId, sessionId);

                var currentEnrolled = ses.SelectNodes("enrolled/user")?.Count ?? 0;
                var remaining = Math.Max(0, cap - currentEnrolled);
                if (remaining > 0) { message = "Seats are still available; please enroll instead of joining the waitlist."; return false; }

                if (ses.SelectSingleNode($"enrolled/user[@id='{userId}']") != null) { message = "You’re already enrolled in this session."; return false; }
                if (ses.SelectSingleNode($"waitlist/user[@id='{userId}']") != null) { message = "You’re already on the waitlist for this session."; return false; }

                if (SessionOverlapsUser(eventId, userId, sessionId, out var conflict))
                {
                    var sLocal = DateTime.SpecifyKind(conflict.StartUtc, DateTimeKind.Utc).ToLocalTime();
                    var eLocal = DateTime.SpecifyKind(conflict.EndUtc, DateTimeKind.Utc).ToLocalTime();
                    message = $"Time conflict with “{conflict.Title}” ({sLocal:ddd, MMM d • h:mm tt}).";
                    return false;
                }

                // compute the position before appending
                var existingWaiters = ses.SelectNodes("waitlist/user")?.Count ?? 0;
                int newPosition = existingWaiters + 1;

                var u = doc.CreateElement("user");
                u.SetAttribute("id", userId);
                u.SetAttribute("ts", DateTime.UtcNow.ToString("o"));
                ses.SelectSingleNode("waitlist").AppendChild(u);

                SaveEnrollmentsDoc(doc);
                message = $"Added to waitlist. Your position is {newPosition}.";
                return true;
            }
        }

        private bool TryMarkComplete(string eventId, string sessionId, string userId, out string message)
        {
            lock (EnrollmentsLock)
            {
                var doc = LoadEnrollmentsDoc();
                var ses = EnsureSessionNode(doc, eventId, sessionId);

                var enrolledNode = (XmlElement)ses.SelectSingleNode($"enrolled/user[@id='{userId}']");
                if (enrolledNode == null) { message = "You are not enrolled in this session."; return false; }

                var courseId = GetCourseIdForSession(eventId, sessionId);

                enrolledNode.ParentNode.RemoveChild(enrolledNode);

                var firstWait = (XmlElement)ses.SelectSingleNode("waitlist/user[1]");
                if (firstWait != null)
                {
                    var moved = doc.CreateElement("user");
                    moved.SetAttribute("id", firstWait.GetAttribute("id"));
                    moved.SetAttribute("ts", DateTime.UtcNow.ToString("o"));
                    ses.SelectSingleNode("enrolled").AppendChild(moved);
                    firstWait.ParentNode.RemoveChild(firstWait);
                }

                SaveEnrollmentsDoc(doc);

                if (!string.IsNullOrWhiteSpace(courseId)) { MarkCourseCompleted(userId, courseId); }

                message = "Marked complete. Your seat was freed (and the next waitlisted participant was enrolled). Prerequisite checks updated.";
                return true;
            }
        }

        // NEW: Unenroll handler for both enrolled and waitlisted users
        private bool TryUnenroll(string eventId, string sessionId, string userId, out string message)
        {
            lock (EnrollmentsLock)
            {
                var doc = LoadEnrollmentsDoc();
                var ses = EnsureSessionNode(doc, eventId, sessionId);

                var enrolledNode = (XmlElement)ses.SelectSingleNode($"enrolled/user[@id='{userId}']");
                var waitNode = (XmlElement)ses.SelectSingleNode($"waitlist/user[@id='{userId}']");

                if (enrolledNode == null && waitNode == null)
                {
                    message = "You are not on this session.";
                    return false;
                }

                if (enrolledNode != null)
                {
                    // Remove from enrolled and promote next from waitlist (FIFO)
                    enrolledNode.ParentNode.RemoveChild(enrolledNode);

                    var firstWait = (XmlElement)ses.SelectSingleNode("waitlist/user[1]");
                    if (firstWait != null)
                    {
                        var moved = doc.CreateElement("user");
                        moved.SetAttribute("id", firstWait.GetAttribute("id"));
                        moved.SetAttribute("ts", DateTime.UtcNow.ToString("o"));
                        ses.SelectSingleNode("enrolled").AppendChild(moved);
                        firstWait.ParentNode.RemoveChild(firstWait);
                        SaveEnrollmentsDoc(doc);
                        message = "You have been unenrolled. The next person on the waitlist was enrolled.";
                        return true;
                    }

                    SaveEnrollmentsDoc(doc);
                    message = "You have been unenrolled.";
                    return true;
                }
                else
                {
                    // Was waitlisted: simply remove from waitlist
                    waitNode.ParentNode.RemoveChild(waitNode);
                    SaveEnrollmentsDoc(doc);
                    message = "Removed from the waitlist.";
                    return true;
                }
            }
        }
    }
}




