using System.Collections.Generic;
using WinSCP;
using System.IO;
namespace TelcobrightFileOperations
{
    class DirectoryLister
    {
        public List<RemoteFileInfo> ListRemoteDirectoryRecursive(Session session, string relativePath)
        {
            List<RemoteFileInfo> remoteFiles = new List<RemoteFileInfo>();
            RemoteDirectoryInfo directoryInfo = session.ListDirectory(relativePath);
            foreach (RemoteFileInfo fileInfo in directoryInfo.Files)
            {
                string remoteFilePath = relativePath + "/" + fileInfo.Name;
                if (fileInfo.IsDirectory)
                {
                    // Skip references to current and parent directories
                    if ((fileInfo.Name != ".") &&
                        (fileInfo.Name != ".."))
                    {
                        ListRemoteDirectoryRecursive(session, remoteFilePath);
                    }
                }
                else
                {
                    remoteFiles.Add(fileInfo);
                }
            }
            return remoteFiles;
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
