using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelcobrightMediation.Config
{
    public class EmailSenderConfig
    {
        public string SmtpHost { get; set; }
        public int SmtpPort { get; set; }
        public String Username { get; set; }
        public String Password { get; set; }
        public bool EnableSsl { get; set; }
        public string MailFrom { get; set; }
        public string MailFromDisplayName { get; set; }
    }
}
