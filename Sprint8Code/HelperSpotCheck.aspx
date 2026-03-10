<%@ Page Language="C#" AutoEventWireup="true"
    CodeBehind="HelperSpotCheck.aspx.cs"
    Inherits="CyberApp_FIA.Account.HelperSpotCheck" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>FIA • Helper Spot-Check</title>
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <link
        href="https://fonts.googleapis.com/css2?family=Lato:wght@300;400&family=Poppins:wght@600&display=swap"
        rel="stylesheet" />
    <style>
        :root {
            --fia-pink: #f06aa9;
            --fia-blue: #2a99db;
            --fia-teal: #45c3b3;
            --ink: #111827;
            --muted: #6b7280;
            --ring: rgba(42,153,219,.25);
            --card: #ffffff;
            --bg: #f9fafb;
            --border: #e5e7eb;
        }

        * {
            box-sizing: border-box;
        }

        html,body{height:100%}
        body {
            margin: 0;
            font-family: 'Lato',sans-serif;
            background: var(--bg);
            color: var(--ink);
            line-height:1.5;
            overflow-x:hidden;
        }

        .wrap {
            max-width: 1120px;
            margin: 0 auto;
            padding: 24px 16px 40px;
        }

        h1 {
            font-family: 'Poppins',sans-serif;
            margin: 0 0 4px;
            font-size: 1.3rem;
        }

        .sub {
            margin: 0 0 16px;
            color: var(--muted);
            font-size: 0.95rem;
            line-height:1.45;
        }

        .card {
            background: var(--card);
            border-radius: 18px;
            padding:18px;
            border: 1px solid var(--border);
            box-shadow: 0 10px 30px rgba(15,23,42,.06);
        }

        .item {
            border-bottom: 1px dashed var(--border);
            padding: 12px 0;
        }

            .item:last-child {
                border-bottom: none;
            }

        .meta {
            font-size: 0.85rem;
            color: var(--muted);
            margin-bottom:6px;
            line-height:1.35;
        }

        textarea {
            width: 100%;
            min-height:70px;
            border-radius:10px;
            border: 1px solid var(--border);
            padding:10px 12px;
            font-size:16px;
        }

            textarea:focus {
                border-color: var(--fia-blue);
                box-shadow: 0 0 0 2px var(--ring);
                outline: none;
            }

        .row {
            display: flex;
            align-items: center;
            gap: 10px;
            margin-top:8px;
            font-size: 0.9rem;
            flex-wrap:wrap;
        }

        .row table{
            width:auto;
        }

        .row td{
            padding-right:14px;
        }

        .btn {
            border: none;
            border-radius: 999px;
            padding: 10px 18px;
            font-size: 0.9rem;
            cursor: pointer;
            min-height:44px;
        }

        .btn-primary {
            background: linear-gradient(135deg,var(--fia-pink),var(--fia-blue));
            color: #fff;
        }

        .action-row{
            margin-top:14px;
            display:flex;
            justify-content:space-between;
            align-items:center;
            gap:10px;
            flex-wrap:wrap;
        }

        .status-text{
            font-size:0.9rem;
            color:var(--muted);
        }

        /* MOBILE OPTIMIZATION */
        @media (max-width:430px){
            .wrap{padding:18px 14px 32px;}
            h1{font-size:1.2rem;}
            .sub{font-size:0.95rem;}
            .card{padding:16px;border-radius:16px;}
            .meta{font-size:0.85rem;}
            textarea{min-height:80px;}
            .btn{width:100%}
            .action-row{flex-direction:column;align-items:stretch;}
        }

    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="wrap">
            <h1>Helper Spot-Check</h1>
            <p class="sub">
                Review a small sample of Helper logs. Mark each as Verified, Questioned, or Skip.
        Each decision updates Helper certification progress and writes to the Super audit log.
     
            </p>

            <div class="card">
                <asp:Repeater ID="SampleRepeater" runat="server">
                    <ItemTemplate>
                        <div class="item">
                            <div class="meta">
                                <%# Eval("TimestampUtc", "{0:yyyy-MM-dd HH:mm} (UTC)") %> •
               
                                <%# Eval("Source") %> •
                Helper: <%# Eval("HelperId") %>
                            </div>
                            <div style="font-size: 0.9rem; margin-bottom: 4px;">
                                Course: <%# Eval("CourseTitle") %>
                            </div>
                            <div style="font-size: 0.85rem; color: var(--muted); margin-bottom: 6px;">
                                <%# Eval("Preview") %>
                            </div>

                            <div class="row">
                                <asp:RadioButtonList ID="DecisionList" runat="server" RepeatDirection="Horizontal">
                                    <asp:ListItem Text="Verified" Value="Verified" />
                                    <asp:ListItem Text="Questioned" Value="Questioned" />
                                    <asp:ListItem Text="Skip" Value="Skip" Selected="True" />
                                </asp:RadioButtonList>
                            </div>

                            <div style="margin-top: 6px;">
                                <asp:Label ID="NoteLabel" runat="server" Text="Short note (optional, no sensitive details):"
                                    AssociatedControlID="AdminNote" Style="font-size: 0.8rem; color: var(--muted);" />
                                <asp:TextBox ID="AdminNote" runat="server" TextMode="MultiLine" />
                            </div>

                            <asp:HiddenField ID="LogKey" runat="server" Value='<%# Eval("Key") %>' />
                            <asp:HiddenField ID="HelperIdHidden" runat="server" Value='<%# Eval("HelperId") %>' />
                            <asp:HiddenField ID="HelperNameHidden" runat="server" Value='<%# Eval("HelperName") %>' />
                            <asp:HiddenField ID="SourceHidden" runat="server" Value='<%# Eval("Source") %>' />
                        </div>
                    </ItemTemplate>
                </asp:Repeater>

                <div style="margin-top: 12px; display: flex; justify-content: space-between; align-items: center;">
                    <asp:Label ID="StatusLabel" runat="server" Style="font-size: 0.85rem; color: var(--muted);" />
                    <asp:Button ID="BtnSaveDecisions" runat="server" Text="Save decisions"
                        CssClass="btn btn-primary" OnClick="BtnSaveDecisions_Click" />
                </div>
            </div>
        </div>
    </form>
</body>
</html>
