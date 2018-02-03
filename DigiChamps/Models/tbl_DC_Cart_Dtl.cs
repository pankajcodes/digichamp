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
    
    public partial class tbl_DC_Cart_Dtl
    {
        public int CartDtl_ID { get; set; }
        public Nullable<int> Cart_ID { get; set; }
        public int Package_ID { get; set; }
        public Nullable<int> Board_ID { get; set; }
        public Nullable<int> Class_ID { get; set; }
        public Nullable<int> Subject_ID { get; set; }
        public Nullable<int> Chapter_ID { get; set; }
        public Nullable<bool> Status { get; set; }
        public Nullable<System.DateTime> Inserted_Date { get; set; }
        public string Inserted_By { get; set; }
        public string Modified_By { get; set; }
        public Nullable<System.DateTime> Modified_Date { get; set; }
        public Nullable<bool> Is_Active { get; set; }
        public Nullable<bool> Is_Deleted { get; set; }
    
        public virtual tbl_DC_Board tbl_DC_Board { get; set; }
        public virtual tbl_DC_Cart tbl_DC_Cart { get; set; }
        public virtual tbl_DC_Chapter tbl_DC_Chapter { get; set; }
        public virtual tbl_DC_Class tbl_DC_Class { get; set; }
        public virtual tbl_DC_Package tbl_DC_Package { get; set; }
        public virtual tbl_DC_Subject tbl_DC_Subject { get; set; }
    }
}