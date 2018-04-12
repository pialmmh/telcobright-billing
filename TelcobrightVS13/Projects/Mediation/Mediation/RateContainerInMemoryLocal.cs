using System;
using System.Collections.Generic;
using MediationModel;

namespace TelcobrightMediation
{
    public class RateContainerInMemoryLocal
    {
        public PartnerEntities Context { get; }
        Dictionary<string, Rateext> _dicRatesInMemory = new Dictionary<string, Rateext>();//key=id/startdate/enddate tuple,value=rateext
        public RateContainerInMemoryLocal(PartnerEntities context)
        {
            this.Context = context;
        }
        public Rateext Append(Rateext newRate)
        {
            Rateext existingRateInMemory = null;
            this._dicRatesInMemory.TryGetValue(newRate.ToString(), out existingRateInMemory);
            if (existingRateInMemory != null)
            {
                return existingRateInMemory;//rate already in memory
            }
            else
            {
                //add this rate to the dictionary and return the new instance
                this._dicRatesInMemory.Add(newRate.ToString(), newRate);
                this._dicRatesInMemory.TryGetValue(newRate.ToString(), out existingRateInMemory);
                if (existingRateInMemory == null)
                {
                    throw new Exception("Existing Rate should not be null right after being added.");
                }
                //else return the new rate instance pointed by existingrate
                return existingRateInMemory;
            }
        }
    }
}