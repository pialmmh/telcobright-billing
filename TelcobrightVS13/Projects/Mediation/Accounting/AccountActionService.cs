using MediationModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TelcobrightMediation.Accounting
{
    public class AccountActionService
    {
        private List<IAutomationAction> AccountAutomationActions { get; set; }
        private List<partner> AllPartners { get; set; }
        private TelcobrightConfig Tbc { get; set; }

        public AccountActionService(TelcobrightConfig tbc)
        {
            this.Tbc = tbc;
            // TODO: Replace 4 with correct variable
            AccountAutomationActions = tbc.CdrSetting.ServiceGroupConfigurations[4].AccountActions;
            using (PartnerEntities context = new PartnerEntities())
            {
                AllPartners = context.partners.ToList();
            }
        }

        public void CheckBalanceAndTakeNecessaryAction()
        {
            using (PartnerEntities context = new PartnerEntities())
            {
                List<KeyValuePair<Regex, string>> serviceAliases = Tbc.ServiceAliasesRegex;
                List<string> billableType = new List<string>()
                {
                    "/custBilled", "/suppBilled", "/billable"
                };
                List<account> payableAccounts = context.accounts.Where(x => billableType.Contains(x.billableType)).ToList();
                foreach (account account in payableAccounts)
                {
                    partner partner = AllPartners.Where(x => x.idPartner == account.idPartner).First();
                    List<acc_action> actions = account.acc_action.OrderBy(x => x.threshhold_value).ToList();
                    if (actions.Count > 0)
                    {
                        foreach (acc_action action in actions)
                        {
                            if (account.getCurrentBalanceWithTempTransaction() <= action.threshhold_value)
                            {
                                if (action.isNotified != 1)
                                {
                                    IAutomationAction accountAction = AccountAutomationActions.Where(x => x.Id == action.idAccountAction).FirstOrDefault();
                                    accountAction.Tbc = this.Tbc;
                                    if (accountAction != null)
                                    {
                                        accountAction.Execute(partner);
                                        action.isNotified = 1;
                                        context.SaveChanges();
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
