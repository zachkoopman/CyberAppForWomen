<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="SelectEvent.aspx.cs" Inherits="CyberApp_FIA.Participant.SelectEvent" %> 

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
  <title>FIA • Choose your Cyberfair</title>
  <meta name="viewport" content="width=device-width, initial-scale=1" />
  <link href="https://fonts.googleapis.com/css2?family=Lato:wght@300;400&family=Poppins:wght@600&display=swap" rel="stylesheet" />

  <style>
    /* =========================================================================
       Design tokens (brand colors, typography helpers, focus ring)
       ========================================================================= */
    :root{
      --fia-pink:#f06aa9;
      --fia-blue:#2a99db;
      --fia-teal:#45c3b3;
      --ink:#1c1c1c;
      --muted:#6b7280;
      --ring:rgba(42,153,219,.25);
    }

    /* Base layout and typography */
    *{ box-sizing:border-box }
    html,body{ height:100% }
    body{
      margin:0;
      font-family:Lato,Arial,sans-serif;
      background:linear-gradient(135deg,#fff,#f9fbff);
      color:var(--ink);
    }

    /* Centered card container */
    .wrap{
      min-height:100vh;
      display:grid;
      place-items:center;
      padding:24px;
    }

    /* Main card styling */
    .card{
      width:min(680px,100%);
      background:#fff;
      border:1px solid #e8eef7;
      border-radius:20px;
      box-shadow:0 12px 36px rgba(42,153,219,.08);
      padding:24px;
    }

    /* Brand header (badge + title) */
    .brand{
      display:flex;
      align-items:center;
      gap:10px;
      margin-bottom:10px;
    }
    .badge{
      width:40px; height:40px; border-radius:12px;
      background:linear-gradient(135deg,var(--fia-pink),var(--fia-blue));
      display:grid; place-items:center;
      color:#fff; font-family:Poppins;
    }
    h1{ font-family:Poppins; margin:0 0 6px 0; font-size:1.35rem }
    p.sub{ color:var(--muted); margin:.25rem 0 1rem }

    /* Form elements */
    label{ font-weight:600; font-family:Poppins; font-size:.95rem }
    select{
      width:100%;
      padding:12px 14px;
      border-radius:12px;
      border:1px solid #e5e7eb;
      background:#fff;
    }
    select:focus{
      outline:0;
      box-shadow:0 0 0 5px var(--ring);
      border-color:var(--fia-blue);
    }

    /* Layout helpers */
    .row{ margin-bottom:12px }

    /* Buttons */
    .btnrow{ display:flex; gap:10px; margin-top:14px; flex-wrap:wrap }
    .btn{
      border:0; border-radius:12px;
      padding:12px 18px;
      font-weight:700; font-family:Poppins;
      cursor:pointer;
    }
    .primary{ color:#fff; background:linear-gradient(135deg,var(--fia-blue),var(--fia-teal)) }
    .link{ background:#fff; border:2px solid var(--fia-blue); color:var(--fia-blue) }

    /* Validation messages */
    .val{ color:#c21d1d; font-size:.9rem; margin-top:4px }

    /* Informational note */
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
      <div class="card">

        <!-- Brand header -->
        <div class="brand">
          <div class="badge">FIA</div>
          <h1>Choose your Cyberfair</h1>
        </div>
        <p class="sub">Pick your university and active event to get started.</p>

        <!-- ======================= University selection ======================= -->
        <div class="row">
          <label for="UniversitySelect">University</label>
          <!-- AutoPostBack triggers server to refresh the Event list when university changes -->
          <asp:DropDownList
            ID="UniversitySelect"
            runat="server"
            AutoPostBack="true"
            OnSelectedIndexChanged="UniversitySelect_SelectedIndexChanged" />
          <!-- Server-side validator to ensure a university is chosen -->
          <asp:CustomValidator
            ID="UniRequired"
            runat="server"
            CssClass="val"
            OnServerValidate="UniRequired_ServerValidate"
            ErrorMessage="Please select a university."
            Display="Dynamic" />
        </div>

        <!-- ========================= Event selection ========================= -->
        <div class="row">
          <label for="EventSelect">Active Cyberfair event</label>
          <!-- Populated server-side based on the selected university -->
          <asp:DropDownList ID="EventSelect" runat="server" />
          <!-- Server-side validator to ensure an event is chosen -->
          <asp:CustomValidator
            ID="EventRequired"
            runat="server"
            CssClass="val"
            OnServerValidate="EventRequired_ServerValidate"
            ErrorMessage="Please select an event."
            Display="Dynamic" />
        </div>

        <!-- Privacy notice for participants -->
        <div class="note">
          <strong>Privacy:</strong>
          Your learning results are private to you and the FIA team.
          Your university sees only anonymized or aggregated insights—never your personal answers.
        </div>

        <!-- Actions: continue into the app or go back to the welcome page -->
        <div class="btnrow">
          <asp:Button
            ID="BtnContinue"
            runat="server"
            Text="Continue"
            CssClass="btn primary"
            OnClick="BtnContinue_Click" />
          <a class="btn link" href="<%: ResolveUrl("~/Welcome_Page.aspx") %>">Back to home</a>
        </div>

        <!-- General message area (status/errors), viewstate disabled to avoid stale text -->
        <asp:Label ID="FormMessage" runat="server" EnableViewState="false" />

      </div>
    </div>
  </form>
</body>
</html>
