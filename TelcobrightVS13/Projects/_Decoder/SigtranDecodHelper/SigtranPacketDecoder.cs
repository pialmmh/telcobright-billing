using System.Diagnostics;
using System.Runtime.InteropServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MediationModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Decoders
{
   
    class SigtranPacketDecoder
    {
        private string PcapFileName { get; set; }
        private Dictionary<string, partnerprefix> Ansprefixes {get;set;}
        public SigtranPacketDecoder(string pcapFilename, Dictionary<string, partnerprefix> ansprefixes)
        {
            this.PcapFileName = pcapFilename;
            this.Ansprefixes = ansprefixes;
        }
        private const string LibTsharkDllPath = "C:\\Development\\wsbuild64\\run\\RelWithDebInfo\\libtshark.dll";

        public static Dictionary<string, string> GT_Prefix = new Dictionary<string, string>() {
            { "71","GP"},
            { "71","GP"},
        };
        public void populatePrefix()
        {
            GT_Prefix["88071"] = "GP";
            GT_Prefix["88091"] = "BL";
            GT_Prefix["88081"] = "RB";
            GT_Prefix["8809638"] = "IC";
            GT_Prefix["88017"] = "GP";
        }
        [DllImport(LibTsharkDllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr Tb_Main(string filename);

        public List<SigtranPacket> GetPackets()
        {
            IntPtr resultPtr = Tb_Main(this.PcapFileName);
            string result = Marshal.PtrToStringAnsi(resultPtr);
            Marshal.FreeCoTaskMem(resultPtr);
            List<JObject> packetList = new List<JObject>();
            if (result != null) packetList = GetNewJObjectList(result);
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
                if (keys.Length>1)
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

        private static List<JObject> GetNewJObjectList(string result)
        {
            var packets = JArray.Parse(result);
            List<JObject> packetList = new List<JObject>();
            foreach (var packet in packets.Cast<JObject>())
            {
                packetList.Add(GetNewJObject(GetKeyValuePairs(packet)));
            }
            return packetList;
        }

        public List<string[]> CdrRecords(List<SigtranPacket> packets)
        {
            List<string[]> records = new List<string[]>();
            foreach (var packet in packets)
            {
                string[] record = Enumerable.Repeat((string)null, 104).ToArray();

                record[Fn.Originatingip] = packet.Ip.SrcIp;
                record[Fn.TerminatingIp] = packet.Ip.DstIp;

                record[Fn.IncomingRoute] = packet.Sctp.ToString();
                record[Fn.OutgoingRoute] = packet.Sctp.ToString();

                record[Fn.Opc] = packet.M3Ua.Opc.ToString();
                record[Fn.Dpc] = packet.M3Ua.Dpc.ToString();
                record[Fn.ReleaseDirection] = packet.M3Ua.RoutingContext.ToString();
                record[Fn.Connectednumbertype] = packet.M3Ua.Si.ToString();
                record[Fn.IdCall] = packet.M3Ua.Ni.ToString();
                record[Fn.Sequencenumber] = packet.M3Ua.Sls.ToString();

                record[Fn.OriginatingCallingNumber] = packet.Sccp.CallingPartyGt;
                record[Fn.OriginatingCalledNumber] = packet.Sccp.CalledPartyGt;
                record[Fn.AdditionalSystemCodes] = packet.Sccp.Ssn.ToString();

                record[Fn.Codec] = packet.Tcap.Tid;
                record[Fn.InMgwId] = packet.Tcap.Otid;
                record[Fn.OutMgwId] = packet.Tcap.Dtid;

                record[Fn.AdditionalMetaData] = packet.GSM_MAP.Sms;

                string[] GT_Pair = { Extract_GT_Prefix(record[Fn.OriginatingCalledNumber]), Extract_GT_Prefix(record[Fn.OriginatingCallingNumber]) };
                Array.Sort(GT_Pair);
                record[Fn.UniqueBillId] = GT_Pair[0] + "-" + GT_Pair[1] + "/" + record[Fn.Codec];

                records.Add(record);
            }
            return records;
        }

        

        private static string Extract_GT_Prefix(string GT)
        {
            string value;
            if (GT_Prefix.TryGetValue(GT.Substring(0, 7), out value))
            {
                return GT_Prefix[GT.Substring(0, 7)];
            }
            else if (GT_Prefix.TryGetValue(GT.Substring(0, 5), out value))
            {
                return GT_Prefix[GT.Substring(0, 5)];
            }
            else return null;
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

