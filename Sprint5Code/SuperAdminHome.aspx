<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="SuperAdminHome.aspx.cs" Inherits="CyberApp_FIA.Account.SuperAdminHome" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
  <title>FIA • Super Admin Home</title>
  <meta name="viewport" content="width=device-width, initial-scale=1" />

  <!-- FIA fonts -->
  <link href="https://fonts.googleapis.com/css2?family=Lato:wght@300;400&family=Poppins:wght@600&display=swap" rel="stylesheet" />

  <style>
    :root {
      --fia-pink:#f06aa9;
      --fia-blue:#2a99db;
      --fia-teal:#45c3b3;
      --ink:#111827;
      --muted:#6b7280;
      --border:#e5e7eb;
      --surface:#ffffff;
      --bg:#f3f4f6;
      --ring:rgba(42,153,219,.25);
    }

    * { box-sizing:border-box; }

    body {
      margin:0;
      font-family:'Lato',sans-serif;
      color:var(--ink);
      background:
        radial-gradient(circle at top left, rgba(240,106,169,.12), transparent 55%),
        radial-gradient(circle at bottom right, rgba(69,195,179,.12), transparent 55%),
        #f9fafb;
    }

    .page {
      min-height:100vh;
      max-width:1120px;
      margin:0 auto;
      padding:24px 16px 40px;
    }

    .topbar {
      display:flex;
      justify-content:space-between;
      align-items:center;
      gap:12px;
      margin-bottom:18px;
    }

    .brand {
      display:flex;
      align-items:center;
      gap:10px;
    }

    .logo-pill {
      width:40px;
      height:40px;
      border-radius:14px;
      background:linear-gradient(135deg,var(--fia-pink),var(--fia-blue));
      display:grid;
      place-items:center;
      color:#fff;
      font-family:'Poppins',sans-serif;
      font-weight:600;
      font-size:0.9rem;
      box-shadow:0 10px 30px rgba(15,23,42,.16);
    }

    .brand-text h1 {
      font-family:'Poppins',sans-serif;
      font-size:1.4rem;
      margin:0;
    }

    .brand-text p {
      margin:2px 0 0;
      font-size:0.85rem;
      color:var(--muted);
    }

    .logout {
      display:flex;
      align-items:center;
      gap:8px;
      font-size:0.85rem;
      color:var(--muted);
    }

    .btn {
      border-radius:999px;
      border:1px solid transparent;
      padding:8px 16px;
      font-size:0.85rem;
      font-weight:500;
      cursor:pointer;
      background:#fff;
      color:var(--ink);
      transition:background .15s ease, box-shadow .15s ease, transform .05s ease;
      font-family:'Lato',sans-serif;
    }

    .btn.primary {
      background:linear-gradient(135deg,var(--fia-pink),var(--fia-blue));
      color:#fff;
      box-shadow:0 10px 24px rgba(42,153,219,.35);
      border:none;
    }

    .btn.link {
      background:transparent;
      color:var(--fia-blue);
      border:1px dashed rgba(148,163,184,.7);
    }

    .btn:hover {
      transform:translateY(-1px);
      box-shadow:0 10px 28px rgba(15,23,42,.14);
    }

    .btn:focus {
      outline:none;
      box-shadow:0 0 0 4px var(--ring);
    }

    .layout {
      display:grid;
      grid-template-columns:minmax(0,2fr) minmax(0,1.2fr);
      gap:18px;
    }

    @media (max-width:960px) {
      .layout {
        grid-template-columns:minmax(0,1fr);
      }
    }

    .card {
      background:var(--surface);
      border-radius:20px;
      border:1px solid var(--border);
      box-shadow:0 18px 45px rgba(15,23,42,.06);
      padding:18px 18px 20px;
    }

    .card-header {
      display:flex;
      justify-content:space-between;
      align-items:center;
      margin-bottom:12px;
    }

    .card-title {
      font-family:'Poppins',sans-serif;
      font-size:1rem;
      margin:0;
    }

    .chip {
      display:inline-flex;
      align-items:center;
      gap:6px;
      padding:4px 10px;
      border-radius:999px;
      background:rgba(42,153,219,.06);
      color:var(--muted);
      font-size:0.75rem;
    }

    .chip-dot {
      width:7px;
      height:7px;
      border-radius:999px;
      background:var(--fia-blue);
    }

    .field-grid {
      display:grid;
      grid-template-columns:repeat(2,minmax(0,1fr));
      gap:12px 14px;
      margin-top:6px;
    }

    @media (max-width:720px) {
      .field-grid {
        grid-template-columns:minmax(0,1fr);
      }
    }

    .field {
      display:flex;
      flex-direction:column;
      gap:4px;
      font-size:0.85rem;
    }

    label {
      font-weight:600;
      font-family:'Poppins',sans-serif;
      font-size:0.85rem;
    }

    input[type=text],
    input[type=url],
    textarea,
    select {
      font-family:'Lato',sans-serif;
      font-size:0.9rem;
      border-radius:12px;
      border:1px solid var(--border);
      padding:9px 12px;
      background:#fff;
      transition:border-color .15s ease, box-shadow .15s ease, background .15s ease;
      width:100%;
    }

    textarea {
      min-height:80px;
      resize:vertical;
    }

    input:focus,
    textarea:focus,
    select:focus {
      outline:none;
      border-color:var(--fia-blue);
      box-shadow:0 0 0 3px var(--ring);
      background:#f9fafb;
    }

    .status-row {
      margin-top:14px;
      display:flex;
      justify-content:space-between;
      align-items:center;
      gap:10px;
      flex-wrap:wrap;
    }

    .status-text {
      font-size:0.8rem;
      color:var(--muted);
    }

    .status-text.error {
      color:#b91c1c;
    }

    .rules-wrap {
      margin-top:12px;
      padding:10px 10px 8px;
      border-radius:16px;
      background:linear-gradient(120deg,rgba(240,106,169,.06),rgba(69,195,179,.04));
      border:1px dashed rgba(148,163,184,.6);
      font-size:0.85rem;
    }

    .rules-label {
      display:flex;
      justify-content:space-between;
      align-items:center;
      margin-bottom:6px;
      font-family:'Poppins',sans-serif;
      font-size:0.85rem;
    }

    .cbgrid {
      display:flex;
      flex-wrap:wrap;
      gap:6px 12px;
      font-size:0.8rem;
    }

    .cbgrid input[type=checkbox] {
      margin-right:4px;
    }

    .micro-hint {
      font-size:0.78rem;
      color:var(--muted);
      margin-top:2px;
    }

    .secondary-card {
      display:flex;
      flex-direction:column;
      gap:10px;
      font-size:0.88rem;
    }

    .secondary-section-title {
      font-family:'Poppins',sans-serif;
      font-size:0.9rem;
      margin:0 0 4px;
    }

    .secondary-list {
      list-style:none;
      padding:0;
      margin:0;
      display:flex;
      flex-direction:column;
      gap:8px;
    }

    .secondary-list li {
      display:flex;
      justify-content:space-between;
      align-items:center;
      gap:8px;
      padding:6px 8px;
      border-radius:12px;
      background:#f9fafb;
      border:1px dashed rgba(148,163,184,.6);
    }

    .tag-pill {
      font-size:0.8rem;
      padding:2px 8px;
      border-radius:999px;
      background:rgba(240,106,169,.06);
      color:var(--muted);
    }
  </style>
</head>
<body>
  <form id="form1" runat="server">
    <div class="page">
      <!-- Top bar -->
      <div class="topbar">
        <div class="brand">
          <div class="logo-pill">FIA</div>
          <div class="brand-text">
            <h1>Super Admin workspace</h1>
            <p>Design and manage FIA microcourses for every campus.</p>
          </div>
        </div>

        <div class="logout">
          <span>Welcome, <asp:Literal ID="WelcomeName" runat="server" />.</span>
          <asp:Button ID="BtnLogout"
                      runat="server"
                      Text="Sign out"
                      CssClass="btn link"
                      OnClick="BtnLogout_Click" />
        </div>
      </div>

      <div class="layout">
        <!-- Left: create / edit microcourse -->
        <div class="card">
          <div class="card-header">
            <h2 class="card-title">Create or edit microcourse</h2>
            <div class="chip">
              <span class="chip-dot"></span>
              <span>Changes sync to the FIA catalog</span>
            </div>
          </div>

          <div class="field-grid">
            <div class="field">
              <label for="Title">Title</label>
              <asp:TextBox ID="Title" runat="server" />
              <div class="micro-hint">Example: “Safer Social Media Basics”.</div>
            </div>

            <div class="field">
              <label for="Duration">Duration (minutes)</label>
              <asp:TextBox ID="Duration" runat="server" />
              <div class="micro-hint">Approximate live session time.</div>
            </div>

            <div class="field" style="grid-column:1 / -1;">
              <label for="Summary">Short description</label>
              <asp:TextBox ID="Summary" runat="server" TextMode="MultiLine" />
              <div class="micro-hint">One clear sentence about what participants will learn.</div>
            </div>

            <div class="field">
              <label for="ExternalLink">Resource link (optional)</label>
              <asp:TextBox ID="ExternalLink" runat="server" TextMode="Url" />
              <div class="micro-hint">Slides, FIA doc, or course landing page.</div>
            </div>

            <div class="field">
              <label for="Tags">Tags</label>
              <asp:TextBox ID="Tags" runat="server" />
              <div class="micro-hint">Comma-separated keywords (e.g., “passwords, consent, social media”).</div>
            </div>

            <div class="field">
              <label for="Status">Status</label>
              <asp:DropDownList ID="Status" runat="server">
                <!-- Items are bound in code-behind; keep defaults light here -->
                <asp:ListItem Text="Draft" Value="Draft"></asp:ListItem>
                <asp:ListItem Text="Published" Value="Published"></asp:ListItem>
                <asp:ListItem Text="Archived" Value="Archived"></asp:ListItem>
              </asp:DropDownList>
              <div class="micro-hint">Draft items stay hidden from participants.</div>
            </div>
          </div>

          <!-- Rules checkboxes -->
          <div class="rules-wrap">
            <div class="rules-label">
              <span>Certification rules</span>
              <a href="<%: ResolveUrl("~/Account/SuperAdmin/CertificationRules.aspx") %>" style="font-size:0.78rem;color:var(--fia-blue);text-decoration:none;">
                Configure rules
              </a>
            </div>
            <asp:CheckBoxList ID="RulesList"
                              runat="server"
                              CssClass="cbgrid"
                              RepeatLayout="Flow" />
            <div class="micro-hint">
              Attach one or more rules (e.g., “Quiz + score ≥ 80%”) to this course.
            </div>
          </div>

          <!-- Actions -->
          <div class="status-row">
            <div style="display:flex;gap:8px;flex-wrap:wrap;">
              <asp:Button ID="BtnSaveMicrocourse"
                          runat="server"
                          Text="Save microcourse"
                          CssClass="btn primary"
                          OnClick="BtnSaveMicrocourse_Click" />
              <asp:Button ID="BtnClear"
                          runat="server"
                          Text="Clear"
                          CssClass="btn link"
                          OnClick="BtnClear_Click"
                          CausesValidation="false" />
            </div>

            <asp:Label ID="FormMessage"
                       runat="server"
                       EnableViewState="false"
                       CssClass="status-text" />
          </div>
        </div>

        <!-- Right: quick overview / helper text -->
        <div class="card secondary-card">
          <div>
            <h3 class="secondary-section-title">How this page fits FIA</h3>
            <p style="margin:4px 0 0;color:var(--muted);">
              Super Admins define the core FIA learning catalog. Once saved, courses appear for
              University Admins and Helpers to schedule and teach, while audit and certification
              rules keep progress trustworthy.
            </p>
          </div>

          <div>
            <h3 class="secondary-section-title">At a glance</h3>
            <ul class="secondary-list">
              <li>
                <span>Draft vs. Published</span>
                <span class="tag-pill">Visibility control</span>
              </li>
              <li>
                <span>Certification rules</span>
                <span class="tag-pill">Trusted progress</span>
              </li>
              <li>
                <span>Linked resources</span>
                <span class="tag-pill">Consistent content</span>
              </li>
            </ul>
          </div>

          <div>
            <h3 class="secondary-section-title">Next steps</h3>
            <p style="margin:4px 0 0;color:var(--muted);">
              After publishing, use the University Admin tools to create events from these courses,
              and watch the audit log to confirm that catalog changes and helper certifications are
              tracked correctly.
            </p>
          </div>
        </div>
      </div>
    </div>
  </form>
</body>
</html>
