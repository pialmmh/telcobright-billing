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
    public class CreateNe: IScript
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
            string sql = $@"drop table if exists ne;
                    CREATE TABLE `ne` (
                      `idSwitch` int(11) NOT NULL AUTO_INCREMENT,
                      `idCustomer` int(11) NOT NULL,
                      `idcdrformat` int(11) NOT NULL DEFAULT '0',
                      `idMediationRule` int(11) NOT NULL DEFAULT '1',
                      `SwitchName` varchar(100) COLLATE utf8mb4_bin DEFAULT NULL,
                      `CDRPrefix` varchar(50) COLLATE utf8mb4_bin DEFAULT NULL,
                      `FileExtension` varchar(45) COLLATE utf8mb4_bin DEFAULT NULL,
                      `Description` varchar(100) COLLATE utf8mb4_bin DEFAULT NULL,
                      `SourceFileLocations` text COLLATE utf8mb4_bin,
                      `BackupFileLocations` text COLLATE utf8mb4_bin,
                      `LoadingStopFlag` int(11) DEFAULT NULL,
                      `LoadingSpanCount` int(11) DEFAULT NULL,
                      `TransactionSizeForCDRLoading` int(11) DEFAULT NULL,
                      `DecodingSpanCount` int(11) DEFAULT NULL,
                      `SkipAutoCreateJob` int(11) DEFAULT NULL,
                      `SkipCdrListed` int(11) DEFAULT '0',
                      `SkipCdrReceived` int(11) DEFAULT '0',
                      `SkipCdrDecoded` int(11) DEFAULT '0',
                      `SkipCdrBackedup` int(11) DEFAULT '0',
                      `KeepDecodedCDR` int(11) DEFAULT NULL,
                      `KeepReceivedCdrServer` int(11) DEFAULT NULL,
                      `CcrCauseCodeField` int(11) DEFAULT NULL,
                      `SwitchTimeZoneId` int(11) DEFAULT NULL,
                      `CallConnectIndicator` varchar(45) COLLATE utf8mb4_bin NOT NULL DEFAULT 'CT',
                      `FieldNoForTimeSummary` int(11) NOT NULL DEFAULT '29',
                      `EnableSummaryGeneration` varchar(45) COLLATE utf8mb4_bin NOT NULL DEFAULT '0',
                      `ExistingSummaryCacheSpanHr` int(11) NOT NULL DEFAULT '6',
                      `BatchToDecodeRatio` int(11) NOT NULL DEFAULT '3',
                      `FilterDuplicateCdr` int(11) DEFAULT '0',
                      `UseIdCallAsBillId` int(11) DEFAULT '0',
                      `PrependLocationNumberToFileName` int(11) NOT NULL DEFAULT '0',
                      `AllowEmptyFile` INT NULL DEFAULT 0,
                      `ipOrTdm` VARCHAR(1) NULL DEFAULT 'i',
                      PRIMARY KEY (`idSwitch`),
                      KEY `idCustomer` (`idCustomer`),
                      KEY `fk_cdrformat_idx` (`idcdrformat`),
                      CONSTRAINT `fk_cdrformat` FOREIGN KEY (`idcdrformat`) REFERENCES `enumcdrformat` (`id`) ON DELETE NO ACTION ON UPDATE NO ACTION,
                      CONSTRAINT `ne_ibfk_1` FOREIGN KEY (`idCustomer`) REFERENCES `telcobrightpartner` (`idCustomer`)
                    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin;";

            return sql;
        }
    }
}
