//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DigiChamps.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class tbl_DC_Exam_Result
    {
        public int Result_ID { get; set; }
        public Nullable<int> Regd_ID { get; set; }
        public string Regd_No { get; set; }
        public Nullable<int> Exam_ID { get; set; }
        public string Exam_Name { get; set; }
        public Nullable<int> Board_Id { get; set; }
        public string Board_Name { get; set; }
        public Nullable<int> Class_Id { get; set; }
        public string Class_Name { get; set; }
        public Nullable<int> Subject_Id { get; set; }
        public string Subject_Name { get; set; }
        public Nullable<int> Chapter_Id { get; set; }
        public string Chapter_Name { get; set; }
        public Nullable<System.DateTime> StartTime { get; set; }
        public Nullable<System.DateTime> EndTime { get; set; }
        public Nullable<int> Question_Nos { get; set; }
        public Nullable<int> Question_Attempted { get; set; }
        public Nullable<int> Total_Correct_Ans { get; set; }
        public Nullable<bool> Is_Global { get; set; }
        public string Inserted_By { get; set; }
        public Nullable<System.DateTime> Modified_Date { get; set; }
        public string Modified_By { get; set; }
        public Nullable<bool> Is_Active { get; set; }
        public Nullable<bool> Is_Deleted { get; set; }
    }
}
