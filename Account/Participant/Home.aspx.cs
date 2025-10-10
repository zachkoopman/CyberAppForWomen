using System;
using System.Collections.Generic;
using System.IO;
using System.Web.UI;
using System.Xml;

namespace CyberApp_FIA.Participant
{
    public partial class Home : Page
    {
        private string EventsXmlPath => Server.MapPath("~/App_Data/events.xml");
        private string MicrocoursesXmlPath => Server.MapPath("~/App_Data/microcourses.xml");
        private string EventCoursesXmlPath => Server.MapPath("~/App_Data/eventCourses.xml");
        private string EventSessionsXmlPath => Server.MapPath("~/App_Data/eventSessions.xml");

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

                var eventId = (string)Session["EventId"];
                if (string.IsNullOrWhiteSpace(eventId))
                {
                    Response.Redirect("~/Account/Participant/SelectEvent.aspx");
                    return;
                }

                LoadEventHeader(eventId);
                BindCourses(eventId);
            }
        }

        private void LoadEventHeader(string eventId)
        {
            var uni = (string)Session["University"] ?? "";
            University.Text = string.IsNullOrWhiteSpace(uni) ? "(not set)" : uni;

            if (!File.Exists(EventsXmlPath)) { EventName.Text = "(unknown)"; return; }
            var doc = new XmlDocument(); doc.Load(EventsXmlPath);
            var ev = (XmlElement)doc.SelectSingleNode($"/events/event[@id='{eventId}']");
            EventName.Text = ev?["name"]?.InnerText ?? "(unknown)";
        }

        private void BindCourses(string eventId)
        {
            var visibleIds = LoadEventVisibleCourseIds(eventId);
            var sessionsByCourse = LoadUpcomingSessions(eventId); // courseId -> list of session rows (HTML)
            var rows = new List<object>();

            if (visibleIds.Count == 0 || !File.Exists(MicrocoursesXmlPath))
            {
                EmptyPH.Visible = true;
                CoursesRepeater.DataSource = rows;
                CoursesRepeater.DataBind();
                return;
            }

            var doc = new XmlDocument(); doc.Load(MicrocoursesXmlPath);
            foreach (XmlElement c in doc.SelectNodes("/microcourses/course[@status='Published']"))
            {
                var id = c.GetAttribute("id");
                if (!visibleIds.Contains(id)) continue;

                var title = c["title"]?.InnerText ?? "(untitled)";
                var summary = c["summary"]?.InnerText ?? "";
                var duration = c["duration"]?.InnerText ?? "";

                // tags
                string tags = "";
                var tnode = c["tags"];
                if (tnode != null && tnode.HasChildNodes)
                {
                    var list = new List<string>();
                    foreach (XmlElement t in tnode.SelectNodes("tag")) list.Add(t.InnerText);
                    tags = list.Count > 0 ? "Tags: " + string.Join(", ", list) : "";
                }

                // nice little table (or empty message)
                var sessionsHtml = sessionsByCourse.TryGetValue(id, out var html)
                    ? html
                    : "<em>No upcoming sessions yet.</em>";

                rows.Add(new { title, summary, duration, tags, sessionsHtml });
            }

            EmptyPH.Visible = rows.Count == 0;
            CoursesRepeater.DataSource = rows;
            CoursesRepeater.DataBind();
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

        /// <summary>
        /// Returns a map of courseId -> prebuilt HTML table of upcoming/active sessions in local time.
        /// </summary>
        private Dictionary<string, string> LoadUpcomingSessions(string eventId)
        {
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (!File.Exists(EventSessionsXmlPath)) return map;

            var nowUtc = DateTime.UtcNow;
            var doc = new XmlDocument(); doc.Load(EventSessionsXmlPath);

            // Collect sessions grouped by courseId
            var tmp = new Dictionary<string, List<(DateTime start, DateTime end, string room, string helper, string cap)>>(StringComparer.OrdinalIgnoreCase);

            foreach (XmlElement s in doc.SelectNodes($"/eventSessions/session[@eventId='{eventId}']"))
            {
                var courseId = s["courseId"]?.InnerText ?? "";
                if (string.IsNullOrEmpty(courseId)) continue;

                var startIso = s["start"]?.InnerText ?? "";
                var endIso = s["end"]?.InnerText ?? "";

                if (!DateTime.TryParse(endIso, out var endDt) || endDt < nowUtc) continue; // past session
                DateTime.TryParse(startIso, out var startDt);

                var room = s["room"]?.InnerText ?? "";
                var helper = s["helper"]?.InnerText ?? "";
                var cap = s["capacity"]?.InnerText ?? "";

                if (!tmp.TryGetValue(courseId, out var list))
                {
                    list = new List<(DateTime, DateTime, string, string, string)>();
                    tmp[courseId] = list;
                }
                list.Add((startDt, endDt, room, helper, cap));
            }

            // Build compact HTML table for each course
            foreach (var kv in tmp)
            {
                kv.Value.Sort((a, b) => a.start.CompareTo(b.start));
                var html = @"<table class='sess'>
<thead><tr><th>Start</th><th>End</th><th>Room</th><th>Helper</th><th>Capacity</th></tr></thead><tbody>";
                foreach (var row in kv.Value)
                {
                    html += $"<tr>" +
                            $"<td>{row.start.ToLocalTime():yyyy-MM-dd HH:mm}</td>" +
                            $"<td>{row.end.ToLocalTime():yyyy-MM-dd HH:mm}</td>" +
                            $"<td>{(string.IsNullOrWhiteSpace(row.room) ? "—" : row.room)}</td>" +
                            $"<td>{(string.IsNullOrWhiteSpace(row.helper) ? "—" : row.helper)}</td>" +
                            $"<td>{(string.IsNullOrWhiteSpace(row.cap) ? "—" : row.cap)}</td>" +
                            $"</tr>";
                }
                html += "</tbody></table>";
                map[kv.Key] = html;
            }

            return map;
        }
    }
}
