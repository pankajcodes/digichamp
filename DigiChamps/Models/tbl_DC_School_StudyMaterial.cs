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
    
    public partial class tbl_DC_School_StudyMaterial
    {
        public System.Guid StudyMaterialId { get; set; }
        public Nullable<System.Guid> SchoolId { get; set; }
        public Nullable<System.Guid> ClassId { get; set; }
        public Nullable<System.Guid> SubjectId { get; set; }
        public string Topic { get; set; }
        public string FilePath { get; set; }
        public string FileType { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public Nullable<int> Class_Id { get; set; }
        public string StudyMaterialTxt { get; set; }
    
        public virtual tbl_DC_School_Class tbl_DC_School_Class { get; set; }
        public virtual tbl_DC_School_Info tbl_DC_School_Info { get; set; }
        public virtual tbl_DC_School_Subject tbl_DC_School_Subject { get; set; }
    }
}
