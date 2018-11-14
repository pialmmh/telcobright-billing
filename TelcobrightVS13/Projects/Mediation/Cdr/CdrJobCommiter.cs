using System;
using System.Data.Common;

namespace TelcobrightMediation.Cdr
{
    public static class CdrJobCommiter
    {
        public static void Commit(DbCommand cmd)
        {
            CheckIfCdrDurationMatchesSummaryDuration(cmd);
            CheckIfTransactionAmountMatchesLedgerSummary(cmd);
            cmd.CommandText = "commit;";
            cmd.ExecuteNonQuery();
        }
        private static void CheckIfCdrDurationMatchesSummaryDuration(DbCommand cmd)
        {
            cmd.CommandText = @"select totalinsertedduration as duration from cdrmeta union all
										select 
											(select totalinsertedduration from cdrsummarymeta_day_01)+
											(select totalinsertedduration from cdrsummarymeta_day_02)+
											(select totalinsertedduration from cdrsummarymeta_day_03)+
											(select totalinsertedduration from cdrsummarymeta_day_04)+
											(select totalinsertedduration from cdrsummarymeta_day_05)+
											(select totalinsertedduration from cdrsummarymeta_day_06) as duration union all
										select 
											(select totalinsertedduration from cdrsummarymeta_hr_01)+
											(select totalinsertedduration from cdrsummarymeta_hr_02)+
											(select totalinsertedduration from cdrsummarymeta_hr_03)+
											(select totalinsertedduration from cdrsummarymeta_hr_04)+
											(select totalinsertedduration from cdrsummarymeta_hr_05)+
											(select totalinsertedduration from cdrsummarymeta_hr_06) as duration;";
            DbDataReader reader = cmd.ExecuteReader();
            decimal durationInCdr = -1;
            int durationRowCount = 0;
            while (reader.Read())
            {
                if (++durationRowCount == 1)
                    durationInCdr = reader.GetDecimal(0);
                else
                {
                    decimal dayOrSummaryDuration = reader.GetDecimal(0);
                    if (dayOrSummaryDuration != durationInCdr)
                        throw new Exception("Duration in cdr & summary tables do not match after writing cdrs.");
                }
            }
            reader.Close();
        }
        private static void CheckIfTransactionAmountMatchesLedgerSummary(DbCommand cmd)
        {
            cmd.CommandText = @"select totalinsertedAmount as amount from transactionmeta union all
								  select totalinsertedamount from ledger_summary_meta as amount;";
            DbDataReader reader = cmd.ExecuteReader();
            decimal amountInTransaction = -1;
            int durationRowCount = 0;
            while (reader.Read())
            {
                if (++durationRowCount == 1)
                    amountInTransaction = reader.GetDecimal(0);
                else
                {
                    decimal ledgerSummaryAmount = reader.GetDecimal(0);
                    if (ledgerSummaryAmount != amountInTransaction)
                        throw new Exception("Amount in transaction & ledger summary tables do not match after writing transactions.");
                }
            }
            reader.Close();
        }
    }
}