using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kea.Services.Mail
{
    /// <summary>
    /// SmtpClient factory
    /// </summary>
    public interface ISmtpClientFactory
    {
        /// <summary>
        /// Create an ISmtpClient
        /// </summary>
        /// <param name="Host">Host name</param>
        /// <param name="Port">Port number</param>
        /// <param name="User">Network credential user</param>
        /// <param name="Password">Password</param>
        /// <returns></returns>
        ISmtpClient Create(string Host, int Port, string User, string Password);
    }

    /// <summary>
    /// SmtpClient capable of sending emails
    /// </summary>
    public interface ISmtpClient
    {
        /// <summary>
        /// Send an email message asynchronously
        /// </summary>
        /// <returns></returns>
        Task SendMailAsync(EmailMessage Message);
    }


}
