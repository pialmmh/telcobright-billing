<%@ Page Title="" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="True" CodeBehind="ViewRoutes.aspx.cs" Inherits="ConfigViewRoutes" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" Runat="Server">

<div style="padding-left:0px;margin-top:-5px;">

 <div>
    

      
        <asp:SqlDataSource ID="SqlDataPartner" runat="server" 
            ConnectionString="<%$ ConnectionStrings:Partner %>" 
            ProviderName="<%$ ConnectionStrings:Partner.ProviderName %>" 
            SelectCommand=" select (select -1) as idPartner,(select ' [All]') as PartnerName 
                            union all 
                             select idPartner,PartnerName from partner where partnertype!=1 order by PartnerName"></asp:SqlDataSource>
       
     
    </div>

<div style="padding:2px; width:900px;background-color:#f2f2f2;margin-top:5px;height:23px;">
        <span style="color:Black;margin-right:20px;"><b>View Routes</b></span>
        Partner: 

            <asp:DropDownList ID="ddlistPartner" runat="server" 
                DataSourceID="SqlDataPartner" DataTextField="PartnerName" 
                DataValueField="idPartner" AutoPostBack="false" 
                >
            </asp:DropDownList>
    Switch:
    <asp:DropDownList ID="DropDownListNE" runat="server" >
            </asp:DropDownList>

    Route Name Contains:
    <asp:TextBox ID="TextBoxName" runat="server"></asp:TextBox>
    
    </div>
    
  <div style="clear:left;padding-top:5px;padding-left:0px;">
      <asp:Button ID="Button1" runat="server" Text="Find" Width="100px" OnClick="Button1_Click" />

  
    <asp:Label ID="lblStatus" runat="server" Text=""></asp:Label>
    <asp:Button ID="ButtonExport" runat="server" Text="Export" Visible="false" 
                onclick="ButtonExport_Click"> </asp:Button>

  </div>
    
    
  
<div style="margin-left:0px;margin-top:5px;clear:left;">

    <asp:GridView ID="GridView1" runat="server" CellPadding="4" ForeColor="#333333" GridLines="Vertical">
        <AlternatingRowStyle BackColor="White" ForeColor="#284775" />
        <EditRowStyle BackColor="#999999" />
        <FooterStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
        <HeaderStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
        <PagerStyle BackColor="#284775" ForeColor="White" HorizontalAlign="Center" />
        <RowStyle BackColor="#F7F6F3" ForeColor="#333333" />
        <SelectedRowStyle BackColor="#E2DED6" Font-Bold="True" ForeColor="#333333" />
        <SortedAscendingCellStyle BackColor="#E9E7E2" />
        <SortedAscendingHeaderStyle BackColor="#506C8C" />
        <SortedDescendingCellStyle BackColor="#FFFDF8" />
        <SortedDescendingHeaderStyle BackColor="#6F8DAE" />
    </asp:GridView>

  
    
    </div>

        


</asp:Content>

