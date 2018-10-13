<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="BalanceAdjustment.aspx.cs" Inherits="PortalApp.config.BalanceAdjustment" MasterPageFile="~/Site.master" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>
<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">
   
    <script src="../Scripts/moment.js" type="text/javascript"></script>

    <%--Page Load and Other Server Side Asp.net scripts--%>
       
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" Runat="Server">
    <ajaxToolkit:ToolkitScriptManager ID="ToolkitScriptManager1" runat="server" />
    <div>
        <span style="color: black;"><b>Balance Adjustment</b></span><br/>
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
                        <asp:Button runat="server" ID="btnShow" Text="Show" OnClick="btnShow_OnClick"/>
                    </div>
                </fieldset>
            </ContentTemplate>
        </asp:UpdatePanel>
        <asp:UpdatePanel runat="server">
            <ContentTemplate>
                <div>
                    <asp:GridView ID="gvInvoice" runat="server" AutoGenerateColumns="False" DataKeyNames="RowId"
                                  CellPadding="4" ForeColor="#333333" GridLines="Vertical"
                                  Font-Size="9pt" BorderColor="Silver" BorderStyle="Solid"
                                  OnRowDataBound="gvInvoice_OnRowDataBound">
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
                            <asp:boundfield datafield="ServiceAccountAlias" headertext="Service Account"/>
                            <asp:TemplateField HeaderText="Balance On" SortExpression="StartDateTime" ItemStyle-Wrap="false">
                                <ItemTemplate >
                                    <asp:TextBox ID="txtStartDate" runat="server" AutoPostBack="true" Enabled="True" Width="140px"
                                                 dataformatstring="{0:yyyy-MM-dd HH:mm:ss}" OnTextChanged="txtStartDate_TextChanged"/>
                                    <asp:CalendarExtender ID="CalendarStartDate" runat="server" 
                                                          TargetControlID="txtStartDate"  PopupButtonID="txtStartDate" Format="yyyy-MM-dd 00:00:00" />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Balance" SortExpression="CurrentBalance" ItemStyle-Wrap="false">
                                <ItemTemplate>
                                    <asp:TextBox runat="server" ID="txtCurrentBalance" Width="80px" Enabled="True" AutoPostBack="True" 
                                        dataformatstring="{0:n4}" OnTextChanged="txtCurrentBalance_OnTextChanged" />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="Currency" HeaderText="Currency"/>
                        </Columns>
                    </asp:GridView>
                    
                </div>

                <asp:SqlDataSource ID="SqlDataUOM" runat="server" 
                                   ConnectionString="<%$ ConnectionStrings:Partner %>" 
                                   ProviderName="<%$ ConnectionStrings:Partner.ProviderName %>" 
                                   SelectCommand="select UOM_ID, DESCRIPTION from uom where UOM_TYPE_ID = 'CURRENCY_MEASURE' order by DESCRIPTION" />
            </ContentTemplate>
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="btnShow" EventName="Click"/>
            </Triggers>
        </asp:UpdatePanel>
    </div>
    <br/>
    <div style="float: left">
        <asp:Button runat="server" ID="btnBalanceAdjustment" OnClick="btnBalanceAdjustment_OnClick" Text="Create Balance Adjustment Jobs"/>
    </div>
    <div style="float: left; padding-left: 10px;">
        <asp:Label ID="lblStatus" runat="server" Text=""></asp:Label>    
    </div>
    <br/>
    
</asp:Content>
