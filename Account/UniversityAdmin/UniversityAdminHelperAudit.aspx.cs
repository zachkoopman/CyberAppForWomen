using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using CyberApp_FIA.Services;

namespace CyberApp_FIA.Account
{
    public partial class UniversityAdminHelperAudit : Page
    {
        private const int PageSize = 25;

        private string AuditXmlPath => Server.MapPath("~/App_Data/Audit_Log/UnvAdminAudit.xml");
        private string UsersXmlPath => Server.MapPath("~/App_Data/users.xml");
        private string HelperProgressXmlPath => Server.MapPath("~/App_Data/helperProgress.xml");

        private string HelperEmailQuery =>
            (Request.QueryString["helperEmail"] ?? string.Empty).Trim();

        private int CurrentPageIndex
        {
            get => ViewState["HelperAuditPageIndex"] is int i ? i : 0;
            set => ViewState["HelperAuditPageIndex"] = value < 0 ? 0 : value;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Only University Admins can use this view
                var role = (string)Session["Role"];
                if (!string.Equals(role, "UniversityAdmin", StringComparison.OrdinalIgnoreCase))
                {
                    Response.Redirect("~/Account/Login.aspx");
                    return;
                }

                var helperEmail = HelperEmailQuery;
                if (string.IsNullOrWhiteSpace(helperEmail))
                {
                    Response.Redirect("~/Account/UniversityAdmin/UniversityAdminAudit.aspx");
                    return;
                }

                var adminEmail = (string)Session["Email"] ?? string.Empty;
                WelcomeName.Text = adminEmail.Length > 0 ? adminEmail : "University Admin";

                // Figure out the admin’s university (from session or users.xml)
                var adminUni = (string)Session["University"];
                if (string.IsNullOrWhiteSpace(adminUni))
                {
                    adminUni = LookupUniversityByEmail(adminEmail);
                }

                // Look up helper and make sure they belong to this university
                if (!TryBindHelperHeader(helperEmail, adminUni, out var helperUni, out var helperId))
                {
                    // Either helper not found or belongs to another university
                    Response.Redirect("~/Account/UniversityAdmin/UniversityAdminAudit.aspx");
                    return;
                }

                UniversityValue.Value = helperUni ?? string.Empty;
                HelperIdValue.Value = helperId ?? string.Empty;

                EnsureAuditXml();
                InitializeFilters(helperEmail, helperUni);
                BindAuditLog();
                BindVerificationCards();
                BindSessionReviewCards();
            }
        }

        protected void BtnBack_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Account/UniversityAdmin/UniversityAdminAudit.aspx");
        }

        protected void BtnApplyFilters_Click(object sender, EventArgs e)
        {
            CurrentPageIndex = 0;
            BindAuditLog();
        }

        protected void BtnClearFilters_Click(object sender, EventArgs e)
        {
            TxtSearch.Text = string.Empty;
            TxtFromTime.Text = string.Empty;
            TxtToTime.Text = string.Empty;

            if (DdlTypeFilter.Items.Count > 0) DdlTypeFilter.SelectedIndex = 0;

            CurrentPageIndex = 0;
            BindAuditLog();
        }

        protected void BtnPrevPage_Click(object sender, EventArgs e)
        {
            if (CurrentPageIndex > 0)
            {
                CurrentPageIndex--;
                BindAuditLog();
            }
        }

        protected void BtnNextPage_Click(object sender, EventArgs e)
        {
            CurrentPageIndex++;
            BindAuditLog();
        }

        // Export currently filtered (all pages) to CSV
        protected void BtnExportCsv_Click(object sender, EventArgs e)
        {
            var rows = GetFilteredAuditRows();

            Response.Clear();
            Response.ContentType = "text/csv";
            var fileName = $"FIA_HelperAudit_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";
            Response.AddHeader("Content-Disposition", "attachment; filename=" + fileName);

            using (var writer = new StringWriter())
            {
                writer.WriteLine("LocalTimestamp,Role,Type,FirstName,Email,Details");

                foreach (var row in rows)
                {
                    var ts = row.TimestampLocal != DateTime.MinValue
                        ? row.TimestampLocal.ToString("yyyy-MM-dd HH:mm:ss")
                        : "";

                    writer.WriteLine(string.Join(",",
                        CsvEscape(ts),
                        CsvEscape(row.Role),
                        CsvEscape(row.Type),
                        CsvEscape(row.FirstName),
                        CsvEscape(row.Email),
                        CsvEscape(row.Details)
                    ));
                }

                Response.Write(writer.ToString());
            }

            Response.End();
        }

        protected void VerificationRepeater_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            var helperId = HelperIdValue.Value;
            if (string.IsNullOrWhiteSpace(helperId))
            {
                return;
            }

            var courseId = e.CommandArgument as string;
            if (string.IsNullOrWhiteSpace(courseId))
            {
                return;
            }

            string newStatus;
            string auditType;

            if (string.Equals(e.CommandName, "markVerified", StringComparison.OrdinalIgnoreCase))
            {
                newStatus = "Verified";
                auditType = "Helper Certification Verified";
            }
            else if (string.Equals(e.CommandName, "markQuestioned", StringComparison.OrdinalIgnoreCase))
            {
                newStatus = "Questioned";
                auditType = "Helper Certification Questioned";
            }
            else
            {
                return;
            }

            // read admin note from card
            string adminNote = string.Empty;
            var noteBox = (TextBox)e.Item.FindControl("TxtAdminNote");
            if (noteBox != null)
            {
                adminNote = (noteBox.Text ?? string.Empty).Trim();
            }

            UpdateVerificationStatus(helperId, courseId, newStatus, adminNote);

            // log admin decision into the audit log
            try
            {
                var details = $"Admin set certification verification to '{newStatus}' for helperId={helperId}, courseId={courseId}.";
                if (!string.IsNullOrWhiteSpace(adminNote))
                {
                    var preview = adminNote;
                    if (preview.Length > 180) preview = preview.Substring(0, 177) + "...";
                    details += " Admin note: " + preview;
                }

                UniversityAuditLogger.AppendForCurrentUser(this, auditType, details);
            }
            catch
            {
                // Do not block UI if audit logging fails.
            }

            // Refresh both verification cards and the table below so the admin sees latest state.
            BindVerificationCards();
            BindSessionReviewCards();
            BindAuditLog();
        }

        protected void SessionReviewRepeater_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            var helperId = HelperIdValue.Value;
            if (string.IsNullOrWhiteSpace(helperId))
            {
                return;
            }

            var courseId = e.CommandArgument as string;
            if (string.IsNullOrWhiteSpace(courseId))
            {
                return;
            }

            var noteBox = (TextBox)e.Item.FindControl("TxtSessionReviewNote");
            var adminNote = noteBox != null ? (noteBox.Text ?? string.Empty).Trim() : string.Empty;

            string scope;      // "Teaching" or "Help"
            string newStatus;  // "Ok" or "Questioned"
            string auditType;

            if (string.Equals(e.CommandName, "verifyTeaching", StringComparison.OrdinalIgnoreCase))
            {
                scope = "Teaching";
                newStatus = "Ok";
                auditType = "Helper Teaching Session Verified";
            }
            else if (string.Equals(e.CommandName, "questionTeaching", StringComparison.OrdinalIgnoreCase))
            {
                scope = "Teaching";
                newStatus = "Questioned";
                auditType = "Helper Teaching Session Questioned";
            }
            else if (string.Equals(e.CommandName, "verifyHelp", StringComparison.OrdinalIgnoreCase))
            {
                scope = "Help";
                newStatus = "Ok";
                auditType = "Helper 1:1 Help Session Verified";
            }
            else if (string.Equals(e.CommandName, "questionHelp", StringComparison.OrdinalIgnoreCase))
            {
                scope = "Help";
                newStatus = "Questioned";
                auditType = "Helper 1:1 Help Session Questioned";
            }
            else
            {
                return;
            }

            UpdateSessionReviewStatus(helperId, courseId, scope, newStatus, adminNote, auditType);

            // Refresh both sections so admin sees updated state
            BindVerificationCards();
            BindSessionReviewCards();
            BindAuditLog();
        }

        private static string CsvEscape(string s)
        {
            if (s == null) return "";
            if (s.Contains("\"") || s.Contains(",") || s.Contains("\r") || s.Contains("\n"))
            {
                return "\"" + s.Replace("\"", "\"\"") + "\"";
            }
            return s;
        }

        private void EnsureAuditXml()
        {
            var dir = Path.GetDirectoryName(AuditXmlPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            if (!File.Exists(AuditXmlPath))
            {
                var init = "<?xml version='1.0' encoding='utf-8'?><auditLog version='1'></auditLog>";
                File.WriteAllText(AuditXmlPath, init);
            }
        }

        private void EnsureHelperProgressXml()
        {
            var dir = Path.GetDirectoryName(HelperProgressXmlPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            if (!File.Exists(HelperProgressXmlPath))
            {
                var init = "<?xml version='1.0' encoding='utf-8'?><helperProgress version='1'></helperProgress>";
                File.WriteAllText(HelperProgressXmlPath, init);
            }
        }

        /// <summary>
        /// Bind helper name/email/university at top of page.
        /// Returns false if helper not found or not in this admin’s university.
        /// </summary>
        private bool TryBindHelperHeader(string helperEmail, string adminUni, out string helperUni, out string helperId)
        {
            helperUni = string.Empty;
            helperId = string.Empty;

            try
            {
                if (string.IsNullOrWhiteSpace(helperEmail) || !File.Exists(UsersXmlPath))
                    return false;

                var doc = new XmlDocument();
                doc.Load(UsersXmlPath);
                var emailLower = helperEmail.ToLowerInvariant();

                var node = doc.SelectSingleNode(
                    $"/users/user[translate(email,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz')='{emailLower}']"
                ) as XmlElement;

                if (node == null)
                {
                    return false;
                }

                // Read helper id so we can look up progress in helperProgress.xml
                helperId = node.GetAttribute("id") ?? string.Empty;

                var roleAttr = node.GetAttribute("role") ?? string.Empty;
                if (!string.Equals(roleAttr, "Helper", StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                helperUni = node["university"]?.InnerText ?? string.Empty;

                // Security: helper must belong to same university as this admin
                if (!string.IsNullOrWhiteSpace(adminUni) &&
                    !string.Equals(adminUni, helperUni, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                var firstName = node["firstName"]?.InnerText ?? "";
                var lastName = node["lastName"]?.InnerText ?? "";

                var displayName = (firstName + " " + lastName).Trim();
                if (string.IsNullOrWhiteSpace(displayName))
                {
                    displayName = helperEmail;
                }

                HelperName.Text = displayName;
                HelperEmailLiteral.Text = helperEmail;
                HelperUniversityLiteral.Text = string.IsNullOrWhiteSpace(helperUni)
                    ? "(university not set)"
                    : helperUni;

                return true;
            }
            catch
            {
                return false;
            }
        }

        private void InitializeFilters(string helperEmail, string helperUni)
        {
            DdlTypeFilter.Items.Clear();
            DdlTypeFilter.Items.Add(new ListItem("All types", ""));

            var types = GetHelperAuditTypes(helperEmail, helperUni);
            foreach (var t in types.OrderBy(x => x))
            {
                DdlTypeFilter.Items.Add(new ListItem(t, t));
            }
        }

        private HashSet<string> GetHelperAuditTypes(string helperEmail, string helperUni)
        {
            var types = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            try
            {
                if (File.Exists(AuditXmlPath) &&
                    !string.IsNullOrWhiteSpace(helperEmail) &&
                    !string.IsNullOrWhiteSpace(helperUni))
                {
                    var doc = new XmlDocument();
                    doc.Load(AuditXmlPath);
                    var nodes = doc.SelectNodes("/auditLog/entry");

                    foreach (XmlElement entry in nodes)
                    {
                        var entryUni = entry.GetAttribute("university");
                        if (!string.Equals(entryUni, helperUni, StringComparison.OrdinalIgnoreCase))
                            continue;

                        var emailAttr = entry.GetAttribute("email");
                        if (!string.Equals(emailAttr, helperEmail, StringComparison.OrdinalIgnoreCase))
                            continue;

                        var roleAttr = entry.GetAttribute("role") ?? string.Empty;
                        if (!string.Equals(roleAttr, "Helper", StringComparison.OrdinalIgnoreCase))
                            continue;

                        var typeAttr = entry.GetAttribute("type");
                        if (!string.IsNullOrWhiteSpace(typeAttr))
                        {
                            types.Add(typeAttr.Trim());
                        }
                    }
                }
            }
            catch
            {
                // fall through with whatever we've collected
            }

            return types;
        }

        private List<AuditRow> GetFilteredAuditRows()
        {
            var helperEmail = HelperEmailQuery;
            var uni = UniversityValue.Value ?? string.Empty;
            var rows = new List<AuditRow>();

            try
            {
                if (!string.IsNullOrWhiteSpace(helperEmail) &&
                    !string.IsNullOrWhiteSpace(uni) &&
                    File.Exists(AuditXmlPath))
                {
                    var doc = new XmlDocument();
                    doc.Load(AuditXmlPath);
                    var nodes = doc.SelectNodes("/auditLog/entry");

                    foreach (XmlElement entry in nodes)
                    {
                        var entryUni = entry.GetAttribute("university");
                        if (!string.Equals(entryUni, uni, StringComparison.OrdinalIgnoreCase))
                            continue;

                        var emailAttr = entry.GetAttribute("email");
                        if (!string.Equals(emailAttr, helperEmail, StringComparison.OrdinalIgnoreCase))
                            continue;

                        var roleAttr = entry.GetAttribute("role") ?? string.Empty;
                        if (!string.Equals(roleAttr, "Helper", StringComparison.OrdinalIgnoreCase))
                            continue;

                        var timestampRaw = entry.GetAttribute("timestamp");
                        var type = entry.GetAttribute("type");
                        var firstName = entry.GetAttribute("firstName");
                        var details = entry["details"]?.InnerText ?? entry.GetAttribute("details");

                        DateTime tsUtc;
                        DateTime localTs = DateTime.MinValue;
                        string dateLabel = "";
                        string timeLabel = "";

                        if (DateTime.TryParse(timestampRaw, null, DateTimeStyles.AdjustToUniversal, out tsUtc))
                        {
                            localTs = tsUtc.ToLocalTime();
                            dateLabel = localTs.ToString("MMM d", CultureInfo.InvariantCulture);
                            timeLabel = localTs.ToString("h:mm tt", CultureInfo.InvariantCulture);
                        }

                        rows.Add(new AuditRow
                        {
                            TimestampDate = dateLabel,
                            TimestampTime = timeLabel,
                            TimestampLocal = localTs,
                            Role = roleAttr,
                            Type = type,
                            FirstName = firstName,
                            Email = emailAttr,
                            Details = details
                        });
                    }
                }
            }
            catch
            {
                // safe fail to empty rows
            }

            var search = (TxtSearch.Text ?? "").Trim().ToLowerInvariant();
            var typeFilter = (DdlTypeFilter.SelectedValue ?? "").Trim();
            var fromText = (TxtFromTime.Text ?? "").Trim();
            var toText = (TxtToTime.Text ?? "").Trim();

            var fromLocalOpt = ParseLocal(fromText);
            var toLocalOpt = ParseLocal(toText);
            var hasFrom = fromLocalOpt.HasValue;
            var hasTo = toLocalOpt.HasValue;

            IEnumerable<AuditRow> filtered = rows;

            if (!string.IsNullOrEmpty(typeFilter))
            {
                filtered = filtered.Where(r =>
                    string.Equals(r.Type, typeFilter, StringComparison.OrdinalIgnoreCase));
            }

            if (hasFrom)
            {
                filtered = filtered.Where(r => r.TimestampLocal >= fromLocalOpt.Value);
            }

            if (hasTo)
            {
                filtered = filtered.Where(r => r.TimestampLocal <= toLocalOpt.Value);
            }

            if (!string.IsNullOrEmpty(search))
            {
                filtered = filtered.Where(r =>
                    (r.Type ?? "").ToLowerInvariant().Contains(search) ||
                    (r.Details ?? "").ToLowerInvariant().Contains(search));
            }

            return filtered
                .OrderByDescending(r => r.TimestampLocal)
                .ToList();
        }

        private void BindAuditLog()
        {
            var filteredList = GetFilteredAuditRows();

            var totalCount = filteredList.Count;
            var totalPages = totalCount == 0 ? 1 : (int)Math.Ceiling(totalCount / (double)PageSize);

            if (CurrentPageIndex >= totalPages) CurrentPageIndex = totalPages - 1;
            if (CurrentPageIndex < 0) CurrentPageIndex = 0;

            var skip = CurrentPageIndex * PageSize;
            var pageRows = filteredList.Skip(skip).Take(PageSize).ToList();

            NoAuditPlaceholder.Visible = totalCount == 0;
            AuditRepeater.DataSource = pageRows;
            AuditRepeater.DataBind();

            BtnPrevPage.Enabled = CurrentPageIndex > 0;
            BtnNextPage.Enabled = CurrentPageIndex < totalPages - 1;

            if (totalCount == 0)
            {
                LblPageInfo.Text = "No entries yet";
            }
            else
            {
                LblPageInfo.Text = $"Page {CurrentPageIndex + 1} of {totalPages} • {totalCount} entr" +
                                   (totalCount == 1 ? "y" : "ies");
            }
        }

        private void BindVerificationCards()
        {
            var helperId = HelperIdValue.Value;
            if (string.IsNullOrWhiteSpace(helperId))
            {
                VerificationListPlaceholder.Visible = false;
                return;
            }

            EnsureHelperProgressXml();
            if (!File.Exists(HelperProgressXmlPath))
            {
                VerificationListPlaceholder.Visible = false;
                return;
            }

            var items = new List<VerificationItem>();

            var doc = new XmlDocument();
            doc.Load(HelperProgressXmlPath);

            var helperEl = (XmlElement)doc.SelectSingleNode($"/helperProgress/helper[@id='{helperId}']");
            if (helperEl == null)
            {
                VerificationListPlaceholder.Visible = false;
                return;
            }

            var displayName = helperEl["displayName"]?.InnerText ?? string.Empty;

            foreach (XmlElement c in helperEl.SelectNodes("./course"))
            {
                var status = (c["verificationStatus"]?.InnerText ?? "NotRequested").Trim();
                if (string.Equals(status, "NotRequested", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var courseId = c.GetAttribute("id");
                var title = c["title"]?.InnerText ?? courseId;
                var updatedText = c["verificationUpdatedUtc"]?.InnerText ?? c["lastUpdatedUtc"]?.InnerText ?? string.Empty;

                var adminNote = c["verificationAdminNote"]?.InnerText ?? string.Empty;
                var helperNote = c["verificationHelperNote"]?.InnerText ?? string.Empty;
                var submissionKind = (c["verificationSubmissionKind"]?.InnerText ?? "Initial").Trim();

                DateTime updated;
                string updatedLabel = string.Empty;
                if (DateTime.TryParse(updatedText, out updated))
                {
                    updatedLabel = updated.ToLocalTime().ToString("MMM d, yyyy h:mm tt");
                }

                string cssClass;
                string statusLabel;

                if (status.Equals("Verified", StringComparison.OrdinalIgnoreCase))
                {
                    cssClass = "verified";
                    statusLabel = "Verified";
                }
                else if (status.Equals("Questioned", StringComparison.OrdinalIgnoreCase))
                {
                    cssClass = "questioned";
                    statusLabel = "Questioned";
                }
                else
                {
                    cssClass = "pending";
                    statusLabel = "Pending review";
                }

                var isResub = submissionKind.Equals("Resubmission", StringComparison.OrdinalIgnoreCase);
                if (isResub)
                {
                    cssClass += " resubmission";
                }

                var submissionLabel = isResub ? "Resubmission" : "Initial submission";

                items.Add(new VerificationItem
                {
                    CourseId = courseId,
                    CourseTitle = title,
                    Status = status,
                    StatusLabel = statusLabel,
                    CssClass = cssClass,
                    LastUpdatedLabel = updatedLabel,
                    HelperDisplayName = displayName,
                    AdminNote = adminNote,
                    HelperNote = helperNote,
                    HasHelperNote = !string.IsNullOrWhiteSpace(helperNote),
                    HasAdminNote = !string.IsNullOrWhiteSpace(adminNote),
                    IsResubmission = isResub,
                    SubmissionLabel = submissionLabel
                });
            }

            if (items.Count == 0)
            {
                VerificationListPlaceholder.Visible = false;
                return;
            }

            VerificationListPlaceholder.Visible = true;
            VerificationRepeater.DataSource = items.OrderBy(i => i.CourseTitle).ToList();
            VerificationRepeater.DataBind();
        }

        private void BindSessionReviewCards()
        {
            var helperId = HelperIdValue.Value;
            if (string.IsNullOrWhiteSpace(helperId))
            {
                SessionReviewPlaceholder.Visible = false;
                return;
            }

            EnsureHelperProgressXml();
            if (!File.Exists(HelperProgressXmlPath))
            {
                SessionReviewPlaceholder.Visible = false;
                return;
            }

            var items = new List<SessionReviewItem>();

            var doc = new XmlDocument();
            doc.Load(HelperProgressXmlPath);

            var helperEl = (XmlElement)doc.SelectSingleNode($"/helperProgress/helper[@id='{helperId}']");
            if (helperEl == null)
            {
                SessionReviewPlaceholder.Visible = false;
                return;
            }

            foreach (XmlElement c in helperEl.SelectNodes("./course"))
            {
                var courseId = c.GetAttribute("id");
                var title = c["title"]?.InnerText ?? courseId;

                var teaching = ParseIntSafe(c["teachingSessions"]?.InnerText, 0);
                var help = ParseIntSafe(c["helpSessions"]?.InnerText, 0);

                // Only show microcourses that have at least one teaching or 1:1 session logged.
                if (teaching == 0 && help == 0)
                {
                    continue;
                }

                var teachingStatus = (c["teachingReviewStatus"]?.InnerText ?? "Ok").Trim();
                var helpStatus = (c["helpReviewStatus"]?.InnerText ?? "Ok").Trim();

                var teachingLabel = teachingStatus.Equals("Questioned", StringComparison.OrdinalIgnoreCase)
                    ? "Questioned (1 session on hold)"
                    : "OK";

                var helpLabel = helpStatus.Equals("Questioned", StringComparison.OrdinalIgnoreCase)
                    ? "Questioned (1 session on hold)"
                    : "OK";

                var teachingCss = teachingStatus.Equals("Questioned", StringComparison.OrdinalIgnoreCase)
                    ? "color:#b91c1c; font-weight:600;"
                    : "color:#15803d; font-weight:600;";

                var helpCss = helpStatus.Equals("Questioned", StringComparison.OrdinalIgnoreCase)
                    ? "color:#b91c1c; font-weight:600;"
                    : "color:#15803d; font-weight:600;";

                var teachingNote = c["teachingReviewAdminNote"]?.InnerText ?? string.Empty;
                var helpNote = c["helpReviewAdminNote"]?.InnerText ?? string.Empty;

                items.Add(new SessionReviewItem
                {
                    CourseId = courseId,
                    CourseTitle = title,
                    TeachingSessions = teaching,
                    HelpSessions = help,
                    HasTeaching = teaching > 0,
                    HasHelp = help > 0,
                    TeachingStatusLabel = teachingLabel,
                    TeachingStatusCss = teachingCss,
                    TeachingAdminNote = teachingNote,
                    HelpStatusLabel = helpLabel,
                    HelpStatusCss = helpCss,
                    HelpAdminNote = helpNote
                });
            }

            if (items.Count == 0)
            {
                SessionReviewPlaceholder.Visible = false;
                return;
            }

            SessionReviewPlaceholder.Visible = true;
            SessionReviewRepeater.DataSource = items.OrderBy(i => i.CourseTitle).ToList();
            SessionReviewRepeater.DataBind();
        }

        private static int ParseIntSafe(string value, int fallback)
        {
            if (int.TryParse(value ?? string.Empty, out var v))
            {
                return v < 0 ? 0 : v;
            }

            return fallback;
        }

        private static DateTime? ParseLocal(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            if (DateTime.TryParse(s, out var dt)) return dt;
            return null;
        }

        private string LookupUniversityByEmail(string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email) || !File.Exists(UsersXmlPath)) return "";
                var doc = new XmlDocument();
                doc.Load(UsersXmlPath);
                var emailLower = email.ToLowerInvariant();

                var node = doc.SelectSingleNode(
                    $"/users/user[translate(email,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz')='{emailLower}']");
                return node?["university"]?.InnerText ?? "";
            }
            catch
            {
                return "";
            }
        }

        private void UpdateVerificationStatus(string helperId, string courseId, string newStatus, string adminNote)
        {
            EnsureHelperProgressXml();

            var doc = new XmlDocument();
            doc.Load(HelperProgressXmlPath);

            var helperEl = (XmlElement)doc.SelectSingleNode($"/helperProgress/helper[@id='{helperId}']");
            if (helperEl == null) return;

            var courseEl = (XmlElement)helperEl.SelectSingleNode($"./course[@id='{courseId}']");
            if (courseEl == null) return;

            var statusEl = courseEl["verificationStatus"] ?? doc.CreateElement("verificationStatus");
            statusEl.InnerText = newStatus;
            if (statusEl.ParentNode == null) courseEl.AppendChild(statusEl);

            var updatedEl = courseEl["verificationUpdatedUtc"] ?? doc.CreateElement("verificationUpdatedUtc");
            updatedEl.InnerText = DateTime.UtcNow.ToString("o");
            if (updatedEl.ParentNode == null) courseEl.AppendChild(updatedEl);

            // store admin note explaining this decision
            var adminNoteEl = courseEl["verificationAdminNote"] ?? doc.CreateElement("verificationAdminNote");
            adminNoteEl.InnerText = adminNote ?? string.Empty;
            if (adminNoteEl.ParentNode == null) courseEl.AppendChild(adminNoteEl);

            doc.Save(HelperProgressXmlPath);
        }

        private void UpdateSessionReviewStatus(
            string helperId,
            string courseId,
            string scope,
            string newStatus,
            string adminNote,
            string auditType)
        {
            EnsureHelperProgressXml();

            var doc = new XmlDocument();
            doc.Load(HelperProgressXmlPath);

            var helperEl = (XmlElement)doc.SelectSingleNode($"/helperProgress/helper[@id='{helperId}']");
            if (helperEl == null)
            {
                return;
            }

            var courseEl = (XmlElement)helperEl.SelectSingleNode($"./course[@id='{courseId}']");
            if (courseEl == null)
            {
                return;
            }

            // For logging & UI readability
            var courseTitle = (courseEl["title"]?.InnerText ?? courseId).Trim();
            if (string.IsNullOrEmpty(courseTitle))
            {
                courseTitle = courseId;
            }

            var totalsEl = helperEl["totals"] ?? doc.CreateElement("totals");
            if (totalsEl.ParentNode == null) helperEl.AppendChild(totalsEl);

            XmlElement EnsureTotalsChild(string name)
            {
                var n = totalsEl[name] ?? doc.CreateElement(name);
                if (n.ParentNode == null) totalsEl.AppendChild(n);
                return n;
            }

            var totalTeachEl = EnsureTotalsChild("totalTeachingSessions");
            var totalHelpEl = EnsureTotalsChild("totalHelpSessions");

            var totalTeach = ParseIntSafe(totalTeachEl.InnerText, 0);
            var totalHelp = ParseIntSafe(totalHelpEl.InnerText, 0);

            var nowIso = DateTime.UtcNow.ToString("o");

            if (string.Equals(scope, "Teaching", StringComparison.OrdinalIgnoreCase))
            {
                var statusEl = courseEl["teachingReviewStatus"] ?? doc.CreateElement("teachingReviewStatus");
                if (statusEl.ParentNode == null) courseEl.AppendChild(statusEl);
                var prevStatus = (statusEl.InnerText ?? "Ok").Trim();
                statusEl.InnerText = newStatus;

                var noteEl = courseEl["teachingReviewAdminNote"] ?? doc.CreateElement("teachingReviewAdminNote");
                noteEl.InnerText = adminNote ?? string.Empty;
                if (noteEl.ParentNode == null) courseEl.AppendChild(noteEl);

                var flagEl = courseEl["teachingRevokedFlag"] ?? doc.CreateElement("teachingRevokedFlag");
                if (flagEl.ParentNode == null) courseEl.AppendChild(flagEl);
                var prevFlag = (flagEl.InnerText ?? "false").Trim();

                var teachingEl = courseEl["teachingSessions"] ?? doc.CreateElement("teachingSessions");
                if (teachingEl.ParentNode == null) courseEl.AppendChild(teachingEl);
                var teachingCount = ParseIntSafe(teachingEl.InnerText, 0);

                // Question ⇒ subtract 1 once; Verify ⇒ add it back once
                if (newStatus.Equals("Questioned", StringComparison.OrdinalIgnoreCase)
                    && !prevFlag.Equals("true", StringComparison.OrdinalIgnoreCase))
                {
                    if (teachingCount > 0)
                    {
                        teachingCount -= 1;
                        teachingEl.InnerText = teachingCount.ToString();
                        totalTeach = Math.Max(0, totalTeach - 1);
                    }

                    flagEl.InnerText = "true";
                }
                else if (newStatus.Equals("Ok", StringComparison.OrdinalIgnoreCase)
                         && prevFlag.Equals("true", StringComparison.OrdinalIgnoreCase))
                {
                    teachingCount += 1;
                    teachingEl.InnerText = teachingCount.ToString();
                    totalTeach += 1;
                    flagEl.InnerText = "false";
                }

                var updatedEl = courseEl["teachingReviewUpdatedUtc"] ?? doc.CreateElement("teachingReviewUpdatedUtc");
                updatedEl.InnerText = nowIso;
                if (updatedEl.ParentNode == null) courseEl.AppendChild(updatedEl);
            }
            else if (string.Equals(scope, "Help", StringComparison.OrdinalIgnoreCase))
            {
                var statusEl = courseEl["helpReviewStatus"] ?? doc.CreateElement("helpReviewStatus");
                if (statusEl.ParentNode == null) courseEl.AppendChild(statusEl);
                var prevStatus = (statusEl.InnerText ?? "Ok").Trim();
                statusEl.InnerText = newStatus;

                var noteEl = courseEl["helpReviewAdminNote"] ?? doc.CreateElement("helpReviewAdminNote");
                noteEl.InnerText = adminNote ?? string.Empty;
                if (noteEl.ParentNode == null) courseEl.AppendChild(noteEl);

                var flagEl = courseEl["helpRevokedFlag"] ?? doc.CreateElement("helpRevokedFlag");
                if (flagEl.ParentNode == null) courseEl.AppendChild(flagEl);
                var prevFlag = (flagEl.InnerText ?? "false").Trim();

                var helpEl = courseEl["helpSessions"] ?? doc.CreateElement("helpSessions");
                if (helpEl.ParentNode == null) courseEl.AppendChild(helpEl);
                var helpCount = ParseIntSafe(helpEl.InnerText, 0);

                if (newStatus.Equals("Questioned", StringComparison.OrdinalIgnoreCase)
                    && !prevFlag.Equals("true", StringComparison.OrdinalIgnoreCase))
                {
                    if (helpCount > 0)
                    {
                        helpCount -= 1;
                        helpEl.InnerText = helpCount.ToString();
                        totalHelp = Math.Max(0, totalHelp - 1);
                    }

                    flagEl.InnerText = "true";
                }
                else if (newStatus.Equals("Ok", StringComparison.OrdinalIgnoreCase)
                         && prevFlag.Equals("true", StringComparison.OrdinalIgnoreCase))
                {
                    helpCount += 1;
                    helpEl.InnerText = helpCount.ToString();
                    totalHelp += 1;
                    flagEl.InnerText = "false";
                }

                var updatedEl = courseEl["helpReviewUpdatedUtc"] ?? doc.CreateElement("helpReviewUpdatedUtc");
                updatedEl.InnerText = nowIso;
                if (updatedEl.ParentNode == null) courseEl.AppendChild(updatedEl);
            }

            // Update totals snapshot
            totalTeachEl.InnerText = totalTeach.ToString();
            totalHelpEl.InnerText = totalHelp.ToString();
            var totalsUpdatedEl = totalsEl["lastUpdatedUtc"] ?? doc.CreateElement("lastUpdatedUtc");
            totalsUpdatedEl.InnerText = nowIso;
            if (totalsUpdatedEl.ParentNode == null) totalsEl.AppendChild(totalsUpdatedEl);

            doc.Save(HelperProgressXmlPath);

            // Audit the admin decision into the same university audit log
            try
            {
                var details = $"Admin set {scope} review status to '{newStatus}' for helperId={helperId}, courseId={courseId}, course=\"{courseTitle}\".";
                if (!string.IsNullOrWhiteSpace(adminNote))
                {
                    var preview = adminNote;
                    if (preview.Length > 180) preview = preview.Substring(0, 177) + "...";
                    details += " Admin note: " + preview;
                }

                UniversityAuditLogger.AppendForCurrentUser(this, auditType, details);
            }
            catch
            {
                // Never block admin UI on audit failures.
            }
        }

        private class AuditRow
        {
            public string TimestampDate { get; set; }
            public string TimestampTime { get; set; }
            public DateTime TimestampLocal { get; set; }

            public string Role { get; set; }
            public string Type { get; set; }
            public string FirstName { get; set; }
            public string Email { get; set; }
            public string Details { get; set; }
        }

        private sealed class VerificationItem
        {
            public string CourseId { get; set; }
            public string CourseTitle { get; set; }
            public string Status { get; set; }
            public string StatusLabel { get; set; }
            public string CssClass { get; set; }
            public string LastUpdatedLabel { get; set; }
            public string HelperDisplayName { get; set; }

            public string AdminNote { get; set; }
            public string HelperNote { get; set; }
            public bool HasHelperNote { get; set; }
            public bool HasAdminNote { get; set; }
            public bool IsResubmission { get; set; }
            public string SubmissionLabel { get; set; }
        }

        private sealed class SessionReviewItem
        {
            public string CourseId { get; set; }
            public string CourseTitle { get; set; }
            public int TeachingSessions { get; set; }
            public int HelpSessions { get; set; }

            public bool HasTeaching { get; set; }
            public bool HasHelp { get; set; }

            public string TeachingStatusLabel { get; set; }
            public string TeachingStatusCss { get; set; }
            public string TeachingAdminNote { get; set; }

            public string HelpStatusLabel { get; set; }
            public string HelpStatusCss { get; set; }
            public string HelpAdminNote { get; set; }
        }
    }
}


