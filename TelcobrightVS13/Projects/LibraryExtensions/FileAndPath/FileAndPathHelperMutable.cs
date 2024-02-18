using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryExtensions
{
    public class FileAndPathHelperMutable//don't make static, let there be a new instance for thread safety
    {
        public bool IsFileLockedOrBeingWritten(FileInfo file)
        {
            FileStream stream = null;
            try
            {

                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                stream.Close();
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }
        public string readTextFromCompressedFile(string fileName)
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


        public void DeleteFileContaining(string targetDirectory, string wildcard)
        {
            string searchPattern = string.Format("*{0}*", wildcard);
            var filesToDelete = Directory.EnumerateFiles(targetDirectory, searchPattern);
            foreach (var fileToDelete in filesToDelete)
            {
                File.Delete(fileToDelete);
            }
        }
    }
}
