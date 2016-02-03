using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kea.Services.Mail
{
    /// <summary>
    /// SmtpClient mock factory
    /// </summary>
    public class SmtpClientFactoryMock : ISmtpClientFactory
    {
        ISmtpClient ISmtpClientFactory.Create(string Host, int Port, string User, string Password)
        {
            return new SmtpClientMock();
        }
    }
    class SmtpClientMock : ISmtpClient
    {
        async Task ISmtpClient.SendMailAsync(EmailMessage Message)
        {
            return;
        }
    }
}
