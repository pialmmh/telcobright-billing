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
    public partial class CDRFileReconciliation : System.Web.UI.Page
    {
        TelcobrightConfig telcobrightConfig { get; set; }
        DatabaseSetting databaseSetting { get; set; }
        telcobrightpartner thisPartner { get; set; }
        PartnerEntities context { get; set; }
        string switchName = "";
        string dbName { get; set; }
        List<ne> nes { get; set; }
        Dictionary<string, CDRFileComparer> CDRFileComparers = new Dictionary<string, CDRFileComparer>();
        protected void Page_Load(object sender, EventArgs e)
        {
            TelcobrightConfig telcobrightConfig = PageUtil.GetTelcobrightConfig();
            DatabaseSetting databaseSetting = telcobrightConfig.DatabaseSetting;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // or LicenseContext.Commercial

            /////
            telcobrightConfig = PageUtil.GetTelcobrightConfig();
            databaseSetting = telcobrightConfig.DatabaseSetting;

            string userName = Page.User.Identity.Name;
            
            if (telcobrightConfig.DeploymentProfile.UserVsDbName.ContainsKey(userName))
            {
                this.dbName = telcobrightConfig.DeploymentProfile.UserVsDbName[userName];
            }
            else
            {
                this.dbName = telcobrightConfig.DatabaseSetting.DatabaseName;
            }
            databaseSetting.DatabaseName = this.dbName;

            this.context = PortalConnectionHelper.GetPartnerEntitiesDynamic(databaseSetting);
            List<telcobrightpartner> telcoTelcobrightpartners = context.telcobrightpartners.ToList();
            thisPartner = telcoTelcobrightpartners.Where(c => c.databasename == dbName).ToList().First();
            /////


            string selectNeQuery = $@"select * from ne where switchname!='dummy';";
            this.nes = context.Database.SqlQuery<ne>(selectNeQuery).ToList();           

            // Populate the DropDownList with the list values
            foreach (ne ne in nes)
            {
                ddlOptions.Items.Add(ne.SwitchName);
            }

        }

        protected void upload_Excel_File(object sender, EventArgs e)
        {
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
                    ExportButton.Enabled = true;
                    ////////////


                    List<telcobrightpartner> telcoTelcobrightpartners = context.telcobrightpartners.ToList();
                    thisPartner = telcoTelcobrightpartners.Where(c => c.databasename == dbName).ToList().First();



                    string switchName = "";
                    string[] jobNames = new string[CDRFileComparers.Count()];
                    int i = 0;
                    foreach (var item in CDRFileComparers)
                    {
                        jobNames[i] = "\'" + item.Value.FileName + "\'";
                        switchName = this.switchName;
                        i++;
                    }

                    Dictionary<string, int> switchNameVsId = new Dictionary<string, int>();
                    foreach(ne ne in nes)
                    {
                        switchNameVsId[ne.SwitchName] = ne.idSwitch;
                    }


                    string jobNamesToMatch = string.Join(",", jobNames);
                    string query = $@"SELECT *
                                            FROM job
                                            WHERE idne={switchNameVsId[this.switchName]}
                                            and JobName IN ({jobNamesToMatch})";
                    List<job> jobs = context.Database.SqlQuery<job>(query).ToList();
                    HashSet<string> fileNamesfromDB = new HashSet<string>();
                    foreach (job j in jobs)
                    {
                        string jobNameTB = j.JobName;
                        fileNamesfromDB.Add(jobNameTB);
                        string durationTB = j.JobSummary;
                        string recordCountTB = j.NoOfSteps.ToString();

                        CDRFileComparers[jobNameTB].ActualDuratinoTB = durationTB.ToString();
                        CDRFileComparers[jobNameTB].RecordCountFromTB = recordCountTB.ToString();

                        Decimal diffDuration = Decimal.Parse(CDRFileComparers[jobNameTB].ActualDurationFromICX) - Decimal.Parse(CDRFileComparers[jobNameTB].ActualDuratinoTB);
                        int diffRecordCount = int.Parse(CDRFileComparers[jobNameTB].RecordCountFromICX) - int.Parse(CDRFileComparers[jobNameTB].RecordCountFromTB);

                        CDRFileComparers[jobNameTB].DiffDuration = diffDuration.ToString();
                        CDRFileComparers[jobNameTB].DiffRecordCount = diffRecordCount.ToString();
                    }
                    //for missing file
                    
                    foreach(string keyFileName in CDRFileComparers.Keys)
                    {
                        if (!fileNamesfromDB.Contains(keyFileName))
                        {
                            CDRFileComparers[keyFileName].RecordCountFromTB = "File Missing";
                            CDRFileComparers[keyFileName].DiffRecordCount = "File Missing";
                            CDRFileComparers[keyFileName].ActualDuratinoTB = "File Missing";
                            CDRFileComparers[keyFileName].DiffDuration = "File Missing";
                        }
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
            ////
            using (var package = new ExcelPackage(new System.IO.FileInfo(fileName)))
            {
                if (package.Workbook.Worksheets.Count > 0)
                {
                    var firstSheet = package.Workbook.Worksheets[0];
                    switchName = firstSheet.Name; // sheet name same as switch name
                }
                else
                {
                    Console.WriteLine("No sheets found in the Excel file.");
                }
            }
            ////
            Dictionary<string, CDRFileComparer> CDRFileComparers = new Dictionary<string, CDRFileComparer>();

            for (int i = 0; i < rows.Count; i++)
            {
                if (i == 0 ) continue; // skipping the first row
                if (rows[i][0] == "" ) continue; // filename cant be empty

                CDRFileComparer obj = new CDRFileComparer(rows[i]);
                if (!CDRFileComparers.ContainsKey(obj.FileName))
                {
                    CDRFileComparers.Add(obj.FileName, obj);
                }
                
            }
            return CDRFileComparers;
        }
        protected void ExportButton_Click(object sender, EventArgs e)
        {
            ExportToExcel(GridView1);
        }
        private void ExportToExcel(GridView gridView)
        {
            string fileName = "CDR_comparer_export.xlsx";
            string folderPath = Server.MapPath("~/Downloads/");
            string filePath = Path.Combine(folderPath, fileName);
            //FileInfo excelFile = new FileInfo(filePath);
            using (ExcelPackage excelPackage = new ExcelPackage())
            {
                ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.Add("comparer_data");

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
        protected void DownloadButton_Click(object sender, EventArgs e)
        {
            string selectedSwitch = ddlOptions.SelectedValue;

            generate_Template_file(selectedSwitch);
            
        }
        void generate_Template_file(string selectedSwitch)
        {
            string fileName = "Template_File.xlsx";
            string folderPath = Server.MapPath("~/Downloads/");
            string filePath = Path.Combine(folderPath, fileName);
            //FileInfo excelFile = new FileInfo(filePath);
            if (!File.Exists(filePath))
            {
                File.Create(filePath);
            }
            using (ExcelPackage excelPackage = new ExcelPackage())
            {
                ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.Add(selectedSwitch);

                string[] headerRow = { "fileName", "recordCount", "Actual Duration Sum from Switch (sec)" };
                // Add header row
                for (int i = 0; i < headerRow.Length; i++)
                {
                    worksheet.Cells[1, i + 1].Value = headerRow[i];
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