using Newtonsoft.Json;
using System;
using System.IO;
using MediationModel;
using TelcobrightMediation.Config;

namespace TelcobrightFileOperations
{
    public class ErrorWriter
    {
        public ErrorWriter(Exception e,string processInformation,job telcobrightJob,string messageToPrepend,
            string databaseName)
        {
            try
            {
                string entityConStr = ConnectionManager.GetEntityConnectionStringByOperator(databaseName);
                using (PartnerEntities context = new PartnerEntities(entityConStr))
                {
                    allerror thisError = new allerror
                    {
                        TimeRaised = DateTime.Now,
                        Status = 1,
                        ExceptionMessage = string.IsNullOrEmpty(messageToPrepend)
                        ?e.Message:(messageToPrepend+Environment.NewLine+e.Message),
                        ProcessName = $@"{processInformation}/{telcobrightJob?.JobName}[jobid={telcobrightJob?.id}]",
                        ExceptionDetail = e.InnerException?.ToString() ?? ""
                    };
                    context.allerrors.Add(thisError);
                    context.SaveChanges();
                }
            }
            catch (Exception e2)//database error
            {
                Console.WriteLine(e2);
                try
                {
                    allerror thisError = new allerror
                    {
                        TimeRaised = DateTime.Now,
                        Status = 1,
                        ExceptionMessage = e2.Message,
                        ProcessName = processInformation,
                        ExceptionDetail = e2.InnerException?.ToString() ?? ""
                    };
                    File.AppendAllText("telcobright.log", JsonConvert.SerializeObject(thisError) + Environment.NewLine);
                }
                finally
                {
                    //do nothing just make sure that the application keeps running if even file writing fails
                }
            }
        }
        

    }
}
