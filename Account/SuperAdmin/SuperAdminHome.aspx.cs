using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;

namespace CyberApp_FIA.Account
{
    /// <summary>
    /// SuperAdmin dashboard:
    /// - Creates and saves microcourses to ~/App_Data/microcourses.xml
    /// - Binds selectable certification rules from ~/App_Data/certificationRules.xml
    /// - Persists metadata (creator, timestamps, status), free-form tags, and selected rules
    /// Notes:
    ///   * Uses App_Data for simple file-based persistence (not publicly served by IIS).
    ///   * All rule selections are many-to-one: a course can require multiple rules.
    /// </summary>
    public partial class SuperAdminHome : Page
    {
        // Path to microcourses datastore (XML with <microcourses><course .../></microcourses>)
        private string MicrocoursesXmlPath => Server.MapPath("~/App_Data/microcourses.xml");

        // Path to certification rules datastore (XML with <certRules><rule .../></certRules>)
        private string RulesXmlPath => Server.MapPath("~/App_Data/certificationRules.xml");

        /// <summary>
        /// First load:
        /// - Gate access to "SuperAdmin"
        /// - Set greeting
        /// - Bind the multi-select list of available certification rules
        /// </summary>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // ---- Access Gate: only SuperAdmin role ----
                var role = (string)Session["Role"];
                if (!string.Equals(role, "SuperAdmin", StringComparison.OrdinalIgnoreCase))
                {
                    Response.Redirect("~/Account/Login.aspx");
                    return;
                }

                // Friendly header text
                WelcomeName.Text = (string)Session["Email"] ?? "Super Admin";

                // Populate rules listbox with options loaded from certificationRules.xml
                BindRuleChoices();
            }
        }

        /// <summary>
        /// Clears session and returns to the welcome page.
        /// </summary>
        protected void BtnLogout_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Response.Redirect("~/Welcome_Page.aspx");
        }

        /// <summary>
        /// Persist a new microcourse:
        /// - Validates required inputs (Title, Summary, Duration)
        /// - Ensures microcourses.xml exists
        /// - Creates a <course> with attributes (id, status, createdAt, createdBy)
        /// - Adds child nodes: title, summary, duration, externalLink, tags, requiredRules
        /// - Adds placeholders UA may fill later: startTime, endTime, maxParticipants
        /// - Saves document and clears the form
        /// </summary>
        protected void BtnSaveMicrocourse_Click(object sender, EventArgs e)
        {
            // Normalize and collect inputs from form fields.
            var title = Title.Text?.Trim();
            var summary = Summary.Text?.Trim();
            var duration = Duration.Text?.Trim();
            var externalLink = ExternalLink.Text?.Trim();
            var tagsCsv = Tags.Text?.Trim();
            var status = Status.SelectedValue?.Trim();

            // Basic required fields (the page likely also has client/server validators).
            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(summary) || string.IsNullOrEmpty(duration))
            {
                FormMessage.Text = "<span style='color:#c21d1d'>Please fill in Title, Summary, and Duration.</span>";
                return;
            }

            // Ensure the datastore exists with a root node.
            EnsureMicrocourseXml();

            // Load document for modification.
            var doc = new XmlDocument();
            doc.Load(MicrocoursesXmlPath);

            // Create <course> with audit and lifecycle attributes.
            var course = doc.CreateElement("course");
            course.SetAttribute("id", Guid.NewGuid().ToString("N")); // compact GUID as primary key
            course.SetAttribute("status", string.IsNullOrEmpty(status) ? "Draft" : status);
            course.SetAttribute("createdAt", DateTime.UtcNow.ToString("o"));                 // ISO 8601 UTC timestamp
            course.SetAttribute("createdBy", (Session["Email"] as string) ?? "superadmin@unknown"); // audit actor

            // Core content fields
            course.AppendChild(Mk(doc, "title", title));
            course.AppendChild(Mk(doc, "summary", summary));
            course.AppendChild(Mk(doc, "duration", duration));
            course.AppendChild(Mk(doc, "externalLink", externalLink ?? ""));

            // Tags: CSV -> <tags><tag>..</tag>...</tags>
            course.AppendChild(MkTags(doc, tagsCsv));

            // --- Required certification rules (multi-select) ---
            // Gather selected rules from the ListBox; store distinct IDs defensively.
            var selectedRuleIds = RulesList.Items.Cast<ListItem>()
                                      .Where(i => i.Selected)
                                      .Select(i => i.Value)
                                      .Distinct()
                                      .ToList();

            // Persist as <requiredRules><rule id="..."/>...</requiredRules>
            course.AppendChild(MkRequiredRules(doc, selectedRuleIds));

            // Placeholders for University Admin to configure later (left empty here).
            course.AppendChild(Mk(doc, "startTime", ""));        // UA will schedule actual time ranges
            course.AppendChild(Mk(doc, "endTime", ""));          // UA will schedule actual time ranges
            course.AppendChild(Mk(doc, "maxParticipants", ""));  // empty = unlimited/not set yet

            // Append to document root and save.
            doc.DocumentElement.AppendChild(course);
            doc.Save(MicrocoursesXmlPath);

            // UX: show success, clear inputs, and rebind rules to show everything unselected again.
            FormMessage.Text = "<span style='color:#0a7a3c'>Microcourse saved.</span>";
            ClearForm();
            BindRuleChoices();
        }

        /// <summary>
        /// Manual Clear button handler: resets fields and rebinds rules.
        /// </summary>
        protected void BtnClear_Click(object sender, EventArgs e)
        {
            ClearForm();
            FormMessage.Text = ""; // explicit clear on manual action
            BindRuleChoices();
        }

        /// <summary>
        /// Resets all input fields to defaults.
        /// Leaves FormMessage untouched when called from save (so success stays visible).
        /// </summary>
        private void ClearForm()
        {
            Title.Text = "";
            Summary.Text = "";
            Duration.Text = "";
            ExternalLink.Text = "";
            Tags.Text = "";
            Status.SelectedValue = "Draft";

            // Uncheck all rule selections.
            foreach (ListItem li in RulesList.Items) li.Selected = false;
        }

        // =========================
        // Helpers: Datastore setup & binding
        // =========================

        /// <summary>
        /// Ensures microcourses.xml exists with a root <microcourses version="1">.
        /// Creates directories as needed.
        /// </summary>
        private void EnsureMicrocourseXml()
        {
            if (File.Exists(MicrocoursesXmlPath)) return;
            Directory.CreateDirectory(Path.GetDirectoryName(MicrocoursesXmlPath));
            var init = "<?xml version='1.0' encoding='utf-8'?><microcourses version='1'></microcourses>";
            File.WriteAllText(MicrocoursesXmlPath, init);
        }

        /// <summary>
        /// Ensures certificationRules.xml exists with a root <certRules version="1">.
        /// (No rules are added here; this only establishes the container.)
        /// </summary>
        private void EnsureRulesXml()
        {
            if (File.Exists(RulesXmlPath)) return;
            Directory.CreateDirectory(Path.GetDirectoryName(RulesXmlPath));
            File.WriteAllText(RulesXmlPath, "<?xml version='1.0' encoding='utf-8'?><certRules version='1'></certRules>");
        }

        /// <summary>
        /// Loads all available rules from certificationRules.xml and binds them
        /// to the multi-select ListBox (RulesList).
        /// Expected rule shape:
        ///   <rule id="RULE_ID">
        ///     <name>Human readable rule name</name>
        ///     ...optional fields...
        ///   </rule>
        /// </summary>
        private void BindRuleChoices()
        {
            EnsureRulesXml();

            var doc = new XmlDocument();
            doc.Load(RulesXmlPath);

            var items = new List<ListItem>();
            foreach (XmlElement r in doc.SelectNodes("/certRules/rule"))
            {
                var id = r.GetAttribute("id");
                var name = r["name"]?.InnerText ?? id;
                if (string.IsNullOrWhiteSpace(id)) continue; // skip malformed entries

                // Show "Name (ID)" to give administrators clarity about the underlying identifier.
                items.Add(new ListItem($"{name} ({id})", id));
            }

            RulesList.DataSource = items;
            RulesList.DataTextField = "Text";
            RulesList.DataValueField = "Value";
            RulesList.DataBind();
        }

        // =========================
        // Helpers: XML element builders
        // =========================

        /// <summary>
        /// Utility: creates a simple element with text content (null-safe -> empty string).
        /// </summary>
        private static XmlElement Mk(XmlDocument d, string name, string val)
        {
            var el = d.CreateElement(name);
            el.InnerText = val ?? "";
            return el;
        }

        /// <summary>
        /// Builds a <tags>...</tags> element from a CSV/semicolon list.
        /// Splits on ',' and ';', trims whitespace, and drops empty segments.
        /// </summary>
        private static XmlElement MkTags(XmlDocument d, string tagsCsv)
        {
            var tags = d.CreateElement("tags");
            if (!string.IsNullOrWhiteSpace(tagsCsv))
            {
                var parts = tagsCsv.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var raw in parts)
                {
                    var t = raw.Trim();
                    if (t.Length == 0) continue;
                    var tag = d.CreateElement("tag");
                    tag.InnerText = t;
                    tags.AppendChild(tag);
                }
            }
            return tags;
        }

        /// <summary>
        /// Builds:
        ///   <requiredRules>
        ///     <rule id="RULE_A" />
        ///     <rule id="RULE_B" />
        ///   </requiredRules>
        /// from a sequence of rule IDs (duplicates removed, whitespace-trimmed).
        /// </summary>
        private static XmlElement MkRequiredRules(XmlDocument d, IEnumerable<string> ruleIds)
        {
            var container = d.CreateElement("requiredRules");
            if (ruleIds == null) return container;

            foreach (var id in ruleIds.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct())
            {
                var el = d.CreateElement("rule");
                el.SetAttribute("id", id.Trim());
                container.AppendChild(el);
            }
            return container;
        }
    }
}



