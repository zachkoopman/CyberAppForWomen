using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web.UI;
using CyberApp_FIA.Services;

namespace CyberApp_FIA.Account
{
    /// <summary>
    /// Super Admin Activity & Audit Log viewer.
    /// - Read-only view (no edits or deletes).
    /// - Uses AuditLogService to load entries.
    /// - Provides search, basic filters, and paging.
    /// </summary>
    public partial class SuperAdminAuditLogViewer : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindGrid();
            }
        }

        protected void BtnApply_Click(object sender, EventArgs e)
        {
            // When the user clicks Apply, reload grid with filters.
            BindGrid();
        }

        protected void BtnClear_Click(object sender, EventArgs e)
        {
            SearchText.Text = string.Empty;
            CategoryFilter.SelectedIndex = 0;
            RoleFilter.SelectedIndex = 0;
            FromDate.Text = string.Empty;
            ToDate.Text = string.Empty;
            AuditGrid.PageIndex = 0;
            BindGrid();
        }

        protected void AuditGrid_PageIndexChanging(object sender, System.Web.UI.WebControls.GridViewPageEventArgs e)
        {
            AuditGrid.PageIndex = e.NewPageIndex;
            BindGrid();
        }

        private void BindGrid()
        {
            var svc = new AuditLogService();

            var query = new AuditLogQuery
            {
                SearchText = SearchText.Text.Trim(),
                Category = CategoryFilter.SelectedValue,
                Role = RoleFilter.SelectedValue
            };

            if (DateTime.TryParse(FromDate.Text.Trim(), CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                    out var from))
            {
                query.FromUtc = from;
            }

            if (DateTime.TryParse(ToDate.Text.Trim(), CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                    out var to))
            {
                query.ToUtc = to;
            }

            var raw = svc.Query(query);

            // Build a small view model for the grid.
            var rows = new List<AuditGridRow>();
            foreach (var e in raw)
            {
                rows.Add(new AuditGridRow
                {
                    TimestampUtc = e.TimestampUtc,
                    ActorRole = e.ActorRole,
                    ActorEmail = e.ActorEmail,
                    Category = e.Category,
                    ActionType = e.ActionType,
                    TargetLabel = e.TargetLabel,
                    ClientIpMasked = MaskIp(e.ClientIp),
                    FollowUpUrl = BuildFollowUpUrl(e)
                });
            }

            AuditGrid.DataSource = rows;
            AuditGrid.DataBind();

            ResultCount.Text = $"{rows.Count} entr{(rows.Count == 1 ? "y" : "ies")} shown.";
        }

        private static string MaskIp(string ip)
        {
            if (string.IsNullOrWhiteSpace(ip)) return string.Empty;
            if (ip.Contains(":"))
            {
                // Very simple IPv6 masking: show first block only.
                var parts = ip.Split(':');
                return parts.Length > 0 ? parts[0] + ":•••" : "•••";
            }

            var v4 = ip.Split('.');
            if (v4.Length == 4)
            {
                return $"{v4[0]}.{v4[1]}.{v4[2]}.*";
            }

            return "•••";
        }

        /// <summary>
        /// Build a link into a future security follow-up tool.
        /// This does not change the log – it only helps Super Admins act on it.
        /// </summary>
        private static string BuildFollowUpUrl(AuditLogEntry e)
        {
            if (e == null) return string.Empty;

            // For now we send user-related events to a planned follow-up page.
            if (e.TargetType == "ParticipantAccount" || e.TargetType == "HelperAccount" || e.TargetType == "UniversityAdminAccount")
            {
                return $"~/Account/SuperAdmin/SecurityFollowup.aspx?targetId={Uri.EscapeDataString(e.TargetId ?? string.Empty)}&actionHint={Uri.EscapeDataString(e.ActionType ?? string.Empty)}";
            }

            if (e.TargetType == "Event" || e.TargetType == "Session")
            {
                return $"~/Account/SuperAdmin/SecurityFollowup.aspx?eventId={Uri.EscapeDataString(e.TargetId ?? string.Empty)}";
            }

            // Default: no-op link.
            return "~/Account/SuperAdmin/SecurityFollowup.aspx";
        }

        private sealed class AuditGridRow
        {
            public DateTime TimestampUtc { get; set; }
            public string ActorRole { get; set; }
            public string ActorEmail { get; set; }
            public string Category { get; set; }
            public string ActionType { get; set; }
            public string TargetLabel { get; set; }
            public string ClientIpMasked { get; set; }
            public string FollowUpUrl { get; set; }
        }
    }
}
