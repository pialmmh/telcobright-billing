﻿using Newtonsoft.Json;
using System;
using System.IO;
using MediationModel;
using TelcobrightMediation.Config;

namespace TelcobrightFileOperations
{
    public class ErrorWriter
    {
        public ErrorWriter(Exception e,string processInformation,job telcobrightJob,string messageToPrepend,
            string operatorName, PartnerEntities partnerEntities=null)
        {
            try
            {
                string entityConStr = "";
                PartnerEntities context = null;
                if (partnerEntities == null)
                {
                    entityConStr = ConnectionManager.GetEntityConnectionStringByOperator(operatorName);
                    context = new PartnerEntities(entityConStr);
                }
                else {
                    context = partnerEntities;
                }

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
