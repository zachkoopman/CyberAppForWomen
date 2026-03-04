using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Xml;

namespace CyberApp_FIA.Helper
{
    public partial class Badges : Page
    {
        private string HelperBadgesXmlPath => Server.MapPath("~/App_Data/helperBadges.xml");
        private string BadgeDescriptionsXmlPath => Server.MapPath("~/App_Data/badgeDescriptions.xml");

        protected void Page_Load(object sender, EventArgs e)
        {
            var role = (string)Session["Role"];
            if (!string.Equals(role, "Helper", StringComparison.OrdinalIgnoreCase))
            {
                Response.Redirect("~/Account/Login.aspx");
                return;
            }

            var helperId = Session["UserId"] as string;
            if (string.IsNullOrWhiteSpace(helperId))
            {
                Response.Redirect("~/Account/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                BindBadges(helperId);
            }
        }

        private sealed class BadgeRow
        {
            public string ImageUrl { get; set; }
            public string CourseTitle { get; set; }
            public string Description { get; set; }
            public string TierLabel { get; set; }
            public string RuleLabel { get; set; }
            public DateTime EarnedOnUtc { get; set; }
            public string EarnedOnLabel { get; set; }
        }

        private void BindBadges(string helperId)
        {
            var rows = LoadHelperBadges(helperId);

            if (rows.Count == 0)
            {
                EmptyPH.Visible = true;
                BadgesRepeater.DataSource = null;
                BadgesRepeater.DataBind();
                return;
            }

            EmptyPH.Visible = false;
            BadgesRepeater.DataSource = rows
                .OrderByDescending(r => r.EarnedOnUtc)
                .ThenBy(r => r.CourseTitle)
                .ToList();
            BadgesRepeater.DataBind();
        }

        private Dictionary<string, string> LoadDescriptionsByCourseId()
        {
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (!File.Exists(BadgeDescriptionsXmlPath))
                return map;

            var doc = new XmlDocument();
            doc.Load(BadgeDescriptionsXmlPath);

            foreach (XmlElement b in doc.SelectNodes("/badgeDescriptions/badge"))
            {
                var courseId = (b.GetAttribute("courseId") ?? "").Trim();
                var desc = (b["description"]?.InnerText ?? "").Trim();
                if (!string.IsNullOrWhiteSpace(courseId) && !string.IsNullOrWhiteSpace(desc))
                    map[courseId] = desc;
            }

            return map;
        }

        private List<BadgeRow> LoadHelperBadges(string helperId)
        {
            var rows = new List<BadgeRow>();

            if (!File.Exists(HelperBadgesXmlPath))
                return rows;

            var descByCourseId = LoadDescriptionsByCourseId();

            var doc = new XmlDocument();
            doc.Load(HelperBadgesXmlPath);

            var helper = doc.SelectSingleNode($"/helperBadges/helper[@id='{helperId}']") as XmlElement;
            if (helper == null)
                return rows;

            foreach (XmlElement b in helper.SelectNodes("badge"))
            {
                var courseId = (b.GetAttribute("courseId") ?? "").Trim();
                var title = (b.GetAttribute("courseTitle") ?? "").Trim();
                var tier = (b.GetAttribute("tier") ?? "").Trim();
                var image = (b.GetAttribute("image") ?? "").Trim();
                var earnedRaw = (b.GetAttribute("earnedOnUtc") ?? "").Trim();

                if (!DateTime.TryParse(earnedRaw, CultureInfo.InvariantCulture,
                    DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var earnedUtc))
                {
                    earnedUtc = DateTime.UtcNow;
                }

                // shared description by courseId
                string desc = null;
                if (!string.IsNullOrWhiteSpace(courseId) && descByCourseId.TryGetValue(courseId, out var d))
                    desc = d;

                if (string.IsNullOrWhiteSpace(desc))
                    desc = "Earned by teaching this microcourse and supporting participants.";

                string ruleLabel = "Bronze: 5 taught • Silver: 10 taught • Gold: 20 taught";
                string tierLabel = string.IsNullOrWhiteSpace(tier) ? "Tier badge" : (tier + " badge");

                var local = DateTime.SpecifyKind(earnedUtc, DateTimeKind.Utc).ToLocalTime();

                rows.Add(new BadgeRow
                {
                    ImageUrl = ResolveUrl(image),
                    CourseTitle = string.IsNullOrWhiteSpace(title) ? "Microcourse badge" : title,
                    Description = desc,
                    TierLabel = tierLabel,
                    RuleLabel = ruleLabel,
                    EarnedOnUtc = earnedUtc,
                    EarnedOnLabel = local.ToString("MMM d, yyyy", CultureInfo.CurrentCulture)
                });
            }

            return rows;
        }
    }
}