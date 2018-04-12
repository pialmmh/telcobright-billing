<%@ Page Title="" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="True" CodeBehind="ratetask.aspx.cs" Inherits="ConfigRateTask" EnableViewState="true" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>



<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" Runat="Server">

  <%--  <asp:UpdatePanel ID="UpdatePanel2" runat="server" >
        <Triggers>
            <asp:PostBackTrigger ControlID="UploadButton" />
        </Triggers>
        <ContentTemplate>    --%>
    <asp:SqlDataSource ID="SqlDataCountry" runat="server" 
        ConnectionString="<%$ ConnectionStrings:Partner %>" 
        ProviderName="<%$ ConnectionStrings:Partner.ProviderName %>" 
        SelectCommand="select (select null) as Code,(select ' [None]') as Name
union all					
select Code,Name from countrycode "
        ></asp:SqlDataSource>

        <asp:SqlDataSource ID="SqlDataYesNo" runat="server" 
        ConnectionString="<%$ ConnectionStrings:Partner %>" 
        ProviderName="<%$ ConnectionStrings:Partner.ProviderName %>" 
        SelectCommand="select (select 0) as id,(select 'No') as Type
union all					
select (select 1) as id,(select 'Yes') as Type "
        ></asp:SqlDataSource>

        <asp:Label ID="lblRateGlobal" runat="server" Text="" Visible="false"></asp:Label>

        <asp:Label ID="lblEditPrefix" runat="server" Text="" Visible="false"></asp:Label>

        <%--DataSourceID="EntityDataSupplierRates"--%>

    <asp:SqlDataSource ID="SqlDataServiceType" runat="server" 
    ConnectionString="<%$ ConnectionStrings:Partner %>" 
    ProviderName="<%$ ConnectionStrings:Partner.ProviderName %>" 
    SelectCommand="select * from enumservicecategory"></asp:SqlDataSource>
    <asp:SqlDataSource ID="SqlDataSubServiceType" runat="server" 
        ConnectionString="<%$ ConnectionStrings:Partner %>" 
        ProviderName="<%$ ConnectionStrings:Partner.ProviderName %>" 
        SelectCommand="select * from enumservicesubcategory"></asp:SqlDataSource>

    <asp:EntityDataSource ID="EntityDataRateTask" runat="server" 
        ConnectionString="name=PartnerEntities" DefaultContainerName="PartnerEntities" 
        EnableDelete="True" EnableFlattening="False" EnableUpdate="True" 
        EntitySetName="ratetasks" onquerycreated="EntityDataRateTask_QueryCreated">
    </asp:EntityDataSource>



     <asp:ToolkitScriptManager ID="ToolkitScriptManager1" runat="server" EnablePageMethods="true" />
    
    Rate Plan: <span style="padding-left:10px;padding-right:5px;"> <asp:Label ID="lblRatePlan" runat="server" Font-Bold="true" Text=""></asp:Label> 
        </span>
    <asp:LinkButton ID="LinkButtonRate" runat="server">Rates</asp:LinkButton>
    <span style="padding-left:20px;">
        
        
        <b> Reference Time Zone: </b>
    <asp:Label ID="lblTimeZone" runat="server" Text=""></asp:Label>
    </span>
    
    <span style="padding-left:20px;">
    <b> Task Reference: </b>
        <asp:DropDownList ID="DropDownListTaskRef" runat="server" AutoPostBack="true" 
        onselectedindexchanged="DropDownListTaskRef_SelectedIndexChanged">
        </asp:DropDownList>
    <asp:LinkButton ID="EditTaskRefName" runat="server" OnClientClick="var value = prompt('Enter a Task Reference Name:'); SetHidValueRefName(value);" OnClick="EditTaskRefName_Click" 
            style="margin-left: 0px" Text="Rename" Visible="True"/>
            </span>
    <span style="padding-left:20px;">
    <asp:LinkButton ID="NewTaskRefName" runat="server" OnClientClick="var value = prompt('Enter a Task Reference Name:'); SetHidValueRefName(value);" OnClick="NewTaskRefName_Click" 
            style="margin-left: 0px" Text="New Task Reference" Visible="True"/>
    <input type="hidden" id="hidValueRefName" runat="server" />
    <script type="text/javascript">

        function SetHidValueRefName(value) {
            document.getElementById("<%= this.hidValueRefName.ClientID%>").value = value;
        }
            
    </script>

        <asp:HiddenField ID="hidValueCommit" runat="server" Value="1" />
            <script type="text/javascript">
                function setcommitflag() {
                    document.getElementById('<%= this.hidValueCommit.ClientID%>').value = "1";
                }
                function clearcommitflag() {
                    document.getElementById('<%= this.hidValueCommit.ClientID%>').value = "0";
                }
            </script>

    </span>

    
    <br />
    

    <div style="min-width:1500px;min-height:100px;background-color:#f2f2f2;padding-left:10px;margin-bottom:5px;padding-top:5px;margin-top:3px;">
    <div style="color:Black;">Create or Import New Rate Task
       <span style="padding-left:5px;"> <asp:CheckBox ID="CheckBoxAutoConvertTZ" runat="server" Text="Auto Adjust effective Date/Time for New/Import Task" Checked="true"></asp:CheckBox></span>
        <span style="padding-left:5px;"> <asp:CheckBox ID="CheckBoxAutoDetectCountry" runat="server" Text="Auto Detect Country Code" Checked="true"></asp:CheckBox></span>
    </div> 
    <div style="min-width:1500px;min-height:60px;border-style:ridge;float:left;padding-top:2.5px;background-color:#F7F6F3;color:Black;">
            
            <%--<asp:Label ID="Label4" runat="server" Text="Default Effective Date (Partner's TZ)"></asp:Label>--%>
        <div style="float:left;">
            <asp:CheckBox ID="CheckBoxDefaultDate" checked="true" runat="server" Text="Default Effective Date (Own TZ)" />
                        <span style="padding-left:40px;"><asp:TextBox ID="TextBoxDefaultDate" runat="server"> </asp:TextBox></span>
                      
                        <asp:CalendarExtender ID="CalendarStartDate" runat="server" 
                                        TargetControlID="TextBoxDefaultDate"  PopupButtonID="TextBoxStartDatePicker" Format="yyyy-MM-dd">
                      </asp:CalendarExtender>
                      Time: <asp:TextBox ID="TextBoxDefaultTime" runat="server" Text="00:00:00">
                      </asp:TextBox>
            <asp:Label ID="Label4" runat="server" Text="(Effective date for prefixes if not specified)"></asp:Label>
        </div>
        <div style="clear:left;"></div>    
        
        <div style="clear:left;"></div>        
        
         
        


        <div style="float:left; height:22px;width:750px;margin-left:2px;padding-top:10px;float:left;background-color:#F7F6F3;color:Black;">

                Create
                <span style="padding-left:3px;">
                <asp:LinkButton ID="LinkButton1" runat="server" Text="Add New Rate" 
                onclick="LinkButton1_Click">New Rate Task</asp:LinkButton>
                    </span>
            Or, <span style="padding-left:3px;">
                <asp:LinkButton ID="LinkButtonCodeDelete" runat="server" Text="Code Delete Task with Helper" 
                onclick="LinkButtonCodeDelete_Click"></asp:LinkButton>
                    </span>
                Or, <span style="padding-left:10px;">
                  <asp:LinkButton runat="server" id="UploadButton" text="Import" 
                OnClientClick="return confirm('Exising tasks will be deleted under current task reference. Are you sure to import?');"
                onclick="UploadButton_Click" />
                </span> Rate Sheet
            
        
            
            <asp:FileUpload id="FileUploadControl" runat="server" />
    
    
                <%--Rate Plan Format: --%>
                <asp:DropDownList ID="DropDownListFormat" runat="server" Visible="false">
            <asp:ListItem Value="-1"> [Auto Detect Any Format]</asp:ListItem>
            <asp:ListItem Value="1"> Generic (Excel-Native)</asp:ListItem>
            <asp:ListItem Value="2"> Generic (Text/CSV)</asp:ListItem>
            <asp:ListItem Value="101"> Tata</asp:ListItem>
            <asp:ListItem Value="102"> Bharti</asp:ListItem>
            <asp:ListItem Value="103"> IDT</asp:ListItem>
            <asp:ListItem Value="104"> Mainberg</asp:ListItem>
            <asp:ListItem Value="105"> Vodafone</asp:ListItem>
            <asp:ListItem Value="106"> Idea</asp:ListItem>
            <asp:ListItem Value="107"> CKRI</asp:ListItem>
        </asp:DropDownList>

              

                

             </div>


        </div>
     
            
        

    <div style="height:4px;width:800px;clear:both;"></div>

        
    
        
        <asp:HiddenField ID="HiddenFieldSelect" runat="server" Value="0" />
            <script type="text/javascript">
                function SetHidValueFilter() {
                document.getElementById("<%= this.HiddenFieldSelect.ClientID%>").value = "1";
            }
            </script>

        
       
                   

</div>

    


<asp:ValidationSummary ID="ValidatorSummary" runat="server"
        ValidationGroup="allcontrols" ForeColor="Red" />

    <div style="padding-left:12px;background-color:#F7F6F3;">

        <asp:FormView ID="frmSupplierRatePlanInsert" runat="server" DataKeyNames="id"                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                             
        Width="1000px" DefaultMode="Insert" 
        Height="76px" CellPadding="4" Font-Size="Small" ForeColor="#333333" 
        Visible="False" oniteminserted="frmSupplierRatePlanInsert_ItemInserted" 
        onitemcreated="frmSupplierRatePlanInsert_ItemCreated" 
        onload="frmSupplierRatePlanInsert_Load" 
        oniteminserting="frmSupplierRatePlanInsert_ItemInserting" 
        onmodechanging="frmSupplierRatePlanInsert_ModeChanging" 
        ondatabound="frmSupplierRatePlanInsert_DataBound" 
        onmodechanged="frmSupplierRatePlanInsert_ModeChanged" BorderStyle="Solid" 
        BorderWidth="1">

        <FooterStyle BackColor="#990000" Font-Bold="True" ForeColor="White" />
        <HeaderStyle BackColor="#990000" Font-Bold="True" ForeColor="White" />

        <InsertItemTemplate>
          
          <b>Create a New Rate Task</b> &nbsp [Enter Rateamount=-1 for Code Delete Task]
          <table border="0" width="2200px">
            <tr style="text-align:left;">

            <td style="width:350px;" > <%--width="200px"--%>
                
               <div style="text-align:right; width:350px;margin-top:0px;">
                    <span style="padding-left:5px;"></span>
                    <b> Prefix:</b> <asp:TextBox ID="txtPrefix" Enabled="True" runat="server" Text=""></asp:TextBox>
                    <br />    
               Country:
               <asp:DropDownList ID="DropDownListCountry" runat="server" AutoPostBack="false" 
                        DataSourceID="SqlDataCountry" DataTextField="Name" DataValueField="Code" SelectedValue='<%# Bind("CountryCode") %>'
                        Enabled="True" >
                    </asp:DropDownList>
               <br />
               
                    Description:
                        <asp:TextBox ID="txtDescription" Enabled="True" runat="server" Text=""></asp:TextBox>
                        <br />
                    <b> Rate Amount:</b>
                        <asp:TextBox ID="txtRateAmount" Enabled="True" runat="server" Text=""></asp:TextBox>
                    <br />
                    <b> Pulse:</b>
               <asp:TextBox ID="txtResolution" Enabled="True" runat="server" Text="1"></asp:TextBox>
               <br />
               <b> Minimum Duration: (Sec)</b>
               <asp:TextBox ID="txtMinDurationSec" Enabled="True" runat="server" Text="0"></asp:TextBox>
               <br />
               
                    
                </div>
            </td>

            <td style="width:200px;" > <%--width="200px"--%>
                
                <div style="text-align:right;margin-top:50px;">
                
                
                <b> Effective From:</b> [Own TZ]<br />
                Date: <asp:TextBox ID="TextBoxStartDatePickerFrm" runat="server">
                  </asp:TextBox>
                    <asp:CalendarExtender ID="CalendarStartDateFrm" runat="server" 
                                    TargetControlID="TextBoxStartDatePickerFrm"  PopupButtonID="TextBoxStartDatePickerFrm" Format="yyyy-MM-dd">
                  </asp:CalendarExtender>     
                  <br />
                  Time: <asp:TextBox ID="TextBoxStartDateTimePickerFrm" runat="server" Text="00:00:00">
                  </asp:TextBox>
                    <br />
                  
                  Valid Before: [Own TZ] <br />
                  Date: <asp:TextBox ID="TextBoxEndDatePickerFrm" runat="server">
                  </asp:TextBox>
                    <asp:CalendarExtender ID="CalendarEndDateFrm" runat="server" 
                                    TargetControlID="TextBoxEndDatePickerFrm"  PopupButtonID="TextBoxEndDatePickerFrm" Format="yyyy-MM-dd">
                  </asp:CalendarExtender>
                  <br />
                  Time: <asp:TextBox ID="TextBoxEndDateTimePickerFrm" runat="server" Text="00:00:00">
                  </asp:TextBox>
                  


                    <%--<b> Inactive:</b>--%>
                    <asp:DropDownList ID="DropDownListInactive" runat="server" AutoPostBack="false" 
                        DataSourceID="SqlDataYesNo" DataTextField="Type" DataValueField="id" 
                        Enabled="True" Visible="false" >
                    </asp:DropDownList>
                    <br />
               
                    <span style="visibility:hidden;">
                    <%--<b> Route Disabled:</b>--%>
                    <%--<asp:DropDownList ID="DropDownListRouteDisabled" runat="server" AutoPostBack="True" 
                        DataSourceID="SqlDataYesNo" DataTextField="Type" DataValueField="id" Visible="false" 
                        Enabled="True" >
                    </asp:DropDownList>
                    </span>
--%>
                    <br />

                    <asp:Label ID="lblOtherAmount10" runat="server" Text="ICX % Rev Share:"></asp:Label>
                    
                    <asp:TextBox ID="txtOtherAmount10" visible="True" runat="server" Text=""></asp:TextBox>
                    <br />



            </div>

            </td>


            <td style="width:280px;padding-left:15px;">
                    <div style="text-align:right">
                        <asp:Label ID="lblOtherAmount1" runat="server" Text="Other Amount 1"  Font-Bold="true"></asp:Label>
                        <asp:TextBox ID="txtOtherAmount1" visible="True" runat="server" Text=""></asp:TextBox>
                        <br />
                    
                        <asp:Label ID="lblOtherAmount2" runat="server" Text="Other Amount 2"  Font-Bold="true"></asp:Label>
                        <asp:TextBox ID="txtOtherAmount2" visible="True" runat="server" Text=""></asp:TextBox>
                        <br />

                        <asp:Label ID="lblOtherAmount3" runat="server" Text="Other Amount 3"  Font-Bold="true"></asp:Label>
                        <asp:TextBox ID="txtOtherAmount3" visible="True" runat="server" Text=""></asp:TextBox>
                        <br />

                        <asp:Label ID="lblOtherAmount4" runat="server" Text="Other Amount 4" Font-Bold="true"></asp:Label>
                            <asp:TextBox ID="txtOtherAmount4" visible="True" runat="server" Text=""></asp:TextBox>
                        <br />

                    </div>
                
            </td>

            <td style="text-align:left;padding-left:15px;">

                    <asp:Label ID="lblOtherAmount5" runat="server" Text="Other Amount 5" Font-Bold="true"></asp:Label>
                    <asp:TextBox ID="txtOtherAmount5" visible="True" runat="server" Text=""></asp:TextBox>
                    <br />

                    <asp:Label ID="lblOtherAmount6" runat="server" Text="Other Amount 6"  Font-Bold="true"></asp:Label>
                    <asp:TextBox ID="txtOtherAmount6" visible="True" runat="server" Text=""></asp:TextBox>
                    <br />
                    
                    <asp:Label ID="lblOtherAmount7" runat="server" Text="Other Amount 7"  Font-Bold="true"></asp:Label>
                    <asp:TextBox ID="txtOtherAmount7" visible="True" runat="server" Text=""></asp:TextBox>
                    <br />

                    <asp:Label ID="lblOtherAmount8" runat="server" Text="Other Amount 8"  Font-Bold="true"></asp:Label>
                    <asp:TextBox ID="txtOtherAmount8" visible="True" runat="server" Text=""></asp:TextBox>
                    <asp:TextBox ID="txtOtherAmount9" visible="False" runat="server" Text=""></asp:TextBox>
                <br />

            
            </td>
            
          
          
            </tr>
          
          <tr>
            <asp:TextBox ID="txtWeekDayStart" visible="false" runat="server" Text="1"></asp:TextBox>
                    
                
                    <asp:TextBox ID="txtWeekDayEnd" visible="false" runat="server" Text="7"></asp:TextBox>
                
                <asp:TextBox ID="txtStartTime" visible="false" runat="server" Text="00:00:00"></asp:TextBox>
                    
                    <asp:TextBox ID="txtEndTime" visible="false" runat="server" Text="23:59:59"></asp:TextBox>
          </tr>
            
           </table>
           
           <div style="padding-left:100px;">
            <div style="float:left;text-align:right;">
                <b>Category</b> 
                      <asp:DropDownList ID="DropDownListServiceType" runat="server" AutoPostBack="false" 
                            DataSourceID="SqlDataServiceType" DataTextField="Type" DataValueField="id" 
                            Enabled="true" >
                        </asp:DropDownList>
                        <br />
                      <b>Sub Category</b>   
                        <asp:DropDownList ID="DropDownListSubServiceType" runat="server" AutoPostBack="false" 
                            DataSourceID="SqlDataSubServiceType" DataTextField="Type" DataValueField="id" 
                            Enabled="true" >
                        </asp:DropDownList>
                      <br />
                </div>
                <div style="float:left;width:500px;text-align:right;">
                    
                  
                  <b> Fixed Initial Period (Sec):</b> 
                  <asp:TextBox ID="txtSurchargeTime" visible="true" runat="server" Text="0"></asp:TextBox>
                    <br />
                    <b> Charge for Fixed Initial Period:</b> 
                    <asp:TextBox ID="txtSurchargeAmount" visible="true" runat="server" Text="0.0"></asp:TextBox>
                    <br />
                    
                    <b> Round Digits after Decimal for Rate Amount:</b> 
                    <asp:TextBox ID="TextBoxRoundDigits" visible="true" runat="server" Text="0"></asp:TextBox>
                    <br />
                    <asp:Label ID="label10" runat="server" Text="[0=No Rounding]"></asp:Label>
                </div>
            </div>
            <div style="clear:both;"></div>
            
            <asp:LinkButton ID="InsertCommitButton" runat="server" CausesValidation="True" Visible="false"
                CommandName="Insert" Text="Insert & Commit" ValidationGroup="allcontrols" OnClientClick="setcommitflag();"
                />
            &nbsp;
            <asp:LinkButton ID="InsertButton" runat="server" CausesValidation="True" 
                CommandName="Insert" Text="Insert" ValidationGroup="allcontrols" OnClientClick="clearcommitflag();"/>

&nbsp;<asp:LinkButton ID="InsertCancelButton" runat="server" CausesValidation="False" 
                CommandName="Cancel" Text="Cancel" OnClick="FormViewCancel_Click"/> <%--OnClientClick="clearinsertflag();"--%>
        </InsertItemTemplate>
       
        <PagerStyle BackColor="#FFCC66" ForeColor="#333333" HorizontalAlign="Center" />
        <RowStyle BackColor="white" ForeColor="#333333" />
        
        
       
    </asp:FormView>
        
        </div>
    <div style="padding-left:12px;background-color:#F7F6F3;">
    
        <asp:UpdatePanel ID="UpdatePanel1" runat="server">
            <ContentTemplate>
                <asp:FormView ID="FormViewCodeDelete" runat="server" DataKeyNames="id" Width="1000px" DefaultMode="Insert" Height="76px" CellPadding="4" Font-Size="Small" ForeColor="#333333" 
                Visible="False" oniteminserting="frmCodeDelete_ItemInserting" BorderStyle="Solid" BorderWidth="1" onmodechanging="frmCodeDelete_ModeChanging" BackColor="#F7F6F3" >

        <FooterStyle BackColor="#990000" Font-Bold="True" ForeColor="White" />
        <HeaderStyle BackColor="#990000" Font-Bold="True" ForeColor="White" />

        <InsertItemTemplate>
          
            <b> Create Code Delete Item with Helper:</b>
          <table border="0" width="2200px">
            <tr style="text-align:left;">

            <td style="width:350px;background-color:white;" > <%--width="200px"--%>
                
              <div style="float:left;">
        <%--<asp:Label ID="Label5" runat="server" Text="Obsoletes all Prev Prefix (Partner's TZ)"></asp:Label>--%>
                    Code Ending Date:
                        <asp:TextBox ID="TextBoxDefaultDeleteDate" runat="server">
                      </asp:TextBox>
                        <asp:CalendarExtender ID="CalendarExtender1" runat="server" 
                                        TargetControlID="TextBoxDefaultDeleteDate"  PopupButtonID="TextBoxStartDatePicker" Format="yyyy-MM-dd">
                      </asp:CalendarExtender>
                      Time: <asp:TextBox ID="TextBoxDefaultDeleteTime" runat="server" Text="00:00:00">
                      </asp:TextBox>
            <div style="clear:both;"></div>
                  <asp:CheckBox ID="CheckBoxDeleteExcept" runat="server" Text="Supersede Prefixes except in this task Started before Code End Date (Own TZ)" AutoPostBack="true"  OnCheckedChanged="Supersede_CheckedExceptChange" ForeColor="Black" Font-Bold="false" Checked="true"  />
                  <span style="padding-left:30px;">
                    <asp:Label ID="Label7" runat="server" Text="[Makes previous prefixes obsolete, excluding those in this task just before this date]"></asp:Label>
                  </span>
                  <div style="clear:both;"></div>
                  <asp:CheckBox ID="CheckBoxDefaultDeleteDate" runat="server" Text="Supersede all Prefix Started before Code End Date (Own TZ)" AutoPostBack="true"  OnCheckedChanged="Supersede_CheckedChange" ForeColor="Black" Font-Bold="false"  />
                  <span style="padding-left:131px;">
                    <asp:Label ID="Label5" runat="server" Text="[Makes previous prefixes obsolete just before this date]"></asp:Label>
                  </span>
                <div style="clear:both;"></div>
                   <asp:CheckBox ID="CheckBoxCountryWise" runat="server" Text="Supersede Prefixes Country Wise Started before Code End Date (Own TZ)" OnCheckedChanged="CountryWise_CheckedChange" AutoPostBack="true" ForeColor="Black" Font-Bold="false" />
                  <span style="padding-left:55px;">
                  <asp:Label ID="Label6" runat="server" Text="   [For Partial/Incremental Ratesheets, supersedes previous codes only for countries under current task]"></asp:Label>
                      </span>
                  <div style="clear:both;"></div>
                   <asp:CheckBox ID="CheckBoxCountryWiseMinDate" runat="server" Text="Supersede Prefixes Country Wise at the minimum effective date specified per country under current task (Own TZ)" OnCheckedChanged="CountryWiseMin_CheckedChange" AutoPostBack="true" ForeColor="Black" Font-Bold="false" />
        </div>

            
            </td>
            
          
          
            </tr>
         
            
           </table>
           
           
            <div style="clear:both;"></div>
            
           
            <asp:LinkButton ID="InsertButton" runat="server" CausesValidation="True" 
                CommandName="Insert" Text="Insert" ValidationGroup="allcontrols" OnClientClick="clearcommitflag();"/>

&nbsp;<asp:LinkButton ID="InsertCancelButton" runat="server" CausesValidation="False" 
                CommandName="Cancel" Text="Cancel" OnClick="FormViewCodeDeleteCancel_Click"/> <%--OnClientClick="clearinsertflag();"--%>

            <asp:Label ID="lblCodeDeleteStatus" runat="server" Text=""></asp:Label>

        </InsertItemTemplate>
       
        <PagerStyle BackColor="#FFCC66" ForeColor="#333333" HorizontalAlign="Center" />
        <RowStyle BackColor="white" ForeColor="#333333" />
        
        
       
    </asp:FormView>
            </ContentTemplate>
            
        </asp:UpdatePanel>
    
        </div>

     <div style="background-color:#F7F6F3;margin-top:4px;padding-left:20px;">
        
        <div style="width:900px;height:5px;"></div>
        <div style="height:24px;border-style:ridge;width:1500px;margin-left:-10px;padding-top:2.5px;background-color:#f2f2f2;color:Black;">

            <asp:Label ID="Label1" runat="server" Text="Find Prefix [*]"></asp:Label>
            <asp:TextBox ID="TextBoxFindByPrefix" runat="server" Width="100px"></asp:TextBox>
            <asp:Label ID="Label2" runat="server" Text=""></asp:Label> and
            <asp:Label ID="Label3" runat="server" Text="Search Description"></asp:Label>
            <asp:TextBox ID="TextBoxFindByDescription" runat="server"  Width="115px"></asp:TextBox>
    
            and, Type:
        <asp:DropDownList ID="DropDownListMoreFilters" runat="server">
        <asp:ListItem Value="-1">All</asp:ListItem>
        <asp:ListItem Value="0">Validation Errors</asp:ListItem>
        <asp:ListItem Value="1">Code End</asp:ListItem>
        <asp:ListItem Value="2">New Codes</asp:ListItem>
        <asp:ListItem Value="3">Increase</asp:ListItem>
        <asp:ListItem Value="4">Decrease</asp:ListItem>
        <asp:ListItem Value="5">Unchanged</asp:ListItem>
        <%--<asp:ListItem Value="6">Errors</asp:ListItem>--%>
        <asp:ListItem Value="7">Complete</asp:ListItem>
        <asp:ListItem Value="8">Incomplete</asp:ListItem>
        <asp:ListItem Value="9">Overlap</asp:ListItem>
        <asp:ListItem Value="10">Overlap Adjusted</asp:ListItem>
        <asp:ListItem Value="11">Rate Param Conflict</asp:ListItem>
        <asp:ListItem Value="12">Rate Position Not Found</asp:ListItem> 
        <asp:ListItem Value="13">Existing</asp:ListItem> 
    </asp:DropDownList>
    
            <asp:Button ID="ButtonFindPrefix" runat="server" Text="Find" width="100px"
                onclick="ButtonFindPrefix_Click"></asp:Button>
            <asp:Button ID="ButtonFindPrefixSelect" runat="server" Text="Find & Select" 
                onclick="ButtonFindPrefixSelect_Click" OnClientClick="SetHidValueFilter(); return true;"> </asp:Button>
            <asp:Button ID="ButtonExport" runat="server" Text="Export" 
                onclick="ButtonExport_Click"> </asp:Button>
        </div>
            
        <div style="margin-left:-15px;">
            <asp:ListView ID="ListView1" runat="server" DataSourceID="SqlDataTaskStatus" 
            GroupItemCount="9" 
             onitemcommand="ListView1_ItemCommand">
            <AlternatingItemTemplate>
                <td id="Td1" runat="server" style="background-color:#F7F6F3;">
                    <asp:LinkButton ID="LinkButton2" runat="server" ForeColor="Brown" CommandArgument='<%#Eval("status")%>' ><%# Eval("Description") %>:<%# Eval("cnt") %></asp:LinkButton>
                    
                    <%--<asp:Label ID="status
                        Label" runat="server" Text='<%# Eval("status") %>' />
                    <br />Description:
                    <asp:Label ID="DescriptionLabel" runat="server" 
                        Text='<%# Eval("Description") %>' />
                    <br />cnt:
                    <asp:Label ID="cntLabel" runat="server" Text='<%# Eval("cnt") %>' />
                    --%><br />
                </td>
            </AlternatingItemTemplate>
            
            <EmptyDataTemplate>
                <table id="Table1" runat="server" style="">
                    <tr>
                        <td>
                            No data was returned.</td>
                    </tr>
                </table>
            </EmptyDataTemplate>
            <EmptyItemTemplate>
<td id="Td2" runat="server" />
            </EmptyItemTemplate>
            <GroupTemplate>
                <tr ID="itemPlaceholderContainer" runat="server">
                    <td ID="itemPlaceholder" runat="server">
                    </td>
                </tr>
            </GroupTemplate>
            
            <ItemTemplate>
                <td id="Td1" runat="server" style="background-color:#F7F6F3;">
                    <asp:LinkButton ID="LinkButton2" runat="server" ForeColor="Brown" CommandArgument='<%#Eval("status")%>' ><%# Eval("Description") %>:<%# Eval("cnt") %></asp:LinkButton>
                    
                    <%--<asp:Label ID="status
                        Label" runat="server" Text='<%# Eval("status") %>' />
                    <br />Description:
                    <asp:Label ID="DescriptionLabel" runat="server" 
                        Text='<%# Eval("Description") %>' />
                    <br />cnt:
                    <asp:Label ID="cntLabel" runat="server" Text='<%# Eval("cnt") %>' />
                    --%><br />
                </td>
            </ItemTemplate>
            <LayoutTemplate>
                <table id="Table2" runat="server">
                    <tr id="Tr1" runat="server">
                        <td id="Td3" runat="server">
                            <table ID="groupPlaceholderContainer" runat="server" border="0" style="">
                                <tr ID="groupPlaceholder" runat="server">
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr id="Tr2" runat="server">
                        <td id="Td4" runat="server" style="">
                        </td>
                    </tr>
                </table>
            </LayoutTemplate>
            <SelectedItemTemplate>
                <td id="Td2" runat="server" style="background-color:#F7F6F3;">
                    <asp:LinkButton ID="LinkButton2" runat="server" ForeColor="Brown" CommandArgument='<%#Eval("status")%>' ><%# Eval("Description") %>:<%# Eval("cnt") %></asp:LinkButton>
                    
                    <%--<asp:Label ID="status
                        Label" runat="server" Text='<%# Eval("status") %>' />
                    <br />Description:
                    <asp:Label ID="DescriptionLabel" runat="server" 
                        Text='<%# Eval("Description") %>' />
                    <br />cnt:
                    <asp:Label ID="cntLabel" runat="server" Text='<%# Eval("cnt") %>' />
                    --%><br />
                </td>
            </SelectedItemTemplate>
        
        </asp:ListView>
        </div>
        
        </div>

        <asp:SqlDataSource ID="SqlDataTaskStatus" runat="server" 
            ConnectionString="<%$ ConnectionStrings:Partner %>" 
            ProviderName="<%$ ConnectionStrings:Partner.ProviderName %>" SelectCommand="select (select -1) as status ,(select 'Total') as Description,count(*) as cnt from ratetask where idrateplan=@idRatePlan union all
select (select 7) as status  ,(select 'Complete') as Description,count(*) as cnt from ratetask where idrateplan=@idRatePlan and changecommitted=1 union all
select (select 0) as status  ,(select 'Incomplete') as Description,count(*) as cnt from ratetask where idrateplan=@idRatePlan and changecommitted!=1 union all
select (select 100) as status,(select 'Validation Error') as Description,count(*) as cnt from ratetask where idrateplan=@idRatePlan and field2&gt;0 union all
select (select 9) as status  ,(select 'Overlap') as Description,count(*) as cnt from ratetask where idrateplan=@idRatePlan and status=9 and changecommitted!=1  union all
select (select 11) as status  ,(select 'Rate Param Conflict') as Description,count(*) as cnt from ratetask where idrateplan=@idRatePlan and status=11 and changecommitted!=1 union all
select (select 12) as status  ,(select 'Rate Position Not Found') as Description,count(*) as cnt from ratetask where idrateplan=@idRatePlan and status=12 and changecommitted!=1 union all
select (select 13) as status  ,(select 'Existing') as Description,count(*) as cnt from ratetask where idrateplan=@idRatePlan and status=13 and changecommitted !=1 union all
select (select 1) as status  ,(select 'Code End') as Description,count(*) as cnt from ratetask where idrateplan=@idRatePlan and rateamount='-1' union all
select (select 2) as status  ,(select 'New') as Description,count(*) as cnt from ratetask where idrateplan=@idRatePlan and status=2 and changecommitted=1 union all
select (select 3) as status  ,(select 'Increase') as Description,count(*) as cnt from ratetask where idrateplan=@idRatePlan and status=3 and changecommitted=1 union all
select (select 4) as status  ,(select 'Decrease') as Description,count(*) as cnt from ratetask where idrateplan=@idRatePlan and status=4 and changecommitted=1 union all
select (select 5) as status  ,(select 'Unchanged') as Description,count(*) as cnt from ratetask where idrateplan=@idRatePlan and status=5  and changecommitted=1 union all
select (select 10) as status  ,(select 'Overlap Adjusted') as Description,count(*) as cnt from ratetask where idrateplan=@idRatePlan and status=10 and changecommitted=1
">

<SelectParameters>
    <asp:Parameter Name="idRatePlan" Type="Int32" DefaultValue="0" />
  </SelectParameters>

</asp:SqlDataSource>


 <div style="height:22px;background-color:#f2f2f2;">
    
     <asp:CheckBox ID="CheckBoxContinueOnError" runat="server" Text="Continue with next on Error" Checked="true" />
     <span style="padding-left:20px;">
        <asp:CheckBox ID="CheckBoxContinueOnOverlap" runat="server" Text="Continue with next on Overlap" Checked="true" />
     </span>
     <span style="padding-left:20px;">
        <asp:CheckBox ID="CheckBoxAutoAdjust" runat="server" Text="Auto Adjust Overlap" Checked="false" />
     </span>
     <span style="padding-left:20px;">
        
        <asp:HiddenField ID="hidvalueCodeDelete" runat="server" />
        <script type="text/javascript">
            function codedeleteflag() {
                
                var r = document.getElementById('<%= this.hidvalueCodeDelete.ClientID%>').value;

                if (r != "1") {
                    return confirm('No Code Ending instruction in this rate plan, are you sure to commit all changes?');
                }
                else {
                    return confirm('Are you sure to commit all changes?');
                }
            }
      function codedeleteexists() {
          //confirm('test2');
          PageMethods.CodeDeleteExists(onSuccess, onFailure);
      }
 
      function onSuccess(result) {
          confirm('hello!');
          if (result != "1") {//no code delete exists
              confirm('No Code Ending instruction in this rate plan, are you sure to continue?');
          }
          confirm('Cancel');
      }
 
      function onFailure(error) {
          alert(error);
          return false;
      }
      InsertLabelData();
    </script>
         
        
        <asp:LinkButton ID="LinkButtonSaveAll" runat="server" 
            OnClientClick="return codedeleteflag();"
            onclick="LinkButtonSaveAll_Click"
            >Commit Changes</asp:LinkButton>

            <span style="padding-left:15px;">
            <asp:LinkButton ID="LinkButtonTaskOnly" runat="server" 
            onclick="LinkButtonDeleteAll_Click"
            OnClientClick="return confirm('Existing tasks in this task reference will be deleted, are you sure?');" 
            Visible="true">Delete All</asp:LinkButton>
        </span>
        <span style="padding-left:15px;">
            <asp:LinkButton ID="LinkButtonDelCommitted" runat="server" 
            onclick="LinkButtonDeleteCommitted_Click"
            OnClientClick="return confirm('Committed tasks in this rate plan will be deleted, are you sure?');" 
            Visible="true">Delete Completed</asp:LinkButton>
        </span>

        <span style="padding-left:15px;">
            <asp:LinkButton ID="LinkButtonDeleteSelected" runat="server" 
            onclick="LinkButtonDeleteSelected_Click"
            OnClientClick="return confirm('Selected tasks in this rate plan will be deleted, are you sure?');" 
            Visible="true">Delete Selected</asp:LinkButton>
        </span>
        
     </span>
            

</div>   
            
        
<div style="min-height:1px;overflow:visible;">
<asp:Label runat="server" id="StatusLabel" text="" ForeColor="Red" Width="800" />
</div>
      <%--  </ContentTemplate>
</asp:UpdatePanel>--%>

<div style="width:2100px;overflow:visible;margin-top:1px;">
    <asp:GridView ID="GridViewSupplierRates" runat="server" AllowPaging="True" 
        AutoGenerateColumns="False" DataKeyNames="id" CellPadding="4" ForeColor="#333333" 
        GridLines="Vertical" onprerender="GridViewSupplierRates_PreRender" 
        onrowcommand="GridViewSupplierRates_RowCommand" 
        onrowdatabound="GridViewSupplierRates_RowDataBound" 
        onrowediting="GridViewSupplierRates_RowEditing" style="margin-left: 0px" 
        onrowcancelingedit="GridViewSupplierRates_RowCancelingEdit" 
        onrowupdating="GridViewSupplierRates_RowUpdating" 
        onrowupdated="GridViewSupplierRates_RowUpdated" 
        onpageindexchanging="GridViewSupplierRates_PageIndexChanging" PageSize="20"
        font-size="9pt" DataSourceID="EntityDataRateTask" 
        onrowdeleting="GridViewSupplierRates_RowDeleting"
        PagerSettings-LastPageText="&lt;&lt;" PagerSettings-Mode="NumericFirstLast">
        <AlternatingRowStyle BackColor="#f2f2f2" ForeColor="#284775" />
        
        
        <Columns>
            <%--<asp:CommandField ShowDeleteButton="True" ShowEditButton="True" />--%>

             <asp:TemplateField>
                <ItemTemplate>
                    <asp:LinkButton ID="LinkButtonEdit" ButtonType="Button" CommandName="Edit"  runat="server">Edit</asp:LinkButton>
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:LinkButton ID="LinkButtonEdit" ButtonType="Button" CommandName="Update"  runat="server" CausesValidation="false" ValidationGroup="allcontrols">Update</asp:LinkButton>
                    <asp:LinkButton ID="LinkButtonCancel" ButtonType="Button" CommandName="myCancel"  runat="server">Cancel</asp:LinkButton>
                </EditItemTemplate>
            </asp:TemplateField>

            <asp:TemplateField>
                <ItemTemplate>
                    <asp:LinkButton ID="LinkButtonDelete" ButtonType="Button" 
                    OnClientClick='return confirm("Are you sure you want to delete this entry?");'
                    CommandName="Delete"  runat="server">Delete</asp:LinkButton>
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:LinkButton ID="LinkButtonDelete" Visible="false" ButtonType="Button" CommandName="myDelete"  runat="server">Delete</asp:LinkButton>
                </EditItemTemplate>
            </asp:TemplateField>

            <asp:TemplateField HeaderText="Selected" SortExpression="">
                <ItemTemplate>
                    <asp:CheckBox ID="CheckBoxSelected" runat="server" Enabled="true" />
                </ItemTemplate>
            </asp:TemplateField>
            
            <asp:TemplateField HeaderText="Complete" SortExpression="ChangeCommitted" ControlStyle-Width="20px">
                
                <ItemTemplate>
                    <asp:CheckBox ID="CheckBox1" runat="server" Enabled="false" />
                </ItemTemplate>

<ControlStyle Width="20px"></ControlStyle>
            </asp:TemplateField>
           
            
             <asp:TemplateField HeaderText="Type" SortExpression="Status" ItemStyle-Wrap="false">
                
                <ItemTemplate>
                    <asp:Label ID="lblRateChangeType" runat="server" ></asp:Label>
                </ItemTemplate>
                 <EditItemTemplate>
                    <asp:Label ID="lblRateChangeType" runat="server" ></asp:Label>
                </EditItemTemplate>


            </asp:TemplateField>
            
            <asp:TemplateField HeaderText="Execution Order" SortExpression="ExecutionOrder" ControlStyle-Width="50px">
                
                <ItemTemplate>
                    <asp:Label ID="lblExecutionOrder" runat="server" Text='<%#Eval("ExecutionOrder")!=null?Eval("ExecutionOrder").ToString():"" %>' ></asp:Label>
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:Label ID="lblExecutionOrder" runat="server" Text='<%#Eval("ExecutionOrder")!=null?Eval("ExecutionOrder").ToString():"" %>'  ></asp:Label>
                </EditItemTemplate>

<ControlStyle Width="50px"></ControlStyle>
            </asp:TemplateField>

            <asp:TemplateField HeaderText="id" SortExpression="id" ControlStyle-Width="50px" Visible="false">
                <ItemTemplate>
                    <asp:Label ID="lblId" runat="server" Text='<%# Eval("id").ToString() %>'></asp:Label>
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:Label ID="lblId" runat="server" Text='<%# Eval("id").ToString() %>'></asp:Label>
                </EditItemTemplate>

<ControlStyle Width="50px"></ControlStyle>
            </asp:TemplateField>

            
            <asp:TemplateField HeaderText="Prefix" SortExpression="Prefix" ItemStyle-Wrap="false">
                <ItemTemplate>
                    <asp:Label ID="lblPrefix" runat="server" Text='<%#Eval("Prefix")!=null?Eval("Prefix").ToString():"" %>'></asp:Label>
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:TextBox ID="txtPrefix" runat="server" Text='<%#Eval("Prefix")!=null?Eval("Prefix").ToString():""%>'></asp:TextBox>

                </EditItemTemplate>





            </asp:TemplateField>

            
            <asp:TemplateField HeaderText="Description" SortExpression="Description" ItemStyle-Wrap="false">
                <ItemTemplate>
                    <asp:Label ID="lblDescription" runat="server" Text='<%# Eval("description")!=null?Eval("description").ToString():"" %>'></asp:Label>
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:TextBox ID="txtDescription" Enabled="True" runat="server" Text='<%# Bind("description") %>'></asp:TextBox>
                </EditItemTemplate>


            </asp:TemplateField>


            
            <asp:TemplateField HeaderText="Rate" SortExpression="rateamount" ItemStyle-Wrap="false"  >
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


                <asp:TemplateField HeaderText="Effective Since" SortExpression="startdate" ControlStyle-Width="125px">
                
                <ItemTemplate>
                    <asp:Label ID="lblStartDate" runat="server" Text='<%# Eval("startdate") %>'></asp:Label>
                </ItemTemplate>
                    
                <EditItemTemplate>
                    Date:
                    <%--<asp:Calendar ID="CalendarStartDate" runat="server"></asp:Calendar>--%>
                    
                    <asp:TextBox ID="TextBoxStartDatePicker" runat="server">
                  </asp:TextBox>
                    <asp:CalendarExtender ID="CalendarStartDate" runat="server" 
                                    TargetControlID="TextBoxStartDatePicker"  PopupButtonID="TextBoxStartDatePicker" Format="yyyy-MM-dd">
                  </asp:CalendarExtender>
                  <br />
                  Time: <asp:TextBox ID="TextBoxStartDateTimePicker" runat="server">
                  </asp:TextBox>
                
                </EditItemTemplate>
                 
                    
                
<ControlStyle Width="115px"></ControlStyle>
                 
                    
                
            </asp:TemplateField>

             <asp:TemplateField HeaderText="Valid Before" SortExpression="enddate" ControlStyle-Width="125px">
                
                <ItemTemplate>
                    <asp:Label ID="lblEndDate" runat="server" Text='<%# Eval("enddate")%>'></asp:Label>
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

<ControlStyle Width="125px"></ControlStyle>
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

            <asp:TemplateField HeaderText="Min Duration (Sec)" SortExpression="MinDurationSec" ControlStyle-Width="80" >
                <ItemTemplate>
                    <asp:Label ID="lblMinDurationSec" runat="server" Text='<%#Eval("MinDurationSec")!=null?Eval("MinDurationSec").ToString():"" %>'></asp:Label>
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:TextBox ID="txtMinDurationSec" Enabled="True" runat="server" Text='<%# Eval("MinDurationSec")!=null?Eval("MinDurationSec").ToString():"" %>'></asp:TextBox>
                </EditItemTemplate>

            </asp:TemplateField>

            
             <asp:TemplateField HeaderText="Country" SortExpression="CountryCode" ControlStyle-Width="200px" >
                
                <ItemTemplate>
                  
                    <asp:Label ID="lblCountry" runat="server" Text=""></asp:Label>
                </ItemTemplate>
                
                <EditItemTemplate>
                    <asp:DropDownList ID="DropDownListCountry" runat="server" AutoPostBack="True" 
                        DataSourceID="SqlDataCountry" DataTextField="Name" DataValueField="Code" SelectedValue='<%# Bind("CountryCode") %>'
                        Enabled="True" >
                    </asp:DropDownList>
                    
                    
                </EditItemTemplate>

<ControlStyle Width="200px"></ControlStyle>
            </asp:TemplateField>

             <asp:TemplateField HeaderText="Round Digits after Decimal for Rate Amount" SortExpression="" Visible="true" ControlStyle-Width="200">
                <ItemTemplate>
                    <asp:Label ID="lblRoundDigits" runat="server" Text='<%# Eval("RateAmountRoundupDecimal")==null?"": Eval("RateAmountRoundupDecimal").ToString() %>'></asp:Label>
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:TextBox ID="txtRoundDigits" Enabled="true" runat="server" Text='<%# Eval("RateAmountRoundupDecimal")==null?"": Eval("RateAmountRoundupDecimal").ToString() %>'></asp:TextBox>
                </EditItemTemplate>
            </asp:TemplateField>
            
            <asp:TemplateField HeaderText="Other Amount 1" SortExpression="OtherAmount1" Visible="true" ControlStyle-Width="30px">
                <ItemTemplate>
                    <asp:Label ID="lblOtherAmount1" runat="server" Text='<%# Eval("OtherAmount1")==null?"": Eval("OtherAmount1").ToString() %>'></asp:Label>
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:TextBox ID="txtOtherAmount1" Enabled="true" runat="server" Text='<%# Eval("OtherAmount1")==null?"": Eval("OtherAmount1").ToString() %>'></asp:TextBox>
                </EditItemTemplate>
            </asp:TemplateField>

            <asp:TemplateField HeaderText="Other Amount 2" SortExpression="OtherAmount2" Visible="true" ControlStyle-Width="50px">
                <ItemTemplate>
                    <asp:Label ID="lblOtherAmount2" runat="server" Text='<%# Eval("OtherAmount2")==null?"": Eval("OtherAmount2").ToString() %>'></asp:Label>
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:TextBox ID="txtOtherAmount2" Enabled="true" runat="server" Text='<%# Eval("OtherAmount2")==null?"": Eval("OtherAmount2").ToString() %>'></asp:TextBox>
                </EditItemTemplate>

<ControlStyle Width="50px"></ControlStyle>
            </asp:TemplateField>

            <asp:TemplateField HeaderText="Other Amount 3" SortExpression="OtherAmount3" Visible="true" ControlStyle-Width="50px">
                <ItemTemplate>
                    <asp:Label ID="lblOtherAmount3" runat="server" Text='<%# Eval("OtherAmount3")==null?"": Eval("OtherAmount3").ToString() %>'></asp:Label>
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:TextBox ID="txtOtherAmount3" Enabled="true" runat="server" Text='<%# Eval("OtherAmount3")==null?"": Eval("OtherAmount3").ToString() %>'></asp:TextBox>
                </EditItemTemplate>

<ControlStyle Width="50px"></ControlStyle>
            </asp:TemplateField>

            <asp:TemplateField HeaderText="Other Amount 4" SortExpression="OtherAmount4" Visible="true" ControlStyle-Width="70px">
                <ItemTemplate>
                    <asp:Label ID="lblOtherAmount4" runat="server" Text='<%# Eval("OtherAmount4")==null?"0": Convert.ToDecimal(Eval("OtherAmount4").ToString()).ToString("0.#00000") %>'></asp:Label>
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:TextBox ID="txtOtherAmount4" Enabled="true" runat="server" Text='<%# Eval("OtherAmount4")==null?"0": Convert.ToDecimal(Eval("OtherAmount4").ToString()).ToString("0.#00000") %>'></asp:TextBox>
                </EditItemTemplate>

<ControlStyle Width="70px"></ControlStyle>
            </asp:TemplateField>

            <asp:TemplateField HeaderText="Other Amount 5" SortExpression="OtherAmount5" Visible="true" ControlStyle-Width="70px">
                <ItemTemplate>
                    <asp:Label ID="lblOtherAmount5" runat="server" Text='<%# Eval("OtherAmount5")==null?"0": Convert.ToDecimal(Eval("OtherAmount5").ToString()).ToString("0.#00000") %>'></asp:Label>
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:TextBox ID="txtOtherAmount5" Enabled="true" runat="server" Text='<%# Eval("OtherAmount5")==null?"0": Convert.ToDecimal(Eval("OtherAmount5").ToString()).ToString("0.#00000") %>'></asp:TextBox>
                </EditItemTemplate>

<ControlStyle Width="70px"></ControlStyle>
            </asp:TemplateField>

            <asp:TemplateField HeaderText="Other Amount 6" SortExpression="OtherAmount6" Visible="true" ControlStyle-Width="50px">
                <ItemTemplate>
                    <asp:Label ID="lblOtherAmount6" runat="server" Text='<%# Eval("OtherAmount6")==null?"0": Convert.ToDecimal(Eval("OtherAmount6").ToString()).ToString("0.#00000") %>'></asp:Label>
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:TextBox ID="txtOtherAmount6" Enabled="true" runat="server" Text='<%# Eval("OtherAmount6")==null?"0": Convert.ToDecimal(Eval("OtherAmount6").ToString()).ToString("0.#00000") %>'></asp:TextBox>
                </EditItemTemplate>

<ControlStyle Width="50px"></ControlStyle>
            </asp:TemplateField>

            <asp:TemplateField HeaderText="Other Amount 7" SortExpression="OtherAmount7" Visible="true" ControlStyle-Width="50px">
                <ItemTemplate>
                    <asp:Label ID="lblOtherAmount7" runat="server" Text='<%# Eval("OtherAmount7")==null?"0": Convert.ToDecimal(Eval("OtherAmount7").ToString()).ToString("0.#00000") %>'></asp:Label>
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:TextBox ID="txtOtherAmount7" Enabled="true" runat="server" Text='<%# Eval("OtherAmount7")==null?"0": Convert.ToDecimal(Eval("OtherAmount7").ToString()).ToString("0.#00000") %>'></asp:TextBox>
                </EditItemTemplate>

<ControlStyle Width="50px"></ControlStyle>
            </asp:TemplateField>

            <asp:TemplateField HeaderText="Other Amount 8" SortExpression="OtherAmount8" Visible="true" ControlStyle-Width="50px">
                <ItemTemplate>
                    <asp:Label ID="lblOtherAmount8" runat="server" Text='<%# Eval("OtherAmount8")==null?"0": Convert.ToDecimal(Eval("OtherAmount8").ToString()).ToString("0.#00000") %>'></asp:Label>
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:TextBox ID="txtOtherAmount8" Enabled="true" runat="server" Text='<%# Eval("OtherAmount8")==null?"0": Convert.ToDecimal(Eval("OtherAmount8").ToString()).ToString("0.#00000") %>'></asp:TextBox>
                </EditItemTemplate>

<ControlStyle Width="50px"></ControlStyle>
            </asp:TemplateField>

            <asp:TemplateField HeaderText="Other Amount 9" SortExpression="OtherAmount9" Visible="false" ControlStyle-Width="50px">
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




           <asp:TemplateField HeaderText="Fixed Charge Duration (Sec)" SortExpression="SurchargeTime" ControlStyle-Width="100px" >
                <ItemTemplate>
                    <asp:Label ID="lblSurchargeTime" runat="server" Text='<%# Eval("SurchargeTime").ToString() %>'></asp:Label>
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:TextBox ID="txtSurchargeTime" Enabled="True" runat="server" Text='<%# Eval("SurchargeTime").ToString() %>'></asp:TextBox>
                </EditItemTemplate>

            </asp:TemplateField>

            <asp:TemplateField HeaderText="Fixed Charge Amount" SortExpression="SurchargeAmount" ControlStyle-Width="30px" >
                <ItemTemplate>
                    <asp:Label ID="lblSurchargeAmount" runat="server" Text='<%# Convert.ToDecimal(Eval("SurchargeAmount").ToString()).ToString("0.#00000") %>'></asp:Label>
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:TextBox ID="txtSurchargeAmount" Enabled="True" runat="server" Text='<%# Eval("SurchargeAmount").ToString() %>'></asp:TextBox>
                </EditItemTemplate>

<ControlStyle Width="30px"></ControlStyle>
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
                        Enabled="True" >
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
                        Enabled="true" >
                    </asp:DropDownList>
                    
                    
                </EditItemTemplate>

<ControlStyle Width="100px"></ControlStyle>
            </asp:TemplateField>

            <asp:TemplateField HeaderText="Validation Error(s)" SortExpression="Field2" >
                
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
        <FooterStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
        <HeaderStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />

<PagerSettings LastPageText="&lt;&lt;"></PagerSettings>

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

</asp:Content>

