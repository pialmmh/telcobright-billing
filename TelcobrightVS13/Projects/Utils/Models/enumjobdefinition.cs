namespace Utils.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("platinum.enumjobdefinition")]
    public partial class enumjobdefinition
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public enumjobdefinition()
        {
            jobs = new HashSet<job>();
        }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int id { get; set; }

        public int jobtypeid { get; set; }

        [Required]
        [StringLength(45)]
        public string Type { get; set; }

        public int Priority { get; set; }

        public int BatchCreatable { get; set; }

        public int JobQueue { get; set; }

        public int Disabled { get; set; }

        public virtual enumjobtype enumjobtype { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<job> jobs { get; set; }
    }
}
