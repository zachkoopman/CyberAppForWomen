using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace CyberApp_FIA.Account.Participant
{
    public partial class ParticipantAccount : System.Web.UI.Page
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
                LoadParticipantAccount();
            }
        }

        private void LoadParticipantAccount()
        {
            List<string> identityValues = GetCurrentIdentityValues();

            if (!identityValues.Any())
            {
                ShowError("No signed-in participant was found in session. Update the session key names in this page if your project uses different login keys.");
                return;
            }

            XElement participantNode = FindUserRecord("Participant", identityValues);

            if (participantNode == null)
            {
                ShowError("A matching participant record could not be found in users.xml.");
                return;
            }

            pnlError.Visible = false;
            pnlContent.Visible = true;

            string fullName = FirstNonEmpty(
                ReadValue(participantNode, "FullName", "fullName", "Name", "name"),
                BuildFullName(participantNode),
                "Participant"
            );

            lblParticipantNameHeader.Text = fullName;
            lblParticipantName.Text = fullName;
            lblParticipantUsername.Text = FirstNonEmpty(ReadValue(participantNode, "Username", "UserName", "username", "userName"), "Not available");
            lblParticipantEmail.Text = FirstNonEmpty(ReadValue(participantNode, "Email", "UserEmail", "email", "userEmail"), "Not available");
            lblParticipantUniversity.Text = FirstNonEmpty(ReadValue(participantNode, "University", "School", "College", "university"), "Not available");
            lblParticipantJoined.Text = FormatDate(
                ReadValue(participantNode, "JoinedDate", "CreatedDate", "DateCreated", "joinedDate", "createdDate")
            );
            lblParticipantRole.Text = "Participant";

            lblCompletedLessons.Text = CountParticipantCompletedLessons(identityValues).ToString();
            lblEnrolledSessions.Text = CountParticipantEnrolledSessions(identityValues).ToString();
            lblBadgesEarned.Text = CountBadges(identityValues, "Participant").ToString();
            lblNoShows.Text = CountParticipantNoShows(identityValues).ToString();
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

        private int CountParticipantCompletedLessons(IEnumerable<string> identityValues)
        {
            XDocument doc = LoadXml(CompletionsFile);
            if (doc == null) return 0;

            var completionKeys = doc.Descendants()
                .Where(x => x.Elements().Any() || x.HasAttributes)
                .Where(x =>
                    FieldEqualsAny(x, identityValues,
                        "ParticipantUsername", "participantUsername",
                        "ParticipantEmail", "participantEmail",
                        "Username", "UserName", "username", "userName",
                        "Email", "UserEmail", "email", "userEmail"))
                .Where(x => !ValueMatchesAny(ReadValue(x, "Status", "status"), "Revoked", "Rejected"))
                .Select(x => FirstNonEmpty(
                    ReadValue(x, "CompletionId", "completionId"),
                    ReadValue(x, "SessionId", "sessionId") + "|" + ReadValue(x, "LessonId", "lessonId", "CourseId", "courseId", "MicrocourseId", "microcourseId"),
                    Guid.NewGuid().ToString()
                ))
                .Distinct(StringComparer.OrdinalIgnoreCase);

            return completionKeys.Count();
        }

        private int CountParticipantEnrolledSessions(IEnumerable<string> identityValues)
        {
            XDocument doc = LoadXml(AttendanceFile);
            if (doc == null) return 0;

            var sessionKeys = doc.Descendants()
                .Where(x => x.Elements().Any() || x.HasAttributes)
                .Where(x =>
                    FieldEqualsAny(x, identityValues,
                        "ParticipantUsername", "participantUsername",
                        "ParticipantEmail", "participantEmail",
                        "Username", "UserName", "username", "userName",
                        "Email", "UserEmail", "email", "userEmail"))
                .Where(x => !ValueMatchesAny(ReadValue(x, "Status", "status"), "Dropped", "Removed", "Cancelled", "Canceled", "Unenrolled"))
                .Select(x => FirstNonEmpty(
                    ReadValue(x, "SessionId", "sessionId"),
                    ReadValue(x, "EventId", "eventId") + "|" + ReadValue(x, "Title", "title"),
                    Guid.NewGuid().ToString()
                ))
                .Distinct(StringComparer.OrdinalIgnoreCase);

            return sessionKeys.Count();
        }

        private int CountParticipantNoShows(IEnumerable<string> identityValues)
        {
            XDocument doc = LoadXml(AttendanceFile);
            if (doc == null) return 0;

            return doc.Descendants()
                .Where(x => x.Elements().Any() || x.HasAttributes)
                .Where(x =>
                    FieldEqualsAny(x, identityValues,
                        "ParticipantUsername", "participantUsername",
                        "ParticipantEmail", "participantEmail",
                        "Username", "UserName", "username", "userName",
                        "Email", "UserEmail", "email", "userEmail"))
                .Count(x => ValueMatchesAny(ReadValue(x, "Status", "status"), "Missing", "No Show", "No-Show", "Absent"));
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
                        "ParticipantUsername", "participantUsername",
                        "ParticipantEmail", "participantEmail",
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
