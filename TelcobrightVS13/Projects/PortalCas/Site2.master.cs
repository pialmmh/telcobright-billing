using Microsoft.AspNet.Identity;
using PortalApp;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI.WebControls;

public partial class Site2Master : System.Web.UI.MasterPage
{
    protected void Page_Load(object sender, EventArgs e)
    { 
        
        //TreeView1 restore node states...
        if (this.Session["sesCommonState"] != null)
        {
            CommonCode thisState = this.Session["sesCommonState"] as CommonCode;
            foreach (KeyValuePair<string, TreeNode> dicItem in thisState.DicTreeState)
            {
                TreeNode node = this.TreeView1.FindNode(dicItem.Key);
                if(node!=null)
                {
                    node.Expanded = dicItem.Value.Expanded;
                }
                
            }
        }

       

            if (!this.IsPostBack)
        {

            //Load Report Templates in TreeView dynically from database.
            CommonCode commonCode = new CommonCode();
            commonCode.LoadReportTemplatesTree(ref this.TreeView1);
            //if(HeadLoginView.tet)
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
