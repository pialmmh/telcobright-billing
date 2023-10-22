<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="True" CodeBehind="DashboardForIcx.aspx.cs" Inherits="DashboardAspxForIcx" %>

<%@ Register Assembly="System.Web.DataVisualization, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" Namespace="System.Web.UI.DataVisualization.Charting" TagPrefix="asp" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    
    <div class="jumbotron">
        <h1>
            <asp:Label ID="lblCustomerDisplayName" runat="server" Text=""></asp:Label></h1>
        <p class="lead">CDR Analyzer System (CAS)</p>

    </div>


    <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
    <div style="text-align: left;min-width:1000px;padding-left:50px;">
        <div style="float: left">
            
                <asp:UpdatePanel ID="UpdatePanel3" runat="server">
                    <Triggers>
                        <asp:AsyncPostBackTrigger ControlID="Timer3" EventName="Tick"/>
                    </Triggers>
                    <ContentTemplate>
                       <div>
                         <h2 style="visibility: hidden">International Incoming</h2>
                            <asp:HyperLink ID="HyperLinkIntlIn" runat="server" Target="_blank" NavigateUrl="~/reports/InternationalIn.aspx" ForeColor="#08605c" ></asp:HyperLink>
                            </h2>
                        <p></p>
                       </div>
                        


                        <div style="float: left;">
                            <asp:GridView ID="GridViewIntlin" runat="server" CellPadding="4" ForeColor="#333333" GridLines="None">
                                <AlternatingRowStyle BackColor="White" ForeColor="#284775" />
                                <EditRowStyle BackColor="#999999" />
                                <FooterStyle BackColor="#086052" Font-Bold="True" ForeColor="White" />
                                <HeaderStyle BackColor="#086052" Font-Bold="True" ForeColor="White" />
                                <PagerStyle BackColor="#284775" ForeColor="White" HorizontalAlign="Center" />
                                <RowStyle BackColor="#F7F6F3" ForeColor="#333333" />
                                <SelectedRowStyle BackColor="#E2DED6" Font-Bold="True" ForeColor="#333333" />
                                <SortedAscendingCellStyle BackColor="#E9E7E2" />
                                <SortedAscendingHeaderStyle BackColor="#506C8C" />
                                <SortedDescendingCellStyle BackColor="#FFFDF8" />
                                <SortedDescendingHeaderStyle BackColor="#6F8DAE" />
                            </asp:GridView>
                            <asp:SqlDataSource ID="SqlDataSource3" runat="server"></asp:SqlDataSource>
                        </div>
                        <div style="float: left; ">
                        </div>
                        <p></p>
                        </div>
                    </ContentTemplate>
                </asp:UpdatePanel>
            
            <asp:Timer ID="Timer3" runat="server" Interval="30000" OnTick="Timer3_Tick" Enabled="false">
            </asp:Timer>
           
            <div>
                <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                    <Triggers>
                        <asp:AsyncPostBackTrigger ControlID="Timer1" EventName="Tick" />
                    </Triggers>

                    <ContentTemplate>
                        <h2>
                        
                            <asp:HyperLink ID="HyperLinkError" runat="server" NavigateUrl="~/reports/CdrError.aspx" Target="_blank" ForeColor="#08605c" ></asp:HyperLink>

                        </h2>
                        <p>
                            <asp:GridView ID="GridViewError" runat="server" CellPadding="4" ForeColor="#333333" GridLines="None">
                                <AlternatingRowStyle BackColor="White" ForeColor="#284775" />
                                <EditRowStyle BackColor="#999999" />
                                <FooterStyle BackColor="#086052" Font-Bold="True" ForeColor="White" />
                                <HeaderStyle BackColor="#086052" Font-Bold="True" ForeColor="White" />
                                <PagerStyle BackColor="#284775" ForeColor="White" HorizontalAlign="Center" />
                                <RowStyle BackColor="#F7F6F3" ForeColor="#333333" />
                                <SelectedRowStyle BackColor="#E2DED6" Font-Bold="True" ForeColor="#333333" />
                                <SortedAscendingCellStyle BackColor="#E9E7E2" />
                                <SortedAscendingHeaderStyle BackColor="#506C8C" />
                                <SortedDescendingCellStyle BackColor="#FFFDF8" />
                                <SortedDescendingHeaderStyle BackColor="#6F8DAE" />
                            </asp:GridView>

                        </p>
                    </ContentTemplate>


                </asp:UpdatePanel>
                <asp:Timer ID="Timer1" runat="server" Interval="30000" OnTick="Timer1_Tick" Enabled="false">
                </asp:Timer>
            </div>
            
            <p></p>
        </div>
        <div style="padding-left: 20px;">

            <div style="height:40px;"></div>

            
               <asp:UpdatePanel ID="UpdatePanel2" runat="server" style ="min-width: 1800px;">
                <Triggers>
                    <asp:AsyncPostBackTrigger ControlID="Timer2" EventName="Tick" />
                </Triggers>
                <ContentTemplate>
                    
 
                    <div style="text-align: center; float: left;">
                        <div style="float: left;">
                            <%--main div 1--%>
                            <div>
                                <%--div 1--%>
                                <div style="float: left;">
                                    <div style="margin-left: auto; margin-right: auto; text-align: center;">
                                        <asp:Label ID="Label2" runat="server" Text="CDR File Receiving Status By ICX (Current Month)" Font-Bold="true" Font-Size="Large" ForeColor="#08605c" CssClass="StrongText"></asp:Label>
                                    </div>
                                    <asp:GridView ID="GridViewCompleted" runat="server" AutoGenerateColumns="False" CellPadding="4" DataKeyNames="id" ForeColor="#333333" GridLines="None" BorderStyle="None" BorderWidth="1"  >
                                        <AlternatingRowStyle BackColor="White" ForeColor="#284775" />
                                        <Columns>
                                            <asp:TemplateField HeaderText="Status">
                                                <ItemTemplate>
                                                    <div style="text-align: center;">
                                                        <asp:Image ID="StatusImage" runat="server" ImageUrl="https://i.postimg.cc/Rh0G70KG/5610944.png" Width="14" Height="14" />
                                                    </div>
                                                </ItemTemplate>
                                            </asp:TemplateField> 
                                            <asp:BoundField DataField="id" HeaderText="id" InsertVisible="False" ReadOnly="True" SortExpression="id" Visible="false" />
                                            <asp:BoundField DataField="JobName" HeaderText="JobName" SortExpression="JobName" />
                                            <asp:BoundField DataField="CreationTime" HeaderText="CreationTime" SortExpression="CreationTime" DataFormatString="{0:yyyy-MM-dd HH:mm:ss}" />
                                            <asp:BoundField DataField="CompletionTime" HeaderText="CompletionTime" SortExpression="CompletionTime" DataFormatString="{0:yyyy-MM-dd HH:mm:ss}" />
                                            
                                        </Columns>
                                        <EditRowStyle BackColor="#999999" />
                                        <FooterStyle BackColor="#086052" Font-Bold="True" ForeColor="White" />
                                        <HeaderStyle BackColor="#086052" Font-Bold="True" ForeColor="White" />
                                        <PagerStyle BackColor="#284775" ForeColor="White" HorizontalAlign="Center" />
                                        <RowStyle BackColor="#F7F6F3" ForeColor="#333333" />
                                        <SelectedRowStyle BackColor="#E2DED6" Font-Bold="True" ForeColor="#333333" />
                                        <SortedAscendingCellStyle BackColor="#E9E7E2" />
                                        <SortedAscendingHeaderStyle BackColor="#506C8C" />
                                        <SortedDescendingCellStyle BackColor="#FFFDF8" />
                                        <SortedDescendingHeaderStyle BackColor="#6F8DAE" />
                                    </asp:GridView>    
                                </div>
                                    
                            </div>
                        </div>

                    </div>  

                </ContentTemplate>

            </asp:UpdatePanel>

            <asp:Timer ID="Timer2" runat="server" Interval="30000" OnTick="Timer2_Tick" Enabled="True">
            </asp:Timer>
            

        </div>

    </div>
</asp:Content>
