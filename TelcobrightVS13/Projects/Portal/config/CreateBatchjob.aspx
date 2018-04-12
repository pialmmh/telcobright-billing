<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="True"
    CodeBehind="CreateBatchjob.aspx.cs" Inherits="_DefaultBatch" %>
<%--Common--%>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>
<%----%>
<%@ Import Namespace="MediationModel" %>
<%@ Import Namespace="MySql.Data.MySqlClient" %>
<%@ Import Namespace="MySql.Data" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">
   
<%--Page Load and Other Server Side Asp.net scripts--%>
        <script runat="server">
        
            

            public DateTime FirstDayOfMonthFromDateTime(DateTime dateTime)
            {
                return new DateTime(dateTime.Year, dateTime.Month, 1);
            }

            public DateTime LastDayOfMonthFromDateTime(DateTime dateTime)
            {
                DateTime firstDayOfTheMonth = new DateTime(dateTime.Year, dateTime.Month, 1);
                return firstDayOfTheMonth.AddMonths(1).AddDays(-1);
            } 
         
        protected void DropDownListMonth_SelectedIndexChanged(object sender, EventArgs e)
            {   
                //select 15th of month to find out first and last day of a month as it exists in all months.
                DateTime AnyDayOfMonth = new DateTime(int.Parse(TextBoxYear.Text), int.Parse(DropDownListMonth.SelectedValue), 15);
                txtDate.Text= FirstDayOfMonthFromDateTime(AnyDayOfMonth).ToString("dd/MM/yyyy");
            }
         protected void DropDownListMonth1_SelectedIndexChanged(object sender, EventArgs e)
         {
             //select 15th of month to find out first and last day of a month as it exists in all months.
             DateTime AnyDayOfMonth = new DateTime(int.Parse(TextBoxYear1.Text), int.Parse(DropDownListMonth1.SelectedValue), 15);
             txtDate1.Text = LastDayOfMonthFromDateTime(AnyDayOfMonth).ToString("dd/MM/yyyy");
         }
         protected void ButtonTemplate_Click(object sender, EventArgs e)
         {
             //exit if cancel clicked in javascript...
             if (hidValueTemplate.Value == null || hidValueTemplate.Value == "")
             {
                 return;
             }

             //check for duplicate templatename and alert the client...
             string TemplateName = hidValueTemplate.Value;
             if (TemplateName == "")
             {
                 string script = "alert('Templatename cannot be empty!');";
                 ClientScript.RegisterClientScriptBlock(this.GetType(), "Alert", script, true);
                 return;
             }
             else if (TemplateName.IndexOf('=') >= 0 || TemplateName.IndexOf(':') >= 0 ||
                 TemplateName.IndexOf(',') >= 0 || TemplateName.IndexOf('?') >= 0)
             {
                 string script = "alert('Templatename cannot contain characters =:,?');";
                 ClientScript.RegisterClientScriptBlock(this.GetType(), "Alert", script, true);
                 return;
             }
             using (PartnerEntities context = new PartnerEntities())
             {
                 if (context.reporttemplates.Any(c => c.Templatename == TemplateName))
                 {
                     string script = "alert('Templatename: " + TemplateName + " exists, try a different name.');";
                     ClientScript.RegisterClientScriptBlock(this.GetType(), "Alert", script, true);
                     return;
                 }
             }
             string LocalPath = Request.Url.LocalPath;
             int Pos2ndSlash = LocalPath.Substring(1, LocalPath.Length - 1).IndexOf("/");
             string Root_Folder = LocalPath.Substring(1, Pos2ndSlash);
             int EndOfRootFolder = Request.Url.AbsoluteUri.IndexOf(Root_Folder);
             string UrlWithQueryString = "~" + Request.Url.AbsoluteUri.Substring((EndOfRootFolder + Root_Folder.Length), Request.Url.AbsoluteUri.Length - (EndOfRootFolder + Root_Folder.Length));
             int PosQMark = UrlWithQueryString.IndexOf("?");
             string UrlWithoutQS = (PosQMark < 0 ? UrlWithQueryString : UrlWithQueryString.Substring(0, PosQMark)).Replace("~","~/reports").Replace("~","~/reports");
             CommonCode CommonCode = new CommonCode();
             string RetVal = CommonCode.SaveTemplateControlsByPage(this, TemplateName, UrlWithoutQS);
             TreeView MasterTree = (TreeView)Page.Master.FindControl("Treeview1");
             CommonCode.LoadReportTemplatesTree(ref MasterTree);

             //Retrieve Path from TreeView for displaying in the master page caption label
             TreeNodeCollection cNodes = MasterTree.Nodes;
             TreeNode MatchedNode = null;
             foreach (TreeNode N in cNodes)//for each nodes at root level, loop through children
             {
                 MatchedNode = CommonCode.RetrieveNodes(N, UrlWithoutQS + "?templ=" + TemplateName);
                 if (MatchedNode != null)
                 {
                     break;
                 }
             }
             //set screentile/caption in the master page...
             Label lblScreenTitle = (Label)Master.FindControl("lblScreenTitle");
             if (MatchedNode != null)
             {
                 lblScreenTitle.Text = MatchedNode.ValuePath;
             }
             else
             {
                 lblScreenTitle.Text = "";
             }
             
             if (RetVal == "success")
             {
                 string ScrSuccess = "alert('Template created successfully');";
                 ClientScript.RegisterClientScriptBlock(this.GetType(), "Alert", ScrSuccess, true);
             }
             
         }
         
     </script> 


</asp:Content>

<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">
   
 <div id="report" style="clear:both;height:1px;background-color:white;padding-left:5px;width:1009px;margin-bottom:2px;visibility:hidden;">
    
    <script type="text/javascript">
        function ToggleParamBorderDiv() 
        {   
            var filter = document.getElementById('ParamBorder');
            if (filter.style.display == 'none') {
                filter.style.display = 'block';
                document.getElementById("<%= ShowHideFilter.ClientID %>").value = "Hide Filter";
                SetHidValueFilter("visible");
            }
            else {
                filter.style.display = 'none';
                document.getElementById("<%= ShowHideFilter.ClientID %>").value = "Show Filter";
                SetHidValueFilter("invisible");     
            }
        }
        function HideParamBorderDiv() {
            var filter = document.getElementById('ParamBorder');
            filter.style.display = 'none';
            document.getElementById("<%= ShowHideFilter.ClientID %>").value = "Show Filter";
            SetHidValueFilter("invisible");

        }

        function HideParamBorderDivSubmit() {
            var filter = document.getElementById('ParamBorder');
            filter.style.display = 'none';
            document.getElementById("<%= ShowHideFilter.ClientID %>").value = "Show Filter";
            SetHidValueFilter("invisible");

        }

        function ShowParamBorderDiv() {
            var filter = document.getElementById('ParamBorder');
            filter.style.display = 'block';
            document.getElementById("<%= ShowHideFilter.ClientID %>").value = "Hide Filter";
            SetHidValueFilter("visible");
        }

        function ShowMessage(message) {
            alert(message);
        }

        function SetHidValueFilter(value) {
            document.getElementById("<%=hidValueFilter.ClientID%>").value = value;
        }

        function SetHidValueTemplate(value) {
            document.getElementById("<%=hidValueTemplate.ClientID%>").value = value;
        }

        function SethidValueSubmitClickFlag(value) {
            document.getElementById("<%=hidValueSubmitClickFlag.ClientID%>").value = value;
        }
            
    </script>

    
    <span style="padding-left:0px;float:left;left:0px;font-weight:bold;margin-top:2px;margin-right:20px;color:Black;"> Report:</span>
    <span style="font-weight:bold;"> Source</span>
     <asp:DropDownList ID="DropDownListReportSource" runat="server">
         <asp:ListItem Value="1">CDR</asp:ListItem>
         <asp:ListItem Value="2">Summary Data</asp:ListItem>
         <asp:ListItem Value="3">Error Table</asp:ListItem>
     </asp:DropDownList>


    <asp:Button ID="submit" runat="server" Text="Show Report" onclick="submit_Click" OnClientClick="SethidValueSubmitClickFlag('true');"/> 
    <asp:Button ID="Button1" runat="server" onclick="Button1_Click" 
            style="margin-left: 0px" Text="Export" Visible="False" />
    <asp:Button ID="ShowHideFilter" runat="server" ViewStateMode="Enabled"
            style="margin-left: 0px" Text="Hide Filter" Visible="True" OnClientClick="ToggleParamBorderDiv();return false;"/>
    <asp:Button ID="ButtonTemplate" runat="server" OnClientClick="var value = prompt('Enter name of the Report Template:'); SetHidValueTemplate(value);" OnClick="ButtonTemplate_Click" 
            style="margin-left: 0px" Text="Save as Template" Visible="True"/>
    <asp:Label ID="Label1" runat="server" Text="" ForeColor="Red"></asp:Label> 
    <span style="font-weight:bold;"> Real Time Update 
            <asp:CheckBox ID="CheckBoxRealTimeUpdate" runat="server" AutoPostBack="true" oncheckedchanged="CheckBoxRealTimeUpdate_CheckedChanged"/></span>
        <span style="font-weight:bold;"> Update Duration Last  
        <asp:TextBox ID="TextBoxDuration" runat="server" Text="30" Width="30px" ontextchanged="TextBoxDuration_TextChanged" Enabled="false" ></asp:TextBox> Minutes</span>  
        <input type="hidden" id="hidValueFilter" runat="server"/>
        <input type="hidden" id="hidValueSubmitClickFlag" runat="server" value="false"/>
        <input type="hidden" id="hidValueTemplate" runat="server" />
 </div>   

<div>
    <ajaxToolkit:ToolkitScriptManager ID="ToolkitScriptManager1" runat="server" > </ajaxToolkit:ToolkitScriptManager>
    
</div>
 
 <div id="ParamBorder" style="margin-top:-20px; float:left;padding-top:5px;padding-left:10px;height:140px;display:block;border:2px ridge #E5E4E2;margin-bottom:5px;width:1300px;">
    <div style="height:25px;background-color:#f2f2f2;color:black;"> 
        <span style="float:left;font-weight:bold;padding-left:20px;">   
        
        Job Name: 
     <asp:TextBox ID="TextBoxJobName" runat="server" Width="200px"></asp:TextBox>
     
        
         Job Type 
        <asp:DropDownList ID="DropDownListJobType" runat="server">
         
     </asp:DropDownList>
    
    Batch Size: 
     <asp:TextBox ID="TextBoxBatchSize" runat="server" Text="5000"></asp:TextBox>
     
                
            
        <asp:CheckBox ID="CheckBoxShowPerformance" runat="server" Checked="true" Visible="false" /></span> 
        <%--<div style="clear:right;"></div>--%>
        <span style="float:left;font-weight:bold;padding-left:20px;">   <asp:CheckBox ID="CheckBoxShowCost" runat="server" Checked="true" Visible="false" /></span> 
    </div>
    <br />
    <%--date time div--%>
     <div id="DateTimeDiv" style="padding-left:5px;position:relative;float:left;left:10px;top:-11px;width:630px;z-index:10;background-color:#F7F6F3;height:70px;"> <%--Start OF date time/months field DIV--%>
     <span style="padding-left:0px;">
        
        Start Year/Month: 
        <asp:TextBox ID="TextBoxYear" runat="server" Text="" Width="30px"></asp:TextBox>
        Month <asp:DropDownList ID="DropDownListMonth" runat="server" 
          onselectedindexchanged="DropDownListMonth_SelectedIndexChanged" 
          AutoPostBack="True">
          <asp:ListItem Value="01">Jan</asp:ListItem>
          <asp:ListItem Value="02">Feb</asp:ListItem>
          <asp:ListItem Value="03">Mar</asp:ListItem>
          <asp:ListItem Value="04">Apr</asp:ListItem>
          <asp:ListItem Value="05">May</asp:ListItem>
          <asp:ListItem Value="06">Jun</asp:ListItem>
          <asp:ListItem Value="07">Jul</asp:ListItem>
          <asp:ListItem Value="08">Aug</asp:ListItem>
          <asp:ListItem Value="09">Sep</asp:ListItem>
          <asp:ListItem Value="10">Oct</asp:ListItem>
          <asp:ListItem Value="11">Nov</asp:ListItem>
          <asp:ListItem Value="12">Dec</asp:ListItem>
      </asp:DropDownList>

      End Year/Month: 
        <asp:TextBox ID="TextBoxYear1" runat="server" Text="" Width="30px"></asp:TextBox>
        Month <asp:DropDownList ID="DropDownListMonth1" runat="server" 
          onselectedindexchanged="DropDownListMonth1_SelectedIndexChanged" AutoPostBack="True">
          <asp:ListItem Value="01">Jan</asp:ListItem>
          <asp:ListItem Value="02">Feb</asp:ListItem>
          <asp:ListItem Value="03">Mar</asp:ListItem>
          <asp:ListItem Value="04">Apr</asp:ListItem>
          <asp:ListItem Value="05">May</asp:ListItem>
          <asp:ListItem Value="06">Jun</asp:ListItem>
          <asp:ListItem Value="07">Jul</asp:ListItem>
          <asp:ListItem Value="08">Aug</asp:ListItem>
          <asp:ListItem Value="09">Sep</asp:ListItem>
          <asp:ListItem Value="10">Oct</asp:ListItem>
          <asp:ListItem Value="11">Nov</asp:ListItem>
          <asp:ListItem Value="12">Dec</asp:ListItem>
      </asp:DropDownList>

      </span>


    <div style="float:left;width:280px;">
        Start Date [Time] <asp:TextBox id="txtDate" Runat="server" /> 
        <asp:CalendarExtender ID="CalendarStartDate" runat="server" 
            TargetControlID="txtDate"  PopupButtonID="txtDate" Format="dd/MM/yyyy">
        </asp:CalendarExtender>     
        
        
    </div>
    
    <div style="float:left;width:280px;">
        End Date [Time] <asp:TextBox id="txtDate1" Runat="server" />
        <asp:CalendarExtender ID="CalendarEndDate" runat="server" 
            TargetControlID="txtDate1"  PopupButtonID="txtDate1" Format="dd/MM/yyyy">
        </asp:CalendarExtender>     
    </div>
   
    <div style="font-size:smaller;text-align:left;overflow:visible;clear:left;color:#8B4500;">[Enter only Date in "dd/MM/yyyy (e.g. 21/11/2012) or Date+Time in "dd/MM/yyyy HH:mm:ss" (e.g. 21/11/2012 19:01:59) format]</div>

</div> <%--END OF date time/months field DIV--%>
<div id="TimeSummary" style="float:left;margin-left:15px;padding-left:20px;height:70px;width: 280px;background-color: #faebd7;margin-top: -10px;visibility:hidden;">
    <div style="font-weight:bold;float:left;">Period wise Summary<asp:CheckBox ID="CheckBoxDailySummary" runat="server" /></div> 
    <div style="clear:left;margin-top:5px;"></div>
    <div style="float:left; margin-right:5px;">
        <asp:RadioButton ID="RadioButtonHalfHourly" Text="Half Hourly" GroupName="Time" runat="server" AutoPostBack="false"/><br />
        <asp:RadioButton ID="RadioButtonHourly" Text="Hourly" GroupName="Time" runat="server" AutoPostBack="false"/>
    </div>
    <div style="float:left;margin-right:5px;">
        <asp:RadioButton ID="RadioButtonDaily" Text="Daily" GroupName="Time" runat="server" AutoPostBack="false" Checked="true"/><br />
        <asp:RadioButton ID="RadioButtonWeekly" Text="Weekly" GroupName="Time" runat="server" AutoPostBack="false" Checked="false"/><br />
    </div>
    <div style="float:left;">
        <asp:RadioButton ID="RadioButtonMonthly" Text="Monthly" GroupName="Time" runat="server" AutoPostBack="false" Checked="false"/><br />
        <asp:RadioButton ID="RadioButtonYearly" Text="Yearly" GroupName="Time" runat="server" AutoPostBack="false"/>
    </div>
</div>

     <div style="clear: left; padding-top: 0px; margin-left: 10px;">
         <div style="float: left; height: 25px; text-align: left; margin-right: 5px;">

             <span style="font-weight: bold;">Switch: &nbsp </span>
             <asp:DropDownList ID="DropDownListSwitch" runat="server">
             </asp:DropDownList>
             <span style="font-weight: bold;">Error Code: &nbsp </span>
             <asp:DropDownList ID="DropDownListError" runat="server">
             </asp:DropDownList>
             <span style="font-weight: bold;">Incoming Route: &nbsp </span>
             <asp:DropDownList ID="DropDownListInRoute" runat="server">
             </asp:DropDownList>

             <%-- <div style="width: 750px;padding-left:28px;">
                   
                        <span style="width:300px;padding-left:127px;">&nbsp;</span>
                        
                        <span style="width:50px;padding-left:100px;">&nbsp;</span>
                           
                       
                </div>--%>
         </div>



         <asp:Button ID="ButtonCreateJob" runat="server" Text="Create Job"
             OnClick="ButtonCreateJob_Click" />
     </div>
         <br />
     <asp:Label ID="lblJobStatus" runat="server" Text=""></asp:Label>
     
     
     <%--<asp:LinkButton ID="LinkButton1" runat="server" PostBackUrl="~/reports/MediationBatch.aspx">View Batch Jobs</asp:LinkButton>--%>


     
          
</div> <%--Param Border--%>
<%--Common--%>

<asp:SqlDataSource ID="SqlDataSource4" runat="server" 
              ConnectionString="<%$ ConnectionStrings:Reader %>" 
              ProviderName="<%$ ConnectionStrings:Reader.ProviderName %>" 
              SelectCommand="AllAns" SelectCommandType="StoredProcedure">
            </asp:SqlDataSource>

 <asp:SqlDataSource ID="SqlDataSource3" runat="server" 
                 ConnectionString="<%$ ConnectionStrings:Reader %>" 
                ProviderName="<%$ ConnectionStrings:Reader.ProviderName %>" 
                SelectCommand="AllICX" SelectCommandType="StoredProcedure">
              </asp:SqlDataSource>
     
     




</asp:Content>

