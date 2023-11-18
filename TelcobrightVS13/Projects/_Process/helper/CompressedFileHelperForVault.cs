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
    public class CompressedFileHelperForVault
    {
        public List<string> ExtensionsToAcceptAfterUnzip { get; set; }
        public string OriginalPathToExtract { get; set; }

        public string TempPathToExtract { get; set; }

        public List<CompressionType> SupportedCompressionTypes { get; set; } = new List<CompressionType>()
        {
            CompressionType.Gzip,
            CompressionType.Zip,
            CompressionType.tarZip,
            CompressionType.Sevenzip
        };

        public CompressedFileHelperForVault(List<string> extensionsToAcceptAfterUnzip, string originalPathToExtract = "",string tempPathToExtract ="")
        {
            ExtensionsToAcceptAfterUnzip = extensionsToAcceptAfterUnzip;
            this.OriginalPathToExtract = originalPathToExtract;
            TempPathToExtract = tempPathToExtract;
        }
        public void ExtractToTempDir(string compressedFile)
        {
            string fileExtension = Path.GetExtension(compressedFile);
            if (fileExtension.IsNullOrEmptyOrWhiteSpace())
                throw new Exception("File extension can't be empty for compressed files while creating cdr or logfile job.");
            CompressionType compressionType = CompressionType.None;
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
            DirectoryInfo tempDir = new DirectoryInfo(Path.Combine(this.OriginalPathToExtract, "temp"));
            UnZipper unzipper = new UnZipper(compressedFile, tempDir.FullName);
            unzipper.UnZipAll();
        }

        public void MoveToOriginalPath(DirectoryInfo tempDir)
        {
            foreach (FileInfo extractedFileInfo in tempDir.GetFiles("*.*", SearchOption.AllDirectories))
            {
                if (this.ExtensionsToAcceptAfterUnzip.Contains(extractedFileInfo.Extension) || extractedFileInfo.Extension == "")
                {
                    var destCdrFile = this.OriginalPathToExtract + Path.DirectorySeparatorChar + extractedFileInfo.Name;
                    if (File.Exists(destCdrFile))
                    {
                        FileInfo existingFileINfo = new FileInfo(destCdrFile);
                        if (extractedFileInfo.Name == existingFileINfo.Name &&
                            (existingFileINfo.Length == extractedFileInfo.Length || existingFileINfo.Length > extractedFileInfo.Length))
                        {
                            extractedFileInfo.Delete();
                        }
                        else if (extractedFileInfo.Name == existingFileINfo.Name && existingFileINfo.Length < extractedFileInfo.Length)
                        {
                            existingFileINfo.Delete();


                        }
                    }

                    string tempExtension = ".tmp";
                    string destTempFileName = destCdrFile + tempExtension;
                    File.Copy(extractedFileInfo.FullName, destTempFileName);

                    FileInfo copiedTempFileInfo = new FileInfo(destTempFileName);
                    if (copiedTempFileInfo.Length == extractedFileInfo.Length)
                    {
                        File.Move(destTempFileName, destTempFileName.Replace(tempExtension, ""));//rename to remove .tmp extension
                        extractedFileInfo.Delete();
                    }
                    else
                    {
                        throw new Exception("temp file and dest file length did not match");
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


