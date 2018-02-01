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

namespace DigiChamps.Controllers
{
    [Authorize] 
    public class AcademicController : ApiController
    {
        DigiChampsEntities dbContext = new DigiChampsEntities();

        public HttpResponseMessage GetBoard()
        {
            try
            {
                var data = (from a in dbContext.tbl_DC_Board.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                            select new DigiChampsApiBoardModel
                            {
                                Board_Id = a.Board_Id,
                                Board_Name = a.Board_Name
                            }).ToList();
                if (data != null)
                {
                    var obj = new getorders
                    {
                        success= new successss()
                        {
                            getboard = data.ToList()
                        }
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, obj);
                }
                else
                {
                    var obj = new Errorresult
                    {
                        Error = new Errorresponse
                        {
                            Message = "Board not available."
                        }
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, obj);

                }
            }
            catch (Exception ex)
            {
                var obj = new Errorresult
                {
                    Error = new Errorresponse
                    {
                        Message = "Something went wrong."
                    }
                };
                return Request.CreateResponse(HttpStatusCode.OK, obj);
            }
        }

        public class successss
        {
            public List<DigiChampsApiBoardModel> getboard { get; set; }

        }
        public class getorders
        {
            public successss success { get; set; }
        }

        public class successs
        {
            public List<DigiChampsApiClassModel> getclass { get; set; }

        }
        public class getorderss
        {
            public successs success { get; set; }
        }
        public class DigiChampsApiBoardModel
        {
            public int Board_Id { get; set; }
            public string Board_Name { get; set; }
        }

        public HttpResponseMessage GetClass(int id)
        {
            try
            {
                if (id != null)
                {
                    var data = (from a in dbContext.tbl_DC_Class.Where(x => x.Is_Active == true && x.Is_Deleted == false && x.Board_Id == id)
                                select new DigiChampsApiClassModel
                                {
                                    Class_Id = a.Class_Id,
                                    Class_Name = a.Class_Name
                                }).ToList();
                    if (data != null)
                    {

                        var obj = new getorderss
                        {
                            success = new successs
                            {
                                getclass = data.ToList()
                            }

                        };
                        return Request.CreateResponse(HttpStatusCode.OK, obj);
                    }
                    else
                    {

                        var obj = new Errorresult
                        {
                            Error = new Errorresponse
                            {
                                Message = "Class not available."
                            }
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, obj);

                    }

                }
                else
                {

                    var obj = new Errorresult
                    {
                        Error = new Errorresponse
                        {
                            Message = "Class not available."
                        }
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, obj);
                }


            }
            catch (Exception ex)
            {
                var obj = new Errorresult
                {
                    Error = new Errorresponse
                    {
                        Message = "Something went wrong."
                    }
                };
                return Request.CreateResponse(HttpStatusCode.OK, obj);
            }
        }
        public class DigiChampsApiClassModel
        {
            public int Class_Id { get; set; }
            public string Class_Name { get; set; }
        }
      
        [HttpGet]
        public HttpResponseMessage GetPackage(int? id, int? eid)
        {
            try
            {
                if (eid != null && id != null)
                {
                    //var classs = dbContext.tbl_DC_Registration_Dtl.Where(x => x.Regd_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    //int cls_id = Convert.ToInt32(classs.Class_ID);

                    var z = dbContext.tbl_DC_Package.Where(x => x.Is_Active == true && x.Is_Deleted == false).ToList();
                    var y = dbContext.tbl_DC_Package_Dtl.Where(x => x.Class_Id == eid  && x.Board_Id == id && x.Is_Active == true && x.Is_Deleted == false).Select(x => x.Package_ID).Distinct().ToList();
                    var data = (from a in z
                                join b in y
                                on a.Package_ID equals b
                                select new DigiChampsPackApiClassModel
                                {
                                    Package_ID = a.Package_ID,
                                    Package_Name = a.Package_Name,
                                    Price = packprice(a.Package_ID),
                                    DiscPrice = discprice(a.Package_ID),
                                    Disc_Percent = Disc_Percent(a.Package_ID),
                                    Subscripttion_Period = a.Subscripttion_Period,
                                    Subscripttion_Limit = Subscripttion_Limit(a.Package_ID),
                                    Is_Offline = a.Is_Offline,
                                    Package_Desc = a.Package_Desc,
                                    Thumbnail = a.Thumbnail
                                }).ToList();

                    if (data.Count > 0)
                    {
                        var package = new PkgresultRESPONSE
                        {
                            success = new success
                            {
                                Package = data.ToList()
                            }
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, package);
                    }
                    else
                    {
                        var ob = new Errorresult
                        {
                            Error = new Errorresponse
                            {
                                Message = "No pacakages found."
                            }
                      };
                        return Request.CreateResponse(HttpStatusCode.OK, ob);
                    }
                }
            }
            catch (Exception ex)
            {
                var ob = new Errorresult
                {
                    Error = new Errorresponse
                    {
                        Message = "Something went wrong."
                    }
                };
                return Request.CreateResponse(HttpStatusCode.OK, ob);
            }
            return Request.CreateResponse(HttpStatusCode.OK);
        }
        [HttpGet]
        public HttpResponseMessage GetPackage(int? id)
        {
            try
            {
                if (id != null)
                {
                    var classs = dbContext.tbl_DC_Registration_Dtl.Where(x => x.Regd_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    int cls_id = Convert.ToInt32(classs.Class_ID);

                    var z = dbContext.tbl_DC_Package.Where(x => x.Is_Active == true && x.Is_Deleted == false).ToList();
                    var y = dbContext.tbl_DC_Package_Dtl.Where(x => x.Class_Id == cls_id && x.Is_Active == true && x.Is_Deleted == false).Select(x => x.Package_ID).Distinct().ToList();
                    var data = (from a in z
                                join b in y
                                on a.Package_ID equals b
                                select new DigiChampsPackApiClassModel
                                {
                                    Package_ID = a.Package_ID,
                                    Package_Name = a.Package_Name,
                                    Price = packprice(a.Package_ID),
                                    DiscPrice = discprice(a.Package_ID),
                                    Disc_Percent = Disc_Percent(a.Package_ID),
                                    Subscripttion_Period = a.Subscripttion_Period,
                                    Subscripttion_Limit = Subscripttion_Limit(a.Package_ID),
                                    Is_Offline = a.Is_Offline,
                                    Package_Desc = a.Package_Desc,
                                    Thumbnail = a.Thumbnail
                                }).ToList();

                    if (data.Count < 0)
                    {
                        var ob = new Errorresult
                        {
                            Error = new Errorresponse
                            {
                                Message = "No pacakages found."
                            }
                        };
                    }
                    else
                    {
                        var package = new PkgresultRESPONSE
                        {
                            success = new success
                            {
                                Package = data.ToList()
                            }
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, package);
                    }
                }
            }
            catch (Exception ex)
            {
                var ob = new Errorresult
                {
                    Error = new Errorresponse
                    {
                        Message = "Something went wrong."
                    }
                };
                return Request.CreateResponse(HttpStatusCode.OK, ob);
            }
            return Request.CreateResponse(HttpStatusCode.OK);
        }
        public class PkgresultRESPONSE
        {
            public success success { get; set; }
        }
        public class success
        {
            public List<DigiChampsPackApiClassModel> Package { get; set; }
        }
        public class DigiChampsPackApiClassModel
        {
            public Nullable<int> Package_ID { get; set; }
            public string Package_Name { get; set; }
            public string Package_Desc { get; set; }
            public Nullable<decimal> Price { get; set; }
            public Nullable<decimal> DiscPrice { get; set; }
            public string Thumbnail { get; set; }
            public Nullable<decimal> Disc_Percent { get; set; }
            public Nullable<int> Subscripttion_Period { get; set; }
            public Nullable<int> Subscripttion_Limit { get; set; }
            public Nullable<bool> Is_Offline { get; set; }
        }

        public HttpResponseMessage GetPackageDetail(int? id)
        {
            try
            {
                if (id != null)
                {
                    var pack_period = dbContext.tbl_DC_Package_Period.Where(x => x.Package_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    if (pack_period != null)
                    {
                        var a = dbContext.tbl_DC_Package.Where(x => x.Package_ID == id && x.Is_Active == true && x.Is_Deleted == false).ToList();
                        if (a.Count > 0)
                        {
                            var packagedtl = new DigiChampsPackDtlModel
                            {
                                success = new success1()
                                {

                                    pkg_dtl = (from z in dbContext.tbl_DC_Package.Where(x => x.Package_ID == id && x.Is_Active == true && x.Is_Deleted == false)
                                               join y in dbContext.tbl_DC_PackageSub_Dtl.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                               on z.Package_ID equals y.Package_ID
                                               select new PackageDetailModel
                                               {
                                                   Package_ID = z.Package_ID,
                                                   Package_Name = z.Package_Name,
                                                   Package_Desc = z.Package_Desc,
                                                   Subscripttion_Period = z.Subscripttion_Period,
                                                   Price = Math.Round((decimal)z.Price + (decimal)pack_period.Excluded_Price),
                                                   Is_Offline = z.Is_Offline,
                                                   Total_Chapter = dbContext.tbl_DC_Package_Dtl.Where(x => x.Package_ID == id && x.Is_Active == true && x.Is_Deleted == false).ToList().Count,
                                                   Thumbnail = z.Thumbnail,
                                                   SubScription_Limit = dbContext.tbl_DC_PackageSub_Dtl.Where(x => x.Package_ID == id).Select(x => x.SubScription_Limit).ToList().Sum(),
                                                   Sublist = (from p in dbContext.tbl_DC_Package_Dtl.Where(x => x.Package_ID == z.Package_ID && x.Is_Active == true && x.Is_Deleted == false).Select(x => x.Subject_Id).Distinct()
                                                              join r in dbContext.tbl_DC_Subject.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                                              on p equals r.Subject_Id
                                                              select new Package_subject_Detail
                                                              {
                                                                  Subect_Id = p,
                                                                  Subject_Name = r.Subject,
                                                                  Chaplist = (from pobj in dbContext.tbl_DC_Package_Dtl.Where(x => x.Subject_Id == p && x.Package_ID == id && x.Is_Active == true && x.Is_Deleted == false)
                                                                              join s in dbContext.tbl_DC_Chapter.Where(x => x.Is_Active == true && x.Is_Deleted == false && x.Subject_Id == p)
                                                                              on pobj.Chapter_Id equals s.Chapter_Id
                                                                              select new Package_chaptrs
                                                                              {
                                                                                  Chapter_Id = s.Chapter_Id,
                                                                                  Chapter_Name = s.Chapter
                                                                              }).ToList()
                                                              }).ToList()
                                               }).Take(1).ToList()
                                }
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, packagedtl);
                        }
                        else
                        {
                            var ob = new Errorresult
                            {
                                Error = new Errorresponse
                                {
                                    Message = "No pacakages detail found."
                                }
                            };
                        }
                    }
                    else
                    {
                        var a = dbContext.tbl_DC_Package.Where(x => x.Package_ID == id && x.Is_Active == true && x.Is_Deleted == false).ToList();
                        if (a.Count > 0)
                        {
                            var packagedtl = new DigiChampsPackDtlModel
                            {
                                success = new success1()
                                {

                                    pkg_dtl = (from z in dbContext.tbl_DC_Package.Where(x => x.Package_ID == id && x.Is_Active == true && x.Is_Deleted == false)
                                               join y in dbContext.tbl_DC_PackageSub_Dtl.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                               on z.Package_ID equals y.Package_ID
                                               select new PackageDetailModel
                                               {
                                                   Package_ID = z.Package_ID,
                                                   Package_Name = z.Package_Name,
                                                   Package_Desc = z.Package_Desc,
                                                   Subscripttion_Period = z.Subscripttion_Period,
                                                   Price = Math.Round((decimal)z.Price),
                                                   Is_Offline = z.Is_Offline,
                                                   Total_Chapter = dbContext.tbl_DC_Package_Dtl.Where(x => x.Package_ID == id && x.Is_Active == true && x.Is_Deleted == false).ToList().Count,
                                                   Thumbnail = z.Thumbnail,
                                                   SubScription_Limit = dbContext.tbl_DC_PackageSub_Dtl.Where(x => x.Package_ID == id).Select(x => x.SubScription_Limit).ToList().Sum(),
                                                   Sublist = (from p in dbContext.tbl_DC_Package_Dtl.Where(x => x.Package_ID == z.Package_ID && x.Is_Active == true && x.Is_Deleted == false).Select(x => x.Subject_Id).Distinct()
                                                              join r in dbContext.tbl_DC_Subject.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                                              on p equals r.Subject_Id
                                                              select new Package_subject_Detail
                                                              {
                                                                  Subect_Id = p,
                                                                  Subject_Name = r.Subject,
                                                                  Chaplist = (from pobj in dbContext.tbl_DC_Package_Dtl.Where(x => x.Subject_Id == p && x.Package_ID == id && x.Is_Active == true && x.Is_Deleted == false)
                                                                              join s in dbContext.tbl_DC_Chapter.Where(x => x.Is_Active == true && x.Is_Deleted == false && x.Subject_Id == p)
                                                                              on pobj.Chapter_Id equals s.Chapter_Id
                                                                              select new Package_chaptrs
                                                                              {
                                                                                  Chapter_Id = s.Chapter_Id,
                                                                                  Chapter_Name = s.Chapter
                                                                              }).ToList()
                                                              }).ToList()
                                               }).Take(1).ToList()
                                }
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, packagedtl);
                        }
                        else
                        {
                            var ob = new Errorresult
                            {
                                Error = new Errorresponse
                                {
                                    Message = "No pacakages detail found."
                                }
                            };
                        }
                    }

                }
                else
                {
                    var ob = new Errorresult
                    {
                        Error = new Errorresponse
                        {
                            Message = "Please provide data correctly."
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
                        Message = "Something went wrong."
                    }
                };
                return Request.CreateResponse(HttpStatusCode.OK, ob);
            }
            return Request.CreateResponse(HttpStatusCode.OK);
        }
        public class DigiChampsPackDtlModel
        {
            public success1 success { get; set; }
        }
        public class success1
        {
            public List<PackageDetailModel> pkg_dtl { get; set; }
        }

        public class PackageDetailModel
        {
            public Nullable<int> Package_ID { get; set; }
            public string Package_Name { get; set; }
            public string Package_Desc { get; set; }
            public Nullable<int> Total_Chapter { get; set; }
            public string Thumbnail { get; set; }
            public Nullable<int> Subscripttion_Period { get; set; }
            public Nullable<int> SubScription_Limit { get; set; }
            public Nullable<decimal> Price { get; set; }
            public Nullable<bool> Is_Offline { get; set; }
            public List<Package_subject_Detail> Sublist { get; set; }
        }
        public class Package_subject_Detail
        {
            public Nullable<int> Subect_Id { get; set; }
            public string Subject_Name { get; set; }
            public List<Package_chaptrs> Chaplist { get; set; } 
        }
        public class Package_chaptrs
        {
            public Nullable<int> Chapter_Id { get; set; }
            public string Chapter_Name { get; set; }
        }

        public decimal Disc_Percent(int id)
        {
            decimal percent = 0;
            DateTime today = DigiChamps.Models.DigiChampsModel.datetoserver();
            DigiChamps.Models.DigiChampsEntities dc = new DigiChamps.Models.DigiChampsEntities();
            var dt1 = dc.tbl_DC_Package.Where(x => x.Package_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
            var dt = dc.tbl_DC_Package_Period.Where(x => x.Package_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
            var disc_prc = dc.tbl_DC_CouponCode.Where(x => x.Is_Default == true && x.Valid_From <= today && x.Valid_To >= today && x.Is_Active == true && x.Is_Delete == false).FirstOrDefault();
            if (disc_prc != null && disc_prc.Discount_Percent != null)
            {
                if (dt != null && dt1 != null)
                {
                    //decimal price2 = Convert.ToDecimal(disc_prc.Discount_Percent);
                    percent = Math.Round((Convert.ToDecimal(disc_prc.Discount_Percent)));
                }
                else
                {
                    //decimal price2 = Convert.ToDecimal(dt1.Price);
                    percent = Math.Round((Convert.ToDecimal(disc_prc.Discount_Percent)));
                }
            }
            else
            {
                if (dt != null && dt1 != null)
                {
                    decimal pric = Convert.ToDecimal(dt1.Price);
                    if (disc_prc != null && disc_prc.Discount_Price != null)
                    {
                        if (pric > disc_prc.Discount_Price)
                        {
                            decimal price2 = Convert.ToDecimal(dt1.Price);
                            decimal price_package = Convert.ToDecimal(disc_prc.Discount_Price);
                            decimal percent_price = Convert.ToDecimal((price_package * 100) / price2);
                            percent = Math.Round(percent_price + Convert.ToDecimal(dt.Excluded_Price));
                        }
                        else
                        {
                            percent = 0;
                        }
                    }
                    else
                    {
                        percent = 0;
                    }

                }
                else
                {
                    decimal pric = Convert.ToDecimal(dt1.Price);
                    if (disc_prc != null && disc_prc.Discount_Price != null)
                    {
                        if (pric > disc_prc.Discount_Price)
                        {
                            decimal price2 = Convert.ToDecimal(dt1.Price);
                            decimal price_package = Convert.ToDecimal(disc_prc.Discount_Price);
                            decimal percent_price = Convert.ToDecimal((price_package * 100) / price2);
                            percent = Math.Round(percent_price);
                        }
                        else
                        {
                            percent = 0;
                        }
                    }
                    else
                    {
                        percent = 0;
                    }
                }
            }
            return percent;
        }
        public decimal packprice(int id)
        {
            decimal price1 = 0;
            DigiChamps.Models.DigiChampsEntities dc = new DigiChamps.Models.DigiChampsEntities();
            var dt1 = dc.tbl_DC_Package.Where(x => x.Package_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
            var dt = dc.tbl_DC_Package_Period.Where(x => x.Package_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
            if (dt != null && dt1 !=null)
            {
                decimal price2 = Convert.ToDecimal(dt.Excluded_Price + dt1.Price);
                price1 = Math.Round(price2);
            }
            else
            {
                decimal price2 = Convert.ToDecimal(dt1.Price);
                price1 = Math.Round(price2);
            }          
            return price1;
        }
        public decimal discprice(int id)
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
                            decimal price2 = Convert.ToDecimal(dt1.Price - Convert.ToDecimal(disc_prc.Discount_Price));
                            price1 = Math.Round(price2 + Convert.ToDecimal(dt.Excluded_Price));
                        }
                        else
                        {
                            decimal price2 = Convert.ToDecimal(dt.Excluded_Price + dt1.Price);
                            price1 = Math.Round(price2);
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
                            price1 = Math.Round(price2 + Convert.ToDecimal(dt.Excluded_Price));
                        }
                        else
                        {
                            decimal price2 = Convert.ToDecimal(price);
                            price1 = Math.Round(price2);
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
                            price1 = Math.Round(price2);
                        }
                        else
                        {
                            decimal price2 = Convert.ToDecimal(dt1.Price);
                            price1 = Math.Round(price2);
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
                            price1 = Math.Round(price2);
                        }
                        else
                        {
                            decimal price2 = Convert.ToDecimal(dt1.Price + Convert.ToDecimal(dt.Excluded_Price));
                            price1 = Math.Round(price2);
                        }
                    }

                }
            }
            else
            {
                if (dt1 != null)
                {
                    price1 = Math.Round(Convert.ToDecimal(dt1.Price) + Convert.ToDecimal(dt.Excluded_Price));
                }
                else
                {
                    price1 = Math.Round(Convert.ToDecimal(dt1.Price));
                }
            }
            return price1;
        }

        public int Subscripttion_Limit(int id)
        {
            int limit = 0;
            DigiChamps.Models.DigiChampsEntities dc = new DigiChamps.Models.DigiChampsEntities();
            int lt=Convert.ToInt32(dbContext.tbl_DC_PackageSub_Dtl.Where(x => x.Package_ID == id).Select(x => x.SubScription_Limit).ToList().Sum());
            if (lt > 0)
            {
                limit = lt;
            }
            else
            {
                limit = lt;
            }
            return limit;
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
