using System;
using MediationModel;

namespace TelcobrightMediation
{
    public class AutoIncrementManagerManualInt : AutoIncrementManagerManual<int>
    {
        public AutoIncrementManagerManualInt(PartnerEntities context) : base(context)
        {
        }
        public override int GetNewCounter()
        {
            int newCounter = base.GetCounterFromDb<int>("autoinc_manual_int");
            return Int32.MaxValue - Convert.ToInt32(newCounter) + 1;
        }

        public override long GetNewCounter(AutoIncrementCounterType counterType)
        {
            return this.GetNewCounter();
        }
    }

    public class AutoIncrementManagerManualLong : AutoIncrementManagerManual<long>
    {
        public AutoIncrementManagerManualLong(PartnerEntities context) : base(context){}
        public override long GetNewCounter()
        {
            long newCounter = base.GetCounterFromDb<int>("autoinc_manual_long");
            return Int64.MaxValue - newCounter + 1;
        }

        public override long GetNewCounter(AutoIncrementCounterType counterType)
        {
            return this.GetNewCounter();
        }
    }
}