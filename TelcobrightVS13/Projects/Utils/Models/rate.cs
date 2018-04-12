namespace Utils.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("platinum.rate")]
    public partial class rate
    {
        public long id { get; set; }

        public int ProductId { get; set; }

        [Required]
        [StringLength(25)]
        public string Prefix { get; set; }

        [StringLength(200)]
        public string description { get; set; }

        public double rateamount { get; set; }

        public int WeekDayStart { get; set; }

        public int WeekDayEnd { get; set; }

        [StringLength(6)]
        public string starttime { get; set; }

        [StringLength(6)]
        public string endtime { get; set; }

        public int Resolution { get; set; }

        public float MinDurationSec { get; set; }

        public int SurchargeTime { get; set; }

        public double SurchargeAmount { get; set; }

        public long? idrateplan { get; set; }

        [StringLength(20)]
        public string CountryCode { get; set; }

        public DateTime? date1 { get; set; }

        public int? field1 { get; set; }

        public int? field2 { get; set; }

        public int field3 { get; set; }

        [StringLength(45)]
        public string field4 { get; set; }

        [StringLength(45)]
        public string field5 { get; set; }

        public DateTime startdate { get; set; }

        public DateTime? enddate { get; set; }

        public int Inactive { get; set; }

        public int RouteDisabled { get; set; }

        public int Type { get; set; }

        public int Currency { get; set; }

        public float? OtherAmount1 { get; set; }

        public float? OtherAmount2 { get; set; }

        public float? OtherAmount3 { get; set; }

        public double? OtherAmount4 { get; set; }

        public double? OtherAmount5 { get; set; }

        public float? OtherAmount6 { get; set; }

        public float? OtherAmount7 { get; set; }

        public float? OtherAmount8 { get; set; }

        public float? OtherAmount9 { get; set; }

        public float? OtherAmount10 { get; set; }

        public double TimeZoneOffsetSec { get; set; }

        public int? RatePosition { get; set; }

        public float? IgwPercentageIn { get; set; }

        [StringLength(300)]
        public string ConflictingRateIds { get; set; }

        public long? ChangedByTaskId { get; set; }

        public DateTime? ChangedOn { get; set; }

        public int? Status { get; set; }

        public long? idPreviousRate { get; set; }

        public sbyte? EndPreviousRate { get; set; }

        public sbyte? Category { get; set; }

        public sbyte? SubCategory { get; set; }

        public int? ChangeCommitted { get; set; }

        [StringLength(50)]
        public string ConflictingRates { get; set; }

        [StringLength(50)]
        public string OverlappingRates { get; set; }

        [StringLength(200)]
        public string Comment1 { get; set; }

        [StringLength(200)]
        public string Comment2 { get; set; }

        public int? billingspan { get; set; }

        public int? RateAmountRoundupDecimal { get; set; }
    }
}
