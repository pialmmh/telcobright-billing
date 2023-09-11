<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/Site.master" EnableEventValidation="false" CodeBehind="EgressInvoiceClearance.aspx.cs" Inherits="PortalApp.reports.Accounts.EgressInvoiceClearence" %>


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

    <div id="report" style="clear:both; padding-bottom:10px; margin-left:10px; border:solid 1px; height:50px;background-color:#FFFAF0;padding-left:5px;padding-top:10px;width:auto;margin-bottom:2px;">
        <asp:Label ID="lblStartDate" runat="server" Text="Start Date:" ></asp:Label>
        <asp:TextBox ID="txtStartDate" runat="server" ClientIDMode="Static" Font-Names="Arial-Narrow"></asp:TextBox>


        <asp:Label ID="lblEndDate" runat="server" Text="End Date:"></asp:Label>
        <asp:TextBox ID="txtEndDate" runat="server" ClientIDMode="Static" Font-Names="Arial-Narrow"></asp:TextBox>


        <asp:Label ID="lblISOName" runat="server" Text="Select Carrier Name:"></asp:Label>
        <asp:DropDownList ID="dpdIsoName" runat="server" Font-Names="Arial-Narrow">
            <asp:ListItem>All</asp:ListItem>
        </asp:DropDownList>

         <asp:CheckBox ID="chkdetailreport" Text="Details Report" runat="server" />
          

     <div style="margin-top:10px;">
        <asp:Label ID="lblGmt" runat="server" Text="Select GMT:"></asp:Label>
        <asp:DropDownList ID="dpdgmt" runat="server" Width="165px" Font-Names="Arial-Narrow">
            <asp:ListItem>Select GMT</asp:ListItem>
            <asp:ListItem>GMT 12</asp:ListItem>
            <asp:ListItem>GMT 11</asp:ListItem>
            <asp:ListItem>GMT 10</asp:ListItem>
            <asp:ListItem>GMT 9</asp:ListItem>
            <asp:ListItem>GMT 8</asp:ListItem>
            <asp:ListItem>GMT 7</asp:ListItem>
            <asp:ListItem>GMT 6</asp:ListItem>
            <asp:ListItem>GMT 5</asp:ListItem>
             <asp:ListItem>GMT 4</asp:ListItem>
            <asp:ListItem>GMT 3</asp:ListItem>
            <asp:ListItem>GMT 2</asp:ListItem>
            <asp:ListItem>GMT 1</asp:ListItem>
            <asp:ListItem>GMT 0</asp:ListItem>
            <asp:ListItem>GMT -1</asp:ListItem>
            <asp:ListItem>GMT -2</asp:ListItem>
            <asp:ListItem>GMT -3</asp:ListItem>
            <asp:ListItem>GMT -4</asp:ListItem>
            <asp:ListItem>GMT -5</asp:ListItem>
            <asp:ListItem>GMT -6</asp:ListItem>
            <asp:ListItem>GMT -7</asp:ListItem>
            <asp:ListItem>GMT -8</asp:ListItem>
            <asp:ListItem>GMT -9</asp:ListItem>
            <asp:ListItem>GMT -10</asp:ListItem>
            <asp:ListItem>GMT -11</asp:ListItem>
            <asp:ListItem>GMT -12</asp:ListItem>
            <asp:ListItem>GMT -13</asp:ListItem>
            <asp:ListItem>GMT -14</asp:ListItem>
        </asp:DropDownList>

        <asp:Button ID="btnShowReport" runat="server" Font-Names="Arial-Narrow" Text="Show Report" OnClick="btnShowReport_Click" />
     </div>


    </div>

    <div id="gridview" style="clear:both; margin-left:10px; margin-right:2px; height:auto;padding-top:12px;width:1050px;margin-bottom:2px;">
       <asp:Panel ID="panel" runat="server" ScrollBars="Auto" Width="100%" 
            Height="490px" Wrap="false" Direction="LeftToRight" EnableTheming="False" 
            Font-Names="Times New Roman" Font-Size="11pt">

            <asp:GridView ID="GridView" runat="server" HeaderStyle-BackColor="PowderBlue" Width="100%" Font-Size="13px"
                    ShowFooter="true"  OnPageIndexChanging="PageIndexChanging" AutoGenerateColumns="false"
                Font-Names="Arial-Narrow" AllowPaging="True" ViewStateMode="Enabled" PageSize="21" CssClass="Grid">
                    <Columns>
                        <asp:BoundField DataField="Date" HeaderText="Call Date"/>
                        <asp:BoundField DataField="Egress Carrier" HeaderText="Egress Carrier"/>
                        <asp:BoundField DataField="Destination" HeaderText="Destination"/>
                        <asp:BoundField DataField="Dial Code" HeaderText="Dial Code"/>
                        <asp:BoundField DataField="Call Qty." HeaderText="Call Qty."/>
                        <asp:BoundField DataField="Duration (Min)" HeaderText="Duration (Min)"/>
                        <asp:BoundField DataField="Rate ($)" HeaderText="Rate ($)"/>
                        <asp:BoundField DataField="Bill Amount ($)" HeaderText="Bill Amount ($)"/>
                    </Columns>
                    <PagerSettings FirstPageImageUrl="First" FirstPageText="First" LastPageText="Last" PageButtonCount="30" />
                    <PagerStyle HorizontalAlign="Center" />
                    <RowStyle HorizontalAlign="Center"/>
                    <AlternatingRowStyle BackColor="#EBF0FF" ForeColor="#284775"/>
                    <EditRowStyle HorizontalAlign="Center" />
                    <HeaderStyle HorizontalAlign="Center" BackColor="#08605c" Font-Bold="True" ForeColor="White"/>
                    <FooterStyle BackColor="#08605c" Font-Bold="true" ForeColor="White" HorizontalAlign="Center"/>
                    
                </asp:GridView>


           <asp:GridView ID="GridView1" runat="server" HeaderStyle-BackColor="PowderBlue" Width="100%" Font-Size="13px"
                    ShowFooter="true"  OnPageIndexChanging="PageIndexChangingDetail" AutoGenerateColumns="false"
                Font-Names="Arial-Narrow" AllowPaging="True" ViewStateMode="Enabled" PageSize="21" CssClass="Grid">
                    <Columns>
                        <asp:BoundField DataField="Date" HeaderText="Call Date"/>
                        <asp:BoundField DataField="Call Start Time" HeaderText="Call Start Time"/>
                        <asp:BoundField DataField="Call End Time" HeaderText="Call End Time"/>
                        <asp:BoundField DataField="Egress Carrier" HeaderText="Egress Carrier"/>
                        <asp:BoundField DataField="Outbound IP" HeaderText="Outbound IP"/>
                        <asp:BoundField DataField="A Number" HeaderText="A Number"/>
                        <asp:BoundField DataField="B Number" HeaderText="B Number"/>
                        <asp:BoundField DataField="Dial Code" HeaderText="Dial Code"/>
                        <asp:BoundField DataField="Duration (Min)" HeaderText="Duration (Min)"/>
                        <asp:BoundField DataField="Rate ($)" HeaderText="Rate ($)"/>
                        <asp:BoundField DataField="Egress Amount ($)" HeaderText="Egress Amount ($)"/>                       
                    </Columns>
                    <PagerSettings FirstPageImageUrl="First" FirstPageText="First" LastPageText="Last" PageButtonCount="30" />
                    <PagerStyle HorizontalAlign="Center" />
                    <RowStyle HorizontalAlign="Center"/>
                    <AlternatingRowStyle BackColor="#EBF0FF" ForeColor="#284775"/>
                    <EditRowStyle HorizontalAlign="Center" />
                    <HeaderStyle HorizontalAlign="Center" BackColor="#08605c" Font-Bold="True" ForeColor="White"/>
                    <FooterStyle BackColor="#08605c" Font-Bold="true" ForeColor="White" />
                    
                </asp:GridView>

           <br />
            <asp:Button ID="btnExporttoCSV" runat="server" Font-Names="Arial-Narrow" CssClass="btnExport" Text="Export to CSV" OnClick="btnExporttoCSV_Click"/>
            <asp:Button ID="btnExportCSVDetails" runat="server" Font-Names="Arial-Narrow" CssClass="btnExport" Text="Export to CSV" OnClick="btnExportCSVDetails_Click"/>
         </asp:Panel>
    </div>


    <%-- Export to Excel button--%>
     <%--background-color:rosybrown;--%>
     <div id="Export" style="clear:both; margin-left:20px; float:right; padding-bottom:5px; margin-right:2px; height:auto;padding-top:5px;width:1030px;margin-bottom:2px;">
        <div style='float: right;'>
         
        </div>
     </div>
    


</asp:Content>
