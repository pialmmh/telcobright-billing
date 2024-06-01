using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Decoders
{
    public class Frame
    {
        [JsonProperty("frame.time_utc")]
        public string Timestamp { get; set; }
    }
    public  class Ip
    {
        [JsonProperty("ip.src")]
        public string SrcIp { get; set; }

        [JsonProperty("ip.dst")]
        public string DstIp { get; set; }
    }
    public class Sctp
    {
        [JsonProperty("sctp.srcport")]
        public int SrcPort { get; set; }

        [JsonProperty("sctp.dstport")]
        public int DstPort { get; set; }
    }
    public class M3Ua
    {
        [JsonProperty("mtp3.opc")]
        public int Opc { get; set; }

        [JsonProperty("mtp3.dpc")]
        public int Dpc { get; set; }

        [JsonProperty("m3ua.routing_context")]
        public long RoutingContext { get; set; }

        [JsonProperty("m3ua.protocol_data_si")]
        public int Si { get; set; }

        [JsonProperty("mtp3.ni")]
        public int Ni { get; set; }

        [JsonProperty("mtp3.sls")]
        public int Sls { get; set; }
    }
    public class Sccp
    {
        [JsonProperty("sccp.called.digits")]
        public string CalledPartyGt { get; set; }

        [JsonProperty("sccp.calling.digits")]
        public string CallingPartyGt { get; set; }

        [JsonProperty("sccp.ssn")]
        public int Ssn { get; set; }
    }
    public class Tcap
    {
        [JsonProperty("tcap.tid")]
        public string Tid { get; set; }

        [JsonProperty("tcap.otid")]
        public string Otid { get; set; }

        [JsonProperty("tcap.dtid")]
        public string Dtid { get; set; }
    }
    public class GSM_MAP
    {
        [JsonProperty("gsm_sms.sms_text")]
        public string Sms { get; set; }

        [JsonProperty("gsm_old.localValue")]
        public string LocalValue { get; set; }

        [JsonProperty("e212.imsi")]
        public string Imsi { get; set; }

        [JsonProperty("gsm_map.old.Component")]
        public string SystemCodes { get; set; }

        [JsonProperty("gsm_map.sm.serviceCentreAddress")]
        public string ServiceCentreAddress { get; set; }

        [JsonProperty("e164.msisdn")]
        public string SmsPartyNum { get; set; }

    }

    public class SigtranPacket
    {
        [JsonProperty("frame")]
        public Frame Frame { get; set; }

        [JsonProperty("ip")]
        public Ip Ip { get; set; }

        [JsonProperty("sctp")]
        public Sctp Sctp { get; set; }

        [JsonProperty("m3ua")]
        public M3Ua M3Ua { get; set; }

        [JsonProperty("sccp")]
        public Sccp Sccp { get; set; }

        [JsonProperty("tcap")]
        public Tcap Tcap { get; set; }

        [JsonProperty("gsm_map")]
        public GSM_MAP GSM_MAP { get; set; }
    }
}
