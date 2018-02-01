using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data;
using System.Data.Entity;
using System.Web;
using DigiChamps.Models;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Net.Mail;


namespace DigiChamps.Controllers
{
    [Authorize]
    public class ContactController : ApiController
    {
        public class Contact
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string EmailID { get; set; }
            public string Subject { get; set; }
            public string Message { get; set; }
        }
         public class Successresult
        {
            public Successrespone Success { get; set; }
        }

        public class Successrespone
        {
            public string Message { get; set; }
        }
         public class Errorresult
        {
            public Errorresponse Error { get; set; }
        }
        public class Errorresponse
        {
            public string Message { get; set; }
        }
        [HttpPost]
        public HttpResponseMessage Contact_Us([FromBody]Contact obj1)
        {
            string msg = string.Empty;
           
            try
            {
               
                if (obj1.EmailID!=null)
                {
                    if(obj1.FirstName!=null)
                    {
                        if(obj1.LastName!=null)
                        {
                            if(obj1.Message!=null)
                            {
                                if(obj1.Subject!=null)
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
                                    if (obj1.LastName != null)
                                    {
                                        sb.Append("<strong>Mr/Ms " + obj1.FirstName + "</strong></p>");
                                    }
                                    else
                                    {
                                        sb.Append("<strong>Mr/Ms " + obj1.FirstName + " " + obj1.LastName + "'</strong></p>");
                                    }

                                    sb.Append("<p>");
                                    sb.Append("<br/> Email : <span style='color: white'> " + obj1.EmailID + "</span> <br/>  Subject : <span style='color: white'> " + obj1.Subject + " </span>.<br />Message : <span style='color: white'> " + obj1.Message + "</span> </p>");
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

                                    DMLhelper.sendMail1("info@thedigichamps.com", obj1.EmailID, "Regarding Contact", sb.ToString());
                                    msg = "2";
                                    LeadsSquaredAPI obj = new Models.LeadsSquaredAPI();
                                    bool chk = obj.submitQueryAPI(obj1.EmailID, "", obj1.FirstName, obj1.LastName);
                                    var result=new Successresult
                                     {
                                        Success=new Successrespone
                                        {
                                            Message="Thank you for contacting us"
                                        }
                                    };
                                    return Request.CreateResponse(HttpStatusCode.OK, result);
                                }
                                else
                                {
                                   var  result=new Errorresult
                                     {
                                        Error=new Errorresponse
                                        {
                                            Message="Please provide subject"
                                        }
                                    };
                                   return Request.CreateResponse(HttpStatusCode.OK, result);
                                }
                            }
                             else
                             {
                               var result=new Errorresult
                                {
                                    Error=new Errorresponse
                                    {
                                        Message="Please provide message"
                                    }
                                };
                               return Request.CreateResponse(HttpStatusCode.OK, result);
                        }
                        }
                        else
                        {
                           var result=new Errorresult
                            {
                                Error=new Errorresponse
                                {
                                    Message="Please provide lastname"
                                }
                            };
                           return Request.CreateResponse(HttpStatusCode.OK, result);
                        }
                    }
                    else
                    {
                       var  result=new Errorresult
                            {
                                Error=new Errorresponse
                                {
                                    Message="Please provide firstname"
                                }
                            };
                       return Request.CreateResponse(HttpStatusCode.OK, result);
                    }
                }
                else
                {
                    var result=new Errorresult
                            {
                                Error=new Errorresponse
                                {
                                    Message="Please provide emailid"
                                }
                            };
                    return Request.CreateResponse(HttpStatusCode.OK, result);
                }
               
            }
      
              catch(Exception ex)
                {
                   var result = new Errorresult
                    {
                        Error = new Errorresponse
                        {
                            Message = "Something went wrong"
                        }
                    };
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, result);
               }
            return Request.CreateResponse(HttpStatusCode.OK);
            }
                   
        }
       
    }

