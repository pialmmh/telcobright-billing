<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="BDIncomingBusiness.aspx.cs" MasterPageFile="~/Site.master" EnableEventValidation="false" Inherits="PortalApp.reports.Accounts.BdIncomingBusiness" %>


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
    .btnExport 
     {
       float:right;
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

    <div id="report" style="clear:both; border:solid 1px; margin-left:10px; font-family:Arial-Narrow; height:35px;background-color:#FFFAF0;padding-left:5px;padding-top:10px;width:auto;margin-bottom:2px;">
        <asp:Label ID="lblStartDate" runat="server" Text="Start Date:" ></asp:Label>
        <asp:TextBox ID="txtStartDate" runat="server" ClientIDMode="Static" Font-Names="Arial-Narrow"></asp:TextBox>


        <asp:Label ID="lblEndDate" runat="server" Text="End Date:"></asp:Label>
        <asp:TextBox ID="txtEndDate" runat="server" ClientIDMode="Static" Font-Names="Arial-Narrow"></asp:TextBox>
      

        <asp:Button ID="btnShowReport" runat="server" Text="Show Report" Font-Names="Arial-Narrow" OnClick="btnShowReport_Click"/>

    </div>

    <div id="gridview" style="clear:both; margin-left:10px; margin-right:2px; height:auto;padding-top:12px;width:1050px;margin-bottom:2px;">
       <asp:Panel ID="panel" runat="server" ScrollBars="Auto" Width="100%" 
            Height="490px" Wrap="false" Direction="LeftToRight" EnableTheming="False" 
            Font-Names="Times New Roman" Font-Size="11pt">

            <asp:GridView ID="GridView" runat="server" HeaderStyle-BackColor="PowderBlue" Width="100%" Font-Size="13px"
                   Font-Names="Arial-Narrow" AllowPaging="True" ViewStateMode="Enabled" PageSize="21" CssClass="Grid"
                OnPageIndexChanging="PageIndexChanging" AutoGenerateColumns="false" ShowFooter="true">
                    <Columns>
                        <asp:BoundField DataField="Call Date" HeaderText="Call Date"/>
                        <asp:BoundField DataField="Ingress Carrier" HeaderText="Ingress Carrier"/>
                        <asp:BoundField DataField="Call qty." HeaderText="Call qty."/>
                        <asp:BoundField DataField="Duration (Min)" HeaderText="Duration (Min)"/>
                        <asp:BoundField DataField="Ingress Rate ($)" HeaderText="Ingress Rate ($)"/>
                        <asp:BoundField DataField="Ingress Amount ($)" HeaderText="Ingress Amount ($)"/>
                        <asp:BoundField DataField="Egress Carrier" HeaderText="Egress Carrier"/>
                        <asp:BoundField DataField="IOS Amount ($)" HeaderText="IOS Amount ($)"/>
                        <asp:BoundField DataField="BTRC Amount" HeaderText="BTRC Amount"/>
                        <asp:BoundField DataField="DCL Margin ($)" HeaderText="IGW Margin ($)"/>
                    </Columns>
                    <PagerSettings FirstPageImageUrl="First" FirstPageText="First" LastPageText="Last" PageButtonCount="30" />
                    <PagerStyle HorizontalAlign="Center" />
                    <RowStyle HorizontalAlign="Center"/>
                    <AlternatingRowStyle BackColor="#EBF0FF" ForeColor="#284775"/>
                    <EditRowStyle HorizontalAlign="Center" />
                    <HeaderStyle HorizontalAlign="Center" Font-Bold="true" BackColor="#08605c" ForeColor="White"/>
                    <FooterStyle BackColor="#08605c" Font-Bold="true" ForeColor="White" HorizontalAlign="Center"/>
                </asp:GridView>
           <br />
           <asp:Button ID="btnExporttoCSV" runat="server" Font-Names="Arial-Narrow" Text="Export to CSV" CssClass="btnExport" OnClick="btnExporttoCSV_Click"/>
         </asp:Panel>
    </div>


    <%-- Export to Excel button--%>
     <%--background-color:rosybrown;--%>
     <div id="Export" style="clear:both; margin-left:20px; float:right; padding-bottom:5px; margin-right:2px; height:auto;padding-top:5px;width:1030px;margin-bottom:2px;">
        <div style='float: right;'>
          
        </div>
     </div>
    


</asp:Content>
