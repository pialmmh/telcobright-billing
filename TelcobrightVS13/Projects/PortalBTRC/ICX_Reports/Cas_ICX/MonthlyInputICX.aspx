<%@ Page Title="" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeBehind="MonthlyInputICX.aspx.cs" Inherits="PortalApp.ICX_Reports.Cas_ICX.MonthlyInputICX" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    
    <div style="background-color:#edf2ef; color: black; display: flex; align-items: flex-start; border: 1px solid #707070; padding:10px;">
        
       
          
       
        <div style=" font-weight: bold; padding-bottom:5px;">
            <asp:Label ID="Label1" runat="server"  Text="Select Year & Month:"></asp:Label> <br>
            <asp:DropDownList ID="DropDownYear" runat="server" AutoPostBack="True" OnSelectedIndexChanged="ddlMonth_SelectedIndexChanged"></asp:DropDownList>
            <asp:DropDownList ID="DropDownMonth" runat="server" AutoPostBack="True" OnSelectedIndexChanged="ddlMonth_SelectedIndexChanged"></asp:DropDownList>
        </div>
        
    </div>

    <div style="height:20px;clear:both;"></div>

    <div style="display: flex; background-color: #086052; width:1188px">
        <div style=" padding: 5px; margin-left: 206px;border: 1px solid #ccc; padding-left:135px; padding-right:137px; color: white;">Domestic</div>
        <div style=" padding: 5px; border: 1px solid #ccc; width: 315px; text-align: center; color: white;">International Incoming</div>
        <div style=" padding: 5px; border: 1px solid #ccc; width: 315px; text-align: center; color: white;">International Outgoing</div>
    </div>

    <div >
        
    <asp:GridView ID="GridView2" runat="server" AllowPaging="false" 
        AutoGenerateColumns="False" ShowHeaderWhenEmpty="true"  CellPadding="4" ForeColor="#086052" ShowFooter="true"
        GridLines="Vertical" 
        ShowHeader ="true"
        
        
         onrowdatabound="GridViewSupplierRates_RowDataBound" 
        font-size="9pt" 
         BorderColor="#CCCCCC" BorderStyle="Solid"
        >
        <AlternatingRowStyle BackColor="#f2f2f2" ForeColor="#284775" />
        <Columns>

             

           

                
            <asp:TemplateField HeaderText="ICX" ItemStyle-Width="197px">
                <ItemTemplate>
                    <asp:Label ID="lblDate" runat="server" Text='<%# Eval("callDateCalc") %>' />
                </ItemTemplate>
                <ItemStyle HorizontalAlign="Center" />
            </asp:TemplateField>




            <asp:TemplateField HeaderText="CAS" ItemStyle-Width="100px">
                <ItemTemplate>
                    <asp:Label ID="lblCasDomestic" runat="server" Text='<%# Eval("DomesticDurationCalc" , "{0:F2}") %>' DataFormatString="{0:N2}" />
                </ItemTemplate>
                <ItemStyle HorizontalAlign="Center" />
            </asp:TemplateField>


            <asp:TemplateField HeaderText="ICX" ItemStyle-Width="100px">
                <ItemStyle HorizontalAlign="Center" />
                <ItemTemplate>
                    <asp:Label ID="ICXDomInput" runat="server" Text='<%# Eval("DomesticICX", "{0:F2}") %>'  ></asp:Label>
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:TextBox ID="ICXDomInput" runat="server" style="width: 100px;" Text='<%# Eval("DomesticICX", "{0:F2}") %>'>  </asp:TextBox>
                </EditItemTemplate>
            </asp:TemplateField>


            <asp:TemplateField HeaderText="Difference" ItemStyle-Width="100px">
                <ItemTemplate>  <%# Eval("DomesticDurationCalc") != null && Eval("DomesticICX") != null ?
                                    String.Format("{0:F2}", (decimal)Eval("DomesticDurationCalc") - (decimal)Eval("DomesticICX")) : "N/A"  %> </ItemTemplate>            
                <ItemStyle HorizontalAlign="Center" />
            </asp:TemplateField>
            
           





            <asp:TemplateField HeaderText="CAS" ItemStyle-Width="100px">
                <ItemTemplate>
                    <asp:Label ID="lblCasIntIn" runat="server" Text='<%# Eval("IntlInDurationCalc", "{0:F2}") %>' />
                </ItemTemplate>
                <ItemStyle HorizontalAlign="Center" />
            </asp:TemplateField>


            <asp:TemplateField HeaderText="ICX" ItemStyle-Width="100px">
                <ItemStyle HorizontalAlign="Center" />
                <ItemTemplate>
                    <asp:Label ID="ICXIntInInput" runat="server" Text='<%# Eval("IntInICX", "{0:F2}") %>'  ></asp:Label>
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:TextBox ID="ICXIntInInput" runat="server" style="width: 100px;" Text='<%# Eval("IntInICX", "{0:F2}") %>' >  </asp:TextBox>
                </EditItemTemplate>
            </asp:TemplateField>


            <asp:TemplateField HeaderText="Difference" ItemStyle-Width="100px">
                <ItemTemplate>  <%#  Eval("IntlInDurationCalc") != null && Eval("IntInICX") != null ?
                                     String.Format("{0:F2}", (decimal)Eval("IntlInDurationCalc") - (decimal)Eval("IntInICX")) : "N/A" %> </ItemTemplate>    
                <ItemStyle HorizontalAlign="Center" />
            </asp:TemplateField>

            





            <asp:TemplateField HeaderText="CAS" ItemStyle-Width="100px">
                <ItemTemplate>
                    <asp:Label ID="lblCasIntOut" runat="server" Text='<%# Eval("IntlOutDurationCalc", "{0:F2}") %>' />
                </ItemTemplate>
                <ItemStyle HorizontalAlign="Center" />
            </asp:TemplateField>


            <asp:TemplateField HeaderText="ICX" ItemStyle-Width="100px">
                
                <ItemTemplate>
                    <asp:Label ID="ICXIntOutInput" runat="server" Text='<%# Eval("IntOutICX", "{0:F2}") %>' ></asp:Label>
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:TextBox ID="ICXIntOutInput" runat="server" style="width: 100px;" Text='<%# Eval("IntOutICX", "{0:F2}") %>'>  </asp:TextBox>
                </EditItemTemplate>
                <ItemStyle HorizontalAlign="Center" />
            </asp:TemplateField>


            <asp:TemplateField HeaderText="Difference" ItemStyle-Width="100px">
                <ItemStyle HorizontalAlign="Center" />
                <ItemTemplate>  <%#  Eval("IntlOutDurationCalc") != null && Eval("IntOutICX") != null ?
                                          String.Format("{0:F2}", (decimal)Eval("IntlOutDurationCalc") - (decimal)Eval("IntOutICX")) : "N/A" %> </ItemTemplate>    
            </asp:TemplateField>

            

              
        </Columns>
        <FooterStyle HorizontalAlign="Center" />

        <EditRowStyle BackColor="#999999" />
        <FooterStyle BackColor="#086052" Font-Bold="True" ForeColor="White" />
        <HeaderStyle BackColor="#086052" Font-Bold="True" ForeColor="White" />
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
