<%@ Page Language="C#" AutoEventWireup="true"
    CodeBehind="AuditLogViewer.aspx.cs"
    Inherits="CyberApp_FIA.Account.SuperAdminAuditLogViewer" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
  <title>FIA • Super Admin Activity Log</title>
  <meta name="viewport" content="width=device-width, initial-scale=1" />
  <link
    href="https://fonts.googleapis.com/css2?family=Lato:wght@300;400&family=Poppins:wght@600&display=swap"
    rel="stylesheet" />

  <style>
    :root {
      --fia-pink:#f06aa9;
      --fia-blue:#2a99db;
      --fia-teal:#45c3b3;
      --ink:#111827;
      --muted:#6b7280;
      --ring:rgba(42,153,219,.25);
      --card:#ffffff;
      --bg:#f9fafb;
      --border:#e5e7eb;
    }

    *{box-sizing:border-box;}
    body{
      margin:0;
      font-family:'Lato',sans-serif;
      background:var(--bg);
      color:var(--ink);
    }

    .wrap{
      max-width:1120px;
      margin:0 auto;
      padding:24px 16px 40px;
    }

    .header{
      display:flex;
      justify-content:space-between;
      align-items:center;
      margin-bottom:16px;
    }

    .brand{
      display:flex;
      align-items:center;
      gap:8px;
    }

    .pill{
      font-family:'Poppins',sans-serif;
      font-size:0.8rem;
      padding:4px 10px;
      border-radius:999px;
      background:linear-gradient(135deg,var(--fia-pink),var(--fia-blue));
      color:#fff;
    }

    h1{
      font-family:'Poppins',sans-serif;
      margin:0;
      font-size:1.4rem;
    }

    .sub{
      margin-top:4px;
      color:var(--muted);
      font-size:0.9rem;
    }

    .layout{
      display:grid;
      grid-template-columns:280px 1fr;
      gap:16px;
      align-items:flex-start;
    }

    .card{
      background:var(--card);
      border-radius:18px;
      padding:16px 18px;
      border:1px solid var(--border);
      box-shadow:0 10px 30px rgba(15,23,42,.06);
    }

    .card h2{
      font-family:'Poppins',sans-serif;
      font-size:1rem;
      margin:0 0 8px;
    }

    label{
      display:block;
      font-size:0.8rem;
      font-weight:600;
      margin-bottom:4px;
    }

    .field{
      margin-bottom:10px;
    }

    input[type=text], select{
      width:100%;
      padding:7px 9px;
      font-size:0.85rem;
      border-radius:8px;
      border:1px solid var(--border);
      outline:none;
    }

    input[type=text]:focus, select:focus{
      border-color:var(--fia-blue);
      box-shadow:0 0 0 2px var(--ring);
    }

    .row{
      display:flex;
      gap:8px;
    }

    .actions{
      display:flex;
      gap:8px;
      margin-top:6px;
    }

    .btn{
      border:none;
      border-radius:999px;
      padding:7px 14px;
      font-size:0.85rem;
      cursor:pointer;
    }

    .btn-primary{
      background:linear-gradient(135deg,var(--fia-pink),var(--fia-blue));
      color:#fff;
    }

    .btn-ghost{
      background:transparent;
      border:1px dashed var(--border);
      color:var(--muted);
    }

    .badge{
      display:inline-block;
      padding:2px 8px;
      border-radius:999px;
      font-size:0.75rem;
      background:#eff6ff;
      color:#1d4ed8;
    }

    .table-wrap{
      overflow-x:auto;
    }

    .meta{
      display:flex;
      justify-content:space-between;
      align-items:center;
      margin-bottom:8px;
      font-size:0.8rem;
      color:var(--muted);
    }

    .link{
      font-size:0.8rem;
      color:var(--fia-blue);
      text-decoration:none;
    }

    .link:hover{text-decoration:underline;}
  </style>
</head>

<body>
  <form id="form1" runat="server">
    <div class="wrap">

      <div class="header">
        <div>
          <div class="brand">
            <span class="pill">FIA Super Admin</span>
            <span style="font-size:0.8rem;color:var(--muted);">Activity &amp; Audit Log</span>
          </div>
          <h1>Trusted Activity Log</h1>
          <p class="sub">
            Read-only history of critical actions like consent, catalog changes,
            password resets, and security follow-up. No edits or deletions allowed.
          </p>
        </div>
      </div>

      <div class="layout">
        <!-- Filters column -->
        <div class="card">
          <h2>Filters</h2>

          <div class="field">
            <label for="SearchText">Search</label>
            <asp:TextBox ID="SearchText" runat="server" placeholder="Actor, action, target…" />
          </div>

          <div class="field row">
            <div style="flex:1;">
              <label for="CategoryFilter">Category</label>
              <asp:DropDownList ID="CategoryFilter" runat="server">
                <asp:ListItem Text="All" Value="" />
                <asp:ListItem Text="Auth" Value="Auth" />
                <asp:ListItem Text="Consent" Value="Consent" />
                <asp:ListItem Text="Catalog" Value="Catalog" />
                <asp:ListItem Text="Helper" Value="Helper" />
                <asp:ListItem Text="Security" Value="Security" />
                <asp:ListItem Text="System" Value="System" />
              </asp:DropDownList>
            </div>
            <div style="flex:1;">
              <label for="RoleFilter">Actor role</label>
              <asp:DropDownList ID="RoleFilter" runat="server">
                <asp:ListItem Text="All" Value="" />
                <asp:ListItem Text="SuperAdmin" Value="SuperAdmin" />
                <asp:ListItem Text="UniversityAdmin" Value="UniversityAdmin" />
                <asp:ListItem Text="Helper" Value="Helper" />
                <asp:ListItem Text="Participant" Value="Participant" />
                <asp:ListItem Text="System" Value="System" />
              </asp:DropDownList>
            </div>
          </div>

          <div class="field row">
            <div style="flex:1;">
              <label for="FromDate">From date (UTC)</label>
              <asp:TextBox ID="FromDate" runat="server" placeholder="YYYY-MM-DD" />
            </div>
            <div style="flex:1;">
              <label for="ToDate">To date (UTC)</label>
              <asp:TextBox ID="ToDate" runat="server" placeholder="YYYY-MM-DD" />
            </div>
          </div>

          <div class="actions">
            <asp:Button ID="BtnApply" runat="server"
              Text="Apply filters"
              CssClass="btn btn-primary"
              OnClick="BtnApply_Click" />
            <asp:Button ID="BtnClear" runat="server"
              Text="Clear"
              CssClass="btn btn-ghost"
              OnClick="BtnClear_Click" CausesValidation="false" />
          </div>

          <hr style="margin:14px 0;border:none;border-top:1px dashed var(--border);" />

          <p class="sub" style="margin:0;">
            Use the follow-up link in each row to jump into security actions
            like locking accounts or triggering safe password resets.
          </p>
        </div>

        <!-- Table column -->
        <div class="card">
          <div class="meta">
            <asp:Label ID="ResultCount" runat="server" />
            <span>
              <span class="badge">Read-only</span>
              &nbsp;
              <a href="#" class="link" onclick="return false;">Export (CSV coming later)</a>
            </span>
          </div>

          <div class="table-wrap">
            <asp:GridView ID="AuditGrid" runat="server"
              AutoGenerateColumns="False"
              AllowPaging="True"
              PageSize="25"
              OnPageIndexChanging="AuditGrid_PageIndexChanging"
              CssClass="fia-grid">
              <Columns>
                <asp:BoundField DataField="TimestampUtc" HeaderText="Time (UTC)"
                  DataFormatString="{0:yyyy-MM-dd HH:mm}" />
                <asp:BoundField DataField="ActorRole" HeaderText="Role" />
                <asp:BoundField DataField="ActorEmail" HeaderText="Actor" />
                <asp:BoundField DataField="Category" HeaderText="Category" />
                <asp:BoundField DataField="ActionType" HeaderText="Action" />
                <asp:BoundField DataField="TargetLabel" HeaderText="Target" />
                <asp:BoundField DataField="ClientIpMasked" HeaderText="IP (masked)" />
                <asp:TemplateField HeaderText="Follow-up">
                  <ItemTemplate>
                    <!-- Link to a future security follow-up page. Does not edit the log. -->
                    <asp:HyperLink ID="FollowUpLink" runat="server"
                      CssClass="link"
                      Text="Review"
                      NavigateUrl='<%# Eval("FollowUpUrl") %>' />
                  </ItemTemplate>
                </asp:TemplateField>
              </Columns>
            </asp:GridView>
          </div>
        </div>
      </div>

    </div>
  </form>
</body>
</html>
