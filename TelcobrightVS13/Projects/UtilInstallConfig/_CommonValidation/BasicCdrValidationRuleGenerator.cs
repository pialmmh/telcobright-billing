using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstallConfig._CommonValidation
{
    public class BasicCdrValidationRuleGenerator
    {
        private DateTime NotAllowedCallDateTimeBefore { get; }

        public BasicCdrValidationRuleGenerator(DateTime notAllowedCallDateTimeBefore)
        {
            this.NotAllowedCallDateTimeBefore = notAllowedCallDateTimeBefore;
        }

        public virtual Dictionary<string, string> GetCommonValidationRules()
        {
            return new Dictionary<string, string>()
            {
                {
                    "!String.IsNullOrEmpty(obj.UniqueBillId) and !String.IsNullOrWhiteSpace(obj.UniqueBillId)",
                    "UniqueBillId cannot be empty"
                }, //public const int UniqueBillId = 98;
                {
                    "obj.SequenceNumber > 0",
                    "SequenceNumber must be > 0"
                }, //public const int Sequencenumber = 2;
                {
                    "!String.IsNullOrEmpty(obj.IncomingRoute) and !String.IsNullOrWhiteSpace(obj.IncomingRoute)",
                    "IncomingRoute cannot be empty"
                }, //public const int IncomingRoute = 5;
                {
                    "!String.IsNullOrEmpty(obj.OriginatingCalledNumber) and !String.IsNullOrWhiteSpace(obj.OriginatingCalledNumber)",
                    "OriginatingCalledNumber cannot be empty"
                }, //public const int Originatingcallednumber = 9;
                {
                    "obj.DurationSec >= 0",
                    "DurationSec must be >= 0"
                }, //public const int DurationSec = 14;
                {
                    "obj.StartTime > date('" + this.NotAllowedCallDateTimeBefore.ToString("yyyy-MM-dd") + "')",
                    "StartTime must be > " + this.NotAllowedCallDateTimeBefore.ToString("yyyy-MM-dd")
                }, //public const int StartTime = 29;
                {
                    "obj.EndTime >= obj.StartTime",
                    "EndTime must be >= StartTime"
                }, //public const int Endtime = 15;
                {
                    "obj.validflag > 0",
                    "validflag must be > 0"
                }, //public const int Validflag = 54;
                {
                    "obj.SwitchId > 0",
                    "SwitchId must be > 0"
                },
                {
                    "obj.IdCall > 0",
                    "IdCall must be > 0"
                },
                {
                    "!String.IsNullOrEmpty(obj.FileName) and !String.IsNullOrWhiteSpace(obj.FileName)",
                    "FileName cannot be empty"
                },
                {
                    "obj.FinalRecord == 1",
                    "FinalRecord must be 1"
                },
                {
                    "obj.EndTime !=null and obj.EndTime >= obj.StartTime",
                    "EndTime must be >= StartTime"
                },
                {
                    "obj.DurationSec > 0?obj.ChargingStatus == 1: obj.ChargingStatus == 0",
                    "ChargingStatus must be 1 when DurationSec > 0 , otherwise == 0 "
                },
                {
                    "obj.InPartnerId!=null and obj.InPartnerId > 0",
                    "InPartnerId must be > 0"
                },
                {
                    "obj.ServiceGroup > 0",
                    "ServiceGroup must be > 0"
                },
            };
        }

        public virtual Dictionary<string, string> GetInconsistentValidationRules()
        {
            return new Dictionary<string, string>()
            {
                {
                    "!String.IsNullOrEmpty(obj[98]) and !String.IsNullOrWhiteSpace(obj[98])",
                    "UniqueBillId cannot be empty"
                }, //public const int UniqueBillId = 98;
                {
                    $@"Validator.BooleanParsers['isNumericChecker'].Invoke(obj[2]) == true 
                         and Validator.IntParsers['intConverterProxy'].Invoke(obj[2]) > 0",
                    "SequenceNumber must be numeric and > 0"
                }, //public const int Sequencenumber = 2;
                {
                    "!String.IsNullOrEmpty(obj[5]) and !String.IsNullOrWhiteSpace(obj[5])",
                    "IncomingRoute cannot be empty"
                }, //public const int IncomingRoute = 5;
                {
                    "!String.IsNullOrEmpty(obj[9]) and !String.IsNullOrWhiteSpace(obj[9])",
                    "OriginatingCalledNumber cannot be empty"
                }, //public const int Originatingcallednumber = 9;
                {
                    $@"Validator.BooleanParsers['isNumericChecker'].Invoke(obj[14]) 
                        and Validator.DoubleParsers['doubleConverterProxy'].Invoke(obj[14]) >= 0",
                    "DurationSec must be numeric and >= 0"
                }, //public const int DurationSec = 14;
                {
                    $@"Validator.BooleanParsers['isDateTimeChecker'].Invoke(obj[29]) == true
                        and Validator.DateParsers['strToMySqlDtConverter'].Invoke(obj[29]) 
                        > date('" + this.NotAllowedCallDateTimeBefore.ToString("yyyy-MM-dd") + "')",
                    "StartTime must be a valid datetime and > " +
                    this.NotAllowedCallDateTimeBefore.ToString("yyyy-MM-dd")
                }, //public const int StartTime = 29;
                {
                    $@"Validator.BooleanParsers['isDateTimeChecker'].Invoke(obj[15]) == true and 
                        Validator.DateParsers['strToMySqlDtConverter'].Invoke(obj[15]) >= 
                        Validator.DateParsers['strToMySqlDtConverter'].Invoke(obj[29]" + ")",
                    "EndTime must be a valid datetime and >= StartTime"
                }, //public const int Endtime = 15;
                {
                    "obj[54] == '1'",
                    "validflag must be 1"
                }, //public const int Validflag = 54;
            };
        }
        
    }
}
