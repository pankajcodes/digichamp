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
    
    public partial class tbl_DC_Exam_Result_Dtl
    {
        public int ResultdtL_ID { get; set; }
        public Nullable<int> Result_ID { get; set; }
        public Nullable<int> Question_ID { get; set; }
        public Nullable<int> Board_Id { get; set; }
        public Nullable<int> Class_Id { get; set; }
        public Nullable<int> Subject_Id { get; set; }
        public Nullable<int> Chapter_Id { get; set; }
        public Nullable<int> Topic_ID { get; set; }
        public Nullable<int> Power_ID { get; set; }
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
