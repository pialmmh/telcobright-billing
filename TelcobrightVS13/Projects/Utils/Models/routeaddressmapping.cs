namespace Utils.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("platinum.routeaddressmapping")]
    public partial class routeaddressmapping
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int idRouteAddressMapping { get; set; }

        [Required]
        [StringLength(45)]
        public string IpTdmAddress { get; set; }

        public int NoOfChannels { get; set; }

        public int AddressType { get; set; }

        public int? SignalingProtocol { get; set; }

        public int? SS7NetworkIndicator { get; set; }

        public int? TransportProtocol { get; set; }

        public short? SignalingPort { get; set; }

        [StringLength(45)]
        public string Comment { get; set; }

        public int idRoute { get; set; }

        public int? SwitchVendor { get; set; }

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
