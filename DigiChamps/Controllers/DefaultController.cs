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
    public class DefaultController : Controller
    {
        DigiChampsEntities DbContext = new DigiChampsEntities();
        DateTime today = DigiChampsModel.datetoserver();

        [HttpGet]
        public ActionResult Index()
        {
            ViewBag.Board_Id = new SelectList(DbContext.tbl_DC_Board.Where(x => x.Is_Active == true && x.Is_Deleted == false), "Board_Id", "Board_Name");
            ViewBag.secureqsn = new SelectList(DbContext.tbl_DC_Security_Question.Where(x => x.Is_Active == true && x.Is_Deleted == false), "Security_Question_ID", "Security_Question");
            if (Session["USER_CODE"] != null)
            {
                if (Session["ROLE_CODE"].ToString() == "C")
                {
                    return RedirectToAction("studentdashboard", "student");
                }
                else
                {
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
                                else
                                {
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
                        sb.Append("<strong>Mr/Ms " + fname + " " + lname + "'</strong></p>");
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
                   bool chk= obj.submitQueryAPI(email,"",fname,lname);

                    return Json(msg, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    msg = "1";
                }

            }
            catch (Exception)
            {
                msg = "3";

            }
            return Json(msg, JsonRequestBehavior.AllowGet);
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

                        }else { fname = names[0]; }
                      
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
            int studentid = Convert.ToInt32(Convert.ToString(Session["USER_CODE"]).Substring(1));
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
            List<SP_DC_Askdoubt_Chapter_Result> states = DbContext.SP_DC_Askdoubt_Chapter(studentid, brdId, ClsId, SubId).ToList();
            states.ForEach(x =>
            {
                ChaptNames.Add(new SelectListItem { Text = x.Chapter_Name, Value = x.Chapter_ID.ToString() });
            });
            //ViewBag.state = new SelectList(dbContext.tbl_JV_State.Where(x => x.FK_Country_ID == conId && x.Is_Active == true && x.Is_Deleted == false), "PK_State_ID", "State_Name");

            return Json(ChaptNames, JsonRequestBehavior.AllowGet);
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
                    if (notyid != null)
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
            catch
            {


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
                    noty = DbContext.tbl_DC_Notification.Where(x => x.Regdno == _student_id && x.Is_Clicked == false).OrderByDescending(x => x.Inserted_Date).ToList();
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
                if (Session["sessionid"] != null)
                {
                    int sid = Convert.ToInt32(Session["sessionid"]);
                    bool get_logdata = Convert.ToBoolean(DbContext.tbl_DC_LoginStatus.Where(x => x.id == sid).Select(x => x.Status).FirstOrDefault());

                    if (get_logdata == true)
                    {
                        msg = 1;
                    }
                }
                else if (Session["USER_CODE"] != null)
                {
                    string s = Session["USER_CODE"].ToString().Trim();
                    var get = DbContext.tbl_DC_LoginStatus.Where(x => x.Login_ID == s && x.Status == true).FirstOrDefault();
                    if (get != null)
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
                if (Session["sessionid"] != null)
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
            string message = string.Empty;
            var found_id = DbContext.tbl_DC_LoginStatus.Where(x => x.Login_ID == usercode).FirstOrDefault();
            DateTime _dt_log = Convert.ToDateTime(Session["Time_for_login"]);
            DateTime _dt_db = Convert.ToDateTime(found_id.Login_DateTime);
            if (_dt_log.ToLongTimeString() == _dt_db.ToLongTimeString())
            {
                message = "i";
            }
            else
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

    }
}
