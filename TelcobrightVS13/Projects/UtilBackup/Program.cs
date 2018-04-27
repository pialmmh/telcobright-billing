using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
namespace ProjectBackup
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                //MySqlDump("telcobrightmediation");
                //MySqlDump("platinum");
                try
                {
                    Directory.Delete(@"d:\dropbox\TelcobrightVS13_old", true);
                }
                catch (Exception e1)
                {
                    Console.WriteLine(e1);
                    Thread.Sleep(5000);
                    Directory.Delete(@"d:\dropbox\TelcobrightVS13_old", true);
                }
                Console.WriteLine("Old Solution Deleted. Renaming Last solution to old...");
                Directory.Move(@"d:\dropbox\TelcobrightVS13", @"d:\dropbox\TelcobrightVS13_old");
                Console.WriteLine("Renaming Complete, copying solution from desktop to dropbox...");
                DirectoryCopyExample dcopy = new DirectoryCopyExample(@"C:\Users\Telco\Desktop\TelcobrightVS13", @"d:\dropbox\TelcobrightVS13\");
                Console.Write("Copying complete, Job Successful!");
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            Console.Read();
        }


        static void MySqlDump(string databaseName)
        {
            string mySqlDumpFullPath = File.ReadAllLines("config.txt")[0].ToLower();
            string mySqlBinDirectory = mySqlDumpFullPath.Replace("mysqldump", "");

            Process myProcess = new Process();
            myProcess.StartInfo.FileName ="cmd.exe" ;
            myProcess.StartInfo.Arguments=File.ReadAllLines("config.txt")[0] +
                " -uroot -pTakay1#$ane " + databaseName +
                                   " >"+ mySqlBinDirectory + databaseName + DateTime.Now.ToString("yyyyMMddHHmmss") + ".sql";
            myProcess.StartInfo.UseShellExecute = false;
            myProcess.StartInfo.RedirectStandardInput = true;
            myProcess.StartInfo.RedirectStandardOutput = true;
            myProcess.StartInfo.ErrorDialog = false;

            myProcess.Start();

            
            // Wait for the process to finish.
            myProcess.WaitForExit();
            myProcess.Close();
        }

    }
    class DirectoryCopyExample
    {
        public DirectoryCopyExample(string src, string destination)
        {
            DirectoryCopy(src, destination, true);
        }
        
        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            // If the destination directory doesn't exist, create it. 
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            // If copying subdirectories, copy them and their contents to new location. 
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }
    }
}
