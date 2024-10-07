<%@ Page Title="" Language="C#" MasterPageFile="~/Site.master" 
Theme="" AutoEventWireup="True" CodeBehind="purchasePackage.aspx.cs" Inherits="purchasePackage" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>



<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" Runat="Server">


<asp:HiddenField ID="hidvaluerowcolorchange" runat="server" />

<asp:SqlDataSource ID="SqlDataCountry" runat="server" 
                   ConnectionString="<%$ ConnectionStrings:Partner %>" 
                   ProviderName="<%$ ConnectionStrings:Partner.ProviderName %>" 
                   SelectCommand="	select -1 as idPartner,' [Select]' as PartnerName union all
        select 0 as idPartner,' [None]' as PartnerName union all
select idpartner as idPartner,partnername as PartnerName from partner order by partnername "
></asp:SqlDataSource>
    
<asp:SqlDataSource ID="SqlDataPartnerType" runat="server" 
                   ConnectionString="<%$ ConnectionStrings:Partner %>" 
                   ProviderName="<%$ ConnectionStrings:Partner.ProviderName %>" 
                   SelectCommand="	select -1 as id,' [ All]' as Type union all
        select id as id, type as Type from enumpartnertype "
></asp:SqlDataSource>

    
<%--for formview--%>
<asp:EntityDataSource ID="EntityDataservice" runat="server" 
                      ConnectionString="name=PartnerEntities" DefaultContainerName="PartnerEntities" 
                      EnableFlattening="False" EntitySetName="services" 
                      Select="it.[id], it.[servicename]" 
                      onquerycreated="EntityDataservice_QueryCreated">
    
</asp:EntityDataSource>

<%--for search portion of the form--%>
<asp:SqlDataSource ID="SqlDataservice" runat="server" 
                   ConnectionString="<%$ ConnectionStrings:Partner %>" 
                   ProviderName="<%$ ConnectionStrings:Partner.ProviderName %>" 
                   SelectCommand=
                   "select (select -1) as id,(select '[ All]') as type 
union all 
(select sf.id, concat(servicename,' [',sc.type,']') as type from enumservicefamily sf
 left join enumservicecategory sc
 on sf.servicecategory=sc.id) "
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

        

    
<asp:SqlDataSource ID="SqlDataSubServiceType" runat="server" 
                   ConnectionString="<%$ ConnectionStrings:Partner %>" 
                   ProviderName="<%$ ConnectionStrings:Partner.ProviderName %>" 
                   SelectCommand="select * from enumservicesubcategory"></asp:SqlDataSource>

<asp:EntityDataSource ID="EntityDataRateTask" runat="server" 
                      ConnectionString="name=PartnerEntities" DefaultContainerName="PartnerEntities" 
                      EnableDelete="True" EnableFlattening="False" EnableUpdate="True" 
                      EntitySetName="ratetaskassigns" onquerycreated="EntityDataRateTask_QueryCreated">
</asp:EntityDataSource>



<asp:ToolkitScriptManager ID="ToolkitScriptManager1" runat="server" EnablePageMethods="true" />
    
<span style="padding-left:10px;"> <asp:Label ID="lblRatePlan" runat="server" Font-Bold="true" Text=""></asp:Label> </span>
    
<div style="margin-left:-10px;margin-top:-20px;">
    <asp:Label ID="lblTimeZone" runat="server" Visible="false" Text=""></asp:Label>           
    <div style="float:left;width:200px;padding-top:5px;padding-left:10px;">
        <asp:LinkButton ID="LinkButton1" runat="server" Text="Add New Rate" 
                        onclick="LinkButton1_Click">New Package Purchase</asp:LinkButton>
    </div>
</div>
    
<span style="padding-left:20px;visibility:hidden;">
    <b> Task Reference: </b>
    <asp:DropDownList ID="DropDownListTaskRef" runat="server" AutoPostBack="true" 
                      onselectedindexchanged="DropDownListTaskRef_SelectedIndexChanged">
    </asp:DropDownList>
    <asp:LinkButton ID="EditTaskRefName" runat="server" OnClientClick="var value = prompt('Enter a Task Reference Name:'); SetHidValueRefName(value);" OnClick="EditTaskRefName_Click" 
                    style="margin-left: 0px" Text="Rename" Visible="True"/>
</span>
<span style="padding-left:20px;visibility:hidden;">
    <asp:LinkButton ID="NewTaskRefName" runat="server" OnClientClick="var value = prompt('Enter a Task Reference Name:'); SetHidValueRefName(value);" OnClick="NewTaskRefName_Click" 
                    style="margin-left: 0px" Text="New Task Reference" Visible="True"/>
    <input type="hidden" id="hidValueRefName" runat="server" />
    <script type="text/javascript">

        function SetHidValueRefName(value) {
            document.getElementById("<%=hidValueRefName.ClientID%>").value = value;
        }
            
    </script>
</span>

    
<br />
    
         
         
        

<div style="min-width:1500px;height:0px;background-color:#f2f2f2;padding-left:10px;margin-bottom:5px;padding-top:5px;margin-top:3px;visibility:hidden;overflow:hidden;">
    <div style="color:Black;">Import or Create New Rate Task</div> 
    <div style="height:85px;border-style:ridge;width:750px;float:left;padding-top:2.5px;background-color:#F7F6F3;color:Black;">
            
        <asp:Label ID="Label4" runat="server" Text="Default Effective Date (Partner's TZ)"></asp:Label>
        <span style="padding-left:11px;"><asp:TextBox ID="TextBoxDefaultDate" runat="server"> </asp:TextBox></span>
                      
        <asp:CalendarExtender ID="CalendarStartDate" runat="server" 
                              TargetControlID="TextBoxDefaultDate"  PopupButtonID="TextBoxStartDatePicker" Format="yyyy-MM-dd">
        </asp:CalendarExtender>
        Time: <asp:TextBox ID="TextBoxDefaultTime" runat="server" Text="00:00:00">
        </asp:TextBox>
        <asp:CheckBox ID="CheckBoxDefaultDate" runat="server" Text="Use if not found per prefix" />

        <p></p>
        <asp:Label ID="Label5" runat="server" Text="Prev Prefix Ending Date (Partner's TZ)"></asp:Label>
        <asp:TextBox ID="TextBoxDefaultDeleteDate" runat="server">
        </asp:TextBox>
        <asp:CalendarExtender ID="CalendarExtender1" runat="server" 
                              TargetControlID="TextBoxDefaultDeleteDate"  PopupButtonID="TextBoxStartDatePicker" Format="yyyy-MM-dd">
        </asp:CalendarExtender>
        Time: <asp:TextBox ID="TextBoxDefaultDeleteTime" runat="server" Text="00:00:00">
        </asp:TextBox>
        <asp:CheckBox ID="CheckBoxDefaultDeleteDate" runat="server" Text="End all active prefixes" />

        <div style="height:22px;width:750px;margin-left:0px;padding-top:10px;float:left;background-color:#F7F6F3;color:Black;">

                
            Import Rate Sheet

            <asp:FileUpload id="FileUploadControl" runat="server" />

            <span style="padding-left:10px;">
                <asp:LinkButton runat="server" id="UploadButton" text="Import" 
                                OnClientClick="return confirm('Exising tasks will be deleted under current task reference. Are you sure to import?');"
                                onclick="UploadButton_Click" />
            </span>
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
    <div style="padding-left:5px;padding-top:15px;">
        <asp:CheckBox ID="CheckBoxAutoConvertTZ" runat="server" Text="Auto Adjust Timezone for New/Import Task" Checked="false"></asp:CheckBox>
    </div> 
            

        

    <div style="height:4px;width:800px;clear:both;"></div>

        
    
        
    <asp:HiddenField ID="HiddenFieldSelect" runat="server" Value="0" />
    <script type="text/javascript">
        function SetHidValueFilter() {
            document.getElementById("<%=HiddenFieldSelect.ClientID%>").value = "1";
        }
    </script>

        
       
                   

</div>

<%--//mapping ratetask vs partnerrate assignment fields

//tuple:
idpartner,priority,service, goes to table rateplanassignmenttuple, id in the table is one tuple
//in table ratetaskassign:
//prefix=tupleid
--%>
    



<asp:ValidationSummary ID="ValidatorSummary" runat="server"
                       ValidationGroup="allcontrols" ForeColor="Red" />

    
    
<asp:FormView ID="frmSupplierRatePlanInsert" runat="server" DataKeyNames="id"                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                             
              Width="1000px" DefaultMode="Insert" 
              Height="76px" CellPadding="4" Font-Size="Small" ForeColor="#333333" 
              Visible="False" oniteminserted="frmSupplierRatePlanInsert_ItemInserted" 
              onitemcreated="frmSupplierRatePlanInsert_ItemCreated" 
              onload="frmSupplierRatePlanInsert_Load" 
              oniteminserting="frmSupplierRatePlanInsert_ItemInserting" 
              onmodechanging="frmSupplierRatePlanInsert_ModeChanging" 
              ondatabound="frmSupplierRatePlanInsert_DataBound" 
              onmodechanged="frmSupplierRatePlanInsert_ModeChanged" BorderStyle="None" 
              BorderWidth="2">

    <FooterStyle BackColor="#990000" Font-Bold="True" ForeColor="White" />
    <HeaderStyle BackColor="#990000" Font-Bold="True" ForeColor="White" />

    <InsertItemTemplate>
        <asp:UpdatePanel ID="UpdatePanel1" runat="server">
            
            <ContentTemplate>
 

                <table border="0" width="980px">
                    <tr>
                        <td style="width:70px;"></td>
                        <td > <%--width="200px"--%>
                
                            <div style="float:left;text-align:right; min-width:200px;margin-top:0px;">
                                
                                
                                <b>Partner:</b>
                                <asp:DropDownList ID="DropDownListPartner" runat="server" AutoPostBack="True" 
                                                  DataTextField="PartnerName" DataValueField="idpartner" 
                                                  Enabled="True" OnSelectedIndexChanged="FormviewPartnerSelectedIndexChanged" >
                                </asp:DropDownList>
                                <br />
                                <b>Package</b>
                                <asp:DropDownList ID="DropDownListRatePlan" runat="server" AutoPostBack="True" 
                                                  DataTextField="rateplanname" OnSelectedIndexChanged="DropDownListPackage_OnSelectedIndexChanged" DataValueField="id" 
                                                  Enabled="true" Visible="true" >
                                </asp:DropDownList>
                                <br />
                                <b>Price</b> <asp:TextBox ID="peiceTextBox" runat="server" ReadOnly="true">
                                </asp:TextBox><br />
                                <b>Vat</b> <asp:TextBox ID="vatTextBox" runat="server" ReadOnly="true">
                                </asp:TextBox><br />
                                <b>Discount</b> <asp:TextBox ID="discountTextBox" runat="server">
                                </asp:TextBox><br />
                                <asp:RegularExpressionValidator 
                                    ID="regexDiscountValidator" 
                                    ControlToValidate="discountTextBox" 
                                    ErrorMessage="Please enter a valid number" 
                                    ValidationExpression="^\d+$" 
                                    ForeColor="Red" 
                                    runat="server" 
                                    Display="Dynamic">
                                </asp:RegularExpressionValidator>
                                <br/>
                                <b>Total</b> <asp:TextBox ID="totalCostTextBox" runat="server" ReadOnly="true">
                                </asp:TextBox><br />
                                <asp:Button ID="MyButton" runat="server" Text="Purchase" OnClick="MyButton_Click" />
                                <%--<b> Assignment Order:</b>--%>
                                <asp:TextBox ID="txtResolution" Enabled="True" Visible="false" runat="server" Text=""></asp:TextBox>
                                <br />
                               <%--<b>Price</b>--%> <asp:TextBox ID="TextBoxForPrice" Visible="false" runat="server">
                                </asp:TextBox>
                                <br/>
                                <%--<b> Effective From:</b> <br />--%>
                                <%--Date:--%> <asp:TextBox ID="TextBoxStartDatePickerFrm" runat="server" Visible="false">
                                </asp:TextBox>
                                <asp:CalendarExtender ID="CalendarStartDateFrm" runat="server" 
                                                      TargetControlID="TextBoxStartDatePickerFrm"  PopupButtonID="TextBoxStartDatePickerFrm" Format="yyyy-MM-dd">
                                </asp:CalendarExtender>     
                                <br />
                                <%--Time:--%> <asp:TextBox ID="TextBoxStartDateTimePickerFrm" Visible="false" runat="server" Text="00:00:00">
                                </asp:TextBox>
                                <br />

                   

                                <%--<b>Valid Before:</b> <br />--%>
                                <%--Date:--%> <asp:TextBox ID="TextBoxEndDatePickerFrm" runat="server" Visible="false">
                                </asp:TextBox>
                                <asp:CalendarExtender ID="CalendarEndDateFrm" runat="server" 
                                                      TargetControlID="TextBoxEndDatePickerFrm"  PopupButtonID="TextBoxEndDatePickerFrm" Format="yyyy-MM-dd">
                                </asp:CalendarExtender>
                                <br />
                               <%-- Time:--%> <asp:TextBox ID="TextBoxEndDateTimePickerFrm" Visible="false" runat="server" Text="00:00:00">
                                </asp:TextBox>
                                <br />
                              
                                
                                <%--<b>Service Group:</b>--%>
                                <asp:DropDownList ID="DropDownListServiceGroup" runat="server" AutoPostBack="True" Visible="false" OnSelectedIndexChanged="DropDownListServiceGroup_SelectedIndexChanged"
                                                  >
                                </asp:DropDownList>
                                <br />
                                <%--<b>Partner Type:</b>--%>
                                <asp:DropDownList ID="DropDownListPartnerType" runat="server" AutoPostBack="True" 
                                                  DataSourceID="SqlDataPartnerType" DataTextField="Type" DataValueField="id" 
                                                  Enabled="True" OnSelectedIndexChanged="PartnerTypeSelectedIndexChanged" Visible="False" >
                                </asp:DropDownList>
                                <br />
                                
                                       
                                


                                <%--<b>Route:</b>--%>
                                <asp:DropDownList ID="DropDownListRoute" runat="server" AutoPostBack="false" 
                                                  DataTextField="RouteName" DataValueField="idRoute" 
                                                  Enabled="false" Visible="False"  >
                                </asp:DropDownList>
                                <br />

                                <%--<b> Assigned Direction:</b>--%>
                                <asp:DropDownList ID="DropDownListAssignedDirection" runat="server" AutoPostBack="true" 
                                                  DataTextField="name" DataValueField="id" 
                                                  Enabled="true" OnSelectedIndexChanged="FormviewPartnerDirectionSelectedIndexChanged" Visible="False">
                                    <asp:ListItem Value="1" Text="Customer" Selected="True"/>
                                    <asp:ListItem Value="2" Text="Supplier"/>
                                </asp:DropDownList>
                                <br />
               

                                <%--<b>Service Family</b> --%>
                                <asp:DropDownList ID="DropDownListservice" runat="server" AutoPostBack="True"  Visible="False"
                                                  DataTextField="servicename" DataValueField="id" OnSelectedIndexChanged="service_SelectedIndexChanged" 
                                                  Enabled="true" >
                                </asp:DropDownList>
                                <br />

               
                                <%--<b>Billing Rule</b>--%> 
                                <asp:DropDownList ID="DropDownBillingRule" runat="server" 
                                    AutoPostBack="True" OnSelectedIndexChanged="DropDownBillingRule_SelectedIndexChanged" 
                                    Enabled="true" Visible="False">
                                </asp:DropDownList>
                                <br />
                    
                            </div>
                        </td>

          
                        <td > <%--width="200px"--%>
                
                            <div style="float:left;text-align:right;margin-top:0px;margin-right:100px; visibility: hidden">
                
                
                                
                  
                                <span style="padding-right:4px; visibility: hidden">
                                    Exclude From LCR:
                                    <asp:DropDownList id="ddlExcludeLCR" runat="server">
                                        <asp:ListItem Selected="True" Value="0" Text="No"></asp:ListItem>
                                        <asp:ListItem Selected="False" Value="1" Text="Yes"></asp:ListItem>
                                    </asp:DropDownList>
                                </span>
                                </br>
                                <span style="padding-right:4px; visibility: hidden">
                                    Payment Method:
                                    <asp:DropDownList ID="DropDownListBillingCycle" runat="server" AutoPostBack="True" 
                                                      Enabled="true" >
                                    </asp:DropDownList>
                                </span>

                            </div>
                            
                        </td>

       

                    </tr>
            
                </table>
           
          
                <div style="clear:both;"></div>
            </ContentTemplate>
        </asp:UpdatePanel>

        <asp:LinkButton ID="InsertButton" runat="server" CausesValidation="True" 
                        CommandName="Insert" Text="Insert" ValidationGroup="allcontrols"/>
        <asp:Label ID="statusLabel" runat="server" Text='' />
       
        &nbsp;<asp:LinkButton ID="InsertCancelButton" runat="server" CausesValidation="False" 
                              CommandName="Cancel" Text="Cancel" OnClick="FormViewCancel_Click"/>
        
    <asp:LinkButton ID="LinkButton2" runat="server" ForeColor="Brown" CommandArgument='<%#Eval("status")%>' ><%# Eval("Description") %>:<%# Eval("cnt") %></asp:LinkButton>


    </InsertItemTemplate>
       
    <PagerStyle BackColor="#FFCC66" ForeColor="#333333" HorizontalAlign="Center" />
    <RowStyle BackColor="#F7F6F3" ForeColor="#333333" />
        
       
       
</asp:FormView>
  
        


<div style="margin-top:4px;padding-left:10px;">
        
    <div style="width:1000px;height:5px;"></div>
    <div style="height:24px;border-style:ridge;min-width:1040px;margin-left:-10px;padding-top:2.5px;background-color:#f2f2f2;color:Black;">

        <asp:Label ID="Label1" runat="server" Text="Partner"></asp:Label>
        <asp:TextBox ID="TextBoxPartnerFind" runat="server" Width="100px"></asp:TextBox>
        <asp:Label ID="Label2" runat="server" Text=""></asp:Label>
        <asp:Label ID="Label3" runat="server" Text="Effective on a Date"></asp:Label>
        <asp:TextBox ID="TextBoxDateFind" runat="server"  Width="115px"></asp:TextBox>
        <asp:CalendarExtender ID="CalendarExtender2" runat="server" 
                              TargetControlID="TextBoxDateFind"  PopupButtonID="TextBoxDateFind" Format="yyyy-MM-dd">
        </asp:CalendarExtender>
            
            
        <%--Service:--%>
        <asp:DropDownList ID="ddlTypeFind" runat="server" AutoPostBack="false" Visible="False"
                          DataSourceID="SqlDataservice" DataTextField="type" DataValueField="id" 
                          Enabled="True" >
        </asp:DropDownList>
        
        <%--Assigned Direction:--%>
        <asp:DropDownList ID="ddlAssignedDirectionFind" runat="server" AutoPostBack="false" Visible="False" 
                          DataTextField="name" DataValueField="id" 
                          Enabled="true" >
            <asp:ListItem Value="0" Text=" [All]" Selected="True"/>
            <asp:ListItem Value="1" Text="Customer"/>
            <asp:ListItem Value="2" Text="Supplier"/>
        </asp:DropDownList>
        <asp:Button ID="ButtonFindPrefix" runat="server" Text="Find" Width="100px"
                    onclick="ButtonFindPrefix_Click"></asp:Button>
        <asp:Button ID="ButtonFindPrefixSelect" runat="server" Text="Find & Select" Visible="false" Width="50px"
        > </asp:Button>
    </div>
            
    <div style="margin-left:-15px;">
        <asp:ListView ID="ListView1" runat="server" DataSourceID="SqlDataTaskStatus" Visible="false"
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
                            No input was returned.</td>
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
                   ProviderName="<%$ ConnectionStrings:Partner.ProviderName %>" SelectCommand="select (select -1) as status ,(select 'Total') as Description,count(*) as cnt from ratetaskassign where idrateplan=@idRatePlan union all
select (select 7) as status  ,(select 'Complete') as Description,count(*) as cnt from ratetaskassign where idrateplan=@idRatePlan and changecommitted=1 union all
select (select 0) as status  ,(select 'Incomplete') as Description,count(*) as cnt from ratetaskassign where idrateplan=@idRatePlan and changecommitted!=1 union all
select (select 100) as status,(select 'Validation Error') as Description,count(*) as cnt from ratetaskassign where idrateplan=@idRatePlan and field2&gt;0 union all
select (select 9) as status  ,(select 'Overlap') as Description,count(*) as cnt from ratetaskassign where idrateplan=@idRatePlan and status=9 and changecommitted!=1  union all
select (select 13) as status  ,(select 'Existing') as Description,count(*) as cnt from ratetaskassign where idrateplan=@idRatePlan and status=13 and changecommitted !=1 
">

    <SelectParameters>
        <asp:Parameter Name="idRatePlan" Type="Int32" DefaultValue="0" />
    </SelectParameters>

</asp:SqlDataSource>


<div style="height:0px;background-color:#f2f2f2;visibility:hidden;">
    
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
                
                var r = document.getElementById('<%=hidvalueCodeDelete.ClientID%>').value;

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
    

<asp:Label ID="lblUnassignedPlans" runat="server" Text="Unassigned Rateplans" Font-Bold="true"></asp:Label>
<asp:LinkButton ID="lnkCommitchanges" runat="server" 
                onclick="lnkCommitchanges_Click">Commit Changes</asp:LinkButton>

    

<div style="width:2100px;overflow:visible;margin-top:1px;">
<asp:GridView ID="GridViewSupplierRates" runat="server" AllowPaging="True" 
              AutoGenerateColumns="False" DataKeyNames="id" CellPadding="4" ForeColor="Black" 
              GridLines="Vertical" onprerender="GridViewSupplierRates_PreRender" 
              onrowcommand="GridViewSupplierRates_RowCommand" 
              onrowdatabound="GridViewSupplierRates_RowDataBound" 
              onrowediting="GridViewSupplierRates_RowEditing" style="margin-left: 0px" 
              onrowcancelingedit="GridViewSupplierRates_RowCancelingEdit" 
              onrowupdating="GridViewSupplierRates_RowUpdating" 
              onrowupdated="GridViewSupplierRates_RowUpdated" 
              onpageindexchanging="GridViewSupplierRates_PageIndexChanging" PageSize="20"
              font-size="9pt" DataSourceID="EntityDataRateTask"  
              onrowdeleting="GridViewSupplierRates_RowDeleting" BackColor="White" BorderColor="#DEDFDE" BorderStyle="None" BorderWidth="1px"
>
<AlternatingRowStyle BackColor="White" />
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
                        runat="server"
                        OnClick='DeleteWithTransaction'>Delete</asp:LinkButton>
    </ItemTemplate>
    <EditItemTemplate>
        <asp:LinkButton ID="LinkButtonDelete" Visible="false" ButtonType="Button" CommandName="myDelete"  runat="server">Delete</asp:LinkButton>
    </EditItemTemplate>
</asp:TemplateField>


           

<asp:TemplateField HeaderText="Selected" SortExpression="" Visible="false">
    <ItemTemplate>
        <asp:CheckBox ID="CheckBoxSelected" runat="server" Enabled="true" />
    </ItemTemplate>
</asp:TemplateField>
            
<asp:TemplateField HeaderText="In Effect" SortExpression="ChangeCommitted" ControlStyle-Width="20px" Visible="false">
                
    <ItemTemplate>
        <asp:CheckBox ID="CheckBox1" runat="server" Enabled="false" />
    </ItemTemplate>

    <ControlStyle Width="20px"></ControlStyle>
</asp:TemplateField>
           
            
<asp:TemplateField HeaderText="Status" SortExpression="Status" ControlStyle-Width="50px" Visible="true">
                
    <ItemTemplate>
        <asp:Label ID="lblRateChangeType" runat="server" ></asp:Label>
    </ItemTemplate>

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


<asp:TemplateField HeaderText="prefix" SortExpression="id" ControlStyle-Width="50px" Visible="false">
    <ItemTemplate>
        <asp:Label ID="lblPrefix" runat="server" Text='<%# Eval("prefix")%>'></asp:Label>
    </ItemTemplate>
    <EditItemTemplate>
        <asp:Label ID="lblPrefix" runat="server" Text='<%# Eval("prefix")%>'></asp:Label>
    </EditItemTemplate>

    <ControlStyle Width="50px"></ControlStyle>
</asp:TemplateField>

            

<asp:TemplateField HeaderText="Service" SortExpression="Category">
                
    <ItemTemplate>
        <asp:Label ID="lblservice" runat="server" Text=""></asp:Label>
    </ItemTemplate>
                
    <EditItemTemplate>
        <asp:Label ID="lblservice" runat="server" Text=""></asp:Label>
    </EditItemTemplate>

</asp:TemplateField>


<asp:TemplateField HeaderText="Assigned Order" SortExpression="" ControlStyle-Width="30px" >
    <ItemTemplate>
        <asp:Label ID="lblPulse" runat="server" Text=""></asp:Label>
    </ItemTemplate>
    <EditItemTemplate>
        <asp:Label ID="lblPulse" runat="server" Text=""></asp:Label>
                    
    </EditItemTemplate>

    <ControlStyle Width="30px"></ControlStyle>
</asp:TemplateField>

<asp:TemplateField HeaderText="Assigned Direction" SortExpression="SubCategory" HeaderStyle-Wrap="true" HeaderStyle-Width="30px">
                
    <ItemTemplate>
        <asp:Label ID="lblAssignedDirection" runat="server" Text=""></asp:Label>
    </ItemTemplate>
                
    <EditItemTemplate>
        <asp:Label ID="lblAssignedDirection" runat="server" Text=""></asp:Label>
    </EditItemTemplate>
    <ControlStyle Width="30px"></ControlStyle>

    <HeaderStyle Wrap="True" Width="30px"></HeaderStyle>
</asp:TemplateField>


<asp:TemplateField HeaderText="Rate Plan" SortExpression="Category">
                
    <ItemTemplate>
        <asp:DropDownList ID="DropDownListRatePlan" runat="server" AutoPostBack="false" 
                          DataTextField="rateplanname" DataValueField="id" 
                          Enabled="false" >
        </asp:DropDownList>   
                
    </ItemTemplate>
                
    <EditItemTemplate>
        <asp:DropDownList ID="DropDownListRatePlan" runat="server" AutoPostBack="false" 
                          DataTextField="rateplanname" DataValueField="id" 
                          Enabled="true" >
        </asp:DropDownList>
    </EditItemTemplate>

</asp:TemplateField>





<asp:TemplateField HeaderText="Partner" SortExpression="prefix">
                
    <ItemTemplate>
                  
        <asp:Label ID="lblCountry" runat="server" Text="" Width="50px"></asp:Label>
    </ItemTemplate>
                
    <EditItemTemplate>
        <asp:Label ID="lblCountry" runat="server" Text=""></asp:Label>
    </EditItemTemplate>
    <HeaderStyle Width="50" />
    <ItemStyle Width="50" />
</asp:TemplateField>


<asp:TemplateField HeaderText="Route" SortExpression="RouteDisabled" ItemStyle-Width="140px">
                
    <ItemTemplate>
        <asp:Label ID="lblRoute" runat="server" Text=""></asp:Label>
    </ItemTemplate>
                
    <EditItemTemplate>
        <asp:Label ID="lblRoute" runat="server" Text=""></asp:Label>
    </EditItemTemplate>
</asp:TemplateField>

             
            


            
           
           


<asp:TemplateField HeaderText="Effective Since" SortExpression="startdate" ControlStyle-Width="125px">
                
    <ItemTemplate>
        <asp:Label ID="lblStartDate" runat="server" Text='<%# Eval("startdate") %>'></asp:Label>
    </ItemTemplate>
                    
    <EditItemTemplate>
        Date:
        <%--need to keep the old date in invisible label so that when updating end date
                    start date property can be retrieved from this label which is in the same edit item template--%>
        <asp:Label ID="lblStartDate" runat="server" Visible="false" Text=""></asp:Label>
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
            



<asp:TemplateField HeaderText="Validation Error(s)" SortExpression="Field2" >
                
    <ItemTemplate>
        <asp:Label ID="lblRateErrors" runat="server" ></asp:Label>
    </ItemTemplate>

    <EditItemTemplate>
        <asp:Label ID="lblRateErrors" runat="server" ></asp:Label>
    </EditItemTemplate>

</asp:TemplateField>



</Columns>
<FooterStyle BackColor="#CCCC99" />
<HeaderStyle BackColor="#6B696B" Font-Bold="True" ForeColor="White" />
<PagerStyle BackColor="#F7F6F3" ForeColor="Black" 
            HorizontalAlign="Left" />
<RowStyle BackColor="#F7F6F3" Width="5px" />
<AlternatingRowStyle Width="5px" />
<SelectedRowStyle BackColor="#CE5D5A" ForeColor="White" Font-Bold="True" />
<SortedAscendingCellStyle BackColor="#FBFBF2" />
<SortedAscendingHeaderStyle BackColor="#848384" />
<SortedDescendingCellStyle BackColor="#EAEAD3" />
<SortedDescendingHeaderStyle BackColor="#575357" />
</asp:GridView>

    
<%-- <asp:EntityDataSource ID="EntityDataRateAssign" runat="server" 
        ConnectionString="name=PartnerEntities" DefaultContainerName="PartnerEntities" 
        EnableDelete="True" EnableFlattening="False" EnableUpdate="True" 
        EntitySetName="rateassigns" onquerycreated="EntityDataRateAssign_QueryCreated">
                 </asp:EntityDataSource>--%>
    
    
<asp:SqlDataSource ID="SqlDataRateAssign" runat="server" 
                   ConnectionString="<%$ ConnectionStrings:Reader %>" 
                   DeleteCommand="delete from rateassign where id &lt;0" 
                   ProviderName="<%$ ConnectionStrings:Reader.ProviderName %>" 
                   SelectCommand="select * from rateassign" 
                   UpdateCommand="update rateassign set routedisabled=1 where 1=2"></asp:SqlDataSource>
    
    
    
<asp:Label ID="lblAssignedPlans" runat="server" Text="Assigned Package" Font-Bold="true"></asp:Label>
    
    
    
<asp:GridView ID="GridViewRateAssign" runat="server" 
              AutoGenerateColumns="False" DataKeyNames="id" CellPadding="4" ForeColor="#333333" 
              GridLines="Vertical"
              style="margin-left: 0px" 
              font-size="9pt" 
              onrowdatabound="GridViewRateAssign_RowDataBound"
              onrowcommand="GridViewRateAssign_RowCommand"  
              onrowupdating="GridViewRateAssign_RowUpdating"
              onrowupdated="GridViewRateAssign_RowUpdated" 
              onrowcancelingedit="GridViewRateAssign_RowCancelingEdit" onrowediting="GridViewRateAssign_RowEditing" OnRowDeleted="GridViewRateAssign_RowDeleted" OnRowDeleting="GridViewRateAssign_RowDeleting" BorderColor="#CCCCCC" BorderStyle="Solid"
>
<AlternatingRowStyle BackColor="#f2f2f2" ForeColor="#284775" />
<Columns>
<%--<asp:CommandField ShowDeleteButton="True" ShowEditButton="True" />--%>

<asp:TemplateField>
                 
    <ItemTemplate>
        <asp:LinkButton ID="LinkButtonEdit" ButtonType="Button" CommandName="Edit"  runat="server">Edit</asp:LinkButton>
        <asp:LinkButton ID="LinkButtonDelete" ButtonType="Button" OnClientClick='return confirm("Are you sure you want to delete this entry?");'
                        CommandName="Delete"  runat="server">Delete</asp:LinkButton>
        <asp:LinkButton ID="LinkButtonRate" ButtonType="Button" runat="server">Package</asp:LinkButton>
    </ItemTemplate>
    <EditItemTemplate>
        <asp:LinkButton ID="LinkButtonEdit" ButtonType="Button" CommandName="Update"  runat="server" CausesValidation="false" ValidationGroup="allcontrols">Update</asp:LinkButton>
        <asp:LinkButton ID="LinkButtonCancel" ButtonType="Button" CommandName="Cancel"  runat="server">Cancel</asp:LinkButton>
    </EditItemTemplate>
</asp:TemplateField>

            

<asp:TemplateField HeaderText="Selected" SortExpression="" Visible="false">
    <ItemTemplate>
        <asp:CheckBox ID="CheckBoxSelected" runat="server" Enabled="true" />
    </ItemTemplate>
</asp:TemplateField>
            
<asp:TemplateField HeaderText="Deactive" SortExpression="ChangeCommitted" ControlStyle-Width="20px" Visible="false">
                
    <ItemTemplate>
        <asp:CheckBox ID="CheckBox1" runat="server" Enabled="false" />
    </ItemTemplate>

    <ControlStyle Width="20px"></ControlStyle>
</asp:TemplateField>
           
            

<asp:TemplateField HeaderText="Status" SortExpression="Status" ControlStyle-Width="50px" Visible="false">
                
    <ItemTemplate>
        <asp:Label ID="lblRateChangeType" runat="server" ></asp:Label>
    </ItemTemplate>

    <EditItemTemplate>
        <asp:Label ID="lblRateChangeType" runat="server" ></asp:Label>
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

<asp:TemplateField HeaderText="Partner" SortExpression="idpartner" ItemStyle-Width="140px">
                
    <ItemTemplate>
                  
        <asp:Label ID="lblCountry" runat="server" Text=""></asp:Label>
    </ItemTemplate>
                
    <EditItemTemplate>
        <asp:Label ID="lblCountry" runat="server" Text=""></asp:Label>
    </EditItemTemplate>
</asp:TemplateField>

<asp:TemplateField HeaderText="Service" SortExpression="Category" Visible="False">
                
    <ItemTemplate>
        <asp:Label ID="lblservice" runat="server" Text=""></asp:Label>
    </ItemTemplate>
                
    <EditItemTemplate>
        <asp:Label ID="lblservice" runat="server" Text=""></asp:Label>
    </EditItemTemplate>

</asp:TemplateField>

<asp:TemplateField HeaderText="Assigned Order" SortExpression="" ItemStyle-Width="50px" >
    <ItemTemplate>
        <asp:Label ID="lblPulse" runat="server" Text=""></asp:Label>
    </ItemTemplate>
    <EditItemTemplate>
        <asp:Label ID="lblPulse" runat="server" Text=""></asp:Label>
    </EditItemTemplate>


</asp:TemplateField>

             
            
<asp:TemplateField HeaderText="Assigned Direction" SortExpression="" ItemStyle-Width="40px">
                
    <ItemTemplate>
        <asp:Label ID="lblAssignDir" runat="server" Text=""></asp:Label>
    </ItemTemplate>
                
    <EditItemTemplate>
        <asp:Label ID="lblAssignDir" runat="server" Text=""></asp:Label>
    </EditItemTemplate>

</asp:TemplateField>




<asp:TemplateField HeaderText="Route" SortExpression="RouteDisabled" Visible="False">
                
    <ItemTemplate>
        <asp:Label ID="lblRoute" runat="server" Text=""></asp:Label>
    </ItemTemplate>
                
    <EditItemTemplate>
        <asp:Label ID="lblRoute" runat="server" Text=""></asp:Label>
    </EditItemTemplate>
</asp:TemplateField>

<asp:TemplateField HeaderText="Rate Plan" SortExpression="Category" ControlStyle-Width="150px" Visible="False">
                
    <ItemTemplate>
        <asp:DropDownList ID="DropDownListRatePlan" runat="server" AutoPostBack="false" 
                          DataTextField="rateplanname" DataValueField="id" 
                          Enabled="false" >
        </asp:DropDownList>   
                
    </ItemTemplate>
                
    <EditItemTemplate>
        <asp:DropDownList ID="DropDownListRatePlan" runat="server" AutoPostBack="false" 
                          DataTextField="rateplanname" DataValueField="id" 
                          Enabled="true" >
        </asp:DropDownList>
    </EditItemTemplate>

</asp:TemplateField>

<asp:TemplateField HeaderText="prefix" SortExpression="id" ControlStyle-Width="50px" Visible="false">
    <ItemTemplate>
        <asp:Label ID="lblPrefix" runat="server" Text='<%# Eval("prefix")%>'></asp:Label>
    </ItemTemplate>
    <EditItemTemplate>
        <asp:Label ID="lblPrefix" runat="server" Text='<%# Eval("prefix")%>'></asp:Label>
    </EditItemTemplate>

    <ControlStyle Width="50px"></ControlStyle>
</asp:TemplateField>

<asp:TemplateField HeaderText="Billing Rule" ControlStyle-Width="150px" Visible="False">
    <ItemTemplate>
        <asp:DropDownList runat="server" ID="DropDownListBillingRule" AutoPostBack="False" Enabled="False"/>
    </ItemTemplate>
    <EditItemTemplate>
        <asp:DropDownList runat="server" ID="DropDownListBillingRule" AutoPostBack="False" Enabled="True"/>
    </EditItemTemplate>
</asp:TemplateField>             
           

<asp:TemplateField HeaderText="Effective Since" SortExpression="startdate" ControlStyle-Width="125px">
                
    <ItemTemplate>
        <asp:Label ID="lblStartDate" runat="server" Text='<%# Convert.ToDateTime(Eval("startdate")).ToString("yyyy-MM-dd HH:mm:ss") %>'></asp:Label>
    </ItemTemplate>
                    
    <EditItemTemplate>
        <div style="text-align:right;">
            <div style="float:right;">
                Date:
                <%--need to keep the old date in invisible label so that when updating end date
                            start date property can be retrieved from this label which is in the same edit item template--%>
                <asp:Label ID="lblStartDate" runat="server" Visible="false" Text=""></asp:Label>
                <asp:TextBox ID="TextBoxStartDatePicker" runat="server" Enabled="false">
                </asp:TextBox>
                <asp:CalendarExtender ID="CalendarStartDate" runat="server" 
                                      TargetControlID="TextBoxStartDatePicker"  PopupButtonID="TextBoxStartDatePicker" Format="yyyy-MM-dd">
                </asp:CalendarExtender>
            </div>
            <div style="clear:right;">
                Time: <asp:TextBox ID="TextBoxStartDateTimePicker" runat="server" Enabled="false">
                </asp:TextBox>
            </div>
        </div>
                      
                  
                  
                      
                  
                    
                
    </EditItemTemplate>
                 
                    
                
    <ControlStyle Width="115px"></ControlStyle>
                 
                    
                
</asp:TemplateField>

<asp:TemplateField HeaderText="Valid Before" SortExpression="enddate" ControlStyle-Width="125px">
                
    <ItemTemplate>
        <asp:Label ID="lblEndDate" runat="server" Text=""></asp:Label>
    </ItemTemplate>
                    
    <EditItemTemplate>
                    
        <div style="text-align:right;">

            <div style="float:right;">
                Date:
                <asp:TextBox ID="TextBoxEndDatePicker" runat="server">
                </asp:TextBox>
                <asp:CalendarExtender ID="CalendarEndDate" runat="server" 
                                      TargetControlID="TextBoxEndDatePicker"  PopupButtonID="TextBoxEndDatePicker" Format="yyyy-MM-dd">
                </asp:CalendarExtender>
            </div>

            <div style="clear:right;">
                Time: <asp:TextBox ID="TextBoxEndDateTimePicker" Text="00:00:00" runat="server">
                </asp:TextBox>
            </div>

        </div>

                     
                  
                  


    </EditItemTemplate>

    <ControlStyle Width="125px"></ControlStyle>
</asp:TemplateField>
            
<asp:TemplateField HeaderText="Exclude from LCR" SortExpression="field3" Visible="False" >
                
    <ItemTemplate>
        <asp:Label ID="lblExcludeLCR" runat="server" Text=""></asp:Label>
    </ItemTemplate>
                    
    <EditItemTemplate>
                    
        <asp:DropDownList id="ddlExcludeLCR" runat="server">
            <asp:ListItem Selected="True" Value="0" Text="No"></asp:ListItem>
            <asp:ListItem Selected="False" Value="1" Text="Yes"></asp:ListItem>
        </asp:DropDownList>

    </EditItemTemplate>

    <%--<ControlStyle Width="125px"></ControlStyle>--%>
</asp:TemplateField>

<asp:TemplateField HeaderText="Validation Error(s)" SortExpression="Field2" Visible="false" >
                
    <ItemTemplate>
        <asp:Label ID="lblRateErrors" runat="server" ></asp:Label>
    </ItemTemplate>

    <EditItemTemplate>
        <asp:Label ID="lblRateErrors" runat="server" ></asp:Label>
    </EditItemTemplate>

</asp:TemplateField>
<asp:TemplateField HeaderText="Billing Params" SortExpression="BillingParams" Visible="false" >
                
    <ItemTemplate>
        <asp:Label ID="lblBillingInfo" runat="server" ></asp:Label>
    </ItemTemplate>

    <EditItemTemplate>
        <asp:Label ID="lblBillingInfo" runat="server" ></asp:Label>
    </EditItemTemplate>

</asp:TemplateField>



</Columns>
<EditRowStyle BackColor="#999999" />
<FooterStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
<HeaderStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
<PagerStyle BackColor="#284775" ForeColor="White" 
            HorizontalAlign="Center" />
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