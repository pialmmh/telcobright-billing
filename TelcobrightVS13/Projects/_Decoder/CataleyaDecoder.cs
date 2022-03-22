using TelcobrightMediation;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using TelcobrightFileOperations;
using MediationModel;
using TelcobrightMediation.Cdr;
using TelcobrightMediation.Mediation.Cdr;
using System.Linq;
using System.Globalization;
using CataleySdrExtractor;
using LibraryExtensions;
namespace Decoders
{

    [Export("Decoder", typeof(IFileDecoder))]
    public class CataleyaDecoder : IFileDecoder
    {
        public override string ToString() => this.RuleName;
        public virtual string RuleName => GetType().Name;
        public int Id => 21;
        public string HelpText => "Decodes Cataleya CDR.";
        protected CdrCollectorInputData Input { get; set; }
        protected virtual List<string[]> GetTxtCdrs()
        {
            return FileUtil.ParseCsvWithEnclosedAndUnenclosedFields(this.Input.FullPath, ',', 0,"\"",";");
        }


        public List<string[]> DecodeFile(CdrCollectorInputData input, out List<cdrinconsistent> inconsistentCdrs)
        {
            
            inconsistentCdrs = new List<cdrinconsistent>();
            List<string[]> decodedRows = new List<string[]>();
            this.Input = input;
            List<cdrfieldmappingbyswitchtype> fieldMappings = null;
            input.MefDecodersData.DicFieldMapping.TryGetValue(this.Id, out fieldMappings);
            List<string[]> tempTable = GetTxtCdrs();

            SdrExtractor sdrExtractor = new SdrExtractor();
            List<PbSdrRecord> sdrs = sdrExtractor.GetSdrs(input.FullPath);

            string[] replaceChars = new string[] { "'", "<", ">" };
            foreach (var record in sdrs) //for each row
            {
                string[] normalizedRow = new string[104];
                try
                {
                    normalizedRow[Fn.Sequencenumber] = record.SeqNum.ToString();
                    Func<PbTime, DateTime> unixTsToDateTimeWithMs = pbTime =>
                    {
                        var dateTime = DateTimeExtensions.UnixTimeStampToDateTime(pbTime.Sec).AddMilliseconds(pbTime.MilliSec);
                        return dateTime;
                    };
                    normalizedRow[Fn.SignalingStartTime] = unixTsToDateTimeWithMs(record.IngressCallInfo.InvitingTs)
                        .ToMySqlFormatWithoutQuote();
                    normalizedRow[Fn.StartTime] = normalizedRow[Fn.SignalingStartTime];
                    normalizedRow[Fn.Endtime] = unixTsToDateTimeWithMs(record.IngressCallInfo.DisconnectTs)
                        .ToMySqlFormatWithoutQuote();
                    normalizedRow[Fn.UniqueBillId] = record.Icid;
                    Func<string, Dictionary<string, string>> getSplitPartyInfo = party =>
                    {
                        var partyInfo = new Dictionary<string, string>()
                            {
                                { "phoneNumber",""},
                                { "ipAndPort", ""},
                            };
                        if (!String.IsNullOrEmpty(party))
                        {
                            var tempArr = party.Split('@');
                            var phoneNumber = tempArr[0].Split(':')[1];
                            partyInfo["phoneNumber"] = phoneNumber;
                            partyInfo["ipAndPort"] = tempArr[1];
                        }
                        return partyInfo;
                    };
                    switch (record.ReleaseDirection)
                    {
                        case PbReleaseDirectionType.InternalReleaseDirection:
                            normalizedRow[Fn.ReleaseDirection] = "7";//internal release due to some problem, matching with zte and dialogic
                            break;
                        case PbReleaseDirectionType.OriginationReleaseDirection:
                            normalizedRow[Fn.ReleaseDirection] = "0";
                            break;
                        case PbReleaseDirectionType.TerminationReleaseDirection:
                            normalizedRow[Fn.ReleaseDirection] = "1";
                            break;
                        case PbReleaseDirectionType.UndefinedReleaseDirection:
                            normalizedRow[Fn.ReleaseDirection] = "8";
                            break;
                    }

                    //ingress
                    PbSdrCallInfo ingressCallInfo = record.IngressCallInfo;
                    Dictionary<string, string> ingressCallingPartyInfo = getSplitPartyInfo(ingressCallInfo.CallingParty);
                    normalizedRow[Fn.OriginatingCallingNumber] = ingressCallingPartyInfo["phoneNumber"];
                    normalizedRow[Fn.Originatingip] = ingressCallingPartyInfo["ipAndPort"].Split(';')[0].Trim();
                    normalizedRow[Fn.IncomingRoute] = ingressCallInfo.ZoneId.ToString();
                    Dictionary<string, string> ingressCalledPartyInfo = getSplitPartyInfo(ingressCallInfo.CalledParty);
                    normalizedRow[Fn.OriginatingCalledNumber] = ingressCalledPartyInfo["phoneNumber"];
                    normalizedRow[Fn.ReleaseCauseIngress] = record.SipStatusCode.ToString();

                    normalizedRow[Fn.DurationSec] = "0";
                    normalizedRow[Fn.ChargingStatus] = "0";
                    //egress
                    PbSdrCallInfo egressCallInfo = record.EgressCallInfo;
                    if (egressCallInfo != null)
                    {
                        Dictionary<string, string> egressCallingPartyInfo = getSplitPartyInfo(egressCallInfo.CallingParty);
                        normalizedRow[Fn.TerminatingCallingNumber] =
                            egressCallInfo.TransCallingParty.Split(':')[1].Split('@')[0];

                        Dictionary<string, string> egressCalledPartyInfo = getSplitPartyInfo(egressCallInfo.CalledParty);
                        normalizedRow[Fn.TerminatingCalledNumber] =
                        egressCallInfo.TransCalledParty.Split(':')[1].Split('@')[0];


                        normalizedRow[Fn.TerminatingIp] = egressCalledPartyInfo["ipAndPort"].Split(';')[0];
                        normalizedRow[Fn.OutgoingRoute] = egressCallInfo.ZoneId.ToString();

                        PbTime ringingTs = egressCallInfo.RingingTs;
                        normalizedRow[Fn.ConnectTime] = ringingTs != null ?
                            unixTsToDateTimeWithMs(ringingTs).ToMySqlFormatWithoutQuote() : "";

                        //duration and answertime
                        double durationSec = 0;
                        if (egressCallInfo.AnswerTs != null)
                        {
                            DateTime answerTime = unixTsToDateTimeWithMs(egressCallInfo.AnswerTs);
                            DateTime endTime = unixTsToDateTimeWithMs(egressCallInfo.DisconnectTs);
                            durationSec = (endTime - answerTime).TotalMilliseconds / 1000;
                            normalizedRow[Fn.AnswerTime] = answerTime.ToMySqlFormatWithoutQuote();
                        }
                        normalizedRow[Fn.DurationSec] = durationSec.ToString(CultureInfo.InvariantCulture);
                        normalizedRow[Fn.ChargingStatus] = durationSec > 0 ? "1" : "0";
                    }

                    //add valid flag for this type of switch, valid flag comes from cdr for zte
                    normalizedRow[Fn.Validflag] = "1";
                    normalizedRow[Fn.Partialflag] = "0";
                    normalizedRow[Fn.FinalRecord] = "1";
                    decodedRows.Add(normalizedRow);
                }
                catch (Exception e1)
                {
                    //if error found for one row, add this to inconsistent
                    Console.WriteLine(e1);
                    var inconsistentCdr = CdrConversionUtil.ConvertTxtRowToCdrinconsistent(normalizedRow);
                    inconsistentCdr.SwitchId = input.Ne.idSwitch.ToString();
                    inconsistentCdr.FileName = input.TelcobrightJob.JobName;
                    inconsistentCdrs.Add(inconsistentCdr);
                    ErrorWriter wr = new ErrorWriter(e1, "DecodeCdr", null,
                        this.RuleName + " encounterd error during decoding and an Inconsistent cdr has been generated."
                        , input.Tbc.DatabaseSetting.GetOperatorName);
                }
            }//for each row
            return decodedRows;
        }




    }
}