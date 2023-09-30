<%@ Page Language="C#" AutoEventWireup="true"  MasterPageFile="~/Site.master" EnableEventValidation="false" CodeBehind="OutgoingCallAnalysis.aspx.cs" Inherits="PortalApp.reports.Accounts.CountryWiseAnsInvoice" %>


<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" href="http://code.jquery.com/ui/1.10.3/themes/smoothness/jquery-ui.css">
    <script src="http://code.jquery.com/jquery-1.9.1.js"></script>
    <script src="http://code.jquery.com/ui/1.10.3/jquery-ui.js"></script>

    <script>
        $(function () {
            $("#txtStartDate").datepicker({ dateFormat: 'yy-mm-dd' });
            $("#txtEndDate").datepicker({ dateFormat: 'yy-mm-dd' });
        });
    </script>

     <style type="text/css">  
    /* CSS to change the GridLines color */
    .Grid, .Grid th, .Grid td
    {
        border:1px solid lightgray;
        
    }
    </style>

</asp:Content>


<%--Main body portion--%>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    
   

    <div id="errormsg" style="clear:both; margin-left:10px; height:15px;background-color:#EBF0FF;padding-left:5px;width:auto;margin-bottom:2px;">
        <div style='text-align:center'>
            <asp:Label ID="lblErrMsg" runat="server" ForeColor="Red"></asp:Label>
        </div>
    </div>

    <div id="report" style="clear:both; margin-left:10px; border:solid 1px; font-family:Arial-Narrow; height:35px;background-color:#FFFAF0;padding-left:5px;padding-top:10px;width:auto;margin-bottom:2px;">
        <asp:Label ID="lblStartDate" runat="server" Text="Start Date:" ></asp:Label>
        <asp:TextBox ID="txtStartDate" runat="server" ClientIDMode="Static" Font-Names="Arial-Narrow"></asp:TextBox>


        <asp:Label ID="lblEndDate" runat="server" Text="End Date:"></asp:Label>
        <asp:TextBox ID="txtEndDate" runat="server" ClientIDMode="Static" Font-Names="Arial-Narrow"></asp:TextBox>


        <asp:Label ID="lblISOName" runat="server" Text="Select IOS Name:"></asp:Label>
        <asp:DropDownList ID="dpdIsoName" runat="server" Width="130px" Font-Names="Arial-Narrow">
            <asp:ListItem>Select IOS Name</asp:ListItem>
            <asp:ListItem>All IOS</asp:ListItem>
            <asp:ListItem>Novotel Limited</asp:ListItem>
            <asp:ListItem>Mir Telecom Ltd</asp:ListItem>
            <asp:ListItem>Bangla Trac Communications Limited</asp:ListItem>
            <asp:ListItem>Global Voice Telecom Limited</asp:ListItem>
            <asp:ListItem>Digicon Telecommunication Ltd</asp:ListItem>
            <asp:ListItem>Roots Communication Ltd</asp:ListItem>
            <asp:ListItem>Unique Infoway Limited</asp:ListItem>
        </asp:DropDownList>

        <asp:Label ID="lblRate" runat="server" Text="Ex. Rate (BDT):"></asp:Label>
        <asp:TextBox ID="txtRate" runat="server" Width="70px"></asp:TextBox>        


        <asp:Button ID="btnShowReport" runat="server" Font-Names="Arial-Narrow" Text="Show Report" OnClick="btnShowReport_Click" />

    </div>

    <div id="gridview" style="clear:both; margin-left:10px; margin-right:2px; height:auto;padding-top:12px;width:1050px;margin-bottom:2px;">
       <asp:Panel ID="panel" runat="server" ScrollBars="Auto" Width="100%" 
            Height="490px" Wrap="false" Direction="LeftToRight" EnableTheming="False" 
            Font-Names="Arial-Narrow" Font-Size="11pt">

            <asp:GridView ID="GridView" runat="server" HeaderStyle-BackColor="PowderBlue" Width="100%" Font-Size="13px"
                Font-Names="Arial-Narrow" AllowPaging="True" ViewStateMode="Enabled" PageSize="21" CssClass="Grid"
                OnPageIndexChanging="PageIndexChanging" AutoGenerateColumns="false" ShowFooter="True">
                    <Columns>
                       <asp:BoundField DataField="Date" HeaderText="Call Date"/>
                        <asp:BoundField DataField="ANS Name" HeaderText="ANS Name"/>
                        <asp:BoundField DataField="IOS" HeaderText="IOS"/>
                        <asp:BoundField DataField="Egress Carrier" HeaderText="Egress Carrier"/>
                        <asp:BoundField DataField="Country" HeaderText="Country"/>
                        <asp:BoundField DataField="Destination" HeaderText="Destination"/>
                        <asp:BoundField DataField="Prefix" HeaderText="Prefix"/>
                        <asp:BoundField DataField="No. Of Calls" HeaderText="No. Of Calls"/>
                        <asp:BoundField DataField="Actual Minute" HeaderText="Actual Minute"/>
                        <asp:BoundField DataField="Billing Minute" HeaderText="Billing Minute"/>
                        <asp:BoundField DataField="Carrier Rate ($)" HeaderText="Carrier Rate ($)"/>
                        <asp:BoundField DataField="Y Rate ($)" HeaderText="Y Rate ($)"/>
                        <asp:BoundField DataField="Rate difference" HeaderText="Rate difference"/>
                        <asp:BoundField DataField="DBL Protion (BDT)" HeaderText="DBL Portion (BDT)"/>
                        <asp:BoundField DataField="Invoice Amount" HeaderText="Invoice Amount"/>
                        
                    </Columns>
                    <PagerSettings FirstPageImageUrl="First" FirstPageText="First" LastPageText="Last" PageButtonCount="30" />
                    <PagerStyle HorizontalAlign="Center" />
                    <RowStyle HorizontalAlign="Center"/>
                    <AlternatingRowStyle BackColor="#EBF0FF" ForeColor="#284775"/>
                    <EditRowStyle HorizontalAlign="Center" />
                    <HeaderStyle HorizontalAlign="Center" BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
                     <FooterStyle BackColor="#5D7B9D" Font-Bold="true" ForeColor="White" />
                </asp:GridView>
         </asp:Panel>
    </div>


    <%-- Export to Excel button--%>
     <%--background-color:rosybrown;--%>
     <div id="Export" style="clear:both; margin-left:20px; float:right; padding-bottom:5px; margin-right:2px; height:auto;padding-top:5px;width:1030px;margin-bottom:2px;">
        <div style='float: right;'>
          <asp:Button ID="btnExporttoCSV" runat="server" Text="Export to CSV" Font-Names="Arial-Narrow" OnClick="btnExporttoCSV_Click"/>
        </div>
     </div>
    


</asp:Content>