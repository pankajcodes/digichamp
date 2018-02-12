using DigiChamps.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;

namespace DigiChamps.Common
{
    public static class EmailHelper
    {
        public static void SendEmail(string sendTo, string subject, string msg)
        {
            string smtpServer = ConfigurationManager.AppSettings["smtpServer"];
            int smtpPort = Convert.ToInt32(ConfigurationManager.AppSettings["smtpPort"]);
            string smtpUsername = ConfigurationManager.AppSettings["smtpUsername"];            
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

            //string[] toArray = smtpBccUsername.Split(';');
            //foreach (string addr in toArray)
            //{
            //    MailAddress to = new MailAddress(addr);
            //    mailMessage.To.Add(to);
            //}

            //string[] bccArray = smtpBccUsername.Split(';');
            //foreach (string addr in bccArray)
            //{
            //    MailAddress bcc = new MailAddress(addr);
            //    mailMessage.Bcc.Add(bcc);
            //}
            mailMessage.To.Add(sendTo);
            smtpClient.Send(mailMessage);
        }

        public bool sendMail1(string parameter, string email, string sub, string name, string msgbody)
        {
            DigiChampsEntities DbContext = new DigiChampsEntities();
            var getall = DbContext.SP_DC_Get_maildetails(parameter).FirstOrDefault();
            string eidTo = email;
            string mailtoshow = getall.SMTP_Email.ToString();
            string eidFrom = getall.SMTP_User.ToString();
            string password = getall.SMTP_Pwd.ToString();
            string msgsub = sub;
            string hostname = getall.SMTP_HostName;
            string portname = getall.SMTP_Port.ToString();
            bool ssl_tof = true;
            //string msgbody = getall.EmailConf_Body.ToString().Replace("{{name}}", name);
            MailMessage greetings = new MailMessage();
            SmtpClient smtp = new SmtpClient();
            try
            {
                greetings.From = new MailAddress(mailtoshow, "DIGICHAMPS");//sendername
                greetings.To.Add(eidTo);//to whom
                greetings.IsBodyHtml = true;
                greetings.Priority = MailPriority.High;
                greetings.Body = msgbody;
                greetings.Subject = msgsub;
                smtp.Host = hostname;//host name
                smtp.EnableSsl = ssl_tof;//ssl
                smtp.Port = Convert.ToInt32(portname);//port
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential(eidFrom, password);//from(user)//password
                smtp.Send(greetings);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}