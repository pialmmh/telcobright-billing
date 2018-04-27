<%@ Page Title="" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="True" CodeBehind="PartnerDetail.aspx.cs" Inherits="ConfigPartnerDetail" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" Runat="Server">

    <div style="margin-left:5px; min-width:600px;min-height:600px;">

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
    
<span style="color:Black;"><b> Partner Detail: </b></span>
<asp:Label ID="lblPartnerDetail" Font-Bold="true"  runat="server"></asp:Label>
  <asp:SqlDataSource ID="SqlDataPrePost" runat="server" 
        ConnectionString="<%$ ConnectionStrings:Partner %>" 
        ProviderName="<%$ ConnectionStrings:Partner.ProviderName %>" 
        SelectCommand="select * from enumprepostpaid"></asp:SqlDataSource>



    <asp:SqlDataSource ID="SqlDataRouteProtocol" runat="server" 
        ConnectionString="<%$ ConnectionStrings:Partner %>" 
        ProviderName="<%$ ConnectionStrings:Partner.ProviderName %>" 
        SelectCommand="select (select null) as id,(select 'Unknown') as type union all select id,type from enumsignalingprotocol"></asp:SqlDataSource>

    <asp:FormView ID="frmPartnerEdit" runat="server" DataKeyNames="idPartner" 
        DataSourceID="EntityDataPartner" Width="925px" DefaultMode="Edit" 
        Height="96px" CellPadding="4" Font-Size="Small" ForeColor="#333333" 
        BorderStyle="None">
       


        <EditRowStyle BackColor="#f2f2f2" />
       


        <FooterStyle BackColor="#1C5E55" Font-Bold="True" ForeColor="White" />
        <HeaderStyle BackColor="#1C5E55" Font-Bold="True" ForeColor="White" />
       


        <EditItemTemplate>
          

          <table border="0" style="min-width:925px">
            <tr>
            <td style="min-width:260px"> <%--first column--%>
                
                <div style="text-align:right;width:200px;float:left;">
                <b> PartnerName:</b>
                <div style="height:3px;"></div>
                
                Telephone:
                
                <div style="height:4px;"></div>
                <b> Email:</b>
                
                <div style="height:5px;"></div>
                
                <b> Pre/Post Paid:</b>
                
                <div style="height:3px;"></div>
                
                <b> PartnerType:</b>
                
                <div style="height:3px;"></div>
                Billing Day in Month:
                
                <br />
                    
            </div>
                <%--values--%>
                <div style="text-align:left;width:200px;float:left;padding-left:5px;">
                <asp:TextBox ID="TextBox4" runat="server" Width="170px"
                    Text='<%# Bind("PartnerName") %>' />
                <br />
                <asp:TextBox ID="TextBox5" runat="server"  Width="170px"
                    Text='<%# Bind("Telephone") %>' />
                <br />
                
                <asp:TextBox ID="TextBox6" runat="server"  Width="170px" Text='<%# Bind("email") %>' />
                <br />
                
                    <asp:DropDownList ID="DropDownList1" runat="server" AutoPostBack="True" 
                        DataSourceID="SqlDataPrePost" DataTextField="type" DataValueField="id"
                        SelectedValue='<%# Bind("CustomerPrePaid") %>'
                        Enabled="false" Width="173px">
                        
                    </asp:DropDownList>
                
                <br />
                
                <asp:DropDownList ID="DropDownList2" runat="server" AutoPostBack="True" 
                    DataSourceID="SqlDataPartnerTypeEdit" DataTextField="type" DataValueField="id"
                    SelectedValue='<%# Bind("PartnerType") %>'
                    Enabled="false" Width="173px">
                        
                </asp:DropDownList>
                
                <br />
                
                <asp:TextBox ID="TextBox7" runat="server"  Width="170px"
                    Text='<%# Bind("billingdate") %>' />
                <br />
                    
            </div>

            </td>
            
            
            <td style="min-width:260px;"> <%--Column 2--%>
                <div style="text-align:right;margin-right:200px;">
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
                CommandName="Update" Text="Update" />
&nbsp;<asp:LinkButton ID="InsertCancelButton" runat="server" CausesValidation="False" 
                CommandName="Cancel" Text="Cancel" />
        
        </EditItemTemplate>
       
        <PagerStyle BackColor="#666666" ForeColor="White" HorizontalAlign="Center" />
        <RowStyle BackColor="white" />
       
       
    </asp:FormView>
    <br />
    <asp:Label ID="lblRoute" runat="server" Text="Routes" ForeColor="Black" Font-Bold="true"></asp:Label>
    <br />
    <asp:Label ID="LabelUpdateValidate" Visible="false" ForeColor="Red" runat="server" Text=""></asp:Label>
    
    <asp:GridView ID="GridView1" runat="server" AllowSorting="True" 
        AutoGenerateColumns="False" CellPadding="4" DataKeyNames="idroute" 
        DataSourceID="EntityDataSource1" ForeColor="#333333" GridLines="Vertical" 
        Font-Size="9pt" BorderColor="Silver" onrowediting="GridView1_RowEditing" 
        onrowupdating="GridView1_RowUpdating"
        onrowcancelingedit="GridView1_RowCancelingEdit" 
        onrowupdated="GridView1_RowUpdated" 
        OnRowDataBound="GridView1_OnRowDataBound"
        BorderStyle="Solid">
        <AlternatingRowStyle BackColor="#f2f2f2" ForeColor="#284775" />
        <Columns>
            <asp:CommandField ShowDeleteButton="false" ShowEditButton="True" />
            <asp:TemplateField>
                <ItemTemplate>
                    
                    <asp:LinkButton ID="LinkButton2"  runat="server" ButtonType="Button" CommandName="Delete"
                        Text="Delete" HeaderText="Delete" CausesValidation="true" ForeColor="Black"
                    OnClientClick='return confirm("Are you sure you want to delete this entry?");'> 
                    </asp:LinkButton>
                </ItemTemplate>
            </asp:TemplateField>
            <asp:BoundField DataField="idroute" HeaderText="idroute" ReadOnly="True" 
                SortExpression="idroute" visible="false"/>
            <asp:BoundField DataField="RouteName" HeaderText="Route/TG Name" 
                SortExpression="RouteName" />
            <%--<asp:BoundField DataField="SwitchId" HeaderText="SwitchId" 
                SortExpression="SwitchId" />
--%>
            
            <asp:TemplateField HeaderText="Switch" SortExpression="SwitchId">
                <EditItemTemplate>
                    <asp:DropDownList ID="ddlistSwitch" runat="server" AutoPostBack="false" 
                        DataSourceID="EntityDataCustomerSwitch" DataTextField="SwitchName" DataValueField="idSwitch" SelectedValue='<%# Bind("SwitchId") %>'
                        Enabled="True" >
                    </asp:DropDownList>
                    
                </EditItemTemplate>
                <ItemTemplate>
                    <asp:DropDownList ID="ddlistSwitch" runat="server" AutoPostBack="false" 
                        DataSourceID="EntityDataCustomerSwitch" DataTextField="SwitchName" DataValueField="idSwitch" SelectedValue='<%# Bind("SwitchId") %>'
                        Enabled="false" >
                    </asp:DropDownList>
                </ItemTemplate>
            </asp:TemplateField>


            <asp:BoundField DataField="CommonRoute" HeaderText="CommonRoute" 
                SortExpression="CommonRoute" visible="false"/>
            <asp:BoundField DataField="idPartner" HeaderText="idPartner" 
                SortExpression="idPartner" visible="false"/>
            <asp:BoundField DataField="NationalOrInternational" 
                HeaderText="NationalOrInternational" SortExpression="NationalOrInternational" visible="false"/>
            <asp:BoundField DataField="Description" HeaderText="Route Description" 
                SortExpression="Description" />
            
            <asp:TemplateField HeaderText="Route Protocol" SortExpression="field1">
                <EditItemTemplate>
                    <asp:DropDownList ID="ddlistRouteProtocol" runat="server" AutoPostBack="false" 
                        DataSourceID="SqlDataRouteProtocol" DataTextField="Type" DataValueField="id" SelectedValue='<%# Bind("field5") %>'
                        Enabled="True" >
                    </asp:DropDownList>
                    
                </EditItemTemplate>
                <ItemTemplate>
                    <asp:DropDownList ID="ddlistRouteProtocol" runat="server" AutoPostBack="false" 
                        DataSourceID="SqlDataRouteProtocol" DataTextField="Type" DataValueField="id" SelectedValue='<%# Bind("field5") %>'
                        Enabled="false" >
                    </asp:DropDownList>
                </ItemTemplate>
            </asp:TemplateField>

            <asp:TemplateField HeaderText="Route Status" SortExpression="status">
                <EditItemTemplate>
                    <asp:DropDownList ID="ddlistRouteStatus" runat="server" AutoPostBack="True" 
                        DataSourceID="SqlDataRouteStatus" DataTextField="Type" DataValueField="id" SelectedValue='<%# Bind("Status") %>'
                        Enabled="True" >
                    </asp:DropDownList>
                    
                </EditItemTemplate>
                <ItemTemplate>
                    <asp:DropDownList ID="ddlistRouteStatus" runat="server" AutoPostBack="True" 
                        DataSourceID="SqlDataRouteStatus" DataTextField="Type" DataValueField="id" SelectedValue='<%# Bind("Status") %>'
                        Enabled="false" >
                    </asp:DropDownList>
                </ItemTemplate>
            </asp:TemplateField>
            
            <asp:TemplateField HeaderText="Roaming ANS" SortExpression="field1">
                <EditItemTemplate>
                    <asp:DropDownList ID="ddlistRoamingAns" runat="server" AutoPostBack="false" 
                        DataSourceID="SqlDataAnsPartners" DataTextField="PARTNERNAME" DataValueField="idPARTNER" SelectedValue='<%# Bind("field1") %>'
                        Enabled="True" >
                    </asp:DropDownList>
                    
                </EditItemTemplate>
                <ItemTemplate>
                    <asp:DropDownList ID="ddlistRoamingAns" runat="server" AutoPostBack="false" 
                        DataSourceID="SqlDataAnsPartners" DataTextField="PARTNERNAME" DataValueField="idPARTNER" SelectedValue='<%# Bind("field1") %>'
                        Enabled="false" >
                    </asp:DropDownList>
                </ItemTemplate>
            </asp:TemplateField>

            <%--<asp:BoundField DataField="field2" HeaderText="Ingress Port" 
                SortExpression="field2" />
--%>
            <asp:TemplateField HeaderText="Ingress Port" SortExpression="field2">
                <EditItemTemplate>
                    <asp:TextBox ID="TextBoxIngressPort" runat="server" Text='<%# Bind("field2") %>'></asp:TextBox>
                    
                </EditItemTemplate>
                <ItemTemplate>
                    <asp:Label ID="LabelIngressPort" runat="server" Text='<%# Bind("field2") %>'></asp:Label>
                    
                </ItemTemplate>
            </asp:TemplateField>


            <%--<asp:BoundField DataField="field3" HeaderText="Egress Port" 
                SortExpression="field3" />--%>

                <asp:TemplateField HeaderText="Egress Port" SortExpression="field3">
                <EditItemTemplate>
                    <asp:TextBox ID="TextBoxEgressPort" runat="server" Text='<%# Bind("field3") %>'></asp:TextBox>
                    
                </EditItemTemplate>
                <ItemTemplate>
                    <asp:Label ID="LabelEgressPort" runat="server" Text='<%# Bind("field3") %>'></asp:Label>
                    
                </ItemTemplate>
            </asp:TemplateField>

            <%--<asp:BoundField DataField="field4" HeaderText="Common Port" 
                SortExpression="field4" />--%>
            
            <asp:TemplateField HeaderText="Bothway Port" SortExpression="field4">
                <EditItemTemplate>
                    <asp:TextBox ID="TextBoxCommonPort" runat="server" Text='<%# Bind("field4") %>'></asp:TextBox>
                    
                </EditItemTemplate>
                <ItemTemplate>
                    <asp:Label ID="LabelCommonPort" runat="server" Text='<%# Bind("field4") %>'></asp:Label>
                    
                </ItemTemplate>
            </asp:TemplateField>
            
            <asp:TemplateField HeaderText="Route Type" SortExpression="NationalOrInternational">
                <EditItemTemplate>
                    <asp:Label ID="lblRouteType" runat="server" Text='<%# Bind("NationalOrInternational") %>' Visible = "false" />
                    <asp:DropDownList ID="ddlistRouteType" runat="server" Enabled="True" AutoPostBack="True" OnSelectedIndexChanged="ddlistRouteType_OnSelectedIndexChanged">
                    </asp:DropDownList>
                    
                </EditItemTemplate>
                <ItemTemplate>
                    <asp:Label ID="lblRouteType" runat="server" Text='<%# Bind("NationalOrInternational") %>' Visible = "false" />
                    <asp:DropDownList ID="ddlistRouteType" runat="server" Enabled="False">
                    </asp:DropDownList>
                    
                </ItemTemplate>
            </asp:TemplateField>


            <asp:BoundField DataField="date1" HeaderText="date1" SortExpression="date1" visible="false"/>
            <%--<asp:BoundField DataField="field1" HeaderText="field1" 
                SortExpression="field1" visible="false"/>
            <asp:BoundField DataField="field2" HeaderText="field2" 
                SortExpression="field2" visible="false"/>
            <asp:BoundField DataField="field3" HeaderText="field3" 
                SortExpression="field3" visible="false"/>
            <asp:BoundField DataField="field4" HeaderText="field4" 
                SortExpression="field4" visible="false"/>
            <asp:BoundField DataField="field5" HeaderText="field5" 
                SortExpression="field5" visible="false"/>--%>
        </Columns>
        <EditRowStyle BackColor="#999999" />
        <FooterStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
        <HeaderStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
        <PagerStyle BackColor="#284775" ForeColor="White" HorizontalAlign="Center" />
        <RowStyle BackColor="white" ForeColor="#333333" />
        <SelectedRowStyle BackColor="#E2DED6" Font-Bold="True" ForeColor="#333333" />
        <SortedAscendingCellStyle BackColor="#E9E7E2" />
        <SortedAscendingHeaderStyle BackColor="#506C8C" />
        <SortedDescendingCellStyle BackColor="#FFFDF8" />
        <SortedDescendingHeaderStyle BackColor="#6F8DAE" />
    </asp:GridView>
    

    <asp:SqlDataSource ID="SqlDataAnsPartners" runat="server" 
        ConnectionString="<%$ ConnectionStrings:Partner %>" 
        ProviderName="<%$ ConnectionStrings:Partner.ProviderName %>" 
        SelectCommand="select (SELECT NULL) AS idpartner,(SELECT ' NONE') AS partnername 
union all
select idpartner,partnername from partner where partnertype=1 order by partnername " ></asp:SqlDataSource>

    <asp:EntityDataSource ID="EntityDataSource1" runat="server" 
        ConnectionString="name=PartnerEntities" DefaultContainerName="PartnerEntities" 
        EnableDelete="True" EnableFlattening="False" EnableInsert="True" 
        EnableUpdate="True" EntitySetName="routes" 
        onquerycreated="EntityDataSource1_QueryCreated">
    </asp:EntityDataSource>

    <asp:LinkButton ID="LinkButton1" runat="server" Text="Add New Route" 
        onclick="LinkButton1_Click" Font-Size="Smaller">Add New Route</asp:LinkButton>
    
    <asp:ValidationSummary ID="ValidatorSummary" runat="server"
    ValidationGroup="allcontrols" ForeColor="Red" />

    

    <asp:FormView ID="FormViewRouteAdd" runat="server" DataKeyNames="idroute" 
        DataSourceID="EntityDataSource1" DefaultMode="Insert" Font-Size="Smaller" 
        CellPadding="4" ForeColor="#333333" Visible="False" 
        oniteminserted="FormViewRouteAdd_ItemInserted" 
        onitemcreated="FormViewRouteAdd_ItemCreated" 
        oniteminserting="FormViewRouteAdd_ItemInserting" BorderStyle="None" >
        
        <EditRowStyle BackColor="#f2f2f2" />
        
        <FooterStyle BackColor="#1C5E55" Font-Bold="True" ForeColor="White" />
        <HeaderStyle BackColor="#1C5E55" Font-Bold="True" ForeColor="White" />
        
        <InsertItemTemplate>
            
            <table border="0" width="962px">
                <tr>
                    <td width="300px"> <%--first column--%>
                        
                        <div style="text-align:right;">
                        RouteName:
                        <asp:TextBox ID="RouteNameTextBox" runat="server" 
                            Text='<%# Bind("RouteName") %>' />
                        <br />
                        SwitchId:
                        <asp:DropDownList ID="ddlistSwitch" runat="server" AutoPostBack="True" 
                                    DataSourceID="EntityDataCustomerSwitch" DataTextField="SwitchName" DataValueField="idSwitch" SelectedValue='<%# Bind("SwitchId") %>'
                                    Enabled="true" >
                                </asp:DropDownList>
                        <br />
                        Roaming ANS:
                        <asp:DropDownList ID="ddlistRoamingAns" runat="server" AutoPostBack="True" 
                            DataSourceID="SqlDataAnsPartners" DataTextField="PARTNERNAME" DataValueField="idPARTNER" SelectedValue='<%# Bind("field1") %>'
                            Enabled="True" >
                        </asp:DropDownList>
                        <br />
                        Route Type:
                        <asp:DropDownList ID="ddlistRouteType" runat="server" AutoPostBack="True" 
                                            SelectedValue='<%# Bind("NationalOrInternational") %>'
                                            Enabled="True" Visible="True" >
                        </asp:DropDownList>
                        <br />
                        </div>
                    </td>

                    <td width="300px"> <%--Column 2--%>
                        
                        <div style="text-align:right;margin-right:0px;">
                        
                        Route Description:
                        <asp:TextBox ID="DescriptionTextBox" runat="server" 
                        Text='<%# Bind("Description") %>' />
                        <br />
                        Status:
                        <asp:DropDownList ID="ddlistRouteStatus" runat="server" AutoPostBack="True" 
                                DataSourceID="SqlDataRouteStatus" DataTextField="Type" DataValueField="id" SelectedValue='<%# Bind("Status") %>'
                                Enabled="True" >
                            </asp:DropDownList>
                            <br />
                        Route Protocol:
                        <asp:DropDownList ID="ddlistRouteProtocol" runat="server" AutoPostBack="True" 
                        DataSourceID="SqlDataRouteProtocol" DataTextField="Type" DataValueField="id" SelectedValue='<%# Bind("field5") %>'
                        Enabled="True" >
                        </asp:DropDownList>
                        <br />
                    </div>
                    </td>

                    <td width="300px"> <%--first column--%>
                        
                        <div style="text-align:right;">
                        Ingress Port:
                        <asp:TextBox ID="TextBox1" runat="server" 
                            Text='<%# Bind("field2") %>' />
                        <br />
                        Egress Port:
                        <asp:TextBox ID="TextBox2" runat="server" 
                            Text='<%# Bind("field3") %>' />
                        <br />
                        Bothway Port:
                        <asp:TextBox ID="TextBox3" runat="server" 
                            Text='<%# Bind("field4") %>' />
                        <br />
                        </div>
                    </td>

                    
                </tr>
                
                <tr>
                    <td width="198px"> <%--Column 2--%>
                        
                    
                    </td>

                    <td width="100px"> <%--Column 2--%>
                        
                    </td>
                </tr>
            </table>

            <div style="height:0px;visibility:hidden">
            
                <%--idPartner:--%>
                <asp:TextBox ID="idPartnerTextBox" runat="server" 
                    Text='<%# Bind("idPartner") %>' Visible="false" />
                <br />
                <%--NationalOrInternational:--%>
<%--                <asp:TextBox ID="NationalOrInternationalTextBox" runat="server" 
                    Text='<%# Bind("NationalOrInternational") %>' Visible="false" />--%>
                <br />
            
            </div>
            
            <asp:LinkButton ID="InsertButton" runat="server" CausesValidation="True" 
                CommandName="Insert" Text="Insert" ValidationGroup="allcontrols" />
            &nbsp;<asp:LinkButton ID="InsertCancelButton" runat="server" 
                CausesValidation="False" CommandName="Cancel" Text="Cancel" OnClick="FormViewCancel_Click" />
        </InsertItemTemplate>
        
        <PagerStyle BackColor="#666666" ForeColor="White" HorizontalAlign="Center" />
        <RowStyle BackColor="white" />
        
    </asp:FormView>

    
    <asp:EntityDataSource ID="EntityDataSourcePrefix" runat="server" 
        ConnectionString="name=PartnerEntities" DefaultContainerName="PartnerEntities" 
        EnableDelete="True" EnableFlattening="False" EnableInsert="True" 
        EnableUpdate="True" EntitySetName="partnerprefixes" 
        onquerycreated="EntityDataSourcePrefix_QueryCreated">
    </asp:EntityDataSource>

    <asp:SqlDataSource ID="SqlDataCommonTG" runat="server" 
        ConnectionString="<%$ ConnectionStrings:Partner %>" 
        
        ProviderName="<%$ ConnectionStrings:Partner.ProviderName %>" SelectCommand="select (select null) as id, (select null) as TgName
union all
select t.id, concat(tgname,' (',s.switchname,')') as TgName
from commonTG t
join ne s
on t.idswitch=s.idswitch;"></asp:SqlDataSource>

            
<%--"1"> Customer
"2"> Supplier
"3"> ANS--%>
<br />
<br />
    <asp:Label ID="lblPrefix" runat="server" Font-Bold="true" ForeColor="Black" Text="Prefixes"></asp:Label>
    <br />
     <asp:GridView ID="GridViewPrefix" runat="server" AllowPaging="True" 
        AutoGenerateColumns="False" DataKeyNames="id" 
        DataSourceID="EntityDataSourcePrefix" CellPadding="4" 
        ForeColor="#333333" GridLines="Vertical" PageSize="100" 
        BorderColor="#CCCCCC" onrowupdating="GridViewPrefix_RowUpdating" 
        >
         
         <AlternatingRowStyle BackColor="White" />
         
         <Columns>
             
              <asp:TemplateField Visible="false">
                <ItemTemplate>
                    <asp:LinkButton ID="LinkButtonPartnerDetail" runat="server">Edit</asp:LinkButton>
                </ItemTemplate>
            </asp:TemplateField>

            <asp:TemplateField>
                <ItemTemplate>
                    <asp:LinkButton ID="LinkButton2"  runat="server" ButtonType="Button" CommandName="Delete"
                        Text="Delete" HeaderText="Delete" CausesValidation="false" 
                    OnClientClick='return confirm("Are you sure you want to delete this entry?");'> 
                    </asp:LinkButton>
                </ItemTemplate>
            </asp:TemplateField>

             <asp:BoundField DataField="id" HeaderText="id" ReadOnly="True"
                 SortExpression="id" />
             <asp:BoundField DataField="idPartner" HeaderText="idPartner" Visible="false"
                 SortExpression="idPartner" />


             <asp:BoundField DataField="Prefix" HeaderText="Prefix" 
                 SortExpression="Prefix" />

             <%--<asp:BoundField DataField="CommonTG" HeaderText="CommonTG" 
                 SortExpression="CommonTG" />--%>

                  <asp:TemplateField HeaderText="Common TG" SortExpression="CommonTG">
                
                
                <ItemTemplate>
                    <asp:DropDownList ID="DropDownListCommonTG" runat="server" AutoPostBack="True" 
                        DataSourceID="SqlDataCommonTG" DataTextField="TgName" DataValueField="id" SelectedValue='<%# Eval("CommonTG") %>'
                        Enabled="false" >
                    </asp:DropDownList>
                    
                </ItemTemplate>
                
                <EditItemTemplate>
                    <asp:DropDownList ID="DropDownListCommonTG" runat="server" AutoPostBack="True" 
                        DataSourceID="SqlDataCommonTG" DataTextField="TgName" DataValueField="id" SelectedValue='<%# Bind("CommonTG") %>'
                        Enabled="true" >
                    </asp:DropDownList>
                </EditItemTemplate>
                
            </asp:TemplateField>
             

              <asp:TemplateField HeaderText="Prefix Direction" SortExpression="PrefixType">
                  
                
                <ItemTemplate>
                    
                      <asp:DropDownList ID="DropDownListPrefixType" runat="server" SelectedValue='<%# Eval("PrefixType")!=null?Eval("PrefixType"):"" %>' Enabled="false" Visible="true">
                        <asp:ListItem Value="1"> Customer</asp:ListItem>
                        <asp:ListItem Value="2"> Supplier</asp:ListItem>
                        <asp:ListItem Value="3"> ANS</asp:ListItem>
                  </asp:DropDownList>
                    
                </ItemTemplate>
                
                <EditItemTemplate>
                    
                     <asp:DropDownList ID="DropDownListPrefixType" runat="server" SelectedValue='<%# Bind("PrefixType") %>' Enabled="true" Visible="true">
                        <asp:ListItem Value="1"> Customer</asp:ListItem>
                        <asp:ListItem Value="2"> Supplier</asp:ListItem>
                  </asp:DropDownList>

                </EditItemTemplate>
                
            </asp:TemplateField>



         </Columns>
         <EditRowStyle BackColor="#f2f2f2" />
         <FooterStyle BackColor="#1C5E55" Font-Bold="True" ForeColor="White" />
         <HeaderStyle BackColor="#1C5E55" Font-Bold="True" ForeColor="White" />
         <PagerStyle BackColor="#666666" ForeColor="White" HorizontalAlign="Center" />
         <RowStyle BackColor="white" />
         <SelectedRowStyle BackColor="#C5BBAF" Font-Bold="True" ForeColor="#333333" />
         <SortedAscendingCellStyle BackColor="#F8FAFA" />
         <SortedAscendingHeaderStyle BackColor="#246B61" />
         <SortedDescendingCellStyle BackColor="#D4DFE1" />
         <SortedDescendingHeaderStyle BackColor="#15524A" />
        </asp:GridView>

        
         <asp:LinkButton ID="LinkButtonNewPrefix" runat="server" 
        onclick="LinkButtonNewPrefix_Click">Add New Prefix</asp:LinkButton>

        <asp:ValidationSummary ID="ValidationSummaryPrefix" runat="server"
    ValidationGroup="PrefixValidation" ForeColor="Red" />

    <%--DataSourceID="EntityDataSourcePrefix"--%>
    <asp:FormView ID="FormViewPrefix" runat="server" DataKeyNames="id" 
         DefaultMode="Insert" 
        oniteminserting="FormViewPrefix_ItemInserting"
        onitemcreated="FormViewPrefix_ItemCreated"  Visible="False"
        oniteminserted="FormViewPrefix_ItemInserted" CellPadding="4" 
        ForeColor="#333333" onmodechanging="FormViewPrefix_ModeChanging" BorderStyle="Solid"
        >
      
        <EditRowStyle BackColor="#f2f2f2" />
        <FooterStyle BackColor="#1C5E55" Font-Bold="True" ForeColor="White" />
        <HeaderStyle BackColor="#1C5E55" Font-Bold="True" ForeColor="White" />
      
        <InsertItemTemplate>
            
            Prefix:
            <asp:TextBox ID="PrefixTextBox" runat="server" Text="" />
            
            <asp:Label ID="lblCommonTG" runat="server" Text="Common TG"></asp:Label>
                <asp:DropDownList ID="DropDownListCommonTG" runat="server" AutoPostBack="false" 
                        DataSourceID="SqlDataCommonTG" DataTextField="TgName" DataValueField="id" 
                        Enabled="true" >
                    </asp:DropDownList>
            <asp:Label ID="lblPrefixDirection" runat="server" Text="Prefix Direction"></asp:Label>

            <asp:DropDownList ID="DropDownListPType" runat="server" Enabled="true" Visible="true">
                <asp:ListItem Value="1"> Customer</asp:ListItem>
                <asp:ListItem Value="2"> Supplier</asp:ListItem>
            </asp:DropDownList>

            <asp:TextBox ID="PrefixTypeTextBox" Visible="false" runat="server" Text="" />

            <%--<asp:TextBox ID="idTextBox" runat="server" Text='<%# Bind("id") %>' visible="false"/>--%>
            <br />
            
            <asp:TextBox ID="idPartnerTextBox" runat="server" visible="false"
                Text="" />
            

            <br />      
            

            <br />
            
                  <asp:LinkButton ID="LinkButtonFrmPrefixInsert" runat="server" CausesValidation="True" 
                CommandName="Insert" Text="Insert" ValidationGroup="PrefixValidation" />

&nbsp;<asp:LinkButton ID="LinkButtonFrmPrefixCancel" runat="server" CausesValidation="False" 
                CommandName="Cancel" Text="Cancel" OnClick="LinkButtonFrmPrefixCancel_Click" />

        </InsertItemTemplate>
       
        <PagerStyle BackColor="#666666" ForeColor="White" HorizontalAlign="Center" />
        <RowStyle BackColor="white" />
       
    </asp:FormView>


    

     <asp:CustomValidator ID="cvAll" runat="server" 
    Display="Dynamic"
    ErrorMessage="This membership"
    OnServerValidate="cvAll_Validate" 
    ValidationGroup="allcontrols"
    Text=""></asp:CustomValidator>

     <asp:CustomValidator ID="cvPrefix" runat="server" 
    Display="Dynamic"
    ErrorMessage=""
    OnServerValidate="cvPrefix_Validate" 
    ValidationGroup="PrefixValidation"
    Text=""></asp:CustomValidator>


    <asp:SqlDataSource ID="SqlDataRouteStatus" runat="server" 
        ConnectionString="<%$ ConnectionStrings:Partner %>" 
        ProviderName="<%$ ConnectionStrings:Partner.ProviderName %>" 
        SelectCommand="select * from enumroutestatus"></asp:SqlDataSource>
    <asp:EntityDataSource ID="EntityDataCustomerSwitch" runat="server" 
        onquerycreated="EntityDataCustomerSwitch_QueryCreated" 
        ConnectionString="name=PartnerEntities" 
        DefaultContainerName="PartnerEntities" EnableFlattening="False" 
        EntitySetName="nes">
    </asp:EntityDataSource>



    </div>

</asp:Content>

