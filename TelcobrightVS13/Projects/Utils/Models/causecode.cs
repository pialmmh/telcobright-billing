namespace Utils.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("platinum.causecode")]
    public partial class causecode
    {
        public int id { get; set; }

        public int idSwitch { get; set; }

        public int CC { get; set; }

        [StringLength(45)]
        public string Description { get; set; }

        public int? CallCompleteIndicator { get; set; }
    }
}
