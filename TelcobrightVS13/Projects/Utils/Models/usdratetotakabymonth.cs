namespace Utils.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("platinum.usdratetotakabymonth")]
    public partial class usdratetotakabymonth
    {
        public int id { get; set; }

        public int year_ { get; set; }

        public int? month_ { get; set; }

        public float? BuyingRate { get; set; }

        public float? SellingRate { get; set; }

        public DateTime? date1 { get; set; }

        public int? field1 { get; set; }

        public int? field2 { get; set; }

        public int? field3 { get; set; }

        [StringLength(45)]
        public string field4 { get; set; }

        [StringLength(45)]
        public string field5 { get; set; }
    }
}
