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
                select x.Date, p.PartnerName, x.Amount, 
                x.Currency
                from 
                (
                    select ac.idPartner, 
                    {GetDateExpression(this.groupInterval)} as Date, 
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
                order by " + (GetGroupBy().Contains("tup_starttime") ? "Date, " : string.Empty) + " p.PartnerName asc;";
        }

        protected override string GetDateExpression(string interval)
        {
            if (interval != string.Empty)
                return DateExpressions[interval];
            else return DateExpressions["None"];
        }
    }
}