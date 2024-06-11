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
using Decoders.SigtranDecoderHelper;
using TelcobrightMediation;


namespace Decoders
{
    static class NativeMethods
    {
        [DllImport("kernel32.dll")]
        public static extern IntPtr LoadLibrary(string dllToLoad);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

        [DllImport("kernel32.dll")]
        public static extern bool FreeLibrary(IntPtr hModule);
    }

    public class JsonStringBuilderParser
    {
        public static List<JObject> ParseJsonArray(StringBuilder stringBuilder)
        {
            using (var jsonReader = new JsonTextReader(new StringBuilderReader(stringBuilder)))
            {
                jsonReader.SupportMultipleContent = true;
                var serializer = new JsonSerializer();
                var list = new List<JObject>();

                while (jsonReader.Read())
                {
                    if (jsonReader.TokenType == JsonToken.StartObject)
                    {
                        JObject obj = serializer.Deserialize<JObject>(jsonReader);
                        list.Add(obj);
                    }
                }

                return list;
            }
        }

        // Custom StringReader implementation for StringBuilder
        private class StringBuilderReader : TextReader
        {
            private StringBuilder _stringBuilder;
            private int _position;

            public StringBuilderReader(StringBuilder stringBuilder)
            {
                _stringBuilder = stringBuilder;
                _position = 0;
            }

            public override int Read()
            {
                if (_position >= _stringBuilder.Length)
                    return -1;
                else
                    return _stringBuilder[_position++];
            }

            public override int Peek()
            {
                if (_position >= _stringBuilder.Length)
                    return -1;
                else
                    return _stringBuilder[_position];
            }
        }
    }

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

        private readonly Dictionary<string, SmsType> causeCodes = new Dictionary<string, SmsType>
        {
            {"1:44",SmsType.InvokeMtForwardSm},
            {"2:44",SmsType.ReturnResultLastMtForwardSm},
            {"1:45",SmsType.InvokeSendRoutingInfoForSm},
            {"2:45",SmsType.ReturnResultLastSendRoutingInfoForSm},
            {"1:63",SmsType.InvokeInformServiceCenter}
        };
        public SigtranPacketDecoder(string pcapFilename, Dictionary<string, partnerprefix> ansPrefixes)
        {
            this.PcapFileName = pcapFilename;
            this.ansPrefixes = ansPrefixes;
        }


        List<JObject> DecodePacket()
        {
            Process senderProcess = new Process();
            senderProcess.StartInfo.FileName = TSharkExe;
            senderProcess.StartInfo.Arguments = PcapFileName;
            senderProcess.StartInfo.UseShellExecute = false;
            //senderProcess.StartInfo.RedirectStandardOutput = true;
            senderProcess.StartInfo.CreateNoWindow = true;
            senderProcess.Start();
            senderProcess.WaitForExit();
            var data = JsonStringReader.ReadJsonObjects(filePath);
            var objectList = JsonStringReader.ParseJsonStrings(data);
            return objectList;
        }

        public List<SigtranPacket> GetPackets()
        {
            List<JObject> packetList = DecodePacket();
            packetList = GetNewJObjectList(packetList);
            List<SigtranPacket> sigtranPackets =
                packetList.Select(packet => packet.ToObject<SigtranPacket>()).ToList();

            return sigtranPackets;
        }
        private static List<KeyValuePair<string, JToken>> GetKeyValuePairs(JObject jsonObject, string parentKey = "")
        {
            List<KeyValuePair<string, JToken>> pairs = new List<KeyValuePair<string, JToken>>();
            foreach (var property in jsonObject.Properties())
            {
                string key = property.Name;
                JToken value = property.Value;
                bool isJbject = value is JObject;
                if (isJbject)
                {
                    var nestedObject = (JObject)value;
                    if (DoesKeyMatchProperty(key))
                    {
                        parentKey = key;
                    }
                    pairs.AddRange(GetKeyValuePairs(nestedObject, parentKey));
                }
                else
                {
                    if (parentKey != "")
                    {
                        pairs.Add(new KeyValuePair<string, JToken>(parentKey + "," + key, value));
                    }
                    else
                    {
                        pairs.Add(new KeyValuePair<string, JToken>(key, value));
                    }
                }
            }
            return pairs;
        }

        public static bool DoesKeyMatchProperty(string key)
        {
            var properties = typeof(SigtranPacket).GetProperties();

            foreach (var property in properties)
            {
                var attribute = property.GetCustomAttributes(typeof(JsonPropertyAttribute), true)
                                       .FirstOrDefault() as JsonPropertyAttribute;

                if (attribute != null && attribute.PropertyName == key)
                {
                    return true;
                }
            }

            return false;
        }

        static JObject GetNewJObject(List<KeyValuePair<string, JToken>> keyValuePairs)
        {
            JObject packet = new JObject();
            foreach (var pair in keyValuePairs)
            {
                string[] keys = pair.Key.Split(',');
                string parentKey = keys[0];
                if (keys.Length > 1)
                {
                    if (packet[parentKey] == null)
                    {
                        packet[parentKey] = new JObject();
                    }
                    packet[parentKey][keys[1]] = pair.Value;
                }
                else
                {
                    packet[parentKey] = pair.Value;
                }
            }
            return packet;
        }


        private static List<JObject> GetNewJObjectList(List<JObject> packets)
        {
            ConcurrentBag<JObject> packetBag = new ConcurrentBag<JObject>();

            Parallel.ForEach(packets.Cast<JObject>(), packet =>
            {
                packetBag.Add(CustomJObjectParser.GetCustomJObject(packet));
            });

            //Parallel.ForEach(packets.Cast<JObject>(), packet =>
            //{
            //    packetBag.Add(GetNewJObject(GetKeyValuePairs(packet)));
            //});

            return packetBag.ToList();
        }
        string ParseAndFormatTimestamp(string timestamp)
        {
            timestamp = timestamp.Substring(0, timestamp.Length - 4);
            DateTime parsedDate = DateTimeOffset.Parse(timestamp).DateTime.AddHours(6);
            return parsedDate.ToString("yyyy-MM-dd HH:mm:ss");
        }


        public List<string[]> CdrRecords(List<SigtranPacket> packets)
        {
            ConcurrentBag<string[]> records = new ConcurrentBag<string[]>();

            Parallel.ForEach(packets, packet =>
            {
                string[] record = Enumerable.Repeat((string)null, 104).ToArray();

                // gms layer
                GsmMap gsmMapLayer = packet.GsmMap;
                if (gsmMapLayer == null) return;

                Frame frameLayer = packet.Frame;
                string dateTime = ParseAndFormatTimestamp(frameLayer?.FrameTimeUtc.ToString());
                record[Sn.StartTime] = dateTime;
                record[Sn.AnswerTime] = dateTime;

                record[Sn.Duration1] = "60";
                record[Sn.DurationSec] = "60";

                //record[Sn.Originatingip] = packet.Ip?.SrcIp?.ToString();
                //record[Sn.TerminatingIp] = packet.Ip?.DstIp?.ToString();

                // sctp layer
                Sctp sctpLayer = packet.Sctp;
                record[Sn.IncomingRoute] = sctpLayer?.SrcPort.ToString();
                record[Sn.OutgoingRoute] = sctpLayer?.DstPort.ToString();

                // m3ua layer
                M3ua m3UaLayer = packet.M3Ua;
                Mtp3Equivalents mtp3Equ = m3UaLayer.ProtocolData.Mtp3Equivalents;
                string opc = mtp3Equ?.Opc.ToString();

                bool isOpcContained = this.opcList.Any(x => opc != null && opc.Contains(x));

                if (!isOpcContained)
                    return;

                record[Sn.Opc] = opc;
                record[Sn.Dpc] = mtp3Equ?.Dpc.ToString();
                record[Sn.ReleaseDirection] = m3UaLayer?.RoutingContext?.RoutingContextValue.ToString();
                record[Sn.Connectednumbertype] = m3UaLayer.ProtocolData?.ProtocolDataSi.ToString();
                record[Sn.IdCall] = mtp3Equ?.Ni.ToString();
                record[Sn.Sequencenumber] = mtp3Equ?.Sls.ToString();

                Sccp sccpLayer = packet.Sccp;
                record[Sn.OriginatingCallingNumber] = sccpLayer?.CallingPartyAddress?.GlobalTitle?.CallingDigits?.ToString();
                record[Sn.OriginatingCalledNumber] = sccpLayer?.CalledPartyAddress?.GlobalTitle?.CalledDigits?.ToString();
                //record[Sn.Duration2] = sccpLayer?.CalledPartyAddress.Ssn.ToString();

                // tcap layer
                Tcap tcapLayer = packet.Tcap;

                var transid = tcapLayer.BeginElement?.Tid == null ? tcapLayer.EndElement?.Tid : tcapLayer.BeginElement.Tid;
                if (transid == null)
                {
                    transid = tcapLayer.ContinueElement?.Tid;
                }
                record[Sn.Codec] = transid?.ToString();

                transid = tcapLayer.BeginElement?.Otid == null
                    ? tcapLayer.EndElement?.Otid
                    : tcapLayer.BeginElement.Otid;
                record[Sn.InMgwId] = transid?.ToString();

                transid = tcapLayer.BeginElement?.SourceTransactionId?.Dtid == null
                    ? tcapLayer.EndElement?.DestinationTransactionId?.Dtid
                    : tcapLayer.BeginElement.Otid;
                record[Sn.OutMgwId] = transid?.ToString();

                // gsm sms
                GsmSms gsmSmsLayer = packet.GsmSms;
                record[Sn.AdditionalMetaData] = gsmSmsLayer?.TpUserData?.SmsText?.ToString().EncodeToBase64();

                ComponentTree componentTree = gsmMapLayer.ComponentTree;
                string localValue = componentTree?.InvokeElement?.OpCodeTree?.LocalValue ??
                                    componentTree?.ReturnResultLastElement?.ResultretresElement?.OpCodeTree?.LocalValue;

                if (localValue == null)
                {
                    // throw new Exception("OpCode tree local can not be null");
                }
                // excluding reportSM-DeliveryStatus
                if (localValue == "47") return;

                record[Sn.OptionalCode] = localValue;

                //gsm_map.old.Component
                string smsSystemcodes = gsmMapLayer.OldComponent?.ToString();

                string tempSystemCodes = new StringBuilder(smsSystemcodes)
                    .Append(":")
                    .Append(localValue).ToString();

                // combination of gsm_map.old.Component and gsm_old.localValue
                SmsType systemCodes = SmsType.None;
                systemCodes = this.causeCodes.TryGetValue(tempSystemCodes, out systemCodes) ? systemCodes : SmsType.None;

                // Sms Type ReturnError
                if (smsSystemcodes == "3" && systemCodes == SmsType.None)
                {
                    record[Sn.SmsType] = "5";
                }

                InvokeElement componentTreeInvokeElement = gsmMapLayer?.ComponentTree?.InvokeElement;
                ResultretresElement componentTreeReturnResultLastElement = gsmMapLayer?.ComponentTree?.ReturnResultLastElement?.ResultretresElement;

                string serviceCentreAddress = componentTreeInvokeElement?.ServiceCenterAddress?.ToString();
                string calledNumber = componentTreeInvokeElement?.MsisdnTree?.Msisdn?.ToString();
                string callerNumber = gsmSmsLayer?.CallerNumber?.Msisdn?.ToString();

                string imsi = componentTreeInvokeElement?.ImsiTree?.Imsi?.ToString();

                // imsi, A Party,B Party
                if (systemCodes == SmsType.InvokeSendRoutingInfoForSm)
                {
                    // e164.msisdn => terminating called number ,gsm_map.sm.serviceCentreAddress => redirectnumber
                    record[Sn.TerminatingCalledNumber] = calledNumber;
                    record[Sn.Imsi] = serviceCentreAddress;
                    record[Sn.SmsType] = "1";
                }
                if (systemCodes == SmsType.ReturnResultLastSendRoutingInfoForSm)
                {
                    //	e164.msisdn => terminating called number,imsi => redirect number
                    imsi = componentTreeReturnResultLastElement?.Imsi?.ToString();
                    record[Sn.TerminatingCalledNumber] = calledNumber;

                    // actual caller number
                    callerNumber = componentTreeReturnResultLastElement?.LocationInfoWithLmsiElement?.SriResCallerTree?.Msisdn?.ToString();
                    record[Sn.TerminatingCallingNumber] = callerNumber;
                    record[Sn.Imsi] = imsi;
                    record[Sn.SmsType] = "2";
                }
                if (systemCodes == SmsType.InvokeMtForwardSm)
                {
                    // e164.msisdn => terminating caller number	,imsi => redirectnumber
                    record[Sn.TerminatingCallingNumber] = callerNumber;
                    record[Sn.Imsi] = imsi;
                    record[Sn.SmsType] = "3";
                }
                if (systemCodes == SmsType.ReturnResultLastMtForwardSm)
                {
                    // e164.msisdn => terminating caller number, imsi => redirect number
                    record[Sn.TerminatingCallingNumber] = callerNumber;
                    record[Sn.Imsi] = imsi;
                    record[Sn.SmsType] = "4";
                }
                if (systemCodes == SmsType.InvokeInformServiceCenter)
                {
                    // e164.msisdn => terminating caller number, imsi => redirect number
                    record[Sn.TerminatingCallingNumber] = callerNumber;
                    record[Sn.Imsi] = imsi;
                    record[Sn.SmsType] = "6";
                }

                string outPartnerId = getPartneridByGtPrefix(record[Sn.OriginatingCalledNumber]).ToString();
                string inPartnerId = getPartneridByGtPrefix(record[Sn.OriginatingCallingNumber]).ToString();

                record[Sn.OutPartnerId] = outPartnerId;
                record[Sn.InPartnerId] = inPartnerId;

                string[] gtPair =
                {
                    outPartnerId,
                    inPartnerId
                };
                Array.Sort(gtPair);

                DateTime startTime = Convert.ToDateTime(dateTime);
                startTime = startTime.Hour == 23 ? startTime.Date.AddDays(1) : startTime.Date;
                string separator = "/";
                record[Sn.UniqueBillId] = new StringBuilder(startTime.ToMySqlFormatDateOnlyWithoutTimeAndQuote())
                    .Append(separator)
                    .Append(string.Join("-", gtPair))
                    .Append(separator)
                    .Append(record[Sn.Codec]).ToString();

                record[Sn.Validflag] = "1";
                record[Sn.Partialflag] = "1";

                records.Add(record);
            });

            return records.ToList();
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



