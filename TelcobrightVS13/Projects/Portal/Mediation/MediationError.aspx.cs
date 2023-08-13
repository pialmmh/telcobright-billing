using MySql.Data.MySqlClient;
using PortalApp;
using System;
using System.Configuration;
using System.Web.UI.WebControls;
using PortalApp._portalHelper;

//using IgwModel;
//using telcobrightmediationModel;

public partial class DefaultMedError : System.Web.UI.Page
{
   

    protected void Timer1_Tick(object sender, EventArgs e)
    {
        if (this.CheckBoxRealTimeUpdate.Checked)
        {
            submit_Click(sender, e);
        }
    }

    protected void submit_Click(object sender, EventArgs e)
    {
        BindGrid();
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!this.IsPostBack)
        {
             
            //Retrieve Path from TreeView for displaying in the master page caption label
            TreeView masterTree = (TreeView) this.Master.FindControl("TreeView1");
            string localPath = this.Request.Url.LocalPath;
            int pos2NdSlash = localPath.Substring(1, localPath.Length - 1).IndexOf("/");
            string rootFolder = localPath.Substring(1, pos2NdSlash);
            int endOfRootFolder = this.Request.Url.AbsoluteUri.IndexOf(rootFolder);
            string urlWithQueryString = ("~" +"/"+rootFolder + this.Request.Url.AbsoluteUri.Substring((endOfRootFolder + rootFolder.Length), this.Request.Url.AbsoluteUri.Length - (endOfRootFolder + rootFolder.Length))).Replace("%20", " ");
            TreeNodeCollection cNodes = masterTree.Nodes;
            TreeNode matchedNode = null;
            CommonCode commonCodes = new CommonCode();
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

            
            BindGrid();
        }
    }

    protected void BindGrid()
    {
        using (MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["telcobrightmediationSql"].ConnectionString))
        {
            con.Open();
            string sql="select * from allerror";
            using (MySqlCommand cmd = new MySqlCommand(sql, con))
            {
                MySqlDataReader dr = cmd.ExecuteReader();
                this.GridView1.DataSource = dr;
            }
        }
    }
   
}