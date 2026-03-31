using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace CyberApp_FIA.Account.UniversityAdmin
{
    public partial class UniversityAdminParticipants : System.Web.UI.Page
    {
        private const string UsersFile = "~/App_Data/users.xml";
        private const string CompletionsFile = "~/App_Data/lessonCompletions.xml";
        private const string AttendanceFile = "~/App_Data/attendance.xml";
        private const string BadgesFile = "~/App_Data/badges.xml";
        private const string OverridesFile = "~/App_Data/participantStatOverrides.xml";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadParticipants(string.Empty);
            }
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            LoadParticipants(txtSearch.Text.Trim());
        }

        private void LoadParticipants(string searchText)
        {
            string adminUniversity = GetCurrentAdminUniversity();

            if (string.IsNullOrWhiteSpace(adminUniversity))
            {
                ShowError("A University Admin account or university scope could not be found for the current session.");
                return;
            }

            lblUniversityName.Text = adminUniversity;
            lblUniversityNameHeader.Text = adminUniversity;

            XDocument usersDoc = LoadXml(UsersFile);
            if (usersDoc == null)
            {
                ShowError("users.xml could not be found.");
                return;
            }

            List<ParticipantRow> rows = usersDoc.Descendants()
                .Where(x => x.Elements().Any() || x.HasAttributes)
                .Where(IsParticipantRole)
                .Where(x => UniversityMatches(x, adminUniversity))
                .Select(x => BuildParticipantRow(x, adminUniversity))
                .Where(x => x != null)
                .ToList();

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                rows = rows.Where(r =>
                        ContainsIgnoreCase(r.FullName, searchText) ||
                        ContainsIgnoreCase(r.Username, searchText) ||
                        ContainsIgnoreCase(r.Email, searchText))
                    .ToList();
            }

            rows = rows
                .OrderBy(r => r.FullName)
                .ThenBy(r => r.Username)
                .ToList();

            rptParticipants.DataSource = rows;
            rptParticipants.DataBind();

            lblParticipantCount.Text = rows.Count.ToString();
            pnlEmpty.Visible = rows.Count == 0;
            pnlContent.Visible = true;
            pnlError.Visible = false;
        }

        private ParticipantRow BuildParticipantRow(XElement participantNode, string adminUniversity)
        {
            List<string> identityValues = GetIdentityValuesFromUserNode(participantNode);
            ParticipantStats stats = GetParticipantStats(identityValues, adminUniversity);

            string username = FirstNonEmpty(ReadValue(participantNode, "Username", "UserName", "username", "userName"));
            string email = FirstNonEmpty(ReadValue(participantNode, "Email", "UserEmail", "email", "userEmail"));
            string participantKey = FirstNonEmpty(username, email, ReadValue(participantNode, "Id", "UserId", "id", "userId"));

            return new ParticipantRow
            {
                FullName = FirstNonEmpty(ReadValue(participantNode, "FullName", "fullName", "Name", "name"), BuildFullName(participantNode), "Participant"),
                Username = FirstNonEmpty(username, "Not available"),
                Email = FirstNonEmpty(email, "Not available"),
                JoinedText = FormatDate(ReadValue(participantNode, "JoinedDate", "CreatedDate", "DateCreated", "joinedDate", "createdDate")),
                CompletedLessons = stats.CompletedLessons,
                EnrolledSessions = stats.EnrolledSessions,
                BadgesEarned = stats.BadgesEarned,
                NoShows = stats.NoShows,
                DetailUrl = ResolveUrl("~/Account/UniversityAdmin/UniversityAdminParticipantDetails.aspx?participantKey=" + HttpUtility.UrlEncode(participantKey))
            };
        }

        private ParticipantStats GetParticipantStats(IEnumerable<string> participantIdentityValues, string adminUniversity)
        {
            ParticipantStats stats = new ParticipantStats
            {
                CompletedLessons = CountCompletedLessons(participantIdentityValues),
                EnrolledSessions = CountEnrolledSessions(participantIdentityValues),
                BadgesEarned = CountBadges(participantIdentityValues),
                NoShows = CountNoShows(participantIdentityValues)
            };

            ParticipantOverride latestOverride = GetLatestOverride(participantIdentityValues, adminUniversity);
            if (latestOverride != null)
            {
                if (latestOverride.CompletedLessons.HasValue) stats.CompletedLessons = latestOverride.CompletedLessons.Value;
                if (latestOverride.EnrolledSessions.HasValue) stats.EnrolledSessions = latestOverride.EnrolledSessions.Value;
                if (latestOverride.BadgesEarned.HasValue) stats.BadgesEarned = latestOverride.BadgesEarned.Value;
                if (latestOverride.NoShows.HasValue) stats.NoShows = latestOverride.NoShows.Value;
            }

            return stats;
        }

        private int CountCompletedLessons(IEnumerable<string> identityValues)
        {
            XDocument doc = LoadXml(CompletionsFile);
            if (doc == null) return 0;

            return doc.Descendants()
                .Where(x => x.Elements().Any() || x.HasAttributes)
                .Where(x => FieldEqualsAny(x, identityValues,
                    "ParticipantUsername", "participantUsername",
                    "ParticipantEmail", "participantEmail",
                    "Username", "UserName", "username", "userName",
                    "Email", "UserEmail", "email", "userEmail"))
                .Where(x => !ValueMatchesAny(ReadValue(x, "Status", "status"), "Revoked", "Rejected"))
                .Select(x => FirstNonEmpty(
                    ReadValue(x, "CompletionId", "completionId"),
                    ReadValue(x, "SessionId", "sessionId") + "|" + ReadValue(x, "LessonId", "lessonId", "CourseId", "courseId", "MicrocourseId", "microcourseId"),
                    Guid.NewGuid().ToString()))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count();
        }

        private int CountEnrolledSessions(IEnumerable<string> identityValues)
        {
            XDocument doc = LoadXml(AttendanceFile);
            if (doc == null) return 0;

            return doc.Descendants()
                .Where(x => x.Elements().Any() || x.HasAttributes)
                .Where(x => FieldEqualsAny(x, identityValues,
                    "ParticipantUsername", "participantUsername",
                    "ParticipantEmail", "participantEmail",
                    "Username", "UserName", "username", "userName",
                    "Email", "UserEmail", "email", "userEmail"))
                .Where(x => !ValueMatchesAny(ReadValue(x, "Status", "status"), "Dropped", "Removed", "Cancelled", "Canceled", "Unenrolled"))
                .Select(x => FirstNonEmpty(
                    ReadValue(x, "SessionId", "sessionId"),
                    ReadValue(x, "EventId", "eventId") + "|" + ReadValue(x, "Title", "title"),
                    Guid.NewGuid().ToString()))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count();
        }

        private int CountBadges(IEnumerable<string> identityValues)
        {
            XDocument doc = LoadXml(BadgesFile);
            if (doc == null) return 0;

            return doc.Descendants()
                .Where(x => x.Elements().Any() || x.HasAttributes)
                .Where(x => FieldEqualsAny(x, identityValues,
                    "AwardedToUsername", "awardedToUsername",
                    "AwardedToEmail", "awardedToEmail",
                    "ParticipantUsername", "participantUsername",
                    "ParticipantEmail", "participantEmail",
                    "Username", "UserName", "username", "userName",
                    "Email", "UserEmail", "email", "userEmail"))
                .Where(x =>
                {
                    string roleValue = ReadValue(x, "Role", "role", "AwardedRole", "awardedRole");
                    return string.IsNullOrWhiteSpace(roleValue) || roleValue.Equals("Participant", StringComparison.OrdinalIgnoreCase);
                })
                .Select(x => FirstNonEmpty(
                    ReadValue(x, "BadgeId", "badgeId"),
                    ReadValue(x, "BadgeName", "badgeName", "Title", "title"),
                    Guid.NewGuid().ToString()))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count();
        }

        private int CountNoShows(IEnumerable<string> identityValues)
        {
            XDocument doc = LoadXml(AttendanceFile);
            if (doc == null) return 0;

            return doc.Descendants()
                .Where(x => x.Elements().Any() || x.HasAttributes)
                .Where(x => FieldEqualsAny(x, identityValues,
                    "ParticipantUsername", "participantUsername",
                    "ParticipantEmail", "participantEmail",
                    "Username", "UserName", "username", "userName",
                    "Email", "UserEmail", "email", "userEmail"))
                .Count(x => ValueMatchesAny(ReadValue(x, "Status", "status"), "Missing", "No Show", "No-Show", "Absent"));
        }

        private ParticipantOverride GetLatestOverride(IEnumerable<string> participantIdentityValues, string adminUniversity)
        {
            XDocument doc = LoadXml(OverridesFile);
            if (doc == null) return null;

            return doc.Descendants("Override")
                .Where(x => UniversityMatches(x, adminUniversity))
                .Where(x => FieldEqualsAny(x, participantIdentityValues,
                    "ParticipantUsername", "participantUsername",
                    "ParticipantEmail", "participantEmail",
                    "ParticipantId", "participantId"))
                .Select(x => new ParticipantOverride
                {
                    CompletedLessons = ParseIntOrNull(ReadValue(x, "CompletedLessons", "completedLessons")),
                    EnrolledSessions = ParseIntOrNull(ReadValue(x, "EnrolledSessions", "enrolledSessions")),
                    BadgesEarned = ParseIntOrNull(ReadValue(x, "BadgesEarned", "badgesEarned")),
                    NoShows = ParseIntOrNull(ReadValue(x, "NoShows", "noShows")),
                    OverriddenAtUtc = ParseDate(ReadValue(x, "OverriddenAtUtc", "overriddenAtUtc"))
                })
                .OrderByDescending(x => x.OverriddenAtUtc)
                .FirstOrDefault();
        }

        private string GetCurrentAdminUniversity()
        {
            List<string> identityValues = GetCurrentIdentityValues();
            if (!identityValues.Any()) return string.Empty;

            XDocument usersDoc = LoadXml(UsersFile);
            if (usersDoc == null) return string.Empty;

            XElement adminNode = usersDoc.Descendants()
                .Where(x => x.Elements().Any() || x.HasAttributes)
                .FirstOrDefault(x =>
                    IsUniversityAdminRole(x) &&
                    FieldEqualsAny(x, identityValues,
                        "Username", "UserName", "username", "userName",
                        "Email", "UserEmail", "email", "userEmail",
                        "LoginEmail", "loginEmail",
                        "Id", "UserId", "id", "userId"));

            if (adminNode == null) return string.Empty;

            return FirstNonEmpty(ReadValue(adminNode, "University", "School", "College", "university"));
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

        private List<string> GetIdentityValuesFromUserNode(XElement userNode)
        {
            var values = new List<string>();

            AddIfNotBlank(values, ReadValue(userNode, "Username", "UserName", "username", "userName"));
            AddIfNotBlank(values, ReadValue(userNode, "Email", "UserEmail", "email", "userEmail"));
            AddIfNotBlank(values, ReadValue(userNode, "Id", "UserId", "id", "userId"));

            return values
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Select(v => v.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private bool IsParticipantRole(XElement element)
        {
            string role = ReadValue(element, "Role", "role", "UserRole", "userRole");
            return role.Equals("Participant", StringComparison.OrdinalIgnoreCase) ||
                   element.Name.LocalName.Equals("Participant", StringComparison.OrdinalIgnoreCase);
        }

        private bool IsUniversityAdminRole(XElement element)
        {
            string role = ReadValue(element, "Role", "role", "UserRole", "userRole");

            return role.Equals("UniversityAdmin", StringComparison.OrdinalIgnoreCase) ||
                   role.Equals("University Admin", StringComparison.OrdinalIgnoreCase) ||
                   element.Name.LocalName.Equals("UniversityAdmin", StringComparison.OrdinalIgnoreCase);
        }

        private bool UniversityMatches(XElement element, string expectedUniversity)
        {
            string university = ReadValue(element, "University", "School", "College", "university");
            return !string.IsNullOrWhiteSpace(university) &&
                   university.Equals(expectedUniversity, StringComparison.OrdinalIgnoreCase);
        }

        private XDocument LoadXml(string virtualPath)
        {
            string physicalPath = Server.MapPath(virtualPath);
            if (!File.Exists(physicalPath)) return null;
            return XDocument.Load(physicalPath);
        }

        private string ReadValue(XElement element, params string[] names)
        {
            if (element == null || names == null || names.Length == 0) return string.Empty;

            foreach (string name in names)
            {
                XElement child = element.Elements().FirstOrDefault(e => e.Name.LocalName.Equals(name, StringComparison.OrdinalIgnoreCase));
                if (child != null && !string.IsNullOrWhiteSpace(child.Value))
                {
                    return child.Value.Trim();
                }

                XAttribute attr = element.Attributes().FirstOrDefault(a => a.Name.LocalName.Equals(name, StringComparison.OrdinalIgnoreCase));
                if (attr != null && !string.IsNullOrWhiteSpace(attr.Value))
                {
                    return attr.Value.Trim();
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

        private bool ValueMatchesAny(string currentValue, params string[] matches)
        {
            if (string.IsNullOrWhiteSpace(currentValue)) return false;
            return matches.Any(m => currentValue.Equals(m, StringComparison.OrdinalIgnoreCase));
        }

        private string BuildFullName(XElement element)
        {
            string first = ReadValue(element, "FirstName", "firstName", "FName", "fname");
            string last = ReadValue(element, "LastName", "lastName", "LName", "lname");
            string full = (first + " " + last).Trim();
            return string.IsNullOrWhiteSpace(full) ? string.Empty : full;
        }

        private string FormatDate(string rawDate)
        {
            DateTime parsed;
            return DateTime.TryParse(rawDate, out parsed)
                ? parsed.ToString("MMMM d, yyyy")
                : "Not available";
        }

        private int? ParseIntOrNull(string value)
        {
            int parsed;
            return int.TryParse(value, out parsed) ? parsed : (int?)null;
        }

        private DateTime ParseDate(string value)
        {
            DateTime parsed;
            return DateTime.TryParse(value, out parsed) ? parsed : DateTime.MinValue;
        }

        private bool ContainsIgnoreCase(string source, string search)
        {
            if (string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(search)) return false;
            return source.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
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

        private void ShowError(string message)
        {
            pnlContent.Visible = false;
            pnlError.Visible = true;
            lblError.Text = HttpUtility.HtmlEncode(message);
        }

        private class ParticipantRow
        {
            public string FullName { get; set; }
            public string Username { get; set; }
            public string Email { get; set; }
            public string JoinedText { get; set; }
            public int CompletedLessons { get; set; }
            public int EnrolledSessions { get; set; }
            public int BadgesEarned { get; set; }
            public int NoShows { get; set; }
            public string DetailUrl { get; set; }
        }

        private class ParticipantStats
        {
            public int CompletedLessons { get; set; }
            public int EnrolledSessions { get; set; }
            public int BadgesEarned { get; set; }
            public int NoShows { get; set; }
        }

        private class ParticipantOverride
        {
            public int? CompletedLessons { get; set; }
            public int? EnrolledSessions { get; set; }
            public int? BadgesEarned { get; set; }
            public int? NoShows { get; set; }
            public DateTime OverriddenAtUtc { get; set; }
        }
    }
}
