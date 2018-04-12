namespace Utils.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("platinum.cdrfieldlist")]
    public partial class cdrfieldlist
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public cdrfieldlist()
        {
            cdrfieldmappingbyswitchtypes = new HashSet<cdrfieldmappingbyswitchtype>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int fieldnumber { get; set; }

        [Required]
        [StringLength(100)]
        public string FieldName { get; set; }

        public int? IsNumeric { get; set; }

        public int? IsDateTime { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<cdrfieldmappingbyswitchtype> cdrfieldmappingbyswitchtypes { get; set; }
    }
}
