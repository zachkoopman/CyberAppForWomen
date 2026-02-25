<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="OneOnOneHelp.aspx.cs" Inherits="CyberApp_FIA.Helper.OneOnOneHelp" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
  <title>FIA • 1:1 Help Sessions</title>
  <meta name="viewport" content="width=device-width, initial-scale=1" />
  <link href="https://fonts.googleapis.com/css2?family=Lato:wght@300;400&family=Poppins:wght@500;600;700&display=swap" rel="stylesheet" />

  <style>
    /* ============================================================
       FIA DESIGN TOKENS
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
       PAGE HEADER (shared pattern across workspace pages)
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
        radial-gradient(circle at 0% 0%,  rgba(240, 106, 169, 0.14) 0%, transparent 55%),
        radial-gradient(circle at 100% 0%, rgba(69, 195, 179, 0.18) 0%, transparent 55%),
        linear-gradient(150deg, #fefcff, #f4f7ff);
    }

    .page-header-main { max-width: 620px; }

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

    .btn-back:hover { box-shadow: var(--shadow-lg); transform: translateY(-1px); }

    /* ============================================================
       LOG 1:1 HELP PANEL
       Allows helpers to log a one-on-one help session for a
       specific microcourse category and track recent history.
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

    /* Three-column form: course · notes · submit */
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

    .btn-submit:hover { opacity: 0.92; box-shadow: 0 10px 26px rgba(240, 106, 169, 0.36); }
    .btn-submit:focus { outline: none; box-shadow: 0 0 0 3px rgba(42, 153, 219, 0.35); }

    /* Status / error message below the form */
    .log-status { margin-top: 8px; font-size: 0.85rem; color: #4b5563; }
    .log-status-error { color: #b91c1c; }

    /* ---- Recent help history inside log panel ---- */
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

    .history-empty { font-size: 0.86rem; color: var(--subtle); }

    .history-list { display: flex; flex-direction: column; gap: 8px; }

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
    .history-item-title { font-size: 0.88rem; font-weight: 600; color: var(--ink); overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
    .history-item-meta  { font-size: 0.78rem; color: var(--muted); }

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
       ASSIGNED PARTICIPANTS SECTION
       Card grid of all participants the helper is paired with.
    ============================================================ */
    .participants-shell {
      background: var(--surface);
      border-radius: var(--radius-lg);
      border: 1px solid var(--border);
      padding: 22px 22px 24px;
      box-shadow: var(--shadow-md);
    }

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

    .section-sub { margin: 0 0 16px; font-size: 0.9rem; color: var(--muted); }

    /* Empty-state notice */
    .empty-note {
      padding: 14px 16px;
      border-radius: var(--radius-md);
      background: var(--surface-raised);
      border: 1px solid var(--border);
      font-size: 0.9rem;
      color: var(--muted);
    }

    /* Auto-fit responsive card grid */
    .participant-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(260px, 1fr));
      gap: 14px;
    }

    /* Individual participant card */
    .participant-card {
      border-radius: var(--radius-md);
      border: 1px solid var(--border);
      background: linear-gradient(150deg, var(--surface), rgba(240, 106, 169, 0.03));
      padding: 14px 16px 16px;
      box-shadow: var(--shadow-sm);
      position: relative;
      overflow: hidden;
      display: flex;
      flex-direction: column;
      gap: 4px;
      transition: box-shadow 0.2s, transform 0.2s;
    }

    .participant-card:hover {
      box-shadow: var(--shadow-md);
      transform: translateY(-1px);
    }

    /* Left pink→teal accent bar */
    .participant-card::before {
      content: "";
      position: absolute;
      inset-block: 0;
      inset-inline-start: 0;
      width: 4px;
      background: linear-gradient(180deg, var(--fia-pink), var(--fia-teal));
    }

    /* Header row: name + optional message indicator */
    .p-header {
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: 8px;
      padding-inline-start: 4px;
    }

    .p-name {
      font-family: Poppins, sans-serif;
      font-size: 1rem;
      font-weight: 600;
      color: var(--ink);
    }

    /* "Messages" badge when the participant has sent new messages */
    .msg-badge {
      display: inline-flex;
      align-items: center;
      gap: 5px;
      padding: 3px 8px;
      border-radius: var(--radius-pill);
      font-size: 0.72rem;
      font-weight: 700;
      background: linear-gradient(135deg, var(--fia-pink-soft), var(--fia-blue-soft));
      color: #7a103a;
      border: 1px solid rgba(240, 106, 169, 0.40);
      white-space: nowrap;
    }

    .msg-dot {
      width: 7px;
      height: 7px;
      border-radius: var(--radius-pill);
      background: var(--fia-pink);
    }

    .p-email {
      font-size: 0.88rem;
      color: var(--muted);
      padding-inline-start: 4px;
      word-break: break-all;
    }

    .p-meta {
      font-size: 0.84rem;
      color: var(--subtle);
      padding-inline-start: 4px;
    }

    /* Actions row — link to participant's conversation list */
    .p-actions {
      margin-top: 10px;
      display: flex;
      gap: 8px;
      padding-inline-start: 4px;
    }

    .btn-convo {
      display: inline-flex;
      align-items: center;
      gap: 5px;
      padding: 7px 14px;
      border-radius: var(--radius-pill);
      border: 1px solid #dbeafe;
      background: linear-gradient(135deg, #eff6ff, var(--surface));
      font-size: 0.84rem;
      font-weight: 600;
      color: #1d4ed8;
      text-decoration: none;
      box-shadow: var(--shadow-sm);
      transition: box-shadow 0.15s, border-color 0.15s;
    }

    .btn-convo:hover {
      border-color: #bfdbfe;
      box-shadow: var(--shadow-md);
    }

    /* ============================================================
       RESPONSIVE
    ============================================================ */
    @media (max-width: 680px) {
      .wrap          { padding: 18px 14px 40px; }
      .page-header   { flex-direction: column-reverse; align-items: flex-start; border-radius: var(--radius-md); }
      .log-form      { grid-template-columns: 1fr; align-items: stretch; }
      .history-item  { align-items: flex-start; }
    }
  </style>
</head>

<body>
  <form id="form1" runat="server">
    <div class="wrap">

      <!-- ======================================================
           PAGE HEADER
      ====================================================== -->
      <div class="page-header">
        <div class="page-header-main">
          <div class="eyebrow">
            <span class="eyebrow-dot">FIA</span>
            <span>Helper workspace</span>
          </div>
          <h1 class="page-title">1:1 Help Sessions</h1>
          <p class="page-sub">
            These are the participants assigned to you for one-on-one support.
            Use this page to schedule time, walk through security tasks together,
            or log support moments that count toward your certification.
          </p>
        </div>
        <a href="<%: ResolveUrl("~/Account/Helper/Home.aspx") %>" class="btn-back">
          ← Back to Helper Home
        </a>
      </div>


      <!-- ======================================================
           LOG A 1:1 HELP SESSION PANEL
           Form to log a one-on-one help session for a given
           microcourse. Recent history with undo appears below.
      ====================================================== -->
      <div class="log-panel">

        <div class="log-eyebrow">Certification progress</div>
        <h2 class="log-title">Log a one-on-one help session</h2>
        <p class="log-sub">
          Track one-to-one help you gave for a microcourse so your support work
          counts toward certification. Add a brief note about what you covered.
        </p>

        <div class="log-form">
          <div class="log-field">
            <span class="field-label">Microcourse category</span>
            <asp:DropDownList ID="HelpCourseDropDown" runat="server" CssClass="field-select" />
          </div>

          <div class="log-field">
            <span class="field-label">What did you help with?</span>
            <asp:TextBox ID="HelpDetailsTextBox"
                         runat="server"
                         CssClass="field-textarea"
                         TextMode="MultiLine"
                         Rows="2"
                         placeholder="e.g. Helped Abby set stronger privacy settings on Instagram." />
          </div>

          <div class="log-field">
            <asp:Button ID="HelpSubmitButton"
                        runat="server"
                        CssClass="btn-submit"
                        Text="Log one-to-one help"
                        OnClick="HelpSubmitButton_Click" />
          </div>
        </div>

        <%-- Server-set status / error message after submit --%>
        <asp:Label ID="HelpStatusLabel" runat="server" CssClass="log-status" />

        <%-- Recent help history with undo --%>
        <div class="history-block">
          <div class="history-label">Recent one-to-one help</div>

          <asp:PlaceHolder ID="HelpHistoryEmpty" runat="server">
            <div class="history-empty">
              Once you log help sessions, the last three will appear here with a quick undo.
            </div>
          </asp:PlaceHolder>

          <asp:Repeater ID="HelpHistoryRepeater"
                        runat="server"
                        OnItemCommand="HelpHistoryRepeater_ItemCommand">
            <HeaderTemplate><div class="history-list"></HeaderTemplate>

            <ItemTemplate>
              <div class="history-item">
                <div class="history-item-text">
                  <div class="history-item-title"><%# Eval("CourseTitle") %></div>
                  <div class="history-item-meta"><%# Eval("WhenLabel") %></div>
                </div>
                <asp:Button ID="UndoHelpItemButton"
                            runat="server"
                            CssClass="btn-undo"
                            Text="Undo"
                            CommandName="undoHelp"
                            CommandArgument='<%# Eval("Snapshot") %>' />
              </div>
            </ItemTemplate>

            <FooterTemplate></div></FooterTemplate>
          </asp:Repeater>
        </div>

      </div>
      <%-- /log-panel --%>


      <!-- ======================================================
           ASSIGNED PARTICIPANTS GRID
           Auto-fit card grid showing every participant paired with
           this helper. Cards include name, email, university, and
           a link to the participant's conversation threads.
      ====================================================== -->
      <div class="participants-shell">

        <div class="section-head">
          Assigned participants
          <span class="section-pill">1:1 list</span>
        </div>
        <p class="section-sub">
          Each card shows the participant's name and contact details.
          A message badge appears when they have sent you a new message.
        </p>

        <%-- Empty-state — made visible from code-behind when no participants exist --%>
        <asp:PlaceHolder ID="NoParticipantsPH" runat="server" Visible="false">
          <div class="empty-note">
            You don't have any assigned participants yet. Once a University Admin connects
            participants to you as their Helper they will appear here.
          </div>
        </asp:PlaceHolder>

        <asp:Repeater ID="ParticipantsRepeater" runat="server">
          <HeaderTemplate><div class="participant-grid"></HeaderTemplate>

          <ItemTemplate>
            <div class="participant-card">

              <%-- Header: first name + optional unread-message badge --%>
              <div class="p-header">
                <div class="p-name"><%# Eval("FirstName") %></div>

                <asp:PlaceHolder ID="HasConvoPH" runat="server"
                                 Visible='<%# (bool)Eval("HasConversation") %>'>
                  <span class="msg-badge" title="This participant has sent you messages.">
                    <span class="msg-dot"></span>
                    <span>Messages</span>
                  </span>
                </asp:PlaceHolder>
              </div>

              <%-- Email address --%>
              <div class="p-email"><%# Eval("Email") %></div>

              <%-- University — hidden when empty --%>
              <asp:PlaceHolder ID="UniversityPH" runat="server"
                               Visible='<%# !string.IsNullOrWhiteSpace(Convert.ToString(Eval("University"))) %>'>
                <div class="p-meta"><%# Eval("University") %></div>
              </asp:PlaceHolder>

              <%-- Navigation to the participant's conversation threads --%>
              <div class="p-actions">
                <asp:HyperLink ID="ConversationsLink"
                               runat="server"
                               CssClass="btn-convo"
                               NavigateUrl='<%# Eval("ConversationsUrl") %>'>
                  💬 Conversations
                </asp:HyperLink>
              </div>

            </div>
          </ItemTemplate>

          <FooterTemplate></div></FooterTemplate>
        </asp:Repeater>

      </div>
      <%-- /participants-shell --%>

    </div><%-- /wrap --%>
  </form>
</body>
</html>
