using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using DigiChamps.Models;

namespace DigiChamps.Controllers
{
    public class feedbackController : ApiController
    {
        DigiChampsEntities db = new DigiChampsEntities();
        DateTime today = DigiChampsModel.datetoserver();

        public class ErrorResult
        {
            public ErrorResponse Error { get; set; }
        }

        public class ErrorResponse
        {
            public string Message { get; set; }
        }

        public class SuccessResponse
        {
            public string Message { get; set; }
        }
        public class SuccessResult
        {
            public SuccessResponse Success { get; set; }
        }     

        public class feedback
        {
            public int? Regd_Id { get; set;}
          //  public string Usercode { get; set; }
            public string Remark { get; set; }
            public decimal? Rating { get; set; }
            public DateTime? Inserted_Date { get; set; }
            public string Inserted_By { get; set; }
            public DateTime? Modified_Date { get; set; }
            public string Modified_By { get; set; }
            public Nullable<bool> Is_Active { get; set; }
            public Nullable<bool> Is_Deleted { get; set; }

        }
        [HttpPost]
        public HttpResponseMessage Addfeedback([FromBody] feedback obj)
        {
          
            try
            {
                
                    if (obj.Regd_Id != null && obj.Rating != null)
                    {
                        var data = db.tbl_DC_Feedback.Where(x => x.Regd_Id == obj.Regd_Id).FirstOrDefault();
                        if (data != null)
                        {
                            data.Regd_Id = obj.Regd_Id;
                            //data.Usercode = obj.Usercode;
                            data.Remark = obj.Remark;
                            data.Rating = obj.Rating;
                            data.Modified_By = Convert.ToString(obj.Regd_Id);
                            data.Modified_Date = today;

                            db.Entry(data).State = EntityState.Modified;
                            db.SaveChanges();

                            var result = new SuccessResult { Success = new SuccessResponse { Message = "Thank you for your valuable Feedback. Our Customer support team will get in touch with you shortly" } };
                          return Request.CreateResponse(HttpStatusCode.OK, result);
                        }
                        else
                        {

                            tbl_DC_Feedback tr = new Models.tbl_DC_Feedback();
                            tr.Regd_Id = obj.Regd_Id;
                           // tr.Usercode = obj.Usercode;
                            tr.Remark = obj.Remark;
                            tr.Rating = obj.Rating;
                            tr.Inserted_Date = today;
                            tr.Inserted_By =Convert.ToString(obj.Regd_Id);
                            tr.Is_Active = true;
                            tr.Is_Deleted = false;
                            db.tbl_DC_Feedback.Add(tr);
                            db.SaveChanges();
                            var result = new SuccessResult { Success = new SuccessResponse { Message = "Thank you for your valuable Feedback. Our Customer support team will get in touch with you shortly" } };
                           return Request.CreateResponse(HttpStatusCode.OK, result);
                        }

                    }
               
             }
            catch (Exception)
            {
              var  result = new ErrorResult { Error = new ErrorResponse { Message = " Something went wrong." } };
              return Request.CreateResponse(HttpStatusCode.InternalServerError, result);
            }
            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}
