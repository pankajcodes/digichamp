using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Web.Security;
using System.Text;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;

namespace DigiChamps.Models
{
    public class Digichamps
    {
        public class Student_Edit
        {
            public int? Regd_ID { get; set; }
            public string Regd_No { get; set; }
            public string Customer_Name { get; set; }
            public string Email { get; set; }
            public string Mobile { get; set; }
            public string Phone { get; set; }
            public string Organisation_Name { get; set; }
            public string Pincode { get; set; }
            public string Address { get; set; }
            public string Image_Url { get; set; }
            public string Profile_Status { get; set; }
            public int? Security_Question_ID { get; set; }
            public int? Board_ID { get; set; }
            public int? Class_ID { get; set; }
            public string Board_Name { get; set; }
            public string Class_Name { get; set; }
            public string Security_Question { get; set; }
            public string Security_Answer { get; set; }
            public List<Security_Question> securityquestions;
        }
        public class profiledetails
        {
            public Student_Edit success;
           
        }
        public class Security_Question
        {
            public int? Security_Question_ID { get; set; }
            public string Security_Questions { get; set; }
        }
        public class errormessage
        {
            public Display error { get; set; }
        }
        public class Display
        {
            public string Message { get;set; }
        }
        public class SignUp
        {
            public Nullable<int> Regd_ID { get; set; }
            public string Regd_No { get; set; }
            public string Customer_Name { get; set; }
            public string Email { get; set; }
            public string Mobile { get; set; }
            public DateTime DateOfBirth { get; set; }
            public bool gender { get; set; }
        }
        public class otp
        {
            public Nullable<int> Regd_ID { get; set; }
            //public string Mobile { get; set; }
            //public string Otp { get; set; }

            public string Message { get; set; }

        }
        public class otp_confirmation
        {
            public Nullable<int> Regd_ID { get; set; }
            public string Mobile { get; set; }
            public string Otp { get; set; }
            public string Confirm_password { get; set; }
            public string New_password { get; set; }
            public string Device_Id { get; set; }
        }
        public class otp_confirm_message
        {
            public Nullable<int> Regd_ID { get; set; }
            public string Mobile { get; set; }
            public string Message { get; set; }

            public bool AlreadyLogin { get; set; }

            public int SessionID { get; set; }
        }
        public class SuccessResponse
        {
            public string Message { get; set; }
        }
        public class SuccessResult
        {
            public SuccessResponse success { get; set; }
       
        }

        public class SuccessregistratonResult
        {
            public SuccessregistratonResponse success { get; set; }

        }

        public class SuccessregistratonResponse
        {
            public string Message { get; set; }

            public bool AlreadyLogin { get; set; }

            public int SessionID { get; set; }

            public int? Regid { get; set; }
        }
        public class OTPSuccessResult
        {
            public otp_confirm_message success { get; set; }

        }
        public class SuccessResults
        {
            public otp success { get; set; }
        }
        public class ErrorResponse
        {
            public string Message { get; set; }
        }
        public class ErrorResult
        {
            public ErrorResponse error { get; set; }
        }
        public static DateTime datetoserver()
        {
            string zoneId = "India Standard Time";
            TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById(zoneId);
            DateTime result = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tzi);
            return result;
        }
        public class Encrypt_Password
        {
            public static string HashPassword(string pasword)
            {
                byte[] arrbyte = Encoding.UTF8.GetBytes(string.Concat(pasword, "#s$"));
                SHA256 hash = new SHA256CryptoServiceProvider();
                arrbyte = hash.ComputeHash(arrbyte);
                return Convert.ToBase64String(arrbyte);
            }
        }

        public class Registration_Detail
        {
            public string Regd_No { get; set; }
            public int? Regd_ID { get; set; }
            public int? Board_ID { get; set; }
            public int? Class_ID { get; set; }
            public int? Secure_Id { get; set; }
            public string Answer { get; set; }
            public string DateOfBirth { get; set; }
            public bool? Gender { get; set; }
            public bool? Is_Active { get; set; }
            public bool? Is_Deleted { get; set; }
            public DateTime? Inserted_Date { get; set; }
            public string Inserted_By { get; set; }

        }
        //public class SuccessResponse
        //{
        //    public string Message { get; set; }
        //}
        //public class SuccessResult
        //{
        //    public SuccessResponse success { get; set; }

        //}


        public class Question_List
        {
            public List<Security_Question> question_list { get; set; }
        }
        public class success_ques
        {
            public Question_List success { get; set; }
        }

        public class profileget
        {
            public Student_Profile success;

        }

        public class globaltest
        {

            public globalexamlist success { get; set; }
        }
     public class  globalexamlist 

{
         public List<Global_testdata> examlists  { get;set;}


}
        public class Student_Profile
        {
            public int? Regd_ID { get; set; }
            public string Customer_Name { get; set; }
            public string Email { get; set; }
            public string Mobile { get; set; }
            public string Image_Url { get; set; }
            public string Profile_Status { get; set; }
            public int? Board_ID { get; set; }
            public int? Class_ID { get; set; }
            public string Board_Name { get; set; }
            public string Class_Name { get; set; }
            public string testdate { get; set; }
            public string testtime { get; set; }
            public string remain_time { get; set; }
            public int? Exam_ID { get; set; }
            public int? Exam_Type { get; set; }
        }
        public class Global_testdata
        {
            public int? Regd_ID { get; set; }
          
            public Nullable< DateTime> testdate { get; set; }
            public string testtime { get; set; }
           
            public int? Exam_ID { get; set; }
            public int? Exam_Type { get; set; }

            public string Examname { get; set; }
        }


        public class SuccessResponse_Exam
        {                    
            public Offline_success offline_Exam { get; set; }          

            public string Is_Offline { get; set; }
        }
        public class SuccessResponse_Exam1
        {
            public List<Digichamps.Exam_List> Onine_Exam_List { get; set; }        
            public int Online_Exam_Count { get; set; }
            //public List<Offline_exam_Pre_Requisite_test1> Pre_Requisite_test { get; set; }
        }
        public class SuccessResponse_Exam2
        {
            //public List<Digichamps.Exam_List> Onine_Exam_List { get; set; }
            //public int Online_Exam_Count { get; set; }
            public List<Offline_exam_Pre_Requisite_test1> Pre_Requisite_test { get; set; }
        }
        public class Offline_exam_Pre_Requisite_test1
        {
            public int? Exam_ID { get; set; }

            public int? Attempt_nos { get; set; }
            public string Exam_Name { get; set; }
            public Nullable<int> Chapter_Id { get; set; }
            public Nullable<int> Time { get; set; }
            public Nullable<int> Exam_type { get; set; }
            public Nullable<int> Subject_Id { get; set; }
            public Nullable<int> Question_nos { get; set; }
            public Nullable<int> student_attempt { get; set; }
            public int? Participants { get; set; }
        }
        public class SuccessResult_Exam
        {
            public SuccessResponse_Exam success { get; set; }

        }
        public class SuccessResult_Exam1
        {
            public SuccessResponse_Exam1 success { get; set; }

        }
        public class SuccessResult_Exam2
        {
            public SuccessResponse_Exam2 success { get; set; }

        }
        public class Exam_List
        {
            public Nullable<int> Chapter_Id { get; set; }
            public Nullable<int> Board_Id { get; set; }
            public Nullable<int> Class_Id { get; set; }
            public int Exam_ID { get; set; }
            public string Exam_Name { get; set; }
            public Nullable<bool> Is_Global { get; set; }
            public Nullable<int> Question_nos { get; set; }
            public string Subject { get; set; }
            public Nullable<int> Subject_Id { get; set; }
            public Nullable<int> Time { get; set; }
            public Nullable<int> Attempt_nos { get; set; }
            public int student_Attempt { get; set; }
            public int Participants { get; set; }
            public bool is_free { get; set; }

            public Nullable<int> Validity { get; set; }
        }

        public class SuccessResponse_Free_Exam
        {
            public List<Free_Exam_List> Onine_Exam_List { get; set; }
            //public List<Offline_exam_Pre_Requisite_test1> Pre_Requisite_test { get; set; }
        }

        public class Success_freeexam
        {
            public SuccessResponse_Free_Exam success { get; set; }
          
        }
        public class SuccessResponse_Free_Exam1
        {
            //public List<Free_Exam_List> Onine_Exam_List { get; set; }
            public List<Offline_exam_Pre_Requisite_test1> Pre_Requisite_test { get; set; }
        }

        public class Success_freeexam1
        {
            public SuccessResponse_Free_Exam1 success { get; set; }

        }
        public class Free_Exam_List
        {
            public Nullable<int> Chapter_Id { get; set; }
            public Nullable<int> Board_Id { get; set; }
            public Nullable<int> Class_Id { get; set; }
            public int Exam_ID { get; set; }
            public string Exam_Name { get; set; }
            public Nullable<bool> Is_Global { get; set; }
            public Nullable<int> Question_nos { get; set; }
            public string Subject { get; set; }
            public Nullable<int> Subject_Id { get; set; }
            public Nullable<int> Time { get; set; }
            public Nullable<int> Attempt_nos { get; set; }
            public int student_Attempt { get; set; }
            public int Participants { get; set; }
            public bool is_free { get; set; }

            public Nullable<int> Validity { get; set; }
        }

        public class ErrorResponse_Exam
        {
            public string Message { get; set; }
        }

        public class ErrorResult_Exam
        {
            public ErrorResponse_Exam error { get; set; }
        }


        public class successexamstart
        {
            public examstart success { get; set; }
        }

        public class examstart
        {
            public List<examstart_data> ExamData { get; set; }

            public DateTime Start_Time { get; set; }

            public DateTime End_Time { get; set; }

            public int Result_ID { get; set; }
        }

        public class examstart_data
        {
            public Nullable<long> RowID { get; set; }
            public Nullable<int> question_id { get; set; }
            public Nullable<int> Board_Id { get; set; }
            public Nullable<int> Class_Id { get; set; }
            public Nullable<int> Subject_Id { get; set; }
            public Nullable<int> ch_id { get; set; }
            public Nullable<int> topicId { get; set; }
            public Nullable<int> power_id { get; set; }
            public string question { get; set; }
            public string Qustion_Desc { get; set; }
            public List<Question_options> Options { get; set; }
            public List<Question_image> Image { get; set; }
        }

        public class Question_options
        {
            public int Answer_ID { get; set; }
            public Nullable<int> Question_ID { get; set; }
            public string Option_Desc { get; set; }
            public string Option_Image { get; set; }
            public string Answer_desc { get; set; }
            public string Answer_Image { get; set; }
            public Nullable<bool> Is_Answer { get; set; }
        }

        public class Question_image
        {
            public string Question_desc_Image { get; set; }
        }

        public class Offline
        {
            public Offline_success success { get; set; }
        }
        public class Offline_success
        {
            public List<Offline_exam_Pre_Requisite_test> Pre_Requisite_test { get; set; }
            public List<Offline_exam_Practice> Practice { get; set; }
            public List<Offline_exam_Re_Test> Re_Test { get; set; }

            public int Total_Requisite_test { get; set; }

            public int Total_Practice_Test { get; set; }

            public int Total_Retest { get; set; }
        }
        public class Offline_exam_Pre_Requisite_test
        {
            public int Exam_ID { get; set; }

            public int Attempt_nos { get; set; }
            public string Exam_Name { get; set; }
            public Nullable<int> Chapter_Id { get; set; }
            public string Chapter { get; set; }
            public Nullable<int> Time { get; set; }
            public Nullable<int> Exam_type { get; set; }
            public Nullable<int> Subject_Id { get; set; }
            public string Subject { get; set; }
            public Nullable<int> Question_nos { get; set; }
            public Nullable<int> max_attempt { get; set; }
            public int Participants { get; set; }
        }

        public class Offline_exam_Practice
        {
            public int Exam_ID { get; set; }
            public Nullable<int> Regd_ID { get; set; }
            public int Attempt_nos { get; set; }
            public string Exam_Name { get; set; }
            public Nullable<int> Chapter_Id { get; set; }
            public string Chapter { get; set; }
            public Nullable<int> Time { get; set; }
            public Nullable<int> Exam_type { get; set; }
            public Nullable<int> Subject_Id { get; set; }
            public string Subject { get; set; }
            public Nullable<int> Question_nos { get; set; }
            public Nullable<int> max_attempt { get; set; }
            public int Participants { get; set; }
        }

        public class Offline_exam_Re_Test
        {
            public int Exam_ID { get; set; }
            public Nullable<int> Regd_ID { get; set; }
            public int Attempt_nos { get; set; }
            public string Exam_Name { get; set; }
            public Nullable<int> Chapter_Id { get; set; }
            public string Chapter { get; set; }
            public Nullable<int> Time { get; set; }
            public Nullable<int> Exam_type { get; set; }
            public Nullable<int> Subject_Id { get; set; }
            public string Subject { get; set; }
            public Nullable<int> Question_nos { get; set; }
            public Nullable<int> max_attempt { get; set; }
            public int Participants { get; set; }
        }
        public class Is_Offline_Result
        {
            public bool? Is_offline { get; set; }
        }

        public class Status_Message
        {
            public bool? Status { get; set; }
            public int? Regd_Id { get; set; }
        }
        public class Status_Response
        {
            public Status_Message Success { get; set; }
        }


        public class security_confirmation
        {

            public Nullable<int> Regd_ID { get; set; }
           
            public string Otp { get; set; }

        }
    }
    
}