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
using System.Drawing;
using System.Drawing.Imaging;
using System.Net.Mail;

namespace DigiChamps.Controllers
{
    public class TeacherController : Controller
    {
        DigiChampsEntities DbContext = new DigiChampsEntities();
        DateTime today = DigiChampsModel.datetoserver().Date;

        #region-------------------------Teacher 1ststep-------------------------------
        [HttpGet]
        public ActionResult Index()
        {
            return RedirectToAction("Login");
        }
        [HttpGet]
        public ActionResult Login()
        {
            if (Session["USER_CODE"] != null)
            {
                if (Session["ROLE_CODE"].ToString() == "T")
                {
                return RedirectToAction("TeacherDashboard");
                }
                else
                {
                    return RedirectToAction("Logout");
                }
            }
            else
            {
                if (Request.Cookies["Login"] != null)
                {
                    ViewBag.user = Request.Cookies["Login"].Values["user"];
                    ViewBag.Password = Request.Cookies["Login"].Values["Password"];
                }
                return View();
            }
        }
        [HttpPost]
        public ActionResult Login(string User_name, string password, string checkbox)
        {

            try
            {
                overdue();
                string pass_word = DigiChampsModel.Encrypt_Password.HashPassword(password);

                var obj = DbContext.tbl_DC_USER_SECURITY.Where(x => x.USER_NAME == User_name && x.PASSWORD == pass_word && x.IS_ACCEPTED == true).FirstOrDefault();

                if (obj != null)
                {
                    if (obj.ROLE_CODE == "T")
                    {
                        var teacher = DbContext.tbl_DC_Teacher.Where(x => x.Email_ID == obj.USER_NAME && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        if (teacher != null)
                        {
                            Session["USER_CODE"] = obj.USER_CODE;
                            TempData["USER_CODE"] = obj.USER_CODE;
                            Session["USER_NAME"] = obj.USER_NAME;
                            Session["ROLE_CODE"] = obj.ROLE_CODE;
                            Session["Time"] = today.ToShortTimeString();
                            Session["pfimage"] = teacher.Image;

                            FormsAuthentication.RedirectFromLoginPage(obj.USER_NAME, false);
                            if (checkbox != null)
                            {
                                HttpCookie cookie = new HttpCookie("Login");
                                cookie.Values.Add("user", User_name);
                                cookie.Values.Add("Password", password);
                                cookie.Expires = DateTime.Now.AddDays(10);
                                Response.Cookies.Add(cookie);
                            }
                            return RedirectToAction("TeacherDashboard");
                        }
                        else {
                            TempData["ErrorMessage"] = "User Details does not exist, Please contact adminstrator.";
                            return View();
                        }
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Please login as a Teacher.";
                        return View();
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "Invalid user name or password.";
                    return View();
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return View();
        }
        public ActionResult Logout()
        {
            if (Session["USER_CODE"] != null)
            {
                logoutstatus(Convert.ToString(Session["USER_CODE"]));
            }
            Session.Abandon();
            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }
        [HttpGet]
        public ActionResult ForgotPassword()
        {
            return View();
        }
        [HttpPost]
        public JsonResult ForgotPassword(string email)
        {
            string message;
            try
            {
                var find_mail = DbContext.tbl_DC_USER_SECURITY.Where(x => x.USER_NAME == email && x.ROLE_CODE == "T").FirstOrDefault();
                if (find_mail != null)
                {
                    string password = CreateRandomPassword(6);
                    string t_password = DigiChampsModel.Encrypt_Password.HashPassword(password);

                    find_mail.PASSWORD = t_password;
                    DbContext.Entry(find_mail).State = EntityState.Modified;
                    DbContext.SaveChanges();
                    tbl_DC_Teacher obj = DbContext.tbl_DC_Teacher.Where(x => x.Email_ID == email).FirstOrDefault();
                    T_sendMail("T_F_PASS", email, password, email, obj.Teacher_Name);
                    message = "Login details successfully sent to the mailid.";
                }
                else
                {
                    message = "Mail id does not belongs to any teacher.";
                }
            }
            catch (Exception ex)
            {
                message = "Something went wrong.";
            }
            return Json(message);
        }
        public bool T_sendMail(string parameter, string username, string password1, string email, string name)
        {
            var getall = DbContext.SP_DC_Get_maildetails(parameter).FirstOrDefault();
            string eidTo = email;
            string mailtoshow = getall.SMTP_Email.ToString();
            string eidFrom = getall.SMTP_User.ToString();
            string Password = getall.SMTP_Pwd.ToString();
            string msgsub = "Digichamps Password Recovery";
            string hostname = getall.SMTP_HostName;
            string portname = getall.SMTP_Port.ToString();
            bool ssl_tof = true;
            string msgbody = getall.EmailConf_Body.ToString().Replace("{{name}}", name).Replace("{{username}}", username).Replace("{{password}}", password1);
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
                smtp.Credentials = new NetworkCredential(eidFrom, Password);//from(user)//password
                smtp.Send(greetings);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        
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
        [HttpGet]
        public ActionResult ChangePassword()
        {
            return View();
        }
        [HttpPost]
        public JsonResult ChangePassword(string Old_Password, string New_Password, string Conf_Password)
        {
            string message = string.Empty;
            if (Session["USER_CODE"] != null)
            {
                if (New_Password != "" && Conf_Password != "" && Old_Password != "")
                {

                    try
                    {
                        string u_code = Session["USER_CODE"].ToString();
                        string old_pass_word = DigiChampsModel.Encrypt_Password.HashPassword(Old_Password);
                        var data = DbContext.tbl_DC_USER_SECURITY.Where(x => x.USER_CODE == u_code && x.PASSWORD == old_pass_word).FirstOrDefault();

                        if (data != null)
                        {
                            if (New_Password == Conf_Password)
                            {
                                string new_pass_word = DigiChampsModel.Encrypt_Password.HashPassword(New_Password).ToString();
                                var obj = DbContext.tbl_DC_USER_SECURITY.Where(x => x.USER_CODE == u_code).FirstOrDefault();

                                obj.PASSWORD = new_pass_word;
                                DbContext.Entry(obj).State = EntityState.Modified;
                                DbContext.SaveChanges();
                                message = "0";
                            }
                            else
                            {
                                message = "1";
                            }
                        }
                        else
                        {
                            message = "2";
                        }
                    }
                    catch (Exception ex)
                    {
                        message = "3";
                        return Json(message, JsonRequestBehavior.AllowGet);
                    }

                }
                else
                {
                    message = "5";
                }
            }
            else
            {
                message = "4";
            }
            return Json(message, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult EditProfile()
        {
            return View();
        }
        #endregion

        #region -------------------------tecaherticket---------------------------------
        public ActionResult Viewticekts()
        {
            if (Session["USER_CODE"] != null)
            {
                ViewBag.Breadcrumb = "View Tickets";
                try
                {
                    int _Teacher_id = Convert.ToInt32(Convert.ToString(Session["USER_CODE"]).Substring(1));
                    int Teacher_subject = Convert.ToInt32(DbContext.tbl_DC_Course_Teacher_Assign.Where(x => x.Teacher_ID == _Teacher_id && x.Is_Active == true && x.Is_Deleted == false).Select(x => x.Subject_Id).FirstOrDefault());

                    var data = DbContext.SP_DC_Ticket_Assigned_to_teacher(_Teacher_id).OrderByDescending(x => x.Ticket_ID).ToList();
                    ViewBag.Ticket = data;
                }
                catch (Exception)
                {
                    return RedirectToAction("Logout", "Teacher");
                }
            }
            else
            {
                return RedirectToAction("Logout", "Teacher");
            }

            return View();
        }
        public ActionResult mytickets()
        {
            if (Session["USER_CODE"] != null)
            {
                ViewBag.Breadcrumb = "View Tickets";
                try
                {
                    int _Teacher_id = Convert.ToInt32(Convert.ToString(Session["USER_CODE"]).Substring(1));
                    var data = DbContext.SP_DC_Ticket_Assigned_to_teacher(_Teacher_id).Where(x => x.Teach_ID == _Teacher_id).OrderByDescending(x => x.Ticket_ID).ToList();
                    ViewBag.Ticket = data;
                }
                catch (Exception)
                {
                    return RedirectToAction("Logout", "Teacher");
                }
            }
            else
            {
                return RedirectToAction("Logout", "Teacher");

            }
            return View("Viewticekts");
        }

        [HttpGet]
        public ActionResult AnswerTicket(int? id)
        {
            if (Session["USER_CODE"] != null)
            {
                try
                {
                    ViewBag.Breadcrumb = "Answer Ticket";
                    if (id != null)
                    {

                        var ticket_qsn = DbContext.View_DC_All_Tickets_Details.Where(x => x.Ticket_ID == id).FirstOrDefault();
                        ViewBag.student_name = ticket_qsn.Customer_Name;
                        ViewBag.TClass_name = ticket_qsn.Class_Name;
                        ViewBag.tquestion = ticket_qsn.Question;
                        ViewBag.status = ticket_qsn.Status;
                        ViewBag.h_tkid = ticket_qsn.Ticket_ID;
                        ViewBag.ticketno = ticket_qsn.Ticket_No;
                        ViewBag.studentname = ticket_qsn.Student_ID;
                        var ticket_answer = DbContext.tbl_DC_Ticket_Dtl.Where(x => x.Ticket_ID == id && x.Is_Active == true && x.Is_Deleted == false).ToList();

                        ViewBag.all_ticketansr = DbContext.tbl_DC_Ticket_Dtl.Where(x => x.Ticket_ID == id && x.Is_Active == true && x.Is_Deleted == false).ToList();
                        ViewBag.comments = DbContext.tbl_DC_Ticket_Thread.Where(x => x.Ticket_ID == id && x.Is_Active == true && x.Is_Deleted == false).ToList();
                        ViewBag.isclosed = DbContext.tbl_DC_Ticket.Where(x => x.Ticket_ID == id && x.Is_Active == true && x.Is_Deleted == false).Select(x => x.Status).FirstOrDefault();
                        ViewBag.check_answer = DbContext.tbl_DC_Ticket_Dtl.Where(x => x.Ticket_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    }
                    else
                    {
                        return RedirectToAction("Viewticekts", "Teacher");
                    }
                }
                catch (Exception ex)
                {

                    TempData["ErrorMessage"] = "Something went wrong.";
                    return RedirectToAction("Viewticekts", "Teacher");
                }
            }
            else
            {
                return RedirectToAction("Logout", "Teacher");
            }

            return View();
        }
        [HttpPost]
        public ActionResult AnswerTicket(string Answer_Ticket, HttpPostedFileBase RegImage3, string h_tkid, string h_ticno, string h_sname)
        {
            int id = Convert.ToInt32(h_tkid);
          
            if (Session["USER_CODE"] != null)
            {
                int replied_byid = Convert.ToInt32(Convert.ToString(Session["USER_CODE"]).Substring(1));
                try
                {
                    if (h_tkid != "")
                    {
                        if (Answer_Ticket.Trim() != "")
                        {
                            var _find_assin_or_not = DbContext.tbl_DC_Ticket.Where(x => x.Ticket_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                            if (_find_assin_or_not.Teach_ID != null)
                            {
                                tbl_DC_Ticket_Dtl tk_dtl = new tbl_DC_Ticket_Dtl();
                                tk_dtl.Ticket_ID = id;
                                tk_dtl.Answer = Answer_Ticket;
                                tk_dtl.Replied_By = replied_byid;
                                tk_dtl.Replied_Date = today;
                                tk_dtl.Is_Active = true;
                                tk_dtl.Is_Deleted = false;
                                if (RegImage3 != null)
                                {

                                    string guid = Guid.NewGuid().ToString();
                                    var fileName = Path.GetFileName(RegImage3.FileName.Replace(RegImage3.FileName.Split('.').FirstOrDefault().ToString(), guid.ToString()));
                                    var path = Path.Combine(Server.MapPath("~/Images/Answerimage/"), fileName);
                                    RegImage3.SaveAs(path);
                                    tk_dtl.Answer_Image = fileName.ToString();

                                }
                                DbContext.tbl_DC_Ticket_Dtl.Add(tk_dtl);
                                DbContext.SaveChanges();
                                if (_find_assin_or_not.Status == "D")
                                {
                                    _find_assin_or_not.Status = "O";
                                    DbContext.SaveChanges();
                                }
                                int student_id_ = Convert.ToInt32(h_sname);
                                var student_mail = DbContext.tbl_DC_Registration.Where(x => x.Regd_ID == student_id_ && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                                //Teacher_sendMail(student_mail.Customer_Name, student_mail.Email, h_ticno);
                                answer_mail("Ticket_answer", student_mail.Email, student_mail.Customer_Name, h_ticno.ToString());
                                try
                                {
                                    var pushnot = (from c in DbContext.tbl_DC_Registration.Where(x => x.Regd_ID == student_mail.Regd_ID)

                                                   select new { c.Regd_ID, c.Device_id }).FirstOrDefault();
                                    string body = "tktid#{{tktid}}# DOUBT {{ticketno}} has been answered Hello {{name}}, Your doubt no- {{ticketno}} has been answered. View the answer and reply back if you have any further doubts ";
                                    string msg = body.ToString().Replace("{{name}}", student_mail.Customer_Name).Replace("{{ticketno}}", _find_assin_or_not.Ticket_No).Replace("{{tktid}}", tk_dtl.Ticket_ID.ToString());
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
                                TempData["SuccessMessage"] = "You have answerd successfully.";
                                return RedirectToAction("AnswerTicket", "Teacher", new { id = h_tkid });
                            }
                            else
                            {
                                tbl_DC_Ticket_Assign _ticket_Assign = new tbl_DC_Ticket_Assign();
                                _ticket_Assign.Ticket_ID = id;
                                _ticket_Assign.Ticket_No = _find_assin_or_not.Ticket_No;
                                _ticket_Assign.Student_ID = _find_assin_or_not.Student_ID;
                                _ticket_Assign.Teach_ID = replied_byid;
                                _ticket_Assign.Assign_Date = today;
                                _ticket_Assign.Is_Close = false;
                                _ticket_Assign.Inserted_By = replied_byid;
                                _ticket_Assign.Inserted_Date = today;
                                _ticket_Assign.Is_Active = true;
                                _ticket_Assign.Is_Deleted = false;
                                DbContext.tbl_DC_Ticket_Assign.Add(_ticket_Assign);
                                DbContext.SaveChanges();
                                tbl_DC_Ticket _ticket = DbContext.tbl_DC_Ticket.Where(x => x.Ticket_ID == id).FirstOrDefault();
                                
                                _ticket.Teach_ID = replied_byid;
                                _ticket.Assign_Date = today;
                                DbContext.SaveChanges();
                                #region ticketanswer
                                tbl_DC_Ticket_Dtl tk_dtl = new tbl_DC_Ticket_Dtl();
                                tk_dtl.Ticket_ID = id;
                                tk_dtl.Answer = Answer_Ticket;
                                tk_dtl.Replied_By = replied_byid;
                                tk_dtl.Replied_Date = today;
                                tk_dtl.Is_Active = true;
                                tk_dtl.Is_Deleted = false;
                                if (RegImage3 != null)
                                {

                                    string guid = Guid.NewGuid().ToString();
                                    var fileName = Path.GetFileName(RegImage3.FileName.Replace(RegImage3.FileName.Split('.').FirstOrDefault().ToString(), guid.ToString()));
                                    var path = Path.Combine(Server.MapPath("~/Images/Answerimage/"), fileName);
                                    RegImage3.SaveAs(path);
                                    tk_dtl.Answer_Image = fileName.ToString();

                                }
                                DbContext.tbl_DC_Ticket_Dtl.Add(tk_dtl);
                                DbContext.SaveChanges();
                                if (_find_assin_or_not.Status == "D")
                                {
                                    _find_assin_or_not.Status = "O";
                                    DbContext.SaveChanges();
                                }
                                int student_id_ = Convert.ToInt32(h_sname);
                                var student_mail = DbContext.tbl_DC_Registration.Where(x => x.Regd_ID == student_id_ && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                               // Teacher_sendMail(student_mail.Customer_Name, student_mail.Email, h_ticno);
                                if (student_mail.Email!=null)
                                {
                                    answer_mail("Ticket_answer", student_mail.Email, student_mail.Customer_Name, h_ticno.ToString());
                                }

                                try
                                {
                                    var pushnot = (from c in DbContext.tbl_DC_Registration.Where(x => x.Regd_ID == student_mail.Regd_ID)

                                                   select new { c.Regd_ID, c.Device_id }).FirstOrDefault();
                                    string body = "tktid#{{tktid}}# DOUBT {{ticketno}} has been answered Hello {{name}}, Your doubt no- {{ticketno}} has been answered. View the answer and reply back if you have any further doubts ";
                                    string msg = body.ToString().Replace("{{name}}", student_mail.Customer_Name).Replace("{{ticketno}}", _find_assin_or_not.Ticket_No).Replace("{{tktid}}", tk_dtl.Ticket_ID.ToString());
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

                                TempData["SuccessMessage"] = "You have answerd successfully.";
                                return RedirectToAction("AnswerTicket", "Teacher", new { id = h_tkid });
                                #endregion

                            }
                        }
                        else
                        {
                            TempData["WarningMessage"] = "Please provide a answer.";
                            return RedirectToAction("AnswerTicket", "Teacher", new { id = h_tkid });
                        }
                    }

                    else
                    {
                        TempData["WarningMessage"] = "Please try again.";
                        return RedirectToAction("Viewtickets", "Teacher");

                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Something went wrong.";
                    return RedirectToAction("AnswerTicket", "Teacher", new { id = h_tkid });

                }
            }
            else
            {
                return RedirectToAction("Logout", "Teacher");
            }


        }
        [HttpGet]
        public ActionResult RejectTicket(int? id)
        {
            try
            {
                ViewBag.Breadcrumb = "Reject Ticket";
                if (id != null)
                {
                    var tdata = DbContext.View_DC_All_Tickets_Details.Where(x => x.Ticket_ID == id).FirstOrDefault();


                    ViewBag.studentname = tdata.Customer_Name;


                    ViewBag.classname = tdata.Class_Name;

                    ViewBag.question = tdata.Question.ToString();
                    ViewBag.tkid = tdata.Ticket_ID;
                    ViewBag.status = tdata.Status;
                    ViewBag.remark = tdata.Remark;

                }
                else
                {
                    TempData["ErrorMessage"] = "Invalid ticket details.";
                    return RedirectToAction("Viewticekts", "Teacher");
                }
            }
            catch (Exception ex)
            {

                TempData["ErrorMessage"] = "Something went wrong.";
                return RedirectToAction("Viewticekts", "Teacher");

            }

            return View();
        }
        [HttpPost]
        public ActionResult RejectTicket(string Remark_Reject, string h_tkid)
        {
            if (Session["USER_CODE"] != null)
            {
                try
                {
                    if (Remark_Reject.Trim() != "")
                    {
                        int id = Convert.ToInt32(h_tkid);
                        tbl_DC_Ticket tkt_rj = DbContext.tbl_DC_Ticket.Where(x => x.Ticket_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        tkt_rj.Status = "R";
                        tkt_rj.Remark = Remark_Reject;
                        DbContext.SaveChanges();
                        var get_student = DbContext.tbl_DC_Registration.Where(x => x.Regd_ID == tkt_rj.Student_ID).FirstOrDefault();
                        if (get_student.Email != null)
                        {
                           // Close_ticket_mail(tkt_rj.Student_ID.ToString(), tkt_rj.Ticket_No);

                            sendMail_close_reject("Ticket_close", get_student.Email.ToString(), get_student.Customer_Name, tkt_rj.Ticket_No.ToString(), "R", Remark_Reject);
                        }

                        TempData["SuccessMessage"] = "Question rejected";
                    }
                    else
                    {
                        TempData["WarningMessage"] = "Please provide reason of rejection.";
                    }
                }
                catch (Exception ex)
                {
                    TempData["WarningMessage"] = "Something went wrong.";
                    return RedirectToAction("Logout", "Teacher");

                }

                return RedirectToAction("Viewticekts", "Teacher");
            }
            else
            {
                return RedirectToAction("Logout", "Teacher");
            }

        }

        public bool sendMail_close_reject(string parameter, string email, string name, string ticket_no, string typ, string remark)
        {
            var getall = DbContext.SP_DC_Get_maildetails(parameter).FirstOrDefault();
            string eidTo = email;
            string toshow = getall.SMTP_Sender.ToString();
            string from_mail = getall.SMTP_Email;
            string eidFrom = getall.SMTP_User.ToString();
            string password = getall.SMTP_Pwd.ToString();
            string ticket = ticket_no;
            string msgsub = string.Empty;
            if (typ == "R")
            {
                msgsub = getall.Email_Subject.ToString().Replace("{{ticketno}}", ticket_no).Replace("closed", "Rejected");
            }
            else if (typ == "C")
            {
                msgsub = getall.Email_Subject.ToString().Replace("{{ticketno}}", ticket_no);
            }
            string hostname = getall.SMTP_HostName;
            string portname = getall.SMTP_Port.ToString();
            bool ssl_tof = true;
            string msgbody = getall.EmailConf_Body;
            if (typ == "R")
            {
                msgbody = getall.EmailConf_Body.ToString().Replace("{{name}}", name).Replace("{{ticketno}}", ticket).Replace("{{tickettype}}", "rejected").Replace("{{date}}", DateTime.Now.ToString()).Replace("{{remark}}", remark);
            }
            else if (typ == "C")
            {
                if (remark != "")
                {
                    msgbody = getall.EmailConf_Body.ToString().Replace("{{name}}", name).Replace("{{ticketno}}", ticket).Replace("{{tickettype}}", "closed").Replace("{{date}}", DateTime.Now.ToString()).Replace("{{remark}}", remark);
                }
                else
                {
                    msgbody = getall.EmailConf_Body.ToString().Replace("{{name}}", name).Replace("{{ticketno}}", ticket).Replace("{{tickettype}}", "closed").Replace("{{date}}", DateTime.Now.ToString()).Replace("{{remark}}", "");
                }

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


        public ActionResult ViewTickectDetail(int? id)
        {
            ViewBag.Breadcrumb = "View ticket details";
            try
            {
                if (id != null)
                {

                    var ticket_data = DbContext.View_DC_All_Tickets_Details.Where(x => x.Ticket_ID == id).ToList();
                    int ticket_id = Convert.ToInt32(ticket_data.FirstOrDefault().Ticket_ID);
                    var get_ans = DbContext.tbl_DC_Ticket_Dtl.Where(x => x.Ticket_ID == ticket_id).Select(x => x.Ticket_Dtls_ID).FirstOrDefault();
                    if (get_ans != 0)
                    {
                        ViewBag.chkans = get_ans;
                    }
                    else
                    {
                        ViewBag.chkans = null;
                    }
                    ViewBag.viewticket = ticket_data.ToList();

                }
                else
                {
                    ViewBag.viewticket = null;
                    ViewBag.chkans = null;
                }
            }
            catch (Exception ex)
            { 
            ViewBag.viewticket = null;
            ViewBag.chkans = null;
            }
           
            
            return View();
        }
        [HttpPost]
        public JsonResult tooltip_ticket_details(int id)
        {

            string[] msg = new string[4];
            if (id != 0)
            {
                try
                {
                    var _ticket_data = DbContext.View_DC_Tickets_and_Teacher.Where(x => x.Ticket_ID == id).FirstOrDefault();
                    if (_ticket_data != null)
                    {
                        msg[0] = _ticket_data.Question;
                        msg[1] = Convert.ToString(_ticket_data.Inserted_Date).Substring(0, 10);
                    }
                    else
                    {
                        msg[0] = "No data found";
                    }

                }
                catch (Exception ex)
                {
                    msg[0] = "No data found";
                }
            }

            return Json(msg, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Ticket_report(string f_Date, string t_Date)
        {
            if (f_Date != "" && t_Date != "")
            {
                int _Teacher_id = Convert.ToInt32(Convert.ToString(Session["USER_CODE"]).Substring(1));
                if (Convert.ToDateTime(f_Date) <= today.Date)
                {
                    if (Convert.ToDateTime(t_Date) <= today.Date)
                    {


                        if (Convert.ToDateTime(f_Date) <= Convert.ToDateTime(t_Date))
                        {
                            string fdtt = f_Date + " 00:00:00 AM";
                            string tdtt = t_Date + " 23:59:59 PM";
                            DateTime fdt = Convert.ToDateTime(fdtt);
                            DateTime tdt = Convert.ToDateTime(tdtt);
                            var logstatus = (from c in DbContext.SP_DC_Ticket_Assigned_to_teacher(_Teacher_id) where c.Inserted_Date >= fdt && c.Inserted_Date <= tdt select c).ToList().OrderByDescending(x=>x.Inserted_Date).OrderByDescending(x=>x.Ticket_ID);
                            ViewBag.Ticket = logstatus.ToList();
                            ViewBag.Breadcrumb = "view Tickets";
                            ViewBag.teachernames_tickets = DbContext.View_DC_CourseAssign.ToList();
                        }
                        else
                        {

                            TempData["ErrorMessage"] = "From date should be less or equal to todate.";
                        }

                    }
                    else
                    {

                        TempData["ErrorMessage"] = "To date should be less or equal to from-date.";
                    }
                }
                else
                {

                    TempData["ErrorMessage"] = "From date should be less than today date.";
                }
            }
            else
            {

                TempData["ErrorMessage"] = "Please enter Date to Search.";
            }
            return View("Viewticekts");
        }


        public ActionResult TeacherDashboard()
        {
             if (Session["USER_CODE"] != null)
            {
                   int teacher_id=Convert.ToInt32(Convert.ToString(Session["USER_CODE"]).Substring(1));
                   var batch = DbContext.tbl_DC_Batch_Assign.Where(x => x.Teach_ID == teacher_id && x.Is_Active==true && x.Is_Deleted==false).Take(10).ToList();
                   var course = DbContext.tbl_DC_Course_Teacher_Assign.Where(x => x.Teacher_ID == teacher_id && x.Is_Active == true && x.Is_Deleted == false).Take(10).ToList();
                   if (course.Count > 0)
                   {
                       if(batch.Count > 0)
                     {
                         ViewBag.batch = batch.ToList();
                     }
                       else
                       {
                           ViewBag.batch = null;
                       }
                   
                       ViewBag.course = course.ToList();
                   }
                   else
                   {
                       ViewBag.course = null;
                   }
                 
                   var ticket = DbContext.tbl_DC_Ticket.Where(x => x.Teach_ID == teacher_id).ToList();
                   
                 string name=Session["USER_CODE"].ToString().Trim();
                 DateTime last_dt=Convert.ToDateTime(DbContext.tbl_DC_LoginStatus.Where(x=>x.Login_ID==name).Select(x=>x.Logout_DateTime).FirstOrDefault());

                 List<tbl_DC_Ticket_Thread> li = new List<tbl_DC_Ticket_Thread>();
                   if (ticket.Where(x=>x.Status=="O").Select(x=>x.Ticket_ID).ToList().Count()>0)
                   {
                       foreach (var v in ticket.Where(x => x.Status == "O").Select(x => x.Ticket_ID).ToList())
                    {
                        teacher_id=Convert.ToInt32(v);
                        var data=DbContext.tbl_DC_Ticket_Thread.Where(x=>x.Ticket_ID==teacher_id && x.Is_Teacher==false && x.User_Comment_Date>last_dt ).ToList();
                        if(data.Count>0)
                        {
                         li.Add(data.FirstOrDefault());
                        }
                       
                  
                    }
                   }
              if(li.Count>0)
              {
                ViewBag.rply = li.Take(10);
              }
              else
              {
                  ViewBag.rply = null;
              }
              
              loginstatus(Convert.ToString(Session["USER_CODE"]).Trim());
              if (ticket.Where(x => x.Status == "D").ToList().Count()>0 && ticket!=null)
              {
                ViewBag.allticket = ticket.Where(x=>x.Status=="D").ToList().Take(10);
              }
              else
              {
                  ViewBag.allticket = null;
              }
              
            }
         
            ViewBag.Breadcrumb = "Dashboard";
            return View();
        }

        public void overdue()
        {
            try
            {
                var _all_tickets = DbContext.tbl_DC_Ticket.Where(x => x.Is_Active == true && x.Is_Deleted == false && x.Assign_Date != null).ToList();
                DateTime dt = today;
                for (int i = 0; i < _all_tickets.Count; i++)
                {
                    int ticekt_id_over = Convert.ToInt32(_all_tickets[i].Ticket_ID);
                    DateTime _tdt = Convert.ToDateTime(_all_tickets[i].Assign_Date);
                    if (dt.Date > _tdt.Date)
                    {
                        tbl_DC_Ticket _ticket_overdue = DbContext.tbl_DC_Ticket.Where(x => x.Ticket_ID == ticekt_id_over && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        if (_ticket_overdue.Status != "R" && _ticket_overdue.Status != "D" && _ticket_overdue.Status != "C")
                        {
                            var found_answer = DbContext.tbl_DC_Ticket_Dtl.Where(x => x.Ticket_ID == ticekt_id_over && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                            if (found_answer == null)
                            {
                                _ticket_overdue.Status = "D";
                                DbContext.SaveChanges();
                            }

                        }
                    }

                }
            }
            catch (Exception ex)
            {

                ex.Message.ToString();
            }
        }
        [HttpPost]
        public ActionResult AnswerReply(int? Ticket_id, int? Ticket_answerid, string msgbody, string close, string remark, HttpPostedFileBase RegImage3)
        {
            string message = string.Empty;
            if (Session["USER_CODE"] != null)
            {
                if (msgbody != "")
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
                        _ticket_thred.Is_Teacher = true;
                        _ticket_thred.Is_Active = true;
                        _ticket_thred.Is_Deleted = false;
                        if (RegImage3 != null)
                        {

                            string guid = Guid.NewGuid().ToString();
                            var fileName = Path.GetFileName(RegImage3.FileName.Replace(RegImage3.FileName.Split('.').FirstOrDefault().ToString(), guid.ToString()));
                            var path = Path.Combine(Server.MapPath("~/Images/Qusetionimages/"), fileName);
                            RegImage3.SaveAs(path);
                            _ticket_thred.R_image = fileName.ToString();

                        }
                        DbContext.tbl_DC_Ticket_Thread.Add(_ticket_thred);
                        DbContext.SaveChanges();
                        if (close == "on")
                        {
                            tbl_DC_Ticket_Assign _tbl_close = DbContext.tbl_DC_Ticket_Assign.Where(x => x.Ticket_ID == Ticket_id).FirstOrDefault();
                            _tbl_close.Is_Close = true;
                            _tbl_close.Remark = remark;
                            _tbl_close.Close_Date = today;
                            _tbl_close.Modified_By = _Teacher_id;
                            _tbl_close.Modified_Date = today;
                            DbContext.SaveChanges();
                            tbl_DC_Ticket _tbl_status = DbContext.tbl_DC_Ticket.Where(x => x.Ticket_ID == Ticket_id).FirstOrDefault();
                            _tbl_status.Status = "C";
                            _tbl_status.Modified_By = _Teacher_id;
                            _tbl_status.Modified_Date = today;
                            DbContext.SaveChanges();
                            var get_student = DbContext.tbl_DC_Registration.Where(x => x.Regd_ID == _tbl_close.Student_ID).FirstOrDefault();
                            if (get_student!= null)
                            {
                                //Close_ticket_mail(_tbl_close.Student_ID.ToString(), _tbl_close.Ticket_No);
                                sendMail_close_reject("Ticket_close", get_student.Email.ToString(), get_student.Customer_Name, _tbl_close.Ticket_No.ToString(), "C", remark.ToString());
                            }
                        }
                        TempData["SuccessMessage"] = "You reply submited successfully.";
                        return RedirectToAction("AnswerTicket", "Teacher", new { id = Ticket_id });
                    }
                    catch (Exception ex)
                    {
                        TempData["ErrorMessage"] = "Something went wrong.";
                        return RedirectToAction("AnswerTicket", "Teacher", new { id = Ticket_id });

                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "Reply should not be empty.";
                    return RedirectToAction("AnswerTicket", "Teacher", new { id = Ticket_id });
                }
            }
            else
            {
                return RedirectToAction("Logout", "Teacher");
            }


        }


        #endregion

        #region -------------------------------shift-----------------------------------
        public ActionResult shift()
        {
            ViewBag.Breadcrumb = "View Shift";

            try
            {
                int id = Convert.ToInt32(Session["USER_CODE"].ToString().Substring(1));
                ViewBag.shift = DbContext.tbl_DC_Shift_Teacher_Assign.Where(x => x.Teacher_ID == id).ToList();
                return View();
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
                return View();
            }
        }
        #endregion

        #region -------------------------------getuserid-------------------------------
        public int getuserid(object o)
        {
            string data = o.ToString();
            int id = 0;
            var prefix = DbContext.tbl_DC_Prefix.Where(X => X.PrefixType_ID == 1).ToList();
            for (int i = 0; i < prefix.Count; i++)
            {
                string find = Convert.ToString(prefix[i].Prefix_Name);
                if (data.Contains(find))
                {
                    int j=Convert.ToInt16(data.Replace(find,"").Trim());
                    //int count = find.Length;
                    //id = Convert.ToInt32(data.Substring(count).Trim());

                }
            }
            return id;
        }
        #endregion
        
        #region -----------------------------Teacher profile---------------------------
        [HttpGet]
        public ActionResult TecaherProfile()
        {
            if (Session["USER_CODE"] != null)
            {
                int teacher_id = Convert.ToInt32(Convert.ToString(Session["USER_CODE"]).Substring(1));
                ViewBag.Breadcrumb = "Profile";

                tbl_DC_Teacher _teachr = DbContext.tbl_DC_Teacher.Where(x => x.Teach_ID == teacher_id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                ViewBag.Teach_ID = _teachr.Teach_ID;
                ViewBag.Teacher_name = _teachr.Teacher_Name;
                ViewBag.degination = _teachr.Designation;
                string date = Convert.ToString(_teachr.DateOfBirth).Substring(0, 10);
                ViewBag.dateofbirth = date;
                ViewBag.gender = _teachr.Gender;
                ViewBag.email = _teachr.Email_ID;
                ViewBag.mobile = _teachr.Mobile;
                ViewBag.address = _teachr.Address;
                ViewBag.image = _teachr.Image;

            }
            else
            {
                return RedirectToAction("Logout", "Teacher");
            }
            return View();
        }
        [HttpPost]
        public ActionResult TecaherProfile(int id, DigiChampsModel.DigiChampsTeacherRegModel obj, HttpPostedFileBase teacher_img)
        {
            if (Session["USER_CODE"] != null)
            {
                if (ModelState.IsValid)
                {
                    if (Convert.ToDateTime(obj.DateOfBirth).Date < today.Date)
                    {

                        tbl_DC_Teacher obj1 = DbContext.tbl_DC_Teacher.Where(x => x.Teach_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        if (obj1 != null)
                        {
                            string image = string.Empty;

                            obj1.Teacher_Name = obj.Teacher_Name;
                            obj1.Address = obj.Address;
                            obj1.DateOfBirth = obj.DateOfBirth;
                            obj1.Designation = obj.Designation;
                            obj1.Email_ID = obj.Email_ID;
                            obj1.Gender = obj.Gender;
                            obj1.Mobile = obj.Mobile;
                            string guid = Guid.NewGuid().ToString();
                            if (teacher_img != null)
                            {
                                var fileName = Path.GetFileName(teacher_img.FileName.Replace(teacher_img.FileName.Split('.').FirstOrDefault().ToString(), guid.ToString()));
                                image = Convert.ToString(fileName);
                                var path = Path.Combine(Server.MapPath("~/Images/Teacherprofile/"), fileName);
                                teacher_img.SaveAs(path);
                                obj1.Image = image;
                            }

                            obj1.Is_Active = true;
                            obj1.Is_Deleted = false;
                            obj1.Modified_By = HttpContext.User.Identity.Name;
                            obj1.Modified_Date = today;
                            DbContext.SaveChanges();
                            TempData["SuccessMessage"] = "Your details updated successfully.";
                            return RedirectToAction("TecaherProfile", "Teacher");
                        }
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Please provide date of birth correctly";
                        return RedirectToAction("TecaherProfile", "Teacher");
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "Please provide data correctly";
                    return RedirectToAction("TecaherProfile", "Teacher");
                }


            }
            else
            {
                return RedirectToAction("Logout", "Teacher");
            }
            return View();
        }
        #endregion

        #region ------------------------------------------------------login/logoutstatus--------------------------------------------------------
        public void loginstatus(string usercode)
        {
            try
            {
                string hostNames = Dns.GetHostName();
                IPAddress[] arrs = Dns.GetHostEntry(hostNames).AddressList;
                string ip = arrs[1].ToString();

                tbl_DC_LoginStatus id_found = DbContext.tbl_DC_LoginStatus.Where(x => x.Login_ID == usercode).FirstOrDefault();
                if (id_found != null)
                {
                    id_found.Login_DateTime = DateTime.Now;
                    id_found.Status = true;
                    id_found.Login_IPAddress = ip;
                    id_found.Logout_DateTime = null;
                    DbContext.SaveChanges();
                }
                else
                {
                    tbl_DC_LoginStatus t_logi = new tbl_DC_LoginStatus();
                    t_logi.Login_ID = usercode;
                    t_logi.Login_IPAddress = ip;
                    t_logi.Login_DateTime = DateTime.Now;
                    var log_name = DbContext.tbl_DC_USER_SECURITY.Where(x => x.USER_CODE == usercode).Select(x => x.USER_NAME).FirstOrDefault();
                    t_logi.Login_By = log_name;
                    t_logi.Status = true;
                    DbContext.tbl_DC_LoginStatus.Add(t_logi);
                    DbContext.SaveChanges();
                }

            }
            catch (Exception ex)
            {
                ex.Message.ToString();
            }

        }
        public void logoutstatus(string usercode)
        {
            try
            {
                tbl_DC_LoginStatus t_logo = DbContext.tbl_DC_LoginStatus.Where(x => x.Login_ID == usercode).FirstOrDefault();
                if (t_logo != null)
                {
                    t_logo.Logout_DateTime = DateTime.Now;
                    t_logo.Status = false;
                    DbContext.SaveChanges();
                }

            }
            catch (Exception ex)
            {
                ex.Message.ToString();
            }

        }
        #endregion

        #region -----------------------------------------------------Question ------------------------------------------------------------------
        [HttpGet]
        public ActionResult QuestionBank()
        {
            try
            {
                ViewBag.Breadcrumb = "Question";
                ViewBag.Question = DbContext.tbl_DC_Question.Where(x => x.Is_Active == true && x.Is_Deleted == false).OrderByDescending(x => x.Question_ID).ToList();
                var Count_Que = DbContext.SP_DC_Question_Count(0).ToList();
                ViewBag.Count_Que = Count_Que.ToList();
                return View();
            }
            catch (Exception)
            {
                throw;
            }
        }
        [HttpGet]
        public ActionResult Add_New_Question()
        {
            ViewBag.Breadcrumb = "Question";
            ViewBag.Board_details = DbContext.tbl_DC_Board.Where(x => x.Is_Active == true && x.Is_Deleted == false).ToList();
            ViewBag.Power_Id = DbContext.tbl_DC_Power_Question.Where(x => x.Is_Active == true && x.Is_Deleted == false).ToList();
            return View();
        }
        public ActionResult Edit_Question(int? id)
        {
            try
            {
                ViewBag.Breadcrumb = "Question";
                ViewBag.Board_details = DbContext.tbl_DC_Board.Where(x => x.Is_Active == true && x.Is_Deleted == false).ToList();
                ViewBag.Power_Id = DbContext.tbl_DC_Power_Question.Where(x => x.Is_Active == true && x.Is_Deleted == false).ToList();
                if (id != null)
                {
                    var obj = DbContext.tbl_DC_Question.Where(x => x.Question_ID == id && x.Is_Active == true && x.Is_Deleted == false);
                    if (obj != null)
                    {
                        ViewBag.queDetails = obj.ToList();
                        var d = obj.FirstOrDefault();
                        ViewBag.classid = d.Class_Id;
                        ViewBag.Boardid = d.Board_Id;
                        ViewBag.subid = d.Subject_Id;
                        ViewBag.chapterid = d.Chapter_Id;
                        ViewBag.topicid = d.Topic_ID;
                        ViewBag.practice = d.Is_Practice;
                        ViewBag.pre_req = d.Is_PreRequisite;
                        ViewBag.test = d.Is_online;
                        ViewBag.global = d.Is_Global;
                        ViewBag.qstn_img = DbContext.tbl_DC_Question_Images.Where(x => x.Question_ID == id && x.Is_Active == true && x.Is_Deleted == false).OrderByDescending(x => x.Ques_img_id).ToList();
                        ViewBag.qansDetails = DbContext.tbl_DC_Question_Answer.Where(x => x.Question_ID == id && x.Is_Active == true && x.Is_Deleted == false).ToList();

                        ViewBag.answer = DbContext.tbl_DC_Question_Answer.Where(x => x.Question_ID == id && x.Is_Answer == true && x.Is_Active == true && x.Is_Deleted == false).Select(x => x.Option_Desc).FirstOrDefault();
                    }
                    else
                    {
                        TempData["WarningMessage"] = "Particular question doesnot exist.";
                        return RedirectToAction("QuestionBank");
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return View();
        }

        [HttpPost]
        public ActionResult View_Question(int? Qid, int?[] ans_id, int?[] qimg_id, string ddlboard, string ddlclass, string ddlsubject, string ddlchapter, string ddltopic, string ddlpower, string Quest_, string[] Qphotos, string Quest_desc, string Practice, string Pre_requisite, string Test, string Global, string online, string[] answernew, HttpPostedFileBase[] optn_img, string[] des_text, HttpPostedFileBase[] ans_img, string[] chk_ans)
        {
            ViewBag.Board_details = DbContext.tbl_DC_Board.Where(x => x.Is_Active == true && x.Is_Deleted == false).ToList();
            ViewBag.Power_Id = DbContext.tbl_DC_Power_Question.Where(x => x.Is_Active == true && x.Is_Deleted == false).ToList();
            try
            {
                if (Quest_ != "" || Qphotos.Length > 0)
                {
                    if (ddlboard == "" || ddlchapter == "" || ddlclass == "" || ddlsubject == "" || ddltopic == "" || ddlpower == "" || answernew.Length == 0)
                    {
                        TempData["WarningMessage"] = "Please enter each field properly.";
                    }
                    else
                    {
                        if (Qid == null)
                        {
                            #region Insert
                            if (Quest_ != "")
                            {
                                int? chapter = Convert.ToInt32(ddlchapter);
                                var qst = DbContext.tbl_DC_Question.Where(x => x.Question == Quest_ && x.Chapter_Id == chapter && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                                if (qst != null)
                                {
                                    TempData["WarningMessage"] = "Particular question already exists.";
                                }
                                else
                                {
                                    tbl_DC_Question Qsn = new tbl_DC_Question();
                                    if (online == "1")
                                    {
                                        Qsn.Is_online = true;
                                    }
                                    else
                                    {
                                        Qsn.Is_online = false;
                                    }
                                    if (Practice == "1")
                                    {
                                        Qsn.Is_Practice = true;
                                    }
                                    else
                                    {
                                        Qsn.Is_Practice = false;
                                    }
                                    if (Pre_requisite == "1")
                                    {
                                        Qsn.Is_PreRequisite = true;
                                    }
                                    else
                                    {
                                        Qsn.Is_PreRequisite = false;
                                    }
                                    if (Global == "1")
                                    {
                                        Qsn.Is_Global = true;
                                    }
                                    else
                                    {
                                        Qsn.Is_Global = false;
                                    }
                                    Qsn.Board_Id = Convert.ToInt32(ddlboard);
                                    Qsn.Class_Id = Convert.ToInt32(ddlclass);
                                    Qsn.Subject_Id = Convert.ToInt32(ddlsubject);
                                    Qsn.Chapter_Id = Convert.ToInt32(ddlchapter);
                                    Qsn.Topic_ID = Convert.ToInt32(ddltopic);
                                    Qsn.Power_ID = Convert.ToInt32(ddlpower);
                                    Qsn.Question = Quest_;
                                    Qsn.Qustion_Desc = Quest_desc;
                                    Qsn.Inserted_By = HttpContext.User.Identity.Name;
                                    Qsn.Is_Active = true;
                                    Qsn.Is_Deleted = false;
                                    Qsn.Modified_By = HttpContext.User.Identity.Name;
                                    Qsn.Modified_Date = today;
                                    DbContext.tbl_DC_Question.Add(Qsn);
                                    DbContext.SaveChanges();


                                    if (Qphotos != null)
                                    {
                                        for (int i = 0; i < Qphotos.Length; i++)
                                        {
                                            tbl_DC_Question_Images Qimg = new tbl_DC_Question_Images();
                                            Qimg.Question_ID = Qsn.Question_ID;
                                            if (Qphotos[i] != null)
                                            {
                                                string[] image1 = Qphotos[i].ToString().Split(',');
                                                string name = Convert.ToString(Qsn.Question_ID + "" + today.ToShortTimeString() + "" + i);
                                                name = name.Replace(':', '_');
                                                Base64ToImage_Qstn(name, image1[1]);
                                                Qphotos[i] = name + ".jpg";
                                                Qimg.Question_desc_Image = Qphotos[i].ToString();
                                            }
                                            Qimg.Inserted_By = HttpContext.User.Identity.Name;
                                            Qimg.Is_Active = true;
                                            Qimg.Is_Deleted = false;
                                            DbContext.tbl_DC_Question_Images.Add(Qimg);
                                        }
                                        DbContext.SaveChanges();
                                    }

                                    tbl_DC_Question_Answer qans = new tbl_DC_Question_Answer();
                                    if (answernew != null)
                                    {
                                        for (int i = 0; i < answernew.Length; i++)
                                        {
                                            qans.Question_ID = Qsn.Question_ID;
                                            qans.Option_Desc = answernew[i];
                                            for (int j = 0; j < chk_ans.Length; j++)
                                            {
                                                if (chk_ans[j].ToString() == Convert.ToString(i + 1))
                                                {
                                                    qans.Is_Answer = true;
                                                    break;
                                                }
                                                else
                                                {
                                                    qans.Is_Answer = false;
                                                }
                                            }
                                            if (optn_img[i] != null)
                                            {
                                                string guid = Guid.NewGuid().ToString();
                                                var fileName = Path.GetFileName(optn_img[i].FileName.Replace(optn_img[i].FileName.Split('.').FirstOrDefault().ToString(), guid.ToString()));
                                                var path = Path.Combine(Server.MapPath("~/Content/Qusetion/"), fileName);
                                                optn_img[i].SaveAs(path);
                                                qans.Option_Image = fileName.ToString();
                                            }
                                            qans.Answer_desc = des_text[i].ToString();
                                            if (ans_img[i] != null)
                                            {
                                                string guid = Guid.NewGuid().ToString();
                                                var fileName = Path.GetFileName(ans_img[i].FileName.Replace(ans_img[i].FileName.Split('.').FirstOrDefault().ToString(), guid.ToString()));
                                                var path = Path.Combine(Server.MapPath("~/Content/Qusetion/"), fileName);
                                                ans_img[i].SaveAs(path);
                                                qans.Answer_Image = fileName.ToString();
                                            }
                                            qans.Inserted_By = HttpContext.User.Identity.Name;
                                            qans.Is_Active = true;
                                            qans.Is_Deleted = false;
                                            qans.Modified_By = HttpContext.User.Identity.Name;
                                            qans.Modified_Date = today;
                                            DbContext.tbl_DC_Question_Answer.Add(qans);
                                            DbContext.SaveChanges();
                                        }
                                    }
                                    TempData["SuccessMessage"] = "Question details inserted successfully.";
                                    return RedirectToAction("QuestionBank");

                                }
                            }
                            else
                            {
                                TempData["WarningMessage"] = "Question can not be left blank.";
                            }
                            #endregion
                        }
                        else
                        {
                            #region Update
                            if (Quest_ != "")
                            {
                                var qst = DbContext.tbl_DC_Question.Where(x => x.Question_ID == Qid && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                                if (qst == null)
                                {
                                    TempData["WarningMessage"] = "Invalid question details.";
                                }
                                else
                                {
                                    if (Convert.ToInt32(ddltopic) != qst.Topic_ID)
                                    {
                                        //var 
                                    }
                                    //qst.Board_Id = Convert.ToInt32(ddlboard);
                                    //qst.Class_Id = Convert.ToInt32(ddlclass);
                                    //qst.Subject_Id = Convert.ToInt32(ddlsubject);
                                    //qst.Chapter_Id = Convert.ToInt32(ddlchapter);
                                    qst.Topic_ID = Convert.ToInt32(ddltopic);
                                    qst.Power_ID = Convert.ToInt32(ddlpower);
                                    qst.Question = Quest_;
                                    qst.Qustion_Desc = Quest_desc;
                                    if (online == "1")
                                    {
                                        qst.Is_online = true;
                                    }
                                    else
                                    {
                                        qst.Is_online = false;
                                    }
                                    if (Practice == "1")
                                    {
                                        qst.Is_Practice = true;
                                    }
                                    else
                                    {
                                        qst.Is_Practice = false;
                                    }
                                    if (Pre_requisite == "1")
                                    {
                                        qst.Is_PreRequisite = true;
                                    }
                                    else
                                    {
                                        qst.Is_PreRequisite = false;
                                    }
                                    if (Global == "1")
                                    {
                                        qst.Is_Global = true;
                                    }
                                    else
                                    {
                                        qst.Is_Global = false;
                                    }
                                    qst.Inserted_By = HttpContext.User.Identity.Name;
                                    qst.Is_Active = true;
                                    qst.Is_Deleted = false;

                                    qst.Modified_By = HttpContext.User.Identity.Name;
                                    qst.Modified_Date = today;
                                    DbContext.Entry(qst).State = EntityState.Modified;
                                    DbContext.SaveChanges();

                                    if (Qphotos != null)
                                    {
                                        for (int i = 0; i < Qphotos.Length; i++)
                                        {
                                            tbl_DC_Question_Images qans = new tbl_DC_Question_Images();
                                            qans.Question_ID = Qid;
                                            if (Qphotos[i] != null)
                                            {
                                                string[] image1 = Qphotos[i].ToString().Split(',');
                                                string name = Convert.ToString(Qid + "" + today.ToShortTimeString() + "" + i);
                                                name = name.Replace(':', '_');
                                                Base64ToImage_Qstn(name, image1[1]);
                                                Qphotos[i] = name + ".jpg";
                                                qans.Question_desc_Image = Qphotos[i].ToString();
                                            }
                                            qans.Inserted_By = HttpContext.User.Identity.Name;
                                            qans.Modified_Date = today;
                                            qans.Is_Active = true;
                                            qans.Is_Deleted = false;
                                            DbContext.tbl_DC_Question_Images.Add(qans);
                                        }
                                        DbContext.SaveChanges();
                                    }

                                    if (answernew != null)
                                    {
                                        for (int i = 0; i < answernew.Length; i++)
                                        {
                                            if (ans_id[i] == null)
                                            {
                                                tbl_DC_Question_Answer qans = new tbl_DC_Question_Answer();
                                                qans.Question_ID = Qid;
                                                qans.Option_Desc = answernew[i];
                                                for (int j = 0; j < chk_ans.Length; j++)
                                                {
                                                    if (chk_ans[j].ToString() == Convert.ToString(i + 1))
                                                    {
                                                        qans.Is_Answer = true;
                                                        break;
                                                    }
                                                    else
                                                    {
                                                        qans.Is_Answer = false;
                                                    }
                                                }
                                                if (optn_img[i] != null)
                                                {
                                                    string guid = Guid.NewGuid().ToString();
                                                    var fileName = Path.GetFileName(optn_img[i].FileName.Replace(optn_img[i].FileName.Split('.').FirstOrDefault().ToString(), guid.ToString()));
                                                    var path = Path.Combine(Server.MapPath("~/Content/Qusetion/"), fileName);
                                                    optn_img[i].SaveAs(path);
                                                    qans.Option_Image = fileName.ToString();
                                                }
                                                qans.Answer_desc = des_text[i].ToString();
                                                if (ans_img[i] != null)
                                                {
                                                    string guid = Guid.NewGuid().ToString();
                                                    var fileName = Path.GetFileName(ans_img[i].FileName.Replace(ans_img[i].FileName.Split('.').FirstOrDefault().ToString(), guid.ToString()));
                                                    var path = Path.Combine(Server.MapPath("~/Content/Qusetion/"), fileName);
                                                    ans_img[i].SaveAs(path);
                                                    qans.Answer_Image = fileName.ToString();
                                                }
                                                qans.Inserted_By = HttpContext.User.Identity.Name;
                                                qans.Is_Active = true;
                                                qans.Is_Deleted = false;
                                                qans.Modified_By = HttpContext.User.Identity.Name;
                                                qans.Modified_Date = today;
                                                DbContext.tbl_DC_Question_Answer.Add(qans);
                                            }
                                            else
                                            {
                                                int? answer_id = ans_id[i];
                                                var qst_ans = DbContext.tbl_DC_Question_Answer.Where(x => x.Question_ID == Qid && x.Answer_ID == answer_id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                                                qst_ans.Option_Desc = answernew[i];
                                                for (int j = 0; j < chk_ans.Length; j++)
                                                {
                                                    if (chk_ans[j].ToString() == Convert.ToString(i + 1))
                                                    {
                                                        qst_ans.Is_Answer = true;
                                                        break;
                                                    }
                                                    else
                                                    {
                                                        qst_ans.Is_Answer = false;
                                                    }
                                                }
                                                if (optn_img[i] != null)
                                                {
                                                    string guid = Guid.NewGuid().ToString();
                                                    var fileName = Path.GetFileName(optn_img[i].FileName.Replace(optn_img[i].FileName.Split('.').FirstOrDefault().ToString(), guid.ToString()));
                                                    var path = Path.Combine(Server.MapPath("~/Content/Qusetion/"), fileName);
                                                    optn_img[i].SaveAs(path);
                                                    qst_ans.Option_Image = fileName.ToString();
                                                }
                                                qst_ans.Answer_desc = des_text[i].ToString();
                                                if (ans_img[i] != null)
                                                {
                                                    string guid = Guid.NewGuid().ToString();
                                                    var fileName = Path.GetFileName(ans_img[i].FileName.Replace(ans_img[i].FileName.Split('.').FirstOrDefault().ToString(), guid.ToString()));
                                                    var path = Path.Combine(Server.MapPath("~/Content/Qusetion/"), fileName);
                                                    ans_img[i].SaveAs(path);
                                                    qst_ans.Answer_Image = fileName.ToString();
                                                }
                                                qst_ans.Modified_Date = today;
                                                qst_ans.Modified_By = HttpContext.User.Identity.Name;
                                                DbContext.Entry(qst_ans).State = EntityState.Modified;
                                            }
                                        }
                                        DbContext.SaveChanges();
                                    }
                                    TempData["SuccessMessage"] = "Question details updated successfully.";
                                    return RedirectToAction("QuestionBank");
                                }
                            }
                            else
                            {
                                TempData["WarningMessage"] = "Question can not be left blank.";
                            }

                            #endregion
                        }
                    }
                }
                else
                {
                    TempData["WarningMessage"] = "Question can not be left blank.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return RedirectToAction("QuestionBank");
        }
        public Image Base64ToImage_Qstn(string name, string base64String)
        {
            // Convert base 64 string to byte[]
            byte[] imageBytes = Convert.FromBase64String(base64String);
            // Convert byte[] to Image
            using (var ms = new MemoryStream(imageBytes, 0, imageBytes.Length))
            {
                string filename = name + ".jpg";
                Image image = Image.FromStream(ms, true);
                var img = new Bitmap(Image.FromStream(ms));
                string tempFolderName = Server.MapPath("~/Content/Qusetion/" + filename);
                image.Save(tempFolderName, ImageFormat.Jpeg);
                return image;
            }
        }
        public ActionResult Delete_Question(int? id)
        {
            try
            {
                if (id != null)
                {
                    tbl_DC_Question dq = DbContext.tbl_DC_Question.Where(x => x.Question_ID == id).FirstOrDefault();
                    dq.Is_Active = false;
                    dq.Is_Deleted = true;
                    dq.Modified_By = HttpContext.User.Identity.Name;
                    dq.Modified_Date = today;
                    DbContext.Entry(dq).State = EntityState.Modified;
                    DbContext.SaveChanges();

                    var qans = DbContext.tbl_DC_Question_Answer.Where(x => x.Question_ID == id);
                    if (qans != null)
                    {
                        foreach (tbl_DC_Question_Answer qa in qans)
                        {
                            qa.Is_Active = false;
                            qa.Is_Deleted = true;
                            qa.Modified_By = HttpContext.User.Identity.Name;
                            qa.Modified_Date = today;
                            DbContext.Entry(qa).State = EntityState.Modified;
                            DbContext.SaveChanges();
                        }
                    }
                    var qanimg = DbContext.tbl_DC_Question_Images.Where(x => x.Question_ID == id);
                    if (qans != null)
                    {
                        foreach (tbl_DC_Question_Images qa in qanimg)
                        {
                            qa.Is_Active = false;
                            qa.Is_Deleted = true;
                            qa.Modified_By = HttpContext.User.Identity.Name;
                            qa.Modified_Date = today;
                            DbContext.Entry(qa).State = EntityState.Modified;
                            DbContext.SaveChanges();
                        }
                    }
                    TempData["SuccessMessage"] = "Question deleted successfully.";
                }
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return RedirectToAction("QuestionBank", "Teacher");
        }

        [HttpPost]
        public JsonResult deleteQstnOption(int id)
        {
            string msg = string.Empty;
            try
            {
                tbl_DC_Question_Answer dq = DbContext.tbl_DC_Question_Answer.Where(x => x.Answer_ID == id).FirstOrDefault();
                if (dq != null)
                {
                    int? qid = dq.Question_ID;
                    var qstotn = DbContext.tbl_DC_Question_Answer.Where(x => x.Question_ID == qid).ToList();
                    if (qstotn.Count > 1)
                    {
                        DbContext.tbl_DC_Question_Answer.Remove(dq);
                        DbContext.SaveChanges();
                        msg = "1";        //"Option deleted successfully.";
                    }
                    else
                    {
                        msg = "0";
                    }
                }
                else
                {
                    msg = "0";
                }
            }
            catch { msg = "0"; }
            return Json(msg, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult delete_Qsn_img(int? Qimg_id)
        {
            string msg = string.Empty;
            try
            {
                if (Qimg_id != null)
                {
                    tbl_DC_Question_Images dq = DbContext.tbl_DC_Question_Images.Where(x => x.Ques_img_id == Qimg_id).FirstOrDefault();
                    if (dq != null)
                    {
                        dq.Is_Active = false;
                        dq.Is_Deleted = true;
                        dq.Modified_By = HttpContext.User.Identity.Name;
                        dq.Modified_Date = today;
                        DbContext.Entry(dq).State = EntityState.Modified;
                        DbContext.SaveChanges();
                        msg = "1";        //"Question deleted successfully.";
                    }
                    else
                    {
                        msg = "0";
                    }
                }
                else
                {
                    msg = "0";
                }
            }
            catch (Exception)
            {
                msg = "-1";       // "Something went wrong.";
            }
            return Json(msg, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]  //call on delete click
        public ActionResult getquestion_avl(int qsn)
        {
            string b = string.Empty;
            var question = DbContext.tbl_DC_Exam_Result_Dtl.Where(x => x.Question_ID == qsn).FirstOrDefault();
            if (question != null)
            {
                b = "true";

            }
            else
            {
                b = "false";
            }
            return Json(b, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region --------------------------------------------------- Get All from ID ------------------------------------------------------------
        [HttpPost]
        public ActionResult GetAllChapter_subwise(int subid)
        {
            List<SelectListItem> modulename = new List<SelectListItem>();

            List<DigiChampsModel.DigiChampsModuleModel> states = (from a in DbContext.tbl_DC_Board.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                                                  join b in DbContext.tbl_DC_Class.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                                                      on a.Board_Id equals b.Board_Id
                                                                  join c in DbContext.tbl_DC_Subject.Where(x => x.Subject_Id == subid && x.Is_Active == true && x.Is_Deleted == false)
                                                                      on b.Class_Id equals c.Class_Id
                                                                  join d in DbContext.tbl_DC_Chapter.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                                                      on c.Subject_Id equals d.Subject_Id

                                                                  select new DigiChampsModel.DigiChampsModuleModel
                                                                  {
                                                                      Subject_Id = c.Subject_Id,
                                                                      Subject = c.Subject,
                                                                      Chapter_Id = d.Chapter_Id,
                                                                      Chapter = d.Chapter,

                                                                  }).ToList();

            DigiChampsModel.DigiChampsModuleModel obj = new DigiChampsModel.DigiChampsModuleModel();
            obj.subdtls = states;
            states.ForEach(x =>
            {
                modulename.Add(new SelectListItem { Text = x.Chapter.ToString(), Value = x.Chapter_Id.ToString() });
            });
            //ViewBag.state = new SelectList(dbContext.tbl_JV_State.Where(x => x.FK_Country_ID == conId && x.Is_Active == true && x.Is_Deleted == false), "PK_State_ID", "State_Name");

            return Json(modulename, JsonRequestBehavior.AllowGet);

        }
        [HttpPost]
        public ActionResult package_chapter(int[] checklistitem, DigiChampsModel.DigiChampsModuleModel obj1)
        {

            //List<DigiChampsModel.DigiChampsModuleModel> states1 = new List<DigiChampsModel.DigiChampsModuleModel>();
            List<DigiChampsModel.DigiChampsModuleModel> list = (List<DigiChampsModel.DigiChampsModuleModel>)Session["editpckgdtls"];
            DigiChampsModel.DigiChampsModuleModel obj = new DigiChampsModel.DigiChampsModuleModel();
            int Chkid = checklistitem.Count();

            string value1 = string.Empty;
            foreach (var item in checklistitem)
            {

                value1 += item.ToString() + ',';

            }
            value1 = value1.Substring(0, value1.Length - 1);

            var states1 = DbContext.SP_chapterList(value1).ToList();
            ViewBag.chapter = states1;
            obj.subdtls1 = states1;
            if (list != null)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    int chapid = Convert.ToInt32(list.ToList()[i].Chapter_Id);
                    string chapter = list.ToList()[i].Chapter;
                    string subject = list.ToList()[i].Subject;
                    int subjectid = Convert.ToInt32(list.ToList()[i].Subject_Id);
                    obj.subdtls1.Add(new SP_chapterList_Result
                    {

                        Chapter_Id = chapid,
                        Chapter = chapter,
                        Subject = subject,
                        Subject_Id = subjectid
                    });
                }
            }
            //obj.subdtls.Add = list;
            //list.ToList()[0].subdtls1 = states1;
            //ViewBag.chapter = obj.subdtls;
            // Chapter.Add(obj);
            ViewBag.tabledtls = null;
            Session["editpckgdtls"] = null;
            return Json(obj.subdtls1, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public ActionResult delete_chapter(int id, int packgid)
        {
            tbl_DC_Package_Dtl _pc_dtbl = DbContext.tbl_DC_Package_Dtl.Where(x => x.Chapter_Id == id && x.Package_ID == packgid && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
            if (_pc_dtbl != null)
            {
                _pc_dtbl.Is_Active = false;
                _pc_dtbl.Is_Deleted = true;
                DbContext.SaveChanges();
                return Json("cvv", JsonRequestBehavior.AllowGet);
            }
            return Json("cvv", JsonRequestBehavior.AllowGet);

        }

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
        public ActionResult GetAllChapter_pkg(int clsid)
        {
            List<SelectListItem> modulename = new List<SelectListItem>();

            List<DigiChampsModel.DigiChampsModuleModel> states = (from a in DbContext.tbl_DC_Board.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                                                  join b in DbContext.tbl_DC_Class.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                                                      on a.Board_Id equals b.Board_Id
                                                                  join c in DbContext.tbl_DC_Subject.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                                                      on b.Class_Id equals c.Class_Id
                                                                  join d in DbContext.tbl_DC_Chapter.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                                                      on c.Subject_Id equals d.Subject_Id
                                                                  join f in DbContext.tbl_DC_Module.Where(x => x.Class_Id == clsid && x.Is_Active == true && x.Is_Deleted == false)
                                                                      on d.Chapter_Id equals f.Chapter_Id
                                                                  select new DigiChampsModel.DigiChampsModuleModel
                                                                  {
                                                                      Subject_Id = c.Subject_Id,
                                                                      Subject = c.Subject,
                                                                      Chapter_Id = d.Chapter_Id,
                                                                      Chapter = d.Chapter,
                                                                      Module_ID = f.Module_ID,
                                                                      Module_Name = f.Module_Name
                                                                  }).OrderByDescending(x => x.Module_ID).ToList();

            states.ForEach(x =>
            {
                modulename.Add(new SelectListItem { Text = x.Subject.ToString() + " : " + x.Chapter.ToString(), Value = x.Module_ID.ToString() });
            });
            //ViewBag.state = new SelectList(dbContext.tbl_JV_State.Where(x => x.FK_Country_ID == conId && x.Is_Active == true && x.Is_Deleted == false), "PK_State_ID", "State_Name");

            return Json(modulename, JsonRequestBehavior.AllowGet);

        }
        [HttpPost]
        public ActionResult GetAllTopic(int SubId)
        {
            List<SelectListItem> ChaptNames = new List<SelectListItem>();
            List<tbl_DC_Topic> states = DbContext.tbl_DC_Topic.Where(x => x.Chapter_Id == SubId && x.Is_Active == true && x.Is_Deleted == false).ToList();
            states.ForEach(x =>
            {
                ChaptNames.Add(new SelectListItem { Text = x.Topic_Name, Value = x.Topic_ID.ToString() });
            });
            //ViewBag.state = new SelectList(dbContext.tbl_JV_State.Where(x => x.FK_Country_ID == conId && x.Is_Active == true && x.Is_Deleted == false), "PK_State_ID", "State_Name");

            return Json(ChaptNames, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetAllBatch_att()
        {
            List<SelectListItem> BatchNames = new List<SelectListItem>();
            List<tbl_DC_Batch> batches = DbContext.tbl_DC_Batch.Where(x => x.Is_Active == true && x.Is_Deleted == false).ToList();
            batches.ForEach(x =>
            {
                BatchNames.Add(new SelectListItem { Text = x.Batch_Name + " - " + x.Batch_Code, Value = x.Batch_Id.ToString() });
            });
            return Json(BatchNames, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetAllStudent_Batchwise(int Bid)
        {
            List<DigiChampsModel.DigiChampsAssignBatchModel> batch_stu = (from s in DbContext.tbl_DC_Student_Batch_Assign.Where(x=> x.Is_Active == true && x.Is_Deleted == false) join a in DbContext.tbl_DC_Batch_Assign.Where(x => x.Batch_Id == Bid && x.Is_Active == true && x.Is_Deleted == false) on s.Batch_Assign_Id equals a.Batch_Assign_Id
                                                                          join c in DbContext.tbl_DC_Class.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                                                          on a.Class_Id equals c.Class_Id
                                                                          join b in DbContext.tbl_DC_Registration.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                                                          on s.Regd_Id equals b.Regd_ID
                                                                          select new DigiChampsModel.DigiChampsAssignBatchModel
                                                                          {
                                                                              Class_Id = a.Class_Id,
                                                                              Class_Name = c.Class_Name,
                                                                              Regd_ID = b.Regd_ID,
                                                                              Customer_Name = b.Customer_Name
                                                                          }).ToList();
            return Json(batch_stu, JsonRequestBehavior.AllowGet);
        }
        #endregion

        public bool answer_mail(string parameter, string email, string name, string ticket_no)
        {
            var getall = DbContext.SP_DC_Get_maildetails(parameter).FirstOrDefault();
            string eidTo = email;
            string toshow = getall.SMTP_Sender.ToString();
            string from_mail = getall.SMTP_Email;
            string eidFrom = getall.SMTP_User.ToString();
            string password = getall.SMTP_Pwd.ToString();
            string ticket = ticket_no;
            string msgsub = getall.Email_Subject.Replace("{{ticketno}}", ticket);
            string hostname = getall.SMTP_HostName;
            string portname = getall.SMTP_Port.ToString();
            bool ssl_tof = true;
            string msgbody = getall.EmailConf_Body;

            msgbody = getall.EmailConf_Body.ToString().Replace("{{name}}", name).Replace("{{ticketno}}", ticket).Replace("{{date}}", DateTime.Now.ToString());


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

        #region----------------------------------------------------------Test Assign-----------------------------------------------------------
        [HttpGet]
        public ActionResult TestAssign()
        {

            if (Session["USER_CODE"] != null)
            {
                int teacher_id = Convert.ToInt32(Convert.ToString(Session["USER_CODE"]).Substring(1));
                ViewBag.Breadcrumb = "Assign test";

                ViewBag.ExamType = DbContext.tbl_DC_Exam_Type.Where(x => x.E_ID == 1).ToList();
            }
            else
            {
                return RedirectToAction("Logout", "Teacher");
            }
            return View();
        }
        [HttpPost]
        public ActionResult TestAssign(int Teacher_name)
        {
            if (Session["USER_CODE"] != null)
            {
                int teacher_id = Convert.ToInt32(Convert.ToString(Session["USER_CODE"]).Substring(1));
                ViewBag.ExamAssign = DbContext.SP_DC_TestAssign(Teacher_name).ToList();
                ViewBag.ExamType = DbContext.tbl_DC_Exam_Type.Where(x=> x.E_ID == 1).ToList();
            }
            else
            {
                return RedirectToAction("Logout", "Teacher");
            }
            return View();
        }

        public ActionResult Vwtestdtls(int id, int id1)
        {
            var _get_result_id = DbContext.tbl_DC_Exam_Result.Where(x => x.Regd_ID == id && x.Exam_ID == id1 && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
            int _resid = Convert.ToInt32(_get_result_id.Result_ID);
            var testdtls = DbContext.SP_DC_Startegic_Report(_resid).ToList();
            ViewBag.powers = (from a in DbContext.tbl_DC_Exam_Power.Where(x => x.Exam_ID == id1 && x.Is_Active == true && x.Is_Deleted == false)
                              join b in DbContext.tbl_DC_Power_Question.Where(x => x.Is_Active == true && x.Is_Deleted == false) on a.Power_Id equals b.Power_Id
                             select new DigiChamps.Models.DigiChampsModel.DigiChampsPowerModel
                             {
                                 Power_Type = b.Power_Type,
                                 no_of_ques = a.No_Of_Qstn,
                                 Power_Id = a.Power_Id,
                                 Exam_Id = a.Exam_ID
                             }).ToList();
           ViewBag.testdtls = testdtls;
           ViewBag.totalques = testdtls.Sum(x => x.Total_question);
           ViewBag.regdid = id;

           return PartialView("Vwtestdtls", testdtls);
        }

        [HttpPost]
        public ActionResult Vwtestdtls(string[] Topic_Name, string[] Total_Question, string[] correct_answer, string[] Percentage, string[] p_id, string[] P_Quantity, string[] E_id, string Redid, int[] No_Ques)
        {

            int Noques = No_Ques.Sum();

            var id = p_id.Distinct().Count();
            int j = 0;
            for (int i = 0; i < p_id.Length; i++)
            {
                if (i != 0)
                {
                    if (i % id == 0)
                    {

                        j = j + 1;
                    }
                }
                tbl_DC_Test_Assign tbdc = new tbl_DC_Test_Assign();
                tbdc.Regd_ID = Convert.ToInt32(Redid);
                tbdc.Topic_ID = Convert.ToInt32(Topic_Name[0 + j]);
                tbdc.No_Of_Ques = Convert.ToInt32(No_Ques[0 + j]);
                tbdc.No_Of_PowerQues = Convert.ToInt32(P_Quantity[i]);
                tbdc.Power_ID = Convert.ToInt32(p_id[i]);
                tbdc.Exam_ID = Convert.ToInt32(E_id[i]);
                tbdc.Is_Active = true;
                tbdc.Is_Deleted = false;
                DbContext.tbl_DC_Test_Assign.Add(tbdc);

                DbContext.SaveChanges();
                TempData["SuccessMessage"] = "Test assigned  successfully.";
            }


            return RedirectToAction("TestAssign");
        }

        #endregion

        #region---------------------------------------------------------Test Result------------------------------------------------------------

        public ActionResult TestResult()
        {
            try
            {
                if (Session["USER_CODE"] != null)
                {
                    int _Teacher_id = Convert.ToInt32(Convert.ToString(Session["USER_CODE"]).Substring(1));

                    var data = (from a in DbContext.tbl_DC_Registration.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                join b in DbContext.tbl_DC_Student_Batch_Assign.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                on a.Regd_ID equals b.Regd_Id
                                join c in DbContext.tbl_DC_Batch_Assign.Where(x => x.Teach_ID == _Teacher_id && x.Is_Active == true && x.Is_Deleted == false)
                                on b.Batch_Assign_Id equals c.Batch_Assign_Id
                                join d in DbContext.tbl_DC_Batch.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                on c.Batch_Id equals d.Batch_Id
                                select new TestResult
                                {
                                    Regd_ID = a.Regd_ID,
                                    Customer_Name = a.Customer_Name,
                                    Batch_Name = d.Batch_Name,
                                }).ToList();
                    ViewBag.test_result = data.ToList();
                }
            }
            catch (Exception ex)
            {
                ex.Message.ToString();
            }
            return View();
        }
        public ActionResult ViewResultDetail(int id)
        {
            try
            {
                if (Session["USER_CODE"] != null)
                {
                    var data = (from a in DbContext.tbl_DC_Exam_Result.Where(x => x.Regd_ID == id && x.Is_Active == true && x.Is_Deleted == false)
                                join b in DbContext.tbl_DC_Exam.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                on a.Exam_ID equals b.Exam_ID
                                select new TestResult
                                {
                                    Result_ID = a.Result_ID,
                                    Exam_Name = b.Exam_Name,
                                    StartTime = a.StartTime,
                                    EndTime = a.EndTime,
                                    Question_Nos = a.Question_Nos,
                                    Question_Attempted = a.Question_Attempted,
                                    Total_Correct_Ans = a.Total_Correct_Ans
                                }).ToList();
                    if (data != null)
                    {
                        ViewBag.test_result_dtl = data.ToList();
                    }
                    ViewBag.name = DbContext.tbl_DC_Registration.Where(x => x.Regd_ID == id).Select(x => x.Customer_Name).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                ex.Message.ToString();
            }
            return View();
        }

        #endregion

        #region----------------------------------------------------------Attendance------------------------------------------------------------
        [HttpGet]
        public ActionResult ViewAttendance()
        {
            ViewBag.Attendance = DbContext.Vw_Attendance_Report.ToList();
            return View();
        }

        [HttpGet]
        public ActionResult Attendance(int? Bid)
        {

            try
            {
                if(Session["USER_CODE"]!=null)
                {
                    ViewBag.Breadcrumb = "Attendance";
                    int teacher_id = Convert.ToInt32(Convert.ToString(Session["USER_CODE"]).Substring(1));
                    var attendance = DbContext.tbl_DC_Attendance.Where(x => x.Is_Present == true && x.Is_Active == true && x.Is_Delete == false).ToList();
                    var teacher_data = DbContext.SP_DC_teacher_All_Batch(teacher_id).ToList();
                    if (teacher_data.Count > 0)
                    {
                        ViewBag.batchofteacher = teacher_data;
                    }
                    else
                    {
                        ViewBag.batchofteacher = null;
                    }

                    var data = DbContext.tbl_DC_Batch.Where(x => x.Is_Active == true && x.Is_Deleted == false).ToList();
                    if (Bid != null && Bid != 0)
                    {
                        ViewBag.Batch_id = DbContext.tbl_DC_Batch.Where(x => x.Batch_Id == Bid && x.Is_Active == true && x.Is_Deleted == false).ToList();
                    }
                }
                else
                {
                    return RedirectToAction("Logout");
                }
               
            }
            catch (Exception ex)
            {
                  return View();
                
            }
            return View();
        }

        [HttpPost]
        public ActionResult Attendance(int? ddlbatch, int[] ddlRegd, int[] ddlClass, int[] ddlpresent, int? Attendance_Id, string Attend_Date)
        {
            try
            {
                if (Attend_Date != "")
                {
                    if (ddlpresent.Length > 0)
                    {
                        if (ddlRegd.Length > 0)
                        {
                            if (Attendance_Id == null)
                            {
                                for (int i = 0; i < ddlRegd.Length; i++)
                                {
                                    bool bu;
                                    int regdid = ddlRegd.ToList()[i];
                                    int teach_id = Convert.ToInt32(Convert.ToString(Session["USER_CODE"]).Substring(1));
                                    if (ddlpresent[i] == 1)
                                    {
                                        bu = true;
                                    }
                                    else
                                    {
                                        bu = false;
                                    }
                                    DateTime dt = Convert.ToDateTime(Attend_Date).Date;
                                    var Att_Date = DbContext.tbl_DC_Attendance.Where(x => x.Regd_Id == regdid && x.Is_Active == true && x.Is_Delete == false && x.Batch_Id == ddlbatch && x.Attendance_Date == dt).FirstOrDefault();
                                    if (Att_Date != null)
                                    {
                                        Att_Date.Is_Present = bu;
                                        DbContext.SaveChanges();
                                        TempData["SuccessMessage"] = "Attendance updated successfully.";
                                    }
                                    else
                                    {
                                        tbl_DC_Attendance obj = new tbl_DC_Attendance();
                                        obj.Regd_Id = regdid;
                                        obj.Batch_Id = Convert.ToInt32(ddlbatch);
                                        obj.Class_Id = ddlClass[i];
                                        obj.Teach_Id = teach_id;
                                        obj.Attendance_Date = dt;
                                        obj.Inserted_Date = today;
                                        obj.Inserted_By = HttpContext.User.Identity.Name;
                                        if (ddlpresent[i].ToString() == "1")
                                        {
                                            obj.Is_Present = true;
                                        }
                                        else
                                        {
                                            obj.Is_Present = false;
                                        }
                                        obj.Is_Active = true;
                                        obj.Is_Delete = false;
                                        DbContext.tbl_DC_Attendance.Add(obj);
                                        DbContext.SaveChanges();
                                        TempData["SuccessMessage"] = "Add attendance successfully.";

                                    }
                                }
                                return RedirectToAction("ViewAttendance");
                            }
                            else
                            {
                                TempData["WarningMessage"] = "Invalid attendance details.";
                            }
                        }
                        else
                        {
                            TempData["WarningMessage"] = "Invalid attendance details.";
                        }
                    }
                    else
                    {
                        TempData["WarningMessage"] = "Invalid attendance details.";
                    }
                }
                else
                {
                    TempData["WarningMessage"] = "Enter attendance date to search.";
                }
            }

            catch (Exception ex)
            {
                ex.Message.ToString();
                TempData["WarningMessage"] = "Something went wrong.";
            }
            return RedirectToAction("ViewAttendance");
        }

        public ActionResult Delete_Attendance(int? id)
        {
            try
            {
                if (id != null)
                {
                    tbl_DC_Attendance obj = DbContext.tbl_DC_Attendance.Where(x => x.Attendance_Id == id && x.Is_Active == true && x.Is_Delete == false).FirstOrDefault();
                    obj.Is_Active = false;
                    obj.Is_Delete = true;
                    obj.Modified_By = HttpContext.User.Identity.Name;
                    obj.Modified_Date = today;
                    DbContext.Entry(obj).State = EntityState.Modified;
                    DbContext.SaveChanges();
                    TempData["SuccessMessage"] = "Attendance deleted successfully.";
                    return RedirectToAction("ViewAttendance");
                }
            }
            catch (Exception ex)
            {
                ex.Message.ToString();
            }
            return View();
        }
        #endregion
    }

}
