using System.Collections.Generic;
using WinSCP;
using System.IO;
using System.Linq;
namespace TelcobrightFileOperations
{
    class DirectoryLister
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
                    if ((fileInfo.Name != ".") && (fileInfo.Name != "..")) {
                        foldersOnly.Add(fileInfo);
                    }
                }
            }
            return foldersOnly;
        }
        bool HasSubDirectory(RemoteDirectoryInfo directoryInfo) {
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
            List<RemoteFileInfoExt> fileInfoExtOnly = filesOnly.Select(f => new RemoteFileInfoExt(f,relativePath)).ToList();
            if (HasSubDirectory(directoryInfo) == false) return fileInfoExtOnly;
            List<RemoteFileInfo> foldersOnly = GetFoldersOnly(directoryInfo);
            foreach (RemoteFileInfo subDir in foldersOnly) {
                string subDirRelativePath = relativePath + subDir.Name + "/";
                fileInfoExtOnly.AddRange(ListRemoteDirectoryRecursive(session, subDirRelativePath));
            }
            return fileInfoExtOnly;
        }
        public List<RemoteFileInfo> ListRemoteDirectoryNonRecursive(Session session, string relativePath)
        {
            List<RemoteFileInfo> remoteFiles = new List<RemoteFileInfo>();
            RemoteDirectoryInfo directoryInfo = session.ListDirectory(relativePath);
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
    }
}
