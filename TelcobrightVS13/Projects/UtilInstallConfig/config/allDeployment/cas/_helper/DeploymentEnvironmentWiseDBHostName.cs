using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstallConfig
{
   
    public static class DeploymentEnvironmentWiseDBHostName
    {
        public static Dictionary<string, string> getDbHostName(string deploymentEnvironmentType)
        {
            switch (deploymentEnvironmentType)
            {
                case "production":
                    return new Dictionary<string, string>()
        {
            {"agni_cas","10.100.150.20"},
            {"banglatelecom_cas","10.100.150.20"},
            {"bantel_cas","10.100.150.20"},
            {"gazinetworks_cas","10.100.150.21"},
            {"summit_cas","10.100.150.21"},
            {"ringtech_cas","10.100.150.21"},
            {"mothertelecom_cas","10.100.150.22"},
            {"teleplusnewyork_cas","10.100.150.22"},
            {"voicetel_cas","10.100.150.22"},
            {"mnh_cas","10.100.150.23"},
            {"srtelecom_cas","10.100.150.23"},
            {"banglaicx_cas","10.100.150.24"},
            {"crossworld_cas","10.100.150.24"},
            {"btcl_cas","10.100.150.25"},
            {"imamnetwork_cas","10.100.150.25"},
            {"jibondhara_cas","10.100.150.26"},
            {"paradise_cas","10.100.150.26"},
            {"purple_cas","10.100.150.26"},
            {"softex_cas","10.100.150.27"},
            {"sheba_cas","10.100.150.27"},
            {"teleexchange_cas","10.100.150.27"},
            {"newgenerationtelecom_cas","10.100.150.27"}
                    };
                default:
                    return new Dictionary<string, string>()
        {
            {"agni_cas","localhost"},
            {"banglatelecom_cas","localhost"},
            {"bantel_cas","localhost"},
            {"gazinetworks_cas","localhost"},
            {"summit_cas","localhost"},
            {"btrc_cas","localhost" },
            {"ringtech_cas","localhost"},
            {"mothertelecom_cas","localhost"},
            {"teleplusnewyork_cas","localhost"},
            {"voicetel_cas","localhost"},
            {"mnh_cas","localhost"},
            {"srtelecom_cas","localhost"},
            {"banglaicx_cas","localhost"},
            {"crossworld_cas","localhost"},
            {"btcl_cas","localhost"},
            {"imamnetwork_cas","localhost"},
            {"jibondhara_cas","localhost"},
            {"paradise_cas","localhost"},
            {"purple_cas","localhost"},
            {"softex_cas","localhost"},
            {"sheba_cas","localhost"},
            {"teleexchange_cas","localhost"},
            {"newgenerationtelecom_cas","localhost"}
            };
            }

        }
    }
}
