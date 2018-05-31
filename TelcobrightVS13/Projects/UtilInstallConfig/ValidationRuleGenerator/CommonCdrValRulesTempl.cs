using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstallConfig._CommonValidation
{
    public class CommonCdrValRulesTempl
    {
        private DateTime NotAllowedCallDateTimeBefore { get; }

        public CommonCdrValRulesTempl(DateTime notAllowedCallDateTimeBefore)
        {
            this.NotAllowedCallDateTimeBefore = notAllowedCallDateTimeBefore;
        }
        public virtual Dictionary<string, string> GetCommonValidationRules()
        {
            return new Dictionary<string, string>()
            {
                {
                    "!String.IsNullOrEmpty(obj.UniqueBillId) && !String.IsNullOrWhiteSpace(obj.UniqueBillId)",
                    "UniqueBillId cannot be empty"
                }, //public const int UniqueBillId = 98;
                {
                    "obj.SequenceNumber > 0",
                    "SequenceNumber Must be Greater than 0"
                }, //public const int Sequencenumber = 2;
                {
                    "!String.IsNullOrEmpty(obj.IncomingRoute) && !String.IsNullOrWhiteSpace(obj.IncomingRoute)",
                    "IncomingRoute cannot be empty"
                }, //public const int IncomingRoute = 5;
                {
                    "!String.IsNullOrEmpty(obj.OriginatingCalledNumber) && !String.IsNullOrWhiteSpace(obj.OriginatingCalledNumber)",
                    "OriginatingCalledNumber cannot be empty"
                }, //public const int Originatingcallednumber = 9;
                {
                    "obj.DurationSec >= 0",
                    "DurationSec must be Greater than or Equal to 0"
                }, //public const int DurationSec = 14;
                {
                    "obj.StartTime > new DateTime("+this.NotAllowedCallDateTimeBefore.Year+ ","+
                    this.NotAllowedCallDateTimeBefore.Month+","+this.NotAllowedCallDateTimeBefore.Day+")",
                    "StartTime must be Greater than " + this.NotAllowedCallDateTimeBefore.ToString("yyyy-MM-dd")
                }, //public const int StartTime = 29;
                {
                    "obj.EndTime >= obj.StartTime",
                    "EndTime must be Greater than or Equal to StartTime"
                }, //public const int Endtime = 15;
                {
                    "obj.validflag > 0",
                    "validflag must be Greater than 0"
                }, //public const int Validflag = 54;
                {
                    "obj.SwitchId > 0",
                    "SwitchId must be Greater than 0"
                },
                {
                    "obj.IdCall > 0",
                    "IdCall must be Greater than 0"
                },
                {
                    "!String.IsNullOrEmpty(obj.FileName) && !String.IsNullOrWhiteSpace(obj.FileName)",
                    "FileName cannot be empty"
                },
                {
                    "obj.FinalRecord == 1",
                    "FinalRecord must be 1"
                },
                {
                    "obj.EndTime !=null && obj.EndTime >= obj.StartTime",
                    "EndTime must be Greater than or Equal to StartTime"
                },
                {
                    "obj.DurationSec > 0?obj.ChargingStatus == 1: obj.ChargingStatus == 0",
                    "ChargingStatus must be 1 when DurationSec Greater than zero"
                },
                {
                    "obj.InPartnerId!=null && obj.InPartnerId > 0",
                    "InPartnerId must be Greater than 0"
                },
                {
                    "obj.ServiceGroup > 0",
                    "ServiceGroup must be Greater than 0"
                },
            };
        }

    }
}
