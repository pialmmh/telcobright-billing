<%@ Page Title="" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeBehind="CDRFileReconciliation.aspx.cs" Inherits="PortalApp.Mediation.CDRFileReconciliation" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
   <div style="background-color:#edf2ef;color: black;float:left;border: 1px solid #707070;padding:10px; padding-right: 645px;">
        <div style=" font-weight: bold; ">
             <asp:Label ID="lblSelectOption" runat="server" Text="Select a switch for cdr file reconciliatiohn template:"></asp:Label>
        </div>
          
       
        <div>
            <asp:DropDownList ID="ddlOptions" runat="server"></asp:DropDownList>
        <asp:Button ID="btnDownload" runat="server" Text="Download Template" OnClick="DownloadButton_Click" />
        </div>
        
    </div>

    <div style="height:20px;clear:both;"></div>

     <div style="background-color:#edf2ef; color: black;float:left;clear: right; border: 1px solid #707070;padding:10px; padding-right: 560px;">
        <div style="color: black; font-weight: bold; ">
                 <asp:Label ID="Label1" runat="server" Text="Fillup template from own billing data and upload"></asp:Label>
           </div>

         <div style=" background-color: #f2f2f2; color: black;">
            <asp:FileUpload ID="fileUpload" runat="server" />
            <asp:Button ID="btnUpload" runat="server" Text="Upload & View Report" OnClick="upload_Excel_File" />
        </div>
    </div>


     <div style="height:20px;clear:both;"></div>

    <div style=" color: black;float:left;clear: right;">
        <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="False" CellPadding="4" ForeColor="#333333" GridLines="None">
            <AlternatingRowStyle BackColor="White" />
           <Columns>
                <asp:BoundField DataField="Value.switchName" HeaderText="switchName" SortExpression="switchName" />
                <asp:BoundField DataField="Value.FileName" HeaderText="FileName" SortExpression="FileName" />

                <asp:BoundField DataField="Value.RecordCountFromICX" HeaderText="RecordCountFromICX" SortExpression="RecordCountFromICX" />
                <asp:BoundField DataField="Value.RecordCountFromTB" HeaderText="RecordCountFromTB" SortExpression="RecordCountFromTB" />

                <asp:BoundField DataField="Value.DiffRecordCount" HeaderText="DiffRecordCount" SortExpression="DiffRecordCount" />
                <asp:BoundField DataField="Value.ActualDurationFromICX" HeaderText="ActualDurationFromICX" SortExpression="ActualDurationFromICX" />

                <asp:BoundField DataField="Value.ActualDuratinoTB" HeaderText="ActualDuratinoTB" SortExpression="ActualDuratinoTB" />
                <asp:BoundField DataField="Value.DiffDuration" HeaderText="DiffDuration" SortExpression="DiffDuration" />
          </Columns>
            <EditRowStyle BackColor="#7C6F57" />
            <FooterStyle BackColor="#1C5E55" Font-Bold="True" ForeColor="White" />
            <HeaderStyle BackColor="#1C5E55" Font-Bold="True" ForeColor="White" />
            <PagerStyle BackColor="#666666" ForeColor="White" HorizontalAlign="Center" />
            <RowStyle BackColor="#E3EAEB" />
            <SelectedRowStyle BackColor="#C5BBAF" Font-Bold="True" ForeColor="#333333" />
            <SortedAscendingCellStyle BackColor="#F8FAFA" />
            <SortedAscendingHeaderStyle BackColor="#246B61" />
            <SortedDescendingCellStyle BackColor="#D4DFE1" />
            <SortedDescendingHeaderStyle BackColor="#15524A" />
        </asp:GridView>
        <div style="height:20px;clear:both;"></div>
        <asp:Button ID="ExportButton" runat="server" Text="Export to Excel" OnClick="ExportButton_Click"  Enabled="false" />
    </div>
</asp:Content>
