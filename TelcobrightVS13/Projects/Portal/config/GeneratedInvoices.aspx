<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="GeneratedInvoices.aspx.cs" Inherits="PortalApp.config.GeneratedInvoices" MasterPageFile="~/Site.master" %>
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
                <asp:GridView ID="gvInvoice" runat="server" AutoGenerateColumns="False" DataKeyNames="PartnerId"
                    CellPadding="4" ForeColor="#333333" GridLines="Vertical"
                    Font-Size="9pt" BorderColor="Silver" BorderStyle="Solid"
                    OnRowEditing="gvInvoice_OnRowEditing" OnRowDataBound="gvInvoice_OnRowDataBound">
                    <AlternatingRowStyle BackColor="#f2f2f2" ForeColor="#284775"></AlternatingRowStyle>
                    <HeaderStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
                    <PagerStyle BackColor="#284775" ForeColor="White" HorizontalAlign="Center" />
                    <RowStyle BackColor="white" ForeColor="#333333" />
                    <Columns>
                        <asp:templatefield>
                            <itemtemplate>
                                <asp:linkbutton id="btnEdit" runat="server" commandname="Edit" text="Edit" />
                            </itemtemplate>
                            <edititemtemplate>
                                <asp:linkbutton id="btnUpdate" runat="server" commandname="Update" text="Update" />
                                <asp:linkbutton id="btnCancel" runat="server" commandname="Cancel" text="Cancel" />
                            </edititemtemplate>
                        </asp:templatefield>                        
                        <asp:boundfield datafield="PartnerName" headertext="Partner"/>
                        <asp:boundfield datafield="ServiceAccount" headertext="Service Account"/>
                        <asp:TemplateField HeaderText="Time Zone" SortExpression="TimeZone" ItemStyle-Wrap="false">
                            <ItemTemplate>
                                <asp:Label ID="lblTimeZone" runat="server" Text=""></asp:Label>
                            </ItemTemplate>
                            <EditItemTemplate>
                                <asp:DropDownList ID="ddlistTimeZone" runat="server" AutoPostBack="false" Enabled="True"/>
                            </EditItemTemplate>
                        </asp:TemplateField>
                        <asp:boundfield datafield="StartDateTime" headertext="From" dataformatstring="{0:yyyy-MM-dd HH:mm:ss}" />
                        <asp:boundfield datafield="EndDateTime" headertext="Till" dataformatstring="{0:yyyy-MM-dd HH:mm:ss}" />
                        <asp:boundfield datafield="Amount" headertext="Amount" DataFormatString="{0:n2}">
                            <ItemStyle HorizontalAlign="Right" />
                        </asp:boundfield>
                        <asp:TemplateField>
                            <ItemTemplate>
                                <asp:CheckBox runat="server" id="cbSelect" Checked="False"/>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </ContentTemplate>
            <Triggers>
                
            </Triggers>
        </asp:UpdatePanel>
    </div>
    <div>
        <asp:Label runat="server" Text="Report Template:"/>
        <asp:DropDownList runat="server" ID="DropDownListReportTemplate" />
        <asp:Button runat="server" ID="ButtonSaveReport" Text="Save Report" OnClick="ButtonSaveReport_Click" />
    </div>
</asp:Content>
