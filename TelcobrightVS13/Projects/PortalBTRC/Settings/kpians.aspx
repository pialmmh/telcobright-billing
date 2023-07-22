<%@ Page Title="" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="True" CodeBehind="kpians.aspx.cs" Inherits="ConfigKpians" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" Runat="Server">

    <div style="padding-left:20px;">

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

<div style="padding-left:0px;">
        <span style="color:Black;margin-right:20px;"><b>KPI for ANS Termination</b></span>
        

            <asp:DropDownList ID="ddlistPartnerType" runat="server" 
                DataSourceID="SqlDataPartnerType" DataTextField="Type" 
                DataValueField="id" AutoPostBack="True" Visible="False" 
                onselectedindexchanged="DropDownPartnerType_SelectedIndexChanged">
            </asp:DropDownList>

    </div>
    
    <asp:Label ID="lblIdPartnerGlobal" runat="server" Text="" Visible="false"></asp:Label>

    <asp:SqlDataSource ID="SqlDataPartnerType" runat="server" 
        ConnectionString="<%$ ConnectionStrings:Partner %>" 
        ProviderName="<%$ ConnectionStrings:Partner.ProviderName %>" SelectCommand="select (select -1) as id,(select ' [All]') as Type
union all					
select id,Type from enumpartnertype
order by type "></asp:SqlDataSource>
  <asp:GridView ID="GridViewPartner" runat="server" AllowPaging="True" 
        AllowSorting="True" AutoGenerateColumns="False" CellPadding="4" 
        DataKeyNames="idpartner" DataSourceID="EntityDataPartner" ForeColor="#333333" 
        GridLines="Vertical" Font-Size="9pt" PageSize="20" 
        onrowdatabound="GridViewPartner_RowDataBound" 
        onrowcommand="GridViewPartner_RowCommand" 
        onrowediting="GridViewPartner_RowEditing" 
        onrowupdating="GridViewPartner_RowUpdating" 
        onrowdeleting="GridViewPartner_RowDeleting">
        <AlternatingRowStyle BackColor="White" />
        <Columns>
            
             <asp:CommandField ShowEditButton="True" />
            <asp:BoundField DataField="idpartner" HeaderText="idpartner" ReadOnly="True" 
                SortExpression="idpartner" visible="false"/>

         <asp:TemplateField HeaderText="ANS Name" SortExpression="PartnerName">
                
                <ItemTemplate>
                    <asp:Label ID="Label1" runat="server" Text='<%# Eval("PartnerName") %>'></asp:Label>
                </ItemTemplate>
                
                <EditItemTemplate>
                    <asp:Label ID="Label1" runat="server" Text='<%# Eval("PartnerName") %>'></asp:Label>
                    
                </EditItemTemplate>
                
            </asp:TemplateField>
          

            <asp:BoundField DataField="refasr" HeaderText="Ref. ASR" SortExpression="refasr" visible="true" />
            <asp:BoundField DataField="refacd" HeaderText="Ref. ACD" SortExpression="refacd" visible="true" />
            <asp:BoundField DataField="refccr" HeaderText="Ref. CCR" SortExpression="refccr" visible="true" />
            <asp:BoundField DataField="refccrbycc" HeaderText="Ref. CCR (CC)" SortExpression="refccrbycc" visible="true" />
            <asp:BoundField DataField="refpdd" HeaderText="Ref. PDD" SortExpression="refpdd" visible="true" />
            <asp:BoundField DataField="refasrfas" HeaderText="Ref. ASR (False Answer Signal)" SortExpression="refasrfas" visible="true" />
        </Columns>

      
        <EditRowStyle BackColor="#7C6F57" />

      
        <FooterStyle BackColor="#1C5E55" Font-Bold="True" ForeColor="White" />
        <HeaderStyle BackColor="#1C5E55" Font-Bold="True" ForeColor="White" />
        <PagerStyle BackColor="#666666" ForeColor="White" 
            HorizontalAlign="Center" />
        <RowStyle BackColor="white" />
        <SelectedRowStyle BackColor="#C5BBAF" ForeColor="#333333" Font-Bold="True" />
        <SortedAscendingCellStyle BackColor="#F8FAFA" />
        <SortedAscendingHeaderStyle BackColor="#246B61" />
        <SortedDescendingCellStyle BackColor="#D4DFE1" />
        <SortedDescendingHeaderStyle BackColor="#15524A" />
    </asp:GridView>
    
    <br />
        
<asp:LinkButton ID="LinkButton1" runat="server" Text="Add New Route" Visible="false" 
        onclick="LinkButton1_Click" Font-Size="Smaller">Add New Partner</asp:LinkButton>

  <asp:SqlDataSource ID="SqlDataPrePost" runat="server" 
        ConnectionString="<%$ ConnectionStrings:Partner %>" 
        ProviderName="<%$ ConnectionStrings:Partner.ProviderName %>" 
        SelectCommand="select * from enumprepostpaid"></asp:SqlDataSource>     

    <asp:ValidationSummary ID="ValidatorSummary" runat="server"
    ValidationGroup="allcontrols" ForeColor="Red" />

    <asp:FormView ID="frmPartnerInsert" runat="server" DataKeyNames="idpartner" 
        DataSourceID="EntityDataPartner" Width="483px" DefaultMode="Insert" 
        Height="96px" CellPadding="4" Font-Size="Small" ForeColor="#333333" 
        Visible="False" oniteminserted="frmPartnerInsert_ItemInserted" 
        onitemcreated="frmPartnerInsert_ItemCreated">
       


        <FooterStyle BackColor="#990000" Font-Bold="True" ForeColor="White" />
        <HeaderStyle BackColor="#990000" Font-Bold="True" ForeColor="White" />
       


        <InsertItemTemplate>
          

          <table border="0" width="580px">
            <tr>
            <td width="160px"> <%--first column--%>
                
                <div style="text-align:right;">
                <b> PartnerName:</b>
                <asp:TextBox ID="PartnerNameTextBox" runat="server" 
                    Text='<%# Bind("PartnerName") %>' />
                <br />
                Telephone:
                <asp:TextBox ID="TelephoneTextBox" runat="server" 
                    Text='<%# Bind("Telephone") %>' />
                <br />
                <b> Email:</b>
                <asp:TextBox ID="EmailTextBox" runat="server" Text='<%# Bind("Email") %>' />
                <br />
                <b> Pre/Post Paid:</b>
                
                    <asp:DropDownList ID="ddlistPrePostAdd" runat="server" AutoPostBack="True" 
                        DataSourceID="SqlDataPrePost" DataTextField="type" DataValueField="id"
                        SelectedValue='<%# Bind("CustomerPrePaid") %>'
                        Enabled="True" Width="143px">
                        
                    </asp:DropDownList>
            
                <br />
                <b> PartnerType:</b>
                    
                    
                    <asp:DropDownList ID="ddlistCustomerTypeAdd" runat="server" AutoPostBack="True" 
                        DataSourceID="SqlDataPartnerTypeEdit" DataTextField="type" DataValueField="id"
                        SelectedValue='<%# Bind("PartnerType") %>'
                        Enabled="True" Width="143px">
                        
                    </asp:DropDownList>
                <br />
                Billing Day in Month:
                <asp:TextBox ID="BillingDateTextBox" runat="server" 
                    Text='<%# Bind("BillingDate") %>' />
                <br />
                    
            </div>

            </td>
            
            
            <td width="160px"> <%--Column 2--%>
                <div style="text-align:right;">
                Address1:
                <asp:TextBox ID="Address1TextBox" runat="server" 
                    Text='<%# Bind("Address1") %>' />
                <br />
                Address2:
                <asp:TextBox ID="Address2TextBox" runat="server" 
                    Text='<%# Bind("Address2") %>' />
                <br />
                City:
                <asp:TextBox ID="CityTextBox" runat="server" Text='<%# Bind("City") %>' />
                <br />
                State:
                <asp:TextBox ID="StateTextBox" runat="server" Text='<%# Bind("State") %>' />
                <br />
                PostalCode:
                <asp:TextBox ID="PostalCodeTextBox" runat="server" 
                    Text='<%# Bind("PostalCode") %>' />
                <br />
                Country:
                <asp:TextBox ID="CountryTextBox" runat="server" Text='<%# Bind("Country") %>' />
                <br />
                
                </div>

            </td>
            </tr>
            
           </table>
           
            
            <asp:LinkButton ID="InsertButton" runat="server" CausesValidation="True" 
                CommandName="Insert" Text="Insert" ValidationGroup="allcontrols"/>

&nbsp;<asp:LinkButton ID="InsertCancelButton" runat="server" CausesValidation="False" 
                CommandName="Cancel" Text="Cancel" OnClick="FormViewCancel_Click"/>
        </InsertItemTemplate>
       
        <PagerStyle BackColor="#FFCC66" ForeColor="#333333" HorizontalAlign="Center" />
        <RowStyle BackColor="#FFFBD6" ForeColor="#333333" />

        
       
    </asp:FormView>


    <asp:CustomValidator ID="cvAll" runat="server" 
    Display="Dynamic"
    ErrorMessage="This membership"
    OnServerValidate="cvAll_Validate" 
    ValidationGroup="allcontrols"
    Text=""></asp:CustomValidator>

   </div>

</asp:Content>

