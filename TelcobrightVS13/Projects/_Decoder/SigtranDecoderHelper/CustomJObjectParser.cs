using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Decoders.SigtranDecoderHelper
{
    public class CustomJObjectParser 
    {
        public static JObject GetCustomJObject(JObject thisJObject)
        {
            //JObject newJObj = new JObject();
            //List<string> parentPropsToFind = new List<string>() { "_index", "_source" };
            Dictionary<string, List<string>> propsToStay = new Dictionary<string, List<string>>()
            {
                {
                    "gsm_map",
                    new List<string>
                    {
                        "gsm_map.old.Component",
                        "gsm_old.localValue",
                        "gsm_map.sm.serviceCentreAddress",
                        "e164.msisdn",
                        "e212.imsi"
                    }
                },
                {"tcap", new List<string> {"tcap.tid", "tcap.otid", "tcap.oid"}},
                {"sccp", new List<string> { "sccp.return_cause", "sccp.called.digits", "sccp.calling.digits", "sccp.ssn"}},
                {
                    "m3ua",
                    new List<string>
                    {
                        "mtp3.opc",
                        "mtp3.dpc",
                        "m3ua.routing_context",
                        "m3ua.protocol_data_si",
                        "mtp3.ni",
                        "mtp3.sls"
                    }
                },
                {"frame", new List<string> {"frame.time_utc"}},
                {"sctp", new List<string> {"sctp.srcport", "sctp.dstport"}},
                {"gsm_sms", new List<string> { "gsm_sms.sms_text" ,"e164.msisdn"}}
            };
            Dictionary<string, List<string>> propsToRename = new Dictionary<string, List<string>>()
            {
                {"sccp", new List<string> {"Called Party address", "Calling Party address", "Global Title 0x4"}},
                {"m3ua", new List<string> {"Protocol data"}},
                {"gsm_sms", new List<string> {"TP-Originating-Address"}},
            };

            //foreach (var pair in thisJObject)
            //{
            //    string key = pair.Key;
            //    JToken value = pair.Value;
            //    if (parentPropsToFind.Contains(key))
            //    {
            //        newJObj.Add(key, value.DeepClone());
            //    }
            //}

            //if (!(newJObj["_source"] == null || newJObj["_source"]["layers"] == null))
            //{
            //JObject layers = (JObject)newJObj["_source"]["layers"];

            JObject layers = (JObject)thisJObject;

            foreach (JProperty layer in layers.Properties().ToArray())
            {

                string layerKey = layer.Name;
                if (!propsToStay.ContainsKey(layerKey))
                {
                    layer.Remove();
                }
                else
                {
                    JObject layerValue = (JObject)layer.Value;
                    RemoveUnwantedProps(layerValue, propsToStay[layerKey]);

                    // New logic to rename properties
                    if (propsToRename.ContainsKey(layerKey))
                    {
                        RenameProps(layerValue, propsToRename[layerKey]);
                    }


                    // Special handling for "sccp" layer
                    if (layerKey == "sccp")
                    {
                        foreach (var prop in propsToRename["sccp"])
                        {
                            foreach (var innerProp in layerValue.Properties().ToList())
                            {
                                if (innerProp.Name.StartsWith(prop))
                                {
                                    string newName = prop;
                                    innerProp.Replace(new JProperty(newName, innerProp.Value));
                                }

                                // Recursively apply renaming to nested objects
                                if (innerProp.Value.Type == JTokenType.Object)
                                {
                                    RenameProps((JObject)innerProp.Value, propsToRename["sccp"]);
                                }
                            }
                        }
                    }

                }
            }
            //}
            return thisJObject;
        }


        static void RemoveUnwantedProps(JObject jObject, List<string> propertiesToKeep)
        {
            foreach (JProperty props in jObject.Properties().ToList())
            {
                if (!propertiesToKeep.Contains(props.Name))
                {
                    if (props.Value.Type == JTokenType.Object)
                    {
                        RemoveUnwantedProps((JObject)props.Value, propertiesToKeep);
                        // If the nested object is empty after removal, remove the property itself
                        if (!((JObject)props.Value).Properties().Any())
                        {
                            props.Remove();
                        }
                    }
                    else
                    {
                        props.Remove();
                    }
                }
            }
        }

        static void RenameProps(JObject jObject, List<string> propsToRename)
        {
            foreach (JProperty property in jObject.Properties().ToList())
            {
                if (propsToRename.Any(pr => property.Name.StartsWith(pr)))
                {
                    string newName = propsToRename.First(pr => property.Name.StartsWith(pr));
                    property.Replace(new JProperty(newName, property.Value));
                }

                JObject nestedObject = property.Value as JObject;
                if (nestedObject != null)
                {
                    RenameProps(nestedObject, propsToRename);
                }
            }
        }

    }

}