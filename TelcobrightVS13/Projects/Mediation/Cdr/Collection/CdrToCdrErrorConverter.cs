using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
using MediationModel;

namespace TelcobrightMediation.EntityHelpers
{
    //cdrerror, cdrinconsistent, cdrdiscarded have same properties
    //no need to handle more types other than these 3. Interface, factory etc. are not required.
    //if necessary cast convertedErrorCdr to discarded or inconsistent
    public static class CdrToCdrErrorConverter
    {
        public static cdrerror Convert(cdr sourceCdr)
        {
            Func<DateTime, string> dateToString = sourceDate =>
                                    sourceDate.ToMySqlStyleDateTimeStrWithoutQuote();
            Func<DateTime?, string> nullableDateToString = sourceDate =>
            {
                string strDate = "";
                strDate = sourceDate != null
                    ? dateToString(System.Convert.ToDateTime(sourceDate))
                    : "";
                return strDate;
            };
            return new cdrerror()//star with cdr error, then cast if necessary
            {
                SwitchId = sourceCdr.SwitchId.ToString(),
                idcall = sourceCdr.idcall.ToString(),
                SequenceNumber = sourceCdr.SequenceNumber.ToString(),
                FileName = sourceCdr.FileName.ToString(),
                CallDirection = sourceCdr.CallDirection.ToString(),
                incomingroute = sourceCdr.incomingroute.ToString(),
                OriginatingIP = sourceCdr.OriginatingIP.ToString(),
                OPC = sourceCdr.OPC.ToString(),
                OriginatingCIC = sourceCdr.OriginatingCIC.ToString(),
                OriginatingCalledNumber = sourceCdr.OriginatingCalledNumber.ToString(),
                TerminatingCalledNumber = sourceCdr.TerminatingCalledNumber.ToString(),
                OriginatingCallingNumber = sourceCdr.OriginatingCallingNumber.ToString(),
                TerminatingCallingNumber = sourceCdr.TerminatingCallingNumber.ToString(),
                CustomerPrePaid = sourceCdr.CustomerPrePaid.ToString(),
                DurationSec = sourceCdr.DurationSec.ToString(),
                EndTime = nullableDateToString(sourceCdr.EndTime),
                ConnectTime = nullableDateToString(sourceCdr.ConnectTime),
                AnswerTime = nullableDateToString(sourceCdr.AnswerTime),
                ChargingStatus = sourceCdr.ChargingStatus.ToString(),
                PDD = sourceCdr.PDD.ToString(),
                CountryCode = sourceCdr.CountryCode.ToString(),
                MinuteID = sourceCdr.MinuteID.ToString(),
                ReleaseDirection = sourceCdr.ReleaseDirection.ToString(),
                ReleaseCauseSystem = sourceCdr.ReleaseCauseSystem.ToString(),
                ReleaseCauseEgress = sourceCdr.ReleaseCauseEgress.ToString(),
                outgoingroute = sourceCdr.outgoingroute.ToString(),
                TerminatingIP = sourceCdr.TerminatingIP.ToString(),
                DPC = sourceCdr.DPC.ToString(),
                TerminatingCIC = sourceCdr.TerminatingCIC.ToString(),
                StartTime = sourceCdr.StartTime.ToMySqlStyleDateTimeStrWithoutQuote(),
                inPartnerId = sourceCdr.inPartnerId.ToString(),
                CustomerRate = sourceCdr.CustomerRate.ToString(),
                outPartnerId = sourceCdr.outPartnerId.ToString(),
                SupplierRate = sourceCdr.SupplierRate.ToString(),
                MatchedPrefixY = sourceCdr.MatchedPrefixY.ToString(),
                USDRateY = sourceCdr.USDRateY.ToString(),
                matchedprefixcustomer = sourceCdr.matchedprefixcustomer.ToString(),
                matchedprefixsupplier = sourceCdr.matchedprefixsupplier.ToString(),
                CustomerCost = sourceCdr.CustomerCost.ToString(),
                SupplierCost = sourceCdr.SupplierCost.ToString(),
                CostANSIn = sourceCdr.CostANSIn.ToString(),
                CostICXIn = sourceCdr.CostICXIn.ToString(),
                CostVATCommissionIn = sourceCdr.CostVATCommissionIn.ToString(),
                IGWRevenueIn = sourceCdr.IGWRevenueIn.ToString(),
                RevenueANSOut = sourceCdr.RevenueANSOut.ToString(),
                RevenueIGWOut = sourceCdr.RevenueIGWOut.ToString(),
                RevenueICXOut = sourceCdr.RevenueICXOut.ToString(),
                RevenueVATCommissionOut = sourceCdr.RevenueVATCommissionOut.ToString(),
                SubscriberChargeXOut = sourceCdr.SubscriberChargeXOut.ToString(),
                CarrierCostYIGWOut = sourceCdr.CarrierCostYIGWOut.ToString(),
                ANSPrefixOrig = sourceCdr.ANSPrefixOrig.ToString(),
                AnsIdOrig = sourceCdr.AnsIdOrig.ToString(),
                AnsPrefixTerm = sourceCdr.AnsPrefixTerm.ToString(),
                AnsIdTerm = sourceCdr.AnsIdTerm.ToString(),
                validflag = sourceCdr.validflag.ToString(),
                PartialFlag = sourceCdr.PartialFlag.ToString(),
                releasecauseingress = sourceCdr.releasecauseingress.ToString(),
                CustomerCallNumberANS = sourceCdr.CustomerCallNumberANS.ToString(),
                SupplierCallNumberANS = sourceCdr.SupplierCallNumberANS.ToString(),
                CalledPartyNOA = sourceCdr.CalledPartyNOA.ToString(),
                CallingPartyNOA = sourceCdr.CallingPartyNOA.ToString(),
                GrpDayId = sourceCdr.GrpDayId.ToString(),
                MonthId = sourceCdr.MonthId.ToString(),
                DayId = sourceCdr.DayId.ToString(),
                BTRCTermRate = sourceCdr.BTRCTermRate.ToString(),
                WeekDayId = sourceCdr.WeekDayId.ToString(),
                E1Id = sourceCdr.E1Id.ToString(),
                MediaIP1 = sourceCdr.MediaIP1.ToString(),
                MediaIP2 = sourceCdr.MediaIP2.ToString(),
                MediaIP3 = sourceCdr.MediaIP3.ToString(),
                MediaIP4 = sourceCdr.MediaIP4.ToString(),
                CallCancelDuration = sourceCdr.CallCancelDuration.ToString(),
                E1IdOut = sourceCdr.E1IdOut.ToString(),
                inTrunkAdditionalInfo = sourceCdr.inTrunkAdditionalInfo.ToString(),
                outTrunkAdditionalInfo = sourceCdr.outTrunkAdditionalInfo.ToString(),
                inMgwId = sourceCdr.inMgwId.ToString(),
                outMgwId = sourceCdr.outMgwId.ToString(),
                mediationcomplete = sourceCdr.mediationcomplete.ToString(),
                codec = sourceCdr.codec.ToString(),
                ConnectedNumberType = sourceCdr.ConnectedNumberType.ToString(),
                RedirectingNumber = sourceCdr.RedirectingNumber.ToString(),
                CallForwardOrRoamingType = sourceCdr.CallForwardOrRoamingType.ToString(),
                date1 = nullableDateToString(sourceCdr.date1),
                field1 = sourceCdr.field1.ToString(),
                field2 = sourceCdr.field2.ToString(),
                field3 = sourceCdr.field3.ToString(),
                errorCode = sourceCdr.errorCode.ToString(),
                field5 = sourceCdr.field5.ToString(),
                roundedduration = sourceCdr.roundedduration.ToString(),
                PartialDuration = sourceCdr.PartialDuration.ToString(),
                PartialAnswerTime = nullableDateToString(sourceCdr.PartialAnswerTime),
                PartialEndTime = sourceCdr.PartialEndTime.ToString(),
                FinalRecord = sourceCdr.FinalRecord.ToString(),
                Duration1 = sourceCdr.Duration1.ToString(),
                Duration2 = sourceCdr.Duration2.ToString(),
                Duration3 = sourceCdr.Duration3.ToString(),
                Duration4 = sourceCdr.Duration4.ToString(),
                PreviousPeriodCdr = sourceCdr.PreviousPeriodCdr.ToString(),
                UniqueBillId = sourceCdr.UniqueBillId.ToString(),
                BillngInfo = sourceCdr.BillngInfo.ToString(),
                Category = sourceCdr.Category.ToString(),
                SubCategory = sourceCdr.SubCategory.ToString(),
                ChangedByJobId = sourceCdr.ChangedByJobId.ToString(),
                ActualStartTime = nullableDateToString(sourceCdr.ActualStartTime)

            };
        }
    }
}
