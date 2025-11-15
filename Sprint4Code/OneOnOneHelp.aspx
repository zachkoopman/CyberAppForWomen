<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="OneOnOneHelp.aspx.cs" Inherits="CyberApp_FIA.Helper.OneOnOneHelp" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
  <title>FIA • 1:1 Help Sessions</title>
  <meta name="viewport" content="width=device-width, initial-scale=1" />

  <!-- Fonts -->
  <link href="https://fonts.googleapis.com/css2?family=Lato:wght@300;400&family=Poppins:wght@600&display=swap" rel="stylesheet" />

  <style>
    :root{
      --fia-pink:#f06aa9;
      --fia-blue:#2a99db;
      --fia-teal:#45c3b3;
      --ink:#1f2933;
      --muted:#6b7280;
      --bg:#f3f5fb;
      --card-bg:#ffffff;
      --card-border:#e2e8f0;
    }

    *{
      box-sizing:border-box;
    }

    body{
      margin:0;
      font-family:Lato, system-ui, -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif;
      background:
        radial-gradient(circle at 0 0, rgba(240,106,169,0.10), transparent 55%),
        radial-gradient(circle at 100% 100%, rgba(42,153,219,0.10), transparent 55%),
        var(--bg);
      color:var(--ink);
    }

    .wrap{
      max-width:1120px;
      margin:0 auto;
      padding:24px 16px 40px;
    }

    /* Top hero header (mirrors Schedule) */
    .page-header{
      border-radius:24px;
      padding:20px 22px 22px;
      background:
        radial-gradient(circle at 0 0, rgba(240,106,169,0.15), transparent 55%),
        radial-gradient(circle at 100% 0, rgba(69,195,179,0.20), transparent 55%),
        linear-gradient(120deg,#fbfbff,#f3f7ff);
      border:1px solid rgba(226,232,240,0.9);
      box-shadow:0 18px 40px rgba(15,23,42,0.10);
      margin-bottom:24px;
      display:flex;
      justify-content:space-between;
      align-items:flex-start;
      gap:18px;
    }

    .page-header-main{
      max-width:640px;
    }

    .page-eyebrow{
      font-size:0.8rem;
      letter-spacing:0.16em;
      text-transform:uppercase;
      color:#6b7280;
      font-weight:700;
      margin-bottom:4px;
      display:flex;
      align-items:center;
      gap:8px;
    }

    .page-eyebrow-pill{
      display:inline-flex;
      align-items:center;
      justify-content:center;
      width:22px;
      height:22px;
      border-radius:999px;
      background:linear-gradient(135deg,var(--fia-pink),var(--fia-blue));
      color:#fff;
      font-size:0.7rem;
      font-family:Poppins,system-ui,sans-serif;
    }

    .page-title{
      margin:0;
      font-family:Poppins, system-ui, sans-serif;
      font-size:1.7rem;
      background:linear-gradient(135deg,var(--fia-blue),var(--fia-pink));
      -webkit-background-clip:text;
      color:transparent;
    }

    .page-sub{
      margin:6px 0 0 0;
      color:var(--muted);
      font-size:.95rem;
      max-width:520px;
    }

    .back-link{
      display:inline-flex;
      align-items:center;
      gap:6px;
      padding:7px 11px;
      border-radius:999px;
      border:1px solid rgba(148,163,184,0.6);
      background:#ffffff;
      font-size:.85rem;
      font-weight:600;
      color:#374151;
      text-decoration:none;
      white-space:nowrap;
      box-shadow:0 10px 24px rgba(15,23,42,0.12);
    }

    .back-link span.icon{
      font-size:.95rem;
    }

    /* Shell for participants list */
    .participants-shell{
      background:linear-gradient(135deg,#ffffff,#f9fbff);
      border-radius:18px;
      border:1px solid var(--card-border);
      padding:18px 18px 20px;
      box-shadow:0 14px 30px rgba(15,23,42,0.05);
    }

    .participants-header{
      margin:0 0 8px 0;
      font-family:Poppins, system-ui, sans-serif;
      font-size:1.1rem;
      display:flex;
      align-items:center;
      gap:8px;
    }

    .participants-header-pill{
      padding:3px 9px;
      border-radius:999px;
      font-size:.75rem;
      text-transform:uppercase;
      letter-spacing:.06em;
      background:linear-gradient(135deg, rgba(42,153,219,0.12), rgba(240,106,169,0.12));
      color:#374151;
    }

    .participants-sub{
      margin:0 0 14px 0;
      color:var(--muted);
      font-size:.92rem;
    }

    .note{
      background:#f9fafb;
      border-radius:12px;
      border:1px solid #e5e7eb;
      padding:10px 12px;
      color:var(--muted);
      font-size:.9rem;
    }

    /* Grid for lots of participants; stays tidy as list grows */
    .participant-grid{
      display:grid;
      grid-template-columns:repeat(auto-fit, minmax(260px, 1fr));
      gap:14px;
      margin-top:10px;
    }

    .participant-card{
      border-radius:14px;
      border:1px solid #e5e7eb;
      background:linear-gradient(135deg,#ffffff,rgba(42,153,219,0.03));
      padding:12px 13px 13px;
      box-shadow:0 10px 22px rgba(15,23,42,0.05);
      display:flex;
      flex-direction:column;
      gap:4px;
      position:relative;
      overflow:hidden;
    }

    .participant-card::before{
      content:"";
      position:absolute;
      inset-inline-start:0;
      top:0;
      bottom:0;
      width:4px;
      background:linear-gradient(180deg,var(--fia-pink),var(--fia-teal));
      opacity:0.75;
    }

    .participant-name{
      font-family:Poppins, system-ui, sans-serif;
      font-size:1rem;
      font-weight:600;
      color:#111827;
      padding-inline-start:2px;
    }

    .participant-email{
      font-size:.9rem;
      color:var(--muted);
      padding-inline-start:2px;
      word-break:break-all;
    }

    .participant-meta{
      font-size:.85rem;
      color:var(--muted);
      padding-inline-start:2px;
      margin-top:2px;
    }

    @media (max-width:640px){
      .page-header{
        flex-direction:column-reverse;
        align-items:flex-start;
      }
    }
  </style>
</head>
<body>
  <form id="form1" runat="server">
    <div class="wrap">

      <!-- Header -->
      <div class="page-header">
        <div class="page-header-main">
          <div class="page-eyebrow">
            <span class="page-eyebrow-pill">FIA</span>
            <span>Helper workspace</span>
          </div>
          <h1 class="page-title">1:1 Help Sessions</h1>
          <p class="page-sub">
            These are the participants who are assigned to you for one-on-one support.
            Use this list when you schedule 1:1 time, walk through security tasks together,
            or log quick support moments that count toward your certification.
          </p>
        </div>
        <a href="<%: ResolveUrl("~/Account/Helper/Home.aspx") %>" class="back-link">
          <span class="icon">←</span>
          <span>Back to Helper Home</span>
        </a>
      </div>

      <!-- Participants shell -->
      <div class="participants-shell">
        <div class="participants-header">
          Assigned participants
          <span class="participants-header-pill">1:1 list</span>
        </div>
        <p class="participants-sub">
          Each card shows a participant’s first name and email address so you can
          quickly reach out, schedule time, or look them up in another system.
        </p>

        <!-- Empty state -->
        <asp:PlaceHolder ID="NoParticipantsPH" runat="server" Visible="false">
          <div class="note">
            You don’t have any assigned participants yet. Once a University Admin connects
            participants to you as their Helper, they will appear in this list.
          </div>
        </asp:PlaceHolder>

        <!-- Assigned participants grid -->
        <asp:Repeater ID="ParticipantsRepeater" runat="server">
          <HeaderTemplate>
            <div class="participant-grid">
          </HeaderTemplate>

          <ItemTemplate>
            <div class="participant-card">
              <div class="participant-name">
                <%# Eval("FirstName") %>
              </div>
              <div class="participant-email">
                <%# Eval("Email") %>
              </div>
              <asp:PlaceHolder ID="UniversityPH" runat="server"
                               Visible='<%# !string.IsNullOrWhiteSpace(Convert.ToString(Eval("University"))) %>'>
                <div class="participant-meta">
                  <%# Eval("University") %>
                </div>
              </asp:PlaceHolder>
            </div>
          </ItemTemplate>

          <FooterTemplate>
            </div>
          </FooterTemplate>
        </asp:Repeater>
      </div>

    </div>
  </form>
</body>
</html>

