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
    public class TestResult
    {
        public int Regd_ID { get; set; }
        public string Customer_Name { get; set; }

        public int Batch_Id { get; set; }
        public string Batch_Name { get; set; }

        public int Batch_Assign_Id { get; set; }
        public Nullable<int> Teach_ID { get; set; }
        public long Id { get; set; }

        public int Exam_ID { get; set; }
        public string Exam_Name { get; set; }
        public int Result_ID { get; set; }

        public Nullable<int> Board_Id { get; set; }
        public string Board_Name { get; set; }
        public Nullable<int> Class_Id { get; set; }
        public string Class_Name { get; set; }
        public Nullable<int> Subject_Id { get; set; }
        public string Subject { get; set; }

        public Nullable<System.DateTime> StartTime { get; set; }
        public Nullable<System.DateTime> EndTime { get; set; }
        public Nullable<int> Question_Nos { get; set; }
        public Nullable<int> Question_Attempted { get; set; }
        public Nullable<int> Total_Correct_Ans { get; set; }
        public string Chapter { get; set; }
        public int ResultdtL_ID { get; set; }
        public Nullable<int> Question_ID { get; set; }
        public string Question { get; set; }
        public string Qustion_Desc { get; set; }
        public string Answer_Desc { get; set; }
        public string Answer_Image { get; set; }
        public Nullable<int> Answer_ID { get; set; }
        public string Option_Desc { get; set; }
        public string Option_Image { get; set; }
        public Nullable<bool> Is_Correct { get; set; }
        public string Inserted_By { get; set; }
        public Nullable<System.DateTime> Modified_Date { get; set; }
        public string Modified_By { get; set; }
        public Nullable<bool> Is_Active { get; set; }
        public Nullable<bool> Is_Deleted { get; set; }
    }
}