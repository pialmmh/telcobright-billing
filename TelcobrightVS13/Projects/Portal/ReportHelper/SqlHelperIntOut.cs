using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PortalApp.ReportHelper
{
    public class SqlHelperIntOut : AbstractSqlHelper{
    public  SqlHelperIntOut(string startDate, string endDate, string groupInterval, string tablename,
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
                Concat(cn.name,' (',cx.CountryCode,')') as Country,
				Concat(x.tup_matchedprefixcustomer,' (',cx.Description,')') AS Destination,
                cr.partnerName AS ANS,
                cr1.partnerName AS IGW,
                cr2.partnerName AS 'International Partner',
                TotalCalls AS CallsCount,
                Successfulcalls  AS 'No of Calls (Outgoing International)',
                ConnectedCalls AS ConnectedCount,
                MinutesIn AS 'Paid Minutes (Outgoing Internaitonal)',
                duration3 AS 'hmsduration',
                roundedduration AS 'roundedduration', 
                duration1 AS 'supplierduration',
                ASR,
                ACD,
                PDD,
                CCR,
                ConectbyCC,
                CCRbyCC,  
                X AS 'X (BDT)',
                Y AS 'Y (USD)',
                Z AS 'Z (BDT)',
                revenueigwout,
                partnercost,
                suppliercost,
                SupplierPrefix,
                XRate AS 'X RATE(BDT)', 
                YRate AS 'Y RATE(USD)', 
                USDRate AS 'Dollar Rate' 
                FROM
                (
	            SELECT {GetDateExpression(this.groupInterval)} AS Date,
                tup_countryorareacode,
                tup_matchedprefixcustomer,            
	            tup_sourceID ,  
                tup_inpartnerid  ,   
	            tup_outpartnerid ,  
                SUM(totalcalls) AS TotalCalls, 
				SUM(successfulcalls) AS Successfulcalls, 
				SUM(connectedcalls) AS ConnectedCalls,
		        SUM((actualduration)/60)AS MinutesIn, 
                sum(roundedduration)/60 AS roundedduration, 
                sum(duration2)/60 AS duration1,
                sum(duration3)/60 AS duration3,
                ((SUM(Successfulcalls)*100)/SUM(totalcalls)) AS ASR, 
		        ((SUM(actualDuration)/60)/SUM(Successfulcalls)) AS ACD,
		        (SUM(pdd)/SUM(Successfulcalls))AS PDD ,
		        100*(SUM(connectedCalls)/SUM(totalcalls)) AS CCR,
                SUM(connectedCallscc) AS ConectbyCC ,
		        100*(SUM(connectedCallsCC)/SUM(totalcalls)) AS CCRByCC,
                SUM(longDecimalAmount1) AS X,
                SUM(longDecimalAmount2) AS Y,
                SUM(longDecimalAmount3) AS Z,
                SUM(customercost) AS revenueigwout,
                Sum(tax1) AS partnercost,
                suppliercost,
                tup_matchedprefixsupplier AS SupplierPrefix,
                tup_customerrate as XRate, 
                tup_supplierrate as YRate, 
                tup_customercurrency as USDRate 

	            FROM {TableName}
                WHERE tup_starttime>='{StartDate}'
                AND tup_starttime<'{EndDate}'
                {GetWhereClauseAdditional()}
                {GetGroupBy()}
            
            ) x
            LEFT JOIN partner cr
            ON x.tup_sourceID = cr.idpartner
            LEFT JOIN partner cr1
            ON x.tup_inpartnerid = cr1.idpartner
            LEFT JOIN partner cr2
            ON x.tup_outpartnerid = cr2.idpartner
            LEFT JOIN xyzprefix cx
            ON x.tup_matchedprefixcustomer=cx.Prefix
            LEFT JOIN CountryCode cn
            ON cx.CountryCode=cn.Code
            ORDER BY " + (GetGroupBy().Contains("tup_starttime") ? "Date, " : string.Empty) + " Successfulcalls DESC ;";
        }
    }
}