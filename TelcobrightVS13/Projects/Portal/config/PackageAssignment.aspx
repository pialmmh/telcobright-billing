<%@ Page Title="" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeBehind="PackageAssignment.aspx.cs" Inherits="PortalApp.config.PackageAssignment" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <asp:SqlDataSource ID="SqlDataSource1" runat="server" ConnectionString="server=127.0.0.1;user id=root;password=Takay1#$ane;persistsecurityinfo=True;database=tpny" ProviderName="MySql.Data.MySqlClient" SelectCommand="select * from datedassignment"></asp:SqlDataSource>
    <asp:FormView ID="FormView1" runat="server" DataSourceID="SqlDataSource1" AllowPaging="True" CellPadding="4" DataKeyNames="id" ForeColor="#333333" OnItemInserting="FormView1_ItemInserting">
        <EditItemTemplate>
            id:
            <asp:Label ID="idLabel1" runat="server" Text='<%# Eval("id") %>' />
            <br />
            AssignmentType:
            <asp:TextBox ID="AssignmentTypeTextBox" runat="server" Text='<%# Bind("AssignmentType") %>' />
            <br />
            description:
            <asp:TextBox ID="descriptionTextBox" runat="server" Text='<%# Bind("description") %>' />
            <br />
            assignedvalue:
            <asp:TextBox ID="assignedvalueTextBox" runat="server" Text='<%# Bind("assignedvalue") %>' />
            <br />
            startdate:
            <asp:TextBox ID="startdateTextBox" runat="server" Text='<%# Bind("startdate") %>' />
            <br />
            enddate:
            <asp:TextBox ID="enddateTextBox" runat="server" Text='<%# Bind("enddate") %>' />
            <br />
            Inactive:
            <asp:TextBox ID="InactiveTextBox" runat="server" Text='<%# Bind("Inactive") %>' />
            <br />
            field1:
            <asp:TextBox ID="field1TextBox" runat="server" Text='<%# Bind("field1") %>' />
            <br />
            field2:
            <asp:TextBox ID="field2TextBox" runat="server" Text='<%# Bind("field2") %>' />
            <br />
            field3:
            <asp:TextBox ID="field3TextBox" runat="server" Text='<%# Bind("field3") %>' />
            <br />
            field4:
            <asp:TextBox ID="field4TextBox" runat="server" Text='<%# Bind("field4") %>' />
            <br />
            field5:
            <asp:TextBox ID="field5TextBox" runat="server" Text='<%# Bind("field5") %>' />
            <br />
            AssignmentParam:
            <asp:TextBox ID="AssignmentParamTextBox" runat="server" Text='<%# Bind("AssignmentParam") %>' />
            <br />
            Comment:
            <asp:TextBox ID="CommentTextBox" runat="server" Text='<%# Bind("Comment") %>' />
            <br />
            <asp:LinkButton ID="UpdateButton" runat="server" CausesValidation="True" CommandName="Update" Text="Update" />
            &nbsp;<asp:LinkButton ID="UpdateCancelButton" runat="server" CausesValidation="False" CommandName="Cancel" Text="Cancel" />
        </EditItemTemplate>
        <EditRowStyle BackColor="#999999" />
        <FooterStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
        <HeaderStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
        <InsertItemTemplate>
            AssignmentType:
            <asp:TextBox ID="AssignmentTypeTextBox" runat="server" Text='<%# Bind("AssignmentType") %>' />
            <br />
            description:
            <asp:TextBox ID="descriptionTextBox" runat="server" Text='<%# Bind("description") %>' />
            <br />
            assignedvalue:
            <asp:TextBox ID="assignedvalueTextBox" runat="server" Text='<%# Bind("assignedvalue") %>' />
            <br />
            startdate:
            <asp:TextBox ID="startdateTextBox" runat="server" Text='<%# Bind("startdate") %>' />
            <br />
            enddate:
            <asp:TextBox ID="enddateTextBox" runat="server" Text='<%# Bind("enddate") %>' />
            <br />
            Inactive:
            <asp:TextBox ID="InactiveTextBox" runat="server" Text='<%# Bind("Inactive") %>' />
            <br />
            field1:
            <asp:TextBox ID="field1TextBox" runat="server" Text='<%# Bind("field1") %>' />
            <br />
            field2:
            <asp:TextBox ID="field2TextBox" runat="server" Text='<%# Bind("field2") %>' />
            <br />
            field3:
            <asp:TextBox ID="field3TextBox" runat="server" Text='<%# Bind("field3") %>' />
            <br />
            field4:
            <asp:TextBox ID="field4TextBox" runat="server" Text='<%# Bind("field4") %>' />
            <br />
            field5:
            <asp:TextBox ID="field5TextBox" runat="server" Text='<%# Bind("field5") %>' />
            <br />
            AssignmentParam:
            <asp:TextBox ID="AssignmentParamTextBox" runat="server" Text='<%# Bind("AssignmentParam") %>' />
            <br />
            Comment:
            <asp:TextBox ID="CommentTextBox" runat="server" Text='<%# Bind("Comment") %>' />
            <br />
            <asp:LinkButton ID="InsertButton" runat="server" CausesValidation="True" CommandName="Insert" Text="Insert" />
            &nbsp;<asp:LinkButton ID="InsertCancelButton" runat="server" CausesValidation="False" CommandName="Cancel" Text="Cancel" />
        </InsertItemTemplate>
        <ItemTemplate>
            id:
            <asp:Label ID="idLabel" runat="server" Text='<%# Eval("id") %>' />
            <br />
            AssignmentType:
            <asp:Label ID="AssignmentTypeLabel" runat="server" Text='<%# Bind("AssignmentType") %>' />
            <br />
            description:
            <asp:Label ID="descriptionLabel" runat="server" Text='<%# Bind("description") %>' />
            <br />
            assignedvalue:
            <asp:Label ID="assignedvalueLabel" runat="server" Text='<%# Bind("assignedvalue") %>' />
            <br />
            startdate:
            <asp:Label ID="startdateLabel" runat="server" Text='<%# Bind("startdate") %>' />
            <br />
            enddate:
            <asp:Label ID="enddateLabel" runat="server" Text='<%# Bind("enddate") %>' />
            <br />
            Inactive:
            <asp:Label ID="InactiveLabel" runat="server" Text='<%# Bind("Inactive") %>' />
            <br />
            field1:
            <asp:Label ID="field1Label" runat="server" Text='<%# Bind("field1") %>' />
            <br />
            field2:
            <asp:Label ID="field2Label" runat="server" Text='<%# Bind("field2") %>' />
            <br />
            field3:
            <asp:Label ID="field3Label" runat="server" Text='<%# Bind("field3") %>' />
            <br />
            field4:
            <asp:Label ID="field4Label" runat="server" Text='<%# Bind("field4") %>' />
            <br />
            field5:
            <asp:Label ID="field5Label" runat="server" Text='<%# Bind("field5") %>' />
            <br />
            AssignmentParam:
            <asp:Label ID="AssignmentParamLabel" runat="server" Text='<%# Bind("AssignmentParam") %>' />
            <br />
            Comment:
            <asp:Label ID="CommentLabel" runat="server" Text='<%# Bind("Comment") %>' />
            <br />

        </ItemTemplate>
        <PagerStyle BackColor="#284775" ForeColor="White" HorizontalAlign="Center" />
        <RowStyle BackColor="#F7F6F3" ForeColor="#333333" />
    </asp:FormView>
    </asp:Content>


