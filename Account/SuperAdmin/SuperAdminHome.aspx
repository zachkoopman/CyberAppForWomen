<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="SuperAdminHome.aspx.cs" Inherits="CyberApp_FIA.Account.SuperAdminHome" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
  <title>FIA • Super Admin</title>
  <meta name="viewport" content="width=device-width, initial-scale=1" />
  <link href="https://fonts.googleapis.com/css2?family=Lato:wght@300;400&family=Poppins:wght@600&display=swap" rel="stylesheet" />

  <style>
    /* ========= Design tokens (brand colors, text colors, focus ring) ========= */
    :root{--fia-pink:#f06aa9;--fia-blue:#2a99db;--fia-teal:#45c3b3;--ink:#1c1c1c;--muted:#6b7280;--ring:rgba(42,153,219,.25)}

    /* ========= Base layout ========= */
    *{box-sizing:border-box} html,body{height:100%}
    body{
      margin:0;
      font-family:Lato,Arial,sans-serif;
      background:linear-gradient(135deg,#fff,#f9fbff);
      color:var(--ink)
    }

    /* ========= Page wrapper ========= */
    .wrap{
      min-height:100vh;
      padding:24px;
      max-width:1100px;
      margin:0 auto
    }

    /* ========= Header bar (brand + welcome + sign-out) ========= */
    .header{display:flex;align-items:center;justify-content:space-between;gap:14px;margin-bottom:16px}
    .brand{display:flex;align-items:center;gap:10px}
    .badge{
      width:42px;height:42px;border-radius:12px;
      background:linear-gradient(135deg,var(--fia-pink),var(--fia-blue));
      display:grid;place-items:center;color:#fff;font-family:Poppins
    }
    h1{font-family:Poppins;margin:0;font-size:1.35rem}
    .hello{color:var(--muted);font-size:.95rem}

    /* ========= Content cards & typography ========= */
    .cards{display:grid;grid-template-columns:1fr;gap:16px}
    .card{
      background:#fff;border:1px solid #e8eef7;border-radius:20px;
      box-shadow:0 12px 36px rgba(42,153,219,.08);padding:20px
    }
    .card h2{font-family:Poppins;margin:0 0 8px 0;font-size:1.15rem}
    .sub{color:var(--muted);margin:0 0 12px 0}

    /* ========= Form controls ========= */
    label{font-weight:600;font-family:Poppins;font-size:.95rem}
    input[type=text], input[type=url], textarea, select{
      width:100%;padding:12px 14px;border-radius:12px;border:1px solid #e5e7eb
    }
    textarea{min-height:110px;resize:vertical}
    input:focus, textarea:focus, select:focus{outline:0;box-shadow:0 0 0 5px var(--ring);border-color:var(--fia-blue)}

    /* Two-column form grid (collapses on small screens) */
    .grid{display:grid;grid-template-columns:1fr 1fr;gap:14px}
    @media (max-width:800px){.grid{grid-template-columns:1fr}}

    /* Buttons */
    .btnrow{display:flex;gap:10px;flex-wrap:wrap;margin-top:14px}
    .btn{border:0;border-radius:12px;padding:12px 18px;font-weight:700;font-family:Poppins;cursor:pointer}
    .primary{color:#fff;background:linear-gradient(135deg,var(--fia-blue),var(--fia-teal))}
    .link{background:#fff;border:2px solid var(--fia-blue);color:var(--fia-blue)}

    /* Helper styles */
    .note{background:#f6f7fb;border:1px solid #e8eef7;border-radius:12px;padding:12px;color:var(--muted);font-size:.95rem}
    .pill{display:inline-block;padding:6px 10px;border-radius:999px;background:#f6f7fb;border:1px solid #e8eef7;margin-right:8px;font-size:.9rem}
    .list li{margin:.35rem 0}
    .val{color:#c21d1d;font-size:.9rem;margin-top:4px}
    .fieldset{border:1px dashed #e5e7eb;border-radius:14px;padding:12px}
    .cbgrid{display:grid;grid-template-columns:1fr 1fr;gap:8px}
    @media (max-width:800px){.cbgrid{grid-template-columns:1fr}}

    /* Stacked one-per-line CheckBoxList for prerequisites */
    .cblist table{ width:100%; border-collapse:collapse; }
    .cblist td{
     padding:8px 6px;
    border-bottom:1px dashed #e5e7eb;
    vertical-align:middle;
    }
    .cblist input{ margin-right:8px; }
  </style>
</head>

<body>
<form id="form1" runat="server">
  <div class="wrap">

    <!-- ========================== Header ========================== -->
    <div class="header">
      <div class="brand">
        <div class="badge">FIA</div>
        <div>
          <h1>Super Admin Home</h1>
          <div class="hello">
            Welcome, <asp:Literal ID="WelcomeName" runat="server" />.
          </div>
        </div>
      </div>

      <!-- Sign-out (no validation on click) -->
      <div>
        <asp:Button
          ID="BtnLogout"
          runat="server"
          Text="Sign out"
          CssClass="btn link"
          OnClick="BtnLogout_Click"
          CausesValidation="false"/>
      </div>
    </div>

    <div class="cards">
      <!-- ===================== Microcourse Creation ===================== -->
      <div class="card">
        <h2>Add a new microcourse</h2>
        <p class="sub">Consistent fields help University Admins understand and adopt content quickly.</p>

        <div class="grid">
          <!-- Title -->
          <div>
            <label for="Title">Title</label>
            <asp:TextBox ID="Title" runat="server" />
            <asp:RequiredFieldValidator runat="server" ControlToValidate="Title" CssClass="val"
              ErrorMessage="Title is required." Display="Dynamic" />
          </div>

          <!-- Duration -->
          <div>
            <label for="Duration">Duration</label>
            <asp:TextBox ID="Duration" runat="server" />
            <asp:RequiredFieldValidator runat="server" ControlToValidate="Duration" CssClass="val"
              ErrorMessage="Duration is required." Display="Dynamic" />
          </div>

          <!-- Summary -->
          <div style="grid-column:1/-1">
            <label for="Summary">Summary</label>
            <asp:TextBox ID="Summary" runat="server" TextMode="MultiLine" />
            <asp:RequiredFieldValidator runat="server" ControlToValidate="Summary" CssClass="val"
              ErrorMessage="Summary is required." Display="Dynamic" />
          </div>

          <!-- External link -->
          <div style="grid-column:1/-1">
            <label for="ExternalLink">External link (slides / video / PDF)</label>
            <asp:TextBox ID="ExternalLink" runat="server" TextMode="Url" />
          </div>

          <!-- Tags -->
          <div>
            <label for="Tags">Tags (comma-separated)</label>
            <asp:TextBox ID="Tags" runat="server" />
          </div>

          <!-- Status -->
          <div>
            <label for="Status">Status</label>
            <asp:DropDownList ID="Status" runat="server">
              <asp:ListItem Text="Draft" Value="Draft" />
              <asp:ListItem Text="Published" Value="Published" />
              <asp:ListItem Text="Deprecated" Value="Deprecated" />
            </asp:DropDownList>
          </div>

          <!-- Certification rules multi-select (checkbox list) -->
          <div style="grid-column:1/-1">
            <label>Certification rules required (multi-select)</label>
            <div class="fieldset">
              <asp:CheckBoxList ID="RulesList" runat="server" CssClass="cbgrid" RepeatLayout="Flow" />
              <div class="note" style="margin-top:8px">
                Selected rules become prerequisites users must satisfy to complete this microcourse.
              </div>
            </div>
          </div>

          <!-- ========= NEW: Prerequisites from Catalog ========= -->
          <div style="grid-column:1/-1">
            <label>Prerequisites (existing microcourses)</label>
            <div class="fieldset">
              <!-- Clean two-column checkbox grid; each item shows Title (ID) -->
              <asp:CheckBoxList
    ID="PrereqList"
    runat="server"
    CssClass="cblist"
    RepeatLayout="Table"
    RepeatDirection="Vertical"
    RepeatColumns="1"
    CellPadding="0"
    CellSpacing="0" />

              <div class="note" style="margin-top:8px">
                Choose any microcourses learners must complete before starting this one.
              </div>
            </div>
          </div>
          <!-- ========= END NEW ========= -->

        </div>

        <!-- Actions: save/clear microcourse -->
        <div class="btnrow">
          <asp:Button ID="BtnSaveMicrocourse" runat="server" Text="Save microcourse" CssClass="btn primary" OnClick="BtnSaveMicrocourse_Click" />
          <asp:Button ID="BtnClear" runat="server" Text="Clear" CssClass="btn link" OnClick="BtnClear_Click" CausesValidation="false" />
        </div>

        <!-- Vertical spacing preserved exactly as provided -->
        <p />
        <p />
        <p />
        <p />
        <p />
        <p />

        <!-- ===================== Manage Certification Rules ===================== -->
        <div class="card">
          <h2 style="margin-top:0">Certification rules</h2>
          <p class="sub">Create, edit, and set prerequisites for certification requirements.</p>
          <p />
          <p />
          <a class="btn link" href="<%: ResolveUrl("~/Account/SuperAdmin/CertificationRules.aspx") %>">Open Certification Rules</a>
        </div>

        <!-- General message area (status/errors), viewstate off to avoid stale text -->
        <asp:Label ID="FormMessage" runat="server" EnableViewState="false" />
      </div>
    </div>
  </div>
</form>
</body>
</html>



