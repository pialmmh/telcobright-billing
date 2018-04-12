using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryExtensions
{
    public static class FileAndPathHelper
    {
        public static void DeleteFileContaining(string targetDirectory, string wildcard)
        {
            string searchPattern = string.Format("*{0}*", wildcard);
            var filesToDelete = Directory.EnumerateFiles(targetDirectory, searchPattern);
            foreach (var fileToDelete in filesToDelete)
            {
                File.Delete(fileToDelete);
            }
        }

        public static string GetBinPath()
        {
            string binFolder = Path.GetDirectoryName(
                System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            binFolder = binFolder.Substring(6, binFolder.Length - 6);
            return binFolder;
        }
    }
}
