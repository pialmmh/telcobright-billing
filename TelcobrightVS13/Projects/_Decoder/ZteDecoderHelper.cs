using TelcobrightMediation;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using LibraryExtensions;
using TelcobrightFileOperations;
using MediationModel;
using TelcobrightMediation.Mediation.Cdr;


namespace Decoders
{
    public static class ZteDecoderHelper
    {
        public static List<string[]> DecodeFile(int idCdrFormat, CdrCollectorInputData input,out List<cdrinconsistent> inconsistentCdrs)
        {
            inconsistentCdrs = new List<cdrinconsistent>();
            List<string[]> decodedRows = new List<string[]>();

            List<cdrfieldmappingbyswitchtype> fieldMappings = null;
            input.MefDecodersData.DicFieldMapping.TryGetValue(idCdrFormat, out fieldMappings);
            string zteStartofTimeStr = "2000-01-01 00:00:00"; // <-- Valid
            string format = "yyyy-MM-dd HH:mm:ss";
            DateTime zteStartofTime;
            DateTime.TryParseExact(zteStartofTimeStr, format, CultureInfo.InvariantCulture,
            DateTimeStyles.None, out zteStartofTime);
            int zteFixedBytePerRows = 559;
            byte[] a = FileUtil.GetBytesFromFile(input.FullPath);
            //check if file is multiple of byteperrows
            int byteRowCount = a.Count();
            int totalCdrCountInFile = byteRowCount / zteFixedBytePerRows;
            if (byteRowCount % zteFixedBytePerRows != 0)
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
                    DateTime zteAnswerTime = DateTime.Now;
                    DateTime zteEndTime = DateTime.Now;
                    foreach (cdrfieldmappingbyswitchtype fldMapping in fieldMappings) //for each field
                    {
                        if (fldMapping.BinByteOffset == null) //field is not present in cdr
                        {
                            thisRow[fldMapping.FieldNumber] = "";
                        }
                        else //field is present in CDR and get the string representation by huawei data type
                        {
                            //float to int conversion of offset value 
                            string thisOffSetStr = fldMapping.BinByteOffset.ToString();
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
                            string thisLenStr = fldMapping.BinByteLen.ToString();
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
                            if (bitOffSet == 0) //regular byte array, no fractional bit
                            {
                                if (fldMapping.BinByteLen >= 1)
                                {
                                    fieldAsByte = new byte[Convert.ToInt32(fldMapping.BinByteLen)];
                                    Buffer.BlockCopy(a, Convert.ToInt32(thisOffSetInt + (rowCnt * zteFixedBytePerRows)),
                                        fieldAsByte, 0, Convert.ToInt32(fldMapping.BinByteLen));
                                }
                                else //read certain bits of length <1 e.g. .25 from the byte
                                {
                                    fieldAsByte = new byte[Convert.ToInt32(1)];
                                    Buffer.BlockCopy(a, Convert.ToInt32(thisOffSetInt + (rowCnt * zteFixedBytePerRows)),
                                        fieldAsByte, 0, 1);
                                    intEquivalentBits =
                                        BinCdr.BitStringToIntFromByte(fieldAsByte[0], 0, bitLengthToRead);
                                }

                            }

                            else //fractional bit,read the byte as single byte first.
                            {
                                fieldAsByte = new byte[1];
                                Buffer.BlockCopy(a, Convert.ToInt32(thisOffSetInt + (rowCnt * zteFixedBytePerRows)),
                                    fieldAsByte, 0, 1);
                                intEquivalentBits =
                                    BinCdr.BitStringToIntFromByte(fieldAsByte[0], startBitPosition, bitLengthToRead);
                                //use this value for the case "unsigned char" for field 18 and 22 below in the switch
                            }

                            string strThisField = "";
                            switch (fldMapping.BinByteType)
                            {
                                case "unsigned short":

                                    strThisField = BinCdr.ByteArrayToUInt16BigIndian(fieldAsByte).ToString();

                                    if (fldMapping.FieldNumber == 74) //OutTrkGroupNo
                                    {
                                        //set TG for international incoming 
                                        if ((thisRow[73] == "0") || (thisRow[33] == "1")) //InTrkGrpno=0 and OutMg=1
                                        {

                                            thisRow[5] = thisRow[31]; //set incoming route
                                            thisRow[25] = strThisField; //set outgoing route
                                            //field ThisRow[74] is not assigned yet...

                                        }
                                        else if ((thisRow[74] == "0") || thisRow[31] == "1") //OutTrkGrpno=0 and InMg=1
                                        {
                                            thisRow[5] = thisRow[73]; //set incoming route
                                            thisRow[25] = thisRow[33]; //set outgoing route

                                        }
                                    }

                                    break;
                                case "unsigned long":
                                    UInt32 x = BinCdr.ByteArrayToUInt32BigEndian(fieldAsByte);
                                    strThisField = x.ToString();


                                    break;
                                case "unsigned char":


                                    if (fldMapping.FieldNumber == 55) // cdr type, interim, final etc.
                                    {
                                        //0：one record
                                        //1：first part
                                        //2：interim part
                                        //3：final part
                                        UInt32 x1 = BinCdr.ByteArraySingleToInt(fieldAsByte);
                                        strThisField = x1.ToString();
                                    }

                                    //for answerid field    
                                    //0：not answer
                                    //1：answer

                                    //for valid id field
                                    //0=valid
                                    //1=invalid
                                    else if ((fldMapping.FieldNumber == 18)) //charging status
                                    {
                                        strThisField = intEquivalentBits.ToString();
                                    }
                                    else if ((fldMapping.FieldNumber == 54)) ////valid flag, 

                                    {
                                        strThisField = intEquivalentBits==0?"1":"0" ; //0 = valid for zte, so invert
                                    }

                                    else if ((fldMapping.FieldNumber == 22)) //release direction
                                    {
                                        strThisField = intEquivalentBits.ToString();
                                    }

                                    else
                                    {
                                        //for all other fields
                                        UInt32 x2 = BinCdr.ByteArraySingleToInt(fieldAsByte);
                                        strThisField = x2.ToString();
                                    }

                                    break;

                                case "bcd":
                                    //if a phone number type of field...    
                                    //OriginatingCalledNumber	 9   
                                    //TerminatingCalledNumber	 10  
                                    //OriginatingCallingNumber 11  
                                    //TerminatingCallingNumber 12  

                                    if (
                                            (fldMapping.FieldNumber == 80) ||//redirectingnumber
                                        (fldMapping.FieldNumber == 9) ||
                                            (fldMapping.FieldNumber == 10) ||
                                            (fldMapping.FieldNumber == 11) ||
                                            (fldMapping.FieldNumber == 12)) //||
                                                                           //(ThisField.FieldNumber == 80)) have to find re-direct number field for zte
                                    {
                                        strThisField = BinCdr.BcdToDigitStringZte(fieldAsByte).ToString();
                                    }

                                    break;

                                case "zte time":

                                    //debug
                                    //if (RowCnt == 3)
                                    //{
                                    //    int r = 1;
                                    //}

                                    byte[] b = new byte[4];
                                    int j = 0;
                                    int m10 = fieldAsByte[4]; //10 milli seconds part
                                    for (j = 0; j < 4; j++)
                                    {
                                        b[j] = fieldAsByte[j];
                                    }

                                    UInt32 elapsedSeconds = BinCdr.ByteArrayToZteSeconds(b);

                                    if (fldMapping.FieldNumber == 29) //start time
                                    {
                                        //take intrkconnecttime or outtrkconnecttime, whichever is present
                                        //if both are 0, then use answer time

                                        //outtrk	intrk	diff
                                        //255	    214	    41

                                        //ans	    intrk	diff
                                        //195	    214	    -19

                                        if (elapsedSeconds == 0) //start time=intrkconnecttime not present
                                        {
                                            //read outtrkconnecttime
                                            byte[] tempFieldAsByte = null;
                                            tempFieldAsByte = new byte[Convert.ToInt32(fldMapping.BinByteLen)];
                                            Buffer.BlockCopy(a,
                                                Convert.ToInt32(thisOffSetInt + 41 + (rowCnt * zteFixedBytePerRows)),
                                                tempFieldAsByte, 0, Convert.ToInt32(fldMapping.BinByteLen));

                                            byte[] b1 = new byte[4];
                                            int j1 = 0;
                                            m10 = tempFieldAsByte[4]; //10 milli seconds part
                                            for (j1 = 0; j1 < 4; j1++)
                                            {
                                                b1[j1] = tempFieldAsByte[j1];
                                            }

                                            elapsedSeconds =
                                                BinCdr.ByteArrayToZteSeconds(
                                                    b1); //outtrkconnect time is found and will be used as start time in the next block
                                        }

                                        if (elapsedSeconds == 0
                                        ) //start time=outrkconnecttime is also not present, then use answer time as starttime
                                        {
                                            //read outtrkconnecttime
                                            byte[] tempFieldAsByte = null;
                                            tempFieldAsByte = new byte[Convert.ToInt32(fldMapping.BinByteLen)];
                                            Buffer.BlockCopy(a,
                                                Convert.ToInt32(thisOffSetInt - 19 + (rowCnt * zteFixedBytePerRows)),
                                                tempFieldAsByte, 0, Convert.ToInt32(fldMapping.BinByteLen));

                                            byte[] b1 = new byte[4];
                                            int j1 = 0;
                                            m10 = tempFieldAsByte[4]; //10 milli seconds part
                                            for (j1 = 0; j1 < 4; j1++)
                                            {
                                                b1[j1] = tempFieldAsByte[j1];
                                            }

                                            elapsedSeconds =
                                                BinCdr.ByteArrayToZteSeconds(
                                                    b1); //answer time is found and will be used as start time in the next block
                                        }
                                    } //if field number 29=starttime


                                    DateTime thisDateTime =
                                        zteStartofTime.AddSeconds(elapsedSeconds); //seconds part addition
                                    thisDateTime = thisDateTime.AddMilliseconds(m10 * 10); //milli seconds part addition
                                    strThisField = thisDateTime.ToString("yyyy-MM-dd HH:mm:ss");

                                    if (fldMapping.FieldNumber == 15) //end time
                                    {
                                        zteEndTime = thisDateTime;
                                    }

                                    else if (fldMapping.FieldNumber == 17) //answer time
                                    {
                                        zteAnswerTime = thisDateTime;

                                        //calculate DurationSec and populate in current row
                                        TimeSpan thisDurationSec = zteEndTime - zteAnswerTime;
                                        thisRow[14] = Math.Round(thisDurationSec.TotalMilliseconds / 1000, 2)
                                            .ToString();
                                    }
                                    break;
                                case "ip address":
                                    string ip = BinCdr.ByteArrayToIpAddress(fieldAsByte);
                                    strThisField = ip;
                                    break;

                                case "zte spc":
                                    string pointCode = BinCdr.ByteArrayToZteSpc(fieldAsByte);
                                    strThisField = pointCode;
                                    break;

                            } //switch
                            thisRow[fldMapping.FieldNumber] = strThisField;
                        } //if field present in CDR
                        //fieldCnt++;


                    } //for each field

                    //if this row is a final cdr then add this...based on field=55

                    //0：one record
                    //1：first part
                    //2：interim part
                    //3：final part

                    //TxtTable.Add(ThisRow);

                    //if ((ThisRow[55] == "0") || (ThisRow[55] == "3"))
                    //{
                    //    //also check if this cdr has a valid flag according to zte
                    //    //0: valid; Shows a valid CDR record. 
                    //    //1: invalid; Shows an invalid CDR record. 

                    ////if valid answertime and partial cdr flag found
                    if (!thisRow[Fn.Partialflag].IsNullOrEmptyOrWhiteSpace() 
                        && thisRow[Fn.Partialflag].Trim().ValueIn(new[] {"1", "2", "3"}))
                    {
                        thisRow[89] = thisRow[14]; //partial duration
                        thisRow[90] = thisRow[17]; //partial answertime
                        thisRow[91] = thisRow[15]; //partial endtime
                    }
                    else
                    {
                        thisRow[Fn.Partialflag] = "0";
                        thisRow[Fn.FinalRecord] = "1";
                    }
                    decodedRows.Add(thisRow); 
                }
                catch (Exception e1)
                {
                    Console.WriteLine(e1);
                    inconsistentCdrs.Add(CdrConversionUtil.ConvertTxtRowToCdrinconsistent(thisRow));
                    ErrorWriter wr = new ErrorWriter(e1, "DecodeCdr", null,
                        "Encounterd error during decoding with ZteDecoderHelper."
                        , input.Tbc.DatabaseSetting.DatabaseName);
                    continue;//with next switch
                }
            }//for each row in byte array
            return decodedRows;
        }
    }
}
