using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using DigiChamps.Models;

namespace DigiChamps.Controllers
{
   //  [Authorize]
    public class ExamController : ApiController
    {
        DigiChampsEntities DbContext = new DigiChampsEntities();
        DateTime today = DigiChampsModel.datetoserver();


        public class Success_leader
        {
            public Leader_Board success { get; set; }
        }
        public class Leader_Board
        {
            public List< Myresult> My_Result { get; set; }
            public List< Leader_result> Top10 { get; set; }

        }
        public class Myresult
        {
              public   Nullable<int>   Max_rid          { get; set; }       
              public   Nullable<int>   Regd_ID          { get; set; }      
              public   Nullable<int>   Totaltime        { get; set; }     
              public   Nullable<int>  Question_Nos      { get; set; }   
              public   Nullable<int>  Total_Correct_Ans    { get; set; }
              public   Nullable<int>   Appear              { get; set; }      
              public  string          Customer_Name      { get; set; }   
              public  string           Image            { get; set; }
              public int Rank { get; set; }
              public int? Incorrect { get; set; }
        }
        public class Leader_result
        {
                public Nullable<int> Max_rid { get; set; }
                public Nullable<int> Regd_ID { get; set; }
                public Nullable<int> Totaltime { get; set; }
                public Nullable<int> Question_Nos { get; set; }
                public Nullable<int> Total_Correct_Ans { get; set; }
                public Nullable<int> Appear { get; set; }
                public string Customer_Name { get; set; }
                public string Image { get; set; }

                public int? Incorrect { get; set; }
        }

        public class Success_message_Blank
        {
            public Message_for_success success { get; set; }
        }
        public class Message_for_success
        {
            public string message { get; set; }
        }
        public class Result_Succes
        {
            public Result_Data success { get; set; }
        }
        public class Result_Data
   {
       public Nullable<int>    Question_Nos { get; set; }
       public Nullable<int>    Total_Correct_Ans { get; set; }
       public Nullable<double> Totaltime { get; set; }
       public Nullable<int>    wrong  { get; set; }
       public int              Accuracy { get; set; }
       public List<Difficulty> Difficultys  {get;set;}
       public List<Startegic_Report_Result> Topic_details { get; set; }

       public int? Exam_ID { get; set; }

       public int? Total_Incorrec_Ans { get; set; }

       public int? Question_Attempted { get; set; }

       public int? Unanswered { get; set; }
   }

        public  class Startegic_Report_Result
        {
            public Nullable<int> Topic_ID   { get; set; }
            public string    Topic_Name      { get; set; }
            public Nullable<int>  Total_question  { get; set; }
            public int    Correct_answer       { get; set; }
            public int       Incorrect_answer   { get; set; }
            public Nullable<int>  Percentage     { get; set; }
            public string      Remark     { get; set; }
        }

        public class Difficulty
      {
             public int  ?No_of_Question{get;set;}
             public int  ?Power_ID{get;set;}
             public string Power_Type { get; set; }
      }

        [HttpGet]
        public HttpResponseMessage Get_Exam(int? id, int? eid)//redg id
        {
            if (id != null)
            {
                try
                {
                    var chaptersubscribes = DbContext.SP_DC_Getchapter(id).Distinct().ToList();
                    if (chaptersubscribes.Count > 0)
                    {
                        var examdetails = DbContext.SP_ExamList(id, 5).ToList();
                        //var examdetails1 = DbContext.SP_ExamList(id, 1).ToList();
                        if (examdetails.Count > 0)
                        {
                            var obj = new DigiChamps.Models.Digichamps.SuccessResult_Exam1
                            {
                                success = new Digichamps.SuccessResponse_Exam1
                                {
                                    Online_Exam_Count = examdetails.Where(x => x.Chapter_Id == eid).ToList().Count(),

                                    Onine_Exam_List = (from c in examdetails.Where(x => x.Chapter_Id == eid)
                                                       select new DigiChamps.Models.Digichamps.Exam_List
                                                       {

                                                           Exam_ID = c.Exam_ID,
                                                           Exam_Name = c.Exam_Name,
                                                           Chapter_Id = c.Chapter_Id,
                                                           Board_Id = c.Board_Id,
                                                           Class_Id = c.Class_Id,
                                                           Is_Global = c.Is_Global,
                                                           Question_nos = c.Question_nos,
                                                           Subject = c.Subject,
                                                           Subject_Id = c.Subject_Id,
                                                           Time = c.Time,
                                                           Attempt_nos = c.Attempt_nos,
                                                           student_Attempt = c.student_Attempt,
                                                           Participants = c.Participants,
                                                           is_free = false,
                                                           Validity = 0
                                                       }).ToList(),
                                //    Pre_Requisite_test = (from a in examdetails1.Where(x => x.Chapter_Id == eid)
                                //                          select new DigiChamps.Models.Digichamps.Offline_exam_Pre_Requisite_test1
                                //                          {
                                //                              Exam_ID = a.Exam_ID,

                                //                              Attempt_nos = a.Attempt_nos,
                                //                              Exam_Name = a.Exam_Name,
                                //                              Chapter_Id = a.Chapter_Id,
                                //                              Time = a.Time,
                                //                              Exam_type = 1,
                                //                              Subject_Id = a.Subject_Id,
                                //                              Question_nos = a.Question_nos,
                                //                              student_attempt = a.student_Attempt,
                                //                              Participants = a.Participants
                                //                          }).ToList(),
                               }
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, obj);
                        }
                        else
                        {
                            var obj = new DigiChamps.Models.Digichamps.ErrorResult_Exam
                            {
                                error = new Digichamps.ErrorResponse_Exam
                                {
                                    Message = "No exams Found."
                                }
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, obj);
                        }
                    }
                    else
                    {
                        var free_test = DbContext.Sp_DC_Free_Exam(id).ToList();
                        //  var data_pre = DbContext.SP_DC_Offline_exams(id, 1).Where(x => x.Chapter_Id == eid).ToList();

                        if (free_test.Count > 0)
                        {
                            var obj = new DigiChamps.Models.Digichamps.Success_freeexam
                            {
                                success = new Digichamps.SuccessResponse_Free_Exam
                                {
                                    Onine_Exam_List = (from c in free_test.Where(x => x.Exam_type == 5)
                                                       select new DigiChamps.Models.Digichamps.Free_Exam_List
                                                       {
                                                           Exam_ID = c.Exam_ID,
                                                           Exam_Name = c.Exam_Name,
                                                           Subject_Id = c.Subject_Id,
                                                           Question_nos = c.Question_nos,
                                                           Attempt_nos = c.Attempt_nos,
                                                           Time = c.Time,
                                                           is_free = true,
                                                           Validity = c.Validity,
                                                           Chapter_Id = 0,
                                                           Board_Id = 0,
                                                           Class_Id = 0,
                                                           Is_Global = false,
                                                           Subject = "",
                                                           student_Attempt = c.stu_Attempt_nos,
                                                           Participants = c.Participants,


                                                       }).ToList(),
                                    //Pre_Requisite_test = (from a in free_test.Where(x => x.Exam_type == 1)
                                    //                      select new DigiChamps.Models.Digichamps.Offline_exam_Pre_Requisite_test1
                                    //                      {
                                    //                          Exam_ID = a.Exam_ID,

                                    //                          Attempt_nos = a.Attempt_nos,
                                    //                          Exam_Name = a.Exam_Name,
                                    //                          Chapter_Id = a.Chapter_Id,
                                    //                          Time = a.Time,
                                    //                          Exam_type = a.Exam_type,
                                    //                          Subject_Id = a.Subject_Id,
                                    //                          Question_nos = a.Question_nos,
                                    //                          student_attempt = a.stu_Attempt_nos,
                                    //                          Participants = a.Participants
                                    //                      }).ToList(),

                                }
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, obj);
                        }
                        else
                        {
                            var obj = new DigiChamps.Models.Digichamps.ErrorResult_Exam
                            {
                                error = new Digichamps.ErrorResponse_Exam
                                {
                                    Message = "No exams Found."
                                }
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, obj);
                        }
                    }
                }
                catch
                {
                    var obj = new DigiChamps.Models.Digichamps.ErrorResult_Exam
                    {
                        error = new Digichamps.ErrorResponse_Exam
                        {
                            Message = "Something went wrong."
                        }
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, obj);
                }
            }
            else
            {
                var obj = new DigiChamps.Models.Digichamps.ErrorResult_Exam
                {
                    error = new Digichamps.ErrorResponse_Exam
                    {
                        Message = "Please provide data correctly."
                    }
                };
                return Request.CreateResponse(HttpStatusCode.OK, obj);
            }
        }
        [HttpGet]
        public HttpResponseMessage Get_PRT(int? id, int? eid)//redg id
        {
            if (id != null)
            {
                try
                {
                    var chaptersubscribes = DbContext.SP_DC_Getchapter(id).Distinct().ToList();
                    if (chaptersubscribes.Count > 0)
                    {
                       // var examdetails = DbContext.SP_ExamList(id, 5).ToList();
                        var examdetails1 = DbContext.SP_ExamList(id, 1).ToList();
                        if (examdetails1.Count > 0)
                        {
                            var obj = new DigiChamps.Models.Digichamps.SuccessResult_Exam2
                            {
                                success = new Digichamps.SuccessResponse_Exam2
                                {
                                    //Online_Exam_Count = examdetails.Where(x => x.Chapter_Id == eid).ToList().Count(),

                                    //Onine_Exam_List = (from c in examdetails.Where(x => x.Chapter_Id == eid)
                                    //                   select new DigiChamps.Models.Digichamps.Exam_List
                                    //                   {

                                    //                       Exam_ID = c.Exam_ID,
                                    //                       Exam_Name = c.Exam_Name,
                                    //                       Chapter_Id = c.Chapter_Id,
                                    //                       Board_Id = c.Board_Id,
                                    //                       Class_Id = c.Class_Id,
                                    //                       Is_Global = c.Is_Global,
                                    //                       Question_nos = c.Question_nos,
                                    //                       Subject = c.Subject,
                                    //                       Subject_Id = c.Subject_Id,
                                    //                       Time = c.Time,
                                    //                       Attempt_nos = c.Attempt_nos,
                                    //                       student_Attempt = c.student_Attempt,
                                    //                       Participants = c.Participants,
                                    //                       is_free = false,
                                    //                       Validity = 0
                                    //                   }).ToList(),
                                    Pre_Requisite_test = (from a in examdetails1.Where(x => x.Chapter_Id == eid)
                                                          select new DigiChamps.Models.Digichamps.Offline_exam_Pre_Requisite_test1
                                                          {
                                                              Exam_ID = a.Exam_ID,

                                                              Attempt_nos = a.Attempt_nos,
                                                              Exam_Name = a.Exam_Name,
                                                              Chapter_Id = a.Chapter_Id,
                                                              Time = a.Time,
                                                              Exam_type = 1,
                                                              Subject_Id = a.Subject_Id,
                                                              Question_nos = a.Question_nos,
                                                              student_attempt = a.student_Attempt,
                                                              Participants = a.Participants
                                                          }).ToList(),
                                }
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, obj);
                        }
                        else
                        {
                            var obj = new DigiChamps.Models.Digichamps.ErrorResult_Exam
                            {
                                error = new Digichamps.ErrorResponse_Exam
                                {
                                    Message = "No exams Found."
                                }
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, obj);
                        }
                    }
                    else
                    {
                        var free_test = DbContext.Sp_DC_Free_Exam(id).ToList();
                        //  var data_pre = DbContext.SP_DC_Offline_exams(id, 1).Where(x => x.Chapter_Id == eid).ToList();

                        if (free_test.Count > 0)
                        {
                            var obj = new DigiChamps.Models.Digichamps.Success_freeexam1
                            {
                                success = new Digichamps.SuccessResponse_Free_Exam1
                                {
                                  
                                    Pre_Requisite_test = (from a in free_test.Where(x => x.Exam_type == 1)
                                                          select new DigiChamps.Models.Digichamps.Offline_exam_Pre_Requisite_test1
                                                          {
                                                              Exam_ID = a.Exam_ID,

                                                              Attempt_nos = a.Attempt_nos,
                                                              Exam_Name = a.Exam_Name,
                                                              Chapter_Id = a.Chapter_Id,
                                                              Time = a.Time,
                                                              Exam_type = a.Exam_type,
                                                              Subject_Id = a.Subject_Id,
                                                              Question_nos = a.Question_nos,
                                                              student_attempt = a.stu_Attempt_nos,
                                                              Participants = a.Participants
                                                          }).ToList(),

                                }
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, obj);
                        }
                        else
                        {
                            var obj = new DigiChamps.Models.Digichamps.ErrorResult_Exam
                            {
                                error = new Digichamps.ErrorResponse_Exam
                                {
                                    Message = "No exams Found."
                                }
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, obj);
                        }
                    }
                }
                catch
                {
                    var obj = new DigiChamps.Models.Digichamps.ErrorResult_Exam
                    {
                        error = new Digichamps.ErrorResponse_Exam
                        {
                            Message = "Something went wrong."
                        }
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, obj);
                }
            }
            else
            {
                var obj = new DigiChamps.Models.Digichamps.ErrorResult_Exam
                {
                    error = new Digichamps.ErrorResponse_Exam
                    {
                        Message = "Please provide data correctly."
                    }
                };
                return Request.CreateResponse(HttpStatusCode.OK, obj);
            }
        }
        [HttpGet]
        public HttpResponseMessage Get_Exam_Offline(int? id, int? eid)//redg id
        {
            if (id != null)
            {
                try
                {
                    var chaptersubscribes = DbContext.SP_DC_Getchapter(id).Distinct().ToList();
                    if (chaptersubscribes.Count > 0)
                    {

                        var data_pre = DbContext.SP_DC_Offline_exams(id, 1).Where(x => x.Chapter_Id == eid).ToList();

                        var data_prac = DbContext.SP_DC_Offline_exams(id, 3).Where(x => x.Chapter_Id == eid).ToList();

                        var data_Retest = DbContext.SP_DC_Offline_exams(id, 2).Where(x => x.Chapter_Id == eid).ToList();

                        var data_Online = DbContext.SP_DC_Offline_exams(id, 5).Where(x => x.Chapter_Id == eid).ToList();

                        var data_Sche = DbContext.SP_DC_Offline_exams(id, 4).Where(x => x.Chapter_Id == eid).ToList();

                        var examdetails = DbContext.SP_ExamList(id, 5).ToList();

                        var examdetails1 = examdetails.Where(x => x.Chapter_Id == eid).ToList();
                        var data_pre1 = data_pre.Where(x => x.Chapter_Id == eid).ToList();
                        var data_prac1 = data_prac.Where(x => x.Chapter_Id == eid).ToList();
                        var data_Retest1 = data_Retest.Where(x => x.Chapter_Id == eid).ToList();
                        var data_Sche1 = data_Sche.Where(x => x.Chapter_Id == eid).ToList();
                        var data_Online1 = data_Online.Where(x => x.Chapter_Id == eid).ToList();
                        string status = string.Empty;
                        var data = (from c in DbContext.tbl_DC_Package_Dtl.Where(x => x.Chapter_Id == eid && x.Is_Active == true && x.Is_Deleted == false)
                                    join d in DbContext.tbl_DC_Package.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                    on c.Package_ID equals d.Package_ID

                                    select new DigiChamps.Models.Digichamps.Is_Offline_Result
                                    {
                                        Is_offline = d.Is_Offline
                                    }).FirstOrDefault();
                        if (data != null)
                        {
                            status = Convert.ToString(data.Is_offline);
                        }
                        if (examdetails1.Count > 0 || data_pre1.Count > 0 || data_Retest1.Count > 0 || data_prac1.Count > 0 || data_Sche1.Count > 0 || data_Online1.Count > 0)
                        {
                            var obj = new DigiChamps.Models.Digichamps.SuccessResult_Exam
                            {
                                success = new Digichamps.SuccessResponse_Exam
                                {                                 
                                    Is_Offline = status,                               
                                    offline_Exam = new Digichamps.Offline_success
                                    {
                                        Total_Requisite_test = data_pre.Count,
                                        Total_Practice_Test = data_prac.Count,
                                        Total_Retest = data_Retest.Count,

                                        Pre_Requisite_test = (from a in data_pre
                                                              select new DigiChamps.Models.Digichamps.Offline_exam_Pre_Requisite_test
                                                              {
                                                                  Exam_ID = a.Exam_ID,

                                                                  Attempt_nos = a.Attempt_nos,
                                                                  Exam_Name = a.Exam_Name,
                                                                  Chapter_Id = a.Chapter_Id,
                                                                  Chapter = a.Chapter,
                                                                  Time = a.Time,
                                                                  Exam_type = a.Exam_type,
                                                                  Subject_Id = a.Subject_Id,
                                                                  Subject = a.Subject,
                                                                  Question_nos = a.Question_nos,
                                                                  max_attempt = a.max_attempt,
                                                                  Participants = a.Participants
                                                              }).ToList(),
                                        Practice = (from b in data_prac
                                                    select new DigiChamps.Models.Digichamps.Offline_exam_Practice
                                                    {
                                                        Exam_ID = b.Exam_ID,

                                                        Attempt_nos = b.Attempt_nos,
                                                        Exam_Name = b.Exam_Name,
                                                        Chapter_Id = b.Chapter_Id,
                                                        Chapter = b.Chapter,
                                                        Time = b.Time,
                                                        Exam_type = b.Exam_type,
                                                        Subject_Id = b.Subject_Id,
                                                        Subject = b.Subject,
                                                        Question_nos = b.Question_nos,
                                                        max_attempt = b.max_attempt,
                                                        Participants = b.Participants
                                                    }).ToList(),
                                        Re_Test = (from c in data_Retest
                                                   select new DigiChamps.Models.Digichamps.Offline_exam_Re_Test
                                                   {
                                                       Exam_ID = c.Exam_ID,

                                                       Attempt_nos = c.Attempt_nos,
                                                       Exam_Name = c.Exam_Name,
                                                       Chapter_Id = c.Chapter_Id,
                                                       Chapter = c.Chapter,
                                                       Time = c.Time,
                                                       Exam_type = c.Exam_type,
                                                       Subject_Id = c.Subject_Id,
                                                       Subject = c.Subject,
                                                       Question_nos = c.Question_nos,
                                                       max_attempt = c.max_attempt,
                                                       Participants = c.Participants
                                                   }).ToList()
                                    }
                                }
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, obj);

                        }
                        else
                        {
                            var obj = new DigiChamps.Models.Digichamps.ErrorResult_Exam
                            {
                                error = new Digichamps.ErrorResponse_Exam
                                {
                                    Message = "No exams Found."
                                }
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, obj);
                        }
                    }
                    else
                    {
                        var obj = new DigiChamps.Models.Digichamps.ErrorResult_Exam
                        {
                            error = new Digichamps.ErrorResponse_Exam
                            {
                                Message = "No exams Found."
                            }
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, obj);
                    }
                    //else
                    //{
                    //    var free_test = DbContext.Sp_DC_Free_Exam(id).ToList();
                    //    if (free_test.Count > 0)
                    //    {
                    //        var obj = new DigiChamps.Models.Digichamps.Success_freeexam
                    //        {
                    //            success = new Digichamps.SuccessResponse_Free_Exam
                    //            {
                    //                Onine_Exam_List = (from c in free_test
                    //                             select new DigiChamps.Models.Digichamps.Free_Exam_List
                    //                             {
                    //                                 Exam_ID = c.Exam_ID,
                    //                                 Exam_Name = c.Exam_Name,
                    //                                 Subject_Id = c.Subject_Id,
                    //                                 Question_nos = c.Question_nos,
                    //                                 Attempt_nos = c.Attempt_nos,
                    //                                 Time = c.Time,
                    //                                 Validity = c.Validity,
                    //                                 stu_Attempt_nos = c.Attempt_nos,
                    //                             }).ToList()

                    //            }
                    //        };
                    //        return Request.CreateResponse(HttpStatusCode.OK, obj);
                    //    }
                    //    else
                    //    {
                    //        var obj = new DigiChamps.Models.Digichamps.ErrorResult_Exam
                    //        {
                    //            error = new Digichamps.ErrorResponse_Exam
                    //            {
                    //                Message = "No exams Found."
                    //            }
                    //        };
                    //        return Request.CreateResponse(HttpStatusCode.OK, obj);
                    //    }
                    //}
                }
                catch
                {
                    var obj = new DigiChamps.Models.Digichamps.ErrorResult_Exam
                    {
                        error = new Digichamps.ErrorResponse_Exam
                        {
                            Message = "Something went wrong."
                        }
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, obj);
                }
            }
            else
            {
                var obj = new DigiChamps.Models.Digichamps.ErrorResult_Exam
                {
                    error = new Digichamps.ErrorResponse_Exam
                    {
                        Message = "Please provide data correctly."
                    }
                };
                return Request.CreateResponse(HttpStatusCode.OK, obj);
            }
        }
        [HttpGet]
        public HttpResponseMessage Get_Questions(int? id, int? eid)
      {
          int exam_typ = 0;
          if (id != null && eid != null)
          {
              try
              {


                  var exam = DbContext.tbl_DC_Exam.Where(x => x.Exam_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                  var count = DbContext.tbl_DC_Exam_Result.Where(x => x.Regd_ID == eid && x.Exam_ID == id).ToList().Count();
                    if (exam != null)
                    {
                        if (Convert.ToInt32(count) == Convert.ToInt32(exam.Attempt_nos))
                        {
                            var obj = new DigiChamps.Models.Digichamps.ErrorResult_Exam
                            {
                                error = new Digichamps.ErrorResponse_Exam
                                {
                                    Message = "Your no of attemp has been exceeded."
                                }
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, obj);
                        }

                        tbl_DC_Exam_Result result = new tbl_DC_Exam_Result();
                        result.Exam_ID = id;
                        result.StartTime = today;
                        result.Exam_Name = exam.Exam_Name;
                        DbContext.tbl_DC_Exam_Result.Add(result);
                        DbContext.SaveChanges();


                        exam_typ = Convert.ToInt32(exam.Exam_type);
                        var getdata = DbContext.SP_DC_Procedurefor_test(id, eid, exam_typ).ToList();
                        if (getdata.Count > 0)
                        {
                            var obj = new DigiChamps.Models.Digichamps.successexamstart
                            {
                                success = new Digichamps.examstart
                                {
                                    Start_Time = today,
                                    End_Time = today.AddMinutes(Convert.ToInt32(exam.Time)),
                                    Result_ID = result.Result_ID,
                                    ExamData = (from c in DbContext.SP_DC_Procedurefor_test(id, eid, exam_typ)
                                                select new DigiChamps.Models.Digichamps.examstart_data
                                                {

                                                    RowID = c.RowID,
                                                    question_id = c.question_id,
                                                    Board_Id = c.Board_Id,
                                                    Class_Id = c.Class_Id,
                                                    Subject_Id = c.Subject_Id,
                                                    ch_id = c.ch_id,
                                                    topicId = c.topicId,
                                                    power_id = c.power_id,
                                                    question = c.question,
                                                    Qustion_Desc = c.Qustion_Desc,

                                                    Image = (from i in DbContext.tbl_DC_Question_Images.Where(x => x.Is_Active == true && x.Is_Deleted == false && x.Question_ID == (Int32)c.question_id)
                                                             select new DigiChamps.Models.Digichamps.Question_image
                                                             {
                                                                 Question_desc_Image = i.Question_desc_Image == null ? "" : "/Content/Qusetion/" + i.Question_desc_Image

                                                             }).ToList(),
                                                    Options = (from o in DbContext.tbl_DC_Question_Answer.Where(x => x.Is_Active == true && x.Is_Deleted == false && x.Question_ID == (Int32)c.question_id)
                                                               select new DigiChamps.Models.Digichamps.Question_options
                                                               {
                                                                   Answer_ID = o.Answer_ID,
                                                                   Question_ID = o.Question_ID,
                                                                   Option_Desc = o.Option_Desc,
                                                                   Option_Image = o.Option_Image == null ? "" : "/Content/Qusetion/" + o.Option_Image,
                                                                   Answer_desc = o.Answer_desc,
                                                                   Answer_Image = o.Answer_Image == null ? "" : "/Content/Qusetion/" + o.Answer_Image,
                                                                   Is_Answer = o.Is_Answer
                                                               }).ToList()
                                                }).ToList()
                                }
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, obj);
                        }
                        else
                        {
                            var obj = new DigiChamps.Models.Digichamps.ErrorResult_Exam
                            {
                                error = new Digichamps.ErrorResponse_Exam
                                {
                                    Message = "No questions left."
                                }
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, obj);
                        }



                    }
                    else
                    {
                        var obj = new DigiChamps.Models.Digichamps.ErrorResult_Exam
                        {
                            error = new Digichamps.ErrorResponse_Exam
                            {
                                Message = "Wrong data given."
                            }
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, obj);
                    }
              }
                   
              catch(Exception ex)
              {
                  var obj = new DigiChamps.Models.Digichamps.ErrorResult_Exam
                  {
                      error = new Digichamps.ErrorResponse_Exam
                      {
                          Message = "Something went wrong."
                      }
                  };
                  return Request.CreateResponse(HttpStatusCode.OK, obj);
              }
          }
          else
          {
              var obj = new DigiChamps.Models.Digichamps.ErrorResult_Exam
              {
                  error = new Digichamps.ErrorResponse_Exam
                  {
                      Message = "Please provide data correctly."
                  }
              };
              return Request.CreateResponse(HttpStatusCode.OK, obj);
          }

      }

        [HttpGet]
        public HttpResponseMessage Leader_Boards(int?id,int ?eid)
        {
            try
            {
                var Leader = DbContext.SP_DC_lead(eid).ToList();
                if (Leader.Count>0)
                {

                if (Leader.Take(10).Where(x => x.Regd_ID == id).FirstOrDefault() == null)
                {
                    int count = Leader.FindIndex(X => X.Regd_ID == id);
                    if (count != -1)
                    {
                        int myrank = count + 1;
                        //Mylead = Leader.Where(x => x.Regd_ID == id).ToList();

                        var obj = new Success_leader
                        {
                            success = new Leader_Board
                            {
                                Top10 = (from c in Leader.Take(10)
                                         select new Leader_result
                                         {
                                             
                                             Max_rid = c.Max_rid,
                                             Regd_ID = c.Regd_ID,
                                             Totaltime = c.Totaltime,
                                             Question_Nos = c.Question_Nos,
                                             Total_Correct_Ans = c.Total_Correct_Ans,
                                             Incorrect = (Convert.ToInt32(c.Question_Nos) - Convert.ToInt32(c.Total_Correct_Ans)),
                                             Appear = c.Appear,
                                             Customer_Name = c.Customer_Name,
                                             Image =  c.Image == null ? "" : "/Images/Profile/"+ c.Image

                                         }).ToList(),

                                My_Result = (from c in Leader.Where(x => x.Regd_ID == id)
                                             select new Myresult
                                             {
                                                 Rank=myrank,
                                                 Max_rid = c.Max_rid,
                                                 Regd_ID = c.Regd_ID,
                                                 Totaltime = c.Totaltime,
                                                 Question_Nos = c.Question_Nos,
                                                 Total_Correct_Ans = c.Total_Correct_Ans,
                                                 Incorrect = (Convert.ToInt32(c.Question_Nos) - Convert.ToInt32(c.Total_Correct_Ans)),
                                                 Appear = c.Appear,
                                                 Customer_Name = c.Customer_Name,
                                                 Image = c.Image == null ? "" : "/Images/Profile/" + c.Image

                                             }).ToList()
                            }
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, obj);
                    }
                    else
                    {
                            Myresult ob = new Myresult();
                        var obj = new Success_leader
                        {
                            success = new Leader_Board
                            {
                                Top10 = (from c in Leader
                                        select new Leader_result
                                        {
                                              Max_rid=c.Max_rid,         
                                              Regd_ID =c.Regd_ID   ,
                                              Totaltime=c.Totaltime    ,
                                             Question_Nos=c.Question_Nos,
                                             Total_Correct_Ans=c.Total_Correct_Ans,
                                              Incorrect = (Convert.ToInt32(c.Question_Nos) - Convert.ToInt32(c.Total_Correct_Ans)),
                                              Appear=c.Appear,   
                                             Customer_Name =c.Customer_Name,
                                              Image = c.Image == null ? "" : "/Images/Profile/" + c.Image

                                        }).ToList(),
                                My_Result=(from c in Leader.Take(0)
                                          select new Myresult {
                                    Max_rid = ob.Max_rid,
                                    Regd_ID = ob.Regd_ID,
                                    Totaltime =  ob.Totaltime,
                                    Question_Nos = ob.Question_Nos,
                                    Total_Correct_Ans = ob.Total_Correct_Ans,
                                    Incorrect = ob.Incorrect,
                                    Appear = ob.Appear,
                                    Customer_Name = ob.Customer_Name,
                                    Image = ob.Image
                                } ).ToList()  

                            }
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, obj);
                    }
                }
                else
                    {
                        Myresult ob = new Myresult();
                        var obj = new Success_leader
                    {
                        success = new Leader_Board
                        {
                            Top10 = (from c in Leader.Take(10)
                                     select new Leader_result
                                     {
                                         Max_rid = c.Max_rid,
                                         Regd_ID = c.Regd_ID,
                                         Totaltime = c.Totaltime,
                                         Question_Nos = c.Question_Nos,
                                         Total_Correct_Ans = c.Total_Correct_Ans,
                                         Appear = c.Appear,
                                         Incorrect = (Convert.ToInt32(c.Question_Nos) - Convert.ToInt32(c.Total_Correct_Ans)),
                                         Customer_Name = c.Customer_Name,
                                         Image = c.Image == null ? "" : "/Images/Profile/" + c.Image

                                     }).ToList(),
                            My_Result = (from c in Leader.Take(0)
                                         select new Myresult
                                         {
                                             Max_rid = ob.Max_rid,
                                             Regd_ID = ob.Regd_ID,
                                             Totaltime = ob.Totaltime,
                                             Question_Nos = ob.Question_Nos,
                                             Total_Correct_Ans = ob.Total_Correct_Ans,
                                             Incorrect = ob.Incorrect,
                                             Appear = ob.Appear,
                                             Customer_Name = ob.Customer_Name,
                                             Image = ob.Image
                                         }).ToList()

                        }
                    };
                   
                    return Request.CreateResponse(HttpStatusCode.OK, obj);
                }


                }
                else
                {
                    var obj = new DigiChamps.Models.Digichamps.ErrorResult_Exam
                    {
                        error = new Digichamps.ErrorResponse_Exam
                        {
                            Message = "No one given exam."
                        }
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, obj);
                }


            }
            catch
            {
                var obj = new DigiChamps.Models.Digichamps.ErrorResult_Exam
                {
                    error = new Digichamps.ErrorResponse_Exam
                    {
                        Message = "No exams Found."
                    }
                };
                return Request.CreateResponse(HttpStatusCode.OK, obj);
            }
        }

        [HttpGet]
        public HttpResponseMessage Test_Result(int? id, int? eid)//registration Id/ Result Id
        {
            try
            {
              
               var startegic = DbContext.SP_DC_Startegic_Report(eid).ToList();

               var examresult_cal = DbContext.SP_DC_Examresultcalulation(id, eid).ToList();

               var question = DbContext.SP_DC_Getallquestion_Appeard(eid);

               var get_Level = DbContext.SP_DC_Get_Power_Result(eid).ToList();

               var get_result_data = DbContext.tbl_DC_Exam_Result.Where(x => x.Result_ID == eid).FirstOrDefault();

               DateTime sdt = Convert.ToDateTime(get_result_data.StartTime);

               DateTime edt = Convert.ToDateTime(get_result_data.EndTime);

               var hours = (edt - sdt).TotalMilliseconds;

             int  accuracy =Convert.ToInt32( Convert.ToDecimal( examresult_cal.FirstOrDefault().Total_Correct_Ans )/Convert.ToDecimal(  examresult_cal.FirstOrDefault().Question_Nos)*100);

             var obj = new Result_Succes
               {
                   success = new Result_Data
                   {
                       Question_Nos = examresult_cal.FirstOrDefault().Question_Nos,
                       Total_Correct_Ans = examresult_cal.FirstOrDefault().Total_Correct_Ans,
                       Total_Incorrec_Ans = examresult_cal.FirstOrDefault().Total_InCorrect_Ans,
                       Question_Attempted = examresult_cal.FirstOrDefault().Question_Attempted,
                       Unanswered = examresult_cal.FirstOrDefault().Question_Nos - examresult_cal.FirstOrDefault().Question_Attempted,
                       Totaltime = hours,
                       Exam_ID=get_result_data.Exam_ID,
                       wrong =Convert.ToInt32( examresult_cal.FirstOrDefault().wrong),
                       Accuracy = accuracy,
                       Difficultys = (from c in get_Level
                                      select new Difficulty
                                      {
                                         No_of_Question=c.No_of_Question,
                                         Power_ID=c.Power_ID,
                                         Power_Type=c.Power_Type
                                      }).ToList(),
                        Topic_details = (from d in startegic select new Startegic_Report_Result { 
                                      Topic_ID =d.Topic_ID,      
                                      Topic_Name    =d.Topic_Name,  
                                      Total_question =d.Total_question, 
                                      Correct_answer  =d.Correct_answer,
                                      Incorrect_answer=d.Incorrect_answer,
                                      Percentage    =d.Percentage,  
                                      Remark =d.Remark       
                       }).ToList()
                   }
               };
               return Request.CreateResponse(HttpStatusCode.OK, obj);
            }
            catch
            {
                var obj = new DigiChamps.Models.Digichamps.ErrorResult_Exam
                {
                    error = new Digichamps.ErrorResponse_Exam
                    {
                        Message = "No data Found."
                    }
                };
                return Request.CreateResponse(HttpStatusCode.OK, obj);
            }

        }

        public class RootObject2
        {
            public Get_all_question Get_question { get; set; }
        }
        public class Get_all_question
        {
            public int? Result_id { get; set; }
            public int? Student_id { get; set; }
            public int? Exam_id { get; set; }
            public DateTime? Start_time { get; set; }

            public List<Questions> Question { get; set; }
        }
        public class Questions
        {
            public int Question_id { get; set; }
            public List<Answer> Answers { get; set; }
        }
        public class Answer
        {
            public int Answer_id { get; set; }
        }
        public class Result_success
        {
            public success_Result success { get; set; }
        }
        public class success_Result
        {
            public string Message { get; set; }
            public int ?Result_id { get; set; }
        }
        [HttpPost]
        public HttpResponseMessage Submit_Test([FromBody] RootObject2 root)
        {
            try
            {

                if (root == null)
                {
                    var obj = new DigiChamps.Models.Digichamps.ErrorResult_Exam
                    {
                        error = new Digichamps.ErrorResponse_Exam
                        {
                            Message = "No data Found."
                        }
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, obj);
                }
                else
                {
                    List<int> Question_attempted_list = new List<int>();
                    List<int> Question_answer_real = new List<int>();
                    List<int> Question_answer_given = new List<int>();
                    bool equals = false;
                    int not_atm_qsn = 0;
                    int total_correct = 0;
                    int Qsn_atemp = 0;
                    var Get_result = DbContext.tbl_DC_Exam_Result.Where(x => x.Result_ID == root.Get_question.Result_id).FirstOrDefault();
                    var Get_student_data = DbContext.tbl_DC_Registration.Where(x => x.Regd_ID == root.Get_question.Student_id).FirstOrDefault();
                    var Get_exam = DbContext.tbl_DC_Exam.Where(x => x.Exam_ID == root.Get_question.Exam_id).FirstOrDefault();
                    DateTime End_time = today;
                    var Question_lists = root.Get_question.Question.ToList();

                    if (Get_result != null && Get_student_data != null && Get_exam != null)
                    {
                        foreach (var v in Question_lists)
                        {
                            int Question_id = Convert.ToInt32(v.Question_id);
                            var Get_Question_Dtl = DbContext.tbl_DC_Question.Where(x => x.Question_ID == Question_id).FirstOrDefault();
                            var Answers = DbContext.tbl_DC_Question_Answer.Where(x => x.Question_ID == Question_id && x.Is_Answer == true && x.Is_Active == true && x.Is_Deleted == false).ToList();
                            foreach (var ans in Answers)
                            {
                                Question_answer_real.Add(ans.Answer_ID);

                            }
                            if (v.Answers.ToList().Count > 0)  //if answer  given or attempted
                            {
                                Qsn_atemp = Qsn_atemp + 1;

                                foreach (var qa in v.Answers)
                                {

                                    Question_answer_given.Add(qa.Answer_id);
                                    //check for multiple answer is correct or not
                                    var set = new HashSet<int>(Question_answer_given);
                                    equals = set.SetEquals(Question_answer_real);
                                }

                                if (equals == true)
                                {
                                    //Answer is correct
                                    total_correct = total_correct + 1;
                                    tbl_DC_Exam_Result_Dtl _result_dtl = new tbl_DC_Exam_Result_Dtl();
                                    _result_dtl.Result_ID = root.Get_question.Result_id;
                                    _result_dtl.Question_ID = Question_id;
                                    _result_dtl.Is_Active = true;
                                    _result_dtl.Is_Deleted = false;
                                    _result_dtl.Is_Correct = true;
                                    _result_dtl.Board_Id = Get_Question_Dtl.Board_Id;
                                    _result_dtl.Class_Id = Get_Question_Dtl.Class_Id;
                                    _result_dtl.Chapter_Id = Get_Question_Dtl.Chapter_Id;
                                    _result_dtl.Topic_ID = Get_Question_Dtl.Topic_ID;
                                    _result_dtl.Question = Get_Question_Dtl.Question;
                                    DbContext.tbl_DC_Exam_Result_Dtl.Add(_result_dtl);
                                    DbContext.SaveChanges();
                                    Question_answer_real.Clear();
                                    Question_answer_given.Clear();
                                }
                                else
                                {
                                    //Answer is incorrect

                                    tbl_DC_Exam_Result_Dtl _result_dtl = new tbl_DC_Exam_Result_Dtl();
                                    _result_dtl.Result_ID = root.Get_question.Result_id;
                                    _result_dtl.Question_ID = Question_id;
                                    _result_dtl.Is_Active = true;
                                    _result_dtl.Is_Deleted = false;
                                    _result_dtl.Is_Correct = false;
                                    _result_dtl.Board_Id = Get_Question_Dtl.Board_Id;
                                    _result_dtl.Class_Id = Get_Question_Dtl.Class_Id;
                                    _result_dtl.Chapter_Id = Get_Question_Dtl.Chapter_Id;
                                    _result_dtl.Topic_ID = Get_Question_Dtl.Topic_ID;
                                    _result_dtl.Question = Get_Question_Dtl.Question;
                                    DbContext.tbl_DC_Exam_Result_Dtl.Add(_result_dtl);
                                    DbContext.SaveChanges();
                                    Question_answer_real.Clear();
                                    Question_answer_given.Clear();
                                }
                            }
                            else //if answer not  given
                            {
                                not_atm_qsn = not_atm_qsn + 1;
                                //if Question not attempted
                                //insert into exam Result_dtl
                                tbl_DC_Exam_Result_Dtl _result_dtl = new tbl_DC_Exam_Result_Dtl();
                                _result_dtl.Result_ID = root.Get_question.Result_id;
                                _result_dtl.Question_ID = Question_id;
                                _result_dtl.Is_Active = true;
                                _result_dtl.Is_Deleted = false;
                                _result_dtl.Is_Correct = false;
                                _result_dtl.Board_Id = Get_Question_Dtl.Board_Id;
                                _result_dtl.Class_Id = Get_Question_Dtl.Class_Id;
                                _result_dtl.Chapter_Id = Get_Question_Dtl.Chapter_Id;
                                _result_dtl.Topic_ID = Get_Question_Dtl.Topic_ID;
                                _result_dtl.Question = Get_Question_Dtl.Question;
                                DbContext.tbl_DC_Exam_Result_Dtl.Add(_result_dtl);
                                DbContext.SaveChanges();
                                Question_answer_real.Clear();
                                Question_answer_given.Clear();
                            }

                        }

                        Get_result.Board_Id = Get_exam.Board_Id;
                        Get_result.Chapter_Id = Get_exam.Chapter_Id;
                        Get_result.Class_Id = Get_exam.Class_Id;
                        Get_result.EndTime = today;
                        Get_result.Exam_ID = Get_exam.Exam_ID;
                        Get_result.Question_Nos = Get_exam.Question_nos;
                        Get_result.Question_Attempted = Qsn_atemp;
                        Get_result.Total_Correct_Ans = total_correct;
                        Get_result.Is_Active = true;
                        Get_result.Is_Deleted = false;
                        Get_result.Regd_ID = Get_student_data.Regd_ID;
                        Get_result.Regd_No = Get_student_data.Regd_No;
                        DbContext.SaveChanges();
                        try
                        {
                            var pushnot = (from c in DbContext.tbl_DC_Registration.Where(x => x.Regd_ID == Get_student_data.Regd_ID)

                                           select new { c.Regd_ID, c.Device_id }).FirstOrDefault();
                            string body = "Resultid#{{resid}}#Examid#{{examid}}#  Exam Appeared : Hello {{name}},! You have attempted {{correctno}} Correct Answers in {{test}} exam. View your test details and leader board now.";
                            string msg = body.ToString().Replace("{{name}}", Get_student_data.Customer_Name).Replace("{{correctno}}", total_correct.ToString()).Replace("{{test}}", Get_exam.Exam_Name).Replace("{{resid}}", root.Get_question.Result_id.ToString()).Replace("{{examid}}", root.Get_question.Exam_id.ToString());

                            if (pushnot != null)
                            {
                                if (pushnot.Device_id != null)
                                {
                                    var note = new PushNotiStatus(msg, pushnot.Device_id);
                                }
                            }
                        }
                        catch (Exception)
                        {

                        }
                        var obj = new Result_success
                        {
                            success = new success_Result
                            {
                                Message = "Test submitted sucessfully.",
                                Result_id = root.Get_question.Result_id

                            }
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, obj);
                    }
                    else
                    {
                        var obj = new DigiChamps.Models.Digichamps.ErrorResult_Exam
                        {
                            error = new Digichamps.ErrorResponse_Exam
                            {
                                Message = "No data Found."
                            }
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, obj);
                    }
                }



            }
            catch
            {
                var obj = new DigiChamps.Models.Digichamps.ErrorResult_Exam
                {
                    error = new Digichamps.ErrorResponse_Exam
                    {
                        Message = "No data Found."
                    }
                };
                return Request.CreateResponse(HttpStatusCode.OK, obj);
            }
        }
        //[HttpGet]
        //public HttpResponseMessage Get_quest()
        //{
        //    var obj = new RootObject2
        //    {
        //        Get_question = new Get_all_question
        //        {
        //            Exam_id = 19,
        //            Student_id = 3,
        //            Result_id = 2010,
        //            Start_time = today,
        //            Question = (from c in DbContext.tbl_DC_Question.Where(x => x.Chapter_Id == 1).Take(9).ToList()
        //                        select new Questions
        //                            {
        //                                Question_id = c.Question_ID,
        //                                Answers = (from d in DbContext.tbl_DC_Question_Answer.Where(x => x.Question_ID == c.Question_ID && x.Is_Answer == true).ToList()
        //                                           select new Answer
        //                                               {
        //                                                   Answer_id=d.Answer_ID
        //                                               }).ToList()
        //                            }).ToList()

        //        }
        //    };
        //    return Request.CreateResponse(HttpStatusCode.OK,obj);
        //}     
    }
}
