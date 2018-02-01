using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DigiChamps.Models
{
    public class Digichamps_web_Api
    {
        public class result
        {
            public LoginResponse Success { get; set; }
        }
        public class LoginResponse
        {
            public string Message { get; set; }
            public Nullable<int> Regid { get; set; }
            public string UserName { get; set; }
            public DateTime logtime { get; set; }
            public string Name { get; set; }
            public int Isselect { get; set; }

            public bool AlreadyLogin { get; set; }

            public int SessionID { get; set; }
        }

        public class Errorresult
        {
            public Errorresponse Error { get; set; }
        }
        public class Errorresponse
        {
            public string Message { get; set; }
        }

        public class OTPresultRESPONSE
        {
            public OTPresult Result { get; set; }
        }
        public class OTPresult
        {
            public string Message { get; set; }
        }

        public class LoginRequest
        {
            public string username { get; set; }
            public string password { get; set; }
        }
        public class Authenticate
        {
            public string Auth_Key { get; set; }
        }

        public class dashboardSuccessResult
        {
            public student_dashboard success { get; set; }

        }
        public class student_dashboard
        {
            public int? ord_customer { get; set; }
            
            public float Raised { get; set; }
            public float open { get; set; }
            public float Reject { get; set; }
            public float Closed { get; set; }
            public List<DigiChamps.Models.tbl_DC_Exam_Result> test;
            public List<DigiChamps.Models.SP_DC_Order_Details_Result> packagedetail;
            public int? exams { get; set; }
            // public int? pkgmnth { get; set; }
            public string cls { get; set; }

            public string Exam_Namee { get; set; }
            public int Total_Question { get; set; }
            public int Total_Attempted { get; set; }
            public int Total_Answered { get; set; }

            public string testdate { get; set; }
            public string testtime { get; set; }
            public string remain_time { get; set; }

            public string Exam_Name { get; set; }

            public int chaptersubscribe { get; set; }
        }
        public class drawerSuccessResult
        {
            public userdrawer success { get; set; }

        }
        public class userdrawer
        {
            public string Image { get; set; }
            public string name { get; set; }
            public bool? Isassigned { get; set; }
        }

        public class changepassword
        {
            public Nullable<int> id { get; set; }
            public string old_password { get; set; }
            public string new_password { get; set; }
            public string confirm_password { get; set; }
        }
        public class chngpasswordresult
        {
            public ChangepasswordResponse success { get; set; }
        }
        public class ChangepasswordResponse
        {
            public string Message { get; set; }
        }
        public class learnresultRESPONSE
        {
            public Pkglearnmodel success { get; set; }
        }
        public class Pkglearnmodel
        {
            public  List<pkgLearnSubjects> Subjectlists { get; set; }
        }
        public class pkgLearnSubjects
        {
            public int total_pdfs;
            
            public Nullable<int> subjectid { get; set; }
            public string subject { get; set; }
            public int? total_chapters { get; set; }
            public int total_videos { get; set; }


            public int Total_Pre_req_test { get; set; }

            public int Total_question_pdf { get; set; }

            public int Total_question { get; set; }
        }
        public class Successimageresult
        {

            public Successimageresponse Success { get; set; }



        }


        public class sqresult
        {
            public sqResponse Success { get; set; }

        }
        public class sqresult1
        {
            public sqResponse1 Success { get; set; }

        }
        public class sqResponse1
        {
            public string Message { get; set; }
            //public int log_status { get; set; }
            //public int log_id { get; set; }


            public int Regid { get; set; }

            public int SessionID { get; set; }

            public bool AlreadyLogin { get; set; }
        }
        public class sqResponse
        {
            public string Question { get; set; }

        }

        public class Successimageresponse
        {

            public string Message { get; set; }
            public string image { get; set; }
            public int Regd_ID { get; set; }

        }
        public class learnsubjectwiseRESPONSE
        {
            public Pkgsubjectwisemodel success { get; set; }
        }
        public class Pkgsubjectwisemodel
        {
            public List<pkgLearnChapters> Chapterlist { get; set; }

            public int Total_Chapters { get; set; }

            public int Total_Online_test { get; set; }

            public int Total_Pre_req_test { get; set; }

            public int Total_question_pdf { get; set; }

            public int Total_Videos { get; set; }

            public int? Total_question { get; set; }
            public int total_pdfs { get; set; }
        }
        public class pkgLearnChapters
        {
            public Nullable<int> chapterid { get; set; }
            public string Chapter { get; set; }
            public int? total_pdfs { get; set; }
            public int total_videos { get; set; }

            public int total_question_pdf { get; set; }

            public int online_test { get; set; }

            public int Pre_req_test { get; set; }
        }
        //public class pkgLearnChapters
        //{
        //    public int ChapterId { get; set; }

        //}

        public class academicFilterResponse
        {
            public int? Board_id { get; set; }
            public int? Class_id { get; set; }
            public List<DigiChamps.Controllers.AcademicController.DigiChampsApiBoardModel> Board_list { get; set; }
            public List<DigiChamps.Controllers.AcademicController.DigiChampsApiClassModel> Class_list { get; set; }
        }

        public class ChapterDetailsRESPONSE
        {
            public chapterlist success { get; set; }
        }
      

        public class Questionbanks
        {
            public Nullable<int> noofques { get; set; }
            public string Question_Pdfs { get; set; }


            public string Modulename { get; set; }
        }
        public class Chapterpdfs
        {

            public string pdf_file { get; set; }

        }
        public class ChapterModuleList
        {
            public Nullable<int> Module_Id { get; set; }
            public string Module_Title { get; set; }
            public string Description { get; set; }
            public string Module_Image { get; set; }
            public string Image_Key { get; set; }
            public bool Is_Avail { get; set; }
            //public string Video_Content { get; set; }
            public string pdf_file { get; set; }
            public string pdf_name { get; set; }
            public bool? Is_Free { get; set; }
            public Nullable<DateTime> Validity { get; set; }
            public string Question_Pdf { get; set; }
            public string Question_Pdf_Name { get; set; }
            public Nullable<int> No_Of_Question { get; set; }
            public bool? Is_Free_Test { get; set; }

            public string Media_Id { get; set; }

            public string VideoKey { get; set; }

            public string template_id { get; set; }

            public string thumbnail_key { get; set; }

            //public Nullable<bool> Is_Offline { get; set; }

            public bool Is_Expire { get; set; }
        }
        public class VideoDetailsRESPONSE
        {
            public videolist success { get; set; }
        }
        public class videolist
        {
            public string ModuleName { get; set; }
            public string thumbnail_key { get; set; }
            public string template_id { get; set; }
            public string media_Id { get; set; }
        }

        public class chapterlist
        {
            public Nullable<int> Chapterid { get; set; }
            public string Chapter_Name { get; set; }
            public List<ChapterModuleList> ChapterModules { get; set; }
           
            public List<Questionbanks> Quesbank;

            public List<Pdf_url> pdfs;
            public DateTime Today_date { get; set; }
            public bool? Is_Offline { get; set; }
        }
        public class Pdf_url
        {
            public string Url { get; set; }

            public string Modulename { get; set; }
        }
    }
}