using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.Entity;
using DigiChamps.Models;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Web.Security;
using System.Text;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;

namespace DigiChamps.Models
{
    public class DigiChampsModel
    {
        //Indian DateTime to server
        public static DateTime datetoserver()
        {
            string zoneId = "India Standard Time";
            TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById(zoneId);
            DateTime result = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tzi);
            return result;
        }

        public class DigiChampsBoardModel
        {
            public int Board_Id { get; set; }
            [Required(ErrorMessage = "Please enter board name")]
            public string Board_Name { get; set; }
            public Nullable<System.DateTime> Inserted_Date { get; set; }
            public Nullable<int> Inserted_By { get; set; }
            public Nullable<System.DateTime> Modified_Date { get; set; }
            public Nullable<int> Modified_By { get; set; }
            public Nullable<bool> Is_Active { get; set; }
            public Nullable<bool> Is_Deleted { get; set; }
        }
        public class DigiChampsClassModel
        {
            public int Class_Id { get; set; }
            [Required(ErrorMessage = "Please select board")]
            public int Board_Id { get; set; }
            [Required(ErrorMessage = "Please select class")]
            public string Class_Name { get; set; }
            public Nullable<System.DateTime> Inserted_Date { get; set; }
            public Nullable<int> Inserted_By { get; set; }
            public Nullable<System.DateTime> Modified_Date { get; set; }
            public Nullable<int> Modified_By { get; set; }
            public Nullable<bool> Is_Active { get; set; }
            public Nullable<bool> Is_Deleted { get; set; }

            public string Board_Name { get; set; }
            public IList<SelectListItem> ClsName { get; set; }
        }
        public class DigiChampsSubjectModel
        {
            public int Subject_Id { get; set; }
            [Required(ErrorMessage = "Please select class ")]
            public int Class_Id { get; set; }
            [Required(ErrorMessage = "Please enter subject")]
            public string Subject { get; set; }
            public Nullable<System.DateTime> Inserted_Date { get; set; }
            public Nullable<int> Inserted_By { get; set; }
            public Nullable<System.DateTime> Modified_Date { get; set; }
            public Nullable<int> Modified_By { get; set; }
            public Nullable<bool> Is_Active { get; set; }
            public Nullable<bool> Is_Deleted { get; set; }


            [Required(ErrorMessage = "Please enter board ")]
            public Nullable<int> Board_Id { get; set; }
            public string Board_Name { get; set; }
            public string Class_Name { get; set; }
            public IList<SelectListItem> BoardNames { get; set; }
            public IList<SelectListItem> ClassNames { get; set; }
        }
        public class DigiChampsChapterModel
        {
            public HttpPostedFileBase MyFile { get; set; }
            public string CroppedImagePath { get; set; }
            public int Chapter_Id { get; set; }
            [Required(ErrorMessage = "Please select subject ")]
            public int Subject_Id { get; set; }
            [Required(ErrorMessage = "Please enter chapter name")]
            public string Chapter { get; set; }
            public Nullable<System.DateTime> Inserted_Date { get; set; }
            public Nullable<int> Inserted_By { get; set; }
            public Nullable<System.DateTime> Modified_Date { get; set; }
            public Nullable<int> Modified_By { get; set; }
            public Nullable<bool> Is_Active { get; set; }
            public Nullable<bool> Is_Deleted { get; set; }

            [Required(ErrorMessage = "Please select board")]
            public int Board_Id { get; set; }
            [Required(ErrorMessage = "Please select class")]
            public int Class_Id { get; set; }

            public string Board_Name { get; set; }

            public string Class_Name { get; set; }

            public string Subject { get; set; }

            public IList<SelectListItem> BoardNamess { get; set; }
            public IList<SelectListItem> ClassNamess { get; set; }

            public int Tablet_Id { get; set; }
            public string Tablet_Name { get; set; }
        }
        public class DigiChampsPowerModel
        {
            public Nullable<int> Power_Id { get; set; }
            public Nullable<int> Exam_Id { get; set; }
            [Required(ErrorMessage = "Please enter power type")]
            public string Power_Type { get; set; }
            public Nullable<int> no_of_ques { get; set; }
            public Nullable<System.DateTime> Inserted_Date { get; set; }
            public string Inserted_By { get; set; }
            public Nullable<System.DateTime> Modified_Date { get; set; }
            public string Modified_By { get; set; }
            public Nullable<bool> Is_Active { get; set; }
            public Nullable<bool> Is_Deleted { get; set; }



        }
        public class DigiChampsShiftModel
        {
            public Nullable<int> ShiftMst_ID { get; set; }
            [Required(ErrorMessage = "Please select shift name")]
            public string Shift_Name { get; set; }
            [Required(ErrorMessage = "Please select board")]
            public string Shift_From_Time { get; set; }
            public string Shift_To_Time { get; set; }
            public Nullable<System.DateTime> Inserted_Date { get; set; }
            public string Inserted_By { get; set; }
            public Nullable<System.DateTime> Modified_Date { get; set; }
            public string Modified_By { get; set; }
            public Nullable<bool> Is_Active { get; set; }
            public Nullable<bool> Is_Deleted { get; set; }

            //For Shift Report
            public Nullable<int> Shift_T_Id { get; set; }
            public Nullable<int> Teacher_Id { get; set; }
            public string Shift_days { get; set; }
            public Nullable<System.DateTime> effective_date { get; set; }

        }
        public class DigiChampTicketModel
        {
            public int Ticket_ID { get; set; }
            public string Ticket_No { get; set; }
            public Nullable<int> Student_ID { get; set; }
            public Nullable<int> Board_ID { get; set; }
            public Nullable<int> Class_ID { get; set; }
            public Nullable<int> Subject_ID { get; set; }
            public Nullable<int> Chapter_ID { get; set; }
            public string Question { get; set; }
            public string Question_Image { get; set; }
            public Nullable<bool> Status { get; set; }
            public Nullable<int> Inserted_By { get; set; }
            public Nullable<System.DateTime> Inserted_Date { get; set; }
            public Nullable<int> Modified_By { get; set; }
            public Nullable<System.DateTime> Modified_Date { get; set; }
            public Nullable<bool> Is_Active { get; set; }
            public Nullable<bool> Is_Deleted { get; set; }
            public string Student_name { get; set; }
            public string Class_Name { get; set; }
        }
        public class DigiChampsSMTPConfigModel
        {
            public int SMTP_ID { get; set; }
            [Required(ErrorMessage = "Please enter sender name")]
            public string SMTP_Sender { get; set; }
            [Required(ErrorMessage = "Please enter host name")]
            public string SMTP_HostName { get; set; }
            [RegularExpression(@"^([a-zA-Z0-9_\.\-])+\@(([a-zA-Z0-9\-])+\.)+([a-zA-Z0-9]{2,4})+$",
            ErrorMessage = "Please Enter Correct Email Address")]
            [Required(ErrorMessage = "Please enter user name")]
            public string SMTP_User { get; set; }
            [Required(ErrorMessage = "Please enter password")]
            public string SMTP_Pwd { get; set; }
            [Required(ErrorMessage = "Please enter port")]
            public Nullable<int> SMTP_Port { get; set; }
            public bool? SMTP_Ssl { get; set; }
        }
        public class DigiChampsTeachershiftModel
        {
            public int? Shift_teach_ID { get; set; }
            public int? ShiftMst_ID { get; set; }
            public string Shift_Days { get; set; }
            public int? Teacher_ID { get; set; }
            public string Teacher_names { get; set; }
            public string shiftinword { get; set; }
            public DateTime? Efective_Date { get; set; }
            public DateTime? Inserted_Date { get; set; }
            public string Inserted_By { get; set; }
            public DateTime? Modified_Date { get; set; }
            public string Modified_By { get; set; }
            public Nullable<bool> Is_Active { get; set; }
            public Nullable<bool> Is_Deleted { get; set; }
        }
        public class DigiChampsMAILConfigModel
        {
            public int EmailConf_ID { get; set; }
            [Required(ErrorMessage = "Select alertname")]
            public int? EmailConf_AlertName { get; set; }
            public string EmailConf_Body { get; set; }
            public string EmailConf_Alert { get; set; }
            public DateTime Inserted_Date { get; set; }
            public string Inserted_By { get; set; }
            public DateTime Modified_Date { get; set; }
            public string Modified_By { get; set; }
            public Nullable<bool> Is_Active { get; set; }
            public Nullable<bool> Is_Deleted { get; set; }
            public string Subject { get; set; }
            public Nullable<int> SMTP_ID { get; set; }
            public string Email_Subject { get; set; }
            public string SMTP_Sender { get; set; }
            public int EmailType_ID { get; set; }
            public string Email_AlertName { get; set; }
            public string Email_Alert { get; set; }
        }
        public class DigiChampsMAILTypeModel
        {
            public int EmailType_ID { get; set; }
            [Required]
            public string Email_AlertName { get; set; }
            public DateTime Inserted_Date { get; set; }
            public string Inserted_By { get; set; }
            public DateTime Modified_Date { get; set; }
            public string Modified_By { get; set; }
            public Nullable<bool> Is_Active { get; set; }
            public Nullable<bool> Is_Deleted { get; set; }
            [Required]
            public string Email_Alert { get; set; }
        }
        public class DigiChampsTeacherRegModel
        {
            public HttpPostedFileBase MyFile { get; set; }
            public string CroppedImagePath { get; set; }
            public int Teach_ID { get; set; }
            [Required(ErrorMessage = "Please enter teacher name")]
            public string Teacher_Name { get; set; }
            [Required(ErrorMessage = "Please enter designation")]
            public string Designation { get; set; }
            [DataType(DataType.Date)]
            public Nullable<DateTime> DateOfBirth { get; set; }
            [Required(ErrorMessage = "Select gender")]
            public string Gender { get; set; }
            [Required(ErrorMessage = "Please enter emailid")]
            [DataType(DataType.EmailAddress, ErrorMessage = "E-mail is not valid")]
            public string Email_ID { get; set; }
            [Required(ErrorMessage = "Please enter mobile number")]
            public string Mobile { get; set; }
            [Required(ErrorMessage = "Please enter address")]
            public string Address { get; set; }
            public string Image { get; set; }
            public DateTime Inserted_Date { get; set; }
            public string Inserted_By { get; set; }
            public DateTime Modified_Date { get; set; }
            public string Modified_By { get; set; }
            public Nullable<bool> Is_Active { get; set; }
            public Nullable<bool> Is_Deleted { get; set; }
        }
        public class DigiChampsTopicModel
        {
            [Required(ErrorMessage = "Please select board")]
            public int Board_ID { get; set; }
            [Required(ErrorMessage = "Please select class")]
            public int Class_ID { get; set; }
            [Required(ErrorMessage = "Please select subject")]
            public Nullable<int> Subject_ID { get; set; }
            [Required(ErrorMessage = "Please enter topic name")]
            public string Topic_Name { get; set; }
            public string Topic_Desc { get; set; }
            public DateTime Inserted_Date { get; set; }
            public string Inserted_By { get; set; }
            public DateTime Modified_Date { get; set; }
            public string Modified_By { get; set; }
            public bool Is_Active { get; set; }
            public bool Is_Deleted { get; set; }

            public int Topic_ID { get; set; }
            public int Chapter_ID { get; set; }
            public string chapter_Name { get; set; }
            public string Board_Name { get; set; }

            public string Class_Name { get; set; }
            public string Subject { get; set; }

            public IList<SelectListItem> BoardNamess { get; set; }
            public IList<SelectListItem> ClassNamess { get; set; }
            public IList<SelectListItem> SubjectNamess { get; set; }
        }
        public class DigiChampsModuleModel
        {
            [Required(ErrorMessage = "Please select chapter ")]
            public Nullable<int> Chapter_Id { get; set; }
            [Required(ErrorMessage = "Please select subject ")]
            public Nullable<int> Subject_Id { get; set; }
            public string Board_Name { get; set; }
            public string Class_Name { get; set; }
            public Nullable<System.DateTime> Inserted_Date { get; set; }
            public Nullable<int> Inserted_By { get; set; }
            public Nullable<System.DateTime> Modified_Date { get; set; }
            public Nullable<int> Modified_By { get; set; }
            public Nullable<bool> Is_Active { get; set; }
            public Nullable<bool> Is_Deleted { get; set; }

            [Required(ErrorMessage = "Please select board")]
            public int Board_Id { get; set; }
            [Required(ErrorMessage = "Please select class")]
            public int Class_Id { get; set; }

            public string Subject { get; set; }
            public string Chapter { get; set; }
            public Nullable<int> Chapterlimit { get; set; }
            public Nullable<int> Module_ID { get; set; }
            [Required(ErrorMessage = "Please enter module name")]
            public string Module_Name { get; set; }
            public string Module_Desc { get; set; }
            public string Module_video { get; set; }
            public string Module_Content { get; set; }
            public Nullable<bool> Is_Free { get; set; }
            public List<DigiChampsModuleModel> subdtls { get; set; }
            public List<SP_chapterList_Result> subdtls1 { get; set; }
            public string Module_Image { get; set; }
            public Nullable<int> Validity { get; set; }
            public string Question_PDF { get; set; }
            public Nullable<bool> Is_Free_Test { get; set; }
            public Nullable<int> No_Question { get; set; }
            public string Upload_PDF { get; set; }
            public string Question_PDF_Name { get; set; }
            public static List<DigiChampsModel.DigiChampsModuleModel> Chapter1 = null;
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
        public class RoleTypeModel
        {
            public int Role_Type_ID { get; set; }
            public string Role_Type_Name { get; set; }
            public string Inserted_By { get; set; }
            public Nullable<System.DateTime> Inserted_Date { get; set; }
            public string Modified_By { get; set; }
            public Nullable<System.DateTime> Modified_Date { get; set; }
            public Nullable<bool> Is_Active { get; set; }
            public Nullable<bool> Is_Deleted { get; set; }
        }
        public class MenuItemModel
        {
            public int Menu_Key_ID { get; set; }
            public string Menu_Parameter1 { get; set; }
            public string Menu_Parameter2 { get; set; }
            public string Menu_Parameter3 { get; set; }
            public string Inserted_By { get; set; }
            public Nullable<System.DateTime> Inserted_Date { get; set; }
            public string Modified_By { get; set; }
            public Nullable<System.DateTime> Modified_Date { get; set; }
            public Nullable<bool> Is_Active { get; set; }
            public Nullable<bool> Is_Deleted { get; set; }
        }
        public class MenuPermissionModel
        {
            public int Permission_ID { get; set; }
            public Nullable<int> ROLE_ID { get; set; }
            public Nullable<int> Role_Type_ID { get; set; }
            public Nullable<int> Menu_Key_ID { get; set; }
            public string Inserted_By { get; set; }
            public Nullable<System.DateTime> Inserted_Date { get; set; }
            public string Modified_By { get; set; }
            public Nullable<System.DateTime> Modified_Date { get; set; }
            public Nullable<bool> Is_Active { get; set; }
            public Nullable<bool> Is_Deleted { get; set; }

            public string ROLE_TYPE { get; set; }

            public string Role_Type_Name { get; set; }
            public string Menu_Parameter1 { get; set; }
            public string Menu_Parameter2 { get; set; }
            public string Menu_Parameter3 { get; set; }

            public string ROLE_CODE { get; set; }

        }
        public class DigiChampsexamModel
        {
            public string Exam_ID { get; set; }
            [Required(ErrorMessage = "Please select board ")]
            public int Board_Id { get; set; }
            [Required(ErrorMessage = "Please select class ")]
            public int Class_Id { get; set; }
            [Required(ErrorMessage = "Please select subject ")]
            public int Subject_Id { get; set; }
            [Required(ErrorMessage = "Please select Chapter ")]
            public Nullable<int> Chapter_Id { get; set; }

            public int Question_nos { get; set; }
            public int Time { get; set; }
            public Nullable<bool> Is_Global { get; set; }

            public Nullable<int> Inserted_By { get; set; }
            public Nullable<System.DateTime> Modified_Date { get; set; }
            public Nullable<int> Modified_By { get; set; }
            public Nullable<bool> Is_Active { get; set; }
            public Nullable<bool> Is_Deleted { get; set; }

            public string Board_Name { get; set; }
            public string Class_Name { get; set; }
            public string Subject { get; set; }
            public string Chapter { get; set; }
        }
        public class DigiChampsTaxTypeMasterModel
        {
            public int TaxType_ID { get; set; }
            [Required(ErrorMessage = "Please enter tax type")]
            public string Tax_Type { get; set; }
            [Required(ErrorMessage = "Please enter tax short name")]
            public string Tax_Type_Short { get; set; }
            [Required(ErrorMessage = "Please enter tax code")]
            public string TAX_CODE { get; set; }
            public Nullable<System.DateTime> Inserted_Date { get; set; }
            public string Inserted_By { get; set; }
            public string Modified_By { get; set; }
            public Nullable<System.DateTime> Modified_Date { get; set; }
            public Nullable<bool> Is_Active { get; set; }
            public Nullable<bool> Is_Deleted { get; set; }
        }
        public class DigiChampsTaxMasterModel
        {
            public int Tax_ID { get; set; }
            [Required(ErrorMessage = "Please select tax type")]
            public Nullable<int> TaxType_ID { get; set; }
            [Required(ErrorMessage = "Please enter tax rate")]
            public Nullable<decimal> Tax_Rate { get; set; }
            public Nullable<System.DateTime> TAX_Efect_Date { get; set; }
            public Nullable<System.DateTime> Inserted_Date { get; set; }
            public string Inserted_By { get; set; }
            public string Modified_By { get; set; }
            public Nullable<System.DateTime> Modified_Date { get; set; }
            public Nullable<bool> Is_Active { get; set; }
            public Nullable<bool> Is_Deleted { get; set; }

            public string Tax_Type { get; set; }
        }
        public partial class View_All_Students
        {
            public HttpPostedFileBase MyFile { get; set; }
            public string CroppedImagePath { get; set; }
            public int Regd_ID { get; set; }
            public string Regd_No { get; set; }
            public string Customer_Name { get; set; }
            public string Mobile { get; set; }
            public string Email { get; set; }
            public string Board_Name { get; set; }
            public string Class_Name { get; set; }
            public Nullable<int> Board_Id { get; set; }
            public Nullable<int> Class_ID { get; set; }
            public string STATUS { get; set; }
            public string Phone { get; set; }
            public string Organisation_Name { get; set; }
            public string Pincode { get; set; }
            public string Address { get; set; }
            public string Image { get; set; }

            public int Parent_Id { get; set; }
            public string Praent_Name { get; set; }
            public string Parent_Mail { get; set; }
            public string Parent_Mobile { get; set; }
            public string P_Relation { get; set; }
        }

        public class DigiChampsDailySalesModel
        {
            public Nullable<int> Package_ID { get; set; }
            public int Regd_ID { get; set; }
            public int Order_ID { get; set; }
            public string Order_No { get; set; }
            public int USER_CODE { get; set; }
            public string Package_Name { get; set; }
            public Nullable<decimal> Price { get; set; }
            public string Customer_Name { get; set; }
            public DateTime From_Date { get; set; }
            public DateTime To_date { get; set; }
            public DateTime? Inserted_Date { get; set; }
            public string Bank_Name { get; set; }
            public string Branch_Name { get; set; }
            public string Address_Details { get; set; }
            public DateTime Cheque_Date { get; set; }
            public string Cheque_No { get; set; }

        }
        public class DigiChampsQuestionMasterModel
        {
            public int Question_ID { get; set; }
            public int Ques_ID { get; set; }
            public string Exam_Name { get; set; }
            public Nullable<int> Board_Id { get; set; }
            public Nullable<int> Regd_Id { get; set; }
            public Nullable<int> Class_Id { get; set; }
            public Nullable<int> Subject_Id { get; set; }
            public Nullable<int> Chapter_Id { get; set; }
            public Nullable<int> Topic_ID { get; set; }
            public Nullable<int> Power_ID { get; set; }
            public string Question { get; set; }
            public string Qustion_Desc { get; set; }
            public string Answer_Desc { get; set; }
            public string Answer_Image { get; set; }
            public string Inserted_By { get; set; }
            public int Answer_ID { get; set; }
            public Nullable<int> Question_nos { get; set; }
            public string Option_Desc { get; set; }
            public string Option_Image { get; set; }
            public Nullable<System.DateTime> Modified_Date { get; set; }
            public string Modified_By { get; set; }
            public Nullable<bool> Is_Active { get; set; }
            public Nullable<bool> Is_Deleted { get; set; }
            public List<SP_DC_Procedurefor_test_Result> quesdtls { get; set; }
            public virtual tbl_DC_Board tbl_DC_Board { get; set; }
            public virtual tbl_DC_Chapter tbl_DC_Chapter { get; set; }
            public virtual tbl_DC_Class tbl_DC_Class { get; set; }
            public virtual tbl_DC_Power_Question tbl_DC_Power_Question { get; set; }
            public virtual tbl_DC_Subject tbl_DC_Subject { get; set; }
        }


        public class SP_DC_GetExamwisequestions_Result1
        {
            public Nullable<long> RowID { get; set; }
            public int Question_ID { get; set; }
            public string Question { get; set; }
            public Nullable<int> Chapter_Id { get; set; }
            public Nullable<int> Subject_Id { get; set; }
            public string Exam_Name { get; set; }
            public string Qustion_Desc { get; set; }

            public List<DigiChampsModel.SP_DC_GetExamwisequestions_Result1> quesdtls { get; set; }
        }

        public class percentagecls
        {
            public int Percentage { get; set; }
            public string message { get; set; }
        }

        public class feedback_model
        {
            public Nullable<int> Chapter_Id { get; set; }
            public Nullable<int> Teach_ID { get; set; }
        }

        public class DigiChampsCentreModel
        {
            public int Centre_Id { get; set; }
            [Required(ErrorMessage = "Please enter centre name")]
            public string Centre_Name { get; set; }
            [Required(ErrorMessage = "Please enter centre code")]
            public string Centre_Code { get; set; }
            [Required(ErrorMessage = "Please enter address")]
            public string Address_Line_1 { get; set; }
            [Required(ErrorMessage = "Please enter reference address")]
            public string Address_Line_2 { get; set; }
            [Required(ErrorMessage = "Please select state")]
            public Nullable<int> State_Id { get; set; }
            [Required(ErrorMessage = "Please select city")]
            public Nullable<int> City_Id { get; set; }
            [Required(ErrorMessage = "Please enter pin code")]
            public string Pin_Code { get; set; }
            public Nullable<System.DateTime> Inserted_Date { get; set; }
            public Nullable<int> Inserted_By { get; set; }
            public Nullable<System.DateTime> Modified_Date { get; set; }
            public Nullable<int> Modified_By { get; set; }
            public Nullable<bool> Is_Active { get; set; }
            public Nullable<bool> Is_Deleted { get; set; }

            public string State_Name { get; set; }
            public string City_Name { get; set; }

            public IList<SelectListItem> CityNames { get; set; }
        }

        public class DigiChampsBatchModel
        {
            public Nullable<int> Batch_Id { get; set; }
            [Required(ErrorMessage = "Please select batch name")]
            public string Batch_Name { get; set; }
            [Required(ErrorMessage = "Please select from time")]
            public string Batch_From_Time { get; set; }
            [Required(ErrorMessage = "Please select to time")]
            public string Batch_To_Time { get; set; }
            [Required(ErrorMessage = "Please select batch days")]
            public string Batch_Days { get; set; }
            [Required(ErrorMessage = "Please select batch code")]
            public string Batch_Code { get; set; }
            public Nullable<System.DateTime> Inserted_Date { get; set; }
            public string Inserted_By { get; set; }
            public Nullable<System.DateTime> Modified_Date { get; set; }
            public string Modified_By { get; set; }
            public Nullable<bool> Is_Active { get; set; }
            public Nullable<bool> Is_Deleted { get; set; }

            public Nullable<int> Centre_Id { get; set; }
            public string Centre_Name { get; set; }

            public Nullable<int> Class_Id { get; set; }
            public string Class_Name { get; set; }

            public Nullable<int> Subject_Id { get; set; }
            public string Subject { get; set; }

            public Nullable<int> Board_Id { get; set; }
            public string Board_Name { get; set; }

        }

        public class DigiChampsAssignBatchModel
        {
            public Nullable<int> Batch_Assign_Id { get; set; }
            public Nullable<int> Batch_Id { get; set; }
            [Required(ErrorMessage = "Please select batch name")]
            public string Batch_Name { get; set; }
            [Required(ErrorMessage = "Please select batch code")]
            public string Batch_Code { get; set; }
            public Nullable<System.DateTime> Inserted_Date { get; set; }
            public string Inserted_By { get; set; }
            public Nullable<System.DateTime> Modified_Date { get; set; }
            public string Modified_By { get; set; }
            public Nullable<bool> Is_Active { get; set; }
            public Nullable<bool> Is_Deleted { get; set; }

            public Nullable<int> Regd_ID { get; set; }
            public string Customer_Name { get; set; }
            public string Mobile { get; set; }
            public string Email { get; set; }

            public Nullable<int> Teach_ID { get; set; }
            public string Teacher_Name { get; set; }

            public Nullable<int> Class_Id { get; set; }
            public string Class_Name { get; set; }

            public Nullable<int> Subject_Id { get; set; }
            public string Subject { get; set; }

            public Nullable<int> Board_Id { get; set; }
            public string Board_Name { get; set; }
            public bool Is_present { get; set; }
            public Nullable<int> Order_Id { get; set; }
            public Nullable<int> Package_Id { get; set; }

        }

        public class state_entry
        {

            public string statename { get; set; }
            public Nullable<int> stateid { get; set; }
            public Nullable<System.DateTime> inserted_date { get; set; }
            public int inserted_by { get; set; }
            public bool is_active { get; set; }
            public bool id_deleted { get; set; }

            public Nullable<int> id { get; set; }

            public string c_name { get; set; }

        }

        public class TaxModel
        {
            public int TaxType_ID { get; set; }
            public string Tax_Type { get; set; }
            public string Tax_Type_Short { get; set; }
            public string TAX_CODE { get; set; }
            public int Tax_ID { get; set; }
            public Nullable<decimal> Tax_Rate { get; set; }
        }

        public class Security_Question
        {
            public int Question_ID { get; set; }
            public string Question { get; set; }
        }
        public class DigichampsSampleImage
        {
            public Nullable<int> Sample_Img_Id { get; set; }
            public string Image_Type { get; set; }
            public string Image_URL { get; set; }
            public string Image_Title { get; set; }
            public Nullable<System.DateTime> Inserted_Date { get; set; }
            public Nullable<int> Inserted_By { get; set; }
            public Nullable<System.DateTime> Modified_Date { get; set; }
            public Nullable<int> Modified_By { get; set; }
            public Nullable<bool> Is_Active { get; set; }
            public Nullable<bool> Is_Deleted { get; set; }

        }
        public class DigichampsOurTeam
        {
            public Nullable<int> Team_Id { get; set; }
            public string Name { get; set; }
            public string Designation { get; set; }
            public string Image { get; set; }
            public string Description { get; set; }
            public Nullable<System.DateTime> Inserted_Date { get; set; }
            public Nullable<int> Inserted_By { get; set; }
            public Nullable<System.DateTime> Modified_Date { get; set; }
            public Nullable<int> Modified_By { get; set; }
            public Nullable<bool> Is_Active { get; set; }
            public Nullable<bool> Is_Deleted { get; set; }

        }
        public class DigichampsMarketingBlog
        {
            public Nullable<int> Blog_Id { get; set; }
            public string Name { get; set; }
            public string Designation { get; set; }
            public string Image { get; set; }
            public string Blog_Image { get; set; }
            public string Description { get; set; }
            public Nullable<System.DateTime> Inserted_Date { get; set; }
            public Nullable<int> Inserted_By { get; set; }
            public Nullable<System.DateTime> Modified_Date { get; set; }
            public Nullable<int> Modified_By { get; set; }
            public Nullable<bool> Is_Active { get; set; }
            public Nullable<bool> Is_Deleted { get; set; }

        }
        public class DigichampsCareer
        {
            public Nullable<int> Career_ID { get; set; }
            public string career_Name { get; set; }
            public int? No_of_vacancy { get; set; }
            public string Experience { get; set; }
            public string Location { get; set; }
            public string Qualification { get; set; }
            [DataType(DataType.DateTime)]
            [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
            public DateTime? Opening_Date { get; set; }
            public DateTime? Close_Date { get; set; }
            public string Walk_in_Time { get; set; }
            public string Phone { get; set; }
            public string Job_Description { get; set; }
            // public Nullable<System.DateTime> Inserted_Date { get; set; }
            public string Inserted_By { get; set; }
            public Nullable<System.DateTime> Modified_Date { get; set; }
            public Nullable<int> Modified_By { get; set; }
            public Nullable<bool> Is_Active { get; set; }
            public Nullable<bool> Is_Deleted { get; set; }
        }

        public class Digichampsfaq
        {
            public Nullable<int> FAQs_ID { get; set; }
            public string FAQ { get; set; }
            public string FAQ_Answer { get; set; }
            public string Inserted_By { get; set; }
            public Nullable<System.DateTime> Modified_Date { get; set; }
            public Nullable<int> Modified_By { get; set; }
            public Nullable<bool> Is_Active { get; set; }
            public Nullable<bool> Is_Deleted { get; set; }
        }
        public class DigichampsBanner
        {
            public Nullable<int> banner_Id { get; set; }

            public string Image_URL { get; set; }
            public string Image_Title { get; set; }
            public Nullable<System.DateTime> Inserted_Date { get; set; }
            public Nullable<int> Inserted_By { get; set; }
            public Nullable<System.DateTime> Modified_Date { get; set; }
            public Nullable<int> Modified_By { get; set; }
            public Nullable<bool> Is_Active { get; set; }
            public Nullable<bool> Is_Deleted { get; set; }

        }
    }
}