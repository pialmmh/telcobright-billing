using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MediationModel
{
    public class LcrDisplayClass
    {
        [Key]
        public long Id { get; set; }
        public string Prefix { get; set; }
        public double LowestRate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime LastUpdated { get; set; }
        public string PrefixDescription { get; set; }
        public Dictionary<string, List<RoutePartnerPair>> DicCostEntities { get; set; }//cost is the key
        public LcrDisplayClass(lcr lcr, string prefixDescription, Dictionary<string, List<RoutePartnerPair>> diccostEntities)
        {
            this.Id = lcr.id;
            this.Prefix = lcr.Prefix;
            this.PrefixDescription = prefixDescription;
            this.StartDate = lcr.startdate;
            this.LastUpdated = lcr.LastUpdated;
            this.DicCostEntities = diccostEntities;
        }

    }
}