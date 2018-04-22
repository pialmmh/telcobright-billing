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
    public class SgIntlInIgw : IServiceGroup
    {
        private readonly SgIntlTransitVoice _sgIntlTransitVoice = new SgIntlTransitVoice();
        public override string ToString() => this.RuleName;
        public string RuleName => "International Incoming Calls [IGW]";

        public string HelpText =>
            "Service group International Incoming for BD IGW. Old common mediation codes for IGW, could not separate in short time, covers both intl in/out";

        public int Id => 4;
        private Dictionary<string, Type> SummaryTargetTables { get; }

        public SgIntlInIgw() //constructor
        {
            this.SummaryTargetTables = new Dictionary<string, Type>()
            {
                {"sum_voice_day_03", typeof(sum_voice_day_03)},
                {"sum_voice_hr_03", typeof(sum_voice_hr_03)},
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
            MefServiceGroupsContainer servGroupData =
                cdrProcessor.CdrJobContext.MediationContext.MefServiceGroupContainer;
            //international in call direction/service group
            var dicRoutes = servGroupData.SwitchWiseRoutes;
            var key=new ValueTuple<int,string>(thisCdr.SwitchId,thisCdr.incomingroute); 
            route thisRoute = null;
            dicRoutes.TryGetValue(key, out thisRoute);
            if (thisRoute != null)
            {
                if (thisRoute.partner.PartnerType == 3) //foreign partner
                {
                    thisCdr.CallDirection = 4; //internationa inco in IGW
                    int roundedDuration = 0;
                    //set rounded duration [field 88] 100 ms
                    //get decimal part only by rouding the actual duration to 2 decimals first

                    //set ttclean/bcsellling 
                    thisCdr.field1 = 1;
                    //set year-month id for this call
                    string strDateTime = thisCdr.StartTime.ToMySqlStyleDateTimeStrWithoutQuote();
                    thisCdr.field2 = Convert.ToInt32(strDateTime.Substring(2, 2) + strDateTime.Substring(5, 2));

                    string terminatingCalledNumber = thisCdr.TerminatingCalledNumber.ToString();
                    if (terminatingCalledNumber != "" && terminatingCalledNumber.Length >= 5)
                    {
                        terminatingCalledNumber =
                            terminatingCalledNumber.Substring(5,
                                terminatingCalledNumber.Length - 5); //trim igwprefix+0 terminating
                    }
                    else //Term. call number may not be present in failed calls...in that case
                    {
                        //determine ansidterm from originating called number (for accurate performance monitoring)
                        string originatingCalledNumber = thisCdr.OriginatingCalledNumber;
                        if (originatingCalledNumber != null)
                        {
                            foreach (ansprefixextra extraPrefixBeforeNumber in cdrProcessor.CdrJobContext
                                .MediationContext.LstAnsPrefixExtra)
                            {
                                int thisLen = extraPrefixBeforeNumber.PrefixBeforeAnsNumber.Length;
                                if (thisLen > originatingCalledNumber.Length) continue;
                                //remove the extra prefix before ans identifier e.g. 17, 19 etc.
                                if (originatingCalledNumber.Substring(0, thisLen) ==
                                    extraPrefixBeforeNumber.PrefixBeforeAnsNumber)
                                {
                                    //set term called number based on orig called number to find out ANS
                                    terminatingCalledNumber =
                                        originatingCalledNumber.Substring(thisLen,
                                            originatingCalledNumber.Length - thisLen);
                                    break;
                                }
                            }
                        }
                    }
                    int iteration = 0;
                    //find out AnsidTerm for this call
                    //50	ANSPrefixOrig
                    //51	AnsIdOrig
                    //52	AnsPrefixTerm
                    //53	AnsIdTerm
                    string ansPrefixTerm = "";
                    int? ansIdTerm = null;
                    for (iteration = 0; iteration < terminatingCalledNumber.Length; iteration++)
                    {
                        partnerprefix thisPrefix = null;
                        string matchStr = terminatingCalledNumber.Substring(0, iteration + 1);
                        cdrProcessor.CdrJobContext.MediationContext.DictAnsOrig.TryGetValue(matchStr, out thisPrefix);
                        if (thisPrefix != null)
                        {
                            ansPrefixTerm = thisPrefix.Prefix;
                            ansIdTerm = thisPrefix.idPartner;
                            continue;
                        }
                        continue;
                    }
                    thisCdr.AnsPrefixTerm = ansPrefixTerm; //if the code reaches here, there's been no error
                    thisCdr.AnsIdTerm = ansIdTerm;
                }
            }
        }

        public void SetServiceGroupWiseSummaryParams(CdrExt cdrExt, AbstractCdrSummary newSummary)
        {
            newSummary.tup_countryorareacode = cdrExt.Cdr.CountryCode;
            newSummary.tup_destinationId = cdrExt.Cdr.AnsIdTerm.ToString();
            newSummary.tup_matchedprefixsupplier =
                cdrExt.Cdr
                    .matchedprefixsupplier; //this is overwritten in SetChargingSummaryInCustomerDirection for successful calls
            if (cdrExt.Cdr.ChargingStatus != 1) return;

            acc_chargeable chargeableCust = null;
            cdrExt.Chargeables.TryGetValue(new ValueTuple<int, int, int>(this.Id, 1, 1), out chargeableCust);
            if (chargeableCust == null)
            {
                throw new Exception("Chargeable info not found for customer direction.");
            }
            this._sgIntlTransitVoice.SetChargingSummaryInCustomerDirection(chargeableCust, newSummary);

            acc_chargeable chargeableSupp = null;
            cdrExt.Chargeables.TryGetValue(new ValueTuple<int, int, int>(this.Id, 3, 2), out chargeableSupp);
            if (chargeableSupp == null)
            {
                throw new Exception("Chargeable not found for supplier direction.");
            }

            newSummary.tup_matchedprefixsupplier = chargeableSupp.Prefix;
            newSummary.tup_suppliercurrency = "USD";
            newSummary.tup_supplierrate = chargeableSupp.unitPriceOrCharge;
            newSummary.suppliercost = chargeableSupp.BilledAmount;
            newSummary.tup_tax1currency = "USD"; //btrc
            newSummary.tax1 = Convert.ToDecimal(Convert.ToDouble(chargeableSupp.OtherAmount1)); //btrc

            newSummary.tup_tax2currency = "";
            newSummary.tup_vatcurrency = "";
            newSummary.tax2 = 0;
            newSummary.vat = 0;
            newSummary.intAmount1 = 0;
            newSummary.intAmount2 = 0;
            newSummary.longAmount1 = 0;
            newSummary.longAmount2 = 0;
            newSummary.doubleAmount1 = 0;
            newSummary.doubleAmount2 = 0;
        }
    }
}
