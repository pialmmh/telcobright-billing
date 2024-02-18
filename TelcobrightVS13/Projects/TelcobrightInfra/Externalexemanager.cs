using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace TelcobrightInfra
{
    public enum ExternalResourceType{
        SevenZip,
        WinScp
    }
    public static class ExternalResourceManager
    {
        public static string getToolsAndScriptPath()
        {
            //string toolsAndScript = "ToolsAndScripts\exetools";
            UpwordPathFinder<DirectoryInfo> pathFinder = new UpwordPathFinder<DirectoryInfo>("ToolsAndScripts");
            return pathFinder.FindAndGetFullPath();
         }
        public static string getExeternalResourcesPath()
        {
            return Path.Combine(getToolsAndScriptPath(), "externalResources");
        }

        public static string getResourcePath(ExternalResourceType resourceType)
        {
            switch (resourceType)
            {
                case ExternalResourceType.SevenZip:
                    return Path.Combine(getExeternalResourcesPath(), "7z.exe");
                    break;
                case ExternalResourceType.WinScp:
                    throw new NotImplementedException();
                    break;
                default:
                    throw new Exception("Resource Type Not Found");
                    break;
            }

            throw new Exception("Resource Type Not Found");
        }
    }
}
