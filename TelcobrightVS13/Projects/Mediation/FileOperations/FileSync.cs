using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LibraryExtensions;
using WinSCP;


namespace TelcobrightFileOperations
{
    public class FileSyncInfo
    {

        public string FullPath { get; set; }//includes starting path of file location
        public string RelativePath { get; set; }
        public string FileNameOnly { get; set; }
        public List<string> SubDirsAfterStartingPath { get; set; }
        public SyncLocation SyncLocation { get; set; }
        public FileSyncInfo(string RelativePath, SyncLocation syncLocation)//for temp file use fullpath=true
        {
            if (RelativePath != null)
            {
                char pathSeparator = syncLocation.FileLocation.GetPathSeparator();
                this.SyncLocation = syncLocation;
                this.RelativePath = syncLocation.FileLocation.GetOsNormalizedPath(RelativePath);

                this.FullPath = syncLocation.FileLocation.GetOsNormalizedPath(syncLocation.FileLocation.StartingPath + pathSeparator + this.RelativePath);

                string[] tempArr = this.RelativePath.Split(pathSeparator);
                string[] fileParts = this.RelativePath.Split(pathSeparator);
                this.SubDirsAfterStartingPath = new List<string>();
                int length = fileParts.GetLength(0);
                this.FileNameOnly = fileParts[length - 1];
                for (int i = 0; i < length - 1; i++)
                {
                    this.SubDirsAfterStartingPath.Add(fileParts[i]);
                }
            }
        }
        public void CreatePaths(Session session)//null will mean local
        {
            if (this.RelativePath.EndsWith(".tbtemp"))
            {
                return;//no need to check path for temp files, they must exist
            }
            char pathSeparator = this.SyncLocation.FileLocation.GetPathSeparator();
            List<string> pathSoFar = new List<string>();
            List<string> paths = new List<string>();
            foreach (string subDir in this.SubDirsAfterStartingPath)
            {
                pathSoFar.Add(subDir);
                string currentPath = this.SyncLocation.FileLocation.GetOsNormalizedPath(this.SyncLocation.FileLocation.StartingPath)
                    + this.SyncLocation.FileLocation.GetPathSeparator() + string.Join(this.SyncLocation.FileLocation.GetPathSeparator().ToString(), pathSoFar);
                if (session != null)
                {                   
                    if (Directory.Exists(currentPath) == false)
                    {
                        Directory.CreateDirectory(currentPath);
                    }                
                    else
                    {
                        if (session.FileExists(currentPath) == false)
                        {
                            session.CreateDirectory(currentPath);
                        }
                    }
                }

            }
        }
        public FileSyncInfo GetCopy()
        {
            FileSyncInfo newInfo = new FileSyncInfo(this.RelativePath, this.SyncLocation)
            {
                FullPath = this.FullPath,
                RelativePath = this.RelativePath,
                FileNameOnly = this.FileNameOnly,
                SubDirsAfterStartingPath = this.SubDirsAfterStartingPath,
                SyncLocation = this.SyncLocation
            };
            return newInfo;
        }
    }
    public class FileSyncher
    {
        public bool CopyFileInsideLocal(FileSyncInfo srcInfoLocal, FileSyncInfo dstInfoLocal, bool overwrite)
        {
            if (!srcInfoLocal.SyncLocation.FileLocation.LocationType.StartsWith("local"))
            {
                throw new Exception("Can't invoke remote-remote file copy when file location type is remote.");
            }
            if (overwrite == false)
            {
                if (File.Exists(dstInfoLocal.FullPath))
                {
                    return true;//throw new System.Exception("File " + dstInfoLocal.fullPath + " exists!");
                }
            }
            dstInfoLocal.CreatePaths(null);
            File.Copy(srcInfoLocal.FullPath, dstInfoLocal.FullPath, true);
            if (File.Exists(dstInfoLocal.FullPath))
            {
                return true;
            }
            return false;
        }
        public bool MoveFileInsideLocal(FileSyncInfo srcInfoLocal, FileSyncInfo dstInfoLocal, bool overwrite)
        {
            if (!srcInfoLocal.SyncLocation.FileLocation.LocationType.StartsWith("local"))
            {
                throw new Exception("Can't invoke remote-remote file copy when file location type is remote.");
            }
            if (overwrite == false)
            {
                if (File.Exists(dstInfoLocal.FullPath))
                {
                    return true;//throw new System.Exception("File " + dstInfoLocal.fullPath + " exists!");
                }
            }
            if (File.Exists(dstInfoLocal.FullPath))
            {
                File.Delete(dstInfoLocal.FullPath);
            }
            dstInfoLocal.CreatePaths(null);
            File.Move(srcInfoLocal.FullPath, dstInfoLocal.FullPath);
            if (File.Exists(dstInfoLocal.FullPath))
            {
                return true;
            }
            return false;
        }
        public bool DeleteFileRemote(Session session, FileSyncInfo delInfo)
        {
            string delCommand = "";
            if (session.FileExists(delInfo.FullPath))
            {
                session.RemoveFiles(delInfo.FullPath);
                if (session.FileExists(delInfo.FullPath) == true)
                    return false;
            }
            return true;
        }
        public bool CopyFileInsideRemote(Session session, FileSyncInfo srcSyncInfoRemote, FileSyncInfo dstsyncInfoRemote, bool overwrite)
        {
            if (srcSyncInfoRemote.SyncLocation.FileLocation.LocationType.StartsWith("local"))
            {
                throw new Exception("Can't invoke remote-remote file copy when file location type is local.");
            }
            if (overwrite == false)
            {
                if (session.FileExists(dstsyncInfoRemote.FullPath))
                {
                    return true;//throw new System.Exception("File " + dstsyncInfoRemote + " exists!");
                }
            }
            string copyCommand = "";
            string delCommand = "";
            switch (dstsyncInfoRemote.SyncLocation.FileLocation.OsType)
            {
                case "windows":
                    copyCommand = "copy " + srcSyncInfoRemote.FullPath + " " + dstsyncInfoRemote.FullPath;
                    delCommand = "del /F /Q" + dstsyncInfoRemote.FullPath;
                    break;
                case "linux":
                    copyCommand = "cp " + srcSyncInfoRemote.FullPath + " " + dstsyncInfoRemote.FullPath;
                    delCommand = "rm -f " + dstsyncInfoRemote.FullPath;
                    break;
            }
            if (session.FileExists(dstsyncInfoRemote.FullPath))
            {
                session.ExecuteCommand(delCommand).Check();
            }
            dstsyncInfoRemote.CreatePaths(session);
            session.ExecuteCommand(copyCommand).Check();
            if (session.FileExists(dstsyncInfoRemote.FullPath))
            {
                return true;
            }
            return false;
        }
        public bool MoveFileInsideRemote(Session session, FileSyncInfo srcSyncinfoRemote, FileSyncInfo dstSyncinfoRemote, bool overwrite)
        {
            if (srcSyncinfoRemote.SyncLocation.FileLocation.LocationType.StartsWith("local"))
            {
                throw new Exception("Can't invoke remote-remote file copy when file location type is local.");
            }
            if (overwrite == false)
            {
                if (session.FileExists(srcSyncinfoRemote.RelativePath))
                {
                    return true;//throw new System.Exception("File " + srcSyncinfoRemote.relativePath + " exists!");
                }
            }
            srcSyncinfoRemote.CreatePaths(session);
            session.MoveFile(srcSyncinfoRemote.RelativePath, dstSyncinfoRemote.RelativePath);
            if (session.FileExists(dstSyncinfoRemote.RelativePath))
            {
                return true;
            }
            return false;
        }

        public bool CopyFileRemoteLocal(SyncSettingsDest dstSettings, Session session, FileSyncInfo srcInfoRemote,
            FileSyncInfo dstInfoLocal, bool overwrite, bool removeOriginal,SyncSettingsSource syncSettingsSource)
        {
            if (srcInfoRemote.SyncLocation.FileLocation.LocationType.StartsWith("local"))
            {
                throw new Exception("Can't invoke remote-local file copy when file location type is local.");
            }
            //if()
            if (overwrite == false)
            {
                if (File.Exists(dstInfoLocal.FullPath))
                {
                    return true; //throw new System.Exception("File " + dstInfoLocal.fullPath + " exists!");
                }
            }
            else {//overwrite
                if (File.Exists(dstInfoLocal.FullPath))
                {
                    File.Delete(dstInfoLocal.FullPath);
                }
                string tempExt = "";
                string secondaryDirectory = syncSettingsSource.SecondaryDirectory;
                string downloadedDirFullPath = Path.GetDirectoryName(srcInfoRemote.FullPath) + "/" + secondaryDirectory;
                string alternateDownloadPathFromSecondaryDir = downloadedDirFullPath + "/" +
                    Path.GetFileName(srcInfoRemote.FullPath);
                
                bool alreadyDownloadingFromSecondaryDir = false;
                if (dstSettings.FileExtensionForSafeCopyWithTempFile != "")
                {
                    RemoteFileInfo remoteFileInfo = null;
                    tempExt = dstSettings.FileExtensionForSafeCopyWithTempFile;
                    dstInfoLocal.CreatePaths(null);
                    string tempFile = dstInfoLocal.FullPath + tempExt;
                    try
                    {
                        remoteFileInfo= session.GetFileInfo(srcInfoRemote.FullPath);
                    }
                    catch (Exception e)
                    {
                        
                        if (e.Message.Contains("Can't get attributes of file") && !secondaryDirectory//file not found and there is a downloaded dir
                                .IsNullOrEmptyOrWhiteSpace())
                        {
                            
                            srcInfoRemote.FullPath = alternateDownloadPathFromSecondaryDir;
                            remoteFileInfo = session.GetFileInfo(srcInfoRemote.FullPath);
                            alreadyDownloadingFromSecondaryDir = true;
                        }
                        else
                        {
                            Console.WriteLine(e);
                            throw;
                        }
                    }
                    session.GetFiles(srcInfoRemote.FullPath, tempFile, removeOriginal);
                
                    if (removeOriginal == true)
                    {
                        session.RemoveFile(srcInfoRemote.FullPath);
                    }
                    else if (!secondaryDirectory.IsNullOrEmptyOrWhiteSpace())//move to secondary dir if set in config
                    {

                        if (!alreadyDownloadingFromSecondaryDir)
                        {
                            //string downloadedDirFullPath = "c:/sdfsdf/downloaded";
                            if (session.FileExists(downloadedDirFullPath)==false)
                            {
                                session.CreateDirectory(downloadedDirFullPath);
                            }
                            session.MoveFile(srcInfoRemote.FullPath,alternateDownloadPathFromSecondaryDir);
                        }
                    }
                    long tempFileLength = new FileInfo(tempFile).Length;
                    if (tempFileLength != remoteFileInfo.Length) {
                        throw new Exception("Length of Downloaded File with temporary extension " + tempFileLength + " != remoteFile's length " + remoteFileInfo.Length);
                    }
                    File.Move(dstInfoLocal.FullPath + tempExt, dstInfoLocal.FullPath);
                    long renamedFileLength = new FileInfo(dstInfoLocal.FullPath).Length;
                    if (tempFileLength != renamedFileLength)
                    {
                        throw new Exception("Length of Downloaded File after renaming " + renamedFileLength + " != " + tempFileLength);
                    }
                }
                else
                {
                    dstInfoLocal.CreatePaths(null);
                                        RemoteFileInfo remoteFileInfo = session.GetFileInfo(srcInfoRemote.FullPath);
                    session.GetFiles(srcInfoRemote.FullPath, dstInfoLocal.FullPath, removeOriginal);
                    long downloadedFileLength = new FileInfo(dstInfoLocal.FullPath).Length;
                    if (downloadedFileLength != remoteFileInfo.Length)
                    {
                        throw new Exception("Length of Downloaded File " + downloadedFileLength + " != " + remoteFileInfo.Length);
                    }
                }
            }
            if (!File.Exists(dstInfoLocal.FullPath))
            {
                //File.Delete(dstInfoLocal.FullPath);
                //copy with temp ext first
                string tempExt = "";
                if (dstSettings.FileExtensionForSafeCopyWithTempFile != "")
                {
                    tempExt = dstSettings.FileExtensionForSafeCopyWithTempFile;
                    dstInfoLocal.CreatePaths(null);
                    session.GetFiles(srcInfoRemote.FullPath, dstInfoLocal.FullPath + tempExt, removeOriginal);
                    File.Move(dstInfoLocal.FullPath + tempExt, dstInfoLocal.FullPath);
                }
                else
                {
                    dstInfoLocal.CreatePaths(null);
                    session.GetFiles(srcInfoRemote.FullPath, dstInfoLocal.FullPath, removeOriginal);
                }
            }

            if (File.Exists(dstInfoLocal.FullPath))//the file has already been copied to local, somehow the process failed or was stopped before updating job
            {
                if (!syncSettingsSource.SecondaryDirectory.IsNullOrEmptyOrWhiteSpace() &&
                    syncSettingsSource.MoveFilesToSecondaryAfterCopy == true)
                {
                    string[] pathToDestinationFile = srcInfoRemote.FullPath.Split('/');
                    string destNamePrefixedBySecondary =
                        syncSettingsSource.SecondaryDirectory + "/" + pathToDestinationFile.Last();
                    pathToDestinationFile[pathToDestinationFile.Length - 1] = destNamePrefixedBySecondary;
                    string finalTargetFileName = string.Join("/", pathToDestinationFile);
                    string[] secondaryDirectory = finalTargetFileName.Split('/');
                    secondaryDirectory = secondaryDirectory.Take(secondaryDirectory.Count() - 1).ToArray();
                    string finalSecondaryDirectory = string.Join("/", secondaryDirectory);
                    
                    if (!session.FileExists(finalSecondaryDirectory)) session.CreateDirectory(finalSecondaryDirectory);
                    if (session.FileExists(srcInfoRemote.FullPath))
                    {
                        try//as the file was copied before, no need to propagate the exception so that job won't update
                        {
                            
                            session.MoveFile(srcInfoRemote.FullPath, finalTargetFileName);
                            //string targetMoveDirOnly = Path.GetDirectoryName(finalTargetFileName);
                            //session.MoveFile(srcInfoRemote.FullPath, targetMoveDirOnly);
                        }
                        catch (Exception e) {
                            if (e.Message.ToLower().Contains("filename invalid")) //target dirs don't exist
                            {
                                string[] targetDirectoryChain = finalTargetFileName.Split('/')
                                    .Where(s => !string.IsNullOrEmpty(s)).ToArray();
                                string parent = new string(finalTargetFileName.ToCharArray()
                                    .TakeWhile(c => c == '/').ToArray());

                                for (int i = 0; i < targetDirectoryChain.Length-1; i++)
                                {
                                    string folderName = targetDirectoryChain[i];
                                    string dirToCreate = parent + folderName;
                                    session.CreateDirectory(dirToCreate);
                                    parent = dirToCreate + "/";
                                }
                                session.MoveFile(srcInfoRemote.FullPath, finalTargetFileName);
                            }
                            else
                            {
                                Console.WriteLine(e.Message);
                                throw;
                            }
                        }
                    }
                }
                return true;
            }
            return false;
        }

        public bool CopyFileLocalRemote(SyncSettingsDest dstSettings, Session session,
            FileSyncInfo srcInfoLocal, FileSyncInfo dstInfoRemote, bool overwrite, bool removeOriginal)
        {
            if (srcInfoLocal.SyncLocation.FileLocation.LocationType.StartsWith("remote"))
            {
                throw new Exception("Can't invoke local-remote file copy when file location type is remote.");
            }
            if (overwrite == false)
            {
                if (session.FileExists(dstInfoRemote.FullPath))
                {
                    return true;//throw new System.Exception("File " + dstInfoRemote.fullPath + " exists!");
                }
            }
            if (session.FileExists(dstInfoRemote.FullPath))
            {
                if (DeleteFileRemote(session, dstInfoRemote) == true)
                {
                    //copy with temp ext first
                    string tempExt = "";
                    if (dstSettings.FileExtensionForSafeCopyWithTempFile != "")
                    {
                        tempExt = dstSettings.FileExtensionForSafeCopyWithTempFile;
                        dstInfoRemote.CreatePaths(session);
                        // Upload files
                        TransferOptions transferOptions = new TransferOptions();
                        transferOptions.TransferMode = TransferMode.Binary;

                        TransferOperationResult transferResult;
                        
                        transferResult =session.PutFiles(srcInfoLocal.FullPath,
                            dstInfoRemote.FullPath+tempExt
                            , false);
                        transferResult.Check();
                        session.MoveFile(dstInfoRemote.FullPath + tempExt, dstInfoRemote.FullPath);
                        return true;
                    }
                    else
                    {
                        // Upload files
                        TransferOptions transferOptions = new TransferOptions();
                        transferOptions.TransferMode = TransferMode.Binary;
                        TransferOperationResult transferResult;
                        transferResult=session.PutFiles(srcInfoLocal.FullPath, dstInfoRemote.FullPath, true);
                        transferResult.Check();
                        return true;
                    }
                }
                else
                {
                    throw new Exception("Couldn't delete existing file!");
                }
            }
            else//if dest file doesn't exist
            {
                //copy with temp ext first
                string tempExt = "";
                if (dstSettings.FileExtensionForSafeCopyWithTempFile != "")
                {
                    tempExt = dstSettings.FileExtensionForSafeCopyWithTempFile;
                    dstInfoRemote.CreatePaths(session);
                    // Upload files
                    TransferOptions transferOptions = new TransferOptions();
                    transferOptions.TransferMode = TransferMode.Binary;
                    TransferOperationResult transferResult;
                    transferResult=session.PutFiles(srcInfoLocal.FullPath,
                        dstInfoRemote.FullPath+tempExt
                        , false);
                    transferResult.Check();
                    session.MoveFile(dstInfoRemote.FullPath + tempExt, dstInfoRemote.FullPath);
                    return true;
                }
                else
                {
                    dstInfoRemote.CreatePaths(session);
                    // Upload files
                    TransferOptions transferOptions = new TransferOptions();
                    transferOptions.TransferMode = TransferMode.Binary;
                    TransferOperationResult transferResult;
                    transferResult = session.PutFiles(srcInfoLocal.FullPath, dstInfoRemote.FullPath, true);
                    transferResult.Check();
                    return true;
                }
            }
            if (session.FileExists(dstInfoRemote.FullPath))
            {
                return true;
            }
            return false;
        }
    }
}
