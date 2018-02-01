using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DigiChamps.Models
{
    public class Orderdetails
    {
        public class orders
        {
          //  public Nullable<int> OrderPkg_ID { get; set; }
            //public Nullable<int> Package_ID { get; set; }
            //public string Package_Name { get; set; }
            //public string Package_Desc { get; set; }
            //public Nullable<System.DateTime> Expiry_Date { get; set; }
            //public string Board_Name { get; set; }
            //public string Class_Name { get; set; }
            //public string Chapters { get; set; }
            //public string Regd_No { get; set; }
            //public string subjects { get; set; }
            //public string Customer_Name { get; set; }
            //public string Email { get; set; }
            //public string Mobile { get; set; }
            //public string Phone { get; set; }
            //public string Pincode { get; set; }
            //public string Address { get; set; }

            public int Order_ID { get; set; }
            public Nullable<System.DateTime> order_date { get; set; }
          
            public string Order_No { get; set; }
            public Nullable<int> Regd_ID { get; set; }
            public Nullable<decimal> tax_Amt { get; set; }
            public Nullable<decimal> Total { get; set; }
            public int Count { get; set; }

            public decimal? Grand_Total { get; set; }
        }
        public class success
        {
            public List<orders> order { get; set; }

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


    }
}