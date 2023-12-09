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
using System.IO;                                                                                                   
using System.Text;                                                                                                 
using LibraryExtensions;                                                                                           
                                                                                                                   
namespace Decoders                                                                                                 
{                                                                                                                  
                                                                                                                   
    [Export("Decoder", typeof(AbstractCdrDecoder))]                                                                
    public class TelcobridgeDecoderGazi : AbstractCdrDecoder                                                       
    {                                                                                                              
        public override string ToString() => this.RuleName;                                                        
        public override string RuleName => GetType().Name;                                                         
        public override int Id => 57;                                                                              
        public override string HelpText => "Gazi Telcobridge";                                                     
        public override CompressionType CompressionType { get; set; }                                              
        protected CdrCollectorInputData Input { get; set; }                                                        
                                                                                                                   
        private static string parseStringToDate(string timestamp)                                                  
        {                                                                                                          
            DateTime dateTime;                                                                                     
            timestamp = timestamp.Split('.')[0]; //remove milliseconds part, not necessaryf for gazi decoder       
            if (DateTime.TryParseExact(timestamp, "yyyyMMddHHmmss", CultureInfo.InvariantCulture,                  
                DateTimeStyles.None, out dateTime))                                                                
            {                                                                                                      
                return dateTime.ToMySqlFormatWithoutQuote();                                                       
            }                                                                                                      
            return "";                                                                                             
        }                                                                                                          
                                                                                                                   
        public override List<string[]> DecodeFile(CdrCollectorInputData input,                                     
            out List<cdrinconsistent> inconsistentCdrs)                                                            
        {                                                                                                          
            this.Input = input;                                                                                    
            string fileName = this.Input.FullPath;                                                                 
            //List<string[]> lines = FileUtil.ParseCsvWithEnclosedAndUnenclosedFields(fileName, ',', 1, "\"", ";");
            //List<string[]> lines = FileUtil.ParseCsvWithEnclosedAndUnenclosedFields(fileName, ',', 0, "\"", ",");
                                                                                                                   
            // my parser                                                                                           
            string[] linesAsString = File.ReadAllLines(fileName);                                                  
            string[] lineAsArr;                                                                                    
                                                                                                                   
            inconsistentCdrs = new List<cdrinconsistent>();                                                        
            List<string[]> decodedRows = new List<string[]>();                                                     
            //this.Input = input;                                                                                  
            List<cdrfieldmappingbyswitchtype> fieldMappings = null;                                                
                                                                                                                   
            try                                                                                                    
            {                                                                                                      
                foreach (string ln in linesAsString)                                                               
                {                                                                                                  
                    lineAsArr = ln.Split(',');                                                                     
                    string[] textCdr = new string[input.MefDecodersData.Totalfieldtelcobright];                    
                                                                                                                   
                    string durationStr = lineAsArr[7].Trim();                                                      
                    double durationSec = 0;                                                                        
                    double.TryParse(durationStr, out durationSec);                                                 
                                                                                                                   
                    textCdr[Fn.UniqueBillId] = lineAsArr[2].Trim();                                                
                    textCdr[Fn.Partialflag] = "1";// all telcobridge cdrs are partial                              
                    textCdr[Fn.DurationSec] = durationSec.ToString();                                              
                    textCdr[Fn.ChargingStatus] = durationSec > 0 ? "1" : "0";                                      
                                                                                                                   
                    //originate or answer, intrunk taken from "originate" let, outtrunk from "answer" leg          
                    textCdr[Fn.InTrunkAdditionalInfo] = lineAsArr[16].Trim().ToLower();                            
                    textCdr[Fn.OutTrunkAdditionalInfo] = lineAsArr[0].Trim().ToLower();//start,update,end          
                                                                                                                   
                    string startTime = lineAsArr[4].Trim();                                                        
                    if (!string.IsNullOrEmpty(startTime))                                                          
                    {                                                                                              
                        startTime = parseStringToDate(startTime);                                                  
                    }                                                                                              
                                                                                                                   
                    string connectTime = lineAsArr[5].Trim();                                                      
                    if (!string.IsNullOrEmpty(connectTime))                                                        
                    {                                                                                              
                        connectTime = parseStringToDate(connectTime);                                              
                    }                                                                                              
                                                                                                                   
                    string endTime = lineAsArr[6].Trim();                                                          
                    if (!string.IsNullOrEmpty(endTime))                                                            
                    {                                                                                              
                        endTime = parseStringToDate(endTime);                                                      
                    }                                                                                              
                                                                                                                   
                    textCdr[Fn.Filename] = fileName;                                                               
                    textCdr[Fn.Switchid] = Input.Ne.idSwitch.ToString();                                           
                                                                                                                   
                    textCdr[Fn.OriginatingCallingNumber] = lineAsArr[8].Trim();                                    
                    textCdr[Fn.OriginatingCalledNumber] = lineAsArr[9].Trim();                                     
                                                                                                                   
                    textCdr[Fn.TerminatingCallingNumber] = lineAsArr[8].Trim();                                    
                    textCdr[Fn.TerminatingCalledNumber] = lineAsArr[9].Trim();                                     
                                                                                                                   
                                                                                                                   
                    textCdr[Fn.IncomingRoute] = lineAsArr[12].Trim();                                              
                    textCdr[Fn.OutgoingRoute] = lineAsArr[11].Trim();                                              
                                                                                                                   
                    textCdr[Fn.AnswerTime] = connectTime.IsNullOrEmptyOrWhiteSpace()?startTime:connectTime;        
                    textCdr[Fn.StartTime] = startTime;                                                             
                    textCdr[Fn.Endtime] = endTime.IsNullOrEmptyOrWhiteSpace()?connectTime:endTime;                 
                                                                                                                   
                                                                                                                   
                    string status = lineAsArr[0].Trim().ToLower();                                                 
                                                                                                                   
                    if (lineAsArr[16].ToLower() == "originate")                                                    
                    {                                                                                              
                        textCdr[Fn.IncomingRoute] = lineAsArr[11].Trim();                                          
                    }                                                                                              
                    else if (lineAsArr[16].ToLower() == "answer")                                                  
                    {                                                                                              
                        textCdr[Fn.OutgoingRoute] = lineAsArr[11].Trim();                                          
                    }                                                                                              
                                                                                                                   
                    textCdr[Fn.Validflag] = "1";                                                                   
                    string seqNumber = lineAsArr[3].Remove(0, 2);                                                  
                    seqNumber = Int64.Parse(seqNumber, NumberStyles.HexNumber).ToString();                         
                    textCdr[Fn.Sequencenumber] = seqNumber;                                                        
                                                                                                                   
                    decodedRows.Add(textCdr);                                                                      
                }                                                                                                  
            }                                                                                                      
            catch (Exception e)                                                                                    
            {                                                                                                      
                Console.WriteLine(e);                                                                              
                e.Data.Add("customError", "Possibly Corrupted");                                                   
                throw e;                                                                                           
            }                                                                                                      
            return decodedRows;                                                                                    
        }                                                                                                          
                                                                                                                   
        public override string getTupleExpression(Object data)                                                     
        {                                                                                                          
            Dictionary<string, object> dataAsDic = (Dictionary<string, object>)data;                               
            CdrCollectorInputData collectorInput = (CdrCollectorInputData)dataAsDic["collectorInput"];             
            CdrSetting cdrSetting = collectorInput.CdrSetting;                                                     
            string[] row = (string[])dataAsDic["row"];                                                             
            int switchId = collectorInput.Ne.idSwitch;                                                             
            DateTime startTime = getEventDatetime(new Dictionary<string, object>                                   
            {                                                                                                      
                {"cdrSetting", cdrSetting},                                                                        
                {"row", row}                                                                                       
            });                                                                                                    
            //23:00 hours eventid to be rounded up as 00:00 next hour in uniqueEventTupleId                        
            //aggregation logic checks cdr for +-1 hour, so collection and aggregation will be possible            
            if (startTime.Hour == 23)                                                                              
            {                                                                                                      
                startTime = startTime.Date.AddDays(1);                                                             
            }                                                                                                      
            else                                                                                                   
            {                                                                                                      
                startTime = startTime.Date.AddHours(startTime.Hour);                                               
            }                                                                                                      
            string sessionId = row[Fn.UniqueBillId];                                                               
            string separator = "/";                                                                                
            return new StringBuilder(switchId.ToString()).Append(separator)                                        
                .Append(startTime.ToMySqlFormatWithoutQuote()).Append(separator)                                   
                .Append(sessionId).ToString();                                                                     
        }                                                                                                          
        public override EventAggregationResult Aggregate(object data)                                              
        {                                                                                                          
            return TelcobridgeAggregationHelper.Aggregate(data);                                                   
        }                                                                                                          
    }                                                                                                              
}                                                                                                                  