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
      margin:0;
      font-family:Lato,Arial,sans-serif;
      background:linear-gradient(135deg,#fff,#f9fbff);
      color:var(--ink);
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
      padding:10px 12px;
      border-radius:12px;
      border:1px solid #e5e7eb;
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
      padding:10px 14px;
      font-weight:700;
      font-family:Poppins;
      cursor:pointer;
    }
    .primary{ color:#fff; background:linear-gradient(135deg,var(--fia-blue),var(--fia-teal)) }
    .link{ background:#fff; border:2px solid var(--fia-blue); color:var(--fia-blue) }

    /* Tables */
    table{ width:100%; border-collapse:collapse }
    th,td{ padding:8px; border-bottom:1px solid #f0f3f9; text-align:left }

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
    .fieldset{ border:1px dashed #e5e7eb; border-radius:14px; padding:10px }
    .cbgrid{ display:grid; grid-template-columns:1fr 1fr; gap:8px }
    @media (max-width:960px){ .cbgrid{ grid-template-columns:1fr } }
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
          <div class="sub">Create and update certification requirements without changing code.</div>
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
          <div>
            <label for="RuleName">Name</label>
            <asp:TextBox ID="RuleName" runat="server" />
          </div>

          <!-- Description -->
          <div>
            <label for="RuleDesc">Description</label>
            <asp:TextBox ID="RuleDesc" runat="server" TextMode="MultiLine" Rows="3" />
          </div>

          <!-- Numeric fields, laid out in a small grid -->
          <div class="grid" style="grid-template-columns:1fr 1fr;gap:12px;margin-top:8px">
            <div>
              <label for="PassScore">Pass score %</label>
              <asp:TextBox ID="PassScore" runat="server" TextMode="Number" />
            </div>
            <div>
              <label for="MinSessions">Min sessions taught</label>
              <asp:TextBox ID="MinSessions" runat="server" TextMode="Number" />
            </div>
            <div>
              <label for="ExpiryDays">Expiry (days)</label>
              <asp:TextBox ID="ExpiryDays" runat="server" TextMode="Number" />
            </div>
            <div>
              <label for="MaxAttempts">Max attempts (0=∞)</label>
              <asp:TextBox ID="MaxAttempts" runat="server" TextMode="Number" />
            </div>
            <div>
              <label for="CooldownDays">Retake cooldown (days)</label>
              <asp:TextBox ID="CooldownDays" runat="server" TextMode="Number" />
            </div>
            <div>
              <label for="Evidence">Evidence type</label>
              <asp:TextBox ID="Evidence" runat="server" Placeholder="quiz | demo | mentorApproval | mixed" />
            </div>
          </div>

          <!-- Prerequisite rules (multi-select) -->
          <div style="margin-top:8px">
            <label>Prerequisite rules (multi-select)</label>
            <div class="fieldset">
              <asp:CheckBoxList ID="PrereqList" runat="server" CssClass="cbgrid" RepeatLayout="Flow" />
              <div class="pill" style="margin-top:8px">Tip: a rule cannot depend on itself</div>
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
                    <th>Pass</th>
                    <th>Min Sessions</th>
                    <th>Actions</th>
                  </tr>
                </thead>
                <tbody>
            </HeaderTemplate>

            <ItemTemplate>
              <tr>
                <td><%# Eval("id") %></td>
                <td><%# Eval("name") %></td>
                <td><%# Eval("passScore") %>%</td>
                <td><%# Eval("minSessions") %></td>
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

