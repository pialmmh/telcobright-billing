using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryExtensions
{
    public class ConsoleRedirector : TextWriter
    {
        private Action<string> callbackFromUI;
        private string instanceName;

        public ConsoleRedirector(string instanceName, Action<string> callbackFromUi)
        {
            this.instanceName = instanceName;
            this.callbackFromUI = callbackFromUi;
        }

        public override void Write(string value)
        {
            callbackFromUI(value);
        }

        public override void WriteLine(string outputFromConsole)
        {
            string prefix = "%%%%%";
            string ownConsoleIdentifier = prefix + this.instanceName + prefix;
            if (outputFromConsole.StartsWith(ownConsoleIdentifier))
            {
                callbackFromUI(outputFromConsole.Replace(ownConsoleIdentifier, ""));
            }
        }

        public override System.Text.Encoding Encoding => System.Text.Encoding.Default;
    }
}
