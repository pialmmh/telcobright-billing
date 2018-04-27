<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="InvoiceGeneration.aspx.cs" Inherits="PortalApp.config.InvoiceGeneration" MasterPageFile="~/Site.master" %>
<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">
   
    <script src="../Scripts/moment.js" type="text/javascript"></script>

    <%--Page Load and Other Server Side Asp.net scripts--%>
       


</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" Runat="Server">
    <asp:SqlDataSource ID="SqlDataTimeZone" runat="server" 
                       ConnectionString="<%$ ConnectionStrings:Partner %>" 
                       ProviderName="<%$ ConnectionStrings:Partner.ProviderName %>" 
                       SelectCommand="select t.id,concat(c.country_name,' ',t.offsetdesc,' [',z.zone_name,']') as Name
                                        from timezone t join zone z using(zone_id) join country c using(country_code)
                                        order by c.country_name">
    </asp:SqlDataSource>
    <ajaxToolkit:ToolkitScriptManager ID="ToolkitScriptManager1" runat="server" />
    <div>
        <span style="color: black;"><b>Due Invoices to be generated:</b></span>
        <asp:GridView ID="gvInvoice" runat="server" AutoGenerateColumns="False" DataKeyNames="PartnerId"
                      CellPadding="4" ForeColor="#333333" GridLines="Vertical"
                      Font-Size="9pt" BorderColor="Silver" BorderStyle="Solid"
                      OnRowEditing="gvInvoice_OnRowEditing" OnRowDataBound="gvInvoice_OnRowDataBound"
            
            >
            <AlternatingRowStyle BackColor="#f2f2f2" ForeColor="#284775"></AlternatingRowStyle>
            <HeaderStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
            <PagerStyle BackColor="#284775" ForeColor="White" HorizontalAlign="Center" />
            <RowStyle BackColor="white" ForeColor="#333333" />
            <Columns>
                <asp:TemplateField>
                    <ItemTemplate>
                        <asp:CheckBox runat="server" id="cbSelect" Checked="False"/>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:boundfield datafield="PartnerName" headertext="Partner"/>
                <asp:boundfield datafield="ServiceAccount" headertext="Service Account"/>
                <asp:TemplateField HeaderText="Time Zone" SortExpression="TimeZone" ItemStyle-Wrap="false">
                    <ItemTemplate>
                        <asp:Label ID="lblTimeZone" runat="server" Text=""></asp:Label>
                    </ItemTemplate>
                    <EditItemTemplate>
                        <asp:DropDownList ID="ddlistTimeZone" runat="server" AutoPostBack="False" 
                                          DataTextField="Name" DataValueField="id"
                                          Enabled="False" DataSourceID="SqlDataTimeZone"
                                          SelectedValue='<%# Bind("TimeZone") %>' />
                    </EditItemTemplate>
                </asp:TemplateField>
                <asp:boundfield datafield="StartDateWithTime" headertext="From" dataformatstring="{0:yyyy-MM-dd HH:mm:ss}" />
                <asp:boundfield datafield="EndDateWithTime" headertext="Till" dataformatstring="{0:yyyy-MM-dd HH:mm:ss}" />
                <asp:boundfield datafield="Amount" headertext="Amount" DataFormatString="{0:n2}">
                    <ItemStyle HorizontalAlign="Right" />
                </asp:boundfield>
            </Columns>
        </asp:GridView>
    </div>
    <br/>
    <div>
        <span style="color: black;"><b>Add row for custom invoice generation:</b></span>
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
                            <asp:DropDownList ID="ddlistTimeZone" runat="server" AutoPostBack="false" DataSourceID="SqlDataTimeZone" 
                                              DataTextField="Name" DataValueField="id"
                                              Enabled="True"/>
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
