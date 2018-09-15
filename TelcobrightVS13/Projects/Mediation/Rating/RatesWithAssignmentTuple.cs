using System.Collections.Generic;
using MediationModel;

namespace TelcobrightMediation
{
    public class RatesWithAssignmentTuple
    {
        public TupleByPeriod Tup { get; }
        public List<Rateext> Rates { get; }

        public RatesWithAssignmentTuple(TupleByPeriod tup, List<Rateext> rates)
        {
            this.Tup = tup;
            this.Rates = rates;
        }
    }
}