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
        public SigtranPacketDecoder(string pcapFilename, Dictionary<string, partnerprefix> ansPrefixes)
        {
            this.PcapFileName = pcapFilename;
            this.ansPrefixes = ansPrefixes;
        }

        StringBuilder DecodePacket()
        {
            StringBuilder data = new StringBuilder();
            Process senderProcess = new Process();
            senderProcess.StartInfo.FileName = TSharkExe;
            senderProcess.StartInfo.Arguments = PcapFileName;
            senderProcess.StartInfo.UseShellExecute = false;
            //senderProcess.StartInfo.RedirectStandardOutput = true;
            senderProcess.StartInfo.CreateNoWindow = true;
            senderProcess.Start();
            senderProcess.WaitForExit();
            data = ReadFromMemoryMappedFile();
            return data;
        }

        StringBuilder ReadFromMemoryMappedFile()
        {
            StringBuilder output = new StringBuilder();
            const int chunkSize = 200 * 1024 * 1024; // 1 MB chunk size (adjust as needed)
            using (MemoryMappedFile mmf = MemoryMappedFile.CreateFromFile(filePath, FileMode.Open, null))
            {
                using (MemoryMappedViewAccessor accessor = mmf.CreateViewAccessor())
                {
                    // Get the length of the memory-mapped file
                    long fileSize = new FileInfo(filePath).Length;

                    byte[] buffer = new byte[chunkSize];
                    for (long offset = 0; offset < fileSize; offset += chunkSize)
                    {
                        int bytesToRead = (int)Math.Min(chunkSize, fileSize - offset);
                        accessor.ReadArray(offset, buffer, 0, bytesToRead);
                        output.Append(Encoding.UTF8.GetString(buffer, 0, bytesToRead));
                    }
                    mmf.Dispose();
                }
            }
            return output;
        }

        public List<SigtranPacket> GetPackets()
        {
            StringBuilder output = DecodePacket();
            List<JObject> packetList = new List<JObject>();
            if (output != null) packetList = GetNewJObjectList(output);

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

        private static List<JObject> GetNewJObjectList(StringBuilder result)
        {
            List<JObject> packets = JsonStringBuilderParser.ParseJsonArray(result);
            ConcurrentBag<JObject> packetBag = new ConcurrentBag<JObject>();

            Parallel.ForEach(packets.Cast<JObject>(), packet =>
            {
                packetBag.Add(GetNewJObject(GetKeyValuePairs(packet)));
            });

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
                if (packet.GSM_MAP == null) return;

                record[Fn.StartTime] = ParseAndFormatTimestamp(packet.Frame?.Timestamp);
                record[Fn.AnswerTime] = ParseAndFormatTimestamp(packet.Frame?.Timestamp);

                record[Fn.Duration1] = "60";
                record[Fn.DurationSec] = "60";



                record[Fn.Originatingip] = packet.Ip?.SrcIp?.ToString();
                record[Fn.TerminatingIp] = packet.Ip?.DstIp?.ToString();

                record[Fn.IncomingRoute] = packet.Sctp?.SrcPort.ToString();
                record[Fn.OutgoingRoute] = packet.Sctp?.DstPort.ToString();

                string opc = packet.M3Ua?.Opc.ToString();
                if (opc.IsNullOrEmptyOrWhiteSpace())
                {
                    throw new Exception("opc can not be null or empty");
                }

                List<string> opcList = new List<string>()
                {
                    "4702",
                    "4699",
                    "4700",
                    "4701"
                };

                bool isOpcContained = opcList.Any(x => opc.Contains(x));

                if (!isOpcContained)
                    return;

                record[Fn.Opc] = opc;
                record[Fn.Dpc] = packet.M3Ua?.Dpc.ToString();
                record[Fn.ReleaseDirection] = packet.M3Ua?.RoutingContext.ToString();
                record[Fn.Connectednumbertype] = packet.M3Ua?.Si.ToString();
                record[Fn.IdCall] = packet.M3Ua?.Ni.ToString();
                record[Fn.Sequencenumber] = packet.M3Ua?.Sls.ToString();

                record[Fn.OriginatingCallingNumber] = packet.Sccp?.CallingPartyGt;
                record[Fn.OriginatingCalledNumber] = packet.Sccp?.CalledPartyGt;
                record[Fn.Duration3] = packet.Sccp?.Ssn.ToString();

                record[Fn.Codec] = packet.Tcap?.Tid;
                record[Fn.InMgwId] = packet.Tcap?.Otid;
                record[Fn.OutMgwId] = packet.Tcap?.Dtid;

                record[Fn.AdditionalMetaData] = packet.GSM_MAP?.Sms?.ToString().EncodeToBase64();

                //record[Fn.AdditionalMetaData] = packet.GSM_MAP?.Sms?.ToString().Replace("'", " ").Replace(@"\", @"\\");
                //    .Replace("\n", "")
                //    .Replace("\t", "")
                //    .Replace("\r", "");

                record[Fn.Duration4] = packet.GSM_MAP?.LocalValue?.ToString();

                string[] gtPair =
                {
                    ExtractGtPrefix(record[Fn.OriginatingCalledNumber]).ToString(),
                    ExtractGtPrefix(record[Fn.OriginatingCallingNumber]).ToString()
                };
                Array.Sort(gtPair);


                string separator = "/";

                record[Fn.UniqueBillId] = new StringBuilder(string.Join("-", gtPair))
                    .Append(separator)
                    .Append(record[Fn.Codec]).ToString();

                //record[Fn.UniqueBillId] = GT_Pair[0] + "-" 
                //                        + GT_Pair[1] + "/" 
                //                        + timeWindow + "/" 
                //                        + record[Fn.Codec];

                record[Fn.Validflag] = "1";
                record[Fn.Partialflag] = "1";

                records.Add(record);
            });

            return records.ToList();
        }

        private int ExtractGtPrefix(string gt)
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

