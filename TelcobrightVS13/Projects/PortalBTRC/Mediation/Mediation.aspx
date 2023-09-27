<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="True"
    CodeBehind="Mediation.aspx.cs" Inherits="DefaultMediation" %>

<%--Common--%>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>
<%@ Import Namespace="System.ServiceModel.Security" %>
<%@ Import Namespace="MediationModel" %>
<%@ Import Namespace="PortalApp" %>
<%@ Import Namespace="TelcobrightMediation" %>

<%----%>
<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">
   
    <%--Page Load and Other Server Side Asp.net scripts--%>
        <script runat="server">
            TelcobrightConfig telcobrightConfig = PageUtil.GetTelcobrightConfig();

            protected void Page_Load(object sender, EventArgs e)
            {
                //common code for report pages
                //view state of ParamBorder div
                string tempText = this.hidValueFilter.Value;
                bool lastVisible = this.hidValueFilter.Value == "invisible" ? false : true;
                var databaseSetting = telcobrightConfig.DatabaseSetting;
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
                    //load job type and job status dropdownlist

                    using (PartnerEntities context = PortalConnectionHelper.GetPartnerEntitiesDynamic(databaseSetting))
                    {
                        //populate switch
                        //List<ne> lstNe = context.nes.ToList();
                        //this.DropDownListPartner.Items.Clear();
                        //this.DropDownListPartner.Items.Add(new ListItem(" [All]", "-1"));
                        //foreach (ne nE in lstNe)
                        //{
                        //    this.DropDownListPartner.Items.Add(new ListItem(nE.SwitchName, nE.idSwitch.ToString()));
                        //}
                        //List<ne> lstne=context.nes
                        //populate jobtype
                        List<enumjobdefinition> lstJobDef = context.enumjobdefinitions.Include("enumjobtype").ToList();
                        this.DropDownListJobtype.Items.Clear();
                        this.DropDownListJobtype.Items.Add(new ListItem(" [All]","-1"));
                        foreach (enumjobdefinition jobdef in lstJobDef)
                        {
                            this.DropDownListJobtype.Items.Add(new ListItem(jobdef.enumjobtype.Type + "/" + jobdef.Type, jobdef.id.ToString()));
                        }
                        this.DropDownListJobtype.SelectedValue = "1";
                        //populate jobstatus
                        List<enumjobstatu> lstJobStatus = context.enumjobstatus.ToList();
                        this.DropDownListJobStatus.Items.Clear();
                        this.DropDownListJobStatus.Items.Add(new ListItem(" [All]", "-1"));
                        foreach (enumjobstatu jobstat in lstJobStatus)
                        {
                            this.DropDownListJobStatus.Items.Add(new ListItem(jobstat.Type, jobstat.id.ToString()));
                        }

                    }
                    
                    setICXListDropDown(DropDownListViewIncomingRoute, EventArgs.Empty);
                    setSwitchListDropDown(DropDownListPartner, EventArgs.Empty);


                    this.TextBoxYear.Text = DateTime.Now.ToString("yyyy");
                    this.TextBoxYear1.Text = DateTime.Now.ToString("yyyy");
                    this.DropDownListMonth.SelectedIndex = int.Parse(DateTime.Now.ToString("MM")) - 1;
                    this.DropDownListMonth1.SelectedIndex = int.Parse(DateTime.Now.ToString("MM")) - 1;
                    //txtDate.Text = FirstDayOfMonthFromDateTime(System.DateTime.Now).ToString("dd/MM/yyyy");
                    //txtDate1.Text = LastDayOfMonthFromDateTime(System.DateTime.Now).ToString("dd/MM/yyyy");
                    this.txtDate.Text = DateTime.Now.ToString("dd/MM/yyyy");
                    this.txtDate1.Text = DateTime.Now.ToString("dd/MM/yyyy");


                    //set controls if page is called for a template
                    this.ViewState["jobtype"]="";
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
                            else if (thisParam.key == "type")
                            {
                                this.ViewState["jobtype"] = thisParam.value;
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
                this.txtDate.Text= FirstDayOfMonthFromDateTime(anyDayOfMonth).ToString("dd/MM/yyyy");
            }
            protected void DropDownListMonth1_SelectedIndexChanged(object sender, EventArgs e)
            {
                //select 15th of month to find out first and last day of a month as it exists in all months.
                DateTime anyDayOfMonth = new DateTime(int.Parse(this.TextBoxYear1.Text), int.Parse(this.DropDownListMonth1.SelectedValue), 15);
                this.txtDate1.Text = LastDayOfMonthFromDateTime(anyDayOfMonth).ToString("dd/MM/yyyy");
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
                var databaseSetting = telcobrightConfig.DatabaseSetting;
                using (PartnerEntities context = PortalConnectionHelper.GetPartnerEntitiesDynamic(databaseSetting))
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
    <asp:Timer ID="Timer1" Interval="300000" runat="server" ontick="Timer1_Tick" ></asp:Timer>  
 <div id="report" style="clear:both;height:25px;background-color:white;padding-left:5px;min-width:1009px;margin-bottom:2px;">
    
    <script type="text/javascript">
        function ToggleParamBorderDiv() {
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

    
    <%--<span style="padding-left:0px;float:left;left:0px;font-weight:bold;margin-top:2px;margin-right:20px;color:Black;"> Report:</span>--%>
     <span style="font-weight:bold;">
         Select ICX:                
         <asp:DropDownList ID="DropDownListViewIncomingRoute" runat="server"
                           OnSelectedIndexChanged="DropDownListViewIncomingRoute_SelectedChanged"
                           AutoPostBack="True"
                           Enabled="true">
         </asp:DropDownList>

     </span>

    <span style="font-weight:bold;"> Switch</span>
<%--     <asp:CheckBox ID="CheckBoxViewSwithcName" runat="server" AutoPostBack="True"
                   OnCheckedChanged="CheckBoxSwithcName_CheckedChanged" Checked="false" />--%>
                <asp:DropDownList ID="DropDownListPartner" runat="server"  Enabled="True"
                        DataTextField="SwitchName" 
                        DataValueField="idSwitch">
                    </asp:DropDownList>
    
         <asp:Label ID="Label2" runat="server" Font-Bold="true" Text="Job Type"></asp:Label>
         <asp:DropDownList ID="DropDownListJobtype" runat="server" DataValueField="id" DataTextField="type"></asp:DropDownList>
         <asp:Label ID="Label3" runat="server" Font-Bold="true" Text="Job Status"  DataValueField="id" DataTextField="type"></asp:Label>
     <asp:CheckBox ID="CheckBoxNegateStatus" runat="server" Text="Not" />
     <asp:DropDownList ID="DropDownListJobStatus" runat="server"></asp:DropDownList>
     <asp:Label ID="Label4" runat="server" Font-Bold="true" Text="Job name ["></asp:Label>
     <asp:CheckBox ID="CheckBoxJobNameContains" runat="server" Text="does not" />
     <asp:Label ID="Label5" runat="server" Font-Bold="true" Text="] contain(s) "></asp:Label>
     
     <asp:TextBox ID="TextBoxJobName" runat="server"></asp:TextBox>
                    <asp:EntityDataSource ID="EntityDataSwitch" runat="server" 
                        ConnectionString="name=PartnerEntities" 
                        DefaultContainerName="PartnerEntities" EnableFlattening="False" 
                        EntitySetName="nes" 
                        onquerycreated="EntityDataSwitch_QueryCreated" 
                        >
                    </asp:EntityDataSource>

                    <asp:EntityDataSource ID="EntityDataAllCdr" runat="server" 
                         ConnectionString="name=PartnerEntities" 
                         DefaultContainerName="PartnerEntities" EnableFlattening="False" 
                         EntitySetName="allcdrs" onquerycreated="EntityDataAllCdr_QueryCreated">
                    </asp:EntityDataSource>

     <div style="clear:both;"></div>
    
 </div>   

<div>
    <ajaxToolkit:ToolkitScriptManager ID="ToolkitScriptManager1" runat="server" > </ajaxToolkit:ToolkitScriptManager>
    
</div>



 <div id="ParamBorder" style="margin-top:0px; float:left;padding-top:3px;padding-left:10px;height:80px;display:block;border:2px ridge #E5E4E2;margin-bottom:5px;width:1100px;">
    <div style="height:2px;background-color:#f2f2f2;color:black; visibility:hidden;"> 
        
        
    
    
        <%--<span style="float:left;font-weight:bold;padding-left:20px;">   Show Egress Erlang <asp:CheckBox ID="CheckBoxShowPerformance" runat="server" /></span> --%>
        <%--<div style="clear:right;"></div>--%>
        
    </div>
    
    
    
     <%--date time div--%>
     <div id="DateTimeDiv" style="padding-left:2px;position:relative;float:left;left:0px;top:0px;width:730px;z-index:10;background-color:#F7F6F3;height:70px;"> <%--Start OF date time/months field DIV--%>
    
          
         
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


    <div style="float:left;min-width:280px;">
        Start (Creation) Date [Time] <asp:TextBox id="txtDate" Runat="server" /> 
        <asp:CalendarExtender ID="CalendarStartDate" runat="server" 
            TargetControlID="txtDate"  PopupButtonID="txtDate" Format="dd/MM/yyyy">
        </asp:CalendarExtender>     
        
        
    </div>
    
    <div style="float:left;min-width:280px;">
        End (Creation) Date [Time] <asp:TextBox id="txtDate1" Runat="server" />
        <asp:CalendarExtender ID="CalendarEndDate" runat="server" 
            TargetControlID="txtDate1"  PopupButtonID="txtDate1" Format="dd/MM/yyyy">
        </asp:CalendarExtender>     
    </div>
         
    

   
    <div style="font-size:smaller;text-align:left;overflow:visible;clear:left;color:#8B4500;">[Enter only Date in "dd/MM/yyyy (e.g. 21/11/2012) or Date+Time in "dd/MM/yyyy HH:mm:ss" (e.g. 21/11/2012 19:01:59) format]</div>


</div> <%--END OF date time/months field DIV--%>
<div id="TimeSummary" style="float:left;margin-left:15px;padding-left:20px;height:70px;width: 280px;background-color: #faebd7;margin-top: 0px;">
    <div style="font-weight:bold;float:left;">Quick View:<asp:CheckBox ID="CheckBoxDailySummary" runat="server" Visible="false" /></div> 
    <div style="clear:left;margin-top:5px;"></div>
    <div style="float:left; margin-right:5px;">
        <asp:LinkButton ID="LinkButtonToday" runat="server" 
            onclick="LinkButtonToday_Click">Today</asp:LinkButton><br />
        <asp:LinkButton ID="LinkButtonYesterday" runat="server" 
            onclick="LinkButtonYesterday_Click">Yesterday</asp:LinkButton>
    </div>
    <div style="float:left;margin-right:5px;">
        <asp:LinkButton ID="LinkButtonLast3" runat="server" 
            onclick="LinkButtonLast3_Click">Last 3 Days</asp:LinkButton><br />
        <asp:LinkButton ID="LinkButtonLast7" runat="server" 
            onclick="LinkButtonLast7_Click">Last 7 Days</asp:LinkButton>
    </div>
    <div style="float:left;">
        <asp:LinkButton ID="LinkButtonThisMonth" runat="server" 
            onclick="LinkButtonThisMonth_Click">This Month</asp:LinkButton>
    </div>
</div>    
       
       <div id="PartnerFilter" style="margin-top:-4px;margin-left:10px;float:left;width:1080px;padding-left:5px;background-color:#f2f2f2;visibility:hidden;">
           <%--<span style="font-size: smaller;position:relative;left:-53px;padding-left:0px;clear:right;">[Enter only Date in "dd/MM/yyyy (e.g. 21/11/2012) or Date+Time in "dd/MM/yyyy HH:mm:ss" (e.g. 21/11/2012 19:01:59) format]</span>   --%>

            <div style="text-align:left;">
                
                <div style="float:left; height: 2px;">

                <div style="float:left;">
                    
                    
                </div>
                
                &nbsp;&nbsp;

               
             </div>
                
            </div>
       </div>
       <%--End Div Partner***************************************************--%>



          
</div> <%--Param Border--%>

     <div style="clear:both;">
        <asp:Button ID="submit" runat="server" Text="Show Report" onclick="submit_Click" OnClientClick="SethidValueSubmitClickFlag('true');"/> 
        <asp:Button ID="Button1" runat="server" onclick="Button1_Click" 
                style="margin-left: 0px" Text="Export" Visible="False" />
         
        <asp:Button ID="ShowHideFilter" runat="server" ViewStateMode="Enabled"
                style="margin-left: 0px" Text="Hide Filter" Visible="True" OnClientClick="ToggleParamBorderDiv();return false;"/>
        <asp:Button ID="ButtonTemplate" runat="server" OnClientClick="var value = prompt('Enter name of the Report Template:'); SetHidValueTemplate(value);" OnClick="ButtonTemplate_Click" 
                style="margin-left: 0px" Text="Save as Template" Visible="True"/>
        <asp:Label ID="Label1" runat="server" Text="" ForeColor="Red"></asp:Label> 
        
            <span style="font-weight:bold;"> 
            <asp:TextBox ID="TextBoxDuration" runat="server" Text="30" Width="30px" ontextchanged="TextBoxDuration_TextChanged" Enabled="false" Visible="false" ></asp:TextBox></span>  
            <input type="hidden" id="hidValueFilter" runat="server"/>
            <input type="hidden" id="hidValueSubmitClickFlag" runat="server" value="false"/>
            <input type="hidden" id="hidValueTemplate" runat="server" />
         <asp:Button ID="Button2" runat="server" 
            style="margin-left: 0px" Text="Export First " Visible="true" Enabled="false"
             onclick="Button2_Click" />
             <asp:TextBox ID="TextBoxNoOfRecords" runat="server" Text="1000"></asp:TextBox> Records or, 
     <span style="padding-left:5px;">
     <asp:Button ID="Button3" runat="server" 
            style="margin-left: 0px" Text="Export All" Visible="true" Enabled="false"
             onclick="Button1_Click" />
     </span>
     <span style="padding-left:5px;"> (Might take long...)</span>
         <span style="padding-left:20px;font-weight:bold;"> Real Time Update 
                <asp:CheckBox ID="CheckBoxRealTimeUpdate" runat="server" AutoPostBack="true" oncheckedchanged="CheckBoxRealTimeUpdate_CheckedChanged"/></span>
    </div>
    <div style="clear:both;padding:1px;">
         <asp:Label ID="lblStatus" Visible="false" ForeColor="Black" Font-Bold="true" runat="server" Text=""></asp:Label>
    </div>
    
    <%--All Partners Combo--%>
 




<%--ListView Goes Here***************DataFormatString="{0:F2}" ****--%>
<div style="float:left;text-align:left;width:900px;position:relative;top:0px;padding-left:0px;">
    
    <div style="min-width:760px;text-align:center;">
        
       
        
        <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="False" 
            CellPadding="4" ForeColor="#333333" GridLines="Vertical" 
            onrowdatabound="GridView1_RowDataBound" DataKeyNames="id" 
            AllowPaging="True" PageSize="300" BorderColor="Silver" 
            OnPageIndexChanging="GridView1_PageIndexChanging"
            >
            <AlternatingRowStyle BackColor="#f2f2f2" ForeColor="#284775" />
        <Columns>
            <asp:BoundField DataField="id" HeaderText="JobID" 
                SortExpression="id" ReadOnly="True" />
            <asp:TemplateField>
                <HeaderTemplate>Job Type</HeaderTemplate>
            <ItemTemplate>
               <%#Eval("enumjobdefinition.enumjobtype.type")+"/"+Eval("enumjobdefinition.type") %>
            </ItemTemplate>
        </asp:TemplateField>
            <asp:BoundField DataField="JobName" HeaderText="JobName" ItemStyle-Wrap="false"
                SortExpression="JobName" />
            
            <asp:BoundField DataField="ne.switchname" HeaderText="Network Element" 
                SortExpression="idSwitch" />
            <asp:BoundField DataField="SerialNumber" HeaderText="SerialNumber" 
                SortExpression="SerialNumber" />
            <asp:BoundField DataField="enumjobstatu.type" HeaderText="Status" 
                SortExpression="Status" />
            <asp:BoundField DataField="Progress" HeaderText="Progress" 
                SortExpression="Progress" />
            <asp:BoundField DataField="CreationTime" HeaderText="CreationTime" DataFormatString="{0:yyyy-MM-dd HH:mm:ss}"  ItemStyle-Wrap="false" 
                SortExpression="CreationTime"  />    
            <%--<asp:BoundField DataField="ReceiveTime" HeaderText="ReceiveTime" 
                SortExpression="ReceiveTime"  DataFormatString="{0:yyyy-MM-dd HH:mm:ss}"/>--%>
            <asp:BoundField DataField="CompletionTime" HeaderText="CompletionTime"  ItemStyle-Wrap="false" 
                SortExpression="CompletionTime"  DataFormatString="{0:yyyy-MM-dd HH:mm:ss}" />    
            <asp:BoundField DataField="NoOfSteps" HeaderText="NoOfSteps" 
                SortExpression="NoOfSteps"/>
            <%--<asp:BoundField DataField="JobParameter" HeaderText="Parameters" ItemStyle-Width="500px" ItemStyle-Wrap="false"
                SortExpression="JobParameter" />--%>
            <asp:BoundField DataField="JobSummary" HeaderText="Summary" 
                SortExpression="JobSummary" />
            <asp:BoundField DataField="Error" HeaderText="Execution Error" 
                SortExpression="Error" />

        </Columns>
            <HeaderStyle BackColor="#08605c" Font-Bold="True" ForeColor="White" />
            <EditRowStyle BackColor="#999999" />
            <FooterStyle BackColor="#08605c" Font-Bold="true" ForeColor="White" />
            <PagerStyle BackColor="#284775" ForeColor="White" HorizontalAlign="Center" />
            <RowStyle BackColor="white" ForeColor="#333333" />
            <SelectedRowStyle BackColor="#E2DED6" Font-Bold="True" ForeColor="#333333" />
            <SortedAscendingCellStyle BackColor="#E9E7E2" />
            <SortedAscendingHeaderStyle BackColor="#506C8C" />
            <SortedDescendingCellStyle BackColor="#FFFDF8" />
            <SortedDescendingHeaderStyle BackColor="#6F8DAE" />
    </asp:GridView>
    </div>

</div>
<asp:Timer ID="Timer2" Interval="300000" runat="server" ontick="Timer1_Tick" ></asp:Timer>      
    
</asp:Content>

