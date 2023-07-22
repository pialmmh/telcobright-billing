<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="True"
    CodeBehind="~/reports/MediationError.aspx.cs" Inherits="DefaultMedError" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>
<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">
    <script type="text/javascript">
     function SetTarget() {

      document.forms[0].target = "_blank";
  }
  function setTargetInit() {
      document.forms[0].target = "_self";
  }
 </script>
</asp:Content>
<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">
    <ajaxToolkit:ToolkitScriptManager ID="ToolkitScriptManager1" runat="server" > </ajaxToolkit:ToolkitScriptManager>
    

    <div>
        <asp:Label runat="server" ID="lbl1"></asp:Label>
    </div>
    <span style="font-weight:bold;"> Real Time Update 
            <asp:CheckBox ID="CheckBoxRealTimeUpdate" runat="server" AutoPostBack="true" /></span>
    <asp:Button ID="submit" runat="server" Text="Refresh" onclick="submit_Click" />  
     <asp:Timer ID="Timer1" Interval="300000" runat="server" ontick="Timer1_Tick" ></asp:Timer>
     <div id="gridViewArea" style="margin-top:12px;">
         <asp:GridView ID="GridView1" runat="server" AllowPaging="True" PageSize="1000">
         </asp:GridView>



        </div>
</asp:Content>
