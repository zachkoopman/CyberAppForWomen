<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Home.aspx.cs" Inherits="CyberApp_FIA.Participant.Home" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
  <title>FIA • Your Cyberfair</title>
  <meta name="viewport" content="width=device-width, initial-scale=1" />

  <!-- Fonts -->
  <link href="https://fonts.googleapis.com/css2?family=Lato:wght@300;400&family=Poppins:wght@600&display=swap" rel="stylesheet" />

  <style>
    /* ---------- Design tokens (brand colors, text colors, focus ring) ---------- */
    :root{
      --fia-pink:#f06aa9;
      --fia-blue:#2a99db;
      --fia-teal:#45c3b3;
      --ink:#1c1c1c;
      --muted:#6b7280;
      --ring:rgba(42,153,219,.25);
    }

    /* ---------- Global layout ---------- */
    * { box-sizing: border-box; }
    html, body { height: 100%; }

    body{
      margin: 0;
      font-family: Lato, Arial, sans-serif;
      color: var(--ink);
      background: linear-gradient(135deg, #fff, #f9fbff);
    }

    /* Wrap constrains content to a readable width and centers it */
    .wrap{
      min-height: 100vh;
      padding: 24px;
      max-width: 1100px;
      margin: 0 auto;
    }

    /* ---------- Header block (brand + context chips) ---------- */
    .brand{
      display: flex;
      align-items: center;
      gap: 10px;
      margin-bottom: 10px;
    }

    .badge{
      width: 42px;
      height: 42px;
      border-radius: 12px;
      background: linear-gradient(135deg, var(--fia-pink), var(--fia-blue));
      display: grid;
      place-items: center;
      color: #fff;
      font-family: Poppins;
    }

    h1{
      font-family: Poppins;
      margin: 0 0 6px 0;
      font-size: 1.35rem;
    }

    .sub{
      color: var(--muted);
      margin: 0 0 16px 0;
    }

    /* Context “pills” for university and event */
    .pill{
      display: inline-block;
      padding: 6px 10px;
      border-radius: 999px;
      background: #f6f7fb;
      border: 1px solid #e8eef7;
      margin-right: 8px;
      font-size: .9rem;
    }

    /* ---------- Cards grid (vertical stack) ---------- */
    .grid{
      display: grid;
      grid-template-columns: 1fr; /* single column = vertical cards */
      gap: 16px;
      max-width: 820px;           /* keeps line length comfortable */
      margin: 0 auto;             /* centers the column on the page */
    }
    /* media queries no longer needed */

    /* ---------- Card styling ---------- */
    .card{
      background: #fff;
      border: 1px solid #e8eef7;
      border-radius: 20px;
      box-shadow: 0 12px 36px rgba(42,153,219,.08);
      padding: 18px;
      display: flex;
      flex-direction: column;
    }

    .title{
      font-family: Poppins;
      font-weight: 600;
      margin: 0 0 6px 0;
    }

    .meta{
      color: var(--muted);
      font-size: .9rem;
      margin-bottom: 8px;
    }

    .tags{
      margin-top: 10px;
      color: var(--muted);
      font-size: .9rem;
    }

    /* Site-wide informational notice */
    .note{
      background: #f6f7fb;
      border: 1px solid #e8eef7;
      border-radius: 12px;
      padding: 12px;
      color: var(--muted);
      font-size: .95rem;
      margin-bottom: 12px;
    }

    /* ---------- Sessions table inside each card ---------- */
    .sess{
      width: 100%;
      border-collapse: collapse;
      margin-top: 10px;
    }
    .sess th,
    .sess td{
      padding: 8px;
      border-bottom: 1px solid #f0f3f9;
      text-align: left;
      font-size: .92rem;
    }
    .sess th{
      font-weight: 600;
      font-family: Poppins;
    }
  </style>
</head>

<body>
  <form id="form1" runat="server">
    <div class="wrap">

      <!-- Brand header + context chips (University, Event, Change link) -->
      <div class="brand">
        <div class="badge">FIA</div>
        <div>
          <h1>Your Cyberfair</h1>
          <div class="sub">
            <span class="pill">
              University:
              <asp:Literal ID="University" runat="server" />
            </span>

            <span class="pill">
              Event:
              <asp:Literal ID="EventName" runat="server" />
            </span>

            <!-- Change event link styled like a pill -->
            <a class="pill"
               href="<%: ResolveUrl("~/Account/Participant/SelectEvent.aspx") %>"
               style="text-decoration:none;color:var(--fia-blue);border-color:#d9e9f6;background:#f0f7fd">
              Change event
            </a>
          </div>
        </div>
      </div>

      <!-- Privacy note shown to participants -->
      <div class="note">
        <strong>Privacy:</strong>
        Your learning results are private to you and the FIA team.
        Your university sees only anonymized or aggregated insights—never your personal answers.
      </div>

      <!-- Empty state if no microcourses are visible -->
      <asp:PlaceHolder ID="EmptyPH" runat="server" Visible="false">
        <div class="note">
          No visible microcourses yet for this event. Please check back later.
        </div>
      </asp:PlaceHolder>

      <!-- Course cards (one per microcourse) -->
      <asp:Repeater ID="CoursesRepeater" runat="server">
        <HeaderTemplate>
          <%-- Start grid container --%>
          <div class="grid">
        </HeaderTemplate>

        <ItemTemplate>
          <div class="card">
            <h3 class="title"><%# Eval("title") %></h3>
            <div class="meta"><%# Eval("duration") %></div>
            <div><%# Eval("summary") %></div>

            <!-- Sessions list injected as HTML from server (table with .sess class) -->
            <asp:Literal ID="SessHtml"
                         runat="server"
                         Mode="PassThrough"
                         Text='<%# Eval("sessionsHtml") %>' />

            <div class="tags"><%# Eval("tags") %></div>
          </div>
        </ItemTemplate>

        <FooterTemplate>
          <%-- End grid container --%>
          </div>
        </FooterTemplate>
      </asp:Repeater>

      <!-- General page message placeholder -->
      <asp:Label ID="FormMessage" runat="server" EnableViewState="false" />

    </div>
  </form>
</body>
</html>

