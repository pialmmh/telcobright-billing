using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelcobrightMediation.Config
{
    public class InternetSite
    {
        public string SiteType { get; set; }
        public string SiteName { get; set; }
        public int SiteId { get; set; }
        public string BindAddress { get; set; }
        public string PhysicalPath { get; set; }
        public string TemplateFileName { get; set; }
        public IisApplicationPool ApplicationPool { get; set; }
        public string ImpersonateUserName { get; set; }
        public string ImpersonatePassword { get; set; }
        public string ApplicationPoolName
        {
            get
            {
                return this.ApplicationPool.AppPoolName;
            }
        }
        public InternetSite(TelcobrightConfig tbc)
        {
            this.ApplicationPool = new IisApplicationPool();
        }
        public Dictionary<string, string> GetDicProperties()
        {
            Dictionary<string, string> dicParam = new Dictionary<string, string>();
            dicParam.Add("siteType", this.SiteType);
            dicParam.Add("siteName", this.SiteName);
            dicParam.Add("siteId", this.SiteId.ToString());
            dicParam.Add("bindAddress", this.BindAddress);
            dicParam.Add("physicalPath", this.PhysicalPath);
            dicParam.Add("templateFileName", this.TemplateFileName);
            dicParam.Add("applicationPoolName", this.ApplicationPool.AppPoolName);
            return dicParam;
        }
    }
}
