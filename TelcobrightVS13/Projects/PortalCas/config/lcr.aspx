<%@ Page Title="" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="True" CodeBehind="lcr.aspx.cs" Inherits="config_lcr" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>
<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">
   
    <script src="../Scripts/moment.js" type="text/javascript"></script>

<%--Page Load and Other Server Side Asp.net scripts--%>
       


</asp:Content>

<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">
   
 
    
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
    <asp:Button ID="ButtonTemplate" runat="server" 
            style="margin-left: 0px" Text="Save as Template" Visible="True"/>
    <asp:Label ID="Label1" runat="server" Text="" ForeColor="Red"></asp:Label> 
    <span style="font-weight:bold;"> Real Time Update 
            <asp:CheckBox ID="CheckBoxRealTimeUpdate" runat="server" AutoPostBack="true" Visible="false" /></span>
        <span style="font-weight:bold;"> Update Duration Last  
        <asp:TextBox ID="TextBoxDuration" runat="server" Text="30" Width="30px"  Enabled="false" ></asp:TextBox> Minutes</span>  

        </div>

        <input type="hidden" id="hidValueFilter" runat="server"/>
        <input type="hidden" id="hidValueSubmitClickFlag" runat="server" value="false"/>
        <input type="hidden" id="hidValueTemplate" runat="server" />
 

<div>
    <%--<ajaxToolkit:ToolkitScriptManager ID="ToolkitScriptManager1" runat="server" > </ajaxToolkit:ToolkitScriptManager>--%>
    <asp:ToolkitScriptManager runat="server"></asp:ToolkitScriptManager>
</div>
 
 <%--<asp:SqlDataSource ID="SqlDataRatePlan" runat="server" 
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
 on sf.servicecategory=sc.id) "></asp:SqlDataSource>--%>



 <div id="ParamBorder" style="overflow:hidden;margin-top:-40px;float:left;padding-top:0px;padding-left:0px;height:35px;display:block;border:2px ridge #E5E4E2;margin-bottom:5px;width:1400px;">
    
    <%--<div style="margin-left:10px;float:left;left:0px;font-weight:bold;margin-top:2px;margin-right:20px;color:Black;clear:both;"> Rates</div>--%>

    <div style="height:2px;clear:both;"></div>
    <%--date time div--%>
     

      <asp:UpdatePanel ID="UpdatePanel2" runat="server">
            
            <ContentTemplate>
            

  <div id="PartnerFilter" style="height: 100px; margin-bottom: 5px; padding-top: 2px; width: 1285px; margin-top: 0px; margin-left: 0px; float: left; padding-left: 5px; background-color: #F7F6F3;">


        

         <asp:UpdatePanel ID="UpdatePanel1" runat="server">
             <Triggers>
                <asp:PostBackTrigger ControlID="ButtonFind" />
                 <asp:PostBackTrigger ControlID="ButtonExport" />
            </Triggers>

             
             <ContentTemplate>
                 <div style="float: left; padding-left: 10px; margin-top: 0px; min-width: 1200px; text-align: left;padding-bottom:5px;">
                     <b>Reference Rate Plan: </b> 
                     <asp:DropDownList ID="DropDownListRatePlanLcr" runat="server">
    </asp:DropDownList>
                     

                     View Mode:
                     <asp:DropDownList ID="DropDownListViewMode" runat="server" onchange="ViewModeSelect();"
                         Enabled="true" AutoPostBack="false">
                         
                         <asp:ListItem Text="Incremental (By Date)" Value="1" />
                         <asp:ListItem Text="Incremental (By ID)" Value="2" />
                         <asp:ListItem Text="Full (Current)" Value="3" />
                         
                     </asp:DropDownList>

                          Prefix[*]:
                                    <asp:TextBox ID="TextBoxPrefix" runat="server"></asp:TextBox>   
                     <asp:Button ID="ButtonFind" runat="server" Text="Find" Width="117px" 
        onclick="ButtonFind_Click" />
                     <asp:Button ID="ButtonExport" runat="server" Text="Export" Width="117px" Visible="false"
        onclick="ButtonExport_Click" />
                     
                 </div>

                 

               <div id="ViewById" style="clear:left;text-align:left;padding-left:10px;background-color:#f2f2f2;min-width:1275px;visibility:hidden;height:0px;"> 
               Start ID>=    <asp:TextBox ID="TextBoxStartId" runat="server" Text="" Width="100px"></asp:TextBox>
                   End ID<=    <asp:TextBox ID="TextBoxEndId" runat="server" Text="" Width="100px"></asp:TextBox>
               </div>
                 <div style="float: left;margin-left:-5px; margin-top: 0px; min-width: 300px; text-align: left;">
                     <div style="text-align: right; float: left;">
                            <div id="DateTimeDiv" style="overflow:hidden;padding-top:2px;text-align:left;padding-left:15px;background-color:#f2f2f2;height:90px;min-width:1275px;"> <%--Start OF date time/months field DIV--%>
        
        <div style="font-weight:bold;">Select a period over which LCR prefixes are effective</div>

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

               function ShowFormViewLCR() {
                   
                   var frmView = document.getElementById("divFormView");
                   frmView.style.visibility = 'visible';
                   frmView.style.height = 30;
                   var divGridView = document.getElementById("divGridView");
                   divGridView.style.marginTop = 0;
                   
                   return false;
               }

               function HideFormViewLCR() {

                   var frmView = document.getElementById("divFormView");
                   frmView.style.visibility = 'hidden';
                   frmView.style.height = 0;
                   var divGridView = document.getElementById("divGridView");
                   divGridView.style.marginTop = -33;
                   return false;
               }

              function SetDefaultViewMode()
              {
                  
                  var ddlist = document.getElementById("<%= this.DropDownListViewMode.ClientID %>");
                  ddlist.selectedindex = 2;
              }


             

              function ViewModeSelect()
              {
                  var ddlist = document.getElementById("<%= this.DropDownListViewMode.ClientID %>");
                  var viewMode = ddlist.options[ddlist.selectedIndex].value;
                  
                  if (viewMode === "2")//incremental by id
                  {
                      
                      document.getElementById("ViewById").style.height='20px';
                      document.getElementById("ViewById").style.visibility = 'visible';
                      document.getElementById("DateTimeDiv").style.visibility = 'hidden';
                      document.getElementById("ParamBorder").style.height = '68px';
                      document.getElementById("ParamBorder").style.overflow = 'hidden';
                  }
                  else if (viewMode === "1")//incremental by date
                  {
                      
                      document.getElementById("ViewById").style.height = '0px';
                      document.getElementById("ViewById").style.visibility = 'hidden';
                      document.getElementById("DateTimeDiv").style.visibility = 'visible';
                      document.getElementById("ParamBorder").style.height = '130px';
                      document.getElementById("ParamBorder").style.overflow = 'hidden';
                  }
                  if (viewMode === "3"||viewMode === "4")//full current, current+history
                  {

                      //document.getElementById("ViewById").style.height = '20px';
                      //document.getElementById("ViewById").style.visibility = 'visible';
                      document.getElementById("DateTimeDiv").style.visibility = 'hidden';
                      document.getElementById("ParamBorder").style.height = '33px';
                      document.getElementById("ParamBorder").style.overflow = 'hidden';
                  }
              }
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

                <%--  var last6m = document.getElementById("<%= RadioButtonLastSixm.ClientID %>");
                  var next6m = document.getElementById("<%= RadioButtonNextSixm.ClientID %>");

                  var last1y = document.getElementById("<%= RadioButtonLastY.ClientID %>");
                  var next1y = document.getElementById("<%= RadioButtonNextY.ClientID %>");--%>

                  var now = moment();
                  var addition = 0;
                  var substraction = 0;
                  
                  if (recent.checked === true)
                  {
                      //last one month to any date in future
                      addition = 1;
                      substraction = 3;
                  }
                  else if (today.checked === true)
                  {
                      addition = 1;
                      substraction = 0;
                  }
                  else if (last7.checked === true) {
                      addition = 1;
                      substraction = 7;
                  }
                  else if (last30.checked === true) {
                      addition = 1;
                      substraction = 30;
                  }
                  else if (next7.checked === true) {
                      addition = 8;
                      substraction = 0;
                  }
                  else if (next30.checked === true) {
                      addition = 31;
                      substraction = 0;
                  }
                  else if (last6m.checked === true) {
                      addition = 1;
                      substraction = 180;
                  }
                  else if (next6m.checked === true) {
                      addition = 181;
                      substraction = 0;
                  }
                  else if (last1y.checked === true) {
                      addition = 1;
                      substraction = 365;
                  }
                  else if (next1y.checked === true) {
                      addition = 365;
                      substraction = 0;
                  }
                  var startdate = moment(now).subtract(substraction, 'days');
                  var enddate = moment(now).add(addition, 'days');

                  starttxt.value = startdate.format("YYYY-MM-DD");
                  endtxt.value = enddate.format("YYYY-MM-DD");

                  return false;

              }
            </script>

    <div id="TimeSummary" style="float:left;margin-left:15px;padding-left:20px;height:70px;width: 345px;background-color: #faebd7;margin-top: -7px;">
        <div style="font-weight:bold;float:left;">Quick Select Period<asp:CheckBox ID="CheckBoxDailySummary" Visible="false" runat="server" />
            <%--<span style="padding-left:10px;"><asp:CheckBox ID="CheckBoxAllTime" runat="server" 
                Text="All Period when viewing one Rate Plan" /></span>--%>
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
        <%--<div style="float:left;">
            <asp:RadioButton onclick="DateSelect();"   ID="RadioButtonLastSixm" Text="Last 6 Months" GroupName="Time" runat="server" AutoPostBack="false" Checked="false"/><br />
            <asp:RadioButton onclick="DateSelect();"   ID="RadioButtonNextSixm" Text="Next 6 Months" GroupName="Time" runat="server" AutoPostBack="false"/>
        </div>
         <div style="float:left;">
            <asp:RadioButton onclick="DateSelect();"   ID="RadioButtonLastY" Text="Last 1 Year" GroupName="Time" runat="server" AutoPostBack="false" Checked="false"/><br />
            <asp:RadioButton onclick="DateSelect();"   ID="RadioButtonNextY" Text="Next 1 Year" GroupName="Time" runat="server" AutoPostBack="false"/>
        </div>--%>
    </div>    

   
</div> <%--END OF date time/months field DIV--%>
                        
                     </div>

                   
                 </div>

                


             </ContentTemplate>

         </asp:UpdatePanel>


         <div style="clear: left; height: 5px;"></div>




     </div>
     
       <%--End Div Partner***************************************************--%>
                
                

                </ContentTemplate>
          </asp:UpdatePanel>
       
     
    
</div> <%--Param Border--%>
<%--Common--%>
    
  

 <div style="clear:left;">

 </div>

      <div style="clear: left;display:block;">
        
           
        <span style="padding-left:2px;">
          <asp:LinkButton ID="LinkButtonInitialize" runat="server"
            OnClientClick="ShowFormViewLCR();return false;">(Re-) Initialize LCR Rateplan</asp:LinkButton>
        </span>
        
    </div>
 <div style="clear:left;">
     <asp:Label ID="lblStatus" runat="server" Text=""></asp:Label>       
 </div>
    
    

    <div id="divFormView" style="float: left; visibility: hidden; height: 30px; margin-top: 4px;display:block;">
           <asp:FormView ID="frmInitLCR" runat="server" 
        Width="1200px" DefaultMode="Insert" 
        CellPadding="4" Font-Size="Small" 
        Visible="True" oniteminserting="frmSupplierRatePlanInsert_ItemInserting" 
        ForeColor="#333333">
       
               
        <%--<EditRowStyle BackColor="#E5E4E2" />--%>
       <EditRowStyle BackColor="#f2f2f2" />
       <FooterStyle BackColor="#5D7B9D" ForeColor="White" Font-Bold="True" />
        <HeaderStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
               
        <InsertItemTemplate>
            Rateplan:
            <asp:DropDownList ID="ddlistFrmRatePlan" runat="server"></asp:DropDownList>

            Initialization Date:
                <asp:TextBox id="txtDateFrm" Runat="server" /> 
                <ajaxToolkit:CalendarExtender ID="CalendarStartDateFrm" runat="server" 
                    TargetControlID="txtDateFrm"  PopupButtonID="txtDateFrm" Format="yyyy-MM-dd">
                </ajaxToolkit:CalendarExtender>  
            
                  Time:<asp:TextBox ID="TextBoxTime" runat="server" Text="00:00:00"></asp:TextBox>   

            <asp:LinkButton ID="InsertButton" runat="server" CausesValidation="True" 
                CommandName="Insert" Text="Initialize" ValidationGroup="allcontrols"/>

&nbsp;<asp:LinkButton ID="InsertCancelButton" runat="server" CausesValidation="False" 
                CommandName="Cancel" Text="Cancel" OnClientClick="HideFormViewLCR();return false;" CommandArgument="0"/>
        </InsertItemTemplate>
       
        <PagerStyle ForeColor="White" HorizontalAlign="Center" BackColor="#284775" />
        <RowStyle BackColor="white" ForeColor="#333333" />

        
       
    </asp:FormView>     
  

     
         
 </div>

     <div style="clear:left;text-align:left;">
        
     </div>
        
    
      
    
  

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
<div id="divGridView" style="clear:left;padding-left:0px;margin-top:-33px;">
    <asp:HiddenField ID="hidvaluerowcolorchange" runat="server" />
    <asp:GridView ID="GridViewLCR" runat="server" CellPadding="4" ForeColor="#333333" GridLines="Vertical" OnRowDataBound="GridViewLCR_RowDataBound" AllowPaging="True" PageSize="50" OnPageIndexChanging="GridViewLCR_PageIndexChanging" AutoGenerateColumns="False" BorderStyle="None">
        <AlternatingRowStyle BackColor="White" />
        <EditRowStyle BackColor="#999999" />
        <FooterStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
        <HeaderStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
        <PagerStyle BackColor="#284775" ForeColor="White" HorizontalAlign="Center" />
        <RowStyle BackColor="#F7F6F3" ForeColor="#333333" />
        <SelectedRowStyle BackColor="#E2DED6" Font-Bold="True" ForeColor="#333333" />
        <SortedAscendingCellStyle BackColor="#E9E7E2" />
        <SortedAscendingHeaderStyle BackColor="#506C8C" />
        <SortedDescendingCellStyle BackColor="#FFFDF8" />
        <SortedDescendingHeaderStyle BackColor="#6F8DAE" />
    <AlternatingRowStyle BackColor="#f2f2f2" ForeColor="#284775" />
         <Columns>
            <%--<asp:CommandField ShowDeleteButton="True" ShowEditButton="True" />--%>

             <asp:TemplateField HeaderText="Id">
                <ItemTemplate>
                    <asp:Label ID="lblId" runat="server" Text='<%# Eval("Id") %>'></asp:Label>
                </ItemTemplate>
             </asp:TemplateField>

             <asp:TemplateField HeaderText="Prefix">
                <ItemTemplate>
                    <asp:Label ID="lblPrefix" runat="server" Text='<%# Eval("Prefix") %>'></asp:Label>
                </ItemTemplate>
            </asp:TemplateField>
             
              <asp:TemplateField HeaderText="Description">
                <ItemTemplate>
                    <asp:Label ID="lblDescription" runat="server" Text='<%#Eval("PrefixDescription")%>'></asp:Label>
                </ItemTemplate>
            </asp:TemplateField>

            

             <asp:TemplateField HeaderText="Effective Since">
                <ItemTemplate>
                    <asp:Label ID="lblStartDate" runat="server" Text='<%# Convert.ToDateTime(Eval("StartDate")).ToString("yyyy-MM-dd HH:mm:ss")%>'></asp:Label>
                </ItemTemplate>
            </asp:TemplateField>
            
             <asp:TemplateField HeaderText="Last Updated">
                <ItemTemplate>
                    <asp:Label ID="lblStartDateUpdated" runat="server" Text='<%# Convert.ToDateTime(Eval("LastUpdated")).ToString("yyyy-MM-dd HH:mm:ss")%>'></asp:Label>
                </ItemTemplate>
            </asp:TemplateField>

               <asp:TemplateField HeaderText="Lowest Rate">
                <ItemTemplate>
                    <asp:Label ID="lblRate" runat="server" Text='<%#
                        Eval("LowestRate")!=null
                        ?Math.Round(Convert.ToDouble(Eval("LowestRate")),2)
                        :0%>'></asp:Label>
                </ItemTemplate>
            </asp:TemplateField>

             <asp:TemplateField HeaderText="Least Cost Routing (LCR)" HeaderStyle-HorizontalAlign="Center">
                <ItemTemplate>
                    <table>
                        <tr>
                            <td>
                                <div id="DivListView">
                        <p>
                        <asp:ListView ID="ListViewLcr" runat="server" OnItemDataBound="ListViewLcr_ItemDataBound">
                            <AlternatingItemTemplate>
                                <td runat="server" style="background-color: #FFFFFF;color: #284775;vertical-align:top;text-align:center;">
                                    <asp:Label ID="nameLabel" runat="server" Text='<%#"Rate: "+Eval("Cost")%>' Font-Bold="true" BackColor="#DDDDDD" width="150" />
                                    <div style="margin-top:5px;">
                                       <asp:GridView ID="GridViewRouteCost" runat="server" AutoGenerateColumns="False" ShowHeader="False" BackColor="#DEBA84" BorderColor="#DEBA84" BorderStyle="None" BorderWidth="1px" CellPadding="3" CellSpacing="2">
                                            <Columns>
                                                <asp:TemplateField>
                                                    <ItemTemplate>
                                                        <asp:Label ID="lblRouteCost" runat="server" Text='<%#Eval("RouteCarrierCombined") %>'></asp:Label>
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                            </Columns>
                                            <FooterStyle BackColor="#F7DFB5" ForeColor="#8C4510" />
                                            <HeaderStyle BackColor="#A55129" Font-Bold="True" ForeColor="White" />
                                            <PagerStyle ForeColor="#8C4510"/>
                                            <RowStyle BackColor="#FFF7E7" ForeColor="#8C4510" />
                                            <SelectedRowStyle BackColor="#738A9C" Font-Bold="True" ForeColor="White" />
                                            <SortedAscendingCellStyle BackColor="#FFF1D4" />
                                            <SortedAscendingHeaderStyle BackColor="#B95C30" />
                                            <SortedDescendingCellStyle BackColor="#F1E5CE" />
                                            <SortedDescendingHeaderStyle BackColor="#93451F" />
                                        </asp:GridView>
                                    </div>
                            </td>
                            </AlternatingItemTemplate>
       
                            <ItemTemplate>
                                <td runat="server" style="background-color: #FFFFFF;color: #284775;vertical-align:top;text-align:center;">
                                    <asp:Label ID="nameLabel" runat="server" Text='<%#"Rate: "+Eval("Cost")%>'  Font-Bold="true" BackColor="#99ccff" width="150"  />
                                    <div style="margin-top:5px;">
                                        <asp:GridView ID="GridViewRouteCost" runat="server" AutoGenerateColumns="False" ShowHeader="False" BackColor="#DEBA84" BorderColor="#DEBA84" BorderStyle="None" BorderWidth="1px" CellPadding="3" CellSpacing="2">
                                            <Columns>
                                                <asp:TemplateField>
                                                    <ItemTemplate>
                                                        <asp:Label ID="lblRouteCost" runat="server" Text='<%#Eval("RouteCarrierCombined") %>'></asp:Label>
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                            </Columns>
                                            <FooterStyle BackColor="#F7DFB5" ForeColor="#8C4510" />
                                            <HeaderStyle BackColor="#A55129" Font-Bold="True" ForeColor="White" />
                                            <PagerStyle ForeColor="#8C4510" HorizontalAlign="Center" />
                                            <RowStyle BackColor="#FFF7E7" ForeColor="#8C4510" />
                                            <SelectedRowStyle BackColor="#738A9C" Font-Bold="True" ForeColor="White" />
                                            <SortedAscendingCellStyle BackColor="#FFF1D4" />
                                            <SortedAscendingHeaderStyle BackColor="#B95C30" />
                                            <SortedDescendingCellStyle BackColor="#F1E5CE" />
                                            <SortedDescendingHeaderStyle BackColor="#93451F" />
                                        </asp:GridView>
                                    </div>
                                </td>
                            </ItemTemplate>
       
                            </asp:ListView>
                            </p>
                        </div>
                            </td>
                        </tr>
                    </table>
                </ItemTemplate>

<HeaderStyle HorizontalAlign="Center"></HeaderStyle>
            </asp:TemplateField>

           
            </Columns>
    </asp:GridView>
</div>
   
</div>
    
    
    
    
    
   
    
    

    

</asp:Content>