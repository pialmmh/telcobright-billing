namespace Utils.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("platinum.account")]
    public partial class account
    {
        public long id { get; set; }

        public long? idParent { get; set; }

        [StringLength(200)]
        public string idParentExternal { get; set; }

        public int idPartner { get; set; }

        [StringLength(200)]
        public string accountName { get; set; }

        [Required]
        [StringLength(100)]
        public string iduom { get; set; }

        public int idaccountingClass { get; set; }

        public int Depth { get; set; }

        [StringLength(400)]
        public string Lineage { get; set; }

        [StringLength(100)]
        public string remark { get; set; }

        public virtual acc_balance acc_balance { get; set; }

        public virtual enumaccountingclass enumaccountingclass { get; set; }
    }
}
