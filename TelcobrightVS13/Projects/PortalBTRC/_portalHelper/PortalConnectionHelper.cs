using TelcobrightMediation;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
using System.IO;
using System.Web;
using Spring.Expressions;
using Spring.Core.TypeResolution;
using MediationModel;
using TelcobrightMediation.Config;
using LibraryExtensions.ConfigHelper;
using TelcobrightInfra;
/// <summary>
/// Summary description for CommonCode
/// </summary>

namespace PortalApp
{
    public class PortalConnectionHelper
    {
        public static PartnerEntities GetPartnerEntitiesDynamic(DatabaseSetting databaseSetting) {
            return new PartnerEntities(DbUtil.GetEntityConnectionString(databaseSetting));
        }

    }
}
