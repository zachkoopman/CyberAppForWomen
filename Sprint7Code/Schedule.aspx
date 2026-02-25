<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Schedule.aspx.cs" Inherits="CyberApp_FIA.Helper.Schedule" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
  <title>FIA • Your Schedule</title>
  <meta name="viewport" content="width=device-width, initial-scale=1" />
  <link href="https://fonts.googleapis.com/css2?family=Lato:wght@300;400&family=Poppins:wght@500;600;700&display=swap" rel="stylesheet" />

  <style>
    /* ============================================================
       FIA DESIGN TOKENS — shared across every Helper workspace page
    ============================================================ */
    :root {
      --fia-pink:       #f06aa9;
      --fia-pink-soft:  rgba(240, 106, 169, 0.12);
      --fia-blue:       #2a99db;
      --fia-blue-soft:  rgba(42, 153, 219, 0.12);
      --fia-teal:       #45c3b3;
      --fia-teal-soft:  rgba(69, 195, 179, 0.12);

      --ink:            #1c2233;
      --muted:          #6b7280;
      --subtle:         #94a3b8;

      --bg:             #f2f4fb;
      --surface:        #ffffff;
      --surface-raised: #fafbff;
      --border:         #e4e9f2;
      --border-soft:    rgba(226, 232, 240, 0.7);

      --shadow-sm:      0 2px 8px rgba(15, 23, 42, 0.06);
      --shadow-md:      0 8px 24px rgba(15, 23, 42, 0.08);
      --shadow-lg:      0 18px 48px rgba(15, 23, 42, 0.10);

      --radius-sm:      10px;
      --radius-md:      16px;
      --radius-lg:      22px;
      --radius-pill:    999px;
    }

    /* ============================================================
       RESET & BASE
    ============================================================ */
    *, *::before, *::after { box-sizing: border-box; }

    body {
      margin: 0;
      font-family: Lato, system-ui, -apple-system, sans-serif;
      font-size: 16px;
      line-height: 1.6;
      color: var(--ink);
      background:
        radial-gradient(ellipse at 0% 0%,   rgba(240, 106, 169, 0.08) 0%, transparent 50%),
        radial-gradient(ellipse at 100% 100%, rgba(42, 153, 219, 0.07) 0%, transparent 50%),
        var(--bg);
      min-height: 100vh;
    }

    /* ============================================================
       LAYOUT WRAPPER
    ============================================================ */
    .wrap {
      max-width: 1100px;
      margin: 0 auto;
      padding: 28px 20px 56px;
    }

    /* ============================================================
       PAGE HEADER
       Reused across Schedule, 1:1 Help, Conversations.
       Contains page eyebrow, title, subtitle, and back button.
    ============================================================ */
    .page-header {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      gap: 20px;
      padding: 24px 26px;
      margin-bottom: 24px;
      border-radius: var(--radius-lg);
      border: 1px solid var(--border-soft);
      box-shadow: var(--shadow-lg);
      background:
        radial-gradient(circle at 0% 0%,   rgba(240, 106, 169, 0.14) 0%, transparent 55%),
        radial-gradient(circle at 100% 0%,  rgba(69,  195, 179, 0.18) 0%, transparent 55%),
        linear-gradient(150deg, #fefcff, #f4f7ff);
    }

    .page-header-main { max-width: 620px; }

    /* Small "FIA · Helper workspace" breadcrumb above the title */
    .eyebrow {
      display: flex;
      align-items: center;
      gap: 8px;
      margin-bottom: 4px;
      font-size: 0.75rem;
      font-weight: 700;
      text-transform: uppercase;
      letter-spacing: 0.14em;
      color: var(--muted);
    }

    .eyebrow-dot {
      display: inline-grid;
      place-items: center;
      width: 20px;
      height: 20px;
      border-radius: var(--radius-pill);
      background: linear-gradient(135deg, var(--fia-pink), var(--fia-blue));
      color: #fff;
      font-family: Poppins, sans-serif;
      font-size: 0.62rem;
      font-weight: 700;
    }

    .page-title {
      margin: 0 0 6px;
      font-family: Poppins, sans-serif;
      font-size: 1.7rem;
      font-weight: 700;
      background: linear-gradient(135deg, var(--fia-blue), var(--fia-pink));
      -webkit-background-clip: text;
      -webkit-text-fill-color: transparent;
    }

    .page-sub {
      margin: 0;
      font-size: 0.93rem;
      color: var(--muted);
      max-width: 520px;
    }

    /* Back navigation button */
    .btn-back {
      display: inline-flex;
      align-items: center;
      gap: 6px;
      padding: 7px 14px;
      border-radius: var(--radius-pill);
      border: 1px solid var(--border);
      background: var(--surface);
      font-size: 0.84rem;
      font-weight: 600;
      color: #374151;
      text-decoration: none;
      white-space: nowrap;
      box-shadow: var(--shadow-md);
      flex-shrink: 0;
      transition: box-shadow 0.15s, transform 0.15s;
    }

    .btn-back:hover {
      box-shadow: var(--shadow-lg);
      transform: translateY(-1px);
    }

    /* ============================================================
       LOG PANEL — "Mark a session as delivered"
       The form that allows helpers to log delivered sessions and
       view recent history with quick undo.
    ============================================================ */
    .log-panel {
      margin-bottom: 22px;
      padding: 20px 22px 22px;
      border-radius: var(--radius-lg);
      border: 1px solid var(--border);
      box-shadow: var(--shadow-md);
      background:
        radial-gradient(circle at 0% 0%,    rgba(240, 106, 169, 0.10) 0%, transparent 55%),
        radial-gradient(circle at 100% 100%, rgba(42, 153, 219, 0.12) 0%, transparent 55%),
        linear-gradient(150deg, var(--surface), #f5f8ff);
    }

    /* Sub-label above the panel heading */
    .log-eyebrow {
      font-size: 0.72rem;
      font-weight: 700;
      text-transform: uppercase;
      letter-spacing: 0.12em;
      color: var(--fia-pink);
      margin-bottom: 2px;
    }

    .log-title {
      margin: 0 0 4px;
      font-family: Poppins, sans-serif;
      font-size: 1.1rem;
      font-weight: 600;
      color: var(--ink);
    }

    .log-sub {
      margin: 0 0 16px;
      font-size: 0.88rem;
      color: var(--muted);
      max-width: 640px;
    }

    /* Three-column form: course dropdown · notes · submit */
    .log-form {
      display: grid;
      grid-template-columns: minmax(0, 2fr) minmax(0, 3fr) auto;
      gap: 12px;
      align-items: flex-end;
    }

    .log-field {
      display: flex;
      flex-direction: column;
      gap: 4px;
    }

    .field-label {
      font-size: 0.78rem;
      font-weight: 700;
      color: var(--muted);
      text-transform: uppercase;
      letter-spacing: 0.06em;
    }

    /* Shared input / select styles */
    .field-select,
    .field-textarea {
      width: 100%;
      border: 1px solid var(--border);
      background: var(--surface);
      font-family: inherit;
      font-size: 0.9rem;
      color: var(--ink);
      outline: none;
      transition: border-color 0.15s, box-shadow 0.15s;
    }

    .field-select   { border-radius: var(--radius-pill); padding: 8px 14px; }
    .field-textarea { border-radius: var(--radius-sm); padding: 8px 12px; resize: vertical; min-height: 42px; max-height: 120px; }

    .field-select:focus,
    .field-textarea:focus {
      border-color: var(--fia-pink);
      box-shadow: 0 0 0 3px rgba(240, 106, 169, 0.18);
    }

    /* Primary submit button */
    .btn-submit {
      appearance: none;
      border: none;
      cursor: pointer;
      padding: 10px 18px;
      border-radius: var(--radius-pill);
      font-family: Poppins, sans-serif;
      font-size: 0.86rem;
      font-weight: 600;
      background: linear-gradient(135deg, var(--fia-pink), var(--fia-blue));
      color: #fff;
      box-shadow: 0 6px 18px rgba(240, 106, 169, 0.30);
      white-space: nowrap;
      transition: opacity 0.15s, box-shadow 0.15s;
    }

    .btn-submit:hover  { opacity: 0.92; box-shadow: 0 10px 26px rgba(240, 106, 169, 0.36); }
    .btn-submit:focus  { outline: none; box-shadow: 0 0 0 3px rgba(42, 153, 219, 0.35); }

    /* ---- Recent history section inside the log panel ---- */
    .history-block {
      margin-top: 18px;
      padding-top: 14px;
      border-top: 1px dashed var(--border);
    }

    .history-label {
      font-size: 0.72rem;
      font-weight: 700;
      text-transform: uppercase;
      letter-spacing: 0.12em;
      color: var(--muted);
      margin-bottom: 8px;
    }

    .history-empty {
      font-size: 0.86rem;
      color: var(--subtle);
    }

    .history-list {
      display: flex;
      flex-direction: column;
      gap: 8px;
    }

    .history-item {
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: 12px;
      padding: 9px 12px;
      border-radius: var(--radius-md);
      background: linear-gradient(135deg, var(--fia-blue-soft), var(--fia-pink-soft));
      border: 1px solid var(--border);
    }

    .history-item-text { display: flex; flex-direction: column; gap: 2px; min-width: 0; }

    .history-item-title {
      font-size: 0.88rem;
      font-weight: 600;
      color: var(--ink);
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
    }

    .history-item-meta { font-size: 0.78rem; color: var(--muted); }

    /* Undo button in history rows */
    .btn-undo {
      appearance: none;
      border: 1px solid rgba(42, 153, 219, 0.45);
      cursor: pointer;
      padding: 5px 12px;
      border-radius: var(--radius-pill);
      font-family: Poppins, sans-serif;
      font-size: 0.76rem;
      font-weight: 600;
      background: var(--surface);
      color: var(--fia-blue);
      box-shadow: var(--shadow-sm);
      white-space: nowrap;
      flex-shrink: 0;
      transition: box-shadow 0.15s;
    }

    .btn-undo:hover { box-shadow: var(--shadow-md); }
    .btn-undo:focus { outline: none; box-shadow: 0 0 0 2px rgba(42, 153, 219, 0.40); }

    /* ============================================================
       UPCOMING SESSIONS SECTION
       Card grid listing each session the helper is assigned to.
    ============================================================ */
    .sessions-shell {
      background: var(--surface);
      border-radius: var(--radius-lg);
      border: 1px solid var(--border);
      padding: 22px 22px 24px;
      box-shadow: var(--shadow-md);
    }

    /* Section heading row */
    .section-head {
      display: flex;
      align-items: center;
      gap: 10px;
      margin: 0 0 6px;
      font-family: Poppins, sans-serif;
      font-size: 1.1rem;
      font-weight: 600;
      color: var(--ink);
    }

    .section-pill {
      padding: 3px 10px;
      border-radius: var(--radius-pill);
      font-size: 0.72rem;
      font-weight: 700;
      text-transform: uppercase;
      letter-spacing: 0.07em;
      background: linear-gradient(135deg, var(--fia-blue-soft), var(--fia-pink-soft));
      color: #374151;
      border: 1px solid var(--border);
    }

    .section-sub {
      margin: 0 0 16px;
      font-size: 0.9rem;
      color: var(--muted);
    }

    /* Empty-state notice */
    .empty-note {
      padding: 14px 16px;
      border-radius: var(--radius-md);
      background: var(--surface-raised);
      border: 1px solid var(--border);
      font-size: 0.9rem;
      color: var(--muted);
    }

    /* Session card grid — responsive auto-fit columns */
    .session-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
      gap: 14px;
      margin-top: 4px;
    }

    /* Individual session card */
    .session-card {
      border-radius: var(--radius-md);
      border: 1px solid var(--border);
      background: linear-gradient(150deg, var(--surface), rgba(42, 153, 219, 0.03));
      padding: 14px 16px 16px;
      box-shadow: var(--shadow-sm);
      display: flex;
      flex-direction: column;
      gap: 6px;
      position: relative;
      overflow: hidden;
      transition: box-shadow 0.2s, transform 0.2s;
    }

    .session-card:hover {
      box-shadow: var(--shadow-md);
      transform: translateY(-1px);
    }

    /* Left pink-to-teal accent stripe */
    .session-card::before {
      content: "";
      position: absolute;
      inset-block: 0;
      inset-inline-start: 0;
      width: 4px;
      background: linear-gradient(180deg, var(--fia-pink), var(--fia-teal));
    }

    .session-title {
      font-family: Poppins, sans-serif;
      font-size: 0.98rem;
      font-weight: 600;
      color: var(--ink);
      padding-inline-start: 4px;
    }

    /* Key-value metadata rows (Day & Time, Capacity, Room) */
    .session-meta {
      display: flex;
      flex-direction: column;
      gap: 4px;
      padding-inline-start: 4px;
    }

    .meta-row {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      gap: 8px;
      font-size: 0.86rem;
    }

    .meta-label { font-weight: 700; color: var(--muted); }
    .meta-value { color: var(--ink); text-align: right; }
    .meta-value a { color: var(--fia-blue); font-weight: 600; text-decoration: none; }
    .meta-value a:hover { text-decoration: underline; }

    /* Participant list inside session card */
    .session-participants {
      margin-top: 6px;
      padding-inline-start: 4px;
      font-size: 0.84rem;
    }

    .participant-row {
      display: flex;
      justify-content: space-between;
      gap: 6px;
      margin-top: 3px;
      color: var(--muted);
    }

    .participant-row .p-name  { color: var(--ink); font-weight: 500; }
    .participant-row .p-status { font-weight: 700; }

    /* Action buttons row at the bottom of each session card */
    .session-actions {
      margin-top: 10px;
      padding-inline-start: 4px;
      display: flex;
      flex-wrap: wrap;
      gap: 8px;
    }

    /* Base session action button */
    .btn-action {
      appearance: none;
      border: none;
      cursor: pointer;
      padding: 8px 13px;
      border-radius: var(--radius-pill);
      font-family: Poppins, sans-serif;
      font-size: 0.82rem;
      font-weight: 600;
      white-space: nowrap;
      transition: opacity 0.15s, box-shadow 0.15s;
    }

    .btn-action:focus { outline: none; box-shadow: 0 0 0 3px rgba(42, 153, 219, 0.35); }
    .btn-action:hover { opacity: 0.88; }

    /* Admit (primary pink→blue) */
    .btn-action--admit {
      background: linear-gradient(135deg, var(--fia-pink), var(--fia-blue));
      color: #fff;
      box-shadow: 0 6px 16px rgba(240, 106, 169, 0.25);
    }

    /* Check-in (teal) */
    .btn-action--checkin {
      background: linear-gradient(135deg, var(--fia-teal), #5dd6c6);
      color: #fff;
      box-shadow: 0 6px 16px rgba(69, 195, 179, 0.25);
    }

    /* Undo check-in (outline style) */
    .btn-action--undo {
      background: var(--surface);
      color: var(--fia-blue);
      border: 1px solid rgba(42, 153, 219, 0.40);
      box-shadow: var(--shadow-sm);
    }

    /* Check-in confirmation metadata line */
    .checkin-meta {
      margin-top: 4px;
      padding-inline-start: 4px;
      font-size: 0.82rem;
      color: var(--muted);
    }

    /* ============================================================
       TOAST NOTIFICATION
       Lightweight brand-gradient notification strip shown after
       actions (delivered, undo, etc.). Controlled via JS.
    ============================================================ */
    .fia-toast {
      position: fixed;
      bottom: 24px;
      right: 24px;
      max-width: 340px;
      padding: 12px 18px;
      border-radius: var(--radius-pill);
      background: linear-gradient(135deg, var(--fia-pink), var(--fia-blue));
      color: #fff;
      font-family: Poppins, sans-serif;
      font-size: 0.88rem;
      font-weight: 500;
      box-shadow: 0 16px 40px rgba(15, 23, 42, 0.25);
      z-index: 9999;
    }

    .toast-undo {
      margin-left: 10px;
      padding: 3px 10px;
      border-radius: var(--radius-pill);
      border: 1px solid rgba(255, 255, 255, 0.80);
      background: rgba(255, 255, 255, 0.08);
      color: #fff;
      font-size: 0.76rem;
      font-weight: 700;
      cursor: pointer;
    }

    .toast-undo:focus { outline: none; box-shadow: 0 0 0 2px rgba(255, 255, 255, 0.55); }

    /* ============================================================
       RESPONSIVE
    ============================================================ */
    @media (max-width: 680px) {
      .wrap         { padding: 18px 14px 40px; }
      .page-header  { flex-direction: column-reverse; align-items: flex-start; border-radius: var(--radius-md); }
      .log-form     { grid-template-columns: 1fr; align-items: stretch; }
      .history-item { align-items: flex-start; }
      .meta-row     { flex-direction: column; align-items: flex-start; }
      .meta-value   { text-align: left; }
    }
  </style>

  <%-- Toast helper: shows a branded notification for 3 seconds --%>
  <script type="text/javascript">
    function showFiaToast(message) {
      var box = document.getElementById('fiaToast');
      var txt = document.getElementById('fiaToastText');
      if (!box || !txt) return;
      txt.textContent = message || '';
      box.style.display = 'block';
      setTimeout(function () { box.style.display = 'none'; }, 3000);
    }
  </script>
</head>

<body>
  <form id="form1" runat="server">
    <div class="wrap">

      <!-- ======================================================
           PAGE HEADER
           Eyebrow, page title, subtitle, and back-to-home link.
      ====================================================== -->
      <div class="page-header">
        <div class="page-header-main">
          <div class="eyebrow">
            <span class="eyebrow-dot">FIA</span>
            <span>Helper workspace</span>
          </div>
          <h1 class="page-title">Your schedule</h1>
          <p class="page-sub">
            Each card shows the microcourse name, your local date and time,
            and session capacity so you know exactly what you're running and when.
          </p>
        </div>
        <a href="<%: ResolveUrl("~/Account/Helper/Home.aspx") %>" class="btn-back">
          ← Back to Helper Home
        </a>
      </div>


      <!-- ======================================================
           LOG DELIVERED SESSION PANEL
           Helpers log that they taught a session here so that it
           counts toward their certification progress. Recent history
           with undo is shown below the form.
      ====================================================== -->
      <div class="log-panel">

        <div class="log-eyebrow">Certification progress</div>
        <h2 class="log-title">Mark a session as delivered</h2>
        <p class="log-sub">
          Quickly log a microcourse session you've taught so it counts toward certification.
          You can add a short note that only admins will see.
        </p>

        <%-- Form: course selector · optional notes · submit button --%>
        <div class="log-form">
          <div class="log-field">
            <span class="field-label">Course</span>
            <asp:DropDownList ID="DeliverCourseDropDown" runat="server" CssClass="field-select" />
          </div>

          <div class="log-field">
            <span class="field-label">Notes (optional)</span>
            <asp:TextBox ID="DeliverNotesTextBox"
                         runat="server"
                         CssClass="field-textarea"
                         TextMode="MultiLine"
                         Rows="2"
                         placeholder="Any quick context you want University Admins to see." />
          </div>

          <div class="log-field">
            <asp:Button ID="DeliverSubmitButton"
                        runat="server"
                        CssClass="btn-submit"
                        Text="Log delivered session"
                        OnClick="DeliverSubmitButton_Click" />
          </div>
        </div>

        <%-- Recent delivered sessions with per-row undo --%>
        <div class="history-block">
          <div class="history-label">Recent delivered sessions</div>

          <asp:PlaceHolder ID="DeliverHistoryEmpty" runat="server">
            <div class="history-empty">
              Once you log delivered sessions, the last three will appear here with a quick undo.
            </div>
          </asp:PlaceHolder>

          <asp:Repeater ID="DeliverHistoryRepeater"
                        runat="server"
                        OnItemCommand="DeliverHistoryRepeater_ItemCommand">
            <HeaderTemplate><div class="history-list"></HeaderTemplate>

            <ItemTemplate>
              <div class="history-item">
                <div class="history-item-text">
                  <div class="history-item-title"><%# Eval("CourseTitle") %></div>
                  <div class="history-item-meta"><%# Eval("WhenLabel") %></div>
                </div>
                <%-- Undo passes a serialised snapshot so the server can reverse the log --%>
                <asp:Button ID="UndoDeliverItemButton"
                            runat="server"
                            CssClass="btn-undo"
                            Text="Undo"
                            CommandName="undoDeliver"
                            CommandArgument='<%# Eval("Snapshot") %>' />
              </div>
            </ItemTemplate>

            <FooterTemplate></div></FooterTemplate>
          </asp:Repeater>
        </div>

      </div>
      <%-- /log-panel --%>


      <!-- ======================================================
           UPCOMING SESSIONS GRID
           Auto-fit card grid; each card shows course, date/time,
           capacity, room link, participant list, and action buttons.
      ====================================================== -->
      <div class="sessions-shell">

        <div class="section-head">
          Upcoming sessions
          <span class="section-pill">Helper view</span>
        </div>
        <p class="section-sub">
          Upcoming sessions are converted to your local timezone.
          Use the action buttons to admit participants, check yourself in, and record completions.
        </p>

        <%-- Empty state placeholder — made visible from code-behind when no sessions exist --%>
        <asp:PlaceHolder ID="NoSessionsPH" runat="server" Visible="false">
          <div class="empty-note">
            You don't have any assigned sessions yet. Once a University Admin assigns you to a
            microcourse session it will appear here automatically.
          </div>
        </asp:PlaceHolder>

        <asp:Repeater ID="SessionsRepeater" runat="server" OnItemCommand="SessionsRepeater_ItemCommand">
          <HeaderTemplate><div class="session-grid"></HeaderTemplate>

          <ItemTemplate>
            <div class="session-card">

              <%-- Course name heading --%>
              <div class="session-title"><%# Eval("CourseTitle") %></div>

              <%-- Day/time, capacity, and room metadata --%>
              <div class="session-meta">

                <div class="meta-row">
                  <span class="meta-label">Day &amp; time</span>
                  <span class="meta-value"><%# Eval("DayTime") %></span>
                </div>

                <div class="meta-row">
                  <span class="meta-label">Capacity</span>
                  <span class="meta-value"><%# Eval("Capacity") %></span>
                </div>

                <div class="meta-row">
                  <span class="meta-label">Session room</span>
                  <%-- Show link if room URL exists; otherwise show "Not set" --%>
                  <asp:PlaceHolder runat="server"
                    Visible='<%# !string.IsNullOrWhiteSpace(Convert.ToString(Eval("Room"))) %>'>
                    <span class="meta-value">
                      <a href='<%# Eval("Room") %>' target="_blank" rel="noopener">Open room ↗</a>
                    </span>
                  </asp:PlaceHolder>
                  <asp:PlaceHolder runat="server"
                    Visible='<%# string.IsNullOrWhiteSpace(Convert.ToString(Eval("Room"))) %>'>
                    <span class="meta-value">Not set</span>
                  </asp:PlaceHolder>
                </div>

              </div>

              <%-- Participant list — visible only if the session has participants --%>
              <asp:PlaceHolder ID="ParticipantsBlock" runat="server"
                               Visible='<%# (bool)Eval("HasParticipants") %>'>
                <div class="session-participants">
                  <span class="meta-label">Participants</span>
                  <asp:Repeater ID="ParticipantsRepeater" runat="server"
                                DataSource='<%# Eval("Participants") %>'>
                    <ItemTemplate>
                      <div class="participant-row">
                        <span class="p-name"><%# (Container.ItemIndex + 1) %>. <%# Eval("Name") %></span>
                        <span class="p-status"><%# (bool)Eval("Invited") ? "Invited" : "Needs Invite" %></span>
                      </div>
                    </ItemTemplate>
                  </asp:Repeater>
                </div>
              </asp:PlaceHolder>

              <%-- Primary session action buttons --%>
              <div class="session-actions">
                <asp:Button ID="AdmitBtn"
                            runat="server"
                            CssClass="btn-action btn-action--admit"
                            Text="Admit Participants"
                            CommandName="admit"
                            CommandArgument='<%# Eval("EventId") + "|" + Eval("SessionId") %>' />

                <%-- Check-in visible only when helper has not yet checked in --%>
                <asp:Button ID="CheckinBtn"
                            runat="server"
                            CssClass="btn-action btn-action--checkin"
                            Text="Mark Myself Checked In"
                            CommandName="checkinHelper"
                            CommandArgument='<%# Eval("EventId") + "|" + Eval("SessionId") %>'
                            Visible='<%# !(bool)Eval("HasCheckin") %>' />

                <%-- Undo check-in visible only when check-in can still be reversed --%>
                <asp:Button ID="UndoCheckinBtn"
                            runat="server"
                            CssClass="btn-action btn-action--undo"
                            Text="Undo Check-in"
                            CommandName="undoCheckinHelper"
                            CommandArgument='<%# Eval("EventId") + "|" + Eval("SessionId") %>'
                            Visible='<%# (bool)Eval("CanUndoCheckin") %>' />
              </div>

              <%-- Post-invite actions: appear after the first participant has been invited --%>
              <asp:PlaceHolder ID="PostInviteActionsPH" runat="server"
                               Visible='<%# (bool)Eval("HasActiveInvite") %>'>
                <div class="session-actions" style="margin-top: 6px;">
                  <asp:Button ID="CompleteParticipantBtn"
                              runat="server"
                              CssClass="btn-action btn-action--checkin"
                              Text="Complete with Participant"
                              CommandName="completeParticipant"
                              CommandArgument='<%# Eval("EventId") + "|" + Eval("SessionId") + "|" + Eval("ActiveParticipantId") %>' />

                  <asp:Button ID="MissingParticipantBtn"
                              runat="server"
                              CssClass="btn-action btn-action--admit"
                              Text="Mark Participant Missing"
                              CommandName="markMissingParticipant"
                              CommandArgument='<%# Eval("EventId") + "|" + Eval("SessionId") + "|" + Eval("ActiveParticipantId") %>' />
                </div>
              </asp:PlaceHolder>

              <%-- Check-in confirmation timestamp — shown after the helper checks in --%>
              <asp:PlaceHolder ID="CheckinMetaPH" runat="server"
                               Visible='<%# (bool)Eval("HasCheckin") %>'>
                <div class="checkin-meta">
                  ✓ Checked in at <%# Eval("CheckedInAtLabel") %>
                </div>
              </asp:PlaceHolder>

            </div>
          </ItemTemplate>

          <FooterTemplate></div></FooterTemplate>
        </asp:Repeater>
      </div>
      <%-- /sessions-shell --%>

    </div><%-- /wrap --%>

    <!-- FIA toast — shown after successful actions (JS-controlled) -->
    <div id="fiaToast" class="fia-toast" style="display: none;">
      <span id="fiaToastText"></span>
    </div>

  </form>
</body>
</html>
