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
    
    public partial class tbl_DC_Course_Teacher_Assign
    {
        public int CourseAssn_ID { get; set; }
        public Nullable<int> Board_Id { get; set; }
        public Nullable<int> Class_Id { get; set; }
        public Nullable<int> Subject_Id { get; set; }
        public Nullable<int> Teacher_ID { get; set; }
        public Nullable<System.DateTime> Inserted_Date { get; set; }
        public string Inserted_By { get; set; }
        public Nullable<System.DateTime> Modified_Date { get; set; }
        public string Modified_By { get; set; }
        public Nullable<bool> Is_Active { get; set; }
        public Nullable<bool> Is_Deleted { get; set; }
    
        public virtual tbl_DC_Board tbl_DC_Board { get; set; }
        public virtual tbl_DC_Class tbl_DC_Class { get; set; }
        public virtual tbl_DC_Subject tbl_DC_Subject { get; set; }
        public virtual tbl_DC_Teacher tbl_DC_Teacher { get; set; }
    }
}
