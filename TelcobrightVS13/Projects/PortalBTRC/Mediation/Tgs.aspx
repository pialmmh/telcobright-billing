<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="True" CodeBehind="Tgs.aspx.cs" Inherits="TgsOfICX" %>

<%@ Register Assembly="System.Web.DataVisualization, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" Namespace="System.Web.UI.DataVisualization.Charting" TagPrefix="asp" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <div class="jumbotron">
        <h1>
            <asp:Label ID="lblCustomerDisplayName" runat="server" Text=""></asp:Label></h1>
    </div>

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
