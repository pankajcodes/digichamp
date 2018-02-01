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
    public class FAQController : ApiController
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
          
            public List<FAQ> Faqs { get; set; }
        }


        public class FAQ
        {
            public int? FAQ_Id { get; set; }
             public string Answer { get; set; }
            public string Question { get; set; }
          
        

        }

        public class FAQ_List
        {
            public List<FAQ> list { get; set; }
        }
        public class success_FAQ
        {
            public FAQ_List Success { get; set; }
        }
        public class Errorresult
        {
            public Errorresponse Error { get; set; }
        }
        public class Errorresponse
        {
            public string Message { get; set; }
        }
        [HttpGet]
        public HttpResponseMessage GetFAQ()
        {
            try
            {
                var obj = new success_FAQ
                {
                    Success = new FAQ_List
                    {
                        list = (from c in db.tbl_DC_FAQs.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                select new FAQ
                                {

                                    FAQ_Id = c.FAQs_ID,
                                    Answer = c.FAQ_Answer,
                                    Question = c.FAQ
                                }).ToList(),

                    }
                };
              
                return Request.CreateResponse(HttpStatusCode.OK, obj);
            }
            catch (Exception)
            {

                var obj = new Errorresult
                {
                    Error = new Errorresponse
                    {
                        Message = "Something went wrong."
                    }
                };
                return Request.CreateResponse(HttpStatusCode.OK, obj);
            }
        }
        }
           

        

    }

