using System;
using System.Collections.Generic;
using System.Web.UI;
using CyberApp_FIA.Services;

namespace CyberApp_FIA.Account
{
    /// <summary>
    /// University Admin read-only audit log viewer.
    /// Automatically filters entries to the University in Session["University"].
    /// </summary>
    public partial class UniversityAdminAuditLogViewer : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindGrid();
            }
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
                University = Session["University"] as string ?? string.Empty
            };

            // You can tighten this later (e.g., also filter by TargetUniversity in meta if you add it)
            var raw = svc.Query(query);

            var rows = new List<Row>();
            foreach (var e in raw)
            {
                rows.Add(new Row
                {
                    TimestampUtc = e.TimestampUtc,
                    ActorRole = e.ActorRole,
                    ActorEmail = e.ActorEmail,
                    Category = e.Category,
                    ActionType = e.ActionType,
                    TargetLabel = e.TargetLabel,
                    ClientIpMasked = MaskIp(e.ClientIp)
                });
            }

            AuditGrid.DataSource = rows;
            AuditGrid.DataBind();

            ResultCount.Text = $"{rows.Count} entr{(rows.Count == 1 ? "y" : "ies")} for your university.";
        }

        private static string MaskIp(string ip)
        {
            if (string.IsNullOrWhiteSpace(ip)) return string.Empty;
            if (ip.Contains(":"))
            {
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

        private sealed class Row
        {
            public DateTime TimestampUtc { get; set; }
            public string ActorRole { get; set; }
            public string ActorEmail { get; set; }
            public string Category { get; set; }
            public string ActionType { get; set; }
            public string TargetLabel { get; set; }
            public string ClientIpMasked { get; set; }
        }
    }
}
