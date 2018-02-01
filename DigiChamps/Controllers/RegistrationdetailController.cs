using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using DigiChamps.Models;
using System.Data.Entity;

namespace digichamps_api.Controllers
{
    [Authorize]
    public class RegistrationdetailController : ApiController
    {
        DigiChampsEntities db = new DigiChampsEntities();
        DateTime today = Digichamps.datetoserver();
        // GET: api/Registrationdetail
        [HttpGet]
        public HttpResponseMessage getquestion()
        {
            try
            {
                var obj10 = new Digichamps.success_ques
                {
                    success = new Digichamps.Question_List
                    {
                        question_list = (from c in db.tbl_DC_Security_Question.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                         select new Digichamps.Security_Question
                                         {
                                             Security_Question_ID = c.Security_Question_ID,
                                             Security_Questions = c.Security_Question,
                                         }).ToList(),

                    }
                };
                return Request.CreateResponse(HttpStatusCode.OK, obj10);

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }
       [HttpPost]
       public HttpResponseMessage registration_detail([FromBody]Digichamps.Registration_Detail obj)
       {
           try
           {
               if (obj.Board_ID != null)
               {
                   if (obj.Class_ID != null)
                   {
                       if (obj.Gender != null)
                       {
                           if (obj.DateOfBirth != null)
                           {
                                string[] dobb = obj.DateOfBirth.Split('/');
                                string yy = dobb[2];
                                string mm = dobb[1];
                                string dd = dobb[0];
                                DateTime dob = Convert.ToDateTime(yy+"-"+mm+"-"+dd);
                               var data = db.tbl_DC_Registration.Where(x => x.Regd_ID == obj.Regd_ID && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                               if (data != null)
                               {
                                   var securtity = db.tbl_DC_USER_SECURITY.Where(x => x.USER_NAME == data.Mobile && x.STATUS == "A").FirstOrDefault();

                                   var data1 = db.tbl_DC_Registration_Dtl.Where(x => x.Regd_ID == obj.Regd_ID && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                                   if (data1 != null)
                                   {


                                       data1.Board_ID = obj.Board_ID;
                                       data1.Class_ID = obj.Class_ID;
                                       //obj1.Regd_ID = null ;
                                       db.Entry(data1).State = EntityState.Modified;
                                       db.SaveChanges();

                                    //   tbl_DC_Registration obj2 = new tbl_DC_Registration();
                                       data.Gender = obj.Gender;
                                       data.DateOfBirth = dob;
                                       db.Entry(data).State = EntityState.Modified;
                                       db.SaveChanges();
                                       if (securtity != null)
                                       {
                                           int sessionid = 0;
                                           //bool get_logdata = Convert.ToBoolean(db.tbl_DC_LoginStatus.Where(x => x.Login_ID == securtity.USER_CODE).OrderByDescending(x => x.id).Select(x => x.Status).FirstOrDefault());
                                           //if (get_logdata == false)
                                           //{

                                           tbl_DC_LoginStatus t_logi = new tbl_DC_LoginStatus();
                                           t_logi.Login_ID = securtity.USER_CODE;
                                           t_logi.Login_IPAddress = "Android";
                                           t_logi.Login_DateTime = DateTime.Now;
                                           //var log_name = db.tbl_DC_USER_SECURITY.Where(x => x.USER_CODE == data1.Regd_No).Select(x => x.USER_NAME).FirstOrDefault();
                                           t_logi.Login_By = securtity.USER_NAME;
                                           t_logi.Status = true;
                                           db.tbl_DC_LoginStatus.Add(t_logi);
                                           db.SaveChanges();
                                           int id = Convert.ToInt32(t_logi.id);
                                           db.login_fin_status(id);
                                           sessionid = id;



                                           var obj10 = new Digichamps.SuccessregistratonResult
                                           {
                                               success = new Digichamps.SuccessregistratonResponse
                                               {
                                                   Message = "Registration Completed Successfully",
                                                   AlreadyLogin = false,
                                                   SessionID = sessionid,
                                                   Regid = data1.Regd_ID,
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
                                       var obj25 = new Digichamps.ErrorResult
                                       {
                                           error = new Digichamps.ErrorResponse
                                           {
                                               Message = "Invalid registration number"
                                           }
                                       };
                                       return Request.CreateResponse(HttpStatusCode.OK, obj25);
                                   }

                               }
                               else
                               {
                                   var obj15 = new Digichamps.ErrorResult
                                   {
                                       error = new Digichamps.ErrorResponse
                                       {
                                           Message = "Invalid registration number"
                                       }
                                   };
                                   return Request.CreateResponse(HttpStatusCode.OK, obj15);
                               }
                           }
                           else
                           {
                               var obj12 = new Digichamps.ErrorResult
                               {
                                   error = new Digichamps.ErrorResponse
                                   {
                                       Message = "Please Enter Date Of Birth"
                                   }
                               };
                               return Request.CreateResponse(HttpStatusCode.OK, obj12);
                           }
                       }
                       else
                       {
                           var obj11 = new Digichamps.ErrorResult
                           {
                               error = new Digichamps.ErrorResponse
                               {
                                   Message = "Please Selct Your Gender"
                               }
                           };
                           return Request.CreateResponse(HttpStatusCode.OK, obj11);
                       }
                   }
                   else
                   {
                       var obj13 = new Digichamps.ErrorResult
                       {
                           error = new Digichamps.ErrorResponse
                           {
                               Message = "Please select class name"
                           }
                       };
                       return Request.CreateResponse(HttpStatusCode.OK, obj13);
                   }
               }
               else
               {
                   var obj14 = new Digichamps.ErrorResult
                   {
                       error = new Digichamps.ErrorResponse
                       {
                           Message = "Please select board name"
                       }
                   };
                   return Request.CreateResponse(HttpStatusCode.OK, obj14);
               }

           }

           catch (Exception ex)
           {
               return Request.CreateResponse(HttpStatusCode.InternalServerError);
           }
           
       }
    
    }
}
