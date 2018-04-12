namespace Utils.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("platinum.job")]
    public partial class job
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public job()
        {
            jobsegments = new HashSet<jobsegment>();
        }

        public long id { get; set; }

        public int idjobdefinition { get; set; }

        [Required]
        [StringLength(200)]
        public string JobName { get; set; }

        public int? OwnerServer { get; set; }

        public int idNE { get; set; }

        public int priority { get; set; }

        public long? SerialNumber { get; set; }

        public int Status { get; set; }

        public long? Progress { get; set; }

        public DateTime? CreationTime { get; set; }

        public DateTime? LastExecuted { get; set; }

        public DateTime? CompletionTime { get; set; }

        public int? NoOfRecords { get; set; }

        public double? TotalDuration { get; set; }

        public double? PartialDuration { get; set; }

        public int? StartSequenceNumber { get; set; }

        public int? EndSequenceNumber { get; set; }

        public int? FailedCount { get; set; }

        public int? SuccessfulCount { get; set; }

        public DateTime? MinCallStartTime { get; set; }

        public DateTime? MaxCallStartTime { get; set; }

        [StringLength(1073741823)]
        public string JobParameter { get; set; }

        [StringLength(1073741823)]
        public string OtherDetail { get; set; }

        [Column(TypeName = "text")]
        [StringLength(65535)]
        public string Error { get; set; }

        public virtual enumjobdefinition enumjobdefinition { get; set; }

        public virtual enumjobstatu enumjobstatu { get; set; }

        public virtual ne ne { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<jobsegment> jobsegments { get; set; }
    }
}
