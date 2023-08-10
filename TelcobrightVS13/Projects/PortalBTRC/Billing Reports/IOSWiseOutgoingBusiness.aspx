<%@ Page Title="" Language="C#" MasterPageFile="~/Site.master" EnableEventValidation="false" AutoEventWireup="true" CodeBehind="IOSWiseOutgoingBusiness.aspx.cs" Inherits="PortalApp.reports.Accounts.DemoReport" %>


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

    <div id="report" style="clear:both; border:solid 1px; margin-left:10px; font-family:Arial-Narrow; height:50px;padding-bottom:10px;background-color:#FFFAF0;padding-left:5px;padding-top:10px;width:auto;margin-bottom:2px;">
        <asp:Label ID="lblStartDate" runat="server" Text="Start Date:" ></asp:Label>
        <asp:TextBox ID="txtStartDate" runat="server" ClientIDMode="Static" Font-Names="Arial-Narrow"></asp:TextBox>


        <asp:Label ID="lblEndDate" runat="server" Text="End Date:"></asp:Label>
        <asp:TextBox ID="txtEndDate" runat="server" ClientIDMode="Static" Font-Names="Arial-Narrow"></asp:TextBox>


        <asp:Label ID="lblISOName" runat="server" Text="Select IOS Name:"></asp:Label>
        <asp:DropDownList ID="dpdIsoName" runat="server" Font-Names="Arial-Narrow">
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


      <div style="margin-top:10px;">
        <asp:Label ID="lblRate" runat="server" Text="Ex. Rate (BDT):"></asp:Label>
        <asp:TextBox ID="txtRate" runat="server" Font-Names="Arial-Narrow" Width="137px"></asp:TextBox>     
           <asp:CheckBox ID="chkdetailreport" Text="Details Report" runat="server" />
        &nbsp;&nbsp;&nbsp;&nbsp; 
        <asp:Button ID="btnShowReport" runat="server" Text="Show Report" Font-Names="Arial-Narrow" OnClick="btnShowReport_Click" />
      </div>

    </div>

    <div id="gridview" style="clear:both; margin-left:10px; margin-right:2px; height:auto;padding-top:12px;width:1050px;margin-bottom:2px;">
       <asp:Panel ID="panel" runat="server" ScrollBars="Auto" Width="100%" 
            Height="490px" Wrap="false" Direction="LeftToRight" EnableTheming="False" 
            Font-Names="Times New Roman" Font-Size="11pt">

            <asp:GridView ID="GridView" runat="server" HeaderStyle-BackColor="PowderBlue" Width="100%" CssClass="Grid" Font-Size="13px"
                   Font-Names="Arial-Narrow" ForeColor="#333333" AllowPaging="True" ViewStateMode="Enabled" OnPageIndexChanging="PageIndexChanging" 
                PageSize="21" ShowFooter="True" AutoGenerateColumns="false">
                    <Columns>
                         <asp:BoundField DataField="Date" HeaderText="Call Date"/>
                         <asp:BoundField DataField="ANS" HeaderText="ANS"/>
                         <asp:BoundField DataField="IOS" HeaderText="IOS"/>
                         <asp:BoundField DataField="Destination" HeaderText="Destination"/>
                         <asp:BoundField DataField="Country" HeaderText="Country"/>
                         <asp:BoundField DataField="Prefix" HeaderText="Prefix"/>
                         <asp:BoundField DataField="No. Of Calls" HeaderText="No. Of Calls"/>
                         <asp:BoundField DataField="Actual Minute" HeaderText="Actual Minute"/>
                         <asp:BoundField DataField="Billing Minute" HeaderText="Billing Minute"/>
                         <asp:BoundField DataField="X (BDT)" HeaderText="X (BDT)"/>
                         <asp:BoundField DataField="Y (USD)" HeaderText="Y (USD)"/>
                         <asp:BoundField DataField="USD Rate" HeaderText="USD Rate"/>
                         <asp:BoundField DataField="Y (BDT)" HeaderText="Y (BDT)"/>
                         <asp:BoundField DataField="Z (BDT)" HeaderText="Z (BDT)"/>
                         <asp:BoundField DataField="15% of Z" HeaderText="15% of Z"/>
                         <asp:BoundField DataField="Invoice Amount" HeaderText="Invoice Amount"/>
                    </Columns>
                    <PagerSettings FirstPageImageUrl="First" FirstPageText="First" LastPageText="Last" PageButtonCount="30" />
                    <PagerStyle HorizontalAlign="Center" />
                    <RowStyle HorizontalAlign="Center"/>
                    <AlternatingRowStyle BackColor="#EBF0FF" ForeColor="#284775"/>
                    <EditRowStyle HorizontalAlign="Center" />
                    <HeaderStyle HorizontalAlign="Center" Font-Bold="true" BackColor="#086052" ForeColor="White"/>
                    <FooterStyle BackColor="#086052" Font-Bold="true" ForeColor="White" />
                </asp:GridView>

            <asp:GridView ID="GridView1" runat="server" HeaderStyle-BackColor="PowderBlue" Width="100%" CssClass="Grid" Font-Size="13px"
                   Font-Names="Arial-Narrow" ForeColor="#333333" AllowPaging="True" ViewStateMode="Enabled" OnPageIndexChanging="PageIndexChangingGridview1" 
                PageSize="21" ShowFooter="True" AutoGenerateColumns="false">
                    <Columns>
                         <asp:BoundField DataField="Call Date" HeaderText="Call Date"/>
                        <asp:BoundField DataField="ANS Name" HeaderText="ANS Name"/>
                         <asp:BoundField DataField="IOS Name" HeaderText="IOS Name"/>
                         <asp:BoundField DataField="IOS IP" HeaderText="IOS IP"/>
                         <asp:BoundField DataField="A Number" HeaderText="A Number"/>
                         <asp:BoundField DataField="B Number" HeaderText="B Number"/>
                         <asp:BoundField DataField="Destination Name" HeaderText="Destination Name"/>
                         <asp:BoundField DataField="Dial Code" HeaderText="Dial Code"/>
                         <asp:BoundField DataField="Actual Duration (Sec)" HeaderText="Actual Duration (Sec)"/>
                         <asp:BoundField DataField="Billed Duration (Sec)" HeaderText="Billed Duration (Sec)"/>
                         <asp:BoundField DataField="X Value" HeaderText="X Value"/>
                         <asp:BoundField DataField="Y Value" HeaderText="Y Value"/>
                         <asp:BoundField DataField="Z Value" HeaderText="Z Value"/>
                         <asp:BoundField DataField="15% of Z" HeaderText="15% of Z"/>
                         <asp:BoundField DataField="Invoice Amount" HeaderText="Invoice Amount"/>                       
                    </Columns>
                    <PagerSettings FirstPageImageUrl="First" FirstPageText="First" LastPageText="Last" PageButtonCount="30" />
                    <PagerStyle HorizontalAlign="Center" />
                    <RowStyle HorizontalAlign="Center"/>
                    <AlternatingRowStyle BackColor="#EBF0FF" ForeColor="#284775"/>
                    <EditRowStyle HorizontalAlign="Center" />
                    <HeaderStyle HorizontalAlign="Center" Font-Bold="true" BackColor="#086052" ForeColor="White"/>
                    <FooterStyle BackColor="#086052" Font-Bold="true" ForeColor="White" />
                </asp:GridView>
           <br />
          
         </asp:Panel>
    </div>


    <%-- Export to Excel button--%>
     <%--background-color:rosybrown;--%>
     <div id="Export" style="clear:both; margin-left:20px; float:right; padding-bottom:5px; margin-right:2px; height:auto;padding-top:5px;width:1030px;margin-bottom:2px;">
        <div style='float: right;'>
         <asp:Button ID="btnExporttoCSV" runat="server" Font-Names="Arial-Narrow" Text="Export to CSV" OnClick="btnExporttoCSV_Click"/>
          <asp:Button ID="btnExportCSVDetails" runat="server" Font-Names="Arial-Narrow" Text="Export to CSV" OnClick="btnExportCSVDetails_Click"/>
        </div>
     </div>
    


</asp:Content>
