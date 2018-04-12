namespace Utils.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("platinum.cdrfieldmappingbyswitchtype")]
    public partial class cdrfieldmappingbyswitchtype
    {
        public int Id { get; set; }

        public int FieldNumber { get; set; }

        public int idCdrFormat { get; set; }

        public int? FieldPositionInCDRRow { get; set; }

        public float? BinByteOffset { get; set; }

        public float? BinByteLen { get; set; }

        [StringLength(45)]
        public string BinByteType { get; set; }

        public virtual cdrfieldlist cdrfieldlist { get; set; }
    }
}
