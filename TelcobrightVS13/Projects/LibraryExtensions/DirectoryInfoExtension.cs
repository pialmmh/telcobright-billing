using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryExtensions
{
    public static class DirectoryInfoExtensions
    {
        public static void DeepCopy(this DirectoryInfo srcDir, string destinationDir)
        {
            foreach (string dir in Directory.GetDirectories(srcDir.FullName, "*", SearchOption.AllDirectories))
            {
                string dirToCreate = dir.Replace(srcDir.FullName, destinationDir);
                Directory.CreateDirectory(dirToCreate);
            }

            foreach (string newPath in Directory.GetFiles(srcDir.FullName, "*.*", SearchOption.AllDirectories))
            {
                File.Copy(newPath, newPath.Replace(srcDir.FullName, destinationDir), true);
            }
        }
    }
}
