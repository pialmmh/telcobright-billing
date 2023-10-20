using TelcobrightMediation;
using MySql.Data.MySqlClient;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using TelcobrightFileOperations;
using System.Reflection;
using System.Threading.Tasks;
using MediationModel;
using System.Configuration;
using System.Data.Common;
using System.Data.Entity;
using Quartz;
using LibraryExtensions;
using QuartzTelcobright;
using TelcobrightMediation.Cdr;
using TelcobrightMediation.Config;
using TelcobrightFileOperations;
using System.IO;
namespace Process
{
    [Export("LogPreprocessor", typeof(ILogPreprocessor))]
    public class CompressedCdrExtractor : ILogPreprocessor
    {
        public override string ToString()
        {
            return this.RuleName;
        }
        public string RuleName => this.GetType().ToString();
        public string HelpText => "Compressed Cdr Extractor";
        public bool IsPrepared { get; set; }
        public object RuleConfigData { get; set; }
        public List<CompressionType> SupportedCompressionTypes { get; set; }

        public void PrepareRule()
        {
            Dictionary<string, object> dataAsDic = (Dictionary<string, object>) this.RuleConfigData;
            this.SupportedCompressionTypes = (List<CompressionType>) dataAsDic["SupportedCompressionTypes"];
        }

        public void Execute(Object input)
        {
            Dictionary<string, object> dataAsMap = (Dictionary<string, object>)input;
            string compressedFile = (string)dataAsMap["compressedFile"];
            string fileExtension = Path.GetExtension(compressedFile);
            if(fileExtension.IsNullOrEmptyOrWhiteSpace())
                throw new Exception("File extension can't be empty for compressed files while creating cdr or logfile job.");
            CompressionType compressionType=CompressionType.None;
            if (CompressionTypeHelper.ExtensionVsCompressionTypes.TryGetValue(fileExtension, out compressionType) ==
                false)
            {
                throw new Exception("Invalid compressed file extension:" + fileExtension);
            }
            else
            {
                if (!this.SupportedCompressionTypes.Contains(compressionType))
                    throw new Exception(
                        $"Unsupported compression type {compressionType.ToString()}, " +
                        $"supported extensions are: {string.Join(",", this.SupportedCompressionTypes.Select(ct => ct.ToString()))}");
            }
            string targetVaultDir = (string)dataAsMap["targetVaultDir"];
            DirectoryInfo tempDir = new DirectoryInfo(Path.Combine(targetVaultDir, "temp"));
            List<string> extensionsToIncludeAfterUnzip = (List<string>)dataAsMap["extensionsToIncludeAfterUnzip"];
            UnZipper unzipper = new UnZipper(compressedFile, tempDir.FullName, false);
            unzipper.UnZipAll();

            Func<FileInfo, FileInfo, bool> sameFileExists = (src, dst) => src.FullName == dst.FullName && src.Length == dst.Length;

            foreach (FileInfo extractedFileInfo in tempDir.GetFiles("*.*", SearchOption.AllDirectories))
            {
                if (extensionsToIncludeAfterUnzip.Contains(extractedFileInfo.Extension))
                {
                    var destCdrFile = targetVaultDir + "\\" + extractedFileInfo.Name;
                    if (File.Exists(destCdrFile))
                    {
                        FileInfo existingFileINfo = new FileInfo(destCdrFile);
                        if (sameFileExists(extractedFileInfo, existingFileINfo))
                        {
                            extractedFileInfo.Delete();
                        }
                    }
                    string tempExtension = ".tmp";
                    var targetFilenameWithTempExtension = extractedFileInfo.FullName + tempExtension;
                    File.Copy(extractedFileInfo.FullName, targetFilenameWithTempExtension);
                    FileInfo copiedTempFileInfo = new FileInfo(targetFilenameWithTempExtension);
                    if (copiedTempFileInfo.Length == extractedFileInfo.Length)
                    {
                        File.Move(copiedTempFileInfo.FullName, copiedTempFileInfo.FullName.Replace(tempExtension, ""));
                        extractedFileInfo.Delete();
                    }
                }
                else//this extension is not a cdr file, delete
                {
                    extractedFileInfo.Delete();
                }
            }
        }
    }
}


