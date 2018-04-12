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
	public class SgIntlOutIcx : IServiceGroup
	{
		private readonly SgIntlTransitVoice _sgIntlTransitVoice=new SgIntlTransitVoice();
		public override string ToString() => this.RuleName;
		public string RuleName => GetType().Name;
		public string HelpText => "Service group International Outgoing for BD ICX.";
		public int Id => 2;
		private Dictionary<string, Type> SummaryTargetTables { get; }
		public SgIntlOutIcx()//constructor
		{
			this.SummaryTargetTables = new Dictionary<string, Type>()
				{
					{ "sum_voice_day_02",typeof(sum_voice_day_02)},
					{ "sum_voice_hr_02" ,typeof(sum_voice_hr_02) },
				};
		}
		public Dictionary<string, Type> GetSummaryTargetTables()
		{
			return this.SummaryTargetTables;
		}

		public void ExecutePostRatingActions(CdrExt cdrExt, CdrProcessor cdrProcessor)
		{
			return;
		}

		public void Execute(cdr thisCdr, CdrProcessor cdrProcessor)
		{
			//international Out call direction/service group
			var dicRoutes = cdrProcessor.CdrJobContext.MediationContext.MefServiceGroupContainer.DicRouteIncludePartner;
			string key = thisCdr.SwitchId + "-" + thisCdr.incomingroute;
			route thisRoute = null;
			dicRoutes.TryGetValue(key, out thisRoute);
			if (thisRoute != null)
			{
				if (thisRoute.partner.PartnerType == 1 && thisRoute.NationalOrInternational == 2
				) //ANS and route=international
				{

					thisCdr.CallDirection = 2; //international Out in IGW
											   //set year-month id for this call for tt clean & bc selling
					string tempDateTime = thisCdr.StartTime.ToMySqlStyleDateTimeStrWithoutQuote();
					thisCdr.field1 =Convert.ToInt32(tempDateTime.Substring(2, 2) + tempDateTime.Substring(5, 2));
					thisCdr.field2 = Convert.ToInt32(tempDateTime.Substring(2, 2) + tempDateTime.Substring(5, 2));
				}
			}
		}

		public void SetServiceGroupWiseSummaryParams(CdrExt cdrExt, AbstractCdrSummary newSummary)
		{
			acc_chargeable chargeableCust = null;
			cdrExt.Chargeables.TryGetValue(new ValueTuple<int, int,int>(this.Id, 7, 1), out chargeableCust);
			if (chargeableCust == null)
			{
				throw new Exception("Chargeable not found for customer direction.");
			}
			newSummary.tup_matchedprefixcustomer = cdrExt.Cdr.MatchedPrefixY;
			newSummary.tup_countryorareacode = cdrExt.Cdr.CountryCode;
			newSummary.tup_sourceId = cdrExt.Cdr.AnsIdOrig.ToString();
			if (cdrExt.Cdr.ChargingStatus != 1) return;

			newSummary.tup_customerrate = chargeableCust.unitPriceOrCharge;
			newSummary.tup_customercurrency = chargeableCust.idBilledUom;
			newSummary.customercost = chargeableCust.BilledAmount;
			newSummary.doubleAmount1 = Convert.ToDouble(chargeableCust.OtherAmount1);
			newSummary.doubleAmount2 = Convert.ToDouble(chargeableCust.OtherAmount2);
			newSummary.doubleAmount3 = Convert.ToDouble(chargeableCust.OtherAmount3);

			newSummary.tup_destinationId = "";
			newSummary.tup_tax1currency = "";
			newSummary.tup_tax2currency = "";
			newSummary.tup_vatcurrency = "";
			newSummary.tax1 = 0;
			newSummary.tax2 = 0;
			newSummary.vat = 0;
			newSummary.intAmount1 = 0;
			newSummary.intAmount2 = 0;
			newSummary.longAmount1 = 0;
			newSummary.longAmount2 = 0;
		}
	}
}
