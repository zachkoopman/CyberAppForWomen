<%@ Page Language="C#" AutoEventWireup="true"
    CodeBehind="UniversityAdminHelperAudit.aspx.cs"
    Inherits="CyberApp_FIA.Account.UniversityAdminHelperAudit" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
  <title>FIA • Helper Audit Detail</title>
  <meta name="viewport" content="width=device-width, initial-scale=1" />
  <link href="https://fonts.googleapis.com/css2?family=Lato:wght@300;400&family=Poppins:wght@600&display=swap" rel="stylesheet" />

  <style>
    :root{
      --fia-pink:#f06aa9;
      --fia-blue:#2a99db;
      --fia-teal:#45c3b3;
      --ink:#1c1c1c;
      --muted:#6b7280;
      --bg:#f3f5fb;
      --card-bg:#ffffff;
      --card-border:#e2e8f0;
      --ring:rgba(42,153,219,.25);
    }

    *{ box-sizing:border-box }
    html,body{ height:100% }
    body{
      margin:0;
      font-family:Lato,Arial,sans-serif;
      background:
        radial-gradient(circle at 0 0, rgba(240,106,169,0.10), transparent 55%),
        radial-gradient(circle at 100% 100%, rgba(42,153,219,0.10), transparent 55%),
        var(--bg);
      color:var(--ink);
    }

    .wrap{
      min-height:100vh;
      padding:24px 16px 40px;
      max-width:1100px;
      margin:0 auto;
    }

    /* ---------- Hero header ---------- */
    .page-header{
      border-radius:24px;
      padding:20px 22px 22px;
      background:
        radial-gradient(circle at 0 0, rgba(240,106,169,0.15), transparent 55%),
        radial-gradient(circle at 100% 0, rgba(69,195,179,0.20), transparent 55%),
        linear-gradient(120deg,#fbfbff,#f3f7ff);
      border:1px solid rgba(226,232,240,0.9);
      box-shadow:0 18px 40px rgba(15,23,42,0.10);
      margin-bottom:24px;
      display:flex;
      justify-content:space-between;
      align-items:flex-start;
      gap:18px;
    }

    .page-header-main{
      display:flex;
      gap:10px;
      align-items:flex-start;
      max-width:640px;
    }

    .badge{
      width:42px;
      height:42px;
      border-radius:12px;
      background:linear-gradient(135deg,var(--fia-pink),var(--fia-blue));
      display:grid;
      place-items:center;
      color:#fff;
      font-family:Poppins,system-ui,sans-serif;
      font-weight:700;
      font-size:.9rem;
      flex-shrink:0;
    }

    .page-header-text h1{
      font-family:Poppins, system-ui, sans-serif;
      margin:0;
      font-size:1.5rem;
      background:linear-gradient(135deg,var(--fia-blue),var(--fia-pink));
      -webkit-background-clip:text;
      color:transparent;
    }

    .page-sub{
      margin:6px 0 0 0;
      color:var(--muted);
      font-size:.95rem;
    }

    .page-admin-line{
      margin-top:8px;
      font-size:0.9rem;
      color:#4b5563;
    }

    .page-header-side{
      display:flex;
      flex-direction:column;
      gap:10px;
      align-items:flex-end;
      flex-shrink:0;
    }

    .page-chip{
      padding:8px 14px;
      border-radius:999px;
      border:1px solid rgba(42,153,219,0.55);
      background:
        radial-gradient(circle at 0 0, rgba(240,106,169,0.18), transparent 60%),
        radial-gradient(circle at 100% 100%, rgba(69,195,179,0.22), transparent 55%),
        #f0f9ff;
      font-size:.8rem;
      color:#0f172a;
      max-width:260px;
      text-align:center;
    }

    .header-btn-row{
      display:flex;
      gap:8px;
      flex-wrap:wrap;
      justify-content:flex-end;
    }

    @media (max-width:800px){
      .page-header{
        flex-direction:column;
        align-items:flex-start;
      }
      .page-header-main{
        align-items:flex-start;
      }
      .page-header-side{
        align-items:flex-start;
      }
      .page-chip{
        text-align:left;
      }
    }

    /* ---------- Cards & common UI ---------- */
    .card{
      background:var(--card-bg);
      border:1px solid var(--card-border);
      border-radius:20px;
      box-shadow:0 12px 36px rgba(42,153,219,.08);
      padding:20px;
      margin-bottom:16px;
    }
    .card h2{
      font-family:Poppins;
      margin:0 0 8px 0;
      font-size:1.15rem;
    }
    .sub{
      color:var(--muted);
      margin:0 0 12px 0;
      font-size:.9rem;
    }

    label{
      font-weight:600;
      font-family:Poppins;
      font-size:.9rem;
      display:block;
      margin-bottom:4px;
    }
    input[type=text], select{
      width:100%;
      padding:10px 12px;
      border-radius:12px;
      border:1px solid #e5e7eb;
      font-family:inherit;
      font-size:.9rem;
    }
    input:focus, select:focus{
      outline:0;
      box-shadow:0 0 0 5px var(--ring);
      border-color:var(--fia-blue);
    }

    .btn{
      border-radius:12px;
      padding:10px 16px;
      font-weight:700;
      font-family:Poppins;
      cursor:pointer;
      font-size:.85rem;
      text-decoration:none;
      display:inline-flex;
      align-items:center;
      justify-content:center;
      white-space:nowrap;
      border:0;
    }
    .primary{
      color:#fff;
      background:linear-gradient(135deg,var(--fia-blue),var(--fia-teal));
      box-shadow:0 10px 26px rgba(37,99,235,0.18);
    }
    .link{
      background:#fff;
      border:2px solid var(--fia-blue);
      color:var(--fia-blue);
      box-shadow:0 8px 18px rgba(15,23,42,0.06);
    }

    .note{
      background:#f6f7fb;
      border:1px solid #e8eef7;
      border-radius:12px;
      padding:12px;
      color:var(--muted);
      font-size:.9rem;
    }

    /* Certification verification cards */
    .verification-list{
      margin-top:12px;
      display:flex;
      flex-direction:column;
      gap:10px;
    }

    .verification-card{
      border-radius:16px;
      border:1px solid #e5e7eb;
      padding:10px 12px;
      background:#ffffff;
      display:flex;
      align-items:flex-start;
      justify-content:space-between;
      gap:10px;
    }

    .verification-card.pending{
      border-color:#fde68a;
      background:#fffbeb;
    }

    .verification-card.verified{
      border-color:#bbf7d0;
      background:#ecfdf3;
    }

    .verification-card.questioned{
      border-color:#fecaca;
      background:#fef2f2;
    }

    /* Visually flag resubmissions */
    .verification-card.resubmission{
      border-style:dashed;
    }

    .verification-main{
      font-size:0.88rem;
      color:var(--ink);
      flex:1 1 auto;
    }

    .verification-title{
      font-family:Poppins, system-ui, sans-serif;
      font-size:0.95rem;
      font-weight:600;
      margin-bottom:2px;
    }

    .verification-status{
      font-size:0.78rem;
      font-weight:600;
      text-transform:uppercase;
      letter-spacing:0.08em;
      color:#6b7280;
      margin-bottom:4px;
      display:flex;
      align-items:center;
      gap:6px;
      flex-wrap:wrap;
    }

    .submission-pill{
      display:inline-flex;
      align-items:center;
      padding:2px 8px;
      border-radius:999px;
      font-size:0.7rem;
      font-weight:600;
      background:rgba(59,130,246,0.08);
      color:#1d4ed8;
      border:1px solid rgba(59,130,246,0.25);
    }

    .verification-meta{
      font-size:0.8rem;
      color:var(--muted);
      margin-top:2px;
    }

    /* Helper note chip shown to admin */
    .helper-note-chip{
      margin-top:6px;
      padding:6px 8px;
      border-radius:10px;
      background:#eff6ff;
      color:#1d4ed8;
      font-size:0.8rem;
    }
    .helper-note-label{
      font-weight:600;
      margin-right:4px;
    }

    .verification-actions{
      display:flex;
      flex-direction:column;
      gap:6px;
      align-items:flex-end;
      font-size:0.8rem;
      flex:0 0 240px;
    }

    .verification-actions .btn{
      font-size:0.78rem;
      padding:7px 12px;
    }

    .verification-actions-label{
      font-weight:600;
      font-size:0.78rem;
      color:#4b5563;
      align-self:flex-start;
    }

    .admin-note-input{
      width:100%;
      min-height:64px;
      border-radius:10px;
      border:1px solid #e5e7eb;
      padding:6px 8px;
      font-family:inherit;
      font-size:0.8rem;
      resize:vertical;
    }

    .helper-header{
      display:flex;
      align-items:flex-start;
      justify-content:space-between;
      gap:16px;
      margin-bottom:8px;
    }
    .helper-meta{
      display:flex;
      flex-direction:column;
      gap:4px;
    }
    .helper-name{
      font-family:Poppins;
      font-weight:600;
      font-size:1.05rem;
    }
    .helper-email{
      font-size:.9rem;
      color:var(--muted);
    }
    .helper-uni{
      font-size:.85rem;
      color:var(--muted);
    }
    .pill{
      display:inline-block;
      padding:2px 10px;
      border-radius:999px;
      font-size:.75rem;
      font-weight:600;
      background:rgba(42,153,219,.08);
      color:#2563eb;
      margin-left:6px;
    }

    .log-filters{
      display:flex;
      flex-wrap:wrap;
      gap:12px;
      margin:12px 0;
    }
    .log-filters .field{
      flex:1 1 150px;
      min-width:140px;
    }
    .log-filters .field-buttons{
      display:flex;
      align-items:flex-end;
      gap:8px;
      flex-wrap:wrap;
    }

    .log-table{
      border-radius:16px;
      border:1px solid #e8eef7;
      background:#fff;
      max-height:320px;
      overflow-y:auto;
      overflow-x:hidden;
    }

    .log-table table{
      width:100%;
      border-collapse:collapse;
      font-size:.85rem;
      table-layout:fixed;
    }

    .log-table th,
    .log-table td{
      padding:8px 10px;
      border-bottom:1px solid #eef1f8;
      text-align:left;
      vertical-align:top;
    }

    .log-table thead th{
      position:sticky;
      top:0;
      z-index:1;
      background:#f3f5fb;
    }

    .log-table th:nth-child(1),
    .log-table td:nth-child(1){ width:110px; }
    .log-table th:nth-child(2),
    .log-table td:nth-child(2){ width:130px; }
    .log-table th:nth-child(3),
    .log-table td:nth-child(3){ width:auto; }

    .details-cell{
      white-space:normal;
      word-break:break-word;
      overflow-wrap:anywhere;
    }

    .pagination{
      display:flex;
      align-items:center;
      justify-content:flex-end;
      gap:10px;
      margin-top:10px;
      font-size:.85rem;
      color:var(--muted);
    }
    .page-link{
      background:#fff;
      border-radius:999px;
      padding:6px 12px;
      border:1px solid #e5e7eb;
      cursor:pointer;
      font-size:.85rem;
    }
    .page-link[disabled],
    .page-link[aria-disabled="true"]{
      opacity:.45;
      cursor:default;
    }
    .page-info{
      font-size:.85rem;
    }

    @media (max-width:800px){
      .helper-header{
        flex-direction:column;
      }
      .verification-card{
        flex-direction:column;
      }
      .verification-actions{
        align-items:stretch;
        flex:1 1 auto;
      }
    }
  </style>

  <script type="text/javascript">
      document.addEventListener('DOMContentLoaded', function () {
          ['<%=TxtFromTime.ClientID%>', '<%=TxtToTime.ClientID%>'].forEach(function (id) {
              var el = document.getElementById(id);
              if (el) {
                  el.setAttribute('type', 'datetime-local');
              }
          });
      });
  </script>
</head>
<body>
  <form id="form1" runat="server">
    <div class="wrap">

      <!-- Hero header -->
      <div class="page-header">
        <div class="page-header-main">
          <div class="badge">FIA</div>
          <div class="page-header-text">
            <h1>Helper Audit Timeline</h1>
            <p class="page-sub">
              Review this helper’s timeline across quizzes, teaching sessions, and 1:1 help to support safe,
              fair certification decisions.
            </p>
            <div class="page-admin-line">
              Signed in as <strong><asp:Literal ID="WelcomeName" runat="server" /></strong>
            </div>
          </div>
        </div>

        <div class="page-header-side">
          <div class="page-chip">
            Shows audit entries only for this helper account within your university.
          </div>
          <div class="header-btn-row">
            <asp:Button
              ID="BtnBack"
              runat="server"
              Text="Back to university audit"
              CssClass="btn link"
              OnClick="BtnBack_Click"
              CausesValidation="false" />
          </div>
        </div>
      </div>

      <!-- Helper context -->
      <div class="card">
        <div class="helper-header">
          <div class="helper-meta">
            <div class="helper-name">
              <asp:Literal ID="HelperName" runat="server" />
              <span class="pill">Helper</span>
            </div>
            <div class="helper-email">
              <asp:Literal ID="HelperEmailLiteral" runat="server" />
            </div>
            <div class="helper-uni">
              University:
              <asp:Literal ID="HelperUniversityLiteral" runat="server" />
            </div>
          </div>
        </div>

        <p class="sub">
          This view scopes the audit log to this single helper. Use it to review their activity,
          spot unusual patterns, and support certification decisions.
        </p>

        <div class="note">
          Only audit entries where this account appears as the Helper role are shown here
          (sign-ins, quiz completions, teaching logs, 1:1 help logs, etc.).
        </div>

        <asp:HiddenField ID="UniversityValue" runat="server" />
        <asp:HiddenField ID="HelperIdValue" runat="server" />
      </div>

      <!-- Certification verification + session review -->
      <div class="card">
        <h2>Certification verification</h2>
        <p class="sub">
          When a helper says they’ve reviewed all materials and passed the quiz for a microcourse,
          a request appears here. Spot-check their logs across systems before marking it verified or questioned.
        </p>

        <div class="note">
          Check this helper’s activity in:
          <ul style="margin:8px 0 0 18px; padding:0;">
            <li>the helper audit timeline below (sign-ins, quiz/material completion, teaching and 1:1 logs);</li>
            <li>Google Classroom or your LMS (attendance, assignments, and grades for this microcourse);</li>
            <li>Zoom or other video tools (meeting history and recurring session links);</li>
            <li>email or chat threads where sessions were scheduled.</li>
          </ul>
          Use “Verified” when the records line up, or “Questioned” if something doesn’t match and needs follow-up.
          For questioned items, leave a short note so the helper knows what to review before resubmitting.
        </div>

        <asp:PlaceHolder ID="VerificationListPlaceholder" runat="server" Visible="false">
          <div class="verification-list">
            <asp:Repeater ID="VerificationRepeater" runat="server" OnItemCommand="VerificationRepeater_ItemCommand">
              <ItemTemplate>
                <div class="verification-card <%# Eval("CssClass") %>">
                  <div class="verification-main">
                    <div class="verification-status">
                      <%# Eval("StatusLabel") %>
                      <span class="submission-pill"><%# Eval("SubmissionLabel") %></span>
                    </div>
                    <div class="verification-title"><%# Eval("CourseTitle") %></div>
                    <div>
                      Helper reported: reviewed resources and passed the quiz for this microcourse.
                    </div>
                    <div class="verification-meta">
                      Last updated: <%# Eval("LastUpdatedLabel") %>
                    </div>

                    <!-- Helper's note visible to admin on resubmission or initial -->
                    <asp:PlaceHolder ID="HelperNotePH" runat="server"
                      Visible='<%# (bool)Eval("HasHelperNote") %>'>
                      <div class="helper-note-chip">
                        <span class="helper-note-label">Helper note:</span>
                        <span class="helper-note-text"><%# Eval("HelperNote") %></span>
                      </div>
                    </asp:PlaceHolder>
                  </div>

                  <div class="verification-actions">
                    
                    <span class="verification-actions-label">Note to helper</span>
                    <asp:TextBox
                      ID="TxtAdminNote"
                      runat="server"
                      CssClass="admin-note-input"
                      TextMode="MultiLine"
                      Rows="3"
                      Text='<%# Eval("AdminNote") %>'
                      Placeholder="Add a short note on why you verified or questioned this." />
                    <asp:Button
                      ID="BtnMarkVerified"
                      runat="server"
                      CssClass="btn primary"
                      Text="Mark verified"
                      CommandName="markVerified"
                      CommandArgument='<%# Eval("CourseId") %>' />
                    <asp:Button
                      ID="BtnMarkQuestioned"
                      runat="server"
                      CssClass="btn link"
                      Text="Mark questioned"
                      CommandName="markQuestioned"
                      CommandArgument='<%# Eval("CourseId") %>' />
                  </div>
                </div>
              </ItemTemplate>
            </asp:Repeater>
          </div>
        </asp:PlaceHolder>

        <!-- Teaching & 1:1 session review (course-level) -->
        <asp:PlaceHolder ID="SessionReviewPlaceholder" runat="server" Visible="false">
          <h3 style="margin-top:18px; font-family:Poppins; font-size:1rem;">
            Teaching &amp; 1:1 session review
          </h3>
          <p class="sub" style="margin-bottom:10px;">
            Use this section to spot-check teaching and 1:1 logs for each microcourse.
            Questioning a log temporarily removes one session from the helper’s progress
            for that course until you verify it again.
          </p>

          <div class="verification-list">
            <asp:Repeater ID="SessionReviewRepeater" runat="server" OnItemCommand="SessionReviewRepeater_ItemCommand">
              <ItemTemplate>
                <div class="verification-card pending">
                  <div class="verification-main">
                    <div class="verification-title"><%# Eval("CourseTitle") %></div>
                    <div class="verification-meta">
                      Teaching sessions logged:
                      <strong><%# Eval("TeachingSessions") %></strong>
                      &nbsp;•&nbsp;
                      1:1 help sessions logged:
                      <strong><%# Eval("HelpSessions") %></strong>
                    </div>

                    <!-- Teaching status + note -->
                    <asp:PlaceHolder ID="TeachingRowPH" runat="server" Visible='<%# (bool)Eval("HasTeaching") %>'>
                      <div style="margin-top:8px; font-size:0.82rem;">
                        <strong>Teaching status:</strong>
                        <span style='<%# Eval("TeachingStatusCss") %>'>
                          <%# Eval("TeachingStatusLabel") %>
                        </span>
                      </div>
                      <asp:PlaceHolder ID="TeachingNotePH" runat="server"
                        Visible='<%# !string.IsNullOrWhiteSpace((string)Eval("TeachingAdminNote")) %>'>
                        <div class="helper-note-chip" style="margin-top:4px;">
                          <span class="helper-note-label">Admin note (teaching):</span>
                          <span><%# Eval("TeachingAdminNote") %></span>
                        </div>
                      </asp:PlaceHolder>
                    </asp:PlaceHolder>

                    <!-- Help status + note -->
                    <asp:PlaceHolder ID="HelpRowPH" runat="server" Visible='<%# (bool)Eval("HasHelp") %>'>
                      <div style="margin-top:8px; font-size:0.82rem;">
                        <strong>1:1 help status:</strong>
                        <span style='<%# Eval("HelpStatusCss") %>'>
                          <%# Eval("HelpStatusLabel") %>
                        </span>
                      </div>
                      <asp:PlaceHolder ID="HelpNotePH" runat="server"
                        Visible='<%# !string.IsNullOrWhiteSpace((string)Eval("HelpAdminNote")) %>'>
                        <div class="helper-note-chip" style="margin-top:4px;">
                          <span class="helper-note-label">Admin note (1:1 help):</span>
                          <span><%# Eval("HelpAdminNote") %></span>
                        </div>
                      </asp:PlaceHolder>
                    </asp:PlaceHolder>
                  </div>

                  <div class="verification-actions">
                    <!-- One note box shared for this course; you can aim it at either action -->
                    <span class="verification-actions-label">Note for this course</span>
                    <asp:TextBox
                      ID="TxtSessionReviewNote"
                      runat="server"
                      CssClass="admin-note-input"
                      TextMode="MultiLine"
                      Rows="3"
                      Placeholder="Add a short note about why you verified or questioned a teaching or 1:1 log." />

                    <asp:PlaceHolder ID="TeachingButtonsPH" runat="server" Visible='<%# (bool)Eval("HasTeaching") %>'>
                      <asp:Button
                        ID="BtnVerifyTeaching"
                        runat="server"
                        CssClass="btn primary"
                        Text="Verify teaching"
                        CommandName="verifyTeaching"
                        CommandArgument='<%# Eval("CourseId") %>' />
                      <asp:Button
                        ID="BtnQuestionTeaching"
                        runat="server"
                        CssClass="btn link"
                        Text="Question teaching"
                        CommandName="questionTeaching"
                        CommandArgument='<%# Eval("CourseId") %>' />
                    </asp:PlaceHolder>

                    <asp:PlaceHolder ID="HelpButtonsPH" runat="server" Visible='<%# (bool)Eval("HasHelp") %>'>
                      <asp:Button
                        ID="BtnVerifyHelp"
                        runat="server"
                        CssClass="btn primary"
                        Text="Verify 1:1 help"
                        CommandName="verifyHelp"
                        CommandArgument='<%# Eval("CourseId") %>' />
                      <asp:Button
                        ID="BtnQuestionHelp"
                        runat="server"
                        CssClass="btn link"
                        Text="Question 1:1 help"
                        CommandName="questionHelp"
                        CommandArgument='<%# Eval("CourseId") %>' />
                    </asp:PlaceHolder>
                  </div>
                </div>
              </ItemTemplate>
            </asp:Repeater>
          </div>
        </asp:PlaceHolder>

        <!-- Per-log quiz / teaching / 1:1 views -->
        <asp:PlaceHolder ID="QuizLogsPlaceholder" runat="server" Visible="false">
          <h3 style="margin-top:20px; font-family:Poppins; font-size:1rem;">
            Quiz &amp; materials completion logs
          </h3>
          <p class="sub" style="margin-bottom:10px;">
            Each row is a single time this helper completed quiz or materials work for a microcourse.
          </p>
          <div class="verification-list">
            <asp:Repeater ID="QuizLogRepeater" runat="server">
              <ItemTemplate>
                <div class="verification-card">
                  <div class="verification-main">
                    <div class="verification-title"><%# Eval("CourseTitle") %></div>
                    <div class="verification-meta">
                      Logged: <%# Eval("WhenLabel") %>
                    </div>
                    <asp:PlaceHolder ID="QuizDetailsPH" runat="server"
                      Visible='<%# !string.IsNullOrWhiteSpace((string)Eval("DetailsPreview")) %>'>
                      <div style="margin-top:4px; font-size:0.8rem; color:var(--muted);">
                        <%# Eval("DetailsPreview") %>
                      </div>
                    </asp:PlaceHolder>
                  </div>
                </div>
              </ItemTemplate>
            </asp:Repeater>
          </div>
        </asp:PlaceHolder>

        <asp:PlaceHolder ID="TeachingLogsPlaceholder" runat="server" Visible="false">
          <h3 style="margin-top:20px; font-family:Poppins; font-size:1rem;">
            Teaching session logs
          </h3>
          <p class="sub" style="margin-bottom:10px;">
            One row per teaching session the helper logged from their schedule screen.
          </p>
          <div class="verification-list">
            <asp:Repeater ID="TeachingLogRepeater" runat="server">
              <ItemTemplate>
                <div class="verification-card">
                  <div class="verification-main">
                    <div class="verification-title"><%# Eval("CourseTitle") %></div>
                    <div class="verification-meta">
                      Logged: <%# Eval("WhenLabel") %>
                    </div>
                    <asp:PlaceHolder ID="TeachDetailsPH" runat="server"
                      Visible='<%# !string.IsNullOrWhiteSpace((string)Eval("DetailsPreview")) %>'>
                      <div style="margin-top:4px; font-size:0.8rem; color:var(--muted);">
                        <%# Eval("DetailsPreview") %>
                      </div>
                    </asp:PlaceHolder>
                  </div>
                </div>
              </ItemTemplate>
            </asp:Repeater>
          </div>
        </asp:PlaceHolder>

        <asp:PlaceHolder ID="HelpLogsPlaceholder" runat="server" Visible="false">
          <h3 style="margin-top:20px; font-family:Poppins; font-size:1rem;">
            1:1 help session logs
          </h3>
          <p class="sub" style="margin-bottom:10px;">
            One row per one-on-one help session the helper logged from their 1:1 workspace.
          </p>
          <div class="verification-list">
            <asp:Repeater ID="HelpLogRepeater" runat="server">
              <ItemTemplate>
                <div class="verification-card">
                  <div class="verification-main">
                    <div class="verification-title"><%# Eval("CourseTitle") %></div>
                    <div class="verification-meta">
                      Logged: <%# Eval("WhenLabel") %>
                    </div>
                    <asp:PlaceHolder ID="HelpDetailsPH" runat="server"
                      Visible='<%# !string.IsNullOrWhiteSpace((string)Eval("DetailsPreview")) %>'>
                      <div style="margin-top:4px; font-size:0.8rem; color:var(--muted);">
                        <%# Eval("DetailsPreview") %>
                      </div>
                    </asp:PlaceHolder>
                  </div>
                </div>
              </ItemTemplate>
            </asp:Repeater>
          </div>
        </asp:PlaceHolder>
      </div>

      <!-- Helper-scoped audit log -->
      <div class="card">
        <h2>Helper activity log</h2>
        <p class="sub">
          Filter, browse, or export this helper’s entries from the university audit log.
        </p>

        <div class="log-filters">
          <div class="field">
            <label for="TxtSearch">Search</label>
            <asp:TextBox
              ID="TxtSearch"
              runat="server"
              Placeholder="Search by type or details..." />
          </div>

          <div class="field">
            <label for="DdlTypeFilter">Log type</label>
            <asp:DropDownList ID="DdlTypeFilter" runat="server" />
          </div>

          <div class="field field-buttons">
            <asp:Button
              ID="BtnApplyFilters"
              runat="server"
              Text="Apply filters"
              CssClass="btn primary"
              OnClick="BtnApplyFilters_Click" />
            <asp:Button
              ID="BtnClearFilters"
              runat="server"
              Text="Clear"
              CssClass="btn link"
              OnClick="BtnClearFilters_Click"
              CausesValidation="false" />
            <asp:Button
              ID="BtnExportCsv"
              runat="server"
              Text="Export CSV"
              CssClass="btn link"
              OnClick="BtnExportCsv_Click"
              CausesValidation="false" />
          </div>
        </div>

        <div class="log-filters" style="margin-top:4px;">
          <div class="field">
            <label for="TxtFromTime">From (local time)</label>
            <asp:TextBox
              ID="TxtFromTime"
              runat="server"
              Placeholder="YYYY-MM-DDThh:mm" />
          </div>

          <div class="field">
            <label for="TxtToTime">To (local time)</label>
            <asp:TextBox
              ID="TxtToTime"
              runat="server"
              Placeholder="YYYY-MM-DDThh:mm" />
          </div>
        </div>

        <asp:PlaceHolder ID="NoAuditPlaceholder" runat="server" Visible="false">
          <div class="note" style="margin-top:8px;">
            No audit entries yet for this helper. Once they sign in, complete quizzes,
            log help, or take other actions, entries will appear here.
          </div>
        </asp:PlaceHolder>

        <div class="log-table" aria-label="Helper audit log entries">
          <table>
            <thead>
              <tr>
                <th>Timestamp</th>
                <th>Type</th>
                <th>Details</th>
              </tr>
            </thead>
            <tbody>
              <asp:Repeater ID="AuditRepeater" runat="server">
                <ItemTemplate>
                  <tr>
                    <td>
                      <div><%# Eval("TimestampDate") %></div>
                      <div style="font-size:.8rem; color:var(--muted);">
                        <%# Eval("TimestampTime") %>
                      </div>
                    </td>
                    <td><%# Eval("Type") %></td>
                    <td class="details-cell"><%# Eval("Details") %></td>
                  </tr>
                </ItemTemplate>
              </asp:Repeater>
            </tbody>
          </table>
        </div>

        <div class="pagination">
          <asp:LinkButton
            ID="BtnPrevPage"
            runat="server"
            Text="‹ Previous"
            CssClass="page-link"
            OnClick="BtnPrevPage_Click" />
          <asp:Label
            ID="LblPageInfo"
            runat="server"
            CssClass="page-info" />
          <asp:LinkButton
            ID="BtnNextPage"
            runat="server"
            Text="Next ›"
            CssClass="page-link"
            OnClick="BtnNextPage_Click" />
        </div>
      </div>

    </div>
  </form>
</body>
</html>


