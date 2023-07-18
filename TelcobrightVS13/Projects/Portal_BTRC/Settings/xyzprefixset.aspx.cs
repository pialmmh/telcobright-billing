using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using MediationModel;

public partial class ConfigRouteImportxyz : Page
{
    public enum LabelForeColor { Red, Green };

    protected void Page_Load(object sender, EventArgs e)
    {

    }

    protected void DeleteAndUpload_Click(object sender, EventArgs e)
    {
        Import(true);
        this.GridViewPrefixSet.DataBind();
    }
    protected void UploadButton_Click(object sender, EventArgs e)
    {
        Import(false);
        this.GridViewPrefixSet.DataBind();
    }

    public void Import(bool deleteallPrefix)
    {
        try
        {
            if (this.HiddenFieldSelectedId.Value == "-1")
            {
                UpdateStatusLabel(LabelForeColor.Red, "No prefix set selected !");
                return;
            }
            if (this.FileUploadControl.HasFile == false)
            {
                UpdateStatusLabel(LabelForeColor.Red, "No file selected !");
                return;
            }
            

            if (this.FileUploadControl.PostedFile.ContentLength <= 10485760)
            {
                Stream thisStream = this.FileUploadControl.PostedFile.InputStream;
                var lines = ReadLines(() => thisStream, Encoding.UTF8, "").ToList();
                
                int skippedCount = 0;
                int insertCount = 0;
                int deleteCount = 0;

                using (PartnerEntities context = new PartnerEntities())
                {
                    Dictionary<string, xyzselected> dicPrefixsExisting = new Dictionary<string, xyzselected>();
                    Dictionary<string, xyzselected> dicInsertWithSkip = new Dictionary<string, xyzselected>();
                    Dictionary<string, xyzselected> dicInsertAllNewPrefix = new Dictionary<string, xyzselected>();
                    //load existing prefixes under this prefix set

                    int prefixSet=Convert.ToInt32(this.HiddenFieldSelectedId.Value);

                    foreach (xyzselected prefix in context.xyzselecteds.Where(c => c.PrefixSet == prefixSet).ToList())
                    {
                        dicPrefixsExisting.Add(prefix.prefix, prefix);
                    }
                    //entity framework transaction with mysql doesn't seem to work as expected
                    //use work around method, and at the end use sql with transaction to make the final changes.
                    
                    for (int i = 0; i < lines.Count; i++)
                    {
                        string[] lineArr = lines[i].Split(',');
                        string prefixName = lineArr[0];
                        if (prefixName == "")
                        {
                            UpdateStatusLabel(LabelForeColor.Red, "Prefix can't be empty: ! Line:"+ (i+1) +".");
                            return;
                        }

                        //Find if this Prefix already exists
                        xyzselected thisPrefix=null;
                        string prefixKey = prefixName;
                        dicPrefixsExisting.TryGetValue(prefixKey,out thisPrefix);
                        //add to all prefix dic, if not duplicate
                        if (dicInsertAllNewPrefix.ContainsKey(prefixName))
                        {
                            //duplicate in this batch
                            UpdateStatusLabel(LabelForeColor.Red, "Duplicate Prefix " + prefixName + " ! Line:" + (i + 1) + ".");
                            return;
                        }
                        dicInsertAllNewPrefix.Add(prefixKey, thisPrefix);
                        if (thisPrefix != null)
                        {//Prefix exists, so update count++ and skip
                            skippedCount++;
                        }
                        else
                        {//Prefix doesn't exist, so insert
                            //skip duplicate prefix if 
                            dicInsertWithSkip.Add(prefixKey, thisPrefix);
                            insertCount++;
                        }

                    }//for each line
                    
                    //delete, insert using sql, with safe transaction...update can be handled using context
                    if (deleteallPrefix == true)
                    {
                        using (MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["partner"].ConnectionString))
                        {
                            try
                            {

                                con.Open();
                                string sql = " set autocommit=0;";
                                using (MySqlCommand cmd = new MySqlCommand(sql, con))
                                {
                                    cmd.CommandType = CommandType.Text;
                                    cmd.ExecuteNonQuery();
                                }
                                sql = " delete from xyzselected where prefixset=" + prefixSet;
                                using (MySqlCommand cmd = new MySqlCommand(sql, con))
                                {
                                    cmd.CommandType = CommandType.Text;
                                    deleteCount=cmd.ExecuteNonQuery();
                                }

                                string sqlInsert = "";
                                foreach (KeyValuePair<string, xyzselected> prefix in dicInsertAllNewPrefix)
                                {
                                    sqlInsert += "INSERT INTO xyzselected " +
                                                "(" +
                                                "Prefix,PrefixSet) " +
                                                " VALUES" +
                                                "( " +
                                                "'" + prefix.Key + "'," + prefixSet +
                                                 ");";
                                }// for each line

                                if (sqlInsert != "")
                                {
                                    using (MySqlCommand cmd = new MySqlCommand(sqlInsert, con))
                                    {
                                        cmd.CommandType = CommandType.Text;
                                        cmd.ExecuteNonQuery();
                                    }
                                }

                                sql = " commit;";
                                using (MySqlCommand cmd = new MySqlCommand(sql, con))
                                {
                                    cmd.CommandType = CommandType.Text;
                                    cmd.ExecuteNonQuery();
                                }
                                sql = " set autocommit=1;";
                                using (MySqlCommand cmd = new MySqlCommand(sql, con))
                                {
                                    cmd.CommandType = CommandType.Text;
                                    cmd.ExecuteNonQuery();
                                }
                                UpdateStatusLabel(LabelForeColor.Green, "Successfully imported. " + deleteCount+ " Prefixes deleted, " + insertCount + " new Prefixes inserted, " + skippedCount + "  Prefixes skipped.");

                            }//try
                            catch (Exception e2)
                            {
                                string sql = " rollback;";
                                using (MySqlCommand cmd = new MySqlCommand(sql, con))
                                {
                                    cmd.CommandType = CommandType.Text;
                                    cmd.ExecuteNonQuery();
                                }
                                sql = " set autocommit=1;";
                                using (MySqlCommand cmd = new MySqlCommand(sql, con))
                                {
                                    cmd.CommandType = CommandType.Text;
                                    cmd.ExecuteNonQuery();
                                }
                                UpdateStatusLabel(LabelForeColor.Red, "Error while saving: " + e2.Message);
                            }
                        }//using mysql connection
                        UpdateStatusLabel(LabelForeColor.Green, "Successfully imported. " + deleteCount + " Prefixes deleted, " + dicInsertAllNewPrefix.Count + " new Prefixes inserted, " +  "0  Prefixes skipped.");
                    }//delete all=true
                    else //insert/skip with existing Prefix...
                    {
                        string sqlInsert = "";
                        foreach (KeyValuePair<string, xyzselected> prefix in dicInsertWithSkip)
                        {
                            sqlInsert += "INSERT INTO xyzselected " +
                                        "(" +
                                        "Prefix,PrefixSet) " +
                                        " VALUES" +
                                        "( " +
                                        "'" + prefix.Key + "'," + prefixSet +
                                         ");";
                        }// for each line

                        using (MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["partner"].ConnectionString))
                        {
                            try
                            {

                                con.Open();
                                string sql = " set autocommit=0;";

                                if (sqlInsert != "")
                                {
                                    using (MySqlCommand cmd = new MySqlCommand(sqlInsert, con))
                                    {
                                        cmd.CommandType = CommandType.Text;
                                        cmd.ExecuteNonQuery();
                                    }
                                }

                                sql = " commit;";
                                using (MySqlCommand cmd = new MySqlCommand(sql, con))
                                {
                                    cmd.CommandType = CommandType.Text;
                                    cmd.ExecuteNonQuery();
                                }
                                sql = " set autocommit=1;";
                                using (MySqlCommand cmd = new MySqlCommand(sql, con))
                                {
                                    cmd.CommandType = CommandType.Text;
                                    cmd.ExecuteNonQuery();
                                }
                            }
                            catch (Exception e3)
                            {
                                string sql = " rollback;";
                                using (MySqlCommand cmd = new MySqlCommand(sql, con))
                                {
                                    cmd.CommandType = CommandType.Text;
                                    cmd.ExecuteNonQuery();
                                }
                                sql = " set autocommit=1;";
                                using (MySqlCommand cmd = new MySqlCommand(sql, con))
                                {
                                    cmd.CommandType = CommandType.Text;
                                    cmd.ExecuteNonQuery();
                                }
                                UpdateStatusLabel(LabelForeColor.Red, "Error while saving: " + e3.Message);
                            }
                        }
                        UpdateStatusLabel(LabelForeColor.Green, "Successfully imported. " + dicInsertWithSkip.Count + " new Prefixs inserted, " + (dicInsertAllNewPrefix.Count-dicInsertWithSkip.Count) + "  Prefixs skipped.");
                    }//just insert and skip
                    
                }//context end

                
                
            }//if fileaide 5MB
            else
            {
                UpdateStatusLabel(LabelForeColor.Red, "File size can't exceed 10 MB");
                return;
            }

        }//try
        catch (Exception e1)
        {
            UpdateStatusLabel(LabelForeColor.Red, "Error occured during import:" + e1.InnerException + " !");
        }
    }//import function

    public void UpdateStatusLabel(LabelForeColor thisColor, string labelText)
    {
        var textColor = ColorTranslator.FromHtml("#00FF00");//initialize;
        switch (thisColor)
        {
            case LabelForeColor.Red:
                textColor = ColorTranslator.FromHtml("#FF0000");
                break;
            case LabelForeColor.Green:
                textColor = ColorTranslator.FromHtml("#1C5E55");
                break;
        }

        this.StatusLabel.ForeColor = textColor;
        this.StatusLabel.Text = labelText;
        this.GridViewPrefixSet.DataBind();
    }

    public IEnumerable<string> ReadLines(Func<Stream> streamProvider,Encoding encoding, string removeChar)
    {
        using (var stream = streamProvider())
        using (var reader = new StreamReader(stream, encoding))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (removeChar == "") yield return line;
                else yield return line.Replace(removeChar, "");
            }
        }
    }
    protected void LinkButtonNewPrefix_Click(object sender, EventArgs e)
    {
        this.FormView1.DefaultMode = FormViewMode.Insert;
    }
    protected void LinkButtonCancel_Click(object sender, EventArgs e)
    {
        this.FormView1.DefaultMode = FormViewMode.ReadOnly;
        this.lblValidation.Visible = false;
    }
    protected void FormView1_ItemInserted(object sender, FormViewInsertedEventArgs e)
    {
        this.FormView1.DefaultMode = FormViewMode.ReadOnly;
        this.GridViewPrefixSet.DataBind();
        this.lblValidation.Visible = false;
        this.GridViewPrefixSet.DataBind();
    }
    protected void FormView1_ItemInserting(object sender, FormViewInsertEventArgs e)
    {
        //validate
        string name = "";
        using (PartnerEntities context = new PartnerEntities())
        {
            name = ((TextBox) this.FormView1.FindControl("NameTextBox")).Text;
            if (context.xyzprefixsets.Any(c => c.Name == name))
            {
                this.lblValidation.Text = " A Prefix Set with name '" + name + "' already exists!";
                this.lblValidation.Visible = true;
                e.Cancel = true;
            }
        }
    }
    protected void EntityDataPrefix_QueryCreated(object sender, QueryCreatedEventArgs e)
    {
        int setId=-1;
        if (int.TryParse(this.HiddenFieldSelectedId.Value, out setId) == true)
        {
            var prefixes = e.Query.Cast<xyzselected>();
            e.Query = from c in prefixes
                      where c.PrefixSet == setId
                      select c;
        }
        else
        {
            //return no result
            var prefixes = e.Query.Cast<xyzselected>();
            e.Query = from c in prefixes
                      where c.PrefixSet == -100
                      select c;
        }
    }
    protected void GridViewPrefixSet_SelectedIndexChanged(object sender, EventArgs e)
    {
        this.HiddenFieldSelectedId.Value = this.GridViewPrefixSet.SelectedDataKey.Value.ToString();
        this.ListView1.DataBind();
    }
    protected void GridViewPrefixSet_DataBound(object sender, EventArgs e)
    {
        if (this.GridViewPrefixSet.Rows.Count > 0)
        {
            if (this.HiddenFieldSelectedId.Value == "-1")//no selection
            {
                this.GridViewPrefixSet.Rows[0].RowState = DataControlRowState.Selected;
                this.HiddenFieldSelectedId.Value = this.GridViewPrefixSet.DataKeys[0]["id"].ToString();
            }
            else
            {
                //GridViewPrefixSet.Rows[Convert.ToInt32(HiddenFieldSelectedId.Value)].RowState = DataControlRowState.Selected;
                //it's done in row databound event
                
            }
        }
        using (PartnerEntities context = new PartnerEntities())
        {
            this.lblPrefixSet.Text = " " + this.HiddenFieldSelectedId.Value;
            int idSet = Convert.ToInt32(this.HiddenFieldSelectedId.Value);
            this.lblNoOfPrefix.Text = context.xyzselecteds.Where(c => c.PrefixSet == idSet).Count().ToString() + " ";
        }
        this.ListView1.DataBind();
    }
    protected void GridViewPrefixSet_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        
        if (Convert.ToInt32(DataBinder.Eval(e.Row.DataItem, "id")) == Convert.ToInt32(this.HiddenFieldSelectedId.Value))
        {
            e.Row.RowState = DataControlRowState.Selected;
        }
                
    }
    protected void ListView1_ItemDeleted(object sender, ListViewDeletedEventArgs e)
    {
        this.GridViewPrefixSet.DataBind();
    }
    
    protected void FormViewPrefixInsert_ItemInserting(object sender, FormViewInsertEventArgs e)
    {
        using (PartnerEntities context = new PartnerEntities())
        {   
            string strPrefix=((TextBox) this.FormViewPrefixInsert.FindControl("prefixTextBox")).Text;
            if (strPrefix != "")
            {
                int prefixSet=Convert.ToInt32(this.HiddenFieldSelectedId.Value);
                
                xyzselected newPrefix = context.xyzselecteds.Where(c=>c.PrefixSet==prefixSet && c.prefix==strPrefix).FirstOrDefault();
                if (newPrefix == null)
                {
                    newPrefix = new xyzselected();
                    newPrefix.prefix = strPrefix;
                    newPrefix.PrefixSet = prefixSet;
                    context.xyzselecteds.Add(newPrefix);
                    context.SaveChanges();
                    this.lblPrefixValidate.Text = "";
                    ((TextBox) this.FormViewPrefixInsert.FindControl("prefixTextBox")).Text = "";
                    this.FormViewPrefixInsert.Visible = false;
                    this.GridViewPrefixSet.DataBind();
                }
                else
                {
                    this.lblPrefixValidate.Text = "Prefix " + strPrefix + " already exists!";
                }
            }
        }
        e.Cancel = true;
    }
    protected void NewButton_Click1(object sender, EventArgs e)
    {
        this.FormViewPrefixInsert.DefaultMode = FormViewMode.Insert;
        this.FormViewPrefixInsert.Visible = true;
    }

    protected void DeleteButton_Click1(object sender, EventArgs e)
    {
        using (MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["partner"].ConnectionString))
        {
            con.Open();
            using(MySqlCommand cmd=new MySqlCommand(" delete from xyzselected where prefixset="
                + this.HiddenFieldSelectedId.Value.ToString(), con))
                {
                    cmd.ExecuteNonQuery();
                }
        }
        this.GridViewPrefixSet.DataBind();
    }

    protected void PrefixInsertCancel_Click(object sender, EventArgs e)
    {
        ((TextBox) this.FormViewPrefixInsert.FindControl("prefixTextBox")).Text = "";
        this.FormViewPrefixInsert.DefaultMode = FormViewMode.Edit;
        this.FormViewPrefixInsert.Visible = false;
        this.GridViewPrefixSet.DataBind();
    }

    protected void FormViewPrefixInsert_ModeChanging(object sender, FormViewModeEventArgs e)
    {

    }
    protected void FormView1_ModeChanging(object sender, FormViewModeEventArgs e)
    {

    }
    protected void ListView1_PagePropertiesChanged(object sender, EventArgs e)
    {
        this.GridViewPrefixSet.DataBind();
    }
}