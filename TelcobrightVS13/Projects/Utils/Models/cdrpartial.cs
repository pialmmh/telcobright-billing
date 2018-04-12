namespace Utils.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("platinum.cdrpartial")]
    public partial class cdrpartial
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(100)]
        public string UniqueBillId { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int idswitch { get; set; }

        [Key]
        [Column(Order = 2)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long idcall { get; set; }

        [Key]
        [Column(Order = 3)]
        public DateTime StartTime { get; set; }
    }
}
