namespace Utils.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("platinum.acc_ledger_summary")]
    public partial class acc_ledger_summary
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long id { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long idAccount { get; set; }

        [Key]
        [Column(Order = 2)]
        public DateTime transactionDate { get; set; }

        [Key]
        [Column(Order = 3)]
        public double AMOUNT { get; set; }
    }
}
