using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LibraryExtensions
{
    public static class FileSplitter
    {
        static List<string> splitedFileNames;
        public static List<string> SplitFile(FileInfo inputFile, int chunkSize)
        {
            splitedFileNames=new List<string>();
            const int BUFFER_SIZE = 20 * 1024;
            byte[] buffer = new byte[BUFFER_SIZE];

            using (Stream input = File.OpenRead(inputFile.FullName))
            {
                int index = 0;
                long expectedNoOfFiles = inputFile.Length / chunkSize;
                if (inputFile.Length % chunkSize > 0) expectedNoOfFiles++;
                while (input.Position < input.Length)
                {
                    string fileNumber = (index+1).ToString("0000");
                    string targetFileName = inputFile.DirectoryName+ Path.DirectorySeparatorChar
                               + Path.GetFileNameWithoutExtension(inputFile.Name)
                               +"_" + fileNumber+inputFile.Extension;
                    splitedFileNames.Add(targetFileName);
                    using (Stream output = File.Create(targetFileName))
                    {
                        int remaining = chunkSize, bytesRead;
                        while (remaining > 0 && (bytesRead = input.Read(buffer, 0,
                                   Math.Min(remaining, BUFFER_SIZE))) > 0)
                        {
                            output.Write(buffer, 0, bytesRead);
                            remaining -= bytesRead;
                        }
                    }
                    index++;
                    Console.WriteLine($"Spliting large cdr file. Progress {fileNumber} of {expectedNoOfFiles}.");
                    Thread.Sleep(500); // experimental; perhaps try it
                }
                return splitedFileNames;
            }
        }
    }
}
