using System;
using System.Diagnostics;
using System.IO;

namespace LogFileCompressor
{
    class Program
    {
        static void Main(string[] args)
        {
            // Input source directory and destination directory from user
            Console.WriteLine("Enter the source directory containing .log files:");
            string sourceDirectory = Console.ReadLine();

            Console.WriteLine("Enter the destination directory for compressed .log.gz files:");
            string destinationDirectory = Console.ReadLine();

            // Ensure the source directory exists
            if (!Directory.Exists(sourceDirectory))
            {
                Console.WriteLine("Source directory does not exist.");
                return;
            }

            // Ensure the destination directory exists, if not create it
            if (!Directory.Exists(destinationDirectory))
            {
                Directory.CreateDirectory(destinationDirectory);
            }

            // Get all .log files in the source directory
            string[] logFiles = Directory.GetFiles(sourceDirectory, "*.log");

            // Compress each file using 7zip
            foreach (string logFile in logFiles)
            {
                string fileName = Path.GetFileName(logFile); // Keep the .log extension
                string compressedFilePath = Path.Combine(destinationDirectory, fileName + ".gz");

                // Create the 7zip command
                ProcessStartInfo processInfo = new ProcessStartInfo
                {
                    FileName = "C:\\sftproot\\TelcobrightProject\\7z.exe", // Path to 7-Zip executable
                    Arguments = $"a -tgzip \"{compressedFilePath}\" \"{logFile}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                // Start the process
                using (Process process = Process.Start(processInfo))
                {
                    process.WaitForExit();
                    Console.WriteLine($"Compressed: {logFile} -> {compressedFilePath}");
                }
            }

            Console.WriteLine("Compression completed.");
        }
    }
}
