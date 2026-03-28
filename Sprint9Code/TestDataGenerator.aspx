<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="TestDataGenerator.aspx.cs" Inherits="Account.Participant.TestDataGenerator" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Test Data Generator</title>
</head>
<body>
    <form id="form1" runat="server">
        <h2>Generate Test Users</h2>
        <asp:Label ID="lblInfo" runat="server" Text="Number of users to generate:" />
        <br />
        <asp:TextBox ID="txtUserCount" runat="server" />
        <br /><br />
        <asp:Button ID="btnGenerate" runat="server" Text="Generate" OnClick="btnGenerate_Click" />
        <br /><br />
        <asp:Literal ID="ltOutput" runat="server" />
    </form>
</body>
</html>