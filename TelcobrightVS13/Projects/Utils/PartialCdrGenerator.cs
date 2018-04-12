using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace MediationModel
{
    public class PartialCdrGenerator
    {
        public string cdrFormat { get; set; }
        public string fullSourcePath { get; set; }
        public string fullTargetPath { get; set; }
        public PartialCdrGenerator(string strCdrFormat, string fullSrcPath)//e.g. dialogic, zte
        {
            this.cdrFormat = strCdrFormat;
            this.fullSourcePath = fullSrcPath;
            this.fullTargetPath = fullSourcePath + Path.DirectorySeparatorChar + "PartialCdrsOnly";
        }
        public void Generate()
        {
            Directory.CreateDirectory(fullTargetPath);
            List<FileInfo> files = GetFileNames();
            switch (cdrFormat)
            {
                case "dialogic":
                    GenerateDialogic(files);
                    break;
                case "zte":
                    break;
            }
        }
        private void GenerateDialogic(List<FileInfo> files)
        {
            files.ForEach(fileInfo =>ProcessOneCSVFile(fileInfo,';'));
        }
        private void ProcessOneCSVFile(FileInfo fileInfo,char sepChar)
        {
            List<string[]> lines = new List<string[]>();
            File.ReadAllLines(fileInfo.FullName).ToList().ForEach(line =>
                {
                    string[] fields = line.Split(sepChar);
                    lines.Add(fields);
                }
            );
            List<string[]> linesWithPartialsOnly = getIntermediates(lines);
            linesWithPartialsOnly.AddRange(getSubsequentsToIntermediates(lines,
                linesWithPartialsOnly.Select(lineWithI=>lineWithI[3]).ToList()));
            if (linesWithPartialsOnly.Count > 0)
            {
                using (TextWriter tw = new StreamWriter(fullTargetPath + Path.DirectorySeparatorChar + fileInfo.Name))
                {
                    linesWithPartialsOnly.ForEach(line => tw.WriteLine(string.Join(sepChar.ToString(), line)));
                }
            }
        }
        private List<string[]> getIntermediates(List<string[]> lines)
        {
            return lines.Where(line => line[6].Equals("I")).ToList();
        }
        private List<string[]> getSubsequentsToIntermediates(List<string[]> lines, List<string> intermediatesBillIds)
        {
            return lines.Where(line => intermediatesBillIds.Contains(line[3])).ToList();
        }
        private void GenerateZte()
        {
            throw new NotImplementedException();
        }
        private List<FileInfo> GetFileNames()
        {
            var dir = new DirectoryInfo(fullSourcePath);
            return dir.GetFiles().ToList();
        }
    }
}
