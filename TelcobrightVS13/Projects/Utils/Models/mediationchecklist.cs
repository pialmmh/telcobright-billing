namespace Utils.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("platinum.mediationchecklist")]
    public partial class mediationchecklist
    {
        public int id { get; set; }

        public int CallDirection { get; set; }

        public int ChargingStatus { get; set; }

        public int fieldnumber { get; set; }

        [Column(TypeName = "text")]
        [StringLength(65535)]
        public string Expression { get; set; }

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
