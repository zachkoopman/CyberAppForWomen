using System;
using System.Linq;
using System.Web.UI;
using CyberApp_FIA.Services;

namespace CyberApp_FIA.Account.Participant
{
    public partial class ParticipantScoreWidget : UserControl
    {
        private string UserKey => Page.Session["UserId"] as string ?? "guest";
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                var svc = new QuizService(Page.Server);
                var res = svc.LoadLatestResult(UserKey);
                if (res == null)
                {
                    PillScore.InnerText = "Take the quiz";
                    RptDomainMini.DataSource = Enumerable.Empty<object>();
                    RptDomainMini.DataBind();
                    ChkShare.Enabled = false;
                    BtnShare.Enabled = false;
                }
                else
                {
                    PillScore.InnerText = res.OverallScore.ToString("0.0");
                    RptDomainMini.DataSource = res.DomainScores.ToList();
                    RptDomainMini.DataBind();
                    ChkShare.Checked = res.ShareWithHelper;
                }
            }
        }

        protected void BtnShare_Click(object sender, EventArgs e)
        {
            var svc = new QuizService(Page.Server);
            svc.SetShareWithHelper(UserKey, ChkShare.Checked);
        }
    }
}