using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace CyberApp_FIA.Account.Helper
{
    public partial class HelperAccount : System.Web.UI.Page
    {
        // Update these file names if your XML files use different names.
        private const string UsersFile = "~/App_Data/users.xml";
        private const string CompletionsFile = "~/App_Data/lessonCompletions.xml";
        private const string AttendanceFile = "~/App_Data/attendance.xml";
        private const string BadgesFile = "~/App_Data/badges.xml";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadHelperAccount();
            }
        }

        private void LoadHelperAccount()
        {
            List<string> identityValues = GetCurrentIdentityValues();

            if (!identityValues.Any())
            {
                ShowError("No signed-in helper was found in session. Update the session key names in this page if your project uses different login keys.");
                return;
            }

            XElement helperNode = FindUserRecord("Helper", identityValues);

            if (helperNode == null)
            {
                ShowError("A matching helper record could not be found in users.xml.");
                return;
            }

            pnlError.Visible = false;
            pnlContent.Visible = true;

            string fullName = FirstNonEmpty(
                ReadValue(helperNode, "FullName", "fullName", "Name", "name"),
                BuildFullName(helperNode),
                "Helper"
            );

            lblHelperNameHeader.Text = fullName;
            lblHelperName.Text = fullName;
            lblHelperUsername.Text = FirstNonEmpty(ReadValue(helperNode, "Username", "UserName", "username", "userName"), "Not available");
            lblHelperEmail.Text = FirstNonEmpty(ReadValue(helperNode, "Email", "UserEmail", "email", "userEmail"), "Not available");
            lblHelperUniversity.Text = FirstNonEmpty(ReadValue(helperNode, "University", "School", "College", "university"), "Not available");
            lblHelperJoined.Text = FormatDate(
                ReadValue(helperNode, "JoinedDate", "CreatedDate", "DateCreated", "joinedDate", "createdDate")
            );
            lblHelperRole.Text = "Helper";

            lblSessionsTaught.Text = CountSessionsTaught(identityValues).ToString();
            lblParticipantsHelped.Text = CountParticipantsHelped(identityValues).ToString();
            lblVerifiedCompletions.Text = CountVerifiedCompletions(identityValues).ToString();
            lblBadgesEarned.Text = CountBadges(identityValues, "Helper").ToString();
        }

        private List<string> GetCurrentIdentityValues()
        {
            var values = new List<string>();

            AddIfNotBlank(values, Convert.ToString(Session["Username"]));
            AddIfNotBlank(values, Convert.ToString(Session["UserName"]));
            AddIfNotBlank(values, Convert.ToString(Session["Email"]));
            AddIfNotBlank(values, Convert.ToString(Session["UserEmail"]));
            AddIfNotBlank(values, Convert.ToString(Session["LoginEmail"]));
            AddIfNotBlank(values, Convert.ToString(Session["CurrentUser"]));
            AddIfNotBlank(values, Convert.ToString(Session["CurrentUserEmail"]));

            if (Context != null && Context.User != null && Context.User.Identity != null && Context.User.Identity.IsAuthenticated)
            {
                AddIfNotBlank(values, Context.User.Identity.Name);
            }

            return values
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Select(v => v.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private XElement FindUserRecord(string expectedRole, IEnumerable<string> identityValues)
        {
            XDocument doc = LoadXml(UsersFile);
            if (doc == null) return null;

            return doc.Descendants()
                .Where(x => x.Elements().Any() || x.HasAttributes)
                .FirstOrDefault(x =>
                    RoleMatches(x, expectedRole) &&
                    FieldEqualsAny(x, identityValues,
                        "Username", "UserName", "username", "userName",
                        "Email", "UserEmail", "email", "userEmail",
                        "LoginEmail", "loginEmail", "Id", "UserId", "id", "userId"));
        }

        private int CountSessionsTaught(IEnumerable<string> identityValues)
        {
            var sessionKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            XDocument attendanceDoc = LoadXml(AttendanceFile);
            if (attendanceDoc != null)
            {
                foreach (XElement item in attendanceDoc.Descendants().Where(x => x.Elements().Any() || x.HasAttributes))
                {
                    bool isHelperMatch = FieldEqualsAny(item, identityValues,
                        "HelperUsername", "helperUsername",
                        "HelperEmail", "helperEmail",
                        "Username", "UserName", "username", "userName",
                        "Email", "UserEmail", "email", "userEmail",
                        "MarkedBy", "markedBy", "AssignedHelper", "assignedHelper");

                    if (!isHelperMatch) continue;

                    string status = ReadValue(item, "Status", "status");
                    if (ValueMatchesAny(status, "Cancelled", "Canceled", "Dropped")) continue;

                    string sessionKey = FirstNonEmpty(
                        ReadValue(item, "SessionId", "sessionId"),
                        ReadValue(item, "EventId", "eventId") + "|" + ReadValue(item, "Title", "title")
                    );

                    if (!string.IsNullOrWhiteSpace(sessionKey))
                    {
                        sessionKeys.Add(sessionKey);
                    }
                }
            }

            XDocument completionDoc = LoadXml(CompletionsFile);
            if (completionDoc != null)
            {
                foreach (XElement item in completionDoc.Descendants().Where(x => x.Elements().Any() || x.HasAttributes))
                {
                    bool isHelperMatch = FieldEqualsAny(item, identityValues,
                        "HelperUsername", "helperUsername",
                        "HelperEmail", "helperEmail",
                        "ApprovedBy", "approvedBy",
                        "MarkedBy", "markedBy",
                        "AssignedHelper", "assignedHelper");

                    if (!isHelperMatch) continue;

                    string sessionKey = FirstNonEmpty(
                        ReadValue(item, "SessionId", "sessionId"),
                        ReadValue(item, "EventId", "eventId") + "|" + ReadValue(item, "LessonId", "lessonId", "CourseId", "courseId")
                    );

                    if (!string.IsNullOrWhiteSpace(sessionKey))
                    {
                        sessionKeys.Add(sessionKey);
                    }
                }
            }

            return sessionKeys.Count;
        }

        private int CountParticipantsHelped(IEnumerable<string> identityValues)
        {
            var participantKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            XDocument attendanceDoc = LoadXml(AttendanceFile);
            if (attendanceDoc == null) return 0;

            foreach (XElement item in attendanceDoc.Descendants().Where(x => x.Elements().Any() || x.HasAttributes))
            {
                bool isHelperMatch = FieldEqualsAny(item, identityValues,
                    "HelperUsername", "helperUsername",
                    "HelperEmail", "helperEmail",
                    "MarkedBy", "markedBy",
                    "AssignedHelper", "assignedHelper");

                if (!isHelperMatch) continue;

                string status = ReadValue(item, "Status", "status");
                if (ValueMatchesAny(status, "Missing", "No Show", "No-Show", "Absent", "Cancelled", "Canceled")) continue;

                string participantKey = FirstNonEmpty(
                    ReadValue(item, "ParticipantUsername", "participantUsername"),
                    ReadValue(item, "ParticipantEmail", "participantEmail"),
                    ReadValue(item, "Username", "UserName", "username", "userName"),
                    ReadValue(item, "Email", "UserEmail", "email", "userEmail")
                );

                if (!string.IsNullOrWhiteSpace(participantKey))
                {
                    participantKeys.Add(participantKey);
                }
            }

            return participantKeys.Count;
        }

        private int CountVerifiedCompletions(IEnumerable<string> identityValues)
        {
            XDocument doc = LoadXml(CompletionsFile);
            if (doc == null) return 0;

            var completionKeys = doc.Descendants()
                .Where(x => x.Elements().Any() || x.HasAttributes)
                .Where(x =>
                    FieldEqualsAny(x, identityValues,
                        "HelperUsername", "helperUsername",
                        "HelperEmail", "helperEmail",
                        "ApprovedBy", "approvedBy",
                        "MarkedBy", "markedBy",
                        "AssignedHelper", "assignedHelper"))
                .Where(x => !ValueMatchesAny(ReadValue(x, "Status", "status"), "Revoked", "Rejected"))
                .Select(x => FirstNonEmpty(
                    ReadValue(x, "CompletionId", "completionId"),
                    ReadValue(x, "SessionId", "sessionId") + "|" + ReadValue(x, "ParticipantUsername", "participantUsername", "ParticipantEmail", "participantEmail"),
                    Guid.NewGuid().ToString()
                ))
                .Distinct(StringComparer.OrdinalIgnoreCase);

            return completionKeys.Count();
        }

        private int CountBadges(IEnumerable<string> identityValues, string expectedRole)
        {
            XDocument doc = LoadXml(BadgesFile);
            if (doc == null) return 0;

            var badgeKeys = doc.Descendants()
                .Where(x => x.Elements().Any() || x.HasAttributes)
                .Where(x =>
                    FieldEqualsAny(x, identityValues,
                        "AwardedToUsername", "awardedToUsername",
                        "AwardedToEmail", "awardedToEmail",
                        "HelperUsername", "helperUsername",
                        "HelperEmail", "helperEmail",
                        "Username", "UserName", "username", "userName",
                        "Email", "UserEmail", "email", "userEmail"))
                .Where(x =>
                {
                    string roleValue = ReadValue(x, "Role", "role", "AwardedRole", "awardedRole");
                    return string.IsNullOrWhiteSpace(roleValue) || roleValue.Equals(expectedRole, StringComparison.OrdinalIgnoreCase);
                })
                .Select(x => FirstNonEmpty(
                    ReadValue(x, "BadgeId", "badgeId"),
                    ReadValue(x, "BadgeName", "badgeName", "Title", "title"),
                    Guid.NewGuid().ToString()
                ))
                .Distinct(StringComparer.OrdinalIgnoreCase);

            return badgeKeys.Count();
        }

        private void ShowError(string message)
        {
            pnlContent.Visible = false;
            pnlError.Visible = true;
            lblError.Text = HttpUtility.HtmlEncode(message);
        }

        private XDocument LoadXml(string virtualPath)
        {
            string physicalPath = Server.MapPath(virtualPath);

            if (!File.Exists(physicalPath))
            {
                return null;
            }

            return XDocument.Load(physicalPath);
        }

        private string ReadValue(XElement element, params string[] names)
        {
            if (element == null || names == null || names.Length == 0)
            {
                return string.Empty;
            }

            foreach (string name in names)
            {
                XElement child = element.Elements()
                    .FirstOrDefault(e => e.Name.LocalName.Equals(name, StringComparison.OrdinalIgnoreCase));

                if (child != null && !string.IsNullOrWhiteSpace(child.Value))
                {
                    return child.Value.Trim();
                }

                XAttribute attribute = element.Attributes()
                    .FirstOrDefault(a => a.Name.LocalName.Equals(name, StringComparison.OrdinalIgnoreCase));

                if (attribute != null && !string.IsNullOrWhiteSpace(attribute.Value))
                {
                    return attribute.Value.Trim();
                }
            }

            return string.Empty;
        }

        private bool FieldEqualsAny(XElement element, IEnumerable<string> identityValues, params string[] fieldNames)
        {
            if (element == null || identityValues == null) return false;

            List<string> ids = identityValues
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Select(v => v.Trim())
                .ToList();

            if (!ids.Any()) return false;

            foreach (string fieldName in fieldNames)
            {
                string currentValue = ReadValue(element, fieldName);

                if (!string.IsNullOrWhiteSpace(currentValue) &&
                    ids.Any(id => currentValue.Equals(id, StringComparison.OrdinalIgnoreCase)))
                {
                    return true;
                }
            }

            return false;
        }

        private bool RoleMatches(XElement element, string expectedRole)
        {
            string role = ReadValue(element, "Role", "role", "UserRole", "userRole");

            if (!string.IsNullOrWhiteSpace(role))
            {
                return role.Equals(expectedRole, StringComparison.OrdinalIgnoreCase);
            }

            return element.Name.LocalName.IndexOf(expectedRole, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private bool ValueMatchesAny(string currentValue, params string[] matches)
        {
            if (string.IsNullOrWhiteSpace(currentValue)) return false;

            return matches.Any(m => currentValue.Equals(m, StringComparison.OrdinalIgnoreCase));
        }

        private string BuildFullName(XElement element)
        {
            string first = ReadValue(element, "FirstName", "firstName", "FName", "fname");
            string last = ReadValue(element, "LastName", "lastName", "LName", "lname");
            string combined = (first + " " + last).Trim();

            return string.IsNullOrWhiteSpace(combined) ? string.Empty : combined;
        }

        private string FormatDate(string rawDate)
        {
            DateTime parsed;
            return DateTime.TryParse(rawDate, out parsed)
                ? parsed.ToString("MMMM d, yyyy")
                : "Not available";
        }

        private string FirstNonEmpty(params string[] values)
        {
            foreach (string value in values)
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value.Trim();
                }
            }

            return string.Empty;
        }

        private void AddIfNotBlank(List<string> list, string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                list.Add(value.Trim());
            }
        }
    }
}
