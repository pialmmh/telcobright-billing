<%@ Page Title="" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeBehind="PaymentManagement.aspx.cs" Inherits="PortalApp.config.PaymentManagement" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <span style="color:Black;"><b> PAYMENT MANAGEMENT: </b>
    
    </span>
    <asp:ToolkitScriptManager ID="ScriptManager1" runat="server">
</asp:ToolkitScriptManager>
     <asp:GridView ID="GridView" OnRowDataBound="GridView_RowDataBound"
                Font-Names="Arial"  DataKeyNames="idPartner"
                Font-Size="0.75em" 
                CellPadding="4" 
                ForeColor="#333333"
                AutoGenerateColumns="False"
                runat="server">
                
                <EditRowStyle BackColor="#999999" />
                <FooterStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
                
                <HeaderStyle BackColor="#5D7B9D" ForeColor="white" Font-Bold="True" />
                <AlternatingRowStyle BackColor="White" ForeColor="#284775" />
    <Columns>
        <asp:TemplateField>
            <ItemTemplate>
                <asp:DropDownList runat="server" ID="ddlPartner" DataValueField="idPartner" DataTextField="PartnerName" Enabled="False"/>
            </ItemTemplate>
        </asp:TemplateField>
        <asp:BoundField DataField="accountName" HeaderText="Account Name"/>
         <asp:BoundField DataField="serviceGroup" HeaderText="Service"/>
        <asp:BoundField DataField="uom" HeaderText="Currency"/>
<%--         <asp:BoundField DataField="PaymentMode" HeaderText="PaymentMode"/>
         <asp:BoundField DataField="CurrentBalance" HeaderText="CurrentBalance"/>
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
  <asp:Label ID="lblresult" runat="server"/>
<asp:Button ID="btnShowPopup" runat="server" style="display:none" />
    <script src="http://ajax.googleapis.com/ajax/libs/jquery/1.11.1/jquery.min.js"></script>
                <script type="text/javascript">
    
                // Adds a new row in service drop down table
                    function addNewServiceRow() {
                    //alert();

                        var $firstRow = $('#addPaymentRow').eq(0);
                    var $newRow = $firstRow.clone();

                    $newRow.insertAfter($firstRow);

                }

               
            </script>

<asp:ModalPopupExtender ID="ModalPopupExtender1" runat="server" TargetControlID="btnShowPopup" PopupControlID="pnlpopup"
CancelControlID="btnCancel" BackgroundCssClass="modalBackground" >
</asp:ModalPopupExtender>
<asp:Panel ID="pnlpopup" runat="server" BackColor="White" Height="269px" Width="400px" style="display:none">
    <table width="100%" style="border:Solid 3px #0094ff; width:100%; height:100%" cellpadding="0" cellspacing="0">
<tr style="background-color:#0094ff">
<td colspan="2" style=" height:10%; color:White; font-weight:bold; font-size:larger" align="center">Carrier Details</td>
</tr>
<tr>
<td align="right" style=" width:45%">
ID:
</td>
<td>
<asp:Label ID="lblID" runat="server"></asp:Label>
</td>
</tr>
<tr>
<td align="right">
Partner:
</td>
<td>
<asp:Label ID="lblusername" runat="server"></asp:Label>
</td>
</tr>
<tr>
    <td align="right">Service: </td>
    <td>
        <asp:Label ID="lblSer" runat="server"></asp:Label>
    </td>
    </tr>
        <tr>
            <td align="right">Type: </td>
    <td>
        <asp:DropDownList ID="ddlistType" runat="server"
                DataTextField="Type"
                AutoPostBack="false">
             <asp:ListItem Value="Select Type" Selected="True"></asp:ListItem>
             <asp:ListItem Text="TopUp"> </asp:ListItem>
            <asp:ListItem Text="Credit"> </asp:ListItem>
        </asp:DropDownList>
    </td>
        </tr>
     <tr>
            <td align="right">Payment Reference: </td>
    <td>
        <asp:TextBox ID="paymentReference" runat="server"> </asp:TextBox>
    </td>
        </tr>

        <tr>
            <td align="right">Comment: </td>
    <td>
        <asp:TextBox ID="comment" runat="server" TextMode="MultiLine"> </asp:TextBox>
    </td>
        </tr>
    <tr>
        <td>Date</td>
         <td>Amount</td>
    
    </tr>
   
          <tr id="addPaymentRow">
             
              <td>

                  <asp:TextBox id="txtDate" Runat="server" /> 
        <asp:CalendarExtender ID="CalendarStartDate" runat="server" 
            TargetControlID="txtDate"  PopupButtonID="txtDate" Format="yyyy-MM-dd">
        </asp:CalendarExtender>
                  </td>
               <td style="padding:3px"> <asp:TextBox id="txtAmount" Runat="server" /> </td>
          </tr>
    <%--<tr> <td align="center"><button onclick="addNewServiceRow();" type="button">Add New</button></td></tr>--%>
   
        <tr>
            <td></td>
            <td>
                <asp:Button ID="btnOK" runat="server" CommandName="OK" Text="OK" OnClick="btnOK_Click"  />
                <asp:Button ID="btnCancel" runat="server" Text="Cancel" />
            </td>
        </tr>
</table>

</asp:Panel>
</asp:Content>
