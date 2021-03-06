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
    
    public partial class tbl_DC_Chapter
    {
        public tbl_DC_Chapter()
        {
            this.tbl_DC_Cart_Dtl = new HashSet<tbl_DC_Cart_Dtl>();
            this.tbl_DC_Exam = new HashSet<tbl_DC_Exam>();
            this.tbl_DC_Feedback_Chapter = new HashSet<tbl_DC_Feedback_Chapter>();
            this.tbl_DC_Module = new HashSet<tbl_DC_Module>();
            this.tbl_DC_Order_Pkg_Sub_Mod = new HashSet<tbl_DC_Order_Pkg_Sub_Mod>();
            this.tbl_DC_Order_Pkg_Sub = new HashSet<tbl_DC_Order_Pkg_Sub>();
            this.tbl_DC_Question = new HashSet<tbl_DC_Question>();
        }
    
        public int Chapter_Id { get; set; }
        public int Subject_Id { get; set; }
        public string Chapter { get; set; }
        public Nullable<int> TotalRaters { get; set; }
        public Nullable<decimal> AverageRating { get; set; }
        public Nullable<System.DateTime> Inserted_Date { get; set; }
        public Nullable<int> Inserted_By { get; set; }
        public Nullable<System.DateTime> Modified_Date { get; set; }
        public Nullable<int> Modified_By { get; set; }
        public Nullable<bool> Is_Active { get; set; }
        public Nullable<bool> Is_Deleted { get; set; }
    
        public virtual ICollection<tbl_DC_Cart_Dtl> tbl_DC_Cart_Dtl { get; set; }
        public virtual ICollection<tbl_DC_Exam> tbl_DC_Exam { get; set; }
        public virtual ICollection<tbl_DC_Feedback_Chapter> tbl_DC_Feedback_Chapter { get; set; }
        public virtual ICollection<tbl_DC_Module> tbl_DC_Module { get; set; }
        public virtual ICollection<tbl_DC_Order_Pkg_Sub_Mod> tbl_DC_Order_Pkg_Sub_Mod { get; set; }
        public virtual ICollection<tbl_DC_Order_Pkg_Sub> tbl_DC_Order_Pkg_Sub { get; set; }
        public virtual ICollection<tbl_DC_Question> tbl_DC_Question { get; set; }
    }
}
