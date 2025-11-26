<%@ Page Language="C#" AutoEventWireup="true"
    CodeBehind="CatalogHistory.aspx.cs"
    Inherits="CyberApp_FIA.Account.UniversityAdminCatalogHistory" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
  <title>FIA • Catalog History (University)</title>
  <meta name="viewport" content="width=device-width, initial-scale=1" />
  <link href="https://fonts.googleapis.com/css2?family=Lato:wght@300;400&family=Poppins:wght@600&display=swap"
        rel="stylesheet" />
  <style>
    :root{
      --fia-pink:#f06aa9;--fia-blue:#2a99db;--fia-teal:#45c3b3;
      --ink:#111827;--muted:#6b7280;--border:#e5e7eb;--card:#ffffff;--bg:#f9fafb;
    }
    *{box-sizing:border-box;}
    body{margin:0;font-family:'Lato',sans-serif;background:var(--bg);color:var(--ink);}
    .wrap{max-width:1024px;margin:0 auto;padding:24px 16px 40px;}
    h1{font-family:'Poppins',sans-serif;font-size:1.4rem;margin:0 0 4px;}
    .sub{font-size:0.9rem;color:var(--muted);margin:0 0 16px;}
    .card{background:var(--card);border-radius:18px;padding:16px 18px;border:1px solid var(--border);
          box-shadow:0 10px 30px rgba(15,23,42,.06);}
    .meta{display:flex;justify-content:space-between;align-items:center;margin-bottom:8px;font-size:0.8rem;color:var(--muted);}
    .badge{display:inline-block;padding:2px 8px;border-radius:999px;font-size:0.75rem;background:#eff6ff;color:#1d4ed8;}
    .table-wrap{overflow-x:auto;}
  </style>
</head>
<body>
  <form id="form1" runat="server">
    <div class="wrap">
      <h1>Catalog History (Your University)</h1>
      <p class="sub">
        Read-only history of course and event changes affecting your university’s catalog.
      </p>

      <div class="card">
        <div class="meta">
          <asp:Label ID="ResultCount" runat="server" />
          <span class="badge">Read-only</span>
        </div>

        <div class="table-wrap">
          <asp:GridView ID="HistoryGrid" runat="server"
            AutoGenerateColumns="False"
            AllowPaging="True"
            PageSize="25"
            OnPageIndexChanging="HistoryGrid_PageIndexChanging">
            <Columns>
              <asp:BoundField DataField="TimestampUtc" HeaderText="Time (UTC)" DataFormatString="{0:yyyy-MM-dd HH:mm}" />
              <asp:BoundField DataField="ActorEmail" HeaderText="Actor" />
              <asp:BoundField DataField="ActorRole" HeaderText="Role" />
              <asp:BoundField DataField="ActionType" HeaderText="Action" />
              <asp:BoundField DataField="TargetType" HeaderText="Target type" />
              <asp:BoundField DataField="TargetLabel" HeaderText="Target" />
            </Columns>
          </asp:GridView>
        </div>
      </div>
    </div>
  </form>
</body>
</html>
