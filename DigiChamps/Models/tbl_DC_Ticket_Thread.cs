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
    
    public partial class tbl_DC_Ticket_Thread
    {
        public int Comment_ID { get; set; }
        public Nullable<int> Ticket_ID { get; set; }
        public Nullable<int> Ticket_Dtl_ID { get; set; }
        public Nullable<int> User_Id { get; set; }
        public Nullable<bool> Is_Teacher { get; set; }
        public string User_Comment { get; set; }
        public Nullable<System.DateTime> User_Comment_Date { get; set; }
        public Nullable<System.DateTime> Modified_Date { get; set; }
        public string Modified_By { get; set; }
        public Nullable<bool> Is_Active { get; set; }
        public Nullable<bool> Is_Deleted { get; set; }
        public string R_image { get; set; }
    
        public virtual tbl_DC_Ticket tbl_DC_Ticket { get; set; }
        public virtual tbl_DC_Ticket_Dtl tbl_DC_Ticket_Dtl { get; set; }
    }
}
