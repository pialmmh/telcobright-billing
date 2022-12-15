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
            this.FullRelativePath = relativePath.Substring(1) + remoteFileInfo.Name;//skip the first /
        }
    }
}
