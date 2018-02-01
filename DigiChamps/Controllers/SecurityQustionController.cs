using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using DigiChamps.Models;
using System.Text.RegularExpressions;
using System.Text;
using System.Data.Entity;
namespace DigiChamps.Controllers
{
    public class SecurityQustionController : ApiController
    {
        DigiChampsEntities dbContext = new DigiChampsEntities();
        DateTime today = DigiChampsModel.datetoserver();
        [HttpPost]
        public HttpResponseMessage Security_question([FromBody]securityques secques)
        {
            try
            {
                if (secques.reg_id != null && secques.answer != "")
                {
                    var get_user = dbContext.tbl_DC_Registration_Dtl.Where(x => x.Regd_ID == secques.reg_id && x.Answer == secques.answer).FirstOrDefault();
                    if (get_user != null)
                    {
                        var get_user_reg = dbContext.tbl_DC_Registration.Where(x => x.Regd_ID == secques.reg_id).FirstOrDefault();
                        var get_user_from_sec = dbContext.tbl_DC_USER_SECURITY.Where(x => x.USER_NAME == get_user_reg.Mobile).FirstOrDefault();
                        int sessionid = 0;
                        //int[] logs_details = Login_status(get_user_from_sec.USER_CODE, dev_id);
                        //bool get_logdata = Convert.ToBoolean(dbContext.tbl_DC_LoginStatus.Where(x => x.Login_ID == get_user_from_sec.USER_CODE).OrderByDescending(x=>x.id).Select(x => x.Status).FirstOrDefault());
                        //if (get_logdata == true)
                        //{

                            tbl_DC_LoginStatus t_logi = new tbl_DC_LoginStatus();
                            t_logi.Login_ID = get_user_from_sec.USER_CODE;
                            t_logi.Login_IPAddress = "Android";
                            t_logi.Login_DateTime = DateTime.Now;

                            t_logi.Login_By = get_user_from_sec.USER_NAME;
                            t_logi.Status = true;
                            dbContext.tbl_DC_LoginStatus.Add(t_logi);
                            dbContext.SaveChanges();
                            int id = Convert.ToInt32(t_logi.id);
                            dbContext.login_fin_status(id);
                            sessionid = id;
                            var obj = new Digichamps_web_Api.sqresult1
                            {
                                Success = new Digichamps_web_Api.sqResponse1
                                {
                                    Message = "Answer is correct",
                                    AlreadyLogin = false,
                                    SessionID = sessionid,
                                    Regid = get_user_reg.Regd_ID,
                                }
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, obj);
                           // get_logdata = true;
                        //}
                        
                    }
                    else
                    {
                        var obj = new Digichamps_web_Api.Errorresult
                        {
                            Error = new Digichamps_web_Api.Errorresponse
                            {
                                Message = "Invalid details.",
                            }
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, obj);
                    }

                }
                else
                {
                    var obj = new Digichamps_web_Api.Errorresult
                    {
                        Error = new Digichamps_web_Api.Errorresponse
                        {
                            Message = "Invalid details.",
                        }
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, obj);
                }

            }
            catch (Exception)
            {

                var obj = new Digichamps_web_Api.Errorresult
                   {
                       Error = new Digichamps_web_Api.Errorresponse
                       {
                           Message = "Something went wrong.",
                       }
                   };
                return Request.CreateResponse(HttpStatusCode.OK, obj);
            }
            return Request.CreateResponse(HttpStatusCode.OK);
            
        }
        [HttpGet]
        public HttpResponseMessage Get_security(int ?id)
        {
            try
            {

                if (id != null)
                {
                    var data = dbContext.tbl_DC_Registration_Dtl.Where(x => x.Regd_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    if (data != null)
                    {
                        var qus = dbContext.tbl_DC_Security_Question.Where(x => x.Security_Question_ID == data.Secure_Id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        if (qus != null)
                        {
                            var obj = new Digichamps_web_Api.sqresult
                            {
                                Success = new Digichamps_web_Api.sqResponse
                                {
                                    Question = qus.Security_Question

                                }
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, obj);
                        }
                        else
                        {
                            var obj = new Digichamps_web_Api.Errorresult
                            {
                                Error = new Digichamps_web_Api.Errorresponse
                                {
                                    Message = "No quesion found.",
                                }
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, obj);
                        }
                    }
                    else
                    {
                        var obj = new Digichamps_web_Api.Errorresult
                        {
                            Error = new Digichamps_web_Api.Errorresponse
                            {
                                Message = "Invalid details.",
                            }
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, obj);
                    }
                }
                else
                {
                    var obj = new Digichamps_web_Api.Errorresult
                    {
                        Error = new Digichamps_web_Api.Errorresponse
                        {
                            Message = "Invalid details.",
                        }
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, obj);
                }
            }
            catch (Exception)
            {
                var obj = new Digichamps_web_Api.Errorresult
                    {
                        Error = new Digichamps_web_Api.Errorresponse
                        {
                            Message = "Something went wrong.",
                        }
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, obj);
            }
        }

        //public int[] Login_status(string redg, string dev)
        //{
        //    int[] log_id = new int[2];
        //    int login = Convert.ToInt32(Convert.ToString(redg).Trim().Substring(1));    
        //    var get_user = dbContext.tbl_DC_Registration.Where(x => x.Regd_ID == login).FirstOrDefault();
        //    if (get_user != null)
        //    {
        //        var get_log_details = dbContext.tbl_DC_LoginStatus.Where(x => x.Login_ID == redg && x.Status == true).ToList();
        //        if (get_log_details.Count > 0)
        //        {
        //            foreach(var v in get_log_details)
        //            {
        //                v.Status = false;
        //                v.Logout_DateTime = today;
        //                dbContext.SaveChanges();

        //            }
        //            tbl_DC_LoginStatus _log = new tbl_DC_LoginStatus();
        //            _log.Login_ID = redg;
        //            _log.Login_DateTime = today;
        //            _log.Status = true;
        //            _log.Login_By = get_user.Customer_Name;
        //            _log.Is_Android = true;
        //            _log.Login_IPAddress = dev;
        //            dbContext.tbl_DC_LoginStatus.Add(_log);
        //            dbContext.SaveChanges();
        //            log_id[0] = Convert.ToInt32(_log.id);
        //            log_id[1] = 200;
        //        }
        //        else
        //        {
        //            tbl_DC_LoginStatus log = new tbl_DC_LoginStatus();
        //            log.Login_ID = redg;
        //            log.Login_DateTime = today;
        //            log.Status = true;
        //            log.Login_By = get_user.Customer_Name;
        //            log.Is_Android = true;
        //            log.Login_IPAddress = dev;
        //            dbContext.tbl_DC_LoginStatus.Add(log);
        //            dbContext.SaveChanges();
        //            log_id[0] = Convert.ToInt32(log.id);
        //            log_id[1] = 200;

        //        }
        //    }
        //    else
        //    {
        //        log_id[0] = 0;
        //        log_id[1] = 404;
        //    }
        //    return log_id;
        //}

        [HttpGet]
        public HttpResponseMessage Security_otpsend(int? Regid)
        {

            if (Regid != 0)
            {

                string usercode = "C0" + Regid.ToString();
                var chk_type = dbContext.tbl_DC_USER_SECURITY.Where(x => x.USER_CODE == usercode && x.ROLE_CODE == "C").FirstOrDefault();

                if (chk_type != null)
                {
                    var widget = dbContext.tbl_DC_OTP.Where(x => x.Regd_Id == Regid).FirstOrDefault();
                    if (widget != null)
                    {
                        if (widget.Count <= 3 )
                        {
                            string otp = random_password(6);
                            widget.OTP = otp;
                            widget.Count = widget.Count + 1;
                            widget.From_Date = today;
                            widget.To_Date = today.AddHours(1);
                            dbContext.SaveChanges();

                            Sendsms("FRGPASS", otp.Trim(), widget.Mobile.Trim());

                            var obj10 = new Digichamps.OTPSuccessResult
                            {
                                success = new Digichamps.otp_confirm_message
                                {
                                    Regd_ID = Regid,
                                    Message = "Otp sent successfully "
                                }
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, obj10);
                        }
                        else
                        {
                            var obj = new Digichamps_web_Api.Errorresult
                            {
                                Error = new Digichamps_web_Api.Errorresponse
                                {
                                    Message = "Maximum number of otp sent",
                                }
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, obj);
                        }
                    }
                }
                else
                {
                    var obj = new Digichamps_web_Api.Errorresult
                    {
                        Error = new Digichamps_web_Api.Errorresponse
                        {
                            Message = "User not yet registered",
                        }
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, obj);
                }
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }
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
        public bool Sendsms(string type, string opt, string mobile)
        {
            try
            {
                var sms_obj = dbContext.View_DC_SMS_API.Where(x => x.Sms_Alert_Name == type).FirstOrDefault();
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
        public HttpResponseMessage security_confirm([FromBody]Digichamps.security_confirmation obj)
        {
            try
            {

                if (obj.Otp != null && obj.Regd_ID != null )
                {
                    var data = dbContext.tbl_DC_OTP.Where(x => x.OTP == obj.Otp).FirstOrDefault();
                    if (data != null)
                    {
                        if (today > data.From_Date && today < data.To_Date)
                        {
                           
                             

                                tbl_DC_Registration obj5 = dbContext.tbl_DC_Registration.Where(x => x.Regd_ID == data.Regd_Id).FirstOrDefault();
                                var securtity = dbContext.tbl_DC_USER_SECURITY.Where(x => x.USER_NAME == data.Mobile && x.STATUS == "A").FirstOrDefault();
                                if (obj5 != null)
                                {
                                    obj5.Is_Active = true;
                                    obj5.Is_Deleted = false;
                                    obj5.Modified_Date = today;

                                    data.OTP = null;
                                    //data.From_Date = null;
                                    //data.To_Date = null;
                                    dbContext.SaveChanges();
                                    if (securtity != null)
                                    {


                                        int sessionid = 0;
                               
                                            tbl_DC_LoginStatus t_logi = new tbl_DC_LoginStatus();
                                            t_logi.Login_ID = securtity.USER_CODE;
                                            t_logi.Login_IPAddress = "Android";
                                            t_logi.Login_DateTime = DateTime.Now;
                                            //var log_name = db.tbl_DC_USER_SECURITY.Where(x => x.USER_CODE == data1.Regd_No).Select(x => x.USER_NAME).FirstOrDefault();
                                            t_logi.Login_By = securtity.USER_NAME;
                                            t_logi.Status = true;
                                            t_logi.Is_Android = true;
                                            dbContext.tbl_DC_LoginStatus.Add(t_logi);
                                            dbContext.SaveChanges();
                                            int id = Convert.ToInt32(t_logi.id);
                                            dbContext.login_fin_status(id);
                                            sessionid = id;
                                          
                                        



                                        var obj10 = new Digichamps.OTPSuccessResult
                                        {
                                            success = new Digichamps.otp_confirm_message
                                            {
                                                Regd_ID = obj5.Regd_ID,
                                                Message = "Otp has successfully verified",
                                                AlreadyLogin = false,
                                                SessionID = sessionid,
                                            }
                                        };
                                        return Request.CreateResponse(HttpStatusCode.OK, obj10);
                                    }
                                    else
                                    {
                                        var obj26 = new Digichamps.ErrorResult
                                        {
                                            error = new Digichamps.ErrorResponse
                                            {
                                                Message = "User not yet registered"
                                            }
                                        };
                                        return Request.CreateResponse(HttpStatusCode.OK, obj26);
                                    }
                                }

                                else
                                {
                                    var obj11 = new Digichamps.ErrorResult
                                    {
                                        error = new Digichamps.ErrorResponse
                                        {
                                            Message = "User is invalid"
                                        }
                                    };
                                    return Request.CreateResponse(HttpStatusCode.OK, obj11);

                                }
                        }
                        else
                        {
                            var obj11 = new Digichamps.ErrorResult
                            {
                                error = new Digichamps.ErrorResponse
                                {
                                    Message = "OTP has Expired"
                                }
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, obj11);
                        }
                    }
                    else
                    {
                        var obj11 = new Digichamps.ErrorResult
                        {
                            error = new Digichamps.ErrorResponse
                            {
                                Message = "OTP is Invalid"
                            }
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, obj11);
                    }
                }
                else
                {
                    var obj11 = new Digichamps.ErrorResult
                    {
                        error = new Digichamps.ErrorResponse
                        {
                            Message = "Please enter all the details"
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
                      
         
    }

   
}

 
