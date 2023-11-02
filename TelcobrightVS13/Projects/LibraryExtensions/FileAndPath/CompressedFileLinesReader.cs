using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LibraryExtensions
{
    public class CompressedFileLinesReader
    {
        public FileInfo CompressedFileInfo { get;}
        public string ExtractedTempFileName { get; }
        public DirectoryInfo ExtractedTempDir { get; }
        public CompressedFileLinesReader(string compressedFileName)
        {
            this.CompressedFileInfo = new FileInfo(compressedFileName);
            this.ExtractedTempDir = new DirectoryInfo("tempcdr" + compressedFileName.GetHashCode());

            if (!Directory.Exists(this.ExtractedTempDir.Name))
            {
                Directory.CreateDirectory(this.ExtractedTempDir.Name);
            }
            else
            {
                this.ExtractedTempDir.DeleteContentRecusively();
            }
            this.ExtractedTempFileName = this.ExtractedTempDir.Name + Path.DirectorySeparatorChar +
                                         Path.GetFileNameWithoutExtension(this.CompressedFileInfo.FullName);

        }

        public string[] readLinesFromCompressedFile()
        {
            // Extract the .gz file into the temporary file
            using (FileStream gzFileStream = File.OpenRead(this.CompressedFileInfo.FullName))
            {
                using (FileStream tempFileStream = File.Create(this.ExtractedTempFileName))
                {
                    using (GZipStream gzipStream = new GZipStream(gzFileStream, CompressionMode.Decompress))
                    {
                        gzipStream.CopyTo(tempFileStream);
                    }
                }
            }
            // Read all lines from the temporary file
            string[] lines = File.ReadAllLines(this.ExtractedTempFileName);
            File.Delete(this.ExtractedTempFileName);
            Directory.Delete(this.ExtractedTempDir.FullName,true);
            return lines;
        }
    }
}
