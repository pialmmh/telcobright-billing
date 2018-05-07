<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="BalanceAdjustment.aspx.cs" Inherits="PortalApp.config.BalanceAdjustment" MasterPageFile="~/Site.master" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">
   
    <script src="../Scripts/moment.js" type="text/javascript"></script>

    <%--Page Load and Other Server Side Asp.net scripts--%>
       


</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" Runat="Server">
    <ajaxToolkit:ToolkitScriptManager ID="ToolkitScriptManager1" runat="server" />
    <div style="height:220px;margin-bottom:5px;padding-top:5px; width:640px; margin-top:5px;margin-left:0px;float:left;padding-left:5px;background-color:#F7F6F3;">
        <asp:UpdatePanel runat="server">
            <ContentTemplate>
                <div>
                    Partner:
                    <div style="clear:left;height:7px;"></div>
                    <asp:DropDownList ID="ddlistPartner" runat="server" Enabled="true" AutoPostBack="true" 
                        DataTextField="PartnerName" DataValueField="idPartner"
                        OnSelectedIndexChanged="ddlistPartner_OnSelectedIndexChanged" />
                </div>
                <div>
                    Service Account:
                    <div style="clear:left;height:7px;"></div>
                    <asp:DropDownList ID="ddlistServiceAccount" runat="server" Enabled="true" AutoPostBack="true" 
                                      DataTextField="accountName" DataValueField="id" />
                </div>
                <div>
                    Date:
                    <div style="clear:left;height:7px;"></div>
                    <asp:TextBox ID="txtDate" runat="server" />
                    <ajaxToolkit:CalendarExtender ID="CalendarStartDate" runat="server"
                                                  TargetControlID="txtDate" PopupButtonID="txtDate" Format="yyyy-MM-dd">
                    </ajaxToolkit:CalendarExtender>
                </div>
                <div>
                    Amount:
                    <div style="clear:left;height:7px;"></div>
                    <asp:TextBox runat="server" ID="txtAmount"></asp:TextBox>
                    <asp:RegularExpressionValidator ID="RegularExpressionValidator1"
                                                    ControlToValidate="txtAmount" runat="server"
                                                    ErrorMessage="Only decimal with a precision of 2 allowed"
                                                    ValidationExpression="\d+(\.\d{1,2})?">
                    </asp:RegularExpressionValidator>
                </div>
                <div>
                    <div style="clear:left;height:7px;"></div>
                    <asp:Button runat="server" ID="btnAddAdjustment" Text="Add Adjustment"/>
                </div>
            </ContentTemplate>
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="ddlistPartner" EventName="SelectedIndexChanged"/>
            </Triggers>
        </asp:UpdatePanel>
    </div>
</asp:Content>