using DigiChamps.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web.Http;
using System.Web.Security;

namespace DigiChamps.Controllers
{
    [Authorize]
    public class LoginController : ApiController
    {
        DigiChampsEntities dbContext = new DigiChampsEntities();
        DateTime today = DigiChampsModel.datetoserver();
        public bool IsNumeric(string value)
        {
            return Regex.IsMatch(value, "^[7-9][0-9]{9}$");
        }

        public class Login_Model
        {
            public string USER_NAME { get; set; }
            public string PASSWORD { get; set; }
            public string Device_ID { get; set; }
        }
        [ActionName("login")]

        [HttpPost]
        public HttpResponseMessage Login([FromBody] Login_Model request)
        {

            //var authey = dbContext.tbl_DC_USER_SECURITY.Where(x => x.ROLE_CODE == "A").FirstOrDefault();

            //AuthKey ob = new AuthKey();
            //string apikey = ob.getsignedurl(authey.USER_NAME, authey.PASSWORD);

            //if (apikey == Auth_Key && Auth_Key!="")
            //{

            bool eml = true, mon = true;


            // result obj = new result();
            if (request.USER_NAME != null)
            {
                mon = IsNumeric(request.USER_NAME);

                if (mon == false)
                {
                    var obj = new Digichamps_web_Api.Errorresult
                    {
                        Error = new Digichamps_web_Api.Errorresponse
                        {
                            Message = "Mobile number is invalid",
                        }
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, obj);
                }
            }
            if (request.PASSWORD != null)
            {
                if (request.PASSWORD.Length < 6)
                {
                    var obj = new Digichamps_web_Api.Errorresult
                    {
                        Error = new Digichamps_web_Api.Errorresponse
                        {
                            Message = "Password must be atleast 6 characters",

                        }
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, obj);
                }
            }


            Digichamps_web_Api.LoginResponse responseLoginAsync = new Digichamps_web_Api.LoginResponse();
            if (request.USER_NAME != null && request.PASSWORD != null)
            {
                var user_name = (from c in dbContext.tbl_DC_USER_SECURITY.Where(x => x.USER_NAME == request.USER_NAME) select c).FirstOrDefault();
                if (user_name != null)
                {
                    string pass_word = DigiChampsModel.Encrypt_Password.HashPassword(request.PASSWORD);
                    if (user_name.PASSWORD == pass_word)
                    {
                        if (user_name.ROLE_CODE == "C")
                        {
                            if (user_name.IS_ACCEPTED == true)
                            {
                                var reguser = dbContext.tbl_DC_Registration.Where(x => x.Mobile == user_name.USER_NAME && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                                int regid = Convert.ToInt32(reguser.Regd_ID);

                                int msg = 0;

                                var chkbrd_cls = dbContext.tbl_DC_Registration_Dtl.Where(x => x.Regd_ID == regid && x.Is_Active == true && x.Is_Deleted == false && x.Board_ID != null && x.Class_ID != null).FirstOrDefault();
                                if (chkbrd_cls == null)
                                {
                                    msg = 0; //choose board
                                }
                                else
                                {

                                    msg = 1; //board already there
                                }
                                bool get_logdata = false;
                                int sessionid = 0;
                                if (chkbrd_cls != null)
                                {
                                    get_logdata = Convert.ToBoolean(dbContext.tbl_DC_LoginStatus.Where(x => x.Login_ID == user_name.USER_CODE).OrderByDescending(x => x.id).Select(x => x.Status).FirstOrDefault());
                                    if (get_logdata == false)
                                    {

                                        tbl_DC_LoginStatus t_logi = new tbl_DC_LoginStatus();
                                        t_logi.Login_ID = user_name.USER_CODE;
                                        t_logi.Login_IPAddress = "Android";
                                        t_logi.Login_DateTime = DateTime.Now;
                                        //var log_name = dbContext.tbl_DC_USER_SECURITY.Where(x => x.USER_CODE == user_name.USER_CODE).Select(x => x.USER_NAME).FirstOrDefault();
                                        t_logi.Login_By = user_name.USER_NAME;
                                        t_logi.Status = true;
                                        dbContext.tbl_DC_LoginStatus.Add(t_logi);
                                        dbContext.SaveChanges();
                                        int id = Convert.ToInt32(t_logi.id);
                                        dbContext.login_fin_status(id);
                                        sessionid = id;

                                    }
                                }
                                tbl_DC_Registration regu = dbContext.tbl_DC_Registration.Where(x => x.Mobile == user_name.USER_NAME && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                                if (regu != null)
                                {
                                    regu.Device_id = request.Device_ID;
                                    dbContext.SaveChanges();
                                }
                                var obj = new Digichamps_web_Api.result
                                {
                                    Success = new Digichamps_web_Api.LoginResponse
                                    {
                                        Message = "You have successfully logged in",
                                        UserName = user_name.USER_NAME.ToString(),
                                        Regid = regid,
                                        logtime = today,
                                        Isselect = msg,
                                        AlreadyLogin = get_logdata,
                                        SessionID = sessionid,



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
                                        Message = "Please Login as a student.",
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
                                    Message = "Incorrect Password.",
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
                                Message = "Incorrect Password.",
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
                            Message = "Incorrect username or password",
                        }
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, obj);

                }
                //}
                //else
                //{

                //    return Request.CreateResponse(HttpStatusCode.Unauthorized, "Unauthorized access");
                //}


            }
            return Request.CreateResponse(HttpStatusCode.OK);
        }



        [HttpGet]
        public HttpResponseMessage loginstatus(int? id, int? eid)
        {
            try
            {
                if (id != null)
                {
                    if (eid != null)
                    {
                        var obj = dbContext.tbl_DC_Registration.Where(x => x.Regd_ID == eid && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        if (obj != null)
                        {
                            var data = dbContext.tbl_DC_LoginStatus.Where(x => x.id == id).FirstOrDefault();
                            if (data != null)
                            {
                                var status = data.Status;
                            }

                            var obj3 = new Digichamps.Status_Response
                            {
                                Success = new Digichamps.Status_Message
                                {
                                    Status = data.Status,
                                    Regd_Id = eid
                                }
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, obj3);
                        }
                        else
                        {
                            var obj1 = new Digichamps.errormessage
                            {
                                error = new Digichamps.Display
                                {
                                    Message = "Invalid  registration id"
                                }
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, obj1);
                        }

                    }
                    else
                    {
                        var obj1 = new Digichamps.errormessage
                        {
                            error = new Digichamps.Display
                            {
                                Message = "Please provide registration id"
                            }
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, obj1);
                    }
                }
                else
                {
                    var obj1 = new Digichamps.errormessage
                    {
                        error = new Digichamps.Display
                        {
                            Message = "Please provide session id"
                        }
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, obj1);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
            return Request.CreateResponse(HttpStatusCode.OK);

        }



        [HttpGet]
        public HttpResponseMessage logout(int? id, int? eid)
        {
            try
            {
                if (id != null)
                {
                    if (eid != null)
                    {
                        var obj = dbContext.tbl_DC_Registration.Where(x => x.Regd_ID == eid && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        if (obj != null)
                        {
                            var data = dbContext.tbl_DC_LoginStatus.Where(x => x.id == id).FirstOrDefault();
                            if (data != null)
                            {
                                data.Logout_DateTime = today;
                                data.Status = false;
                                dbContext.Entry(data).State = EntityState.Modified;
                                dbContext.SaveChanges();
                                var obj1 = new Digichamps.SuccessResult
                                {
                                    success = new Digichamps.SuccessResponse
                                    {
                                        Message = "You have successfully logged out "
                                    }

                                };
                                return Request.CreateResponse(HttpStatusCode.OK, obj1);

                            }
                            else
                            {
                                var obj1 = new Digichamps.errormessage
                                {
                                    error = new Digichamps.Display
                                    {
                                        Message = "Invalid  session id"
                                    }
                                };
                                return Request.CreateResponse(HttpStatusCode.OK, obj1);
                            }

                        }
                        else
                        {
                            var obj1 = new Digichamps.errormessage
                            {
                                error = new Digichamps.Display
                                {
                                    Message = "Invalid registration id"
                                }
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, obj1);
                        }
                    }
                    else
                    {
                        var obj1 = new Digichamps.errormessage
                        {
                            error = new Digichamps.Display
                            {
                                Message = "Please provide registration id"
                            }
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, obj1);
                    }
                }
                else
                {
                    var obj1 = new Digichamps.errormessage
                    {
                        error = new Digichamps.Display
                        {
                            Message = "Please provide session id"
                        }
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, obj1);
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