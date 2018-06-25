using System.Collections.Generic;
using LibraryExtensions;
using WinSCP;
using System.IO;
using Spring.Expressions;
using System;
using System.Globalization;
using TelcobrightMediation;

namespace TelcobrightFileOperations
{
    public enum CompressionType
    {
        None = 0,
        Zip = 1,
        Sevenzip = 2,
        Gzip = 3
    }

    public class SpringExpression//need this to support json config files, parsed spring expressions loses their value when serialized
    {
        public string Expression { get; set; }
        public SpringExpression(string strExp)
        {
            this.Expression = strExp;
        }
        public IExpression GetParsedExpression()
        {
            if (this.Expression == null || this.Expression == "")
            {
                return null;
            }
            return Spring.Expressions.Expression.Parse(this.Expression);
        }
    }

    public class SyncLocation
    {
        public string Name { get; set; }
        public FileLocation FileLocation { get; set; }
        public bool DescendingFileListByFileName { get; set; }
        public Session FileTransferSession { get; set; }
        public SyncLocation(string name)
        {
            this.Name = name;
        }
        public string GetUrlWithSubUrl()
        {
            return (this.FileLocation.ServerIp + Path.DirectorySeparatorChar + this.FileLocation.StartingPath).Replace("/", Path.DirectorySeparatorChar.ToString());
        }
        public List<RemoteFileInfo> GetRemoteFilesNonRecursive(TelcobrightConfig tbc)
        {
            using (Session session = GetRemoteFileTransferSession(tbc))
            {
                DirectoryLister dirlister = new DirectoryLister();
                return dirlister.ListRemoteDirectoryNonRecursive(session, this.FileLocation.StartingPath);
            }
        }
        public List<RemoteFileInfo> GetRemoteFilesRecursive(TelcobrightConfig tbc)
        {
            using (Session session = GetRemoteFileTransferSession(tbc))
            {
                DirectoryLister dirlister = new DirectoryLister();
                return dirlister.ListRemoteDirectoryNonRecursive(session, this.FileLocation.StartingPath);
            }
        }
        public List<FileInfo> GetLocalFilesNonRecursive()
        {
            DirectoryLister dirlister = new DirectoryLister();
            return dirlister.ListLocalDirectoryNonRecursive(this.FileLocation.StartingPath);
        }
        public List<FileInfo> GetLocalFilesRecursive()
        {
            DirectoryLister dirlister = new DirectoryLister();
            return dirlister.ListLocalDirectoryRecursive(this.FileLocation.StartingPath);
        }
        public List<FileInfo> GetLocalFiles(SyncSettingsSource srcSettings)
        {
            List<FileInfo> tempFiles = new List<FileInfo>();
            if (srcSettings.Recursive == false)
            {
                tempFiles = GetLocalFilesNonRecursive();
            }
            else
            {
                tempFiles = GetLocalFilesRecursive();
            }
            List<FileInfo> filteredFiles = new List<FileInfo>();
            if (srcSettings.ExpFileNameFilter.Expression == "")
            {
                return tempFiles;
            }
            IExpression exp = srcSettings.ExpFileNameFilter.GetParsedExpression();
            foreach (FileInfo fileInfo in tempFiles)
            {
                if (fileInfo.GetBoolByExpression(exp) == true)
                {
                    filteredFiles.Add(fileInfo);
                }
            }
            return filteredFiles;
        }
        public List<RemoteFileInfo> GetRemoteFiles(SyncSettingsSource srcSettings, TelcobrightConfig tbc)
        {
            List<RemoteFileInfo> tempFiles = new List<RemoteFileInfo>();
            if (srcSettings.Recursive == false)
            {
                tempFiles = GetRemoteFilesNonRecursive(tbc);
            }
            else if (srcSettings.Recursive == true)
            {
                tempFiles = GetRemoteFilesRecursive(tbc);
            }

            if (srcSettings.ExpFileNameFilter.Expression == "")
            {
                return tempFiles;
            }
            List<RemoteFileInfo> filteredFiles = new List<RemoteFileInfo>();
            IExpression exp = srcSettings.ExpFileNameFilter.GetParsedExpression();
            foreach (RemoteFileInfo fileInfo in tempFiles)
            {
                if (fileInfo.GetBoolByExpression(exp) == true)
                {
                    filteredFiles.Add(fileInfo);
                }
            }
            return filteredFiles;
        }



        public List<string> GetFileNames(SyncSettingsSource srcSettings, TelcobrightConfig tbc)
        {
            //get list of files
            List<FileInfo> localFiles = new List<FileInfo>();
            List<RemoteFileInfo> remoteFiles = new List<RemoteFileInfo>();
            List<string> fileNames = new List<string>();
            switch (this.FileLocation.LocationType)
            {
                case "local":
                case "local-linux":
                    GetLocalFiles(srcSettings).ForEach(c => fileNames.Add(c.Name.Replace("\\", "/")));
                    break;
                case "sftp":
                case "ftp":
                    GetRemoteFiles(srcSettings,tbc).ForEach(c => fileNames.Add(c.Name.Replace("\\", "/")));
                    break;
            }
            return fileNames;
        }
        public List<string> GetFileNamesFiltered(SyncSettingsSource srcSettings, TelcobrightConfig tbc)
        {
            //get list of files
            List<FileInfo> localFiles = new List<FileInfo>();
            List<RemoteFileInfo> remoteFiles = new List<RemoteFileInfo>();
            List<string> fileNames = new List<string>();
            switch (this.FileLocation.LocationType)
            {
                case "local":
                case "local-linux":
                    GetLocalFiles(srcSettings).ForEach(c => fileNames.Add(c.Name.Replace("\\", "/")));
                    break;
                case "sftp":
                case "ftp":
                    GetRemoteFiles(srcSettings,tbc).ForEach(c => fileNames.Add(c.Name.Replace("\\", "/")));
                    break;
            }
            return fileNames;
        }
        public Session GetRemoteFileTransferSession(TelcobrightConfig tbc)
        {
            SessionOptions sessionOptions = new SessionOptions
            {
                Protocol = Protocol.Ftp,//default
                HostName = this.FileLocation.ServerIp,
                UserName = this.FileLocation.User,
                Password = this.FileLocation.Pass,
            };
            switch (this.FileLocation.LocationType)
            {
                case "ftp":
                    sessionOptions.Protocol = Protocol.Ftp;
                    break;
                case "sftp":
                    sessionOptions.Protocol = Protocol.Sftp;
                    if (string.IsNullOrEmpty(this.FileLocation.Sftphostkey))
                    {
                        sessionOptions.GiveUpSecurityAndAcceptAnySshHostKey = true;
                    }
                    else
                    {
                        sessionOptions.SshHostKeyFingerprint = this.FileLocation.Sftphostkey;
                    }
                    
                    break;
            }
            Session session = new Session();
            session.SessionLogPath = null;
            //session.DebugLogPath=
            session.Open(sessionOptions);
            this.FileTransferSession = session;
            //add this session to diccache
            //if (tbc.resourcePool.winscpSessionPool.ContainsKey(sessionKey)) tbc.resourcePool.winscpSessionPool.Remove(sessionKey);
            //tbc.resourcePool.winscpSessionPool.Add(sessionKey, fileTransferSession);
            return session;
        }

    }
    public class SyncSettingsSource
    {
        public SpringExpression ExpFileNameFilter { get; set; }
        public string SecondaryDirectory { get; set; }
        public bool MoveFilesToSecondaryAfterCopy { get; set; }
        public bool Recursive { get; set; }
    }
    public enum DateWiseSubDirCreationType
    {
        ByFileName,
        ByFileCreationDate
    }
    public class SpringParsingContextForNewFileName//to be used as spring context
    {
        public string Name { get; set; }
        public string GetStringByExpression(string srcFilename, IExpression exp)
        {
            this.Name = srcFilename;
            if (exp == null) return this.Name;
            return (string)exp.GetValue(this);
        }
    }
    public class SyncSettingsDstSubDirectoryRule
    {
        public bool MonthWiseGrouping { get; set; }
        public DateWiseSubDirCreationType DateWiseSubDirCreationType { get; set; }
        public SpringExpression ExpDatePartInFileName { get; set; }
        public string DateFormatInFileName { get; set; }
        public DateTime FileDate { get; set; }
        public SyncSettingsDstSubDirectoryRule()//default const, json.net stops without this
        {

        }
        public SyncSettingsDstSubDirectoryRule(DateWiseSubDirCreationType dwSubDirCreationType,
            SpringExpression ExpDatePartInFileName,
            string DateFormatInFileName,
            bool MonthWiseGrouping
            )
        {
            this.DateWiseSubDirCreationType = dwSubDirCreationType;
            this.ExpDatePartInFileName = ExpDatePartInFileName;
            this.DateFormatInFileName = DateFormatInFileName;
            this.MonthWiseGrouping = MonthWiseGrouping;
        }
        public SyncSettingsDstSubDirectoryRule(DateWiseSubDirCreationType dwSubDirCreationType,
            DateTime FileDate
            )
        {
            this.DateWiseSubDirCreationType = dwSubDirCreationType;
            this.FileDate = FileDate;
        }

        public string GetDateWiseArchiveFolderNameByFileName(string srcFileName, string pathSeparator)
        {
            DateTime dateInFileName = new DateTime();
            string dateStr = new SpringParsingContextForNewFileName().GetStringByExpression(srcFileName, this.ExpDatePartInFileName.GetParsedExpression());
            if (DateTime.TryParseExact(dateStr, this.DateFormatInFileName, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateInFileName) == false)
            {
                throw new Exception("Date parsing Error, could not extract datetime info from filename.");
            }
            //return dateInFileName.ToString("yyyy-MM-dd");
            return this.MonthWiseGrouping == false
                ? dateInFileName.ToString("yyyy-MM-dd")
                : dateInFileName.ToString("MMM-yyyy") + pathSeparator + dateInFileName.ToString("dd");
        }
        public string GetDateWiseArchiveFolderNameByCreationDate(string pathSeparator)
        {
            return this.MonthWiseGrouping == false
                ? this.FileDate.ToString("yyyy-MM-dd")
                : this.FileDate.ToString("MMM-yyyy") + pathSeparator + this.FileDate.ToString("dd");
        }
    }
    public class SyncSettingsDest
    {
        public CompressionType CompressionType { get; set; }
        public string FileExtensionForSafeCopyWithTempFile { get; set; }//string.empty means do not use safe copy
        public bool Overwrite { get; set; }
        public SpringExpression ExpDestFileName { get; set; }
        public SyncSettingsDstSubDirectoryRule SubDirRule { get; set; }



        public string GetDestinationFileNamebyExpression(string srcFileName, SyncSettingsDest syncSettingsWhenDestination, FileLocation dstFileLocation)
        {
            //if dstexpression is null, return filename as it is
            if (syncSettingsWhenDestination == null || syncSettingsWhenDestination.ExpDestFileName == null)
            {
                return srcFileName;
            }
            //use linux char path.diresepchar causes problem here
            return dstFileLocation.GetOsNormalizedPath(new SpringParsingContextForNewFileName().GetStringByExpression(srcFileName, syncSettingsWhenDestination.ExpDestFileName.GetParsedExpression()));
        }
    }
    public class SyncPair
    {
        //don't use constructor, will be populated from json config file
        public string Name { get; set; }
        public bool SkipCopyingToDestination { get; set; }//this is for directory sync process, has effect on autoarchivecdr
        public bool SkipSourceFileListing { get; set; }
        public SyncLocation SrcSyncLocation { get; set; }
        public SyncLocation DstSyncLocation { get; set; }
        public SyncSettingsSource SrcSettings { get; set; }
        public SyncSettingsDest DstSettings { get; set; }

        public SyncPair(string pairName)
        {
            this.Name = pairName;
            this.DstSyncLocation = this.DstSyncLocation;
            this.SrcSyncLocation = this.SrcSyncLocation;
        }

    }

    public class CdrArchive
    {
        //don't use constructor, will be populated from json config file
        public string Name { get; set; }
        public bool Skip { get; set; }//this is for directory sync process, has to effect on autoarchivecdr
        public SyncLocation DstSyncLocation { get; set; }
        public SyncSettingsDest DstSettings { get; set; }
        public CdrArchive(string pairName)
        {
            this.Name = pairName;
            this.DstSyncLocation = this.DstSyncLocation;
        }
    }




}
