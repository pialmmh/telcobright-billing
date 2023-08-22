using ExportToExcel;
using TelcobrightMediation;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web.UI.WebControls;
using MediationModel;
using PortalApp;
public partial class ConfigViewRoutes : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!this.IsPostBack)
        {

            //Retrieve Path from TreeView for displaying in the master page caption label

            TreeView masterTree = (TreeView) this.Master.FindControl("TreeView1");
            CommonCode commonCodes = new CommonCode();
            commonCodes.LoadReportTemplatesTree(ref masterTree);

            string localPath = this.Request.Url.LocalPath;
            int pos2NdSlash = localPath.Substring(1, localPath.Length - 1).IndexOf("/");
            string rootFolder = localPath.Substring(1, pos2NdSlash);
            int endOfRootFolder = this.Request.Url.AbsoluteUri.IndexOf(rootFolder);
            string urlWithQueryString = ("~" + "/" + rootFolder + this.Request.Url.AbsoluteUri.Substring((endOfRootFolder + rootFolder.Length), this.Request.Url.AbsoluteUri.Length - (endOfRootFolder + rootFolder.Length))).Replace("%20", " ");
            //for some reason url was not including .aspx
            if(urlWithQueryString.EndsWith(".aspx==")==false)
            {
                urlWithQueryString += ".aspx";
            }
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
            
            
            using (PartnerEntities context = new PartnerEntities())
            {
                IQueryable<partner> partners = from c in context.partners
                                               select c;
            }

            using (PartnerEntities context = new PartnerEntities())
            {
                IList<enumpartnertype> partnerTypes = (from c in context.enumpartnertypes
                                                       select c).ToList();
                this.Session["sesPartnerTypes"] = partnerTypes;


            }


            //load ne for this tb parter
            //get any ne of this telcobright partner, required by rate handling objects
            string conStrPartner = ConfigurationManager.ConnectionStrings["partner"].ConnectionString;
            string dbNameAppConf = "";
            foreach (string param in conStrPartner.Split(';'))
            {
                if (param.ToLower().Contains("database"))
                {
                    dbNameAppConf = param.Split('=')[1].Trim('"');
                    break;
                }
            }
            List<ne> lstNe = new List<ne>();
            using (PartnerEntities conTelco = new PartnerEntities())
            {
                lstNe = conTelco.telcobrightpartners.Where(c => c.databasename == dbNameAppConf).First().nes.ToList();
            }
            this.DropDownListNE.Items.Clear();
            this.DropDownListNE.Items.Add(new ListItem(" [All]","-1"));
            foreach (ne thisne in lstNe)
            {
                this.DropDownListNE.Items.Add(new ListItem(thisne.SwitchName, thisne.idSwitch.ToString()));
            }

        }
    }

    protected void Button1_Click(object sender, EventArgs e)
    {
        try
        {
            List<route.DisplayClass> lst=GetList();
            this.GridView1.DataSource = lst;
            this.GridView1.DataBind();
            this.lblStatus.Text = lst.Count + " Records";
            if(lst.Count>0)
            {
                this.ButtonExport.Visible = true;
            }
            else
            {
                this.ButtonExport.Visible = false;
            }
        }
        catch(Exception e1)
        {
            this.lblStatus.ForeColor = Color.Red;
            this.lblStatus.Text = e1.Message + "<br/>" + (e1.InnerException != null ? e1.InnerException.ToString() : "");
        }
        
    }

    public List<route.DisplayClass> GetList()
    {
        //get DisplayClassFirst
        string extensionDirectory = (Directory.GetParent(System.Web.HttpRuntime.BinDirectory)).Parent.FullName + Path.DirectorySeparatorChar
                        + "Extensions";
        RouteDisplayClassData dcMefData = new RouteDisplayClassData();
        dcMefData.Composer.Compose(extensionDirectory);
        foreach (IDisplayClass ext in dcMefData.Composer.DisplayClasses)
        {
            dcMefData.DicExtensions.Add(ext.Id.ToString(), ext);
        }
        IDisplayClass thisDisplayClass = dcMefData.DicExtensions["1"];
        
        int idPartner = Convert.ToInt32(this.ddlistPartner.SelectedValue);
        int idne = Convert.ToInt32(this.DropDownListNE.SelectedValue);
        string routeName = this.TextBoxName.Text.ToLower();
        List<route> lstRoute = new List<route>();
        string sql = " select * from route where idroute>0 ";

        if (idne > 0)
        {
            sql += " and switchid=" + idne;
        }
        if (idPartner > 0)
        {
            sql += " and idpartner=" + idPartner;
        }
        if (routeName != "")
        {
            sql += " and instr(routename,'" + routeName + "')>0 ";
        }

        using (PartnerEntities context = new PartnerEntities())
        {
            lstRoute = context.Database.SqlQuery<route>(sql).ToList();
        }
        List<route.DisplayClass> lstRouteDisplay = new List<route.DisplayClass>();
        foreach(route r in lstRoute)
        {
            lstRouteDisplay.Add((route.DisplayClass)thisDisplayClass.GetDisplayClass(r));
        }
        return lstRouteDisplay;   
    }

    protected void ButtonExport_Click(object sender, EventArgs e)
    {
        
        List<route.DisplayClass> lst = GetList();
        CreateExcelFileAspNet.CreateExcelDocumentAndWriteBrowser(lst, "Routes.xlsx", this.Response);

    }

}