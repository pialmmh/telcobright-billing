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
        private Action<string> callbackFromExternalApp;

        public ConsoleRedirector(Action<string> callbackFromExternalApp)
        {
            this.callbackFromExternalApp = callbackFromExternalApp;
        }

        public override void Write(string value)
        {
            callbackFromExternalApp(value);
        }

        public override void WriteLine(string value)
        {
            callbackFromExternalApp(value);
        }

        public override System.Text.Encoding Encoding => System.Text.Encoding.Default;
    }
}
