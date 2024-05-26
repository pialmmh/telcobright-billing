using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Process.helper
{
    class RamDiskHelper
    {
        private int sizeMb { get; set; }
        private char mountingPoint { get; set; }

        public RamDiskHelper(int sizeMb, char mountingPoint)
        {
            this.sizeMb = sizeMb;
            this.mountingPoint = mountingPoint;
        }

        public void Create()
        {
            if (CheckIfRamDiskExists())
            {
                Console.WriteLine("RAMDisk already exists.");
                return;
            }

            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo.FileName = "imdisk";
            process.StartInfo.Arguments = $"-a -t vm -s {sizeMb}M -m {mountingPoint}: -p \"/fs:ntfs /q /y\"";
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.UseShellExecute = false;

            try
            {
                process.Start();
                process.WaitForExit();

                if (process.ExitCode == 0)
                {
                    Console.WriteLine("RAMDisk created successfully.");
                }
                else
                {
                    Console.WriteLine($"Failed to create RAMDisk. Exit code: {process.ExitCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        public void Unmount()
        {
            if (!CheckIfRamDiskExists())
            {
                Console.WriteLine("RAMDisk does not exist. Cannot unmount.");
                return;
            }

            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo.FileName = "imdisk";
            process.StartInfo.Arguments = $"-D -m {mountingPoint}:";
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.UseShellExecute = false;

            try
            {
                process.Start();
                process.WaitForExit();

                if (process.ExitCode == 0)
                {
                    Console.WriteLine("RAMDisk unmounted successfully.");
                }
                else
                {
                    Console.WriteLine($"Failed to unmount RAMDisk. Exit code: {process.ExitCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while unmounting RAMDisk: {ex.Message}");
            }
        }

        private bool CheckIfRamDiskExists()
        {
            return System.IO.Directory.Exists($"{mountingPoint}:\\");
        }
    }
}
