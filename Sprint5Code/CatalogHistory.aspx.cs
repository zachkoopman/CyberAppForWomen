using System;
using System.Collections.Generic;
using System.Web.UI;
using CyberApp_FIA.Services;

namespace CyberApp_FIA.Account
{
    /// <summary>
    /// University Admin read-only view of catalog changes.
    /// Filters Catalog events to the current University in Session["University"].
    /// </summary>
    public partial class UniversityAdminCatalogHistory : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindGrid();
            }
        }

        protected void HistoryGrid_PageIndexChanging(object sender, System.Web.UI.WebControls.GridViewPageEventArgs e)
        {
            HistoryGrid.PageIndex = e.NewPageIndex;
            BindGrid();
        }

        private void BindGrid()
        {
            var university = Session["University"] as string ?? string.Empty;
            var svc = new AuditLogService();

            var query = new AuditLogQuery
            {
                Category = "Catalog",
                University = university
            };

            var entries = svc.Query(query);
            var rows = new List<Row>();

            foreach (var e in entries)
            {
                rows.Add(new Row
                {
                    TimestampUtc = e.TimestampUtc,
                    ActorEmail = e.ActorEmail,
                    ActorRole = e.ActorRole,
                    ActionType = e.ActionType,
                    TargetType = e.TargetType,
                    TargetLabel = e.TargetLabel
                });
            }

            HistoryGrid.DataSource = rows;
            HistoryGrid.DataBind();
            ResultCount.Text = $"{rows.Count} catalog change entr{(rows.Count == 1 ? "y" : "ies")} for your university.";
        }

        private sealed class Row
        {
            public DateTime TimestampUtc { get; set; }
            public string ActorEmail { get; set; }
            public string ActorRole { get; set; }
            public string ActionType { get; set; }
            public string TargetType { get; set; }
            public string TargetLabel { get; set; }
        }
    }
}
