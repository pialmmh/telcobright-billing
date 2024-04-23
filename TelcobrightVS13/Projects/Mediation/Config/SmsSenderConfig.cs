using System;
using System.Collections.Generic;

namespace TelcobrightMediation.Config
{
    public class SmsSenderConfig
    {
        public string ApiUrl { get; set; }
        public String Username { get; set; }
        public String Password { get; set; }
        public List<string> DestinationNumber { get; set; }
        public string Source { get; set; }
        public string Message { get; set; }
    }
}
