<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="True"
    CodeBehind="~/reports/CdrError.aspx.cs" Inherits="DefaultCdrError" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>
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
    <ajaxToolkit:ToolkitScriptManager ID="ToolkitScriptManager1" runat="server" > </ajaxToolkit:ToolkitScriptManager>
    

    <div>
        <asp:Label runat="server" ID="lbl1"></asp:Label>
    </div>
    <span style="font-weight:bold;"> Real Time Update 
            <asp:CheckBox ID="CheckBoxRealTimeUpdate" runat="server" Checked="false" AutoPostBack="true" /></span>
    <asp:Button ID="submit" runat="server" Text="Refresh" onclick="submit_Click" />  
     <asp:Timer ID="Timer1" Interval="300000" runat="server" ontick="Timer1_Tick" ></asp:Timer>
   



     <div id="gridViewArea" style="margin-top:12px; width:1000px; overflow:scroll;">
            <asp:GridView ID="gridView" AllowPaging="True"
                runat="server" onrowcommand="gridView_RowCommand" 
                CellPadding="4" ForeColor="#333333" GridLines="Vertical" 
                Font-Size="Smaller" PageSize="5000">
                <AlternatingRowStyle BackColor="White" />
                <HeaderStyle BackColor="#1C5E55" Font-Bold="True" ForeColor="White" />
                <EditRowStyle BackColor="#7C6F57" />
                <FooterStyle BackColor="#1C5E55" Font-Bold="true" ForeColor="White" />
                <PagerStyle BackColor="#666666" ForeColor="White" HorizontalAlign="Center" />
                <RowStyle BackColor="white" />
                <SelectedRowStyle BackColor="#C5BBAF" Font-Bold="True" ForeColor="#333333" />
                <PagerTemplate>
                    <table width="100%">
                    <tr>
                        <td style="text-align: Left">
                            <asp:PlaceHolder ID="placeholder" runat="server" />
                        </td>
                    </tr>
                </table>
                </PagerTemplate>
                <SortedAscendingCellStyle BackColor="#F8FAFA" />
                <SortedAscendingHeaderStyle BackColor="#246B61" />
                <SortedDescendingCellStyle BackColor="#D4DFE1" />
                <SortedDescendingHeaderStyle BackColor="#15524A" />
            </asp:GridView>
        </div>
</asp:Content>
