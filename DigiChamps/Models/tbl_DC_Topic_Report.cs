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
    
    public partial class tbl_DC_Topic_Report
    {
        public int Report_ID { get; set; }
        public Nullable<int> Topic_ID { get; set; }
        public Nullable<int> Start_Percentage { get; set; }
        public Nullable<int> End_Percentage { get; set; }
        public string Remark { get; set; }
        public Nullable<bool> Is_Active { get; set; }
        public Nullable<bool> Is_Deleted { get; set; }
        public string Modified_By { get; set; }
        public Nullable<System.DateTime> Modified_Date { get; set; }
        public Nullable<int> Inserted_By { get; set; }
        public Nullable<System.DateTime> Inserted_Date { get; set; }
        public Nullable<int> Percentage_id { get; set; }
    
        public virtual tbl_DC_Topic tbl_DC_Topic { get; set; }
    }
}
