<%@ Page Language="VB" AutoEventWireup="false" CodeBehind="Default.aspx.vb" Inherits="DefaultAspx" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head id="Head1" runat="server">
	<title>Create Data Table</title>
   <style type="text/css">
<!--
body {
	margin-left: 0px;
	margin-top: 0px;
	margin-right: 0px;
	margin-bottom: 0px;
}
a:link {
	color: #0000FF;
}
a:visited {
	color: #0000FF;
}
a:hover {
	color: #0000FF;
	text-decoration: none;
}
a:active {
	color: #0000FF;
	}
.basix {
	font-family: Verdana, Arial, Helvetica, sans-serif;
	font-size: 11px;
}
.header1 {
	font-family: Verdana, Arial, Helvetica, sans-serif;
	font-size: 11px;
	font-weight: bold;
	color: #006699;
}
.lgHeader1 {
	font-family: Arial, Helvetica, sans-serif;
	font-size: 18px;
	font-weight: bold;
	color: #0066CC;
	background-color: #CEE9FF;
}
-->
</style> 
</head>
<body>
	<form id="form1" runat="server">
	<div>
	<fieldset style="height:200px;">
		<br />
		<table align="center" border="0" cellpadding="0" cellspacing="0" style="position: static"
			width="752">
			<tr bgcolor="#5482fc">
				<td colspan="4">
					<img height="1" src="/media/spacer.gif" width="1" /></td>
			</tr>
			<tr>
				<td bgcolor="#5482fc" width="1">
					<img alt="Server Intellect" height="1" src="media/spacer.gif" width="1" /></td>
				<td width="250">
					<a href="http://www.serverintellect.com">
						<img alt="Server Intellect" border="0" height="75" src="media/logo.gif" width="250" /></a></td>
				<td bgcolor="#3399ff" width="500">
					<a href="http://www.serverintellect.com">
						<img alt="Server Intellect" border="0" height="75" src="media/headerR1.gif" width="500" /></a></td>
				<td bgcolor="#5482fc" width="1">
					<img alt="Server Intellect" height="1" src="media/spacer.gif" width="1" /></td>
			</tr>
			<tr bgcolor="#5482fc">
				<td colspan="4">
					<img height="1" src="media/spacer.gif" width="1" /></td>
			</tr>
		</table>
		<br />
		<legend>Create Define Table(Unuse any DataBase) </legend>
		<asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="False" Width="346px" BackColor="White" BorderColor="#CCCCCC" BorderStyle="None" BorderWidth="1px" CellPadding="3" EmptyDataText="There is no any input." ForeColor="Red">
			<Columns>
			<asp:CommandField ShowSelectButton="True" ShowEditButton="True" />
				<asp:BoundField DataField="id" HeaderText="ID" Visible="False" />
				<asp:BoundField DataField="username" HeaderText="UserName" />
				<asp:BoundField DataField="firstname" HeaderText="First Name" />
				<asp:BoundField DataField="lastname" HeaderText="Last Name" />
			</Columns>
			<FooterStyle BackColor="White" ForeColor="#000066" />
			<RowStyle ForeColor="#000066" />
			<SelectedRowStyle BackColor="#669999" Font-Bold="True" ForeColor="White" />
			<PagerStyle BackColor="White" ForeColor="#000066" HorizontalAlign="Left" />
			<HeaderStyle BackColor="#006699" Font-Bold="True" ForeColor="White" />
		</asp:GridView>
		<br />
		<legend>Add new user</legend>
		 <table style="width: 360px">
			<tr>
				<td style="width: 120px" >
					username:</td>
				<td style="width: 18px">
					<asp:TextBox ID="txtUserName" runat="server"></asp:TextBox></td>                
			</tr>
			<tr>
				<td style="width: 120px">
					first name:</td>
				<td style="width: 18px">
					<asp:TextBox ID="txtFirstName" runat="server"></asp:TextBox></td>                
			</tr>
			<tr>
				<td style="width: 120px; height: 21px;">
					last name:</td>
				<td style="width: 18px; height: 21px;">
					<asp:TextBox ID="txtLastName" runat="server" Width="150px"></asp:TextBox></td>               
			</tr>
			 <tr>
				 <td colspan="2" align="center">
					 <asp:Button ID="btnAdd" runat="server" Text="Add new user" />
					 <asp:Label ID="lblTips" runat="server" ForeColor="Red"></asp:Label></td>
				
			 </tr>
		</table>
	</fieldset>    
	</div>
		<br />
		<table align="center" cellpadding="0" cellspacing="0" style="position: static" width="500">
			<tr>
				<td align="center" class="basix" height="50">
					<strong>Power. Stability. Flexibility.</strong><br />
					Hosting from <a href="http://www.serverintellect.com">Server Intellect</a><br />
					<br />
					For more ASP.NET Tutorials visit <a href="http://www.AspNetTutorials.com">www.AspNetTutorials.com</a></td>
			</tr>
		</table>
	   
	</form>
</body>
</html>