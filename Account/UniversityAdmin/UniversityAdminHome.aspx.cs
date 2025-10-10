using System;
using System.Globalization;
using System.IO;
using System.Web.UI;
using System.Xml;
using System.Collections.Generic;

namespace CyberApp_FIA.Account
{
    /// <summary>
    /// University Admin home/dashboard.
    /// - Gates access to users with the "UniversityAdmin" role.
    /// - Displays the admin's university and lists that university's events.
    /// - Allows creating new events stored in ~/App_Data/events.xml.
    /// </summary>
    public partial class UniversityAdminHome : Page
    {
        /// <summary>
        /// Physical path to the events XML datastore (per-app, non-public App_Data folder).
        /// </summary>
        private string EventsXmlPath => Server.MapPath("~/App_Data/events.xml");

        /// <summary>
        /// Physical path to the users XML datastore (used to look up a user's university).
        /// </summary>
        private string UsersXmlPath => Server.MapPath("~/App_Data/users.xml");

        /// <summary>
        /// Initial page load: authorize role, resolve university, populate UI, bind events list.
        /// </summary>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // ---- Access gate: only University Admins allowed on this page ----
                var role = (string)Session["Role"];
                if (!string.Equals(role, "UniversityAdmin", StringComparison.OrdinalIgnoreCase))
                {
                    // If not authorized, bounce to Login.
                    Response.Redirect("~/Account/Login.aspx");
                    return;
                }

                // Show a friendly greeting: use email if available, else a generic label.
                var email = (string)Session["Email"] ?? "";
                WelcomeName.Text = email.Length > 0 ? email : "University Admin";

                // Determine university to operate under:
                // 1) prefer value from session (set at login),
                // 2) else try users.xml lookup by email.
                var uni = (string)Session["University"];
                if (string.IsNullOrWhiteSpace(uni))
                {
                    uni = LookupUniversityByEmail(email);
                }

                // Display the university and keep a hidden field for consistent saves.
                UniversityDisplay.Text = string.IsNullOrWhiteSpace(uni) ? "(not set)" : uni;
                UniversityValue.Value = uni; // fixed value used when saving the event

                // Populate the event list for this university.
                BindEventsForUniversity(uni);
            }
        }

        /// <summary>
        /// Logs the user out by clearing the session and returning to the welcome page.
        /// </summary>
        protected void BtnLogout_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Response.Redirect("~/Welcome_Page.aspx");
        }

        /// <summary>
        /// Creates a new event for the admin's university:
        /// - Validates inputs
        /// - Ensures events.xml exists
        /// - Appends a new <event> with metadata, description, and date (ISO 8601 UTC midnight)
        /// - Rebinds the event list
        /// </summary>
        protected void BtnCreateEvent_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            // Read current university (hidden, predefined), and form inputs.
            var uni = (UniversityValue.Value ?? "").Trim();    // predefined, required
            var name = (EventName.Text ?? "").Trim();
            var desc = (Description.Text ?? "").Trim();
            var dateInput = (EventDate.Text ?? "").Trim();

            // University must be known (this page is scoped to a specific institution).
            if (string.IsNullOrEmpty(uni))
            {
                FormMessage.Text = "<span style='color:#c21d1d'>Your university is not set. Ask a Super Admin to add it to your account.</span>";
                return;
            }

            // Parse date from the HTML5 date input; persist as ISO 8601 UTC midnight.
            if (!TryParseDateToIso(dateInput, out var eventDateIso))
            {
                FormMessage.Text = "<span style='color:#c21d1d'>Please enter a valid date.</span>";
                return;
            }

            // Make sure the events XML file exists and has a root container.
            EnsureEventsXml();

            // Create and append the <event> node.
            var doc = new XmlDocument();
            doc.Load(EventsXmlPath);

            var ev = doc.CreateElement("event");
            ev.SetAttribute("id", Guid.NewGuid().ToString("N"));        // Compact unique id
            ev.SetAttribute("status", "Draft");                         // Future: Published, Archived, etc.
            ev.SetAttribute("createdAt", DateTime.UtcNow.ToString("o")); // Audit timestamp (UTC ISO 8601)
            ev.SetAttribute("createdBy", (Session["Email"] as string) ?? "universityadmin@unknown"); // Audit actor

            // Child nodes: university scoping, name, date (ISO string), and description.
            ev.AppendChild(Mk(doc, "university", uni));
            ev.AppendChild(Mk(doc, "name", name));
            ev.AppendChild(Mk(doc, "date", eventDateIso));     // ISO 8601 UTC midnight
            ev.AppendChild(Mk(doc, "description", desc));

            // Placeholder container for future features (attaching course IDs, microcourses, etc.).
            ev.AppendChild(doc.CreateElement("courses"));

            // Persist to disk.
            doc.DocumentElement.AppendChild(ev);
            doc.Save(EventsXmlPath);

            // Inform user, clear input fields (but keep the fixed university), and refresh the list.
            FormMessage.Text = "<span style='color:#0a7a3c'>Cyberfair event created.</span>";
            ClearForm(); // leave university alone (it's fixed)
            BindEventsForUniversity(uni);
        }

        /// <summary>
        /// Clears form fields for event creation (university remains unchanged).
        /// </summary>
        protected void BtnClear_Click(object sender, EventArgs e)
        {
            ClearForm();
            FormMessage.Text = "";
        }

        // ---------- Helpers ----------

        /// <summary>
        /// Ensures events.xml exists in App_Data with a versioned <events> root element.
        /// Creates directories as needed.
        /// </summary>
        private void EnsureEventsXml()
        {
            if (File.Exists(EventsXmlPath)) return;
            Directory.CreateDirectory(Path.GetDirectoryName(EventsXmlPath));
            var init = "<?xml version='1.0' encoding='utf-8'?><events version='1'></events>";
            File.WriteAllText(EventsXmlPath, init);
        }

        /// <summary>
        /// Binds the repeater with this university's events.
        /// - Reads events.xml
        /// - Filters by <university> exact match (case-insensitive)
        /// - Projects minimal fields for UI consumption (id, name, status, date, manageUrl)
        /// </summary>
        private void BindEventsForUniversity(string uni)
        {
            var rows = new List<object>();

            // If the store doesn't exist, show an empty state.
            if (!File.Exists(EventsXmlPath))
            {
                NoEventsPlaceholder.Visible = true;
                EventsRepeater.DataSource = rows;
                EventsRepeater.DataBind();
                return;
            }

            var doc = new XmlDocument();
            doc.Load(EventsXmlPath);
            var nodes = doc.SelectNodes("/events/event");

            // Iterate over all events and keep only those for this university.
            foreach (XmlElement ev in nodes)
            {
                var evUni = ev["university"]?.InnerText ?? "";
                if (!string.Equals(evUni, uni, StringComparison.OrdinalIgnoreCase)) continue;

                var id = ev.GetAttribute("id");
                var name = ev["name"]?.InnerText ?? "(unnamed)";
                var date = ev["date"]?.InnerText ?? "";
                var status = ev.GetAttribute("status");

                // Convert stored ISO timestamp to a local date string for display.
                string dateHuman;
                if (DateTime.TryParse(date, out var dt))
                    dateHuman = dt.ToLocalTime().ToString("yyyy-MM-dd");
                else
                    dateHuman = "(unset)";

                // Create a simple row object for the repeater (anonymous type).
                rows.Add(new
                {
                    id,
                    name,
                    status,
                    dateHuman,
                    manageUrl = ResolveUrl($"~/Account/UniversityAdmin/EventManage.aspx?id={id}")
                });
            }

            // Toggle "no events" placeholder and bind the list.
            NoEventsPlaceholder.Visible = rows.Count == 0;
            EventsRepeater.DataSource = rows;
            EventsRepeater.DataBind();
        }

        /// <summary>
        /// Utility: create an XML element with text content (null-safe to empty).
        /// </summary>
        private static XmlElement Mk(XmlDocument d, string name, string val)
        {
            var el = d.CreateElement(name);
            el.InnerText = val ?? "";
            return el;
        }

        /// <summary>
        /// Parses a date string (typically HTML5 yyyy-MM-dd) as local time and converts to ISO 8601 UTC midnight.
        /// Returns true on success with the ISO string in 'iso'; otherwise false.
        /// </summary>
        private static bool TryParseDateToIso(string input, out string iso)
        {
            iso = "";
            if (string.IsNullOrWhiteSpace(input)) return false;

            // HTML5 date usually "yyyy-MM-dd"; assume local zone, persist as UTC midnight for comparability.
            if (DateTime.TryParse(input, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var dt))
            {
                var localMidnight = new DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0, DateTimeKind.Local);
                iso = localMidnight.ToUniversalTime().ToString("o");
                return true;
            }
            return false;
        }

        /// <summary>
        /// Looks up the user's university in users.xml based on their email (case-insensitive).
        /// Returns empty string if not found or on error.
        /// </summary>
        private string LookupUniversityByEmail(string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email) || !File.Exists(UsersXmlPath)) return "";
                var doc = new XmlDocument();
                doc.Load(UsersXmlPath);
                var emailLower = email.ToLowerInvariant();

                // Case-insensitive email match via translate() for normalization.
                var node = doc.SelectSingleNode(
                    $"/users/user[translate(email,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz')='{emailLower}']");
                return node?["university"]?.InnerText ?? "";
            }
            catch { return ""; }
        }

        /// <summary>
        /// Resets form input fields; university stays as-is (set at login/page load).
        /// </summary>
        private void ClearForm()
        {
            EventName.Text = "";
            EventDate.Text = "";
            Description.Text = "";
            // UniversityDisplay/Value stay as-is (predefined)
        }
    }
}

