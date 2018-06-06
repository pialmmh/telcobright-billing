using System;
using System.ComponentModel.Composition;
using MediationModel;
using TelcobrightMediation;
using LibraryExtensions;
namespace CdrValidationRules
{
    [Export(typeof(IValidationRule<>))]
    public class StrEndTimeIsGtEqStartTime : IValidationRule<string[]>
    {
        public override string ToString() => this.RuleName;
        public string RuleName => GetType().Name;
        public string ValidationMessage => "EndTime Must be a Valid Datetime and >= to StartTime";
        public object Data { get; set; }
        private DateTime StartTime { get; set; }
        public bool IsPrepared { get; private set; }
        public void Prepare()
        {
            this.IsPrepared = true;
        }
        public bool Validate(string[] obj)
        {
            if (this.IsPrepared == false)
                throw new Exception("Rule is not prepared, method Prepare must be called first.");
            DateTime endTime;
            DateTime startTime;
            if (obj[15].TryParseToDateTimeFromMySqlFormat(out endTime) == true)
            {
                if (obj[29].TryParseToDateTimeFromMySqlFormat(out startTime) == true)
                {
                    return endTime >= startTime;
                }
            }
            return false;
        }
    }
}

