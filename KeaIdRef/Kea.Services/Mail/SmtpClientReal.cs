using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kea.Services.Mail
{
    /// <summary>
    /// Return a read smtp client 
    /// </summary>
    public class SmtpClientFactory : ISmtpClientFactory
    {
        ISmtpClient ISmtpClientFactory.Create(string Host, int Port, string User, string Password)
        {
            return new SmtpClientReal(new System.Net.Mail.SmtpClient
            {
                Host = Host,
                Port = Port,
                EnableSsl = true,
                DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new System.Net.NetworkCredential(User, Password)
            });
        }
    }

    class SmtpClientReal : ISmtpClient
    {
        public SmtpClientReal(System.Net.Mail.SmtpClient client)
        {
            this.client = client;
        }
        readonly System.Net.Mail.SmtpClient client;
        async Task ISmtpClient.SendMailAsync(EmailMessage Message)
        {
            var M = new System.Net.Mail.MailMessage();
            M.From = new System.Net.Mail.MailAddress(Message.From);
            M.IsBodyHtml = Message.IsBodyHtml;
            M.Subject = Message.Subject;
            M.Body = Message.Body;
            foreach (var To in Message.To)
                M.To.Add(To);

            await this.client.SendMailAsync(M);
        }
    }
}
