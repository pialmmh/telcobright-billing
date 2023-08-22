<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="True" CodeBehind="Dashboard.aspx.cs" Inherits="DashboardAspx" %>

<%@ Register Assembly="System.Web.DataVisualization, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" Namespace="System.Web.UI.DataVisualization.Charting" TagPrefix="asp" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">


    <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
    <div style="text-align: left;min-width:1000px;padding-left:50px; margin-top: -80px">
        <div style="float: left; visibility: hidden">
            
                <asp:UpdatePanel ID="UpdatePanel3" runat="server">
                    <Triggers>
                        <asp:AsyncPostBackTrigger ControlID="Timer3" EventName="Tick"/>
                    </Triggers>
                    <ContentTemplate>
                       <div style="visibility: hidden">
                         <h2 >International Incoming</h2>
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
                        <div style="float: left; padding-left: 100px;">
                        </div>
                        <p></p>
                        </div>
                    </ContentTemplate>
                </asp:UpdatePanel>
            
            <asp:Timer ID="Timer3" runat="server" Interval="30000" OnTick="Timer3_Tick" Enabled="false">
            </asp:Timer>
           
            <div style="visibility: hidden">
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
                <%--<Triggers>
                    <asp:AsyncPostBackTrigger ControlID="Timer2" EventName="Tick" />
                </Triggers>--%>
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
                                    
                                <%--div 2--%>
                                <div style="float: left;">
                                    <%--PieChartIpTdm--%>
                                    <div>
                                        <div>
                               
                                            <div style="margin-left: auto; margin-right: auto; text-align: center;">
                                                <asp:Label ID="Label1" runat="server" Text="IP TDM Distribution (All Zones)" Font-Bold="true" Font-Size="Large" ForeColor="#08605c" CssClass="StrongText"></asp:Label>
                                            </div>
                                            <p>
                                            </p>
                                            <p>
                                    
                                                <asp:Chart ID="PieChartIpTdm" runat="server" Width="550px" Height="300px">
                                                    <Series>
                                                        <asp:Series Name="Series1" ChartType="Pie">
                                                            <Points>
                                                                <asp:DataPoint runat="server" Color="#08605c" LabelForeColor="White"/>
                                                                <asp:DataPoint runat="server" Color="#e40613" LabelForeColor="White"/>
        
                                                            </Points>
                                                        </asp:Series>
                                                    </Series>
                                                    <ChartAreas>
                                                        <asp:ChartArea Name="ChartArea1">
                                                            <Area3DStyle Enable3D="true" />
                                                        </asp:ChartArea>
                                                    </ChartAreas>
                                                </asp:Chart>
                                    

                                            </p>
                                        </div>
                                    </div>

                                    <%--DomesticDistribution--%>                                       
                                    <div >
                                        <div>
                                
                                            <div style="margin-left: auto; margin-right: auto; text-align: center;">
                                                <asp:Label ID="Label11" runat="server" Text="Distribution Of Domestic Call (Current Month)" Font-Bold="true" Font-Size="Medium" ForeColor="#08605c" CssClass="StrongText"></asp:Label>
                                            </div>
                                  
                                    
                                                <asp:Chart ID="DomesticDistribution" runat="server" Width="480px" Height="300px">
                                                    <Series>
                                                        <asp:Series  Name="Series1" ChartType="Column">
                                                            <Points>
                                                                <%--<asp:DataPoint AxisLabel="Agni" YValues="90" Color="#08605c" />
                                                <asp:DataPoint AxisLabel="Banglatelecom" YValues="10" Color="#e40613"/>
                                                <asp:DataPoint AxisLabel="Bangla" YValues="56" Color="#F86F03"/>
                                                <asp:DataPoint AxisLabel="Bantel" YValues="78" Color="#FFA41B"/>
                                                <asp:DataPoint AxisLabel="Gazinetwork" YValues="25" Color="#8EAC50"/>
                                                <asp:DataPoint AxisLabel="Getco" YValues="96" Color="#898121"/>
                                                <asp:DataPoint AxisLabel="Immamnetworks" YValues="33" Color="#E7B10A"/>
                                                <asp:DataPoint AxisLabel="Jibondhara" YValues="44" Color="#4E4FEB"/>
                                                <asp:DataPoint AxisLabel="Mmcommunication" YValues="11" Color="#068FFF"/>
                                                <asp:DataPoint AxisLabel="M&h" YValues="55" Color="#1D5B79"/>
                                                <asp:DataPoint AxisLabel="Btcl" YValues="55" Color="#EF6262"/>
                                                <asp:DataPoint AxisLabel="Paradise" YValues="75" Color="#F3AA60"/>
                                                <asp:DataPoint AxisLabel="Purple" YValues="66" Color="#F2EE9D"/>
                                                <asp:DataPoint AxisLabel="Ringtech" YValues="91" Color="#7A9D54"/>
                                                <asp:DataPoint AxisLabel="Crossworld" YValues="88" Color="#557A46"/>
                                                <asp:DataPoint AxisLabel="Sheba" YValues="99" Color="#8C3333"/>
                                                <asp:DataPoint AxisLabel="Softex" YValues="34" Color="#252B48"/>
                                                <asp:DataPoint AxisLabel="Teleexchange" YValues="75" Color="#448069"/>
                                                <asp:DataPoint AxisLabel="NewGeneration" YValues="20" Color="#F7E987"/>
                                                <asp:DataPoint AxisLabel="TeleplusNetwork" YValues="60" Color="#8CABFF"/>
                                                <asp:DataPoint AxisLabel="Summit" YValues="40" Color="#4477CE"/>
                                                <asp:DataPoint AxisLabel="Mothertel" YValues="45" Color="#512B81"/>
                                                <asp:DataPoint AxisLabel="Voicetel" YValues="17" Color="#35155D"/>--%>
                                                    
                                                    
                                                            </Points>
                                                        </asp:Series>
                                                    </Series>
                                                    <ChartAreas>
                                                        <asp:ChartArea Name="ChartArea1">
                                                            <AxisX Title="ICX Names" TitleFont="Arial, 12px" LineColor="#666666" LineWidth="2">
                                                                <LabelStyle Font="Arial, 10px" Interval="1" />
                                                            </AxisX>
                                                            <AxisY Title="Minutes(Million)" TitleFont="Arial, 12px" LineColor="#666666" LineWidth="2">
                                                                <LabelStyle Font="Arial, 10px" />
                                                            </AxisY>
                                                        </asp:ChartArea>
                                                    </ChartAreas>
                                      
                                                </asp:Chart>
                                

                                        </div>
                                    </div>
                                    
                                </div>

                                <%--div 3--%>
                                <div style="float: left;">
                                    <%-- IpTdmDistribution --%>
                                    <div>

                                            <div style="margin-left: auto; margin-right: auto; text-align: center;">
                                                <asp:Label ID="Label3" runat="server" Text="IP TDM Distribution (Zone Wise)" Font-Bold="true" Font-Size="large" ForeColor="#08605c" CssClass="StrongText"></asp:Label>
                                            </div>
                                            
                                            

                                                <div id="chartContainer">
                                                    <asp:Chart ID="IpTdmDistribution" runat="server" Width="480px" Height="300px">
                                                        <Series>
                                                            <asp:Series Name="PositiveSeries" ChartType="StackedBar">
                                                                <Points>
                                                                    <%--<asp:DataPoint AxisLabel="Sylhet" YValues="25" Color="#08605c" LabelForeColor="White" />
                                                    <asp:DataPoint AxisLabel="Bogura" YValues="5" Color="#08605c" LabelForeColor="White" />
                                                    <asp:DataPoint AxisLabel="Khulna" YValues="7" Color="#08605c" LabelForeColor="White" />
                                                    <asp:DataPoint AxisLabel="Chattogram" YValues="9" Color="#08605c" LabelForeColor="White" />
                                                    <asp:DataPoint AxisLabel="Dhaka" YValues="29" Color="#08605c" LabelForeColor="White" />--%>
                                                                </Points>
                                                            </asp:Series>
                                                            <asp:Series Name="NegativeSeries" ChartType="StackedBar">
                                                                <Points>
                                                                    <%--<asp:DataPoint AxisLabel="Sylhet" YValues="5" Color="#e40613" LabelForeColor="White" />
                                                    <asp:DataPoint AxisLabel="Bogura" YValues="15" Color="#e40613" LabelForeColor="White" />
                                                    <asp:DataPoint AxisLabel="Khulna" YValues="5" Color="#e40613" LabelForeColor="White" />
                                                    <asp:DataPoint AxisLabel="Chattogram" YValues="2" Color="#e40613" LabelForeColor="White" />
                                                    <asp:DataPoint AxisLabel="Dhaka" YValues="12" Color="#e40613" LabelForeColor="White" />--%>
                                                                </Points>
                                                            </asp:Series>
                                                        </Series>
                                                        <ChartAreas>
                                                            <asp:ChartArea Name="ChartArea1">
                                                                <AxisY Title="Minutes(Million)" TitleFont="Arial, 12px" LineColor="#666666" LineWidth="2">
                                                                    <LabelStyle Font="Arial, 10px" Interval="5" />
                                                                </AxisY>
                                                                <AxisX Title="Zone" TitleFont="Arial, 12px" LineColor="#666666" LineWidth="2">
                                                                    <LabelStyle Font="Arial, 10px" />
                                                                </AxisX>
                                                            </asp:ChartArea>
                                                        </ChartAreas>
                                                    </asp:Chart>
                                                </div>

                                        
                                    </div>

                                    <%-- Distribution Of Int. Incoming Call (Current Month) --%>
                                    <div>
                                        <div>
                                
                                            <div style="margin-left: auto; margin-right: auto; text-align: center;">
                                                <asp:Label ID="Label4" runat="server" Text="Distribution Of Int. Incoming Call (Current Month)" Font-Bold="true" Font-Size="Medium" ForeColor="#08605c" CssClass="StrongText"></asp:Label>
                                            </div>
                                            

                                                <asp:Chart ID="Chart1" runat="server" Width="480px" Height="300px">
                                                    <Series>
                                                        <asp:Series ChartType="Column">
                                                            <Points>
                                                                <asp:DataPoint AxisLabel="Agni" YValues="90" Color="#08605c" />
                                                                <asp:DataPoint AxisLabel="Banglatelecom" YValues="10" Color="#e40613"/>
                                                                <asp:DataPoint AxisLabel="Bangla" YValues="56" Color="#F86F03"/>
                                                                <asp:DataPoint AxisLabel="Bantel" YValues="78" Color="#FFA41B"/>
                                                                <asp:DataPoint AxisLabel="Gazinetwork" YValues="25" Color="#8EAC50"/>
                                                                <asp:DataPoint AxisLabel="Getco" YValues="96" Color="#898121"/>
                                                                <asp:DataPoint AxisLabel="Immamnetworks" YValues="33" Color="#E7B10A"/>
                                                                <asp:DataPoint AxisLabel="Jibondhara" YValues="44" Color="#4E4FEB"/>
                                                                <asp:DataPoint AxisLabel="Mmcommunication" YValues="11" Color="#068FFF"/>
                                                                <asp:DataPoint AxisLabel="M&h" YValues="55" Color="#1D5B79"/>
                                                                <asp:DataPoint AxisLabel="Btcl" YValues="55" Color="#EF6262"/>
                                                                <asp:DataPoint AxisLabel="Paradise" YValues="75" Color="#F3AA60"/>
                                                                <asp:DataPoint AxisLabel="Purple" YValues="66" Color="#F2EE9D"/>
                                                                <asp:DataPoint AxisLabel="Ringtech" YValues="91" Color="#7A9D54"/>
                                                                <asp:DataPoint AxisLabel="Crossworld" YValues="88" Color="#557A46"/>
                                                                <asp:DataPoint AxisLabel="Sheba" YValues="99" Color="#8C3333"/>
                                                                <asp:DataPoint AxisLabel="Softex" YValues="34" Color="#252B48"/>
                                                                <asp:DataPoint AxisLabel="Teleexchange" YValues="75" Color="#448069"/>
                                                                <asp:DataPoint AxisLabel="NewGeneration" YValues="20" Color="#F7E987"/>
                                                                <asp:DataPoint AxisLabel="TeleplusNetwork" YValues="60" Color="#8CABFF"/>
                                                                <asp:DataPoint AxisLabel="Summit" YValues="40" Color="#4477CE"/>
                                                                <asp:DataPoint AxisLabel="Mothertel" YValues="45" Color="#512B81"/>
                                                                <asp:DataPoint AxisLabel="Voicetel" YValues="17" Color="#35155D"/>
                                                    
                                                    
                                                            </Points>
                                                        </asp:Series>
                                                    </Series>
                                                    <ChartAreas>
                                                        <asp:ChartArea Name="ChartArea1">
                                                            <AxisX Title="ICX Names" TitleFont="Arial, 12px" LineColor="#666666" LineWidth="2">
                                                                <LabelStyle Font="Arial, 10px" Interval="1" />
                                                            </AxisX>
                                                            <AxisY Title="Minutes(Million)" TitleFont="Arial, 12px" LineColor="#666666" LineWidth="2">
                                                                <LabelStyle Font="Arial, 10px" />
                                                            </AxisY>
                                                        </asp:ChartArea>
                                                    </ChartAreas>
                                      
                                                </asp:Chart>
                                    

                                            
                                        </div>
                                    </div>
                                    

                                </div>

                            </div>
                        </div>

                    </div>
                    


                    
                    
                <%--                    Bar Chart Sylhet--%>
                <div style="text-align: center; float: left;">
                    <div style="float: left;">
                        <div class="col-3">
                                
                            <div style="margin-left: auto; margin-right: auto; text-align: center;">
                                <asp:Label ID="Label5" runat="server" Text="Distribution Of Int. Outgoing Call (Current Month)" Font-Bold="true" Font-Size="medium" ForeColor="#08605c" CssClass="StrongText"></asp:Label>
                            </div>
                            <p>
                            </p>
                            <p>
                                  
                                    
                                <asp:Chart ID="Chart2" runat="server" Width="480px" Height="300px">
                                    <Series>
                                        <asp:Series ChartType="Column">
                                            <Points>
                                                <asp:DataPoint AxisLabel="Agni" YValues="90" Color="#08605c" />
                                                <asp:DataPoint AxisLabel="Banglatelecom" YValues="10" Color="#e40613"/>
                                                <asp:DataPoint AxisLabel="Bangla" YValues="56" Color="#F86F03"/>
                                                <asp:DataPoint AxisLabel="Bantel" YValues="78" Color="#FFA41B"/>
                                                <asp:DataPoint AxisLabel="Gazinetwork" YValues="25" Color="#8EAC50"/>
                                                <asp:DataPoint AxisLabel="Getco" YValues="96" Color="#898121"/>
                                                <asp:DataPoint AxisLabel="Immamnetworks" YValues="33" Color="#E7B10A"/>
                                                <asp:DataPoint AxisLabel="Jibondhara" YValues="44" Color="#4E4FEB"/>
                                                <asp:DataPoint AxisLabel="Mmcommunication" YValues="11" Color="#068FFF"/>
                                                <asp:DataPoint AxisLabel="M&h" YValues="55" Color="#1D5B79"/>
                                                <asp:DataPoint AxisLabel="Btcl" YValues="55" Color="#EF6262"/>
                                                <asp:DataPoint AxisLabel="Paradise" YValues="75" Color="#F3AA60"/>
                                                <asp:DataPoint AxisLabel="Purple" YValues="66" Color="#F2EE9D"/>
                                                <asp:DataPoint AxisLabel="Ringtech" YValues="91" Color="#7A9D54"/>
                                                <asp:DataPoint AxisLabel="Crossworld" YValues="88" Color="#557A46"/>
                                                <asp:DataPoint AxisLabel="Sheba" YValues="99" Color="#8C3333"/>
                                                <asp:DataPoint AxisLabel="Softex" YValues="34" Color="#252B48"/>
                                                <asp:DataPoint AxisLabel="Teleexchange" YValues="75" Color="#448069"/>
                                                <asp:DataPoint AxisLabel="NewGeneration" YValues="20" Color="#F7E987"/>
                                                <asp:DataPoint AxisLabel="TeleplusNetwork" YValues="60" Color="#8CABFF"/>
                                                <asp:DataPoint AxisLabel="Summit" YValues="40" Color="#4477CE"/>
                                                <asp:DataPoint AxisLabel="Mothertel" YValues="45" Color="#512B81"/>
                                                <asp:DataPoint AxisLabel="Voicetel" YValues="17" Color="#35155D"/>
                                                    
                                                    
                                            </Points>
                                        </asp:Series>
                                    </Series>
                                    <ChartAreas>
                                        <asp:ChartArea Name="ChartArea1">
                                            <AxisX Title="ICX Names" TitleFont="Arial, 12px" LineColor="#666666" LineWidth="2">
                                                <LabelStyle Font="Arial, 10px" Interval="1" />
                                            </AxisX>
                                            <AxisY Title="Minutes(Million)" TitleFont="Arial, 12px" LineColor="#666666" LineWidth="2">
                                                <LabelStyle Font="Arial, 10px" />
                                            </AxisY>
                                        </asp:ChartArea>
                                    </ChartAreas>
                                      
                                </asp:Chart>
                                    

                            </p>
                        </div>
                    </div>

                </div>
                    
                    
                <%--                    Bar Chart Barishal--%>
                <div style="text-align: center; float: left;">
                    <div style="float: left;">
                        <div class="col-3">
                                
                            <div style="margin-left: auto; margin-right: auto; text-align: center;">
                                <asp:Label ID="Label6" runat="server" Text="ICX Distribution Sylhet Division (Current Month)" Font-Bold="true" Font-Size="Large" ForeColor="#08605c" CssClass="StrongText"></asp:Label>
                            </div>
                            <p>
                            </p>
                            <p>
                                  
                                    
                                <asp:Chart ID="Chart3" runat="server" Width="480px" Height="300px">
                                    <Series>
                                        <asp:Series ChartType="Column">
                                            <Points>
                                                <asp:DataPoint AxisLabel="Agni" YValues="90" Color="#08605c" />
                                                <asp:DataPoint AxisLabel="Banglatelecom" YValues="10" Color="#e40613"/>
                                                <asp:DataPoint AxisLabel="Bangla" YValues="56" Color="#F86F03"/>
                                                <asp:DataPoint AxisLabel="Bantel" YValues="78" Color="#FFA41B"/>
                                                <asp:DataPoint AxisLabel="Gazinetwork" YValues="25" Color="#8EAC50"/>
                                                <asp:DataPoint AxisLabel="Getco" YValues="96" Color="#898121"/>
                                                <asp:DataPoint AxisLabel="Immamnetworks" YValues="33" Color="#E7B10A"/>
                                                <asp:DataPoint AxisLabel="Jibondhara" YValues="44" Color="#4E4FEB"/>
                                                <asp:DataPoint AxisLabel="Mmcommunication" YValues="11" Color="#068FFF"/>
                                                <asp:DataPoint AxisLabel="M&h" YValues="55" Color="#1D5B79"/>
                                                <asp:DataPoint AxisLabel="Btcl" YValues="55" Color="#EF6262"/>
                                                <asp:DataPoint AxisLabel="Paradise" YValues="75" Color="#F3AA60"/>
                                                <asp:DataPoint AxisLabel="Purple" YValues="66" Color="#F2EE9D"/>
                                                <asp:DataPoint AxisLabel="Ringtech" YValues="91" Color="#7A9D54"/>
                                                <asp:DataPoint AxisLabel="Crossworld" YValues="88" Color="#557A46"/>
                                                <asp:DataPoint AxisLabel="Sheba" YValues="99" Color="#8C3333"/>
                                                <asp:DataPoint AxisLabel="Softex" YValues="34" Color="#252B48"/>
                                                <asp:DataPoint AxisLabel="Teleexchange" YValues="75" Color="#448069"/>
                                                <asp:DataPoint AxisLabel="NewGeneration" YValues="20" Color="#F7E987"/>
                                                <asp:DataPoint AxisLabel="TeleplusNetwork" YValues="60" Color="#8CABFF"/>
                                                <asp:DataPoint AxisLabel="Summit" YValues="40" Color="#4477CE"/>
                                                <asp:DataPoint AxisLabel="Mothertel" YValues="45" Color="#512B81"/>
                                                <asp:DataPoint AxisLabel="Voicetel" YValues="17" Color="#35155D"/>
                                                    
                                                    
                                            </Points>
                                        </asp:Series>
                                    </Series>
                                    <ChartAreas>
                                        <asp:ChartArea Name="ChartArea1">
                                            <AxisX Title="ICX Names" TitleFont="Arial, 12px" LineColor="#666666" LineWidth="2">
                                                <LabelStyle Font="Arial, 10px" Interval="1" />
                                            </AxisX>
                                            <AxisY Title="Minutes(Million)" TitleFont="Arial, 12px" LineColor="#666666" LineWidth="2">
                                                <LabelStyle Font="Arial, 10px" />
                                            </AxisY>
                                        </asp:ChartArea>
                                    </ChartAreas>
                                      
                                </asp:Chart>
                                    

                            </p>
                        </div>
                    </div>

                </div>
                    
                    
                <%--                    Bar Chart Rangpur--%>
                <div style="text-align: center; float: left;">
                    <div style="float: left;">
                        <div class="col-3">
                                
                            <div style="margin-left: auto; margin-right: auto; text-align: center;">
                                <asp:Label ID="Label7" runat="server" Text="ICX Distribution Barishal Division (Current Month)" Font-Bold="true" Font-Size="Large" ForeColor="#08605c" CssClass="StrongText"></asp:Label>
                            </div>
                            <p>
                            </p>
                            <p>
                                  
                                    
                                <asp:Chart ID="Chart4" runat="server" Width="480px" Height="300px">
                                    <Series>
                                        <asp:Series ChartType="Column">
                                            <Points>
                                                <asp:DataPoint AxisLabel="Agni" YValues="90" Color="#08605c" />
                                                <asp:DataPoint AxisLabel="Banglatelecom" YValues="10" Color="#e40613"/>
                                                <asp:DataPoint AxisLabel="Bangla" YValues="56" Color="#F86F03"/>
                                                <asp:DataPoint AxisLabel="Bantel" YValues="78" Color="#FFA41B"/>
                                                <asp:DataPoint AxisLabel="Gazinetwork" YValues="25" Color="#8EAC50"/>
                                                <asp:DataPoint AxisLabel="Getco" YValues="96" Color="#898121"/>
                                                <asp:DataPoint AxisLabel="Immamnetworks" YValues="33" Color="#E7B10A"/>
                                                <asp:DataPoint AxisLabel="Jibondhara" YValues="44" Color="#4E4FEB"/>
                                                <asp:DataPoint AxisLabel="Mmcommunication" YValues="11" Color="#068FFF"/>
                                                <asp:DataPoint AxisLabel="M&h" YValues="55" Color="#1D5B79"/>
                                                <asp:DataPoint AxisLabel="Btcl" YValues="55" Color="#EF6262"/>
                                                <asp:DataPoint AxisLabel="Paradise" YValues="75" Color="#F3AA60"/>
                                                <asp:DataPoint AxisLabel="Purple" YValues="66" Color="#F2EE9D"/>
                                                <asp:DataPoint AxisLabel="Ringtech" YValues="91" Color="#7A9D54"/>
                                                <asp:DataPoint AxisLabel="Crossworld" YValues="88" Color="#557A46"/>
                                                <asp:DataPoint AxisLabel="Sheba" YValues="99" Color="#8C3333"/>
                                                <asp:DataPoint AxisLabel="Softex" YValues="34" Color="#252B48"/>
                                                <asp:DataPoint AxisLabel="Teleexchange" YValues="75" Color="#448069"/>
                                                <asp:DataPoint AxisLabel="NewGeneration" YValues="20" Color="#F7E987"/>
                                                <asp:DataPoint AxisLabel="TeleplusNetwork" YValues="60" Color="#8CABFF"/>
                                                <asp:DataPoint AxisLabel="Summit" YValues="40" Color="#4477CE"/>
                                                <asp:DataPoint AxisLabel="Mothertel" YValues="45" Color="#512B81"/>
                                                <asp:DataPoint AxisLabel="Voicetel" YValues="17" Color="#35155D"/>
                                                    
                                                    
                                            </Points>
                                        </asp:Series>
                                    </Series>
                                    <ChartAreas>
                                        <asp:ChartArea Name="ChartArea1">
                                            <AxisX Title="ICX Names" TitleFont="Arial, 12px" LineColor="#666666" LineWidth="2">
                                                <LabelStyle Font="Arial, 10px" Interval="1" />
                                            </AxisX>
                                            <AxisY Title="Minutes(Million)" TitleFont="Arial, 12px" LineColor="#666666" LineWidth="2">
                                                <LabelStyle Font="Arial, 10px" />
                                            </AxisY>
                                        </asp:ChartArea>
                                    </ChartAreas>
                                      
                                </asp:Chart>
                                    

                            </p>
                        </div>
                    </div>

                </div>

                <%--                    Bar Chart Rajshahi--%>
                <div style="text-align: center; float: left;">
                    <div style="float: left;">
                        <div class="col-3">
                                
                            <div style="margin-left: auto; margin-right: auto; text-align: center;">
                                <asp:Label ID="Label8" runat="server" Text="ICX Distribution  Rangpur Division (Current Month)" Font-Bold="true" Font-Size="Large" ForeColor="#08605c" CssClass="StrongText"></asp:Label>
                            </div>
                            <p>
                            </p>
                            <p>
                                  
                                    
                                <asp:Chart ID="Chart5" runat="server" Width="480px" Height="300px">
                                    <Series>
                                        <asp:Series ChartType="Column">
                                            <Points>
                                                <asp:DataPoint AxisLabel="Agni" YValues="90" Color="#08605c" />
                                                <asp:DataPoint AxisLabel="Banglatelecom" YValues="10" Color="#e40613"/>
                                                <asp:DataPoint AxisLabel="Bangla" YValues="56" Color="#F86F03"/>
                                                <asp:DataPoint AxisLabel="Bantel" YValues="78" Color="#FFA41B"/>
                                                <asp:DataPoint AxisLabel="Gazinetwork" YValues="25" Color="#8EAC50"/>
                                                <asp:DataPoint AxisLabel="Getco" YValues="96" Color="#898121"/>
                                                <asp:DataPoint AxisLabel="Immamnetworks" YValues="33" Color="#E7B10A"/>
                                                <asp:DataPoint AxisLabel="Jibondhara" YValues="44" Color="#4E4FEB"/>
                                                <asp:DataPoint AxisLabel="Mmcommunication" YValues="11" Color="#068FFF"/>
                                                <asp:DataPoint AxisLabel="M&h" YValues="55" Color="#1D5B79"/>
                                                <asp:DataPoint AxisLabel="Btcl" YValues="55" Color="#EF6262"/>
                                                <asp:DataPoint AxisLabel="Paradise" YValues="75" Color="#F3AA60"/>
                                                <asp:DataPoint AxisLabel="Purple" YValues="66" Color="#F2EE9D"/>
                                                <asp:DataPoint AxisLabel="Ringtech" YValues="91" Color="#7A9D54"/>
                                                <asp:DataPoint AxisLabel="Crossworld" YValues="88" Color="#557A46"/>
                                                <asp:DataPoint AxisLabel="Sheba" YValues="99" Color="#8C3333"/>
                                                <asp:DataPoint AxisLabel="Softex" YValues="34" Color="#252B48"/>
                                                <asp:DataPoint AxisLabel="Teleexchange" YValues="75" Color="#448069"/>
                                                <asp:DataPoint AxisLabel="NewGeneration" YValues="20" Color="#F7E987"/>
                                                <asp:DataPoint AxisLabel="TeleplusNetwork" YValues="60" Color="#8CABFF"/>
                                                <asp:DataPoint AxisLabel="Summit" YValues="40" Color="#4477CE"/>
                                                <asp:DataPoint AxisLabel="Mothertel" YValues="45" Color="#512B81"/>
                                                <asp:DataPoint AxisLabel="Voicetel" YValues="17" Color="#35155D"/>
                                                    
                                                    
                                            </Points>
                                        </asp:Series>
                                    </Series>
                                    <ChartAreas>
                                        <asp:ChartArea Name="ChartArea1">
                                            <AxisX Title="ICX Names" TitleFont="Arial, 12px" LineColor="#666666" LineWidth="2">
                                                <LabelStyle Font="Arial, 10px" Interval="1" />
                                            </AxisX>
                                            <AxisY Title="Minutes(Million)" TitleFont="Arial, 12px" LineColor="#666666" LineWidth="2">
                                                <LabelStyle Font="Arial, 10px" />
                                            </AxisY>
                                        </asp:ChartArea>
                                    </ChartAreas>
                                      
                                </asp:Chart>
                                    

                            </p>
                        </div>
                    </div>

                </div>  
                    
                    
                <%--                    Bar Chart Chottogram--%>
                <div style="text-align: center; float: left;">
                    <div style="float: left;">
                        <div class="col-3">
                                
                            <div style="margin-left: auto; margin-right: auto; text-align: center;">
                                <asp:Label ID="Label9" runat="server" Text="ICX Distribution Rajshahi Division (Current Month)" Font-Bold="true" Font-Size="Large" ForeColor="#08605c" CssClass="StrongText"></asp:Label>
                            </div>
                            <p>
                            </p>
                            <p>
                                  
                                    
                                <asp:Chart ID="Chart6" runat="server" Width="480px" Height="300px">
                                    <Series>
                                        <asp:Series ChartType="Column">
                                            <Points>
                                                <asp:DataPoint AxisLabel="Agni" YValues="90" Color="#08605c" />
                                                <asp:DataPoint AxisLabel="Banglatelecom" YValues="10" Color="#e40613"/>
                                                <asp:DataPoint AxisLabel="Bangla" YValues="56" Color="#F86F03"/>
                                                <asp:DataPoint AxisLabel="Bantel" YValues="78" Color="#FFA41B"/>
                                                <asp:DataPoint AxisLabel="Gazinetwork" YValues="25" Color="#8EAC50"/>
                                                <asp:DataPoint AxisLabel="Getco" YValues="96" Color="#898121"/>
                                                <asp:DataPoint AxisLabel="Immamnetworks" YValues="33" Color="#E7B10A"/>
                                                <asp:DataPoint AxisLabel="Jibondhara" YValues="44" Color="#4E4FEB"/>
                                                <asp:DataPoint AxisLabel="Mmcommunication" YValues="11" Color="#068FFF"/>
                                                <asp:DataPoint AxisLabel="M&h" YValues="55" Color="#1D5B79"/>
                                                <asp:DataPoint AxisLabel="Btcl" YValues="55" Color="#EF6262"/>
                                                <asp:DataPoint AxisLabel="Paradise" YValues="75" Color="#F3AA60"/>
                                                <asp:DataPoint AxisLabel="Purple" YValues="66" Color="#F2EE9D"/>
                                                <asp:DataPoint AxisLabel="Ringtech" YValues="91" Color="#7A9D54"/>
                                                <asp:DataPoint AxisLabel="Crossworld" YValues="88" Color="#557A46"/>
                                                <asp:DataPoint AxisLabel="Sheba" YValues="99" Color="#8C3333"/>
                                                <asp:DataPoint AxisLabel="Softex" YValues="34" Color="#252B48"/>
                                                <asp:DataPoint AxisLabel="Teleexchange" YValues="75" Color="#448069"/>
                                                <asp:DataPoint AxisLabel="NewGeneration" YValues="20" Color="#F7E987"/>
                                                <asp:DataPoint AxisLabel="TeleplusNetwork" YValues="60" Color="#8CABFF"/>
                                                <asp:DataPoint AxisLabel="Summit" YValues="40" Color="#4477CE"/>
                                                <asp:DataPoint AxisLabel="Mothertel" YValues="45" Color="#512B81"/>
                                                <asp:DataPoint AxisLabel="Voicetel" YValues="17" Color="#35155D"/>
                                                    
                                                    
                                            </Points>
                                        </asp:Series>
                                    </Series>
                                    <ChartAreas>
                                        <asp:ChartArea Name="ChartArea1">
                                            <AxisX Title="ICX Names" TitleFont="Arial, 12px" LineColor="#666666" LineWidth="2">
                                                <LabelStyle Font="Arial, 10px" Interval="1" />
                                            </AxisX>
                                            <AxisY Title="Minutes(Million)" TitleFont="Arial, 12px" LineColor="#666666" LineWidth="2">
                                                <LabelStyle Font="Arial, 10px" />
                                            </AxisY>
                                        </asp:ChartArea>
                                    </ChartAreas>
                                      
                                </asp:Chart>
                                    

                            </p>
                        </div>
                    </div>

                </div>                     
                <%--                    Bar Chart Mymenshing --%>
                <div style="text-align: center; float: left;">
                    <div style="float: left;">
                        <div class="col-3">
                                
                            <div style="margin-left: auto; margin-right: auto; text-align: center;">
                                <asp:Label ID="Label10" runat="server" Text="ICX Distribution Mymenshing Division (Current Month)" Font-Bold="true" Font-Size="Large" ForeColor="#08605c" CssClass="StrongText"></asp:Label>
                            </div>
                            <p>
                            </p>
                            <p>
                                  
                                    
                                <asp:Chart ID="Chart7" runat="server" Width="480px" Height="300px">
                                    <Series>
                                        <asp:Series ChartType="Column">
                                            <Points>
                                                <asp:DataPoint AxisLabel="Agni" YValues="90" Color="#08605c" />
                                                <asp:DataPoint AxisLabel="Banglatelecom" YValues="10" Color="#e40613"/>
                                                <asp:DataPoint AxisLabel="Bangla" YValues="56" Color="#F86F03"/>
                                                <asp:DataPoint AxisLabel="Bantel" YValues="78" Color="#FFA41B"/>
                                                <asp:DataPoint AxisLabel="Gazinetwork" YValues="25" Color="#8EAC50"/>
                                                <asp:DataPoint AxisLabel="Getco" YValues="96" Color="#898121"/>
                                                <asp:DataPoint AxisLabel="Immamnetworks" YValues="33" Color="#E7B10A"/>
                                                <asp:DataPoint AxisLabel="Jibondhara" YValues="44" Color="#4E4FEB"/>
                                                <asp:DataPoint AxisLabel="Mmcommunication" YValues="11" Color="#068FFF"/>
                                                <asp:DataPoint AxisLabel="M&h" YValues="55" Color="#1D5B79"/>
                                                <asp:DataPoint AxisLabel="Btcl" YValues="55" Color="#EF6262"/>
                                                <asp:DataPoint AxisLabel="Paradise" YValues="75" Color="#F3AA60"/>
                                                <asp:DataPoint AxisLabel="Purple" YValues="66" Color="#F2EE9D"/>
                                                <asp:DataPoint AxisLabel="Ringtech" YValues="91" Color="#7A9D54"/>
                                                <asp:DataPoint AxisLabel="Crossworld" YValues="88" Color="#557A46"/>
                                                <asp:DataPoint AxisLabel="Sheba" YValues="99" Color="#8C3333"/>
                                                <asp:DataPoint AxisLabel="Softex" YValues="34" Color="#252B48"/>
                                                <asp:DataPoint AxisLabel="Teleexchange" YValues="75" Color="#448069"/>
                                                <asp:DataPoint AxisLabel="NewGeneration" YValues="20" Color="#F7E987"/>
                                                <asp:DataPoint AxisLabel="TeleplusNetwork" YValues="60" Color="#8CABFF"/>
                                                <asp:DataPoint AxisLabel="Summit" YValues="40" Color="#4477CE"/>
                                                <asp:DataPoint AxisLabel="Mothertel" YValues="45" Color="#512B81"/>
                                                <asp:DataPoint AxisLabel="Voicetel" YValues="17" Color="#35155D"/>
                                                    
                                                    
                                            </Points>
                                        </asp:Series>
                                    </Series>
                                    <ChartAreas>
                                        <asp:ChartArea Name="ChartArea1">
                                            <AxisX Title="ICX Names" TitleFont="Arial, 12px" LineColor="#666666" LineWidth="2">
                                                <LabelStyle Font="Arial, 10px" Interval="1" />
                                            </AxisX>
                                            <AxisY Title="Minutes (Million)" TitleFont="Arial, 12px" LineColor="#666666" LineWidth="2">
                                                <LabelStyle Font="Arial, 10px" />
                                            </AxisY>
                                            <%--<Area3DStyle Enable3D="true" />--%>
                                        </asp:ChartArea>
                                    </ChartAreas>
                                      
                                </asp:Chart>
                                    

                            </p>
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
