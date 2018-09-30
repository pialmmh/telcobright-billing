using System;
using System.Collections.Generic;
using System.Linq;
using TelcobrightMediation;

namespace LibraryExtensions.TimeCycle
{
    public class TimeCycleFactory
    {
        public string TimeCycleName { get; set; }
        public int Duration { get; set; }
        private static bool IsMefComposed { get; set; }
        private static Dictionary<string, ITimeCycle> TimeCyclesDictionary { get; set; } =
            new Dictionary<string, ITimeCycle>();
        public ITimeCycle GetTimeCycle()
        {
            if (!IsMefComposed)
            {
                ComposeBillingCycles();
            }
            return TimeCyclesDictionary[this.TimeCycleName];
        }
        private void ComposeBillingCycles()
        {
            TimeCycleComposer composer = new TimeCycleComposer();
            composer.Compose();
            TimeCyclesDictionary = composer.TimeCycles.ToDictionary(c => c.Name);
            IsMefComposed = true;
        }
    }
}