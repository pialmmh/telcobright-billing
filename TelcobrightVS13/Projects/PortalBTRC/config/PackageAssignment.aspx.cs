using System;
using System.Web.UI.WebControls;

namespace PortalApp.config
{
    public partial class PackageAssignment : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            this.FormView1.DefaultMode = FormViewMode.Insert;
        }

        protected void FormView1_ItemInserting(object sender, FormViewInsertEventArgs e)
        {

        }
    }
}