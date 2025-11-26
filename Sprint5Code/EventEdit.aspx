<%@ Page Language="C#" AutoEventWireup="true"
    CodeBehind="CourseEdit.aspx.cs"
    Inherits="CyberApp_FIA.Account.SuperAdminCourseEdit" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
  <title>FIA • Super Admin Course Editor</title>
  <meta name="viewport" content="width=device-width, initial-scale=1" />
  <link href="https://fonts.googleapis.com/css2?family=Lato:wght@300;400&family=Poppins:wght@600&display=swap"
        rel="stylesheet" />
  <style>
    :root{
      --fia-pink:#f06aa9;--fia-blue:#2a99db;--fia-teal:#45c3b3;
      --ink:#111827;--muted:#6b7280;--border:#e5e7eb;--ring:rgba(42,153,219,.25);
      --card:#ffffff;--bg:#f9fafb;
    }
    *{box-sizing:border-box;}
    body{margin:0;font-family:'Lato',sans-serif;background:var(--bg);color:var(--ink);}
    .wrap{max-width:720px;margin:0 auto;padding:24px 16px 40px;}
    h1{font-family:'Poppins',sans-serif;font-size:1.4rem;margin:0 0 4px;}
    .sub{font-size:0.9rem;color:var(--muted);margin:0 0 16px;}
    .card{background:var(--card);border-radius:18px;padding:18px 20px;border:1px solid var(--border);
          box-shadow:0 10px 30px rgba(15,23,42,.06);}
    .field{margin-bottom:12px;}
    label{display:block;font-size:0.85rem;font-weight:600;margin-bottom:4px;}
    input[type=text], textarea, select{
      width:100%;padding:8px 10px;border-radius:8px;border:1px solid var(--border);
      font-size:0.9rem;font-family:'Lato',sans-serif;
    }
    input[type=text]:focus, textarea:focus, select:focus{
      outline:none;border-color:var(--fia-blue);box-shadow:0 0 0 2px var(--ring);
    }
    textarea{min-height:80px;resize:vertical;}
    .inline{
      display:flex;align-items:center;gap:8px;font-size:0.85rem;color:var(--muted);
    }
    .inline input[type=checkbox]{width:auto;}
    .actions{margin-top:16px;display:flex;gap:10px;align-items:center;}
    .btn{border:none;border-radius:999px;padding:8px 16px;font-size:0.9rem;cursor:pointer;}
    .btn-primary{background:linear-gradient(135deg,var(--fia-pink),var(--fia-blue));color:#fff;}
    .btn-ghost{background:transparent;border:1px dashed var(--border);color:var(--muted);}
    .status{font-size:0.85rem;color:var(--muted);}
    .status.error{color:#b91c1c;}
  </style>
</head>
<body>
  <form id="form1" runat="server">
    <div class="wrap">
      <h1>Course Editor (Super Admin)</h1>
      <p class="sub">
        Create or edit course entries for the FIA catalog. Titles must be unique.
        All changes are logged in the Activity Log for a trusted history.
      </p>

      <div class="card">
        <asp:HiddenField ID="CourseId" runat="server" />

        <div class="field">
          <label for="OwnerUniversity">Owner university</label>
          <asp:TextBox ID="OwnerUniversity" runat="server" placeholder="e.g., Arizona State University" />
        </div>

        <div class="field">
          <label for="ShortCode">Course code</label>
          <asp:TextBox ID="ShortCode" runat="server" placeholder="e.g., FIA-101" />
        </div>

        <div class="field">
          <label for="Title">Title</label>
          <asp:TextBox ID="Title" runat="server" placeholder="Enhancing Social Media Privacy Settings" />
        </div>

        <div class="field">
          <label for="Description">Description</label>
          <asp:TextBox ID="Description" runat="server" TextMode="MultiLine"
                       placeholder="Short description for admins and catalog views." />
        </div>

        <div class="field inline">
          <asp:CheckBox ID="IsPublished" runat="server" />
          <span>Published to admin catalogs (participants see only published courses).</span>
        </div>

        <div class="actions">
          <asp:Button ID="BtnSave" runat="server"
                      Text="Save course"
                      CssClass="btn btn-primary"
                      OnClick="BtnSave_Click" />
          <asp:Button ID="BtnCancel" runat="server"
                      Text="Cancel"
                      CssClass="btn btn-ghost"
                      CausesValidation="false"
                      OnClick="BtnCancel_Click" />
          <asp:Label ID="StatusLabel" runat="server" CssClass="status" />
        </div>
      </div>
    </div>
  </form>
</body>
</html>
