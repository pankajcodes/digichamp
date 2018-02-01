using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using DigiChamps.Models;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Security;

namespace DigiChamps.Controllers
{
    public class ParentController : Controller
    {
        DigiChampsEntities DbContext = new DigiChampsEntities();
        DateTime today = DigiChampsModel.datetoserver();
        //
        // GET: /Parent/

        public ActionResult ParentDashboard()
        {
            try
            {
                ViewBag.Breadcrumb = "Dashboard";
                if (Session["USER_CODE"] != null)
                {
                    int sid = Convert.ToInt32(Convert.ToString(Session["USER_CODE"]).Substring(1));
                    var data1 = (from k in DbContext.tbl_DC_Registration.Where(x => x.Regd_ID == sid && x.Is_Active == true && x.Is_Deleted == false)
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
                    //result id as parameter
                    if (data1 != null)
                    {
                        int bid = Convert.ToInt32(data1.ToList()[0].Board_Id);
                        int cls_id = Convert.ToInt32(data1.ToList()[0].Class_Id);
                        var packagename = (from d in DbContext.VW_DC_Package_Learn.Where(x => x.Regd_ID == sid && x.Class_ID == cls_id).GroupBy(x => x.Package_Name)
                                           select new DigiChampCartModel
                                           {
                                               Package_Name = d.FirstOrDefault().Package_Name,
                                               Package_ID = d.FirstOrDefault().Package_ID,
                                               Package_Desc = d.FirstOrDefault().Package_Desc

                                           }).ToList();
                        ViewBag.packagename = packagename;
                        ViewBag.packagecount = packagename.Count;
                        ViewBag.exams = DbContext.tbl_DC_Exam_Result.Where(x => x.Is_Active == true && x.Is_Deleted == false && x.Regd_ID == sid).ToList().Count(); // Total exams given
                        var onlinemarks = DbContext.SP_DC_Parentg1(sid, 5).ToList();
                        if (onlinemarks.Count > 0)
                        {
                            string result = string.Empty;
                            for (int i = 0; i < onlinemarks.Count; i++)
                            {
                                var id = onlinemarks.ToList()[i].Result_ID;
                                if (id != null && id != 0)
                                {
                                    result += DbContext.SP_DC_Parentg2(id).ToList()[0].percenatge + ",";
                                }
                            }
                            string[] resultonline = result.Split(',');
                            if (onlinemarks.Count == 1)
                            {
                                if (resultonline[0] != "")
                                {
                                    ViewBag.onlinetest1 = resultonline[0];
                                    ViewBag.onlinetest2 = 0;
                                    ViewBag.onlinetest3 = 0;

                                }
                                else
                                {
                                    ViewBag.onlinetest1 = 0;

                                }
                            }
                            if (onlinemarks.Count == 2)
                            {
                                if (resultonline[0] != "")
                                {
                                    ViewBag.onlinetest1 = resultonline[0];
                                }
                                else
                                {
                                    ViewBag.onlinetest1 = 0;

                                }
                                if (resultonline[1] != "")
                                {
                                    ViewBag.onlinetest2 = resultonline[1];
                                    ViewBag.onlinetest3 = 0;
                                }
                                else
                                {
                                    ViewBag.onlinetest2 = 0;
                                    ViewBag.onlinetest3 = 0;

                                }
                            }
                            if (onlinemarks.Count == 3)
                            {
                                if (resultonline[0] != "")
                                {
                                    ViewBag.onlinetest1 = resultonline[0];
                                }
                                else
                                {
                                    ViewBag.onlinetest1 = 0;

                                }
                                if (resultonline[1] != "")
                                {
                                    ViewBag.onlinetest2 = resultonline[1];
                                }
                                else
                                {
                                    ViewBag.onlinetest2 = 0;

                                }
                                if (resultonline[2] != "")
                                {
                                    ViewBag.onlinetest3 = resultonline[2];
                                }
                                else
                                {
                                    ViewBag.onlinetest3 = 0;

                                }
                            }

                        }
                        else
                        {
                            ViewBag.onlinetest1 = 0;
                            ViewBag.onlinetest2 = 0;
                            ViewBag.onlinetest3 = 0;

                        }

                        var prereqmarks = DbContext.SP_DC_Parentg1(sid, 1).ToList();
                        if (prereqmarks.Count > 0)
                        {
                            string result1 = string.Empty;
                            for (int i = 0; i < prereqmarks.Count; i++)
                            {
                                var id = prereqmarks.ToList()[i].Result_ID;
                                if (id != null && id != 0)
                                {
                                    result1 += DbContext.SP_DC_Parentg2(id).ToList()[0].percenatge + ",";
                                }
                            }
                            string[] resultprereqmarks = result1.Split(',');
                            if (prereqmarks.Count == 1)
                            {
                                if (resultprereqmarks[0] != "")
                                {
                                    ViewBag.prereqtest1 = resultprereqmarks[0];
                                    ViewBag.prereqtest2 = 0;
                                    ViewBag.prereqtest3 = 0;

                                }
                                else
                                {
                                    ViewBag.prereqtest1 = 0;
                                    ViewBag.prereqtest2 = 0;
                                    ViewBag.prereqtest3 = 0;


                                }
                            }
                            if (prereqmarks.Count == 2)
                            {
                                if (resultprereqmarks[0] != "")
                                {
                                    ViewBag.prereqtest1 = resultprereqmarks[0];
                                }
                                else
                                {
                                    ViewBag.prereqtest1 = 0;

                                }
                                if (resultprereqmarks[1] != "")
                                {
                                    ViewBag.prereqtest2 = resultprereqmarks[1];
                                    ViewBag.prereqtest3 = 0;
                                }
                                else
                                {
                                    ViewBag.prereqtest2 = 0;

                                }
                            }
                            if (prereqmarks.Count == 3)
                            {
                                if (resultprereqmarks[0] != "")
                                {
                                    ViewBag.prereqtest1 = resultprereqmarks[0];
                                }
                                else
                                {
                                    ViewBag.prereqtest1 = 0;

                                }
                                if (resultprereqmarks[1] != "")
                                {
                                    ViewBag.prereqtest2 = resultprereqmarks[1];
                                }
                                else
                                {
                                    ViewBag.prereqtest2 = 0;

                                }
                                if (resultprereqmarks[2] != "")
                                {
                                    ViewBag.prereqtest3 = resultprereqmarks[2];
                                }
                                else
                                {
                                    ViewBag.prereqtest3 = 0;

                                }
                            }
                        }
                        else
                        {
                            ViewBag.prereqtest1 = 0;
                            ViewBag.prereqtest2 = 0;
                            ViewBag.prereqtest3 = 0;

                        }

                        var practicemarks = DbContext.SP_DC_Parentg1(sid, 3).ToList();
                        if (practicemarks.Count < 3)
                        {
                            string result2 = string.Empty;
                            for (int i = 0; i < practicemarks.Count; i++)
                            {
                                var id = practicemarks.ToList()[i].Result_ID;
                                if (id != null && id != 0)
                                {

                                    result2 += DbContext.SP_DC_Parentg2(id).ToList()[0].percenatge + ",";
                                }
                            }
                            string[] resultpracticemarks = result2.Split(',');
                            if (practicemarks.Count == 1)
                            {
                                if (resultpracticemarks[0] != "")
                                {
                                    ViewBag.practicetest1 = resultpracticemarks[0];
                                    ViewBag.practicetest2 = 0;
                                    ViewBag.practicetest3 = 0;

                                }
                                else
                                {
                                    ViewBag.practicetest1 = 0;
                                    ViewBag.practicetest2 = 0;
                                    ViewBag.practicetest3 = 0;

                                }
                            }
                            if (practicemarks.Count == 2)
                            {
                                if (resultpracticemarks[0] != "")
                                {
                                    ViewBag.practicetest1 = resultpracticemarks[0];
                                }
                                else
                                {
                                    ViewBag.practicetest1 = 0;
                                    ViewBag.practicetest2 = 0;
                                    ViewBag.practicetest3 = 0;
                                }
                                if (resultpracticemarks[1] != "")
                                {
                                    ViewBag.practicetest2 = resultpracticemarks[1];
                                    ViewBag.practicetest3 = 0;
                                }
                                else
                                {
                                    ViewBag.practicetest2 = 0;
                                    ViewBag.practicetest3 = 0;

                                }
                            }
                            if (practicemarks.Count == 3)
                            {
                                if (resultpracticemarks[0] != "")
                                {
                                    ViewBag.practicetest1 = resultpracticemarks[0];
                                }
                                else
                                {
                                    ViewBag.practicetest1 = 0;

                                }
                                if (resultpracticemarks[1] != "")
                                {
                                    ViewBag.practicetest2 = resultpracticemarks[1];
                                }
                                else
                                {
                                    ViewBag.practicetest2 = 0;

                                }
                                if (resultpracticemarks[2] != "")
                                {
                                    ViewBag.practicetest3 = resultpracticemarks[2];
                                }
                                else
                                {
                                    ViewBag.practicetest3 = 0;

                                }
                            }

                        }

                        else
                        {
                            ViewBag.practicetest1 = 0;
                            ViewBag.practicetest2 = 0;
                            ViewBag.practicetest3 = 0;

                        }
                        var retestmarks = DbContext.SP_DC_Parentg1(sid, 2).ToList();
                        if (retestmarks.Count > 0)
                        {
                            string result4 = string.Empty;
                            for (int i = 0; i < retestmarks.Count; i++)
                            {
                                var id = retestmarks.ToList()[i].Result_ID;
                                if (id != null && id != 0)
                                {
                                    result4 += DbContext.SP_DC_Parentg2(id).ToList()[0].percenatge + ",";
                                }
                            }
                            string[] resulttestmarks = result4.Split(',');
                            if (retestmarks.Count == 1)
                            {
                                if (resulttestmarks[0] != "")
                                {
                                    ViewBag.retest1 = resulttestmarks[0];
                                    ViewBag.retest2 = 0;
                                    ViewBag.retest3 = 0;
                                }
                                else
                                {
                                    ViewBag.retest1 = 0;
                                    ViewBag.retest2 = 0;
                                    ViewBag.retest3 = 0;
                                }
                            }
                            if (retestmarks.Count == 2)
                            {
                                if (resulttestmarks[0] != "")
                                {
                                    ViewBag.retest1 = resulttestmarks[0];
                                }
                                else
                                {
                                    ViewBag.retest1 = 0;
                                    ViewBag.retest2 = 0;
                                    ViewBag.retest3 = 0;
                                }
                                if (resulttestmarks[1] != "")
                                {
                                    ViewBag.retest2 = resulttestmarks[1];
                                    ViewBag.retest3 = 0;
                                }
                                else
                                {
                                    ViewBag.retest2 = 0;
                                    ViewBag.retest3 = 0;
                                }
                            }
                            if (retestmarks.Count == 3)
                            {
                                if (resulttestmarks[0] != "")
                                {
                                    ViewBag.retest1 = resulttestmarks[0];
                                }
                                else
                                {
                                    ViewBag.retest1 = 0;
                                }
                                if (resulttestmarks[1] != "")
                                {
                                    ViewBag.retest2 = resulttestmarks[1];
                                }
                                else
                                {
                                    ViewBag.retest2 = 0;
                                    ViewBag.retest3 = 0;
                                }
                                if (resulttestmarks[2] != "")
                                {
                                    ViewBag.retest3 = resulttestmarks[2];
                                }
                                else
                                {
                                    ViewBag.retest3 = 0;
                                }
                            }
                        }
                        else
                        {
                            ViewBag.retest1 = 0;
                            ViewBag.retest2 = 0;
                            ViewBag.retest3 = 0;
                        }

                    }
                }
            }
            catch (Exception ex)
            {

                return View();
            }

            return View();
        }

        #region-----------------------------------------------Parent Primarystep--------------------------------------------------
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(string Username, string password)
        {
            string msg =  string.Empty;
            try
            {
                string pass_word = DigiChampsModel.Encrypt_Password.HashPassword(password);
                var obj = DbContext.tbl_DC_USER_SECURITY.Where(x => x.USER_NAME == Username && x.PASSWORD == pass_word && x.ROLE_CODE == "P").FirstOrDefault();

                if (obj != null)
                {
                    if (obj.PASSWORD == pass_word)
                    {
                        if (obj.ROLE_CODE == "P")
                        {
                            loginstatus(Convert.ToString(obj.USER_CODE));
                            Session["USER_CODE"] = obj.USER_CODE;
                            Session["USER_NAME"] = obj.USER_NAME;
                            Session["ROLE_CODE"] = obj.ROLE_CODE;
                            msg = "1"; // "Successfully logged in"
                        }
                        else
                        {
                            msg = "Please login as a parent";
                        }
                    }

                    else
                    {
                        msg = "-3"; // "Password doesnot match.";
                    }
                }
                else
                {
                    msg = "-1";   // "User name is not yet registered.";
                }
            }
            catch (Exception ex)
            {
                ex.Message.ToString();
            }
            return Json(msg, JsonRequestBehavior.AllowGet);
        }

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
                    Session["Time_for_login"] = id_found.Login_DateTime;
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

        public ActionResult Logout()
        {
            logoutstatus(Convert.ToString(Session["USER_CODE"]));
            Session.Abandon();
            FormsAuthentication.SignOut();
            return RedirectToAction("Index");
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

        [HttpPost]
        public JsonResult ForgotPassword(string mobile)
        {
            string msg = string.Empty;
            try
            {
                if (mobile != "")
                {
                    tbl_DC_Registration obj = DbContext.tbl_DC_Registration.Where(x => x.Parent_Mobile == mobile).FirstOrDefault();
                    if (obj == null)
                    {
                        msg = "0";  // "Mobile number is not yet registered";
                    }
                    else
                    {
                        int reg_id = Convert.ToInt32(obj.Regd_ID);
                        Session["Reg_Id"] = reg_id;

                        tbl_DC_OTP obj1 = DbContext.tbl_DC_OTP.Where(x => x.Regd_Id == reg_id && x.Mobile == mobile).FirstOrDefault();
                        if (obj1 != null)   //if already generated
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

                                var sms_obj = DbContext.View_DC_SMS_API.Where(x => x.Sms_Alert_Name == "FRGPASS").FirstOrDefault();
                                if (sms_obj != null)
                                {
                                    string message = sms_obj.Sms_Body;
                                    var regex = new Regex(Regex.Escape("{{otpno}}"));
                                    var newText = regex.Replace(message, "" + obj1.OTP + "", 9);

                                      string baseurl = "" + sms_obj.Api_Url.ToString().Replace("mobile", mobile).Replace("message", newText);

                                    HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(baseurl);
                                    //Get response from the SMS Gateway Server and read the answer
                                    HttpWebResponse myResp = (HttpWebResponse)myReq.GetResponse();
                                    System.IO.StreamReader respStreamReader = new System.IO.StreamReader(myResp.GetResponseStream());
                                    string responseString = respStreamReader.ReadToEnd();
                                    respStreamReader.Close();
                                    myResp.Close();
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

                                var sms_obj = DbContext.View_DC_SMS_API.Where(x => x.Sms_Alert_Name == "FRGPASS").FirstOrDefault();
                                if (sms_obj != null)
                                {
                                    string message = sms_obj.Sms_Body;
                                    var regex = new Regex(Regex.Escape("{{otpno}}"));
                                    var newText = regex.Replace(message, "" + obj1.OTP + "", 9);
                                      string baseurl = "" + sms_obj.Api_Url.ToString().Replace("mobile", mobile).Replace("message", newText);

                                    HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(baseurl);
                                    //Get response from the SMS Gateway Server and read the answer
                                    HttpWebResponse myResp = (HttpWebResponse)myReq.GetResponse();
                                    System.IO.StreamReader respStreamReader = new System.IO.StreamReader(myResp.GetResponseStream());
                                    string responseString = respStreamReader.ReadToEnd();
                                    respStreamReader.Close();
                                    myResp.Close();
                                }
                                msg = "1";  //redirect to otp conform OTP_Confirmation
                            }
                            else
                            {
                                msg = "-2"; //You have already requested for Maximum no of OTP.
                            }
                        }
                        else {  //new to forgot password
                            tbl_DC_OTP obj_otp = new tbl_DC_OTP();
                            obj_otp.Mobile = mobile.Trim();
                            obj_otp.Regd_Id = reg_id;
                            string otp = random_password(6);
                            obj_otp.OTP = otp;
                            obj_otp.From_Date = today;
                            obj_otp.Count = 1;
                            obj_otp.To_Date = Convert.ToDateTime(today.AddHours(1));
                            DbContext.tbl_DC_OTP.Add(obj_otp);
                            DbContext.SaveChanges();

                            var sms_obj = DbContext.View_DC_SMS_API.Where(x => x.Sms_Alert_Name == "FRGPASS").FirstOrDefault();
                            if (sms_obj != null)
                            {
                                string message = sms_obj.Sms_Body;
                                var regex = new Regex(Regex.Escape("{{otpno}}"));
                                var newText = regex.Replace(message, "" + obj_otp.OTP + "", 9);
                                  string baseurl = "" + sms_obj.Api_Url.ToString().Replace("mobile", mobile).Replace("message", newText);

                                HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(baseurl);
                                //Get response from the SMS Gateway Server and read the answer
                                HttpWebResponse myResp = (HttpWebResponse)myReq.GetResponse();
                                System.IO.StreamReader respStreamReader = new System.IO.StreamReader(myResp.GetResponseStream());
                                string responseString = respStreamReader.ReadToEnd();
                                respStreamReader.Close();
                                myResp.Close();
                            }
                            msg = "1";  //redirect to otp conform OTP_Confirmation
                        }
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
        public JsonResult OTP_Confirmation(string Mobile_OTP, string New_Password, string Confirm_Password)
        {
            string msg = string.Empty; ;
            try
            {
                if (Session["Reg_Id"] != null)
                {
                    #region Forgot password OTP
                    int regdid = Convert.ToInt16(Session["Reg_Id"].ToString());
                    tbl_DC_Registration obj = DbContext.tbl_DC_Registration.Where(x => x.Regd_ID == regdid).FirstOrDefault();
                    if (obj == null)
                    {
                        msg = "-1";  // "Mobile number is not yet registered";
                        Session["Reg_Id"] = null;
                    }
                    else
                    {
                        var data1 = DbContext.tbl_DC_OTP.Where(x => x.Regd_Id == regdid && x.Mobile == obj.Parent_Mobile).FirstOrDefault();
                        if (data1 != null)
                        {
                            if (data1.OTP == Mobile_OTP)
                            {
                                if (data1.To_Date > today)
                                {
                                    string ucode = "P0" + Convert.ToString(regdid);
                                    if (New_Password == Confirm_Password)
                                    {
                                        tbl_DC_USER_SECURITY secobj = DbContext.tbl_DC_USER_SECURITY.Where(x => x.USER_CODE == ucode && x.ROLE_CODE == "P").FirstOrDefault();

                                        secobj.PASSWORD = DigiChampsModel.Encrypt_Password.HashPassword(New_Password).ToString();
                                        secobj.STATUS = "A";
                                        secobj.IS_ACCEPTED = true;
                                        DbContext.Entry(obj).State = EntityState.Modified;
                                        DbContext.SaveChanges();
                                        msg = "1";
                                        Session["Reg_Id"] = null;
                                    }
                                    else
                                    {
                                        msg = "0";  //not match
                                    }
                                }
                                else
                                {
                                    msg = "-2"; //expires
                                }

                            }

                            else
                            {
                                msg = "-11";    //Invalid OTP
                            }
                        }
                        else
                        {
                            msg = "-1";
                        }
                    }
                    #endregion
                }
                else
                {
                    msg = "-1";
                }
            }
            catch (Exception ex)
            {
                ex.Message.ToString();
            }
            return Json(msg, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult ChangePassword()
        {
            return View();
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
                    var data = DbContext.tbl_DC_USER_SECURITY.Where(x => x.USER_CODE == u_code && x.PASSWORD == old_pass_word && x.STATUS == "A" && x.ROLE_CODE == "P").FirstOrDefault();

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
        #endregion

        #region---------------------------------------------------Autogenerate OTP------------------------------------------------
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

        #region----------------------------------------------------Resend otp-----------------------------------------------------
        public JsonResult Resend_OTP(int? count)
        {
            string msg = string.Empty;
            try
            {
                if (Session["Reg_Id"] != null)
                {
                    int rgt_id = Convert.ToInt32(Session["Reg_Id"]);
                    string usercode = "P0" + Session["Reg_Id"].ToString();
                    var chk_type = DbContext.tbl_DC_USER_SECURITY.Where(x => x.USER_CODE == usercode && x.ROLE_CODE == "P").FirstOrDefault();

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

                                var sms_obj = DbContext.View_DC_SMS_API.Where(x => x.Sms_Alert_Name == "FRGPASS").FirstOrDefault();
                                if (sms_obj != null)
                                {
                                    string message = sms_obj.Sms_Body;
                                    var regex = new Regex(Regex.Escape("{{otpno}}"));
                                    var newText = regex.Replace(message, "" + widget.OTP + "", 9);
                                    string baseurl = "" + sms_obj.Api_Url + "?uname=" + sms_obj.Username + "&pass=" + sms_obj.Password + "&send=" + sms_obj.Sender_Type + "&dest=" + widget.Mobile.Trim() + "&msg=" + newText.ToString() + "&priority=1";

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
                }
                else if (Session["Rg_Id"] != null)
                {
                    int rgt_id = Convert.ToInt32(Session["Rg_Id"]);
                    string usercode = "P0" + Session["Rg_Id"].ToString();
                    var chk_type = DbContext.tbl_DC_USER_SECURITY.Where(x => x.USER_CODE == usercode && x.ROLE_CODE == "P").FirstOrDefault();

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

                                var sms_obj = DbContext.View_DC_SMS_API.Where(x => x.Sms_Alert_Name == "REG").FirstOrDefault();
                                if (sms_obj != null)
                                {
                                    string message = sms_obj.Sms_Body;
                                    var regex = new Regex(Regex.Escape("{{otpno}}"));
                                    var newText = regex.Replace(message, "" + widget.OTP + "", 9);
                                    string baseurl = "" + sms_obj.Api_Url + "?uname=" + sms_obj.Username + "&pass=" + sms_obj.Password + "&send=" + sms_obj.Sender_Type + "&dest=" + widget.Mobile.Trim() + "&msg=" + newText.ToString() + "&priority=1";

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

        public JsonResult Parent_GetNotification()
        {
            var notoficationt_time = Session["LastUpDated"] != null ? Convert.ToDateTime(Session["LastUpdated"]) : DateTime.Now;
            NotificationComponent nc = new NotificationComponent();
            var list = nc.GetContacts(notoficationt_time);
            Session["LastUpdate"] = DateTime.Now;
            return new JsonResult{Data = list, JsonRequestBehavior = JsonRequestBehavior.AllowGet};
        }

        #region----------------------------------------------------Exam result-----------------------------------------------------------
        [HttpGet]
        public ActionResult testresult()
        {
            int id = 0;
            if (Session["USER_CODE"] != null)
            {
                id = Convert.ToInt32(Convert.ToString(Session["USER_CODE"]).Substring(1));
                ViewBag.retestassign = DbContext.SP_DC_TestAssign(1).Where(x => x.Regd_ID == id).FirstOrDefault();
            }
            var online1 = DbContext.SP_DC_Parentg1(id, 5).ToList();
            int count = 0;
            foreach (var item in online1)
            {
                if (item.Result_ID != null)
                {
                    count += 1;
                }
            }
            ViewBag.online1 = count;

            var global = DbContext.SP_DC_Parentg1(id, 4).ToList();
            int count1 = 0;
            foreach (var item in global)
            {
                if (item.Result_ID != null)
                {
                    count1 += 1;
                }
            }
            ViewBag.global = count1;

            var offline = DbContext.VW_Offline_Student_Orders.Where(x => x.Regd_ID == id).FirstOrDefault();
            if (offline != null)
            {
                string id1 = Convert.ToString(id);
                ViewBag.test1 = DbContext.SP_Offline_Exams_Count(id1, 1).FirstOrDefault();
                ViewBag.test2 = DbContext.SP_Offline_Exams_Count(id1, 3).FirstOrDefault();

                ViewBag.test3 = DbContext.SP_Offline_Exams_Count(id1, 2).FirstOrDefault();
                ViewBag.ofline = null;
            }
            else
            {
                ViewBag.ofline = "blank";
            }
            return View();
        }

        [HttpGet]
        public ActionResult resultview(int id)
        {
            if (Session["USER_CODE"] != null)
            {
                int sid = Convert.ToInt32(Convert.ToString(Session["USER_CODE"]).Substring(1));
                ViewBag.exams = DbContext.SP_DC_Parentg1(sid, id).Where(x=> x.Result_ID != null).ToList();
            }
            return View();
        }
        [HttpPost]
        public JsonResult answerdetail(int id)
        {
                int sid = Convert.ToInt32(Convert.ToString(Session["USER_CODE"]).Substring(1));
                var result = DbContext.SP_DC_Parentg2(id).ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult examtopicdetails(string id)
        {
            try
            {
                uint out_id;
                if (Session["USER_CODE"] != null)
                {
                    if (uint.TryParse(id, out out_id) == true)
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
                            ViewBag.studentid = sid;
                        }
                    }
                    return View();
                }
                else
                {
                    return RedirectToAction("logout", "Parent");
                }
            }
            catch { return RedirectToAction("logout", "Parent"); }
        }
        #endregion

        #region------------------------------------------------Parent Attendance---------------------------------------------------------
        [HttpGet]
        public ActionResult attendance()
        {
            try{
            if (Session["USER_CODE"] != null)
            {
                int regid = Convert.ToInt32(Convert.ToString(Session["USER_CODE"]).Substring(1));
                ViewBag.Parent_Attendance = DbContext.Vw_Attendance_Report.Where(x => x.Regd_ID == regid).ToList();
            }
            }
            catch{}
                return View();
        }
        [HttpPost]
        public ActionResult attendance(string frm_dt, string to_dt)
        {
            try
            {
                if (Session["USER_CODE"] != null)
                {
                    if (frm_dt != "" && to_dt != "")
                    {
                        if (Convert.ToDateTime(frm_dt) <= today.Date && Convert.ToDateTime(to_dt) <= today.Date)
                        {
                            if (Convert.ToDateTime(frm_dt) <= Convert.ToDateTime(to_dt))
                            {

                                string fdtt = frm_dt + " 00:00:00 AM";
                                string tdtt = to_dt + " 23:59:59 PM";

                                DateTime from_date1 = Convert.ToDateTime(fdtt);
                                DateTime to_date1 = Convert.ToDateTime(tdtt);

                                int regid = Convert.ToInt32(Convert.ToString(Session["USER_CODE"]).Substring(1));
                                var attendance_status = (from c in DbContext.Vw_Attendance_Report where c.Attendance_Date >= from_date1 && c.Attendance_Date <= to_date1 select c).OrderByDescending(c => c.Attendance_Date).Where(x => x.Regd_ID == regid);
                                if (attendance_status != null)
                                {
                                    ViewBag.Stu_Attendance = attendance_status.ToList();
                                }
                                else { ViewBag.Stu_Attendance = null; }
                            }
                            else
                            {
                                TempData["ErrorMessage"] = "From date should be less or equal to todate.";
                            }
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "Date should be less or equal to today date.";
                        }
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Please Select Dates.";
                    }
                }
                else {
                    return RedirectToAction("Logout");
                }
            }

            catch (Exception ex)
            {
                ex.Message.ToString();
            }
            return View();
        }
        #endregion

        #region-------------------------------------------------Parent Timetable---------------------------------------------------------
        [HttpGet]
        public ActionResult timetable()
        {
            try
            {
                if (Session["USER_CODE"] != null)
                {
                    int regid = Convert.ToInt32(Convert.ToString(Session["USER_CODE"]).Substring(1));

                    var data = (from a in DbContext.tbl_DC_Batch.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                join b in DbContext.tbl_DC_Batch_Assign.Where(x =>x.Is_Active == true && x.Is_Deleted == false) on a.Batch_Id equals b.Batch_Id join c in DbContext.tbl_DC_Student_Batch_Assign.Where(x=> x.Regd_Id == regid && x.Is_Active == true && x.Is_Deleted == false) on b.Batch_Assign_Id equals c.Batch_Assign_Id
                                select new DigiChampsModel.DigiChampsBatchModel
                                {
                                    Batch_Name = a.Batch_Name,
                                    Batch_From_Time = a.Batch_From_Time,
                                    Batch_To_Time = a.Batch_To_Time,
                                    Batch_Days = a.Batch_Days,
                                    Inserted_Date=c.Inserted_Date

                                }).ToList().Take(500);
                    ViewBag.TimeTable_Details_parent = data;
                }
            }
            catch { 
            
            }

            return View();
        }



        [HttpPost]
        public ActionResult timetable(string t_Date, string f_Date)
        {
            try
            {
                if (f_Date != "" && t_Date != "")
                {
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
                                if (Session["USER_CODE"] != null)
                                {
                                    int regid = Convert.ToInt32(Convert.ToString(Session["USER_CODE"]).Substring(1));

                                    var data = (from a in DbContext.tbl_DC_Batch.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                                join b in DbContext.tbl_DC_Batch_Assign.Where(x => x.Is_Active == true && x.Is_Deleted == false) on a.Batch_Id equals b.Batch_Id
                                                join c in DbContext.tbl_DC_Student_Batch_Assign.Where(x => x.Regd_Id == regid && x.Is_Active == true && x.Is_Deleted == false && x.Inserted_Date >= fdt && x.Inserted_Date <= tdt).OrderByDescending(x => x.Inserted_Date) on b.Batch_Assign_Id equals c.Batch_Assign_Id
                                                select new DigiChampsModel.DigiChampsBatchModel
                                                {
                                                    Batch_Name = a.Batch_Name,
                                                    Batch_From_Time = a.Batch_From_Time,
                                                    Batch_To_Time = a.Batch_To_Time,
                                                    Batch_Days = a.Batch_Days,
                                                    Inserted_Date = c.Inserted_Date

                                                }).ToList().Take(500);
                                    ViewBag.TimeTable_Details_parent = data;
                                }
                                else
                                {
                                    return RedirectToAction("Logout");
                                }
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
            }
            catch (Exception ex)
            {
                
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return View("timetable");
        }
        #endregion

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
    }

    
}
