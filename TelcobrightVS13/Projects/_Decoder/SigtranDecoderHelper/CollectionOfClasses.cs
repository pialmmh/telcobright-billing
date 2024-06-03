using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Decoders;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Decoders
{
    public class Ip
    {
        [JsonProperty("ip.src")]
        public string SrcIp { get; set; }

        [JsonProperty("ip.dst")]
        public string DstIp { get; set; }
    }
    public class SigtranPacket
    {
        [JsonProperty("_index")]
        public string Index { get; set; }

        [JsonProperty("_source")]
        public Source Source { get; set; }
    }

    public class Source
    {
        [JsonProperty("layers")]
        public Layers Layers { get; set; }
    }

    public class Layers
    {
        [JsonProperty("frame")]
        public Frame Frame { get; set; }

        [JsonProperty("sctp")]
        public Sctp Sctp { get; set; }

        [JsonProperty("m3ua")]
        public M3ua M3Ua { get; set; }

        [JsonProperty("sccp")]
        public Sccp Sccp { get; set; }

        [JsonProperty("tcap")]
        public Tcap Tcap { get; set; }

        [JsonProperty("gsm_map")]
        public GsmMap GsmMap { get; set; }

        [JsonProperty("gsm_sms")]
        public GsmSms GsmSms { get; set; }
    }

    public class Frame
    {
        [JsonProperty("frame.time_utc")]
        public string FrameTimeUtc { get; set; }
    }

    public class Sctp
    {
        [JsonProperty("sctp.srcport")]
        public string SrcPort { get; set; }

        [JsonProperty("sctp.dstport")]
        public string DstPort { get; set; }
    }

    public class M3ua
    {
        [JsonProperty("Routing context (1 context)")]
        public RoutingContext RoutingContext { get; set; }

        [JsonProperty("Protocol data")]
        public ProtocolData ProtocolData { get; set; }
    }

    public class RoutingContext
    {
        [JsonProperty("m3ua.routing_context")]
        public string RoutingContextValue { get; set; }
    }

    public class ProtocolData
    {
        [JsonProperty("m3ua.protocol_data_si")]
        public string ProtocolDataSi { get; set; }

        [JsonProperty("MTP3 equivalents")]
        public Mtp3Equivalents Mtp3Equivalents { get; set; }
    }

    public class Mtp3Equivalents
    {
        [JsonProperty("mtp3.opc")]
        public string Opc { get; set; }

        [JsonProperty("mtp3.dpc")]
        public string Dpc { get; set; }

        [JsonProperty("mtp3.ni")]
        public string Ni { get; set; }

        [JsonProperty("mtp3.sls")]
        public string Sls { get; set; }
    }

    public class Sccp
    {
        [JsonProperty("Called Party address")]
        public PartyAddress CalledPartyAddress { get; set; }

        [JsonProperty("Calling Party address")]
        public PartyAddress CallingPartyAddress { get; set; }
    }

    public class PartyAddress
    {
        [JsonProperty("sccp.ssn")]
        public string Ssn { get; set; }

        [JsonProperty("Global Title 0x4")]
        public GlobalTitle GlobalTitle { get; set; }
    }

    public class GlobalTitle
    {
        [JsonProperty("sccp.called.digits", NullValueHandling = NullValueHandling.Ignore)]
        public string CalledDigits { get; set; }

        [JsonProperty("sccp.calling.digits", NullValueHandling = NullValueHandling.Ignore)]
        public string CallingDigits { get; set; }
    }

    public class Tcap
    {
        [JsonProperty("tcap.begin_element")]
        public Element BeginElement { get; set; }
        [JsonProperty("tcap.end_element")]
        public Element EndElement { get; set; }
    }

    public class Element
    {
        [JsonProperty("tcap.tid")]
        public string Tid { get; set; }

        [JsonProperty("Source Transaction ID")]
        public SourceTransactionID SourceTransactionID { get; set; }

        [JsonProperty("tcap.otid")]
        public string Otid { get; set; }
    }

    public class SourceTransactionID
    {
        [JsonProperty("tcap.dtid")]
        public string Dtid { get; set; }
    }

    public class GsmMap
    {
        [JsonProperty("gsm_map.old.Component")]
        public string OldComponent { get; set; }

        [JsonProperty("gsm_map.old.Component_tree")]
        public ComponentTree ComponentTree { get; set; }
    }

    public class ComponentTree
    {
        [JsonProperty("gsm_old.invoke_element")]
        public InvokeElement InvokeElement { get; set; }
    }

    public class InvokeElement
    {
        [JsonProperty("gsm_old.opCode_tree")]
        public OpCodeTree OpCodeTree { get; set; }
        [JsonProperty("gsm_map.sm.msisdn_tree")]
        public MsisdnTree MsisdnTree { get; set; }

        [JsonProperty("gsm_map.sm.sm_RP_DA_tree")]
        public SmRpDaTree SmRpDaTree { get; set; }

        [JsonProperty("gsm_map.sm.sm_RP_OA_tree")]
        public SmRpOaTree SmRpOaTree { get; set; }
        [JsonProperty("gsm_map.sm.serviceCentreAddress")]
        public string ServiceCenterAddress { get; set; }
    }
    
    public class OpCodeTree
    {
        [JsonProperty("gsm_old.localValue")]
        public string LocalValue { get; set; }
    }
    
    public class MsisdnTree
    {
        [JsonProperty("e164.msisdn")]
        public string Msisdn { get; set; }
    }
    public class SmRpDaTree
    {
        [JsonProperty("e212.imsi")]
        public string Imsi { get; set; }
    }

    public class SmRpOaTree
    {
        [JsonProperty("gsm_map.sm.serviceCentreAddressOA_tree")]
        public ServiceCentreAddressOaTree ServiceCentreAddressOaTree { get; set; }
    }

    public class ServiceCentreAddressOaTree
    {
        [JsonProperty("e164.msisdn")]
        public string Msisdn { get; set; }
    }

    public class GsmSms
    {
        [JsonProperty("TP-User-Data")]
        public TpUserData TpUserData { get; set; }
    }

    public class TpUserData
    {
        [JsonProperty("gsm_sms.sms_text")]
        public string SmsText { get; set; }
    }
}
