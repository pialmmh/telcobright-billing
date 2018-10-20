using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;

namespace PortalApp.ReportHelper
{
    public class SqlHelperIntlInTransit : AbstractSqlHelper
    {
        public SqlHelperIntlInTransit(string startDate, string endDate, string groupInterval,string tablename,
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
                SELECT Date,
                inPartner.partnerName AS 'In Partner',
                outPartner.partnerName AS 'Out Partner',
                TotalCalls AS 'Total Calls',
                Successfulcalls  AS 'Successful Calls',
                ConnectedCalls AS 'Connected Calls',
                customerduration as 'Customer Duration',
                supplierDuration AS 'Supplier Duration', 
                customercost as 'Revenue',
                supplierCost as 'Cost',
                customercost-suppliercost as 'Margin',
                ASR,
                ACD,
                PDD,
                CCR,
                ConectbyCC,
                CCRbyCC, 
                tup_matchedprefixcustomer, 
                tup_matchedprefixsupplier 
                FROM
                (
	            SELECT {GetDateExpression(this.groupInterval)} AS Date,
                tup_inpartnerid,              
                tup_outpartnerid ,  
                tup_matchedprefixcustomer, 
                tup_matchedprefixsupplier, 
                SUM(totalcalls) AS TotalCalls, 
                SUM(successfulcalls) AS Successfulcalls, 
                SUM(connectedcalls) AS ConnectedCalls,
                sum(duration1)/60 AS customerDuration,
                sum(duration2)/60 AS supplierDuration,
                Sum(customercost) AS customercost,
                Sum(suppliercost) AS suppliercost,
                ((SUM(Successfulcalls)*100)/SUM(totalcalls)) AS ASR, 
                ((SUM(actualDuration)/60)/SUM(Successfulcalls)) AS ACD,
                (SUM(pdd)/SUM(Successfulcalls))AS PDD ,
                100*(SUM(connectedCalls)/SUM(totalcalls)) AS CCR,
                SUM(connectedCallscc) AS ConectbyCC ,
                100*(SUM(connectedCallsCC)/SUM(totalcalls)) AS CCRByCC 
                FROM {TableName}
                WHERE tup_starttime>='{StartDate}'
                AND tup_starttime<'{EndDate}'
                {GetWhereClauseAdditional()}
                {GetGroupBy()}
            
            ) x
            LEFT JOIN partner inPartner
            ON x.tup_inpartnerid = inPartner.idpartner
            LEFT JOIN partner outPartner
            ON x.tup_outPartnerid = outPartner.idpartner
            ORDER BY " + (GetGroupBy().Contains("tup_starttime") ? "Date, " : string.Empty) + " Successfulcalls DESC ;";
        }
    }
}