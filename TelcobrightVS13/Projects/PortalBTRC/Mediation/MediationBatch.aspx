﻿<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="True"
    CodeBehind="MediationBatch.aspx.cs" Inherits="DefaultMedBatch" %>

<%--Common--%>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>
<%@ Import Namespace="MediationModel" %>
<%----%>
<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">
   
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
                    this.TextBoxYear.Text = DateTime.Now.ToString("yyyy");
                    this.TextBoxYear1.Text = DateTime.Now.ToString("yyyy");
                    this.DropDownListMonth.SelectedIndex = int.Parse(DateTime.Now.ToString("MM")) - 1;
                    this.DropDownListMonth1.SelectedIndex = int.Parse(DateTime.Now.ToString("MM")) - 1;
                    this.txtDate.Text = FirstDayOfMonthFromDateTime(DateTime.Now).ToString("dd/MM/yyyy");
                    this.txtDate1.Text = LastDayOfMonthFromDateTime(DateTime.Now).ToString("dd/MM/yyyy");
                    //txtDate.Text = DateTime.Now.ToString("dd/MM/yyyy");
                    //txtDate1.Text = DateTime.Now.ToString("dd/MM/yyyy");
                    
                    
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

                    submit_Click(sender, e);
                    
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
    <asp:Timer ID="Timer1" Interval="300000" runat="server" ontick="Timer1_Tick" ></asp:Timer>  
 <div id="report" style="clear:both;height:25px;background-color:white;padding-left:5px;width:1009px;margin-bottom:2px;">
    
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

    
    <span style="padding-left:0px;float:left;left:0px;font-weight:bold;margin-top:2px;margin-right:20px;color:Black;"> Report:</span>
    <span style="font-weight:bold; visibility:hidden;width:1px;left:-10px;"> Switch</span>
                
      <asp:DropDownList ID="DropDownListPartner" runat="server" 
                            DataSourceID="EntityDataSwitch" DataTextField="SwitchName" 
                            DataValueField="idSwitch" Visible="false">
                        </asp:DropDownList>
                
                  
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


    <asp:Button ID="submit" runat="server" Text="Show Report" onclick="submit_Click" OnClientClick="SethidValueSubmitClickFlag('true');"/> 
    <asp:Button ID="Button1" runat="server" onclick="Button1_Click" 
            style="margin-left: 0px" Text="Export" Visible="False" />
    <asp:Button ID="ShowHideFilter" runat="server" ViewStateMode="Enabled"
            style="margin-left: 0px" Text="Hide Filter" Visible="True" OnClientClick="ToggleParamBorderDiv();return false;"/>
    <asp:Button ID="ButtonTemplate" runat="server" OnClientClick="var value = prompt('Enter name of the Report Template:'); SetHidValueTemplate(value);" OnClick="ButtonTemplate_Click" 
            style="margin-left: 0px" Text="Save as Template" Visible="True"/>
    <asp:Label ID="Label1" runat="server" Text="" ForeColor="Red"></asp:Label> 
    <span style="font-weight:bold;"> Real Time Update 
            <asp:CheckBox ID="CheckBoxRealTimeUpdate" runat="server" Checked="true" AutoPostBack="true" oncheckedchanged="CheckBoxRealTimeUpdate_CheckedChanged"/></span>
        <span style="font-weight:bold;"> 
        <asp:TextBox ID="TextBoxDuration" runat="server" Text="30" Width="30px" ontextchanged="TextBoxDuration_TextChanged" Enabled="false" Visible="false" ></asp:TextBox> </span>  
        <input type="hidden" id="hidValueFilter" runat="server"/>
        <input type="hidden" id="hidValueSubmitClickFlag" runat="server" value="false"/>
        <input type="hidden" id="hidValueTemplate" runat="server" />
 </div>   

<div>
    <ajaxToolkit:ToolkitScriptManager ID="ToolkitScriptManager1" runat="server" > </ajaxToolkit:ToolkitScriptManager>
    
</div>



 <div id="ParamBorder" style="float:left;padding-top:3px;padding-left:10px;height:87px;display:block;border:2px ridge #E5E4E2;margin-bottom:5px;width:1100px;">
    <div style="height:2px;background-color:#f2f2f2;color:black; visibility:hidden"> 
        <span style="float:left;font-weight:bold;padding-left:20px;">   Show Egress Erlang <asp:CheckBox ID="CheckBoxShowPerformance" runat="server" /></span> 
        <%--<div style="clear:right;"></div>--%>
        
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
<div id="TimeSummary" style="float:left;margin-left:15px;padding-left:20px;height:70px;width: 280px;background-color: #faebd7;margin-top: -10px;">
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

    
    <%--All Partners Combo--%>
 




<%--ListView Goes Here***************DataFormatString="{0:F2}" ****--%>
<div style="float:left;text-align:left;width:900px;position:relative;top:0px;padding-left:5px;">
    
    <div style="width:760px;text-align:center;">
        <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="False" 
            CellPadding="4" ForeColor="#333333" GridLines="Vertical" 
            onrowdatabound="GridView1_RowDataBound" DataKeyNames="idallcdr" 
            AllowPaging="True" PageSize="1000" onrowcommand="GridView1_RowCommand" 
            onrowdeleted="GridView1_RowDeleted" onrowdeleting="GridView1_RowDeleting" ShowFooter="True" 
            >
            <AlternatingRowStyle BackColor="White" />
        <Columns>
            
            <asp:TemplateField>
                <ItemTemplate>
                    
                    <asp:LinkButton ID="LinkButton2"  runat="server" ButtonType="Button" CommandName="Delete"
                        Text="Delete" HeaderText="Delete" CausesValidation="false" ValidationGroup="allcontrols"
                    OnClientClick='return confirm("Are you sure you want to delete this entry?");'> 
                    </asp:LinkButton>
                    
                </ItemTemplate>
            </asp:TemplateField>

            
            <asp:BoundField DataField="idallcdr" HeaderText="idallcdr" 
                SortExpression="idallcdr" ReadOnly="True" Visible="false" />
            <asp:BoundField DataField="FileName" HeaderText="Job Name" 
                SortExpression="FileName" />



                <asp:TemplateField HeaderText="Job Type">
                    <ItemTemplate>
                       <asp:Label ID="lblJobType" runat="server" Text=""></asp:Label>
                    </ItemTemplate>
                </asp:TemplateField>

            

            <asp:BoundField DataField="idSwitch" HeaderText="Switch Id" 
                SortExpression="idSwitch" />
            <asp:BoundField DataField="FileSerialNumber" HeaderText="Batch No" 
                SortExpression="FileSerialNumber" />
            <asp:BoundField DataField="ListingTime" HeaderText="Creating Time" 
                SortExpression="ListingTime" DataFormatString="{0:dd-MM-yyyy HH:mm:ss}" />    
            
            <asp:BoundField DataField="NoOfRecords" HeaderText="No Of Records" 
                SortExpression="NoOfRecords"/>
            
            <asp:BoundField DataField="LoadingTime" HeaderText="Completion Time" 
                SortExpression="LoadingTime"  DataFormatString="{0:dd-MM-yyyy HH:mm:ss}" />    
            
                <asp:TemplateField HeaderText="Status">
                    <ItemTemplate>
                       <asp:Label ID="lblStatus" runat="server" Text='<%#Eval("Status") %>'>'></asp:Label>
                    </ItemTemplate>
                </asp:TemplateField>


            
                <asp:TemplateField HeaderText="Enabled">
                    <ItemTemplate>
                        <asp:CheckBox ID="CheckBox1" runat="server" />
                       
                    </ItemTemplate>
                        
                    <footertemplate>
                        <asp:Button ID="Button2" runat="server" Text="Save Changes" onclick="Save_Click" />
                </footertemplate>
                    

                </asp:TemplateField>


                

            <%--<asp:BoundField DataField="ReceiveTime" HeaderText="ReceiveTime" 
                SortExpression="ReceiveTime"  DataFormatString="{0:dd-MM-yyyy HH:mm:ss}"/>
            <asp:BoundField DataField="LoadingTime" HeaderText="LoadingTime" 
                SortExpression="LoadingTime"  DataFormatString="{0:dd-MM-yyyy HH:mm:ss}" />    
            <asp:BoundField DataField="NoOfRecords" HeaderText="NoOfRecords" 
                SortExpression="NoOfRecords"/>
            <asp:BoundField DataField="TotalDuration" HeaderText="TotalDuration" 
                SortExpression="TotalDuration"/>
            <asp:BoundField DataField="StartSequenceNumber" 
                HeaderText="StartSequenceNumber" SortExpression="StartSequenceNumber"/>--%>
            <%--<asp:BoundField DataField="EndSequenceNumber" HeaderText="EndSequenceNumber" 
                SortExpression="EndSequenceNumber"/>


            <asp:BoundField DataField="FailedCount" HeaderText="FailedCount" 
                SortExpression="FailedCount" />
            <asp:BoundField DataField="SuccessfulCount" HeaderText="SuccessfulCount" 
                SortExpression="SuccessfulCount" />
            <asp:BoundField DataField="MinCallStartTime" HeaderText="MinCallStartTime" 
                SortExpression="MinCallStartTime" />
            <asp:BoundField DataField="MaxCallStartTime" HeaderText="MaxCallStartTime" 
                SortExpression="MaxCallStartTime" />
            <asp:BoundField DataField="FullPathLocal" HeaderText="FullPathLocal" 
                SortExpression="FullPathLocal" />
--%>

        </Columns>
            <HeaderStyle BackColor="#1C5E55" Font-Bold="True" ForeColor="White" />
            <EditRowStyle BackColor="#7C6F57" />
            <FooterStyle BackColor="#1C5E55" Font-Bold="true" ForeColor="White" />
            <PagerStyle BackColor="#666666" ForeColor="White" HorizontalAlign="Center" />
            <RowStyle BackColor="white" />
            <SelectedRowStyle BackColor="#C5BBAF" Font-Bold="True" ForeColor="#333333" />
            <SortedAscendingCellStyle BackColor="#F8FAFA" />
            <SortedAscendingHeaderStyle BackColor="#246B61" />
            <SortedDescendingCellStyle BackColor="#D4DFE1" />
            <SortedDescendingHeaderStyle BackColor="#15524A" />
    </asp:GridView>
    </div>

</div>
<asp:Timer ID="Timer2" Interval="30000" runat="server" ontick="Timer1_Tick" ></asp:Timer>      
    
</asp:Content>

