<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CertificationProgress.aspx.cs" Inherits="CyberApp_FIA.Helper.CertificationProgress" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
  <title>FIA • Certification Progress</title>
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

      /* Status: green (certified / met) */
      --green:          #16a34a;
      --green-bg:       #ecfdf5;
      --green-border:   #bbf7d0;

      /* Status: red (not certified / not met) */
      --red:            #dc2626;
      --red-bg:         #fef2f2;
      --red-border:     #fecaca;

      /* Status: blue (eligible to teach, in progress) */
      --blue-status:    #1d4ed8;
      --blue-bg:        #dbeafe;
      --blue-border:    rgba(59, 130, 246, 0.60);

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
       PAGE HERO
       Full-width hero with helper's name tag and page description.
    ============================================================ */
    .cert-hero {
      display: flex;
      align-items: flex-start;
      justify-content: space-between;
      gap: 20px;
      padding: 24px 26px;
      margin-bottom: 22px;
      border-radius: var(--radius-lg);
      border: 1px solid var(--border-soft);
      box-shadow: var(--shadow-lg);
      background:
        radial-gradient(circle at 0% 0%,   rgba(240, 106, 169, 0.14) 0%, transparent 55%),
        radial-gradient(circle at 100% 0%,  rgba(69,  195, 179, 0.18) 0%, transparent 55%),
        linear-gradient(150deg, #fefcff, #f4f7ff);
    }

    .cert-hero-main { max-width: 620px; }

    /* Breadcrumb eyebrow */
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

    .cert-title {
      margin: 0 0 6px;
      font-family: Poppins, sans-serif;
      font-size: 1.7rem;
      font-weight: 700;
      background: linear-gradient(135deg, var(--fia-blue), var(--fia-pink));
      -webkit-background-clip: text;
      -webkit-text-fill-color: transparent;
    }

    .cert-sub {
      margin: 0;
      font-size: 0.93rem;
      color: var(--muted);
    }

    /* Floating helper name tag in the hero's top-right corner */
    .helper-tag {
      display: inline-flex;
      align-items: center;
      gap: 8px;
      padding: 8px 14px;
      border-radius: var(--radius-pill);
      background: var(--surface);
      border: 1px solid var(--border);
      box-shadow: var(--shadow-md);
      font-size: 0.83rem;
      flex-shrink: 0;
    }

    .helper-tag .tag-label { color: var(--muted); }
    .helper-tag .tag-name  { font-weight: 700; color: var(--fia-blue); }

    /* ============================================================
       ON-HOLD / QUESTIONED BANNER
       Shown when one or more certifications are flagged by admin.
    ============================================================ */
    .hold-banner {
      margin-bottom: 20px;
      padding: 12px 16px;
      border-radius: var(--radius-md);
      border: 1px solid var(--red-border);
      background: var(--red-bg);
      font-size: 0.9rem;
      color: #991b1b;
    }

    .hold-banner-title {
      display: flex;
      align-items: center;
      gap: 8px;
      font-weight: 700;
      margin-bottom: 4px;
    }

    .hold-banner-icon {
      display: inline-grid;
      place-items: center;
      width: 20px;
      height: 20px;
      border-radius: var(--radius-pill);
      background: #fee2e2;
      font-size: 0.72rem;
      font-weight: 900;
    }

    /* ============================================================
       SECTION BLOCK
       Wraps each content section (status widget, requirements list).
    ============================================================ */
    .section-block { margin-bottom: 28px; }

    .section-heading {
      display: flex;
      align-items: center;
      gap: 10px;
      margin: 0 0 4px;
      font-family: Poppins, sans-serif;
      font-size: 1.08rem;
      font-weight: 600;
      color: var(--ink);
    }

    /* Decorative accent bar before each section heading */
    .section-heading::before {
      content: "";
      display: block;
      width: 22px;
      height: 3px;
      border-radius: var(--radius-pill);
      background: linear-gradient(90deg, var(--fia-pink), var(--fia-teal));
      flex-shrink: 0;
    }

    .section-sub {
      margin: 0 0 12px;
      font-size: 0.9rem;
      color: var(--muted);
    }

    /* ============================================================
       STATUS MODULE GRID
       Auto-fit pills showing each module's certification state.
    ============================================================ */
    .status-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(260px, 1fr));
      gap: 10px;
    }

    .module-pill {
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: 10px;
      padding: 10px 14px;
      border-radius: var(--radius-md);
      border: 1px solid var(--border);
      background: linear-gradient(150deg, var(--surface), var(--surface-raised));
      box-shadow: var(--shadow-sm);
      position: relative;
      overflow: hidden;
    }

    /* Subtle pink wash accent inside module pill */
    .module-pill::before {
      content: "";
      position: absolute;
      inset: 0;
      background: radial-gradient(circle at 0% 0%, rgba(240, 106, 169, 0.08) 0%, transparent 60%);
      pointer-events: none;
    }

    .module-pill-title {
      font-size: 0.88rem;
      font-weight: 600;
      position: relative;
      z-index: 1;
    }

    /* Inline "✓ Verified" badge — hidden unless verified-show class is added from code-behind */
    .verified-check {
      display: none;
      margin-left: 6px;
      font-size: 0.75rem;
      padding: 2px 7px;
      border-radius: var(--radius-pill);
      background: var(--green-bg);
      color: var(--green);
      border: 1px solid var(--green-border);
      font-weight: 700;
    }

    .verified-check.verified-show {
      display: inline-flex;
      align-items: center;
      gap: 3px;
    }

    /* Status badge on module pill — colour driven by CSS class from code-behind */
    .module-pill-status {
      font-size: 0.78rem;
      font-weight: 700;
      padding: 4px 10px;
      border-radius: var(--radius-pill);
      white-space: nowrap;
      position: relative;
      z-index: 1;
    }

    /* Status colour variants */
    .status-certified { background: var(--green-bg);   color: var(--green);        border: 1px solid var(--green-border); }
    .status-notcert   { background: var(--red-bg);     color: var(--red);           border: 1px solid var(--red-border); }
    .status-eligible  { background: var(--blue-bg);    color: var(--blue-status);  border: 1px solid var(--blue-border); }

    /* ============================================================
       REQUIREMENTS CARD LIST
       Detailed per-microcourse breakdown: quiz, teaching, 1:1 help,
       expiry, resources, and action buttons.
    ============================================================ */
    .req-list {
      display: flex;
      flex-direction: column;
      gap: 14px;
    }

    .req-card {
      background: var(--surface);
      border-radius: var(--radius-lg);
      border: 1px solid var(--border);
      padding: 18px 20px 16px;
      box-shadow: var(--shadow-md);
      display: flex;
      flex-direction: column;
      gap: 10px;
      position: relative;
      overflow: hidden;
    }

    /* Left accent bar — pink to blue gradient */
    .req-card::before {
      content: "";
      position: absolute;
      inset-block: 0;
      inset-inline-start: 0;
      width: 5px;
      background: linear-gradient(180deg, var(--fia-pink), var(--fia-blue));
    }

    /* Card header row: title + status pill */
    .req-header {
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: 12px;
    }

    .req-title {
      margin: 0;
      font-family: Poppins, sans-serif;
      font-size: 1rem;
      font-weight: 600;
    }

    /* Header-level status pill (mirrors module pill status) */
    .req-status-pill {
      font-size: 0.78rem;
      font-weight: 700;
      padding: 4px 10px;
      border-radius: var(--radius-pill);
      white-space: nowrap;
    }

    .req-status-certified { background: var(--green-bg); color: var(--green);       border: 1px solid var(--green-border); }
    .req-status-not       { background: var(--red-bg);   color: var(--red);          border: 1px solid var(--red-border); }
    .req-status-eligible  { background: var(--blue-bg);  color: var(--blue-status); border: 1px solid var(--blue-border); }

    .req-desc {
      margin: 0;
      font-size: 0.9rem;
      color: var(--muted);
    }

    /* Four-column grid: Quiz · Teaching · 1:1 Help · Expiry */
    .req-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(210px, 1fr));
      gap: 8px;
    }

    /* Individual requirement sub-card */
    .req-item {
      border-radius: var(--radius-sm);
      border: 1px dashed var(--border);
      padding: 10px 12px;
      font-size: 0.84rem;
      background: var(--surface-raised);
    }

    /* Per-category background tints */
    .req-item-quiz   { background: linear-gradient(150deg, var(--surface), var(--fia-blue-soft)); border-color: rgba(42, 153, 219, 0.30); }
    .req-item-teach  { background: linear-gradient(150deg, var(--surface), var(--fia-teal-soft)); border-color: rgba(69, 195, 179, 0.30); }
    .req-item-help   { background: linear-gradient(150deg, var(--surface), var(--fia-pink-soft)); border-color: rgba(240, 106, 169, 0.35); }
    .req-item-expiry { background: linear-gradient(150deg, var(--surface), rgba(148, 163, 184, 0.10)); border-color: rgba(148, 163, 184, 0.35); }

    .req-item-title {
      font-weight: 700;
      margin-bottom: 4px;
      font-size: 0.83rem;
    }

    /* Category accent colours */
    .req-item-title.quiz   { color: var(--fia-blue); }
    .req-item-title.teach  { color: var(--fia-teal); }
    .req-item-title.help   { color: var(--fia-pink); }
    .req-item-title.expiry { color: #475569; }

    .req-item-row {
      display: flex;
      justify-content: space-between;
      gap: 4px;
    }

    .req-item-label { color: var(--muted); }
    .req-item-value { font-weight: 700; }

    .req-item-status {
      margin-top: 4px;
      font-size: 0.8rem;
      font-weight: 700;
    }

    .req-item-status.met    { color: var(--green); }
    .req-item-status.notmet { color: var(--red);   }

    /* Small admin / on-hold notice inside a requirement card */
    .req-note {
      margin-top: 6px;
      padding: 8px 10px;
      border-radius: var(--radius-sm);
      background: var(--red-bg);
      color: #991b1b;
      font-size: 0.84rem;
    }

    /* Faint rule at the bottom of a req-card (RuleMetaText) */
    .req-meta {
      font-size: 0.8rem;
      color: var(--muted);
      margin-top: 2px;
    }

    /* ============================================================
       RESOURCE ROW & ACTION BUTTONS
    ============================================================ */
    .resource-row {
      display: flex;
      flex-wrap: wrap;
      align-items: center;
      gap: 6px;
      font-size: 0.85rem;
      margin-top: 4px;
    }

    .resource-label { font-weight: 700; color: var(--muted); }

    .resource-link {
      font-weight: 700;
      color: var(--fia-blue);
      text-decoration: none;
    }

    .resource-link:hover { text-decoration: underline; }

    .resource-none { color: var(--subtle); }

    /* Confirm / resubmit action buttons */
    .btn-confirm {
      appearance: none;
      border: none;
      border-radius: var(--radius-pill);
      padding: 8px 16px;
      font-family: Poppins, sans-serif;
      font-size: 0.82rem;
      font-weight: 600;
      cursor: pointer;
      background: linear-gradient(135deg, var(--fia-blue), var(--fia-teal));
      color: #fff;
      box-shadow: 0 6px 16px rgba(42, 153, 219, 0.25);
      margin-top: 8px;
      transition: opacity 0.15s, box-shadow 0.15s;
    }

    .btn-confirm:hover { opacity: 0.9; box-shadow: 0 8px 20px rgba(42, 153, 219, 0.32); }
    .btn-confirm:focus { outline: none; box-shadow: 0 0 0 3px rgba(42, 153, 219, 0.35); }

    /* Resubmit textarea */
    .resubmit-textarea {
      width: 100%;
      margin-top: 8px;
      margin-bottom: 4px;
      border-radius: var(--radius-sm);
      border: 1px solid var(--border);
      padding: 8px 12px;
      font-family: inherit;
      font-size: 0.85rem;
      resize: vertical;
      outline: none;
      transition: border-color 0.15s, box-shadow 0.15s;
    }

    .resubmit-textarea:focus {
      border-color: var(--fia-pink);
      box-shadow: 0 0 0 3px rgba(240, 106, 169, 0.18);
    }

    /* ============================================================
       RESPONSIVE
    ============================================================ */
    @media (max-width: 720px) {
      .cert-hero { flex-direction: column; border-radius: var(--radius-md); }
      .req-grid  { grid-template-columns: 1fr; }
    }
  </style>
</head>

<body>
  <form id="form1" runat="server">
    <div class="wrap">

      <!-- ======================================================
           PAGE HERO
           Title, subtitle, and helper's name tag.
      ====================================================== -->
      <div class="cert-hero">

        <div class="cert-hero-main">
          <div class="eyebrow">
            <span class="eyebrow-dot">FIA</span>
            <span>Helper workspace</span>
          </div>
          <h1 class="cert-title">Cybersecurity Microcourse Certification</h1>
          <p class="cert-sub">
            Track how close you are to being certified in each FIA cybersecurity topic.
            See which modules still need a quiz score, teaching sessions, or 1:1 help,
            plus a detailed breakdown per microcourse.
          </p>
        </div>

        <%-- Helper name tag — bound from code-behind --%>
        <div class="helper-tag">
          <span class="tag-label">Peer Helper:</span>
          <span class="tag-name"><asp:Literal ID="HelperName" runat="server" /></span>
        </div>

      </div>
      <%-- /cert-hero --%>


      <!-- ======================================================
           ON-HOLD BANNER
           Shown by code-behind when one or more module certifications
           have been flagged / put on hold by an admin.
      ====================================================== -->
      <asp:PlaceHolder ID="QuestionedNoticePH" runat="server" Visible="false">
        <div class="hold-banner">
          <div class="hold-banner-title">
            <span class="hold-banner-icon">!</span>
            <span>Some certifications are on hold</span>
          </div>
          <asp:Literal ID="QuestionedNoticeText" runat="server" />
        </div>
      </asp:PlaceHolder>


      <!-- ======================================================
           CERTIFICATION STATUS WIDGET
           Three repeaters for: in-progress · eligible · certified.
           Visibility of each PlaceHolder is controlled server-side.
      ====================================================== -->
      <div class="section-block">
        <h2 class="section-heading">Your module certifications</h2>
        <p class="section-sub">
          Modules still in progress appear first. Eligible means you can teach but aren't
          fully certified yet. Certified means all requirements are met.
        </p>

        <%-- In-progress modules --%>
        <asp:PlaceHolder ID="NotCertifiedPH" runat="server">
          <p class="section-sub" style="margin-top: 2px;">Still in progress:</p>
          <div class="status-grid">
            <asp:Repeater ID="NotCertifiedRepeater" runat="server">
              <ItemTemplate>
                <div class="module-pill">
                  <div class="module-pill-title">
                    <%# Eval("Title") %>
                    <span class="verified-check <%# Eval("VerificationCssClass") %>">✓ Verified</span>
                  </div>
                  <div class="module-pill-status <%# Eval("StatusCssClass") %>">
                    <%# Eval("StatusLabel") %>
                  </div>
                </div>
              </ItemTemplate>
            </asp:Repeater>
          </div>
        </asp:PlaceHolder>

        <%-- Eligible modules (quiz done; teaching / help still needed) --%>
        <asp:PlaceHolder ID="EligiblePH" runat="server" Visible="false">
          <p class="section-sub" style="margin-top: 14px;">Eligible to teach (quiz complete):</p>
          <div class="status-grid">
            <asp:Repeater ID="EligibleRepeater" runat="server">
              <ItemTemplate>
                <div class="module-pill">
                  <div class="module-pill-title">
                    <%# Eval("Title") %>
                    <span class="verified-check <%# Eval("VerificationCssClass") %>">✓ Verified</span>
                  </div>
                  <div class="module-pill-status <%# Eval("StatusCssClass") %>">
                    <%# Eval("StatusLabel") %>
                  </div>
                </div>
              </ItemTemplate>
            </asp:Repeater>
          </div>
        </asp:PlaceHolder>

        <%-- Fully certified modules --%>
        <asp:PlaceHolder ID="CertifiedPH" runat="server" Visible="false">
          <p class="section-sub" style="margin-top: 14px;">Fully certified:</p>
          <div class="status-grid">
            <asp:Repeater ID="CertifiedRepeater" runat="server">
              <ItemTemplate>
                <div class="module-pill">
                  <div class="module-pill-title">
                    <%# Eval("Title") %>
                    <span class="verified-check <%# Eval("VerificationCssClass") %>">✓ Verified</span>
                  </div>
                  <div class="module-pill-status <%# Eval("StatusCssClass") %>">
                    <%# Eval("StatusLabel") %>
                  </div>
                </div>
              </ItemTemplate>
            </asp:Repeater>
          </div>
        </asp:PlaceHolder>

      </div>
      <%-- /section-block (status widget) --%>


      <!-- ======================================================
           MICROCOURSE REQUIREMENTS LIST
           Full detail card for every microcourse: quiz, teaching,
           1:1 help, expiry rules, resources, and action buttons.
      ====================================================== -->
      <div class="section-block">
        <h2 class="section-heading">Microcourse-by-microcourse details</h2>
        <p class="section-sub">
          Each cybersecurity microcourse is listed below with its certification rules.
          Requirements and your current progress are shown side by side.
        </p>

        <div class="req-list">
          <asp:Repeater ID="RequirementsRepeater"
                        runat="server"
                        OnItemCommand="RequirementsRepeater_ItemCommand">
            <ItemTemplate>

              <div class="req-card">

                <%-- Card header: microcourse title + certification status pill --%>
                <div class="req-header">
                  <h3 class="req-title">
                    <%# Eval("Title") %>
                    <span class="verified-check <%# Eval("VerificationCssClass") %>">✓ Verified</span>
                  </h3>
                  <span class="req-status-pill <%# Eval("HeaderStatusCss") %>">
                    <%# Eval("HeaderStatusLabel") %>
                  </span>
                </div>

                <%-- Short description of the microcourse --%>
                <p class="req-desc"><%# Eval("Description") %></p>

                <%-- Four requirement sub-cards in a responsive grid --%>
                <div class="req-grid">

                  <!-- Quiz requirement -->
                  <div class="req-item req-item-quiz">
                    <div class="req-item-title quiz">Quiz</div>
                    <div class="req-item-row">
                      <span class="req-item-label">Required:</span>
                      <span class="req-item-value"><%# Eval("QuizRequirementText") %></span>
                    </div>
                    <div class="req-item-row">
                      <span class="req-item-label">Your score:</span>
                      <span class="req-item-value"><%# Eval("QuizProgressText") %></span>
                    </div>
                    <div class="req-item-status <%# Eval("QuizStatusCss") %>">
                      <%# Eval("QuizStatusText") %>
                    </div>
                  </div>

                  <!-- Teaching sessions requirement -->
                  <div class="req-item req-item-teach">
                    <div class="req-item-title teach">Teaching sessions</div>
                    <div class="req-item-row">
                      <span class="req-item-label">Required:</span>
                      <span class="req-item-value"><%# Eval("TeachingRequirementText") %></span>
                    </div>
                    <div class="req-item-row">
                      <span class="req-item-label">Your progress:</span>
                      <span class="req-item-value"><%# Eval("TeachingProgressText") %></span>
                    </div>
                    <div class="req-item-status <%# Eval("TeachingStatusCss") %>">
                      <%# Eval("TeachingStatusText") %>
                    </div>
                    <%-- Admin on-hold note for teaching — shown when a session is under review --%>
                    <asp:PlaceHolder ID="TeachingOnHoldPH" runat="server"
                                     Visible='<%# (bool)Eval("TeachingOnHold") %>'>
                      <div class="req-note">
                        <strong>Admin note (teaching):</strong>
                        <%# Eval("TeachingReviewNote") %><br />
                        One teaching session for this microcourse is currently on hold.
                      </div>
                    </asp:PlaceHolder>
                  </div>

                  <!-- 1:1 help sessions requirement -->
                  <div class="req-item req-item-help">
                    <div class="req-item-title help">1:1 help sessions</div>
                    <div class="req-item-row">
                      <span class="req-item-label">Required:</span>
                      <span class="req-item-value"><%# Eval("HelpRequirementText") %></span>
                    </div>
                    <div class="req-item-row">
                      <span class="req-item-label">Your progress:</span>
                      <span class="req-item-value"><%# Eval("HelpProgressText") %></span>
                    </div>
                    <div class="req-item-status <%# Eval("HelpStatusCss") %>">
                      <%# Eval("HelpStatusText") %>
                    </div>
                    <%-- Admin on-hold note for 1:1 help — shown when a session is under review --%>
                    <asp:PlaceHolder ID="HelpOnHoldPH" runat="server"
                                     Visible='<%# (bool)Eval("HelpOnHold") %>'>
                      <div class="req-note">
                        <strong>Admin note (1:1 help):</strong>
                        <%# Eval("HelpReviewNote") %><br />
                        One 1:1 help session for this microcourse is currently on hold.
                      </div>
                    </asp:PlaceHolder>
                  </div>

                  <!-- Expiry / validity information -->
                  <div class="req-item req-item-expiry">
                    <div class="req-item-title expiry">Expiry</div>
                    <div class="req-item-row">
                      <span class="req-item-label">Rule:</span>
                      <span class="req-item-value"><%# Eval("ExpiryText") %></span>
                    </div>
                    <div class="req-meta" style="margin-top: 6px;">
                      <%# Eval("ExpiryMetaText") %>
                    </div>
                  </div>

                </div>
                <%-- /req-grid --%>

                <!-- Resources link row -->
                <asp:PlaceHolder ID="ResourceAvailablePH" runat="server"
                                 Visible='<%# (bool)Eval("HasExternalLink") %>'>
                  <div class="resource-row">
                    <span class="resource-label">Resources:</span>
                    <a class="resource-link"
                       href='<%# Eval("ExternalLink") %>'
                       target="_blank" rel="noopener">
                      Open Google Classroom resources ↗
                    </a>
                  </div>
                </asp:PlaceHolder>

                <asp:PlaceHolder ID="ResourceMissingPH" runat="server"
                                 Visible='<%# !(bool)Eval("HasExternalLink") %>'>
                  <div class="resource-row resource-none">
                    No additional resources have been added for this microcourse yet.
                  </div>
                </asp:PlaceHolder>

                <%-- Confirm button — shown when helper can self-attest they've reviewed materials --%>
                <asp:PlaceHolder ID="ConfirmResourcesPH" runat="server"
                                 Visible='<%# (bool)Eval("ShowConfirmButton") %>'>
                  <asp:Button ID="BtnConfirmResources"
                              runat="server"
                              CssClass="btn-confirm"
                              Text="I've reviewed the resources and passed the quiz"
                              CommandName="confirmResources"
                              CommandArgument='<%# Eval("CourseId") %>' />
                </asp:PlaceHolder>

                <%-- Admin note shown when a certification is questioned / on hold --%>
                <asp:PlaceHolder ID="AdminNotePH" runat="server"
                                 Visible='<%# (bool)Eval("ShowAdminNoteForHelper") %>'>
                  <div class="req-note" style="margin-top: 6px;">
                    <strong>Your admin's note:</strong>
                    <%# Eval("AdminNote") %>
                  </div>
                </asp:PlaceHolder>

                <%-- Resubmit panel — shown when module is on hold and helper can re-submit --%>
                <asp:PlaceHolder ID="ResubmitPH" runat="server"
                                 Visible='<%# (bool)Eval("ShowResubmitButton") %>'>
                  <asp:TextBox ID="HelperNoteText"
                               runat="server"
                               TextMode="MultiLine"
                               Rows="3"
                               CssClass="resubmit-textarea"
                               Placeholder="Share a quick note for your admin (e.g. what you fixed or double-checked)."
                               Text='<%# Eval("HelperNote") %>' />
                  <asp:Button ID="BtnResubmit"
                              runat="server"
                              CssClass="btn-confirm"
                              Text="Resubmit for verification"
                              CommandName="resubmitVerification"
                              CommandArgument='<%# Eval("CourseId") %>' />
                </asp:PlaceHolder>

                <%-- Certification rule meta text at the foot of each card --%>
                <div class="req-meta"><%# Eval("RuleMetaText") %></div>

              </div>
              <%-- /req-card --%>

            </ItemTemplate>
          </asp:Repeater>
        </div>
        <%-- /req-list --%>

      </div>
      <%-- /section-block (requirements list) --%>

    </div><%-- /wrap --%>
  </form>
</body>
</html>
