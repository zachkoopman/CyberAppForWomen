<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Schedule.aspx.cs" Inherits="CyberApp_FIA.Helper.Schedule" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
  <title>FIA • Your Schedule</title>
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
      --chip-soft-blue:rgba(42,153,219,0.12);
      --chip-soft-pink:rgba(240,106,169,0.12);
      --chip-soft-teal:rgba(69,195,179,0.12);
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

    /* Top hero header */
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

    /* Schedule cards */
    .schedule-shell{
      background:linear-gradient(135deg,#ffffff,#f9fbff);
      border-radius:18px;
      border:1px solid var(--card-border);
      padding:18px 18px 20px;
      box-shadow:0 14px 30px rgba(15,23,42,0.05);
    }

    .schedule-header{
      margin:0 0 8px 0;
      font-family:Poppins, system-ui, sans-serif;
      font-size:1.1rem;
      display:flex;
      align-items:center;
      gap:8px;
    }

    .schedule-header-pill{
      padding:3px 9px;
      border-radius:999px;
      font-size:.75rem;
      text-transform:uppercase;
      letter-spacing:.06em;
      background:linear-gradient(135deg, rgba(42,153,219,0.12), rgba(240,106,169,0.12));
      color:#374151;
    }

    .schedule-sub{
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

    .session-grid{
      display:grid;
      grid-template-columns:repeat(auto-fit, minmax(260px, 1fr));
      gap:14px;
      margin-top:10px;
    }

    .session-card{
      border-radius:14px;
      border:1px solid #e5e7eb;
      background:linear-gradient(135deg,#ffffff,rgba(42,153,219,0.03));
      padding:12px 13px 13px;
      box-shadow:0 10px 22px rgba(15,23,42,0.05);
      display:flex;
      flex-direction:column;
      gap:6px;
      position:relative;
      overflow:hidden;
    }

    .session-card::before{
      content:"";
      position:absolute;
      inset-inline-start:0;
      top:0;
      bottom:0;
      width:4px;
      background:linear-gradient(180deg,var(--fia-pink),var(--fia-teal));
      opacity:0.75;
    }

    .session-title{
      font-family:Poppins, system-ui, sans-serif;
      font-size:1rem;
      font-weight:600;
      color:#111827;
      padding-inline-start:2px;
    }

    .session-meta{
      font-size:.9rem;
      color:var(--muted);
      margin-top:2px;
      display:flex;
      flex-direction:column;
      gap:4px;
      padding-inline-start:2px;
    }

    .session-row{
      display:flex;
      justify-content:space-between;
      gap:6px;
      align-items:flex-start;
    }

    .session-label{
      font-weight:600;
      color:#4b5563;
    }

    .session-value{
      text-align:right;
      color:#111827;
    }

    /* Participants list */
    .session-participants{
      margin-top:6px;
      padding-inline-start:2px;
      font-size:.85rem;
      color:var(--muted);
    }

    .session-participant-row{
      display:flex;
      justify-content:space-between;
      gap:6px;
      margin-top:2px;
    }

    .participant-name{
      color:#111827;
    }

    .participant-status{
      font-weight:600;
    }

    .session-actions{
      margin-top:8px;
      padding-inline-start:2px;
      display:flex;
      flex-wrap:wrap;
      gap:8px;
    }

    .session-admit-btn{
      appearance:none;
      border:none;
      cursor:pointer;
      padding:8px 11px;
      border-radius:999px;
      font-family:Poppins, system-ui, sans-serif;
      font-size:.85rem;
      font-weight:600;
      background:linear-gradient(135deg,var(--fia-pink),var(--fia-blue));
      color:#fff;
      box-shadow:0 8px 18px rgba(15,23,42,0.15);
      white-space:nowrap;
    }

    .session-admit-btn:focus{
      outline:none;
      box-shadow:0 0 0 3px rgba(42,153,219,0.35);
    }

    .session-checkin-btn{
      background:linear-gradient(135deg,var(--fia-teal),#6be0cf);
    }

    .session-undo-btn{
      background:#ffffff;
      color:var(--fia-blue);
      border:1px solid #d9e9f6;
      box-shadow:0 6px 14px rgba(15,23,42,0.08);
    }

    .session-checkin-meta{
      margin-top:6px;
      font-size:.85rem;
      color:var(--muted);
      padding-inline-start:4px;
    }

    /* FIA toast for brand-friendly popups */
    .fia-toast{
      position:fixed;
      bottom:24px;
      right:24px;
      max-width:320px;
      padding:12px 16px;
      border-radius:999px;
      background:linear-gradient(135deg,var(--fia-pink),var(--fia-blue));
      color:#fff;
      font-family:Poppins, system-ui, sans-serif;
      font-size:.9rem;
      box-shadow:0 16px 40px rgba(15,23,42,0.28);
      z-index:9999;
    }

    @media (max-width:640px){
      .page-header{
        flex-direction:column-reverse;
        align-items:flex-start;
      }
      .session-row{
        flex-direction:column;
        align-items:flex-start;
      }
      .session-value{
        text-align:left;
      }
    }
  </style>

  <!-- Define showFiaToast early so startup scripts can call it -->
  <script type="text/javascript">
      function showFiaToast(message) {
          var box = document.getElementById('fiaToast');
          var txt = document.getElementById('fiaToastText');
          if (!box || !txt) return;

          txt.textContent = message || '';
          box.style.display = 'block';

          // auto-hide after ~3 seconds
          setTimeout(function () {
              box.style.display = 'none';
          }, 3000);
      }
  </script>
</head>
<body>
  <form id="form1" runat="server">
    <div class="wrap">

      <div class="page-header">
        <div class="page-header-main">
          <div class="page-eyebrow">
            <span class="page-eyebrow-pill">FIA</span>
            <span>Helper workspace</span>
          </div>
          <h1 class="page-title">Your schedule</h1>
          <p class="page-sub">
            Each card shows the microcourse name, the session’s date and time in your local timezone,
            and the capacity so you can quickly see what you’re running and when.
          </p>
        </div>
        <a href="<%: ResolveUrl("~/Account/Helper/Home.aspx") %>" class="back-link">
          <span class="icon">←</span>
          <span>Back to Helper Home</span>
        </a>
      </div>

      <div class="schedule-shell">
        <div class="schedule-header">
          Upcoming sessions
          <span class="schedule-header-pill">Helper view</span>
        </div>
        <p class="schedule-sub">
          Each upcoming-session card highlights the microcourse title, your local day and time
          (converted from stored UTC), and total capacity so you’re ready to run your session events.
        </p>

        <asp:PlaceHolder ID="NoSessionsPH" runat="server" Visible="false">
          <div class="note">
            You don’t have any assigned sessions yet. Once a University Admin assigns you to a
            microcourse session, it will show up here automatically.
          </div>
        </asp:PlaceHolder>

        <asp:Repeater ID="SessionsRepeater" runat="server" OnItemCommand="SessionsRepeater_ItemCommand">
          <HeaderTemplate>
            <div class="session-grid">
          </HeaderTemplate>

          <ItemTemplate>
            <div class="session-card">
              <div class="session-title">
                <%# Eval("CourseTitle") %>
              </div>
              <div class="session-meta">
                <div class="session-row">
                  <span class="session-label">Day &amp; time</span>
                  <span class="session-value"><%# Eval("DayTime") %></span>
                </div>
                <div class="session-row">
                  <span class="session-label">Capacity</span>
                  <span class="session-value"><%# Eval("Capacity") %></span>
                </div>
                <div class="session-row">
                  <span class="session-label">Session Room</span>
                  <asp:PlaceHolder runat="server" Visible='<%# !string.IsNullOrWhiteSpace(Convert.ToString(Eval("Room"))) %>'>
                    <span class="session-value">
                      <a href='<%# Eval("Room") %>' target="_blank" rel="noopener">Open Room</a>
                    </span>
                  </asp:PlaceHolder>
                  <asp:PlaceHolder runat="server" Visible='<%# string.IsNullOrWhiteSpace(Convert.ToString(Eval("Room"))) %>'>
                    <span class="session-value">Not set</span>
                  </asp:PlaceHolder>
                </div>
              </div>

              <!-- Participants list -->
              <asp:PlaceHolder ID="ParticipantsBlock" runat="server" Visible='<%# (bool)Eval("HasParticipants") %>'>
                <div class="session-participants">
                  <span class="session-label">Participants</span>
                  <asp:Repeater ID="ParticipantsRepeater" runat="server" DataSource='<%# Eval("Participants") %>'>
                    <ItemTemplate>
                      <div class="session-participant-row">
                        <span class="participant-name"><%# Eval("Name") %></span>
                        <span class="participant-status">
                          <%# (bool)Eval("Invited") ? "Invited" : "Needs Invite" %>
                        </span>
                      </div>
                    </ItemTemplate>
                  </asp:Repeater>
                </div>
              </asp:PlaceHolder>

              <div class="session-actions">
                <asp:Button ID="AdmitBtn"
                            runat="server"
                            CssClass="session-admit-btn"
                            Text="Admit Participants for Session"
                            CommandName="admit"
                            CommandArgument='<%# Eval("EventId") + "|" + Eval("SessionId") %>' />

                <asp:Button ID="CheckinBtn"
                            runat="server"
                            CssClass="session-admit-btn session-checkin-btn"
                            Text="Mark Myself Checked In"
                            CommandName="checkinHelper"
                            CommandArgument='<%# Eval("EventId") + "|" + Eval("SessionId") %>'
                            Visible='<%# !(bool)Eval("HasCheckin") %>' />

                <asp:Button ID="UndoCheckinBtn"
                            runat="server"
                            CssClass="session-admit-btn session-undo-btn"
                            Text="Undo Check-in"
                            CommandName="undoCheckinHelper"
                            CommandArgument='<%# Eval("EventId") + "|" + Eval("SessionId") %>'
                            Visible='<%# (bool)Eval("CanUndoCheckin") %>' />
              </div>

              <asp:PlaceHolder ID="CheckinMetaPH" runat="server" Visible='<%# (bool)Eval("HasCheckin") %>'>
                <div class="session-checkin-meta">
                  Checked in at <%# Eval("CheckedInAtLabel") %>.
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

    <!-- FIA toast for brand-friendly notifications -->
    <div id="fiaToast" class="fia-toast" style="display:none;">
      <span id="fiaToastText"></span>
    </div>
  </form>
</body>
</html>
