<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="UniversityAdminHome.aspx.cs" Inherits="CyberApp_FIA.Account.UniversityAdminHome" %> 

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
  <title>FIA • University Admin Home</title>
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
        radial-gradient(circle at top left,rgba(240,106,169,.14),transparent 50%),
        radial-gradient(circle at bottom right,rgba(69,195,179,.14),transparent 55%),
        #f9fafb;
    }

    .page{
      max-width:1120px;
      margin:0 auto;
      padding:24px 16px 40px;
    }

    .topbar{
      display:flex;
      justify-content:space-between;
      align-items:flex-start;
      gap:12px;
      margin-bottom:18px;
    }

    .brand{
      display:flex;
      align-items:center;
      gap:10px;
    }

    .logo-pill{
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
      box-shadow:0 14px 32px rgba(15,23,42,.18);
    }

    .brand-text h1{
      font-family:'Poppins',sans-serif;
      font-size:1.35rem;
      margin:0;
    }

    .brand-text p{
      margin:2px 0 0;
      font-size:0.85rem;
      color:var(--muted);
    }

    .ua-meta{
      font-size:0.83rem;
      color:var(--muted);
      text-align:right;
    }

    .ua-meta strong{
      display:block;
      font-weight:600;
      color:var(--ink);
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

    .layout{
      display:grid;
      grid-template-columns:minmax(0,1.1fr) minmax(0,1.2fr);
      gap:18px;
    }

    @media (max-width:960px){
      .layout{grid-template-columns:minmax(0,1fr);}
      .ua-meta{text-align:left;}
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

    .field{
      display:flex;
      flex-direction:column;
      gap:4px;
      margin-bottom:10px;
      font-size:0.85rem;
    }

    label{
      font-weight:600;
      font-family:'Poppins',sans-serif;
      font-size:0.85rem;
    }

    input[type=text],
    input[type=date]{
      width:100%;
      padding:9px 12px;
      border-radius:12px;
      border:1px solid var(--border);
      font-family:'Lato',sans-serif;
      font-size:0.9rem;
      transition:border-color .15s ease, box-shadow .15s ease, background .15s ease;
    }

    input:focus{
      outline:none;
      border-color:var(--fia-blue);
      box-shadow:0 0 0 3px var(--ring);
      background:#f9fafb;
    }

    .hint{
      font-size:0.8rem;
      color:var(--muted);
    }

    .btn-row{
      margin-top:10px;
      display:flex;
      gap:8px;
      flex-wrap:wrap;
      align-items:center;
    }

    .status{
      font-size:0.8rem;
      color:var(--muted);
    }

    .status.error{
      color:#b91c1c;
    }

    .pill-row{
      display:flex;
      flex-wrap:wrap;
      gap:6px;
      margin-top:6px;
      font-size:0.78rem;
    }

    .pill{
      padding:3px 10px;
      border-radius:999px;
      background:rgba(42,153,219,.06);
      color:var(--muted);
    }

    .events-empty{
      padding:12px;
      border-radius:14px;
      border:1px dashed rgba(148,163,184,.7);
      background:#f9fafb;
      font-size:0.85rem;
      color:var(--muted);
      text-align:center;
    }

    .events-list{
      list-style:none;
      padding:0;
      margin:6px 0 0;
      display:flex;
      flex-direction:column;
      gap:10px;
      font-size:0.88rem;
    }

    .event-card{
      border-radius:16px;
      border:1px solid var(--border);
      padding:10px 12px;
      background:#fff;
      display:flex;
      justify-content:space-between;
      gap:10px;
      align-items:flex-start;
    }

    .event-main{
      display:flex;
      flex-direction:column;
      gap:2px;
    }

    .event-title{
      font-weight:600;
    }

    .event-meta{
      font-size:0.8rem;
      color:var(--muted);
    }

    .event-actions{
      display:flex;
      flex-direction:column;
      align-items:flex-end;
      gap:4px;
      font-size:0.8rem;
    }

    .event-manage-link{
      color:var(--fia-blue);
      text-decoration:none;
      font-weight:500;
    }
  </style>
</head>
<body>
  <form id="form1" runat="server">
    <div class="page">
      <div class="topbar">
        <div class="brand">
          <div class="logo-pill">FIA</div>
          <div class="brand-text">
            <h1>University Admin workspace</h1>
            <p>Schedule FIA events and keep your campus catalog clean and safe.</p>
          </div>
        </div>

        <div class="ua-meta">
          <strong><asp:Literal ID="WelcomeName" runat="server" /></strong>
          <asp:Literal ID="UniversityDisplay" runat="server" /><br />
          <asp:HiddenField ID="UniversityValue" runat="server" />
          <div style="margin-top:6px;">
            <asp:Button ID="BtnLogout"
                        runat="server"
                        Text="Sign out"
                        CssClass="btn link"
                        OnClick="BtnLogout_Click" />
          </div>
        </div>
      </div>

      <div class="layout">
        <!-- Left: create new event -->
        <div class="card">
          <h2 class="card-title">Create a new FIA event</h2>
          <p class="hint" style="margin-top:0;margin-bottom:8px;">
            Select a date and give the event a clear, participant-friendly name. You can add sessions and helpers from the manage page.
          </p>

          <div class="field">
            <label for="EventDate">Event date</label>
            <asp:TextBox ID="EventDate" runat="server" TextMode="Date" />
            <span class="hint">This anchors the event; sessions can span times within this day.</span>
          </div>

          <div class="field">
            <label for="EventName">Event name</label>
            <asp:TextBox ID="EventName" runat="server" Placeholder="e.g., Fall Cyberfair 2025" />
            <span class="hint">Participants will see this in the catalog and reminders.</span>
          </div>

          <div class="btn-row">
            <asp:Button ID="BtnCreateEvent"
                        runat="server"
                        Text="Create event"
                        CssClass="btn primary"
                        OnClick="BtnCreateEvent_Click" />
            <asp:Button ID="BtnClear"
                        runat="server"
                        Text="Clear form"
                        CssClass="btn link"
                        OnClick="BtnClear_Click"
                        CausesValidation="false" />

            <asp:Label ID="FormMessage" runat="server" EnableViewState="false" CssClass="status" />
          </div>

          <div class="pill-row">
            <div class="pill">Privacy-by-default participants</div>
            <div class="pill">Audit-logged catalog changes</div>
            <div class="pill">Helper certification rules from FIA</div>
          </div>
        </div>

        <!-- Right: upcoming events list -->
        <div class="card">
          <h2 class="card-title">Upcoming FIA events</h2>
          <p class="hint" style="margin-top:0;margin-bottom:6px;">
            Manage sessions and helpers for each event. Cancel or reschedule using the impact summary flow.
          </p>

          <asp:PlaceHolder ID="NoEventsPlaceholder" runat="server" Visible="false">
            <div class="events-empty">
              No events yet. Create your first FIA event for this university on the left.
            </div>
          </asp:PlaceHolder>

          <asp:Repeater ID="EventsRepeater" runat="server">
            <ItemTemplate>
              <div class="event-card">
                <div class="event-main">
                  <div class="event-title"><%# Eval("Name") %></div>
                  <div class="event-meta">
                    <%# Eval("Date", "{0:yyyy-MM-dd}") %> • <%# Eval("Status") %>
                  </div>
                </div>
                <div class="event-actions">
                  <a class="event-manage-link"
                     href='<%# ResolveUrl("~/Account/UniversityAdmin/EventManage.aspx?eventId=" + Eval("Id")) %>'>
                    Manage event →
                  </a>
                  <span class="hint">Add sessions, helpers, and apply schedule changes.</span>
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
