using System;
using System.Collections.Generic;
using System.Linq;

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
                throw new Exception("MefTimeCycleRules are not composed, " +
                                    "method ComposeMefRules must be called first.");
            return TimeCyclesDictionary[this.TimeCycleName];
        }
        public void ComposeMefRules(string mefCatalogPath)
        {
            TimeCycleComposer composer = new TimeCycleComposer();
            composer.ComposeFromPath(mefCatalogPath);
            TimeCyclesDictionary = composer.TimeCycles.ToDictionary(c => c.Name);
            IsMefComposed = true;
        }
    }
}