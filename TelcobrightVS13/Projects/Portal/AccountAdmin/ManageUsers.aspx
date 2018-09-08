<%@ Page Title="Manage Users" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ManageUsers.aspx.cs" Inherits="WebApplication1.Account.Register" %>

<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <h2><%: this.Title %></h2>
    <p class="text-danger">
        <asp:Literal runat="server" ID="ErrorMessage" />
    </p>
    
    <div class="form-horizontal">
        
        <h3>Select Users to Manage</h3>

        <asp:GridView ID="GridViewUsers" runat="server" AllowPaging="True" 
        AllowSorting="False" AutoGenerateColumns="False" CellPadding="4" 
        DataKeyNames="userId"  ForeColor="#333333" OnRowEditing="GridViewUsers_RowEditing" OnRowDataBound="GridViewUsers_RowDataBound" OnRowDeleting="GridViewUsers_RowDeleting" 
        >
        <AlternatingRowStyle BackColor="#f2f2f2" ForeColor="#284775" />
        <Columns>
            <%--onClientClick="window.open('PartnerDetail.aspx?idpartner=3')"--%>
            
             <asp:TemplateField> <%--delete--%>
                <ItemTemplate>
                    
                    <asp:LinkButton ID="LinkButtonDelete"  runat="server" ButtonType="Button" CommandName="Delete"
                        Text="Delete" HeaderText="" CausesValidation="false" ValidationGroup="allcontrols"
                    OnClientClick='return confirm("Are you sure you want to delete this entry?");'> 
                    </asp:LinkButton>
                    <asp:Label ID="lblUserId" runat="server" Text='<%# Eval("userId") %>'  Visible="false"></asp:Label>
                </ItemTemplate>
            </asp:TemplateField>
             
            <asp:TemplateField>
                <ItemTemplate>
                   
                   
                    <asp:LinkButton ID="LinkButtonEdit" CommandName="edit" runat="server">Edit</asp:LinkButton>
                    
                </ItemTemplate>
            </asp:TemplateField>

            <%--<asp:TemplateField>
                <ItemTemplate>
                    
                    <asp:LinkButton ID="LinkButton2"  runat="server" ButtonType="Button" CommandName="Delete"
                        Text="Delete" HeaderText="Delete" CausesValidation="false" ValidationGroup="allcontrols"
                    OnClientClick='return confirm("Are you sure you want to delete this entry?");'> 
                    </asp:LinkButton>
                    <asp:Label ID="lblIdPartner" runat="server" Text='<%# Eval("idpartner") %>'  Visible="false"></asp:Label>
                </ItemTemplate>
            </asp:TemplateField>--%>

            
            <%--<asp:CommandField ShowDeleteButton="false" />--%>
            <asp:BoundField DataField="userId" HeaderText="User Id" ReadOnly="True"
                SortExpression="userId" visible="false"/>
            <asp:BoundField DataField="userName" HeaderText="User Name" SortExpression="userName" visible="true" />
            <asp:BoundField DataField="roleName" HeaderText="Role" SortExpression="userRole" visible="true" />


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
        
        <hr />
        
        <asp:LinkButton ID="LinkButtonNewUser" runat="server" OnClick="LinkButtonNewUser_Click">Create New User</asp:LinkButton>
        <br />
        <asp:LinkButton ID="LinkButtonLoginHistory" runat="server" OnClick="LinkButtonLoginHistory_Click">Login History</asp:LinkButton>
        <br />
        <asp:Label ID="lblStatus" runat="server" Text=""></asp:Label>
        <div id="DivManageUser" runat="server">
            
            
        <h4>
            <asp:Label ID="lblEditCaption" Font-Bold="true" runat="server" Text="Create/ Manage User" ></asp:Label>

        </h4>
            
            <div class="form-group" style="margin-top:10px;">
                <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
            <asp:Label runat="server" AssociatedControlID="Email" CssClass="col-md-2 control-label">Email</asp:Label>
            <div class="col-md-10">
                <asp:TextBox runat="server" ID="Email" CssClass="form-control" TextMode="Email"  style="min-width:600px;" />
                
            </div>
            <asp:Label runat="server" Width="300px" CssClass="col-md-2 control-label">Role</asp:Label>
            <div class="col-md-10">
                <asp:DropDownList ID="DropDownListRole" runat="server"></asp:DropDownList>
            </div>

                 <div id="div1" runat="server" visible="false">
            <div class="form-group">
            <asp:Label runat="server" AssociatedControlID="Password" CssClass="col-md-2 control-label">Password</asp:Label>
            <div class="col-md-10">
                <asp:TextBox runat="server" ID="TextBox1" TextMode="Password" CssClass="form-control" />
                
            </div>
        </div>
        </div>
                
        <div style="margin-top:10px;">
            
            
            <asp:UpdatePanel ID="UpdatePanel1" runat="server">


                <ContentTemplate>


        <div id="divPasswordCaption" runat="server">
            <asp:LinkButton ID="LinkButtonChangePassword" runat="server" OnClick="LinkButton1_Click">Change Password</asp:LinkButton>
        </div>
         
        <div id="divPassword" runat="server" visible="false">
            <div class="form-group">
            <asp:Label runat="server" AssociatedControlID="Password" CssClass="col-md-2 control-label">Password</asp:Label>
            <div class="col-md-10">
                <asp:TextBox runat="server" ID="Password" TextMode="Password" CssClass="form-control" />
                
            </div>
        </div>
                       
        <div class="form-group">
            <asp:Label runat="server" AssociatedControlID="ConfirmPassword" CssClass="col-md-2 control-label">Confirm password</asp:Label>
            <div class="col-md-10">
                <asp:TextBox runat="server" ID="ConfirmPassword" TextMode="Password" CssClass="form-control" />
                
                
            </div>
        </div>
        </div>

                </ContentTemplate>

            </asp:UpdatePanel>

        </div>
            

        
        
        </div>
        
        <br />

        <div class="form-group">
            <div class="col-md-offset-2 col-md-10">
                <asp:Button runat="server" OnClick="Cancel_Click" Text="Cancel" CssClass="btn btn-default" />
                <span style="padding-left:10px;">
                    <asp:Button ID="button_save" runat="server" OnClick="CreateUser_Click" Text="Save" CssClass="btn btn-default" />
                </span>
            </div>
        </div>
    </div>
</asp:Content>
