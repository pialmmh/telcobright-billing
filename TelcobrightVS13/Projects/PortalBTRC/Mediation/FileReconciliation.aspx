<%@ Page Title="" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeBehind="FileReconciliation.aspx.cs" Inherits="PortalApp.Mediation.FileReconciliation" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:FileUpload ID="fileUpload" runat="server" />
            <asp:Button ID="btnUpload" runat="server" Text="Upload File" OnClick="submit_Click" />
            <br> <br>
        </div>


        <div>
            <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="False">
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
            </asp:GridView>
        <asp:Button ID="ExportButton" runat="server" Text="Export to Excel" OnClick="ExportButton_Click" />

        </div>
          
    </form>
</body>
</html>
