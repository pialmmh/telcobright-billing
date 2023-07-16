<%@ Page Title="" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeBehind="PaymentHistory.aspx.cs" Inherits="PortalApp.config.PaymentHistory" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

   
        <asp:ToolkitScriptManager ID="toolScriptManageer1" runat="server"></asp:ToolkitScriptManager> 
       <div>
     <span style="font-weight:bold;"> Select patner:</span>    
     <asp:DropDownList ID="ddlPartner" runat="server"></asp:DropDownList>
     <span style="font-weight:bold;"> Start Date:</span>  
            
             <asp:TextBox ID="starttxtDate" runat="server" />
            <asp:CalendarExtender ID="CalendarStartDate" runat="server"
                TargetControlID="starttxtDate" PopupButtonID="starttxtDate" Format="yyyy-MM-dd">
            </asp:CalendarExtender>
    <span style="font-weight:bold;"> End Date:</span>  
           
            <asp:TextBox ID="endtxtDate" runat="server" />
            <asp:CalendarExtender ID="CalendarEndDate" runat="server"
                TargetControlID="endtxtDate" PopupButtonID="endtxtDate" Format="yyyy-MM-dd">
                 </asp:CalendarExtender> 
           <asp:Button ID="showBtn" runat="server" Text="Show" OnClick="showBtn_Click" />
           <br /><br />
       </div>
          
   
  
        
          
   
    <asp:GridView ID="GridView1"
        Font-Names="Arial" DataKeyNames="PartnerID"
        Font-Size="0.75em"
        CellPadding="5" CellSpacing="0"
        ForeColor="#333"
        AutoGenerateColumns="false"
        runat="server" Width="896px">

        <HeaderStyle BackColor="#989898" ForeColor="white" />
        <Columns>
            <asp:BoundField DataField="PartnerID" HeaderText="PartnerID" />
            <asp:BoundField DataField="PartnerName" HeaderText="PartnerName" />
            <asp:BoundField DataField="Service" HeaderText="Service" />
            <asp:BoundField DataField="Date" HeaderText="Date" />
            <asp:BoundField DataField="PaymentAmount" HeaderText="Payment Amount" />
            <asp:BoundField DataField="Type" HeaderText="Type" />
            <asp:BoundField DataField="BalanceBefore" HeaderText="Balance Before" />
            <asp:BoundField DataField="BalanceAfter" HeaderText="Balance After" />


        </Columns>

    </asp:GridView>



    

</asp:Content>
