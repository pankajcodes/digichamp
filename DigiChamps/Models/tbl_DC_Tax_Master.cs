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
    
    public partial class tbl_DC_Tax_Master
    {
        public tbl_DC_Tax_Master()
        {
            this.tbl_DC_Order_Tax = new HashSet<tbl_DC_Order_Tax>();
        }
    
        public int Tax_ID { get; set; }
        public Nullable<int> TaxType_ID { get; set; }
        public Nullable<decimal> Tax_Rate { get; set; }
        public Nullable<System.DateTime> TAX_Efect_Date { get; set; }
        public Nullable<System.DateTime> Inserted_Date { get; set; }
        public string Inserted_By { get; set; }
        public string Modified_By { get; set; }
        public Nullable<System.DateTime> Modified_Date { get; set; }
        public Nullable<bool> Is_Active { get; set; }
        public Nullable<bool> Is_Deleted { get; set; }
    
        public virtual ICollection<tbl_DC_Order_Tax> tbl_DC_Order_Tax { get; set; }
        public virtual tbl_DC_Tax_Type_Master tbl_DC_Tax_Type_Master { get; set; }
    }
}
