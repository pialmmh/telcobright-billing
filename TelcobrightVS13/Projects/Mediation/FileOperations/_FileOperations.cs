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


    public static class FileUtil
    {
        public static job CreateFileDeleteJob(string fileName,FileLocation fileLocation,
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
                JobName = fileLocation.Name + "-" + fileName.Replace(@"\", @"\\"),
                Status = 6, //created
                JobParameter = JsonConvert.SerializeObject(jobParamFileDel),
                CreationTime = DateTime.Now
            };
            int priority = context.enumjobdefinitions.Where(c => c.id == newJob.idjobdefinition).First().Priority;
            newJob.priority = priority;
            return newJob;
         }

        public static job CreateFileCopyJob(TelcobrightConfig tbc, string syncPairName, string fileName,PartnerEntities context)
        {
            //returns job id
            SyncPair syncPair = tbc.DirectorySettings.SyncPairs[syncPairName];
            SyncLocation srcLocation = syncPair.SrcSyncLocation;
            SyncLocation dstLocation = syncPair.DstSyncLocation;
            int priority = context.enumjobdefinitions.Where(c => c.id == 6).First().Priority;
            job fileCopyJob = null;
            fileCopyJob = new job();
            int startingPathLen = srcLocation.FileLocation.StartingPath.Length;
            string relativeFileName = fileName.StartsWith(srcLocation.FileLocation.StartingPath) ?
                    fileName.Substring(startingPathLen) : fileName;
            relativeFileName = relativeFileName.StartsWith("/") ? relativeFileName.Substring(1) : relativeFileName;
            JobParamFileCopy copyParam = new JobParamFileCopy()
            {
                //relative path processing
                RelativeFileName = relativeFileName,
                SyncPairName = syncPair.Name
            };

            fileCopyJob.idNE = 0;
            fileCopyJob.idjobdefinition = 6;
            //fileCopyJob.JobName = syncPair.Name + "-" + copyParam.RelativeFileName;
            fileCopyJob.JobName = syncPair.Name + "-" + copyParam.RelativeFileName.Replace(@"\",@"\\");
            if (context.jobs.Any(c => c.idjobdefinition == 6 && c.JobName == fileCopyJob.JobName) == true)
            {
                //exists
                return null;
            }
            fileCopyJob.Status = 6; //created
            fileCopyJob.priority = priority;
            //newJob.OwnerServer = tbc.thisServerId;
            fileCopyJob.JobParameter = JsonConvert.SerializeObject(copyParam);
            //context.jobs.Add(newJob);
            fileCopyJob.CreationTime = DateTime.Now;
            return fileCopyJob;
        }
        public static long WriteFileCopyJobSingle(job newJob, DbConnection connection)
        {
            DbCommand command = connection.CreateCommand();
            command.CommandText =
                newJob.GetExtInsertCustom(
                    j => new StringBuilder($@"insert into job(idne,idjobdefinition,jobname,status,priority,jobparameter,creationtime) 
                                            values(
                                            {newJob.idNE},{newJob.idjobdefinition},'{newJob.JobName}'
                                            ,{newJob.Status},{newJob.priority},'{newJob.JobParameter}'
                                            ,{newJob.CreationTime.ToMySqlField()})"
                    ).ToString()).ToString();
            command.ExecuteNonQuery();
            command.CommandText = $@"select id from job where idjobdefinition={newJob.idjobdefinition}
                                     and jobname='{newJob.JobName}' ";
            long insertedJobsid = (long)command.ExecuteScalar();
            return insertedJobsid;
        }
        public static long WriteFileCopyJobMultiple(List<job> newJobs, PartnerEntities context)
        {
            if (!newJobs.Any()) return 0;
            int insertedRecordCount = 0;
            CollectionSegmenter<job> segments=new CollectionSegmenter<job>(newJobs,0);
            DbCommand command = context.Database.Connection.CreateCommand();
            try
            {
                command.CommandText = "set autocommit=0;";
                command.ExecuteNonQuery();
                int segmentSize = 10000;
                segments.ExecuteMethodInSegments(segmentSize,
                    segment =>
                    {
                        int segmentCount = segment.Count();
                        string extInsertHeader = 
                            $@"insert into job(idne,idjobdefinition,jobname,status,priority,jobparameter,creationtime) values ";
                                            
                        List<string> sqlsExtValues = segment.AsParallel().Select(j =>
                            j.GetExtInsertCustom(
                                newJob => new StringBuilder($@"(
                                    {newJob.idNE},{newJob.idjobdefinition},'{newJob.JobName}'
                                    ,{newJob.Status},{newJob.priority},'{newJob.JobParameter}'
                                    ,{newJob.CreationTime.ToMySqlField()})"
                                ).ToString()).ToString()
                        ).ToList();
                        command.CommandText=new StringBuilder(extInsertHeader)
                            .Append(string.Join(",",sqlsExtValues)).ToString();
                        insertedRecordCount += command.ExecuteNonQuery();
                    });
                if (insertedRecordCount != newJobs.Count)
                    throw new Exception("Affected record count does not match expected count.");
                command.CommandText = "commit;";
                command.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                command.CommandText = "rollback;";
                command.ExecuteNonQuery();
                throw;
            }
            return insertedRecordCount;
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
        
        public static List<string[]> ParseTextFileToListOfStrArray(string path, char separator, int linesToSkipBefore,
            char enclosingCharToRemove='\0')
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
                    if (enclosingCharToRemove!='\0')
                    {
                        row = row.Select(arrItem => arrItem.Trim(enclosingCharToRemove)).ToArray();
                    }
                    parsedData.Add(row);
                    thisLine += 1;
                }
            }
            return parsedData;
        }


        public static List<string[]> ParseCsvWithEnclosedAndUnenclosedFields(string path, char delimeter, int linesToSkipBefore,
            string enclosingChar, string charToReplaceDelimiterInsideField)
        {
            List<string> lines = new List<string>();
            using (StreamReader readFile = new StreamReader(path))
            {
                lines = readFile.ReadToEnd().Split('\n')
                    .Skip(linesToSkipBefore)
                    .Select(line => line.Replace('\r', '\0'))
                    .Where(line=>line.Trim()!="").ToList();
            }
            //temp
            //lines = new List<string>();
            //string tempLine = File.ReadAllText(@"c:\temp\oneLiner.txt");
            //lines.Add(tempLine);
            //Func<List<string[]>> test= ()=>;

            //List<string[]> rowsWithFieldArrs = lines.Select(
            //    line =>
            //    {
            //        var delReplaced = ReplaceDelimeterInsideField(line, delimeter, enclosingChar, ";");
            //        string[] quoteReplaced = delReplaced.Split(',').Select(c => c.Replace("\"", "")).ToArray();
            //        return quoteReplaced;
            //    }).ToList();

            //File.WriteAllText(@"c:\temp\oneLinerProcessed.txt", lines[0]);

            List<string[]> rowsWithFieldArrs = lines.Select(
                line => ReplaceDelimeterInsideField(line, delimeter, enclosingChar, ";"))
                        .Select(line => line.Split(delimeter))
                        .Select(fieldArray => fieldArray.Select(field => field.Replace(enclosingChar, "")).ToArray())
                        .ToList();
            return rowsWithFieldArrs;                        
        }


        static string ReplaceDelimeterInsideField(string line, char delimeter, string enclosingCharStr, string charToReplaceDelimiterInsideField)
        {
            line = line.Replace(new string(new char[] { '\"', '\"', '\"', '\"' }), "");
            var lineChars = line.ToCharArray();
            char enclosingChar = Convert.ToChar(enclosingCharStr);
            bool quoteOpen = false;
            char[] allowedCharsAfterQuoteClosed = new char[] { ',', '\r', '\n' };
            int len = lineChars.GetLength(0);
            for (int i = 0; i < len; i++)
            {
                char c = lineChars[i];

                if (c == delimeter)
                {
                    if (quoteOpen)
                    {
                        lineChars[i] = Convert.ToChar(charToReplaceDelimiterInsideField);
                    }
                }
                else if (c == enclosingChar)
                {
                    //if (quoteOpen)
                    //{//bug in cdr row, remove unnecessary double quote
                    //    lineChars[i] = '\0';
                    //}
                    quoteOpen = !quoteOpen;
                    //if ((!quoteOpen && i > 0) && (len - i >= 2))
                    //{
                    //    char nextChar = lineChars[i + 1];
                    //    if (!allowedCharsAfterQuoteClosed.Contains(nextChar))
                    //    {
                    //        lineChars[i + 1] = '\0';
                    //    }
                    //}
                }
            }
            var nullReplaced = lineChars.Where(c => c != '\0').ToArray();
            return new string(nullReplaced.ToArray());
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
