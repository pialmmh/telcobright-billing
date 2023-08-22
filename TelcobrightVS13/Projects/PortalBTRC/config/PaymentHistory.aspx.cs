using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using PortalApp._myCodes;

namespace PortalApp.config
{
   
    public partial class PaymentHistory : System.Web.UI.Page
    {

        Dictionary<string, string> _partners = new Dictionary<string, string>()
     {

        {"0","Select Partner" },
        {"1", "Onetel"},
        {"2", "Carrier-2"},
        {"3", "GP"}
     };
      

        PaymentHistoryInfo py = new PaymentHistoryInfo();
       

        protected void Page_Load(object sender, EventArgs e)
        {

            if (!IsPostBack)
            {
                ddlPartner.DataSource = _partners;
                ddlPartner.DataTextField = "value";
                ddlPartner.DataValueField = "key";
                ddlPartner.DataBind();
                string value = Request.QueryString["id"];
                populatrGrid(value);
            }
            }
         public void populatrGrid(string value)
        {
            List<PaymentHistoryInfo> pylist = py.populatePaymentHistory();
            if (value != null||value=="0")
            {

                GridView1.DataSource = pylist.Where(p => p.PartnerID == Int32.Parse(value));

            }
            else
            {
                GridView1.DataSource = py.populatePaymentHistory();

            }
     
            GridView1.DataBind();
        }
        protected void showBtn_Click(object sender, EventArgs e)
        {
            string value = ddlPartner.SelectedItem.Value;
            populatrGrid(value);
        }

      
    }

     
    }

       
            

    
    
    
