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
    
    public partial class tbl_DC_Period
    {
        public System.Guid Id { get; set; }
        public string Title { get; set; }
        public string FromTime { get; set; }
        public string ToTime { get; set; }
        public Nullable<System.Guid> SchoolId { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<System.DateTime> Create_Date { get; set; }
        public Nullable<System.DateTime> Modified_Date { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
    }
}