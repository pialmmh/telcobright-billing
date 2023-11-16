using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediationModel;
using LibraryExtensions;
using System.IO;
using LogPreProcessor;
using TelcobrightFileOperations;
using TelcobrightMediation;

namespace InstallConfig.config._helper
{
    public class NeWrapperWithAdditionalInfo
    {
        public ne ne { get; }
        public NeAdditionalSetting neAdditionalSetting { get; }

        public NeWrapperWithAdditionalInfo(ne ne, NeAdditionalSetting neAdditionalSetting)
        {
            this.ne = ne;
            this.neAdditionalSetting = neAdditionalSetting;
        }
    }
    public class CasNeInfoHelper
    {

        Dictionary<int, List<NeWrapperWithAdditionalInfo>> partnerWiseNesFromCsv;
        private string casOpCsvFileName;
        public CasNeInfoHelper(string casOpCsvFileName)
        {
            this.casOpCsvFileName = casOpCsvFileName;

            //List<string[]> rows = FileUtil.ParseTextFileToListOfStrArraySimple(this.casOpCsvFileName, ',', 1);
            List<string[]> rows = ExcelHelper.parseExcellRows(this.casOpCsvFileName);
            this.partnerWiseNesFromCsv = rows.Where(r => r[20] == "0")
                .Select(r => convertRowToNe(r))
                .GroupBy(n => n.ne.idCustomer)
                .ToDictionary(grouping => grouping.Key, grouping => grouping.ToList());
            ;
            //this.partnerWiseNesFromCsv = allNes.GroupBy(n => n.idCustomer).ToDictionary(g => g.Key, g => g.ToList());
        }


        public static string getCasOperatorInfoFile()
        {
            return new DirectoryInfo(FileAndPathHelperReadOnly.GetCurrentExecPath()).Parent.Parent.FullName + 
                Path.DirectorySeparatorChar.ToString() + "config" + 
                Path.DirectorySeparatorChar.ToString() + "allDeployment" + 
                Path.DirectorySeparatorChar.ToString() + "cas" + 
                Path.DirectorySeparatorChar.ToString() + "_helper" + 
                Path.DirectorySeparatorChar.ToString() + "casOperatorInfo.xlsx";//add more
        }


        private static NeWrapperWithAdditionalInfo convertRowToNe(string[] row)
        {
            ne ne = TemplateNeFactory.GetInstanceNe();          
            ne.idSwitch = Convert.ToInt32(row[2]);
            ne.idCustomer = Convert.ToInt32(row[1]);
            ne.idcdrformat = Convert.ToInt32(row[10]);
            ne.CDRPrefix = Convert.ToString(row[11]);
            ne.FileExtension = Convert.ToString(row[12]);
            ne.SkipCdrDecoded = Convert.ToInt32(row[15]);
            ne.AllowEmptyFile = Convert.ToInt32(row[17]);
            ne.SkipCdrListed = Convert.ToInt32(row[14]);
            ne.BackupFileLocations = Convert.ToString(row[13]);
            ne.FilterDuplicateCdr = Convert.ToInt32(row[19]);
            ne.SwitchName= Convert.ToString(row[6]);
            ne.UseIdCallAsBillId = Convert.ToInt32(row[16]);
            ne.SourceFileLocations = Convert.ToString(row[18]);

            NeAdditionalSetting neAdditionalSetting = new NeAdditionalSetting()
            {
                    ExpectedNoOfCdrIn24Hour = Convert.ToInt32(row[22]),
                    AggregationStyle = Convert.ToString(row[23]),
                    ProcessMultipleCdrFilesInBatch = Convert.ToBoolean(row[24]),
                    PreDecodeAsTextFile = Convert.ToBoolean(row[25]),
                    MaxConcurrentFilesForParallelPreDecoding = Convert.ToInt32(row[26]),
                    MinRowCountToStartBatchCdrProcessing = Convert.ToInt32(row[27]),
                    MaxNumberOfFilesInPreDecodedDirectory = Convert.ToInt32(row[28])
            };

            return new NeWrapperWithAdditionalInfo(ne, neAdditionalSetting);
        }
        public List<NeWrapperWithAdditionalInfo> getNesByOpId(int opId)
        {
            List<NeWrapperWithAdditionalInfo> nes = this.partnerWiseNesFromCsv[opId];
            return nes;
        }
    } 
}

