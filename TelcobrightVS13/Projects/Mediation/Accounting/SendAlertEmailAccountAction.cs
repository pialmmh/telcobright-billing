using MediationModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using TelcobrightMediation.Config;

namespace TelcobrightMediation.Accounting
{
    public class SendAlertEmailAccountAction : IAutomationAction
    {
        public int Id { get; set; }
        public string ActionName { get; set; }
        public TelcobrightConfig Tbc { get; set; }

        public SendAlertEmailAccountAction()
        {
            this.Id = 1;
            this.ActionName = "Send Alert Email";
        }

        public void Execute(object data)
        {
            if (Tbc != null)
            {
                EmailSenderConfig emailSenderConfig = Tbc.EmailSenderConfig;
                partner partner = (partner)data;
                SmtpClient smtpClient = new SmtpClient(emailSenderConfig.SmtpHost, emailSenderConfig.SmtpPort);

                smtpClient.Credentials = new System.Net.NetworkCredential(emailSenderConfig.Username, emailSenderConfig.Password);
                smtpClient.UseDefaultCredentials = true;
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtpClient.EnableSsl = emailSenderConfig.EnableSsl;
                MailMessage mail = new MailMessage();

                //Setting From , To and CC
                mail.From = new MailAddress(emailSenderConfig.MailFrom, emailSenderConfig.MailFromDisplayName);
                mail.To.Add(new MailAddress(partner.email));
                // mail.CC.Add(new MailAddress("MyEmailID@gmail.com"));
                mail.Subject = "Account balance below threshold value";
                mail.Body =
                    "Dear Valued Partner, \n Our system indicates that you account balance is below threshold value. \n\n BR \n\n Telcobright Billing Portal";

                smtpClient.Send(mail);
            }
            else
            {
                throw new Exception("Email configuration missing.");
            }
        }
    }
}
