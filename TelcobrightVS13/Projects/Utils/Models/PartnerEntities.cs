namespace Utils.Models
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class PartnerEntities : DbContext
    {
        public PartnerEntities()
            : base("name=PartnerEntities")
        {
        }

        public virtual DbSet<acc_balance> acc_balance { get; set; }
        public virtual DbSet<acc_tmp_credit> acc_tmp_credit { get; set; }
        public virtual DbSet<account> accounts { get; set; }
        public virtual DbSet<allerror> allerrors { get; set; }
        public virtual DbSet<ansprefixextra> ansprefixextras { get; set; }
        public virtual DbSet<aspnetuserclaim> aspnetuserclaims { get; set; }
        public virtual DbSet<autoincrementcounter> autoincrementcounters { get; set; }
        public virtual DbSet<carriercontactmapping> carriercontactmappings { get; set; }
        public virtual DbSet<causecode> causecodes { get; set; }
        public virtual DbSet<cdrdiscarded> cdrdiscardeds { get; set; }
        public virtual DbSet<cdrerror> cdrerrors { get; set; }
        public virtual DbSet<cdrfieldlist> cdrfieldlists { get; set; }
        public virtual DbSet<cdrfieldmappingbyswitchtype> cdrfieldmappingbyswitchtypes { get; set; }
        public virtual DbSet<cdrinconsistent> cdrinconsistents { get; set; }
        public virtual DbSet<cdrsummaryinfo> cdrsummaryinfoes { get; set; }
        public virtual DbSet<commontg> commontgs { get; set; }
        public virtual DbSet<country> countries { get; set; }
        public virtual DbSet<countrycode> countrycodes { get; set; }
        public virtual DbSet<enumaccountingclass> enumaccountingclasses { get; set; }
        public virtual DbSet<enumanstype> enumanstypes { get; set; }
        public virtual DbSet<enumbillingspan> enumbillingspans { get; set; }
        public virtual DbSet<enumbilltype> enumbilltypes { get; set; }
        public virtual DbSet<enumcalldirection> enumcalldirections { get; set; }
        public virtual DbSet<enumcallforwardingroamingtype> enumcallforwardingroamingtypes { get; set; }
        public virtual DbSet<enumcdrformat> enumcdrformats { get; set; }
        public virtual DbSet<enumcreditrule> enumcreditrules { get; set; }
        public virtual DbSet<enumcurrency> enumcurrencies { get; set; }
        public virtual DbSet<enumdatedassignment> enumdatedassignments { get; set; }
        public virtual DbSet<enumdateparsestring> enumdateparsestrings { get; set; }
        public virtual DbSet<enuminvoicetemplate> enuminvoicetemplates { get; set; }
        public virtual DbSet<enumjobautocreatetype> enumjobautocreatetypes { get; set; }
        public virtual DbSet<enumjobdefinition> enumjobdefinitions { get; set; }
        public virtual DbSet<enumjobstatu> enumjobstatus { get; set; }
        public virtual DbSet<enumjobtype> enumjobtypes { get; set; }
        public virtual DbSet<enumnationalorinternationalroute> enumnationalorinternationalroutes { get; set; }
        public virtual DbSet<enumpartnerrule> enumpartnerrules { get; set; }
        public virtual DbSet<enumpartnertype> enumpartnertypes { get; set; }
        public virtual DbSet<enumpostpaidinvoicestatu> enumpostpaidinvoicestatus { get; set; }
        public virtual DbSet<enumprepaidinvoicestatu> enumprepaidinvoicestatus { get; set; }
        public virtual DbSet<enumprepostpaid> enumprepostpaids { get; set; }
        public virtual DbSet<enumrateplanformat> enumrateplanformats { get; set; }
        public virtual DbSet<enumrateplantype> enumrateplantypes { get; set; }
        public virtual DbSet<enumratesheetformat> enumratesheetformats { get; set; }
        public virtual DbSet<enumroutestatu> enumroutestatus { get; set; }
        public virtual DbSet<enumservicecategory> enumservicecategories { get; set; }
        public virtual DbSet<enumservicefamily> enumservicefamilies { get; set; }
        public virtual DbSet<enumservicegroup> enumservicegroups { get; set; }
        public virtual DbSet<enumservicesubcategory> enumservicesubcategories { get; set; }
        public virtual DbSet<enumsignalingprotocol> enumsignalingprotocols { get; set; }
        public virtual DbSet<enumss7networkindicator> enumss7networkindicator { get; set; }
        public virtual DbSet<enumswitchvendor> enumswitchvendors { get; set; }
        public virtual DbSet<enumtaxrule> enumtaxrules { get; set; }
        public virtual DbSet<enumtelcobrightforcarriertype> enumtelcobrightforcarriertypes { get; set; }
        public virtual DbSet<enumtransactiontype> enumtransactiontypes { get; set; }
        public virtual DbSet<enumtransportprotocol> enumtransportprotocols { get; set; }
        public virtual DbSet<enumvatrule> enumvatrules { get; set; }
        public virtual DbSet<errordefinition> errordefinitions { get; set; }
        public virtual DbSet<job> jobs { get; set; }
        public virtual DbSet<jobcompletion> jobcompletions { get; set; }
        public virtual DbSet<jobsegment> jobsegments { get; set; }
        public virtual DbSet<lcr> lcrs { get; set; }
        public virtual DbSet<lcrpoint> lcrpoints { get; set; }
        public virtual DbSet<lcrrateplan> lcrrateplans { get; set; }
        public virtual DbSet<mediationchecklist> mediationchecklists { get; set; }
        public virtual DbSet<mediationrule> mediationrules { get; set; }
        public virtual DbSet<ne> nes { get; set; }
        public virtual DbSet<partner> partners { get; set; }
        public virtual DbSet<partnerprefix> partnerprefixes { get; set; }
        public virtual DbSet<process> processes { get; set; }
        public virtual DbSet<product> products { get; set; }
        public virtual DbSet<qrtz_blob_triggers> qrtz_blob_triggers { get; set; }
        public virtual DbSet<qrtz_calendars> qrtz_calendars { get; set; }
        public virtual DbSet<qrtz_cron_triggers> qrtz_cron_triggers { get; set; }
        public virtual DbSet<qrtz_fired_triggers> qrtz_fired_triggers { get; set; }
        public virtual DbSet<qrtz_job_details> qrtz_job_details { get; set; }
        public virtual DbSet<qrtz_locks> qrtz_locks { get; set; }
        public virtual DbSet<qrtz_paused_trigger_grps> qrtz_paused_trigger_grps { get; set; }
        public virtual DbSet<qrtz_scheduler_state> qrtz_scheduler_state { get; set; }
        public virtual DbSet<qrtz_simple_triggers> qrtz_simple_triggers { get; set; }
        public virtual DbSet<qrtz_simprop_triggers> qrtz_simprop_triggers { get; set; }
        public virtual DbSet<qrtz_triggers> qrtz_triggers { get; set; }
        public virtual DbSet<rate> rates { get; set; }
        public virtual DbSet<rateassign> rateassigns { get; set; }
        public virtual DbSet<rateplan> rateplans { get; set; }
        public virtual DbSet<rateplanassign> rateplanassigns { get; set; }
        public virtual DbSet<rateplanassignmenttuple> rateplanassignmenttuples { get; set; }
        public virtual DbSet<ratetask> ratetasks { get; set; }
        public virtual DbSet<ratetaskassign> ratetaskassigns { get; set; }
        public virtual DbSet<ratetaskassignreference> ratetaskassignreferences { get; set; }
        public virtual DbSet<ratetaskreference> ratetaskreferences { get; set; }
        public virtual DbSet<reporttemplate> reporttemplates { get; set; }
        public virtual DbSet<role> roles { get; set; }
        public virtual DbSet<route> routes { get; set; }
        public virtual DbSet<routeaddressmapping> routeaddressmappings { get; set; }
        public virtual DbSet<telcobrightpartner> telcobrightpartners { get; set; }
        public virtual DbSet<timezone> timezones { get; set; }
        public virtual DbSet<usdexchangerateagainstbdt> usdexchangerateagainstbdts { get; set; }
        public virtual DbSet<usdratetotakabymonth> usdratetotakabymonths { get; set; }
        public virtual DbSet<userclaim> userclaims { get; set; }
        public virtual DbSet<userlogin> userlogins { get; set; }
        public virtual DbSet<userrole> userroles { get; set; }
        public virtual DbSet<user> users { get; set; }
        public virtual DbSet<xyzprefix> xyzprefixes { get; set; }
        public virtual DbSet<xyzprefixset> xyzprefixsets { get; set; }
        public virtual DbSet<xyzselected> xyzselecteds { get; set; }
        public virtual DbSet<zone> zones { get; set; }
        public virtual DbSet<acc_billable> acc_billable { get; set; }
        public virtual DbSet<acc_ledger> acc_ledger { get; set; }
        public virtual DbSet<acc_ledger_summary> acc_ledger_summary { get; set; }
        public virtual DbSet<acc_transaction_entry> acc_transaction_entry { get; set; }
        public virtual DbSet<cdrloaded> cdrloadeds { get; set; }
        public virtual DbSet<cdrpartial> cdrpartials { get; set; }
        public virtual DbSet<cdrsummary> cdrsummaries { get; set; }
        public virtual DbSet<cdrsummarytemp> cdrsummarytemps { get; set; }
        public virtual DbSet<sum_voice_day_01> sum_voice_day_01 { get; set; }
        public virtual DbSet<sum_voice_day_02> sum_voice_day_02 { get; set; }
        public virtual DbSet<sum_voice_day_03> sum_voice_day_03 { get; set; }
        public virtual DbSet<sum_voice_hr_01> sum_voice_hr_01 { get; set; }
        public virtual DbSet<sum_voice_hr_02> sum_voice_hr_02 { get; set; }
        public virtual DbSet<sum_voice_hr_03> sum_voice_hr_03 { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<acc_tmp_credit>()
                .Property(e => e.TargetAccountName)
                .IsUnicode(false);

            modelBuilder.Entity<acc_tmp_credit>()
                .Property(e => e.jsonDetail)
                .IsUnicode(false);

            modelBuilder.Entity<account>()
                .Property(e => e.idParentExternal)
                .IsUnicode(false);

            modelBuilder.Entity<account>()
                .Property(e => e.accountName)
                .IsUnicode(false);

            modelBuilder.Entity<account>()
                .Property(e => e.iduom)
                .IsUnicode(false);

            modelBuilder.Entity<account>()
                .Property(e => e.Lineage)
                .IsUnicode(false);

            modelBuilder.Entity<account>()
                .Property(e => e.remark)
                .IsUnicode(false);

            modelBuilder.Entity<account>()
                .HasOptional(e => e.acc_balance)
                .WithRequired(e => e.account);

            modelBuilder.Entity<allerror>()
                .Property(e => e.jobname)
                .IsUnicode(false);

            modelBuilder.Entity<allerror>()
                .Property(e => e.ExceptionMessage)
                .IsUnicode(false);

            modelBuilder.Entity<allerror>()
                .Property(e => e.ProcessName)
                .IsUnicode(false);

            modelBuilder.Entity<allerror>()
                .Property(e => e.ExceptionDetail)
                .IsUnicode(false);

            modelBuilder.Entity<ansprefixextra>()
                .Property(e => e.PrefixBeforeAnsNumber)
                .IsUnicode(false);

            modelBuilder.Entity<aspnetuserclaim>()
                .Property(e => e.UserId)
                .IsUnicode(false);

            modelBuilder.Entity<aspnetuserclaim>()
                .Property(e => e.ClaimType)
                .IsUnicode(false);

            modelBuilder.Entity<aspnetuserclaim>()
                .Property(e => e.ClaimValue)
                .IsUnicode(false);

            modelBuilder.Entity<autoincrementcounter>()
                .Property(e => e.tableName)
                .IsUnicode(false);

            modelBuilder.Entity<carriercontactmapping>()
                .Property(e => e.Name)
                .IsUnicode(false);

            modelBuilder.Entity<carriercontactmapping>()
                .Property(e => e.Designation)
                .IsUnicode(false);

            modelBuilder.Entity<carriercontactmapping>()
                .Property(e => e.Department)
                .IsUnicode(false);

            modelBuilder.Entity<carriercontactmapping>()
                .Property(e => e.OfficePhone)
                .IsUnicode(false);

            modelBuilder.Entity<carriercontactmapping>()
                .Property(e => e.Mobile)
                .IsUnicode(false);

            modelBuilder.Entity<carriercontactmapping>()
                .Property(e => e.email)
                .IsUnicode(false);

            modelBuilder.Entity<carriercontactmapping>()
                .Property(e => e.field4)
                .IsUnicode(false);

            modelBuilder.Entity<carriercontactmapping>()
                .Property(e => e.field5)
                .IsUnicode(false);

            modelBuilder.Entity<causecode>()
                .Property(e => e.Description)
                .IsUnicode(false);

            modelBuilder.Entity<cdrdiscarded>()
                .Property(e => e.FileName)
                .IsUnicode(false);

            modelBuilder.Entity<cdrdiscarded>()
                .Property(e => e.incomingroute)
                .IsUnicode(false);

            modelBuilder.Entity<cdrdiscarded>()
                .Property(e => e.OriginatingIP)
                .IsUnicode(false);

            modelBuilder.Entity<cdrdiscarded>()
                .Property(e => e.OriginatingCalledNumber)
                .IsUnicode(false);

            modelBuilder.Entity<cdrdiscarded>()
                .Property(e => e.TerminatingCalledNumber)
                .IsUnicode(false);

            modelBuilder.Entity<cdrdiscarded>()
                .Property(e => e.OriginatingCallingNumber)
                .IsUnicode(false);

            modelBuilder.Entity<cdrdiscarded>()
                .Property(e => e.TerminatingCallingNumber)
                .IsUnicode(false);

            modelBuilder.Entity<cdrdiscarded>()
                .Property(e => e.CountryCode)
                .IsUnicode(false);

            modelBuilder.Entity<cdrdiscarded>()
                .Property(e => e.outgoingroute)
                .IsUnicode(false);

            modelBuilder.Entity<cdrdiscarded>()
                .Property(e => e.TerminatingIP)
                .IsUnicode(false);

            modelBuilder.Entity<cdrdiscarded>()
                .Property(e => e.MatchedPrefixY)
                .IsUnicode(false);

            modelBuilder.Entity<cdrdiscarded>()
                .Property(e => e.MatchedPrefixCustomer)
                .IsUnicode(false);

            modelBuilder.Entity<cdrdiscarded>()
                .Property(e => e.MatchedPrefixSupplier)
                .IsUnicode(false);

            modelBuilder.Entity<cdrdiscarded>()
                .Property(e => e.ANSPrefixOrig)
                .IsUnicode(false);

            modelBuilder.Entity<cdrdiscarded>()
                .Property(e => e.AnsPrefixTerm)
                .IsUnicode(false);

            modelBuilder.Entity<cdrdiscarded>()
                .Property(e => e.MediaIP1)
                .IsUnicode(false);

            modelBuilder.Entity<cdrdiscarded>()
                .Property(e => e.MediaIP2)
                .IsUnicode(false);

            modelBuilder.Entity<cdrdiscarded>()
                .Property(e => e.MediaIP3)
                .IsUnicode(false);

            modelBuilder.Entity<cdrdiscarded>()
                .Property(e => e.MediaIP4)
                .IsUnicode(false);

            modelBuilder.Entity<cdrdiscarded>()
                .Property(e => e.RedirectingNumber)
                .IsUnicode(false);

            modelBuilder.Entity<cdrdiscarded>()
                .Property(e => e.field4)
                .IsUnicode(false);

            modelBuilder.Entity<cdrdiscarded>()
                .Property(e => e.UniqueBillId)
                .IsUnicode(false);

            modelBuilder.Entity<cdrdiscarded>()
                .Property(e => e.BillngInfo)
                .IsUnicode(false);

            modelBuilder.Entity<cdrerror>()
                .Property(e => e.FileName)
                .IsUnicode(false);

            modelBuilder.Entity<cdrerror>()
                .Property(e => e.incomingroute)
                .IsUnicode(false);

            modelBuilder.Entity<cdrerror>()
                .Property(e => e.OriginatingIP)
                .IsUnicode(false);

            modelBuilder.Entity<cdrerror>()
                .Property(e => e.OriginatingCalledNumber)
                .IsUnicode(false);

            modelBuilder.Entity<cdrerror>()
                .Property(e => e.TerminatingCalledNumber)
                .IsUnicode(false);

            modelBuilder.Entity<cdrerror>()
                .Property(e => e.OriginatingCallingNumber)
                .IsUnicode(false);

            modelBuilder.Entity<cdrerror>()
                .Property(e => e.TerminatingCallingNumber)
                .IsUnicode(false);

            modelBuilder.Entity<cdrerror>()
                .Property(e => e.CountryCode)
                .IsUnicode(false);

            modelBuilder.Entity<cdrerror>()
                .Property(e => e.outgoingroute)
                .IsUnicode(false);

            modelBuilder.Entity<cdrerror>()
                .Property(e => e.TerminatingIP)
                .IsUnicode(false);

            modelBuilder.Entity<cdrerror>()
                .Property(e => e.MatchedPrefixY)
                .IsUnicode(false);

            modelBuilder.Entity<cdrerror>()
                .Property(e => e.matchedprefixcustomer)
                .IsUnicode(false);

            modelBuilder.Entity<cdrerror>()
                .Property(e => e.matchedprefixsupplier)
                .IsUnicode(false);

            modelBuilder.Entity<cdrerror>()
                .Property(e => e.ANSPrefixOrig)
                .IsUnicode(false);

            modelBuilder.Entity<cdrerror>()
                .Property(e => e.AnsPrefixTerm)
                .IsUnicode(false);

            modelBuilder.Entity<cdrerror>()
                .Property(e => e.MediaIP1)
                .IsUnicode(false);

            modelBuilder.Entity<cdrerror>()
                .Property(e => e.MediaIP2)
                .IsUnicode(false);

            modelBuilder.Entity<cdrerror>()
                .Property(e => e.MediaIP3)
                .IsUnicode(false);

            modelBuilder.Entity<cdrerror>()
                .Property(e => e.MediaIP4)
                .IsUnicode(false);

            modelBuilder.Entity<cdrerror>()
                .Property(e => e.RedirectingNumber)
                .IsUnicode(false);

            modelBuilder.Entity<cdrerror>()
                .Property(e => e.field4)
                .IsUnicode(false);

            modelBuilder.Entity<cdrerror>()
                .Property(e => e.UniqueBillId)
                .IsUnicode(false);

            modelBuilder.Entity<cdrerror>()
                .Property(e => e.BillngInfo)
                .IsUnicode(false);

            modelBuilder.Entity<cdrfieldlist>()
                .Property(e => e.FieldName)
                .IsUnicode(false);

            modelBuilder.Entity<cdrfieldlist>()
                .HasMany(e => e.cdrfieldmappingbyswitchtypes)
                .WithRequired(e => e.cdrfieldlist)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<cdrfieldmappingbyswitchtype>()
                .Property(e => e.BinByteType)
                .IsUnicode(false);

            modelBuilder.Entity<cdrinconsistent>()
                .Property(e => e.SequenceNumber)
                .IsUnicode(false);

            modelBuilder.Entity<cdrinconsistent>()
                .Property(e => e.FileName)
                .IsUnicode(false);

            modelBuilder.Entity<cdrinconsistent>()
                .Property(e => e.incomingroute)
                .IsUnicode(false);

            modelBuilder.Entity<cdrinconsistent>()
                .Property(e => e.OriginatingIP)
                .IsUnicode(false);

            modelBuilder.Entity<cdrinconsistent>()
                .Property(e => e.OPC)
                .IsUnicode(false);

            modelBuilder.Entity<cdrinconsistent>()
                .Property(e => e.OriginatingCIC)
                .IsUnicode(false);

            modelBuilder.Entity<cdrinconsistent>()
                .Property(e => e.OriginatingCalledNumber)
                .IsUnicode(false);

            modelBuilder.Entity<cdrinconsistent>()
                .Property(e => e.TerminatingCalledNumber)
                .IsUnicode(false);

            modelBuilder.Entity<cdrinconsistent>()
                .Property(e => e.OriginatingCallingNumber)
                .IsUnicode(false);

            modelBuilder.Entity<cdrinconsistent>()
                .Property(e => e.TerminatingCallingNumber)
                .IsUnicode(false);

            modelBuilder.Entity<cdrinconsistent>()
                .Property(e => e.DurationSec)
                .IsUnicode(false);

            modelBuilder.Entity<cdrinconsistent>()
                .Property(e => e.EndTime)
                .IsUnicode(false);

            modelBuilder.Entity<cdrinconsistent>()
                .Property(e => e.ConnectTime)
                .IsUnicode(false);

            modelBuilder.Entity<cdrinconsistent>()
                .Property(e => e.AnswerTime)
                .IsUnicode(false);

            modelBuilder.Entity<cdrinconsistent>()
                .Property(e => e.CountryCode)
                .IsUnicode(false);

            modelBuilder.Entity<cdrinconsistent>()
                .Property(e => e.ReleaseCauseSystem)
                .IsUnicode(false);

            modelBuilder.Entity<cdrinconsistent>()
                .Property(e => e.ReleaseCauseEgress)
                .IsUnicode(false);

            modelBuilder.Entity<cdrinconsistent>()
                .Property(e => e.outgoingroute)
                .IsUnicode(false);

            modelBuilder.Entity<cdrinconsistent>()
                .Property(e => e.TerminatingIP)
                .IsUnicode(false);

            modelBuilder.Entity<cdrinconsistent>()
                .Property(e => e.DPC)
                .IsUnicode(false);

            modelBuilder.Entity<cdrinconsistent>()
                .Property(e => e.TerminatingCIC)
                .IsUnicode(false);

            modelBuilder.Entity<cdrinconsistent>()
                .Property(e => e.StartTime)
                .IsUnicode(false);

            modelBuilder.Entity<cdrinconsistent>()
                .Property(e => e.MatchedPrefixY)
                .IsUnicode(false);

            modelBuilder.Entity<cdrinconsistent>()
                .Property(e => e.matchedprefixcustomer)
                .IsUnicode(false);

            modelBuilder.Entity<cdrinconsistent>()
                .Property(e => e.matchedprefixsupplier)
                .IsUnicode(false);

            modelBuilder.Entity<cdrinconsistent>()
                .Property(e => e.ANSPrefixOrig)
                .IsUnicode(false);

            modelBuilder.Entity<cdrinconsistent>()
                .Property(e => e.AnsPrefixTerm)
                .IsUnicode(false);

            modelBuilder.Entity<cdrinconsistent>()
                .Property(e => e.releasecauseingress)
                .IsUnicode(false);

            modelBuilder.Entity<cdrinconsistent>()
                .Property(e => e.MediaIP1)
                .IsUnicode(false);

            modelBuilder.Entity<cdrinconsistent>()
                .Property(e => e.MediaIP2)
                .IsUnicode(false);

            modelBuilder.Entity<cdrinconsistent>()
                .Property(e => e.MediaIP3)
                .IsUnicode(false);

            modelBuilder.Entity<cdrinconsistent>()
                .Property(e => e.mediaip4)
                .IsUnicode(false);

            modelBuilder.Entity<cdrinconsistent>()
                .Property(e => e.RedirectingNumber)
                .IsUnicode(false);

            modelBuilder.Entity<cdrinconsistent>()
                .Property(e => e.field4)
                .IsUnicode(false);

            modelBuilder.Entity<cdrinconsistent>()
                .Property(e => e.UniqueBillId)
                .IsUnicode(false);

            modelBuilder.Entity<cdrinconsistent>()
                .Property(e => e.BillngInfo)
                .IsUnicode(false);

            modelBuilder.Entity<cdrsummaryinfo>()
                .Property(e => e.SummaryExpression)
                .IsUnicode(false);

            modelBuilder.Entity<cdrsummaryinfo>()
                .Property(e => e.SummaryInterval)
                .IsUnicode(false);

            modelBuilder.Entity<cdrsummaryinfo>()
                .Property(e => e.Cumulatives)
                .IsUnicode(false);

            modelBuilder.Entity<cdrsummaryinfo>()
                .Property(e => e.Incrementals)
                .IsUnicode(false);

            modelBuilder.Entity<cdrsummaryinfo>()
                .Property(e => e.field4)
                .IsUnicode(false);

            modelBuilder.Entity<cdrsummaryinfo>()
                .Property(e => e.field5)
                .IsUnicode(false);

            modelBuilder.Entity<commontg>()
                .Property(e => e.TgName)
                .IsUnicode(false);

            modelBuilder.Entity<commontg>()
                .Property(e => e.description)
                .IsUnicode(false);

            modelBuilder.Entity<country>()
                .Property(e => e.country_code)
                .IsUnicode(false);

            modelBuilder.Entity<country>()
                .Property(e => e.country_name)
                .IsUnicode(false);

            modelBuilder.Entity<countrycode>()
                .Property(e => e.Code)
                .IsUnicode(false);

            modelBuilder.Entity<countrycode>()
                .Property(e => e.Name)
                .IsUnicode(false);

            modelBuilder.Entity<enumaccountingclass>()
                .Property(e => e.Name)
                .IsUnicode(false);

            modelBuilder.Entity<enumaccountingclass>()
                .HasMany(e => e.accounts)
                .WithRequired(e => e.enumaccountingclass)
                .HasForeignKey(e => e.idaccountingClass)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<enumanstype>()
                .Property(e => e.Type)
                .IsUnicode(false);

            modelBuilder.Entity<enumbillingspan>()
                .Property(e => e.Type)
                .IsUnicode(false);

            modelBuilder.Entity<enumbillingspan>()
                .Property(e => e.ofbiz_uom_Id)
                .IsUnicode(false);

            modelBuilder.Entity<enumbilltype>()
                .Property(e => e.Type)
                .IsUnicode(false);

            modelBuilder.Entity<enumcalldirection>()
                .Property(e => e.Type)
                .IsUnicode(false);

            modelBuilder.Entity<enumcallforwardingroamingtype>()
                .Property(e => e.Type)
                .IsUnicode(false);

            modelBuilder.Entity<enumcdrformat>()
                .Property(e => e.Type)
                .IsUnicode(false);

            modelBuilder.Entity<enumcdrformat>()
                .Property(e => e.FieldSeparatorTxt)
                .IsUnicode(false);

            modelBuilder.Entity<enumcdrformat>()
                .HasMany(e => e.nes)
                .WithRequired(e => e.enumcdrformat)
                .HasForeignKey(e => e.idcdrformat)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<enumcreditrule>()
                .Property(e => e.RuleName)
                .IsUnicode(false);

            modelBuilder.Entity<enumcreditrule>()
                .Property(e => e.Description)
                .IsUnicode(false);

            modelBuilder.Entity<enumcurrency>()
                .Property(e => e.Type)
                .IsUnicode(false);

            modelBuilder.Entity<enumcurrency>()
                .Property(e => e.Description)
                .IsUnicode(false);

            modelBuilder.Entity<enumcurrency>()
                .Property(e => e.Symbol)
                .IsUnicode(false);

            modelBuilder.Entity<enumdatedassignment>()
                .Property(e => e.Type)
                .IsUnicode(false);

            modelBuilder.Entity<enumdatedassignment>()
                .Property(e => e.Tuple)
                .IsUnicode(false);

            modelBuilder.Entity<enumdatedassignment>()
                .Property(e => e.description)
                .IsUnicode(false);

            modelBuilder.Entity<enumdateparsestring>()
                .Property(e => e.value)
                .IsUnicode(false);

            modelBuilder.Entity<enumdateparsestring>()
                .Property(e => e.ParseString)
                .IsUnicode(false);

            modelBuilder.Entity<enuminvoicetemplate>()
                .Property(e => e.TemplateName)
                .IsUnicode(false);

            modelBuilder.Entity<enuminvoicetemplate>()
                .Property(e => e.Description)
                .IsUnicode(false);

            modelBuilder.Entity<enuminvoicetemplate>()
                .Property(e => e.OtherInfo)
                .IsUnicode(false);

            modelBuilder.Entity<enumjobautocreatetype>()
                .Property(e => e.Type)
                .IsUnicode(false);

            modelBuilder.Entity<enumjobautocreatetype>()
                .Property(e => e.Description)
                .IsUnicode(false);

            modelBuilder.Entity<enumjobdefinition>()
                .Property(e => e.Type)
                .IsUnicode(false);

            modelBuilder.Entity<enumjobdefinition>()
                .HasMany(e => e.jobs)
                .WithRequired(e => e.enumjobdefinition)
                .HasForeignKey(e => e.idjobdefinition)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<enumjobstatu>()
                .Property(e => e.Type)
                .IsUnicode(false);

            modelBuilder.Entity<enumjobstatu>()
                .HasMany(e => e.jobs)
                .WithRequired(e => e.enumjobstatu)
                .HasForeignKey(e => e.Status)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<enumjobtype>()
                .Property(e => e.Type)
                .IsUnicode(false);

            modelBuilder.Entity<enumjobtype>()
                .HasMany(e => e.enumjobdefinitions)
                .WithRequired(e => e.enumjobtype)
                .HasForeignKey(e => e.jobtypeid)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<enumnationalorinternationalroute>()
                .Property(e => e.Type)
                .IsUnicode(false);

            modelBuilder.Entity<enumpartnerrule>()
                .Property(e => e.Type)
                .IsUnicode(false);

            modelBuilder.Entity<enumpartnertype>()
                .Property(e => e.Type)
                .IsUnicode(false);

            modelBuilder.Entity<enumpostpaidinvoicestatu>()
                .Property(e => e.Type)
                .IsUnicode(false);

            modelBuilder.Entity<enumprepaidinvoicestatu>()
                .Property(e => e.Type)
                .IsUnicode(false);

            modelBuilder.Entity<enumprepostpaid>()
                .Property(e => e.Type)
                .IsUnicode(false);

            modelBuilder.Entity<enumrateplanformat>()
                .Property(e => e.Type)
                .IsUnicode(false);

            modelBuilder.Entity<enumrateplanformat>()
                .Property(e => e.Description)
                .IsUnicode(false);

            modelBuilder.Entity<enumrateplantype>()
                .Property(e => e.Type)
                .IsUnicode(false);

            modelBuilder.Entity<enumratesheetformat>()
                .Property(e => e.Type)
                .IsUnicode(false);

            modelBuilder.Entity<enumratesheetformat>()
                .Property(e => e.IdentifierTextJson)
                .IsUnicode(false);

            modelBuilder.Entity<enumroutestatu>()
                .Property(e => e.Type)
                .IsUnicode(false);

            modelBuilder.Entity<enumservicecategory>()
                .Property(e => e.Type)
                .IsUnicode(false);

            modelBuilder.Entity<enumservicecategory>()
                .HasMany(e => e.enumservicefamilies)
                .WithRequired(e => e.enumservicecategory)
                .HasForeignKey(e => e.ServiceCategory)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<enumservicefamily>()
                .Property(e => e.ServiceName)
                .IsUnicode(false);

            modelBuilder.Entity<enumservicefamily>()
                .Property(e => e.Description)
                .IsUnicode(false);

            modelBuilder.Entity<enumservicegroup>()
                .Property(e => e.Type)
                .IsUnicode(false);

            modelBuilder.Entity<enumservicesubcategory>()
                .Property(e => e.Type)
                .IsUnicode(false);

            modelBuilder.Entity<enumsignalingprotocol>()
                .Property(e => e.Type)
                .IsUnicode(false);

            modelBuilder.Entity<enumss7networkindicator>()
                .Property(e => e.Type)
                .IsUnicode(false);

            modelBuilder.Entity<enumswitchvendor>()
                .Property(e => e.Type)
                .IsUnicode(false);

            modelBuilder.Entity<enumtaxrule>()
                .Property(e => e.Type)
                .IsUnicode(false);

            modelBuilder.Entity<enumtaxrule>()
                .Property(e => e.description)
                .IsUnicode(false);

            modelBuilder.Entity<enumtelcobrightforcarriertype>()
                .Property(e => e.Type)
                .IsUnicode(false);

            modelBuilder.Entity<enumtransactiontype>()
                .Property(e => e.Type)
                .IsUnicode(false);

            modelBuilder.Entity<enumtransportprotocol>()
                .Property(e => e.Type)
                .IsUnicode(false);

            modelBuilder.Entity<enumvatrule>()
                .Property(e => e.Type)
                .IsUnicode(false);

            modelBuilder.Entity<enumvatrule>()
                .Property(e => e.description)
                .IsUnicode(false);

            modelBuilder.Entity<errordefinition>()
                .Property(e => e.Description)
                .IsUnicode(false);

            modelBuilder.Entity<errordefinition>()
                .Property(e => e.Action)
                .IsUnicode(false);

            modelBuilder.Entity<job>()
                .Property(e => e.JobName)
                .IsUnicode(false);

            modelBuilder.Entity<job>()
                .Property(e => e.JobParameter)
                .IsUnicode(false);

            modelBuilder.Entity<job>()
                .Property(e => e.OtherDetail)
                .IsUnicode(false);

            modelBuilder.Entity<job>()
                .Property(e => e.Error)
                .IsUnicode(false);

            modelBuilder.Entity<job>()
                .HasMany(e => e.jobsegments)
                .WithRequired(e => e.job)
                .HasForeignKey(e => e.idJob)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<jobsegment>()
                .Property(e => e.SegmentDetail)
                .IsUnicode(false);

            modelBuilder.Entity<lcr>()
                .Property(e => e.Prefix)
                .IsUnicode(false);

            modelBuilder.Entity<lcr>()
                .Property(e => e.LcrCurrent)
                .IsUnicode(false);

            modelBuilder.Entity<lcr>()
                .Property(e => e.LcrHistory)
                .IsUnicode(false);

            modelBuilder.Entity<lcrpoint>()
                .Property(e => e.RateChangeType)
                .IsUnicode(false);

            modelBuilder.Entity<lcrpoint>()
                .Property(e => e.prefix)
                .IsUnicode(false);

            modelBuilder.Entity<lcrrateplan>()
                .Property(e => e.Description)
                .IsUnicode(false);

            modelBuilder.Entity<mediationchecklist>()
                .Property(e => e.Expression)
                .IsUnicode(false);

            modelBuilder.Entity<mediationchecklist>()
                .Property(e => e.field4)
                .IsUnicode(false);

            modelBuilder.Entity<mediationchecklist>()
                .Property(e => e.field5)
                .IsUnicode(false);

            modelBuilder.Entity<mediationrule>()
                .Property(e => e.Description)
                .IsUnicode(false);

            modelBuilder.Entity<mediationrule>()
                .Property(e => e.medrulesjson)
                .IsUnicode(false);

            modelBuilder.Entity<mediationrule>()
                .HasMany(e => e.nes)
                .WithRequired(e => e.mediationrule)
                .HasForeignKey(e => e.idMediationRule)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ne>()
                .Property(e => e.SwitchName)
                .IsUnicode(false);

            modelBuilder.Entity<ne>()
                .Property(e => e.CDRPrefix)
                .IsUnicode(false);

            modelBuilder.Entity<ne>()
                .Property(e => e.FileExtension)
                .IsUnicode(false);

            modelBuilder.Entity<ne>()
                .Property(e => e.Description)
                .IsUnicode(false);

            modelBuilder.Entity<ne>()
                .Property(e => e.SourceFileLocations)
                .IsUnicode(false);

            modelBuilder.Entity<ne>()
                .Property(e => e.BackupFileLocations)
                .IsUnicode(false);

            modelBuilder.Entity<ne>()
                .Property(e => e.CallConnectIndicator)
                .IsUnicode(false);

            modelBuilder.Entity<ne>()
                .Property(e => e.EnableSummaryGeneration)
                .IsUnicode(false);

            modelBuilder.Entity<ne>()
                .HasMany(e => e.jobs)
                .WithRequired(e => e.ne)
                .HasForeignKey(e => e.idNE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<partner>()
                .Property(e => e.PartnerName)
                .IsUnicode(false);

            modelBuilder.Entity<partner>()
                .Property(e => e.Address1)
                .IsUnicode(false);

            modelBuilder.Entity<partner>()
                .Property(e => e.Address2)
                .IsUnicode(false);

            modelBuilder.Entity<partner>()
                .Property(e => e.City)
                .IsUnicode(false);

            modelBuilder.Entity<partner>()
                .Property(e => e.State)
                .IsUnicode(false);

            modelBuilder.Entity<partner>()
                .Property(e => e.PostalCode)
                .IsUnicode(false);

            modelBuilder.Entity<partner>()
                .Property(e => e.Country)
                .IsUnicode(false);

            modelBuilder.Entity<partner>()
                .Property(e => e.Telephone)
                .IsUnicode(false);

            modelBuilder.Entity<partner>()
                .Property(e => e.email)
                .IsUnicode(false);

            modelBuilder.Entity<partner>()
                .Property(e => e.field4)
                .IsUnicode(false);

            modelBuilder.Entity<partner>()
                .Property(e => e.field5)
                .IsUnicode(false);

            modelBuilder.Entity<partner>()
                .HasMany(e => e.routes)
                .WithRequired(e => e.partner)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<partnerprefix>()
                .Property(e => e.Prefix)
                .IsUnicode(false);

            modelBuilder.Entity<process>()
                .Property(e => e.ProcessName)
                .IsUnicode(false);

            modelBuilder.Entity<process>()
                .Property(e => e.ProcessParamaterJson)
                .IsUnicode(false);

            modelBuilder.Entity<product>()
                .Property(e => e.Prefix)
                .IsUnicode(false);

            modelBuilder.Entity<product>()
                .Property(e => e.Name)
                .IsUnicode(false);

            modelBuilder.Entity<product>()
                .Property(e => e.description)
                .IsUnicode(false);

            modelBuilder.Entity<qrtz_blob_triggers>()
                .Property(e => e.SCHED_NAME)
                .IsUnicode(false);

            modelBuilder.Entity<qrtz_blob_triggers>()
                .Property(e => e.TRIGGER_NAME)
                .IsUnicode(false);

            modelBuilder.Entity<qrtz_blob_triggers>()
                .Property(e => e.TRIGGER_GROUP)
                .IsUnicode(false);

            modelBuilder.Entity<qrtz_calendars>()
                .Property(e => e.SCHED_NAME)
                .IsUnicode(false);

            modelBuilder.Entity<qrtz_calendars>()
                .Property(e => e.CALENDAR_NAME)
                .IsUnicode(false);

            modelBuilder.Entity<qrtz_cron_triggers>()
                .Property(e => e.SCHED_NAME)
                .IsUnicode(false);

            modelBuilder.Entity<qrtz_cron_triggers>()
                .Property(e => e.TRIGGER_NAME)
                .IsUnicode(false);

            modelBuilder.Entity<qrtz_cron_triggers>()
                .Property(e => e.TRIGGER_GROUP)
                .IsUnicode(false);

            modelBuilder.Entity<qrtz_cron_triggers>()
                .Property(e => e.CRON_EXPRESSION)
                .IsUnicode(false);

            modelBuilder.Entity<qrtz_cron_triggers>()
                .Property(e => e.TIME_ZONE_ID)
                .IsUnicode(false);

            modelBuilder.Entity<qrtz_fired_triggers>()
                .Property(e => e.SCHED_NAME)
                .IsUnicode(false);

            modelBuilder.Entity<qrtz_fired_triggers>()
                .Property(e => e.ENTRY_ID)
                .IsUnicode(false);

            modelBuilder.Entity<qrtz_fired_triggers>()
                .Property(e => e.TRIGGER_NAME)
                .IsUnicode(false);

            modelBuilder.Entity<qrtz_fired_triggers>()
                .Property(e => e.TRIGGER_GROUP)
                .IsUnicode(false);

            modelBuilder.Entity<qrtz_fired_triggers>()
                .Property(e => e.INSTANCE_NAME)
                .IsUnicode(false);

            modelBuilder.Entity<qrtz_fired_triggers>()
                .Property(e => e.STATE)
                .IsUnicode(false);

            modelBuilder.Entity<qrtz_fired_triggers>()
                .Property(e => e.JOB_NAME)
                .IsUnicode(false);

            modelBuilder.Entity<qrtz_fired_triggers>()
                .Property(e => e.JOB_GROUP)
                .IsUnicode(false);

            modelBuilder.Entity<qrtz_job_details>()
                .Property(e => e.SCHED_NAME)
                .IsUnicode(false);

            modelBuilder.Entity<qrtz_job_details>()
                .Property(e => e.JOB_NAME)
                .IsUnicode(false);

            modelBuilder.Entity<qrtz_job_details>()
                .Property(e => e.JOB_GROUP)
                .IsUnicode(false);

            modelBuilder.Entity<qrtz_job_details>()
                .Property(e => e.DESCRIPTION)
                .IsUnicode(false);

            modelBuilder.Entity<qrtz_job_details>()
                .Property(e => e.JOB_CLASS_NAME)
                .IsUnicode(false);

            modelBuilder.Entity<qrtz_job_details>()
                .HasMany(e => e.qrtz_triggers)
                .WithRequired(e => e.qrtz_job_details)
                .HasForeignKey(e => new { e.SCHED_NAME, e.JOB_NAME, e.JOB_GROUP })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<qrtz_locks>()
                .Property(e => e.SCHED_NAME)
                .IsUnicode(false);

            modelBuilder.Entity<qrtz_locks>()
                .Property(e => e.LOCK_NAME)
                .IsUnicode(false);

            modelBuilder.Entity<qrtz_paused_trigger_grps>()
                .Property(e => e.SCHED_NAME)
                .IsUnicode(false);

            modelBuilder.Entity<qrtz_paused_trigger_grps>()
                .Property(e => e.TRIGGER_GROUP)
                .IsUnicode(false);

            modelBuilder.Entity<qrtz_scheduler_state>()
                .Property(e => e.SCHED_NAME)
                .IsUnicode(false);

            modelBuilder.Entity<qrtz_scheduler_state>()
                .Property(e => e.INSTANCE_NAME)
                .IsUnicode(false);

            modelBuilder.Entity<qrtz_simple_triggers>()
                .Property(e => e.SCHED_NAME)
                .IsUnicode(false);

            modelBuilder.Entity<qrtz_simple_triggers>()
                .Property(e => e.TRIGGER_NAME)
                .IsUnicode(false);

            modelBuilder.Entity<qrtz_simple_triggers>()
                .Property(e => e.TRIGGER_GROUP)
                .IsUnicode(false);

            modelBuilder.Entity<qrtz_simprop_triggers>()
                .Property(e => e.SCHED_NAME)
                .IsUnicode(false);

            modelBuilder.Entity<qrtz_simprop_triggers>()
                .Property(e => e.TRIGGER_NAME)
                .IsUnicode(false);

            modelBuilder.Entity<qrtz_simprop_triggers>()
                .Property(e => e.TRIGGER_GROUP)
                .IsUnicode(false);

            modelBuilder.Entity<qrtz_simprop_triggers>()
                .Property(e => e.STR_PROP_1)
                .IsUnicode(false);

            modelBuilder.Entity<qrtz_simprop_triggers>()
                .Property(e => e.STR_PROP_2)
                .IsUnicode(false);

            modelBuilder.Entity<qrtz_simprop_triggers>()
                .Property(e => e.STR_PROP_3)
                .IsUnicode(false);

            modelBuilder.Entity<qrtz_triggers>()
                .Property(e => e.SCHED_NAME)
                .IsUnicode(false);

            modelBuilder.Entity<qrtz_triggers>()
                .Property(e => e.TRIGGER_NAME)
                .IsUnicode(false);

            modelBuilder.Entity<qrtz_triggers>()
                .Property(e => e.TRIGGER_GROUP)
                .IsUnicode(false);

            modelBuilder.Entity<qrtz_triggers>()
                .Property(e => e.JOB_NAME)
                .IsUnicode(false);

            modelBuilder.Entity<qrtz_triggers>()
                .Property(e => e.JOB_GROUP)
                .IsUnicode(false);

            modelBuilder.Entity<qrtz_triggers>()
                .Property(e => e.DESCRIPTION)
                .IsUnicode(false);

            modelBuilder.Entity<qrtz_triggers>()
                .Property(e => e.TRIGGER_STATE)
                .IsUnicode(false);

            modelBuilder.Entity<qrtz_triggers>()
                .Property(e => e.TRIGGER_TYPE)
                .IsUnicode(false);

            modelBuilder.Entity<qrtz_triggers>()
                .Property(e => e.CALENDAR_NAME)
                .IsUnicode(false);

            modelBuilder.Entity<qrtz_triggers>()
                .HasOptional(e => e.qrtz_blob_triggers)
                .WithRequired(e => e.qrtz_triggers);

            modelBuilder.Entity<qrtz_triggers>()
                .HasOptional(e => e.qrtz_cron_triggers)
                .WithRequired(e => e.qrtz_triggers);

            modelBuilder.Entity<qrtz_triggers>()
                .HasOptional(e => e.qrtz_simple_triggers)
                .WithRequired(e => e.qrtz_triggers);

            modelBuilder.Entity<qrtz_triggers>()
                .HasOptional(e => e.qrtz_simprop_triggers)
                .WithRequired(e => e.qrtz_triggers);

            modelBuilder.Entity<rate>()
                .Property(e => e.Prefix)
                .IsUnicode(false);

            modelBuilder.Entity<rate>()
                .Property(e => e.description)
                .IsUnicode(false);

            modelBuilder.Entity<rate>()
                .Property(e => e.starttime)
                .IsUnicode(false);

            modelBuilder.Entity<rate>()
                .Property(e => e.endtime)
                .IsUnicode(false);

            modelBuilder.Entity<rate>()
                .Property(e => e.CountryCode)
                .IsUnicode(false);

            modelBuilder.Entity<rate>()
                .Property(e => e.field4)
                .IsUnicode(false);

            modelBuilder.Entity<rate>()
                .Property(e => e.field5)
                .IsUnicode(false);

            modelBuilder.Entity<rate>()
                .Property(e => e.ConflictingRateIds)
                .IsUnicode(false);

            modelBuilder.Entity<rate>()
                .Property(e => e.ConflictingRates)
                .IsUnicode(false);

            modelBuilder.Entity<rate>()
                .Property(e => e.OverlappingRates)
                .IsUnicode(false);

            modelBuilder.Entity<rate>()
                .Property(e => e.Comment1)
                .IsUnicode(false);

            modelBuilder.Entity<rate>()
                .Property(e => e.Comment2)
                .IsUnicode(false);

            modelBuilder.Entity<rateassign>()
                .Property(e => e.Prefix)
                .IsUnicode(false);

            modelBuilder.Entity<rateassign>()
                .Property(e => e.description)
                .IsUnicode(false);

            modelBuilder.Entity<rateassign>()
                .Property(e => e.starttime)
                .IsUnicode(false);

            modelBuilder.Entity<rateassign>()
                .Property(e => e.endtime)
                .IsUnicode(false);

            modelBuilder.Entity<rateassign>()
                .Property(e => e.CountryCode)
                .IsUnicode(false);

            modelBuilder.Entity<rateassign>()
                .Property(e => e.field4)
                .IsUnicode(false);

            modelBuilder.Entity<rateassign>()
                .Property(e => e.field5)
                .IsUnicode(false);

            modelBuilder.Entity<rateassign>()
                .Property(e => e.ConflictingRateIds)
                .IsUnicode(false);

            modelBuilder.Entity<rateassign>()
                .Property(e => e.ConflictingRates)
                .IsUnicode(false);

            modelBuilder.Entity<rateassign>()
                .Property(e => e.OverlappingRates)
                .IsUnicode(false);

            modelBuilder.Entity<rateassign>()
                .Property(e => e.Comment1)
                .IsUnicode(false);

            modelBuilder.Entity<rateassign>()
                .Property(e => e.Comment2)
                .IsUnicode(false);

            modelBuilder.Entity<rateplan>()
                .Property(e => e.RatePlanName)
                .IsUnicode(false);

            modelBuilder.Entity<rateplan>()
                .Property(e => e.Description)
                .IsUnicode(false);

            modelBuilder.Entity<rateplan>()
                .Property(e => e.field4)
                .IsUnicode(false);

            modelBuilder.Entity<rateplan>()
                .Property(e => e.field5)
                .IsUnicode(false);

            modelBuilder.Entity<rateplan>()
                .Property(e => e.Currency)
                .IsUnicode(false);

            modelBuilder.Entity<rateplan>()
                .Property(e => e.BillingSpan)
                .IsUnicode(false);

            modelBuilder.Entity<rateplan>()
                .HasMany(e => e.ratetaskreferences)
                .WithOptional(e => e.rateplan)
                .HasForeignKey(e => e.idRatePlan);

            modelBuilder.Entity<rateplanassign>()
                .Property(e => e.Description)
                .IsUnicode(false);

            modelBuilder.Entity<rateplanassign>()
                .Property(e => e.field4)
                .IsUnicode(false);

            modelBuilder.Entity<rateplanassign>()
                .Property(e => e.field5)
                .IsUnicode(false);

            modelBuilder.Entity<rateplanassign>()
                .Property(e => e.RatePlanName)
                .IsUnicode(false);

            modelBuilder.Entity<ratetask>()
                .Property(e => e.Prefix)
                .IsUnicode(false);

            modelBuilder.Entity<ratetask>()
                .Property(e => e.description)
                .IsUnicode(false);

            modelBuilder.Entity<ratetask>()
                .Property(e => e.rateamount)
                .IsUnicode(false);

            modelBuilder.Entity<ratetask>()
                .Property(e => e.WeekDayStart)
                .IsUnicode(false);

            modelBuilder.Entity<ratetask>()
                .Property(e => e.WeekDayEnd)
                .IsUnicode(false);

            modelBuilder.Entity<ratetask>()
                .Property(e => e.starttime)
                .IsUnicode(false);

            modelBuilder.Entity<ratetask>()
                .Property(e => e.endtime)
                .IsUnicode(false);

            modelBuilder.Entity<ratetask>()
                .Property(e => e.Resolution)
                .IsUnicode(false);

            modelBuilder.Entity<ratetask>()
                .Property(e => e.MinDurationSec)
                .IsUnicode(false);

            modelBuilder.Entity<ratetask>()
                .Property(e => e.SurchargeTime)
                .IsUnicode(false);

            modelBuilder.Entity<ratetask>()
                .Property(e => e.SurchargeAmount)
                .IsUnicode(false);

            modelBuilder.Entity<ratetask>()
                .Property(e => e.CountryCode)
                .IsUnicode(false);

            modelBuilder.Entity<ratetask>()
                .Property(e => e.date1)
                .IsUnicode(false);

            modelBuilder.Entity<ratetask>()
                .Property(e => e.field1)
                .IsUnicode(false);

            modelBuilder.Entity<ratetask>()
                .Property(e => e.field2)
                .IsUnicode(false);

            modelBuilder.Entity<ratetask>()
                .Property(e => e.field3)
                .IsUnicode(false);

            modelBuilder.Entity<ratetask>()
                .Property(e => e.field4)
                .IsUnicode(false);

            modelBuilder.Entity<ratetask>()
                .Property(e => e.field5)
                .IsUnicode(false);

            modelBuilder.Entity<ratetask>()
                .Property(e => e.startdate)
                .IsUnicode(false);

            modelBuilder.Entity<ratetask>()
                .Property(e => e.enddate)
                .IsUnicode(false);

            modelBuilder.Entity<ratetask>()
                .Property(e => e.Inactive)
                .IsUnicode(false);

            modelBuilder.Entity<ratetask>()
                .Property(e => e.RouteDisabled)
                .IsUnicode(false);

            modelBuilder.Entity<ratetask>()
                .Property(e => e.Type)
                .IsUnicode(false);

            modelBuilder.Entity<ratetask>()
                .Property(e => e.Currency)
                .IsUnicode(false);

            modelBuilder.Entity<ratetask>()
                .Property(e => e.OtherAmount1)
                .IsUnicode(false);

            modelBuilder.Entity<ratetask>()
                .Property(e => e.OtherAmount2)
                .IsUnicode(false);

            modelBuilder.Entity<ratetask>()
                .Property(e => e.OtherAmount3)
                .IsUnicode(false);

            modelBuilder.Entity<ratetask>()
                .Property(e => e.OtherAmount4)
                .IsUnicode(false);

            modelBuilder.Entity<ratetask>()
                .Property(e => e.OtherAmount5)
                .IsUnicode(false);

            modelBuilder.Entity<ratetask>()
                .Property(e => e.OtherAmount6)
                .IsUnicode(false);

            modelBuilder.Entity<ratetask>()
                .Property(e => e.OtherAmount7)
                .IsUnicode(false);

            modelBuilder.Entity<ratetask>()
                .Property(e => e.OtherAmount8)
                .IsUnicode(false);

            modelBuilder.Entity<ratetask>()
                .Property(e => e.OtherAmount9)
                .IsUnicode(false);

            modelBuilder.Entity<ratetask>()
                .Property(e => e.OtherAmount10)
                .IsUnicode(false);

            modelBuilder.Entity<ratetask>()
                .Property(e => e.TimeZoneOffsetSec)
                .IsUnicode(false);

            modelBuilder.Entity<ratetask>()
                .Property(e => e.RatePosition)
                .IsUnicode(false);

            modelBuilder.Entity<ratetask>()
                .Property(e => e.IgwPercentageIn)
                .IsUnicode(false);

            modelBuilder.Entity<ratetask>()
                .Property(e => e.ConflictingRateIds)
                .IsUnicode(false);

            modelBuilder.Entity<ratetask>()
                .Property(e => e.ChangedByTaskId)
                .IsUnicode(false);

            modelBuilder.Entity<ratetask>()
                .Property(e => e.ChangedOn)
                .IsUnicode(false);

            modelBuilder.Entity<ratetask>()
                .Property(e => e.Status)
                .IsUnicode(false);

            modelBuilder.Entity<ratetask>()
                .Property(e => e.idPreviousRate)
                .IsUnicode(false);

            modelBuilder.Entity<ratetask>()
                .Property(e => e.EndPreviousRate)
                .IsUnicode(false);

            modelBuilder.Entity<ratetask>()
                .Property(e => e.Category)
                .IsUnicode(false);

            modelBuilder.Entity<ratetask>()
                .Property(e => e.SubCategory)
                .IsUnicode(false);

            modelBuilder.Entity<ratetask>()
                .Property(e => e.OverlappingRates)
                .IsUnicode(false);

            modelBuilder.Entity<ratetask>()
                .Property(e => e.ConflictingRates)
                .IsUnicode(false);

            modelBuilder.Entity<ratetask>()
                .Property(e => e.AffectedRates)
                .IsUnicode(false);

            modelBuilder.Entity<ratetask>()
                .Property(e => e.Comment1)
                .IsUnicode(false);

            modelBuilder.Entity<ratetask>()
                .Property(e => e.Comment2)
                .IsUnicode(false);

            modelBuilder.Entity<ratetaskassign>()
                .Property(e => e.Prefix)
                .IsUnicode(false);

            modelBuilder.Entity<ratetaskassign>()
                .Property(e => e.description)
                .IsUnicode(false);

            modelBuilder.Entity<ratetaskassign>()
                .Property(e => e.rateamount)
                .IsUnicode(false);

            modelBuilder.Entity<ratetaskassign>()
                .Property(e => e.WeekDayStart)
                .IsUnicode(false);

            modelBuilder.Entity<ratetaskassign>()
                .Property(e => e.WeekDayEnd)
                .IsUnicode(false);

            modelBuilder.Entity<ratetaskassign>()
                .Property(e => e.starttime)
                .IsUnicode(false);

            modelBuilder.Entity<ratetaskassign>()
                .Property(e => e.endtime)
                .IsUnicode(false);

            modelBuilder.Entity<ratetaskassign>()
                .Property(e => e.Resolution)
                .IsUnicode(false);

            modelBuilder.Entity<ratetaskassign>()
                .Property(e => e.MinDurationSec)
                .IsUnicode(false);

            modelBuilder.Entity<ratetaskassign>()
                .Property(e => e.SurchargeTime)
                .IsUnicode(false);

            modelBuilder.Entity<ratetaskassign>()
                .Property(e => e.SurchargeAmount)
                .IsUnicode(false);

            modelBuilder.Entity<ratetaskassign>()
                .Property(e => e.CountryCode)
                .IsUnicode(false);

            modelBuilder.Entity<ratetaskassign>()
                .Property(e => e.date1)
                .IsUnicode(false);

            modelBuilder.Entity<ratetaskassign>()
                .Property(e => e.field1)
                .IsUnicode(false);

            modelBuilder.Entity<ratetaskassign>()
                .Property(e => e.field2)
                .IsUnicode(false);

            modelBuilder.Entity<ratetaskassign>()
                .Property(e => e.field3)
                .IsUnicode(false);

            modelBuilder.Entity<ratetaskassign>()
                .Property(e => e.field4)
                .IsUnicode(false);

            modelBuilder.Entity<ratetaskassign>()
                .Property(e => e.field5)
                .IsUnicode(false);

            modelBuilder.Entity<ratetaskassign>()
                .Property(e => e.startdate)
                .IsUnicode(false);

            modelBuilder.Entity<ratetaskassign>()
                .Property(e => e.enddate)
                .IsUnicode(false);

            modelBuilder.Entity<ratetaskassign>()
                .Property(e => e.Inactive)
                .IsUnicode(false);

            modelBuilder.Entity<ratetaskassign>()
                .Property(e => e.RouteDisabled)
                .IsUnicode(false);

            modelBuilder.Entity<ratetaskassign>()
                .Property(e => e.Type)
                .IsUnicode(false);

            modelBuilder.Entity<ratetaskassign>()
                .Property(e => e.Currency)
                .IsUnicode(false);

            modelBuilder.Entity<ratetaskassign>()
                .Property(e => e.OtherAmount1)
                .IsUnicode(false);

            modelBuilder.Entity<ratetaskassign>()
                .Property(e => e.OtherAmount2)
                .IsUnicode(false);

            modelBuilder.Entity<ratetaskassign>()
                .Property(e => e.OtherAmount3)
                .IsUnicode(false);

            modelBuilder.Entity<ratetaskassign>()
                .Property(e => e.OtherAmount4)
                .IsUnicode(false);

            modelBuilder.Entity<ratetaskassign>()
                .Property(e => e.OtherAmount5)
                .IsUnicode(false);

            modelBuilder.Entity<ratetaskassign>()
                .Property(e => e.OtherAmount6)
                .IsUnicode(false);

            modelBuilder.Entity<ratetaskassign>()
                .Property(e => e.OtherAmount7)
                .IsUnicode(false);

            modelBuilder.Entity<ratetaskassign>()
                .Property(e => e.OtherAmount8)
                .IsUnicode(false);

            modelBuilder.Entity<ratetaskassign>()
                .Property(e => e.OtherAmount9)
                .IsUnicode(false);

            modelBuilder.Entity<ratetaskassign>()
                .Property(e => e.OtherAmount10)
                .IsUnicode(false);

            modelBuilder.Entity<ratetaskassign>()
                .Property(e => e.TimeZoneOffsetSec)
                .IsUnicode(false);

            modelBuilder.Entity<ratetaskassign>()
                .Property(e => e.RatePosition)
                .IsUnicode(false);

            modelBuilder.Entity<ratetaskassign>()
                .Property(e => e.IgwPercentageIn)
                .IsUnicode(false);

            modelBuilder.Entity<ratetaskassign>()
                .Property(e => e.ConflictingRateIds)
                .IsUnicode(false);

            modelBuilder.Entity<ratetaskassign>()
                .Property(e => e.ChangedByTaskId)
                .IsUnicode(false);

            modelBuilder.Entity<ratetaskassign>()
                .Property(e => e.ChangedOn)
                .IsUnicode(false);

            modelBuilder.Entity<ratetaskassign>()
                .Property(e => e.Status)
                .IsUnicode(false);

            modelBuilder.Entity<ratetaskassign>()
                .Property(e => e.idPreviousRate)
                .IsUnicode(false);

            modelBuilder.Entity<ratetaskassign>()
                .Property(e => e.EndPreviousRate)
                .IsUnicode(false);

            modelBuilder.Entity<ratetaskassign>()
                .Property(e => e.Category)
                .IsUnicode(false);

            modelBuilder.Entity<ratetaskassign>()
                .Property(e => e.SubCategory)
                .IsUnicode(false);

            modelBuilder.Entity<ratetaskassign>()
                .Property(e => e.OverlappingRates)
                .IsUnicode(false);

            modelBuilder.Entity<ratetaskassign>()
                .Property(e => e.ConflictingRates)
                .IsUnicode(false);

            modelBuilder.Entity<ratetaskassign>()
                .Property(e => e.AffectedRates)
                .IsUnicode(false);

            modelBuilder.Entity<ratetaskassign>()
                .Property(e => e.Comment1)
                .IsUnicode(false);

            modelBuilder.Entity<ratetaskassign>()
                .Property(e => e.Comment2)
                .IsUnicode(false);

            modelBuilder.Entity<ratetaskassignreference>()
                .Property(e => e.Description)
                .IsUnicode(false);

            modelBuilder.Entity<ratetaskreference>()
                .Property(e => e.Description)
                .IsUnicode(false);

            modelBuilder.Entity<reporttemplate>()
                .Property(e => e.Templatename)
                .IsUnicode(false);

            modelBuilder.Entity<reporttemplate>()
                .Property(e => e.PageUrl)
                .IsUnicode(false);

            modelBuilder.Entity<reporttemplate>()
                .Property(e => e.ControlValues)
                .IsUnicode(false);

            modelBuilder.Entity<role>()
                .Property(e => e.Id)
                .IsUnicode(false);

            modelBuilder.Entity<role>()
                .Property(e => e.Name)
                .IsUnicode(false);

            modelBuilder.Entity<role>()
                .HasMany(e => e.userroles)
                .WithRequired(e => e.role)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<route>()
                .Property(e => e.RouteName)
                .IsUnicode(false);

            modelBuilder.Entity<route>()
                .Property(e => e.Description)
                .IsUnicode(false);

            modelBuilder.Entity<route>()
                .Property(e => e.field5)
                .IsUnicode(false);

            modelBuilder.Entity<routeaddressmapping>()
                .Property(e => e.IpTdmAddress)
                .IsUnicode(false);

            modelBuilder.Entity<routeaddressmapping>()
                .Property(e => e.Comment)
                .IsUnicode(false);

            modelBuilder.Entity<routeaddressmapping>()
                .Property(e => e.field4)
                .IsUnicode(false);

            modelBuilder.Entity<routeaddressmapping>()
                .Property(e => e.field5)
                .IsUnicode(false);

            modelBuilder.Entity<telcobrightpartner>()
                .Property(e => e.CustomerName)
                .IsUnicode(false);

            modelBuilder.Entity<telcobrightpartner>()
                .Property(e => e.databasename)
                .IsUnicode(false);

            modelBuilder.Entity<telcobrightpartner>()
                .Property(e => e.databasetype)
                .IsUnicode(false);

            modelBuilder.Entity<telcobrightpartner>()
                .Property(e => e.user)
                .IsUnicode(false);

            modelBuilder.Entity<telcobrightpartner>()
                .Property(e => e.pass)
                .IsUnicode(false);

            modelBuilder.Entity<telcobrightpartner>()
                .Property(e => e.ServerNameOrIP)
                .IsUnicode(false);

            modelBuilder.Entity<telcobrightpartner>()
                .Property(e => e.IBServerNameOrIP)
                .IsUnicode(false);

            modelBuilder.Entity<telcobrightpartner>()
                .Property(e => e.IBdatabasename)
                .IsUnicode(false);

            modelBuilder.Entity<telcobrightpartner>()
                .Property(e => e.IBdatabasetype)
                .IsUnicode(false);

            modelBuilder.Entity<telcobrightpartner>()
                .Property(e => e.IBuser)
                .IsUnicode(false);

            modelBuilder.Entity<telcobrightpartner>()
                .Property(e => e.IBpass)
                .IsUnicode(false);

            modelBuilder.Entity<telcobrightpartner>()
                .Property(e => e.IgwPrefix)
                .IsUnicode(false);

            modelBuilder.Entity<telcobrightpartner>()
                .HasMany(e => e.nes)
                .WithRequired(e => e.telcobrightpartner)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<timezone>()
                .Property(e => e.abbreviation)
                .IsUnicode(false);

            modelBuilder.Entity<timezone>()
                .Property(e => e.dst)
                .IsUnicode(false);

            modelBuilder.Entity<timezone>()
                .Property(e => e.offsetdesc)
                .IsUnicode(false);

            modelBuilder.Entity<usdexchangerateagainstbdt>()
                .Property(e => e.Comment)
                .IsUnicode(false);

            modelBuilder.Entity<usdexchangerateagainstbdt>()
                .Property(e => e.field4)
                .IsUnicode(false);

            modelBuilder.Entity<usdexchangerateagainstbdt>()
                .Property(e => e.field5)
                .IsUnicode(false);

            modelBuilder.Entity<usdratetotakabymonth>()
                .Property(e => e.field4)
                .IsUnicode(false);

            modelBuilder.Entity<usdratetotakabymonth>()
                .Property(e => e.field5)
                .IsUnicode(false);

            modelBuilder.Entity<userclaim>()
                .Property(e => e.UserId)
                .IsUnicode(false);

            modelBuilder.Entity<userclaim>()
                .Property(e => e.ClaimType)
                .IsUnicode(false);

            modelBuilder.Entity<userclaim>()
                .Property(e => e.ClaimValue)
                .IsUnicode(false);

            modelBuilder.Entity<userlogin>()
                .Property(e => e.LoginProvider)
                .IsUnicode(false);

            modelBuilder.Entity<userlogin>()
                .Property(e => e.ProviderKey)
                .IsUnicode(false);

            modelBuilder.Entity<userlogin>()
                .Property(e => e.UserId)
                .IsUnicode(false);

            modelBuilder.Entity<userrole>()
                .Property(e => e.UserId)
                .IsUnicode(false);

            modelBuilder.Entity<userrole>()
                .Property(e => e.RoleId)
                .IsUnicode(false);

            modelBuilder.Entity<user>()
                .Property(e => e.Id)
                .IsUnicode(false);

            modelBuilder.Entity<user>()
                .Property(e => e.Email)
                .IsUnicode(false);

            modelBuilder.Entity<user>()
                .Property(e => e.PasswordHash)
                .IsUnicode(false);

            modelBuilder.Entity<user>()
                .Property(e => e.SecurityStamp)
                .IsUnicode(false);

            modelBuilder.Entity<user>()
                .Property(e => e.PhoneNumber)
                .IsUnicode(false);

            modelBuilder.Entity<user>()
                .Property(e => e.UserName)
                .IsUnicode(false);

            modelBuilder.Entity<user>()
                .HasMany(e => e.userroles)
                .WithRequired(e => e.user)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<xyzprefix>()
                .Property(e => e.Prefix)
                .IsUnicode(false);

            modelBuilder.Entity<xyzprefix>()
                .Property(e => e.Description)
                .IsUnicode(false);

            modelBuilder.Entity<xyzprefix>()
                .Property(e => e.CountryCode)
                .IsUnicode(false);

            modelBuilder.Entity<xyzprefix>()
                .Property(e => e.field4)
                .IsUnicode(false);

            modelBuilder.Entity<xyzprefix>()
                .Property(e => e.field5)
                .IsUnicode(false);

            modelBuilder.Entity<xyzprefixset>()
                .Property(e => e.Name)
                .IsUnicode(false);

            modelBuilder.Entity<xyzprefixset>()
                .Property(e => e.Description)
                .IsUnicode(false);

            modelBuilder.Entity<xyzselected>()
                .Property(e => e.prefix)
                .IsUnicode(false);

            modelBuilder.Entity<zone>()
                .Property(e => e.country_code)
                .IsUnicode(false);

            modelBuilder.Entity<zone>()
                .Property(e => e.zone_name)
                .IsUnicode(false);

            modelBuilder.Entity<zone>()
                .HasMany(e => e.timezones)
                .WithRequired(e => e.zone)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<acc_billable>()
                .Property(e => e.uniqueBillInfo)
                .IsUnicode(false);

            modelBuilder.Entity<acc_billable>()
                .Property(e => e.idBilledUom)
                .IsUnicode(false);

            modelBuilder.Entity<acc_billable>()
                .Property(e => e.idQuantityUom)
                .IsUnicode(false);

            modelBuilder.Entity<acc_billable>()
                .Property(e => e.Prefix)
                .IsUnicode(false);

            modelBuilder.Entity<acc_billable>()
                .Property(e => e.jsonDetail)
                .IsUnicode(false);

            modelBuilder.Entity<acc_ledger>()
                .Property(e => e.description)
                .IsUnicode(false);

            modelBuilder.Entity<acc_ledger>()
                .Property(e => e.debit_Credit_flag)
                .IsUnicode(false);

            modelBuilder.Entity<acc_ledger>()
                .Property(e => e.settlementstatus)
                .IsUnicode(false);

            modelBuilder.Entity<acc_transaction_entry>()
                .Property(e => e.description)
                .IsUnicode(false);

            modelBuilder.Entity<acc_transaction_entry>()
                .Property(e => e.jsonDetail)
                .IsUnicode(false);

            modelBuilder.Entity<cdrloaded>()
                .Property(e => e.FileName)
                .IsUnicode(false);

            modelBuilder.Entity<cdrloaded>()
                .Property(e => e.incomingroute)
                .IsUnicode(false);

            modelBuilder.Entity<cdrloaded>()
                .Property(e => e.OriginatingIP)
                .IsUnicode(false);

            modelBuilder.Entity<cdrloaded>()
                .Property(e => e.OriginatingCalledNumber)
                .IsUnicode(false);

            modelBuilder.Entity<cdrloaded>()
                .Property(e => e.TerminatingCalledNumber)
                .IsUnicode(false);

            modelBuilder.Entity<cdrloaded>()
                .Property(e => e.OriginatingCallingNumber)
                .IsUnicode(false);

            modelBuilder.Entity<cdrloaded>()
                .Property(e => e.TerminatingCallingNumber)
                .IsUnicode(false);

            modelBuilder.Entity<cdrloaded>()
                .Property(e => e.CountryCode)
                .IsUnicode(false);

            modelBuilder.Entity<cdrloaded>()
                .Property(e => e.outgoingroute)
                .IsUnicode(false);

            modelBuilder.Entity<cdrloaded>()
                .Property(e => e.TerminatingIP)
                .IsUnicode(false);

            modelBuilder.Entity<cdrloaded>()
                .Property(e => e.MatchedPrefixY)
                .IsUnicode(false);

            modelBuilder.Entity<cdrloaded>()
                .Property(e => e.matchedprefixcustomer)
                .IsUnicode(false);

            modelBuilder.Entity<cdrloaded>()
                .Property(e => e.matchedprefixsupplier)
                .IsUnicode(false);

            modelBuilder.Entity<cdrloaded>()
                .Property(e => e.ANSPrefixOrig)
                .IsUnicode(false);

            modelBuilder.Entity<cdrloaded>()
                .Property(e => e.AnsPrefixTerm)
                .IsUnicode(false);

            modelBuilder.Entity<cdrloaded>()
                .Property(e => e.MediaIP1)
                .IsUnicode(false);

            modelBuilder.Entity<cdrloaded>()
                .Property(e => e.MediaIP2)
                .IsUnicode(false);

            modelBuilder.Entity<cdrloaded>()
                .Property(e => e.MediaIP3)
                .IsUnicode(false);

            modelBuilder.Entity<cdrloaded>()
                .Property(e => e.MediaIP4)
                .IsUnicode(false);

            modelBuilder.Entity<cdrloaded>()
                .Property(e => e.RedirectingNumber)
                .IsUnicode(false);

            modelBuilder.Entity<cdrloaded>()
                .Property(e => e.field4)
                .IsUnicode(false);

            modelBuilder.Entity<cdrloaded>()
                .Property(e => e.UniqueBillId)
                .IsUnicode(false);

            modelBuilder.Entity<cdrloaded>()
                .Property(e => e.BillngInfo)
                .IsUnicode(false);

            modelBuilder.Entity<cdrpartial>()
                .Property(e => e.UniqueBillId)
                .IsUnicode(false);

            modelBuilder.Entity<cdrsummary>()
                .Property(e => e.FileName)
                .IsUnicode(false);

            modelBuilder.Entity<cdrsummary>()
                .Property(e => e.IncomingRoute)
                .IsUnicode(false);

            modelBuilder.Entity<cdrsummary>()
                .Property(e => e.OriginatingIP)
                .IsUnicode(false);

            modelBuilder.Entity<cdrsummary>()
                .Property(e => e.OriginatingCalledNumber)
                .IsUnicode(false);

            modelBuilder.Entity<cdrsummary>()
                .Property(e => e.TerminatingCalledNumber)
                .IsUnicode(false);

            modelBuilder.Entity<cdrsummary>()
                .Property(e => e.OriginatingCallingNumber)
                .IsUnicode(false);

            modelBuilder.Entity<cdrsummary>()
                .Property(e => e.TerminatingCallingNumber)
                .IsUnicode(false);

            modelBuilder.Entity<cdrsummary>()
                .Property(e => e.CountryCode)
                .IsUnicode(false);

            modelBuilder.Entity<cdrsummary>()
                .Property(e => e.OutgoingRoute)
                .IsUnicode(false);

            modelBuilder.Entity<cdrsummary>()
                .Property(e => e.TerminatingIP)
                .IsUnicode(false);

            modelBuilder.Entity<cdrsummary>()
                .Property(e => e.MatchedPrefixY)
                .IsUnicode(false);

            modelBuilder.Entity<cdrsummary>()
                .Property(e => e.MatchedPrefixCustomer)
                .IsUnicode(false);

            modelBuilder.Entity<cdrsummary>()
                .Property(e => e.MatchedPrefixSupplier)
                .IsUnicode(false);

            modelBuilder.Entity<cdrsummary>()
                .Property(e => e.ANSPrefixOrig)
                .IsUnicode(false);

            modelBuilder.Entity<cdrsummary>()
                .Property(e => e.AnsPrefixTerm)
                .IsUnicode(false);

            modelBuilder.Entity<cdrsummary>()
                .Property(e => e.MediaIP1)
                .IsUnicode(false);

            modelBuilder.Entity<cdrsummary>()
                .Property(e => e.MediaIP2)
                .IsUnicode(false);

            modelBuilder.Entity<cdrsummary>()
                .Property(e => e.MediaIP3)
                .IsUnicode(false);

            modelBuilder.Entity<cdrsummary>()
                .Property(e => e.MediaIP4)
                .IsUnicode(false);

            modelBuilder.Entity<cdrsummary>()
                .Property(e => e.RedirectingNumber)
                .IsUnicode(false);

            modelBuilder.Entity<cdrsummary>()
                .Property(e => e.field4)
                .IsUnicode(false);

            modelBuilder.Entity<cdrsummary>()
                .Property(e => e.UniqueBillId)
                .IsUnicode(false);

            modelBuilder.Entity<cdrsummary>()
                .Property(e => e.BillingInfo)
                .IsUnicode(false);

            modelBuilder.Entity<cdrsummarytemp>()
                .Property(e => e.FileName)
                .IsUnicode(false);

            modelBuilder.Entity<cdrsummarytemp>()
                .Property(e => e.IncomingRoute)
                .IsUnicode(false);

            modelBuilder.Entity<cdrsummarytemp>()
                .Property(e => e.OriginatingIP)
                .IsUnicode(false);

            modelBuilder.Entity<cdrsummarytemp>()
                .Property(e => e.OriginatingCalledNumber)
                .IsUnicode(false);

            modelBuilder.Entity<cdrsummarytemp>()
                .Property(e => e.TerminatingCalledNumber)
                .IsUnicode(false);

            modelBuilder.Entity<cdrsummarytemp>()
                .Property(e => e.OriginatingCallingNumber)
                .IsUnicode(false);

            modelBuilder.Entity<cdrsummarytemp>()
                .Property(e => e.TerminatingCallingNumber)
                .IsUnicode(false);

            modelBuilder.Entity<cdrsummarytemp>()
                .Property(e => e.CountryCode)
                .IsUnicode(false);

            modelBuilder.Entity<cdrsummarytemp>()
                .Property(e => e.OutgoingRoute)
                .IsUnicode(false);

            modelBuilder.Entity<cdrsummarytemp>()
                .Property(e => e.TerminatingIP)
                .IsUnicode(false);

            modelBuilder.Entity<cdrsummarytemp>()
                .Property(e => e.MatchedPrefixY)
                .IsUnicode(false);

            modelBuilder.Entity<cdrsummarytemp>()
                .Property(e => e.MatchedPrefixCustomer)
                .IsUnicode(false);

            modelBuilder.Entity<cdrsummarytemp>()
                .Property(e => e.MatchedPrefixSupplier)
                .IsUnicode(false);

            modelBuilder.Entity<cdrsummarytemp>()
                .Property(e => e.ANSPrefixOrig)
                .IsUnicode(false);

            modelBuilder.Entity<cdrsummarytemp>()
                .Property(e => e.AnsPrefixTerm)
                .IsUnicode(false);

            modelBuilder.Entity<cdrsummarytemp>()
                .Property(e => e.MediaIP1)
                .IsUnicode(false);

            modelBuilder.Entity<cdrsummarytemp>()
                .Property(e => e.MediaIP2)
                .IsUnicode(false);

            modelBuilder.Entity<cdrsummarytemp>()
                .Property(e => e.MediaIP3)
                .IsUnicode(false);

            modelBuilder.Entity<cdrsummarytemp>()
                .Property(e => e.MediaIP4)
                .IsUnicode(false);

            modelBuilder.Entity<cdrsummarytemp>()
                .Property(e => e.RedirectingNumber)
                .IsUnicode(false);

            modelBuilder.Entity<cdrsummarytemp>()
                .Property(e => e.field4)
                .IsUnicode(false);

            modelBuilder.Entity<cdrsummarytemp>()
                .Property(e => e.UniqueBillId)
                .IsUnicode(false);

            modelBuilder.Entity<cdrsummarytemp>()
                .Property(e => e.BillingInfo)
                .IsUnicode(false);

            modelBuilder.Entity<sum_voice_day_01>()
                .Property(e => e.tup_incomingroute)
                .IsUnicode(false);

            modelBuilder.Entity<sum_voice_day_01>()
                .Property(e => e.tup_outgoingroute)
                .IsUnicode(false);

            modelBuilder.Entity<sum_voice_day_01>()
                .Property(e => e.tup_incomingip)
                .IsUnicode(false);

            modelBuilder.Entity<sum_voice_day_01>()
                .Property(e => e.tup_outgoingip)
                .IsUnicode(false);

            modelBuilder.Entity<sum_voice_day_01>()
                .Property(e => e.tup_countryorareacode)
                .IsUnicode(false);

            modelBuilder.Entity<sum_voice_day_01>()
                .Property(e => e.tup_matchedprefixcustomer)
                .IsUnicode(false);

            modelBuilder.Entity<sum_voice_day_01>()
                .Property(e => e.tup_matchedprefixsupplier)
                .IsUnicode(false);

            modelBuilder.Entity<sum_voice_day_01>()
                .Property(e => e.tup_sourceId)
                .IsUnicode(false);

            modelBuilder.Entity<sum_voice_day_01>()
                .Property(e => e.tup_destinationId)
                .IsUnicode(false);

            modelBuilder.Entity<sum_voice_day_01>()
                .Property(e => e.tup_customercurrency)
                .IsUnicode(false);

            modelBuilder.Entity<sum_voice_day_01>()
                .Property(e => e.tup_suppliercurrency)
                .IsUnicode(false);

            modelBuilder.Entity<sum_voice_day_01>()
                .Property(e => e.tup_tax1currency)
                .IsUnicode(false);

            modelBuilder.Entity<sum_voice_day_01>()
                .Property(e => e.tup_tax2currency)
                .IsUnicode(false);

            modelBuilder.Entity<sum_voice_day_01>()
                .Property(e => e.tup_vatcurrency)
                .IsUnicode(false);

            modelBuilder.Entity<sum_voice_day_02>()
                .Property(e => e.tup_incomingroute)
                .IsUnicode(false);

            modelBuilder.Entity<sum_voice_day_02>()
                .Property(e => e.tup_outgoingroute)
                .IsUnicode(false);

            modelBuilder.Entity<sum_voice_day_02>()
                .Property(e => e.tup_incomingip)
                .IsUnicode(false);

            modelBuilder.Entity<sum_voice_day_02>()
                .Property(e => e.tup_outgoingip)
                .IsUnicode(false);

            modelBuilder.Entity<sum_voice_day_02>()
                .Property(e => e.tup_countryorareacode)
                .IsUnicode(false);

            modelBuilder.Entity<sum_voice_day_02>()
                .Property(e => e.tup_matchedprefixcustomer)
                .IsUnicode(false);

            modelBuilder.Entity<sum_voice_day_02>()
                .Property(e => e.tup_matchedprefixsupplier)
                .IsUnicode(false);

            modelBuilder.Entity<sum_voice_day_02>()
                .Property(e => e.tup_sourceId)
                .IsUnicode(false);

            modelBuilder.Entity<sum_voice_day_02>()
                .Property(e => e.tup_destinationId)
                .IsUnicode(false);

            modelBuilder.Entity<sum_voice_day_02>()
                .Property(e => e.tup_customercurrency)
                .IsUnicode(false);

            modelBuilder.Entity<sum_voice_day_02>()
                .Property(e => e.tup_suppliercurrency)
                .IsUnicode(false);

            modelBuilder.Entity<sum_voice_day_02>()
                .Property(e => e.tup_tax1currency)
                .IsUnicode(false);

            modelBuilder.Entity<sum_voice_day_02>()
                .Property(e => e.tup_tax2currency)
                .IsUnicode(false);

            modelBuilder.Entity<sum_voice_day_02>()
                .Property(e => e.tup_vatcurrency)
                .IsUnicode(false);

            modelBuilder.Entity<sum_voice_day_03>()
                .Property(e => e.tup_incomingroute)
                .IsUnicode(false);

            modelBuilder.Entity<sum_voice_day_03>()
                .Property(e => e.tup_outgoingroute)
                .IsUnicode(false);

            modelBuilder.Entity<sum_voice_day_03>()
                .Property(e => e.tup_incomingip)
                .IsUnicode(false);

            modelBuilder.Entity<sum_voice_day_03>()
                .Property(e => e.tup_outgoingip)
                .IsUnicode(false);

            modelBuilder.Entity<sum_voice_day_03>()
                .Property(e => e.tup_countryorareacode)
                .IsUnicode(false);

            modelBuilder.Entity<sum_voice_day_03>()
                .Property(e => e.tup_matchedprefixcustomer)
                .IsUnicode(false);

            modelBuilder.Entity<sum_voice_day_03>()
                .Property(e => e.tup_matchedprefixsupplier)
                .IsUnicode(false);

            modelBuilder.Entity<sum_voice_day_03>()
                .Property(e => e.tup_sourceId)
                .IsUnicode(false);

            modelBuilder.Entity<sum_voice_day_03>()
                .Property(e => e.tup_destinationId)
                .IsUnicode(false);

            modelBuilder.Entity<sum_voice_day_03>()
                .Property(e => e.tup_customercurrency)
                .IsUnicode(false);

            modelBuilder.Entity<sum_voice_day_03>()
                .Property(e => e.tup_suppliercurrency)
                .IsUnicode(false);

            modelBuilder.Entity<sum_voice_day_03>()
                .Property(e => e.tup_tax1currency)
                .IsUnicode(false);

            modelBuilder.Entity<sum_voice_day_03>()
                .Property(e => e.tup_tax2currency)
                .IsUnicode(false);

            modelBuilder.Entity<sum_voice_day_03>()
                .Property(e => e.tup_vatcurrency)
                .IsUnicode(false);

            modelBuilder.Entity<sum_voice_hr_01>()
                .Property(e => e.tup_incomingroute)
                .IsUnicode(false);

            modelBuilder.Entity<sum_voice_hr_01>()
                .Property(e => e.tup_outgoingroute)
                .IsUnicode(false);

            modelBuilder.Entity<sum_voice_hr_01>()
                .Property(e => e.tup_incomingip)
                .IsUnicode(false);

            modelBuilder.Entity<sum_voice_hr_01>()
                .Property(e => e.tup_outgoingip)
                .IsUnicode(false);

            modelBuilder.Entity<sum_voice_hr_01>()
                .Property(e => e.tup_countryorareacode)
                .IsUnicode(false);

            modelBuilder.Entity<sum_voice_hr_01>()
                .Property(e => e.tup_matchedprefixcustomer)
                .IsUnicode(false);

            modelBuilder.Entity<sum_voice_hr_01>()
                .Property(e => e.tup_matchedprefixsupplier)
                .IsUnicode(false);

            modelBuilder.Entity<sum_voice_hr_01>()
                .Property(e => e.tup_sourceId)
                .IsUnicode(false);

            modelBuilder.Entity<sum_voice_hr_01>()
                .Property(e => e.tup_destinationId)
                .IsUnicode(false);

            modelBuilder.Entity<sum_voice_hr_01>()
                .Property(e => e.tup_customercurrency)
                .IsUnicode(false);

            modelBuilder.Entity<sum_voice_hr_01>()
                .Property(e => e.tup_suppliercurrency)
                .IsUnicode(false);

            modelBuilder.Entity<sum_voice_hr_01>()
                .Property(e => e.tup_tax1currency)
                .IsUnicode(false);

            modelBuilder.Entity<sum_voice_hr_01>()
                .Property(e => e.tup_tax2currency)
                .IsUnicode(false);

            modelBuilder.Entity<sum_voice_hr_01>()
                .Property(e => e.tup_vatcurrency)
                .IsUnicode(false);

            modelBuilder.Entity<sum_voice_hr_02>()
                .Property(e => e.tup_incomingroute)
                .IsUnicode(false);

            modelBuilder.Entity<sum_voice_hr_02>()
                .Property(e => e.tup_outgoingroute)
                .IsUnicode(false);

            modelBuilder.Entity<sum_voice_hr_02>()
                .Property(e => e.tup_incomingip)
                .IsUnicode(false);

            modelBuilder.Entity<sum_voice_hr_02>()
                .Property(e => e.tup_outgoingip)
                .IsUnicode(false);

            modelBuilder.Entity<sum_voice_hr_02>()
                .Property(e => e.tup_countryorareacode)
                .IsUnicode(false);

            modelBuilder.Entity<sum_voice_hr_02>()
                .Property(e => e.tup_matchedprefixcustomer)
                .IsUnicode(false);

            modelBuilder.Entity<sum_voice_hr_02>()
                .Property(e => e.tup_matchedprefixsupplier)
                .IsUnicode(false);

            modelBuilder.Entity<sum_voice_hr_02>()
                .Property(e => e.tup_sourceId)
                .IsUnicode(false);

            modelBuilder.Entity<sum_voice_hr_02>()
                .Property(e => e.tup_destinationId)
                .IsUnicode(false);

            modelBuilder.Entity<sum_voice_hr_02>()
                .Property(e => e.tup_customercurrency)
                .IsUnicode(false);

            modelBuilder.Entity<sum_voice_hr_02>()
                .Property(e => e.tup_suppliercurrency)
                .IsUnicode(false);

            modelBuilder.Entity<sum_voice_hr_02>()
                .Property(e => e.tup_tax1currency)
                .IsUnicode(false);

            modelBuilder.Entity<sum_voice_hr_02>()
                .Property(e => e.tup_tax2currency)
                .IsUnicode(false);

            modelBuilder.Entity<sum_voice_hr_02>()
                .Property(e => e.tup_vatcurrency)
                .IsUnicode(false);

            modelBuilder.Entity<sum_voice_hr_03>()
                .Property(e => e.tup_incomingroute)
                .IsUnicode(false);

            modelBuilder.Entity<sum_voice_hr_03>()
                .Property(e => e.tup_outgoingroute)
                .IsUnicode(false);

            modelBuilder.Entity<sum_voice_hr_03>()
                .Property(e => e.tup_incomingip)
                .IsUnicode(false);

            modelBuilder.Entity<sum_voice_hr_03>()
                .Property(e => e.tup_outgoingip)
                .IsUnicode(false);

            modelBuilder.Entity<sum_voice_hr_03>()
                .Property(e => e.tup_countryorareacode)
                .IsUnicode(false);

            modelBuilder.Entity<sum_voice_hr_03>()
                .Property(e => e.tup_matchedprefixcustomer)
                .IsUnicode(false);

            modelBuilder.Entity<sum_voice_hr_03>()
                .Property(e => e.tup_matchedprefixsupplier)
                .IsUnicode(false);

            modelBuilder.Entity<sum_voice_hr_03>()
                .Property(e => e.tup_sourceId)
                .IsUnicode(false);

            modelBuilder.Entity<sum_voice_hr_03>()
                .Property(e => e.tup_destinationId)
                .IsUnicode(false);

            modelBuilder.Entity<sum_voice_hr_03>()
                .Property(e => e.tup_customercurrency)
                .IsUnicode(false);

            modelBuilder.Entity<sum_voice_hr_03>()
                .Property(e => e.tup_suppliercurrency)
                .IsUnicode(false);

            modelBuilder.Entity<sum_voice_hr_03>()
                .Property(e => e.tup_tax1currency)
                .IsUnicode(false);

            modelBuilder.Entity<sum_voice_hr_03>()
                .Property(e => e.tup_tax2currency)
                .IsUnicode(false);

            modelBuilder.Entity<sum_voice_hr_03>()
                .Property(e => e.tup_vatcurrency)
                .IsUnicode(false);
        }
    }
}
