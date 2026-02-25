<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ParticipantConversations.aspx.cs" Inherits="CyberApp_FIA.Helper.ParticipantConversations" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
  <title>FIA • Participant Conversations</title>
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
       LAYOUT WRAPPER
    ============================================================ */
    .wrap {
      max-width: 900px;
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

    .page-header-main { max-width: 580px; }

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

    /* Title: "Conversations with [Name]" — name gets brand gradient */
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
       CONVERSATIONS SHELL
       White card wrapping the thread list.
    ============================================================ */
    .conversations-shell {
      background: var(--surface);
      border-radius: var(--radius-lg);
      border: 1px solid var(--border);
      padding: 22px 22px 24px;
      box-shadow: var(--shadow-md);
    }

    /* Shell header: title + badge */
    .shell-head {
      display: flex;
      align-items: center;
      gap: 10px;
      margin: 0 0 6px;
      font-family: Poppins, sans-serif;
      font-size: 1.1rem;
      font-weight: 600;
      color: var(--ink);
    }

    .shell-pill {
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

    .shell-sub { margin: 0 0 16px; font-size: 0.9rem; color: var(--muted); }

    /* Empty-state notice */
    .empty-note {
      padding: 14px 16px;
      border-radius: var(--radius-md);
      background: var(--surface-raised);
      border: 1px solid var(--border);
      font-size: 0.9rem;
      color: var(--muted);
    }

    /* ============================================================
       CONVERSATION THREAD CARDS
       Stacked list — one card per conversation thread.
    ============================================================ */
    .threads-list {
      display: flex;
      flex-direction: column;
      gap: 10px;
    }

    .thread-card {
      border-radius: var(--radius-md);
      border: 1px solid var(--border);
      background: linear-gradient(150deg, var(--surface), rgba(42, 153, 219, 0.03));
      padding: 14px 16px 16px;
      box-shadow: var(--shadow-sm);
      display: flex;
      flex-direction: column;
      gap: 4px;
      position: relative;
      overflow: hidden;
      transition: box-shadow 0.2s, transform 0.2s;
    }

    .thread-card:hover {
      box-shadow: var(--shadow-md);
      transform: translateY(-1px);
    }

    /* Left accent bar — pink to teal */
    .thread-card::before {
      content: "";
      position: absolute;
      inset-block: 0;
      inset-inline-start: 0;
      width: 4px;
      background: linear-gradient(180deg, var(--fia-pink), var(--fia-teal));
    }

    /* Thread topic / subject line */
    .thread-topic {
      font-family: Poppins, sans-serif;
      font-size: 0.98rem;
      font-weight: 600;
      color: var(--ink);
      padding-inline-start: 4px;
    }

    /* Started and last-updated timestamps */
    .thread-meta {
      font-size: 0.82rem;
      color: var(--muted);
      padding-inline-start: 4px;
    }

    .thread-meta time { font-weight: 600; color: #374151; }

    /* View-thread button */
    .thread-actions { margin-top: 8px; padding-inline-start: 4px; }

    .btn-view {
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

    .btn-view:hover {
      border-color: #bfdbfe;
      box-shadow: var(--shadow-md);
    }

    /* ============================================================
       RESPONSIVE
    ============================================================ */
    @media (max-width: 680px) {
      .wrap        { padding: 18px 14px 40px; }
      .page-header { flex-direction: column-reverse; align-items: flex-start; border-radius: var(--radius-md); }
    }
  </style>
</head>

<body>
  <form id="form1" runat="server">
    <div class="wrap">

      <!-- ======================================================
           PAGE HEADER
           Title includes the participant's first name (bound from
           code-behind). Back button returns to the 1:1 list.
      ====================================================== -->
      <div class="page-header">
        <div class="page-header-main">
          <div class="eyebrow">
            <span class="eyebrow-dot">FIA</span>
            <span>Helper workspace</span>
          </div>
          <h1 class="page-title">
            Conversations with
            <span class="participant-name">
              <asp:Literal ID="ParticipantName" runat="server" />
            </span>
          </h1>
          <p class="page-sub">
            Browse all one-on-one threads you have with this participant.
            Open any thread to read the full message chain and send a reply.
          </p>
        </div>
        <a href="<%: ResolveUrl("~/Account/Helper/OneOnOneHelp.aspx") %>" class="btn-back">
          ← Back to 1:1 list
        </a>
      </div>


      <!-- ======================================================
           CONVERSATION THREADS
           One card per thread showing topic, timestamps, and a
           link to open the full message chain.
      ====================================================== -->
      <div class="conversations-shell">

        <div class="shell-head">
          Conversation threads
          <span class="shell-pill">Messages</span>
        </div>
        <p class="shell-sub">
          Each card shows the topic and last-updated time.
          Click "View thread" to read the full conversation and write back.
        </p>

        <%-- Empty-state — made visible from code-behind when no threads exist --%>
        <asp:PlaceHolder ID="NoConversationsPH" runat="server" Visible="false">
          <div class="empty-note">
            You don't have any conversations with this participant yet.
            Once they send you a message their threads will appear here.
          </div>
        </asp:PlaceHolder>

        <asp:Repeater ID="ConversationsRepeater" runat="server">
          <HeaderTemplate><div class="threads-list"></HeaderTemplate>

          <ItemTemplate>
            <div class="thread-card">

              <%-- Thread subject / topic --%>
              <div class="thread-topic"><%# Eval("Topic") %></div>

              <%-- Start and last-updated timestamps (formatted to local time) --%>
              <div class="thread-meta">
                Started <time><%# Eval("CreatedOnLocal", "{0:MMM d, yyyy • h:mm tt}") %></time>
                &nbsp;·&nbsp;
                Last updated <time><%# Eval("LastUpdatedLocal", "{0:MMM d, yyyy • h:mm tt}") %></time>
              </div>

              <%-- Link to the full HelperConversation thread view --%>
              <div class="thread-actions">
                <a class="btn-view" href='<%# Eval("ViewUrl") %>'>
                  💬 View thread
                </a>
              </div>

            </div>
          </ItemTemplate>

          <FooterTemplate></div></FooterTemplate>
        </asp:Repeater>

      </div>
      <%-- /conversations-shell --%>

    </div><%-- /wrap --%>
  </form>
</body>
</html>
