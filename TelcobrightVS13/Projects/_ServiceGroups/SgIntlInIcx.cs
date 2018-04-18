using System.ComponentModel.Composition;
using System;
using MediationModel;
using System.Collections.Generic;
using LibraryExtensions;
using TelcobrightMediation.Cdr;
using TransactionTuple = System.ValueTuple<int, int, long, int,long>;
namespace TelcobrightMediation
{

	[Export("ServiceGroup", typeof(IServiceGroup))]
	public class SgIntlInIcx : IServiceGroup
	{
		private readonly SgIntlTransitVoice _sgIntlTransitVoice = new SgIntlTransitVoice();
		public override string ToString() => this.RuleName;
		public string RuleName => "International Incoming Calls [ICX]";
		public string HelpText { get; } = "Service group Intl In for BD ICX.";
		public int Id => 3;
		private Dictionary<string, Type> SummaryTargetTables { get; }
		public SgIntlInIcx()//constructor
		{
		    this.SummaryTargetTables = new Dictionary<string, Type>()
				{
					{ "sum_voice_day_03",typeof(sum_voice_day_03)},
					{ "sum_voice_hr_03" ,typeof(sum_voice_hr_03) },
				};
		}
		public Dictionary<string, Type> GetSummaryTargetTables()
		{
			return this.SummaryTargetTables;
		}

		public void ExecutePostRatingActions(CdrExt cdrExt, CdrProcessor cdrProcessor)
		{
			cdrExt.Cdr.roundedduration = cdrExt.Cdr.Duration1;
			cdrExt.Cdr.CostICXIn = cdrExt.Cdr.CustomerCost;
		}

		public void Execute(cdr thisCdr, CdrProcessor cdrProcessor)
		{
			//international in call direction/service group
			var dicRoutes = cdrProcessor.CdrJobContext.MediationContext.MefServiceGroupContainer.DicRouteIncludePartner;
			string key = thisCdr.SwitchId + "-" + thisCdr.incomingroute;
			route thisRoute = null;
			dicRoutes.TryGetValue(key, out thisRoute);
			if (thisRoute != null)
			{
				if (thisRoute.partner.PartnerType == IcxPartnerType.IOS 
                    && thisRoute.NationalOrInternational == RouteLocalityType.International
				) //IGW and route=international
				{
					thisCdr.CallDirection = 3; //Intl in ICX
											   //set year-month id for this call for tt clean & bc selling
					string startTimeAsStr = thisCdr.StartTime.ToMySqlStyleDateTimeStrWithoutQuote();
					thisCdr.field2 = Convert.ToInt32(startTimeAsStr.Substring(2, 2) + startTimeAsStr.Substring(5, 2));
				}
			}
		}

		public void SetServiceGroupWiseSummaryParams(CdrExt cdrExt, AbstractCdrSummary newSummary)
		{
			this._sgIntlTransitVoice.SetServiceGroupWiseSummaryParams(cdrExt, newSummary);
		}
	}
}
