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
    
    public partial class Vw_Attendance_Report
    {
        public int Attendance_Id { get; set; }
        public Nullable<System.DateTime> Attendance_Date { get; set; }
        public Nullable<bool> Is_Present { get; set; }
        public string Class_Name { get; set; }
        public string Batch_Name { get; set; }
        public int Regd_ID { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
        public string Customer_Name { get; set; }
        public string Praent_Name { get; set; }
        public string Parent_Mail { get; set; }
        public string Parent_Mobile { get; set; }
        public string P_Relation { get; set; }
        public Nullable<int> Parent_Id { get; set; }
        public string Batch_From_Time { get; set; }
        public string Batch_To_Time { get; set; }
        public string Teacher_Name { get; set; }
    }
}
