using LibraryExtensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using TelcobrightMediation.Accounting;
using MediationModel;
using MySql.Data.MySqlClient;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Web.ApplicationServices;
using FlexValidation;
using TelcobrightMediation.Cache;
using TelcobrightMediation.Cdr;
using TelcobrightMediation.Config;
using TelcobrightMediation.EntityHelpers;
using TelcobrightMediation.Mediation.Cdr;
using TransactionTuple = System.ValueTuple<int, int, long, int>;
using CdrSummaryTuple = System.ValueTuple<int, int, int, string, string, decimal, decimal, System.ValueTuple<string, string, string, string, string, string, string, System.ValueTuple<string, string, string, string, string, string>>>;

namespace TelcobrightMediation
{
    public enum CdrReProcessingType
    {
        NoneOrNew = 1,
        ErrorProcess = 2,
        ReProcess = 3,
        Unprocess = 4
    }

    public class CdrJobContext
    {
        public CdrJobInputData CdrjobInputData { get; }
        public MediationContext MediationContext => this.CdrjobInputData.MediationContext;
        public AccountingContext AccountingContext { get; }
        public CdrSummaryContext CdrSummaryContext { get; }
        public ne Ne => this.CdrjobInputData.Ne;
        public List<DateTime> DatesInvolved { get; } //set by new cdrs only, which covers old cdr case as well.
        public List<DateTime> HoursInvolved { get; } //set by new cdrs only, which covers old cdr case as well.
        public int SegmentSizeForDbWrite => this.MediationContext.Tbc.CdrSetting.SegmentSizeForDbWrite;
        public job TelcobrightJob => this.CdrjobInputData.TelcobrightJob;
        public PartnerEntities Context => this.CdrjobInputData.Context;
        public DbCommand DbCmd { get; }
        public AutoIncrementManager AutoIncrementManager { get; }

        public CdrJobContext(CdrJobInputData cdrJobInputData, AutoIncrementManager autoIncrementManager,
            List<DateTime> hoursInvolvedInNewCollection) //constructor
        {
            this.CdrjobInputData = cdrJobInputData;
            this.HoursInvolved = hoursInvolvedInNewCollection;
            this.DatesInvolved = this.HoursInvolved.Select(c => c.Date).Distinct().ToList();
            this.DbCmd = ConnectionManager.CreateCommandFromDbContext(this.Context);
            this.DbCmd.CommandType = CommandType.Text;
            this.AutoIncrementManager = autoIncrementManager;
            this.AccountingContext = new AccountingContext(this.CdrjobInputData.Context, 0, this.AutoIncrementManager,
                this.DatesInvolved, this.CdrjobInputData.CdrSetting.SegmentSizeForDbWrite);
            this.CdrSummaryContext = new CdrSummaryContext(this.MediationContext,
                this.AutoIncrementManager, this.Context, this.HoursInvolved);
            this.DatesInvolved.ForEach(
                d =>
                {
                    var rateCache = this.MediationContext.MefServiceFamilyContainer.RateCache;
                    var dateRange = new DateRange(d.Date, d.AddDays(1));
                    if (rateCache.DateRangeWiseRateDic.ContainsKey(dateRange) == false)
                        this.MediationContext.MefServiceFamilyContainer.RateCache
                            .PopulateDicByDay(dateRange, flagLcr: false, useInMemoryTable: true);
                });
        }
    }
}
