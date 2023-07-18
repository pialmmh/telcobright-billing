<%@ Page Title="" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="True" 
    CodeBehind="Country.aspx.cs" Inherits="SettingsCountry" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" Runat="Server">



    <asp:EntityDataSource ID="EntityDataSource1" runat="server" 
        ConnectionString="name=PartnerEntities" 
        DefaultContainerName="PartnerEntities" EnableDelete="True" 
        EnableFlattening="False" EnableInsert="True" EnableUpdate="True" 
        EntitySetName="countrycodes" 
        onquerycreated="EntityDataSource1_QueryCreated"
        >
        
    </asp:EntityDataSource>

    <script type="text/javascript">
        function SetHidValueFilter(value) {
            document.getElementById("<%= this.hidValueFilter.ClientID%>").value = value;
        }
    </script>

<div style="padding-left:20px;min-width:700px;">
    <div style="font-weight:bold;float:left;"> Country Codes </div>
    <div style="padding-left:20px;clear:left;">
    Show Codes starting with Digits:    
        <asp:TextBox ID="TextBoxSearchCode" runat="server" ></asp:TextBox>
        <asp:Button ID="ButtonFilter" runat="server" Text="Search" ViewStateMode="Enabled" 
            onclick="ButtonFilter_Click" />
    </div>
    <asp:HiddenField ID="hidValueFilter" runat="server" Value="" />

            <asp:CustomValidator ID="cvAll" runat="server" 
                Display="Dynamic"
                ErrorMessage="This membership"
                OnServerValidate="cvAll_Validate" 
                ValidationGroup="allcontrols"
                Text=""></asp:CustomValidator>

    <asp:ValidationSummary ID="ValidationSummary1" runat="server" ValidationGroup="allcontrols" ForeColor="Red" />


    <asp:FormView ID="FormView1" runat="server" CellPadding="4" DataKeyNames="Code" 
        DataSourceID="EntityDataSource1" ForeColor="#333333" 
        oniteminserted="FormView1_ItemInserted1">
        
        <EditRowStyle BackColor="#7C6F57" />
        <FooterStyle BackColor="#1C5E55" Font-Bold="True" ForeColor="White" />
        <HeaderStyle BackColor="#1C5E55" Font-Bold="True" ForeColor="White" />
        <InsertItemTemplate>
            <div style="text-align:right;background-color:#f2f2f2;">
                Code:
                <asp:TextBox ID="CodeTextBox" runat="server" Text='<%# Bind("Code") %>' />
                <br />
                Name:
                <asp:TextBox ID="NameTextBox" runat="server" Text='<%# Bind("Name") %>' />
                <br />
                Ref ASR:
                <asp:TextBox ID="refasrTextBox" runat="server" Text='<%# Bind("refasr") %>' />
                <br />
                Ref ACD:
                <asp:TextBox ID="refacdTextBox" runat="server" Text='<%# Bind("refacd") %>' />
                <br />
                Ref CCR:
                <asp:TextBox ID="refccrTextBox" runat="server" Text='<%# Bind("refccr") %>' />
                <br />
                Ref CCR by Cause Code:
                <asp:TextBox ID="refccrbyccTextBox" runat="server" 
                    Text='<%# Bind("refccrbycc") %>' />
                <br />
                Ref PDD:
                <asp:TextBox ID="TextBox1" runat="server" 
                    Text='<%# Bind("refpdd") %>' />
                <br />
                Ref ASR (False Answer Signal):
                <asp:TextBox ID="refasrfasTextBox" runat="server" Text='<%# Bind("refasrfas") %>' />
                <br />
            </div>
            <asp:LinkButton ID="InsertButton" runat="server" CausesValidation="True" ValidationGroup="allcontrols" 
                CommandName="Insert" Text="Insert" />
            &nbsp;<asp:LinkButton ID="InsertCancelButton" runat="server" 
                CausesValidation="False" CommandName="Cancel" Text="Cancel" />
        </InsertItemTemplate>

        <ItemTemplate>
            
                &nbsp;<asp:LinkButton ID="NewButton" runat="server" CausesValidation="False" 
                CommandName="New" Text="New" ValidationGroup="allcontrols" />
            
        </ItemTemplate>
        <PagerStyle BackColor="#666666" ForeColor="White" HorizontalAlign="Center" />
        <RowStyle BackColor="white" />
    </asp:FormView>

    <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="False" 
        DataKeyNames="Code" DataSourceID="EntityDataSource1" AllowPaging="True" 
        AllowSorting="True" CellPadding="4" ForeColor="#333333" GridLines="Vertical" 
        PageSize="100">
        <AlternatingRowStyle BackColor="White" />
        <Columns>
            <asp:CommandField ShowDeleteButton="True" ShowEditButton="True" />
            <asp:TemplateField HeaderText="Country Code" SortExpression="Code">
                <EditItemTemplate>
                    <asp:Label ID="Label1" runat="server" Text='<%# Eval("Code") %>'></asp:Label>
                </EditItemTemplate>
                <ItemTemplate>
                    <asp:Label ID="Label1" runat="server" Text='<%# Bind("Code") %>'></asp:Label>
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Name" SortExpression="Name">
                <EditItemTemplate>
                    <asp:Label ID="Label2" runat="server" Text='<%# Bind("Name") %>'></asp:Label>
                </EditItemTemplate>
                <ItemTemplate>
                    <asp:Label ID="Label2" runat="server" Text='<%# Bind("Name") %>'></asp:Label>
                </ItemTemplate>
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
        <PagerStyle BackColor="#666666" ForeColor="White" HorizontalAlign="Center" />
        <RowStyle BackColor="white" />
        <SelectedRowStyle BackColor="#C5BBAF" Font-Bold="True" ForeColor="#333333" />
        <SortedAscendingCellStyle BackColor="#F8FAFA" />
        <SortedAscendingHeaderStyle BackColor="#246B61" />
        <SortedDescendingCellStyle BackColor="#D4DFE1" />
        <SortedDescendingHeaderStyle BackColor="#15524A" />

    </asp:GridView>
    

</div>


</asp:Content>

