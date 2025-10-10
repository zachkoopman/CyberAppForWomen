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
    /// CertificationRules admin page (SuperAdmin only).
    /// - Lists existing certification rules (right-side table)
    /// - Creates/updates/deletes rules
    /// - Supports multi-select prerequisites between rules
    /// - Persists to ~/App_Data/certificationRules.xml
    /// </summary>
    public partial class CertificationRules : Page
    {
        /// <summary>
        /// Physical path to the certification rules datastore.
        /// </summary>
        private string RulesXmlPath => Server.MapPath("~/App_Data/certificationRules.xml");

        /// <summary>
        /// Auth gate (SuperAdmin only) and first-load bindings
        /// (rules table + prerequisite choices).
        /// </summary>
        protected void Page_Load(object sender, EventArgs e)
        {
            // ---- Access Gate: only SuperAdmin can manage certification rules ----
            var role = (string)Session["Role"];
            if (!string.Equals(role, "SuperAdmin", StringComparison.OrdinalIgnoreCase))
            {
                Response.Redirect("~/Account/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                BindRules();         // populate right-side rules table
                BindPrereqChoices(); // populate left-side prerequisite chooser
            }
        }

        /// <summary>
        /// Ensures the rules XML datastore exists with a <certRules> root.
        /// Creates directories as needed.
        /// </summary>
        private void EnsureXml()
        {
            if (File.Exists(RulesXmlPath)) return;
            Directory.CreateDirectory(Path.GetDirectoryName(RulesXmlPath));
            File.WriteAllText(RulesXmlPath, "<?xml version='1.0' encoding='utf-8'?><certRules version='1'></certRules>");
        }

        /// <summary>
        /// Reads all rules and binds them to the repeater on the right.
        /// Shows id, name, passScore, and minSessionsTaught.
        /// </summary>
        private void BindRules()
        {
            EnsureXml();

            var rows = new List<object>();
            var doc = new XmlDocument(); doc.Load(RulesXmlPath);

            foreach (XmlElement r in doc.SelectNodes("/certRules/rule"))
            {
                rows.Add(new
                {
                    id = r.GetAttribute("id"),
                    name = r["name"]?.InnerText ?? "",
                    passScore = r["passScore"]?.InnerText ?? "0",
                    minSessions = r["minSessionsTaught"]?.InnerText ?? "0"
                });
            }

            NoRulesPH.Visible = rows.Count == 0;
            RulesRepeater.DataSource = rows;
            RulesRepeater.DataBind();
        }

        /// <summary>
        /// Binds the prerequisite multiselect list with all rules except
        /// the one currently being edited (prevents self-dependency).
        /// Optionally accepts a set of pre-selected IDs to reflect current state.
        /// </summary>
        private void BindPrereqChoices(string currentRuleId = null, IEnumerable<string> selected = null)
        {
            EnsureXml();

            var doc = new XmlDocument(); doc.Load(RulesXmlPath);
            var items = new List<ListItem>();

            foreach (XmlElement r in doc.SelectNodes("/certRules/rule"))
            {
                var id = r.GetAttribute("id");
                var name = r["name"]?.InnerText ?? id;
                if (string.IsNullOrWhiteSpace(id)) continue;

                // Do not allow selecting itself as a prerequisite.
                if (!string.IsNullOrWhiteSpace(currentRuleId) &&
                    id.Equals(currentRuleId, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                items.Add(new ListItem($"{name} ({id})", id));
            }

            PrereqList.DataSource = items;
            PrereqList.DataTextField = "Text";
            PrereqList.DataValueField = "Value";
            PrereqList.DataBind();

            // Restore selection state if provided.
            if (selected != null)
            {
                var set = new HashSet<string>(selected, StringComparer.OrdinalIgnoreCase);
                foreach (ListItem li in PrereqList.Items)
                    li.Selected = set.Contains(li.Value);
            }
        }

        /// <summary>
        /// Handles command events from the rules repeater (e.g., Edit button).
        /// </summary>
        protected void RulesRepeater_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "edit")
            {
                LoadRuleToForm(e.CommandArgument as string);
            }
        }

        /// <summary>
        /// Loads a rule into the left-side edit form by id.
        /// Also binds the prerequisites multiselect with the rule's current prereqs.
        /// Supports both structured (<prerequisites><rule id="..."/></prerequisites>)
        /// and legacy CSV in <prerequisites> text.
        /// </summary>
        private void LoadRuleToForm(string id)
        {
            EnsureXml();

            var doc = new XmlDocument(); doc.Load(RulesXmlPath);
            var r = (XmlElement)doc.SelectSingleNode($"/certRules/rule[@id='{id}']");
            if (r == null) return;

            // Basic fields
            RuleId.Text = r.GetAttribute("id");
            RuleName.Text = r["name"]?.InnerText ?? "";
            RuleDesc.Text = r["description"]?.InnerText ?? "";
            PassScore.Text = r["passScore"]?.InnerText ?? "0";
            MinSessions.Text = r["minSessionsTaught"]?.InnerText ?? "0";
            ExpiryDays.Text = r["expiryDays"]?.InnerText ?? "0";
            MaxAttempts.Text = r["maxAttempts"]?.InnerText ?? "0";
            CooldownDays.Text = r["retakeCooldownDays"]?.InnerText ?? "0";
            Evidence.Text = r["evidence"]?.InnerText ?? "";

            // Collect prerequisite IDs (structured first, legacy CSV fallback).
            var selected = new List<string>();

            // Structured form
            foreach (XmlElement rr in r.SelectNodes("./prerequisites/rule"))
            {
                var rid = rr.GetAttribute("id");
                if (!string.IsNullOrWhiteSpace(rid)) selected.Add(rid);
            }

            // Legacy fallback: CSV within <prerequisites> element text (no children).
            if (selected.Count == 0 && r["prerequisites"] != null && r["prerequisites"].HasChildNodes == false)
            {
                var csv = r["prerequisites"]?.InnerText ?? "";
                var parts = (csv ?? "").Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                                       .Select(s => s.Trim());
                selected.AddRange(parts);
            }

            // Rebind left-side choices excluding the rule itself; mark current selections.
            BindPrereqChoices(RuleId.Text, selected);

            // UX: notify which rule is loaded.
            FormMessage.Text = $"Loaded rule <strong>{RuleId.Text}</strong>.";
        }

        /// <summary>
        /// Save (upsert) the current rule from the edit form.
        /// - Validates RuleId
        /// - Inserts or updates <rule id="..."> with core fields
        /// - Replaces <prerequisites> with structured child <rule id="..."/> entries
        /// - Saves the document, refreshes table, and rebinds prereqs
        /// </summary>
        protected void BtnSave_Click(object sender, EventArgs e)
        {
            EnsureXml();

            var doc = new XmlDocument(); doc.Load(RulesXmlPath);
            var root = doc.DocumentElement;

            // Require an ID for the rule.
            if (string.IsNullOrWhiteSpace(RuleId.Text))
            {
                FormMessage.Text = "<span style='color:#c21d1d'>Rule ID is required.</span>";
                return;
            }

            // Upsert: find existing or create a new <rule>.
            var r = (XmlElement)doc.SelectSingleNode($"/certRules/rule[@id='{RuleId.Text.Trim()}']");
            if (r == null)
            {
                r = doc.CreateElement("rule");
                r.SetAttribute("id", RuleId.Text.Trim());
                r.SetAttribute("version", "1"); // initial version; reserved for future migrations
                root.AppendChild(r);
            }

            // Update core fields (create node if missing).
            SetOrCreate(doc, r, "name", RuleName.Text);
            SetOrCreate(doc, r, "description", RuleDesc.Text);
            SetOrCreate(doc, r, "passScore", PassScore.Text);
            SetOrCreate(doc, r, "minSessionsTaught", MinSessions.Text);
            SetOrCreate(doc, r, "expiryDays", ExpiryDays.Text);
            SetOrCreate(doc, r, "maxAttempts", MaxAttempts.Text);
            SetOrCreate(doc, r, "retakeCooldownDays", CooldownDays.Text);
            SetOrCreate(doc, r, "evidence", Evidence.Text);

            // --- Structured prerequisites from the CheckBoxList ---
            // Remove existing <prerequisites> block to rewrite cleanly.
            var existingPrereqs = r.SelectSingleNode("./prerequisites");
            if (existingPrereqs != null) r.RemoveChild(existingPrereqs);

            // Build a new prerequisites container with distinct IDs.
            var pre = doc.CreateElement("prerequisites");
            var selected = PrereqList.Items.Cast<ListItem>()
                             .Where(i => i.Selected)
                             .Select(i => i.Value.Trim())
                             .Where(v => !string.IsNullOrWhiteSpace(v))
                             .Distinct(StringComparer.OrdinalIgnoreCase)
                             .ToList();

            foreach (var pid in selected)
            {
                var rr = doc.CreateElement("rule");
                rr.SetAttribute("id", pid);
                pre.AppendChild(rr);
            }
            r.AppendChild(pre);

            // Persist changes.
            doc.Save(RulesXmlPath);

            // UX: notify, refresh rules table, and keep prereq selections visible.
            FormMessage.Text = "<span style='color:#0a7a3c'>Rule saved.</span>";
            BindRules();
            BindPrereqChoices(RuleId.Text, selected);
        }

        /// <summary>
        /// Deletes the current rule (if it exists) and refreshes UI.
        /// </summary>
        protected void BtnDelete_Click(object sender, EventArgs e)
        {
            EnsureXml();

            var doc = new XmlDocument(); doc.Load(RulesXmlPath);
            var r = (XmlElement)doc.SelectSingleNode($"/certRules/rule[@id='{RuleId.Text.Trim()}']");
            if (r != null)
            {
                r.ParentNode.RemoveChild(r);
                doc.Save(RulesXmlPath);
            }

            ClearForm();
            BindRules();
            BindPrereqChoices();
            FormMessage.Text = "<span style='color:#0a7a3c'>Rule deleted.</span>";
        }

        /// <summary>
        /// Resets the form and prerequisite choices without saving.
        /// </summary>
        protected void BtnClear_Click(object sender, EventArgs e)
        {
            ClearForm();
            BindPrereqChoices();
            FormMessage.Text = "";
        }

        /// <summary>
        /// Helper: set an element's InnerText or create the element if missing,
        /// then attach it to the parent node.
        /// </summary>
        private static void SetOrCreate(XmlDocument d, XmlElement parent, string name, string value)
        {
            var node = parent[name] ?? d.CreateElement(name);
            node.InnerText = value?.Trim() ?? "";
            if (node.ParentNode == null) parent.AppendChild(node);
        }

        /// <summary>
        /// Clears all form fields to defaults and unselects prerequisites.
        /// </summary>
        private void ClearForm()
        {
            RuleId.Text = RuleName.Text = RuleDesc.Text = "";
            PassScore.Text = MinSessions.Text = ExpiryDays.Text = MaxAttempts.Text = CooldownDays.Text = "0";
            Evidence.Text = "";

            foreach (ListItem li in PrereqList.Items) li.Selected = false;
        }
    }
}


