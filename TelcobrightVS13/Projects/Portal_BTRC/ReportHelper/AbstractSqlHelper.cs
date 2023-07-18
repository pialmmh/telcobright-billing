using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;

namespace PortalApp
{
	//public class TupleColumnHelper
	//{
	//    public string ColumnName { get; set; }
	//    public string SelectExpression { get; set; }
	//    public string GroupByExpression { get; set; }
	//    public string JoinExpression { get; set; }

	//}
	public abstract class AbstractSqlHelper
	{
		protected string StartDate { get; set; }
		protected string EndDate { get; set; }
		protected string TableName { get; set; }
		protected string groupInterval { get; set; }
		protected List<string> groupExpressions { get; set; }
		protected List<string> whereExpressions { get; set; }
		protected Dictionary<string, string> dateExpressions = new Dictionary<string, string>()
			{
				{"None","(select null)"},
				{ "Yearly","year(tup_starttime)"},
				{ "Monthly","concat(year(tup_starttime),'-',date_format(date(tup_starttime),'%b'))"},
				//{ "Daily","date(tup_starttime)" },
			    { "Daily","date_format(tup_starttime, '%M %d %Y')" },
//                { "Hourly","concat(date(tup_starttime),' ',date_format(date(tup_starttime),'%H'),':00')"},
				{ "Hourly","concat(date(tup_starttime),' ',date_format(tup_starttime,'%H'),':00')"},
				{"Weekly","concat(year(tup_starttime),'-W',week(tup_starttime))" }
			};

		public virtual string getSQLString()
		{
			return $@"
				SELECT Date,
				Concat(cn.name,' (',cx.CountryCode,')') as Country,
				Concat(x.tup_matchedprefixcustomer,' (',cx.Description,')') AS Destination,
				cr.partnerName AS ANS,
				cr1.partnerName AS IGW,
				TotalCalls,
				Successfulcalls,
				ConnectedCalls,
				CallsIn AS 'callstatus', 
				round(MinutesIn,6) AS CauseCode,
				(SELECT null) AS 'International partner',
				costansin,
				costicxin,
				costvatcomissionin,
				igwrevenuein,
				customercost,
				profit,
				ASR,
				ACD,
				PDD,
				duration1 ,
				roundedduration, 
				CCR,
				CallsCount,
				CCRbyCC,
				ConnectedCount ,
				ConnectByCC
				FROM
				(
				SELECT {GetDateExpression(this.groupInterval)} AS Date,

				tup_inpartnerid,              
				tup_destinationID ,       
				tup_outpartnerid ,  
				tup_countryorareacode,
				tup_matchedprefixcustomer,
				SUM(totalcalls) AS TotalCalls, 
				SUM(successfulcalls) AS Successfulcalls, 
				SUM(connectedcalls) AS ConnectedCalls,
				SUM(Successfulcalls) AS CallsIn, 
				SUM((actualDuration)/60)AS MinutesIn, 
				(SELECT 0) AS costansin,
				SUM(OutPartnerCost) AS costicxin, 
				SUM(tax1) AS costvatcomissionin,
				(SELECT 0) AS igwrevenuein,
				(SELECT customercost) AS customercost,
				(SELECT 0) AS profit,
				((SUM(Successfulcalls)*100)/SUM(totalcalls)) AS ASR, 
				((SUM(actualDuration)/60)/SUM(Successfulcalls)) AS ACD,
				(SUM(pdd)/SUM(Successfulcalls))AS PDD ,
				sum(roundedduration)/60 AS roundedduration, 
				sum(duration1)/60 AS duration1, 
				100*(SUM(connectedCalls)/SUM(totalcalls)) AS CCR, 
				SUM(totalcalls) AS CallsCount,
				SUM(connectedCalls)AS ConnectedCount ,
				SUM(connectedCallscc) AS ConectbyCC ,
				100*(SUM(connectedCallsCC)/SUM(totalcalls)) AS CCRByCC 
				FROM '{TableName}'
				WHERE tup_starttime>='{StartDate}'
				AND tup_starttime<='{EndDate}'
				{GetWhereClauseAdditional()}
				{GetGroupBy()}
			
			) x
			LEFT JOIN partner cr
			ON x.ANS = cr.idpartner
			LEFT JOIN partner cr1
			ON x.IGW = cr1.idpartner
			LEFT JOIN xyzprefix cx
			ON x.tup_matchedprefixcustomer=cx.Prefix
			LEFT JOIN CountryCode cn
			ON cx.CountryCode=cn.Code
			ORDER BY callsin, costansin DESC ;";
		}
		protected virtual string GetGroupBy()
		{
			List<string> nonEmptyExpressions = groupExpressions.Where(s => !string.IsNullOrEmpty(s)).ToList();
			if (nonEmptyExpressions.Count > 0)
			{
				return "group by " + string.Join(",", nonEmptyExpressions);
			}
			return "";
		}

		protected virtual string GetGroupByWithPreExistingClauses(string preExistingGrouByExpression)
		{
			List<string> nonEmptyExpressions = groupExpressions.Where(s => !string.IsNullOrEmpty(s)).ToList();
			if (nonEmptyExpressions.Count > 0)
			{
				return new StringBuilder(preExistingGrouByExpression).Append(string.Join(",", nonEmptyExpressions)).ToString();
			}
			return preExistingGrouByExpression.Substring(0, preExistingGrouByExpression.Length-2);
		}


		protected virtual string GetDateExpression(string groupInterval)
		{
		    if (groupInterval != string.Empty)
		        return dateExpressions[groupInterval];
		    else return dateExpressions["None"];
		}
		protected string GetWhereClauseAdditional()
		{
			List<string> nonEmptyExpression = this.whereExpressions.Where(s => !string.IsNullOrEmpty(s)).ToList();
			return string.Join(Environment.NewLine, nonEmptyExpression.Select(s => " and " + s).ToList());
		}
	}
}