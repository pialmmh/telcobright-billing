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
                convertedCdr.IdCall = txtRow[Fn.IdCall].GetValue<long>();
                convertedCdr.SequenceNumber = txtRow[Fn.Sequencenumber].GetValue<long>();
                convertedCdr.FileName = txtRow[Fn.Filename];
                convertedCdr.ServiceGroup = txtRow[Fn.ServiceGroup].GetValue<int>();
                convertedCdr.IncomingRoute = txtRow[Fn.IncomingRoute];
                convertedCdr.OriginatingIP = txtRow[Fn.Originatingip];
                convertedCdr.OPC = txtRow[Fn.Opc].GetValueOrNull<int>();
                convertedCdr.OriginatingCIC = txtRow[Fn.OriginatingCIC].GetValueOrNull<int>();
                convertedCdr.OriginatingCalledNumber = txtRow[Fn.OriginatingCalledNumber];
                convertedCdr.TerminatingCalledNumber = txtRow[Fn.TerminatingCalledNumber];
                convertedCdr.OriginatingCallingNumber = txtRow[Fn.OriginatingCallingNumber];
                convertedCdr.TerminatingCallingNumber = txtRow[Fn.TerminatingCallingNumber];
                convertedCdr.PrePaid = txtRow[Fn.PrePaid].GetValueOrNull<byte>();
                convertedCdr.DurationSec = txtRow[Fn.DurationSec].GetValue<decimal>();
                convertedCdr.EndTime = txtRow[Fn.Endtime].ConvertToDateTimeFromMySqlFormat();
                convertedCdr.ConnectTime = txtRow[Fn.ConnectTime].ConvertToNullableDateTimeFromMySqlFormat();
                convertedCdr.AnswerTime = txtRow[Fn.AnswerTime].ConvertToNullableDateTimeFromMySqlFormat();
                convertedCdr.ChargingStatus = txtRow[Fn.ChargingStatus].GetValueOrNull<byte>();
                convertedCdr.PDD = txtRow[Fn.Pdd].GetValueOrNull<Single>();
                convertedCdr.CountryCode = txtRow[Fn.CountryCode];
                convertedCdr.AreaCodeOrLata = txtRow[Fn.AreaCodeOrLata];
                convertedCdr.ReleaseDirection = txtRow[Fn.ReleaseDirection].GetValueOrNull<byte>();
                convertedCdr.ReleaseCauseSystem = txtRow[Fn.ReleaseCauseSystem].GetValueOrNull<int>();
                convertedCdr.ReleaseCauseEgress = txtRow[Fn.ReleaseCauseEgress].GetValueOrNull<int>();
                convertedCdr.OutgoingRoute = txtRow[Fn.OutgoingRoute];
                convertedCdr.TerminatingIP = txtRow[Fn.TerminatingIp];
                convertedCdr.DPC = txtRow[Fn.Dpc].GetValueOrNull<int>();
                convertedCdr.TerminatingCIC = txtRow[Fn.TerminatingCIC].GetValueOrNull<int>();
                convertedCdr.StartTime = txtRow[Fn.StartTime].ConvertToDateTimeFromMySqlFormat();
                convertedCdr.InPartnerId = txtRow[Fn.InPartnerId].GetValueOrNull<int>();
                convertedCdr.CustomerRate = txtRow[Fn.CustomerRate].GetValueOrNull<decimal>();
                convertedCdr.OutPartnerId = txtRow[Fn.OutPartnerId].GetValueOrNull<int>();
                convertedCdr.SupplierRate = txtRow[Fn.SupplierRate].GetValueOrNull<decimal>();
                convertedCdr.MatchedPrefixY = txtRow[Fn.MatchedPrefixY];
                convertedCdr.UsdRateY = txtRow[Fn.UsdRateY].GetValueOrNull<decimal>();
                convertedCdr.MatchedPrefixCustomer = txtRow[Fn.MatchedPrefixCustomer];
                convertedCdr.MatchedPrefixSupplier = txtRow[Fn.MatchedPrefixSupplier];
                convertedCdr.InPartnerCost = System.Convert.ToDecimal(txtRow[Fn.InPartnerCost].GetValueOrNull<double>());
                convertedCdr.OutPartnerCost = System.Convert.ToDecimal(txtRow[Fn.OutPartnerCost].GetValueOrNull<double>());
                convertedCdr.CostAnsIn = System.Convert.ToDecimal(txtRow[Fn.CostAnsIn].GetValueOrNull<double>());
                convertedCdr.CostIcxIn = System.Convert.ToDecimal(txtRow[Fn.CostIcxIn].GetValueOrNull<double>());
                convertedCdr.Tax1 = System.Convert.ToDecimal(txtRow[Fn.Tax1].GetValueOrNull<double>());
                convertedCdr.IgwRevenueIn = System.Convert.ToDecimal(txtRow[Fn.IgwRevenueIn].GetValueOrNull<double>());
                convertedCdr.RevenueAnsOut = System.Convert.ToDecimal(txtRow[Fn.RevenueAnsOut].GetValueOrNull<double>());
                convertedCdr.RevenueIgwOut = System.Convert.ToDecimal(txtRow[Fn.RevenueIgwOut].GetValueOrNull<double>());
                convertedCdr.RevenueIcxOut = System.Convert.ToDecimal(txtRow[Fn.RevenueIcxOut].GetValueOrNull<double>());
                convertedCdr.Tax2 = System.Convert.ToDecimal(txtRow[Fn.Tax2].GetValueOrNull<double>());
                convertedCdr.XAmount = System.Convert.ToDecimal(txtRow[Fn.XAmount].GetValueOrNull<double>());
                convertedCdr.YAmount = System.Convert.ToDecimal(txtRow[Fn.YAmount].GetValueOrNull<double>());
                convertedCdr.AnsPrefixOrig = txtRow[Fn.AnsPrefixOrig];
                convertedCdr.AnsIdOrig = txtRow[Fn.AnsIdOrig].GetValueOrNull<int>();
                convertedCdr.AnsPrefixTerm = txtRow[Fn.AnsPrefixTerm];
                convertedCdr.AnsIdTerm = txtRow[Fn.AnsIdTerm].GetValueOrNull<int>();
                convertedCdr.ValidFlag = txtRow[Fn.Validflag].GetValueOrNull<int>();
                convertedCdr.PartialFlag = txtRow[Fn.Partialflag].GetValueOrNull<sbyte>();
                convertedCdr.ReleaseCauseIngress = txtRow[Fn.ReleaseCauseIngress].GetValueOrNull<int>();
                convertedCdr.InRoamingOpId = txtRow[Fn.InRoamingOpId].GetValueOrNull<int    >();
                convertedCdr.OutRoamingOpId = txtRow[Fn.OutRoamingOpId].GetValueOrNull<int>();
                convertedCdr.CalledPartyNOA = txtRow[Fn.CalledpartyNOA].GetValueOrNull<byte>();
                convertedCdr.CallingPartyNOA = txtRow[Fn.CallingPartyNOA].GetValueOrNull<byte>();
                convertedCdr.AdditionalSystemCodes = txtRow[Fn.AdditionalSystemCodes];
                convertedCdr.AdditionalPartyNumber = txtRow[Fn.AdditionalPartyNumber];
                convertedCdr.ResellerIds = txtRow[Fn.ResellerIds];
                convertedCdr.ZAmount = txtRow[Fn.ZAmount].GetValueOrNull<decimal>();
                convertedCdr.PreviousRoutes = txtRow[Fn.PreviousRoutes];
                convertedCdr.E1Id = txtRow[Fn.E1Id].GetValueOrNull<int>();
                convertedCdr.MediaIp1 = txtRow[Fn.Mediaip1];
                convertedCdr.MediaIp2 = txtRow[Fn.Mediaip2];
                convertedCdr.MediaIp3 = txtRow[Fn.Mediaip3];
                convertedCdr.MediaIp4 = txtRow[Fn.Mediaip4];
                convertedCdr.CallReleaseDuration = txtRow[Fn.CallReleaseduration].GetValueOrNull<Single>();
                convertedCdr.E1IdOut = txtRow[Fn.E1Idout].GetValueOrNull<int>();
                convertedCdr.InTrunkAdditionalInfo = txtRow[Fn.InTrunkAdditionalInfo];
                convertedCdr.OutTrunkAdditionalInfo = txtRow[Fn.OutTrunkAdditionalInfo];
                convertedCdr.InMgwId = txtRow[Fn.InMgwId];
                convertedCdr.OutMgwId = txtRow[Fn.OutMgwId];
                convertedCdr.MediationComplete = txtRow[Fn.MediationComplete].GetValueOrNull<sbyte>();
                convertedCdr.Codec = txtRow[Fn.Codec];
                convertedCdr.ConnectedNumberType = txtRow[Fn.Connectednumbertype].GetValueOrNull<byte>();
                convertedCdr.RedirectingNumber = txtRow[Fn.Redirectingnumber];
                convertedCdr.CallForwardOrRoamingType = txtRow[Fn.Callforwardorroamingtype].GetValueOrNull<byte>();
                convertedCdr.OtherDate = txtRow[Fn.OtherDate].ConvertToNullableDateTimeFromMySqlFormat();
                convertedCdr.SummaryMetaTotal = txtRow[Fn.SummaryMetaTotal].GetValueOrNull<decimal>();
                convertedCdr.TransactionMetaTotal = txtRow[Fn.TransactionMetaTotal].GetValueOrNull<decimal>();
                convertedCdr.ChargeableMetaTotal = txtRow[Fn.ChargeableMetaTotal].GetValueOrNull<decimal>();
                convertedCdr.ErrorCode = txtRow[Fn.ErrorCode];
                convertedCdr.NERSuccess = txtRow[Fn.NERSuccess].GetValueOrNull<int>();
                convertedCdr.RoundedDuration = txtRow[Fn.RoundedDuration].GetValueOrNull<decimal>();
                convertedCdr.PartialDuration = txtRow[Fn.PartialDuration].GetValueOrNull<decimal>();
                convertedCdr.PartialAnswerTime = txtRow[Fn.PartialAnswertime]
                    .ConvertToNullableDateTimeFromMySqlFormat();
                convertedCdr.PartialEndTime = txtRow[Fn.PartialEndtime].ConvertToNullableDateTimeFromMySqlFormat();
                convertedCdr.FinalRecord = txtRow[Fn.FinalRecord].GetValueOrNull<long>();
                convertedCdr.Duration1 = txtRow[Fn.Duration1].GetValueOrNull<decimal>();
                convertedCdr.Duration2 = txtRow[Fn.Duration2].GetValueOrNull<decimal>();
                convertedCdr.Duration3 = txtRow[Fn.Duration3].GetValueOrNull<decimal>();
                convertedCdr.Duration4 = txtRow[Fn.Duration4].GetValueOrNull<decimal>();
                convertedCdr.PreviousPeriodCdr = txtRow[Fn.PreviousPeriodCdr].GetValueOrNull<int>();
                convertedCdr.UniqueBillId = txtRow[Fn.UniqueBillId];
                convertedCdr.AdditionalMetaData = txtRow[Fn.AdditionalMetaData];
                convertedCdr.Category = txtRow[Fn.Category].GetValueOrNull<int>();
                convertedCdr.SubCategory = txtRow[Fn.Subcategory].GetValueOrNull<int>();
                convertedCdr.ChangedByJobId = txtRow[Fn.ChangedByJobId].GetValueOrNull<long>();
                convertedCdr.SignalingStartTime = txtRow[Fn.SignalingStartTime].ConvertToDateTimeFromMySqlFormat();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                new ErrorWriter(e, "CdrJobExecuter", null,
                    "Could not convert text row to cdr", "");
                cdrInconsistent = ConvertTxtRowToCdrinconsistent(txtRow);
                cdrInconsistent.ErrorCode = e.Message;
                return null;
            }
            return convertedCdr;
        }

        public static cdrinconsistent ConvertTxtRowToCdrinconsistent(string[] txtRow)
        {
            cdrinconsistent inconsistentCdr = new cdrinconsistent();
            inconsistentCdr.SwitchId = txtRow[Fn.Switchid];
            inconsistentCdr.IdCall = System.Convert.ToInt64(txtRow[Fn.IdCall]).ToString();
            inconsistentCdr.SequenceNumber = txtRow[Fn.Sequencenumber];
            inconsistentCdr.FileName = txtRow[Fn.Filename];
            inconsistentCdr.ServiceGroup = txtRow[Fn.ServiceGroup];
            inconsistentCdr.IncomingRoute = txtRow[Fn.IncomingRoute];
            inconsistentCdr.OriginatingIP = txtRow[Fn.Originatingip];
            inconsistentCdr.OPC = txtRow[Fn.Opc];
            inconsistentCdr.OriginatingCIC = txtRow[Fn.OriginatingCIC];
            inconsistentCdr.OriginatingCalledNumber = txtRow[Fn.OriginatingCalledNumber];
            inconsistentCdr.TerminatingCalledNumber = txtRow[Fn.TerminatingCalledNumber];
            inconsistentCdr.OriginatingCallingNumber = txtRow[Fn.OriginatingCallingNumber];
            inconsistentCdr.TerminatingCallingNumber = txtRow[Fn.TerminatingCallingNumber];
            inconsistentCdr.PrePaid = txtRow[Fn.PrePaid];
            inconsistentCdr.DurationSec = txtRow[Fn.DurationSec];
            inconsistentCdr.EndTime = txtRow[Fn.Endtime];
            inconsistentCdr.ConnectTime = txtRow[Fn.ConnectTime];
            inconsistentCdr.AnswerTime = txtRow[Fn.AnswerTime];
            inconsistentCdr.ChargingStatus = txtRow[Fn.ChargingStatus];
            inconsistentCdr.PDD = txtRow[Fn.Pdd];
            inconsistentCdr.CountryCode = txtRow[Fn.CountryCode];
            inconsistentCdr.AreaCodeOrLata = txtRow[Fn.AreaCodeOrLata];
            inconsistentCdr.ReleaseDirection = txtRow[Fn.ReleaseDirection];
            inconsistentCdr.ReleaseCauseSystem = txtRow[Fn.ReleaseCauseSystem];
            inconsistentCdr.ReleaseCauseEgress = txtRow[Fn.ReleaseCauseEgress];
            inconsistentCdr.OutgoingRoute = txtRow[Fn.OutgoingRoute];
            inconsistentCdr.TerminatingIP = txtRow[Fn.TerminatingIp];
            inconsistentCdr.DPC = txtRow[Fn.Dpc];
            inconsistentCdr.TerminatingCIC = txtRow[Fn.TerminatingCIC];
            inconsistentCdr.StartTime = txtRow[Fn.StartTime];
            inconsistentCdr.InPartnerId = txtRow[Fn.InPartnerId];
            inconsistentCdr.CustomerRate = txtRow[Fn.CustomerRate];
            inconsistentCdr.OutPartnerId = txtRow[Fn.OutPartnerId];
            inconsistentCdr.SupplierRate = txtRow[Fn.SupplierRate];
            inconsistentCdr.MatchedPrefixY = txtRow[Fn.MatchedPrefixY];
            inconsistentCdr.UsdRateY = txtRow[Fn.UsdRateY];
            inconsistentCdr.MatchedPrefixCustomer = txtRow[Fn.MatchedPrefixCustomer];
            inconsistentCdr.MatchedPrefixSupplier = txtRow[Fn.MatchedPrefixSupplier];
            inconsistentCdr.InPartnerCost = txtRow[Fn.InPartnerCost];
            inconsistentCdr.OutPartnerCost = txtRow[Fn.OutPartnerCost];
            inconsistentCdr.CostAnsIn = txtRow[Fn.CostAnsIn];
            inconsistentCdr.CostIcxIn = txtRow[Fn.CostIcxIn];
            inconsistentCdr.Tax1 = txtRow[Fn.Tax1];
            inconsistentCdr.IgwRevenueIn = txtRow[Fn.IgwRevenueIn];
            inconsistentCdr.RevenueAnsOut = txtRow[Fn.RevenueAnsOut];
            inconsistentCdr.RevenueIgwOut = txtRow[Fn.RevenueIgwOut];
            inconsistentCdr.RevenueIcxOut = txtRow[Fn.RevenueIcxOut];
            inconsistentCdr.Tax2 = txtRow[Fn.Tax2];
            inconsistentCdr.XAmount = txtRow[Fn.XAmount];
            inconsistentCdr.YAmount = txtRow[Fn.YAmount];
            inconsistentCdr.AnsPrefixOrig = txtRow[Fn.AnsPrefixOrig];
            inconsistentCdr.AnsIdOrig = txtRow[Fn.AnsIdOrig];
            inconsistentCdr.AnsPrefixTerm = txtRow[Fn.AnsPrefixTerm];
            inconsistentCdr.AnsIdTerm = txtRow[Fn.AnsIdTerm];
            inconsistentCdr.ValidFlag = txtRow[Fn.Validflag];
            inconsistentCdr.PartialFlag = txtRow[Fn.Partialflag];
            inconsistentCdr.ReleaseCauseIngress = txtRow[Fn.ReleaseCauseIngress];
            inconsistentCdr.InRoamingOpId = txtRow[Fn.InRoamingOpId];
            inconsistentCdr.OutRoamingOpId = txtRow[Fn.OutRoamingOpId];
            inconsistentCdr.CalledPartyNOA = txtRow[Fn.CalledpartyNOA];
            inconsistentCdr.CallingPartyNOA = txtRow[Fn.CallingPartyNOA];
            inconsistentCdr.AdditionalPartyNumber = txtRow[Fn.AdditionalPartyNumber];
            inconsistentCdr.ResellerIds = txtRow[Fn.ResellerIds];
            inconsistentCdr.ZAmount = txtRow[Fn.ZAmount];
            inconsistentCdr.PreviousRoutes = txtRow[Fn.PreviousRoutes];
            inconsistentCdr.E1Id = txtRow[Fn.E1Id];
            inconsistentCdr.MediaIp1 = txtRow[Fn.Mediaip1];
            inconsistentCdr.MediaIp2 = txtRow[Fn.Mediaip2];
            inconsistentCdr.MediaIp3 = txtRow[Fn.Mediaip3];
            inconsistentCdr.MediaIp4 = txtRow[Fn.Mediaip4];
            inconsistentCdr.CallReleaseDuration = txtRow[Fn.CallReleaseduration];
            inconsistentCdr.E1IdOut = txtRow[Fn.E1Idout];
            inconsistentCdr.InTrunkAdditionalInfo = txtRow[Fn.InTrunkAdditionalInfo];
            inconsistentCdr.OutTrunkAdditionalInfo = txtRow[Fn.OutTrunkAdditionalInfo];
            inconsistentCdr.InMgwId = txtRow[Fn.InMgwId];
            inconsistentCdr.OutMgwId = txtRow[Fn.OutMgwId];
            inconsistentCdr.MediationComplete= txtRow[Fn.MediationComplete];
            inconsistentCdr.Codec = txtRow[Fn.Codec];
            inconsistentCdr.ConnectedNumberType = txtRow[Fn.Connectednumbertype];
            inconsistentCdr.RedirectingNumber = txtRow[Fn.Redirectingnumber];
            inconsistentCdr.CallForwardOrRoamingType = txtRow[Fn.Callforwardorroamingtype];
            inconsistentCdr.OtherDate = txtRow[Fn.OtherDate];
            inconsistentCdr.SummaryMetaTotal = txtRow[Fn.SummaryMetaTotal];
            inconsistentCdr.TransactionMetaTotal = txtRow[Fn.TransactionMetaTotal];
            inconsistentCdr.ChargeableMetaTotal = txtRow[Fn.ChargeableMetaTotal];
            inconsistentCdr.ErrorCode = txtRow[Fn.ErrorCode];
            inconsistentCdr.NERSuccess = txtRow[Fn.NERSuccess];
            inconsistentCdr.RoundedDuration = txtRow[Fn.RoundedDuration];
            inconsistentCdr.PartialDuration = txtRow[Fn.PartialDuration];
            inconsistentCdr.PartialAnswerTime = txtRow[Fn.PartialAnswertime];
            inconsistentCdr.PartialEndTime = txtRow[Fn.PartialEndtime];
            inconsistentCdr.FinalRecord = txtRow[Fn.FinalRecord];
            inconsistentCdr.Duration1 = txtRow[Fn.Duration1];
            inconsistentCdr.Duration2 = txtRow[Fn.Duration2];
            inconsistentCdr.Duration3 = txtRow[Fn.Duration3];
            inconsistentCdr.Duration4 = txtRow[Fn.Duration4];
            inconsistentCdr.PreviousPeriodCdr = txtRow[Fn.PreviousPeriodCdr];
            inconsistentCdr.UniqueBillId = txtRow[Fn.UniqueBillId];
            inconsistentCdr.AdditionalMetaData = txtRow[Fn.AdditionalMetaData];
            inconsistentCdr.Category = txtRow[Fn.Category];
            inconsistentCdr.SubCategory = txtRow[Fn.Subcategory];
            inconsistentCdr.ChangedByJobId = txtRow[Fn.ChangedByJobId];
            inconsistentCdr.SignalingStartTime = txtRow[Fn.SignalingStartTime];
            return inconsistentCdr;
        }

        public static ICdr Clone(ICdr sourceCdr)
        {
            ICdr newInstance = new cdr();
            newInstance.SwitchId = sourceCdr.SwitchId;
            newInstance.IdCall = sourceCdr.IdCall;
            newInstance.SequenceNumber = sourceCdr.SequenceNumber;
            newInstance.FileName = sourceCdr.FileName;
            newInstance.ServiceGroup = sourceCdr.ServiceGroup;
            newInstance.IncomingRoute = sourceCdr.IncomingRoute;
            newInstance.OriginatingIP = sourceCdr.OriginatingIP;
            newInstance.OPC = sourceCdr.OPC;
            newInstance.OriginatingCIC = sourceCdr.OriginatingCIC;
            newInstance.OriginatingCalledNumber = sourceCdr.OriginatingCalledNumber;
            newInstance.TerminatingCalledNumber = sourceCdr.TerminatingCalledNumber;
            newInstance.OriginatingCallingNumber = sourceCdr.OriginatingCallingNumber;
            newInstance.TerminatingCallingNumber = sourceCdr.TerminatingCallingNumber;
            newInstance.PrePaid = sourceCdr.PrePaid;
            newInstance.DurationSec = sourceCdr.DurationSec;
            newInstance.EndTime = sourceCdr.EndTime;
            newInstance.ConnectTime = sourceCdr.ConnectTime;
            newInstance.AnswerTime = sourceCdr.AnswerTime;
            newInstance.ChargingStatus = sourceCdr.ChargingStatus;
            newInstance.PDD = sourceCdr.PDD;
            newInstance.CountryCode = sourceCdr.CountryCode;
            newInstance.AreaCodeOrLata = sourceCdr.AreaCodeOrLata;
            newInstance.ReleaseDirection = sourceCdr.ReleaseDirection;
            newInstance.ReleaseCauseSystem = sourceCdr.ReleaseCauseSystem;
            newInstance.ReleaseCauseEgress = sourceCdr.ReleaseCauseEgress;
            newInstance.OutgoingRoute = sourceCdr.OutgoingRoute;
            newInstance.TerminatingIP = sourceCdr.TerminatingIP;
            newInstance.DPC = sourceCdr.DPC;
            newInstance.TerminatingCIC = sourceCdr.TerminatingCIC;
            newInstance.StartTime = sourceCdr.StartTime;
            newInstance.InPartnerId = sourceCdr.InPartnerId;
            newInstance.CustomerRate = sourceCdr.CustomerRate;
            newInstance.OutPartnerId = sourceCdr.OutPartnerId;
            newInstance.SupplierRate = sourceCdr.SupplierRate;
            newInstance.MatchedPrefixY = sourceCdr.MatchedPrefixY;
            newInstance.UsdRateY = sourceCdr.UsdRateY;
            newInstance.MatchedPrefixCustomer = sourceCdr.MatchedPrefixCustomer;
            newInstance.MatchedPrefixSupplier = sourceCdr.MatchedPrefixSupplier;
            newInstance.InPartnerCost = sourceCdr.InPartnerCost;
            newInstance.OutPartnerCost = sourceCdr.OutPartnerCost;
            newInstance.CostAnsIn = sourceCdr.CostAnsIn;
            newInstance.CostIcxIn = sourceCdr.CostIcxIn;
            newInstance.Tax1 = sourceCdr.Tax1;
            newInstance.IgwRevenueIn = sourceCdr.IgwRevenueIn;
            newInstance.RevenueAnsOut = sourceCdr.RevenueAnsOut;
            newInstance.RevenueIgwOut = sourceCdr.RevenueIgwOut;
            newInstance.RevenueIcxOut = sourceCdr.RevenueIcxOut;
            newInstance.Tax2 = sourceCdr.Tax2;
            newInstance.XAmount = sourceCdr.XAmount;
            newInstance.YAmount = sourceCdr.YAmount;
            newInstance.AnsPrefixOrig = sourceCdr.AnsPrefixOrig;
            newInstance.AnsIdOrig = sourceCdr.AnsIdOrig;
            newInstance.AnsPrefixTerm = sourceCdr.AnsPrefixTerm;
            newInstance.AnsIdTerm = sourceCdr.AnsIdTerm;
            newInstance.ValidFlag = sourceCdr.ValidFlag;
            newInstance.PartialFlag = sourceCdr.PartialFlag;
            newInstance.ReleaseCauseIngress = sourceCdr.ReleaseCauseIngress;
            newInstance.InRoamingOpId = sourceCdr.InRoamingOpId;
            newInstance.OutRoamingOpId = sourceCdr.OutRoamingOpId;
            newInstance.CalledPartyNOA = sourceCdr.CalledPartyNOA;
            newInstance.CallingPartyNOA = sourceCdr.CallingPartyNOA;
            newInstance.AdditionalSystemCodes = sourceCdr.AdditionalSystemCodes;
            newInstance.AdditionalPartyNumber = sourceCdr.AdditionalPartyNumber;
            newInstance.ResellerIds = sourceCdr.ResellerIds;
            newInstance.ZAmount = sourceCdr.ZAmount;
            newInstance.PreviousRoutes = sourceCdr.PreviousRoutes;
            newInstance.E1Id = sourceCdr.E1Id;
            newInstance.MediaIp1 = sourceCdr.MediaIp1;
            newInstance.MediaIp2 = sourceCdr.MediaIp2;
            newInstance.MediaIp3 = sourceCdr.MediaIp3;
            newInstance.MediaIp4 = sourceCdr.MediaIp4;
            newInstance.CallReleaseDuration = sourceCdr.CallReleaseDuration;
            newInstance.E1IdOut = sourceCdr.E1IdOut;
            newInstance.InTrunkAdditionalInfo = sourceCdr.InTrunkAdditionalInfo;
            newInstance.OutTrunkAdditionalInfo = sourceCdr.OutTrunkAdditionalInfo;
            newInstance.InMgwId = sourceCdr.InMgwId;
            newInstance.OutMgwId = sourceCdr.OutMgwId;
            newInstance.MediationComplete = sourceCdr.MediationComplete;
            newInstance.Codec = sourceCdr.Codec;
            newInstance.ConnectedNumberType = sourceCdr.ConnectedNumberType;
            newInstance.RedirectingNumber = sourceCdr.RedirectingNumber;
            newInstance.CallForwardOrRoamingType = sourceCdr.CallForwardOrRoamingType;
            newInstance.OtherDate = sourceCdr.OtherDate;
            newInstance.SummaryMetaTotal = sourceCdr.SummaryMetaTotal;
            newInstance.TransactionMetaTotal = sourceCdr.TransactionMetaTotal;
            newInstance.ChargeableMetaTotal = sourceCdr.ChargeableMetaTotal;
            newInstance.ErrorCode = sourceCdr.ErrorCode;
            newInstance.NERSuccess = sourceCdr.NERSuccess;
            newInstance.RoundedDuration = sourceCdr.RoundedDuration;
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
            newInstance.AdditionalMetaData = sourceCdr.AdditionalMetaData;
            newInstance.Category = sourceCdr.Category;
            newInstance.SubCategory = sourceCdr.SubCategory;
            newInstance.ChangedByJobId = sourceCdr.ChangedByJobId;
            newInstance.SignalingStartTime = sourceCdr.SignalingStartTime;
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
                IdCall = sourceCdr.IdCall,
                SequenceNumber = sourceCdr.SequenceNumber.ToString(),
                FileName = sourceCdr.FileName.ToString(),
                ServiceGroup = sourceCdr.ServiceGroup.ToString(),
                IncomingRoute = sourceCdr.IncomingRoute.ToString(),
                OriginatingIP = sourceCdr.OriginatingIP.ToString(),
                OPC = sourceCdr.OPC.ToString(),
                OriginatingCIC = sourceCdr.OriginatingCIC.ToString(),
                OriginatingCalledNumber = sourceCdr.OriginatingCalledNumber.ToString(),
                TerminatingCalledNumber = sourceCdr.TerminatingCalledNumber.ToString(),
                OriginatingCallingNumber = sourceCdr.OriginatingCallingNumber.ToString(),
                TerminatingCallingNumber = sourceCdr.TerminatingCallingNumber.ToString(),
                PrePaid = sourceCdr.PrePaid.ToString(),
                DurationSec = sourceCdr.DurationSec.ToString(),
                EndTime = nullableDateToString(sourceCdr.EndTime),
                ConnectTime = nullableDateToString(sourceCdr.ConnectTime),
                AnswerTime = nullableDateToString(sourceCdr.AnswerTime),
                ChargingStatus = sourceCdr.ChargingStatus.ToString(),
                PDD = sourceCdr.PDD.ToString(),
                CountryCode = sourceCdr.CountryCode.ToString(),
                AreaCodeOrLata = sourceCdr.AreaCodeOrLata.ToString(),
                ReleaseDirection = sourceCdr.ReleaseDirection.ToString(),
                ReleaseCauseSystem = sourceCdr.ReleaseCauseSystem.ToString(),
                ReleaseCauseEgress = sourceCdr.ReleaseCauseEgress.ToString(),
                OutgoingRoute = sourceCdr.OutgoingRoute.ToString(),
                TerminatingIP = sourceCdr.TerminatingIP.ToString(),
                DPC = sourceCdr.DPC.ToString(),
                TerminatingCIC = sourceCdr.TerminatingCIC.ToString(),
                StartTime = sourceCdr.StartTime,
                InPartnerId = sourceCdr.InPartnerId.ToString(),
                CustomerRate = sourceCdr.CustomerRate.ToString(),
                OutPartnerId = sourceCdr.OutPartnerId.ToString(),
                SupplierRate = sourceCdr.SupplierRate.ToString(),
                MatchedPrefixY = sourceCdr.MatchedPrefixY.ToString(),
                UsdRateY = sourceCdr.UsdRateY.ToString(),
                MatchedPrefixCustomer = sourceCdr.MatchedPrefixCustomer.ToString(),
                MatchedPrefixSupplier = sourceCdr.MatchedPrefixSupplier.ToString(),
                InPartnerCost = sourceCdr.InPartnerCost.ToString(),
                OutPartnerCost = sourceCdr.OutPartnerCost.ToString(),
                CostAnsIn = sourceCdr.CostAnsIn.ToString(),
                CostIcxIn = sourceCdr.CostIcxIn.ToString(),
                Tax1 = sourceCdr.Tax1.ToString(),
                IgwRevenueIn = sourceCdr.IgwRevenueIn.ToString(),
                RevenueAnsOut = sourceCdr.RevenueAnsOut.ToString(),
                RevenueIgwOut = sourceCdr.RevenueIgwOut.ToString(),
                RevenueIcxOut = sourceCdr.RevenueIcxOut.ToString(),
                Tax2 = sourceCdr.Tax2.ToString(),
                XAmount = sourceCdr.XAmount.ToString(),
                YAmount = sourceCdr.YAmount.ToString(),
                AnsPrefixOrig = sourceCdr.AnsPrefixOrig.ToString(),
                AnsIdOrig = sourceCdr.AnsIdOrig.ToString(),
                AnsPrefixTerm = sourceCdr.AnsPrefixTerm.ToString(),
                AnsIdTerm = sourceCdr.AnsIdTerm.ToString(),
                ValidFlag = sourceCdr.ValidFlag.ToString(),
                PartialFlag = sourceCdr.PartialFlag.ToString(),
                ReleaseCauseIngress = sourceCdr.ReleaseCauseIngress.ToString(),
                InRoamingOpId = sourceCdr.InRoamingOpId.ToString(),
                OutRoamingOpId = sourceCdr.OutRoamingOpId.ToString(),
                CalledPartyNOA = sourceCdr.CalledPartyNOA.ToString(),
                CallingPartyNOA = sourceCdr.CallingPartyNOA.ToString(),
                AdditionalSystemCodes = sourceCdr.AdditionalSystemCodes,
                AdditionalPartyNumber = sourceCdr.AdditionalPartyNumber.ToString(),
                ResellerIds = sourceCdr.ResellerIds.ToString(),
                ZAmount = sourceCdr.ZAmount.ToString(),
                PreviousRoutes = sourceCdr.PreviousRoutes.ToString(),
                E1Id = sourceCdr.E1Id.ToString(),
                MediaIp1 = sourceCdr.MediaIp1,
                MediaIp2 = sourceCdr.MediaIp2,
                MediaIp3 = sourceCdr.MediaIp3,
                MediaIp4 = sourceCdr.MediaIp4,
                CallReleaseDuration = sourceCdr.CallReleaseDuration.ToString(),
                E1IdOut = sourceCdr.E1IdOut.ToString(),
                InTrunkAdditionalInfo = sourceCdr.InTrunkAdditionalInfo.ToString(),
                OutTrunkAdditionalInfo = sourceCdr.OutTrunkAdditionalInfo.ToString(),
                InMgwId = sourceCdr.InMgwId.ToString(),
                OutMgwId = sourceCdr.OutMgwId.ToString(),
                MediationComplete = sourceCdr.MediationComplete.ToString(),
                Codec = sourceCdr.Codec.ToString(),
                ConnectedNumberType = sourceCdr.ConnectedNumberType.ToString(),
                RedirectingNumber = sourceCdr.RedirectingNumber.ToString(),
                CallForwardOrRoamingType = sourceCdr.CallForwardOrRoamingType.ToString(),
                OtherDate = nullableDateToString(sourceCdr.OtherDate),
                SummaryMetaTotal = sourceCdr.SummaryMetaTotal.ToString(),
                TransactionMetaTotal = sourceCdr.TransactionMetaTotal.ToString(),
                ChargeableMetaTotal = sourceCdr.ChargeableMetaTotal.ToString(),
                ErrorCode = sourceCdr.ErrorCode.ToString(),
                NERSuccess = sourceCdr.NERSuccess.ToString(),
                RoundedDuration = sourceCdr.RoundedDuration.ToString(),
                PartialDuration = sourceCdr.PartialDuration.ToString(),
                PartialAnswerTime = nullableDateToString(sourceCdr.PartialAnswerTime),
                PartialEndTime = sourceCdr.PartialEndTime.ToString(),
                FinalRecord = sourceCdr.FinalRecord.ToString(),
                Duration1 = sourceCdr.Duration1.ToString(),
                Duration2 = sourceCdr.Duration2.ToString(),
                Duration3 = sourceCdr.Duration3.ToString(),
                Duration4 = sourceCdr.Duration4.ToString(),
                PreviousPeriodCdr = sourceCdr.PreviousPeriodCdr.ToString(),
                UniqueBillId = sourceCdr.UniqueBillId,
                AdditionalMetaData = sourceCdr.AdditionalMetaData,
                Category = sourceCdr.Category.ToString(),
                SubCategory = sourceCdr.SubCategory.ToString(),
                ChangedByJobId = sourceCdr.ChangedByJobId.ToString(),
                SignalingStartTime = nullableDateToString(sourceCdr.SignalingStartTime)

            };
        }
    }


}
