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
    
    public partial class tbl_DC_City
    {
        public int City_Id { get; set; }
        public string City_Name { get; set; }
        public Nullable<int> State_Id { get; set; }
        public Nullable<System.DateTime> Inserted_Date { get; set; }
        public Nullable<int> Inserted_By { get; set; }
        public Nullable<System.DateTime> Modified_Date { get; set; }
        public Nullable<int> Modified_By { get; set; }
        public Nullable<bool> Is_Active { get; set; }
        public Nullable<bool> Is_Deleted { get; set; }
    
        public virtual tbl_DC_State tbl_DC_State { get; set; }
    }
}