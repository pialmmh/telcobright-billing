using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryExtensions
{
    public static class FileAndPathHelper
    {
        public static string[] readLinesFromCompressedFile(string fileName)
        {
            string compressedFile = fileName;
            string tempFileName = Path.GetFileName(fileName) + Guid.NewGuid().ToString();

            // Extract the .gz file into the temporary file
            using (FileStream gzFileStream = File.OpenRead(compressedFile))
            {
                using (FileStream tempFileStream = File.Create(tempFileName))
                {
                    using (GZipStream gzipStream = new GZipStream(gzFileStream, CompressionMode.Decompress))
                    {
                        gzipStream.CopyTo(tempFileStream);
                    }
                }
            }
            // Read all lines from the temporary file
            string[] lines = File.ReadAllLines(tempFileName);
            // Delete the temporary file
            File.Delete(tempFileName);
            return lines;
        }

        public static string readTextFromCompressedFile(string fileName)
        {
            string compressedFile = fileName;
            string tempFilePath = "tmpFile.txt";

            // Extract the .gz file into the temporary file
            using (FileStream gzFileStream = File.OpenRead(compressedFile))
            {
                using (FileStream tempFileStream = File.Create(tempFilePath))
                {
                    using (GZipStream gzipStream = new GZipStream(gzFileStream, CompressionMode.Decompress))
                    {
                        gzipStream.CopyTo(tempFileStream);
                    }
                }
            }
            // Read all lines from the temporary file
            string allText = File.ReadAllText(tempFilePath);
            // Delete the temporary file
            File.Delete(tempFilePath);
            return allText;
        }


        public static void DeleteFileContaining(string targetDirectory, string wildcard)
        {
            string searchPattern = string.Format("*{0}*", wildcard);
            var filesToDelete = Directory.EnumerateFiles(targetDirectory, searchPattern);
            foreach (var fileToDelete in filesToDelete)
            {
                File.Delete(fileToDelete);
            }
        }

        public static string GetCurrentExecPath()
        {
            string binFolder = Path.GetDirectoryName(
                System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            binFolder = binFolder.Substring(6, binFolder.Length - 6);
            return binFolder;
        }
        public static string getBinPath()
        {
            string curDir= Path.GetDirectoryName(
                System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            curDir = curDir.Substring(6, curDir.Length - 6);
            if (curDir.EndsWith("bin")) return curDir;
            DirectoryInfo di = new DirectoryInfo(curDir).Parent;
            curDir = di.FullName;
            if (curDir.EndsWith("bin"))
            {
                return curDir;
            }
            else throw new Exception("Could not locate bin path.");
        }


    }
}
