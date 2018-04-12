namespace Utils.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("platinum.usdexchangerateagainstbdt")]
    public partial class usdexchangerateagainstbdt
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int idUSDExchangeRateAgainstBDT { get; set; }

        public DateTime EffectiveDate { get; set; }

        public decimal RateAmount { get; set; }

        [StringLength(100)]
        public string Comment { get; set; }

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
