using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryExtensions
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    namespace LibraryExtensions
    {
        public class VaultFileMover
        {
            public string RootDir;
            public List<string> Prefixes;
            public string Extension;

            public List<FileInfo> AllFileInfos { get; } = new List<FileInfo>();
            public List<FileInfo> ZipFileInfos { get; } = new List<FileInfo>();
            public List<FileInfo> CdrFileInfos { get; } = new List<FileInfo>();
            public List<FileInfo> UnwantedFileInfos { get; } = new List<FileInfo>();
            private HashSet<string> Extensions = new HashSet<string> { ".gz", ".7z", ".zip", ".tar" };
            private HashSet<string> TempFileExtensionsToAlwaysExclude = new HashSet<string> { ".tmp", ".filepart" };
            public VaultFileMover(List<string> prefixes, string extension, string rootDir)
            {
                this.Prefixes = prefixes;
                this.Extension = extension;
                this.RootDir = rootDir;
                populateAllFilesRecursively(RootDir);   // this will populate the AllFileInfos member variable
                AllFileInfos = AllFileInfos.Where(f => TempFileExtensionsToAlwaysExclude.Contains(f.Extension) == false)
                    .ToList();
                populateZipFiles();
                populateCdrFiles(Prefixes, Extension);
                populateUnwantedFile();
            }

            private void populateAllFilesRecursively(string parentDir)
            {
                string[] filePahts = Directory.GetFiles(parentDir);
                foreach (string filePath in filePahts)
                {
                    FileInfo fileInfo = new FileInfo(filePath);
                    AllFileInfos.Add(fileInfo);
                }

                string[] subDirs = Directory.GetDirectories(parentDir);
                foreach (string subDir in subDirs)
                {
                    if (!Directory.GetFileSystemEntries(subDir).Any())   // if a sub directory is empty then it will be deleted
                    {
                        Directory.Delete(subDir);
                        continue;
                    }
                    populateAllFilesRecursively(subDir);
                }
            }

            void populateZipFiles()
            {
                foreach (FileInfo fileInfo in AllFileInfos)
                {
                    if (Extensions.Contains(fileInfo.Extension))
                    {
                        ZipFileInfos.Add(fileInfo);
                    }
                }
            }

            void populateCdrFiles(List<string> prefixes, string extension)
            {
                foreach (FileInfo fileInfo in AllFileInfos)
                {
                    if (fileInfo.Extension == "")
                    {

                    }
                    if (!extension.IsNullOrEmptyOrWhiteSpace())
                    {
                        
                        if (prefixes.Any(pref => fileInfo.Name.StartsWith(pref)) && fileInfo.Extension == extension)
                        {
                            CdrFileInfos.Add(fileInfo); // this will populate the CdrFileInfos member variable
                        }
                    }
                    else
                    {
                        if (prefixes.Any(pref => fileInfo.Name.StartsWith(pref)))
                        {
                            CdrFileInfos.Add(fileInfo); // this will populate the CdrFileInfos member variable
                        }
                    }
                }
            }

            void populateUnwantedFile()
            {
                foreach (FileInfo fileInfo in AllFileInfos)
                {
                    if (!ZipFileInfos.Contains(fileInfo) && !CdrFileInfos.Contains(fileInfo))
                    {
                        UnwantedFileInfos.Add(fileInfo);
                    }
                }
            }

            public void moveFiles(string targetDir, List<FileInfo> filesToMove)
            {
                foreach (FileInfo fileInfo in filesToMove)
                {
                    string originalFile = Path.Combine(fileInfo.DirectoryName, fileInfo.Name);
                    string tmpFile = Path.Combine(targetDir, fileInfo.Name + ".tmp");

                    File.Copy(originalFile, tmpFile);      // tmp file copying here

                    FileInfo originalFileInfo = new FileInfo(originalFile);
                    FileInfo tempFileInfo = new FileInfo(tmpFile);

                    if (originalFileInfo.Length == tempFileInfo.Length)
                    {
                        File.Delete(originalFile);          //deleting original file
                        File.Move(tmpFile, tmpFile.Remove(tmpFile.Length - 4, 4));  //renaming tmp file to its original name
                    }
                }
            }
        }
    }

}
