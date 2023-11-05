<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="True" CodeBehind="Tgs.aspx.cs" Inherits="TgsOfICX" %>

<%@ Register Assembly="System.Web.DataVisualization, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" Namespace="System.Web.UI.DataVisualization.Charting" TagPrefix="asp" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

   <%-- <div>
        <label for="searchTerm">Search:</label>
        <input type="text" id="searchTerm" runat="server" />
        <asp:Button ID="btnSearch" runat="server" Text="Search" OnClick="btnSearch_Click" />
    </div>--%>

    

    <%--<asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>--%>
    <ajaxtoolkit:toolkitscriptmanager runat="Server" enablescriptglobalization="true"
        enablescriptlocalization="true" id="ToolkitScriptManager1" />
    <div style="text-align: left; min-width: 1000px;">
    </div>
    <div style="padding-left: 20px;">

        <div style="height: 40px;"></div>

        <%--TGs --%>
        <div>
            <%--div 1--%>
            <div style="float: left; padding-left: 20px">
                <div style="margin-right: auto; text-align: left;">
                    <asp:Label ID="LabelTgs" runat="server" Text="Tg List" Font-Bold="true" Font-Size="Large" ForeColor="#08605c" CssClass="StrongText"></asp:Label>
                </div>
                <asp:GridView ID="GridViewTgs" runat="server" AutoGenerateColumns="false" DataKeyNames="id">
                    <Columns>
                        <asp:BoundField DataField="TgName" HeaderText="Tg" />
                        <asp:BoundField DataField="SwitchName" HeaderText="SwitchName" />
                        <asp:BoundField DataField="Zone" HeaderText="Zone" />
                        <asp:BoundField DataField="Partner" HeaderText="Partners" />
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
    </div>
</asp:Content>
