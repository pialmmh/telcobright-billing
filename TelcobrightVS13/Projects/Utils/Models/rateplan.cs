namespace Utils.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("platinum.rateplan")]
    public partial class rateplan
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public rateplan()
        {
            ratetaskreferences = new HashSet<ratetaskreference>();
        }

        public long id { get; set; }

        public int Type { get; set; }

        [Required]
        [StringLength(200)]
        public string RatePlanName { get; set; }

        [StringLength(200)]
        public string Description { get; set; }

        public DateTime date1 { get; set; }

        public int? field1 { get; set; }

        public int? field2 { get; set; }

        public int? field3 { get; set; }

        [StringLength(45)]
        public string field4 { get; set; }

        [StringLength(45)]
        public string field5 { get; set; }

        public int TimeZone { get; set; }

        public int idCarrier { get; set; }

        [Required]
        [StringLength(10)]
        public string Currency { get; set; }

        public DateTime? codedeletedate { get; set; }

        public int? ChangeCommitted { get; set; }

        public int Resolution { get; set; }

        public float mindurationsec { get; set; }

        public int SurchargeTime { get; set; }

        public double SurchargeAmount { get; set; }

        public sbyte? Category { get; set; }

        public sbyte? SubCategory { get; set; }

        [Required]
        [StringLength(10)]
        public string BillingSpan { get; set; }

        public int? RateAmountRoundupDecimal { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ratetaskreference> ratetaskreferences { get; set; }
    }
}
