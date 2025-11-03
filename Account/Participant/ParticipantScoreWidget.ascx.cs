using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
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
                    // No score yet — keep things minimal, disable share
                    PillScore.InnerText = "Take the quiz";
                    PillScore.Attributes["class"] = "pill"; // neutral pill
                    RptDomainMini.DataSource = Enumerable.Empty<object>();
                    RptDomainMini.DataBind();
                    ChkShare.Enabled = false;
                    BtnShare.Enabled = false;
                    return;
                }

                // Show overall with banded color (supports v1 0–100 or v2 0–10)
                var overallRaw = res.OverallScore;
                var overall10 = NormalizeToTen(overallRaw);
                PillScore.InnerText = overall10.ToString("0.0", CultureInfo.InvariantCulture);
                PillScore.Attributes["class"] = "pill " + BandClass(overall10); // pill low/med/high

                // Domains: List<KeyValuePair<string,double>>
                // The repeater ItemDataBound handles coloring and chip text
                RptDomainMini.DataSource = res.DomainScores.ToList();
                RptDomainMini.DataBind();

                // Share toggle reflects persisted preference
                ChkShare.Checked = res.ShareWithHelper;
            }
        }

        protected void RptDomainMini_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != System.Web.UI.WebControls.ListItemType.Item &&
                e.Item.ItemType != System.Web.UI.WebControls.ListItemType.AlternatingItem)
                return;

            // Expect KeyValuePair<string,double> from .ToList()
            var kv = (KeyValuePair<string, double>)e.Item.DataItem;
            var cell = (HtmlGenericControl)e.Item.FindControl("Cell");
            var chip = (HtmlGenericControl)e.Item.FindControl("Chip");

            // Normalize to 0–10 in case older results were 0–100
            var score10 = NormalizeToTen(kv.Value);

            // Assign text
            chip.InnerText = score10.ToString("0.0", CultureInfo.InvariantCulture);

            // Apply band classes to both cell and chip
            var band = BandClass(score10); // "low" | "med" | "high"
            cell.Attributes["class"] = "cell " + band;
            chip.Attributes["class"] = "chip " + band;
        }

        protected void BtnShare_Click(object sender, EventArgs e)
        {
            var svc = new QuizService(Page.Server);
            svc.SetShareWithHelper(UserKey, ChkShare.Checked);
        }

        // --- Helpers ---

        /// <summary>
        /// Converts either a 0–10 or 0–100 score into 0–10 for consistent display.
        /// </summary>
        private static double NormalizeToTen(double raw)
        {
            // Heuristic: values > 10 are assumed legacy 0–100
            return raw > 10.0 ? Math.Max(0.0, Math.Min(10.0, raw / 10.0)) : Math.Max(0.0, Math.Min(10.0, raw));
        }

        /// <summary>
        /// Returns "low" (0–3.9), "med" (4.0–6.9), or "high" (7.0–10.0) for styling.
        /// Higher = higher priority (pink).
        /// </summary>
        private static string BandClass(double tenScale)
        {
            if (tenScale >= 7.0) return "high";    // pink (highest priority)
            if (tenScale >= 4.0) return "med";     // blue (moderate)
            return "low";                           // teal (lower priority)
        }
    }
}

