using System.Diagnostics;
using System.Runtime.InteropServices;
using System;
using System.Collections.Generic;
using System.Linq;
using MediationModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO.MemoryMappedFiles;
using System.IO;
using System.Text;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using LibraryExtensions;
using System.Globalization;
using System.Net;
using System.Runtime;
using Decoders.SigtranDecoderHelper;
using TelcobrightMediation;


namespace Decoders
{
    class SigtranPacketDecoder
    {
        private string PcapFileName { get; set; }
        string filePath = "telco.bin";

        private static string TSharkExe =
            Path.Combine(new UpwordPathFinder<DirectoryInfo>("ToolsAndScripts").FindAndGetFullPath(),
                "externalResources", "Tshark", "tshark.exe");
        Dictionary<string, partnerprefix> ansPrefixes { get; }

        private readonly List<string> opcList = new List<string>()
        {
            "4702",
            "4699",
            "4700",
            "4701"
        };

        private readonly Dictionary<string, MsuType> causeCodes = new Dictionary<string, MsuType>
        {
            {"1:44",MsuType.Mt},
            {"2:44",MsuType.MtResp},
            {"1:46",MsuType.Mt},
            {"2:46",MsuType.MtResp},
            {"1:45",MsuType.Sri},
            {"2:45",MsuType.SriResp},
            {"1:63",MsuType.InformServiceCenter}
        };
        public SigtranPacketDecoder(string pcapFilename, Dictionary<string, partnerprefix> ansPrefixes)
        {
            this.PcapFileName = pcapFilename;
            this.ansPrefixes = ansPrefixes;
        }


        public List<SigtranPacket> DecodePackets()
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = $"{TSharkExe}",
                Arguments = $"-r {PcapFileName} -Tjson -eframe.time " +
                            "-esccp.return_cause -emtp3.opc -emtp3.dpc -esccp.called.digits " +
                            "-esccp.calling.digits -etcap.tid -etcap.dtid -egsm_map.old.Component -ee212.imsi " +
                            "-egsm_old.localValue -ee164.msisdn -egsm_sms.sms_text -egsm_map.sm.serviceCentreAddress " +
                            "-egsm_map.sm.msisdn -Y((gsm_map)&&(mtp3.opc==4699||mtp3.opc==4700||mtp3.opc==4701||mtp3.opc==4702))",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            string output, error;
            int exitCode;
            using (var process = new Process { StartInfo = processStartInfo })
            {
                process.Start();

                // Read the output directly into a variable
                output = process.StandardOutput.ReadToEnd();
                error = process.StandardError.ReadToEnd();

                process.WaitForExit();
                exitCode = process.ExitCode;
            }
            var prevLatencyMode = GCSettings.LatencyMode;
            GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
            List<SigtranPacket> data = JsonToSigtranPacket.ConvertJsonToSigtranPacket(output);
            GCSettings.LatencyMode = prevLatencyMode;
            return data;

            //var prevLatencyMode = GCSettings.LatencyMode;
            //GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
            //List<SigtranPacket> data = JsonToSigtranPacket.ConvertJsonToSigtranPacket(output);
            //GCSettings.LatencyMode = prevLatencyMode;
        }


        //public List<SigtranPacket> DecodePackets()
        //{
        //    Process senderProcess = new Process();
        //    senderProcess.StartInfo.FileName = TSharkExe;
        //    senderProcess.StartInfo.Arguments = this.PcapFileName;
        //    senderProcess.StartInfo.UseShellExecute = false;
        //    //senderProcess.StartInfo.RedirectStandardOutput = true;
        //    senderProcess.StartInfo.CreateNoWindow = true;
        //    senderProcess.Start();
        //    senderProcess.WaitForExit();
        //    var prevLatencyMode = GCSettings.LatencyMode;
        //    GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
        //    List<SigtranPacket> data = JsonToSigtranPacket.ConvertJsonToSigtranPacket(this.PcapFileName.Replace(".pcap.gz", ".json"));
        //    GCSettings.LatencyMode = prevLatencyMode;
        //    //List<SigtranPacket> data = new List<SigtranPacket>();
        //    return data;
        //}

        //List<JObject> DecodePackets()
        //{
        //    Process senderProcess = new Process();
        //    senderProcess.StartInfo.FileName = TSharkExe;
        //    senderProcess.StartInfo.Arguments = PcapFileName;                                                                                                                                                                                                                                                                                                                                                                                                                                     
        //    senderProcess.StartInfo.UseShellExecute = false;
        //    senderProcess.StartInfo.RedirectStandardOutput = true;
        //    senderProcess.StartInfo.CreateNoWindow = true;
        //    senderProcess.Start();
        //    senderProcess.WaitForExit();
        //    var data = JsonStringReader.ReadJsonObjects(filePath);
        //    var objectList = JsonStringReader.ParseJsonStrings(data);
        //    return objectList;
        //}


        string ParseAndFormatTimestamp(string timestamp)
        {
            timestamp = timestamp.Substring(0, timestamp.Length - 25);
            DateTime parsedDate = DateTimeOffset.Parse(timestamp).DateTime;
            //timestamp = timestamp.Substring(0, timestamp.Length - 4);
            //DateTime parsedDate = DateTimeOffset.Parse(timestamp).DateTime.AddHours(6);
            return parsedDate.ToString("yyyy-MM-dd HH:mm:ss");
        }

        string ParseAndFormatTimestampWtihMs(string timestamp)
        {
            timestamp = timestamp.Substring(0, timestamp.Length - 25);
            DateTime parsedDate = DateTimeOffset.Parse(timestamp).DateTime;
            //timestamp = timestamp.Substring(0, timestamp.Length - 4);
            //DateTime parsedDate = DateTimeOffset.Parse(timestamp).DateTime.AddHours(6);
            return parsedDate.ToString("yyyy-MM-dd HH:mm:ss.fffffff");
        }


        public List<string[]> CdrRecords(List<SigtranPacket> packets)
        {
            List<string[]> records = new List<string[]>(packets.Count);
            for (int i = 0; i < packets.Count; i++)
            {
                records.Add(null); // Add an empty array to the list
            }

            //Parallel.For(0,packets.Count, index =>
            for (int index = 0; index < packets.Count; index++)
            {
                var packet = packets[index];
                //string[] record = Enumerable.Repeat((string)null, 104).ToArray();
                string[] record = new string[104];

                // gms layer
                GsmMap gsmMapLayer = packet.GsmMap;

                if (gsmMapLayer == null)
                {
                    continue;
                }

                Frame frameLayer = packet.Frame;
                string frameTime = frameLayer?.FrameTime.ToString();
                string dateTime = ParseAndFormatTimestamp(frameTime);

                record[Sn.PacketFrameTime] = ParseAndFormatTimestampWtihMs(frameTime);
                record[Sn.StartTime] = dateTime;
                record[Sn.AnswerTime] = dateTime;

                record[Sn.Duration1] = "60";
                record[Sn.DurationSec] = "60";

                //record[Sn.Originatingip] = packet.Ip?.SrcIp?.ToString();
                //record[Sn.TerminatingIp] = packet.Ip?.DstIp?.ToString();

                // sctp layer
                //Sctp sctpLayer = packet.Sctp;
                //record[Sn.IncomingRoute] = sctpLayer?.SrcPort.ToString();
                //record[Sn.OutgoingRoute] = sctpLayer?.DstPort.ToString();

                //m3ua layer
                M3ua m3UaLayer = packet.M3Ua;
                Mtp3Equivalents mtp3Equ = m3UaLayer.ProtocolData.Mtp3Equivalents;
                string opc = mtp3Equ?.Opc.ToString();

                //bool isOpcContained = this.opcList.Any(x => opc != null && opc.Contains(x));

                //if (!isOpcContained)
                //    return;

                record[Sn.Opc] = opc;
                record[Sn.Dpc] = mtp3Equ?.Dpc.ToString();
                //record[Sn.ReleaseDirection] = m3UaLayer?.RoutingContext?.RoutingContextValue.ToString();
                //record[Sn.Connectednumbertype] = m3UaLayer.ProtocolData?.ProtocolDataSi.ToString();
                //record[Sn.IdCall] = mtp3Equ?.Ni.ToString();
                //record[Sn.Sequencenumber] = mtp3Equ?.Sls.ToString();
                record[Sn.Sequencenumber] = "0";

                Sccp sccpLayer = packet.Sccp;
                record[Sn.OriginatingCallingNumber] = sccpLayer?.CallingPartyAddress?.GlobalTitle?.CallingDigits?.ToString();
                record[Sn.OriginatingCalledNumber] = sccpLayer?.CalledPartyAddress?.GlobalTitle?.CalledDigits?.ToString();
                //record[Sn.Duration2] = sccpLayer?.CalledPartyAddress.Ssn.ToString();

                // tcap layer
                Tcap tcapLayer = packet.Tcap;

                //transid = tcapLayer.BeginElement?.Otid == null
                //    ? tcapLayer.EndElement?.Otid
                //    : tcapLayer.BeginElement.Otid;
                //record[Sn.InMgwId] = transid?.ToString();

                //transid = tcapLayer.BeginElement?.SourceTransactionId?.Dtid == null
                //    ? tcapLayer.EndElement?.DestinationTransactionId?.Dtid
                //    : tcapLayer.BeginElement.Otid;
                //record[Sn.OutMgwId] = transid?.ToString();

                // gsm sms
                GsmSms gsmSmsLayer = packet.GsmSms;
                //record[Sn.Message] = gsmSmsLayer?.TpUserData?.SmsText?.ToString().EncodeToBase64();

                ComponentTree componentTree = gsmMapLayer.ComponentTree;
                string smsTypeIdentifier = componentTree?.InvokeElement?.OpCodeTree?.LocalValue ??
                                    componentTree?.ReturnResultLastElement?.ResultretresElement?.OpCodeTree?.LocalValue;

                if (smsTypeIdentifier == null)
                {
                    // throw new Exception("OpCode tree local can not be null");
                }
                //if (smsTypeIdentifier == "46")
                //{
                //    ;
                //}
                // excluding reportSM-DeliveryStatus
                if (smsTypeIdentifier != "46" && smsTypeIdentifier != "44" && smsTypeIdentifier != "45" && !smsTypeIdentifier.IsNullOrEmptyOrWhiteSpace())
                    continue;


                if (smsTypeIdentifier == "45")
                    continue;
                // exluding Udts
                if (sccpLayer?.ReturnCause != null)
                    continue;

                record[Sn.OptionalCode] = smsTypeIdentifier;

                //gsm_map.old.Component
                string reqResIdentifier = gsmMapLayer.OldComponent?.ToString();

                string tempSystemCodes = new StringBuilder(reqResIdentifier)
                    .Append(":")
                    .Append(smsTypeIdentifier).ToString();

                // combination of gsm_map.old.Component and gsm_old.localValue
                MsuType systemCodes = MsuType.None;
                systemCodes = this.causeCodes.TryGetValue(tempSystemCodes, out systemCodes) ? systemCodes : MsuType.None;

                // Sms Type ReturnError
                if (reqResIdentifier == "3" && systemCodes == MsuType.None)
                {
                    //record[Sn.SmsType] = "5";
                    continue;
                }

                InvokeElement componentTreeInvokeElement = gsmMapLayer?.ComponentTree?.InvokeElement;
                ResultretresElement componentTreeReturnResultLastElement = gsmMapLayer?.ComponentTree?.ReturnResultLastElement?.ResultretresElement;

                string serviceCentreAddress = componentTreeInvokeElement?.ServiceCenterAddress?.ToString();
                string calledNumber = componentTreeInvokeElement?.MsisdnTree?.Msisdn?.ToString();
                string callerNumber = gsmSmsLayer?.CallerNumber?.Msisdn?.ToString();

                string imsi = componentTreeInvokeElement?.ImsiTree?.Imsi?.ToString();

                // imsi, A Party,B Party
                if (systemCodes == MsuType.Sri)
                {
                    continue;
                    // e164.msisdn => terminating called number ,gsm_map.sm.serviceCentreAddress => redirectnumber
                    record[Sn.TerminatingCalledNumber] = calledNumber;
                    record[Sn.Imsi] = serviceCentreAddress;
                    record[Sn.SmsType] = "1";

                }
                if (systemCodes == MsuType.SriResp)
                {
                    continue;
                    //	e164.msisdn => terminating called number,imsi => redirect number
                    imsi = componentTreeReturnResultLastElement?.Imsi?.ToString();
                    record[Sn.TerminatingCalledNumber] = calledNumber;

                    // actual caller number
                    callerNumber = componentTreeReturnResultLastElement?.LocationInfoWithLmsiElement?.SriResCallerTree?.Msisdn?.ToString();
                    record[Sn.TerminatingCallingNumber] = callerNumber;
                    record[Sn.Imsi] = imsi;
                    record[Sn.SmsType] = "2";
                }

                string transid;
                if (systemCodes == MsuType.Mt)
                {
                    // e164.msisdn => terminating caller number	,imsi => redirectnumber
                    record[Sn.TerminatingCallingNumber] = callerNumber;
                    record[Sn.Imsi] = imsi;
                    record[Sn.SmsType] = "3";
                    transid = tcapLayer.BeginElement.Tid.ToString();
                    record[Sn.Codec] = transid?.ToString();
                }
                if (systemCodes == MsuType.MtResp)
                {
                    // e164.msisdn => terminating caller number, imsi => redirect number
                    record[Sn.TerminatingCallingNumber] = callerNumber;
                    record[Sn.Imsi] = imsi;
                    record[Sn.SmsType] = "4";
                    transid = tcapLayer.EndElement.Tid.ToString();
                    record[Sn.Codec] = transid?.ToString();
                }
                if (systemCodes == MsuType.InformServiceCenter)
                {
                    // e164.msisdn => terminating caller number, imsi => redirect number
                    //record[Sn.TerminatingCallingNumber] = callerNumber;
                    //record[Sn.Imsi] = imsi;
                    //record[Sn.SmsType] = "6";
                    continue;
                }
                if((imsi.IsNullOrEmptyOrWhiteSpace() == false && reqResIdentifier == "2") || reqResIdentifier=="3")
                {
                    ;
                }
                if (record[Sn.SmsType] != "1" && record[Sn.SmsType] != "3" && packet.GsmMap.ComponentTree?.ReturnResultLastElement != null)
                {
                    if (packet.GsmMap.ComponentTree?.ReturnResultLastElement.ResultretresElement.Imsi.IsNullOrEmptyOrWhiteSpace() == false)
                    {
                        //ignore sri for now;

                        //imsi = componentTreeReturnResultLastElement?.Imsi?.ToString();
                        //record[Sn.TerminatingCalledNumber] = calledNumber;

                        //// actual caller number
                        //callerNumber = componentTreeReturnResultLastElement?.LocationInfoWithLmsiElement?.SriResCallerTree?.Msisdn?.ToString();
                        //record[Sn.TerminatingCallingNumber] = callerNumber;
                        //record[Sn.Imsi] = imsi;
                        //record[Sn.SmsType] = "2";
                        continue;
                    }
                    else
                    {
                        record[Sn.TerminatingCallingNumber] = callerNumber;
                        record[Sn.Imsi] = imsi;
                        record[Sn.SmsType] = "4";
                        transid = tcapLayer.EndElement.Tid.ToString();
                        record[Sn.Codec] = transid?.ToString();
                    }
                }

                string outPartnerId = getPartneridByGtPrefix(record[Sn.OriginatingCalledNumber]).ToString();
                string inPartnerId = getPartneridByGtPrefix(record[Sn.OriginatingCallingNumber]).ToString();

                record[Sn.OutPartnerId] = outPartnerId;
                record[Sn.InPartnerId] = inPartnerId;

                string[] gtPair =
                {
                    reqResIdentifier == "2"? outPartnerId :inPartnerId,
                    reqResIdentifier == "2"? inPartnerId :outPartnerId
                };

                DateTime startTime = Convert.ToDateTime(dateTime);
                startTime = startTime.Hour == 23 ? startTime.Date.AddDays(1) : startTime.Date;
                string separator = "/";

                //string syscode ="5";
                //if (localValue == "44" || smsSystemcodes == "3")
                //{
                //    syscode = "3";
                //}
                //if (localValue == "45")
                //{
                //    syscode = "1";
                //}

                record[Sn.UniqueBillId] = new StringBuilder(startTime.ToMySqlFormatDateOnlyWithoutTimeAndQuote())
                    .Append(separator)
                    //.Append(syscode)
                    //.Append(separator)
                    .Append(string.Join("-", gtPair))
                    .Append(separator)
                    .Append(record[Sn.Codec]).ToString();

                record[Sn.Validflag] = "1";
                record[Sn.Partialflag] = "1";
                record[Sn.Category] = "1";
                record[Sn.Subcategory] = "1";
                record[Sn.ChargingStatus] = "0";
                record[Sn.ServiceGroup] = "1";
                
                //if (record[Sn.SmsType] == "3")
                //{
                //    ;
                //}

                //if (record[Sn.SmsType] == "4")
                //{
                //    ;
                //}
                //if (record[Sn.SmsType] != "3" && record[Sn.SmsType] != "4")
                //{
                //    ;
                //}
                //records.Add(record);
                records[index] = record;
            }

            return records.Where(r => r != null).ToList();
        }

        private int getPartneridByGtPrefix(string gt)
        {
            partnerprefix value;
            if (gt.Length >= 7 && ansPrefixes.TryGetValue(gt.Substring(0, 7), out value))
            {
                return value.idPartner;
            }
            else if (gt.Length >= 5 && ansPrefixes.TryGetValue(gt.Substring(0, 5), out value))
            {
                return value.idPartner;
            }
            else return 0;
        }

    }
}

/*
1. Get Packets
   - For A Single Packet, Get Key Value Pairs If Key Matches With Any Sigtran Packet Property
   - For A Single Packet, Get New JObject From Key Value Pairs
   - For List Of Packet, Get New JObject List
   - Map The JObjectList With SigtranPacketList
   - Return SigtranPacketList
2. Get CDR Records
*/



