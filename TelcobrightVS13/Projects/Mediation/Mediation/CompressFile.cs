using System;
using System.Collections.Generic;
using System.Diagnostics;
using LibraryExtensions;

namespace TelcobrightFileOperations
{
    public class FileCompressor
    {
        public string GetFileExtensionByCompressionType(CompressionType compType)
        {
            switch (compType)
            {
                case CompressionType.None:
                    return "";
                case CompressionType.Gzip:
                    return ".gzip";
                case CompressionType.Sevenzip:
                    return ".7z";
                case CompressionType.Zip:
                    return ".zip";
            }
            return "";
        }
        public string CompressFile(CompressionType compressionType, string sourceFileOrFolder, string targetArchivePathWithFileExtension, int compressionLevel)
        {
            string compressionString = "7z";
            Process p = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = "7z.exe",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            switch (compressionType)
            {
                case CompressionType.None:
                    throw new Exception("Compression Type not set!");
                    break;
                case CompressionType.Sevenzip:
                    compressionString = "7z";
                    p.StartInfo.Arguments = "a -t" + compressionString + " \"" + targetArchivePathWithFileExtension + "\" \"" +
                sourceFileOrFolder + "\" -mx=" + compressionLevel;
                    break;
                case CompressionType.Gzip:
                    throw new Exception("Compression Type not implemented yet!");
                    break;
                case CompressionType.Zip:
                    compressionString = "zip";
                    p.StartInfo.Arguments = "a -t" + compressionString + " \"" + targetArchivePathWithFileExtension + "\" \"" +
                sourceFileOrFolder + "\" -mx=" + compressionLevel;
                    break;
            }

            p.Start();
            string sevenZipsOkSignal = "Everything is Ok";
            bool success = false;
            List<string> lstOutput = new List<string>();
            while (!p.StandardOutput.EndOfStream)
            {
                string line = p.StandardOutput.ReadLine();
                lstOutput.Add(line);
                if (line.Contains(sevenZipsOkSignal))
                {
                    success = true;
                }
            }
            if (success == true)
            {
                return "OK";
            }
            else
            {
                return string.Join(Environment.NewLine, lstOutput);
            }
        }
    }
}
