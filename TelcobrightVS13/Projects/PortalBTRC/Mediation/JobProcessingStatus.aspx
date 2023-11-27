<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="True" CodeBehind="JobProcessingStatus.aspx.cs" Inherits="JobProcessingStatusICX" %>

<%@ Register Assembly="System.Web.DataVisualization, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" Namespace="System.Web.UI.DataVisualization.Charting" TagPrefix="asp" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

   <%-- <div>
        <label for="searchTerm">Search:</label>
        <input type="text" id="searchTerm" runat="server" />
        <asp:Button ID="btnSearch" runat="server" Text="Search" OnClick="btnSearch_Click" />
    </div>--%>

    

    <%--<asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>--%>
    <ajaxtoolkit:toolkitscriptmanager runat="Server" enablescriptglobalization="true"
        enablescriptlocalization="true" id="ToolkitScriptManager2" />
    <div style="text-align: left; min-width: 1000px;">
    </div>
    <div style="padding-left: 20px;">

        <div style="height: 40px;"></div>
         <asp:UpdatePanel ID="UpdatePanel1" runat="server">
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="Timer1" EventName="Tick" />
            </Triggers>

        <ContentTemplate>
        <%--TGs --%>
        <div >
            <%--div 1--%>
                    <div style="float: left; padding-left: 20px; text-align: center;">
                        <div style="margin-right: auto; text-align: center;">
                            <asp:Label ID="LabelJobProcessingStatus" runat="server" Text="Job Processing Status" Font-Bold="true" Font-Size="Large" ForeColor="#08605c" CssClass="StrongText"></asp:Label>
                        </div>
                        <asp:GridView ID="GridViewJobProcessingStatus" runat="server" AutoGenerateColumns="false" DataKeyNames="id">
                            <Columns>
                                <asp:BoundField DataField="ICXName" HeaderText="ICX Name" />
                                <asp:BoundField DataField="SwitchId" HeaderText="SwitchId" />
                                <asp:BoundField DataField="LastJobName" HeaderText="Last Job Name" />
                                <asp:BoundField DataField="CompletionTime" HeaderText="Completion Time" />
                                <asp:BoundField DataField="NoofRecords" HeaderText="No of Records" />
                            </Columns>
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

                    </div>

            <%--div 1--%>
                     <div style="float: left; padding-left: 40px; text-align: center;">
                        <div style="margin-right: auto; text-align: center;">
                            <asp:Label ID="Label1" runat="server" Text="Error Status" Font-Bold="true" Font-Size="Large" ForeColor="#08605c" CssClass="StrongText"></asp:Label>
                        </div>
                        <asp:GridView ID="GridViewErrorStatus" runat="server" AutoGenerateColumns="false" DataKeyNames="id">
                            <Columns>
                                <asp:BoundField DataField="ICXName" HeaderText="ICX Name" />
                                <asp:BoundField DataField="Error" HeaderText="Error" />
                                <asp:BoundField DataField="ProcessName" HeaderText="Process Name" />
                                <asp:BoundField DataField="OccuranceTime" HeaderText="Occurance Time" />
                            </Columns>
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

                    </div>

                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
        <asp:Timer ID="Timer1" runat="server" Interval="30000" OnTick="Timer1_Tick" Enabled="false">
        </asp:Timer>

    </div>
</asp:Content>
