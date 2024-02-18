using System.Collections.Generic;


namespace PortalApp.ReportHelper
{
    public class SqlHelperDomestic:AbstractSqlHelper
    {
    public  SqlHelperDomestic(string startDate, string endDate, string groupInterval, string tablename,
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
                cr.partnerName AS 'Incoming ANS',
                cr1.partnerName AS 'Outgoing ASN',
                cr2.partnerName AS 'Int Partner',
                TotalCalls AS CallsCount,
                Successfulcalls  AS 'Number Of Calls',
                ConnectedCalls AS ConnectedCount,
                MinutesIn AS 'Paid Minutes',
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
                tup_inpartnerid,                     
	            tup_outpartnerid , 
                tup_sourceId, 
                SUM(totalcalls) AS TotalCalls, 
				SUM(successfulcalls) AS Successfulcalls, 
				SUM(connectedcalls) AS ConnectedCalls,
		        SUM((actualduration)/60)AS MinutesIn, 
                sum(roundedduration)/60 AS roundedduration, 
                sum(duration1)/60 AS duration1,
		        (SELECT 0) AS costansin,
		        SUM(OutPartnerCost) AS costicxin, 
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
            LEFT JOIN partner cr
            ON x.tup_inpartnerid = cr.idpartner
            LEFT JOIN partner cr1
            ON x.tup_outpartnerid = cr1.idpartner
            LEFT JOIN partner cr2
            ON x.tup_sourceId = cr2.idpartner

            ORDER BY Successfulcalls,costansin DESC ;";
        }
    }
}