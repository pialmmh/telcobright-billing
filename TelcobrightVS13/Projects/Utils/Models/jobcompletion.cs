namespace Utils.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("platinum.jobcompletion")]
    public partial class jobcompletion
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long idJob { get; set; }
    }
}
