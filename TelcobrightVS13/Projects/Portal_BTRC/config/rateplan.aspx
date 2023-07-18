<%@ Page Title="" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="True" CodeBehind="rateplan.aspx.cs" Inherits="ConfigSupplierRatePlan" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" Runat="Server">

          
    <asp:EntityDataSource ID="EntityDataSupplierRatePlan" runat="server" 
            ConnectionString="name=PartnerEntities" DefaultContainerName="PartnerEntities" 
            EnableDelete="True" EnableFlattening="False" EnableInsert="True" 
            EnableUpdate="True" EntitySetName="rateplans" 
            onquerycreated="EntityDataSupplierRatePlan_QueryCreated" 
            EntityTypeFilter="rateplan" oninserting="EntityDataSupplierRatePlan_Inserting"  
            >
        </asp:EntityDataSource>

        <asp:SqlDataSource ID="SqlDataTimeZone" runat="server" 
            ConnectionString="<%$ ConnectionStrings:Partner %>" 
            ProviderName="<%$ ConnectionStrings:Partner.ProviderName %>" 
            SelectCommand="select t.id,concat(c.country_name,' ',t.offsetdesc,' [',z.zone_name,']') as Name
from timezone t
join zone z
using(zone_id)
join country c
using(country_code)
order by c.country_name"></asp:SqlDataSource>

<asp:SqlDataSource ID="SqlDataCurrency" runat="server" 
                   ConnectionString="<%$ ConnectionStrings:Partner %>" 
                   ProviderName="<%$ ConnectionStrings:Partner.ProviderName %>" 
                   SelectCommand="SELECT uom_id as id FROM uom
                                  where uom_type_id='CURRENCY_MEASURE'"
></asp:SqlDataSource>

 <%--   </div>
<span style="color:Black;"><b>Rate Plans</b></span>
<br />--%>

<asp:SqlDataSource ID="SqlDataservices1" runat="server" 
        ConnectionString="<%$ ConnectionStrings:Reader %>" 
        ProviderName="<%$ ConnectionStrings:Reader.ProviderName %>" 
        SelectCommand="select (select -1) as id,(select '[ All]') as  servicename 
union all 
(select sf.id, concat(servicename,' [',sc.type,']') as servicename from enumservicefamily sf
 left join enumservicecategory sc
 on sf.servicecategory=sc.id)                 
    "></asp:SqlDataSource>

 Service: 

            <asp:DropDownList ID="ddlistSupplierRatePlanType" runat="server" 
                Enabled="true" AutoPostBack="false" 
                DataSourceID="SqlDataservices1" DataTextField="servicename" 
                DataValueField="id">
            </asp:DropDownList>


Rate Plan Name Having: <asp:TextBox ID="TextBoxRatePlanName" runat="server"></asp:TextBox>
    <asp:Button ID="ButtonFind" runat="server" Text="Find" Width="100px" 
        onclick="ButtonFind_Click" />


        <br />
<asp:LinkButton ID="LinkButton1" runat="server" Text="Add New Rate Plan" 
        onclick="LinkButton1_Click" Font-Size="Smaller">Add New Rate Plan</asp:LinkButton>


    

    <asp:ValidationSummary ID="ValidatorSummary" runat="server"
    ValidationGroup="allcontrols" ForeColor="Red" />



    <asp:ToolkitScriptManager ID="ToolkitScriptManager1" runat="server" />
   
    <asp:SqlDataSource ID="SqlDataServiceType" runat="server" 
    ConnectionString="<%$ ConnectionStrings:Partner %>" 
    ProviderName="<%$ ConnectionStrings:Partner.ProviderName %>" 
    SelectCommand="select * from enumservicecategory"></asp:SqlDataSource>
    <asp:SqlDataSource ID="SqlDataSubServiceType" runat="server" 
        ConnectionString="<%$ ConnectionStrings:Partner %>" 
        ProviderName="<%$ ConnectionStrings:Partner.ProviderName %>" 
        SelectCommand="select * from enumservicesubcategory"></asp:SqlDataSource>

<asp:SqlDataSource ID="SqlDataservices" runat="server" 
        ConnectionString="<%$ ConnectionStrings:Reader %>" 
        ProviderName="<%$ ConnectionStrings:Reader.ProviderName %>" 
        SelectCommand="select (select -1) as id,(select '[ Select]') as  servicename 
union all 
(select sf.id, concat(servicename,' [',sc.type,']') as servicename from enumservicefamily sf
 left join enumservicecategory sc
 on sf.servicecategory=sc.id) "></asp:SqlDataSource>
    
<asp:SqlDataSource ID="SqlDataBillingSpan" runat="server" 
        ConnectionString="<%$ ConnectionStrings:Reader %>" 
        ProviderName="<%$ ConnectionStrings:Reader.ProviderName %>" 
        SelectCommand="select uom_id as id,abbreviation as type from uom
                       where uom_type_id='TIME_FREQ_MEASURE' "></asp:SqlDataSource>
    

    <asp:FormView ID="frmSupplierRatePlanInsert" runat="server" DataKeyNames="id" 
        Width="800px" DefaultMode="Insert" 
        Height="96px" CellPadding="4" Font-Size="Small" 
        Visible="False" oniteminserted="frmSupplierRatePlanInsert_ItemInserted" 
        onitemcreated="frmSupplierRatePlanInsert_ItemCreated" 
        onload="frmSupplierRatePlanInsert_Load" 
        oniteminserting="frmSupplierRatePlanInsert_ItemInserting" 
        ForeColor="#333333" ondatabound="frmSupplierRatePlanInsert_DataBound" 
        onmodechanged="frmSupplierRatePlanInsert_ModeChanged" OnItemCommand="frmSupplierRatePlanInsert_ItemCommand" >
       
        

        <%--<EditRowStyle BackColor="#E5E4E2" />--%>
       <EditRowStyle BackColor="#f2f2f2" />
       


        <FooterStyle BackColor="#5D7B9D" ForeColor="White" Font-Bold="True" />
        <HeaderStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
       


        <InsertItemTemplate>
          

          <table border="0" width="1000px">
            <tr>

            <td style="min-width:500px">
                
                <div style="text-align:right;">

                <b> Service Family:</b>
                 <asp:DropDownList ID="DropDownListType" runat="server" SelectedValue='<%# Bind("Type") %>'
                        Enabled="true" AutoPostBack="false" OnSelectedIndexChanged="DropDownListType_SelectionChanged"
                        DataSourceID="SqlDataservices" DataTextField="servicename" DataValueField="id">
                        <%--<asp:ListItem Value="1">Transit</asp:ListItem>
                        <asp:ListItem Value="3">Intl. Incoming</asp:ListItem>
                        <asp:ListItem Value="4">Intl. Outgoing</asp:ListItem>--%>
                    </asp:DropDownList>
                <br />

                <b> Rateplan Name:</b>
                <asp:TextBox ID="RatePlanNameTextBox" runat="server" 
                    Text='<%# Bind("RatePlanName") %>' />
                <br />

                

                <%--<span style="padding-left:53px;"></span>--%>
                
                Description:
                <asp:TextBox ID="DescriptionTextBox" runat="server" Text="" />
                <br />

                    <asp:TextBox ID="TextBoxIdPartner" runat="server" Visible="false" 
                    Text="" />
            
            
            </div>

            </td>

            <td style="width:400px;padding-right:270px;"> <%--first column--%>
                
                <div style="text-align:right;margin-top:0px;">
                
                    
                <b>Created On:</b>
                <br />
                Date:<asp:TextBox ID="ActiveDateTextBox" runat="server" 
                    Text="" />
                <asp:CalendarExtender ID="CalendarStartDateFrm" runat="server" 
                                    TargetControlID="ActiveDateTextBox"  PopupButtonID="ActiveDateTextBox" Format="yyyy-MM-dd">
                  </asp:CalendarExtender>     
                  <br />
                  Time:<asp:TextBox ID="TextBoxTime" runat="server" Text="00:00:00"></asp:TextBox>
                <br />
                
                
                
                    
            </div>

            </td>
            
            
          
            </tr>
            <tr>
                <td>
                  
               
                </td>
            </tr>
            
           </table>
           
            <div style="padding-left:115px;">
                        <b> Time Zone:</b>
                        <asp:DropDownList ID="ddlistTimeZone" runat="server" AutoPostBack="false" DataSourceID="SqlDataTimeZone" 
                        DataTextField="Name" DataValueField="id"
                        Enabled="True" Width="538px"
                        >
                        </asp:DropDownList>
                        <br />
               
                
                    
        <br />
        <span style="margin-left:-70px;"> Defaults for Rates: [Can be specified for each Rate] </span>  <br />

        <div style="margin-left:-70px;width:900px;overflow:visible;margin-top:10px;background-color:#F7F6F3;height:100px;padding:5px;">
            
            <div style="float:left;text-align:right;">
                
                <span style="padding-right:0px;padding-top:10px;">
            <b>Currency:</b>
                <asp:DropDownList ID="DropDownListCurrency" runat="server" 
                        Enabled="true" >
                        
                        </asp:DropDownList>
                
                
            </span>
                <br />

                <b>Billing Span</b> 
                      <asp:DropDownList ID="DropDownListBillingSpan" runat="server" AutoPostBack="false" 
                            DataSourceID="SqlDataBillingSpan" DataTextField="Type" DataValueField="id" 
                            Enabled="true" >
                        </asp:DropDownList>
                        <br />
                
                
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
                       <b> Pulse:</b>
               <asp:TextBox ID="txtResolution" Enabled="True" runat="server" Text="1"></asp:TextBox>
               <br />
               
                 
                </div>
                <div style="float:left;min-width:400px;text-align:right;margin-left:50px;">
                    
                    <span style="margin-right:105px;">
                        <b> Fixed Initial Period (Sec):</b> 
                        <asp:TextBox ID="txtSurchargeTime" visible="true" runat="server" Text="0"></asp:TextBox>
                    </span>
                    <br />
                    <span style="margin-right:105px;">
                    <b> Charge for Fixed Initial Period:</b> 
                    <asp:TextBox ID="txtSurchargeAmount" visible="true" runat="server" Text="0.0"></asp:TextBox>
                    </span>
                        <br />
                    <span style="margin-right:105px;">
                    <b> Minimum Duration (Sec):</b>
               <asp:TextBox ID="txtMinDurationSec" Enabled="True" runat="server" Text="0"></asp:TextBox>
               </span>
                        <br />
                    
                    <b> Round Digits after Decimal for Rate Amount:</b>
                        <asp:TextBox ID="TextBoxRoundUp" Enabled="True" runat="server" Text="0"></asp:TextBox>
                    
                    <asp:Label ID="Label1" runat="server" Text="[0=No Rounding]"></asp:Label>
                    <br />
                     <span style="margin-right:106px;">
                        <b> Ambiguous Date Handling:</b> 
                        <asp:DropDownList ID="DropDownListAmbiguous" AppendDataBoundItems="true" runat="server" Width="173px">
                             <asp:ListItem Text="Month First, Then Day" Value="MF" />
                             <asp:ListItem Text="Day First, Then Month" Value="DF" />
                             </asp:DropDownList>
                    </span>
                    <br />
                    <span style="margin-right:110px;">
                        <b> Reference Rateplan for LCR:</b> 
                        <asp:DropDownList ID="DropDownListLcrRef" AppendDataBoundItems="true" runat="server" Width="173px">
                             <asp:ListItem Text="No" Value="0" />
                             <asp:ListItem Text="Yes" Value="1" />
                             </asp:DropDownList>
                    </span>
                </div>
            
            </div>



                <br />




                <%--Previous Codes Validity Ends:
                Date:<asp:TextBox ID="TextBoxPrevious" runat="server" 
                    Text="" />
                <asp:CalendarExtender ID="CalendarExtenderPrevious" runat="server" 
                                    TargetControlID="TextBoxPrevious"  PopupButtonID="TextBoxPrevious" Format="dd-MM-yyyy">
                  </asp:CalendarExtender>     
                
                Time:<asp:TextBox ID="TextBoxPreviousTime" runat="server" Text="23:59:59"></asp:TextBox>--%>


                    </div>
            
            <div style="clear:both;"></div>

            <asp:LinkButton ID="InsertButton" runat="server" CausesValidation="True" 
                CommandName="Insert" Text="Insert" ValidationGroup="allcontrols"/>

&nbsp;<asp:LinkButton ID="InsertCancelButton" runat="server" CausesValidation="False" 
                CommandName="Cancel" Text="Cancel" OnClick="FormViewCancel_Click" CommandArgument="0"/>
        </InsertItemTemplate>
       
        <PagerStyle ForeColor="White" HorizontalAlign="Center" BackColor="#284775" />
        <RowStyle BackColor="white" ForeColor="#333333" />

        
       
    </asp:FormView>



    <asp:CustomValidator ID="cvAll" runat="server" 
    Display="Dynamic"
    ErrorMessage="This membership"
    OnServerValidate="cvAll_Validate" 
    ValidationGroup="allcontrols"
    Text=""></asp:CustomValidator>

    
    <br />
       
    
    
    <asp:Label ID="lblIdSupplierRatePlanGlobal" runat="server" Text="" Visible="false"></asp:Label>

    <asp:SqlDataSource ID="SqlDataSupplierRatePlanType" runat="server" 
        ConnectionString="<%$ ConnectionStrings:Partner %>" 
        ProviderName="<%$ ConnectionStrings:Partner.ProviderName %>" 
        SelectCommand="select (select -1) as id,(select ' [All]') as Type
union all					
select id as id,Type from enumrateplantype
order by id"

        ></asp:SqlDataSource>
        <%--SelectCommand="select (select -1) as id,(select ' [Select]') as Type
union all					
select idpartner as id,partnername as Type from partner
order by Type "--%>

<asp:SqlDataSource ID="SqlDataFrmDdListIgwOperator" runat="server" 
        ConnectionString="<%$ ConnectionStrings:Partner %>" 
        ProviderName="<%$ ConnectionStrings:Partner.ProviderName %>" 
        SelectCommand="select (select -1) as id,(select ' [Select]') as Type
union all					
select idpartner as id,partnername as Type from partner where partnertype=3
order by Type "
        ></asp:SqlDataSource>

<asp:SqlDataSource ID="SqlDataFrmDdListICXOperator" runat="server" 
        ConnectionString="<%$ ConnectionStrings:Partner %>" 
        ProviderName="<%$ ConnectionStrings:Partner.ProviderName %>" 
        SelectCommand="select (select -1) as id,(select ' [Select]') as Type
union all					
select idpartner as id,partnername as Type from partner 
order by Type "
        ></asp:SqlDataSource>

    <br />
    
   <div style="width:1800px;overflow-x:scroll;">
        <asp:GridView ID="GridViewSupplierRatePlan" 
            runat="server" AllowPaging="True" 
        AllowSorting="True" AutoGenerateColumns="False" CellPadding="4" 
        DataKeyNames="id" DataSourceID="EntityDataSupplierRatePlan" ForeColor="#333333" 
        GridLines="Vertical" Font-Size="9pt" PageSize="30" 
        onrowdatabound="GridViewSupplierRatePlan_RowDataBound" 
        onrowcommand="GridViewSupplierRatePlan_RowCommand" 
        onrowediting="GridViewSupplierRatePlan_RowEditing" 
        onrowupdating="GridViewSupplierRatePlan_RowUpdating" 
        onrowdeleting="GridViewSupplierRatePlan_RowDeleting" BorderColor="#CCCCCC" BorderStyle="Solid">
        <AlternatingRowStyle BackColor="#f2f2f2" ForeColor="#284775" />
        <PagerStyle  HorizontalAlign ="Center" />
        <Columns>
            <%--onClientClick="window.open('SupplierRatePlanDetail.aspx?idSupplierRatePlan=3')"--%>
             <%--<asp:CommandField ShowSelectButton="False" ShowEditButton="false" />--%>
            
             <asp:TemplateField>
                <ItemTemplate>
                    <asp:LinkButton ID="LinkButtonEdit" ButtonType="Button" CommandName="Edit"  runat="server">Edit</asp:LinkButton>
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:LinkButton ID="LinkButtonEdit" ButtonType="Button" CommandName="Update"  runat="server" CausesValidation="false" ValidationGroup="allcontrols">Update</asp:LinkButton>
                    <asp:LinkButton ID="LinkButtonCancel" ButtonType="Button" CommandName="Cancel"  runat="server">Cancel</asp:LinkButton>
                </EditItemTemplate>
            </asp:TemplateField>
          

              

            <asp:TemplateField> <%--delete--%>
                <ItemTemplate>
                    
                    <asp:LinkButton ID="LinkButton2"  runat="server" ButtonType="Button" CommandName="Delete"
                        Text="Delete" HeaderText="" CausesValidation="false" ValidationGroup="allcontrols"
                    OnClientClick='return confirm("Are you sure you want to delete this entry?");'> 
                    </asp:LinkButton>
                    <asp:Label ID="lblIdSupplierRatePlan" runat="server" Text='<%# Eval("id") %>'  Visible="false"></asp:Label>
                </ItemTemplate>
            </asp:TemplateField>

            <asp:TemplateField>
                <ItemTemplate>
                   
                   
                    <asp:LinkButton ID="LinkButtonTask" runat="server">Task</asp:LinkButton>
                    
                </ItemTemplate>
            </asp:TemplateField>

             <asp:TemplateField>
                <ItemTemplate>
                   
                   
                    <asp:LinkButton ID="LinkButtonRate" runat="server">Rates</asp:LinkButton>
                    
                </ItemTemplate>
            </asp:TemplateField>
            


            <asp:BoundField DataField="RatePlanName" HeaderText="Plan Name" ItemStyle-Wrap="false"
                SortExpression="RatePlanName" />
            

             <asp:TemplateField HeaderText="Service Family" SortExpression="Type">
                
                <ItemTemplate>
                    <%--<asp:Label ID="lblType" runat="server" Text="Type"></asp:Label>--%>
                    <asp:DropDownList ID="DropDownListType" runat="server" SelectedValue='<%# Bind("Type") %>'
                        Enabled="false" 
                        DataSourceID="SqlDataservices" DataTextField="servicename" DataValueField="id">
                        
                    </asp:DropDownList>
                </ItemTemplate>
                
                <EditItemTemplate>
                    <asp:DropDownList ID="DropDownListType" runat="server" SelectedValue='<%# Bind("Type") %>'
                        Enabled="false" 
                        DataSourceID="SqlDataservices" DataTextField="servicename" DataValueField="id">
                        
                    </asp:DropDownList>
                </EditItemTemplate>
                
            </asp:TemplateField>


             <asp:TemplateField HeaderText="Currency" SortExpression="Currency">
                
                <ItemTemplate>
                    <asp:Label ID="lblCurrency" runat="server" Text=""></asp:Label>
                </ItemTemplate>
                
                <EditItemTemplate>
                    <asp:DropDownList ID="DropDownListCurrency" runat="server" SelectedValue='<%# Bind("Currency") %>'
                    DataSourceID="SqlDataCurrency" DataTextField="id" DataValueField="id" Enabled="false">
                        
                    </asp:DropDownList>
                </EditItemTemplate>
                
            </asp:TemplateField>



            <%--<asp:CommandField ShowDeleteButton="false" />--%>
            <asp:BoundField DataField="id" HeaderText="idS" ReadOnly="True" 
                SortExpression="id" visible="false"/>
            
            
            <asp:TemplateField HeaderText="TimeZone" SortExpression="TimeZone" ItemStyle-Wrap="false">
                
                <ItemTemplate>
                    <asp:Label ID="lblTimeZone" runat="server" Text=""></asp:Label>
                </ItemTemplate>
                
                <EditItemTemplate>
                    
                    <asp:DropDownList ID="ddlistTimeZone" runat="server" AutoPostBack="True" 
                        DataTextField="Name" DataValueField="id"
                        Enabled="True" DataSourceID="SqlDataTimeZone" SelectedValue='<%# Bind("TimeZone") %>'
                        >
                        </asp:DropDownList>
                </EditItemTemplate>
                
            </asp:TemplateField>



            <asp:TemplateField HeaderText="Created On" SortExpression="date1" ItemStyle-Wrap="false">
                
                <ItemTemplate>
                    <asp:Label ID="lbldate1" runat="server" Text='<%# Convert.ToDateTime(Eval("date1")).ToString("yyyy-MM-dd HH:mm:ss") %>' ></asp:Label>
                </ItemTemplate>
                
                <EditItemTemplate>
                    <asp:TextBox ID="ActiveDateTextBox" runat="server" DataFormatString="{0:dd-MM-yyyy}"
                    enabled="false" Text='<%# Convert.ToDateTime(Eval("date1")).ToString("dd-MM-yyyy") %>' />
                <asp:CalendarExtender ID="CalendarStartDateFrm" runat="server" 
                                    TargetControlID="ActiveDateTextBox"  PopupButtonID="ActiveDateTextBox" Format="dd-MM-yyyy">
                  </asp:CalendarExtender>     
                  <asp:TextBox ID="TextBoxTime" Enabled="false" runat="server" Text='<%# Convert.ToDateTime(Eval("date1")).ToString("HH:mm:ss") %>'></asp:TextBox>
                </EditItemTemplate>
                
            </asp:TemplateField>




            <%--<asp:BoundField DataField="date1" HeaderText="Activation Time" 
                SortExpression="date1" visible="True" DataFormatString="{0:dd-MM-yyyy HH:mm:ss}" />--%>
            <asp:BoundField DataField="field4" HeaderText="Tech Prefix" SortExpression="field4" visible="true" />
            <asp:BoundField DataField="Description" HeaderText="Description" ItemStyle-Wrap="false" 
                SortExpression="Description" visible="true" />
            

           
            <asp:TemplateField HeaderText="Default Pulse" SortExpression="Resolution" ControlStyle-Width="30px" >
                <ItemTemplate>
                    <asp:Label ID="lblResolution" runat="server" DataFormatString="{0:#,0.#00000}" Text='<%# Eval("Resolution")!=null?Eval("Resolution").ToString():"0" %>'></asp:Label>
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:TextBox ID="txtResolution" Enabled="True" runat="server" Text='<%# Bind("Resolution") %>'></asp:TextBox>
                </EditItemTemplate>

<ControlStyle Width="30px"></ControlStyle>
            </asp:TemplateField>

              <asp:TemplateField HeaderText="Default Round Digits after Decimal for Rate Amount" SortExpression="RateAmountRoundupDecimal" ControlStyle-Width="120" >
                <ItemTemplate>
                    <asp:Label ID="lblRoundUp" runat="server" Text='<%# Eval("RateAmountRoundupDecimal")!=null?Eval("RateAmountRoundupDecimal").ToString():"" %>'></asp:Label>
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:TextBox ID="txtRoundUp" Enabled="True" runat="server" Text='<%# Bind("RateAmountRoundupDecimal") %>'></asp:TextBox>
                </EditItemTemplate>


            </asp:TemplateField>

            <asp:TemplateField HeaderText="Min Duration (Sec)" SortExpression="mindurationsec" ControlStyle-Width="30px" >
                <ItemTemplate>
                    <asp:Label ID="lblMinDurationSec" runat="server" Text='<%#Eval("mindurationSec")!=null?Eval("MinDurationSec").ToString():"" %>'></asp:Label>
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:TextBox ID="txtMinDurationSec" Enabled="True" runat="server" Text='<%# Bind("mindurationsec") %>'></asp:TextBox>
                </EditItemTemplate>

<ControlStyle Width="30px"></ControlStyle>
            </asp:TemplateField>
            
            
            <asp:TemplateField HeaderText="Reference Rateplan for LCR" SortExpression="field3" >
                <ItemTemplate>
                    <%--if DF then Day first, else MF, Months First is the default--%>
                    <asp:Label ID="lblRefRatePlan" runat="server" Text='<%# Eval("field3")==null?"No":(Eval("field3").ToString()=="0"?"No":"Yes") %>'></asp:Label>
                </ItemTemplate>
                <EditItemTemplate>
                     <asp:DropDownList ID="DropDownListLcrRef" AppendDataBoundItems="true" runat="server" Width="173px"
                         SelectedValue='<%# Bind("field3")%>'>
                             <asp:ListItem Text="No" Value="0" />
                             <asp:ListItem Text="Yes" Value="1" />
                             </asp:DropDownList>
                </EditItemTemplate>

            </asp:TemplateField>

             <asp:TemplateField HeaderText="Ambiguous Date Handling by" SortExpression="field5" ControlStyle-Width="175px" >
                <ItemTemplate>
                    <%--if DF then Day first, else MF, Months First is the default--%>
                    <asp:Label ID="lblAmbiguousDate" runat="server" Text='<%# Eval("field5")==null?"Month First, Then Day":(Eval("field5").ToString()=="DF"?"Day First, Then Month":"Month First, Then Day") %>'></asp:Label>
                </ItemTemplate>
                <EditItemTemplate>
                     <asp:DropDownList ID="DropDownListAmbiguousDate" AppendDataBoundItems="true" runat="server" Width="173px"
                         SelectedValue='<%# Bind("field5")%>'>
                         <asp:ListItem Text="Month First, Then Day" Value="MF" />
                             <asp:ListItem Text="Day First, Then Month" Value="DF" />
                             </asp:DropDownList>
                </EditItemTemplate>

            </asp:TemplateField>

            
           <asp:TemplateField HeaderText="Default Fixed Charge Duration (Sec)" SortExpression="SurchargeTime" ControlStyle-Width="120px" >
                <ItemTemplate>
                    <asp:Label ID="lblSurchargeTime" runat="server" Text='<%# Eval("SurchargeTime").ToString() %>'></asp:Label>
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:TextBox ID="txtSurchargeTime" Enabled="True" runat="server" Text='<%# Bind("SurchargeTime")%>'></asp:TextBox>
                </EditItemTemplate>

            </asp:TemplateField>

            <asp:TemplateField HeaderText="Default Fixed Charge Amount" SortExpression="SurchargeAmount" ControlStyle-Width="120px" >
                <ItemTemplate>
                    <asp:Label ID="lblSurchargeAmount" runat="server" Text='<%# Convert.ToDecimal(Eval("SurchargeAmount").ToString()).ToString("0.#00000") %>'></asp:Label>
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:TextBox ID="txtSurchargeAmount" Enabled="True" runat="server" Text='<%# Bind("SurchargeAmount") %>'></asp:TextBox>
                </EditItemTemplate>


            </asp:TemplateField>

             <asp:TemplateField HeaderText="Default Billing Span" SortExpression="billingspan">
                
                <ItemTemplate>
                  <asp:DropDownList ID="DropDownListBillingSpan" runat="server" AutoPostBack="false" 
                        DataSourceID="SqlDataBillingSpan" DataTextField="Type" DataValueField="id" SelectedValue='<%# Bind("BillingSpan") %>'
                        Enabled="false" >
                    </asp:DropDownList>
                    
                </ItemTemplate>
                
                <EditItemTemplate>
                    <asp:DropDownList ID="DropDownListBillingSpan" runat="server" AutoPostBack="false" 
                        DataSourceID="SqlDataBillingSpan" DataTextField="Type" DataValueField="id" SelectedValue='<%# Bind("BillingSpan") %>'
                        Enabled="True" >
                    </asp:DropDownList>
                    
                    
                </EditItemTemplate>
            </asp:TemplateField>


  <asp:TemplateField HeaderText="Default Category" SortExpression="Category">
                
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


             <asp:TemplateField HeaderText="Default Sub Category" SortExpression="SubCategory" ControlStyle-Width="100px">
                
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

        </Columns>

      
        <EditRowStyle BackColor="#999999" />

      
        <FooterStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
        <HeaderStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
        <PagerStyle BackColor="#284775" ForeColor="White" 
            HorizontalAlign="Center" />
        <RowStyle BackColor="white" ForeColor="#333333" />
        <SelectedRowStyle BackColor="#E2DED6" ForeColor="#333333" Font-Bold="True" />
        <SortedAscendingCellStyle BackColor="#E9E7E2" />
        <SortedAscendingHeaderStyle BackColor="#506C8C" />
        <SortedDescendingCellStyle BackColor="#FFFDF8" />
        <SortedDescendingHeaderStyle BackColor="#6F8DAE" />
    </asp:GridView>

    </div>

    
    
    
        


    

    
    
   

</asp:Content>

