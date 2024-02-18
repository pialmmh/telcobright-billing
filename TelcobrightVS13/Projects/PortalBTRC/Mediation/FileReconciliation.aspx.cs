using System;
using System.IO;
using PortalApp;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using PortalApp._myCodes;
using PortalApp._portalHelper;
using LibraryExtensions.ConfigHelper;
using MediationModel;
using TelcobrightMediation;
using System.Data.SqlClient;
using MySql.Data.MySqlClient;
using OfficeOpenXml;

namespace PortalApp.Mediation
{
    public partial class FileReconciliation : System.Web.UI.Page
    {
        TelcobrightConfig telcobrightConfig { get; set; }
        DatabaseSetting databaseSetting { get; set; }
        telcobrightpartner thisPartner { get; set; }
        Dictionary<string, CDRFileComparer> CDRFileComparers = new Dictionary<string, CDRFileComparer>();
        protected void Page_Load(object sender, EventArgs e)
        {
            TelcobrightConfig telcobrightConfig = PageUtil.GetTelcobrightConfig();
            DatabaseSetting databaseSetting = telcobrightConfig.DatabaseSetting;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // or LicenseContext.Commercial

        }

        protected void submit_Click(object sender, EventArgs e){
            if (fileUpload.HasFile)
            {
                try
                {
                    string uploadDirectory = Path.Combine(PageUtil.GetPortalBinPath(), "tmp");
                    if (!Directory.Exists(uploadDirectory))
                    {
                        Directory.CreateDirectory(uploadDirectory);
                    }

                    string fileName = Path.Combine(uploadDirectory, Path.GetFileName(fileUpload.FileName));
                    fileUpload.SaveAs(fileName);

                    
                    CDRFileComparers = getCDRFileInfoFromExcel(fileName); // parse the uploaded excel file 

                    ////////////

                    telcobrightConfig = PageUtil.GetTelcobrightConfig();
                    databaseSetting = telcobrightConfig.DatabaseSetting;

                    string userName = Page.User.Identity.Name;
                    string dbName;
                    if (telcobrightConfig.DeploymentProfile.UserVsDbName.ContainsKey(userName))
                    {
                        dbName = telcobrightConfig.DeploymentProfile.UserVsDbName[userName];
                    }
                    else
                    {
                        dbName = telcobrightConfig.DatabaseSetting.DatabaseName;
                    }
                    databaseSetting.DatabaseName = dbName;
                    
                    PartnerEntities context = PortalConnectionHelper.GetPartnerEntitiesDynamic(databaseSetting);
                    List<telcobrightpartner> telcoTelcobrightpartners = context.telcobrightpartners.ToList();
                    thisPartner = telcoTelcobrightpartners.Where(c => c.databasename == dbName).ToList().First();



                    int IdSwitch = 0;
                    string[] jobNames = new string[CDRFileComparers.Count()];
                    int i = 0;
                    foreach (var item in CDRFileComparers)
                    {
                        jobNames[i] = "\'" + item.Value.FileName + "\'";
                        IdSwitch = int.Parse(item.Value.switchName);
                        i++;
                    }

                    
                    string jobNamesToMatch = string.Join(",", jobNames);
                    string query = $@"SELECT *
                                            FROM job
                                            WHERE idne={IdSwitch}
                                            and JobName IN ({jobNamesToMatch})";
                    List<job> jobs= context.Database.SqlQuery<job>(query).ToList();
                    foreach(job j in jobs)
                    {
                        //look up targetcdrcompare instancce
                        //update values from j
                        //e.g. j.stepscount
                        //j.jobsummary
                        string jobNameTB = j.JobName;
                        Decimal durationTB = Decimal.Parse(j.JobSummary);
                        int recordCountTB = Convert.ToInt32(j.NoOfSteps);

                        CDRFileComparers[jobNameTB].ActualDuratinoTB = durationTB;
                        CDRFileComparers[jobNameTB].RecordCountFromTB = recordCountTB;

                        Decimal diffDuration = CDRFileComparers[jobNameTB].ActualDurationFromICX - CDRFileComparers[jobNameTB].ActualDuratinoTB;
                        int diffRecordCount = CDRFileComparers[jobNameTB].RecordCountFromICX - CDRFileComparers[jobNameTB].RecordCountFromTB;

                        CDRFileComparers[jobNameTB].DiffDuration = diffDuration;
                        CDRFileComparers[jobNameTB].DiffRecordCount = diffRecordCount;                        

                    }



                    GridView1.DataSource = CDRFileComparers;
                    GridView1.DataBind();


                    Response.Write("File uploaded and processed successfully!");
                }
                catch (Exception ex)
                {
                    Response.Write("Error: " + ex.Message);
                }
            }
            else
            {
                Response.Write("Please select a file to upload.");
            }
        }

        private Dictionary<string, CDRFileComparer> getCDRFileInfoFromExcel(string fileName)
        {
            List<string[]> rows = ExcelHelper.parseExcellRows(fileName);
            Dictionary<string, CDRFileComparer> CDRFileComparers = new Dictionary<string, CDRFileComparer>();

            for(int i = 0; i < rows.Count; i++)
            {
                if (i == 0) continue; // skipping the first row
                CDRFileComparer obj = new CDRFileComparer(rows[i]);
                CDRFileComparers.Add(obj.FileName, obj);
            }
            return CDRFileComparers;
        }
        protected void ExportButton_Click(object sender, EventArgs e)
        {
            ExportToExcel(GridView1);
        }
        private void ExportToExcel(GridView gridView)
        {
            string fileName = "CDRData.xlsx";
            string folderPath = Server.MapPath("~/Downloads/");
            string filePath = Path.Combine(folderPath, fileName);
            //FileInfo excelFile = new FileInfo(filePath);
            using (ExcelPackage excelPackage = new ExcelPackage())
            {
                ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.Add("CDRData");

                // Add header row
                for (int i = 0; i < gridView.HeaderRow.Cells.Count; i++)
                {
                    worksheet.Cells[1, i + 1].Value = gridView.HeaderRow.Cells[i].Text;
                }

                // Add data rows
                for (int i = 0; i < gridView.Rows.Count; i++)
                {
                    for (int j = 0; j < gridView.Rows[i].Cells.Count; j++)
                    {
                        worksheet.Cells[i + 2, j + 1].Value = gridView.Rows[i].Cells[j].Text;
                    }
                }

                // Save the file
                //string fileName = "CDRData.xlsx";
                //FileInfo excelFile = new FileInfo(Server.MapPath($"C:/Users/Mahathir/Downloads/{fileName}"));
                excelPackage.SaveAs(new FileInfo(filePath));
            }
            // Provide the file for download
            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("Content-Disposition", $"attachment; filename={fileName}");
            Response.TransmitFile(filePath);
            Response.Flush();
            HttpContext.Current.ApplicationInstance.CompleteRequest();

        }
    }
}
