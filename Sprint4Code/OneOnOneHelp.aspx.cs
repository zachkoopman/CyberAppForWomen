using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Xml;

namespace CyberApp_FIA.Helper
{
    /// <summary>
    /// 1:1 Help workspace view for Helpers.
    /// Shows all participants assigned to the signed-in Helper
    /// with their first name and email address.
    /// </summary>
    public partial class OneOnOneHelp : Page
    {
        private string UsersXmlPath => Server.MapPath("~/App_Data/users.xml");

        private sealed class ParticipantRow
        {
            public string FirstName { get; set; }
            public string Email { get; set; }
            public string University { get; set; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                GuardHelperRole();
                BindParticipants();
            }
        }

        /// <summary>
        /// Ensures only Helpers can access this page.
        /// Redirects to login if the role or user id is missing/mismatched.
        /// </summary>
        private void GuardHelperRole()
        {
            var roleRaw = Session["Role"] as string ?? "";
            if (!string.Equals(roleRaw.Trim(), "Helper", StringComparison.OrdinalIgnoreCase))
            {
                Response.Redirect("~/Account/Login.aspx");
                return;
            }

            var userId = Session["UserId"] as string;
            if (string.IsNullOrWhiteSpace(userId))
            {
                Response.Redirect("~/Account/Login.aspx");
                return;
            }
        }

        /// <summary>
        /// Loads all participants that are assigned to the current Helper and
        /// binds them into the participants repeater.
        /// </summary>
        private void BindParticipants()
        {
            var currentHelperId = Session["UserId"] as string;
            var rows = new List<ParticipantRow>();

            if (string.IsNullOrWhiteSpace(currentHelperId))
            {
                NoParticipantsPH.Visible = true;
                ParticipantsRepeater.DataSource = null;
                ParticipantsRepeater.DataBind();
                return;
            }

            if (!File.Exists(UsersXmlPath))
            {
                NoParticipantsPH.Visible = true;
                ParticipantsRepeater.DataSource = null;
                ParticipantsRepeater.DataBind();
                return;
            }

            try
            {
                var doc = new XmlDocument();
                doc.Load(UsersXmlPath);

                var userNodes = doc.SelectNodes("/users/user");
                if (userNodes != null)
                {
                    foreach (XmlElement user in userNodes)
                    {
                        if (!IsAssignedToHelper(user, currentHelperId))
                            continue;

                        var firstName = user["firstName"]?.InnerText ?? "";
                        var email = user["email"]?.InnerText ?? "";
                        var uni = user["university"]?.InnerText ?? "";

                        if (string.IsNullOrWhiteSpace(firstName) &&
                            string.IsNullOrWhiteSpace(email))
                        {
                            continue; // skip incomplete rows
                        }

                        rows.Add(new ParticipantRow
                        {
                            FirstName = firstName.Trim(),
                            Email = email.Trim(),
                            University = (uni ?? string.Empty).Trim()
                        });
                    }
                }
            }
            catch
            {
                // If anything goes wrong loading XML, show empty state gracefully.
                rows.Clear();
            }

            rows = rows
                .OrderBy(r => string.IsNullOrWhiteSpace(r.FirstName) ? "{" : r.FirstName)
                .ThenBy(r => r.Email)
                .ToList();

            NoParticipantsPH.Visible = rows.Count == 0;
            ParticipantsRepeater.DataSource = rows;
            ParticipantsRepeater.DataBind();
        }

        /// <summary>
        /// Determines whether a given user node is assigned to the specified Helper.
        /// This is intentionally flexible to support multiple possible XML shapes:
        ///   - helperId attribute on &lt;user&gt;
        ///   - &lt;helperId&gt; child element
        ///   - &lt;assignedHelper id="..." /&gt; child element
        /// </summary>
        private static bool IsAssignedToHelper(XmlElement userNode, string helperId)
        {
            if (userNode == null || string.IsNullOrWhiteSpace(helperId))
                return false;

            // 1) helperId attribute on <user>
            var helperAttr = (userNode.GetAttribute("helperId") ?? string.Empty).Trim();
            if (!string.IsNullOrEmpty(helperAttr) &&
                string.Equals(helperAttr, helperId, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // 2) <helperId> child element
            var helperElemVal = (userNode["helperId"]?.InnerText ?? string.Empty).Trim();
            if (!string.IsNullOrEmpty(helperElemVal) &&
                string.Equals(helperElemVal, helperId, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // 3) <assignedHelper id="..." /> child element
            var assignedHelperNode = userNode.SelectSingleNode("assignedHelper") as XmlElement;
            if (assignedHelperNode != null)
            {
                var assignedId = (assignedHelperNode.GetAttribute("id") ?? string.Empty).Trim();
                if (!string.IsNullOrEmpty(assignedId) &&
                    string.Equals(assignedId, helperId, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
