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
    
    public partial class tbl_DC_Order_Pkg_Sub
    {
        public tbl_DC_Order_Pkg_Sub()
        {
            this.tbl_DC_Order_Pkg_Sub_Mod = new HashSet<tbl_DC_Order_Pkg_Sub_Mod>();
        }
    
        public int OrderPkgSub_ID { get; set; }
        public Nullable<int> OrderPkg_ID { get; set; }
        public Nullable<int> Package_ID { get; set; }
        public Nullable<int> Board_ID { get; set; }
        public string Board_Name { get; set; }
        public Nullable<int> Class_ID { get; set; }
        public string Class_Name { get; set; }
        public Nullable<int> Subject_ID { get; set; }
        public string Subject_Name { get; set; }
        public Nullable<int> Chapter_ID { get; set; }
        public string Chapter_Name { get; set; }
        public Nullable<bool> Status { get; set; }
        public Nullable<System.DateTime> Inserted_Date { get; set; }
        public string Inserted_By { get; set; }
        public string Modified_By { get; set; }
        public Nullable<System.DateTime> Modified_Date { get; set; }
        public Nullable<bool> Is_Active { get; set; }
        public Nullable<bool> Is_Deleted { get; set; }
    
        public virtual tbl_DC_Board tbl_DC_Board { get; set; }
        public virtual tbl_DC_Chapter tbl_DC_Chapter { get; set; }
        public virtual tbl_DC_Class tbl_DC_Class { get; set; }
        public virtual tbl_DC_Order_Pkg tbl_DC_Order_Pkg { get; set; }
        public virtual ICollection<tbl_DC_Order_Pkg_Sub_Mod> tbl_DC_Order_Pkg_Sub_Mod { get; set; }
        public virtual tbl_DC_Subject tbl_DC_Subject { get; set; }
    }
}