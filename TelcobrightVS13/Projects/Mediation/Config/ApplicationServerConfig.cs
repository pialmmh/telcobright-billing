using System.Web.Script.Serialization;

namespace TelcobrightMediation
{
    public class ApplicationServerConfig
    {
        public int ServerId { get; set; }
        public string OwnIpAddress { get; set; }
        [ScriptIgnore]
        private TelcobrightConfig TelcobrightConfig { get; set; }
        public ApplicationServerConfig(TelcobrightConfig tbConfig)
        {
            this.TelcobrightConfig = tbConfig;
        }
    }
}
