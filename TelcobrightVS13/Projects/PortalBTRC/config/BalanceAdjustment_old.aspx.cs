using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MediationModel;
using TelcobrightMediation;

namespace PortalApp.config
{
    public partial class BalanceAdjustment_old : System.Web.UI.Page
    {
        private static TelcobrightConfig Tbc { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                Tbc = PageUtil.GetTelcobrightConfig();
                using (PartnerEntities context = new PartnerEntities())
                {
                    List<partner> allPartners = context.partners.OrderBy(i => i.PartnerName).ToList();
                    ddlistPartner.DataSource = allPartners;
                    ddlistPartner.DataBind();
                }
                txtDate.Text = DateTime.Now.ToString("yyyy-MM-dd");
                ddlistPartner_OnSelectedIndexChanged(ddlistPartner, EventArgs.Empty);
            }
        }

        protected void ddlistPartner_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            int idPartner = Convert.ToInt32(ddlistPartner.SelectedValue);
            using (PartnerEntities context = new PartnerEntities())
            {
                List<KeyValuePair<Regex, string>> serviceAliases = Tbc.ServiceAliasesRegex;
                List<account> accounts = context.accounts.Where(p => p.idPartner == idPartner && p.isBillable == 1 && p.isCustomerAccount == 1).ToList();
                foreach (account account in accounts)
                {
                    foreach (var kv in serviceAliases)
                    {
                        var regex = kv.Key;
                        if (regex.Matches(account.accountName).Count > 0)
                        {
                            account.accountName = kv.Value;
                            break;
                        }
                    }
                }
                ddlistServiceAccount.DataSource = accounts;
                ddlistServiceAccount.DataBind();
            }
        }
    }
}