using System;
using System.Linq;
using System.Web;
using System.Web.UI;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using WebApplication1.Models;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using MySql.Data.MySqlClient;
using System.Configuration;
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