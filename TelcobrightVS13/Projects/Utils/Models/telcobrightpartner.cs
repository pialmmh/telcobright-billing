namespace Utils.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("platinum.telcobrightpartner")]
    public partial class telcobrightpartner
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public telcobrightpartner()
        {
            nes = new HashSet<ne>();
        }

        [Key]
        public int idCustomer { get; set; }

        [StringLength(100)]
        public string CustomerName { get; set; }

        public int? idOperatorType { get; set; }

        [StringLength(100)]
        public string databasename { get; set; }

        [StringLength(100)]
        public string databasetype { get; set; }

        [StringLength(100)]
        public string user { get; set; }

        [StringLength(100)]
        public string pass { get; set; }

        [StringLength(45)]
        public string ServerNameOrIP { get; set; }

        [StringLength(100)]
        public string IBServerNameOrIP { get; set; }

        [StringLength(100)]
        public string IBdatabasename { get; set; }

        [StringLength(100)]
        public string IBdatabasetype { get; set; }

        [StringLength(100)]
        public string IBuser { get; set; }

        [StringLength(100)]
        public string IBpass { get; set; }

        public int? TransactionSizeForCDRLoading { get; set; }

        public int NativeTimeZone { get; set; }

        [StringLength(45)]
        public string IgwPrefix { get; set; }

        public int RateDictionaryMaxRecords { get; set; }

        public float MinMSForIntlOut { get; set; }

        public int RawCdrKeepDurationDays { get; set; }

        public int SummaryKeepDurationDays { get; set; }

        public int AutoDeleteOldData { get; set; }

        public float? AutoDeleteStartHour { get; set; }

        public float? AutoDeleteEndHour { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ne> nes { get; set; }
    }
}
