<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CertificationRules.aspx.cs" Inherits="CyberApp_FIA.Account.CertificationRules" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
  <title>FIA • Certification Rules</title>
  <meta name="viewport" content="width=device-width, initial-scale=1" />

  <!-- Google Fonts -->
  <link
    href="https://fonts.googleapis.com/css2?family=Lato:wght@300;400&family=Poppins:wght@600&display=swap"
    rel="stylesheet" />

  <style>
    /* =========================
       Design tokens / base theme
       ========================= */
    :root{
      --fia-pink:#f06aa9;
      --fia-blue:#2a99db;
      --fia-teal:#45c3b3;
      --ink:#1c1c1c;
      --muted:#6b7280;
      --ring:rgba(42,153,219,.25);
    }

    /* =========================
       Page layout and typography
       ========================= */
    body{
      margin:0;max-width:100%;overflow-x;
      font-family:Lato,Arial,sans-serif;
      background:linear-gradient(135deg,#fff,#f9fbff);
      color:var(--ink);
      line-height:1.55;
    }
    .wrap{
      min-height:100vh;
      padding:24px;
      max-width:1100px;
      margin:0 auto;
    }

    /* =========================
       Header (brand + title)
       ========================= */
    .brand{
      display:flex;
      align-items:center;
      gap:10px;
      margin-bottom:10px;
    }
    .badge{
      width:42px;
      height:42px;
      border-radius:12px;
      background:linear-gradient(135deg,var(--fia-pink),var(--fia-blue));
      display:grid;
      place-items:center;
      color:#fff;
      font-family:Poppins;
    }
    h1{
      font-family:Poppins;
      margin:0 0 6px 0;
      font-size:1.35rem;
    }
    .sub{
      color:var(--muted);
      margin:0 0 16px 0;
    }

    /* =========================
       Grid + cards
       ========================= */
    .grid{ display:grid; grid-template-columns:1fr 1fr; gap:16px }
    @media (max-width:960px){ .grid{ grid-template-columns:1fr } }

    .card{
      background:#fff;
      border:1px solid #e8eef7;
      border-radius:20px;
      box-shadow:0 12px 36px rgba(42,153,219,.08);
      padding:18px;
    }

    /* =========================
       Form controls
       ========================= */
    label{ font-weight:600; font-family:Poppins; font-size:.95rem }
    input[type=text],
    textarea,
    input[type=number]{
      width:100%;
      padding:11px 12px;
      border-radius:12px;
      border:1px solid #e5e7eb;
      min-height:44px;
      font-size:0.95rem;
    }
    input:focus,
    textarea:focus{
      outline:0;
      box-shadow:0 0 0 5px var(--ring);
      border-color:var(--fia-blue);
    }

    /* Buttons */
    .btn{
      border:0;
      border-radius:12px;
      padding:11px 16px;
      font-weight:700;
      font-family:Poppins;
      cursor:pointer;
      min-height:44px;
    }
    .primary{ color:#fff; background:linear-gradient(135deg,var(--fia-blue),var(--fia-teal)) }
    .link{ background:#fff; border:2px solid var(--fia-blue); color:var(--fia-blue) }

    /* Tables */
    .table-wrap{width:100%;overflow-x:hidden;}
    table{ width:100%; border-collapse:collapse;font-size:.92rem;}
    th,td{ padding:10px 8px; border-bottom:1px solid #f0f3f9; text-align:left }

    /* Helpers */
    .pill{
      display:inline-block;
      padding:6px 10px;
      border-radius:999px;
      background:#f6f7fb;
      border:1px solid #e8eef7;
      margin-right:8px;
      font-size:.9rem;
    }
    .fieldset{ border:1px dashed #e5e7eb; border-radius:14px; padding:10px; margin-top:6px; }
    .muted{ color:var(--muted); font-size:.85rem; }
    .quiz-toggle-row{
      display:flex;
      align-items:center;
      justify-content:space-between;
      gap:12px;
      margin-top:8px;
      margin-bottom:4px;
      flex-wrap:wrap;
    }

    .form-actions{
        display:flex;
        gap:10px;
        margin-top:12px;
        flex-wrap:wrap;
    }

    /* MOBILE OPTIMIZATION */
    @media (max-width:720px){
        .wrap{padding:18px 14px 26px;}
        h1{font-size:1.25rem;}
        .sub{font-size:.9rem;}
        .card{padding:16px;}
        .form-actions{flex-direction:column;}
        .form-actions .btn{width:100%;}
        table{font-size:.85rem;}

    }

    @media (max-width:430px){
        .wrap{padding:16px 12px 24px;}
        h1{font-size:1.15rem;}
        table{font-size:.8rem;}

    }

  </style>
</head>

<body>
  <form id="form1" runat="server">
    <div class="wrap">

      <!-- =========================================================
           Page header (brand + title + short description)
           ========================================================= -->
      <div class="brand">
        <div class="badge">FIA</div>
        <div>
          <h1>Certification Rules</h1>
          <div class="sub">Define what Helpers must complete to earn or keep a certification.</div>
        </div>
      </div>

      <!-- =========================================================
           Two-column layout:
           - Left: Rule editor form
           - Right: Rules table/list
           ========================================================= -->
      <div class="grid">

        <!-- ===================== Rule editor (left card) ===================== -->
        <div class="card">
          <h2 style="margin-top:0">Add / Edit Rule</h2>

          <!-- Rule ID -->
          <div>
            <label for="RuleId">Rule ID (unique, stable)</label>
            <asp:TextBox ID="RuleId" runat="server" />
          </div>

          <!-- Name -->
          <div style="margin-top:8px">
            <label for="RuleName">Name</label>
            <asp:TextBox ID="RuleName" runat="server" />
          </div>

          <!-- Description -->
          <div style="margin-top:8px">
            <label for="RuleDesc">Description</label>
            <asp:TextBox ID="RuleDesc" runat="server" TextMode="MultiLine" Rows="3" />
          </div>

          <!-- Quiz requirement -->
          <div style="margin-top:10px">
            <div class="quiz-toggle-row">
              <div>
                <label>Quiz requirement</label>
                <div class="muted">Toggle on if this certification requires a quiz.</div>
              </div>
              <div>
                <asp:CheckBox ID="RequireQuiz" runat="server"
                              AutoPostBack="true"
                              OnCheckedChanged="RequireQuiz_CheckedChanged" />
                <span class="muted">Require quiz</span>
              </div>
            </div>

            <asp:Panel ID="PassScorePanel" runat="server" CssClass="fieldset">
              <label for="PassScore">Pass score %</label>
              <asp:TextBox ID="PassScore" runat="server" TextMode="Number" />
              <div class="muted">0–100. Leave 0 if you are still drafting this rule.</div>
            </asp:Panel>
          </div>

          <!-- Numeric fields: teaching sessions, 1:1 help, expiry -->
          <div style="margin-top:10px">
            <div class="grid" style="grid-template-columns:1fr 1fr;gap:12px">
              <div>
                <label for="MinSessions">Teaching sessions (min)</label>
                <asp:TextBox ID="MinSessions" runat="server" TextMode="Number" />
              </div>
              <div>
                <label for="HelpSessions">1:1 help sessions (min)</label>
                <asp:TextBox ID="HelpSessions" runat="server" TextMode="Number" />
              </div>
              <div>
                <label for="ExpiryDays">Expiry (days)</label>
                <asp:TextBox ID="ExpiryDays" runat="server" TextMode="Number" />
              </div>
            </div>
          </div>

          <!-- Form actions: Save / Clear / Delete -->
          <div style="display:flex;gap:10px;margin-top:12px">
            <asp:Button
              ID="BtnSave"
              runat="server"
              CssClass="btn primary"
              Text="Save rule"
              OnClick="BtnSave_Click" />
            <asp:Button
              ID="BtnClear"
              runat="server"
              CssClass="btn link"
              Text="Clear"
              OnClick="BtnClear_Click"
              CausesValidation="false" />
            <asp:Button
              ID="BtnDelete"
              runat="server"
              CssClass="btn link"
              Text="Delete"
              OnClick="BtnDelete_Click"
              CausesValidation="false" />
          </div>

          <!-- Inline form status / messages -->
          <asp:Label ID="FormMessage" runat="server" />
        </div>

        <!-- ===================== Rules list (right card) ===================== -->
        <div class="card">
          <h2 style="margin-top:0">Existing rules</h2>

          <!-- Empty state when there are no rules yet -->
          <asp:PlaceHolder ID="NoRulesPH" runat="server" Visible="false">
            <div class="pill">No rules yet. Create one on the left.</div>
          </asp:PlaceHolder>

          <!-- Rules table rendered with a Repeater -->
          <asp:Repeater ID="RulesRepeater" runat="server" OnItemCommand="RulesRepeater_ItemCommand">
            <HeaderTemplate>
              <table>
                <thead>
                  <tr>
                    <th>ID</th>
                    <th>Name</th>
                    <th>Requires quiz?</th>
                    <th>Pass</th>
                    <th>Teaching sessions</th>
                    <th>1:1 help</th>
                    <th>Actions</th>
                  </tr>
                </thead>
                <tbody>
            </HeaderTemplate>

            <ItemTemplate>
              <tr>
                <td><%# Eval("id") %></td>
                <td><%# Eval("name") %></td>
                <td><%# Eval("requireQuizText") %></td>
                <td><%# Eval("passScore") %>%</td>
                <td><%# Eval("minSessions") %></td>
                <td><%# Eval("minHelpSessions") %></td>
                <td>
                  <asp:LinkButton
                    runat="server"
                    CommandName="edit"
                    CommandArgument='<%# Eval("id") %>'
                    Text="Edit" />
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

      <!-- Back navigation -->
      <div style="margin-top:16px">
        <a class="pill" href="<%: ResolveUrl("~/Account/SuperAdmin/SuperAdminHome.aspx") %>">← Back to Super Admin Home</a>
      </div>

    </div>
  </form>
</body>
</html>

