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
    public class ForgotPasswordController : ApiController
    {
        DigiChampsEntities DbContext = new DigiChampsEntities();
        DateTime today = Digichamps.datetoserver();
        // GET: api/ForgotPassword
       [HttpPost]
        public HttpResponseMessage password_change([FromBody]Digichamps.Student_Edit obj)
        {
            try
            {
               
                    if (obj.Mobile != "" && obj.Mobile != null)
                    {
                        tbl_DC_Registration obj1 = DbContext.tbl_DC_Registration.Where(x => x.Mobile == obj.Mobile).FirstOrDefault();
                        if (obj1 == null)
                        {
                            var obj10 = new Digichamps.ErrorResult
                            {
                                error = new Digichamps.ErrorResponse
                                {
                                    Message = "Mobile number is not yet registered"
                                }
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, obj10);
                        }
                        else
                        {
                            tbl_DC_USER_SECURITY obj20 = DbContext.tbl_DC_USER_SECURITY.Where(x => x.USER_NAME == obj.Mobile && x.IS_ACCEPTED == true).FirstOrDefault();

                            int reg_id = Convert.ToInt32(obj1.Regd_ID);
                            tbl_DC_OTP obj2 = DbContext.tbl_DC_OTP.Where(x => x.Regd_Id == reg_id).FirstOrDefault();
                            if (obj1 != null)
                            {
                                if (obj2.To_Date < today)
                                {
                                    string otp = random_password(6);
                                    obj2.OTP = otp;
                                    obj2.From_Date = today;
                                    obj2.Count = 1;
                                    obj2.To_Date = Convert.ToDateTime(today.AddHours(1));
                                    DbContext.Entry(obj1).State = EntityState.Modified;
                                    DbContext.SaveChanges();

                                    Sendsms("FRGPASS", otp.Trim(), obj1.Mobile.Trim());  //send otp

                                    if (obj1.Email != null && obj1.Email != "")
                                    {
                                        var getall = DbContext.SP_DC_Get_maildetails("ST_FPASS").FirstOrDefault();
                                        if (getall != null)
                                        {
                                            string msgbody = getall.EmailConf_Body.ToString().Replace("{{name}}", obj1.Customer_Name).Replace("{{OTP}}", otp);
                                            sendMail1("ST_FPASS", obj1.Email, "Digichamps Reset Password", obj1.Customer_Name, msgbody);
                                        }
                                    }
                                    var obj30 = new Digichamps.OTPSuccessResult
                                    {
                                        success = new Digichamps.otp_confirm_message
                                        {
                                            Regd_ID = obj1.Regd_ID,
                                            Message = "Otp has been send to your registered  mobile number"

                                        }
                                    };

                                    return Request.CreateResponse(HttpStatusCode.OK, obj30);
                                }
                                else if (obj2.Count <= 3)
                                {
                                    string otp = random_password(6);
                                    obj2.OTP = otp;
                                    obj2.From_Date = today;
                                    obj2.Count = obj2.Count + 1;
                                    obj2.To_Date = Convert.ToDateTime(today.AddHours(1));
                                    DbContext.Entry(obj1).State = EntityState.Modified;
                                    DbContext.SaveChanges();

                                    Sendsms("FRGPASS", otp.Trim(), obj.Mobile.Trim());

                                    if (obj1.Email != null && obj1.Email != "")
                                    {
                                        var getall = DbContext.SP_DC_Get_maildetails("ST_FPASS").FirstOrDefault();
                                        if (getall != null)
                                        {
                                            string msgbody = getall.EmailConf_Body.ToString().Replace("{{name}}", obj.Customer_Name).Replace("{{OTP}}", otp);
                                            sendMail1("ST_FPASS", obj1.Email, "Digichamps Reset Password", obj1.Customer_Name, msgbody);
                                        }
                                    }

                                    var obj10 = new Digichamps.OTPSuccessResult
                                    {
                                        success = new Digichamps.otp_confirm_message
                                        {
                                            Regd_ID = obj1.Regd_ID,
                                            Message = "Otp has been send to your registered  mobile number"
                                        }
                                    };
                                    return Request.CreateResponse(HttpStatusCode.OK, obj10);
                                }
                                else
                                {
                                    var obj11 = new Digichamps.ErrorResult
                                    {
                                        error = new Digichamps.ErrorResponse
                                        {
                                            Message = "You have already requested for Maximum no of OTP."
                                        }
                                    };
                                    return Request.CreateResponse(HttpStatusCode.OK, obj11);
                                }

                            }
                        }
                    }
                    else
                    {
                        var obj11 = new Digichamps.ErrorResult
                        {
                            error = new Digichamps.ErrorResponse
                            {
                                Message = "Please enter mobile number."
                            }
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, obj11);
                    }
              
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
            return Request.CreateResponse(HttpStatusCode.OK);
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
    }
}
