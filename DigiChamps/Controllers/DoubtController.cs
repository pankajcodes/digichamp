using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using DigiChamps.Models;
using System.Threading.Tasks;
using System.Web;
using System.Net.Mail;

namespace DigiChamps.Controllers
{
   // [Authorize]
    public class DoubtController : ApiController
    {
        DigiChampsEntities DbContext = new DigiChampsEntities();
        DateTime today = DigiChampsModel.datetoserver();
        /// <Doubt 1st get>
      public class Success_Doubt
        {
            public success success { get; set; }
        }

        public class success
        {
           
            public List< Boards> Board { get; set; }
            public List<Class> Class { get; set; }
        }

        public class Boards
        {
            public string Board_Name { get; set; }
            public int ?Board_ID { get; set; }
            public bool Seleceted { get; set; }
        }
       
       public class Class
       {
           public string Class_Name { get; set; }
           public int? Class_ID { get; set; }
           public bool Seleceted { get; set; }
       }
       /// </summary>
      

       #region get Class

       public class Get_class_res
        {
            public Get_classess success { get; set; }
        }

        public class Get_classess
       {
           public List<Get_Class> Get_Classs { get; set; }
       }

        public class Get_Class
        {
            public int? Class_ID { get; set; }
            public string Class_Name{ get; set; }
        }
       #endregion


        #region get Subject

        public class Get_Subject_res
        {
            public Get_Subject success { get; set; }
        }

        public class Get_Subject
        {
            public List<Get_Subjects> Get_Subjects { get; set; }
        }

        public class Get_Subjects
        {
            public int? Subject_id { get; set; }
            public string Subject_Name { get; set; }
        }
        #endregion



        #region get Chapter

        public class Get_Chapter_res
        {
            public Get_Chapter success { get; set; }
        }

        public class Get_Chapter
        {
            public List<Get_Chapters> Get_Chapters { get; set; }
        }

        public class Get_Chapters
        {
            public int? Chapter_ID { get; set; }
            public string Chapter_Name { get; set; }
        }
        #endregion

        #region-----Doubt_details
        public class AskaDoubt
        {
            public int? Regd_ID { get; set; }
          public int? Subject_ID {get;set;}
          public int? Chapter_ID{get;set;}
          public string Question_Detail{get;set;}
          public int? Board_id { get; set; }
          public int? Class_id{get;set;} 
          public string MyImages{get;set;}
        }
#endregion


        public class Doubtsuccess
        {
            public message_doubt success { get; set; }
        }

        public class message_doubt
        {
            public string Message { get; set; }
            public int Ticket_id { get; set; }
        }

        public class all_doubt_success
        {
            public success_doubts_all success { get; set; }
        }

        public class success_doubts_all
        {
            public List< doubt_list> doubt_list { get;set;}
        }

        public class doubt_list
        {
            public string Question { get; set; }
            public int? Ticket_id { get; set; }
            public DateTime? Created_on { get; set; }

            public string Status { get; set; }

            public string Ticket_No { get; set; }
        }

        public class Doubt_details_success
        {
            public Doubt_alldetails success { get; set; }
        }
        public class Doubt_alldetails 
        {

            public Doubt_details doubts { get; set; }

         }

        public class Doubt_details
        {

            public  question_details questiondetails { get; set; }
            public List<answer_details> answerdetails { get; set; }
            //public List<Doubt_single_thread> Thread_details { get; set; }

           

          
          

           
        }
        public class answer_details
        {
            public string Answer { get; set; }
            public string Answer_image { get; set; }
            public string Asked_by { get; set; }
            public string Answered_by { get; set; }
            public string Status { get; set; }
              public DateTime? Answer_date { get; set; }
              public int? answer_id { get; set; }
            public string Remark { get; set; }
             public string Answer_by_profile { get; set; }

            public string Asked_Profile_image { get; set; }



            public bool? is_teacher { get; set; }

            public int commentid { get; set; }
        }


       public class question_details
       {
           public int Ticket_id { get; set; }
          
           public string Question { get; set; }
           public string Question_image { get; set; }
            public DateTime? Question_date { get; set; }
             public string Ticket_No { get; set; }
            public string Qsubject { get; set; }


            public string Asked_by { get; set; }

            public string Asked_Profile_image { get; set; }

            public string Status { get; set; }
       }
        public class Doubt_single_thread
        {
            public string Reply_message { get; set; }
            public string Reply_Image { get; set;}
            public string Remark_by { get; set; }
            public string Profile_image { get; set; }
            public bool? Is_teacher { get; set; }

            public DateTime? Reply_Date { get; set; }
        }

        public class Answer_rply_success
        {
            public Answer_rply_msg success { get;set;}
            
        }
      
        public class Answer_rply_msg
        {
            public string message { get; set; }
        }


        public class Success_message_Blank
        {
            public Message_for_success success { get; set; }
        }
        public class Message_for_success
        {
            public string message { get; set; }
        }


        [HttpGet]
        public HttpResponseMessage Doubt(int? id)
        {
            if (id != null)
            {
                var get_Student=DbContext.tbl_DC_Registration_Dtl.Where(x=>x.Regd_ID==id).FirstOrDefault();
               int Board=Convert.ToInt32(get_Student.Board_ID);
               int Class = Convert.ToInt32(get_Student.Class_ID);

              var Board_data=  DbContext.SP_DC_Askdoubt_Board(id).ToList();
              var class_data = DbContext.SP_DC_Askdoubt_Class(id, Board).ToList(); 

                try
                {
                    var obj = new Success_Doubt
                    {
                        success = new success
                        {
                            Board = (from c in Board_data
                                    select new Boards
                                    {
                                        Board_ID=c.Board_ID,
                                        Board_Name=c.Board_Name,
                                        Seleceted=Board==c.Board_ID?true:false
                                    }).ToList(),

                            Class = (from c in class_data
                                     select new Class
                                     {
                                         Class_Name = c.Class_Name,
                                         Class_ID = c.Class_ID,
                                         Seleceted = Class == c.Class_ID ? true : false
                                     }).ToList(),
                        }
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, obj);
                }
                catch (Exception)
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
                        Message = "Data not found."
                    }
                };
                return Request.CreateResponse(HttpStatusCode.OK, obj);
            }

        }

        [HttpGet]
        public HttpResponseMessage Classs(int?id,int?eid)
        {
            try
            {
                if (id != null && eid != null)
                {
                    var Get_Class = DbContext.SP_DC_Askdoubt_Class(id, eid).ToList();
                    var obj = new Get_class_res
                    {
                        success = new Get_classess
                        {
                            Get_Classs = (from c in Get_Class
                                         select new Get_Class
                                             {
                                                 Class_ID=c.Class_ID,
                                                 Class_Name=c.Class_Name
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
                            Message = "Data not found."
                        }
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, obj);
                }
            }
            catch (Exception)
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

        [HttpGet]
        public HttpResponseMessage Subject(int? id, int? eid, int? ClsId)
        {
            if(id!=null && eid!=null&& ClsId!=null)
            {
                var get_sub = DbContext.SP_DC_Askdoubt_Subject(id, eid, ClsId).ToList();
                try
                {
                    var obj = new Get_Subject_res
                    {
                        success = new Get_Subject
                        {
                           Get_Subjects = (from c in get_sub
                                           select new Get_Subjects
                                         {
                                            Subject_id=c.Subject_ID,
                                            Subject_Name=c.Subject_Name
                                         }).ToList()
                        }
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, obj);
                }
                catch (Exception)
                {

                    var obj = new DigiChamps.Models.Digichamps.ErrorResult_Exam
                    {
                        error = new Digichamps.ErrorResponse_Exam
                        {
                            Message = "Data not found."
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
                        Message = "Data not found."
                    }
                };
                return Request.CreateResponse(HttpStatusCode.OK, obj);
            }
            
        }

        [HttpGet]
        public HttpResponseMessage Chapter(int? id, int? eid, int? ClsId, int? SubId)
        {
            if(id!=null && eid!=null&& ClsId!=null && SubId!=null)
            {
                var get_chap = DbContext.SP_DC_Askdoubt_Chapter(id, eid, ClsId, SubId).ToList();
                try
                {
                    var obj = new Get_Chapter_res
                    {
                        success = new Get_Chapter
                        {
                            Get_Chapters = (from c in get_chap
                                           select new Get_Chapters
                                         {
                                             Chapter_ID=c.Chapter_ID,
                                             Chapter_Name=c.Chapter_Name
                                         }).ToList()
                        }
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, obj);
                }
                catch (Exception)
                {

                    var obj = new DigiChamps.Models.Digichamps.ErrorResult_Exam
                    {
                        error = new Digichamps.ErrorResponse_Exam
                        {
                            Message = "Data not found."
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
                        Message = "Data not found."
                    }
                };
                return Request.CreateResponse(HttpStatusCode.OK, obj);
            }
            
        }
    
        [HttpPost]
        public HttpResponseMessage AskDoubt()
        {
             try
            {
                var httprequest = HttpContext.Current.Request;
                int? Subject_ID = Convert.ToInt32(httprequest.Form["Subject_ID"]);
                int ?Chapter_ID = Convert.ToInt32(httprequest.Form["Chapter_ID"]);
                int ?_board_id = Convert.ToInt32(httprequest.Form["Board_id"]);
                string Question_Detail = Convert.ToString(httprequest.Form["Question_Detail"]);
                int ?_class_id = Convert.ToInt32(httprequest.Form["Class_id"]);
                int ?_student_id = Convert.ToInt32(httprequest.Form["Regd_ID"]);
                var _student_details = DbContext.tbl_DC_Registration.Where(x => x.Regd_ID == _student_id).FirstOrDefault();
               
                    var ticket_autoid = DbContext.SP_DC_Generate_Ticket_ID().FirstOrDefault() + _student_id;
                    tbl_DC_Ticket _ticket = new tbl_DC_Ticket();
                    _ticket.Ticket_No = Convert.ToString(ticket_autoid);
                    _ticket.Student_ID = _student_id;
                    _ticket.Board_ID = _board_id;
                    _ticket.Class_ID = _class_id;
                    _ticket.Subject_ID = Subject_ID;
                    _ticket.Chapter_ID = Chapter_ID;
                    _ticket.Question = Question_Detail;
                    string guid = Guid.NewGuid().ToString();
                    //var docfile = new List<string>(); 
                 if (httprequest.Files.Count > 0)
                {
                    foreach (string files in httprequest.Files)
                    {
                        var postedfile = httprequest.Files[files];
                        var path = HttpContext.Current.Server.MapPath("~/Images/Qusetionimages/" + guid + postedfile.FileName);
                        postedfile.SaveAs(path);
                        //docfile.Add(path);
                        _ticket.Question_Image = guid + postedfile.FileName;
                    }
                 }
                    _ticket.Inserted_Date = today;
                    _ticket.Inserted_By = _student_id;
                    _ticket.Status = "O";
                    _ticket.Is_Active = true;
                    _ticket.Is_Deleted = false;
                    DbContext.tbl_DC_Ticket.Add(_ticket);
                    DbContext.SaveChanges();
                    sendMail_ticketgenerate("Ticket_Generate", _student_details.Email, _student_details.Customer_Name, ticket_autoid.ToString());


                try
                {
                    var pushnot = (from c in DbContext.tbl_DC_Registration.Where(x => x.Regd_ID == _student_details.Regd_ID)

                                   select new { c.Regd_ID, c.Device_id }).FirstOrDefault();
                    string body = "tktid#{{tktid}}# DOUBT {{ticketno}} has been generated Dear {{name}}, ! Your doubt no- {{ticketno}} has been created. It will get resolved by your DigiGuru within 1working day.";
                    string msg = body.ToString().Replace("{{name}}", _student_details.Customer_Name).Replace("{{ticketno}}", ticket_autoid).Replace("{{tktid}}", _ticket.Ticket_ID.ToString());
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

                var obj = new Doubtsuccess
                    {
                        success = new message_doubt
                        {
                            Message="Doubt submited successfully.",
                            Ticket_id=_ticket.Ticket_ID
                        }
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, obj);
                
                
            }



            catch (Exception ex)
            {
                var obj = new DigiChamps.Models.Digichamps.ErrorResult_Exam
                {
                    error = new Digichamps.ErrorResponse_Exam
                    {
                        Message = "Data not found."
                    }
                };
                return Request.CreateResponse(HttpStatusCode.OK, obj);
            }
        }

        public bool sendMail_ticketgenerate(string parameter, string email, string name, string ticket_no)
         {
             var getall = DbContext.SP_DC_Get_maildetails(parameter).FirstOrDefault();
             string eidTo = email;
             string toshow = getall.SMTP_Sender.ToString();
             string from_mail = getall.SMTP_Email;
             string eidFrom = getall.SMTP_User.ToString();
             string password = getall.SMTP_Pwd.ToString();
             string ticket = ticket_no;
             string msgsub = getall.Email_Subject.ToString().Replace("{{ticketno}}", ticket_no);
             string hostname = getall.SMTP_HostName;
             string portname = getall.SMTP_Port.ToString();
             bool ssl_tof = true;
             string msgbody = getall.EmailConf_Body;
             if (ticket_no != "")
             {
                 msgbody = getall.EmailConf_Body.ToString().Replace("{{name}}", name).Replace("{{ticketno}}", ticket).Replace("{{date}}", DateTime.Now.ToString());
             }
             MailMessage greetings = new MailMessage();
             SmtpClient smtp = new SmtpClient();
             try
             {
                 greetings.From = new MailAddress(from_mail, toshow);//sendername
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

        [HttpGet]
        public HttpResponseMessage Get_All_Doubts(int id)
        {
             var b = DbContext.View_DC_Tickets_and_Teacher.Where(x => x.Student_ID == id).OrderByDescending(x => x.Ticket_ID).ToList();
             try
             {
                 if(b.Count>0)
                 {
                     var obj = new all_doubt_success
                     {
                         success = new success_doubts_all
                         {
                             doubt_list = (from c in b
                                           select new doubt_list
                                           {
                                               Question = c.Question,
                                               Ticket_id = c.Ticket_ID,
                                               Created_on = c.Inserted_Date,
                                               Status=c.Status,
                                               Ticket_No=c.Ticket_No,
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
                             Message = "You have not asked any doubt."
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
                         Message = "Data not found."
                     }
                 };
                 return Request.CreateResponse(HttpStatusCode.OK, obj);
             }
         }


        [HttpGet]
        public HttpResponseMessage Get_Doubt_Details(int id)
        {
            try
            {

                var get_ticket = DbContext.View_DC_Tickets_and_Teacher.Where(x => x.Ticket_ID == id).FirstOrDefault();
                if (get_ticket != null)
                {
                    int ask_id = Convert.ToInt32(get_ticket.Regd_ID);

                    var get_sdata = DbContext.tbl_DC_Registration.Where(x => x.Regd_ID == ask_id).FirstOrDefault();

                    var get_ticket_answer = DbContext.tbl_DC_Ticket_Dtl.Where(x => x.Ticket_ID == id).ToList();
                    if (get_ticket_answer.Count > 0)
                    {
                        int rply_id = Convert.ToInt32(get_ticket_answer.ToList()[0].Replied_By);
                        var get_data = DbContext.tbl_DC_Teacher.Where(x => x.Teach_ID == rply_id).FirstOrDefault();




                        var get_ticket_thread = (from a in DbContext.tbl_DC_Ticket_Thread.Where(x => x.Ticket_ID == id)
                                                 select new answer_details

                                                 {
                                                     Answer = a.User_Comment,
                                                     Answer_image = a.R_image == null ? "" : a.R_image,
                                                     answer_id = a.Ticket_Dtl_ID,
                                                     is_teacher = a.Is_Teacher,
                                                     Answered_by = get_data.Teacher_Name,
                                                     Answer_by_profile = get_sdata.Image == null ? "" : "/Images/Profile/" + get_sdata.Image,
                                                     Answer_date = a.User_Comment_Date,
                                                     commentid = a.Comment_ID

                                                 }).Union(from a in DbContext.tbl_DC_Ticket_Dtl.Where(x => x.Ticket_ID == id)
                                                          select new answer_details

                                                          {
                                                              Answer = a.Answer,
                                                              Answer_image = a.Answer_Image == null ? "" : a.Answer_Image,
                                                              answer_id = a.Ticket_Dtls_ID,
                                                              is_teacher = true,
                                                              Answered_by = get_data.Teacher_Name,
                                                              Answer_by_profile = get_sdata.Image == null ? "" : "/Images/Profile/" + get_sdata.Image,
                                                              Answer_date = a.Replied_Date,
                                                              commentid = 0

                                                          }).OrderBy(x => x.commentid).ToList();


                        var obj = new Doubt_details_success
                        {
                            success = new Doubt_alldetails
                            {
                                doubts = new Doubt_details

                                {
                                    questiondetails = new question_details
                                    {
                                        Ticket_id = get_ticket.Ticket_ID,
                                        Ticket_No = get_ticket.Ticket_No,

                                        Question = get_ticket.Question,
                                        Question_image = get_ticket.Question_Image == null ? "" : "/Images/Qusetionimages/" + get_ticket.Question_Image,
                                        Question_date = get_ticket.Inserted_Date,
                                        Qsubject = get_ticket.Subject,
                                        Asked_by = get_sdata.Customer_Name,
                                        Asked_Profile_image = get_sdata.Image == null ? "" : "/Images/Profile/" + get_sdata.Image,
                                        Status = get_ticket.Status,

                                    },
                                    answerdetails = (from c in get_ticket_thread
                                                     select new answer_details
                                                     {

                                                         Answer = c.Answer,
                                                         Answer_image = c.Answer_image == "" ? "" : c.is_teacher==true? "/Images/Answerimage/" + c.Answer_image: "/Images/Qusetionimages/" + c.Answer_image,
                                                         answer_id = c.answer_id,
                                                         is_teacher = c.is_teacher,
                                                         Answered_by = get_data.Teacher_Name,
                                                         Answer_by_profile = get_data.Image == "" ? "" : "/Images/Teacherprofile/" + get_data.Image,
                                                         Answer_date = c.Answer_date,
                                                         Asked_by = get_sdata.Customer_Name,
                                                         Asked_Profile_image = get_sdata.Image == "" ? "" : "/Images/Profile/" + get_sdata.Image,
                                                         Remark = get_ticket.Remark,
                                                     }).OrderBy(x => x.Answer_date).ToList(),
                                    //Thread_details = (from c in get_ticket_thread select new Doubt_single_thread
                                    //{
                                    //Remark_by=get_user_name(c.User_Id,c.Is_Teacher,"N"),
                                    //Reply_message=c.User_Comment,
                                    //Reply_Image = "/Images/Qusetionimages/" + c.R_image,
                                    //Is_teacher=c.Is_Teacher,
                                    //Profile_image = get_user_name(c.User_Id, c.Is_Teacher, "P"),
                                    //Reply_Date=c.User_Comment_Date

                                    //}).ToList()
                                }
                            }
                        };

                        return Request.CreateResponse(HttpStatusCode.OK, obj);

                    }
                    else
                    {



                        var obj = new Doubt_details_success
                        {
                            success = new Doubt_alldetails
                            {
                                doubts = new Doubt_details

                                {
                                    questiondetails = new question_details
                                    {
                                        Ticket_id = get_ticket.Ticket_ID,
                                        Ticket_No = get_ticket.Ticket_No,

                                        Question = get_ticket.Question,
                                        Question_image = get_ticket.Question_Image == null ? "" : "/Images/Qusetionimages/" + get_ticket.Question_Image,
                                        Question_date = get_ticket.Inserted_Date,
                                        Qsubject = get_ticket.Subject,
                                        Asked_by = get_sdata.Customer_Name,
                                        Asked_Profile_image = get_sdata.Image == null ? "" : "/Images/Profile/" + get_sdata.Image,
                                        Status = get_ticket.Status,

                                    },
                                    answerdetails = new List<answer_details>(),
                                    //Thread_details = (from c in get_ticket_thread select new Doubt_single_thread
                                    //{
                                    //Remark_by=get_user_name(c.User_Id,c.Is_Teacher,"N"),
                                    //Reply_message=c.User_Comment,
                                    //Reply_Image = "/Images/Qusetionimages/" + c.R_image,
                                    //Is_teacher=c.Is_Teacher,
                                    //Profile_image = get_user_name(c.User_Id, c.Is_Teacher, "P"),
                                    //Reply_Date=c.User_Comment_Date

                                    //}).ToList()
                                }
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
                            Message = "Data not found."
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
                        Message = "Data not found."
                    }
                };
                return Request.CreateResponse(HttpStatusCode.OK, obj);
            }
        }

        private string get_user_name(int? nullable1, bool? nullable2, string msg)
        {
            string msgs = string.Empty;
            if(nullable2==true)
            {
                var get_data = DbContext.tbl_DC_Teacher.Where(x => x.Teach_ID == nullable1).FirstOrDefault();
                if(msg=="N")
                {
                    msgs = get_data.Teacher_Name;
                }
                else if (msg == "P")
                {
                    msgs = "/Images/Teacherprofile/" + get_data.Image;
                }
            }
            else
            {
                var get_sdata = DbContext.tbl_DC_Registration.Where(x => x.Regd_ID == nullable1).FirstOrDefault();
                if (msg == "N")
                {
                    msgs = get_sdata.Customer_Name;
                }
                else if (msg == "P")
                {
                    msgs = "/Images/Profile/" + get_sdata.Image;
                }
            }
            return msgs;
        }

        [HttpPost]
        public HttpResponseMessage Answer_Rply()
        {
            var httprequest = HttpContext.Current.Request;
            int? Ticket_id = Convert.ToInt32(httprequest.Form["Ticket_id"]);
            int? Answer_id = Convert.ToInt32(httprequest.Form["Answer_id"]);
            int? Reg_id = Convert.ToInt32(httprequest.Form["Reg_id"]);
            string msgbody = Convert.ToString(httprequest.Form["msgbody"]);
            try
            {
                tbl_DC_Ticket_Thread _ticket_thred = new tbl_DC_Ticket_Thread();

                _ticket_thred.Ticket_ID = Ticket_id;
                _ticket_thred.Ticket_Dtl_ID = Answer_id;
                _ticket_thred.User_Comment = msgbody;
                _ticket_thred.User_Comment_Date = today;
                _ticket_thred.User_Id = Reg_id;
                _ticket_thred.Is_Teacher = false;
                string guid = Guid.NewGuid().ToString();
                if (httprequest.Files.Count > 0)
                {
                    foreach (string files in httprequest.Files)
                    {
                        var postedfile = httprequest.Files[files];
                        var path = HttpContext.Current.Server.MapPath("~/Images/Qusetionimages/" + guid + postedfile.FileName);
                        postedfile.SaveAs(path);
                        //docfile.Add(path);
                        _ticket_thred.R_image = guid + postedfile.FileName;
                    }
                }
                _ticket_thred.Is_Active = true;
                _ticket_thred.Is_Deleted = false;
                DbContext.tbl_DC_Ticket_Thread.Add(_ticket_thred);
                DbContext.SaveChanges();
                var obj = new Answer_rply_success
                {
                    success = new Answer_rply_msg
                    {
                        message="Rply submited successfuly."
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
                        Message = "Something went wrong."
                    }
                };
                return Request.CreateResponse(HttpStatusCode.OK, obj);
            }
        }
       

    }
}
