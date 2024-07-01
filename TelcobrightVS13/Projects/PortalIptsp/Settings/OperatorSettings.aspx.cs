using PortalApp;
using System;
using System.Web.UI.WebControls;
using PortalApp._portalHelper;

public class SettingsGlobalsettingsOpSetting : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!this.IsPostBack)
        {
            //common code for report pages
            string localPath = this.Request.Url.LocalPath;
            int pos2NdSlash = localPath.Substring(1, localPath.Length - 1).IndexOf("/");
            string pagePathInTree = "~" + localPath.Substring(pos2NdSlash + 1, localPath.Length - pos2NdSlash - 1);
            TreeView masterTree = (TreeView) this.Master.FindControl("TreeView1");
            TreeNodeCollection cNodes = masterTree.Nodes;

            TreeNode matchedNode = null;

            CommonCode commonCodes = new CommonCode();
            foreach (TreeNode n in cNodes)
            {
                matchedNode = commonCodes.RetrieveNodes(n, pagePathInTree);
                if (matchedNode != null)
                {
                    break;
                }
            }

            Label lblScreenTitle = (Label) this.Master.FindControl("lblScreenTitle");
            if (matchedNode != null)
            {
                lblScreenTitle.Text = matchedNode.ValuePath;
            }
            else
            {
                lblScreenTitle.Text = "";
            }
            //End of Common and Site Map Part *******************************************************************
        }
    }
}