using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;

namespace CyberApp_FIA.Helper
{
    /// <summary>
    /// Helper schedule page.
    /// Shows all sessions where this helper is assigned in eventSessions.xml,
    /// resolving microcourse titles from microcourses.xml and converting
    /// stored UTC timestamps to local, human-readable strings.
    /// </summary>
    public partial class Schedule : Page
    {
        private string UsersXmlPath => Server.MapPath("~/App_Data/users.xml");
        private string EventSessionsXmlPath => Server.MapPath("~/App_Data/eventSessions.xml");
        private string MicrocoursesXmlPath => Server.MapPath("~/App_Data/microcourses.xml");
        private string EnrollmentsXmlPath => Server.MapPath("~/App_Data/enrollments.xml");
        private string HelperCheckinsXmlPath => Server.MapPath("~/App_Data/helperCheckins.xml");

        private static readonly object HelperCheckinsLock = new object();
        private static readonly TimeSpan HelperCheckinUndoWindow = TimeSpan.FromMinutes(5);

        /// <summary>
        /// Small DTO for binding schedule cards.
        /// </summary>
        private sealed class SessionRow
        {
            public string CourseTitle { get; set; }
            public string DayTime { get; set; }
            public string Capacity { get; set; }
            public DateTime StartUtc { get; set; } // for sorting
            public string SessionId { get; set; }  // for admit actions
            public string EventId { get; set; }    // for admit actions
            public string Room { get; set; }       // Zoom / session room link
            public bool HasCheckin { get; set; }   // whether helper has a check-in recorded
            public bool CanUndoCheckin { get; set; } // undo availability window
            public string CheckedInAtLabel { get; set; } // local time display

            public bool HasParticipants { get; set; }           // enrolled participants present
            public List<ParticipantRow> Participants { get; set; } // enrolled participants
        }

        /// <summary>
        /// Small DTO for enrolled participants shown in the card.
        /// </summary>
        private sealed class ParticipantRow
        {
            public string Name { get; set; }
            public bool Invited { get; set; } // true if admitted == true
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Enforce Helper role and bind their sessions.
                if (!EnsureHelperRole(out var userId))
                {
                    return; // EnsureHelperRole already redirected
                }

                // Pull the helper's identity keys (email / full name / first name)
                GetHelperIdentity(userId, out var email, out var fullName, out var firstName);

                BindSessions(email, fullName, firstName, userId);
            }
        }

        /// <summary>
        /// Ensures only Helpers reach this page.
        /// Returns false and redirects if the role or user id is missing/invalid.
        /// </summary>
        private bool EnsureHelperRole(out string userId)
        {
            userId = Session["UserId"] as string ?? "";

            var roleRaw = Session["Role"] as string ?? "";
            if (!string.Equals(roleRaw.Trim(), "Helper", StringComparison.OrdinalIgnoreCase) ||
                string.IsNullOrWhiteSpace(userId))
            {
                Response.Redirect("~/Account/Login.aspx");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Reads users.xml and returns the helper's email, full name, and first name.
        /// We use these to match sessions that may store the helper as an email,
        /// full display name, or just first name (older data).
        /// </summary>
        private void GetHelperIdentity(string userId, out string email, out string fullName, out string firstName)
        {
            email = "";
            fullName = "";
            firstName = "";

            try
            {
                if (File.Exists(UsersXmlPath))
                {
                    var doc = new XmlDocument();
                    doc.Load(UsersXmlPath);

                    var node = doc.SelectSingleNode($"/users/user[@id='{userId}']") as XmlElement;
                    if (node != null)
                    {
                        firstName = (node["firstName"]?.InnerText ?? "").Trim();
                        var lastName = (node["lastName"]?.InnerText ?? "").Trim();
                        email = (node["email"]?.InnerText ?? "").Trim();

                        fullName = (firstName + " " + lastName).Trim();
                    }
                }
            }
            catch
            {
                // If something goes wrong, we fall back to session-based info if available later.
            }

            // Basic fallbacks so we always have some keys to try.
            var sessionEmail = Session["Email"] as string ?? "";
            if (string.IsNullOrWhiteSpace(email) && !string.IsNullOrWhiteSpace(sessionEmail))
            {
                email = sessionEmail.Trim();
            }

            if (string.IsNullOrWhiteSpace(fullName))
            {
                fullName = (Session["DisplayName"] as string ?? "").Trim();
            }
        }

        /// <summary>
        /// Binds all sessions where the &lt;helper&gt; node matches the current helper
        /// (by email, full name, or first name), and decorates each row with
        /// session room + helper check-in state + enrolled participants.
        /// </summary>
        private void BindSessions(string email, string fullName, string firstName, string helperId)
        {
            var rows = new List<SessionRow>();

            if (!File.Exists(EventSessionsXmlPath))
            {
                NoSessionsPH.Visible = true;
                SessionsRepeater.DataSource = rows;
                SessionsRepeater.DataBind();
                return;
            }

            // Build courseId -> title map from microcourses.xml (best-effort).
            var courseTitles = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (File.Exists(MicrocoursesXmlPath))
            {
                try
                {
                    var md = new XmlDocument();
                    md.Load(MicrocoursesXmlPath);

                    foreach (XmlElement c in md.SelectNodes("/microcourses/course"))
                    {
                        var id = c.GetAttribute("id");
                        if (string.IsNullOrEmpty(id)) continue;

                        var title = c["title"]?.InnerText ?? "(untitled)";
                        courseTitles[id] = title;
                    }
                }
                catch
                {
                    // If microcourses fail to load, we just fall back to "(untitled)" later.
                }
            }

            XmlDocument checkinsDoc = null;
            try
            {
                checkinsDoc = LoadHelperCheckinsDoc();
            }
            catch
            {
                // If helper checkins file cannot be read, we just skip check-in state.
            }

            // Load enrollments.xml (read-only) to show currently enrolled participants.
            XmlDocument enrollmentsDoc = null;
            if (File.Exists(EnrollmentsXmlPath))
            {
                try
                {
                    enrollmentsDoc = new XmlDocument();
                    enrollmentsDoc.Load(EnrollmentsXmlPath);
                }
                catch
                {
                    enrollmentsDoc = null;
                }
            }

            // Load all sessions and filter to those assigned to this helper.
            var doc = new XmlDocument();
            doc.Load(EventSessionsXmlPath);

            foreach (XmlElement s in doc.SelectNodes("/eventSessions/session"))
            {
                var helperVal = (s["helper"]?.InnerText ?? "").Trim();
                if (string.IsNullOrWhiteSpace(helperVal))
                {
                    continue;
                }

                if (!HelperMatches(helperVal, email, fullName, firstName))
                {
                    continue;
                }

                var eventId = s.GetAttribute("eventId");
                var sessionId = s.GetAttribute("id");

                var courseId = s["courseId"]?.InnerText ?? "";
                var startIso = s["start"]?.InnerText ?? "";
                var endIso = s["end"]?.InnerText ?? "";
                var capRaw = (s["capacity"]?.InnerText ?? "").Trim();
                var room = s["room"]?.InnerText ?? "";

                if (!TryParseIsoUtc(startIso, out var startUtc) ||
                    !TryParseIsoUtc(endIso, out var endUtc))
                {
                    // Skip malformed or empty times; schedule view should be clean.
                    continue;
                }

                var localStart = startUtc.ToLocalTime();
                var localEnd = endUtc.ToLocalTime();

                string title;
                if (!string.IsNullOrEmpty(courseId) && courseTitles.TryGetValue(courseId, out var t))
                {
                    title = t;
                }
                else
                {
                    title = "(untitled microcourse)";
                }

                // Example: "Thu, Nov 6 2025 • 5:00–5:30 PM"
                var dayPart = localStart.ToString("ddd, MMM d yyyy", CultureInfo.CurrentCulture);
                var timeStart = localStart.ToString("h:mm tt", CultureInfo.CurrentCulture);
                var timeEnd = localEnd.ToString("h:mm tt", CultureInfo.CurrentCulture);
                var dayTime = $"{dayPart} • {timeStart}–{timeEnd}";

                var capacity = string.IsNullOrWhiteSpace(capRaw) ? "—" : capRaw;

                bool hasCheckin = false;
                bool canUndo = false;
                string checkedLabel = null;

                if (!string.IsNullOrWhiteSpace(helperId) && checkinsDoc != null)
                {
                    GetCheckinState(checkinsDoc, helperId, sessionId, out hasCheckin, out canUndo, out checkedLabel);
                }

                // Enrolled participants for this session.
                var participants = GetParticipantsForSession(enrollmentsDoc, eventId, sessionId);
                var hasParticipants = participants != null && participants.Count > 0;

                rows.Add(new SessionRow
                {
                    CourseTitle = title,
                    DayTime = dayTime,
                    Capacity = capacity,
                    StartUtc = startUtc,
                    SessionId = sessionId,
                    EventId = eventId,
                    Room = room,
                    HasCheckin = hasCheckin,
                    CanUndoCheckin = canUndo,
                    CheckedInAtLabel = checkedLabel,
                    HasParticipants = hasParticipants,
                    Participants = participants
                });
            }

            // Sort by start time ascending so upcoming sessions read top-to-bottom.
            rows.Sort((a, b) => a.StartUtc.CompareTo(b.StartUtc));

            NoSessionsPH.Visible = rows.Count == 0;
            SessionsRepeater.DataSource = rows;
            SessionsRepeater.DataBind();
        }

        /// <summary>
        /// Matches the stored helper string from eventSessions.xml against
        /// the current helper's email, full name, or first name (case-insensitive).
        /// This supports both new and older styles of storing helper identifiers.
        /// </summary>
        private static bool HelperMatches(string storedHelper, string email, string fullName, string firstName)
        {
            if (string.IsNullOrWhiteSpace(storedHelper))
            {
                return false;
            }

            var val = storedHelper.Trim();

            bool EqualsIgnoreCase(string a, string b)
                => !string.IsNullOrWhiteSpace(a) &&
                   !string.IsNullOrWhiteSpace(b) &&
                   string.Equals(a.Trim(), b.Trim(), StringComparison.OrdinalIgnoreCase);

            if (EqualsIgnoreCase(val, email)) return true;
            if (EqualsIgnoreCase(val, fullName)) return true;
            if (EqualsIgnoreCase(val, firstName)) return true;

            return false;
        }

        /// <summary>
        /// Parses an ISO-8601 "o" format string that is expected to be UTC,
        /// returning a DateTime in UTC on success.
        /// </summary>
        private static bool TryParseIsoUtc(string iso, out DateTime utc)
        {
            return DateTime.TryParseExact(
                iso,
                "o",
                CultureInfo.InvariantCulture,
                DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal,
                out utc);
        }

        /// <summary>
        /// Event handler wired via OnItemCommand in Schedule.aspx.
        /// - admit: mark enrolled users as admitted so Participants see the room link.
        /// - checkinHelper: record Helper check-in with timestamp, helper id/name, session id.
        /// - undoCheckinHelper: allow undo for a short window.
        /// </summary>
        protected void SessionsRepeater_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            var arg = Convert.ToString(e.CommandArgument ?? string.Empty);
            var parts = arg.Split('|');
            if (parts.Length != 2)
                return;

            var eventId = parts[0];
            var sessionId = parts[1];

            if (string.IsNullOrWhiteSpace(eventId) || string.IsNullOrWhiteSpace(sessionId))
                return;

            // Ensure helper is valid and load identity (for name + later rebind)
            if (!EnsureHelperRole(out var helperId))
            {
                return;
            }

            string email, fullName, firstName;
            GetHelperIdentity(helperId, out email, out fullName, out firstName);

            // Prefer full name, then email, then helper id
            var helperName =
                !string.IsNullOrWhiteSpace(fullName) ? fullName :
                !string.IsNullOrWhiteSpace(email) ? email :
                helperId;

            try
            {
                if (string.Equals(e.CommandName, "admit", StringComparison.OrdinalIgnoreCase))
                {
                    AdmitParticipants(eventId, sessionId);

                    // FIA toast instead of browser alert
                    ClientScript.RegisterStartupScript(
                        GetType(),
                        "AdmitConfirm",
                        "showFiaToast('Participants were sent the room link.');",
                        true);
                }
                else if (string.Equals(e.CommandName, "checkinHelper", StringComparison.OrdinalIgnoreCase))
                {
                    RecordHelperCheckin(helperId, helperName, eventId, sessionId);

                    ClientScript.RegisterStartupScript(
                        GetType(),
                        "HelperCheckin",
                        "showFiaToast('You are checked in for this session. You can undo this for a few minutes.');",
                        true);
                }
                else if (string.Equals(e.CommandName, "undoCheckinHelper", StringComparison.OrdinalIgnoreCase))
                {
                    if (TryUndoHelperCheckin(helperId, eventId, sessionId))
                    {
                        ClientScript.RegisterStartupScript(
                            GetType(),
                            "UndoHelperCheckin",
                            "showFiaToast('Your check-in has been undone.');",
                            true);
                    }
                }
            }
            catch
            {
                // Keep Helper UI quiet on errors; nothing else depends on this.
            }
            finally
            {
                // Rebind so that check-in/undo state and participant list refresh.
                BindSessions(email, fullName, firstName, helperId);
            }
        }

        private XmlDocument LoadEnrollmentsDoc()
        {
            var doc = new XmlDocument();

            if (File.Exists(EnrollmentsXmlPath))
            {
                doc.Load(EnrollmentsXmlPath);
                if (doc.DocumentElement == null)
                {
                    var rootMissing = doc.CreateElement("enrollments");
                    doc.AppendChild(rootMissing);
                }
            }
            else
            {
                var root = doc.CreateElement("enrollments");
                doc.AppendChild(root);
            }

            return doc;
        }

        private void SaveEnrollmentsDoc(XmlDocument doc)
        {
            doc.Save(EnrollmentsXmlPath);
        }

        /// <summary>
        /// Mark all currently enrolled users in a session as admitted so
        /// their "My Sessions" cards can show the Join Room link.
        /// </summary>
        private void AdmitParticipants(string eventId, string sessionId)
        {
            var doc = LoadEnrollmentsDoc();
            var ses = doc.SelectSingleNode($"/enrollments/session[@eventId='{eventId}' and @id='{sessionId}']") as XmlElement;
            if (ses == null)
                return;

            var nowIso = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);

            foreach (XmlElement u in ses.SelectNodes("enrolled/user"))
            {
                u.SetAttribute("admitted", "true");
                u.SetAttribute("admittedAt", nowIso);
            }

            SaveEnrollmentsDoc(doc);
        }

        // ---------- Helper check-ins storage ----------

        private XmlDocument LoadHelperCheckinsDoc()
        {
            var doc = new XmlDocument();

            if (File.Exists(HelperCheckinsXmlPath))
            {
                doc.Load(HelperCheckinsXmlPath);
                if (doc.DocumentElement == null)
                {
                    var rootMissing = doc.CreateElement("helperCheckins");
                    doc.AppendChild(rootMissing);
                }
            }
            else
            {
                var root = doc.CreateElement("helperCheckins");
                doc.AppendChild(root);
            }

            return doc;
        }

        private void SaveHelperCheckinsDoc(XmlDocument doc)
        {
            lock (HelperCheckinsLock)
            {
                doc.Save(HelperCheckinsXmlPath);
            }
        }

        /// <summary>
        /// Records a helper check-in for the given session with timestamp,
        /// helper id, helper name, event id, and session id.
        /// </summary>
        private void RecordHelperCheckin(string helperId, string helperName, string eventId, string sessionId)
        {
            if (string.IsNullOrWhiteSpace(helperId) || string.IsNullOrWhiteSpace(sessionId))
                return;

            var doc = LoadHelperCheckinsDoc();
            var root = doc.DocumentElement;
            if (root == null)
            {
                root = doc.CreateElement("helperCheckins");
                doc.AppendChild(root);
            }

            var node = doc.CreateElement("checkin");
            node.SetAttribute("helperId", helperId);
            node.SetAttribute("helperName", helperName ?? string.Empty);
            node.SetAttribute("eventId", eventId ?? string.Empty);
            node.SetAttribute("sessionId", sessionId ?? string.Empty);
            node.SetAttribute("tsUtc", DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));

            root.AppendChild(node);
            SaveHelperCheckinsDoc(doc);
        }

        /// <summary>
        /// Attempts to undo the most recent helper check-in for this helper and session
        /// if it falls within the configured undo window.
        /// </summary>
        private bool TryUndoHelperCheckin(string helperId, string eventId, string sessionId)
        {
            if (string.IsNullOrWhiteSpace(helperId) || string.IsNullOrWhiteSpace(sessionId))
                return false;

            var doc = LoadHelperCheckinsDoc();
            var root = doc.DocumentElement;
            if (root == null)
                return false;

            XmlElement latest = null;
            DateTime latestTsUtc = default;
            foreach (XmlElement ck in doc.SelectNodes($"/helperCheckins/checkin[@helperId='{helperId}' and @sessionId='{sessionId}']"))
            {
                var tsRaw = ck.GetAttribute("tsUtc");
                if (!DateTime.TryParse(tsRaw, null, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var tsUtc))
                    continue;

                if (latest == null || tsUtc > latestTsUtc)
                {
                    latest = ck;
                    latestTsUtc = tsUtc;
                }
            }

            if (latest == null)
                return false;

            var age = DateTime.UtcNow - latestTsUtc;
            if (age > HelperCheckinUndoWindow)
            {
                // Undo window expired.
                return false;
            }

            root.RemoveChild(latest);
            SaveHelperCheckinsDoc(doc);
            return true;
        }

        /// <summary>
        /// Reads helper check-in state (has check-in, undo availability, display label)
        /// for the given helper and session from the helper check-ins document.
        /// </summary>
        private void GetCheckinState(
            XmlDocument doc,
            string helperId,
            string sessionId,
            out bool hasCheckin,
            out bool canUndo,
            out string label)
        {
            hasCheckin = false;
            canUndo = false;
            label = null;

            if (doc == null || doc.DocumentElement == null ||
                string.IsNullOrWhiteSpace(helperId) ||
                string.IsNullOrWhiteSpace(sessionId))
            {
                return;
            }

            XmlElement latest = null;
            DateTime latestTsUtc = default;

            foreach (XmlElement ck in doc.SelectNodes($"/helperCheckins/checkin[@helperId='{helperId}' and @sessionId='{sessionId}']"))
            {
                var tsRaw = ck.GetAttribute("tsUtc");
                if (!DateTime.TryParse(tsRaw, null, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var tsUtc))
                    continue;

                if (latest == null || tsUtc > latestTsUtc)
                {
                    latest = ck;
                    latestTsUtc = tsUtc;
                }
            }

            if (latest == null)
            {
                return;
            }

            hasCheckin = true;

            var local = DateTime.SpecifyKind(latestTsUtc, DateTimeKind.Utc).ToLocalTime();
            label = local.ToString("h:mm tt", CultureInfo.CurrentCulture);

            var age = DateTime.UtcNow - latestTsUtc;
            canUndo = age <= HelperCheckinUndoWindow;
        }

        /// <summary>
        /// Returns enrolled participants for a session with their invite status.
        /// Names are resolved from enrollments.xml attributes if present,
        /// otherwise we fall back to users.xml via userId / id.
        /// </summary>
        private List<ParticipantRow> GetParticipantsForSession(XmlDocument enrollmentsDoc, string eventId, string sessionId)
        {
            var result = new List<ParticipantRow>();

            if (enrollmentsDoc == null || enrollmentsDoc.DocumentElement == null ||
                string.IsNullOrWhiteSpace(sessionId))
            {
                return result;
            }

            var ses = enrollmentsDoc.SelectSingleNode($"/enrollments/session[@eventId='{eventId}' and @id='{sessionId}']") as XmlElement;
            if (ses == null)
            {
                return result;
            }

            // Load users.xml once to help resolve names from userId / id.
            XmlDocument usersDoc = null;
            if (File.Exists(UsersXmlPath))
            {
                try
                {
                    usersDoc = new XmlDocument();
                    usersDoc.Load(UsersXmlPath);
                }
                catch
                {
                    usersDoc = null;
                }
            }

            foreach (XmlElement u in ses.SelectNodes("enrolled/user"))
            {
                var name = ResolveParticipantName(u, usersDoc);

                var admittedAttr = u.GetAttribute("admitted");
                var invited = string.Equals(admittedAttr, "true", StringComparison.OrdinalIgnoreCase);

                result.Add(new ParticipantRow
                {
                    Name = name,
                    Invited = invited
                });
            }

            return result;
        }

        /// <summary>
        /// Resolves a participant's display name from the enrollment node,
        /// falling back to users.xml (firstName, displayName, or email)
        /// and finally to "Participant".
        /// </summary>
        private string ResolveParticipantName(XmlElement enrolledUser, XmlDocument usersDoc)
        {
            if (enrolledUser == null)
            {
                return "Participant";
            }

            // Try attributes directly on the enrollment <user> node.
            var firstAttr = enrolledUser.GetAttribute("firstName");
            if (!string.IsNullOrWhiteSpace(firstAttr))
            {
                return firstAttr;
            }

            var displayAttr = enrolledUser.GetAttribute("displayName");
            if (!string.IsNullOrWhiteSpace(displayAttr))
            {
                return displayAttr;
            }

            var nameAttr = enrolledUser.GetAttribute("name");
            if (!string.IsNullOrWhiteSpace(nameAttr))
            {
                return nameAttr;
            }

            // In enrollments.xml the user is stored as either userId="..." or id="..."
            var userId = enrolledUser.GetAttribute("userId");
            if (string.IsNullOrWhiteSpace(userId))
            {
                userId = enrolledUser.GetAttribute("id");
            }

            if (!string.IsNullOrWhiteSpace(userId) && usersDoc != null)
            {
                try
                {
                    var node = usersDoc.SelectSingleNode($"/users/user[@id='{userId}']") as XmlElement;
                    if (node != null)
                    {
                        var firstNode = (node["firstName"]?.InnerText ?? "").Trim();
                        if (!string.IsNullOrWhiteSpace(firstNode))
                        {
                            return firstNode;
                        }

                        var displayNode = (node["displayName"]?.InnerText ?? "").Trim();
                        if (!string.IsNullOrWhiteSpace(displayNode))
                        {
                            return displayNode;
                        }

                        var emailNode = (node["email"]?.InnerText ?? "").Trim();
                        if (!string.IsNullOrWhiteSpace(emailNode))
                        {
                            return emailNode;
                        }
                    }
                }
                catch
                {
                    // If users.xml lookup fails, fall through to generic label.
                }
            }

            return "Participant";
        }
    }
}

