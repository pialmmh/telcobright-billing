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
                <asp:GridView ID="gvInvoice" runat="server" AutoGenerateColumns="False" DataKeyNames="INVOICE_ID"
                    CellPadding="4" ForeColor="#333333" GridLines="Vertical"
                    Font-Size="9pt" BorderColor="Silver" BorderStyle="Solid" OnRowDataBound="gvInvoice_RowDataBound">
                    <AlternatingRowStyle BackColor="#f2f2f2" ForeColor="#284775"></AlternatingRowStyle>
                    <HeaderStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
                    <PagerStyle BackColor="#284775" ForeColor="White" HorizontalAlign="Center" />
                    <RowStyle BackColor="white" ForeColor="#333333" />
                    <Columns>
                        <asp:BoundField DataField="DESCRIPTION" HeaderText="Description" />
                        <asp:BoundField DataField="REFERENCE_NUMBER" HeaderText="Reference Number" />
                        <asp:TemplateField HeaderText="Invoice Date" SortExpression="INVOICE_DATE" ItemStyle-Wrap="false">
                            <ItemTemplate>
                                <asp:TextBox runat="server" ID="txtInvoiceDate" Enabled="false" dataformatstring="{0:yyyy-MM-dd HH:mm:ss}" />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Due Date" SortExpression="DUE_DATE" ItemStyle-Wrap="false">
                            <ItemTemplate>
                                <asp:TextBox runat="server" ID="txtDueDate" Enabled="false" dataformatstring="{0:yyyy-MM-dd HH:mm:ss}" />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="" />
                    </Columns>
                </asp:GridView>
            </ContentTemplate>
            <Triggers>
                
            </Triggers>
        </asp:UpdatePanel>
    </div>
</asp:Content>
