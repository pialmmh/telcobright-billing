<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="True"
    CodeBehind="InternationalInTransit.aspx.cs" Inherits="InternationalInTransit" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>
<%@ Import Namespace="MediationModel" %>
<%@ Import Namespace="TelcobrightMediation" %>
<%@ Import Namespace="PortalApp" %>




<%----%>
<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">

    <%--Page Load and Other Server Side Asp.net scripts--%>
    <script runat="server">

        protected void Page_Load(object sender, EventArgs e)
        {
            TelcobrightConfig tbc = PageUtil.GetTelcobrightConfig();
            PageUtil.ApplyPageSettings(this, false, tbc);
            //common code for report pages
            //view state of ParamBorder div
            string tempText = hidValueFilter.Value;
            bool lastVisible = hidValueFilter.Value == "invisible" ? false : true;
            if (hidValueSubmitClickFlag.Value == "false")
            {
                if (lastVisible)
                {
                    //show filters...
                    Page.ClientScript.RegisterStartupScript(GetType(), "MyKey", "ShowParamBorderDiv();", true);
                }
                else
                {
                    //hide filters...
                    Page.ClientScript.RegisterStartupScript(GetType(), "MyKey", "HideParamBorderDiv();", true);
                }
            }
            //set this month's start and End Date [Time] in the date picker controls...
            if (!IsPostBack)
            {

                //set summary as report source default
                DropDownListReportSource.SelectedIndex = 0;
                TextBoxYear.Text = System.DateTime.Now.ToString("yyyy");
                TextBoxYear1.Text = System.DateTime.Now.ToString("yyyy");
                DropDownListMonth.SelectedIndex = int.Parse(System.DateTime.Now.ToString("MM")) - 1;
                DropDownListMonth1.SelectedIndex = int.Parse(System.DateTime.Now.ToString("MM")) - 1;
                //txtDate.Text = FirstDayOfMonthFromDateTime(System.DateTime.Now).ToString("dd/MM/yyyy");
                //txtDate1.Text = LastDayOfMonthFromDateTime(System.DateTime.Now).ToString("dd/MM/yyyy");
                txtDate.Text = DateTime.Now.ToString("yyyy-MM-dd 00:00:00");
                txtDate1.Text = DateTime.Now.ToString("yyyy-MM-dd 23:59:59");


                //set controls if page is called for a template
                TreeView masterTree = (TreeView)Master.FindControl("TreeView1");
                NameValueCollection n = Request.QueryString;
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
                            ClientScript.RegisterClientScriptBlock(this.GetType(), "Alert", script, true);
                            return;
                        }
                    }
                    //Load Report Templates in TreeView dynically from database.
                    CommonCode commonCode = new CommonCode();
                    commonCode.LoadReportTemplatesTree(ref masterTree);
                }

                using (PartnerEntities contex = new PartnerEntities())
                {
                    var IOSList = contex.partners.Where(c => c.PartnerType == 3).ToList();

                    DropDownListPartner.Items.Clear();
                    DropDownListOutPartner.Items.Clear();
                    DropDownListPartner.Items.Add(new ListItem(" [All]", "-1"));
                    DropDownListOutPartner.Items.Add(new ListItem(" [All]", "-1"));
                    foreach (partner p in IOSList)
                    {
                        DropDownListPartner.Items.Add(new ListItem(p.PartnerName, p.idPartner.ToString()));
                        DropDownListOutPartner.Items.Add(new ListItem(p.PartnerName, p.idPartner.ToString()));
                    }
                    //var ANSList = contex.partners.Where(c => c.PartnerType == 1).ToList();
                    //DropDownListAns.Items.Clear();
                    //DropDownListAns.Items.Add(new ListItem("[All]","-1"));
                    //foreach (partner p in ANSList)
                    //{
                    //    DropDownListAns.Items.Add(new ListItem(p.PartnerName, p.idPartner.ToString()));
                    //}

                    //var IGWList = contex.partners.Where(c => c.PartnerType == 2).ToList();
                    //DropDownListIgw.Items.Clear();
                    //DropDownListIgw.Items.Add(new ListItem("[All]", "-1"));
                    //foreach(partner p in IGWList)
                    //{
                    //    DropDownListIgw.Items.Add(new ListItem(p.PartnerName, p.idPartner.ToString()));
                    //}

                }




                //Retrieve Path from TreeView for displaying in the master page caption label
                //set screentile/caption in the master page...
                Label lblScreenTitle = (Label)Master.FindControl("lblScreenTitle");
                lblScreenTitle.Text = "";
                string localPath = Request.Url.LocalPath;
                int pos2NdSlash = localPath.Substring(1, localPath.Length - 1).IndexOf("/");
                string rootFolder = localPath.Substring(1, pos2NdSlash);
                int endOfRootFolder = Request.Url.AbsoluteUri.IndexOf(rootFolder);
                string urlWithQueryString = ("~" + "/" + rootFolder + Request.Url.AbsoluteUri.Substring((endOfRootFolder + rootFolder.Length), Request.Url.AbsoluteUri.Length - (endOfRootFolder + rootFolder.Length))).Replace("%20", " ");

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

                if (matchedNode != null)
                {
                    lblScreenTitle.Text = matchedNode.ValuePath;
                }
                else
                {
                    lblScreenTitle.Text = "";
                }

                if (lblScreenTitle.Text == "")
                {
                    lblScreenTitle.Text = "Reports/Intl. Incoming/Traffic";
                }
                //End of Site Map Part *******************************************************************

            }

            hidValueSubmitClickFlag.Value = "false";
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
            DateTime anyDayOfMonth = new DateTime(int.Parse(TextBoxYear.Text), int.Parse(DropDownListMonth.SelectedValue), 15);
            txtDate.Text = FirstDayOfMonthFromDateTime(anyDayOfMonth).ToString("yyyy-MM-dd 00:00:00");
        }
        protected void DropDownListMonth1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //select 15th of month to find out first and last day of a month as it exists in all months.
            DateTime anyDayOfMonth = new DateTime(int.Parse(TextBoxYear1.Text), int.Parse(DropDownListMonth1.SelectedValue), 15);
            txtDate1.Text = LastDayOfMonthFromDateTime(anyDayOfMonth).ToString("yyyy-MM-dd 23:59:59");
        }
        protected void ButtonTemplate_Click(object sender, EventArgs e)
        {
            //exit if cancel clicked in javascript...
            if (hidValueTemplate.Value == null || hidValueTemplate.Value == "")
            {
                return;
            }

            //check for duplicate templatename and alert the client...
            string templateName = hidValueTemplate.Value;
            if (templateName == "")
            {
                string script = "alert('Templatename cannot be empty!');";
                ClientScript.RegisterClientScriptBlock(this.GetType(), "Alert", script, true);
                return;
            }
            else if (templateName.IndexOf('=') >= 0 || templateName.IndexOf(':') >= 0 ||
                templateName.IndexOf(',') >= 0 || templateName.IndexOf('?') >= 0)
            {
                string script = "alert('Templatename cannot contain characters =:,?');";
                ClientScript.RegisterClientScriptBlock(this.GetType(), "Alert", script, true);
                return;
            }
            using (PartnerEntities context = new PartnerEntities())
            {
                if (context.reporttemplates.Any(c => c.Templatename == templateName))
                {
                    string script = "alert('Templatename: " + templateName + " exists, try a different name.');";
                    ClientScript.RegisterClientScriptBlock(this.GetType(), "Alert", script, true);
                    return;
                }
            }
            string rootFolder = "reports";
            string[] paths = Request.Url.AbsoluteUri.Split('/');
            List<string> normalizedPathParts = new List<string>();
            normalizedPathParts.Add("~");
            bool rootFolderFound = false;
            foreach (string path in paths)
            {
                if (path.ToLower() == "reports")
                {
                    rootFolderFound = true;
                }
                if (rootFolderFound == false) continue;
                normalizedPathParts.Add(path);
            }
            string urlWithQueryString = string.Join("/", normalizedPathParts);
            string urlWithoutQs = urlWithQueryString.Split('?')[0];
            CommonCode commonCode = new CommonCode();
            string retVal = commonCode.SaveTemplateControlsByPage(this, templateName, urlWithoutQs);
            TreeView masterTree = (TreeView)Page.Master.FindControl("Treeview1");
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
            Label lblScreenTitle = (Label)Master.FindControl("lblScreenTitle");
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
                ClientScript.RegisterClientScriptBlock(this.GetType(), "Alert", scrSuccess, true);
            }

        }

    </script>


</asp:Content>

<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">

    <div id="report" style="clear: both; height: 25px; background-color: white; padding-left: 5px; width: 1009px; margin-bottom: 2px;">

        <script type="text/javascript">
            function ToggleParamBorderDiv() {
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


        <span style="padding-left: 0px; float: left; left: 0px; font-weight: bold; margin-top: 2px; margin-right: 20px; color: Black;">Report:</span>
        <span style="font-weight: bold;">Source</span>
         <asp:DropDownList ID="DropDownListReportSource" runat="server" >
         <asp:ListItem Value="sum_voice_day_">Day Wise</asp:ListItem>
         <asp:ListItem Value="sum_voice_hr_">Hour Wise</asp:ListItem>
        
     </asp:DropDownList>


        <asp:Button ID="submit" runat="server" Text="Show Report" OnClick="submit_Click" OnClientClick="SethidValueSubmitClickFlag('true');" />
        <asp:Button ID="Button1" runat="server" OnClick="Button1_Click"
            Style="margin-left: 0px" Text="Export" Visible="False" />
        <asp:Button ID="ShowHideFilter" runat="server" ViewStateMode="Enabled"
            Style="margin-left: 0px" Text="Hide Filter" Visible="True" OnClientClick="ToggleParamBorderDiv();return false;" />
        <asp:Button ID="ButtonTemplate" runat="server" OnClientClick="var value = prompt('Enter name of the Report Template:'); SetHidValueTemplate(value);" OnClick="ButtonTemplate_Click"
            Style="margin-left: 0px" Text="Save as Template" Visible="True" />
        <asp:Label ID="Label1" runat="server" Text="" ForeColor="Red"></asp:Label>
        <span style="font-weight: bold;">Real Time Update 
            <asp:CheckBox ID="CheckBoxRealTimeUpdate" runat="server" AutoPostBack="true" OnCheckedChanged="CheckBoxRealTimeUpdate_CheckedChanged" /></span>
        <span style="font-weight: bold;">Update Duration Last  
        <asp:TextBox ID="TextBoxDuration" runat="server" Text="30" Width="30px" OnTextChanged="TextBoxDuration_TextChanged" Enabled="false"></asp:TextBox>
            Minutes</span>
        <input type="hidden" id="hidValueFilter" runat="server" />
        <input type="hidden" id="hidValueSubmitClickFlag" runat="server" value="false" />
        <input type="hidden" id="hidValueTemplate" runat="server" />
    </div>

    <div>
        <ajaxToolkit:ToolkitScriptManager ID="ToolkitScriptManager1" runat="server"></ajaxToolkit:ToolkitScriptManager>

    </div>

    <div id="ParamBorder" style="float: left; padding-top: 3px; padding-left: 10px; height: 135px; display: block; border: 2px ridge #E5E4E2; margin-bottom: 5px; width: 1300px;">
        <div style="height: 20px; background-color: #f2f2f2; color: black;">
            <span style="float: left; font-weight: bold; padding-left: 20px;">Show Performance
                <asp:CheckBox ID="CheckBoxShowPerformance" runat="server" Checked="true" /></span>
            <%--<div style="clear:right;"></div>--%>
            <span style="float: left; font-weight: bold; padding-left: 20px;">Show Revenue
                <asp:CheckBox ID="CheckBoxShowCost" runat="server" Checked="false" /></span>
        </div>
        <br />
        <%--date time div--%>
        <div id="DateTimeDiv" style="padding-left: 5px; position: relative; float: left; left: 10px; top: -11px; width: 630px; z-index: 10; background-color: #F7F6F3; height: 70px;">
            <%--Start OF date time/months field DIV--%>
            <span style="padding-left: 0px;">Start Year/Month: 
        <asp:TextBox ID="TextBoxYear" runat="server" Text="" Width="30px"></asp:TextBox>
                Month
                <asp:DropDownList ID="DropDownListMonth" runat="server"
                    OnSelectedIndexChanged="DropDownListMonth_SelectedIndexChanged"
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
                Month
                <asp:DropDownList ID="DropDownListMonth1" runat="server"
                    OnSelectedIndexChanged="DropDownListMonth1_SelectedIndexChanged" AutoPostBack="True">
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


            <div style="float: left; width: 280px;">
                Start Date [Time]
                <asp:TextBox ID="txtDate" runat="server" />
                <asp:CalendarExtender ID="CalendarStartDate" runat="server"
                    TargetControlID="txtDate" PopupButtonID="txtDate" Format="yyyy-MM-dd 00:00:00">
                </asp:CalendarExtender>


            </div>

            <div style="float: left; width: 280px;">
                End Date [Time]
                <asp:TextBox ID="txtDate1" runat="server" />
                <asp:CalendarExtender ID="CalendarEndDate" runat="server"
                    TargetControlID="txtDate1" PopupButtonID="txtDate1" Format="yyyy-MM-dd 23:59:59">
                </asp:CalendarExtender>
            </div>

            <div style="font-size: smaller; text-align: left; overflow: visible; clear: left; color: #8B4500;">[Enter only Date in "dd/MM/yyyy (e.g. 21/11/2012) or Date+Time in "dd/MM/yyyy HH:mm:ss" (e.g. 21/11/2012 19:01:59) format]</div>

        </div>
        <%--END OF date time/months field DIV--%>
        <div id="TimeSummary" style="float: left; margin-left: 15px; padding-left: 20px; height: 70px; width: 280px; background-color: #faebd7; margin-top: -10px;">
            <div style="font-weight: bold; float: left;">Period wise Summary<asp:CheckBox ID="CheckBoxDailySummary" runat="server" /></div>
            <div style="clear: left; margin-top: 5px;"></div>
            <div style="float: left; margin-right: 5px;">
                <%--<asp:RadioButton ID="RadioButtonHalfHourly" Text="Half Hourly" GroupName="Time" runat="server" AutoPostBack="false" /><br />--%>
                <asp:RadioButton ID="RadioButtonHourly" Text="Hourly" GroupName="Time" runat="server" AutoPostBack="false" />
            </div>
            <div style="float: left; margin-right: 5px;">
                <asp:RadioButton ID="RadioButtonDaily" Text="Daily" GroupName="Time" runat="server" AutoPostBack="false" Checked="true" /><br />
                <asp:RadioButton ID="RadioButtonWeekly" Text="Weekly" GroupName="Time" runat="server" AutoPostBack="false" Checked="false" /><br />
            </div>
            <div style="float: left;">
                <asp:RadioButton ID="RadioButtonMonthly" Text="Monthly" GroupName="Time" runat="server" AutoPostBack="false" Checked="false" /><br />
                <asp:RadioButton ID="RadioButtonYearly" Text="Yearly" GroupName="Time" runat="server" AutoPostBack="false" />
            </div>
        </div>

        <div id="PartnerFilter" style="margin-top: -4px; margin-left: 10px; float: left; padding-left: 5px; background-color: #f2f2f2;">
            <%--<span style="font-size: smaller;position:relative;left:-53px;padding-left:0px;clear:right;">[Enter only Date in "dd/MM/yyyy (e.g. 21/11/2012) or Date+Time in "dd/MM/yyyy HH:mm:ss" (e.g. 21/11/2012 19:01:59) format]</span>   --%>

            <div style="text-align: left;">

                <div style="float: left; height: 25px; min-width: 1400px;">

                    <div style="float: left;">
                        View by In Partner: 
                            <asp:CheckBox ID="CheckBoxPartner" runat="server" AutoPostBack="True"
                                OnCheckedChanged="CheckBoxShowByPartner_CheckedChanged" Checked="True" />
                       
                        <asp:DropDownList ID="DropDownListPartner" runat="server"
                            Enabled="true">
                        </asp:DropDownList>

                    </div>
                    <div style="float: left;">
                        View by Out Partner: 
                        <asp:CheckBox ID="CheckBoxOutPartner" runat="server" AutoPostBack="True"
                                      OnCheckedChanged="CheckBoxOutPartner_OnCheckedChanged" Checked="True" />
                       
                        <asp:DropDownList ID="DropDownListOutPartner" runat="server"
                                          Enabled="true">
                        </asp:DropDownList>

                    </div>
                    <div style="float: left; margin-left: 15px;">
                        <asp:CheckBox ID="CheckBoxMatchedCustomerPrefix" runat="server" AutoPostBack="false" Checked="false" Text="View By Customer Prefix" />
                    </div>
                    <div style="float: left; margin-left: 15px;">
                        <asp:CheckBox ID="CheckBoxMatchedSupplierPrefix" runat="server" AutoPostBack="false" Checked="false" Text="View By Supplier Prefix" />
                    </div>
<%--                    <div style="float: left; margin-left: 15px;">
                        View by ANS: 
                                <asp:CheckBox ID="CheckBoxShowByAns" runat="server" AutoPostBack="True"
                                    OnCheckedChanged="CheckBoxShowByAns_CheckedChanged" Checked="false" />
                        <asp:DropDownList ID="DropDownListAns" runat="server"
                            Enabled="False">
                        </asp:DropDownList>

                    </div>

                    <div style="float: left; margin-left: 18px;">
                        View by ICX/IOS:
                        <asp:CheckBox ID="CheckBoxShowByIgw" runat="server"
                            AutoPostBack="True" OnCheckedChanged="CheckBoxShowByIgw_CheckedChanged" Checked="false" />
                        <asp:DropDownList ID="DropDownListIgw" runat="server"
                             Enabled="False">
                        </asp:DropDownList>

                    </div>--%>
                    &nbsp;&nbsp;

                <div style="width: 750px; padding-left: 28px;">

                    <span style="width: 300px; padding-left: 127px;">&nbsp;</span>

                    <span style="width: 50px; padding-left: 100px;">&nbsp;</span>


                </div>

                </div>

            </div>
        </div>
        <%--End Div Partner***************************************************--%>
    </div>
    <%--Param Border--%>
    <%--Common--%>

    <asp:SqlDataSource ID="SqlDataSource4" runat="server"
        ConnectionString="<%$ ConnectionStrings:Reader %>"
        ProviderName="<%$ ConnectionStrings:Reader.ProviderName %>"
        SelectCommand="AllAns" SelectCommandType="StoredProcedure"></asp:SqlDataSource>

    <asp:SqlDataSource ID="SqlDataSource3" runat="server"
        ConnectionString="<%$ ConnectionStrings:Reader %>"
        ProviderName="<%$ ConnectionStrings:Reader.ProviderName %>"
        SelectCommand="AllICX" SelectCommandType="StoredProcedure"></asp:SqlDataSource>




    <div>
        <%--ListView Goes Here*******************--%>
        <div style="float: left; text-align: left; width: 900px; position: relative; top: 0px; padding-left: 5px;">


            <div style="text-align: center;">
                <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="False"
                    CellPadding="4" ForeColor="#333333" GridLines="Vertical"
                    OnRowDataBound="GridView1_RowDataBound">
                    <AlternatingRowStyle BackColor="White" ForeColor="#284775" />
                    <Columns>
                        <asp:BoundField DataField="Date" HeaderText="Date" SortExpression="Date" ItemStyle-Wrap="false" />
                        <asp:BoundField DataField="In Partner" HeaderText="In Partner" SortExpression="In Partner" />
                        <asp:BoundField DataField="Out Partner" HeaderText="Out Partner" SortExpression="Out Partner" />
                        <asp:BoundField DataField="tup_matchedprefixcustomer" HeaderText="Customer Prefix" SortExpression="tup_matchedprefixcustomer" />
                        <asp:BoundField DataField="tup_matchedprefixsupplier" HeaderText="Supplier Prefix" SortExpression="tup_matchedprefixsupplier" />

                        <asp:BoundField DataField="Total Calls"
                            HeaderText="Total Calls"
                            SortExpression="Total Calls" />

                        <asp:BoundField DataField="Successful Calls"
                            HeaderText="Successful Calls"
                            SortExpression="Successful Calls" />

                        <asp:BoundField DataField="Connected Calls"
                            HeaderText="Connected Calls"
                            SortExpression="Connected Calls" />

                        <asp:BoundField DataField="Customer Duration"
                            DataFormatString="{0:F2}"
                            HeaderText="Customer Duration"
                            SortExpression="Customer Duration" />
                        <asp:BoundField DataField="Supplier Duration"
                            DataFormatString="{0:F2}"
                            HeaderText="Supplier Duration"
                            SortExpression="Supplier Duration" />
                        <asp:BoundField DataField="Cost"
                            DataFormatString="{0:F2}"
                            HeaderText="Cost"
                            SortExpression="Cost" />
                        <asp:BoundField DataField="Revenue"
                            DataFormatString="{0:F2}"
                            HeaderText="Revenue"
                            SortExpression="Revenue" />
                        <asp:BoundField DataField="Margin"
                            DataFormatString="{0:F2}"
                            HeaderText="Margin"
                            SortExpression="Margin" />
                        <asp:BoundField DataField="ASR"
                            DataFormatString="{0:F2}"
                            HeaderText="ASR"
                            SortExpression="ASR" />
                        <asp:BoundField DataField="ACD"
                            DataFormatString="{0:F2}"
                            HeaderText="ACD"
                            SortExpression="ACD" />
                        <asp:BoundField DataField="PDD"
                            DataFormatString="{0:F2}"
                            HeaderText="PDD"
                            SortExpression="PDD" />
                        <asp:BoundField DataField="CCR"
                            DataFormatString="{0:F2}"
                            HeaderText="CCR"
                            SortExpression="CCR" />
                        <asp:BoundField DataField="ConectbyCC"
                            DataFormatString="{0:F0}"
                            HeaderText="Connect Count (CC)"
                            SortExpression="ConnectByCC" />
                        <asp:BoundField DataField="CCRByCC"
                            DataFormatString="{0:F2}"
                            HeaderText="CCR By CC"
                            SortExpression="CCRByCC" />
                    </Columns>
                    <HeaderStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
                    <EditRowStyle BackColor="#999999" />
                    <FooterStyle BackColor="#5D7B9D" Font-Bold="true" ForeColor="White" />
                    <PagerStyle BackColor="#284775" ForeColor="White" HorizontalAlign="Center" />
                    <RowStyle BackColor="#F7F6F3" ForeColor="#333333" />
                    <SelectedRowStyle BackColor="#E2DED6" Font-Bold="True" ForeColor="#333333" />
                    <SortedAscendingCellStyle BackColor="#E9E7E2" />
                    <SortedAscendingHeaderStyle BackColor="#506C8C" />
                    <SortedDescendingCellStyle BackColor="#FFFDF8" />
                    <SortedDescendingHeaderStyle BackColor="#6F8DAE" />
                </asp:GridView>
            </div>

        </div>
        <asp:Timer ID="Timer1" Interval="300000" runat="server" OnTick="Timer1_Tick"></asp:Timer>
    </div>

</asp:Content>

