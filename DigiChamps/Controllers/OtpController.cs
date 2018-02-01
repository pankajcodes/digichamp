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
    public class OtpController : ApiController
    {
        DigiChampsEntities DbContext = new DigiChampsEntities();
        DateTime today = Digichamps.datetoserver();
        [HttpPost]
        public HttpResponseMessage otp_confirm([FromBody]Digichamps.otp_confirmation obj)
        {
            try
            {

                if (obj.Otp != null && obj.New_password != null && obj.Confirm_password != null)
                {
                    var data = DbContext.tbl_DC_OTP.Where(x => x.OTP == obj.Otp).FirstOrDefault();
                    if (data != null)
                    {
                        if (today > data.From_Date && today < data.To_Date)
                        {
                            if (obj.New_password == obj.Confirm_password)
                            {
                                string ucode = "C0" + Convert.ToString(data.Regd_Id);
                                tbl_DC_USER_SECURITY obj4 = DbContext.tbl_DC_USER_SECURITY.Where(x => x.USER_CODE == ucode).FirstOrDefault();
                                obj4.PASSWORD = Digichamps.Encrypt_Password.HashPassword(obj.New_password).ToString();
                                obj4.STATUS = "A";
                                obj4.IS_ACCEPTED = true;
                                DbContext.Entry(obj4).State = EntityState.Modified;
                                //FormsAuthentication.RedirectFromLoginPage(obj4.USER_NAME, false);
                                DbContext.SaveChanges();

                                tbl_DC_Registration obj5 = DbContext.tbl_DC_Registration.Where(x => x.Regd_ID == data.Regd_Id).FirstOrDefault();
                                obj5.Device_id = obj.Device_Id;
                                obj5.Is_Active = true;
                                obj5.Is_Deleted = false;
                                obj5.Modified_Date = today;
                                DbContext.Entry(obj5).State = EntityState.Modified;
                                data.OTP = null;
                                //data.From_Date = null;
                                //data.To_Date = null;
                                DbContext.SaveChanges();

                                //tbl_dc_loginstatus t_logi = new tbl_dc_loginstatus();
                                //t_logi.login_id = user_name.user_code;
                                //t_logi.login_ipaddress = "android";
                                //t_logi.login_datetime = datetime.now;
                                ////var log_name = dbcontext.tbl_dc_user_security.where(x => x.user_code == user_name.user_code).select(x => x.user_name).firstordefault();
                                //t_logi.login_by = user_name.user_name;
                                //t_logi.status = true;
                                //dbcontext.tbl_dc_loginstatus.add(t_logi);
                                //dbcontext.savechanges();
                                //int id = convert.toint32(t_logi.id);
                                //dbcontext.login_fin_status(id);
                                //sessionid = id;

                                var obj10 = new Digichamps.OTPSuccessResult
                                {
                                    success = new Digichamps.otp_confirm_message
                                    {
                                        Regd_ID = obj5.Regd_ID,
                                        Message = "Otp has successfully verified",
                                        Mobile = ""
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
                                        Message = "Password doesnot match"
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
