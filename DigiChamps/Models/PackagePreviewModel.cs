using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.Entity;
using DigiChamps.Models;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Web.Security;
using System.Text;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;

namespace DigiChamps.Models
{
    public class PackagePreviewModel
    {
        public string USER_CODE { get; set; }
        public string USER_NAME { get; set; }

        public int Regd_ID { get; set; }
        public string Regd_No { get; set; }
        public string Customer_Name { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }

        public int RegdDtl_ID { get; set; }
        public string Year { get; set; }

        public int PackageDtl_ID { get; set; }
        public Nullable<int> Package_ID { get; set; }
        public Nullable<int> Board_Id { get; set; }
        public string Board_Name { get; set; }
        public Nullable<int> Class_Id { get; set; }
        public string Class_Name { get; set; }
        public Nullable<int> Subject_Id { get; set; }
        public string Subject { get; set; }
        public Nullable<int> Chapter_Id { get; set; }
        public string Chapter { get; set; }

        public string Package_Name { get; set; }
        public string Package_Desc { get; set; }
        public Nullable<decimal> Price { get; set; }
        public Nullable<int> Total_Chapter { get; set; }
        public string Thumbnail { get; set; }
        public Nullable<int> Subscripttion_Period { get; set; }
        
        public int PackageSubDtl_ID { get; set; }
        public Nullable<int> SubScription_Limit { get; set; }

        public int Module_ID { get; set; }
        public string Module_Name { get; set; }
        public string Module_Desc { get; set; }
        public string Module_Content { get; set; }
        public string Module_video { get; set; }
        public DateTime? Inserted_Date { get; set; }
        public Nullable<bool> Is_Offline { get; set; }

        public int? Tablet_Id { get; set; }
    }
}