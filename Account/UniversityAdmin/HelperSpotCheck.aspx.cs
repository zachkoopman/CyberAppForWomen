using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using CyberApp_FIA.Services;

namespace CyberApp_FIA.Account
{
    /// <summary>
    /// University Admin spot-check workflow UI.
    /// Uses HelperVerificationService to fetch latest Helper logs
    /// and to record Verify/Question/Skip decisions.
    /// </summary>
    public partial class HelperSpotCheck : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindSamples();
            }
        }

        private void BindSamples()
        {
            var svc = new HelperVerificationService();
            var samples = svc.GetLatestSamples(maxCount: 10);
            SampleRepeater.DataSource = samples;
            SampleRepeater.DataBind();
            StatusLabel.Text = $"{samples.Count} helper logs loaded for review.";
        }

        protected void BtnSaveDecisions_Click(object sender, EventArgs e)
        {
            var svc = new HelperVerificationService();
            var adminEmail = Session["Email"] as string ?? "";
            var adminUniversity = Session["University"] as string ?? "";

            foreach (RepeaterItem item in SampleRepeater.Items)
            {
                var decisionList = (RadioButtonList)item.FindControl("DecisionList");
                var noteBox = (TextBox)item.FindControl("AdminNote");
                var keyField = (HiddenField)item.FindControl("LogKey");
                var helperIdField = (HiddenField)item.FindControl("HelperIdHidden");
                var helperNameField = (HiddenField)item.FindControl("HelperNameHidden");
                var sourceField = (HiddenField)item.FindControl("SourceHidden");

                var decision = decisionList.SelectedValue;
                if (string.IsNullOrWhiteSpace(decision) || decision == "Skip")
                {
                    // Skip means we do not write anything to the verification store or audit log.
                    continue;
                }

                var model = new HelperVerificationService.HelperVerificationDecision
                {
                    LogKey = keyField.Value,
                    HelperId = helperIdField.Value,
                    HelperName = helperNameField.Value,
                    Source = sourceField.Value,
                    Decision = decision,
                    Note = noteBox.Text,
                    AdminEmail = adminEmail,
                    AdminUniversity = adminUniversity
                };

                svc.RecordDecision(model);
            }

            StatusLabel.Text = "Decisions saved. Helper progress and audit log have been updated.";
            BindSamples();
        }
    }
}
