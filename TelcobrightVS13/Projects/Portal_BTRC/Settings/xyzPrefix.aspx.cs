using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using MediationModel;

public partial class SettingsXyzprefix : Page
{
    Dictionary<string, string> _dicCountryCodes=new Dictionary<string,string>();
    
    protected void Page_Load(object sender, EventArgs e)
    {
        string tempStr = this.TextBoxSearchCode.Text;

        //load country codes
        using (PartnerEntities context=new PartnerEntities())
        {
            foreach(xyzprefix c in context.xyzprefixes.ToList())
            {
                this._dicCountryCodes.Add(c.Prefix, c.Prefix+" ("+c.Description+")");
                this.Session["dicCountryCodes"] = this._dicCountryCodes;
            }
        }

    }
    protected void ButtonFilter_Click(object sender, EventArgs e)
    {
        this.GridView1.DataBind();
    }
    protected void EntityDataSource1_QueryCreated(object sender, QueryCreatedEventArgs e)
    {
        string startPrefix = this.TextBoxSearchCode.Text;
        if (startPrefix != "")
        {
            var allCountry = e.Query.Cast<xyzprefix>();
            e.Query = from c in allCountry
                      where c.Prefix.StartsWith( startPrefix)
                      select c;
        }
    }
    protected void cvAll_Validate(object source, ServerValidateEventArgs args)
    {
        using (PartnerEntities context = new PartnerEntities())
        {
            string code = ((TextBox) this.FormView1.FindControl("CodeTextBox")).Text;
            if (context.xyzprefixes.Any(c => c.Prefix == code))
            {
                this.cvAll.ErrorMessage = "Duplicate Prefix:" + code + " !";
                args.IsValid = false;
                return;
            }

            string name = ((TextBox) this.FormView1.FindControl("NameTextBox")).Text;

            if (name=="")
            {
                this.cvAll.ErrorMessage = "Description is empty!";
                args.IsValid = false;
                return;
            }
            else if (new[] { @",", @"/", @"`", @"@", @"#", @"$", @"%", @"^", @"*", @";", @"\", "\"" }.Any(name.Contains))
            {
                this.cvAll.ErrorMessage = "Field Partner Name cannot contain characters" +
                    string.Join(" ", new[] { @",", @"/", @"`", @"@", @"#", @"$", @"%", @"^", @"*", @";", @"\", "\"" }) + "!";
                args.IsValid = false;
                return;
            }
        }

    }

    

    protected void GridView1_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            Label lblCountry = (Label)e.Row.FindControl("Label3");
            string thisCountryCode=DataBinder.Eval(e.Row.DataItem, "countrycode").ToString();
            string countryName="";
            this._dicCountryCodes.TryGetValue(thisCountryCode, out countryName);
            lblCountry.Text = countryName;
        }
    }
    protected void FormView1_ModeChanging(object sender, FormViewModeEventArgs e)
    {

    }
    protected void FormView1_ModeChanged(object sender, EventArgs e)
    {
        
    }
    protected void FormView1_DataBound(object sender, EventArgs e)
    {
        if (this.FormView1.CurrentMode == FormViewMode.Insert)
        {
            DropDownList dropDownListCountry = (DropDownList) this.FormView1.FindControl("DropDownListCountry");
            if (this.Session["dicCountryCodes"] != null)
            {
                this._dicCountryCodes = (Dictionary<string, string>) this.Session["dicCountryCodes"];
                foreach (KeyValuePair<string, string> dicItem in this._dicCountryCodes)
                {
                    dropDownListCountry.Items.Add(new ListItem(dicItem.Value,dicItem.Key));
                }
            }
        }
    }
    protected void FormView1_ItemInserted(object sender, FormViewInsertedEventArgs e)
    {
        this.GridView1.DataBind();
    }
}