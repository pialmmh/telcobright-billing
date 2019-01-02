﻿<%@ Page Title="" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="True" CodeBehind="Partner.aspx.cs" Inherits="ConfigPartner" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" Runat="Server">

<div style="padding-left:0px;margin-top:-5px;">

 <div>
    

        <asp:EntityDataSource ID="EntityDataPartner" runat="server" 
            ConnectionString="name=PartnerEntities" DefaultContainerName="PartnerEntities" 
            EnableDelete="True" EnableFlattening="False" EnableInsert="True" 
            EnableUpdate="True" EntitySetName="partners" 
            AutoGenerateWhereClause="false" onquerycreated="EntityDataPartner_QueryCreated"  
            >
            
            
        </asp:EntityDataSource>
        <asp:SqlDataSource ID="SqlDataPartnerTypeEdit" runat="server" 
            ConnectionString="<%$ ConnectionStrings:Partner %>" 
            ProviderName="<%$ ConnectionStrings:Partner.ProviderName %>" 
            SelectCommand="select id,type from enumpartnertype"></asp:SqlDataSource>

    </div>

<div style="padding:2px; width:900px;background-color:#f2f2f2;margin-top:5px;">
        <span style="color:Black;margin-right:20px;"><b>Partners</b></span>
        Type: 

            <asp:DropDownList ID="ddlistPartnerType" runat="server" 
                DataSourceID="SqlDataPartnerType" DataTextField="Type" 
                DataValueField="id" AutoPostBack="false" 
                onselectedindexchanged="DropDownPartnerType_SelectedIndexChanged">
            </asp:DropDownList>
    
    Name:
    <asp:TextBox ID="TextBoxFind" runat="server"></asp:TextBox>
    <asp:Button ID="Button1" runat="server" Text="Find" onclick="Button1_Click" Width="100px" />

    <br />

    <asp:LinkButton ID="LinkButton1" runat="server" 
        onclick="LinkButton1_Click" Font-Size="Smaller">Add New Partner</asp:LinkButton>


    </div>
    
    <asp:Label ID="lblIdPartnerGlobal" runat="server" Text="" Visible="false"></asp:Label>


  <asp:SqlDataSource ID="SqlDataPrePost" runat="server" 
        ConnectionString="<%$ ConnectionStrings:Partner %>" 
        ProviderName="<%$ ConnectionStrings:Partner.ProviderName %>" 
        SelectCommand="select * from enumprepostpaid"></asp:SqlDataSource>     

        <span style="height:5px;"></span>

    <asp:ValidationSummary ID="ValidatorSummary" runat="server"
    ValidationGroup="allcontrols" ForeColor="Red" />

    <div style="height: 10px;"></div>

    <asp:FormView ID="frmPartnerInsert" runat="server" DataKeyNames="idpartner" 
        Width="483px" DefaultMode="Insert" 
        Height="96px" CellPadding="4" Font-Size="Small" ForeColor="#333333" 
        Visible="False" oniteminserted="frmPartnerInsert_ItemInserted" 
        onitemcreated="frmPartnerInsert_ItemCreated" BorderColor="#7C6F57" 
        BorderStyle="None" oniteminserting="frmPartnerInsert_ItemInserting" 
        onmodechanging="frmPartnerInsert_ModeChanging">
       


        <%--<EditRowStyle BackColor="#f2f2f2" />--%>
       <EditRowStyle BackColor="#F7F6F3" />
        


        <FooterStyle BackColor="#7C6F57" Font-Bold="false" ForeColor="White" />
        <HeaderStyle BackColor="#1C5E55" Font-Bold="True" ForeColor="White" />
       


        <InsertItemTemplate>
          

          <table style="width: 720px;">
            <tr>
            <td style="width: 460px; padding-left: 30px;" valign="top"> <%--first column--%>
                <table cellpadding="2" cellspacing="0" border="0">
                    <tr>
                        <td align="right"><b>PartnerName: </b></td>
                        <td align="left"><asp:TextBox ID="PartnerNameTextBox" runat="server" Text="" /></td>
                    </tr>
                    <tr>
                        <td align="right">Telephone: </td>
                        <td align="left"><asp:TextBox ID="TelephoneTextBox" runat="server" Text="" /></td>
                    </tr>
                    <tr>
                        <td align="right">Email: </td>
                        <td align="left"><asp:TextBox ID="EmailTextBox" runat="server" Text="" /></td>
                    </tr>
                    <tr>
                        <td align="right"><b>Pre/Post Paid: </b></td>
                        <td align="left"><asp:DropDownList ID="ddlistPrePostAdd" runat="server" AutoPostBack="True"
                                                           DataSourceID="SqlDataPrePost" DataTextField="type" DataValueField="id"
                                                           Enabled="True" Width="173px">
                        </asp:DropDownList></td>
                    </tr>
                    <tr>
                        <td align="right"><b>PartnerType: </b></td>
                        <td align="left"><asp:DropDownList ID="ddlistCustomerTypeAdd" runat="server" AutoPostBack="True"
                                                           DataSourceID="SqlDataPartnerTypeEdit" DataTextField="type" DataValueField="id"
                                                           Enabled="True" Width="173px">
                        </asp:DropDownList></td>
                    </tr>
                    <tr>
                        <td align="right"><b>Alternate Name (Invoice): </b></td>
                        <td align="left"><asp:TextBox ID="AlternateNameInvoiceTextBox" runat="server" Text="" /></td>
                    </tr>
                    <tr>
                        <td align="right">Alternate Name(Other): </td>
                        <td align="left"><asp:TextBox ID="AlternateNameOtherTextBox" runat="server" Text="" /></td>
                    </tr>
                    <tr>
                        <td align="right">VAT Registration No: </td>
                        <td align="left"><asp:TextBox ID="vatRegistrationNoTextBox" runat="server" Text="" /></td>
                    </tr>
                </table>
            </td>
            
            
            <td style="width: 260px; padding-left: 30px;" valign="top"> <%--Column 2--%>
                <table cellpadding="2" cellspacing="0" border="0">
                    <tr>
                        <td align="right">Address1: </td>
                        <td align="left"><asp:TextBox ID="Address1TextBox" runat="server" Text="" /></td>
                    </tr>
                    <tr>
                        <td align="right">Address2: </td>
                        <td align="left"><asp:TextBox ID="Address2TextBox" runat="server" Text="" /></td>
                    </tr>
                    <tr>
                        <td align="right">City: </td>
                        <td align="left"><asp:TextBox ID="CityTextBox" runat="server" Text="" /></td>
                    </tr>
                    <tr>
                        <td align="right">State: </td>
                        <td align="left"><asp:TextBox ID="StateTextBox" runat="server" Text="" /></td>
                    </tr>
                    <tr>
                        <td align="right">PostalCode: </td>
                        <td align="left"><asp:TextBox ID="PostalCodeTextBox" runat="server" Text="" /></td>
                    </tr>
                    <tr>
                        <td align="right">Country: </td>
                        <td align="left"><asp:TextBox ID="CountryTextBox" runat="server" Text="" /></td>
                    </tr>
                    <tr>
                        <td align="right"><b>InvoiceAddress: </b></td>
                        <td align="left"><asp:TextBox ID="InvoiceAddressTextBox" runat="server" Width="167px" TextMode="MultiLine" Text="" /></td>
                    </tr>
                </table>
            </td>
            </tr>
            
           </table>
       
      
        </InsertItemTemplate>
       
        <FooterTemplate>
         <asp:LinkButton ID="InsertButton" runat="server" CausesValidation="True" 
                CommandName="Insert" Text="Insert" ValidationGroup="allcontrols"/>

&nbsp;<asp:LinkButton ID="InsertCancelButton" runat="server" CausesValidation="False" 
                CommandName="Cancel" Text="Cancel" OnClick="FormViewCancel_Click"/>
       </FooterTemplate>

        <PagerStyle BackColor="#666666" ForeColor="White" HorizontalAlign="Center" />
        <RowStyle BackColor="white" />
        
        
       
    </asp:FormView>


    <asp:CustomValidator ID="cvAll" runat="server" 
    Display="Dynamic"
    ErrorMessage="This membership"
    OnServerValidate="cvAll_Validate" 
    ValidationGroup="allcontrols"
    Text=""></asp:CustomValidator>

   </div>


    <asp:SqlDataSource ID="SqlDataPartnerType" runat="server" 
        ConnectionString="<%$ ConnectionStrings:Partner %>" 
        ProviderName="<%$ ConnectionStrings:Partner.ProviderName %>" SelectCommand="select (select -1) as id,(select ' [All]') as Type
union all					
select id,Type from enumpartnertype
order by type "></asp:SqlDataSource>

<div style="margin-left:0px;margin-top:5px;">

    <asp:GridView ID="GridViewPartner" runat="server" AllowPaging="True" 
        AllowSorting="True" AutoGenerateColumns="False" CellPadding="4" 
        DataKeyNames="idPartner" DataSourceID="EntityDataPartner" ForeColor="#333333" 
        GridLines="Vertical" Font-Size="9pt" PageSize="49" 
        onrowdatabound="GridViewPartner_RowDataBound" 
        onrowcommand="GridViewPartner_RowCommand" 
        onrowediting="GridViewPartner_RowEditing" 
        onrowupdating="GridViewPartner_RowUpdating" 
        onrowdeleting="GridViewPartner_RowDeleting" BorderStyle="Solid" BorderColor="Silver">
        <AlternatingRowStyle BackColor="#f2f2f2" ForeColor="#284775" />
        <Columns>
            <%--onClientClick="window.open('PartnerDetail.aspx?idpartner=3')"--%>
             <asp:TemplateField>
                <ItemTemplate>
                   
                   
                    <asp:LinkButton ID="LinkButtonPartnerDetail" runat="server">Edit</asp:LinkButton>
                    
                </ItemTemplate>
            </asp:TemplateField>

            <asp:TemplateField>
                <ItemTemplate>
                    
                    <asp:LinkButton ID="LinkButton2"  runat="server" ButtonType="Button" CommandName="Delete"
                        Text="Delete" HeaderText="Delete" CausesValidation="false" ValidationGroup="allcontrols"
                    OnClientClick='return confirm("Are you sure you want to delete this entry?");'> 
                    </asp:LinkButton>
                    <asp:Label ID="lblIdPartner" runat="server" Text='<%# Eval("idpartner") %>'  Visible="false"></asp:Label>
                </ItemTemplate>
            </asp:TemplateField>

            
            <%--<asp:CommandField ShowDeleteButton="false" />--%>
            <asp:BoundField DataField="idPartner" HeaderText="idpartner" ReadOnly="True" 
                SortExpression="idPartner" visible="True"/>
            <asp:BoundField DataField="PartnerName" HeaderText="PartnerName" 
                SortExpression="PartnerName" />
            <asp:BoundField DataField="Address1" HeaderText="Address1" 
                SortExpression="Address1" visible="false" />
            <asp:BoundField DataField="Address2" HeaderText="Address2" 
                SortExpression="Address2" visible="false" />
            <asp:BoundField DataField="City" HeaderText="City" SortExpression="City" visible="false" />
            <asp:BoundField DataField="State" HeaderText="State" SortExpression="State" visible="false" />
            <asp:BoundField DataField="PostalCode" HeaderText="PostalCode" visible="false" 
                SortExpression="PostalCode" />
            <asp:BoundField DataField="Country" HeaderText="Country" visible="false" 
                SortExpression="Country" />
            <asp:BoundField DataField="Telephone" HeaderText="Telephone" visible="false" 
                SortExpression="Telephone" />
            <asp:BoundField DataField="Email" HeaderText="Email" SortExpression="Email" visible="false" />
            <asp:BoundField DataField="CustomerPrePaid" HeaderText="Pre/Post Paid" 
                SortExpression="CustomerPrePaid" Visible="false" />

            

            <asp:BoundField DataField="SupplierPrePaid" HeaderText="SupplierPrePaid" 
                SortExpression="SupplierPrePaid" visible="false" />
            
            <asp:TemplateField HeaderText="PartnerType" SortExpression="PartnerType">
                
                <ItemTemplate>
                    <asp:DropDownList ID="DropDownList2" runat="server" AutoPostBack="True" 
                        DataSourceID="SqlDataPartnerTypeEdit" DataTextField="Type" DataValueField="id" SelectedValue='<%# Bind("PartnerType") %>'
                        Enabled="false" >
                    </asp:DropDownList>
                    
                </ItemTemplate>
                
                <EditItemTemplate>
                    <asp:DropDownList ID="DropDownList2" runat="server" AutoPostBack="True" 
                        DataSourceID="SqlDataPartnerTypeEdit" DataTextField="Type" DataValueField="id" SelectedValue='<%# Bind("PartnerType") %>'
                        Enabled="True" >
                    </asp:DropDownList>
                    
                </EditItemTemplate>
                
            </asp:TemplateField>

           


            <asp:BoundField DataField="BillingDate" HeaderText="Billing Day of Month" 
                SortExpression="BillingDate" Visible="false" />

            <asp:BoundField DataField="AllowedDaysForInvoicePayment" 
                HeaderText="AllowedDaysForInvoicePayment" 
                SortExpression="AllowedDaysForInvoicePayment" visible="false" />
            <asp:BoundField DataField="TimeZone" HeaderText="TimeZone" 
                SortExpression="TimeZone" visible="false" />
            <asp:BoundField DataField="date1" HeaderText="date1" SortExpression="date1" visible="false" />
            <asp:BoundField DataField="field1" HeaderText="field1" visible="false" 
                SortExpression="field1" />
            <asp:BoundField DataField="field2" HeaderText="field2" visible="false" 
                SortExpression="field2" />
            <asp:BoundField DataField="field3" HeaderText="field3" visible="false" 
                SortExpression="field3" />
            <asp:BoundField DataField="field4" HeaderText="field4" visible="false" 
                SortExpression="field4" />
            <asp:BoundField DataField="field5" HeaderText="field5" visible="false" 
                SortExpression="field5" />

            <asp:BoundField DataField="refasr" HeaderText="Ref. ASR" SortExpression="refasr" visible="false" />
            <asp:BoundField DataField="refacd" HeaderText="Ref. ACD" SortExpression="refacd" visible="false" />
            <asp:BoundField DataField="refccr" HeaderText="Ref. CCR" SortExpression="refccr" visible="false" />
            <asp:BoundField DataField="refccrbycc" HeaderText="Ref. CCR (CC)" SortExpression="refccrbycc" visible="false" />
            <asp:TemplateField>
                <HeaderTemplate>
                    <table border="0" cellpadding="0" cellspacing="0" width="100%">
                        <tr>
                            <td width="220px" align="left">Account Name</td>
                            <td width="80px" align="center">Amount</td>
                            <td width="30px" align="right">UOM</td>
                        </tr>
                    </table>
                </HeaderTemplate>
                <ItemTemplate>
                    <asp:Panel ID="pnlAccount" runat="server" >
                        <asp:GridView ID="gvAccount" runat="server" AutoGenerateColumns="false" ForeColor="#333333" GridLines="Vertical" Font-Size="9pt" 
                            ShowHeader="False" BorderStyle="Solid" BorderColor="Silver">
                            <Columns>
                                <asp:BoundField ItemStyle-Width="220px" DataField="accountName" HeaderText="Service Account" HeaderStyle-HorizontalAlign="Left" ItemStyle-HorizontalAlign="Left" />
                                <asp:BoundField ItemStyle-Width="80px" DataField="BalanceAfter" DataFormatString="{0:n2}" HeaderText="Balance" HeaderStyle-HorizontalAlign="Right" ItemStyle-HorizontalAlign="Right" />                                
                                <asp:BoundField ItemStyle-Width="30px" DataField="uom" HeaderText="UOM" HeaderStyle-HorizontalAlign="Right" ItemStyle-HorizontalAlign="Right" />                                
                            </Columns>
                            <HeaderStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
                            <PagerStyle BackColor="#284775" ForeColor="White" 
                                        HorizontalAlign="Center" />
                            <RowStyle BackColor="white" Height="10" ForeColor="#333333" />
                        </asp:GridView>
                    </asp:Panel>
                </ItemTemplate>
            </asp:TemplateField>


        </Columns>

      
        <EditRowStyle BackColor="#999999" />

      
        <FooterStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
        <HeaderStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
        <PagerStyle BackColor="#284775" ForeColor="White" 
            HorizontalAlign="Center" />
        <RowStyle BackColor="white" Height="10" ForeColor="#333333" />
        <SelectedRowStyle BackColor="#E2DED6" ForeColor="#333333" Font-Bold="True" />
        <SortedAscendingCellStyle BackColor="#E9E7E2" />
        <SortedAscendingHeaderStyle BackColor="#506C8C" />
        <SortedDescendingCellStyle BackColor="#FFFDF8" />
        <SortedDescendingHeaderStyle BackColor="#6F8DAE" />
    </asp:GridView>
    
    </div>
    <br />
        


</asp:Content>

