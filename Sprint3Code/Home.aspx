<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Home.aspx.cs" Inherits="CyberApp_FIA.Participant.Home" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
  <title>FIA • Your Cyberfair</title>
  <meta name="viewport" content="width=device-width, initial-scale=1" />

  <!-- Fonts -->
  <link href="https://fonts.googleapis.com/css2?family=Lato:wght@300;400&family=Poppins:wght@600&display=swap" rel="stylesheet" />

  <style>
    /* ---------- Design tokens (brand) ---------- */
    :root{
      --fia-pink:#f06aa9;
      --fia-blue:#2a99db;
      --fia-teal:#45c3b3;
      --ink:#1c1c1c;
      --muted:#6b7280;
      --ring:rgba(42,153,219,.25);
      --card-border:#e8eef7;
      --card-bg:#ffffff;
      --page-grad:linear-gradient(135deg,#ffffff,#f9fbff);
      --pill-bg:#f6f7fb;
    }

    *{ box-sizing:border-box; }
    html,body{ height:100%; }
    body{
      margin:0;
      font-family:Lato, Arial, sans-serif;
      color:var(--ink);
      background:var(--page-grad);
    }

    .wrap{
      min-height:100vh;
      padding:24px;
      max-width:1100px;
      margin:0 auto;
    }

    /* ---------- Header ---------- */
    .brand{ display:flex; align-items:center; gap:10px; margin-bottom:10px; }
    .badge{
      width:42px; height:42px; border-radius:12px;
      background:linear-gradient(135deg, var(--fia-pink), var(--fia-blue));
      display:grid; place-items:center; color:#fff; font-family:Poppins;
    }
    h1{ font-family:Poppins; margin:0 0 6px 0; font-size:1.35rem; }
    .sub{ color:var(--muted); margin:0 0 16px 0; }

    /* Base pill + remove underline for anchors */
    .pill{
      display:inline-block;
      padding:6px 10px;
      border-radius:999px;
      background:var(--pill-bg);
      border:1px solid var(--card-border);
      margin-right:8px;
      font-size:.9rem;
      text-decoration:none;
      color:inherit;
    }
    .pill-link{ text-decoration:none; color:var(--fia-blue); border-color:#d9e9f6; background:#f0f7fd; }

    /* Make the chips row a flex line + push helper */
    .subchips{ display:flex; align-items:center; gap:8px; flex-wrap:wrap; }
    .pill-push{ margin-left:auto; }

    /* ---------- Note ---------- */
    .note{
      background:var(--pill-bg); border:1px solid var(--card-border);
      border-radius:12px; padding:12px; color:var(--muted);
      font-size:.95rem; margin-bottom:12px;
    }

    /* ---------- Sections / titles / dividers ---------- */
    .section{ margin:28px 0; }
    .section-title{
      font-family:Poppins; font-weight:600; font-size:1.1rem; margin:0 0 10px 0;
    }
    .divider{
      height:1px; border:none; margin:24px 0 8px 0;
      background:linear-gradient(90deg, rgba(42,153,219,.0), rgba(42,153,219,.25), rgba(42,153,219,.0));
    }

    /* ---------- Sessions Grid (unique class to avoid collisions) ---------- */
    .fia-sessions-grid{
      display:grid;
      grid-template-columns:repeat(3, minmax(0, 1fr)) !important;
      gap:18px;
      margin-top:10px;
      grid-auto-flow:row;
    }
    @media (max-width: 980px){
      .fia-sessions-grid{ grid-template-columns:repeat(2, minmax(0, 1fr)) !important; }
    }
    @media (max-width: 640px){
      .fia-sessions-grid{ grid-template-columns:1fr !important; }
    }

    /* ---------- Card ---------- */
    .card{
      background:var(--card-bg);
      border:1px solid var(--card-border);
      border-radius:20px;
      box-shadow:0 12px 36px rgba(42,153,219,.08);
      padding:18px;
      display:flex; flex-direction:column; gap:10px;
      transition: transform .12s ease, box-shadow .12s ease, border-color .12s ease;
      width:100%;
    }
    .card:hover, .card:focus-within{
      transform: translateY(-2px);
      border-color:#d4e6f7;
      box-shadow:0 16px 40px rgba(42,153,219,.14);
    }
    .title{ font-family:Poppins; font-weight:600; margin:0; font-size:1.05rem; line-height:1.3; }
    .meta-row{ display:flex; align-items:center; gap:8px; flex-wrap:wrap; color:var(--muted); font-size:.92rem; }
    .chip{
      display:inline-flex; align-items:center; gap:6px; padding:6px 10px; border-radius:999px; font-size:.85rem;
      background:#fff; border:1px dashed #e7edf7;
    }
    .helper-chip{ border:1px solid rgba(240,106,169,.25); background:linear-gradient(180deg,#fff,#fff7fb); }
    .dot{ width:8px; height:8px; border-radius:999px; background:var(--fia-pink); display:inline-block; }

    .remain-badge{
      margin-left:auto; padding:6px 10px; border-radius:999px;
      font-family:Poppins; font-weight:600; font-size:.85rem;
      color:#084c61; background:linear-gradient(135deg,#e6fbf7,#d6f5ef);
      border:1px solid rgba(69,195,179,.35);
    }
    .remain-low{
      background:linear-gradient(135deg,#fff2f7,#ffe7f0);
      color:#7a103a; border-color:rgba(240,106,169,.45);
    }

    .subtle{ color:var(--muted); font-size:.88rem; border-top:1px solid #f0f3f9; padding-top:10px; }

    .cta-row{ display:flex; gap:8px; margin-top:4px; }
    .btn{
      appearance:none; border:none; cursor:pointer;
      padding:10px 12px; border-radius:12px; font-weight:600; font-family:Poppins;
      box-shadow:0 2px 0 rgba(0,0,0,.04);
    }
    .btn-primary{ background:linear-gradient(135deg,var(--fia-blue),#6bc1f1); color:#fff; }
    .btn-ghost{ background:#fff; color:var(--fia-blue); border:1px solid #d9e9f6; }
    .btn:focus{ outline:none; box-shadow:0 0 0 4px var(--ring); }

    /* Fully pink-filled pill (override base) */
    .pill-pink{
      background: linear-gradient(135deg, var(--fia-pink), #ff86bf) !important;
      border-color: rgba(240,106,169,.55) !important;
      color: #fff !important;
    }
    .pill-pink:hover{ filter:brightness(.98); }
    .pill-pink:focus{ outline:none; box-shadow:0 0 0 4px var(--ring); }
  </style>
</head>

<body>
  <form id="form1" runat="server">
    <div class="wrap">

      <!-- Header -->
      <div class="brand">
        <div class="badge">FIA</div>
        <div>
          <h1>Your Cyberfair</h1>

          <!-- Chips row -->
          <div class="sub subchips">
            <span class="pill">University: <asp:Literal ID="University" runat="server" /></span>
            <span class="pill">Event: <asp:Literal ID="EventName" runat="server" /></span>

            <a class="pill pill-pink" href="<%: ResolveUrl("~/Account/Participant/SelectEvent.aspx?change=1") %>">
              Change event
            </a>

            <asp:LinkButton ID="BtnLogout"
                            runat="server"
                            CssClass="pill pill-pink pill-push"
                            OnClick="BtnLogout_Click"
                            CausesValidation="false">
              Sign out
            </asp:LinkButton>
          </div>
        </div>
      </div>

      <!-- Privacy note -->
      <div class="note">
        <strong>Privacy:</strong> Your learning results are private to you and the FIA team.
        Your university sees only anonymized or aggregated insights—never your personal answers.
      </div>

      <br /><br />
      <%@ Register Src="~/Account/Participant/ParticipantScoreWidget.ascx" TagName="ParticipantScoreWidget" TagPrefix="fia" %>
      <fia:ParticipantScoreWidget ID="ScoreWidget" runat="server" />
      <br /><br />

      <!-- ==== MY SESSIONS (NEW) ==== -->
      <asp:PlaceHolder ID="MySessionsWrap" runat="server" Visible="false">
        <div class="section">
          <h2 class="section-title">My Sessions</h2>
          <asp:Repeater ID="MySessionsRepeater" runat="server">
            <HeaderTemplate>
              <div class="fia-sessions-grid">
            </HeaderTemplate>
            <ItemTemplate>
              <div class="card">
                <h3 class="title"><%# Eval("microcourseTitle") %></h3>
                <div class="meta-row">
                  <span class="chip helper-chip">
                    <span class="dot" style="background:var(--fia-teal);"></span>
                    Helper: <strong><%# Eval("helperName") %></strong>
                  </span>
                  <span class="remain-badge">Enrolled</span>
                </div>
                <div class="subtle">
                  <%# Eval("startLocal", "{0:ddd, MMM d • h:mm tt}") %>
                  <asp:PlaceHolder runat="server" Visible='<%# !string.IsNullOrWhiteSpace(Convert.ToString(Eval("room"))) %>'>
                    <div class="subtle">Room <%# Eval("room") %></div>
                  </asp:PlaceHolder>
                </div>
                <div class="cta-row">
                  <asp:Button ID="CompleteBtn" runat="server" CssClass="btn btn-ghost" Text="Mark as Complete"
                              CommandName="complete" CommandArgument='<%# Eval("sessionId") %>' />
                </div>
              </div>
            </ItemTemplate>
            <FooterTemplate>
              </div>
            </FooterTemplate>
          </asp:Repeater>

          <hr class="divider" />
        </div>
      </asp:PlaceHolder>

      <!-- Empty state for sessions -->
      <asp:PlaceHolder ID="EmptySessionsPH" runat="server" Visible="false">
        <div class="note">No sessions are currently available. Please check back soon.</div>
      </asp:PlaceHolder>

      <!-- ==== AVAILABLE SESSIONS (heading + grid) ==== -->
      <div class="section">
        <h2 class="section-title">Available Sessions</h2>

        <asp:Repeater ID="SessionsRepeater" runat="server">
          <HeaderTemplate>
            <div class="fia-sessions-grid">
          </HeaderTemplate>

          <ItemTemplate>
            <div class="card">
              <h3 class="title"><%# Eval("microcourseTitle") %></h3>

              <div class="meta-row">
                <span class="chip helper-chip">
                  <span class="dot" style="background:var(--fia-teal);"></span>
                  Helper: <strong><%# Eval("helperName") %></strong>
                </span>

                <span class='<%# (Convert.ToInt32(Eval("remainingSeats")) <= 3) ? "remain-badge remain-low" : "remain-badge" %>'>
                  Seats Remaining: <%# Eval("remainingSeats") %> (<%# Eval("capacity") %>)
                </span>
              </div>

              <asp:Literal ID="StatusBadge" runat="server"
                Visible='<%# (bool)Eval("isEnrolled") || (bool)Eval("isWaitlisted") %>'
                Text='<%# (bool)Eval("isEnrolled") ? "<div class=\"note\" style=\"margin-top:6px;border-style:dashed\">You are enrolled in this session.</div>" : "<div class=\"note\" style=\"margin-top:6px;border-style:dashed\">You are on the waitlist for this session.</div>" %>' />

              <div class="subtle">
                <%# Eval("startLocal", "{0:ddd, MMM d • h:mm tt}") %>
              </div>

              <div class="cta-row">
                <asp:Button ID="EnrollBtn" runat="server" CssClass="btn btn-primary" Text="Enroll"
                            CommandName="enroll" CommandArgument='<%# Eval("sessionId") %>'
                            Visible='<%# !(bool)Eval("isEnrolled") && !(bool)Eval("isWaitlisted") && !(bool)Eval("isFull") %>' />

                <asp:Button ID="WaitlistBtn" runat="server" CssClass="btn btn-ghost" Text="Join Waitlist"
                            CommandName="waitlist" CommandArgument='<%# Eval("sessionId") %>'
                            Visible='<%# !(bool)Eval("isEnrolled") && !(bool)Eval("isWaitlisted") && (bool)Eval("isFull") %>' />
              </div>
            </div>
          </ItemTemplate>

          <FooterTemplate>
            </div>
          </FooterTemplate>
        </asp:Repeater>
      </div>

      <asp:Label ID="FormMessage" runat="server" EnableViewState="false" />
    </div>
  </form>
</body>
</html>





