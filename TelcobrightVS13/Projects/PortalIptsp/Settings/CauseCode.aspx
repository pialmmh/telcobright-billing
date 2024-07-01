<%@ Page Title="" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="True" CodeBehind="CauseCode.aspx.cs" Inherits="SettingsCauseCode" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" Runat="Server">



    <asp:EntityDataSource ID="EntityDataSource1" runat="server" 
        ConnectionString="name=PartnerEntities" 
        DefaultContainerName="PartnerEntities" 
        EnableFlattening="False" 
        EntitySetName="causecodes" 
        onquerycreated="EntityDataSource1_QueryCreated" EnableDelete="True" 
        EnableInsert="True" EnableUpdate="True"
        >
        
    </asp:EntityDataSource>

    
    

    <script type="text/javascript">
        function SetHidValueFilter(value) {
            document.getElementById("<%= this.hidValueFilter.ClientID%>").value = value;
        }
    </script>

<div style="padding-left:20px;min-width:700px;">
    <div style="font-weight:bold;float:left;"> Cause Codes </div>
    <div style="padding-left:20px;clear:left;">
    
    Switch:
     <asp:DropDownList ID="DropDownListSwitchSelect" runat="server" AutoPostBack="True" 
                        DataTextField="SwitchName" 
            DataValueField="idSwitch" >
            <asp:ListItem Value="-1"> All</asp:ListItem>
                    </asp:DropDownList>
    
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


    <asp:FormView ID="FormView1" runat="server" CellPadding="4" DataKeyNames="id" 
        DataSourceID="EntityDataSource1" ForeColor="#333333" 
        oniteminserted="FormView1_ItemInserted">
        
        <EditRowStyle BackColor="#7C6F57" />
        <FooterStyle BackColor="#1C5E55" Font-Bold="True" ForeColor="White" />
        <HeaderStyle BackColor="#1C5E55" Font-Bold="True" ForeColor="White" />
        <InsertItemTemplate>
            <div style="text-align:right;background-color:#f2f2f2;">
                Cause Code:
                <asp:TextBox ID="CodeTextBox" runat="server" Text='<%# Bind("CC") %>' />
                <br />
                Description:
                <asp:TextBox ID="NameTextBox" runat="server" Text='<%# Bind("Description") %>' />
                <br />
                Switch:
                <asp:DropDownList ID="DropDownList2" runat="server" AutoPostBack="True" 
                        DataSourceID="EntityDataSourceSwitch" DataTextField="SwitchName" DataValueField="idSwitch" SelectedValue='<%# Bind("idSwitch") %>'
                        Enabled="True" >
                    </asp:DropDownList>
                <br />
                Call Connect Indicator:
                    <asp:DropDownList ID="DropDownListFormat" runat="server" SelectedValue='<%# Bind("CallCompleteIndicator") %>' Enabled="true">
                        <asp:ListItem Value="0"> No</asp:ListItem>
                        <asp:ListItem Value="1"> Yes</asp:ListItem>
                    </asp:DropDownList>        

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




    <asp:EntityDataSource ID="EntityDataSourceSwitch" runat="server" 
        ConnectionString="name=PartnerEntities" 
        DefaultContainerName="PartnerEntities" EnableFlattening="False" 
        EntitySetName="nes" 
        onquerycreated="EntityDataSourceSwitch_QueryCreated">
    </asp:EntityDataSource>


    <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="False" 
        DataKeyNames="id" DataSourceID="EntityDataSource1" AllowPaging="True" 
        AllowSorting="True" CellPadding="4" ForeColor="#333333" GridLines="Vertical" 
        PageSize="100">
        <AlternatingRowStyle BackColor="White" />
        <Columns>
            <asp:CommandField ShowDeleteButton="True" ShowEditButton="True" />
            <asp:TemplateField HeaderText="id" SortExpression="id" Visible="false">
                <EditItemTemplate>
                    <asp:Label ID="Label1" runat="server" Text='<%# Eval("id") %>' Visible="false"></asp:Label>
                </EditItemTemplate>
                <ItemTemplate>
                    <asp:Label ID="Label11" runat="server" Text='<%# Bind("id") %>' Visible="false"></asp:Label>
                </ItemTemplate>
            </asp:TemplateField>

            <asp:TemplateField HeaderText="Switch" SortExpression="idSwitch">
                
                <ItemTemplate>
                    
                   
                    <asp:DropDownList ID="DropDownList2" runat="server" AutoPostBack="True" 
                        DataSourceID="EntityDataSourceSwitch" DataTextField="SwitchName" DataValueField="idSwitch" SelectedValue='<%# Bind("idSwitch") %>'
                        Enabled="false" >
                    </asp:DropDownList>
                    
                </ItemTemplate>
                
                <EditItemTemplate>
                    <asp:DropDownList ID="DropDownList2" runat="server" AutoPostBack="True" 
                        DataSourceID="EntityDataSourceSwitch" DataTextField="SwitchName" DataValueField="idSwitch" SelectedValue='<%# Bind("idSwitch") %>'
                        Enabled="false" >
                    </asp:DropDownList>
                    
                </EditItemTemplate>
                
            </asp:TemplateField>

            <asp:TemplateField HeaderText="Cause Code" SortExpression="CC">
                
                <ItemTemplate>
                    <asp:Label ID="Label22" runat="server" Text='<%# Eval("CC").ToString() %>' Visible="true"></asp:Label>
                </ItemTemplate>
                
                <EditItemTemplate>
                    <asp:Label ID="Txt2" runat="server" Text='<%# Bind("CC") %>' Visible="true"></asp:Label>
                </EditItemTemplate>
                
            </asp:TemplateField>

            <asp:TemplateField HeaderText="Description" SortExpression="Name">
                <ItemTemplate>
                    <asp:Label ID="Label2" runat="server" Text='<%# Eval("Description") %>'></asp:Label>
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:TextBox ID="textbox2" runat="server" Text='<%# Bind("Description") %>'></asp:TextBox>
                </EditItemTemplate>
            </asp:TemplateField>

             
            
             <asp:TemplateField HeaderText="Call Connect Indicator" SortExpression="CallCompleteIndicator">
                
                <ItemTemplate>
                    <asp:DropDownList ID="DropDownListFormat" runat="server" SelectedValue='<%# Eval("CallCompleteIndicator") %>' Enabled="false">
                        <asp:ListItem Value="0"> No</asp:ListItem>
                        <asp:ListItem Value="1"> Yes</asp:ListItem>
                    </asp:DropDownList>        
                </ItemTemplate>
                
                <EditItemTemplate>
                    <asp:DropDownList ID="DropDownListFormat" runat="server" SelectedValue='<%# Bind("CallCompleteIndicator") %>'>
                        <asp:ListItem Value="0"> No</asp:ListItem>
                        <asp:ListItem Value="1"> Yes</asp:ListItem>
                    </asp:DropDownList>        
                    
                </EditItemTemplate>
                
            </asp:TemplateField>


            
            







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

