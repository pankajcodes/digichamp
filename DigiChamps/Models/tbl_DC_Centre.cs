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
    
    public partial class tbl_DC_Centre
    {
        public tbl_DC_Centre()
        {
            this.tbl_DC_Batch = new HashSet<tbl_DC_Batch>();
        }
    
        public int Centre_Id { get; set; }
        public string Centre_Name { get; set; }
        public string Centre_Code { get; set; }
        public string Address_Line_1 { get; set; }
        public string Address_Line_2 { get; set; }
        public Nullable<int> State_Id { get; set; }
        public Nullable<int> City_Id { get; set; }
        public string Pin_Code { get; set; }
        public Nullable<System.DateTime> Inserted_Date { get; set; }
        public Nullable<int> Inserted_By { get; set; }
        public Nullable<System.DateTime> Modified_Date { get; set; }
        public Nullable<int> Modified_By { get; set; }
        public Nullable<bool> Is_Active { get; set; }
        public Nullable<bool> Is_Deleted { get; set; }
    
        public virtual ICollection<tbl_DC_Batch> tbl_DC_Batch { get; set; }
    }
}
