using System;
using System.Collections.Generic;
using WinSCP;
using System.IO;
using System.Linq;
using LibraryExtensions;
using TelcobrightMediation;

namespace TelcobrightFileOperations
{
    public class DirectoryLister
    {
        List<RemoteFileInfo> GetFilesOnlyWithoutFolders(RemoteDirectoryInfo directoryInfo)
        {
            List<RemoteFileInfo> filesOnly = new List<RemoteFileInfo>();
            foreach (RemoteFileInfo fileInfo in directoryInfo.Files)
            {
                if (!fileInfo.IsDirectory)
                {
                    filesOnly.Add(fileInfo);
                }
            }
            return filesOnly;
        }

        List<RemoteFileInfo> GetFoldersOnly(RemoteDirectoryInfo directoryInfo)
        {
            List<RemoteFileInfo> foldersOnly = new List<RemoteFileInfo>();
            foreach (RemoteFileInfo fileInfo in directoryInfo.Files)
            {
                if (fileInfo.IsDirectory)
                {
                    if ((fileInfo.Name != ".") && (fileInfo.Name != ".."))
                    {
                        foldersOnly.Add(fileInfo);
                    }
                }
            }
            return foldersOnly;
        }

        bool HasSubDirectory(RemoteDirectoryInfo directoryInfo)
        {
            foreach (RemoteFileInfo fileInfo in directoryInfo.Files)
            {
                if ((fileInfo.Name == ".") || (fileInfo.Name == "..")) continue;
                if (fileInfo.IsDirectory)
                {
                    return true;
                }
            }
            return false;
        }

        public List<RemoteFileInfoExt> ListRemoteDirectoryRecursive(Session session, string relativePath)
        {
            List<RemoteFileInfo> remoteFiles = new List<RemoteFileInfo>();
            RemoteDirectoryInfo directoryInfo = session.ListDirectory(relativePath);
            List<RemoteFileInfo> filesOnly = GetFilesOnlyWithoutFolders(directoryInfo);
            List<RemoteFileInfoExt> fileInfoExtOnly =
                filesOnly.Select(f => new RemoteFileInfoExt(f, relativePath)).ToList();
            if (HasSubDirectory(directoryInfo) == false) return fileInfoExtOnly;
            List<RemoteFileInfo> foldersOnly = GetFoldersOnly(directoryInfo);
            foreach (RemoteFileInfo subDir in foldersOnly)
            {
                string subDirRelativePath = !relativePath.EndsWith("/")
                    ? relativePath + "/" + subDir.Name + "/"
                    : relativePath + subDir.Name + "/";
                fileInfoExtOnly.AddRange(ListRemoteDirectoryRecursive(session, subDirRelativePath));
            }
            return fileInfoExtOnly;
        }

        public List<RemoteFileInfo> ListRemoteDirectoryNonRecursive(Session session, string relativePath)
        {
            List<RemoteFileInfo> remoteFiles = new List<RemoteFileInfo>();
            string path = relativePath.IsNullOrEmptyOrWhiteSpace() ? "/" : relativePath;
            RemoteDirectoryInfo directoryInfo = session.ListDirectory(path);
            foreach (RemoteFileInfo fileInfo in directoryInfo.Files)
            {
                string remoteFilePath = relativePath + "/" + fileInfo.Name;
                if (!fileInfo.IsDirectory)
                {
                    remoteFiles.Add(fileInfo);
                }
            }
            return remoteFiles;
        }

        public List<FileInfo> ListLocalDirectoryRecursive(string dir)
        {
            List<FileInfo> localFiles = new List<FileInfo>();
            foreach (string f in Directory.GetFiles(dir))
            {
                localFiles.Add(new FileInfo(f));
            }
            foreach (string d in Directory.GetDirectories(dir))
            {
                ListLocalDirectoryRecursive(d);
            }
            return localFiles;
        }

        public List<FileInfo> ListLocalDirectoryNonRecursive(string dir)
        {
            List<FileInfo> localFiles = new List<FileInfo>();
            foreach (string f in Directory.GetFiles(dir))
            {
                localFiles.Add(new FileInfo(f));
            }
            return localFiles;
        }

        public List<FileInfo> ListLocalDirectoryZipfileNonRecursive(string dir, List<string> extensions)
        {
            List<FileInfo> zipFiles = new List<FileInfo>();
            
            foreach (string f in Directory.GetFiles(dir))
            {
                if (extensions.Any(e => f.EndsWith(e, StringComparison.OrdinalIgnoreCase)))
                {
                        zipFiles.Add(new FileInfo(f));

                }
            }
            List<FileInfo> templist = zipFiles ;//add logic to check if this file already exists in job table
            zipFiles = new List<FileInfo>();
            FileAndPathHelperMutable pathHelper = new FileAndPathHelperMutable();
            foreach (FileInfo fileInfo in templist)
            {
                if (pathHelper.IsFileLockedOrBeingWritten(fileInfo) == false)
                {
                    zipFiles.Add(fileInfo);
                }
            }
            return zipFiles;
        }
    }
}