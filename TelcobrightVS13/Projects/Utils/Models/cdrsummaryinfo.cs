namespace Utils.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("platinum.cdrsummaryinfo")]
    public partial class cdrsummaryinfo
    {
        public int id { get; set; }

        public int SwitchId { get; set; }

        public int CallDirection { get; set; }

        [Column(TypeName = "text")]
        [StringLength(65535)]
        public string SummaryExpression { get; set; }

        [Required]
        [StringLength(45)]
        public string SummaryInterval { get; set; }

        [Column(TypeName = "text")]
        [StringLength(65535)]
        public string Cumulatives { get; set; }

        [Column(TypeName = "text")]
        [StringLength(65535)]
        public string Incrementals { get; set; }

        public DateTime? date1 { get; set; }

        public int? field1 { get; set; }

        public int? field2 { get; set; }

        public int? field3 { get; set; }

        [StringLength(45)]
        public string field4 { get; set; }

        [StringLength(45)]
        public string field5 { get; set; }

        public int Active { get; set; }
    }
}
