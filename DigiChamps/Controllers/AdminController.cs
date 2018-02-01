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
using System.Xml;
using System.Xml.Linq;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net.Mail;
using System.Data.OleDb;
using System.Text.RegularExpressions;
using System.Collections.Specialized;
using Newtonsoft.Json.Linq;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using DigiChamps.Common;
namespace DigiChamps.Controllers
{
    public class AdminController : Controller
    {
        DigiChampsEntities DbContext = new DigiChampsEntities();
        DateTime today = DigiChampsModel.datetoserver();
        //
        // GET: /Admin/

        public ActionResult Index()
        {
            return RedirectToAction("Login");
        }
        [HttpGet]
        public ActionResult Login()
        {
            if (Session["USER_CODE"] != null)
            {
                if (Session["ROLE_CODE"].ToString() == "A" || Session["ROLE_CODE"].ToString() == "S")
                {
                    return RedirectToAction("AdminDashboard");
                }
                else
                {
                    return RedirectToAction("Logout");
                }
            }
            else
            {
                return View();
            }
        }
        public ActionResult Logout()
        {
            logoutstatus(Convert.ToString(Session["USER_CODE"]));
            Session.Abandon();
            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }

        [HttpPost]
        public ActionResult Login(string User_name, string password)
        {
            try
            {
                string pass_word = "test123";//DigiChampsModel.Encrypt_Password.HashPassword(password);
                var obj = DbContext.tbl_DC_USER_SECURITY.Where(x => x.USER_NAME == User_name && x.PASSWORD == pass_word && x.IS_ACCEPTED == true).FirstOrDefault();

                //var obj1 = DbContext.tbl_DC_USER_SECURITY.Where(x => x.USER_NAME == User_name && x.PASSWORD == password).FirstOrDefault();

                if (obj != null)
                {
                    if (obj.ROLE_CODE == "A")
                    {
                        loginstatus(Convert.ToString(obj.USER_CODE));
                        FormsAuthentication.RedirectFromLoginPage(obj.USER_NAME, false);
                        Session["USER_CODE"] = obj.USER_CODE;
                        TempData["USER_CODE"] = obj.USER_CODE;
                        TempData["USER_NAME"] = obj.USER_NAME;
                        Session["ROLE_CODE"] = obj.ROLE_CODE;
                        Session["username"] = obj.USER_NAME;
                        Session["Time"] = today.ToShortTimeString();
                        return RedirectToAction("AdminDashboard");
                    }
                    else if (obj.ROLE_CODE == "S")
                    {
                        var obj1 = DbContext.tbl_DC_USER_MASTER.Where(x => x.EMAIL == User_name).FirstOrDefault();
                        if (obj1 != null)
                        {
                            Session["Role_Id"] = Convert.ToInt32(obj1.ROLE_TYPE);
                            Session["username"] = obj1.USER_NAME;

                        }
                        loginstatus(Convert.ToString(obj.USER_CODE));
                        FormsAuthentication.RedirectFromLoginPage(obj.USER_NAME, false);
                        Session["USER_CODE"] = obj.USER_CODE;
                        TempData["USER_CODE"] = obj.USER_CODE;
                        TempData["USER_NAME"] = obj.USER_NAME;
                        Session["ROLE_CODE"] = obj.ROLE_CODE;
                        Session["Time"] = today.ToShortTimeString();
                        return RedirectToAction("AdminDashboard");
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Invalid credential for admin login.";
                        return View();
                    }

                }
                else
                {
                    var schoolUser = DbContext.tbl_DC_SchoolUser.Where(x => x.UserName == User_name && x.UserPassword == password && x.IsActive == true).FirstOrDefault();

                    //var obj1 = DbContext.tbl_DC_USER_SECURITY.Where(x => x.USER_NAME == User_name && x.PASSWORD == password).FirstOrDefault();

                    if (schoolUser != null)
                    {
                        if (schoolUser.UserRole == "SchoolAdmin")
                        {
                            // return RedirectToAction("SchoolDashboard","School",schoolUser.SchoolId);
                            return RedirectToAction("SchoolDashboard", "School", new { @id = schoolUser.SchoolId });
                        }

                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Invalid user name or password.";
                        return View();
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return View();
        }

        [HttpPost]
        public JsonResult ForgotPassword(string email)
        {
            string message;
            try
            {
                var find_mail = DbContext.tbl_DC_USER_SECURITY.Where(x => x.USER_NAME == email && x.ROLE_CODE == "S").FirstOrDefault();
                if (find_mail != null)
                {
                    tbl_DC_USER_MASTER obj = DbContext.tbl_DC_USER_MASTER.Where(x => x.EMAIL == email && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    if (obj != null)
                    {
                        string password = CreateRandomPassword(6);
                        string encrypt_pass = DigiChampsModel.Encrypt_Password.HashPassword(password);

                        find_mail.PASSWORD = encrypt_pass;
                        DbContext.Entry(find_mail).State = EntityState.Modified;
                        DbContext.SaveChanges();
                        Admin_Mail("T_F_PASS", email, password, email, obj.USER_NAME);
                        message = "<i class='fa fa-check-square'></i> Login details successfully sent to the registred mailid.";
                    }
                    else
                    {
                        message = "<i class='fa fa-exclamation-circle'></i> User details does not exist, Please contact adminstrator.";
                    }

                }
                else
                {
                    message = "<i class='fa fa-exclamation-triangle'></i> Mail id does not belongs to any Admin User.";
                }

            }
            catch (Exception ex)
            {
                message = "<i class='fa fa-exclamation-triangle'></i> Something went wrong.";
            }
            return Json(message);
        }
        public bool Admin_Mail(string parameter, string username, string password1, string email, string name)
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
        [HttpGet]
        public ActionResult ChangePassword()
        {
            Menupermission();
            ViewBag.Breadcrumb = "Change password";
            return View();
        }
        [HttpPost]
        public ActionResult ChangePassword(string Old_Password, string New_Password, string Conf_Password)
        {
            try
            {
                string u_code = Session["USER_CODE"].ToString();
                string old_pass_word = DigiChampsModel.Encrypt_Password.HashPassword(Old_Password);
                var data = DbContext.tbl_DC_USER_SECURITY.Where(x => x.USER_CODE == u_code && x.PASSWORD == old_pass_word && x.STATUS == "A" && (x.ROLE_CODE == "A" || x.ROLE_CODE == "S")).FirstOrDefault();

                if (data != null)
                {
                    if (New_Password == Conf_Password)
                    {
                        string new_pass_word = DigiChampsModel.Encrypt_Password.HashPassword(New_Password).ToString();

                        data.PASSWORD = new_pass_word;
                        DbContext.Entry(data).State = EntityState.Modified;
                        DbContext.SaveChanges();
                        TempData["SuccessMessage"] = "Password is changed successfully.";
                    }
                    else
                    {
                        TempData["WarningMessage"] = "The new password & confirm password are not matching";
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "current password is wrong";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return View();
        }
        [HttpGet]
        public ActionResult EditProfile()
        {
            Menupermission();
            return View();
        }

        public ActionResult AdminDashboard()
        {
            Menupermission();
            ViewBag.sales = DbContext.tbl_DC_Order_Pkg.Where(x => x.Is_Active == true && x.Is_Deleted == false).ToList().Count();
            var orders = DbContext.tbl_DC_Order.Where(x => x.Is_Active == true && x.Is_Deleted == false && x.Is_Paid == true).ToList();
            ViewBag.orders = orders.Count();
            ViewBag.price = DbContext.tbl_DC_Order.Where(x => x.Is_Active == true && x.Is_Deleted == false && x.Is_Paid == true).Select(a => a.Total).ToList().Sum();
            ViewBag.totalstu = DbContext.View_All_Student_Details.OrderByDescending(x => x.Regd_ID).ToList().Count();
            ViewBag.total_user = DbContext.tbl_DC_USER_MASTER.Where(x => x.Is_Active == true && x.Is_Deleted == false).ToList().Count();
            ViewBag.total_teach = DbContext.tbl_DC_Teacher.Where(x => x.Is_Active == true && x.Is_Deleted == false).ToList().Count();

            ViewBag.pkg = DbContext.tbl_DC_Order_Pkg.Where(x => x.Is_Active == true && x.Is_Deleted == false).ToList().GroupBy(p => p.Package_ID).OrderByDescending(pi => pi.Sum(pii => pii.Package_ID)).Select(p => p.Key).Take(5);    //Top Selling

            ViewBag.neword = orders.Where(x => x.Inserted_Date > today.AddMonths(-1)).ToList().Count(); //New Orders-1 month
            ViewBag.cartitm = DbContext.tbl_DC_Cart.Where(x => x.Is_Paid == false && x.Is_Active == true && x.Is_Deleted == false).ToList().Count();
            ViewBag.exppkg = DbContext.tbl_DC_Order_Pkg.Where(x => x.Is_Active == true && x.Is_Deleted == false && x.Expiry_Date < today).GroupBy(p => p.Package_ID).ToList().Count();  //Expired packages

            int mon = Convert.ToDateTime(DateTime.Now).Month;
            if (mon == 1)
            {
                ViewBag.month = "January";
            }
            if (mon == 2)
            {
                ViewBag.month = "February";
            }
            if (mon == 3)
            {
                ViewBag.month = "March";
            }
            if (mon == 4)
            {
                ViewBag.month = "April";
            }
            if (mon == 5)
            {
                ViewBag.month = "May";
            }
            if (mon == 6)
            {
                ViewBag.month = "June";
            }
            if (mon == 7)
            {
                ViewBag.month = "July";
            }
            if (mon == 8)
            {
                ViewBag.month = "August";
            }
            if (mon == 9)
            {
                ViewBag.month = "September";
            }
            if (mon == 10)
            {
                ViewBag.month = "October";
            }
            if (mon == 11)
            {
                ViewBag.month = "November";
            }
            if (mon == 12)
            {
                ViewBag.month = "December";
            }
            var order1 = (DbContext.SP_Order_Datewise(mon)).ToList();
            if (order1.Count != 0)
            {
                var order_perday = (from a in order1.OrderBy(x => x.Inserted_Date)
                                    select new DigiChampCartModel
                                    {
                                        Order_ID = Convert.ToInt32(a.Order_ID),
                                        Inserted_Date = Convert.ToDateTime(a.Inserted_Date)
                                    }).ToList();
                string ord_date = string.Empty;
                string order = string.Empty;
                string finalorder = string.Empty;
                foreach (var item in order_perday)
                {
                    string[] ord = item.Inserted_Date.ToString().Split('/');
                    ord_date = ord[1];

                    order = item.Order_ID.ToString();
                    finalorder += "[" + ord_date + ',' + order + "]" + ',';

                }
                ViewBag.finaloredr = finalorder.Replace("\r\n", "").Substring(0, finalorder.Length - 1);
            }
            return View();
        }

        public ActionResult AddTeacher()
        {
            Menupermission();
            ViewBag.Breadcrumb = "Add teacher";
            return View();
        }

        #region---------------------percentage----------------------------
        [HttpGet]
        [ActionName("Percentage-Entry")]
        public ActionResult Percentage_entry()
        {
            ViewBag.setting = "setting";
            ViewBag.Breadcrumb = "Percentage";
            if (Convert.ToString(Session["ROLE_CODE"]) == "S")
            {
                if (Session["Role_Id"] != null)
                {
                    int id = Convert.ToInt32(Session["Role_Id"]);
                    var p_chk = DbContext.tbl_DC_Menu_Permission.Where(x => x.Menu_Key_ID == 1 && x.ROLE_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    if (p_chk == null)
                    {
                        return RedirectToAction("checkpermission");
                    }
                }

            }
            Menupermission();
            ViewBag.pagetitle = "Percentage-Entry";
            return View();
        }

        [HttpPost]
        public ActionResult Percentage_entry(int start, int end)
        {
            ViewBag.setting = "setting";
            ViewBag.Breadcrumb = "Percentage Entry";
            ViewBag.pagetitle = "Add Percentage";
            try
            {
                UInt32 defaultnum;
                if (UInt32.TryParse(Convert.ToString(start), out defaultnum) != false && UInt32.TryParse(Convert.ToString(end), out defaultnum) != false)
                {
                    if (Session["USER_CODE"] != null)
                    {
                        bool b;
                        int _id = Convert.ToInt32(Session["USER_CODE"].ToString().Trim());
                        var data = DbContext.tbl_DC_Percentage.Where(x => x.Is_Active == true && x.Is_Deleted == false).ToList();
                        foreach (var v in data)
                        {
                            int s = Convert.ToInt32(v.Start_P);
                            int e = Convert.ToInt32(v.END_P);
                            if (start > s && start < e)
                            {
                                TempData["WarningMessage"] = "Percentage range already exist.";
                                return RedirectToAction("Percentage-Entry");
                            }

                        }
                        var data2 = data.Where(x => x.Start_P == start && x.END_P == end).FirstOrDefault();
                        if (start != end && start < end)
                        {
                            if (data2 == null)
                            {

                                tbl_DC_Percentage _percen = new tbl_DC_Percentage();
                                _percen.Start_P = start;
                                _percen.END_P = end;
                                _percen.Inserted_by = _id;
                                _percen.Inserted_date = today;
                                _percen.Is_Active = true;
                                _percen.Is_Deleted = false;

                                DbContext.tbl_DC_Percentage.Add(_percen);
                                DbContext.SaveChanges();
                                TempData["SuccessMessage"] = "Percentage successfully inserted.";
                                return RedirectToAction("Showall");
                            }
                            else
                            {
                                TempData["WarningMessage"] = "Provided data alreday exist.";
                                return RedirectToAction("Percentage-Entry");
                            }

                        }
                        else
                        {
                            TempData["WarningMessage"] = "Start percentage should be less than end percentage.";
                            return RedirectToAction("Percentage-Entry");
                        }

                    }
                    else
                    {
                        return RedirectToAction("Logout", "Admin");
                    }
                }
                else
                {
                    TempData["WarningMessage"] = "Please enter data correctly.";
                    return RedirectToAction("Percentage-Entry");
                }


            }
            catch (Exception)
            {

                TempData["WarningMessage"] = "Something went wrong.";
                return RedirectToAction("Percentage-Entry");
            }
        }

        [ActionName("percentage")]
        public ActionResult Showall()
        {
            ViewBag.setting = "setting";
            Menupermission();

            if (Session["Role_Id"] != null)
            {
                int id = Convert.ToInt32(Session["Role_Id"]);
                var p_chk = DbContext.tbl_DC_Menu_Permission.Where(x => x.Menu_Key_ID == 1 && x.ROLE_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                if (p_chk == null)
                {
                    return RedirectToAction("checkpermission");
                }
            }
            ViewBag.Breadcrumb = "Percentage";
            ViewBag.all_percentage = DbContext.tbl_DC_Percentage.Where(x => x.Is_Active == true && x.Is_Deleted == false).OrderByDescending(x => x.ID).ToList();
            return View();
        }
        public ActionResult Delete_Percentage(int id)
        {
            ViewBag.setting = "setting";
            try
            {
                if (Session["USER_CODE"] != null)
                {
                    int _id = Convert.ToInt32(Session["USER_CODE"].ToString());

                    var _pd = DbContext.tbl_DC_Percentage.Where(x => x.ID == id).FirstOrDefault();
                    if (_pd != null)
                    {
                        _pd.Is_Active = false;
                        _pd.Modify_by = _id;
                        _pd.Modified_date = today;
                        _pd.Is_Deleted = true;
                        DbContext.SaveChanges();
                        TempData["SuccessMessage"] = "Successfully deleted.";
                        return RedirectToAction("Showall-Percentage");
                    }
                    else
                    {
                        TempData["WarningMessage"] = "Please try again.";
                    }


                }
                else
                {
                    return RedirectToAction("Logout", "Admin");
                }
            }
            catch (Exception ex)
            {

                TempData["WarningMessage"] = "Something went wrong.";
                return RedirectToAction("Logout", "Admin");
            }
            return View("Showall-Percentage");
        }
        #endregion

        #region ------------------- Get All from ID -----------------------
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
            //if (list != null)
            //{
            //    for (int i = 0; i < list.Count; i++)
            //    {
            //        int chapid = Convert.ToInt32(list.ToList()[i].Chapter_Id);
            //        string chapter = list.ToList()[i].Chapter;
            //        string subject = list.ToList()[i].Subject;
            //        int subjectid = Convert.ToInt32(list.ToList()[i].Subject_Id);
            //        obj.subdtls1.Add(new SP_chapterList_Result
            //        {

            //            Chapter_Id = chapid,
            //            Chapter = chapter,
            //            Subject = subject,
            //            Subject_Id = subjectid
            //        });
            //    }
            //}
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
        public ActionResult GetAllBatch(string Subid)
        {
            List<SelectListItem> BatchNames = new List<SelectListItem>();
            int sid = Convert.ToInt32(Subid);
            List<tbl_DC_Batch> batches = DbContext.tbl_DC_Batch.Where(x => x.Subject_Id == sid && x.Is_Active == true && x.Is_Deleted == false).ToList();
            batches.ForEach(x =>
            {
                BatchNames.Add(new SelectListItem { Text = x.Batch_Name + " - " + x.Batch_Code + " - " + x.Batch_Days + " - " + x.Centre_Id, Value = x.Batch_Id.ToString() });
            });
            return Json(BatchNames, JsonRequestBehavior.AllowGet);
        }

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
        public ActionResult GetAllStudent_subwise(int subid)
        {
            var student_nm = DbContext.VW_DC_Assign_Batch.Where(x => x.Subject_ID == subid).ToList();

            return Json(student_nm, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult GetAllTeacher_subwise(int subid)
        {
            List<SelectListItem> teacher_nm = new List<SelectListItem>();
            List<DigiChampsModel.DigiChampsTeacherRegModel> teacher = (from a in DbContext.tbl_DC_Teacher.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                                                       join b in DbContext.tbl_DC_Course_Teacher_Assign.Where(x => x.Subject_Id == subid && x.Is_Active == true && x.Is_Deleted == false)
                                                      on a.Teach_ID equals b.Teacher_ID
                                                                       select new DigiChampsModel.DigiChampsTeacherRegModel
                                                                       {
                                                                           Teach_ID = a.Teach_ID,
                                                                           Teacher_Name = a.Teacher_Name

                                                                       }).ToList();
            teacher.ForEach(x =>
            {
                teacher_nm.Add(new SelectListItem { Value = x.Teach_ID.ToString(), Text = x.Teacher_Name.ToString() });
            });

            //var teacher = DbContext.View_DC_Tickets_and_Teacher.Where(x => x.Subject_ID == subid).ToList();

            return Json(teacher, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult GetAllStudent_Batchwise(int Bid)
        {
            List<DigiChampsModel.DigiChampsAssignBatchModel> batch_stu = (from s in DbContext.tbl_DC_Student_Batch_Assign.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                                                          join a in DbContext.tbl_DC_Batch_Assign.Where(x => x.Batch_Id == Bid && x.Is_Active == true && x.Is_Deleted == false) on s.Batch_Assign_Id equals a.Batch_Assign_Id
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
        [HttpPost]
        public ActionResult GetAllStudent_batchidwise(int subid)
        {
            var get_subject = DbContext.tbl_DC_Batch_Assign.Where(x => x.Batch_Assign_Id == subid).FirstOrDefault();
            int sub_id = Convert.ToInt32(get_subject.Subject_Id);
            var student_nm = DbContext.VW_DC_Assign_Batch.Where(x => x.Subject_ID == sub_id).ToList();

            return Json(student_nm, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region---------------------State & city--------------------------
        [HttpGet]
        public ActionResult StateMaster()
        {
            ViewBag.general = "general";
            ViewBag.StateMaster = "active";
            Menupermission();
            if (Session["Role_Id"] != null)
            {
                int id = Convert.ToInt32(Session["Role_Id"]);
                var p_chk = DbContext.tbl_DC_Menu_Permission.Where(x => x.Menu_Key_ID == 20 && x.ROLE_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                if (p_chk == null)
                {
                    return RedirectToAction("checkpermission");
                }
            }
            ViewBag.state = new SelectList(DbContext.tbl_DC_State.OrderBy(x => x.State_Id), "State_Id", "State_Name");
            ViewBag.statelist = DbContext.tbl_DC_State.Where(x => x.Is_Active == true && x.Is_Deleted == false).OrderByDescending(x => x.State_Id).ToList();
            ViewBag.city = (from s in DbContext.tbl_DC_State.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                            join c in DbContext.tbl_DC_City.OrderByDescending(x => x.City_Id) on s.State_Id equals c.State_Id
                            select new DigiChampsModel.state_entry
                            {
                                id = c.City_Id,
                                c_name = c.City_Name,
                                statename = s.State_Name
                            }).ToList();

            //ViewBag.city = DbContext.tbl_DC_City.Where(x => x.Is_Active == true && x.Is_Deleted == false).OrderByDescending(x => x.State_Id).ToList();

            return View();
        }

        [HttpPost]
        public ActionResult StateMaster(string sname)
        {
            ViewBag.general = "general";
            ViewBag.StateMaster = "active";
            ViewBag.state = new SelectList(DbContext.tbl_DC_State.OrderBy(x => x.State_Id), "State_Id", "State_Name");
            DateTime now = Convert.ToDateTime(System.DateTime.Now.ToShortDateString());
            if (Session["USER_CODE"] != null)
            {
                var statename = DbContext.tbl_DC_State.Where(x => x.State_Name == sname && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                if (statename != null)
                {
                    TempData["WarningMessage"] = "State name already exist.";
                }
                else
                {
                    tbl_DC_State sm = new tbl_DC_State();
                    sm.State_Name = sname;
                    sm.Inserted_Date = now;
                    sm.Inserted_By = Convert.ToInt32(Session["USER_CODE"]);
                    sm.Is_Active = true;
                    sm.Is_Deleted = false;
                    DbContext.tbl_DC_State.Add(sm);
                    DbContext.SaveChanges();
                    TempData["SuccessMessage"] = "State added successfully.";
                    ModelState.Clear();
                }
            }
            return RedirectToAction("StateMaster");
        }
        [HttpPost]
        public ActionResult CityMaster(DigiChampsModel.state_entry ce, string cname)
        {
            ViewBag.general = "general";
            ViewBag.StateMaster = "active";
            ViewBag.state = new SelectList(DbContext.tbl_DC_State.OrderBy(x => x.State_Id), "State_Id", "State_Name");
            DateTime now = Convert.ToDateTime(System.DateTime.Now.ToShortDateString());

            if (Session["USER_CODE"] != null)
            {
                var cityname = DbContext.tbl_DC_City.Where(x => x.City_Name == cname && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();

                if (cityname != null)
                {
                    TempData["WarningMessage"] = "City name already exist.";
                }
                else
                {
                    tbl_DC_City cm = new tbl_DC_City();
                    cm.State_Id = ce.stateid;
                    cm.City_Name = cname;
                    cm.Inserted_Date = now;
                    cm.Inserted_By = Convert.ToInt32(Session["USER_CODE"]);
                    cm.Is_Active = true;
                    cm.Is_Deleted = false;
                    DbContext.tbl_DC_City.Add(cm);
                    DbContext.SaveChanges();
                    TempData["SuccessMessage"] = "City added successfully.";
                    ModelState.Clear();
                }
            }
            return RedirectToAction("StateMaster");
        }

        public ActionResult deletestate(int? id)
        {
            ViewBag.general = "general";
            ViewBag.StateMaster = "active";
            try
            {
                if (id != null)
                {
                    var data = DbContext.tbl_DC_State.Where(x => x.State_Id == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    if (data != null)
                    {
                        data.Is_Active = false;
                        data.Is_Deleted = true;
                        data.Modified_Date = today;
                        DbContext.SaveChanges();
                        TempData["SuccessMessage"] = "State deleted successfully.";
                        return RedirectToAction("StateMaster");
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Can't delete the State";
                    }

                }

                else
                {
                    TempData["ErrorMessage"] = "No data Found";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something Went Wrong";
            }
            return RedirectToAction("StateMaster");
        }

        public ActionResult deletecity(int? id)
        {
            ViewBag.general = "general";
            ViewBag.StateMaster = "active";
            try
            {
                if (id != null)
                {
                    var data = DbContext.tbl_DC_City.Where(x => x.City_Id == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    if (data != null)
                    {
                        data.Is_Active = false;
                        data.Is_Deleted = true;
                        data.Modified_Date = today;
                        DbContext.SaveChanges();
                        TempData["SuccessMessage"] = "City deleted successfully.";
                        return RedirectToAction("StateMaster");
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Can't delete the City";
                    }

                }

                else
                {
                    TempData["ErrorMessage"] = "No data Found";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something Went Wrong";
            }
            return RedirectToAction("StateMaster");
        }
        #endregion

        #region------------------------Board------------------------------
        [HttpGet]
        public ActionResult BoardMaster()
        {
            ViewBag.setting = "setting";
            ViewBag.Breadcrumb = "Board";
            if (Convert.ToString(Session["ROLE_CODE"]) == "S")
            {
                if (Session["Role_Id"] != null)
                {
                    int id = Convert.ToInt32(Session["Role_Id"]);
                    var p_chk = DbContext.tbl_DC_Menu_Permission.Where(x => x.Menu_Key_ID == 1 && x.ROLE_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    if (p_chk == null)
                    {
                        return RedirectToAction("checkpermission");
                    }
                }
            }
            var data = (from a in DbContext.tbl_DC_Board.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                        select new DigiChampsModel.DigiChampsBoardModel
                        {
                            Board_Id = a.Board_Id,
                            Board_Name = a.Board_Name
                        }).OrderByDescending(x => x.Board_Id).ToList();
            ViewBag.Board = data;

            Menupermission();
            return View();
        }
        [HttpGet]
        public ActionResult AddBoard()
        {
            ViewBag.setting = "setting";
            if (Convert.ToString(Session["ROLE_CODE"]) == "S")
            {
                if (Session["Role_Id"] != null)
                {
                    int id = Convert.ToInt32(Session["Role_Id"]);
                    var p_chk = DbContext.tbl_DC_Menu_Permission.Where(x => x.Menu_Key_ID == 1 && x.ROLE_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    if (p_chk == null)
                    {
                        return RedirectToAction("checkpermission");
                    }
                }
            }
            ViewBag.pagetitle = "Add";
            ViewBag.Breadcrumb = "Board";
            //rackSpace_Api.getProvider();
            return View();

        }
        [HttpPost]
        public ActionResult AddBoard(string Board_Name, string board_id)
        {
            ViewBag.setting = "setting";
            try
            {
                if (Board_Name.Trim() != "")
                {
                    if (board_id == "")
                    {
                        ViewBag.pagetitle = "Add";
                        var board = DbContext.tbl_DC_Board.Where(x => x.Board_Name == Board_Name && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        if (board != null)
                        {
                            TempData["WarningMessage"] = "Board name already exist.";
                        }
                        else
                        {
                            tbl_DC_Board obj = new tbl_DC_Board();
                            obj.Board_Name = Board_Name;
                            obj.Inserted_Date = today;
                            obj.Is_Active = true;
                            obj.Is_Deleted = false;
                            //obj.Inserted_By = Convert.ToInt32((Session["USER_CODE"]).Substring(1));
                            DbContext.tbl_DC_Board.Add(obj);
                            DbContext.SaveChanges();
                            TempData["SuccessMessage"] = "Board added successfully.";
                            return RedirectToAction("BoardMaster");
                        }
                    }
                    else
                    {
                        ViewBag.pagetitle = "Update";
                        var board = DbContext.tbl_DC_Board.Where(x => x.Board_Name == Board_Name && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        if (board != null)
                        {
                            TempData["WarningMessage"] = "Board name already exists.";
                        }
                        else
                        {
                            int b_id = Convert.ToInt32(board_id);
                            tbl_DC_Board obj = DbContext.tbl_DC_Board.Where(x => x.Board_Id == b_id).FirstOrDefault();
                            obj.Board_Name = Board_Name;
                            obj.Modified_Date = today;
                            DbContext.Entry(obj).State = EntityState.Modified;
                            DbContext.SaveChanges();
                            TempData["SuccessMessage"] = "Board updated successfully.";
                            return RedirectToAction("BoardMaster");
                        }
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "Please enter board name.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return View();
        }
        public ActionResult EditBoard(int? id)
        {
            ViewBag.setting = "setting";
            try
            {
                ViewBag.Breadcrumb = "Board";
                ViewBag.pagetitle = "Update";
                if (id != null)
                {
                    var data1 = (from a in DbContext.tbl_DC_Board.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                 select new DigiChampsModel.DigiChampsBoardModel
                                 {
                                     Board_Id = a.Board_Id,
                                     Board_Name = a.Board_Name
                                 }).ToList();
                    if (data1 != null)
                    {
                        ViewBag.Board = data1;
                    }
                    else
                    {
                        TempData["WarningMessage"] = "Invalid board details.";
                    }
                    var data = DbContext.tbl_DC_Board.Where(x => x.Board_Id == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    if (data != null)
                    {
                        DigiChampsModel.DigiChampsBoardModel obj = new DigiChampsModel.DigiChampsBoardModel();
                        obj.Board_Id = data.Board_Id;
                        obj.Board_Name = data.Board_Name;
                        ViewBag.Board_Id = obj.Board_Id;
                        ViewBag.Board_Name = data.Board_Name;
                        return View("AddBoard", obj);
                    }
                    else
                    {
                        TempData["WarningMessage"] = "Invalid board details.";
                    }
                }
                else
                {
                    TempData["WarningMessage"] = "Invalid board details.";
                    return RedirectToAction("BoardMaster");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return View();
        }
        public ActionResult DeleteBoard(int? id)
        {
            try
            {
                if (id != null)
                {
                    var board_found = DbContext.tbl_DC_Class.Where(x => x.Board_Id == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    if (board_found == null)
                    {
                        var obj = DbContext.tbl_DC_Board.Where(x => x.Board_Id == id).FirstOrDefault();
                        obj.Modified_Date = today;
                        obj.Is_Active = false;
                        obj.Is_Deleted = true;
                        DbContext.Entry(obj).State = EntityState.Modified;
                        DbContext.SaveChanges();
                        TempData["SuccessMessage"] = "Board deleted successfully.";
                        return RedirectToAction("BoardMaster");

                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Board can not be deleted because its in use.";
                    }

                }
                else
                {
                    TempData["ErrorMessage"] = "No data found";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return RedirectToAction("BoardMaster");
        }
        #endregion

        #region-------------------------Class-----------------------------
        [HttpGet]
        public ActionResult ClassMaster()
        {
            ViewBag.setting = "setting";
            try
            {
                ViewBag.Breadcrumb = "Class";
                if (Convert.ToString(Session["ROLE_CODE"]) == "S")
                {
                    if (Session["Role_Id"] != null)
                    {
                        int id = Convert.ToInt32(Session["Role_Id"]);
                        var p_chk = DbContext.tbl_DC_Menu_Permission.Where(x => x.Menu_Key_ID == 1 && x.ROLE_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        if (p_chk == null)
                        {
                            return RedirectToAction("checkpermission");
                        }
                    }
                }
                ViewBag.Board_Id = new SelectList(DbContext.tbl_DC_Board.Where(x => x.Is_Active == true && x.Is_Deleted == false), "Board_Id", "Board_Name");

                var data = (from a in DbContext.tbl_DC_Board.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                            join b in DbContext.tbl_DC_Class.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                            on a.Board_Id equals b.Board_Id
                            select new DigiChampsModel.DigiChampsClassModel
                            {
                                Class_Id = b.Class_Id,
                                Class_Name = b.Class_Name,
                                Board_Id = a.Board_Id,
                                Board_Name = a.Board_Name
                            }).OrderByDescending(x => x.Class_Id).ToList();
                ViewBag.classdata = data;

                Menupermission();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return View();
        }
        public ActionResult EditClass(int? id)
        {
            ViewBag.setting = "setting";
            try
            {
                ViewBag.pagetitle = "Update";
                if (id != null)
                {
                    var data1 = (from a in DbContext.tbl_DC_Class.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                 join b in DbContext.tbl_DC_Board
                                 on a.Board_Id equals b.Board_Id
                                 select new DigiChampsModel.DigiChampsClassModel
                                 {
                                     Board_Id = a.Board_Id,
                                     Board_Name = b.Board_Name,
                                     Class_Id = a.Class_Id,
                                     Class_Name = a.Class_Name
                                 }).ToList();
                    if (data1 != null)
                    {
                        ViewBag.Class = data1;
                    }
                    else
                    {
                        TempData["WarningMessage"] = "Invalid class details.";
                    }

                    var data = (from a in DbContext.tbl_DC_Class.Where(x => x.Class_Id == id && x.Is_Active == true && x.Is_Deleted == false)
                                join b in DbContext.tbl_DC_Board
                                on a.Board_Id equals b.Board_Id
                                select new DigiChampsModel.DigiChampsClassModel
                                {
                                    Board_Id = b.Board_Id,
                                    Board_Name = b.Board_Name,
                                    Class_Id = a.Class_Id,
                                    Class_Name = a.Class_Name
                                }).FirstOrDefault();
                    if (data != null)
                    {
                        DigiChampsModel.DigiChampsClassModel obj = new DigiChampsModel.DigiChampsClassModel();
                        obj.Board_Id = data.Board_Id;
                        obj.Class_Id = data.Class_Id;
                        obj.Class_Name = data.Class_Name;
                        obj.Board_Name = data.Board_Name;
                        ViewBag.boardid = obj.Board_Id;
                        ViewBag.Class_Id = obj.Class_Id;
                        ViewBag.brd_name = new SelectList(DbContext.tbl_DC_Board.Where(x => x.Is_Active == true && x.Is_Deleted == false), "Board_Id", "Board_Name", data.Board_Id);
                        ViewBag.Class_Name = data.Class_Name;
                        return View("AddClass", obj);
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Class does not exist.";
                    }
                }
                else
                {
                    TempData["WarningMessage"] = "Invalid class details.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return View("AddClass");
        }
        public ActionResult DeleteClass(int? id)
        {
            ViewBag.setting = "setting";
            try
            {
                if (id != null)
                {
                    ViewBag.Board_Id = new SelectList(DbContext.tbl_DC_Board.Where(x => x.Is_Active == true && x.Is_Deleted == false), "Board_Id", "Board_Name");
                    var class_found = DbContext.tbl_DC_Subject.Where(x => x.Class_Id == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    if (class_found == null)
                    {
                        var obj = DbContext.tbl_DC_Class.Where(x => x.Class_Id == id).FirstOrDefault();
                        obj.Modified_Date = today;
                        obj.Is_Active = false;
                        obj.Is_Deleted = true;
                        obj.Modified_By = Convert.ToInt32(Convert.ToString(Session["USER_CODE"]).Substring(1));
                        obj.Modified_Date = today;
                        DbContext.Entry(obj).State = EntityState.Modified;
                        DbContext.SaveChanges();
                        TempData["SuccessMessage"] = "Class deleted successfully.";
                        return RedirectToAction("ClassMaster");
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Class can not be deleted because it is in use";
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "No data found";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong";
            }
            return RedirectToAction("ClassMaster");
        }
        [HttpGet]
        public ActionResult AddClass(string classname)
        {
            ViewBag.setting = "setting";
            ViewBag.Breadcrumb = "Class";
            if (Convert.ToString(Session["ROLE_CODE"]) == "S")
            {
                if (Session["Role_Id"] != null)
                {
                    int id = Convert.ToInt32(Session["Role_Id"]);
                    var p_chk = DbContext.tbl_DC_Menu_Permission.Where(x => x.Menu_Key_ID == 1 && x.ROLE_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    if (p_chk == null)
                    {
                        return RedirectToAction("checkpermission");
                    }
                }
            }
            ViewBag.pagetitle = "Add";
            var data = (from a in DbContext.tbl_DC_Board.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                        select new DigiChampsModel.DigiChampsBoardModel
                        {
                            Board_Id = a.Board_Id,
                            Board_Name = a.Board_Name
                        }).FirstOrDefault();
            ViewBag.brdid = data;
            ViewBag.Board_Id = new SelectList(DbContext.tbl_DC_Board.Where(x => x.Is_Active == true && x.Is_Deleted == false), "Board_Id", "Board_Name");
            ViewBag.Class_Name = classname;
            return View();
        }
        [HttpPost]
        public ActionResult AddClass(DigiChampsModel.DigiChampsClassModel obj, string class_id, string Class_Name)
        {
            ViewBag.setting = "setting";
            try
            {
                ViewBag.Board_Id = new SelectList(DbContext.tbl_DC_Board.Where(x => x.Is_Active == true && x.Is_Deleted == false), "Board_Id", "Board_Name");

                if (class_id.Trim() == "")
                {
                    if (Class_Name.Trim() != "")
                    {
                        var cls = DbContext.tbl_DC_Class.Where(x => x.Board_Id == obj.Board_Id && x.Class_Name == obj.Class_Name && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        if (cls != null)
                        {
                            TempData["WarningMessage"] = "Class name already exists.";
                            ModelState.Clear();
                            Class_Name = " ";
                        }
                        else
                        {
                            if (obj.Board_Id != 0)
                            {
                                tbl_DC_Class obj1 = new tbl_DC_Class();
                                obj1.Board_Id = obj.Board_Id;
                                obj1.Class_Name = Class_Name;
                                obj1.Inserted_Date = today;
                                obj1.Is_Active = true;
                                obj1.Is_Deleted = false;
                                DbContext.tbl_DC_Class.Add(obj1);
                                DbContext.SaveChanges();
                                TempData["SuccessMessage"] = "Class added successfully.";
                                return RedirectToAction("ClassMaster");
                            }
                            else
                            {
                                TempData["WarningMessage"] = "Please select board.";
                                return RedirectToAction("AddClass", "Admin", new { classname = Class_Name });
                            }

                        }
                    }
                    else
                    {
                        TempData["WarningMessage"] = "Please enter class name.";

                    }
                }
                else
                {
                    if (Class_Name.Trim() != "")
                    {
                        var cls = DbContext.tbl_DC_Class.Where(x => x.Board_Id == obj.Board_Id && x.Class_Name == obj.Class_Name && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        if (cls != null)
                        {
                            TempData["WarningMessage"] = "Class name already exist.";

                            ModelState.Clear();
                            Class_Name = " ";
                        }
                        else
                        {
                            if (obj.Board_Id != 0)
                            {
                                int cls_id = Convert.ToInt32(class_id);
                                tbl_DC_Class obj1 = DbContext.tbl_DC_Class.Where(x => x.Class_Id == cls_id).FirstOrDefault();
                                obj1.Board_Id = obj.Board_Id;
                                obj1.Class_Name = Class_Name;
                                obj1.Modified_Date = today;
                                obj1.Is_Active = true;
                                obj1.Is_Deleted = false;
                                DbContext.Entry(obj1).State = EntityState.Modified;
                                DbContext.SaveChanges();
                                TempData["SuccessMessage"] = "Class updated successfully.";
                                return RedirectToAction("ClassMaster");
                            }
                            else
                            {
                                TempData["WarningMessage"] = "Please select board.";
                                return RedirectToAction("AddClass", "Admin", new { classname = Class_Name });
                            }
                        }
                    }
                    else
                    {
                        TempData["WarningMessage"] = "Please enter class name.";

                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return View();
        }
        #endregion

        #region--------------------------Subject--------------------------
        [HttpGet]
        public ActionResult SubjectMaster()
        {
            ViewBag.setting = "setting";
            try
            {
                ViewBag.Breadcrumb = "Subject";
                if (Convert.ToString(Session["ROLE_CODE"]) == "S")
                {
                    if (Session["Role_Id"] != null)
                    {
                        int id = Convert.ToInt32(Session["Role_Id"]);
                        var p_chk = DbContext.tbl_DC_Menu_Permission.Where(x => x.Menu_Key_ID == 1 && x.ROLE_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        if (p_chk == null)
                        {
                            return RedirectToAction("checkpermission");
                        }
                    }
                }
                ViewBag.Board_Id = new SelectList(DbContext.tbl_DC_Board.Where(x => x.Is_Active == true && x.Is_Deleted == false), "Board_Id", "Board_Name");
                ViewBag.Class_Id = new SelectList(DbContext.tbl_DC_Class.Where(x => x.Is_Active == true && x.Is_Deleted == false), "Class_Id", "Class_Name");

                var data = (from a in DbContext.tbl_DC_Board.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                            join b in DbContext.tbl_DC_Class.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                            on a.Board_Id equals b.Board_Id
                            join c in DbContext.tbl_DC_Subject.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                            on b.Class_Id equals c.Class_Id
                            select new DigiChampsModel.DigiChampsSubjectModel
                            {
                                Subject_Id = c.Subject_Id,
                                Subject = c.Subject,
                                Board_Id = a.Board_Id,
                                Board_Name = a.Board_Name,
                                Class_Id = b.Class_Id,
                                Class_Name = b.Class_Name
                            }).OrderByDescending(x => x.Subject_Id).ToList();
                ViewBag.subjectdata = data;

                Menupermission();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return View();
        }
        [HttpGet]
        public ActionResult AddSubject(string Board_Name, string Class_Name, string Subject)
        {
            ViewBag.setting = "setting";
            try
            {
                if (Convert.ToString(Session["ROLE_CODE"]) == "S")
                {
                    if (Session["Role_Id"] != null)
                    {
                        int id = Convert.ToInt32(Session["Role_Id"]);
                        var p_chk = DbContext.tbl_DC_Menu_Permission.Where(x => x.Menu_Key_ID == 1 && x.ROLE_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        if (p_chk == null)
                        {
                            return RedirectToAction("checkpermission");
                        }
                    }
                }
                ViewBag.Breadcrumb = "Subject";
                ViewBag.pagetitle = "Add";
                ViewBag.Board_Id = new SelectList(DbContext.tbl_DC_Board.Where(x => x.Is_Active == true && x.Is_Deleted == false), "Board_Id", "Board_Name");

                List<SelectListItem> BrdNamess = new List<SelectListItem>();
                DigiChampsModel.DigiChampsSubjectModel mods = new DigiChampsModel.DigiChampsSubjectModel();

                List<tbl_DC_Board> boardss = DbContext.tbl_DC_Board.ToList();
                boardss.ForEach(x =>
                {
                    BrdNamess.Add(new SelectListItem { Text = x.Board_Name, Value = x.Board_Id.ToString() });
                });
                mods.BoardNames = BrdNamess;
                TempData["Ssubject_name"] = Subject;
                ViewBag.classid = Class_Name;
                ViewBag.b_id = Board_Name;
                return View(mods);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return View();
        }
        [HttpPost]
        public ActionResult AddSubject(DigiChampsModel.DigiChampsSubjectModel obj, string subject_id)
        {
            ViewBag.setting = "setting";
            try
            {
                ViewBag.Board_Id = new SelectList(DbContext.tbl_DC_Board.Where(x => x.Is_Active == true && x.Is_Deleted == false), "Board_Id", "Board_Name");
                if (obj.Subject.Trim() != "")
                {
                    if (subject_id == "")
                    {
                        if (obj.Class_Id != 0)
                        {
                            if (obj.Board_Name != "")
                            {
                                ViewBag.Class_Id = new SelectList(DbContext.tbl_DC_Class.Where(x => x.Is_Active == true && x.Is_Deleted == false), "Class_Id", "Class_Name");

                                var sub = DbContext.tbl_DC_Subject.Where(x => x.Class_Id == obj.Class_Id && x.Subject == obj.Subject && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                                if (sub != null)
                                {
                                    TempData["WarningMessage"] = "Subject already exist for this class.";
                                    return RedirectToAction("AddSubject", "Admin", obj);
                                }
                                else
                                {
                                    tbl_DC_Subject obj1 = new tbl_DC_Subject();
                                    obj1.Class_Id = obj.Class_Id;
                                    obj1.Subject = obj.Subject;
                                    obj1.Inserted_Date = today;
                                    obj1.Is_Active = true;
                                    obj1.Is_Deleted = false;
                                    //obj1.Inserted_By = Convert.ToInt32(Convert.ToString(Session["USER_CODE"]).Substring(1));
                                    obj1.Inserted_Date = today;
                                    DbContext.tbl_DC_Subject.Add(obj1);
                                    DbContext.SaveChanges();
                                    TempData["SuccessMessage"] = "Subject added successfully.";
                                    return RedirectToAction("SubjectMaster");
                                }
                            }
                            else
                            {
                                TempData["WarningMessage"] = "Please select Board.";
                                return RedirectToAction("AddSubject", "Admin", obj);
                            }
                        }
                        else
                        {
                            TempData["WarningMessage"] = "Please select class.";
                            return RedirectToAction("AddSubject", "Admin", obj);
                        }
                    }
                    else
                    {
                        var sub = DbContext.tbl_DC_Subject.Where(x => x.Subject_Id == obj.Subject_Id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        if (sub != null)
                        {
                            int sub_id = Convert.ToInt32(subject_id);
                            sub.Subject = obj.Subject;
                            sub.Modified_Date = today;
                            DbContext.Entry(sub).State = EntityState.Modified;
                            DbContext.SaveChanges();
                            TempData["SuccessMessage"] = "Subject updated successfully.";
                            return RedirectToAction("SubjectMaster");
                        }
                        else
                        {
                            TempData["WarningMessage"] = "Subject does not exist.";
                            return RedirectToAction("AddSubject", obj);
                        }
                    }

                }
                else
                {
                    TempData["WarningMessage"] = "Please enter subject.";
                    return RedirectToAction("AddSubject", obj);
                }
            }

            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return View();
        }
        public ActionResult EditSubject(int? id)
        {
            ViewBag.setting = "setting";
            try
            {
                ViewBag.pagetitle = "Update";
                if (id != null)
                {
                    var data = (from a in DbContext.tbl_DC_Board.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                join b in DbContext.tbl_DC_Class.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                on a.Board_Id equals b.Board_Id
                                join c in DbContext.tbl_DC_Subject.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                on b.Class_Id equals c.Class_Id
                                select new DigiChampsModel.DigiChampsSubjectModel
                                {
                                    Subject_Id = c.Subject_Id,
                                    Subject = c.Subject,
                                    Board_Id = a.Board_Id,
                                    Board_Name = a.Board_Name,
                                    Class_Id = b.Class_Id,
                                    Class_Name = b.Class_Name
                                }).OrderByDescending(x => x.Subject_Id).FirstOrDefault();
                    ViewBag.subjectdata = data;

                    var data1 = (from a in DbContext.tbl_DC_Board.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                 join b in DbContext.tbl_DC_Class.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                 on a.Board_Id equals b.Board_Id
                                 join c in DbContext.tbl_DC_Subject.Where(x => x.Subject_Id == id && x.Is_Active == true && x.Is_Deleted == false)
                                 on b.Class_Id equals c.Class_Id
                                 select new DigiChampsModel.DigiChampsSubjectModel
                                 {
                                     Subject_Id = c.Subject_Id,
                                     Subject = c.Subject,
                                     Board_Id = a.Board_Id,
                                     Board_Name = a.Board_Name,
                                     Class_Id = b.Class_Id,
                                     Class_Name = b.Class_Name
                                 }).FirstOrDefault();
                    if (data1 != null)
                    {
                        DigiChampsModel.DigiChampsSubjectModel obj = new DigiChampsModel.DigiChampsSubjectModel();
                        obj.Board_Id = data1.Board_Id;
                        obj.Board_Name = data1.Board_Name;
                        obj.Class_Id = data1.Class_Id;
                        obj.Class_Name = data1.Class_Name;
                        obj.Subject_Id = data1.Subject_Id;
                        obj.Subject = data1.Subject;
                        ViewBag.subject_id = data1.Subject_Id;
                        ViewBag.classid = data1.Class_Id;
                        ViewBag.b_id = data1.Board_Id;
                        ViewBag.Board = DbContext.tbl_DC_Board.Where(x => x.Is_Active == true && x.Is_Deleted == false).ToList();
                        return View("AddSubject", obj);
                    }
                    else
                    {
                        TempData["WarningMessage"] = "Particular subject doesnot exist in current context.";
                    }
                }
                else
                {
                    TempData["WarningMessage"] = "Unsupported URL Link, Please try again.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return View("SubjectMaster");
        }
        public ActionResult DeleteSubject(int? id)
        {
            ViewBag.setting = "setting";
            try
            {
                if (id != null)
                {
                    var subject_found = DbContext.tbl_DC_Chapter.Where(x => x.Subject_Id == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    if (subject_found == null)
                    {
                        tbl_DC_Subject obj = DbContext.tbl_DC_Subject.Where(x => x.Subject_Id == id).FirstOrDefault();
                        obj.Modified_Date = today;
                        obj.Is_Active = false;
                        obj.Is_Deleted = true;
                        //obj.Modified_By = Convert.ToInt32(Convert.ToString(Session["USER_CODE"]).Substring(1));
                        obj.Modified_Date = today;
                        DbContext.Entry(obj).State = EntityState.Modified;
                        DbContext.SaveChanges();
                        TempData["SuccessMessage"] = "Subject deleted successfully.";
                        return RedirectToAction("SubjectMaster");
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Subject can not be deleted because it is in use.";
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "No data found";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong";
            }
            return RedirectToAction("SubjectMaster");
        }
        #endregion

        #region-------------------------Chapter---------------------------
        [HttpGet]
        public ActionResult ChapterMaster()
        {
            ViewBag.setting = "setting";
            try
            {
                ViewBag.Breadcrumb = "Chapter";
                if (Convert.ToString(Session["ROLE_CODE"]) == "S")
                {
                    if (Session["Role_Id"] != null)
                    {
                        int id = Convert.ToInt32(Session["Role_Id"]);
                        var p_chk = DbContext.tbl_DC_Menu_Permission.Where(x => x.Menu_Key_ID == 1 && x.ROLE_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        if (p_chk == null)
                        {
                            return RedirectToAction("checkpermission");
                        }
                    }
                }
                var data = (from a in DbContext.tbl_DC_Board.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                            join b in DbContext.tbl_DC_Class.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                            on a.Board_Id equals b.Board_Id
                            join c in DbContext.tbl_DC_Subject.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                            on b.Class_Id equals c.Class_Id
                            join d in DbContext.tbl_DC_Chapter.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                            on c.Subject_Id equals d.Subject_Id
                            select new DigiChampsModel.DigiChampsChapterModel
                            {
                                Subject_Id = c.Subject_Id,
                                Subject = c.Subject,
                                Board_Id = a.Board_Id,
                                Board_Name = a.Board_Name,
                                Class_Id = b.Class_Id,
                                Class_Name = b.Class_Name,
                                Chapter_Id = d.Chapter_Id,
                                Chapter = d.Chapter
                            }).OrderByDescending(x => x.Chapter_Id).ToList();

                ViewBag.chapterdata = data;

                Menupermission();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return View();
        }
        [HttpGet]
        public ActionResult AddChapter()
        {
            ViewBag.setting = "setting";
            try
            {
                if (Convert.ToString(Session["ROLE_CODE"]) == "S")
                {
                    if (Session["Role_Id"] != null)
                    {
                        int id = Convert.ToInt32(Session["Role_Id"]);
                        var p_chk = DbContext.tbl_DC_Menu_Permission.Where(x => x.Menu_Key_ID == 1 && x.ROLE_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        if (p_chk == null)
                        {
                            return RedirectToAction("checkpermission");
                        }
                    }
                }
                ViewBag.Breadcrumb = "Chapter";
                ViewBag.pagetitle = "Add";
                ViewBag.Board_Id = new SelectList(DbContext.tbl_DC_Board.Where(x => x.Is_Active == true && x.Is_Deleted == false), "Board_Id", "Board_Name");

                List<SelectListItem> BrdNamess = new List<SelectListItem>();
                DigiChampsModel.DigiChampsChapterModel mod = new DigiChampsModel.DigiChampsChapterModel();

                List<tbl_DC_Board> boards = DbContext.tbl_DC_Board.ToList();
                boards.ForEach(x =>
                {
                    BrdNamess.Add(new SelectListItem { Text = x.Board_Name, Value = x.Board_Id.ToString() });
                });
                mod.BoardNamess = BrdNamess;
                return View(mod);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return View();
        }
        [HttpPost]
        public ActionResult AddChapter(DigiChampsModel.DigiChampsChapterModel obj)
        {
            ViewBag.setting = "setting";
            try
            {
                ViewBag.Board_Id = new SelectList(DbContext.tbl_DC_Board.Where(x => x.Is_Active == true && x.Is_Deleted == false), "Board_Id", "Board_Name");
                if (obj.Chapter.Trim() != "")
                {
                    if (obj.Chapter_Id == 0)
                    {
                        if (ModelState.IsValid)
                        {
                            if (obj.Board_Id == 0 || obj.Class_Id == 0 || obj.Subject_Id == 0)
                            {
                                TempData["WarningMessage"] = "Please select each field properly.";
                            }
                            else
                            {
                                var chapter = DbContext.tbl_DC_Chapter.Where(x => x.Subject_Id == obj.Subject_Id && x.Chapter == obj.Chapter && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                                if (chapter != null)
                                {
                                    TempData["WarningMessage"] = "Chapter name already exist for this subject.";
                                }
                                else
                                {
                                    tbl_DC_Chapter obj1 = new tbl_DC_Chapter();
                                    obj1.Subject_Id = obj.Subject_Id;
                                    obj1.Chapter = obj.Chapter;
                                    obj1.Inserted_Date = today;
                                    obj1.Is_Active = true;
                                    obj1.Is_Deleted = false;
                                    DbContext.tbl_DC_Chapter.Add(obj1);
                                    DbContext.SaveChanges();
                                    TempData["SuccessMessage"] = "Chapter added successfully.";
                                    return RedirectToAction("ChapterMaster");
                                }
                            }

                        }
                    }
                    else
                    {
                        var chapter = DbContext.tbl_DC_Chapter.Where(x => x.Chapter_Id == obj.Chapter_Id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        if (chapter == null)
                        {
                            TempData["WarningMessage"] = "Invalid chapter name for the subject.";
                        }
                        else
                        {
                            int chp_id = Convert.ToInt32(obj.Chapter_Id);
                            tbl_DC_Chapter obj1 = DbContext.tbl_DC_Chapter.Where(x => x.Chapter_Id == chp_id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                            obj1.Chapter = obj.Chapter;
                            obj1.Modified_Date = today;
                            DbContext.Entry(obj1).State = EntityState.Modified;
                            DbContext.SaveChanges();
                            TempData["SuccessMessage"] = "Chapter updated successfully.";
                            return RedirectToAction("ChapterMaster");
                        }
                    }
                }
                else
                {
                    TempData["WarningMessage"] = "Please enter chapter name.";
                    return RedirectToAction("ChapterMaster", obj);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return View();
        }
        public ActionResult DeleteChapter(int? id)
        {
            ViewBag.setting = "setting";
            try
            {
                if (id != null)
                {
                    var chapter_found = DbContext.tbl_DC_Topic.Where(x => x.Chapter_Id == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    if (chapter_found == null)
                    {
                        tbl_DC_Chapter obj = DbContext.tbl_DC_Chapter.Where(x => x.Chapter_Id == id).FirstOrDefault();
                        obj.Modified_Date = today;
                        obj.Is_Active = false;
                        obj.Is_Deleted = true;
                        obj.Modified_Date = today;
                        //obj.Modified_By = 
                        DbContext.Entry(obj).State = EntityState.Modified;
                        DbContext.SaveChanges();
                        TempData["SuccessMessage"] = "Chapter deleted successfully.";
                        return RedirectToAction("ChapterMaster");
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Chapter can not be deleted because its in use.";
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "No data found";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong";
            }
            return RedirectToAction("ChapterMaster");
        }
        public ActionResult EditChapter(int? id)
        {
            ViewBag.setting = "setting";
            try
            {
                ViewBag.pagetitle = "Update";
                if (id != null)
                {
                    var data = (from a in DbContext.tbl_DC_Board.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                join b in DbContext.tbl_DC_Class.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                on a.Board_Id equals b.Board_Id
                                join c in DbContext.tbl_DC_Subject.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                on b.Class_Id equals c.Class_Id
                                join d in DbContext.tbl_DC_Chapter.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                on c.Subject_Id equals d.Subject_Id
                                select new DigiChampsModel.DigiChampsChapterModel
                                {
                                    Subject_Id = c.Subject_Id,
                                    Subject = c.Subject,
                                    Board_Id = a.Board_Id,
                                    Board_Name = a.Board_Name,
                                    Class_Id = b.Class_Id,
                                    Class_Name = b.Class_Name,
                                    Chapter_Id = d.Chapter_Id,
                                    Chapter = d.Chapter
                                }).Where(x => x.Chapter_Id == id).OrderByDescending(x => x.Chapter_Id).ToList();
                    if (data != null)
                    {
                        ViewBag.chapterdata = data;
                        var data1 = data.FirstOrDefault();

                        ViewBag.Board = new SelectList(DbContext.tbl_DC_Board.Where(x => x.Is_Active == true && x.Is_Deleted == false), "Board_Id", "Board_Name", Convert.ToString(data1.Board_Id));

                        DigiChampsModel.DigiChampsChapterModel obj = new DigiChampsModel.DigiChampsChapterModel();
                        obj.Chapter_Id = data1.Chapter_Id;
                        obj.Chapter = data1.Chapter;
                        obj.Subject = data1.Subject;
                        obj.Subject_Id = data1.Subject_Id;
                        obj.Class_Id = data1.Class_Id;
                        obj.Class_Name = data1.Class_Name;
                        obj.Board_Id = data1.Board_Id;
                        obj.Board_Name = data1.Board_Name;

                        ViewBag.Boardid = data1.Board_Id;
                        ViewBag.classid = data1.Class_Id;
                        ViewBag.subid = data1.Subject_Id;
                        return View("AddChapter", obj);
                    }
                    else
                    {
                        TempData["WarningMessage"] = "Invalid chapter details.";
                    }
                }
                else
                {
                    TempData["WarningMessage"] = "Invalid chapter details.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return View("AddChapter");
        }
        #endregion

        #region------------------------Power------------------------------
        [HttpGet]
        public ActionResult Power()
        {
            ViewBag.setting = "setting";
            try
            {
                ViewBag.pagetitle = "Add";
                ViewBag.Breadcrumb = "Power";
                if (Convert.ToString(Session["ROLE_CODE"]) == "S")
                {
                    if (Session["Role_Id"] != null)
                    {
                        int id = Convert.ToInt32(Session["Role_Id"]);
                        var p_chk = DbContext.tbl_DC_Menu_Permission.Where(x => x.Menu_Key_ID == 1 && x.ROLE_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        if (p_chk == null)
                        {
                            return RedirectToAction("checkpermission");
                        }
                    }
                }
                var data = (from a in DbContext.tbl_DC_Power_Question.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                            select new DigiChampsModel.DigiChampsPowerModel
                            {
                                Power_Id = a.Power_Id,
                                Power_Type = a.Power_Type
                            }).OrderByDescending(x => x.Power_Id).ToList();
                ViewBag.power = data;
                Menupermission();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return View();
        }
        [HttpGet]
        public ActionResult AddPower()
        {
            ViewBag.setting = "setting";
            ViewBag.pagetitle = "Add";
            if (Convert.ToString(Session["ROLE_CODE"]) == "S")
            {
                if (Session["Role_Id"] != null)
                {
                    int id = Convert.ToInt32(Session["Role_Id"]);
                    var p_chk = DbContext.tbl_DC_Menu_Permission.Where(x => x.Menu_Key_ID == 1 && x.ROLE_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    if (p_chk == null)
                    {
                        return RedirectToAction("checkpermission");
                    }
                }
            }
            ViewBag.Breadcrumb = "Power";

            return View();
        }
        [HttpPost]
        public ActionResult AddPower(string Power_Name, string power_id)
        {
            ViewBag.setting = "setting";
            try
            {
                if (Power_Name.Trim() != "")
                {
                    if (power_id == "")
                    {
                        var pow_type = DbContext.tbl_DC_Power_Question.Where(x => x.Power_Type == Power_Name && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        if (pow_type != null)
                        {
                            TempData["WarningMessage"] = "Power type already exists.";
                        }
                        else
                        {
                            tbl_DC_Power_Question obj = new tbl_DC_Power_Question();
                            obj.Power_Type = Power_Name;
                            obj.Inserted_Date = today;
                            obj.Is_Active = true;
                            obj.Is_Deleted = false;
                            DbContext.tbl_DC_Power_Question.Add(obj);
                            DbContext.SaveChanges();
                            TempData["SuccessMessage"] = "Power added successfully.";
                            return RedirectToAction("Power");
                        }
                    }
                    else
                    {
                        var pow_type = DbContext.tbl_DC_Power_Question.Where(x => x.Power_Type == Power_Name && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        if (pow_type != null)
                        {
                            TempData["WarningMessage"] = "Power type already exists.";
                        }
                        else
                        {
                            int pow_id = Convert.ToInt32(power_id);
                            tbl_DC_Power_Question obj = DbContext.tbl_DC_Power_Question.Where(x => x.Power_Id == pow_id).FirstOrDefault();
                            obj.Power_Type = Power_Name;
                            obj.Modified_Date = today;
                            DbContext.Entry(obj).State = EntityState.Modified;
                            DbContext.SaveChanges();
                            TempData["SuccessMessage"] = "Power updated successfully.";
                            return RedirectToAction("Power");
                        }
                    }
                }
                else
                {
                    TempData["WarningMessage"] = "Please enter power.";
                    return RedirectToAction("AddPower");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return View();
        }
        public ActionResult EditPower(int? id)
        {
            ViewBag.setting = "setting";
            try
            {
                ViewBag.pagetitle = "Update";
                if (id != null)
                {
                    var data = (from a in DbContext.tbl_DC_Power_Question.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                select new DigiChampsModel.DigiChampsPowerModel
                                {
                                    Power_Id = a.Power_Id,
                                    Power_Type = a.Power_Type
                                }).ToList();
                    if (data != null)
                    {
                        ViewBag.power = data;
                    }

                    var data1 = DbContext.tbl_DC_Power_Question.Where(x => x.Power_Id == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    if (data1 != null)
                    {
                        DigiChampsModel.DigiChampsPowerModel obj = new DigiChampsModel.DigiChampsPowerModel();
                        obj.Power_Id = data1.Power_Id;
                        obj.Power_Type = data1.Power_Type;
                        ViewBag.Power_Id = data1.Power_Id;
                        ViewBag.Power_Type = data1.Power_Type;
                        return View("AddPower", obj);
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Power does not exists.";
                    }
                }
                else
                {
                    TempData["WarningMessage"] = "Power does not exist.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return View("AddPower");
        }
        public ActionResult DeletePower(int? id)
        {
            ViewBag.setting = "setting";
            try
            {
                if (id != null)
                {
                    tbl_DC_Power_Question obj = DbContext.tbl_DC_Power_Question.Where(x => x.Power_Id == id).FirstOrDefault();
                    obj.Is_Active = false;
                    obj.Is_Deleted = true;
                    obj.Modified_Date = today;
                    obj.Modified_By = HttpContext.User.Identity.Name;
                    DbContext.Entry(obj).State = EntityState.Modified;
                    DbContext.SaveChanges();
                    TempData["SuccessMessage"] = "Power deleted successfully.";
                    return RedirectToAction("Power", "Admin");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return View();
        }
        #endregion

        #region------------------------Shift------------------------------
        [HttpGet]
        public ActionResult ShiftMaster()
        {
            ViewBag.setting = "setting";
            try
            {
                ViewBag.Breadcrumb = "Shift";
                if (Convert.ToString(Session["ROLE_CODE"]) == "S")
                {
                    if (Session["Role_Id"] != null)
                    {
                        int id = Convert.ToInt32(Session["Role_Id"]);
                        var p_chk = DbContext.tbl_DC_Menu_Permission.Where(x => x.Menu_Key_ID == 2 && x.ROLE_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        if (p_chk == null)
                        {
                            return RedirectToAction("checkpermission");
                        }
                    }
                }
                var data = (from a in DbContext.tbl_DC_Shift_Mst.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                            select new DigiChampsModel.DigiChampsShiftModel
                            {
                                ShiftMst_ID = a.ShiftMst_ID,
                                Shift_Name = a.Shift_Name,
                                Shift_From_Time = a.Shift_From_Time,
                                Shift_To_Time = a.Shift_To_Time
                            }).OrderByDescending(x => x.ShiftMst_ID).ToList();
                ViewBag.Shift_Timing = data;
                Menupermission();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return View();
        }
        [HttpGet]
        public ActionResult Admin_Shift(string Shift_Name, string From_Time, string To_Time)
        {
            ViewBag.setting = "setting";
            if (Convert.ToString(Session["ROLE_CODE"]) == "S")
            {
                if (Session["Role_Id"] != null)
                {
                    int id = Convert.ToInt32(Session["Role_Id"]);
                    var p_chk = DbContext.tbl_DC_Menu_Permission.Where(x => x.Menu_Key_ID == 2 && x.ROLE_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    if (p_chk == null)
                    {
                        return RedirectToAction("checkpermission");
                    }
                }
            }
            ViewBag.Shift_Name = Shift_Name;
            ViewBag.Shift_To_Time = To_Time;
            ViewBag.Shift_From_Time = From_Time;
            ViewBag.Breadcrumb = "Shift";
            return View();
        }
        [HttpPost]
        public ActionResult Admin_Shift(string Shift_Name, string From_Time, string To_Time, string shift_id)
        {
            ViewBag.setting = "setting";
            try
            {
                if (Shift_Name == "" || From_Time == "" || To_Time == "")
                {
                    TempData["WarningMessage"] = "Please enter shift details properly.";
                    return RedirectToAction("Admin_Shift", "Admin", new { Shift_Name = Shift_Name, From_Time = From_Time, To_Time = To_Time });
                }
                else
                {
                    if (shift_id == "")
                    {
                        if (From_Time != To_Time)
                        {
                            var shift = DbContext.tbl_DC_Shift_Mst.Where(x => x.Shift_Name == Shift_Name.Trim() && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                            var shift_time = DbContext.tbl_DC_Shift_Mst.Where(x => x.Shift_From_Time == From_Time && x.Shift_To_Time == To_Time && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                            if (shift == null && shift_time == null)
                            {
                                tbl_DC_Shift_Mst obj = new tbl_DC_Shift_Mst();
                                obj.Shift_Name = Shift_Name;
                                obj.Shift_From_Time = From_Time;
                                obj.Shift_To_Time = To_Time;
                                obj.Inserted_Date = today;
                                obj.Is_Active = true;
                                obj.Is_Deleted = false;
                                DbContext.tbl_DC_Shift_Mst.Add(obj);
                                DbContext.SaveChanges();
                                TempData["SuccessMessage"] = "Shift details added successfully.";
                                return RedirectToAction("ShiftMaster");
                            }
                            else
                            {
                                TempData["ErrorMessage"] = "Shift details already exists.";
                                return RedirectToAction("Admin_Shift", "Admin", new { Shift_Name = Shift_Name, From_Time = From_Time, To_Time = To_Time });
                            }
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "Shift time can not be same.";
                            return RedirectToAction("Admin_Shift", "Admin", new { Shift_Name = Shift_Name, From_Time = From_Time, To_Time = To_Time });
                        }
                    }
                    else
                    {
                        int shft_id = Convert.ToInt32(shift_id);
                        tbl_DC_Shift_Mst obj = DbContext.tbl_DC_Shift_Mst.Where(x => x.ShiftMst_ID == shft_id).FirstOrDefault();
                        if (obj != null)
                        {
                            obj.Shift_Name = Shift_Name;
                            obj.Shift_From_Time = From_Time;
                            obj.Shift_To_Time = To_Time;
                            obj.Modified_Date = today;
                            obj.Is_Active = true;
                            obj.Is_Deleted = false;
                            DbContext.Entry(obj).State = EntityState.Modified;
                            DbContext.SaveChanges();
                            TempData["SuccessMessage"] = "Shift details updated successfully.";
                            return RedirectToAction("ShiftMaster");
                        }
                        else
                        {
                            TempData["WarningMessage"] = "Invalid shift details.";
                            return RedirectToAction("Admin_Shift", "Admin", new { Shift_Name = Shift_Name, From_Time = From_Time, To_Time = To_Time });
                        }
                        return RedirectToAction("ShiftMaster");
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return View();
        }
        public ActionResult EditShift(int? id)
        {
            ViewBag.setting = "setting";
            try
            {
                if (id != null)
                {
                    var data = (from a in DbContext.tbl_DC_Shift_Mst.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                select new DigiChampsModel.DigiChampsShiftModel
                                {
                                    ShiftMst_ID = a.ShiftMst_ID,
                                    Shift_Name = a.Shift_Name,
                                    Shift_From_Time = a.Shift_From_Time,
                                    Shift_To_Time = a.Shift_To_Time
                                }).ToList();
                    ViewBag.Shift_Timing = data;

                    var data1 = (from a in DbContext.tbl_DC_Shift_Mst.Where(x => x.ShiftMst_ID == id)
                                 select new DigiChampsModel.DigiChampsShiftModel
                                 {
                                     ShiftMst_ID = a.ShiftMst_ID,
                                     Shift_Name = a.Shift_Name,
                                     Shift_From_Time = a.Shift_From_Time,
                                     Shift_To_Time = a.Shift_To_Time
                                 }).FirstOrDefault();
                    DigiChampsModel.DigiChampsShiftModel obj = new DigiChampsModel.DigiChampsShiftModel();
                    obj.ShiftMst_ID = data1.ShiftMst_ID;
                    obj.Shift_Name = data1.Shift_Name;
                    obj.Shift_From_Time = data1.Shift_From_Time;
                    obj.Shift_To_Time = data1.Shift_To_Time;
                    ViewBag.shift_id = obj.ShiftMst_ID;
                    ViewBag.Shift_Name = obj.Shift_Name;
                    ViewBag.Shift_From_Time = obj.Shift_From_Time;
                    ViewBag.Shift_To_Time = obj.Shift_To_Time;
                    return View("Admin_Shift", obj);
                }
                else
                {
                    TempData["WarningMessage"] = "Invalid shift details.";
                    return RedirectToAction("ShiftMaster");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return View();
        }
        public ActionResult DeleteShift(int? id)
        {
            ViewBag.setting = "setting";
            try
            {
                if (id != null)
                {
                    tbl_DC_Shift_Mst obj = DbContext.tbl_DC_Shift_Mst.Where(x => x.ShiftMst_ID == id).FirstOrDefault();
                    obj.Is_Active = false;
                    obj.Is_Deleted = true;
                    obj.Modified_Date = today;
                    obj.Modified_By = HttpContext.User.Identity.Name;
                    DbContext.Entry(obj).State = EntityState.Modified;
                    DbContext.SaveChanges();
                    TempData["SuccessMessage"] = "Shift deleted successfully.";
                    return RedirectToAction("ShiftMaster");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return View();
        }
        #endregion

        #region----------------------------------------Package----------------------------------------------
        [HttpGet]
        public ActionResult PackageCreate()
        {
            Menupermission();
            if (Convert.ToString(Session["ROLE_CODE"]) == "S")
            {
                if (Session["Role_Id"] != null)
                {
                    int id = Convert.ToInt32(Session["Role_Id"]);
                    var p_chk = DbContext.tbl_DC_Menu_Permission.Where(x => x.Menu_Key_ID == 4 && x.ROLE_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    if (p_chk == null)
                    {
                        return RedirectToAction("checkpermission");
                    }
                }
            }
            ViewBag.Breadcrumb = "Package";
            ViewBag.package = DbContext.tbl_DC_Package.Where(x => x.Is_Active == true && x.Is_Deleted == false).OrderByDescending(x => x.Package_ID).ToList();
            return View();
        }
        [HttpGet]
        public ActionResult AddNewPackage(int? id, int? pkid)
        {
            Session["editpckgdtls"] = null;
            if (Convert.ToString(Session["ROLE_CODE"]) == "S")
            {
                if (Session["Role_Id"] != null)
                {
                    int id1 = Convert.ToInt32(Session["Role_Id"]);
                    var p_chk = DbContext.tbl_DC_Menu_Permission.Where(x => x.Menu_Key_ID == 4 && x.ROLE_ID == id1 && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    if (p_chk == null)
                    {
                        return RedirectToAction("checkpermission");
                    }
                }
            }
            ViewBag.Breadcrumb = "Package";
            ViewBag.Board_Id = new SelectList(DbContext.tbl_DC_Board.Where(x => x.Is_Active == true && x.Is_Deleted == false),
                "Board_Id", "Board_Name");

            ViewBag.TabletIds = new SelectList(DbContext.tbl_DC_Tablet_Purchase.Where(x => x.Is_Active == true && x.Is_Deleted == false),
              "Tablet_Id", "Tablet_Name");


            if (id != null)
            {
                var data = DbContext.tbl_DC_Package.Where(x => x.Package_ID == id).Where(x => x.Is_Active == true && x.Is_Deleted == false).ToList().FirstOrDefault();
                ViewBag.Boarddetails = DbContext.tbl_DC_Board.Where(x => x.Is_Active == true && x.Is_Deleted == false).ToList();
                if (data != null)
                {
                    ViewBag.hbid = data.Package_ID;
                    ViewBag.package_name = data.Package_Name;
                    ViewBag.Package_Desc = data.Package_Desc;
                    ViewBag.sub_period = data.Subscripttion_Period;
                    decimal price = Decimal.Round(Convert.ToDecimal(data.Price), 2);
                    ViewBag.price = price;
                    ViewBag.profimage = data.Thumbnail;
                    ViewBag.sub_limit = data.Total_Chapter;
                    ViewBag.isoffline = data.Is_Offline;
                    ViewBag.Tabletid = data.Tablet_Id;
                    var obj = DbContext.tbl_DC_Package_Dtl.Where(x => x.Package_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    if (obj != null)
                    {
                        ViewBag.Boardid = obj.Board_Id;
                        ViewBag.classid = obj.Class_Id;
                        ViewBag.subid = obj.Subject_Id;
                        ViewBag.chapterid = obj.Chapter_Id;
                        //var mod_id = DbContext.DC_Package_Module_ID(id).ToList();
                        //TempData["Moduleid"] = mod_id[0];
                        ViewBag.pkdtlid = obj.PackageDtl_ID;
                    }

                    var obj2 = DbContext.tbl_DC_Package_Period.Where(x => x.Package_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    if (obj2 != null)
                    {
                        ViewBag.Included_Price = obj2.Included_Price;
                        ViewBag.Excluded_Price = obj2.Excluded_Price;
                        ViewBag.Inserted_Date = obj2.Inserted_Date;
                        ViewBag.Package_From = obj2.Package_From;
                        ViewBag.Packagetype = obj2.Package_Type;
                        //var mod_id = DbContext.DC_Package_Module_ID(id).ToList();
                        //TempData["Moduleid"] = mod_id[0];
                        ViewBag.Package_To = obj2.Package_To;

                    }

                    var obj3 = (from v in DbContext.tbl_DC_Package
                                join d in DbContext.tbl_DC_PackageSub_Dtl.Where(x => x.Is_Active == true && x.Is_Deleted == false) on v.Package_ID equals d.Package_ID
                                join a in DbContext.tbl_DC_Package_Dtl.Where(x => x.Package_ID == id && x.Is_Active == true && x.Is_Deleted == false) on new { d.Package_ID, d.Board_Id, d.Class_Id, d.Subject_Id } equals new { a.Package_ID, a.Board_Id, a.Class_Id, a.Subject_Id }
                                join b in DbContext.tbl_DC_Chapter on a.Chapter_Id equals b.Chapter_Id
                                join c in DbContext.tbl_DC_Subject on b.Subject_Id equals c.Subject_Id
                                select new DigiChampsModel.DigiChampsModuleModel
                                {
                                    Subject = c.Subject,
                                    Chapter = b.Chapter,
                                    Chapter_Id = a.Chapter_Id,
                                    Subject_Id = a.Subject_Id,
                                    Chapterlimit = d.SubScription_Limit
                                }

                    ).ToList();
                    Session["editpckgdtls"] = obj3;
                    ViewBag.tabledtls = obj3;
                    var obj1 = DbContext.tbl_DC_PackageSub_Dtl.Where(x => x.Package_ID == id && x.Is_Active == true && x.Is_Deleted == false).ToList();
                }
                else
                {
                    return RedirectToAction("PackageCreate");
                }
                return View();
            }
            else
            {
                return View();
            }
        }

        [HttpPost]
        public ActionResult AddNewPackage(string chk_pac, string Package_Name, string Package_Desc, string Price, string Package_Limit, string Subscripttion_Period, string Package_Total, string Board_Id, string Class_Id, string[] module, string hpid, string pkid, string imagebase64, DigiChampsModel.DigiChampsModuleModel obj, string[] h_sub, string[] h_chap, int[] h_limit, string isoffline, string inprice, string exprice, string packagetype, string from_dt, string to_dt, string Tablet_Id)
        {
            string module_data = string.Empty;
            string mods_id = string.Empty;
            try
            {
                ViewBag.Breadcrumb = "Package";
                if (module != null)
                {
                    for (int i = 0; i < module.Length; i++)
                    {

                        module_data += "," + module[i];
                    }

                    mods_id = module_data.Substring(1);
                }

                decimal pkg_price;
                int ls_umber;
                if (Package_Name == "" || Price == "" || Subscripttion_Period == "" || Board_Id == "" || Class_Id == "" || int.TryParse(Subscripttion_Period, out ls_umber) != true || decimal.TryParse(Price, out pkg_price) != true || Package_Desc == "")
                {
                    TempData["ErrorMessage"] = "Enter Data Properly.";

                    return RedirectToAction("AddNewPackage", "Admin", new { chk_pac = chk_pac, Package_Name = Package_Name, Package_Desc = Package_Desc, Price = Price, Package_Limit = Package_Limit, Subscripttion_Period = Subscripttion_Period, Package_Total = Package_Total, Board_Id = Board_Id, Class_Id = Class_Id, mods_id = mods_id });
                }
                else
                {
                    if (imagebase64 != null && imagebase64 != "")
                    {
                        string[] image1 = imagebase64.Split(',');
                        string pkgname = Convert.ToString(Package_Name + CreateRandomPassword(6));
                        Base64ToImage_pkg(pkgname, image1[1]);
                        imagebase64 = pkgname + ".jpg";
                    }

                    int Ch_id = Convert.ToInt32(chk_pac);
                    int Br_Id = Convert.ToInt32(Board_Id);
                    int Cs_Id = Convert.ToInt32(Class_Id);
                    int mod = 0;
                    int sub = 0;
                    int count = 0; ;
                    if (h_chap != null)
                    {
                        count = h_chap.Length;
                    }

                    if (hpid == "")
                    {
                        #region insert_package
                        if (Convert.ToInt32(Package_Limit) > count)
                        {
                            TempData["ErrorMessage"] = "Package Limit Equal or Less Than Chapter.";
                            return RedirectToAction("AddNewPackage", "Admin", new { chk_pac = chk_pac, Package_Name = Package_Name, Package_Desc = Package_Desc, Price = Price, Package_Limit = Package_Limit, Subscripttion_Period = Subscripttion_Period, Package_Total = Package_Total, Board_Id = Board_Id, Class_Id = Class_Id, mods_id = mods_id, Tablet_Id = Tablet_Id });
                        }
                        else
                        {
                            try
                            {
                                var modnm = DbContext.tbl_DC_Package.Where(x => x.Package_Name == Package_Name && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                                if (modnm == null)
                                {
                                    tbl_DC_Package Pac_tbl = new tbl_DC_Package();
                                    Pac_tbl.Package_Name = Package_Name;
                                    if (imagebase64.Length > 0)
                                    {
                                        Pac_tbl.Thumbnail = imagebase64.ToString();
                                    }
                                    Pac_tbl.Package_Desc = Package_Desc;
                                    Pac_tbl.Price = Convert.ToDecimal(Price);
                                    // Pac_tbl.Package_Limit = Convert.ToInt32(Package_Limit);
                                    Pac_tbl.Subscripttion_Period = Convert.ToInt32(Subscripttion_Period);
                                    Pac_tbl.Inserted_By = HttpContext.User.Identity.Name;
                                    // Pac_tbl.Package_Total = count;
                                    Pac_tbl.Is_Active = true;
                                    Pac_tbl.Is_Deleted = false;
                                    if (Tablet_Id != "")
                                    {
                                        Pac_tbl.Tablet_Id = Convert.ToInt32(Tablet_Id);
                                    }
                                    if (isoffline == "on")
                                    {
                                        Pac_tbl.Is_Offline = true;
                                    }
                                    else
                                    {
                                        Pac_tbl.Is_Offline = false;
                                    }
                                    DbContext.tbl_DC_Package.Add(Pac_tbl);
                                    DbContext.SaveChanges();

                                    string msgs = Pac_tbl.Package_ID.ToString();
                                    tbl_DC_Package_Dtl _pc_dtbl = new tbl_DC_Package_Dtl();

                                    for (int i = 0; i < h_chap.Length; i++)
                                    {
                                        mod = Convert.ToInt32(h_chap[i]);

                                        var _pchid = DbContext.tbl_DC_Chapter.Where(x => x.Chapter_Id == mod && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();

                                        _pc_dtbl.Package_ID = Convert.ToInt32(msgs);
                                        _pc_dtbl.Board_Id = Br_Id;
                                        _pc_dtbl.Class_Id = Cs_Id;
                                        _pc_dtbl.Subject_Id = _pchid.Subject_Id;
                                        _pc_dtbl.Chapter_Id = _pchid.Chapter_Id;
                                        // _pc_dtbl.Module_ID = mod;
                                        _pc_dtbl.Inserted_By = HttpContext.User.Identity.Name;
                                        _pc_dtbl.Is_Active = true;
                                        _pc_dtbl.Is_Deleted = false;
                                        DbContext.tbl_DC_Package_Dtl.Add(_pc_dtbl);
                                        DbContext.SaveChanges();
                                    }
                                    tbl_DC_PackageSub_Dtl pc_subchtbl = new tbl_DC_PackageSub_Dtl();

                                    var subdistinct = h_sub.Distinct().ToArray();
                                    var limit = h_limit.Distinct().ToArray();
                                    int total_chapter = 0;
                                    for (int j = 0; j < subdistinct.Length; j++)
                                    {
                                        sub = Convert.ToInt32(subdistinct[j]);
                                        pc_subchtbl.Package_ID = Convert.ToInt32(msgs);
                                        var _pchid = DbContext.tbl_DC_Subject.Where(x => x.Subject_Id == sub && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                                        pc_subchtbl.Board_Id = Br_Id;
                                        pc_subchtbl.Class_Id = Cs_Id;
                                        pc_subchtbl.Subject_Id = _pchid.Subject_Id;
                                        if (limit.Count() == 1)
                                        {
                                            pc_subchtbl.SubScription_Limit = limit[0];

                                        }
                                        else
                                        {

                                            pc_subchtbl.SubScription_Limit = limit[j];
                                        }
                                        // _pc_dtbl.Module_ID = mod;
                                        pc_subchtbl.Inserted_By = HttpContext.User.Identity.Name;
                                        pc_subchtbl.Is_Active = true;
                                        pc_subchtbl.Is_Deleted = false;
                                        if (limit.Count() == 1)
                                        {
                                            total_chapter += limit[0];

                                        }
                                        else
                                        {
                                            total_chapter += limit[j];

                                        }
                                        DbContext.tbl_DC_PackageSub_Dtl.Add(pc_subchtbl);
                                        DbContext.SaveChanges();
                                    }
                                    if (isoffline != null)
                                    {
                                        tbl_DC_Package_Period tbDP = new tbl_DC_Package_Period();
                                        tbDP.Package_ID = Convert.ToInt32(msgs);
                                        tbDP.Excluded_Price = Convert.ToDecimal(exprice);
                                        tbDP.Included_Price = Convert.ToDecimal(inprice);
                                        tbDP.Package_From = Convert.ToDateTime(from_dt);
                                        tbDP.Package_To = Convert.ToDateTime(to_dt);
                                        tbDP.Package_Type = packagetype;
                                        tbDP.Is_Active = true;
                                        tbDP.Is_Deleted = false;
                                        tbDP.Inserted_Date = System.DateTime.Now;
                                        DbContext.tbl_DC_Package_Period.Add(tbDP);
                                        DbContext.SaveChanges();

                                    }
                                    Pac_tbl.Total_Chapter = total_chapter;
                                    DbContext.SaveChanges();

                                    TempData["SuccessMessage"] = "Package Created Succesfully.";
                                    return Json("success", JsonRequestBehavior.AllowGet);
                                }
                                else
                                {
                                    TempData["WarningMessage"] = "Package name already exist.";
                                    return Json("exist", JsonRequestBehavior.AllowGet);
                                }
                            }
                            catch (Exception rx)
                            {
                                TempData["ErrorMessage"] = "Something Went Wrong.";
                                return RedirectToAction("AddNewPackage", "Admin", new { chk_pac = chk_pac, Package_Name = Package_Name, Package_Desc = Package_Desc, Price = Price, Package_Limit = Package_Limit, Subscripttion_Period = Subscripttion_Period, Package_Total = Package_Total, Board_Id = Board_Id, Class_Id = Class_Id, mods_id = mods_id, Tablet_Id = Tablet_Id });
                            }
                        }
                        #endregion
                    }
                    else
                    {
                        #region update_package
                        try
                        {
                            int pkg_id = Convert.ToInt32(hpid);
                            tbl_DC_Package Pac_tbl = DbContext.tbl_DC_Package.Where(x => x.Package_ID == pkg_id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                            if (Pac_tbl != null)
                            {
                                Pac_tbl.Package_Name = Package_Name;
                                Pac_tbl.Package_Desc = Package_Desc;
                                Pac_tbl.Price = Convert.ToDecimal(Price);
                                if (imagebase64.Length > 0)
                                {
                                    Pac_tbl.Thumbnail = imagebase64.ToString();
                                }
                                Pac_tbl.Subscripttion_Period = Convert.ToInt32(Subscripttion_Period);
                                Pac_tbl.Total_Chapter = count;
                                Pac_tbl.Modified_By = HttpContext.User.Identity.Name;
                                Pac_tbl.Modified_Date = today;
                                if (Tablet_Id != "")
                                {
                                    Pac_tbl.Tablet_Id = Convert.ToInt32(Tablet_Id);
                                }
                                DbContext.SaveChanges();

                                if (h_chap != null)
                                {
                                    for (int i = 0; i < h_chap.Length; i++)
                                    {
                                        int msgs = Convert.ToInt32(Pac_tbl.Package_ID);
                                        mod = Convert.ToInt32(h_chap[i]);
                                        tbl_DC_Package_Dtl _pc_dtbl = DbContext.tbl_DC_Package_Dtl.Where(x => x.Chapter_Id == mod && x.Package_ID == msgs && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();

                                        if (_pc_dtbl == null)
                                        {
                                            tbl_DC_Package_Dtl _pc_dtbl1 = DbContext.tbl_DC_Package_Dtl.Where(x => x.Package_ID == msgs).FirstOrDefault();
                                            var _pchid = DbContext.tbl_DC_Chapter.Where(x => x.Chapter_Id == mod && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();

                                            _pc_dtbl1.Package_ID = msgs;
                                            _pc_dtbl1.Board_Id = Br_Id;
                                            _pc_dtbl1.Class_Id = Cs_Id;
                                            _pc_dtbl1.Subject_Id = _pchid.Subject_Id;
                                            _pc_dtbl1.Chapter_Id = mod;
                                            // _pc_dtbl.Module_ID = mod;
                                            _pc_dtbl1.Inserted_By = HttpContext.User.Identity.Name;
                                            _pc_dtbl1.Is_Active = true;
                                            _pc_dtbl1.Is_Deleted = false;
                                            DbContext.tbl_DC_Package_Dtl.Add(_pc_dtbl1);
                                            DbContext.SaveChanges();
                                        }
                                    }
                                }
                                if (h_sub != null)
                                {
                                    var subdistinct = h_sub.Distinct().ToArray();
                                    for (int j = 0; j < subdistinct.Length; j++)
                                    {
                                        int msgs = Convert.ToInt32(Pac_tbl.Package_ID);
                                        sub = Convert.ToInt32(subdistinct[j]);
                                        tbl_DC_PackageSub_Dtl _pc_dtbl = DbContext.tbl_DC_PackageSub_Dtl.Where(x => x.Subject_Id == sub && x.Package_ID == msgs).FirstOrDefault();
                                        if (_pc_dtbl == null)
                                        {
                                            tbl_DC_PackageSub_Dtl pc_subchtbl = DbContext.tbl_DC_PackageSub_Dtl.Where(x => x.Package_ID == msgs).FirstOrDefault();
                                            pc_subchtbl.Package_ID = Convert.ToInt32(msgs);
                                            var _pchid = DbContext.tbl_DC_Subject.Where(x => x.Subject_Id == sub && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                                            pc_subchtbl.Board_Id = Br_Id;
                                            pc_subchtbl.Class_Id = Cs_Id;

                                            pc_subchtbl.Subject_Id = _pchid.Subject_Id;

                                            pc_subchtbl.SubScription_Limit = h_limit[j];
                                            // _pc_dtbl.Module_ID = mod;
                                            pc_subchtbl.Inserted_By = HttpContext.User.Identity.Name;
                                            pc_subchtbl.Is_Active = true;
                                            pc_subchtbl.Is_Deleted = false;
                                            DbContext.tbl_DC_PackageSub_Dtl.Add(pc_subchtbl);

                                            DbContext.SaveChanges();
                                        }
                                    }
                                }
                                if (isoffline != null)
                                {
                                    int msgs = Convert.ToInt32(Pac_tbl.Package_ID);
                                    tbl_DC_Package_Period tbDP = DbContext.tbl_DC_Package_Period.Where(x => x.Package_ID == msgs).FirstOrDefault();
                                    if (tbDP != null)
                                    {
                                        tbDP.Excluded_Price = Convert.ToDecimal(exprice);
                                        tbDP.Included_Price = Convert.ToDecimal(inprice);
                                        tbDP.Package_From = Convert.ToDateTime(from_dt);
                                        tbDP.Package_To = Convert.ToDateTime(to_dt);
                                        //tbDP.Package_Type = packagetype;
                                        tbDP.Is_Active = true;
                                        tbDP.Is_Deleted = false;
                                        tbDP.Inserted_Date = System.DateTime.Now;

                                        DbContext.SaveChanges();

                                    }

                                    else
                                    {
                                        tbl_DC_Package_Period tbDPP = new tbl_DC_Package_Period();
                                        tbDPP.Package_ID = msgs;
                                        tbDPP.Excluded_Price = Convert.ToDecimal(exprice);
                                        tbDPP.Included_Price = Convert.ToDecimal(inprice);
                                        tbDPP.Package_From = Convert.ToDateTime(from_dt);
                                        tbDPP.Package_To = Convert.ToDateTime(to_dt);
                                        //tbDP.Package_Type = packagetype;
                                        tbDPP.Is_Active = true;
                                        tbDPP.Is_Deleted = false;
                                        tbDPP.Inserted_Date = System.DateTime.Now;
                                        DbContext.tbl_DC_Package_Period.Add(tbDPP);
                                        DbContext.SaveChanges();



                                    }



                                }

                                TempData["SuccessMessage"] = "Package Updated Successfully.";
                                return Json("success", JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                TempData["WarningMessage"] = "Invalid package details.";
                            }
                        }
                        catch (Exception ex)
                        {
                            TempData["ErrorMessage"] = "Something Went Wrong.";
                            return RedirectToAction("AddNewPackage", "Admin", new { chk_pac = chk_pac, Package_Name = Package_Name, Package_Desc = Package_Desc, Price = Price, Package_Limit = Package_Limit, Subscripttion_Period = Subscripttion_Period, Package_Total = Package_Total, Board_Id = Board_Id, Class_Id = Class_Id, mods_id = mods_id, Tablet_Id = Tablet_Id });
                        }
                        #endregion
                    }
                }
            }
            catch
            {
                TempData["ErrorMessage"] = "Somethimg went wrong.";
            }
            return Json("save", JsonRequestBehavior.AllowGet);
        }
        public Image Base64ToImage_pkg(string packname, string base64String)
        {
            // Convert base 64 string to byte[]
            byte[] imageBytes = Convert.FromBase64String(base64String);
            // Convert byte[] to Image
            using (var ms = new MemoryStream(imageBytes, 0, imageBytes.Length))
            {
                string filename = packname + ".jpg";
                Image image = Image.FromStream(ms, true);
                var img = new Bitmap(Image.FromStream(ms));
                string tempFolderName = Server.MapPath("~/Images/" + filename);
                image.Save(tempFolderName, ImageFormat.Jpeg);
                return image;
            }
        }
        public ActionResult Delete_package(int? id)
        {
            try
            {
                if (id != null)
                {
                    tbl_DC_Package obj = DbContext.tbl_DC_Package.Where(x => x.Package_ID == id).FirstOrDefault();
                    obj.Modified_Date = today;
                    obj.Is_Active = false;
                    obj.Is_Deleted = true;
                    //obj.Modified_By = 
                    DbContext.Entry(obj).State = EntityState.Modified;
                    DbContext.SaveChanges();

                    var obj1 = DbContext.tbl_DC_Package_Dtl.Where(x => x.Package_ID == id);
                    if (obj1 != null)
                    {
                        foreach (tbl_DC_Package_Dtl qa in obj1)
                        {
                            qa.Modified_Date = today;
                            qa.Is_Active = false;
                            qa.Is_Deleted = true;
                            //obj.Modified_By = 
                            DbContext.Entry(qa).State = EntityState.Modified;
                            DbContext.SaveChanges();
                        }
                    }

                    var obj2 = DbContext.tbl_DC_PackageSub_Dtl.Where(x => x.Package_ID == id);
                    if (obj2 != null)
                    {
                        foreach (tbl_DC_PackageSub_Dtl dtl in obj2)
                        {
                            dtl.Modified_Date = today;
                            dtl.Is_Active = false;
                            dtl.Is_Deleted = true;
                            //obj.Modified_By = 
                            DbContext.Entry(dtl).State = EntityState.Modified;
                            DbContext.SaveChanges();
                        }
                    }
                    TempData["SuccessMessage"] = "Package deleted successfully.";
                    return RedirectToAction("PackageCreate");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }

            return RedirectToAction("PackageCreate");
        }

        [HttpPost]
        public ActionResult GetTabletPrice(int id)
        {
            var price = DbContext.tbl_DC_Tablet_Purchase.Where(x => x.Tablet_Id == id).FirstOrDefault();

            return Json(price.Tablet_Price, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region --------------------------------------TeacherShift-------------------------------------------
        [HttpPost]
        public ActionResult TeacherShift(string ShiftMst_ID, string Teacher_ID, string Shift_Days, string Efective_Date, string Shift_teach_ID)
        {
            ViewBag.setting = "setting";
            if (Convert.ToString(Session["ROLE_CODE"]) == "S")
            {
                if (Session["Role_Id"] != null)
                {
                    int id = Convert.ToInt32(Session["Role_Id"]);
                    var p_chk = DbContext.tbl_DC_Menu_Permission.Where(x => x.Menu_Key_ID == 2 && x.ROLE_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    if (p_chk == null)
                    {
                        return RedirectToAction("checkpermission");
                    }
                }
            }
            string sft_id = string.Empty;
            string thr_id = string.Empty;
            string sft_day = string.Empty;
            try
            {
                Menupermission();
                if (Efective_Date != "")
                {
                    DateTime _tdt = Convert.ToDateTime(Efective_Date);

                    if (_tdt.Date >= today.Date)
                    {
                        if (ShiftMst_ID != null)
                        {
                            sft_id = ShiftMst_ID;
                        }
                        if (Teacher_ID != null)
                        {
                            thr_id = Teacher_ID;
                        }
                        if (Shift_Days != null)
                        {
                            sft_day = Shift_Days;
                        }
                        int teacherid = Convert.ToInt32(Teacher_ID);
                        var teacher = DbContext.tbl_DC_Teacher.Where(x => x.Teach_ID == teacherid && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        if (teacher != null)
                        {
                            #region Update Shift
                            if (Shift_teach_ID != "" && Shift_teach_ID != null)
                            {
                                int teacher_asinid = Convert.ToInt32(Shift_teach_ID);
                                string shiftdays = Shift_Days.ToString();
                                int shiftid = Convert.ToInt32(ShiftMst_ID);
                                DateTime effective_date = Convert.ToDateTime(Efective_Date);


                                //
                                var findshift_time = DbContext.tbl_DC_Shift_Teacher_Assign.Where(x => x.Teacher_ID == teacherid && x.Shift_Days == Shift_Days && x.Is_Active == true && x.Is_Deleted == false).ToList();
                                if (findshift_time != null)
                                {
                                    for (int i = 0; i < findshift_time.Count; i++)
                                    {
                                        int shift_id = Convert.ToInt32(findshift_time[i].ShiftMst_ID);
                                        var if_find = DbContext.tbl_DC_Shift_Mst.Where(x => x.ShiftMst_ID == shift_id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                                        if (if_find != null)
                                        {
                                            DateTime frm_time = Convert.ToDateTime(if_find.Shift_From_Time);
                                            DateTime to_time = Convert.ToDateTime(if_find.Shift_To_Time);
                                            var selected_shift_from_time = DbContext.tbl_DC_Shift_Mst.Where(x => x.ShiftMst_ID == shiftid && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                                            DateTime select_time = Convert.ToDateTime(selected_shift_from_time.Shift_From_Time);

                                            if ((frm_time < select_time) && (to_time > select_time))
                                            {
                                                TempData["WarningMessage"] = "Teacher  shift time is not suitable.";
                                                return RedirectToAction("CreateShift", "Admin");
                                            }
                                        }
                                    }
                                }

                                //
                                var shift_found = DbContext.tbl_DC_Shift_Teacher_Assign.Where(x => x.Shift_Days == shiftdays && x.ShiftMst_ID == shiftid && x.Efective_Date == effective_date && x.Is_Active == true && x.Is_Deleted == false && x.Teacher_ID == teacherid).FirstOrDefault();
                                if (shift_found == null)
                                {
                                    tbl_DC_Shift_Teacher_Assign thr = DbContext.tbl_DC_Shift_Teacher_Assign.Where(x => x.Shift_teach_ID == teacher_asinid && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                                    thr.ShiftMst_ID = shiftid;
                                    thr.Shift_Days = shiftdays;
                                    thr.Teacher_ID = teacherid;
                                    thr.Efective_Date = effective_date;
                                    thr.Modified_Date = today;
                                    thr.Modified_By = HttpContext.User.Identity.Name;
                                    DbContext.SaveChanges();
                                    TempData["SuccessMessage"] = "Shift updated successfully.";

                                    return RedirectToAction("CreateShift", "Admin");
                                }
                                else
                                {
                                    TempData["WarningMessage"] = "Shift already assigned.";
                                    return RedirectToAction("CreateShift", "Admin");
                                }
                            }
                            #endregion

                            #region Add shift
                            else
                            {
                                string shiftdays = Shift_Days.ToString();
                                int shiftid = Convert.ToInt32(ShiftMst_ID);
                                DateTime effective_date = Convert.ToDateTime(Efective_Date);

                                var shift_found = DbContext.tbl_DC_Shift_Teacher_Assign.Where(x => x.Shift_Days == shiftdays && x.ShiftMst_ID == shiftid && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                                if (shift_found == null)
                                {
                                    var shift = DbContext.tbl_DC_Shift_Teacher_Assign.Where(x => x.Teacher_ID == teacherid && x.Shift_Days == shiftdays && x.ShiftMst_ID == shiftid && x.Efective_Date == effective_date && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                                    if (shift == null)
                                    {
                                        ///////region for find shift time between
                                        var findshift_time = DbContext.tbl_DC_Shift_Teacher_Assign.Where(x => x.Teacher_ID == teacherid && x.Shift_Days == Shift_Days && x.Is_Active == true && x.Is_Deleted == false).ToList();
                                        if (findshift_time != null)
                                        {
                                            for (int i = 0; i < findshift_time.Count; i++)
                                            {
                                                int shift_id = Convert.ToInt32(findshift_time[i].ShiftMst_ID);
                                                var if_find = DbContext.tbl_DC_Shift_Mst.Where(x => x.ShiftMst_ID == shift_id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                                                if (if_find != null)
                                                {
                                                    DateTime frm_time = Convert.ToDateTime(if_find.Shift_From_Time);
                                                    DateTime to_time = Convert.ToDateTime(if_find.Shift_To_Time);
                                                    var selected_shift_from_time = DbContext.tbl_DC_Shift_Mst.Where(x => x.ShiftMst_ID == shiftid && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                                                    DateTime select_time = Convert.ToDateTime(selected_shift_from_time.Shift_From_Time);

                                                    if ((frm_time < select_time) && (to_time > select_time))
                                                    {
                                                        TempData["WarningMessage"] = "Teacher shift time is not suitable.";
                                                        return RedirectToAction("CreateShift", "Admin");
                                                    }
                                                }
                                            }
                                        }

                                        tbl_DC_Shift_Teacher_Assign thr = new tbl_DC_Shift_Teacher_Assign();
                                        thr.ShiftMst_ID = shiftid;
                                        thr.Shift_Days = shiftdays;
                                        thr.Teacher_ID = teacherid;
                                        thr.Efective_Date = effective_date;
                                        thr.Inserted_Date = today;
                                        thr.Inserted_By = HttpContext.User.Identity.Name;
                                        thr.Is_Active = true;
                                        thr.Is_Deleted = false;
                                        DbContext.tbl_DC_Shift_Teacher_Assign.Add(thr);
                                        DbContext.SaveChanges();

                                        //send mail to teacher on shift creation
                                        if (teacher.Email_ID != null && teacher.Email_ID != "")
                                        {
                                            var getall = DbContext.SP_DC_Get_maildetails("T_SHIFT").FirstOrDefault();
                                            if (getall != null)
                                            {
                                                string msgbody = getall.EmailConf_Body.ToString().Replace("{{name}}", teacher.Teacher_Name).Replace("{{shift}}", "for " + Shift_Days).Replace("{{effdate}}", Efective_Date);
                                                sendMail1("T_SHIFT", teacher.Email_ID, getall.Email_Subject, teacher.Teacher_Name, msgbody);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        TempData["WarningMessage"] = "Teacher is already assigned.";
                                        return RedirectToAction("CreateShift", "Admin");

                                    }
                                }
                                else
                                {
                                    TempData["WarningMessage"] = "Selected shift is already assigned.";
                                    return RedirectToAction("CreateShift", "Admin");
                                }


                                TempData["SuccessMessage"] = "Shift created successfully.";
                                return RedirectToAction("CreateShift", "Admin");
                            }
                            #endregion
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "Invalid teacher details.";
                        }
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Effective date should be greater or equal to today date.";
                        //return RedirectToAction("TeacherShift", "Admin", new { sft_id = sft_id, thr_id = thr_id, sft_day = sft_day, Efective_Date = Efective_Date });
                        return RedirectToAction("TeacherShift", "Admin");
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "Please enter effective date.";
                    //return RedirectToAction("TeacherShift", "Admin", new { sft_id = sft_id, thr_id = thr_id, sft_day = sft_day, Efective_Date = Efective_Date });
                }

            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
                //return RedirectToAction("TeacherShift", "Admin", new { sft_id = sft_id, thr_id = thr_id, sft_day = sft_day, Efective_Date = Efective_Date });
            }
            return RedirectToAction("TeacherShift", "Admin");
        }
        [HttpGet]
        public ActionResult TeacherShift(int? id, string sft_id, string thr_id, string sft_day, string Efective_Date)
        {
            ViewBag.setting = "setting";
            try
            {
                Menupermission();
                if (Convert.ToString(Session["ROLE_CODE"]) == "S")
                {
                    if (Session["Role_Id"] != null)
                    {
                        int id1 = Convert.ToInt32(Session["Role_Id"]);
                        var p_chk = DbContext.tbl_DC_Menu_Permission.Where(x => x.Menu_Key_ID == 2 && x.ROLE_ID == id1 && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        if (p_chk == null)
                        {
                            return RedirectToAction("checkpermission");
                        }
                    }
                }
                ViewBag.Breadcrumb = "TeacherShift";
                if (id != null)
                {
                    var datas = DbContext.tbl_DC_Shift_Teacher_Assign.Where(x => x.Shift_teach_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    if (datas != null)
                    {
                        int S_id = Convert.ToInt32(datas.ShiftMst_ID);
                        int T_id = Convert.ToInt32(datas.Teacher_ID);

                        //ViewBag.Shift = DbContext.tbl_DC_Shift_Mst.Where(x => x.ShiftMst_ID == S_id).ToList();
                        //ViewBag.Teachername = DbContext.tbl_DC_Teacher.Where(x => x.Teach_ID == T_id).ToList();
                        ViewBag.Shift = S_id;
                        ViewBag.Teachername = T_id;
                        ViewBag.Days = datas.Shift_Days;
                        ViewBag.Effectivedate = datas.Efective_Date;
                        ViewBag.ids = datas.Shift_teach_ID;
                    }

                    else
                    {

                        TempData["ErrorMessage"] = "Teacher doesnot exist, Please try again.";
                    }
                }
                else
                {
                    ViewBag.Shift = sft_id;
                    ViewBag.Teachernamed = thr_id;
                    ViewBag.Days = sft_day;
                    ViewBag.Effectivedate = Efective_Date;
                }
                ViewBag.shiftdetails = DbContext.tbl_DC_Shift_Mst.Where(x => x.Is_Active == true && x.Is_Deleted == false).ToList();
                ViewBag.teachernames = DbContext.tbl_DC_Teacher.Where(x => x.Is_Active == true && x.Is_Deleted == false).ToList();

            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return View();
        }

        [HttpGet]
        public ActionResult CreateShift()
        {
            ViewBag.setting = "setting";
            try
            {
                Menupermission();
                if (Convert.ToString(Session["ROLE_CODE"]) == "S")
                {
                    if (Session["Role_Id"] != null)
                    {
                        int id = Convert.ToInt32(Session["Role_Id"]);
                        var p_chk = DbContext.tbl_DC_Menu_Permission.Where(x => x.Menu_Key_ID == 2 && x.ROLE_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        if (p_chk == null)
                        {
                            return RedirectToAction("checkpermission");
                        }
                    }
                }
                ViewBag.Breadcrumb = "Teacher Shift";
                ViewBag.Teacher_Shift = DbContext.View_DC_Teacher_Shift.OrderByDescending(x => x.Shift_teach_ID).ToList();

            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return View();
        }

        public ActionResult DeleteShift_Teacher(int? id)
        {
            ViewBag.setting = "setting";
            try
            {
                if (id != null)
                {
                    tbl_DC_Shift_Teacher_Assign tchr = DbContext.tbl_DC_Shift_Teacher_Assign.Where(x => x.Shift_teach_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    tchr.Is_Active = false;
                    tchr.Is_Deleted = true;
                    DbContext.Entry(tchr).State = EntityState.Modified;
                    DbContext.SaveChanges();
                    TempData["SuccessMessage"] = "Shift deleted successfully.";
                }
            }
            catch (Exception ex)
            {

                TempData["ErrorMessage"] = "Something went wrong.";
            }

            return RedirectToAction("CreateShift");
        }
        #endregion

        #region ----------------------------------------- Module --------------------------------------------
        [HttpGet]
        public ActionResult ViewModule()
        {
            Menupermission();
            if (Convert.ToString(Session["ROLE_CODE"]) == "S")
            {
                if (Session["Role_Id"] != null)
                {
                    int id = Convert.ToInt32(Session["Role_Id"]);
                    var p_chk = DbContext.tbl_DC_Menu_Permission.Where(x => x.Menu_Key_ID == 4 && x.ROLE_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    if (p_chk == null)
                    {
                        return RedirectToAction("checkpermission");
                    }
                }
            }
            ViewBag.Breadcrumb = "Module";
            ViewBag.moduledata = (from a in DbContext.tbl_DC_Board.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                  join b in DbContext.tbl_DC_Class.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                      on a.Board_Id equals b.Board_Id
                                  join c in DbContext.tbl_DC_Subject.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                      on b.Class_Id equals c.Class_Id
                                  join d in DbContext.tbl_DC_Chapter.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                      on c.Subject_Id equals d.Subject_Id
                                  join f in DbContext.tbl_DC_Module.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                      on d.Chapter_Id equals f.Chapter_Id
                                  select new DigiChampsModel.DigiChampsModuleModel
                                  {
                                      Module_ID = f.Module_ID,
                                      Module_Name = f.Module_Name,
                                      Chapter = d.Chapter,
                                      Subject = c.Subject,
                                      Board_Name = a.Board_Name,
                                      Class_Name = b.Class_Name,
                                      Module_Desc = f.Module_Desc,
                                      Module_Content = f.Module_Content,
                                      Module_video = f.Module_video,
                                      Is_Free = f.Is_Free,
                                      Question_PDF_Name = f.Question_PDF_Name,
                                      Upload_PDF = f.Module_Content_Name
                                  }).OrderByDescending(x => x.Module_ID).ToList();

            return View();
        }
        [HttpGet]
        public ActionResult CreateModule(string id)
        {
            Menupermission();
            if (Convert.ToString(Session["ROLE_CODE"]) == "S")
            {
                if (Session["Role_Id"] != null)
                {
                    int id1 = Convert.ToInt32(Session["Role_Id"]);
                    var p_chk = DbContext.tbl_DC_Menu_Permission.Where(x => x.Menu_Key_ID == 4 && x.ROLE_ID == id1 && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    if (p_chk == null)
                    {
                        return RedirectToAction("checkpermission");
                    }
                }
            }
            ViewBag.Breadcrumb = "Module";

            string new_video_title = "VIDEO_TITLE";          // This should be obtained from DB
            string api_secret = "d87d725cc2f9c4d0ae4fbf956e797089bdbcac7929143d478e9c60bb0d98629c";
            string uri = "https://api.vdocipher.com/v2/uploadPolicy/";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            using (StreamWriter writer = new StreamWriter(request.GetRequestStream(), Encoding.ASCII))
            {
                writer.Write("clientSecretKey=" + api_secret + "&title=" + new_video_title);
            }
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            dynamic otp_data;
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                string json_otp = reader.ReadToEnd();
                otp_data = JObject.Parse(json_otp);
            }
            ViewBag.upload_data = otp_data;
            ViewBag.board = new SelectList(DbContext.tbl_DC_Board.Where(b => b.Is_Active == true && b.Is_Deleted == false), "Board_Id", "Board_Name");
            if (id != "" && id != null)
            {
                UInt32 moduleid;
                if (UInt32.TryParse(id, out moduleid) == true)
                {
                    int mid = Convert.ToInt32(moduleid);
                    DigiChampsModel.DigiChampsModuleModel obj = (from a in DbContext.tbl_DC_Board.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                                                 join b in DbContext.tbl_DC_Class.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                                                     on a.Board_Id equals b.Board_Id
                                                                 join c in DbContext.tbl_DC_Subject.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                                                     on b.Class_Id equals c.Class_Id
                                                                 join d in DbContext.tbl_DC_Chapter.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                                                     on c.Subject_Id equals d.Subject_Id
                                                                 join f in DbContext.tbl_DC_Module.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                                                     on d.Chapter_Id equals f.Chapter_Id
                                                                 select new DigiChampsModel.DigiChampsModuleModel
                                                                 {
                                                                     Board_Id = a.Board_Id,
                                                                     Class_Id = b.Class_Id,
                                                                     Subject_Id = c.Subject_Id,
                                                                     Chapter_Id = d.Chapter_Id,
                                                                     Module_ID = f.Module_ID,
                                                                     Module_Name = f.Module_Name,
                                                                     Module_Desc = f.Module_Desc,
                                                                     Module_Content = f.Module_Content,
                                                                     Module_video = f.Module_video,
                                                                     Is_Free = f.Is_Free,
                                                                     Validity = f.Validity,
                                                                     Question_PDF = f.Question_PDF,
                                                                     Is_Free_Test = f.Is_Free_Test,
                                                                     No_Question = f.No_Of_Question,
                                                                     Upload_PDF = f.Module_Content_Name,
                                                                     Question_PDF_Name = f.Question_PDF_Name
                                                                 }).Where(x => x.Module_ID == mid).FirstOrDefault();
                    if (obj != null)
                    {
                        ViewBag.classid = obj.Class_Id;
                        ViewBag.subid = obj.Subject_Id;
                        ViewBag.chapterid = obj.Chapter_Id;
                        ViewBag.Modelvideo = obj.Module_video;
                        ViewBag.modelcontent = obj.Module_Content;
                        ViewBag.qstnpdf = obj.Question_PDF;
                        return View(obj);
                    }
                    else
                    {
                        TempData["WarningMessage"] = "Invalid module details.";
                    }
                }
                else
                {
                    TempData["WarningMessage"] = "Invalid module details.";
                }
            }

            return View();
        }


        [HttpPost]
        public ActionResult CreateModule(DigiChampsModel.DigiChampsModuleModel obj, HttpPostedFileBase RegImage3, HttpPostedFileBase module_pdf, HttpPostedFileBase Question_PDF, string key, string XAmzCredential, string XAmzAlgorithm, string XAmzDate, string Policy, string XAmzSignature, string success_action_status, string success_action_redirect, string url, string videoid)
        {
            Menupermission();
            ViewBag.Breadcrumb = "Module";
            string msg = string.Empty;
            try
            {
                ViewBag.Board_Id = new SelectList(DbContext.tbl_DC_Board.Where(x => x.Is_Active == true && x.Is_Deleted == false), "Board_Id", "Board_Name");
                if (ModelState.IsValid)
                {
                    if (obj.Module_Name.Trim() != "")
                    {
                        if (obj.Module_ID == null)
                        {
                            var module = DbContext.tbl_DC_Module.Where(x => x.Module_Name == obj.Module_Name && x.Board_Id == obj.Board_Id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                            if (module != null)
                            {
                                TempData["WarningMessage"] = "Module name already exists.";
                            }
                            else
                            {
                                tbl_DC_Module obj1 = new tbl_DC_Module();
                                obj1.Module_Name = obj.Module_Name;
                                obj1.Module_Desc = obj.Module_Desc;
                                //if (RegImage3 != null)
                                //{
                                //    int filelength = RegImage3.ContentLength;
                                //    string guid = Guid.NewGuid().ToString();
                                //    string path = string.Empty;
                                //    var fileName = Path.GetFileName(RegImage3.FileName.Replace(RegImage3.FileName.Split('.').FirstOrDefault().ToString(), guid.ToString()));
                                //    path = Path.Combine(Server.MapPath("../Module/Video/"), fileName);
                                //    RegImage3.SaveAs(path);
                                //    Program pr = new Program();
                                //    string retun_msg = pr.Programs(path, RegImage3.InputStream, filelength);
                                //    XmlDocument doc = new XmlDocument();
                                //    doc.LoadXml(retun_msg);
                                //    var element = ((XmlElement)doc.GetElementsByTagName("video")[0]); //null
                                //    var video_key = element.GetAttribute("key"); //cannot get values
                                //    obj1.Module_video = video_key;
                                //    //Delete the file from local server
                                //    if (System.IO.File.Exists(Server.MapPath("../Module/Video/" + fileName)))
                                //    {
                                //        System.IO.File.Delete(Server.MapPath("../Module/Video/" + fileName));
                                //    }
                                //}

                                if (RegImage3 != null)
                                {
                                    int filelength = RegImage3.ContentLength;
                                    string guid = Guid.NewGuid().ToString();
                                    string path = string.Empty;
                                    var fileName = Path.GetFileName(RegImage3.FileName.Replace(RegImage3.FileName.Split('.').FirstOrDefault().ToString(), guid.ToString()));
                                    Vdocipher vdcip = new Vdocipher();
                                    string retun_msg = vdcip.videoupload(url, key, XAmzCredential, XAmzAlgorithm, XAmzDate, Policy, XAmzSignature, success_action_status, success_action_redirect, RegImage3.InputStream, RegImage3.FileName);
                                    obj1.Module_video = videoid;



                                }
                                if (module_pdf != null)
                                {
                                    string guid = Guid.NewGuid().ToString();
                                    string path = string.Empty;
                                    var fileName = Path.GetFileName(module_pdf.FileName.Replace(module_pdf.FileName.Split('.').FirstOrDefault().ToString(), guid.ToString()));
                                    path = Path.Combine(Server.MapPath("../Module/PDF/"), fileName);
                                    module_pdf.SaveAs(path);
                                    obj1.Module_Content = fileName;
                                }
                                obj1.Board_Id = obj.Board_Id;
                                obj1.Class_Id = obj.Class_Id;
                                obj1.Subject_Id = obj.Subject_Id;
                                obj1.Chapter_Id = obj.Chapter_Id;
                                obj1.Is_Free = obj.Is_Free;
                                if (obj1.Is_Free == true)
                                {
                                    obj1.Validity = obj.Validity;
                                }
                                if (Question_PDF != null)
                                {
                                    string guid = Guid.NewGuid().ToString();
                                    string path = string.Empty;
                                    var fileName = Path.GetFileName(Question_PDF.FileName.Replace(Question_PDF.FileName.Split('.').FirstOrDefault().ToString(), guid.ToString()));
                                    path = Path.Combine(Server.MapPath("../Module/Question_PDF/"), fileName);
                                    Question_PDF.SaveAs(path);
                                    obj1.Question_PDF = fileName;
                                    obj1.No_Of_Question = obj.No_Question;
                                }
                                string extramsg = "";
                                obj1.Question_PDF_Name = obj.Question_PDF_Name;
                                obj1.Module_Content_Name = obj.Upload_PDF;
                                if (obj.Is_Free_Test == true)
                                {
                                    var iftest_chap = DbContext.tbl_DC_Module.Where(x => x.Chapter_Id == obj.Chapter_Id && x.Is_Free_Test == true).FirstOrDefault();
                                    if (iftest_chap == null)
                                    {
                                        obj1.Is_Free_Test = true;
                                    }
                                    else
                                    {
                                        obj1.Is_Free_Test = false;
                                        extramsg = " Each chapter can only contain single free test.";
                                    }
                                }
                                obj1.Inserted_By = HttpContext.User.Identity.Name;
                                obj1.Is_Active = true;
                                obj1.Is_Deleted = false;
                                DbContext.tbl_DC_Module.Add(obj1);
                                DbContext.SaveChanges();
                                msg = "1";      //"Module details saved successfully.";
                                TempData["SuccessMessage"] = "Module details saved successfully." + extramsg;
                            }
                        }
                        else
                        {
                            var module = DbContext.tbl_DC_Module.Where(x => x.Module_ID == obj.Module_ID && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                            if (module == null)
                            {
                                TempData["WarningMessage"] = "Module does not exist.";
                            }
                            else
                            {
                                int mod_id = Convert.ToInt32(obj.Module_ID);
                                tbl_DC_Module obj1 = DbContext.tbl_DC_Module.Where(x => x.Module_ID == mod_id).FirstOrDefault();
                                obj1.Module_Name = obj.Module_Name;
                                obj1.Module_Desc = obj.Module_Desc;
                                obj1.Is_Free = obj.Is_Free;
                                //if (RegImage3 != null)
                                //{
                                //    int filelength = RegImage3.ContentLength;
                                //    string guid = Guid.NewGuid().ToString();
                                //    string path = string.Empty;
                                //    var fileName = Path.GetFileName(RegImage3.FileName.Replace(RegImage3.FileName.Split('.').FirstOrDefault().ToString(), guid.ToString()));
                                //    path = Path.Combine(Server.MapPath("~/Module/Video/"), fileName);
                                //    RegImage3.SaveAs(path);
                                //    Program pr = new Program();
                                //    string retun_msg = pr.Programs(path, RegImage3.InputStream, filelength);
                                //    XmlDocument doc = new XmlDocument();
                                //    doc.LoadXml(retun_msg);
                                //    var element = ((XmlElement)doc.GetElementsByTagName("video")[0]); //null
                                //    var video_key = element.GetAttribute("key"); //cannot get values
                                //    obj1.Module_video = video_key;
                                //    //Delete the file from local server
                                //    if (System.IO.File.Exists(path))
                                //    {
                                //        System.IO.File.Delete(path);
                                //    }
                                //}

                                if (RegImage3 != null)
                                {
                                    int filelength = RegImage3.ContentLength;
                                    string guid = Guid.NewGuid().ToString();
                                    string path = string.Empty;
                                    var fileName = Path.GetFileName(RegImage3.FileName.Replace(RegImage3.FileName.Split('.').FirstOrDefault().ToString(), guid.ToString()));
                                    Vdocipher vdcip = new Vdocipher();
                                    string retun_msg = vdcip.videoupload(url, key, XAmzCredential, XAmzAlgorithm, XAmzDate, Policy, XAmzSignature, success_action_status, success_action_redirect, RegImage3.InputStream, RegImage3.FileName);
                                    obj1.Module_video = videoid;
                                }
                                if (module_pdf != null)
                                {
                                    string guid = Guid.NewGuid().ToString();
                                    string path = string.Empty;
                                    var fileName = Path.GetFileName(module_pdf.FileName.Replace(module_pdf.FileName.Split('.').FirstOrDefault().ToString(), guid.ToString()));
                                    path = Path.Combine(Server.MapPath("~/Module/PDF/"), fileName);
                                    module_pdf.SaveAs(path);
                                    obj1.Module_Content = fileName;
                                }
                                obj1.Board_Id = obj.Board_Id;
                                obj1.Class_Id = obj.Class_Id;
                                obj1.Subject_Id = obj.Subject_Id;
                                obj1.Chapter_Id = obj.Chapter_Id;

                                if (obj1.Is_Free == true)
                                {
                                    obj1.Validity = obj.Validity;
                                }
                                if (Question_PDF != null)
                                {
                                    string guid = Guid.NewGuid().ToString();
                                    string path = string.Empty;
                                    var fileName = Path.GetFileName(Question_PDF.FileName.Replace(Question_PDF.FileName.Split('.').FirstOrDefault().ToString(), guid.ToString()));
                                    path = Path.Combine(Server.MapPath("~/Module/Question_PDF/"), fileName);
                                    Question_PDF.SaveAs(path);
                                    obj1.Question_PDF = fileName;
                                    obj1.No_Of_Question = obj.No_Question;
                                }
                                obj1.Question_PDF_Name = obj.Question_PDF_Name;
                                obj1.Module_Content_Name = obj.Upload_PDF;
                                if (obj.Is_Free_Test == true)
                                {
                                    var iftest_chap = DbContext.tbl_DC_Module.Where(x => x.Chapter_Id == obj.Chapter_Id && x.Is_Free_Test == true).FirstOrDefault();
                                    if (iftest_chap == null)
                                    {
                                        obj1.Is_Free_Test = true;
                                    }
                                    else
                                    {
                                        obj1.Is_Free_Test = false;
                                    }
                                }

                                obj1.Inserted_By = HttpContext.User.Identity.Name;
                                obj1.Is_Active = true;
                                obj1.Is_Deleted = false;
                                obj1.Modified_Date = today;
                                DbContext.Entry(obj1).State = EntityState.Modified;
                                DbContext.SaveChanges();
                                msg = "11";         // "Module details updated successfully.";
                                TempData["SuccessMessage"] = "Module details updated successfully";
                            }
                        }
                    }
                    else
                    {
                        msg = "00";         // "Please enter module name.";
                    }
                }
            }
            catch (Exception ex)
            {
                msg = "Something went wrong.";
            }
            //return Json(msg, JsonRequestBehavior.AllowGet);
            return RedirectToAction("ViewModule");
        }


        public ActionResult DeleteModule(int? id)
        {
            try
            {
                if (id != null)
                {
                    tbl_DC_Module obj = DbContext.tbl_DC_Module.Where(x => x.Module_ID == id).FirstOrDefault();
                    if (obj != null)
                    {
                        obj.Is_Active = false;
                        obj.Is_Deleted = true;
                        obj.Modified_By = HttpContext.User.Identity.Name;
                        obj.Modified_Date = today;
                        DbContext.Entry(obj).State = EntityState.Modified;
                        DbContext.SaveChanges();

                        BotR.API.BotRAPI ab = new BotR.API.BotRAPI("rhhqK8sD", "M8sVGyMY0tpIftRBJ2PPVuzz");
                        if (obj.Module_video != null)
                        {
                            NameValueCollection shw = new NameValueCollection(){
                                                                                    {"video_key",obj.Module_video}
                                                                               };
                            string xml1 = ab.Call("/videos/delete", shw);
                        }
                        TempData["SuccessMessage"] = "Module deleted successfully.";
                    }
                    return RedirectToAction("ViewModule");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return View("ViewModule");
        }
        #endregion

        #region -------------------------------------- Shift Report------------------------------------------
        [HttpGet]
        public ActionResult Shift_Report()
        {
            ViewBag.report = "report";
            ViewBag.Shift_Report = "Active";
            Menupermission();
            if (Convert.ToString(Session["ROLE_CODE"]) == "S")
            {
                if (Session["Role_Id"] != null)
                {
                    int id = Convert.ToInt32(Session["Role_Id"]);
                    var p_chk = DbContext.tbl_DC_Menu_Permission.Where(x => x.Menu_Key_ID == 11 && x.ROLE_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    if (p_chk == null)
                    {
                        return RedirectToAction("checkpermission");
                    }
                }
            }
            ViewBag.Breadcrumb = "Shift Report";
            ViewBag.teacher_name = DbContext.tbl_DC_Teacher.Where(x => x.Is_Active == true && x.Is_Deleted == false).OrderByDescending(x => x.Teach_ID).ToList();

            return View();
        }
        [HttpPost]
        public ActionResult Shift_Report(int? Teacher_name)
        {
            ViewBag.Shift_Report = "Active";
            ViewBag.report = "report";
            Menupermission();
            if (Teacher_name != null)
            {
                ViewBag.Breadcrumb = "Shift Report";
                ViewBag.teacher_name = DbContext.tbl_DC_Teacher.Where(x => x.Is_Active == true && x.Is_Deleted == false).OrderByDescending(x => x.Teach_ID).ToList();
                var siftdata = DbContext.SP_DC_Shiftreport(Teacher_name).ToList();
                if (siftdata.Count > 0)
                {
                    ViewBag.subjectdata = siftdata;
                }
                else
                {
                    ViewBag.subjectdata = null;
                }

                return View("Shift_Report");
            }
            return View("Shift_Report");
        }
        #endregion

        #region -------------------------------------------------Teacher Registration ----------------------------------------------------------
        [HttpGet]
        public ActionResult Registration()
        {
            ViewBag.setting = "setting";
            Menupermission();
            if (Convert.ToString(Session["ROLE_CODE"]) == "S")
            {
                if (Session["Role_Id"] != null)
                {
                    int id = Convert.ToInt32(Session["Role_Id"]);
                    var p_chk = DbContext.tbl_DC_Menu_Permission.Where(x => x.Menu_Key_ID == 2 && x.ROLE_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    if (p_chk == null)
                    {
                        return RedirectToAction("checkpermission");
                    }
                }
            }
            ViewBag.Breadcrumb = "Registration";
            ViewBag.teach = DbContext.tbl_DC_Teacher.Where(x => x.Is_Active == true && x.Is_Deleted == false).OrderByDescending(x => x.Teach_ID).ToList();
            ViewBag.user = DbContext.tbl_DC_USER_MASTER.Where(x => x.Is_Active == true && x.Is_Deleted == false).OrderByDescending(x => x.USER_ID).ToList();

            return View();
        }
        [HttpGet]
        public ActionResult AddRegistration()
        {
            ViewBag.setting = "setting";
            Menupermission();
            if (Convert.ToString(Session["ROLE_CODE"]) == "S")
            {
                if (Session["Role_Id"] != null)
                {
                    int id = Convert.ToInt32(Session["Role_Id"]);
                    var p_chk = DbContext.tbl_DC_Menu_Permission.Where(x => x.Menu_Key_ID == 2 && x.ROLE_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    if (p_chk == null)
                    {
                        return RedirectToAction("checkpermission");
                    }
                }
            }
            ViewBag.Breadcrumb = "Registration";
            var role_type = (from a in DbContext.tbl_DC_Role.Where(x => x.ROLE_CODE == "S" || x.ROLE_CODE == "T" && x.IS_ACTIVE == true && x.IS_DELTED == false)
                             select new GroupModel
                             {
                                 ROLE_ID = a.ROLE_ID,
                                 ROLE_TYPE = a.ROLE_TYPE
                             }).ToList();
            ViewBag.role_type = role_type;
            ViewBag.auto_pass = CreateRandomPassword(6);
            // ViewBag.auto_pass = DigiChampsModel.Encrypt_Password.HashPassword(CreateRandomPassword(6));
            return View();
        }

        public string getEmailid(int? id)   //get username of respective teacher & user
        {
            var email = (from c in DbContext.tbl_DC_Teacher.Where(d => d.Teach_ID == id && d.Is_Active == true && d.Is_Deleted == false) select c.Email_ID).SingleOrDefault();

            var uemail = (from d in DbContext.tbl_DC_USER_MASTER.Where(x => x.USER_ID == id && x.Is_Active == true && x.Is_Deleted == false) select d.EMAIL).SingleOrDefault();

            return email;
        }

        [HttpGet]
        public ActionResult EditRegistration(int? id, string type, string password1, string name)
        {
            ViewBag.setting = "setting";
            Menupermission();
            TempData["item1"] = null;
            TempData["item2"] = null;
            try
            {
                if (id != null)
                {
                    if (type == "T")
                    {
                        tbl_DC_Teacher teacher = DbContext.tbl_DC_Teacher.Where(x => x.Teach_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        ViewBag.tid = id;


                        tbl_DC_Role obj = DbContext.tbl_DC_Role.Where(x => x.ROLE_CODE == "T" && x.IS_ACTIVE == true && x.IS_DELTED == false).FirstOrDefault();
                        ViewBag.ROLE_ID = obj.ROLE_ID;
                        ViewBag.ROLE_TYPE = obj.ROLE_TYPE;


                        ViewBag.name = teacher.Teacher_Name;
                        ViewBag.address = teacher.Address;
                        ViewBag.dateofbirth = teacher.DateOfBirth;
                        ViewBag.designation = teacher.Designation;
                        ViewBag.email = teacher.Email_ID;
                        ViewBag.gender = teacher.Gender;
                        ViewBag.mobile = teacher.Mobile;
                        ViewBag.image = teacher.Image;
                        string email = teacher.Email_ID;

                        var data = DbContext.tbl_DC_USER_SECURITY.Where(x => x.USER_NAME == email).Select(x => x.PASSWORD).FirstOrDefault();
                        ViewBag.auto_pass = Convert.ToString(data).Substring(0, 6);
                        TempData["image"] = teacher.Image;
                        return View("AddRegistration");
                        TempData["item2"] = "value2";
                    }
                    else if (type == "S")
                    {
                        tbl_DC_USER_MASTER user = DbContext.tbl_DC_USER_MASTER.Where(x => x.USER_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        ViewBag.uid = id;
                        int rtp_id = Convert.ToInt32(user.ROLE_TYPE);
                        tbl_DC_Role obj = DbContext.tbl_DC_Role.Where(x => x.ROLE_CODE == "S" && x.ROLE_ID == rtp_id && x.IS_ACTIVE == true && x.IS_DELTED == false).FirstOrDefault();
                        ViewBag.ROLE_ID1 = obj.ROLE_ID;
                        ViewBag.ROLE_TYPE1 = obj.ROLE_TYPE;


                        ViewBag.name = user.USER_NAME;
                        ViewBag.address = user.ADDRESS1;
                        ViewBag.dateofbirth = user.DATE_OF_BIRTH;
                        ViewBag.designation = user.DESIGNATION;
                        ViewBag.email = user.EMAIL;
                        ViewBag.gender = user.GENDER;
                        ViewBag.mobile = user.MOBILE;
                        ViewBag.image = user.IMAGE;
                        string email = user.EMAIL;

                        var data = DbContext.tbl_DC_USER_SECURITY.Where(x => x.USER_NAME == email).Select(x => x.PASSWORD).FirstOrDefault();
                        ViewBag.auto_pass = Convert.ToString(data).Substring(0, 6);
                        TempData["image"] = user.IMAGE;
                        return View("AddRegistration");
                        TempData["item1"] = "value1";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "User does not exists.";
                    }
                }
                else
                {

                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return View("AddRegistration");
        }

        [HttpPost]
        public JsonResult AddRegistration(string selectuser, int? teacher_id, int? adm_user_id, string name, string designation, string gender, DateTime dateofbirth, string emailid, string mobile, string address, string imagebase64, string password1)
        {
            int rid = Convert.ToInt32(selectuser);
            ViewBag.setting = "setting";
            Menupermission();
            string message = string.Empty;
            TempData["item1"] = null;
            TempData["item2"] = null;
            try
            {
                if (ModelState.IsValid)
                {
                    if (Convert.ToDateTime(dateofbirth).Date < DateTime.Now.Date)
                    {
                        if (name.Trim() == "" || mobile.Trim() == "")
                        {
                            message = "0";       //"Please enter name or mobile number.";
                        }
                        else
                        {
                            if (imagebase64 != "" && imagebase64 != null)
                            {
                                string[] image1 = imagebase64.Split(',');
                                string imgname = CreateRandomPassword(6);
                                Base64ToImage_teach(imgname, image1[1]);
                                imagebase64 = imgname + ".jpg";
                            }
                            if (rid == 2)
                            {
                                if (teacher_id == null)
                                {

                                    var v = DbContext.tbl_DC_Teacher.Where(x => x.Mobile == mobile && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();

                                    if (v != null)
                                    {
                                        message = "-1"; //"Mobile number alredy exist.";
                                    }
                                    var w = DbContext.tbl_DC_Teacher.Where(x => x.Email_ID == emailid && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                                    if (w != null)
                                    {
                                        message = "-5";    // "Email-Id already exist.";
                                    }
                                    else
                                    {
                                        tbl_DC_Teacher teacher = new tbl_DC_Teacher();

                                        if (imagebase64 != "")
                                        {
                                            teacher.Image = imagebase64.ToString();
                                        }

                                        teacher.Teacher_Name = name;
                                        teacher.Address = address;
                                        teacher.DateOfBirth = dateofbirth.Date;
                                        teacher.Designation = designation;
                                        teacher.Email_ID = emailid;
                                        teacher.Gender = gender;
                                        teacher.Mobile = mobile;
                                        teacher.Is_Active = true;
                                        teacher.Is_Deleted = false;
                                        teacher.Inserted_By = HttpContext.User.Identity.Name;
                                        teacher.Inserted_Date = today;

                                        DbContext.tbl_DC_Teacher.Add(teacher);
                                        DbContext.SaveChanges();

                                        int typ_id = 1;
                                        var prefix = DbContext.tbl_DC_Prefix.Where(x => x.Is_Active == true && x.Is_Deleted == false && x.PrefixType_ID == typ_id).Select(x => x.Prefix_Name).FirstOrDefault();

                                        string id = DbContext.tbl_DC_Role.Where(x => x.ROLE_ID == rid).Select(x => x.ROLE_CODE).FirstOrDefault();

                                        tbl_DC_USER_SECURITY obj = new tbl_DC_USER_SECURITY();
                                        obj.USER_NAME = teacher.Email_ID;
                                        obj.USER_CODE = prefix + teacher.Teach_ID;
                                        obj.ROLE_TYPE = rid;
                                        obj.ROLE_CODE = id;
                                        obj.STATUS = "A";
                                        obj.IS_ACCEPTED = true;
                                        obj.PASSWORD = DigiChampsModel.Encrypt_Password.HashPassword(password1);
                                        DbContext.tbl_DC_USER_SECURITY.Add(obj);
                                        DbContext.SaveChanges();

                                        message = "11";  //"Teacher details saved successfully.";

                                        //Teacher_sendMail(emailid, name, password1, "T");
                                        if (emailid != null && emailid != "")
                                        {
                                            var getall = DbContext.SP_DC_Get_maildetails("T_REG").FirstOrDefault();
                                            if (getall != null)
                                            {
                                                string msgbody = getall.EmailConf_Body.ToString().Replace("{{name}}", name).Replace("{{username}}", emailid).Replace("{{password}}", password1);
                                                sendMail1("T_REG", emailid, getall.Email_Subject, name, msgbody);
                                            }
                                        }
                                        TempData["item2"] = "value2";
                                        // return RedirectToAction("Registration");
                                    }
                                }
                                else
                                {
                                    tbl_DC_Teacher obj1 = DbContext.tbl_DC_Teacher.Where(x => x.Teach_ID == teacher_id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                                    if (obj1 != null)
                                    {
                                        var v = DbContext.tbl_DC_Teacher.Where(x => x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();

                                        if (v != null)
                                        {
                                            //message = "-1";              //"Mobile number already exist.";
                                        }
                                        obj1.Teacher_Name = name;
                                        if (imagebase64 != "")
                                        {
                                            obj1.Image = imagebase64.ToString();
                                        }

                                        obj1.Address = address;
                                        obj1.DateOfBirth = dateofbirth;
                                        obj1.Designation = designation;
                                        // obj1.Email_ID = emailid;
                                        obj1.Gender = gender;
                                        obj1.Mobile = mobile;
                                        obj1.Is_Active = true;
                                        obj1.Is_Deleted = false;
                                        obj1.Modified_By = HttpContext.User.Identity.Name;
                                        obj1.Modified_Date = today;
                                        //tbl_DC_USER_SECURITY obj = new tbl_DC_USER_SECURITY();
                                        //obj.PASSWORD = password1;
                                        DbContext.SaveChanges();
                                        message = "12";         //"Teacher details updated successfully.";
                                        //return RedirectToAction("Registration");
                                        TempData["item2"] = "value2";
                                    }
                                }
                            }
                            else if (rid != 2)
                            {
                                if (adm_user_id == null)
                                {
                                    var v = DbContext.tbl_DC_USER_MASTER.Where(x => x.MOBILE == mobile && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();

                                    if (v != null)
                                    {
                                        message = "-1";         //"Mobile number already exist.";
                                    }
                                    var w = DbContext.tbl_DC_USER_MASTER.Where(x => x.EMAIL == emailid && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                                    if (w != null)
                                    {
                                        message = "-5";    // "Email-Id already exist.";
                                    }
                                    else
                                    {
                                        try
                                        {
                                            string id = DbContext.tbl_DC_Role.Where(x => x.ROLE_ID == rid).Select(x => x.ROLE_CODE).FirstOrDefault();
                                            tbl_DC_USER_MASTER user = new tbl_DC_USER_MASTER();

                                            if (imagebase64 != "")
                                            {
                                                user.IMAGE = imagebase64.ToString();
                                            }
                                            user.ROLE_TYPE = rid;
                                            user.ROLE_CODE = id;
                                            user.USER_NAME = name;
                                            user.ADDRESS1 = address;
                                            user.DATE_OF_BIRTH = dateofbirth;
                                            user.DESIGNATION = designation;
                                            user.EMAIL = emailid;
                                            user.GENDER = gender;
                                            user.MOBILE = mobile;
                                            user.Is_Active = true;
                                            user.Is_Deleted = false;
                                            user.Inserted_By = HttpContext.User.Identity.Name;
                                            user.Inserted_Date = today;
                                            DbContext.tbl_DC_USER_MASTER.Add(user);
                                            DbContext.SaveChanges();

                                            int typ_id = 2;
                                            var prefix = DbContext.tbl_DC_Prefix.Where(x => x.Is_Active == true && x.Is_Deleted == false && x.PrefixType_ID == typ_id).Select(x => x.Prefix_Name).FirstOrDefault();

                                            //int id = Convert.ToInt32(DbContext.tbl_DC_Role.Where(x => x.ROLE_CODE == selectuser).Select(x => x.ROLE_ID).FirstOrDefault());

                                            tbl_DC_USER_SECURITY obj = new tbl_DC_USER_SECURITY();
                                            obj.USER_NAME = user.EMAIL;
                                            int count = Convert.ToInt32(Convert.ToString(prefix + user.USER_ID).Count());
                                            obj.USER_CODE = prefix + user.USER_ID;
                                            obj.ROLE_TYPE = rid;
                                            obj.ROLE_CODE = id;
                                            obj.STATUS = "A";
                                            obj.IS_ACCEPTED = true;
                                            obj.PASSWORD = DigiChampsModel.Encrypt_Password.HashPassword(password1);
                                            DbContext.tbl_DC_USER_SECURITY.Add(obj);
                                            DbContext.SaveChanges();
                                            message = "21";             //"User details saved successfully.";

                                            //AdminUser_sendMail(emailid, name, password1, "S");
                                            if (emailid != null && emailid != "")
                                            {
                                                var getall = DbContext.SP_DC_Get_maildetails("T_REG").FirstOrDefault();
                                                if (getall != null)
                                                {
                                                    string msgbody = getall.EmailConf_Body.ToString().Replace("{{name}}", name).Replace("{{username}}", emailid).Replace("{{password}}", password1);
                                                    sendMail1("T_REG", emailid, getall.Email_Subject, name, msgbody);
                                                }
                                            }
                                            TempData["item1"] = "value1";
                                            //return RedirectToAction("Registration");
                                        }
                                        catch (Exception ex)
                                        {
                                            ex.InnerException.Message.ToString();
                                        }
                                    }

                                }
                                else
                                {
                                    tbl_DC_USER_MASTER obj1 = DbContext.tbl_DC_USER_MASTER.Where(x => x.USER_ID == adm_user_id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                                    if (obj1 != null)
                                    {
                                        var v = DbContext.tbl_DC_USER_MASTER.Where(x => x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();

                                        if (v != null)
                                        {
                                            // message = "-1";         //"Mobile number already exist.";
                                        }
                                        obj1.USER_NAME = name;
                                        if (imagebase64 != "")
                                        {
                                            obj1.IMAGE = imagebase64.ToString();
                                        }
                                        obj1.ADDRESS1 = address;
                                        obj1.DATE_OF_BIRTH = dateofbirth;
                                        obj1.DESIGNATION = designation;
                                        //obj1.EMAIL = emailid;
                                        obj1.GENDER = gender;
                                        obj1.MOBILE = mobile;
                                        obj1.Is_Active = true;
                                        obj1.Is_Deleted = false;
                                        obj1.Modified_By = HttpContext.User.Identity.Name;
                                        obj1.Modified_Date = today;
                                        DbContext.SaveChanges();
                                        message = "22";                 //"User details updated successfully.";
                                        //return RedirectToAction("Registration");
                                        TempData["item1"] = "value1";
                                    }
                                }
                            }
                            else
                            {
                                message = "-2";                 //"Please select user.";
                            }
                        }
                    }
                    else
                    {
                        message = "-4";                         //"Date of birth should be less than today date.";
                    }

                }
                else
                {
                    message = "-3";                             //"Please enter data properly.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return Json(message, JsonRequestBehavior.AllowGet);
        }

        public Image Base64ToImage_teach(string mob, string base64String)
        {
            // Convert base 64 string to byte[]
            byte[] imageBytes = Convert.FromBase64String(base64String);
            // Convert byte[] to Image
            using (var ms = new MemoryStream(imageBytes, 0, imageBytes.Length))
            {
                string filename = mob + ".jpg";
                Image image = Image.FromStream(ms, true);
                var img = new Bitmap(Image.FromStream(ms));
                string tempFolderName = Server.MapPath("~/Images/Teacherprofile/" + filename);
                image.Save(tempFolderName, ImageFormat.Jpeg);
                return image;
            }
        }
        public ActionResult DeleteRegistration(int? id, string type)
        {
            try
            {
                if (id != null)
                {
                    if (type == "T")
                    {
                        tbl_DC_Teacher obj = DbContext.tbl_DC_Teacher.Where(x => x.Teach_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        if (obj != null)
                        {
                            obj.Is_Active = false;
                            obj.Is_Deleted = true;
                            obj.Modified_By = HttpContext.User.Identity.Name;
                            obj.Modified_Date = today;
                            DbContext.Entry(obj).State = EntityState.Modified;
                            DbContext.SaveChanges();
                            string usercode = "TO" + obj.Teach_ID;
                            tbl_DC_USER_SECURITY tea = DbContext.tbl_DC_USER_SECURITY.Where(x => x.USER_CODE == usercode).FirstOrDefault();
                            if (tea != null)
                            {
                                tea.IS_ACCEPTED = false;
                                DbContext.SaveChanges();
                            }
                            else
                            {
                                TempData["ErrorMessage"] = "No data found.";
                                return RedirectToAction("Registration", "Admin");
                            }
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "No data found.";
                            return RedirectToAction("Registration", "Admin");
                        }


                        TempData["SuccessMessage"] = "Teacher details deleted successfully.";
                        return RedirectToAction("Registration", "Admin");
                    }
                    else if (type == "S")
                    {
                        tbl_DC_USER_MASTER obj1 = DbContext.tbl_DC_USER_MASTER.Where(x => x.USER_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        if (obj1 != null)
                        {
                            obj1.Is_Active = false;
                            obj1.Is_Deleted = true;
                            obj1.Modified_By = HttpContext.User.Identity.Name;
                            obj1.Modified_Date = today;
                            DbContext.Entry(obj1).State = EntityState.Modified;

                            DbContext.SaveChanges();
                            string usercode = "AO" + obj1.USER_ID;
                            tbl_DC_USER_SECURITY tea = DbContext.tbl_DC_USER_SECURITY.Where(x => x.USER_CODE == usercode).FirstOrDefault();

                            if (tea != null)
                            {
                                tea.IS_ACCEPTED = false;
                                DbContext.SaveChanges();
                            }
                            else
                            {
                                TempData["ErrorMessage"] = "No data found.";
                                return RedirectToAction("Registration", "Admin");
                            }

                        }
                        else
                        {
                            TempData["ErrorMessage"] = "No data found.";
                            return RedirectToAction("Registration", "Admin");
                        }
                        TempData["SuccessMessage"] = "User details deleted successfully.";
                        return RedirectToAction("Registration", "Admin");
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            //return Json(message, JsonRequestBehavior.AllowGet);
            return RedirectToAction("Registration", "Admin");
        }

        #endregion

        #region -----------------------------------------------------Ticket --------------------------------------------------------------------
        [HttpGet]
        public ActionResult Ticket()
        {
            try
            {
                Menupermission();
                if (Convert.ToString(Session["ROLE_CODE"]) == "S")
                {
                    if (Session["Role_Id"] != null)
                    {
                        int id = Convert.ToInt32(Session["Role_Id"]);
                        var p_chk = DbContext.tbl_DC_Menu_Permission.Where(x => x.Menu_Key_ID == 5 && x.ROLE_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        if (p_chk == null)
                        {
                            return RedirectToAction("checkpermission");
                        }
                    }
                }
                ViewBag.Breadcrumb = "view Tickets";
                overdue();
                var data = DbContext.View_DC_Tickets_and_Teacher.OrderByDescending(x => x.Ticket_ID).ToList();
                ViewBag.Ticket = data;
                ViewBag.teachernames_tickets = DbContext.View_DC_CourseAssign.ToList();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return View();
        }
        public ActionResult ViewTickectDetail(int? id)
        {
            try
            {
                Menupermission();
                if (Convert.ToString(Session["ROLE_CODE"]) == "S")
                {
                    if (Session["Role_Id"] != null)
                    {
                        int id1 = Convert.ToInt32(Session["Role_Id"]);
                        var p_chk = DbContext.tbl_DC_Menu_Permission.Where(x => x.Menu_Key_ID == 5 && x.ROLE_ID == id1 && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        if (p_chk == null)
                        {
                            return RedirectToAction("checkpermission");
                        }
                    }
                }
                ViewBag.Breadcrumb = "Ticket";
                if (id != null)
                {
                    var get = DbContext.View_DC_All_Tickets_Details.Where(x => x.Ticket_ID == id).ToList();
                    if (get.Count > 0)
                    {
                        ViewBag.check_answer = DbContext.tbl_DC_Ticket_Dtl.Where(x => x.Ticket_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        ViewBag.teacher = get.FirstOrDefault().Teach_ID;
                        ViewBag.status = get.FirstOrDefault().Status;
                        if (get.FirstOrDefault().Teach_ID != null)
                        {
                            int tr = Convert.ToInt32(get.FirstOrDefault().Teach_ID);
                            var tdata = DbContext.tbl_DC_Teacher.Where(x => x.Teach_ID == tr).FirstOrDefault();
                            if (tdata != null)
                            {
                                if (tdata.Teacher_Name != null)
                                {
                                    ViewBag.teacher = tdata.Teacher_Name;
                                }
                                else
                                {
                                    ViewBag.teacher = null;
                                }

                            }
                            else
                            {
                                ViewBag.teacher = null;
                            }
                        }
                        ViewBag.viewticket = get.ToList();
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Invalid ticket details.";
                        return RedirectToAction("Ticket");
                    }


                }
                else
                {
                    TempData["ErrorMessage"] = "Invalid ticket details.";
                    return RedirectToAction("Ticket");
                }
            }
            catch (Exception ex)
            {

                TempData["ErrorMessage"] = "Something went wrong.";
                return RedirectToAction("Ticket");
            }
            return View();
        }
        [HttpGet]
        public ActionResult RejectTicket(int? id)
        {
            try
            {
                if (Convert.ToString(Session["ROLE_CODE"]) == "S")
                {
                    if (Session["Role_Id"] != null)
                    {
                        int id1 = Convert.ToInt32(Session["Role_Id"]);
                        var p_chk = DbContext.tbl_DC_Menu_Permission.Where(x => x.Menu_Key_ID == 5 && x.ROLE_ID == id1 && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        if (p_chk == null)
                        {
                            return RedirectToAction("checkpermission");
                        }
                    }
                }
                ViewBag.Breadcrumb = "Ticket";
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
                    return RedirectToAction("Ticket");
                }
            }
            catch (Exception ex)
            {

                TempData["ErrorMessage"] = "Something went wrong.";

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
                            //Close_ticket_mail(tkt_rj.Student_ID.ToString(), tkt_rj.Ticket_No);
                            sendMail_close_reject("Ticket_close", get_student.Email.ToString(), get_student.Customer_Name, tkt_rj.Ticket_No.ToString(), "R", Remark_Reject.ToString());
                        }
                        TempData["SuccessMessage"] = "Question is rejected.";
                    }
                    else
                    {
                        TempData["WarningMessage"] = "Please provide reason of rejection.";
                    }
                }
                catch (Exception ex)
                {
                    TempData["WarningMessage"] = "Something went wrong.";

                }

                return RedirectToAction("RejectTicket");
            }
            else
            {
                return RedirectToAction("Logout", "Admin");
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
            if (ticket_no != "")
            {
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
            }
            else
            {
                msgbody = getall.EmailConf_Body.ToString().Replace("{{user_name}}", name);
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
                    Menupermission();
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
                        return RedirectToAction("Ticket", "Admin");
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
                return RedirectToAction("Logout", "Admin");
            }

            return View();
        }
        [HttpPost]
        public JsonResult AnswerReply(int Ticket_id, int Ticket_answerid, string msgbody, string close, string remark)
        {
            Menupermission();
            string message = string.Empty;
            if (Session["USER_CODE"] != null)
            {
                if (msgbody != "")
                {
                    try
                    {
                        int _Admin_id = Convert.ToInt32(Convert.ToString(Session["USER_CODE"].ToString().Trim()));
                        //tbl_DC_Ticket_Thread _ticket_thred = new tbl_DC_Ticket_Thread();
                        //_ticket_thred.Ticket_ID = Ticket_id;
                        //_ticket_thred.Ticket_Dtl_ID = Ticket_answerid;
                        //_ticket_thred.User_Comment = msgbody;
                        //_ticket_thred.User_Comment_Date = today;
                        //_ticket_thred.User_Id = _Admin_id;
                        //_ticket_thred.Is_Teacher = true;
                        //_ticket_thred.Is_Active = true;
                        //_ticket_thred.Is_Deleted = false;
                        //DbContext.tbl_DC_Ticket_Thread.Add(_ticket_thred);
                        //DbContext.SaveChanges();
                        if (close == "on")
                        {
                            tbl_DC_Ticket_Assign _tbl_close = DbContext.tbl_DC_Ticket_Assign.Where(x => x.Ticket_ID == Ticket_id).FirstOrDefault();
                            _tbl_close.Is_Close = true;
                            _tbl_close.Remark = remark;
                            _tbl_close.Close_Date = today;
                            _tbl_close.Modified_By = _Admin_id;
                            _tbl_close.Modified_Date = today;
                            DbContext.SaveChanges();
                            tbl_DC_Ticket _tbl_status = DbContext.tbl_DC_Ticket.Where(x => x.Ticket_ID == Ticket_id).FirstOrDefault();
                            _tbl_status.Status = "C";
                            _tbl_status.Modified_By = _Admin_id;
                            _tbl_status.Modified_Date = today;
                            DbContext.SaveChanges();
                            var get_student = DbContext.tbl_DC_Registration.Where(x => x.Regd_ID == _tbl_close.Student_ID).FirstOrDefault();
                            if (get_student.Email != null)
                            {
                                sendMail_close_reject("Ticket_close", get_student.Email.ToString(), get_student.Customer_Name, _tbl_close.Ticket_No.ToString(), "C", remark);
                            }
                        }
                        message = "1";
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

            return Json(message, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult AnswerTicket(string Answer_Ticket, HttpPostedFileBase RegImage3, string h_tkid)
        {
            Menupermission();
            if (h_tkid != "")
            {
                if (Answer_Ticket.Trim() != "")
                {
                    int id = Convert.ToInt32(h_tkid);

                    tbl_DC_Ticket_Dtl tk_dtl = new tbl_DC_Ticket_Dtl();
                    tk_dtl.Ticket_ID = id;
                    tk_dtl.Answer = Answer_Ticket;
                    tk_dtl.Replied_By = 1;//hard_coded
                    tk_dtl.Replied_Date = today;
                    tk_dtl.Is_Active = true;
                    tk_dtl.Is_Deleted = false;
                    if (RegImage3 != null)
                    {

                        string guid = Guid.NewGuid().ToString();
                        var fileName = Path.GetFileName(RegImage3.FileName.Replace(RegImage3.FileName.Split('.').FirstOrDefault().ToString(), guid.ToString()));
                        var path = Path.Combine(Server.MapPath("~/Images/Qusetionimages/"), fileName);
                        RegImage3.SaveAs(path);
                        tk_dtl.Answer_Image = fileName.ToString();

                    }
                    DbContext.tbl_DC_Ticket_Dtl.Add(tk_dtl);
                    DbContext.SaveChanges();
                    TempData["SuccessMessage"] = "You have answerd successfully.";
                    return RedirectToAction("AnswerTicket", "Admin", new { id = h_tkid });
                }
                else
                {
                    TempData["WarningMessage"] = "Insert a answer to the question.";
                    return RedirectToAction("AnswerTicket", "Admin", new { id = h_tkid });
                }
            }

            else
            {
                TempData["ErrorMessage"] = "Something went wrong.";

            }

            return View();
        }
        public ActionResult filtertickets(string id)
        {
            Menupermission();
            ViewBag.Breadcrumb = "view Tickets";
            ViewBag.teachernames_tickets = DbContext.View_DC_CourseAssign.ToList();

            if (id == "U")
            {
                var data = DbContext.View_DC_Tickets_and_Teacher.Where(x => x.Teach_ID == null).OrderByDescending(x => x.Ticket_ID).ToList();
                if (data.Count > 0)
                {
                    ViewBag.Ticket = data;
                }
            }
            else
            {
                var data = DbContext.View_DC_Tickets_and_Teacher.Where(x => x.Status == id).OrderByDescending(x => x.Ticket_ID).ToList();
                if (data.Count > 0)
                {
                    ViewBag.Ticket = data;
                }
            }

            return View("Ticket");
        }
        [HttpPost]
        public JsonResult Delete_ALLTickets(int[] Chk_tickets)
        {
            Menupermission();
            string message = string.Empty;

            if (Chk_tickets != null)
            {
                try
                {
                    for (int i = 0; i < Chk_tickets.Length; i++)
                    {
                        int ticket_id = Chk_tickets[i];
                        tbl_DC_Ticket _tickte_delete = DbContext.tbl_DC_Ticket.Where(x => x.Ticket_ID == ticket_id).FirstOrDefault();
                        _tickte_delete.Is_Active = false;
                        _tickte_delete.Is_Deleted = true;
                        _tickte_delete.Modified_Date = today;

                        DbContext.SaveChanges();
                    }
                    message = "1";
                }
                catch (Exception ex)
                {
                    message = "0";
                }
            }
            else
            {
                message = "0";

            }
            return Json(message, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult Bulk_Assign_Tickets(int[] Chk_tickets, int Teacher_name)
        {
            Menupermission();
            string message = string.Empty;
            if (Session["USER_CODE"] != null)
            {

                if (Chk_tickets != null && Teacher_name != 0)
                {
                    int teacher_ = 0;
                    try
                    {
                        var teacher_subject = DbContext.tbl_DC_Course_Teacher_Assign.Where(x => x.CourseAssn_ID == Teacher_name && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        for (int i = 0; i < Chk_tickets.Length; i++)
                        {
                            //teacher's subject id must be same with tickets's subject id
                            int teacher_subject_id = Convert.ToInt32(teacher_subject.Subject_Id);
                            int ticket_id = Chk_tickets[i];
                            var _ticket_details = DbContext.tbl_DC_Ticket.Where(x => x.Ticket_ID == ticket_id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                            int studentid = Convert.ToInt32(_ticket_details.Student_ID);
                            int ticket_subject_id = Convert.ToInt32(_ticket_details.Subject_ID);
                            var get_teacher = DbContext.tbl_DC_Course_Teacher_Assign.Where(x => x.CourseAssn_ID == Teacher_name).Select(x => x.Teacher_ID).FirstOrDefault();
                            int teacher = Convert.ToInt32(get_teacher);
                            var Teacher_mail_name = DbContext.tbl_DC_Teacher.Where(x => x.Teach_ID == teacher && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();

                            var student = DbContext.tbl_DC_Registration.Where(x => x.Regd_ID == studentid).FirstOrDefault();
                            int _insertedby = Convert.ToInt32(Convert.ToString(Session["USER_CODE"]));
                            if (teacher_subject_id == ticket_subject_id)
                            {
                                teacher_ = Convert.ToInt32(teacher_subject.Teacher_ID);
                                var _ticket_assign = DbContext.tbl_DC_Ticket_Assign.Where(x => x.Ticket_ID == ticket_id && x.Teach_ID == teacher_ && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                                if (_ticket_assign == null)
                                {
                                    tbl_DC_Ticket_Assign _ticket_Assign = new tbl_DC_Ticket_Assign();
                                    _ticket_Assign.Ticket_ID = ticket_id;
                                    _ticket_Assign.Ticket_No = _ticket_details.Ticket_No;
                                    _ticket_Assign.Student_ID = _ticket_details.Student_ID;
                                    _ticket_Assign.Teach_ID = teacher_;
                                    _ticket_Assign.Assign_Date = today;
                                    _ticket_Assign.Is_Close = false;
                                    _ticket_Assign.Inserted_By = _insertedby;
                                    _ticket_Assign.Inserted_Date = today;
                                    _ticket_Assign.Is_Active = true;
                                    _ticket_Assign.Is_Deleted = false;
                                    DbContext.tbl_DC_Ticket_Assign.Add(_ticket_Assign);
                                    tbl_DC_Ticket _ticket = DbContext.tbl_DC_Ticket.Where(x => x.Ticket_ID == ticket_id).FirstOrDefault();
                                    _ticket.Teach_ID = teacher_;
                                    _ticket.Assign_Date = today;
                                    DbContext.SaveChanges();
                                    if (Teacher_mail_name != null)
                                    {
                                        sendMail_ticketassign("Ticket_assign", Teacher_mail_name.Email_ID.ToString(), Teacher_mail_name.Teacher_Name.ToString(), _ticket_details.Ticket_No.ToString());
                                    }
                                    if (student != null)
                                    {
                                        sendmail_student("Student_assign", student.Email, student.Customer_Name, _ticket_details.Ticket_No.ToString(), Teacher_mail_name.Teacher_Name.ToString());
                                    }
                                    try
                                    {
                                        var pushnot = (from c in DbContext.tbl_DC_Registration.Where(x => x.Regd_ID == student.Regd_ID)

                                                       select new { c.Regd_ID, c.Device_id }).FirstOrDefault();
                                        string body = "tktid#{{tktid}}# Hello {{name}}, Your Question no- {{ticketno}} has been assigned to {{teacher}}. It will get resolved by your DigiGuru within 2 working days.";
                                        string msg = body.ToString().Replace("{{name}}", student.Customer_Name).Replace("{{ticketno}}", _ticket_details.Ticket_No).Replace("{{teacher}}", Teacher_mail_name.Teacher_Name).Replace("{{tktid}}", _ticket_details.Ticket_ID.ToString());
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
                                else
                                {
                                    message = "Teacher already assigned to a ticket.";
                                    return Json(message, JsonRequestBehavior.AllowGet);
                                }

                            }
                            else
                            {
                                message = "Teacher assigned subject must be same with ticket subject.";
                                return Json(message, JsonRequestBehavior.AllowGet);
                            }

                        }



                        message = "1";
                        return Json(message, JsonRequestBehavior.AllowGet);
                    }
                    catch (Exception ex)
                    {
                        message = "Something went wrong.";
                    }
                }
                else
                {
                    message = "Please provide data properly.";
                }

            }
            else
            {
                message = "3";
            }
            return Json(message, JsonRequestBehavior.AllowGet);

        }
        [HttpPost]
        public JsonResult tooltip_ticket_details(int id)
        {
            Menupermission();
            string[] msg = new string[4];
            if (id != 0)
            {
                try
                {
                    var _ticket_data = DbContext.View_DC_Tickets_and_Teacher.Where(x => x.Ticket_ID == id).FirstOrDefault();
                    if (_ticket_data != null)
                    {
                        msg[0] = _ticket_data.Question;
                        msg[1] = Convert.ToString(_ticket_data.Inserted_Date);
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
        public ActionResult reopenticket(int id)//close ticket
        {
            Menupermission();
            if (id != 0)
            {
                try
                {
                    int _insertedby = Convert.ToInt32(Convert.ToString(Session["USER_CODE"]));
                    tbl_DC_Ticket _reopen_ticket = DbContext.tbl_DC_Ticket.Where(x => x.Ticket_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    if (_reopen_ticket != null)
                    {
                        _reopen_ticket.Remark = null;
                        _reopen_ticket.Status = "O";
                        _reopen_ticket.Modified_By = _insertedby
                            ;
                        _reopen_ticket.Modified_Date = today;
                        DbContext.SaveChanges();
                        tbl_DC_Ticket_Assign _tbl_close = DbContext.tbl_DC_Ticket_Assign.Where(x => x.Ticket_ID == id).FirstOrDefault();
                        _tbl_close.Is_Close = false;
                        _tbl_close.Remark = null;
                        _tbl_close.Modified_By = _insertedby;
                        _tbl_close.Modified_Date = today;
                        DbContext.SaveChanges();
                        var get_student = DbContext.tbl_DC_Registration.Where(x => x.Regd_ID == _tbl_close.Student_ID).FirstOrDefault();
                        if (get_student.Email != null)
                        {
                            open_ticket_student("Ticket_open", get_student.Email.ToString(), get_student.Customer_Name, _tbl_close.Ticket_No.ToString(), "C");
                        }
                        TempData["SuccessMessage"] = "Ticket reopened successfully.";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Ticket not found.";
                    }

                }
                catch (Exception ex)
                {

                    TempData["ErrorMessage"] = "Somethimng went wrong.";
                }
            }
            return RedirectToAction("Ticket", "Admin");
        }
        public ActionResult reopen_ticket(int id)//reject
        {
            try
            {
                Menupermission();
                tbl_DC_Ticket _reopen_ticket = DbContext.tbl_DC_Ticket.Where(x => x.Ticket_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                if (_reopen_ticket != null)
                {
                    _reopen_ticket.Remark = null;
                    _reopen_ticket.Status = "O";
                    DbContext.SaveChanges();
                    var get_student = DbContext.tbl_DC_Registration.Where(x => x.Regd_ID == _reopen_ticket.Student_ID).FirstOrDefault();
                    if (get_student.Email != null)
                    {
                        open_ticket_student("Ticket_open", get_student.Email.ToString(), get_student.Customer_Name, _reopen_ticket.Ticket_No.ToString(), "R");
                    }
                    TempData["SuccessMessage"] = "Ticket reopened successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Ticket not found.";
                }

            }
            catch (Exception ex)
            {

                TempData["ErrorMessage"] = "Somethimng went wrong.";
            }
            return RedirectToAction("Ticket", "Admin");
        }

        public ActionResult Ticket_report(string f_Date, string t_Date)
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
                            var logstatus = (from c in DbContext.View_DC_Tickets_and_Teacher where c.Inserted_Date >= fdt && c.Inserted_Date <= tdt select c).ToList().OrderByDescending(x => x.Inserted_Date).OrderByDescending(x => x.Ticket_ID);
                            ViewBag.Ticket = logstatus.ToList();
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
            ViewBag.Breadcrumb = "view Tickets";
            return View("Ticket");
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

                TempData["ErrorMessage"] = "Something went wrong.";
            }
        }
        public bool sendMail_ticketassign(string parameter, string email, string name, string ticket_no)
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

        public bool sendmail_student(string parameter, string email, string name, string ticket_no, string tecahername)
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
            msgbody = getall.EmailConf_Body.ToString().Replace("{{name}}", name).Replace("{{ticketno}}", " " + ticket).Replace("{{teacher}}", tecahername).Replace("{{date}}", DateTime.Now.ToString());


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

        public bool open_ticket_student(string parameter, string email, string name, string ticket_no, string typ)
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
                if (typ == "R")
                {
                    msgbody = getall.EmailConf_Body.ToString().Replace("{{name}}", name).Replace("{{ticketno}}", ticket).Replace("{{tickettype}}", "rejected").Replace("{{date}}", DateTime.Now.ToString());
                }
                else if (typ == "C")
                {
                    msgbody = getall.EmailConf_Body.ToString().Replace("{{name}}", name).Replace("{{ticketno}}", ticket).Replace("{{tickettype}}", "closed").Replace("{{date}}", DateTime.Now.ToString());
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

        #endregion

        #region---------------------------------------------------Assign Course----------------------------------------------------------------
        [HttpGet]
        public ActionResult ViewAssignedCourse()
        {
            ViewBag.setting = "setting";
            Menupermission();
            if (Convert.ToString(Session["ROLE_CODE"]) == "S")
            {
                if (Session["Role_Id"] != null)
                {
                    int id = Convert.ToInt32(Session["Role_Id"]);
                    var p_chk = DbContext.tbl_DC_Menu_Permission.Where(x => x.Menu_Key_ID == 2 && x.ROLE_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    if (p_chk == null)
                    {
                        return RedirectToAction("checkpermission");
                    }
                }
            }
            ViewBag.Breadcrumb = "Assign Course";
            ViewBag.AssignCourse = DbContext.View_DC_CourseAssign.OrderByDescending(x => x.CourseAssn_ID).ToList();

            return View();
        }
        [HttpPost]
        public ActionResult TeacherAssignCourse(int[] Teacher_ID, int[] chk_course, int? id)
        {
            ViewBag.setting = "setting";
            Menupermission();
            if (Convert.ToString(Session["ROLE_CODE"]) == "S")
            {
                if (Session["Role_Id"] != null)
                {
                    int id1 = Convert.ToInt32(Session["Role_Id"]);
                    var p_chk = DbContext.tbl_DC_Menu_Permission.Where(x => x.Menu_Key_ID == 2 && x.ROLE_ID == id1 && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    if (p_chk == null)
                    {
                        return RedirectToAction("checkpermission");
                    }
                }
            }
            int Board_Id = 0;
            int Class_Id = 0;
            if (Teacher_ID.Length > 0)
            {
                if (chk_course.Length > 0)
                {
                    if (id == null)
                    {
                        for (int j = 0; j < Teacher_ID.Length; j++)
                        {

                            for (int i = 0; i < chk_course.Length; i++)
                            {
                                int techerid = Teacher_ID.ToList()[j];
                                int chkcousre = chk_course.ToList()[i];
                                var data1 = DbContext.tbl_DC_Course_Teacher_Assign.Where(x => x.Teacher_ID == techerid && x.Subject_Id == chkcousre && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                                if (data1 != null)
                                {
                                    TempData["WarningMessage"] = "Already assigend with same course.";
                                    return RedirectToAction("ViewAssignedCourse");
                                }
                                else
                                {
                                    int subid = Convert.ToInt32(chk_course[i].ToString());

                                    var data = (from a in DbContext.tbl_DC_Subject.Where(x => x.Subject_Id == subid && x.Is_Active == true && x.Is_Deleted == false) join b in DbContext.tbl_DC_Class.Where(x => x.Is_Active == true && x.Is_Deleted == false) on a.Class_Id equals b.Class_Id select new { a.Subject_Id, a.Class_Id, b.Board_Id }).FirstOrDefault();
                                    Board_Id = Convert.ToInt32(data.Board_Id);
                                    Class_Id = Convert.ToInt32(data.Class_Id);

                                    tbl_DC_Course_Teacher_Assign obj = new tbl_DC_Course_Teacher_Assign();
                                    obj.Board_Id = Board_Id;
                                    obj.Class_Id = Class_Id;
                                    obj.Subject_Id = subid;
                                    obj.Teacher_ID = techerid;
                                    obj.Inserted_Date = today;
                                    obj.Inserted_By = HttpContext.User.Identity.Name;
                                    obj.Is_Active = true;
                                    obj.Is_Deleted = false;
                                    DbContext.tbl_DC_Course_Teacher_Assign.Add(obj);
                                    DbContext.SaveChanges();
                                }
                            }
                        }

                        TempData["SuccessMessage"] = "Course assigned successfully.";
                        return RedirectToAction("ViewAssignedCourse");
                    }
                    else
                    {
                        if (Teacher_ID.Length == 1)
                        {
                            if (chk_course.Length == 1)
                            {
                                int tid = Teacher_ID[0];
                                var data = DbContext.tbl_DC_Course_Teacher_Assign.Where(x => x.Teacher_ID == tid && x.CourseAssn_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                                if (data != null)
                                {
                                    data.Subject_Id = chk_course[0];
                                    data.Modified_Date = today;
                                    data.Modified_By = HttpContext.User.Identity.Name;
                                    DbContext.Entry(data).State = EntityState.Modified;
                                    DbContext.SaveChanges();
                                    TempData["SuccessMessage"] = "Course updated successfully.";
                                }
                                else
                                {
                                    TempData["WarningMessage"] = "Invalid assign details.";
                                    return RedirectToAction("ViewAssignedCourse");
                                }
                            }
                            else
                            {
                                TempData["WarningMessage"] = "More than one course cannot be selected.";
                            }
                        }
                        else
                        {
                            TempData["WarningMessage"] = "More than one teacher cannot be selected.";
                        }

                    }
                }
                else
                {
                    TempData["WarningMessage"] = "Please select course to assign.";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Please assign to particular teacher.";
            }
            return RedirectToAction("ViewAssignedCourse");
        }
        [HttpGet]
        public ActionResult TeacherAssignCourse(int? id)
        {
            ViewBag.setting = "setting";
            Menupermission();
            ViewBag.Breadcrumb = "Assign Course";
            if (id != null)
            {
                ViewBag.teacherdata = DbContext.tbl_DC_Teacher.Where(x => x.Is_Active == true && x.Is_Deleted == false).ToList();
                ViewBag.valuechecked = "";
                var data = DbContext.tbl_DC_Course_Teacher_Assign.Where(x => x.CourseAssn_ID == id).FirstOrDefault();
                if (data != null)
                {
                    int T_id = Convert.ToInt32(data.Teacher_ID);
                    ViewBag.teachername = T_id;

                    var data1 = (from a in DbContext.tbl_DC_Board.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                 join b in DbContext.tbl_DC_Class.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                     on a.Board_Id equals b.Board_Id
                                 join c in DbContext.tbl_DC_Subject.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                     on b.Class_Id equals c.Class_Id
                                 select new DigiChampsModel.DigiChampsSubjectModel
                                 {
                                     Board_Id = a.Board_Id,
                                     Board_Name = a.Board_Name,
                                     Class_Id = b.Class_Id,
                                     Class_Name = b.Class_Name,
                                     Subject_Id = c.Subject_Id,
                                     Subject = c.Subject
                                 }).ToList();

                    ViewBag.subid = data.Subject_Id;
                    ViewBag.Assigned_Details = data1;
                    TempData["EditAssign"] = "(Please select at most one course to update)";
                }
                else
                {
                    TempData["ErrorMessage"] = "Invalid teacher assigned, try again.";
                }
            }
            else
            {
                try
                {
                    var data = (from a in DbContext.tbl_DC_Board.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                join b in DbContext.tbl_DC_Class.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                    on a.Board_Id equals b.Board_Id
                                join c in DbContext.tbl_DC_Subject.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                    on b.Class_Id equals c.Class_Id
                                select new DigiChampsModel.DigiChampsSubjectModel
                                {
                                    Board_Id = a.Board_Id,
                                    Board_Name = a.Board_Name,
                                    Class_Id = b.Class_Id,
                                    Class_Name = b.Class_Name,
                                    Subject_Id = c.Subject_Id,
                                    Subject = c.Subject

                                }).ToList();
                    if (data != null)
                    {
                        ViewBag.Assigned_Details = data;
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Invalid teacher assigned, try again.";
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Something went wrong.";
                }
            }

            ViewBag.teacherdata = DbContext.tbl_DC_Teacher.Where(x => x.Is_Active == true && x.Is_Deleted == false).ToList();
            return View("TeacherAssignCourse");
        }
        public ActionResult Delete_AssignTeacher(int? id)
        {
            ViewBag.setting = "setting";
            try
            {
                if (id != null)
                {
                    tbl_DC_Course_Teacher_Assign obj = DbContext.tbl_DC_Course_Teacher_Assign.Where(x => x.CourseAssn_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    obj.Is_Active = false;
                    obj.Is_Deleted = true;
                    obj.Modified_By = HttpContext.User.Identity.Name;
                    obj.Modified_Date = today;
                    DbContext.Entry(obj).State = EntityState.Modified;
                    DbContext.SaveChanges();
                    TempData["SuccessMessage"] = "Course deleted successfully.";
                    return RedirectToAction("ViewAssignedCourse");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return View();
        }
        #endregion

        #region------------------------------------------------------Topic---------------------------------------------------------------------
        [HttpGet]
        public ActionResult Topics()
        {
            ViewBag.setting = "setting";
            try
            {
                Menupermission();
                if (Convert.ToString(Session["ROLE_CODE"]) == "S")
                {
                    if (Session["Role_Id"] != null)
                    {
                        int id = Convert.ToInt32(Session["Role_Id"]);
                        var p_chk = DbContext.tbl_DC_Menu_Permission.Where(x => x.Menu_Key_ID == 1 && x.ROLE_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        if (p_chk == null)
                        {
                            return RedirectToAction("checkpermission");
                        }
                    }
                }
                ViewBag.Breadcrumb = "Topic";
                ViewBag.board = new SelectList(DbContext.tbl_DC_Board.Where(b => b.Is_Active == true && b.Is_Deleted == false), "Board_Id", "Board_Name");

                var data1 = (from a in DbContext.tbl_DC_Topic.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                             join g in DbContext.tbl_DC_Chapter.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                            on a.Chapter_Id equals g.Chapter_Id
                             join b in DbContext.tbl_DC_Subject.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                            on a.Subject_ID equals b.Subject_Id
                             join c in DbContext.tbl_DC_Class.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                             on b.Class_Id equals c.Class_Id
                             join d in DbContext.tbl_DC_Board.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                             on c.Board_Id equals d.Board_Id
                             select new DigiChampsModel.DigiChampsTopicModel
                             {
                                 Board_ID = d.Board_Id,
                                 Board_Name = d.Board_Name,
                                 Class_ID = c.Class_Id,
                                 Class_Name = c.Class_Name,
                                 Subject = b.Subject,
                                 Subject_ID = b.Subject_Id,
                                 chapter_Name = g.Chapter,
                                 Chapter_ID = g.Chapter_Id,
                                 Topic_Name = a.Topic_Name,
                                 Topic_ID = a.Topic_ID
                             }).OrderByDescending(x => x.Topic_ID).ToList();

                ViewBag.Topicdata = data1.ToList();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }

            return View();
        }

        [HttpGet]
        public ActionResult AddNewTopic(int? id, string Board_ID, string Class_ID, string Subject_ID, string Topic_Name, string Topic_Desc)
        {
            ViewBag.setting = "setting";
            try
            {
                if (Convert.ToString(Session["ROLE_CODE"]) == "S")
                {
                    if (Session["Role_Id"] != null)
                    {
                        int id1 = Convert.ToInt32(Session["Role_Id"]);
                        var p_chk = DbContext.tbl_DC_Menu_Permission.Where(x => x.Menu_Key_ID == 1 && x.ROLE_ID == id1 && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        if (p_chk == null)
                        {
                            return RedirectToAction("checkpermission");
                        }
                    }
                }
                ViewBag.Breadcrumb = "Topic";
                ViewBag.pagetitle = "Add";
                if (id != null)
                {
                    DigiChampsModel.DigiChampsTopicModel obj = (from b in DbContext.tbl_DC_Board.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                                                join c in DbContext.tbl_DC_Class.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                                                    on b.Board_Id equals c.Board_Id
                                                                join d in DbContext.tbl_DC_Subject.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                                                    on c.Class_Id equals d.Class_Id
                                                                join e in DbContext.tbl_DC_Chapter.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                                                on d.Subject_Id equals e.Subject_Id
                                                                join a in DbContext.tbl_DC_Topic.Where(x => x.Topic_ID == id && x.Is_Active == true && x.Is_Deleted == false) on d.Subject_Id equals a.Subject_ID
                                                                select new DigiChampsModel.DigiChampsTopicModel
                                                                {
                                                                    Board_ID = b.Board_Id,
                                                                    Board_Name = b.Board_Name,
                                                                    Class_ID = c.Class_Id,
                                                                    Class_Name = c.Class_Name,
                                                                    Subject_ID = d.Subject_Id,
                                                                    Subject = d.Subject,
                                                                    Chapter_ID = e.Chapter_Id,
                                                                    chapter_Name = e.Chapter,
                                                                    Topic_Name = a.Topic_Name,
                                                                    Topic_ID = a.Topic_ID,
                                                                    Topic_Desc = a.Topic_Desc
                                                                }).FirstOrDefault();
                    ViewBag.board = new SelectList(DbContext.tbl_DC_Board.Where(b => b.Is_Active == true && b.Is_Deleted == false), "Board_Id", "Board_Name", obj.Board_ID);
                    if (obj != null)
                    {
                        ViewBag.topicid = obj.Topic_ID;
                        ViewBag.classid = obj.Class_ID;
                        ViewBag.subid = obj.Subject_ID;
                        ViewBag.bid_hid = obj.Board_ID;
                        ViewBag.chapterid = obj.Chapter_ID;
                        ViewBag.tbod = obj.Board_ID;
                        ViewBag.topicnaem = obj.Topic_Name;
                        return View(obj);
                    }
                    else
                    {
                        TempData["WarningMessage"] = "Invalid topic details.";
                        return RedirectToAction("Topics");
                    }
                }
                else
                {
                    ViewBag.board = new SelectList(DbContext.tbl_DC_Board.Where(b => b.Is_Active == true && b.Is_Deleted == false), "Board_Id", "Board_Name");
                    ViewBag.Topic_Boradid = Board_ID;
                    ViewBag.topicname = Topic_Name;
                    ViewBag.classid = Class_ID;
                    ViewBag.subid = Subject_ID;
                }

            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return View();
        }
        [HttpPost]
        public ActionResult AddNewTopic(DigiChampsModel.DigiChampsTopicModel obj, string topicid)
        {
            ViewBag.setting = "setting";
            try
            {
                Menupermission();
                ViewBag.pagetitle = "Update";
                ViewBag.board = new SelectList(DbContext.tbl_DC_Board.Where(b => b.Is_Active == true && b.Is_Deleted == false), "Board_Id", "Board_Name", obj.Board_ID);
                if (ModelState.IsValid)
                {
                    if (obj.Topic_Name.Trim() != null)
                    {
                        if (topicid != "")
                        {
                            int tid = Convert.ToInt32(topicid);
                            tbl_DC_Topic obj1 = DbContext.tbl_DC_Topic.Where(x => x.Topic_ID == tid && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                            if (obj1 != null)
                            {
                                obj1.Board_ID = obj.Board_ID;
                                obj1.Class_ID = obj.Class_ID;
                                obj1.Subject_ID = obj.Subject_ID;
                                obj1.Topic_Name = obj.Topic_Name;
                                obj1.Topic_Desc = obj.Topic_Desc;
                                obj1.Chapter_Id = obj.Chapter_ID;
                                obj1.Modified_By = HttpContext.User.Identity.Name;
                                obj1.Modified_Date = today;
                                DbContext.SaveChanges();
                                TempData["SuccessMessage"] = "Topic updated successfully.";
                                return RedirectToAction("Topics");
                            }
                            else
                            {
                                TempData["ErrorMessage"] = "Invalid topic details.";
                                return RedirectToAction("AddNewTopic", "Admin", new { Board_ID = obj.Board_ID, Class_ID = obj.Class_ID, Subject_ID = obj.Subject_ID, Topic_Name = obj.Topic_Name, Topic_Desc = obj.Topic_Desc });
                            }
                        }
                        else
                        {

                            try
                            {
                                var topic = DbContext.tbl_DC_Topic.Where(x => x.Topic_Name == obj.Topic_Name && x.Board_ID == obj.Board_ID && x.Subject_ID == obj.Subject_ID && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                                if (topic == null)
                                {
                                    tbl_DC_Topic obj1 = new tbl_DC_Topic();
                                    obj1.Topic_ID = Convert.ToInt32(obj.Topic_ID);
                                    obj1.Board_ID = obj.Board_ID;
                                    obj1.Class_ID = obj.Class_ID;
                                    obj1.Subject_ID = obj.Subject_ID;
                                    obj1.Chapter_Id = obj.Chapter_ID;
                                    obj1.Topic_Name = obj.Topic_Name;
                                    obj1.Topic_Desc = obj.Topic_Desc;
                                    obj1.Is_Active = true;
                                    obj1.Is_Deleted = false;
                                    obj1.Inserted_By = HttpContext.User.Identity.Name;
                                    obj1.Inserted_Date = today;
                                    DbContext.tbl_DC_Topic.Add(obj1);
                                    DbContext.SaveChanges();
                                    TempData["SuccessMessage"] = "Topic added successfully.";
                                    return RedirectToAction("Topics");
                                }
                                else
                                {
                                    TempData["WarningMessage"] = "Topic name already exists.";
                                    return RedirectToAction("AddNewTopic", "Admin", new { Board_ID = obj.Board_ID, Class_ID = obj.Class_ID, Subject_ID = obj.Subject_ID, Topic_Name = obj.Topic_Name, Topic_Desc = obj.Topic_Desc });
                                }
                            }
                            catch (Exception ex)
                            {
                                TempData["ErrorMessage"] = "Something went wrong.";
                                return RedirectToAction("AddNewTopic", "Admin", new { Board_ID = obj.Board_ID, Class_ID = obj.Class_ID, Subject_ID = obj.Subject_ID, Topic_Name = obj.Topic_Name, Topic_Desc = obj.Topic_Desc });
                            }
                        }
                    }
                    else
                    {
                        TempData["WarningMessage"] = "Please enter topic name.";
                        return RedirectToAction("AddNewTopic", "Admin", new { Board_ID = obj.Board_ID, Class_ID = obj.Class_ID, Subject_ID = obj.Subject_ID, Topic_Name = obj.Topic_Name, Topic_Desc = obj.Topic_Desc });
                    }
                }
                else
                {
                    TempData["WarningMessage"] = "Please enter data properly.";
                    return RedirectToAction("AddNewTopic", "Admin", new { Board_ID = obj.Board_ID, Class_ID = obj.Class_ID, Subject_ID = obj.Subject_ID, Topic_Name = obj.Topic_Name, Topic_Desc = obj.Topic_Desc });
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return RedirectToAction("AddNewTopic");
        }

        public ActionResult DeleteTopic(int? id)
        {
            ViewBag.setting = "setting";
            try
            {
                if (id != null)
                {
                    var topic_found = DbContext.tbl_DC_Question.Where(x => x.Topic_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    if (topic_found == null)
                    {
                        tbl_DC_Topic obj = DbContext.tbl_DC_Topic.Where(x => x.Topic_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        obj.Is_Active = false;
                        obj.Is_Deleted = true;
                        obj.Modified_By = HttpContext.User.Identity.Name;
                        obj.Modified_Date = today;
                        DbContext.Entry(obj).State = EntityState.Modified;
                        DbContext.SaveChanges();
                        TempData["SuccessMessage"] = "Topic deleted successfully.";
                        return RedirectToAction("Topics");
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Topic can not be deleted because its in use";
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "No data Found";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong";
            }
            return RedirectToAction("Topics");
        }
        #endregion

        #region -----------------------------------------------------Question ------------------------------------------------------------------
        [HttpGet]
        public ActionResult QuestionBank()
        {
            try
            {
                Menupermission();
                if (Convert.ToString(Session["ROLE_CODE"]) == "S")
                {
                    if (Session["Role_Id"] != null)
                    {
                        int id = Convert.ToInt32(Session["Role_Id"]);
                        var p_chk = DbContext.tbl_DC_Menu_Permission.Where(x => x.Menu_Key_ID == 6 && x.ROLE_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        if (p_chk == null)
                        {
                            return RedirectToAction("checkpermission");
                        }
                    }
                }
                ViewBag.Breadcrumb = "Question";
                ViewBag.subjlist = (from a in DbContext.tbl_DC_Question.Where(x => x.Is_Active == true && x.Is_Deleted == false).Select(a => a.Subject_Id).Distinct()
                                    join b in DbContext.tbl_DC_Subject.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                    on a equals b.Subject_Id
                                    select new DigiChampsModel.DigiChampsSubjectModel
                                    {
                                        Class_Id = b.Class_Id,
                                        Subject = b.Subject,
                                        Subject_Id = (int)a
                                    }).ToList();

                ViewBag.Question = DbContext.tbl_DC_Question.Where(x => x.Is_Active == true && x.Is_Deleted == false).OrderByDescending(x => x.Question_ID).Take(200).ToList();

                var Count_Que = DbContext.SP_DC_Question_Count(0).ToList();
                ViewBag.Count_Que = Count_Que.ToList();
                return View();
            }
            catch (Exception)
            {
                throw;
            }
        }
        [HttpPost]
        public ActionResult QuestionBank(int? ddlchapter)
        {
            try
            {
                ViewBag.Breadcrumb = "Question";
                ViewBag.subjlist = (from a in DbContext.tbl_DC_Question.Where(x => x.Is_Active == true && x.Is_Deleted == false).Select(a => a.Subject_Id).Distinct()
                                    join b in DbContext.tbl_DC_Subject.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                    on a equals b.Subject_Id
                                    select new DigiChampsModel.DigiChampsSubjectModel
                                    {
                                        Class_Id = b.Class_Id,
                                        Subject = b.Subject,
                                        Subject_Id = (int)a
                                    }).ToList();
                if (ddlchapter != null)
                {
                    ViewBag.Question = DbContext.tbl_DC_Question.Where(x => x.Chapter_Id == ddlchapter && x.Is_Active == true && x.Is_Deleted == false).OrderByDescending(x => x.Question_ID).Take(200).ToList();
                }
                else
                {
                    ViewBag.Question = DbContext.tbl_DC_Question.Where(x => x.Is_Active == true && x.Is_Deleted == false).OrderByDescending(x => x.Question_ID).Take(200).ToList();
                }
                var Count_Que = DbContext.SP_DC_Question_Count(0).ToList();
                ViewBag.Count_Que = Count_Que.ToList();
                return View("QuestionBank");
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpGet]
        public ActionResult Add_New_Question()
        {
            Menupermission();
            if (Convert.ToString(Session["ROLE_CODE"]) == "S")
            {
                if (Session["Role_Id"] != null)
                {
                    int id = Convert.ToInt32(Session["Role_Id"]);
                    var p_chk = DbContext.tbl_DC_Menu_Permission.Where(x => x.Menu_Key_ID == 6 && x.ROLE_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    if (p_chk == null)
                    {
                        return RedirectToAction("checkpermission");
                    }
                }
            }
            ViewBag.Breadcrumb = "Question";
            ViewBag.Board_details = DbContext.tbl_DC_Board.Where(x => x.Is_Active == true && x.Is_Deleted == false).ToList();
            ViewBag.Power_Id = DbContext.tbl_DC_Power_Question.Where(x => x.Is_Active == true && x.Is_Deleted == false).ToList();
            return View();
        }
        public ActionResult Edit_Question(int? id)
        {
            try
            {
                Menupermission();
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
                        ViewBag.freetest = d.Is_Free;
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
        public ActionResult View_Question(int? Qid, int?[] ans_id, int?[] qimg_id, string ddlboard, string ddlclass, string ddlsubject, string ddlchapter, string ddltopic, string ddlpower, string Quest_, string[] Qphotos, string Quest_desc, string Practice, string Pre_requisite, string Test, string Global, string online, string freetest, string[] answernew, HttpPostedFileBase[] optn_img, string[] des_text, HttpPostedFileBase[] ans_img, string[] chk_ans)
        {
            Menupermission();
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
                                    if (freetest == "1")
                                    {
                                        Qsn.Is_Free = true;
                                    }
                                    else
                                    {
                                        Qsn.Is_Free = false;
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
                                    if (freetest == "1")
                                    {
                                        qst.Is_Free = true;
                                    }
                                    else
                                    {
                                        qst.Is_Free = false;
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

        [HttpPost]
        public JsonResult Dlt_selectQuestions(int[] qstdlt)
        {
            string msg = "";
            try
            {
                if (qstdlt.Length > 0)
                {
                    foreach (int qd in qstdlt)
                    {
                        var questionfound = DbContext.tbl_DC_Exam_Result_Dtl.Where(x => x.Question_ID == qd && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        if (questionfound == null)
                        {
                            tbl_DC_Question dq = DbContext.tbl_DC_Question.Where(x => x.Question_ID == qd).FirstOrDefault();
                            dq.Is_Active = false;
                            dq.Is_Deleted = true;
                            dq.Modified_By = HttpContext.User.Identity.Name;
                            dq.Modified_Date = today;
                            DbContext.Entry(dq).State = EntityState.Modified;
                            DbContext.SaveChanges();

                            var qans = DbContext.tbl_DC_Question_Answer.Where(x => x.Question_ID == qd);
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
                            var qanimg = DbContext.tbl_DC_Question_Images.Where(x => x.Question_ID == qd);
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
                            msg = "1";
                        }
                    }
                }
                else
                {
                    msg = "3";
                }
            }
            catch
            {
                msg = "2";
            }
            return Json(msg);
        }

        public ActionResult Delete_Question(int? id)
        {
            try
            {
                if (id != null)
                {
                    var questionfound = DbContext.tbl_DC_Exam_Result_Dtl.Where(x => x.Question_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    if (questionfound == null)
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
                    else
                    {
                        TempData["ErrorMessage"] = "Question can not be deleted because it is in use.";
                    }

                }
                else
                {
                    TempData["ErrorMessage"] = "No question found.";
                }
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return RedirectToAction("QuestionBank", "Admin");
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

        //Question Bulk Upload
        [HttpPost]
        public ActionResult UploadQuestion(HttpPostedFileBase qstnupld, IEnumerable<HttpPostedFileBase> imagefiles)
        {
            try
            {

                if (qstnupld != null)
                {
                    string filename = qstnupld.FileName;
                    string extension = System.IO.Path.GetExtension(qstnupld.FileName);
                    //Save file upload file in to server path for temporary
                    if ((extension == ".xls") | (extension == ".xlsx"))
                    {

                        var path = Path.Combine(Server.MapPath("~/Content/Upload/"), filename);
                        qstnupld.SaveAs(path);
                        //Export excel data into Gridview using below method
                        if (ExportToGrid(path, imagefiles))
                        {
                            TempData["SuccessMessage"] = "Excel sheet exported successfully.";
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "Something went wrong, try again.";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;

            }
            return RedirectToAction("QuestionBank");
        }

        public bool ExportToGrid(String path, IEnumerable<HttpPostedFileBase> fileimages)
        {
            try
            {
                string ext = string.Empty;
                string[] strs = path.Split('.');
                foreach (string str in strs)
                {
                    ext = str;
                }
                DataTable dt = new DataTable();
                OleDbConnection MyConnection = null;
                DataSet DtSet = null;
                OleDbDataAdapter MyCommand = null;
                if (ext == "xls")
                {
                    //Connection for MS Excel 2003 .xls format
                    MyConnection = new OleDbConnection("provider=Microsoft.Jet.OLEDB.4.0; Data Source='" + path + "';Extended Properties=Excel 8.0;");
                }
                if (ext == "xlsx")
                {
                    //Connection for .xslx 2007 format
                    MyConnection = new OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0;Data Source='" + path + "';Extended Properties=Excel 12.0;");
                }
                //Select your Excel file
                MyCommand = new System.Data.OleDb.OleDbDataAdapter("select * from [Sheet1$]", MyConnection);
                MyConnection.Open();
                DtSet = new System.Data.DataSet();
                //Bind all excel data in to data set
                MyCommand.Fill(DtSet, "[Sheet1$]");
                dt = DtSet.Tables[0];
                MyConnection.Close();
                tbl_DC_Question tblobj = new tbl_DC_Question();
                tbl_DC_Question_Images qimg = new tbl_DC_Question_Images();
                tbl_DC_Question_Answer qst = new tbl_DC_Question_Answer();

                foreach (DataRow dr in dt.Rows)
                {
                    int count = dt.Rows.Count;
                    if (!string.IsNullOrEmpty(dr[0].ToString().Trim()) && !string.IsNullOrEmpty(dr[1].ToString().Trim()))
                    {
                        string board = dr[0].ToString();
                        var bid = DbContext.tbl_DC_Board.Where(x => x.Board_Name == board && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        tblobj.Board_Id = bid.Board_Id;

                        string classnm = dr[1].ToString();
                        var cid = DbContext.tbl_DC_Class.Where(x => x.Class_Name == classnm && x.Board_Id == tblobj.Board_Id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        tblobj.Class_Id = cid.Class_Id;
                        if (!string.IsNullOrEmpty(dr[2].ToString().Trim()))
                        {
                            string subname = dr[2].ToString();
                            var sid = DbContext.tbl_DC_Subject.Where(x => x.Subject == subname && x.Class_Id == tblobj.Class_Id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                            tblobj.Subject_Id = sid.Subject_Id;

                            if (!string.IsNullOrEmpty(dr[3].ToString().Trim()))
                            {
                                string chapt = dr[3].ToString();
                                var chid = DbContext.tbl_DC_Chapter.Where(x => x.Chapter == chapt && x.Subject_Id == tblobj.Subject_Id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                                tblobj.Chapter_Id = chid.Chapter_Id;

                                if (!string.IsNullOrEmpty(dr[4].ToString().Trim()))
                                {
                                    string topic = dr[4].ToString();
                                    var tid = DbContext.tbl_DC_Topic.Where(x => x.Topic_Name == topic && x.Subject_ID == tblobj.Subject_Id && x.Class_ID == tblobj.Class_Id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                                    tblobj.Topic_ID = tid.Topic_ID;
                                    if (!string.IsNullOrEmpty(dr[5].ToString().Trim()))
                                    {
                                        string pwr = dr[5].ToString();
                                        var prid = DbContext.tbl_DC_Power_Question.Where(x => x.Power_Type == pwr && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                                        tblobj.Power_ID = prid.Power_Id;
                                    }
                                    if (!string.IsNullOrEmpty(dr[6].ToString().Trim()))
                                    {
                                        string qstn = dr[6].ToString();
                                        tblobj.Question = qstn;
                                        if (!string.IsNullOrEmpty(dr[7].ToString().Trim()))
                                        {
                                            string qtype = dr[7].ToString();

                                            string[] strng = qtype.Split(',');
                                            foreach (string st in strng)
                                            {
                                                if (st.Trim() == "Online")
                                                {
                                                    tblobj.Is_online = true;
                                                }
                                                else if (st.Trim() == "Practice")
                                                {
                                                    tblobj.Is_Practice = true;
                                                }
                                                else if (st.Trim() == "Pre-requisite")
                                                {
                                                    tblobj.Is_PreRequisite = true;
                                                }
                                                else if (st.Trim() == "Re-Test")
                                                {
                                                    tblobj.Is_Test = true;
                                                }
                                                else if (st.Trim() == "Global")
                                                {
                                                    tblobj.Is_Global = true;
                                                }
                                                else if (st.Trim() == "Free-Test")
                                                {
                                                    tblobj.Is_Free = true;
                                                }
                                            }
                                        }
                                        if (!string.IsNullOrEmpty(dr[8].ToString().Trim()))
                                        {
                                            tblobj.Qustion_Desc = dr[8].ToString();
                                        }


                                        if (!string.IsNullOrEmpty(dr[9].ToString().Trim()))
                                        {
                                            string imgname = dr[9].ToString();

                                            string date1 = Convert.ToString(DateTime.Now.Ticks);
                                            string fileExtension = Path.GetExtension(imgname);
                                            string pathimage = imgname;
                                            foreach (var file in fileimages)
                                            {
                                                if (file != null && file.ContentLength > 0)
                                                {
                                                    if (imgname == file.FileName)
                                                    {
                                                        var filename = Path.GetFileName(file.FileName.Replace(file.FileName.Split('.').FirstOrDefault().ToString(), date1));
                                                        var filepath = Path.Combine(Server.MapPath("~/Content/Qusetion/"), filename);
                                                        file.SaveAs(filepath);
                                                        qimg.Question_desc_Image = filename;
                                                        break;
                                                    }
                                                }
                                            }
                                            qimg.Question_ID = tblobj.Question_ID;
                                            qimg.Is_Active = true;
                                            qimg.Is_Deleted = false;
                                            DbContext.tbl_DC_Question_Images.Add(qimg);
                                            DbContext.SaveChanges();
                                        }
                                        tblobj.Is_Active = true;
                                        tblobj.Is_Deleted = false;
                                        DbContext.tbl_DC_Question.Add(tblobj);
                                        DbContext.SaveChanges();

                                        #region Options

                                        //first option
                                        if (!string.IsNullOrEmpty(dr[10].ToString().Trim()) || !string.IsNullOrEmpty(dr[11].ToString().Trim()))
                                        {
                                            if (!string.IsNullOrEmpty(dr[10].ToString().Trim()))
                                            {
                                                qst.Option_Desc = dr[10].ToString();
                                            }
                                            if (!string.IsNullOrEmpty(dr[11].ToString().Trim()))
                                            {
                                                string imgname = dr[11].ToString();

                                                string date1 = Convert.ToString(DateTime.Now.Ticks);
                                                string fileExtension = Path.GetExtension(imgname);
                                                string pathimage = imgname;
                                                foreach (var file in fileimages)
                                                {
                                                    if (file != null && file.ContentLength > 0)
                                                    {
                                                        if (imgname.ToUpper() == file.FileName.ToUpper())
                                                        {
                                                            var filename = Path.GetFileName(file.FileName.Replace(file.FileName.Split('.').FirstOrDefault().ToString(), date1));
                                                            var filepath = Path.Combine(Server.MapPath("~/Content/Qusetion/"), filename);
                                                            file.SaveAs(filepath);
                                                            qst.Option_Image = filename;
                                                            break;
                                                        }
                                                    }
                                                }
                                            }
                                            if (!string.IsNullOrEmpty(dr[14].ToString().Trim()))
                                            {
                                                string isans = dr[14].ToString();
                                                if (isans.ToUpper() == "TRUE")
                                                {
                                                    qst.Is_Answer = true;
                                                    if (!string.IsNullOrEmpty(dr[12].ToString().Trim()))
                                                    {
                                                        qst.Option_Desc = dr[12].ToString();
                                                    }
                                                    if (!string.IsNullOrEmpty(dr[13].ToString().Trim()))
                                                    {
                                                        string imgname = dr[13].ToString();

                                                        string date1 = Convert.ToString(DateTime.Now.Ticks);
                                                        string fileExtension = Path.GetExtension(imgname);
                                                        string pathimage = imgname;
                                                        foreach (var file in fileimages)
                                                        {
                                                            if (file != null && file.ContentLength > 0)
                                                            {
                                                                if (imgname.ToUpper() == file.FileName.ToUpper())
                                                                {
                                                                    var filename = Path.GetFileName(file.FileName.Replace(file.FileName.Split('.').FirstOrDefault().ToString(), date1));
                                                                    var filepath = Path.Combine(Server.MapPath("~/Content/Qusetion/"), filename);
                                                                    file.SaveAs(filepath);
                                                                    qst.Option_Image = filename;
                                                                    break;
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                                else if (isans.ToUpper() == "FALSE")
                                                {
                                                    qst.Is_Answer = false;
                                                }
                                            }
                                            else
                                            {
                                                qst.Is_Answer = false;
                                            }
                                            qst.Question_ID = tblobj.Question_ID;
                                            qst.Is_Active = true;
                                            qst.Is_Deleted = false;
                                            DbContext.tbl_DC_Question_Answer.Add(qst);
                                            DbContext.SaveChanges();
                                        }

                                        //second option
                                        if (!string.IsNullOrEmpty(dr[15].ToString().Trim()) || !string.IsNullOrEmpty(dr[16].ToString().Trim()))
                                        {
                                            if (!string.IsNullOrEmpty(dr[15].ToString().Trim()))
                                            {
                                                qst.Option_Desc = dr[15].ToString();
                                            }
                                            if (!string.IsNullOrEmpty(dr[16].ToString().Trim()))
                                            {
                                                string imgname = dr[16].ToString();

                                                string date1 = Convert.ToString(DateTime.Now.Ticks);
                                                string fileExtension = Path.GetExtension(imgname);
                                                string pathimage = imgname;
                                                foreach (var file in fileimages)
                                                {
                                                    if (file != null && file.ContentLength > 0)
                                                    {
                                                        if (imgname.ToUpper() == file.FileName.ToUpper())
                                                        {
                                                            var filename = Path.GetFileName(file.FileName.Replace(file.FileName.Split('.').FirstOrDefault().ToString(), date1));
                                                            var filepath = Path.Combine(Server.MapPath("~/Content/Qusetion/"), filename);
                                                            file.SaveAs(filepath);
                                                            qst.Option_Image = filename;
                                                            break;
                                                        }
                                                    }
                                                }
                                            }
                                            if (!string.IsNullOrEmpty(dr[19].ToString().Trim()))
                                            {
                                                string isans = dr[19].ToString();
                                                if (isans.ToUpper() == "TRUE")
                                                {
                                                    qst.Is_Answer = true;
                                                    if (!string.IsNullOrEmpty(dr[17].ToString().Trim()))
                                                    {
                                                        qst.Option_Desc = dr[17].ToString();
                                                    }
                                                    if (!string.IsNullOrEmpty(dr[18].ToString().Trim()))
                                                    {
                                                        string imgname = dr[18].ToString();

                                                        string date1 = Convert.ToString(DateTime.Now.Ticks);
                                                        string fileExtension = Path.GetExtension(imgname);
                                                        string pathimage = imgname;
                                                        foreach (var file in fileimages)
                                                        {
                                                            if (file != null && file.ContentLength > 0)
                                                            {
                                                                if (imgname.ToUpper() == file.FileName.ToUpper())
                                                                {
                                                                    var filename = Path.GetFileName(file.FileName.Replace(file.FileName.Split('.').FirstOrDefault().ToString(), date1));
                                                                    var filepath = Path.Combine(Server.MapPath("~/Content/Qusetion/"), filename);
                                                                    file.SaveAs(filepath);
                                                                    qst.Option_Image = filename;
                                                                    break;
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                                else if (isans.ToUpper() == "FALSE")
                                                {
                                                    qst.Is_Answer = false;
                                                }
                                            }
                                            else
                                            {
                                                qst.Is_Answer = false;
                                            }
                                            qst.Question_ID = tblobj.Question_ID;
                                            qst.Is_Active = true;
                                            qst.Is_Deleted = false;
                                            DbContext.tbl_DC_Question_Answer.Add(qst);
                                            DbContext.SaveChanges();
                                        }

                                        //third option
                                        if (!string.IsNullOrEmpty(dr[20].ToString().Trim()) || !string.IsNullOrEmpty(dr[21].ToString().Trim()))
                                        {
                                            if (!string.IsNullOrEmpty(dr[20].ToString().Trim()))
                                            {
                                                qst.Option_Desc = dr[20].ToString();
                                            }
                                            if (!string.IsNullOrEmpty(dr[21].ToString().Trim()))
                                            {
                                                string imgname = dr[21].ToString();

                                                string date1 = Convert.ToString(DateTime.Now.Ticks);
                                                string fileExtension = Path.GetExtension(imgname);
                                                string pathimage = imgname;
                                                foreach (var file in fileimages)
                                                {
                                                    if (file != null && file.ContentLength > 0)
                                                    {
                                                        if (imgname.ToUpper() == file.FileName.ToUpper())
                                                        {
                                                            var filename = Path.GetFileName(file.FileName.Replace(file.FileName.Split('.').FirstOrDefault().ToString(), date1));
                                                            var filepath = Path.Combine(Server.MapPath("~/Content/Qusetion/"), filename);
                                                            file.SaveAs(filepath);
                                                            qst.Option_Image = filename;
                                                            break;
                                                        }
                                                    }
                                                }
                                            }
                                            if (!string.IsNullOrEmpty(dr[24].ToString().Trim()))
                                            {
                                                string isans = dr[24].ToString();
                                                if (isans.ToUpper() == "TRUE")
                                                {
                                                    qst.Is_Answer = true;
                                                    if (!string.IsNullOrEmpty(dr[22].ToString().Trim()))
                                                    {
                                                        qst.Option_Desc = dr[22].ToString();
                                                    }
                                                    if (!string.IsNullOrEmpty(dr[23].ToString().Trim()))
                                                    {
                                                        string imgname = dr[23].ToString();

                                                        string date1 = Convert.ToString(DateTime.Now.Ticks);
                                                        string fileExtension = Path.GetExtension(imgname);
                                                        string pathimage = imgname;
                                                        foreach (var file in fileimages)
                                                        {
                                                            if (file != null && file.ContentLength > 0)
                                                            {
                                                                if (imgname.ToUpper() == file.FileName.ToUpper())
                                                                {
                                                                    var filename = Path.GetFileName(file.FileName.Replace(file.FileName.Split('.').FirstOrDefault().ToString(), date1));
                                                                    var filepath = Path.Combine(Server.MapPath("~/Content/Qusetion/"), filename);
                                                                    file.SaveAs(filepath);
                                                                    qst.Option_Image = filename;
                                                                    break;
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                                else if (isans.ToUpper() == "FALSE")
                                                {
                                                    qst.Is_Answer = false;
                                                }
                                            }
                                            else
                                            {
                                                qst.Is_Answer = false;
                                            }
                                            qst.Question_ID = tblobj.Question_ID;
                                            qst.Is_Active = true;
                                            qst.Is_Deleted = false;
                                            DbContext.tbl_DC_Question_Answer.Add(qst);
                                            DbContext.SaveChanges();
                                        }
                                        else
                                        {
                                            qst.Is_Answer = false;
                                        }

                                        //forth option
                                        if (!string.IsNullOrEmpty(dr[25].ToString().Trim()) || !string.IsNullOrEmpty(dr[26].ToString().Trim()))
                                        {
                                            if (!string.IsNullOrEmpty(dr[25].ToString().Trim()))
                                            {
                                                qst.Option_Desc = dr[25].ToString();
                                            }
                                            if (!string.IsNullOrEmpty(dr[26].ToString().Trim()))
                                            {
                                                string imgname = dr[26].ToString();

                                                string date1 = Convert.ToString(DateTime.Now.Ticks);
                                                string fileExtension = Path.GetExtension(imgname);
                                                string pathimage = imgname;
                                                foreach (var file in fileimages)
                                                {
                                                    if (file != null && file.ContentLength > 0)
                                                    {
                                                        if (imgname.ToUpper() == file.FileName.ToUpper())
                                                        {
                                                            var filename = Path.GetFileName(file.FileName.Replace(file.FileName.Split('.').FirstOrDefault().ToString(), date1));
                                                            var filepath = Path.Combine(Server.MapPath("~/Content/Qusetion/"), filename);
                                                            file.SaveAs(filepath);
                                                            qst.Option_Image = filename;
                                                            break;
                                                        }
                                                    }
                                                }
                                            }
                                            if (!string.IsNullOrEmpty(dr[29].ToString().Trim()))
                                            {
                                                string isans = dr[29].ToString();
                                                if (isans.ToUpper() == "TRUE")
                                                {
                                                    qst.Is_Answer = true;
                                                    if (!string.IsNullOrEmpty(dr[27].ToString().Trim()))
                                                    {
                                                        qst.Option_Desc = dr[27].ToString();
                                                    }
                                                    if (!string.IsNullOrEmpty(dr[28].ToString().Trim()))
                                                    {
                                                        string imgname = dr[28].ToString();

                                                        string date1 = Convert.ToString(DateTime.Now.Ticks);
                                                        string fileExtension = Path.GetExtension(imgname);
                                                        string pathimage = imgname;
                                                        foreach (var file in fileimages)
                                                        {
                                                            if (file != null && file.ContentLength > 0)
                                                            {
                                                                if (imgname.ToUpper() == file.FileName.ToUpper())
                                                                {
                                                                    var filename = Path.GetFileName(file.FileName.Replace(file.FileName.Split('.').FirstOrDefault().ToString(), date1));
                                                                    var filepath = Path.Combine(Server.MapPath("~/Content/Qusetion/"), filename);
                                                                    file.SaveAs(filepath);
                                                                    qst.Option_Image = filename;
                                                                    break;
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                                else if (isans.ToUpper() == "FALSE")
                                                {
                                                    qst.Is_Answer = false;
                                                }
                                            }
                                            else
                                            {
                                                qst.Is_Answer = false;
                                            }
                                            qst.Question_ID = tblobj.Question_ID;
                                            qst.Is_Active = true;
                                            qst.Is_Deleted = false;
                                            DbContext.tbl_DC_Question_Answer.Add(qst);
                                            DbContext.SaveChanges();
                                        }

                                        //Fifth option
                                        if (!string.IsNullOrEmpty(dr[30].ToString().Trim()) || !string.IsNullOrEmpty(dr[31].ToString().Trim()))
                                        {
                                            if (!string.IsNullOrEmpty(dr[30].ToString().Trim()))
                                            {
                                                qst.Option_Desc = dr[30].ToString();
                                            }
                                            if (!string.IsNullOrEmpty(dr[31].ToString().Trim()))
                                            {
                                                string imgname = dr[31].ToString();

                                                string date1 = Convert.ToString(DateTime.Now.Ticks);
                                                string fileExtension = Path.GetExtension(imgname);
                                                string pathimage = imgname;
                                                foreach (var file in fileimages)
                                                {
                                                    if (file != null && file.ContentLength > 0)
                                                    {
                                                        if (imgname.ToUpper() == file.FileName.ToUpper())
                                                        {
                                                            var filename = Path.GetFileName(file.FileName.Replace(file.FileName.Split('.').FirstOrDefault().ToString(), date1));
                                                            var filepath = Path.Combine(Server.MapPath("~/Content/Qusetion/"), filename);
                                                            file.SaveAs(filepath);
                                                            qst.Option_Image = filename;
                                                            break;
                                                        }
                                                    }
                                                }
                                            }
                                            if (!string.IsNullOrEmpty(dr[34].ToString().Trim()))
                                            {
                                                string isans = dr[34].ToString();
                                                if (isans.ToUpper() == "TRUE")
                                                {
                                                    qst.Is_Answer = true;
                                                    if (!string.IsNullOrEmpty(dr[32].ToString().Trim()))
                                                    {
                                                        qst.Option_Desc = dr[32].ToString();
                                                    }
                                                    if (!string.IsNullOrEmpty(dr[33].ToString().Trim()))
                                                    {
                                                        string imgname = dr[33].ToString();

                                                        string date1 = Convert.ToString(DateTime.Now.Ticks);
                                                        string fileExtension = Path.GetExtension(imgname);
                                                        string pathimage = imgname;
                                                        foreach (var file in fileimages)
                                                        {
                                                            if (file != null && file.ContentLength > 0)
                                                            {
                                                                if (imgname.ToUpper() == file.FileName.ToUpper())
                                                                {
                                                                    var filename = Path.GetFileName(file.FileName.Replace(file.FileName.Split('.').FirstOrDefault().ToString(), date1));
                                                                    var filepath = Path.Combine(Server.MapPath("~/Content/Qusetion/"), filename);
                                                                    file.SaveAs(filepath);
                                                                    qst.Option_Image = filename;
                                                                    break;
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                                else if (isans.ToUpper() == "FALSE")
                                                {
                                                    qst.Is_Answer = false;
                                                }
                                            }
                                            else
                                            {
                                                qst.Is_Answer = false;
                                            }
                                            qst.Question_ID = tblobj.Question_ID;
                                            qst.Is_Active = true;
                                            qst.Is_Deleted = false;
                                            DbContext.tbl_DC_Question_Answer.Add(qst);
                                            DbContext.SaveChanges();
                                        }

                                        //Six option
                                        if (!string.IsNullOrEmpty(dr[35].ToString().Trim()) || !string.IsNullOrEmpty(dr[36].ToString().Trim()))
                                        {
                                            if (!string.IsNullOrEmpty(dr[35].ToString().Trim()))
                                            {
                                                qst.Option_Desc = dr[35].ToString();
                                            }
                                            if (!string.IsNullOrEmpty(dr[36].ToString().Trim()))
                                            {
                                                string imgname = dr[36].ToString();

                                                string date1 = Convert.ToString(DateTime.Now.Ticks);
                                                string fileExtension = Path.GetExtension(imgname);
                                                string pathimage = imgname;
                                                foreach (var file in fileimages)
                                                {
                                                    if (file != null && file.ContentLength > 0)
                                                    {
                                                        if (imgname.ToUpper() == file.FileName.ToUpper())
                                                        {
                                                            var filename = Path.GetFileName(file.FileName.Replace(file.FileName.Split('.').FirstOrDefault().ToString(), date1));
                                                            var filepath = Path.Combine(Server.MapPath("~/Content/Qusetion/"), filename);
                                                            file.SaveAs(filepath);
                                                            qst.Option_Image = filename;
                                                            break;
                                                        }
                                                    }
                                                }
                                            }
                                            if (!string.IsNullOrEmpty(dr[39].ToString().Trim()))
                                            {
                                                string isans = dr[39].ToString();
                                                if (isans.ToUpper() == "TRUE")
                                                {
                                                    qst.Is_Answer = true;
                                                    if (!string.IsNullOrEmpty(dr[37].ToString().Trim()))
                                                    {
                                                        qst.Option_Desc = dr[37].ToString();
                                                    }
                                                    if (!string.IsNullOrEmpty(dr[38].ToString().Trim()))
                                                    {
                                                        string imgname = dr[38].ToString();

                                                        string date1 = Convert.ToString(DateTime.Now.Ticks);
                                                        string fileExtension = Path.GetExtension(imgname);
                                                        string pathimage = imgname;
                                                        foreach (var file in fileimages)
                                                        {
                                                            if (file != null && file.ContentLength > 0)
                                                            {
                                                                if (imgname.ToUpper() == file.FileName.ToUpper())
                                                                {
                                                                    var filename = Path.GetFileName(file.FileName.Replace(file.FileName.Split('.').FirstOrDefault().ToString(), date1));
                                                                    var filepath = Path.Combine(Server.MapPath("~/Content/Qusetion/"), filename);
                                                                    file.SaveAs(filepath);
                                                                    qst.Option_Image = filename;
                                                                    break;
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                                else if (isans.ToUpper() == "FALSE")
                                                {
                                                    qst.Is_Answer = false;
                                                }
                                            }
                                            else
                                            {
                                                qst.Is_Answer = false;
                                            }
                                            qst.Question_ID = tblobj.Question_ID;
                                            qst.Is_Active = true;
                                            qst.Is_Deleted = false;
                                            DbContext.tbl_DC_Question_Answer.Add(qst);
                                            DbContext.SaveChanges();
                                        }
                                        #endregion


                                    }
                                }
                            }
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        #endregion

        #region -------------------------------------------------------Exam --------------------------------------------------------------------
        [HttpGet]
        public ActionResult Exam()
        {
            Menupermission();
            if (Convert.ToString(Session["ROLE_CODE"]) == "S")
            {
                if (Session["Role_Id"] != null)
                {
                    int id = Convert.ToInt32(Session["Role_Id"]);
                    var p_chk = DbContext.tbl_DC_Menu_Permission.Where(x => x.Menu_Key_ID == 6 && x.ROLE_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    if (p_chk == null)
                    {
                        return RedirectToAction("checkpermission");
                    }
                }
            }
            ViewBag.Breadcrumb = "Exam";
            IEnumerable<View_DC_Exam_Setup> displayobj = DbContext.View_DC_Exam_Setup.OrderByDescending(x => x.Exam_ID).ToList();
            return View(displayobj);
        }
        [HttpGet]
        public ActionResult examsetup(int? id)
        {
            ViewBag.valid = 0;
            ViewBag.examtyp = 0;
            ViewBag.Breadcrumb = "Exam";

            ViewBag.powers = DbContext.tbl_DC_Power_Question.Where(x => x.Is_Active == true && x.Is_Deleted == false).OrderByDescending(x => x.Power_Type).ToList();
            ViewBag.pre_test_percentage = DbContext.tbl_DC_Percentage.Where(x => x.Is_Active == true && x.Is_Deleted == false).ToList();
            ViewBag.exam_type = DbContext.tbl_DC_Exam_Type.ToList();
            if (id != null)
            {
                tbl_DC_Exam examedit = DbContext.tbl_DC_Exam.Where(x => x.Exam_ID == id).FirstOrDefault();
                ViewBag.Boards = new SelectList(DbContext.tbl_DC_Board.Where(d => d.Is_Active == true && d.Is_Deleted == false), "Board_Id", "Board_Name", examedit.Board_Id);
                ViewBag.Boardid = examedit.Board_Id;
                ViewBag.classid = examedit.Class_Id;
                ViewBag.subjectid = examedit.Subject_Id;
                ViewBag.chid = examedit.Chapter_Id;
                ViewBag.examtyp = examedit.Exam_type;
                ViewBag.questions = examedit.Question_nos;
                ViewBag.attempt = examedit.Attempt_nos;
                ViewBag.time = examedit.Time;
                ViewBag.exam_name = examedit.Exam_Name;
                ViewBag.exam_id = examedit.Exam_ID;
                if (examedit.Percentage_retest != null)
                {
                    ViewBag.percentage = examedit.Percentage_retest;
                }
                if (examedit.Shedule_date != null)
                {
                    ViewBag.sdate = examedit.Shedule_date;
                }
                if (examedit.Shedule_time != null)
                {
                    ViewBag.stime = examedit.Shedule_time;
                }
                if (examedit.Validity != null)
                {
                    ViewBag.valid = examedit.Validity;
                }
                else
                {
                    ViewBag.valid = 0;
                }

                int? pexid = Convert.ToInt32(id);
                ViewBag.pow_exm = DbContext.SP_DC_Power_Examsetup(pexid).ToList();
                return View();
            }
            else
            {
                ViewBag.Boards = new SelectList(DbContext.tbl_DC_Board.Where(d => d.Is_Active == true && d.Is_Deleted == false), "Board_Id", "Board_Name");
            }
            return View();
        }

        [HttpPost]
        public JsonResult Addexams(tbl_DC_Exam obj, string[] P_Quantity, string[] p_id, string[] expid, string exam_type, string percentage, DateTime? s_date, string s_time, int Validity)
        {
            Menupermission();

            string message = string.Empty;
            try
            {
                int exam_types = Convert.ToInt32(exam_type);
                int percentages = 0;
                if (percentage != "" && percentage != null)
                {
                    percentages = Convert.ToInt32(percentage);
                }

                if (obj.Exam_Name == null || obj.Time == null)
                {
                    message = "0";
                }
                else
                {
                    if (P_Quantity.Length > 0)
                    {
                        #region add Exam
                        if (obj.Exam_ID == 0)        //----------For add Exam details
                        {
                            var exam = DbContext.tbl_DC_Exam.Where(b => b.Exam_Name == obj.Exam_Name && b.Is_Active == true && b.Is_Deleted == false).FirstOrDefault();
                            if (exam == null)
                            {
                                if (exam_types != 2)
                                {
                                    //var qstn_count = DbContext.tbl_DC_Question.Where(x => x.Chapter_Id == obj.Chapter_Id && x.Is_Active == true && x.Is_Deleted == false).Select(x => x.Question_ID).Count();
                                    //if (qstn_count >= obj.Question_nos)
                                    //{
                                    //var exam_chapter = DbContext.tbl_DC_Exam.Where(b => b.Chapter_Id == obj.Chapter_Id && b.Is_Active == true && b.Is_Deleted == false).FirstOrDefault();
                                    //if (exam_chapter == null)
                                    //{

                                    var _find = (tbl_DC_Exam)null;
                                    if (exam_types == 4 || exam_types == 6)
                                    {
                                        if (exam_types == 4)
                                        {
                                            _find = DbContext.tbl_DC_Exam.Where(x => x.Subject_Id == obj.Subject_Id && x.Exam_type == exam_types && x.Shedule_date == s_date && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();

                                        }
                                        if (exam_types == 6)
                                        {
                                            _find = DbContext.tbl_DC_Exam.Where(x => x.Subject_Id == obj.Subject_Id && x.Exam_type == exam_types && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();

                                        }
                                    }
                                    else
                                    {
                                        _find = DbContext.tbl_DC_Exam.Where(x => x.Chapter_Id == obj.Chapter_Id && x.Exam_type == exam_types && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                                    }

                                    if (_find == null)
                                    {
                                        var qstn_count = (Nullable<int>)null;
                                        if (exam_types == 4 || exam_types == 6)
                                        {
                                            qstn_count = DbContext.SP_count_of_questions(obj.Subject_Id, exam_types).FirstOrDefault();
                                        }
                                        else
                                        {
                                            qstn_count = DbContext.SP_count_of_questions(obj.Chapter_Id, exam_types).FirstOrDefault();
                                        }

                                        if (qstn_count >= obj.Question_nos)
                                        {

                                            message = "12";
                                            tbl_DC_Exam ex = new tbl_DC_Exam();
                                            ex.Board_Id = obj.Board_Id;
                                            ex.Class_Id = obj.Class_Id;
                                            ex.Subject_Id = obj.Subject_Id;

                                            ex.Exam_Name = obj.Exam_Name;
                                            ex.Attempt_nos = obj.Attempt_nos;
                                            ex.Question_nos = obj.Question_nos;
                                            ex.Time = obj.Time;
                                            ex.Exam_type = exam_types;
                                            if (exam_types == 4)
                                            {
                                                message = "13";
                                                //  / DateTime date =  s_date;
                                                //DateTime time = Convert.ToDateTime(s_time);
                                                ex.Shedule_date = s_date;
                                                ex.Shedule_time = s_time;
                                                ex.Is_Global = true;
                                                ex.Chapter_Id = null;
                                            }
                                            else if (exam_types == 6)
                                            {
                                                ex.Is_Global = false;
                                                ex.Chapter_Id = null;
                                                ex.Validity = Validity;
                                            }
                                            else if (exam_types == 1)
                                            {
                                                ex.Is_Global = false;
                                                ex.Chapter_Id = obj.Chapter_Id;
                                                ex.Validity = Validity;
                                            }
                                            else if (exam_types == 5)
                                            {
                                                ex.Is_Global = false;
                                                ex.Chapter_Id = obj.Chapter_Id;
                                                ex.Validity = Validity;
                                            }
                                            else
                                            {
                                                ex.Is_Global = false;
                                                ex.Chapter_Id = obj.Chapter_Id;
                                            }
                                            if (exam_types == 6)
                                            {
                                                ex.Validity = Validity;
                                            }
                                            ex.Is_Active = true;
                                            ex.Is_Deleted = false;
                                            ex.Inserted_By = HttpContext.User.Identity.Name;
                                            DbContext.tbl_DC_Exam.Add(ex);
                                            DbContext.SaveChanges();

                                            tbl_DC_Exam_Power ex_pow = new tbl_DC_Exam_Power();
                                            for (int i = 0; i < p_id.Length; i++)
                                            {
                                                if (P_Quantity[i] != "0" && P_Quantity[i] != "")
                                                {

                                                    message = "14";
                                                    ex_pow.Exam_ID = ex.Exam_ID;
                                                    ex_pow.Power_Id = Convert.ToInt32(p_id[i].ToString());
                                                    ex_pow.No_Of_Qstn = Convert.ToInt32(P_Quantity[i].ToString());
                                                    ex_pow.Inserted_By = HttpContext.User.Identity.Name;
                                                    ex_pow.Is_Active = true;
                                                    ex_pow.Is_Deleted = false;
                                                    DbContext.tbl_DC_Exam_Power.Add(ex_pow);
                                                    DbContext.SaveChanges();
                                                }
                                            }
                                            message = "1";
                                        }
                                        else
                                        {
                                            message = "02";
                                        }
                                    }
                                    else
                                    {
                                        message = "07";
                                    }


                                    //}
                                    //else
                                    //{
                                    //    message = "03";
                                    //}
                                    //}
                                    //else
                                    //{
                                    //    message = "02";
                                    //}
                                }
                                else                    //----------if test is a retest
                                {
                                    var qstn_count = DbContext.SP_count_of_questions(obj.Chapter_Id, exam_types).FirstOrDefault();
                                    if (qstn_count >= obj.Question_nos)
                                    {

                                        var _find = DbContext.tbl_DC_Exam.Where(x => x.Chapter_Id == obj.Chapter_Id && x.Exam_type == exam_types && x.Percentage_retest == percentages && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                                        if (_find == null)
                                        {
                                            tbl_DC_Exam ex = new tbl_DC_Exam();
                                            ex.Board_Id = obj.Board_Id;
                                            ex.Class_Id = obj.Class_Id;
                                            ex.Subject_Id = obj.Subject_Id;
                                            ex.Chapter_Id = obj.Chapter_Id;
                                            ex.Exam_Name = obj.Exam_Name;
                                            ex.Attempt_nos = obj.Attempt_nos;
                                            ex.Question_nos = obj.Question_nos;
                                            ex.Time = obj.Time;
                                            ex.Is_Global = false;
                                            ex.Exam_type = exam_types;
                                            ex.Percentage_retest = percentages;
                                            ex.Is_Active = true;
                                            ex.Is_Deleted = false;
                                            //ex.Inserted_By = HttpContext.User.Identity.Name;
                                            DbContext.tbl_DC_Exam.Add(ex);
                                            DbContext.SaveChanges();

                                            tbl_DC_Exam_Power ex_pow = new tbl_DC_Exam_Power();
                                            for (int i = 0; i < p_id.Length; i++)
                                            {
                                                if (P_Quantity[i] != "0" && P_Quantity[i] != "")
                                                {
                                                    ex_pow.Exam_ID = ex.Exam_ID;
                                                    ex_pow.Power_Id = Convert.ToInt32(p_id[i].ToString());
                                                    ex_pow.No_Of_Qstn = Convert.ToInt32(P_Quantity[i].ToString());
                                                    ex_pow.Inserted_By = HttpContext.User.Identity.Name;
                                                    ex_pow.Is_Active = true;
                                                    ex_pow.Is_Deleted = false;
                                                    DbContext.tbl_DC_Exam_Power.Add(ex_pow);
                                                    DbContext.SaveChanges();
                                                }
                                            }
                                            message = "1";
                                        }
                                        else
                                        {
                                            message = "07";
                                        }

                                    }
                                    else
                                    {
                                        message = "02";
                                    }


                                    //}
                                    //else
                                    //{
                                    //    message = "06";
                                    //}
                                }
                            }
                            else
                            {
                                message = "01";
                            }
                        }
                        #endregion

                        #region edit Exam
                        else        //For edit Exam details
                        {
                            if (obj.Exam_ID != 0)
                            {
                                var ex = DbContext.tbl_DC_Exam.Where(b => b.Exam_ID == obj.Exam_ID && b.Is_Active == true && b.Is_Deleted == false).FirstOrDefault();
                                var ex_pow = DbContext.tbl_DC_Exam_Power.Where(b => b.Exam_ID == obj.Exam_ID && b.Is_Active == true && b.Is_Deleted == false).ToList();
                                if (ex != null)
                                {
                                    if (exam_types != 2)
                                    {
                                        //var qstn_count = DbContext.tbl_DC_Question.Where(x => x.Chapter_Id == obj.Chapter_Id && x.Is_Active == true && x.Is_Deleted == false).Select(x => x.Question_ID).Count();
                                        var qstn_count = (Nullable<int>)null;
                                        if (exam_types == 4 || exam_types == 6)
                                        {
                                            qstn_count = DbContext.SP_count_of_questions(obj.Subject_Id, exam_types).FirstOrDefault();
                                        }
                                        else
                                        {
                                            qstn_count = DbContext.SP_count_of_questions(obj.Chapter_Id, exam_types).FirstOrDefault();
                                        }
                                        if (Convert.ToInt32(qstn_count) >= obj.Question_nos)
                                        {
                                            ex.Board_Id = obj.Board_Id;
                                            ex.Class_Id = obj.Class_Id;
                                            ex.Subject_Id = obj.Subject_Id;
                                            ex.Chapter_Id = obj.Chapter_Id;
                                            ex.Exam_type = obj.Exam_type;
                                            ex.Exam_Name = obj.Exam_Name;
                                            ex.Attempt_nos = obj.Attempt_nos;
                                            ex.Question_nos = obj.Question_nos;
                                            ex.Time = obj.Time;
                                            if (exam_types == 4)
                                            {
                                                // DateTime edate = Convert.ToDateTime(s_date);
                                                //DateTime etime = Convert.ToDateTime(s_time);
                                                ex.Is_Global = true;
                                                ex.Chapter_Id = null;
                                                ex.Shedule_date = s_date;
                                                ex.Shedule_time = s_time;
                                            }
                                            else if (exam_types == 6)
                                            {
                                                ex.Is_Global = false;
                                                ex.Chapter_Id = null;
                                                ex.Validity = Validity;
                                            }
                                            else if (exam_types == 1)
                                            {
                                                ex.Is_Global = false;
                                                ex.Chapter_Id = obj.Chapter_Id;
                                                ex.Validity = Validity;
                                            }
                                            else if (exam_types == 5)
                                            {
                                                ex.Is_Global = false;
                                                ex.Chapter_Id = obj.Chapter_Id;
                                                ex.Validity = Validity;
                                            }
                                            else
                                            {
                                                ex.Is_Global = false;
                                                ex.Chapter_Id = obj.Chapter_Id;
                                            }
                                            ex.Modified_By = HttpContext.User.Identity.Name;
                                            ex.Modified_Date = today;
                                            DbContext.Entry(ex).State = EntityState.Modified;
                                            DbContext.SaveChanges();

                                            for (int i = 0; i < expid.Length; i++)  // first isactive false
                                            {
                                                if (expid[i] != "0" && expid[i] != "")
                                                {
                                                    int id = Convert.ToInt32(expid[i].ToString());
                                                    var ex_pows = DbContext.tbl_DC_Exam_Power.Where(b => b.ExamP_ID == id && b.Is_Active == true && b.Is_Deleted == false).FirstOrDefault();
                                                    ex_pows.Is_Active = false;
                                                    ex_pows.Is_Deleted = true;
                                                    ex_pows.Modified_By = HttpContext.User.Identity.Name;
                                                    ex_pows.Modified_Date = today;
                                                    DbContext.Entry(ex_pows).State = EntityState.Modified;
                                                    DbContext.SaveChanges();
                                                }
                                            }

                                            for (int i = 0; i < p_id.Length; i++)
                                            {
                                                if (P_Quantity[i] != "0" && P_Quantity[i] != "")    //added new power exam
                                                {
                                                    if (expid[i] != "0" && expid[i] != "")
                                                    {
                                                        int id = Convert.ToInt32(expid[i].ToString());
                                                        var ex_pows = DbContext.tbl_DC_Exam_Power.Where(b => b.ExamP_ID == id).FirstOrDefault();
                                                        ex_pows.Power_Id = Convert.ToInt32(p_id[i].ToString());
                                                        ex_pows.No_Of_Qstn = Convert.ToInt32(P_Quantity[i].ToString());
                                                        ex_pows.Modified_By = HttpContext.User.Identity.Name;
                                                        ex_pows.Modified_Date = today;
                                                        ex_pows.Is_Active = true;
                                                        ex_pows.Is_Deleted = false;
                                                        DbContext.Entry(ex_pows).State = EntityState.Modified;
                                                        DbContext.SaveChanges();
                                                    }
                                                    else
                                                    {
                                                        tbl_DC_Exam_Power ex_pows = new tbl_DC_Exam_Power();
                                                        ex_pows.Exam_ID = obj.Exam_ID;
                                                        ex_pows.Power_Id = Convert.ToInt32(p_id[i].ToString());
                                                        ex_pows.No_Of_Qstn = Convert.ToInt32(P_Quantity[i].ToString());
                                                        ex_pows.Inserted_By = HttpContext.User.Identity.Name;
                                                        ex_pows.Modified_Date = today;
                                                        ex_pows.Is_Active = true;
                                                        ex_pows.Is_Deleted = false;
                                                        DbContext.tbl_DC_Exam_Power.Add(ex_pows);
                                                        DbContext.SaveChanges();
                                                    }
                                                }

                                            }
                                            message = "1";
                                        }
                                        else
                                        {
                                            message = "02";
                                        }
                                    }
                                    else
                                    {
                                        //var qstn_count = DbContext.tbl_DC_Exam.Where(x => x.Chapter_Id == obj.Chapter_Id && x.Exam_type == exam_types && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();


                                        var qstn_count = DbContext.SP_count_of_questions(obj.Chapter_Id, exam_types).FirstOrDefault();
                                        if (Convert.ToInt32(qstn_count) >= obj.Question_nos)
                                        {
                                            ex.Board_Id = obj.Board_Id;
                                            ex.Class_Id = obj.Class_Id;
                                            ex.Subject_Id = obj.Subject_Id;
                                            ex.Chapter_Id = obj.Chapter_Id;
                                            ex.Exam_Name = obj.Exam_Name;
                                            ex.Attempt_nos = obj.Attempt_nos;
                                            ex.Question_nos = obj.Question_nos;
                                            ex.Time = obj.Time;
                                            ex.Exam_type = exam_types;
                                            ex.Percentage_retest = percentages;
                                            ex.Is_Global = false;
                                            ex.Modified_By = HttpContext.User.Identity.Name;
                                            ex.Modified_Date = today;
                                            DbContext.Entry(ex).State = EntityState.Modified;
                                            DbContext.SaveChanges();

                                            for (int i = 0; i < expid.Length; i++)  // first isactive false
                                            {
                                                if (expid[i] != "0" && expid[i] != "")
                                                {
                                                    int id = Convert.ToInt32(expid[i].ToString());
                                                    var ex_pows = DbContext.tbl_DC_Exam_Power.Where(b => b.ExamP_ID == id && b.Is_Active == true && b.Is_Deleted == false).FirstOrDefault();
                                                    ex_pows.Is_Active = false;
                                                    ex_pows.Is_Deleted = true;
                                                    ex_pows.Modified_By = HttpContext.User.Identity.Name;
                                                    ex_pows.Modified_Date = today;
                                                    DbContext.Entry(ex_pows).State = EntityState.Modified;
                                                    DbContext.SaveChanges();
                                                }
                                            }

                                            for (int i = 0; i < p_id.Length; i++)
                                            {
                                                if (P_Quantity[i] != "0" && P_Quantity[i] != "")    //added new power exam
                                                {
                                                    if (expid[i] != "0" && expid[i] != "")
                                                    {
                                                        int id = Convert.ToInt32(expid[i].ToString());
                                                        var ex_pows = DbContext.tbl_DC_Exam_Power.Where(b => b.ExamP_ID == id).FirstOrDefault();
                                                        ex_pows.Power_Id = Convert.ToInt32(p_id[i].ToString());
                                                        ex_pows.No_Of_Qstn = Convert.ToInt32(P_Quantity[i].ToString());
                                                        ex_pows.Modified_By = HttpContext.User.Identity.Name;
                                                        ex_pows.Modified_Date = today;
                                                        ex_pows.Is_Active = true;
                                                        ex_pows.Is_Deleted = false;
                                                        DbContext.Entry(ex_pows).State = EntityState.Modified;
                                                        DbContext.SaveChanges();
                                                    }
                                                    else
                                                    {
                                                        tbl_DC_Exam_Power ex_pows = new tbl_DC_Exam_Power();
                                                        ex_pows.Exam_ID = obj.Exam_ID;
                                                        ex_pows.Power_Id = Convert.ToInt32(p_id[i].ToString());
                                                        ex_pows.No_Of_Qstn = Convert.ToInt32(P_Quantity[i].ToString());
                                                        ex_pows.Inserted_By = HttpContext.User.Identity.Name;
                                                        ex_pows.Modified_Date = today;
                                                        ex_pows.Is_Active = true;
                                                        ex_pows.Is_Deleted = false;
                                                        DbContext.tbl_DC_Exam_Power.Add(ex_pows);
                                                        DbContext.SaveChanges();
                                                    }
                                                }

                                            }
                                            message = "1";
                                        }
                                        else
                                        {
                                            message = "06";
                                        }
                                    }
                                }
                                else
                                {
                                    message = "05";
                                }
                            }
                        }
                        #endregion
                    }
                    else
                    {
                        message = "04";
                        return Json(message, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["WarningMessage"] = "Something went wrong." + ex.Message;
                //  return Json(ex.Message, JsonRequestBehavior.AllowGet);
                message = "15";

            }
            return Json(message, JsonRequestBehavior.AllowGet);
        }
        public ActionResult deletexam(int? id)
        {
            try
            {
                var data = DbContext.tbl_DC_Exam.Where(x => x.Exam_ID == id).FirstOrDefault();
                if (data != null)
                {
                    var exam_found = DbContext.tbl_DC_Exam_Result.Where(x => x.Exam_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    if (exam_found == null)
                    {
                        data.Is_Active = false;
                        data.Is_Deleted = true;
                        DbContext.Entry(data).State = EntityState.Modified;
                        DbContext.SaveChanges();

                        var expwr = DbContext.tbl_DC_Exam_Power.Where(x => x.Exam_ID == id);
                        foreach (var obj in expwr)
                        {
                            obj.Is_Active = false;
                            obj.Is_Deleted = true;
                            obj.Modified_Date = today;
                            DbContext.Entry(data).State = EntityState.Modified;

                        }
                        DbContext.SaveChanges();
                        TempData["SuccessMessage"] = "Selected test deleted successfully.";
                        return RedirectToAction("Exam");
                    }
                    else
                    {
                        TempData["WarningMessage"] = "Selected Eexam can not be deleted because it is in use.";
                        return RedirectToAction("Exam");
                    }

                }
                else
                {
                    TempData["WarningMessage"] = "No test found.";
                    return RedirectToAction("Exam");
                }
            }
            catch (Exception)
            {
                TempData["WarningMessage"] = "Something went wrong.";
                return RedirectToAction("Exam");
            }

        }

        [HttpPost]
        public JsonResult Getcounts_power(string chid, string exam_id)
        {
            string message = string.Empty;
            if (exam_id != "")
            {
                int ch_id = Convert.ToInt32(chid);
                int exam = Convert.ToInt32(exam_id);
                var obj = DbContext.SP_count_of_power(ch_id, exam).ToList();
                return Json(obj, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var objs = "";
                return Json(objs, JsonRequestBehavior.AllowGet);
            }

        }
        [HttpPost]
        public JsonResult Getcounts_power_sub(string subid, int exam_id)
        {
            int sub = Convert.ToInt32(subid);
            var obj = DbContext.SP_count_of_power_sub(sub, exam_id).ToList();

            return Json(obj, JsonRequestBehavior.AllowGet);

        }
        #endregion

        #region--------------------------------------------------SMTP Configuration------------------------------------------------------------
        [HttpGet]
        public ActionResult AddSMTPConfig(int? id)
        {
            ViewBag.smtpconfiguration = "active";
            ViewBag.general = "general";
            Menupermission();
            if (Convert.ToString(Session["ROLE_CODE"]) == "S")
            {
                if (Session["Role_Id"] != null)
                {
                    int id1 = Convert.ToInt32(Session["Role_Id"]);
                    var p_chk = DbContext.tbl_DC_Menu_Permission.Where(x => x.Menu_Key_ID == 7 && x.ROLE_ID == id1 && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    if (p_chk == null)
                    {
                        return RedirectToAction("checkpermission");
                    }
                }
            }
            ViewBag.Breadcrumb = "SMTP";
            if (id != null)
            {
                tbl_DC_Smtp_Details editobj = (from a in DbContext.tbl_DC_Smtp_Details.Where(x => x.SMTP_ID == id && x.Is_Active == true && x.Is_Deleted == false) select a).FirstOrDefault();
                if (editobj != null)
                {
                    ViewBag.editid = editobj.SMTP_ID;
                    TempData["chkor"] = editobj.SMTP_Ssl;
                    return View(editobj);
                }
                else
                {
                    TempData["WarningMessage"] = "Invalid SMTP configuration details.";
                    return View();
                }
            }
            return View();
        }
        [HttpGet]
        public ActionResult SMTPconfiguration()
        {
            ViewBag.general = "general";
            ViewBag.smtpconfiguration = "active";
            Menupermission();
            if (Convert.ToString(Session["ROLE_CODE"]) == "S")
            {
                if (Session["Role_Id"] != null)
                {
                    int id = Convert.ToInt32(Session["Role_Id"]);
                    var p_chk = DbContext.tbl_DC_Menu_Permission.Where(x => x.Menu_Key_ID == 7 && x.ROLE_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    if (p_chk == null)
                    {
                        return RedirectToAction("checkpermission");
                    }
                }
            }
            ViewBag.Breadcrumb = "SMTP";
            var data = DbContext.tbl_DC_Smtp_Details.Where(x => x.Is_Active == true && x.Is_Deleted == false).OrderByDescending(x => x.SMTP_ID).ToList();
            ViewBag.data = data;
            return View();
        }

        [HttpPost]
        public ActionResult AddSMTPConfig(DigiChampsModel.DigiChampsSMTPConfigModel obj, string smtpid)
        {
            ViewBag.general = "general";
            ViewBag.smtpconfiguration = "active";
            try
            {
                Menupermission();
                if (ModelState.IsValid)
                {
                    if (smtpid == "")
                    {
                        if (obj.SMTP_Sender.Trim() != "")
                        {
                            if (obj.SMTP_HostName.Trim() != "")
                            {
                                var data = (DbContext.tbl_DC_Smtp_Details.Where(x => x.SMTP_Sender == obj.SMTP_Sender || x.SMTP_HostName == obj.SMTP_HostName && x.Is_Active == true && x.Is_Deleted == false)).FirstOrDefault();
                                if (data != null)
                                {
                                    TempData["WarningMessage"] = "Same sendername or hostname already exists.";
                                }
                                else
                                {
                                    tbl_DC_Smtp_Details obj1 = new tbl_DC_Smtp_Details();
                                    obj1.SMTP_Sender = obj.SMTP_Sender;
                                    obj1.SMTP_HostName = obj.SMTP_HostName;
                                    obj1.SMTP_User = obj.SMTP_User;
                                    obj1.SMTP_Pwd = obj.SMTP_Pwd;
                                    obj1.SMTP_Port = obj.SMTP_Port;
                                    obj1.SMTP_Ssl = obj.SMTP_Ssl;
                                    obj1.Is_Active = true;
                                    obj1.Is_Deleted = false;
                                    DbContext.tbl_DC_Smtp_Details.Add(obj1);
                                    DbContext.SaveChanges();
                                    TempData["SuccessMessage"] = "SMTPConfig details successfully saved.";
                                    return RedirectToAction("SMTPconfiguration");
                                }
                            }
                            else
                            {
                                TempData["WarningMessage"] = "Please enter host details.";
                            }
                        }
                        else
                        {
                            TempData["WarningMessage"] = "Please enter sender's name.";
                        }
                    }
                    else
                    {
                        int smtp_id = Convert.ToInt32(smtpid);
                        tbl_DC_Smtp_Details obj1 = DbContext.tbl_DC_Smtp_Details.Where(x => x.SMTP_ID == smtp_id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        if (obj1 != null)
                        {
                            if (obj.SMTP_Sender.Trim() != "")
                            {
                                if (obj.SMTP_HostName.Trim() != "")
                                {
                                    obj1.SMTP_Sender = obj.SMTP_Sender;
                                    obj1.SMTP_HostName = obj.SMTP_HostName;
                                    obj1.SMTP_User = obj.SMTP_User;
                                    obj1.SMTP_Pwd = obj.SMTP_Pwd;
                                    obj1.SMTP_Port = obj.SMTP_Port;
                                    obj1.SMTP_Ssl = obj.SMTP_Ssl;
                                    obj1.Is_Active = true;
                                    obj1.Is_Deleted = false;
                                    DbContext.Entry(obj1).State = EntityState.Modified;
                                    DbContext.SaveChanges();
                                    TempData["SuccessMessage"] = "SMTPConfig details successfully saved.";
                                    return RedirectToAction("SMTPconfiguration");
                                }
                                else
                                {
                                    TempData["WarningMessage"] = "Please enter host details.";
                                }
                            }
                            else
                            {
                                TempData["WarningMessage"] = "Please enter sender's name.";
                            }
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "Invalid SMTPConfig details.";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return View();
        }
        public ActionResult DeleteSmtpConfig(int? id)
        {
            ViewBag.smtpconfiguration = "active";
            ViewBag.general = "general";
            try
            {
                if (id != null)
                {
                    tbl_DC_Smtp_Details obj = DbContext.tbl_DC_Smtp_Details.First(x => x.SMTP_ID == id);
                    obj.Is_Active = false;
                    obj.Is_Deleted = true;
                    obj.Modified_Date = today;
                    DbContext.Entry(obj).State = EntityState.Modified;
                    DbContext.SaveChanges();
                    TempData["SuccessMessage"] = "SMTPConfig deleted successfully.";
                    return RedirectToAction("SMTPconfiguration");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return View();
        }
        #endregion

        #region--------------------------------------------------MAIL Configuration------------------------------------------------------------

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
        [HttpGet]
        public ActionResult AddMailConfig(int? id)
        {
            Menupermission();
            ViewBag.smtpconfiguration = "active";
            ViewBag.general = "general";
            if (Convert.ToString(Session["ROLE_CODE"]) == "S")
            {
                if (Session["Role_Id"] != null)
                {
                    int id1 = Convert.ToInt32(Session["Role_Id"]);
                    var p_chk = DbContext.tbl_DC_Menu_Permission.Where(x => x.Menu_Key_ID == 7 && x.ROLE_ID == id1 && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    if (p_chk == null)
                    {
                        return RedirectToAction("checkpermission");
                    }
                }
            }
            ViewBag.Breadcrumb = "MAIL";
            if (id == null)
            {
                //var data = DbContext.tbl_DC_MailConfigBody.Where(x => x.Is_Active == true && x.Is_Deleted == false).ToList();
                //ViewBag.data = data.ToList();
                //  DigiChampsModel.DigiChampsMAILConfigModel obj = new DigiChampsModel.DigiChampsMAILConfigModel();
                ViewBag.EmailType_ID = new SelectList(DbContext.tbl_DC_MailType.Where(x => x.Is_Active == true && x.Is_Deleted == false).OrderByDescending(x => x.EmailType_ID), "EmailType_ID", "Email_Alert");

                ViewBag.SMTP_ID = new SelectList(DbContext.tbl_DC_Smtp_Details.Where(x => x.Is_Active == true && x.Is_Deleted == false).OrderByDescending(x => x.SMTP_ID), "SMTP_ID", "SMTP_Sender");
                ViewBag.alert = id;
            }
            else
            {
                var data1 = (from a in DbContext.tbl_DC_MailConfigBody.Where(x => x.EmailConf_ID == id && x.Is_Active == true && x.Is_Deleted == false)
                             join c in DbContext.tbl_DC_Smtp_Details.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                             on a.SMTP_ID equals c.SMTP_ID
                             join b in DbContext.tbl_DC_MailType.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                             on a.EmailConf_AlertName equals b.EmailType_ID
                             select new DigiChampsModel.DigiChampsMAILConfigModel
                             {
                                 SMTP_ID = c.SMTP_ID,
                                 SMTP_Sender = c.SMTP_Sender,
                                 EmailType_ID = b.EmailType_ID,
                                 EmailConf_Alert = b.Email_Alert,
                                 EmailConf_Body = a.EmailConf_Body,
                                 EmailConf_AlertName = a.EmailConf_AlertName,
                                 Subject = a.Email_Subject
                             }).FirstOrDefault();
                ViewBag.alert = id;

                if (data1 != null)
                {
                    DigiChampsModel.DigiChampsMAILConfigModel obj = new DigiChampsModel.DigiChampsMAILConfigModel();
                    obj.SMTP_ID = data1.SMTP_ID;
                    obj.SMTP_Sender = data1.SMTP_Sender;
                    obj.EmailType_ID = data1.EmailType_ID;
                    obj.EmailConf_AlertName = data1.EmailConf_AlertName;
                    obj.EmailConf_Body = data1.EmailConf_Body;
                    obj.EmailConf_Alert = data1.EmailConf_Alert;
                    obj.Subject = data1.Subject;
                    //ViewBag.sender = data1.SMTP_Sender;
                    ViewBag.EmailBody = data1.EmailConf_Body;
                    ViewBag.mail_id = id;
                    ViewBag.EmailType_ID = new SelectList(DbContext.tbl_DC_MailType.Where(x => x.Is_Active == true && x.Is_Deleted == false).OrderByDescending(x => x.EmailType_ID), "EmailType_ID", "Email_Alert", obj.EmailConf_AlertName);

                    ViewBag.SMTP = new SelectList(DbContext.tbl_DC_Smtp_Details.Where(x => x.Is_Active == true && x.Is_Deleted == false).OrderByDescending(x => x.SMTP_ID), "SMTP_ID", "SMTP_Sender", obj.SMTP_ID);

                    return View(data1);
                }
                else
                {
                    TempData["WarningMessage"] = "Invalid mail configuration.";
                    return RedirectToAction("MailConfiguration");
                }
            }

            return View();
        }
        [HttpGet]
        public ActionResult MailConfiguration()
        {
            ViewBag.smtpconfiguration = "active";
            ViewBag.general = "general";
            Menupermission();
            if (Convert.ToString(Session["ROLE_CODE"]) == "S")
            {
                if (Session["Role_Id"] != null)
                {
                    int id = Convert.ToInt32(Session["Role_Id"]);
                    var p_chk = DbContext.tbl_DC_Menu_Permission.Where(x => x.Menu_Key_ID == 7 && x.ROLE_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    if (p_chk == null)
                    {
                        return RedirectToAction("checkpermission");
                    }
                }
            }
            ViewBag.Breadcrumb = "MAIL";
            ViewBag.EmailConf_ID = new SelectList(DbContext.tbl_DC_MailType.Where(x => x.Is_Active == true && x.Is_Deleted == false).OrderByDescending(x => x.EmailType_ID), "EmailType_ID", "Email_Alert");
            ViewBag.SMTP_ID = new SelectList(DbContext.tbl_DC_Smtp_Details.Where(x => x.Is_Active == true && x.Is_Deleted == false), "SMTP_ID", "SMTP_Sender");

            var data = (from b in DbContext.tbl_DC_MailConfigBody.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                        join a in DbContext.tbl_DC_MailType.Where(x => x.Is_Active == true && x.Is_Deleted == false) on b.EmailConf_AlertName equals a.EmailType_ID
                        join c in DbContext.tbl_DC_Smtp_Details.Where(x => x.Is_Active == true && x.Is_Deleted == false) on b.SMTP_ID equals c.SMTP_ID
                        select new DigiChampsModel.DigiChampsMAILConfigModel
                        {
                            SMTP_ID = c.SMTP_ID,
                            SMTP_Sender = c.SMTP_Sender,
                            EmailConf_ID = b.EmailConf_ID,
                            Email_Subject = b.Email_Subject,
                            EmailConf_Alert = a.Email_Alert,
                            EmailConf_Body = b.EmailConf_Body,
                            Email_AlertName = a.Email_AlertName
                        }).OrderByDescending(x => x.EmailConf_ID).ToList();

            ViewBag.data = data.ToList();

            return View();
        }
        [HttpPost, ValidateInput(false)]
        public ActionResult AddMailConfig(DigiChampsModel.DigiChampsMAILConfigModel obj, string emailid, string msg_body)
        {
            ViewBag.smtpconfiguration = "active";
            ViewBag.general = "general";
            try
            {
                Menupermission();
                int alert_id = Convert.ToInt32(obj.EmailConf_ID);
                var v = DbContext.tbl_DC_MailType.Where(x => x.EmailType_ID == alert_id).FirstOrDefault();
                if (v == null)
                {
                    if (emailid != "")
                    {
                        if (msg_body.Trim() != "")
                        {

                            int id = Convert.ToInt32(emailid);
                            tbl_DC_MailConfigBody obj1 = DbContext.tbl_DC_MailConfigBody.Where(x => x.EmailConf_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                            int eid = Convert.ToInt32(obj.EmailConf_AlertName);
                            obj1.EmailConf_AlertName = eid;
                            var mlt = DbContext.tbl_DC_MailType.Where(x => x.EmailType_ID == eid && x.Is_Active == true && x.Is_Deleted == false).Select(x => x.Email_AlertName).FirstOrDefault();
                            obj1.EmailConf_Body = msg_body;
                            obj1.SMTP_ID = Convert.ToInt32(obj.SMTP_ID);
                            obj1.EmailConf_Alert = Convert.ToString(mlt);
                            obj1.Modified_Date = today;
                            obj1.Email_Subject = obj.Subject;
                            obj1.Modified_By = HttpContext.User.Identity.Name;
                            DbContext.SaveChanges();
                            TempData["SuccessMessage"] = "Mail successfully updated.";
                        }
                        else
                        {
                            TempData["WarningMessage"] = "Please enter mail alert message.";
                        }
                    }
                    else
                    {

                        //ViewBag.EmailType_ID = new SelectList(DbContext.tbl_DC_MailType.Where(x => x.Is_Active == true && x.Is_Deleted == false).OrderByDescending(x => x.Email_Alert), "EmailType_ID", "Email_Alert");

                        tbl_DC_MailConfigBody obj1 = new tbl_DC_MailConfigBody();
                        // obj1.EmailConf_ID = obj.EmailConf_ID;
                        int eid = Convert.ToInt32(obj.EmailConf_AlertName);
                        if (msg_body.Trim() != "")
                        {
                            var data = DbContext.tbl_DC_MailConfigBody.Where(x => x.EmailConf_AlertName == obj.EmailConf_AlertName && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                            if (data != null)
                            {
                                TempData["WarningMessage"] = "Alert name already exist";
                                return RedirectToAction("MailConfiguration");
                            }

                            obj1.EmailConf_AlertName = eid;
                            var mlt = DbContext.tbl_DC_MailType.Where(x => x.EmailType_ID == eid && x.Is_Active == true && x.Is_Deleted == false).Select(x => x.Email_AlertName).FirstOrDefault();
                            obj1.EmailConf_Body = msg_body;
                            obj1.SMTP_ID = Convert.ToInt32(obj.SMTP_ID);
                            obj1.EmailConf_Alert = Convert.ToString(mlt);
                            obj1.Is_Active = true;
                            obj1.Is_Deleted = false;
                            obj1.Email_Subject = obj.Subject;
                            obj1.Inserted_By = HttpContext.User.Identity.Name;
                            obj1.Inserted_Date = today;
                            DbContext.tbl_DC_MailConfigBody.Add(obj1);
                            DbContext.SaveChanges();
                            TempData["SuccessMessage"] = "Mail created successfully.";

                        }
                        else
                        {
                            TempData["WarningMessage"] = "Please enter mail alert message.";
                        }
                        return RedirectToAction("MailConfiguration");
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "Selected alert name doesnot exist.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
                return RedirectToAction("Mailconfiguration");
            }
            return RedirectToAction("Mailconfiguration");
        }
        public ActionResult DeleteMailConfig(int? id)
        {
            ViewBag.smtpconfiguration = "active";
            ViewBag.general = "general";
            try
            {
                tbl_DC_MailConfigBody obj = DbContext.tbl_DC_MailConfigBody.Where(x => x.EmailConf_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                obj.Is_Active = false;
                obj.Is_Deleted = true;
                obj.Modified_By = HttpContext.User.Identity.Name;
                obj.Modified_Date = today;
                DbContext.Entry(obj).State = EntityState.Modified;
                DbContext.SaveChanges();
                TempData["SuccessMessage"] = "Mail deleted successfully.";
                return RedirectToAction("Mailconfiguration");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something Went Wrong.";
            }
            return View();
        }
        #endregion-----------------

        #region---------------------------------------------------------Sms_API----------------------------------------------------------------
        [HttpGet]
        public ActionResult View_Smsapi()
        {
            ViewBag.general = "general";
            ViewBag.view_smsapi = "active";
            Menupermission();
            if (Convert.ToString(Session["ROLE_CODE"]) == "S")
            {
                if (Session["Role_Id"] != null)
                {
                    int id = Convert.ToInt32(Session["Role_Id"]);
                    var p_chk = DbContext.tbl_DC_Menu_Permission.Where(x => x.Menu_Key_ID == 8 && x.ROLE_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    if (p_chk == null)
                    {
                        return RedirectToAction("checkpermission");
                    }
                }
            }
            ViewBag.Breadcrumb = "Sms";
            IEnumerable<tbl_DC_Sms_API> displyobj = DbContext.tbl_DC_Sms_API.Where(x => x.Is_Active == true && x.Is_Deleted == false).OrderByDescending(x => x.Api_Id).ToList();
            return View(displyobj);
        }
        [HttpGet]
        public ActionResult add_Smsapi(int? id)
        {
            ViewBag.view_smsapi = "active";
            ViewBag.general = "general";
            try
            {
                Menupermission();
                if (Convert.ToString(Session["ROLE_CODE"]) == "S")
                {
                    if (Session["Role_Id"] != null)
                    {
                        int id1 = Convert.ToInt32(Session["Role_Id"]);
                        var p_chk = DbContext.tbl_DC_Menu_Permission.Where(x => x.Menu_Key_ID == 8 && x.ROLE_ID == id1 && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        if (p_chk == null)
                        {
                            return RedirectToAction("checkpermission");
                        }
                    }
                }
                if (id != null)
                {
                    ViewBag.Breadcrumb = "Sms";
                    var displyobj = DbContext.tbl_DC_Sms_API.Where(x => x.Api_Id == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    if (displyobj != null)
                    {
                        ViewBag.messagetype = displyobj.Message_Type;
                        ViewBag.password = displyobj.Password;
                        return View(displyobj);
                    }
                    else
                    {
                        TempData["WarningMessage"] = "Invalid API details.";
                    }
                }
                else { return View(); }
            }
            catch
            {
                TempData["WarningMessage"] = "Something went wrong.";
            }
            return View();
        }
        [HttpPost]
        public ActionResult add_Smsapi(tbl_DC_Sms_API obj, string Message_Type, string uppassword)
        {
            ViewBag.general = "general";
            ViewBag.view_smsapi = "active";
            try
            {
                if (obj.Api_Url == null || obj.Username == null || Message_Type == "" || obj.Api_Name == null)
                {
                    TempData["WarningMessage"] = "Please enter all required fields.";
                    return View(obj);
                }
                else
                {
                    if (obj.Api_Id == 0)
                    {
                        var getobj = DbContext.tbl_DC_Sms_API.Where(x => x.Api_Name == obj.Api_Name && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        if (getobj == null)
                        {
                            tbl_DC_Sms_API obj1 = obj;
                            obj1.Message_Type = Message_Type;
                            obj1.Is_Active = true;
                            obj1.Is_Deleted = false;
                            obj1.Inserted_By = HttpContext.User.Identity.Name;
                            obj1.Inserted_Date = today;
                            DbContext.tbl_DC_Sms_API.Add(obj1);
                            DbContext.SaveChanges();
                            TempData["SuccessMessage"] = "Sms API details saved successfully.";
                            return RedirectToAction("View_Smsapi", "Admin", obj);
                        }
                        else
                        {
                            TempData["WarningMessage"] = "Sms API name already exists.";
                            return View("SMSTemplate_Details", obj);
                        }
                    }
                    else
                    {
                        tbl_DC_Sms_API editobj = DbContext.tbl_DC_Sms_API.Where(x => x.Api_Id == obj.Api_Id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        editobj.Api_Name = obj.Api_Name;
                        editobj.Username = obj.Username;
                        if (uppassword != "*****")
                        {
                            editobj.Password = uppassword;
                        }
                        editobj.Sender_Type = obj.Sender_Type;
                        editobj.Api_Url = obj.Api_Url;
                        editobj.Message_Type = Message_Type;
                        editobj.Inserted_By = HttpContext.User.Identity.Name;
                        editobj.Inserted_Date = today;
                        DbContext.Entry(editobj).State = EntityState.Modified;
                        DbContext.SaveChanges();
                        TempData["SuccessMessage"] = "Sms API details updated successfully.";
                        return RedirectToAction("View_Smsapi", "Admin", obj);
                    }
                }
            }
            catch
            {
                TempData["WarningMessage"] = "Something went wrong.";
            }
            return View();
        }
        [HttpPost]
        public ActionResult add_Smsapi(tbl_DC_Sms_API obj, string Message_Type)
        {
            ViewBag.general = "general";
            ViewBag.view_smsapi = "active";
            try
            {
                if (obj.Api_Url == null || obj.Username == null || Message_Type == "" || obj.Api_Name == null)
                {
                    TempData["WarningMessage"] = "Please enter all required fields.";
                    return View(obj);
                }
                else
                {
                    if (obj.Api_Id == 0)
                    {
                        var getobj = DbContext.tbl_DC_Sms_API.Where(x => x.Api_Name == obj.Api_Name && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        if (getobj == null)
                        {
                            tbl_DC_Sms_API obj1 = obj;
                            obj1.Message_Type = Message_Type;
                            obj1.Is_Active = true;
                            obj1.Is_Deleted = false;
                            obj1.Inserted_By = HttpContext.User.Identity.Name;
                            obj1.Inserted_Date = today;
                            DbContext.tbl_DC_Sms_API.Add(obj1);
                            DbContext.SaveChanges();
                            TempData["SuccessMessage"] = "Sms API details saved successfully.";
                            return RedirectToAction("View_Smsapi", "Admin", obj);
                        }
                        else
                        {
                            TempData["WarningMessage"] = "Sms API name already exists.";
                            return View("SMSTemplate_Details", obj);
                        }
                    }
                    else
                    {
                        tbl_DC_Sms_API editobj = DbContext.tbl_DC_Sms_API.Where(x => x.Api_Id == obj.Api_Id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        editobj.Api_Name = obj.Api_Name;
                        editobj.Username = obj.Username;
                        editobj.Password = obj.Password;
                        editobj.Api_Url = obj.Api_Url;
                        editobj.Message_Type = Message_Type;
                        editobj.Inserted_By = HttpContext.User.Identity.Name;
                        editobj.Inserted_Date = today;
                        DbContext.Entry(editobj).State = EntityState.Modified;
                        DbContext.SaveChanges();
                        TempData["SuccessMessage"] = "Sms API details updated successfully.";
                        return RedirectToAction("View_Smsapi", "Admin", obj);
                    }
                }
            }
            catch
            {
                TempData["WarningMessage"] = "Something went wrong.";
            }
            return View();
        }
        [HttpGet]
        public ActionResult delete_Smsapi(int? id)
        {
            ViewBag.general = "general";
            ViewBag.view_smsapi = "active";
            if (id != null)
            {
                tbl_DC_Sms_API deleteobj = DbContext.tbl_DC_Sms_API.Where(x => x.Api_Id == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                if (deleteobj != null)
                {
                    deleteobj.Is_Active = false;
                    deleteobj.Is_Deleted = true;
                    deleteobj.Modified_Date = today;
                    deleteobj.Modified_By = HttpContext.User.Identity.Name;
                    DbContext.Entry(deleteobj).State = EntityState.Modified;
                    DbContext.SaveChanges();
                    TempData["SuccessMessage"] = "API deleted successfully.";
                    return RedirectToAction("View_Smsapi");
                }
            }
            return View("View_Smsapi", "Admin");
        }
        #endregion

        #region-------------------------------------------------------Sms Template-------------------------------------------------------------
        [HttpGet]
        public ActionResult sms_templates()
        {
            ViewBag.general = "general";
            ViewBag.view_smsapi = "active";
            Menupermission();
            if (Convert.ToString(Session["ROLE_CODE"]) == "S")
            {
                if (Session["Role_Id"] != null)
                {
                    int id = Convert.ToInt32(Session["Role_Id"]);
                    var p_chk = DbContext.tbl_DC_Menu_Permission.Where(x => x.Menu_Key_ID == 8 && x.ROLE_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    if (p_chk == null)
                    {
                        return RedirectToAction("checkpermission");
                    }
                }
            }
            ViewBag.Breadcrumb = "Sms template";
            IEnumerable<View_DC_SMS_API> displyobj = DbContext.View_DC_SMS_API.OrderByDescending(x => x.Api_Id).ToList();
            return View(displyobj);
        }
        [HttpGet]
        public ActionResult SMSTemplate_Details(int? id)
        {
            ViewBag.general = "general";
            ViewBag.view_smsapi = "active";
            try
            {
                Menupermission();
                if (id != null)
                {
                    ViewBag.Breadcrumb = "Sms template";
                    var displyobj = DbContext.tbl_DC_Sms_Template.Where(x => x.Sms_Id == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    if (displyobj != null)
                    {
                        ViewBag.smstemp = new SelectList(DbContext.tbl_DC_Sms_API.OrderByDescending(x => x.Api_Name), "Api_Id", "Api_Name", displyobj.Api_Id);
                        return View(displyobj);
                    }
                    else
                    {
                        TempData["WarningMessage"] = "Invalid API details.";
                    }
                }
                else
                {
                    ViewBag.smstemplate = new SelectList(DbContext.tbl_DC_Sms_API.OrderByDescending(x => x.Api_Name), "Api_Id", "Api_Name");
                    return View();
                }
            }
            catch
            {
                TempData["WarningMessage"] = "Something went wrong.";
            }
            return View();
        }
        [HttpPost]
        public ActionResult add_Smstemplate(tbl_DC_Sms_Template obj)
        {
            ViewBag.general = "general";
            ViewBag.view_smsapi = "active";
            try
            {
                if (Convert.ToString(Session["ROLE_CODE"]) == "S")
                {
                    if (Session["Role_Id"] != null)
                    {
                        int id = Convert.ToInt32(Session["Role_Id"]);
                        var p_chk = DbContext.tbl_DC_Menu_Permission.Where(x => x.Menu_Key_ID == 8 && x.ROLE_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        if (p_chk == null)
                        {
                            return RedirectToAction("checkpermission");
                        }
                    }
                }
                ViewBag.smstemplate = new SelectList(DbContext.tbl_DC_Sms_API.OrderByDescending(x => x.Api_Name), "Api_Id", "Api_Name");
                if (obj.Sms_Alert_Name == null || obj.Api_Id == null || obj.Sms_Body == null)
                {
                    TempData["WarningMessage"] = "Please enter all required fields.";
                    return View("SMSTemplate_Details", obj);
                }
                else
                {
                    if (obj.Sms_Id == 0)
                    {
                        var getobj = DbContext.tbl_DC_Sms_Template.Where(x => x.Sms_Alert_Name == obj.Sms_Alert_Name && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        if (getobj == null)
                        {
                            tbl_DC_Sms_Template obj1 = obj;
                            obj1.Is_Active = true;
                            obj1.Is_Deleted = false;
                            obj1.Inserted_By = HttpContext.User.Identity.Name;
                            obj1.Inserted_Date = today;
                            DbContext.tbl_DC_Sms_Template.Add(obj1);
                            DbContext.SaveChanges();
                            TempData["SuccessMessage"] = "Sms template details saved successfully.";
                            return RedirectToAction("sms_templates", "Admin");
                        }
                        else
                        {
                            TempData["WarningMessage"] = "Sms alert name already exists.";
                            return View("SMSTemplate_Details", obj);
                        }
                    }
                    else
                    {
                        tbl_DC_Sms_Template editobj = DbContext.tbl_DC_Sms_Template.Where(x => x.Sms_Id == obj.Sms_Id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        if (editobj != null)
                        {
                            editobj.Sms_Body = obj.Sms_Body;
                            editobj.Sms_Alert_Name = obj.Sms_Alert_Name;
                            editobj.Api_Id = obj.Api_Id;
                            editobj.Inserted_By = HttpContext.User.Identity.Name;
                            editobj.Inserted_Date = today;
                            DbContext.Entry(editobj).State = EntityState.Modified;
                            DbContext.SaveChanges();
                            TempData["SuccessMessage"] = "Sms template details updated successfully.";
                            return RedirectToAction("sms_templates", "Admin");
                        }
                    }
                }
            }
            catch
            {
                TempData["WarningMessage"] = "Something went wrong.";
            }
            return View();
        }
        [HttpGet]
        public ActionResult delete_Smstemplate(int? id)
        {
            ViewBag.general = "general";
            ViewBag.view_smsapi = "active";
            if (id != null)
            {
                tbl_DC_Sms_Template deleteobj = DbContext.tbl_DC_Sms_Template.Where(x => x.Sms_Id == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                if (deleteobj != null)
                {
                    deleteobj.Is_Active = false;
                    deleteobj.Is_Deleted = true;
                    deleteobj.Modified_Date = today;
                    deleteobj.Modified_By = HttpContext.User.Identity.Name;
                    DbContext.Entry(deleteobj).State = EntityState.Modified;
                    DbContext.SaveChanges();
                    TempData["SuccessMessage"] = "Sms template deleted successfully.";
                    return RedirectToAction("sms_templates");
                }
            }
            return View("View_Smsapi", "Admin");
        }
        #endregion

        #region ----------------------------------------------------login/logoutstatus----------------------------------------------------------
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
                    id_found.Login_DateTime = today;
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
                    t_logi.Login_DateTime = today;
                    var log_name = DbContext.tbl_DC_USER_SECURITY.Where(x => x.USER_CODE == usercode).Select(x => x.USER_NAME).FirstOrDefault();
                    t_logi.Login_By = log_name;
                    t_logi.Status = true;
                    DbContext.tbl_DC_LoginStatus.Add(t_logi);
                    DbContext.SaveChanges();
                }

            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }

        }
        public void logoutstatus(string usercode)
        {
            try
            {
                tbl_DC_LoginStatus t_logo = DbContext.tbl_DC_LoginStatus.Where(x => x.Login_ID == usercode).FirstOrDefault();
                if (t_logo != null)
                {
                    t_logo.Logout_DateTime = today;
                    t_logo.Status = false;
                    DbContext.SaveChanges();
                }

            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }

        }
        #endregion

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

        #region --------------------------------------------------------LOGIN REPORT------------------------------------------------------------------
        [HttpGet]
        public ActionResult Login_report()
        {
            ViewBag.Shift_Report = "active";
            ViewBag.report = "report";
            Menupermission();
            ViewBag.Breadcrumb = "Login Status Report";
            ViewBag.login_Status = DbContext.tbl_DC_LoginStatus.OrderByDescending(x => x.Login_DateTime).ToList();
            return View();
        }
        [HttpPost]
        public ActionResult Login_report(string f_Date, string t_Date)
        {
            ViewBag.Shift_Report = "active";
            ViewBag.report = "report";
            Menupermission();
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
                            var logstatus = (from c in DbContext.tbl_DC_LoginStatus where c.Login_DateTime >= fdt && c.Login_DateTime <= tdt select c).OrderByDescending(c => c.Login_DateTime);
                            ViewBag.login_Status = logstatus.ToList();
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
            ViewBag.Breadcrumb = "Login Status Report";
            return View("Login_report");
        }

        #endregion

        #region----------------------------User group---------------------

        [HttpGet]
        public ActionResult Group()
        {
            ViewBag.setting = "setting";
            try
            {
                Menupermission();
                ViewBag.Breadcrumb = "User Group";
                var data = (from a in DbContext.tbl_DC_Role.Where(x => x.ROLE_CODE == "S" && x.IS_ACTIVE == true && x.IS_DELTED == false)
                            select new GroupModel
                            {
                                ROLE_ID = a.ROLE_ID,
                                ROLE_TYPE = a.ROLE_TYPE,
                                ROLE_CODE = a.ROLE_CODE,
                                INSERTED_DATE = a.INSERTED_DATE
                            }).OrderByDescending(x => x.ROLE_ID).ToList();
                ViewBag.Group = data;

                Menupermission();
                return View();
            }
            catch (Exception ex)
            {
                TempData["WarningMessage"] = "Something went wrong.";
            }
            return View();
        }
        [HttpGet]
        public ActionResult Addusergroup()
        {
            Menupermission();
            if (Convert.ToString(Session["ROLE_CODE"]) == "S")
            {
                var p_chk = DbContext.tbl_DC_Menu_Permission.Where(x => x.Menu_Key_ID == 1 && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                if (p_chk == null)
                {
                    return RedirectToAction("checkpermission");
                }
            }
            ViewBag.pagetitle = "Add";
            ViewBag.Breadcrumb = "User Group";
            //rackSpace_Api.getProvider();
            return View();

        }
        [HttpPost]
        public ActionResult Addusergroup(string Group_Name, string Group_id)
        {
            try
            {
                if (Group_Name.Trim() != "")
                {
                    if (Group_id == "")
                    {
                        ViewBag.pagetitle = "Add";
                        var board = DbContext.tbl_DC_Role.Where(x => x.ROLE_TYPE == Group_Name && x.IS_ACTIVE == true && x.IS_DELTED == false).FirstOrDefault();
                        if (board != null)
                        {
                            TempData["WarningMessage"] = "User group name already exist.";
                        }
                        else
                        {
                            tbl_DC_Role obj = new tbl_DC_Role();
                            obj.ROLE_TYPE = Group_Name;
                            obj.ROLE_CODE = "S";
                            obj.INSERTED_DATE = today;
                            obj.IS_ACTIVE = true;
                            obj.IS_DELTED = false;
                            DbContext.tbl_DC_Role.Add(obj);
                            DbContext.SaveChanges();
                            TempData["SuccessMessage"] = "User group added successfully.";
                            return RedirectToAction("Group");
                        }
                    }
                    else
                    {
                        ViewBag.pagetitle = "Update";
                        var board = DbContext.tbl_DC_Role.Where(x => x.ROLE_TYPE == Group_Name && x.IS_ACTIVE == true && x.IS_DELTED == false).FirstOrDefault();
                        if (board != null)
                        {
                            TempData["WarningMessage"] = "User group name already exists.";
                        }
                        else
                        {
                            int b_id = Convert.ToInt32(Group_id);
                            tbl_DC_Role obj = DbContext.tbl_DC_Role.Where(x => x.ROLE_ID == b_id).FirstOrDefault();
                            obj.ROLE_TYPE = Group_Name;
                            obj.MODIFIED_DATE = today;
                            DbContext.Entry(obj).State = EntityState.Modified;
                            DbContext.SaveChanges();
                            TempData["SuccessMessage"] = "User group updated successfully.";
                            return RedirectToAction("Group");
                        }
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "Please enter user group name.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return View();
        }
        public ActionResult EditGroup(int? id)
        {
            try
            {
                ViewBag.Breadcrumb = "Group";
                ViewBag.pagetitle = "Update";
                if (id != null)
                {
                    var data1 = (from a in DbContext.tbl_DC_Role.Where(x => x.ROLE_CODE == "S" && x.IS_ACTIVE == true && x.IS_DELTED == false)
                                 select new GroupModel
                                 {
                                     ROLE_ID = a.ROLE_ID,
                                     ROLE_TYPE = a.ROLE_TYPE
                                 }).ToList();

                    var data = DbContext.tbl_DC_Role.Where(x => x.ROLE_ID == id && x.IS_ACTIVE == true && x.IS_DELTED == false).FirstOrDefault();
                    if (data != null)
                    {
                        GroupModel obj = new GroupModel();
                        obj.ROLE_ID = data.ROLE_ID;
                        obj.ROLE_TYPE = data.ROLE_TYPE;
                        ViewBag.Group_Id = data.ROLE_ID;
                        ViewBag.Group_Name = data.ROLE_TYPE;
                        return View("Addusergroup", obj);
                    }
                    else
                    {
                        TempData["WarningMessage"] = "Invalid user group details.";
                    }
                }
                else
                {
                    TempData["WarningMessage"] = "Invalid user group details.";
                    return RedirectToAction("Group");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return View();
        }
        public ActionResult DeleteGroup(int? id)
        {
            try
            {
                if (id != null)
                {
                    var group = DbContext.tbl_DC_Role.Where(x => x.ROLE_ID == id && x.IS_ACTIVE == true && x.IS_DELTED == false).FirstOrDefault();
                    if (group == null)
                    {
                        var obj = DbContext.tbl_DC_Role.Where(x => x.ROLE_ID == id).FirstOrDefault();
                        obj.MODIFIED_DATE = today;
                        obj.IS_ACTIVE = false;
                        obj.IS_DELTED = true;
                        DbContext.Entry(obj).State = EntityState.Modified;
                        DbContext.SaveChanges();
                        TempData["SuccessMessage"] = "User group deleted successfully.";
                        return RedirectToAction("Group");

                    }
                    else
                    {
                        TempData["ErrorMessage"] = "User group can not be deleted because its in use.";
                    }

                }
                else
                {
                    TempData["ErrorMessage"] = "No data found";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return RedirectToAction("Group");
        }

        public class GroupModel
        {
            public int ROLE_ID { get; set; }
            public string ROLE_CODE { get; set; }
            public string ROLE_TYPE { get; set; }
            public Nullable<System.DateTime> INSERTED_DATE { get; set; }
            public string INSERTED_BY { get; set; }
            public Nullable<System.DateTime> MODIFIED_DATE { get; set; }
            public string MODIFIED_BY { get; set; }
            public Nullable<bool> IS_ACTIVE { get; set; }
            public Nullable<bool> IS_DELTED { get; set; }
        }

        #endregion

        #region--------------------------------------------------------Role Creation-----------------------------------------------------------

        [HttpGet]
        public ActionResult ViewRoleType()
        {
            ViewBag.setting = "setting";
            try
            {
                Menupermission();
                if (Convert.ToString(Session["ROLE_CODE"]) == "S")
                {
                    if (Session["Role_Id"] != null)
                    {
                        int id = Convert.ToInt32(Session["Role_Id"]);
                        var p_chk = DbContext.tbl_DC_Menu_Permission.Where(x => x.Menu_Key_ID == 3 && x.ROLE_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        if (p_chk == null)
                        {
                            return RedirectToAction("checkpermission");
                        }
                    }
                }
                ViewBag.Breadcrumb = "Role Assign";
                var data2 = (from c in DbContext.tbl_DC_Menu_Permission.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                             join e in DbContext.tbl_DC_Menuitems.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                             on c.Menu_Key_ID equals e.Menu_Key_ID
                             join d in DbContext.tbl_DC_Role.Where(x => x.IS_ACTIVE == true && x.IS_DELTED == false)
                             on c.ROLE_ID equals d.ROLE_ID
                             //join f in DbContext.tbl_DC_Role_Type.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                             //on c.Role_Type_ID equals f.Role_Type_ID
                             select new DigiChampsModel.MenuPermissionModel
                             {
                                 //Role_Type_ID = f.Role_Type_ID,
                                 //Role_Type_Name = f.Role_Type_Name,
                                 Permission_ID = c.Permission_ID,
                                 ROLE_TYPE = d.ROLE_TYPE,
                                 Menu_Parameter2 = e.Menu_Parameter2
                             }).ToList();
                ViewBag.details = data2;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return View();
        }

        [HttpGet]
        public ActionResult AssignRole()
        {
            ViewBag.setting = "setting";
            try
            {
                ViewBag.Breadcrumb = "Role Assign";
                if (Convert.ToString(Session["ROLE_CODE"]) == "S")
                {
                    if (Session["Role_Id"] != null)
                    {
                        int id = Convert.ToInt32(Session["Role_Id"]);
                        var p_chk = DbContext.tbl_DC_Menu_Permission.Where(x => x.Menu_Key_ID == 3 && x.ROLE_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        if (p_chk == null)
                        {
                            return RedirectToAction("checkpermission");
                        }
                    }
                }
                ViewBag.role = new SelectList(DbContext.tbl_DC_Role.Where(x => x.IS_ACTIVE == true && x.IS_DELTED == false && x.ROLE_CODE == "S"), "ROLE_ID", "ROLE_TYPE");


                var data = (from a in DbContext.tbl_DC_Menuitems.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                            select new DigiChampsModel.MenuItemModel
                            {
                                Menu_Key_ID = a.Menu_Key_ID,
                                Menu_Parameter1 = a.Menu_Parameter1,
                                Menu_Parameter2 = a.Menu_Parameter2,
                                Menu_Parameter3 = a.Menu_Parameter3
                            }).ToList();
                ViewBag.menuitems = data;


                var data1 = (from b in DbContext.tbl_DC_Role_Type.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                             select new DigiChampsModel.RoleTypeModel
                             {
                                 Role_Type_ID = b.Role_Type_ID,
                                 Role_Type_Name = b.Role_Type_Name
                             }).ToList();
                ViewBag.roletype = data1;

            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return View();
        }

        [HttpPost]
        public ActionResult AssignRole(DigiChampsModel.MenuPermissionModel obj, int[] chkmenu, int[] chkroletype)
        {
            ViewBag.setting = "setting";
            try
            {

                ViewBag.role = new SelectList(DbContext.tbl_DC_Role.Where(x => x.IS_ACTIVE == true && x.IS_DELTED == false && x.ROLE_CODE == "S"), "ROLE_ID", "ROLE_TYPE");

                tbl_DC_Menu_Permission obj1 = new tbl_DC_Menu_Permission();
                obj1.ROLE_ID = obj.ROLE_ID;
                int roleid = Convert.ToInt32(obj.ROLE_ID);
                if (chkmenu != null)
                {
                    var data = chkmenu.ToList();

                    //var data1 = chkroletype.ToList();

                    for (int i = 0; i < data.Count; i++)
                    {

                        int a = Convert.ToInt32(data.ToList()[i]);
                        var menu = DbContext.tbl_DC_Menu_Permission.Where(x => x.Menu_Key_ID == a && x.ROLE_ID == roleid && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        if (menu != null)
                        {
                            int menu_id = Convert.ToInt32(menu.Menu_Key_ID);
                            tbl_DC_Menu_Permission obj2 = DbContext.tbl_DC_Menu_Permission.Where(x => x.Menu_Key_ID == menu_id && x.ROLE_ID == roleid && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                            obj2.Is_Active = true;
                            obj2.Is_Deleted = false;
                            DbContext.Entry(obj2).State = EntityState.Modified;
                            DbContext.SaveChanges();
                        }
                        else
                        {
                            obj1.Menu_Key_ID = Convert.ToInt32(data.ToList()[i]);

                            //for (int j = 0; j < data1.Count; j++)
                            //{
                            //obj1.Role_Type_ID = Convert.ToInt32(data1.ToList()[j]);
                            obj1.Is_Active = true;
                            obj1.Is_Deleted = false;
                            obj1.Inserted_Date = DateTime.Now;
                            DbContext.tbl_DC_Menu_Permission.Add(obj1);
                            DbContext.SaveChanges();
                            //}
                        }
                    }
                    TempData["SuccessMessage"] = "Role type added successfully";
                }
                else
                {
                    if (chkmenu == null)
                    {
                        TempData["Message1"] = "Please select any menu item";
                    }
                    //if (chkroletype == null)
                    //{
                    //    TempData["Message2"] = "Please select any role type";
                    //}
                    return RedirectToAction("AssignRole");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return RedirectToAction("ViewRoleType");
        }

        public ActionResult DeletePermission(int? id)
        {
            ViewBag.setting = "setting";
            try
            {
                tbl_DC_Menu_Permission obj = DbContext.tbl_DC_Menu_Permission.Where(x => x.Permission_ID == id).FirstOrDefault();
                obj.Is_Active = false;
                obj.Is_Deleted = true;
                DbContext.Entry(obj).State = EntityState.Modified;
                DbContext.SaveChanges();
                TempData["SuccessMessage"] = "Menu permission deleted successfully";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return RedirectToAction("ViewRoleType");
        }
        #endregion

        #region------------------------------------------------------Tax Master----------------------------------------------------------------
        [HttpGet]
        public ActionResult ViewTaxMaster()
        {
            ViewBag.viewtaxtypemaster = "active";
            ViewBag.general = "general";
            try
            {
                Menupermission();
                if (Convert.ToString(Session["ROLE_CODE"]) == "S")
                {
                    if (Session["Role_Id"] != null)
                    {
                        int id = Convert.ToInt32(Session["Role_Id"]);
                        var p_chk = DbContext.tbl_DC_Menu_Permission.Where(x => x.Menu_Key_ID == 9 && x.ROLE_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        if (p_chk == null)
                        {
                            return RedirectToAction("checkpermission");
                        }
                    }
                }
                ViewBag.Breadcrumb = "Tax";
                if (Session["USER_CODE"] != null)
                {
                    ViewBag.servicetype = new SelectList(DbContext.tbl_DC_Tax_Type_Master.Where(x => x.Is_Active == true && x.Is_Deleted == false), "TaxType_ID", "Tax_Type");

                    var data = (from a in DbContext.tbl_DC_Tax_Master.OrderByDescending(x => x.Tax_ID)
                                join b in DbContext.tbl_DC_Tax_Type_Master.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                on a.TaxType_ID equals b.TaxType_ID
                                where (a.Is_Active == true && a.Is_Deleted == false)
                                select new DigiChampsModel.DigiChampsTaxMasterModel
                                {
                                    Tax_ID = a.Tax_ID,
                                    Tax_Type = b.Tax_Type,
                                    Tax_Rate = a.Tax_Rate,
                                    TAX_Efect_Date = a.TAX_Efect_Date
                                }).ToList().OrderByDescending(x => x.Tax_ID);
                    ViewBag.taxrate = data;
                }
                else
                {
                    return RedirectToAction("Login");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return View();
        }
        [HttpGet]
        public ActionResult TaxMaster()
        {
            //ViewBag.report = "report";
            ViewBag.general = "general";
            ViewBag.viewtaxtypemaster = "active";
            Menupermission();
            if (Convert.ToString(Session["ROLE_CODE"]) == "S")
            {
                if (Session["Role_Id"] != null)
                {
                    int id = Convert.ToInt32(Session["Role_Id"]);
                    var p_chk = DbContext.tbl_DC_Menu_Permission.Where(x => x.Menu_Key_ID == 9 && x.ROLE_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    if (p_chk == null)
                    {
                        return RedirectToAction("checkpermission");
                    }
                }
            }
            ViewBag.Breadcrumb = "Tax";
            ViewBag.servicetype = new SelectList(DbContext.tbl_DC_Tax_Type_Master.Where(x => x.Is_Active == true && x.Is_Deleted == false), "TaxType_ID", "Tax_Type");
            return View();
        }

        [HttpPost]
        public ActionResult TaxMaster(DigiChampsModel.DigiChampsTaxMasterModel obj, string hdtaxrateid, string TAX_Efect_Date)
        {
            //ViewBag.report = "report";
            ViewBag.general = "general";
            ViewBag.viewtaxtypemaster = "active";
            try
            {
                if (ModelState.IsValid)
                {
                    Menupermission();
                    int rid = 0;
                    if (hdtaxrateid != "")
                    {
                        rid = Convert.ToInt32(hdtaxrateid);
                    }
                    ViewBag.servicetype = new SelectList(DbContext.tbl_DC_Tax_Type_Master.Where(x => x.Is_Active == true && x.Is_Deleted == false), "TaxType_ID", "Tax_Type");

                    if (TAX_Efect_Date != "")
                    {

                        if (Session["USER_CODE"] != null)
                        {
                            if (rid <= 0)
                            {
                                if (Convert.ToDateTime(TAX_Efect_Date) >= today.Date)
                                {
                                    var data1 = DbContext.tbl_DC_Tax_Master.Where(x => x.TaxType_ID == obj.TaxType_ID && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                                    if (data1 != null)
                                    {
                                        TempData["WarningMessage"] = "Tax type already exist.";
                                    }
                                    else
                                    {
                                        tbl_DC_Tax_Master obj1 = new tbl_DC_Tax_Master();
                                        obj1.TaxType_ID = obj.TaxType_ID;
                                        obj1.Tax_Rate = obj.Tax_Rate;
                                        obj1.TAX_Efect_Date = obj.TAX_Efect_Date;
                                        obj1.Is_Active = true;
                                        obj1.Is_Deleted = false;
                                        obj1.Inserted_Date = DateTime.Now;
                                        obj1.Inserted_By = HttpContext.User.Identity.Name;
                                        DbContext.tbl_DC_Tax_Master.Add(obj1);
                                        DbContext.SaveChanges();
                                        TempData["SuccessMessage"] = "Tax master inserted successfully.";
                                        return RedirectToAction("ViewTaxMaster");
                                    }
                                }
                                else
                                {
                                    TempData["WarningMessage"] = "Select tax effective date";
                                    return View();
                                }
                            }
                            else
                            {
                                tbl_DC_Tax_Master obj1 = DbContext.tbl_DC_Tax_Master.Where(x => x.Tax_ID == rid).FirstOrDefault();
                                obj1.TaxType_ID = obj.TaxType_ID;
                                obj1.Tax_Rate = obj.Tax_Rate;
                                obj1.TAX_Efect_Date = obj.TAX_Efect_Date;
                                obj1.Is_Active = true;
                                obj1.Is_Deleted = false;
                                obj1.Modified_Date = DateTime.Now;
                                obj.Modified_By = HttpContext.User.Identity.Name;
                                DbContext.Entry(obj1).State = EntityState.Modified;
                                DbContext.SaveChanges();
                                TempData["SuccessMessage"] = "Tax master updated successfully.";
                                return RedirectToAction("ViewTaxMaster");
                            }
                        }
                        else
                        {
                            return RedirectToAction("Logout");
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return RedirectToAction("TaxMaster", "Admin");
        }
        public ActionResult DeleteTaxRate(int? id)
        {
            //ViewBag.report = "report";
            ViewBag.general = "general";
            ViewBag.viewtaxtypemaster = "active";
            try
            {
                if (Session["USER_CODE"] != null)
                {
                    tbl_DC_Tax_Master obj1 = DbContext.tbl_DC_Tax_Master.Where(x => x.Tax_ID == id).FirstOrDefault();

                    obj1.Is_Active = false;
                    obj1.Is_Deleted = true;
                    obj1.Modified_Date = DateTime.Now;
                    DbContext.SaveChanges();
                    TempData["SuccessMessage"] = "Tax deleted successfully.";
                    return RedirectToAction("ViewTaxMaster");
                }
                else
                {
                    return RedirectToAction("Login");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return View("ViewTaxMaster");
        }
        public ActionResult EditTaxRate(int id)
        {
            //ViewBag.report = "report";
            ViewBag.general = "general";
            ViewBag.viewtaxtypemaster = "active";
            try
            {
                var data = (from a in DbContext.tbl_DC_Tax_Master.OrderByDescending(x => x.Tax_ID)
                            join b in DbContext.tbl_DC_Tax_Type_Master.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                            on a.TaxType_ID equals b.TaxType_ID
                            where (a.Is_Active == true && a.Is_Deleted == false)
                            select new DigiChampsModel.DigiChampsTaxMasterModel
                            {
                                Tax_ID = a.Tax_ID,
                                Tax_Type = b.Tax_Type,
                                Tax_Rate = a.Tax_Rate,
                                TAX_Efect_Date = a.TAX_Efect_Date
                            }).ToList();
                ViewBag.taxrate = data;

                var obj1 = (from a in DbContext.tbl_DC_Tax_Master.Where(x => x.Tax_ID == id)
                            join b in DbContext.tbl_DC_Tax_Type_Master
                            on a.TaxType_ID equals b.TaxType_ID
                            select new DigiChampsModel.DigiChampsTaxMasterModel
                            {
                                Tax_ID = a.Tax_ID,
                                TaxType_ID = a.TaxType_ID,
                                Tax_Type = b.Tax_Type,
                                Tax_Rate = a.Tax_Rate,
                                TAX_Efect_Date = a.TAX_Efect_Date
                            }).FirstOrDefault();
                DigiChampsModel.DigiChampsTaxMasterModel obj = new DigiChampsModel.DigiChampsTaxMasterModel();

                obj.Tax_ID = obj1.Tax_ID;
                obj.TaxType_ID = obj1.TaxType_ID;
                obj.Tax_Type = obj1.Tax_Type;
                obj.Tax_Rate = obj1.Tax_Rate;
                obj.TAX_Efect_Date = obj1.TAX_Efect_Date;
                ViewBag.taxratee = obj1.Tax_ID;
                ViewBag.efectdate = Convert.ToDateTime(obj1.TAX_Efect_Date).ToShortDateString();
                ViewBag.servicetype = new SelectList(DbContext.tbl_DC_Tax_Type_Master.Where(x => x.Is_Active == true && x.Is_Deleted == false), "TaxType_ID", "Tax_Type");

                return View("TaxMaster", obj);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return View();
        }
        #endregion

        #region-------------------------------------------------------TaxTypeMaster------------------------------------------------------------
        [HttpGet]
        public ActionResult ViewTaxTypeMaster()
        {
            //ViewBag.report = "report";
            ViewBag.general = "general";
            ViewBag.viewtaxtypemaster = "active";
            try
            {
                Menupermission();
                if (Convert.ToString(Session["ROLE_CODE"]) == "S")
                {
                    if (Session["Role_Id"] != null)
                    {
                        int id = Convert.ToInt32(Session["Role_Id"]);
                        var p_chk = DbContext.tbl_DC_Menu_Permission.Where(x => x.Menu_Key_ID == 9 && x.ROLE_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        if (p_chk == null)
                        {
                            return RedirectToAction("checkpermission");
                        }
                    }
                }
                ViewBag.Breadcrumb = "Tax Type";
                if (Session["USER_CODE"] != null)
                {

                    var data = (from a in DbContext.tbl_DC_Tax_Type_Master.OrderByDescending(x => x.TaxType_ID).Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                select new DigiChampsModel.DigiChampsTaxTypeMasterModel
                                {
                                    TaxType_ID = a.TaxType_ID,
                                    TAX_CODE = a.TAX_CODE,
                                    Tax_Type = a.Tax_Type,
                                    Tax_Type_Short = a.Tax_Type_Short
                                }).ToList().OrderByDescending(x => x.TaxType_ID);
                    ViewBag.taxtype = data;
                }
                else
                {
                    return RedirectToAction("Logout");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return View();
        }
        [HttpGet]
        public ActionResult TaxTypeMaster()
        {
            ViewBag.general = "general";
            ViewBag.viewtaxtypemaster = "active";
            Menupermission();
            if (Convert.ToString(Session["ROLE_CODE"]) == "S")
            {
                if (Session["Role_Id"] != null)
                {
                    int id = Convert.ToInt32(Session["Role_Id"]);
                    var p_chk = DbContext.tbl_DC_Menu_Permission.Where(x => x.Menu_Key_ID == 9 && x.ROLE_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    if (p_chk == null)
                    {
                        return RedirectToAction("checkpermission");
                    }
                }
            }
            ViewBag.Breadcrumb = "Tax Type";
            ViewBag.Taxcode = DbContext.Taxprefix_code().FirstOrDefault();
            return View();
        }
        [HttpPost]
        public ActionResult TaxTypeMaster(DigiChampsModel.DigiChampsTaxTypeMasterModel obj, string hdtaxtypeid)
        {
            ViewBag.general = "general";
            ViewBag.viewtaxtypemaster = "active";
            //ViewBag.report = "report";
            int taxtypeid = 0;
            if (hdtaxtypeid != "")
            {
                taxtypeid = Convert.ToInt32(hdtaxtypeid);
            }
            if (ModelState.IsValid)
            {
                try
                {
                    if (Session["USER_CODE"] != null)
                    {
                        if (taxtypeid <= 0)
                        {
                            var data1 = DbContext.tbl_DC_Tax_Type_Master.Where(x => x.Tax_Type == obj.Tax_Type && x.Is_Active == true).FirstOrDefault();
                            if (data1 == null)
                            {
                                var data2 = DbContext.tbl_DC_Tax_Type_Master.Where(x => x.Tax_Type_Short == obj.Tax_Type_Short && x.Is_Active == true).FirstOrDefault();
                                if (data2 == null)
                                {
                                    var data3 = DbContext.tbl_DC_Tax_Type_Master.Where(x => x.TAX_CODE == obj.TAX_CODE && x.Is_Active == true).FirstOrDefault();
                                    if (data3 == null)
                                    {
                                        tbl_DC_Tax_Type_Master obj1 = new tbl_DC_Tax_Type_Master();
                                        obj1.Tax_Type = obj.Tax_Type;
                                        obj1.Tax_Type_Short = obj.Tax_Type_Short;
                                        obj1.TAX_CODE = obj.TAX_CODE;
                                        obj1.Is_Active = true;
                                        obj1.Is_Deleted = false;
                                        obj1.Inserted_Date = DateTime.Now;
                                        obj1.Inserted_By = HttpContext.User.Identity.Name;
                                        DbContext.tbl_DC_Tax_Type_Master.Add(obj1);
                                        DbContext.SaveChanges();
                                        TempData["SuccessMessage"] = "Tax type inserted successfully.";
                                        return RedirectToAction("ViewTaxTypeMaster");
                                    }
                                    else
                                    {
                                        TempData["WarningMessage"] = "Tax code already exist.";
                                        return RedirectToAction("TaxTypeMaster");
                                    }
                                }
                                else
                                {
                                    TempData["WarningMessage"] = "Tax short name already exist.";
                                    return RedirectToAction("TaxTypeMaster");
                                }
                            }
                            else
                            {
                                TempData["WarningMessage"] = "Tax type already exist.";
                                return RedirectToAction("TaxTypeMaster");

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
                    TempData["ErrorMessage"] = "Something went wrong.";
                }
            }
            return View();
        }
        public ActionResult DeleteTaxType(int? id)
        {
            ViewBag.general = "general";
            ViewBag.viewtaxtypemaster = "active";
            //ViewBag.report = "report";
            try
            {
                if (Session["USER_CODE"] != null)
                {
                    tbl_DC_Tax_Type_Master obj1 = DbContext.tbl_DC_Tax_Type_Master.Where(x => x.TaxType_ID == id).FirstOrDefault();

                    obj1.Is_Active = false;
                    obj1.Is_Deleted = true;
                    obj1.Modified_Date = DateTime.Now;
                    DbContext.SaveChanges();
                    TempData["SuccessMessage"] = "Tax type deleted successfully";
                    return RedirectToAction("ViewTaxTypeMaster");
                }
                else
                {
                    return RedirectToAction("Login");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return View("ViewTaxTypeMaster");
        }
        #endregion

        #region -----------------------------------------------------------sales----------------------------------------------------------------
        [HttpGet]
        public ActionResult DailySales()
        {
            ViewBag.report = "report";
            ViewBag.DailySales = "active";
            Menupermission();
            if (Convert.ToString(Session["ROLE_CODE"]) == "S")
            {
                if (Session["Role_Id"] != null)
                {
                    int id = Convert.ToInt32(Session["Role_Id"]);
                    var p_chk = DbContext.tbl_DC_Menu_Permission.Where(x => x.Menu_Key_ID == 12 && x.ROLE_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    if (p_chk == null)
                    {
                        return RedirectToAction("checkpermission");
                    }
                }
            }
            ViewBag.Breadcrumb = "Daily Sales";
            DigiChampsModel.DigiChampsDailySalesModel obj = new DigiChampsModel.DigiChampsDailySalesModel();
            ViewBag.package_Id = new SelectList(DbContext.tbl_DC_Package.Where(x => x.Is_Active == true && x.Is_Deleted == false).OrderByDescending(x => x.Package_ID), "Package_Id", "Package_Name", obj.Package_ID);

            ViewBag.status = DbContext.Order_report().ToList();

            return View();
        }
        [HttpPost]
        public ActionResult DailySales(string frm_dt, string to_dt, string Order_No, int? Package_ID)
        {
            ViewBag.DailySales = "active";
            ViewBag.report = "report";
            try
            {
                if (frm_dt != "" && to_dt != "")
                {
                    if (Convert.ToDateTime(frm_dt) <= today.Date && Convert.ToDateTime(to_dt) <= today.Date)
                    {
                        string fdtt = frm_dt + " 00:00:00 AM";
                        string tdtt = to_dt + " 23:59:59 PM";

                        ViewBag.package_Id = new SelectList(DbContext.tbl_DC_Package.Where(x => x.Is_Active == true && x.Is_Deleted == false), "Package_Id", "Package_Name");
                        DateTime from_date1 = Convert.ToDateTime(fdtt);
                        DateTime to_date1 = Convert.ToDateTime(tdtt);
                        int pkg_id = Convert.ToInt32(Package_ID);
                        if (Order_No == "" && Package_ID == null)
                        {
                            ViewBag.status = DbContext.Order_report().Where(x => x.Inserted_Date >= from_date1 && x.Inserted_Date <= to_date1).ToList();
                        }
                        else if (Order_No != "" && Package_ID == null)
                        {
                            ViewBag.status = DbContext.Order_report().Where(x => x.Inserted_Date >= from_date1 && x.Inserted_Date <= to_date1 && x.Order_No == Order_No).ToList();
                        }
                        else if (Order_No == "" && Package_ID != null)
                        {
                            int pk_id = Convert.ToInt32(Package_ID);
                            ViewBag.status = DbContext.Order_report().Where(x => x.Inserted_Date >= from_date1 && x.Inserted_Date <= to_date1 && x.Package_ID == pk_id).ToList();
                        }
                        else if (Order_No != "" && Package_ID != null)
                        {
                            int pk_id = Convert.ToInt32(Package_ID);
                            ViewBag.status = DbContext.Order_report().Where(x => x.Inserted_Date >= from_date1 && x.Inserted_Date <= to_date1 && x.Package_ID == pk_id && x.Order_No == Order_No).ToList();
                        }
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "From date should be less or equal to today date.";
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "Please Select Dates.";
                }

            }

            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }


            ViewBag.package_Id = new SelectList(DbContext.tbl_DC_Package.Where(x => x.Is_Active == true && x.Is_Deleted == false).OrderByDescending(x => x.Package_ID), "Package_Id", "Package_Name");
            return View();
        }
        #endregion

        #region----------------------------------------------------------startegic-------------------------------------------------------------
        [HttpGet]
        public ActionResult TopicStartegic(string id)
        {
            ViewBag.setting = "setting";
            Menupermission();
            ViewBag.Breadcrumb = "Topic strategic";
            ViewBag.pagetitle = "Add";
            ViewBag.getalltopic = DbContext.tbl_DC_Topic.Where(x => x.Is_Active == true && x.Is_Deleted == false).ToList();
            ViewBag.percentage = DbContext.tbl_DC_Percentage.Where(x => x.Is_Active == true && x.Is_Deleted == false).ToList();
            if (id != null)
            {
                try
                {
                    ViewBag.pagetitle = "Update";
                    int _id = Convert.ToInt32(id);
                    var get_all = DbContext.tbl_DC_Topic_Report.Where(x => x.Report_ID == _id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    if (get_all != null)
                    {
                        ViewBag.Topicname_p = get_all.Topic_ID;
                        ViewBag.start_p = get_all.Percentage_id;
                        ViewBag.remark_p = get_all.Remark;
                        ViewBag.hid = get_all.Report_ID;
                    }
                    else
                    {
                        ViewBag.Breadcrumb = "Topic Startegic";
                        TempData["ErrorMessage"] = "Topic strategic not found.";
                        return RedirectToAction("ShowTopicStartegic");

                    }

                }
                catch (Exception ex)
                {
                    ViewBag.Breadcrumb = "Topic strategic";
                    TempData["ErrorMessage"] = "Something went wrong.";
                    return RedirectToAction("ShowTopicStartegic");

                }
            }
            return View();
        }
        [HttpPost]
        public ActionResult TopicStartegic(string topic_id, string remark, int percentage, string hide_id)
        {
            ViewBag.setting = "setting";
            if (Session["USER_CODE"] != null)
            {
                int _id = Convert.ToInt32(Session["USER_CODE"].ToString().Trim().Substring(1));
                try
                {
                    var _find_percen = DbContext.tbl_DC_Percentage.Where(x => x.ID == percentage && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    if (_find_percen != null)
                    {
                        string start = Convert.ToString(_find_percen.Start_P);
                        string end = Convert.ToString(_find_percen.END_P);
                        if (topic_id != "" && start != "" && end != "" && remark != "")
                        {
                            UInt32 i;

                            if (UInt32.TryParse(start, out i) && UInt32.TryParse(end, out i) && UInt32.TryParse(topic_id, out i))
                            {

                                if ((Convert.ToInt32(start) != Convert.ToInt32(end)) && (Convert.ToInt32(start) < Convert.ToInt32(end)))
                                {
                                    if (hide_id == "")
                                    {
                                        int _star = Convert.ToInt32(start);
                                        int _end = Convert.ToInt32(end);
                                        int _tid = Convert.ToInt32(topic_id);
                                        var find_rep = DbContext.tbl_DC_Topic_Report.Where(x => x.Start_Percentage == _star && x.End_Percentage == _end && x.Is_Active == true && x.Is_Deleted == false && x.Topic_ID == _tid).FirstOrDefault();
                                        if (find_rep == null)
                                        {
                                            tbl_DC_Topic_Report _topic_rep = new tbl_DC_Topic_Report();
                                            _topic_rep.Topic_ID = Convert.ToInt32(topic_id);
                                            _topic_rep.Start_Percentage = Convert.ToInt32(start);
                                            _topic_rep.End_Percentage = Convert.ToInt32(end);
                                            _topic_rep.Remark = remark;
                                            _topic_rep.Inserted_By = _id;
                                            _topic_rep.Inserted_Date = today;
                                            _topic_rep.Percentage_id = percentage;
                                            _topic_rep.Is_Active = true;
                                            _topic_rep.Is_Deleted = false;
                                            DbContext.tbl_DC_Topic_Report.Add(_topic_rep);
                                            DbContext.SaveChanges();
                                            TempData["SuccessMessage"] = "Topic strategic added successfully.";
                                            return RedirectToAction("ShowTopicStartegic");
                                        }
                                        else
                                        {
                                            TempData["ErrorMessage"] = "strategic report already available.";
                                            return RedirectToAction("TopicStartegic");
                                        }

                                    }
                                    else
                                    {
                                        int id = Convert.ToInt32(hide_id);
                                        tbl_DC_Topic_Report _tbl = DbContext.tbl_DC_Topic_Report.Where(x => x.Report_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                                        if (_tbl != null)
                                        {
                                            _tbl.Start_Percentage = Convert.ToInt32(start);
                                            _tbl.Topic_ID = Convert.ToInt32(topic_id);
                                            _tbl.End_Percentage = Convert.ToInt32(end);
                                            _tbl.Percentage_id = percentage;
                                            _tbl.Remark = remark;
                                            _tbl.Modified_By = HttpContext.User.Identity.Name;
                                            _tbl.Modified_Date = today;
                                            DbContext.SaveChanges();
                                            TempData["SuccessMessage"] = "Topic strategic updated successfully .";
                                            return RedirectToAction("ShowTopicStartegic");
                                        }
                                        else
                                        {
                                            TempData["ErrorMessage"] = "Please try again.";
                                            return RedirectToAction("TopicStartegic");
                                        }
                                    }
                                }
                                else
                                {
                                    TempData["ErrorMessage"] = "Start percentage should be less.";
                                    return RedirectToAction("TopicStartegic");
                                }
                            }
                            else
                            {
                                TempData["ErrorMessage"] = "Percentage should be number.";
                                return RedirectToAction("TopicStartegic");
                            }
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "Provide data properly.";
                            return RedirectToAction("TopicStartegic");
                        }
                    }
                    else//not found percentagre
                    {
                        TempData["ErrorMessage"] = "Provide data properly.";
                        return RedirectToAction("TopicStartegic");
                    }
                }
                catch (Exception ex)
                {

                    TempData["ErrorMessage"] = "Something went wrong.";
                    return RedirectToAction("TopicStartegic");
                }

            }
            else
            {
                return RedirectToAction("Logout", "Admin");
            }
        }

        [HttpGet]
        public ActionResult Deletestartegic(int id)
        {
            ViewBag.setting = "setting";
            try
            {
                tbl_DC_Topic_Report _tbl = DbContext.tbl_DC_Topic_Report.Where(x => x.Report_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                if (_tbl != null)
                {
                    _tbl.Is_Active = false;
                    _tbl.Is_Deleted = true;
                    _tbl.Modified_By = HttpContext.User.Identity.Name;
                    _tbl.Modified_Date = today;
                    DbContext.SaveChanges();
                    TempData["SuccessMessage"] = "Topic strategic deleted successfuly.";
                    return RedirectToAction("ShowTopicStartegic");
                }
                else
                {
                    TempData["ErrorMessage"] = "Please try again.";
                    return RedirectToAction("ShowTopicStartegic");
                }

            }
            catch (Exception es)
            {

                TempData["ErrorMessage"] = "Something went wrong.";
                return RedirectToAction("ShowTopicStartegic");
            }
        }
        public ActionResult ShowTopicStartegic()
        {
            ViewBag.setting = "setting";
            ViewBag.Topicstartegic = DbContext.tbl_DC_Topic_Report.Where(x => x.Is_Active == true && x.Is_Deleted == false).OrderByDescending(x => x.Report_ID).ToList();
            return View();
        }

        #endregion

        #region--------------------------------------------------------Order details-----------------------------------------------------------
        [HttpGet]
        public ActionResult orderdetails()
        {
            ViewBag.report = "report";
            ViewBag.orderdetails = "active";
            Menupermission();
            if (Convert.ToString(Session["ROLE_CODE"]) == "S")
            {
                if (Session["Role_Id"] != null)
                {
                    int id = Convert.ToInt32(Session["Role_Id"]);
                    var p_chk = DbContext.tbl_DC_Menu_Permission.Where(x => x.Menu_Key_ID == 13 && x.ROLE_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    if (p_chk == null)
                    {
                        return RedirectToAction("checkpermission");
                    }
                }
            }
            ViewBag.Breadcrumb = "order details";
            IEnumerable<View_DC_Order_Details> ord_detail = DbContext.View_DC_Order_Details.OrderByDescending(x => x.Order_ID).ToList();
            return View(ord_detail);
        }

        [HttpGet]
        public ActionResult userorder(int? id)
        {
            ViewBag.orderdetails = "active";
            ViewBag.report = "report";
            ViewBag.Breadcrumb = "User Order";
            var regid = DbContext.tbl_DC_Order.Where(x => x.Order_ID == id).Select(x => x.Regd_ID).FirstOrDefault();
            var userorder = DbContext.SP_DC_Order_Details(regid).Where(x => x.Order_ID == id).FirstOrDefault();
            ViewBag.userorder = DbContext.SP_DC_Order_Details(regid).Where(x => x.Order_ID == id).ToList();
            var data = DbContext.View_DC_Order_Details.Where(x => x.Order_ID == id).FirstOrDefault();
            if (data != null)
            {
                ViewBag.totalprice = data.Total;
                ViewBag.nos = data.No_of_Package;
            }
            if (userorder != null)
            {
                return View(userorder);
            }
            else
            {
                TempData["WarningMessage"] = "No order details to show.";
                return View();
            }
        }
        #endregion

        #region----------------------------------------------------Prebook-User Report---------------------------------------------------------
        [HttpGet]
        public ActionResult prebook_user()
        {
            ViewBag.report = "report";
            ViewBag.prebook_user = "active";
            Menupermission();
            if (Convert.ToString(Session["ROLE_CODE"]) == "S")
            {
                if (Session["Role_Id"] != null)
                {
                    int id = Convert.ToInt32(Session["Role_Id"]);
                    var p_chk = DbContext.tbl_DC_Menu_Permission.Where(x => x.Menu_Key_ID == 10 && x.ROLE_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    if (p_chk == null)
                    {
                        return RedirectToAction("checkpermission");
                    }
                }
            }
            ViewBag.Breadcrumb = "Pre-Book Customers";
            IEnumerable<VW_Prebook_Students> pre = DbContext.VW_Prebook_Students.OrderByDescending(x => x.Book_Id).ToList();
            return View(pre);
        }

        [HttpPost]
        public ActionResult prebook_user(string f_Date, string t_Date)
        {
            ViewBag.prebook_user = "active";
            ViewBag.report = "report";
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
                            ViewBag.Breadcrumb = "Pre-Book Customers";
                            IEnumerable<VW_Prebook_Students> pre = (from c in DbContext.VW_Prebook_Students where c.Inserted_Date >= fdt && c.Inserted_Date <= tdt select c).OrderByDescending(c => c.Inserted_Date);
                            return View("prebook_user", pre);
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


            return RedirectToAction("prebook_user");
        }

        #endregion

        #region---------------------------------------------------------User Report------------------------------------------------------------
        [HttpGet]
        public ActionResult UserReport()
        {
            ViewBag.report = "report";
            ViewBag.UserReport = "active";
            Menupermission();
            if (Convert.ToString(Session["ROLE_CODE"]) == "S")
            {
                if (Session["Role_Id"] != null)
                {
                    int id = Convert.ToInt32(Session["Role_Id"]);
                    var p_chk = DbContext.tbl_DC_Menu_Permission.Where(x => x.Menu_Key_ID == 14 && x.ROLE_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    if (p_chk == null)
                    {
                        return RedirectToAction("checkpermission");
                    }
                }
            }
            ViewBag.Breadcrumb = "Users Report";
            IEnumerable<View_All_Student_Details> students = DbContext.View_All_Student_Details.OrderByDescending(x => x.Regd_ID).ToList();
            return View(students);
        }
        [HttpGet]
        public ActionResult ViewStudent(int? id)
        {
            ViewBag.UserReport = "active";
            ViewBag.report = "report";
            Menupermission();
            if (Convert.ToString(Session["ROLE_CODE"]) == "S")
            {
                if (Session["Role_Id"] != null)
                {
                    int id1 = Convert.ToInt32(Session["Role_Id"]);
                    var p_chk = DbContext.tbl_DC_Menu_Permission.Where(x => x.Menu_Key_ID == 14 && x.ROLE_ID == id1 && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    if (p_chk == null)
                    {
                        return RedirectToAction("checkpermission");
                    }
                }
            }
            ViewBag.Breadcrumb = "Student details";
            View_All_Student_Details students = DbContext.View_All_Student_Details.Where(x => x.Regd_ID == id).FirstOrDefault();
            if (students.Image != null)
            {
                ViewBag.pfimage = students.Image;
            }
            return View(students);
        }
        [HttpGet]
        public ActionResult Blockuser(int? id)
        {
            ViewBag.UserReport = "active";
            ViewBag.report = "report";
            tbl_DC_Registration student = DbContext.tbl_DC_Registration.Where(x => x.Regd_ID == id).FirstOrDefault();
            if (student != null)
            {
                var obj = DbContext.tbl_DC_USER_SECURITY.Where(x => x.USER_NAME == student.Mobile).FirstOrDefault();
                if (obj.IS_ACCEPTED == false)
                {
                    obj.IS_ACCEPTED = true;
                    TempData["SuccessMessage"] = "User is unblocked successfully.";
                }
                else
                {
                    obj.IS_ACCEPTED = false;
                    TempData["SuccessMessage"] = "User is blocked successfully.";
                }
                DbContext.Entry(obj).State = EntityState.Modified;
                DbContext.SaveChanges();

            }
            return RedirectToAction("UserReport");
        }
        #endregion

        #region-------------------------------------------------------Feedback Report----------------------------------------------------------
        [HttpGet]
        public ActionResult ViewFeedback()
        {
            ViewBag.report = "report";
            ViewBag.ViewFeedback = "active";
            Menupermission();
            if (Convert.ToString(Session["ROLE_CODE"]) == "S")
            {
                if (Session["Role_Id"] != null)
                {
                    int id = Convert.ToInt32(Session["Role_Id"]);
                    var p_chk = DbContext.tbl_DC_Menu_Permission.Where(x => x.Menu_Key_ID == 17 && x.ROLE_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    if (p_chk == null)
                    {
                        return RedirectToAction("checkpermission");
                    }
                }
            }
            ViewBag.Breadcrumb = "Feedback Report";
            ViewBag.ch_feed = DbContext.SP_DC_Student_Feedback_Chap("C").ToList();
            ViewBag.Teach_feed = DbContext.SP_DC_Student_Feedback_Teach("T").ToList();


            ViewBag.feedback = DbContext.SP_DC_feedback().ToList();
            return View();
        }
        #endregion

        #region----------------------------------------------------Teacher Report--------------------------------------------------------------
        [HttpGet]
        public ActionResult TeacherTicket(int? id)
        {
            ViewBag.report = "report";
            ViewBag.ViewTeacherReportDetail = "active";
            try
            {
                Menupermission();
                if (Convert.ToString(Session["ROLE_CODE"]) == "S")
                {
                    if (Session["Role_Id"] != null)
                    {
                        int id1 = Convert.ToInt32(Session["Role_Id"]);
                        var p_chk = DbContext.tbl_DC_Menu_Permission.Where(x => x.Menu_Key_ID == 15 && x.ROLE_ID == id1 && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        if (p_chk == null)
                        {
                            return RedirectToAction("checkpermission");
                        }
                    }
                }
                ViewBag.Breadcrumb = "Teacher Ticket";
                overdue();
                var data = DbContext.View_DC_Tickets_and_Teacher.Where(x => x.Teach_ID == id).OrderByDescending(x => x.Inserted_Date).ToList();
                ViewBag.teacher_id = id;
                ViewBag.Ticket = data;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return View();
        }

        public ActionResult TeacherAnswerTicket(int id)
        {
            ViewBag.report = "report";
            ViewBag.ViewTeacherReportDetail = "active";
            Menupermission();
            ViewBag.Breadcrumb = "Teacher Answer Ticket";

            if (id != null)
            {
                ViewBag.viewticket = DbContext.View_DC_All_Tickets_Details.Where(x => x.Ticket_ID == id).ToList();
            }
            return View();
        }

        [HttpGet]
        public ActionResult ViewTeacherReportDetail()
        {
            ViewBag.report = "report";
            ViewBag.ViewTeacherReportDetail = "active";
            Menupermission();
            ViewBag.Breadcrumb = "Teacher Ticket Report";
            try
            {
                // int _Teacher_id = Convert.ToInt32(Convert.ToString(Session["USER_CODE"]).Substring(1));


                var users = DbContext.SP_DC_Teacher_Ticket_Count().ToList();

                ViewBag.Teacher = users;

            }
            catch (Exception)
            {
                return RedirectToAction("TeacherTicket", "Admin");
            }
            return View();
        }

        [HttpGet]
        public ActionResult TeacherAnswerTickett(int? id)
        {
            ViewBag.report = "report";
            ViewBag.ViewTeacherReportDetail = "active";
            Menupermission();
            if (Session["USER_CODE"] != null)
            {
                try
                {
                    ViewBag.Breadcrumb = "Teacher Answer Ticket";
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
                        return RedirectToAction("TeacherTicket", "Admin");
                    }
                }
                catch (Exception ex)
                {

                    TempData["ErrorMessage"] = "Something went wrong.";
                    return RedirectToAction("TeacherTicket", "Admin");
                }
            }
            else
            {
                return RedirectToAction("Logout", "Admin");
            }

            return View();
        }


        [HttpPost]
        public ActionResult Teacher_ticket_report(string f_Date, string t_Date, int teach_id)
        {
            ViewBag.ViewTeacherReportDetail = "active";
            ViewBag.report = "report";
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
                            var logstatus = (from c in DbContext.View_DC_Tickets_and_Teacher where c.Inserted_Date >= fdt && c.Inserted_Date <= tdt && c.Teach_ID == teach_id select c).ToList().OrderByDescending(x => x.Inserted_Date).OrderByDescending(x => x.Ticket_ID);
                            ViewBag.Ticket = logstatus.ToList();
                            ViewBag.Breadcrumb = "Teacher Ticket Report";
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
            ViewBag.teacher_id = teach_id;
            return View("TeacherTicket");
        }
        #endregion

        #region----------------------------------------------------Menu Permission-------------------------------------------------------------
        public void Menupermission()
        {
            try
            {
                if (Convert.ToString(Session["ROLE_CODE"]) == "S")
                {
                    if (Session["Role_Id"] != null)
                    {
                        int roleid = Convert.ToInt32(Session["Role_Id"]);
                        var permission = (from c in DbContext.tbl_DC_Menuitems.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                          join a in DbContext.tbl_DC_Menu_Permission.Where(x => x.ROLE_ID == roleid && x.Is_Active == true && x.Is_Deleted == false)
                                          on c.Menu_Key_ID equals a.Menu_Key_ID
                                          join b in DbContext.tbl_DC_Role.Where(x => x.ROLE_CODE == "S" && x.IS_ACTIVE == true && x.IS_DELTED == false)
                                          on a.ROLE_ID equals b.ROLE_ID
                                          select new DigiChampsModel.MenuPermissionModel
                                          {
                                              Menu_Parameter1 = c.Menu_Parameter1,
                                              Menu_Parameter2 = c.Menu_Parameter2,
                                              Menu_Parameter3 = c.Menu_Parameter3,
                                              Role_Type_ID = a.Role_Type_ID,
                                              ROLE_ID = b.ROLE_ID,
                                              ROLE_CODE = b.ROLE_CODE,
                                              ROLE_TYPE = b.ROLE_TYPE,
                                              Menu_Key_ID = a.Menu_Key_ID
                                          }).ToList();
                        ViewBag.permission1 = permission.Count;
                        ViewBag.permission = permission;
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
        }
        #endregion

        #region--------------------------------------------------------Centre------------------------------------------------------------------
        [HttpGet]
        public ActionResult CentreMaster()
        {
            ViewBag.setting = "setting";
            Menupermission();
            if (Convert.ToString(Session["ROLE_CODE"]) == "S")
            {
                if (Session["Role_Id"] != null)
                {
                    int id = Convert.ToInt32(Session["Role_Id"]);
                    var p_chk = DbContext.tbl_DC_Menu_Permission.Where(x => x.Menu_Key_ID == 1 && x.ROLE_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    if (p_chk == null)
                    {
                        return RedirectToAction("checkpermission");
                    }
                }
            }
            ViewBag.Breadcrumb = "Centre";
            ViewBag.State_Id = new SelectList(DbContext.tbl_DC_State.OrderByDescending(x => x.State_Id).Where(x => x.Is_Active == true && x.Is_Deleted == false), "State_Id", "State_Name");

            var data = (from a in DbContext.tbl_DC_Centre.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                        select new DigiChampsModel.DigiChampsCentreModel
                        {
                            Centre_Id = a.Centre_Id,
                            Centre_Name = a.Centre_Name,
                            Centre_Code = a.Centre_Code,
                            Address_Line_1 = a.Address_Line_1,
                            Pin_Code = a.Pin_Code
                        }).OrderByDescending(x => x.Centre_Id).ToList();
            ViewBag.Centre = data;
            return View();
        }
        [HttpGet]
        public ActionResult AddCentre()
        {
            ViewBag.setting = "setting";
            Menupermission();
            if (Convert.ToString(Session["ROLE_CODE"]) == "S")
            {
                if (Session["Role_Id"] != null)
                {
                    int id = Convert.ToInt32(Session["Role_Id"]);
                    var p_chk = DbContext.tbl_DC_Menu_Permission.Where(x => x.Menu_Key_ID == 1 && x.ROLE_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    if (p_chk == null)
                    {
                        return RedirectToAction("checkpermission");
                    }
                }
            }
            ViewBag.State_Id = new SelectList(DbContext.tbl_DC_State.OrderByDescending(x => x.State_Id).Where(x => x.Is_Active == true && x.Is_Deleted == false), "State_Id", "State_Name");
            ViewBag.Breadcrumb = "Centre";
            ViewBag.pagetitle = "Add";
            return View();
        }
        [HttpPost]
        public ActionResult AddCentre(string Centre_Name, string centre_id, string Centre_Code, string Address1, string Address2, string pincode, DigiChampsModel.DigiChampsCentreModel center)
        {
            ViewBag.setting = "setting";
            try
            {
                Menupermission();
                ViewBag.State_Id = new SelectList(DbContext.tbl_DC_State.OrderByDescending(x => x.State_Id).Where(x => x.Is_Active == true && x.Is_Deleted == false), "State_Id", "State_Name");

                if (Centre_Name.Trim() != "" && Centre_Code.Trim() != "")
                {
                    if (centre_id == "")
                    {
                        ViewBag.pagetitle = "Add";
                        var board = DbContext.tbl_DC_Centre.Where(x => x.Centre_Name == Centre_Name && x.Centre_Code == Centre_Code && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        if (board != null)
                        {
                            TempData["WarningMessage"] = "Centre name already exist.";
                        }
                        else
                        {
                            tbl_DC_Centre obj = new tbl_DC_Centre();
                            obj.Centre_Name = Centre_Name;
                            obj.Centre_Code = Centre_Code;
                            obj.Address_Line_1 = Address1;
                            obj.Address_Line_2 = Address2;
                            obj.Pin_Code = pincode;
                            obj.State_Id = center.State_Id;
                            obj.City_Id = center.City_Id;
                            obj.Inserted_Date = today;
                            obj.Is_Active = true;
                            obj.Is_Deleted = false;
                            DbContext.tbl_DC_Centre.Add(obj);
                            DbContext.SaveChanges();
                            TempData["SuccessMessage"] = "Centre added successfully.";
                            return RedirectToAction("CentreMaster");
                        }
                    }
                    else
                    {
                        ViewBag.pagetitle = "Update";
                        //var centre = DbContext.tbl_DC_Centre.Where(x => x.Centre_Name == Centre_Name && x.Centre_Code == Centre_Code && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        //if (centre != null)
                        //{
                        //    TempData["WarningMessage"] = "Centre name already exists.";
                        //}
                        //else
                        //{
                        int c_id = Convert.ToInt32(centre_id);
                        tbl_DC_Centre obj = DbContext.tbl_DC_Centre.Where(x => x.Centre_Id == c_id).FirstOrDefault();
                        obj.Centre_Name = Centre_Name;
                        obj.Centre_Code = Centre_Code;
                        obj.Address_Line_1 = Address1;
                        obj.Address_Line_2 = Address2;
                        obj.Pin_Code = pincode;
                        obj.State_Id = center.State_Id;
                        obj.City_Id = center.City_Id;
                        obj.Modified_Date = today;
                        DbContext.Entry(obj).State = EntityState.Modified;
                        DbContext.SaveChanges();
                        TempData["SuccessMessage"] = "Centre updated successfully.";
                        return RedirectToAction("CentreMaster");
                        //}
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "Please enter centre name and centre code";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return View();
        }
        public ActionResult EditCentre(int? id)
        {
            ViewBag.setting = "setting";
            try
            {
                ViewBag.State_Id = new SelectList(DbContext.tbl_DC_State.OrderByDescending(x => x.State_Id).Where(x => x.Is_Active == true && x.Is_Deleted == false), "State_Id", "State_Name");
                ViewBag.Breadcrumb = "Centre";
                ViewBag.pagetitle = "Update";
                if (id != null)
                {
                    var data1 = (from a in DbContext.tbl_DC_Centre.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                 select new DigiChampsModel.DigiChampsCentreModel
                                 {
                                     Centre_Id = a.Centre_Id,
                                     Centre_Name = a.Centre_Name,
                                     Centre_Code = a.Centre_Code
                                 }).ToList();
                    if (data1 != null)
                    {
                        ViewBag.Centre = data1;
                    }
                    else
                    {
                        TempData["WarningMessage"] = "Invalid centre details.";
                    }
                    var data = DbContext.tbl_DC_Centre.Where(x => x.Centre_Id == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    int c_id = Convert.ToInt32(data.City_Id);
                    var city = DbContext.tbl_DC_City.Where(x => x.City_Id == c_id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    int s_id = Convert.ToInt32(data.State_Id);
                    var state = DbContext.tbl_DC_State.Where(x => x.State_Id == s_id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    if (data != null)
                    {
                        DigiChampsModel.DigiChampsCentreModel obj = new DigiChampsModel.DigiChampsCentreModel();
                        obj.Centre_Id = data.Centre_Id;
                        obj.Centre_Name = data.Centre_Name;
                        obj.Centre_Code = data.Centre_Code;
                        obj.City_Id = Convert.ToInt32(city.City_Id);
                        obj.State_Id = Convert.ToInt32(state.State_Id);
                        obj.Address_Line_1 = data.Address_Line_1;
                        obj.Address_Line_2 = data.Address_Line_2;
                        obj.Pin_Code = data.Pin_Code;
                        ViewBag.pin = obj.Pin_Code;
                        ViewBag.address1 = obj.Address_Line_1;
                        ViewBag.address2 = obj.Address_Line_2;
                        ViewBag.Centre_Id = obj.Centre_Id;
                        ViewBag.Centre_Name = data.Centre_Name;
                        ViewBag.Centre_Code = data.Centre_Code;
                        ViewBag.State_name = new SelectList(DbContext.tbl_DC_State.Where(x => x.State_Id == s_id && x.Is_Active == true && x.Is_Deleted == false), "State_Id", "State_Name");
                        ViewBag.city_Name = new SelectList(DbContext.tbl_DC_City.Where(x => x.City_Id == c_id && x.Is_Active == true && x.Is_Deleted == false), "City_Id", "City_Name");
                        return View("AddCentre", obj);
                    }
                    else
                    {
                        TempData["WarningMessage"] = "Invalid centre details.";
                    }
                }
                else
                {
                    TempData["WarningMessage"] = "Invalid centre details.";
                    return RedirectToAction("CentreMaster");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return View();
        }
        public ActionResult DeleteCentre(int? id)
        {
            ViewBag.setting = "setting";
            try
            {
                if (id != null)
                {
                    var obj = DbContext.tbl_DC_Centre.Where(x => x.Centre_Id == id).FirstOrDefault();
                    obj.Modified_Date = today;
                    obj.Is_Active = false;
                    obj.Is_Deleted = true;
                    DbContext.Entry(obj).State = EntityState.Modified;
                    DbContext.SaveChanges();
                    TempData["SuccessMessage"] = "Centre deleted successfully.";
                    return RedirectToAction("CentreMaster");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return View();
        }
        [HttpPost]
        public ActionResult GetCity(int StaId)
        {
            List<SelectListItem> CityNames = new List<SelectListItem>();
            List<tbl_DC_City> states = DbContext.tbl_DC_City.Where(x => x.State_Id == StaId && x.Is_Active == true && x.Is_Deleted == false).ToList();
            states.ForEach(x =>
            {
                CityNames.Add(new SelectListItem { Text = x.City_Name, Value = x.City_Id.ToString() });
            });
            //ViewBag.state = new SelectList(dbContext.tbl_JV_State.Where(x => x.FK_Country_ID == conId && x.Is_Active == true && x.Is_Deleted == false), "PK_State_ID", "State_Name");

            return Json(CityNames, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region---------------------------------------------------------Tablet-----------------------------------------------------------------

        [HttpGet]
        public ActionResult CreateTablet(int? id)
        {
            ViewBag.Breadcrumb = "Tablet";
            Menupermission();
            if (Convert.ToString(Session["ROLE_CODE"]) == "S")
            {
                if (Session["Role_Id"] != null)
                {
                    int id1 = Convert.ToInt32(Session["Role_Id"]);
                    var p_chk = DbContext.tbl_DC_Menu_Permission.Where(x => x.Menu_Key_ID == 4 && x.ROLE_ID == id1 && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    if (p_chk == null)
                    {
                        return RedirectToAction("checkpermission");
                    }
                }
            }
            Session["editpckgdtls"] = null;
            ViewBag.hidden = "hidden";
            ViewBag.title = "Create";
            List<tbl_DC_Tablet_Technical_Details> ci = new List<tbl_DC_Tablet_Technical_Details> { new tbl_DC_Tablet_Technical_Details { Tech_Detail_Id = 0, Technical_Name = "", Technical_Desc = "" } };
            if (id != null)
            {
                var data = DbContext.tbl_DC_Tablet_Purchase.Where(x => x.Tablet_Id == id).Where(x => x.Is_Active == true && x.Is_Deleted == false).ToList().FirstOrDefault();
                // ViewBag.Boarddetails = DbContext.tbl_DC_Board.Where(x => x.Is_Active == true && x.Is_Deleted == false).ToList();
                List<tbl_DC_Tablet_Technical_Details> obj = DbContext.tbl_DC_Tablet_Technical_Details.Where(x => x.Tablet_Id == id && x.Is_Active == true && x.Is_Deleted == false).ToList();
                if (data != null)
                {
                    ViewBag.hbid = data.Tablet_Id;
                    ViewBag.package_name = data.Tablet_Name;
                    ViewBag.Package_Desc = data.Tablet_Description;
                    ViewBag.sub_period = data.Subscription_Period;
                    decimal price = Decimal.Round(Convert.ToDecimal(data.Tablet_Price), 2);
                    ViewBag.price = price;
                    ViewBag.OtherDeatls = data.Other_Details;
                    //ViewBag.profimage = data.Thumbnail;
                    //ViewBag.sub_limit = data.Total_Chapter;
                    ViewBag.tabimages = DbContext.tbl_DC_Tablet_Image.Where(x => x.Tablet_Id == id && x.Is_Active == true && x.Is_Deleted == false).OrderByDescending(x => x.Tablet_Image_Id).ToList();
                    ViewBag.title = "Edit";
                    if (ViewBag.tabimages.Count != 0)
                    {
                        ViewBag.hidden = null;
                    }
                    var obj1 = DbContext.tbl_DC_PackageSub_Dtl.Where(x => x.Package_ID == id && x.Is_Active == true && x.Is_Deleted == false).ToList();
                }
                else
                {
                    return RedirectToAction("CreateTablet");
                }
                return View(obj);
            }
            else
            {
                return View(ci);
            }
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult CreateTablet(string chk_pac, string tablet_name, string Desc_body, string Price, string Package_Limit, string Subscripttion_Period, string Package_Total, string Board_Id, string Class_Id, string[] module, string hpid, string pkid, string[] Qphotos, string Other_Desc, List<tbl_DC_Tablet_Technical_Details> obj)
        {
            string module_data = string.Empty;
            string mods_id = string.Empty;
            try
            {
                decimal pkg_price;
                int ls_umber;

                if (hpid == "")
                {
                    #region Insert_Tablet
                    if (tablet_name == "" || Price == "" || Subscripttion_Period == "" || Board_Id == "" || Class_Id == "" || int.TryParse(Subscripttion_Period, out ls_umber) != true || decimal.TryParse(Price, out pkg_price) != true || Desc_body == "")
                    {
                        TempData["ErrorMessage"] = "Enter Data Properly.";

                        return RedirectToAction("CreateTablet", "Admin", new { chk_pac = chk_pac, Package_Name = tablet_name, Package_Desc = Desc_body, Price = Price, Package_Limit = Package_Limit, Subscripttion_Period = Subscripttion_Period, Package_Total = Package_Total, Board_Id = Board_Id, Class_Id = Class_Id, mods_id = mods_id });
                    }
                    else
                    {
                        tbl_DC_Tablet_Purchase tblpr = new tbl_DC_Tablet_Purchase();
                        tblpr.Tablet_Name = tablet_name;
                        tblpr.Tablet_Price = Convert.ToDecimal(Price);
                        tblpr.Tablet_Description = Desc_body;
                        //tblpr.Discount_Amount = 
                        tblpr.Subscription_Period = Convert.ToInt32(Subscripttion_Period);
                        tblpr.Other_Details = Other_Desc;
                        tblpr.Is_Active = true;
                        tblpr.Is_Deleted = false;
                        tblpr.Inserted_By = HttpContext.User.Identity.Name;
                        DbContext.tbl_DC_Tablet_Purchase.Add(tblpr);
                        DbContext.SaveChanges();

                        foreach (var item in obj)
                        {
                            tbl_DC_Tablet_Technical_Details tbltech = new tbl_DC_Tablet_Technical_Details();

                            tbltech.Tablet_Id = tblpr.Tablet_Id;
                            tbltech.Technical_Name = item.Technical_Name;
                            tbltech.Technical_Desc = item.Technical_Desc;
                            tbltech.Is_Active = true;
                            tbltech.Is_Deleted = false;
                            DbContext.tbl_DC_Tablet_Technical_Details.Add(tbltech);
                            DbContext.SaveChanges();
                        }
                        if (Qphotos != null)
                        {
                            for (int i = 0; i < Qphotos.Length; i++)
                            {
                                tbl_DC_Tablet_Image tbletimg = new tbl_DC_Tablet_Image();
                                tbletimg.Tablet_Id = tblpr.Tablet_Id;
                                if (Qphotos[i] != null)
                                {
                                    string[] image1 = Qphotos[i].ToString().Split(',');
                                    string name = Convert.ToString(tblpr.Tablet_Id + "" + today.ToShortTimeString() + "" + i);
                                    name = name.Replace(':', '_');
                                    Base64ToImage_tblet(name, image1[1]);
                                    Qphotos[i] = name + ".jpg";
                                    tbletimg.Tablet_Image = Qphotos[i].ToString();
                                }
                                tbletimg.Is_Active = true;
                                tbletimg.Is_Deleted = false;
                                DbContext.tbl_DC_Tablet_Image.Add(tbletimg);
                                DbContext.SaveChanges();
                            }
                        }
                        TempData["SuccessMessage"] = "Tablet details inserted successfully.";
                    }
                    #endregion
                }
                else
                {
                    #region update_Tablet
                    int tblet_id = Convert.ToInt32(hpid);
                    tbl_DC_Tablet_Purchase tbltab = DbContext.tbl_DC_Tablet_Purchase.Where(x => x.Tablet_Id == tblet_id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    if (tbltab != null)
                    {
                        tbltab.Tablet_Name = tablet_name;
                        tbltab.Tablet_Description = Desc_body;
                        tbltab.Tablet_Price = Convert.ToDecimal(Price);
                        tbltab.Subscription_Period = Convert.ToInt32(Subscripttion_Period);
                        tbltab.Other_Details = Other_Desc;
                        tbltab.Inserted_By = HttpContext.User.Identity.Name;
                        tbltab.Modified_Date = today;
                        DbContext.Entry(tbltab).State = EntityState.Modified;
                        DbContext.SaveChanges();

                        //First is_active false for all
                        List<tbl_DC_Tablet_Technical_Details> detailids = DbContext.tbl_DC_Tablet_Technical_Details.Where(x => x.Tablet_Id == tblet_id && x.Is_Active == true && x.Is_Deleted == false).ToList();
                        for (int j = 0; j < detailids.Count; j++)
                        {
                            int id = detailids.ToList()[j].Tech_Detail_Id;
                            tbl_DC_Tablet_Technical_Details tbltech = DbContext.tbl_DC_Tablet_Technical_Details.Where(x => x.Tablet_Id == tblet_id && x.Tech_Detail_Id == id).FirstOrDefault();
                            tbltech.Is_Active = false;
                            tbltech.Is_Deleted = true;
                            DbContext.Entry(tbltech).State = EntityState.Modified;
                            DbContext.SaveChanges();
                        }

                        //Then modify which exists
                        for (int i = 0; i < obj.Count; i++)
                        {
                            int? tecid = obj.ToList()[i].Tech_Detail_Id;
                            if (tecid != null && tecid != 0)
                            {
                                tbl_DC_Tablet_Technical_Details tbltech = DbContext.tbl_DC_Tablet_Technical_Details.Where(x => x.Tablet_Id == tblet_id && x.Tech_Detail_Id == tecid).FirstOrDefault();
                                if (obj.ToList()[i].Technical_Name != null)
                                {
                                    tbltech.Technical_Name = obj.ToList()[i].Technical_Name;
                                    tbltech.Technical_Desc = obj.ToList()[i].Technical_Desc;
                                    tbltech.Is_Active = true;
                                    tbltech.Is_Deleted = false;
                                    tbltech.Modified_Date = today;
                                    DbContext.Entry(tbltech).State = EntityState.Modified;
                                    DbContext.SaveChanges();
                                }
                            }
                            else
                            {
                                tbl_DC_Tablet_Technical_Details tbldtls = new tbl_DC_Tablet_Technical_Details();
                                int tblet_id1 = Convert.ToInt32(hpid);
                                tbldtls.Tablet_Id = tblet_id1;
                                tbldtls.Technical_Name = obj.ToList()[i].Technical_Name;
                                tbldtls.Technical_Desc = obj.ToList()[i].Technical_Desc;
                                tbldtls.Is_Active = true;
                                tbldtls.Is_Deleted = false;
                                tbldtls.Inserted_Date = today;
                                DbContext.tbl_DC_Tablet_Technical_Details.Add(tbldtls);
                                DbContext.SaveChanges();
                            }
                        }

                        if (Qphotos != null)
                        {
                            for (int i = 0; i < Qphotos.Length; i++)
                            {
                                if (Qphotos[i] != null)
                                {
                                    tbl_DC_Tablet_Image tbletimg1 = new tbl_DC_Tablet_Image();
                                    tbletimg1.Tablet_Id = Convert.ToInt32(hpid);
                                    string[] image1 = Qphotos[i].ToString().Split(',');
                                    string name = Convert.ToString(hpid + "" + today.ToShortTimeString() + "" + i);
                                    name = name.Replace(':', '_');
                                    Base64ToImage_tblet(name, image1[1]);
                                    Qphotos[i] = name + ".jpg";
                                    tbletimg1.Tablet_Image = Qphotos[i].ToString();
                                    tbletimg1.Is_Active = true;
                                    tbletimg1.Is_Deleted = false;
                                    tbletimg1.Inserted_Date = today;
                                    DbContext.tbl_DC_Tablet_Image.Add(tbletimg1);
                                    DbContext.SaveChanges();
                                }
                            }
                        }
                        TempData["SuccessMessage"] = "Tablet details updated successfully.";
                        return RedirectToAction("ViewTablet");
                    }
                    else
                    {
                        TempData["WarningMessage"] = "Invalid tablet details.";
                    }
                    #endregion
                }
            }

            catch
            {
                TempData["ErrorMessage"] = "Somethimg went wrong.";
            }
            return RedirectToAction("ViewTablet");
        }
        public ActionResult ViewTablet()
        {
            if (Convert.ToString(Session["ROLE_CODE"]) == "S")
            {
                if (Session["Role_Id"] != null)
                {
                    int id = Convert.ToInt32(Session["Role_Id"]);
                    var p_chk = DbContext.tbl_DC_Menu_Permission.Where(x => x.Menu_Key_ID == 4 && x.ROLE_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    if (p_chk == null)
                    {
                        return RedirectToAction("checkpermission");
                    }
                }
            }
            ViewBag.Breadcrumb = "Tablet";
            ViewBag.Tabletdtls = DbContext.tbl_DC_Tablet_Purchase.Where(x => x.Is_Active == true && x.Is_Deleted == false).OrderByDescending(x => x.Tablet_Id).ToList();
            return View();

        }

        public Image Base64ToImage_tblet(string packname, string base64String)
        {
            // Convert base 64 string to byte[]
            byte[] imageBytes = Convert.FromBase64String(base64String);
            // Convert byte[] to Image
            using (var ms = new MemoryStream(imageBytes, 0, imageBytes.Length))
            {
                string filename = packname + ".jpg";
                Image image = Image.FromStream(ms, true);
                var img = new Bitmap(Image.FromStream(ms));
                string tempFolderName = Server.MapPath("~/Images/Tablet/" + filename);
                image.Save(tempFolderName, ImageFormat.Jpeg);
                return image;
            }

        }
        public ActionResult Delete_Tablet(int? id)
        {
            try
            {
                if (id != null)
                {
                    tbl_DC_Tablet_Purchase obj = DbContext.tbl_DC_Tablet_Purchase.Where(x => x.Tablet_Id == id).FirstOrDefault();
                    obj.Modified_Date = today;
                    obj.Is_Active = false;
                    obj.Is_Deleted = true;
                    //obj.Modified_By = 
                    DbContext.Entry(obj).State = EntityState.Modified;
                    DbContext.SaveChanges();

                    TempData["SuccessMessage"] = "Tablet deleted successfully.";
                    return RedirectToAction("ViewTablet");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }

            return RedirectToAction("ViewTablet");
        }

        [HttpPost]
        public JsonResult delete_tab_img(int? img_id)
        {
            string msg = string.Empty;
            try
            {
                if (img_id != null)
                {
                    tbl_DC_Tablet_Image dq = DbContext.tbl_DC_Tablet_Image.Where(x => x.Tablet_Image_Id == img_id).FirstOrDefault();
                    if (dq != null)
                    {
                        dq.Is_Active = false;
                        dq.Is_Deleted = true;
                        dq.Inserted_By = HttpContext.User.Identity.Name;
                        dq.Inserted_Date = today;
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
        #endregion

        #region-------------------------------------------------------Batch Assign-------------------------------------------------------------
        [HttpGet]
        public ActionResult TeacherBatchAssign()
        {
            ViewBag.setting = "setting";
            try
            {
                Menupermission();
                if (Convert.ToString(Session["ROLE_CODE"]) == "S")
                {
                    if (Session["Role_Id"] != null)
                    {
                        int id = Convert.ToInt32(Session["Role_Id"]);
                        var p_chk = DbContext.tbl_DC_Menu_Permission.Where(x => x.Menu_Key_ID == 1 && x.ROLE_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        if (p_chk == null)
                        {
                            return RedirectToAction("checkpermission");
                        }
                    }
                }
                ViewBag.Breadcrumb = "Assign Batch";
                ViewBag.Assign_Details = DbContext.SP_DC_Assign_Batch_Details().OrderByDescending(x => x.Batch_Assign_Id).ToList();
                ViewBag.teacherbatch = DbContext.tbl_DC_Batch_Assign.Where(x => x.Is_Active == true && x.Is_Deleted == false).ToList();
                return View();
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return View();
        }

        [HttpGet]
        public ActionResult AddBatchAssign(int? id)
        {
            ViewBag.setting = "setting";
            try
            {
                ViewBag.Breadcrumb = "Assign Batch";
                Menupermission();
                if (Convert.ToString(Session["ROLE_CODE"]) == "S")
                {
                    if (Session["Role_Id"] != null)
                    {
                        int id1 = Convert.ToInt32(Session["Role_Id"]);
                        var p_chk = DbContext.tbl_DC_Menu_Permission.Where(x => x.Menu_Key_ID == 1 && x.ROLE_ID == id1 && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        if (p_chk == null)
                        {
                            return RedirectToAction("checkpermission");
                        }
                    }
                }
                if (id != null)
                {
                    var batch_data = DbContext.SP_DC_Assign_Batch_Details().Where(x => x.Batch_Assign_Id == id).ToList();
                    ViewBag.batch = batch_data.Count();
                    ViewBag.boarddetails = new SelectList(DbContext.tbl_DC_Board.Where(x => x.Is_Active == true && x.Is_Deleted == false).ToList(), "Board_Id", "Board_Name", batch_data[0].Board_Id.ToString());
                    if (batch_data.Count > 0)
                    {
                        ViewBag.batch_data = batch_data.ToList();
                        ViewBag.batch_id = batch_data[0].Batch_Id;
                        ViewBag.Boardid = batch_data[0].Board_Id;
                        ViewBag.subid = batch_data[0].Subject_Id;
                        ViewBag.classid = batch_data[0].Class_Id;
                        ViewBag.teachid = batch_data[0].Teach_ID;
                        ViewBag.Batch_Assignids = id;
                        ViewBag.Batch_Assignid = DbContext.tbl_DC_Batch_Assign.Where(x => x.Is_Active == true && x.Is_Deleted == false).ToList();
                    }


                    return View();
                }
                else
                {
                    ViewBag.batch = 0;
                    ViewBag.Board_Id = new SelectList(DbContext.tbl_DC_Board.Where(x => x.Is_Active == true && x.Is_Deleted == false), "Board_Id", "Board_Name");
                    ViewBag.Batch_Assignid = DbContext.tbl_DC_Batch_Assign.Where(x => x.Is_Active == true && x.Is_Deleted == false).ToList();

                }
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }

            return View();
        }

        [HttpPost]
        public ActionResult AddBatchAssign(int? Teacher_Id, int[] chk_Id, int? id, int? Board_Id, int? ddlclass, int? ddlsubject, int? Batch_Id)
        {
            ViewBag.setting = "setting";
            try
            {
                if (id == null)
                {
                    int teacherid = Convert.ToInt32(Teacher_Id);

                    var data1 = DbContext.tbl_DC_Batch_Assign.Where(x => x.Batch_Id == Batch_Id && x.Class_Id == ddlclass && x.Teach_ID == teacherid && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    if (data1 != null)
                    {
                        TempData["WarningMessage"] = "Teacher already assigned to same batch.";
                        return RedirectToAction("TeacherBatchAssign");
                    }
                    else
                    {
                        tbl_DC_Batch_Assign obj = new tbl_DC_Batch_Assign();
                        var get_t = DbContext.tbl_DC_Batch_Assign.Where(x => x.Batch_Id == Batch_Id).FirstOrDefault();
                        if (get_t == null)
                        {
                            obj.Board_Id = Board_Id;
                            obj.Class_Id = ddlclass;
                            obj.Subject_Id = ddlsubject;
                            obj.Batch_Id = Convert.ToInt32(Batch_Id);
                            obj.Teach_ID = teacherid;
                            obj.Inserted_Date = today;
                            obj.Inserted_By = HttpContext.User.Identity.Name;
                            obj.Is_Active = true;
                            obj.Is_Deleted = false;
                            DbContext.tbl_DC_Batch_Assign.Add(obj);
                            DbContext.SaveChanges();
                            TempData["SuccessMessage"] = "Batch assigned to Teacher successfully.";
                        }
                        else
                        {
                            int teacher_id = Convert.ToInt32(get_t.Teach_ID);
                            var t = DbContext.tbl_DC_Teacher.Where(x => x.Teach_ID == teacher_id).FirstOrDefault();
                            TempData["WarningMessage"] = "Batch already assigned to" + t.Teacher_Name + ".";

                        }



                    }

                    return RedirectToAction("TeacherBatchAssign");
                }
                else
                {
                    var btassign = DbContext.tbl_DC_Batch_Assign.Where(x => x.Batch_Assign_Id == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    if (btassign != null)
                    {

                    }
                    else
                    {
                        TempData["WarningMessage"] = "Assigned batch does not exist.";
                        return RedirectToAction("TeacherBatchAssign");
                    }
                }

            }

            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }

            return RedirectToAction("TeacherBatchAssign");
        }

        [HttpPost]
        public ActionResult AddBatchAssign_stu(int[] chk_Id, int Batch_teach_Id)
        {
            ViewBag.setting = "setting";
            try
            {
                tbl_DC_Student_Batch_Assign sba = new tbl_DC_Student_Batch_Assign();
                List<int> li = new List<int>();
                List<int> li2 = new List<int>(chk_Id.ToList());


                var data = DbContext.tbl_DC_Student_Batch_Assign.Where(x => x.Batch_Assign_Id == Batch_teach_Id).ToList();
                for (int j = 0; j < data.Count; j++)
                {
                    li.Add(Convert.ToInt32(data[j].Regd_Id));
                }
                int[] result = li.Except(li2).Concat(li2.Except(li)).ToArray();
                bool b = li2.Count < li.Count;
                if (li2.Count < li.Count)
                {
                    for (int k = 0; k < result.Length; k++)
                    {
                        int redgid = Convert.ToInt32(result[k]);
                        var find_students = DbContext.tbl_DC_Student_Batch_Assign.Where(x => x.Regd_Id == redgid && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        DbContext.tbl_DC_Student_Batch_Assign.Remove(find_students);
                        DbContext.SaveChanges();
                    }
                }

                for (int i = 0; i < chk_Id.Length; i++)
                {
                    int rid = Convert.ToInt32(chk_Id[i]);

                    var find_student = DbContext.tbl_DC_Student_Batch_Assign.Where(x => x.Regd_Id == rid && x.Batch_Assign_Id == Batch_teach_Id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    if (find_student == null)
                    {

                    }
                    else
                    {
                        DbContext.tbl_DC_Student_Batch_Assign.Remove(find_student);
                        DbContext.SaveChanges();
                    }

                    sba.Batch_Assign_Id = Batch_teach_Id;
                    sba.Regd_Id = chk_Id[i];
                    sba.Is_Active = true;
                    sba.Is_Deleted = false;
                    sba.Inserted_Date = today;
                    sba.Inserted_By = HttpContext.User.Identity.Name;
                    DbContext.tbl_DC_Student_Batch_Assign.Add(sba);
                    DbContext.SaveChanges();
                    if (b == true)
                    {
                        TempData["SuccessMessage"] = "Unselect Student removed and new Student Added Successfully.";
                    }
                    else
                    {
                        TempData["SuccessMessage"] = "Student Added Successfully.";
                    }

                }
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return RedirectToAction("TeacherBatchAssign", "Admin");
        }

        public ActionResult Delete_BatchAssign(int? id)
        {
            ViewBag.setting = "setting";
            try
            {
                if (id != null)
                {
                    tbl_DC_Batch_Assign obj = DbContext.tbl_DC_Batch_Assign.Where(x => x.Batch_Assign_Id == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    obj.Is_Active = false;
                    obj.Is_Deleted = true;
                    obj.Modified_By = HttpContext.User.Identity.Name;
                    obj.Modified_Date = today;
                    DbContext.Entry(obj).State = EntityState.Modified;
                    DbContext.SaveChanges();
                    TempData["SuccessMessage"] = "Batch deleted successfully.";
                    return RedirectToAction("TeacherBatchAssign");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return View();
        }

        public ActionResult Delete_teach(int? id)
        {
            ViewBag.setting = "setting";
            try
            {
                if (id != null)
                {
                    var find_teacher = DbContext.tbl_DC_Batch_Assign.Where(x => x.Batch_Assign_Id == id).FirstOrDefault();
                    if (find_teacher != null)
                    {
                        find_teacher.Is_Active = false;
                        find_teacher.Is_Deleted = true;
                        DbContext.SaveChanges();
                        TempData["SuccessMessage"] = "Teacher batch deleted successfully.";
                        return RedirectToAction("TeacherBatchAssign");
                    }
                    else
                    {

                        TempData["ErrorMessage"] = "No data found.";

                        return RedirectToAction("TeacherBatchAssign");
                    }
                }
                else
                {

                    TempData["ErrorMessage"] = "No data found.";

                    return RedirectToAction("TeacherBatchAssign");
                }
            }
            catch (Exception)
            {

                TempData["ErrorMessage"] = "Something went wrong.";
                return RedirectToAction("TeacherBatchAssign");
            }

        }


        #endregion

        #region----------------------------------------------------------Batch-----------------------------------------------------------------
        [HttpGet]
        public ActionResult BatchMaster()
        {
            ViewBag.setting = "setting";
            try
            {
                Menupermission();
                if (Convert.ToString(Session["ROLE_CODE"]) == "S")
                {
                    if (Session["Role_Id"] != null)
                    {
                        int id = Convert.ToInt32(Session["Role_Id"]);
                        var p_chk = DbContext.tbl_DC_Menu_Permission.Where(x => x.Menu_Key_ID == 1 && x.ROLE_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        if (p_chk == null)
                        {
                            return RedirectToAction("checkpermission");
                        }
                    }
                }
                ViewBag.Breadcrumb = "Batch";
                var data = (from a in DbContext.tbl_DC_Batch.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                            join b in DbContext.tbl_DC_Centre.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                             on a.Centre_Id equals b.Centre_Id
                            join c in DbContext.tbl_DC_Class.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                            on a.Class_Id equals c.Class_Id
                            join d in DbContext.tbl_DC_Subject.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                            on a.Subject_Id equals d.Subject_Id
                            join e in DbContext.tbl_DC_Board.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                            on a.Board_Id equals e.Board_Id
                            select new DigiChampsModel.DigiChampsBatchModel
                            {
                                Batch_Id = a.Batch_Id,
                                Board_Id = a.Board_Id,
                                Board_Name = e.Board_Name,
                                Class_Id = a.Class_Id,
                                Class_Name = c.Class_Name,
                                Subject_Id = a.Subject_Id,
                                Subject = d.Subject,
                                Centre_Id = a.Centre_Id,
                                Centre_Name = b.Centre_Name,
                                Batch_Name = a.Batch_Name,
                                Batch_From_Time = a.Batch_From_Time,
                                Batch_To_Time = a.Batch_To_Time,
                                Batch_Code = a.Batch_Code,
                                Batch_Days = a.Batch_Days
                            }).OrderByDescending(x => x.Batch_Id).ToList();
                ViewBag.Batch_Timing = data;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return View();
        }
        [HttpGet]
        public ActionResult AddBatch(int? id)
        {
            ViewBag.setting = "setting";
            try
            {
                Menupermission();
                if (Convert.ToString(Session["ROLE_CODE"]) == "S")
                {
                    if (Session["Role_Id"] != null)
                    {
                        int id1 = Convert.ToInt32(Session["Role_Id"]);
                        var p_chk = DbContext.tbl_DC_Menu_Permission.Where(x => x.Menu_Key_ID == 1 && x.ROLE_ID == id1 && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        if (p_chk == null)
                        {
                            return RedirectToAction("checkpermission");
                        }
                    }
                }
                ViewBag.centerdetails = DbContext.tbl_DC_Centre.Where(x => x.Is_Active == true && x.Is_Deleted == false).ToList();
                if (id != null)
                {
                    var batch_data = DbContext.tbl_DC_Batch.Where(x => x.Batch_Id == id).FirstOrDefault();
                    if (batch_data != null)
                    {
                        ViewBag.Boar_Id = new SelectList(DbContext.tbl_DC_Board.Where(x => x.Is_Active == true && x.Is_Deleted == false), "Board_Id", "Board_Name", Convert.ToString(batch_data.Board_Id));
                        ViewBag.board = batch_data.Board_Id;
                        ViewBag.Class_Name = batch_data.Class_Id;
                        ViewBag.Subject_Name = batch_data.Subject_Id;
                        ViewBag.Batch_Name = batch_data.Batch_Name;
                        ViewBag.Batch_To_Time = batch_data.Batch_To_Time;
                        ViewBag.Centre_Name = batch_data.Centre_Id;
                        ViewBag.Batch_From_Time = batch_data.Batch_From_Time;
                        ViewBag.Batch_Days = batch_data.Batch_Days;
                        ViewBag.Batch_Code = batch_data.Batch_Code;
                        ViewBag.batch_id = batch_data.Batch_Id;
                        ViewBag.Breadcrumb = "Edit Batch";
                    }
                    return View();
                }
                else
                {
                    ViewBag.Breadcrumb = " Create Batch";
                }
                ViewBag.Board_Id = new SelectList(DbContext.tbl_DC_Board.Where(x => x.Is_Active == true && x.Is_Deleted == false), "Board_Id", "Board_Name");

                ViewBag.classdetails = DbContext.tbl_DC_Class.Where(x => x.Is_Active == true && x.Is_Deleted == false).ToList();
                ViewBag.subjectdetails = DbContext.tbl_DC_Subject.Where(x => x.Is_Active == true && x.Is_Deleted == false).ToList();
            }
            catch (Exception ex)
            {

            }
            return View();
        }

        [HttpPost]
        public ActionResult AddBatch(string Batch_Name, string From_Time, string To_Time, int Centre_Name, int? hidden_id, string[] Batch_Days, string Batch_Code, int? Class_Id, int?[] Subject_Id, int? Board_Id)
        {
            ViewBag.setting = "setting";
            try
            {

                ViewBag.Board_Id = new SelectList(DbContext.tbl_DC_Board.Where(x => x.Is_Active == true && x.Is_Deleted == false), "Board_Id", "Board_Name");

                DateTime time1 = Convert.ToDateTime(From_Time);
                DateTime time2 = Convert.ToDateTime(To_Time);
                var get_batch_code = DbContext.tbl_DC_Batch.Where(x => x.Is_Active == true && x.Is_Deleted == false).ToList();
                if (time1 < time2)
                {

                    if (hidden_id != null)
                    {
                        var get_update = DbContext.tbl_DC_Batch.Where(x => x.Batch_Id == hidden_id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        int update_subject_id = Convert.ToInt32(get_update.Subject_Id);
                        string batchdays = get_update.Batch_Days;
                        if (get_update != null)
                        {
                            if (get_batch_code.Where(x => x.Subject_Id == update_subject_id && x.Batch_From_Time == From_Time && x.Batch_To_Time == To_Time && x.Batch_Id != hidden_id && x.Batch_Days == batchdays && x.Centre_Id == Centre_Name).FirstOrDefault() == null)
                            {
                                get_update.Batch_Name = Batch_Name;
                                get_update.Centre_Id = Centre_Name;
                                get_update.Batch_From_Time = From_Time;
                                get_update.Batch_To_Time = To_Time;
                                get_update.Batch_Code = Batch_Code;
                                DbContext.SaveChanges();
                                TempData["SuccessMessage"] = "Batch details updated successfully.";
                            }
                            else
                            {
                                TempData["ErrorMessage"] = "Batch with same subject and same time already exist.";
                            }
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "No batch found for update.";
                        }
                    }
                    else
                    {

                        int count = 0;
                        tbl_DC_Batch obj = new tbl_DC_Batch();
                        for (int i = 0; i < Subject_Id.Length; i++)
                        {
                            for (int j = 0; j < Batch_Days.Length; j++)
                            {
                                int subject = (int)Subject_Id[i];
                                string batchday = Batch_Days[j];
                                var batch_found = DbContext.tbl_DC_Batch.Where(x => x.Batch_From_Time == From_Time && x.Batch_To_Time == To_Time && x.Subject_Id == subject && x.Batch_Days == batchday && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();

                                if (batch_found == null)
                                {

                                    obj.Subject_Id = Subject_Id[i];
                                    obj.Batch_Days = Batch_Days[j];
                                    obj.Batch_Name = Batch_Name;
                                    obj.Batch_From_Time = From_Time;
                                    obj.Batch_To_Time = To_Time;
                                    obj.Centre_Id = Centre_Name;
                                    obj.Batch_Code = Batch_Code;
                                    obj.Class_Id = Class_Id;
                                    obj.Board_Id = Board_Id;
                                    obj.Inserted_Date = today;
                                    obj.Inserted_By = HttpContext.User.Identity.Name;
                                    obj.Is_Active = true;
                                    obj.Is_Deleted = false;
                                    DbContext.tbl_DC_Batch.Add(obj);
                                    DbContext.SaveChanges();
                                    if (count == 0)
                                    {
                                        TempData["SuccessMessage"] = "Batch details added successfully.";
                                    }
                                    else if (count > 0)
                                    {
                                        TempData["SuccessMessage"] = " unavailable Batch details added successfully.";
                                    }

                                }
                                else
                                {
                                    count = count + 1;
                                    TempData["ErrorMessage"] = "Batch already availabel.";
                                }

                            }
                        }

                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "Batch from time canot be less than to time.";
                }


            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return RedirectToAction("BatchMaster");
        }

        public ActionResult DeleteBatch(int? id)
        {
            ViewBag.setting = "setting";
            try
            {
                if (id != null)
                {
                    tbl_DC_Batch obj = DbContext.tbl_DC_Batch.Where(x => x.Batch_Id == id).FirstOrDefault();
                    obj.Is_Active = false;
                    obj.Is_Deleted = true;
                    obj.Modified_Date = today;
                    obj.Modified_By = HttpContext.User.Identity.Name;
                    DbContext.Entry(obj).State = EntityState.Modified;
                    DbContext.SaveChanges();
                    TempData["SuccessMessage"] = "Batch details deleted successfully.";
                    return RedirectToAction("BatchMaster");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return View();
        }
        #endregion

        #region------------------------------------------------------Attendance Report---------------------------------------------------------
        [HttpGet]
        public ActionResult Attendance_Report()
        {
            ViewBag.report = "report";
            ViewBag.Attendance_Report = "active";
            Menupermission();
            if (Convert.ToString(Session["ROLE_CODE"]) == "S")
            {
                if (Session["Role_Id"] != null)
                {
                    int id = Convert.ToInt32(Session["Role_Id"]);
                    var p_chk = DbContext.tbl_DC_Menu_Permission.Where(x => x.Menu_Key_ID == 16 && x.ROLE_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    if (p_chk == null)
                    {
                        return RedirectToAction("checkpermission");
                    }
                }
            }
            ViewBag.Breadcrumb = "Attendance Report";
            ViewBag.Attendance = DbContext.Vw_Attendance_Report.ToList();
            return View();
        }
        [HttpPost]
        public ActionResult Attendance_Report(string frm_dt, string to_dt)
        {
            ViewBag.Attendance_Report = "active";
            ViewBag.report = "report";
            try
            {
                ViewBag.Breadcrumb = "Attendance Report";
                Menupermission();
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

                            var attendance_status = (from c in DbContext.Vw_Attendance_Report where c.Attendance_Date >= from_date1 && c.Attendance_Date <= to_date1 select c).OrderByDescending(c => c.Attendance_Date);
                            if (attendance_status != null)
                            {
                                ViewBag.Attendance = attendance_status.ToList();
                            }
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

            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return View();
        }
        #endregion

        #region------------------------------------------------------- Exam Reports -----------------------------------------------------------
        [HttpGet]
        public ActionResult examreport()
        {
            ViewBag.report = "report";
            ViewBag.examreport = "active";
            try
            {
                if (Convert.ToString(Session["ROLE_CODE"]) == "S")
                {
                    if (Session["Role_Id"] != null)
                    {
                        int id = Convert.ToInt32(Session["Role_Id"]);
                        var p_chk = DbContext.tbl_DC_Menu_Permission.Where(x => x.Menu_Key_ID == 18 && x.ROLE_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        if (p_chk == null)
                        {
                            return RedirectToAction("checkpermission");
                        }
                    }
                }
                var data2 = (from m in DbContext.tbl_DC_Exam.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                             select new TestResult
                             {
                                 Exam_ID = m.Exam_ID,
                                 Exam_Name = m.Exam_Name
                             }).ToList();
                ViewBag.exam_name = data2;
                Menupermission();
                ViewBag.Breadcrumb = "Exam Report";
                var data1 = (from c in DbContext.tbl_DC_Registration.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                             join a in DbContext.tbl_DC_Exam_Result.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                             on c.Regd_ID equals a.Regd_ID
                             join b in DbContext.tbl_DC_Exam.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                             on a.Exam_ID equals b.Exam_ID
                             join d in DbContext.tbl_DC_Board.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                             on b.Board_Id equals d.Board_Id
                             join e in DbContext.tbl_DC_Class.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                             on b.Class_Id equals e.Class_Id
                             join f in DbContext.tbl_DC_Subject.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                             on b.Subject_Id equals f.Subject_Id
                             select new TestResult
                             {
                                 Customer_Name = c.Customer_Name,
                                 Exam_ID = b.Exam_ID,
                                 Result_ID = a.Result_ID,
                                 Exam_Name = b.Exam_Name,
                                 Board_Name = d.Board_Name,
                                 Class_Name = e.Class_Name,
                                 Subject = f.Subject,
                                 StartTime = a.StartTime,
                                 EndTime = a.EndTime,
                                 Question_Nos = a.Question_Nos,
                                 Question_Attempted = a.Question_Attempted,
                                 Total_Correct_Ans = a.Total_Correct_Ans
                             }).OrderByDescending(x => x.Result_ID).ToList();
                if (data1 != null)
                {
                    ViewBag.test_result_dtl = data1.ToList();
                }

            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return View();
        }
        [HttpPost]
        public ActionResult examreport(string ddlexam)
        {
            ViewBag.examreport = "active";
            ViewBag.report = "report";
            try
            {
                int id = Convert.ToInt32(ddlexam);
                var data2 = (from m in DbContext.tbl_DC_Exam.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                             select new TestResult
                             {
                                 Exam_ID = m.Exam_ID,
                                 Exam_Name = m.Exam_Name
                             }).ToList();
                ViewBag.exam_name = data2;
                if (id > 0)
                {
                    Menupermission();
                    ViewBag.Breadcrumb = "Exam Report";
                    var data1 = (from c in DbContext.tbl_DC_Registration.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                 join a in DbContext.tbl_DC_Exam_Result.Where(x => x.Exam_ID == id && x.Is_Active == true && x.Is_Deleted == false)
                                 on c.Regd_ID equals a.Regd_ID
                                 join b in DbContext.tbl_DC_Exam.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                 on a.Exam_ID equals b.Exam_ID
                                 join d in DbContext.tbl_DC_Board.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                 on b.Board_Id equals d.Board_Id
                                 join e in DbContext.tbl_DC_Class.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                 on b.Class_Id equals e.Class_Id
                                 join f in DbContext.tbl_DC_Subject.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                 on b.Subject_Id equals f.Subject_Id
                                 select new TestResult
                                 {
                                     Customer_Name = c.Customer_Name,
                                     Exam_ID = b.Exam_ID,
                                     Result_ID = a.Result_ID,
                                     Exam_Name = b.Exam_Name,
                                     Board_Name = d.Board_Name,
                                     Class_Name = e.Class_Name,
                                     Subject = f.Subject,
                                     StartTime = a.StartTime,
                                     EndTime = a.EndTime,
                                     Question_Nos = a.Question_Nos,
                                     Question_Attempted = a.Question_Attempted,
                                     Total_Correct_Ans = a.Total_Correct_Ans
                                 }).OrderByDescending(x => x.Result_ID).ToList();
                    if (data1 != null)
                    {
                        ViewBag.test_result_dtl = data1.ToList();
                    }
                }
                else
                {
                    Menupermission();
                    ViewBag.Breadcrumb = "Exam Report";
                    var data1 = (from c in DbContext.tbl_DC_Registration.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                 join a in DbContext.tbl_DC_Exam_Result.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                 on c.Regd_ID equals a.Regd_ID
                                 join b in DbContext.tbl_DC_Exam.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                 on a.Exam_ID equals b.Exam_ID
                                 join d in DbContext.tbl_DC_Board.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                 on b.Board_Id equals d.Board_Id
                                 join e in DbContext.tbl_DC_Class.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                 on b.Class_Id equals e.Class_Id
                                 join f in DbContext.tbl_DC_Subject.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                 on b.Subject_Id equals f.Subject_Id
                                 select new TestResult
                                 {
                                     Customer_Name = c.Customer_Name,
                                     Exam_ID = b.Exam_ID,
                                     Result_ID = a.Result_ID,
                                     Exam_Name = b.Exam_Name,
                                     Board_Name = d.Board_Name,
                                     Class_Name = e.Class_Name,
                                     Subject = f.Subject,
                                     StartTime = a.StartTime,
                                     EndTime = a.EndTime,
                                     Question_Nos = a.Question_Nos,
                                     Question_Attempted = a.Question_Attempted,
                                     Total_Correct_Ans = a.Total_Correct_Ans
                                 }).OrderByDescending(x => x.Result_ID).ToList();
                    if (data1 != null)
                    {
                        ViewBag.test_result_dtl = data1.ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return View();
        }
        #endregion

        #region ------------------------------------------------------Security Question-----------------------------------------------------
        [HttpGet]
        public ActionResult security_question()
        {
            ViewBag.general = "general";
            ViewBag.view_security_question = "active";
            if (Convert.ToString(Session["ROLE_CODE"]) == "S")
            {
                if (Session["Role_Id"] != null)
                {
                    int id = Convert.ToInt32(Session["Role_Id"]);
                    var p_chk = DbContext.tbl_DC_Menu_Permission.Where(x => x.Menu_Key_ID == 19 && x.ROLE_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    if (p_chk == null)
                    {
                        return RedirectToAction("checkpermission");
                    }
                }
            }
            ViewBag.Breadcrumb = "Security Question";
            ViewBag.pagetitle = "Add";
            return View();
        }
        [HttpPost]
        public ActionResult security_question(string txtquestion, string hid)
        {
            ViewBag.general = "general";
            ViewBag.view_security_question = "active";
            try
            {
                if (txtquestion.Trim() != "")
                {
                    if (hid == "")
                    {
                        ViewBag.pagetitle = "Add";
                        var board = DbContext.tbl_DC_Security_Question.Where(x => x.Security_Question == txtquestion && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        if (board != null)
                        {
                            TempData["WarningMessage"] = "Question already exist.";
                        }
                        else
                        {
                            tbl_DC_Security_Question obj = new tbl_DC_Security_Question();
                            obj.Security_Question = txtquestion;
                            obj.Inserted_Date = today;
                            obj.Is_Active = true;
                            obj.Is_Deleted = false;
                            DbContext.tbl_DC_Security_Question.Add(obj);
                            DbContext.SaveChanges();
                            TempData["SuccessMessage"] = "Question added successfully.";
                            return RedirectToAction("view_security_question");
                        }
                    }
                    else
                    {
                        ViewBag.pagetitle = "Update";
                        var board = DbContext.tbl_DC_Security_Question.Where(x => x.Security_Question == txtquestion && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        if (board != null)
                        {
                            TempData["WarningMessage"] = "Question already exists.";
                        }
                        else
                        {
                            int b_id = Convert.ToInt32(hid);
                            tbl_DC_Security_Question obj = DbContext.tbl_DC_Security_Question.Where(x => x.Security_Question_ID == b_id).FirstOrDefault();
                            obj.Security_Question = txtquestion;
                            obj.Modified_Date = today;
                            DbContext.Entry(obj).State = EntityState.Modified;
                            DbContext.SaveChanges();
                            TempData["SuccessMessage"] = "Question updated successfully.";
                            return RedirectToAction("view_security_question");
                        }
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "Please enter Security Question.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return View();
        }
        public ActionResult view_security_question()
        {
            Menupermission();
            ViewBag.general = "general";
            ViewBag.view_security_question = "active";
            ViewBag.Breadcrumb = "Security Question";

            try
            {
                var data = (from b in DbContext.tbl_DC_Security_Question.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                            select new DigiChampsModel.Security_Question
                            {
                                Question_ID = b.Security_Question_ID,
                                Question = b.Security_Question
                            }).OrderByDescending(x => x.Question_ID).ToList();
                ViewBag.question = data;
            }
            catch (Exception)
            {

                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return View();
        }
        public ActionResult editquestion(int? id)
        {
            ViewBag.general = "general";
            ViewBag.view_security_question = "active";
            try
            {
                ViewBag.Breadcrumb = "Question";
                ViewBag.pagetitle = "Update";
                if (id != null)
                {
                    //var data1 = (from a in DbContext.tbl_DC_Security_Question.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                    //             select new DigiChampsModel.Security_Question
                    //             {
                    //                Question_ID= a.Security_Question_ID,
                    //                 Question= a.Security_Question
                    //             }).ToList();
                    //if (data1 != null)
                    //{
                    //    ViewBag.Board = data1;
                    //}
                    //else
                    //{
                    //    TempData["WarningMessage"] = "Invalid board details.";
                    //}
                    var data = DbContext.tbl_DC_Security_Question.Where(x => x.Security_Question_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    if (data != null)
                    {
                        DigiChampsModel.Security_Question obj = new DigiChampsModel.Security_Question();
                        obj.Question_ID = data.Security_Question_ID;
                        obj.Question = data.Security_Question;
                        ViewBag.id = obj.Question_ID;
                        ViewBag.question = obj.Question;
                        return View("security_question", obj);
                    }
                    else
                    {
                        TempData["WarningMessage"] = "Invalid Question details.";
                    }
                }
                else
                {
                    TempData["WarningMessage"] = "Invalid Questiondetails.";
                    return RedirectToAction("view_security_question");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return View();

        }
        public ActionResult deletequestion(int? id)
        {
            ViewBag.general = "general";
            ViewBag.view_security_question = "active";
            if (id != null)
            {
                var data = DbContext.tbl_DC_Security_Question.Where(x => x.Security_Question_ID == id && x.Is_Deleted == false && x.Is_Active == true).FirstOrDefault();
                if (data != null)
                {

                    data.Is_Deleted = true;
                    data.Is_Active = false;
                    DbContext.SaveChanges();
                    TempData["SuccessMessage"] = "Question deleted successfully.";
                    return RedirectToAction("view_security_question");
                }
                else
                {
                    TempData["ErrorMessage"] = "Please provide data correctly.";
                }
            }
            return View();
        }
        #endregion

        public JsonResult GetNotification()
        {
            var notificationRegisterTime = Session["LastUpdated"] != null ? Convert.ToDateTime(Session["LastUpdated"]) : DateTime.Now;
            NotificationComponent NC = new NotificationComponent();
            var list = NC.Gettickets(notificationRegisterTime);
            //update session here for get only new added contacts (notification)
            Session["LastUpdate"] = DateTime.Now;
            return new JsonResult { Data = list, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public ActionResult checkpermission()
        {
            return View();
        }

        #region-------------------------------------------Offline book-----------------------------------------
        [HttpPost]
        public ActionResult packagename(int regdid)
        {
            List<SelectListItem> ClsNames = new List<SelectListItem>();
            var classname = DbContext.tbl_DC_Registration_Dtl.Where(x => x.Regd_ID == regdid && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
            if (classname != null)
            {
                int classid = Convert.ToInt32(classname.Class_ID);
                var allpackage = DbContext.SP_DC_Student_PacakgeModule_ID(classid).ToList();
                allpackage.ForEach(x =>
                {
                    ClsNames.Add(new SelectListItem { Text = x.Package_Name, Value = x.Package_ID.ToString() });
                });
            }
            return Json(ClsNames, JsonRequestBehavior.AllowGet);
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
            Session["taxamount"] = Math.Round(taxamount, 2);
            return taxamount;
        }
        public ActionResult offlinebook()
        {
            try
            {
                ViewBag.Breadcrumb = "manualbooking";
                Menupermission();
                if (Convert.ToString(Session["ROLE_CODE"]) == "S")
                {
                    if (Session["Role_Id"] != null)
                    {
                        int id = Convert.ToInt32(Session["Role_Id"]);
                        var p_chk = DbContext.tbl_DC_Menu_Permission.Where(x => x.Menu_Key_ID == 21 && x.ROLE_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        if (p_chk == null)
                        {
                            return RedirectToAction("checkpermission");
                        }
                    }
                }
                ViewBag.totalstu = (from a in DbContext.View_All_Student_Details.Where(x => x.Class_ID != null).OrderByDescending(x => x.Regd_ID) select new DigiChampsModel.DigiChampsDailySalesModel { Regd_ID = a.Regd_ID, Customer_Name = a.Customer_Name + " - " + a.Mobile }).ToList();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }

            return View();
        }


        [HttpPost]
        public ActionResult offlinebook(DigiChampsModel.DigiChampsDailySalesModel obj1, int[] checklistitem, string Student_name, string txtprice, string txtpacktype, string txtlimit, string payment)
        {
            ViewBag.Breadcrumb = "manualbooking";
            Menupermission();
            try
            {
                ViewBag.totalstu = (from a in DbContext.View_All_Student_Details.OrderByDescending(x => x.Regd_ID) select new DigiChampsModel.DigiChampsDailySalesModel { Regd_ID = a.Regd_ID, Customer_Name = a.Customer_Name + " - " + a.Mobile }).ToList();
                decimal tax = taxcalculate();
                string disc_amt = null;

                int pkg_id1 = Convert.ToInt32(obj1.Package_ID);
                decimal orginalprice = 0;
                var package = DbContext.tbl_DC_Package.Where(x => x.Package_ID == pkg_id1).FirstOrDefault();
                if (package != null)
                {
                    bool pack = Convert.ToBoolean(package.Is_Offline);
                    if (pack != true)
                    {
                        orginalprice = Convert.ToDecimal(package.Price) + (Convert.ToDecimal(package.Price) * tax) / 100;
                    }
                    else
                    {

                        var dt = DbContext.tbl_DC_Package_Period.Where(x => x.Package_ID == pkg_id1 && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        if (dt != null && package != null)
                        {

                            orginalprice = Convert.ToDecimal(package.Price + dt.Excluded_Price) + (Convert.ToDecimal(package.Price + dt.Excluded_Price) * tax) / 100;

                        }
                        else
                        {
                            orginalprice = Convert.ToDecimal(package.Price) + (Convert.ToDecimal(package.Price) * tax) / 100;
                        }

                    }

                }
                int reg_id = Convert.ToInt32(Student_name);
                tbl_DC_Order ordobj = new tbl_DC_Order();
                ordobj.Trans_No = null;
                ordobj.Regd_ID = Convert.ToInt32(Student_name);
                if (payment == "Cash")
                {
                    ordobj.Payment_Mode = "Cash";
                }
                else if (payment == "Cheque")
                {
                    ordobj.Payment_Mode = "Cheque";
                }
                decimal amt = (Convert.ToDecimal(txtprice) / (100 + tax)) * 100;
                //ordobj.Regd_No = regno;
                ordobj.No_of_Package = 1;
                ordobj.Amount = Convert.ToDecimal(amt);
                ordobj.Disc_Perc = null;
                ordobj.Disc_Amt = orginalprice - Convert.ToDecimal(txtprice);
                ordobj.Total = Convert.ToDecimal(txtprice);
                ordobj.Amt_In_Words = null;
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
                var order_pkg = (from pkg in DbContext.tbl_DC_Package.Where(x => x.Package_ID == pkg_id1 && x.Is_Active == true && x.Is_Deleted == false)
                                 select new DigiChampCartModel
                                 {
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
                    ordpkg.Order_ID = ordobj.Order_ID;
                    ordpkg.Order_No = ordobj.Order_No;
                    ordpkg.Package_ID = obj1.Package_ID;
                    ordpkg.Package_Name = item.Package_Name;
                    TempData["PK_Name"] = ordpkg.Package_Name;
                    ordpkg.Package_Desc = item.Package_Desc;
                    ordpkg.Total_Chapter = item.Total_Chapter;
                    ordpkg.Price = item.Price;
                    TempData["Price"] = ordpkg.Price.ToString();
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

                    foreach (var item1 in checklistitem)
                    {
                        tbl_DC_Order_Pkg_Sub ordpkgsub = new tbl_DC_Order_Pkg_Sub();
                        ordpkgsub.OrderPkg_ID = ord_pkg_id;
                        ordpkgsub.Package_ID = ordpkg.Package_ID;
                        int itemid = Convert.ToInt32(item1);
                        var data = (from
                                          ag in DbContext.tbl_DC_Chapter.Where(x => x.Chapter_Id == item1 && x.Is_Active == true && x.Is_Deleted == false)

                                    join ah in DbContext.tbl_DC_Subject.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                    on ag.Subject_Id equals ah.Subject_Id
                                    join ai in DbContext.tbl_DC_Class.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                    on ah.Class_Id equals ai.Class_Id
                                    join aj in DbContext.tbl_DC_Board.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                    on ai.Board_Id equals aj.Board_Id
                                    select new DigiChampCartModel
                                    {

                                        Board_ID = aj.Board_Id,
                                        Board_Name = aj.Board_Name,
                                        Class_ID = ai.Class_Id,
                                        Class_Name = ai.Class_Name,
                                        Subject_ID = ah.Subject_Id,
                                        Subject = ah.Subject,
                                        Chapter_ID = ag.Chapter_Id,
                                        Chapter = ag.Chapter
                                    }).FirstOrDefault();
                        ordpkgsub.Chapter_ID = data.Chapter_ID;
                        ordpkgsub.Chapter_Name = data.Chapter;

                        ordpkgsub.Board_ID = data.Board_ID;
                        ordpkgsub.Board_Name = data.Board_Name;
                        ordpkgsub.Class_ID = data.Class_ID;
                        ordpkgsub.Class_Name = data.Class_Name;
                        ordpkgsub.Subject_ID = data.Subject_ID;
                        ordpkgsub.Subject_Name = data.Subject;

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
                    var data2 = (from a in DbContext.tbl_DC_Registration.Where(x => x.Regd_ID == reg_id && x.Is_Active == true && x.Is_Deleted == false)
                                 join b in DbContext.tbl_DC_Order.Where(x => x.Status == true && x.Order_ID == ord_id && x.Is_Active == true && x.Is_Deleted == false)
                                    on a.Regd_ID equals b.Regd_ID
                                 select new DigiChampCartModel
                                 {
                                     firstname = a.Customer_Name,
                                     Regd_ID = a.Regd_ID,
                                     email = a.Email,
                                     Regd_No = a.Regd_No,
                                     //  Cart_ID = b.Cart_ID,
                                     Order_ID = b.Order_ID,
                                     Order_No = b.Order_No,
                                     Address = a.Address,
                                     Mobile = a.Mobile,
                                     PIN = a.Pincode


                                 }).ToList();

                    string name = data2.ToList()[0].firstname;
                    string odno = TempData["ordernumber"].ToString();
                    string total_amt = amt.ToString("N2");
                    decimal total_amt1 = Convert.ToDecimal(amt);
                    int order_id = Convert.ToInt32(ordobj.Order_ID);
                    var data5 = (from k in DbContext.tbl_DC_Order_Tax.Where(x => x.Order_ID == order_id && x.Is_Active == true && x.Is_Deleted == false)
                                 select new DigiChampCartModel
                                 {
                                     Tax_Amt = (decimal)k.Tax_Amt
                                 }).ToList();
                    decimal totalprice = data5.ToList().Select(c => (decimal)c.Tax_Amt).Sum();
                    TempData["Tax"] = totalprice.ToString("N2");
                    string tx_amt = TempData["Tax"].ToString();
                    // TempData["payblamt"] = Convert.ToDecimal(total_amt1 + totalprice);
                    string pbl_amt = txtprice;
                    string ord_date = date;

                    // TempData["payblamt"] = Convert.ToDecimal(total_amt1 + totalprice);
                    string mobile = data2.ToList()[0].Mobile;
                    string address = data2.ToList()[0].Address;
                    string pin = data2.ToList()[0].PIN;
                    var getall = DbContext.SP_DC_Get_maildetails("ORD_CONF").FirstOrDefault();
                    StringBuilder sb = new StringBuilder();
                    var pkdtl = DbContext.tbl_DC_Package_Period.Where(x => x.Is_Active == true && x.Is_Deleted == false && x.Package_ID == pkg_id1).FirstOrDefault();


                    var order_pkg1 = (from pkg in DbContext.tbl_DC_Package.Where(x => x.Package_ID == pkg_id1 && x.Is_Active == true && x.Is_Deleted == false)
                                      select new DigiChampCartModel
                                      {
                                          Package_Name = pkg.Package_Name,
                                          Package_Desc = pkg.Package_Desc,
                                          Total_Chapter = pkg.Total_Chapter,
                                          Price = pkg.Price,
                                          Thumbnail = pkg.Thumbnail,
                                          Subscripttion_Period = pkg.Subscripttion_Period,
                                          Is_Offline = pkg.Is_Offline,
                                          Excluded_Price = pkdtl.Excluded_Price
                                      }).FirstOrDefault();
                    var data10 = (from a in DbContext.tbl_DC_Registration.Where(x => x.Mobile == mobile && x.Is_Active == true && x.Is_Deleted == false)
                                  join b in DbContext.tbl_DC_Order.Where(x => x.Status == true && x.Order_ID == ord_id && x.Is_Active == true && x.Is_Deleted == false)
                                   on a.Regd_ID equals b.Regd_ID
                                  select new DigiChampCartModel
                                  {
                                      Package_Name = order_pkg1.Package_Name,
                                      Subscripttion_Period = order_pkg1.Subscripttion_Period,
                                      Price = order_pkg1.Price + (order_pkg1.Excluded_Price == null ? 0 : order_pkg1.Excluded_Price),
                                      Order_ID = b.Order_ID,
                                      Order_No = b.Order_No,
                                      Inserted_Date = order_pkg1.Inserted_Date,
                                      //Cart_ID = b.Cart_ID
                                  }).ToList();
                    //ViewBag.cartitems = data;
                    ViewBag.data = data10;
                    if (ViewBag.data != null)
                    {

                        foreach (var it in ViewBag.data)
                        {
                            int m = 1;
                            string price = string.Format("{0:0.00}", it.Price);
                            sb.Append("<tr style='text-align:center;font-family: monospace; height:50px;'>");
                            sb.Append("<td style='border:1px solid #0f6fc6;'>" + m++ + "</td>");
                            sb.Append("<td style='border:1px solid #0f6fc6;'>" + it.Package_Name + "</td>");
                            sb.Append("<td style='border:1px solid #0f6fc6;'>" + 1 + "</td>");
                            sb.Append("<td style='border:1px solid #0f6fc6;'> " + price + "</td>");
                            sb.Append("<td style='border:1px solid #0f6fc6;'>" + orginalprice.ToString("N2") + "</td>");
                            sb.Append("</tr>");
                        }
                    }
                    string tbl_dtl = sb.ToString();
                    string tax_amount = (Convert.ToDecimal(txtprice) - amt).ToString("N2");
                    disc_amt = (orginalprice - Convert.ToDecimal(txtprice)).ToString(); ;
                    string msgbody = getall.EmailConf_Body.ToString().Replace("{{name}}", name).Replace("{{orderno}}", odno).Replace("{{orderdate}}", date).Replace("{{packagedetails}}", tbl_dtl).Replace("{{totaltax}}", tax_amount).Replace("{{totalpbl}}", pbl_amt).Replace("{{address}}", address).Replace("{{pin}}", pin).Replace("{{mobile}}", mobile).Replace("{{discount}}", Convert.ToDecimal(disc_amt).ToString("N2"));
                    sendMail1("ORD_CONF", data2.ToList()[0].email, "Order Confirmation", name, msgbody);


                }
                TempData["SuccessMessage"] = "Offline order created successfully with order no " + TempData["ordernumber"] + "";

            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return View();
        }



        public ActionResult bindchapterdetails(int pkgid)
        {

            List<SelectListItem> modulename = new List<SelectListItem>();
            var a = DbContext.tbl_DC_Package.Where(x => x.Package_ID == pkgid && x.Is_Active == true && x.Is_Deleted == false).ToList();
            var b = DbContext.tbl_DC_Package_Dtl.Where(x => x.Package_ID == pkgid && x.Is_Active == true && x.Is_Deleted == false).ToList();
            var c = DbContext.tbl_DC_Chapter.Where(x => x.Is_Active == true && x.Is_Deleted == false).ToList();
            var data2 = (from q in a
                         join p in b
                         on q.Package_ID equals p.Package_ID
                         join r in c
                         on p.Chapter_Id equals r.Chapter_Id
                         select new DigiChampsModel.DigiChampsModuleModel
                         {
                             Chapter_Id = p.Chapter_Id,
                             Chapter = r.Chapter
                         }).ToList();

            DigiChampsModel.DigiChampsModuleModel obj = new DigiChampsModel.DigiChampsModuleModel();
            obj.subdtls = data2;
            data2.ForEach(x =>
            {
                modulename.Add(new SelectListItem { Text = x.Chapter.ToString(), Value = x.Chapter_Id.ToString() });
            });
            //ViewBag.state = new SelectList(dbContext.tbl_JV_State.Where(x => x.FK_Country_ID == conId && x.Is_Active == true && x.Is_Deleted == false), "PK_State_ID", "State_Name");

            return Json(modulename, JsonRequestBehavior.AllowGet);

        }

        public ActionResult bindpricedetails(int pkgid)
        {

            decimal price = 0;
            decimal tax = taxcalculate();
            var package = DbContext.tbl_DC_Package.Where(x => x.Package_ID == pkgid).FirstOrDefault();
            {
                if (package != null)
                {
                    bool pack = Convert.ToBoolean(package.Is_Offline);
                    if (pack != true)
                    {

                        var packdetails = DbContext.tbl_DC_Package.Where(x => x.Package_ID == pkgid).FirstOrDefault();
                        price = Convert.ToDecimal(packdetails.Price);
                    }
                    else
                    {
                        var packdetails = DbContext.tbl_DC_Package.Where(x => x.Package_ID == pkgid).FirstOrDefault();
                        var dt = DbContext.tbl_DC_Package_Period.Where(x => x.Package_ID == pkgid && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        if (dt != null && packdetails != null)
                        {

                            price = Convert.ToDecimal(packdetails.Price + dt.Excluded_Price);

                        }
                        else
                        {
                            price = Convert.ToDecimal(packdetails.Price);
                        }

                    }

                }
            }
            price = price + ((price * tax) / 100);
            return Json(price, JsonRequestBehavior.AllowGet);
        }
        public ActionResult bindsublimit(int pkgid)
        {
            var data1 = (from z in DbContext.tbl_DC_Package.Where(x => x.Package_ID == pkgid && x.Is_Active == true && x.Is_Deleted == false)
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
                             SubScription_Limit = y.SubScription_Limit
                         }).ToList();
            //ViewBag.Package_Previeww = data1.Take(1);
            int limit = Convert.ToInt32(data1.ToList().Select(x => x.SubScription_Limit).Sum());
            return Json(limit, JsonRequestBehavior.AllowGet);
        }
        public ActionResult packagetype(int pkgid)
        {
            var packtype = DbContext.tbl_DC_Package.Where(x => x.Package_ID == pkgid).Select(c => c.Is_Offline).FirstOrDefault();
            return Json(packtype, JsonRequestBehavior.AllowGet);
        }
        #endregion


        #region---------------------------------Sample Image------------------------------------

        public ActionResult viewsampleimage()
        {
            ViewBag.general = "general";
            ViewBag.viewsampleimage = "active";
            ViewBag.Breadcrumb = "View Sample Image";
            try
            {
                if (Session["USER_CODE"] != null)
                {
                    var data = DbContext.tbl_DC_Sample_Image.Where(x => x.Is_Active == true && x.Is_Deleted == false).OrderByDescending(x => x.Sample_Img_Id).ToList();
                    ViewBag.data = data;

                }
                else
                {
                    return RedirectToAction("Login");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return View();
        }
        [HttpGet]
        public ActionResult addsampleimage(int? id)
        {
            ViewBag.general = "general";
            ViewBag.viewsampleimage = "active";
            ViewBag.Breadcrumb = "Add Sample Image";
            try
            {
                if (id != null)
                {
                    var data = (from c in DbContext.tbl_DC_Sample_Image.Where(x => x.Sample_Img_Id == id && x.Is_Active == true && x.Is_Deleted == false)
                                select new DigiChampsModel.DigichampsSampleImage
                                {
                                    Image_Title = c.Image_Title,
                                    Image_URL = c.Image_URL,
                                    Image_Type = c.Image_Type,
                                    Sample_Img_Id = c.Sample_Img_Id
                                }).FirstOrDefault();

                    if (data != null)
                    {
                        ViewBag.imagettle = data.Image_Title;
                        ViewBag.type = data.Image_Type;
                        ViewBag.id = data.Sample_Img_Id;
                        ViewBag.image = data.Image_URL;
                    }


                }
                else
                {
                    return View();
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return View();
        }
        [HttpPost]
        public ActionResult addsampleimage(string txttype, HttpPostedFileBase fileupload, string txttitle, HttpPostedFileBase fileupload1, int? hid)
        {
            ViewBag.general = "general";
            ViewBag.viewsampleimage = "active";
            ViewBag.Breadcrumb = "Add Sample Image";
            try
            {
                if (Session["USER_CODE"] != null)
                {
                    if (hid != null)
                    {
                        string image = string.Empty;
                        //tbl_DC_Sample_Image obj2 = DbContext.tbl_DC_Sample_Image.Where(x => x.Image_Type == txttype && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        //if (obj2 != null)
                        //{
                        //    TempData["WarningMessage"] = "Sample Image  Already Exist.";
                        //    return RedirectToAction("viewsampleimage");
                        //}
                        tbl_DC_Sample_Image obj1 = DbContext.tbl_DC_Sample_Image.Where(x => x.Sample_Img_Id == hid && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        if (obj1 != null)
                        {
                            obj1.Image_Type = txttype.Trim();
                            obj1.Image_Title = txttitle.Trim();
                            string guid = Guid.NewGuid().ToString();
                            if (fileupload != null)
                            {
                                var fileName = Path.GetFileName(fileupload.FileName.Replace(fileupload.FileName.Split('.').FirstOrDefault().ToString(), guid.ToString()));
                                image = Convert.ToString(fileName);
                                var path = Path.Combine(Server.MapPath("~/Images/SampleImage/"), fileName);
                                fileupload.SaveAs(path);
                                obj1.Image_URL = image;
                            }
                            if (fileupload1 != null)
                            {
                                var fileName = Path.GetFileName(fileupload1.FileName.Replace(fileupload1.FileName.Split('.').FirstOrDefault().ToString(), guid.ToString()));
                                image = Convert.ToString(fileName);
                                var path = Path.Combine(Server.MapPath("~/Images/SampleImage/"), fileName);
                                fileupload1.SaveAs(path);
                                obj1.Image_URL = image;
                            }
                            obj1.Inserted_By = "Admin";
                            obj1.Inserted_Date = today;
                            obj1.Is_Active = true;
                            obj1.Is_Deleted = false;
                            DbContext.Entry(obj1).State = EntityState.Modified;
                            DbContext.SaveChanges();
                            TempData["SuccessMessage"] = "Sample Image Updates Successfully.";
                            return RedirectToAction("viewsampleimage");
                        }
                        else
                        {
                            TempData["WarningMessage"] = "Invalid Image Details.";
                            return View();
                        }

                    }
                    else
                    {
                        var data = DbContext.tbl_DC_Sample_Image.Where(x => x.Image_Type == txttype && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        if (data != null)
                        {
                            TempData["WarningMessage"] = "Default Image Already Exist.";
                            return RedirectToAction("viewsampleimage");
                        }
                        else
                        {
                            string image = string.Empty;
                            tbl_DC_Sample_Image obj1 = new tbl_DC_Sample_Image();
                            obj1.Image_Type = txttype.Trim();
                            obj1.Image_Title = txttitle.Trim();
                            string guid = Guid.NewGuid().ToString();
                            if (fileupload != null)
                            {
                                var fileName = Path.GetFileName(fileupload.FileName.Replace(fileupload.FileName.Split('.').FirstOrDefault().ToString(), guid.ToString()));
                                image = Convert.ToString(fileName);
                                var path = Path.Combine(Server.MapPath("~/Images/SampleImage/"), fileName);
                                fileupload.SaveAs(path);
                                obj1.Image_URL = image;
                            }
                            if (fileupload1 != null)
                            {
                                var fileName = Path.GetFileName(fileupload1.FileName.Replace(fileupload1.FileName.Split('.').FirstOrDefault().ToString(), guid.ToString()));
                                image = Convert.ToString(fileName);
                                var path = Path.Combine(Server.MapPath("~/Images/SampleImage/"), fileName);
                                fileupload1.SaveAs(path);
                                obj1.Image_URL = image;
                            }
                            obj1.Inserted_By = "Admin";
                            obj1.Inserted_Date = today;
                            obj1.Is_Active = true;
                            obj1.Is_Deleted = false;
                            DbContext.tbl_DC_Sample_Image.Add(obj1);
                            DbContext.SaveChanges();
                            TempData["SuccessMessage"] = "Sample Image Added Successfully.";
                            return RedirectToAction("viewsampleimage");

                        }
                    }

                }
                else
                {
                    return RedirectToAction("Login");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }

            return View();
        }


        public ActionResult deleteimage(int? id)
        {
            ViewBag.general = "general";
            ViewBag.viewsampleimage = "active";
            try
            {
                if (id != null)
                {

                    tbl_DC_Sample_Image obj = DbContext.tbl_DC_Sample_Image.Where(x => x.Sample_Img_Id == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    if (obj != null)
                    {

                        obj.Is_Deleted = true;
                        obj.Is_Active = false;
                        obj.Modified_Date = today;
                        DbContext.SaveChanges();
                        TempData["SuccessMessage"] = "Image deleted  successfully.";

                    }
                    else
                    {
                        TempData["WarningMessage"] = "Invalid image details.";
                    }
                }


            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return RedirectToAction("viewsampleimage");
        }
        #endregion


        #region--------------------------------------Coupon-------------------------------------------
        [HttpGet]
        public ActionResult coupontype()
        {
            ViewBag.general = "general";
            ViewBag.coupons = "active";
            try
            {
                ViewBag.Breadcrumb = "Coupon Details";


                var data = DbContext.tbl_DC_CouponType.Where(x => x.Is_Active == true && x.Is_Delete == false).ToList();
                ViewBag.data = data;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";

            }
            return View();
        }
        [HttpGet]
        public ActionResult addcoupon(int? id)
        {
            ViewBag.coupons = "active";
            ViewBag.general = "general";
            try
            {
                if (id != null)
                {
                    tbl_DC_CouponType data = DbContext.tbl_DC_CouponType.Where(x => x.Id == id && x.Is_Active == true && x.Is_Delete == false).FirstOrDefault();
                    if (data != null)
                    {
                        return View(data);
                    }
                    else
                    {
                        TempData["WarningMessage"] = "Invalid coupon details.";
                    }
                }
                else
                {
                    return View();
                }
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return View();
        }
        [HttpPost]
        public ActionResult addcoupon(tbl_DC_CouponType obj1)
        {
            ViewBag.general = "general";
            ViewBag.coupons = "active";

            try
            {
                ViewBag.Breadcrumb = "Coupon Type";

                if (obj1.Id != null && obj1.Id != 0)
                {
                    tbl_DC_CouponType obj3 = DbContext.tbl_DC_CouponType.Where(x => x.Id == obj1.Id && x.Is_Active == true && x.Is_Delete == false).FirstOrDefault();
                    if (obj3 != null)
                    {
                        obj3.Coupon_Type = obj1.Coupon_Type;
                        DbContext.Entry(obj3).State = EntityState.Modified;
                        DbContext.SaveChanges();
                        TempData["SuccessMessage"] = "Coupon type updated  successfully.";
                        return RedirectToAction("coupontype");
                    }
                    else
                    {
                        TempData["WarningMessage"] = "Invalid coupon details.";
                    }
                }
                else
                {
                    tbl_DC_CouponType obj = new tbl_DC_CouponType();
                    obj.Coupon_Type = obj1.Coupon_Type;
                    obj.Inserted_By = HttpContext.User.Identity.Name;
                    obj.Inserted_Date = today;
                    obj.Is_Active = true;
                    obj.Is_Delete = false;
                    DbContext.tbl_DC_CouponType.Add(obj);
                    DbContext.SaveChanges();
                    TempData["SuccessMessage"] = "Coupon type added  successfully.";

                }


            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";

            }

            return RedirectToAction("coupontype");
        }
        public ActionResult deletecoupon(int? id)
        {
            ViewBag.general = "general";
            ViewBag.coupons = "active";
            try
            {
                if (id != null)
                {
                    var data = DbContext.tbl_DC_CouponCode.Where(x => x.Coupon_Type == id && x.Is_Active == true && x.Is_Default == false).FirstOrDefault();
                    if (data != null)
                    {
                        TempData["WarningMessage"] = "Coupon Type already in use.";

                    }
                    else
                    {
                        tbl_DC_CouponType obj = DbContext.tbl_DC_CouponType.Where(x => x.Id == id && x.Is_Active == true && x.Is_Delete == false).FirstOrDefault();
                        if (obj != null)
                        {

                            obj.Is_Delete = true;
                            obj.Is_Active = false;
                            obj.Modified_Date = today;
                            DbContext.SaveChanges();
                            TempData["SuccessMessage"] = "Coupon deleted  successfully.";

                        }
                        else
                        {
                            TempData["WarningMessage"] = "Invalid coupon details.";
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return RedirectToAction("coupontype");
        }

        public ActionResult viewcouponcode()
        {
            ViewBag.general = "general";
            ViewBag.viewcouponcode = "active";
            ViewBag.Breadcrumb = "Coupon Code";
            var data = DbContext.tbl_DC_CouponCode.Where(x => x.Is_Active == true && x.Is_Delete == false).OrderByDescending(x => x.Code_Id).ToList();
            ViewBag.coupon = data;

            return View();
        }
        [HttpPost]
        public ActionResult addcouponimage(int? id)
        {
            //ViewBag.viewcouponcode = "account";
            ViewBag.general = "general";
            try
            {
                if (id != null)
                {
                    var data = DbContext.tbl_DC_CouponCode.Where(x => x.Code_Id == id && x.Is_Active == true && x.Is_Delete == false).FirstOrDefault();
                    if (data != null)
                    {
                        ViewBag.coupon = new SelectList(DbContext.tbl_DC_CouponType.OrderByDescending(x => x.Id), "Id", "Coupon_Type", data.Coupon_Type);
                        ViewBag.price = data.Discount_Price;
                        ViewBag.percent = data.Discount_Percent;
                        ViewBag.couponcode = data.Coupon_Code;
                        ViewBag.expire = data.Valid_To;
                        return PartialView("couponview", data);
                    }
                    else
                    {
                        TempData["WarningMessage"] = "Invalid coupon details.";
                    }

                }
                else
                {
                    ViewBag.price = null;
                    ViewBag.percent = null;
                    ViewBag.coupon = new SelectList(DbContext.tbl_DC_CouponType.OrderByDescending(x => x.Id), "Id", "Coupon_Type");

                    return PartialView("couponview");
                }

            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }

            return View();
        }
        [HttpGet]
        public ActionResult addcouponcode(int? id)
        {
            ViewBag.viewcouponcode = "active";
            ViewBag.general = "general";
            ViewBag.Breadcrumb = "Add Coupon Code";
            try
            {
                if (id != null)
                {
                    var data = DbContext.tbl_DC_CouponCode.Where(x => x.Code_Id == id && x.Is_Active == true && x.Is_Delete == false).FirstOrDefault();
                    if (data != null)
                    {
                        ViewBag.coupon = new SelectList(DbContext.tbl_DC_CouponType.OrderByDescending(x => x.Id), "Id", "Coupon_Type", data.Coupon_Type);
                        ViewBag.price = data.Discount_Price;
                        ViewBag.percent = data.Discount_Percent;
                        ViewBag.couponcode = data.Coupon_Code;
                        ViewBag.expire = data.Valid_To;
                        return View(data);
                    }
                    else
                    {
                        TempData["WarningMessage"] = "Invalid coupon details.";
                    }

                }
                else
                {
                    ViewBag.price = null;
                    ViewBag.percent = null;
                    ViewBag.coupon = new SelectList(DbContext.tbl_DC_CouponType.OrderByDescending(x => x.Id), "Id", "Coupon_Type");

                    return View();
                }

            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }

            return View();
        }
        [HttpPost]
        public ActionResult addcouponcode(tbl_DC_CouponCode obj)
        {
            ViewBag.viewcouponcode = "active";
            ViewBag.general = "general";
            ViewBag.coupon = new SelectList(DbContext.tbl_DC_CouponType.OrderByDescending(x => x.Id).Where(x => x.Is_Active == true && x.Is_Delete == false), "Id", "Coupon_Type");
            try
            {


                if (obj.Code_Id != 0)
                {

                    if (obj.Is_Default == true)
                    {
                        var data = DbContext.tbl_DC_CouponCode.Where(x => x.Is_Default == true && x.Valid_From <= obj.Valid_From && x.Valid_To >= obj.Valid_To && x.Is_Active == true && x.Is_Delete == false).FirstOrDefault();

                        tbl_DC_CouponCode obj1 = DbContext.tbl_DC_CouponCode.Where(x => x.Code_Id == obj.Code_Id && x.Is_Active == true && x.Is_Delete == false).FirstOrDefault();
                        if (obj1 != null)
                        {
                            obj1.Coupon_Type = obj.Coupon_Type;
                            obj1.Coupon_Code = obj.Coupon_Code;
                            obj1.Discount_Price = obj.Discount_Price;
                            obj1.Valid_From = obj.Valid_From;
                            obj1.Valid_To = obj.Valid_To;
                            if (obj.Discount_Percent != null)
                            {
                                obj1.Discount_Percent = Convert.ToDecimal(obj.Discount_Percent);
                                if (obj1.Discount_Price != null)
                                {
                                    obj1.Discount_Price = null;
                                }
                            }
                            if (obj.Discount_Price != null)
                            {
                                obj1.Discount_Price = Convert.ToDecimal(obj.Discount_Price);
                                if (obj1.Discount_Percent != null)
                                {
                                    obj1.Discount_Percent = null;
                                }
                            }
                            obj1.Is_Default = true;
                            obj1.Modified_Date = today;
                            obj1.Is_Active = true;
                            obj1.Is_Delete = false;
                            DbContext.Entry(obj1).State = EntityState.Modified;
                            DbContext.SaveChanges();
                            TempData["SuccessMessage"] = "Coupon code updated successfully.";
                            return RedirectToAction("viewcouponcode");
                        }
                        else
                        {
                            TempData["WarningMessage"] = "Invali coupon details.";
                        }



                    }
                    else
                    {
                        tbl_DC_CouponCode obj1 = DbContext.tbl_DC_CouponCode.Where(x => x.Code_Id == obj.Code_Id && x.Is_Active == true && x.Is_Delete == false).FirstOrDefault();
                        if (obj1 != null)
                        {
                            obj1.Coupon_Type = obj.Coupon_Type;
                            obj1.Coupon_Code = obj.Coupon_Code;
                            if (obj.Discount_Percent != null)
                            {
                                obj1.Discount_Percent = Convert.ToDecimal(obj.Discount_Percent);
                                if (obj1.Discount_Price != null)
                                {
                                    obj1.Discount_Price = null;
                                }
                            }
                            obj1.Valid_From = Convert.ToDateTime(obj.Valid_From);
                            obj1.Valid_To = Convert.ToDateTime(obj.Valid_To);
                            if (obj.Discount_Price != null)
                            {
                                obj1.Discount_Price = Convert.ToDecimal(obj.Discount_Price);
                                if (obj1.Discount_Percent != null)
                                {
                                    obj1.Discount_Percent = null;
                                }
                            }
                            if (obj.Pricerange_From != null)
                            {
                                obj1.Pricerange_From = Convert.ToDecimal(obj.Pricerange_From);
                            }

                            if (obj.Pricerange_From != null)
                            {
                                obj1.Pricertange_To = Convert.ToDecimal(obj.Pricertange_To);
                            }

                            obj1.Valid_From = Convert.ToDateTime(obj.Valid_From);
                            obj1.Valid_To = Convert.ToDateTime(obj.Valid_To);
                            obj1.Is_Default = false;
                            obj1.Modified_Date = today;
                            obj1.Is_Active = true;
                            obj1.Is_Delete = false;
                            DbContext.Entry(obj1).State = EntityState.Modified;
                            DbContext.SaveChanges();
                            TempData["SuccessMessage"] = "Coupon code updated successfully.";
                            return RedirectToAction("viewcouponcode");
                        }
                        else
                        {
                            TempData["WarningMessage"] = "Invalid coupon details.";
                        }
                    }

                }
                else
                {
                    var data2 = DbContext.tbl_DC_CouponCode.Where(x => x.Coupon_Code == obj.Coupon_Code && x.Is_Active == true && x.Is_Delete == false).FirstOrDefault();
                    if (data2 != null)
                    {
                        TempData["WarningMessage"] = "Same coupon code already exists.";
                        return View();
                    }
                    if (obj.Is_Default == true)
                    {
                        var data = DbContext.tbl_DC_CouponCode.Where(x => x.Is_Default == true && x.Valid_From <= obj.Valid_From && x.Valid_To >= obj.Valid_To && x.Is_Active == true && x.Is_Delete == false).FirstOrDefault();
                        if (data != null)
                        {
                            TempData["WarningMessage"] = "Default coupon already exists.";
                            return View();
                        }
                        else
                        {
                            tbl_DC_CouponCode obj1 = new tbl_DC_CouponCode();
                            obj1.Coupon_Type = obj.Coupon_Type;
                            obj1.Coupon_Code = obj.Coupon_Code;
                            if (obj.Discount_Percent != null)
                            {
                                obj1.Discount_Percent = Convert.ToDecimal(obj.Discount_Percent);
                                if (obj1.Discount_Price != null)
                                {
                                    obj1.Discount_Price = null;
                                }
                            }
                            if (obj.Discount_Price != null)
                            {
                                obj1.Discount_Price = Convert.ToDecimal(obj.Discount_Price);
                                if (obj1.Discount_Percent != null)
                                {
                                    obj1.Discount_Percent = null;
                                }
                            }
                            obj1.Valid_From = Convert.ToDateTime(obj.Valid_From);
                            obj1.Valid_To = Convert.ToDateTime(obj.Valid_To);
                            obj1.Is_Default = true;
                            obj1.Inserted_By = "Admin";
                            obj1.Inserted_Date = today;
                            obj1.Is_Active = true;
                            obj1.Is_Delete = false;
                            DbContext.tbl_DC_CouponCode.Add(obj1);
                            DbContext.SaveChanges();
                            TempData["SuccessMessage"] = "Coupon code added  successfully.";
                            return RedirectToAction("viewcouponcode");

                        }
                    }
                    else
                    {
                        tbl_DC_CouponCode obj1 = new tbl_DC_CouponCode();
                        obj1.Coupon_Type = obj.Coupon_Type;
                        obj1.Coupon_Code = obj.Coupon_Code;
                        if (obj.Discount_Percent != null)
                        {
                            obj1.Discount_Percent = Convert.ToDecimal(obj.Discount_Percent);
                            if (obj1.Discount_Price != null)
                            {
                                obj1.Discount_Price = null;
                            }
                        }
                        obj1.Valid_From = Convert.ToDateTime(obj.Valid_From);
                        obj1.Valid_To = Convert.ToDateTime(obj.Valid_To);
                        if (obj.Discount_Price != null)
                        {
                            obj1.Discount_Price = Convert.ToDecimal(obj.Discount_Price);
                            if (obj1.Discount_Percent != null)
                            {
                                obj1.Discount_Percent = null;
                            }
                        }
                        if (obj.Pricerange_From != null)
                        {
                            obj1.Pricerange_From = Convert.ToDecimal(obj.Pricerange_From);
                        }

                        if (obj.Pricerange_From != null)
                        {
                            obj1.Pricertange_To = Convert.ToDecimal(obj.Pricertange_To);
                        }

                        obj1.Is_Default = false;
                        obj1.Inserted_By = "Admin";
                        obj1.Inserted_Date = today;
                        obj1.Is_Active = true;
                        obj1.Is_Delete = false;
                        DbContext.tbl_DC_CouponCode.Add(obj1);
                        DbContext.SaveChanges();
                        TempData["SuccessMessage"] = "Coupon code added  successfully.";
                        return RedirectToAction("viewcouponcode");
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return View();
        }

        public ActionResult deletecouponcode(int? id)
        {
            ViewBag.viewcouponcode = "active";
            ViewBag.general = "general";
            try
            {
                if (id != null)
                {
                    tbl_DC_CouponCode obj = DbContext.tbl_DC_CouponCode.Where(x => x.Code_Id == id && x.Is_Active == true && x.Is_Delete == false).FirstOrDefault();
                    if (obj != null)
                    {

                        obj.Is_Delete = true;
                        obj.Is_Active = false;
                        obj.Modified_Date = today;
                        DbContext.SaveChanges();
                        TempData["SuccessMessage"] = "Coupon code deleted  successfully.";
                    }
                    else
                    {
                        TempData["WarningMessage"] = "Invalid coupon code .";
                    }
                }

            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return RedirectToAction("viewcouponcode");
        }
        #endregion


        public JsonResult ciphervideo(string video_key)
        {

            ViewBag.Message = "Your application description page.";

            string video_id = video_key;          // This should be obtained from DB
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
            dynamic otp;
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                string json_otp = reader.ReadToEnd();
                otp_data = JObject.Parse(json_otp);


            }
            var data = otp_data.otp;
            string data1 = data.Value;

            return Json(data1, JsonRequestBehavior.AllowGet);
        }


        #region----------------------------------------------------Marketing Blogs----------------------------------------------------
        [HttpGet]

        public ActionResult viewblogdetails()
        {
            ViewBag.general = "general";
            ViewBag.blog = "active";
            ViewBag.Breadcrumb = "Marketing Blog";
            try
            {
                if (Session["USER_CODE"] != null)
                {
                    var data = DbContext.tbl_DC_Marketing_Blog.Where(x => x.Is_Active == true && x.Is_Deleted == false).OrderByDescending(x => x.Blog_Id).ToList();
                    ViewBag.data = data;

                }
                else
                {
                    return RedirectToAction("Login");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return View();
        }
        [HttpGet]
        public ActionResult marketing_blog(int? id)
        {
            ViewBag.Breadcrumb = "Marketing Blog";
            ViewBag.blog = "active";
            try
            {
                if (Session["USER_CODE"] != null)
                {

                    if (id != null)
                    {
                        var data = (from c in DbContext.tbl_DC_Marketing_Blog.Where(x => x.Blog_Id == id && x.Is_Active == true && x.Is_Deleted == false)
                                    select new DigiChampsModel.DigichampsMarketingBlog
                                    {
                                        Blog_Id = c.Blog_Id,
                                        Name = c.Name,
                                        Designation = c.Designation,
                                        Image = c.Image,
                                        Blog_Image = c.Blog_Image,
                                        Description = c.Description

                                    }).FirstOrDefault();

                        if (data != null)
                        {
                            ViewBag.blogimage = data.Blog_Image;
                            ViewBag.image = data.Image;
                            ViewBag.name = data.Name;
                            ViewBag.designation = data.Designation;
                            ViewBag.description = data.Description;
                            ViewBag.id = data.Blog_Id;
                        }


                    }
                    else
                    {
                        return View();
                    }
                }
                else
                {
                    return RedirectToAction("Login");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return View();
        }
        [ValidateInput(false)]
        [HttpPost]
        public ActionResult marketing_blog(string name, string desig, string description, HttpPostedFileBase fileupload, string hid, HttpPostedFileBase fileupload1, string hidans_img, string hidans_img1)
        {
            ViewBag.general = "general";
            ViewBag.blog = "active";
            ViewBag.Breadcrumb = "Marketing Blog";
            int? id = 0;
            try
            {
                if (Session["USER_CODE"] != null)
                {
                    if (hid != "")
                    {
                        id = Convert.ToInt32(hid);
                        //string image = string.Empty;
                        //string image1 = string.Empty;
                        tbl_DC_Marketing_Blog obj1 = DbContext.tbl_DC_Marketing_Blog.Where(x => x.Blog_Id == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        if (obj1 != null)
                        {
                            obj1.Name = name.Trim();
                            obj1.Designation = desig.Trim();
                            obj1.Description = description.Trim();
                            string guid = Guid.NewGuid().ToString();
                            if (hidans_img != null && hidans_img != "")
                            {
                                string[] image = hidans_img.Split(',');
                                string tktname = Convert.ToString(CreateRandomPassword(6));
                                Base64ToImage_blog(tktname, image[1]);
                                hidans_img = tktname + ".jpg";
                                obj1.Blog_Image = hidans_img;
                            }
                            if (hidans_img1 != null && hidans_img1 != "")
                            {
                                string[] image1 = hidans_img1.Split(',');
                                string tktname1 = Convert.ToString(CreateRandomPassword(6));
                                Base64ToImage_blog(tktname1, image1[1]);
                                hidans_img1 = tktname1 + ".jpg";
                                obj1.Image = hidans_img1;
                            }
                            //string guid1 = Guid.NewGuid().ToString();
                            //if (fileupload1 != null)
                            //{
                            //    var fileName1 = Path.GetFileName(fileupload1.FileName.Replace(fileupload1.FileName.Split('.').FirstOrDefault().ToString(), guid1.ToString()));
                            //    image1 = Convert.ToString(fileName1);
                            //    var path1 = Path.Combine(Server.MapPath("~/Images/Blog/"), fileName1);
                            //    fileupload1.SaveAs(path1);
                            //    obj1.Blog_Image = image1;
                            //}
                            obj1.Inserted_By = "Admin";
                            obj1.Inserted_Date = today;
                            obj1.Is_Active = true;
                            obj1.Is_Deleted = false;
                            DbContext.Entry(obj1).State = EntityState.Modified;
                            DbContext.SaveChanges();
                            TempData["SuccessMessage"] = "Marketing Blog Updated Successfully.";
                            return RedirectToAction("viewblogdetails");
                        }
                        else
                        {
                            TempData["WarningMessage"] = "Invalid Blog Details.";
                            return View();
                        }

                    }
                    else
                    {
                        //var data = DbContext.tbl_DC_Sample_Image.Where(x => x.Image_Type == txttype && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        //if (data != null)
                        //{
                        //    TempData["WarningMessage"] = "Default Image Already Exist.";
                        //    return RedirectToAction("viewsampleimage");
                        //}
                        //else
                        //{
                        //string image = string.Empty;
                        //string image1 = string.Empty;
                        tbl_DC_Marketing_Blog obj1 = new tbl_DC_Marketing_Blog();
                        obj1.Name = name.Trim();
                        obj1.Designation = desig.Trim();
                        obj1.Description = description.Trim();
                        string guid = Guid.NewGuid().ToString();
                        if (hidans_img != null && hidans_img != "")
                        {
                            string[] image = hidans_img.Split(',');
                            string tktname = Convert.ToString(CreateRandomPassword(6));
                            Base64ToImage_blog(tktname, image[1]);
                            hidans_img = tktname + ".jpg";
                            obj1.Blog_Image = hidans_img;
                        }
                        if (hidans_img1 != null && hidans_img1 != "")
                        {
                            string[] image1 = hidans_img1.Split(',');
                            string tktname1 = Convert.ToString(CreateRandomPassword(6));
                            Base64ToImage_blog(tktname1, image1[1]);
                            hidans_img1 = tktname1 + ".jpg";
                            obj1.Image = hidans_img1;
                        }
                        obj1.Inserted_By = "Admin";
                        obj1.Inserted_Date = today;
                        obj1.Is_Active = true;
                        obj1.Is_Deleted = false;
                        DbContext.tbl_DC_Marketing_Blog.Add(obj1);
                        DbContext.SaveChanges();
                        TempData["SuccessMessage"] = "Marketing Blog Added Successfully.";
                        return RedirectToAction("viewblogdetails");

                    }
                }

                // }
                else
                {
                    return RedirectToAction("Login");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return View();
        }
        public Image Base64ToImage_blog(string tktname, string base64String)
        {
            // Convert base 64 string to byte[]
            byte[] imageBytes = Convert.FromBase64String(base64String);
            // Convert byte[] to Image
            using (var ms = new MemoryStream(imageBytes, 0, imageBytes.Length))
            {
                string filename = tktname + ".jpg";
                Image image = Image.FromStream(ms, true);
                var img = new Bitmap(Image.FromStream(ms));
                string tempFolderName = Server.MapPath("~/Images/Blog/" + filename);
                image.Save(tempFolderName, ImageFormat.Jpeg);
                return image;
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
                string tempFolderName = Server.MapPath("~/Images/OurTeam/" + filename);
                image.Save(tempFolderName, ImageFormat.Jpeg);
                return image;
            }
        }
        public ActionResult deleteblog(int? id)
        {
            ViewBag.general = "general";
            ViewBag.viewsampleimage = "active";
            try
            {
                if (id != null)
                {

                    tbl_DC_Marketing_Blog obj = DbContext.tbl_DC_Marketing_Blog.Where(x => x.Blog_Id == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    if (obj != null)
                    {

                        obj.Is_Deleted = true;
                        obj.Is_Active = false;
                        obj.Modified_Date = today;
                        DbContext.SaveChanges();
                        TempData["SuccessMessage"] = "Blog deleted  successfully.";

                    }
                    else
                    {
                        TempData["WarningMessage"] = "Invalid blog details.";
                    }
                }


            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return RedirectToAction("viewblogdetails");
        }
        #endregion
        #region-----------------------------------------------------Our Team----------------------------------------------------
        [HttpGet]

        public ActionResult viewteamdetails()
        {
            ViewBag.general = "general";
            ViewBag.ourteam = "active";
            ViewBag.Breadcrumb = "Our Team";
            try
            {
                if (Session["USER_CODE"] != null)
                {
                    var data = DbContext.tbl_DC_Our_Team.Where(x => x.Is_Active == true && x.Is_Deleted == false).OrderByDescending(x => x.Team_Id).ToList();
                    ViewBag.data = data;

                }
                else
                {
                    return RedirectToAction("Login");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return View();
        }
        [HttpGet]
        public ActionResult ourteam(int? id)
        {
            ViewBag.Breadcrumb = "Our Team";
            ViewBag.ourteam = "team";
            try
            {
                if (Session["USER_CODE"] != null)
                {

                    if (id != null)
                    {
                        var data = (from c in DbContext.tbl_DC_Our_Team.Where(x => x.Team_Id == id && x.Is_Active == true && x.Is_Deleted == false)
                                    select new DigiChampsModel.DigichampsOurTeam
                                    {
                                        Team_Id = c.Team_Id,
                                        Name = c.Name,
                                        Designation = c.Designation,
                                        Image = c.Image,
                                        Description = c.Description

                                    }).FirstOrDefault();

                        if (data != null)
                        {
                            ViewBag.image = data.Image;
                            ViewBag.name = data.Name;
                            ViewBag.designation = data.Designation;
                            ViewBag.description = data.Description;
                            ViewBag.id = data.Team_Id;
                        }


                    }
                    else
                    {
                        return View();
                    }
                }
                else
                {
                    return RedirectToAction("Login");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return View();
        }
        [ValidateInput(false)]
        [HttpPost]
        public ActionResult ourteam(string name, string desig, string description, HttpPostedFileBase fileupload1, string hid, string hidans_img)
        {
            ViewBag.general = "general";
            ViewBag.ourteam = "active";
            ViewBag.Breadcrumb = "Our Team";
            int? id = 0;
            try
            {
                if (Session["USER_CODE"] != null)
                {
                    if (hid != "")
                    {
                        id = Convert.ToInt32(hid);
                        string image = string.Empty;

                        tbl_DC_Our_Team obj1 = DbContext.tbl_DC_Our_Team.Where(x => x.Team_Id == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        if (obj1 != null)
                        {
                            obj1.Name = name.Trim();
                            obj1.Designation = desig.Trim();
                            obj1.Description = description.Trim();
                            string guid = Guid.NewGuid().ToString();
                            if (hidans_img != null && hidans_img != "")
                            {
                                string[] image1 = hidans_img.Split(',');
                                string tktname = Convert.ToString(CreateRandomPassword(6));
                                Base64ToImage_tkt(tktname, image1[1]);
                                hidans_img = tktname + ".jpg";
                                obj1.Image = hidans_img;
                            }

                            obj1.Inserted_By = "Admin";
                            obj1.Inserted_Date = today;
                            obj1.Is_Active = true;
                            obj1.Is_Deleted = false;
                            DbContext.Entry(obj1).State = EntityState.Modified;
                            DbContext.SaveChanges();
                            TempData["SuccessMessage"] = "Team Updated Successfully.";
                            return RedirectToAction("viewteamdetails");
                        }
                        else
                        {
                            TempData["WarningMessage"] = "Invalid Team Details.";
                            return View();
                        }

                    }
                    else
                    {
                        //var data = DbContext.tbl_DC_Sample_Image.Where(x => x.Image_Type == txttype && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        //if (data != null)
                        //{
                        //    TempData["WarningMessage"] = "Default Image Already Exist.";
                        //    return RedirectToAction("viewsampleimage");
                        //}
                        //else
                        //{
                        string image = string.Empty;
                        tbl_DC_Our_Team obj1 = new tbl_DC_Our_Team();
                        obj1.Name = name.Trim();
                        obj1.Designation = desig.Trim();
                        obj1.Description = description.Trim();
                        string guid = Guid.NewGuid().ToString();
                        if (hidans_img != null && hidans_img != "")
                        {
                            string[] image1 = hidans_img.Split(',');
                            string tktname = Convert.ToString(CreateRandomPassword(6));
                            Base64ToImage_tkt(tktname, image1[1]);
                            hidans_img = tktname + ".jpg";
                            obj1.Image = hidans_img;
                        }

                        obj1.Inserted_By = "Admin";
                        obj1.Inserted_Date = today;
                        obj1.Is_Active = true;
                        obj1.Is_Deleted = false;
                        DbContext.tbl_DC_Our_Team.Add(obj1);
                        DbContext.SaveChanges();
                        TempData["SuccessMessage"] = "Team Added Successfully.";
                        return RedirectToAction("viewteamdetails");

                    }
                }

                // }
                else
                {
                    return RedirectToAction("Login");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return View();
        }

        public ActionResult deleteteam(int? id)
        {
            ViewBag.general = "general";
            ViewBag.viewsampleimage = "active";
            try
            {
                if (id != null)
                {

                    tbl_DC_Our_Team obj = DbContext.tbl_DC_Our_Team.Where(x => x.Team_Id == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    if (obj != null)
                    {

                        obj.Is_Deleted = true;
                        obj.Is_Active = false;
                        obj.Modified_Date = today;
                        DbContext.SaveChanges();
                        TempData["SuccessMessage"] = "Team deleted  successfully.";

                    }
                    else
                    {
                        TempData["WarningMessage"] = "Invalid team details.";
                    }
                }


            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return RedirectToAction("viewteamdetails");
        }
        #endregion
        #region----------------------------------------------------career----------------------------------------------------
        [HttpGet]
        public ActionResult career()
        {
            ViewBag.Breadcrumb = "View Career";
            ViewBag.general = "general";
            ViewBag.career = "active";
            try
            {



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
        [HttpGet]
        public ActionResult addcareer(int? id)
        {
            ViewBag.Breadcrumb = "View Career";
            ViewBag.general = "general";
            ViewBag.career = "active";
            try
            {

                if (id != null)
                {

                    var data = DbContext.tbl_DC_Career.Where(x => x.Career_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    if (data != null)
                    {
                        DigiChampsModel.DigichampsCareer obj = new DigiChampsModel.DigichampsCareer();
                        obj.career_Name = data.career_Name;
                        obj.No_of_vacancy = data.No_of_vacancy;
                        obj.Experience = data.Experience;
                        obj.Location = data.Location;
                        obj.Qualification = data.Qualification;
                        obj.Opening_Date = data.Opening_Date;
                        obj.Close_Date = data.Close_Date;
                        obj.Walk_in_Time = data.Walk_in_Time;
                        obj.Phone = data.Phone;
                        obj.Job_Description = data.Job_Description;
                        ViewBag.Career_ID = data.Career_ID;

                        return View("addcareer", obj);
                    }
                    else
                    {
                        TempData["WarningMessage"] = "Invalid career details.";
                    }
                }
                else
                {

                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }

            return View();
        }
        [HttpPost]
        public ActionResult addcareer(DigiChampsModel.DigichampsCareer obj, string career_id)
        {
            ViewBag.Breadcrumb = "View Career";
            ViewBag.general = "general";
            ViewBag.career = "active";
            try
            {
                if (career_id != "")
                {

                    int id = Convert.ToInt32(career_id);

                    var item = DbContext.tbl_DC_Career.Where(x => x.Career_ID == id).FirstOrDefault();
                    if (item != null)
                    {

                        item.career_Name = obj.career_Name;
                        item.No_of_vacancy = obj.No_of_vacancy;
                        item.Experience = obj.Experience;
                        item.Location = obj.Location;
                        item.Qualification = obj.Qualification;
                        item.Opening_Date = obj.Opening_Date;
                        item.Close_Date = obj.Close_Date;
                        item.Walk_in_Time = obj.Walk_in_Time;
                        item.Phone = obj.Phone;
                        item.Job_Description = obj.Job_Description;
                        item.Modified_By = Convert.ToString(Session["username"]);
                        item.Modified_Date = today;

                        DbContext.Entry(item).State = EntityState.Modified;

                        DbContext.SaveChanges();
                        TempData["success"] = "Career updated successfully";
                        return RedirectToAction("career");


                    }



                }
                else
                {

                    if (Session["USER_CODE"] != null)
                    {
                        tbl_DC_Career tb = new tbl_DC_Career();
                        tb.career_Name = obj.career_Name;
                        tb.No_of_vacancy = obj.No_of_vacancy;
                        tb.Experience = obj.Experience;
                        tb.Location = obj.Location;
                        tb.Qualification = obj.Qualification;
                        tb.Opening_Date = obj.Opening_Date;
                        tb.Close_Date = obj.Close_Date;
                        tb.Walk_in_Time = obj.Walk_in_Time;
                        tb.Phone = obj.Phone;
                        tb.Job_Description = obj.Job_Description;
                        // tb..Inserted_Date =today;
                        tb.Inserted_By = Convert.ToString(Session["username"]);
                        tb.Is_Active = true;
                        tb.Is_Deleted = false;
                        DbContext.tbl_DC_Career.Add(tb);
                        DbContext.SaveChanges();
                        TempData["SuccessMessage"] = "Career added successfully.";
                        return RedirectToAction("career");
                    }
                }

            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return View();
        }

        public ActionResult Deletecareer(int? id)
        {

            try
            {
                if (id != null)
                {
                    var career_found = DbContext.tbl_DC_Career.Where(x => x.Career_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    if (career_found != null)
                    {
                        var obj = DbContext.tbl_DC_Career.Where(x => x.Career_ID == id).FirstOrDefault();
                        obj.Modified_Date = today;
                        obj.Is_Active = false;
                        obj.Is_Deleted = true;
                        DbContext.Entry(obj).State = EntityState.Modified;
                        DbContext.SaveChanges();
                        TempData["SuccessMessage"] = "Career deleted successfully.";
                        return RedirectToAction("career");

                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Career can not be deleted because its in use.";
                    }

                }
                else
                {
                    TempData["ErrorMessage"] = "No data found";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return RedirectToAction("career");

        }

        #endregion
        #region----------------------------------------------------faq----------------------------------------------------
        public ActionResult faq()
        {
            ViewBag.Breadcrumb = "View FAQ";
            ViewBag.general = "general";
            ViewBag.faq = "active";
            try
            {
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

        [HttpGet]
        public ActionResult addfaq(int? id)
        {
            ViewBag.Breadcrumb = "View FAQ";
            ViewBag.general = "general";
            ViewBag.faq = "active";

            try
            {

                if (id != null)
                {

                    var data = DbContext.tbl_DC_FAQs.Where(x => x.FAQs_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    if (data != null)
                    {
                        DigiChampsModel.Digichampsfaq obj = new DigiChampsModel.Digichampsfaq();
                        obj.FAQ = data.FAQ;
                        obj.FAQ_Answer = data.FAQ_Answer;

                        ViewBag.FAQs_ID = data.FAQs_ID;

                        return View("addfaq", obj);
                    }
                    else
                    {
                        TempData["WarningMessage"] = "Invalid faq details.";
                    }
                }
                else
                {

                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return View();
        }
        [HttpPost]
        public ActionResult addfaq(DigiChampsModel.Digichampsfaq obj, string faqs_id)
        {
            try
            {
                if (faqs_id != "")
                {

                    int id = Convert.ToInt32(faqs_id);

                    var item = DbContext.tbl_DC_FAQs.Where(x => x.FAQs_ID == id).FirstOrDefault();
                    if (item != null)
                    {

                        item.FAQ = obj.FAQ;
                        item.FAQ_Answer = obj.FAQ_Answer;
                        item.Modified_By = Convert.ToString(Session["username"]);
                        item.Modified_Date = today;
                        DbContext.Entry(item).State = EntityState.Modified;
                        DbContext.SaveChanges();
                        TempData["success"] = "Faq updated successfully";
                        return RedirectToAction("faq");


                    }



                }
                else
                {

                    if (Session["USER_CODE"] != null)
                    {
                        tbl_DC_FAQs tb = new tbl_DC_FAQs();
                        tb.FAQ = obj.FAQ;
                        tb.FAQ_Answer = obj.FAQ_Answer;
                        tb.Inserted_By = Convert.ToString(Session["username"]);
                        tb.Is_Active = true;
                        tb.Is_Deleted = false;
                        DbContext.tbl_DC_FAQs.Add(tb);
                        DbContext.SaveChanges();
                        TempData["SuccessMessage"] = "Faq added successfully.";
                        return RedirectToAction("faq");
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return View();
        }

        public ActionResult Deletefaq(int? id)
        {


            try
            {
                if (id != null)
                {
                    var faq_found = DbContext.tbl_DC_FAQs.Where(x => x.FAQs_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    if (faq_found != null)
                    {
                        var obj = DbContext.tbl_DC_FAQs.Where(x => x.FAQs_ID == id).FirstOrDefault();
                        obj.Modified_Date = today;
                        obj.Is_Active = false;
                        obj.Is_Deleted = true;
                        DbContext.Entry(obj).State = EntityState.Modified;
                        DbContext.SaveChanges();
                        TempData["SuccessMessage"] = "Faq deleted successfully.";
                        return RedirectToAction("faq");

                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Faq can not be deleted because its in use.";
                    }

                }
                else
                {
                    TempData["ErrorMessage"] = "No data found";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return RedirectToAction("faq");



        }
        #endregion



        #region---------------------------------Banner------------------------------------
        [HttpGet]
        public ActionResult viewbanner()
        {
            ViewBag.general = "general";
            ViewBag.banner = "active";
            ViewBag.Breadcrumb = "View Banner";
            try
            {
                if (Session["USER_CODE"] != null)
                {
                    var data = DbContext.tbl_DC_Banner.Where(x => x.Is_Active == true && x.Is_Deleted == false).OrderByDescending(x => x.banner_Id).ToList();
                    ViewBag.data = data;

                }
                else
                {
                    return RedirectToAction("Login");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return View();
        }
        [HttpGet]
        public ActionResult addbanner(int? id)
        {
            ViewBag.general = "general";
            ViewBag.banner = "active";
            ViewBag.Breadcrumb = "Add Banner";
            try
            {
                if (id != null)
                {
                    var data = (from c in DbContext.tbl_DC_Banner.Where(x => x.banner_Id == id && x.Is_Active == true && x.Is_Deleted == false)
                                select new DigiChampsModel.DigichampsBanner
                                {
                                    Image_Title = c.Image_Title,
                                    Image_URL = c.Image_URL,
                                    banner_Id = c.banner_Id
                                }).FirstOrDefault();

                    if (data != null)
                    {
                        ViewBag.imagettle = data.Image_Title;

                        ViewBag.id = data.banner_Id;
                        ViewBag.image = data.Image_URL;
                    }


                }
                else
                {
                    return View();
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return View();
        }
        [HttpPost]
        public ActionResult addbanner(HttpPostedFileBase fileupload, string txttitle, int? hid)
        {
            ViewBag.general = "general";
            ViewBag.banner = "active";
            ViewBag.Breadcrumb = "Add Banner";
            try
            {
                if (Session["USER_CODE"] != null)
                {
                    if (hid != null)
                    {
                        string image = string.Empty;
                        //tbl_DC_Sample_Image obj2 = DbContext.tbl_DC_Sample_Image.Where(x => x.Image_Type == txttype && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        //if (obj2 != null)
                        //{
                        //    TempData["WarningMessage"] = "Sample Image  Already Exist.";
                        //    return RedirectToAction("viewsampleimage");
                        //}
                        tbl_DC_Banner obj1 = DbContext.tbl_DC_Banner.Where(x => x.banner_Id == hid && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        if (obj1 != null)
                        {

                            obj1.Image_Title = txttitle.Trim();
                            string guid = Guid.NewGuid().ToString();
                            if (fileupload != null)
                            {
                                var fileName = Path.GetFileName(fileupload.FileName.Replace(fileupload.FileName.Split('.').FirstOrDefault().ToString(), guid.ToString()));
                                image = Convert.ToString(fileName);
                                var path = Path.Combine(Server.MapPath("~/Images/Banner/"), fileName);
                                fileupload.SaveAs(path);
                                obj1.Image_URL = image;
                            }

                            obj1.Inserted_By = "Admin";
                            obj1.Inserted_Date = today;
                            obj1.Is_Active = true;
                            obj1.Is_Deleted = false;
                            DbContext.Entry(obj1).State = EntityState.Modified;
                            DbContext.SaveChanges();
                            TempData["SuccessMessage"] = "Banner Updated Successfully.";
                            return RedirectToAction("viewbanner");
                        }
                        else
                        {
                            TempData["WarningMessage"] = "Invalid banner Details.";
                            return View();
                        }

                    }
                    else
                    {
                        var data = DbContext.tbl_DC_Banner.Where(x => x.Image_Title == txttitle && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        if (data != null)
                        {
                            TempData["WarningMessage"] = "Banner Already Exist.";
                            return RedirectToAction("viewbanner");
                        }
                        else
                        {
                            string image = string.Empty;
                            tbl_DC_Banner obj1 = new tbl_DC_Banner();

                            obj1.Image_Title = txttitle.Trim();
                            string guid = Guid.NewGuid().ToString();
                            if (fileupload != null)
                            {
                                var fileName = Path.GetFileName(fileupload.FileName.Replace(fileupload.FileName.Split('.').FirstOrDefault().ToString(), guid.ToString()));
                                image = Convert.ToString(fileName);
                                var path = Path.Combine(Server.MapPath("~/Images/Banner/"), fileName);
                                fileupload.SaveAs(path);
                                obj1.Image_URL = image;
                            }

                            obj1.Inserted_By = "Admin";
                            obj1.Inserted_Date = today;
                            obj1.Is_Active = true;
                            obj1.Is_Deleted = false;
                            DbContext.tbl_DC_Banner.Add(obj1);
                            DbContext.SaveChanges();
                            TempData["SuccessMessage"] = "Banner Added Successfully.";
                            return RedirectToAction("viewbanner");

                        }
                    }

                }
                else
                {
                    return RedirectToAction("Login");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }

            return View();
        }


        public ActionResult deletebanner(int? id)
        {
            ViewBag.general = "general";
            ViewBag.viewsampleimage = "active";
            try
            {
                if (id != null)
                {

                    tbl_DC_Banner obj = DbContext.tbl_DC_Banner.Where(x => x.banner_Id == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    if (obj != null)
                    {

                        obj.Is_Deleted = true;
                        obj.Is_Active = false;
                        obj.Modified_Date = today;
                        DbContext.SaveChanges();
                        TempData["SuccessMessage"] = "Banner deleted  successfully.";

                    }
                    else
                    {
                        TempData["WarningMessage"] = "Invalid banner details.";
                    }
                }


            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return RedirectToAction("viewbanner");
        }
        #endregion

        #region----------------------------------------------------About Us----------------------------------------------------
        public ActionResult about_us()
        {
            ViewBag.Breadcrumb = " View About Us";
            ViewBag.general = "general";
            ViewBag.about = "active";

            try
            {
                var data = DbContext.tbl_DC_Aboutus.Where(x => x.Is_Active == true && x.Is_Deleted == false).ToList();
                ViewBag.about = data;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return View();


        }

        [HttpGet]
        public ActionResult addaboutus(int? id)
        {
            ViewBag.Breadcrumb = " Add About Us";
            ViewBag.general = "general";
            ViewBag.about = "active";
            try
            {

                if (id != null)
                {

                    var data = DbContext.tbl_DC_Aboutus.Where(x => x.About_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    if (data != null)
                    {


                        ViewBag.about = data.Aboutus;

                        ViewBag.id = data.About_ID;

                        return View();
                    }
                    else
                    {
                        TempData["WarningMessage"] = "Invalid about us details.";
                    }
                }
                else
                {

                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return View();
        }
        [HttpPost]
        public ActionResult addaboutus(string about, string hid)
        {
            try
            {
                if (hid != "")
                {

                    int id = Convert.ToInt32(hid);

                    var item = DbContext.tbl_DC_Aboutus.Where(x => x.About_ID == id).FirstOrDefault();
                    if (item != null)
                    {


                        item.Aboutus = about;
                        item.Modified_By = Convert.ToString(Session["username"]);
                        item.Modified_Date = today;
                        DbContext.Entry(item).State = EntityState.Modified;
                        DbContext.SaveChanges();
                        TempData["SuccessMessage"] = "About us updated successfully";
                        return RedirectToAction("about_us");


                    }



                }
                else
                {

                    if (Session["USER_CODE"] != null)
                    {
                        tbl_DC_Aboutus tb = new tbl_DC_Aboutus();
                        tb.Aboutus = about;
                        tb.Inserted_By = Convert.ToString(Session["username"]);
                        tb.Is_Active = true;
                        tb.Is_Deleted = false;
                        DbContext.tbl_DC_Aboutus.Add(tb);
                        DbContext.SaveChanges();
                        TempData["SuccessMessage"] = "About us added successfully.";
                        return RedirectToAction("about_Us");
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return View();
        }

        public ActionResult Deleteabout(int? id)
        {


            try
            {
                if (id != null)
                {
                    var faq_found = DbContext.tbl_DC_Aboutus.Where(x => x.About_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    if (faq_found != null)
                    {

                        faq_found.Modified_Date = today;
                        faq_found.Is_Active = false;
                        faq_found.Is_Deleted = true;
                        DbContext.Entry(faq_found).State = EntityState.Modified;
                        DbContext.SaveChanges();
                        TempData["SuccessMessage"] = "About us deleted successfully.";
                        return RedirectToAction("about_us");

                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Invalid details";
                    }

                }
                else
                {
                    TempData["ErrorMessage"] = "No data found";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong.";
            }
            return RedirectToAction("about_us");



        }
        #endregion

        #region-----------------------------------------School----------------------------------------------



        public static void CreateThumbnail(Stream sourcePath, string filename)
        {
            Image image = Image.FromStream(sourcePath);
            Image thumb = image.GetThumbnailImage(120, 120, () => false, IntPtr.Zero);

            var filePath1 = System.Web.HttpContext.Current.Server.MapPath("~/Upload/") + "School\\Thumbnail";

            thumb.Save(filePath1 + filename);

        }

        /// <summary>
        /// //Delete school from db
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult DeleteSchool(Guid id)
        {
            try
            {
                // get data for same id 
                var result = DbContext.tbl_DC_School_Info.Where(x => x.SchoolId == id && x.IsActive == true).FirstOrDefault();
                //                  //Set status false for delete
                result.IsActive = false;
                DbContext.SaveChanges();

            }

            catch (Exception ex)
            {



            }
            return RedirectToAction("GetSchoolList", "School");

        }
        #endregion
    }
}