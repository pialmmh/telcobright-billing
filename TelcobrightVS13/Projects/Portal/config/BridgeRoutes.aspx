<%@ Page Title="" Language="C#" MasterPageFile="~/Site.master" 
Theme="" AutoEventWireup="True" CodeBehind="BridgeRoutes.aspx.cs" Inherits="BridgeRoutes" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>



<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" Runat="Server">
<div>
    <ajaxToolkit:ToolkitScriptManager ID="ToolkitScriptManager1" runat="server" > </ajaxToolkit:ToolkitScriptManager>
    
</div>
    <div>
        <asp:Label ID="lblAssignedPlans" runat="server" Text="Bridge Routes" Font-Bold="true"></asp:Label>
    </div>
    <div style="clear: both; padding-top: 10px;">
        <span style="color: black;"><b>Add new bridge route:</b></span> <br/>
        <table width="800px" cellpadding="2" cellspacing="2" border="0">
            <tr>
                <td style="width: 15%; horiz-align: left">
                    TG Name
                </td>
                <td style="width: 35%; horiz-align: left">
                    <asp:TextBox runat="server" ID="txtTGName" />
                </td>
                <td style="width: 15%; horiz-align: left">&nbsp;</td>
                <td style="width: 35%; horiz-align: left">&nbsp;</td>
            </tr>
            <tr>
                <td style="width: 15%; horiz-align: left">
                    Carrier 1
                </td>
                <td style="width: 35%; horiz-align: left">
                    <asp:DropDownList ID="ddlistIncomingPartner" runat="server"  Enabled="true" AutoPostBack="false" 
                        DataSourceID="SqlDataUOM" DataTextField="PartnerName" DataValueField="idPartner" />
                </td>
                <td style="width: 15%; horiz-align: left">
                    Carrier 2
                </td>
                <td style="width: 35%; horiz-align: left">
                    <asp:DropDownList ID="ddlistOutgoingPartner" runat="server" Enabled="true" AutoPostBack="false" 
                        DataSourceID="SqlDataUOM" DataTextField="PartnerName" DataValueField="idPartner" />
                </td>
            </tr>
            <tr>
                <td style="width: 15%; horiz-align: left">&nbsp;</td>
                <td style="width: 85%; horiz-align: left" colspan="3">
                    <asp:Button runat="server" ID="btnAdd" Text="Add" OnClick="btnAdd_Click" />
                </td>
            </tr>
        </table>
    </div>    
    <div>
    <asp:EntityDataSource ID="EntityDataConversionRates" runat="server" 
        ConnectionString="name=PartnerEntities" DefaultContainerName="PartnerEntities" 
        EnableDelete="True" EnableFlattening="False" EnableInsert="True" 
        EnableUpdate="True" EntitySetName="bridgedroutes" 
        AutoGenerateWhereClause="false" OnQueryCreated="EntityDataConversionRates_QueryCreated" />         
    <asp:SqlDataSource ID="SqlDataUOM" runat="server" 
        ConnectionString="<%$ ConnectionStrings:Partner %>" 
        ProviderName="<%$ ConnectionStrings:Partner.ProviderName %>" 
        SelectCommand="select idPartner, PartnerName from partner order by PartnerName" />

    <asp:GridView ID="GridViewConversionRates" runat="server" DataSourceID="EntityDataConversionRates"
              AutoGenerateColumns="False" DataKeyNames="id" CellPadding="4" ForeColor="#333333" 
              GridLines="Vertical"
              style="margin-left: 0px" 
              font-size="9pt" OnRowDataBound="GridViewConversionRates_RowDataBound" >
        <AlternatingRowStyle BackColor="#f2f2f2" ForeColor="#284775" />
        <Columns>
             <asp:TemplateField>
                <ItemTemplate>
                    <asp:LinkButton ID="LinkButtonEdit" CommandName="Edit"  runat="server">Edit</asp:LinkButton>
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:LinkButton ID="LinkButtonEdit"  CommandName="Update"  runat="server" CausesValidation="false" ValidationGroup="allcontrols">Update</asp:LinkButton>
                    <asp:LinkButton ID="LinkButtonCancel"  CommandName="Cancel"  runat="server">Cancel</asp:LinkButton>
                </EditItemTemplate>
            </asp:TemplateField>

            <asp:TemplateField>
                <ItemTemplate>
                    <asp:LinkButton ID="LinkButtonDelete" 
                    OnClientClick='return confirm("Are you sure you want to delete this entry?");'
                    CommandName="Delete"  runat="server" >Delete</asp:LinkButton>
                    
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:LinkButton ID="LinkButtonDelete" Visible="false"  CommandName="myDelete"  runat="server">Delete</asp:LinkButton>
                </EditItemTemplate>
            </asp:TemplateField>
            <asp:BoundField DataField="tgName" HeaderText="TG Name" SortExpression="tgName" Visible="true" />
            <asp:TemplateField HeaderText="Carrier 1">
                <ItemTemplate>
                    <asp:DropDownList ID="DropDownListIncomingPartner" runat="server" AutoPostBack="True" 
                        DataSourceID="SqlDataUOM" DataTextField="PartnerName" DataValueField="idPartner" SelectedValue='<%# Bind("inPartner") %>'
                        Enabled="false" >
                    </asp:DropDownList>
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:DropDownList ID="DropDownListIncomingPartner" runat="server" AutoPostBack="True" 
                        DataSourceID="SqlDataUOM" DataTextField="PartnerName" DataValueField="idPartner" SelectedValue='<%# Bind("inPartner") %>'
                        Enabled="True" >
                    </asp:DropDownList>
                </EditItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Carrier 2">
                <ItemTemplate>
                    <asp:DropDownList ID="DropDownListOutgoingPartner" runat="server" AutoPostBack="True" 
                        DataSourceID="SqlDataUOM" DataTextField="PartnerName" DataValueField="idPartner" SelectedValue='<%# Bind("outPartner") %>'
                        Enabled="false" >
                    </asp:DropDownList>
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:DropDownList ID="DropDownListOutgoingPartner" runat="server" AutoPostBack="True" 
                        DataSourceID="SqlDataUOM" DataTextField="PartnerName" DataValueField="idPartner" SelectedValue='<%# Bind("outPartner") %>'
                        Enabled="True" >
                    </asp:DropDownList>
                </EditItemTemplate>
            </asp:TemplateField>
        </Columns>
    <EditRowStyle BackColor="#999999" />
    <FooterStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
    <HeaderStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
    <PagerStyle BackColor="#284775" ForeColor="White" 
                HorizontalAlign="Center" />
    <RowStyle BackColor="white" Width="5px" ForeColor="#333333" />
    <AlternatingRowStyle Width="5px" />
    <SelectedRowStyle BackColor="#E2DED6" ForeColor="#333333" Font-Bold="True" />
    <SortedAscendingCellStyle BackColor="#E9E7E2" />
    <SortedAscendingHeaderStyle BackColor="#506C8C" />
    <SortedDescendingCellStyle BackColor="#FFFDF8" />
    <SortedDescendingHeaderStyle BackColor="#6F8DAE" />
    </asp:GridView>
    </div>
</asp:Content>