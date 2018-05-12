﻿<%@ Page Title="" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="True" CodeBehind="BatchJob.aspx.cs" Inherits="ConfigBatcJob" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>
<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">
   
    <script src="../Scripts/moment.js" type="text/javascript"></script>

<%--Page Load and Other Server Side Asp.net scripts--%>
       


</asp:Content>

<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">
   
    
    <%--<asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>--%>
    <ajaxToolkit:ToolkitScriptManager runat="Server" EnableScriptGlobalization="true"
        EnableScriptLocalization="true" ID="ScriptManager1" />
    <span style="width:50px;padding-left:50px;"></span>
     <div style="float: left;visibility:visible;">
           Job Name: 
     <asp:TextBox ID="TextBoxJobName" runat="server" Width="200px"></asp:TextBox>
     
        
         Job Type 
        <asp:DropDownList ID="DropDownListJobType" runat="server" onchange="SetCdrSource();">
         
     </asp:DropDownList>
    
    Batch Size: 
     <asp:TextBox ID="TextBoxBatchSize" runat="server" Text="5000"></asp:TextBox>

         

        </div>
 
 
    <div style="clear:left"></div>
 <div id="ParamBorder" style="overflow:hidden;margin-top:4px;float:left;padding-top:0px;padding-left:0px;height:190px;display:block;border:2px ridge #E5E4E2;margin-bottom:0px;width:1400px;">
    
    <%--<div style="margin-left:10px;float:left;left:0px;font-weight:bold;margin-top:2px;margin-right:20px;color:Black;clear:both;"> Rates</div>--%>

    <div style="height:2px;clear:both;"></div>
    <%--date time div--%>
     

      <asp:UpdatePanel ID="UpdatePanel2" runat="server">
            
            <ContentTemplate>
            

  <div id="PartnerFilter" style="height: 100px; margin-bottom: 5px; padding-top: 5px; width: 1285px; margin-top: 5px; margin-left: 0px; float: left; padding-left: 5px; background-color: #F7F6F3;">


        

         <asp:UpdatePanel ID="UpdatePanel1" runat="server">
             <Triggers>
                <asp:PostBackTrigger ControlID="ButtonCreateJob" />
             </Triggers>

             
             <ContentTemplate>
               <div style="float: left;margin-left:-5px; margin-top: -10px; min-width: 300px; text-align: left;">
                     <div style="text-align: right; float: left;">
                            <div id="DateTimeDiv" style="overflow:hidden;padding-top:2px;text-align:left;padding-left:15px;background-color:#f2f2f2;color:black;height:225px;min-width:1275px;"> <%--Start OF date time/months field DIV--%>
        
        

        <div style="float:left;padding-top:4px;">

        <div>
        
        <span style="padding-left:2px;">Start Year/Month:</span>
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
      <span style="padding-left:64px;">End Year/Month: </span>
      
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
            TargetControlID="txtDate"  PopupButtonID="txtDate" Format="yyyy-MM-dd 00:00:00">
        </asp:CalendarExtender>     
    </div>
    
        <div style="float:left;padding-top:5px;">
        <span style="padding-left:10px;">End Date [Time]:</span>  <asp:TextBox id="txtDate1" Runat="server" /> <span style="padding-left:0px;font-weight:bold;"></span>
        <asp:CalendarExtender ID="CalendarEndDate" runat="server" 
            TargetControlID="txtDate1"  PopupButtonID="txtDate1" Format="yyyy-MM-dd 23:59:59">
        </asp:CalendarExtender> 
    </div>

             <div style="font-size:smaller;text-align:left;overflow:visible;clear:left;color:#8B4500;padding-top:4px;">
        [Enter only Date in "yyyy-MM-dd (e.g. 2012-11-21) or Date+Time in "yyyy-MM-dd HH:mm:ss" (e.g. 2012-11-21 19:01:59) format]</div>

   
        </div>
        
          <script type="text/javascript">

              

              function SetCdrSource()
              {
                  var ddlistSource = document.getElementById("<%= this.DropDownListSource.ClientID %>");
                  var ddlistErrReason = document.getElementById("<%= this.DropDownListErrorReason.ClientID %>");
                  var ddlistJobType = document.getElementById("<%= this.DropDownListJobType.ClientID %>");
                  
                  
                  if (ddlistJobType.value === '2')
                  {
                      ddlistSource.value = '1';
                      ddlistErrReason.disabled = false;
                  }
                  else if (ddlistJobType.value === '3' || ddlistJobType.value === '4')
                  {
                      ddlistSource.value = '0';
                      ddlistErrReason.disabled = true;
                  }
              }

              function DateSelect()
              {   
                  var starttxt = document.getElementById("<%= this.txtDate.ClientID %>");
                  var endtxt = document.getElementById("<%= this.txtDate1.ClientID %>");

                  var lasthour = document.getElementById("<%= this.RadioButtonHalfHourly.ClientID %>");
                  var today = document.getElementById("<%= this.RadioButtonHourly.ClientID %>");

                  var yesterday = document.getElementById("<%= this.RadioButtonYesterday.ClientID %>");
                  var last2 = document.getElementById("<%= this.RadioButtonLast2Days.ClientID %>");

                  var last3 = document.getElementById("<%= this.RadioButtonDaily.ClientID %>");
                  var last7 = document.getElementById("<%= this.RadioButtonWeekly.ClientID %>");

                <%--  var last6m = document.getElementById("<%= RadioButtonLastSixm.ClientID %>");
                  var next6m = document.getElementById("<%= RadioButtonNextSixm.ClientID %>");

                  var last1y = document.getElementById("<%= RadioButtonLastY.ClientID %>");
                  var next1y = document.getElementById("<%= RadioButtonNextY.ClientID %>");--%>

                  var now = moment();
                  
                  if (lasthour.checked === true)
                  {
                      var enddate = moment(now);
                      var startdate = enddate.clone();
                      var startdate = startdate.subtract(1, 'hours');

                      starttxt.value = startdate.format("YYYY-MM-DD HH:mm:ss");
                      endtxt.value = enddate.format("YYYY-MM-DD HH:mm:ss");

                  }
                  else if (today.checked === true)
                  {
                      var toDay = moment(today).startOf('day');
                      starttxt.value = toDay.format("YYYY-MM-DD 00:00:00");
                      endtxt.value = toDay.format("YYYY-MM-DD 23:59:59");
                  }
                  else if (yesterday.checked === true) {
                      var toDay = moment(today).startOf('day');
                      var startdate = toDay.clone().subtract(1, 'days');
                      starttxt.value = startdate.format("YYYY-MM-DD 00:00:00");
                      endtxt.value = startdate.format("YYYY-MM-DD 23:59:59");
                  }
                  else if (last2.checked === true) {
                      var toDay = moment(today).startOf('day');
                      var startdate = toDay.clone().subtract(1, 'days');
                      starttxt.value = startdate.format("YYYY-MM-DD 00:00:00");
                      endtxt.value = toDay.format("YYYY-MM-DD 23:59:59");
                  }
                  else if (last3.checked === true) {
                      var toDay = moment(today).startOf('day');
                      var startdate = toDay.clone().subtract(2, 'days');
                      starttxt.value = startdate.format("YYYY-MM-DD 00:00:00");
                      endtxt.value = toDay.format("YYYY-MM-DD 23:59:59");
                  }
                  else if (last7.checked === true) {
                      var toDay = moment(today).startOf('day');
                      var startdate = toDay.clone().subtract(6, 'days');
                      starttxt.value = startdate.format("YYYY-MM-DD 00:00:00");
                      endtxt.value = toDay.format("YYYY-MM-DD 23:59:59");
                  }
                  //else if (next7.checked === true) {
                  //    Addition = 8;
                  //    Substraction = 0;
                  //}
                  //else if (next30.checked === true) {
                  //    Addition = 31;
                  //    Substraction = 0;
                  //}
                  //else if (last6m.checked === true) {
                  //    Addition = 1;
                  //    Substraction = 180;
                  //}
                  //else if (next6m.checked === true) {
                  //    Addition = 181;
                  //    Substraction = 0;
                  //}
                  //else if (last1y.checked === true) {
                  //    Addition = 1;
                  //    Substraction = 365;
                  //}
                  //else if (next1y.checked === true) {
                  //    Addition = 365;
                  //    Substraction = 0;
                  //}
                  
                 

                  return false;

              }
            </script>

    <div id="TimeSummary" style="float:left;margin-left:15px;padding-left:20px;height:70px;width: 345px;background-color: #faebd7;margin-top: 0px;">
        <div style="font-weight:bold;float:left;">Quick Select Period<asp:CheckBox ID="CheckBoxDailySummary" Visible="false" runat="server" />
            <%--<span style="padding-left:10px;"><asp:CheckBox ID="CheckBoxAllTime" runat="server" 
                Text="All Period when viewing one Rate Plan" /></span>--%>
        </div> 
        <div style="clear:left;margin-top:5px;"></div>
        <div style="float:left; margin-right:5px;">
            <asp:RadioButton onclick="DateSelect();" ID="RadioButtonHalfHourly" Checked="false" Text="Last Hour" GroupName="Time" runat="server" AutoPostBack="false"/><br />
            <asp:RadioButton onclick="DateSelect();"  ID="RadioButtonHourly" Text="Today" GroupName="Time" runat="server" AutoPostBack="false"/>
        </div>

        <div style="float:left;margin-right:5px;">
            <asp:RadioButton onclick="DateSelect();"   ID="RadioButtonYesterday" Text="Yesterday" GroupName="Time" runat="server" AutoPostBack="false" Checked="false"/><br />
            <asp:RadioButton onclick="DateSelect();"   ID="RadioButtonLast2Days" Text="Last 2 days" GroupName="Time" runat="server" AutoPostBack="false" Checked="false"/><br />
        </div>

        <div style="float:left;margin-right:5px;">
            <asp:RadioButton onclick="DateSelect();"   ID="RadioButtonDaily" Text="Last 3 days" GroupName="Time" runat="server" AutoPostBack="false" Checked="true"/><br />
            <asp:RadioButton onclick="DateSelect();"   ID="RadioButtonWeekly" Text="Last 7 days" GroupName="Time" runat="server" AutoPostBack="false" Checked="false"/><br />
        </div>
        
    </div>    
                                

                               <div style="height:5px;clear:left;"></div>
                                    
                                    <div style="float: left;padding-left:4px;">
                                        <asp:UpdatePanel ID="UpdatePanel3" runat="server">
                                            <ContentTemplate>
                                        
                                            Switch: 
                                                 <asp:DropDownList ID="DropDownListSwitch" runat="server" AutoPostBack="true"
                                                     OnSelectedIndexChanged="DropDownListSwitch_SelectedIndexChanged">
                                                 </asp:DropDownList>
                                        
                                        
                                        
                                                <span style="padding-left: 5px;">Ingress Partner:</span>
                                                <asp:DropDownList ID="DropDownListInPartner" runat="server" OnSelectedIndexChanged="DropDownListInPartner_SelectedIndexChanged"
                                                    Enabled="true" AutoPostBack="true">
                                                </asp:DropDownList>
                                                <span style="padding-left: 5px;">Ingress Route:</span>
                                                <asp:DropDownList ID="DropDownListInRoute" runat="server"
                                                    Enabled="true" AutoPostBack="false">
                                                </asp:DropDownList>
                                                <span style="padding-left: 5px;">Egress Partner:</span>
                                                <asp:DropDownList ID="DropDownListOutPartner" runat="server" OnSelectedIndexChanged="DropDownListOutPartner_SelectedIndexChanged"
                                                    Enabled="true" AutoPostBack="true">
                                                </asp:DropDownList>
                                                <span style="padding-left: 5px;">Egress Route:</span>
                                                <asp:DropDownList ID="DropDownListOutRoute" runat="server"
                                                    Enabled="true" AutoPostBack="false">
                                                </asp:DropDownList>
                                            </ContentTemplate>
                                        </asp:UpdatePanel>
                                    </div>
                                  

                                    <div style="clear: left;">

                                        <div style="float: left; padding-top: 5px;">
                                            <span style="padding-left: 5px;">Ingress Called Number</span>
                                            <asp:DropDownList ID="ddlistIngressCalled" runat="server"
                                                Enabled="true" AutoPostBack="false">
                                                <asp:ListItem Text="Starts With" Value="0" />
                                                <asp:ListItem Text="Exactly Matches" Value="1" />
                                                <asp:ListItem Text="Contains" Value="2" />
                                            </asp:DropDownList>

                                            <span style="padding-left: 5px;"></span>
                                            <asp:TextBox ID="TextBoxIngressCalled" runat="server"></asp:TextBox>
                                            <span style="padding-left: 5px;">Ingress Calling Number</span>
                                            <asp:DropDownList ID="ddlistIngressCalling" runat="server"
                                                Enabled="true" AutoPostBack="false">
                                                <asp:ListItem Text="Starts With" Value="0" />
                                                <asp:ListItem Text="Exactly Matches" Value="1" />
                                                <asp:ListItem Text="Contains" Value="2" />
                                            </asp:DropDownList>
                                            <span style="padding-left: 5px;"></span>
                                            <asp:TextBox ID="TextBoxIngressCalling" runat="server"></asp:TextBox>
                                        </div>

                                        <div style="clear: left; padding-top: 5px; padding-left: 4px;">
                                            <span style="padding-left: 5px;">Egress Called Number</span>
                                            <asp:DropDownList ID="ddlistEgressCalled" runat="server"
                                                Enabled="true" AutoPostBack="false">
                                                <asp:ListItem Text="Starts With" Value="0" />
                                                <asp:ListItem Text="Exactly Matches" Value="1" />
                                                <asp:ListItem Text="Contains" Value="2" />
                                            </asp:DropDownList>

                                            <span style="padding-left: 5px;"></span>
                                            <asp:TextBox ID="TextBoxEgressCalled" runat="server"></asp:TextBox>
                                            <span style="padding-left: 9px;">Egress Calling Number</span>
                                            <asp:DropDownList ID="ddlistEgressCalling" runat="server"
                                                Enabled="true" AutoPostBack="true">
                                                <asp:ListItem Text="Starts With" Value="0" />
                                                <asp:ListItem Text="Exactly Matches" Value="1" />
                                                <asp:ListItem Text="Contains" Value="2" />
                                            </asp:DropDownList>
                                            <span style="padding-left: 5px;"></span>
                                            <asp:TextBox ID="TextBoxEgressCalling" runat="server"></asp:TextBox>
                                        </div>

                                         <div style="float: left; clear: left; min-width: 1500px; margin-top: 10px;">

                                    <div style="float: left;">
                                        <div style="float: left;">
                                            <asp:UpdatePanel ID="UpdatePanel5" runat="server">
                                                <ContentTemplate>
                                                    <span style="padding-left: 5px;">Source
                            <asp:DropDownList ID="DropDownListSource" runat="server" Enabled="false" AutoPostBack="true" OnSelectedIndexChanged="DropDownListSource_SelectedIndexChanged">
                                <asp:ListItem Text="CDR" Value="0" />
                                <asp:ListItem Text="Error" Value="1" Selected="True" />
                            </asp:DropDownList>
                                                    </span>

                                             
                                                    <span style="padding-left:5px;">Service Group</span>
                                                    <asp:DropDownList ID="DropDownListServiceGroup" runat="server" Enabled="true">
                                                    </asp:DropDownList>
                                                    <span style="padding-left:5px;">Charging Status</span>
                                                    <asp:DropDownList ID="DropDownListChargingStatus" runat="server" Enabled="true">
                                                    <asp:ListItem Text="[All]" Value="0" />    
                                                        <asp:ListItem Text="Successful" Value="1"/>    
                                                        <asp:ListItem Text="Failed" Value="2" />    
                                                    </asp:DropDownList>
                                                    
                                                   <span style="padding-left:5px;visibility:hidden;">Field List</span>
                                                    <asp:DropDownList ID="DropDownListFieldList" runat="server" Enabled="false" Visible="false">
                                                        </asp:DropDownList>

                                                    <span style="padding-left: 5px;visibility:hidden;">

                                                        <asp:Label ID="lblErrorReason" runat="server" Text="Error Reason"></asp:Label>
                                                        <asp:DropDownList ID="DropDownListErrorReason" runat="server">
                                                            <asp:ListItem Text="N/A" Value="0" />
                                                        </asp:DropDownList>
                                                    </span>
                                                </ContentTemplate>
                                            </asp:UpdatePanel>
                                        </div>

                                    </div>

                                       

                                    </div>

                                </div>
    
   
</div> <%--END OF date time/months field DIV--%>
                        
                     </div>

                   
                 </div>

                


             </ContentTemplate>

         </asp:UpdatePanel>


         




     </div>
     
       <%--End Div Partner***************************************************--%>
                
                

                </ContentTemplate>
          </asp:UpdatePanel>
       
     
    
</div> <%--Param Border--%>

    <div style="clear:left;padding-top:5px;">
        <asp:Button ID="ButtonCreateJob" runat="server" Text="Create Job" Width="110px"
             OnClick="ButtonCreateJob_Click" />

         <asp:Label ID="lblStatus" runat="server" Text=""></asp:Label>
    </div>

    

<%--Common--%>
    
  


   
    
    
    
    
    
   
    
    

    

</asp:Content>