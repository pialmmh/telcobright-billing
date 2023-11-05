using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;
using LibraryExtensions;

using SharpCompress.Archives;
using SharpCompress.Common;
using SharpCompress.Readers;

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
            else if (zippedFile.FullName.EndsWith("tar.Z"))
            {
                string extension = Path.GetExtension(zippedFile.Name);
                string compressedFileName = zippedFile.Name.Substring(0, zippedFile.Name.Length - extension.Length); ;
                string tempFileName = tempDir.FullName + Path.DirectorySeparatorChar + compressedFileName;

                string tarGzFilePath = tempFileName; // Replace with the path to your .tar.gz file
                string extractPath = tempDir.FullName;  // Replace with the directory where you want to extract the contents

                using (Stream stream = File.OpenRead(zippedFile.FullName))
                using (var reader = ReaderFactory.Open(stream))
                {
                    while (reader.MoveToNextEntry())
                    {
                        if (!reader.Entry.IsDirectory)
                        {
                            reader.WriteEntryToDirectory(extractPath, new ExtractionOptions
                            {
                                ExtractFullPath = true,
                                Overwrite = true
                            });
                        }
                    }
                }

                Console.WriteLine("Extraction completed.");


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
