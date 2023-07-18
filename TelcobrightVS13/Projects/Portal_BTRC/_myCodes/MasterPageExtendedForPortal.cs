using System.Collections.Generic;
using System.Web.UI.WebControls;

namespace PortalApp
{
    public class MasterPageExtendedForSpring
    {
        public Dictionary<string, TreeNode> Nodes { get; set; }
        public System.Web.UI.MasterPage MasterPage { get; set; }
        public MasterPageExtendedForSpring(System.Web.UI.MasterPage mPage)
        {
            this.MasterPage = mPage;
            this.Nodes = new Dictionary<string, TreeNode>();
        }
    }
}