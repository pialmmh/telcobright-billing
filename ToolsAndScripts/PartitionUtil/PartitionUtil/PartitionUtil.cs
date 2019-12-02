using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace PartitionUtil
{
    public class PartitionUtil
    {
        private static string srcDir = "..\\..\\..\\PartitionInfoCsvs";

        public static void Main()
        {
            string csvFileName = GetInput();
            Dictionary<string, List<SinglePartition>> partitions = ParseTableWisePartitions(csvFileName);

            List<TablePartitionInfo> tableWisePartitionInfo
                = partitions.Select(kv => new TablePartitionInfo(kv.Key, kv.Value.ToList())).ToList();

            Console.WriteLine("Enter date before which partitions will be deleted:");
            string delBefore = Console.ReadLine();
            DateTime delBeforeDate = Convert.ToDateTime(delBefore);

            Console.WriteLine("Enter number of new partitions to add [max number of partitions can be 1025 and will be adjusted automatically]:");
            int noOfNewPartition = Convert.ToInt32(Console.ReadLine());
            //write
            ;


            using (TextWriter textWriter =
                File.CreateText($"{srcDir}{Path.DirectorySeparatorChar}{csvFileName.Replace(".csv", "_sql.txt")}"))
            {
                tableWisePartitionInfo.ForEach(p =>
                {
                    var tableName = p.TableName;
                    Console.Write($"Processing table {tableName.ToUpper()}...");
                    var stringBuilder = new PartitionChangeScriptGenerator(p, delBeforeDate,noOfNewPartition).GenerateScript();
                    textWriter.WriteLine($@"#TABLE: {p.TableName}
#LAST MIN PARTITION: {p.MinPartitionNo}, LAST MAX PARTITION: {p.MaxPartitionNo}
#LAST MIN DATE: {p.MinPartitionDate.Date:yyyy-MM-dd}, LAST MAX DATE: {p.MaxPartitionDate.Date:yyyy-MM-dd}
#PARTITION NUMBERS TO BE DELETED BEFORE: {p.Partitions.First(part=>part.PartitionDate==delBeforeDate).PartitionNumber}, DELETE BEFORE DATE: {delBefore:yyyy-MM-dd}
#");
                    textWriter.WriteLine(stringBuilder);
                    textWriter.WriteLine();
                    Console.WriteLine("Finished");
                });
            }

            Console.WriteLine("Successfully generated.");
            Console.Read();
        }

        private static string GetInput()
        {
            Dictionary<string, string> csvFiles = new DirectoryInfo(srcDir).GetFiles("*.csv")
                            .Select((finfo, index) => new
                            {
                                Id = index + 1,
                                FileName = finfo.Name
                            }).ToDictionary(a => a.Id.ToString(), a => a.FileName);

            Console.WriteLine("Select file number in PartionInfoCsvs folder to process:");
            Console.Write(string.Join(Environment.NewLine,
                csvFiles.Select(kv => kv.Key + ". " + kv.Value).ToList()));
            Console.WriteLine();
            string csvFileName = csvFiles[Console.ReadLine()];
            Console.WriteLine("Processing " + csvFileName.ToUpper());
            return csvFileName;
        }

        private static Dictionary<string, List<SinglePartition>> ParseTableWisePartitions(string csvFileName)
        {
            
            var lines = File.ReadAllLines(srcDir + Path.DirectorySeparatorChar + csvFileName).Skip(1);
            var rowsWithCols = lines.Select(line => line.Split(','));
            var partitions = rowsWithCols.Select(row => new SinglePartition()
            {
                TableName = row[2],
                PartitionName = row[3],
                PartitionNumber = Convert.ToInt32(Regex.Match(row[3], @"\d+").Value),
                PartitionExpression = row[9].Replace("`", ""),
                PartitionDate = Convert.ToDateTime(row[11].Replace("'", string.Empty)),
                Engine = row[29],
            }).ToList();

            return partitions.GroupBy(p => p.TableName).ToDictionary(g => g.Key, g => g.ToList());
        }
    }
}
