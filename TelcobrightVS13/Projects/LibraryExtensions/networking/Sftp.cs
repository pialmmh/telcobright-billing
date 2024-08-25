using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Renci.SshNet;
using Renci.SshNet.Common;
using Renci.SshNet.Sftp;
using System.IO;
namespace LibraryExtensions
{
    public class Sftp
    {
        SftpClient sftp { get; set; }
        public Sftp(string host, int port, string username, string password)
        {
            this.sftp = new SftpClient(host, port, username, password);
            this.sftp.Connect();
        }
        public void getFile(string remoteFileName, string localFileName)
        {
            using (var file = File.OpenWrite(localFileName))
            {
                this.sftp.DownloadFile(remoteFileName, file);
            }
        }

        public long getFileSize(string remoteFileName)
        {
            var fileInfo = sftp.GetAttributes(remoteFileName);
            // Retrieve the file size
            long fileSize = fileInfo.Size;
            return fileSize;
        }

        public void disconnect()
        {
            sftp.Disconnect();
        }
    }
}
