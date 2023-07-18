<%@ Page Title="Login History" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="LoginHistory.aspx.cs" Inherits="WebApplication1.Account.LoginHistory" %>

<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <h2><%: this.Title %></h2>
    <p class="text-danger">
        <asp:Literal runat="server" ID="ErrorMessage" />
    </p>
    
    <div class="form-horizontal">
        
        <h3>Select Users to Manage</h3>

        <asp:GridView ID="gvLoginHistory" runat="server" AllowPaging="True" 
        AllowSorting="False" AutoGenerateColumns="False" CellPadding="4" 
        DataKeyNames="id"  ForeColor="#333333" >
        <AlternatingRowStyle BackColor="#f2f2f2" ForeColor="#284775" />
        <Columns>             
            <asp:BoundField DataField="username" HeaderText="User Name" SortExpression="username" visible="true" />
            <asp:BoundField DataField="remote_ip" HeaderText="Remote IP" SortExpression="remote_ip" visible="true" />
            <asp:BoundField DataField="login_time" HeaderText="Login Time" SortExpression="login_time" visible="true" />
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
</asp:Content>
