<%@ Page Language="C#" AutoEventWireup="true"
    CodeBehind="ParticipantAccount.aspx.cs"
    Inherits="CyberApp_FIA.Account.Participant.ParticipantAccount" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>FIA • Participant Account</title>
    <meta name="viewport" content="width=device-width, initial-scale=1" />

    <link href="https://fonts.googleapis.com/css2?family=Lato:wght@300;400;700&family=Poppins:wght@600;700&display=swap" rel="stylesheet" />

    <style>
        :root{
            --fia-pink:#f06aa9;
            --fia-blue:#2a99db;
            --fia-teal:#45c3b3;
            --ink:#1f2937;
            --muted:#6b7280;
            --bg:#f4f7fb;
            --card:#ffffff;
            --border:#e5e7eb;
            --shadow:0 10px 28px rgba(31,41,55,0.08);
        }

        *{ box-sizing:border-box; }

        body{
            margin:0;
            font-family:'Lato', sans-serif;
            background:linear-gradient(180deg,#f8fbff 0%, #f3f6fb 100%);
            color:var(--ink);
        }

        .page{
            max-width:1100px;
            margin:0 auto;
            padding:32px 18px 50px;
        }

        .hero{
            background:linear-gradient(135deg, rgba(42,153,219,0.14), rgba(240,106,169,0.12));
            border:1px solid rgba(42,153,219,0.14);
            border-radius:24px;
            padding:26px 24px;
            box-shadow:var(--shadow);
            margin-bottom:22px;
        }

        .eyebrow{
            display:inline-block;
            font-size:13px;
            color:var(--fia-blue);
            background:rgba(42,153,219,0.10);
            border:1px solid rgba(42,153,219,0.16);
            border-radius:999px;
            padding:6px 12px;
            margin-bottom:12px;
            font-weight:700;
            letter-spacing:.2px;
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

        .grid{
            display:grid;
            grid-template-columns:repeat(4, minmax(0, 1fr));
            gap:16px;
            margin-bottom:20px;
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
            color:var(--ink);
        }

        .stat-accent-blue .stat-value{ color:var(--fia-blue); }
        .stat-accent-pink .stat-value{ color:var(--fia-pink); }
        .stat-accent-teal .stat-value{ color:var(--fia-teal); }

        .stat-note{
            font-size:13px;
            color:var(--muted);
        }

        .content-grid{
            display:grid;
            grid-template-columns:1.1fr .9fr;
            gap:18px;
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

        .summary-list{
            display:grid;
            gap:12px;
        }

        .summary-item{
            border:1px solid var(--border);
            border-radius:16px;
            padding:14px 15px;
            background:#fafcff;
        }

        .summary-item strong{
            display:block;
            margin-bottom:4px;
            color:var(--ink);
        }

        .summary-item span{
            color:var(--muted);
            font-size:14px;
        }

        .role-pill{
            display:inline-block;
            background:rgba(69,195,179,0.12);
            color:#0f766e;
            border:1px solid rgba(69,195,179,0.20);
            padding:6px 12px;
            border-radius:999px;
            font-weight:700;
            font-size:13px;
        }

        @media (max-width: 900px){
            .grid{
                grid-template-columns:repeat(2, minmax(0, 1fr));
            }

            .content-grid{
                grid-template-columns:1fr;
            }
        }

        @media (max-width: 600px){
            .page{
                padding:20px 12px 36px;
            }

            .hero h1{
                font-size:26px;
            }

            .grid{
                grid-template-columns:1fr;
            }

            .detail-list{
                grid-template-columns:1fr;
                gap:6px 0;
            }
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="page">

            <div class="hero">
                <div class="eyebrow">Participant Account</div>
                <h1>Welcome, <asp:Label ID="lblParticipantNameHeader" runat="server" Text="Participant" /></h1>
                <p>View your account details, progress summary, and FIA activity in one place.</p>
            </div>

            <asp:Panel ID="pnlError" runat="server" CssClass="notice notice-error" Visible="false">
                <asp:Label ID="lblError" runat="server" />
            </asp:Panel>

            <asp:Panel ID="pnlContent" runat="server" Visible="true">

                <div class="grid">
                    <div class="stat-card stat-accent-blue">
                        <div class="stat-label">Completed Lessons</div>
                        <div class="stat-value">
                            <asp:Label ID="lblCompletedLessons" runat="server" Text="0" />
                        </div>
                        <div class="stat-note">Lessons or microcourses you have completed.</div>
                    </div>

                    <div class="stat-card stat-accent-pink">
                        <div class="stat-label">Enrolled Sessions</div>
                        <div class="stat-value">
                            <asp:Label ID="lblEnrolledSessions" runat="server" Text="0" />
                        </div>
                        <div class="stat-note">Sessions currently linked to your attendance records.</div>
                    </div>

                    <div class="stat-card stat-accent-teal">
                        <div class="stat-label">Badges Earned</div>
                        <div class="stat-value">
                            <asp:Label ID="lblBadgesEarned" runat="server" Text="0" />
                        </div>
                        <div class="stat-note">Badges awarded to your participant account.</div>
                    </div>

                    <div class="stat-card">
                        <div class="stat-label">No Shows</div>
                        <div class="stat-value">
                            <asp:Label ID="lblNoShows" runat="server" Text="0" />
                        </div>
                        <div class="stat-note">Attendance records marked missing or no-show.</div>
                    </div>
                </div>

                <div class="content-grid">
                    <div class="panel">
                        <h2>Account Details</h2>

                        <div style="margin-bottom:18px;">
                            <span class="role-pill">
                                <asp:Label ID="lblParticipantRole" runat="server" Text="Participant" />
                            </span>
                        </div>

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
                    </div>

                    <div class="panel">
                        <h2>Progress Summary</h2>

                        <div class="summary-list">
                            <div class="summary-item">
                                <strong>Learning progress</strong>
                                <span>Your completed lesson count is pulled from stored completion records.</span>
                            </div>

                            <div class="summary-item">
                                <strong>Session participation</strong>
                                <span>Your enrolled and attendance-related session records are summarized here for quick review.</span>
                            </div>

                            <div class="summary-item">
                                <strong>Badge visibility</strong>
                                <span>Your awarded badges can be counted and shown from the badge data source tied to your account.</span>
                            </div>

                            <div class="summary-item">
                                <strong>Attendance history</strong>
                                <span>No-show totals can help University Admins and Helpers review participation trends when needed.</span>
                            </div>
                        </div>
                    </div>
                </div>

            </asp:Panel>

        </div>
    </form>
</body>
</html>
