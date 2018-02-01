using DigiChamps.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace DigiChamps.Controllers
{

    [Authorize]
    public class OrderdetailsController : ApiController
    {  
        DigiChampsEntities DbContext = new DigiChampsEntities();

        [HttpGet]
        public HttpResponseMessage orderdetails(int? id, int? eid)
        {
            try
            {
                uint gid;
               
                if (id != null && eid != null && uint.TryParse(id.ToString().Trim(), out gid) && uint.TryParse(eid.ToString().Trim(), out gid) && id!=0 && eid!=0)
                    {
                        var ord_detail = DbContext.SP_DC_Order_Details(id).Where(x => x.Order_ID == eid).ToList();
                        if (ord_detail.Count > 0)
                        {
                            int oderid=Convert.ToInt32( ord_detail.FirstOrDefault().Order_ID);
                            int tax_am = Convert.ToInt32(DbContext.tbl_DC_Order_Tax.Where(x => x.Order_ID == oderid).ToList().Select(x => x.Tax_Amt).Sum());
                            int tot=  Convert.ToInt32( ord_detail.FirstOrDefault().Total);
                            var obj = new DigiChamps.Models.Orderdetails_inlist.getorders
                           {

                               success = new Orderdetails_inlist.success
                               {
                                    Customer_Name=ord_detail.FirstOrDefault().Customer_Name,
                                    Email   =ord_detail.FirstOrDefault().Email,     
                                    Mobile   =ord_detail.FirstOrDefault().Mobile,    
                                    Phone   =ord_detail.FirstOrDefault().Phone,  
                                    Pincode    =ord_detail.FirstOrDefault().Pincode,  
                                    Address   =ord_detail.FirstOrDefault().Address,   
                                    Order_ID   =ord_detail.FirstOrDefault().Order_ID,  
                                    Order_No   =ord_detail.FirstOrDefault().Order_No,  
                                    Regd_ID     =ord_detail.FirstOrDefault().Regd_ID, 
                                    order_date  =ord_detail.FirstOrDefault().order_date, 
                                     tax_Amt    =ord_detail.FirstOrDefault().tax_Amt,
                                    Total = tot - tax_am,
                                    Grand_Total=ord_detail.FirstOrDefault().Total,
                                    Total_savings = 0,
                                   order = (from c in ord_detail
                                            select new DigiChamps.Models.Orderdetails_inlist.orders
                                            { 
                                                Pacakage_price = getprice(c.OrderPkg_ID, c.Regd_ID),
                                                OrderPkg_ID = c.OrderPkg_ID,
                                                validity =(int)DbContext.tbl_DC_Package.Where(x => x.Package_ID == c.Package_ID).FirstOrDefault().Subscripttion_Period,
                                                Chapters = getchapter(c.Order_ID, c.Regd_ID).ToList().Count,
                                                Package_ID = c.Package_ID,
                                                Package_Name = c.Package_Name,
                                                Package_Desc = c.Package_Desc,
                                                Expiry_Date = c.Expiry_Date,
                                                Board_Name = c.Board_Name,
                                                Class_Name = c.Class_Name,
                                                subjects = (from s in getsubjects(c.Order_ID, c.Regd_ID)
                                                            select new DigiChamps.Models.Orderdetails_inlist.Subjects
                                                            {
                                                                Subject_Name = s.Trim()
                                                            }).ToList(),
                                               
                                            }).ToList()
                               }

                           };
                            return Request.CreateResponse(HttpStatusCode.OK, obj);
                        }
                        else
                        {
                            var obj = new DigiChamps.Models.Orderdetails.Errorresult
                           {
                               Error = new DigiChamps.Models.Orderdetails.Errorresponse
                               {
                                   Message = "You have not orderd anything."
                               }
                           };
                            return Request.CreateResponse(HttpStatusCode.OK, obj);
                        }
                    }
                    else
                    {
                        var obj = new DigiChamps.Models.Orderdetails.Errorresult
                        {
                            Error = new DigiChamps.Models.Orderdetails.Errorresponse
                            {
                                Message = "Please provide data correctly."
                            }
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, obj);
                    }
              
            }
            catch (Exception ex)
            {
                
              var obj = new DigiChamps.Models.Orderdetails.Errorresult
                    {
                        Error = new DigiChamps.Models.Orderdetails.Errorresponse
                        {
                            Message = "Something went wrong, Please try again."
                        }
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, obj);
            }
        }

        [HttpGet]
        public HttpResponseMessage myorders(int? id)
        {
            try
            {
               
                uint gid;
               
                if (id != null && uint.TryParse(id.ToString().Trim(), out gid) &&  id != 0 )
                    {
                        var ord_detail = DbContext.SP_DC_Order_Details(id).ToList();
                        if (ord_detail.Count > 0)
                        {
                            var obj = new DigiChamps.Models.Orderdetails.getorders
                            {
                                success = new Orderdetails.success
                                {
                                    order = (from c in ord_detail
                                             select new DigiChamps.Models.Orderdetails.orders
                                             {
                                                
                                                 Order_ID = c.Order_ID,
                                                 order_date = c.order_date,
                                                 Order_No = c.Order_No,
                                                 Regd_ID = c.Regd_ID,
                                                 tax_Amt = c.tax_Amt,
                                                 Total = (c.Total-DbContext.tbl_DC_Order_Tax.Where(x=>x.Order_ID==c.Order_ID).ToList().Select(x=>x.Tax_Amt).Sum()),
                                                 Grand_Total = c.Total,
                                                 Count = packagecount(c.Order_ID, c.Regd_ID)

                                             }).ToList()
                                }

                            };
                            return Request.CreateResponse(HttpStatusCode.OK, obj);
                        }
                        else
                        {
                            var obj = new DigiChamps.Models.Orderdetails.Errorresult
                            {
                                Error = new DigiChamps.Models.Orderdetails.Errorresponse
                                {
                                    Message = "You have not ordered anything."
                                }
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, obj);
                        }
                    }
                    else
                    {
                        var obj = new DigiChamps.Models.Orderdetails.Errorresult
                        {
                            Error = new DigiChamps.Models.Orderdetails.Errorresponse
                            {
                                Message = "Please provide data correctly."
                            }
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, obj);
                    }
               
            }
            catch (Exception ex)
            {

                var obj = new DigiChamps.Models.Orderdetails.Errorresult
                {
                    Error = new DigiChamps.Models.Orderdetails.Errorresponse
                    {
                        Message = "Something went wrong, Please try again."
                    }
                };
                return Request.CreateResponse(HttpStatusCode.OK, obj);
            }
        }


        public List<string> getchapter(int? id, int? red)
        {
            string  chapter = DbContext.SP_DC_Order_Details(red).Where(x => x.Order_ID == id).Select(x=>x.Chapters).FirstOrDefault().ToString();
            string[] chp = chapter.Split(',');
            List<string> li = new List<string>();
            foreach(string s in chp)
            {
                li.Add(s);
            }
            return li;
        }

        public List<string> getsubjects(int? id, int? red)
        {
            string chapter = DbContext.SP_DC_Order_Details(red).Where(x => x.Order_ID == id).Select(x => x.subjects).FirstOrDefault().ToString();
            string[] chp = chapter.Split(',');
            List<string> li = new List<string>();
            foreach (string s in chp)
            {
                li.Add(s);
            }
            return li;
        }


        public decimal getprice(int? id, int? red)
        {
           int pkg =Convert.ToInt32( DbContext.SP_DC_Order_Details(red).Where(x => x.OrderPkg_ID == id).Select(x => x.Package_ID).FirstOrDefault());
            decimal price=Convert.ToDecimal( DbContext.tbl_DC_Package.Where(x=>x.Package_ID==pkg).Select(x=>x.Price).FirstOrDefault());

            return price;
        }

          public int packagecount(int ?id, int ?red)
        {
            int count = DbContext.SP_DC_Order_Details(red).Where(x => x.Order_ID == id).Count();
            return count;
        }
       
    }

  
}
