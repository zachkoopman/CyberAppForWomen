<%@ Page Language="C#" AutoEventWireup="true"
    CodeBehind="UniversityAdminParticipantDetails.aspx.cs"
    Inherits="CyberApp_FIA.Account.UniversityAdmin.UniversityAdminParticipantDetails" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>FIA • Participant Detail</title>
    <meta name="viewport" content="width=device-width, initial-scale=1" />

    <link href="https://fonts.googleapis.com/css2?family=Lato:wght@300;400;700&family=Poppins:wght@600;700&display=swap" rel="stylesheet" />

    <style>
        :root{
            --fia-pink:#f06aa9;
            --fia-blue:#2a99db;
            --fia-teal:#45c3b3;
            --ink:#1f2937;
            --muted:#6b7280;
            --card:#ffffff;
            --border:#e5e7eb;
            --shadow:0 10px 28px rgba(31,41,55,0.08);
        }

        *{ box-sizing:border-box; }

        body{
            margin:0;
            font-family:'Lato', sans-serif;
            color:var(--ink);
            background:linear-gradient(180deg,#f8fbff 0%, #f3f6fb 100%);
        }

        .page{
            max-width:1200px;
            margin:0 auto;
            padding:30px 18px 50px;
        }

        .top-link{
            margin-bottom:14px;
        }

        .top-link a{
            text-decoration:none;
            color:var(--fia-blue);
            font-weight:700;
        }

        .hero{
            background:linear-gradient(135deg, rgba(240,106,169,0.10), rgba(42,153,219,0.12));
            border:1px solid rgba(42,153,219,0.14);
            border-radius:24px;
            padding:26px 24px;
            box-shadow:var(--shadow);
            margin-bottom:20px;
        }

        .eyebrow{
            display:inline-block;
            padding:6px 12px;
            border-radius:999px;
            background:rgba(240,106,169,0.10);
            border:1px solid rgba(240,106,169,0.18);
            color:var(--fia-pink);
            font-size:13px;
            font-weight:700;
            margin-bottom:12px;
        }

        .hero h1{
            margin:0 0 8px 0;
            font-family:'Poppins', sans-serif;
            font-size:32px;
            line-height:1.15;
        }

        .hero p{
            margin:0;
            color:var(--muted);
            font-size:16px;
        }

        .notice{
            border-radius:16px;
            padding:16px 18px;
            margin-bottom:18px;
            font-size:15px;
        }

        .notice-error{
            background:#fff1f2;
            border:1px solid #fecdd3;
            color:#9f1239;
        }

        .notice-success{
            background:#ecfdf5;
            border:1px solid #a7f3d0;
            color:#065f46;
        }

        .grid{
            display:grid;
            grid-template-columns:repeat(4, minmax(0, 1fr));
            gap:16px;
            margin-bottom:18px;
        }

        .stat-card{
            background:var(--card);
            border:1px solid var(--border);
            border-radius:22px;
            padding:18px;
            box-shadow:var(--shadow);
        }

        .stat-label{
            font-size:14px;
            color:var(--muted);
            margin-bottom:10px;
        }

        .stat-value{
            font-family:'Poppins', sans-serif;
            font-size:28px;
            line-height:1;
            margin-bottom:8px;
        }

        .blue .stat-value{ color:var(--fia-blue); }
        .pink .stat-value{ color:var(--fia-pink); }
        .teal .stat-value{ color:var(--fia-teal); }

        .stat-note{
            font-size:13px;
            color:var(--muted);
        }

        .content-grid{
            display:grid;
            grid-template-columns:1fr 1fr;
            gap:18px;
            margin-bottom:18px;
        }

        .panel{
            background:var(--card);
            border:1px solid var(--border);
            border-radius:24px;
            padding:22px;
            box-shadow:var(--shadow);
        }

        .panel h2{
            margin:0 0 18px 0;
            font-family:'Poppins', sans-serif;
            font-size:22px;
        }

        .detail-list{
            display:grid;
            grid-template-columns:180px 1fr;
            gap:14px 18px;
        }

        .detail-label{
            color:var(--muted);
            font-weight:700;
        }

        .detail-value{
            color:var(--ink);
            word-break:break-word;
        }

        .override-note{
            margin-top:16px;
            padding:12px 14px;
            border-radius:14px;
            background:#fafcff;
            border:1px solid #e8edf5;
            color:var(--muted);
            font-size:14px;
        }

        .logs-panel{
            background:var(--card);
            border:1px solid var(--border);
            border-radius:24px;
            padding:22px;
            box-shadow:var(--shadow);
            margin-bottom:18px;
        }

        .log-list{
            display:grid;
            gap:12px;
        }

        .log-item{
            border:1px solid #e9edf5;
            border-radius:16px;
            padding:14px 15px;
            background:#fafcff;
        }

        .log-top{
            display:flex;
            gap:10px;
            flex-wrap:wrap;
            justify-content:space-between;
            margin-bottom:6px;
        }

        .log-type{
            font-weight:700;
            color:var(--ink);
        }

        .log-date{
            color:var(--muted);
            font-size:13px;
        }

        .log-summary{
            color:var(--ink);
            margin-bottom:6px;
        }

        .log-source{
            color:var(--muted);
            font-size:13px;
        }

        .form-grid{
            display:grid;
            grid-template-columns:repeat(2, minmax(0, 1fr));
            gap:14px;
            margin-bottom:14px;
        }

        .field{
            display:flex;
            flex-direction:column;
            gap:7px;
        }

        .field label{
            font-size:14px;
            font-weight:700;
            color:var(--muted);
        }

        .input{
            border:1px solid var(--border);
            border-radius:12px;
            padding:11px 12px;
            font-size:15px;
            outline:none;
            width:100%;
        }

        .input:focus{
            border-color:var(--fia-blue);
            box-shadow:0 0 0 3px rgba(42,153,219,0.10);
        }

        textarea.input{
            min-height:110px;
            resize:vertical;
        }

        .btn{
            display:inline-block;
            border:none;
            border-radius:12px;
            padding:11px 16px;
            font-weight:700;
            font-size:14px;
            cursor:pointer;
        }

        .btn-blue{
            background:var(--fia-blue);
            color:#fff;
        }

        .help{
            color:var(--muted);
            font-size:13px;
            margin-top:6px;
        }

        .empty{
            color:var(--muted);
            font-size:15px;
        }

        @media (max-width:950px){
            .grid{
                grid-template-columns:repeat(2, minmax(0, 1fr));
            }

            .content-grid{
                grid-template-columns:1fr;
            }
        }

        @media (max-width:700px){
            .page{
                padding:20px 12px 36px;
            }

            .hero h1{
                font-size:26px;
            }

            .grid{
                grid-template-columns:1fr;
            }

            .detail-list,
            .form-grid{
                grid-template-columns:1fr;
            }
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="page">

            <div class="top-link">
                <a href="<%= ResolveUrl("~/Account/UniversityAdmin/UniversityAdminParticipants.aspx") %>">&larr; Back to participant list</a>
            </div>

            <div class="hero">
                <div class="eyebrow">Participant Detail</div>
                <h1><asp:Label ID="lblParticipantNameHeader" runat="server" Text="Participant" /></h1>
                <p>Review participant stats, account information, logs, and controlled manual overrides.</p>
            </div>

            <asp:Panel ID="pnlError" runat="server" CssClass="notice notice-error" Visible="false">
                <asp:Label ID="lblError" runat="server" />
            </asp:Panel>

            <asp:Panel ID="pnlSuccess" runat="server" CssClass="notice notice-success" Visible="false">
                <asp:Label ID="lblSuccess" runat="server" />
            </asp:Panel>

            <asp:Panel ID="pnlContent" runat="server" Visible="true">

                <div class="grid">
                    <div class="stat-card blue">
                        <div class="stat-label">Completed Lessons</div>
                        <div class="stat-value"><asp:Label ID="lblCompletedLessons" runat="server" Text="0" /></div>
                        <div class="stat-note">Effective value shown to admins.</div>
                    </div>

                    <div class="stat-card pink">
                        <div class="stat-label">Enrolled Sessions</div>
                        <div class="stat-value"><asp:Label ID="lblEnrolledSessions" runat="server" Text="0" /></div>
                        <div class="stat-note">Effective value shown to admins.</div>
                    </div>

                    <div class="stat-card teal">
                        <div class="stat-label">Badges Earned</div>
                        <div class="stat-value"><asp:Label ID="lblBadgesEarned" runat="server" Text="0" /></div>
                        <div class="stat-note">Effective value shown to admins.</div>
                    </div>

                    <div class="stat-card">
                        <div class="stat-label">No Shows</div>
                        <div class="stat-value"><asp:Label ID="lblNoShows" runat="server" Text="0" /></div>
                        <div class="stat-note">Effective value shown to admins.</div>
                    </div>
                </div>

                <div class="content-grid">
                    <div class="panel">
                        <h2>Account Details</h2>

                        <div class="detail-list">
                            <div class="detail-label">Full Name</div>
                            <div class="detail-value"><asp:Label ID="lblParticipantName" runat="server" /></div>

                            <div class="detail-label">Username</div>
                            <div class="detail-value"><asp:Label ID="lblParticipantUsername" runat="server" /></div>

                            <div class="detail-label">Email</div>
                            <div class="detail-value"><asp:Label ID="lblParticipantEmail" runat="server" /></div>

                            <div class="detail-label">University</div>
                            <div class="detail-value"><asp:Label ID="lblParticipantUniversity" runat="server" /></div>

                            <div class="detail-label">Joined</div>
                            <div class="detail-value"><asp:Label ID="lblParticipantJoined" runat="server" /></div>
                        </div>

                        <div class="override-note">
                            <asp:Label ID="lblOverrideStatus" runat="server" Text="No manual override is currently applied." />
                        </div>
                    </div>

                    <div class="panel">
                        <h2>Manual Override</h2>

                        <div class="form-grid">
                            <div class="field">
                                <label for="txtCompletedLessonsOverride">Completed Lessons</label>
                                <asp:TextBox ID="txtCompletedLessonsOverride" runat="server" CssClass="input" />
                            </div>

                            <div class="field">
                                <label for="txtEnrolledSessionsOverride">Enrolled Sessions</label>
                                <asp:TextBox ID="txtEnrolledSessionsOverride" runat="server" CssClass="input" />
                            </div>

                            <div class="field">
                                <label for="txtBadgesOverride">Badges Earned</label>
                                <asp:TextBox ID="txtBadgesOverride" runat="server" CssClass="input" />
                            </div>

                            <div class="field">
                                <label for="txtNoShowsOverride">No Shows</label>
                                <asp:TextBox ID="txtNoShowsOverride" runat="server" CssClass="input" />
                            </div>
                        </div>

                        <div class="field" style="margin-bottom:14px;">
                            <label for="txtOverrideReason">Reason for override</label>
                            <asp:TextBox ID="txtOverrideReason" runat="server" TextMode="MultiLine" CssClass="input" />
                        </div>

                        <asp:Button ID="btnSaveOverride" runat="server" Text="Save Override" CssClass="btn btn-blue" OnClick="btnSaveOverride_Click" />

                        <div class="help">
                            This example stores a new override record for audit visibility instead of silently changing original attendance or completion data.
                        </div>
                    </div>
                </div>

                <div class="logs-panel">
                    <h2 style="margin-top:0;">Recent Participant Logs</h2>

                    <asp:Repeater ID="rptLogs" runat="server">
                        <ItemTemplate>
                            <div class="log-item">
                                <div class="log-top">
                                    <div class="log-type"><%#: Eval("Type") %></div>
                                    <div class="log-date"><%#: Eval("DateText") %></div>
                                </div>
                                <div class="log-summary"><%#: Eval("Summary") %></div>
                                <div class="log-source">Source: <%#: Eval("Source") %></div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>

                    <asp:Panel ID="pnlNoLogs" runat="server" CssClass="empty" Visible="false">
                        No logs were found for this participant.
                    </asp:Panel>
                </div>

                <div class="logs-panel">
                    <h2 style="margin-top:0;">Override History</h2>

                    <asp:Repeater ID="rptOverrides" runat="server">
                        <ItemTemplate>
                            <div class="log-item">
                                <div class="log-top">
                                    <div class="log-type">Manual Override</div>
                                    <div class="log-date"><%#: Eval("DateText") %></div>
                                </div>
                                <div class="log-summary"><%#: Eval("Summary") %></div>
                                <div class="log-source">By: <%#: Eval("Source") %></div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>

                    <asp:Panel ID="pnlNoOverrides" runat="server" CssClass="empty" Visible="false">
                        No overrides have been recorded for this participant.
                    </asp:Panel>
                </div>

            </asp:Panel>
        </div>
    </form>
</body>
</html>
