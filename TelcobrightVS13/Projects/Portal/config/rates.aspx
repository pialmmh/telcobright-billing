<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="True"
    CodeBehind="rates.aspx.cs" Inherits="DefaultRates" %>
<%--Common--%>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>
<%@ Import Namespace="MediationModel" %>
<%@ Import Namespace="PortalApp" %>
<%@ Import Namespace="PortalApp._portalHelper" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">
   
    <script src="../Scripts/moment.js" type="text/javascript"></script>

<%--Page Load and Other Server Side Asp.net scripts--%>
        <script runat="server">
        
            protected void Page_Load(object sender, EventArgs e)
            {
                //common code for report pages
                //view state of ParamBorder div
                string tempText = this.hidValueFilter.Value;
                bool lastVisible = this.hidValueFilter.Value == "invisible" ? false : true;
                if (this.hidValueSubmitClickFlag.Value == "false")
                {
                    if (lastVisible)
                    {
                        //show filters...
                        this.Page.ClientScript.RegisterStartupScript(GetType(), "MyKey", "ShowParamBorderDiv();", true);
                    }
                    else
                    {
                        //hide filters...
                        this.Page.ClientScript.RegisterStartupScript(GetType(), "MyKey", "HideParamBorderDiv();", true);
                    }
                }
                //set this month's start and End Date [Time] in the date picker controls...
                if (!this.IsPostBack)
                {
                    //set summary as report source default
                    this.DropDownListReportSource.SelectedIndex = 1;

                    this.TextBoxYear.Text = DateTime.Now.ToString("yyyy");
                    this.TextBoxYear1.Text = DateTime.Now.ToString("yyyy");
                    this.DropDownListMonth.SelectedIndex = int.Parse(DateTime.Now.ToString("MM")) - 1;
                    this.DropDownListMonth1.SelectedIndex = int.Parse(DateTime.Now.ToString("MM")) - 1;
                    //txtDate.Text = FirstDayOfMonthFromDateTime(System.DateTime.Now).ToString("dd/MM/yyyy");
                    //txtDate1.Text = LastDayOfMonthFromDateTime(System.DateTime.Now).ToString("dd/MM/yyyy");

                    this.txtDate1.Text = DateTime.Now.ToString("yyyy-MM-dd");
                    this.txtDate.Text = DateTime.Now.AddMonths(-3).ToString("yyyy-MM-dd");
                    
                    //set controls if page is called for a template
                    TreeView masterTree = (TreeView) this.Master.FindControl("TreeView1");
                    NameValueCollection n = this.Request.QueryString;
                    CommonCode commonCodes = new CommonCode();
                    if (n.HasKeys())
                    {
                        string templateName = "";
                        var items = n.AllKeys.SelectMany(n.GetValues, (k, v) => new { key = k, value = v });
                        foreach (var thisParam in items)
                        {
                            if (thisParam.key == "templ")
                            {
                                templateName = thisParam.value;
                                break;
                            }
                        }
                        if (templateName != "")
                        {
                            //set controls here ...
                            string retVal = commonCodes.SetTemplateControls(this, templateName);
                            if (retVal != "success")
                            {
                                string script = "alert('Error occured while loading template: " + templateName
                                    + "! " + Environment.NewLine + retVal + "');";
                                this.ClientScript.RegisterClientScriptBlock(GetType(), "Alert", script, true);
                                return;
                            }
                        }
                        //Load Report Templates in TreeView dynically from database.
                        CommonCode commonCode = new CommonCode();
                        commonCode.LoadReportTemplatesTree(ref masterTree);
                    }
                    //Retrieve Path from TreeView for displaying in the master page caption label
                    string localPath = this.Request.Url.LocalPath;
                    int pos2NdSlash = localPath.Substring(1, localPath.Length - 1).IndexOf("/");
                    string rootFolder = localPath.Substring(1, pos2NdSlash);
                    int endOfRootFolder = this.Request.Url.AbsoluteUri.IndexOf(rootFolder);
                    string urlWithQueryString = ("~" +"/"+rootFolder + this.Request.Url.AbsoluteUri.Substring((endOfRootFolder + rootFolder.Length), this.Request.Url.AbsoluteUri.Length - (endOfRootFolder + rootFolder.Length))).Replace("%20", " ");
                    TreeNodeCollection cNodes = masterTree.Nodes;
                    TreeNode matchedNode = null;
                    foreach (TreeNode N in cNodes)//for each nodes at root level, loop through children
                    {
                        matchedNode = commonCodes.RetrieveNodes(N, urlWithQueryString);
                        if (matchedNode != null)
                        {
                            break;
                        }
                    }
                    //set screentile/caption in the master page...
                    Label lblScreenTitle = (Label) this.Master.FindControl("lblScreenTitle");
                    if (matchedNode != null)
                    {
                        lblScreenTitle.Text = matchedNode.ValuePath;
                    }
                    else
                    {
                        lblScreenTitle.Text = "";
                    }


                    //End of Site Map Part *******************************************************************

                }

                this.hidValueSubmitClickFlag.Value = "false";
            }

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
                DateTime anyDayOfMonth = new DateTime(int.Parse(this.TextBoxYear.Text), int.Parse(this.DropDownListMonth.SelectedValue), 15);
            this.txtDate.Text= FirstDayOfMonthFromDateTime(anyDayOfMonth).ToString("yyyy-MM-dd");
            }
         protected void DropDownListMonth1_SelectedIndexChanged(object sender, EventArgs e)
         {
             //select 15th of month to find out first and last day of a month as it exists in all months.
             DateTime anyDayOfMonth = new DateTime(int.Parse(this.TextBoxYear1.Text), int.Parse(this.DropDownListMonth1.SelectedValue), 15);
             this.txtDate1.Text = LastDayOfMonthFromDateTime(anyDayOfMonth).AddDays(1).ToString("yyyy-MM-dd");
         }
         protected void ButtonTemplate_Click(object sender, EventArgs e)
         {
             //exit if cancel clicked in javascript...
             if (this.hidValueTemplate.Value == null || this.hidValueTemplate.Value == "")
             {
                 return;
             }

             //check for duplicate templatename and alert the client...
             string templateName = this.hidValueTemplate.Value;
             if (templateName == "")
             {
                 string script = "alert('Templatename cannot be empty!');";
                 this.ClientScript.RegisterClientScriptBlock(GetType(), "Alert", script, true);
                 return;
             }
             else if (templateName.IndexOf('=') >= 0 || templateName.IndexOf(':') >= 0 ||
                 templateName.IndexOf(',') >= 0 || templateName.IndexOf('?') >= 0)
             {
                 string script = "alert('Templatename cannot contain characters =:,?');";
                 this.ClientScript.RegisterClientScriptBlock(GetType(), "Alert", script, true);
                 return;
             }

             
             
             using (PartnerEntities context = new PartnerEntities())
             {
                 if (context.reporttemplates.Any(c => c.Templatename == templateName))
                 {
                     string script = "alert('Templatename: " + templateName + " exists, try a different name.');";
                     this.ClientScript.RegisterClientScriptBlock(GetType(), "Alert", script, true);
                     return;
                 }
             }
             string localPath = this.Request.Url.LocalPath;
             int pos2NdSlash = localPath.Substring(1, localPath.Length - 1).IndexOf("/");
             string rootFolder = localPath.Substring(1, pos2NdSlash);
             int endOfRootFolder = this.Request.Url.AbsoluteUri.IndexOf(rootFolder);
             string urlWithQueryString = "~" + this.Request.Url.AbsoluteUri.Substring((endOfRootFolder + rootFolder.Length), this.Request.Url.AbsoluteUri.Length - (endOfRootFolder + rootFolder.Length));
             int posQMark = urlWithQueryString.IndexOf("?");
             string urlWithoutQs = (posQMark < 0 ? urlWithQueryString : urlWithQueryString.Substring(0, posQMark)).Replace("~","~/reports").Replace("~","~/reports");
             CommonCode commonCode = new CommonCode();
             string retVal = commonCode.SaveTemplateControlsByPage(this, templateName, urlWithoutQs);
             TreeView masterTree = (TreeView) this.Page.Master.FindControl("Treeview1");
             commonCode.LoadReportTemplatesTree(ref masterTree);
             
             //Retrieve Path from TreeView for displaying in the master page caption label
             TreeNodeCollection cNodes = masterTree.Nodes;
             TreeNode matchedNode = null;
             foreach (TreeNode n in cNodes)//for each nodes at root level, loop through children
             {
                 matchedNode = commonCode.RetrieveNodes(n, urlWithoutQs + "?templ=" + templateName);
                 if (matchedNode != null)
                 {
                     break;
                 }
             }
             //set screentile/caption in the master page...
             Label lblScreenTitle = (Label) this.Master.FindControl("lblScreenTitle");
             if (matchedNode != null)
             {
                 lblScreenTitle.Text = matchedNode.ValuePath;
             }
             else
             {
                 lblScreenTitle.Text = "";
             }
             
             if (retVal == "success")
             {
                 string scrSuccess = "alert('Template created successfully');";
                 this.ClientScript.RegisterClientScriptBlock(GetType(), "Alert", scrSuccess, true);
             }
             
         }
         
     </script> 


</asp:Content>

<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">
   
 <div id="report" style="clear:both;height:0px;background-color:white;padding-left:5px;width:1009px;margin-bottom:2px;">
    </div>
    <script type="text/javascript">
        function ToggleParamBorderDiv() 
        {   
            var filter = document.getElementById('ParamBorder');
            if (filter.style.display == 'none') {
                filter.style.display = 'block';
                document.getElementById("<%= this.ShowHideFilter.ClientID %>").value = "Hide Filter";
                SetHidValueFilter("visible");
            }
            else {
                filter.style.display = 'none';
                document.getElementById("<%= this.ShowHideFilter.ClientID %>").value = "Show Filter";
                SetHidValueFilter("invisible");     
            }
        }
        function HideParamBorderDiv() {
            var filter = document.getElementById('ParamBorder');
            filter.style.display = 'none';
            document.getElementById("<%= this.ShowHideFilter.ClientID %>").value = "Show Filter";
            SetHidValueFilter("invisible");

        }

        function HideParamBorderDivSubmit() {
            var filter = document.getElementById('ParamBorder');
            filter.style.display = 'none';
            document.getElementById("<%= this.ShowHideFilter.ClientID %>").value = "Show Filter";
            SetHidValueFilter("invisible");

        }

        function ShowParamBorderDiv() {
            var filter = document.getElementById('ParamBorder');
            filter.style.display = 'block';
            document.getElementById("<%= this.ShowHideFilter.ClientID %>").value = "Hide Filter";
            SetHidValueFilter("visible");
        }

        function ShowMessage(message) {
            alert(message);
        }

        function SetHidValueFilter(value) {
            document.getElementById("<%= this.hidValueFilter.ClientID%>").value = value;
        }

        function SetHidValueTemplate(value) {
            document.getElementById("<%= this.hidValueTemplate.ClientID%>").value = value;
        }

        function SethidValueSubmitClickFlag(value) {
            document.getElementById("<%= this.hidValueSubmitClickFlag.ClientID%>").value = value;
        }
            
    </script>


    
    <span style="width:50px;padding-left:50px;"></span>
        
    <div style="visibility:hidden;">
    <span style="padding-left:0px;float:left;left:0px;font-weight:bold;margin-top:2px;margin-right:20px;color:Black;"> Report:</span>
    <span style="font-weight:bold;"> Source</span>
     <asp:DropDownList ID="DropDownListReportSource" runat="server">
         <asp:ListItem Value="1">CDR</asp:ListItem>
         <asp:ListItem Value="2">Summary Data</asp:ListItem>
         <asp:ListItem Value="3">Error Table</asp:ListItem>
     </asp:DropDownList>


    <asp:Button ID="submit" runat="server" Text="Show Report" OnClientClick="SethidValueSubmitClickFlag('true');"/> 
    <asp:Button ID="Button1" runat="server" onclick="Button1_Click" 
            style="margin-left: 0px" Text="Export" Visible="False" />
    <asp:Button ID="ShowHideFilter" runat="server" ViewStateMode="Enabled"
            style="margin-left: 0px" Text="Hide Filter" Visible="True" OnClientClick="ToggleParamBorderDiv();return false;"/>
    <asp:Button ID="ButtonTemplate" runat="server" OnClientClick="var value = prompt('Enter name of the Report Template:'); SetHidValueTemplate(value);" OnClick="ButtonTemplate_Click" 
            style="margin-left: 0px" Text="Save as Template" Visible="True"/>
    <asp:Label ID="Label1" runat="server" Text="" ForeColor="Red"></asp:Label> 
    <span style="font-weight:bold;"> Real Time Update 
            <asp:CheckBox ID="CheckBoxRealTimeUpdate" runat="server" AutoPostBack="true" Visible="false" /></span>
        <span style="font-weight:bold;"> Update Duration Last  
        <asp:TextBox ID="TextBoxDuration" runat="server" Text="30" Width="30px" ontextchanged="TextBoxDuration_TextChanged" Enabled="false" ></asp:TextBox> Minutes</span>  

        </div>

        <input type="hidden" id="hidValueFilter" runat="server"/>
        <input type="hidden" id="hidValueSubmitClickFlag" runat="server" value="false"/>
        <input type="hidden" id="hidValueTemplate" runat="server" />
 

<div>
    <ajaxToolkit:ToolkitScriptManager ID="ToolkitScriptManager1" runat="server" > </ajaxToolkit:ToolkitScriptManager>
    
</div>
 
 <asp:SqlDataSource ID="SqlDataRatePlan" runat="server" 
        ConnectionString="<%$ ConnectionStrings:Reader %>" 
        ProviderName="<%$ ConnectionStrings:Reader.ProviderName %>" 
        SelectCommand="select (select -1) as id, (select ' All') as rateplanname union all select id, rateplanname from rateplan"></asp:SqlDataSource>

    <asp:SqlDataSource ID="SqlDataservice" runat="server" 
        ConnectionString="<%$ ConnectionStrings:Reader %>" 
        ProviderName="<%$ ConnectionStrings:Reader.ProviderName %>" 
        SelectCommand="select (select -1) as id,(select '[ All]') as name 
union all 
(select sf.id, concat(servicename,' [',sc.type,']') as name from enumservicefamily sf
 left join enumservicecategory sc
 on sf.servicecategory=sc.id) "></asp:SqlDataSource>


<asp:Label ID="lblRateGlobal" runat="server" Text="" Visible="false"></asp:Label>

 <div id="ParamBorder" style="margin-top:-35px;float:left;padding-top:0px;padding-left:0px;height:197px;display:block;border:2px ridge #E5E4E2;margin-bottom:5px;width:1500px;">
    <%--  --%>
    <%--<div style="margin-left:10px;float:left;left:0px;font-weight:bold;margin-top:2px;margin-right:20px;color:Black;clear:both;"> Rates</div>--%>

    <div style="height:2px;clear:both;"></div>
    <%--date time div--%>
     
      <asp:UpdatePanel ID="UpdatePanel2" runat="server">
            
            <ContentTemplate>
            
     <div id="DateTimeDiv" style="padding-top:2px;text-align:left;padding-left:15px;background-color:#f2f2f2;height:90px;min-width:1300px;"> <%--Start OF date time/months field DIV--%>
        
        <div style="font-weight:bold;">Select a period over which rates are effective</div>

        <div style="float:left;padding-top:4px;">

        <div>

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
      <span style="width:50px;padding-left:50px;"></span>
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

      
      </div>
        <div style="clear:left;"></div>
        <div style="float:left;width:290px; padding-top:5px;">
       <span style="padding-left:2.5px;"> 
           
           Start Date [Time]:

       </span> 
            
            <asp:TextBox id="txtDate" Runat="server" /> 
        <asp:CalendarExtender ID="CalendarStartDate" runat="server" 
            TargetControlID="txtDate"  PopupButtonID="txtDate" Format="yyyy-MM-dd">
        </asp:CalendarExtender>     
    </div>
    
        <div style="float:left;padding-top:5px;">
        <span style="padding-left:10px;">End Date [Time]:</span>  <asp:TextBox id="txtDate1" Runat="server" /> <span style="padding-left:0px;font-weight:bold;">[*Valid Before]</span>
        <asp:CalendarExtender ID="CalendarEndDate" runat="server" 
            TargetControlID="txtDate1"  PopupButtonID="txtDate1" Format="yyyy-MM-dd">
        </asp:CalendarExtender> 
    </div>

             <div style="font-size:smaller;text-align:left;overflow:visible;clear:left;color:#8B4500;padding-top:4px;">
        [Enter only Date in "yyyy-MM-dd (e.g. 2012-11-21) or Date+Time in "yyyy-MM-dd HH:mm:ss" (e.g. 2012-11-21 19:01:59) format]</div>

   
        </div>
        
          <script type="text/javascript">
              function DateSelect() 
              {   
                  var starttxt = document.getElementById("<%= this.txtDate.ClientID %>");
                  var endtxt = document.getElementById("<%= this.txtDate1.ClientID %>");

                  var recent = document.getElementById("<%= this.RadioButtonHalfHourly.ClientID %>");
                  var today = document.getElementById("<%= this.RadioButtonHourly.ClientID %>");

                  var last7 = document.getElementById("<%= this.RadioButtonDaily.ClientID %>");
                  var last30 = document.getElementById("<%= this.RadioButtonWeekly.ClientID %>");

                  var next7 = document.getElementById("<%= this.RadioButtonMonthly.ClientID %>");
                  var next30 = document.getElementById("<%= this.RadioButtonYearly.ClientID %>");

                  var last6m = document.getElementById("<%= this.RadioButtonLastSixm.ClientID %>");
                  var next6m = document.getElementById("<%= this.RadioButtonNextSixm.ClientID %>");

                  var last1y = document.getElementById("<%= this.RadioButtonLastY.ClientID %>");
                  var next1y = document.getElementById("<%= this.RadioButtonNextY.ClientID %>");

                  var now = moment();
                  var Addition = 0;
                  var Substraction = 0;
                  
                  if (recent.checked === true)
                  {
                      //last one month to any date in future
                      Addition = 31;
                      Substraction = 30;
                  }
                  else if (today.checked === true)
                  {
                      Addition = 1;
                      Substraction = 0;
                  }
                  else if (last7.checked === true) {
                      Addition = 1;
                      Substraction = 7;
                  }
                  else if (last30.checked === true) {
                      Addition = 1;
                      Substraction = 30;
                  }
                  else if (next7.checked === true) {
                      Addition = 8;
                      Substraction = 0;
                  }
                  else if (next30.checked === true) {
                      Addition = 31;
                      Substraction = 0;
                  }
                  else if (last6m.checked === true) {
                      Addition = 1;
                      Substraction = 180;
                  }
                  else if (next6m.checked === true) {
                      Addition = 181;
                      Substraction = 0;
                  }
                  else if (last1y.checked === true) {
                      Addition = 1;
                      Substraction = 365;
                  }
                  else if (next1y.checked === true) {
                      Addition = 365;
                      Substraction = 0;
                  }
                  var startdate = moment(now).subtract(Substraction, 'days');
                  var enddate = moment(now).add(Addition, 'days');

                  starttxt.value = startdate.format("YYYY-MM-DD");
                  endtxt.value = enddate.format("YYYY-MM-DD");

                  return false;

              }
            </script>

    <div id="TimeSummary" style="float:left;margin-left:15px;padding-left:20px;height:70px;width: 545px;background-color: #faebd7;margin-top: -7px;">
        <div style="font-weight:bold;float:left;">Quick Select Period<asp:CheckBox ID="CheckBoxDailySummary" Visible="false" runat="server" />
            <span style="padding-left:10px;"><asp:CheckBox ID="CheckBoxAllTime" runat="server" 
                Text="All Period when viewing one Rate Plan" /></span>
        </div> 
        <div style="clear:left;margin-top:5px;"></div>
        <div style="float:left; margin-right:5px;">
            <asp:RadioButton onclick="DateSelect();" ID="RadioButtonHalfHourly" Checked="true" Text="Recent" GroupName="Time" runat="server" AutoPostBack="false"/><br />
            <asp:RadioButton onclick="DateSelect();"  ID="RadioButtonHourly" Text="Today" GroupName="Time" runat="server" AutoPostBack="false"/>
        </div>
        <div style="float:left;margin-right:5px;">
            <asp:RadioButton onclick="DateSelect();"   ID="RadioButtonDaily" Text="Last 7 days" GroupName="Time" runat="server" AutoPostBack="false" Checked="false"/><br />
            <asp:RadioButton onclick="DateSelect();"   ID="RadioButtonWeekly" Text="Last 30 days" GroupName="Time" runat="server" AutoPostBack="false" Checked="false"/><br />
        </div>
        <div style="float:left;">
            <asp:RadioButton onclick="DateSelect();"   ID="RadioButtonMonthly" Text="Next 7 days" GroupName="Time" runat="server" AutoPostBack="false" Checked="false"/><br />
            <asp:RadioButton onclick="DateSelect();"   ID="RadioButtonYearly" Text="Next 30 days" GroupName="Time" runat="server" AutoPostBack="false"/>
        </div>
        <div style="float:left;">
            <asp:RadioButton onclick="DateSelect();"   ID="RadioButtonLastSixm" Text="Last 6 Months" GroupName="Time" runat="server" AutoPostBack="false" Checked="false"/><br />
            <asp:RadioButton onclick="DateSelect();"   ID="RadioButtonNextSixm" Text="Next 6 Months" GroupName="Time" runat="server" AutoPostBack="false"/>
        </div>
         <div style="float:left;">
            <asp:RadioButton onclick="DateSelect();"   ID="RadioButtonLastY" Text="Last 1 Year" GroupName="Time" runat="server" AutoPostBack="false" Checked="false"/><br />
            <asp:RadioButton onclick="DateSelect();"   ID="RadioButtonNextY" Text="Next 1 Year" GroupName="Time" runat="server" AutoPostBack="false"/>
        </div>
    </div>    

   
</div> <%--END OF date time/months field DIV--%>
                </ContentTemplate>
          </asp:UpdatePanel>
       
       <div id="PartnerFilter" style="height:100px;margin-bottom:5px;padding-top:0px; width:1400px; margin-top:0px;margin-left:0px;float:left;padding-left:5px;background-color:#F7F6F3;">
           
        
        <asp:UpdatePanel ID="UpdatePanel1" runat="server">
            
            <ContentTemplate>
            
                <div style="float:left;padding-left: 0px; margin-top: 0px; width: 120px; text-align: right;">
                    Service: 
                <div style="clear:left;height:7px;"></div>
                    Rate Plan:
                    <div style="clear:left;height:8px;"></div>
                    Prefix[*]:
                    <div style="clear:left;height:7px;"></div>
                    Description Having:
                 </div>
                
                <div style="text-align:left;float:left;margin-left:5px;">
                    
                    <asp:DropDownList ID="DropDownListservice" runat="server" AutoPostBack="true"
                        DataSourceID="SqlDataservice" DataTextField="Name"
                        DataValueField="id" Enabled="true" OnSelectedIndexChanged="ddlservice_SelectedIndexChanged">
                    </asp:DropDownList>
                    
                    
                    <div style="clear:left;height:5px;"></div>
                    
                        <asp:DropDownList ID="DropDownListRatePlan" runat="server"
                        Enabled="true" AutoPostBack="true"
                        DataSourceID="SqlDataRatePlan" DataTextField="RatePlanName" OnSelectedIndexChanged="ddlRateplan_SelectedIndexChanged"
                        DataValueField="id">
                        </asp:DropDownList>
                        
                    <asp:LinkButton ID="LinkButtonRate" runat="server">(New Rate)</asp:LinkButton>
                    &nbsp<asp:LinkButton ID="LinkButtonDelete" runat="server" OnClick="LinkButtonDelete_Click">(Delete Rates)</asp:LinkButton>
                    
                    
                <div style="height:5px;"></div>    
                
                
                     <asp:TextBox ID="TextBoxPrefix" runat="server"></asp:TextBox>
                <div style="height:5px;"></div>    
                    <asp:TextBox ID="TextBoxDescription" runat="server"></asp:TextBox>
                </div>

                  <div style="float:left;padding-left: 5px; margin-top: 0px; min-width: 300px; text-align: left;">
  
            

   </div>
            <div style="float:left;padding-left: 10px; margin-top: 0px; min-width: 300px; text-align: left;">
                <div style="text-align:right;float:left;">
                    Partner:
                    <div style="clear:left;"></div>    
                    <div style="height:7px;"></div>
                    Route:
                    <div style="clear:left;"></div>    
                    <div style="height:8px;"></div>
                    Assigned Direction:
                    <div style="clear:left;"></div>
                    <div style="height:7px;"></div>    
                    Assigned Order [0=All]:
                </div>
                    
                <div style="text-align:left;min-width:300px;float:left;margin-left:5px;">
                    <asp:DropDownList ID="DropDownListPartner" runat="server" AutoPostBack="true"
                        DataSourceID="SqlDataSource1" DataTextField="PartnerName" 
                        DataValueField="idPartner" Enabled="false" OnSelectedIndexChanged="DropDownListPartner_SelectedIndexChanged">
                    </asp:DropDownList>
                    <div style="height:5px;"></div>    
                    <asp:DropDownList ID="DropDownListRoute" runat="server" 
                            DataTextField="RouteName" 
                            DataValueField="idRoute" Enabled="false">
                        <asp:ListItem Value="-1" Text=" [All]" Selected="True"/>
                        </asp:DropDownList>
                    <div style="height:5px;"></div>    
                    
                    <asp:DropDownList ID="DropDownListAssignedDirection" runat="server" 
                DataTextField="Name" 
                DataValueField="id" Enabled="true">
                 <asp:ListItem Value="customer" Text="customer" Selected="True"/>
                <asp:ListItem Value="supplier" Text="supplier"/>
            </asp:DropDownList>
                    <div style="height:5px;"></div>    
                
         <asp:TextBox ID="TextBoxPriority" Enabled="True" runat="server" Width="77px" Text="0"></asp:TextBox>  
                </div>    
            </div>

            <div style="float:left;padding-left: 10px; margin-top: 0px; min-width: 300px; text-align: left;">
  
                 <div style="min-width:300px;">
                    <span style="width:10px;padding-left:25px;">Category: </span>
                    <asp:DropDownList ID="ddlistServiceType" runat="server" 
                        Enabled="true" AutoPostBack="false" 
                        DataSourceID="SqlDataServiceType" DataTextField="Type" 
                        DataValueField="id"
                        >
                    </asp:DropDownList>
            

                </div>
            <div style="padding-top:5px;">
                Sub Category: 

                <asp:DropDownList ID="ddlistSubServiceType" runat="server" 
                    Enabled="true" AutoPostBack="false" 
                    DataSourceID="SqlDataSubServiceType" DataTextField="Type" 
                    DataValueField="id"
                    >
                </asp:DropDownList>
            </div>
                <div style="padding-top:5px;padding-left:3px;">
                    Change Type: 
    
    <asp:DropDownList ID="ddlistChangetype" runat="server">
        <asp:ListItem Value="-1">All</asp:ListItem>
        <asp:ListItem Value="2">New</asp:ListItem>
        <asp:ListItem Value="3">Increase</asp:ListItem>
        <asp:ListItem Value="4">Decrease</asp:ListItem>
        <asp:ListItem Value="5">Unchanged</asp:ListItem>
    </asp:DropDownList>
                </div>

   </div>


                

             <div style="clear:both;"></div>
                                
            <asp:SqlDataSource ID="SqlDataSource1" runat="server" 
                ConnectionString="<%$ ConnectionStrings:Reader %>" 
                ProviderName="<%$ ConnectionStrings:Reader.ProviderName %>" 
                SelectCommand="AllIntlPartners" SelectCommandType="StoredProcedure">
            </asp:SqlDataSource>
                <div style="width:800px;margin-top:-20px;margin-left:430px;">

         

                </div>
            

            </ContentTemplate>        
                    
        </asp:UpdatePanel>
 
 
 <div style="clear:left;height:5px;"></div>
 
   
   
     
     </div>
     
       <%--End Div Partner***************************************************--%>
    
</div> <%--Param Border--%>
<%--Common--%>

 <div style="clear:left;"></div>
    <asp:Button ID="ButtonFind" runat="server" Text="Find" Width="117px" 
        onclick="ButtonFind_Click" />
 
    <asp:Label ID="lblStatus" runat="server" Text=""></asp:Label>

    <div style="clear:left;height:4px;"></div>

<div>


    <asp:EntityDataSource ID="EntityDataRates" runat="server" 
        ConnectionString="name=PartnerEntities" DefaultContainerName="PartnerEntities" 
        EnableDelete="True" EnableFlattening="False" EnableUpdate="True" 
        EntitySetName="rates">
    </asp:EntityDataSource>

    <asp:SqlDataSource ID="SqlDataServiceType" runat="server" 
    ConnectionString="<%$ ConnectionStrings:Partner %>" 
    ProviderName="<%$ ConnectionStrings:Partner.ProviderName %>" 
    SelectCommand="select (select -1) as id, (select '[ All]') as Type union all select id,type from enumservicecategory"></asp:SqlDataSource>
    <asp:SqlDataSource ID="SqlDataSubServiceType" runat="server" 
        ConnectionString="<%$ ConnectionStrings:Partner %>" 
        ProviderName="<%$ ConnectionStrings:Partner.ProviderName %>" 
        SelectCommand="select (select -1) as id, (select '[ All]') as Type union all select id,type from enumservicesubcategory"></asp:SqlDataSource>

        <asp:SqlDataSource ID="SqlDataYesNo" runat="server" 
        ConnectionString="<%$ ConnectionStrings:Partner %>" 
        ProviderName="<%$ ConnectionStrings:Partner.ProviderName %>" 
        SelectCommand="select (select 0) as id,(select 'No') as Type
union all					
select (select 1) as id,(select 'Yes') as Type "
        ></asp:SqlDataSource>


        <asp:SqlDataSource ID="SqlDataCountry" runat="server" 
        ConnectionString="<%$ ConnectionStrings:Partner %>" 
        ProviderName="<%$ ConnectionStrings:Partner.ProviderName %>" 
        SelectCommand="select (select null) as Code,(select ' [None]') as Name
union all					
select Code,Name from countrycode "
        ></asp:SqlDataSource>


    <asp:SqlDataSource ID="SqlDataRate" runat="server" 
        ConnectionString="<%$ ConnectionStrings:Partner %>" 
        ProviderName="<%$ ConnectionStrings:Partner.ProviderName %>" 
        SelectCommand="select * from rate where id < 0"
        UpdateCommand="Update rate SET surchargeamount=0 where 1=2"
        DeleteCommand="delete from rate where id<0"
        ></asp:SqlDataSource>

        
<%--ListView Goes Here*******************--%>
<%--DataSourceID="SqlDataRate" --%>
<div style="width:1500px;overflow-x:scroll;">
    <asp:HiddenField ID="hidvaluerowcolorchange" runat="server" />
    <asp:GridView ID="GridViewSupplierRates" runat="server" AllowPaging="True" 
        AutoGenerateColumns="False" DataKeyNames="id" CellPadding="4" ForeColor="#333333" 
        GridLines="Vertical" 
        onrowcommand="GridViewSupplierRates_RowCommand" 
        onrowdatabound="GridViewSupplierRates_RowDataBound" 
        onrowediting="GridViewSupplierRates_RowEditing" style="margin-left: 0px" 
        onrowcancelingedit="GridViewSupplierRates_RowCancelingEdit" 
        onrowupdating="GridViewSupplierRates_RowUpdating" 
        onrowupdated="GridViewSupplierRates_RowUpdated" 
        onpageindexchanging="GridViewSupplierRates_PageIndexChanging" PageSize="20"
        font-size="9pt" 
        onrowdeleting="GridViewSupplierRates_RowDeleting" 
        onselectedindexchanged="GridViewSupplierRates_SelectedIndexChanged" BorderColor="#CCCCCC" BorderStyle="Solid"
        >
        <AlternatingRowStyle BackColor="#f2f2f2" ForeColor="#284775" />
        <Columns>
            <%--<asp:CommandField ShowDeleteButton="True" ShowEditButton="True" />--%>

             <asp:TemplateField>
                <ItemTemplate>
                    <asp:LinkButton ID="LinkButtonEdit" CommandName="Edit"  runat="server">Edit</asp:LinkButton>
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:LinkButton ID="LinkButtonEdit"  CommandName="Update"  runat="server" CausesValidation="false" ValidationGroup="allcontrols">Update</asp:LinkButton>
                    <asp:LinkButton ID="LinkButtonCancel"  CommandName="Cancel"  runat="server">Cancel</asp:LinkButton>
                </EditItemTemplate>
            </asp:TemplateField>

            <asp:TemplateField>
                <ItemTemplate>
                    <asp:LinkButton ID="LinkButtonDelete" 
                    OnClientClick='return confirm("Are you sure you want to delete this entry?");'
                    CommandName="Delete"  runat="server" >Delete</asp:LinkButton>
                    
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:LinkButton ID="LinkButtonDelete" Visible="false"  CommandName="myDelete"  runat="server">Delete</asp:LinkButton>
                </EditItemTemplate>
            </asp:TemplateField>

            <asp:TemplateField HeaderText="Selected" SortExpression="" Visible="false">
                <ItemTemplate>
                    <asp:CheckBox ID="CheckBoxSelected" runat="server" Enabled="true" />
                </ItemTemplate>
            </asp:TemplateField>
            
            <asp:TemplateField HeaderText="Complete" SortExpression="ChangeCommitted" ControlStyle-Width="20px" Visible="false">
                
                <ItemTemplate>
                    <asp:CheckBox ID="CheckBox1" runat="server" Enabled="false" />
                </ItemTemplate>

<ControlStyle Width="20px"></ControlStyle>
            </asp:TemplateField>
           
           
            <asp:TemplateField HeaderText="Assigned Order" SortExpression="priority" ItemStyle-Width="25px">
                <ItemTemplate>
                    <asp:Label ID="lblPriority" runat="server" Text='<%#Eval("priority")!=null?Eval("priority").ToString():"" %>'></asp:Label>
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:Label ID="lblPriority" runat="server" Text='<%#Eval("priority")!=null?Eval("priority").ToString():"" %>'></asp:Label>
                </EditItemTemplate>
            </asp:TemplateField>
            

            <asp:TemplateField HeaderText="Partner" SortExpression="idpartner">
                <ItemTemplate>
                    <asp:Label ID="lblPartner" runat="server" Text=""></asp:Label>
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:Label ID="lblPartner" runat="server" Text=""></asp:Label>
                </EditItemTemplate>
            </asp:TemplateField>

            <asp:TemplateField HeaderText="Route" SortExpression="idroute">
                <ItemTemplate>
                    <asp:Label ID="lblRoute" runat="server" Text=""></asp:Label>
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:Label ID="lblRoute" runat="server" Text=""></asp:Label>
                </EditItemTemplate>
            </asp:TemplateField>


            <asp:TemplateField HeaderText="Rate Id" SortExpression="id" ControlStyle-Width="50px" Visible="True">
                <ItemTemplate>
                    <asp:Label ID="lblId" runat="server" Text='<%# Eval("id").ToString() %>'></asp:Label>
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:Label ID="lblId" runat="server" Text='<%# Eval("id").ToString() %>'></asp:Label>
                </EditItemTemplate>

            <ControlStyle Width="50px"></ControlStyle>

            </asp:TemplateField>

            

<asp:TemplateField HeaderText="Rate Plan" SortExpression="idrateplan" ItemStyle-Wrap="false">
                
                <ItemTemplate>
                  
                    <asp:Label ID="lblRatePlan" runat="server" Text=""></asp:Label>
                </ItemTemplate>
                
                <EditItemTemplate>
                   <asp:Label ID="lblRatePlan" runat="server" Text=""></asp:Label>
                </EditItemTemplate>

<%--<ControlStyle Width="200px"></ControlStyle>--%>
            </asp:TemplateField>
            
             <asp:TemplateField HeaderText="Change" SortExpression="Status" ItemStyle-Wrap="false">
                
                <ItemTemplate>
                    <asp:Label ID="lblRateChangeType" runat="server" ></asp:Label>
                </ItemTemplate>


            </asp:TemplateField>

             <asp:TemplateField HeaderText="Tech Prefix" SortExpression="TechPrefix">
                <ItemTemplate>
                    <asp:Label ID="lblTechPrefix" runat="server" Text='<%#Eval("TechPrefix")!=null?Eval("TechPrefix").ToString():"" %>'></asp:Label>
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:Label ID="lblTechPrefix" runat="server" Text='<%#Eval("TechPrefix")!=null?Eval("TechPrefix").ToString():"" %>'></asp:Label>
                </EditItemTemplate>

            </asp:TemplateField>

            
            <asp:TemplateField HeaderText="Prefix" SortExpression="Prefix" ItemStyle-Wrap="false">
                <ItemTemplate>
                    <asp:Label ID="lblPrefix" runat="server" Text='<%#Eval("Prefix")!=null?Eval("Prefix").ToString():"" %>'></asp:Label>
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:TextBox ID="txtPrefix" Enabled="false" runat="server" Text='<%#Eval("Prefix")!=null?Eval("Prefix").ToString():""%>'></asp:TextBox>

                </EditItemTemplate>

            </asp:TemplateField>

            
            <asp:TemplateField HeaderText="Description" SortExpression="Description"  ItemStyle-Wrap="false">
                <ItemTemplate>
                    <asp:Label ID="lblDescription" runat="server" Text='<%# Eval("description").ToString() %>'></asp:Label>
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:TextBox ID="txtDescription" Enabled="True" runat="server" Text='<%# Bind("description") %>'></asp:TextBox>
                </EditItemTemplate>

<%--<ControlStyle Width="200px"></ControlStyle>--%>
            </asp:TemplateField>

              <asp:TemplateField HeaderText="Currency" SortExpression="pcurrency">
                <ItemTemplate>
                    <asp:Label ID="lblCurrency" runat="server" Text='<%# Eval("pcurrency").ToString() %>'></asp:Label>
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:Label ID="lblCurrency" runat="server" Text='<%# Eval("pcurrency").ToString() %>'></asp:Label>
                </EditItemTemplate>

<%--<ControlStyle Width="200px"></ControlStyle>--%>
            </asp:TemplateField>

            
            <asp:TemplateField HeaderText="Rate" SortExpression="rateamount" ItemStyle-Wrap="false">
                <ItemTemplate>
                    <%--'<%# Convert.ToDecimal(Eval("rateamount").ToString()).ToString("0.#00000") %>'--%>
                    <asp:Label ID="lblRateAmount" runat="server"  Text='<%#Eval("rateamount")!=null?Eval("rateamount").ToString():""%>'></asp:Label>
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:TextBox ID="txtRateAmount" Enabled="True" runat="server" Text='<%#Eval("rateamount")!=null? Eval("rateamount").ToString():"" %>'></asp:TextBox>
                </EditItemTemplate>





            </asp:TemplateField>

            
            <asp:TemplateField HeaderText="WeekDayStart" SortExpression="WeekDayStart" Visible="false">
                <ItemTemplate>
                    <asp:Label ID="lblWeekDayStart" runat="server" Text='<%# Eval("WeekDayStart")==null?"":Eval("WeekDayStart").ToString() %>'></asp:Label>
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:TextBox ID="txtWeekDayStart" Enabled="false" runat="server" Text='<%# Eval("WeekDayStart")==null?"":Eval("WeekDayStart").ToString() %>'></asp:TextBox>
                </EditItemTemplate>
            </asp:TemplateField>

            <asp:TemplateField HeaderText="WeekDayEnd" SortExpression="WeekDayEnd" Visible="false">
                <ItemTemplate>
                    <asp:Label ID="lblWeekDayEnd" runat="server" Text='<%# Eval("WeekDayEnd")==null?"":Eval("WeekDayEnd").ToString() %>'></asp:Label>
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:TextBox ID="txtWeekDayEnd" Enabled="false" runat="server" Text='<%# Eval("WeekDayEnd")==null?"":Eval("WeekDayEnd").ToString() %>'></asp:TextBox>
                </EditItemTemplate>
            </asp:TemplateField>

            <asp:TemplateField HeaderText="StartTime" SortExpression="StartTime" Visible="false">
                <ItemTemplate>
                    <asp:Label ID="lblStartTime" runat="server" Text='<%# Eval("StartTime")==null?"":Eval("StartTime").ToString() %>'></asp:Label>
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:TextBox ID="txtStartTime" Enabled="false" runat="server" Text='<%# Eval("StartTime")==null?"":Eval("StartTime").ToString() %>'></asp:TextBox>
                </EditItemTemplate>
            </asp:TemplateField>

            <asp:TemplateField HeaderText="EndTime" SortExpression="EndTime" Visible="false">
                <ItemTemplate>
                    <asp:Label ID="lblEndTime" runat="server" Text='<%# Eval("EndTime")==null?"":Eval("EndTime").ToString() %>'></asp:Label>
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:TextBox ID="txtEndTime" Enabled="false" runat="server" Text='<%# Eval("EndTime")==null?"":Eval("EndTime").ToString() %>'></asp:TextBox>
                </EditItemTemplate>
            </asp:TemplateField>


                <asp:TemplateField HeaderText="Effective Since" SortExpression="startdate"  ItemStyle-Wrap="false">
                
                <ItemTemplate>
                    <asp:Label ID="lblStartDate" runat="server" Text='<%# 
                    Eval("p_startdate").ToString().Trim()!=""
                    ? Convert.ToDateTime(Eval("p_startdate")).ToString("yyyy-MM-dd HH:mm:ss")
                    :""
                    %>'></asp:Label>
                </ItemTemplate>
                    
                <EditItemTemplate>
                    Date:
                    <%--<asp:Calendar ID="CalendarStartDate" runat="server"></asp:Calendar>--%>
                    
                    <asp:TextBox ID="TextBoxStartDatePicker" runat="server" Enabled="false"
                        Text='<%# 
                    Eval("p_startdate").ToString().Trim()!=""
                    ? Convert.ToDateTime(Eval("p_startdate")).ToString("yyyy-MM-dd")
                    :""
                    %>'>>
                  </asp:TextBox>
                    <asp:CalendarExtender ID="CalendarStartDate" runat="server" 
                                    TargetControlID="TextBoxStartDatePicker"  PopupButtonID="TextBoxStartDatePicker" Format="yyyy-MM-dd">
                  </asp:CalendarExtender>
                  <br />
                  Time: <asp:TextBox ID="TextBoxStartDateTimePicker" runat="server" Enabled="false"
                      Text='<%# 
                    Eval("p_startdate").ToString().Trim()!=""
                    ? Convert.ToDateTime(Eval("p_startdate")).ToString("HH:mm:ss")
                    :""
                    %>'>>
                  </asp:TextBox>
                
                </EditItemTemplate>
                 

            </asp:TemplateField>

             <asp:TemplateField HeaderText="Valid Before" SortExpression="enddate"  ItemStyle-Wrap="false">
                
                <ItemTemplate>
                    <asp:Label ID="lblEndDate" runat="server" Text='<%# 
                    ( Eval("p_enddate")!=null)
                    ? Convert.ToDateTime(Eval("p_enddate")).ToString("yyyy-MM-dd HH:mm:ss")
                    :""
                    %>'></asp:Label>
                </ItemTemplate>
                    
                <EditItemTemplate>
                    
                     Date:
                    <%--<asp:Calendar ID="CalendarStartDate" runat="server"></asp:Calendar>--%>
                    
                    <asp:TextBox ID="TextBoxEndDatePicker" runat="server">
                  </asp:TextBox>
                    <asp:CalendarExtender ID="CalendarEndDate" runat="server" 
                                    TargetControlID="TextBoxEndDatePicker"  PopupButtonID="TextBoxEndDatePicker" Format="yyyy-MM-dd">
                  </asp:CalendarExtender>
                  <br />
                  Time: <asp:TextBox ID="TextBoxEndDateTimePicker" Text="00:00:00" runat="server">
                  </asp:TextBox>


                </EditItemTemplate>
        </asp:TemplateField>
            

            <asp:TemplateField HeaderText="Pulse" SortExpression="Resolution" ControlStyle-Width="30px" >
                <ItemTemplate>
                    
                    <asp:Label ID="lblPulse" runat="server" Text='<%# Eval("Resolution")!=null?Eval("Resolution").ToString():"0" %>'></asp:Label>
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:TextBox ID="txtResolution" Enabled="True" runat="server" Text='<%# Eval("Resolution")!=null?Eval("Resolution").ToString():"0" %>'></asp:TextBox>
                </EditItemTemplate>

<ControlStyle Width="30px"></ControlStyle>
            </asp:TemplateField>

            <asp:TemplateField HeaderText="Min Duration (Sec)" SortExpression="MinDurationSec" ItemStyle-Width="30px">
                <ItemTemplate>
                    <asp:Label ID="lblMinDurationSec" runat="server" Text='<%#Eval("MinDurationSec")!=null?Eval("MinDurationSec").ToString():"" %>'></asp:Label>
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:TextBox ID="txtMinDurationSec" Enabled="True" runat="server" Text='<%# Eval("MinDurationSec")!=null?Eval("MinDurationSec").ToString():"" %>'></asp:TextBox>
                </EditItemTemplate>
            
<ItemStyle Width="30px"></ItemStyle>
            
            </asp:TemplateField>

            
             <asp:TemplateField HeaderText="Country" SortExpression="CountryCode" ItemStyle-Wrap="false" >
                
                <ItemTemplate>
                  
                    <asp:Label ID="lblCountry" runat="server" Text=""></asp:Label>
                </ItemTemplate>
                
                <EditItemTemplate>
                   <asp:Label ID="lblCountry" runat="server" Text=""></asp:Label> 
                </EditItemTemplate>

<%--<ControlStyle Width="200px"></ControlStyle>--%>
            </asp:TemplateField>


             <asp:TemplateField HeaderText="Round Digits after Decimal for Rate Amount" SortExpression="" Visible="true">
                <ItemTemplate>
                    <asp:Label ID="lblRoundDigits" runat="server" Text='<%# Eval("RateAmountRoundupDecimal")==null?"": Eval("RateAmountRoundupDecimal").ToString() %>'></asp:Label>
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:TextBox ID="txtRoundDigits" Enabled="true" runat="server" Text='<%# Eval("RateAmountRoundupDecimal")==null?"": Eval("RateAmountRoundupDecimal").ToString() %>'></asp:TextBox>
                </EditItemTemplate>
            </asp:TemplateField>

            
            <asp:TemplateField HeaderText="Other Amount1" SortExpression="OtherAmount1" Visible="true" ControlStyle-Width="70px">
                <ItemTemplate>
                    <asp:Label ID="lblOtherAmount1" runat="server" Text='<%# Eval("OtherAmount1")==null?"": Eval("OtherAmount1").ToString() %>'></asp:Label>
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:TextBox ID="txtOtherAmount1" Enabled="true" runat="server" Text='<%# Eval("OtherAmount1")==null?"": Eval("OtherAmount1").ToString() %>'></asp:TextBox>
                </EditItemTemplate>

<ControlStyle Width="70px"></ControlStyle>
            </asp:TemplateField>

            <asp:TemplateField HeaderText="Other Amount2" SortExpression="OtherAmount2" Visible="true" ControlStyle-Width="70px">
                <ItemTemplate>
                    <asp:Label ID="lblOtherAmount2" runat="server" Text='<%# Eval("OtherAmount2")==null?"": Eval("OtherAmount2").ToString() %>'></asp:Label>
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:TextBox ID="txtOtherAmount2" Enabled="true" runat="server" Text='<%# Eval("OtherAmount2")==null?"": Eval("OtherAmount2").ToString() %>'></asp:TextBox>
                </EditItemTemplate>

<ControlStyle Width="70px"></ControlStyle>
            </asp:TemplateField>

            <asp:TemplateField HeaderText="Other Amount3" SortExpression="OtherAmount3" Visible="true" ControlStyle-Width="70px">
                <ItemTemplate>
                    <asp:Label ID="lblOtherAmount3" runat="server" Text='<%# Eval("OtherAmount3")==null?"": Eval("OtherAmount3").ToString() %>'></asp:Label>
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:TextBox ID="txtOtherAmount3" Enabled="true" runat="server" Text='<%# Eval("OtherAmount3")==null?"": Eval("OtherAmount3").ToString() %>'></asp:TextBox>
                </EditItemTemplate>

<ControlStyle Width="70px"></ControlStyle>
            </asp:TemplateField>

            <asp:TemplateField HeaderText="X (BDT)" SortExpression="OtherAmount4" Visible="false" ControlStyle-Width="70px">
                <ItemTemplate>
                    <asp:Label ID="lblOtherAmount4" runat="server" Text='<%# Eval("OtherAmount4")==null?"0": Convert.ToDecimal(Eval("OtherAmount4").ToString()).ToString("0.#00000") %>'></asp:Label>
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:TextBox ID="txtOtherAmount4" Enabled="true" runat="server" Text='<%# Eval("OtherAmount4")==null?"0": Convert.ToDecimal(Eval("OtherAmount4").ToString()).ToString("0.#00000") %>'></asp:TextBox>
                </EditItemTemplate>

<ControlStyle Width="70px"></ControlStyle>
            </asp:TemplateField>

            <asp:TemplateField HeaderText="Y (USD)" SortExpression="OtherAmount5" Visible="false" ControlStyle-Width="70px">
                <ItemTemplate>
                    <asp:Label ID="lblOtherAmount5" runat="server" Text='<%# Eval("OtherAmount5")==null?"0": Convert.ToDecimal(Eval("OtherAmount5").ToString()).ToString("0.#00000") %>'></asp:Label>
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:TextBox ID="txtOtherAmount5" Enabled="true" runat="server" Text='<%# Eval("OtherAmount5")==null?"0": Convert.ToDecimal(Eval("OtherAmount5").ToString()).ToString("0.#00000") %>'></asp:TextBox>
                </EditItemTemplate>

<ControlStyle Width="70px"></ControlStyle>
            </asp:TemplateField>

            <asp:TemplateField HeaderText="ANS % of Z" SortExpression="OtherAmount6" Visible="false" ControlStyle-Width="50px">
                <ItemTemplate>
                    <asp:Label ID="lblOtherAmount6" runat="server" Text='<%# Eval("OtherAmount6")==null?"0": Convert.ToDecimal(Eval("OtherAmount6").ToString()).ToString("0.#00000") %>'></asp:Label>
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:TextBox ID="txtOtherAmount6" Enabled="true" runat="server" Text='<%# Eval("OtherAmount6")==null?"0": Convert.ToDecimal(Eval("OtherAmount6").ToString()).ToString("0.#00000") %>'></asp:TextBox>
                </EditItemTemplate>

<ControlStyle Width="50px"></ControlStyle>
            </asp:TemplateField>

            <asp:TemplateField HeaderText="ICX % of Z" SortExpression="OtherAmount7" Visible="false" ControlStyle-Width="50px">
                <ItemTemplate>
                    <asp:Label ID="lblOtherAmount7" runat="server" Text='<%# Eval("OtherAmount7")==null?"0": Convert.ToDecimal(Eval("OtherAmount7").ToString()).ToString("0.#00000") %>'></asp:Label>
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:TextBox ID="txtOtherAmount7" Enabled="true" runat="server" Text='<%# Eval("OtherAmount7")==null?"0": Convert.ToDecimal(Eval("OtherAmount7").ToString()).ToString("0.#00000") %>'></asp:TextBox>
                </EditItemTemplate>

<ControlStyle Width="50px"></ControlStyle>
            </asp:TemplateField>

            <asp:TemplateField HeaderText="IGW % of Z" SortExpression="OtherAmount8" Visible="false" ControlStyle-Width="50px">
                <ItemTemplate>
                    <asp:Label ID="lblOtherAmount8" runat="server" Text='<%# Eval("OtherAmount8")==null?"0": Convert.ToDecimal(Eval("OtherAmount8").ToString()).ToString("0.#00000") %>'></asp:Label>
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:TextBox ID="txtOtherAmount8" Enabled="true" runat="server" Text='<%# Eval("OtherAmount8")==null?"0": Convert.ToDecimal(Eval("OtherAmount8").ToString()).ToString("0.#00000") %>'></asp:TextBox>
                </EditItemTemplate>

<ControlStyle Width="50px"></ControlStyle>
            </asp:TemplateField>

            <asp:TemplateField HeaderText="BTRC % of Z" SortExpression="OtherAmount9" Visible="false" ControlStyle-Width="50px">
                <ItemTemplate>
                    <asp:Label ID="lblOtherAmount9" runat="server" Text='<%# Eval("OtherAmount9")==null?"0": Convert.ToDecimal(Eval("OtherAmount9").ToString()).ToString("0.#00000") %>'></asp:Label>
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:TextBox ID="txtOtherAmount9" Enabled="true" runat="server" Text='<%# Eval("OtherAmount9")==null?"0": Convert.ToDecimal(Eval("OtherAmount9").ToString()).ToString("0.#00000") %>'></asp:TextBox>
                </EditItemTemplate>

<ControlStyle Width="50px"></ControlStyle>
            </asp:TemplateField>

            <asp:TemplateField HeaderText="ICX % Rev. Share" SortExpression="OtherAmount10" Visible="false" ControlStyle-Width="50px">
                <ItemTemplate>
                    <asp:Label ID="lblOtherAmount10" runat="server" Text='<%# Eval("OtherAmount10")==null?"0": Convert.ToDecimal(Eval("OtherAmount10").ToString()).ToString("0.#00000") %>'></asp:Label>
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:TextBox ID="txtOtherAmount10" Enabled="true" runat="server" Text='<%# Eval("OtherAmount10")==null?"0": Convert.ToDecimal(Eval("OtherAmount10").ToString()).ToString("0.#00000") %>'></asp:TextBox>
                </EditItemTemplate>

<ControlStyle Width="50px"></ControlStyle>
            </asp:TemplateField>




           <asp:TemplateField HeaderText="Fixed Charge Duration (Sec)" SortExpression="SurchargeTime" >
                <ItemTemplate>
                    <asp:Label ID="lblSurchargeTime" runat="server" Text='<%# Eval("SurchargeTime").ToString() %>'></asp:Label>
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:TextBox ID="txtSurchargeTime" Enabled="True" runat="server" Text='<%# Eval("SurchargeTime").ToString() %>'></asp:TextBox>
                </EditItemTemplate>

                <ControlStyle Width="50px"></ControlStyle>
            </asp:TemplateField>

            <asp:TemplateField HeaderText="Fixed Charge Amount" SortExpression="SurchargeAmount"  >
                <ItemTemplate>
                    <asp:Label ID="lblSurchargeAmount" runat="server" Text='<%# Convert.ToDecimal(Eval("SurchargeAmount").ToString()).ToString("0.#00000") %>'></asp:Label>
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:TextBox ID="txtSurchargeAmount" Enabled="True" runat="server" Text='<%# Eval("SurchargeAmount").ToString() %>'></asp:TextBox>
                </EditItemTemplate>

<ControlStyle Width="50px"></ControlStyle>
            </asp:TemplateField>

             <asp:TemplateField HeaderText="Category" SortExpression="Category">
                
                <ItemTemplate>
                  <asp:DropDownList ID="DropDownListServiceType" runat="server" AutoPostBack="false" 
                        DataSourceID="SqlDataServiceType" DataTextField="Type" DataValueField="id" SelectedValue='<%# Bind("Category") %>'
                        Enabled="false" >
                    </asp:DropDownList>
                    
                </ItemTemplate>
                
                <EditItemTemplate>
                    <asp:DropDownList ID="DropDownListServiceType" runat="server" AutoPostBack="false" 
                        DataSourceID="SqlDataServiceType" DataTextField="Type" DataValueField="id" SelectedValue='<%# Bind("Category") %>'
                        Enabled="false" >
                    </asp:DropDownList>
                    
                    
                </EditItemTemplate>
            </asp:TemplateField>

            <asp:TemplateField HeaderText="Sub Category" SortExpression="SubCategory" ControlStyle-Width="100px">
                
                <ItemTemplate>
                  <asp:DropDownList ID="DropDownListSubServiceType" runat="server" AutoPostBack="false" 
                        DataSourceID="SqlDataSubServiceType" DataTextField="Type" DataValueField="id" SelectedValue='<%# Bind("SubCategory") %>'
                        Enabled="false" >
                    </asp:DropDownList>
                    
                </ItemTemplate>
                
                <EditItemTemplate>
                    <asp:DropDownList ID="DropDownListSubServiceType" runat="server" AutoPostBack="false" 
                        DataSourceID="SqlDataSubServiceType" DataTextField="Type" DataValueField="id" SelectedValue='<%# Bind("SubCategory") %>'
                        Enabled="false" >
                    </asp:DropDownList>
                    
                    
                </EditItemTemplate>

<ControlStyle Width="100px"></ControlStyle>
            </asp:TemplateField>

            <asp:TemplateField HeaderText="Validation Error(s)" SortExpression="Field2" Visible="false" >
                
                <ItemTemplate>
                    <asp:Label ID="lblRateErrors" runat="server" ></asp:Label>
                </ItemTemplate>
            </asp:TemplateField>


            <asp:TemplateField HeaderText="Inactive" SortExpression="Inactive" Visible="false">
                
                <%--SelectedValue='<%# Bind("Inactive") %>'--%>
                <ItemTemplate>
                    <asp:DropDownList ID="DropDownListInactive" runat="server" AutoPostBack="false" 
                        DataSourceID="SqlDataYesNo" DataTextField="Type" DataValueField="id"
                        Enabled="false" >
                    </asp:DropDownList>
                    
                </ItemTemplate>
                
                <EditItemTemplate>
                    <asp:DropDownList ID="DropDownListInactive" runat="server" AutoPostBack="True" 
                        DataSourceID="SqlDataYesNo" DataTextField="Type" DataValueField="id" SelectedValue='<%# Bind("Inactive") %>'
                        Enabled="True" >
                    </asp:DropDownList>
                    
                    
                </EditItemTemplate>
                
            </asp:TemplateField>

            <%--<asp:BoundField DataField="RouteDisabled" HeaderText="Route Disabled" 
                SortExpression="RouteDisabled" />--%>

             


             <%--field1 will show the Rate Change type e.g. new, delete,increase,decrease etc. in row data bound event--%>


        </Columns>
        <EditRowStyle BackColor="#999999" />
        <FooterStyle BackColor="#08605c" Font-Bold="True" ForeColor="White" />
        <HeaderStyle BackColor="#08605c" Font-Bold="True" ForeColor="White" />
        <PagerStyle BackColor="#284775" ForeColor="White" 
            HorizontalAlign="Left" />
        <RowStyle BackColor="white" Width="5px" ForeColor="#333333" />
        <AlternatingRowStyle Width="5px" />
        <SelectedRowStyle BackColor="#E2DED6" ForeColor="#333333" Font-Bold="True" />
        <SortedAscendingCellStyle BackColor="#E9E7E2" />
        <SortedAscendingHeaderStyle BackColor="#506C8C" />
        <SortedDescendingCellStyle BackColor="#FFFDF8" />
        <SortedDescendingHeaderStyle BackColor="#6F8DAE" />
    </asp:GridView>

</div>
   
</div>

</asp:Content>

