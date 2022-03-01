using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz;
namespace WS_Telcobright_Topshelf
{
    public class TriggerKeyExt
    {
        public Dictionary<string, string> ArgsTelcobright { get; set; } = new Dictionary<string, string>();
        public TriggerKey TriggerKey { get; set; }
        public TriggerKeyExt(TriggerKey triggerKey, string argsTelcobrightStr) {// argsTelcobrightStr example: name=mustafa age=30
            if(!string.IsNullOrEmpty(argsTelcobrightStr) && argsTelcobrightStr.Contains("="))
            this.ArgsTelcobright = argsTelcobrightStr.Split(null).Select(paramValue =>
            {
                var arr = paramValue.Split('=');
                return new KeyValuePair<string, string>(arr[0], arr[1]);
            }).ToDictionary(kv => kv.Key, kv=> kv.Value);
            this.TriggerKey = triggerKey;
        }
    }
}
