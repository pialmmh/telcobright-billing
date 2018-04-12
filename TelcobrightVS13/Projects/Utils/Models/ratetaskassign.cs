namespace Utils.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("platinum.ratetaskassign")]
    public partial class ratetaskassign
    {
        public long id { get; set; }

        [StringLength(100)]
        public string Prefix { get; set; }

        [StringLength(100)]
        public string description { get; set; }

        [StringLength(100)]
        public string rateamount { get; set; }

        [StringLength(100)]
        public string WeekDayStart { get; set; }

        [StringLength(100)]
        public string WeekDayEnd { get; set; }

        [StringLength(100)]
        public string starttime { get; set; }

        [StringLength(100)]
        public string endtime { get; set; }

        [StringLength(100)]
        public string Resolution { get; set; }

        [StringLength(100)]
        public string MinDurationSec { get; set; }

        [StringLength(100)]
        public string SurchargeTime { get; set; }

        [StringLength(100)]
        public string SurchargeAmount { get; set; }

        public long idrateplan { get; set; }

        [StringLength(100)]
        public string CountryCode { get; set; }

        [StringLength(100)]
        public string date1 { get; set; }

        [StringLength(100)]
        public string field1 { get; set; }

        [StringLength(100)]
        public string field2 { get; set; }

        [StringLength(100)]
        public string field3 { get; set; }

        [StringLength(100)]
        public string field4 { get; set; }

        [StringLength(100)]
        public string field5 { get; set; }

        [StringLength(100)]
        public string startdate { get; set; }

        [StringLength(100)]
        public string enddate { get; set; }

        [StringLength(100)]
        public string Inactive { get; set; }

        [StringLength(100)]
        public string RouteDisabled { get; set; }

        [StringLength(100)]
        public string Type { get; set; }

        [StringLength(100)]
        public string Currency { get; set; }

        [StringLength(100)]
        public string OtherAmount1 { get; set; }

        [StringLength(100)]
        public string OtherAmount2 { get; set; }

        [StringLength(100)]
        public string OtherAmount3 { get; set; }

        [StringLength(100)]
        public string OtherAmount4 { get; set; }

        [StringLength(100)]
        public string OtherAmount5 { get; set; }

        [StringLength(100)]
        public string OtherAmount6 { get; set; }

        [StringLength(100)]
        public string OtherAmount7 { get; set; }

        [StringLength(100)]
        public string OtherAmount8 { get; set; }

        [StringLength(100)]
        public string OtherAmount9 { get; set; }

        [StringLength(100)]
        public string OtherAmount10 { get; set; }

        [StringLength(100)]
        public string TimeZoneOffsetSec { get; set; }

        [StringLength(100)]
        public string RatePosition { get; set; }

        [StringLength(100)]
        public string IgwPercentageIn { get; set; }

        [StringLength(100)]
        public string ConflictingRateIds { get; set; }

        [StringLength(100)]
        public string ChangedByTaskId { get; set; }

        [StringLength(100)]
        public string ChangedOn { get; set; }

        [StringLength(100)]
        public string Status { get; set; }

        [StringLength(100)]
        public string idPreviousRate { get; set; }

        [StringLength(100)]
        public string EndPreviousRate { get; set; }

        [StringLength(100)]
        public string Category { get; set; }

        [StringLength(100)]
        public string SubCategory { get; set; }

        public int changecommitted { get; set; }

        [StringLength(50)]
        public string OverlappingRates { get; set; }

        [StringLength(50)]
        public string ConflictingRates { get; set; }

        [StringLength(50)]
        public string AffectedRates { get; set; }

        public DateTime PartitionDate { get; set; }

        [StringLength(200)]
        public string Comment1 { get; set; }

        [StringLength(200)]
        public string Comment2 { get; set; }
    }
}
