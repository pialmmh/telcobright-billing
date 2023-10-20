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
        public string VaultPathToExtract { get; set; }
        public List<CompressionType> SupportedCompressionTypes { get; set; }= new List<CompressionType>()
        {
            CompressionType.Gzip,
            CompressionType.Zip
        };

        public CompressedFileHelperForVault(List<string> extensionsToAcceptAfterUnzip, string vaultPathToExtract="")
        {
            ExtensionsToAcceptAfterUnzip = extensionsToAcceptAfterUnzip;
            VaultPathToExtract = vaultPathToExtract;
        }
        public void ExtractWithSafeCopy(string compressedFile)
        {
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
            DirectoryInfo tempDir = new DirectoryInfo(Path.Combine(this.VaultPathToExtract, "temp"));
            UnZipper unzipper = new UnZipper(compressedFile, tempDir.FullName, false);
            unzipper.UnZipAll();

            Func<FileInfo, FileInfo, bool> sameFileExists = (src, dst) => src.FullName == dst.FullName && src.Length == dst.Length;

            foreach (FileInfo extractedFileInfo in tempDir.GetFiles("*.*", SearchOption.AllDirectories))
            {
                if (this.ExtensionsToAcceptAfterUnzip.Contains(extractedFileInfo.Extension)||extractedFileInfo.Extension=="")
                {
                    var destCdrFile = this.VaultPathToExtract + Path.DirectorySeparatorChar + extractedFileInfo.Name;
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
                    File.Copy(extractedFileInfo.FullName, targetFilenameWithTempExtension);//safe copy with .tmp extension
                    FileInfo copiedTempFileInfo = new FileInfo(targetFilenameWithTempExtension);
                    if (copiedTempFileInfo.Length == extractedFileInfo.Length)
                    {
                        File.Move(copiedTempFileInfo.FullName, copiedTempFileInfo.FullName.Replace(tempExtension, ""));//rename to remove .tmp extension
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


