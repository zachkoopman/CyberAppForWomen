<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CertificationRules.aspx.cs" Inherits="CyberApp_FIA.Account.CertificationRules" %> 

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
  <title>FIA • Certification Rules</title>
  <meta name="viewport" content="width=device-width, initial-scale=1" />

  <link href="https://fonts.googleapis.com/css2?family=Lato:wght@300;400&family=Poppins:wght@600&display=swap" rel="stylesheet" />

  <style>
    :root{
      --fia-pink:#f06aa9;
      --fia-blue:#2a99db;
      --fia-teal:#45c3b3;
      --ink:#111827;
      --muted:#6b7280;
      --border:#e5e7eb;
      --surface:#ffffff;
      --bg:#f3f4f6;
      --ring:rgba(42,153,219,.25);
    }

    *{box-sizing:border-box;}

    body{
      margin:0;
      font-family:'Lato',sans-serif;
      color:var(--ink);
      background:
        radial-gradient(circle at 0 0,rgba(240,106,169,.12),transparent 55%),
        radial-gradient(circle at 100% 100%,rgba(42,153,219,.12),transparent 55%),
        #f9fafb;
    }

    .page{
      max-width:1120px;
      margin:0 auto;
      padding:24px 16px 40px;
    }

    .topbar{
      display:flex;
      justify-content:space-between;
      align-items:center;
      gap:12px;
      margin-bottom:16px;
    }

    .brand{
      display:flex;
      align-items:center;
      gap:10px;
    }

    .logo-pill{
      width:40px;
      height:40px;
      border-radius:14px;
      background:linear-gradient(135deg,var(--fia-pink),var(--fia-blue));
      display:grid;
      place-items:center;
      color:#fff;
      font-family:'Poppins',sans-serif;
      font-weight:600;
      font-size:0.9rem;
      box-shadow:0 14px 30px rgba(15,23,42,.18);
    }

    .brand-text h1{
      font-family:'Poppins',sans-serif;
      font-size:1.35rem;
      margin:0;
    }

    .brand-text p{
      margin:2px 0 0;
      font-size:0.85rem;
      color:var(--muted);
    }

    .layout{
      display:grid;
      grid-template-columns:minmax(0,1.4fr) minmax(0,1.6fr);
      gap:18px;
    }
    @media (max-width:960px){
      .layout{grid-template-columns:minmax(0,1fr);}
    }

    .card{
      background:var(--surface);
      border-radius:20px;
      border:1px solid var(--border);
      padding:18px 18px 20px;
      box-shadow:0 18px 42px rgba(15,23,42,.06);
    }

    .card-title{
      font-family:'Poppins',sans-serif;
      font-size:1rem;
      margin:0 0 10px;
    }

    .field{
      display:flex;
      flex-direction:column;
      gap:4px;
      margin-bottom:10px;
      font-size:0.85rem;
    }

    label{
      font-weight:600;
      font-family:'Poppins',sans-serif;
      font-size:0.85rem;
    }

    input[type=text],
    input[type=number],
    textarea{
      width:100%;
      padding:9px 12px;
      border-radius:12px;
      border:1px solid var(--border);
      font-family:'Lato',sans-serif;
      font-size:0.9rem;
      transition:border-color .15s ease, box-shadow .15s ease, background .15s ease;
    }

    textarea{
      resize:vertical;
      min-height:68px;
    }

    input:focus,
    textarea:focus{
      outline:none;
      border-color:var(--fia-blue);
      box-shadow:0 0 0 3px var(--ring);
      background:#f9fafb;
    }

    .inline{
      display:flex;
      align-items:center;
      gap:8px;
      font-size:0.85rem;
    }

    .fieldset{
      margin-top:8px;
      padding:10px 12px;
      border-radius:14px;
      border:1px dashed rgba(148,163,184,.7);
      background:rgba(249,250,251,.9);
    }

    .fieldset-title{
      font-weight:600;
      font-size:0.85rem;
      margin-bottom:6px;
    }

    .hint{
      font-size:0.8rem;
      color:var(--muted);
      margin-top:2px;
    }

    .btn-row{
      margin-top:12px;
      display:flex;
      gap:8px;
      flex-wrap:wrap;
      align-items:center;
    }

    .btn{
      border-radius:999px;
      border:1px solid transparent;
      padding:8px 16px;
      font-size:0.85rem;
      font-weight:500;
      cursor:pointer;
      background:#fff;
      color:var(--ink);
      transition:background .15s ease, box-shadow .15s ease, transform .05s ease;
      font-family:'Lato',sans-serif;
    }

    .btn.primary{
      background:linear-gradient(135deg,var(--fia-pink),var(--fia-blue));
      color:#fff;
      border:none;
      box-shadow:0 10px 24px rgba(42,153,219,.35);
    }

    .btn.danger{
      background:linear-gradient(135deg,#fee2e2,#fecaca);
      color:#991b1b;
      border:none;
    }

    .btn.link{
      background:transparent;
      color:var(--fia-blue);
      border:1px dashed rgba(148,163,184,.7);
    }

    .btn:hover{
      transform:translateY(-1px);
      box-shadow:0 10px 28px rgba(15,23,42,.14);
    }

    .btn:focus{
      outline:none;
      box-shadow:0 0 0 4px var(--ring);
    }

    .status{
      font-size:0.8rem;
      color:var(--muted);
    }

    .rules-empty{
      padding:10px 12px;
      border-radius:14px;
      border:1px dashed rgba(148,163,184,.7);
      background:rgba(249,250,251,.9);
      font-size:0.85rem;
      color:var(--muted);
      text-align:center;
    }

    .rules-table{
      width:100%;
      border-collapse:collapse;
      font-size:0.85rem;
      margin-top:4px;
    }

    .rules-table th,
    .rules-table td{
      padding:8px 8px;
      border-bottom:1px solid var(--border);
      text-align:left;
    }

    .rules-table th{
      font-size:0.78rem;
      text-transform:uppercase;
      letter-spacing:.03em;
      color:var(--muted);
    }

    .edit-link{
      font-size:0.8rem;
      color:var(--fia-blue);
      text-decoration:none;
      font-weight:500;
    }
  </style>
</head>
<body>
  <form id="form1" runat="server">
    <div class="page">
      <div class="topbar">
        <div class="brand">
          <div class="logo-pill">FIA</div>
          <div class="brand-text">
            <h1>Certification rules</h1>
            <p>Define how helpers earn and keep certification for each microcourse.</p>
          </div>
        </div>
      </div>

      <div class="layout">
        <!-- Left: edit / create rule -->
        <div class="card">
          <h2 class="card-title">Create or edit rule</h2>

          <div class="field">
            <label for="RuleId">Rule ID (system key)</label>
            <asp:TextBox ID="RuleId" runat="server" />
            <div class="hint">Short, stable key used in XML (e.g., “Quiz”, “Teach3Sessions”).</div>
          </div>

          <div class="field">
            <label for="RuleName">Rule name</label>
            <asp:TextBox ID="RuleName" runat="server" />
            <div class="hint">Example: “Quiz (score ≥ 80%)”.</div>
          </div>

          <div class="field">
            <label for="RuleDesc">Brief description</label>
            <asp:TextBox ID="RuleDesc" runat="server" TextMode="MultiLine" Rows="3" />
            <div class="hint">What this rule checks for in plain language.</div>
          </div>

          <div class="inline" style="margin:8px 0;">
            <asp:CheckBox ID="RequireQuiz" runat="server" />
            <span>Helper must pass a quiz to satisfy this rule.</span>
          </div>

          <asp:Panel ID="PassScorePanel" runat="server" CssClass="fieldset">
            <div class="fieldset-title">Quiz requirement</div>
            <div class="field" style="margin-bottom:6px;">
              <label for="PassScore">Minimum quiz score (%)</label>
              <asp:TextBox ID="PassScore" runat="server" TextMode="Number" />
            </div>
            <div class="hint">Only used when “Require quiz” is checked.</div>
          </asp:Panel>

          <div class="fieldset" style="margin-top:10px;">
            <div class="fieldset-title">Teaching & help thresholds</div>
            <div class="field">
              <label for="MinSessions">Minimum teaching sessions</label>
              <asp:TextBox ID="MinSessions" runat="server" TextMode="Number" />
            </div>
            <div class="field">
              <label for="HelpSessions">Minimum 1:1 help sessions</label>
              <asp:TextBox ID="HelpSessions" runat="server" TextMode="Number" />
            </div>
            <div class="field">
              <label for="ExpiryDays">Expires after (days)</label>
              <asp:TextBox ID="ExpiryDays" runat="server" TextMode="Number" />
              <div class="hint">Leave 0 if this rule never expires once earned.</div>
            </div>
          </div>

          <div class="btn-row">
            <asp:Button ID="BtnSave"
                        runat="server"
                        Text="Save rule"
                        CssClass="btn primary"
                        OnClick="BtnSave_Click" />
            <asp:Button ID="BtnDelete"
                        runat="server"
                        Text="Delete rule"
                        CssClass="btn danger"
                        OnClick="BtnDelete_Click"
                        CausesValidation="false" />
            <asp:Button ID="BtnClear"
                        runat="server"
                        Text="Clear form"
                        CssClass="btn link"
                        OnClick="BtnClear_Click"
                        CausesValidation="false" />

            <asp:Label ID="FormMessage" runat="server" CssClass="status" />
          </div>
        </div>

        <!-- Right: existing rules list -->
        <div class="card">
          <h2 class="card-title">Existing rules</h2>
          <p class="hint" style="margin-top:0;margin-bottom:8px;">
            Rules appear here once saved. You can attach them to microcourses from the Super Admin home page.
          </p>

          <asp:PlaceHolder ID="NoRulesPH" runat="server" Visible="false">
            <div class="rules-empty">
              No certification rules yet. Create your first rule on the left and it will appear here.
            </div>
          </asp:PlaceHolder>

          <asp:Repeater ID="RulesRepeater" runat="server" OnItemCommand="RulesRepeater_ItemCommand">
            <HeaderTemplate>
              <table class="rules-table">
                <thead>
                  <tr>
                    <th>Rule</th>
                    <th>Quiz / score</th>
                    <th>Sessions</th>
                    <th>Expires</th>
                    <th></th>
                  </tr>
                </thead>
                <tbody>
            </HeaderTemplate>
            <ItemTemplate>
              <tr>
                <td>
                  <div style="font-weight:600;"><%# Eval("Name") %></div>
                  <div class="hint"><%# Eval("Description") %></div>
                </td>
                <td>
                  <%# (bool)Eval("RequireQuiz") ? "Yes (min " + Eval("PassScore") + "%)" : "No" %>
                </td>
                <td>
                  Teach: <%# Eval("MinSessions") %><br />
                  Help: <%# Eval("HelpSessions") %>
                </td>
                <td>
                  <%# (int)Eval("ExpiryDays") > 0 ? Eval("ExpiryDays") + " days" : "No expiry" %>
                </td>
                <td>
                  <asp:LinkButton runat="server"
                                  CommandName="Edit"
                                  CommandArgument='<%# Eval("RuleId") %>'
                                  CssClass="edit-link">
                    Edit
                  </asp:LinkButton>
                </td>
              </tr>
            </ItemTemplate>
            <FooterTemplate>
                </tbody>
              </table>
            </FooterTemplate>
          </asp:Repeater>
        </div>
      </div>
    </div>
  </form>
</body>
</html>
