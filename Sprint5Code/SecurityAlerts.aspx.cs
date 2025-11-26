using System;
using System.Collections.Generic;
using System.Web.UI;
using CyberApp_FIA.Services;

namespace CyberApp_FIA.Account
{
    /// <summary>
    /// Super Admin view over security alerts produced by SecurityMonitoringService.
    /// This is read-only and does not change any history.
    /// </summary>
    public partial class SecurityAlerts : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindAlerts();
            }
        }

        protected void AlertsGrid_PageIndexChanging(object sender, System.Web.UI.WebControls.GridViewPageEventArgs e)
        {
            AlertsGrid.PageIndex = e.NewPageIndex;
            BindAlerts();
        }

        private void BindAlerts()
        {
            var svc = new SecurityMonitoringService();
            var alerts = svc.GetAllAlerts() ?? new List<SecurityAlertRecord>();

            AlertsGrid.DataSource = alerts;
            AlertsGrid.DataBind();

            ResultCount.Text = $"{alerts.Count} alert{(alerts.Count == 1 ? "" : "s")}.";
        }
    }
}
