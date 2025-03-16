using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Threading;
using LibraryExtensions;
using TelcobrightMediation;
using TelcobrightMediation.Config;

namespace WatchDog
{
    class WatchDog
    {
        static string logFilePath = @"C:\Logs\CdrProcessLog.txt";
        static readonly string topshelfDir = new UpwordPathFinder<DirectoryInfo>("WS_Topshelf_Quartz").FindAndGetFullPath();
        static DateTime lastModifiedTime;
        static string exeToRestart = Path.Combine(topshelfDir, @"bin\Debug\WS_Telcobright_Topshelf.exe");

        static void Main(string[] args)
        {
            string configFilePath = Path.Combine(topshelfDir, "deployedInstances");
            string configFileName = Directory.GetFiles(configFilePath, "*.conf", SearchOption.AllDirectories).First();
            TelcobrightConfig tbc = ConfigFactory.GetConfigFromFile(configFileName);

            ExeRestartType restartType = tbc.CdrSetting.WatchDogRestartRule;
            switch (restartType)
            {
                case ExeRestartType.None:
                    break;
                case ExeRestartType.EveryMorning:
                    Timer timer2 = new Timer(CheckingUpdate, null, 0, 60000);
                    Console.ReadLine();
                    break;
                case ExeRestartType.AfterProcessingStop:
                    lastModifiedTime = File.GetLastWriteTime(logFilePath);
                    FileSystemWatcher watcher = new FileSystemWatcher();
                    watcher.Path = Path.GetDirectoryName(logFilePath);
                    watcher.Filter = Path.GetFileName(logFilePath);
                    watcher.NotifyFilter = NotifyFilters.LastWrite;

                    watcher.Changed += OnLogFileChanged;
                    watcher.EnableRaisingEvents = true;

                    Timer timer = new Timer(CheckLogFileUpdate, null, 0, 30000);

                    Console.WriteLine("Monitoring the log file. Press [Enter] to exit...");
                    Console.ReadLine();
                    break;
            }



        }

        static void OnLogFileChanged(object sender, FileSystemEventArgs e)
        {
            lastModifiedTime = File.GetLastWriteTime(logFilePath);
            Console.WriteLine($"Log file updated at: {lastModifiedTime}");
        }

        static void CheckLogFileUpdate(object state)
        {
            DateTime currentTime = DateTime.Now;

            if ((currentTime - lastModifiedTime).TotalMinutes >= 10)
            {
                Console.WriteLine("Log file hasn't been updated in the last 30 minutes. Restarting the executable...");
                RestartExecutable();
            }
        }

        static void CheckingUpdate(object state)
        {
            // Check if the current time is 6:00 AM
            DateTime currentTime = DateTime.Now;
            if (currentTime.Hour == 06 && currentTime.Minute == 00)
            {
                RestartExecutable();
            }
        }

        static void RestartExecutable()
        {
            try
            {

                string processName = Path.GetFileNameWithoutExtension(exeToRestart);

                foreach (var process in Process.GetProcessesByName(processName))
                {
                    Console.WriteLine($"Killing process: {process.ProcessName}, ID: {process.Id}");
                    process.Kill();
                    process.WaitForExit();
                }

                // Start the process again
                ProcessStartInfo processStartInfo = new ProcessStartInfo(exeToRestart)
                {
                    UseShellExecute = true,
                    CreateNoWindow = false
                };

                Process processToStart = new Process
                {
                    StartInfo = processStartInfo
                };

                processToStart.Start();
                Console.WriteLine($"Restarted {exeToRestart} successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error restarting executable: {ex.Message}");
            }
        }
    }
}
