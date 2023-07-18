<%@ Page Title="" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="True" CodeBehind="xyzprefixset.aspx.cs" Inherits="ConfigRouteImportxyz" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" Runat="Server">


<div style="min-width:1200px;text-align:left;">
    <span style="font-weight:bold;">XYZ Prefix Sets</span>
    <br />
    <div style="width:400px;float:left;min-height:800px;background-color:#f2f2f2;margin-right:5px;padding-left:5px;">
        <%--<asp:LinkButton ID="LinkButtonNewPrefix" runat="server" 
        onclick="LinkButtonNewPrefix_Click">New Set</asp:LinkButton>--%>

        <asp:Label ID="lblValidation" ForeColor="Red" runat="server" Text=""></asp:Label>

        <asp:FormView ID="FormView1" runat="server" DataKeyNames="id" 
        DataSourceID="EntityDataPrefixSet" CellPadding="4" ForeColor="#333333" 
            oniteminserted="FormView1_ItemInserted" 
            oniteminserting="FormView1_ItemInserting" 
            onmodechanging="FormView1_ModeChanging">
        
            <EditRowStyle BackColor="#7C6F57" />
            <FooterStyle BackColor="#1C5E55" Font-Bold="True" ForeColor="White" />
            <HeaderStyle BackColor="#1C5E55" Font-Bold="True" ForeColor="White" />
        
        <InsertItemTemplate>
            <div style="text-align:right;">
            
            <%--<asp:TextBox ID="idTextBox" runat="server" Text='<%# Bind("id") %>' />--%>
            <br />
            Name:
            <asp:TextBox ID="NameTextBox" runat="server" Text='<%# Bind("Name") %>' />
            <br />
            Description:
            <asp:TextBox ID="DescriptionTextBox" runat="server" 
                Text='<%# Bind("Description") %>' />
            <br />
            </div>
            <asp:LinkButton ID="InsertButton" runat="server" CausesValidation="True" 
                CommandName="Insert" Text="Insert" />
            &nbsp;<asp:LinkButton ID="InsertCancelButton" runat="server" 
                CausesValidation="False" CommandName="Cancel" Text="Cancel" onclick="LinkButtonCancel_Click" />
        </InsertItemTemplate>
        
        <ItemTemplate>
            
            
            <asp:LinkButton ID="NewButton" runat="server" CausesValidation="False" 
                CommandName="New" Text="New" />
        </ItemTemplate>
        
            <PagerStyle BackColor="#666666" ForeColor="White" HorizontalAlign="Center" />
            <RowStyle BackColor="white" />
        
    </asp:FormView>
        <span style="font-weight:bold;">Select a Prefix Set to manage/import prefix</span>   
        
        <asp:HiddenField ID="HiddenFieldSelectedId" runat="server" Value="-1" />
        
        <asp:GridView ID="GridViewPrefixSet" runat="server" AutoGenerateColumns="False" 
        CellPadding="4" DataKeyNames="id" DataSourceID="EntityDataPrefixSet" 
        ForeColor="Black" GridLines="Vertical" AllowPaging="True" BackColor="White" 
            BorderColor="#DEDFDE" BorderStyle="None" BorderWidth="1px" 
            EnablePersistedSelection="True" ondatabound="GridViewPrefixSet_DataBound" 
            onrowdatabound="GridViewPrefixSet_RowDataBound" 
            onselectedindexchanged="GridViewPrefixSet_SelectedIndexChanged">
    <AlternatingRowStyle BackColor="White" />
    <Columns>
        <asp:CommandField ShowDeleteButton="false" ShowEditButton="false" 
            ShowSelectButton="True" />
        <asp:TemplateField> <%--delete--%>
                <ItemTemplate>
                    
                    <asp:LinkButton ID="LinkButton2"  runat="server" ButtonType="Button" CommandName="Delete"
                        Text="Delete" HeaderText="" CausesValidation="false" ValidationGroup="allcontrols"
                    OnClientClick='return confirm("Prefixes under this set will be deleted automatically, are you sure to delete this entry?");'> 
                    </asp:LinkButton>
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField> <%--edit--%>
                <ItemTemplate>
                    
                    <asp:LinkButton ID="LinkButtonEdit"  runat="server" ButtonType="Button" CommandName="Edit"
                        Text="Edit" HeaderText="" CausesValidation="false" ValidationGroup="allcontrols"
                    > 
                    </asp:LinkButton>
                </ItemTemplate>
            </asp:TemplateField>
        <asp:BoundField DataField="id" HeaderText="id" ReadOnly="True" 
            SortExpression="id" Visible="false" />
        <asp:BoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
        <asp:BoundField DataField="Description" HeaderText="Description" 
            SortExpression="Description" />
    </Columns>
    <FooterStyle BackColor="#CCCC99" />
    <HeaderStyle BackColor="#6B696B" Font-Bold="True" ForeColor="White" />
    <PagerStyle BackColor="#F7F6F3" ForeColor="Black" HorizontalAlign="Right" />
    <RowStyle BackColor="#F7F6F3" />
    <SelectedRowStyle BackColor="#CE5D5A" Font-Bold="True" ForeColor="White" />
    <SortedAscendingCellStyle BackColor="#FBFBF2" />
    <SortedAscendingHeaderStyle BackColor="#848384" />
    <SortedDescendingCellStyle BackColor="#EAEAD3" />
    <SortedDescendingHeaderStyle BackColor="#575357" />
    </asp:GridView>

        <asp:EntityDataSource ID="EntityDataPrefixSet" runat="server" 
        ConnectionString="name=PartnerEntities" DefaultContainerName="PartnerEntities" 
        EnableDelete="True" EnableFlattening="False" EnableInsert="True" 
        EnableUpdate="True" EntitySetName="xyzprefixsets">
        </asp:EntityDataSource>

        
        <br />
        <span style="font-weight:bold;">Browse for File to Import Prefixes in the selected Prefix Set</span>   
    <br />



    <div style="background-color:#F7F7F2;min-height:50px;">
    <asp:FileUpload id="FileUploadControl" runat="server" />
    <br />
    <asp:LinkButton runat="server" id="UploadButton" text="Import & Merge" 
                     OnClientClick="return confirm('Existing prefixes will be ignored, New prefixes will be merged with existing ones. Are you sure to continue?');" onclick="UploadButton_Click"/>
    <span style="margin-left:10px;">
    <asp:LinkButton runat="server" id="DeleteAndUpload" text="Delete All and Import" Font-Bold="true" ForeColor="Red"
        OnClientClick="return confirm('All existing prefixes will be deleted before import. Are you sure to continue?');" onclick="DeleteAndUpload_Click"/>
    </span>
                    <br />
                    <asp:Label runat="server" id="Label2" text="" ForeColor="Red" />            
                    
                    
                    <br />
                    <asp:Label runat="server" id="StatusLabel" text="" ForeColor="Red" />            

   </div>

    </div>
    <div style="float:left;width:600px;min-height:800px;text-align:left;background-color:#f2f2f2;margin-top:0px;margin-left:0px;padding-top:0px;padding-left:10px;">


        <asp:EntityDataSource ID="EntityDataPrefix" runat="server" 
            ConnectionString="name=PartnerEntities" DefaultContainerName="PartnerEntities" 
            EnableDelete="True" EnableFlattening="False" EntitySetName="xyzselecteds" 
            onquerycreated="EntityDataPrefix_QueryCreated" EnableInsert="True">
        </asp:EntityDataSource>

        <asp:EntityDataSource ID="EntityDataSourcePrefixInsert" runat="server" 
            ConnectionString="name=PartnerEntities" DefaultContainerName="PartnerEntities" 
            EnableFlattening="False" EnableInsert="True" EntitySetName="xyzselecteds">
        </asp:EntityDataSource>


        <span style="font-weight:bold;">
            <asp:Label ID="lblNoOfPrefix" runat="server" Text=""  Font-Bold="True" ForeColor="Black" ></asp:Label>
            Prefixes under selected Prefix Set
            <asp:Label ID="lblPrefixSet" runat="server" Text=""  Font-Bold="true" ForeColor="Black" ></asp:Label>
        </span>   
        <br />


        <asp:LinkButton ID="NewButton" runat="server" CausesValidation="False" 
                 Text="New Prefix" onclick="NewButton_Click1" />
        <span style="padding-left:10px;"><asp:LinkButton ID="LinkButtonDeleteAll" runat="server" CausesValidation="False" 
                Text="Delete All Prefix" onclick="DeleteButton_Click1"
                OnClientClick="return confirm('All prefixes under this Prefix Set will be deleted, Are you sure to continue?');" /></span>
        

        <asp:Label ID="lblPrefixValidate" ForeColor="Red" runat="server" Text=""></asp:Label>

        <asp:FormView ID="FormViewPrefixInsert" runat="server" DataKeyNames="id" 
            DataSourceID="" 
            oniteminserting="FormViewPrefixInsert_ItemInserting" 
            onmodechanging="FormViewPrefixInsert_ModeChanging">
            
            <InsertItemTemplate>
                <div style="text-align:right;">
                prefix:
                <asp:TextBox ID="prefixTextBox" runat="server" Text="" />
                <br />
                </div>
                <asp:LinkButton ID="InsertButton" runat="server" CausesValidation="True" 
                    CommandName="Insert" Text="Insert" />
                &nbsp;<asp:LinkButton ID="InsertCancelButton" runat="server" 
                    CausesValidation="False" CommandName="Cancel" Text="Cancel" OnClick="PrefixInsertCancel_Click" />
            </InsertItemTemplate>
            <EditItemTemplate>
            </EditItemTemplate>
        </asp:FormView>
       




        <asp:ListView ID="ListView1" runat="server" DataKeyNames="id" 
            DataSourceID="EntityDataPrefix" GroupItemCount="10" 
            onitemdeleted="ListView1_ItemDeleted" 
            onpagepropertieschanged="ListView1_PagePropertiesChanged">
            <AlternatingItemTemplate>
                <td runat="server" style="background-color:#FFF8DC;">
                    <%--id:
                    <asp:Label ID="idLabel" runat="server" Text='<%# Eval("id") %>' />
                    <br />--%>
                    <asp:Label ID="prefixLabel" runat="server" Text='<%# Eval("prefix") %>' />
                    <br /><%--PrefixSet:
                    <asp:Label ID="PrefixSetLabel" runat="server" Text='<%# Eval("PrefixSet") %>' />
                    <br />--%>
                    <asp:LinkButton ID="DeleteButton" runat="server" CommandName="Delete" 
                        Text="Delete" />
                    <br />
                </td>
            </AlternatingItemTemplate>
            
            <EditItemTemplate>
                <td runat="server" style="background-color:#008A8C;color: #FFFFFF;">
                    id:
                    <asp:Label ID="idLabel1" runat="server" Text='<%# Eval("id") %>' />
                    <br />
                    prefix:
                    <asp:TextBox ID="prefixTextBox" runat="server" Text='<%# Bind("prefix") %>' />
                    <br />
                    PrefixSet:
                    <asp:TextBox ID="PrefixSetTextBox" runat="server" 
                        Text='<%# Bind("PrefixSet") %>' />
                    <br />
                    <asp:Button ID="UpdateButton" runat="server" CommandName="Update" 
                        Text="Update" />
                    <br />
                    <asp:Button ID="CancelButton" runat="server" CommandName="Cancel" 
                        Text="Cancel" />
                    <br />
                </td>
            </EditItemTemplate>
            
            <EmptyDataTemplate>
                <table runat="server" 
                    style="background-color: #FFFFFF;border-collapse: collapse;border-color: #999999;border-style:none;border-width:1px;">
                    <tr>
                        <td>
                        No Data.
                           </td>
                    </tr>
                </table>
            </EmptyDataTemplate>
            <EmptyItemTemplate>
<td runat="server" />
            </EmptyItemTemplate>
            <GroupTemplate>
                <tr ID="itemPlaceholderContainer" runat="server">
                    <td ID="itemPlaceholder" runat="server">
                    </td>
                </tr>
            </GroupTemplate>
            <InsertItemTemplate>
                <td runat="server" style="">
                    
                    prefix:
                    <asp:TextBox ID="prefixTextBox" runat="server" Text='<%# Bind("prefix") %>' />
                    <br />
                    PrefixSet:
                    <asp:TextBox ID="PrefixSetTextBox" runat="server" 
                        Text='<%# Bind("PrefixSet") %>' />
                    <br />
                    <asp:Button ID="InsertButton" runat="server" CommandName="Insert" 
                        Text="Insert" />
                    <br />
                    <asp:Button ID="CancelButton" runat="server" CommandName="Cancel" 
                        Text="Clear" />
                    <br />
                </td>
            </InsertItemTemplate>
            <ItemTemplate>
                <td runat="server" style="background-color:#DCDCDC;color: #000000;">
                    <%--id:
                    <asp:Label ID="idLabel" runat="server" Text='<%# Eval("id") %>' />
                    <br />--%>
                    <asp:Label ID="prefixLabel" runat="server" Text='<%# Eval("prefix") %>' />
                    <br /><%--PrefixSet:
                    <asp:Label ID="PrefixSetLabel" runat="server" Text='<%# Eval("PrefixSet") %>' />
                    <br />--%>
                    <asp:LinkButton ID="DeleteButton" runat="server" CommandName="Delete" 
                        Text="Delete" />
                    <br />
                </td>
            </ItemTemplate>
            <LayoutTemplate>
                <table runat="server">
                    <tr runat="server">
                        <td runat="server">
                            <table ID="groupPlaceholderContainer" runat="server" border="1" 
                                style="background-color: #FFFFFF;border-collapse: collapse;border-color: #999999;border-style:none;border-width:1px;font-family: Verdana, Arial, Helvetica, sans-serif;">
                                <tr ID="groupPlaceholder" runat="server">
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr runat="server">
                        <td runat="server" 
                            style="text-align: center;background-color: #CCCCCC;font-family: Verdana, Arial, Helvetica, sans-serif;color: #000000;">
                            <asp:DataPager ID="DataPager1" runat="server" PageSize="100">
                                <Fields>
                                    <asp:NextPreviousPagerField ButtonType="Button" ShowFirstPageButton="True" 
                                        ShowNextPageButton="False" ShowPreviousPageButton="False" />
                                    <asp:NumericPagerField />
                                    <asp:NextPreviousPagerField ButtonType="Button" ShowLastPageButton="True" 
                                        ShowNextPageButton="False" ShowPreviousPageButton="False" />
                                </Fields>
                            </asp:DataPager>
                        </td>
                    </tr>
                </table>
            </LayoutTemplate>
            <SelectedItemTemplate>
                <td runat="server" 
                    style="background-color:Teal;font-weight: bold;color:White;">
                    id:
                    <asp:Label ID="idLabel" runat="server" Text='<%# Eval("id") %>' />
                    <br />prefix:
                    <asp:Label ID="prefixLabel" runat="server" Text='<%# Eval("prefix") %>' />
                    <br />PrefixSet:
                    <asp:Label ID="PrefixSetLabel" runat="server" Text='<%# Eval("PrefixSet") %>' />
                    <br />
                    <asp:Button ID="DeleteButton" runat="server" CommandName="Delete" 
                        Text="Delete" />
                    <br />
                </td>
            </SelectedItemTemplate>
        </asp:ListView>



    
   
  <script runat="server">
    protected void Button1_Click(object sender, EventArgs e)
    {
        // Introducing delay for demonstration.
        //System.Threading.Thread.Sleep(3000);
        
    }
</script>


    <br />
    
</div>
</div>

    

</asp:Content>

