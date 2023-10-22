using TelcobrightMediation;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using System.Collections;
using System.Drawing;
using ClosedXML.Excel;
using LibraryExtensions;
using PortalApp;
using MediationModel;

public partial class SiteMaster : System.Web.UI.MasterPage
{
    //for spring execution to set settings
    Hashtable _pageFunctions = new Hashtable();
    public delegate void Exptreenode(string nodeValue);
    public void ExpandTreeNode(string nodeValue)
    {
        (this.TreeView1.FindNode(nodeValue)).Expanded = true;
    }
    protected void Page_Load(object sender, EventArgs e)
    {

        TelcobrightConfig TelcoConf = PageUtil.GetTelcobrightConfig();
        var databaseSetting = TelcoConf.DatabaseSetting;
        string userName = Page.User.Identity.Name;
        string dbName;
        if (TelcoConf.DeploymentProfile.UserVsDbName.ContainsKey(userName))
        {
            dbName = TelcoConf.DeploymentProfile.UserVsDbName[userName];
        }
        else
        {
            dbName = TelcoConf.DatabaseSetting.DatabaseName;
        }
        databaseSetting.DatabaseName = dbName;
        //TreeView1 restore node states...
        if (this.Session["sesCommonState"] != null)
        {
            CommonCode thisState = this.Session["sesCommonState"] as CommonCode;
            foreach (KeyValuePair<string, TreeNode> dicItem in thisState.DicTreeState)
            {
                TreeNode nodeToExpand = this.TreeView1.FindNode(dicItem.Key);
                if (nodeToExpand != null)
                {
                    nodeToExpand.Expanded = dicItem.Value.Expanded;
                }
            }
        }

        

        if (!this.IsPostBack)
        {
            TreeView1.NodeStyle.ForeColor = System.Drawing.ColorTranslator.FromHtml("#08605c");
            TreeView1.SelectedNodeStyle.ForeColor = System.Drawing.ColorTranslator.FromHtml("#08605c");
            //TreeView1.NodeStyle.ForeColor = Color.Green;
            //TreeView1.SelectedNodeStyle.ForeColor = Color.Green;
            //            this.Session["isTreeLoaded"] = true;
            //Load Report Templates in TreeView dynically from database.
            CommonCode commonCode = new CommonCode();
            commonCode.LoadReportTemplatesTree(ref this.TreeView1);
            //if(HeadLoginView.tet)

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
            telcobrightpartner thisPartner = null;
            TelcobrightConfig tbc = PageUtil.GetTelcobrightConfig();
            

            if (tbc.PortalSettings.AlternateDisplayName.IsNullOrEmptyOrWhiteSpace())
            {
                using (PartnerEntities conTelco = PortalConnectionHelper.GetPartnerEntitiesDynamic(databaseSetting))
                {
                    thisPartner = conTelco.telcobrightpartners.Where(c => c.databasename == dbName).ToList().First();

                }
                //this.lblCustomerDisplayName.Text = thisPartner.CustomerName;
            }
            else
            {
                //this.lblCustomerDisplayName.Text = tbc.PortalSettings.AlternateDisplayName;
                //this.lblCustomerDisplayName.Text = "CDR Analyzer System (CAS)";
            }
            List<role> roles = PageUtil.GetRoles();
            //nodes can't be accessed by name, so adding them so that they can be parsed by spring expression
            //for applying page settings
            MasterPageExtendedForSpring mExt = new MasterPageExtendedForSpring(this);
            foreach (TreeNode thisNode in this.TreeView1.Nodes)//for each level 0 nodes
            {
                //mExt.nodes.Add(thisNode.Value, thisNode);
                AddNodesRecursivelyToDictionary(thisNode, mExt.Nodes);
            }
            
            //ExpressionEvaluator.SetValue(rootTreeNodes, "nodes['Report Templates'].Expanded", true);
            //apply master page settings
            List<role> currentRoles = PageUtil.GetRoles();
            //apply page settings
            PageUtil.ApplyPageSettings(mExt, true, tbc);
            //remove nodes other than expanded, for role based secutiry.  TV doesnot support visible property.
            if (currentRoles.Select(c => c.Name).Contains("admin") == false)
            {

                this.TreeView1.Nodes.Clear();
                List<TreeNode> authenticatedNodes = new List<TreeNode>();
                List<TreeNode> nonExpandedNodes = mExt.Nodes.Values.Where(n => n.Expanded == false).ToList();
                foreach (TreeNode node in mExt.Nodes.Values)
                {
                    bool bValid = true;
                    foreach (TreeNode neNode in nonExpandedNodes)
                    {
                        if (node.ValuePath.Contains(neNode.ValuePath))
                        {
                            bValid = false;
                            break;
                        }
                    }
                    if (bValid) authenticatedNodes.Add(node);
                }
                
                foreach (TreeNode node in authenticatedNodes)
                {
                   
                    if (node.Expanded == false) continue;
                    String[] parts = node.ValuePath.Split('/');
                    String parentPath = String.Empty;
                    for (int i = 0; i < parts.Length; i++)
                    {
                        if (i == 0)
                        {
                            TreeNode tNode = TreeView1.FindNode(parts[i]);
                            if (tNode == null)
                            {
                                tNode = new TreeNode(node.Text, node.Value, node.NavigateUrl);
                                if (node.NavigateUrl == String.Empty) tNode.SelectAction = TreeNodeSelectAction.None;
                                this.TreeView1.Nodes.Add(tNode);
                            }
                            parentPath = parts[i];
                        }
                        else
                        {
                            TreeNode tNode = TreeView1.FindNode(parentPath);
                            parentPath = $"{parentPath}/{parts[i]}";
                            TreeNode cNode = TreeView1.FindNode(parentPath);
                            if (cNode == null)
                            {
                                cNode = new TreeNode(node.Text, node.Value, node.ImageUrl, node.NavigateUrl,
                                    node.Target);
                                if (node.NavigateUrl == String.Empty) cNode.SelectAction = TreeNodeSelectAction.None;
                               
                                tNode.ChildNodes.Add(cNode);
                            }
                        }
                    }
                    //TreeNode parentNode = null;
                    //if (parts.Length > 1)
                    //{
                    //    String parentPath = String.Empty;
                    //    for (int i = 0; i < parts.Length - 1; i++)
                    //    {
                    //        if (i == 0) parentPath = parts[i];
                    //        else parentPath = String.Format("{0}/{1}", parentPath, parts[i]);
                    //    }
                    //    parentNode = TreeView1.FindNode(parentPath);
                    //}
                    //if (parentNode == null)
                    //{
                    //    TreeNode tNode = new TreeNode(node.Text, node.ValuePath, node.NavigateUrl);
                    //    if (node.NavigateUrl == String.Empty) tNode.SelectAction = TreeNodeSelectAction.None;
                    //    this.TreeView1.Nodes.Add(tNode);
                    //}
                    //else
                    //{
                    //    TreeNode tNode = new TreeNode(node.Text, node.ValuePath, node.NavigateUrl);
                    //    if (node.NavigateUrl == String.Empty) tNode.SelectAction = TreeNodeSelectAction.None;
                    //    parentNode.ChildNodes.Add(tNode);
                    //}
                }
            }
            //set home page link to dashboard if specified in portalsettings

            var x = (LinkButton)FindControl("LinkButtonHome");
            //if (!string.IsNullOrEmpty(tbc.PortalSettings.HomePageUrl))
            //{
            //    x.PostBackUrl = tbc.PortalSettings.HomePageUrl;
            //}
            if (dbName=="btrc_cas")
            {
                x.PostBackUrl = tbc.PortalSettings.HomePageUrl;
            }
            else
            {
                x.PostBackUrl = tbc.PortalSettings.HomePageUrlForIcx;
            }

        }//if not postback
       
    }

    public void AddNodesRecursivelyToDictionary(TreeNode thisNode, Dictionary<string, TreeNode> nodes)
    {
        Console.WriteLine(thisNode.Text);
        nodes.Add(thisNode.ValuePath, thisNode);
        // Start recursion on all subnodes.
        foreach (TreeNode oSubNode in thisNode.ChildNodes)
        {
            AddNodesRecursivelyToDictionary(oSubNode,nodes);
        }
    }

    protected void HeadLoginStatus_LoggingOut(object sender, LoginCancelEventArgs e)
    {
        //Session.Clear();
        //Context.User = null;
        //Response.Cache.SetCacheability(HttpCacheability.NoCache);
        //Response.Expires = 0;
        //Response.Cache.SetNoStore();
        //Response.AppendHeader("pragma", "no-cache");
        //HeadLoginView.Visible = false;
        //FormsAuthentication.SignOut();
        ////Roles.DeleteCookie();
        //Session.Clear();
        //FormsAuthentication.SignOut();
        //Session.Abandon();

        //// clear authentication cookie
        //HttpCookie cookie1 = new HttpCookie(FormsAuthentication.FormsCookieName, "");
        //cookie1.Expires = DateTime.Now.AddYears(-1);
        //Response.Cookies.Add(cookie1);

        //// clear session cookie (not necessary for your current problem but i would recommend you do it anyway)
        //HttpCookie cookie2 = new HttpCookie("ASP.NET_SessionId", "");
        //cookie2.Expires = DateTime.Now.AddYears(-1);
        //Response.Cookies.Add(cookie2);

        //FormsAuthentication.RedirectToLoginPage();

        //HttpContext.Current.Request.Cookies.AllKeys.ToList().ForEach(s => HttpContext.Current.Request.Cookies[s].Expires = DateTime.Now.AddDays(-1));
        //Session.Clear();
        //FormsAuthentication.SignOut();
        //Session.Abandon();

        var authenticationManager = HttpContext.Current.GetOwinContext().Authentication;
        authenticationManager.SignOut();
        //Response.Redirect("~/Account/Login.aspx");
    }


    protected void OnExpand(Object sender, EventArgs e)
    {
        //save node state for loading in postback...   
        CommonCode thisCommonState = new CommonCode();
        foreach (TreeNode n in this.TreeView1.Nodes)
        {
            thisCommonState.GetAllTreeNodes(n, ref thisCommonState.DicTreeState);
        }
        this.Session["sesCommonState"] = thisCommonState;
    }

    protected void OnCollapse(Object sender, EventArgs e)
    {
        //save node state for loading in postback...   
        CommonCode thisCommonState = new CommonCode();
        foreach (TreeNode n in this.TreeView1.Nodes)
        {
            thisCommonState.GetAllTreeNodes(n, ref thisCommonState.DicTreeState);
        }
        this.Session["sesCommonState"] = thisCommonState;
    }

    protected void TreeView1_OnItemSelected(Object sender, EventArgs e)
    {
        //Response.Redirect(TreeView1.SelectedNode.Value);
        
    }

    protected void Unnamed_LoggingOut(object sender, LoginCancelEventArgs e)
    {
        this.Context.GetOwinContext().Authentication.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
    }

}
