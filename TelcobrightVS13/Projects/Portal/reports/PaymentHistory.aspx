<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeBehind="PaymentHistory.aspx.cs" Inherits="PortalApp.reports.PaymentHistory" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>
<%@ Import Namespace="MediationModel" %>
<%@ Import Namespace="TelcobrightMediation" %>
<%@ Import Namespace="PortalApp" %>
<%@ Import Namespace="PortalApp._portalHelper" %>




<%----%>
<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">

<%--Page Load and Other Server Side Asp.net scripts--%>
<script runat="server">

    protected void Page_Load(object sender, EventArgs e)
    {
        TelcobrightConfig tbc = PageUtil.GetTelcobrightConfig();
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
            TextBoxYear.Text = System.DateTime.Now.ToString("yyyy");
            TextBoxYear1.Text = System.DateTime.Now.ToString("yyyy");
            DropDownListMonth.SelectedIndex = int.Parse(System.DateTime.Now.ToString("MM")) - 1;
            DropDownListMonth1.SelectedIndex = int.Parse(System.DateTime.Now.ToString("MM")) - 1;
            //txtDate.Text = FirstDayOfMonthFromDateTime(System.DateTime.Now).ToString("dd/MM/yyyy");
            //txtDate1.Text = LastDayOfMonthFromDateTime(System.DateTime.Now).ToString("dd/MM/yyyy");
            txtDate.Text = DateTime.Now.ToString("yyyy-MM-01 00:00:00");
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

            List<string> billableType = new List<string>()
            {
                "/custBilled", "/suppBilled", "/billable"
                //"/custBilled"
            };
            using (PartnerEntities context = new PartnerEntities())
            {
                var partners = context.partners.ToList();
                List<account> payableAccounts = context.accounts.Where(x => billableType.Contains(x.billableType)).ToList();
                DropDownListPartner.Items.Clear();
                DropDownListPartner.Items.Add(new ListItem(" [All]", "-1"));
                foreach (partner p in partners.OrderBy(x => x.PartnerName))
                {
                    account account = payableAccounts.FirstOrDefault(x => x.idPartner == p.idPartner);
                    if (account != null)
                    {
                        DropDownListPartner.Items.Add(new ListItem(p.PartnerName, p.idPartner.ToString()));
                    }
                }
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
                lblScreenTitle.Text = "Reports/Payment History";
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

        function SethidValueSubmitClickFlag(value) {
            document.getElementById("<%=hidValueSubmitClickFlag.ClientID%>").value = value;
        }

    </script>


    <span style="padding-left: 0px; float: left; left: 0px; font-weight: bold; margin-top: 2px; margin-right: 20px; color: Black;">Report Source:</span>

    <asp:Button ID="submit" runat="server" Text="Show Report" OnClick="ShowReport_Click" OnClientClick="SethidValueSubmitClickFlag('true');" />
    <asp:Button ID="Button1" runat="server" OnClick="Export_Click"
                Style="margin-left: 0px" Text="Export" Visible="False" />
    <asp:Button ID="ShowHideFilter" runat="server" ViewStateMode="Enabled"
                Style="margin-left: 0px" Text="Hide Filter" Visible="True" OnClientClick="ToggleParamBorderDiv();return false;" />
    <asp:Label ID="Label1" runat="server" Text="" ForeColor="Red"></asp:Label>
    <input type="hidden" id="hidValueFilter" runat="server" />
    <input type="hidden" id="hidValueSubmitClickFlag" runat="server" value="false" />
</div>

<div>
    <ajaxToolkit:ToolkitScriptManager ID="ToolkitScriptManager1" runat="server"></ajaxToolkit:ToolkitScriptManager>

</div>

<div id="ParamBorder" style="float: left; padding-top: 3px; padding-left: 10px; height: 115px; display: block; border: 2px ridge #E5E4E2; margin-bottom: 5px; width: 1300px;">
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
        <div style="font-weight: bold; float: left;"><asp:CheckBox ID="CheckBoxDailySummary" runat="server" Checked="True" /> Period wise Summary</div>
        <div style="clear: left; margin-top: 5px;"></div>
        <div style="float: left; margin-right: 5px;">
            <asp:RadioButton ID="RadioButtonDaily" Text="Daily" GroupName="Time" runat="server" AutoPostBack="false" Checked="True" /><br />
            <asp:RadioButton ID="RadioButtonMonthly" Text="Monthly" GroupName="Time" runat="server" AutoPostBack="false" Checked="False" /><br />
        </div>
        <div style="float: left;">
            <asp:RadioButton ID="RadioButtonYearly" Text="Yearly" GroupName="Time" runat="server" AutoPostBack="False" />
        </div>
    </div>

    <div id="PartnerFilter" style="margin-top: -4px; margin-left: 10px; float: left; padding-left: 5px; background-color: #f2f2f2;">
        <%--<span style="font-size: smaller;position:relative;left:-53px;padding-left:0px;clear:right;">[Enter only Date in "dd/MM/yyyy (e.g. 21/11/2012) or Date+Time in "dd/MM/yyyy HH:mm:ss" (e.g. 21/11/2012 19:01:59) format]</span>   --%>

        <div style="text-align: left;">

            <div style="float: left; height: 25px; min-width: 1285px;">

                <div style="float: left;">
                    View by Partner: 
                    <asp:CheckBox ID="CheckBoxPartner" runat="server" AutoPostBack="True"
                                  OnCheckedChanged="CheckBoxShowByPartner_CheckedChanged" Checked="True" />
                       
                    <asp:DropDownList ID="DropDownListPartner" runat="server"
                                      Enabled="true">
                    </asp:DropDownList>

                </div>
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
                          CellPadding="4" ForeColor="#333333" GridLines="Vertical">
                <AlternatingRowStyle BackColor="White" ForeColor="#284775" />
                <Columns>
                    <asp:BoundField DataField="Date" HeaderText="Date" SortExpression="Date" ItemStyle-Wrap="false" />
                    <asp:BoundField DataField="PartnerName" HeaderText="Partner" SortExpression="PartnerName" />
                    <asp:BoundField DataField="Currency" HeaderText="Currency" SortExpression="Currency" />
                    <asp:BoundField DataField="Amount"
                                    DataFormatString="{0:F2}"
                                    HeaderText="Payment Amount"
                                    SortExpression="Amount" />
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
</div>

</asp:Content>

