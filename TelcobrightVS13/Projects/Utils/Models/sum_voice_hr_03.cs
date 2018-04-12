namespace Utils.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("platinum.sum_voice_hr_03")]
    public partial class sum_voice_hr_03
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long id { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int tup_switchid { get; set; }

        [Key]
        [Column(Order = 2)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int tup_inpartnerid { get; set; }

        [Key]
        [Column(Order = 3)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int tup_outpartnerid { get; set; }

        [Key]
        [Column(Order = 4)]
        [StringLength(50)]
        public string tup_incomingroute { get; set; }

        [Key]
        [Column(Order = 5)]
        [StringLength(50)]
        public string tup_outgoingroute { get; set; }

        [Key]
        [Column(Order = 6)]
        public double tup_customerrate { get; set; }

        [Key]
        [Column(Order = 7)]
        public double tup_supplierrate { get; set; }

        [Key]
        [Column(Order = 8)]
        [StringLength(50)]
        public string tup_incomingip { get; set; }

        [Key]
        [Column(Order = 9)]
        [StringLength(50)]
        public string tup_outgoingip { get; set; }

        [Key]
        [Column(Order = 10)]
        [StringLength(50)]
        public string tup_countryorareacode { get; set; }

        [Key]
        [Column(Order = 11)]
        [StringLength(50)]
        public string tup_matchedprefixcustomer { get; set; }

        [Key]
        [Column(Order = 12)]
        [StringLength(50)]
        public string tup_matchedprefixsupplier { get; set; }

        [Key]
        [Column(Order = 13)]
        [StringLength(50)]
        public string tup_sourceId { get; set; }

        [Key]
        [Column(Order = 14)]
        [StringLength(50)]
        public string tup_destinationId { get; set; }

        [Key]
        [Column(Order = 15)]
        [StringLength(50)]
        public string tup_customercurrency { get; set; }

        [Key]
        [Column(Order = 16)]
        [StringLength(50)]
        public string tup_suppliercurrency { get; set; }

        [Key]
        [Column(Order = 17)]
        [StringLength(50)]
        public string tup_tax1currency { get; set; }

        [Key]
        [Column(Order = 18)]
        [StringLength(50)]
        public string tup_tax2currency { get; set; }

        [Key]
        [Column(Order = 19)]
        [StringLength(50)]
        public string tup_vatcurrency { get; set; }

        [Key]
        [Column(Order = 20)]
        public DateTime tup_starttime { get; set; }

        public long? totalcalls { get; set; }

        public long? connectedcalls { get; set; }

        public long? connectedcallsCC { get; set; }

        public long? successfulcalls { get; set; }

        public double? actualduration { get; set; }

        public double? roundedduration { get; set; }

        public double? duration1 { get; set; }

        public double? duration2 { get; set; }

        public double? duration3 { get; set; }

        public double? PDD { get; set; }

        public double? customercost { get; set; }

        public double? suppliercost { get; set; }

        public double? tax1 { get; set; }

        public double? tax2 { get; set; }

        public double? vat { get; set; }

        public int? intAmount1 { get; set; }

        public int? intAmount2 { get; set; }

        public int? longAmount1 { get; set; }

        public int? longAmount2 { get; set; }

        public double? doubleAmount1 { get; set; }

        public double? doubleAmount2 { get; set; }
    }
}
