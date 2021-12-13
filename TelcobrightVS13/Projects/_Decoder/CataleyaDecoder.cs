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
            return new List<string[]>();
        }


        public List<string[]> DecodeFile(CdrCollectorInputData input, out List<cdrinconsistent> inconsistentCdrs)
        {
            inconsistentCdrs = new List<cdrinconsistent>();
            List<string[]> decodedRows = new List<string[]>();
            this.Input = input;
            List<cdrfieldmappingbyswitchtype> fieldMappings = null;
            //input.MefDecodersData.DicFieldMapping.TryGetValue(this.Id, out fieldMappings);
            //int maxFieldPositionInInputCsv = fieldMappings.Max(
            //    cdrFldMapping => Convert.ToInt32(cdrFldMapping.FieldPositionInCDRRow));
            String inputFilename = this.Input.FullPath;
            SdrExtractor sdrExtractor = new SdrExtractor();
            List<PbSdrRecord> sdrs = sdrExtractor.GetSdrs(inputFilename);
            string[] replaceChars = new string[] { "'", "<", ">" };
            foreach (var record in sdrs) //for each row
            {
                string[] normalizedRow = new string[104];
                try
                {
                    normalizedRow[Fn.Sequencenumber] = record.SeqNum.ToString();
                    Func<double, string> unixTsToMySqlDateTime =
                        ts => DateTimeExtensions.UnixTimeStampToDateTime(ts).ToString("yyyy-MM-dd hh:mm:ss");
                    normalizedRow[Fn.SignalingStartTime] = unixTsToMySqlDateTime(record.IngressCallInfo.InvitingTs.Sec);
                    normalizedRow[Fn.StartTime] = normalizedRow[Fn.SignalingStartTime];

                    Func<string, Dictionary<string, string>> getSplitPartyInfo = party =>
                        {
                            var tempArr = party.Split('@');
                            var phoneNumber = tempArr[0].Split(':')[1];
                            return new Dictionary<string, string>()
                            {
                                { "phoneNumber",phoneNumber},
                                { "ipAndPort", tempArr[1]},
                            };
                        };
                    PbSdrCallInfo egressCallInfo = record.EgressCallInfo;
                    if (egressCallInfo != null) {
                        Dictionary<string, string> egressCallingPartyInfo = getSplitPartyInfo(egressCallInfo.CallingParty);
                        normalizedRow[Fn.TerminatingCallingNumber] = egressCallingPartyInfo["phoneNumber"];

                        Dictionary<string, string> egressCalledPartyInfo = getSplitPartyInfo(egressCallInfo.CalledParty);
                        normalizedRow[Fn.TerminatingCalledNumber] = egressCalledPartyInfo["phoneNumber"];
                        normalizedRow[Fn.TerminatingIp] = egressCalledPartyInfo["ipAndPort"].Split(';')[0];
                        normalizedRow[Fn.OutgoingRoute] = egressCallInfo.ZoneTgid;

                        PbTime ringingTs = egressCallInfo.RingingTs;
                        normalizedRow[Fn.ConnectTime] = ringingTs != null ?
                            unixTsToMySqlDateTime(ringingTs.MilliSec) : "";
                        
                        PbTime answerTs = egressCallInfo.AnswerTs;
                        normalizedRow[Fn.AnswerTime] = answerTs != null ? unixTsToMySqlDateTime(answerTs.MilliSec) : "";
                    }

                    PbSdrCallInfo ingressCallInfo = record.IngressCallInfo;
                    Dictionary<string, string> ingressCallingPartyInfo = getSplitPartyInfo(ingressCallInfo.CallingParty);
                    normalizedRow[Fn.OriginatingCallingNumber] = ingressCallingPartyInfo["phoneNumber"];
                    normalizedRow[Fn.Originatingip] = ingressCallingPartyInfo["ipAndPort"];
                    normalizedRow[Fn.IncomingRoute] = ingressCallInfo.ZoneTgid;

                    Dictionary<string, string> ingressCalledPartyInfo = getSplitPartyInfo(ingressCallInfo.CalledParty);
                    normalizedRow[Fn.OriginatingCalledNumber] = ingressCalledPartyInfo["phoneNumber"];

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
                    normalizedRow[Fn.ReleaseCauseIngress] = record.SipStatusCode.ToString();
                    double durationSec = Convert.ToDouble(record.Duration) / 1000;
                    normalizedRow[Fn.DurationSec] = durationSec.ToString(CultureInfo.InvariantCulture);
                    normalizedRow[Fn.ChargingStatus] = durationSec > 0 ? "1" : "0";
                    
                    //add valid flag for this type of switch, valid flag comes from cdr for zte
                    normalizedRow[Fn.Validflag] = "1";
                    normalizedRow[Fn.Partialflag] = "0";
                    normalizedRow[Fn.FinalRecord] = "1";
                    normalizedRow[Fn.ConnectTime] = normalizedRow[Fn.StartTime];
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
                        , input.Tbc.DatabaseSetting.DatabaseName);
                }
            }//for each row
            return decodedRows;
        }

        


    }
}
