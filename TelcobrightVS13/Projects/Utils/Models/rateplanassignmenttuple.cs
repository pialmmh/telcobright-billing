namespace Utils.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("platinum.rateplanassignmenttuple")]
    public partial class rateplanassignmenttuple
    {
        public long id { get; set; }

        public long idService { get; set; }

        public int AssignDirection { get; set; }

        public int? idpartner { get; set; }

        public int? route { get; set; }

        public int priority { get; set; }
    }
}
