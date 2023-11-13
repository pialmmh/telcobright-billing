using System;
using System.Linq;
using System.Web.UI;
using System.Collections.Generic;
using MediationModel;
namespace WebApplication1.Account
{
    public partial class LoginHistory : Page
    {
        private static List<login_history> loginHistories { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                using (PartnerEntities context = new PartnerEntities())
                {
                    loginHistories = context.login_history.OrderByDescending(x => x.login_time).ToList();
                    gvLoginHistory.DataSource = loginHistories;
                    gvLoginHistory.DataBind();
                }
            }
        }
    }

}