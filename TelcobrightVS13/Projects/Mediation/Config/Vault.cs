using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WinSCP;
using TelcobrightFileOperations;
using TelcobrightMediation.Config;

namespace TelcobrightMediation
{
    public class Vault
    {
        public string Name { get; set; }
        public SyncLocation LocalLocation { get; set; }
        public List<SyncLocation> RemoteFtpLocations { get; set; }
        private TelcobrightConfig Tbc { get; set; }
        public Vault(string name, TelcobrightConfig telcobrightConfig)
        {
            //json deserialize auto calls this, so skip below when null, tbc yet to be initialized
            this.Name = name;
            this.Tbc = telcobrightConfig;
            string localFtpServerName = "";
            if (this.Tbc != null)
            {
                localFtpServerName = "AppServerFTP" + this.Tbc.ServerId;
            }
        }

        public List<FileInfo> GetFileListLocal()
        {
            Dictionary<string, FileInfo> fileNames = new Dictionary<string, FileInfo>();
            List<FileInfo> localFiles = this.LocalLocation.GetLocalFilesNonRecursive();
            return localFiles;
        }
        public List<string> GetFileListRemote()
        {
            Dictionary<string, string> fileNames = new Dictionary<string, string>();
            foreach (SyncLocation rsl in this.RemoteFtpLocations.Where(c => c.FileLocation.Skip == false).ToList())
            {
                try
                {
                    List<RemoteFileInfo> tempRemoteFiles = rsl.GetRemoteFilesNonRecursive(this.Tbc);
                    foreach (RemoteFileInfo rf in tempRemoteFiles)
                    {
                        if (fileNames.ContainsKey(rf.Name) == false)
                        {
                            fileNames.Add(rf.Name, "");
                        }
                    }
                }
                catch (Exception e1)
                {
                    Console.WriteLine(e1);
                    ErrorWriter wr = new ErrorWriter(e1, "GetFileListFromInsideClassVault", null, "",
                        this.Tbc.Telcobrightpartner.CustomerName);
                }
            }
            return fileNames.Keys.ToList(); //already sorted because dic key
        }
        public string GetSingleFile(FileSyncInfo localInfo)
        {
            //will return existing files path, if does not exist in this vault will copy from other vault to here locally
            //file will be searhed after flocation.url+suburl
            if (File.Exists(localInfo.FullPath))
            {
                return localInfo.FullPath;
            }
            else
            {
                SyncSettingsDest dstSettings = new SyncSettingsDest()
                {
                    FileExtensionForSafeCopyWithTempFile = ".tmp",
                    Overwrite = true,
                };
                //copy from remote ftp if not found locally
                FileSyncher fs = new FileSyncher();
                FileSyncInfo destinationInfo = new FileSyncInfo(localInfo.RelativePath, this.LocalLocation);
                foreach (SyncLocation rsl in this.RemoteFtpLocations.Where(c => c.FileLocation.Skip == false).ToList())
                {
                    try
                    {
                        FileSyncInfo sourceInfo = new FileSyncInfo(localInfo.RelativePath, rsl);
                        using (Session session = rsl.GetRemoteFileTransferSession(this.Tbc))
                        {
                            if (fs.CopyFileRemoteLocal(dstSettings, session, sourceInfo, destinationInfo, true, false, null) == true)
                            {
                                if (File.Exists(localInfo.FullPath))
                                {
                                    return localInfo.FullPath;
                                }
                            }
                        }
                    }
                    catch (Exception e1)
                    {
                        Console.WriteLine(e1);
                        ErrorWriter wr = new ErrorWriter(e1, "GetSingleFileFromVault",null,"", this.Tbc.Telcobrightpartner.CustomerName);
                    }
                }
            }
            return "";
        }
        public void SyncOthers(FileSyncInfo sourceInfo)//sync with other vaults
        {
            //don't use overwrite for this
            if (File.Exists(sourceInfo.FullPath) == true)
            {
                SyncSettingsDest dstSettings = new SyncSettingsDest()
                {
                    FileExtensionForSafeCopyWithTempFile = ".tmp",
                    Overwrite = false,
                };
                FileSyncher fs = new FileSyncher();
                foreach (SyncLocation rsl in this.RemoteFtpLocations.Where(c => c.FileLocation.Skip == false).ToList())
                {
                    try
                    {
                        FileSyncInfo destinationInfo = new FileSyncInfo(sourceInfo.RelativePath, rsl);
                        using (Session session = rsl.GetRemoteFileTransferSession(this.Tbc))
                        {
                            fs.CopyFileLocalRemote(dstSettings, session, sourceInfo, destinationInfo, false, false);
                        }
                    }
                    catch (Exception e1)
                    {
                        Console.WriteLine(e1);
                        ErrorWriter wr = new ErrorWriter(e1, "SyncOtherVaults",null,"", this.Tbc.Telcobrightpartner.CustomerName);
                    }
                }
            }
        }
        public bool DeleteSingleFile(string relativePath)//sync with other vaults
        {
            //don't use overwrite for this
            bool fileDeleted = false;
            var fileName = this.LocalLocation.FileLocation.StartingPath + Path.DirectorySeparatorChar + relativePath;
            if (File.Exists(fileName) == true)
            {
                File.Delete(fileName);
                string historyFileName = fileName+".history";
                if (File.Exists(historyFileName))
                {
                    File.Delete(historyFileName);
                }
                if (File.Exists(fileName) == true) return false;
            }
            FileSyncher fs = new FileSyncher();
            foreach (SyncLocation rsl in this.RemoteFtpLocations.Where(c => c.FileLocation.Skip == false).ToList())
            {
                try
                {
                    FileSyncInfo destinationInfo = new FileSyncInfo(relativePath, rsl);
                    using (Session session = rsl.GetRemoteFileTransferSession(this.Tbc))
                    {
                        fileDeleted = fs.DeleteFileRemote(session, destinationInfo);
                        //if for one location file cannot be delete, just return, jobqueue will try again
                        if (fileDeleted == false) return false;
                    }
                }
                catch (Exception e1)
                {
                    Console.WriteLine(e1);
                    ErrorWriter wr = new ErrorWriter(e1, "DeleteSingleFileFromVault",null,"", this.Tbc.Telcobrightpartner.CustomerName);
                    return false;
                }
            }
            return true;
        }

    }

}
