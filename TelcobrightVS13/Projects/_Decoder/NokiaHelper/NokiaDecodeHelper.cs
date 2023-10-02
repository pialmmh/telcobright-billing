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
using LibraryExtensions;

namespace Decoders
{

    
    public class NokiaDecodeHelper
    {


        public static List<string> FieldList = new List<string>()
        {
            "record_length",
            "record_type",
            "record_number",
            "record_status",
            "check_sum",
            "call_reference",
            "exchange_id",
            "intermediate_record_number",
            "intermediate_charging_ind",
            "number_of_ss_records",
            "calling_number_ton",
            "calling_number",
            "called_number_ton",
            "called_number",
            "out_circuit_group",
            "out_circuit",
            "in_channel_allocated_time",
            "charging_start_time",
            "charging_end_time",
            "cause_for_termination",
            "call_type",
            "ticket_type",
            "oaz_chrg_type",
            "oaz_duration",
            "oaz_tariff_class",
            "oaz_pulses",
            "called_msrn_ton",
            "called_msrn",
            "intermediate_chrg_cause",
            // "leg_call_reference",
            // "out_channel_allocated_time",
            "basic_service_type",
            "basic_service_code",
            // "call_reference_time",
            "number_of_all_in_records",
            "char_band_change_direction",
            "char_band_change_percent",
            "number_of_in_records",
            "oaz_change_direction",
            "oaz_change_percent",
            "scp_connection",
            "term_mcz_change_direction",
            "term_mcz_change_percent",
            "cug_information",
            "cug_interlock",
            "cug_outgoing_access",
            "selected_codec",
            "out_bnc_connection_type",
            "outside_control_plane_index",
            "outside_user_plane_index",
            "in_circuit_group_name",
            "out_circuit_group_name",
            "in_circuit",
            "in_circuit_group",
            "oaz_duration_ten_ms"
        };

        private static Dictionary<string, FieldInfo> PocFieldsDetails = new Dictionary<string, FieldInfo>()
        {
               { "record_length", new FieldInfo(0, 2,DataType.HexWord )},
               { "record_type", new FieldInfo(2, 1,DataType.HexByteRev) },
               { "record_number", new FieldInfo(3, 4,DataType.HexByteRev)},
               { "record_status", new FieldInfo(7, 1, DataType.HexByte) },
               { "check_sum", new FieldInfo(8, 2, DataType.HexWord) },
               { "call_reference", new FieldInfo(10, 5,DataType.WordWordByte ) },
               { "exchange_id", new FieldInfo(15, 10,DataType.BcdByte) },
               { "intermediate_record_number", new FieldInfo(25, 1,DataType.BcdByte) },
               { "intermediate_charging_ind", new FieldInfo(26, 1, DataType.HexByte) },
               { "number_of_ss_records", new FieldInfo(27, 1, DataType.BcdByte) },
               { "calling_number_ton", new FieldInfo(28, 1, DataType.HexByte) },
               { "calling_number", new FieldInfo(29, 12, DataType.BcdByte) },
               { "called_number_ton", new FieldInfo(41, 1, DataType.HexByte) },
               { "called_number", new FieldInfo(42, 12, DataType.BcdByte) },
               { "out_circuit_group", new FieldInfo(52, 2, DataType.BcdWord) },
               { "out_circuit", new FieldInfo(54, 2, DataType.BcdWord) },
               { "in_channel_allocated_time", new FieldInfo(58, 7, DataType.TimeFieldDate) },
               { "charging_start_time", new FieldInfo(65, 7, DataType.TimeFieldDate) },
               { "charging_end_time", new FieldInfo(72, 7, DataType.TimeFieldDate) },
               { "cause_for_termination", new FieldInfo(79, 4, DataType.HexDword) },
               { "call_type", new FieldInfo(83, 1, DataType.HexByte) },
               { "ticket_type", new FieldInfo(84, 1,DataType.HexByte) }, //Type of CDR POC/PTC 
               { "oaz_chrg_type", new FieldInfo(85, 1,DataType.HexByte) },
               { "oaz_duration", new FieldInfo(86, 3,DataType.BcdByteRev) },
               { "oaz_tariff_class", new FieldInfo(89, 3,DataType.HexWordHexbyte) },
               { "oaz_pulses", new FieldInfo(92, 2,DataType.BcdWord) },
               { "called_msrn_ton", new FieldInfo(94, 1,DataType.HexByte) },
               { "called_msrn", new FieldInfo(95, 12, DataType.HexByte) },
               { "intermediate_chrg_cause", new FieldInfo(107, 4, DataType.HexDword) },
               { "leg_call_reference", new FieldInfo(111, 1,DataType.HexByte) },
               { "out_channel_allocated_time", new FieldInfo(112, 5,DataType.HexByte) },//Change Type
               { "basic_service_type", new FieldInfo(118, 1,DataType.HexByte) },
               { "basic_service_code", new FieldInfo(119, 1,DataType.HexByte) },
               { "call_reference_time", new FieldInfo(120, 7,DataType.TimeFieldDate) },
               { "number_of_all_in_records", new FieldInfo(127, 8,DataType.HexByte) },
               { "char_band_change_direction", new FieldInfo(135, 9, DataType.HexByte) },
               { "char_band_change_percent", new FieldInfo(144, 1, DataType.HexByte) },
               { "number_of_in_records", new FieldInfo(145, 1,DataType.BcdByte) },
               { "oaz_change_direction", new FieldInfo(146, 1,DataType.HexByte) },
               { "oaz_change_percent", new FieldInfo(147, 1,DataType.HexByte) },
               { "scp_connection", new FieldInfo(148, 1, DataType.HexByte) },
               { "term_mcz_change_direction", new FieldInfo(149, 1,DataType.HexByte) },
               { "term_mcz_change_percent", new FieldInfo(150, 1, DataType.HexByte) },
               { "cug_information", new FieldInfo(151, 1, DataType.HexByte) },
               { "cug_interlock", new FieldInfo(152, 1,DataType.HexByte) },
               { "cug_outgoing_access", new FieldInfo(153, 1, DataType.HexByte) },
               { "selected_codec", new FieldInfo(154, 1, DataType.HexByte) },
               { "out_bnc_connection_type", new FieldInfo(155, 4, DataType.HexByte) },
               { "outside_control_plane_index", new FieldInfo(159, 1,DataType.HexByte) },
               { "outside_user_plane_index", new FieldInfo(160, 1,DataType.HexByte) },
               { "in_circuit_group_name", new FieldInfo(161, 1,DataType.HexByte) },
               { "out_circuit_group_name", new FieldInfo(162, 2,DataType.HexByte) },
               { "in_circuit", new FieldInfo(164, 2,DataType.HexByte) },
               { "in_circuit_group", new FieldInfo(166, 6,DataType.HexByte) },
               { "oaz_duration_ten_ms", new FieldInfo(172, 8,DataType.HexByte) }

          };
        private static Dictionary<string, FieldInfo> PtcFieldsDetails = new Dictionary<string, FieldInfo>()
        {
               { "record_length", new FieldInfo(0, 2,DataType.HexWord )},
               { "record_type", new FieldInfo(2, 1,DataType.HexByteRev) },
               { "record_number", new FieldInfo(3, 4,DataType.HexByteRev)},
               { "record_status", new FieldInfo(7, 1, DataType.HexByte) },
               { "check_sum", new FieldInfo(8, 2, DataType.HexWord) },
               { "call_reference", new FieldInfo(10, 5,DataType.WordWordByte ) },
               { "exchange_id", new FieldInfo(15, 10,DataType.BcdByte) },
               { "intermediate_record_number", new FieldInfo(25, 1,DataType.BcdByte) },
               { "intermediate_charging_ind", new FieldInfo(26, 1, DataType.HexByte) },
               { "number_of_ss_records", new FieldInfo(27, 1, DataType.BcdByte) },
               { "calling_number_ton", new FieldInfo(28, 1, DataType.HexByte) },
               { "calling_number", new FieldInfo(29, 12, DataType.BcdByte) },
               { "called_number_ton", new FieldInfo(41, 1, DataType.HexByte) },
               { "called_number", new FieldInfo(42, 12, DataType.BcdByte) },
               { "out_circuit_group", new FieldInfo(54, 2, DataType.BcdWord) },
               { "out_circuit", new FieldInfo(56, 2, DataType.BcdWord) },
               { "in_channel_allocated_time", new FieldInfo(58, 7, DataType.TimeFieldDate) },
               { "charging_start_time", new FieldInfo(65, 7, DataType.TimeFieldDate) },
               { "charging_end_time", new FieldInfo(72, 7, DataType.TimeFieldDate) },
               { "cause_for_termination", new FieldInfo(79, 4, DataType.HexDword) },
               { "call_type", new FieldInfo(83, 1, DataType.HexByte) },
               { "ticket_type", new FieldInfo(84, 1,DataType.HexByte) }, //Type of CDR POC/PTC 
               { "oaz_chrg_type", new FieldInfo(85, 1,DataType.HexByte) },
               { "oaz_duration", new FieldInfo(86, 3,DataType.BcdByteRev) },
               { "oaz_tariff_class", new FieldInfo(89, 3,DataType.HexWordHexbyte) },
               { "oaz_pulses", new FieldInfo(92, 2,DataType.BcdWord) },
               { "called_msrn_ton", new FieldInfo(94, 1,DataType.HexByte) },
               { "called_msrn", new FieldInfo(95, 12, DataType.HexByte) },
               { "intermediate_chrg_cause", new FieldInfo(107, 4, DataType.HexDword) },
               { "leg_call_reference", new FieldInfo(111, 5,DataType.WordWordByte) }, //poc
               { "out_channel_allocated_time", new FieldInfo(116, 7,DataType.TimeFieldDate) },//Change Type
               { "basic_service_type", new FieldInfo(123, 1,DataType.HexByte) },
               { "basic_service_code", new FieldInfo(124, 1,DataType.HexByte) },
               { "call_reference_time", new FieldInfo(125, 7,DataType.TimeFieldDate) },
               { "number_of_all_in_records", new FieldInfo(132, 1,DataType.HexByte) },
               { "char_band_change_direction", new FieldInfo(133, 1, DataType.HexByte) },
               { "char_band_change_percent", new FieldInfo(134, 1, DataType.HexByte) },
               { "number_of_in_records", new FieldInfo(135, 1,DataType.BcdByte) },
               { "oaz_change_direction", new FieldInfo(136, 1,DataType.HexByte) },
               { "oaz_change_percent", new FieldInfo(137, 1,DataType.HexByte) },
               { "scp_connection", new FieldInfo(138, 1, DataType.HexByte) },
               { "term_mcz_change_direction", new FieldInfo(139, 1,DataType.HexByte) },
               { "term_mcz_change_percent", new FieldInfo(140, 1, DataType.HexByte) },
               { "cug_information", new FieldInfo(141, 1, DataType.HexByte) },
              { "cug_interlock", new FieldInfo(142, 4,DataType.HexByte) },
              { "cug_outgoing_access", new FieldInfo(146, 1, DataType.HexByte) },
              { "selected_codec", new FieldInfo(147, 1, DataType.HexByte) },
              { "out_bnc_connection_type", new FieldInfo(148, 1, DataType.HexByte) },
              { "outside_control_plane_index", new FieldInfo(149, 2,DataType.HexByte) },
              { "outside_user_plane_index", new FieldInfo(151, 2,DataType.HexByte) },
              { "in_circuit_group_name", new FieldInfo(153, 8,DataType.HexByte) },
              { "out_circuit_group_name", new FieldInfo(161, 8,DataType.HexByte) },
              { "in_circuit", new FieldInfo(169, 2,DataType.BcdWord) },
              { "in_circuit_group", new FieldInfo(171, 2,DataType.BcdWord) },
              { "oaz_duration_ten_ms", new FieldInfo(173, 4,DataType.BcdByteRev) }

  };
        public static string[] Decode(int currentPosition, List<byte> fileData, CdrType cdrType)
        {
            List<string> records = new List<string>();
            int totalSkip = 0, totalLengthNeedToIncrease = 0;
            bool skiped = false;
            List<string> allFieldList = FieldList;

            allFieldList.ForEach(f =>
            {
                int offset = 0, length = 0, tempOfset = 0;
                DataType dataType = DataType.Unknown;
                switch (cdrType)
                {
                    case CdrType.Ptc:
                        {
                            length = PtcFieldsDetails[f].Length;
                            offset = PtcFieldsDetails[f].Offset + currentPosition;
                            tempOfset = PtcFieldsDetails[f].Offset + currentPosition;
                            dataType = (DataType)PtcFieldsDetails[f].DataType;
                            if (f == "exchange_id")
                            {
                                totalSkip += TotalSkip(fileData, tempOfset);
                            }

                        }
                        break;
                    case CdrType.Poc:
                        {
                            length = PocFieldsDetails[f].Length;
                            offset = PocFieldsDetails[f].Offset + currentPosition;
                            tempOfset = offset;

                            dataType = PocFieldsDetails[f].DataType;
                            if (f == "exchange_id")
                            {
                                totalSkip += TotalSkip(fileData, tempOfset);
                            }
                        }
                        break;

                    default:
                        {
                            Console.WriteLine("----Unknown Block-----");
                        };
                        break;
                }

                List<byte> recordBytes;
                if (f == "in_circuit_group" && cdrType == CdrType.Ptc)
                {
                    recordBytes = fileData.GetRange(currentPosition + (177 - 6), length);
                }
                else if (f == "oaz_duration_ten_ms" && cdrType == CdrType.Ptc)
                {
                    recordBytes = fileData.GetRange(currentPosition + (177 - 4), length);
                }
                else
                {
                    recordBytes = fileData.GetRange(offset + (totalSkip), length);

                }

                

                string recordData = GetRecordData(dataType, recordBytes);
                records.Add(recordData);
                if (f == "calling_number")
                {
                    int tmp = offset + totalSkip + length;
                    while (fileData[tmp] == 255)
                    {
                        totalSkip++;
                        tmp++;
                    }
                }

            });

            return records.ToArray();
        }

        private static string GetRecordData(DataType dataType, List<byte> recordBytes)
        {
            string record = "";
            switch (dataType)
            {
                case DataType.HexByte:
                    {
                        List<char> rs = new List<char>();

                        recordBytes.ForEach(b =>
                        {
                            foreach (var c in new HexByte(b).ToString()) rs.Add(c);
                        });

                        record = string.Join("", rs);
                    }
                    break;
                case DataType.HexByteRev:
                    {
                        List<char> rs = new List<char>();
                        recordBytes.ForEach(b =>
                        {
                            foreach (var c in new HexByte(b).ToString().Reverse()) rs.Add(c);
                        });
                        record = string.Join("", rs.ToArray().Reverse());
                    }
                    break;

                case DataType.HexWordHexbyte:
                    {
                        HexWord hexWord = new HexWord(new HexByte[] { new HexByte(recordBytes[0]), new HexByte(recordBytes[1]) });
                        HexByte hexByte = new HexByte(recordBytes[2]);
                        record = string.Join("", hexByte.ToString());
                    }
                    break;

                case DataType.HexWord:
                    {
                        HexWord hexWord = new HexWord(new HexByte[]
                        {
                        new HexByte(recordBytes[0]),
                        new HexByte(recordBytes[1]),
                        });
                        record = string.Join("", hexWord.getInt16().ToString());
                    }
                    break;
                case DataType.HexDword:
                    {
                        HexWord hexWord1 = new HexWord(new HexByte[]
                        {
                        new HexByte(recordBytes[0]),
                        new HexByte(recordBytes[1]),
                        });
                        HexWord hexWord2 = new HexWord(new HexByte[]
                        {
                        new HexByte(recordBytes[2]),
                        new HexByte(recordBytes[3]),
                        });
                        HexDWord hexDWord = new HexDWord(new HexWord[]
                        {
                        hexWord2,
                        hexWord1
                        });
                        record = string.Join("", hexDWord.getHexDwordStrReversed());
                    }
                    break;
                case DataType.BcdWord:
                    {
                        HexWord hexWord = new HexWord(new HexByte[]
                        {
                        new HexByte(recordBytes[0]),
                        new HexByte(recordBytes[1]),
                        });
                        record = string.Join("", hexWord.getHexStrReversed());
                    }
                    break;

                case DataType.TimeFieldDate:
                    {
                        List<byte> year = new List<byte>(recordBytes.Skip(5).Take(2));

                        TimeFieldData td = new TimeFieldData(new List<byte>(recordBytes.Take(5)), year);
                        record = string.Join("", td.dt.ToString());
                    }
                    break;

                case DataType.WordWordByte:
                    {
                        Word<HexByte> word1 = new Word<HexByte>(
                            new HexByte[]
                            {
                            new HexByte(recordBytes[0]),
                            new HexByte(recordBytes[1]),
                            }
                        );
                        Word<HexByte> word2 = new Word<HexByte>(
                            new HexByte[]
                            {
                            new HexByte(recordBytes[2]),
                            new HexByte(recordBytes[3]),
                            }
                        );
                        record = string.Join("",
                            word1.getHexStrReversed() + word2.getHexStrReversed() + new HexByte(recordBytes[4]));
                    }
                    break;

                case DataType.BcdByte:
                    {
                        List<char> rs = new List<char>();
                        recordBytes.ForEach(b =>
                        {
                            foreach (var c in new BcdBytes(b).ToString().Reverse()) rs.Add(c);
                        });
                        record = string.Join("", rs);
                    }
                    break;
                case DataType.BcdByteRev:
                    {
                        Stack<char> myStack = new Stack<char>();
                        List<char> rs = new List<char>();
                        recordBytes.ForEach(b =>
                        {
                            var s = b.ToString().Reverse();
                            foreach (var c in new BcdBytes(b).ToString().Reverse())
                            {
                                rs.Add(c);
                                myStack.Push(c);
                            }

                        });
                        record = string.Join("", myStack);
                    }
                    break;


                default: break;
            }

            return record;
        }

        private static int TotalSkip(List<byte> fileData, int tempOfset)
        {
            int totalSkip = 0;
            while (fileData[tempOfset] != 136)
            {
                totalSkip += 1;
                tempOfset += 1;
                if (fileData[tempOfset] == 136)
                {
                    break;
                }
            }
            while (fileData[tempOfset + 1] == 136)
            {
                totalSkip += 1;
                tempOfset += 1;
            }

            return totalSkip;
        }
    }


    public class Word<T>
    {
        public T[] Value { get; set; } = new T[2];
        public Word(T[] values)
        {
            if (values.Length != 2) throw new Exception("Array Length must be 2");
            this.Value[0] = values[0];
            this.Value[1] = values[1];
        }
        public override string ToString()
        {
            return string.Join("", this.Value.Select(v => v.ToString()));
        }

        public int getInt16()
        {
            return Convert.ToInt16(getHexStrReversed(), 16);
        }

        public int getInt32()
        {
            return Convert.ToInt32(getHexStrReversed(), 32);
        }
        public long getInt64()
        {
            return Convert.ToInt64(getHexStrReversed(), 8);
        }
        public Decimal getDecimal()
        {
            int decimalValue = Convert.ToInt32(Convert.ToInt16(getHexDwordStrReversed()).ToString(), 32);
            return decimalValue;
        }
        public string getReverseHexString()
        {
            string decimalValue = Convert.ToInt16(getHexDwordStrReversed()).ToString();
            return decimalValue;
        }

        public string getHexStrReversed()
        {
            var reversed = this.Value.Reverse();
            string hexStr = string.Join("", reversed.Select(hexByte => hexByte.ToString()));
            return hexStr;
        }

        public string reverseSubstrings(string input)
        {
            int index = 2;
            string firstSubstring = input.Substring(0, index);
            string secondSubstring = input.Substring(index);
            return secondSubstring + firstSubstring;
        }
        public string getHexDwordStrReversed()
        {
            var reversedFirstHexWord = reverseSubstrings(this.Value[0].ToString());
            var reversedSecondHexWord = reverseSubstrings(this.Value[1].ToString());
            return reversedFirstHexWord + reversedSecondHexWord;
        }

    }

    public class HexWord : Word<HexByte>
    {
        public Word<HexByte> Value { get; set; }
        public HexWord(HexByte[] value) : base(value) { }
    }
    public class HexDWord : Word<HexWord>
    {
        public Word<HexWord> Value { get; set; }
        public HexDWord(HexWord[] value) : base(value) { }

    }

    public class BcdDWord : Word<HexWord>
    {
        public Word<HexWord> Value { get; set; }
        public BcdDWord(HexWord[] value) : base(value) { }
    }

    public class HexByte
    {
        public byte Value { get; set; }
        private readonly byte[] _valueAsBytes = new byte[1];
        public HexByte(byte value)
        {
            this.Value = value;
            this._valueAsBytes[0] = this.Value;
        }


        public override string ToString()
        {
            return BitConverter.ToString(this._valueAsBytes).Replace("-", "");
        }

    }

    public class BcdBytes
    {
        public byte Value { get; set; }
        private readonly byte[] _valueAsBytes = new byte[1];
        public BcdBytes(byte value)
        {
            this.Value = value;
            this._valueAsBytes[0] = this.Value;
        }

        public override string ToString()
        {
            return BitConverter.ToString(this._valueAsBytes).Replace("-", "");
        }
    }
    public class TimeFieldData
    {
        private BcdBytes[] TimeSpan { get; set; } = new BcdBytes[5];
        public string dt { get; set; }
        private HexWord Year { get; set; }

        public TimeFieldData(List<byte> timeSpan, List<byte> year)
        {
            try
            {
                string yearString = new HexByte(year[1]).ToString() + new HexByte(year[0]).ToString();

                string timestamp = new HexByte(timeSpan[4]).ToString()+ new HexByte(timeSpan[3]).ToString()+yearString+ 
                                   new HexByte(timeSpan[2]).ToString()+ new HexByte(timeSpan[1]).ToString()+ new HexByte(timeSpan[0]).ToString();
                this.dt = timestamp;
            }
            catch (Exception e)
            {
                // Console.WriteLine(e);
                // throw;
            }


        }
    }
    public enum DataType
    {
        TimeFieldDate,
        HexByte,
        HexByteRev,
        HexWord,
        HexDword,
        BcdByte,
        BcdByteRev,
        BcdWord,
        BcdDword,
        WordWordByte,
        BcdBcdHex,
        BcdBcdWord,
        HexWordHexbyte,
        Unknown
    }
}
