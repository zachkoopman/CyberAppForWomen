using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace CyberApp_FIA.Services
{
    public class QuizService
    {
        private readonly string _appData;
        private readonly string _questionsPath;
        private readonly string _rulesPath;
        private readonly string _progressPath;
        private readonly string _resultsPath;

        public QuizService(HttpServerUtility server)
        {
            _appData = server.MapPath("~/App_Data/");
            _questionsPath = Path.Combine(_appData, "quiz_questions.xml");
            _rulesPath = Path.Combine(_appData, "quiz_rules.xml");
            _progressPath = Path.Combine(_appData, "quiz_progress.xml");
            _resultsPath = Path.Combine(_appData, "quiz_results.xml");
        }

        // ---------- Public DTOs ----------
        public class ScoreResult
        {
            public string Username { get; set; }
            public string RulesetVersion { get; set; }
            public DateTime CompletedUtc { get; set; }
            public double OverallScore { get; set; } // v1: 0–100, v2: 0–10
            public Dictionary<string, double> DomainScores { get; set; } = new Dictionary<string, double>();
            public List<string> TopFactors { get; set; } = new List<string>();
            public List<string> QuickWins { get; set; } = new List<string>();
            public bool ShareWithHelper { get; set; }
        }

        // For binding questions in the UI (works for both v1 and v2)
        public class UiQuestion
        {
            public string Id { get; set; }
            public string Text { get; set; }
            public string A { get; set; }
            public string B { get; set; }
            public string C { get; set; }
            public string D { get; set; }
        }

        // ---------- Legacy (v1) DTOs (internal) ----------
        private class V1Question
        {
            public string Id;
            public string Domain;
            public int Weight;
            public string Text;
            public string A;
            public string B;
            public string C;
            public string D;
            public string Correct;
            public string Explanation;
            public string QuickWin;
        }

        // ---------- V2 DTOs (internal) ----------
        private class V2Domain
        {
            public string Id;
            public string Title;
            public string QuickWin;
            public string Factor;
        }

        private class V2Option
        {
            public string Label;
            public List<(string module, int weight)> Maps = new List<(string, int)>();
        }

        private class V2Question
        {
            public string Id;
            public string Text;
            public Dictionary<string, V2Option> Options = new Dictionary<string, V2Option>(); // "A","B","C","D"
        }

        // ---------- Version helpers ----------
        public string CurrentRulesetVersion()
        {
            var x = XDocument.Load(_rulesPath);
            return x.Root.Element("Ruleset")?.Attribute("version")?.Value ?? "1.0";
        }

        private bool IsV2Active()
        {
            var q = XDocument.Load(_questionsPath);
            var v = q.Root.Attribute("rulesetVersion")?.Value ?? "1.0";
            return VersionTryParse(v) >= VersionTryParse("2.0");
        }

        private static Version VersionTryParse(string v)
        {
            try { return new Version(v); } catch { return new Version(1, 0); }
        }

        // ---------- UI Question Loader (works for both v1 and v2) ----------
        public List<UiQuestion> LoadQuestionsForUi(out string rulesetVersion)
        {
            if (IsV2Active())
            {
                var (ver, v2Qs, _) = LoadV2Questions();
                rulesetVersion = ver;
                return v2Qs.Select(q => new UiQuestion
                {
                    Id = q.Id,
                    Text = q.Text,
                    A = q.Options["A"].Label,
                    B = q.Options["B"].Label,
                    C = q.Options["C"].Label,
                    D = q.Options["D"].Label
                }).ToList();
            }
            else
            {
                var (ver, v1Qs) = LoadV1Questions();
                rulesetVersion = ver;
                return v1Qs.Select(q => new UiQuestion
                {
                    Id = q.Id,
                    Text = q.Text,
                    A = q.A,
                    B = q.B,
                    C = q.C,
                    D = q.D
                }).ToList();
            }
        }

        // ---------- v1 loaders ----------
        private (string rulesetVersion, List<V1Question>) LoadV1Questions()
        {
            var x = XDocument.Load(_questionsPath);
            var ver = x.Root.Attribute("rulesetVersion")?.Value ?? "1.0";
            var list = x.Root.Elements("Question").Select(q => new V1Question
            {
                Id = (string)q.Attribute("id"),
                Domain = (string)q.Attribute("domain"),
                Weight = int.Parse((string)q.Attribute("weight") ?? "1", CultureInfo.InvariantCulture),
                Text = (string)q.Element("Text"),
                A = (string)q.Element("A"),
                B = (string)q.Element("B"),
                C = (string)q.Element("C"),
                D = (string)q.Element("D"),
                Correct = ((string)q.Element("Correct") ?? "A").Trim(),
                Explanation = (string)q.Element("Explanation") ?? "",
                QuickWin = (string)q.Element("QuickWin") ?? ""
            }).ToList();
            return (ver, list);
        }

        private Dictionary<string, int> V1DomainWeights()
        {
            var map = new Dictionary<string, int>();
            var x = XDocument.Load(_rulesPath);
            foreach (var d in x.Root.Element("Ruleset").Elements("Domain"))
            {
                var wAttr = d.Attribute("weight")?.Value;
                int w = 1;
                if (!string.IsNullOrEmpty(wAttr))
                    w = int.Parse(wAttr, CultureInfo.InvariantCulture);
                map[(string)d.Attribute("id")] = w;
            }
            return map;
        }

        private Dictionary<string, string> V1DomainTitles()
        {
            var map = new Dictionary<string, string>();
            var x = XDocument.Load(_rulesPath);
            foreach (var d in x.Root.Element("Ruleset").Elements("Domain"))
                map[(string)d.Attribute("id")] = (string)d.Attribute("title") ?? (string)d.Attribute("id");
            return map;
        }

        // ---------- v2 loaders ----------
        private (string rulesetVersion, List<V2Question> questions, Dictionary<string, V2Domain> domains) LoadV2Questions()
        {
            var ruleDoc = XDocument.Load(_rulesPath);
            var rs = ruleDoc.Root.Element("Ruleset");
            var rulesVersion = (string)rs.Attribute("version") ?? "2.0";

            var domains = rs.Elements("Domain")
                .Select(d => new V2Domain
                {
                    Id = (string)d.Attribute("id"),
                    Title = (string)d.Attribute("title") ?? (string)d.Attribute("id"),
                    QuickWin = (string)d.Attribute("quickWin") ?? "",
                    Factor = (string)d.Attribute("factor") ?? ""
                })
                .ToDictionary(x => x.Id, x => x);

            var qdoc = XDocument.Load(_questionsPath);
            var v2Qs = new List<V2Question>();
            foreach (var qn in qdoc.Root.Elements("Question"))
            {
                var q = new V2Question { Id = (string)qn.Attribute("id"), Text = (string)qn.Element("Text") };
                foreach (var key in new[] { "A", "B", "C", "D" })
                {
                    var on = qn.Element(key);
                    var label = (on?.Nodes().OfType<XText>().FirstOrDefault()?.Value ?? "").Trim();
                    var opt = new V2Option { Label = label };
                    foreach (var map in on?.Elements("Map") ?? Enumerable.Empty<XElement>())
                    {
                        var mod = (string)map.Attribute("module");
                        var wt = int.Parse(((string)map.Attribute("weight")) ?? "0", CultureInfo.InvariantCulture);
                        opt.Maps.Add((mod, wt));
                    }
                    q.Options[key] = opt;
                }
                v2Qs.Add(q);
            }

            return (rulesVersion, v2Qs, domains);
        }

        // ---------- Progress storage ----------
        private XDocument EnsureProgressDoc()
        {
            if (!File.Exists(_progressPath))
            {
                var newDoc = new XDocument(new XElement("Progress"));
                newDoc.Save(_progressPath);
                return newDoc;
            }
            return XDocument.Load(_progressPath);
        }

        private XDocument EnsureResultsDoc()
        {
            if (!File.Exists(_resultsPath))
            {
                var newDoc = new XDocument(new XElement("Results"));
                newDoc.Save(_resultsPath);
                return newDoc;
            }
            return XDocument.Load(_resultsPath);
        }

        public void SaveAnswer(string username, string questionId, string selectedOption, int index, int total, string rulesetVersion)
        {
            var doc = EnsureProgressDoc();
            var user = doc.Root.Elements("User").FirstOrDefault(u => (string)u.Attribute("name") == username && (string)u.Attribute("rules") == rulesetVersion);
            if (user == null)
            {
                user = new XElement("User",
                    new XAttribute("name", username),
                    new XAttribute("rules", rulesetVersion),
                    new XAttribute("lastIndex", index),
                    new XAttribute("completed", "false"));
                doc.Root.Add(user);
            }
            else
            {
                user.SetAttributeValue("lastIndex", index);
            }

            var answers = user.Element("Answers");
            if (answers == null)
            {
                answers = new XElement("Answers");
                user.Add(answers);
            }

            var existing = answers.Elements("Answer").FirstOrDefault(a => (string)a.Attribute("qid") == questionId);
            if (existing == null)
            {
                existing = new XElement("Answer",
                    new XAttribute("qid", questionId),
                    new XAttribute("choice", selectedOption));
                answers.Add(existing);
            }
            else
            {
                existing.SetAttributeValue("choice", selectedOption);
            }

            user.SetAttributeValue("total", total);
            doc.Save(_progressPath);
        }

        public (int lastIndex, Dictionary<string, string> selections, bool completed) LoadProgress(string username, string rulesetVersion)
        {
            var doc = EnsureProgressDoc();
            var user = doc.Root.Elements("User").FirstOrDefault(u => (string)u.Attribute("name") == username && (string)u.Attribute("rules") == rulesetVersion);
            if (user == null) return (0, new Dictionary<string, string>(), false);
            var idx = int.Parse((string)user.Attribute("lastIndex") ?? "0", CultureInfo.InvariantCulture);
            var completed = string.Equals((string)user.Attribute("completed"), "true", StringComparison.OrdinalIgnoreCase);
            var map = new Dictionary<string, string>();
            var answers = user.Element("Answers");
            if (answers != null)
            {
                foreach (var a in answers.Elements("Answer"))
                    map[(string)a.Attribute("qid")] = (string)a.Attribute("choice");
            }
            return (idx, map, completed);
        }

        public void MarkCompleted(string username, string rulesetVersion)
        {
            var doc = EnsureProgressDoc();
            var user = doc.Root.Elements("User").FirstOrDefault(u => (string)u.Attribute("name") == username && (string)u.Attribute("rules") == rulesetVersion);
            if (user != null)
            {
                user.SetAttributeValue("completed", "true");
                doc.Save(_progressPath);
            }
        }

        public bool IsCompleted(string username, string rulesetVersion)
        {
            var doc = EnsureProgressDoc();
            var user = doc.Root.Elements("User").FirstOrDefault(u => (string)u.Attribute("name") == username && (string)u.Attribute("rules") == rulesetVersion);
            return user != null && string.Equals((string)user.Attribute("completed"), "true", StringComparison.OrdinalIgnoreCase);
        }

        // ---------- Compute & Save (version-aware) ----------
        public ScoreResult ComputeAndSaveResult(string username, Dictionary<string, string> selections)
        {
            return IsV2Active()
                ? ComputeAndSaveResultV2(username, selections)
                : ComputeAndSaveResultV1(username, selections);
        }

        // ----- v1 legacy (0–100, knowledge-based) -----
        private ScoreResult ComputeAndSaveResultV1(string username, Dictionary<string, string> selections)
        {
            var (rulesetVersion, questions) = LoadV1Questions();
            var weights = V1DomainWeights();
            var titles = V1DomainTitles();

            var domainTotals = new Dictionary<string, int>();
            var domainEarned = new Dictionary<string, int>();
            var domainQ = new Dictionary<string, List<V1Question>>();

            foreach (var q in questions)
            {
                if (!domainTotals.ContainsKey(q.Domain)) domainTotals[q.Domain] = 0;
                if (!domainEarned.ContainsKey(q.Domain)) domainEarned[q.Domain] = 0;
                if (!domainQ.ContainsKey(q.Domain)) domainQ[q.Domain] = new List<V1Question>();
                domainTotals[q.Domain] += q.Weight;
                domainQ[q.Domain].Add(q);

                if (selections.TryGetValue(q.Id, out var pick) && string.Equals(pick, q.Correct, StringComparison.OrdinalIgnoreCase))
                {
                    domainEarned[q.Domain] += q.Weight;
                }
            }

            double weightSum = Math.Max(1, weights.Values.Sum());
            double overall = 0;
            var domainScores = new Dictionary<string, double>();

            foreach (var d in domainTotals.Keys)
            {
                var frac = domainTotals[d] == 0 ? 0 : (double)domainEarned[d] / domainTotals[d];
                var score = Math.Round(frac * 100.0, 1);
                var title = titles.ContainsKey(d) ? titles[d] : d;
                domainScores[title] = score;

                var dw = weights.ContainsKey(d) ? weights[d] : 1;
                overall += (frac * dw);
            }
            overall = Math.Round((overall / weightSum) * 100.0, 1);

            var lowest = domainTotals.Keys
                .Select(d => new { Domain = d, Score = domainTotals[d] == 0 ? 0 : (double)domainEarned[d] / domainTotals[d] })
                .OrderBy(x => x.Score)
                .Take(3).ToList();

            var factors = new List<string>();
            var quickWins = new List<string>();
            foreach (var item in lowest)
            {
                var nice = titles.ContainsKey(item.Domain) ? titles[item.Domain] : item.Domain;
                var missed = domainQ[item.Domain].FirstOrDefault(q =>
                    !selections.TryGetValue(q.Id, out var pick) || !string.Equals(pick, q.Correct, StringComparison.OrdinalIgnoreCase));
                if (missed != null)
                {
                    if (!string.IsNullOrWhiteSpace(missed.Explanation))
                        factors.Add($"{nice}: {missed.Explanation}");
                    if (!string.IsNullOrWhiteSpace(missed.QuickWin))
                        quickWins.Add(missed.QuickWin);
                }
            }
            quickWins = quickWins.Distinct().Take(2).ToList();

            var result = new ScoreResult
            {
                Username = username,
                RulesetVersion = rulesetVersion,
                CompletedUtc = DateTime.UtcNow,
                OverallScore = overall,
                DomainScores = domainScores,
                TopFactors = factors,
                QuickWins = quickWins,
                ShareWithHelper = false
            };

            SaveResult(result);
            MarkCompleted(username, rulesetVersion);
            return result;
        }

        // ----- v2 life-context mapping (0–10 per module) -----
        private ScoreResult ComputeAndSaveResultV2(string username, Dictionary<string, string> selections)
        {
            var (rulesVersion, v2Qs, domains) = LoadV2Questions();

            // accumulate module scores from selected option → Map(module, weight)
            var scores = domains.Keys.ToDictionary(k => k, k => 0.0);
            foreach (var q in v2Qs)
            {
                if (!selections.TryGetValue(q.Id, out var pick)) continue;
                if (!q.Options.TryGetValue(pick, out var opt)) continue;
                foreach (var (module, weight) in opt.Maps)
                {
                    if (!scores.ContainsKey(module)) continue;
                    scores[module] += weight;
                }
            }

            // clamp to 0..10
            foreach (var k in scores.Keys.ToList())
            {
                if (scores[k] < 0) scores[k] = 0;
                if (scores[k] > 10) scores[k] = 10;
            }

            // overall = mean of modules (0..10)
            var overall = scores.Values.Any() ? scores.Values.Average() : 0.0;

            // Top 3 (highest-need modules → we surface their factor & quick win)
            var ordered = scores.OrderByDescending(kv => kv.Value)
                                .ThenBy(kv => domains[kv.Key].Title)
                                .Take(3).ToList();
            var factors = ordered.Select(kv => $"{domains[kv.Key].Title}: {domains[kv.Key].Factor}").ToList();
            var wins = ordered.Select(kv => domains[kv.Key].QuickWin)
                              .Where(s => !string.IsNullOrWhiteSpace(s))
                              .Distinct()
                              .Take(3).ToList();

            // Title → score
            var titled = scores.ToDictionary(k => domains[k.Key].Title, v => Math.Round(v.Value, 2));

            var result = new ScoreResult
            {
                Username = username,
                RulesetVersion = rulesVersion,
                CompletedUtc = DateTime.UtcNow,
                OverallScore = Math.Round(overall, 2),
                DomainScores = titled,
                TopFactors = factors,
                QuickWins = wins,
                ShareWithHelper = false
            };

            SaveResult(result);
            MarkCompleted(username, rulesVersion);
            return result;
        }

        // ---------- Results storage ----------
        private void SaveResult(ScoreResult r)
        {
            var doc = EnsureResultsDoc();

            var user = new XElement("UserResult",
                new XAttribute("name", r.Username),
                new XAttribute("rules", r.RulesetVersion),
                new XAttribute("completedUtc", r.CompletedUtc.ToString("o")),
                new XAttribute("overall", r.OverallScore.ToString(CultureInfo.InvariantCulture)),
                new XAttribute("shareWithHelper", r.ShareWithHelper ? "true" : "false"));

            var domains = new XElement("Domains",
                r.DomainScores.Select(kv => new XElement("Domain",
                    new XAttribute("title", kv.Key),
                    new XAttribute("score", kv.Value.ToString(CultureInfo.InvariantCulture)))));

            var factors = new XElement("TopFactors",
                r.TopFactors.Select(f => new XElement("Factor", f)));

            var wins = new XElement("QuickWins",
                r.QuickWins.Select(w => new XElement("Win", w)));

            user.Add(domains);
            user.Add(factors);
            user.Add(wins);

            // keep history; append
            doc.Root.Add(user);
            doc.Save(_resultsPath);
        }

        public ScoreResult LoadLatestResult(string username)
        {
            var doc = EnsureResultsDoc();
            var latest = doc.Root.Elements("UserResult")
                .Where(e => (string)e.Attribute("name") == username)
                .OrderByDescending(e => (string)e.Attribute("completedUtc"))
                .FirstOrDefault();

            if (latest == null) return null;

            var r = new ScoreResult
            {
                Username = username,
                RulesetVersion = (string)latest.Attribute("rules"),
                CompletedUtc = DateTime.Parse((string)latest.Attribute("completedUtc"), null, DateTimeStyles.RoundtripKind),
                OverallScore = double.Parse((string)latest.Attribute("overall"), CultureInfo.InvariantCulture),
                ShareWithHelper = string.Equals((string)latest.Attribute("shareWithHelper"), "true", StringComparison.OrdinalIgnoreCase)
            };

            var domains = latest.Element("Domains");
            if (domains != null)
            {
                foreach (var d in domains.Elements("Domain"))
                {
                    r.DomainScores[(string)d.Attribute("title")] = double.Parse((string)d.Attribute("score"), CultureInfo.InvariantCulture);
                }
            }
            var tf = latest.Element("TopFactors");
            if (tf != null) r.TopFactors = tf.Elements("Factor").Select(x => x.Value).ToList();
            var qw = latest.Element("QuickWins");
            if (qw != null) r.QuickWins = qw.Elements("Win").Select(x => x.Value).ToList();

            return r;
        }

        public void SetShareWithHelper(string username, bool share)
        {
            var doc = EnsureResultsDoc();
            var items = doc.Root.Elements("UserResult").Where(e => (string)e.Attribute("name") == username);
            foreach (var e in items) e.SetAttributeValue("shareWithHelper", share ? "true" : "false");
            doc.Save(_resultsPath);
        }
    }
}
