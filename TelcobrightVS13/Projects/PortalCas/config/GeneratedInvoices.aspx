<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="GeneratedInvoices.aspx.cs" Inherits="PortalApp.config.GeneratedInvoices" MasterPageFile="~/Site.master" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>
<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">
   
    <script src="../Scripts/moment.js" type="text/javascript"></script>
    <%--Page Load and Other Server Side Asp.net scripts--%>
       
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" Runat="Server">
    <ajaxToolkit:ToolkitScriptManager ID="ToolkitScriptManager1" runat="server" />
    <div>
        <span style="color: black;"><b>Generated Invoices:</b></span><br/>
        <asp:UpdatePanel runat="server">
            <ContentTemplate>
                <fieldset>
                    <div>
                        <asp:CheckBox runat="server" ID="cbServiceAccountFilter" Text="Service Account" Checked="False" 
                                      OnCheckedChanged="cbServiceAccountFilter_OnCheckedChanged" AutoPostBack="True" />
                        <asp:DropDownList ID="ddlistServiceAccountFilter" runat="server" 
                                          Enabled="False" AutoPostBack="False" />                        
                        <asp:CheckBox runat="server" ID="cbPartnerFilter" Text="Partner" Checked="False" 
                                      OnCheckedChanged="cbPartnerFilter_OnCheckedChanged" AutoPostBack="True" />
                        <asp:DropDownList ID="ddlistPartnerFilter" runat="server" 
                                          Enabled="False" AutoPostBack="False" />
                        <asp:CheckBox runat="server" ID="cbMonthFilter" Text="Year/Month:" Checked="True" AutoPostBack="True" OnCheckedChanged="cbMonthFilter_OnCheckedChanged"/>
                        <asp:TextBox ID="TextBoxYear" runat="server" Text="" Width="30px"></asp:TextBox>
                        <asp:DropDownList ID="DropDownListMonth" runat="server" AutoPostBack="False">
                            <asp:ListItem Value="01">Jan</asp:ListItem>
                            <asp:ListItem Value="02">Feb</asp:ListItem>
                            <asp:ListItem Value="03">Mar</asp:ListItem>
                            <asp:ListItem Value="04">Apr</asp:ListItem>
                            <asp:ListItem Value="05">May</asp:ListItem>
                            <asp:ListItem Value="06">Jun</asp:ListItem>
                            <asp:ListItem Value="07">Jul</asp:ListItem>
                            <asp:ListItem Value="08">Aug</asp:ListItem>
                            <asp:ListItem Value="09">Sep</asp:ListItem>
                            <asp:ListItem Value="10">Oct</asp:ListItem>
                            <asp:ListItem Value="11">Nov</asp:ListItem>
                            <asp:ListItem Value="12">Dec</asp:ListItem>
                        </asp:DropDownList>
                        <asp:Button runat="server" ID="btnShow" Text="Show" OnClick="btnShow_OnClick"/>
                    </div>
                </fieldset>
            </ContentTemplate>
        </asp:UpdatePanel>
        <asp:UpdatePanel runat="server">
            <ContentTemplate>
                <div>
                    <asp:GridView ID="gvInvoice" runat="server" AutoGenerateColumns="False" DataKeyNames="INVOICE_ID"
                                  CellPadding="4" ForeColor="#333333" GridLines="Vertical"
                                  Font-Size="9pt" BorderColor="Silver" BorderStyle="Solid" OnRowDataBound="gvInvoice_RowDataBound">
                        <AlternatingRowStyle BackColor="#f2f2f2" ForeColor="#284775"></AlternatingRowStyle>
                        <HeaderStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
                        <PagerStyle BackColor="#284775" ForeColor="White" HorizontalAlign="Center" />
                        <RowStyle BackColor="white" ForeColor="#333333" />
                        <Columns>
                            <asp:TemplateField>
                                <ItemTemplate>
                                    <asp:LinkButton ID="LinkButtonEdit" Text="Edit" runat="server" OnClick="LinkButtonEdit_Click" />
                                </ItemTemplate>
                            </asp:TemplateField>
                            
                            <asp:TemplateField HeaderText="Partner">
                                <ItemTemplate>
                                    <asp:Label runat="server" ID="lblPartner" />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="DESCRIPTION" HeaderText="Description" />
                            <asp:BoundField DataField="REFERENCE_NUMBER" HeaderText="Reference Number" />
                            <asp:TemplateField HeaderText="Invoice Date" SortExpression="INVOICE_DATE" ItemStyle-Wrap="false">
                                <ItemTemplate>
                                    <asp:Label runat="server" ID="lblInvoiceDate" Enabled="false" dataformatstring="{0:yyyy-MM-dd}" />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Due Date" SortExpression="DUE_DATE" ItemStyle-Wrap="false">
                                <ItemTemplate>
                                    <asp:Label runat="server" ID="lblDueDate" Enabled="false" dataformatstring="{0:yyyy-MM-dd}" />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="convertedFinalCurrency" HeaderText="Currency" ItemStyle-HorizontalAlign="Center"/>
                            <asp:BoundField DataField="convertedFinalAmount" HeaderText="Amount" DataFormatString="{0:n2}" ItemStyle-HorizontalAlign="Right"/>
                            <asp:BoundField DataField="" />
                            <asp:TemplateField>
                                <ItemTemplate>
                                    <asp:LinkButton ID="LinkButtonDelete" Text="Delete" runat="server" OnClick="LinkButtonDelete_Click"
                                        OnClientClick='return confirm("Are you sure you want to delete this entry?");' />
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>
            </ContentTemplate>
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="btnShow" EventName="Click"/>
            </Triggers>
        </asp:UpdatePanel>
    </div>

<%--<asp:ToolkitScriptManager ID="ScriptManager1" runat="server" />--%>
<asp:Button ID="btnShowRule" runat="server" style="display:none" />
<asp:ModalPopupExtender ID="mpeInvoice" runat="server" TargetControlID="btnShowRule" PopupControlID="pnlInvoice"
CancelControlID="btnCancel" BackgroundCssClass="modalBackground" BehaviorID="pnlInvoice" >
</asp:ModalPopupExtender>
<asp:Panel ID="pnlInvoice" runat="server" BackColor="White" Height="269px" Width="400px" style="display:none;">
    <asp:UpdatePanel runat="server" ID="upInner" UpdateMode="Conditional">
        <ContentTemplate>
            <table width="100%" style="border:Solid 3px #5d7b9d; width:100%; height:100%" cellpadding="0" cellspacing="0">
                <tr style="background-color:#5d7b9d">
                    <td colspan="2" style=" height:10%; color:White; font-weight:bold; font-size:larger" align="center">Invoice Details</td>
                </tr>
                <tr style="background-color:#5d7b9d">
                    <td colspan="2" style=" height:10%; color:White; font-weight:bold; font-size:larger" align="center">
                        <asp:Label ID="LabelDESCRIPTION" runat="server" />
                    </td>
                </tr>
                <tr>
                    <td style="width:120px">Reference Number:</td>
                    <td><asp:TextBox runat="server" ID="TextBoxReferenceNumber"  /></td>
                </tr>
                <tr>
                    <td style="width:120px">Invoice Date:</td>
                    <td>
                        <asp:TextBox runat="server" ID="TextBoxInvoiceDate"  />
                        <asp:CalendarExtender ID="CalendarInvoiceDate" runat="server"
                                              TargetControlID="TextBoxInvoiceDate" PopupButtonID="TextBoxInvoiceDate" Format="yyyy-MM-dd">
                        </asp:CalendarExtender>
                    </td>
                </tr>
                <tr>
                    <td style="width:120px">Due Date:</td>
                    <td>
                        <asp:TextBox runat="server" ID="TextBoxDueDate"  />
                        <asp:CalendarExtender ID="CalendarExtenderDueDate" runat="server"
                                              TargetControlID="TextBoxDueDate" PopupButtonID="TextBoxDueDate" Format="yyyy-MM-dd">
                        </asp:CalendarExtender>
                    </td>
                </tr>
                <tr>
                    <td></td>
                    <td>
                        <asp:Button ID="btnOK" runat="server" CommandName="OK" Text="OK" OnClick="btnOK_Click"  />
                        <asp:Button ID="btnCancel" runat="server" Text="Cancel" OnClick="btnCancel_OnClick" />
                        <asp:HiddenField runat="server" ID="hfInvoiceId" />
                    </td>
                </tr>
            </table>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Panel>
</asp:Content>
