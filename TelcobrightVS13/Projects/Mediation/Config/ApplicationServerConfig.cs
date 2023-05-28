using System.Web.Script.Serialization;

namespace TelcobrightMediation
{
    public class ApplicationServerConfig
    {
        public int ServerId { get; set; }
        public string OwnIpAddress { get; set; }

        public ApplicationServerConfig(int serverId, string ownIpAddress)
        {
            ServerId = serverId;
            OwnIpAddress = ownIpAddress;
        }
    }
}
