using TelcobrightMediation;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using TelcobrightFileOperations;
using System.Globalization;
using LibraryExtensions;
using MediationModel;
using TelcobrightMediation.Mediation.Cdr;


namespace Decoders
{

    [Export("Decoder", typeof(IFileDecoder))]
    public class HuaweiSoftx3000Decoder : IFileDecoder
    {
        public override string ToString() => this.RuleName;
        public virtual string RuleName => GetType().Name;
        public virtual int Id => 3;
        public virtual string HelpText => "Decodes Huawei Softx3000 CDR.";
        public virtual CompressionType CompressionType { get; set; }
        public string PartialTablePrefix { get; }
        public string PartialTableStorageEngine { get; }
        public string partialTablePartitionColName { get; }

        public virtual List<string[]> DecodeFile(CdrCollectorInputData input,out List<cdrinconsistent> inconsistentCdrs)
        {
            inconsistentCdrs = new List<cdrinconsistent>();
            List<string[]> decodedRows = new List<string[]>();

            List<cdrfieldmappingbyswitchtype> lstFieldMapping = null;
            input.MefDecodersData.DicFieldMapping.TryGetValue(this.Id, out lstFieldMapping);
            int huaweiFixedBytePerRows = 907;
            byte[] a = FileUtil.GetBytesFromFile(input.FullPath);
            //check if file is multiple of byteperrows
            int byteRowCount = a.Count();
            int totalCdrCountInFile = byteRowCount / huaweiFixedBytePerRows;
            if (byteRowCount % huaweiFixedBytePerRows != 0)
            {
                throw new Exception("CDR file " + input.FullPath + " not consistent!");
            }
            int rowCnt = 0;
            string[] thisRow = null;
            for (; rowCnt < totalCdrCountInFile; rowCnt++)//for each row in byte array
            {
                try
                {
                    thisRow = null;
                    thisRow = new string[input.MefDecodersData.Totalfieldtelcobright];
                    foreach (cdrfieldmappingbyswitchtype thisField in lstFieldMapping) //for each field
                    {
                        if (thisField.BinByteOffset == null)//field is not present in cdr
                        {

                            thisRow[thisField.FieldNumber] = "";
                        }
                        else //field is present in CDR and get the string representation by huawei data type
                        {

                            //if (ThisField.FieldNumber == 80)
                            //{
                            //    int r = 1;
                            //}

                            //float to int conversion of offset value 
                            string thisOffSetStr = thisField.BinByteOffset.ToString();
                            int posDot = thisOffSetStr.IndexOf(".");
                            int thisOffSetInt = 0;
                            float bitOffSet = 0;
                            int startBitPosition = 0;
                            if (posDot > 0)
                            {
                                thisOffSetInt = int.Parse(thisOffSetStr.Substring(0, posDot));
                                bitOffSet = Single.Parse(thisOffSetStr.Substring(posDot));
                                //offset	bitposition
                                //0	          1
                                //0.125	      2
                                //0.25	      3
                                //0.375	      4
                                //0.5	        5
                                //0.625	      6
                                //0.75	      7
                                //0.875	      8

                                startBitPosition = Convert.ToInt32(Convert.ToSingle(8) * bitOffSet + 1);
                            }
                            else
                            {
                                thisOffSetInt = Convert.ToInt32(thisOffSetStr);
                            }
                            //float to int conversion of Length, will work for 0.125 types of values only
                            //has to add the logic to make 1.125 types of value
                            string thisLenStr = thisField.BinByteLen.ToString();
                            posDot = thisLenStr.IndexOf(".");
                            Single thisLenFloat = 0;
                            //read number of bit length from byte based on this calc...
                            //example byte position 171
                            //171.875		8	no bit of byte	
                            //171.75	=	7	no bit of byte	
                            //171.625	=	6	no bit of byte	
                            //171.5	=	5	no bit of byte	
                            //171.375	=	4	no bit of byte	
                            //171.25	=	3	no bit of byte	
                            //171.125	=	2	no bit of byte	
                            //171	=	1	no bit of byte	

                            byte bitLengthToRead = 0;

                            if (posDot > 0)
                            {
                                thisLenFloat = Single.Parse(thisLenStr.Substring(posDot));
                                bitLengthToRead = Convert.ToByte(8 * thisLenFloat);
                            }
                            else
                            {
                                thisLenFloat = Convert.ToInt32(thisLenStr);
                            }

                            byte[] fieldAsByte = null;
                            int intEquivalentBits = 0;
                            if (bitOffSet == 0)//regular byte array, no fractional bit
                            {
                                if (thisField.BinByteLen >= 1)
                                {
                                    fieldAsByte = new byte[Convert.ToInt32(thisField.BinByteLen)];
                                    Buffer.BlockCopy(a, Convert.ToInt32(thisOffSetInt + (rowCnt * huaweiFixedBytePerRows)), fieldAsByte, 0, Convert.ToInt32(thisField.BinByteLen));
                                }
                                else //read certain bits of length <1 e.g. .25 from the byte
                                {
                                    fieldAsByte = new byte[Convert.ToInt32(1)];
                                    Buffer.BlockCopy(a, Convert.ToInt32(thisOffSetInt + (rowCnt * huaweiFixedBytePerRows)), fieldAsByte, 0, 1);
                                    intEquivalentBits = HuaweiBinCdr.BitStringToIntFromByte(fieldAsByte[0], 0, bitLengthToRead);
                                }

                            }

                            else //fractional bit,read the byte as single byte first.
                            {
                                fieldAsByte = new byte[1];
                                Buffer.BlockCopy(a, Convert.ToInt32(thisOffSetInt + (rowCnt * huaweiFixedBytePerRows)), fieldAsByte, 0, 1);
                                intEquivalentBits = HuaweiBinCdr.BitStringToIntFromByte(fieldAsByte[0], startBitPosition, bitLengthToRead);
                                //use this value for the case "unsigned char" for field 18 and 22 below in the switch
                            }

                            string strThisField = "";
                            switch (thisField.BinByteType)
                            {
                                case "unsigned short":
                                    //strThisField = IntEquivalentBits.ToString().ToString();
                                    UInt32 x = HuaweiBinCdr.ByteArrayToUInt16BigIndian(fieldAsByte);
                                    if (thisField.FieldNumber == 24)//releasecauseegress=ip cause code
                                    {
                                        int tempInt = -1;
                                        int.TryParse(strThisField, out tempInt);
                                        //strThisField = (Convert.ToInt32(strThisField) - 2000).ToString();
                                        if (tempInt > -1) strThisField = tempInt.ToString();
                                    }
                                    else
                                    {
                                        strThisField = HuaweiBinCdr.ByteArrayToUInt16(fieldAsByte).ToString();
                                    }
                                    break;
                                case "unsigned long":
                                    x = HuaweiBinCdr.ByteArrayToUInt32(fieldAsByte);
                                    strThisField = x.ToString();
                                    if (thisField.FieldNumber == 14)//duration, which is in 10 mili seconds unit in huawei cdr
                                    {
                                        decimal durationInSec = decimal.Divide(Convert.ToDecimal(x), 100);
                                        strThisField = durationInSec.ToString();
                                    }

                                    if ((thisField.FieldNumber == 7) || (thisField.FieldNumber == 27))//if OPC, DPC is missing, show all FF in  hex
                                    {
                                        if (x > 32000)
                                        {
                                            strThisField = "\\N";
                                        }
                                    }


                                    break;
                                case "unsigned char":
                                    //if a date time field...
                                    //StartTime	  	29
                                    //EndTime	    	15
                                    //ConnectTime		16
                                    //AnswerTime		17
                                    if ((thisField.FieldNumber == 29) ||
                                       (thisField.FieldNumber == 15) ||
                                       (thisField.FieldNumber == 16) ||
                                       (thisField.FieldNumber == 17))
                                    {
                                        strThisField = HuaweiBinCdr.ByteArrayToDateTime(fieldAsByte).ToString();
                                    }

                                    if ((thisField.FieldNumber == 18)) //answer flag and Release Direction
                                    {
                                        //if duration>0 set answerflag, regardless of whether it is 1 by field18's original value
                                        double tempDuration = 0;
                                        double.TryParse(thisRow[Fn.DurationSec], out tempDuration);
                                        if (tempDuration > 0)
                                        {
                                            strThisField = "1";
                                        }
                                        else
                                        {
                                            int tempAnswer = intEquivalentBits;
                                            if (tempAnswer == 0)//in huawei softx, answer=0 no answer=1
                                            {
                                                strThisField = "1";
                                            }
                                            else if (tempAnswer == 1)
                                            {
                                                strThisField = "0";
                                            }
                                        }

                                    }
                                    else if ((thisField.FieldNumber == 22) || (thisField.FieldNumber == 79)) //release direction, connected number type
                                    {
                                        strThisField = intEquivalentBits.ToString();
                                    }

                                    else if ((thisField.FieldNumber == 23) || (thisField.FieldNumber == 59) || (thisField.FieldNumber == 60))
                                    { //releasecausesytem, calledpartynoa,callingpartynoa
                                        strThisField = fieldAsByte[0].ToString();
                                    }
                                    else if ((thisField.FieldNumber == 56))
                                    { //releasecauseingress
                                        strThisField = fieldAsByte[0].ToString();
                                    }
                                    break;

                                case "bcd":
                                    //if a phone number type of field...    
                                    //OriginatingCalledNumber	 9   
                                    //TerminatingCalledNumber	 10  
                                    //OriginatingCallingNumber 11  
                                    //TerminatingCallingNumber 12  

                                    /*if (
                                    (thisField.FieldNumber == 9) ||
                                    (thisField.FieldNumber == 10) ||
                                    (thisField.FieldNumber == 11) ||
                                    (thisField.FieldNumber == 12) ||
                                    (thisField.FieldNumber == 80))
                                    {
                                        strThisField = HuaweiBinCdr.BcdToDigitString(fieldAsByte).ToString();
                                    }*/

                                    if (thisField.FieldNumber == 9)
                                    {
                                        strThisField = HuaweiBinCdr.BcdToDigitString(fieldAsByte).ToString();
                                    }

                                    if (thisField.FieldNumber == 10)
                                    {
                                        strThisField = HuaweiBinCdr.BcdToDigitString(fieldAsByte).ToString();
                                    }

                                    if (thisField.FieldNumber == 11)
                                    {
                                        strThisField = HuaweiBinCdr.BcdToDigitString(fieldAsByte).ToString();
                                    }

                                    if (thisField.FieldNumber == 12)
                                    {
                                        strThisField = HuaweiBinCdr.BcdToDigitString(fieldAsByte).ToString();
                                    }

                                    if (thisField.FieldNumber == 80)
                                    {
                                        strThisField = HuaweiBinCdr.BcdToDigitString(fieldAsByte).ToString();
                                    }

                                    break;
                                case "ip address":
                                    string ip = HuaweiBinCdr.ByteArrayToIpAddressReversedOctets(fieldAsByte);
                                    strThisField = ip;
                                    break;
                            }//switch
                            thisRow[thisField.FieldNumber] = strThisField;
                        }//if field present in CDR
                        
                    } //for each field
                      //add valid flag for this type of switch, for zte, valid flag comes from the switch
                    thisRow[54] = "1";
                    thisRow[55] = "0";//for now mark as non-partial, single cdr
                    //found invalid starttime for failed calls, put connecttime's value in starttime
                    //also found answertime for failed calls, set them to null
                    if (thisRow[Fn.ChargingStatus] != "1")
                    {
                        DateTime tempDate = new DateTime();
                        //if not valid start time
                        if (!DateTime.TryParseExact(thisRow[Fn.StartTime], "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out tempDate))
                        {
                            //put connect time into starttime
                            if (DateTime.TryParseExact(thisRow[Fn.ConnectTime], "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out tempDate))
                            {
                                thisRow[Fn.StartTime] = thisRow[Fn.ConnectTime];
                            }//else try answertime
                            else if (DateTime.TryParseExact(thisRow[Fn.AnswerTime], "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out tempDate))
                            {
                                thisRow[Fn.StartTime] = thisRow[Fn.AnswerTime];
                            }
                            else if (DateTime.TryParseExact(thisRow[Fn.Endtime], "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out tempDate))
                            {
                                thisRow[Fn.StartTime] = thisRow[Fn.Endtime];
                            }
                        }
                        thisRow[Fn.AnswerTime] = "";//set answertime to null
                    }
                    thisRow[Fn.FinalRecord] = "1";
                    thisRow[Fn.Validflag] = "1";
                    decodedRows.Add(thisRow);
                }
                catch (Exception e1)
                {//if error found for one row, add this to inconsistent

                    Console.WriteLine(e1);
                    inconsistentCdrs.Add(CdrConversionUtil.ConvertTxtRowToCdrinconsistent(thisRow));
                    ErrorWriter wr = new ErrorWriter(e1, "DecodeCdr", null,
                        this.RuleName + " encounterd error during decoding and an Inconsistent cdr has been generated."
                        , input.Tbc.Telcobrightpartner.CustomerName);
                    continue;//with next row
                }
            }//try for each row in byte array
            return decodedRows;
        }

        public string getTupleExpression(CdrCollectorInputData decoderInputData, string[] row)
        {
            throw new NotImplementedException();
        }

        public string getSqlWhereClauseForHourWiseSafeCollection(CdrCollectorInputData decoderInputData,DateTime hourOfDay, int minusHoursForSafeCollection, int plusHoursForSafeCollection)
        {
            throw new NotImplementedException();
        }

        public string getCreateTableSqlForUniqueEvent(CdrCollectorInputData decoderInputData)
        {
            throw new NotImplementedException();
        }

        public string getDuplicateCollectionSql(CdrCollectorInputData decoderInputData, DateTime hourOfTheDay, List<string> tuples)
        {
            throw new NotImplementedException();
        }

        public string getPartialCollectionSql(CdrCollectorInputData decoderInputData, DateTime hourOfTheDay, List<string> tuples)
        {
            throw new NotImplementedException();
        }
    }
}
