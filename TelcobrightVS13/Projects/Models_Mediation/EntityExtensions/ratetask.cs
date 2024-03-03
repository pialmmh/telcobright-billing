using System;
using System.Globalization;

namespace MediationModel
{
    public partial class ratetask
    {
        public void AdjustDateTimeToNativeTimeZone(long timeZoneDifference)
        {
            var tempDate = new DateTime();
            if (DateTime.TryParseExact(this.startdate, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out tempDate))
                this.startdate = tempDate.AddSeconds(timeZoneDifference).ToString("yyyy-MM-dd HH:mm:ss");
            if (DateTime.TryParseExact(this.enddate, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out tempDate))
                this.enddate = tempDate.AddSeconds(timeZoneDifference).ToString("yyyy-MM-dd HH:mm:ss");
            if (DateTime.TryParseExact(this.date1, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out tempDate))
                this.date1 = tempDate.AddSeconds(timeZoneDifference).ToString("yyyy-MM-dd HH:mm:ss");
        }
        public string GetExtendedInsertSql()
        {
            ;
            return
                "('"
                + (this.Prefix != null ? this.Prefix.Replace("'", "") : "") + "'" + "," +
                "'" + (this.description != null ? this.description.Replace("'", "") : "") + "'" + "," +
                "'" + (this.rateamount != null ? this.rateamount.Replace("'", "") : "") + "'" + "," +
                "'" + (this.WeekDayStart != null ? this.WeekDayStart.Replace("'", "") : "") + "'" + "," +
                "'" + (this.WeekDayEnd != null ? this.WeekDayEnd.Replace("'", "") : "") + "'" + "," +
                "'" + (this.starttime != null ? this.starttime.Replace("'", "") : "") + "'" + "," +
                "'" + (this.endtime != null ? this.endtime.Replace("'", "") : "") + "'" + "," +
                "'" + (this.Resolution != null ? this.Resolution.Replace("'", "") : "") + "'" + "," +
                "'" + (this.MinDurationSec != null ? this.MinDurationSec.Replace("'", "") : "") + "'" + "," +
                "'" + (this.SurchargeTime != null ? this.SurchargeTime.Replace("'", "") : "") + "'" + "," +
                "'" + (this.SurchargeAmount != null ? this.SurchargeAmount.Replace("'", "") : "") + "'" + "," +
                "'" + (this.idrateplan != null ? this.idrateplan : 0) + "'" + "," +
                "'" + (this.CountryCode != null ? this.CountryCode.Replace("'", "") : "") + "'" + "," +
                "'" + (this.date1 != null ? this.date1.Replace("'", "") : "") + "'" + "," +
                "'" + (this.field1 != null ? this.field1.Replace("'", "") : "") + "'" + "," +
                "'" + (this.field2 != null ? this.field2.Replace("'", "") : "") + "'" + "," +
                "'" + (this.field3 != null ? this.field3.Replace("'", "") : "") + "'" + "," +
                "'" + (this.field4 != null ? this.field4.Replace("'", "") : "") + "'" + "," +
                "'" + (this.field5 != null ? this.field5.Replace("'", "") : "") + "'" + "," +
                "'" + (this.startdate != null ? this.startdate.Replace("'", "") : "") + "'" + "," +
                "'" + (this.enddate != null ? this.enddate.Replace("'", "") : "") + "'" + "," +
                "'" + (this.Inactive != null ? this.Inactive.Replace("'", "") : "") + "'" + "," +
                "'" + (this.RouteDisabled != null ? this.RouteDisabled.Replace("'", "") : "") + "'" + "," +
                "'" + (this.Type != null ? this.Type.Replace("'", "") : "") + "'" + "," +
                "'" + (this.Currency != null ? this.Currency.Replace("'", "") : "") + "'" + "," +
                "'" + (this.OtherAmount1 != null ? this.OtherAmount1.Replace("'", "") : "") + "'" + "," +
                "'" + (this.OtherAmount2 != null ? this.OtherAmount2.Replace("'", "") : "") + "'" + "," +
                "'" + (this.OtherAmount3 != null ? this.OtherAmount3.Replace("'", "") : "") + "'" + "," +
                "'" + (this.OtherAmount4 != null ? this.OtherAmount4.Replace("'", "") : "") + "'" + "," +
                "'" + (this.OtherAmount5 != null ? this.OtherAmount5.Replace("'", "") : "") + "'" + "," +
                "'" + (this.OtherAmount6 != null ? this.OtherAmount6.Replace("'", "") : "") + "'" + "," +
                "'" + (this.OtherAmount7 != null ? this.OtherAmount7.Replace("'", "") : "") + "'" + "," +
                "'" + (this.OtherAmount8 != null ? this.OtherAmount8.Replace("'", "") : "") + "'" + "," +
                "'" + (this.OtherAmount9 != null ? this.OtherAmount9.Replace("'", "") : "") + "'" + "," +
                "'" + (this.OtherAmount10 != null ? this.OtherAmount10.Replace("'", "") : "") + "'" + "," +
                "'" + (this.TimeZoneOffsetSec != null ? this.TimeZoneOffsetSec.Replace("'", "") : "") + "'" + "," +
                "'" + (this.RatePosition != null ? this.RatePosition.Replace("'", "") : "") + "'" + "," +
                "'" + (this.IgwPercentageIn != null ? this.IgwPercentageIn.Replace("'", "") : "") + "'" + "," +
                "'" + (this.ConflictingRateIds != null ? this.ConflictingRateIds.Replace("'", "") : "") + "'" + "," +
                "'" + (this.ChangedByTaskId != null ? this.ChangedByTaskId.Replace("'", "") : "") + "'" + "," +
                "'" + (this.ChangedOn != null ? this.ChangedOn.Replace("'", "") : "") + "'" + "," +
                "'" + (this.Status != null ? this.Status.Replace("'", "") : "") + "'" + "," +
                "'" + (this.idPreviousRate != null ? this.idPreviousRate.Replace("'", "") : "") + "'" + "," +
                "'" + (this.EndPreviousRate != null ? this.EndPreviousRate.Replace("'", "") : "") + "'" + "," +
                "'" + (this.Category != null ? this.Category.Replace("'", "") : "") + "'" + "," +
                "'" + (this.SubCategory != null ? this.SubCategory.Replace("'", "") : "") + "'" + "," +
                "'" + (this.changecommitted != null ? this.changecommitted : 0) + "'" + "," +
                "'" + (this.OverlappingRates != null ? this.OverlappingRates.Replace("'", "") : "") + "'" + "," +
                "'" + (this.ConflictingRates != null ? this.ConflictingRates.Replace("'", "") : "") + "'" + "," +
                "'" + (this.AffectedRates != null ? this.AffectedRates.Replace("'", "") : "") + "'" + "," +
                "'" + (this.PartitionDate != null ? this.PartitionDate.ToString("yyyy-MM-dd HH:mm:ss") : "") + "'" + "," +
                "'" + (this.Comment1 != null ? this.Comment1.Replace("'", "") : "") + "'" + "," +
                "'" + (this.Comment2 != null ? this.Comment2.Replace("'", "") : "") + "'" + "," +
                "'" + (this.ExecutionOrder != null ? this.ExecutionOrder : 0) + "'" + "," +
                "'" + (this.RateAmountRoundupDecimal != null ? this.RateAmountRoundupDecimal : 0) + "'" + ")";

        }
    }
}