<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="BTRCReport.aspx.cs" EnableEventValidation="false" Inherits="PortalApp.reports.BTRC.BtrcReport" MasterPageFile="~/Site.master" %>

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
</asp:Content>



<%--Main body portion--%>
<asp:content id="Content2" contentplaceholderid="MainContent" runat="server">
     <div id="errormsg" style="clear:both; margin-left:10px; height:15px;background-color:lightblue;padding-left:5px;width:auto;margin-bottom:2px;">
        <div style='text-align:center'>
            <asp:Label ID="lblErrMsg" runat="server" ForeColor="Red"></asp:Label>
        </div>
    </div>

    <div id="report" style="clear:both; margin-left:10px; height:35px;background-color:lightblue;padding-left:5px;padding-top:10px;width:auto;margin-bottom:2px;">
        <asp:Label ID="lblStartDate" runat="server" Text="Start Date:" ></asp:Label>
        <asp:TextBox ID="txtStartDate" runat="server" ClientIDMode="Static"></asp:TextBox>


        <asp:Label ID="lblEndDate" runat="server" Text="End Date:"></asp:Label>
        <asp:TextBox ID="txtEndDate" runat="server" ClientIDMode="Static"></asp:TextBox>


        <asp:Button ID="btnShowReport" runat="server" Text="Show Report" OnClick="btnShowReport_Click" />

    </div>

    <div id="gridview" style="clear:both; margin-left:10px; margin-right:2px; height:auto;padding-top:12px;width:1050px;margin-bottom:2px;">
       <asp:Panel ID="panel" runat="server" ScrollBars="Auto" Width="100%" 
            Height="353px" Wrap="false" Direction="LeftToRight" EnableTheming="False" BackColor="WhiteSmoke"
             Font-Names="Arial-Narrow" Font-Size="11pt">

            <asp:GridView ID="GridView" runat="server" HeaderStyle-BackColor="PowderBlue" Width="100%" AutoGenerateColumns="False"
                    Font-Names="Arial-Narrow" AllowPaging="True" ViewStateMode="Enabled" PageSize="10" ShowFooter="True"
                onrowcreated="grdViewProducts_RowCreated"  OnRowDataBound="grdViewProducts_RowDataBound" OnPageIndexChanging="PageIndexChanging" CellPadding="4" ForeColor="#333333" GridLines="Vertical">
                    <Columns>
                        <asp:BoundField DataField="Call Date" HeaderText="Call Date"/>
                        <asp:BoundField DataField="Incoming Call No." HeaderText="Number of Calls" />
                        <asp:BoundField DataField="Incoming Duration (Min)" HeaderText="Paid Mins" ItemStyle-HorizontalAlign="Center" DataFormatString="{0:N2}">
                        <ItemStyle HorizontalAlign="Center" />
                        </asp:BoundField>
                        <asp:BoundField DataField="Outgoing Call No." HeaderText="Number of Calls" />
                        <asp:BoundField DataField="Outgoing Duration" HeaderText="Paid Mins" DataFormatString="{0:N2}"/>
                    </Columns>
                    <PagerSettings FirstPageImageUrl="First" FirstPageText="First" LastPageText="Last" PageButtonCount="16" />
                    <PagerStyle HorizontalAlign="Center" BackColor="#284775" ForeColor="White" />
                    <RowStyle HorizontalAlign="Center" BackColor="#F7F6F3" ForeColor="#333333"/>
                    <AlternatingRowStyle BackColor="White" ForeColor="#284775"/>
                    <EditRowStyle HorizontalAlign="Center" BackColor="#999999" />
                    <FooterStyle BackColor="#5D7B9D" Font-Bold="true" ForeColor="White"/>
                    <HeaderStyle HorizontalAlign="Center" Font-Bold="true" BackColor="#5D7B9D" ForeColor="White"/>
                    
                    <SelectedRowStyle BackColor="#E2DED6" Font-Bold="True" ForeColor="#333333" />
                    <SortedAscendingCellStyle BackColor="#E9E7E2" />
                    <SortedAscendingHeaderStyle BackColor="#506C8C" />
                    <SortedDescendingCellStyle BackColor="#FFFDF8" />
                    <SortedDescendingHeaderStyle BackColor="#6F8DAE" />
                    
                </asp:GridView>
         </asp:Panel>
    </div>


    <%-- Export to Excel button--%>
     <%--background-color:rosybrown;--%>
     <div id="Export" style="clear:both; margin-left:20px; float:right; padding-bottom:5px; margin-right:2px; height:auto;padding-top:5px;width:1030px;margin-bottom:2px;">
        <div style='float: right;'>
          <asp:Button ID="btnExporttoCSV" runat="server" Text="Export to Excel" OnClick="btnExporttoCSV_Click" />
        </div>
     </div>
</asp:content>
