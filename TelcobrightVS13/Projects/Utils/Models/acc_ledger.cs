namespace Utils.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("platinum.acc_ledger")]
    public partial class acc_ledger
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long id { get; set; }

        [Key]
        [Column(Order = 1, TypeName = "date")]
        public DateTime starttime { get; set; }

        [Key]
        [Column(Order = 2)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long idTransaction { get; set; }

        [StringLength(30)]
        public string description { get; set; }

        [Key]
        [Column(Order = 3)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ACCTG_TRANS_ENTRY_SEQ_ID { get; set; }

        [Key]
        [Column(Order = 4)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long account_id { get; set; }

        [Key]
        [Column(Order = 5)]
        [StringLength(1)]
        public string debit_Credit_flag { get; set; }

        [Key]
        [Column(Order = 6)]
        public double BalanceBefore { get; set; }

        [Key]
        [Column(Order = 7)]
        public double AMOUNT { get; set; }

        [Key]
        [Column(Order = 8)]
        public double BalanceAfter { get; set; }

        [StringLength(15)]
        public string settlementstatus { get; set; }
    }
}
