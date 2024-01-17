<%@ Page Title="" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeBehind="DailyInput.aspx.cs" Inherits="PortalApp.ICX_Reports.Cas_ICX.WebForm1" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    
    <div style="background-color:#edf2ef;color: black;float:left;border: 1px solid #707070; padding:10px; padding-right: 613px;">
        <div style=" font-weight: bold; padding-bottom:5px; ">
                <asp:Label ID="lblSelectOption" runat="server" Text="Select Year and Month:"></asp:Label>
        </div>
          
       
        <div>
            <asp:DropDownList ID="DropDownYear" runat="server"></asp:DropDownList>
            <asp:DropDownList ID="DropDownMonth" runat="server" AutoPostBack="True" OnSelectedIndexChanged="ddlMonth_SelectedIndexChanged"></asp:DropDownList>
        </div>
        
    </div>

    <div style="height:20px;clear:both;"></div>



    <div style="margin-top = 20px">
        
    <asp:GridView ID="GridView2" runat="server" AllowPaging="false" 
        AutoGenerateColumns="False" ShowHeaderWhenEmpty="true"  CellPadding="4" ForeColor="#333333" 
        GridLines="Vertical" 
        ShowHeader ="true"
        onrowediting="GridViewRowEditing" style="margin-left: 0px"
        onrowcancelingedit="GridViewRowCancelingEdit" 
        onrowupdating="GridViewRowUpdating" 
        onrowdeleting="GridViewRowSubmitting" 
         onrowdatabound="GridViewSupplierRates_RowDataBound" 
        font-size="9pt" 
         BorderColor="#CCCCCC" BorderStyle="Solid"
        >
        <AlternatingRowStyle BackColor="#f2f2f2" ForeColor="#284775" />
        <Columns>

             <asp:TemplateField HeaderText="Action">
                 <ItemStyle HorizontalAlign="Center" />
                <ItemTemplate>
                    <asp:LinkButton ID="LinkButtonEdit" CommandName="Edit"  runat="server">Edit</asp:LinkButton>
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:LinkButton ID="LinkButtonEdit"  CommandName="Update"  runat="server" CausesValidation="false" ValidationGroup="allcontrols">Update</asp:LinkButton>
                    <asp:LinkButton ID="LinkButtonCancel"  CommandName="Cancel"  runat="server">Cancel</asp:LinkButton>
                </EditItemTemplate>
            </asp:TemplateField>

           

                
            <asp:TemplateField HeaderText="Date">
                <ItemTemplate>
                    <asp:Label ID="lblDate" runat="server" Text='<%# Eval("callDateICX") %>' />
                </ItemTemplate>
                <ItemStyle HorizontalAlign="Center" />
            </asp:TemplateField>







            <asp:TemplateField HeaderText="CAS">
                <ItemTemplate>
                    <asp:Label ID="lblCasDomestic" runat="server" Text='<%# Eval("DomesticDurationCalc" , "{0:F2}") %>' DataFormatString="{0:N2}" />
                </ItemTemplate>
                <ItemStyle HorizontalAlign="Center" />
            </asp:TemplateField>


            <asp:TemplateField HeaderText="ICX" ItemStyle-Width="50px">
                <ItemTemplate>
                    <asp:Label ID="ICXDomInput" runat="server" Text='<%# Eval("DomesticICX", "{0:F2}") %>'  ></asp:Label>
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:TextBox ID="ICXDomInput" runat="server" style="width: 50px;" Text='<%# Eval("DomesticICX", "{0:F2}") %>'>  </asp:TextBox>
                </EditItemTemplate>
            </asp:TemplateField>


            <asp:TemplateField HeaderText="Difference">
                <ItemTemplate>  <%# Eval("DomesticDurationCalc") != null && Eval("DomesticICX") != null ?
                                    String.Format("{0:F2}", (decimal)Eval("DomesticDurationCalc") - (decimal)Eval("DomesticICX")) : "N/A"  %> </ItemTemplate>            
            </asp:TemplateField>
            
           





            <asp:TemplateField HeaderText="CAS">
                <ItemTemplate>
                    <asp:Label ID="lblCasIntIn" runat="server" Text='<%# Eval("IntlInDurationCalc", "{0:F2}") %>' />
                </ItemTemplate>
                <ItemStyle HorizontalAlign="Center" />
            </asp:TemplateField>


            <asp:TemplateField HeaderText="ICX" ItemStyle-Width="50px">
                <ItemTemplate>
                    <asp:Label ID="ICXIntInInput" runat="server" Text='<%# Eval("IntInICX", "{0:F2}") %>'  ></asp:Label>
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:TextBox ID="ICXIntInInput" runat="server" style="width: 50px;" Text='<%# Eval("IntInICX", "{0:F2}") %>' >  </asp:TextBox>
                </EditItemTemplate>
            </asp:TemplateField>


            <asp:TemplateField HeaderText="Difference">
                <ItemTemplate>  <%#  Eval("IntlInDurationCalc") != null && Eval("IntInICX") != null ?
                                     String.Format("{0:F2}", (decimal)Eval("IntlInDurationCalc") - (decimal)Eval("IntInICX")) : "N/A" %> </ItemTemplate>    
            </asp:TemplateField>

            





            <asp:TemplateField HeaderText="CAS">
                <ItemTemplate>
                    <asp:Label ID="lblCasIntOut" runat="server" Text='<%# Eval("IntlOutDurationCalc", "{0:F2}") %>' />
                </ItemTemplate>
                <ItemStyle HorizontalAlign="Center" />
            </asp:TemplateField>


            <asp:TemplateField HeaderText="ICX" ItemStyle-Width="50px">
                <ItemTemplate>
                    <asp:Label ID="ICXIntOutInput" runat="server" Text='<%# Eval("IntOutICX", "{0:F2}") %>' ></asp:Label>
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:TextBox ID="ICXIntOutInput" runat="server" style="width: 50px;" Text='<%# Eval("IntOutICX", "{0:F2}") %>'>  </asp:TextBox>
                </EditItemTemplate>
            </asp:TemplateField>


            <asp:TemplateField HeaderText="Difference">
                <ItemTemplate>  <%#  Eval("IntlOutDurationCalc") != null && Eval("IntOutICX") != null ?
                                          String.Format("{0:F2}", (decimal)Eval("IntlOutDurationCalc") - (decimal)Eval("IntOutICX")) : "N/A" %> </ItemTemplate>    
            </asp:TemplateField>

            

               <asp:TemplateField>
                   <ItemStyle HorizontalAlign="Center" />
                <ItemTemplate>
                    <asp:LinkButton ID="LinkButtonSubmit" 
                    OnClientClick='return confirm("Are you sure to submit billed minutes for this day? Data cannot be changed after that. If you need to amend incorrect submission, please contact TB support.");'
                    CommandName="Delete"  runat="server" >Submit</asp:LinkButton>
                    
                </ItemTemplate>
                <EditItemTemplate>
                    
                </EditItemTemplate>
            </asp:TemplateField>
        </Columns>

        <EditRowStyle BackColor="#999999" />
        <FooterStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
        <HeaderStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
        <PagerStyle BackColor="#284775" ForeColor="White" 
            HorizontalAlign="Left" />
        <RowStyle BackColor="white" Width="5px" ForeColor="#333333" />
        <AlternatingRowStyle Width="5px" />
        <SelectedRowStyle BackColor="#E2DED6" ForeColor="#333333" Font-Bold="True" />
        <SortedAscendingCellStyle BackColor="#E9E7E2" />
        <SortedAscendingHeaderStyle BackColor="#506C8C" />
        <SortedDescendingCellStyle BackColor="#FFFDF8" />
        <SortedDescendingHeaderStyle BackColor="#6F8DAE" />
    </asp:GridView>

    
    </div>

</asp:Content>
