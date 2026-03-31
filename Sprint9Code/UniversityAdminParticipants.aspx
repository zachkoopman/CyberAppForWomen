<%@ Page Language="C#" AutoEventWireup="true"
    CodeBehind="UniversityAdminParticipants.aspx.cs"
    Inherits="CyberApp_FIA.Account.UniversityAdmin.UniversityAdminParticipants" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>FIA • University Participants</title>
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
            color:var(--ink);
            background:linear-gradient(180deg,#f8fbff 0%, #f3f6fb 100%);
        }

        .page{
            max-width:1200px;
            margin:0 auto;
            padding:30px 18px 50px;
        }

        .hero{
            background:linear-gradient(135deg, rgba(42,153,219,0.13), rgba(69,195,179,0.11));
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
            background:rgba(42,153,219,0.10);
            border:1px solid rgba(42,153,219,0.16);
            color:var(--fia-blue);
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

        .toolbar{
            background:var(--card);
            border:1px solid var(--border);
            border-radius:20px;
            padding:16px;
            box-shadow:var(--shadow);
            margin-bottom:18px;
            display:flex;
            gap:12px;
            flex-wrap:wrap;
            align-items:center;
            justify-content:space-between;
        }

        .toolbar-left{
            display:flex;
            align-items:center;
            gap:10px;
            flex-wrap:wrap;
        }

        .toolbar-right{
            display:flex;
            gap:10px;
            flex-wrap:wrap;
            align-items:center;
        }

        .pill{
            display:inline-block;
            padding:6px 12px;
            border-radius:999px;
            font-size:13px;
            font-weight:700;
            background:rgba(69,195,179,0.12);
            color:#0f766e;
            border:1px solid rgba(69,195,179,0.20);
        }

        .count-pill{
            display:inline-block;
            padding:6px 12px;
            border-radius:999px;
            font-size:13px;
            font-weight:700;
            background:rgba(240,106,169,0.10);
            color:#a21caf;
            border:1px solid rgba(240,106,169,0.18);
        }

        .search-box{
            min-width:260px;
            width:340px;
            max-width:100%;
            border:1px solid var(--border);
            border-radius:12px;
            padding:11px 12px;
            font-size:15px;
            outline:none;
        }

        .search-box:focus{
            border-color:var(--fia-blue);
            box-shadow:0 0 0 3px rgba(42,153,219,0.10);
        }

        .btn{
            display:inline-block;
            border:none;
            border-radius:12px;
            padding:11px 16px;
            font-weight:700;
            font-size:14px;
            cursor:pointer;
            text-decoration:none;
        }

        .btn-blue{
            background:var(--fia-blue);
            color:#fff;
        }

        .btn-blue:hover{
            opacity:.95;
        }

        .table-wrap{
            background:var(--card);
            border:1px solid var(--border);
            border-radius:24px;
            box-shadow:var(--shadow);
            overflow:hidden;
        }

        .table-scroll{
            overflow-x:auto;
        }

        table{
            width:100%;
            border-collapse:collapse;
            min-width:980px;
        }

        th, td{
            padding:16px 14px;
            border-bottom:1px solid #eef2f7;
            text-align:left;
            vertical-align:top;
        }

        th{
            background:#fafcff;
            color:var(--muted);
            font-size:13px;
            letter-spacing:.2px;
            text-transform:uppercase;
        }

        td{
            font-size:15px;
        }

        tr:last-child td{
            border-bottom:none;
        }

        .name{
            font-weight:700;
            color:var(--ink);
            margin-bottom:4px;
        }

        .sub{
            color:var(--muted);
            font-size:13px;
        }

        .stat-chip{
            display:inline-block;
            min-width:38px;
            text-align:center;
            padding:6px 10px;
            border-radius:999px;
            font-weight:700;
            font-size:13px;
            background:#f3f6fb;
            border:1px solid #e5e7eb;
            color:var(--ink);
        }

        .view-link{
            display:inline-block;
            background:rgba(42,153,219,0.10);
            color:var(--fia-blue);
            border:1px solid rgba(42,153,219,0.18);
            border-radius:12px;
            padding:9px 12px;
            font-weight:700;
            text-decoration:none;
        }

        .view-link:hover{
            background:rgba(42,153,219,0.14);
        }

        .empty{
            padding:28px 20px;
            color:var(--muted);
            font-size:15px;
        }

        @media (max-width:700px){
            .page{
                padding:20px 12px 36px;
            }

            .hero h1{
                font-size:26px;
            }

            .toolbar{
                padding:14px;
            }
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="page">

            <div class="hero">
                <div class="eyebrow">University Admin</div>
                <h1>Participants for <asp:Label ID="lblUniversityNameHeader" runat="server" Text="Your University" /></h1>
                <p>View participant accounts, progress stats, and open a detailed participant record for review.</p>
            </div>

            <asp:Panel ID="pnlError" runat="server" CssClass="notice notice-error" Visible="false">
                <asp:Label ID="lblError" runat="server" />
            </asp:Panel>

            <asp:Panel ID="pnlContent" runat="server" Visible="true">

                <div class="toolbar">
                    <div class="toolbar-left">
                        <span class="pill">
                            University: <asp:Label ID="lblUniversityName" runat="server" Text="Not loaded" />
                        </span>
                        <span class="count-pill">
                            Participants: <asp:Label ID="lblParticipantCount" runat="server" Text="0" />
                        </span>
                    </div>

                    <div class="toolbar-right">
                        <asp:TextBox ID="txtSearch" runat="server" CssClass="search-box" placeholder="Search by name, username, or email" />
                        <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="btn btn-blue" OnClick="btnSearch_Click" />
                    </div>
                </div>

                <div class="table-wrap">
                    <div class="table-scroll">
                        <table>
                            <thead>
                                <tr>
                                    <th>Participant</th>
                                    <th>Completed Lessons</th>
                                    <th>Enrolled Sessions</th>
                                    <th>Badges</th>
                                    <th>No Shows</th>
                                    <th>Joined</th>
                                    <th>Open</th>
                                </tr>
                            </thead>
                            <tbody>
                                <asp:Repeater ID="rptParticipants" runat="server">
                                    <ItemTemplate>
                                        <tr>
                                            <td>
                                                <div class="name"><%#: Eval("FullName") %></div>
                                                <div class="sub">Username: <%#: Eval("Username") %></div>
                                                <div class="sub">Email: <%#: Eval("Email") %></div>
                                            </td>
                                            <td><span class="stat-chip"><%#: Eval("CompletedLessons") %></span></td>
                                            <td><span class="stat-chip"><%#: Eval("EnrolledSessions") %></span></td>
                                            <td><span class="stat-chip"><%#: Eval("BadgesEarned") %></span></td>
                                            <td><span class="stat-chip"><%#: Eval("NoShows") %></span></td>
                                            <td><%#: Eval("JoinedText") %></td>
                                            <td>
                                                <a class="view-link" href="<%#: Eval("DetailUrl") %>">View Details</a>
                                            </td>
                                        </tr>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </tbody>
                        </table>
                    </div>

                    <asp:Panel ID="pnlEmpty" runat="server" CssClass="empty" Visible="false">
                        No participants matched this university or search filter.
                    </asp:Panel>
                </div>

            </asp:Panel>
        </div>
    </form>
</body>
</html>
