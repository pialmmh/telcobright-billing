namespace Utils.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("platinum.zone")]
    public partial class zone
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public zone()
        {
            timezones = new HashSet<timezone>();
        }

        [Key]
        public int zone_id { get; set; }

        [StringLength(2)]
        public string country_code { get; set; }

        [Required]
        [StringLength(35)]
        public string zone_name { get; set; }

        public virtual country country { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<timezone> timezones { get; set; }
    }
}
