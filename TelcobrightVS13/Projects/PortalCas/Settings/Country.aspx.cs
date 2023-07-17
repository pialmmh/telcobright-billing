using System;
using System.Linq;
using System.Web.UI.WebControls;
using MediationModel;

public partial class SettingsCountry : System.Web.UI.Page
{

    protected void Page_Load(object sender, EventArgs e)
    {
        string tempStr = this.TextBoxSearchCode.Text;
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
            var allCountry = e.Query.Cast<countrycode>();
            e.Query = from c in allCountry
                      where c.Code.StartsWith( startPrefix)
                      select c;
        }
    }
    protected void cvAll_Validate(object source, ServerValidateEventArgs args)
    {
        using (PartnerEntities context = new PartnerEntities())
        {
            string code = ((TextBox) this.FormView1.FindControl("CodeTextBox")).Text;
            if (context.countrycodes.Any(c => c.Code == code))
            {
                this.cvAll.ErrorMessage = "Duplicate Country Code:" + code + " !";
                args.IsValid = false;
                return;
            }

            string name = ((TextBox) this.FormView1.FindControl("NameTextBox")).Text;

            if (name=="")
            {
                this.cvAll.ErrorMessage = "Country Name is empty!";
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
            if (context.countrycodes.Any(c => c.Name == name))
            {
                this.cvAll.ErrorMessage = "Duplicate Country Name:" + name + " !";
                args.IsValid = false;
                return;
            }
        }

    }



    protected void FormView1_ItemInserted1(object sender, FormViewInsertedEventArgs e)
    {
        this.GridView1.DataBind();
    }
}