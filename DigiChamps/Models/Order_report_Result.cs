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
    
    public partial class Order_report_Result
    {
        public int Order_ID { get; set; }
        public string Order_No { get; set; }
        public Nullable<decimal> Total { get; set; }
        public Nullable<System.DateTime> Inserted_Date { get; set; }
        public string Package_Name { get; set; }
        public string Customer_Name { get; set; }
        public Nullable<int> Package_ID { get; set; }
    }
}