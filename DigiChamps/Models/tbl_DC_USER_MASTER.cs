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
    
    public partial class tbl_DC_USER_MASTER
    {
        public int USER_ID { get; set; }
        public string USER_NAME { get; set; }
        public string GENDER { get; set; }
        public string MOBILE { get; set; }
        public Nullable<System.DateTime> DATE_OF_BIRTH { get; set; }
        public string EMAIL { get; set; }
        public string IMAGE { get; set; }
        public Nullable<int> ROLE_TYPE { get; set; }
        public string ROLE_CODE { get; set; }
        public string ORG_NAME { get; set; }
        public string ADDRESS1 { get; set; }
        public string ADDRESS2 { get; set; }
        public string CITY { get; set; }
        public string STATE { get; set; }
        public string ZIP { get; set; }
        public string COUNTRY { get; set; }
        public string PHONE { get; set; }
        public string FAX { get; set; }
        public string DESIGNATION { get; set; }
        public string CONTACT_PERSON { get; set; }
        public string STATUS { get; set; }
        public string Inserted_By { get; set; }
        public Nullable<System.DateTime> Inserted_Date { get; set; }
        public string Modified_By { get; set; }
        public Nullable<System.DateTime> Modified_Date { get; set; }
        public Nullable<bool> Is_Active { get; set; }
        public Nullable<bool> Is_Deleted { get; set; }
    }
}