namespace Utils.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("platinum.acc_billable")]
    public partial class acc_billable
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long idBillable { get; set; }

        [StringLength(500)]
        public string uniqueBillInfo { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long idevent { get; set; }

        [Key]
        [Column(Order = 2)]
        public DateTime starttime { get; set; }

        public sbyte? AssignedDirection { get; set; }

        [Key]
        [Column(Order = 3)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int servicegroup { get; set; }

        [Key]
        [Column(Order = 4)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int servicefamily { get; set; }

        [Key]
        [Column(Order = 5)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long ProductId { get; set; }

        [Key]
        [Column(Order = 6)]
        [StringLength(10)]
        public string idBilledUom { get; set; }

        [Key]
        [Column(Order = 7)]
        [StringLength(10)]
        public string idQuantityUom { get; set; }

        [Key]
        [Column(Order = 8)]
        public double newBilledAmount { get; set; }

        [Key]
        [Column(Order = 9)]
        public double incrementalBilledAmount { get; set; }

        [Key]
        [Column(Order = 10)]
        public double newQuantity { get; set; }

        [Key]
        [Column(Order = 11)]
        public double incrementalQuantity { get; set; }

        [Key]
        [Column(Order = 12)]
        public double unitPriceOrCharge { get; set; }

        [StringLength(30)]
        public string Prefix { get; set; }

        [Key]
        [Column(Order = 13)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long RateId { get; set; }

        public double? newOtherAmount1 { get; set; }

        public double? incrementalOtherAmount1 { get; set; }

        public double? newOtherAmount2 { get; set; }

        public double? incrementalOtherAmount2 { get; set; }

        [StringLength(400)]
        public string jsonDetail { get; set; }

        [Key]
        [Column(Order = 14)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long debitAccountId { get; set; }

        public int? posted { get; set; }

        public long? changedByJobId { get; set; }
    }
}
