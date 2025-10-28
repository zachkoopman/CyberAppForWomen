using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using CyberApp_FIA.Services;

namespace CyberApp_FIA.Account.Participant
{
    public partial class Quiz : Page
    {
        private QuizService _svc;
        private List<QuizService.QuizQuestion> _questions;
        private string _rules;
        private string UserKey => Session["UserId"] as string ?? "guest";


        private int Index
        {
            get { return ViewState["Index"] == null ? 0 : (int)ViewState["Index"]; }
            set { ViewState["Index"] = value; }
        }

        protected void BtnQuit_Click(object sender, EventArgs e)
        {
            // leave without finishing; send to event select
            Response.Redirect("~/Account/Participant/SelectEvent.aspx");
        }


        private Dictionary<string, string> Selections
        {
            get
            {
                if (ViewState["Sel"] == null) ViewState["Sel"] = new Dictionary<string, string>();
                return (Dictionary<string, string>)ViewState["Sel"];
            }
            set { ViewState["Sel"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            _svc = new QuizService(Server);
            _questions = _svc.LoadQuestions(out _rules);

            if (!IsPostBack)
            {
                // If already completed, go home
                if (_svc.IsCompleted(UserKey, _rules))
                {
                    Response.Redirect("~/Account/Participant/Home.aspx");
                    return;
                }

                // Load saved progress
                var (idx, map, completed) = _svc.LoadProgress(UserKey, _rules);
                Selections = map ?? new Dictionary<string, string>();
                Index = Math.Max(0, Math.Min(idx, _questions.Count - 1));
                BindQuestion();
            }
        }

        private void BindQuestion()
        {
            var q = _questions[Index];
            LblQuestionText.Text = q.Text;

            // Set option texts
            Options.Items[0].Text = $"A) {q.A}";
            Options.Items[1].Text = $"B) {q.B}";
            Options.Items[2].Text = $"C) {q.C}";
            Options.Items[3].Text = $"D) {q.D}";

            // restore selection if exists
            Options.ClearSelection();
            if (Selections.TryGetValue(q.Id, out var sel))
            {
                var item = Options.Items.FindByValue(sel);
                if (item != null) item.Selected = true;
            }

            // progress bar
            var pct = (int)Math.Round(((Index) / (double)_questions.Count) * 100.0);
            ProgressBar.Style["width"] = pct + "%";
            LblQuestionNumber.InnerText = $"Question {Index + 1} of {_questions.Count}";

            // buttons
            BtnPrev.Enabled = Index > 0;
            BtnNext.Visible = Index < _questions.Count - 1;
            BtnFinish.Visible = Index == _questions.Count - 1;

            // flash "Saved" briefly on postbacks
            BadgeSaved.Visible = IsPostBack;
        }

        private void SaveSelectionIfAny()
        {
            var q = _questions[Index];
            var selected = Options.SelectedValue;
            if (!string.IsNullOrEmpty(selected))
            {
                Selections[q.Id] = selected;
                _svc.SaveAnswer(UserKey, q.Id, selected, Index, _questions.Count, _rules);
            }
        }

        protected void Options_SelectedIndexChanged(object sender, EventArgs e)
        {
            SaveSelectionIfAny();
            BindQuestion();
        }

        protected void BtnPrev_Click(object sender, EventArgs e)
        {
            SaveSelectionIfAny();
            Index = Math.Max(0, Index - 1);
            BindQuestion();
        }

        protected void BtnNext_Click(object sender, EventArgs e)
        {
            SaveSelectionIfAny();
            Index = Math.Min(_questions.Count - 1, Index + 1);
            BindQuestion();
        }

        protected void BtnFinish_Click(object sender, EventArgs e)
        {
            SaveSelectionIfAny();
            var result = _svc.ComputeAndSaveResult(UserKey, Selections);
            // Show summary
            PanelQuestion.Visible = false;
            PanelComplete.Visible = true;

            LblOverall.Text = $"Overall Score: <strong>{result.OverallScore}</strong>";
            RptDomains.DataSource = result.DomainScores.ToList();
            RptDomains.DataBind();

            BlFactors.DataSource = result.TopFactors;
            BlFactors.DataBind();

            BlWins.DataSource = result.QuickWins;
            BlWins.DataBind();
        }

        protected void BtnSaveShare_Click(object sender, EventArgs e)
        {
            _svc.SetShareWithHelper(UserKey, ChkShare.Checked);
            Response.Redirect("~/Account/Participant/Home.aspx");
        }
    }
}
