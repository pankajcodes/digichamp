using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DigiChamps.Models
{
    public class Orderdetails_inlist
    {
        public class orders
        {
            public Nullable<int> OrderPkg_ID { get; set; }
            public Nullable<int> Package_ID { get; set; }
            public string Package_Name { get; set; }
            public string Package_Desc { get; set; }
            public Nullable<DateTime> Expiry_Date { get; set; }
            public string Board_Name { get; set; }
            public string Class_Name { get; set; }
            public int? Chapters { get; set; }
            public List<Subjects> subjects { get; set; }
            public decimal ? Pacakage_price { get; set; }
            public int validity { get; set; }
        }
        public class success
        {
            public string Customer_Name           { get; set; }
            public string Email               { get; set; }
            public string Mobile           { get; set; }
            public string Phone             { get; set; }
            public string Pincode           { get; set; }
            public string Address             { get; set; }
            public int    Order_ID        { get; set; }
            public string Order_No              { get; set; }
    public Nullable<int>  Regd_ID           { get; set; }
 public Nullable<DateTime>         order_date           { get; set; }
            public Nullable<decimal> tax_Amt          { get; set; }
            public Nullable<decimal> Total          { get; set; }
            public decimal?        Total_savings          { get; set; }
            public List<orders> order          { get; set; }


            public decimal? Grand_Total { get; set; }
        }
        public class getorders
        {
            public success success { get; set; }
        }

        public class Errorresult
        {
            public Errorresponse Error { get; set; }
        }
        public class Errorresponse
        {
            public string Message { get; set; }
        }
        public class Chapters
        {
            public string Chapter_Name { get; set; }
        }
        public class Subjects
        {
            public string Subject_Name { get; set; }
        }

    }
}