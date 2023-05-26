using System;
using System.Collections.Generic;
using Renci.SshNet.Common;

namespace sshSender
{
    internal class SshCommandLineRunner
    {
        private object ip;
        private string password;
        private object port;
        private object username;

        public SshCommandLineRunner(object ip, object username, string password, object port)
        {
            this.ip = ip;
            this.username = username;
            this.password = password;
            this.port = port;
        }

        internal void Connect()
        {
            throw new NotImplementedException();
        }

        internal IDisposable CreateShellStream(string v1, int v2, int v3, int v4, int v5, int v6, Dictionary<TerminalModes, uint> modes)
        {
            throw new NotImplementedException();
        }
    }
}