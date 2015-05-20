using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Mail;

namespace ServerLogica.Models
{
    class Email
    {
        private string sendTo, subject, emailText;

        public string SendTo
        {
            get { return sendTo; }
            set { sendTo = value; }
        }
        public string Subject
        {
            get { return subject; }
            set { subject = value; }
        }
        public string EmailText
        {
            get { return emailText; }
            set { emailText = value; }
        }

        public Email(string sendTo, string subject, string emailText)
        {
            SendTo = sendTo;
            Subject = subject;
            EmailText = emailText;
            
        }

        public void SendEmail()
        {
            // Add reference System.Net
            MailMessage mail = new MailMessage("projectburocenter@projectburocenter.be", SendTo);
            SmtpClient client = new SmtpClient();
            client.Host = "localhost";
            client.Port = 25;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential("projectburocenter", "Bur0C3nt3r2015");


            mail.Subject = Subject;
            mail.Body = EmailText;
            client.Send(mail);
        }

        public void SendPDF(string path)
        {
            MailMessage mail = new MailMessage("projectburocenter@projectburocenter.be", SendTo);
            SmtpClient client = new SmtpClient();
            client.Host = "localhost";
            client.Port = 25;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential("projectburocenter", "Bur0C3nt3r2015");


            mail.Subject = Subject;
            mail.Body = EmailText;
            mail.Attachments.Add(new Attachment(path));
            client.Send(mail);

        }
    }
}
