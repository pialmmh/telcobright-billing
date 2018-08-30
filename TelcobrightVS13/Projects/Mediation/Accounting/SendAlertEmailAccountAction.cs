using MediationModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace TelcobrightMediation.Accounting
{
    public class SendAlertEmailAccountAction : IAutomationAction
    {
        public int Id { get; set; }
        public string ActionName { get; set; }

        public SendAlertEmailAccountAction()
        {
            this.Id = 1;
            this.ActionName = "Send Alert Email";
        }

        public void Execute(object data)
        {
            partner partner = (partner) data;
            SmtpClient smtpClient = new SmtpClient("mail.telcobright.com", 25);

            smtpClient.Credentials = new System.Net.NetworkCredential("noreply@telcobright.com", "myPassword");
            smtpClient.UseDefaultCredentials = true;
            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtpClient.EnableSsl = true;
            MailMessage mail = new MailMessage();

            //Setting From , To and CC
            mail.From = new MailAddress("noreply@MyWebsiteDomainName", "Telcobright Billing Portal");
            mail.To.Add(new MailAddress(partner.email));
            // mail.CC.Add(new MailAddress("MyEmailID@gmail.com"));
            mail.Subject = "Account balance below threshold value";
            mail.Body =
                "Dear Valued Partner, \n Our system indicates that you account balance is below threshold value. \n\n BR \n\n Telcobright Billing Portal";

            smtpClient.Send(mail);

        }
    }
}
