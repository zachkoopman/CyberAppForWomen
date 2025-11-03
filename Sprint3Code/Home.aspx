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

      /* card accents */
      --rail-grad:linear-gradient(180deg, rgba(42,153,219,.85), rgba(240,106,169,.85));
      --note-blue:#eaf5ff;
      --note-pink:#fff3f9;
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
      display:flex; align-items:center; gap:10px; flex-wrap:wrap;
    }
    .divider{
      height:1px; border:none; margin:24px 0 8px 0;
      background:linear-gradient(90deg, rgba(42,153,219,.0), rgba(42,153,219,.25), rgba(42,153,219,.0));
    }

    /* ---------- Sessions Grid ---------- */
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

    /* ---------- Card (stacked layout) ---------- */
    .card{
      position:relative;
      background:var(--card-bg);
      border:1px solid var(--card-border);
      border-radius:20px;
      box-shadow:0 12px 36px rgba(42,153,219,.08);
      padding:0;
      display:flex; flex-direction:column; gap:0;
      transition: transform .12s ease, box-shadow .12s ease, border-color .12s ease;
      width:100%;
      overflow:hidden;
    }
    .card::before{
      content:"";
      position:absolute; inset:0 0 0 auto;
      width:6px; left:0; right:auto;
      background:var(--rail-grad);
      opacity:.9;
    }
    .card:hover, .card:focus-within{
      transform: translateY(-2px);
      border-color:#d4e6f7;
      box-shadow:0 16px 40px rgba(42,153,219,.14);
    }

    .card-sec{ padding:14px 16px 12px 18px; }
    .card-sec + .card-sec{ border-top:1px solid #f0f3f9; }

    .title{ font-family:Poppins; font-weight:600; margin:0; font-size:1.06rem; line-height:1.35; }

    .line{ display:flex; align-items:center; justify-content:flex-start; gap:8px; font-size:.95rem; color:var(--muted); }
    .label{ font-weight:700; color:var(--fia-blue); font-size:.9rem; min-width:84px; }
    .value{ color:var(--ink); }

    .chip{
      display:inline-flex; align-items:center; gap:6px; padding:6px 10px; border-radius:999px; font-size:.85rem;
      background:#fff; border:1px dashed #e7edf7;
    }
    .helper-chip{ border:1px solid rgba(240,106,169,.25); background:linear-gradient(180deg,#fff,#fff7fb); }
    .dot{ width:8px; height:8px; border-radius:999px; background:var(--fia-pink); display:inline-block; }

    /* Force helper name to FIA blue everywhere */
    .helper-chip strong { 
    color: var(--fia-blue) !important;
    }

    .info-bar{
      display:flex; align-items:center; justify-content:space-between; gap:10px;
      padding:10px 12px; border-radius:12px; background:linear-gradient(180deg,#f7fbff,#ffffff);
      border:1px solid #d9e9f6; color:#0f3d5e; font-size:.92rem;
    }
    .info-bar.ok{ background:linear-gradient(180deg,#e6fbf7,#ffffff); border-color:#bfeee6; color:#0a5b4e; }
    .info-bar.warn{ background:var(--note-pink); border-color:#ffd1e5; color:#7a103a; }
    .remain{
      font-family:Poppins; font-weight:600;
      padding:6px 10px; border-radius:999px;
      background:linear-gradient(135deg,#e6fbf7,#d6f5ef);
      border:1px solid rgba(69,195,179,.35); color:#084c61;
    }
    .remain.low{ background:linear-gradient(135deg,#fff2f7,#ffe7f0); color:#7a103a; border-color:rgba(240,106,169,.45); }

    .subtle{ color:var(--muted); font-size:.92rem; }
    .cta-row{ display:flex; gap:8px; }

    .btn{
      appearance:none; border:none; cursor:pointer;
      padding:10px 12px; border-radius:12px; font-weight:600; font-family:Poppins;
      box-shadow:0 2px 0 rgba(0,0,0,.04);
    }
    .btn-primary{ background:linear-gradient(135deg,var(--fia-blue),#6bc1f1); color:#fff; }
    .btn-ghost{ background:#fff; color:var(--fia-blue); border:1px solid #d9e9f6; }
    .btn:focus{ outline:none; box-shadow:0 0 0 4px var(--ring); }

    .pill-pink{
      background: linear-gradient(135deg, var(--fia-pink), #ff86bf) !important;
      border-color: rgba(240,106,169,.55) !important;
      color: #fff !important;
    }

    /* NEW: Pink action button */
    .btn-pink{
      background: linear-gradient(135deg, var(--fia-pink), #ff86bf);
      color:#fff;
      border:1px solid rgba(240,106,169,.55);
    }

    /* ---------- Prereq / Conflict ---------- */
    .blocked-note{
      background:var(--note-pink); border:1px solid #ffd1e5; color:#7a103a;
      border-radius:12px; padding:10px 12px; font-size:.92rem;
    }
    .conflict-note{
      background:var(--note-blue);
      border:1px solid #d9e9f6;
      color:#0f3d5e;
      border-radius:12px; padding:10px 12px; font-size:.92rem;
    }
    .conflict-actions{ display:flex; gap:8px; margin-top:8px; }
    .card.is-blocked{ opacity:.98; }

    .glow-alt{
      box-shadow:0 0 0 3px rgba(42,153,219,.28), 0 0 20px rgba(42,153,219,.38);
      transition:box-shadow .15s ease, transform .12s ease;
    }
    .glow-alt:hover{ transform:translateY(-1px); }

    /* === Filters card (visuals only) === */
    .filters-card{
      margin:18px 0 8px 0;
      padding:16px;
      background:#fff;
      border:1px solid var(--card-border);
      border-radius:20px;
      box-shadow:0 10px 26px rgba(42,153,219,.06), 0 0 0 2px rgba(69,195,179,.06) inset;
    }
    .filters-title{
      font-family:Poppins; font-weight:600; font-size:1.05rem; margin:0 0 12px 2px;
      color: var(--fia-blue);
    }
    .filters-grid{ display:grid; grid-template-columns: 1.1fr 1.1fr auto auto; gap:12px; align-items:end; }
    .row-full{ grid-column: 1 / -1; }
    @media (max-width: 980px){ .filters-grid{ grid-template-columns: 1fr 1fr; } }
    @media (max-width: 560px){ .filters-grid{ grid-template-columns: 1fr; } }
    .field{ display:flex; flex-direction:column; gap:6px; }
    .field label{ color:var(--fia-blue); font-size:.9rem; padding-left:6px; font-weight:600; }
    .input{
      width:100%; padding:10px 12px;
      border:1px solid #d9e9f6; border-radius:999px;
      background:linear-gradient(180deg,#f0f7fd,#ffffff);
      color:var(--ink); font-size:0.95rem; outline:none;
    }
    .input:focus{ border-color:#bfe6ff; box-shadow:0 0 0 4px var(--ring); background:#fff; }
    .tags-wrap{
      display:flex; flex-wrap:wrap; gap:8px; padding:10px;
      border-radius:16px; border:1px solid #d7efe9;
      background:linear-gradient(180deg,#e8fbf6,#ffffff);
    }
    .tags-wrap span{
      display:inline-flex; align-items:center; gap:6px;
      padding:6px 10px; border-radius:999px; background:#fff;
      border:1px solid rgba(69,195,179,.35); font-size:.9rem;
    }
    .filters-actions{ display:flex; gap:10px; }
    .filters-actions .btn{ min-width:108px; }

    .hint{ margin-top:6px; font-size:.85rem; color:var(--muted); padding-left:8px; }

    /* ---------- Highlight Alternatives: picker ---------- */
    .alt-picker-wrap{ position:relative; display:inline-flex; align-items:center; gap:6px; }
    .alt-caret{ padding:6px 10px; border-radius:999px; border:1px solid #d9e9f6; background:#fff; color:var(--fia-blue); cursor:pointer; }
    .alt-picker{
      position:absolute; top:40px; right:0;
      min-width:280px; max-width:340px;
      background:#fff; border:1px solid #e6eef8; border-radius:16px;
      box-shadow:0 18px 40px rgba(42,153,219,.18);
      padding:10px; z-index:50;
    }
    .alt-title{ font-family:Poppins; font-size:.95rem; color:var(--fia-blue); margin:4px 8px 8px 8px; }
    .alt-list{ display:flex; flex-direction:column; gap:8px; max-height:260px; overflow:auto; padding:0 6px 6px 6px; }
    .alt-option{
      display:flex; justify-content:space-between; align-items:center; gap:10px;
      padding:10px 12px; border-radius:12px; border:1px solid #e6eef8; background:linear-gradient(180deg,#f7fbff,#ffffff);
      cursor:pointer; font-size:.92rem;
    }
    .alt-option:hover{ box-shadow:0 0 0 4px var(--ring); border-color:#cfe8ff; }
    .alt-actions{ display:flex; gap:8px; margin-top:8px; justify-content:flex-end; }
  </style>

  <script type="text/javascript">
      // ---- Helper: gather distinct missing prereqs from blocked cards ----
      function getBlockedPrereqs() {
          var blocked = Array.prototype.slice.call(document.querySelectorAll('.card.is-blocked'));
          var map = {};
          blocked.forEach(function (card) {
              var id = (card.getAttribute('data-missing-prereq-id') || '').trim();
              var title = (card.getAttribute('data-missing-prereq-title') || '').trim();
              if (id) {
                  if (!map[id]) map[id] = { id: id, title: title || id };
              }
          });
          return Object.keys(map).map(function (k) { return map[k]; });
      }

      // ---- Existing: highlight all alternatives for all blocked prereqs ----
      function highlightAlternatives() {
          try {
              var needed = {};
              getBlockedPrereqs().forEach(function (p) { needed[p.id] = true; });

              var cards = Array.prototype.slice.call(document.querySelectorAll('.card'));
              cards.forEach(function (c) {
                  var mcid = (c.getAttribute('data-microcourse-id') || '').trim();
                  if (needed[mcid]) c.classList.add('glow-alt'); else c.classList.remove('glow-alt');
              });

              if (Object.keys(needed).length === 0) {
                  var blocked = Array.prototype.slice.call(document.querySelectorAll('.card.is-blocked'));
                  blocked.forEach(function (b) {
                      b.classList.add('glow-alt');
                      setTimeout(function () { b.classList.remove('glow-alt'); }, 650);
                  });
              }
          } catch (e) { console.warn('Highlight alternatives failed:', e); }
      }

      // ---- New: highlight only for a chosen course ----
      function highlightAlternativesFor(courseId) {
          try {
              var cards = Array.prototype.slice.call(document.querySelectorAll('.card'));
              cards.forEach(function (c) {
                  var mcid = (c.getAttribute('data-microcourse-id') || '').trim();
                  if (mcid === courseId) c.classList.add('glow-alt'); else c.classList.remove('glow-alt');
              });
          } catch (e) { console.warn('Selective highlight failed:', e); }
      }

      // ---- New: toggle and (re)build the picker menu ----
      function toggleAltPicker(ev) {
          ev.preventDefault();
          var picker = document.getElementById('altPicker');
          if (!picker) return;
          if (picker.getAttribute('data-built') !== '1') buildAltPicker(picker);
          picker.hidden = !picker.hidden;
      }

      function buildAltPicker(picker) {
          picker.innerHTML = '';
          var title = document.createElement('div');
          title.className = 'alt-title';
          title.textContent = 'Show alternatives for…';
          picker.appendChild(title);

          var items = getBlockedPrereqs();
          var list = document.createElement('div');
          list.className = 'alt-list';

          if (items.length === 0) {
              var empty = document.createElement('div');
              empty.className = 'alt-option';
              empty.textContent = 'No prerequisite blocks detected.';
              list.appendChild(empty);
          } else {
              items.forEach(function (it) {
                  var row = document.createElement('button');
                  row.type = 'button';
                  row.className = 'alt-option';
                  row.innerHTML = '<span>' + escapeHtml(it.title) + '</span><span class="pill pill-link" style="border-color:#d9e9f6;">Highlight</span>';
                  row.onclick = function () {
                      highlightAlternativesFor(it.id);
                      picker.hidden = true;
                  };
                  list.appendChild(row);
              });
          }

          picker.appendChild(list);

          var actions = document.createElement('div');
          actions.className = 'alt-actions';
          var btnAll = document.createElement('button');
          btnAll.type = 'button'; btnAll.className = 'btn btn-primary';
          btnAll.textContent = 'Highlight All';
          btnAll.onclick = function () { highlightAlternatives(); picker.hidden = true; };
          var btnClear = document.createElement('button');
          btnClear.type = 'button'; btnClear.className = 'btn btn-ghost';
          btnClear.textContent = 'Clear';
          btnClear.onclick = function () {
              Array.prototype.slice.call(document.querySelectorAll('.card.glow-alt')).forEach(function (c) { c.classList.remove('glow-alt'); });
              picker.hidden = true;
          };
          actions.appendChild(btnAll); actions.appendChild(btnClear);
          picker.appendChild(actions);

          picker.setAttribute('data-built', '1');
      }

      function escapeHtml(s) {
          return String(s).replace(/[&<>"']/g, function (m) {
              return ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' })[m];
          });
      }

      // Make From/To act like datetime-local inputs.
      document.addEventListener('DOMContentLoaded', function () {
          ['<%=FilterFrom.ClientID%>','<%=FilterTo.ClientID%>'].forEach(function (id) {
              var el = document.getElementById(id); if (el) { el.setAttribute('type', 'datetime-local'); }
          });

          // close picker on outside click
          document.addEventListener('click', function (e) {
              var picker = document.getElementById('altPicker');
              var wrap = document.querySelector('.alt-picker-wrap');
              if (!picker || !wrap) return;
              if (!wrap.contains(e.target)) picker.hidden = true;
          });
      });
  </script>
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

      <!-- ==== FILTERS ==== -->
      <div class="filters-card">
        <h3 class="filters-title">Filter sessions</h3>

        <div class="filters-grid">
          <div class="field">
            <label for="FilterFrom">From</label>
            <asp:TextBox ID="FilterFrom" runat="server" CssClass="input" />
          </div>

          <div class="field">
            <label for="FilterTo">To</label>
            <asp:TextBox ID="FilterTo" runat="server" CssClass="input" />
          </div>

          <div class="filters-actions">
            <asp:Button ID="ApplyFilters" runat="server" CssClass="btn btn-primary" Text="Apply"
                        OnClick="ApplyFilters_Click" />
            <asp:LinkButton ID="ClearFilters" runat="server" CssClass="btn btn-ghost" Text="Clear"
                            OnClick="ClearFilters_Click" CausesValidation="false" />
          </div>

          <div class="field row-full">
            <label>Tags</label>
            <div class="tags-wrap">
              <asp:CheckBoxList ID="FilterTags" runat="server" RepeatDirection="Horizontal" RepeatLayout="Flow" />
            </div>
          </div>

          <div class="field row-full">
            <label for="FilterQuery">Search</label>
            <asp:TextBox ID="FilterQuery" runat="server" CssClass="input"
                        placeholder="title, helper, or room…" />
            <div class="hint">Tip: type a few letters (e.g., “phish”, “Zoom”, or a helper’s name)</div>
          </div>
        </div>
      </div>
      <hr class="divider" />

      <!-- ==== MY SESSIONS ==== -->
      <asp:PlaceHolder ID="MySessionsWrap" runat="server" Visible="false">
        <div class="section">
          <h2 class="section-title">My Sessions</h2>
          <asp:Repeater ID="MySessionsRepeater" runat="server">
            <HeaderTemplate>
              <div class="fia-sessions-grid">
            </HeaderTemplate>
            <ItemTemplate>
              <div class="card">
                <div class="card-sec">
                  <h3 class="title"><%# Eval("microcourseTitle") %></h3>
                </div>

                <div class="card-sec">
                  <div class="line">
                    <span class="label">Helper</span>
                    <span class="chip helper-chip">
                      <span class="dot" style="background:var(--fia-teal);"></span>
                      <strong><%# Eval("helperName") %></strong>
                    </span>
                  </div>
                </div>

                <div class="card-sec">
                  <div class="line">
                    <span class="label">Time</span>
                    <span class="value"><%# Eval("startLocal", "{0:ddd, MMM d • h:mm tt}") %></span>
                  </div>
                  <asp:PlaceHolder runat="server" Visible='<%# !string.IsNullOrWhiteSpace(Convert.ToString(Eval("room"))) %>'>
                    <div class="line" style="margin-top:6px;">
                      <span class="label">Room</span>
                      <span class="value">
  <a href='<%# Eval("room") %>' target="_blank" rel="noopener">Join Room</a>
</span>
                    </div>
                  </asp:PlaceHolder>
                </div>

                <!-- Status -->
                <asp:Literal ID="MyStatusBadge" runat="server"
                  Text='<%# (bool)Eval("isEnrolled")
                      ? "<div class=\"card-sec\"><div class=\"info-bar ok\"><span><strong>Status</strong></span><span class=\"remain\">Enrolled</span></div></div>"
                      : "<div class=\"card-sec\"><div class=\"info-bar warn\"><span><strong>Status</strong></span><span>Waitlist position: " + Eval("waitlistPosition") + "</span></div></div>" %>' />

                <!-- Actions -->
                <div class="card-sec">
                  <div class="cta-row">
                    <!-- CHANGED: show Mark as Complete only for enrolled -->
                    <asp:Button ID="CompleteBtn" runat="server" CssClass="btn btn-ghost" Text="Mark as Complete"
                                CommandName="complete" CommandArgument='<%# Eval("sessionId") %>'
                                Visible='<%# (bool)Eval("isEnrolled") %>' />
                  </div>
                </div>

                <!-- NEW: Unenroll button (pink) on its own line; visible for enrolled OR waitlisted -->
                <div class="card-sec">
                  <div class="cta-row">
                    <asp:Button ID="UnenrollBtn" runat="server" CssClass="btn btn-pink" Text="Unenroll"
                                CommandName="unenroll" CommandArgument='<%# Eval("sessionId") %>'
                                Visible='<%# (bool)Eval("isEnrolled") || (bool)Eval("isWaitlisted") %>' />
                  </div>
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

      <!-- Empty state -->
      <asp:PlaceHolder ID="EmptySessionsPH" runat="server" Visible="false">
        <div class="note">No sessions are currently available. Please check back soon.</div>
      </asp:PlaceHolder>

      <!-- ==== AVAILABLE SESSIONS ==== -->
      <div class="section">
        <h2 class="section-title">
          Available Sessions
          <!-- Split control: button + caret opens picker -->
          <span class="alt-picker-wrap">
            <button type="button" class="pill pill-link" onclick="highlightAlternatives()">Highlight Alternatives</button>
            <button type="button" class="alt-caret" onclick="toggleAltPicker(event)" aria-haspopup="true" aria-expanded="false">▾</button>
            <div id="altPicker" class="alt-picker" hidden></div>
          </span>
        </h2>

        <asp:Repeater ID="SessionsRepeater" runat="server" OnItemCommand="SessionsRepeater_ItemCommand">
          <HeaderTemplate>
            <div class="fia-sessions-grid">
          </HeaderTemplate>

          <ItemTemplate>
            <div class='card <%# ((bool)Eval("prereqMet")) ? "" : "is-blocked" %>'
                 data-microcourse-id='<%# Eval("courseId") %>'
                 data-missing-prereq-id='<%# Eval("missingPrereqId") %>'
                 data-missing-prereq-title='<%# Server.HtmlEncode(Convert.ToString(Eval("missingPrereqTitle") ?? "")) %>'>

              <!-- Title -->
              <div class="card-sec">
                <h3 class="title"><%# Eval("microcourseTitle") %></h3>
              </div>

              <!-- Helper -->
              <div class="card-sec">
                <div class="line">
                  <span class="label">Helper</span>
                  <span class="chip helper-chip">
                    <span class="dot" style="background:var(--fia-teal);"></span>
                    <strong><%# Eval("helperName") %></strong>
                  </span>
                </div>
              </div>

              <!-- Seats -->
              <div class="card-sec">
                <div class="info-bar">
                  <span><strong>Seats</strong> remaining</span>
                  <span class='<%# (Convert.ToInt32(Eval("remainingSeats")) <= 3) ? "remain low" : "remain" %>'>
                    <%# Eval("remainingSeats") %> (<%# Eval("capacity") %>)
                  </span>
                </div>
              </div>

              <!-- Enrollment / Waitlist status -->
              <asp:Literal ID="StatusBadge" runat="server"
                Visible='<%# (bool)Eval("isEnrolled") || (bool)Eval("isWaitlisted") %>'
                Text='<%# (bool)Eval("isEnrolled")
                    ? "<div class=\"card-sec\"><div class=\"info-bar ok\"><span><strong>Status</strong></span><span class=\"remain\">Enrolled</span></div></div>"
                    : "<div class=\"card-sec\"><div class=\"info-bar warn\"><span><strong>Status</strong></span><span>Waitlist position: " + Eval("waitlistPosition") + "</span></div></div>" %>' />

              <!-- Prereq blocked note -->
              <asp:PlaceHolder ID="BlockedPH" runat="server" Visible='<%# !(bool)Eval("prereqMet") %>'>
                <div class="card-sec">
                  <div class="blocked-note">
                    <strong>Prerequisite needed.</strong>
                    You can’t join this yet. First complete: <em><%# Eval("missingPrereqTitle") %></em>.
                    Use <strong>Highlight Alternatives</strong> to quickly find sessions that unlock this.
                  </div>
                </div>
              </asp:PlaceHolder>

              <!-- Overlap conflict note -->
              <asp:PlaceHolder ID="ConflictPH" runat="server" Visible='<%# (bool)Eval("hasConflict") %>'>
                <div class="card-sec">
                  <div class="conflict-note">
                    <strong>Time conflict.</strong>
                    <%# Eval("conflictMessage") %>
                    <div class="conflict-actions">
                      <a class="btn btn-ghost" href="<%# Eval("replacementUrl") %>">See Replacements</a>
                    </div>
                  </div>
                </div>
              </asp:PlaceHolder>

              <!-- Time -->
              <div class="card-sec">
                <div class="line">
                  <span class="label">Time</span>
                  <span class="value"><%# Eval("startLocal", "{0:ddd, MMM d • h:mm tt}") %></span>
                </div>
              </div>

              <!-- Actions -->
              <div class="card-sec">
                <div class="cta-row">
                  <asp:Button ID="EnrollBtn" runat="server" CssClass="btn btn-primary" Text="Enroll"
                              CommandName="enroll" CommandArgument='<%# Eval("sessionId") %>'
                              Visible='<%# (bool)Eval("prereqMet") && !(bool)Eval("hasConflict") && !(bool)Eval("isEnrolled") && !(bool)Eval("isWaitlisted") && !(bool)Eval("isFull") %>' />

                  <asp:Button ID="WaitlistBtn" runat="server" CssClass="btn btn-ghost" Text="Join Waitlist"
                              CommandName="waitlist" CommandArgument='<%# Eval("sessionId") %>'
                              Visible='<%# (bool)Eval("prereqMet") && !(bool)Eval("hasConflict") && !(bool)Eval("isEnrolled") && !(bool)Eval("isWaitlisted") && (bool)Eval("isFull") %>' />
                </div>
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







