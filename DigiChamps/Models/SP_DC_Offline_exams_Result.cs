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
    
    public partial class SP_DC_Offline_exams_Result
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
}
