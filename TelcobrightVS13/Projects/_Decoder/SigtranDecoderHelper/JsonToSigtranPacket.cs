using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Decoders.SigtranDecoderHelper
{
    class JsonToSigtranPacket
    {
        public static List<SigtranPacket> ConvertJsonToSigtranPacket(string json)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            //string json = "";
            //using (MemoryMappedFile mmf = MemoryMappedFile.CreateFromFile(filePath, FileMode.Open, null))
            //{
            //    // Create a view accessor to access the memory-mapped file
            //    using (MemoryMappedViewAccessor accessor = mmf.CreateViewAccessor())
            //    {
            //        // Determine the size of the file
            //        long fileSize = new FileInfo(filePath).Length;

            //        // Allocate a buffer to hold the file's content
            //        byte[] buffer = new byte[fileSize];

            //        // Read the file content into the buffer
            //        accessor.ReadArray(0, buffer, 0, buffer.Length);

            //        // Convert the buffer to a string and print it
            //        json = Encoding.UTF8.GetString(buffer);
            //    }
            //}
            json = ReplaceCurlyBracesInGsmSmsText(json);

            List<string> packetStrs = ExtractPackets(json);

            //File.Delete(filePath);
            //List<JObject> packets_1 = packetStrs.Select(s => JObject.Parse(s))
            //    .ToList();

            List<Packet> packets = packetStrs.Select(JsonConvert.DeserializeObject<Packet>)
                .ToList();

            var layers =
                packets.Select(p =>
                    {
                        string[] serviceCentreAddress = p.source.layers.serviceCentreAddress;
                        if (serviceCentreAddress != null)
                        {
                            serviceCentreAddress[0] = ConvertToNumber(serviceCentreAddress[0]);
                        }

                        string[] msisdn = p.source.layers.msisdn == null? p.source.layers.msisdnOld : p.source.layers.msisdn;
                        if (msisdn != null)
                        {
                            msisdn[0] = ConvertToNumber(msisdn[0]);
                            msisdn[0] = msisdn[0].Substring(0, msisdn[0].Length - 1);

                        }
                        return p;
                    })
                    .Select(jo => jo.source.layers);


            var enumerable = layers.Select(l =>
                {
                    List<Layers> la = new List<Layers>();
                    if (l.LocalValue == null)
                    {
                        la.Add(l);
                        return la;
                    }
                    else
                    {
                        int noOfMap = l.LocalValue.Length;
                        if (noOfMap > 1)
                        {
                            la.AddRange(PacketAmplifier.Amplify(l, noOfMap));
                        }
                        else
                        {
                            la.Add(l);
                        }
                        return la;
                    }

                })
                .SelectMany(l => l)
                .Select(l =>
                    {
                        return new SigtranPacket
                        {
                            Frame = new Frame
                            {
                                FrameTime = l.FrameTime != null && l.FrameTime.Length > 0 ? l.FrameTime[0] : null,
                            },
                            M3Ua = new M3ua
                            {
                                ProtocolData = new ProtocolData
                                {
                                    Mtp3Equivalents = new Mtp3Equivalents
                                    {
                                        Opc = l.Opc != null && l.Opc.Length > 0 ? l.Opc[0] : null,
                                        Dpc = l.Dpc != null && l.Dpc.Length > 0 ? l.Dpc[0] : null,
                                    }
                                }
                            }, // Assuming M3Ua is always null
                            GsmMap = new GsmMap
                            {
                                OldComponent = l.OldComponent != null && l.OldComponent.Length > 0 ? l.OldComponent[0] : null,
                                ComponentTree = new ComponentTree
                                {
                                    InvokeElement = new InvokeElement
                                    {
                                        MsisdnTree = new MsisdnTree
                                        {
                                            Msisdn = l.msisdn != null && l.msisdn.Length > 0 ? l.msisdn[0] : null
                                        },
                                        ImsiTree = new ImsiTree
                                        {
                                            Imsi = l.Imsi != null && l.Imsi.Length > 0 ? l.Imsi[0] : null
                                        },
                                        OpCodeTree = new OpCodeTree
                                        {
                                            LocalValue = l.LocalValue != null && l.LocalValue.Length > 0 ? l.LocalValue[0] : null
                                        },
                                        ServiceCenterAddress = l.serviceCentreAddress != null && l.serviceCentreAddress.Length > 0 ? l.serviceCentreAddress[0] : null
                                    },
                                    ReturnResultLastElement = new ReturnResultLastElement
                                    {
                                        ResultretresElement = new ResultretresElement
                                        {
                                            Imsi = l.Imsi != null && l.Imsi.Length > 0 ? l.Imsi[0] : null
                                        }
                                    }
                                }
                            },
                            GsmSms = new GsmSms
                            {
                                TpUserData = new TpUserData
                                {
                                    SmsText = l.SmsText != null && l.SmsText.Length > 0 ? l.SmsText[0] : null
                                },
                                CallerNumber = new MsisdnTree
                                {
                                    Msisdn = l.CallerNumber != null && l.CallerNumber.Length > 0 ? l.CallerNumber[0] : null,
                                    CallerNumberMt = l.CallerNumberMt != null && l.CallerNumberMt.Length > 0 ? l.CallerNumberMt[0] : null,
                                },
                            },
                            Sccp = new Sccp
                            {
                                CalledPartyAddress = new PartyAddress
                                {
                                    GlobalTitle = new GlobalTitle
                                    {
                                        CalledDigits = l.CalledDigits != null && l.CalledDigits.Length > 0 ? l.CalledDigits[0] : null
                                    },
                                    InvokeId = l.InvokeId != null && l.InvokeId.Length > 0 ? l.InvokeId[0] : null
                                },
                                CallingPartyAddress = new PartyAddress
                                {
                                    GlobalTitle = new GlobalTitle
                                    {
                                        CallingDigits = l.CallingDigits != null && l.CallingDigits.Length > 0 ? l.CallingDigits[0] : null
                                    }
                                },
                                ReturnCause = l.ReturnCause != null && l.ReturnCause.Length > 0 ? l.ReturnCause[0] : null,
                                
                            },
                            Sctp = null, // Assuming Sctp is always null
                            Tcap = new Tcap
                            {
                                BeginElement = new Element
                                {
                                    Tid = l.Tid != null && l.Tid.Length > 0 ? l.Tid[0] : null
                                },
                                EndElement = new Element
                                {
                                    Tid = l.Dtid != null && l.Dtid.Length > 0 ? l.Dtid[0] : null
                                }
                            }
                        };
                    }
                );


            List<SigtranPacket> countLayers = new List<SigtranPacket>();
            countLayers.AddRange(enumerable.Select(l => l));
            stopwatch.Stop();

            // Get the elapsed time
            TimeSpan elapsedTime = stopwatch.Elapsed;

            // Output the elapsed time
            Console.WriteLine($"Elapsed Time: {elapsedTime}");
            return countLayers;
        }

        static string ReplaceCurlyBracesInGsmSmsText(string json)
        {
            //string pattern = "\"gsm_sms\\.sms_text\"\\s*:\\s*\"((?:[^\"\\\\]|\\\\.)*)\"";
            string pattern = "\"gsm_sms\\.sms_text\"\\s*:\\s*\\[(\\s*\"(?:[^\"\\\\]|\\\\.)*\"(?:\\s*,\\s*\"(?:[^\"\\\\]|\\\\.)*\")*\\s*)\\]";

            return Regex.Replace(json, pattern, match =>
            {
                string text = match.Groups[1].Value;

                text = text.Replace("{", "OPENING_CURLY_BRACES").Replace("}", "ENDING_CURLY_BRACES");
                return $"\"gsm_sms.sms_text\": [{text}]";
            });
        }

        
        static List<string> ExtractPackets(string json)
        {
            List<string> packets = new List<string>();
            int braceCount = 0;
            bool inPacket = false;
            int startIndex = 0;

            for (int i = 0; i < json.Length; i++)
            {
                if (json[i] == '{')
                {
                    if (!inPacket)
                    {
                        inPacket = true;
                        startIndex = i;
                    }
                    braceCount++;
                }
                else if (json[i] == '}')
                {
                    braceCount--;
                    if (braceCount == 0 && inPacket)
                    {
                        inPacket = false;
                        int endIndex = i;
                        packets.Add(json.Substring(startIndex, endIndex - startIndex + 1));
                    }
                }
            }
            return packets;
        }
        public static string ConvertToNumber(string input)
        {
            char[] output = new char[input.Length - 2]; // Create a char array to build the output

            // Process each pair of characters
            for (int i = 2; i < input.Length; i += 2)
            {
                output[i - 2] = input[i + 1]; // Reverse the pair of characters
                output[i - 2 + 1] = input[i];
            }

            return new string(output); // Return the constructed string
        }
    }
}
