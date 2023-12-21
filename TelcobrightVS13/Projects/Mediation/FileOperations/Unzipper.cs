using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;
using LibraryExtensions;
using System.Diagnostics;
using MediationModel;
using MySql.Data.MySqlClient;
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
        private DateTime _compressionTime;
        private DateTime _completionTime;
        private int _status;
        private string ConStr;

        public UnZipper(string zipPath,string con, string extractPath = "")
        {
            this.zippedFile = new FileInfo(zipPath);
            this.extractPath = (extractPath == "") ? zippedFile.DirectoryName : extractPath;
            this.ConStr = con;
        }

        public void UnZipAll()
        {
            DirectoryInfo tempDir = new DirectoryInfo(this.extractPath);
            if (Directory.Exists(tempDir.FullName) == false)
            {
                Directory.CreateDirectory(tempDir.FullName);
            }

            if (zippedFile.FullName.EndsWith(".rar"))
            {

                string rarFilePath = zippedFile.FullName.Replace("\\", "//");
                string targetPath = tempDir.FullName.Replace("\\", "//");

                string sevenZipPath = ExternalResourceManager.getResourcePath(ExternalResourceType.SevenZip);
                string command = $"x \"{rarFilePath}\" -o\"{targetPath}\"";

                Process process = new Process();
                process.StartInfo.FileName = sevenZipPath;
                process.StartInfo.Arguments = command;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.CreateNoWindow = true;

                this._compressionTime = DateTime.Now;

                process.OutputDataReceived += (sender, e) => Console.WriteLine(e.Data);
                process.ErrorDataReceived += (sender, e) => Console.WriteLine(e.Data);

                process.Start();
                process.BeginOutputReadLine();
                process.WaitForExit();

                _completionTime = DateTime.Now;
                if (process.ExitCode == 0)
                {
                    this._status = 1;
                    Console.WriteLine("Extraction completed.");
                }
                else
                {
                    this._status = 5;
                    Console.WriteLine("Extraction Failed.");
                }
            }
            else if (zippedFile.FullName.EndsWith("tar.Z"))
            {
                string extension = Path.GetExtension(zippedFile.Name);
                string compressedFileName = zippedFile.Name.Substring(0, zippedFile.Name.Length - extension.Length); ;

                string tarFilePath = zippedFile.FullName.Replace("\\", "//");
                string targetPath = tempDir.FullName.Replace("\\", "//");

                string sevenZipPath = ExternalResourceManager.getResourcePath(ExternalResourceType.SevenZip);
                string command1 = $@"{sevenZipPath} x -o{targetPath} {tarFilePath}";
                string command2 = $@" & {sevenZipPath} x -o{targetPath} {targetPath + "//" + compressedFileName}";

                string finalCommand = command1 + command2;

                Process process = new Process();
                process.StartInfo.FileName = sevenZipPath;
                process.StartInfo.Arguments = finalCommand;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.CreateNoWindow = true;

                this._compressionTime = DateTime.Now;

                process.OutputDataReceived += (sender, e) => Console.WriteLine(e.Data);
                process.ErrorDataReceived += (sender, e) => Console.WriteLine(e.Data);

                process.Start();
                process.BeginOutputReadLine();
                process.WaitForExit();

                this._completionTime = DateTime.Now;
                if (process.ExitCode == 0)
                {
                    this._status = 1;
                    Console.WriteLine("Extraction completed.");
                }
                else
                {
                    this._status = 5;
                    Console.WriteLine("Extraction Failed.");
                }
            }
            else if (zippedFile.FullName.EndsWith(".tgz") || zippedFile.FullName.EndsWith("tar.gz"))
            {
                string tarFilePath = zippedFile.FullName.Replace("\\", "//");
                string targetPath = tempDir.FullName.Replace("\\", "//");

                string sevenZipPath = ExternalResourceManager.getResourcePath(ExternalResourceType.SevenZip);

                string command1 = $@"{sevenZipPath} x -tgzip -so {tarFilePath}";
                string command2 = $@" | {sevenZipPath} x -aoa -si -ttar -o{targetPath}";


                string finalCommand = "/c " + command1 + command2;
               
                Process process = new Process();
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.Arguments = finalCommand;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.CreateNoWindow = true;

                this._compressionTime = DateTime.Now;

                process.OutputDataReceived += (sender, e) => Console.WriteLine(e.Data);
                process.ErrorDataReceived += (sender, e) => Console.WriteLine(e.Data);

                process.Start();
                process.BeginOutputReadLine();
                process.WaitForExit();

                this._completionTime = DateTime.Now;
                if (process.ExitCode == 0)
                {
                    this._status = 1;
                    Console.WriteLine("Extraction completed.");
                }
                else
                {
                    this._status = 5;
                    Console.WriteLine("Extraction Failed.");
                }
            }
            else if (zippedFile.FullName.EndsWith(".zip") || zippedFile.FullName.EndsWith(".7z")
                     || zippedFile.FullName.EndsWith(".gz")
            )
            {

                string rarFilePath = zippedFile.FullName.Replace("\\", "//");
                string targetPath = tempDir.FullName.Replace("\\", "//");

                string sevenZipPath = ExternalResourceManager.getResourcePath(ExternalResourceType.SevenZip);
                string command = $"x \"{rarFilePath}\" -o\"{targetPath}\"";

                Process process = new Process();
                process.StartInfo.FileName = sevenZipPath;
                process.StartInfo.Arguments = command;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.CreateNoWindow = true;

                this._compressionTime = DateTime.Now;

                process.OutputDataReceived += (sender, e) => Console.WriteLine(e.Data);
                process.ErrorDataReceived += (sender, e) => Console.WriteLine(e.Data);

                process.Start();
                process.BeginOutputReadLine();
                process.WaitForExit();

                this._completionTime = DateTime.Now;
                if (process.ExitCode == 0)
                {
                    this._status = 1;
                    Console.WriteLine("Extraction completed.");
                }
                else
                {
                    this._status = 5;
                    Console.WriteLine("Extraction Failed.");
                }
            }
            UpdateCompressionTable();
            //File.Delete(zippedFile.FullName);
        }

        public void UpdateCompressionTable()
        {
            //string connectionString = "server=localhost;User Id=root;password=;Persist Security Info=True;database=mnh_cas";

            using (MySqlConnection connection = new MySqlConnection(ConStr))
            {
                connection.Open();
                string createTable = @"CREATE TABLE IF NOT EXISTS compressedfile (
                    id bigint(20) NOT NULL AUTO_INCREMENT,
                    compressedfileName varchar(200) COLLATE utf8mb4_bin DEFAULT NULL,
                    Status int(11) NOT NULL,
                    Progress bigint(20) DEFAULT NULL,
                    compressionTime datetime DEFAULT NULL,
                    CompletionTime datetime DEFAULT NULL,
                    NoOfSteps int(11) DEFAULT NULL,
                    Error text COLLATE utf8mb4_bin,
                    PRIMARY KEY(id),
                    CONSTRAINT `com_ibfk_3` FOREIGN KEY (`Status`) REFERENCES `enumjobstatus` (`id`)
                    ) ENGINE = InnoDB AUTO_INCREMENT = 382 DEFAULT CHARSET = utf8mb4 COLLATE = utf8mb4_bin; ";

                string insert = $@"INSERT INTO compressedfile (compressedfileName, Status, compressionTime, CompletionTime) 
                                    VALUES ('{zippedFile.Name}',{this._status},{this._compressionTime.ToMySqlFormatWithQuote()}, {this._completionTime.ToMySqlFormatWithQuote()});";

                using (MySqlCommand command = new MySqlCommand(createTable, connection))
                {
                    command.ExecuteNonQuery();

                    command.CommandText = insert;

                    command.ExecuteNonQuery();
                }
            }
        }
    }
}