namespace Utils.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("platinum.cdrdiscarded")]
    public partial class cdrdiscarded
    {
        public int? SwitchId { get; set; }

        [Key]
        [Column(TypeName = "ubigint")]
        public decimal idcall { get; set; }

        [Column(TypeName = "ubigint")]
        public decimal? SequenceNumber { get; set; }

        [StringLength(100)]
        public string FileName { get; set; }

        public byte? CallDirection { get; set; }

        [StringLength(100)]
        public string incomingroute { get; set; }

        [StringLength(25)]
        public string OriginatingIP { get; set; }

        [Column(TypeName = "usmallint")]
        public int? OPC { get; set; }

        [Column(TypeName = "usmallint")]
        public int? OriginatingCIC { get; set; }

        [StringLength(100)]
        public string OriginatingCalledNumber { get; set; }

        [StringLength(100)]
        public string TerminatingCalledNumber { get; set; }

        [StringLength(100)]
        public string OriginatingCallingNumber { get; set; }

        [StringLength(100)]
        public string TerminatingCallingNumber { get; set; }

        public byte? CustomerPrePaid { get; set; }

        public double? DurationSec { get; set; }

        public DateTime? EndTime { get; set; }

        public DateTime? ConnectTime { get; set; }

        public DateTime? AnswerTime { get; set; }

        public byte? ChargingStatus { get; set; }

        public double? PDD { get; set; }

        [StringLength(15)]
        public string CountryCode { get; set; }

        [Column(TypeName = "usmallint")]
        public int? MinuteID { get; set; }

        public byte? ReleaseDirection { get; set; }

        [Column(TypeName = "usmallint")]
        public int? ReleaseCauseSystem { get; set; }

        [Column(TypeName = "usmallint")]
        public int? ReleaseCauseEgress { get; set; }

        [StringLength(100)]
        public string outgoingroute { get; set; }

        [StringLength(25)]
        public string TerminatingIP { get; set; }

        [Column(TypeName = "usmallint")]
        public int? DPC { get; set; }

        [Column(TypeName = "usmallint")]
        public int? TerminatingCIC { get; set; }

        public DateTime? StartTime { get; set; }

        public int? CustomerID { get; set; }

        public double? CustomerRate { get; set; }

        public int? SupplierID { get; set; }

        public double? SupplierRate { get; set; }

        [StringLength(20)]
        public string MatchedPrefixY { get; set; }

        public double? USDRateY { get; set; }

        [StringLength(20)]
        public string MatchedPrefixCustomer { get; set; }

        [StringLength(20)]
        public string MatchedPrefixSupplier { get; set; }

        public double? CustomerCost { get; set; }

        public double? SupplierCost { get; set; }

        public double? CostANSIn { get; set; }

        public double? CostICXIn { get; set; }

        public double? CostVATCommissionIn { get; set; }

        public double? IGWRevenueIn { get; set; }

        public double? RevenueANSOut { get; set; }

        public double? RevenueIGWOut { get; set; }

        public double? RevenueICXOut { get; set; }

        public double? RevenueVATCommissionOut { get; set; }

        public double? SubscriberChargeXOut { get; set; }

        public double? CarrierCostYIGWOut { get; set; }

        [StringLength(10)]
        public string ANSPrefixOrig { get; set; }

        public int? AnsIdOrig { get; set; }

        [StringLength(10)]
        public string AnsPrefixTerm { get; set; }

        public int? AnsIdTerm { get; set; }

        public sbyte? ValidFlag { get; set; }

        public sbyte? PartialFlag { get; set; }

        public int? releasecauseingress { get; set; }

        [Column(TypeName = "ubigint")]
        public decimal? CustomerCallNumberANS { get; set; }

        [Column(TypeName = "ubigint")]
        public decimal? SupplierCallNumberANS { get; set; }

        public byte? CalledPartyNOA { get; set; }

        public byte? CallingPartyNOA { get; set; }

        [Column(TypeName = "uint")]
        public long? GrpDayId { get; set; }

        public byte? MonthId { get; set; }

        [Column(TypeName = "usmallint")]
        public int? DayId { get; set; }

        public double? BTRCTermRate { get; set; }

        public int? WeekDayId { get; set; }

        [Column(TypeName = "usmallint")]
        public int? E1Id { get; set; }

        [StringLength(15)]
        public string MediaIP1 { get; set; }

        [StringLength(15)]
        public string MediaIP2 { get; set; }

        [StringLength(15)]
        public string MediaIP3 { get; set; }

        [StringLength(15)]
        public string MediaIP4 { get; set; }

        public double? CallCancelDuration { get; set; }

        [Column(TypeName = "usmallint")]
        public int? E1IdOut { get; set; }

        public int? hourid { get; set; }

        public int? halfhourid { get; set; }

        public sbyte? FifteenMinutesId { get; set; }

        public sbyte? FiveMinutesId { get; set; }

        public sbyte? MediationComplete { get; set; }

        public long? temptransactionid { get; set; }

        public byte? ConnectedNumberType { get; set; }

        [StringLength(30)]
        public string RedirectingNumber { get; set; }

        public byte? CallForwardOrRoamingType { get; set; }

        public DateTime? date1 { get; set; }

        public int? field1 { get; set; }

        public int? field2 { get; set; }

        public int? field3 { get; set; }

        [StringLength(45)]
        public string field4 { get; set; }

        public int? field5 { get; set; }

        public double? roundedduration { get; set; }

        public double? PartialDuration { get; set; }

        public DateTime? PartialAnswerTime { get; set; }

        public DateTime? PartialEndTime { get; set; }

        public long? PartialNextIdCall { get; set; }

        public double? Duration1 { get; set; }

        public double? Duration2 { get; set; }

        public double? Duration3 { get; set; }

        public double? Duration4 { get; set; }

        public int? PreviousPeriodCdr { get; set; }

        [StringLength(100)]
        public string UniqueBillId { get; set; }

        [Column(TypeName = "text")]
        [StringLength(65535)]
        public string BillngInfo { get; set; }

        public int? Category { get; set; }

        public int? SubCategory { get; set; }
    }
}
