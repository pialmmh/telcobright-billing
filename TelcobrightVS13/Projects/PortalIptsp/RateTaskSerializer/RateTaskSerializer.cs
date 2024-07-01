using TelcobrightMediation;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using MediationModel;
using Process = System.Diagnostics.Process;

namespace RateTaskSerializer
{
    class Program
    {
        static void Main(string[] args)
        {
            //List<string> lstArgs = args[0].Replace("`", " ").Split(Convert.ToChar(27)).ToList();
            //string Argument = @"C:\Google`Drive\Telcobright`Customers`Work\Roots`Telcobright`Work\Rate`Sheet\latest_31Oct15'Bharti_2015_10_06_ROOTSX_Wholesale_Gold_Voice_s84_key88891164.xls'39'MF'C:\Dropbox\TelcobrightVS13\Projects\Portal\Extensions";
            List<string> lstArgs = args[0].Replace("`", " ").Split(Convert.ToChar(39)).ToList();
            //kill excel again, doesn't want to die. wait before trying again
            bool excelExists = Process.GetProcessesByName("Excel").ToList().Any();
            while (excelExists)
            {
                foreach (Process process in Process.GetProcessesByName("Excel"))
                {
                    try
                    {
                        process.Kill();
                    }
                    catch (Exception e2)
                    {
                        continue;
                    }
                }
                Thread.Sleep(250);
                if(Process.GetProcessesByName("Excel").ToList().Any()==false)
                {
                    excelExists = false;
                }
                else
                {
                    excelExists = true;
                }
            }
            
            int dim = args.GetLength(0);//when using for debugging
            //Console.WriteLine("Hello...............");
            //Console.Write("argument1:" + args[1]);
            string fileUploadDirectory = dim>0?lstArgs[0]: @"C:\Google Drive\Telcobright Customers Work\Roots Telcobright Work\Rate Sheet\latest_31Oct15";
            //Console.WriteLine("UploadDirectory:" + FileUploadDirectory);
            string importFilename = dim>0?lstArgs[1]: @"Bharti_2015_10_06_ROOTSX_Wholesale_Gold_Voice_s84_key88891164.xls";//defaults when debuggin, does not affect real execution
            //Console.WriteLine("ImportFilename:" + ImportFilename);
            string tempExportFilename = importFilename + ".temp";//write as .temp, rename when finished
            string finalExportFilename = tempExportFilename.Replace(".temp", ".ratetask");
            int idRatePlan = dim > 0 ? Convert.ToInt32(lstArgs[2]) : 39;
            //Console.WriteLine("idRateplan:" + idRatePlan);
            string dateParseSelector = dim > 0 ? lstArgs[3] : "MF";
            //Console.WriteLine("DateParseSelector:" + DateParseSelector);
            string extensionDirectory = dim > 0 ? lstArgs[4] : @"C:\Dropbox\TelcobrightVS13\Projects\Portal\Extensions";
            //Console.WriteLine("ExtensionDirectory:" + ExtensionDirectory);
            //Console.ReadLine();
            //for debug
            try
            {

                RateSheetFormatContainer rsMefData = new RateSheetFormatContainer();
                rsMefData.Composer.Compose(extensionDirectory);
                foreach (IRateSheetFormat ext in rsMefData.Composer.RateSheetFormats)
                {
                    rsMefData.DicExtensions.Add(ext.Id.ToString(), ext);
                }

                string[] dateFormats = DateFormatHelper.GetDateFormats(dateParseSelector);

                rateplan thisRatePlan = null;
                using (PartnerEntities conpartner = new PartnerEntities())
                {
                    int intInd = Convert.ToInt32(idRatePlan);
                    thisRatePlan = conpartner.rateplans.Where(c => c.id == intInd).ToList().First();
                }


                List<ratetask> lstRateTask = new List<ratetask>();
                MyExcel pExcel = new MyExcel();
                int idRateSheetFormat = pExcel.GetVendorFormat(fileUploadDirectory + Path.DirectorySeparatorChar + importFilename, ref lstRateTask, thisRatePlan, false, dateFormats);
                if (rsMefData.DicExtensions.ContainsKey(idRateSheetFormat.ToString()) == false)
                {
                    idRateSheetFormat = 1;//try generic/text/excel
                }
                IRateSheetFormat thisRsFormat = rsMefData.DicExtensions[idRateSheetFormat.ToString()];
                string retval = thisRsFormat.GetRates(fileUploadDirectory + Path.DirectorySeparatorChar + importFilename, ref lstRateTask, thisRatePlan, false, dateFormats);

                using (FileStream fs = File.Open(fileUploadDirectory + Path.DirectorySeparatorChar + tempExportFilename, FileMode.CreateNew))
                using (StreamWriter sw = new StreamWriter(fs))
                using (JsonWriter jw = new JsonTextWriter(sw))
                {
                    jw.Formatting = Formatting.Indented;
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(jw, lstRateTask);
                }

                //File.WriteAllText(, JsonConvert.SerializeObject());
                if (File.Exists(fileUploadDirectory + Path.DirectorySeparatorChar + finalExportFilename))
                {
                    File.Delete(fileUploadDirectory + Path.DirectorySeparatorChar + finalExportFilename);
                }
                File.Move(fileUploadDirectory + Path.DirectorySeparatorChar + tempExportFilename,
                    fileUploadDirectory + Path.DirectorySeparatorChar + finalExportFilename);



            }
            catch (Exception e1)
            {
                File.WriteAllText(fileUploadDirectory + Path.DirectorySeparatorChar + tempExportFilename, e1.Message + (e1.InnerException != null ? e1.InnerException.ToString() : ""));
                File.Move(fileUploadDirectory + Path.DirectorySeparatorChar + tempExportFilename,
                    fileUploadDirectory + Path.DirectorySeparatorChar + finalExportFilename);
            }

        }
       }
}
