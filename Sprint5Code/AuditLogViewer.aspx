<%@ Page Language="C#" AutoEventWireup="true"
    CodeBehind="AuditLogViewer.aspx.cs"
    Inherits="CyberApp_FIA.Account.UniversityAdminAuditLogViewer" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
  <title>FIA • University Audit Log</title>
  <meta name="viewport" content="width=device-width, initial-scale=1" />
  <link
    href="https://fonts.googleapis.com/css2?family=Lato:wght@300;400&family=Poppins:wght@600&display=swap"
    rel="stylesheet" />
  <style>
    /* Use the same design tokens as Super Admin viewer for consistency */
    :root{
      --fia-pink:#f06aa9;--fia-blue:#2a99db;--fia-teal:#45c3b3;
      --ink:#111827;--muted:#6b7280;--ring:rgba(42,153,219,.25);
      --card:#ffffff;--bg:#f9fafb;--border:#e5e7eb;
    }
    *{box-sizing:border-box;}
    body{margin:0;font-family:'Lato',sans-serif;background:var(--bg);color:var(--ink);}
    .wrap{max-width:1120px;margin:0 auto;padding:24px 16px 40px;}
    .header{margin-bottom:16px;}
    h1{font-family:'Poppins',sans-serif;margin:0;font-size:1.3rem;}
    .sub{margin-top:4px;color:var(--muted);font-size:0.9rem;}
    .card{background:var(--card);border-radius:18px;padding:16px 18px;border:1px solid var(--border);
          box-shadow:0 10px 30px rgba(15,23,42,.06);}
    .meta{display:flex;justify-content:space-between;align-items:center;margin-bottom:8px;font-size:0.8rem;color:var(--muted);}
    .table-wrap{overflow-x:auto;}
    .badge{display:inline-block;padding:2px 8px;border-radius:999px;font-size:0.75rem;background:#eff6ff;color:#1d4ed8;}
    .link{font-size:0.8rem;color:var(--fia-blue);text-decoration:none;}
    .link:hover{text-decoration:underline;}
  </style>
</head>
<body>
  <form id="form1" runat="server">
    <div class="wrap">
      <div class="header">
        <h1>University Audit Log</h1>
        <p class="sub">
          Read-only view of actions that affected your university’s participants, helpers,
          and catalog. You cannot change or delete history here.
        </p>
      </div>

      <div class="card">
        <div class="meta">
          <asp:Label ID="ResultCount" runat="server" />
          <span class="badge">Read-only</span>
        </div>

        <div class="table-wrap">
          <asp:GridView ID="AuditGrid" runat="server"
            AutoGenerateColumns="False"
            AllowPaging="True"
            PageSize="25"
            OnPageIndexChanging="AuditGrid_PageIndexChanging">
            <Columns>
              <asp:BoundField DataField="TimestampUtc" HeaderText="Time (UTC)"
                DataFormatString="{0:yyyy-MM-dd HH:mm}" />
              <asp:BoundField DataField="ActorRole" HeaderText="Role" />
              <asp:BoundField DataField="ActorEmail" HeaderText="Actor" />
              <asp:BoundField DataField="Category" HeaderText="Category" />
              <asp:BoundField DataField="ActionType" HeaderText="Action" />
              <asp:BoundField DataField="TargetLabel" HeaderText="Target" />
              <asp:BoundField DataField="ClientIpMasked" HeaderText="IP (masked)" />
            </Columns>
          </asp:GridView>
        </div>
      </div>
    </div>
  </form>
</body>
</html>
