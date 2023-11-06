using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;
using LibraryExtensions;
using System.Diagnostics;
using SharpCompress.Archives;
using SharpCompress.Common;
using SharpCompress.Readers;
using TelcobrightInfra;

namespace TelcobrightFileOperations
{
    
    public class UnZipper
    {
        private String zipPath;
        private string extractPath;
        FileInfo zippedFile;
        bool DeleteOriginalCompressedFile { get; set; }

        public UnZipper(String zipPath, String extractPath = "", bool deleteOriginalCompressedFile=false)
        {
            this.DeleteOriginalCompressedFile = deleteOriginalCompressedFile;
            this.zippedFile = new FileInfo(zipPath);
            this.extractPath = (extractPath == "") ? zippedFile.DirectoryName : extractPath;
        }

        public void UnZipAll()
        {
            DirectoryInfo tempDir = new DirectoryInfo(this.extractPath + "\\temp");
            if (Directory.Exists(tempDir.FullName) == false)
            {
                Directory.CreateDirectory(tempDir.FullName);
            }
            else
            {
                tempDir.DeleteContentRecusively();
            }

            if (zippedFile.FullName.EndsWith(".gz"))
            {
                string extension = Path.GetExtension(zippedFile.Name);
                string compressedFileName = zippedFile.Name.Substring(0, zippedFile.Name.Length - extension.Length); ;
                string tempFileName = tempDir.FullName + Path.DirectorySeparatorChar + compressedFileName;

                // Extract the .gz zippedFile into the temporary zippedFile
                using (FileStream gzFileStream = File.OpenRead(zippedFile.FullName))
                {
                    using (FileStream tempFileStream = File.Create(tempFileName))
                    {
                        using (GZipStream gzipStream = new GZipStream(gzFileStream, CompressionMode.Decompress))
                        {
                            gzipStream.CopyTo(tempFileStream);
                        }
                    }
                }
            }
            else if (zippedFile.FullName.EndsWith(".rar"))
            {

                string extension = Path.GetExtension(zippedFile.Name);
                string compressedFileName = zippedFile.Name.Substring(0, zippedFile.Name.Length - extension.Length); ;
                string tempFileName = tempDir.FullName + Path.DirectorySeparatorChar + compressedFileName;

                string rarFilePath = zippedFile.FullName.Replace("\\", "//");
                string targetPath = tempDir.FullName.Replace("\\", "//");

                string sevenZipPath = ExternalResourceManager.getResourcePath(ExternalResourceType.SevenZip);
                string command1 = $@"{sevenZipPath} x {rarFilePath} -o{targetPath}";
                //string command2 = $@" & {sevenZipPath} x -o{targetPath} {targetPath + "//" + compressedFileName}";

                string finalCommand = command1 ;

                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                };

                Process process = new Process { StartInfo = psi };

                process.Start();
                process.StandardInput.WriteLine(finalCommand);
                process.StandardInput.WriteLine("exit");
                process.WaitForExit();

                if (process.ExitCode == 0)
                {
                    Console.WriteLine("Extraction completed.");
                }
                else
                {
                    Console.WriteLine("Extraction Failed.");
                }
            }
            else if (zippedFile.FullName.EndsWith("tar.Z"))
            {
                string extension = Path.GetExtension(zippedFile.Name);
                string compressedFileName = zippedFile.Name.Substring(0, zippedFile.Name.Length - extension.Length); ;
                string tempFileName = tempDir.FullName + Path.DirectorySeparatorChar + compressedFileName;

                string tarFilePath = zippedFile.FullName.Replace("\\","//");
                string targetPath = tempDir.FullName.Replace("\\", "//");
                
                string sevenZipPath = ExternalResourceManager.getResourcePath(ExternalResourceType.SevenZip);
                string command1 = $@"{sevenZipPath} x -o{targetPath} {tarFilePath}"; 
                string command2 = $@" & {sevenZipPath} x -o{targetPath} {targetPath +"//"+ compressedFileName}";

                string finalCommand = command1 + command2;

                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                };

                Process process = new Process { StartInfo = psi };

                process.Start();
                process.StandardInput.WriteLine(finalCommand);
                process.StandardInput.WriteLine("exit");
                process.WaitForExit();

                if (process.ExitCode == 0)
                {
                    Console.WriteLine("Extraction completed.");
                }
                else
                {
                    Console.WriteLine("Extraction Failed.");
                }
            }
            else
            {
                ZipFile.ExtractToDirectory(zippedFile.FullName, tempDir.ToString());
            }

            if(this.DeleteOriginalCompressedFile==true)
                File.Delete(zippedFile.FullName);
        }
    }
}
