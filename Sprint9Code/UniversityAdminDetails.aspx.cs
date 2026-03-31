using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace CyberApp_FIA.Account.UniversityAdmin
{
    public partial class UniversityAdminParticipantDetails : System.Web.UI.Page
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
                LoadParticipantDetail();
            }
        }

        protected void btnSaveOverride_Click(object sender, EventArgs e)
        {
            SaveOverride();
        }

        private void LoadParticipantDetail()
        {
            string adminUniversity = GetCurrentAdminUniversity();
            if (string.IsNullOrWhiteSpace(adminUniversity))
            {
                ShowError("A University Admin account or university scope could not be found for the current session.");
                return;
            }

            string participantKey = (Request.QueryString["participantKey"] ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(participantKey))
            {
                ShowError("No participant was selected.");
                return;
            }

            XElement participantNode = FindParticipantNode(adminUniversity, participantKey);
            if (participantNode == null)
            {
                ShowError("That participant could not be found in your university scope.");
                return;
            }

            List<string> identityValues = GetIdentityValuesFromUserNode(participantNode);

            string fullName = FirstNonEmpty(
                ReadValue(participantNode, "FullName", "fullName", "Name", "name"),
                BuildFullName(participantNode),
                "Participant");

            lblParticipantNameHeader.Text = fullName;
            lblParticipantName.Text = fullName;
            lblParticipantUsername.Text = FirstNonEmpty(ReadValue(participantNode, "Username", "UserName", "username", "userName"), "Not available");
            lblParticipantEmail.Text = FirstNonEmpty(ReadValue(participantNode, "Email", "UserEmail", "email", "userEmail"), "Not available");
            lblParticipantUniversity.Text = FirstNonEmpty(ReadValue(participantNode, "University", "School", "College", "university"), "Not available");
            lblParticipantJoined.Text = FormatDate(ReadValue(participantNode, "JoinedDate", "CreatedDate", "DateCreated", "joinedDate", "createdDate"));

            ParticipantStats baseStats = GetBaseParticipantStats(identityValues);
            ParticipantOverride latestOverride = GetLatestOverride(identityValues, adminUniversity);
            ParticipantStats effectiveStats = ApplyOverride(baseStats, latestOverride);

            lblCompletedLessons.Text = effectiveStats.CompletedLessons.ToString();
            lblEnrolledSessions.Text = effectiveStats.EnrolledSessions.ToString();
            lblBadgesEarned.Text = effectiveStats.BadgesEarned.ToString();
            lblNoShows.Text = effectiveStats.NoShows.ToString();

            if (latestOverride == null)
            {
                lblOverrideStatus.Text = "No manual override is currently applied.";
            }
            else
            {
                lblOverrideStatus.Text = "Latest override applied on " +
                    latestOverride.OverriddenAtUtc.ToLocalTime().ToString("MMMM d, yyyy h:mm tt") +
                    " by " + FirstNonEmpty(latestOverride.OverriddenBy, "an admin") +
                    ". Reason: " + FirstNonEmpty(latestOverride.Reason, "No reason recorded");
            }

            txtCompletedLessonsOverride.Text = effectiveStats.CompletedLessons.ToString();
            txtEnrolledSessionsOverride.Text = effectiveStats.EnrolledSessions.ToString();
            txtBadgesOverride.Text = effectiveStats.BadgesEarned.ToString();
            txtNoShowsOverride.Text = effectiveStats.NoShows.ToString();

            List<ParticipantLogRow> logs = BuildParticipantLogs(identityValues, adminUniversity);
            rptLogs.DataSource = logs;
            rptLogs.DataBind();
            pnlNoLogs.Visible = logs.Count == 0;

            List<ParticipantLogRow> overrides = BuildOverrideHistory(identityValues, adminUniversity);
            rptOverrides.DataSource = overrides;
            rptOverrides.DataBind();
            pnlNoOverrides.Visible = overrides.Count == 0;

            pnlContent.Visible = true;
            pnlError.Visible = false;
        }

        private void SaveOverride()
        {
            pnlSuccess.Visible = false;

            string adminUniversity = GetCurrentAdminUniversity();
            if (string.IsNullOrWhiteSpace(adminUniversity))
            {
                ShowError("Could not determine the current University Admin scope.");
                return;
            }

            string participantKey = (Request.QueryString["participantKey"] ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(participantKey))
            {
                ShowError("No participant was selected.");
                return;
            }

            XElement participantNode = FindParticipantNode(adminUniversity, participantKey);
            if (participantNode == null)
            {
                ShowError("That participant could not be found in your university scope.");
                return;
            }

            int? completedLessons = ParseStrictNonNegativeInt(txtCompletedLessonsOverride.Text.Trim());
            int? enrolledSessions = ParseStrictNonNegativeInt(txtEnrolledSessionsOverride.Text.Trim());
            int? badgesEarned = ParseStrictNonNegativeInt(txtBadgesOverride.Text.Trim());
            int? noShows = ParseStrictNonNegativeInt(txtNoShowsOverride.Text.Trim());
            string reason = txtOverrideReason.Text.Trim();

            if (!completedLessons.HasValue || !enrolledSessions.HasValue || !badgesEarned.HasValue || !noShows.HasValue)
            {
                ShowError("All override stat fields must contain valid non-negative whole numbers.");
                return;
            }

            if (string.IsNullOrWhiteSpace(reason))
            {
                ShowError("A reason is required for the override.");
                return;
            }

            string adminIdentity = FirstNonEmpty(
                Convert.ToString(Session["Username"]),
                Convert.ToString(Session["UserName"]),
                Convert.ToString(Session["Email"]),
                Convert.ToString(Session["UserEmail"]),
                Convert.ToString(Session["CurrentUser"]),
                Convert.ToString(Session["CurrentUserEmail"]),
                Context != null && Context.User != null && Context.User.Identity != null ? Context.User.Identity.Name : string.Empty,
                "UniversityAdmin");

            string participantUsername = ReadValue(participantNode, "Username", "UserName", "username", "userName");
            string participantEmail = ReadValue(participantNode, "Email", "UserEmail", "email", "userEmail");
            string participantId = ReadValue(participantNode, "Id", "UserId", "id", "userId");

            string physicalPath = Server.MapPath(OverridesFile);
            XDocument doc;

            if (File.Exists(physicalPath))
            {
                doc = XDocument.Load(physicalPath);
            }
            else
            {
                doc = new XDocument(new XElement("ParticipantStatOverrides"));
            }

            XElement root = doc.Root ?? new XElement("ParticipantStatOverrides");
            if (doc.Root == null)
            {
                doc.Add(root);
            }

            XElement overrideNode = new XElement("Override",
                new XElement("OverrideId", Guid.NewGuid().ToString()),
                new XElement("University", adminUniversity),
                new XElement("ParticipantUsername", participantUsername),
                new XElement("ParticipantEmail", participantEmail),
                new XElement("ParticipantId", participantId),
                new XElement("CompletedLessons", completedLessons.Value),
                new XElement("EnrolledSessions", enrolledSessions.Value),
                new XElement("BadgesEarned", badgesEarned.Value),
                new XElement("NoShows", noShows.Value),
                new XElement("Reason", reason),
                new XElement("OverriddenBy", adminIdentity),
                new XElement("OverriddenAtUtc", DateTime.UtcNow.ToString("o"))
            );

            root.Add(overrideNode);
            doc.Save(physicalPath);

            lblSuccess.Text = "Manual override saved successfully. The participant detail view now reflects the newest override.";
            pnlSuccess.Visible = true;

            txtOverrideReason.Text = string.Empty;

            LoadParticipantDetail();
        }

        private XElement FindParticipantNode(string adminUniversity, string participantKey)
        {
            XDocument usersDoc = LoadXml(UsersFile);
            if (usersDoc == null) return null;

            return usersDoc.Descendants()
                .Where(x => x.Elements().Any() || x.HasAttributes)
                .FirstOrDefault(x =>
                    IsParticipantRole(x) &&
                    UniversityMatches(x, adminUniversity) &&
                    FieldEqualsAny(x, new[] { participantKey },
                        "Username", "UserName", "username", "userName",
                        "Email", "UserEmail", "email", "userEmail",
                        "Id", "UserId", "id", "userId"));
        }

        private ParticipantStats GetBaseParticipantStats(IEnumerable<string> identityValues)
        {
            return new ParticipantStats
            {
                CompletedLessons = CountCompletedLessons(identityValues),
                EnrolledSessions = CountEnrolledSessions(identityValues),
                BadgesEarned = CountBadges(identityValues),
                NoShows = CountNoShows(identityValues)
            };
        }

        private ParticipantStats ApplyOverride(ParticipantStats baseStats, ParticipantOverride latestOverride)
        {
            if (latestOverride == null) return baseStats;

            return new ParticipantStats
            {
                CompletedLessons = latestOverride.CompletedLessons ?? baseStats.CompletedLessons,
                EnrolledSessions = latestOverride.EnrolledSessions ?? baseStats.EnrolledSessions,
                BadgesEarned = latestOverride.BadgesEarned ?? baseStats.BadgesEarned,
                NoShows = latestOverride.NoShows ?? baseStats.NoShows
            };
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
                    Reason = ReadValue(x, "Reason", "reason"),
                    OverriddenBy = ReadValue(x, "OverriddenBy", "overriddenBy"),
                    OverriddenAtUtc = ParseDate(ReadValue(x, "OverriddenAtUtc", "overriddenAtUtc"))
                })
                .OrderByDescending(x => x.OverriddenAtUtc)
                .FirstOrDefault();
        }

        private List<ParticipantLogRow> BuildParticipantLogs(IEnumerable<string> identityValues, string adminUniversity)
        {
            var rows = new List<ParticipantLogRow>();

            XDocument attendanceDoc = LoadXml(AttendanceFile);
            if (attendanceDoc != null)
            {
                rows.AddRange(attendanceDoc.Descendants()
                    .Where(x => x.Elements().Any() || x.HasAttributes)
                    .Where(x => FieldEqualsAny(x, identityValues,
                        "ParticipantUsername", "participantUsername",
                        "ParticipantEmail", "participantEmail",
                        "Username", "UserName", "username", "userName",
                        "Email", "UserEmail", "email", "userEmail"))
                    .Select(x => new ParticipantLogRow
                    {
                        SortDate = ParseDate(FirstNonEmpty(
                            ReadValue(x, "UpdatedDate", "updatedDate"),
                            ReadValue(x, "AttendanceDate", "attendanceDate"),
                            ReadValue(x, "Date", "date"),
                            ReadValue(x, "CreatedDate", "createdDate"))),
                        DateText = FormatDateTime(FirstNonEmpty(
                            ReadValue(x, "UpdatedDate", "updatedDate"),
                            ReadValue(x, "AttendanceDate", "attendanceDate"),
                            ReadValue(x, "Date", "date"),
                            ReadValue(x, "CreatedDate", "createdDate"))),
                        Type = "Attendance",
                        Summary = "Session: " + FirstNonEmpty(ReadValue(x, "Title", "title"), ReadValue(x, "SessionId", "sessionId"), "Unknown session") +
                                  " | Status: " + FirstNonEmpty(ReadValue(x, "Status", "status"), "Unknown") +
                                  " | Helper: " + FirstNonEmpty(ReadValue(x, "HelperUsername", "helperUsername", "AssignedHelper", "assignedHelper"), "Not recorded"),
                        Source = "attendance.xml"
                    }));
            }

            XDocument completionDoc = LoadXml(CompletionsFile);
            if (completionDoc != null)
            {
                rows.AddRange(completionDoc.Descendants()
                    .Where(x => x.Elements().Any() || x.HasAttributes)
                    .Where(x => FieldEqualsAny(x, identityValues,
                        "ParticipantUsername", "participantUsername",
                        "ParticipantEmail", "participantEmail",
                        "Username", "UserName", "username", "userName",
                        "Email", "UserEmail", "email", "userEmail"))
                    .Select(x => new ParticipantLogRow
                    {
                        SortDate = ParseDate(FirstNonEmpty(
                            ReadValue(x, "CompletedDate", "completedDate"),
                            ReadValue(x, "Date", "date"),
                            ReadValue(x, "CreatedDate", "createdDate"))),
                        DateText = FormatDateTime(FirstNonEmpty(
                            ReadValue(x, "CompletedDate", "completedDate"),
                            ReadValue(x, "Date", "date"),
                            ReadValue(x, "CreatedDate", "createdDate"))),
                        Type = "Completion",
                        Summary = "Lesson: " + FirstNonEmpty(ReadValue(x, "LessonId", "lessonId", "CourseId", "courseId", "MicrocourseId", "microcourseId"), "Unknown lesson") +
                                  " | Session: " + FirstNonEmpty(ReadValue(x, "SessionId", "sessionId"), "Unknown session") +
                                  " | Status: " + FirstNonEmpty(ReadValue(x, "Status", "status"), "Recorded"),
                        Source = "lessonCompletions.xml"
                    }));
            }

            XDocument badgesDoc = LoadXml(BadgesFile);
            if (badgesDoc != null)
            {
                rows.AddRange(badgesDoc.Descendants()
                    .Where(x => x.Elements().Any() || x.HasAttributes)
                    .Where(x => FieldEqualsAny(x, identityValues,
                        "AwardedToUsername", "awardedToUsername",
                        "AwardedToEmail", "awardedToEmail",
                        "ParticipantUsername", "participantUsername",
                        "ParticipantEmail", "participantEmail",
                        "Username", "UserName", "username", "userName",
                        "Email", "UserEmail", "email", "userEmail"))
                    .Select(x => new ParticipantLogRow
                    {
                        SortDate = ParseDate(FirstNonEmpty(
                            ReadValue(x, "AwardedDate", "awardedDate"),
                            ReadValue(x, "Date", "date"),
                            ReadValue(x, "CreatedDate", "createdDate"))),
                        DateText = FormatDateTime(FirstNonEmpty(
                            ReadValue(x, "AwardedDate", "awardedDate"),
                            ReadValue(x, "Date", "date"),
                            ReadValue(x, "CreatedDate", "createdDate"))),
                        Type = "Badge",
                        Summary = "Badge: " + FirstNonEmpty(ReadValue(x, "BadgeName", "badgeName", "Title", "title"), "Unknown badge") +
                                  " | Status: " + FirstNonEmpty(ReadValue(x, "Status", "status"), "Awarded"),
                        Source = "badges.xml"
                    }));
            }

            List<ParticipantLogRow> overrideRows = BuildOverrideHistory(identityValues, adminUniversity);
            rows.AddRange(overrideRows);

            return rows
                .OrderByDescending(x => x.SortDate)
                .ThenByDescending(x => x.DateText)
                .Take(30)
                .ToList();
        }

        private List<ParticipantLogRow> BuildOverrideHistory(IEnumerable<string> participantIdentityValues, string adminUniversity)
        {
            XDocument doc = LoadXml(OverridesFile);
            if (doc == null) return new List<ParticipantLogRow>();

            return doc.Descendants("Override")
                .Where(x => UniversityMatches(x, adminUniversity))
                .Where(x => FieldEqualsAny(x, participantIdentityValues,
                    "ParticipantUsername", "participantUsername",
                    "ParticipantEmail", "participantEmail",
                    "ParticipantId", "participantId"))
                .Select(x => new ParticipantLogRow
                {
                    SortDate = ParseDate(ReadValue(x, "OverriddenAtUtc", "overriddenAtUtc")),
                    DateText = FormatDateTime(ReadValue(x, "OverriddenAtUtc", "overriddenAtUtc")),
                    Type = "Override",
                    Summary = "Completed Lessons=" + FirstNonEmpty(ReadValue(x, "CompletedLessons", "completedLessons"), "-") +
                              ", Enrolled Sessions=" + FirstNonEmpty(ReadValue(x, "EnrolledSessions", "enrolledSessions"), "-") +
                              ", Badges Earned=" + FirstNonEmpty(ReadValue(x, "BadgesEarned", "badgesEarned"), "-") +
                              ", No Shows=" + FirstNonEmpty(ReadValue(x, "NoShows", "noShows"), "-") +
                              " | Reason: " + FirstNonEmpty(ReadValue(x, "Reason", "reason"), "No reason recorded"),
                    Source = FirstNonEmpty(ReadValue(x, "OverriddenBy", "overriddenBy"), "Unknown admin")
                })
                .OrderByDescending(x => x.SortDate)
                .ToList();
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

        private string FormatDateTime(string rawDate)
        {
            DateTime parsed;
            return DateTime.TryParse(rawDate, out parsed)
                ? parsed.ToLocalTime().ToString("MMMM d, yyyy h:mm tt")
                : "Date not available";
        }

        private int? ParseIntOrNull(string value)
        {
            int parsed;
            return int.TryParse(value, out parsed) ? parsed : (int?)null;
        }

        private int? ParseStrictNonNegativeInt(string value)
        {
            int parsed;
            if (int.TryParse(value, out parsed) && parsed >= 0)
            {
                return parsed;
            }

            return null;
        }

        private DateTime ParseDate(string value)
        {
            DateTime parsed;
            return DateTime.TryParse(value, out parsed) ? parsed : DateTime.MinValue;
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
            public string Reason { get; set; }
            public string OverriddenBy { get; set; }
            public DateTime OverriddenAtUtc { get; set; }
        }

        private class ParticipantLogRow
        {
            public DateTime SortDate { get; set; }
            public string DateText { get; set; }
            public string Type { get; set; }
            public string Summary { get; set; }
            public string Source { get; set; }
        }
    }
}
