using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;

namespace CyberApp_FIA.Participant
{
    public partial class SelectEvent : Page
    {
        private string EventsXmlPath => Server.MapPath("~/App_Data/events.xml");
        private string UsersXmlPath => Server.MapPath("~/App_Data/users.xml");

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Gate: only Participants (adjust if Helpers also use this screen)
                var role = (string)Session["Role"];
                if (!string.Equals(role, "Participant", StringComparison.OrdinalIgnoreCase))
                {
                    Response.Redirect("~/Account/Login.aspx");
                    return;
                }

                // Try to preselect from Session or users.xml
                var email = (string)Session["Email"] ?? "";
                var userUni = (string)Session["University"];
                if (string.IsNullOrWhiteSpace(userUni))
                    userUni = LookupUniversityByEmail(email);

                // Always bind the University dropdown; preselect if we know one
                BindUniversities(out var hadSelection, prefer: userUni);

                // If we preselected a university, load its active events
                if (hadSelection)
                    BindEventsForUniversity(UniversitySelect.SelectedValue);
            }
        }

        // --- UI events ---

        protected void UniversitySelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindEventsForUniversity(UniversitySelect.SelectedValue);
        }

        protected void UniRequired_ServerValidate(object source, ServerValidateEventArgs args)
        {
            args.IsValid = !string.IsNullOrWhiteSpace(UniversitySelect.SelectedValue);
        }

        protected void EventRequired_ServerValidate(object source, ServerValidateEventArgs args)
        {
            args.IsValid = !string.IsNullOrWhiteSpace(EventSelect.SelectedValue);
        }

        protected void BtnContinue_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            var selectedUni = UniversitySelect.SelectedValue;
            var selectedEventId = EventSelect.SelectedValue;

            // Persist scope for this session
            Session["University"] = selectedUni;
            Session["EventId"] = selectedEventId;

            // OPTIONAL: store university on the participant’s profile if it was blank
            var email = (string)Session["Email"] ?? "";
            if (!string.IsNullOrWhiteSpace(email))
                SaveUserUniversityIfEmpty(email, selectedUni);

            // Go to participant home/catalog
            Response.Redirect("~/Account/Participant/Home.aspx");
        }

        // --- Binding helpers ---

        private void BindUniversities(out bool preselected, string prefer)
        {
            preselected = false;
            UniversitySelect.Items.Clear();

            var unis = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var ev in LoadActiveEvents())
                unis.Add(ev.university);

            UniversitySelect.Items.Add(new ListItem("-- Select university --", ""));
            foreach (var u in unis)
            {
                var li = new ListItem(u, u);
                UniversitySelect.Items.Add(li);

                if (!string.IsNullOrWhiteSpace(prefer) &&
                    u.Equals(prefer, StringComparison.OrdinalIgnoreCase))
                {
                    UniversitySelect.ClearSelection();
                    li.Selected = true;
                    preselected = true;
                }
            }
        }

        private void BindEventsForUniversity(string uni)
        {
            EventSelect.Items.Clear();
            EventSelect.Items.Add(new ListItem("-- Select event --", ""));

            if (string.IsNullOrWhiteSpace(uni)) return;

            foreach (var ev in LoadActiveEvents())
            {
                if (!ev.university.Equals(uni, StringComparison.OrdinalIgnoreCase)) continue;

                var label = ev.name;
                if (ev.dateUtc.HasValue)
                    label += " — " + ev.dateUtc.Value.ToLocalTime().ToString("yyyy-MM-dd");

                EventSelect.Items.Add(new ListItem(label, ev.id));
            }
        }

        private IEnumerable<(string id, string name, string university, DateTime? dateUtc)> LoadActiveEvents()
        {
            var list = new List<(string, string, string, DateTime?)>();
            if (!File.Exists(EventsXmlPath)) return list;

            var doc = new XmlDocument(); doc.Load(EventsXmlPath);
            foreach (XmlElement ev in doc.SelectNodes("/events/event"))
            {
                // Hide deprecated events; show today/future
                var status = ev.GetAttribute("status");
                if (status.Equals("Deprecated", StringComparison.OrdinalIgnoreCase)) continue;

                var uni = ev["university"]?.InnerText ?? "";
                var name = ev["name"]?.InnerText ?? "(unnamed)";
                var id = ev.GetAttribute("id");
                var date = ev["date"]?.InnerText ?? "";

                DateTime when;
                DateTime? dt = DateTime.TryParse(date, out when) ? when : (DateTime?)null;

                if (dt == null || dt.Value.Date >= DateTime.UtcNow.Date)
                    list.Add((id, name, uni, dt));
            }
            return list;
        }

        // --- Profile helpers ---

        private string LookupUniversityByEmail(string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email) || !File.Exists(UsersXmlPath)) return "";
                var doc = new XmlDocument(); doc.Load(UsersXmlPath);
                var emailLower = email.ToLowerInvariant();
                var node = doc.SelectSingleNode(
                    $"/users/user[translate(email,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz')='{emailLower}']");
                return node?["university"]?.InnerText ?? "";
            }
            catch { return ""; }
        }

        private void SaveUserUniversityIfEmpty(string email, string university)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(university) ||
                    string.IsNullOrWhiteSpace(email) ||
                    !File.Exists(UsersXmlPath)) return;

                var doc = new XmlDocument(); doc.Load(UsersXmlPath);
                var emailLower = email.ToLowerInvariant();
                var node = doc.SelectSingleNode(
                    $"/users/user[translate(email,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz')='{emailLower}']") as XmlElement;

                if (node == null) return;

                var uniNode = node["university"];
                if (uniNode == null)
                {
                    uniNode = doc.CreateElement("university");
                    node.AppendChild(uniNode);
                }

                if (string.IsNullOrWhiteSpace(uniNode.InnerText))
                {
                    uniNode.InnerText = university;
                    doc.Save(UsersXmlPath);
                }
            }
            catch
            {
                // swallow for now; optional profile write shouldn't block flow
            }
        }
    }
}
