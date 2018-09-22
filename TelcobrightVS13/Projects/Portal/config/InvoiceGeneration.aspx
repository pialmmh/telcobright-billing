<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="InvoiceGeneration.aspx.cs" Inherits="PortalApp.config.InvoiceGeneration" MasterPageFile="~/Site.master" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>
<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">
   
    <script src="../Scripts/moment.js" type="text/javascript"></script>

    <%--Page Load and Other Server Side Asp.net scripts--%>
       
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" Runat="Server">
    <ajaxToolkit:ToolkitScriptManager ID="ToolkitScriptManager1" runat="server" />
    <div>
        <span style="color: black;"><b>Due Invoices to be generated:</b></span><br/>
        <asp:UpdatePanel runat="server">
            <ContentTemplate>
                <asp:GridView ID="gvInvoice" runat="server" AutoGenerateColumns="False" DataKeyNames="PartnerId"
                    CellPadding="4" ForeColor="#333333" GridLines="Vertical"
                    Font-Size="9pt" BorderColor="Silver" BorderStyle="Solid"
                    OnRowDataBound="gvInvoice_OnRowDataBound">
                    <AlternatingRowStyle BackColor="#f2f2f2" ForeColor="#284775"></AlternatingRowStyle>
                    <HeaderStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
                    <PagerStyle BackColor="#284775" ForeColor="White" HorizontalAlign="Center" />
                    <RowStyle BackColor="white" ForeColor="#333333" />
                    <Columns>
                        <asp:boundfield datafield="PartnerName" headertext="Partner"/>
                        <asp:boundfield datafield="ServiceAccountAlias" headertext="Service Account"/>
                        <asp:BoundField DataField="Currency" HeaderText="Currency"/>
                        <asp:boundfield datafield="CurrentBalance" headertext="Balance" DataFormatString="{0:n2}">
                            <ItemStyle HorizontalAlign="Right" />
                        </asp:boundfield>
                        <asp:TemplateField HeaderText="Time Zone" SortExpression="TimeZone" ItemStyle-Wrap="false">
                            <ItemTemplate>
                                <asp:DropDownList ID="ddlistTimeZone" runat="server" AutoPostBack="true" Enabled="True" Width="280px"
                                    OnSelectedIndexChanged="ddlistTimeZone_SelectedIndexChanged"/>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <%--<asp:boundfield datafield="StartDateTime" headertext="From" dataformatstring="{0:yyyy-MM-dd HH:mm:ss}" />--%>
                        <asp:TemplateField HeaderText="Start Date Time" SortExpression="StartDateTime" ItemStyle-Wrap="false">
                            <ItemTemplate >
                                <asp:TextBox ID="txtStartDate" runat="server" AutoPostBack="true" Enabled="True" Width="140px"
                                    dataformatstring="{0:yyyy-MM-dd HH:mm:ss}" OnTextChanged="txtStartDate_TextChanged"/>
                                <asp:CalendarExtender ID="CalendarStartDate" runat="server" 
                                  TargetControlID="txtStartDate"  PopupButtonID="txtStartDate" Format="yyyy-MM-dd 00:00:00" />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <%--<asp:boundfield datafield="EndDateTime" headertext="Till" dataformatstring="{0:yyyy-MM-dd HH:mm:ss}" />--%>
                        <asp:TemplateField HeaderText="End Date Time" SortExpression="EndDateTime" ItemStyle-Wrap="false">
                            <ItemTemplate >
                                <asp:TextBox ID="txtEndDate" runat="server" AutoPostBack="true" Enabled="True" Width="140px"
                                    dataformatstring="{0:yyyy-MM-dd HH:mm:ss}" OnTextChanged="txtEndDate_TextChanged"/>
                                <asp:CalendarExtender ID="CalendarEndDate" runat="server" 
                                  TargetControlID="txtEndDate"  PopupButtonID="txtEndDate" Format="yyyy-MM-dd 23:59:59" />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:boundfield datafield="Amount" headertext="Amount" DataFormatString="{0:n2}">
                            <ItemStyle HorizontalAlign="Right" />
                        </asp:boundfield>
                        <asp:TemplateField HeaderText="Due" SortExpression="IsDue" ItemStyle-Wrap="false">
                            <ItemTemplate>
                                <asp:Label runat="server" ID="lblDue" Enabled="false" />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField>
                            <ItemTemplate>
                                <asp:CheckBox runat="server" id="cbSelect" Checked="False"/>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
                <asp:SqlDataSource ID="SqlDataUOM" runat="server" 
                                   ConnectionString="<%$ ConnectionStrings:Partner %>" 
                                   ProviderName="<%$ ConnectionStrings:Partner.ProviderName %>" 
                                   SelectCommand="select UOM_ID, DESCRIPTION from uom where UOM_TYPE_ID = 'CURRENCY_MEASURE' order by DESCRIPTION" />
            </ContentTemplate>
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="btnAddInvoiceRow" EventName="Click" />
            </Triggers>
        </asp:UpdatePanel>
    </div>
    <br/>
    <div style="float: left">
        <asp:Button runat="server" ID="btnGenerateInvoice" OnClick="btnGenerateInvoice_OnClick" Text="Create Invoice Generation Jobs"/>
    </div>
    <div style="float: left; padding-left: 10px;">
        <asp:Label ID="lblStatus" runat="server" Text=""></asp:Label>    
    </div>
    <br/>
    
    <div style="clear: both; padding-top: 10px;">
        <span style="color: black;"><b>Add invoice generation task for custom period:</b></span> <br/>
        <asp:UpdatePanel runat="server" ID="upCustomInvoice">
            <ContentTemplate>
                <table width="800px" cellpadding="2" cellspacing="2" border="0">
                    <tr>
                        <td style="width: 15%; horiz-align: left">
                            Partner
                        </td>
                        <td style="width: 35%; horiz-align: left">
                            <asp:DropDownList ID="ddlistPartner" runat="server" 
                                              Enabled="true" AutoPostBack="false" 
                                              DataTextField="PartnerName" 
                                              DataValueField="idPartner" />
                        </td>
                        <td style="width: 15%; horiz-align: left">
                            Service Account
                        </td>
                        <td style="width: 35%; horiz-align: left">
                            <asp:DropDownList ID="ddlistServiceAccount" runat="server" 
                                              Enabled="true" AutoPostBack="false" />
                        </td>
                    </tr>
                    <tr>
                        <td style="width: 15%; horiz-align: left">
                            Time Zone
                        </td>
                        <td style="width: 85%; horiz-align: left" colspan="3">
                            <asp:DropDownList ID="ddlistTimeZone" runat="server" AutoPostBack="false" Enabled="True"/>
                        </td>
                    </tr>
                    <tr>
                        <td style="width: 15%; horiz-align: left">
                            Start Date [Time]
                        </td>
                        <td style="width: 35%; horiz-align: left">
                            <asp:TextBox ID="txtDate" runat="server" />
                            <ajaxToolkit:CalendarExtender ID="CalendarStartDate" runat="server"
                                                          TargetControlID="txtDate" PopupButtonID="txtDate" Format="yyyy-MM-dd 00:00:00">
                            </ajaxToolkit:CalendarExtender>
                        </td>
                        <td style="width: 15%; horiz-align: left">
                            End Date [Time]
                        </td>
                        <td style="width: 35%; horiz-align: left">
                            <asp:TextBox ID="txtDate1" runat="server" />
                            <ajaxToolkit:CalendarExtender ID="CalendarEndDate" runat="server"
                                                          TargetControlID="txtDate1" PopupButtonID="txtDate1" Format="yyyy-MM-dd 23:59:59">
                            </ajaxToolkit:CalendarExtender>                            
                        </td>
                    </tr>
                    <tr>
                        <td style="width: 15%; horiz-align: left">&nbsp;</td>
                        <td style="width: 85%; horiz-align: left" colspan="3">
                            <asp:Button runat="server" ID="btnAddInvoiceRow" Text="Add Row" OnClick="btnAddInvoiceRow_OnClick"/>
                        </td>
                    </tr>
                </table>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>    
</asp:Content>
