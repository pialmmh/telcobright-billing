namespace Utils.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("platinum.carriercontactmapping")]
    public partial class carriercontactmapping
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int idCarrierContactMapping { get; set; }

        [Required]
        [StringLength(45)]
        public string Name { get; set; }

        [StringLength(45)]
        public string Designation { get; set; }

        [StringLength(45)]
        public string Department { get; set; }

        [StringLength(45)]
        public string OfficePhone { get; set; }

        [StringLength(45)]
        public string Mobile { get; set; }

        [StringLength(45)]
        public string email { get; set; }

        public int idCarrier { get; set; }

        public DateTime? date1 { get; set; }

        public int? field1 { get; set; }

        public int? field2 { get; set; }

        public int? field3 { get; set; }

        [StringLength(45)]
        public string field4 { get; set; }

        [StringLength(45)]
        public string field5 { get; set; }
    }
}
