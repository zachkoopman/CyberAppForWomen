<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="HelperConversation.aspx.cs" Inherits="CyberApp_FIA.Helper.HelperConversation" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
  <title>FIA • Conversation Thread</title>
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
       LAYOUT WRAPPER — narrower for a chat-style layout
    ============================================================ */
    .wrap {
      max-width: 860px;
      margin: 0 auto;
      padding: 28px 20px 56px;
    }

    /* ============================================================
       PAGE HEADER (shared pattern)
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

    .page-header-main { max-width: 560px; }

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

    /* Title: "Conversation with [Name]" — name gets brand gradient */
    .page-title {
      margin: 0 0 6px;
      font-family: Poppins, sans-serif;
      font-size: 1.6rem;
      font-weight: 700;
      color: var(--ink);
    }

    .page-title .participant-name {
      background: linear-gradient(135deg, var(--fia-blue), var(--fia-pink));
      -webkit-background-clip: text;
      -webkit-text-fill-color: transparent;
    }

    .page-sub {
      margin: 0;
      font-size: 0.93rem;
      color: var(--muted);
      max-width: 500px;
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
       THREAD SHELL
       Main white card containing the topic, message panel, and
       reply form.
    ============================================================ */
    .thread-shell {
      background: var(--surface);
      border-radius: var(--radius-lg);
      border: 1px solid var(--border);
      padding: 22px 22px 24px;
      box-shadow: var(--shadow-md);
      display: flex;
      flex-direction: column;
      gap: 18px;
    }

    /* ---- Thread metadata (topic title + timestamps) ---- */
    .thread-info { border-bottom: 1px solid var(--border); padding-bottom: 14px; }

    .thread-topic {
      font-family: Poppins, sans-serif;
      font-size: 1.1rem;
      font-weight: 600;
      color: var(--ink);
      margin: 0 0 4px;
    }

    .thread-meta {
      font-size: 0.82rem;
      color: var(--muted);
    }

    .thread-meta time { font-weight: 600; color: #374151; }

    /* ============================================================
       MESSAGE BUBBLES PANEL
       Scrollable panel showing the full conversation.
       Helper messages align right; participant messages align left.
    ============================================================ */
    .messages-panel {
      border-radius: var(--radius-md);
      border: 1px solid var(--border);
      background: var(--surface-raised);
      padding: 14px;
      max-height: 440px;
      overflow-y: auto;
      /* Custom scrollbar — subtle and brand-tinted */
      scrollbar-width: thin;
      scrollbar-color: rgba(240, 106, 169, 0.35) transparent;
    }

    .messages-panel::-webkit-scrollbar        { width: 5px; }
    .messages-panel::-webkit-scrollbar-thumb  { border-radius: var(--radius-pill); background: rgba(240, 106, 169, 0.35); }

    .messages-list {
      display: flex;
      flex-direction: column;
      gap: 12px;
    }

    /* Each row wraps a bubble + timestamp */
    .message-row {
      display: flex;
      flex-direction: column;
      max-width: 78%;
    }

    /* Helper's own messages → right-aligned */
    .message-row.helper {
      margin-left: auto;
      align-items: flex-end;
    }

    /* Participant's messages → left-aligned */
    .message-row.participant {
      margin-right: auto;
      align-items: flex-start;
    }

    /* Bubble shape */
    .message-bubble {
      border-radius: 16px;
      padding: 9px 13px;
      font-size: 0.9rem;
      line-height: 1.45;
      box-shadow: var(--shadow-sm);
    }

    /* Helper bubble: brand blue gradient */
    .message-bubble.helper {
      background: linear-gradient(135deg, var(--fia-blue), #5bbde8);
      color: #ffffff;
    }

    /* Participant bubble: neutral off-white */
    .message-bubble.participant {
      background: var(--surface);
      color: var(--ink);
      border: 1px solid var(--border);
    }

    /* Timestamp beneath each bubble */
    .message-meta {
      margin-top: 3px;
      font-size: 0.73rem;
      color: var(--subtle);
    }

    /* ============================================================
       REPLY COMPOSE PANEL
       Input area below the message panel for typing a reply.
    ============================================================ */
    .reply-panel {
      border-radius: var(--radius-md);
      border: 1px solid #dbeafe;
      background: linear-gradient(150deg, #eff6ff, var(--surface));
      padding: 14px 16px 16px;
      display: flex;
      flex-direction: column;
      gap: 10px;
    }

    .reply-label {
      font-size: 0.82rem;
      font-weight: 700;
      color: var(--ink);
      text-transform: uppercase;
      letter-spacing: 0.06em;
    }

    .reply-textarea {
      width: 100%;
      min-height: 88px;
      border-radius: var(--radius-sm);
      border: 1px solid var(--border);
      background: var(--surface);
      padding: 10px 12px;
      font-family: inherit;
      font-size: 0.9rem;
      color: var(--ink);
      resize: vertical;
      outline: none;
      transition: border-color 0.15s, box-shadow 0.15s;
    }

    .reply-textarea:focus {
      border-color: var(--fia-pink);
      box-shadow: 0 0 0 3px rgba(240, 106, 169, 0.18);
    }

    /* Send button row — right-aligned */
    .reply-actions {
      display: flex;
      justify-content: flex-end;
    }

    .btn-send {
      appearance: none;
      border: none;
      border-radius: var(--radius-pill);
      padding: 10px 20px;
      font-family: Poppins, sans-serif;
      font-size: 0.88rem;
      font-weight: 600;
      cursor: pointer;
      background: linear-gradient(135deg, var(--fia-pink), var(--fia-blue));
      color: #fff;
      box-shadow: 0 6px 18px rgba(240, 106, 169, 0.30);
      transition: opacity 0.15s, box-shadow 0.15s;
    }

    .btn-send:hover { opacity: 0.9; box-shadow: 0 10px 26px rgba(240, 106, 169, 0.36); }
    .btn-send:focus { outline: none; box-shadow: 0 0 0 3px rgba(42, 153, 219, 0.35); }

    /* Status / error message from the server after sending */
    .form-message { font-size: 0.85rem; color: var(--muted); }

    /* ============================================================
       RESPONSIVE
    ============================================================ */
    @media (max-width: 680px) {
      .wrap        { padding: 18px 14px 40px; }
      .page-header { flex-direction: column-reverse; align-items: flex-start; border-radius: var(--radius-md); }
      .message-row { max-width: 92%; }
    }
  </style>
</head>

<body>
  <form id="form1" runat="server">
    <div class="wrap">

      <!-- ======================================================
           PAGE HEADER
           "Conversation with [Participant Name]" — name bound
           from code-behind. Back button returns to the thread list.
      ====================================================== -->
      <div class="page-header">
        <div class="page-header-main">
          <div class="eyebrow">
            <span class="eyebrow-dot">FIA</span>
            <span>Helper workspace</span>
          </div>
          <h1 class="page-title">
            Conversation with
            <span class="participant-name">
              <asp:Literal ID="ParticipantName" runat="server" />
            </span>
          </h1>
          <p class="page-sub">
            Read the full message chain and send a clear, encouraging reply.
            Keep messages focused on specific questions and next steps.
          </p>
        </div>

        <%-- Back link URL is set in code-behind to preserve the participant ID query param --%>
        <asp:HyperLink ID="BackToParticipantConversations" runat="server" CssClass="btn-back">
          ← Back to conversations
        </asp:HyperLink>
      </div>


      <!-- ======================================================
           THREAD SHELL
           Contains: topic/timestamps · scrollable messages · reply box
      ====================================================== -->
      <div class="thread-shell">

        <!-- Thread topic and metadata -->
        <div class="thread-info">
          <h2 class="thread-topic">
            <asp:Literal ID="Topic" runat="server" />
          </h2>
          <p class="thread-meta">
            Started <time><asp:Literal ID="CreatedOn" runat="server" /></time>
            &nbsp;·&nbsp;
            Last updated <time><asp:Literal ID="LastUpdated" runat="server" /></time>
          </p>
        </div>

        <!-- ------------------------------------------------
             MESSAGE BUBBLE PANEL
             Each message rendered as a bubble aligned based on
             the FromRoleCss class ("helper" → right, "participant" → left).
        ------------------------------------------------ -->
        <div class="messages-panel">
          <asp:Repeater ID="MessagesRepeater" runat="server">
            <HeaderTemplate><div class="messages-list"></HeaderTemplate>

            <ItemTemplate>
              <%-- Row and bubble both use the FromRoleCss class for alignment + colour --%>
              <div class='message-row <%# Eval("FromRoleCss") %>'>
                <div class='message-bubble <%# Eval("FromRoleCss") %>'>
                  <%# Eval("BodyHtml") %>
                </div>
                <div class="message-meta">
                  <%# Eval("FromLabel") %>
                  &nbsp;·&nbsp;
                  <%# Eval("SentOnLocal", "{0:MMM d, yyyy • h:mm tt}") %>
                </div>
              </div>
            </ItemTemplate>

            <FooterTemplate></div></FooterTemplate>
          </asp:Repeater>
        </div>

        <!-- ------------------------------------------------
             REPLY COMPOSE PANEL
             Textarea + send button. FormMessage shows server
             feedback (success or error) after submission.
        ------------------------------------------------ -->
        <div class="reply-panel">
          <div class="reply-label">Your reply</div>

          <asp:TextBox ID="ReplyText"
                       runat="server"
                       TextMode="MultiLine"
                       CssClass="reply-textarea"
                       placeholder="Type your reply here…" />

          <div class="reply-actions">
            <asp:Button ID="SendReplyButton"
                        runat="server"
                        CssClass="btn-send"
                        Text="Send reply ↗"
                        OnClick="SendReplyButton_Click" />
          </div>

          <%-- FormMessage Literal has no CssClass, so it is wrapped in a div --%>
          <div class="form-message">
            <asp:Literal ID="FormMessage" runat="server" />
          </div>
        </div>

      </div>
      <%-- /thread-shell --%>

    </div><%-- /wrap --%>
  </form>
</body>
</html>
