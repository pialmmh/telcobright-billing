using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelcobrightMediation.Config
{
    public class IisApplicationPool
    {
        public string AppPoolName { get; set; }
        public string TemplateFileName { get; set; }
        public Dictionary<string, string> GetDicProperties()
        {
            Dictionary<string, string> dicParam = new Dictionary<string, string>();
            dicParam.Add("appPoolName", this.AppPoolName);
            dicParam.Add("templateFileName", this.TemplateFileName);
            return dicParam;
        }
    }
}
