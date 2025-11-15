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

                // If user explicitly clicked "Change event", bypass fast-path and clear current selection
                var bypass = string.Equals(Request.QueryString["change"], "1", StringComparison.OrdinalIgnoreCase);
                if (bypass)
                {
                    Session["EventId"] = null; // make the picker show instead of jumping back
                }

                // === Fast-path: if this participant has a saved last event and we're NOT bypassing, go straight to Home ===
                if (!bypass)
                {
                    var userId = Session["UserId"] as string; // key by user ID
                    var lastEvent = LoadLastEventIdForUser(userId);
                    if (!string.IsNullOrWhiteSpace(lastEvent) && EventExists(lastEvent))
                    {
                        Session["EventId"] = lastEvent;
                        Response.Redirect("~/Account/Participant/Home.aspx");
                        return;
                    }
                }
                // === end fast-path ===

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
            {
                SaveUserUniversityIfEmpty(email, selectedUni);

                // NEW: assign a Helper from this university to the participant (one-time, balanced by current load).
                AssignHelperIfMissing(email, selectedUni);
            }

            // NEW: persist lastEventId on the user's profile (keyed by UserId, fallback to email lookup)
            var userId = Session["UserId"] as string;
            SaveLastEventIdForUser(userId, selectedEventId, email);

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

        // --- New helpers for assigning Helpers to Participants ---

        /// <summary>
        /// Assigns a Helper from the given university to the participant identified by email,
        /// but only if they do not already have an assignment.
        /// The chosen Helper is the one in that university with the fewest currently assigned participants
        /// (based on the users.xml assignedHelperId attribute), to help balance load.
        /// </summary>
        private void AssignHelperIfMissing(string email, string university)
        {
            try
            {
                // Guard clauses: we need a valid email, university, and users.xml file.
                if (string.IsNullOrWhiteSpace(email) ||
                    string.IsNullOrWhiteSpace(university) ||
                    !File.Exists(UsersXmlPath))
                {
                    return;
                }

                var doc = new XmlDocument();
                doc.Load(UsersXmlPath);

                var emailLower = email.ToLowerInvariant();

                // Locate the participant user row by email (case-insensitive).
                var userNode = doc.SelectSingleNode(
                    $"/users/user[translate(email,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz')='{emailLower}']"
                ) as XmlElement;

                if (userNode == null)
                {
                    // No user found for this email; nothing to assign.
                    return;
                }

                // Only assign Helpers for Participant accounts; skip other roles.
                var role = (userNode.GetAttribute("role") ?? string.Empty).Trim();
                if (!role.Equals("Participant", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                // Respect existing assignments: if this participant already has a Helper, stop.
                var existingHelperId = userNode.GetAttribute("assignedHelperId");
                if (!string.IsNullOrWhiteSpace(existingHelperId))
                {
                    return;
                }

                // Collect all Helpers whose university matches the participant's selected university.
                var helperCandidates = new List<XmlElement>();
                var helperNodes = doc.SelectNodes("/users/user[@role='Helper']");
                foreach (XmlElement helper in helperNodes)
                {
                    var helperUni = (helper["university"]?.InnerText ?? string.Empty).Trim();
                    if (helperUni.Length == 0)
                    {
                        continue; // helper not scoped to any university yet
                    }

                    if (string.Equals(helperUni, university.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        helperCandidates.Add(helper);
                    }
                }

                if (helperCandidates.Count == 0)
                {
                    // No Helpers registered for this university; leave unassigned.
                    return;
                }

                // Choose the Helper with the fewest assigned participants in this university.
                XmlElement chosenHelper = null;
                var minCount = int.MaxValue;

                foreach (var helper in helperCandidates)
                {
                    var helperId = helper.GetAttribute("id");
                    if (string.IsNullOrWhiteSpace(helperId))
                    {
                        continue;
                    }

                    // Count participants that currently reference this Helper's id.
                    var assignedNodes = doc.SelectNodes(
                        $"/users/user[@role='Participant' and @assignedHelperId='{helperId}']"
                    );
                    var currentCount = assignedNodes?.Count ?? 0;

                    if (currentCount < minCount)
                    {
                        minCount = currentCount;
                        chosenHelper = helper;
                    }
                }

                if (chosenHelper == null)
                {
                    return;
                }

                var chosenId = chosenHelper.GetAttribute("id");
                if (string.IsNullOrWhiteSpace(chosenId))
                {
                    return;
                }

                // Persist the assignment as an attribute on the participant <user> row.
                // This adds the new attribute to users.xml:
                //   <user id="..." role="Participant" assignedHelperId="helper-guid">
                userNode.SetAttribute("assignedHelperId", chosenId);

                doc.Save(UsersXmlPath);
            }
            catch
            {
                // Helper assignment is best-effort; failures should not block navigation.
            }
        }


        // --- New helpers for lastEventId persistence/lookup ---

        private string LoadLastEventIdForUser(string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId) || !File.Exists(UsersXmlPath)) return null;
                var doc = new XmlDocument(); doc.Load(UsersXmlPath);
                var node = (XmlElement)doc.SelectSingleNode($"/users/user[@id='{userId}']/lastEventId");
                return node?.InnerText?.Trim();
            }
            catch { return null; }
        }

        private void SaveLastEventIdForUser(string userId, string eventId, string fallbackEmail = "")
        {
            try
            {
                if (string.IsNullOrEmpty(eventId) || !File.Exists(UsersXmlPath)) return;

                var doc = new XmlDocument(); doc.Load(UsersXmlPath);
                XmlElement user = null;

                if (!string.IsNullOrEmpty(userId))
                {
                    user = doc.SelectSingleNode($"/users/user[@id='{userId}']") as XmlElement;
                }

                // Fallback: locate by email if no userId in session
                if (user == null && !string.IsNullOrWhiteSpace(fallbackEmail))
                {
                    var emailLower = fallbackEmail.ToLowerInvariant();
                    user = doc.SelectSingleNode(
                        $"/users/user[translate(email,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz')='{emailLower}']") as XmlElement;
                }

                if (user == null) return;

                var node = user["lastEventId"];
                if (node == null)
                {
                    node = doc.CreateElement("lastEventId");
                    user.AppendChild(node);
                }
                node.InnerText = eventId;

                doc.Save(UsersXmlPath);
            }
            catch
            {
                // don't block navigation on profile write issues
            }
        }

        private bool EventExists(string eventId)
        {
            try
            {
                if (string.IsNullOrEmpty(eventId) || !File.Exists(EventsXmlPath)) return false;
                var doc = new XmlDocument(); doc.Load(EventsXmlPath);
                var ev = (XmlElement)doc.SelectSingleNode($"/events/event[@id='{eventId}']");
                return ev != null;
            }
            catch { return false; }
        }
    }
}
