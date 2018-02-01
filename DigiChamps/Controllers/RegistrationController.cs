using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Threading.Tasks;
using System.Web.Security;
using DigiChamps.Models;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Net.Mail;
using System.Data.Entity;

namespace DigiChamps.Controllers
{
    [Authorize]
    public class RegistrationController : ApiController
    {
        // GET: api/Registration
        DigiChampsEntities DbContext = new DigiChampsEntities();
        DateTime today = Digichamps.datetoserver();
        [HttpPost]
        public HttpResponseMessage register()
        {
            try
            {
                var httprequest = HttpContext.Current.Request;
                string mobile = Convert.ToString(httprequest.Form["Mobile"]);
                string name = Convert.ToString(httprequest.Form["Name"]);
                string email = Convert.ToString(httprequest.Form["Email"]);
                int? questionid = Convert.ToInt32(httprequest.Form["Question_Id"]);
                string answer = Convert.ToString(httprequest.Form["Answer"]);


                if (mobile != null && name != null && email != null)
                {
                    if (questionid != null)
                    {
                        if (answer != null)
                        {
                            var mob = DbContext.tbl_DC_Registration.Where(x => x.Mobile == mobile && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                            if (mob == null)
                            {
                                tbl_DC_Registration dr = new tbl_DC_Registration();
                                dr.Mobile = mobile;
                                dr.Customer_Name = name;
                                dr.Email = email;
                                if (httprequest.Files.Count > 0)
                                {
                                    string guid = Guid.NewGuid().ToString();
                                    //var docfile = new List<string>();
                                    foreach (string files in httprequest.Files)
                                    {
                                        var postedfile = httprequest.Files[files];
                                        var path = HttpContext.Current.Server.MapPath("~/Images/Profile/" + guid + postedfile.FileName);
                                        postedfile.SaveAs(path);
                                        //docfile.Add(path);
                                        dr.Image = guid + postedfile.FileName;
                                    }
                                }
                                dr.Is_Active = true;
                                dr.Is_Deleted = false;
                                dr.Inserted_By = name;
                                dr.Inserted_Date = today;
                                DbContext.tbl_DC_Registration.Add(dr);
                                DbContext.SaveChanges();

                                tbl_DC_Registration_Dtl dr1 = new tbl_DC_Registration_Dtl();
                                dr1.Secure_Id = questionid;
                                dr1.Answer = answer;
                                dr1.Regd_ID = dr.Regd_ID;
                                dr1.Is_Active = true;
                                dr1.Is_Deleted = false;
                                dr1.Inserted_By = name;
                                dr1.Inserted_Date = today;
                                DbContext.tbl_DC_Registration_Dtl.Add(dr1);
                                DbContext.SaveChanges();

                                var regno2 = DbContext.tbl_DC_Registration.Where(x => x.Mobile == mobile && x.Is_Active == true && x.Is_Deleted == false).Take(1).ToList();
                                int reg_num1 = Convert.ToInt32(regno2.ToList()[0].Regd_ID);
                                var pri = DbContext.tbl_DC_Prefix.Where(x => x.PrefixType_ID == 2).Select(x => x.Prefix_Name).FirstOrDefault();
                                string prifix = pri.ToString();

                                //registration no. of student autogenerate
                                if (reg_num1 == 0)
                                {
                                    dr1.Regd_No = prifix + today.ToString("yyyyMMdd") + "00000" + 1;
                                }
                                else
                                {
                                    int regnum = Convert.ToInt32(regno2.ToList()[0].Regd_ID);
                                    if (regnum > 0 && regnum <= 9)
                                    {
                                        dr1.Regd_No = prifix + today.ToString("yyyyMMdd") + "00000" + Convert.ToString(regnum);
                                    }
                                    if (regnum > 9 && regnum <= 99)
                                    {
                                        dr1.Regd_No = prifix + today.ToString("yyyyMMdd") + "0000" + Convert.ToString(regnum);
                                    }
                                    if (regnum > 99 && regnum <= 999)
                                    {
                                        dr1.Regd_No = prifix + today.ToString("yyyyMMdd") + "000" + Convert.ToString(regnum);
                                    }
                                    if (regnum > 999 && regnum <= 9999)
                                    {
                                        dr1.Regd_No = prifix + today.ToString("yyyyMMdd") + "00" + Convert.ToString(regnum);
                                    }
                                    if (regnum > 9999 && regnum <= 99999)
                                    {
                                        dr1.Regd_No = prifix + today.ToString("yyyyMMdd") + "0" + Convert.ToString(regnum);
                                    }
                                }
                                DbContext.Entry(dr1).State = EntityState.Modified;
                                DbContext.SaveChanges();
                                tbl_DC_Registration dr2 = DbContext.tbl_DC_Registration.Where(x => x.Mobile == mobile && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                                dr2.Regd_No = dr1.Regd_No;
                                DbContext.Entry(dr2).State = EntityState.Modified;
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
                                obj1.PASSWORD = Digichamps.Encrypt_Password.HashPassword(new_pass_word).ToString();
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

                                //Digichamps.otp obj6 = new Digichamps.otp();
                                //obj6.Regd_ID =dr.Regd_ID;
                                //obj6.Mobile = dr.Mobile;
                                //obj6.Otp = obj2.OTP;




                                if (email != null && email != "")
                                {
                                    var getall = DbContext.SP_DC_Get_maildetails("S_REG").FirstOrDefault();
                                    if (getall != null)
                                    {
                                        string msgbody = getall.EmailConf_Body.ToString().Replace("{{name}}", name).Replace("{{OTP}}", otp);
                                        sendMail1("S_REG", email, "Welcome to Digichamps", name, msgbody);
                                    }
                                }

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
                                bool chk = obj.submitQueryAPI(email, mobile, fname, lname);
                                var obj7 = new Digichamps.SuccessResults
                                {
                                    success = new Digichamps.otp
                                    {

                                        Regd_ID = dr.Regd_ID,
                                        Message = "You have successfully Registered"
                                    }
                                };
                                return Request.CreateResponse(HttpStatusCode.OK, obj7);

                            }
                            else
                            {
                                var obj10 = new Digichamps.ErrorResult
                                {
                                    error = new Digichamps.ErrorResponse
                                    {
                                        Message = "Mobile number is already registered"
                                    }
                                };
                                return Request.CreateResponse(HttpStatusCode.OK, obj10);
                            }
                        }
                        else
                        {
                            var obj20 = new Digichamps.ErrorResult
                            {
                                error = new Digichamps.ErrorResponse
                                {
                                    Message = "Please Enter Security Answer"
                                }
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, obj20);
                        }
                    }
                    else
                    {
                        var obj21 = new Digichamps.ErrorResult
                        {
                            error = new Digichamps.ErrorResponse
                            {
                                Message = "Please Select Security Question"
                            }
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, obj21);
                    }
                }
                else
                {
                    var obj11 = new Digichamps.ErrorResult
                    {
                        error = new Digichamps.ErrorResponse
                        {
                            Message = "Please Enter Mobile Number and Name"
                        }
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, obj11);
                }

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }

        protected string CreateRandomPassword(int PasswordLength)
        {
            try
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
            catch (Exception)
            {
                return null;
            }
        }

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
        public HttpResponseMessage resend_otp([FromUri] int? id)
        {

            try
            {
                if (id != null)
                {
                    var data = DbContext.tbl_DC_Registration.Where(x => x.Regd_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    if (data != null)
                    {

                        string usercode = "C0" + id.ToString();
                        var chk_type = DbContext.tbl_DC_USER_SECURITY.Where(x => x.USER_CODE == usercode && x.ROLE_CODE == "C").FirstOrDefault();

                        if (chk_type != null)
                        {
                            var widget = DbContext.tbl_DC_OTP.Where(x => x.Regd_Id == id).FirstOrDefault();
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
                                    var obj10 = new Digichamps.SuccessResult
                                    {
                                        success = new Digichamps.SuccessResponse
                                        {
                                            Message = "OTP send successfully"
                                        }
                                    };
                                    return Request.CreateResponse(HttpStatusCode.OK, obj10);
                                }
                                else
                                {
                                    var obj2 = new Digichamps.ErrorResult
                                    {
                                        error = new Digichamps.ErrorResponse
                                        {
                                            Message = "OTP can't send more than 3  times"
                                        }
                                    };
                                    return Request.CreateResponse(HttpStatusCode.OK, obj2);
                                }
                            }
                        }
                    }
                    else
                    {
                        var obj2 = new Digichamps.ErrorResult
                        {
                            error = new Digichamps.ErrorResponse
                            {
                                Message = "Invalid Registration Id"
                            }
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, obj2);
                    }
                }
                else
                {
                    var obj20 = new Digichamps.ErrorResult
                    {
                        error = new Digichamps.ErrorResponse
                        {
                            Message = "Please Provide Registration Id"
                        }
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, obj20);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
            return Request.CreateResponse(HttpStatusCode.OK);
        }

    }
}
