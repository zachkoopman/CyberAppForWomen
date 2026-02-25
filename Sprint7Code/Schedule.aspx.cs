using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using CyberApp_FIA.Services;


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

        // NEW: participant completions storage (shared with Participant view)
        private string CompletionsXmlPath => Server.MapPath("~/App_Data/completions.xml");

        // NEW: missing participant sessions storage (Helper marks no-shows)
        private string MissingParticipantSessionsXmlPath => Server.MapPath("~/App_Data/missingParticipantSessions.xml");

        private static readonly object EnrollmentsLock = new object();
        private static readonly object CompletionsLock = new object();
        private static readonly object MissingLock = new object();

        // New: helper progress + notes storage
        private string HelperProgressXmlPath => Server.MapPath("~/App_Data/helperProgress.xml");
        private string HelperNotesXmlPath => Server.MapPath("~/App_Data/helperNotes.xml");

        private static readonly object HelperCheckinsLock = new object();
        private static readonly TimeSpan HelperCheckinUndoWindow = TimeSpan.FromMinutes(5);

        // New: locks + window for delivered-session undo
        private static readonly object HelperProgressLock = new object();
        private static readonly object HelperNotesLock = new object();
        private static readonly TimeSpan DeliverUndoWindow = TimeSpan.FromMinutes(5);

        private const string DeliverHistorySessionKey = "DeliverHistory";

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

            public bool HasActiveInvite { get; set; }      // true if #1 participant has been invited/admitted
            public string ActiveParticipantId { get; set; } // the admitted participant id (the one the new buttons act on)
        }

        /// <summary>
        /// Small DTO for enrolled participants shown in the card.
        /// </summary>
        private sealed class ParticipantRow
        {
            public string Name { get; set; }
            public bool Invited { get; set; } // true if admitted == true
        }

        /// <summary>
        /// DTO for recent delivered-session history rows.
        /// </summary>
        private sealed class DeliverHistoryRow
        {
            public string CourseTitle { get; set; }
            public string WhenLabel { get; set; }
            public string Snapshot { get; set; }
            public long Ticks { get; set; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            // Always enforce Helper role on load
            if (!EnsureHelperRole(out var userId))
            {
                return;
            }

            if (!IsPostBack)
            {
                // Only bind dropdown + sessions the first time
                try
                {
                    BindDeliverableCourses(userId);
                }
                catch
                {
                    // Keep schedule usable even if helperProgress.xml has issues
                }

                GetHelperIdentity(userId, out var email, out var fullName, out var firstName);
                BindSessions(email, fullName, firstName, userId);

                try
                {
                    BindDeliverHistory(userId);
                }
                catch
                {
                    // History is best-effort; never block page load.
                }
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
        /// Binds the dropdown of courses where this helper is currently
        /// eligible or certified, based on helperProgress.xml.
        /// </summary>
        private void BindDeliverableCourses(string helperId)
        {
            if (DeliverCourseDropDown == null)
            {
                return;
            }

            DeliverCourseDropDown.Items.Clear();
            DeliverCourseDropDown.Items.Add(new ListItem("Select a course…", ""));

            if (!File.Exists(HelperProgressXmlPath))
            {
                return;
            }

            var doc = new XmlDocument();
            doc.Load(HelperProgressXmlPath);

            // We don't assume a specific root; just search for helper by id.
            var helperNode = doc.SelectSingleNode($"//helper[@id='{helperId}']") as XmlElement;
            if (helperNode == null)
            {
                return;
            }

            foreach (XmlElement courseEl in helperNode.SelectNodes("course"))
            {
                var id = courseEl.GetAttribute("id");
                if (string.IsNullOrWhiteSpace(id))
                {
                    continue;
                }

                var titleNode = courseEl["title"];
                var title = titleNode != null ? (titleNode.InnerText ?? "").Trim() : id;

                var isEligibleText = courseEl["isEligible"]?.InnerText ?? "";
                var isCertifiedText = courseEl["isCertified"]?.InnerText ?? "";

                bool isEligible = string.Equals(isEligibleText.Trim(), "true", StringComparison.OrdinalIgnoreCase);
                bool isCertified = string.Equals(isCertifiedText.Trim(), "true", StringComparison.OrdinalIgnoreCase);

                if (isEligible || isCertified)
                {
                    DeliverCourseDropDown.Items.Add(new ListItem(title, id));
                }
            }
        }

        /// <summary>
        /// Binds the "recent delivered sessions" history list,
        /// showing up to the last three logs for this helper.
        /// </summary>
        private void BindDeliverHistory(string helperId)
        {
            if (DeliverHistoryRepeater == null || DeliverHistoryEmpty == null)
            {
                return;
            }

            var rows = new List<DeliverHistoryRow>();

            var list = Session[DeliverHistorySessionKey] as List<string>;
            if (list != null && !string.IsNullOrWhiteSpace(helperId))
            {
                XmlDocument progressDoc = null;
                try
                {
                    if (File.Exists(HelperProgressXmlPath))
                    {
                        progressDoc = new XmlDocument();
                        progressDoc.Load(HelperProgressXmlPath);
                    }
                }
                catch
                {
                    progressDoc = null;
                }

                foreach (var snap in list)
                {
                    if (string.IsNullOrWhiteSpace(snap))
                    {
                        continue;
                    }

                    var parts = snap.Split('|');
                    if (parts.Length != 5)
                    {
                        continue;
                    }

                    var snapHelperId = parts[0];
                    if (!string.Equals(helperId, snapHelperId, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    var courseId = parts[1];
                    if (!long.TryParse(parts[4], NumberStyles.Integer, CultureInfo.InvariantCulture, out var ticks))
                    {
                        continue;
                    }

                    string title = courseId;

                    if (progressDoc != null)
                    {
                        try
                        {
                            var helperNode = progressDoc.SelectSingleNode($"//helper[@id='{helperId}']") as XmlElement;
                            if (helperNode != null)
                            {
                                var courseNode = helperNode.SelectSingleNode($"course[@id='{courseId}']") as XmlElement;
                                if (courseNode != null)
                                {
                                    var tNode = courseNode["title"];
                                    var text = tNode != null ? (tNode.InnerText ?? "").Trim() : null;
                                    if (!string.IsNullOrWhiteSpace(text))
                                    {
                                        title = text;
                                    }
                                }
                            }
                        }
                        catch
                        {
                            // Best-effort; fall back to courseId if needed.
                        }
                    }

                    var local = new DateTime(ticks, DateTimeKind.Utc).ToLocalTime();
                    var whenLabel = local.ToString("MMM d • h:mm tt", CultureInfo.CurrentCulture);

                    rows.Add(new DeliverHistoryRow
                    {
                        CourseTitle = title,
                        WhenLabel = whenLabel,
                        Snapshot = snap,
                        Ticks = ticks
                    });
                }
            }

            // Show the most recent three, newest first.
            rows.Sort((a, b) => b.Ticks.CompareTo(a.Ticks));
            if (rows.Count > 3)
            {
                rows = rows.GetRange(0, 3);
            }

            DeliverHistoryRepeater.DataSource = rows;
            DeliverHistoryRepeater.DataBind();

            DeliverHistoryEmpty.Visible = rows.Count == 0;
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

                // Hide sessions 30 minutes after they end
                if (DateTime.UtcNow > endUtc.AddMinutes(30))
                {
                    continue;
                }

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

                // Active invited participant = the first enrolled user who has admitted="true"
                string activeParticipantId = null;
                bool hasActiveInvite = false;

                if (enrollmentsDoc != null)
                {
                    var sesNode = enrollmentsDoc.SelectSingleNode(
                        $"/enrollments/session[@eventId='{eventId}' and @id='{sessionId}']") as XmlElement;

                    if (sesNode != null)
                    {
                        var activeNode = sesNode.SelectSingleNode("enrolled/user[@admitted='true'][1]") as XmlElement;
                        if (activeNode != null)
                        {
                            activeParticipantId = activeNode.GetAttribute("id");
                            hasActiveInvite = !string.IsNullOrWhiteSpace(activeParticipantId);
                        }
                    }
                }

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
                    Participants = participants,
                    HasActiveInvite = hasActiveInvite,
                    ActiveParticipantId = activeParticipantId,
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
            if (parts.Length < 2)
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

            // For audit logging, resolve the microcourse title (best-effort).
            var courseTitleForLog = GetCourseTitleForSession(eventId, sessionId);


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

                    try
                    {
                        var details = string.Format(
                            "Helper admitted enrolled participants and sent the room link for session \"{0}\" (eventId={1}, sessionId={2}).",
                            courseTitleForLog,
                            eventId,
                            sessionId);

                        UniversityAuditLogger.AppendForCurrentUser(
                            this,
                            "Helper Admit Participants",
                            details);
                    }
                    catch
                    {
                        // Audit log is best-effort; do not block UI.
                    }


                    // FIA toast instead of browser alert
                    ClientScript.RegisterStartupScript(
                        GetType(),
                        "AdmitConfirm",
                        "showFiaToast('Participants were sent the room link.');",
                        true);
                }
                else if (string.Equals(e.CommandName, "completeParticipant", StringComparison.OrdinalIgnoreCase))
                {
                    // parts = eventId|sessionId|participantId
                    if (parts.Length != 3) return;
                    var participantId = parts[2];

                    CompleteWithParticipant(eventId, sessionId, participantId, helperId);

                    ClientScript.RegisterStartupScript(
                        GetType(),
                        "CompleteParticipant",
                        "showFiaToast('Participant marked complete. Next participant is ready to admit.');",
                        true);
                }
                else if (string.Equals(e.CommandName, "markMissingParticipant", StringComparison.OrdinalIgnoreCase))
                {
                    if (parts.Length != 3) return;
                    var participantId = parts[2];

                    MarkParticipantMissing(eventId, sessionId, participantId, helperId);

                    ClientScript.RegisterStartupScript(
                        GetType(),
                        "MissingParticipant",
                        "showFiaToast('Participant marked missing. Next participant is ready to admit.');",
                        true);
                }
                else if (string.Equals(e.CommandName, "checkinHelper", StringComparison.OrdinalIgnoreCase))
                {
                    RecordHelperCheckin(helperId, helperName, eventId, sessionId);

                    try
                    {
                        var details = string.Format(
                            "Helper checked in for session \"{0}\" (eventId={1}, sessionId={2}).",
                            courseTitleForLog,
                            eventId,
                            sessionId);

                        UniversityAuditLogger.AppendForCurrentUser(
                            this,
                            "Helper Check In",
                            details);
                    }
                    catch
                    {
                    }


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
                        try
                        {
                            var details = string.Format(
                                "Helper undid their check-in for session \"{0}\" (eventId={1}, sessionId={2}).",
                                courseTitleForLog,
                                eventId,
                                sessionId);

                            UniversityAuditLogger.AppendForCurrentUser(
                                this,
                                "Helper Check In (Undo)",
                                details);
                        }
                        catch
                        {
                        }

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

            // Admit ONLY the first enrolled participant (1-based XPath index).
            var first = ses.SelectSingleNode("enrolled/user[1]") as XmlElement;
            if (first == null)
            {
                // No enrolled participants to admit.
                return;
            }

            first.SetAttribute("admitted", "true");
            first.SetAttribute("admittedAt", nowIso);

            SaveEnrollmentsDoc(doc);
        }

        private void EnsureXmlDoc(string path, string rootName)
        {
            if (File.Exists(path)) return;
            var doc = new XmlDocument();
            doc.AppendChild(doc.CreateElement(rootName));
            doc.Save(path);
        }

        private string GetCourseIdForSession(string eventId, string sessionId)
        {
            try
            {
                if (!File.Exists(EventSessionsXmlPath)) return null;
                var doc = new XmlDocument();
                doc.Load(EventSessionsXmlPath);

                var ses = doc.SelectSingleNode($"/eventSessions/session[@eventId='{eventId}' and @id='{sessionId}']") as XmlElement;
                return ses?["courseId"]?.InnerText?.Trim();
            }
            catch
            {
                return null;
            }
        }

        private void MarkCourseCompletedForUser(string userId, string courseId)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(courseId))
                return;

            lock (CompletionsLock)
            {
                EnsureXmlDoc(CompletionsXmlPath, "completions");

                var doc = new XmlDocument();
                doc.Load(CompletionsXmlPath);

                var user = doc.SelectSingleNode($"/completions/user[@id='{userId}']") as XmlElement;
                if (user == null)
                {
                    user = doc.CreateElement("user");
                    user.SetAttribute("id", userId);
                    doc.DocumentElement.AppendChild(user);
                }

                var existing = user.SelectSingleNode($"course[@id='{courseId}']") as XmlElement;
                if (existing == null)
                {
                    var c = doc.CreateElement("course");
                    c.SetAttribute("id", courseId);
                    c.SetAttribute("completedOn", DateTime.UtcNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
                    user.AppendChild(c);
                }
                else if (!existing.HasAttribute("completedOn"))
                {
                    existing.SetAttribute("completedOn", DateTime.UtcNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
                }

                doc.Save(CompletionsXmlPath);
            }
        }

        private void AppendMissingRecord(string eventId, string sessionId, string participantId, string helperId)
        {
            lock (MissingLock)
            {
                EnsureXmlDoc(MissingParticipantSessionsXmlPath, "missingParticipantSessions");

                var doc = new XmlDocument();
                doc.Load(MissingParticipantSessionsXmlPath);

                var root = doc.DocumentElement;
                if (root == null)
                {
                    root = doc.CreateElement("missingParticipantSessions");
                    doc.AppendChild(root);
                }

                var miss = doc.CreateElement("missing");
                miss.SetAttribute("eventId", eventId ?? "");
                miss.SetAttribute("sessionId", sessionId ?? "");
                miss.SetAttribute("participantId", participantId ?? "");
                miss.SetAttribute("helperId", helperId ?? "");
                miss.SetAttribute("tsUtc", DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));

                root.AppendChild(miss);
                doc.Save(MissingParticipantSessionsXmlPath);
            }
        }

        private void ClearAdmittedFlags(XmlElement sessionNode)
        {
            if (sessionNode == null) return;
            foreach (XmlElement u in sessionNode.SelectNodes("enrolled/user"))
            {
                if (u.HasAttribute("admitted")) u.RemoveAttribute("admitted");
                if (u.HasAttribute("admittedAt")) u.RemoveAttribute("admittedAt");
            }
        }

        private void PromoteFirstWaitlistToEnrolled(XmlDocument doc, XmlElement sessionNode)
        {
            if (doc == null || sessionNode == null) return;

            var firstWait = sessionNode.SelectSingleNode("waitlist/user[1]") as XmlElement;
            if (firstWait == null) return;

            var moved = doc.CreateElement("user");
            moved.SetAttribute("id", firstWait.GetAttribute("id"));
            moved.SetAttribute("ts", DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));

            var enrolled = sessionNode.SelectSingleNode("enrolled") as XmlElement;
            enrolled?.AppendChild(moved);

            firstWait.ParentNode.RemoveChild(firstWait);
        }

        private void CompleteWithParticipant(string eventId, string sessionId, string participantId, string helperId)
        {
            if (string.IsNullOrWhiteSpace(eventId) || string.IsNullOrWhiteSpace(sessionId) || string.IsNullOrWhiteSpace(participantId))
                return;

            // 1) Remove participant from enrolled, 2) promote waitlist, 3) clear admitted flags, 4) save
            lock (EnrollmentsLock)
            {
                var doc = LoadEnrollmentsDoc();
                var ses = doc.SelectSingleNode($"/enrollments/session[@eventId='{eventId}' and @id='{sessionId}']") as XmlElement;
                if (ses == null) return;

                var enrolledNode = ses.SelectSingleNode($"enrolled/user[@id='{participantId}']") as XmlElement;
                if (enrolledNode == null) return;

                enrolledNode.ParentNode.RemoveChild(enrolledNode);

                // Keep the session filled if someone is waiting.
                PromoteFirstWaitlistToEnrolled(doc, ses);

                // Reset invite state so the next admit sends to the next #1 person.
                ClearAdmittedFlags(ses);

                SaveEnrollmentsDoc(doc);
            }

            // 5) Mark completion in completions.xml (course progress)
            var courseId = GetCourseIdForSession(eventId, sessionId);
            if (!string.IsNullOrWhiteSpace(courseId))
            {
                MarkCourseCompletedForUser(participantId, courseId);
            }

            // Best-effort audit log (optional)
            try
            {
                UniversityAuditLogger.AppendForCurrentUser(
                    this,
                    "Helper Complete With Participant",
                    $"Helper marked participant complete (participantId={participantId}) for session (eventId={eventId}, sessionId={sessionId}).");
            }
            catch { }
        }

        private void MarkParticipantMissing(string eventId, string sessionId, string participantId, string helperId)
        {
            if (string.IsNullOrWhiteSpace(eventId) || string.IsNullOrWhiteSpace(sessionId) || string.IsNullOrWhiteSpace(participantId))
                return;

            // 1) Record missing
            AppendMissingRecord(eventId, sessionId, participantId, helperId);

            // 2) Remove from enrolled, promote waitlist, clear admitted, save
            lock (EnrollmentsLock)
            {
                var doc = LoadEnrollmentsDoc();
                var ses = doc.SelectSingleNode($"/enrollments/session[@eventId='{eventId}' and @id='{sessionId}']") as XmlElement;
                if (ses == null) return;

                var enrolledNode = ses.SelectSingleNode($"enrolled/user[@id='{participantId}']") as XmlElement;
                if (enrolledNode == null) return;

                enrolledNode.ParentNode.RemoveChild(enrolledNode);

                PromoteFirstWaitlistToEnrolled(doc, ses);
                ClearAdmittedFlags(ses);

                SaveEnrollmentsDoc(doc);
            }

            try
            {
                UniversityAuditLogger.AppendForCurrentUser(
                    this,
                    "Helper Mark Participant Missing",
                    $"Helper marked participant missing (participantId={participantId}) for session (eventId={eventId}, sessionId={sessionId}).");
            }
            catch { }
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

        /// <summary>
        /// Best-effort lookup of the microcourse title for a given event/session.
        /// Used only for audit logging; never blocks helper flows.
        /// </summary>
        private string GetCourseTitleForSession(string eventId, string sessionId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(eventId) || string.IsNullOrWhiteSpace(sessionId))
                {
                    return "(session)";
                }

                if (!File.Exists(EventSessionsXmlPath))
                {
                    return "(session)";
                }

                var doc = new XmlDocument();
                doc.Load(EventSessionsXmlPath);

                var ses = doc.SelectSingleNode(
                    $"/eventSessions/session[@eventId='{eventId}' and @id='{sessionId}']") as XmlElement;
                if (ses == null)
                {
                    return "(session)";
                }

                var courseId = (ses["courseId"]?.InnerText ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(courseId))
                {
                    return "(session)";
                }

                if (!File.Exists(MicrocoursesXmlPath))
                {
                    return courseId;
                }

                var mdoc = new XmlDocument();
                mdoc.Load(MicrocoursesXmlPath);

                var courseEl = mdoc.SelectSingleNode(
                    $"/microcourses/course[@id='{courseId}']") as XmlElement;
                var title = courseEl?["title"]?.InnerText;

                title = (title ?? string.Empty).Trim();
                return string.IsNullOrWhiteSpace(title) ? courseId : title;
            }
            catch
            {
                // Audit logging should never break main helper flows.
                return "(session)";
            }
        }




        // ---------- New: helper progress + notes for delivered sessions ----------

        private XmlDocument LoadHelperProgressDoc()
        {
            var doc = new XmlDocument();

            if (File.Exists(HelperProgressXmlPath))
            {
                doc.Load(HelperProgressXmlPath);
                if (doc.DocumentElement == null)
                {
                    var rootMissing = doc.CreateElement("helperProgress");
                    doc.AppendChild(rootMissing);
                }
            }
            else
            {
                var root = doc.CreateElement("helperProgress");
                doc.AppendChild(root);
            }

            return doc;
        }

        private void SaveHelperProgressDoc(XmlDocument doc)
        {
            lock (HelperProgressLock)
            {
                doc.Save(HelperProgressXmlPath);
            }
        }

        private XmlDocument LoadHelperNotesDoc()
        {
            var doc = new XmlDocument();

            if (File.Exists(HelperNotesXmlPath))
            {
                doc.Load(HelperNotesXmlPath);
                if (doc.DocumentElement == null)
                {
                    var rootMissing = doc.CreateElement("helperNotes");
                    doc.AppendChild(rootMissing);
                }
            }
            else
            {
                var root = doc.CreateElement("helperNotes");
                doc.AppendChild(root);
            }

            return doc;
        }

        private void SaveHelperNotesDoc(XmlDocument doc)
        {
            lock (HelperNotesLock)
            {
                doc.Save(HelperNotesXmlPath);
            }
        }

        private static int ParseIntSafe(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return 0;
            }

            int val;
            return int.TryParse(raw.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out val)
                ? val
                : 0;
        }

        /// <summary>
        /// Returns the in-session storage list for delivered-session snapshots.
        /// Each item is a string: helperId|courseId|prevCourseTeaching|prevTotalTeaching|ticks.
        /// </summary>
        private List<string> GetDeliverHistoryStorage()
        {
            var list = Session[DeliverHistorySessionKey] as List<string>;
            if (list == null)
            {
                list = new List<string>();
                Session[DeliverHistorySessionKey] = list;
            }

            return list;
        }

        private void AddDeliverSnapshotToHistory(string snapshot)
        {
            if (string.IsNullOrWhiteSpace(snapshot))
            {
                return;
            }

            var list = GetDeliverHistoryStorage();
            list.Add(snapshot);

            // Keep a modest cap so the session list does not grow without bound.
            if (list.Count > 20)
            {
                list.RemoveAt(0);
            }
        }

        private void RemoveDeliverSnapshotFromHistory(string snapshot)
        {
            if (string.IsNullOrWhiteSpace(snapshot))
            {
                return;
            }

            var list = Session[DeliverHistorySessionKey] as List<string>;
            if (list == null)
            {
                return;
            }

            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (string.Equals(list[i], snapshot, StringComparison.Ordinal))
                {
                    list.RemoveAt(i);
                    break;
                }
            }
        }

        /// <summary>
        /// Increments teachingSessions for the selected course plus totalTeachingSessions
        /// for this helper in helperProgress.xml, and records an undo snapshot.
        /// </summary>
        private bool TryIncrementDeliveredSession(string helperId, string courseId, out string courseTitle)
        {
            courseTitle = null;

            if (string.IsNullOrWhiteSpace(helperId) || string.IsNullOrWhiteSpace(courseId))
            {
                return false;
            }

            var doc = LoadHelperProgressDoc();
            var helperNode = doc.SelectSingleNode($"//helper[@id='{helperId}']") as XmlElement;
            if (helperNode == null)
            {
                return false;
            }

            var courseNode = helperNode.SelectSingleNode($"course[@id='{courseId}']") as XmlElement;
            if (courseNode == null)
            {
                return false;
            }

            courseTitle = (courseNode["title"]?.InnerText ?? "").Trim();

            int prevCourseTeaching = ParseIntSafe(courseNode["teachingSessions"]?.InnerText);

            var totalsNode = helperNode.SelectSingleNode("totals") as XmlElement;
            int prevTotalTeaching = 0;
            if (totalsNode != null)
            {
                prevTotalTeaching = ParseIntSafe(totalsNode["totalTeachingSessions"]?.InnerText);
            }

            int newCourseTeaching = prevCourseTeaching + 1;
            int newTotalTeaching = prevTotalTeaching + 1;

            var teachingEl = courseNode["teachingSessions"];
            if (teachingEl == null)
            {
                teachingEl = doc.CreateElement("teachingSessions");
                courseNode.AppendChild(teachingEl);
            }
            teachingEl.InnerText = newCourseTeaching.ToString(CultureInfo.InvariantCulture);

            if (totalsNode == null)
            {
                totalsNode = doc.CreateElement("totals");
                helperNode.AppendChild(totalsNode);
            }

            var totalTeachEl = totalsNode["totalTeachingSessions"];
            if (totalTeachEl == null)
            {
                totalTeachEl = doc.CreateElement("totalTeachingSessions");
                totalsNode.AppendChild(totalTeachEl);
            }
            totalTeachEl.InnerText = newTotalTeaching.ToString(CultureInfo.InvariantCulture);

            SaveHelperProgressDoc(doc);

            // Store undo snapshot in session: helperId|courseId|prevCourseTeaching|prevTotalTeaching|ticks
            var nowTicks = DateTime.UtcNow.Ticks;
            var snapshot = string.Join("|",
                helperId ?? string.Empty,
                courseId ?? string.Empty,
                prevCourseTeaching.ToString(CultureInfo.InvariantCulture),
                prevTotalTeaching.ToString(CultureInfo.InvariantCulture),
                nowTicks.ToString(CultureInfo.InvariantCulture));

            AddDeliverSnapshotToHistory(snapshot);

            return true;
        }

        /// <summary>
        /// Rolls back a delivered-session increment for this helper based on a snapshot string,
        /// if within the undo window.
        /// </summary>
        private bool TryUndoDeliveredSession(string helperId, string snapshot, out string courseTitle)
        {
            courseTitle = null;

            if (string.IsNullOrWhiteSpace(helperId) || string.IsNullOrWhiteSpace(snapshot))
            {
                return false;
            }

            var parts = snapshot.Split('|');
            if (parts.Length != 5)
            {
                return false;
            }

            var snapHelperId = parts[0];
            var courseId = parts[1];

            if (!string.Equals(helperId, snapHelperId, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (!int.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out var prevCourseTeaching))
            {
                return false;
            }

            if (!int.TryParse(parts[3], NumberStyles.Integer, CultureInfo.InvariantCulture, out var prevTotalTeaching))
            {
                return false;
            }

            if (!long.TryParse(parts[4], NumberStyles.Integer, CultureInfo.InvariantCulture, out var ticks))
            {
                return false;
            }

            var savedUtc = new DateTime(ticks, DateTimeKind.Utc);
            var age = DateTime.UtcNow - savedUtc;
            if (age > DeliverUndoWindow)
            {
                // Undo window expired.
                return false;
            }

            var doc = LoadHelperProgressDoc();
            var helperNode = doc.SelectSingleNode($"//helper[@id='{helperId}']") as XmlElement;
            if (helperNode == null)
            {
                return false;
            }

            var courseNode = helperNode.SelectSingleNode($"course[@id='{courseId}']") as XmlElement;
            if (courseNode == null)
            {
                return false;
            }

            courseTitle = (courseNode["title"]?.InnerText ?? "").Trim();

            var teachingEl = courseNode["teachingSessions"];
            if (teachingEl == null)
            {
                teachingEl = doc.CreateElement("teachingSessions");
                courseNode.AppendChild(teachingEl);
            }
            teachingEl.InnerText = prevCourseTeaching.ToString(CultureInfo.InvariantCulture);

            var totalsNode = helperNode.SelectSingleNode("totals") as XmlElement;
            if (totalsNode == null)
            {
                totalsNode = doc.CreateElement("totals");
                helperNode.AppendChild(totalsNode);
            }

            var totalTeachEl = totalsNode["totalTeachingSessions"];
            if (totalTeachEl == null)
            {
                totalTeachEl = doc.CreateElement("totalTeachingSessions");
                totalsNode.AppendChild(totalTeachEl);
            }
            totalTeachEl.InnerText = prevTotalTeaching.ToString(CultureInfo.InvariantCulture);

            SaveHelperProgressDoc(doc);

            return true;
        }

        /// <summary>
        /// Append a helper note for this delivered session into helperNotes.xml.
        /// </summary>
        private void AppendHelperNote(string helperId, string courseId, string courseTitle, string notes)
        {
            var doc = LoadHelperNotesDoc();
            var root = doc.DocumentElement;
            if (root == null)
            {
                root = doc.CreateElement("helperNotes");
                doc.AppendChild(root);
            }

            var noteEl = doc.CreateElement("note");
            noteEl.SetAttribute("helperId", helperId ?? string.Empty);
            noteEl.SetAttribute("courseId", courseId ?? string.Empty);
            noteEl.SetAttribute("courseTitle", courseTitle ?? string.Empty);
            noteEl.SetAttribute("tsUtc", DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));

            var textEl = doc.CreateElement("text");
            textEl.InnerText = notes ?? string.Empty;
            noteEl.AppendChild(textEl);

            root.AppendChild(noteEl);

            SaveHelperNotesDoc(doc);
        }

        // ---------- New: event handlers for mark-delivered + undo ----------

        protected void DeliverSubmitButton_Click(object sender, EventArgs e)
        {
            if (!EnsureHelperRole(out var helperId))
            {
                return;
            }

            var selectedCourseId = DeliverCourseDropDown.SelectedValue;
            if (string.IsNullOrWhiteSpace(selectedCourseId))
            {
                ClientScript.RegisterStartupScript(
                    GetType(),
                    "DeliverValidation",
                    "showFiaToast('Please choose a course before logging a delivered session.');",
                    true);
                return;
            }

            string courseTitle;
            if (!TryIncrementDeliveredSession(helperId, selectedCourseId, out courseTitle))
            {
                ClientScript.RegisterStartupScript(
                    GetType(),
                    "DeliverError",
                    "showFiaToast('We could not log this delivered session. Please try again.');",
                    true);
                return;
            }

            var notesText = DeliverNotesTextBox.Text ?? string.Empty;
            try
            {
                AppendHelperNote(helperId, selectedCourseId, courseTitle, notesText);
            }
            catch
            {
                // Notes are best-effort; do not block certification progress.
            }

            try
            {
                var details = string.Format(
                    "Helper logged a delivered teaching session for \"{0}\" (courseId={1}). A private helper note was also saved for certification review.",
                    courseTitle,
                    selectedCourseId);

                UniversityAuditLogger.AppendForCurrentUser(
                    this,
                    "Helper Delivered Session",
                    details);
            }
            catch
            {
                // Never block helper progress or notes if audit logging fails.
            }


            // Clear notes box after successful log.
            DeliverNotesTextBox.Text = string.Empty;

            // Refresh dropdown options and recent-history panel after progress change.
            try
            {
                BindDeliverableCourses(helperId);
            }
            catch
            {
                // ignore
            }

            try
            {
                BindDeliverHistory(helperId);
            }
            catch
            {
                // ignore
            }

            ClientScript.RegisterStartupScript(
                GetType(),
                "DeliverLogged",
                "showFiaToast('Session marked as delivered.');",
                true);
        }

        protected void DeliverHistoryRepeater_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (!string.Equals(e.CommandName, "undoDeliver", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (!EnsureHelperRole(out var helperId))
            {
                return;
            }

            var snapshot = Convert.ToString(e.CommandArgument ?? string.Empty);
            if (string.IsNullOrWhiteSpace(snapshot))
            {
                return;
            }

            if (TryUndoDeliveredSession(helperId, snapshot, out var courseTitle))
            {
                // Remove from in-memory history and refresh panels.
                RemoveDeliverSnapshotFromHistory(snapshot);

                try
                {
                    BindDeliverableCourses(helperId);
                }
                catch
                {
                    // ignore
                }

                try
                {
                    BindDeliverHistory(helperId);
                }
                catch
                {
                    // ignore
                }

                try
                {
                    var details = string.Format(
                        "Helper undid a delivered teaching session log for \"{0}\".",
                        courseTitle);

                    UniversityAuditLogger.AppendForCurrentUser(
                        this,
                        "Helper Delivered Session (Undo)",
                        details);
                }
                catch
                {
                }


                ClientScript.RegisterStartupScript(
                    GetType(),
                    "DeliverUndoOk",
                    "showFiaToast('Your delivered session log has been undone.');",
                    true);
            }
            else
            {
                ClientScript.RegisterStartupScript(
                    GetType(),
                    "DeliverUndoFail",
                    "showFiaToast('Undo is no longer available for that session log.');",
                    true);
            }
        }
    }
}

