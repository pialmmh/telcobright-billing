namespace Utils.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("platinum.acc_balance")]
    public partial class acc_balance
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long idAccount { get; set; }

        public double BalanceBefore { get; set; }

        public double? lastAmount { get; set; }

        public double BalanceAfter { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime lastUpdated { get; set; }

        public virtual account account { get; set; }
    }
}
