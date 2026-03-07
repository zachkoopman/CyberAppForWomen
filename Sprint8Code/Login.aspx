<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="CyberApp_FIA.Account.Login" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
  <title>FIA • Sign In</title>
  <meta name="viewport" content="width=device-width, initial-scale=1" />
  <link href="https://fonts.googleapis.com/css2?family=Lato:wght@300;400&family=Poppins:wght@600&display=swap" rel="stylesheet" />

  <style>
    /* =========================
       Design tokens / variables
       ========================= */
    :root{
      --fia-pink:#f06aa9;
      --fia-blue:#2a99db;
      --fia-teal:#45c3b3;
      --ink:#1c1c1c;
      --muted:#6b7280;
      --ring:rgba(42,153,219,.25);
    }

    /* =========================
       Base layout and typography
       ========================= */
    *{ box-sizing:border-box }
    html,body{ height:100% }
    body{
      margin:0; padding:0;
      font-family:Lato,Arial,sans-serif;
      background:linear-gradient(135deg,#fff,#f9fbff);
      overflow-x:hidden;
      line-height:1.5;
    }

    /* Centers the sign-in card on the page */
    .wrap{
      min-height:100vh;
      display:grid;
      place-items:center;
      padding:clamp(16px,4vw,24px);
    }

    /* Main card container */
    .card{
      width:min(560px,100%);
      background:#fff;
      border:1px solid #e8eef7;
      border-radius:20px;
      box-shadow:0 12px 36px rgba(42,153,219,.08);
      padding:clamp(20px,4vw,26px);
    }

    /* Brand row (logo badge + title) */
    .brand{
      display:flex;
      align-items:center;
      gap:12px;
      margin-bottom:12px;
      flex-wrap:wrap;
    }
    .badge{
      width:42px;
      height:42px;
      border-radius:12px;
      background:linear-gradient(135deg,var(--fia-pink),var(--fia-blue));
      display:grid;
      place-items:center;
      color:#fff;
      font-family:Poppins;
      font-size:.9rem;
    }
    h1{
      font-family:Poppins,sans-serif;
      font-size:clamp(1.3rem,2.6vw,1.5rem);
      margin:0;
    }
    p.sub{
      color:var(--muted);
      margin:.25rem 0 1.1rem;
    }

    /* Inputs and labels */
    label{
      font-weight:600;
      font-family:Poppins;
      font-size:.95rem;
      display:block;
      margin-bottom:4px;
    }
    input[type=text],
    input[type=email],
    input[type=password]{
      width:100%;
      padding:14px 16px;
      border-radius:12px;
      border:1px solid #e5e7eb;
      font-size:1rem;
      min-height:44px;
    }
    input:focus{
      outline:0;
      box-shadow:0 0 0 5px var(--ring);
      border-color:var(--fia-blue);
    }

    /* Spacing helpers */
    .row{ margin-bottom:14px }

    /* Primary / secondary buttons */
    .btnrow{
      display:flex;
      gap:12px;
      margin-top:16px;
      flex-wrap:wrap;
    }
    .btn{
      border:0;
      border-radius:12px;
      padding:14px 20px;
      font-weight:700;
      font-family:Poppins;
      cursor:pointer;
      min-height:44px;
    }
    .primary{ color:#fff; background:linear-gradient(135deg,var(--fia-blue),var(--fia-teal)) }
    .link{ background:#fff; border:2px solid var(--fia-blue); color:var(--fia-blue) }

    /* Validation message text */
    .val{
      color:#c21d1d;
      font-size:.9rem;
      margin-top:4px;
    }

    /* Footer row under the buttons */
    .foot{
      display:flex;
      justify-content:space-between;
      align-items:center;
      margin-top:10px;
      color:var(--muted);
      font-size:.9rem;
      flex-wrap:wrap;
      gap:6px;
    }

    /* Anchor defaults */
    a{ color:var(--fia-blue); text-decoration:none; min-height:44px;display:inline-flex;align-items:center;}
    a:hover{ text-decoration:underline }

    /* MOBILE OPTIMIZATION */

    @media (max-width:430px){
      .card{border-radius:16px;}
      .btnrow{flex-direction:column;}
      .btn{width:100%;}
      .foot{flex-direction:column;align-items:flex-start;}

    }

  </style>
</head>

<body>
  <form id="form1" runat="server">
    <div class="wrap">
      <div class="card">

        <!-- Header / page title -->
        <div class="brand">
          <div class="badge">FIA</div>
          <h1>Welcome back</h1>
        </div>
        <p class="sub">Click with confidence—sign in to continue your progress.</p>

        <!-- Email field -->
        <div class="row">
          <label for="Email">Email</label>
          <asp:TextBox ID="Email" runat="server" TextMode="Email" />
          <asp:RequiredFieldValidator
            CssClass="val"
            ControlToValidate="Email"
            runat="server"
            ErrorMessage="Email is required."
            Display="Dynamic" />
        </div>

        <!-- Password field -->
        <div class="row">
          <label for="Password">Password</label>
          <asp:TextBox ID="Password" runat="server" TextMode="Password" />
          <asp:RequiredFieldValidator
            CssClass="val"
            ControlToValidate="Password"
            runat="server"
            ErrorMessage="Password is required."
            Display="Dynamic" />
        </div>

        <!-- Actions: sign in or go to create account (no validation on create) -->
        <div class="btnrow">
          <asp:Button
            ID="BtnLogin"
            runat="server"
            Text="Sign in"
            CssClass="btn primary"
            OnClick="BtnLogin_Click" />
          <asp:Button
            ID="BtnCreate"
            runat="server"
            Text="Create account"
            CssClass="btn link"
            CausesValidation="false"
            PostBackUrl="~/Account/CreateAccountPage.aspx" />
        </div>

        <!-- Aux links (e.g., password reset) -->
        <div class="foot">
          <span><a href="#">Forgot password?</a></span>
        </div>

        <!-- General message area for success/errors; viewstate off to avoid stale text -->
        <asp:Label ID="FormMessage" runat="server" EnableViewState="false" />

      </div>
    </div>
  </form>
</body>
</html>


