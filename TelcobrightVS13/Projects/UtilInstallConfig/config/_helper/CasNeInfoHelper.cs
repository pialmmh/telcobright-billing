using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediationModel;
using LibraryExtensions;
using System.IO;
using TelcobrightFileOperations;
namespace InstallConfig.config._helper
{
    public class CasNeInfoHelper
    {
        Dictionary<int, List<ne>> partnerWiseNesFromCsv = new Dictionary<int, List<ne>>();
        private string casOpCsvFileName;
        public CasNeInfoHelper(string casOpCsvFileName)
        {
            this.casOpCsvFileName = casOpCsvFileName;

            //List<string[]> rows = FileUtil.ParseTextFileToListOfStrArraySimple(this.casOpCsvFileName, ',', 1);
            List<string[]> rows = ExcelHelper.parseExcellRows(this.casOpCsvFileName);
            this.partnerWiseNesFromCsv = rows.Where(r => r[20] == "0")
                .Select(r => convertRowToNe(r))
                .GroupBy(n => n.idCustomer)
                .ToDictionary(grouping => grouping.Key, grouping => grouping.ToList());
            ;
            //this.partnerWiseNesFromCsv = allNes.GroupBy(n => n.idCustomer).ToDictionary(g => g.Key, g => g.ToList());
        }

        private static ne convertRowToNe(string[] row)
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

            return ne;
        }
        public List<ne> getNesByOpId(int opId)
        {
            List<ne> Nes = this.partnerWiseNesFromCsv[opId];
            return Nes;
        }
    } 
}

