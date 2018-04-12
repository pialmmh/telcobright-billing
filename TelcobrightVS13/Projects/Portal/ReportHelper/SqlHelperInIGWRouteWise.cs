using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PortalApp.ReportHelper
{
    public class SqlHelperInIGWRouteWise : AbstractSqlHelper
    {

        public SqlHelperInIGWRouteWise(string startDate, string endDate, string groupInterval, string tablename,
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
                ANS,
                concat('1-',cr.RouteName) AS 'Incoming Route',
                concat('1-', cr1.RouteName) AS 'Outgoing Route',
                TotalCalls AS CallsCount,
                Successfulcalls  AS 'Number Of Calls (International Incoming)',
                ConnectedCalls AS ConnectedCount,
                MinutesIn AS 'Paid Minutes (International Incoming)',
                roundedduration AS 'RoundedDuration', 
                duration1 AS 'Duration1',
                costansin,
                costicxin,
                costvatcomissionin,
                customercost,
                igwrevenuein,
                profit,
                ASR,
                ACD,
                PDD,
                CCR,
                ConectbyCC,
                CCRbyCC             
                FROM
                (
	            SELECT {GetDateExpression(this.groupInterval)} AS Date,
                (SELECT 0) AS ANS,
                tup_incomingroute,              
	            tup_outgoingroute ,       
                SUM(totalcalls) AS TotalCalls, 
				SUM(successfulcalls) AS Successfulcalls, 
				SUM(connectedcalls) AS ConnectedCalls,
		        SUM((actualduration)/60)AS MinutesIn, 
                sum(roundedduration)/60 AS roundedduration, 
                sum(duration1)/60 AS duration1,
		        (SELECT 0) AS costansin,
		        SUM(SupplierCost) AS costicxin, 
		        SUM(tax1) AS costvatcomissionin,
		        Sum(customercost) AS customercost,
                (SELECT 0) AS igwrevenuein,
		        (SELECT 0) AS profit,
		        ((SUM(Successfulcalls)*100)/SUM(totalcalls)) AS ASR, 
		        ((SUM(actualDuration)/60)/SUM(Successfulcalls)) AS ACD,
		        (SUM(pdd)/SUM(Successfulcalls))AS PDD ,
		        100*(SUM(connectedCalls)/SUM(totalcalls)) AS CCR,
                SUM(connectedCallscc) AS ConectbyCC ,
		        100*(SUM(connectedCallsCC)/SUM(totalcalls)) AS CCRByCC 
	            FROM {TableName}
                WHERE tup_starttime>='{StartDate}'
                AND tup_starttime<='{EndDate}'
                {GetWhereClauseAdditional()}
                {GetGroupBy()}
            
            ) x
            LEFT JOIN route cr
            ON x.tup_incomingroute = cr.RouteName
            LEFT JOIN route cr1
            ON x.tup_outgoingroute = cr1.RouteName
            ORDER BY Successfulcalls,costansin DESC ;";
        }

        protected string GetWhereClauseAdditional()
        {
            List<string> nonEmptyExpression = this.whereExpressions.Where(s => !string.IsNullOrEmpty(s)).ToList();
            return string.Join(Environment.NewLine, nonEmptyExpression.Select(s => " and " + s).ToList());
        }
    }
}