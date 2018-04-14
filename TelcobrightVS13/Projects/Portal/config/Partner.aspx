<%@ Page Title="" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="True" CodeBehind="Partner.aspx.cs" Inherits="ConfigPartner" %>

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
          

          <table style="width: 680px;">
            <tr>
            <td style="width: 460px; padding-left: 30px;"> <%--first column--%>
                
                <div style="text-align: right; float: left;">
                    <b> PartnerName:</b>    
                    <div style="height: 9px;"></div>
                    Telephone:
                    <div style="height: 12px;"></div>
                    Email:
                    <div style="height: 7px;"></div>
                    <b> Pre/Post Paid:</b>
                    <div style="height: 7px;"></div>
                    <b> PartnerType:</b>
                    <div style="height: 5px;"></div>
                </div>
                <div style="text-align: left; float: left;">




                    <asp:TextBox ID="PartnerNameTextBox" runat="server"
                        Text="" />
                    <div style="height: 5px;"></div>

                    <asp:TextBox ID="TelephoneTextBox" runat="server"
                        Text="" />
                    <div style="height: 5px;"></div>

                    <asp:TextBox ID="EmailTextBox" runat="server" Text="" />
                    <div style="height: 5px;"></div>
                    


                        <asp:DropDownList ID="ddlistPrePostAdd" runat="server" AutoPostBack="True"
                            DataSourceID="SqlDataPrePost" DataTextField="type" DataValueField="id"
                            Enabled="True" Width="173px">
                        </asp:DropDownList>
                    
                    <div style="height: 5px;"></div>
                    



                    <asp:DropDownList ID="ddlistCustomerTypeAdd" runat="server" AutoPostBack="True"
                        DataSourceID="SqlDataPartnerTypeEdit" DataTextField="type" DataValueField="id"
                        Enabled="True" Width="173px">
                    </asp:DropDownList>
                    <br />


                </div>

            </td>
            
            
            <td style="width: 300px;"> <%--Column 2--%>
                <div style="text-align:right;">
                Address1:
                <asp:TextBox ID="Address1TextBox" runat="server" 
                    Text="" />
                <br />
                Address2:
                <asp:TextBox ID="Address2TextBox" runat="server" 
                    Text="" />
                <br />
                City:
                <asp:TextBox ID="CityTextBox" runat="server" Text="" />
                <br />
                State:
                <asp:TextBox ID="StateTextBox" runat="server" Text="" />
                <br />
                PostalCode:
                <asp:TextBox ID="PostalCodeTextBox" runat="server" 
                    Text="" />
                <br />
                Country:
                <asp:TextBox ID="CountryTextBox" runat="server" Text="" />
                <br />
                
                </div>

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
                            <td width="30px" align="center">UOM</td>
                        </tr>
                    </table>
                </HeaderTemplate>
                <ItemTemplate>
                    <asp:Panel ID="pnlAccount" runat="server" >
                        <asp:GridView ID="gvAccount" runat="server" AutoGenerateColumns="false" ForeColor="#333333" GridLines="Vertical" Font-Size="9pt" 
                            ShowHeader="False" BorderStyle="Solid" BorderColor="Silver">
                            <Columns>
                                <asp:BoundField ItemStyle-Width="220px" DataField="accountName" HeaderText="Account Name" HeaderStyle-HorizontalAlign="Left" ItemStyle-HorizontalAlign="Left" />
                                <asp:BoundField ItemStyle-Width="80px" DataField="BalanceAfter" HeaderText="Balance" HeaderStyle-HorizontalAlign="Right" ItemStyle-HorizontalAlign="Right" />                                
                                <asp:BoundField ItemStyle-Width="30px" DataField="iduom" HeaderText="UOM" HeaderStyle-HorizontalAlign="Left" ItemStyle-HorizontalAlign="Left" />                                
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

