<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ParticipantScoreWidget.ascx.cs" Inherits="CyberApp_FIA.Account.Participant.ParticipantScoreWidget" %>
<style>
  .scoreCard{background:#fff;border-radius:16px;box-shadow:0 8px 24px rgba(0,0,0,.08);padding:16px;margin:12px 0}
  .row{display:flex;align-items:center;gap:12px}
  .pill{margin-left:auto;background:#45c3b3;color:#fff;border-radius:999px;padding:4px 10px;font-size:.85rem}
  .mini{color:#6b7280;font-size:.9rem;margin-top:6px}
  .grid{display:grid;grid-template-columns:repeat(auto-fit,minmax(160px,1fr));gap:8px;margin-top:8px}
  .cell{background:#f7fbff;border:1px solid #e3f0fb;border-radius:12px;padding:8px}
  .actions{margin-top:8px}
  .btnTiny{padding:6px 10px;border:0;border-radius:10px;background:#2a99db;color:#fff;cursor:pointer}
</style>
<div class="scoreCard">
  <div class="row">
    <strong>Cybersecurity Score</strong>
    <span id="PillScore" runat="server" class="pill">–</span>
  </div>
  <div class="mini">Private to you by default. Share with your Helper only if you want targeted help.</div>
  <div class="grid">
    <asp:Repeater ID="RptDomainMini" runat="server">
      <ItemTemplate>
        <div class="cell"><%# Eval("Key") %>: <strong><%# Eval("Value") %></strong></div>
      </ItemTemplate>
    </asp:Repeater>
  </div>
  <div class="actions">
    <asp:CheckBox ID="ChkShare" runat="server" Text="Share with my Helper" />
    <asp:Button ID="BtnShare" runat="server" Text="Update" CssClass="btnTiny" OnClick="BtnShare_Click"/>
  </div>
</div>