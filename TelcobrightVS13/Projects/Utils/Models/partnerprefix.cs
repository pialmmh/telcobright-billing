namespace Utils.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("platinum.partnerprefix")]
    public partial class partnerprefix
    {
        public int id { get; set; }

        public int idPartner { get; set; }

        public int PrefixType { get; set; }

        [Required]
        [StringLength(20)]
        public string Prefix { get; set; }

        public int? CommonTG { get; set; }
    }
}
