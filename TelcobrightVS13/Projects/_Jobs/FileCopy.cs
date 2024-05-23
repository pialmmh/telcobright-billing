using TelcobrightMediation;
using Newtonsoft.Json;
using System.ComponentModel.Composition;
using System.IO;
using TelcobrightFileOperations;
using WinSCP;
using System;
using System.Linq;
using System.Collections.Generic;
using MediationModel;
using TelcobrightMediation.Config;
using System.Diagnostics;
using LibraryExtensions;

namespace Jobs
{
    public static class FileTransferSessionCache
    {
        //Tuple<string,string>= Tuple<src fileLocationName,serverIP, startingpath, protocol (e.g. ftp or sftp)>
        public static Dictionary<Tuple<string, string,string,string>, Session> sessionCache = new Dictionary<Tuple<string, string,string,string>, Session>();
        public static Dictionary<Tuple<string, string, string, string>, int> sessionCacheUsage = new Dictionary<Tuple<string, string, string, string>, int>();
    }

    [Export("Job", typeof(ITelcobrightJob))]
    public class FileCopy : ITelcobrightJob
    {
        public override string ToString() => this.RuleName;
        public string RuleName => "JobFileCopy";
        public string HelpText => "Copy File";
        public int Id => 6;
        public FileSyncInfo GetTempSyncInfo()
        {
            string tmpfileName = Path.GetTempPath() + Guid.NewGuid().ToString() + ".tbtemp";
            FileSyncInfo tempSyncInfo = new FileSyncInfo(null, null);
            tempSyncInfo.SyncLocation = new SyncLocation()
            {
                FileLocation = new FileLocation() { Name = "temp", LocationType = "local" },
            };
            tempSyncInfo.FullPath = tmpfileName;
            string[] tempArr = tempSyncInfo.FullPath.Split(Path.DirectorySeparatorChar);
            tempSyncInfo.FileNameOnly = tempArr[tempArr.GetLength(0) - 1];
            return tempSyncInfo;
        }
        public FileSyncInfo GetDummySyncInfoLocal(string fullPath)
        {
            //upload compressed file to dst remote server
            return new FileSyncInfo(null,
                new SyncLocation()
                {
                    FileLocation = new FileLocation()
                    {
                        LocationType = "local"
                    }
                })
            { FullPath = fullPath };
        }
        public object Execute(ITelcobrightJobInput jobInputData)
        {
            FileCopyJobInputData input = (FileCopyJobInputData)jobInputData;
            JobParamFileCopy paramFileCopy = new JobParamFileCopy();
            paramFileCopy = JsonConvert.DeserializeObject<JobParamFileCopy>
                (input.Job.JobParameter.Replace("unsplit\\", "unsplit`"));
            paramFileCopy.RelativeFileName = paramFileCopy.RelativeFileName.Replace("unsplit`", @"unsplit\");
            SyncPair syncPair = input.Tbc.DirectorySettings.SyncPairs[paramFileCopy.SyncPairName];
            SyncLocation srcLocation = syncPair.SrcSyncLocation;
            SyncLocation dstLocation = syncPair.DstSyncLocation;
            SyncSettingsSource syncSettingSrc = syncPair.SrcSettings;
            SyncSettingsDest syncSettingDst = syncPair.DstSettings;
            CompressionType compType = syncPair.DstSettings.CompressionType;
            FileSyncInfo sourceSyncInfo = new FileSyncInfo(paramFileCopy.RelativeFileName, srcLocation);
            string destinationFileNameOnly = syncSettingDst.GetDestinationFileNamebyExpression(sourceSyncInfo.FileNameOnly, syncPair.DstSettings, dstLocation.FileLocation);
            string archiveDir = "";
            if (syncSettingDst != null && syncSettingDst.SubDirRule != null && syncSettingDst.SubDirRule.ExpDatePartInFileName != null && syncSettingDst.SubDirRule.ExpDatePartInFileName.Expression != "")//datewise archive part
            {
                archiveDir = syncSettingDst.SubDirRule.GetDateWiseArchiveFolderNameByFileName(sourceSyncInfo.FileNameOnly,
                    dstLocation.FileLocation.GetPathSeparator().ToString());
            }
            string destinationRelativePath = archiveDir != "" ? (archiveDir + dstLocation.FileLocation.GetPathSeparator()
                + destinationFileNameOnly) : destinationFileNameOnly;
            if (syncSettingDst.PrefixForUniqueName != "")
            {
                destinationRelativePath = syncSettingDst.PrefixForUniqueName + destinationRelativePath;
            }
            FileSyncInfo destinationSyncInfo = new FileSyncInfo(destinationRelativePath, dstLocation);

            FileSyncher fs = new FileSyncher();
            //if source location type remote
            if (srcLocation.FileLocation.LocationType == "ftp" || srcLocation.FileLocation.LocationType == "sftp")
            {
                //if destination location type remote
                if (dstLocation.FileLocation.LocationType == "ftp" || dstLocation.FileLocation.LocationType == "sftp")
                {
                    //if same server, COPY WITHIN REMOTE
                    if (srcLocation.FileLocation.Name == dstLocation.FileLocation.Name)
                    {
                        if (compType == CompressionType.None)
                        {
                            using (Session session = srcLocation.GetRemoteFileTransferSession(input.Tbc))
                            {
                                if (fs.CopyFileInsideRemote(session, sourceSyncInfo, destinationSyncInfo, true) == false)
                                {
                                    throw new Exception("Could not copy file inside remote server!");
                                }
                            }
                        }
                        else//compression
                        {
                            //download to temp file first
                            FileSyncInfo tempInfo = GetTempSyncInfo();
                            using (Session session = srcLocation.GetRemoteFileTransferSession(input.Tbc))
                            {
                                if (fs.CopyFileRemoteLocal(syncPair.DstSettings, session, sourceSyncInfo, tempInfo, true, false, null) == false)
                                {
                                    throw new Exception("Could not copy file from remote server!");
                                }
                            }
                            //compress to temp compressed file
                            FileCompressor fCompressor = new FileCompressor();
                            string compressedFileNameWithExtesion = tempInfo.FullPath.Replace(".tbtemp", fCompressor.GetFileExtensionByCompressionType(compType));
                            string output = fCompressor.CompressFile(compType, tempInfo.FullPath,
                                compressedFileNameWithExtesion, 7);
                            if (output != "OK")//output has the whole output of the console session
                            {
                                throw new Exception("Error Compressing File!" + Environment.NewLine + output);
                            }
                            //upload compressed file
                            FileSyncInfo compressedInfo = GetDummySyncInfoLocal(compressedFileNameWithExtesion);
                            using (Session session = dstLocation.GetRemoteFileTransferSession(input.Tbc))
                            {
                                if (fs.CopyFileLocalRemote(syncPair.DstSettings, session, compressedInfo, destinationSyncInfo, true, false) == false)
                                {
                                    throw new Exception("Could not upload compressed file to remote server!");
                                }
                            }
                            //delete temp file
                            File.Delete(tempInfo.FullPath);
                            File.Delete(compressedInfo.FullPath);
                        }
                    }
                    //if not same server-> REMOTE1->REMOTE2
                    else
                    {
                        if (compType == CompressionType.None)
                        {
                            //copy to a temp file in local first
                            FileSyncInfo tempInfo = GetTempSyncInfo();
                            using (Session session = srcLocation.GetRemoteFileTransferSession(input.Tbc))
                            {
                                if (fs.CopyFileRemoteLocal(syncPair.DstSettings, session, sourceSyncInfo,
                                        tempInfo, true, false, syncSettingSrc) == false)
                                {
                                    throw new Exception("Could not copy file from remote server!");
                                }
                            }
                            //upload to dst remote server
                            using (Session session = dstLocation.GetRemoteFileTransferSession(input.Tbc))
                            {
                                if (fs.CopyFileLocalRemote(syncPair.DstSettings, session, tempInfo, destinationSyncInfo, true, false) == false)
                                {
                                    throw new Exception("Could not upload file to remote server!");
                                }
                                session.Close();
                            }
                            //delete temp file
                            File.Delete(tempInfo.FullPath);
                        }
                        else //REMOTE1->REMOTE2 +compression
                        {
                            //copy to a temp file in local first
                            FileSyncInfo tempInfo = GetTempSyncInfo();
                            using (Session session = srcLocation.GetRemoteFileTransferSession(input.Tbc))
                            {
                                if (fs.CopyFileRemoteLocal(syncPair.DstSettings, session, sourceSyncInfo, tempInfo, true, false, syncSettingSrc) == false)
                                {
                                    throw new Exception("Could not copy file from remote server!");
                                }
                            }
                            //compress to temp compressed file
                            FileCompressor fCompressor = new FileCompressor();
                            string compressedFileNameWithExtesion = tempInfo.FullPath.Replace(".tbtemp", fCompressor.GetFileExtensionByCompressionType(compType));
                            string output = fCompressor.CompressFile(compType, tempInfo.FullPath,
                                compressedFileNameWithExtesion, 7);
                            if (output != "OK")//output has the whole output of the console session
                            {
                                throw new Exception("Error Compressing File!" + Environment.NewLine + output);
                            }
                            //upload compressed file
                            FileSyncInfo compressedInfo = GetDummySyncInfoLocal(compressedFileNameWithExtesion);
                            using (Session session = dstLocation.GetRemoteFileTransferSession(input.Tbc))
                            {
                                if (fs.CopyFileLocalRemote(syncPair.DstSettings, session, compressedInfo, destinationSyncInfo, true, false) == false)
                                {
                                    throw new Exception("Could not upload compressed file to remote server!");
                                }
                            }
                            //delete temp file
                            File.Delete(tempInfo.FullPath);
                            File.Delete(compressedInfo.FullPath);
                        }
                    }
                }
                //if destination location type local: REMOTE->LOCAL or REMOTE->VAULT
                else
                {
                    SessionOptions sessionOptions = srcLocation.GetRemoteFileTransferSessionOptions(input.Tbc);
                    var sessionLookupKey = new Tuple<string, string,string,string>
                        (srcLocation.Name, srcLocation.FileLocation.ServerIp, srcLocation.FileLocation.StartingPath, sessionOptions.Protocol.ToString());
                    int sessionReOpeningInterval = srcLocation.FileLocation.FtpSessionCloseAndReOpeningtervalByFleTransferCount;//tyring to save winscp from crashing by closign it in every 100 attempts
                    Session session = null;
                    var sessionCache = FileTransferSessionCache.sessionCache;
                    var sessionCacheUsage = FileTransferSessionCache.sessionCacheUsage;
                    int sessionUsageCount = 0;
                    sessionCache.TryGetValue(sessionLookupKey, out session);
                    if (session != null)
                    {
                        sessionCacheUsage.TryGetValue(sessionLookupKey, out sessionUsageCount);

                        if (sessionUsageCount % sessionReOpeningInterval == 0)
                        {
                            sessionUsageCount = 0;
                            session.Abort();
                            session.Dispose();
                            session = null;
                            sessionCache.Remove(sessionLookupKey);
                            sessionCacheUsage.Remove(sessionLookupKey);
                            foreach (var process in Process.GetProcesses().Where(p =>
                            {
                                var processName = p.ProcessName.ToLower();
                                return processName.Contains("winscp") || processName.Contains("werfault");//kill winscp crashed window or windows process that is reporting the fault
                            }))
                            {
                                process.Kill();
                            }
                        }
                        else
                        {
                            sessionCacheUsage.Remove(sessionLookupKey);
                            sessionCacheUsage.Add(sessionLookupKey, ++sessionUsageCount);
                        }
                    }
                    if (session == null || session.Opened==false)
                    {
                        if (session?.Opened == false) {
                            session.Abort();
                            session.Dispose();
                            session = null;
                            sessionCache.Remove(sessionLookupKey);
                            sessionCacheUsage.Remove(sessionLookupKey);
                            foreach (var process in Process.GetProcesses().Where(p => p.ProcessName.ToLower().Contains("winscp")))
                            {
                                process.Kill();
                            }
                        }
                        session = new Session();
                        session.SessionLogPath = null;
                        session.Open(sessionOptions);
                        if (sessionCache.ContainsKey(sessionLookupKey)) {
                            sessionCache.Remove(sessionLookupKey);
                        }
                        sessionCache.Add(sessionLookupKey, session);
                        sessionCacheUsage.Add(sessionLookupKey, ++sessionUsageCount);
                    }


                    if (compType == CompressionType.None)//REMOTE->LOCAL
                    {
                        if (fs.CopyFileRemoteLocal(syncPair.DstSettings, session,
                                sourceSyncInfo, destinationSyncInfo
                                , true, false, syncSettingSrc) == false)
                        {
                            throw new Exception("Could not copy file from remote server!");
                        }
                        //sync if vault
                        if (dstLocation.FileLocation.LocationType == "vault")
                        {
                            string vaultName = dstLocation.FileLocation.Name;
                            //input.Tbc.DirectorySettings.SyncPairs[vaultName].SyncOthers(destinationSyncInfo);
                        }
                    }
                    else//REMOTE->LOCAL+ compression
                    {
                        //copy to a temp file in local first
                        FileSyncInfo tempInfo = GetTempSyncInfo();
                        if (fs.CopyFileRemoteLocal(syncPair.DstSettings, session, sourceSyncInfo,
                                tempInfo, true, false, syncSettingSrc) == false)
                        {
                            throw new Exception("Could not copy file from remote server!");
                        }
                        //compress to temp compressed file
                        FileCompressor fCompressor = new FileCompressor();
                        string compressedFileNameWithExtesion = tempInfo.FullPath.Replace(".tbtemp", fCompressor.GetFileExtensionByCompressionType(compType));
                        string output = fCompressor.CompressFile(compType, tempInfo.FullPath,
                            compressedFileNameWithExtesion, 7);
                        if (output != "OK")//output has the whole output of the console session
                        {
                            throw new Exception("Error Compressing File!" + Environment.NewLine + output);
                        }
                        //move compressed file to final local destination
                        FileSyncInfo compressedInfo = GetDummySyncInfoLocal(compressedFileNameWithExtesion);
                        if (fs.CopyFileInsideLocal(compressedInfo, destinationSyncInfo, true) == false)
                        {
                            throw new Exception("Could not copy file from local to local!");
                        }
                        File.Delete(tempInfo.FullPath);
                        File.Delete(compressedInfo.FullPath);
                    }

                }
            }
            //if source location type local
            if (srcLocation.FileLocation.LocationType.StartsWith("local") ||
                srcLocation.FileLocation.LocationType.StartsWith("vault"))
            {
                if (srcLocation.FileLocation.LocationType.StartsWith("vault"))
                {
                    if (dstLocation.FileLocation.LocationType.StartsWith("vault"))
                    {
                        throw new Exception("Vault to Vault File Copy is not supported!");
                        //get a local file reference from vault and change sourceSyncInfo
                        //Vault vault=tbc.directorySettings.
                    }
                    //otherwise get local file ref from vault (this will copy from other vaults through ftp first if nonexisting) 
                    //& execute as if source is local
                    //Vault vault = input.Tbc.DirectorySettings.Vaults.First(c => c.Name == srcLocation.FileLocation.Name);
                    //string localFileName = vault.GetSingleFile(new FileSyncInfo(paramFileCopy.RelativeFileName, srcLocation));
                    string localFileName = srcLocation.FileLocation.StartingPath + Path.DirectorySeparatorChar +
                                           paramFileCopy.RelativeFileName;
                    if (localFileName == "")
                    {
                        throw new Exception("Could not find source file in vault!");
                    }

                }

                //if destination type local
                if (dstLocation.FileLocation.LocationType.StartsWith("local"))
                {
                    if (srcLocation.FileLocation.Name != dstLocation.FileLocation.Name)
                    {
                        throw new Exception("File Locations must be same for local-local file copy!");
                    }
                    //both local
                    //copy file from local to local
                    if (compType == CompressionType.None)
                    {
                        if (fs.CopyFileInsideLocal(sourceSyncInfo, destinationSyncInfo, true) == false)
                        {
                            throw new Exception("Could not copy file from local to local!");
                        }
                    }
                    else//LOCAL->LOCAL+COMPRESSION
                    {
                        FileCompressor fCompressor = new FileCompressor();
                        string compressedFileNameWithExtesion = destinationSyncInfo.FullPath + fCompressor.GetFileExtensionByCompressionType(compType);
                        string output = fCompressor.CompressFile(compType, paramFileCopy.RelativeFileName,
                            compressedFileNameWithExtesion, 7);
                        if (output != "OK")//output has the whole output of the console session
                        {
                            throw new Exception("Error Compression File!" + Environment.NewLine + output);
                        }
                    }

                }
                //if destination type remote
                else
                {
                    if (compType == CompressionType.None)//LOCAL->REMOTE
                    {
                        using (Session session = dstLocation.GetRemoteFileTransferSession(input.Tbc))
                        {
                            string srcFileName = srcLocation.FileLocation.StartingPath + "/" + paramFileCopy.RelativeFileName;
                            if (fs.CopyFileLocalRemote(syncPair.DstSettings, session, sourceSyncInfo, destinationSyncInfo, true, false) == false)
                            {
                                throw new Exception("Could not copy file from local to remote!");
                            }
                        }
                    }
                    else//LOCAL->REMOTE+COMPRESSION
                    {
                        //compress to temp compressed file
                        FileSyncInfo tempInfo = GetTempSyncInfo();

                        //compress to temp compressed file
                        FileCompressor fCompressor = new FileCompressor();
                        string output = fCompressor.CompressFile(compType, sourceSyncInfo.FullPath, tempInfo.FullPath,
                             7);
                        if (output != "OK")//output has the whole output of the console session
                        {
                            throw new Exception("Error Compressing File!" + Environment.NewLine + output);
                        }
                        //add file extension to destination file
                        string compExt = fCompressor.GetFileExtensionByCompressionType(compType);
                        destinationSyncInfo.FileNameOnly = destinationSyncInfo.FileNameOnly + compExt;
                        destinationSyncInfo.FullPath = destinationSyncInfo.FullPath + compExt;
                        destinationSyncInfo.RelativePath = destinationSyncInfo.RelativePath + compExt;
                        using (Session session = dstLocation.GetRemoteFileTransferSession(input.Tbc))
                        {
                            if (fs.CopyFileLocalRemote(syncPair.DstSettings, session,
                                    tempInfo, destinationSyncInfo, true, false) == false)
                            {
                                throw new Exception("Could not copy file from local to remote!");
                            }
                        }
                        File.Delete(tempInfo.FullPath);
                    }
                }
            }

            return JobCompletionStatus.Complete;
        }

        public object PreprocessJob(object data)
        {
            throw new NotImplementedException();
        }

        public object PostprocessJob(object data)
        {
            throw new NotImplementedException();
        }

        public ITelcobrightJob createNewNonSingletonInstance()
        {
            throw new NotImplementedException();
        }
//execute


        public void CreateJob(SyncPair syncPair, string fileName, TelcobrightConfig tbc)
        {
            SyncLocation srcLocation = syncPair.SrcSyncLocation;
            SyncLocation dstLocation = syncPair.DstSyncLocation;
            List<string> fileNames = srcLocation.GetFileNamesFiltered(syncPair.SrcSettings, tbc);
            string entityConStr = ConnectionManager.GetEntityConnectionStringByOperator(tbc.Telcobrightpartner.CustomerName,
                tbc);
            using (PartnerEntities context = new PartnerEntities(entityConStr))
            {
                int priority = context.enumjobdefinitions.First(c => c.id == 6).Priority;
                job newJob = null;
                newJob = new job();
                JobParamFileCopy copyParam = new JobParamFileCopy()
                {
                    //relative path processing
                    RelativeFileName = fileName.Replace(srcLocation.FileLocation.StartingPath, ""),
                    SyncPairName = syncPair.Name
                };

                newJob.idNE = 0;
                newJob.idjobdefinition = 6;
                newJob.JobName = syncPair.Name + "-" + copyParam.RelativeFileName;
                if (context.jobs.Any(c => c.idjobdefinition == 6 && c.JobName == newJob.JobName) == true)
                {
                    //exists
                    return;
                }
                newJob.Status = 6;//created
                newJob.priority = priority;
                //newJob.OwnerServer = tbc.thisServerId;
                newJob.JobParameter = JsonConvert.SerializeObject(copyParam);
                //context.jobs.Add(newJob);
                newJob.CreationTime = DateTime.Now;
                context.jobs.Add(newJob);
                context.SaveChanges();//canot implement segment because 2 parallel server, have to thoughout later                                }

            }
        }

    }
}
