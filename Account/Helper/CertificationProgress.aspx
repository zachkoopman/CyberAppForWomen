<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CertificationProgress.aspx.cs" Inherits="CyberApp_FIA.Helper.CertificationProgress" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
  <title>FIA • Helper Certification Progress</title>
  <meta name="viewport" content="width=device-width, initial-scale=1" />

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
      --status-green:#16a34a;
      --status-red:#dc2626;
      --status-chip-bg:#ecfdf3;
      --status-chip-border:#bbf7d0;
      --status-chip-red-bg:#fef2f2;
      --status-chip-red-border:#fecaca;

      --chip-soft-blue:rgba(42,153,219,0.12);
      --chip-soft-pink:rgba(240,106,169,0.12);
      --chip-soft-teal:rgba(69,195,179,0.12);
    }

    *{ box-sizing:border-box; }

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

    /* Hero */
    .cert-hero{
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
      align-items:flex-start;
      justify-content:space-between;
      gap:18px;
    }

    .cert-hero-main{
      max-width:640px;
    }

    .cert-eyebrow{
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

    .cert-eyebrow-pill{
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

    .cert-title{
      font-family:Poppins, system-ui, sans-serif;
      font-size:1.7rem;
      margin:0 0 6px 0;
      background:linear-gradient(135deg,var(--fia-blue),var(--fia-pink));
      -webkit-background-clip:text;
      color:transparent;
    }

    .cert-sub{
      margin:0;
      font-size:0.95rem;
      color:var(--muted);
    }

    .cert-helper-tag{
      padding:8px 12px;
      border-radius:999px;
      font-size:0.8rem;
      background:#ffffff;
      border:1px solid #e5e7eb;
      display:inline-flex;
      align-items:center;
      gap:8px;
      box-shadow:0 10px 20px rgba(15,23,42,0.12);
      position:relative;
      overflow:hidden;
    }

    .cert-helper-tag::before{
      content:"";
      position:absolute;
      inset:0;
      background:linear-gradient(135deg,rgba(240,106,169,0.18),rgba(42,153,219,0.12));
      opacity:0.35;
      pointer-events:none;
    }

    .cert-helper-tag span{
      position:relative;
      z-index:1;
    }

    .cert-helper-tag span.name{
      font-weight:600;
      color:var(--fia-blue);
    }

    /* Section headings */
    .section-block{
      margin-bottom:24px;
    }

    .section-heading{
      font-family:Poppins, system-ui, sans-serif;
      font-size:1.05rem;
      margin:0 0 4px 0;
      display:inline-flex;
      align-items:center;
      gap:8px;
    }

    .section-heading::before{
      content:"";
      width:20px;
      height:3px;
      border-radius:999px;
      background:linear-gradient(135deg,var(--fia-pink),var(--fia-teal));
    }

    .section-sub{
      margin:0 0 10px 0;
      font-size:0.9rem;
      color:var(--muted);
    }

    /* Status widget */
    .status-grid{
      display:grid;
      grid-template-columns:repeat(auto-fit, minmax(260px, 1fr));
      gap:10px;
    }

    .module-pill{
      border-radius:16px;
      padding:10px 12px;
      background:linear-gradient(135deg,#ffffff,#f9fbff);
      border:1px solid #e5e7eb;
      display:flex;
      align-items:center;
      justify-content:space-between;
      gap:8px;
      box-shadow:0 10px 24px rgba(15,23,42,0.06);
      position:relative;
      overflow:hidden;
    }

    .module-pill::before{
      content:"";
      position:absolute;
      inset:0;
      background:radial-gradient(circle at 0 0,rgba(240,106,169,0.1),transparent 60%);
      opacity:0.75;
      pointer-events:none;
    }

    .module-pill-title{
      font-size:0.9rem;
      font-weight:600;
      position:relative;
      z-index:1;
    }

    .module-pill-status{
      font-size:0.8rem;
      padding:4px 9px;
      border-radius:999px;
      font-weight:600;
      white-space:nowrap;
      position:relative;
      z-index:1;
    }

    .status-certified{
      background:var(--status-chip-bg);
      color:var(--status-green);
      border:1px solid var(--status-chip-border);
    }

    .status-notcert{
      background:var(--status-chip-red-bg);
      color:var(--status-red);
      border:1px solid var(--status-chip-red-border);
    }

    /* NEW: Eligible status (blue) */
    .status-eligible{
      background:rgba(219,234,254,1); /* blue-100 */
      color:#1d4ed8;
      border:1px solid rgba(59,130,246,0.7);
    }

    .cert-toggle{
      margin-top:10px;
      border-radius:16px;
      background:#f9fafb;
      border:1px solid #e5e7eb;
      padding:8px 10px 10px;
    }

    .cert-toggle > summary{
      list-style:none;
      cursor:pointer;
      font-size:0.9rem;
      font-weight:600;
      display:flex;
      align-items:center;
      justify-content:space-between;
      gap:8px;
    }

    .cert-toggle > summary::marker,
    .cert-toggle > summary::-webkit-details-marker{
      display:none;
    }

    .cert-toggle summary span.chevron{
      font-size:0.9rem;
      opacity:0.8;
    }

    .cert-toggle[open] summary span.chevron{
      transform:rotate(90deg);
    }

    /* Requirements cards */
    .req-list{
      display:flex;
      flex-direction:column;
      gap:12px;
    }

    .req-card{
      background:var(--card-bg);
      border-radius:18px;
      border:1px solid var(--card-border);
      padding:16px 16px 14px;
      box-shadow:0 14px 30px rgba(15,23,42,0.05);
      display:flex;
      flex-direction:column;
      gap:8px;
      position:relative;
      overflow:hidden;
    }

    .req-card::before{
      content:"";
      position:absolute;
      inset-inline-start:0;
      top:0;
      bottom:0;
      width:5px;
      background:linear-gradient(180deg,var(--fia-pink),var(--fia-blue));
      opacity:0.65;
    }

    .req-header{
      display:flex;
      align-items:center;
      justify-content:space-between;
      gap:10px;
      position:relative;
      z-index:1;
    }

    .req-title{
      font-family:Poppins, system-ui, sans-serif;
      font-size:0.98rem;
      margin:0;
    }

    .req-status-pill{
      font-size:0.8rem;
      padding:4px 9px;
      border-radius:999px;
      font-weight:600;
      white-space:nowrap;
    }

    .req-status-certified{
      background:var(--status-chip-bg);
      color:var(--status-green);
      border:1px solid var(--status-chip-border);
    }

    .req-status-not{
      background:var(--status-chip-red-bg);
      color:var(--status-red);
      border:1px solid var(--status-chip-red-border);
    }

    /* NEW: Eligible header pill */
    .req-status-eligible{
      background:rgba(219,234,254,1);
      color:#1d4ed8;
      border:1px solid rgba(59,130,246,0.7);
    }

    .req-desc{
      margin:0;
      font-size:0.9rem;
      color:var(--muted);
      position:relative;
      z-index:1;
    }

    .req-grid{
      display:grid;
      grid-template-columns:repeat(auto-fit, minmax(220px, 1fr));
      gap:8px;
      margin-top:4px;
      position:relative;
      z-index:1;
    }

    .req-item{
      border-radius:12px;
      border:1px dashed #e5e7eb;
      padding:8px 10px;
      font-size:0.85rem;
      background:#f9fafb;
    }

    .req-item-quiz{
      background:linear-gradient(135deg,#ffffff,var(--chip-soft-blue));
      border-color:rgba(42,153,219,0.3);
    }

    .req-item-teach{
      background:linear-gradient(135deg,#ffffff,var(--chip-soft-teal));
      border-color:rgba(69,195,179,0.3);
    }

    .req-item-help{
      background:linear-gradient(135deg,#ffffff,var(--chip-soft-pink));
      border-color:rgba(240,106,169,0.35);
    }

    .req-item-expiry{
      background:linear-gradient(135deg,#ffffff,rgba(148,163,184,0.12));
      border-color:rgba(148,163,184,0.35);
    }

    .req-item-title{
      font-weight:600;
      margin-bottom:2px;
    }

    .req-item-title.quiz{ color:var(--fia-blue); }
    .req-item-title.teach{ color:var(--fia-teal); }
    .req-item-title.help{ color:var(--fia-pink); }
    .req-item-title.expiry{ color:#475569; }

    .req-item-row{
      display:flex;
      justify-content:space-between;
      gap:4px;
    }

    .req-item-label{
      color:#6b7280;
    }

    .req-item-value{
      font-weight:600;
    }

    .req-item-status{
      margin-top:3px;
      font-size:0.8rem;
      font-weight:600;
    }

    .req-item-status.met{
      color:var(--status-green);
    }

    .req-item-status.notmet{
      color:var(--status-red);
    }

    .req-meta{
      font-size:0.8rem;
      color:var(--muted);
      margin-top:4px;
      position:relative;
      z-index:1;
    }

    /* Simple resource row styles */
    .resource-row{
      margin-top:8px;
      font-size:0.85rem;
      display:flex;
      flex-wrap:wrap;
      align-items:center;
      gap:6px;
    }

    .resource-label{
      font-weight:600;
      color:#4b5563;
    }

    .resource-link{
      text-decoration:none;
      font-weight:600;
      color:#2563eb;
    }

    .resource-link:hover{
      text-decoration:underline;
    }

    .resource-none{
      color:var(--muted);
    }

    .resource-confirm-btn{
      margin-top:8px;
      border:none;
      border-radius:999px;
      padding:8px 14px;
      font-size:0.8rem;
      font-weight:600;
      cursor:pointer;
      background:linear-gradient(135deg,var(--fia-blue),var(--fia-teal));
      color:#ffffff;
    }

    .resource-confirm-btn:hover{
      opacity:0.95;
    }

    @media (max-width:720px){
      .cert-hero{
        flex-direction:column;
      }
    }
  </style>
</head>

<body>
  <form id="form1" runat="server">
    <div class="wrap">

      <!-- Hero -->
      <div class="cert-hero">
        <div class="cert-hero-main">
          <div class="cert-eyebrow">
            <span class="cert-eyebrow-pill">FIA</span>
            <span>Helper workspace</span>
          </div>
          <h1 class="cert-title">Cybersecurity Microcourse Certification Progress</h1>
          <p class="cert-sub">
            Track how close you are to being certified in each FIA cybersecurity topic.
            You’ll see which modules still need a quiz score, teaching sessions, or 1:1 help,
            plus a detailed breakdown for every microcourse.
          </p>
        </div>
        <div>
          <div class="cert-helper-tag">
            <span>Peer Helper:</span>
            <span class="name"><asp:Literal ID="HelperName" runat="server" /></span>
          </div>
        </div>
      </div>

      <!-- Certification status widget -->
      <div class="section-block">
        <h2 class="section-heading">Your module certifications</h2>
        <p class="section-sub">
          Modules you still need to finish are shown first. Eligible and certified modules let you see what you can teach and where you’re fully certified.
        </p>

        <!-- Not certified modules grid -->
        <asp:PlaceHolder ID="NotCertifiedPH" runat="server">
          <p class="section-sub" style="margin-top:4px;">
            Still in progress:
          </p>
          <div class="status-grid">
            <asp:Repeater ID="NotCertifiedRepeater" runat="server">
              <ItemTemplate>
                <div class="module-pill">
                  <div class="module-pill-title"><%# Eval("Title") %></div>
                  <div class="module-pill-status <%# Eval("StatusCssClass") %>">
                    <%# Eval("StatusLabel") %>
                  </div>
                </div>
              </ItemTemplate>
            </asp:Repeater>
          </div>
        </asp:PlaceHolder>

        <!-- NEW: Eligible modules grid -->
        <asp:PlaceHolder ID="EligiblePH" runat="server" Visible="false">
          <p class="section-sub" style="margin-top:12px;">
            Eligible to teach (quiz complete, still working toward full certification):
          </p>
          <div class="status-grid">
            <asp:Repeater ID="EligibleRepeater" runat="server">
              <ItemTemplate>
                <div class="module-pill">
                  <div class="module-pill-title"><%# Eval("Title") %></div>
                  <div class="module-pill-status <%# Eval("StatusCssClass") %>">
                    <%# Eval("StatusLabel") %>
                  </div>
                </div>
              </ItemTemplate>
            </asp:Repeater>
          </div>
        </asp:PlaceHolder>

        <!-- Certified modules grid -->
        <asp:PlaceHolder ID="CertifiedPH" runat="server" Visible="false">
          <p class="section-sub" style="margin-top:12px;">
            Fully certified modules:
          </p>
          <div class="status-grid">
            <asp:Repeater ID="CertifiedRepeater" runat="server">
              <ItemTemplate>
                <div class="module-pill">
                  <div class="module-pill-title"><%# Eval("Title") %></div>
                  <div class="module-pill-status <%# Eval("StatusCssClass") %>">
                    <%# Eval("StatusLabel") %>
                  </div>
                </div>
              </ItemTemplate>
            </asp:Repeater>
          </div>
        </asp:PlaceHolder>

      </div>

      <!-- Requirements list -->
      <div class="section-block">
        <h2 class="section-heading">Microcourse-by-microcourse details</h2>
        <p class="section-sub">
          Each cybersecurity microcourse is listed below with its certification rule. You’ll see
          the requirements and your current progress side by side.
        </p>

        <div class="req-list">
          <asp:Repeater ID="RequirementsRepeater" runat="server" OnItemCommand="RequirementsRepeater_ItemCommand">
            <ItemTemplate>
              <div class="req-card">
                <div class="req-header">
                  <h3 class="req-title"><%# Eval("Title") %></h3>
                  <span class="req-status-pill <%# Eval("HeaderStatusCss") %>">
                    <%# Eval("HeaderStatusLabel") %>
                  </span>
                </div>
                <p class="req-desc">
                  <%# Eval("Description") %>
                </p>

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
                  </div>

                  <!-- Expiry info -->
                  <div class="req-item req-item-expiry">
                    <div class="req-item-title expiry">Expiry</div>
                    <div class="req-item-row">
                      <span class="req-item-label">Rule:</span>
                      <span class="req-item-value"><%# Eval("ExpiryText") %></span>
                    </div>
                    <div class="req-meta">
                      <%# Eval("ExpiryMetaText") %>
                    </div>
                  </div>
                </div>

                <!-- Resources -->
                <asp:PlaceHolder ID="ResourceAvailablePH" runat="server" Visible='<%# (bool)Eval("HasExternalLink") %>'>
                  <div class="resource-row">
                    <span class="resource-label">Resources:</span>
                    <a class="resource-link"
                       href='<%# Eval("ExternalLink") %>' target="_blank">
                      Open Google Classroom Resources
                    </a>
                  </div>
                </asp:PlaceHolder>

                <asp:PlaceHolder ID="ResourceMissingPH" runat="server" Visible='<%# !(bool)Eval("HasExternalLink") %>'>
                  <div class="resource-row resource-none">
                    No additional videos or readings have been added yet for this microcourse.
                  </div>
                </asp:PlaceHolder>

                <asp:Button
                  ID="BtnConfirmResources"
                  runat="server"
                  CssClass="resource-confirm-btn"
                  Text="I’ve reviewed the resources and passed the quiz"
                  CommandName="confirmResources"
                  CommandArgument='<%# Eval("CourseId") %>' />

                <!-- IMPORTANT: req-meta stays inside .req-card -->
                <div class="req-meta">
                  <%# Eval("RuleMetaText") %>
                </div>
              </div>
            </ItemTemplate>
          </asp:Repeater>
        </div>
      </div>

    </div>
  </form>
</body>
</html>





