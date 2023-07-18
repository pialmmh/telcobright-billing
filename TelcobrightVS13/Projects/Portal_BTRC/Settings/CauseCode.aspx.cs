using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.UI.WebControls;
using MediationModel;
//using telcobrightmediationModel;

public partial class SettingsCauseCode : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!this.IsPostBack)
        {
            string partnerConStr = ConfigurationManager.ConnectionStrings["reader"].ConnectionString;
            int posDatabase = partnerConStr.IndexOf("database");
            //make sure to keep databasename at the last of the connection string
            string dbName = partnerConStr.Substring(posDatabase + 9, partnerConStr.Length - posDatabase - 9);
            //find TB customerid
            using (PartnerEntities context = new PartnerEntities())
            {
                int idOperator = context.telcobrightpartners.Where(c => c.databasename == dbName).First().idCustomer;
                foreach (ne thisSwitch in context.nes.Where(c => c.idCustomer == idOperator).ToList())
                {
                    this.DropDownListSwitchSelect.Items.Add(new ListItem(thisSwitch.SwitchName, thisSwitch.idSwitch.ToString()));
                }
            }

            this.DropDownListSwitchSelect.SelectedIndex = 0;
        }
    }
    protected void ButtonFilter_Click(object sender, EventArgs e)
    {
        this.GridView1.DataBind();
    }


    protected void EntityDataSource1_QueryCreated(object sender, QueryCreatedEventArgs e)
    {
        string partnerConStr = ConfigurationManager.ConnectionStrings["reader"].ConnectionString;
        int posDatabase = partnerConStr.IndexOf("database");
        //make sure to keep databasename at the last of the connection string
        string dbName = partnerConStr.Substring(posDatabase + 9, partnerConStr.Length - posDatabase - 9);
        //find TB customerid
        using (PartnerEntities context = new PartnerEntities())
        {
            int idOperator = context.telcobrightpartners.Where(c => c.databasename == dbName).First().idCustomer;
            
            List<int> lstSwitchId = context.nes.Where(c => c.idCustomer == idOperator).Select(c => c.idSwitch).ToList();
            //e.Query = AllSwitches.Where(c => c.idCustomer == idOperator);

            var allCauseCodes = e.Query.Cast<causecode>();
            if (this.DropDownListSwitchSelect.SelectedIndex == 0)
            {
                e.Query = allCauseCodes.Where(c => lstSwitchId.Contains(c.idSwitch)).OrderBy(c => c.idSwitch).OrderBy(c => c.CC);
            }
            else
            {
                int idSwitch = int.Parse(this.DropDownListSwitchSelect.SelectedValue);
                e.Query = allCauseCodes.Where(c => c.idSwitch==idSwitch).OrderBy(c => c.idSwitch).OrderBy(c => c.CC);
            }
        }
        
            
        
    }
    protected void cvAll_Validate(object source, ServerValidateEventArgs args)
    {
        using (PartnerEntities context = new PartnerEntities())
        {
            string code = ((TextBox) this.FormView1.FindControl("CodeTextBox")).Text;
            int intCc = -1;
            if (code == "")
            {
                this.cvAll.ErrorMessage = "Cause Code No is empty !";
                args.IsValid = false;
                return;
            }
            else if (new[] { @",", @"/", @"`", @"@", @"#", @"$", @"%", @"^", @"*", @";", @"\", "\"" }.Any(code.Contains))
            {
                this.cvAll.ErrorMessage = "Field Partner Name cannot contain characters" +
                    string.Join(" ", new[] { @",", @"/", @"`", @"@", @"#", @"$", @"%", @"^", @"*", @";", @"\", "\"" }) + "!";
                args.IsValid = false;
                return;
            }
            else
            {
                int.TryParse(code, out intCc);
                if (intCc == -1)
                {
                    this.cvAll.ErrorMessage = "Invalid Cause Code !";
                    args.IsValid = false;
                    return;
                }
            }

            int idSwitch = -1;
            int.TryParse(((DropDownList) this.FormView1.FindControl("DropDownList2")).SelectedValue, out idSwitch);
            if (idSwitch == -1)
            {
                this.cvAll.ErrorMessage = "Invalid SwitchId !";
                args.IsValid = false;
                return;
            }
            

            if (context.causecodes.Where(c=>c.idSwitch==idSwitch&&c.CC==intCc).Any())
            {
                this.cvAll.ErrorMessage = "Duplicate Cause Code:" + code +
                    " for same switchid:" +  idSwitch +  " !";
                args.IsValid = false;
                return;
            }

        }

    }



    protected void EntityDataSourceSwitch_QueryCreated(object sender, QueryCreatedEventArgs e)
    {
        string partnerConStr = ConfigurationManager.ConnectionStrings["reader"].ConnectionString;
        int posDatabase = partnerConStr.IndexOf("database");
        //make sure to keep databasename at the last of the connection string
        string dbName = partnerConStr.Substring(posDatabase + 9, partnerConStr.Length - posDatabase - 9);
        //find TB customerid
        using (PartnerEntities context = new PartnerEntities())
        {
            int idOperator = context.telcobrightpartners.Where(c => c.databasename == dbName).First().idCustomer;
            var allSwitches = e.Query.Cast<ne>();
            e.Query = allSwitches.Where(c => c.idCustomer == idOperator);
        }
    }
    protected void FormView1_ItemInserted(object sender, FormViewInsertedEventArgs e)
    {
        this.GridView1.DataBind();
    }
   
}