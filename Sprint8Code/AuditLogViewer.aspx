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
    html, body {height:100%;}
    body{
        margin:0;
        font-family:'Lato',sans-serif;
        background:var(--bg);
        color:var(--ink);
        line-height:1.5;
        overflow-x:hidden;
    }

    .wrap{max-width:1120px;
          margin:0 auto;
          padding:24px 16px 40px;

    }

    .header{
        margin-bottom:18px;

    }

    h1{
        font-family:'Poppins',sans-serif;
        margin:0;
        font-size:1.3rem;

    }

    .sub{
        margin-top:6px;
        color:var(--muted);
        font-size:0.95rem;
        line-height:1.45px;
        max-width:100%;
    }
    .card{
        background:var(--card);
        border-radius:18px;
        padding:18px;
        border:1px solid var(--border);
        box-shadow:0 10px 30px rgba(15,23,42,.06);

    }

    .meta{
        display:flex;
        justify-content:space-between;
        align-items:center;
        gap:10px;
        margin-bottom:10px;
        font-size:0.85rem;
        color:var(--muted);
        flex-wrap:wrap;
    }

    .table-wrap{
        overflow-x:auto;
        -webkit-overflow-scrolling:touch;
    }

    .table-wrap table{
        width:100%;
        border-collapse:collapse;
        min-width:720px;
    }

    .table-wrap th,
    .table-wrap td{
        padding:10px 12px;
        text-align:left;
        border-bottom:1px solid var(--border);
        font-size:0.9rem;
        vertical-align:top;
        white-space:nowrap;
    }

    .table-wrap th{
        font-family:'Poppins',sans-serif;
        font-size:0.85rem;
        color:var(--muted);
        background:#fafafa;
    }

    .badge{
        display:inline-block;
        padding:4px 10px;
        border-radius:999px;
        font-size:0.75rem;
        background:#eff6ff;
        color:#1d4ed8;
        white-space:nowrap;
    }

    .link{
        font-size:0.85rem;
        color:var(--fia-blue);
        text-decoration:none;
        padding:6px 8px;
        display:inline-block;
    }
    .link:hover{
        text-decoration:underline;

    }

    /* MOBILE OPTIMIZATION */
    @media (max-width:430px){
        .wrap{padding:18px 14px 32px;}
        h1{font-size:1.2rem;}
        .sub{font-size:0.95rem;}
        .card{padding:16px;border-radius:16px;}
        .meta{font-size:0.85rem;gap:6px;}
        .badge{font-size:0.7rem;padding:4px 8px;}
        .table-wrap th, .table-wrap td{padding:10px 10px;font-size:0.9rem;}

    }

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
