﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;

namespace CyberApp_FIA.Account
{
    /// <summary>
    /// EventManage page for a University Admin to:
    /// - View event header (name, university, status, date)
    /// - Toggle which microcourses are enabled for this event (#85)
    /// - Add scheduled sessions for the event (#68/#69) with helper double-booking prevention
    /// - List all sessions for this event
    /// Uses multiple XML files in App_Data as lightweight datastores.
    /// </summary>
    public partial class EventManage : Page
    {
        // --- XML datastore paths (resolved to physical paths under App_Data) ---
        private string EventsXmlPath => Server.MapPath("~/App_Data/events.xml");
        private string MicrocoursesXmlPath => Server.MapPath("~/App_Data/microcourses.xml");
        private string EventCoursesXmlPath => Server.MapPath("~/App_Data/eventCourses.xml");   // #85 switches (per-event microcourse enablement)
        private string EventSessionsXmlPath => Server.MapPath("~/App_Data/eventSessions.xml");  // #68/#69 sessions (per-event session list)

        // Holds the current event id from the query string for use within the page lifecycle.
        private string _eventId;

        /// <summary>
        /// Auth gate (UniversityAdmin only), resolves _eventId from query string, and on first load:
        /// - loads event header
        /// - binds list of published microcourses and their per-event enable switches
        /// - binds course select dropdown for scheduling
        /// - binds existing sessions for the event
        /// </summary>
        protected void Page_Load(object sender, EventArgs e)
        {
            // ---- Access Gate: University Admins only ----
            var role = (string)Session["Role"];
            if (!string.Equals(role, "UniversityAdmin", StringComparison.OrdinalIgnoreCase))
            {
                Response.Redirect("~/Account/Login.aspx");
                return;
            }

            // Require an event id in the query string; otherwise return to the UA home.
            _eventId = Request.QueryString["id"] ?? "";
            if (string.IsNullOrEmpty(_eventId))
            {
                Response.Redirect("~/Account/UniversityAdmin/UniversityAdminHome.aspx");
                return;
            }

            // Initial load only; preserve state across postbacks.
            if (!IsPostBack)
            {
                LoadEventHeader();
                BindMicrocourses();
                BindCourseSelect();
                BindSessions();
            }
        }

        /// <summary>
        /// Loads the event header info from events.xml and displays:
        /// - EventName, University, EventStatus
        /// - EventDate as local yyyy-MM-dd (ISO input-friendly)
        /// Redirects to UA home if the event is not found.
        /// </summary>
        private void LoadEventHeader()
        {
            if (!File.Exists(EventsXmlPath)) return;

            var doc = new XmlDocument(); doc.Load(EventsXmlPath);
            var ev = (XmlElement)doc.SelectSingleNode($"/events/event[@id='{_eventId}']");
            if (ev == null)
            {
                // If event id is invalid or missing, bail out to the UA home page.
                Response.Redirect("~/Account/UniversityAdmin/UniversityAdminHome.aspx");
                return;
            }

            EventName.Text = ev["name"]?.InnerText ?? "(unnamed)";
            University.Text = ev["university"]?.InnerText ?? "";
            EventStatus.Text = ev.GetAttribute("status");

            // Convert stored ISO datetime to a local short date for display.
            var date = ev["date"]?.InnerText ?? "";
            if (DateTime.TryParse(date, out var dt))
                EventDate.Text = dt.ToLocalTime().ToString("yyyy-MM-dd");
            else
                EventDate.Text = "(unset)";
        }

        // =========================
        //  #86: list Published microcourses
        //  #85: per-event enable/disable switches
        // =========================

        /// <summary>
        /// Binds the repeater of published microcourses with a checkbox reflecting whether
        /// each course is enabled for this event. Uses eventCourses.xml for per-event switches.
        /// </summary>
        private void BindMicrocourses()
        {
            var rows = new List<object>();

            // If no microcourses are defined, show empty state.
            if (!File.Exists(MicrocoursesXmlPath))
            {
                NoCoursesPH.Visible = true;
                CoursesRepeater.DataSource = rows; CoursesRepeater.DataBind();
                return;
            }

            // Build a set of course IDs that are enabled for this event.
            var enabledByCourse = LoadEventCourseSwitches();

            // Read all published microcourses and project fields for the UI.
            var doc = new XmlDocument(); doc.Load(MicrocoursesXmlPath);
            var nodes = doc.SelectNodes("/microcourses/course[@status='Published']");
            foreach (XmlElement c in nodes)
            {
                var id = c.GetAttribute("id");
                var title = c["title"]?.InnerText ?? "(untitled)";
                var duration = c["duration"]?.InnerText ?? "";

                // Collect <tags><tag>...</tag></tags> into a comma-separated string.
                var tagsNode = c["tags"];
                var tags = "";
                if (tagsNode != null && tagsNode.HasChildNodes)
                {
                    var list = new List<string>();
                    foreach (XmlElement t in tagsNode.SelectNodes("tag")) list.Add(t.InnerText);
                    tags = string.Join(", ", list);
                }

                // Enabled if present in the per-event set.
                var enabled = enabledByCourse.Contains(id);
                rows.Add(new { id, title, duration, tags, enabled });
            }

            // Bind rows to the repeater and toggle the empty-state placeholder.
            NoCoursesPH.Visible = rows.Count == 0;
            CoursesRepeater.DataSource = rows;
            CoursesRepeater.DataBind();
        }

        /// <summary>
        /// Returns the set of course IDs marked enabled="true" for this event in eventCourses.xml.
        /// Missing file yields an empty set.
        /// </summary>
        private HashSet<string> LoadEventCourseSwitches()
        {
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (!File.Exists(EventCoursesXmlPath)) return set;

            var doc = new XmlDocument(); doc.Load(EventCoursesXmlPath);
            var nodes = doc.SelectNodes($"/eventCourses/event[@id='{_eventId}']/course[@enabled='true']");
            foreach (XmlElement c in nodes) set.Add(c.GetAttribute("id"));
            return set;
        }

        /// <summary>
        /// Saves the per-event microcourse switches:
        /// - Ensures eventCourses.xml exists
        /// - Overwrites the <event id="..."> child set with current repeater state
        /// </summary>
        protected void BtnSaveSwitches_Click(object sender, EventArgs e)
        {
            EnsureEventCoursesXml();

            var doc = new XmlDocument(); doc.Load(EventCoursesXmlPath);

            // Find or create the <event id="..."> node for this event.
            var evNode = doc.SelectSingleNode($"/eventCourses/event[@id='{_eventId}']") as XmlElement;
            if (evNode == null)
            {
                var root = doc.DocumentElement ?? doc.AppendChild(doc.CreateElement("eventCourses")) as XmlElement;
                evNode = doc.CreateElement("event");
                evNode.SetAttribute("id", _eventId);
                root.AppendChild(evNode);
            }
            else
            {
                // Clear existing <course> children to rewrite fresh from the UI state.
                while (evNode.HasChildNodes) evNode.RemoveChild(evNode.FirstChild);
            }

            // Walk repeater rows and write <course id=.. enabled=..> elements.
            foreach (RepeaterItem item in CoursesRepeater.Items)
            {
                var enabled = (item.FindControl("Enabled") as CheckBox)?.Checked ?? false;
                var id = (item.FindControl("CourseId") as HiddenField)?.Value;
                if (string.IsNullOrEmpty(id)) continue;

                var c = doc.CreateElement("course");
                c.SetAttribute("id", id);
                c.SetAttribute("enabled", enabled ? "true" : "false");
                evNode.AppendChild(c);
            }

            doc.Save(EventCoursesXmlPath);
        }

        /// <summary>
        /// Ensures eventCourses.xml exists with a root <eventCourses>.
        /// Creates directory structure as needed.
        /// </summary>
        private void EnsureEventCoursesXml()
        {
            if (File.Exists(EventCoursesXmlPath)) return;
            Directory.CreateDirectory(Path.GetDirectoryName(EventCoursesXmlPath));
            var init = "<?xml version='1.0' encoding='utf-8'?><eventCourses></eventCourses>";
            File.WriteAllText(EventCoursesXmlPath, init);
        }

        // =========================
        //  Scheduling UI (sessions)
        // =========================

        /// <summary>
        /// Populates the course dropdown with Published microcourses (id=Value, title=Text).
        /// </summary>
        private void BindCourseSelect()
        {
            CourseSelect.Items.Clear();
            if (!File.Exists(MicrocoursesXmlPath)) return;

            var doc = new XmlDocument(); doc.Load(MicrocoursesXmlPath);
            var nodes = doc.SelectNodes("/microcourses/course[@status='Published']");
            foreach (XmlElement c in nodes)
            {
                var id = c.GetAttribute("id");
                var title = c["title"]?.InnerText ?? "(untitled)";
                CourseSelect.Items.Add(new ListItem(title, id));
            }
        }

        /// <summary>
        /// Adds a session for this event:
        /// - Validates course pick, start/end times, and helper (required to prevent double-booking)
        /// - Converts user-entered local datetimes to UTC ISO for storage
        /// - Checks for helper overlaps against existing sessions for the same event
        /// - Saves the new <session> to eventSessions.xml and refreshes the list
        /// </summary>
        protected void BtnAddSession_Click(object sender, EventArgs e)
        {
            ScheduleMessage.Text = "";

            // Course selection is mandatory.
            var courseId = CourseSelect.SelectedValue;
            if (string.IsNullOrWhiteSpace(courseId))
            {
                ScheduleMessage.Text = "<span class='err'>Pick a course.</span>";
                return;
            }

            // Parse local start/end into UTC datetimes for consistent storage/comparison.
            if (!TryParseLocalToUtc(SessionDateTimeStart.Text, out var startUtc) ||
                !TryParseLocalToUtc(SessionDateTimeEnd.Text, out var endUtc))
            {
                ScheduleMessage.Text = "<span class='err'>Enter valid start and end times.</span>";
                return;
            }
            if (endUtc <= startUtc)
            {
                ScheduleMessage.Text = "<span class='err'>End time must be after start.</span>";
                return;
            }

            // Helper is required because we enforce "same helper cannot overlap" policy.
            var helper = (Helper.Text ?? "").Trim();
            if (string.IsNullOrWhiteSpace(helper))
            {
                ScheduleMessage.Text = "<span class='err'>Helper is required to prevent double-booking.</span>";
                return;
            }

            // Optional fields
            var room = (Room.Text ?? "").Trim();
            int.TryParse(Capacity.Text, out var cap);
            if (cap < 0) cap = 0;

            EnsureEventSessionsXml();

            // Load all existing sessions for this event to check conflicts.
            var doc = new XmlDocument(); doc.Load(EventSessionsXmlPath);
            var exist = doc.SelectNodes($"/eventSessions/session[@eventId='{_eventId}']");

            // ---- Conflict check: block if the SAME helper overlaps with an existing session ----
            foreach (XmlElement s in exist)
            {
                var sHelper = (s["helper"]?.InnerText ?? "").Trim(); // normalize stored value
                if (!string.Equals(helper, sHelper, StringComparison.OrdinalIgnoreCase)) continue;

                var sStart = s["start"]?.InnerText ?? "";
                var sEnd = s["end"]?.InnerText ?? "";
                if (!TryParseIsoUtc(sStart, out var sStartUtc) ||
                    !TryParseIsoUtc(sEnd, out var sEndUtc))
                    continue; // skip malformed rows silently

                if (IntervalsOverlap(startUtc, endUtc, sStartUtc, sEndUtc))
                {
                    // Build a friendly local-time window for the error.
                    ScheduleMessage.Text =
                        $"<span class='err'>Helper <strong>{Server.HtmlEncode(helper)}</strong> is already booked " +
                        $"from {sStartUtc.ToLocalTime():yyyy-MM-dd HH:mm} to {sEndUtc.ToLocalTime():HH:mm}.</span>";
                    return;
                }
            }

            // No conflicts → append the new <session> to the document.
            var root = doc.DocumentElement ?? doc.AppendChild(doc.CreateElement("eventSessions")) as XmlElement;

            var node = doc.CreateElement("session");
            node.SetAttribute("eventId", _eventId);
            node.SetAttribute("id", Guid.NewGuid().ToString("N"));
            node.AppendChild(Mk(doc, "courseId", courseId));
            node.AppendChild(Mk(doc, "start", startUtc.ToString("o"))); // ISO UTC
            node.AppendChild(Mk(doc, "end", endUtc.ToString("o")));     // ISO UTC
            node.AppendChild(Mk(doc, "room", room));
            node.AppendChild(Mk(doc, "helper", helper));
            node.AppendChild(Mk(doc, "capacity", cap > 0 ? cap.ToString() : ""));

            root.AppendChild(node);
            doc.Save(EventSessionsXmlPath);

            // Refresh list and clear form inputs (keep course dropdown as-is).
            BindSessions();
            SessionDateTimeStart.Text = SessionDateTimeEnd.Text = Room.Text = Helper.Text = Capacity.Text = "";
            ScheduleMessage.Text = "<span class='ok'>Session added.</span>";
        }

        /// <summary>
        /// Ensures eventSessions.xml exists with a root <eventSessions>.
        /// </summary>
        private void EnsureEventSessionsXml()
        {
            if (File.Exists(EventSessionsXmlPath)) return;
            Directory.CreateDirectory(Path.GetDirectoryName(EventSessionsXmlPath));
            var init = "<?xml version='1.0' encoding='utf-8'?><eventSessions></eventSessions>";
            File.WriteAllText(EventSessionsXmlPath, init);
        }

        /// <summary>
        /// Binds the sessions list for this event:
        /// - Resolves courseId to courseTitle (from microcourses.xml)
        /// - Converts stored ISO UTC to local human-readable times
        /// - Displays room/helper/capacity (— when capacity not set)
        /// </summary>
        private void BindSessions()
        {
            var rows = new List<object>();

            // If there are no sessions stored yet, show an empty state.
            if (!File.Exists(EventSessionsXmlPath))
            {
                NoSessionsPH.Visible = true;
                SessionsRepeater.DataSource = rows;
                SessionsRepeater.DataBind();
                return;
            }

            // Build a lookup map courseId -> title (best-effort; missing titles fall back to "(untitled)").
            var titles = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (File.Exists(MicrocoursesXmlPath))
            {
                var md = new XmlDocument(); md.Load(MicrocoursesXmlPath);
                foreach (XmlElement c in md.SelectNodes("/microcourses/course"))
                {
                    titles[c.GetAttribute("id")] = c["title"]?.InnerText ?? "(untitled)";
                }
            }

            // Read sessions for this event and project rows for the repeater.
            var doc = new XmlDocument(); doc.Load(EventSessionsXmlPath);
            foreach (XmlElement s in doc.SelectNodes($"/eventSessions/session[@eventId='{_eventId}']"))
            {
                var courseId = s["courseId"]?.InnerText ?? "";
                var startIso = s["start"]?.InnerText ?? "";
                var endIso = s["end"]?.InnerText ?? "";
                var room = s["room"]?.InnerText ?? "";
                var helper = (s["helper"]?.InnerText ?? "").Trim();
                var cap = s["capacity"]?.InnerText ?? "";

                string startLocal = ToLocalHuman(startIso);
                string endLocal = ToLocalHuman(endIso);

                titles.TryGetValue(courseId, out var title);

                rows.Add(new
                {
                    courseTitle = title ?? "(untitled)",
                    startLocal,
                    endLocal,
                    room,
                    helper,
                    capacity = string.IsNullOrEmpty(cap) ? "—" : cap
                });
            }

            // Bind results and toggle empty-state placeholder.
            NoSessionsPH.Visible = rows.Count == 0;
            SessionsRepeater.DataSource = rows;
            SessionsRepeater.DataBind();
        }

        // =========================
        //  Utility helpers
        // =========================

        /// <summary>
        /// Utility: create an element with text content (null-safe to empty string).
        /// </summary>
        private static XmlElement Mk(XmlDocument d, string name, string val)
        {
            var el = d.CreateElement(name);
            el.InnerText = val ?? "";
            return el;
        }

        /// <summary>
        /// Parses a user-entered local datetime string into UTC.
        /// Tries current culture first, then invariant, assuming local zone on success.
        /// </summary>
        private static bool TryParseLocalToUtc(string input, out DateTime utc)
        {
            utc = default;
            if (DateTime.TryParse(input, CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal, out var local) ||
                DateTime.TryParse(input, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out local))
            {
                utc = local.ToUniversalTime();
                return true;
            }
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
        /// Half-open interval overlap test for UTC times:
        /// [aStart, aEnd) overlaps [bStart, bEnd) if aStart < bEnd AND bStart < aEnd.
        /// </summary>
        private static bool IntervalsOverlap(DateTime aStartUtc, DateTime aEndUtc, DateTime bStartUtc, DateTime bEndUtc)
            => aStartUtc < bEndUtc && bStartUtc < aEndUtc;

        /// <summary>
        /// Converts an ISO UTC timestamp to a local human string "yyyy-MM-dd HH:mm",
        /// or "(unset)" if parsing fails.
        /// </summary>
        private static string ToLocalHuman(string iso)
        {
            if (TryParseIsoUtc(iso, out var dt))
                return dt.ToLocalTime().ToString("yyyy-MM-dd HH:mm");
            return "(unset)";
        }
    }
}


