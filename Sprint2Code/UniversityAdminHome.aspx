<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="UniversityAdminHome.aspx.cs" Inherits="CyberApp_FIA.Account.UniversityAdminHome" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
  <title>FIA • University Admin</title>
  <meta name="viewport" content="width=device-width, initial-scale=1" />
  <link href="https://fonts.googleapis.com/css2?family=Lato:wght@300;400&family=Poppins:wght@600&display=swap" rel="stylesheet" />

  <style>
    /* =========================================================================
       Design tokens (brand colors, text colors, focus ring)
       ========================================================================= */
    :root{ --fia-pink:#f06aa9; --fia-blue:#2a99db; --fia-teal:#45c3b3; --ink:#1c1c1c; --muted:#6b7280; --ring:rgba(42,153,219,.25) }

    /* Base layout */
    *{ box-sizing:border-box }
    html,body{ height:100% }
    body{
      margin:0;
      font-family:Lato,Arial,sans-serif;
      background:linear-gradient(135deg,#fff,#f9fbff);
      color:var(--ink);
    }

    /* Page wrapper centers content and limits width */
    .wrap{
      min-height:100vh;
      padding:24px;
      max-width:1000px;
      margin:0 auto;
    }

    /* Header (brand + sign-out) */
    .header{
      display:flex;
      align-items:center;
      justify-content:space-between;
      gap:14px;
      margin-bottom:16px;
    }
    .brand{ display:flex; align-items:center; gap:10px }
    .badge{
      width:42px; height:42px; border-radius:12px;
      background:linear-gradient(135deg,var(--fia-pink),var(--fia-blue));
      display:grid; place-items:center;
      color:#fff; font-family:Poppins;
    }
    h1{ font-family:Poppins; margin:0; font-size:1.35rem }
    .hello{ color:var(--muted); font-size:.95rem }

    /* Card container used for form blocks and lists */
    .card{
      background:#fff;
      border:1px solid #e8eef7;
      border-radius:20px;
      box-shadow:0 12px 36px rgba(42,153,219,.08);
      padding:20px;
      margin-bottom:16px;
    }
    .card h2{ font-family:Poppins; margin:0 0 8px 0; font-size:1.15rem }
    .sub{ color:var(--muted); margin:0 0 12px 0 }

    /* Form controls */
    label{ font-weight:600; font-family:Poppins; font-size:.95rem }
    input[type=text], input[type=date], textarea{
      width:100%;
      padding:12px 14px;
      border-radius:12px;
      border:1px solid #e5e7eb;
    }
    textarea{ min-height:110px; resize:vertical }
    input:focus, textarea:focus{
      outline:0;
      box-shadow:0 0 0 5px var(--ring);
      border-color:var(--fia-blue);
    }

    /* Two-column grid for form fields; collapses on small screens */
    .grid{ display:grid; grid-template-columns:1fr 1fr; gap:14px }
    @media (max-width:800px){ .grid{ grid-template-columns:1fr } }

    /* Buttons */
    .btnrow{ display:flex; gap:10px; flex-wrap:wrap; margin-top:14px }
    .btn{
      border:0;
      border-radius:12px;
      padding:12px 18px;
      font-weight:700;
      font-family:Poppins;
      cursor:pointer;
    }
    .primary{ color:#fff; background:linear-gradient(135deg,var(--fia-blue),var(--fia-teal)) }
    .link{ background:#fff; border:2px solid var(--fia-blue); color:var(--fia-blue) }

    /* Validation and info */
    .val{ color:#c21d1d; font-size:.9rem; margin-top:4px }
    .note{
      background:#f6f7fb;
      border:1px solid #e8eef7;
      border-radius:12px;
      padding:12px;
      color:var(--muted);
      font-size:.95rem;
    }
  </style>
</head>

<body>
  <form id="form1" runat="server">
    <div class="wrap">

      <!-- ===============================================================
           Page header: product brand + welcome + sign-out
           =============================================================== -->
      <div class="header">
        <div class="brand">
          <div class="badge">FIA</div>
          <div>
            <h1>University Admin Home</h1>
            <div class="hello">
              Welcome, <asp:Literal ID="WelcomeName" runat="server" />.
            </div>
          </div>
        </div>

        <!-- Sign-out button (no validation on click) -->
        <div>
          <asp:Button
            ID="BtnLogout"
            runat="server"
            Text="Sign out"
            CssClass="btn link"
            OnClick="BtnLogout_Click"
            CausesValidation="false" />
        </div>
      </div>

      <!-- ===============================================================
           Create Cyberfair Event (form card)
           =============================================================== -->
      <div class="card">
        <h2>Create a new Cyberfair event</h2>
        <p class="sub">This event will host your selected microcourses.</p>

        <div class="grid">
          <!-- Predefined, read-only university display + hidden value for submission -->
          <div style="grid-column:1/-1">
            <label>University</label>
            <div class="note" style="font-weight:600;">
              <asp:Literal ID="UniversityDisplay" runat="server" />
            </div>
            <asp:HiddenField ID="UniversityValue" runat="server" />
          </div>

          <!-- Event date -->
          <div>
            <label for="EventDate">Event date</label>
            <asp:TextBox ID="EventDate" runat="server" TextMode="Date" />
            <asp:RequiredFieldValidator
              runat="server"
              ControlToValidate="EventDate"
              CssClass="val"
              ErrorMessage="Date is required."
              Display="Dynamic" />
          </div>

          <!-- Event name -->
          <div>
            <label for="EventName">Event name</label>
            <asp:TextBox ID="EventName" runat="server" Placeholder="e.g., Fall Cyberfair 2025" />
            <asp:RequiredFieldValidator
              runat="server"
              ControlToValidate="EventName"
              CssClass="val"
              ErrorMessage="Event name is required."
              Display="Dynamic" />
          </div>

          <!-- Description -->
          <div style="grid-column:1/-1">
            <label for="Description">Description</label>
            <asp:TextBox
              ID="Description"
              runat="server"
              TextMode="MultiLine"
              Placeholder="Brief description for participants and helpers..." />
            <asp:RequiredFieldValidator
              runat="server"
              ControlToValidate="Description"
              CssClass="val"
              ErrorMessage="Description is required."
              Display="Dynamic" />
          </div>
        </div>

        <!-- Submit / Clear buttons -->
        <div class="btnrow">
          <asp:Button
            ID="BtnCreateEvent"
            runat="server"
            Text="Create event"
            CssClass="btn primary"
            OnClick="BtnCreateEvent_Click" />
          <asp:Button
            ID="BtnClear"
            runat="server"
            Text="Clear"
            CssClass="btn link"
            OnClick="BtnClear_Click"
            CausesValidation="false" />
        </div>

        <!-- Inline status/error message area -->
        <asp:Label ID="FormMessage" runat="server" EnableViewState="false" />

        <p></p>
        <p></p>

        <!-- =============================================================
             Your Events (list of existing events for this university)
             ============================================================= -->
        <div class="card">
          <h2>Your events</h2>
          <p class="sub">Manage existing Cyberfairs linked to your university.</p>

          <!-- Empty state shown when there are no events -->
          <asp:PlaceHolder ID="NoEventsPlaceholder" runat="server" Visible="false">
            <div class="note">No events yet. Create one above.</div>
          </asp:PlaceHolder>

          <!-- Events table rendered via Repeater -->
          <asp:Repeater ID="EventsRepeater" runat="server">
            <HeaderTemplate>
              <div style="overflow:auto">
                <table style="width:100%; border-collapse:collapse">
                  <thead>
                    <tr>
                      <th style="text-align:left; padding:8px; border-bottom:1px solid #e8eef7;">Name</th>
                      <th style="text-align:left; padding:8px; border-bottom:1px solid #e8eef7;">Date</th>
                      <th style="text-align:left; padding:8px; border-bottom:1px solid #e8eef7;">Status</th>
                      <th style="text-align:left; padding:8px; border-bottom:1px solid #e8eef7;">Actions</th>
                    </tr>
                  </thead>
                  <tbody>
            </HeaderTemplate>

            <ItemTemplate>
              <tr>
                <td style="padding:8px; border-bottom:1px solid #f0f3f9;"><%# Eval("name") %></td>
                <td style="padding:8px; border-bottom:1px solid #f0f3f9;"><%# Eval("dateHuman") %></td>
                <td style="padding:8px; border-bottom:1px solid #f0f3f9;"><span class="pill"><%# Eval("status") %></span></td>
                <td style="padding:8px; border-bottom:1px solid #f0f3f9;">
                  <a href='<%# Eval("manageUrl") %>'>Manage</a>
                </td>
              </tr>
            </ItemTemplate>

            <FooterTemplate>
                  </tbody>
                </table>
              </div>
            </FooterTemplate>
          </asp:Repeater>
        </div> <!-- /card: Your events -->

      </div> <!-- /card: Create event -->

    </div> <!-- /wrap -->
  </form>
</body>
</html>

