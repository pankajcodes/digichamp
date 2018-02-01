using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data;
using System.Data.Entity;
using System.Web;
using DigiChamps.Models;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Net.Mail;

namespace DigiChamps.Controllers
{
    [Authorize]
    public class CartController : ApiController
    {
        DigiChampsEntities DbContext = new DigiChampsEntities();
        DateTime today = DigiChampsModel.datetoserver();
        // GET api/cart

        [HttpPost]
        public HttpResponseMessage addtocart([FromBody]addtocartmodele acm)
        {
            try
            {
                if (acm.chkpackaged != null)
                {
                    var data1 = acm.chkpackaged.chkpackage.ToList();
                    int u_name = Convert.ToInt32(acm.chkpackaged.id);
                    int pkgid = Convert.ToInt32(acm.chkpackaged.pkid);
                    var price = DbContext.tbl_DC_Package.Where(x => x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    int limit = Convert.ToInt32(acm.chkpackaged.lmt);
                    decimal price1 = Convert.ToDecimal(price.Price);

                    if (data1.Count > limit)
                    {
                        var ob = new Errorresult
                        {
                            Error = new Errorresponse
                            {
                                Message = "Chapeter selection should equal with the subscription limit"
                            }
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, ob);
                    }
                    else if (data1.Count < limit)
                    {
                        var ob = new Errorresult
                        {
                            Error = new Errorresponse
                            {
                                Message = "Chapeter selection should equal with the subscription limit"
                            }
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, ob);
                    }
                    else
                    {
                        var data = DbContext.tbl_DC_Registration.Where(x => x.Regd_ID == u_name && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();

                        int id1 = Convert.ToInt32(data.Regd_ID);
                        string regno = data.Regd_No;

                        var ord = DbContext.tbl_DC_Cart.Where(x => x.Regd_ID == u_name && x.Status == true && x.In_Cart == true && x.Is_Paid == false).FirstOrDefault();

                        tbl_DC_Cart crt = new tbl_DC_Cart();
                        crt.Package_ID = pkgid;
                        //crt.Order_ID = null;
                        //crt.Order_No = null;
                        crt.Regd_ID = id1;
                        crt.Regd_No = regno;
                        crt.Status = true;
                        crt.In_Cart = true;
                        crt.Is_Paid = false;
                        crt.Is_Active = true;
                        crt.Is_Deleted = false;
                        crt.Total_Amt = Math.Round(price1);
                        crt.Inserted_Date = DateTime.Now;
                        DbContext.tbl_DC_Cart.Add(crt);
                        DbContext.SaveChanges();

                        if (ord != null)
                        {
                            crt.Order_ID = ord.Cart_ID;
                            DateTime today = DateTime.Now.Date;
                            var ordid = crt.Order_ID;
                            if (ordid == null)
                            {
                                crt.Order_No = "DGCRT" + "/" + today.ToString("yyMMdd") + "/" + "00000" + 1;
                            }
                            else
                            {
                                int ordno = Convert.ToInt32(crt.Order_ID);
                                if (ordno > 0 && ordno <= 9)
                                {
                                    crt.Order_No = "DGCRT" + "/" + today.ToString("yyMMdd") + "/" + "00000" + Convert.ToString(ordno);
                                }
                                if (ordno > 9 && ordno <= 99)
                                {
                                    crt.Order_No = "DGCRT" + "/" + today.ToString("yyMMdd") + "/" + "0000" + Convert.ToString(ordno);
                                }
                                if (ordno > 99 && ordno <= 999)
                                {
                                    crt.Order_No = "DGCRT" + "/" + today.ToString("yyMMdd") + "/" + "000" + Convert.ToString(ordno);
                                }
                                if (ordno > 999 && ordno <= 9999)
                                {
                                    crt.Order_No = "DGCRT" + "/" + today.ToString("yyMMdd") + "/" + "00" + Convert.ToString(ordno);
                                }
                                if (ordno > 9999 && ordno <= 99999)
                                {
                                    crt.Order_No = "DGCRT" + "/" + today.ToString("yyMMdd") + "/" + "0" + Convert.ToString(ordno);
                                }
                            }
                            DbContext.SaveChanges();
                        }
                        else
                        {
                            crt.Order_ID = crt.Cart_ID;
                            DateTime today = DateTime.Now.Date;
                            var ordid = crt.Order_ID;
                            if (ordid == null)
                            {
                                crt.Order_No = "DGCRT" + "/" + today.ToString("yyMMdd") + "/" + "00000" + 1;
                            }
                            else
                            {
                                int ordno = Convert.ToInt32(crt.Order_ID);
                                if (ordno > 0 && ordno <= 9)
                                {
                                    crt.Order_No = "DGCRT" + "/" + today.ToString("yyMMdd") + "/" + "00000" + Convert.ToString(ordno);
                                }
                                if (ordno > 9 && ordno <= 99)
                                {
                                    crt.Order_No = "DGCRT" + "/" + today.ToString("yyMMdd") + "/" + "0000" + Convert.ToString(ordno);
                                }
                                if (ordno > 99 && ordno <= 999)
                                {
                                    crt.Order_No = "DGCRT" + "/" + today.ToString("yyMMdd") + "/" + "000" + Convert.ToString(ordno);
                                }
                                if (ordno > 999 && ordno <= 9999)
                                {
                                    crt.Order_No = "DGCRT" + "/" + today.ToString("yyMMdd") + "/" + "00" + Convert.ToString(ordno);
                                }
                                if (ordno > 9999 && ordno <= 99999)
                                {
                                    crt.Order_No = "DGCRT" + "/" + today.ToString("yyMMdd") + "/" + "0" + Convert.ToString(ordno);
                                }
                            }
                            DbContext.SaveChanges();
                        }

                        for (int i = 0; i < data1.Count; i++)
                        {
                            int chp_id = Convert.ToInt32(data1.ToList()[i].chpter_id);

                            var data2 = (from a in DbContext.tbl_DC_Chapter.Where(x => x.Chapter_Id == chp_id && x.Is_Active == true && x.Is_Deleted == false)
                                         join b in DbContext.tbl_DC_Subject.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                         on a.Subject_Id equals b.Subject_Id
                                         join c in DbContext.tbl_DC_Class.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                         on b.Class_Id equals c.Class_Id
                                         join d in DbContext.tbl_DC_Board.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                         on c.Board_Id equals d.Board_Id
                                         select new DigiChampCartModel
                                         {
                                             Board_ID = d.Board_Id,
                                             Class_ID = c.Class_Id,
                                             Subject_ID = b.Subject_Id,
                                             Chapter_ID = a.Chapter_Id
                                         }).FirstOrDefault();
                            tbl_DC_Cart_Dtl obj1 = new tbl_DC_Cart_Dtl();
                            obj1.Cart_ID = crt.Cart_ID;
                            obj1.Package_ID = pkgid;
                            obj1.Board_ID = data2.Board_ID;
                            obj1.Class_ID = data2.Class_ID;
                            obj1.Subject_ID = data2.Subject_ID;
                            obj1.Chapter_ID = data2.Chapter_ID;
                            obj1.Status = true;
                            obj1.Inserted_Date = DateTime.Now;
                            obj1.Is_Active = true;
                            obj1.Is_Deleted = false;
                            DbContext.tbl_DC_Cart_Dtl.Add(obj1);
                            DbContext.SaveChanges();
                        }
                        //return Request.CreateResponse(HttpStatusCode.OK, crt);
                        var ob = new Successresult
                        {
                            success = new Successrespone
                            {
                                Message = "Item has been added to your cart"
                            }
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, ob);
                    }
                }
                else
                {
                    var ob = new Errorresult
                    {
                        Error = new Errorresponse
                        {
                            Message = "Please select chapters to buy"
                        }
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, ob);
                }
            }
            catch (Exception ex)
            {
                var ob = new Errorresult
                {
                    Error = new Errorresponse
                    {
                        Message = "Something went wrong"
                    }
                };
                return Request.CreateResponse(HttpStatusCode.OK);
            }
        }
        public class addtocartmodele
        {
            public addtocartmodel chkpackaged { get; set; }
        }
        public class addtocartmodel
        {
            public List<chkpackage> chkpackage { get; set; }
            public int id { get; set; }
            public string lmt { get; set; }
            public string pkid { get; set; }
            public string prc { get; set; }
        }
        public class chkpackage
        {
            public int chpter_id { get; set; }
        }

        public class cart_detail
        {
            public List<CartModel> cartdata { get; set; }
            public Nullable<decimal> Original_price { get; set; }
            public Nullable<decimal> discountprice { get; set; }
            public Nullable<decimal> payblamt { get; set; }
            public Nullable<decimal> taxx { get; set; }
            public Nullable<int> cart_item { get; set; }
            public Nullable<decimal> discountpercentage { get; set; }
            public Nullable<decimal> discounted_price { get; set; }

            public string couponname { get; set; }
        }
        public class successss
        {
            public cart_detail getcartdata { get; set; }
        }
        public class getcart
        {
            public successss success { get; set; }
        }

        [HttpGet]
        public HttpResponseMessage cartdetail(int id)
        {
            try
            {
                decimal discperc = 0;
                var discprice = DbContext.tbl_DC_CouponCode.Where(x => x.Is_Default == true && x.Valid_From <= today && x.Valid_To >= today && x.Is_Active == true && x.Is_Delete == false).FirstOrDefault();
                if (discprice != null)
                {
                    if (discprice.Discount_Percent != null)
                    {
                        discperc = Convert.ToDecimal(discprice.Discount_Percent);
                    }
                    else { discperc = Convert.ToDecimal(discprice.Discount_Price); }

                }
                var data = (from i in
                                (from a in DbContext.tbl_DC_Registration.Where(x => x.Regd_ID == id && x.Is_Active == true && x.Is_Deleted == false)
                                 join b in DbContext.tbl_DC_Cart.Where(x => x.In_Cart == true && x.Is_Paid == false && x.Is_Active == true && x.Is_Deleted == false)
                                 on a.Regd_ID equals b.Regd_ID
                                 join c in DbContext.tbl_DC_Package.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                 on b.Package_ID equals c.Package_ID
                                 select new CartModel
                                 {
                                     Package_Name = c.Package_Name,
                                     Validity = (int)c.Subscripttion_Period,
                                     Package_ID = c.Package_ID,
                                     Subscripttion_Limit = (int)DbContext.tbl_DC_PackageSub_Dtl.Where(x => x.Package_ID == c.Package_ID).Select(x => x.SubScription_Limit).ToList().Sum(),
                                     Price = Math.Round((decimal)c.Price),
                                     Is_Offline = (Boolean)c.Is_Offline,
                                     packageimage = c.Thumbnail,
                                     Cart_ID = b.Cart_ID
                                 }).ToList()
                            select new CartModel
                            {
                                Package_Name = i.Package_Name,
                                Validity = (int)i.Validity,
                                Package_ID = i.Package_ID,
                                Subscripttion_Limit = (int)DbContext.tbl_DC_PackageSub_Dtl.Where(x => x.Package_ID == i.Package_ID).Select(x => x.SubScription_Limit).ToList().Sum(),
                                Price = Math.Round((decimal)i.Price),
                                Package_Price = Math.Round(price(i.Package_ID)),
                                Discounted_Price = Math.Round(Discounted_Price(i.Package_ID)),
                                Discount = Math.Round(price(i.Package_ID) - Discounted_Price(i.Package_ID)),
                                packageimage = i.packageimage,
                                Is_Offline = (Boolean)i.Is_Offline,
                                discountpercentage = discperc,
                                Cart_ID = i.Cart_ID
                            }).ToList();

                if (data.Count == 0)
                {
                    var ob = new Errorresult
                    {
                        Error = new Errorresponse
                        {
                            Message = "Your cart is empty"
                        }
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, ob);
                }
                else
                {
                    var dataa = data;
                    decimal tax = taxcalculate();
                    decimal tax1 = (decimal)tax;
                    int cart_item = Convert.ToInt32(data.Count);
                    decimal totalprice1 = data.ToList().Where(x => x.Is_Offline == false || x.Is_Offline == null).Select(c => (decimal)c.Price).Sum();

                    var data1 = (from x in data.ToList().Where(x => x.Is_Offline == true)
                                 join y in DbContext.tbl_DC_Package_Period.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                 on x.Package_ID equals y.Package_ID
                                 select new DigiChampCartModel
                                 {
                                     Is_Offline = x.Is_Offline,
                                     Package_ID = x.Package_ID,
                                     Price = x.Price,
                                     Excluded_Price = y.Excluded_Price
                                 }).ToList();

                    if (data1.Count > 0)
                    {
                        decimal totalprice2 = data1.ToList().Where(x => x.Is_Offline == true).Select(c => (decimal)c.Price).Sum();
                        decimal totalprice3 = data1.ToList().Where(x => x.Is_Offline == true).Select(c => (decimal)c.Excluded_Price).Sum();
                        decimal totalprice = Convert.ToDecimal(totalprice1 + totalprice2 + totalprice3);
                        decimal totalpaybletax = (tax * totalprice) / 100;
                        decimal totalpayblamt = Math.Round((totalprice + totalpaybletax), 2);
                        var price = DbContext.tbl_DC_CouponCode.Where(x => x.Is_Default == true && x.Valid_From <= today && x.Valid_To >= today && x.Is_Active == true && x.Is_Delete == false).FirstOrDefault();
                        if (price != null)
                        {
                            if (price.Discount_Price != null)
                            {
                                int totl_pkg = Convert.ToInt32(data.ToList().Count);
                                decimal discountprice = Convert.ToDecimal(price.Discount_Price);
                                decimal prc = Convert.ToDecimal(discountprice);
                                decimal discountamount = prc * totl_pkg;
                                decimal pricee = Math.Round(Convert.ToDecimal(data.ToList().Select(x => x.Discounted_Price).Sum()));
                                decimal totalpaybletax1 = Math.Round(tax * Convert.ToDecimal(pricee) / 100);
                                decimal totalpayblamt1 = Math.Round((Convert.ToDecimal(pricee) + totalpaybletax1), 2);
                                decimal payblamt = Math.Round(totalpayblamt1, 2);
                                decimal taxx = Math.Round((Convert.ToDecimal(payblamt) - Convert.ToDecimal(pricee)), 2);
                                //decimal finalprice = pricee - discountamount;

                                cart_detail crt_dtl = new cart_detail();
                                crt_dtl.cartdata = data;
                                crt_dtl.payblamt = Math.Round(payblamt);
                                crt_dtl.taxx = Math.Round(Convert.ToDecimal(taxx));
                                crt_dtl.Original_price = Math.Round(Convert.ToDecimal(totalprice));
                                crt_dtl.discounted_price = Math.Round(Convert.ToDecimal(pricee));
                                crt_dtl.discountprice = Math.Round(Convert.ToDecimal(discountamount));
                                crt_dtl.cart_item = Convert.ToInt32(cart_item);
                                crt_dtl.discountpercentage = 0;
                                crt_dtl.couponname = price.Coupon_Code;
                                if (crt_dtl != null)
                                {
                                    var obj = new getcart
                                    {
                                        success = new successss()
                                        {
                                            getcartdata = crt_dtl
                                        }
                                    };
                                    return Request.CreateResponse(HttpStatusCode.OK, obj);
                                }
                                else
                                {
                                    var ob = new Errorresult
                                    {
                                        Error = new Errorresponse
                                        {
                                            Message = "Something went wrong"
                                        }
                                    };
                                    return Request.CreateResponse(HttpStatusCode.OK, ob);
                                }
                            }
                            else
                            {
                                int totl_pkg = Convert.ToInt32(data.ToList().Count);
                                decimal discountprice = Convert.ToDecimal(price.Discount_Percent);
                                decimal prc = Convert.ToDecimal(discountprice);
                                decimal t_price = Math.Round(Convert.ToDecimal(data.ToList().Select(x => x.Discounted_Price).Sum()));
                                decimal pricee = Math.Round(t_price);
                                decimal totalpaybletax1 = (tax * Convert.ToDecimal(pricee) / 100);
                                decimal totalpayblamt1 = Math.Round((Convert.ToDecimal(pricee) + totalpaybletax1), 2);
                                decimal payblamt = Math.Round(totalpayblamt1, 2);
                                decimal taxx = Math.Round((Convert.ToDecimal(payblamt) - Convert.ToDecimal(pricee)), 2);
                                decimal finalprice = pricee - t_price;

                                cart_detail crt_dtl = new cart_detail();
                                crt_dtl.cartdata = data;
                                crt_dtl.payblamt = Math.Round(payblamt);
                                crt_dtl.Original_price = Math.Round(Convert.ToDecimal(totalprice));
                                crt_dtl.discounted_price = Math.Round(Convert.ToDecimal(pricee));
                                crt_dtl.discountprice = 0;
                                crt_dtl.taxx = Math.Round(Convert.ToDecimal(taxx));
                                crt_dtl.cart_item = Convert.ToInt32(cart_item);
                                crt_dtl.discountpercentage = Math.Round(prc);
                                crt_dtl.couponname = price.Coupon_Code;
                                if (crt_dtl != null)
                                {
                                    var obj = new getcart
                                    {
                                        success = new successss()
                                        {
                                            getcartdata = crt_dtl
                                        }
                                    };
                                    return Request.CreateResponse(HttpStatusCode.OK, obj);
                                }
                                else
                                {
                                    var ob = new Errorresult
                                    {
                                        Error = new Errorresponse
                                        {
                                            Message = "Something went wrong"
                                        }
                                    };
                                    return Request.CreateResponse(HttpStatusCode.OK, ob);
                                }
                            }
                        }
                        else
                        {
                            decimal payblamt = totalpayblamt;
                            decimal pricee = Math.Round(totalprice, 2);
                            decimal taxx = Math.Round((Convert.ToDecimal(payblamt) - Convert.ToDecimal(pricee)), 2);

                            cart_detail crt_dtl = new cart_detail();
                            crt_dtl.cartdata = data;
                            crt_dtl.payblamt = Math.Round(payblamt);
                            crt_dtl.Original_price = Math.Round(Convert.ToDecimal(totalprice));
                            crt_dtl.discounted_price = Math.Round(Convert.ToDecimal(totalprice));
                            crt_dtl.discountpercentage = 0;
                            crt_dtl.discountprice = 0;
                            crt_dtl.taxx = Math.Round(Convert.ToDecimal(taxx));
                            crt_dtl.cart_item = Convert.ToInt32(cart_item);
                            crt_dtl.couponname = "";

                            if (crt_dtl != null)
                            {
                                var obj = new getcart
                                {
                                    success = new successss()
                                    {
                                        getcartdata = crt_dtl
                                    }
                                };
                                return Request.CreateResponse(HttpStatusCode.OK, obj);
                            }
                            else
                            {
                                var ob = new Errorresult
                                {
                                    Error = new Errorresponse
                                    {
                                        Message = "Something went wrong"
                                    }
                                };
                                return Request.CreateResponse(HttpStatusCode.OK, ob);
                            }
                        }
                    }
                    else
                    {
                        decimal totalprice = Convert.ToDecimal(totalprice1);
                        decimal totalpaybletax = (tax * totalprice) / 100;
                        decimal totalpayblamt = Math.Round((totalprice + totalpaybletax), 2);
                        var price = DbContext.tbl_DC_CouponCode.Where(x => x.Is_Default == true && x.Valid_From <= today && x.Valid_To >= today && x.Is_Active == true && x.Is_Delete == false).FirstOrDefault();
                        if (price != null)
                        {
                            if (price.Discount_Price != null)
                            {
                                int totl_pkg = Convert.ToInt32(data.ToList().Count);
                                decimal discountprice = Convert.ToDecimal(price.Discount_Price);
                                decimal prc = Convert.ToDecimal(discountprice);
                                decimal pricee = Math.Round(Convert.ToDecimal(data.ToList().Select(x => x.Discounted_Price).Sum()));
                                decimal totalpaybletax1 = (tax * Convert.ToDecimal(pricee) / 100);
                                decimal totalpayblamt1 = Math.Round((Convert.ToDecimal(pricee) + totalpaybletax1), 2);
                                decimal payblamt = Math.Round(totalpayblamt1, 2);
                                decimal taxx = Math.Round((Convert.ToDecimal(payblamt) - Convert.ToDecimal(pricee)), 2);

                                cart_detail crt_dtl = new cart_detail();
                                crt_dtl.cartdata = data;
                                crt_dtl.payblamt = Math.Round(payblamt);
                                crt_dtl.Original_price = Math.Round(Convert.ToDecimal(pricee));
                                crt_dtl.discountprice = Math.Round(Convert.ToDecimal(discountprice));
                                crt_dtl.discounted_price = Math.Round(Convert.ToDecimal(pricee));
                                crt_dtl.taxx = Math.Round(Convert.ToDecimal(taxx));
                                crt_dtl.cart_item = Convert.ToInt32(cart_item);
                                crt_dtl.discountpercentage = 0;
                                crt_dtl.couponname = price.Coupon_Code;
                                if (crt_dtl != null)
                                {
                                    var obj = new getcart
                                    {
                                        success = new successss()
                                        {
                                            getcartdata = crt_dtl
                                        }
                                    };
                                    return Request.CreateResponse(HttpStatusCode.OK, obj);
                                }
                                else
                                {
                                    var ob = new Errorresult
                                    {
                                        Error = new Errorresponse
                                        {
                                            Message = "Something went wrong"
                                        }
                                    };
                                    return Request.CreateResponse(HttpStatusCode.OK, ob);
                                }
                            }
                            else
                            {
                                int totl_pkg = Convert.ToInt32(data.ToList().Count);
                                decimal discountprice = Convert.ToDecimal(price.Discount_Percent);
                                decimal prc = Convert.ToDecimal(discountprice);
                                decimal t_price = Math.Round(Convert.ToDecimal(data.ToList().Select(x => x.Discounted_Price).Sum()));
                                decimal pricee = Math.Round(t_price);
                                decimal totalpaybletax1 = (tax * Convert.ToDecimal(pricee) / 100);
                                decimal totalpayblamt1 = Math.Round((Convert.ToDecimal(pricee) + totalpaybletax1), 2);
                                decimal payblamt = Math.Round(totalpayblamt1, 2);
                                decimal taxx = Math.Round((Convert.ToDecimal(payblamt) - Convert.ToDecimal(pricee)), 2);
                                decimal finalprice = pricee - t_price;
                                cart_detail crt_dtl = new cart_detail();
                                crt_dtl.cartdata = data;
                                crt_dtl.payblamt = Math.Round(payblamt);
                                crt_dtl.Original_price = Math.Round(Convert.ToDecimal(pricee));
                                crt_dtl.discounted_price = Math.Round(Convert.ToDecimal(pricee));
                                crt_dtl.discountprice = 0;
                                crt_dtl.couponname = price.Coupon_Code;
                                crt_dtl.taxx = Math.Round(Convert.ToDecimal(taxx));
                                crt_dtl.cart_item = Convert.ToInt32(cart_item);
                                crt_dtl.discountpercentage = Math.Round(prc);
                                if (crt_dtl != null)
                                {
                                    var obj = new getcart
                                    {
                                        success = new successss()
                                        {
                                            getcartdata = crt_dtl
                                        }
                                    };
                                    return Request.CreateResponse(HttpStatusCode.OK, obj);
                                }
                                else
                                {
                                    var ob = new Errorresult
                                    {
                                        Error = new Errorresponse
                                        {
                                            Message = "Something went wrong"
                                        }
                                    };
                                    return Request.CreateResponse(HttpStatusCode.OK, ob);
                                }
                            }
                        }
                        else
                        {
                            decimal payblamt = Math.Round(totalpayblamt);
                            decimal pricee = Math.Round(totalprice);
                            decimal taxx = Math.Round((Convert.ToDecimal(payblamt) - Convert.ToDecimal(pricee)));

                            cart_detail crt_dtl = new cart_detail();
                            crt_dtl.cartdata = data;
                            crt_dtl.payblamt = Math.Round(payblamt);
                            crt_dtl.Original_price = Math.Round(Convert.ToDecimal(pricee));
                            crt_dtl.discounted_price = Math.Round(Convert.ToDecimal(pricee));
                            crt_dtl.discountprice = 0;
                            crt_dtl.discountpercentage = 0;
                            crt_dtl.taxx = Math.Round(Convert.ToDecimal(taxx));
                            crt_dtl.cart_item = Convert.ToInt32(cart_item);
                            crt_dtl.couponname = "";
                            if (crt_dtl != null)
                            {
                                var obj = new getcart
                                {
                                    success = new successss()
                                    {
                                        getcartdata = crt_dtl
                                    }
                                };
                                return Request.CreateResponse(HttpStatusCode.OK, obj);
                            }
                            else
                            {
                                var ob = new Errorresult
                                {
                                    Error = new Errorresponse
                                    {
                                        Message = "Something went wrong"
                                    }
                                };
                                return Request.CreateResponse(HttpStatusCode.OK, ob);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var ob = new Errorresult
                {
                    Error = new Errorresponse
                    {
                        Message = "Something went wrong"
                    }
                };
                return Request.CreateResponse(HttpStatusCode.OK, ob);
            }
        }

        private decimal Discounted_val(decimal p1, decimal p2)
        {
            decimal amount = p1 - p2;
            decimal netamount = Math.Round((amount / p1) * 100,0);
            return netamount;
        }
        
        public decimal price(int id)
        {
            decimal price1 = 0;
            DigiChamps.Models.DigiChampsEntities dc = new DigiChamps.Models.DigiChampsEntities();
            var dt1 = dc.tbl_DC_Package.Where(x => x.Package_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
            var dt = dc.tbl_DC_Package_Period.Where(x => x.Package_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
            if (dt != null)
            {
                decimal price2 = Convert.ToDecimal(dt1.Price + dt.Excluded_Price);
                price1 = price2;
            }
            else
            {
                decimal price2 = Convert.ToDecimal(dt1.Price);
                price1 = 0;
            }
            return price1;
        }
        public decimal Discounted_Price(int id)
        {
            DateTime today = DigiChamps.Models.DigiChampsModel.datetoserver();
            decimal price1 = 0;
            DigiChamps.Models.DigiChampsEntities dc = new DigiChamps.Models.DigiChampsEntities();
            var dt1 = dc.tbl_DC_Package.Where(x => x.Package_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
            var dt = dc.tbl_DC_Package_Period.Where(x => x.Package_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
            var disc_prc = dc.tbl_DC_CouponCode.Where(x => x.Is_Default == true && x.Valid_From <= today && x.Valid_To >= today && x.Is_Active == true && x.Is_Delete == false).FirstOrDefault();
            if (disc_prc != null)
            {
                if (dt != null && dt1 != null)
                {
                    if (disc_prc.Discount_Price != null)
                    {
                        decimal price = Convert.ToDecimal(dt1.Price);
                        if (price > disc_prc.Discount_Price)
                        {
                            decimal price2 = Convert.ToDecimal(price - Convert.ToDecimal(disc_prc.Discount_Price));
                            price1 = Convert.ToDecimal(price2 + dt.Excluded_Price);
                        }
                        else
                        {
                            decimal price2 = Convert.ToDecimal(dt.Excluded_Price + dt1.Price);
                            price1 = price2;
                        }
                    }
                    else
                    {
                        decimal price = Convert.ToDecimal(dt1.Price);
                        decimal disc = Convert.ToDecimal(disc_prc.Discount_Percent);
                        if (disc_prc.Discount_Percent != null)
                        {
                            decimal disc_price2 = Convert.ToDecimal((price * Convert.ToDecimal(disc)) / 100);
                            decimal price2 = Convert.ToDecimal(price - Convert.ToDecimal(disc_price2));
                            price1 = Convert.ToDecimal(price2 + dt.Excluded_Price);
                        }
                        else
                        {
                            decimal price2 = Convert.ToDecimal(price);
                            price1 = price2;
                        }
                    }
                }
                else
                {
                    if (disc_prc.Discount_Price != null)
                    {
                        decimal price = Convert.ToDecimal(dt1.Price);
                        if (price > disc_prc.Discount_Price)
                        {
                            decimal price2 = Convert.ToDecimal(dt1.Price - Convert.ToDecimal(disc_prc.Discount_Price));
                            price1 = price2;
                        }
                        else
                        {
                            decimal price2 = Convert.ToDecimal(dt1.Price);
                            price1 = price2;
                        }
                    }
                    else
                    {
                        decimal price = Convert.ToDecimal(dt1.Price);
                        decimal disc = Convert.ToDecimal(disc_prc.Discount_Percent);
                        if (disc_prc.Discount_Percent != null)
                        {
                            decimal disc_price2 = Convert.ToDecimal((price * Convert.ToDecimal(disc)) / 100);
                            decimal price2 = Convert.ToDecimal(price - Convert.ToDecimal(disc_price2));
                            price1 = price2;
                        }
                        else
                        {
                            decimal price2 = Convert.ToDecimal(dt1.Price);
                            price1 = price2;
                        }
                    }
                }
            }
            else
            {
                if (dt != null && dt1 != null)
                {
                    decimal price2 = Convert.ToDecimal(dt.Excluded_Price + dt1.Price);
                    price1 = price2;
                }
                else
                {
                    decimal price2 = Convert.ToDecimal(dt1.Price);
                    price1 = price2;
                }

            }
            return price1;
        }

        public class CartModel
        {
            public string Package_Name {get;set;}
            public int Validity {get;set;}
            public Nullable<int> Subscripttion_Limit { get; set; }
            public int Package_ID {get;set;}
            public Nullable<decimal> Price { get; set; }
            public Nullable<decimal> Discounted_Price { get; set; }
            //public decimal Exclude_Price { get; set; }
            public decimal Package_Price { get; set; }
            public string packageimage { get; set; }
            public Boolean Is_Offline {get;set;}
            public int Cart_ID {get;set;}

            public decimal discountpercentage { get; set; }

            public decimal Discount { get; set; }
        }
        public class cartview
        {
            public List<int> cartid { get; set; }
        }
        public decimal taxcalculate()
        {
            // Get the cart
            var taxcalculate = (from a in DbContext.tbl_DC_Tax_Master.Where(x => x.Is_Active == true && x.Is_Deleted == false && x.TAX_Efect_Date < today)
                                join b in DbContext.tbl_DC_Tax_Type_Master.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                on a.TaxType_ID equals b.TaxType_ID
                                select new DigiChampsModel.TaxModel
                                {
                                    Tax_Rate = a.Tax_Rate
                                }).ToList();

            decimal taxamount = taxcalculate.ToList().Select(c => (decimal)c.Tax_Rate).Sum();
            //TempData["taxamount"] = Math.Round(taxamount, 2);
            return taxamount;
        }
        [HttpPost]
        public HttpResponseMessage DeleteItem(int id)
        {
            try
            {
                if (id != null)
                {
                tbl_DC_Cart obj = DbContext.tbl_DC_Cart.Where(x => x.Cart_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                obj.Is_Active = false;
                obj.Is_Deleted = true;
                DbContext.Entry(obj).State = EntityState.Modified;
                DbContext.SaveChanges();

                tbl_DC_Cart_Dtl obj1 = DbContext.tbl_DC_Cart_Dtl.Where(x => x.Cart_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                obj1.Is_Active = true;
                obj1.Is_Deleted = false;
                DbContext.Entry(obj1).State = EntityState.Modified;
                DbContext.SaveChanges();
                 
                    var ob = new Successresult
                    {
                        success = new Successrespone
                        {
                            Message = "One item deleted from your cart"
                        }

                    };
                    return Request.CreateResponse(HttpStatusCode.OK, ob);
                }
                else
                {
                    var ob = new Errorresult
                    {
                        Error = new Errorresponse
                        {
                            Message = "Please provide data correctly"
                        }
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, ob);
                }

            }
            catch (Exception ex)
            {
                var ob = new Errorresult
                {
                    Error = new Errorresponse
                    {
                        Message = "Something went wrong"
                    }
                };
                return Request.CreateResponse(HttpStatusCode.OK, ob);
            }
        }

        public class RootObject2
        {
            public deleteitems deleteitem { get; set; }
        }
        public class deleteitems
        {
            public List<Removecart> removecart { get; set; }
        }


        public class Removecart
        {
            public int id { get; set; }
        }
        public HttpResponseMessage RemoveCart([FromBody]RootObject2 cv)
        {
            try
            {
                var data1 = cv.deleteitem.removecart.ToList();

                //var data = (from a in DbContext.tbl_DC_Registration.Where(x => x.Mobile == u_name && x.Is_Active == true && x.Is_Deleted == false)
                //            join b in DbContext.tbl_DC_Cart.Where(x => x.In_Cart == true && x.Is_Paid == false && x.Is_Active == true && x.Is_Deleted == false)
                //             on a.Regd_ID equals b.Regd_ID
                //            select new DigiChampCartModel
                //            {
                //                Cart_ID = b.Cart_ID
                //            }).ToList();

                if (data1.Count > 0)
                {
                    foreach (var item in data1)
                    {
                        int regid = item.id;
                        tbl_DC_Cart obj = DbContext.tbl_DC_Cart.Where(x => x.Cart_ID == regid).FirstOrDefault();
                        obj.In_Cart = false;
                        obj.Is_Active = false;
                        obj.Is_Deleted = true;
                        obj.Status = false;
                        DbContext.Entry(obj).State = EntityState.Modified;
                        DbContext.SaveChanges();
                    }
                    var ob = new Successresult1
                    {
                        success = new Successrespone1
                            {
                                deleteitem = new deletemsg
                                {
                                    Message = "cart item deleted successfully"

                                }
                            }
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, ob);
                }
                else
                {
                    var ob = new Errorresult
                    {
                        Error = new Errorresponse
                        {
                            Message = "Please provide data correctly"
                        }
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, ob);
                }
            }
            catch (Exception ex)
            {
                var ob = new Errorresult
                {
                    Error = new Errorresponse
                    {
                        Message = "Something went wrong"
                    }
                };
                return Request.CreateResponse(HttpStatusCode.OK, ob);
            }
        }

        public class checkout_detail
        {
            public List<CartcheckoutModel> cartdata { get; set; }
            public Nullable<decimal> Original_price { get; set; }
            public Nullable<decimal> discountprice { get; set; }
            public Nullable<decimal> discpercentage { get; set; }
            public Nullable<decimal> payblamt { get; set; }
            public Nullable<decimal> taxx { get; set; }
            public string Customer_Name { get; set; }
            public string Mobile_Number { get; set; }
            public string Email_Id { get; set; }
            public string coupon_code { get; set; }
            public Nullable<int> cart_item { get; set; }
            public int Order_Id { get; set; }
            public string Order_No { get; set; }

            public decimal afterdicount { get; set; }
            public string msg { get; set; }
        }
        public class CartcheckoutModel
        {
            public string Package_Image { get; set; }
            public string Package_Name { get; set; }
            public int Validity { get; set; }
            public Nullable<int> Subscripttion_Limit { get; set; }
            public int Package_ID { get; set; }
            public Nullable<decimal> Price { get; set; }
            public Nullable<decimal> Discounted_Price { get; set; }
            //public decimal Exclude_Price { get; set; }
            public decimal Package_Price { get; set; }
            public Boolean Is_Offline { get; set; }
            public int Cart_ID { get; set; }
            public int Order_Id { get; set; }
            public string Order_No { get; set; }

            public decimal discountpercentage { get; set; }
        }
       
        public class successsss
        {
            public checkout_detail getcheckoutdata { get; set; }
        }
        public class getcheckoutdata
        {
            public successsss success { get; set; }
        }
        [HttpGet]
        public HttpResponseMessage checkout(int id)
        {
            try
            {
                var price1 = DbContext.tbl_DC_CouponCode.Where(x => x.Is_Default == true && x.Valid_From <= today && x.Valid_To >= today && x.Is_Active == true && x.Is_Delete == false).FirstOrDefault();
                decimal dispnt = 0;
                if (price1 != null)
                {
                    if(price1.Discount_Percent!=null)
                    {
                        dispnt = Convert.ToDecimal(price1.Discount_Percent);
                    }
                    else
                    {
                        dispnt = 0;
                    }
                    var priceee = price1;
                    var cp_code = price1.Coupon_Code;
                }
                var data1 = DbContext.tbl_DC_Registration.Where(x => x.Regd_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();

                var data = (from i in
                                (from a in DbContext.tbl_DC_Registration.Where(x => x.Regd_ID == id && x.Is_Active == true && x.Is_Deleted == false)
                                 join b in DbContext.tbl_DC_Cart.Where(x => x.In_Cart == true && x.Is_Paid == false && x.Is_Active == true && x.Is_Deleted == false)
                                 on a.Regd_ID equals b.Regd_ID
                                 join c in DbContext.tbl_DC_Package.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                 on b.Package_ID equals c.Package_ID
                                 select new CartcheckoutModel
                                 {
                                     Package_Image = c.Thumbnail,
                                     Package_Name = c.Package_Name,
                                     Validity = (int)c.Subscripttion_Period,
                                     Package_ID = c.Package_ID,
                                     Subscripttion_Limit = (int)DbContext.tbl_DC_PackageSub_Dtl.Where(x => x.Package_ID == c.Package_ID).Select(x => x.SubScription_Limit).ToList().Sum(),
                                     Price = Math.Round((decimal)c.Price),
                                     Is_Offline = (Boolean)c.Is_Offline,
                                     Cart_ID = b.Cart_ID,
                                     Order_Id = (int)b.Order_ID,
                                     Order_No = b.Order_No
                                 }).ToList()
                            select new CartcheckoutModel
                            {
                                Package_Image = i.Package_Image,
                                Package_Name = i.Package_Name,
                                Validity = (int)i.Validity,
                                Package_ID = i.Package_ID,
                                Subscripttion_Limit = (int)DbContext.tbl_DC_PackageSub_Dtl.Where(x => x.Package_ID == i.Package_ID).Select(x => x.SubScription_Limit).ToList().Sum(),
                                Price = Math.Round((decimal)i.Price),
                                Package_Price = Math.Round(price(i.Package_ID)),
                                Discounted_Price = Math.Round(Discounted_Price(i.Package_ID)),
                                Is_Offline = (Boolean)i.Is_Offline,
                                Cart_ID = i.Cart_ID,
                                Order_Id = i.Order_Id,
                                Order_No = i.Order_No,
                                //discountpercentage = Discounted_val(price(i.Package_ID), Discounted_Price(i.Package_ID)),
                                discountpercentage = dispnt,
                            }).ToList();

                if (data.Count == 0)
                {
                    var ob = new Errorresult
                    {
                        Error = new Errorresponse
                        {
                            Message = "Please provide data correctly"
                        }
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, ob);
                }
                else
                {
                    var dataa = data;
                    decimal tax = taxcalculate();
                    decimal tax1 = (decimal)tax;
                    int cart_item = Convert.ToInt32(data.Count);
                    decimal totalprice1 = data.ToList().Where(x => x.Is_Offline == false || x.Is_Offline == null).Select(c => (decimal)c.Price).Sum();

                    var data2 = (from x in data.ToList().Where(x => x.Is_Offline == true)
                                 join y in DbContext.tbl_DC_Package_Period.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                 on x.Package_ID equals y.Package_ID
                                 select new DigiChampCartModel
                                 {
                                     Is_Offline = x.Is_Offline,
                                     Package_ID = x.Package_ID,
                                     Price = x.Price,
                                     Excluded_Price = y.Excluded_Price
                                 }).ToList();

                    if (data2.Count > 0)
                    {
                        decimal totalprice2 = data2.ToList().Where(x => x.Is_Offline == true).Select(c => (decimal)c.Price).Sum();
                        decimal totalprice3 = data2.ToList().Where(x => x.Is_Offline == true).Select(c => (decimal)c.Excluded_Price).Sum();
                        decimal totalprice = Convert.ToDecimal(totalprice1 + totalprice2 + totalprice3);
                        decimal totalpaybletax = (tax * totalprice) / 100;
                        decimal totalpayblamt = Math.Round((totalprice + totalpaybletax), 2);
                        var price = DbContext.tbl_DC_CouponCode.Where(x => x.Is_Default == true && x.Valid_From <= today && x.Valid_To >= today && x.Is_Active == true && x.Is_Delete == false).FirstOrDefault();
                        if (price != null)
                        {
                            if (price.Discount_Price != null)
                            {
                                int totl_pkg = Convert.ToInt32(data.ToList().Count);
                                decimal discountprice = Math.Round(Convert.ToDecimal(price.Discount_Price));
                                decimal prc = Math.Round(Convert.ToDecimal(discountprice));
                                decimal pricee = Math.Round(Convert.ToDecimal(Math.Round(totalprice) - (prc)));
                                decimal totalpaybletax1 = Math.Round(tax * Convert.ToDecimal(pricee) / 100);
                                decimal totalpayblamt1 = Math.Round((Convert.ToDecimal(pricee) + totalpaybletax1));
                                decimal payblamt = Math.Round(totalpayblamt1);
                                decimal taxx = Math.Round((Convert.ToDecimal(payblamt) - Convert.ToDecimal(pricee)));

                                checkout_detail crt_dtl = new checkout_detail();
                                crt_dtl.cartdata = data;
                                crt_dtl.afterdicount = Math.Round(Convert.ToDecimal(pricee));
                                crt_dtl.payblamt = Math.Round(payblamt);
                                crt_dtl.taxx = Math.Round(Convert.ToDecimal(taxx));
                                crt_dtl.Original_price = Math.Round(totalprice);
                                crt_dtl.discountprice = Math.Round(Convert.ToDecimal(discountprice) * Convert.ToInt32(cart_item));
                                crt_dtl.cart_item = Convert.ToInt32(cart_item);
                                crt_dtl.discpercentage = 0;
                                crt_dtl.coupon_code = price.Coupon_Code;
                                //crt_dtl.coupon_id = Convert.ToInt32(price.Code_Id);
                                crt_dtl.Customer_Name = data1.Customer_Name;
                                crt_dtl.Mobile_Number = data1.Mobile;
                                crt_dtl.Email_Id = data1.Email;
                                crt_dtl.Order_Id = Convert.ToInt32(data.ToList()[0].Order_Id);
                                crt_dtl.Order_No = data.ToList()[0].Order_No;
                                crt_dtl.msg = "Coupon " + price.Coupon_Code +" already applied";

                                if (crt_dtl != null)
                                {
                                    var obj = new getcheckoutdata
                                    {
                                        success = new successsss()
                                        {
                                            getcheckoutdata = crt_dtl
                                        }
                                    };
                                    return Request.CreateResponse(HttpStatusCode.OK, obj);
                                }
                                else
                                {
                                    var ob = new Errorresult
                                    {
                                        Error = new Errorresponse
                                        {
                                            Message = "Something went wrong"
                                        }
                                    };
                                    return Request.CreateResponse(HttpStatusCode.OK, ob);
                                }
                            }
                            else
                            {
                                int totl_pkg = Convert.ToInt32(data.ToList().Count);
                                decimal discountprice = Math.Round(Convert.ToDecimal(price.Discount_Percent));
                                decimal prc = Math.Round(Convert.ToDecimal(discountprice));
                                decimal t_price = Math.Round(Convert.ToDecimal((totalprice - totalprice3) * prc) / 100);
                                decimal pricee = Math.Round(totalprice - t_price);
                                decimal totalpaybletax1 = Math.Round((tax * Convert.ToDecimal(pricee) / 100));
                                decimal totalpayblamt1 = Math.Round((Convert.ToDecimal(pricee) + totalpaybletax1));
                                decimal payblamt = Math.Round(totalpayblamt1);
                                decimal taxx = Math.Round((Convert.ToDecimal(payblamt) - Convert.ToDecimal(pricee)));

                                checkout_detail crt_dtl = new checkout_detail();
                                crt_dtl.cartdata = data;
                                crt_dtl.payblamt = Math.Round(payblamt);
                                crt_dtl.Original_price = Math.Round(Convert.ToDecimal(totalprice));
                                crt_dtl.discountprice = Math.Round(Convert.ToDecimal(t_price));
                                crt_dtl.discpercentage = Math.Round(prc);
                                crt_dtl.taxx = Math.Round(Convert.ToDecimal(taxx));
                                crt_dtl.cart_item = Convert.ToInt32(cart_item);
                                crt_dtl.afterdicount = Math.Round(Convert.ToDecimal(pricee));
                                crt_dtl.coupon_code = price.Coupon_Code;
                                //crt_dtl.coupon_id = Convert.ToInt32(price.Code_Id);
                                crt_dtl.Customer_Name = data1.Customer_Name;
                                crt_dtl.Mobile_Number = data1.Mobile;
                                crt_dtl.Email_Id = data1.Email;
                                crt_dtl.Order_Id = Convert.ToInt32(data.ToList()[0].Order_Id);
                                crt_dtl.Order_No = data.ToList()[0].Order_No;
                                crt_dtl.msg = "Coupon " + price.Coupon_Code + " already applied";
                                if (crt_dtl != null)
                                {
                                    var obj = new getcheckoutdata
                                    {
                                        success = new successsss()
                                        {
                                            getcheckoutdata = crt_dtl
                                        }
                                    };
                                    return Request.CreateResponse(HttpStatusCode.OK, obj);
                                }
                                else
                                {
                                    var ob = new Errorresult
                                    {
                                        Error = new Errorresponse
                                        {
                                            Message = "Something went wrong"
                                        }
                                    };
                                    return Request.CreateResponse(HttpStatusCode.OK, ob);
                                }
                            }
                        }
                        else
                        {
                            decimal payblamt = Math.Round(totalpayblamt);
                            decimal pricee = Math.Round(totalprice, 2);
                            decimal taxx = Math.Round((Convert.ToDecimal(payblamt) - Convert.ToDecimal(pricee)), 2);

                            checkout_detail crt_dtl = new checkout_detail();
                            crt_dtl.cartdata = data;
                            crt_dtl.payblamt = Math.Round(payblamt);
                            crt_dtl.Original_price = Math.Round(Convert.ToDecimal(totalprice));
                            crt_dtl.discountprice = 0;
                            crt_dtl.discpercentage = 0;
                            crt_dtl.taxx = Math.Round(Convert.ToDecimal(taxx));
                            crt_dtl.cart_item = Convert.ToInt32(cart_item);
                            crt_dtl.afterdicount = Math.Round(Convert.ToDecimal(totalprice)); ;
                            crt_dtl.coupon_code = "";
                            //crt_dtl.coupon_id = 0;
                            crt_dtl.Customer_Name = data1.Customer_Name;
                            crt_dtl.Mobile_Number = data1.Mobile;
                            crt_dtl.Email_Id = data1.Email;
                            crt_dtl.Order_Id = Convert.ToInt32(data.ToList()[0].Order_Id);
                            crt_dtl.Order_No = data.ToList()[0].Order_No;
                            crt_dtl.msg = "";
                            if (crt_dtl != null)
                            {
                                var obj = new getcheckoutdata
                                {
                                    success = new successsss()
                                    {
                                        getcheckoutdata = crt_dtl
                                    }
                                };
                                return Request.CreateResponse(HttpStatusCode.OK, obj);
                            }
                            else
                            {
                                var ob = new Errorresult
                                {
                                    Error = new Errorresponse
                                    {
                                        Message = "Something went wrong"
                                    }
                                };
                                return Request.CreateResponse(HttpStatusCode.OK, ob);
                            }
                        }
                    }
                    else
                    {
                        decimal totalprice = Convert.ToDecimal(totalprice1);
                        decimal totalpaybletax = (tax * totalprice) / 100;
                        decimal totalpayblamt = Math.Round((totalprice + totalpaybletax), 2);
                        var price = DbContext.tbl_DC_CouponCode.Where(x => x.Is_Default == true && x.Valid_From <= today && x.Valid_To >= today && x.Is_Active == true && x.Is_Delete == false).FirstOrDefault();
                        if (price != null)
                        {
                            if (price.Discount_Price != null)
                            {
                                int totl_pkg = Convert.ToInt32(data.ToList().Count);
                                decimal discountprice = Math.Round(Convert.ToDecimal(price.Discount_Price));
                                decimal prc = Math.Round(Convert.ToDecimal(discountprice));
                                decimal pricee = Math.Round(Convert.ToDecimal(Math.Round(totalprice, 2) - prc));
                                decimal totalpaybletax1 = Math.Round((tax * Convert.ToDecimal(pricee) / 100));
                                decimal totalpayblamt1 = Math.Round((Convert.ToDecimal(pricee) + totalpaybletax1));
                                decimal payblamt = Math.Round(totalpayblamt1);
                                decimal taxx = Math.Round((Convert.ToDecimal(payblamt) - Convert.ToDecimal(pricee)));

                                checkout_detail crt_dtl = new checkout_detail();
                                crt_dtl.cartdata = data;
                                crt_dtl.payblamt = Math.Round(payblamt);
                                crt_dtl.Original_price = Math.Round(Convert.ToDecimal(totalprice));
                                crt_dtl.afterdicount = Math.Round(Convert.ToDecimal(pricee));
                                crt_dtl.discountprice = Math.Round(Convert.ToDecimal(discountprice));
                                crt_dtl.discpercentage = 0;
                                crt_dtl.taxx = Math.Round(Convert.ToDecimal(taxx));
                                crt_dtl.cart_item = Convert.ToInt32(cart_item);

                                crt_dtl.coupon_code = price.Coupon_Code;
                                //crt_dtl.coupon_id = Convert.ToInt32(price.Code_Id);
                                crt_dtl.Customer_Name = data1.Customer_Name;
                                crt_dtl.Mobile_Number = data1.Mobile;
                                crt_dtl.Email_Id = data1.Email;
                                crt_dtl.Order_Id = Convert.ToInt32(data.ToList()[0].Order_Id);
                                crt_dtl.Order_No = data.ToList()[0].Order_No;
                                crt_dtl.msg = "Coupon " + price.Coupon_Code + " already applied";
                                if (crt_dtl != null)
                                {
                                    var obj = new getcheckoutdata
                                    {
                                        success = new successsss()
                                        {
                                            getcheckoutdata = crt_dtl
                                        }
                                    };
                                    return Request.CreateResponse(HttpStatusCode.OK, obj);
                                }
                                else
                                {
                                    var ob = new Errorresult
                                    {
                                        Error = new Errorresponse
                                        {
                                            Message = "Something went wrong"
                                        }
                                    };
                                    return Request.CreateResponse(HttpStatusCode.OK, ob);
                                }
                            }
                            else
                            {
                                int totl_pkg = Convert.ToInt32(data.ToList().Count);
                                decimal discountprice = Math.Round(Convert.ToDecimal(price.Discount_Percent));
                                decimal prc = Math.Round(Convert.ToDecimal(discountprice));
                                decimal t_price = Math.Round(Convert.ToDecimal((Math.Round(totalprice, 2) * prc) / 100));
                                decimal pricee = Math.Round(Convert.ToDecimal(Math.Round(totalprice, 2) - (t_price)));
                                decimal totalpaybletax1 = Math.Round((tax * Convert.ToDecimal(pricee) / 100));
                                decimal totalpayblamt1 = Math.Round((Convert.ToDecimal(pricee) + totalpaybletax1));
                                decimal payblamt = Math.Round(totalpayblamt1);
                                decimal taxx = Math.Round((Convert.ToDecimal(payblamt) - Convert.ToDecimal(pricee)));

                                checkout_detail crt_dtl = new checkout_detail();
                                crt_dtl.cartdata = data;
                                crt_dtl.payblamt = Math.Round(payblamt);
                                crt_dtl.Original_price = Math.Round(Convert.ToDecimal(totalprice));
                                crt_dtl.afterdicount = Math.Round(Convert.ToDecimal(pricee));
                                crt_dtl.discountprice = Math.Round(Convert.ToDecimal(t_price));
                                crt_dtl.discpercentage = Math.Round(prc);
                                crt_dtl.taxx = Math.Round(Convert.ToDecimal(taxx));
                                crt_dtl.cart_item = Convert.ToInt32(cart_item);

                                crt_dtl.coupon_code = price.Coupon_Code;
                                //crt_dtl.coupon_id = Convert.ToInt32(price.Code_Id);
                                crt_dtl.Customer_Name = data1.Customer_Name;
                                crt_dtl.Mobile_Number = data1.Mobile;
                                crt_dtl.Email_Id = data1.Email;
                                crt_dtl.Order_Id = Convert.ToInt32(data.ToList()[0].Order_Id);
                                crt_dtl.Order_No = data.ToList()[0].Order_No;
                                crt_dtl.msg = "Coupon " + price.Coupon_Code + " already applied";
                                if (crt_dtl != null)
                                {
                                    var obj = new getcheckoutdata
                                    {
                                        success = new successsss()
                                        {
                                            getcheckoutdata = crt_dtl
                                        }
                                    };
                                    return Request.CreateResponse(HttpStatusCode.OK, obj);
                                }
                                else
                                {
                                    var ob = new Errorresult
                                    {
                                        Error = new Errorresponse
                                        {
                                            Message = "Something went wrong"
                                        }
                                    };
                                    return Request.CreateResponse(HttpStatusCode.OK, ob);
                                }
                            }
                        }
                        else
                        {
                            decimal payblamt = Math.Round(totalpayblamt, 2);
                            decimal pricee = Math.Round(totalprice, 2);
                            decimal taxx = Math.Round((Convert.ToDecimal(payblamt) - Convert.ToDecimal(pricee)), 2);

                            checkout_detail crt_dtl = new checkout_detail();
                            crt_dtl.cartdata = data;
                            crt_dtl.payblamt = Math.Round(payblamt);
                            crt_dtl.Original_price = Math.Round(Convert.ToDecimal(pricee));
                            crt_dtl.afterdicount = Math.Round(Convert.ToDecimal(pricee));
                            crt_dtl.discountprice = 0;
                            crt_dtl.discpercentage = 0;
                            crt_dtl.taxx = Math.Round(Convert.ToDecimal(taxx));
                            crt_dtl.cart_item = Convert.ToInt32(cart_item);
                            //crt_dtl.afterdicount = 0;
                            crt_dtl.coupon_code = "";
                            //crt_dtl.coupon_id = 0;
                            crt_dtl.Customer_Name = data1.Customer_Name;
                            crt_dtl.Mobile_Number = data1.Mobile;
                            crt_dtl.Email_Id = data1.Email;
                            crt_dtl.Order_Id = Convert.ToInt32(data.ToList()[0].Order_Id);
                            crt_dtl.Order_No = data.ToList()[0].Order_No;
                            crt_dtl.msg = "";
                            if (crt_dtl != null)
                            {
                                var obj = new getcheckoutdata
                                {
                                    success = new successsss()
                                    {
                                        getcheckoutdata = crt_dtl
                                    }
                                };
                                return Request.CreateResponse(HttpStatusCode.OK, obj);
                            }
                            else
                            {
                                var ob = new Errorresult
                                {
                                    Error = new Errorresponse
                                    {
                                        Message = "Something went wrong"
                                    }
                                };
                                return Request.CreateResponse(HttpStatusCode.OK, ob);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var ob = new Errorresult
                {
                    Error = new Errorresponse
                    {
                        Message = "Something went wrong"
                    }
                };
                return Request.CreateResponse(HttpStatusCode.OK, ob);
            }
        }
        [HttpPost]
        public HttpResponseMessage checkout(int id, string eid)
        {
            try
            {
                var price1 = DbContext.tbl_DC_CouponCode.Where(x => x.Coupon_Code == eid && x.Valid_From <= today && x.Valid_To >= today && x.Is_Active == true && x.Is_Delete == false).FirstOrDefault();
                if (price1 != null)
                {
                    var priceee = price1;
                    var cp_code = price1.Coupon_Code;

                    var data1 = DbContext.tbl_DC_Registration.Where(x => x.Regd_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();

                    var data = (from i in
                                    (from a in DbContext.tbl_DC_Registration.Where(x => x.Regd_ID == id && x.Is_Active == true && x.Is_Deleted == false)
                                     join b in DbContext.tbl_DC_Cart.Where(x => x.In_Cart == true && x.Is_Paid == false && x.Is_Active == true && x.Is_Deleted == false)
                                     on a.Regd_ID equals b.Regd_ID
                                     join c in DbContext.tbl_DC_Package.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                     on b.Package_ID equals c.Package_ID
                                     select new CartcheckoutModel
                                     {
                                         Package_Image = c.Thumbnail,
                                         Package_Name = c.Package_Name,
                                         Validity = (int)c.Subscripttion_Period,
                                         Package_ID = c.Package_ID,
                                         Subscripttion_Limit = (int)DbContext.tbl_DC_PackageSub_Dtl.Where(x => x.Package_ID == c.Package_ID).Select(x => x.SubScription_Limit).ToList().Sum(),
                                         Price = Math.Round((decimal)c.Price),
                                         Is_Offline = (Boolean)c.Is_Offline,
                                         Cart_ID = b.Cart_ID,
                                         Order_Id = (int)b.Order_ID,
                                         Order_No = b.Order_No
                                     }).ToList()
                                select new CartcheckoutModel
                                {
                                    Package_Image = i.Package_Image,
                                    Package_Name = i.Package_Name,
                                    Validity = (int)i.Validity,
                                    Package_ID = i.Package_ID,
                                    Subscripttion_Limit = (int)DbContext.tbl_DC_PackageSub_Dtl.Where(x => x.Package_ID == i.Package_ID).Select(x => x.SubScription_Limit).ToList().Sum(),
                                    Price = Math.Round((decimal)i.Price),
                                    Package_Price = Math.Round(price(i.Package_ID)),
                                    Discounted_Price = Math.Round(Discounted_Price(i.Package_ID, eid)),
                                    Is_Offline = (Boolean)i.Is_Offline,
                                    Cart_ID = i.Cart_ID,
                                    Order_Id = i.Order_Id,
                                    Order_No = i.Order_No,
                                    discountpercentage = Convert.ToDecimal(price1.Discount_Percent)
                                }).ToList();

                    if (data.Count == 0)
                    {
                        var ob = new Errorresult
                        {
                            Error = new Errorresponse
                            {
                                Message = "Please provide data correctly"
                            }
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, ob);
                    }
                    else
                    {
                        int ca_id = Convert.ToInt32(data.ToList()[0].Cart_ID);
                        tbl_DC_Cart objjj = DbContext.tbl_DC_Cart.Where(x => x.Cart_ID == ca_id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                        objjj.Coupon_Code = eid;
                        DbContext.Entry(objjj).State = EntityState.Modified;
                        DbContext.SaveChanges();

                        var dataa = data;
                        decimal tax = taxcalculate();
                        decimal tax1 = (decimal)tax;
                        int cart_item = Convert.ToInt32(data.Count);
                        decimal totalprice1 = data.ToList().Where(x => x.Is_Offline == false || x.Is_Offline == null).Select(c => (decimal)c.Price).Sum();

                        var data2 = (from x in data.ToList().Where(x => x.Is_Offline == true)
                                     join y in DbContext.tbl_DC_Package_Period.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                     on x.Package_ID equals y.Package_ID
                                     select new DigiChampCartModel
                                     {
                                         Is_Offline = x.Is_Offline,
                                         Package_ID = x.Package_ID,
                                         Price = x.Price,
                                         Excluded_Price = y.Excluded_Price
                                     }).ToList();

                        if (data2.Count > 0)
                        {
                            decimal totalprice2 = data2.ToList().Where(x => x.Is_Offline == true).Select(c => (decimal)c.Price).Sum();
                            decimal totalprice3 = data2.ToList().Where(x => x.Is_Offline == true).Select(c => (decimal)c.Excluded_Price).Sum();
                            decimal totalprice = Convert.ToDecimal(totalprice1 + totalprice2);
                            decimal totalpaybletax = (tax * totalprice) / 100;
                            decimal totalpayblamt = Math.Round((totalprice + totalpaybletax + totalprice3), 2);
                            var price = DbContext.tbl_DC_CouponCode.Where(x => x.Coupon_Code == eid && x.Valid_From <= today && x.Valid_To >= today && x.Is_Active == true && x.Is_Delete == false).FirstOrDefault();
                            if (price != null)
                            {
                                if (price.Discount_Price != null)
                                {
                                    int totl_pkg = Convert.ToInt32(data.ToList().Count);
                                    decimal discountprice = Math.Round(Convert.ToDecimal(price.Discount_Price * totl_pkg));
                                    decimal prc = Math.Round(Convert.ToDecimal(discountprice));
                                    decimal pricee = Math.Round(Convert.ToDecimal(Math.Round(totalprice + totalprice3) - (prc)));
                                    decimal totalpaybletax1 = Math.Round(tax * Convert.ToDecimal(pricee) / 100);
                                    decimal totalpayblamt1 = Math.Round((Convert.ToDecimal(pricee) + totalpaybletax1 + totalprice3));
                                    decimal payblamt = Math.Round(totalpayblamt1);
                                    decimal taxx = Math.Round((Convert.ToDecimal(payblamt) - Convert.ToDecimal(pricee)));

                                    checkout_detail crt_dtl = new checkout_detail();
                                    crt_dtl.cartdata = data;
                                    crt_dtl.payblamt = Math.Round(payblamt);
                                    crt_dtl.taxx = Math.Round(Convert.ToDecimal(taxx));
                                    crt_dtl.Original_price = Math.Round(Convert.ToDecimal(totalprice + totalprice3));
                                    crt_dtl.afterdicount = Math.Round(Convert.ToDecimal(pricee));
                                    crt_dtl.discountprice = Math.Round(Convert.ToDecimal(discountprice));
                                    crt_dtl.discpercentage = 0;
                                    crt_dtl.cart_item = Convert.ToInt32(cart_item);

                                    crt_dtl.coupon_code = price.Coupon_Code;
                                    //crt_dtl.coupon_id = Convert.ToInt32(price.Code_Id);
                                    crt_dtl.Customer_Name = data1.Customer_Name;
                                    crt_dtl.Mobile_Number = data1.Mobile;
                                    crt_dtl.Email_Id = data1.Email;
                                    crt_dtl.Order_Id = Convert.ToInt32(data.ToList()[0].Order_Id);
                                    crt_dtl.Order_No = data.ToList()[0].Order_No;
                                    crt_dtl.msg = "Coupon applied successfully";
                                    if (crt_dtl != null)
                                    {
                                        var obj = new getcheckoutdata
                                        {
                                            success = new successsss()
                                            {
                                                getcheckoutdata = crt_dtl
                                            }
                                        };
                                        return Request.CreateResponse(HttpStatusCode.OK, obj);
                                    }
                                    else
                                    {
                                        var ob = new Errorresult
                                        {
                                            Error = new Errorresponse
                                            {
                                                Message = "Something went wrong"
                                            }
                                        };
                                        return Request.CreateResponse(HttpStatusCode.OK, ob);
                                    }
                                }
                                else
                                {
                                    int totl_pkg = Convert.ToInt32(data.ToList().Count);
                                    decimal discountprice = Math.Round(Convert.ToDecimal(price.Discount_Percent));
                                    decimal prc = Math.Round(Convert.ToDecimal(discountprice));
                                    decimal t_price = Math.Round(Convert.ToDecimal((Math.Round(totalprice, 2) * prc) / 100));
                                    decimal pricee = Math.Round(Convert.ToDecimal(Math.Round(totalprice + totalprice3) - t_price));
                                    decimal totalpaybletax1 = Math.Round((tax * Convert.ToDecimal(pricee) / 100));
                                    decimal totalpayblamt1 = Math.Round((Convert.ToDecimal(pricee) + totalpaybletax1));
                                    decimal payblamt = Math.Round(totalpayblamt1);
                                    decimal taxx = Math.Round((Convert.ToDecimal(payblamt) - Convert.ToDecimal(pricee)));

                                    checkout_detail crt_dtl = new checkout_detail();
                                    crt_dtl.cartdata = data;
                                    crt_dtl.payblamt = Math.Round(payblamt);
                                    crt_dtl.Original_price = Math.Round(Convert.ToDecimal(totalprice + totalprice3));
                                    crt_dtl.afterdicount = Math.Round(Convert.ToDecimal(pricee));
                                    //crt_dtl.discountprice = 0;
                                    crt_dtl.discountprice = Math.Round(Convert.ToDecimal(t_price));
                                    crt_dtl.discpercentage = Math.Round(prc);
                                    crt_dtl.taxx = Math.Round(Convert.ToDecimal(taxx));
                                    crt_dtl.cart_item = Convert.ToInt32(cart_item);

                                    crt_dtl.coupon_code = price.Coupon_Code;
                                    //crt_dtl.coupon_id = Convert.ToInt32(price.Code_Id);
                                    crt_dtl.Customer_Name = data1.Customer_Name;
                                    crt_dtl.Mobile_Number = data1.Mobile;
                                    crt_dtl.Email_Id = data1.Email;
                                    crt_dtl.Order_Id = Convert.ToInt32(data.ToList()[0].Order_Id);
                                    crt_dtl.Order_No = data.ToList()[0].Order_No;
                                    crt_dtl.msg = "Coupon applied successfully";
                                    if (crt_dtl != null)
                                    {
                                        var obj = new getcheckoutdata
                                        {
                                            success = new successsss()
                                            {
                                                getcheckoutdata = crt_dtl
                                            }
                                        };
                                        return Request.CreateResponse(HttpStatusCode.OK, obj);
                                    }
                                    else
                                    {
                                        var ob = new Errorresult
                                        {
                                            Error = new Errorresponse
                                            {
                                                Message = "Something went wrong"
                                            }
                                        };
                                        return Request.CreateResponse(HttpStatusCode.OK, ob);
                                    }
                                }
                            }
                            else
                            {
                                decimal payblamt = totalpayblamt;
                                decimal pricee = Math.Round(totalprice, 2);
                                decimal taxx = Math.Round((Convert.ToDecimal(payblamt) - Convert.ToDecimal(pricee)), 2);

                                checkout_detail crt_dtl = new checkout_detail();
                                crt_dtl.cartdata = data;
                                crt_dtl.payblamt = Math.Round(payblamt);
                                crt_dtl.Original_price = Math.Round(Convert.ToDecimal(totalprice + totalprice3));
                                crt_dtl.discountprice = 0;
                                crt_dtl.taxx = Math.Round(Convert.ToDecimal(taxx));
                                crt_dtl.cart_item = Convert.ToInt32(cart_item);
                                crt_dtl.afterdicount = 0;
                                crt_dtl.coupon_code = "";
                                //crt_dtl.coupon_id = 0;
                                crt_dtl.Customer_Name = data1.Customer_Name;
                                crt_dtl.Mobile_Number = data1.Mobile;
                                crt_dtl.Email_Id = data1.Email;
                                crt_dtl.Order_Id = Convert.ToInt32(data.ToList()[0].Order_Id);
                                crt_dtl.Order_No = data.ToList()[0].Order_No;
                                crt_dtl.msg = "";
                                if (crt_dtl != null)
                                {
                                    var obj = new getcheckoutdata
                                    {
                                        success = new successsss()
                                        {
                                            getcheckoutdata = crt_dtl
                                        }
                                    };
                                    return Request.CreateResponse(HttpStatusCode.OK, obj);
                                }
                                else
                                {
                                    var ob = new Errorresult
                                    {
                                        Error = new Errorresponse
                                        {
                                            Message = "Something went wrong"
                                        }
                                    };
                                    return Request.CreateResponse(HttpStatusCode.OK, ob);
                                }
                            }
                        }
                        else
                        {
                            decimal totalprice = Convert.ToDecimal(totalprice1);
                            decimal totalpaybletax = (tax * totalprice) / 100;
                            decimal totalpayblamt = Math.Round((totalprice + totalpaybletax), 2);
                            var price = DbContext.tbl_DC_CouponCode.Where(x => x.Coupon_Code == eid && x.Valid_From <= today && x.Valid_To >= today && x.Is_Active == true && x.Is_Delete == false).FirstOrDefault();
                            if (price != null)
                            {
                                if (price.Discount_Price != null)
                                {
                                    int totl_pkg = Convert.ToInt32(data.ToList().Count);
                                    decimal discountprice = Math.Round(Convert.ToDecimal(price.Discount_Price * totl_pkg));
                                    decimal prc = Math.Round(Convert.ToDecimal(discountprice));
                                    decimal pricee = Math.Round(Convert.ToDecimal(Math.Round(totalprice, 2) - (prc * totl_pkg)));
                                    decimal totalpaybletax1 = Math.Round((tax * Convert.ToDecimal(pricee) / 100));
                                    decimal totalpayblamt1 = Math.Round((Convert.ToDecimal(pricee) + totalpaybletax1));
                                    decimal payblamt = Math.Round(totalpayblamt1);
                                    decimal taxx = Math.Round((Convert.ToDecimal(payblamt) - Convert.ToDecimal(pricee)));

                                    checkout_detail crt_dtl = new checkout_detail();
                                    crt_dtl.cartdata = data;
                                    crt_dtl.payblamt = Math.Round(payblamt);
                                    crt_dtl.Original_price = Math.Round(Convert.ToDecimal(totalprice));
                                    crt_dtl.afterdicount = Math.Round(Convert.ToDecimal(pricee));
                                    crt_dtl.discountprice = Math.Round(Convert.ToDecimal(discountprice));
                                    crt_dtl.discpercentage = 0;
                                    crt_dtl.taxx = Math.Round(Convert.ToDecimal(taxx));
                                    crt_dtl.cart_item = Convert.ToInt32(cart_item);

                                    crt_dtl.coupon_code = price.Coupon_Code;
                                    //crt_dtl.coupon_id = Convert.ToInt32(price.Code_Id);
                                    crt_dtl.Customer_Name = data1.Customer_Name;
                                    crt_dtl.Mobile_Number = data1.Mobile;
                                    crt_dtl.Email_Id = data1.Email;
                                    crt_dtl.Order_Id = Convert.ToInt32(data.ToList()[0].Order_Id);
                                    crt_dtl.Order_No = data.ToList()[0].Order_No;
                                    crt_dtl.msg = "Coupon applied successfully";
                                    if (crt_dtl != null)
                                    {
                                        var obj = new getcheckoutdata
                                        {
                                            success = new successsss()
                                            {
                                                getcheckoutdata = crt_dtl
                                            }
                                        };
                                        return Request.CreateResponse(HttpStatusCode.OK, obj);
                                    }
                                    else
                                    {
                                        var ob = new Errorresult
                                        {
                                            Error = new Errorresponse
                                            {
                                                Message = "Something went wrong"
                                            }
                                        };
                                        return Request.CreateResponse(HttpStatusCode.OK, ob);
                                    }
                                }
                                else
                                {
                                    int totl_pkg = Convert.ToInt32(data.ToList().Count);
                                    decimal discountprice = Math.Round(Convert.ToDecimal(price.Discount_Percent));
                                    decimal prc = Math.Round(Convert.ToDecimal(discountprice));
                                    decimal t_price = Math.Round(Convert.ToDecimal((Math.Round(totalprice, 2) * prc) / 100));
                                    decimal pricee = Math.Round(Convert.ToDecimal(Math.Round(totalprice, 2) - t_price));
                                    decimal totalpaybletax1 = Math.Round((tax * Convert.ToDecimal(pricee) / 100));
                                    decimal totalpayblamt1 = Math.Round((Convert.ToDecimal(pricee) + totalpaybletax1));
                                    decimal payblamt = Math.Round(totalpayblamt1);
                                    decimal taxx = Math.Round((Convert.ToDecimal(payblamt) - Convert.ToDecimal(pricee)));

                                    checkout_detail crt_dtl = new checkout_detail();
                                    crt_dtl.cartdata = data;
                                    crt_dtl.payblamt = Math.Round(payblamt);
                                    crt_dtl.Original_price = Math.Round(Convert.ToDecimal(totalprice));
                                    crt_dtl.afterdicount = Math.Round(Convert.ToDecimal(pricee));
                                    //crt_dtl.discountprice = 0;
                                    crt_dtl.discountprice = Math.Round(Convert.ToDecimal(t_price));
                                    crt_dtl.discpercentage = Math.Round(prc);
                                    crt_dtl.taxx = Math.Round(Convert.ToDecimal(taxx));
                                    crt_dtl.cart_item = Convert.ToInt32(cart_item);

                                    crt_dtl.coupon_code = price.Coupon_Code;
                                    //crt_dtl.coupon_id = Convert.ToInt32(price.Code_Id);
                                    crt_dtl.Customer_Name = data1.Customer_Name;
                                    crt_dtl.Mobile_Number = data1.Mobile;
                                    crt_dtl.Email_Id = data1.Email;
                                    crt_dtl.Order_Id = Convert.ToInt32(data.ToList()[0].Order_Id);
                                    crt_dtl.Order_No = data.ToList()[0].Order_No;
                                    crt_dtl.msg = "Coupon applied successfully";
                                    if (crt_dtl != null)
                                    {
                                        var obj = new getcheckoutdata
                                        {
                                            success = new successsss()
                                            {
                                                getcheckoutdata = crt_dtl
                                            }
                                        };
                                        return Request.CreateResponse(HttpStatusCode.OK, obj);
                                    }
                                    else
                                    {
                                        var ob = new Errorresult
                                        {
                                            Error = new Errorresponse
                                            {
                                                Message = "Something went wrong"
                                            }
                                        };
                                        return Request.CreateResponse(HttpStatusCode.OK, ob);
                                    }
                                }
                            }
                            else
                            {
                                decimal payblamt = Math.Round(totalpayblamt, 2);
                                decimal pricee = Math.Round(totalprice, 2);
                                decimal taxx = Math.Round((Convert.ToDecimal(payblamt) - Convert.ToDecimal(pricee)), 2);

                                checkout_detail crt_dtl = new checkout_detail();
                                crt_dtl.cartdata = data;
                                crt_dtl.payblamt = Math.Round(payblamt);
                                crt_dtl.Original_price = Math.Round(Convert.ToDecimal(pricee));
                                crt_dtl.discountprice = 0;
                                crt_dtl.taxx = Math.Round(Convert.ToDecimal(taxx));
                                crt_dtl.cart_item = Convert.ToInt32(cart_item);
                                crt_dtl.afterdicount = 0;
                                crt_dtl.coupon_code = "";
                                //crt_dtl.coupon_id = 0;
                                crt_dtl.Customer_Name = data1.Customer_Name;
                                crt_dtl.Mobile_Number = data1.Mobile;
                                crt_dtl.Email_Id = data1.Email;
                                crt_dtl.Order_Id = Convert.ToInt32(data.ToList()[0].Order_Id);
                                crt_dtl.Order_No = data.ToList()[0].Order_No;
                                crt_dtl.msg = "";
                                if (crt_dtl != null)
                                {
                                    var obj = new getcheckoutdata
                                    {
                                        success = new successsss()
                                        {
                                            getcheckoutdata = crt_dtl
                                        }
                                    };
                                    return Request.CreateResponse(HttpStatusCode.OK, obj);
                                }
                                else
                                {
                                    var ob = new Errorresult
                                    {
                                        Error = new Errorresponse
                                        {
                                            Message = "Something went wrong"
                                        }
                                    };
                                    return Request.CreateResponse(HttpStatusCode.OK, ob);
                                }
                            }
                        }
                    }
                }
                else
                {
                    var ob = new Errorresult
                    {
                        Error = new Errorresponse
                        {
                            Message = "Invalid coupon"
                        }
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, ob);
                }
            }
            catch (Exception ex)
            {
                var ob = new Errorresult
                {
                    Error = new Errorresponse
                    {
                        Message = "Something went wrong"
                    }
                };
                return Request.CreateResponse(HttpStatusCode.OK, ob);
            }
        }
        public decimal Discounted_Price(int id, string ccode)
        {
            DateTime today = DigiChamps.Models.DigiChampsModel.datetoserver();
            decimal price1 = 0;
            tbl_DC_CouponCode disc_prc = new tbl_DC_CouponCode();
            DigiChamps.Models.DigiChampsEntities dc = new DigiChamps.Models.DigiChampsEntities();
            var dt1 = dc.tbl_DC_Package.Where(x => x.Package_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
            var dt = dc.tbl_DC_Package_Period.Where(x => x.Package_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
            if (ccode == null)
            {
                disc_prc = dc.tbl_DC_CouponCode.Where(x => x.Is_Default == true && x.Valid_From <= today && x.Valid_To >= today && x.Is_Active == true && x.Is_Delete == false).FirstOrDefault();
            }
            else
            {
                disc_prc = dc.tbl_DC_CouponCode.Where(x => x.Coupon_Code == ccode && x.Valid_From <= today && x.Valid_To >= today && x.Is_Active == true && x.Is_Delete == false).FirstOrDefault();
            }

            if (disc_prc != null)
            {
                if (dt != null && dt1 != null)
                {
                    if (disc_prc.Discount_Price != null)
                    {
                        decimal price = Convert.ToDecimal(dt1.Price);
                        if (price > disc_prc.Discount_Price)
                        {
                            decimal price2 = Convert.ToDecimal(price - Convert.ToDecimal(disc_prc.Discount_Price));
                            price1 = Convert.ToDecimal(price2 + dt.Excluded_Price);
                        }
                        else
                        {
                            decimal price2 = Convert.ToDecimal(dt.Excluded_Price + dt1.Price);
                            price1 = price2;
                        }
                    }
                    else
                    {
                        decimal price = Convert.ToDecimal(dt1.Price);
                        decimal disc = Convert.ToDecimal(disc_prc.Discount_Percent);
                        if (disc_prc.Discount_Percent != null)
                        {
                            decimal disc_price2 = Convert.ToDecimal((price * Convert.ToDecimal(disc)) / 100);
                            decimal price2 = Convert.ToDecimal(price - Convert.ToDecimal(disc_price2));
                            price1 = Convert.ToDecimal(price2 + dt.Excluded_Price);
                        }
                        else
                        {
                            decimal price2 = Convert.ToDecimal(price);
                            price1 = price2;
                        }
                    }
                }
                else
                {
                    if (disc_prc.Discount_Price != null)
                    {
                        decimal price = Convert.ToDecimal(dt1.Price);
                        if (price > disc_prc.Discount_Price)
                        {
                            decimal price2 = Convert.ToDecimal(dt1.Price - Convert.ToDecimal(disc_prc.Discount_Price));
                            price1 = price2;
                        }
                        else
                        {
                            decimal price2 = Convert.ToDecimal(dt1.Price);
                            price1 = price2;
                        }
                    }
                    else
                    {
                        decimal price = Convert.ToDecimal(dt1.Price);
                        decimal disc = Convert.ToDecimal(disc_prc.Discount_Percent);
                        if (disc_prc.Discount_Percent != null)
                        {
                            decimal disc_price2 = Convert.ToDecimal((price * Convert.ToDecimal(disc)) / 100);
                            decimal price2 = Convert.ToDecimal(price - Convert.ToDecimal(disc_price2));
                            price1 = price2;
                        }
                        else
                        {
                            decimal price2 = Convert.ToDecimal(dt1.Price);
                            price1 = price2;
                        }
                    }
                }
            }
            else
            {
                if (dt != null && dt1 != null)
                {
                    decimal price2 = Convert.ToDecimal(dt.Excluded_Price + dt1.Price);
                    price1 = price2;
                }
                else
                {
                    decimal price2 = Convert.ToDecimal(dt1.Price);
                    price1 = price2;
                }

            }
            return price1;
        }

        [HttpPost]
        public HttpResponseMessage CancelCoupon(int id)
        {
            try
            {
                decimal tax = taxcalculate();
                var data1 = DbContext.tbl_DC_Registration.Where(x => x.Regd_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();

                var data = (from i in
                                (from a in DbContext.tbl_DC_Registration.Where(x => x.Regd_ID == id && x.Is_Active == true && x.Is_Deleted == false)
                                 join b in DbContext.tbl_DC_Cart.Where(x => x.In_Cart == true && x.Is_Paid == false && x.Is_Active == true && x.Is_Deleted == false)
                                 on a.Regd_ID equals b.Regd_ID
                                 join c in DbContext.tbl_DC_Package.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                 on b.Package_ID equals c.Package_ID
                                 select new CartcheckoutModel
                                 {
                                     Package_Name = c.Package_Name,
                                     Validity = (int)c.Subscripttion_Period,
                                     Package_ID = c.Package_ID,
                                     Subscripttion_Limit = (int)DbContext.tbl_DC_PackageSub_Dtl.Where(x => x.Package_ID == c.Package_ID).Select(x => x.SubScription_Limit).ToList().Sum(),
                                     Price = (decimal)c.Price,
                                     Is_Offline = (Boolean)c.Is_Offline,
                                     Cart_ID = b.Cart_ID,
                                     Order_Id = (int)b.Order_ID,
                                     Order_No = b.Order_No
                                 }).ToList()
                            select new CartcheckoutModel
                            {
                                Package_Name = i.Package_Name,
                                Validity = (int)i.Validity,
                                Package_ID = i.Package_ID,
                                Subscripttion_Limit = (int)DbContext.tbl_DC_PackageSub_Dtl.Where(x => x.Package_ID == i.Package_ID).Select(x => x.SubScription_Limit).ToList().Sum(),
                                Price = (decimal)i.Price,
                                Package_Price = price(i.Package_ID),
                                Discounted_Price = Discounted_Price(i.Package_ID),
                                Is_Offline = (Boolean)i.Is_Offline,
                                Cart_ID = i.Cart_ID,
                                Order_Id = i.Order_Id,
                                Order_No = i.Order_No
                            }).ToList();
                decimal totalprice1 = data.ToList().Where(x => x.Is_Offline == false || x.Is_Offline == null).Select(c => (decimal)c.Price).Sum();

                var data2 = (from x in data.ToList().Where(x => x.Is_Offline == true)
                             join y in DbContext.tbl_DC_Package_Period.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                             on x.Package_ID equals y.Package_ID
                             select new DigiChampCartModel
                             {
                                 Is_Offline = x.Is_Offline,
                                 Package_ID = x.Package_ID,
                                 Price = x.Price,
                                 Excluded_Price = y.Excluded_Price
                             }).ToList();
                if (data2.Count > 0)
                {
                    decimal totalprice2 = data2.ToList().Where(x => x.Is_Offline == true).Select(c => (decimal)c.Price).Sum();
                    decimal totalprice3 = data2.ToList().Where(x => x.Is_Offline == true).Select(c => (decimal)c.Excluded_Price).Sum();
                    decimal pricee = Math.Round(Convert.ToDecimal(totalprice1 + totalprice2 + totalprice3), 2);
                    decimal taxx = Math.Round(((tax * pricee) / 100), 2);
                    decimal payblamt = Math.Round((pricee + taxx), 2);

                    checkout_detail crt_dtl = new checkout_detail();
                    crt_dtl.cartdata = data;
                    crt_dtl.payblamt = payblamt;
                    crt_dtl.taxx = Convert.ToDecimal(taxx);
                    crt_dtl.Original_price = Convert.ToDecimal(pricee);
                    crt_dtl.discountprice = 0;
                    crt_dtl.cart_item = Convert.ToInt32(data.ToList().Count);

                    crt_dtl.Customer_Name = data1.Customer_Name;
                    crt_dtl.Mobile_Number = data1.Mobile;
                    crt_dtl.Email_Id = data1.Email;
                    crt_dtl.Order_Id = Convert.ToInt32(data.ToList()[0].Order_Id);
                    crt_dtl.Order_No = data.ToList()[0].Order_No;

                    if (crt_dtl != null)
                    {
                        var obj = new getcheckoutdata
                        {
                            success = new successsss()
                            {
                                getcheckoutdata = crt_dtl
                            }
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, obj);
                    }
                    else
                    {
                        var ob = new Errorresult
                        {
                            Error = new Errorresponse
                            {
                                Message = "Something went wrong"
                            }
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, ob);
                    }
                }
                else
                {
                    decimal pricee = Math.Round(Convert.ToDecimal(totalprice1), 2);
                    decimal taxx = Math.Round(((tax * pricee) / 100), 2);
                    decimal payblamt = Math.Round((pricee + taxx), 2);

                    checkout_detail crt_dtl = new checkout_detail();
                    crt_dtl.cartdata = data;
                    crt_dtl.payblamt = payblamt;
                    crt_dtl.taxx = Convert.ToDecimal(taxx);
                    crt_dtl.Original_price = Convert.ToDecimal(pricee);

                    crt_dtl.Customer_Name = data1.Customer_Name;
                    crt_dtl.Mobile_Number = data1.Mobile;
                    crt_dtl.Email_Id = data1.Email;
                    crt_dtl.Order_Id = Convert.ToInt32(data.ToList()[0].Order_Id);
                    crt_dtl.Order_No = data.ToList()[0].Order_No;

                    if (crt_dtl != null)
                    {
                        var obj = new getcheckoutdata
                        {
                            success = new successsss()
                            {
                                getcheckoutdata = crt_dtl
                            }
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, obj);
                    }
                    else
                    {
                        var ob = new Errorresult
                        {
                            Error = new Errorresponse
                            {
                                Message = "Something went wrong"
                            }
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, ob);
                    }
                }
            }
            catch (Exception)
            {
                var ob = new Errorresult
                {
                    Error = new Errorresponse
                    {
                        Message = "Something went wrong"
                    }
                };
                return Request.CreateResponse(HttpStatusCode.OK, ob);
            }
        }                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                     
        public class CartModel1
        {
            public string Package_Name { get; set; }
            public int Subscripttion_Period { get; set; }
            public int Package_Id { get; set; }
            public Nullable<decimal> Price { get; set; }
            public Boolean Is_Offline { get; set; }
            public decimal Exculde_Price { get; set; }

        }
        public class order_conf_model
        {
            public List<CartModel1> cartdata { get; set; }
            public decimal price { get; set; }
            public decimal payble_price { get; set; }
            public decimal tax_percent { get; set; }
            public decimal tax_amount { get; set; }
            public int total_item { get; set; }
            public string purchase_date { get; set; }
            public int Order_Id { get; set; }
            public string Order_No { get; set; }
        }
        public class successssss
        {
            public order_conf_model get_confdata { get; set; }
        }
        public class successmsg
        {
            public string Message { get; set; }
        }
        public class getconfirmation
        {
            public successssss success { get; set; }
        }
        [HttpGet]
        public HttpResponseMessage Order_Confirmation(int id, int eid, string ClsId)
        {
            try
            {
                var register = DbContext.tbl_DC_Registration.Where(x => x.Regd_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                var cartt = DbContext.tbl_DC_Cart.Where(x => x.Order_ID == eid && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                string disc_amt = string.Empty;
                decimal disc = 0;
                decimal discA = 0;
                string u_name = register.Mobile;
                int ordid = Convert.ToInt32(eid);
                string ordno = cartt.Order_No.ToString();

                var data2 = (from a in DbContext.tbl_DC_Registration.Where(x => x.Mobile == u_name && x.Is_Active == true && x.Is_Deleted == false)
                             join b in DbContext.tbl_DC_Cart.Where(x => x.Status == true && x.In_Cart == true && x.Is_Paid == false && x.Is_Active == true && x.Is_Deleted == false)
                             on a.Regd_ID equals b.Regd_ID
                             select new DigiChampCartModel
                             {
                                 firstname = a.Customer_Name,
                                 Regd_ID = a.Regd_ID,
                                 email = a.Email,
                                 Regd_No = a.Regd_No,
                                 Cart_ID = b.Cart_ID,
                                 Order_ID = b.Order_ID,
                                 Order_No = b.Order_No
                             }).ToList();

                int regid = Convert.ToInt32(id);

                var pre_b = DbContext.tbl_DC_Pre_book.Where(x => x.Regd_Id == regid && x.Ord_Status == false).FirstOrDefault();
                if (pre_b != null)
                {
                    tbl_DC_Pre_book pre_book = DbContext.tbl_DC_Pre_book.Where(x => x.Regd_Id == regid).FirstOrDefault();
                    pre_book.Ord_Status = true;
                    pre_book.Order_Id = ordid;
                    DbContext.Entry(pre_book).State = EntityState.Modified;
                    DbContext.SaveChanges();
                }
                string regno = data2.ToList()[0].Regd_No;

                var data3 = (from e in DbContext.tbl_DC_Cart.Where(x => x.Order_ID == ordid && x.In_Cart == true && x.Is_Paid == false && x.Is_Active == true && x.Is_Deleted == false)
                             join f in DbContext.tbl_DC_Cart_Dtl.Where(x => x.Status == true && x.Is_Active == true && x.Is_Deleted == false)
                             on e.Cart_ID equals f.Cart_ID
                             join a in DbContext.tbl_DC_Chapter.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                             on f.Chapter_ID equals a.Chapter_Id
                             join b in DbContext.tbl_DC_Subject.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                             on a.Subject_Id equals b.Subject_Id
                             join c in DbContext.tbl_DC_Class.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                             on b.Class_Id equals c.Class_Id
                             join d in DbContext.tbl_DC_Board.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                             on c.Board_Id equals d.Board_Id
                             select new DigiChampCartModel
                             {
                                 Regd_ID = e.Regd_ID,
                                 Regd_No = e.Regd_No,
                                 Cart_ID = e.Cart_ID,
                                 Order_ID = e.Order_ID,
                                 Order_No = e.Order_No,
                                 Board_ID = f.Board_ID,
                                 Board_Name = d.Board_Name,
                                 Class_ID = f.Class_ID,
                                 Class_Name = c.Class_Name,
                                 Subject_ID = f.Subject_ID,
                                 Subject = b.Subject,
                                 Chapter_ID = f.Chapter_ID,
                                 Chapter = a.Chapter
                             }).ToList();

                var data31 = DbContext.tbl_DC_Cart.Where(x => x.Order_ID == ordid && x.In_Cart == true && x.Is_Paid == false && x.Is_Active == true && x.Is_Deleted == false).ToList();
                var data1 = DbContext.tbl_DC_Registration.Where(x => x.Mobile == u_name && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();

                var data = (from a in DbContext.tbl_DC_Registration.Where(x => x.Mobile == u_name && x.Is_Active == true && x.Is_Deleted == false)
                            join b in DbContext.tbl_DC_Cart.Where(x => x.In_Cart == true && x.Is_Paid == false && x.Is_Active == true && x.Is_Deleted == false)
                             on a.Regd_ID equals b.Regd_ID
                            join c in DbContext.tbl_DC_Package.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                on b.Package_ID equals c.Package_ID
                            select new CartModel1
                            {
                                Package_Name = c.Package_Name,
                                Subscripttion_Period = (int)c.Subscripttion_Period,
                                Price = c.Price,
                                Package_Id = c.Package_ID,
                                Is_Offline = (Boolean)c.Is_Offline
                            }).ToList();
                var data11 = (from x in data.ToList().Where(x => x.Is_Offline == true)
                              join y in DbContext.tbl_DC_Package_Period.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                              on x.Package_Id equals y.Package_ID
                              select new DigiChampCartModel
                              {
                                  Is_Offline = x.Is_Offline,
                                  Package_ID = x.Package_Id,
                                  Price = x.Price,
                                  Excluded_Price = y.Excluded_Price
                              }).ToList();

                if (data.Count == 0)
                {
                    var ob = new Errorresult
                    {
                        Error = new Errorresponse
                        {
                            Message = "Your cart is empty"
                        }
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, ob);
                }
                else
                {
                    //ViewBag.cartitems = data;
                    decimal tax = taxcalculate();
                    int total_item = Convert.ToInt32(data.Count);
                    decimal totalprice = data.ToList().Select(c => (decimal)c.Price).Sum();
                    decimal totalpaybletax = (tax * totalprice) / 100;
                    decimal totalpayblamt = Math.Round((totalprice + totalpaybletax), 2);
                }
                var price = DbContext.tbl_DC_CouponCode.Where(x => x.Is_Default == true && x.Valid_From <= today && x.Valid_To >= today && x.Is_Active == true && x.Is_Delete == false).FirstOrDefault();
                decimal totalprice1 = 0;
                decimal totalpaybletax1 = 0;
                decimal totalpayblamt1 = 0;

                var coupon = DbContext.tbl_DC_Cart.Where(x => x.Order_ID == eid && x.Is_Active == true && x.Is_Deleted == false && x.Coupon_Code != null).FirstOrDefault();
                if (price == null && coupon == null)
                {
                    decimal taxxx1 = taxcalculate();
                    decimal totalprice3 = data11.ToList().Where(x => x.Is_Offline == true).Select(c => (decimal)c.Excluded_Price).Sum();
                    totalprice1 = Math.Round(Convert.ToDecimal(data.ToList().Select(c => (decimal)c.Price).Sum()) + totalprice3);
                    totalpaybletax1 = (taxxx1 * totalprice1) / 100;
                    totalpayblamt1 = Math.Round((totalprice1 + totalpaybletax1), 2);
                }
                else
                {
                    if (coupon != null)
                    {
                        string co_code = coupon.Coupon_Code;
                        var price1 = DbContext.tbl_DC_CouponCode.Where(x => x.Coupon_Code == co_code && x.Valid_From <= today && x.Valid_To >= today && x.Is_Active == true && x.Is_Delete == false).FirstOrDefault();
                        if (price1 != null)
                        {
                            if (price1.Discount_Price != null)
                            {
                                decimal taxxx1 = taxcalculate();
                                int ttl_item = Convert.ToInt32(data.Count);
                                decimal totalprice3 = data11.ToList().Where(x => x.Is_Offline == true).Select(c => (decimal)c.Excluded_Price).Sum();
                                decimal total_price1 = Math.Round((Convert.ToDecimal(data.ToList().Select(c => (decimal)c.Price).Sum()) + totalprice3) - (Convert.ToDecimal(ttl_item* price.Discount_Price)));
                                discA = Convert.ToDecimal(ttl_item * price.Discount_Price);
                                decimal total_paybletax1 = (taxxx1 * total_price1) / 100;
                                decimal total_payblamt1 = Math.Round((total_price1 + total_paybletax1), 2);
                                totalprice1 = total_price1;
                                totalpaybletax1 = total_paybletax1;
                                totalpayblamt1 = total_payblamt1;
                            }
                            else if (price1.Discount_Percent != null)
                            {
                                decimal taxxx1 = taxcalculate();
                                int ttl_item = Convert.ToInt32(data.Count);
                                decimal totalprice3 = data11.ToList().Where(x => x.Is_Offline == true).Select(c => (decimal)c.Excluded_Price).Sum();
                                decimal disc_price1 = Math.Round(((Convert.ToDecimal(data.ToList().Select(c => (decimal)c.Price).Sum())) * Convert.ToDecimal(price1.Discount_Percent)) / 100);
                                 disc = disc_price1;

                                decimal total_price1 = Math.Round((Convert.ToDecimal(data.ToList().Select(c => (decimal)c.Price).Sum()) + totalprice3) - (Convert.ToDecimal(disc_price1)));
                                decimal total_paybletax1 = (taxxx1 * total_price1) / 100;
                                decimal total_payblamt1 = Math.Round((total_price1 + total_paybletax1), 2);
                                totalprice1 = total_price1;
                                totalpaybletax1 = total_paybletax1;
                                totalpayblamt1 = total_payblamt1;
                            }
                            else
                            {
                                decimal taxxx1 = taxcalculate();
                                decimal totalprice3 = data11.ToList().Where(x => x.Is_Offline == true).Select(c => (decimal)c.Excluded_Price).Sum();
                                int ttl_item = Convert.ToInt32(data.Count);
                                totalprice1 = Math.Round(Convert.ToDecimal(data.ToList().Select(c => (decimal)c.Price).Sum()) + totalprice3);
                                totalpaybletax1 = (taxxx1 * totalprice1) / 100;
                                totalpayblamt1 = Math.Round((totalprice1 + totalpaybletax1), 2);
                            }
                        }
                    }
                    else
                    {
                        if (price != null)
                        {
                            if (price.Discount_Price != null)
                            {
                                decimal taxxx1 = taxcalculate();
                                int ttl_item = Convert.ToInt32(data.Count);
                                decimal totalprice3 = data11.ToList().Where(x => x.Is_Offline == true).Select(c => (decimal)c.Excluded_Price).Sum();
                                decimal total_price1 = Math.Round((Convert.ToDecimal(data.ToList().Select(c => (decimal)c.Price).Sum()) + totalprice3) - (Convert.ToDecimal(ttl_item * price.Discount_Price)));
                                discA = Convert.ToDecimal(ttl_item * price.Discount_Price);
                                decimal total_paybletax1 = (taxxx1 * total_price1) / 100;
                                decimal total_payblamt1 = Math.Round((total_price1 + total_paybletax1), 2);
                                totalprice1 = total_price1;
                                totalpaybletax1 = total_paybletax1;
                                totalpayblamt1 = total_payblamt1;
                            }
                            else if (price.Discount_Percent != null)
                            {
                                decimal taxxx1 = taxcalculate();
                                int ttl_item = Convert.ToInt32(data.Count);
                                decimal totalprice3 = data11.ToList().Where(x => x.Is_Offline == true).Select(c => (decimal)c.Excluded_Price).Sum();
                                decimal disc_price1 = Math.Round(((Convert.ToDecimal(data.ToList().Select(c => (decimal)c.Price).Sum())) * Convert.ToDecimal(price.Discount_Percent)) / 100);
                                disc = disc_price1;
                                decimal total_price1 = Math.Round((Convert.ToDecimal(data.ToList().Select(c => (decimal)c.Price).Sum()) + totalprice3) - (Convert.ToDecimal( disc_price1)));
                                decimal total_paybletax1 = (taxxx1 * total_price1) / 100;
                                decimal total_payblamt1 = Math.Round((total_price1 + total_paybletax1), 2);
                                totalprice1 = total_price1;
                                totalpaybletax1 = total_paybletax1;
                                totalpayblamt1 = total_payblamt1;
                            }
                            else
                            {
                                decimal taxxx1 = taxcalculate();
                                decimal totalprice3 = data11.ToList().Where(x => x.Is_Offline == true).Select(c => (decimal)c.Excluded_Price).Sum();
                                int ttl_item = Convert.ToInt32(data.Count);
                                totalprice1 = Math.Round(Convert.ToDecimal(data.ToList().Select(c => (decimal)c.Price).Sum()) + totalprice3);
                                totalpaybletax1 = (taxxx1 * totalprice1) / 100;
                                totalpayblamt1 = Math.Round((totalprice1 + totalpaybletax1), 2);
                            }
                        }
                    }
                }

                //totalprice1 = data.ToList().Select(c => (decimal)c.Price).Sum();
                //totalpaybletax1 = (taxxx * totalprice1) / 100;
                //totalpayblamt1 = Math.Round((totalprice1 + totalpaybletax1), 2);  
                decimal taxxx = taxcalculate();
                int total_item1 = Convert.ToInt32(data.Count);
                tbl_DC_Order ordobj = new tbl_DC_Order();
                ordobj.Cart_Order_No = ordno;
                ordobj.Trans_No = ClsId;
                ordobj.Regd_ID = regid;
                ordobj.Regd_No = regno;
                ordobj.No_of_Package = Convert.ToInt32(total_item1);
                ordobj.Amount = Convert.ToDecimal(totalprice1);
                if (disc > 0)
                {
                    ordobj.Disc_Amt = disc;
                }
                if (discA > 0)
                {
                    ordobj.Disc_Perc = price.Discount_Percent;
                }
               
                ordobj.Total = Math.Round(Convert.ToDecimal(totalpayblamt1));
                ordobj.Amt_In_Words = null;
                ordobj.Payment_Mode = "Online";
                ordobj.Is_Paid = true;
                ordobj.Status = true;
                ordobj.Inserted_Date = DateTime.Now;
                string date = Convert.ToDateTime(ordobj.Inserted_Date).ToString("MM/dd/yyyy");
                string purchase_date = date;
                ordobj.Inserted_By = u_name;
                ordobj.Is_Active = true;
                ordobj.Is_Deleted = false;
                DbContext.tbl_DC_Order.Add(ordobj);
                DbContext.SaveChanges();

                var ord_id = ordobj.Order_ID;
                if (ord_id == null)
                {
                    ordobj.Order_No = "DCORD" + "00000" + 1;
                }
                else
                {
                    int ord_no = Convert.ToInt32(ordobj.Order_ID);
                    if (ord_no > 0 && ord_no <= 9)
                    {
                        ordobj.Order_No = "DCORD" + "00000" + Convert.ToString(ord_no);
                    }
                    if (ord_no > 9 && ord_no <= 99)
                    {
                        ordobj.Order_No = "DCORD" + "0000" + Convert.ToString(ord_no);
                    }
                    if (ord_no > 99 && ord_no <= 999)
                    {
                        ordobj.Order_No = "DCORD" + "000" + Convert.ToString(ord_no);
                    }
                    if (ord_no > 999 && ord_no <= 9999)
                    {
                        ordobj.Order_No = "DCORD" + "00" + Convert.ToString(ord_no);
                    }
                    if (ord_no > 9999 && ord_no <= 99999)
                    {
                        ordobj.Order_No = "DCORD" + "0" + Convert.ToString(ord_no);
                    }
                    string Order_Noo = ordobj.Order_No;
                }
                DbContext.SaveChanges();
                if (data2.ToList()[0].email != null && data2.ToList()[0].email != "")
                {
                    var getall = DbContext.SP_DC_Get_maildetails("ORD_CONF").FirstOrDefault();

                  //  StringBuilder sb = new StringBuilder();
                    //if (data != null)
                    //{
                    //    foreach (var it in data)
                    //    {
                    //        string price1 = string.Format("{0:0.00}", it.Price);
                    //        sb.Append("<tr><td width='1' bgcolor='#d6d6d6'></td>");
                    //        sb.Append("<td bgcolor='#ffffff' width='19' height='35' class='m_4430186876753305378tbl_spc'></td>");
                    //        sb.Append("<td class='m_4430186876753305378tbl_value' bgcolor='#ffffff' style='color:#000000;font-family:Arial,Helvetica,sans-serif;font-size:14px;line-height:16px'>" + it.Package_Name + "</td>");
                    //        sb.Append("<td width='5' bgcolor='#fff'></td>");
                    //        sb.Append("<td width='1' bgcolor='#d6d6d6' class='m_4430186876753305378tbl_spc'></td>");
                    //        sb.Append("<td bgcolor='#ffffff' width='18' class='m_4430186876753305378tbl_spc'></td>");
                    //        sb.Append("<td class='m_4430186876753305378tbl_value' bgcolor='#ffffff' style='color:#000000;font-family:Arial,Helvetica,sans-serif;font-size:14px;line-height:16px'>&nbsp;" + price + "</td>");
                    //        sb.Append("<td width='5' bgcolor='#fff'></td>");
                    //        sb.Append("<td width='1' bgcolor='#d6d6d6'></td>");
                    //        sb.Append("</tr>");
                    //        sb.Append("<tr><td height='1' colspan='9' bgcolor='#d6d6d6' style='line-height:1px'><img src='https://ci5.googleusercontent.com/proxy/4Qi8BI_DAT4bvhx96rgfhATiPM79ar8FGDx9AJmuyU02yFsOE09ewUXNAjvFaa6jrtpapK__nEkVkCAdZfCEq4kPOVzK9hwF4Q=s0-d-e1-ft#https://test.payumoney.com/media/images//spacer.png' width='1' height='1' alt='' style='display:block' class='CToWUd'></td></tr>");
                    //    }
                    //}
                   // string tbl_dtl = sb.ToString();
                    string name = data2.ToList()[0].firstname;

                    string ord_date = date;
                   
                  
                    string odno = ordobj.Order_No ;
                    string total_amt = ordobj.Total.ToString();
                    string tx_amt = taxxx.ToString();
                    string pbl_amt = ordobj.Amount.ToString() ;
                    //string msgbody = getall.EmailConf_Body.ToString().Replace("{{name}}", name).Replace("{{orderno}}", odno).Replace("{{orderdate}}", date).Replace("{{packagedetails}}", tbl_dtl).Replace("{{totalamt}}", total_amt).Replace("{{taxamt}}", tx_amt).Replace("{{totalpbl}}", pbl_amt);
                    //sendMail1("ORD_CONF", data2.ToList()[0].email, "Order Confirmation", name, msgbody);
                    var dat = DbContext.tbl_DC_Registration.Where(x => x.Mobile == u_name && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();

                  
                   
                    StringBuilder sb = new StringBuilder();
                    if (data != null)
                    {
                        int m = 1;

                        foreach (var it in data)
                        {

                            string prices = string.Format("{0:0.00}", it.Price);
                            sb.Append("<tr style='text-align:center;font-family: monospace; height:50px;'>");
                            sb.Append("<td style='border:1px solid #0f6fc6;'>" + m++ + "</td>");
                            sb.Append("<td style='border:1px solid #0f6fc6;'>" + it.Package_Name + "</td>");
                            sb.Append("<td style='border:1px solid #0f6fc6;'>" + 1 + "</td>");
                            sb.Append("<td style='border:1px solid #0f6fc6;'> " + prices + "</td>");
                            sb.Append("<td style='border:1px solid #0f6fc6;'>" + total_amt + "</td>");
                            sb.Append("</tr>");
                        }
                    }
                    string tbl_dtl = sb.ToString();
                    if (disc == 0)
                    {
                        disc_amt = discA.ToString();
                    }
                    else
                    {
                        disc_amt = disc.ToString();
                    }
                    string msgbody = getall.EmailConf_Body.ToString().Replace("{{name}}", name).Replace("{{orderno}}", odno).Replace("{{orderdate}}", date).Replace("{{packagedetails}}", tbl_dtl).Replace("{{totaltax}}", tx_amt).Replace("{{totalpbl}}", pbl_amt).Replace("{{address}}", dat.Address).Replace("{{pin}}", dat.Pincode).Replace("{{mobile}}", dat.Mobile).Replace("{{discount}}", disc_amt);
                    sendMail1("ORD_CONF", data2.ToList()[0].email, "Order Confirmation", name, msgbody);

                    try
                    {
                        var pushnot = (from c in DbContext.tbl_DC_Registration.Where(x => x.Regd_ID == data2.ToList()[0].Regd_ID)

                                       select new { c.Regd_ID, c.Device_id }).FirstOrDefault();
                        string body = "ordrid#{{orderid}}# Hello {{name}} ! Your {{pkgname}} is confirmed . Thank you  so much for choosing  DIGICHAMPS";
                        string msg = body.ToString().Replace("{{name}}", data2.ToList()[0].firstname).Replace("{{orderid}}", ord_id.ToString());
                        if (pushnot != null)
                        {
                            if (pushnot.Device_id != null)
                            {
                                var note = new PushNotiStatus(msg, pushnot.Device_id);
                            }
                        }
                    }
                    catch (Exception)
                    {


                    }
                }

                var tax3 = (from tax1 in DbContext.tbl_DC_Tax_Type_Master.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                            join tax2 in DbContext.tbl_DC_Tax_Master.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                            on tax1.TaxType_ID equals tax2.TaxType_ID
                            select new DigiChampCartModel
                            {
                                Tax_ID = tax2.Tax_ID,
                                TaxType_ID = tax1.TaxType_ID,
                                Tax_Rate = tax2.Tax_Rate,
                                TAX_Efect_Date = tax2.TAX_Efect_Date
                            }).ToList();

                if (tax3.Count > 0)
                {
                    foreach (var item3 in tax3)
                    {
                        tbl_DC_Order_Tax ordtax = new tbl_DC_Order_Tax();
                        ordtax.Order_ID = ordobj.Order_ID;
                        ordtax.Order_No = ordobj.Order_No;
                        ordtax.Tax_ID = item3.Tax_ID;
                        ordtax.Tax_Type_ID = item3.TaxType_ID;
                        ordtax.Tax_Effect_Date = item3.TAX_Efect_Date;
                        ordtax.Tax_Amt = Convert.ToDecimal(((ordobj.Amount) * (item3.Tax_Rate)) / 100);
                        ordtax.Status = true;
                        ordtax.Inserted_Date = DateTime.Now;
                        ordtax.Inserted_By = u_name;
                        ordtax.Is_Active = true;
                        ordtax.Is_Deleted = false;
                        DbContext.tbl_DC_Order_Tax.Add(ordtax);
                        DbContext.SaveChanges();
                    }
                }

                var order_pkg = (from ord in DbContext.tbl_DC_Order.Where(x => x.Regd_ID == regid && x.Cart_Order_No == ordno && x.Status == true && x.Is_Active == true && x.Is_Deleted == false)
                                 join cart in DbContext.tbl_DC_Cart.Where(x => x.Regd_ID == regid && x.Order_No == ordno && x.In_Cart == true && x.Is_Paid == false && x.Status == true && x.Is_Active == true && x.Is_Deleted == false)
                                 on ord.Cart_Order_No equals cart.Order_No
                                 join pkg in DbContext.tbl_DC_Package.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                 on cart.Package_ID equals pkg.Package_ID
                                 select new DigiChampCartModel
                                 {
                                     OrderID = ord.Order_ID,
                                     OrderNo = ord.Order_No,
                                     Package_ID = cart.Package_ID,
                                     Package_Name = pkg.Package_Name,
                                     Package_Desc = pkg.Package_Desc,
                                     Total_Chapter = pkg.Total_Chapter,
                                     Price = pkg.Price,
                                     Thumbnail = pkg.Thumbnail,
                                     Subscripttion_Period = pkg.Subscripttion_Period,
                                     Is_Offline = pkg.Is_Offline
                                 }).ToList();

                foreach (var item in order_pkg)
                {
                    tbl_DC_Order_Pkg ordpkg = new tbl_DC_Order_Pkg();
                    ordpkg.Order_ID = item.OrderID;
                    ordpkg.Order_No = item.OrderNo;
                    ordpkg.Package_ID = item.Package_ID;
                    ordpkg.Package_Name = item.Package_Name;
                    ordpkg.Package_Desc = item.Package_Desc;
                    ordpkg.Total_Chapter = item.Total_Chapter;
                    ordpkg.Price = item.Price;
                    ordpkg.Thumbnail = item.Thumbnail;
                    ordpkg.Subscription_Period = item.Subscripttion_Period;
                    int days = Convert.ToInt32(ordpkg.Subscription_Period);
                    ordpkg.Status = true;
                    ordpkg.Inserted_Date = DateTime.Now;
                    ordpkg.Expiry_Date = Convert.ToDateTime(ordpkg.Inserted_Date).AddDays(days);
                    ordpkg.Inserted_By = u_name;
                    ordpkg.Is_Active = true;
                    ordpkg.Is_Deleted = false;
                    if (order_pkg.ToList()[0].Is_Offline == true)
                    {
                        ordpkg.Is_Offline = true;
                    }
                    else
                    {
                        ordpkg.Is_Offline = false;
                    }
                    DbContext.tbl_DC_Order_Pkg.Add(ordpkg);
                    DbContext.SaveChanges();

                    int pkg_id = Convert.ToInt32(ordpkg.Package_ID);
                    int ord_pkg_id = Convert.ToInt32(ordpkg.OrderPkg_ID);
                    foreach (var v in data31)
                    {
                        int cart_id = Convert.ToInt32(v.Cart_ID);

                        var ordpkg_sub = (from ab in DbContext.tbl_DC_Order_Pkg.Where(x => x.OrderPkg_ID == ord_pkg_id && x.Status == true && x.Is_Active == true && x.Is_Deleted == false)
                                          join af in DbContext.tbl_DC_Cart_Dtl.Where(x => x.Cart_ID == cart_id && x.Is_Active == true && x.Is_Deleted == false)
                                          on ab.Package_ID equals af.Package_ID
                                          join ag in DbContext.tbl_DC_Chapter.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                          on af.Chapter_ID equals ag.Chapter_Id
                                          join ah in DbContext.tbl_DC_Subject.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                          on ag.Subject_Id equals ah.Subject_Id
                                          join ai in DbContext.tbl_DC_Class.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                          on ah.Class_Id equals ai.Class_Id
                                          join aj in DbContext.tbl_DC_Board.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                          on ai.Board_Id equals aj.Board_Id
                                          select new DigiChampCartModel
                                          {
                                              OrderPkg_ID = ab.OrderPkg_ID,
                                              Package_ID = af.Package_ID,
                                              Board_ID = aj.Board_Id,
                                              Board_Name = aj.Board_Name,
                                              Class_ID = ai.Class_Id,
                                              Class_Name = ai.Class_Name,
                                              Subject_ID = ah.Subject_Id,
                                              Subject = ah.Subject,
                                              Chapter_ID = ag.Chapter_Id,
                                              Chapter = ag.Chapter
                                          }).ToList();

                        foreach (var item1 in ordpkg_sub)
                        {
                            tbl_DC_Order_Pkg_Sub ordpkgsub = new tbl_DC_Order_Pkg_Sub();
                            ordpkgsub.OrderPkg_ID = item1.OrderPkg_ID;
                            ordpkgsub.Package_ID = item1.Package_ID;
                            ordpkgsub.Board_ID = item1.Board_ID;
                            ordpkgsub.Board_Name = item1.Board_Name;
                            ordpkgsub.Class_ID = item1.Class_ID;
                            ordpkgsub.Class_Name = item1.Class_Name;
                            ordpkgsub.Subject_ID = item1.Subject_ID;
                            ordpkgsub.Subject_Name = item1.Subject;
                            ordpkgsub.Chapter_ID = item1.Chapter_ID;
                            ordpkgsub.Chapter_Name = item1.Chapter;
                            ordpkgsub.Status = true;
                            ordpkgsub.Inserted_Date = DateTime.Now;
                            ordpkgsub.Inserted_By = u_name;
                            ordpkgsub.Is_Active = true;
                            ordpkgsub.Is_Deleted = false;
                            DbContext.tbl_DC_Order_Pkg_Sub.Add(ordpkgsub);
                            DbContext.SaveChanges();

                            int ord_pkg_sub_id = Convert.ToInt32(ordpkgsub.OrderPkgSub_ID);

                            var ordpkg_sub_mod = (from ak in DbContext.tbl_DC_Order_Pkg_Sub.Where(x => x.OrderPkgSub_ID == ord_pkg_sub_id && x.Status == true && x.Is_Active == true && x.Is_Deleted == false)
                                                  join al in DbContext.tbl_DC_Module.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                                  on ak.Chapter_ID equals al.Chapter_Id
                                                  select new DigiChampCartModel
                                                  {
                                                      OrderPkgSub_ID = ak.OrderPkgSub_ID,
                                                      Chapter_ID = ak.Chapter_ID,
                                                      Chapter = ak.Chapter_Name,
                                                      Module_ID = al.Module_ID,
                                                      Module_Name = al.Module_Name
                                                  }).ToList();

                            if (ordpkg_sub_mod.Count > 0)
                            {
                                foreach (var item2 in ordpkg_sub_mod)
                                {
                                    tbl_DC_Order_Pkg_Sub_Mod ordpkgsubmod = new tbl_DC_Order_Pkg_Sub_Mod();
                                    ordpkgsubmod.OrderPkgSub_ID = item2.OrderPkgSub_ID;
                                    ordpkgsubmod.Chapter_ID = item2.Chapter_ID;
                                    ordpkgsubmod.Chapter_Name = item2.Chapter;
                                    ordpkgsubmod.Module_ID = item2.Module_ID;
                                    ordpkgsubmod.Module_Name = item2.Module_Name;
                                    ordpkgsubmod.Status = true;
                                    ordpkgsubmod.Inserted_Date = DateTime.Now;
                                    ordpkgsubmod.Inserted_By = u_name;
                                    ordpkgsubmod.Is_Active = true;
                                    ordpkgsubmod.Is_Deleted = false;
                                    DbContext.tbl_DC_Order_Pkg_Sub_Mod.Add(ordpkgsubmod);
                                    DbContext.SaveChanges();
                                }
                            }
                        }
                    }
                }


                var cartobj = (from ct in DbContext.tbl_DC_Cart.Where(x => x.Order_ID == ordid && x.Order_No == ordno && x.Is_Active == true && x.Is_Deleted == false)
                               select new DigiChampCartModel
                               {
                                   Cart_ID = ct.Cart_ID,
                                   In_Cart = ct.In_Cart,
                                   Is_Paid = ct.Is_Paid,
                                   Status = ct.Status,
                                   Is_Active = ct.Is_Active,
                                   Is_Deleted = ct.Is_Deleted
                               }).ToList();

                foreach (var item4 in cartobj)
                {
                    int ctid = Convert.ToInt32(item4.Cart_ID);
                    tbl_DC_Cart cartobj1 = DbContext.tbl_DC_Cart.Where(x => x.Cart_ID == ctid && x.Is_Active == true && x.Is_Deleted == false && x.In_Cart == true && x.Is_Paid == false).FirstOrDefault();
                    cartobj1.In_Cart = false;
                    cartobj1.Is_Paid = true;
                    cartobj1.Status = false;
                    cartobj1.Is_Active = false;
                    cartobj1.Is_Deleted = true;
                    DbContext.Entry(cartobj1).State = EntityState.Modified;
                    DbContext.SaveChanges();
                }
                order_conf_model ocm = new order_conf_model();
                ocm.cartdata = data;
                ocm.price = Math.Round(totalprice1, 2);
                ocm.payble_price = Math.Round(totalpayblamt1, 2);
                ocm.tax_percent = Math.Round(taxxx, 2);
                ocm.tax_amount = Math.Round(totalpaybletax1, 2);
                ocm.total_item = total_item1;
                ocm.purchase_date = purchase_date;
                ocm.Order_Id = ord_id;
                ocm.Order_No = ordobj.Order_No;

                if (ocm != null)
                {
                    var obj = new getconfirmation
                    {
                        success = new successssss()
                        {
                            get_confdata = ocm
                        }
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, obj);
                }

            }
            catch (Exception ex)
            {
                var ob = new Errorresult
                {
                    Error = new Errorresponse
                    {
                        Message = "Something went wrong"
                    }
                };
                return Request.CreateResponse(HttpStatusCode.OK, ob);
            }
            return Request.CreateResponse(HttpStatusCode.OK);
        }


        [HttpGet]
        public HttpResponseMessage OrderResendMail(int id, int eid)
        {
            try
            {
                var register = DbContext.tbl_DC_Registration.Where(x => x.Regd_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                var cartt = DbContext.tbl_DC_Order.Where(x => x.Order_ID == eid && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                var pckg=DbContext.tbl_DC_Order_Pkg.Where(x=> x.Order_ID== eid && x.Is_Active == true && x.Is_Deleted == false).ToList();
                var taxo = DbContext.tbl_DC_Order_Tax.Where(x => x.Order_ID == eid && x.Is_Active == true && x.Is_Deleted == false).ToList().Select(x=> (decimal)x.Tax_Amt).Sum();
                string disc_amt = string.Empty;
                decimal disc = 0;
                decimal discA = 0;
              

             

                int regid = Convert.ToInt32(id);
                if (register != null)
                {

                    if (cartt != null)

                    {
                        string u_name = register.Mobile;
                        int ordid = Convert.ToInt32(eid);
                        string ordno = cartt.Order_No.ToString();
                        var getall = DbContext.SP_DC_Get_maildetails("ORD_CONF").FirstOrDefault();

                        string name = register.Address;

                        string ord_date = cartt.Inserted_Date.ToString();


                        string odno = cartt.Order_No;
                        string total_amt = cartt.Total.ToString();
                        string tx_amt = taxo.ToString();
                        string pbl_amt = cartt.Amount.ToString();
                        //string msgbody = getall.EmailConf_Body.ToString().Replace("{{name}}", name).Replace("{{orderno}}", odno).Replace("{{orderdate}}", date).Replace("{{packagedetails}}", tbl_dtl).Replace("{{totalamt}}", total_amt).Replace("{{taxamt}}", tx_amt).Replace("{{totalpbl}}", pbl_amt);
                        //sendMail1("ORD_CONF", data2.ToList()[0].email, "Order Confirmation", name, msgbody);
                      


                        StringBuilder sb = new StringBuilder();
                        if (pckg != null)
                        {
                            int m = 1;

                            foreach (var it in pckg)
                            {

                                string prices = string.Format("{0:0.00}", it.Price);
                                sb.Append("<tr style='text-align:center;font-family: monospace; height:50px;'>");
                                sb.Append("<td style='border:1px solid #0f6fc6;'>" + m++ + "</td>");
                                sb.Append("<td style='border:1px solid #0f6fc6;'>" + it.Package_Name + "</td>");
                                sb.Append("<td style='border:1px solid #0f6fc6;'>" + 1 + "</td>");
                                sb.Append("<td style='border:1px solid #0f6fc6;'> " + prices + "</td>");
                                sb.Append("<td style='border:1px solid #0f6fc6;'>" + total_amt + "</td>");
                                sb.Append("</tr>");
                            }
                        }
                        string tbl_dtl = sb.ToString();
                        if (cartt.Disc_Perc != null)
                        {
                            disc_amt = cartt.Disc_Perc.ToString() +"%";
                        }
                         if (cartt.Disc_Amt != null)
                        
                            {
                            disc_amt = cartt.Disc_Amt.ToString();
                        }
                        string msgbody = getall.EmailConf_Body.ToString().Replace("{{name}}", name).Replace("{{orderno}}", odno).Replace("{{orderdate}}", ord_date).Replace("{{packagedetails}}", tbl_dtl).Replace("{{totaltax}}", tx_amt).Replace("{{totalpbl}}", pbl_amt).Replace("{{address}}", register.Address).Replace("{{pin}}", register.Pincode).Replace("{{mobile}}", register.Mobile).Replace("{{discount}}", disc_amt);
                        sendMail1("ORD_CONF", register.Email, "Order Confirmation", name, msgbody);
                       
                            var obj = new Successresult
                            {
                                success = new Successrespone()
                                {
                                    Message = "Email resent succesfully."
                                }
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, obj);
                      
                    }
                    else {
                        var ob = new Errorresult
                        {
                            Error = new Errorresponse
                            {
                                Message = "Invalid Users"
                            }
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, ob);
                    }
                }
                else
                {
                    var ob = new Errorresult
                    {
                        Error = new Errorresponse
                        {
                            Message = "Invalid Order"
                        }
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, ob);

                }

              

              

            }
            catch (Exception ex)
            {
                var ob = new Errorresult
                {
                    Error = new Errorresponse
                    {
                        Message = "Something went wrong"
                    }
                };
                return Request.CreateResponse(HttpStatusCode.OK, ob);
            }
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [HttpGet]
        public HttpResponseMessage Cancel_Order()
        {
            try
            {
                var ob = new Successresult
                {
                    success = new Successrespone
                    {
                        Message = "Your last order has been cancelled"
                    }
                };
                return Request.CreateResponse(HttpStatusCode.OK, ob);

            }
            catch (Exception ex)
            {
                var ob = new Errorresult
                {
                    Error = new Errorresponse
                    {
                        Message = "Something went wrong"
                    }
                };
                return Request.CreateResponse(HttpStatusCode.OK);
            }
        }

        public class payumodel
        {
            public string firstName { get; set; }
            public string amount { get; set; }
            public string productInfo { get; set; }
            public string email { get; set; }
            public string phone { get; set; }
            public string key { get; set; }
            public string salt { get; set; }
            public string surl { get; set; }
            public string furl { get; set; }
        }
        public class successsssss
        {
            public payumodel getcartdata { get; set; }
        }
        public class getpayudata
        {
            public successsssss success { get; set; }
        }
        [HttpPost]
        public HttpResponseMessage OrderNow(int id, decimal eid)
        {
            try
            {
                if (id != null && eid != null)
                {
                    var custom = DbContext.tbl_DC_Registration.Where(x => x.Regd_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    string firstName = custom.Customer_Name.ToString();
                    string amount = eid.ToString();

                    string productInfo = "Packages for student";

                    string key = "rjQUPktU";
                    string salt = "e5iIg1jwi8";
                    string email = custom.Email.ToString();
                    string phone = custom.Mobile.ToString();
                    string surl = "http://sms.ntspl.co.in/Cart/Order_Confirmation/id/eid";
                    string furl = "http://sms.ntspl.co.in/Cart/Cancel_Order";

                    payumodel pum = new payumodel();
                    pum.firstName = firstName;
                    pum.amount = amount;
                    pum.productInfo = productInfo;
                    pum.email = email;
                    pum.phone = phone;
                    pum.surl = surl;
                    pum.furl = furl;
                    pum.key = key;
                    pum.salt = salt;

                    var ob = new getpayudata
                    {
                        success = new successsssss()
                        {
                            getcartdata = pum
                        }
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, ob);
                }
                else
                {
                    var ob = new Errorresult
                    {
                        Error = new Errorresponse
                        {
                            Message = "Please provide data correctly"
                        }
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, ob);
                }
            }
            catch (Exception ex)
            {
                var ob = new Errorresult
                {
                    Error = new Errorresponse
                    {
                        Message = "Something went wrong"
                    }
                };
                return Request.CreateResponse(HttpStatusCode.OK, ob);
            }
        }
        public class RemotePost
        {
            private System.Collections.Specialized.NameValueCollection Inputs = new System.Collections.Specialized.NameValueCollection();


            public string Url = "";
            public string Method = "post";
            public string FormName = "form1";

            public void Add(string name, string value)
            {
                Inputs.Add(name, value);
            }

            public void Post()
            {
                System.Web.HttpContext.Current.Response.Clear();

                System.Web.HttpContext.Current.Response.Write("<html><head>");

                System.Web.HttpContext.Current.Response.Write(string.Format("</head><body onload=\"document.{0}.submit()\">", FormName));
                System.Web.HttpContext.Current.Response.Write(string.Format("<form name=\"{0}\" method=\"{1}\" action=\"{2}\" >", FormName, Method, Url));
                for (int i = 0; i < Inputs.Keys.Count; i++)
                {
                    System.Web.HttpContext.Current.Response.Write(string.Format("<input name=\"{0}\" type=\"hidden\" value=\"{1}\">", Inputs.Keys[i], Inputs[Inputs.Keys[i]]));
                }
                System.Web.HttpContext.Current.Response.Write("</form>");
                System.Web.HttpContext.Current.Response.Write("</body></html>");

                System.Web.HttpContext.Current.Response.End();
            }
        }
        public string Generatehash512(string text)
        {

            byte[] message = Encoding.UTF8.GetBytes(text);

            UnicodeEncoding UE = new UnicodeEncoding();
            byte[] hashValue;
            SHA512Managed hashString = new SHA512Managed();
            string hex = "";
            hashValue = hashString.ComputeHash(message);
            foreach (byte x in hashValue)
            {
                hex += String.Format("{0:x2}", x);
            }
            return hex;

        }
        public string Generatetxnid()
        {

            Random rnd = new Random();
            string strHash = Generatehash512(rnd.ToString() + DateTime.Now);
            string txnid1 = strHash.ToString().Substring(0, 20);

            return txnid1;
        }

        public class Errorresult
        {
            public Errorresponse Error { get; set; }
        }
        public class Errorresponse
        {
            public string Message { get; set; }
        }
        public class Successresult
        {
            public Successrespone success { get; set; }
        }

        public class Successresult1
        {
            public Successrespone1 success { get; set; }
        }

        public class Successrespone
        {
            public string Message { get; set; }
        }

        public class Successrespone1
        {
            public deletemsg deleteitem { get; set; }
        }

        public class deletemsg
        {
            public string Message { get; set; }
        }

        public bool sendMail1(string parameter, string email, string sub, string name, string msgbody)
        {
            var getall = DbContext.SP_DC_Get_maildetails(parameter).FirstOrDefault();
            string eidTo = email;
            string mailtoshow = getall.SMTP_Email.ToString();
            string eidFrom = getall.SMTP_User.ToString();
            string password = getall.SMTP_Pwd.ToString();
            string msgsub = sub;
            string hostname = getall.SMTP_HostName;
            string portname = getall.SMTP_Port.ToString();
            bool ssl_tof = true;
            //string msgbody = getall.EmailConf_Body.ToString().Replace("{{name}}", name);
            MailMessage greetings = new MailMessage();
            SmtpClient smtp = new SmtpClient();
            try
            {
                greetings.From = new MailAddress(mailtoshow, "DIGICHAMPS");//sendername
                greetings.To.Add(eidTo);//to whom
                greetings.IsBodyHtml = true;
                greetings.Priority = MailPriority.High;
                greetings.Body = msgbody;
                greetings.Subject = msgsub;
                smtp.Host = hostname;//host name
                smtp.EnableSsl = ssl_tof;//ssl
                smtp.Port = Convert.ToInt32(portname);//port
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential(eidFrom, password);//from(user)//password
                smtp.Send(greetings);
                return true;
            }
            catch (Exception ex)
            {
                return false;   
            }
        }
       
        public class successss1
        {
            public int countdata { get; set; }
        }
        public class getcartcount
        {
            public successss1 success { get; set; }
        }
        public HttpResponseMessage Counter(int id)
        {
            try
            {
                var data = (from a in DbContext.tbl_DC_Registration.Where(x => x.Regd_ID == id && x.Is_Active == true && x.Is_Deleted == false)
                            join b in DbContext.tbl_DC_Cart.Where(x => x.In_Cart == true && x.Is_Paid == false && x.Is_Active == true && x.Is_Deleted == false)
                             on a.Regd_ID equals b.Regd_ID                           
                            select new DigiChampCartModel
                            {                             
                                Cart_ID = b.Cart_ID
                            }).ToList();
                var ob = new getcartcount
                {
                    success = new successss1()
                    {
                        countdata = Convert.ToInt32(data.Count)
                    }
                };
                return Request.CreateResponse(HttpStatusCode.OK, ob);
            }
            catch (Exception ex)
            {
                var ob = new Errorresult
                {
                    Error = new Errorresponse
                    {
                        Message = "Something went wrong"
                    }
                };
                return Request.CreateResponse(HttpStatusCode.OK, ob);
            }

        }
    }
}
