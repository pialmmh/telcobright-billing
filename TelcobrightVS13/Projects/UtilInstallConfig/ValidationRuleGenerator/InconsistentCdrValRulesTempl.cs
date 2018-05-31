using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstallConfig._CommonValidation
{
    public class InconsistentCdrValRulesTempl
    {
        private DateTime NotAllowedCallDateTimeBefore { get; }

        public InconsistentCdrValRulesTempl(DateTime notAllowedCallDateTimeBefore)
        {
            this.NotAllowedCallDateTimeBefore = notAllowedCallDateTimeBefore;
        }
        
        public virtual Dictionary<string, string> GetInconsistentValidationRules()
        {
            return new Dictionary<string, string>()
            {
                {
                    "!String.IsNullOrEmpty(obj[98]) && !String.IsNullOrWhiteSpace(obj[98])",
                    "UniqueBillId Cannot be Empty"
                }, //public const int UniqueBillId = 98;
                {
                    "obj[2].IsNumeric() == true && Convert.ToInt64(obj[2]) > 0",
                    "SequenceNumber Must be Numeric and Greater Than 0"
                }, //public const int Sequencenumber = 2;
                {
                    "!String.IsNullOrEmpty(obj[5]) && !String.IsNullOrWhiteSpace(obj[5])",
                    "IncomingRoute Cannot be Empty"
                }, //public const int IncomingRoute = 5;
                {
                    "!String.IsNullOrEmpty(obj[9]) && !String.IsNullOrWhiteSpace(obj[9])",
                    "OriginatingCalledNumber Cannot be Empty"
                }, //public const int Originatingcallednumber = 9;
                {
                    "obj[14].IsNumeric() && Convert.ToDouble(obj[14]) >= 0",
                    "DurationSec Must be Numeric and Greater than or Equal to 0"
                }, //public const int DurationSec = 14;
                {
                    $@"ClassBody:
                       DateTime startTime;
                       if(obj[29].TryParseToDateTimeFromMySqlFormat() == false)
                           return false;
                       else
                         startTime=obj[29].ConvertToDateTimeFromMySqlFormat();                                                     
                       return startTime > new DateTime("+this.NotAllowedCallDateTimeBefore.Year+ ","+
                    this.NotAllowedCallDateTimeBefore.Month+","+this.NotAllowedCallDateTimeBefore.Day+")",
                    "StartTime Must be a Valid Datetime and Greater Than " + this.NotAllowedCallDateTimeBefore.ToString("yyyy-MM-dd")
                }, //public const int StartTime = 29;
                {
                    $@"ClassBody:
                       DateTime endTime;
                       DateTime startTIme;
                       if(obj[15].TryParseToDateTimeFromMySqlFormat() == true)
                        endTime=obj[15].ConvertToDateTimeFromMySqlFormat();     
                       if(obj[29].TryParseToDateTimeFromMySqlFormat() == true)
                         startTime=obj[29].ConvertToDateTimeFromMySqlFormat();                                                     
                       return endTime >= startTime;",
                    "EndTime Must be a Valid Datetime and Greater than or Equals to StartTime"
                }, //public const int Endtime = 15;
                {
                    "obj[54] == '1'",
                    "validflag must be 1"
                }, //public const int Validflag = 54;
            };
        }
        
        
    }
}
