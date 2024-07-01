<%@ Page Title="" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeBehind="switchsettings.aspx.cs" Inherits="SettingsGlobalsettingsOpSetting" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" Runat="Server">
    <asp:EntityDataSource ID="EntityDataSourceSwitchInfo" runat="server" 
        ConnectionString="name=PartnerEntities" 
        DefaultContainerName="PartnerEntities" EnableDelete="True" 
        EnableFlattening="False" EnableInsert="True" EnableUpdate="True" 
        EntitySetName="nes">
    </asp:EntityDataSource>

     <asp:EntityDataSource ID="EntityDataSourceTBCustomer" runat="server" 
        ConnectionString="name=PartnerEntities" 
        DefaultContainerName="PartnerEntities" EnableFlattening="False" EntitySetName="telcobrightpartners" 
      >
    </asp:EntityDataSource>

    <div style="padding-left:120px;">

    <asp:DetailsView ID="DetailsView1" runat="server" Height="50px" Width="125px" 
        AllowPaging="True" DataSourceID="EntityDataSourceSwitchInfo" 
        CellPadding="4" ForeColor="#333333" GridLines="None" 
        AutoGenerateRows="False" DataKeyNames="idSwitch">
        <AlternatingRowStyle BackColor="White" />
        <CommandRowStyle BackColor="#C5BBAF" Font-Bold="True" />
        <EditRowStyle BackColor="#7C6F57" />
        <FieldHeaderStyle BackColor="#D0D0D0" Font-Bold="True" />
        <Fields>
            <asp:BoundField DataField="idSwitch" HeaderText="idSwitch" ReadOnly="True" 
                SortExpression="idSwitch" />
            <asp:BoundField DataField="idCustomer" HeaderText="idCustomer" 
                SortExpression="idCustomer" />
            <asp:BoundField DataField="idSwitchType" HeaderText="idSwitchType" 
                SortExpression="idSwitchType" />
            <asp:BoundField DataField="SwitchName" HeaderText="SwitchName" 
                SortExpression="SwitchName" />
            <asp:BoundField DataField="CDRPrefix" HeaderText="CDRPrefix" 
                SortExpression="CDRPrefix" />
            <asp:BoundField DataField="FileExtension" HeaderText="FileExtension" 
                SortExpression="FileExtension" />
            <asp:BoundField DataField="Description" HeaderText="Description" 
                SortExpression="Description" />
            <asp:BoundField DataField="FtpUrl1" HeaderText="FtpUrl1" 
                SortExpression="FtpUrl1" />
            <asp:BoundField DataField="FtpUser1" HeaderText="FtpUser1" 
                SortExpression="FtpUser1" />
            <asp:BoundField DataField="FtpPass1" HeaderText="FtpPass1" 
                SortExpression="FtpPass1" />
            <asp:BoundField DataField="FtpUrl2" HeaderText="FtpUrl2" 
                SortExpression="FtpUrl2" />
            <asp:BoundField DataField="FtpUser2" HeaderText="FtpUser2" 
                SortExpression="FtpUser2" />
            <asp:BoundField DataField="FtpPass2" HeaderText="FtpPass2" 
                SortExpression="FtpPass2" />
            <asp:BoundField DataField="FtpUrl3" HeaderText="FtpUrl3" 
                SortExpression="FtpUrl3" />
            <asp:BoundField DataField="FtpUser3" HeaderText="FtpUser3" 
                SortExpression="FtpUser3" />
            <asp:BoundField DataField="FtpPass3" HeaderText="FtpPass3" 
                SortExpression="FtpPass3" />
            <asp:BoundField DataField="FtpUrl4" HeaderText="FtpUrl4" 
                SortExpression="FtpUrl4" />
            <asp:BoundField DataField="FtpUser4" HeaderText="FtpUser4" 
                SortExpression="FtpUser4" />
            <asp:BoundField DataField="FtpPass4" HeaderText="FtpPass4" 
                SortExpression="FtpPass4" />
            <asp:BoundField DataField="FtpUrl5" HeaderText="FtpUrl5" 
                SortExpression="FtpUrl5" />
            <asp:BoundField DataField="FtpUser5" HeaderText="FtpUser5" 
                SortExpression="FtpUser5" />
            <asp:BoundField DataField="FtpPass5" HeaderText="FtpPass5" 
                SortExpression="FtpPass5" />
            <asp:BoundField DataField="LoadingStopFlag" HeaderText="LoadingStopFlag" 
                SortExpression="LoadingStopFlag" />
            <asp:BoundField DataField="LoadingSpanCount" HeaderText="LoadingSpanCount" 
                SortExpression="LoadingSpanCount" />
            <asp:BoundField DataField="TransactionSizeForCDRLoading" 
                HeaderText="TransactionSizeForCDRLoading" 
                SortExpression="TransactionSizeForCDRLoading" />
            <asp:BoundField DataField="DecodingSpanCount" HeaderText="DecodingSpanCount" 
                SortExpression="DecodingSpanCount" />
            <asp:BoundField DataField="ProcessBatchFirst" HeaderText="ProcessBatchFirst" 
                SortExpression="ProcessBatchFirst" />
            <asp:BoundField DataField="SkipCdrListed" HeaderText="SkipCdrListed" 
                SortExpression="SkipCdrListed" />
            <asp:BoundField DataField="SkipCdrReceived" HeaderText="SkipCdrReceived" 
                SortExpression="SkipCdrReceived" />
            <asp:BoundField DataField="SkipCdrDecoded" HeaderText="SkipCdrDecoded" 
                SortExpression="SkipCdrDecoded" />
            <asp:BoundField DataField="SkipCdrBackedup" HeaderText="SkipCdrBackedup" 
                SortExpression="SkipCdrBackedup" />
            <asp:BoundField DataField="KeepDecodedCDR" HeaderText="KeepDecodedCDR" 
                SortExpression="KeepDecodedCDR" />
            <asp:BoundField DataField="KeepReceivedCdr" HeaderText="KeepReceivedCdr" 
                SortExpression="KeepReceivedCdr" />
            <asp:BoundField DataField="DeleteReceivedCdrClient" 
                HeaderText="DeleteReceivedCdrClient" SortExpression="DeleteReceivedCdrClient" />
            <asp:BoundField DataField="CcrCauseCodeField" HeaderText="CcrCauseCodeField" 
                SortExpression="CcrCauseCodeField" />
            <asp:BoundField DataField="SwitchTimeZoneId" HeaderText="SwitchTimeZoneId" 
                SortExpression="SwitchTimeZoneId" />
            <asp:BoundField DataField="CallConnectIndicator" 
                HeaderText="CallConnectIndicator" SortExpression="CallConnectIndicator" />
            <asp:BoundField DataField="FieldNoForTimeSummary" 
                HeaderText="FieldNoForTimeSummary" SortExpression="FieldNoForTimeSummary" />
            <asp:BoundField DataField="EnableSummaryGeneration" 
                HeaderText="EnableSummaryGeneration" SortExpression="EnableSummaryGeneration" />
            <asp:BoundField DataField="ExistingSummaryCacheSpanHr" 
                HeaderText="ExistingSummaryCacheSpanHr" 
                SortExpression="ExistingSummaryCacheSpanHr" />
            <asp:BoundField DataField="BatchToDecodeRatio" HeaderText="BatchToDecodeRatio" 
                SortExpression="BatchToDecodeRatio" />
            <asp:BoundField DataField="FieldSeperatorTxt" HeaderText="FieldSeperatorTxt" 
                SortExpression="FieldSeperatorTxt" />
        </Fields>
        <FooterStyle BackColor="#1C5E55" Font-Bold="True" ForeColor="White" />
        <HeaderStyle BackColor="#1C5E55" Font-Bold="True" ForeColor="White" />
        <PagerStyle BackColor="#666666" ForeColor="White" HorizontalAlign="Center" />
        <RowStyle BackColor="white" />
    </asp:DetailsView>
    </div>
</asp:Content>

