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

    public class Packet
    {
        [JsonProperty("_source")]
        public Source source { get; set; }
    }

    public class Source
    {
        [JsonProperty("layers")]
        public Layers layers { get; set; }
    }
    public class Layers
    {
        [JsonProperty("frame.time")]
        public string[] FrameTime { get; set; }

        [JsonProperty("sccp.return_cause")]
        public string[] ReturnCause { get; set; }

        [JsonProperty("mtp3.opc")]
        public string[] Opc { get; set; }

        [JsonProperty("mtp3.dpc")]
        public string[] Dpc { get; set; }

        [JsonProperty("sccp.called.digits")]
        public string[] CalledDigits { get; set; }

        [JsonProperty("sccp.calling.digits")]
        public string[] CallingDigits { get; set; }

        [JsonProperty("tcap.tid")]
        public string[] Tid { get; set; }

        [JsonProperty("gsm_map.old.Component")]
        public string[] OldComponent { get; set; }

        [JsonProperty("e212.imsi")]
        public string[] Imsi { get; set; }

        [JsonProperty("gsm_old.localValue")]
        public string[] LocalValue { get; set; }

        [JsonProperty("e164.msisdn")]
        public string[] CallerNumber { get; set; }

        [JsonProperty("gsm_sms.sms_text")]
        public string[] SmsText { get; set; }

        [JsonProperty("gsm_map.sm.serviceCentreAddress")]
        public string[] serviceCentreAddress { get; set; }

        [JsonProperty("gsm_map.sm.msisdn")]
        public string[] msisdn { get; set; }

        


    }


    public static class PacketAmplifier {
        public static List<Layers> Amplify(Layers l, int targetNoOfInstances) {
            var newLayers = new List<Layers>();
            for(int i = 0; i < targetNoOfInstances; i++)
            {
                newLayers.Add(new Layers
                {
                    FrameTime = l.FrameTime,
                    ReturnCause = l.ReturnCause,
                    Opc = l.Opc,
                    Dpc = l.Dpc,
                    CalledDigits = l.CalledDigits,
                    CallingDigits = l.CallingDigits,
                    Tid = l.Tid,
                    OldComponent = new[] { l.OldComponent[i] } ,
                    Imsi = l.Imsi,
                    LocalValue = new[] { l.LocalValue[i] },
                    CallerNumber = l.CallerNumber,
                    SmsText = l.SmsText,
                    serviceCentreAddress = l.serviceCentreAddress,
                    msisdn = l.msisdn,
                });
            }
            return newLayers;
        }
    }

}
