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
    public class DigiChampCartModel
    {
        public int Cart_ID { get; set; }
        public Nullable<int> Package_ID { get; set; }
        public Nullable<int> Order_ID { get; set; }
        public string Order_No { get; set; }
        public Nullable<int> Regd_ID { get; set; }
        public string Regd_No { get; set; }
        public Nullable<decimal> Quanity { get; set; }
        public Nullable<decimal> Disc_Perc { get; set; }
        public Nullable<decimal> Disc_Amt { get; set; }
        public Nullable<decimal> Total_Amt { get; set; }
        public string Amt_In_Words { get; set; }
        public Nullable<bool> Status { get; set; }
        public Nullable<bool> In_Cart { get; set; }
        public Nullable<bool> Is_Paid { get; set; }
        public Nullable<System.DateTime> Inserted_Date { get; set; }
        public string Inserted_By { get; set; }
        public string Modified_By { get; set; }
        public Nullable<System.DateTime> Modified_Date { get; set; }
        public Nullable<bool> Is_Active { get; set; }
        public Nullable<bool> Is_Deleted { get; set; }

        public int CartDtl_ID { get; set; }
        public Nullable<int> Board_ID { get; set; }
        public string Board_Name { get; set; }
        public Nullable<int> Class_ID { get; set; }
        public string Class_Name { get; set; }
        public Nullable<int> Subject_ID { get; set; }
        public string Subject { get; set; }
        public Nullable<int> Chapter_ID { get; set; }
        public string Chapter { get; set; }

        public string Package_Name { get; set; }
        public string Package_Desc { get; set; }
        public Nullable<int> Subscripttion_Period { get; set; }
        public Nullable<decimal> Price { get; set; }

        public int OrderID { get; set; }
        public string OrderNo { get; set; }
        public Nullable<int> Total_Chapter { get; set; }
        public string Thumbnail { get; set; }

        public int OrderPkg_ID { get; set; }
        public Nullable<System.DateTime> Expiry_Date { get; set; }
        public Nullable<int> OrderPkgSub_ID { get; set; }
        public Nullable<int> Module_ID { get; set; }
        public string Module_Name { get; set; }

        public int Tax_ID { get; set; }
        public Nullable<int> TaxType_ID { get; set; }
        public Nullable<decimal> Tax_Rate { get; set; }
        public Nullable<System.DateTime> TAX_Efect_Date { get; set; }
        public string Tax_Type { get; set; }
        public string Tax_Type_Short { get; set; }
        public string TAX_CODE { get; set; }
        public decimal Tax_Amt { get; set; }

        public int ModuleContent { get; set; }
        public int ModuleVideo { get; set; }



        public string amount { get; set; }
        public string firstname { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public string productinfo { get; set; }
        public string surl { get; set; }
        public string furl { get; set; }
        public string coupon { get; set; }

        public Nullable<bool> Is_Offline { get; set; }
        public Nullable<System.DateTime> Package_From { get; set; }
        public Nullable<System.DateTime> Package_To { get; set; }
        public Nullable<decimal> Included_Price { get; set; }
        public Nullable<decimal> Excluded_Price { get; set; }

        public string Address { get; set; }

        public string Mobile { get; set; }

        public string PIN { get; set; }
    }

    public class Digichampcartmodel1
    {


        public string coupon { get; set; }
    }
}