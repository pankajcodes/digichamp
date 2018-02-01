using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.Entity;
using DigiChamps.Models;
using DigiChamps.Controllers;

namespace DigiChamps.Controllers
{
    [Authorize]
    public class dashboardController : ApiController
    {
        DigiChampsEntities DbContext = new DigiChampsEntities();
        DateTime today = DigiChampsModel.datetoserver();
        // GET api/<controller>
        [HttpGet]
        public HttpResponseMessage GetDashboard(int? id)
        {
            try { 
               
                    var data1 = (from k in DbContext.tbl_DC_Registration.Where(x => x.Regd_ID == id && x.Is_Active == true && x.Is_Deleted == false)
                                 join l in DbContext.tbl_DC_Registration_Dtl.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                     on k.Regd_ID equals l.Regd_ID
                                 join m in DbContext.tbl_DC_Class.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                 on l.Class_ID equals m.Class_Id
                                 select new PackagePreviewModel
                                 {
                                     Board_Id = l.Board_ID,
                                     Class_Id = l.Class_ID,
                                     Class_Name = m.Class_Name
                                 }).FirstOrDefault();
                    if (data1 != null)
                    {
                        //----------------Schedule Test ------------------------

                        var ord_customer = DbContext.tbl_DC_Order.Where(x => x.Regd_ID == id && x.Is_Paid == true && x.Is_Active == true && x.Is_Deleted == false).ToList();
                        int retord_customer = ord_customer.Count;
                        //DateTime dt = today.Date.AddDays(-6);
                        //var time = (from a in DbContext.tbl_DC_Exam.Where(x => x.Exam_type == 4 && x.Is_Active == true && x.Is_Deleted == false && x.Shedule_date >= dt && x.Shedule_date <= today.Date) select a).ToList();
                        //List<DigiChamps.Models.tbl_DC_Exam> rettime = new List<DigiChamps.Models.tbl_DC_Exam>();
                        //if (time.Count > 0)
                        //{
                        //    rettime = time.ToList();
                        //}
                        //else
                        //{
                        //    rettime = time;
                        //}

                    string date = string.Empty;
                    string time = string.Empty;
                    string sec = string.Empty;
                    int Exam_ID = 0;
                    int Exam_Type = 0;
                    
                    var data2 = DbContext.tbl_DC_Registration_Dtl.Where(x => x.Regd_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    if (data2 != null)
                    {
                        var datelist = DbContext.tbl_DC_Exam.Where(x => x.Is_Active == true && x.Is_Deleted == false && x.Exam_type == 4 && x.Class_Id == data2.Class_ID && x.Shedule_date >= today).OrderBy(x => x.Shedule_time).FirstOrDefault();
                        if (datelist != null)
                        {
                            var chapters = DbContext.VW_DC_Package_Learn.Where(x => x.Regd_ID == id && x.Class_ID == data2.Class_ID).ToList();
                            if (chapters.Count > 0)
                            {
                                Exam_ID = datelist.Exam_ID;
                                Exam_Type = Convert.ToInt32(datelist.Exam_type);
                                date = Convert.ToDateTime(datelist.Shedule_date).ToString("yyyy-MM-dd");
                                time = datelist.Shedule_time + ":00";
                                string acctualtime = date + " " + time;
                                DateTime dt = Convert.ToDateTime(acctualtime);
                                int dt1 = (dt.Date - today.Date).Days;
                                if (dt1 <= 6)
                                {
                                    TimeSpan diff = dt - today;
                                    sec = diff.ToString();
                                }
                            }
                        }
                    }

                        //----------------Ticket pie chart----------------------
                        var data = DbContext.tbl_DC_Ticket.Where(x => x.Student_ID == id && x.Is_Active == true && x.Is_Deleted == false).ToList();

                        var retRaised = 0;
                        var retopen = 0;
                        var retReject = 0;
                        var retClosed = 0;
                        if (data.Count > 0)
                        {
                            retRaised = data.Count;
                            decimal total_ticket = Convert.ToDecimal(data.Count);
                            int perecent = Convert.ToInt32(100 / total_ticket);
                            retopen = Convert.ToInt32(DbContext.View_DC_Tickets_and_Teacher.Where(x => x.Student_ID == id && x.Status != "R" && x.Status != "C").ToList().Count()) * perecent;
                            retReject = Convert.ToInt32(DbContext.tbl_DC_Ticket.Where(x => x.Student_ID == id && x.Status == "R" && x.Is_Active == true && x.Is_Deleted == false).Count()) * perecent;
                            retClosed = Convert.ToInt32(DbContext.tbl_DC_Ticket.Where(x => x.Student_ID == id && x.Status == "C" && x.Is_Active == true && x.Is_Deleted == false).Count()) * perecent;
                        }
                        //------------------------test apper and  package--------------------
                        var test_apr = DbContext.tbl_DC_Exam_Result.Where(x => x.Regd_ID == id).OrderByDescending(x => x.EndTime).Take(5).ToList();
                        var pacakage_detail = DbContext.SP_DC_Order_Details(id).OrderByDescending(x => x.Expiry_Date).ToList();
                        var result = 0;
                        if (pacakage_detail.Count > 0)
                        {

                            foreach (var s in pacakage_detail)
                            {
                                int NumberOfWords = s.Chapters.Split(',').Length;
                                result += NumberOfWords;
                                

                            }
                            //var result = pacakage_detail.Max(x => x.Chapters.Split(',').Length);
                        }
                        else
                        {
                            result = 0;
                        }

                        List<DigiChamps.Models.tbl_DC_Exam_Result> rettest = new List<tbl_DC_Exam_Result>();
                        List<DigiChamps.Models.SP_DC_Order_Details_Result> retpackagedetail = new List<SP_DC_Order_Details_Result>();
                        if (test_apr.Count > 0)
                        {
                            rettest = test_apr;                          
                        }
                        var test_apr1 = test_apr.FirstOrDefault();
                        string ename = "";
                        int qno = 0;
                        int qattemtp = 0;
                        int qanswer = 0;
                        if(test_apr1 !=null)
                        {
                            ename = test_apr1.Exam_Name;
                            qno = (Int32)test_apr1.Question_Nos;
                            qattemtp = (Int32)test_apr1.Question_Attempted;
                            qanswer = (Int32)test_apr1.Total_Correct_Ans;
                        }
                        if (pacakage_detail.Count > 0)
                        {
                            retpackagedetail = pacakage_detail;
                        }
                        //---------------------------------

                        //  ViewBag.participants = ; //Total participants
                        int retexams = DbContext.tbl_DC_Exam_Result.Where(x => x.Is_Active == true && x.Is_Deleted == false && x.Regd_ID == id).ToList().Count(); // Total exams given

                        int retpkgmnth = DbContext.tbl_DC_Order.Where(x => x.Is_Active == true && x.Is_Deleted == false && x.Regd_ID == id && x.Is_Paid == true).ToList().Count(); //No of order 
                        string retcls = "";
                        if (data1 != null)
                        {
                            retcls = data1.Class_Name;
                        }
                        var returnobj = new Digichamps_web_Api.dashboardSuccessResult
                        {
                            success = new Digichamps_web_Api.student_dashboard
                            {
                                ord_customer = retord_customer,
                                testdate = date,
                                testtime = time,
                                remain_time = sec,
                                Raised = retRaised,
                                open = retopen,
                                Reject = retReject,
                                Closed = retClosed,
                                test = rettest,
                                Exam_Namee=ename,
                                Total_Question=qno,
                                Total_Attempted=qattemtp,
                                Total_Answered=qanswer,
                                packagedetail = retpackagedetail,
                                exams = retexams,                              
                                chaptersubscribe = result,
                                cls = retcls
                            }
                        };

                        return Request.CreateResponse(HttpStatusCode.OK, returnobj);
                    }
                    else
                    {
                        var errobj = new Digichamps_web_Api.Errorresult
                        {
                            Error = new Digichamps_web_Api.Errorresponse
                            {
                                Message = "Invalid user details.",
                            }
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, errobj);
                    }
                
            }
            catch{
            var errobj = new Digichamps_web_Api.Errorresult
                    {
                        Error = new Digichamps_web_Api.Errorresponse
                        {
                            Message = "Something went wrong",
                        }
                    };
            return Request.CreateResponse(HttpStatusCode.OK, errobj);
            }
        }

         //GET api/<controller>/5
        [HttpGet]
        public HttpResponseMessage userdrawer(int? id)
        {
            try
            {
                var authey = DbContext.tbl_DC_USER_SECURITY.Where(x => x.ROLE_CODE == "A").FirstOrDefault();
                AuthKey ob = new AuthKey();
                string apikey = ob.getsignedurl(authey.USER_NAME, authey.PASSWORD);

               
                    bool yesno;
                    var stuobj = DbContext.tbl_DC_Registration.Where(x => x.Regd_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    if (stuobj != null)
                    {
                        var offline = DbContext.VW_Offline_Student_Orders.Where(x => x.Regd_ID == id).FirstOrDefault();
                        if (offline != null)
                        {
                            var Isassigned = DbContext.SP_DC_Assign_Batch_Details().Where(x => x.Regd_ID == id).FirstOrDefault();
                            if (Isassigned != null)
                            {
                                yesno = true;
                            }
                            else
                            {
                                yesno = false;
                            }
                        }
                        else
                        {
                            yesno = false;
                        }
                        var returnobj = new Digichamps_web_Api.drawerSuccessResult
                        {
                            success = new Digichamps_web_Api.userdrawer
                            {
                                name = stuobj.Customer_Name,
                                Image =   stuobj.Image == "" ? "" : "/Images/Profile/" + stuobj.Image,
                                Isassigned = yesno
                            }
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, returnobj);
                    }
                    else
                    {
                        var errobj = new Digichamps_web_Api.Errorresult
                        {
                            Error = new Digichamps_web_Api.Errorresponse
                            {
                                Message = "Invalid user details.",
                            }
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, errobj);
                    }
            }
            catch {
                var errobj = new Digichamps_web_Api.Errorresult
                {
                    Error = new Digichamps_web_Api.Errorresponse
                    {
                        Message = "Something went wrong",
                    }
                };
                return Request.CreateResponse(HttpStatusCode.OK, errobj);
            }
        }
    }
}