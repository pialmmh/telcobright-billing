using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;
using MediationModel;
using PortalApp;
public partial class ConfigRouteImport : System.Web.UI.Page
{
    public enum LabelForeColor { Red, Green };

    protected void Page_Load(object sender, EventArgs e)
    {
        TreeView masterTree = (TreeView) this.Master.FindControl("TreeView1");
        //NameValueCollection n = Request.QueryString;
        CommonCode commonCodes = new CommonCode();
        //Retrieve Path from TreeView for displaying in the master page caption label
        string localPath = this.Request.Url.LocalPath;
        int pos2NdSlash = localPath.Substring(1, localPath.Length - 1).IndexOf("/");
        string rootFolder = localPath.Substring(1, pos2NdSlash);
        int endOfRootFolder = this.Request.Url.AbsoluteUri.IndexOf(rootFolder);
        string urlWithQueryString = ("~" + "/" + rootFolder + this.Request.Url.AbsoluteUri.Substring((endOfRootFolder + rootFolder.Length), this.Request.Url.AbsoluteUri.Length - (endOfRootFolder + rootFolder.Length))).Replace("%20", " ");
        TreeNodeCollection cNodes = masterTree.Nodes;
        TreeNode matchedNode = null;
        foreach (TreeNode n in cNodes)//for each nodes at root level, loop through children
        {
            matchedNode = commonCodes.RetrieveNodes(n, urlWithQueryString);
            if (matchedNode != null)
            {
                break;
            }
        }
        //set screentile/caption in the master page...
        Label lblScreenTitle = (Label) this.Master.FindControl("lblScreenTitle");
        if (matchedNode != null)
        {
            lblScreenTitle.Text = matchedNode.ValuePath;
        }
        else
        {
            lblScreenTitle.Text = "";
        }


        //End of Site Map Part *******************************************************************
    }

    protected void DeleteAndUpload_Click(object sender, EventArgs e)
    {
        Import(true);   
    }
    protected void UploadButton_Click(object sender, EventArgs e)
    {
        Import(false);   
    }

    public void Import(bool deleteallRoute)
    {
        try
        {
            if (this.FileUploadControl.HasFile == false)
            {
                UpdateStatusLabel(LabelForeColor.Red, "No file selected !");
                return;
            }
            

            if (this.FileUploadControl.PostedFile.ContentLength <= 10485760)
            {
                Stream thisStream = this.FileUploadControl.PostedFile.InputStream;
                var lines = ReadLines(() => thisStream, Encoding.UTF8, "").ToList();

                //skip first line then process the rest of the lines
                int updateCount = 0;
                int insertCount = 0;
                int deleteCount = 0;

                using (PartnerEntities context = new PartnerEntities())
                {
                    Dictionary<string, route> dicRoutes = new Dictionary<string, route>();
                    Dictionary<int, partner> dicPartner = new Dictionary<int, partner>();

                    foreach (partner cr in context.partners.ToList())
                    {
                        dicPartner.Add(cr.idPartner, cr);
                    }

                    List<int> lstSwitchIdForTbCustomer = new List<int>();
                    string tbCustomerName = "";
                    //entity framework transaction with mysql doesn't seem to work as expected
                    //use work around method, and at the end use sql with transaction to make the final changes.
                    if (deleteallRoute == false)
                    {   
                        foreach (route thisRoute in context.routes.ToList())
                        {
                            dicRoutes.Add(thisRoute.RouteName + "-" + thisRoute.SwitchId, thisRoute);
                        }
                    }

                    //check if this switchid is valid for this operator
                    string partnerConStr = ConfigurationManager.ConnectionStrings["reader"].ConnectionString;
                    int posDatabase = partnerConStr.IndexOf("database");
                    //make sure to keep databasename at the last of the connection string
                    string dbName = partnerConStr.Substring(posDatabase + 9, partnerConStr.Length - posDatabase - 9);
                    //find TB customerid
                    using (PartnerEntities contextmed = new PartnerEntities())
                    {
                        telcobrightpartner thisCustomer = contextmed.telcobrightpartners.Where(c => c.databasename == dbName).First();
                        int idOperator = thisCustomer.idCustomer;
                        tbCustomerName = thisCustomer.CustomerName;
                        lstSwitchIdForTbCustomer = contextmed.nes.Where(c => c.idCustomer == idOperator).Select(c => c.idSwitch).ToList();
                    }

                    for (int i = 1; i < lines.Count; i++)
                    {
                        string[] lineArr = lines[i].Split(',');
                        string routeName = lineArr[0];
                        if (routeName == "")
                        {
                            UpdateStatusLabel(LabelForeColor.Red, "Route/TG Name can't be empty: !");
                            return;
                        }


                        int switchId = -1;
                        int idPartner = -1;
                        int natOrInt = -1;
                        string description = "";
                        int ingressPort = -1;
                        int egressPort = -1;
                        int commonPort = -1;

                        int.TryParse(lineArr[1], out switchId);
                        if (switchId == -1)
                        {
                            UpdateStatusLabel(LabelForeColor.Red, "Invalid SwitchId in Row:" + (i+1) + " !");
                            return;
                        }

                        int.TryParse(lineArr[2], out idPartner);
                        if (idPartner == -1)
                        {
                            UpdateStatusLabel(LabelForeColor.Red, "Invalid idPartner in Row:" + (i + 1) + " !");
                            return;
                        }
                        //foreign key checking...
                        if (dicPartner.ContainsKey(idPartner) == false)
                        {
                            UpdateStatusLabel(LabelForeColor.Red, "idPartner:" + idPartner + " doesn't exist in partner table. Row:" + (i + 1) + " !");
                            return;
                        }

                        int.TryParse(lineArr[3], out natOrInt);
                        if (natOrInt == -1)
                        {
                            UpdateStatusLabel(LabelForeColor.Red, "Invalid National or International indicator in Row:" + (i + 1) + " !");
                            return;
                        }

                        description = lineArr[4];

                        int.TryParse(lineArr[5], out ingressPort);
                        if (ingressPort == -1)
                        {
                            UpdateStatusLabel(LabelForeColor.Red, "Invalid Ingress Port in Row:" + (i + 1) + " !");
                            return;
                        }

                        int.TryParse(lineArr[6], out egressPort);
                        if (egressPort == -1)
                        {
                            UpdateStatusLabel(LabelForeColor.Red, "Invalid Egress Port in Row:" + (i + 1) + " !");
                            return;
                        }

                        int.TryParse(lineArr[7], out commonPort);
                        if (commonPort == -1)
                        {
                            UpdateStatusLabel(LabelForeColor.Red, "Invalid Bothway Port in Row:" + (i + 1) + " !");
                            return;
                        }
                        else
                        {
                            //either Bothway Port can be set, or ingress+egress ports can be set
                            if (commonPort > 0 && (ingressPort>0||egressPort>0))
                            {
                                UpdateStatusLabel(LabelForeColor.Red, "If CommonPort is set, then both Ingress and Egress Ports have to be zero. Row:" + (i + 1) + " !");
                                return;
                            }
                        }

                        //check if this switchid is valid for this operator
                        if (lstSwitchIdForTbCustomer.Contains(switchId) == false)
                        {
                            UpdateStatusLabel(LabelForeColor.Red, "Invalid switchid:" + switchId + " for Telcobright customer:" + tbCustomerName + " Row:" +(i+1) +" !");
                            return;
                        }


                        //Find if this route already exists
                        route thisRoute=null;
                        string routeKey = routeName + "-" + switchId;
                        dicRoutes.TryGetValue(routeKey,out thisRoute);
                        if (thisRoute != null)
                        {//route exists, so update, in the dictionary
                            thisRoute.Description = description;
                            thisRoute.field2 = ingressPort;
                            thisRoute.field3 = egressPort;
                            thisRoute.field4 = commonPort;
                            updateCount++;
                        }
                        else
                        {//route doesn't exist, so insert
                            route newRoute = new route();
                            newRoute.RouteName = routeName;
                            newRoute.SwitchId = switchId;
                            newRoute.idPartner = idPartner;
                            newRoute.NationalOrInternational = natOrInt;
                            newRoute.Description = description;
                            newRoute.field2 = ingressPort;
                            newRoute.field3 = egressPort;
                            newRoute.field4 = commonPort;
                            newRoute.date1 = DateTime.Now;//keep this as a flag to insert this route, rather than update
                            dicRoutes.Add(routeKey, newRoute);
                            insertCount++;
                        }

                    }//for each line
                    
                    //delete, insert using sql, with safe transaction...update can be handled using context
                    if (deleteallRoute == true)
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
                                sql = " delete from route; ";
                                using (MySqlCommand cmd = new MySqlCommand(sql, con))
                                {
                                    cmd.CommandType = CommandType.Text;
                                    deleteCount=cmd.ExecuteNonQuery();
                                }

                                string sqlInsert = "";
                                foreach (KeyValuePair<string, route> route in dicRoutes)
                                {
                                    sqlInsert += "INSERT INTO route " +
                                                "(" +
                                                "`RouteName`," +
                                                "`SwitchId`," +
                                                "`idPartner`," +
                                                "`NationalOrInternational`," +
                                                "`Description`," +
                                                "`field2`," +
                                                "`field3`," +
                                                "`field4`) " +
                                                "VALUES" +
                                                "(" +
                                                "'" + route.Value.RouteName + "'" +
                                                "," + route.Value.SwitchId + "" +
                                                "," + route.Value.idPartner + "" +
                                                "," + route.Value.NationalOrInternational + "" +
                                                ",'" + route.Value.Description + "'" +
                                                "," + route.Value.field2 + "" +
                                                "," + route.Value.field3 + "" +
                                                "," + route.Value.field4 + ");";
                                }// for each line
                                using (MySqlCommand cmd = new MySqlCommand(sqlInsert, con))
                                {
                                    cmd.CommandType = CommandType.Text;
                                    cmd.ExecuteNonQuery();
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
                                UpdateStatusLabel(LabelForeColor.Green, "Successfully imported. " + deleteCount+ " routes deleted, " + insertCount + " new routes inserted, " + updateCount + "  routes updated.");

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
                    }//delete all=true
                    else //update/merge with existing route...
                    {
                        //routes in the dictionary are combination of existing and new routes.
                        //new routes are not in the database yet
                        foreach (KeyValuePair<string, route> route in dicRoutes)
                        {   
                            //MergeRoute = context.routes.Where(c => c.idroute == route.Value.idroute).First();
                            //the above line should have worked, but gave sequence contains no element...
                            //so use the data1 flag in dictionary
                            if (route.Value.date1== null) //update
                            {
                                route mergeRoute = null;
                                mergeRoute = context.routes.Where(c => c.idroute == route.Value.idroute).First();
                                mergeRoute.Description = route.Value.Description;
                                mergeRoute.field2 = route.Value.field2;
                                mergeRoute.field3 = route.Value.field3;
                                mergeRoute.field4 = route.Value.field4;
                            }
                            else//insert
                            {
                                route newRoute = new route();
                                newRoute.RouteName = route.Value.RouteName;
                                newRoute.SwitchId = route.Value.SwitchId;
                                newRoute.idPartner = route.Value.idPartner;
                                newRoute.NationalOrInternational = route.Value.NationalOrInternational;
                                newRoute.Description = route.Value.Description;
                                newRoute.field2 = route.Value.field2;
                                newRoute.field3 = route.Value.field3;
                                newRoute.field4 = route.Value.field4;
                                //omit the date field...
                                context.routes.Add(newRoute);
                                
                            }
                        }
                        //save changes...
                        context.SaveChanges();
                        UpdateStatusLabel(LabelForeColor.Green, "Successfully imported. " + insertCount + " new routes inserted, " + updateCount + "  routes updated.");
                    }
                    
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
}