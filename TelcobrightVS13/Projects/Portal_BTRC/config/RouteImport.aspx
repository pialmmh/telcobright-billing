<%@ Page Title="" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="True" CodeBehind="RouteImport.aspx.cs" Inherits="ConfigRouteImport" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" Runat="Server">

<div style="width:950px;height:100px;text-align:center;background-color:#f2f2f2;margin-top:50px;margin-left:50px;padding-top:60px;">

    <asp:Label ID="Label1" runat="server" Width="500" Text="Browse for file to import/merge route information." BackColor="#FFFFCC"></asp:Label>
    <br />
<asp:FileUpload id="FileUploadControl" runat="server" />

<asp:LinkButton runat="server" id="UploadButton" text="Upload and Import" 
                     OnClientClick="return confirm('For existing routes only No of Ports, Description and Ports of existing routes will be updated, other fields will be ignored. Are you sure to continue?');" onclick="UploadButton_Click"/>
<span style="margin-left:50px;">
    <asp:LinkButton runat="server" id="DeleteAndUpload" text="Delete All and Import" Font-Bold="true" ForeColor="Red"
        OnClientClick="return confirm('All existing routes will be deleted before import. Are you sure to continue?');" onclick="DeleteAndUpload_Click"/>
</span>
                    <br />
                    <asp:Label runat="server" id="Label2" text="" ForeColor="Red" />            
                    
                    
                    <br />
                    <asp:Label runat="server" id="StatusLabel" text="" ForeColor="Red" />            

   
   
  <script runat="server">
    protected void Button1_Click(object sender, EventArgs e)
    {
        // Introducing delay for demonstration.
        System.Threading.Thread.Sleep(3000);
        
    }
</script>


    <br />
    
</div>

</asp:Content>

