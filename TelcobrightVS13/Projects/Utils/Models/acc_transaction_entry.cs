namespace Utils.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("platinum.acc_transaction_entry")]
    public partial class acc_transaction_entry
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long idTransaction { get; set; }

        public long? idevent { get; set; }

        public long? idjob { get; set; }

        [Key]
        [Column(Order = 1)]
        public DateTime starttime { get; set; }

        [StringLength(30)]
        public string description { get; set; }

        [Key]
        [Column(Order = 2)]
        public sbyte posted { get; set; }

        [StringLength(400)]
        public string jsonDetail { get; set; }
    }
}
