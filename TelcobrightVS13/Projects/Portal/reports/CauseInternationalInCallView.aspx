<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="True"
    CodeBehind="~/reports/CauseInternationalInCallView.aspx.cs" Inherits="DefaultCIntlInCallView1" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">
    <script type="text/javascript">
     function SetTarget() {

      document.forms[0].target = "_blank";
  }
  function setTargetInit() {
      document.forms[0].target = "_self";
  }
 </script>
</asp:Content>
<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">
    
    <div>
        <asp:Label runat="server" ID="lbl1"></asp:Label>
    </div>
     <div id="gridViewArea" style="margin-top:12px;">

    
    
    <asp:Button ID="Button2" runat="server" 
            style="margin-left: 0px" Text="Export First " Visible="true" 
             onclick="Button2_Click" />
             <asp:TextBox ID="TextBoxNoOfRecords" runat="server" Text="1000"></asp:TextBox> Records or, 
     <span style="padding-left:5px;">
     <asp:Button ID="Button1" runat="server" 
            style="margin-left: 0px" Text="Export All" Visible="true" 
             onclick="Button1_Click" />
     </span>
     <span style="padding-left:5px;"> (Might take long...)</span>

         <asp:Label ID="lblStatus" runat="server" Text=""></asp:Label>

            <asp:GridView ID="gridView" AllowPaging="True"
                runat="server" onrowcommand="gridView_RowCommand" 
                CellPadding="4" ForeColor="#333333" GridLines="Vertical" 
                Font-Size="Smaller" PageSize="5000">
                <AlternatingRowStyle BackColor="White" ForeColor="#284775" />
                <HeaderStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
                <EditRowStyle BackColor="#999999" />
                <FooterStyle BackColor="#5D7B9D" Font-Bold="true" ForeColor="White" />
                <PagerStyle BackColor="#284775" ForeColor="White" HorizontalAlign="Center" />
                <RowStyle BackColor="#F7F6F3" ForeColor="#333333" />
                <SelectedRowStyle BackColor="#E2DED6" Font-Bold="True" ForeColor="#333333" />
                <PagerTemplate>
                    <table width="100%">
                    <tr>
                        <td style="text-align: Left">
                            <asp:PlaceHolder ID="placeholder" runat="server" />
                        </td>
                    </tr>
                </table>
                </PagerTemplate>
                <SortedAscendingCellStyle BackColor="#E9E7E2" />
                <SortedAscendingHeaderStyle BackColor="#506C8C" />
                <SortedDescendingCellStyle BackColor="#FFFDF8" />
                <SortedDescendingHeaderStyle BackColor="#6F8DAE" />
            </asp:GridView>
        </div>
</asp:Content>
