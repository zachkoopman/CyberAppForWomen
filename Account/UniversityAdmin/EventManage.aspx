<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="EventManage.aspx.cs" Inherits="CyberApp_FIA.Account.EventManage" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
  <title>FIA • Manage Event</title>
  <meta name="viewport" content="width=device-width, initial-scale=1" />
  <link href="https://fonts.googleapis.com/css2?family=Lato:wght@300;400&family=Poppins:wght@600&display=swap" rel="stylesheet" />

  <style>
    /* ========= Design tokens ========= */
    :root{--fia-pink:#f06aa9;--fia-blue:#2a99db;--fia-teal:#45c3b3;--ink:#1c1c1c;--muted:#6b7280;--ring:rgba(42,153,219,.25)}

    /* ========= Base layout ========= */
    *{box-sizing:border-box}
    body{margin:0;font-family:Lato,Arial,sans-serif;background:linear-gradient(135deg,#fff,#f9fbff)}

    /* Page wrapper */
    .wrap{min-height:100vh;padding:24px;max-width:1100px;margin:0 auto}

    /* Brand header */
    .brand{display:flex;align-items:center;gap:10px;margin-bottom:10px}
    .badge{width:42px;height:42px;border-radius:12px;background:linear-gradient(135deg,var(--fia-pink),var(--fia-blue));display:grid;place-items:center;color:#fff;font-family:Poppins}
    h1{font-family:Poppins;margin:0 0 6px 0;font-size:1.35rem}
    .sub{color:var(--muted);margin:0 0 16px 0}

    /* Card containers */
    .card{background:#fff;border:1px solid #e8eef7;border-radius:20px;box-shadow:0 12px 36px rgba(42,153,219,.08);padding:20px;margin-bottom:16px}

    /* Form controls */
    label{font-weight:600;font-family:Poppins;font-size:.95rem}
    input[type=text], input[type=datetime-local], input[type=number], textarea{
      width:100%;padding:12px 14px;border-radius:12px;border:1px solid #e5e7eb
    }
    input:focus, textarea:focus{outline:0;box-shadow:0 0 0 5px var(--ring);border-color:var(--fia-blue)}

    /* Grid layout for form fields */
    .grid{display:grid;grid-template-columns:1fr 1fr;gap:14px}
    @media (max-width:900px){.grid{grid-template-columns:1fr}}

    /* Buttons */
    .btn{border:0;border-radius:12px;padding:12px 18px;font-weight:700;font-family:Poppins;cursor:pointer}
    .primary{color:#fff;background:linear-gradient(135deg,var(--fia-blue),var(--fia-teal))}
    .link{background:#fff;border:2px solid var(--fia-blue);color:var(--fia-blue)}
    .btnrow{display:flex;gap:10px;flex-wrap:wrap;margin-top:14px}

    /* Pills / informational styles */
    .pill{display:inline-block;padding:6px 10px;border-radius:999px;background:#f6f7fb;border:1px solid #e8eef7;margin-right:8px;font-size:.9rem}
    .note{background:#f6f7fb;border:1px solid #e8eef7;border-radius:12px;padding:12px;color:var(--muted);font-size:.95rem}

    /* Tables for lists */
    table{width:100%;border-collapse:collapse}
    th,td{padding:8px;border-bottom:1px solid #f0f3f9;text-align:left}

    /* Validation text */
    .val{color:#c21d1d;font-size:.9rem;margin-top:4px;display:block}
  </style>
</head>

<body>
<form id="form1" runat="server">
  <div class="wrap">

    <!-- =========================
         Event header / context
         ========================= -->
    <div class="brand">
      <div class="badge">FIA</div>
      <div>
        <h1>Manage Event: <asp:Literal ID="EventName" runat="server" /></h1>
        <div class="sub">
          <span class="pill">University: <asp:Literal ID="University" runat="server" /></span>
          <span class="pill">Date: <asp:Literal ID="EventDate" runat="server" /></span>
          <span class="pill">Status: <asp:Literal ID="EventStatus" runat="server" /></span>
        </div>
      </div>
    </div>

    <!-- =====================================================
         #86 Microcourse List + #85 Switch (visibility toggles)
         ===================================================== -->
    <div class="card">
      <h2>Microcourses available</h2>
      <p class="sub">Published microcourses you can include in this event. Toggle visibility per course.</p>

      <!-- Empty state if there are no Published microcourses -->
      <asp:PlaceHolder ID="NoCoursesPH" runat="server" Visible="false">
        <div class="note">No Published microcourses found yet.</div>
      </asp:PlaceHolder>

      <!-- Courses table: title / tags / duration / enabled switch -->
      <asp:Repeater ID="CoursesRepeater" runat="server">
        <HeaderTemplate>
          <%-- Start courses table --%>
          <table>
            <thead>
              <tr>
                <th>Title</th>
                <th>Tags</th>
                <th>Duration</th>
                <th>Visible in event?</th>
              </tr>
            </thead>
            <tbody>
        </HeaderTemplate>

        <ItemTemplate>
          <tr>
            <td><%# Eval("title") %></td>
            <td><%# Eval("tags") %></td>
            <td><%# Eval("duration") %></td>
            <td>
              <asp:CheckBox ID="Enabled" runat="server" Checked='<%# (bool)Eval("enabled") %>' />
              <asp:HiddenField ID="CourseId" runat="server" Value='<%# Eval("id") %>' />
            </td>
          </tr>
        </ItemTemplate>

        <FooterTemplate>
            </tbody>
          </table>
          <%-- End courses table --%>
        </FooterTemplate>
      </asp:Repeater>

      <!-- Persist current visibility switches -->
      <div class="btnrow">
        <asp:Button ID="BtnSaveSwitches" runat="server" Text="Save visibility" CssClass="btn primary" OnClick="BtnSaveSwitches_Click" CausesValidation="false" />
      </div>
    </div>

    <!-- =====================================================
         #68 Scheduling (times) & #69 Capacity (limit/waitlist)
         ===================================================== -->
    <div class="card">
      <h2>Schedule a microcourse session</h2>
      <p class="sub">Only the same helper at overlapping times is blocked. Different helpers can run in parallel.</p>

      <!-- Session scheduling form -->
      <div class="grid">
        <div>
          <label for="CourseSelect">Course</label>
          <asp:DropDownList ID="CourseSelect" runat="server" />
        </div>

        <div>
          <label for="SessionDateTimeStart">Start</label>
          <asp:TextBox ID="SessionDateTimeStart" runat="server" TextMode="DateTimeLocal" />
          <asp:RequiredFieldValidator runat="server" ControlToValidate="SessionDateTimeStart" CssClass="val"
            ErrorMessage="Start is required." Display="Dynamic" />
        </div>

        <div>
          <label for="SessionDateTimeEnd">End</label>
          <asp:TextBox ID="SessionDateTimeEnd" runat="server" TextMode="DateTimeLocal" />
          <asp:RequiredFieldValidator runat="server" ControlToValidate="SessionDateTimeEnd" CssClass="val"
            ErrorMessage="End is required." Display="Dynamic" />
        </div>

        <div>
          <label for="Helper">Helper</label>
          <asp:TextBox ID="Helper" runat="server" Placeholder="e.g., Tracy Nguyen" />
          <asp:RequiredFieldValidator runat="server" ControlToValidate="Helper" CssClass="val"
            ErrorMessage="Helper is required (used to prevent double-booking)." Display="Dynamic" />
        </div>

        <div>
          <label for="Room">Room (optional)</label>
          <asp:TextBox ID="Room" runat="server" Placeholder="e.g., MU 201" />
        </div>

        <div>
          <label for="Capacity">Max participants (optional)</label>
          <asp:TextBox ID="Capacity" runat="server" TextMode="Number" Placeholder="e.g., 25" />
        </div>
      </div>

      <!-- Add session button + inline result message -->
      <div class="btnrow">
        <asp:Button ID="BtnAddSession" runat="server" Text="Add session" CssClass="btn primary" OnClick="BtnAddSession_Click" />
      </div>
      <asp:Label ID="ScheduleMessage" runat="server" EnableViewState="false" />

      <!-- Sessions list -->
      <h3 style="margin-top:16px;">Scheduled sessions</h3>

      <!-- Empty state if there are no sessions yet -->
      <asp:PlaceHolder ID="NoSessionsPH" runat="server" Visible="false">
        <div class="note">No sessions yet for this event.</div>
      </asp:PlaceHolder>

      <!-- Sessions table (rendered via repeater) -->
      <asp:Repeater ID="SessionsRepeater" runat="server">
        <HeaderTemplate>
          <%-- Start sessions table --%>
          <table>
            <thead>
              <tr>
                <th>Course</th>
                <th>Start</th>
                <th>End</th>
                <th>Room</th>
                <th>Helper</th>
                <th>Capacity</th>
              </tr>
            </thead>
            <tbody>
        </HeaderTemplate>

        <ItemTemplate>
          <tr>
            <td><%# Eval("courseTitle") %></td>
            <td><%# Eval("startLocal") %></td>
            <td><%# Eval("endLocal") %></td>
            <td><%# Eval("room") %></td>
            <td><%# Eval("helper") %></td>
            <td><%# Eval("capacity") %></td>
          </tr>
        </ItemTemplate>

        <FooterTemplate>
            </tbody>
          </table>
          <%-- End sessions table --%>
        </FooterTemplate>
      </asp:Repeater>
    </div>

    <!-- Back navigation -->
    <div class="btnrow">
      <a class="btn link" href="<%: ResolveUrl("~/Account/UniversityAdmin/UniversityAdminHome.aspx") %>">← Back to UA Home</a>
    </div>

  </div>
</form>
</body>
</html>


