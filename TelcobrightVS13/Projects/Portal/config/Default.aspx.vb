'This tutorial is provided in part by Server Intellect Web Hosting Solutions http://www.serverintellect.com

'Visit http://www.AspNetTutorials.com for more ASP.NET Tutorials

Imports System.Data.SqlClient
Imports System.Data
Partial Class _Default
    Inherits System.Web.UI.Page
    Dim myDt As DataTable

    
    Private Function CreateDataTable() As DataTable
        Dim myDataTable As DataTable = New DataTable()

        Dim myDataColumn As DataColumn

        myDataColumn = New DataColumn()
        myDataColumn.DataType = Type.GetType("System.String")
        myDataColumn.ColumnName = "id"
        myDataTable.Columns.Add(myDataColumn)

        myDataColumn = New DataColumn()
        myDataColumn.DataType = Type.GetType("System.String")
        myDataColumn.ColumnName = "username"
        myDataTable.Columns.Add(myDataColumn)

        myDataColumn = New DataColumn()
        myDataColumn.DataType = Type.GetType("System.String")
        myDataColumn.ColumnName = "firstname"
        myDataTable.Columns.Add(myDataColumn)

        myDataColumn = New DataColumn()
        myDataColumn.DataType = Type.GetType("System.String")
        myDataColumn.ColumnName = "lastname"
        myDataTable.Columns.Add(myDataColumn)

        Return myDataTable
    End Function
    Private Sub AddDataToTable(ByVal username As String, ByVal firstname As String, ByVal lastname As String, ByVal myTable As DataTable)
        Dim row As DataRow

        row = myTable.NewRow()

        row("id") = Guid.NewGuid().ToString()
        row("username") = username
        row("firstname") = firstname
        row("lastname") = lastname

        myTable.Rows.Add(row)

    End Sub
    Private Sub Page_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Me.Load
        If Me.IsPostBack = False Then
            myDt = New DataTable()
            myDt = CreateDataTable()
            Session("myDatatable") = myDt

            Me.GridView1.DataSource = (CType(Session("myDatatable"), DataTable)).DefaultView
            Me.GridView1.DataBind()
        End If
    End Sub

    Protected Sub btnAdd_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnAdd.Click
        If Me.txtUserName.Text.Trim().Equals("") Then

            Me.lblTips.Text = "You must fill a username."
            Return

        Else

            AddDataToTable(Me.txtUserName.Text.Trim(), Me.txtFirstName.Text.Trim(), Me.txtLastName.Text.Trim(), CType(Session("myDatatable"), DataTable))


            Me.GridView1.DataSource = CType(Session("myDatatable"), DataTable).DefaultView

            Me.GridView1.DataBind()

            Me.txtFirstName.Text = ""
            Me.txtLastName.Text = ""
            Me.txtUserName.Text = ""
            Me.lblTips.Text = ""

        End If
    End Sub

    Protected Sub GridView1_RowEditing(sender As Object, e As System.Web.UI.WebControls.GridViewEditEventArgs) Handles GridView1.RowEditing
        GridView1.EditIndex = e.NewEditIndex
        GridView1.DataBind()
    End Sub
End Class
