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
    public class LearnController : ApiController
    {
        DateTime today = DigiChampsModel.datetoserver();
        DigiChampsEntities DbContext = new DigiChampsEntities();
        // GET api/<controller>
        [HttpGet]
        public HttpResponseMessage GetLearnPackage(int? id)
        {
            try
            {
                var stuobj = DbContext.View_All_Student_Details.Where(x => x.Regd_ID == id && x.IS_ACCEPTED == true).FirstOrDefault();
                    if (stuobj != null)
                    {
                        int classid = Convert.ToInt16(stuobj.Class_ID);
                        var subjects = (from d in DbContext.VW_DC_Package_Learn.Where(x => x.Regd_ID == id && x.Class_ID == classid).GroupBy(x => x.Subject_ID)
                                        select new PackagePreviewModel
                                        {
                                            Subject_Id = d.FirstOrDefault().Subject_ID,
                                            Subject = d.FirstOrDefault().Subject_Name,
                                            Total_Chapter = d.FirstOrDefault().Total_Chapter
                                        }).ToList();
                        //List<>
                        if (subjects.Count > 0)
                        {
                            var resultobj1 = new Digichamps_web_Api.learnresultRESPONSE
                            {
                                success = new Digichamps_web_Api.Pkglearnmodel
                                {
                                    Subjectlists = (from c in subjects
                                                    select new Digichamps_web_Api.pkgLearnSubjects
                                                    {
                                                        subjectid = c.Subject_Id,
                                                        subject = c.Subject,
                                                        total_chapters = DbContext.tbl_DC_Module.Where(x => x.Subject_Id == c.Subject_Id && x.Is_Active == true && x.Is_Deleted == false && x.Module_video != null).Select(x => x.Chapter_Id).Distinct().ToList().Count,
                                                        total_videos = DbContext.tbl_DC_Module.Where(x => x.Subject_Id == c.Subject_Id && x.Is_Active == true && x.Is_Deleted == false && x.Module_video != null).Select(x => x.Module_ID).ToList().Count,
                                                        Total_Pre_req_test = DbContext.tbl_DC_Exam.Where(x => x.Subject_Id == c.Subject_Id && x.Exam_type == 1 && x.Is_Active == true && x.Is_Deleted == false ).ToList().Count(),
                                                        Total_question_pdf = DbContext.tbl_DC_Module.Where(x => x.Subject_Id == c.Subject_Id && x.Question_PDF != null && x.Is_Active == true && x.Is_Deleted == false ).ToList().Select(x => x.Question_PDF).Count(),
                                                        Total_question = (int) DbContext.tbl_DC_Module.Where(x => x.Subject_Id == c.Subject_Id && x.No_Of_Question != null && x.Is_Active == true && x.Is_Deleted == false ).ToList().Select(x => x.No_Of_Question).Sum(),
                                                        total_pdfs = DbContext.tbl_DC_Module.Where(x => x.Subject_Id == c.Subject_Id && x.Module_Content != null && x.Is_Active == true && x.Is_Deleted == false).ToList().Count,
                                                    }).ToList()
                                }
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, resultobj1);
                        }
                        else {
                            var freesubs = DbContext.Sp_DC_Getall_Packagelearn(classid).Select(x => x.Subject_Id).Distinct().ToList();
                            var resultobj = new Digichamps_web_Api.learnresultRESPONSE
                            {
                                success = new Digichamps_web_Api.Pkglearnmodel
                                {
                                    Subjectlists = (from c in freesubs select new Digichamps_web_Api.pkgLearnSubjects
                                    {
                                        subjectid = c,
                                        subject = DbContext.tbl_DC_Subject.Where(x=> x.Subject_Id == c && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault().Subject,
                                        total_chapters = DbContext.tbl_DC_Module.Where(x=> x.Subject_Id == c && x.Is_Active == true && x.Is_Deleted == false && x.Module_video != null).Select(x=> x.Chapter_Id).Distinct().ToList().Count,
                                        total_videos = DbContext.tbl_DC_Module.Where(x => x.Subject_Id == c && x.Is_Active == true && x.Is_Deleted == false && x.Module_video != null).Select(x=> x.Module_ID).ToList().Count,
                                        total_pdfs = DbContext.tbl_DC_Module.Where(x => x.Subject_Id == c && x.Module_Content != null && x.Is_Active == true && x.Is_Deleted == false).ToList().Count,
                                           Total_question_pdf = DbContext.tbl_DC_Module.Where(x => x.Subject_Id == c && x.Question_PDF != null).ToList().Select(x => x.Question_PDF).Count(),
                                    }).ToList()
                                }
                            };
                        return Request.CreateResponse(HttpStatusCode.OK, resultobj);
                        }
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
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }

        [HttpGet]
        public HttpResponseMessage learnSubjectdetails(int id, int eid)
        {
            try
            {
                var stuobj = DbContext.View_All_Student_Details.Where(x => x.Regd_ID == id && x.IS_ACCEPTED == true).FirstOrDefault();
                if (stuobj != null)
                {
                    var chapters = DbContext.VW_DC_Package_Learn.Where(x => x.Regd_ID == id && x.Subject_ID == eid).ToList();
                    if (chapters.Count > 0)
                    {
                        var data = chapters.Select(x => x.Chapter_ID).Distinct();
                        var resultobj1 = new Digichamps_web_Api.learnsubjectwiseRESPONSE
                        {
                            success = new Digichamps_web_Api.Pkgsubjectwisemodel
                            {
                                Total_Chapters = chapters.Select(x => x.Chapter_ID).Distinct().Count(),
                                Total_Videos =  DbContext.tbl_DC_Module.Where(x => x.Subject_Id == eid && x.Is_Active==true && x.Is_Deleted==false && x.Module_video != null).Select(x => x.Module_ID).ToList().Count(),
                                Total_Online_test = DbContext.tbl_DC_Exam.Where(x => x.Subject_Id == eid && x.Exam_type == 5 && x.Is_Active == true && x.Is_Deleted == false).ToList().Count(),
                                Total_Pre_req_test = DbContext.tbl_DC_Exam.Where(x => x.Subject_Id == eid && x.Exam_type == 1 && x.Is_Active == true && x.Is_Deleted == false).ToList().Count(),
                                Total_question_pdf = DbContext.tbl_DC_Module.Where(x => x.Subject_Id == eid && x.Question_PDF != null && x.Is_Active == true && x.Is_Deleted == false).ToList().Select(x => x.Question_PDF).Count(),
                                Total_question = DbContext.tbl_DC_Module.Where(x => x.Subject_Id == eid && x.No_Of_Question != null && x.Is_Active == true && x.Is_Deleted == false).ToList().Select(x => x.No_Of_Question).Sum(),
                                total_pdfs = DbContext.tbl_DC_Module.Where(x => x.Subject_Id == eid && x.Module_Content != null && x.Is_Active == true && x.Is_Deleted == false).ToList().Count,
                                Chapterlist = (from c in chapters.Select(x => x.Chapter_ID).Distinct()
                                               select new Digichamps_web_Api.pkgLearnChapters
                                               {
                                                   chapterid = c,
                                                   total_question_pdf = DbContext.tbl_DC_Module.Where(x => x.Chapter_Id == c && x.Question_PDF != null).ToList().Select(x => x.Question_PDF).Count(),
                                                   online_test = DbContext.tbl_DC_Exam.Where(x => x.Chapter_Id == c && x.Exam_type == 5).ToList().Count(),
                                                   Pre_req_test = DbContext.tbl_DC_Exam.Where(x => x.Chapter_Id == c && x.Exam_type == 1).ToList().Count(),
                                                   Chapter = DbContext.tbl_DC_Chapter.Where(x => x.Chapter_Id == c).FirstOrDefault().Chapter,
                                                   total_pdfs = DbContext.tbl_DC_Module.Where(x => x.Chapter_Id == c && x.Module_Content != null).Select(x => x.Module_ID).Distinct().ToList().Count,
                                                   total_videos = DbContext.tbl_DC_Module.Where(x => x.Chapter_Id == c).Select(x => x.Module_ID).ToList().Count
                                               }).ToList()
                            }
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, resultobj1);
                    }
                    else
                    {
                        var chapters1 = DbContext.Sp_DC_Getall_Packagelearn(stuobj.Class_ID).ToList();
                        if (chapters1.Count > 0)
                        {
                            var data = chapters.Select(x => x.Chapter_ID).Distinct();
                            var resultobj1 = new Digichamps_web_Api.learnsubjectwiseRESPONSE
                            {
                                success = new Digichamps_web_Api.Pkgsubjectwisemodel
                                {
                                    Total_Chapters = chapters1.Where(x => x.Subject_Id == eid).Select(x => x.Chapter_Id).Distinct().Count(),
                                    Total_Videos = DbContext.tbl_DC_Module.Where(x => x.Subject_Id == eid && x.Class_Id == stuobj.Class_ID  && x.Is_Active == true && x.Is_Deleted == false && x.Module_video != null).Select(x => x.Module_ID).ToList().Count(),
                                  
                                    Total_question_pdf = DbContext.tbl_DC_Module.Where(x => x.Subject_Id == eid && x.Question_PDF != null && x.Is_Active == true && x.Is_Deleted == false).ToList().Select(x => x.Question_PDF).Count(),
                                    Total_question = DbContext.tbl_DC_Module.Where(x => x.Subject_Id == eid && x.No_Of_Question != null && x.Is_Active == true && x.Is_Deleted == false).ToList().Select(x => x.No_Of_Question).Sum(),
                                    total_pdfs = DbContext.tbl_DC_Module.Where(x => x.Subject_Id == eid && x.Module_Content != null && x.Is_Active == true && x.Is_Deleted == false).ToList().Count,
                                    Chapterlist = (from c in chapters1.Where(x => x.Subject_Id == eid).Select(x => x.Chapter_Id).Distinct()
                                                   select new Digichamps_web_Api.pkgLearnChapters
                                                   {
                                                       chapterid = c,
                                                       total_question_pdf = DbContext.tbl_DC_Module.Where(x => x.Chapter_Id == c && x.Question_PDF != null).ToList().Select(x => x.Question_PDF).Count(),
                                                       
                                                       Chapter = DbContext.tbl_DC_Chapter.Where(x => x.Chapter_Id == c).FirstOrDefault().Chapter,
                                                       total_pdfs = DbContext.tbl_DC_Module.Where(x => x.Chapter_Id == c && x.Module_Content != null).Select(x => x.Module_ID).Distinct().ToList().Count,
                                                       total_videos = DbContext.tbl_DC_Module.Where(x => x.Chapter_Id == c).Select(x => x.Module_ID).ToList().Count
                                                   }).ToList()
                                }
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, resultobj1);
                        }
                    }
                    return Request.CreateResponse(HttpStatusCode.OK);
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
            catch
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }

        [HttpGet]
        public HttpResponseMessage AcademicFilterLearn(int id)
        {
            try
            {
                var stuobj = DbContext.View_All_Student_Details.Where(x => x.Regd_ID == id && x.IS_ACCEPTED == true).FirstOrDefault();
                if (stuobj != null)
                {
                    var resultobj1 = new Digichamps_web_Api.academicFilterResponse
                    {
                        Board_id = stuobj.Board_Id,
                        Class_id = stuobj.Class_ID,
                        Board_list = (from a in DbContext.tbl_DC_Board.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                         select new AcademicController.DigiChampsApiBoardModel{
                                            Board_Id = a.Board_Id,
                                            Board_Name = a.Board_Name
                                         }).ToList(),
                        Class_list = (from a in DbContext.tbl_DC_Class.Where(x => x.Is_Active == true && x.Is_Deleted == false && x.Board_Id == stuobj.Board_Id)
                                      select new AcademicController.DigiChampsApiClassModel
                                          {
                                          Class_Id = a.Class_Id,
                                          Class_Name = a.Class_Name
                                          }).ToList()
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, resultobj1);
                }
                else {
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
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }

      [HttpGet]
        public HttpResponseMessage LearnChapterdetails(int id, int eid) //Student id and Chapter id
        {
            try
            {           
                List<DigiChamps.Models.Digichamps_web_Api.Pdf_url> sample1 = new List<DigiChamps.Models.Digichamps_web_Api.Pdf_url>();
                var stuobj = DbContext.View_All_Student_Details.Where(x => x.Regd_ID == id && x.IS_ACCEPTED == true).FirstOrDefault();
                if (stuobj != null)
                {
                    DateTime dt = Convert.ToDateTime(stuobj.Inserted_Date);
                }
                if (stuobj != null)
                {
                    var Chapobj = DbContext.tbl_DC_Chapter.Where(x => x.Chapter_Id == eid && x.Is_Active == true).FirstOrDefault();
                    if (Chapobj != null)
                    {
                        var Chapters = (from d in DbContext.VW_DC_Package_Learn.Where(x => x.Regd_ID == id && x.Chapter_ID == eid).GroupBy(x => x.Chapter_ID)
                                        select new PackagePreviewModel
                                        {
                                            Chapter = d.FirstOrDefault().Chapter_Name,
                                             Is_Offline=d.FirstOrDefault().Is_Offline
                                        }).ToList();

                        var listmodule = DbContext.tbl_DC_Module.Where(x => x.Chapter_Id == eid && x.Is_Active == true && x.Is_Deleted == false && x.Module_video!=null).ToList();
                          var v = DbContext.SP_DC_GetModules_for_api(eid, id).ToList();
                        var listdata =( from c in v.Where(x => x.Question_PDF != null) select new DigiChamps.Models.Digichamps_web_Api.Questionbanks { noofques = c.No_Of_Question, Question_Pdfs = c.Question_PDF, Modulename = c.Module_Name }).ToList();
                        if (Chapters.Count > 0)
                        {
                            var resultobj = new Digichamps_web_Api.ChapterDetailsRESPONSE
                            {
                                success = new Digichamps_web_Api.chapterlist
                                {
                                    Chapter_Name = Chapobj.Chapter,
                                    Chapterid = eid,
                                    Today_date = today,
                                    Is_Offline = Chapters.FirstOrDefault().Is_Offline,
                                    ChapterModules = (from d in v
                                                      select new Digichamps_web_Api.ChapterModuleList
                                                      {
                                                          Module_Id = d.Module_ID,
                                                          Module_Title = d.Module_Name == null ? "" : d.Module_Name,
                                                          Module_Image = d.Module_Image == null ? "" : d.Module_Image,
                                                          Description = d.Module_Desc == null ? "" : d.Module_Desc,
                                                          pdf_file = d.Module_Content == null ? "" : d.Module_Content,
                                                          pdf_name = d.Module_Content_Name == null ? d.Module_Name : d.Module_Content_Name,
                                                          Image_Key = d.Image_Key == null ? "" : d.Image_Key,
                                                          Is_Free = d.Is_Free,
                                                          Validity = d.Validity,
                                                          Question_Pdf = d.Question_PDF == null ? "" : d.Question_PDF,
                                                          Question_Pdf_Name = d.Question_PDF_Name == null ? d.Module_Name : d.Question_PDF_Name,
                                                          No_Of_Question = d.No_Of_Question == null ? 0 : d.No_Of_Question,
                                                          Is_Free_Test = d.Is_Free_Test == null ? false : d.Is_Free_Test,
                                                          Media_Id = d.Module_video == null ? "" : d.Module_video,
                                                          VideoKey = d.Module_video == null ? "" : d.Module_video,
                                                          Is_Expire = d.Module_video == null ? true : false,
                                                          Is_Avail = true,
                                                          //thumbnail_key = "-320",
                                                          thumbnail_key = "",
                                                          //template_id = "-ozl7iD1S",
                                                          template_id = "",
                                                      }).ToList(),
                                    Quesbank = (listdata == null) ? (from c in v select new DigiChamps.Models.Digichamps_web_Api.Questionbanks { noofques = 0, Question_Pdfs = "", Modulename = "" }).ToList() : (from c in listdata.Where(x => x.Question_Pdfs != null) select new DigiChamps.Models.Digichamps_web_Api.Questionbanks { noofques = c.noofques, Question_Pdfs = c.Question_Pdfs, Modulename = c.Modulename }).ToList(),
                                    pdfs = (from c in v.Where(x => x.Module_Content != null) select new DigiChamps.Models.Digichamps_web_Api.Pdf_url{
                                    Url=c.Module_Content,
                                    Modulename = c.Module_Name
                                    }).ToList() == null ? (from c in v select new DigiChamps.Models.Digichamps_web_Api.Pdf_url{
                                    Url="",
                                    Modulename = ""
                                    }).ToList() : (from c in v.Where(x => x.Module_Content != null)
                                                                     select new DigiChamps.Models.Digichamps_web_Api.Pdf_url
                                                                     {
                                                                         Url = c.Module_Content,
                                                                         Modulename = c.Module_Name
                                                                     }).ToList()                                 
                                }
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, resultobj);
                        }
                        else {
                            var resultobj = new Digichamps_web_Api.ChapterDetailsRESPONSE
                            {
                                success = new Digichamps_web_Api.chapterlist
                                {
                                    Chapter_Name = Chapobj.Chapter,
                                    Chapterid = eid,
                                    Today_date = today,
                                    ChapterModules = (from d in DbContext.SP_DC_GetModules_for_api(eid, id)
                                                      select new Digichamps_web_Api.ChapterModuleList
                                                      {

                                                          Module_Id = d.Module_ID,
                                                          Module_Title = d.Module_Name == null ? "" : d.Module_Name,
                                                          Module_Image = d.Module_Image == null ? "" : d.Module_Image,
                                                          Description = d.Module_Desc == null ? "" : d.Module_Desc,
                                                          pdf_file = d.Module_Content == null ? "" : d.Module_Content,
                                                          pdf_name = d.Module_Content_Name == null ? d.Module_Name : d.Module_Content_Name,
                                                          Image_Key = d.Image_Key == null ? "" : d.Image_Key,
                                                          Is_Free = d.Is_Free,
                                                          Validity = d.Validity,
                                                          Question_Pdf = d.Question_PDF == null ? "" : d.Question_PDF,
                                                          Question_Pdf_Name = d.Question_PDF_Name == null ? d.Module_Name : d.Question_PDF_Name,
                                                          No_Of_Question = d.No_Of_Question == null ? 0 : d.No_Of_Question,
                                                          Is_Free_Test = d.Is_Free_Test == null ? false : d.Is_Free_Test,
                                                          Media_Id = d.Is_Free == true ? (d.Validity > today ? d.Module_video : "") : "",
                                                          //template_id = "-ozl7iD1S",
                                                          template_id = "",
                                                          VideoKey = d.Is_Free == true ? (d.Validity > today ? d.Module_video : "") : "",
                                                          Is_Expire = d.Is_Free == true ? (d.Validity > today ? false : true) : true,
                                                          //thumbnail_key = "-320",
                                                          thumbnail_key = "",
                                                          Is_Avail = false,


                                                      }).ToList(),
                                    Quesbank = (listdata == null) ? (from c in v select new DigiChamps.Models.Digichamps_web_Api.Questionbanks { noofques = 0, Question_Pdfs = "", Modulename = "" }).ToList() : (from c in listdata.Where(x => x.Question_Pdfs != null) select new DigiChamps.Models.Digichamps_web_Api.Questionbanks { noofques = c.noofques, Question_Pdfs = c.Question_Pdfs, Modulename = c.Modulename }).ToList(),
                                    pdfs = (from c in v.Where(x => x.Module_Content != null)
                                            select new DigiChamps.Models.Digichamps_web_Api.Pdf_url
                                            {
                                                Url = c.Module_Content,
                                                Modulename = c.Module_Name
                                            }).ToList() == null ? (from c in v
                                                                   select new DigiChamps.Models.Digichamps_web_Api.Pdf_url
                                                                   {
                                                                       Url = "",
                                                                       Modulename = ""
                                                                   }).ToList() : (from c in v.Where(x => x.Module_Content != null)
                                                                                  select new DigiChamps.Models.Digichamps_web_Api.Pdf_url
                                                                                  {
                                                                                      Url = c.Module_Content,
                                                                                      Modulename = c.Module_Name
                                                                                  }).ToList()

                                }
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, resultobj);
                        }
                    }
                    else
                    {
                        var errobj = new Digichamps_web_Api.Errorresult
                        {
                            Error = new Digichamps_web_Api.Errorresponse
                            {
                                Message = "Invalid Chapter details.",
                            }
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, errobj);
                    }
                }
                else {
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
            catch(Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }

        public string validays(DateTime? insertdate,int? validity)
        {


            string  lastdate =Convert.ToString( Convert.ToDateTime(insertdate).AddDays(Convert.ToDouble(validity)));

            return lastdate;

        }
        [HttpGet]
        public HttpResponseMessage LearnVideodetails(int id, int eid) //Student id and Module id
        {
            try
            {
                var stuobj = DbContext.View_All_Student_Details.Where(x => x.Regd_ID == id && x.IS_ACCEPTED == true).FirstOrDefault();
                if (stuobj != null)
                {
                    var Modobj = DbContext.tbl_DC_Module.Where(x => x.Module_ID == eid && x.Is_Active == true).FirstOrDefault();
                    if (Modobj != null)
                    {
                        var sentobj = new Digichamps_web_Api.VideoDetailsRESPONSE
                        {
                            success = new Digichamps_web_Api.videolist
                            {
                                ModuleName = Modobj.Module_Name,
                                //template_id = "-ozl7iD1S",
                                template_id="",
                                //thumbnail_key = "-320",
                                thumbnail_key="",
                                media_Id = Modobj.Module_video
                            }
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, sentobj);
                    }
                    else
                    {
                        var errobj = new Digichamps_web_Api.Errorresult
                        {
                            Error = new Digichamps_web_Api.Errorresponse
                            {
                                Message = "Invalid Video details.",
                            }
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, errobj);
                    }
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
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }
    }
}