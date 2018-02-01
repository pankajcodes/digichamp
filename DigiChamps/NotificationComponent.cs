using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Text;
using System.Data.SqlClient;
using Microsoft.AspNet.SignalR;
using DigiChamps.Models;
using System.Web.Mvc;
using DigiChamps.Controllers;
using System.Net.Mail;

namespace DigiChamps
{
    public class NotificationComponent
    {
        string conStr = ConfigurationManager.ConnectionStrings["sqlConString"].ConnectionString;
        
        //Here we will add a function for register notification (will add sql dependency)
        public void RegisterNotification(DateTime currentTime)
        {
            //Package Expire notify
            //ExpirePackage();
            //using (SqlConnection con = new SqlConnection(conStr))
            //{
            //string sqlCommand = @"SELECT [Package_Name],[Price] from [dbo].[tbl_DC_Package] where [Inserted_Date] > @Inserted_Date";
            //    SqlCommand cmd = new SqlCommand(sqlCommand, con);
            //    cmd.Parameters.AddWithValue("@Inserted_Date", currentTime);
            //    if (con.State != System.Data.ConnectionState.Open)
            //    {
            //        con.Open();
            //    }
            //    cmd.Notification = null;
            //    SqlDependency sqlDep = new SqlDependency(cmd);
            //    sqlDep.OnChange += sqlDep_OnChange;
            //    //we must have to execute the command here
            //    using (SqlDataReader reader = cmd.ExecuteReader())
            //    {
            //        // nothing need to add here now
            //    }
            //}
        }

        //Package Expire notification Mail and popup
        //private void ExpirePackage()
        //{
        //    //throw new NotImplementedException();
        //}

        public void TicketNotification(DateTime currentTime)
        {
            //using (SqlConnection con = new SqlConnection(conStr))
            //{
            //    string sqlCommand = @"SELECT [Ticket_No],[Student_ID] from [dbo].[tbl_DC_Ticket] where [Inserted_Date] > @Inserted_Date";
            //    SqlCommand cmd = new SqlCommand(sqlCommand, con);
            //    cmd.Parameters.AddWithValue("@Inserted_Date", currentTime);
            //    if (con.State != System.Data.ConnectionState.Open)
            //    {
            //        con.Open();
            //    }
            //    cmd.Notification = null;
            //    SqlDependency sqlDep = new SqlDependency(cmd);
            //    sqlDep.OnChange += sqlDep_OnChange;
            //    //we must have to execute the command here
            //    using (SqlDataReader reader = cmd.ExecuteReader())
            //    {
            //        // nothing need to add here now
            //    }
            //}
        }


        void sqlDep_OnChange(object sender, SqlNotificationEventArgs e)
        {
            if (e.Type == SqlNotificationType.Change)
            {
                SqlDependency sqlDep = sender as SqlDependency;
                sqlDep.OnChange -= sqlDep_OnChange;

                //from here we will send notification message to client
                var notificationHub = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
                notificationHub.Clients.All.notify("added");

                //re-register notification
                DateTime today = DigiChampsModel.datetoserver();
                RegisterNotification(today);
                TicketNotification(today);

            }
        }

        public List<PackagePreviewModel> GetContacts(DateTime afterDate)
        {
            using (var dc =  new DigiChampsEntities())
            { 
                var obj = (from a in  dc.tbl_DC_Package.Where(a => a.Inserted_Date > afterDate).OrderByDescending(a=>a.Inserted_Date) select new PackagePreviewModel { Package_Name = a.Package_Name,Inserted_Date= a.Inserted_Date }).ToList();
                return obj;
            }
           
        }
        public List<DigiChampsModel.DigiChampTicketModel> Gettickets(DateTime afterDate)
        {
            using (var dc = new DigiChampsEntities())
            {
                var obj = (from a in dc.tbl_DC_Ticket.Where(a => a.Inserted_Date > afterDate).OrderByDescending(a => a.Inserted_Date) select new DigiChampsModel.DigiChampTicketModel { Ticket_No = a.Ticket_No, Inserted_Date = a.Inserted_Date }).ToList();
                return obj;
            }

        }

        private bool ExpirePackage()
        {
            try
            {
                using (var dc = new DigiChampsEntities())
                {
                    var data = (from a in dc.tbl_DC_Registration.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                join b in dc.tbl_DC_Order.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                    on a.Regd_ID equals b.Regd_ID
                                join c in dc.tbl_DC_Order_Pkg.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                    on b.Order_ID equals c.Order_ID
                                select new DigiChampCartModel
                                {
                                    OrderPkg_ID = c.OrderPkg_ID,
                                    firstname = a.Customer_Name,
                                    Package_ID = c.Package_ID,
                                    email = a.Email,
                                    Package_Name = c.Package_Name,
                                    Modified_By = c.Modified_By,
                                    Expiry_Date = c.Expiry_Date
                                }).ToList();
                    if (data.Count > 0)
                    {
                        foreach (var item in data)
                        {
                            DateTime expdate = Convert.ToDateTime(item.Expiry_Date);
                            DateTime tdy = DateTime.Now;
                            int total_days = Convert.ToInt32((expdate - tdy).TotalDays);
                            string days = total_days.ToString();
                            int day = Convert.ToInt32(days);
                            if (item.Modified_By == null)
                            {
                                string days1 = null;
                                if (total_days <= 5 && total_days >= 1)
                                {
                                    if (days == days1)
                                    {

                                    }
                                    else
                                    {
                                        var getall = dc.SP_DC_Get_maildetails("PKGNOTY").FirstOrDefault();
                                        string name = item.firstname;
                                        DateTime pur_date = Convert.ToDateTime(item.Package_From);
                                        DateTime exp_date = pur_date.AddDays(day);
                                        string date = Convert.ToString(exp_date);
                                        string msgbody = getall.EmailConf_Body.ToString().Replace("{{name}}", name).Replace("{{pkgname}}", item.Package_Name).Replace("{{day}}", days).Replace("{{exp_date}}", date);
                                        sendMail1("PKGNOTY", item.email, getall.Email_Subject, name, msgbody);
                                        int ord_pkg_id = Convert.ToInt32(item.OrderPkg_ID);
                                        var obj = dc.tbl_DC_Order_Pkg.Where(x => x.OrderPkg_ID == ord_pkg_id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                                        obj.Modified_By = days;
                                        dc.SaveChanges();
                                    }
                                }
                            }
                            else
                            {
                                string days1 = item.Modified_By;
                                if (total_days <= 5 && total_days >= 1)
                                {
                                    if (days == days1)
                                    {

                                    }
                                    else
                                    {
                                        var getall = dc.SP_DC_Get_maildetails("PKGNOTY").FirstOrDefault();
                                        string name = item.firstname;
                                        string msgbody = getall.EmailConf_Body.ToString().Replace("{{name}}", name).Replace("{{pkgname}}", item.Package_Name).Replace("{{day}}", days);
                                        sendMail1("PKGNOTY", item.email, getall.Email_Subject, name, msgbody);
                                        int ord_pkg_id = Convert.ToInt32(item.OrderPkg_ID);
                                        var obj = dc.tbl_DC_Order_Pkg.Where(x => x.OrderPkg_ID == ord_pkg_id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                                        obj.Modified_By = days;
                                        dc.SaveChanges();
                                    }
                                }
                            }

                        }
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
            //throw new NotImplementedException();
        }
        public bool sendMail1(string parameter, string email, string sub, string name, string msgbody)
        {
            var dc = new DigiChampsEntities();
            var getall = dc.SP_DC_Get_maildetails(parameter).FirstOrDefault();
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
                smtp.Credentials = new System.Net.NetworkCredential(eidFrom, password);//from(user)//password
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