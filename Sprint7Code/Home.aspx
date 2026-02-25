<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Home.aspx.cs" Inherits="CyberApp_FIA.Helper.Home" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
  <title>FIA • Helper Workspace</title>
  <meta name="viewport" content="width=device-width, initial-scale=1" />
  <link href="https://fonts.googleapis.com/css2?family=Lato:wght@300;400&family=Poppins:wght@500;600;700&display=swap" rel="stylesheet" />

  <style>
    /* ============================================================
       FIA DESIGN TOKENS
       Core brand palette, spacing, and shadow variables shared
       across every page in the Helper workspace.
    ============================================================ */
    :root {
      /* Brand colours */
      --fia-pink:       #f06aa9;
      --fia-pink-soft:  rgba(240, 106, 169, 0.12);
      --fia-blue:       #2a99db;
      --fia-blue-soft:  rgba(42, 153, 219, 0.12);
      --fia-teal:       #45c3b3;
      --fia-teal-soft:  rgba(69, 195, 179, 0.12);

      /* Semantic colours */
      --ink:            #1c2233;
      --muted:          #6b7280;
      --subtle:         #94a3b8;

      /* Surface colours */
      --bg:             #f2f4fb;
      --surface:        #ffffff;
      --surface-raised: #fafbff;
      --border:         #e4e9f2;
      --border-soft:    rgba(226, 232, 240, 0.7);

      /* Status colours */
      --green:          #16a34a;
      --green-bg:       #ecfdf5;
      --green-border:   #bbf7d0;
      --red:            #dc2626;
      --red-bg:         #fef2f2;
      --red-border:     #fecaca;

      /* Elevation (box-shadows) */
      --shadow-sm:      0 2px 8px rgba(15, 23, 42, 0.06);
      --shadow-md:      0 8px 24px rgba(15, 23, 42, 0.08);
      --shadow-lg:      0 18px 48px rgba(15, 23, 42, 0.10);

      /* Border radii */
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
      /* Page background with subtle brand blush */
      background:
        radial-gradient(ellipse at 0% 0%, rgba(240, 106, 169, 0.08) 0%, transparent 50%),
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
       HERO / IDENTITY PANEL
       Top card showing the helper's name, university, and role.
    ============================================================ */
    .hero {
      position: relative;
      border-radius: var(--radius-lg);
      padding: 28px 28px 30px;
      overflow: hidden;
      border: 1px solid var(--border-soft);
      box-shadow: var(--shadow-lg);
      /* Layered gradient background — keeps brand warmth without being heavy */
      background:
        radial-gradient(circle at 0% 0%,   rgba(240, 106, 169, 0.13) 0%, transparent 55%),
        radial-gradient(circle at 100% 0%,  rgba(69,  195, 179, 0.14) 0%, transparent 55%),
        linear-gradient(150deg, #fefcff, #f4f7ff);
    }

    /* FIA monogram badge */
    .hero-badge {
      width: 54px;
      height: 54px;
      border-radius: 16px;
      background: linear-gradient(135deg, var(--fia-pink), var(--fia-blue));
      display: grid;
      place-items: center;
      color: #fff;
      font-family: Poppins, sans-serif;
      font-size: 1rem;
      font-weight: 700;
      letter-spacing: 0.06em;
      flex-shrink: 0;
      box-shadow: 0 6px 16px rgba(240, 106, 169, 0.35);
    }

    .hero-body {
      display: flex;
      align-items: flex-start;
      gap: 18px;
    }

    .hero-text { flex: 1; }

    .hero-greeting {
      margin: 0 0 4px;
      font-family: Poppins, sans-serif;
      font-size: 1.65rem;
      font-weight: 600;
      line-height: 1.25;
      color: var(--ink);
    }

    /* Helper's name gets the brand gradient treatment */
    .hero-greeting .name {
      background: linear-gradient(135deg, var(--fia-pink), var(--fia-blue));
      -webkit-background-clip: text;
      -webkit-text-fill-color: transparent;
    }

    .hero-sub {
      margin: 0;
      color: var(--muted);
      font-size: 0.95rem;
      max-width: 500px;
    }

    /* Metadata chips row — university, role */
    .hero-chips {
      margin-top: 18px;
      display: flex;
      flex-wrap: wrap;
      gap: 8px;
    }

    .chip {
      display: inline-flex;
      align-items: center;
      gap: 5px;
      padding: 5px 12px;
      border-radius: var(--radius-pill);
      background: rgba(255, 255, 255, 0.85);
      border: 1px solid var(--border);
      font-size: 0.83rem;
      line-height: 1;
    }

    .chip-label {
      font-weight: 700;
      color: var(--muted);
      text-transform: uppercase;
      letter-spacing: 0.05em;
      font-size: 0.75rem;
    }

    .chip-value { color: var(--ink); font-weight: 600; }

    /* Sign-out button — top-right corner of hero */
    .btn-signout {
      position: absolute;
      top: 20px;
      right: 20px;
      display: inline-flex;
      align-items: center;
      gap: 6px;
      padding: 6px 13px;
      border: 1px solid rgba(220, 38, 38, 0.25);
      border-radius: var(--radius-pill);
      background: rgba(255, 255, 255, 0.9);
      font-family: Poppins, sans-serif;
      font-size: 0.82rem;
      font-weight: 600;
      color: var(--red);
      text-decoration: none;
      cursor: pointer;
      box-shadow: var(--shadow-sm);
      transition: box-shadow 0.15s, transform 0.15s;
    }

    .btn-signout:hover {
      box-shadow: var(--shadow-md);
      transform: translateY(-1px);
    }

    /* ============================================================
       WORKSPACE CARDS GRID
       Three feature cards linking to the main workspace sections.
    ============================================================ */
    .cards-grid {
      margin-top: 24px;
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
      gap: 16px;
    }

    .card {
      background: var(--surface);
      border-radius: var(--radius-lg);
      border: 1px solid var(--border);
      padding: 22px 22px 24px;
      box-shadow: var(--shadow-md);
      display: flex;
      flex-direction: column;
      gap: 0;
      position: relative;
      overflow: hidden;
      transition: box-shadow 0.2s, transform 0.2s;
    }

    .card:hover {
      box-shadow: var(--shadow-lg);
      transform: translateY(-2px);
    }

    /* Left accent stripe per card — uses brand colours */
    .card::before {
      content: "";
      position: absolute;
      inset-block: 0;
      inset-inline-start: 0;
      width: 4px;
    }

    .card--cert::before  { background: linear-gradient(180deg, var(--fia-pink), var(--fia-teal)); }
    .card--sched::before { background: linear-gradient(180deg, var(--fia-blue), var(--fia-pink)); }
    .card--help::before  { background: linear-gradient(180deg, var(--fia-teal), var(--fia-blue)); }

    /* Card header row */
    .card-header {
      display: flex;
      align-items: flex-start;
      justify-content: space-between;
      gap: 10px;
      margin-bottom: 8px;
    }

    .card-icon {
      font-size: 1.5rem;
      line-height: 1;
      flex-shrink: 0;
    }

    /* Section badge pill */
    .badge {
      padding: 3px 10px;
      border-radius: var(--radius-pill);
      font-size: 0.72rem;
      font-weight: 700;
      text-transform: uppercase;
      letter-spacing: 0.07em;
      background: linear-gradient(135deg, var(--fia-pink-soft), var(--fia-blue-soft));
      color: #374151;
      border: 1px solid var(--border);
      white-space: nowrap;
    }

    .card-title {
      margin: 0 0 6px;
      font-family: Poppins, sans-serif;
      font-size: 1.08rem;
      font-weight: 600;
      color: var(--ink);
    }

    .card-body {
      margin: 0 0 16px;
      font-size: 0.92rem;
      color: var(--muted);
      line-height: 1.6;
      flex: 1;
    }

    /* Primary CTA button inside each card */
    .btn-primary {
      display: inline-flex;
      align-items: center;
      gap: 7px;
      padding: 10px 18px;
      border-radius: var(--radius-pill);
      border: none;
      background: linear-gradient(135deg, var(--fia-blue), var(--fia-teal));
      color: #fff;
      font-family: Poppins, sans-serif;
      font-size: 0.88rem;
      font-weight: 600;
      text-decoration: none;
      cursor: pointer;
      box-shadow: 0 6px 18px rgba(42, 153, 219, 0.30);
      align-self: flex-start;
      transition: opacity 0.15s, box-shadow 0.15s, transform 0.15s;
    }

    .btn-primary:hover {
      opacity: 0.92;
      box-shadow: 0 10px 26px rgba(42, 153, 219, 0.38);
      transform: translateY(-1px);
    }

    .btn-primary .btn-icon { font-size: 1rem; }

    /* ============================================================
       RESPONSIVE — collapse to single column on mobile
    ============================================================ */
    @media (max-width: 680px) {
      .wrap            { padding: 18px 14px 40px; }
      .hero            { padding: 20px 16px 22px; border-radius: var(--radius-md); }
      .hero-greeting   { font-size: 1.35rem; }
      .btn-signout     { top: 14px; right: 14px; }
      .cards-grid      { grid-template-columns: 1fr; }
    }
  </style>
</head>

<body>
  <form id="form1" runat="server">
    <div class="wrap">

      <!-- ======================================================
           HERO / IDENTITY PANEL
           Displays the helper's name, university, and role.
           Sign-out button sits in the top-right corner.
      ====================================================== -->
      <div class="hero">

        <%-- Sign-out link rendered as an ASP button to trigger server-side logout --%>
        <asp:LinkButton ID="BtnLogout"
                        runat="server"
                        CssClass="btn-signout"
                        OnClick="BtnLogout_Click"
                        CausesValidation="false">
          ⟵ Sign out
        </asp:LinkButton>

        <div class="hero-body">
          <div class="hero-badge">FIA</div>

          <div class="hero-text">
            <h1 class="hero-greeting">
              Welcome, <span class="name"><asp:Literal ID="HelperName" runat="server" /></span>
            </h1>
            <p class="hero-sub">
              Thank you for showing up as a peer helper. This workspace keeps your sessions,
              certification progress, and 1:1 support in one calm place.
            </p>
          </div>
        </div>

        <%-- Metadata chips: university and role bound from code-behind --%>
        <div class="hero-chips">
          <span class="chip">
            <span class="chip-label">University</span>
            <span class="chip-value"><asp:Literal ID="University" runat="server" /></span>
          </span>
          <span class="chip">
            <span class="chip-label">Role</span>
            <span class="chip-value"><asp:Literal ID="RoleLiteral" runat="server" /></span>
          </span>
        </div>

      </div>
      <%-- /hero --%>


      <!-- ======================================================
           WORKSPACE CARDS
           Three cards linking to the three main workspace areas:
           Certification Progress · Schedule · 1:1 Help
      ====================================================== -->
      <div class="cards-grid">

        <!-- Certification Progress -->
        <div class="card card--cert">
          <div class="card-header">
            <span class="card-icon">🛡️</span>
            <span class="badge">Progress</span>
          </div>
          <h2 class="card-title">Cybersecurity Certification</h2>
          <p class="card-body">
            See how close you are to being certified in each FIA microcourse. View quiz scores,
            teaching progress, and 1:1 help totals — plus links to videos, slides, and quizzes.
          </p>
          <a href="<%: ResolveUrl("~/Account/Helper/CertificationProgress.aspx") %>"
             class="btn-primary">
            <span class="btn-icon">📋</span>
            <span>View certification progress</span>
          </a>
        </div>

        <!-- Schedule -->
        <div class="card card--sched">
          <div class="card-header">
            <span class="card-icon">📅</span>
            <span class="badge">Schedule</span>
          </div>
          <h2 class="card-title">Your Upcoming Sessions</h2>
          <p class="card-body">
            Browse every session you are leading or supporting. Review dates, times, room links,
            and capacity — so you can show up prepared for each group.
          </p>
          <a href="<%: ResolveUrl("~/Account/Helper/Schedule.aspx") %>"
             class="btn-primary">
            <span class="btn-icon">🗓️</span>
            <span>Open your schedule</span>
          </a>
        </div>

        <!-- 1:1 Help Sessions -->
        <div class="card card--help">
          <div class="card-header">
            <span class="card-icon">🤝</span>
            <span class="badge">Support</span>
          </div>
          <h2 class="card-title">1:1 Help Sessions</h2>
          <p class="card-body">
            Walk participants through real security tasks — setting up two-factor authentication,
            updating privacy settings, or installing a password manager.
            This view lists every participant assigned to you.
          </p>
          <a href="<%: ResolveUrl("~/Account/Helper/OneOnOneHelp.aspx") %>"
             class="btn-primary">
            <span class="btn-icon">👥</span>
            <span>See assigned participants</span>
          </a>
        </div>

      </div>
      <%-- /cards-grid --%>

    </div><%-- /wrap --%>
  </form>
</body>
</html>
