using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using TelcobrightMediation;

namespace PortalApp
{
    public static class ServiceGroupPopulatorForDropDown
    {
        public static void Populate(DropDownList ddlserviceGroup, TelcobrightConfig tbc)
        {
            ServiceGroupComposer serviceGroupComposer = new ServiceGroupComposer();
            serviceGroupComposer.ComposeFromPath(PageUtil.GetPortalBinPath() + "\\..\\Extensions");
            Dictionary<int, IServiceGroup> mefServiceGroups =
                serviceGroupComposer.ServiceGroups.ToDictionary(c => c.Id);
            foreach (KeyValuePair<int, ServiceGroupConfiguration> kv in tbc.CdrSetting.ServiceGroupConfigurations)
            {
                if (mefServiceGroups.ContainsKey(kv.Key))
                {
                    IServiceGroup thisServiceGroup = null;
                    mefServiceGroups.TryGetValue(kv.Key, out thisServiceGroup);
                    if (thisServiceGroup == null) throw new Exception("Service group not found for id=" + kv.Key);
                    ddlserviceGroup.Items.Add(new ListItem(thisServiceGroup.RuleName, kv.Key.ToString()));
                }
            }
        }
    }
}