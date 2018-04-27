using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
using MediationModel;
using TelcobrightFileOperations;

namespace TelcobrightMediation.Mediation.Cdr
{
    public static class CdrManipulatingUtil
    {
        public static void SetMissingAnswerWithStartTimeForAnsweredCdr(CdrSetting cdrSettings, ICdr cdrToValidate)
        {
            if (cdrToValidate.ChargingStatus != 1) return;
            if (cdrToValidate.AnswerTime == null)
            {
                cdrToValidate.AnswerTime = cdrToValidate.StartTime;
            }

        }

        public static cdr ConvertTxtRowToCdrOrInconsistentOnFailure(string[] txtRow,
            out cdrinconsistent cdrInconsistent)
        {
            cdrInconsistent = null;
            cdr convertedCdr = null;
            try
            {
                convertedCdr = new cdr(); //don't use obj initializer, difficult to debug with so many params
                convertedCdr.SwitchId = txtRow[Fn.Switchid].GetValue<int>();
                convertedCdr.idcall = txtRow[Fn.Idcall].GetValue<long>();
                convertedCdr.SequenceNumber = txtRow[Fn.Sequencenumber].GetValue<long>();
                convertedCdr.FileName = txtRow[Fn.Filename];
                convertedCdr.ServiceGroup = txtRow[Fn.ServiceGroup].GetValue<int>();
                convertedCdr.incomingroute = txtRow[Fn.Incomingroute];
                convertedCdr.OriginatingIP = txtRow[Fn.Originatingip];
                convertedCdr.OPC = txtRow[Fn.Opc].GetValueOrNull<int>();
                convertedCdr.OriginatingCIC = txtRow[Fn.Originatingcic].GetValueOrNull<int>();
                convertedCdr.OriginatingCalledNumber = txtRow[Fn.Originatingcallednumber];
                convertedCdr.TerminatingCalledNumber = txtRow[Fn.Terminatingcallednumber];
                convertedCdr.OriginatingCallingNumber = txtRow[Fn.Originatingcallingnumber];
                convertedCdr.TerminatingCallingNumber = txtRow[Fn.Terminatingcallingnumber];
                convertedCdr.CustomerPrePaid = txtRow[Fn.Customerprepaid].GetValueOrNull<byte>();
                convertedCdr.DurationSec = txtRow[Fn.Durationsec].GetValue<decimal>();
                convertedCdr.EndTime = txtRow[Fn.Endtime].ConvertToDateTimeFromMySqlFormat();
                convertedCdr.ConnectTime = txtRow[Fn.Connecttime].ConvertToNullableDateTimeFromMySqlFormat();
                convertedCdr.AnswerTime = txtRow[Fn.Answertime].ConvertToNullableDateTimeFromMySqlFormat();
                convertedCdr.ChargingStatus = txtRow[Fn.Chargingstatus].GetValueOrNull<byte>();
                convertedCdr.PDD = txtRow[Fn.Pdd].GetValueOrNull<Single>();
                convertedCdr.CountryCode = txtRow[Fn.Countrycode];
                convertedCdr.MinuteID = txtRow[Fn.Minuteid].GetValueOrNull<int>();
                convertedCdr.ReleaseDirection = txtRow[Fn.Releasedirection].GetValueOrNull<byte>();
                convertedCdr.ReleaseCauseSystem = txtRow[Fn.Releasecausesystem].GetValueOrNull<int>();
                convertedCdr.ReleaseCauseEgress = txtRow[Fn.Releasecauseegress].GetValueOrNull<int>();
                convertedCdr.outgoingroute = txtRow[Fn.Outgoingroute];
                convertedCdr.TerminatingIP = txtRow[Fn.Terminatingip];
                convertedCdr.DPC = txtRow[Fn.Dpc].GetValueOrNull<int>();
                convertedCdr.TerminatingCIC = txtRow[Fn.Terminatingcic].GetValueOrNull<int>();
                convertedCdr.StartTime = txtRow[Fn.Starttime].ConvertToDateTimeFromMySqlFormat();
                convertedCdr.inPartnerId = txtRow[Fn.InPartnerId].GetValueOrNull<int>();
                convertedCdr.CustomerRate = txtRow[Fn.Customerrate].GetValueOrNull<decimal>();
                convertedCdr.outPartnerId = txtRow[Fn.OutPartnerId].GetValueOrNull<int>();
                convertedCdr.SupplierRate = txtRow[Fn.Supplierrate].GetValueOrNull<decimal>();
                convertedCdr.MatchedPrefixY = txtRow[Fn.Matchedprefixy];
                convertedCdr.USDRateY = txtRow[Fn.Usdratey].GetValueOrNull<decimal>();
                convertedCdr.matchedprefixcustomer = txtRow[Fn.Matchedprefixcustomer];
                convertedCdr.matchedprefixsupplier = txtRow[Fn.Matchedprefixsupplier];
                convertedCdr.CustomerCost = System.Convert.ToDecimal(txtRow[Fn.Customercost].GetValueOrNull<double>());
                convertedCdr.SupplierCost = System.Convert.ToDecimal(txtRow[Fn.Suppliercost].GetValueOrNull<double>());
                convertedCdr.CostANSIn = System.Convert.ToDecimal(txtRow[Fn.Costansin].GetValueOrNull<double>());
                convertedCdr.CostICXIn = System.Convert.ToDecimal(txtRow[Fn.Costicxin].GetValueOrNull<double>());
                convertedCdr.CostVATCommissionIn = System.Convert.ToDecimal(txtRow[Fn.Costvatcommissionin].GetValueOrNull<double>());
                convertedCdr.IGWRevenueIn = System.Convert.ToDecimal(txtRow[Fn.Igwrevenuein].GetValueOrNull<double>());
                convertedCdr.RevenueANSOut = System.Convert.ToDecimal(txtRow[Fn.Revenueansout].GetValueOrNull<double>());
                convertedCdr.RevenueIGWOut = System.Convert.ToDecimal(txtRow[Fn.Revenueigwout].GetValueOrNull<double>());
                convertedCdr.RevenueICXOut = System.Convert.ToDecimal(txtRow[Fn.Revenueicxout].GetValueOrNull<double>());
                convertedCdr.RevenueVATCommissionOut = System.Convert.ToDecimal(txtRow[Fn.Revenuevatcommissionout].GetValueOrNull<double>());
                convertedCdr.SubscriberChargeXOut = System.Convert.ToDecimal(txtRow[Fn.Subscriberchargexout].GetValueOrNull<double>());
                convertedCdr.CarrierCostYIGWOut = System.Convert.ToDecimal(txtRow[Fn.Carriercostyigwout].GetValueOrNull<double>());
                convertedCdr.ANSPrefixOrig = txtRow[Fn.Ansprefixorig];
                convertedCdr.AnsIdOrig = txtRow[Fn.Ansidorig].GetValueOrNull<int>();
                convertedCdr.AnsPrefixTerm = txtRow[Fn.Ansprefixterm];
                convertedCdr.AnsIdTerm = txtRow[Fn.Ansidterm].GetValueOrNull<int>();
                convertedCdr.validflag = txtRow[Fn.Validflag].GetValueOrNull<int>();
                convertedCdr.PartialFlag = txtRow[Fn.Partialflag].GetValueOrNull<sbyte>();
                convertedCdr.releasecauseingress = txtRow[Fn.Releasecauseingress].GetValueOrNull<int>();
                convertedCdr.CustomerCallNumberANS = txtRow[Fn.Customercallnumberans].GetValueOrNull<long>();
                convertedCdr.SupplierCallNumberANS = txtRow[Fn.Suppliercallnumberans].GetValueOrNull<long>();
                convertedCdr.CalledPartyNOA = txtRow[Fn.Calledpartynoa].GetValueOrNull<byte>();
                convertedCdr.CallingPartyNOA = txtRow[Fn.Callingpartynoa].GetValueOrNull<byte>();
                convertedCdr.GrpDayId = txtRow[Fn.Grpdayid].GetValueOrNull<long>();
                convertedCdr.MonthId = txtRow[Fn.Monthid].GetValueOrNull<byte>();
                convertedCdr.DayId = txtRow[Fn.Dayid].GetValueOrNull<int>();
                convertedCdr.BTRCTermRate = txtRow[Fn.Btrctermrate].GetValueOrNull<double>();
                convertedCdr.WeekDayId = txtRow[Fn.Weekdayid].GetValueOrNull<int>();
                convertedCdr.E1Id = txtRow[Fn.E1Id].GetValueOrNull<int>();
                convertedCdr.MediaIP1 = txtRow[Fn.Mediaip1];
                convertedCdr.MediaIP2 = txtRow[Fn.Mediaip2];
                convertedCdr.MediaIP3 = txtRow[Fn.Mediaip3];
                convertedCdr.MediaIP4 = txtRow[Fn.Mediaip4];
                convertedCdr.CallCancelDuration = txtRow[Fn.Callcancelduration].GetValueOrNull<double>();
                convertedCdr.E1IdOut = txtRow[Fn.E1Idout].GetValueOrNull<int>();
                convertedCdr.inTrunkAdditionalInfo = txtRow[Fn.InTrunkAdditionalInfo];
                convertedCdr.outTrunkAdditionalInfo = txtRow[Fn.OutTrunkAdditionalInfo];
                convertedCdr.inMgwId = txtRow[Fn.InMgwId];
                convertedCdr.outMgwId = txtRow[Fn.OutMgwId];
                convertedCdr.mediationcomplete = txtRow[Fn.Mediationcomplete].GetValueOrNull<sbyte>();
                convertedCdr.codec = txtRow[Fn.Codec];
                convertedCdr.ConnectedNumberType = txtRow[Fn.Connectednumbertype].GetValueOrNull<byte>();
                convertedCdr.RedirectingNumber = txtRow[Fn.Redirectingnumber];
                convertedCdr.CallForwardOrRoamingType = txtRow[Fn.Callforwardorroamingtype].GetValueOrNull<byte>();
                convertedCdr.date1 = txtRow[Fn.Date1].ConvertToNullableDateTimeFromMySqlFormat();
                convertedCdr.field1 = txtRow[Fn.Field1].GetValueOrNull<int>();
                convertedCdr.field2 = txtRow[Fn.Field2].GetValueOrNull<int>();
                convertedCdr.field3 = txtRow[Fn.Field3].GetValueOrNull<int>();
                convertedCdr.errorCode = txtRow[Fn.Field4];
                convertedCdr.field5 = txtRow[Fn.ConnectedFlagByCauseCodeForCdrIndicatedInField5].GetValueOrNull<int>();
                convertedCdr.roundedduration = txtRow[Fn.Roundedduration].GetValueOrNull<decimal>();
                convertedCdr.PartialDuration = txtRow[Fn.Partialduration].GetValueOrNull<decimal>();
                convertedCdr.PartialAnswerTime = txtRow[Fn.Partialanswertime]
                    .ConvertToNullableDateTimeFromMySqlFormat();
                convertedCdr.PartialEndTime = txtRow[Fn.Partialendtime].ConvertToNullableDateTimeFromMySqlFormat();
                convertedCdr.FinalRecord = txtRow[Fn.Finalrecord].GetValueOrNull<long>();
                convertedCdr.Duration1 = txtRow[Fn.Duration1].GetValueOrNull<decimal>();
                convertedCdr.Duration2 = txtRow[Fn.Duration2].GetValueOrNull<decimal>();
                convertedCdr.Duration3 = txtRow[Fn.Duration3].GetValueOrNull<decimal>();
                convertedCdr.Duration4 = txtRow[Fn.Duration4].GetValueOrNull<decimal>();
                convertedCdr.PreviousPeriodCdr = txtRow[Fn.Previousperiodcdr].GetValueOrNull<int>();
                convertedCdr.UniqueBillId = txtRow[Fn.Uniquebillid];
                convertedCdr.BillngInfo = txtRow[Fn.Billnginfo];
                convertedCdr.Category = txtRow[Fn.Category].GetValueOrNull<int>();
                convertedCdr.SubCategory = txtRow[Fn.Subcategory].GetValueOrNull<int>();
                convertedCdr.ChangedByJobId = txtRow[Fn.Changedbyjobid].GetValueOrNull<long>();
                convertedCdr.ActualStartTime = txtRow[Fn.Actualstarttime].ConvertToDateTimeFromMySqlFormat();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                new ErrorWriter(e, "CdrJobExecuter", null,
                    "Could not convert text row to cdr", "");
                cdrInconsistent = ConvertTxtRowToCdrinconsistent(txtRow);
                cdrInconsistent.errorCode = e.Message;
                return null;
            }
            return convertedCdr;
        }

        public static cdrinconsistent ConvertTxtRowToCdrinconsistent(string[] txtRow)
        {
            cdrinconsistent inconsistentCdr = new cdrinconsistent();
            inconsistentCdr.SwitchId = txtRow[Fn.Switchid];
            inconsistentCdr.idcall = System.Convert.ToInt64(txtRow[Fn.Idcall]);
            inconsistentCdr.SequenceNumber = txtRow[Fn.Sequencenumber];
            inconsistentCdr.FileName = txtRow[Fn.Filename];
            inconsistentCdr.ServiceGroup = txtRow[Fn.ServiceGroup];
            inconsistentCdr.incomingroute = txtRow[Fn.Incomingroute];
            inconsistentCdr.OriginatingIP = txtRow[Fn.Originatingip];
            inconsistentCdr.OPC = txtRow[Fn.Opc];
            inconsistentCdr.OriginatingCIC = txtRow[Fn.Originatingcic];
            inconsistentCdr.OriginatingCalledNumber = txtRow[Fn.Originatingcallednumber];
            inconsistentCdr.TerminatingCalledNumber = txtRow[Fn.Terminatingcallednumber];
            inconsistentCdr.OriginatingCallingNumber = txtRow[Fn.Originatingcallingnumber];
            inconsistentCdr.TerminatingCallingNumber = txtRow[Fn.Terminatingcallingnumber];
            inconsistentCdr.CustomerPrePaid = txtRow[Fn.Customerprepaid];
            inconsistentCdr.DurationSec = txtRow[Fn.Durationsec];
            inconsistentCdr.EndTime = txtRow[Fn.Endtime];
            inconsistentCdr.ConnectTime = txtRow[Fn.Connecttime];
            inconsistentCdr.AnswerTime = txtRow[Fn.Answertime];
            inconsistentCdr.ChargingStatus = txtRow[Fn.Chargingstatus];
            inconsistentCdr.PDD = txtRow[Fn.Pdd];
            inconsistentCdr.CountryCode = txtRow[Fn.Countrycode];
            inconsistentCdr.MinuteID = txtRow[Fn.Minuteid];
            inconsistentCdr.ReleaseDirection = txtRow[Fn.Releasedirection];
            inconsistentCdr.ReleaseCauseSystem = txtRow[Fn.Releasecausesystem];
            inconsistentCdr.ReleaseCauseEgress = txtRow[Fn.Releasecauseegress];
            inconsistentCdr.outgoingroute = txtRow[Fn.Outgoingroute];
            inconsistentCdr.TerminatingIP = txtRow[Fn.Terminatingip];
            inconsistentCdr.DPC = txtRow[Fn.Dpc];
            inconsistentCdr.TerminatingCIC = txtRow[Fn.Terminatingcic];
            inconsistentCdr.StartTime = txtRow[Fn.Starttime];
            inconsistentCdr.inPartnerId = txtRow[Fn.InPartnerId];
            inconsistentCdr.CustomerRate = txtRow[Fn.Customerrate];
            inconsistentCdr.outPartnerId = txtRow[Fn.OutPartnerId];
            inconsistentCdr.SupplierRate = txtRow[Fn.Supplierrate];
            inconsistentCdr.MatchedPrefixY = txtRow[Fn.Matchedprefixy];
            inconsistentCdr.USDRateY = txtRow[Fn.Usdratey];
            inconsistentCdr.matchedprefixcustomer = txtRow[Fn.Matchedprefixcustomer];
            inconsistentCdr.matchedprefixsupplier = txtRow[Fn.Matchedprefixsupplier];
            inconsistentCdr.CustomerCost = txtRow[Fn.Customercost];
            inconsistentCdr.SupplierCost = txtRow[Fn.Suppliercost];
            inconsistentCdr.CostANSIn = txtRow[Fn.Costansin];
            inconsistentCdr.CostICXIn = txtRow[Fn.Costicxin];
            inconsistentCdr.CostVATCommissionIn = txtRow[Fn.Costvatcommissionin];
            inconsistentCdr.IGWRevenueIn = txtRow[Fn.Igwrevenuein];
            inconsistentCdr.RevenueANSOut = txtRow[Fn.Revenueansout];
            inconsistentCdr.RevenueIGWOut = txtRow[Fn.Revenueigwout];
            inconsistentCdr.RevenueICXOut = txtRow[Fn.Revenueicxout];
            inconsistentCdr.RevenueVATCommissionOut = txtRow[Fn.Revenuevatcommissionout];
            inconsistentCdr.SubscriberChargeXOut = txtRow[Fn.Subscriberchargexout];
            inconsistentCdr.CarrierCostYIGWOut = txtRow[Fn.Carriercostyigwout];
            inconsistentCdr.ANSPrefixOrig = txtRow[Fn.Ansprefixorig];
            inconsistentCdr.AnsIdOrig = txtRow[Fn.Ansidorig];
            inconsistentCdr.AnsPrefixTerm = txtRow[Fn.Ansprefixterm];
            inconsistentCdr.AnsIdTerm = txtRow[Fn.Ansidterm];
            inconsistentCdr.validflag = txtRow[Fn.Validflag];
            inconsistentCdr.PartialFlag = txtRow[Fn.Partialflag];
            inconsistentCdr.releasecauseingress = txtRow[Fn.Releasecauseingress];
            inconsistentCdr.CustomerCallNumberANS = txtRow[Fn.Customercallnumberans];
            inconsistentCdr.SupplierCallNumberANS = txtRow[Fn.Suppliercallnumberans];
            inconsistentCdr.CalledPartyNOA = txtRow[Fn.Calledpartynoa];
            inconsistentCdr.CallingPartyNOA = txtRow[Fn.Callingpartynoa];
            inconsistentCdr.MonthId = txtRow[Fn.Monthid];
            inconsistentCdr.DayId = txtRow[Fn.Dayid];
            inconsistentCdr.BTRCTermRate = txtRow[Fn.Btrctermrate];
            inconsistentCdr.WeekDayId = txtRow[Fn.Weekdayid];
            inconsistentCdr.E1Id = txtRow[Fn.E1Id];
            inconsistentCdr.MediaIP1 = txtRow[Fn.Mediaip1];
            inconsistentCdr.MediaIP2 = txtRow[Fn.Mediaip2];
            inconsistentCdr.MediaIP3 = txtRow[Fn.Mediaip3];
            inconsistentCdr.MediaIP4 = txtRow[Fn.Mediaip4];
            inconsistentCdr.CallCancelDuration = txtRow[Fn.Callcancelduration];
            inconsistentCdr.E1IdOut = txtRow[Fn.E1Idout];
            inconsistentCdr.inTrunkAdditionalInfo = txtRow[Fn.InTrunkAdditionalInfo];
            inconsistentCdr.outTrunkAdditionalInfo = txtRow[Fn.OutTrunkAdditionalInfo];
            inconsistentCdr.inMgwId = txtRow[Fn.InMgwId];
            inconsistentCdr.outMgwId = txtRow[Fn.OutMgwId];
            inconsistentCdr.mediationcomplete = txtRow[Fn.Mediationcomplete];
            inconsistentCdr.codec = txtRow[Fn.Codec];
            inconsistentCdr.ConnectedNumberType = txtRow[Fn.Connectednumbertype];
            inconsistentCdr.RedirectingNumber = txtRow[Fn.Redirectingnumber];
            inconsistentCdr.CallForwardOrRoamingType = txtRow[Fn.Callforwardorroamingtype];
            inconsistentCdr.date1 = txtRow[Fn.Date1];
            inconsistentCdr.field1 = txtRow[Fn.Field1];
            inconsistentCdr.field2 = txtRow[Fn.Field2];
            inconsistentCdr.field3 = txtRow[Fn.Field3];
            inconsistentCdr.errorCode = txtRow[Fn.Field4];
            inconsistentCdr.field5 = txtRow[Fn.ConnectedFlagByCauseCodeForCdrIndicatedInField5];
            inconsistentCdr.roundedduration = txtRow[Fn.Roundedduration];
            inconsistentCdr.PartialDuration = txtRow[Fn.Partialduration];
            inconsistentCdr.PartialAnswerTime = txtRow[Fn.Partialanswertime];
            inconsistentCdr.PartialEndTime = txtRow[Fn.Partialendtime];
            inconsistentCdr.FinalRecord = txtRow[Fn.Finalrecord];
            inconsistentCdr.Duration1 = txtRow[Fn.Duration1];
            inconsistentCdr.Duration2 = txtRow[Fn.Duration2];
            inconsistentCdr.Duration3 = txtRow[Fn.Duration3];
            inconsistentCdr.Duration4 = txtRow[Fn.Duration4];
            inconsistentCdr.PreviousPeriodCdr = txtRow[Fn.Previousperiodcdr];
            inconsistentCdr.UniqueBillId = txtRow[Fn.Uniquebillid];
            inconsistentCdr.BillngInfo = txtRow[Fn.Billnginfo];
            inconsistentCdr.Category = txtRow[Fn.Category];
            inconsistentCdr.SubCategory = txtRow[Fn.Subcategory];
            inconsistentCdr.ChangedByJobId = txtRow[Fn.Changedbyjobid];
            inconsistentCdr.ActualStartTime = txtRow[Fn.Actualstarttime];
            return inconsistentCdr;
        }

        public static ICdr Clone(ICdr sourceCdr)
        {
            ICdr newInstance = new cdr();
            newInstance.SwitchId = sourceCdr.SwitchId;
            newInstance.idcall = sourceCdr.idcall;
            newInstance.SequenceNumber = sourceCdr.SequenceNumber;
            newInstance.FileName = sourceCdr.FileName;
            newInstance.ServiceGroup = sourceCdr.ServiceGroup;
            newInstance.incomingroute = sourceCdr.incomingroute;
            newInstance.OriginatingIP = sourceCdr.OriginatingIP;
            newInstance.OPC = sourceCdr.OPC;
            newInstance.OriginatingCIC = sourceCdr.OriginatingCIC;
            newInstance.OriginatingCalledNumber = sourceCdr.OriginatingCalledNumber;
            newInstance.TerminatingCalledNumber = sourceCdr.TerminatingCalledNumber;
            newInstance.OriginatingCallingNumber = sourceCdr.OriginatingCallingNumber;
            newInstance.TerminatingCallingNumber = sourceCdr.TerminatingCallingNumber;
            newInstance.CustomerPrePaid = sourceCdr.CustomerPrePaid;
            newInstance.DurationSec = sourceCdr.DurationSec;
            newInstance.EndTime = sourceCdr.EndTime;
            newInstance.ConnectTime = sourceCdr.ConnectTime;
            newInstance.AnswerTime = sourceCdr.AnswerTime;
            newInstance.ChargingStatus = sourceCdr.ChargingStatus;
            newInstance.PDD = sourceCdr.PDD;
            newInstance.CountryCode = sourceCdr.CountryCode;
            newInstance.MinuteID = sourceCdr.MinuteID;
            newInstance.ReleaseDirection = sourceCdr.ReleaseDirection;
            newInstance.ReleaseCauseSystem = sourceCdr.ReleaseCauseSystem;
            newInstance.ReleaseCauseEgress = sourceCdr.ReleaseCauseEgress;
            newInstance.outgoingroute = sourceCdr.outgoingroute;
            newInstance.TerminatingIP = sourceCdr.TerminatingIP;
            newInstance.DPC = sourceCdr.DPC;
            newInstance.TerminatingCIC = sourceCdr.TerminatingCIC;
            newInstance.StartTime = sourceCdr.StartTime;
            newInstance.inPartnerId = sourceCdr.inPartnerId;
            newInstance.CustomerRate = sourceCdr.CustomerRate;
            newInstance.outPartnerId = sourceCdr.outPartnerId;
            newInstance.SupplierRate = sourceCdr.SupplierRate;
            newInstance.MatchedPrefixY = sourceCdr.MatchedPrefixY;
            newInstance.USDRateY = sourceCdr.USDRateY;
            newInstance.matchedprefixcustomer = sourceCdr.matchedprefixcustomer;
            newInstance.matchedprefixsupplier = sourceCdr.matchedprefixsupplier;
            newInstance.CustomerCost = sourceCdr.CustomerCost;
            newInstance.SupplierCost = sourceCdr.SupplierCost;
            newInstance.CostANSIn = sourceCdr.CostANSIn;
            newInstance.CostICXIn = sourceCdr.CostICXIn;
            newInstance.CostVATCommissionIn = sourceCdr.CostVATCommissionIn;
            newInstance.IGWRevenueIn = sourceCdr.IGWRevenueIn;
            newInstance.RevenueANSOut = sourceCdr.RevenueANSOut;
            newInstance.RevenueIGWOut = sourceCdr.RevenueIGWOut;
            newInstance.RevenueICXOut = sourceCdr.RevenueICXOut;
            newInstance.RevenueVATCommissionOut = sourceCdr.RevenueVATCommissionOut;
            newInstance.SubscriberChargeXOut = sourceCdr.SubscriberChargeXOut;
            newInstance.CarrierCostYIGWOut = sourceCdr.CarrierCostYIGWOut;
            newInstance.ANSPrefixOrig = sourceCdr.ANSPrefixOrig;
            newInstance.AnsIdOrig = sourceCdr.AnsIdOrig;
            newInstance.AnsPrefixTerm = sourceCdr.AnsPrefixTerm;
            newInstance.AnsIdTerm = sourceCdr.AnsIdTerm;
            newInstance.validflag = sourceCdr.validflag;
            newInstance.PartialFlag = sourceCdr.PartialFlag;
            newInstance.releasecauseingress = sourceCdr.releasecauseingress;
            newInstance.CustomerCallNumberANS = sourceCdr.CustomerCallNumberANS;
            newInstance.SupplierCallNumberANS = sourceCdr.SupplierCallNumberANS;
            newInstance.CalledPartyNOA = sourceCdr.CalledPartyNOA;
            newInstance.CallingPartyNOA = sourceCdr.CallingPartyNOA;
            newInstance.GrpDayId = sourceCdr.GrpDayId;
            newInstance.MonthId = sourceCdr.MonthId;
            newInstance.DayId = sourceCdr.DayId;
            newInstance.BTRCTermRate = sourceCdr.BTRCTermRate;
            newInstance.WeekDayId = sourceCdr.WeekDayId;
            newInstance.E1Id = sourceCdr.E1Id;
            newInstance.MediaIP1 = sourceCdr.MediaIP1;
            newInstance.MediaIP2 = sourceCdr.MediaIP2;
            newInstance.MediaIP3 = sourceCdr.MediaIP3;
            newInstance.MediaIP4 = sourceCdr.MediaIP4;
            newInstance.CallCancelDuration = sourceCdr.CallCancelDuration;
            newInstance.E1IdOut = sourceCdr.E1IdOut;
            newInstance.inTrunkAdditionalInfo = sourceCdr.inTrunkAdditionalInfo;
            newInstance.outTrunkAdditionalInfo = sourceCdr.outTrunkAdditionalInfo;
            newInstance.inMgwId = sourceCdr.inMgwId;
            newInstance.outMgwId = sourceCdr.outMgwId;
            newInstance.mediationcomplete = sourceCdr.mediationcomplete;
            newInstance.codec = sourceCdr.codec;
            newInstance.ConnectedNumberType = sourceCdr.ConnectedNumberType;
            newInstance.RedirectingNumber = sourceCdr.RedirectingNumber;
            newInstance.CallForwardOrRoamingType = sourceCdr.CallForwardOrRoamingType;
            newInstance.date1 = sourceCdr.date1;
            newInstance.field1 = sourceCdr.field1;
            newInstance.field2 = sourceCdr.field2;
            newInstance.field3 = sourceCdr.field3;
            newInstance.errorCode = sourceCdr.errorCode;
            newInstance.field5 = sourceCdr.field5;
            newInstance.roundedduration = sourceCdr.roundedduration;
            newInstance.PartialDuration = sourceCdr.PartialDuration;
            newInstance.PartialAnswerTime = sourceCdr.PartialAnswerTime;
            newInstance.PartialEndTime = sourceCdr.PartialEndTime;
            newInstance.FinalRecord = sourceCdr.FinalRecord;
            newInstance.Duration1 = sourceCdr.Duration1;
            newInstance.Duration2 = sourceCdr.Duration2;
            newInstance.Duration3 = sourceCdr.Duration3;
            newInstance.Duration4 = sourceCdr.Duration4;
            newInstance.PreviousPeriodCdr = sourceCdr.PreviousPeriodCdr;
            newInstance.UniqueBillId = sourceCdr.UniqueBillId;
            newInstance.BillngInfo = sourceCdr.BillngInfo;
            newInstance.Category = sourceCdr.Category;
            newInstance.SubCategory = sourceCdr.SubCategory;
            newInstance.ChangedByJobId = sourceCdr.ChangedByJobId;
            newInstance.ActualStartTime = sourceCdr.ActualStartTime;
            return newInstance;
        }

        public static cdrerror ConvertCdrToCdrError(ICdr sourceCdr)
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
                ServiceGroup = sourceCdr.ServiceGroup.ToString(),
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
