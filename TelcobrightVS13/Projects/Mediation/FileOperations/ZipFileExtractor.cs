using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;
using LibraryExtensions;

namespace TelcobrightFileOperations
{
    public class UnZipper
    {
        private String zipPath;
        private string extractPath;
        FileInfo zippedFile;
        public UnZipper(String zipPath, String extractPath = "")
        {
            this.zippedFile = new FileInfo(zipPath);
            this.extractPath = (extractPath == "") ? zippedFile.DirectoryName : extractPath;
        }

        public void UnZip()
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
                else
                {
                    ZipFile.ExtractToDirectory(zippedFile.FullName, tempDir.ToString());
                }

                File.Delete(zippedFile.FullName);

                DirectoryInfo dir = new DirectoryInfo(tempDir.ToString());

                    try
                    {
                        foreach (FileInfo file in dir.GetFiles("*.*", SearchOption.AllDirectories))
                        {
                            file.MoveTo(this.extractPath + "\\" + file.Name);
                        }
                        //File.Delete(unzippedDir.FullName);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }
        }
    }
}
