<%@ Page Title="" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeBehind="PaymentManagement.aspx.cs" Inherits="PortalApp.config.PaymentManagement" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <span style="color:Black;"><b> PAYMENT MANAGEMENT: </b>
    
    </span>
    <asp:ToolkitScriptManager ID="ScriptManager1" runat="server">
</asp:ToolkitScriptManager>
     <asp:GridView ID="GridView" OnRowDataBound="GridView_RowDataBound" runat="server"
                Font-Names="Arial"  DataKeyNames="id" Font-Size="9pt" CellPadding="4" ForeColor="#333333" AutoGenerateColumns="False">                
                <EditRowStyle BackColor="#999999" />
                <FooterStyle BackColor="#08605c" Font-Bold="True" ForeColor="White" />
                
                <HeaderStyle BackColor="#08605c" ForeColor="white" Font-Bold="True" />
                <AlternatingRowStyle BackColor="White" ForeColor="#284775" />
    <Columns>
        <asp:BoundField DataField="idPartner" HeaderText="Partner Id"/>
        <asp:TemplateField HeaderText="Partner Name">
            <ItemTemplate>
                <asp:DropDownList runat="server" ID="ddlPartner" DataValueField="idPartner" DataTextField="PartnerName" Enabled="False"/>
            </ItemTemplate>
        </asp:TemplateField>
        <asp:BoundField DataField="accountName" HeaderText="Account Name"/>
            <asp:BoundField DataField="serviceGroup" HeaderText="Service Group"/>
            <asp:BoundField DataField="uom" HeaderText="Currency"/>
            <asp:BoundField DataField="balanceAfter" HeaderText="Current Balance"/>
<%--         <asp:BoundField DataField="PaymentMode" HeaderText="PaymentMode"/>
         <asp:BoundField DataField="MaxCreditLimit" HeaderText="MaxCreditLimit"/>
         <asp:BoundField DataField="LastCreditedAmount" HeaderText="LastCreditedAmount"/>
        <asp:BoundField DataField="Date" HeaderText="PaymentDate" />
         <asp:BoundField DataField="LastAmountType" HeaderText="LastAmountType"/>
       <asp:BoundField DataField="" />--%>
       <asp:BoundField DataField="" />
       
    </Columns>
                
                <PagerStyle BackColor="#284775" ForeColor="White" HorizontalAlign="Center" />
                <RowStyle BackColor="#F7F6F3" ForeColor="#333333" />
                <SelectedRowStyle BackColor="#E2DED6" Font-Bold="True" ForeColor="#333333" />
                <SortedAscendingCellStyle BackColor="#E9E7E2" />
                <SortedAscendingHeaderStyle BackColor="#506C8C" />
                <SortedDescendingCellStyle BackColor="#FFFDF8" />
                <SortedDescendingHeaderStyle BackColor="#6F8DAE" />
                
            </asp:GridView>
</asp:Content>
