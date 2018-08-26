<%@ Page Title="" Language="C#" MasterPageFile="~/Site.master" 
Theme="" AutoEventWireup="True" CodeBehind="ConversionRates.aspx.cs" Inherits="ConversionRates" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>



<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" Runat="Server">
<div>
    <ajaxToolkit:ToolkitScriptManager ID="ToolkitScriptManager1" runat="server" > </ajaxToolkit:ToolkitScriptManager>
    
</div>
    <div>
        <asp:Label ID="lblAssignedPlans" runat="server" Text="Conversion Rates" Font-Bold="true"></asp:Label>
    </div>
    <div style="clear: both; padding-top: 10px;">
        <span style="color: black;"><b>Add new conversion rate:</b></span> <br/>
        <table width="800px" cellpadding="2" cellspacing="2" border="0">
            <tr>
                <td style="width: 15%; horiz-align: left">
                    From Currency
                </td>
                <td style="width: 35%; horiz-align: left">
                    <asp:DropDownList ID="ddlistFromCurrency" runat="server"  Enabled="true" AutoPostBack="false" 
                        DataSourceID="SqlDataUOM" DataTextField="DESCRIPTION" DataValueField="UOM_ID" OnDataBound="ddlistFromCurrency_DataBound" />
                </td>
                <td style="width: 15%; horiz-align: left">
                    To Currency
                </td>
                <td style="width: 35%; horiz-align: left">
                    <asp:DropDownList ID="ddlistToCurrency" runat="server" Enabled="true" AutoPostBack="false" 
                        DataSourceID="SqlDataUOM" DataTextField="DESCRIPTION" DataValueField="UOM_ID" OnDataBound="ddlistToCurrency_DataBound" />
                </td>
            </tr>
            <tr>
                <td style="width: 15%; horiz-align: left">
                    From Date [Time]
                </td>
                <td style="width: 35%; horiz-align: left">
                    <asp:TextBox ID="txtDateFrom" runat="server" />
                    <ajaxToolkit:CalendarExtender ID="CalendarStartDate" runat="server"
                                                    TargetControlID="txtDateFrom" PopupButtonID="txtDateFrom" Format="yyyy-MM-dd 23:59:59">
                    </ajaxToolkit:CalendarExtender>
                </td>
                <td style="width: 15%; horiz-align: left">
                    Till Date [Time]
                </td>
                <td style="width: 35%; horiz-align: left">
                    <asp:TextBox ID="txtDateTo" runat="server" />
                    <ajaxToolkit:CalendarExtender ID="CalendarEndDate" runat="server"
                                                    TargetControlID="txtDateTo" PopupButtonID="txtDateTo" Format="yyyy-MM-dd 23:59:59">
                    </ajaxToolkit:CalendarExtender>                            
                </td>
            </tr>
            <tr>
                <td style="width: 15%; horiz-align: left">
                    Conversion Rate
                </td>
                <td style="width: 35%; horiz-align: left">
                    <asp:TextBox runat="server" ID="txtConversionRate" />
                </td>
                <td style="width: 15%; horiz-align: left">
                    Purpose
                </td>
                <td style="width: 35%; horiz-align: left">
                    <asp:DropDownList ID="ddlPurpose" runat="server" AutoPostBack="false" Enabled="True">
                        <asp:ListItem Text="External Conversion" Value="EXTERNAL_CONVERSION" Selected="True" />
                    </asp:DropDownList>
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
        EnableUpdate="True" EntitySetName="uom_conversion_dated" 
        AutoGenerateWhereClause="false" OnQueryCreated="EntityDataConversionRates_QueryCreated" />         
    <asp:SqlDataSource ID="SqlDataUOM" runat="server" 
        ConnectionString="<%$ ConnectionStrings:Partner %>" 
        ProviderName="<%$ ConnectionStrings:Partner.ProviderName %>" 
        SelectCommand="select UOM_ID, DESCRIPTION from uom where UOM_TYPE_ID = 'CURRENCY_MEASURE' order by DESCRIPTION" />

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
            <asp:TemplateField HeaderText="From Currency">
                <ItemTemplate>
                    <asp:DropDownList ID="DropDownListUOM_ID" runat="server" AutoPostBack="True" 
                        DataSourceID="SqlDataUOM" DataTextField="DESCRIPTION" DataValueField="UOM_ID" SelectedValue='<%# Bind("UOM_ID") %>'
                        Enabled="false" >
                    </asp:DropDownList>
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:DropDownList ID="DropDownListUOM_ID" runat="server" AutoPostBack="True" 
                        DataSourceID="SqlDataUOM" DataTextField="DESCRIPTION" DataValueField="UOM_ID" SelectedValue='<%# Bind("UOM_ID") %>'
                        Enabled="True" >
                    </asp:DropDownList>
                </EditItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="To Currency">
                <ItemTemplate>
                    <asp:DropDownList ID="DropDownListUOM_ID_TO" runat="server" AutoPostBack="True" 
                        DataSourceID="SqlDataUOM" DataTextField="DESCRIPTION" DataValueField="UOM_ID" SelectedValue='<%# Bind("UOM_ID_TO") %>'
                        Enabled="false" >
                    </asp:DropDownList>
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:DropDownList ID="DropDownListUOM_ID_TO" runat="server" AutoPostBack="True" 
                        DataSourceID="SqlDataUOM" DataTextField="DESCRIPTION" DataValueField="UOM_ID" SelectedValue='<%# Bind("UOM_ID_TO") %>'
                        Enabled="True" >
                    </asp:DropDownList>
                </EditItemTemplate>
            </asp:TemplateField>

                <asp:TemplateField HeaderText="Date From" SortExpression="FROM_DATE"  ItemStyle-Wrap="false">
                
                <ItemTemplate>
                    <asp:Label ID="lblStartDate" runat="server" Text='<%# 
                    Eval("FROM_DATE").ToString().Trim()!=""
                    ? Convert.ToDateTime(Eval("FROM_DATE")).ToString("yyyy-MM-dd HH:mm:ss")
                    :""
                    %>'></asp:Label>
                </ItemTemplate>
                    
                <EditItemTemplate>
                    Date:
                    <%--<asp:Calendar ID="CalendarStartDate" runat="server"></asp:Calendar>--%>
                    
                    <asp:TextBox ID="TextBoxStartDatePicker" runat="server" Enabled="true"
                        Text='<%# 
                    Eval("FROM_DATE").ToString().Trim()!=""
                    ? Convert.ToDateTime(Eval("FROM_DATE")).ToString("yyyy-MM-dd")
                    :""
                    %>'>>
                  </asp:TextBox>
                    <asp:CalendarExtender ID="CalendarStartDate" runat="server" 
                                    TargetControlID="TextBoxStartDatePicker"  PopupButtonID="TextBoxStartDatePicker" Format="yyyy-MM-dd">
                  </asp:CalendarExtender>
                  <br />
                  Time: <asp:TextBox ID="TextBoxStartDateTimePicker" runat="server" Enabled="true"
                      Text='<%# 
                    Eval("FROM_DATE").ToString().Trim()!=""
                    ? Convert.ToDateTime(Eval("FROM_DATE")).ToString("HH:mm:ss")
                    :""
                    %>'>>
                  </asp:TextBox>
                
                </EditItemTemplate>
                 

            </asp:TemplateField>


             <asp:TemplateField HeaderText="Date To" SortExpression="THRU_DATE"  ItemStyle-Wrap="false">
                
                <ItemTemplate>
                    <asp:Label ID="lblEndDate" runat="server" Text='<%# 
                    ( Eval("THRU_DATE")!=null)
                    ? Convert.ToDateTime(Eval("THRU_DATE")).ToString("yyyy-MM-dd HH:mm:ss")
                    :""
                    %>'></asp:Label>
                </ItemTemplate>
                    
                <EditItemTemplate>
                    Date:
                    <%--<asp:Calendar ID="CalendarStartDate" runat="server"></asp:Calendar>--%>
                    
                    <asp:TextBox ID="TextBoxEndDatePicker" runat="server" Enabled="true"
                        Text='<%# 
                    ( Eval("THRU_DATE")!=null)
                    ? Convert.ToDateTime(Eval("THRU_DATE")).ToString("yyyy-MM-dd")
                    :""
                    %>'>>
                  </asp:TextBox>
                    <asp:CalendarExtender ID="CalendarEndDate" runat="server" 
                                    TargetControlID="TextBoxEndDatePicker"  PopupButtonID="TextBoxEndDatePicker" Format="yyyy-MM-dd">
                  </asp:CalendarExtender>
                  <br />
                  Time: <asp:TextBox ID="TextBoxEndDateTimePicker" runat="server" Enabled="true"
                      Text='<%# 
                    ( Eval("THRU_DATE")!=null)
                    ? Convert.ToDateTime(Eval("THRU_DATE")).ToString("HH:mm:ss")
                    :""
                    %>'>>
                  </asp:TextBox>
                
                </EditItemTemplate>
        </asp:TemplateField>
            <asp:BoundField DataField="CONVERSION_FACTOR" HeaderText="Conversion Rate" SortExpression="CONVERSION_FACTOR" Visible="true" />
            <asp:BoundField DataField="PURPOSE_ENUM_ID" HeaderText="Purpose" SortExpression="PURPOSE_ENUM_ID" Visible="true" />
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