// OOPS! The block above ended early. Here's the CORRECT full Home.aspx.cs file:

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
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

        // NEW: enrollments store (enrolled & waitlist)
        private string EnrollmentsXmlPath => Server.MapPath("~/App_Data/enrollments.xml");
        private static readonly object EnrollmentsLock = new object();

        // ---------- Page lifecycle ----------
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            SessionsRepeater.ItemCommand += SessionsRepeater_ItemCommand;
            MySessionsRepeater.ItemCommand += SessionsRepeater_ItemCommand; // reuse handler for "complete"
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Gate: participant only
                var role = (string)Session["Role"];
                if (!string.Equals(role, "Participant", StringComparison.OrdinalIgnoreCase))
                {
                    Response.Redirect("~/Account/Login.aspx");
                    return;
                }

                // === QUIZ GATE: redirect first-time users until quiz complete ===
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
                catch
                {
                    // no-op; never block home if quiz files missing
                }
                // === end QUIZ GATE ===

                var eventId = (string)Session["EventId"];
                if (string.IsNullOrWhiteSpace(eventId))
                {
                    Response.Redirect("~/Account/Participant/SelectEvent.aspx");
                    return;
                }

                LoadEventHeader(eventId);

                // Ensure enrollments store exists
                EnsureEnrollmentsDoc();

                // Initial binds
                BindSessions(eventId);
                BindMySessions(eventId, CurrentUserId());
            }
        }

        // ---------- Identity ----------
        private string CurrentUserId()
        {
            return Session["UserId"] as string;
        }

        // ---------- Header ----------
        private void LoadEventHeader(string eventId)
        {
            var uni = (string)Session["University"] ?? "";
            University.Text = string.IsNullOrWhiteSpace(uni) ? "(not set)" : uni;

            if (!File.Exists(EventsXmlPath)) { EventName.Text = "(unknown)"; return; }
            var doc = new XmlDocument(); doc.Load(EventsXmlPath);
            var ev = (XmlElement)doc.SelectSingleNode($"/events/event[@id='{eventId}']");
            EventName.Text = ev?["name"]?.InnerText ?? "(unknown)";
        }

        // ---------- Bind sessions to the card grid ----------
        private void BindSessions(string eventId)
        {
            var visibleCourseIds = LoadEventVisibleCourseIds(eventId);
            var courseTitleById = LoadCourseTitles();

            var rows = LoadUpcomingSessionRows(eventId, visibleCourseIds, courseTitleById, CurrentUserId());

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

        // Build repeater items with enrollment state
        private List<object> LoadUpcomingSessionRows(
            string eventId,
            HashSet<string> visibleCourseIds,
            Dictionary<string, string> courseTitleById,
            string userId)
        {
            var rows = new List<object>();
            if (!File.Exists(EventSessionsXmlPath)) return rows;

            var nowUtc = DateTime.UtcNow;
            var doc = new XmlDocument(); doc.Load(EventSessionsXmlPath);

            foreach (XmlElement s in doc.SelectNodes($"/eventSessions/session[@eventId='{eventId}']"))
            {
                var courseId = s["courseId"]?.InnerText ?? "";
                if (string.IsNullOrEmpty(courseId)) continue;
                if (visibleCourseIds.Count > 0 && !visibleCourseIds.Contains(courseId)) continue;

                // times
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

                // ids/attrs
                var sessionId = s.GetAttribute("id");
                if (string.IsNullOrWhiteSpace(sessionId))
                    sessionId = $"{courseId}|{startUtc.ToString("o")}";

                var room = s["room"]?.InnerText ?? "";
                var helperName = s["helper"]?.InnerText ?? "";
                var capStr = s["capacity"]?.InnerText ?? "0";
                var capacity = SafeParseInt(capStr, 0);

                // enrollment state
                var enrolledCount = GetEnrolledCount(eventId, sessionId);
                var remaining = Math.Max(0, capacity - enrolledCount);
                var isFull = remaining == 0;
                var isEnrolled = IsUserEnrolled(eventId, sessionId, userId);
                var isWaitlisted = IsUserWaitlisted(eventId, sessionId, userId);

                courseTitleById.TryGetValue(courseId, out var title);
                var microcourseTitle = string.IsNullOrWhiteSpace(title) ? "(untitled)" : title;

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
                    isWaitlisted
                });
            }

            return rows
                .OrderBy(r => (DateTime)r.GetType().GetProperty("startLocal").GetValue(r, null))
                .ToList();
        }

        // ---------- Bind "My Sessions" ----------
        private void BindMySessions(string eventId, string userId)
        {
            var rows = new List<object>();
            if (string.IsNullOrWhiteSpace(userId)) { MySessionsWrap.Visible = false; return; }
            if (!File.Exists(EventSessionsXmlPath)) { MySessionsWrap.Visible = false; return; }

            var docSess = new XmlDocument(); docSess.Load(EventSessionsXmlPath);
            var docEnr = LoadEnrollmentsDoc();

            // find sessions where user is enrolled
            foreach (XmlElement sesNode in docEnr.SelectNodes($"/enrollments/session[@eventId='{eventId}']"))
            {
                var sid = sesNode.GetAttribute("id");
                var userNode = (XmlElement)sesNode.SelectSingleNode($"enrolled/user[@id='{userId}']");
                if (userNode == null) continue;

                // fetch details from eventSessions.xml
                var s = (XmlElement)docSess.SelectSingleNode($"/eventSessions/session[@eventId='{eventId}' and @id='{sid}']");
                if (s == null) continue;

                var courseId = s["courseId"]?.InnerText ?? "";
                var startIso = s["start"]?.InnerText ?? "";
                DateTime.TryParse(startIso, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var startUtc);
                var startLocal = DateTime.SpecifyKind(startUtc, DateTimeKind.Utc).ToLocalTime();
                var room = s["room"]?.InnerText ?? "";
                var helperName = s["helper"]?.InnerText ?? "";

                // title
                var titles = LoadCourseTitles();
                titles.TryGetValue(courseId, out var title);
                var microcourseTitle = string.IsNullOrWhiteSpace(title) ? "(untitled)" : title;

                rows.Add(new
                {
                    microcourseTitle,
                    helperName,
                    room,
                    startLocal,
                    sessionId = sid
                });
            }

            MySessionsWrap.Visible = rows.Count > 0;
            MySessionsRepeater.DataSource = rows
                .OrderBy(r => (DateTime)r.GetType().GetProperty("startLocal").GetValue(r, null))
                .ToList();
            MySessionsRepeater.DataBind();
        }

        // ---------- Commands (Enroll / Waitlist / Complete) ----------
        private void SessionsRepeater_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            var userId = CurrentUserId();
            var eventId = (string)Session["EventId"] ?? "";
            var sessionId = Convert.ToString(e.CommandArgument);

            if (string.IsNullOrWhiteSpace(userId))
            {
                ShowFormInfo(false, "Please sign in again.");
                return;
            }
            if (string.IsNullOrWhiteSpace(eventId) || string.IsNullOrWhiteSpace(sessionId))
            {
                ShowFormInfo(false, "Missing event or session.");
                return;
            }

            try
            {
                string msg;
                bool ok = false;

                if (string.Equals(e.CommandName, "enroll", StringComparison.OrdinalIgnoreCase))
                {
                    ok = TryEnroll(eventId, sessionId, userId, out msg);
                }
                else if (string.Equals(e.CommandName, "waitlist", StringComparison.OrdinalIgnoreCase))
                {
                    ok = TryWaitlist(eventId, sessionId, userId, out msg);
                }
                else if (string.Equals(e.CommandName, "complete", StringComparison.OrdinalIgnoreCase))
                {
                    ok = TryMarkComplete(eventId, sessionId, userId, out msg);
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
                // refresh both lists
                if (!string.IsNullOrWhiteSpace(eventId))
                {
                    BindSessions(eventId);
                    BindMySessions(eventId, userId);
                }
            }
        }

        private void ShowFormInfo(bool ok, string msg)
        {
            FormMessage.Text = ok
                ? $"<span style='color:#0a6b4f'>{Server.HtmlEncode(msg)}</span>"
                : $"<span style='color:#b00020'>{Server.HtmlEncode(msg)}</span>";
        }

        private static int SafeParseInt(string s, int fallback)
        {
            if (int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v)) return v;
            return fallback;
        }

        // ---------- Enrollment store (XML) ----------
        private void EnsureEnrollmentsDoc()
        {
            lock (EnrollmentsLock)
            {
                if (File.Exists(EnrollmentsXmlPath)) return;
                var doc = new XmlDocument();
                var root = doc.CreateElement("enrollments");
                doc.AppendChild(root);
                doc.Save(EnrollmentsXmlPath);
            }
        }

        private XmlDocument LoadEnrollmentsDoc()
        {
            EnsureEnrollmentsDoc();
            var doc = new XmlDocument();
            doc.Load(EnrollmentsXmlPath);
            return doc;
        }

        private void SaveEnrollmentsDoc(XmlDocument doc)
        {
            lock (EnrollmentsLock)
            {
                doc.Save(EnrollmentsXmlPath);
            }
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
            return SafeParseInt(s["capacity"]?.InnerText, 0);
        }

        // ENROLL: only if seats remain; add user to <enrolled>
        private bool TryEnroll(string eventId, string sessionId, string userId, out string message)
        {
            lock (EnrollmentsLock)
            {
                var cap = GetCapacity(eventId, sessionId);
                var doc = LoadEnrollmentsDoc();
                var ses = EnsureSessionNode(doc, eventId, sessionId);

                // already enrolled?
                if (ses.SelectSingleNode($"enrolled/user[@id='{userId}']") != null)
                {
                    message = "You’re already enrolled in this session.";
                    return true;
                }
                // already waitlisted?
                if (ses.SelectSingleNode($"waitlist/user[@id='{userId}']") != null)
                {
                    message = "You’re already on the waitlist for this session.";
                    return false;
                }

                var currentEnrolled = ses.SelectNodes("enrolled/user")?.Count ?? 0;
                var remaining = Math.Max(0, cap - currentEnrolled);

                if (remaining <= 0)
                {
                    message = "This session is full. You can join the waitlist instead.";
                    return false;
                }

                // enroll
                var u = doc.CreateElement("user");
                u.SetAttribute("id", userId);
                u.SetAttribute("ts", DateTime.UtcNow.ToString("o"));
                ses.SelectSingleNode("enrolled").AppendChild(u);

                SaveEnrollmentsDoc(doc);
                message = "Enrolled successfully!";
                return true;
            }
        }

        // WAITLIST: only if full; add user to <waitlist>
        private bool TryWaitlist(string eventId, string sessionId, string userId, out string message)
        {
            lock (EnrollmentsLock)
            {
                var cap = GetCapacity(eventId, sessionId);
                var doc = LoadEnrollmentsDoc();
                var ses = EnsureSessionNode(doc, eventId, sessionId);

                // if seats remain, don't allow waitlist
                var currentEnrolled = ses.SelectNodes("enrolled/user")?.Count ?? 0;
                var remaining = Math.Max(0, cap - currentEnrolled);
                if (remaining > 0)
                {
                    message = "Seats are still available; please enroll instead of joining the waitlist.";
                    return false;
                }

                if (ses.SelectSingleNode($"enrolled/user[@id='{userId}']") != null)
                {
                    message = "You’re already enrolled in this session.";
                    return false;
                }
                if (ses.SelectSingleNode($"waitlist/user[@id='{userId}']") != null)
                {
                    message = "You’re already on the waitlist for this session.";
                    return false;
                }

                var u = doc.CreateElement("user");
                u.SetAttribute("id", userId);
                u.SetAttribute("ts", DateTime.UtcNow.ToString("o"));
                ses.SelectSingleNode("waitlist").AppendChild(u);

                SaveEnrollmentsDoc(doc);
                message = "Added to waitlist.";
                return true;
            }
        }

        // COMPLETE: unenroll user, then promote first waitlisted user (if any)
        private bool TryMarkComplete(string eventId, string sessionId, string userId, out string message)
        {
            lock (EnrollmentsLock)
            {
                var doc = LoadEnrollmentsDoc();
                var ses = EnsureSessionNode(doc, eventId, sessionId);

                var enrolledNode = (XmlElement)ses.SelectSingleNode($"enrolled/user[@id='{userId}']");
                if (enrolledNode == null)
                {
                    message = "You are not enrolled in this session.";
                    return false;
                }

                // remove enrollment (frees one seat)
                enrolledNode.ParentNode.RemoveChild(enrolledNode);

                // promote first waitlisted user (FIFO)
                var firstWait = (XmlElement)ses.SelectSingleNode("waitlist/user[1]");
                if (firstWait != null)
                {
                    // move to enrolled
                    var moved = doc.CreateElement("user");
                    moved.SetAttribute("id", firstWait.GetAttribute("id"));
                    moved.SetAttribute("ts", DateTime.UtcNow.ToString("o"));
                    ses.SelectSingleNode("enrolled").AppendChild(moved);

                    // remove from waitlist
                    firstWait.ParentNode.RemoveChild(firstWait);
                }

                SaveEnrollmentsDoc(doc);
                message = "Marked complete. Your seat was freed, and the next waitlisted participant (if any) was enrolled.";
                return true;
            }
        }
    }
}
