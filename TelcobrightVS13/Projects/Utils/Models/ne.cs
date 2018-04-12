namespace Utils.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("platinum.ne")]
    public partial class ne
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ne()
        {
            jobs = new HashSet<job>();
        }

        [Key]
        public int idSwitch { get; set; }

        public int idCustomer { get; set; }

        public int idcdrformat { get; set; }

        public int idMediationRule { get; set; }

        [StringLength(100)]
        public string SwitchName { get; set; }

        [StringLength(50)]
        public string CDRPrefix { get; set; }

        [StringLength(45)]
        public string FileExtension { get; set; }

        [StringLength(100)]
        public string Description { get; set; }

        [Column(TypeName = "text")]
        [StringLength(65535)]
        public string SourceFileLocations { get; set; }

        [Column(TypeName = "text")]
        [StringLength(65535)]
        public string BackupFileLocations { get; set; }

        public int? LoadingStopFlag { get; set; }

        public int? LoadingSpanCount { get; set; }

        public int? TransactionSizeForCDRLoading { get; set; }

        public int? DecodingSpanCount { get; set; }

        public int? SkipAutoCreateJob { get; set; }

        public int? SkipCdrListed { get; set; }

        public int? SkipCdrReceived { get; set; }

        public int? SkipCdrDecoded { get; set; }

        public int? SkipCdrBackedup { get; set; }

        public int? KeepDecodedCDR { get; set; }

        public int? KeepReceivedCdrServer { get; set; }

        public int? CcrCauseCodeField { get; set; }

        public int? SwitchTimeZoneId { get; set; }

        [Required]
        [StringLength(45)]
        public string CallConnectIndicator { get; set; }

        public int FieldNoForTimeSummary { get; set; }

        [Required]
        [StringLength(45)]
        public string EnableSummaryGeneration { get; set; }

        public int ExistingSummaryCacheSpanHr { get; set; }

        public int BatchToDecodeRatio { get; set; }

        public int PrependLocationNumberToFileName { get; set; }

        public virtual enumcdrformat enumcdrformat { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<job> jobs { get; set; }

        public virtual mediationrule mediationrule { get; set; }

        public virtual telcobrightpartner telcobrightpartner { get; set; }
    }
}
