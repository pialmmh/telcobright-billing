using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WinSCP;
using TelcobrightFileOperations;
using MediationModel;
using TelcobrightMediation.Config;

namespace TelcobrightMediation
{

    public class Vault
    {
        public string Name { get; set; }
        public SyncLocation LocalLocation { get; set; }
        public Dictionary<string, SyncLocation> FtpLocationsAll { get; set; }//serverid.tostring(),sl
        public List<SyncLocation> RemoteFtpLocations { get; set; }
        private TelcobrightConfig Tbc { get; set; }
        public Vault(string name, TelcobrightConfig telcobrightConfig, List<FileLocation> ftpLocations)
        {
            //json deserialize auto calls this, so skip below when null, tbc yet to be initialized
            this.Name = name;
            this.Tbc = telcobrightConfig;
            string localFtpServerName = "";
            if (this.Tbc != null)
            {
                localFtpServerName = "AppServerFTP" + this.Tbc.ServerId;
            }

            this.FtpLocationsAll = new Dictionary<string, SyncLocation>();
            this.RemoteFtpLocations = new List<SyncLocation>();
            if (ftpLocations != null)
            {
                foreach (FileLocation fileloc in ftpLocations)
                {
                    SyncLocation thisSyncLocation = new SyncLocation(fileloc.Name) { FileLocation = fileloc };
                    this.FtpLocationsAll.Add(fileloc.Name, thisSyncLocation);
                    if (fileloc.Name != localFtpServerName)
                    {
                        this.RemoteFtpLocations.Add(thisSyncLocation);
                    }
                }
            }
        }

        public List<FileInfo> GetFileListLocal()
        {
            Dictionary<string, FileInfo> fileNames = new Dictionary<string, FileInfo>();
            List<FileInfo> localFiles = this.LocalLocation.GetLocalFilesNonRecursive();
            return localFiles;
            //foreach (FileInfo lf in localFiles)
            //{
            //    fileNames.Add(lf.Name, lf);
            //}
            //return fileNames; //already sorted because dic key
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
                        this.Tbc.DatabaseSetting.DatabaseName);
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
                        if (fs.CopyFileRemoteLocal(dstSettings, rsl.GetRemoteFileTransferSession(this.Tbc), sourceInfo, destinationInfo, true, false,null) == true)
                        {
                            if (File.Exists(localInfo.FullPath))
                            {
                                return localInfo.FullPath;
                            }
                        }
                    }
                    catch (Exception e1)
                    {
                        Console.WriteLine(e1);
                        ErrorWriter wr = new ErrorWriter(e1, "GetSingleFileFromVault",null,"", this.Tbc.DatabaseSetting.DatabaseName);
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
                        fs.CopyFileLocalRemote(dstSettings, rsl.GetRemoteFileTransferSession(this.Tbc), sourceInfo, destinationInfo, false, false);
                    }
                    catch (Exception e1)
                    {
                        Console.WriteLine(e1);
                        ErrorWriter wr = new ErrorWriter(e1, "SyncOtherVaults",null,"", this.Tbc.DatabaseSetting.DatabaseName);
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
                if (File.Exists(fileName) == true) return false;
            }
            FileSyncher fs = new FileSyncher();
            foreach (SyncLocation rsl in this.RemoteFtpLocations.Where(c => c.FileLocation.Skip == false).ToList())
            {
                try
                {
                    FileSyncInfo destinationInfo = new FileSyncInfo(relativePath, rsl);
                    fileDeleted = fs.DeleteFileRemote(rsl.GetRemoteFileTransferSession(this.Tbc), destinationInfo);
                    //if for one location file cannot be delete, just return, jobqueue will try again
                    if (fileDeleted == false) return false;
                }
                catch (Exception e1)
                {
                    Console.WriteLine(e1);
                    ErrorWriter wr = new ErrorWriter(e1, "DeleteSingleFileFromVault",null,"", this.Tbc.DatabaseSetting.DatabaseName);
                    return false;
                }
            }
            return true;
        }

    }


    public class JobPreRequisite
    {
        public List<long> ExecuteAfterJobs { get; set; }//these idjobs have to be completed first
        public JobPreRequisite()
        {
            this.ExecuteAfterJobs = new List<long>();
        }
        public bool CheckComplete(PartnerEntities context)
        {
            return !context.jobs.Where(c => this.ExecuteAfterJobs.Contains(c.id)).Any(c => c.Status != 1);
        }
    }


    public class DirectorySettings : IConfigurationSection
    {
        public string SectionName { get; set; }
        public int SectionOrder { get; set; }
        public string ApplicationRootDirectory { get; set; }
        public List<Vault> Vaults { get; set; }=new List<Vault>();
        public Dictionary<string, FileLocation> FileLocations { get; set; }
        public Dictionary<string, SyncLocation> SyncLocations { get; set; }
        public Dictionary<string, SyncPair> SyncPairs { get; set; }
        public DirectorySettings(string SectionName)
        {
            this.SectionName = SectionName;
            this.SectionOrder = 1;
            this.FileLocations = new Dictionary<string, FileLocation>();
            this.SyncLocations = new Dictionary<string, SyncLocation>();
            this.SyncPairs = new Dictionary<string, SyncPair>();
        }
    }
    
    public class IisApplicationPool
    {
        public string AppPoolName { get; set; }
        public string TemplateFileName { get; set; }
        public Dictionary<string, string> GetDicProperties()
        {
            Dictionary<string, string> dicParam = new Dictionary<string, string>();
            dicParam.Add("appPoolName", this.AppPoolName);
            dicParam.Add("templateFileName", this.TemplateFileName);
            return dicParam;
        }
    }
    public class InternetSite
    {
        public string SiteType { get; set; }
        public string SiteName { get; set; }
        public int SiteId { get; set; }
        public string BindAddress { get; set; }
        public string PhysicalPath { get; set; }
        public string TemplateFileName { get; set; }
        public IisApplicationPool ApplicationPool { get; set; }
        public string ImpersonateUserName { get; set; }
        public string ImpersonatePassword { get; set; }
        public string ApplicationPoolName
        {
            get
            {
                return this.ApplicationPool.AppPoolName;
            }
        }
        public InternetSite(TelcobrightConfig tbc) {
            this.ApplicationPool = new IisApplicationPool(); }
        public Dictionary<string, string> GetDicProperties()
        {
            Dictionary<string, string> dicParam = new Dictionary<string, string>();
            dicParam.Add("siteType", this.SiteType);
            dicParam.Add("siteName", this.SiteName);
            dicParam.Add("siteId", this.SiteId.ToString());
            dicParam.Add("bindAddress", this.BindAddress);
            dicParam.Add("physicalPath", this.PhysicalPath);
            dicParam.Add("templateFileName", this.TemplateFileName);
            dicParam.Add("applicationPoolName", this.ApplicationPool.AppPoolName);
            return dicParam;
        }
    }
    public class PortalSettings
    {
        public string HomePageUrl { get; set; }
        public string AlternateDisplayName { get; set; }
        public List<InternetSite> PortalSites { get; set; }
        public PortalSettings(string sectionName)
        {
            this.PortalSites = new List<InternetSite>();
        }
        public PortalPageSettings PageSettings { get; set; }
        public Dictionary<string, object> DicConfigObjects { get; set; }
        public PortalSettings()
        {
            this.DicConfigObjects = new Dictionary<string, object>();
        }
        public Dictionary<string,int> RouteTypeEnums { get; set; }= new Dictionary<string, int>();
    }
    public class LogFileProcessorSetting
    {
        public List<string> BackupSyncPairNames { get; set; }//source side is always vault

        public bool DescendingOrderWhileListingFiles { get; set; }
        public LogFileProcessorSetting()
        {

        }
    }

    public enum SummaryTimeFieldEnum
    {
        StartTime=0,
        AnswerTime=1
    }

    public class SimpleCacheSettings
    {
        public Dictionary<string, string> SimpleCachedItemsToBePopulated { get; set; }//dic for fast lookup, 2nd string (val) has no use
        public SimpleCacheSettings() {
            this.SimpleCachedItemsToBePopulated = new Dictionary<string, string>(); }
    }
}
