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
    
    public partial class View_DC_Exam_Setup
    {
        public int Exam_ID { get; set; }
        public Nullable<int> Board_Id { get; set; }
        public Nullable<int> Class_Id { get; set; }
        public Nullable<int> Subject_Id { get; set; }
        public Nullable<int> Chapter_Id { get; set; }
        public string Chapter { get; set; }
        public Nullable<int> Validity { get; set; }
        public Nullable<int> Question_nos { get; set; }
        public Nullable<int> Time { get; set; }
        public Nullable<bool> Is_Global { get; set; }
        public string Subject { get; set; }
        public string Board_Name { get; set; }
        public string Class_Name { get; set; }
        public string Exam_Name { get; set; }
        public Nullable<int> Attempt_nos { get; set; }
    }
}
