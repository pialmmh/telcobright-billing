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
                        switchName = item.Value.switchName;
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
                                            WHERE idne={switchNameVsId[switchName]}
                                            and JobName IN ({jobNamesToMatch})";
                    List<job> jobs = context.Database.SqlQuery<job>(query).ToList();
                    foreach (job j in jobs)
                    {
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

            for (int i = 0; i < rows.Count; i++)
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
            string fileName = "Template File.xlsx";
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

                string[] headerRow = { "SwitchName", "fileName", "recordCount", "DurationCount"};
                // Add header row
                for (int i = 0; i < headerRow.Length; i++)
                {
                    worksheet.Cells[1, i + 1].Value = headerRow[i];
                }

                // Add 5 switch col rows
                worksheet.Cells[2, 1].Value = selectedSwitch;
                worksheet.Cells[3, 1].Value = selectedSwitch;
                worksheet.Cells[4, 1].Value = selectedSwitch;
                worksheet.Cells[5, 1].Value = selectedSwitch;
                worksheet.Cells[6, 1].Value = selectedSwitch;


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