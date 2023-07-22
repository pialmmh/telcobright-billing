<%@ Page Title="" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeBehind="OperatorSettings.aspx.cs" Inherits="SettingsGlobalsettings" %>

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
        DefaultContainerName="PartnerEntities" 
        EnableFlattening="False" EntitySetName="telcobrightpartners" EnableUpdate="True" 
      >
    </asp:EntityDataSource>
    
    <div style="padding-left:240px;">

        <asp:DetailsView ID="DetailsView1" runat="server" Height="50px" Width="125px" 
        AllowPaging="True" DataSourceID="EntityDataSourceTBCustomer" 
        CellPadding="4" ForeColor="#333333" GridLines="None" 
        AutoGenerateRows="False" DataKeyNames="idCustomer">
        <AlternatingRowStyle BackColor="White" />
        <CommandRowStyle BackColor="#C5BBAF" Font-Bold="True" />
        <EditRowStyle BackColor="#7C6F57" />
        <FieldHeaderStyle BackColor="#D0D0D0" Font-Bold="True" />
        <Fields>
            <asp:BoundField DataField="idCustomer" HeaderText="idCustomer" ReadOnly="True" 
                SortExpression="idCustomer" />
            <asp:BoundField DataField="CustomerName" HeaderText="CustomerName" 
                SortExpression="CustomerName" />
            <asp:BoundField DataField="idOperatorType" HeaderText="idOperatorType" 
                SortExpression="idOperatorType" />
            <asp:BoundField DataField="databasename" HeaderText="databasename" 
                SortExpression="databasename" />
            <asp:BoundField DataField="databasetype" HeaderText="databasetype" 
                SortExpression="databasetype" />
            <asp:BoundField DataField="user" HeaderText="user" SortExpression="user" />
            <asp:BoundField DataField="pass" HeaderText="pass" SortExpression="pass" />
            <asp:BoundField DataField="ServerNameOrIP" HeaderText="ServerNameOrIP" 
                SortExpression="ServerNameOrIP" />
            <asp:BoundField DataField="IBServerNameOrIP" HeaderText="IBServerNameOrIP" 
                SortExpression="IBServerNameOrIP" />
            <asp:BoundField DataField="IBdatabasename" HeaderText="IBdatabasename" 
                SortExpression="IBdatabasename" />
            <asp:BoundField DataField="IBdatabasetype" HeaderText="IBdatabasetype" 
                SortExpression="IBdatabasetype" />
            <asp:BoundField DataField="IBuser" HeaderText="IBuser" 
                SortExpression="IBuser" />
            <asp:BoundField DataField="IBpass" HeaderText="IBpass" 
                SortExpression="IBpass" />
            <asp:BoundField DataField="TransactionSizeForCDRLoading" 
                HeaderText="TransactionSizeForCDRLoading" 
                SortExpression="TransactionSizeForCDRLoading" />
            <asp:BoundField DataField="NativeTimeZone" HeaderText="NativeTimeZone" 
                SortExpression="NativeTimeZone" />
            <asp:BoundField DataField="IgwPrefix" HeaderText="IgwPrefix" 
                SortExpression="IgwPrefix" />
            <asp:BoundField DataField="RateDictionaryMaxRecords" 
                HeaderText="RateDictionaryMaxRecords" 
                SortExpression="RateDictionaryMaxRecords" />
            <asp:BoundField DataField="MinMSForIntlOut" HeaderText="MinMSForIntlOut" 
                SortExpression="MinMSForIntlOut" />
            <asp:BoundField DataField="RawCdrKeepDurationDays" 
                HeaderText="RawCdrKeepDurationDays" SortExpression="RawCdrKeepDurationDays" />
            <asp:BoundField DataField="SummaryKeepDurationDays" 
                HeaderText="SummaryKeepDurationDays" SortExpression="SummaryKeepDurationDays" />
            <asp:BoundField DataField="AutoDeleteOldData" HeaderText="AutoDeleteOldData" 
                SortExpression="AutoDeleteOldData" />
            <asp:BoundField DataField="AutoDeleteStartHour" 
                HeaderText="AutoDeleteStartHour" SortExpression="AutoDeleteStartHour" Visible="false" />
            <asp:BoundField DataField="AutoDeleteEndHour" HeaderText="AutoDeleteEndHour" Visible="false"
                SortExpression="AutoDeleteEndHour" />
            <asp:CommandField ShowEditButton="True" />
        </Fields>
        <FooterStyle BackColor="#1C5E55" Font-Bold="True" ForeColor="White" />
        <HeaderStyle BackColor="#1C5E55" Font-Bold="True" ForeColor="White" />
        <PagerStyle BackColor="#666666" ForeColor="White" HorizontalAlign="Center" />
        <RowStyle BackColor="white" />
    </asp:DetailsView>

    </div>

</asp:Content>

