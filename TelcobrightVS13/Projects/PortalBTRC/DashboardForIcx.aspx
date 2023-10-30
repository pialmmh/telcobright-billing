<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="True" CodeBehind="DashboardForIcx.aspx.cs" Inherits="DashboardAspxForIcx" %>

<%@ Register Assembly="System.Web.DataVisualization, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" Namespace="System.Web.UI.DataVisualization.Charting" TagPrefix="asp" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <div class="jumbotron">
        <h1>
            <asp:Label ID="lblCustomerDisplayName" runat="server" Text=""></asp:Label></h1>

    </div>
<script type="text/javascript">
    // Get a reference to the DropDownList control
   <%-- var ddl = document.getElementById('<%= GridView11.FindControl("DropDownList1") %>');

    // Attach an event listener to the DropDownList's change event
    ddl.addEventListener('change', function () {
        // Get the selected index
        var selectedIndex = ddl.selectedIndex;

        // Check if the selected index is not the "Select" item
        if (selectedIndex !== 0) {
            // Change the selected index to the currently selected item
            ddl.selectedIndex = selectedIndex;
        }
    });--%>
</script>

    <%--<asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>--%>
<ajaxToolkit:ToolkitScriptManager runat="Server" EnableScriptGlobalization="true"
                                  EnableScriptLocalization="true" ID="ToolkitScriptManager1" />
    <div style="text-align: left; min-width: 1000px;">
        <div style="float: left">

            <asp:UpdatePanel ID="UpdatePanel3" runat="server">
                <Triggers>
                    <asp:AsyncPostBackTrigger ControlID="Timer3" EventName="Tick" />
                </Triggers>
                <ContentTemplate>
                    <div>
                        <h2 style="visibility: hidden">International Incoming</h2>
                        <asp:HyperLink ID="HyperLinkIntlIn" runat="server" Target="_blank" NavigateUrl="~/reports/InternationalIn.aspx" ForeColor="#08605c"></asp:HyperLink>
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
                    <div style="float: left;">
                    </div>
                    <p></p>
                    </div>
                </ContentTemplate>
            </asp:UpdatePanel>

            <asp:Timer ID="Timer3" runat="server" Interval="30000" OnTick="Timer3_Tick" Enabled="false">
            </asp:Timer>

            <div style="padding-left: 20px">
                <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                    <Triggers>
                        <asp:AsyncPostBackTrigger ControlID="Timer1" EventName="Tick" />
                    </Triggers>

                    <ContentTemplate>
                        <h2 style="text-align: left">
                            <asp:HyperLink ID="HyperLinkError" runat="server" NavigateUrl="~/reports/CdrError.aspx" Target="_blank" ForeColor="#08605c" Style="text-align: left;"></asp:HyperLink>

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

            <div style="height: 40px;"></div>


            <%--<asp:UpdatePanel ID="UpdatePanel2" runat="server" style ="min-width: 800px;">--%>
            <%--<Triggers>
                    <asp:AsyncPostBackTrigger ControlID="Timer2" EventName="Tick" />
                </Triggers>--%>
            <%--<ContentTemplate>--%>


            <div style="text-align: center; float: left;">
                <div style="float: left;">
                    <%--main div 1--%>
                    <div>
                        <%--div 1--%>
                        <div style="float: left;">
                            <div style="margin-right: auto; text-align: left;">
                                <asp:Label ID="Label2" runat="server" Text="Latest CDR Job Process Status" Font-Bold="true" Font-Size="Large" ForeColor="#08605c" CssClass="StrongText"></asp:Label>
                            </div>
                            <asp:GridView ID="GridViewCompleted" runat="server" AutoGenerateColumns="False" CellPadding="4" DataKeyNames="id" ForeColor="#333333" GridLines="None" BorderStyle="None" BorderWidth="1" OnPageIndexChanging="GridViewCompleted_PageIndexChanging">
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
                            <asp:Button ID="PreviousButton" runat="server" Text="Newer" OnClick="PreviousButton_Click" AutoPostBack="True" />
                            <asp:Button ID="NextButton" runat="server" Text="Older" OnClick="NextButton_Click" AutoPostBack="True" />

                        </div>
                    </div>

                </div>
            </div>

            <%-- Missing TG --%>
            <div>
                <%--div 1--%>
                
                <div style="float: left; padding-left: 200px">
                    <div style="margin-right: auto; text-align: left;">
                        <asp:Label ID="Label11" runat="server" Text="Missing Tg's" Font-Bold="true" Font-Size="Large" ForeColor="#08605c" CssClass="StrongText"></asp:Label>
                    </div>

                    <%--<asp:UpdatePanel ID="UpdatePanelFormissingTg" runat="server">--%>
                        <%--<Triggers>
                            <asp:PostBackTrigger ControlID="ButtonCreateJob" />
                        </Triggers>--%>


                        <%--<ContentTemplate>--%>
                            
                            <asp:GridView ID="GridView11" runat="server" AutoGenerateColumns="False" CellPadding="4" DataKeyNames="id" ForeColor="#333333" GridLines="None" BorderStyle="None" BorderWidth="1" OnRowDataBound="GridView_RowDataBound">
                                <AlternatingRowStyle BackColor="White" ForeColor="#284775" />
                                <Columns>
                                    <asp:BoundField DataField="id" HeaderText="id" InsertVisible="False" ReadOnly="True" SortExpression="id" Visible="false" />
                                    <asp:BoundField DataField="TgName" HeaderText="TgName" SortExpression="TgName" />
                                    <asp:BoundField DataField="SwitchName" HeaderText="SwitchName" SortExpression="SwitchName" />
                                    <asp:TemplateField HeaderText="Zone">
                                        <ItemTemplate>
                                            <div style="text-align: center;">
                                                <asp:DropDownList ID="DropDownList1" runat="server"  AutoPostBack="False"
                                                                OnSelectedIndexChanged="DropDownListOfZone_SelectedIndexChanged"
                                                                  Enabled="True">
                                                    <asp:ListItem  Value="Select">Select</asp:ListItem>
                                                    <asp:ListItem  Value="Dhaka">Dhaka</asp:ListItem>
                                                    <asp:ListItem  Value="Sylhet" >Sylhet</asp:ListItem>
                                                    <asp:ListItem  Value="Khulna">Khulna</asp:ListItem>
                                                    <asp:ListItem  Value="Bogra">Bogra</asp:ListItem>
                                                    <asp:ListItem  Value="Chittagong">Chittagong</asp:ListItem>
                                                    <asp:ListItem  Value="Rajshahi" >Rajshahi</asp:ListItem>
                                                </asp:DropDownList>
                                            </div>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Partners">
                                        <ItemTemplate>
                                            <div style="text-align: center;">
                                                <asp:DropDownList ID="DropDownListPartner" runat="server" AutoPostBack="False" 
                                                                  OnSelectedIndexChanged="DropDownListPartner_OnSelectedIndexChanged">
                                                </asp:DropDownList>
                                            </div>
                                        </ItemTemplate>
                                    </asp:TemplateField>

                                    <asp:TemplateField HeaderText="Action">
                                        <ItemTemplate>
                                            <div style="text-align: center;">
                                                <asp:LinkButton ID="button1" Text="Assign" runat="server" OnClick="TgAssignment"  />
                                            </div>
                                        </ItemTemplate>
                                    </asp:TemplateField>

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
                            <p>
                                <asp:Label ID="Msg" runat="server" Text="" ForeColor="Red"/>
                            </p>
                        <%--</ContentTemplate>--%>
                    <%--</asp:UpdatePanel>--%>


                    

                </div>
            </div>
        </div>
        <%--TGs --%>
        <div>
            <%--div 1--%>
            <div style="float: left; padding-left: 200px">
                <div style="margin-right: auto; text-align: left;">
                    <asp:Label ID="LabelTgs" runat="server" Text="Tg List" Font-Bold="true" Font-Size="Large" ForeColor="#08605c" CssClass="StrongText"></asp:Label>
                </div>
                <asp:ListView ID="ListViewTgs" runat="server" DataKeyNames="id">
                    <LayoutTemplate>
                        <table border="1" style="width: 100%;">
                            <tr style="background-color: #086052; color: white; font-weight: bold;">
                                <th>Tg</th>
                                <th>SwitchName</th>
                                <th>Zone</th>
                                <th>Partners</th>
                            </tr>
                            <tr runat="server" id="itemPlaceholder"></tr>
                        </table>
                    </LayoutTemplate>
                    <ItemTemplate>
                        <tr>
                            <td>
                                <asp:Label runat="server" Text='<%# Eval("TgName") %>' />
                            </td>
                            <td>
                                <asp:Label runat="server" Text='<%# Eval("SwitchName") %>' />
                            </td>
                            <td>
                                <asp:Label runat="server" Text='<%# Eval("Zone") %>' />
                            </td>
                            <td>
                                <asp:Label runat="server" Text='<%# Eval("Partner") %>' />
                            </td>
                        </tr>
                    </ItemTemplate>
                </asp:ListView>
            </div>
            <%--</ContentTemplate>--%>
            <%--</asp:UpdatePanel>--%>
        </div>
    </div>
</asp:Content>
