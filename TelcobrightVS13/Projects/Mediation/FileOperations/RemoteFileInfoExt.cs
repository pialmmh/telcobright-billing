using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinSCP;
namespace TelcobrightFileOperations
{
    public class RemoteFileInfoExt
    {
        public RemoteFileInfo RemoteFileInfo { get; set; }
        public String FullRelativePath { get; set; }
        public RemoteFileInfoExt(RemoteFileInfo remoteFileInfo,string relativePath) {
            this.RemoteFileInfo = remoteFileInfo;
            //string tempRelativePath = relativePath.Substring(1);//skip the first /
            //this.FullRelativePath = !tempRelativePath.EndsWith("/")? tempRelativePath + "/" + remoteFileInfo.Name
              //  :tempRelativePath + remoteFileInfo.Name;
            this.FullRelativePath = !relativePath.EndsWith("/") ? relativePath + "/" + remoteFileInfo.Name
                : relativePath+ remoteFileInfo.Name;
        }
    }
}
