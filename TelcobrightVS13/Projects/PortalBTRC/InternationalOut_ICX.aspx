<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="True"
    CodeBehind="InternationalOut _ICX.aspx.cs" Inherits="DefaultRptIntlOutIcx" %>

<%--<%@ Import Namespace="MySql.Web" %>--%>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>


<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">

    <%--Page Load and Other Server Side Asp.net scripts--%>
    <script runat="server">


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
        <%--<span style="font-weight: bold;">Switch</span>--%>
        <asp:DropDownList ID="DropDownListReportSource" runat="server" Visible="False">
            <asp:ListItem Value="sum_voice_day_">Day Wise</asp:ListItem>
            <asp:ListItem Value="sum_voice_hr_">Hour Wise</asp:ListItem>

        </asp:DropDownList>
        
        
           View by Switch:
        <asp:CheckBox ID="ViewBySwitch" runat="server" AutoPostBack="True"
                      OnCheckedChanged="CheckBoxShowBySwitch_CheckedChanged" Checked="True" />
            <asp:DropDownList ID="DropDownListShowBySwitch" runat="server" Visible="true" Enabled="True">
            </asp:DropDownList>

        <asp:Button Style="margin-left: 5px" ID="submit" runat="server" Text="Show Report" OnClick="submit_Click" OnClientClick="SethidValueSubmitClickFlag('true');" />
        <asp:Button ID="Button1" runat="server" OnClick="Button1_Click"
            Style="margin-left: px" Text="Export" Visible="False" />
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

    <asp:UpdatePanel runat="server">
        <ContentTemplate>
            <div id="ParamBorder" style="float: left; padding-top: 3px; padding-left: 10px; height: 175px; display: block; border: 2px ridge #E5E4E2; margin-bottom: 5px; width: 1300px;">
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
                        <%--<asp:RadioButton ID="RadioButtonHalfHourly" Text="Half Hourly" GroupName="Time" runat="server" AutoPostBack="false"/><br />--%>
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
                <asp:TextBox ID="TextBoxUsdRate" runat="server" Visible="false"></asp:TextBox>
                <div id="PartnerFilter" style="min-width: 1285px; margin-top: -4px; margin-left: 10px; float: left; padding-left: 5px; background-color: #f2f2f2;">
                    
                    <div style="float: left;">
                        View by ICX: 
                        <asp:CheckBox ID="CheckBoxViewIncomingRoute" runat="server" AutoPostBack="True"
                                      OnCheckedChanged="CheckBoxViewIncomingRoute_CheckedChanged" Checked="True" />

                        <asp:DropDownList ID="DropDownListViewIncomingRoute" runat="server"
                                          Enabled="False">
                        </asp:DropDownList>

                    </div>

                    <div style="text-align: left; float: left; margin-left: 10px;">
                        View By Country:
                    <asp:CheckBox ID="CheckBoxShowByCountry" runat="server"
                        AutoPostBack="True" Checked="false" EnableViewState="true"
                        OnCheckedChanged="CheckBoxShowByCountry_CheckedChanged" />

                        <asp:DropDownList ID="DropDownListCountry" runat="server"
                            Enabled="False" AutoPostBack="True" OnSelectedIndexChanged="DropDownList1_SelectedIndexChanged">
                        </asp:DropDownList>


                    </div>
                    <div style="text-align: left; float: left; margin-left: 10px;">
                        View by Destination:
                    <asp:CheckBox ID="CheckBoxShowByDestination" runat="server"
                        AutoPostBack="True"
                        OnCheckedChanged="CheckBoxShowByDestination_CheckedChanged" Checked="false" />

                        <asp:DropDownList ID="DropDownPrefix" runat="server" DataSourceID=""
                            DataTextField="Destination" DataValueField="Prefix" Width="330" Enabled="False">
                        </asp:DropDownList>
                    </div>
                    <%--<br />--%>

                    <div style="height: 3px; clear: both;"></div>

                    <div style="float: left;">
                        <%--View by ANS:--%> 
                        <asp:CheckBox ID="CheckBoxShowByAns" runat="server" AutoPostBack="True"
                            OnCheckedChanged="CheckBoxShowByAns_CheckedChanged" Checked="false" Visible="False" />
                        <asp:DropDownList ID="DropDownListAns" runat="server"
                            Visible="False" Enabled="False">

                        </asp:DropDownList>
                    </div>

                    <div style="float: left;">
                        View by ANS:
                        <asp:CheckBox ID="CheckBoxShowByIgw" runat="server"
                            AutoPostBack="True" OnCheckedChanged="CheckBoxShowByIgw_CheckedChanged" Checked="false"/>

                        <asp:DropDownList ID="DropDownListIgw" runat="server" OnSelectedIndexChanged="DropDownListIgw_OnSelectedIndexChanged"
                            Enabled="False" AutoPostBack="True" >
                        </asp:DropDownList>

                    </div>



                    <div style="float: left; margin-left: 10px;">
                        View by IOS:
                    <asp:CheckBox ID="CheckBoxIntlPartner" runat="server" AutoPostBack="True" Checked="false"
                        OnCheckedChanged="CheckBoxShowByPartner_CheckedChanged" />
                        <asp:DropDownList ID="DropDownListIntlCarier" runat="server" OnSelectedIndexChanged="DropDownListIntlCarier_OnSelectedIndexChanged"
                            Enabled="false" AutoPostBack="True">
                        </asp:DropDownList>
                    </div>


                </div>
                <%--End Div Partner***************************************************--%>

                <div id="RouteFilter" style="margin-left: 10px; float: left; padding-left: 5px; background-color: #f2f2f2;">
                    <%--<span style="font-size: smaller;position:relative;left:-53px;padding-left:0px;clear:right;">[Enter only Date in "dd/MM/yyyy (e.g. 21/11/2012) or Date+Time in "dd/MM/yyyy HH:mm:ss" (e.g. 21/11/2012 19:01:59) format]</span>   --%>

                    <div style="text-align: left;">

                        <div style="float: left; height: 25px; min-width: 1285px;">
                            
                            <div style="float: left;">
                                View by Customer Rate:
                                <asp:CheckBox ID="CheckBoxShowByCustomerRate" runat="server" Checked="false" />
                            </div>


                            <div style="float: left; margin-left: 18px;">
                                View by IOS Route:
                            <asp:CheckBox ID="CheckBoxViewOutgoingRoute" runat="server"
                                AutoPostBack="True" OnCheckedChanged="CheckBoxViewOutgoingRoute_CheckedChanged" Checked="false" />
                                <asp:DropDownList ID="DropDownListViewOutgoingRoute" runat="server"
                                    Enabled="False">
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



            </div>
            <%--Param Border--%>
        </ContentTemplate>
    </asp:UpdatePanel>

         <%--ListView Goes Here*******************--%>

    <div style="/*height: 600px; overflow: auto; */">
        <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="False"
            ShowFooter="True"
            CellPadding="4" ForeColor="#333333" GridLines="Vertical"
            Font-Names="Arial-Narrow" OnRowDataBound="GridView1_RowDataBound">
            <AlternatingRowStyle BackColor="#f2f2f2" ForeColor="#284775" />
          <Columns>
                <asp:BoundField DataField="Date" HeaderText="Date" SortExpression="Date" />
              <asp:BoundField DataField="tup_incomingroute" HeaderText="ICX" SortExpression="tup_incomingroute" />
                <asp:BoundField DataField="Country" HeaderText="Country"
                    SortExpression="Country"/>
                <asp:BoundField DataField="Destination" HeaderText="Destination"
                    SortExpression="Destination" />
                <asp:BoundField DataField="IGW" HeaderText="ANSNotUse" SortExpression="ANSNotUse"  Visible="false"/>               
                <asp:BoundField DataField="ANS" HeaderText="ANS" SortExpression="ANS" />
                <asp:BoundField DataField="International Partner" HeaderText="IOS" SortExpression="International Partner" />
                <asp:BoundField DataField="tup_outgoingroute" HeaderText="IOS Route" SortExpression="tup_outgoingroute" />

                <asp:BoundField DataField="CallsCount"
                    DataFormatString="{0:#,0}"
                    HeaderText="Total Calls"
                    SortExpression="CallsCount" />

                <asp:BoundField DataField="No of Calls (Outgoing International)"
                    DataFormatString="{0:#,0}"
                    HeaderText="Succ. Calls"
                    SortExpression="No of Calls (Outgoing International)" />

                <asp:BoundField DataField="ConnectedCount"
                    DataFormatString="{0:#,0}"
                    HeaderText="Conn. Calls"
                    SortExpression="ConnectedCount" />

                <asp:BoundField DataField="Paid Minutes (Outgoing Internaitonal)"
                    DataFormatString="{0:#,0.#0}"
                    HeaderText="Actual Duration"
                    SortExpression="Paid Minutes (Outgoing Internaitonal)"></asp:BoundField>

                <asp:BoundField DataField="hmsduration"
                    DataFormatString="{0:#,0.#0}"
                    HeaderText="100ms Duration"
                    SortExpression="hmsduration"></asp:BoundField>

                <asp:BoundField DataField="roundedduration"
                    DataFormatString="{0:#,0.#0}"
                    HeaderText="Billed Duration"
                    SortExpression="roundedduration"></asp:BoundField>
                <asp:BoundField DataField="supplierduration"
                    DataFormatString="{0:#,0.#0}"
                    HeaderText="Supplier Duration"
                    SortExpression="supplierduration"></asp:BoundField>
                <asp:BoundField DataField="asr" DataFormatString="{0:#,0.#0}" HeaderText="ASR" SortExpression="asr" />
                <asp:BoundField DataField="acd" DataFormatString="{0:#,0.#0}" HeaderText="ACD" SortExpression="acd" />
                <asp:BoundField DataField="pdd" DataFormatString="{0:#,0.#0}" HeaderText="PDD" SortExpression="pdd" />
                <asp:BoundField DataField="ccr" DataFormatString="{0:#,0.#0}" HeaderText="CCR" SortExpression="ccr" />
                <asp:BoundField DataField="ConectbyCC"
                    DataFormatString="{0:F0}"
                    HeaderText="Connect Count (CC)"
                    SortExpression="ConectbyCC" />
                <asp:BoundField DataField="CCRByCC"
                    DataFormatString="{0:F2}"
                    HeaderText="CCR By CC"
                    SortExpression="CCRByCC" />

                <asp:BoundField DataField="X RATE(BDT)" DataFormatString="{0:#,0.#0}" HeaderText="X RATE(BDT)" SortExpression="X RATE(BDT)" />
                <asp:BoundField DataField="Y RATE(USD)" DataFormatString="{0:#,0.#0}" HeaderText="Y RATE(USD)" SortExpression="Y RATE(USD)" />
                <asp:BoundField DataField="Dollar Rate" DataFormatString="{0:#,0.#0}" HeaderText="Dollar Rate" SortExpression="Dollar Rate" />
                <asp:BoundField DataField="X (BDT)" DataFormatString="{0:#,0.#0}" HeaderText="X (BDT)" SortExpression="X (BDT)" />
                <asp:BoundField DataField="Y (USD)" DataFormatString="{0:#,0.#0}" HeaderText="Y (USD)" SortExpression="Y (USD)" />
                <asp:BoundField DataField="Z (BDT)" DataFormatString="{0:#,0.#0}" HeaderText="Z (BDT)" SortExpression="Z (BDT)" />
                <asp:BoundField DataField="revenueigwout" DataFormatString="{0:#,0.#0}" HeaderText="Revenue (BDT)" SortExpression="revenueigwout" />
                <asp:BoundField DataField="tax1" DataFormatString="{0:#,0.#0}" HeaderText="BTRC Revenue Share" SortExpression="tax1" />


                <%-- <asp:BoundField DataField="profitminute" HeaderText="Profit/Minute" SortExpression="profitminute" />--%>
            </Columns>
            <HeaderStyle BackColor="#086052" Font-Bold="True" ForeColor="White" />
            <EditRowStyle BackColor="#999999" />
            <FooterStyle BackColor="#086052" Font-Bold="true" ForeColor="White" />
            <%--<FooterStyle BackColor="#E0DCDF" Font-Bold="true" ForeColor="Black" />--%>
            <PagerStyle BackColor="#284775" ForeColor="White" HorizontalAlign="Center" />
            <RowStyle BackColor="white" ForeColor="#333333" />
            <SelectedRowStyle BackColor="#E2DED6" Font-Bold="True" ForeColor="#333333" />
            <SortedAscendingCellStyle BackColor="#E9E7E2" />
            <SortedAscendingHeaderStyle BackColor="#506C8C" />
            <SortedDescendingCellStyle BackColor="#FFFDF8" />
            <SortedDescendingHeaderStyle BackColor="#6F8DAE" />
        </asp:GridView>

    </div>

    <asp:Timer ID="Timer1" Interval="300000" runat="server" OnTick="Timer1_Tick"></asp:Timer>


</asp:Content>

