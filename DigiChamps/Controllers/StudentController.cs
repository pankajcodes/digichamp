using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.Entity;
using DigiChamps.Models;
using DigiChamps.Controllers;
using System.Web.Security;
using System.IO;
using System.Text;
using System.Net;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Collections.Specialized;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using CCA.Util;

namespace DigiChamps.Controllers
{
    [HandleError]
    public class StudentController : Controller
    {
        DigiChampsEntities DbContext = new DigiChampsEntities();
        DateTime today = DigiChampsModel.datetoserver();

        [HttpGet]
        public ActionResult Index()
        {
            ViewBag.Board_Id = new SelectList(DbContext.tbl_DC_Board.Where(x => x.Is_Active == true && x.Is_Deleted == false), "Board_Id", "Board_Name");
            ViewBag.secureqsn = new SelectList(DbContext.tbl_DC_Security_Question.Where(x => x.Is_Active == true && x.Is_Deleted == false), "Security_Question_ID", "Security_Question");
            ViewBag.blog = DbContext.tbl_DC_Marketing_Blog.Where(x => x.Is_Active == true && x.Is_Deleted == false).ToList();
            ViewBag.team = DbContext.tbl_DC_Our_Team.Where(x => x.Is_Active == true && x.Is_Deleted == false).ToList();

            if (Session["USER_CODE"] != null)
            {
                if (Session["ROLE_CODE"].ToString() == "C")
                {
                    return RedirectToAction("studentdashboard","student");
                }
                else {
                    return RedirectToAction("logout");
                }
            }
            return View();
        }
        [HttpPost]
        public JsonResult Index(string User_name, string password)
        {
            string msg = string.Empty;
            try
            {
                string pass_word = DigiChampsModel.Encrypt_Password.HashPassword(password);
                var obj = DbContext.tbl_DC_USER_SECURITY.Where(x => x.USER_NAME == User_name).FirstOrDefault();

                if (obj != null)
                {
                    if (obj.PASSWORD == pass_word)
                    {
                        if (obj.STATUS == "A")
                        {
                            if (obj.ROLE_CODE == "C")
                            {
                                if (obj.IS_ACCEPTED == true)
                                {
                                    Session["USER_CODE"] = obj.USER_CODE;
                                    Session["USER_NAME"] = obj.USER_NAME;
                                    Session["ROLE_CODE"] = obj.ROLE_CODE;
                                    TempData["USER_NAME"] = obj.USER_NAME;
                                    Session["Time"] = today.ToShortTimeString();
                                    ///  FormsAuthentication.RedirectFromLoginPage(obj.USER_NAME, false);
                                    int sreg_id = Convert.ToInt32(Convert.ToString(obj.USER_CODE).Substring(1));
                                    var chkbrd_cls = DbContext.tbl_DC_Registration_Dtl.Where(x => x.Regd_ID == sreg_id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                                    if (chkbrd_cls == null)
                                    {
                                        msg = "0"; //choose board
                                    }
                                    else
                                    {
                                        Session["class"] = chkbrd_cls.Class_ID;
                                        msg = "1"; //board already there
                                    }
                                }
                                else {
                                    msg = "-5";
                                }
                            }
                            else
                            {
                                msg = "Please login as a student.";
                            }
                        }
                        else
                        {
                            msg = "-1"; //otp conformation
                        }
                    }
                    else
                    {
                        msg = "-3"; // "Password doesnot match.";
                    }
                }
                else
                {
                    msg = "-4";   // "User name is not yet registered.";
                }
            }
            catch (Exception ex)
            {
                ex.Message.ToString();
                msg = "-2";
            }
            return Json(msg, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult letsbegin(string mobno)
        {
            string msg = string.Empty;
            string msg1 = string.Empty;
            try
            {
                if (mobno != null)
                {
                    var data = DbContext.tbl_DC_Registration.Where(x => x.Mobile == mobno && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    if (data != null)
                    {
                        msg = "1"; //Number registered and redirect to login page
                        ViewBag.number = data.Mobile;
                    }
                    else
                    {
                        msg = "2"; //Number not registered and redirect to sign up page

                        ViewBag.number = mobno;
                    }
                }
            }
            catch (Exception ex)
            {
                msg = "0"; // Something went wrong
            }
            return Json(msg, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult PrivacyPolicy()
        {
            ViewBag.Board_Id = new SelectList(DbContext.tbl_DC_Board.Where(x => x.Is_Active == true && x.Is_Deleted == false), "Board_Id", "Board_Name");
            ViewBag.secureqsn = new SelectList(DbContext.tbl_DC_Security_Question.Where(x => x.Is_Active == true && x.Is_Deleted == false), "Security_Question_ID", "Security_Question");
            
            return View();
        }
        [HttpGet]
        public ActionResult contact()
        {
            ViewBag.Board_Id = new SelectList(DbContext.tbl_DC_Board.Where(x => x.Is_Active == true && x.Is_Deleted == false), "Board_Id", "Board_Name");
            ViewBag.secureqsn = new SelectList(DbContext.tbl_DC_Security_Question.Where(x => x.Is_Active == true && x.Is_Deleted == false), "Security_Question_ID", "Security_Question");
            ViewBag.captcha = Createcaptcha(4);
            return View();
        }

        [HttpPost]
        public JsonResult contact(string fname, string lname, string email, string subject, string message, string captcha, string captcha_code)
        {
            string msg = string.Empty;
            try
            {
                if (captcha == captcha_code)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("<body style='margin: 0; padding: 0; border: none;'>");
                    sb.Append("<div style='width: 650px; float: left; border: 5px solid #CCC;color:rgba(255, 255, 255, 0.84); font-family: Arial, Helvetica, sans-serif; font-size: 13px; line-height: 22px; padding: 0; background-color: #36a8f5;'>");
                    sb.Append("<div style='width: 100%; float: left; background-color: #fff; padding: 10px 0px;'>");
                    sb.Append("<div style='margin-left: 20px; margin-right: 20px; background-color: #fff; padding: 0;text-align: center;'>");
                    sb.Append("<a href='http://thedigichamps.com'><img src='http://thedigichamps.com/Student_assets/images/digi-champ-logo.png' width='310' height='88' alt='DigiChamps' /></a>");

                    sb.Append("<div style='clear: both'>");
                    sb.Append("</div>");
                    sb.Append("</div>");
                    sb.Append("</div>");
                    sb.Append("<!-- background pic change  -->");
                    sb.Append("<div style='width: 100%; background-position: center bottom; padding: 0; border: none; float: left; background-size: cover'>");
                    sb.Append("<div style='margin-left: 20px; margin-right: 20px;'>");
                    sb.Append("<p>");
                    sb.Append("<strong>Dear ADMIN,</strong></p>");
                    sb.Append("<p>");
                    sb.Append(" Greetings from DigiChamps !<br/>");
                    sb.Append("<span>Some one wants to contact Digichamps, Below are the provided details :</span></p>");
                    sb.Append("<p>");
                    if (lname != null)
                    {
                        sb.Append("<strong>Mr/Ms " + fname + "</strong></p>");
                    }
                    else
                    {
                        sb.Append("<strong>Mr/Ms " + fname + " "+ lname + "'</strong></p>");
                    }

                    sb.Append("<p>");
                    sb.Append("<br/> Email : <span style='color: white'> " + email + "</span> <br/>  Subject : <span style='color: white'> " + subject + " </span>.<br />Message : <span style='color: white'> " + message + "</span> </p>");
                    sb.Append("<p>");
                    sb.Append("<p><a style='font-style:oblique; color:#bbd6ff; font-size:17px;' href='http://thedigichamps.com/' title='DigiChamps'> thedigichamps.com </a></p>");
                    sb.Append("<div style='clear: both'>");
                    sb.Append("</div>");
                    sb.Append("</div>");
                    sb.Append("<div style='clear: both'>");
                    sb.Append("</div>");
                    sb.Append("</div>");
                    sb.Append("<div style='clear: both'>");
                    sb.Append("</div>");
                    sb.Append("<div style='width: 100%; background-color: #1f2429; float: left; color: #cecece;'>");
                    sb.Append("<div style='margin-left: 20px; margin-right: 20px;'>");
                    sb.Append("<p style='text-align:center;'>");
                    sb.Append("Regards<br />Support , DigiChamps </p>");
                    sb.Append("</div>");
                    sb.Append("<div style='clear: both'>");
                    sb.Append("</div>");
                    sb.Append("<div style='clear: both'>");
                    sb.Append("</div>");
                    sb.Append("</div>");
                    sb.Append("<div style='clear: both'>");
                    sb.Append("</div>");
                    sb.Append("</div>");
                    sb.Append("</body>");

                    DMLhelper.sendMail1("info@thedigichamps.com", email, "Regarding Contact", sb.ToString());
                    msg = "2";
                    LeadsSquaredAPI obj = new Models.LeadsSquaredAPI();
                    bool chk = obj.submitQueryAPI(email, "", fname, lname);
                    return Json(msg, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    msg = "1";
                }

            }
            catch (Exception)
            {
                msg="3";
                
            }
            return Json(msg, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult faq()
        {
            try
            {


                ViewBag.Board_Id = new SelectList(DbContext.tbl_DC_Board.Where(x => x.Is_Active == true && x.Is_Deleted == false), "Board_Id", "Board_Name");
                ViewBag.secureqsn = new SelectList(DbContext.tbl_DC_Security_Question.Where(x => x.Is_Active == true && x.Is_Deleted == false), "Security_Question_ID", "Security_Question");


                var data = (from a in DbContext.tbl_DC_FAQs.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                            select new DigiChampsModel.Digichampsfaq
                            {
                                FAQs_ID = a.FAQs_ID,
                                FAQ = a.FAQ,
                                FAQ_Answer = a.FAQ_Answer

                            }).ToList();
                ViewBag.faq = data;

            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return View();

        }

        public ActionResult career()
        {
            try
            {
                ViewBag.Board_Id = new SelectList(DbContext.tbl_DC_Board.Where(x => x.Is_Active == true && x.Is_Deleted == false), "Board_Id", "Board_Name");
                ViewBag.secureqsn = new SelectList(DbContext.tbl_DC_Security_Question.Where(x => x.Is_Active == true && x.Is_Deleted == false), "Security_Question_ID", "Security_Question");

                var data = (from a in DbContext.tbl_DC_Career.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                            select new DigiChampsModel.DigichampsCareer
                            {
                                Career_ID = a.Career_ID,
                                career_Name = a.career_Name,
                                No_of_vacancy = a.No_of_vacancy,
                                Experience = a.Experience,
                                Location = a.Location,
                                Qualification = a.Qualification,
                                Opening_Date = a.Opening_Date,
                                Close_Date = a.Close_Date,
                                Walk_in_Time = a.Walk_in_Time,
                                Phone = a.Phone,
                                Job_Description = a.Job_Description

                            }).ToList();
                ViewBag.career = data;

            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }

            return View();
        }
        public static string Createcaptcha(int PasswordLength)
        {
            string _allowedChars = "0123456789abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNOPQRSTUVWXYZ";
            Random randNum = new Random();
            char[] chars = new char[PasswordLength];
            int allowedCharCount = _allowedChars.Length;
            for (int i = 0; i < PasswordLength; i++)
            {
                chars[i] = _allowedChars[(int)((_allowedChars.Length) * randNum.NextDouble())];
            }
            return new string(chars);
        }

        public ActionResult Logout()
        {
            logoutstatus();
            Session.Abandon();
            FormsAuthentication.SignOut();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public JsonResult ForgotPassword(string mobile)
        {
            string msg = string.Empty;
            try
            {
                if (mobile != "")
                {
                    tbl_DC_Registration obj = DbContext.tbl_DC_Registration.Where(x => x.Mobile == mobile).FirstOrDefault();
                    if (obj == null)
                    {
                        msg = "0";  // "Mobile number is not yet registered";
                    }
                    else
                    {
                        int reg_id = Convert.ToInt32(obj.Regd_ID);
                        Session["Reg_Id"] = reg_id;
                        //tbl_DC_Pre_book pre = DbContext.tbl_DC_Pre_book.Where(x => x.Regd_Id == reg_id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        //if (pre == null)
                        //{
                            tbl_DC_OTP obj1 = DbContext.tbl_DC_OTP.Where(x => x.Regd_Id == reg_id).FirstOrDefault();
                            if (obj1 != null)
                            {
                                if (obj1.To_Date < today)
                                {
                                    string otp = random_password(6);
                                    obj1.OTP = otp;
                                    obj1.From_Date = today;
                                    obj1.Count = 1;
                                    obj1.To_Date = Convert.ToDateTime(today.AddHours(1));
                                    DbContext.Entry(obj1).State = EntityState.Modified;
                                    DbContext.SaveChanges();

                                    Sendsms("FRGPASS", otp.Trim(), mobile.Trim());  //send otp

                                    if (obj.Email != null && obj.Email != "")
                                    {
                                        var getall = DbContext.SP_DC_Get_maildetails("ST_FPASS").FirstOrDefault();
                                        if (getall != null)
                                        {
                                            string msgbody = getall.EmailConf_Body.ToString().Replace("{{name}}", obj.Customer_Name).Replace("{{OTP}}", otp);
                                            sendMail1("ST_FPASS", obj.Email, "Digichamps Reset Password", obj.Customer_Name, msgbody);
                                        }
                                    }
                                    msg = "1";  //redirect to otp conform OTP_Confirmation
                                }
                                else if (obj1.Count <= 3)
                                {
                                    string otp = random_password(6);
                                    obj1.OTP = otp;
                                    obj1.From_Date = today;
                                    obj1.Count = obj1.Count + 1;
                                    obj1.To_Date = Convert.ToDateTime(today.AddHours(1));
                                    DbContext.Entry(obj1).State = EntityState.Modified;
                                    DbContext.SaveChanges();

                                    Sendsms("FRGPASS", otp.Trim(), mobile.Trim());  //send otp

                                    if (obj.Email != null && obj.Email != "")
                                    {
                                        var getall = DbContext.SP_DC_Get_maildetails("ST_FPASS").FirstOrDefault();
                                        if (getall != null)
                                        {
                                            string msgbody = getall.EmailConf_Body.ToString().Replace("{{name}}", obj.Customer_Name).Replace("{{OTP}}", otp);
                                            sendMail1("ST_FPASS", obj.Email, "Digichamps Reset Password", obj.Customer_Name, msgbody);
                                        }
                                    }
                                    msg = "1";  //redirect to otp conform OTP_Confirmation
                                }
                                else
                                {
                                    msg = "-2"; //You have already requested for Maximum no of OTP.
                                }

                            }
                            Session["fname"] = obj.Customer_Name;
                            Session["email"] = obj.Email;
                            Session["phone"] = obj.Mobile;
                        //}
                        //else {
                        //    msg = "-3";
                        //}
                    }
                }
                else { msg = "-1"; }
            }
            catch (Exception ex)
            {
                msg = ex.Message.ToString();
            }
            return Json(msg, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult ChangePassword(string Old_Password, string New_Password, string Conf_Password)
        {
            string msg = string.Empty;
            try
            {
                if (Session["USER_CODE"] != null)
                {
                    string u_code = Session["USER_CODE"].ToString();
                    string old_pass_word = DigiChampsModel.Encrypt_Password.HashPassword(Old_Password);
                    var data = DbContext.tbl_DC_USER_SECURITY.Where(x => x.USER_CODE == u_code && x.PASSWORD == old_pass_word && x.STATUS == "A" && x.ROLE_CODE == "C").FirstOrDefault();

                    if (data != null)
                    {
                        if (New_Password == Conf_Password)
                        {
                            string new_pass_word = DigiChampsModel.Encrypt_Password.HashPassword(New_Password).ToString();

                            data.PASSWORD = new_pass_word;
                            DbContext.Entry(data).State = EntityState.Modified;
                            DbContext.SaveChanges();
                            msg = "1";           //"Password has been changed.";
                        }
                        else
                        {
                            msg = "-1";          //"The new password & confirm password is not matching";
                        }
                    }
                    else
                    {
                        msg = "0";             // "current password Is Wrong";
                    }
                }
                else { msg = "-2"; }
            }
            catch (Exception ex)
            {
                msg = ex.Message.ToString();
            }
            return Json(msg, JsonRequestBehavior.AllowGet);
        }

        #region -----------------------------------------------register Student--------------------------------------------------

        #region---------------Autogenerate OTP----------------
        protected string random_password(int length)
        {
            try
            {
                const string valid = "1234567890";
                StringBuilder res = new StringBuilder();
                Random rnd = new Random();
                while (0 < length--)
                {
                    res.Append(valid[rnd.Next(valid.Length)]);
                }
                return res.ToString();
            }
            catch (Exception)
            {
                return null;
            }
        }
        #endregion

        [HttpPost]
        public JsonResult student_signup(string mobile, string name, string email, string ref_from, string school, string classpb)
        {
            string msg = string.Empty;
            try
            {
                if (mobile != "" && name != "")
                {
                    var mob = DbContext.tbl_DC_Registration.Where(x => x.Mobile == mobile && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    if (mob == null)
                    {
                        tbl_DC_Registration dr = new tbl_DC_Registration();
                        dr.Mobile = mobile;
                        dr.Customer_Name = name;
                        dr.Email = email;
                        dr.Is_Active = true;
                        dr.Is_Deleted = false;
                        dr.Inserted_By = HttpContext.User.Identity.Name;
                        dr.Inserted_Date = today;
                        DbContext.tbl_DC_Registration.Add(dr);
                        DbContext.SaveChanges();

                        var regno2 = DbContext.tbl_DC_Registration.Where(x => x.Mobile == mobile && x.Is_Active == true && x.Is_Deleted == false).Take(1).ToList();
                        int reg_num1 = Convert.ToInt32(regno2.ToList()[0].Regd_ID);
                        var pri = DbContext.tbl_DC_Prefix.Where(x => x.PrefixType_ID == 2).Select(x => x.Prefix_Name).FirstOrDefault();
                        string prifix = pri.ToString();
                        
                        //registration no. of student autogenerate
                        if (reg_num1 == 0)
                        {
                            dr.Regd_No = prifix + today.ToString("yyyyMMdd") + "00000" + 1;
                        }
                        else
                        {
                            int regnum = Convert.ToInt32(regno2.ToList()[0].Regd_ID);
                            if (regnum > 0 && regnum <= 9)
                            {
                                dr.Regd_No = prifix + today.ToString("yyyyMMdd") + "00000" + Convert.ToString(regnum);
                            }
                            if (regnum > 9 && regnum <= 99)
                            {
                                dr.Regd_No = prifix + today.ToString("yyyyMMdd") + "0000" + Convert.ToString(regnum);
                            }
                            if (regnum > 99 && regnum <= 999)
                            {
                                dr.Regd_No = prifix + today.ToString("yyyyMMdd") + "000" + Convert.ToString(regnum);
                            }
                            if (regnum > 999 && regnum <= 9999)
                            {
                                dr.Regd_No = prifix + today.ToString("yyyyMMdd") + "00" + Convert.ToString(regnum);
                            }
                            if (regnum > 9999 && regnum <= 99999)
                            {
                                dr.Regd_No = prifix + today.ToString("yyyyMMdd") + "0" + Convert.ToString(regnum);
                            }
                        }
                        DbContext.Entry(dr).State = EntityState.Modified;
                        DbContext.SaveChanges();

                        //-----------Add details to User-Security Table
                        tbl_DC_USER_SECURITY obj1 = new tbl_DC_USER_SECURITY();
                        obj1.USER_NAME = mobile;
                        obj1.ROLE_CODE = "C";
                        obj1.ROLE_TYPE = 3;
                        obj1.USER_CODE = "C0" + Convert.ToString(reg_num1);
                        obj1.IS_ACCEPTED = false;                               //-------------To Change---------------------
                        obj1.STATUS = "D";                                      //--------------To Change------------------
                        string new_pass_word = CreateRandomPassword(8);         //--------------random password------------------
                        obj1.PASSWORD = DigiChampsModel.Encrypt_Password.HashPassword(new_pass_word).ToString();
                        DbContext.tbl_DC_USER_SECURITY.Add(obj1);
                        DbContext.SaveChanges();

                        //------------Add details To OTP TAble
                        tbl_DC_OTP obj2 = new tbl_DC_OTP();
                        var regno1 = DbContext.tbl_DC_Registration.Where(x => x.Mobile == mobile && x.Is_Active == true && x.Is_Deleted == false).Take(1).ToList();
                        obj2.Regd_Id = Convert.ToInt32(regno1.ToList()[0].Regd_ID);
                        obj2.Mobile = mobile;

                        string otp = random_password(6);
                        obj2.OTP = otp;
                        obj2.From_Date = today;
                        obj2.To_Date = Convert.ToDateTime(today.AddHours(1));
                        obj2.Count = 1;
                        DbContext.tbl_DC_OTP.Add(obj2);
                        DbContext.SaveChanges();

                        Sendsms("REG", otp.Trim(), mobile.Trim());

                        Session["Rg_Id"] = obj2.Regd_Id;

                        //session for student PreBook()
                        Session["fname"] = name.Trim();
                        Session["email"] = email;
                        Session["phone"] = mobile;
                        string[] names = name.Split(' ');
                        string fname = string.Empty;
                        string lname = string.Empty;
                        LeadsSquaredAPI obj = new Models.LeadsSquaredAPI();
                        if (names.Count() > 1)
                        {
                            fname = names[0];
                            lname = names[1];

                        }
                        else { fname = names[0]; }
                        msg = "1"; //Thank you for Signing up, Conform OTP to continue.
                        if (email != null && email != "")
                        {
                            var getall = DbContext.SP_DC_Get_maildetails("S_REG").FirstOrDefault();
                            if (getall != null)
                            {
                                string msgbody = getall.EmailConf_Body.ToString().Replace("{{name}}", name).Replace("{{OTP}}", otp);
                                sendMail1("S_REG", email, "Welcome to Digichamps", name, msgbody);
                            }
                        }
                        bool chk = obj.submitQueryAPI(email, mobile, fname, lname);
                    }
                    else
                    {
                        msg = "0"; //Mobile no. already exist.
                    }
                }
                else
                {
                    msg = "-1"; //Please enter name and mobile number.
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message.ToString();
            }
            return Json(msg, JsonRequestBehavior.AllowGet);
        }

        public bool sendMail1(string parameter, string email, string sub, string name, string msgbody)
        {
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

        public bool Sendsms(string type, string opt, string mobile)
        {
            try
            {
                var sms_obj = DbContext.View_DC_SMS_API.Where(x => x.Sms_Alert_Name == type).FirstOrDefault();
                if (sms_obj != null)
                {
                    string message = sms_obj.Sms_Body;
                    var regex = new Regex(Regex.Escape("{{otpno}}"));
                    var newText = regex.Replace(message, opt, 9);
                      string baseurl = "" + sms_obj.Api_Url.ToString().Replace("mobile", mobile).Replace("message", newText);

                    HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(baseurl);
                    //Get response from the SMS Gateway Server and read the answer
                    HttpWebResponse myResp = (HttpWebResponse)myReq.GetResponse();
                    System.IO.StreamReader respStreamReader = new System.IO.StreamReader(myResp.GetResponseStream());
                    string responseString = respStreamReader.ReadToEnd();
                    respStreamReader.Close();
                    myResp.Close();
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        [HttpPost]
        public JsonResult OTP_Confirmation(string Mobile_OTP, string New_Password, string Confirm_Password)
        {
            string msg = string.Empty; ;
            try
            {
                if (Session["Rg_Id"] != null)
                {
                    #region After signup
                    int rid = Convert.ToInt32(Session["Rg_Id"]);

                    var data = DbContext.tbl_DC_OTP.Where(x => x.OTP == Mobile_OTP && x.Regd_Id == rid && x.From_Date < today && x.To_Date > today).OrderByDescending(x => x.OTP_ID).FirstOrDefault();
                    if (data != null)
                    {
                        if (New_Password == Confirm_Password)
                        {
                            string ucode = "C0" + Convert.ToString(rid);
                            tbl_DC_USER_SECURITY obj4 = DbContext.tbl_DC_USER_SECURITY.Where(x => x.USER_CODE == ucode).FirstOrDefault();
                            obj4.PASSWORD = DigiChampsModel.Encrypt_Password.HashPassword(New_Password).ToString();
                            obj4.STATUS = "A";
                            obj4.IS_ACCEPTED = true;
                            DbContext.Entry(obj4).State = EntityState.Modified;
                            //FormsAuthentication.RedirectFromLoginPage(obj4.USER_NAME, false);
                            DbContext.SaveChanges();

                            tbl_DC_Registration obj5 = DbContext.tbl_DC_Registration.Where(x => x.Regd_ID == rid).FirstOrDefault();
                            obj5.Is_Active = true;
                            obj5.Is_Deleted = false;
                            obj5.Modified_Date = today;
                            DbContext.Entry(obj5).State = EntityState.Modified;
                            DbContext.SaveChanges();
                            msg = "1";       //"You have successfully registered with us. Now you can login to your account."; return View("choose_board");
                        }
                        else
                        {
                            msg = "0";     //"New password and confirm password are not matching"; return View("Login");
                        }
                    }
                    else
                    {
                        msg = "-1";         //"OTP is not valid"; return View("Login");
                    }
                    #endregion
                }
                else
                {
                    if (Session["Reg_Id"] != null)
                    {
                        #region Forgot password OTP
                        int reg_id = Convert.ToInt32(Session["Reg_Id"].ToString());
                        var to_fetch_reg = DbContext.tbl_DC_Registration.Where(x => x.Regd_ID == reg_id).FirstOrDefault();
                        string ucode = "C0" + Convert.ToString(reg_id);

                        var data = DbContext.tbl_DC_OTP.Where(x => x.OTP == Mobile_OTP && x.Regd_Id == reg_id).FirstOrDefault();

                        if (data != null)
                        {
                            if (New_Password == Confirm_Password)
                            {
                                tbl_DC_USER_SECURITY obj = DbContext.tbl_DC_USER_SECURITY.Where(x => x.USER_CODE == ucode && x.ROLE_CODE == "C").FirstOrDefault();
                                string status = obj.STATUS;
                                obj.USER_NAME = to_fetch_reg.Mobile;
                                obj.PASSWORD = DigiChampsModel.Encrypt_Password.HashPassword(New_Password).ToString();
                                obj.STATUS = "A";
                                obj.IS_ACCEPTED = true;
                                DbContext.Entry(obj).State = EntityState.Modified;
                                DbContext.SaveChanges();

                                to_fetch_reg.Mobile = to_fetch_reg.Mobile;
                                to_fetch_reg.Is_Active = true;
                                to_fetch_reg.Is_Deleted = false;
                                DbContext.Entry(to_fetch_reg).State = EntityState.Modified;
                                DbContext.SaveChanges();
                                Session["Rg_Id"] = reg_id;
                                if (status == "D")
                                {
                                    msg = "11";   //return RedirectToAction("choose_board");
                                }
                                else if (status == "A")
                                {
                                    msg = "12";   //return RedirectToAction("Login");
                                }
                            }
                            else
                            {
                                msg = "0";
                            }
                        }

                        else
                        {
                            msg = "-1";
                        }
                        #endregion
                    }
                    else
                    {
                        msg = "Invalid OTP details";
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Message.ToString();
            }
            return Json(msg, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult EditProfile()
        {
            int id = 0;
            View_All_Student_Details student = new View_All_Student_Details();
            if (Session["USER_CODE"] != null)
            {
                id = Convert.ToInt32(Convert.ToString(Session["USER_CODE"]).Substring(1));
                if (id != 0)
                {
                    string usercode = Session["USER_CODE"].ToString();
                    student = DbContext.View_All_Student_Details.Where(x => x.Regd_ID == id && x.STATUS == "A").FirstOrDefault();
                    tbl_DC_LoginStatus id_found = DbContext.tbl_DC_LoginStatus.Where(x => x.Login_ID == usercode).FirstOrDefault();
                    if (id_found == null)
                    {
                        loginstatus(Convert.ToString(Session["USER_CODE"].ToString()));
                        ViewBag.newuser = student.Customer_Name;    //modal for new User
                    }
                    var percent = DbContext.SP_DC_Student_Profile_Progress(id).FirstOrDefault();        //completion progress

                    string[] getpercentage = Convert.ToString(percent).Split('.');

                    ViewBag.progress = Convert.ToInt32(getpercentage[0]);
                    ViewBag.email = student.Email;
                    ViewBag.alterno = student.Phone;
                    ViewBag.school = student.Organisation_Name;
                    ViewBag.personal = student.Address;
                }
            }
            else{
                if (Session["Rg_Id"] != null)
                {
                    int rid = Convert.ToInt32(Session["Rg_Id"]);
                    var percent = DbContext.SP_DC_Student_Profile_Progress(rid).FirstOrDefault();       //completion progress
                    ViewBag.progress = Convert.ToInt32(percent.ToString());
                    student = DbContext.View_All_Student_Details.Where(x => x.Regd_ID == id && x.STATUS == "A").FirstOrDefault();
                    Session["USER_CODE"] = "C0" + Convert.ToString(rid);
                    ViewBag.school = student.Organisation_Name;
                    ViewBag.personal = student.Address;
                }
            }
            if (student != null)
            {
                if (student.Board_Id == null)       //if board is not selected
                {
                    ViewBag.board = new SelectList(DbContext.tbl_DC_Board.Where(x => x.Is_Active == true && x.Is_Deleted == false),
                "Board_Id", "Board_Name");

                    ViewBag.secure = new SelectList(DbContext.tbl_DC_Security_Question.Where(x => x.Is_Active == true && x.Is_Deleted == false), "Security_Question_ID", "Security_Question");
                }
                else
                {
                    ViewBag.board = new SelectList(DbContext.tbl_DC_Board.Where(x => x.Is_Active == true && x.Is_Deleted == false),
                    "Board_Id", "Board_Name", student.Board_Id);
                    ViewBag.secure = new SelectList(DbContext.tbl_DC_Security_Question.Where(x => x.Is_Active == true && x.Is_Deleted == false), "Security_Question_ID", "Security_Question", student.Secure_Id);
                }

                if (student.Class_ID != null)       //if class already selected
                {
                    ViewBag.classid = student.Class_ID;
                }
                ViewBag.image = student.Image;
            }
            return View(student);
        }

        [HttpPost]
        public JsonResult EditProfile(View_All_Student_Details student)
        {
            string message = string.Empty;
            try
            {
                if (student.Customer_Name.Trim() != "" && student.Board_Id != null && student.Class_ID != null)
                {
                    if (Session["USER_CODE"] != null)
                    {
                        int sreg_id = Convert.ToInt32(Convert.ToString(Session["USER_CODE"]).Substring(1));
                        tbl_DC_Registration stu_details = DbContext.tbl_DC_Registration.Where(x => x.Regd_ID == sreg_id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        if (stu_details != null)
                        {
                            stu_details.Customer_Name = student.Customer_Name.Trim();
                            stu_details.Email = student.Email;
                            stu_details.Phone = student.Phone;
                            stu_details.Organisation_Name = student.Organisation_Name;
                            stu_details.Address = student.Address;
                            stu_details.Pincode = student.Pincode;
                            stu_details.Praent_Name = student.Praent_Name;
                            stu_details.Parent_Mail = student.Parent_Mail;
                            stu_details.Parent_Mobile = student.Parent_Mobile;
                            stu_details.P_Relation = student.P_Relation;
                            stu_details.Modified_By = HttpContext.User.Identity.Name;
                            stu_details.Modified_Date = today;
                            DbContext.Entry(stu_details).State = EntityState.Modified;
                            DbContext.SaveChanges();

                            tbl_DC_Registration_Dtl alldetail = DbContext.tbl_DC_Registration_Dtl.Where(x => x.Regd_ID == sreg_id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                            if (alldetail != null)
                            {
                                alldetail.Board_ID = student.Board_Id;
                                alldetail.Class_ID = student.Class_ID;
                                stu_details.Modified_By = HttpContext.User.Identity.Name;
                                stu_details.Modified_Date = today;
                                DbContext.Entry(stu_details).State = EntityState.Modified;
                                DbContext.SaveChanges();
                            }
                            var percent = DbContext.SP_DC_Student_Profile_Progress(sreg_id).FirstOrDefault();        //completion progress

                            string[] getpercentage = Convert.ToString(percent).Split('.');

                            //int Percentage = Convert.ToInt32(getpercentage[0]);
                            DigiChamps.Models.DigiChampsModel.percentagecls abc = new DigiChamps.Models.DigiChampsModel.percentagecls();
                            abc.message = "1";
                            abc.Percentage = Convert.ToInt32(getpercentage[0]);


                            return new JsonResult { Data = abc, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                        }
                        else
                        {
                            message = "-1";
                        }
                    }
                    else
                    {
                        message = "-2";
                    }
                }
                else
                {
                    message = "0";
                }
            }
            catch (Exception ex)
            {
                message = ex.Message.ToString();
            }
            return Json(message, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult editsecurityqstn(View_All_Student_Details student)
        {
            string message = string.Empty;
            try
            {
                if (student.Secure_Id != null && student.Answer != null)
                {
                    if (Session["USER_CODE"] != null)
                    {
                        int sreg_id = Convert.ToInt32(Convert.ToString(Session["USER_CODE"]).Substring(1));
                        tbl_DC_Registration stu_details = DbContext.tbl_DC_Registration.Where(x => x.Regd_ID == sreg_id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        if (stu_details != null)
                        {

                            tbl_DC_Registration_Dtl alldetail = DbContext.tbl_DC_Registration_Dtl.Where(x => x.Regd_ID == sreg_id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                            if (alldetail != null)
                            {
                                alldetail.Secure_Id = student.Secure_Id;
                                alldetail.Answer = student.Answer;
                                stu_details.Modified_Date = today;
                                DbContext.Entry(alldetail).State = EntityState.Modified;
                                DbContext.SaveChanges();
                                message = "1";
                            }
                            else
                            {
                                message = "0";
                            }
                        }
                        else
                        {
                            message = "0";
                        }
                    }
                }
                else
                {
                    message = "-1";
                }
            }
            catch (Exception ex)
            {
                message = ex.ToString();
            }
            return Json(message, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Edit_ProfileImg(string img)
        {
            string[] message=new string[2] ;
            try
            {
                if (Session["USER_CODE"] != null)
                {
                    if (img != null)
                    {
                        int id = Convert.ToInt32(Convert.ToString(Session["USER_CODE"]).Substring(1));
                        if (id != 0)
                        {
                            tbl_DC_Registration obj = DbContext.tbl_DC_Registration.Where(x => x.Regd_ID == id).FirstOrDefault();
                            string[] image1 = img.Split(',');
                            string name = Convert.ToString(Createcaptcha(4) + id);
                            Base64ToImage_Stu(name, image1[1]);
                            img = name + ".jpg";
                            obj.Image = img.ToString();
                            obj.Modified_By = HttpContext.User.Identity.Name;
                            obj.Modified_Date = today;
                            DbContext.Entry(obj).State = EntityState.Modified;
                            DbContext.SaveChanges();
                            message[0] = "1";
                            message[1] = img;
                        }
                    }
                    else
                    {
                        message[0] = "0";
                    }
                }
                else
                {
                    message[0] = "-1";
                }
            }
            catch (Exception ex){
                message[0] = ex.ToString();
            }
            return Json(message, JsonRequestBehavior.AllowGet);
        }

        public Image Base64ToImage_Stu(string fname, string base64String)
        {
            // Convert base 64 string to byte[]
            byte[] imageBytes = Convert.FromBase64String(base64String);
            // Convert byte[] to Image
            using (var ms = new MemoryStream(imageBytes, 0, imageBytes.Length))
            {
                string filename = fname + ".jpg";
                Image image = Image.FromStream(ms, true);
                var img = new Bitmap(Image.FromStream(ms));
                string tempFolderName = Server.MapPath("~/Images/Profile/" + filename);
                image.Save(tempFolderName, ImageFormat.Jpeg);
                return image;
            }
        }
        #endregion

        #region--------------------------------------------------Resend otp-----------------------------------------------------
        public JsonResult Resend_OTP(int? count)
        {
            string msg = string.Empty;
            try
            {
                    if (Session["Reg_Id"] != null)
                    {
                        int rgt_id = Convert.ToInt32(Session["Reg_Id"]);
                        string usercode = "C0" + Session["Reg_Id"].ToString();
                        var chk_type = DbContext.tbl_DC_USER_SECURITY.Where(x => x.USER_CODE == usercode && x.ROLE_CODE == "C").FirstOrDefault();

                        if (chk_type != null)
                        {
                            var widget = DbContext.tbl_DC_OTP.Where(x => x.Regd_Id == rgt_id).FirstOrDefault();
                            if (widget != null)
                            {
                                if (widget.Count <= 3 && widget.To_Date > today)
                                {
                                    string otp = random_password(6);
                                    widget.OTP = otp;
                                    widget.Count = widget.Count + 1;
                                    DbContext.Entry(widget).State = EntityState.Modified;
                                    DbContext.SaveChanges();

                                    Sendsms("FRGPASS", otp.Trim(), widget.Mobile.Trim());

                                    msg = "1";
                                }
                                else
                                {
                                    msg = "0";  //OTP cant send more than 3 times.
                                }
                            }
                        }
                    }
                    else if (Session["Rg_Id"] != null)
                    {
                        int rgt_id = Convert.ToInt32(Session["Rg_Id"]);
                        string usercode = "C0" + Session["Rg_Id"].ToString();
                        var chk_type = DbContext.tbl_DC_USER_SECURITY.Where(x => x.USER_CODE == usercode && x.ROLE_CODE == "C").FirstOrDefault();

                        if (chk_type != null)
                        {
                            var widget = DbContext.tbl_DC_OTP.Where(x => x.Regd_Id == rgt_id).FirstOrDefault();
                            if (widget != null)
                            {
                                if (widget.Count <= 3 && widget.To_Date > today)
                                {
                                    string otp = random_password(6);
                                    widget.OTP = otp;
                                    widget.From_Date = today;
                                    widget.To_Date = Convert.ToDateTime(today.AddHours(1));
                                    DbContext.Entry(widget).State = EntityState.Modified;
                                    DbContext.SaveChanges();

                                    Sendsms("REG", otp.Trim(), widget.Mobile.Trim());
                                    msg = "1";
                                }
                                else
                                {
                                    msg = "0";  //OTP cant send more than 3 times.
                                }
                            }
                        }
                    }
                    else
                    {
                        msg = "-1";
                    }
            }
            catch (Exception ex)
            {
                msg = "-1";
            }
            return Json(msg, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region ----------------------------------------------Choose board/class-------------------------------------------------
        [HttpPost]
        public ActionResult startboarding(string school_name, int? board, int? classid, int? secureqstn, string answer)
        {
            string message = string.Empty;
            try
            {
                if (board != 0 && classid != 0 && school_name != null && answer != null)
                {
                    ViewBag.Board_Id = new SelectList(DbContext.tbl_DC_Board.Where(x => x.Is_Active == true && x.Is_Deleted == false), "Board_Id", "Board_Name", board);

                    #region ..Direct Redirect from OTP Conformation
                    if (Session["Rg_Id"] != null)
                    {
                        int? regid = Convert.ToInt32(Session["Rg_Id"].ToString());
                        tbl_DC_Registration obj3 = DbContext.tbl_DC_Registration.Where(x => x.Regd_ID == regid).Where(x => x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        tbl_DC_Registration_Dtl obj1 = DbContext.tbl_DC_Registration_Dtl.Where(x => x.Regd_ID == regid).Where(x => x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        if (obj1 != null)
                        {
                            obj1.Board_ID = board;
                            obj1.Class_ID = classid;
                            obj1.Year = today.Year.ToString();
                            obj1.Secure_Id = secureqstn;
                            obj1.Answer = answer.Trim();
                            obj1.Modified_By = HttpContext.User.Identity.Name;
                            obj1.Modified_Date = today;
                            DbContext.Entry(obj1).State = EntityState.Modified;
                            DbContext.SaveChanges();

                            obj3.Organisation_Name = school_name.Trim();
                            DbContext.Entry(obj3).State = EntityState.Modified;
                            DbContext.SaveChanges();
                            message = "1";
                            Session["ROLE_CODE"] = "C";
                            Session["USER_CODE"] = "C0" + Convert.ToString(regid);
                            Session["class"] = obj1.Class_ID;
                        }
                        else
                        {
                            tbl_DC_Registration_Dtl obj2 = new tbl_DC_Registration_Dtl();
                            if (obj3 != null)
                            {
                                obj2.Regd_ID = regid;
                                obj2.Year = today.Year.ToString();
                                obj2.Board_ID = board;
                                obj2.Class_ID = classid;
                                obj2.Secure_Id = secureqstn;
                                obj2.Answer = answer.Trim();
                                obj2.Regd_No = obj3.Regd_No;
                                obj2.Inserted_Date = today;
                                obj2.Inserted_By = HttpContext.User.Identity.Name;
                                obj2.Is_Deleted = false;
                                obj2.Is_Active = true;
                                DbContext.tbl_DC_Registration_Dtl.Add(obj2);
                                DbContext.SaveChanges();

                                obj3.Organisation_Name = school_name.Trim();
                                DbContext.Entry(obj3).State = EntityState.Modified;
                                DbContext.SaveChanges();
                                message = "1";
                                Session["ROLE_CODE"] = "C";
                                Session["USER_CODE"] = "C0" + Convert.ToString(regid);
                                Session["class"] = obj2.Class_ID;
                            }
                            else
                            {
                                message = "-1";
                            }
                        }
                        Session["USER_NAME"] = obj3.Mobile.ToString();

                    }
                    #endregion

                    #region ..choose board after Login success
                    else if (Session["USER_CODE"] != null)
                    {
                        int sreg_id = Convert.ToInt32(Convert.ToString(Session["USER_CODE"]).Substring(1));
                        tbl_DC_Registration obj3 = DbContext.tbl_DC_Registration.Where(x => x.Regd_ID == sreg_id).Where(x => x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        tbl_DC_Registration_Dtl obj1 = DbContext.tbl_DC_Registration_Dtl.Where(x => x.Regd_ID == sreg_id).Where(x => x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        if (obj1 != null)
                        {
                            obj1.Board_ID = board;
                            obj1.Class_ID = classid;
                            obj1.Year = today.Year.ToString();
                            obj1.Secure_Id = secureqstn;
                            obj1.Answer = answer.Trim();
                            obj1.Modified_By = HttpContext.User.Identity.Name;
                            obj1.Modified_Date = today;
                            DbContext.Entry(obj1).State = EntityState.Modified;
                            DbContext.SaveChanges();

                            obj3.Organisation_Name = school_name.Trim();
                            DbContext.Entry(obj3).State = EntityState.Modified;
                            DbContext.SaveChanges();

                            message = "1";
                            Session["ROLE_CODE"] = "C";
                            Session["USER_CODE"] = "C0" + Convert.ToString(sreg_id);
                            Session["class"] = obj1.Class_ID;
                        }
                        else
                        {
                            tbl_DC_Registration_Dtl obj2 = new tbl_DC_Registration_Dtl();
                            if (obj3 != null)
                            {
                                obj2.Regd_ID = sreg_id;
                                obj2.Year = today.Year.ToString();
                                obj2.Board_ID = board;
                                obj2.Class_ID = classid;
                                obj2.Secure_Id = secureqstn;
                                obj2.Answer = answer.Trim();
                                obj2.Regd_No = obj3.Regd_No;
                                obj2.Inserted_Date = today;
                                obj2.Inserted_By = HttpContext.User.Identity.Name;
                                obj2.Is_Deleted = false;
                                obj2.Is_Active = true;
                                DbContext.tbl_DC_Registration_Dtl.Add(obj2);
                                DbContext.SaveChanges();

                                obj3.Organisation_Name = school_name.Trim();
                                DbContext.Entry(obj3).State = EntityState.Modified;
                                DbContext.SaveChanges();
                                message = "1";
                                Session["ROLE_CODE"] = "C";
                                Session["USER_CODE"] = "C0" + Convert.ToString(sreg_id);
                                Session["class"] = obj2.Class_ID;
                            }
                            else
                            {
                                message = "-1";
                            }
                        }
                        Session["USER_NAME"] = obj3.Mobile.ToString();
                        
                    }
                    Session["Time"] = today.ToShortTimeString();
                    #endregion
                }
                else
                {
                    message = "0";
                }
            }
            catch (Exception ex)
            {
                message = ex.Message.ToString();
            }
            return Json(message, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region -----------------------------------------------Get All from ID---------------------------------------------------
        [HttpPost]
        public ActionResult GetAllClass(int brdId)
        {
            List<SelectListItem> ClsNames = new List<SelectListItem>();
            List<tbl_DC_Class> states = DbContext.tbl_DC_Class.Where(x => x.Board_Id == brdId && x.Is_Active == true && x.Is_Deleted == false).ToList();
            states.ForEach(x =>
            {
                ClsNames.Add(new SelectListItem { Text = x.Class_Name, Value = x.Class_Id.ToString() });
            });
            //ViewBag.state = new SelectList(dbContext.tbl_JV_State.Where(x => x.FK_Country_ID == conId && x.Is_Active == true && x.Is_Deleted == false), "PK_State_ID", "State_Name");
            return Json(ClsNames, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult GetAllSubject(int ClsId)
        {
            List<SelectListItem> SubNames = new List<SelectListItem>();
            List<tbl_DC_Subject> states = DbContext.tbl_DC_Subject.Where(x => x.Class_Id == ClsId && x.Is_Active == true && x.Is_Deleted == false).ToList();
            states.ForEach(x =>
            {
                SubNames.Add(new SelectListItem { Text = x.Subject, Value = x.Subject_Id.ToString() });
            });
            //ViewBag.state = new SelectList(dbContext.tbl_JV_State.Where(x => x.FK_Country_ID == conId && x.Is_Active == true && x.Is_Deleted == false), "PK_State_ID", "State_Name");

            return Json(SubNames, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult GetAllChapter(int SubId)
        {
            List<SelectListItem> ChaptNames = new List<SelectListItem>();
            List<tbl_DC_Chapter> states = DbContext.tbl_DC_Chapter.Where(x => x.Subject_Id == SubId && x.Is_Active == true && x.Is_Deleted == false).ToList();
            states.ForEach(x =>
            {
                ChaptNames.Add(new SelectListItem { Text = x.Chapter, Value = x.Chapter_Id.ToString() });
            });
            //ViewBag.state = new SelectList(dbContext.tbl_JV_State.Where(x => x.FK_Country_ID == conId && x.Is_Active == true && x.Is_Deleted == false), "PK_State_ID", "State_Name");

            return Json(ChaptNames, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult GetAllClassdoubt(int brdId)
        {
           int studentid= Convert.ToInt32(Convert.ToString(Session["USER_CODE"]).Substring(1));
            List<SelectListItem> ClsNames = new List<SelectListItem>();
            List<SP_DC_Askdoubt_Class_Result> states = DbContext.SP_DC_Askdoubt_Class(studentid, brdId).ToList();
            states.ForEach(x =>
            {
                ClsNames.Add(new SelectListItem { Text = x.Class_Name, Value = x.Class_ID.ToString() });
            });
            //ViewBag.state = new SelectList(dbContext.tbl_JV_State.Where(x => x.FK_Country_ID == conId && x.Is_Active == true && x.Is_Deleted == false), "PK_State_ID", "State_Name");

            return Json(ClsNames, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult GetAllSubjectdoubt(int ClsId, int brdId)
        {
            int studentid = Convert.ToInt32(Convert.ToString(Session["USER_CODE"]).Substring(1));
            List<SelectListItem> SubNames = new List<SelectListItem>();
            List<SP_DC_Askdoubt_Subject_Result> states = DbContext.SP_DC_Askdoubt_Subject(studentid, brdId, ClsId).ToList();
            states.ForEach(x =>
            {
                SubNames.Add(new SelectListItem { Text = x.Subject_Name, Value = x.Subject_ID.ToString() });
            });
            //ViewBag.state = new SelectList(dbContext.tbl_JV_State.Where(x => x.FK_Country_ID == conId && x.Is_Active == true && x.Is_Deleted == false), "PK_State_ID", "State_Name");

            return Json(SubNames, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult GetAllChapterdoubt(int SubId, int ClsId, int brdId)
        {
            int studentid = Convert.ToInt32(Convert.ToString(Session["USER_CODE"]).Substring(1));
            List<SelectListItem> ChaptNames = new List<SelectListItem>();
            List<SP_DC_Askdoubt_Chapter_Result> states = DbContext.SP_DC_Askdoubt_Chapter(studentid,brdId,ClsId,SubId).ToList();
            states.ForEach(x =>
            {
                ChaptNames.Add(new SelectListItem { Text = x.Chapter_Name, Value = x.Chapter_ID.ToString() });
            });
            //ViewBag.state = new SelectList(dbContext.tbl_JV_State.Where(x => x.FK_Country_ID == conId && x.Is_Active == true && x.Is_Deleted == false), "PK_State_ID", "State_Name");

            return Json(ChaptNames, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region ----------------------------------------------Student Dashboard--------------------------------------------------
        [HttpGet]
        public ActionResult StudentDashboard()
        {
            try
            {
                if (Session["USER_NAME"] != null)
                {
                    string u_name = Session["USER_NAME"].ToString();
                    string usercode = Session["USER_CODE"].ToString();
                    int _student_id = Convert.ToInt32(Convert.ToString(Session["USER_CODE"]).Substring(1));

                    var data1 = (from k in DbContext.tbl_DC_Registration.Where(x => x.Mobile == u_name && x.Is_Active == true && x.Is_Deleted == false)
                                 join l in DbContext.tbl_DC_Registration_Dtl.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                     on k.Regd_ID equals l.Regd_ID
                                 join m in DbContext.tbl_DC_Class.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                 on l.Class_ID equals m.Class_Id
                                 select new PackagePreviewModel
                                 {
                                     Board_Id = l.Board_ID,
                                     Class_Id = l.Class_ID,
                                     Class_Name = m.Class_Name
                                 }).FirstOrDefault();

                    //----------------Schedule Test ------------------------
                    var ord_customer = DbContext.tbl_DC_Order.Where(x => x.Regd_ID == _student_id && x.Is_Paid == true && x.Is_Active == true && x.Is_Deleted == false).ToList();
                    ViewBag.ord_customer = ord_customer.Count;
                    DateTime dt = today.Date.AddDays(-6);
                    var time = (from a in DbContext.tbl_DC_Exam.Where(x => x.Exam_type == 4 && x.Is_Active == true && x.Is_Deleted == false && x.Shedule_date >= dt && x.Shedule_date <= today.Date) select a).ToList();
                    if (time.Count > 0)
                    {
                        ViewBag.time = time.ToList();
                    }
                    else
                    {
                        ViewBag.time = null;
                    }

                    //----------------Ticket pie chart----------------------
                    var data = DbContext.tbl_DC_Ticket.Where(x => x.Student_ID == _student_id && x.Is_Active == true && x.Is_Deleted == false).ToList();
                    if (data.Count > 0)
                    {
                        ViewBag.Raised = data.Count;
                        decimal total_ticket = Convert.ToDecimal(data.Count);
                        int perecent = Convert.ToInt32(100 / total_ticket);
                        ViewBag.open = Convert.ToInt32(DbContext.View_DC_Tickets_and_Teacher.Where(x => x.Student_ID == _student_id && x.Status != "R" && x.Status != "C").ToList().Count()) * perecent;
                        ViewBag.Reject = Convert.ToInt32(DbContext.tbl_DC_Ticket.Where(x => x.Student_ID == _student_id && x.Status == "R" && x.Is_Active == true && x.Is_Deleted == false).Count()) * perecent;
                        ViewBag.Closed = Convert.ToInt32(DbContext.tbl_DC_Ticket.Where(x => x.Student_ID == _student_id && x.Status == "C" && x.Is_Active == true && x.Is_Deleted == false).Count()) * perecent;
                    }
                    else
                    {
                        ViewBag.Raised = 0;
                        ViewBag.open = 0;
                        ViewBag.Reject = 0;
                        ViewBag.Closed = 0;
                    }
                    //----------------
                    //------------------------test apper and  package--------------------
                    var test_apr = DbContext.tbl_DC_Exam_Result.Where(x => x.Regd_ID == _student_id).OrderByDescending(x => x.EndTime).Take(5).ToList();
                    var pacakage_detail = DbContext.SP_DC_Order_Details(_student_id).OrderByDescending(x => x.Expiry_Date).Take(3).ToList();
                    var pacakage_details = DbContext.SP_DC_Order_Details(_student_id).OrderByDescending(x => x.Expiry_Date).ToList();
                    if(test_apr.Count>0)
                    {
                        ViewBag.test = test_apr;

                    }
                    else
                    {
                        ViewBag.test = null;
                    }
                    var result = 0;
                    if(pacakage_detail.Count>0)
                    {
                        ViewBag.packagedetail = pacakage_details;
                        ViewBag.pkgdtl = pacakage_detail.Count;
                        foreach (var s in pacakage_details)
                        {
                            int NumberOfWords = s.Chapters.Split(',').Length; 
                             result += NumberOfWords;
                            ViewBag.totalchapters = result;

                        }
                        //var result = pacakage_detail.Max(x => x.Chapters.Split(',').Length);
                    }
                    else
                    {
                        ViewBag.packagedetail = null;
                    }
                    //---------------------------------

                  //  ViewBag.participants = ; //Total participants
                    ViewBag.exams = DbContext.tbl_DC_Exam_Result.Where(x => x.Is_Active == true && x.Is_Deleted == false && x.Regd_ID == _student_id).ToList().Count(); // Total exams given

                    ViewBag.pkgmnth = DbContext.tbl_DC_Order.Where(x => x.Is_Active == true && x.Is_Deleted == false && x.Regd_ID == _student_id && x.Is_Paid==true).ToList().Count(); //No of order 
                        
                    if (data1 != null)
                    {
                        ViewBag.cls = data1.Class_Name;
                    }
                    ViewBag.cls1 = DbContext.tbl_DC_Class.Where(x => x.Board_Id == data1.Board_Id && x.Is_Active == true && x.Is_Deleted == false).ToList();

                    return View();
                }
            }
            catch (Exception ex)
            {

            }
            return View();
        }
        [HttpGet]
        public ActionResult OTP_ChkIp()
        {
            string msg = "";
            try
            {
                string u_name = Session["USER_NAME"].ToString();
                //------------Add details To OTP Table
                tbl_DC_OTP obj2 = DbContext.tbl_DC_OTP.Where(x => x.Mobile == u_name).FirstOrDefault();
                tbl_DC_Registration name = DbContext.tbl_DC_Registration.Where(x => x.Mobile == u_name).FirstOrDefault();
                if (obj2 != null)
                {
                    if (obj2.To_Date < today)
                    {
                        string otp = random_password(6);
                        obj2.OTP = otp;
                        obj2.From_Date = today;
                        obj2.Count = 1;
                        obj2.To_Date = Convert.ToDateTime(today.AddHours(1));
                        DbContext.Entry(obj2).State = EntityState.Modified;
                        DbContext.SaveChanges();

                        var sms_obj = DbContext.View_DC_SMS_API.Where(x => x.Sms_Alert_Name == "VERIFY").FirstOrDefault();
                        if (sms_obj != null)
                        {
                            string message = sms_obj.Sms_Body;
                            var regex = new Regex(Regex.Escape("{{otpno}}"));
                            var newText = regex.Replace(message, otp, 9);

                            var regex2 = new Regex(Regex.Escape("{{name}}"));
                            newText = regex2.Replace(newText, name.Customer_Name, 8);
                            string baseurl = "" + sms_obj.Api_Url + "?uname=" + sms_obj.Username + "&pass=" + sms_obj.Password + "&send=" + sms_obj.Sender_Type + "&dest=" + u_name.Trim() + "&msg=" + newText.ToString() + "&priority=1";

                            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(baseurl);
                            //Get response from the SMS Gateway Server and read the answer
                            HttpWebResponse myResp = (HttpWebResponse)myReq.GetResponse();
                            System.IO.StreamReader respStreamReader = new System.IO.StreamReader(myResp.GetResponseStream());
                            string responseString = respStreamReader.ReadToEnd();
                            respStreamReader.Close();
                            myResp.Close();
                        }
                        msg = "1";
                    }
                    else if (obj2.Count <= 3 && obj2.To_Date > today)
                    {
                        string otp = random_password(6);
                        obj2.OTP = otp;
                        obj2.From_Date = today;
                        obj2.To_Date = Convert.ToDateTime(today.AddHours(1));
                        obj2.Count = obj2.Count + 1;
                        DbContext.Entry(obj2).State = EntityState.Modified;
                        DbContext.SaveChanges();

                        var sms_obj = DbContext.View_DC_SMS_API.Where(x => x.Sms_Alert_Name == "VERIFY").FirstOrDefault();
                        if (sms_obj != null)
                        {
                            string message = sms_obj.Sms_Body;
                            var regex = new Regex(Regex.Escape("{{otpno}}"));
                            var newText = regex.Replace(message, otp, 9);

                            var regex2 = new Regex(Regex.Escape("{{name}}"));
                            newText = regex2.Replace(newText, name.Customer_Name, 8);
                            string baseurl = "" + sms_obj.Api_Url + "?uname=" + sms_obj.Username + "&pass=" + sms_obj.Password + "&send=" + sms_obj.Sender_Type + "&dest=" + u_name.Trim() + "&msg=" + newText.ToString() + "&priority=1";

                            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(baseurl);
                            //Get response from the SMS Gateway Server and read the answer
                            HttpWebResponse myResp = (HttpWebResponse)myReq.GetResponse();
                            System.IO.StreamReader respStreamReader = new System.IO.StreamReader(myResp.GetResponseStream());
                            string responseString = respStreamReader.ReadToEnd();
                            respStreamReader.Close();
                            myResp.Close();
                        }

                        msg = "1";
                    }
                    else
                    {
                        msg = "0";  //OTP cant send more than 3 times.
                    }
                }
            }
            catch
            {
                msg = "0";
            }
            return Json(msg, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult OTP_ChkIp(string otpchk)
        {
            string msg = "";
            try
            {
                if (otpchk != null)
                {
                    string u_name = Session["USER_NAME"].ToString();

                    var data = DbContext.tbl_DC_OTP.Where(x => x.OTP == otpchk && x.Mobile == u_name && x.From_Date < today && x.To_Date > today).OrderByDescending(x => x.OTP_ID).FirstOrDefault();
                    if (data != null)
                    {
                        loginstatus(Convert.ToString(Session["USER_CODE"].ToString()));
                        ViewBag.usercode1 = null;
                        msg = "1";
                    }
                    else
                    {
                        msg = "-1";         //"OTP is not valid"; return View("Login");
                    }
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message.ToString();
            }

            return Json(msg, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult securecheck()
        {
            string msg = "";
            try
            {
                if (Session["USER_CODE"] != null)
                {
                    int _student_id = Convert.ToInt32(Convert.ToString(Session["USER_CODE"]).Substring(1));

                    var data = DbContext.tbl_DC_Registration_Dtl.Where(x => x.Regd_ID == _student_id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    if (data != null)
                    {
                        var qus = DbContext.tbl_DC_Security_Question.Where(x => x.Security_Question_ID == data.Secure_Id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        if (qus != null)
                        {
                            msg = qus.Security_Question;
                        }
                    }
                    else
                    {
                        msg = "0";         //"OTP is not valid"; return View("Login");
                    }
                }
            }
            catch (Exception ex)
            {
                msg = "0";
            }
            return Json(msg, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult securecheck(string answer)
        {
            string msg = "";
            try
            {
                if (answer != null)
                {
                    if (Session["USER_CODE"] != null)
                    {
                        int _student_id = Convert.ToInt32(Convert.ToString(Session["USER_CODE"]).Substring(1));

                        var data = DbContext.tbl_DC_Registration_Dtl.Where(x => x.Regd_ID == _student_id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        if (data != null)
                        {
                            if (data.Answer.ToUpper().Trim() == answer.ToUpper().Trim())
                            {
                                loginstatus(Convert.ToString(Session["USER_CODE"].ToString()));
                                ViewBag.usercode1 = null;
                                msg = "1";
                            }
                            else
                            {
                                msg = "0";
                            }
                        }
                        else
                        {
                            msg = "-1";         //"OTP is not valid"; return View("Login");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message.ToString();
            }

            return Json(msg, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult StudentDashboard(int? classid)
        {
            try
            {
                ViewBag.packages = DbContext.SP_DC_Student_PacakgeModule_ID(classid).ToList();
                ViewBag.class_name = DbContext.tbl_DC_Class.Where(x => x.Is_Active == true && x.Is_Deleted == false).ToList();
            }
            catch (Exception ex)
            {
                ex.Message.ToString();
            }

            return View("StudentDashboard");
        }

        [HttpGet]
        public ActionResult Getdetail(int? id)
        {
            ViewBag.pkg = DbContext.tbl_DC_Package.Where(x => x.Package_ID == id && x.Is_Active == true && x.Is_Deleted == false).ToList();
            //ViewBag.Detail_Modules = DbContext.View_DC_Package.Where(x => x.Package_ID == id).ToList();
            return View();
        }
        #endregion

        #region ------------------------------------------------student dobut----------------------------------------------------
        [HttpGet]
        public ActionResult Askdoubt()
        {
            if (Session["USER_CODE"] != null)
            {
                try
                {
                    int _student_id = Convert.ToInt32(Convert.ToString(Session["USER_CODE"]).Substring(1));

                    ViewBag.Boards = DbContext.SP_DC_Askdoubt_Board(_student_id).ToList();
                    var b = DbContext.View_DC_Tickets_and_Teacher.Where(x => x.Student_ID == _student_id).OrderByDescending(x => x.Ticket_ID).ToList();
                    ViewBag.studentallquestion = b.Take(5).ToList();
                    ViewBag.studentallquestion_recent = DbContext.View_DC_Tickets_and_Teacher.Where(x => x.Student_ID == _student_id).Where(x => x.Ticket_Dtls_ID != null).OrderByDescending(x => x.Ticket_ID).ToList();
                    var count = DbContext.SP_DC_COunt_Student_Tickets(_student_id).FirstOrDefault();
                    ViewBag.Student_ticket_Count_ask = count.Doubt_Asked;
                    ViewBag.Student_ticket_Count_ans = count.Answered;
                    ViewBag.Student_ticket_Count_uans = count.Not_Answerd;
                    var rej_doubt = DbContext.tbl_DC_Ticket.Where(x => x.Student_ID == _student_id && x.Status == "O" && x.Is_Active == true && x.Is_Deleted == false).ToList();
                    ViewBag.Rejectec_Doubt = rej_doubt.Count;

                }
                catch (Exception ex)
                {
                    return RedirectToAction("logout", "student");
                }
            }
            else
            {
                return RedirectToAction("logout", "student");
            }
            return View();
        }
        [HttpPost]
        public JsonResult Askdoubt(int? Subject_ID, int? Chapter_ID, string Question_Detail, int? _board_id, int? _class_id, string MyImages)
        {
            string message = string.Empty;
            try
            {
                //var pic = System.Web.HttpContext.Current.Request.Files["MyImages"];
                if (Session["USER_CODE"] != null)
                {
                    if (Subject_ID != null && Chapter_ID != null && Question_Detail != "")
                    {
                        try
                        {
                            int _student_id = Convert.ToInt32(Convert.ToString(Session["USER_CODE"]).Substring(1));
                            var _student_details = DbContext.tbl_DC_Registration.Where(x => x.Regd_ID == _student_id).FirstOrDefault();
                            Boolean isAuthorized = DbContext.tbl_DC_Order.Any(x => x.Regd_ID == _student_id);
                            if (isAuthorized)
                            {
                                var ticket_autoid = DbContext.SP_DC_Generate_Ticket_ID().FirstOrDefault() + _student_id;
                                tbl_DC_Ticket _ticket = new tbl_DC_Ticket();
                                _ticket.Ticket_No = Convert.ToString(ticket_autoid);
                                _ticket.Student_ID = _student_id;
                                _ticket.Board_ID = _board_id;
                                _ticket.Class_ID = _class_id;
                                _ticket.Subject_ID = Subject_ID;
                                _ticket.Chapter_ID = Chapter_ID;
                                _ticket.Question = Question_Detail;
                                if (MyImages != null && MyImages != "")
                                {
                                    string[] image1 = MyImages.Split(',');
                                    string output = _ticket.Ticket_No.Replace("#", "_");
                                    string tktname = Convert.ToString(output + CreateRandomPassword(6));
                                    Base64ToImage_tkt(tktname, image1[1]);
                                    MyImages = tktname + ".jpg";
                                    _ticket.Question_Image = MyImages;
                                }
                                _ticket.Inserted_Date = today;
                                _ticket.Inserted_By = _student_id;
                                _ticket.Status = "O";
                                _ticket.Is_Active = true;
                                _ticket.Is_Deleted = false;
                                DbContext.tbl_DC_Ticket.Add(_ticket);
                                DbContext.SaveChanges();
                                message = Convert.ToString(_ticket.Ticket_ID);
                                sendMail_ticketgenerate("Ticket_Generate", _student_details.Email, _student_details.Customer_Name, ticket_autoid.ToString());

                                try
                                {
                                    var pushnot = (from c in DbContext.tbl_DC_Registration.Where(x => x.Regd_ID == _student_details.Regd_ID)

                                                   select new { c.Regd_ID, c.Device_id }).FirstOrDefault();
                                    string body = "tktid#{{tktid}}# DOUBT {{ticketno}} has been generated Dear {{name}}, ! Your doubt no- {{ticketno}} has been created. It will get resolved by your DigiGuru within 1working day.";
                                    string msg = body.ToString().Replace("{{name}}", _student_details.Customer_Name).Replace("{{ticketno}}", ticket_autoid).Replace("{{tktid}}", _ticket.Ticket_ID.ToString());
                                    if (pushnot != null)
                                    {
                                        if (pushnot.Device_id != null)
                                        {
                                            var note = new PushNotiStatus(msg, pushnot.Device_id);
                                        }
                                    }
                                }
                                catch (Exception)
                                {


                                }


                            }
                        }
                        catch (Exception ex)
                        {
                            message = "2";
                        }
                    }
                    else
                    {
                        message = "3";
                    }
                }
                else
                {
                    message = "4";
                }
            }
            catch
            {
                message = "4";
            }
            return Json(Convert.ToString(message), JsonRequestBehavior.AllowGet);
        }
        public bool sendMail_ticketgenerate(string parameter, string email, string name, string ticket_no)
        {
            var getall = DbContext.SP_DC_Get_maildetails(parameter).FirstOrDefault();
            string eidTo = email;
            string toshow = getall.SMTP_Sender.ToString();
            string from_mail = getall.SMTP_Email;
            string eidFrom = getall.SMTP_User.ToString();
            string password = getall.SMTP_Pwd.ToString();
            string ticket = ticket_no;
            string msgsub = getall.Email_Subject.ToString().Replace("{{ticketno}}", ticket_no);
            string hostname = getall.SMTP_HostName;
            string portname = getall.SMTP_Port.ToString();
            bool ssl_tof = true;
            string msgbody = getall.EmailConf_Body;
            if (ticket_no != "")
            {
                msgbody = getall.EmailConf_Body.ToString().Replace("{{name}}", name).Replace("{{ticketno}}", ticket).Replace("{{date}}", DateTime.Now.ToString());
            }
            MailMessage greetings = new MailMessage();
            SmtpClient smtp = new SmtpClient();
            try
            {
                greetings.From = new MailAddress(from_mail, toshow);//sendername
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

        [HttpGet]
        public ActionResult AnswerTicket(int? id)
        {
            if (Session["USER_CODE"] != null)
            {
                try
                {
                    ViewBag.Breadcrumb = "Answer and Reply";
                    if (id != null)
                    {
                        ViewBag.all_ticketansr = DbContext.tbl_DC_Ticket_Dtl.Where(x => x.Ticket_ID == id && x.Is_Active == true && x.Is_Deleted == false).ToList();
                        ViewBag.comments = DbContext.tbl_DC_Ticket_Thread.Where(x => x.Ticket_ID == id && x.Is_Active == true && x.Is_Deleted == false).OrderByDescending(x => x.Comment_ID).ToList();
                        var ticket_close_open = DbContext.tbl_DC_Ticket.Where(x => x.Ticket_ID == id && x.Is_Active == true && x.Is_Deleted == false).ToList();
                        if (ticket_close_open.Count > 0)
                        {
                            ViewBag.isclosed = ticket_close_open.FirstOrDefault().Status;
                            if (ticket_close_open.FirstOrDefault().Remark != null)
                            {
                                ViewBag.isremark = ticket_close_open.FirstOrDefault().Remark;
                            }
                            ViewBag.question = ticket_close_open.ToList();
                        }
                        else
                        {
                            return RedirectToAction("logout", "student");
                        }
                        ViewBag.check_answer = DbContext.tbl_DC_Ticket_Dtl.Where(x => x.Ticket_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    }
                    else
                    {
                        return RedirectToAction("logout", "student");
                    }
                }
                catch (Exception ex)
                {
                    return RedirectToAction("logout", "student");
                }
            }
            else
            {
                return RedirectToAction("logout", "student");
            }
            return View();
        }
        [HttpPost]
        public ActionResult AnswerReply(int? Ticket_id, int? Ticket_answerid, string msgbody, string MyImages)
        {
            string ms = string.Empty;
            //var pic = System.Web.HttpContext.Current.Request.Files["MyImagess"];
            if (Session["USER_CODE"] != null)
            {
                if (Ticket_id != null && Ticket_answerid != null && msgbody != "")
                {
                    try
                    {
                        int _Teacher_id = Convert.ToInt32(Convert.ToString(Session["USER_CODE"]).Substring(1));

                        tbl_DC_Ticket_Thread _ticket_thred = new tbl_DC_Ticket_Thread();

                        _ticket_thred.Ticket_ID = Ticket_id;
                        _ticket_thred.Ticket_Dtl_ID = Ticket_answerid;
                        _ticket_thred.User_Comment = msgbody;
                        _ticket_thred.User_Comment_Date = today;
                        _ticket_thred.User_Id = _Teacher_id;
                        _ticket_thred.Is_Teacher = false;
                        if (MyImages != null && MyImages != "")
                        {
                            string[] image1 = MyImages.Split(',');
                            string tktname = Convert.ToString(Ticket_answerid + CreateRandomPassword(6));
                            Base64ToImage_tkt(tktname, image1[1]);
                            MyImages = tktname + ".jpg";
                            _ticket_thred.R_image = MyImages;
                        }
                        _ticket_thred.Is_Active = true;
                        _ticket_thred.Is_Deleted = false;
                        DbContext.tbl_DC_Ticket_Thread.Add(_ticket_thred);
                        DbContext.SaveChanges();
                        ms = "1";
                        return Json(Convert.ToString(ms), JsonRequestBehavior.AllowGet);
                    }
                    catch (Exception ex)
                    {
                        ms = "2";
                        return Json(Convert.ToString(ms), JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    ms = "3";
                    return Json(Convert.ToString(ms), JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                ms = "4";
                return Json(Convert.ToString(ms), JsonRequestBehavior.AllowGet);
            }

        }
        public Image Base64ToImage_tkt(string tktname, string base64String)
        {
            // Convert base 64 string to byte[]
            byte[] imageBytes = Convert.FromBase64String(base64String);
            // Convert byte[] to Image
            using (var ms = new MemoryStream(imageBytes, 0, imageBytes.Length))
            {
                string filename = tktname + ".jpg";
                Image image = Image.FromStream(ms, true);
                var img = new Bitmap(Image.FromStream(ms));
                string tempFolderName = Server.MapPath("~/Images/Qusetionimages/" + filename);
                image.Save(tempFolderName, ImageFormat.Jpeg);
                return image;
            }
        }

        [HttpPost]
        public ActionResult GetData(int pageIndex, int pageSize)
        {
            int id = Convert.ToInt32(Convert.ToString(Session["USER_CODE"]).Substring(1));
            System.Threading.Thread.Sleep(4000);
            var count_Total = DbContext.View_DC_Tickets_and_Teacher.Where(x => x.Student_ID == id).Count();
            if (Convert.ToInt32(pageIndex * pageSize) > Convert.ToInt32(count_Total))
            {
                var get_all = "";
                return Json(get_all.ToList(), JsonRequestBehavior.AllowGet);
            }
            else
            {
                var query = (from c in DbContext.View_DC_Tickets_and_Teacher
                             where c.Student_ID == id
                             orderby c.Ticket_ID ascending
                             select c)
                              .Skip(pageIndex * pageSize)
                              .Take(pageSize);
                return Json(query.ToList(), JsonRequestBehavior.AllowGet);
            }


        }

        #endregion

        #region -------------------------------------------------studentonline test------------------------------------------------------
        public ActionResult Exam()
        {
            try
            {
                int id = 0;
                if (Session["USER_CODE"] != null)
                {
                    id = Convert.ToInt32(Convert.ToString(Session["USER_CODE"]).Substring(1));
                }
                var chaptersubscribes = DbContext.SP_DC_Getchapter(id).ToList();
                if (chaptersubscribes.Count > 0)
                {
                    string value1 = string.Empty;
                    foreach (var item in chaptersubscribes)
                    {

                        value1 += item.Chapter_Id.ToString() + ',';

                    }
                    value1 = value1.Substring(0, value1.Length - 1);
                    var examdetails = DbContext.SP_ExamList(id, 5).ToList();
                    if (examdetails.Count > 0)
                    {
                        ViewBag.Examnames = examdetails;
                    }
                    else
                    {
                        ViewBag.Examnames = null;
                    }
                    var subjectlist = DbContext.SP_ExamList(id, 5).Select(x => x.Subject_Id).Distinct().ToList();
                    if (subjectlist.Count > 0)
                    {
                        ViewBag.subjectlist = subjectlist;
                    }
                    else
                    {
                        ViewBag.subjectlist = null;
                    }
                }
                else
                {
                    var free_test = DbContext.Sp_DC_Free_Exam(id).ToList();

                    if (free_test.Count > 0)
                    {
                        ViewBag.free_test = free_test;
                        ViewBag.subjects = free_test.Select(x => x.Subject_Id).Distinct().ToList();
                    }
                    else
                    {
                        ViewBag.free_test = null;
                    }
                }
            }
            catch
            {
                return View();
            }
            return View();
        }
        public ActionResult Testresult(string id)
        {
            try
            {
                uint out_id;
                if (Session["USER_CODE"] != null)
                {
                    if (uint.TryParse(id,out out_id) == true)
                    {
                        //result id as parameter
                        int sid = Convert.ToInt32(Convert.ToString(Session["USER_CODE"]).Substring(1));
                        int rid = Convert.ToInt32(out_id);//result id
                        ViewBag.rid = rid;
                        var chk_auth = DbContext.tbl_DC_Exam_Result.Where(x => x.Result_ID == rid).FirstOrDefault();
                        if (chk_auth.Regd_ID == sid)
                        {
                            int eid = Convert.ToInt32(chk_auth.Exam_ID);
                            ViewBag.examid_lead = eid;
                            ViewBag.Questions = DbContext.SP_DC_Getallquestion_Appeard(rid).ToList();
                            ViewBag.Startegicreport = DbContext.SP_DC_Startegic_Report(rid).ToList();
                            ViewBag.examresult = DbContext.SP_DC_Examresultcalulation(sid, rid).ToList();
                            ViewBag.allquestions_appeard = DbContext.SP_DC_Getallquestion_Appeard(rid);
                            ViewBag.allexams = DbContext.tbl_DC_Exam.Where(x => x.Class_Id == chk_auth.Class_Id && x.Is_Active == true && x.Is_Deleted == false).ToList();
                            ViewBag.studentid = sid;
                        }
                    }
                    return View();
                }
                else
                {
                    return RedirectToAction("logout", "student");
                }
            }
            catch { return RedirectToAction("logout", "student"); }
        }

        public ActionResult Startegicreport()
        {
            try
            {
                if (checklogin(Session["USER_CODE"].ToString()) == "i")
                {
                    int id = Convert.ToInt32(Convert.ToString(Session["USER_CODE"]).Substring(1));
                    int eid = 1;//hardcoded due to demo data in database

                    return View();
                }
                else
                {
                    return RedirectToAction("logout", "student");
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("logout", "student");
            }
        }

        public ActionResult Lead_to_board()
        {
            int id = 0;
            if (Session["USER_CODE"] != null)
            {
                id = Convert.ToInt32(Convert.ToString(Session["USER_CODE"]).Substring(1));
            }
            var chaptersubscribes = DbContext.SP_DC_Getchapter(id).ToList();
            if (chaptersubscribes.Count > 0)
            {
                string value1 = string.Empty;
                foreach (var item in chaptersubscribes)
                {

                    value1 += item.Chapter_Id.ToString() + ',';

                }
                value1 = value1.Substring(0, value1.Length - 1);
                //var examdetails = DbContext.SP_ExamList(value1).ToList();
                // ViewBag.Examnames = examdetails;

            }
            return View();
        }

        public ActionResult Leaderboard(int id)
        {
            try
            {
                var exam_name = DbContext.tbl_DC_Exam.Where(x => x.Exam_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                if (exam_name != null)
                {
                    ViewBag.exam_name = exam_name.Exam_Name;
                }
                var Leader = DbContext.SP_DC_lead(id).ToList();
                if (Leader.Count > 0)
                {
                    ViewBag.Leaderboard = Leader.Take(10);

                    if (Session["USER_CODE"] != null)
                    {
                        int regdid = Convert.ToInt32(Convert.ToString(Session["USER_CODE"]).Substring(1));
                        if (Leader.Take(10).Where(x => x.Regd_ID == regdid).FirstOrDefault() == null)
                        {
                            int count = Leader.FindIndex(X => X.Regd_ID == regdid);
                            if (count != -1)
                            {
                                ViewBag.myrank = count + 1;
                                ViewBag.myresult = Leader.Where(x => x.Regd_ID == regdid).ToList();
                            }
                            else
                            {
                                ViewBag.myrank = null;
                                ViewBag.myresult = null;
                            }

                        }
                        else
                        {
                            ViewBag.myrank = null;
                            ViewBag.myresult = null;
                        }

                    }
                    else
                    {
                        ViewBag.myrank = null;
                        ViewBag.myresult = null;
                    }

                }
                else
                {
                    ViewBag.Leaderboard = null;
                }

                return View();
            }
            catch (Exception)
            { 
                return RedirectToAction("logout", "student"); 
            }
        }

        public ActionResult test(int id)
        {
            try
            {


                int eid = 0;
                var exam_name = DbContext.tbl_DC_Exam.Where(x => x.Exam_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                if (exam_name != null)
                {
                    ViewBag.Breadcrumb = exam_name.Exam_Name;
                }
                //Session["USER_CODE"] = 3;
                int regdid = 0;
                if (Session["USER_CODE"] != null)
                {
                    regdid = Convert.ToInt32(Convert.ToString(Session["USER_CODE"]).Substring(1));
                }
                var exam_Dtl = DbContext.tbl_DC_Exam.Where(x => x.Exam_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                DateTime utcTime = DateTime.UtcNow; // convert it to Utc using timezone setting of server computer

                TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                DateTime localTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, tzi);
                var timeing = localTime.AddMinutes(Convert.ToDouble(exam_Dtl.Time)).ToString("dd-MM-yyyy h:mm:ss tt");
                eid = Convert.ToInt32(exam_Dtl.Exam_type);
                DateTime TodayTime = today;

                Session["exam_type"] = eid;
                if (eid == 4)
                {
                    int min = Convert.ToInt32(Convert.ToDouble(exam_Dtl.Time));

                    string time = today.ToString("HH:mm");
                    var data = DbContext.tbl_DC_Exam.Where(x => x.Exam_ID == id && x.Is_Active == true && x.Is_Deleted == false && x.Is_Global == true).FirstOrDefault();
                    DateTime dty1 = Convert.ToDateTime(Convert.ToDateTime(data.Shedule_time).ToShortTimeString());
                    DateTime dt = Convert.ToDateTime(data.Shedule_date);
                    DateTime newDateTime = dt.Add(TimeSpan.Parse(data.Shedule_time));
                    //DateTime dty2 = Convert.ToDateTime(data.Shedule_date);
                    DateTime time_schedule = Convert.ToDateTime(dty1.ToString("HH:mm"));
                    DateTime final_time = Convert.ToDateTime(newDateTime.AddMinutes(min));
                    //DateTime final_timee = Convert.ToDateTime(dty2.AddMinutes(min));
                    ViewBag.final_time = final_time;
                    if (TodayTime > final_time)
                    {
                        ViewBag.message = "Exam already finished";
                        return View();
                    }
                    var res_dtl = DbContext.tbl_DC_Exam_Result.Where(x => x.Regd_ID == regdid && x.Exam_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    if (res_dtl != null)
                    {
                        ViewBag.message1 = "Exam finished";
                        return View();
                    }
                    ViewBag.TodayTime = TodayTime;
                    ViewBag.starttime = TodayTime;

                    //if (Session["stattime"] == null)
                    //{ 
                    // ViewBag.starttime = TodayTime;  Session["stattime"] = TodayTime;
                    //}
                    //else
                    //{

                    //    ViewBag.starttime = Session["stattime"];
                    //}

                    var test = final_time.Subtract(TodayTime);
                    ViewBag.days = test.Days;
                    ViewBag.hour = test.Hours;
                    ViewBag.minute = test.Minutes;
                    ViewBag.second = test.Seconds;
                    ViewBag.timeing = DateTime.Now.ToString("MM-dd-yyyy") + " " + ViewBag.hour + ":" + ViewBag.minute + ":" + ViewBag.second;
                    ViewBag.Rem_Time = time_schedule.AddMinutes(Convert.ToDouble(exam_Dtl.Time)).ToString("dd-MM-yyyy h:mm:ss tt");
                }
                else
                {
                    ViewBag.Rem_Time = timeing;
                    if (Session["stattime"] == null)
                    {
                        ViewBag.starttime = TodayTime;
                        Session["stattime"] = TodayTime;
                    }
                    else
                    {

                        ViewBag.starttime = Session["stattime"];
                    }
                }

                List<SP_DC_Procedurefor_test_Result> mod = null;

                if (Session["question"] == null)
                {
                    mod = DbContext.SP_DC_Procedurefor_test(id, regdid, eid).ToList();
                    Session["question"] = mod.ToList();
                    Session["timenow"] = timeing;

                }
                else
                {
                    mod = (List<SP_DC_Procedurefor_test_Result>)Session["question"];

                    //  DateTime dt = DateTime.Now;
                    //     var dts = Session["timenow"].ToString("dd/MM/yyyy hh:mm:ss");
                    //     DateTime dts1 = Convert.ToDateTime(dts);
                    //      //DateTime dts = (DateTime)Session["timenow"];
                    //   
                    //double time = dt.Subtract(dts).TotalMinutes;
                    //      //double exam_time =(double) exam_name.Time;
                    //if (dt > dts1)
                    //{
                    //    Session["lasttime"] = Session["timenow"];

                    //}
                    ViewBag.Rem_Time = Session["timenow"];// Convert.ToDateTime(dts - dt);

                }


                var exmobj = DbContext.tbl_DC_Exam.Where(x => x.Exam_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                if (mod != null)
                {
                    if (mod.Count > 0)
                    {
                        if (mod.Count == exmobj.Question_nos)
                        {
                            Session["quesdtls"] = mod;
                            DigiChampsModel.DigiChampsQuestionMasterModel obj = new DigiChampsModel.DigiChampsQuestionMasterModel();
                            obj.Regd_Id = regdid;
                            obj.Exam_Name = exam_Dtl.Exam_Name;
                            obj.Subject_Id = mod.ToList()[0].Subject_Id;
                            obj.Chapter_Id = mod.ToList()[0].ch_id;
                            obj.Question_nos = exam_Dtl.Question_nos;
                            obj.Class_Id = mod.ToList()[0].Class_Id;
                            obj.Board_Id = mod.ToList()[0].Board_Id;
                            obj.quesdtls = mod;
                            return View(obj);
                        }
                    }
                }
                Session["TEST"] = "TEST1";

            }
            catch
            {

            }
            return View();
        }
        [HttpPost]
        public ActionResult OnlineTest(DigiChampsModel.DigiChampsQuestionMasterModel obj, List<tbl_DC_Question_Answer> obj1, string[] rdanswer, int regdid, string Exam_Name, int subjectid, int ChapterId, int[] Qs_array, int boardid, int classid, int noofques, string startime)
        {
            int resultid = 0;
            int count = 0;
            Session["question"] = null;
            Session["timenow"] = null;
            Session["stattime"] = null;
            try
            {
                List<int> arr = Qs_array.Distinct().ToList();
                List<int> narr = new List<int>();
                if (rdanswer != null)
                {
                    for (int i = 0; i < rdanswer.Length; i++)
                    {
                        narr.Add(Convert.ToInt32(rdanswer[i].Split('+')[0]));
                    }
                }

                int quesattempt = 0;
                if (rdanswer != null)
                {

                    quesattempt = narr.Distinct().Count();
                    narr = narr.Distinct().ToList();

                }
                var subjectname = DbContext.tbl_DC_Subject.Where(x => x.Subject_Id == subjectid).FirstOrDefault();
                var chaptername = DbContext.tbl_DC_Chapter.Where(x => x.Chapter_Id == ChapterId).FirstOrDefault();
                var examname = DbContext.tbl_DC_Exam.Where(x => x.Exam_Name == Exam_Name).FirstOrDefault();
                var boardname = DbContext.tbl_DC_Board.Where(x => x.Board_Id == boardid).FirstOrDefault();
                var regdno = DbContext.tbl_DC_Registration.Where(x => x.Regd_ID == regdid).FirstOrDefault();
                tbl_DC_Exam_Result tblex_res = new tbl_DC_Exam_Result();
                tblex_res.Regd_ID = regdid;
                tblex_res.Subject_Id = subjectid;
                tblex_res.Subject_Name = subjectname.Subject;
                tblex_res.Regd_No = regdno.Regd_No;
                tblex_res.Question_Attempted = quesattempt;
                tblex_res.Chapter_Id = ChapterId;
                tblex_res.Chapter_Name = chaptername.Chapter;
                tblex_res.Exam_Name = examname.Exam_Name;
                tblex_res.Exam_ID = examname.Exam_ID;
                tblex_res.Board_Id = boardid;
                tblex_res.Class_Id = classid;
                tblex_res.Board_Name = boardname.Board_Name;
                tblex_res.Question_Nos = arr.Count();
                tblex_res.StartTime = Convert.ToDateTime(startime);
                tblex_res.EndTime = today;
                tblex_res.Is_Active = true;
                tblex_res.Is_Deleted = false;
                DbContext.tbl_DC_Exam_Result.Add(tblex_res);
                DbContext.SaveChanges();
                resultid = Convert.ToInt32(tblex_res.Result_ID);

                #region online correctanswer
                if (rdanswer != null)
                {
                    List<int> answer = new List<int>();
                    int question = 0;
                    int var = 0;
                    List<int> li = new List<int>();

                    int not_appear_qid = 0;
                    foreach (int v in arr)
                    {
                        if (narr.Contains(v) == false)
                        {
                            not_appear_qid = (Convert.ToInt32(v));//not appear questionid
                            //enter in the tbl_resultdtl with Is_Correct column as false
                            var ques = DbContext.tbl_DC_Question.Where(x => x.Question_ID == not_appear_qid).FirstOrDefault();
                            tbl_DC_Exam_Result_Dtl tblexredtls = new tbl_DC_Exam_Result_Dtl();
                            tblexredtls.Board_Id = boardid;
                            tblexredtls.Chapter_Id = ChapterId;
                            tblexredtls.Class_Id = classid;
                            tblexredtls.Question_ID = ques.Question_ID;
                            tblexredtls.Answer_ID = null;
                            tblexredtls.Subject_Id = subjectid;
                            tblexredtls.Result_ID = tblex_res.Result_ID;
                            tblexredtls.Question = ques.Question;
                            tblexredtls.Qustion_Desc = ques.Qustion_Desc;
                            tblexredtls.Topic_ID = ques.Topic_ID;
                            //tblexredtls.Answer_Desc = ques.Answer_Desc;
                            //tblexredtls.Answer_Image = ques.Answer_Image;
                            tblexredtls.Option_Desc = null;
                            tblexredtls.Option_Image = null;
                            tblexredtls.Is_Correct = false;
                            tblexredtls.Is_Active = true;
                            tblexredtls.Is_Deleted = false;
                            DbContext.tbl_DC_Exam_Result_Dtl.Add(tblexredtls);
                            DbContext.SaveChanges();
                        }
                    }

                    for (int j = 0; j < narr.Count; j++)
                    {
                        for (int i = 0; i < rdanswer.Length; i++)
                        {
                            if (question == Convert.ToInt32(rdanswer[i].Split('+')[0]))
                            {
                                question = Convert.ToInt32(rdanswer[i].Split('+')[0]);
                                answer.Add(Convert.ToInt32(rdanswer[i].Split('+')[1]));
                                var = 1;
                            }
                            else
                            {
                                if (question != 0 && question != Convert.ToInt32(rdanswer[i].Split('+')[0]))
                                {
                                    var answer_list = DbContext.tbl_DC_Question_Answer.Where(x => x.Question_ID == question && x.Is_Active == true && x.Is_Deleted == false && x.Is_Answer == true).ToList();
                                    foreach (var ans in answer_list)
                                    {
                                        li.Add(Convert.ToInt32(ans.Answer_ID));
                                    }
                                    var set = new HashSet<int>(answer);
                                    bool equals = set.SetEquals(li);
                                    if (equals == true)
                                    {
                                        //iscorrect=true
                                        var ques = DbContext.tbl_DC_Question.Where(x => x.Question_ID == question).FirstOrDefault();

                                        var found_Question_ = DbContext.tbl_DC_Exam_Result_Dtl.Where(x => x.Result_ID == resultid && x.Question_ID == question).FirstOrDefault();
                                        if (found_Question_ == null)
                                        {
                                            tbl_DC_Exam_Result_Dtl tblexredtls = new tbl_DC_Exam_Result_Dtl();
                                            tblexredtls.Board_Id = boardid;
                                            tblexredtls.Chapter_Id = ChapterId;
                                            tblexredtls.Class_Id = classid;
                                            tblexredtls.Question_ID = ques.Question_ID;
                                            tblexredtls.Answer_ID = null;
                                            tblexredtls.Subject_Id = subjectid;
                                            tblexredtls.Result_ID = tblex_res.Result_ID;
                                            tblexredtls.Question = ques.Question;
                                            tblexredtls.Qustion_Desc = ques.Qustion_Desc;
                                            tblexredtls.Topic_ID = ques.Topic_ID;
                                            //tblexredtls.Answer_Desc = ques.Answer_Desc;
                                            //tblexredtls.Answer_Image = ques.Answer_Image;
                                            tblexredtls.Option_Desc = null;
                                            tblexredtls.Option_Image = null;
                                            tblexredtls.Is_Correct = true;
                                            tblexredtls.Is_Active = true;
                                            tblexredtls.Is_Deleted = false;
                                            DbContext.tbl_DC_Exam_Result_Dtl.Add(tblexredtls);
                                            DbContext.SaveChanges();
                                            count = count + 1;//total correct answer count
                                            var tblex_res1 = DbContext.tbl_DC_Exam_Result.Where(x => x.Result_ID == tblex_res.Result_ID).FirstOrDefault();
                                            if (tblex_res1 != null)
                                            {
                                                tblex_res1.Total_Correct_Ans = count;
                                                DbContext.SaveChanges();
                                            }
                                        }
                                    }
                                    else
                                    {
                                        //iscorrect=false


                                        var ques = DbContext.tbl_DC_Question.Where(x => x.Question_ID == question).FirstOrDefault();
                                        var found_Question_ = DbContext.tbl_DC_Exam_Result_Dtl.Where(x => x.Result_ID == resultid && x.Question_ID == question).FirstOrDefault();
                                        if (found_Question_ == null)
                                        {
                                            tbl_DC_Exam_Result_Dtl tblexredtls = new tbl_DC_Exam_Result_Dtl();
                                            tblexredtls.Board_Id = boardid;
                                            tblexredtls.Chapter_Id = ChapterId;
                                            tblexredtls.Class_Id = classid;
                                            tblexredtls.Question_ID = ques.Question_ID;
                                            tblexredtls.Answer_ID = null;
                                            tblexredtls.Subject_Id = subjectid;
                                            tblexredtls.Result_ID = tblex_res.Result_ID;
                                            tblexredtls.Question = ques.Question;
                                            tblexredtls.Qustion_Desc = ques.Qustion_Desc;
                                            tblexredtls.Topic_ID = ques.Topic_ID;
                                            //tblexredtls.Answer_Desc = ques.Answer_Desc;
                                            //tblexredtls.Answer_Image = ques.Answer_Image;
                                            tblexredtls.Option_Desc = null;
                                            tblexredtls.Option_Image = null;
                                            tblexredtls.Is_Correct = false;
                                            tblexredtls.Is_Active = true;
                                            tblexredtls.Is_Deleted = false;
                                            DbContext.tbl_DC_Exam_Result_Dtl.Add(tblexredtls);
                                            DbContext.SaveChanges();
                                            var tblex_res1 = DbContext.tbl_DC_Exam_Result.Where(x => x.Result_ID == tblex_res.Result_ID).FirstOrDefault();
                                            if (tblex_res1 != null)
                                            {
                                                tblex_res1.Total_Correct_Ans = count;
                                                DbContext.SaveChanges();
                                            }
                                        }
                                    }

                                    answer.Clear();
                                    answer.Add(Convert.ToInt32(rdanswer[i].Split('+')[1]));
                                    question = Convert.ToInt32(rdanswer[i].Split('+')[0]);
                                    var = 0;
                                    li.Clear();
                                }
                                else
                                {
                                    if (var == 0)
                                    {

                                        question = Convert.ToInt32(rdanswer[i].Split('+')[0]);
                                        answer.Add(Convert.ToInt32(rdanswer[i].Split('+')[1]));
                                        //ifstudentappear onlyone question
                                        if (rdanswer.Length == 1)
                                        {
                                            var answer_list = DbContext.tbl_DC_Question_Answer.Where(x => x.Question_ID == question && x.Is_Active == true && x.Is_Deleted == false && x.Is_Answer == true).ToList();
                                            foreach (var ans in answer_list)
                                            {
                                                li.Add(Convert.ToInt32(ans.Answer_ID));
                                            }
                                            var set = new HashSet<int>(answer);
                                            bool equals = set.SetEquals(li);
                                            if (equals == true)
                                            {
                                                //iscorrect=true
                                                var ques = DbContext.tbl_DC_Question.Where(x => x.Question_ID == not_appear_qid).FirstOrDefault();

                                                var found_Question_ = DbContext.tbl_DC_Exam_Result_Dtl.Where(x => x.Result_ID == resultid && x.Question_ID == question).FirstOrDefault();
                                                if (found_Question_ == null)
                                                {
                                                    tbl_DC_Exam_Result_Dtl tblexredtls = new tbl_DC_Exam_Result_Dtl();
                                                    tblexredtls.Board_Id = boardid;
                                                    tblexredtls.Chapter_Id = ChapterId;
                                                    tblexredtls.Class_Id = classid;
                                                    tblexredtls.Question_ID = ques.Question_ID;
                                                    tblexredtls.Answer_ID = null;
                                                    tblexredtls.Subject_Id = subjectid;
                                                    tblexredtls.Result_ID = tblex_res.Result_ID;
                                                    tblexredtls.Question = ques.Question;
                                                    tblexredtls.Qustion_Desc = ques.Qustion_Desc;
                                                    tblexredtls.Topic_ID = ques.Topic_ID;
                                                    //tblexredtls.Answer_Desc = ques.Answer_Desc;
                                                    //tblexredtls.Answer_Image = ques.Answer_Image;
                                                    tblexredtls.Option_Desc = null;
                                                    tblexredtls.Option_Image = null;
                                                    tblexredtls.Is_Correct = true;
                                                    tblexredtls.Is_Active = true;
                                                    tblexredtls.Is_Deleted = false;
                                                    DbContext.tbl_DC_Exam_Result_Dtl.Add(tblexredtls);
                                                    DbContext.SaveChanges();
                                                    count = count + 1;//total correct answer count
                                                    var tblex_res1 = DbContext.tbl_DC_Exam_Result.Where(x => x.Result_ID == tblex_res.Result_ID).FirstOrDefault();
                                                    if (tblex_res1 != null)
                                                    {
                                                        tblex_res1.Total_Correct_Ans = count;
                                                        DbContext.SaveChanges();
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                //iscorrect=false
                                                var ques = DbContext.tbl_DC_Question.Where(x => x.Question_ID == not_appear_qid).FirstOrDefault();
                                                var found_Question_ = DbContext.tbl_DC_Exam_Result_Dtl.Where(x => x.Result_ID == resultid && x.Question_ID == question).FirstOrDefault();
                                                if (found_Question_ == null)
                                                {
                                                    tbl_DC_Exam_Result_Dtl tblexredtls = new tbl_DC_Exam_Result_Dtl();
                                                    tblexredtls.Board_Id = boardid;
                                                    tblexredtls.Chapter_Id = ChapterId;
                                                    tblexredtls.Class_Id = classid;
                                                    tblexredtls.Question_ID = ques.Question_ID;
                                                    tblexredtls.Answer_ID = null;
                                                    tblexredtls.Subject_Id = subjectid;
                                                    tblexredtls.Result_ID = tblex_res.Result_ID;
                                                    tblexredtls.Question = ques.Question;
                                                    tblexredtls.Qustion_Desc = ques.Qustion_Desc;
                                                    tblexredtls.Topic_ID = ques.Topic_ID;
                                                    //tblexredtls.Answer_Desc = ques.Answer_Desc;
                                                    //tblexredtls.Answer_Image = ques.Answer_Image;
                                                    tblexredtls.Option_Desc = null;
                                                    tblexredtls.Option_Image = null;
                                                    tblexredtls.Is_Correct = false;
                                                    tblexredtls.Is_Active = true;
                                                    tblexredtls.Is_Deleted = false;
                                                    DbContext.tbl_DC_Exam_Result_Dtl.Add(tblexredtls);
                                                    DbContext.SaveChanges();
                                                    var tblex_res1 = DbContext.tbl_DC_Exam_Result.Where(x => x.Result_ID == tblex_res.Result_ID).FirstOrDefault();
                                                    if (tblex_res1 != null)
                                                    {
                                                        tblex_res1.Total_Correct_Ans = count;
                                                        DbContext.SaveChanges();
                                                    }
                                                }
                                            }
                                            answer.Clear();
                                            answer.Add(Convert.ToInt32(rdanswer[i].Split('+')[1]));
                                            question = Convert.ToInt32(rdanswer[i].Split('+')[0]);
                                            var = 0;
                                            li.Clear();
                                        }
                                    }
                                    else
                                    {
                                        //getting all the answerid of question
                                        var answer_list = DbContext.tbl_DC_Question_Answer.Where(x => x.Question_ID == question && x.Is_Active == true && x.Is_Deleted == false && x.Is_Answer == true).ToList();

                                        foreach (var ans in answer_list)
                                        {
                                            li.Add(Convert.ToInt32(ans.Answer_ID));
                                        }
                                        var set = new HashSet<int>(answer);
                                        bool equals = set.SetEquals(li);
                                        if (equals == true)
                                        {
                                            //iscorrect=true
                                            var ques = DbContext.tbl_DC_Question.Where(x => x.Question_ID == not_appear_qid).FirstOrDefault();

                                            var found_Question_ = DbContext.tbl_DC_Exam_Result_Dtl.Where(x => x.Result_ID == resultid && x.Question_ID == question).FirstOrDefault();
                                            if (found_Question_ == null)
                                            {

                                                tbl_DC_Exam_Result_Dtl tblexredtls = new tbl_DC_Exam_Result_Dtl();
                                                tblexredtls.Board_Id = boardid;
                                                tblexredtls.Chapter_Id = ChapterId;
                                                tblexredtls.Class_Id = classid;
                                                tblexredtls.Question_ID = ques.Question_ID;
                                                tblexredtls.Answer_ID = null;
                                                tblexredtls.Subject_Id = subjectid;
                                                tblexredtls.Result_ID = tblex_res.Result_ID;
                                                tblexredtls.Question = ques.Question;
                                                tblexredtls.Qustion_Desc = ques.Qustion_Desc;
                                                tblexredtls.Topic_ID = ques.Topic_ID;
                                                //tblexredtls.Answer_Desc = ques.Answer_Desc;
                                                //tblexredtls.Answer_Image = ques.Answer_Image;
                                                tblexredtls.Option_Desc = null;
                                                tblexredtls.Option_Image = null;
                                                tblexredtls.Is_Correct = true;
                                                tblexredtls.Is_Active = true;
                                                tblexredtls.Is_Deleted = false;
                                                DbContext.tbl_DC_Exam_Result_Dtl.Add(tblexredtls);
                                                DbContext.SaveChanges();
                                                count = count + 1;//total correct answer count
                                                var tblex_res1 = DbContext.tbl_DC_Exam_Result.Where(x => x.Result_ID == tblex_res.Result_ID).FirstOrDefault();
                                                if (tblex_res1 != null)
                                                {
                                                    tblex_res1.Total_Correct_Ans = count;
                                                    DbContext.SaveChanges();
                                                }
                                            }
                                        }
                                        else
                                        {
                                            //iscorrect=false
                                            var ques = DbContext.tbl_DC_Question.Where(x => x.Question_ID == not_appear_qid).FirstOrDefault();

                                            var found_Question_ = DbContext.tbl_DC_Exam_Result_Dtl.Where(x => x.Result_ID == resultid && x.Question_ID == question).FirstOrDefault();
                                            if (found_Question_ == null)
                                            {
                                                tbl_DC_Exam_Result_Dtl tblexredtls = new tbl_DC_Exam_Result_Dtl();
                                                tblexredtls.Board_Id = boardid;
                                                tblexredtls.Chapter_Id = ChapterId;
                                                tblexredtls.Class_Id = classid;
                                                tblexredtls.Question_ID = ques.Question_ID;
                                                tblexredtls.Answer_ID = null;
                                                tblexredtls.Subject_Id = subjectid;
                                                tblexredtls.Result_ID = tblex_res.Result_ID;
                                                tblexredtls.Question = ques.Question;
                                                tblexredtls.Qustion_Desc = ques.Qustion_Desc;
                                                tblexredtls.Topic_ID = ques.Topic_ID;
                                                //tblexredtls.Answer_Desc = ques.Answer_Desc;
                                                //tblexredtls.Answer_Image = ques.Answer_Image;
                                                tblexredtls.Option_Desc = null;
                                                tblexredtls.Option_Image = null;
                                                tblexredtls.Is_Correct = false;
                                                tblexredtls.Is_Active = true;
                                                tblexredtls.Is_Deleted = false;
                                                DbContext.tbl_DC_Exam_Result_Dtl.Add(tblexredtls);
                                                DbContext.SaveChanges();
                                                var tblex_res1 = DbContext.tbl_DC_Exam_Result.Where(x => x.Result_ID == tblex_res.Result_ID).FirstOrDefault();
                                                if (tblex_res1 != null)
                                                {
                                                    tblex_res1.Total_Correct_Ans = count;
                                                    DbContext.SaveChanges();
                                                }
                                            }
                                        }

                                        answer.Clear();
                                        answer.Add(Convert.ToInt32(rdanswer[i].Split('+')[1]));
                                        question = Convert.ToInt32(rdanswer[i].Split('+')[0]);
                                        var = 0;
                                        li.Clear();
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    int totq = 0;
                    for (int k = 0; k < arr.Count; k++)
                    {
                        int qusetionid = arr[k];
                        var ques = DbContext.tbl_DC_Question.Where(x => x.Question_ID == qusetionid).FirstOrDefault();
                        var found_Question_ = DbContext.tbl_DC_Exam_Result_Dtl.Where(x => x.Result_ID == resultid && x.Question_ID == qusetionid).FirstOrDefault();
                        if (found_Question_ == null)
                        {
                            tbl_DC_Exam_Result_Dtl tblexredtls = new tbl_DC_Exam_Result_Dtl();
                            tblexredtls.Board_Id = boardid;
                            tblexredtls.Chapter_Id = ChapterId;
                            tblexredtls.Class_Id = classid;
                            tblexredtls.Question_ID = ques.Question_ID;
                            tblexredtls.Answer_ID = null;
                            tblexredtls.Subject_Id = subjectid;
                            tblexredtls.Result_ID = tblex_res.Result_ID;
                            tblexredtls.Question = ques.Question;
                            tblexredtls.Qustion_Desc = ques.Qustion_Desc;
                            tblexredtls.Topic_ID = ques.Topic_ID;
                            //tblexredtls.Answer_Desc = ques.Answer_Desc;
                            //tblexredtls.Answer_Image = ques.Answer_Image;
                            tblexredtls.Option_Desc = null;
                            tblexredtls.Option_Image = null;
                            tblexredtls.Is_Correct = false;
                            tblexredtls.Is_Active = true;
                            tblexredtls.Is_Deleted = false;
                            DbContext.tbl_DC_Exam_Result_Dtl.Add(tblexredtls);
                            DbContext.SaveChanges();
                            var tblex_res1 = DbContext.tbl_DC_Exam_Result.Where(x => x.Result_ID == tblex_res.Result_ID).FirstOrDefault();
                            if (tblex_res1 != null)
                            {
                                tblex_res1.Total_Correct_Ans = totq;
                                DbContext.SaveChanges();
                            }
                        }
                    }
                } 
                #endregion

                // resultid = Convert.ToInt32(tblex_res.Result_ID);
                string correct = "";
                var tblres = DbContext.tbl_DC_Exam_Result.Where(x => x.Result_ID == tblex_res.Result_ID).FirstOrDefault();
                if (tblres != null)
                {
                    correct = Convert.ToString(tblres.Total_Correct_Ans);
                }
                //Mail after appearing Exam
                if (regdno.Email != null && regdno.Email != "")
                {
                    var getall = DbContext.SP_DC_Get_maildetails("APREXAM").FirstOrDefault();
                    if (getall != null)
                    {
                        string msgbody = getall.EmailConf_Body.ToString().Replace("{{name}}", regdno.Customer_Name).Replace("{{correctno}}", correct).Replace("{{test}}", tblex_res.Exam_Name).Replace("{{url}}", "http://thedigichamps.com/student/leaderboard/" + tblex_res.Exam_ID);
                        sendMail1("APREXAM", regdno.Email, getall.Email_Subject, regdno.Customer_Name, msgbody);
                    }
                }
                try
                {
                    var pushnot = (from c in DbContext.tbl_DC_Registration.Where(x => x.Regd_ID == regdno.Regd_ID)

                                   select new { c.Regd_ID, c.Device_id }).FirstOrDefault();
                  
                    string body = "Reultid#{{resid}}#Examid#{{examid}}# Exam Appeared : Hello {{name}},! You have attempted {{correctno}} Correct Answers in {{test}} exam. View your test details and leader board now.";
                    string msg = body.ToString().Replace("{{name}}", regdno.Customer_Name).Replace("{{correctno}}", correct.ToString()).Replace("{{test}}", tblex_res.Exam_Name).Replace("{{resid}}", resultid.ToString()).Replace("{{examid}}", examname.Exam_ID.ToString());
                    if (pushnot != null)
                    {
                        if (pushnot.Device_id != null)
                        {
                            var note = new PushNotiStatus(msg, pushnot.Device_id);
                        }
                    }
                }
                catch (Exception)
                {


                }
            }
            catch
            {
                return RedirectToAction("Logout", "Student");
            }
            return RedirectToAction("testresult", new { id = resultid });
        }
        [HttpGet]
        public ActionResult pre_requisite_test()
        {
            try
            {
                int id = 0;
                if (Session["USER_CODE"] != null)
                {
                    id = Convert.ToInt32(Convert.ToString(Session["USER_CODE"]).Substring(1));
                }
                var chaptersubscribes = DbContext.SP_DC_Getchapter(id).ToList();
                if (chaptersubscribes.Count > 0)
                {
                    string value1 = string.Empty;
                    foreach (var item in chaptersubscribes)
                    {

                        value1 += item.Chapter_Id.ToString() + ',';

                    }
                    value1 = value1.Substring(0, value1.Length - 1);
                    var examdetails = DbContext.SP_ExamList(id, 1).ToList();
                    if (examdetails.Count > 0)
                    {
                        ViewBag.Examnames = examdetails;
                    }
                    else
                    {
                        ViewBag.Examnames = null;
                    }
                    var subjectlist = DbContext.SP_ExamList(id, 1).Select(x => x.Subject_Id).Distinct().ToList();
                    if (subjectlist.Count > 0)
                    {
                        ViewBag.subjectlist = subjectlist;
                    }
                    else
                    {
                        ViewBag.subjectlist = null;
                    }
                }
                else
                {
                    var free_test = DbContext.Sp_DC_Free_Exam(id).ToList();
                    if (free_test.Count > 0)
                    {

                        ViewBag.free_test = free_test;
                        ViewBag.subjects = free_test.Select(x => x.Subject_Id).Distinct().ToList();
                    }
                    else
                    {
                        ViewBag.free_test = null;
                    }
                }
            }
            catch
            {
                return View();
            }
            return View();
        }
        #endregion

        #region---------------------------------------------LOGIN/Status/userid/cheklogin-----------------------------------------------
        #region Auto_password
        public static string CreateRandomPassword(int PasswordLength)
        {
            string _allowedChars = "0123456789abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNOPQRSTUVWXYZ";
            Random randNum = new Random();
            char[] chars = new char[PasswordLength];
            int allowedCharCount = _allowedChars.Length;
            for (int i = 0; i < PasswordLength; i++)
            {
                chars[i] = _allowedChars[(int)((_allowedChars.Length) * randNum.NextDouble())];
            }
            return new string(chars);
        }
        #endregion

        [HttpPost]
        public JsonResult updatenoty(string[] notyid) //check notification
        {
            var noty = new List<tbl_DC_Notification>();
           
            try
            {
                if (Session["USER_CODE"] == null)
                {
                    Response.Redirect("/Student/Logout");
                }
                else
                {
                  int _student_id = Convert.ToInt32(Convert.ToString(Session["USER_CODE"]).Substring(1));
                    if (notyid !=null)
                    {
                        
                        for (int id = 0; id < notyid.Length; id++)
                        {
                            int notid = Convert.ToInt32(notyid[id].ToString());
                            var todelete = DbContext.tbl_DC_Notification.Where(x => x.Regdno == _student_id && x.Id == notid).FirstOrDefault();
                            todelete.Is_Clicked = true;
                            DbContext.SaveChanges();
                        }
                        DateTime datadate = today.AddDays(-7);
                        noty = DbContext.tbl_DC_Notification.Where(x => x.Regdno == _student_id).OrderByDescending(x => x.Inserted_Date).Take(25).ToList();
                    }
                    else
                    {
                        noty = DbContext.tbl_DC_Notification.Where(x => x.Regdno == _student_id).OrderByDescending(x => x.Inserted_Date).Take(25).ToList();
                    }
                    
                }
            }
            catch {

                
            }
            return Json(noty);
        }

        [HttpPost]
        public JsonResult checknoty() //check notification
        {
            var noty = new List<tbl_DC_Notification>();
            try
            {
                if (Session["USER_CODE"] == null)
                {
                    Response.Redirect("/Student/Logout");
                }
                else
                {
                    int _student_id = Convert.ToInt32(Convert.ToString(Session["USER_CODE"]).Substring(1));
                   // DateTime datadate = today.AddDays(-7);
                    noty = DbContext.tbl_DC_Notification.Where(x => x.Regdno == _student_id  && x.Is_Clicked == false).OrderByDescending(x => x.Inserted_Date).ToList();
                }
            }
            catch { }
            return Json(noty);
        }

        public ActionResult authenticateuser()
        {
            if (Session["USER_CODE"] == null)
            {
                Response.Redirect("/Student/Logout");
            }
                return View();
        }

        public void loginstatus(string usercode)
        { 
            try
            {
                string hostNames = Dns.GetHostName();
                IPAddress[] arrs = Dns.GetHostEntry(hostNames).AddressList;
                string ip = arrs[1].ToString();

                   
                    tbl_DC_LoginStatus t_logi = new tbl_DC_LoginStatus();
                    t_logi.Login_ID = usercode.Trim();
                    t_logi.Login_IPAddress = ip;
                    t_logi.Login_DateTime = DateTime.Now;
                    var log_name = DbContext.tbl_DC_USER_SECURITY.Where(x => x.USER_CODE == usercode).Select(x => x.USER_NAME).FirstOrDefault();
                    t_logi.Login_By = log_name;
                    t_logi.Status = true;
                    DbContext.tbl_DC_LoginStatus.Add(t_logi);
                    DbContext.SaveChanges();
                    int id = Convert.ToInt32(t_logi.id);
                   DbContext.login_fin_status(id);
                   Session["sessionid"] = id;
            }
            catch (Exception ex)
            {
                ex.Message.ToString();
            }
        }

        [HttpPost]
        public JsonResult getstatusoflogin()
        {    
            int msg = 0; 
            try
            {
                if(Session["sessionid"]!=null)
                {
                    int sid = Convert.ToInt32(Session["sessionid"]);
                    bool get_logdata = Convert.ToBoolean(DbContext.tbl_DC_LoginStatus.Where(x => x.id == sid).Select(x =>    x.Status).FirstOrDefault());

                    if (get_logdata == true)
                    {
                        msg = 1;
                    }
                }
                else if (Session["USER_CODE"] != null)
                    { 
                        string s=Session["USER_CODE"].ToString().Trim();
                        var get = DbContext.tbl_DC_LoginStatus.Where(x => x.Login_ID == s && x.Status == true).FirstOrDefault();
                        if(get!=null)
                        {
                            //verify account
                            ViewBag.userSTATUS = 1;
                            msg = 0;
                        }
                        else
                        {
                            //account login
                            loginstatus(s);
                            msg = 1;
                        }
                    }
                else { ViewBag.userSTATUS = 1; msg = 0; }
            }
            catch (Exception)
            {
                msg = 0;
            }
            return Json(msg, JsonRequestBehavior.AllowGet);
        }
        public void logoutstatus()
        {
            try
            {
                int sid = Convert.ToInt32(Session["sessionid"]);
                if(Session["sessionid"]!=null)
                {
                tbl_DC_LoginStatus t_logo = DbContext.tbl_DC_LoginStatus.Where(x => x.id == sid).FirstOrDefault();
                if (t_logo != null)
                {
                    t_logo.Logout_DateTime = DateTime.Now;
                    t_logo.Status = false;
                    DbContext.SaveChanges();
                }
                }
            }
            catch (Exception ex)
            {
                ex.Message.ToString();
            }

        }
        public string checklogin(string usercode)
        {
           string message=string.Empty;
            var found_id = DbContext.tbl_DC_LoginStatus.Where(x => x.Login_ID == usercode).FirstOrDefault();
            DateTime _dt_log = Convert.ToDateTime(Session["Time_for_login"]);
            DateTime _dt_db=Convert.ToDateTime(found_id.Login_DateTime);
            if (_dt_log.ToLongTimeString() == _dt_db.ToLongTimeString())
            {
                 message = "i";
            }else
            {
                message = "o";
            }
            return message;
        }
        #region ------getuserid-----
        public int getuserid(object o)
        {
            string data = o.ToString();
            int id = 0;
            var prefix = DbContext.tbl_DC_Prefix.Where(X => X.PrefixType_ID == 2).ToList();
            for (int i = 0; i < prefix.Count; i++)
            {
                string find = Convert.ToString(prefix[i].Prefix_Name);
                if (data.Contains(find))
                {
                    int count = find.Length;
                    id = Convert.ToInt32(data.Substring(count).Trim());

                }
            }
            return id;
        }
        #endregion
        #endregion

        #region-----------------------------------------------Package Preview-----------------------------------------------------------
        public ActionResult PackagePreview(int? id)
        {
            try
            {
                if (Session["USER_NAME"] != null)
                {
                    if (id == null)
                    {
                        string u_name = Session["USER_NAME"].ToString();

                        var data1 = (from k in DbContext.tbl_DC_Registration.Where(x => x.Mobile == u_name && x.Is_Active == true && x.Is_Deleted == false)
                                     join l in DbContext.tbl_DC_Registration_Dtl.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                         on k.Regd_ID equals l.Regd_ID
                                     join m in DbContext.tbl_DC_Class.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                     on l.Class_ID equals m.Class_Id
                                     select new PackagePreviewModel
                                     {
                                         Board_Id = l.Board_ID,
                                         Class_Id = l.Class_ID,
                                         Class_Name = m.Class_Name
                                     }).ToList();
                        int bid = Convert.ToInt32(data1.ToList()[0].Board_Id);
                        ViewBag.class_name = DbContext.tbl_DC_Class.Where(x => x.Board_Id == bid && x.Is_Active == true && x.Is_Deleted == false).ToList(); //select class dropdown
                        int cls_id = Convert.ToInt32(data1.ToList()[0].Class_Id);
                        ViewBag.cls = data1.ToList()[0].Class_Name;
                        ViewBag.Package_Preview = DbContext.SP_DC_Student_PacakgeModule_ID(cls_id).OrderByDescending(X => X.Package_ID).ToList();
                    }
                    else
                    {
                        string u_name = Session["USER_NAME"].ToString();
                        var data1 = (from k in DbContext.tbl_DC_Registration.Where(x => x.Mobile == u_name && x.Is_Active == true && x.Is_Deleted == false)
                                     join l in DbContext.tbl_DC_Registration_Dtl.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                         on k.Regd_ID equals l.Regd_ID
                                     join m in DbContext.tbl_DC_Class.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                     on l.Class_ID equals m.Class_Id
                                     select new PackagePreviewModel
                                     {
                                         Board_Id = l.Board_ID,
                                         Class_Id = l.Class_ID,
                                         Class_Name = m.Class_Name
                                     }).ToList();
                        int bid = Convert.ToInt32(data1.ToList()[0].Board_Id);
                        ViewBag.class_name = DbContext.tbl_DC_Class.Where(x => x.Board_Id == bid && x.Is_Active == true && x.Is_Deleted == false).ToList(); //select class dropdown
                        ViewBag.Package_Preview = DbContext.SP_DC_Student_PacakgeModule_ID(id).OrderByDescending(X => X.Package_ID).ToList();
                        var dat = DbContext.tbl_DC_Class.Where(x => x.Class_Id == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        ViewBag.cls = dat.Class_Name;
                    }

                    ViewBag.Register_user = DbContext.tbl_DC_Registration.Where(x => x.Is_Active == true && x.Is_Deleted == false).ToList().Count;
                    ViewBag.package_sold = DbContext.tbl_DC_Order_Pkg.Where(x => x.Is_Active == true && x.Is_Deleted == false).ToList().Count;
                    ViewBag.exam_appear = DbContext.tbl_DC_Exam_Result.Where(x => x.Is_Active == true && x.Is_Deleted == false).ToList().Count;
                    ViewBag.doubt_cleared = DbContext.tbl_DC_Ticket_Dtl.Where(x => x.Is_Active == true && x.Is_Deleted == false).ToList().Count;
                }

                else
                {
                    return RedirectToAction("logout", "student");
                }
            }
            catch (Exception ex)
            {
                ex.Message.ToString();
            }
            return View();
        }

        [HttpPost]
        public ActionResult PackagePreview(int? classid, int? nothing)
        {
            try
            {
                ViewBag.Package_Preview = DbContext.SP_DC_Student_PacakgeModule_ID(classid).ToList();
                ViewBag.class_name = DbContext.tbl_DC_Class.Where(x => x.Is_Active == true && x.Is_Deleted == false).ToList();
            }
            catch (Exception ex)
            {
                ex.Message.ToString();
            }
            return View("packagepreview");
        }

        public ActionResult ViewPackageDetail(int? id)
        {
            try
            {
                var data1 = (from z in DbContext.tbl_DC_Package.Where(x => x.Package_ID == id && x.Is_Active == true && x.Is_Deleted == false)
                             join y in DbContext.tbl_DC_PackageSub_Dtl.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                             on z.Package_ID equals y.Package_ID
                             select new PackagePreviewModel
                             {
                                 Package_ID = z.Package_ID,
                                 Package_Name = z.Package_Name,
                                 Package_Desc = z.Package_Desc,
                                 Subscripttion_Period = z.Subscripttion_Period,
                                 Total_Chapter = z.Total_Chapter,
                                 Price = z.Price,
                                 Thumbnail = z.Thumbnail,
                                 SubScription_Limit = y.SubScription_Limit,
                                 Tablet_Id=z.Tablet_Id
                             }).ToList();
                ViewBag.Package_Previeww = data1.Take(1);
                ViewBag.limit = data1.ToList().Select(x => x.SubScription_Limit).Sum();


                ViewBag.chapters = DbContext.tbl_DC_Package_Dtl.Where(x => x.Package_ID == id && x.Is_Active == true && x.Is_Deleted == false).ToList().Count;

                var video = (from package in DbContext.tbl_DC_Package_Dtl.Where(x => x.Package_ID == id && x.Is_Active == true && x.Is_Deleted == false)
                             join module in DbContext.tbl_DC_Module.Where(x => x.Is_Active == true && x.Is_Deleted == false && x.Module_video != null)
                             on package.Chapter_Id equals module.Chapter_Id
                             select new DigiChampCartModel
                             {
                                 Module_Name = module.Module_Name
                             }).ToList();
                ViewBag.video = video.Count;

                Session["limit"] = ViewBag.limit;
                Session["pkg_id"] = Convert.ToInt32(data1.ToList()[0].Package_ID);
                Session["price"] = data1.ToList()[0].Price;

                var a = DbContext.tbl_DC_Package.Where(x => x.Package_ID == id && x.Is_Active == true && x.Is_Deleted == false).ToList();
                var b = DbContext.tbl_DC_Package_Dtl.Where(x => x.Package_ID == id && x.Is_Active == true && x.Is_Deleted == false).GroupBy(x => x.Subject_Id).ToList();
                var c = DbContext.tbl_DC_Subject.Where(x => x.Is_Active == true && x.Is_Deleted == false).ToList();
                var data2 = (from q in a
                             join p in b
                             on q.Package_ID equals p.FirstOrDefault().Package_ID
                             join r in c
                             on p.FirstOrDefault().Subject_Id equals r.Subject_Id
                             select new PackagePreviewModel
                             {
                                 Subject_Id = p.FirstOrDefault().Subject_Id,
                                 Subject = r.Subject
                             }).ToList();

                ViewBag.subject_name = data2;


            }
            catch (Exception ex)
            {
                ex.Message.ToString();
            }
            return View();
        }

        public ActionResult ViewChapterDetails(int? id)
        {
            try
            {
                int id1 = Convert.ToInt32(Session["pkg_id"]);
                Session["subid"] = id;
                Session["id"] = id1;
                var data2 = (from q in DbContext.tbl_DC_Package.Where(x => x.Package_ID == id1 && x.Is_Active == true && x.Is_Deleted == false)
                             join p in DbContext.tbl_DC_Package_Dtl.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                             on q.Package_ID equals p.Package_ID
                             join r in DbContext.tbl_DC_Subject.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                             on p.Subject_Id equals r.Subject_Id
                             select new PackagePreviewModel
                             {
                                 Subject_Id = p.Subject_Id,
                                 Subject = r.Subject
                             }).OrderBy(x => x.Subject_Id).Distinct().ToList();

                ViewBag.subject_name = data2;

                var data = (from a in DbContext.tbl_DC_Subject.Where(x => x.Subject_Id == id && x.Is_Active == true && x.Is_Deleted == false)
                            join b in DbContext.tbl_DC_Chapter.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                            on a.Subject_Id equals b.Subject_Id
                            select new PackagePreviewModel
                            {
                                Subject_Id = a.Subject_Id,
                                Chapter_Id = b.Chapter_Id,
                                Chapter = b.Chapter
                                //Module_ID=c.Module_ID,
                                //Module_Name=c.Module_Name
                            }).ToList();
                ViewBag.module_name = data;
            }
            catch (Exception ex)
            {
                ex.Message.ToString();
            }
            return RedirectToAction("viewpackagedetail");
        }

        [HttpPost]
        public ActionResult ViewChapterDetails(int[] chkpackage)
        {
            string message = string.Empty;
            try
            {

                if (chkpackage != null)
                {
                    var data1 = chkpackage.ToList();
                    string u_name = Session["USER_NAME"].ToString();
                    int pkgid = Convert.ToInt32(Session["pkg_id"]);
                    int limit = Convert.ToInt32(Session["limit"]);
                    decimal price = Convert.ToDecimal(Session["price"]);

                    if (data1.Count > limit)
                    {
                        message = "error";
                        return Json(message, JsonRequestBehavior.AllowGet);
                        //return Json("ViewPackageDetail");
                    }
                    else if (data1.Count < limit)
                    {
                        message = "error1";
                        return Json(message, JsonRequestBehavior.AllowGet);
                        //return RedirectToAction("ViewPackageDetail");
                    }
                    else
                    {
                        var data = DbContext.tbl_DC_Registration.Where(x => x.Mobile == u_name && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();

                        int id = Convert.ToInt32(data.Regd_ID);
                        string regno = data.Regd_No;

                        var ord = DbContext.tbl_DC_Cart.Where(x => x.Regd_ID == id && x.Status == true && x.In_Cart == true && x.Is_Paid == false).FirstOrDefault();

                        tbl_DC_Cart crt = new tbl_DC_Cart();
                        crt.Package_ID = pkgid;
                        crt.Order_ID = null;
                        crt.Order_No = null;
                        crt.Regd_ID = id;
                        crt.Regd_No = regno;
                        crt.Status = true;
                        crt.In_Cart = true;
                        crt.Is_Paid = false;
                        crt.Is_Active = true;
                        crt.Is_Deleted = false;
                        crt.Total_Amt = price;
                        crt.Inserted_Date = DateTime.Now;
                        crt.Inserted_By = HttpContext.User.Identity.Name;
                        DbContext.tbl_DC_Cart.Add(crt);
                        DbContext.SaveChanges();

                        if (ord != null)
                        {
                            crt.Order_ID = ord.Cart_ID;
                            DateTime today = DateTime.Now.Date;
                            var ordid = crt.Order_ID;
                            if (ordid == null)
                            {
                                crt.Order_No = "DGCRT" + "/" + today.ToString("yyMMdd") + "/" + "00000" + 1;
                            }
                            else
                            {
                                int ordno = Convert.ToInt32(crt.Order_ID);
                                if (ordno > 0 && ordno <= 9)
                                {
                                    crt.Order_No = "DGCRT" + "/" + today.ToString("yyMMdd") + "/" + "00000" + Convert.ToString(ordno);
                                }
                                if (ordno > 9 && ordno <= 99)
                                {
                                    crt.Order_No = "DGCRT" + "/" + today.ToString("yyMMdd") + "/" + "0000" + Convert.ToString(ordno);
                                }
                                if (ordno > 99 && ordno <= 999)
                                {
                                    crt.Order_No = "DGCRT" + "/" + today.ToString("yyMMdd") + "/" + "000" + Convert.ToString(ordno);
                                }
                                if (ordno > 999 && ordno <= 9999)
                                {
                                    crt.Order_No = "DGCRT" + "/" + today.ToString("yyMMdd") + "/" + "00" + Convert.ToString(ordno);
                                }
                                if (ordno > 9999 && ordno <= 99999)
                                {
                                    crt.Order_No = "DGCRT" + "/" + today.ToString("yyMMdd") + "/" + "0" + Convert.ToString(ordno);
                                }
                            }
                            DbContext.SaveChanges();
                        }
                        else
                        {
                            crt.Order_ID = crt.Cart_ID;
                            DateTime today = DateTime.Now.Date;
                            var ordid = crt.Order_ID;
                            if (ordid == null)
                            {
                                crt.Order_No = "DGCRT" + "/" + today.ToString("yyMMdd") + "/" + "00000" + 1;
                            }
                            else
                            {
                                int ordno = Convert.ToInt32(crt.Order_ID);
                                if (ordno > 0 && ordno <= 9)
                                {
                                    crt.Order_No = "DGCRT" + "/" + today.ToString("yyMMdd") + "/" + "00000" + Convert.ToString(ordno);
                                }
                                if (ordno > 9 && ordno <= 99)
                                {
                                    crt.Order_No = "DGCRT" + "/" + today.ToString("yyMMdd") + "/" + "0000" + Convert.ToString(ordno);
                                }
                                if (ordno > 99 && ordno <= 999)
                                {
                                    crt.Order_No = "DGCRT" + "/" + today.ToString("yyMMdd") + "/" + "000" + Convert.ToString(ordno);
                                }
                                if (ordno > 999 && ordno <= 9999)
                                {
                                    crt.Order_No = "DGCRT" + "/" + today.ToString("yyMMdd") + "/" + "00" + Convert.ToString(ordno);
                                }
                                if (ordno > 9999 && ordno <= 99999)
                                {
                                    crt.Order_No = "DGCRT" + "/" + today.ToString("yyMMdd") + "/" + "0" + Convert.ToString(ordno);
                                }
                            }
                            DbContext.SaveChanges();
                        }

                        for (int i = 0; i < data1.Count; i++)
                        {
                            int chp_id = Convert.ToInt32(data1[i]);

                            var data2 = (from a in DbContext.tbl_DC_Chapter.Where(x => x.Chapter_Id == chp_id && x.Is_Active == true && x.Is_Deleted == false)
                                         join b in DbContext.tbl_DC_Subject.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                         on a.Subject_Id equals b.Subject_Id
                                         join c in DbContext.tbl_DC_Class.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                         on b.Class_Id equals c.Class_Id
                                         join d in DbContext.tbl_DC_Board.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                         on c.Board_Id equals d.Board_Id
                                         select new DigiChampCartModel
                                         {
                                             Board_ID = d.Board_Id,
                                             Class_ID = c.Class_Id,
                                             Subject_ID = b.Subject_Id,
                                             Chapter_ID = a.Chapter_Id
                                         }).FirstOrDefault();
                            tbl_DC_Cart_Dtl obj1 = new tbl_DC_Cart_Dtl();
                            obj1.Cart_ID = crt.Cart_ID;
                            obj1.Package_ID = pkgid;
                            obj1.Board_ID = data2.Board_ID;
                            obj1.Class_ID = data2.Class_ID;
                            obj1.Subject_ID = data2.Subject_ID;
                            obj1.Chapter_ID = data2.Chapter_ID;
                            obj1.Status = true;
                            obj1.Inserted_Date = DateTime.Now;
                            obj1.Inserted_By = HttpContext.User.Identity.Name;
                            obj1.Is_Active = true;
                            obj1.Is_Deleted = false;
                            DbContext.tbl_DC_Cart_Dtl.Add(obj1);
                            DbContext.SaveChanges();
                        }
                    }
                }
                else
                {
                    //TempData["message"] = "Please select any item";
                    message = "error2";
                    return Json(message, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                ex.Message.ToString();
            }
            message = "success";
            return Json(message, JsonRequestBehavior.AllowGet);
            //return RedirectToAction("addtocart");
        }
        [HttpGet]
        public ActionResult ViewTablet(int? id)
        {

            try {
                ViewBag.tab_dtl = DbContext.tbl_DC_Tablet_Purchase.Where(x => x.Tablet_Id == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();

                ViewBag.tab_image = DbContext.tbl_DC_Tablet_Image.Where(x => x.Tablet_Id == id && x.Is_Active == true && x.Is_Deleted == false).ToList();
                ViewBag.tab_desc = DbContext.tbl_DC_Tablet_Technical_Details.Where(x => x.Tablet_Id == id && x.Is_Active == true && x.Is_Deleted == false).ToList();


            } catch
            {


            }
            
            return View();
        }

        #endregion

        #region------------------------------------------------Package Learn------------------------------------------------------------
        public ActionResult PackageLearn(int? id)
        {
            try
            {
                if (Session["USER_NAME"] != null)
                {
                    int stuid = Convert.ToInt32(Convert.ToString(Session["USER_CODE"].ToString()).Substring(1));
                    ViewBag.board = (from sub in DbContext.tbl_DC_Board.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                     select new DigiChampsModel.DigiChampsSubjectModel
                                     {
                                         Board_Id = sub.Board_Id,
                                         Board_Name = sub.Board_Name
                                     }).ToList();
                    if (id == null)
                    {
                        string u_name = Session["USER_NAME"].ToString();
                        var data1 = (from k in DbContext.tbl_DC_Registration.Where(x => x.Mobile == u_name && x.Is_Active == true && x.Is_Deleted == false)
                                     join l in DbContext.tbl_DC_Registration_Dtl.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                     on k.Regd_ID equals l.Regd_ID
                                     join n in DbContext.tbl_DC_Class.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                     on l.Class_ID equals n.Class_Id
                                     join o in DbContext.tbl_DC_Board.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                     on n.Board_Id equals o.Board_Id
                                     select new PackagePreviewModel
                                     {
                                         Regd_ID = k.Regd_ID,
                                         Board_Id = l.Board_ID,
                                         Board_Name = o.Board_Name,
                                         Class_Id = l.Class_ID,
                                         Class_Name = n.Class_Name
                                     }).ToList();
                        if (data1 != null)
                        {
                            int bid = Convert.ToInt32(data1.ToList()[0].Board_Id);
                            int cls_id = Convert.ToInt32(data1.ToList()[0].Class_Id);
                            ViewBag.pkgclassid = cls_id;
                            ViewBag.pkgclass = data1.ToList()[0].Class_Name;
                            ViewBag.pkgboard = data1.ToList()[0].Board_Name;
                            ViewBag.class_name = DbContext.tbl_DC_Class.Where(x => x.Board_Id == bid && x.Is_Active == true && x.Is_Deleted == false).ToList();

                            var subjects = (from d in DbContext.VW_DC_Package_Learn.Where(x => x.Regd_ID == stuid && x.Class_ID == cls_id).GroupBy(x => x.Subject_ID)
                                            select new DigiChampCartModel
                                            {
                                                Order_ID = d.FirstOrDefault().Order_ID,
                                                Subject_ID = d.FirstOrDefault().Subject_ID,
                                                Subject = d.FirstOrDefault().Subject_Name
                                            }).ToList();

                            
                            if(subjects.Count > 0)
                            {
                                ViewBag.subjects = subjects;
                            }
                            else
                            {
                                ViewBag.subjects = null;
                                ViewBag.notsubjects = DbContext.Sp_DC_Getall_Packagelearn(cls_id).ToList();
                                ViewBag.notsubjectss = DbContext.Sp_DC_Getall_Packagelearn(cls_id).Select(x => x.Subject_Id).Distinct().ToList();
                                ViewBag.cls_id = cls_id;
                            }
                        }
                        ViewBag.stu_id = stuid;

                    }
                    else
                    {
                        string u_name = Session["USER_NAME"].ToString();

                        var data1 = (from b in DbContext.tbl_DC_Board.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                     join c in DbContext.tbl_DC_Class.Where(x => x.Is_Active == true && x.Is_Deleted == false && x.Class_Id == id) on b.Board_Id equals c.Board_Id
                                     select new PackagePreviewModel
                                     {
                                         Board_Id = b.Board_Id,
                                         Board_Name = b.Board_Name,
                                         Class_Name = c.Class_Name
                                     }).FirstOrDefault();
                        if (data1 != null)
                        {
                            int bid = Convert.ToInt32(data1.Board_Id);
                            int cls_id = Convert.ToInt32(id);
                            ViewBag.pkgboard = data1.Board_Name;
                            ViewBag.pkgclass = data1.Class_Name;

                            ViewBag.class_name = DbContext.tbl_DC_Class.Where(x => x.Board_Id == bid && x.Is_Active == true && x.Is_Deleted == false).ToList();

                            var subjects = (from d in DbContext.VW_DC_Package_Learn.Where(x => x.Regd_ID == stuid && x.Class_ID == id).GroupBy(x => x.Subject_ID)
                                            select new DigiChampCartModel
                                            {
                                                Order_ID = d.FirstOrDefault().Order_ID,
                                                Subject_ID = d.FirstOrDefault().Subject_ID,
                                                Subject = d.FirstOrDefault().Subject_Name
                                            }).ToList();

                            if (subjects.Count > 0)
                            {
                                ViewBag.subjects = subjects;
                            }
                            else
                            {
                                ViewBag.subjects = null;
                                ViewBag.notsubjectss = DbContext.Sp_DC_Getall_Packagelearn(cls_id).Select(x => x.Subject_Id).Distinct().ToList();
                                ViewBag.cls_id = cls_id;
                                ViewBag.notsubjects = DbContext.Sp_DC_Getall_Packagelearn(cls_id).ToList();
                            }
                        }
                        ViewBag.stu_id = stuid;

                    }
                }
                else
                {
                    return RedirectToAction("logout", "student");
                }
            }
            catch (Exception ex)
            {
                ex.Message.ToString();
            }
            return View();
        }
        #endregion

        #region------------------------------------------------Chapter Details----------------------------------------------------------
        //[HttpGet]
        //public ActionResult ChapterDetails(int? id, int? eid)
        //{
        //    try
        //    {
        //        if (Session["USER_NAME"] != null)
        //        {
        //            try
        //            {
        //                var stu_id = Convert.ToInt32(Convert.ToString(Session["USER_CODE"].ToString()).Substring(1));
        //                ViewBag.stu_id = stu_id;
        //                var subjects = (from d in DbContext.VW_DC_Package_Learn.Where(x => x.Regd_ID == stu_id).GroupBy(x => x.Subject_ID)
        //                                select new DigiChampCartModel
        //                                {
        //                                    Order_ID = d.FirstOrDefault().Order_ID,
        //                                    Subject_ID = d.FirstOrDefault().Subject_ID,
        //                                    Subject = d.FirstOrDefault().Subject_Name,
        //                                    Board_ID = d.FirstOrDefault().Board_ID,
        //                                }).ToList();
        //                if(eid!=null)
        //                {
        //                   ViewBag.orderid = eid;
        //                }
        //                if (subjects.Count == 0)
        //                {
        //                   var module_data = DbContext.tbl_DC_Module.Where(x => x.Chapter_Id == id && x.Is_Active == true && x.Is_Deleted == false  && x.Module_video != null).ToList();
        //                   ViewBag.questionpdf = module_data;
        //                   ViewBag.all_video = module_data;
        //                  ViewBag.all_pdf = module_data;
        //                  ViewBag.orderid = null;
        //                  ViewBag.all_test = DbContext.tbl_DC_Exam.Where(x => x.Chapter_Id == id).ToList();
        //                    return View();
        //                }
        //                ViewBag.all_test = DbContext.tbl_DC_Exam.Where(x => x.Chapter_Id == id).ToList();

        //                var exp_dt = DbContext.tbl_DC_Order_Pkg.Where(x => x.Order_ID == eid).Select(x => x.Expiry_Date).Max();
        //                DateTime ex_dt_pkg = Convert.ToDateTime(exp_dt);
        //                if (ex_dt_pkg.Date > today)
        //                {
        //                    ViewBag.chapter_name = DbContext.tbl_DC_Chapter.Where(x => x.Chapter_Id == id).Select(x => x.Chapter).FirstOrDefault();
        //                    var module_data = DbContext.tbl_DC_Module.Where(x => x.Chapter_Id == id && x.Is_Active == true && x.Is_Deleted == false && x.Module_video != null).ToList();
        //                    ViewBag.all_video = module_data;
        //                    ViewBag.all_pdf = module_data;
        //                    ViewBag.expire = null;
        //                }
        //                else
        //                {
        //                    ViewBag.all_video = null;//Package Expired
        //                }
        //            }
        //            catch (Exception)
        //            {
        //                return RedirectToAction("Logout", "Student");
        //            }
        //        }
        //        else
        //        {
        //            return RedirectToAction("Logout", "Student");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return RedirectToAction("Logout", "Student");
        //    }
        //    return View();
        //}

        [HttpGet]
        public ActionResult ChapterDetails(int? id, int? eid)
        {
            try

            {
                if (Session["USER_NAME"] != null)
                {
                    try
                    {
                        var stu_id = Convert.ToInt32(Convert.ToString(Session["USER_CODE"].ToString()).Substring(1));
                        ViewBag.stu_id = stu_id;
                        var subjects = (from d in DbContext.VW_DC_Package_Learn.Where(x => x.Regd_ID == stu_id).GroupBy(x => x.Subject_ID)
                                        select new DigiChampCartModel
                                        {
                                            Order_ID = d.FirstOrDefault().Order_ID,
                                            Subject_ID = d.FirstOrDefault().Subject_ID,
                                            Subject = d.FirstOrDefault().Subject_Name,
                                            Board_ID = d.FirstOrDefault().Board_ID,
                                        }).ToList();
                        if (eid != null)
                        {
                            ViewBag.orderid = eid;
                        }
                        var module_data1 = DbContext.tbl_DC_Module.Where(x => x.Chapter_Id == id && x.Is_Active == true && x.Is_Deleted == false && x.Module_video != null).ToList();
                        if (subjects.Count == 0)
                        {
                            string api_secret = "d87d725cc2f9c4d0ae4fbf956e797089bdbcac7929143d478e9c60bb0d98629c";





                            foreach (var item in module_data1)
                            {
                                var module_data = DbContext.tbl_DC_Module.Where(x => x.Is_Active == true && x.Is_Deleted == false && x.Module_video == item.Module_video).FirstOrDefault();
                                try
                                {

                                    string uri = "http://api.vdocipher.com/v2/video?id=" + item.Module_video;
                                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                                    request.Method = "POST";
                                    request.ContentType = "application/x-www-form-urlencoded";
                                    using (StreamWriter writer = new StreamWriter(request.GetRequestStream(), Encoding.ASCII))
                                    {
                                        writer.Write("clientSecretKey=" + api_secret);
                                    }
                                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                                    dynamic otp_data;
                                    dynamic otp;
                                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                                    {
                                        string json_otp = reader.ReadToEnd();
                                        JObject jResults = JObject.Parse(json_otp);
                                        JToken jResults_bank = jResults["posters"].First["url"]; ;

                                        string data = jResults_bank.ToString();
                                        // var userList = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(json);

                                        //var offeritem = userList.ToList()[0].Value;
                                        module_data.Module_Image = data;



                                        DbContext.SaveChanges();



                                    }
                                }
                                catch (Exception)
                                {


                                }

                            }
                            var module_data2 = DbContext.tbl_DC_Module.Where(x => x.Chapter_Id == id && x.Is_Active == true && x.Is_Deleted == false && x.Module_video != null).ToList();
                            ViewBag.questionpdf = module_data2;
                            ViewBag.all_video = module_data2;
                            ViewBag.all_pdf = module_data2;
                            ViewBag.orderid = null;
                            ViewBag.all_test = DbContext.tbl_DC_Exam.Where(x => x.Chapter_Id == id && x.Is_Active == true).ToList();
                            ViewBag.Free_test = DbContext.tbl_DC_Exam.Where(x => x.Is_Active == true && x.Exam_type == 6).ToList();
                            ViewBag.prereqtest = DbContext.SP_ExamList(stu_id, 1).Select(x => x.Subject_Id).Distinct().ToList();

                            return View();

                        }
                        ViewBag.prereqtest = DbContext.SP_ExamList(stu_id, 1).Select(x => x.Subject_Id).Distinct().ToList();

                        ViewBag.all_test = DbContext.tbl_DC_Exam.Where(x => x.Chapter_Id == id).ToList();

                        var exp_dt = DbContext.tbl_DC_Order_Pkg.Where(x => x.Order_ID == eid).Select(x => x.Expiry_Date).Max();
                        DateTime ex_dt_pkg = Convert.ToDateTime(exp_dt);
                        if (ex_dt_pkg.Date > today)
                        {
                            string api_secret = "d87d725cc2f9c4d0ae4fbf956e797089bdbcac7929143d478e9c60bb0d98629c";





                            foreach (var item in module_data1)
                            {
                                if (item.Module_Image == null)
                                {
                                    var module_data3 = DbContext.tbl_DC_Module.Where(x => x.Is_Active == true && x.Is_Deleted == false && x.Module_video == item.Module_video).FirstOrDefault();
                                    if (module_data3 != null)
                                    {
                                        try
                                        {
                                            string uri = "http://api.vdocipher.com/v2/video?id=" + item.Module_video;
                                            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                                            request.Method = "POST";
                                            request.ContentType = "application/x-www-form-urlencoded";
                                            using (StreamWriter writer = new StreamWriter(request.GetRequestStream(), Encoding.ASCII))
                                            {
                                                writer.Write("clientSecretKey=" + api_secret);
                                            }
                                            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                                            dynamic otp_data;
                                            dynamic otp;
                                            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                                            {
                                                string json_otp = reader.ReadToEnd();
                                                JObject jResults = JObject.Parse(json_otp);
                                                JToken jResults_bank = jResults["posters"].First["url"]; ;

                                                string data = jResults_bank.ToString();
                                                // var userList = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(json);

                                                //var offeritem = userList.ToList()[0].Value;
                                                module_data3.Module_Image = data;



                                                DbContext.SaveChanges();



                                            }
                                        }
                                        catch (Exception)
                                        {


                                        }
                                    }
                                }
                            }


                            ViewBag.chapter_name = DbContext.tbl_DC_Chapter.Where(x => x.Chapter_Id == id).Select(x => x.Chapter).FirstOrDefault();
                            var module_data = DbContext.tbl_DC_Module.Where(x => x.Chapter_Id == id && x.Is_Active == true && x.Is_Deleted == false && x.Module_video != null).ToList();
                            ViewBag.all_video = module_data;
                            ViewBag.all_pdf = module_data;
                            ViewBag.questionpdf = module_data;
                            ViewBag.expire = null;
                        }
                        else
                        {
                            ViewBag.all_video = null;//Package Expired
                        }
                    }

                    catch (Exception)
                    {
                        return RedirectToAction("Logout", "Student");
                    }
                }
                else
                {
                    return RedirectToAction("Logout", "Student");
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("Logout", "Student");
            }
            return View();
        }

        #endregion

        #region--------------------------------------------------Video Details----------------------------------------------------------

        //public ActionResult VideoDetail(string id)
        //{
        //    try
        //    {
        //        if (Session["USER_NAME"] != null)
        //        {
        //            //if (checklogin(Session["USER_NAME"]) == "i")
        //            //{

        //            BotR.API.BotRAPI ab = new BotR.API.BotRAPI("rhhqK8sD", "M8sVGyMY0tpIftRBJ2PPVuzz");
        //            //string time = getUnixTime().ToString();
        //            //ViewBag.exp = getUnixTime().ToString();
        //            //ViewBag.signature = signArgs(time);
        //            NameValueCollection shw = new NameValueCollection(){

        //            {"video_key",id}
        //             };
        //            string xml1 = ab.Call("/videos/show", shw);

        //            //XDocument xml = XDocument.Parse(xml1);
        //            string url = ab.getsignedurl("videos/" + id + "-ozl7iD1S" + ".mp4", 60, "content.jwplatform.com", "M8sVGyMY0tpIftRBJ2PPVuzz");
        //            //}
        //            //else
        //            //{
        //            //    return RedirectToAction("Logout");
        //            //}
        //            ViewBag.url = url;
        //            var data = (from a in DbContext.tbl_DC_Module.Where(x => x.Module_video == id)
        //                        select new DigiChampCartModel
        //                        {
        //                            Module_ID = a.Module_ID,
        //                            Module_Name = a.Module_Name,
        //                            Chapter_ID = a.Chapter_Id,
        //                            Class_ID = a.Class_Id,
        //                            Board_ID = a.Board_Id,
        //                        }).FirstOrDefault();

        //            int clsid = Convert.ToInt32(data.Class_ID);
        //            int stuid = Convert.ToInt32(Convert.ToString(Session["USER_CODE"].ToString()).Substring(1));
        //            var subjects = (from d in DbContext.VW_DC_Package_Learn.Where(x => x.Regd_ID == stuid && x.Class_ID == clsid).GroupBy(x => x.Subject_ID)
        //                            select new DigiChampCartModel
        //                            {
        //                                Order_ID = d.FirstOrDefault().Order_ID,
        //                                Subject_ID = d.FirstOrDefault().Subject_ID,
        //                                Subject = d.FirstOrDefault().Subject_Name,
        //                                Board_ID = d.FirstOrDefault().Board_ID,
        //                            }).ToList();
        //            if (subjects.Count == 0)
        //            {
        //                var classname = DbContext.tbl_DC_Class.Where(x => x.Class_Id == clsid).FirstOrDefault();
        //                int bid = classname.Board_Id;
        //                var module_data1 = (from a in DbContext.tbl_DC_Module.Where(x => x.Class_Id == clsid && x.Board_Id == bid && x.Is_Active == true && x.Is_Deleted == false && x.Module_Content != null && x.Module_video != null && x.Is_Free == true)
        //                                    join b in DbContext.tbl_DC_Chapter on a.Chapter_Id equals b.Chapter_Id
        //                                    select new DigiChampsModel.DigiChampsModuleModel
        //                                    {
        //                                        Chapter = b.Chapter,
        //                                        Module_Name = a.Module_Name,
        //                                        Module_video = a.Module_video,
        //                                        Module_Image = a.Module_Image
        //                                    }).ToList();
        //                ViewBag.all_video = module_data1;
        //                ViewBag.all_pdf = module_data1;


        //                return View();
        //            }

        //            var obj = DbContext.tbl_DC_Module.Where(x => x.Module_video == id).FirstOrDefault();
        //            ViewBag.video = obj.Module_video;
        //            ViewBag.module_name = obj.Module_Name;
        //            ViewBag.module_desc = obj.Module_Desc;


        //            int id1 = Convert.ToInt32(data.Chapter_ID);
        //            var module_data = (from a in DbContext.tbl_DC_Module.Where(x => x.Chapter_Id == id1 && x.Is_Active == true && x.Is_Deleted == false && x.Module_Content != null && x.Module_video != null)
        //                               join b in DbContext.tbl_DC_Chapter on a.Chapter_Id equals b.Chapter_Id
        //                               select new DigiChampsModel.DigiChampsModuleModel
        //                               {
        //                                   Chapter = b.Chapter,
        //                                   Module_Name = a.Module_Name,
        //                                   Module_video = a.Module_video,
        //                                   Module_Image = a.Module_Image
        //                               }
        //            ).ToList();
        //            ViewBag.all_video = module_data;
        //            ViewBag.module = data;
        //            //    BotR.API.BotRAPI ab = new BotR.API.BotRAPI("rhhqK8sD", "M8sVGyMY0tpIftRBJ2PPVuzz");
        //            //    //string time = getUnixTime().ToString();
        //            //    //ViewBag.exp = getUnixTime().ToString();
        //            //    //ViewBag.signature = signArgs(time);
        //            //    NameValueCollection shw = new NameValueCollection(){

        //            //    {"video_key",id}
        //            //};
        //            //    string xml1 = ab.Call("/videos/show", shw);

        //            //    //XDocument xml = XDocument.Parse(xml1);
        //            //    string url = ab.getsignedurl("videos/" + id + "-ozl7iD1S" + ".mp4", 60, "content.jwplatform.com", "M8sVGyMY0tpIftRBJ2PPVuzz");
        //            //    //}
        //            //    //else
        //            //    //{
        //            //    //    return RedirectToAction("Logout");
        //            //    //}
        //            //    ViewBag.url = url;
        //        }
        //        else
        //        {
        //            return RedirectToAction("Logout");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ex.Message.ToString();
        //    }
        //    return View();
        //}
        public ActionResult VideoDetail(string id)
        {
            try
            {
                if (Session["USER_NAME"] != null)
                {
                    //if (checklogin(Session["USER_NAME"]) == "i")
                    //{

                    string video_id = id;          // This should be obtained from DB
                    string api_secret = "d87d725cc2f9c4d0ae4fbf956e797089bdbcac7929143d478e9c60bb0d98629c";
                    string uri = "https://api.vdocipher.com/v2/otp/?video=" + video_id;
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                    request.Method = "POST";
                    request.ContentType = "application/x-www-form-urlencoded";
                    using (StreamWriter writer = new StreamWriter(request.GetRequestStream(), Encoding.ASCII))
                    {
                        writer.Write("clientSecretKey=" + api_secret);
                    }
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    dynamic otp_data;
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        string json_otp = reader.ReadToEnd();
                        otp_data = JObject.Parse(json_otp);
                    }
                    ViewBag.otp = otp_data.otp;
                    var data = (from a in DbContext.tbl_DC_Module.Where(x => x.Module_video == id)
                                select new DigiChampCartModel
                                {
                                    Module_ID = a.Module_ID,
                                    Module_Name = a.Module_Name,
                                    Chapter_ID = a.Chapter_Id,
                                    Class_ID = a.Class_Id,
                                    Board_ID = a.Board_Id,
                                }).FirstOrDefault();

                    int clsid = Convert.ToInt32(data.Class_ID);
                    int stuid = Convert.ToInt32(Convert.ToString(Session["USER_CODE"].ToString()).Substring(1));
                    var subjects = (from d in DbContext.VW_DC_Package_Learn.Where(x => x.Regd_ID == stuid && x.Class_ID == clsid).GroupBy(x => x.Subject_ID)
                                    select new DigiChampCartModel
                                    {
                                        Order_ID = d.FirstOrDefault().Order_ID,
                                        Subject_ID = d.FirstOrDefault().Subject_ID,
                                        Subject = d.FirstOrDefault().Subject_Name,
                                        Board_ID = d.FirstOrDefault().Board_ID,
                                    }).ToList();
                    if (subjects.Count == 0)
                    {
                        var classname = DbContext.tbl_DC_Class.Where(x => x.Class_Id == clsid).FirstOrDefault();
                        int bid = classname.Board_Id;
                        var module_data1 = (from a in DbContext.tbl_DC_Module.Where(x => x.Class_Id == clsid && x.Board_Id == bid && x.Is_Active == true && x.Is_Deleted == false && x.Module_Content != null && x.Module_video != null && x.Is_Free == true)
                                            join b in DbContext.tbl_DC_Chapter on a.Chapter_Id equals b.Chapter_Id
                                            select new DigiChampsModel.DigiChampsModuleModel
                                            {
                                                Chapter = b.Chapter,
                                                Module_Name = a.Module_Name,
                                                Module_video = a.Module_video,
                                                Module_Image = a.Module_Image,
                                                Is_Free=a.Is_Free
                                            }).ToList();

                       





                        foreach (var item in module_data1)
                        {
                            if (item.Module_Image == null)
                            {
                                var module_data3 = DbContext.tbl_DC_Module.Where(x => x.Is_Active == true && x.Is_Deleted == false && x.Module_video == item.Module_video).FirstOrDefault();
                                if (module_data3 != null)
                                {
                                    string uri1 = "http://api.vdocipher.com/v2/video?id=" + item.Module_video;
                                    HttpWebRequest request1 = (HttpWebRequest)WebRequest.Create(uri1);
                                    request1.Method = "POST";
                                    request1.ContentType = "application/x-www-form-urlencoded";
                                    using (StreamWriter writer = new StreamWriter(request1.GetRequestStream(), Encoding.ASCII))
                                    {
                                        writer.Write("clientSecretKey=" + api_secret);
                                    }
                                    HttpWebResponse response1 = (HttpWebResponse)request1.GetResponse();
                                    dynamic otp_data1;
                                    dynamic otp;
                                    using (StreamReader reader = new StreamReader(response1.GetResponseStream()))
                                    {
                                        string json_otp = reader.ReadToEnd();
                                        JObject jResults = JObject.Parse(json_otp);
                                        JToken jResults_bank = jResults["posters"].First["url"]; ;

                                        string data1 = jResults_bank.ToString();
                                        // var userList = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(json);

                                        //var offeritem = userList.ToList()[0].Value;
                                        module_data3.Module_Image = data1;



                                        DbContext.SaveChanges();



                                    }
                                }
                            }
                        }
                        ViewBag.all_video1 = module_data1;
                        ViewBag.all_pdf1 = module_data1;


                        return View();
                    }

                    var obj = DbContext.tbl_DC_Module.Where(x => x.Module_video == id).FirstOrDefault();
                    ViewBag.video = obj.Module_video;
                    ViewBag.module_name = obj.Module_Name;
                    ViewBag.module_desc = obj.Module_Desc;


                    int id1 = Convert.ToInt32(data.Chapter_ID);
                    var module_data = (from a in DbContext.tbl_DC_Module.Where(x => x.Chapter_Id == id1 && x.Is_Active == true && x.Is_Deleted == false && x.Module_Content != null && x.Module_video != null)
                                       join b in DbContext.tbl_DC_Chapter on a.Chapter_Id equals b.Chapter_Id
                                       select new DigiChampsModel.DigiChampsModuleModel
                                       {
                                           Chapter = b.Chapter,
                                           Module_Name = a.Module_Name,
                                           Module_video = a.Module_video,
                                           Module_Image = a.Module_Image
                                       }
                    ).ToList();
                    ViewBag.all_video = module_data;
                    ViewBag.module = data;
                    //    BotR.API.BotRAPI ab = new BotR.API.BotRAPI("rhhqK8sD", "M8sVGyMY0tpIftRBJ2PPVuzz");
                    //    //string time = getUnixTime().ToString();
                    //    //ViewBag.exp = getUnixTime().ToString();
                    //    //ViewBag.signature = signArgs(time);
                    //    NameValueCollection shw = new NameValueCollection(){

                    //    {"video_key",id}
                    //};
                    //    string xml1 = ab.Call("/videos/show", shw);

                    //    //XDocument xml = XDocument.Parse(xml1);
                    //    string url = ab.getsignedurl("videos/" + id + "-ozl7iD1S" + ".mp4", 60, "content.jwplatform.com", "M8sVGyMY0tpIftRBJ2PPVuzz");
                    //    //}
                    //    //else
                    //    //{
                    //    //    return RedirectToAction("Logout");
                    //    //}
                    //    ViewBag.url = url;
                }
                else
                {
                    return RedirectToAction("Logout");
                }
            }
            catch (Exception ex)
            {
                ex.Message.ToString();
            }
            return View();
        }
        [HttpPost]
        public ActionResult Show_video(string video_key)
        {

            BotR.API.BotRAPI ab = new BotR.API.BotRAPI("rhhqK8sD", "M8sVGyMY0tpIftRBJ2PPVuzz");
            NameValueCollection shw = new NameValueCollection(){
                    {"video_key",video_key}
                   
                };
            string xml1 = ab.Call("/videos/show", shw);

            //XDocument xml = XDocument.Parse(xml1);
            string url = ab.getsignedurl("videos/" + video_key + "-ozl7iD1S" + ".mp4", 60, "content.jwplatform.com", "M8sVGyMY0tpIftRBJ2PPVuzz"); ;
            ViewBag.url = url;

            return Json(url, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region--------------------------------------------------Cart Detail------------------------------------------------------------

        public ActionResult addtocart()
        {
            try
            {
                if (Session["USER_NAME"] != null)
                {
                    string u_name = Session["USER_NAME"].ToString();
                    var data = (from a in DbContext.tbl_DC_Registration.Where(x => x.Mobile == u_name && x.Is_Active == true && x.Is_Deleted == false)
                                join b in DbContext.tbl_DC_Cart.Where(x => x.In_Cart == true && x.Is_Paid == false && x.Is_Active == true && x.Is_Deleted == false)
                                 on a.Regd_ID equals b.Regd_ID
                                join c in DbContext.tbl_DC_Package.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                    on b.Package_ID equals c.Package_ID
                                select new DigiChampCartModel
                                {
                                    Package_Name = c.Package_Name,
                                    Subscripttion_Period = c.Subscripttion_Period,
                                    Package_ID = c.Package_ID,
                                    Price = c.Price,
                                    Is_Offline = c.Is_Offline,
                                    Cart_ID = b.Cart_ID
                                }).ToList();
                    //ViewBag.cartitems = data;

                    if (data.Count == 0)
                    {

                        ViewBag.cartitems = null;
                        TempData["emptycart"] = "Your cart is empty";
                    }
                    else
                    {
                        ViewBag.cartitems = data;
                        decimal tax = taxcalculate();
                        ViewBag.taxper = tax;
                        TempData["totalitem"] = Convert.ToInt32(data.Count);
                        decimal totalprice1 = data.ToList().Where(x => x.Is_Offline == false || x.Is_Offline == null).Select(c => (decimal)c.Price).Sum();

                        var data1 = (from x in data.ToList().Where(x => x.Is_Offline == true)
                                     join y in DbContext.tbl_DC_Package_Period.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                     on x.Package_ID equals y.Package_ID
                                     select new DigiChampCartModel
                                     {
                                         Is_Offline = x.Is_Offline,
                                         Package_ID = x.Package_ID,
                                         Price = x.Price,
                                         Excluded_Price = y.Excluded_Price
                                     }).ToList();
                        if (data1.Count > 0)
                        {
                            decimal totalprice2 = data1.ToList().Where(x => x.Is_Offline == true).Select(c => (decimal)c.Price).Sum();
                            decimal totalprice3 = data1.ToList().Where(x => x.Is_Offline == true).Select(c => (decimal)c.Excluded_Price).Sum();
                            decimal totalpriceee = Convert.ToDecimal(totalprice1 + totalprice2);
                            decimal totalprice = Convert.ToDecimal(totalprice1 + totalprice2 + totalprice3);
                            //Session["price2"] = Math.Round(totalprice, 2);
                            decimal totalpaybletax = (tax * totalprice) / 100;
                            decimal totalpayblamt = Math.Round((totalprice + totalpaybletax), 2);
                            var price = DbContext.tbl_DC_CouponCode.Where(x => x.Is_Default == true && x.Valid_From <= today && x.Valid_To >= today && x.Is_Active == true && x.Is_Delete == false).FirstOrDefault();
                            //var price = DbContext.tbl_DC_CouponCode.Where(x => x.Is_Default == true).FirstOrDefault();
                            if (price != null)
                            {
                                if (price.Discount_Price != null)
                                {
                                    int totl_pkg = Convert.ToInt32(data.ToList().Count);
                                    //TempData["copname"] = price.Coupon_Code;
                                    TempData["discountprice"] = Convert.ToDecimal(price.Discount_Price);
                                    decimal prc = Convert.ToDecimal(TempData["discountprice"]);

                                    decimal priced = Math.Round(totalpriceee, 2);
                                    decimal priceddisc = Math.Round((prc * totl_pkg), 2);
                                    TempData["price"] = Math.Round((priced - priceddisc + totalprice3), 2);

                                    Session["price1"] = Convert.ToDecimal(TempData["price"]);
                                    decimal totalpaybletax1 = Math.Round(tax * Convert.ToDecimal(TempData["price"])) / 100;
                                    decimal totalpayblamt1 = Math.Round((Convert.ToDecimal(TempData["price"]) + totalpaybletax1), 2);
                                    TempData["payblamt"] = Math.Round(totalpayblamt1, 2);
                                    Session["payamount"] = totalpayblamt1;
                                    TempData["Tax"] = Math.Round((Convert.ToDecimal(TempData["payblamt"]) - Convert.ToDecimal(TempData["price"])), 2);
                                }
                                else
                                {
                                    int totl_pkg = Convert.ToInt32(data.ToList().Count);
                                    //TempData["copname"] = price.Coupon_Code;
                                    TempData["discountprice1"] = Convert.ToDecimal(price.Discount_Percent);
                                    decimal prc = Convert.ToDecimal(TempData["discountprice1"]);
                                    decimal t_price = Convert.ToDecimal((Math.Round(totalpriceee, 2) * prc) / 100);

                                    decimal priced = Math.Round(totalpriceee, 2);
                                    decimal priceddisc = Math.Round((t_price * totl_pkg), 2);
                                    TempData["price"] = Math.Round((priced - t_price + totalprice3), 2);

                                    Session["price1"] = Convert.ToDecimal(TempData["price"]);
                                    decimal totalpaybletax1 = (tax * Convert.ToDecimal(TempData["price"])) / 100;
                                    decimal totalpayblamt1 = Math.Round((Convert.ToDecimal(TempData["price"]) + totalpaybletax1), 2);
                                    TempData["payblamt"] = Math.Round(totalpayblamt1, 2); ;
                                    Session["payamount"] = totalpayblamt1;
                                    TempData["Tax"] = Math.Round((Convert.ToDecimal(TempData["payblamt"]) - Convert.ToDecimal(TempData["price"])), 2);
                                }
                            }
                            else
                            {
                                //TempData["discountprice"] = Convert.ToDecimal(price.Discount_Price);
                                TempData["payblamt"] = totalpayblamt;
                                Session["payamount"] = totalpayblamt;
                                TempData["price"] = Math.Round(totalprice, 2);
                                Session["price1"] = Convert.ToDecimal(TempData["price"]);
                                TempData["Tax"] = Math.Round((Convert.ToDecimal(TempData["payblamt"]) - Convert.ToDecimal(TempData["price"])), 2);
                            }
                        }
                        else
                        {
                            decimal totalprice = Convert.ToDecimal(totalprice1);
                            decimal totalpaybletax = (tax * totalprice) / 100;
                            decimal totalpayblamt = Math.Round((totalprice + totalpaybletax), 2);
                            var price = DbContext.tbl_DC_CouponCode.Where(x => x.Is_Default == true && x.Valid_From <= today && x.Valid_To >= today && x.Is_Active == true && x.Is_Delete == false).FirstOrDefault();
                            if (price != null)
                            {
                                if (price.Discount_Price != null)
                                {
                                    int totl_pkg = Convert.ToInt32(data.ToList().Count);
                                    //TempData["copname"] = price.Coupon_Code;
                                    TempData["discountprice"] = Convert.ToDecimal(price.Discount_Price);
                                    decimal prc = Convert.ToDecimal(TempData["discountprice"]);

                                    decimal priced = Math.Round(totalprice, 2);
                                    decimal priceddisc = Math.Round((prc * totl_pkg), 2);
                                    TempData["price"] = Math.Round((priced - priceddisc), 2);

                                    Session["price1"] = Convert.ToDecimal(TempData["price"]);
                                    decimal totalpaybletax1 = (tax * Convert.ToDecimal(TempData["price"])) / 100;
                                    decimal totalpayblamt1 = Math.Round((Convert.ToDecimal(TempData["price"]) + totalpaybletax1), 2);
                                    TempData["payblamt"] = totalpayblamt1;
                                    Session["payamount"] = totalpayblamt1;
                                    TempData["Tax"] = Math.Round((Convert.ToDecimal(TempData["payblamt"]) - Convert.ToDecimal(TempData["price"])), 2);
                                }
                                else
                                {
                                    int totl_pkg = Convert.ToInt32(data.ToList().Count);
                                    //TempData["copname"] = price.Coupon_Code;
                                    TempData["discountprice1"] = Convert.ToDecimal(price.Discount_Percent);
                                    decimal prc = Convert.ToDecimal(TempData["discountprice1"]);
                                    decimal t_price = Convert.ToDecimal((Math.Round(totalprice, 2) * prc) / 100);

                                    decimal priced = Math.Round(totalprice, 2);
                                    decimal priceddisc = Math.Round((t_price * totl_pkg), 2);
                                    TempData["price"] = Math.Round((priced - t_price), 2);

                                    Session["price1"] = Convert.ToDecimal(TempData["price"]);
                                    decimal totalpaybletax1 = (tax * Convert.ToDecimal(TempData["price"])) / 100;
                                    decimal totalpayblamt1 = Math.Round((Convert.ToDecimal(TempData["price"]) + totalpaybletax1), 2);
                                    TempData["payblamt"] = totalpayblamt1;
                                    Session["payamount"] = totalpayblamt1;
                                    TempData["Tax"] = Math.Round((Convert.ToDecimal(TempData["payblamt"]) - Convert.ToDecimal(TempData["price"])), 2);
                                }
                            }
                            else
                            {
                                //TempData["discountprice"] = Convert.ToDecimal(price.Discount_Price);
                                TempData["payblamt"] = Math.Round(totalpayblamt, 2);
                                Session["payamount"] = totalpayblamt;
                                TempData["price"] = Math.Round(totalprice, 2);
                                Session["price1"] = Convert.ToDecimal(TempData["price"]);
                                TempData["Tax"] = Math.Round((Convert.ToDecimal(TempData["payblamt"]) - Convert.ToDecimal(TempData["price"])), 2);
                            }
                            //TempData["payblamt"] = totalpayblamt;
                            //Session["payamount"] = totalpayblamt;
                            //TempData["price"] = Math.Round(totalprice, 2);
                            //TempData["Tax"] = Convert.ToDecimal(TempData["payblamt"]) - Convert.ToDecimal(TempData["price"]);
                        }
                    }
                }
                else
                {
                    return RedirectToAction("Logout");
                }
            }
            catch (Exception ex)
            {
                TempData["errormessgae"] = "Oops,Something went wrong";
            }
            return View();
        }

        public ActionResult RemoveCartItem(int id)
        {
            try
            {
                if (Session["USER_NAME"] != null)
                {
                    tbl_DC_Cart obj = DbContext.tbl_DC_Cart.Where(x => x.Cart_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    obj.Is_Active = false;
                    obj.Is_Deleted = true;
                    DbContext.Entry(obj).State = EntityState.Modified;
                    DbContext.SaveChanges();

                    tbl_DC_Cart_Dtl obj1 = DbContext.tbl_DC_Cart_Dtl.Where(x => x.Cart_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    obj1.Is_Active = true;
                    obj1.Is_Deleted = false;
                    DbContext.Entry(obj1).State = EntityState.Modified;
                    DbContext.SaveChanges();

                }
                else
                {
                    return RedirectToAction("Logout");
                }
            }
            catch (Exception ex)
            {
                ex.Message.ToString();
            }
            return RedirectToAction("addtocart");
        }

        public JsonResult ClearCart()
        {
            string message = string.Empty;
            try
            {
                if (Session["USER_NAME"] != null)
                {
                    string u_name = Session["USER_NAME"].ToString();
                    var data = (from a in DbContext.tbl_DC_Registration.Where(x => x.Mobile == u_name && x.Is_Active == true && x.Is_Deleted == false)
                                join b in DbContext.tbl_DC_Cart.Where(x => x.In_Cart == true && x.Is_Paid == false && x.Is_Active == true && x.Is_Deleted == false)
                                 on a.Regd_ID equals b.Regd_ID
                                select new DigiChampCartModel
                                {
                                    Cart_ID = b.Cart_ID
                                }).ToList();
                    ViewBag.cartitems = data;

                    foreach (var item in ViewBag.cartitems)
                    {
                        int regid = Convert.ToInt32(item.Cart_ID);

                        tbl_DC_Cart obj = DbContext.tbl_DC_Cart.Where(x => x.Cart_ID == regid).FirstOrDefault();
                        obj.In_Cart = false;
                        obj.Is_Active = false;
                        obj.Is_Deleted = true;
                        obj.Status = false;
                        DbContext.Entry(obj).State = EntityState.Modified;
                        DbContext.SaveChanges();
                    }
                }
                else
                {
                    //return RedirectToAction("Logout");
                }
            }
            catch (Exception ex)
            {
                ex.Message.ToString();
            }
            message = "success";
            return Json(message, JsonRequestBehavior.AllowGet);
            //return Redirect("addtocart");

        }

        public decimal taxcalculate()
        {
            // Get the cart
            var taxcalculate = (from a in DbContext.tbl_DC_Tax_Master.Where(x => x.Is_Active == true && x.Is_Deleted == false && x.TAX_Efect_Date < today)
                                join b in DbContext.tbl_DC_Tax_Type_Master.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                on a.TaxType_ID equals b.TaxType_ID
                                select new DigiChampsModel.TaxModel
                                {
                                    Tax_Rate = a.Tax_Rate
                                }).ToList();

            decimal taxamount = taxcalculate.ToList().Select(c => (decimal)c.Tax_Rate).Sum();
            TempData["taxamount"] = Math.Round(taxamount, 2);
            return taxamount;
        }

        public class PriceCodeModel
        {
            public int Code_Id { get; set; }
            public Nullable<int> Coupon_Type { get; set; }
            public string Coupon_Code { get; set; }
            public Nullable<decimal> Discount_Price { get; set; }
            public Nullable<System.DateTime> Valid_From { get; set; }
            public Nullable<System.DateTime> Valid_To { get; set; }
            public Nullable<decimal> Pricerange_From { get; set; }
            public Nullable<decimal> Pricertange_To { get; set; }
            public Nullable<bool> Is_Default { get; set; }
        }

        [HttpGet]
        public ActionResult checkout(Digichampcartmodel1 dgcoupon, int? Id)
        {

            try
            {
                if (Session["USER_NAME"] != null)
                {
                    string txnid = Generatetxnid();
                    Session["tnid"] = txnid;
                    decimal totalprice = 0;
                    decimal t_price = Convert.ToDecimal(Session["price1"]);
                    var price = DbContext.tbl_DC_CouponCode.Where(x => x.Is_Default == true && x.Valid_From <= today && x.Valid_To >= today && x.Is_Active == true && x.Is_Delete == false).FirstOrDefault();
                    if (dgcoupon.coupon != null)
                    {
                        var price1 = DbContext.tbl_DC_CouponCode.Where(x => x.Coupon_Code == dgcoupon.coupon && x.Valid_From <= today && x.Valid_To >= today && x.Is_Active == true && x.Is_Delete == false).FirstOrDefault();
                        if (price1.Discount_Percent != null)
                        {
                            TempData["discamt1"] = dgcoupon.coupon;
                            TempData["discamt11"] = Math.Round(Convert.ToDecimal(price1.Discount_Percent), 2);
                            Session["percent"] = TempData["discamt11"];
                            string copname = TempData["discamt1"].ToString();
                            var cop_disc = DbContext.tbl_DC_CouponCode.Where(x => x.Coupon_Code == dgcoupon.coupon && x.Is_Active == true && x.Is_Delete == false).FirstOrDefault();
                            if (cop_disc.Discount_Price != null)
                            {
                                Session["disc"] = cop_disc.Discount_Price;
                            }
                            else
                            {
                                Session["disc"] = cop_disc.Discount_Percent;
                            }


                        }
                        else
                        {
                            TempData["discamt"] = dgcoupon.coupon;
                            TempData["discamt11"] = Math.Round(Convert.ToDecimal(price1.Discount_Price), 2);
                            Session["percent1"] = TempData["discamt11"];
                            string copname = TempData["discamt"].ToString();
                            var cop_disc = DbContext.tbl_DC_CouponCode.Where(x => x.Coupon_Code == copname && x.Is_Active == true && x.Is_Delete == false).FirstOrDefault();
                            if (cop_disc.Discount_Price != null)
                            {
                                Session["disc"] = cop_disc.Discount_Price;
                            }
                            else
                            {
                                Session["disc"] = cop_disc.Discount_Percent;
                            }
                        }
                        TempData["c_name"] = dgcoupon.coupon;

                    }
                    else
                    {
                        if (price != null)
                        {
                            if (price.Discount_Percent != null)
                            {
                                TempData["discamt1"] = price.Coupon_Code;
                                TempData["discamt11"] = Math.Round(Convert.ToDecimal(price.Discount_Percent), 2);
                                Session["percent"] = TempData["discamt11"];
                                string copname = TempData["discamt1"].ToString();
                                var cop_disc = DbContext.tbl_DC_CouponCode.Where(x => x.Coupon_Code == copname && x.Is_Active == true && x.Is_Delete == false).FirstOrDefault();
                                if (cop_disc.Discount_Price != null)
                                {
                                    Session["disc"] = cop_disc.Discount_Price;
                                }
                                else
                                {
                                    Session["disc"] = cop_disc.Discount_Percent;
                                }
                            }
                            else
                            {
                                TempData["discamt"] = price.Coupon_Code;
                                TempData["discamt11"] = Math.Round(Convert.ToDecimal(price.Discount_Price), 2);
                                Session["percent1"] = TempData["discamt11"];
                                string copname = TempData["discamt"].ToString();
                                var cop_disc = DbContext.tbl_DC_CouponCode.Where(x => x.Coupon_Code == copname && x.Is_Active == true && x.Is_Delete == false).FirstOrDefault();
                                if (cop_disc != null)
                                {
                                    if (cop_disc.Discount_Price != null)
                                    {
                                        Session["disc"] = cop_disc.Discount_Price;
                                    }
                                    else
                                    {
                                        Session["disc"] = cop_disc.Discount_Percent;
                                    }
                                }
                            }
                            ViewBag.price = price;
                            TempData["c_name"] = price.Coupon_Code;
                        }
                    }
                    //var coupon = (from a in DbContext.tbl_DC_CouponCode.Where(x => x.Pricerange_From <= t_price && x.Pricertange_To >= t_price && x.Is_Default == false && x.Valid_From <= today && x.Valid_To >= today && x.Is_Active == true && x.Is_Delete == false)
                    //              select new PriceCodeModel
                    //              {
                    //                  Coupon_Code = a.Coupon_Code,
                    //                  Code_Id = a.Code_Id
                    //              }).ToList();
                    //if (coupon.Count > 0)
                    //{
                    //    ViewBag.coupon = coupon;
                    //}
                    string u_name = Session["USER_NAME"].ToString();

                    var data1 = DbContext.tbl_DC_Registration.Where(x => x.Mobile == u_name && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();

                    ViewBag.mobile = data1.Mobile;
                    ViewBag.custmer_name = data1.Customer_Name;
                    ViewBag.Email = data1.Email;
                    ViewBag.address = data1.Address;
                   
                    var data = (from a in DbContext.tbl_DC_Registration.Where(x => x.Mobile == u_name && x.Is_Active == true && x.Is_Deleted == false)
                                join b in DbContext.tbl_DC_Cart.Where(x => x.In_Cart == true && x.Is_Paid == false && x.Is_Active == true && x.Is_Deleted == false)
                                 on a.Regd_ID equals b.Regd_ID
                                join c in DbContext.tbl_DC_Package.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                    on b.Package_ID equals c.Package_ID
                                select new DigiChampCartModel
                                {
                                    Regd_ID = a.Regd_ID,
                                    firstname = a.Customer_Name,
                                    phone = a.Mobile,
                                    email = a.Email,
                                    Package_Name = c.Package_Name,
                                    Package_ID = c.Package_ID,
                                    Subscripttion_Period = c.Subscripttion_Period,
                                    Price = c.Price,
                                    Is_Offline = c.Is_Offline,
                                    Order_ID = b.Order_ID,
                                    Order_No = b.Order_No,
                                    Cart_ID = b.Cart_ID
                                }).ToList();
                    int rgid = Convert.ToInt32(data.ToList()[0].Regd_ID);
                    Session["fname"] = data.ToList()[0].firstname;
                    Session["email"] = data.ToList()[0].email;
                    Session["phone"] = data.ToList()[0].phone;

                    if (data.Count == 0)
                    {
                        ViewBag.cartitems = null;
                        TempData["emptycart"] = "Your cart is empty";
                    }
                    else
                    {
                        decimal tax = taxcalculate();
                        decimal totalprice3 = 0;
                        ViewBag.taxper = tax;
                        if (TempData["cartid"] != null)
                        {


                            int ca_id = Convert.ToInt32(TempData["cartid"]);
                            tbl_DC_Cart obj = DbContext.tbl_DC_Cart.Where(x => x.Cart_ID == ca_id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                            obj.Coupon_Code = dgcoupon.coupon;
                            DbContext.Entry(obj).State = EntityState.Modified;
                            DbContext.SaveChanges();
                        }
                        ViewBag.cartitems = data;
                        decimal totalprice2 = data.ToList().Where(x => x.Is_Offline == true).Select(c => (decimal)c.Price).Sum();
                        if (data.Select(x => x.Excluded_Price).Sum() != 0)
                        {
                            totalprice3 = data.ToList().Where(x => x.Is_Offline == true).Select(c => (decimal)c.Excluded_Price).Sum();
                        }

                        else
                        {
                            totalprice3 = 0;

                        }
                        //decimal totalprice = Convert.ToDecimal(totalprice1 + totalprice2 + totalprice3);

                        if (Session["prc"] != null)
                        {
                            totalprice = Convert.ToDecimal(Session["prc"]);
                        }
                        if (Session["price1"] != null)
                        {
                            totalprice = Convert.ToDecimal(Session["price1"]);
                        }
                        else
                        {
                            totalprice = Convert.ToDecimal(Session["payamount1"]);
                        }
                        //decimal totalprice = Convert.ToDecimal(Session["price1"]);
                        decimal totalpaybletax = (tax * totalprice) / 100;
                        decimal totalpayblamt = Math.Round((totalprice + totalpaybletax), 2);
                        TempData["payblamt"] = totalpayblamt;
                        Session["payamount"] = totalpayblamt;

                        TempData["price"] = Math.Round(totalprice, 2);
                        TempData["Tax"] = Convert.ToDecimal(TempData["payblamt"]) - Convert.ToDecimal(TempData["price"]);
                        var amt = DbContext.tbl_DC_Pre_book.Where(x => x.Regd_Id == rgid && x.Ord_Status == false && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        if (amt != null)
                        {
                            decimal pre_amt = Convert.ToDecimal(amt.Amount);
                            TempData["payblamt"] = Convert.ToDecimal(totalpayblamt - pre_amt);
                            TempData["pre_amt"] = Convert.ToDecimal(pre_amt);
                            Session["payamount"] = TempData["payblamt"];
                            TempData["price"] = Math.Round(totalprice, 2);
                            Session["price"] = Math.Round(totalprice, 2);
                            TempData["Tax"] = Math.Round((totalpaybletax), 2);
                        }
                        else
                        {
                            TempData["payblamt"] = totalpayblamt;
                            TempData["pre_amt"] = null;
                            Session["payamount"] = totalpayblamt;
                            TempData["price"] = Math.Round(totalprice, 2);
                            Session["price"] = Math.Round(totalprice, 2);
                            TempData["Tax"] = Math.Round((totalpaybletax), 2);
                        }

                    }


                    if (Session["price"] != null)
                    {
                        Session["price"] = Math.Round(totalprice, 2);
                    }

                    Session["ordid"] = data.ToList()[0].Order_ID;
                    Session["ordno"] = data.ToList()[0].Order_No;
                }


                else
                {
                    return RedirectToAction("Logout");
                }
            }
            catch (Exception ex)
            {
                TempData["errormessgae"] = "Oops,Something went wrong";
            }

            return View();
        }
        [HttpPost]
        public ActionResult checkout(Digichampcartmodel1 dgcoupon)
        {
            try
            {
                if (Session["USER_NAME"] != null)
                {

                    decimal totalprice = 0;
                    decimal t_price = Convert.ToDecimal(Session["price1"]);
                    var price = DbContext.tbl_DC_CouponCode.Where(x => x.Is_Default == true && x.Valid_From <= today && x.Valid_To >= today && x.Is_Active == true && x.Is_Delete == false).FirstOrDefault();
                    if (price != null)
                    {
                        if (price.Discount_Percent != null)
                        {
                            TempData["discamt1"] = Math.Round(Convert.ToDecimal(price.Discount_Percent), 2);
                        }
                        else
                        {
                            TempData["discamt"] = Math.Round(Convert.ToDecimal(price.Discount_Price), 2);
                        }
                        ViewBag.price = price;
                        TempData["c_name"] = price.Coupon_Code;
                    }

                    string u_name = Session["USER_NAME"].ToString();

                    var data1 = DbContext.tbl_DC_Registration.Where(x => x.Mobile == u_name && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();

                    ViewBag.mobile = data1.Mobile;
                    ViewBag.custmer_name = data1.Customer_Name;

                    var data = (from a in DbContext.tbl_DC_Registration.Where(x => x.Mobile == u_name && x.Is_Active == true && x.Is_Deleted == false)
                                join b in DbContext.tbl_DC_Cart.Where(x => x.In_Cart == true && x.Is_Paid == false && x.Is_Active == true && x.Is_Deleted == false)
                                 on a.Regd_ID equals b.Regd_ID
                                join c in DbContext.tbl_DC_Package.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                    on b.Package_ID equals c.Package_ID
                                select new DigiChampCartModel
                                {
                                    Regd_ID = a.Regd_ID,
                                    firstname = a.Customer_Name,
                                    phone = a.Mobile,
                                    email = a.Email,
                                    Package_Name = c.Package_Name,
                                    Package_ID = c.Package_ID,
                                    Subscripttion_Period = c.Subscripttion_Period,
                                    Price = c.Price,
                                    Is_Offline = c.Is_Offline,
                                    Order_ID = b.Order_ID,
                                    Order_No = b.Order_No,
                                    Cart_ID = b.Cart_ID
                                }).ToList();
                    int rgid = Convert.ToInt32(data.ToList()[0].Regd_ID);
                    Session["fname"] = data.ToList()[0].firstname;
                    Session["email"] = data.ToList()[0].email;
                    Session["phone"] = data.ToList()[0].phone;

                    if (data.Count == 0)
                    {
                        ViewBag.cartitems = null;
                        TempData["emptycart"] = "Your cart is empty";
                    }
                    else
                    {
                        if (dgcoupon.coupon != null)
                        {
                            ViewBag.cartitems = data;
                            decimal tax = taxcalculate();
                            ViewBag.taxper = tax;
                            TempData["totalitem"] = Convert.ToInt32(data.Count);
                            decimal totalprice1 = data.ToList().Where(x => x.Is_Offline == false || x.Is_Offline == null).Select(c => (decimal)c.Price).Sum();

                            var data2 = (from x in data.ToList().Where(x => x.Is_Offline == true)
                                         join y in DbContext.tbl_DC_Package_Period.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                         on x.Package_ID equals y.Package_ID
                                         select new DigiChampCartModel
                                         {
                                             Is_Offline = x.Is_Offline,
                                             Package_ID = x.Package_ID,
                                             Price = x.Price,
                                             Excluded_Price = y.Excluded_Price
                                         }).ToList();
                            if (data2.Count > 0)
                            {
                                decimal totalprice2 = data2.ToList().Where(x => x.Is_Offline == true).Select(c => (decimal)c.Price).Sum();
                                decimal totalprice3 = data2.ToList().Where(x => x.Is_Offline == true).Select(c => (decimal)c.Excluded_Price).Sum();
                                //decimal totalprice = Convert.ToDecimal(totalprice1 + totalprice2 + totalprice3);

                                if (Session["prc"] != null)
                                {
                                    totalprice = Convert.ToDecimal(Session["prc"]);
                                }
                                else
                                {
                                    totalprice = Convert.ToDecimal(Session["price1"]);
                                }
                                //decimal totalprice = Convert.ToDecimal(Session["price1"]);
                                decimal totalpaybletax = (tax * totalprice) / 100;
                                decimal totalpayblamt = Math.Round((totalprice + totalpaybletax), 2);
                                TempData["payblamt"] = totalpayblamt;
                                Session["payamount"] = totalpayblamt;

                                TempData["price"] = Math.Round(totalprice, 2);
                                TempData["Tax"] = Convert.ToDecimal(TempData["payblamt"]) - Convert.ToDecimal(TempData["price"]);
                                var amt = DbContext.tbl_DC_Pre_book.Where(x => x.Regd_Id == rgid && x.Ord_Status == false && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                                if (amt != null)
                                {
                                    decimal pre_amt = Convert.ToDecimal(amt.Amount);
                                    TempData["payblamt"] = Convert.ToDecimal(totalpayblamt - pre_amt);
                                    TempData["pre_amt"] = Convert.ToDecimal(pre_amt);
                                    Session["payamount"] = TempData["payblamt"];
                                    TempData["price"] = Math.Round(totalprice, 2);
                                    Session["price"] = Math.Round(totalprice, 2);
                                    TempData["Tax"] = Math.Round((totalpaybletax), 2);
                                }
                                else
                                {
                                    TempData["payblamt"] = totalpayblamt;
                                    TempData["pre_amt"] = null;
                                    Session["payamount"] = totalpayblamt;
                                    TempData["price"] = Math.Round(totalprice, 2);
                                    Session["price"] = Math.Round(totalprice, 2);
                                    TempData["Tax"] = Math.Round((totalpaybletax), 2);
                                }
                                if (dgcoupon.coupon != "" || dgcoupon.coupon != null)
                                {
                                    var cop = DbContext.tbl_DC_CouponCode.Where(x => x.Coupon_Code == dgcoupon.coupon && x.Valid_From <= today && x.Valid_To >= today && x.Is_Active == true && x.Is_Delete == false).FirstOrDefault();
                                    if (cop != null)
                                    {
                                        if (cop.Discount_Price != null)
                                        {
                                            int totl_pkg = Convert.ToInt32(data.ToList().Count);
                                            TempData["c_name"] = cop.Coupon_Code;
                                            decimal paisa = Convert.ToDecimal(cop.Discount_Price);
                                            decimal price_without_exclude = Math.Round((totalprice - totalprice3), 2);

                                            Session["price"] = Math.Round(price_without_exclude, 2);
                                            Session["payamount1"] = Convert.ToDecimal(Convert.ToDecimal((decimal)Session["price"] + totalprice3) - paisa);
                                            Session["price1"] = Convert.ToDecimal(Session["payamount1"]);
                                            TempData["price"] = Math.Round((decimal)Session["payamount1"], 2);
                                            var amount = Math.Round(Convert.ToDecimal(Session["payamount1"]), 2);
                                            totalpaybletax = (tax * (decimal)Session["payamount1"]) / 100;
                                            TempData["payblamt"] = Math.Round((amount + totalpaybletax), 2);
                                            Session["payamount"] = Math.Round((amount + totalpaybletax), 2); ;
                                            TempData["Tax"] = Math.Round(Convert.ToDouble(totalpaybletax), 2);

                                            //TempData["payblamt"] = Math.Round(Convert.ToDecimal((decimal)Session["payamount"]), 2);
                                            TempData["discamt"] = paisa;
                                            TempData["cartid"] = Convert.ToInt32(data.ToList()[0].Cart_ID);
                                            Session["prc"] = null;
                                        }
                                        else
                                        {
                                            int totl_pkg = Convert.ToInt32(data.ToList().Count);
                                            TempData["c_name"] = cop.Coupon_Code;
                                            decimal paisa1 = Convert.ToDecimal(cop.Discount_Percent);
                                            decimal price_without_exclude = Math.Round((totalprice - totalprice3), 2);

                                            Session["price"] = Math.Round(price_without_exclude, 2);
                                            decimal netprice = Convert.ToDecimal((Convert.ToDecimal(price_without_exclude) * paisa1) / 100);
                                            Session["payamount1"] = Convert.ToDecimal(Convert.ToDecimal((decimal)Session["price"] + totalprice3) - netprice);
                                            Session["price1"] = Convert.ToDecimal(Session["payamount1"]);
                                            TempData["price"] = Math.Round((decimal)Session["payamount1"], 2);
                                            var amount = Math.Round(Convert.ToDecimal(Session["payamount1"]), 2);
                                            TempData["discamt1"] = paisa1;
                                            totalpaybletax = (tax * (decimal)Session["payamount1"]) / 100;

                                            TempData["payblamt"] = Math.Round((amount + totalpaybletax), 2); ;
                                            Session["payamount"] = Math.Round((amount + totalpaybletax), 2); ;
                                            TempData["Tax"] = Math.Round(Convert.ToDouble(totalpaybletax), 2);
                                            TempData["cartid"] = Convert.ToInt32(data.ToList()[0].Cart_ID);
                                            Session["prc"] = null;
                                        }
                                    }
                                    else
                                    {
                                        TempData["Message"] = "Coupon has expired";
                                    }
                                }
                                else
                                {
                                    Session["price"] = Math.Round(totalprice, 2);
                                }
                            }
                            else
                            {
                                //decimal totalprice = Convert.ToDecimal(Session["price1"]);

                                if (Session["prc"] != null)
                                {
                                    totalprice = Convert.ToDecimal(Session["prc"]);
                                }
                                else
                                {
                                    totalprice = Convert.ToDecimal(Session["price1"]);
                                }
                                //decimal totalprice = Convert.ToDecimal(totalprice1);
                                decimal totalpaybletax = (tax * totalprice) / 100;
                                decimal totalpayblamt = Math.Round((totalprice + totalpaybletax), 2);
                                TempData["payblamt"] = totalpayblamt;
                                Session["payamount"] = totalpayblamt;
                                TempData["price"] = Math.Round(totalprice, 2);
                                TempData["Tax"] = Convert.ToDecimal(TempData["payblamt"]) - Convert.ToDecimal(TempData["price"]);
                                var amt = DbContext.tbl_DC_Pre_book.Where(x => x.Regd_Id == rgid && x.Ord_Status == false && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                                if (amt != null)
                                {
                                    decimal pre_amt = Convert.ToDecimal(amt.Amount);
                                    TempData["payblamt"] = Convert.ToDecimal(totalpayblamt - pre_amt);
                                    TempData["pre_amt"] = Convert.ToDecimal(pre_amt);
                                    Session["payamount"] = TempData["payblamt"];
                                    TempData["price"] = Math.Round(totalprice, 2);
                                    Session["price"] = Math.Round(totalprice, 2);
                                    TempData["Tax"] = Math.Round((totalpaybletax), 2);
                                }
                                else
                                {
                                    TempData["payblamt"] = totalpayblamt;
                                    TempData["pre_amt"] = null;
                                    Session["payamount"] = totalpayblamt;
                                    Session["price"] = Math.Round(totalprice, 2);
                                    TempData["price"] = Math.Round(totalprice, 2);
                                    TempData["Tax"] = Math.Round((totalpaybletax), 2);
                                }
                                if (dgcoupon.coupon != "" || dgcoupon.coupon != null)
                                {
                                    var cop = DbContext.tbl_DC_CouponCode.Where(x => x.Coupon_Code == dgcoupon.coupon && x.Valid_From <= today && x.Valid_To >= today && x.Is_Active == true && x.Is_Delete == false).FirstOrDefault();
                                    if (cop != null)
                                    {
                                        if (cop.Discount_Price != null)
                                        {
                                            int totl_pkg = Convert.ToInt32(data.ToList().Count);
                                            //TempData["c_name"] = cop.Coupon_Code;
                                            decimal paisa = Convert.ToDecimal(cop.Discount_Price);
                                            //Session["price"] = Math.Round(totalprice, 2);
                                            //Session["payamount"] = Convert.ToDecimal(Convert.ToDecimal(TempData["payblamt"]) - (paisa * totl_pkg));
                                            //TempData["payblamt"] = Math.Round(Convert.ToDecimal(Session["payamount"]), 2);
                                            //TempData["discamt"] = paisa;
                                            //TempData["cartid"] = Convert.ToInt32(data.ToList()[0].Cart_ID);
                                            //Session["prc"] = null;

                                            TempData["c_name"] = cop.Coupon_Code;
                                            decimal price_without_exclude = Math.Round((totalprice), 2);
                                            Session["price"] = Math.Round(price_without_exclude, 2);
                                            Session["payamount1"] = Convert.ToDecimal(Convert.ToDecimal((decimal)Session["price"]) - paisa);
                                            Session["price1"] = Convert.ToDecimal(Session["payamount1"]);
                                            TempData["price"] = Math.Round((decimal)Session["payamount1"], 2);
                                            var amount = Math.Round(Convert.ToDecimal(Session["payamount1"]), 2);
                                            totalpaybletax = (tax * (decimal)Session["payamount1"]) / 100;
                                            TempData["payblamt"] = Math.Round((amount + totalpaybletax), 2); ;
                                            Session["payamount"] = Math.Round((amount + totalpaybletax), 2); ;
                                            TempData["Tax"] = Math.Round(Convert.ToDouble(totalpaybletax), 2);

                                            //TempData["payblamt"] = Math.Round(Convert.ToDecimal((decimal)Session["payamount"]), 2);
                                            TempData["discamt"] = paisa;
                                            TempData["cartid"] = Convert.ToInt32(data.ToList()[0].Cart_ID);
                                            Session["prc"] = null;
                                        }
                                        else
                                        {
                                            //int totl_pkg = Convert.ToInt32(data.ToList().Count);
                                            //TempData["c_name"] = cop.Coupon_Code;
                                            //decimal paisa1 = Convert.ToDecimal(cop.Discount_Percent);
                                            //Session["price"] = Math.Round(totalprice, 2);
                                            //decimal netprice = Convert.ToDecimal((Convert.ToDecimal(TempData["payblamt"]) * paisa1) / 100);
                                            //Session["payamount"] = Convert.ToDecimal(Convert.ToDecimal(TempData["payblamt"]) - (netprice * totl_pkg));
                                            //TempData["payblamt"] = Math.Round(Convert.ToDecimal(Session["payamount"]), 2);
                                            //TempData["discamt1"] = paisa1;
                                            //TempData["cartid"] = Convert.ToInt32(data.ToList()[0].Cart_ID);
                                            //Session["prc"] = null;

                                            int totl_pkg = Convert.ToInt32(data.ToList().Count);
                                            TempData["c_name"] = cop.Coupon_Code;
                                            decimal paisa1 = Convert.ToDecimal(cop.Discount_Percent);
                                            decimal price_without_exclude = Math.Round((totalprice), 2);

                                            Session["price"] = Math.Round(price_without_exclude, 2);
                                            //decimal netprice = Convert.ToDecimal((Convert.ToDecimal(price_without_exclude) * paisa1) / 100);
                                            Session["payamount1"] = Convert.ToDecimal(Convert.ToDecimal((decimal)Session["price"]));
                                            Session["price1"] = Convert.ToDecimal(Session["payamount1"]);
                                            TempData["price"] = Math.Round((decimal)Session["payamount1"], 2);
                                            var amount = Math.Round(Convert.ToDecimal(Session["payamount1"]), 2);
                                            TempData["discamt1"] = paisa1;
                                            totalpaybletax = (tax * (decimal)Session["payamount1"]) / 100;

                                            TempData["payblamt"] = Math.Round((amount + totalpaybletax), 2); ;
                                            Session["payamount"] = Math.Round((amount + totalpaybletax), 2); ;
                                            TempData["Tax"] = Math.Round(Convert.ToDouble(totalpaybletax), 2);
                                            TempData["cartid"] = Convert.ToInt32(data.ToList()[0].Cart_ID);
                                            Session["prc"] = null;
                                        }
                                    }
                                    else
                                    {
                                        TempData["Message"] = "Coupon has expired";
                                    }
                                }
                                else
                                {
                                    Session["price"] = Math.Round(totalprice, 2);
                                }
                            }
                        }
                        else
                        {
                            if (Session["value"] == null)
                            {
                                ViewBag.cartitems = data;
                                decimal tax = taxcalculate();
                                ViewBag.taxper = tax;
                                TempData["totalitem"] = Convert.ToInt32(data.Count);
                                decimal totalprice1 = data.ToList().Where(x => x.Is_Offline == false || x.Is_Offline == null).Select(c => (decimal)c.Price).Sum();

                                var data2 = (from x in data.ToList().Where(x => x.Is_Offline == true)
                                             join y in DbContext.tbl_DC_Package_Period.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                             on x.Package_ID equals y.Package_ID
                                             select new DigiChampCartModel
                                             {
                                                 Is_Offline = x.Is_Offline,
                                                 Package_ID = x.Package_ID,
                                                 Price = x.Price,
                                                 Excluded_Price = y.Excluded_Price
                                             }).ToList();
                                if (data2.Count > 0)
                                {
                                    decimal totalprice2 = data2.ToList().Where(x => x.Is_Offline == true).Select(c => (decimal)c.Price).Sum();
                                    decimal totalprice3 = data2.ToList().Where(x => x.Is_Offline == true).Select(c => (decimal)c.Excluded_Price).Sum();
                                    //decimal totalprice = Convert.ToDecimal(totalprice1 + totalprice2 + totalprice3);                        
                                    //decimal totalprice = Convert.ToDecimal(Session["price1"]);

                                    if (Session["prc"] != null)
                                    {
                                        totalprice = Convert.ToDecimal(Session["prc"]);
                                    }
                                    else
                                    {
                                        totalprice = Convert.ToDecimal(Session["price1"]);
                                    }
                                    decimal totalpaybletax = (tax * totalprice) / 100;
                                    decimal totalpayblamt = Math.Round((totalprice + totalpaybletax), 2);
                                    TempData["payblamt"] = totalpayblamt;
                                    Session["payamount"] = totalpayblamt;

                                    TempData["price"] = Math.Round(totalprice, 2);
                                    TempData["Tax"] = Convert.ToDecimal(TempData["payblamt"]) - Convert.ToDecimal(TempData["price"]);
                                    var amt = DbContext.tbl_DC_Pre_book.Where(x => x.Regd_Id == rgid && x.Ord_Status == false && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                                    if (amt != null)
                                    {
                                        decimal pre_amt = Convert.ToDecimal(amt.Amount);
                                        TempData["payblamt"] = Convert.ToDecimal(totalpayblamt - pre_amt);
                                        TempData["pre_amt"] = Convert.ToDecimal(pre_amt);
                                        Session["payamount"] = TempData["payblamt"];
                                        TempData["price"] = Math.Round(totalprice, 2);
                                        Session["price"] = Math.Round(totalprice, 2);
                                        TempData["Tax"] = Math.Round((totalpaybletax), 2);
                                    }
                                    else
                                    {
                                        TempData["payblamt"] = totalpayblamt;
                                        TempData["pre_amt"] = null;
                                        Session["payamount"] = totalpayblamt;
                                        TempData["price"] = Math.Round(totalprice, 2);
                                        Session["price"] = Math.Round(totalprice, 2);
                                        TempData["Tax"] = Math.Round((totalpaybletax), 2);
                                    }
                                    if (dgcoupon.coupon != "" || dgcoupon.coupon != null)
                                    {
                                        var cop = DbContext.tbl_DC_CouponCode.Where(x => x.Coupon_Code == dgcoupon.coupon && x.Valid_From <= today && x.Valid_To >= today && x.Is_Active == true && x.Is_Delete == false).FirstOrDefault();
                                        if (cop != null)
                                        {
                                            if (cop.Discount_Price != null)
                                            {
                                                //int totl_pkg = Convert.ToInt32(data.ToList().Count);
                                                //TempData["c_name"] = cop.Coupon_Code;
                                                //decimal paisa = Convert.ToDecimal(cop.Discount_Price);
                                                //Session["price"] = Math.Round(totalprice, 2);
                                                //Session["payamount"] = Convert.ToDecimal(Convert.ToDecimal(TempData["payblamt"]) - (paisa * totl_pkg));
                                                //TempData["payblamt"] = Math.Round(Convert.ToDecimal(Session["payamount"]), 2);
                                                //TempData["discamt"] = paisa;
                                                //TempData["cartid"] = Convert.ToInt32(data.ToList()[0].Cart_ID);
                                                //Session["prc"] = null;

                                                int totl_pkg = Convert.ToInt32(data.ToList().Count);
                                                //TempData["c_name"] = cop.Coupon_Code;
                                                decimal paisa = Convert.ToDecimal(cop.Discount_Price);
                                                //Session["price"] = Math.Round(totalprice, 2);
                                                //Session["payamount"] = Convert.ToDecimal(Convert.ToDecimal(TempData["payblamt"]) - (paisa * totl_pkg));
                                                //TempData["payblamt"] = Math.Round(Convert.ToDecimal(Session["payamount"]), 2);
                                                //TempData["discamt"] = paisa;
                                                //TempData["cartid"] = Convert.ToInt32(data.ToList()[0].Cart_ID);
                                                //Session["prc"] = null;

                                                TempData["c_name"] = cop.Coupon_Code;
                                                decimal price_without_exclude = Math.Round((totalprice), 2);
                                                Session["price"] = Math.Round(price_without_exclude, 2);
                                                Session["payamount1"] = Convert.ToDecimal(Convert.ToDecimal((decimal)Session["price"]) - paisa);
                                                Session["price1"] = Convert.ToDecimal(Session["payamount1"]);
                                                TempData["price"] = Math.Round((decimal)Session["payamount1"], 2);
                                                var amount = Math.Round(Convert.ToDecimal(Session["payamount1"]), 2);
                                                totalpaybletax = (tax * (decimal)Session["payamount1"]) / 100;
                                                TempData["payblamt"] = Math.Round((amount + totalpaybletax), 2); ;
                                                Session["payamount"] = Math.Round((amount + totalpaybletax), 2); ;
                                                TempData["Tax"] = Math.Round(Convert.ToDouble(totalpaybletax), 2);

                                                //TempData["payblamt"] = Math.Round(Convert.ToDecimal((decimal)Session["payamount"]), 2);
                                                TempData["discamt"] = paisa;
                                                TempData["cartid"] = Convert.ToInt32(data.ToList()[0].Cart_ID);
                                                Session["prc"] = null;
                                            }
                                            else
                                            {
                                                //int totl_pkg = Convert.ToInt32(data.ToList().Count);
                                                //TempData["c_name"] = cop.Coupon_Code;
                                                //decimal paisa1 = Convert.ToDecimal(cop.Discount_Percent);
                                                //Session["price"] = Math.Round(totalprice, 2);
                                                //decimal netprice = Convert.ToDecimal((Convert.ToDecimal(TempData["payblamt"]) * paisa1) / 100);
                                                //Session["payamount"] = Convert.ToDecimal(Convert.ToDecimal(TempData["payblamt"]) - (netprice * totl_pkg));
                                                //TempData["payblamt"] = Math.Round(Convert.ToDecimal(Session["payamount"]), 2);
                                                //TempData["discamt1"] = paisa1;
                                                //TempData["cartid"] = Convert.ToInt32(data.ToList()[0].Cart_ID);
                                                //Session["prc"] = null;

                                                //int totl_pkg = Convert.ToInt32(data.ToList().Count);
                                                //TempData["c_name"] = cop.Coupon_Code;
                                                //decimal paisa1 = Convert.ToDecimal(cop.Discount_Percent);
                                                //Session["price"] = Math.Round(totalprice, 2);
                                                //decimal netprice = Convert.ToDecimal((Convert.ToDecimal(TempData["payblamt"]) * paisa1) / 100);
                                                //Session["payamount"] = Convert.ToDecimal(Convert.ToDecimal(TempData["payblamt"]) - (netprice * totl_pkg));
                                                //TempData["payblamt"] = Math.Round(Convert.ToDecimal(Session["payamount"]), 2);
                                                //TempData["discamt1"] = paisa1;
                                                //TempData["cartid"] = Convert.ToInt32(data.ToList()[0].Cart_ID);
                                                //Session["prc"] = null;

                                                int totl_pkg = Convert.ToInt32(data.ToList().Count);
                                                TempData["c_name"] = cop.Coupon_Code;
                                                decimal paisa1 = Convert.ToDecimal(cop.Discount_Percent);
                                                decimal price_without_exclude = Math.Round((totalprice), 2);

                                                Session["price"] = Math.Round(price_without_exclude, 2);
                                                //decimal netprice = Convert.ToDecimal((Convert.ToDecimal(price_without_exclude) * paisa1) / 100);
                                                Session["payamount1"] = Convert.ToDecimal(Convert.ToDecimal((decimal)Session["price"]));
                                                Session["price1"] = Convert.ToDecimal(Session["payamount1"]);
                                                TempData["price"] = Math.Round((decimal)Session["payamount1"], 2);
                                                var amount = Math.Round(Convert.ToDecimal(Session["payamount1"]), 2);
                                                TempData["discamt1"] = paisa1;
                                                totalpaybletax = (tax * (decimal)Session["payamount1"]) / 100;

                                                TempData["payblamt"] = Math.Round((amount + totalpaybletax), 2); ;
                                                Session["payamount"] = Math.Round((amount + totalpaybletax), 2); ;
                                                TempData["Tax"] = Math.Round(Convert.ToDouble(totalpaybletax), 2);
                                                TempData["cartid"] = Convert.ToInt32(data.ToList()[0].Cart_ID);
                                                Session["prc"] = null;
                                            }
                                        }
                                        else
                                        {
                                            TempData["Message"] = "Coupon has expired";
                                        }
                                    }
                                    else
                                    {
                                        Session["price"] = Math.Round(totalprice, 2);
                                    }
                                }
                                else
                                {

                                    if (Session["prc"] != null)
                                    {
                                        totalprice = Convert.ToDecimal(Session["prc"]);
                                    }
                                    else
                                    {
                                        totalprice = Convert.ToDecimal(Session["price1"]);
                                    }
                                    //decimal totalprice = Convert.ToDecimal(Session["price1"]);
                                    //decimal totalprice = Convert.ToDecimal(totalprice1);
                                    decimal totalpaybletax = (tax * totalprice) / 100;
                                    decimal totalpayblamt = Math.Round((totalprice + totalpaybletax), 2);
                                    TempData["payblamt"] = totalpayblamt;
                                    Session["payamount"] = totalpayblamt;
                                    TempData["price"] = Math.Round(totalprice, 2);
                                    TempData["Tax"] = Convert.ToDecimal(TempData["payblamt"]) - Convert.ToDecimal(TempData["price"]);
                                    var amt = DbContext.tbl_DC_Pre_book.Where(x => x.Regd_Id == rgid && x.Ord_Status == false && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                                    if (amt != null)
                                    {
                                        decimal pre_amt = Convert.ToDecimal(amt.Amount);
                                        TempData["payblamt"] = Convert.ToDecimal(totalpayblamt - pre_amt);
                                        TempData["pre_amt"] = Convert.ToDecimal(pre_amt);
                                        Session["payamount"] = TempData["payblamt"];
                                        TempData["price"] = Math.Round(totalprice, 2);
                                        Session["price"] = Math.Round(totalprice, 2);
                                        TempData["Tax"] = Math.Round((totalpaybletax), 2);
                                    }
                                    else
                                    {
                                        TempData["payblamt"] = totalpayblamt;
                                        TempData["pre_amt"] = null;
                                        Session["payamount"] = totalpayblamt;
                                        TempData["price"] = Math.Round(totalprice, 2);
                                        Session["price"] = Math.Round(totalprice, 2);
                                        TempData["Tax"] = Math.Round((totalpaybletax), 2);
                                    }
                                    if (dgcoupon.coupon != "" && dgcoupon.coupon != "")
                                    {
                                        var cop = DbContext.tbl_DC_CouponCode.Where(x => x.Coupon_Code == dgcoupon.coupon && x.Is_Default == false && x.Valid_From <= today && x.Valid_To >= today && x.Is_Active == true && x.Is_Delete == false).FirstOrDefault();
                                        if (cop != null)
                                        {
                                            if (cop.Discount_Price != null)
                                            {
                                                //int totl_pkg = Convert.ToInt32(data.ToList().Count);
                                                //TempData["c_name"] = cop.Coupon_Code;
                                                //decimal paisa = Convert.ToDecimal(cop.Discount_Price);
                                                //Session["price"] = Math.Round(totalprice, 2);
                                                //Session["payamount"] = Convert.ToDecimal(Convert.ToDecimal(TempData["payblamt"]) - (paisa * totl_pkg));
                                                //TempData["payblamt"] = Math.Round(Convert.ToDecimal(Session["payamount"]), 2);
                                                //TempData["discamt"] = paisa;
                                                //TempData["cartid"] = Convert.ToInt32(data.ToList()[0].Cart_ID);
                                                //Session["prc"] = null;

                                                int totl_pkg = Convert.ToInt32(data.ToList().Count);
                                                //TempData["c_name"] = cop.Coupon_Code;
                                                decimal paisa = Convert.ToDecimal(cop.Discount_Price);
                                                //Session["price"] = Math.Round(totalprice, 2);
                                                //Session["payamount"] = Convert.ToDecimal(Convert.ToDecimal(TempData["payblamt"]) - (paisa * totl_pkg));
                                                //TempData["payblamt"] = Math.Round(Convert.ToDecimal(Session["payamount"]), 2);
                                                //TempData["discamt"] = paisa;
                                                //TempData["cartid"] = Convert.ToInt32(data.ToList()[0].Cart_ID);
                                                //Session["prc"] = null;

                                                TempData["c_name"] = cop.Coupon_Code;
                                                decimal price_without_exclude = Math.Round((totalprice), 2);
                                                Session["price"] = Math.Round(price_without_exclude, 2);
                                                Session["payamount1"] = Convert.ToDecimal(Convert.ToDecimal((decimal)Session["price"]) - paisa);
                                                Session["price1"] = Convert.ToDecimal(Session["payamount1"]);
                                                TempData["price"] = Math.Round((decimal)Session["payamount1"], 2);
                                                var amount = Math.Round(Convert.ToDecimal(Session["payamount1"]), 2);
                                                totalpaybletax = (tax * (decimal)Session["payamount1"]) / 100;
                                                TempData["payblamt"] = Math.Round((amount + totalpaybletax), 2); ;
                                                Session["payamount"] = Math.Round((amount + totalpaybletax), 2); ;
                                                TempData["Tax"] = Math.Round(Convert.ToDouble(totalpaybletax), 2);

                                                //TempData["payblamt"] = Math.Round(Convert.ToDecimal((decimal)Session["payamount"]), 2);
                                                TempData["discamt"] = paisa;
                                                TempData["cartid"] = Convert.ToInt32(data.ToList()[0].Cart_ID);
                                                Session["prc"] = null;
                                            }
                                            else
                                            {
                                                //int totl_pkg = Convert.ToInt32(data.ToList().Count);
                                                //TempData["c_name"] = cop.Coupon_Code;
                                                //decimal paisa1 = Convert.ToDecimal(cop.Discount_Percent);
                                                //Session["price"] = Math.Round(totalprice, 2);
                                                //decimal netprice = Convert.ToDecimal((Convert.ToDecimal(TempData["payblamt"]) * paisa1) / 100);
                                                //Session["payamount"] = Convert.ToDecimal(Convert.ToDecimal(TempData["payblamt"]) - (netprice * totl_pkg));
                                                //TempData["payblamt"] = Math.Round(Convert.ToDecimal(Session["payamount"]), 2);
                                                //TempData["discamt1"] = paisa1;
                                                //TempData["cartid"] = Convert.ToInt32(data.ToList()[0].Cart_ID);
                                                //Session["prc"] = null;

                                                int totl_pkg = Convert.ToInt32(data.ToList().Count);
                                                TempData["c_name"] = cop.Coupon_Code;
                                                decimal paisa1 = Convert.ToDecimal(cop.Discount_Percent);
                                                decimal price_without_exclude = Math.Round((totalprice), 2);

                                                Session["price"] = Math.Round(price_without_exclude, 2);
                                                //decimal netprice = Convert.ToDecimal((Convert.ToDecimal(price_without_exclude) * paisa1) / 100);
                                                Session["payamount1"] = Convert.ToDecimal(Convert.ToDecimal((decimal)Session["price"]));
                                                Session["price1"] = Convert.ToDecimal(Session["payamount1"]);
                                                TempData["price"] = Math.Round((decimal)Session["payamount1"], 2);
                                                var amount = Math.Round(Convert.ToDecimal(Session["payamount1"]), 2);
                                                TempData["discamt1"] = paisa1;
                                                totalpaybletax = (tax * (decimal)Session["payamount1"]) / 100;

                                                TempData["payblamt"] = Math.Round((amount + totalpaybletax), 2); ;
                                                Session["payamount"] = Math.Round((amount + totalpaybletax), 2); ;
                                                TempData["Tax"] = Math.Round(Convert.ToDouble(totalpaybletax), 2);
                                                TempData["cartid"] = Convert.ToInt32(data.ToList()[0].Cart_ID);
                                                Session["prc"] = null;
                                            }
                                        }
                                        else
                                        {
                                            TempData["Message"] = "Coupon has expired";
                                        }
                                    }
                                    else
                                    {
                                        Session["price"] = Math.Round(totalprice, 2);
                                    }
                                }
                            }
                            else
                            {
                                ViewBag.cartitems = data;
                                decimal tax = taxcalculate();
                                ViewBag.taxper = tax;
                                TempData["totalitem"] = Convert.ToInt32(data.Count);

                                //decimal totalprice 
                                decimal totalpaybletax = 0;
                                decimal totalpayblamt = 0;

                                //decimal tax = taxcalculate();
                                var data3 = (from a in DbContext.tbl_DC_Registration.Where(x => x.Mobile == u_name && x.Is_Active == true && x.Is_Deleted == false)
                                             join b in DbContext.tbl_DC_Cart.Where(x => x.In_Cart == true && x.Is_Paid == false && x.Is_Active == true && x.Is_Deleted == false)
                                              on a.Regd_ID equals b.Regd_ID
                                             join c in DbContext.tbl_DC_Package.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                                 on b.Package_ID equals c.Package_ID
                                             select new DigiChampCartModel
                                             {
                                                 Regd_ID = a.Regd_ID,
                                                 firstname = a.Customer_Name,
                                                 phone = a.Mobile,
                                                 email = a.Email,
                                                 Package_Name = c.Package_Name,
                                                 Package_ID = c.Package_ID,
                                                 Subscripttion_Period = c.Subscripttion_Period,
                                                 Price = c.Price,
                                                 Is_Offline = c.Is_Offline,
                                                 Order_ID = b.Order_ID,
                                                 Order_No = b.Order_No,
                                                 Cart_ID = b.Cart_ID
                                             }).ToList();
                                decimal totalprice1 = data3.ToList().Where(x => x.Is_Offline == false || x.Is_Offline == null).Select(c => (decimal)c.Price).Sum();

                                var data2 = (from x in data.ToList().Where(x => x.Is_Offline == true)
                                             join y in DbContext.tbl_DC_Package_Period.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                             on x.Package_ID equals y.Package_ID
                                             select new DigiChampCartModel
                                             {
                                                 Is_Offline = x.Is_Offline,
                                                 Package_ID = x.Package_ID,
                                                 Price = x.Price,
                                                 Excluded_Price = y.Excluded_Price
                                             }).ToList();
                                if (data2.Count > 0)
                                {
                                    decimal totalprice2 = data2.ToList().Where(x => x.Is_Offline == true).Select(c => (decimal)c.Price).Sum();
                                    decimal totalprice3 = data2.ToList().Where(x => x.Is_Offline == true).Select(c => (decimal)c.Excluded_Price).Sum();
                                    //totalprice = Convert.ToDecimal(totalprice1 + totalprice2 + totalprice3);
                                    //totalprice = Convert.ToDecimal(Session["price1"]);

                                    if (Session["prc"] != null)
                                    {
                                        totalprice = Convert.ToDecimal(Session["prc"]);
                                    }
                                    else
                                    {
                                        totalprice = Convert.ToDecimal(Session["price1"]);
                                    }
                                    totalpaybletax = (tax * totalprice) / 100;
                                    totalpayblamt = Math.Round((totalprice + totalpaybletax), 2);
                                    TempData["payblamt"] = totalpayblamt;
                                    Session["payamount"] = totalpayblamt;
                                    TempData["price"] = Math.Round(totalprice, 2);
                                    Session["price"] = Math.Round(totalprice, 2);
                                    TempData["Tax"] = Convert.ToDecimal(TempData["payblamt"]) - Convert.ToDecimal(TempData["price"]);
                                    Session["prc"] = null;
                                }
                                else
                                {

                                    if (Session["prc"] != null)
                                    {
                                        totalprice = Convert.ToDecimal(Session["prc"]);
                                    }
                                    else
                                    {
                                        totalprice = Convert.ToDecimal(Session["price1"]);
                                    }
                                    //totalprice = Convert.ToDecimal(Session["price1"]);
                                    //totalprice = Convert.ToDecimal(totalprice1);
                                    totalpaybletax = (tax * totalprice) / 100;
                                    totalpayblamt = Math.Round((totalprice + totalpaybletax), 2);
                                    TempData["payblamt"] = totalpayblamt;
                                    Session["payamount"] = totalpayblamt;
                                    TempData["price"] = Math.Round(totalprice, 2);
                                    Session["price"] = Math.Round(totalprice, 2);
                                    TempData["Tax"] = Convert.ToDecimal(TempData["payblamt"]) - Convert.ToDecimal(TempData["price"]);
                                    Session["prc"] = null;
                                }
                            }

                        }
                        if (TempData["cartid"] != null)
                        {
                            int ca_id = Convert.ToInt32(TempData["cartid"]);
                            tbl_DC_Cart obj = DbContext.tbl_DC_Cart.Where(x => x.Cart_ID == ca_id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                            obj.Coupon_Code = dgcoupon.coupon;
                            DbContext.Entry(obj).State = EntityState.Modified;
                            DbContext.SaveChanges();
                        }
                        Session["ordid"] = data.ToList()[0].Order_ID;
                        Session["ordno"] = data.ToList()[0].Order_No;
                    }

                }
                else
                {
                    return RedirectToAction("Logout");
                }
            }
            catch (Exception ex)
            {
                TempData["errormessgae"] = "Oops,Something went wrong";
            }

            return RedirectToAction("checkout", "Student", dgcoupon);
        }

        //[HttpPost]
        //public void OrderNow(string proceed)
        //{
        //    try
        //    {
        //        if (Session["USER_NAME"] != null)
        //        {

        //            string firstName = Session["fname"].ToString();
        //            string amount = Session["payamount"].ToString();
        //            string productInfo = "Packages for student";
        //            //string email = Session["email"].ToString();
        //            if (Session["email"] != null)
        //            {
        //                string email = Session["email"].ToString();
        //                string phone = Session["phone"].ToString();
        //                //string surl = "http://sms.ntspl.co.in/student/orderconfirmation";
        //                //string furl = "http://sms.ntspl.co.in/student/cancelpayment";
        //                string surl = "http://thedigichamps.com/Student/OrderConfirmation";
        //                string furl = "http://thedigichamps.com/Student/CancelPayment";
        //                //string surl = "http://sms.host.thedigichamps.com/Student/OrderConfirmation";
        //                //string furl = "http://sms.host.thedigichamps.com/Student/CancelPayment";

        //                RemotePost myremotepost = new RemotePost();


        //                string key = "ZbKwHjmu";
        //                string salt = "hhhpte1L9R";

        //                myremotepost.Url = "https://secure.payu.in/_payment";


        //                //string key = "rjQUPktU";
        //                //string salt = "e5iIg1jwi8";

        //                //myremotepost.Url = "https://test.payu.in/_payment";


        //                myremotepost.Add("key", key);
        //                string txnid = Generatetxnid();
        //                myremotepost.Add("txnid", txnid);


        //                myremotepost.Add("amount", amount);
        //                myremotepost.Add("productinfo", productInfo);
        //                myremotepost.Add("firstname", firstName);
        //                myremotepost.Add("phone", phone);
        //                myremotepost.Add("email", email);
        //                myremotepost.Add("surl", surl);
        //                myremotepost.Add("furl", furl);
        //                myremotepost.Add("service_provider", "payu_paisa");
        //                string hashString = key + "|" + txnid + "|" + amount + "|" + productInfo + "|" + firstName + "|" + email + "|||||||||||" + salt;
        //                string hash = Generatehash512(hashString);
        //                myremotepost.Add("hash", hash);
        //                myremotepost.Post();
        //            }
        //            else
        //            {
        //                string email = "ranjan.kumar@ntspl.co.in";
        //                string phone = Session["phone"].ToString();
        //                //string surl = "http://sms.ntspl.co.in/student/orderconfirmation";
        //                //string furl = "http://sms.ntspl.co.in/student/cancelpayment";
        //                string surl = "http://thedigichamps.com/Student/OrderConfirmation";
        //                string furl = "http://thedigichamps.com/Student/CancelPayment";
        //                //string surl = "http://sms.host.thedigichamps.com/Student/OrderConfirmation";
        //                //string furl = "http://sms.host.thedigichamps.com/Student/CancelPayment";

        //                RemotePost myremotepost = new RemotePost();


        //                string key = "ZbKwHjmu";
        //                string salt = "hhhpte1L9R";

        //                myremotepost.Url = "https://secure.payu.in/_payment";


        //                //string key = "rjQUPktU";
        //                //string salt = "e5iIg1jwi8";

        //                //myremotepost.Url = "https://test.payu.in/_payment";


        //                myremotepost.Add("key", key);
        //                string txnid = Generatetxnid();
        //                myremotepost.Add("txnid", txnid);


        //                myremotepost.Add("amount", amount);
        //                myremotepost.Add("productinfo", productInfo);
        //                myremotepost.Add("firstname", firstName);
        //                myremotepost.Add("phone", phone);
        //                myremotepost.Add("email", email);
        //                myremotepost.Add("surl", surl);
        //                myremotepost.Add("furl", furl);
        //                myremotepost.Add("service_provider", "payu_paisa");
        //                string hashString = key + "|" + txnid + "|" + amount + "|" + productInfo + "|" + firstName + "|" + email + "|||||||||||" + salt;
        //                string hash = Generatehash512(hashString);
        //                myremotepost.Add("hash", hash);
        //                myremotepost.Post();
        //            }
        //        }
        //        else
        //        {
        //            Response.Redirect("Logout");
        //            //return RedirectToAction("Logout");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ex.Message.ToString();
        //    }
        //}
        [HttpPost]
        public ActionResult OrderNow(FormCollection form)
        {
            try
            {
                int regid = Convert.ToInt32(Session["User_ID"]);
                if (form != null)
                {
                    CCACrypto ccaCrypto = new CCACrypto();
                    string workingKey = "20E520D5E76B0B58F40F0C10A50632CC";
                    string ccaRequest = "";
                    string strEncRequest = "";
                    ViewBag.strAccessCode = "AVDM71EF56BY18MDYB";
                    foreach (string name in form)
                    {
                        if (name != null)
                        {
                            if (!name.StartsWith("_"))
                            {
                                ccaRequest = ccaRequest + name + "=" + Request.Form[name] + "&";
                                /* Response.Write(name + "=" + Request.Form[name]);
                                  Response.Write("</br>");*/
                            }
                        }
                    }
                    Session["ordid"] = Request.Form["order_id"];
                    ViewBag.strEncRequest = ccaCrypto.Encrypt(ccaRequest, workingKey);
                    return View();
                }
            }
            catch (Exception ex)
            {
                ex.Message.ToString();
            }
            return View();
        }

        public class RemotePost
        {
            private System.Collections.Specialized.NameValueCollection Inputs = new System.Collections.Specialized.NameValueCollection();


            public string Url = "";
            public string Method = "post";
            public string FormName = "form1";

            public void Add(string name, string value)
            {
                Inputs.Add(name, value);
            }

            public void Post()
            {
                System.Web.HttpContext.Current.Response.Clear();

                System.Web.HttpContext.Current.Response.Write("<html><head>");

                System.Web.HttpContext.Current.Response.Write(string.Format("</head><body onload=\"document.{0}.submit()\">", FormName));
                System.Web.HttpContext.Current.Response.Write(string.Format("<form name=\"{0}\" method=\"{1}\" action=\"{2}\" >", FormName, Method, Url));
                for (int i = 0; i < Inputs.Keys.Count; i++)
                {
                    System.Web.HttpContext.Current.Response.Write(string.Format("<input name=\"{0}\" type=\"hidden\" value=\"{1}\">", Inputs.Keys[i], Inputs[Inputs.Keys[i]]));
                }
                System.Web.HttpContext.Current.Response.Write("</form>");
                System.Web.HttpContext.Current.Response.Write("</body></html>");

                System.Web.HttpContext.Current.Response.End();
            }
        }

        public string Generatehash512(string text)
        {

            byte[] message = Encoding.UTF8.GetBytes(text);

            UnicodeEncoding UE = new UnicodeEncoding();
            byte[] hashValue;
            SHA512Managed hashString = new SHA512Managed();
            string hex = "";
            hashValue = hashString.ComputeHash(message);
            foreach (byte x in hashValue)
            {
                hex += String.Format("{0:x2}", x);
            }
            return hex;

        }

        public string Generatetxnid()
        {

            Random rnd = new Random();
            string strHash = Generatehash512(rnd.ToString() + DateTime.Now);
            string txnid1 = strHash.ToString().Substring(0, 20);

            return txnid1;
        }
        //[HttpGet]
        public ActionResult OrderConfirmation()
        {
            try
            {

                string workingKey = "20E520D5E76B0B58F40F0C10A50632CC";//put in the 32bit alpha numeric key in the quotes provided here
                CCACrypto ccaCrypto = new CCACrypto();
                string encResponse = ccaCrypto.Decrypt(Request.Form["encResp"], workingKey);
                NameValueCollection Params = new NameValueCollection();
                string[] segments = encResponse.Split('&');
                foreach (string seg in segments)
                {
                    string[] parts = seg.Split('=');
                    if (parts.Length > 0)
                    {
                        string Key = parts[0].Trim();
                        string Value = parts[1].Trim();
                        Params.Add(Key, Value);
                    }
                }
                string orderid = "";
                string paymentmode = "";
                string trackingid = "";
                string datas = "";
                for (int i = 0; i < Params.Count; i++)
                {
                    // Response.Write(Params.Keys[i] + " = " + Params[i] + "<br>");
                    if (Params.Keys[i] == "order_status")
                    {
                        datas = Params[i];

                    }


                    if (Params.Keys[0] == "order_id")
                    {
                        TempData["orderno"] = Params[0];
                        orderid = Params[0];
                    }
                    if (Params.Keys[1] == "tracking_id")
                    {
                        TempData["trackingid"] = Params[1];

                    }

                    if (Params.Keys[5] == "payment_mode")
                    {

                        TempData["paymentmode"] = Params[5];
                    }
                    if (Params.Keys[10] == "amount")
                    {

                        TempData["amount"] = Params[10];
                        //}
                    }
                    if (Params.Keys[11] == "billing_name")
                    {

                        TempData["billing_name"] = Params[11];
                        //}
                    }

                    if (Params.Keys[18] == "billing_email")
                    {

                        TempData["billing_email"] = Params[18];

                    }


                }


                //int ordid = 0;
                string odid = TempData["orderno"].ToString();
                decimal price = Convert.ToDecimal(TempData["amount"]);
                TempData["payblamt"] = Math.Round(price, 2);
                Session["payblamt"]= Math.Round(price, 2);
                string email = TempData["billing_email"].ToString();
                string name = TempData["billing_name"].ToString();

                string username = string.Empty;
                string usertype = string.Empty;
                //}
                if (datas.Trim() == "Success")
                {
                  
                    if (Session["USER_NAME"] != null)
                    {
                       
                        if (Session["ordid"] != null)
                        {

                           


                            decimal discount = 0;
                            string disc_amt = "";
                            if (Session["disc"] != null)
                            {
                                discount = Math.Round((decimal)Session["disc"], 2);
                                disc_amt = discount.ToString();
                            }
                            else
                            {
                                discount = 0;
                                disc_amt = discount.ToString();
                            }
                            string u_name = Session["USER_NAME"].ToString();
                            string ordno = Session["ordno"].ToString();
                            var vc = (from e in DbContext.tbl_DC_Cart.Where(x => x.Order_No == ordno && x.In_Cart == true && x.Is_Paid == false && x.Is_Active == true && x.Is_Deleted == false ) select e).FirstOrDefault();
                            int ?ordid = 0;
                            if (vc != null)
                            {

                                  ordid = vc.Order_ID;
                            }
                            ViewBag.ordno = ordno;

                            decimal tptal_price = Convert.ToDecimal(Session["price"]);
                            decimal tptal_payble = Convert.ToDecimal(TempData["payblamt"]);

                            var data2 = (from a in DbContext.tbl_DC_Registration.Where(x => x.Mobile == u_name && x.Is_Active == true && x.Is_Deleted == false)
                                         join b in DbContext.tbl_DC_Cart.Where(x => x.Status == true && x.In_Cart == true && x.Is_Paid == false && x.Is_Active == true && x.Is_Deleted == false)
                                         on a.Regd_ID equals b.Regd_ID
                                         select new DigiChampCartModel
                                         {
                                             firstname = a.Customer_Name,
                                             Regd_ID = a.Regd_ID,
                                             email = a.Email,
                                             Regd_No = a.Regd_No,
                                             Cart_ID = b.Cart_ID,
                                             Order_ID = b.Order_ID,
                                             Order_No = b.Order_No,
                                             Address = a.Address,
                                             Mobile = a.Mobile,
                                             PIN = a.Pincode
                                         }).ToList();

                            int regid = Convert.ToInt32(data2.ToList()[0].Regd_ID);

                            var pre_b = DbContext.tbl_DC_Pre_book.Where(x => x.Regd_Id == regid && x.Ord_Status == false).FirstOrDefault();
                            if (pre_b != null)
                            {
                                tbl_DC_Pre_book pre_book = DbContext.tbl_DC_Pre_book.Where(x => x.Regd_Id == regid).FirstOrDefault();
                                pre_book.Ord_Status = true;
                                pre_book.Order_Id = ordid;
                                DbContext.Entry(pre_book).State = EntityState.Modified;
                                DbContext.SaveChanges();
                            }
                            string regno = data2.ToList()[0].Regd_No;

                            #region------------------------------- confirm-ordercode-------------------------------

                            var data3 = (from e in DbContext.tbl_DC_Cart.Where(x => x.Order_ID == ordid && x.Order_No == ordno && x.In_Cart == true && x.Is_Paid == false && x.Is_Active == true && x.Is_Deleted == false)
                                         join f in DbContext.tbl_DC_Cart_Dtl.Where(x => x.Status == true && x.Is_Active == true && x.Is_Deleted == false)
                                         on e.Cart_ID equals f.Cart_ID
                                         join a in DbContext.tbl_DC_Chapter.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                         on f.Chapter_ID equals a.Chapter_Id
                                         join b in DbContext.tbl_DC_Subject.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                         on a.Subject_Id equals b.Subject_Id
                                         join c in DbContext.tbl_DC_Class.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                         on b.Class_Id equals c.Class_Id
                                         join d in DbContext.tbl_DC_Board.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                         on c.Board_Id equals d.Board_Id
                                         select new DigiChampCartModel
                                         {
                                             Regd_ID = e.Regd_ID,
                                             Regd_No = e.Regd_No,
                                             Cart_ID = e.Cart_ID,
                                             Order_ID = e.Order_ID,
                                             Order_No = e.Order_No,
                                             Board_ID = f.Board_ID,
                                             Board_Name = d.Board_Name,
                                             Class_ID = f.Class_ID,
                                             Class_Name = c.Class_Name,
                                             Subject_ID = f.Subject_ID,
                                             Subject = b.Subject,
                                             Chapter_ID = f.Chapter_ID,
                                             Chapter = a.Chapter
                                         }).ToList();

                            var data31 = DbContext.tbl_DC_Cart.Where(x => x.Order_ID == ordid && x.Order_No == ordno && x.In_Cart == true && x.Is_Paid == false && x.Is_Active == true && x.Is_Deleted == false).ToList();

                            ViewBag.cartdata = data31;
                            //int cart_id = Convert.ToInt32(data3.ToList()[0].Cart_ID);

                            //------------------------------Order cofirm region-----------------------

                            var data1 = DbContext.tbl_DC_Registration.Where(x => x.Mobile == u_name && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();

                            ViewBag.mobile = data1.Mobile;
                            ViewBag.custmer_name = data1.Customer_Name;

                            var data = (from a in DbContext.tbl_DC_Registration.Where(x => x.Mobile == u_name && x.Is_Active == true && x.Is_Deleted == false)
                                        join b in DbContext.tbl_DC_Cart.Where(x => x.In_Cart == true && x.Is_Paid == false && x.Is_Active == true && x.Is_Deleted == false)
                                         on a.Regd_ID equals b.Regd_ID
                                        join c in DbContext.tbl_DC_Package.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                            on b.Package_ID equals c.Package_ID
                                        select new DigiChampCartModel
                                        {
                                            Package_Name = c.Package_Name,
                                            Subscripttion_Period = c.Subscripttion_Period,
                                            Price =b.Total_Amt,
                                            Order_ID = b.Order_ID,
                                            Order_No = b.Order_No,
                                            Inserted_Date = c.Inserted_Date,
                                            Cart_ID = b.Cart_ID
                                        }).ToList();
                            //ViewBag.cartitems = data;
                            ViewBag.data = data;

                            if (data.Count == 0)
                            {
                                ViewBag.cartitems = null;
                                TempData["emptycart"] = "Your cart is empty";
                            }
                            else
                            {
                                Session["price"] = data.Sum(x => x.Price) ; 
                                ViewBag.cartitems = data;
                                decimal tax = taxcalculate();
                                TempData["totalitem"] = Convert.ToInt32(data.Count);
                                decimal totalprice = Math.Round((Convert.ToDecimal(Session["price"])), 2);
                                decimal amt = (Convert.ToDecimal(totalprice) / (100 + tax)) * 100;
                                decimal totalpaybletax = totalprice- amt;
                               // decimal totalpayblamt = Math.Round((totalprice + totalpaybletax), 2);
                               // TempData["payblamt"] = totalpayblamt;
                                Session["payamount"] = totalprice;
                                Session["ordid"] = data.ToList()[0].Order_ID;
                                Session["ordno"] = data.ToList()[0].Order_No;
                                TempData["price"] = Math.Round(totalprice, 2);
                                Session["price"] = Math.Round(totalprice, 2);
                                TempData["Tax"] = totalpaybletax;
                            }

                            //------------------------------end region--------------------------------

                            //-------------------------------Cart update region------------------------

                            tbl_DC_Order ordobj = new tbl_DC_Order();
                            ordobj.Cart_Order_No = ordno;
                            ordobj.Trans_No = TempData["trackingid"].ToString();
                            ordobj.Regd_ID = regid;
                            ordobj.Regd_No = regno;
                            ordobj.No_of_Package = Convert.ToInt32(TempData["totalitem"]);
                            ordobj.Amount = Convert.ToDecimal(Session["price"]);
                            if (Session["percent"] != null)
                            {
                                ordobj.Disc_Perc = Convert.ToDecimal(Session["disc"]);
                                disc_amt = Session["disc"] + " %";
                                ordobj.Disc_Amt = null;
                            }
                            if (Session["percent1"] != null)
                            {
                                ordobj.Disc_Perc = null;
                                ordobj.Disc_Amt = Convert.ToDecimal(Session["disc"]);
                                disc_amt = Convert.ToDecimal(Session["disc"]).ToString("N2");
                            }
                            ordobj.Total = Convert.ToDecimal(TempData["payblamt"]);
                            ordobj.Amt_In_Words = null;
                            ordobj.Payment_Mode = "Online";
                            ordobj.Is_Paid = true;
                            ordobj.Status = true;
                            ordobj.Inserted_Date = DateTime.Now;
                            string date = Convert.ToDateTime(ordobj.Inserted_Date).ToString("MM/dd/yyyy");
                            TempData["Purcahse_date"] = date;
                            ordobj.Inserted_By = HttpContext.User.Identity.Name;
                            ordobj.Is_Active = true;
                            ordobj.Is_Deleted = false;
                            DbContext.tbl_DC_Order.Add(ordobj);
                            DbContext.SaveChanges();
                            Session["percent"] = null;
                            Session["percent1"] = null;
                            var ord_id = ordobj.Order_ID;
                            if (ord_id == null)
                            {
                                ordobj.Order_No = "DCORD" + "00000" + 1;
                                TempData["ordernumber"] = ord_id;
                            }
                            else
                            {
                                int ord_no = Convert.ToInt32(ordobj.Order_ID);
                                if (ord_no > 0 && ord_no <= 9)
                                {
                                    ordobj.Order_No = "DCORD" + "00000" + Convert.ToString(ord_no);
                                }
                                if (ord_no > 9 && ord_no <= 99)
                                {
                                    ordobj.Order_No = "DCORD" + "0000" + Convert.ToString(ord_no);
                                }
                                if (ord_no > 99 && ord_no <= 999)
                                {
                                    ordobj.Order_No = "DCORD" + "000" + Convert.ToString(ord_no);
                                }
                                if (ord_no > 999 && ord_no <= 9999)
                                {
                                    ordobj.Order_No = "DCORD" + "00" + Convert.ToString(ord_no);
                                }
                                if (ord_no > 9999 && ord_no <= 99999)
                                {
                                    ordobj.Order_No = "DCORD" + "0" + Convert.ToString(ord_no);
                                }
                                TempData["ordernumber"] = ordobj.Order_No;
                            }
                            DbContext.SaveChanges();

                            var tax3 = (from tax1 in DbContext.tbl_DC_Tax_Type_Master.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                        join tax2 in DbContext.tbl_DC_Tax_Master.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                        on tax1.TaxType_ID equals tax2.TaxType_ID
                                        select new DigiChampCartModel
                                        {
                                            Tax_ID = tax2.Tax_ID,
                                            TaxType_ID = tax1.TaxType_ID,
                                            Tax_Rate = tax2.Tax_Rate,
                                            TAX_Efect_Date = tax2.TAX_Efect_Date
                                        }).ToList();
                            ViewBag.tax3 = tax3;

                            if (tax3.Count > 0)
                            {
                                foreach (var item3 in ViewBag.tax3)
                                {
                                    tbl_DC_Order_Tax ordtax = new tbl_DC_Order_Tax();
                                    ordtax.Order_ID = ordobj.Order_ID;
                                    ordtax.Order_No = ordobj.Order_No;
                                    ordtax.Tax_ID = item3.Tax_ID;
                                    ordtax.Tax_Type_ID = item3.TaxType_ID;
                                    ordtax.Tax_Effect_Date = item3.TAX_Efect_Date;
                                    ordtax.Tax_Amt = Convert.ToDecimal(((ordobj.Amount) * (item3.Tax_Rate)) / 100);
                                    ordtax.Status = true;
                                    ordtax.Inserted_Date = DateTime.Now;
                                    ordtax.Inserted_By = HttpContext.User.Identity.Name;
                                    ordtax.Is_Active = true;
                                    ordtax.Is_Deleted = false;
                                    DbContext.tbl_DC_Order_Tax.Add(ordtax);
                                    DbContext.SaveChanges();
                                }
                            }

                            var order_pkg = (from ord in DbContext.tbl_DC_Order.Where(x => x.Regd_ID == regid && x.Cart_Order_No == ordno && x.Status == true && x.Is_Active == true && x.Is_Deleted == false)
                                             join cart in DbContext.tbl_DC_Cart.Where(x => x.Regd_ID == regid && x.Order_No == ordno && x.In_Cart == true && x.Is_Paid == false && x.Status == true && x.Is_Active == true && x.Is_Deleted == false)
                                             on ord.Cart_Order_No equals cart.Order_No
                                             join pkg in DbContext.tbl_DC_Package.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                             on cart.Package_ID equals pkg.Package_ID
                                             select new DigiChampCartModel
                                             {
                                                 OrderID = ord.Order_ID,
                                                 OrderNo = ord.Order_No,
                                                 Package_ID = cart.Package_ID,
                                                 Package_Name = pkg.Package_Name,
                                                 Package_Desc = pkg.Package_Desc,
                                                 Total_Chapter = pkg.Total_Chapter,
                                                 Price = pkg.Price,
                                                 Thumbnail = pkg.Thumbnail,
                                                 Subscripttion_Period = pkg.Subscripttion_Period,
                                                 Is_Offline = pkg.Is_Offline
                                             }).ToList();
                            ViewBag.package = order_pkg;

                            foreach (var item in ViewBag.package)
                            {
                                tbl_DC_Order_Pkg ordpkg = new tbl_DC_Order_Pkg();
                                ordpkg.Order_ID = item.OrderID;
                                ordpkg.Order_No = item.OrderNo;
                                ordpkg.Package_ID = item.Package_ID;
                                ordpkg.Package_Name = item.Package_Name;
                                ordpkg.Package_Desc = item.Package_Desc;
                                ordpkg.Total_Chapter = item.Total_Chapter;
                                ordpkg.Price = item.Price;
                                ordpkg.Thumbnail = item.Thumbnail;
                                ordpkg.Subscription_Period = item.Subscripttion_Period;
                                int days = Convert.ToInt32(ordpkg.Subscription_Period);
                                ordpkg.Status = true;
                                ordpkg.Inserted_Date = DateTime.Now;
                                ordpkg.Expiry_Date = Convert.ToDateTime(ordpkg.Inserted_Date).AddDays(days);
                                ordpkg.Inserted_By = HttpContext.User.Identity.Name;
                                ordpkg.Is_Active = true;
                                ordpkg.Is_Deleted = false;
                                if (order_pkg.ToList()[0].Is_Offline == true)
                                {
                                    ordpkg.Is_Offline = true;
                                }
                                else
                                {
                                    ordpkg.Is_Offline = false;
                                }
                                DbContext.tbl_DC_Order_Pkg.Add(ordpkg);
                                DbContext.SaveChanges();

                                int pkg_id = Convert.ToInt32(ordpkg.Package_ID);
                                int ord_pkg_id = Convert.ToInt32(ordpkg.OrderPkg_ID);
                                foreach (var v in ViewBag.cartdata)
                                {
                                    int cart_id = Convert.ToInt32(v.Cart_ID);

                                    var ordpkg_sub = (from ab in DbContext.tbl_DC_Order_Pkg.Where(x => x.OrderPkg_ID == ord_pkg_id && x.Status == true && x.Is_Active == true && x.Is_Deleted == false)
                                                      join af in DbContext.tbl_DC_Cart_Dtl.Where(x => x.Cart_ID == cart_id && x.Is_Active == true && x.Is_Deleted == false)
                                                      on ab.Package_ID equals af.Package_ID
                                                      join ag in DbContext.tbl_DC_Chapter.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                                      on af.Chapter_ID equals ag.Chapter_Id
                                                      join ah in DbContext.tbl_DC_Subject.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                                      on ag.Subject_Id equals ah.Subject_Id
                                                      join ai in DbContext.tbl_DC_Class.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                                      on ah.Class_Id equals ai.Class_Id
                                                      join aj in DbContext.tbl_DC_Board.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                                      on ai.Board_Id equals aj.Board_Id
                                                      select new DigiChampCartModel
                                                      {
                                                          OrderPkg_ID = ab.OrderPkg_ID,
                                                          Package_ID = af.Package_ID,
                                                          Board_ID = aj.Board_Id,
                                                          Board_Name = aj.Board_Name,
                                                          Class_ID = ai.Class_Id,
                                                          Class_Name = ai.Class_Name,
                                                          Subject_ID = ah.Subject_Id,
                                                          Subject = ah.Subject,
                                                          Chapter_ID = ag.Chapter_Id,
                                                          Chapter = ag.Chapter
                                                      }).ToList();
                                    ViewBag.ordpkg_sub = ordpkg_sub;

                                    foreach (var item1 in ViewBag.ordpkg_sub)
                                    {
                                        tbl_DC_Order_Pkg_Sub ordpkgsub = new tbl_DC_Order_Pkg_Sub();
                                        ordpkgsub.OrderPkg_ID = item1.OrderPkg_ID;
                                        ordpkgsub.Package_ID = item1.Package_ID;
                                        ordpkgsub.Board_ID = item1.Board_ID;
                                        ordpkgsub.Board_Name = item1.Board_Name;
                                        ordpkgsub.Class_ID = item1.Class_ID;
                                        ordpkgsub.Class_Name = item1.Class_Name;
                                        ordpkgsub.Subject_ID = item1.Subject_ID;
                                        ordpkgsub.Subject_Name = item1.Subject;
                                        ordpkgsub.Chapter_ID = item1.Chapter_ID;
                                        ordpkgsub.Chapter_Name = item1.Chapter;
                                        ordpkgsub.Status = true;
                                        ordpkgsub.Inserted_Date = DateTime.Now;
                                        ordpkgsub.Inserted_By = HttpContext.User.Identity.Name;
                                        ordpkgsub.Is_Active = true;
                                        ordpkgsub.Is_Deleted = false;
                                        DbContext.tbl_DC_Order_Pkg_Sub.Add(ordpkgsub);
                                        DbContext.SaveChanges();

                                        int ord_pkg_sub_id = Convert.ToInt32(ordpkgsub.OrderPkgSub_ID);

                                        var ordpkg_sub_mod = (from ak in DbContext.tbl_DC_Order_Pkg_Sub.Where(x => x.OrderPkgSub_ID == ord_pkg_sub_id && x.Status == true && x.Is_Active == true && x.Is_Deleted == false)
                                                              join al in DbContext.tbl_DC_Module.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                                                  on ak.Chapter_ID equals al.Chapter_Id
                                                              select new DigiChampCartModel
                                                              {
                                                                  OrderPkgSub_ID = ak.OrderPkgSub_ID,
                                                                  Chapter_ID = ak.Chapter_ID,
                                                                  Chapter = ak.Chapter_Name,
                                                                  Module_ID = al.Module_ID,
                                                                  Module_Name = al.Module_Name
                                                              }).ToList();
                                        ViewBag.ordpkg_sub_mod = ordpkg_sub_mod;
                                        if (ordpkg_sub_mod.Count > 0)
                                        {
                                            foreach (var item2 in ViewBag.ordpkg_sub_mod)
                                            {
                                                tbl_DC_Order_Pkg_Sub_Mod ordpkgsubmod = new tbl_DC_Order_Pkg_Sub_Mod();
                                                ordpkgsubmod.OrderPkgSub_ID = item2.OrderPkgSub_ID;
                                                ordpkgsubmod.Chapter_ID = item2.Chapter_ID;
                                                ordpkgsubmod.Chapter_Name = item2.Chapter;
                                                ordpkgsubmod.Module_ID = item2.Module_ID;
                                                ordpkgsubmod.Module_Name = item2.Module_Name;
                                                ordpkgsubmod.Status = true;
                                                ordpkgsubmod.Inserted_Date = DateTime.Now;
                                                ordpkgsubmod.Inserted_By = HttpContext.User.Identity.Name;
                                                ordpkgsubmod.Is_Active = true;
                                                ordpkgsubmod.Is_Deleted = false;
                                                DbContext.tbl_DC_Order_Pkg_Sub_Mod.Add(ordpkgsubmod);
                                                DbContext.SaveChanges();
                                            }
                                        }
                                    }
                                }
                            }


                            var cartobj = (from ct in DbContext.tbl_DC_Cart.Where(x => x.Order_ID == ordid && x.Order_No == ordno && x.Is_Active == true && x.Is_Deleted == false)
                                           select new DigiChampCartModel
                                           {
                                               Cart_ID = ct.Cart_ID,
                                               In_Cart = ct.In_Cart,
                                               Is_Paid = ct.Is_Paid,
                                               Status = ct.Status,
                                               Is_Active = ct.Is_Active,
                                               Is_Deleted = ct.Is_Deleted
                                           }).ToList();
                            ViewBag.cartobj = cartobj;

                            foreach (var item4 in ViewBag.cartobj)
                            {
                                int ctid = Convert.ToInt32(item4.Cart_ID);
                                tbl_DC_Cart cartobj1 = DbContext.tbl_DC_Cart.Where(x => x.Cart_ID == ctid && x.Is_Active == true && x.Is_Deleted == false && x.In_Cart == true && x.Is_Paid == false).FirstOrDefault();
                                cartobj1.In_Cart = false;
                                cartobj1.Is_Paid = true;
                                cartobj1.Status = false;
                                cartobj1.Is_Active = false;
                                cartobj1.Is_Deleted = true;
                                DbContext.Entry(cartobj1).State = EntityState.Modified;
                                DbContext.SaveChanges();
                            }
                            #endregion

                            if (data2.ToList()[0].email != null && data2.ToList()[0].email != "")
                            {
                                var getall = DbContext.SP_DC_Get_maildetails("ORD_CONF").FirstOrDefault();

                                string names = data2.ToList()[0].firstname;
                                string odno = TempData["ordernumber"].ToString();
                                string total_amt = Session["price"].ToString();
                                string tx_amt = TempData["Tax"].ToString();
                                string pbl_amt = TempData["payblamt"].ToString();
                                string ord_date = date;
                                string address = data2.ToList()[0].Address;
                                string pin = data2.ToList()[0].PIN;
                                string mobile = data2.ToList()[0].Mobile;
                                StringBuilder sb = new StringBuilder();
                                if (ViewBag.data != null)
                                {
                                    int m = 1;

                                    foreach (var it in ViewBag.data)
                                    {

                                        string prices = string.Format("{0:0.00}", it.Price);
                                        sb.Append("<tr style='text-align:center;font-family: monospace; height:50px;'>");
                                        sb.Append("<td style='border:1px solid #0f6fc6;'>" + m++ + "</td>");
                                        sb.Append("<td style='border:1px solid #0f6fc6;'>" + it.Package_Name + "</td>");
                                        sb.Append("<td style='border:1px solid #0f6fc6;'>" + 1 + "</td>");
                                        sb.Append("<td style='border:1px solid #0f6fc6;'> " + prices + "</td>");
                                        sb.Append("<td style='border:1px solid #0f6fc6;'>" + total_amt + "</td>");
                                        sb.Append("</tr>");
                                    }
                                }
                                string tbl_dtl = sb.ToString();

                                string msgbody = getall.EmailConf_Body.ToString().Replace("{{name}}", names).Replace("{{orderno}}", odno).Replace("{{orderdate}}", date).Replace("{{packagedetails}}", tbl_dtl).Replace("{{totaltax}}", tx_amt).Replace("{{totalpbl}}", pbl_amt).Replace("{{address}}", address).Replace("{{pin}}", pin).Replace("{{mobile}}", mobile).Replace("{{discount}}", disc_amt);
                                sendMail1("ORD_CONF", data2.ToList()[0].email, "Order Confirmation", names, msgbody);

                                try
                                {
                                    var pushnot = (from c in DbContext.tbl_DC_Registration.Where(x => x.Regd_ID == data2.ToList()[0].Regd_ID)

                                                   select new { c.Regd_ID, c.Device_id }).FirstOrDefault();
                                    string body = "ordrid#{{orderid}}# Hello {{name}} ! Your {{pkgname}} is confirmed . Thank you  so much for choosing  DIGICHAMPS";
                                    string msg = body.ToString().Replace("{{name}}", data2.ToList()[0].firstname).Replace("{{orderid}}", ord_id.ToString());
                                    if (pushnot != null)
                                    {
                                        if (pushnot.Device_id != null)
                                        {
                                            var note = new PushNotiStatus(msg, pushnot.Device_id);
                                        }
                                    }
                                }
                                catch (Exception)
                                {


                                }
                            }
                            Session["ordid"] = null;
                            Session["disc"] = null;
                        }
                        else
                        {
                            return RedirectToAction("studentdashboard");
                        }
                    }
                    else
                    {
                        return RedirectToAction("Logout");
                    }
                }
                else
                {
                    TempData["errormessgae"] = "Payment failed.";
                }
                //empData["message"] = "Success";
                return View();
                //---------------------end region-----------------------------------------

            }
            catch (Exception ex)
            {
                TempData["errormessgae"] = "Oops,Something went wrong "+ ex.Message.ToString();
            }
            
            return View();
        }

        public ActionResult CancelPayment()
        {
            try
            {
                if (Session["USER_NAME"] != null)
                {
                    if (Session["ordid"] != null)
                    {
                        string u_name = Session["USER_NAME"].ToString();

                        var data1 = DbContext.tbl_DC_Registration.Where(x => x.Mobile == u_name && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();

                        ViewBag.mobile = data1.Mobile;
                        ViewBag.custmer_name = data1.Customer_Name;

                        var data = (from a in DbContext.tbl_DC_Registration.Where(x => x.Mobile == u_name && x.Is_Active == true && x.Is_Deleted == false)
                                    join b in DbContext.tbl_DC_Cart.Where(x => x.In_Cart == true && x.Is_Paid == false && x.Is_Active == true && x.Is_Deleted == false)
                                     on a.Regd_ID equals b.Regd_ID
                                    join c in DbContext.tbl_DC_Package.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                        on b.Package_ID equals c.Package_ID
                                    select new DigiChampCartModel
                                    {
                                        firstname = a.Customer_Name,
                                        Package_Name = c.Package_Name,
                                        Subscripttion_Period = c.Subscripttion_Period,
                                        Price = c.Price,
                                        email = a.Email,
                                        Order_ID = b.Order_ID,
                                        Order_No = b.Order_No,
                                        Cart_ID = b.Cart_ID
                                    }).ToList();
                        //ViewBag.cartitems = data;

                        if (data.Count == 0)
                        {
                            ViewBag.cartitems = null;
                            TempData["emptycart"] = "Your cart is empty";
                        }
                        else
                        {
                            ViewBag.cartitems = data;
                            decimal tax = taxcalculate();
                            TempData["totalitem"] = Convert.ToInt32(data.Count);
                            decimal totalprice = Math.Round((Convert.ToDecimal(Session["price"])), 2);
                            decimal totalpaybletax = (tax * totalprice) / 100;
                            decimal totalpayblamt = Math.Round((totalprice + totalpaybletax), 2);
                            TempData["payblamt"] = totalpayblamt;
                            Session["payamount"] = totalpayblamt;
                            Session["ordid"] = data.ToList()[0].Order_ID;
                            Session["ordno"] = data.ToList()[0].Order_No;
                            TempData["price"] = Math.Round(totalprice, 2);
                            Session["price"] = Math.Round(totalprice, 2);
                            TempData["Tax"] = Convert.ToDecimal(TempData["payblamt"]) - Convert.ToDecimal(TempData["price"]);
                        }
                        var getall = DbContext.SP_DC_Get_maildetails("ORD_CANC").FirstOrDefault();
                        string name = data.ToList()[0].firstname;
                        string msgbody = getall.EmailConf_Body.ToString().Replace("{{name}}", name);
                        sendMail1("ORD_CANC", data.ToList()[0].email, "Order Cancelation", name, msgbody);
                        Session["ordid"] = null;
                    }
                    else
                    {
                        return RedirectToAction("studentdashboard");
                    }
                }
                else
                {
                    return RedirectToAction("Logout");
                }
            }
            catch (Exception ex)
            {
                ex.Message.ToString();
            }
            return View();
        }

        [ActionName("paymentinfo")]
        public void paymentinfo()
        {



            string workingKey = "AF533D01B8E98D8ED2E4A950AAA8E45B";//put in the 32bit alpha numeric key in the quotes provided here
            CCACrypto ccaCrypto = new CCACrypto();
            string encResponse = ccaCrypto.Decrypt(Request.Form["encResp"], workingKey);
            NameValueCollection Params = new NameValueCollection();
            string[] segments = encResponse.Split('&');
            foreach (string seg in segments)
            {
                string[] parts = seg.Split('=');
                if (parts.Length > 0)
                {
                    string Key = parts[0].Trim();
                    string Value = parts[1].Trim();
                    Params.Add(Key, Value);
                }
            }
            var param1 = string.Empty;

            Response.Write("<table>");
            for (int i = 0; i < Params.Count; i++)

            {
                Response.Write("<tr><td>" + Params.Keys[i] + " </td><td> " + Params[i] + "</td></tr>");
            }
            Response.Write("</table>");


        }
        public ActionResult GetRSA()
        {


            try

            {

                //string  vParams = "access_code=AVOG66DH75CL93GOLC&merchant_id=108133&order_id= " + ord + "&amount=" + price + "&currency=INR&";

                string queryUrl = "https://secure.ccavenue.com/transaction/getRSAKey";
                string vParams = "";
                foreach (string key in Request.Params.AllKeys)
                {
                    vParams += key + "=" + Request[key] + "&";
                }
                // Url Connection
                String message = postPaymentRequestToGateway(queryUrl, vParams);
                //msg = message;
                return Content(message);
                //return Json(message,JsonRequestBehavior.AllowGet);
            }
            catch (Exception exp)
            {
                //msg = exp;
                Response.Write("Exception " + exp);

            }
            return View();

        }
        private string postPaymentRequestToGateway(String queryUrl, String urlParam)
        {
            String message = "";
            try
            {
                StreamWriter myWriter = null;// it will open a http connection with provided url
                WebRequest objRequest = WebRequest.Create(queryUrl);//send data using objxmlhttp object
                objRequest.Method = "POST";
                //objRequest.ContentLength = TranRequest.Length;
                objRequest.ContentType = "application/x-www-form-urlencoded";//to set content type
                myWriter = new System.IO.StreamWriter(objRequest.GetRequestStream());
                myWriter.Write(urlParam);//send data
                myWriter.Close();//closed the myWriter object

                // Getting Response
                System.Net.HttpWebResponse objResponse = (System.Net.HttpWebResponse)objRequest.GetResponse();//receive the responce from objxmlhttp object 
                using (System.IO.StreamReader sr = new System.IO.StreamReader(objResponse.GetResponseStream()))
                {
                    message = sr.ReadToEnd();
                }
            }
            catch (Exception exception)
            {
                //  Console.Write("Exception occured while connection." + exception);
                message = exception.Message;
            }
            return message;
        }


        [HttpPost]
        public ActionResult Deletedefaultcop()
        {
            string[] arr = new string[3];
            try
            {
                decimal totalprice = 0;
                //decimal totalprice 
                decimal totalpaybletax = 0;
                decimal totalpayblamt = 0;
                string u_name = Session["USER_NAME"].ToString();
                decimal tax = taxcalculate();
                var data = (from a in DbContext.tbl_DC_Registration.Where(x => x.Mobile == u_name && x.Is_Active == true && x.Is_Deleted == false)
                            join b in DbContext.tbl_DC_Cart.Where(x => x.In_Cart == true && x.Is_Paid == false && x.Is_Active == true && x.Is_Deleted == false)
                             on a.Regd_ID equals b.Regd_ID
                            join c in DbContext.tbl_DC_Package.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                on b.Package_ID equals c.Package_ID
                            select new DigiChampCartModel
                            {
                                Regd_ID = a.Regd_ID,
                                firstname = a.Customer_Name,
                                phone = a.Mobile,
                                email = a.Email,
                                Package_Name = c.Package_Name,
                                Package_ID = c.Package_ID,
                                Subscripttion_Period = c.Subscripttion_Period,
                                Price = c.Price,
                                Is_Offline = c.Is_Offline,
                                Order_ID = b.Order_ID,
                                Order_No = b.Order_No,
                                Cart_ID = b.Cart_ID
                            }).ToList();
                decimal totalprice1 = data.ToList().Where(x => x.Is_Offline == false || x.Is_Offline == null).Select(c => (decimal)c.Price).Sum();

                var data2 = (from x in data.ToList().Where(x => x.Is_Offline == true)
                             join y in DbContext.tbl_DC_Package_Period.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                             on x.Package_ID equals y.Package_ID
                             select new DigiChampCartModel
                             {
                                 Is_Offline = x.Is_Offline,
                                 Package_ID = x.Package_ID,
                                 Price = x.Price,
                                 Excluded_Price = y.Excluded_Price
                             }).ToList();
                if (data2.Count > 0)
                {
                    decimal totalprice2 = data2.ToList().Where(x => x.Is_Offline == true).Select(c => (decimal)c.Price).Sum();
                    decimal totalprice3 = data2.ToList().Where(x => x.Is_Offline == true).Select(c => (decimal)c.Excluded_Price).Sum();
                    totalprice = Math.Round(Convert.ToDecimal(totalprice1 + totalprice2 + totalprice3), 2);
                    //totalprice = Convert.ToDecimal(Session["price1"]);
                    totalpaybletax = Math.Round(((tax * totalprice) / 100), 2);
                    totalpayblamt = Math.Round((totalprice + totalpaybletax), 2);
                    TempData["payblamt"] = totalpayblamt;
                    Session["payamount"] = totalpayblamt;
                    TempData["price"] = Math.Round(totalprice, 2);
                    Session["prc"] = TempData["price"];
                    TempData["Tax"] = Convert.ToDecimal(TempData["payblamt"]) - Convert.ToDecimal(TempData["price"]);
                }
                else
                {
                    totalprice = Math.Round(Convert.ToDecimal(totalprice1), 2);
                    totalpaybletax = Math.Round(((tax * totalprice) / 100), 2);
                    totalpayblamt = Math.Round((totalprice + totalpaybletax), 2);
                    TempData["payblamt"] = totalpayblamt;
                    Session["payamount"] = totalpayblamt;
                    TempData["price"] = Math.Round(totalprice, 2);
                    Session["prc"] = TempData["price"];
                    TempData["Tax"] = Convert.ToDecimal(TempData["payblamt"]) - Convert.ToDecimal(TempData["price"]);
                }
                Session["value"] = 2;
                arr[0] = totalprice.ToString();
                arr[1] = totalpaybletax.ToString();
                arr[2] = totalpayblamt.ToString();
                return Json(arr, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                arr[4] = "error";
                return Json(arr, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public ActionResult checkcoupon(string id)
        {
            try
            {
                var cop = DbContext.tbl_DC_CouponCode.Where(x => x.Coupon_Code == id && x.Valid_From <= today && x.Valid_To >= today && x.Is_Active == true && x.Is_Delete == false).FirstOrDefault();
                if (cop != null)
                {
                    if (cop.Pricerange_From != null && cop.Pricertange_To != null)
                    {
                        if (Session["prc"] != null)
                        {
                            decimal total = Convert.ToDecimal(Session["prc"]);
                            if (cop.Is_Default == false && total >= cop.Pricerange_From && total <= cop.Pricertange_To)
                            {
                                return Json(cop, JsonRequestBehavior.AllowGet);
                            }
                            else if (cop.Is_Default == true)
                            {
                                return Json(cop, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                return Json("", JsonRequestBehavior.AllowGet);
                            }
                        }
                        else
                        {
                            return Json("", JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        return Json(cop, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json("", JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }

        #endregion
        
        #region-------------------------------------------------Order details-----------------------------------------------------------
        [HttpGet]
        public ActionResult orderdetails()
        {
            ViewBag.Breadcrumb = "order details";
            if (Session["USER_CODE"] != null)
            {
                int id = Convert.ToInt32(Convert.ToString(Session["USER_CODE"]).Substring(1));
                IEnumerable<SP_DC_Order_Details_Result> ord_detail = DbContext.SP_DC_Order_Details(id).ToList();
                return View(ord_detail);
            }
            else {
                return RedirectToAction("Logout");
            }
        }
        #endregion

        #region-------------------------------------------------Error Exception---------------------------------------------------------

        public JsonResult GetNotification()
        {
            var notificationRegisterTime = Session["LastUpdated"] != null ? Convert.ToDateTime(Session["LastUpdated"]) : DateTime.Now;
            NotificationComponent NC = new NotificationComponent();
            var list = NC.GetContacts(notificationRegisterTime);
            //update session here for get only new added contacts (notification)
            Session["LastUpdate"] = DateTime.Now;
            return new JsonResult { Data = list, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
        
        //protected override void OnException(ExceptionContext filterContext)
        //{
        //    Exception e = filterContext.Exception;
        //    //Log Exception e
        //    filterContext.ExceptionHandled = true;
        //    filterContext.Result = new ViewResult()
        //    {
        //        ViewName = "Logout"
        //    };
        //}
        #endregion

        #region --------------------------------------------------Offline test-----------------------------------------------------------
        [ActionName("offline-exam")]
        public ActionResult offline_exam()
        {
            try
            {
                int id = 0;
                if (Session["USER_CODE"] != null)
                {
                    id = Convert.ToInt32(Convert.ToString(Session["USER_CODE"]).Substring(1));
                }
                var offline = DbContext.VW_Offline_Student_Orders.Where(x => x.Regd_ID == id).FirstOrDefault();
                if (offline != null)
                {
                    var Isassigned = DbContext.SP_DC_Assign_Batch_Details().Where(x => x.Regd_ID == id).FirstOrDefault();
                    if (Isassigned != null)
                    {
                        string id1 = Convert.ToString(id);
                        ViewBag.test1 = DbContext.SP_Offline_Exams_Count(id1, 1).FirstOrDefault();
                        ViewBag.test2 = DbContext.SP_Offline_Exams_Count(id1, 3).FirstOrDefault();

                        ViewBag.test3 = DbContext.SP_Offline_Exams_Count(id1, 2).FirstOrDefault();
                        ViewBag.ofline = null;
                    }
                }

                else
                {
                    ViewBag.ofline = "blank";

                }
            }
            catch
            {
                return View();
            }
            return View();
        }

        public ActionResult OfflineTest(int id)
        {
            try
            {
                int sid = 0;
                if (Session["USER_CODE"] != null)
                {
                    sid = Convert.ToInt32(Convert.ToString(Session["USER_CODE"]).Substring(1));
                    var batchassgn = DbContext.SP_DC_Assign_Batch_Details().Where(x => x.Regd_ID == sid).FirstOrDefault();
                    if (batchassgn != null)
                    {
                        ViewBag.exams = DbContext.SP_DC_Offline_exams(sid, id).Select(x => x.Subject_Id).Distinct().ToList();
                        ViewBag.exams1 = DbContext.SP_DC_Offline_exams(sid, id).ToList();
                        var data = DbContext.SP_DC_Offline_exams(sid, id).ToList();
                        ViewBag.retestassign = DbContext.SP_DC_TestAssign(1).FirstOrDefault();

                       

                        if (id == 1)
                        {
                            ViewBag.exam_name = "Pre-requisite Test";
                            ViewBag.examtypeid = 1;
                          
                            
                        }
                        else if (id == 2)
                        {
                            ViewBag.exam_name = "Re-Test";
                            ViewBag.examtypeid = 2;
                        }
                        else if (id == 3)
                        {
                            ViewBag.exam_name = "Practice Test";
                            ViewBag.examtypeid = 3;
                        }
                    }
                }
            }
            catch
            {
                return View();
            }
            return View();
        }
        #endregion

       #region--------------------------------------------------Guardian Profile--------------------------------------------------------

        [HttpPost]
        public JsonResult GuardianCredential(View_All_Student_Details parents)
        {
            string message = string.Empty;

            try
            {
                if (Session["USER_CODE"] != null)
                {
                    //int rgt_id = Convert.ToInt32(Session["Reg_Id"]);
                    int sreg_id = Convert.ToInt32(Convert.ToString(Session["USER_CODE"]).Substring(1));
                    tbl_DC_Registration prnt_details = DbContext.tbl_DC_Registration.Where(x => x.Regd_ID == sreg_id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();

                    if (prnt_details.Parent_Id == null)
                    {
                        prnt_details.Parent_Id = sreg_id;
                        prnt_details.Praent_Name = parents.Praent_Name;
                        prnt_details.Parent_Mail = parents.Parent_Mail;
                        prnt_details.P_Relation = parents.P_Relation;
                        prnt_details.Parent_Mobile = parents.Parent_Mobile;
                        prnt_details.P_Relation = parents.P_Relation;
                        DbContext.Entry(prnt_details).State = EntityState.Modified;
                        DbContext.SaveChanges();
                        message = "1";

                        var sms_obj = DbContext.View_DC_SMS_API.Where(x => x.Sms_Alert_Name == "PARENTREG").FirstOrDefault();
                        if (sms_obj != null)
                        {
                            string msg = sms_obj.Sms_Body;
                            var regex = new Regex(Regex.Escape("{{name}}"));
                            var newText = regex.Replace(msg, "" + prnt_details.Praent_Name + "", 8);

                            string baseurl = "" + sms_obj.Api_Url + "?uname=" + sms_obj.Username + "&pass=" + sms_obj.Password + "&send=" + sms_obj.Sender_Type + "&dest=" + parents.Parent_Mobile.Trim() + "&msg=" + newText.ToString() + "&priority=1";

                            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(baseurl);
                            //Get response from the SMS Gateway Server and read the answer
                            HttpWebResponse myResp = (HttpWebResponse)myReq.GetResponse();
                            System.IO.StreamReader respStreamReader = new System.IO.StreamReader(myResp.GetResponseStream());
                            string responseString = respStreamReader.ReadToEnd();
                            respStreamReader.Close();
                            myResp.Close();
                        }
                        int pid = Convert.ToInt32(prnt_details.Parent_Id);
                        tbl_DC_USER_SECURITY parent = new tbl_DC_USER_SECURITY();
                        parent.USER_NAME = parents.Parent_Mobile;
                        parent.ROLE_CODE = "P";
                        parent.ROLE_TYPE = 5;
                        parent.STATUS = "A";
                        parent.IS_ACCEPTED = true;
                        parent.USER_CODE = "P0" + Convert.ToString(pid);
                        //------random password-------
                        string new_pass_word = CreateRandomPassword(8);
                        if (parents.Parent_Mail != null && parents.Parent_Mail != "")
                        {
                            GurdianMail("P_REG", parents.Parent_Mail, parents.Praent_Name, parents.Parent_Mobile, new_pass_word);
                        }   
                        parent.PASSWORD = DigiChampsModel.Encrypt_Password.HashPassword(new_pass_word).ToString();
                        DbContext.tbl_DC_USER_SECURITY.Add(parent);
                        DbContext.SaveChanges();
                    }
                    else
                    {
                        prnt_details.Praent_Name = parents.Praent_Name;
                        prnt_details.Parent_Mail = parents.Parent_Mail;
                        prnt_details.P_Relation = parents.P_Relation;
                        DbContext.Entry(prnt_details).State = EntityState.Modified;
                        DbContext.SaveChanges();
                        message = "1";
                    }
                }
            }

            catch (Exception ex)
            {
                message = "Something went wrong.";
                return Json(message, JsonRequestBehavior.AllowGet);
            }
            return Json(message, JsonRequestBehavior.AllowGet);
        }

        public bool GurdianMail(string parameter, string email, string name, string username, string password1)
        {
            var getall = DbContext.SP_DC_Get_maildetails(parameter).FirstOrDefault();
            string eidTo = email;
            string mailtoshow = getall.SMTP_Email.ToString();
            string eidFrom = getall.SMTP_User.ToString();
            string password = getall.SMTP_Pwd.ToString();
            string msgsub = "Welcome to Digichamps";
            string hostname = getall.SMTP_HostName;
            string portname = getall.SMTP_Port.ToString();
            bool ssl_tof = true;
            string msgbody = getall.EmailConf_Body.ToString().Replace("{{name}}", name).Replace("{{username}}", username).Replace("{{password}}", password1);
            MailMessage greetings = new MailMessage();
            SmtpClient smtp = new SmtpClient();
            try
            {
                greetings.From = new MailAddress(mailtoshow, "DigiChamps");//sendername
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
        #endregion--------------------------------------------------------------------------------------------------------------

       #region---------------------------------------------------- Feedback ------------------------------------------------------------
        [HttpGet]
        public ActionResult feedback()
        {
            try
            {
                if (Session["USER_CODE"] != null)
                {
                    int _student_id = Convert.ToInt32(Convert.ToString(Session["USER_CODE"]).Substring(1));
                    string ucode = Session["USER_CODE"].ToString();
                    ViewBag.teacher = new SelectList(DbContext.SP_DC_Feedback_Teacher(_student_id), "Teach_ID", "Teacher_Name");
                    ViewBag.chapter = new SelectList(DbContext.SP_DC_Feedback_Chapter(_student_id), "Chapter_ID", "Chapter");
                    ViewBag.feed = DbContext.tbl_DC_Feedback_Teacher.Where(x => x.Regd_Id == _student_id && x.Usercode == ucode && x.Feedback != null).Select(x => x.Feedback).FirstOrDefault();
                }
                else
                {
                    return RedirectToAction("Logout");
                }
            }
            catch
            {

            }
            return View();
        }

        [HttpPost]
        public JsonResult Rate_teacher(string rate1, int id, string feedback)
        {
            bool msg = false;
            try
            {
                if (Session["USER_CODE"] != null)
                {
                    int _student_id = Convert.ToInt32(Convert.ToString(Session["USER_CODE"]).Substring(1));
                    string ucode = Session["USER_CODE"].ToString();
                    var rate = Convert.ToDecimal(rate1);

                    var teacher = DbContext.tbl_DC_Teacher.Where(x => x.Teach_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    if (teacher != null)
                    {
                        var rat = DbContext.tbl_DC_Feedback_Teacher.Where(x => x.Teach_ID == id && x.Regd_Id == _student_id && x.Usercode == ucode).FirstOrDefault();
                        if (rat != null)
                        {
                            rat.Teach_Rating = rate;
                            if (rat.Feedback != null)
                            {
                                rat.Feedback = feedback;
                            }
                            else
                            {
                                var rat1 = DbContext.tbl_DC_Feedback_Teacher.Where(x => x.Regd_Id == _student_id && x.Feedback != null && x.Usercode == ucode).FirstOrDefault();
                                if (rat1 != null)
                                {
                                    rat1.Feedback = feedback;
                                    DbContext.Entry(rat1).State = EntityState.Modified;
                                    DbContext.SaveChanges();
                                }
                                else
                                {
                                    rat.Feedback = feedback;
                                }
                            }
                            DbContext.Entry(rat).State = EntityState.Modified;
                            DbContext.SaveChanges();
                            msg = true;
                        }
                        else
                        {
                            tbl_DC_Feedback_Teacher ar = new tbl_DC_Feedback_Teacher();
                            ar.Teach_Rating = rate;
                            int s_id = Convert.ToInt32(Convert.ToString(Session["USER_CODE"]).Substring(1));
                            ar.Usercode = ucode;
                            ar.Regd_Id = s_id;
                            ar.Teach_ID = id;
                            var rat1 = DbContext.tbl_DC_Feedback_Teacher.Where(x => x.Regd_Id == _student_id && x.Feedback != null && x.Usercode == ucode).FirstOrDefault();
                            if (rat1 != null)
                            {
                                rat1.Feedback = feedback;
                                DbContext.Entry(rat1).State = EntityState.Modified;
                                DbContext.SaveChanges();
                            }
                            else
                            {
                                ar.Feedback = feedback;
                            }
                            DbContext.tbl_DC_Feedback_Teacher.Add(ar);
                            DbContext.SaveChanges();
                            if (teacher.TotalRaters != null)
                            {
                                teacher.TotalRaters += 1;
                            }
                            else
                            {
                                teacher.TotalRaters = 1;
                            }
                            DbContext.Entry(teacher).State = EntityState.Modified;
                            DbContext.SaveChanges();
                            msg = true;
                        }
                    }
                }
            }
            catch
            {
                msg = false;
            }
            return Json(msg);
        }

        [HttpPost]
        public JsonResult Rate_chapter(string rate1, int id)
        {
            bool msg = false;
            try
            {
                if (Session["USER_CODE"] != null)
                {
                    int _student_id = Convert.ToInt32(Convert.ToString(Session["USER_CODE"]).Substring(1));
                    string ucode = Session["USER_CODE"].ToString();
                    var rate = Convert.ToDecimal(rate1);

                    var chapter = DbContext.tbl_DC_Chapter.Where(x => x.Chapter_Id == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    if (chapter != null)
                    {
                        var rat = DbContext.tbl_DC_Feedback_Chapter.Where(x => x.Chapter_Id == id && x.Regd_Id == _student_id && x.Usercode == ucode).FirstOrDefault();
                        if (rat != null)
                        {
                            rat.Ch_Rating = rate;
                            DbContext.Entry(rat).State = EntityState.Modified;
                            DbContext.SaveChanges();
                            msg = true;
                        }
                        else
                        {
                            tbl_DC_Feedback_Chapter ar = new tbl_DC_Feedback_Chapter();
                            ar.Ch_Rating = rate;
                            int s_id = Convert.ToInt32(Convert.ToString(Session["USER_CODE"]).Substring(1));
                            ar.Usercode = ucode;
                            ar.Regd_Id = s_id;
                            ar.Chapter_Id = id;
                            DbContext.tbl_DC_Feedback_Chapter.Add(ar);
                            DbContext.SaveChanges();
                            if (chapter.TotalRaters != null)
                            {
                                chapter.TotalRaters += 1;
                            }
                            else
                            {
                                chapter.TotalRaters = 1;
                            }
                            DbContext.Entry(chapter).State = EntityState.Modified;
                            DbContext.SaveChanges();
                            msg = true;
                        }
                    }
                }
            }
            catch
            {
                msg = false;
            }
            return Json(msg);
        }

        [HttpPost]
        public JsonResult Check_Rating(int id, string condition)
        {
            decimal? msg = 0;
            try
            {
                if (id != 0)
                {
                    if (condition == "teacher")
                    {
                        var teacher = DbContext.tbl_DC_Teacher.Where(x => x.Teach_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        if (teacher != null)
                        {
                            if (Session["USER_CODE"] != null)
                            {
                                int _student_id = Convert.ToInt32(Convert.ToString(Session["USER_CODE"]).Substring(1));
                                string ucode = Session["USER_CODE"].ToString();
                                var rat = DbContext.tbl_DC_Feedback_Teacher.Where(x => x.Teach_ID == id && x.Regd_Id == _student_id && x.Usercode == ucode).FirstOrDefault();
                                if (rat != null)
                                {
                                    msg = rat.Teach_Rating;
                                }
                                else
                                {
                                    msg = 0;
                                }
                            }
                            else
                            {
                                msg = 0;
                            }

                        }
                    }
                    else if (condition == "chapter")
                    {
                        var chapter = DbContext.tbl_DC_Chapter.Where(x => x.Chapter_Id == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        if (chapter != null)
                        {
                            if (Session["USER_CODE"] != null)
                            {
                                int _student_id = Convert.ToInt32(Convert.ToString(Session["USER_CODE"]).Substring(1));
                                string ucode = Session["USER_CODE"].ToString();
                                var rat = DbContext.tbl_DC_Feedback_Chapter.Where(x => x.Chapter_Id == id && x.Regd_Id == _student_id && x.Usercode == ucode).FirstOrDefault();
                                if (rat != null)
                                {
                                    msg = rat.Ch_Rating;
                                }
                                else
                                {
                                    msg = 0;
                                }
                            }
                            else
                            {
                                msg = 0;
                            }

                        }
                    }
                }
            }
            catch
            {
                msg = 0;
            }
            return Json(msg);
        }
        #endregion

       #region---------------------------------------------------Attendance Report------------------------------------------------------
        [HttpGet]
        public ActionResult Attendance_report()
        {
            try
            {
                if (Session["USER_CODE"] != null)
                {
                    int regdid = Convert.ToInt32(Convert.ToString(Session["USER_CODE"]).Substring(1));
                    ViewBag.Stu_Attendance = DbContext.Vw_Attendance_Report.Where(x => x.Regd_ID == regdid).ToList();
                }
                else
                {
                    return RedirectToAction("logout");
                }
            }
            catch { }
            return View();
        }
        [HttpPost]
        public ActionResult Attendance_report(string frm_dt, string to_dt)
        {
            try
            {
                if (Session["USER_CODE"] != null)
                {
                    int regdid = Convert.ToInt32(Convert.ToString(Session["USER_CODE"]).Substring(1));
                    if (frm_dt != "" && to_dt != "")
                    {
                        if (Convert.ToDateTime(frm_dt) <= today.Date && Convert.ToDateTime(to_dt) <= today.Date)
                        {
                            string fdtt = frm_dt + " 00:00:00 AM";
                            string tdtt = to_dt + " 23:59:59 PM";

                            DateTime from_date1 = Convert.ToDateTime(fdtt);
                            DateTime to_date1 = Convert.ToDateTime(tdtt);

                            var attendance_status = (from c in DbContext.Vw_Attendance_Report where c.Attendance_Date >= from_date1 && c.Attendance_Date <= to_date1 && c.Regd_ID == regdid select c).OrderByDescending(c => c.Attendance_Date);
                            ViewBag.Stu_Attendance = attendance_status.ToList();
                        }
                        else
                        {
                            ViewBag.Breadcrumb = "Attendance Report";
                            TempData["ErrorMessage"] = "From date should be less or equal to todate.";
                        }
                    }
                    else
                    {
                        ViewBag.Breadcrumb = "Attendance Report";
                        TempData["ErrorMessage"] = "Please Select Dates.";
                    }
                }
                else { return RedirectToAction("Logout"); }
            }

            catch (Exception ex)
            {
                ex.Message.ToString();
            }

            return View();
        }
        #endregion

       #region------------------------------------------------------Time table----------------------------------------------------------
       [HttpGet]
       public ActionResult TimeTable_BatchWise()
        {
            if (Session["USER_CODE"] != null)
            {
                int studentid = Convert.ToInt32(Convert.ToString(Session["USER_CODE"]).Substring(1));

                var data = (from a in DbContext.tbl_DC_Batch.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                            join b in DbContext.tbl_DC_Batch_Assign.Where(x => x.Is_Active == true && x.Is_Deleted == false) on a.Batch_Id equals b.Batch_Id
                            join c in DbContext.tbl_DC_Student_Batch_Assign.Where(x => x.Regd_Id == studentid && x.Is_Active == true && x.Is_Deleted == false) on b.Batch_Assign_Id equals c.Batch_Assign_Id
                            select new DigiChampsModel.DigiChampsBatchModel
                            {
                                Batch_Name = a.Batch_Name,
                                Batch_From_Time = a.Batch_From_Time,
                                Batch_To_Time = a.Batch_To_Time,
                                Batch_Days = a.Batch_Days
                            }).ToList();
                ViewBag.TimeTable_Details = data;
            }
            else {
                return RedirectToAction("Logout");
            }
            return View();
        }
        #endregion


        #region----------------------------------------------Offer------------------------------------------------------------
        [HttpGet]
        public ActionResult offer()
        {
            try
            {
                ViewBag.offer = DbContext.tbl_DC_Banner.Where(x => x.Is_Active == true && x.Is_Deleted == false).ToList();

            }
            catch (Exception ex)
            {

            }
            return View();
        }

        #endregion
        #region----------------------------------------------About us------------------------------------------------------------

        [HttpGet]
        public ActionResult Aboutus()
        {
            ViewBag.Board_Id = new SelectList(DbContext.tbl_DC_Board.Where(x => x.Is_Active == true && x.Is_Deleted == false), "Board_Id", "Board_Name");
            ViewBag.secureqsn = new SelectList(DbContext.tbl_DC_Security_Question.Where(x => x.Is_Active == true && x.Is_Deleted == false), "Security_Question_ID", "Security_Question");
            ViewBag.about = DbContext.tbl_DC_Aboutus.Where(x => x.Is_Active == true && x.Is_Deleted == false).ToList();
            return View();
        }
        #endregion
    }
}
