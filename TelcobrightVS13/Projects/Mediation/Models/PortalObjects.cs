using System.Collections.Generic;

namespace TelcobrightMediation
{
    public class SettingByRoles
    {
        public List<string> RoleNames { get; set; }
        public List<string> SpringExpressionIfRole { get; set; }
        public List<string> SpringExpressionIfNotRole { get; set; }
        
    }

    public class CdrFieldTemplate
    {
        public string FieldTemplateName { get; set; }
        public List<string> Fields { get; set; }
    }

    

    public class PortalPageSettings
    {
        
        public Dictionary<string, List<SettingByRoles>> DicPageSettingsByRole { get; set; }
        public PortalPageSettings()
        {
            this.DicPageSettingsByRole = new Dictionary<string, List<SettingByRoles>>();
        
        }
    }
}
