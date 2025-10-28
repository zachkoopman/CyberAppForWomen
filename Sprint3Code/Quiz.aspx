<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Quiz.aspx.cs" Inherits="CyberApp_FIA.Account.Participant.Quiz" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
  <title>FIA • Cybersecurity Quiz</title>
  <meta name="viewport" content="width=device-width, initial-scale=1" />
  <link href="https://fonts.googleapis.com/css2?family=Lato:wght@300;400&family=Poppins:wght@500&display=swap" rel="stylesheet" />
  <style>
    :root{
      --fia-pink:#f06aa9; --fia-blue:#2a99db; --fia-teal:#45c3b3;
      --ink:#1c1c1c; --muted:#6b7280; --ring:rgba(42,153,219,.25);
      --bg:#f9fbff; --card:#ffffff; --progress:#2a99db;
    }
    body{margin:0;background:linear-gradient(135deg,#fff,var(--bg));font-family:Lato,Arial,sans-serif;color:var(--ink)}
    .wrap{max-width:860px;margin:24px auto;padding:20px}
    .card{background:var(--card);border-radius:16px;box-shadow:0 8px 30px rgba(0,0,0,.08);padding:24px}
    h1{font-family:Poppins,Arial,sans-serif;font-weight:500;margin:0 0 6px}
    .sub{color:var(--muted);margin-bottom:18px}
    .progress{height:10px;background:#e9eef7;border-radius:999px;overflow:hidden;margin:8px 0 18px}
    .bar{height:100%;background:var(--progress);width:0%}
    .q{font-size:1.15rem;margin:8px 0 16px}
    .opt{display:block;margin:10px 0;padding:12px 14px;border:1px solid #e5e7eb;border-radius:12px;cursor:pointer}
    .opt:hover{border-color:var(--fia-blue);box-shadow:0 0 0 4px var(--ring)}
    .controls{display:flex;gap:12px;margin-top:16px}
    .btn{padding:10px 14px;border-radius:12px;border:0;background:var(--fia-blue);color:#fff;cursor:pointer}
    .btn.secondary{background:#e5e7eb;color:#111}
    .summary{margin-top:12px;padding:14px;border-radius:12px;background:#f7fbff;border:1px solid #e3f0fb}
    .brand{display:flex;align-items:center;gap:10px;margin-bottom:8px}
    .badge{display:inline-block;padding:4px 10px;border-radius:999px;background:var(--fia-teal);color:#fff;font-size:.8rem;margin-left:auto}
    .disclaimer{font-size:.9rem;color:var(--muted);margin-top:8px}
  </style>
</head>
<body>
<form id="form1" runat="server" enableviewstate="true">
  <div class="wrap">
    <div class="card">
      <div class="brand">
        <h1>Cybersecurity Check-In</h1>
        <span class="badge" id="BadgeSaved" runat="server" visible="false">Saved</span>
      </div>
      <div class="sub">Short, plain-language quiz to tailor your experience. You can exit anytime—your progress is saved.</div>
      <div class="progress"><div id="ProgressBar" runat="server" class="bar"></div></div>

      <asp:Panel ID="PanelQuestion" runat="server">
        <div class="q"><asp:Label ID="LblQuestionText" runat="server" /></div>

        <asp:RadioButtonList ID="Options" runat="server" AutoPostBack="true" CssClass="opts"
          OnSelectedIndexChanged="Options_SelectedIndexChanged" RepeatDirection="Vertical">
          <asp:ListItem Value="A" class="opt">A</asp:ListItem>
          <asp:ListItem Value="B" class="opt">B</asp:ListItem>
          <asp:ListItem Value="C" class="opt">C</asp:ListItem>
          <asp:ListItem Value="D" class="opt">D</asp:ListItem>
        </asp:RadioButtonList>

        <div class="summary">
          <span id="LblQuestionNumber" runat="server"></span>
          <span class="disclaimer">Your answers are private by default.</span>
        </div>

        <div class="controls">
          <asp:Button ID="BtnPrev" runat="server" Text="Back" CssClass="btn secondary" OnClick="BtnPrev_Click" />
          <asp:Button ID="BtnNext" runat="server" Text="Next" CssClass="btn" OnClick="BtnNext_Click" />
          <asp:Button ID="BtnFinish" runat="server" Text="Finish" CssClass="btn" OnClick="BtnFinish_Click" Visible="false" />
        </div>

        

      </asp:Panel>

      <asp:Panel ID="PanelComplete" runat="server" Visible="false">
        <h1>Your Score</h1>
        <div class="summary">
          <asp:Label ID="LblOverall" runat="server"></asp:Label>
          <br />
          <asp:Repeater ID="RptDomains" runat="server">
            <ItemTemplate>
              • <%# Eval("Key") %>: <strong><%# Eval("Value") %></strong><br />
            </ItemTemplate>
          </asp:Repeater>
          <hr />
          <div><strong>Top 3 factors:</strong><br />
            <asp:BulletedList ID="BlFactors" runat="server"></asp:BulletedList>
          </div>
          <div style="margin-top:8px"><strong>Quick wins:</strong><br />
            <asp:BulletedList ID="BlWins" runat="server"></asp:BulletedList>
          </div>
        </div>

        <div class="summary">
          <strong>Sharing (optional):</strong><br />
          <asp:CheckBox ID="ChkShare" runat="server" Text="Share my results with my assigned Helper for tailored support" />
          <div class="disclaimer">You can revoke sharing anytime from your home screen widget.</div>
          <div class="controls" style="margin-top:10px">
            <asp:Button ID="BtnSaveShare" runat="server" Text="Save & Go to My Home" CssClass="btn" OnClick="BtnSaveShare_Click" />
          </div>
        </div>
      </asp:Panel>
    </div>
  </div>
</form>
</body>
</html>

