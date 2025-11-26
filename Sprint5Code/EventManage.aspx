<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="EventManage.aspx.cs" Inherits="CyberApp_FIA.Account.EventManage" %> 

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
  <title>FIA • Manage Event</title>
  <meta name="viewport" content="width=device-width, initial-scale=1" />

  <link href="https://fonts.googleapis.com/css2?family=Lato:wght@300;400&family=Poppins:wght@600&display=swap" rel="stylesheet" />

  <style>
    :root{
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

    *{box-sizing:border-box;}

    body{
      margin:0;
      font-family:'Lato',sans-serif;
      color:var(--ink);
      background:
        radial-gradient(circle at 0 0,rgba(240,106,169,.10),transparent 55%),
        radial-gradient(circle at 100% 100%,rgba(42,153,219,.12),transparent 55%),
        #f9fafb;
    }

    .page{
      max-width:1180px;
      margin:0 auto;
      padding:24px 16px 40px;
    }

    .header{
      margin-bottom:16px;
    }

    .header-title{
      display:flex;
      justify-content:space-between;
      align-items:flex-start;
      gap:12px;
    }

    h1{
      margin:0;
      font-family:'Poppins',sans-serif;
      font-size:1.35rem;
    }

    .pills{
      display:flex;
      flex-wrap:wrap;
      gap:6px;
      margin-top:10px;
      font-size:0.78rem;
    }

    .pill{
      padding:3px 10px;
      border-radius:999px;
      background:rgba(42,153,219,.06);
      color:var(--muted);
    }

    .layout{
      display:grid;
      grid-template-columns:minmax(0,1.1fr) minmax(0,1.4fr);
      gap:18px;
    }

    @media (max-width:1024px){
      .layout{grid-template-columns:minmax(0,1fr);}
    }

    .card{
      background:var(--surface);
      border-radius:20px;
      border:1px solid var(--border);
      padding:18px 18px 20px;
      box-shadow:0 18px 42px rgba(15,23,42,.06);
    }

    .card-title{
      font-family:'Poppins',sans-serif;
      font-size:1rem;
      margin:0 0 8px;
    }

    .hint{
      font-size:0.8rem;
      color:var(--muted);
    }

    label{
      font-weight:600;
      font-family:'Poppins',sans-serif;
      font-size:0.85rem;
    }

    input[type=text],
    input[type=number],
    input[type=datetime-local],
    select{
      width:100%;
      padding:9px 12px;
      border-radius:12px;
      border:1px solid var(--border);
      font-family:'Lato',sans-serif;
      font-size:0.9rem;
      transition:border-color .15s ease, box-shadow .15s ease, background .15s ease;
    }

    input:focus,
    select:focus{
      outline:none;
      border-color:var(--fia-blue);
      box-shadow:0 0 0 3px var(--ring);
      background:#f9fafb;
    }

    .btn{
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

    .btn.primary{
      background:linear-gradient(135deg,var(--fia-pink),var(--fia-blue));
      color:#fff;
      border:none;
      box-shadow:0 10px 24px rgba(42,153,219,.35);
    }

    .btn.link{
      background:transparent;
      color:var(--fia-blue);
      border:1px dashed rgba(148,163,184,.7);
    }

    .btn:hover{
      transform:translateY(-1px);
      box-shadow:0 10px 28px rgba(15,23,42,.14);
    }

    .btn:focus{
      outline:none;
      box-shadow:0 0 0 4px var(--ring);
    }

    .btn-row{
      display:flex;
      gap:8px;
      flex-wrap:wrap;
      align-items:center;
      margin-top:10px;
    }

    .status{
      font-size:0.8rem;
      color:var(--muted);
    }

    .status.error{
      color:#b91c1c;
    }

    .courses-empty,
    .helpers-empty,
    .sessions-empty{
      padding:10px 12px;
      border-radius:14px;
      border:1px dashed rgba(148,163,184,.7);
      background:#f9fafb;
      font-size:0.85rem;
      color:var(--muted);
      text-align:center;
      margin-top:6px;
    }

    .courses-list{
      list-style:none;
      padding:0;
      margin:8px 0 0;
      display:flex;
      flex-direction:column;
      gap:8px;
      font-size:0.88rem;
    }

    .course-item{
      display:flex;
      justify-content:space-between;
      align-items:center;
      gap:10px;
      padding:8px 10px;
      border-radius:14px;
      border:1px solid var(--border);
      background:#fff;
    }

    .course-main{
      display:flex;
      flex-direction:column;
      gap:2px;
    }

    .course-title{
      font-weight:600;
    }

    .course-meta{
      font-size:0.78rem;
      color:var(--muted);
    }

    .filters-row{
      display:flex;
      flex-wrap:wrap;
      gap:10px 16px;
      align-items:center;
      margin-top:6px;
      font-size:0.8rem;
    }

    .filters-row label{
      font-weight:400;
      font-family:'Lato',sans-serif;
      display:flex;
      align-items:center;
      gap:4px;
    }

    .helpers-list{
      list-style:none;
      padding:0;
      margin:8px 0 0;
      display:flex;
      flex-direction:column;
      gap:6px;
      font-size:0.85rem;
    }

    .helper-tag{
      padding:3px 8px;
      border-radius:999px;
      font-size:0.75rem;
      background:rgba(42,153,219,.06);
      color:var(--muted);
    }

    .badge-critical{
      background:#fee2e2;
      color:#b91c1c;
    }
  </style>
</head>
<body>
  <form id="form1" runat="server">
    <div class="page">
      <div class="header">
        <div class="header-title">
          <div>
            <h1>Manage Event: <asp:Literal ID="EventName" runat="server" /></h1>
            <div class="pills">
              <span class="pill">University: <asp:Literal ID="University" runat="server" /></span>
              <span class="pill">Date: <asp:Literal ID="EventDate" runat="server" /></span>
              <span class="pill">Status: <asp:Literal ID="EventStatus" runat="server" /></span>
            </div>
          </div>
          <a class="btn link" href="<%: ResolveUrl("~/Account/UniversityAdmin/UniversityAdminHome.aspx") %>">
            ← Back to University Admin home
          </a>
        </div>
      </div>

      <div class="layout">
        <!-- Left: course catalog visibility -->
        <div class="card">
          <h2 class="card-title">Courses in this event</h2>
          <p class="hint">
            Toggle which FIA microcourses appear in this event’s catalog. Changes are logged in the admin audit trail.
          </p>

          <asp:PlaceHolder ID="NoCoursesPH" runat="server" Visible="false">
            <div class="courses-empty">
              No courses are currently linked to this event. Use the dropdown on the right to add sessions.
            </div>
          </asp:PlaceHolder>

          <asp:Repeater ID="CoursesRepeater" runat="server">
            <ItemTemplate>
              <li class="course-item">
                <div class="course-main">
                  <span class="course-title"><%# Eval("title") %></span>
                  <span class="course-meta">Course ID: <%# Eval("id") %></span>
                </div>
                <div style="display:flex;align-items:center;gap:8px;font-size:0.8rem;">
                  <asp:CheckBox ID="Enabled"
                                runat="server"
                                Checked='<%# (bool)Eval("enabled") %>' />
                  <span>Visible to participants</span>
                  <asp:HiddenField ID="CourseId" runat="server" Value='<%# Eval("id") %>' />
                </div>
              </li>
            </ItemTemplate>
            <HeaderTemplate>
              <ul class="courses-list">
            </HeaderTemplate>
            <FooterTemplate>
              </ul>
            </FooterTemplate>
          </asp:Repeater>

          <div class="btn-row" style="margin-top:12px;">
            <asp:Button ID="BtnSaveSwitches"
                        runat="server"
                        Text="Save visibility"
                        CssClass="btn primary"
                        OnClick="BtnSaveSwitches_Click"
                        CausesValidation="false" />
          </div>

          <div class="hint" style="margin-top:8px;">
            Disabled courses will not show up in the participant catalog for this event, but the historical audit trail is preserved.
          </div>
        </div>

        <!-- Right: schedule and helper assignment -->
        <div class="card">
          <h2 class="card-title">Schedule sessions & assign helpers</h2>
          <p class="hint">
            Create session times, choose rooms, and connect certified helpers. Use the filters to find eligible helpers quickly.
          </p>

          <!-- Session basics -->
          <div class="field" style="margin-top:6px;">
            <label for="CourseSelect">Course</label>
            <asp:DropDownList ID="CourseSelect" runat="server" />
            <span class="hint">Pick which microcourse this session will deliver.</span>
          </div>

          <div class="field">
            <label for="SessionDateTimeStart">Session start (local time)</label>
            <asp:TextBox ID="SessionDateTimeStart" runat="server" TextMode="DateTimeLocal" />
          </div>

          <div class="field">
            <label for="SessionDateTimeEnd">Session end (local time)</label>
            <asp:TextBox ID="SessionDateTimeEnd" runat="server" TextMode="DateTimeLocal" />
          </div>

          <div class="field">
            <label for="Room">Room</label>
            <asp:TextBox ID="Room" runat="server" Placeholder="e.g., MU 201" />
          </div>

          <div class="field">
            <label for="Capacity">Capacity</label>
            <asp:TextBox ID="Capacity" runat="server" TextMode="Number" Placeholder="e.g., 25" />
          </div>

          <!-- Helper filters -->
          <div class="fieldset" style="margin:10px 0 0;">
            <div class="hint" style="margin-bottom:4px;font-weight:600;color:var(--ink);">
              Helper filters
            </div>
            <div class="filters-row">
              <label>
                <asp:CheckBox ID="FilterEligible" runat="server" Text="" />
                Eligible only
              </label>
              <label>
                <asp:CheckBox ID="FilterCertified" runat="server" Text="" />
                Certified only
              </label>
              <label>
                <asp:CheckBox ID="SortByLastDelivered" runat="server" Text="" />
                Sort by last delivered
              </label>
              <label>
                <asp:CheckBox ID="SortByMostDelivered" runat="server" Text="" />
                Sort by most delivered
              </label>
              <asp:Button ID="BtnClearHelperFilters"
                          runat="server"
                          Text="Clear filters"
                          CssClass="btn link"
                          OnClick="BtnClearHelperFilters_Click"
                          CausesValidation="false" />
            </div>
          </div>

          <!-- Helpers list -->
          <asp:PlaceHolder ID="NoHelpersPH" runat="server" Visible="false">
            <div class="helpers-empty">
              No helpers are available for this combination of filters. Try widening the filters or adding more helpers to this university.
            </div>
          </asp:PlaceHolder>

          <asp:Repeater ID="HelpersRepeater" runat="server">
            <HeaderTemplate>
              <ul class="helpers-list">
            </HeaderTemplate>
            <ItemTemplate>
              <li>
                <span><%# Eval("HelperName") %></span>
                <span class="helper-tag">
                  Delivered: <%# Eval("DeliveredCount") %>
                </span>
              </li>
            </ItemTemplate>
            <FooterTemplate>
              </ul>
            </FooterTemplate>
          </asp:Repeater>

          <!-- Overlap / availability messages -->
          <asp:PlaceHolder ID="OverlapPH" runat="server" Visible="false">
            <div class="helpers-empty badge-critical">
              Potential schedule overlap detected for at least one helper. Adjust the time or choose a different helper.
            </div>
          </asp:PlaceHolder>

          <asp:PlaceHolder ID="AvailablePH" runat="server" Visible="false">
            <div class="hint" style="margin-top:6px;">
              Helpers are available at this time based on current bookings.
            </div>
          </asp:PlaceHolder>

          <!-- Final helper selection and add session -->
          <div class="field" style="margin-top:10px;">
            <label for="HelperSelect">Assign helper</label>
            <asp:DropDownList ID="HelperSelect" runat="server" CssClass="helper-select" />
          </div>

          <div class="btn-row">
            <asp:Button ID="BtnAddSession"
                        runat="server"
                        Text="Add session"
                        CssClass="btn primary"
                        OnClick="BtnAddSession_Click" />
            <asp:Label ID="ScheduleMessage" runat="server" EnableViewState="false" CssClass="status" />
          </div>

          <!-- Existing sessions -->
          <asp:PlaceHolder ID="NoSessionsPH" runat="server" Visible="false">
            <div class="sessions-empty">
              No sessions have been scheduled yet. Use the form above to create your first session.
            </div>
          </asp:PlaceHolder>

          <asp:Repeater ID="SessionsRepeater" runat="server">
            <HeaderTemplate>
              <h3 class="card-title" style="margin-top:16px;">Scheduled sessions</h3>
              <ul class="helpers-list">
            </HeaderTemplate>
            <ItemTemplate>
              <li>
                <span>
                  <%# Eval("StartUtc", "{0:yyyy-MM-dd HH:mm}") %> – <%# Eval("EndUtc", "{0:HH:mm}") %>,
                  room <%# Eval("Room") %> • <%# Eval("CourseTitle") %>
                </span>
                <span class="helper-tag">
                  Helper: <%# Eval("HelperName") %>
                </span>
              </li>
            </ItemTemplate>
            <FooterTemplate>
              </ul>
            </FooterTemplate>
          </asp:Repeater>
        </div>
      </div>
    </div>
  </form>
</body>
</html>
