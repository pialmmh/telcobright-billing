using Newtonsoft.Json;
using System;
using System.IO;
using LibraryExtensions;
using LibraryExtensions.ConfigHelper;
using MediationModel;
using MySql.Data.MySqlClient;
using TelcobrightInfra;
using TelcobrightMediation.Config;

namespace TelcobrightFileOperations
{
    public static class ErrorWriter
    {
        public static void WriteError(Exception e, string processInformation, job telcobrightJob, string messageToPrepend,
            string operatorName, PartnerEntities context)
        {
            try
            {
                Console.WriteLine(e);
                string constr = context.Database.Connection.ConnectionString;
                int maxLenOfErrorDescFieldInDb = 1000;
                string exceptionMessage = string.IsNullOrEmpty(messageToPrepend)
                    ? e.Message.Left(maxLenOfErrorDescFieldInDb)
                    : (messageToPrepend + Environment.NewLine + e.Message).Left(maxLenOfErrorDescFieldInDb);
                string processName = $@"{processInformation}/{telcobrightJob?.JobName} [jobid={telcobrightJob?.id}]";
                string exceptionDetail = e.InnerException?.ToString() ?? "";

                var thisError = new allerror
                {
                    TimeRaised = DateTime.Now,
                    Status = 1,
                    ExceptionMessage = exceptionMessage,
                    ProcessName = processName,
                    ExceptionDetail = exceptionDetail
                };
                try
                {
                    using (MySqlConnection con = new MySqlConnection(constr))
                    {
                        con.Open();
                        Func<string, string> purify = s => s.Replace("Can't","Cannot").Replace("can't","cannot").Replace("'", "").Replace("\"", "");
                        using (MySqlCommand cmd = new MySqlCommand("", con))
                        {
                            string sql =
                                $@"insert into allerror (timeraised,status, exceptionmessage, processname, exceptionDetail,iderror) values (
                                    '{DateTime.Now.ToMySqlFormatWithoutQuote()}',1,'{purify(exceptionMessage)}', '{purify(processName)}', 
                                    '{purify(exceptionDetail)}',1);";
                            cmd.CommandText = sql;
                            cmd.ExecuteNonQuery();
                        }
                        con.Close();
                    }
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.Message);
                    File.AppendAllText("telcobright.log", $"Error: {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + JsonConvert.SerializeObject(thisError)} "
                                                          + Environment.NewLine);
                }
            }
            catch (Exception e2) //database error
            {
                Console.WriteLine(e2);
                allerror thisError = new allerror
                {
                    TimeRaised = DateTime.Now,
                    Status = 1,
                    ExceptionMessage = e2.Message,
                    ProcessName = processInformation,
                    ExceptionDetail = e2.InnerException?.ToString() ?? ""
                };
                File.AppendAllText("telcobright.log", $"Error: {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + JsonConvert.SerializeObject(thisError)} "
                                                      + Environment.NewLine);
            }
        }
    }
}
