using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PortalApp.ReportHelper
{
    public class SqlHelperPaymentHistory : AbstractSqlHelper
    {

        protected Dictionary<string, string> DateExpressions = new Dictionary<string, string>()
        {
            {"None","(select null)"},
            { "Daily","date_format(transactionTime, '%M %d %Y')" },
            { "Yearly","year(transactionTime)"},
            { "Monthly","concat(year(transactionTime),'-',date_format(date(transactionTime),'%b'))"},
        };

        public SqlHelperPaymentHistory(string startDate, string endDate, string groupInterval, string tablename,
            List<string> groupExpressions, List<string> whereExpressions)
        {
            StartDate = startDate;
            EndDate = endDate;
            TableName = tablename;
            this.groupInterval = groupInterval;
            this.groupExpressions = groupExpressions;
            this.whereExpressions = whereExpressions;
        }

        public override string getSQLString()
        {
            return $@"
                select {GetDateExpression(this.groupInterval)} as Date, p.PartnerName, x.Amount, 
                x.Currency
                from 
                (
                    select ac.idPartner, 
                    {(GetGroupBy().Contains("transactionTime") ? "transactionTime " : string.Empty)} as transactionTime, 
                    ac.uom Currency, 
                    sum(att.amount) as Amount 
                    from {TableName} att
                    inner join account ac on att.glAccountId = ac.id
                    where att.transactionTime >= '{StartDate}'
                    and att.transactionTime < '{EndDate}'
                    and att.idEvent = -1
                    {GetWhereClauseAdditional()}
                    {GetGroupBy()}
                ) x
                inner join partner p on p.idPartner = x.idPartner
                order by " + (GetGroupBy().Contains("transactionTime") ? "transactionTime desc, " : string.Empty) + " p.PartnerName asc;";
        }

        protected override string GetDateExpression(string interval)
        {
            if (interval != string.Empty)
                return DateExpressions[interval];
            else return DateExpressions["None"];
        }
    }
}