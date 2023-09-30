using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelcobrightInfra;
namespace InstallConfig
{
    [Export("Script", typeof(IScript))]
    //[ExportMetadata("Symbol", '+')]
    public class ViewToBTRC : IScript
    {
        public override string ToString() => this.RuleName;
        public string RuleName => GetType().Name;
        public string HelpText => "";
        public ScriptType ScriptType => ScriptType.SqlDDL;
        public string ScriptDir { get; set; }
        public string SrcTextFileName { get; set; }
        public string GetScript(object data)
        {
            //var map = (Dictionary<string, string>) data;
            //string operatorName = map["operatorName"];
            string sql = $@"set foreign_key_checks=0;
                                drop table if exists ansprefixextra; drop view if exists ansprefixextra; create view ansprefixextra as select * from btrc_cas.ansprefixextra;
                                drop table if exists billingruleassignment; drop view if exists billingruleassignment; create view billingruleassignment as select * from btrc_cas.billingruleassignment;
                                drop table if exists jsonbillingrule; drop view if exists jsonbillingrule; create view jsonbillingrule as select * from btrc_cas.jsonbillingrule;
                                drop table if exists partner; drop view if exists partner; create view partner as select * from btrc_cas.partner;
                                drop table if exists partnerprefix; drop view if exists partnerprefix; create view partnerprefix as select * from btrc_cas.partnerprefix;
                                drop table if exists product; drop view if exists product; create view product as select * from btrc_cas.product;
                                drop table if exists rate; drop view if exists rate; create view rate as select * from btrc_cas.rate;
                                drop table if exists rateassign; drop view if exists rateassign; create view rateassign as select * from btrc_cas.rateassign;
                                drop table if exists rateplan; drop view if exists rateplan; create view rateplan as select * from btrc_cas.rateplan;
                                drop table if exists rateplanassign; drop view if exists rateplanassign; create view rateplanassign as select * from btrc_cas.rateplanassign;
                                drop table if exists rateplanassignmenttuple; drop view if exists rateplanassignmenttuple; create view rateplanassignmenttuple as select * from btrc_cas.rateplanassignmenttuple;
                                drop table if exists ratetask; drop view if exists ratetask; create view ratetask as select * from btrc_cas.ratetask;
                                drop table if exists ratetaskassign; drop view if exists ratetaskassign; create view ratetaskassign as select * from btrc_cas.ratetaskassign;
                                drop table if exists ratetaskassignreference; drop view if exists ratetaskassignreference; create view ratetaskassignreference as select * from btrc_cas.ratetaskassignreference;
                                drop table if exists ratetaskreference; drop view if exists ratetaskreference; create view ratetaskreference as select * from btrc_cas.ratetaskreference;
                                drop table if exists reporttemplate; drop view if exists reporttemplate; create view reporttemplate as select * from btrc_cas.reporttemplate;
                                drop table if exists uom; drop view if exists uom; create view uom as select * from btrc_cas.uom;
                                drop table if exists uom_conversion; drop view if exists uom_conversion; create view uom_conversion as select * from btrc_cas.uom_conversion;
                                drop table if exists uom_conversion_dated; drop view if exists uom_conversion_dated; create view uom_conversion_dated as select * from btrc_cas.uom_conversion_dated;
                                drop table if exists xyzprefix; drop view if exists xyzprefix; create view xyzprefix as select * from btrc_cas.xyzprefix;
                                drop table if exists xyzprefixset; drop view if exists xyzprefixset; create view xyzprefixset as select * from btrc_cas.xyzprefixset;
                                drop table if exists xyzselected; drop view if exists xyzselected; create view xyzselected as select * from btrc_cas.xyzselected;
                                drop table if exists userclaims; drop view if exists userclaims; create view userclaims as select * from btrc_cas.userclaims;
                                drop table if exists userlogins; drop view if exists userlogins; create view userlogins as select * from btrc_cas.userlogins;
                                drop table if exists userroles; drop view if exists userroles; create view userroles as select * from btrc_cas.userroles;
                                drop table if exists users; drop view if exists users; create view users as select * from btrc_cas.users;
                                drop table if exists enumcdrformat; drop view if exists enumcdrformat; create view enumcdrformat as select * from btrc_cas.enumcdrformat;
                                drop table if exists cdrfieldmappingbyswitchtype; drop view if exists cdrfieldmappingbyswitchtype; create view cdrfieldmappingbyswitchtype as select * from btrc_cas.cdrfieldmappingbyswitchtype;
                                drop table if exists cdrfieldlist; drop view if exists cdrfieldlist; create view cdrfieldlist as select * from btrc_cas.cdrfieldlist;
                                drop table if exists temp_route;                                
                                create table temp_route as 
                                select * from route;
                                drop table route;
                                CREATE TABLE `route` (
                                  `idroute` int(11) NOT NULL AUTO_INCREMENT,
                                  `RouteName` varchar(45) COLLATE utf8mb4_bin NOT NULL,
                                  `SwitchId` int(11) NOT NULL,
                                  `CommonRoute` int(11) NOT NULL DEFAULT '0',
                                  `idPartner` int(11) NOT NULL,
                                  `NationalOrInternational` int(11) NOT NULL DEFAULT '0',
                                  `Description` varchar(45) COLLATE utf8mb4_bin DEFAULT NULL,
                                  `Status` int(11) NOT NULL DEFAULT '0',
                                  `date1` datetime DEFAULT NULL,
                                  `field1` int(11) DEFAULT NULL,
                                  `field2` int(11) DEFAULT NULL,
                                  `field3` int(11) DEFAULT NULL,
                                  `field4` int(11) DEFAULT NULL,
                                  `field5` varchar(45) COLLATE utf8mb4_bin DEFAULT NULL,
                                  `zone` enum('Dhaka','Bogra','Sylhet','Khulna','Chittagong') DEFAULT NULL,
                                  PRIMARY KEY (`idroute`),
                                  UNIQUE KEY `RouteName` (`RouteName`,`SwitchId`),
                                  KEY `fk_Ne` (`SwitchId`),
                                  KEY `idCarrier` (`idPartner`),
                                  CONSTRAINT `fk_Ne` FOREIGN KEY (`SwitchId`) REFERENCES `ne` (`idSwitch`),
                                  CONSTRAINT `route_ibfk_1` FOREIGN KEY (`idPartner`) REFERENCES `btrc_cas`.`partner` (`idPartner`) ON DELETE NO ACTION ON UPDATE NO ACTION
                                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin;
                                insert into route select * from temp_route;
                                drop table temp_route;
                                set foreign_key_checks=1;";
            return sql;
        }
    }
}
