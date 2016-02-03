using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kea.Services.Mail
{
    /// <summary>
    /// DTO for an email message
    /// </summary>
    public class EmailMessage
    {
        /// <summary>
        /// Email from 
        /// </summary>
        public string From { get; set; }

        /// <summary>
        /// Email to
        /// </summary>
        public List<string> To { get; } = new List<string>();

        /// <summary>
        /// Email subject
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Email body
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// True if the body represents an html body
        /// </summary>
        public bool IsBodyHtml { get; set; }
    }
}
