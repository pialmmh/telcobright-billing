namespace Utils.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("platinum.autoincrementcounter")]
    public partial class autoincrementcounter
    {
        [Key]
        [StringLength(200)]
        public string tableName { get; set; }

        public long value { get; set; }
    }
}
