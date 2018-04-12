namespace Utils.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("platinum.partner")]
    public partial class partner
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public partner()
        {
            routes = new HashSet<route>();
        }

        [Key]
        public int idPartner { get; set; }

        [Required]
        [StringLength(45)]
        public string PartnerName { get; set; }

        [StringLength(100)]
        public string Address1 { get; set; }

        [StringLength(100)]
        public string Address2 { get; set; }

        [StringLength(45)]
        public string City { get; set; }

        [StringLength(45)]
        public string State { get; set; }

        [StringLength(45)]
        public string PostalCode { get; set; }

        [StringLength(45)]
        public string Country { get; set; }

        [StringLength(45)]
        public string Telephone { get; set; }

        [StringLength(50)]
        public string email { get; set; }

        public int CustomerPrePaid { get; set; }

        public int PartnerType { get; set; }

        public int? billingdate { get; set; }

        public int? AllowedDaysForInvoicePayment { get; set; }

        public int? timezone { get; set; }

        public DateTime? date1 { get; set; }

        public int? field1 { get; set; }

        public int? field2 { get; set; }

        public int? field3 { get; set; }

        [StringLength(45)]
        public string field4 { get; set; }

        [StringLength(45)]
        public string field5 { get; set; }

        public float? refasr { get; set; }

        public float? refacd { get; set; }

        public float? refccr { get; set; }

        public float? refccrbycc { get; set; }

        public float? refpdd { get; set; }

        public float? refasrfas { get; set; }

        public int DefaultCurrency { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<route> routes { get; set; }
    }
}
