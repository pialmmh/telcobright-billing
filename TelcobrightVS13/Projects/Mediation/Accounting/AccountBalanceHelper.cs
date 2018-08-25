using MediationModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelcobrightMediation.Accounting
{
    public static class AccountBalanceHelper
    {
        public static decimal getCurrentBalanceWithTempTransaction(this account account)
        {
            decimal tempBalance;
            using (PartnerEntities context = new PartnerEntities())
            {
                tempBalance = context.acc_temp_transaction.Where(x => x.glAccountId == account.id)
                        .Select(x => x.amount)
                        .DefaultIfEmpty(0)
                        .Sum();
            }
            return tempBalance + account.balanceAfter;
        }
    }
}
