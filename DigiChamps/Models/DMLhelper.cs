using System;
using System.Data;
using System.Configuration;
using System.Collections.Generic;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Web.UI.WebControls;
using System.Linq;
using System.Text;
using System.Threading;

/// <summary>
/// Summary description for DMLhelper
/// </summary>
/// 
namespace DigiChamps.Models
{

    public class CustomerMail
    {
         public string CustEmail { get; set; }
    }

    public class DMLhelper
    {
             
        public static bool sendMail(string eidTo, string msgsub, string msgbody)
        {
            MailMessage greetings = new MailMessage();
            SmtpClient smtp = new SmtpClient();
            try
            {
                var custemail = eidTo.Split(',');
                if (custemail.Count() > 0)
                {
                    foreach (var sCustomerEmailMain in custemail)
                    {
                        if (!string.IsNullOrEmpty(sCustomerEmailMain))
                        {
                            greetings.To.Add(sCustomerEmailMain.ToString());
                        }
                    }
                    greetings.From = new MailAddress("info@thedigichamps.com", "DIGICHAMPS");
                    greetings.IsBodyHtml = true;
                    greetings.Priority = MailPriority.High;
                    greetings.Body = msgbody;
                    greetings.Subject = msgsub;
                    smtp.Host = "smtp.zoho.com";
                    smtp.Port = 465;
                    smtp.EnableSsl = true;
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new NetworkCredential("info@thedigichamps.com", "Therevolution2017");
                    smtp.Send(greetings);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception) { return false; }
        }
      
        public static bool sendMail1(string eidTo, string eidFrom, string msgsub, string msgbody)
        {
            MailMessage greetings = new MailMessage();
            SmtpClient smtp = new SmtpClient();
            try
            {
                greetings.From = new MailAddress("info@thedigichamps.com", "ContactCustomer");
                greetings.To.Add(eidTo);
                greetings.IsBodyHtml = true;
                greetings.Priority = MailPriority.High;
                greetings.Body = msgbody;
                greetings.Subject = msgsub;
                smtp.Host = "smtp.zoho.com";
                smtp.EnableSsl = true;
                smtp.Port = 587;
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential("info@thedigichamps.com", "Therevolution2017");
                smtp.Send(greetings);
                return true;
            }
            catch (Exception) { return false; }
        }

        #region ThreadEmail
        public static bool _SendAsyncEmail(string sendto, string subject, string body)
        {
            bool status = false;
            //Thread email = new Thread(delegate()
            //{
            status = sendMail1(sendto, "", subject, body);
            //});
            //email.IsBackground = true;
            //email.Start();
            return status;
        }

        #endregion
    }
}