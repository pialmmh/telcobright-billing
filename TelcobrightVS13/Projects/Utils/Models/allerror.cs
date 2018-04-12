namespace Utils.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("platinum.allerror")]
    public partial class allerror
    {
        public int id { get; set; }

        public long idError { get; set; }

        public long? jobid { get; set; }

        [StringLength(500)]
        public string jobname { get; set; }

        public DateTime TimeRaised { get; set; }

        public DateTime? TimeCleared { get; set; }

        public int Status { get; set; }

        public int? ClearType { get; set; }

        public int? ClearingUser { get; set; }

        [StringLength(1000)]
        public string ExceptionMessage { get; set; }

        [StringLength(100)]
        public string ProcessName { get; set; }

        [StringLength(3000)]
        public string ExceptionDetail { get; set; }
    }
}
