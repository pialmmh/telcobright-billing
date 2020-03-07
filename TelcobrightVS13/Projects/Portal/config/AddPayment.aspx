<%@ Page Title="" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeBehind="AddPayment.aspx.cs" Inherits="PortalApp.config.AddPayment" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <span style="color:Black;"><b> PAYMENT MANAGEMENT: </b>
    
    </span>
    <asp:ToolkitScriptManager ID="ScriptManager1" runat="server">
</asp:ToolkitScriptManager>

    <div align="left">
    <table width="100%" style="border:Solid 3px #5d7b9d; width:680px; height:100%" cellpadding="0" cellspacing="2">
    <tr style="background-color:#5d7b9d">
        <td colspan="2" style=" height:10%; color:White; font-weight:bold; font-size:larger" align="center">Add Payment</td>
    </tr>
    <tr>
        <td align="right" style=" width:25%"><strong>ID: </strong></td>
        <td><asp:Label ID="lblID" runat="server"></asp:Label></td>
    </tr>
    <tr>
        <td align="right"><strong>Partner: </strong></td>
        <td style="padding:3px"><asp:Label ID="lblusername" runat="server"></asp:Label></td>
    </tr>
    <tr>
        <td align="right"><strong>Service: </strong></td>
        <td style="padding:3px"><asp:Label ID="lblSer" runat="server"></asp:Label></td>
    </tr>
    <tr>
        <td align="right"><strong>Current Balance: </strong></td>
        <td style="padding:3px"><asp:Label ID="lblCurrentBalance" runat="server"></asp:Label></td>
    </tr>
<%--    <tr>
        <td align="right">Type: </td>
        <td style="padding:3px">
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
        <td style="padding:3px"><asp:TextBox ID="paymentReference" runat="server"> </asp:TextBox></td>
    </tr>
    <tr>
        <td align="right">Comment: </td>
        <td style="padding:3px"><asp:TextBox ID="comment" runat="server" TextMode="MultiLine"> </asp:TextBox></td>
    </tr>--%>
    <tr>
        <td align="right"><strong>Type: </strong></td>
        <td style="padding:3px">
            <asp:DropDownList ID="ddlistType" runat="server" AutoPostBack="false" />
        </td>
    </tr>
    <tr>
        <td align="right"><strong>Date: </strong></td>
        <td style="padding:3px">
            <asp:TextBox id="txtDate" Runat="server" /> 
            <asp:CalendarExtender ID="CalendarStartDate" runat="server" 
                                  TargetControlID="txtDate"  PopupButtonID="txtDate" Format="yyyy-MM-dd">
            </asp:CalendarExtender>
        </td>
    </tr>
    <tr>
        <td></td>
        <td>
            <div style="font-size: smaller; text-align: left; overflow: visible; clear: left; color: #8B4500;">[Enter only Date in "yyyy-MM-dd (e.g. 2012-11-21) format]</div>
        </td>
    </tr>
    <tr id="addPaymentRow">
        <td align="right"><strong>Amount: </strong></td>    
        <td style="padding:3px"><asp:TextBox id="txtAmount" Runat="server" AutoPostBack="true" OnTextChanged="txtAmount_TextChanged" Text="0" /> </td>
    </tr>
        <tr>
            <td colspan="2" align="center"><asp:CheckBox runat="server" Text="Update Threshold Settings" ID="cbThresholdSettings"/></td>
        </tr>
        <tr>
            <td colspan="2" align="center">
                <asp:GridView ID="gvThreshold" runat="server"
                    Font-Names="Arial"  DataKeyNames="id" Font-Size="9pt" CellPadding="4" ForeColor="#333333" AutoGenerateColumns="False"
                    OnRowDataBound="gvThreshold_RowDataBound">
                <EditRowStyle BackColor="#999999" />
                <FooterStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />                
                <HeaderStyle BackColor="#5D7B9D" ForeColor="white" Font-Bold="True" />
                <AlternatingRowStyle BackColor="White" ForeColor="#284775" />
                <PagerStyle BackColor="#284775" ForeColor="White" HorizontalAlign="Center" />
                <RowStyle BackColor="#F7F6F3" ForeColor="#333333" />
                <SelectedRowStyle BackColor="#E2DED6" Font-Bold="True" ForeColor="#333333" />
                <SortedAscendingCellStyle BackColor="#E9E7E2" />
                <SortedAscendingHeaderStyle BackColor="#506C8C" />
                <SortedDescendingCellStyle BackColor="#FFFDF8" />
                <SortedDescendingHeaderStyle BackColor="#6F8DAE" />
                    <Columns>
                        <asp:TemplateField HeaderText="Limit">
                            <ItemTemplate>
                                <asp:TextBox runat="server" ID="txtLimit" Enabled="false" />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Action">
                            <ItemTemplate>
                                <asp:DropDownList runat="server" ID="ddlAccountAction" DataTextField="ActionName" DataValueField="Id" 
                                    Enabled="false" />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Rule">
                            <ItemTemplate>
                                <asp:Label runat="server" ID="lblRule" />
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </td>
        </tr>
        <tr>
            <td colspan="2"><asp:LinkButton runat="server" ID="lbAddRule" Text="Add New Rule" OnClick="lbAddRule_Click" /></td>
        </tr>
    <%--<tr> <td align="center"><button onclick="addNewServiceRow();" type="button">Add New</button></td></tr>--%>
        <tr>
            <td colspan="2">&nbsp;</td>
        </tr>
    <tr>
        <td></td>
        <td>
                <asp:Button ID="btnOK" runat="server" CommandName="OK" Text="OK" OnClick="btnOK_Click"  />
                <asp:Button ID="btnCancel" runat="server" Text="Cancel" OnClick="btnCancel_Click" />
        </td>
    </tr>
</table>

    </div>
<asp:Button ID="btnShowRule" runat="server" style="display:none" />
<asp:ModalPopupExtender ID="mpeRule" runat="server" TargetControlID="btnShowRule" PopupControlID="pnlRule"
CancelControlID="btnRuleCancel" BackgroundCssClass="modalBackground" BehaviorID="pnlRule" >
</asp:ModalPopupExtender>
<asp:Panel ID="pnlRule" runat="server" BackColor="White" Height="269px" Width="400px" style="display:none;">
    <asp:UpdatePanel runat="server">
        <ContentTemplate>
            <table width="100%" style="border:Solid 3px #5d7b9d; width:100%; height:100%" cellpadding="0" cellspacing="0">
                <tr style="background-color:#5d7b9d">
                    <td colspan="2" style=" height:10%; color:White; font-weight:bold; font-size:larger" align="center">Account Action Rule</td>
                </tr>
                <tr>
                    <td style="width:120px">Action:</td>
                    <td><asp:DropDownList runat="server" ID="ddlAccountAction" DataTextField="ActionName" DataValueField="Id"  /></td>
                </tr>
                <tr>
                    <td><asp:RadioButton runat="server" ID="rbIsFixedAmount" Text="Fixed Amount" GroupName="grpRule" /></td>
                    <td><asp:TextBox runat="server" ID="txtFixedAmount" Text="0" OnTextChanged="CalculateThresholdValue" AutoPostBack="true" /></td>
                </tr>
                <tr>
                    <td><asp:RadioButton runat="server" ID="rbIsPercent" Text="Percentage" GroupName="grpRule" /></td>
                    <td><asp:TextBox runat="server" ID="txtPercentage" Text="0" OnTextChanged="CalculateThresholdValue" AutoPostBack="true" /> %</td>
                </tr>
                <tr>
                    <td><asp:RadioButton runat="server" ID="rbIsFormulaBased" Text="Formula Based" GroupName="grpRule" /></td>
                    <td>
                        <asp:Label runat="server" Text="ACD:" Width="60px" /><asp:TextBox runat="server" ID="txtACD" Text="0" OnTextChanged="CalculateThresholdValue" AutoPostBack="true" /> Sec<br />
                        <asp:Label runat="server" Text="ACR:" Width="60px" /><asp:TextBox runat="server" ID="txtACR" Text="0" OnTextChanged="CalculateThresholdValue" AutoPostBack="true" /> $<br />
                        <asp:Label runat="server" Text="Minute:" Width="60px" /><asp:TextBox runat="server" ID="txtMinute" Text="0" OnTextChanged="CalculateThresholdValue" AutoPostBack="true" />
                        <asp:Label runat="server" Text="Ports:" Width="60px" /><asp:TextBox runat="server" ID="txtNoOfPorts" Text="0" OnTextChanged="CalculateThresholdValue" AutoPostBack="true" />
                    </td>
                </tr>
                <tr>
                    <td>&nbsp;&nbsp;<b>Result Limit:</b></td>
                    <td>
                        <asp:HiddenField runat="server" ID="hfRowIndex" />
                        <asp:TextBox runat="server" ID="txtResult" />

                    </td>
                </tr>
            </table>
        </ContentTemplate>
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="txtFixedAmount" EventName="TextChanged" />
            <asp:AsyncPostBackTrigger ControlID="txtPercentage" EventName="TextChanged" />
            <asp:AsyncPostBackTrigger ControlID="txtACD" EventName="TextChanged" />
            <asp:AsyncPostBackTrigger ControlID="txtACR" EventName="TextChanged" />
            <asp:AsyncPostBackTrigger ControlID="txtMinute" EventName="TextChanged" />
            <asp:AsyncPostBackTrigger ControlID="txtNoOfPorts" EventName="TextChanged" />
        </Triggers>
    </asp:UpdatePanel>
    <table width="100%" style="width:100%" cellpadding="0" cellspacing="0">
        <tr>
            <td></td>
            <td>
                    <asp:Button ID="btnRuleOK" runat="server" CommandName="OK" Text="OK" OnClick="btnRuleOK_Click"  />
                    <asp:Button ID="btnRuleCancel" runat="server" Text="Cancel" />
            </td>
        </tr>
    </table>
</asp:Panel>
</asp:Content>
