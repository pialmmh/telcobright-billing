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

        public AccountActionService()
        {
            // TODO: get this from config
            AccountAutomationActions = new List<IAutomationAction>();
            AccountAutomationActions.Add(new SendAlertEmailAccountAction());
            AccountAutomationActions.Add(new SMSAccountAction());
            AccountAutomationActions.Add(new ActionBlockingAutomation());

            using (PartnerEntities context = new PartnerEntities())
            {
                AllPartners = context.partners.ToList();
            }
        }

        public void CheckBalanceAndTakeNecessaryAction()
        {
            using (PartnerEntities context = new PartnerEntities())
            {
                TelcobrightConfig Tbc = new TelcobrightConfig();/* = PageUtil.GetTelcobrightConfig();*/
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
                                // TODO: Check for already executed action
                                IAutomationAction accountAction = AccountAutomationActions.Where(x => x.Id == action.idAccountAction).FirstOrDefault();
                                if (accountAction != null)
                                {
                                    accountAction.Execute(partner);
                                    // TODO: Save action execution history
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
