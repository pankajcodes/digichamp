using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Web;

namespace DigiChamps.Common
{
    public static class EmailHelper
    {
        public static void SendEmail(string sendTo, string subject, string msg)
        {
            string smtpServer = ConfigurationManager.AppSettings["smtpServer365"];
            int smtpPort = Convert.ToInt32(ConfigurationManager.AppSettings["smtpPort365"]);
            string smtpUsername = ConfigurationManager.AppSettings["smtpUsername"];
            string smtpBccUsername = ConfigurationManager.AppSettings["smtpBccUsername"];
            string smtpPassword = ConfigurationManager.AppSettings["smtpPassword"];
            string smtpDisplayName = ConfigurationManager.AppSettings["smtpDisplayName"];
            SmtpClient smtpClient = new SmtpClient(smtpServer, smtpPort)
            {
                EnableSsl = true,
                Credentials = new System.Net.NetworkCredential(smtpUsername, smtpPassword)
            };
            MailAddress from = new MailAddress(smtpUsername, smtpDisplayName, System.Text.Encoding.UTF8);



            MailMessage mailMessage = new MailMessage()
            {
                Body = msg,
                Subject = subject,
                IsBodyHtml = true,
                BodyEncoding = System.Text.Encoding.UTF8,
                SubjectEncoding = System.Text.Encoding.UTF8
            };


            mailMessage.From = from;

            string[] toArray = smtpBccUsername.Split(';');
            foreach (string addr in toArray)
            {
                MailAddress to = new MailAddress(addr);
                mailMessage.To.Add(to);
            }

            string[] bccArray = smtpBccUsername.Split(';');
            foreach (string addr in bccArray)
            {
                MailAddress bcc = new MailAddress(addr);
                mailMessage.Bcc.Add(bcc);
            }
            smtpClient.Send(mailMessage);
        }
    }
}