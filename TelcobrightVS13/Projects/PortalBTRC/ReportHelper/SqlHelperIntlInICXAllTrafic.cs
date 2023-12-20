using System.Collections.Generic;

namespace PortalApp.ReportHelper
{
    public class SqlHelperIntlInIcxAllTrafic : AbstractSqlHelper
    {
        public SqlHelperIntlInIcxAllTrafic(string startDate, string endDate, string groupInterval,string tablename,
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
                    SELECT  ab.a_noofcalls as noofcalls1, ab.a_minutes as minutes1, ab.b_noofcalls as noofcalls2, ab.b_minutes as minutes2, c.noofcalls as noofcalls3, c.minutes as minutes3

                    FROM
                      (SELECT 
                        a.callType AS a_callType, 
                        a.noofcalls AS a_noofcalls, 
                        a.minutes AS a_minutes, 
                        b.callType AS b_callType, 
                        b.noofcalls AS b_noofcalls, 
                        b.minutes AS b_minutes
                      FROM
                        (SELECT 
                          'Domestic' as callType,
                          SUM(totalcalls) as noofcalls, 
                          SUM(roundedduration) / 60 as minutes 
                        FROM 
                          sum_voice_day_01
                        WHERE 
                          tup_starttime >= '2023-01-01' 
                          AND tup_starttime < '2023-12-11'
                        GROUP BY 
                          callType) a

                      LEFT JOIN 

                        (SELECT 
                          'International Out' as callType,
                          SUM(totalcalls) as noofcalls, 
                          SUM(roundedduration) / 60 as minutes 
                        FROM 
                          sum_voice_day_02
                        WHERE 
                          tup_starttime >= '2023-01-01' 
                          AND tup_starttime < '2023-12-11'
                        GROUP BY 
                          callType) b

                      ON a.callType != b.callType) ab

                    LEFT JOIN 

                      (SELECT 
                          'International In' as callType,
                          SUM(totalcalls) as noofcalls, 
                          SUM(roundedduration) / 60 as minutes 
                        FROM 
                          sum_voice_day_03
                        WHERE 
                          tup_starttime >= '2023-01-01' 
                          AND tup_starttime < '2023-12-11'
                        GROUP BY 
                          callType) c

                    ON ab.a_callType != c.callType AND ab.b_callType != c.callType;";
        }
    }
}