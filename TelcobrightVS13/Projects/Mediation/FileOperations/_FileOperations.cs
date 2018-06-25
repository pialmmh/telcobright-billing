using TelcobrightMediation;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.AccessControl;
using System.Text;
using LibraryExtensions;
using MediationModel;

namespace TelcobrightFileOperations
{
    public class JobParamFileCopy
    {
        public string SyncPairName { get; set; }
        public string RelativeFileName { get; set; }
    }
    public class JobParamFileDelete
    {
        public string FileName { get; set; }
        public FileLocation FileLocation { get; set; }
        public JobPreRequisite JobPrerequisite { get; set; }
        public JobParamFileDelete()//default , for json
        {
            this.JobPrerequisite = new JobPreRequisite();
        }
        public JobParamFileDelete(string filename, FileLocation filelocation)
        {
            this.FileName = filename;
            this.FileLocation = filelocation;
            this.JobPrerequisite = new JobPreRequisite();
        }
    }


    public static class FileUtil
    {
        public static void CreateFileDeleteJob(string fileName,FileLocation fileLocation,
            PartnerEntities context,JobPreRequisite jobPreReq)
        {
            JobParamFileDelete jobParamFileDel = new JobParamFileDelete(fileName, fileLocation)
            {
                JobPrerequisite = jobPreReq
            };
            job newJob = new job()
            {
                idjobdefinition = 8,
                idNE = 0,
                JobName = fileLocation.Name + "-" + fileName,
                Status = 6, //created
                JobParameter = JsonConvert.SerializeObject(jobParamFileDel),
                CreationTime = DateTime.Now
            };
            int priority = context.enumjobdefinitions.Where(c => c.id == newJob.idjobdefinition).First().Priority;
            newJob.priority = priority;

            if (context.jobs.Any(c => c.idjobdefinition == newJob.idjobdefinition && c.JobName == newJob.JobName))
            {
                //exists
                return;
            }

            //don't use context.add as context.Connection has been used with autocommit=0 for transaction. 
            //use the same connection
            DbCommand cmd = context.Database.Connection.CreateCommand();
            cmd.CommandText= newJob.GetExtInsertCustom(
                                    j => new StringBuilder(
                                         $@"insert into job(idjobdefinition,priority,idne,jobname,status,jobparameter,creationtime) 
                                         values ({newJob.idjobdefinition},{priority},{0},'{newJob.JobName}',
                                         {newJob.Status},'{newJob.JobParameter}',{newJob.CreationTime.ToMySqlField()})")
                                         .ToString()).ToString();
            cmd.ExecuteNonQuery();
            //context.jobs.Add(newJob);
            //context.SaveChanges();
        }

        public static long CreateFileCopyJob(TelcobrightConfig tbc, string syncPairName, string fileName,PartnerEntities context)
        {
            //returns job id
            SyncPair syncPair = tbc.DirectorySettings.SyncPairs[syncPairName];
            SyncLocation srcLocation = syncPair.SrcSyncLocation;
            SyncLocation dstLocation = syncPair.DstSyncLocation;
            int priority = context.enumjobdefinitions.Where(c => c.id == 6).First().Priority;
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
                return -1;
            }
            newJob.Status = 6; //created
            newJob.priority = priority;
            //newJob.OwnerServer = tbc.thisServerId;
            newJob.JobParameter = JsonConvert.SerializeObject(copyParam);
            //context.jobs.Add(newJob);
            newJob.CreationTime = DateTime.Now;
            DbCommand command = context.Database.Connection.CreateCommand();
            command.CommandText=
                newJob.GetExtInsertCustom(
                    j=>new StringBuilder($@"insert into job(idne,idjobdefinition,jobname,status,priority,jobparameter,creationtime) 
                                            values(
                                            {newJob.idNE},{newJob.idjobdefinition},'{newJob.JobName}'
                                            ,{newJob.Status},{newJob.priority},'{newJob.JobParameter}'
                                            ,'{newJob.CreationTime.ToMySqlField()}')"
                                        ).ToString()).ToString();
            command.ExecuteNonQuery();
            //context.jobs.Add(newJob);
            //context.SaveChanges(); //canot implement segment because 2 parallel server, have to thoughout later                                }
            //get the id of the new job
            //return context.jobs.Where(c => c.idjobdefinition == newJob.idjobdefinition
            //                             && c.JobName == newJob.JobName).First().id;
            command.CommandText = $@"select id from job where idjobdefinition={newJob.idjobdefinition}
                                     and jobname='{newJob.JobName}' ";
            int insertedJobsid = (int)command.ExecuteScalar();
            return insertedJobsid;
        }

        public static void AddDirectorySecurity(string fileName, string account, FileSystemRights rights,
            AccessControlType controlType)
        {
            // Create a new DirectoryInfo object.
            DirectoryInfo dInfo = new DirectoryInfo(fileName);

            // Get a DirectorySecurity object that represents the 
            // current security settings.
            DirectorySecurity dSecurity = dInfo.GetAccessControl();

            // Add the FileSystemAccessRule to the security settings. 
            //dSecurity.AddAccessRule(new FileSystemAccessRule(Account,Rights,ControlType));
            dSecurity.AddAccessRule(new FileSystemAccessRule(
                account, rights, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                PropagationFlags.None, controlType));
            // Set the new access settings.
            dInfo.SetAccessControl(dSecurity);
        }


        static void CopyStream(Stream input, Stream output)
        {
            byte[] buffer = new byte[32768];
            while (true)
            {
                int read = input.Read(buffer, 0, buffer.Length);
                if (read <= 0)
                    return;
                output.Write(buffer, 0, read);
            }
        }
        static int FileExistsInFtp(string ftpUrl, string ftpUser, string ftpPass, string fileNameOnly)
        {
            // Get the object used to communicate with the server.
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpUrl);
            request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
            request.Credentials = new NetworkCredential(ftpUser, ftpPass);

            FtpWebResponse response = (FtpWebResponse)request.GetResponse();

            Stream responseStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(responseStream);
            string directoryList = reader.ReadToEnd();
            string statusDescription = response.StatusDescription;

            char[] delimiters = new char[] { '\r', '\n' };
            //string[] parts = value.Split(delimiters,StringSplitOptions.RemoveEmptyEntries);

            string[] directorylistRows = directoryList.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);

            //System.IO.File.WriteAllText(@"FtpDir.txt",DirectoryList + "\r\n" +StatusDescription);
            int i = 0;
            for (i = 0; i < directorylistRows.Length; i++)
            {
                //if any row contains the specified file name then the file exists
                int posFileName = directorylistRows[i].IndexOf(fileNameOnly);
                if (posFileName > 0) //file exists in remote ftp server
                {
                    return 1;
                }
            }// for each row in directoryliststring

            return 0;
        }
        
        public static List<string[]> ParseTextFileToListOfStrArray(string path, char separator, int linesToSkipBefore)
        {
            List<string[]> parsedData = new List<string[]>();
            using (StreamReader readFile = new StreamReader(path))
            {
                string line;
                string[] row;
                int thisLine = 1;
                while ((line = readFile.ReadLine()) != null)
                {
                    if (thisLine <= linesToSkipBefore)
                    {
                        thisLine += 1;
                        continue;
                    }
                    else if (line.Trim()==""||line.Contains(separator)==false)//skip blanks and not having separator char
                    {
                        continue;
                    }
                    row = line.Split(separator);
                    parsedData.Add(row);
                    thisLine += 1;
                }
            }
            return parsedData;
        }

        public static byte[] GetBytesFromFile(string fullFilePath)
        {
            // this method is limited to 2^32 byte files (4.2 GB)
            FileStream fs = File.OpenRead(fullFilePath);
            byte[] bytes = new byte[fs.Length];
            fs.Read(bytes, 0, Convert.ToInt32(fs.Length));
            fs.Close();
            return bytes;
        }

    }

    


   

   
   


}
