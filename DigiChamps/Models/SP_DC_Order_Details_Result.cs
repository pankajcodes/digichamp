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
    
    public partial class SP_DC_Order_Details_Result
    {
        public int Order_ID { get; set; }
        public int OrderPkg_ID { get; set; }
        public Nullable<System.DateTime> order_date { get; set; }
        public string Order_No { get; set; }
        public Nullable<int> Regd_ID { get; set; }
        public Nullable<decimal> tax_Amt { get; set; }
        public Nullable<decimal> Total { get; set; }
        public Nullable<int> Package_ID { get; set; }
        public string Package_Name { get; set; }
        public string Package_Desc { get; set; }
        public Nullable<int> Total_Chapter { get; set; }
        public Nullable<System.DateTime> Expiry_Date { get; set; }
        public string Board_Name { get; set; }
        public string Class_Name { get; set; }
        public string subjects { get; set; }
        public string Chapters { get; set; }
        public string Regd_No { get; set; }
        public string Customer_Name { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
        public string Phone { get; set; }
        public string Pincode { get; set; }
        public string Address { get; set; }
    }
}
