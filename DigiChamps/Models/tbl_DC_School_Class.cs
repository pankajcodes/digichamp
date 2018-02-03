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
    
    public partial class tbl_DC_School_Class
    {
        public tbl_DC_School_Class()
        {
            this.tbl_DC_Class_Section = new HashSet<tbl_DC_Class_Section>();
            this.tbl_DC_ToppersWay = new HashSet<tbl_DC_ToppersWay>();
            this.tbl_DC_School_AssingTeacher = new HashSet<tbl_DC_School_AssingTeacher>();
            this.tbl_DC_School_ExamSchedule = new HashSet<tbl_DC_School_ExamSchedule>();
            this.tbl_DC_School_Homework = new HashSet<tbl_DC_School_Homework>();
            this.tbl_DC_School_StudyMaterial = new HashSet<tbl_DC_School_StudyMaterial>();
            this.tbl_DC_School_MessageCreation = new HashSet<tbl_DC_School_MessageCreation>();
        }
    
        public System.Guid ClassId { get; set; }
        public Nullable<System.Guid> SchoolId { get; set; }
        public string ClassName { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public Nullable<int> Class_id { get; set; }
    
        public virtual ICollection<tbl_DC_Class_Section> tbl_DC_Class_Section { get; set; }
        public virtual tbl_DC_School_Info tbl_DC_School_Info { get; set; }
        public virtual ICollection<tbl_DC_ToppersWay> tbl_DC_ToppersWay { get; set; }
        public virtual ICollection<tbl_DC_School_AssingTeacher> tbl_DC_School_AssingTeacher { get; set; }
        public virtual ICollection<tbl_DC_School_ExamSchedule> tbl_DC_School_ExamSchedule { get; set; }
        public virtual ICollection<tbl_DC_School_Homework> tbl_DC_School_Homework { get; set; }
        public virtual ICollection<tbl_DC_School_StudyMaterial> tbl_DC_School_StudyMaterial { get; set; }
        public virtual ICollection<tbl_DC_School_MessageCreation> tbl_DC_School_MessageCreation { get; set; }
    }
}
