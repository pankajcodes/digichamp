using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using DigiChamps.Models;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Web;

namespace digichamps_api.Controllers
{

   
    public class EditProfileController : ApiController
    {
        DigiChampsEntities db = new DigiChampsEntities();
        // GET: api/EditProfile
        DateTime today = DigiChampsModel.datetoserver();

        [HttpGet]
        public HttpResponseMessage profile(int? id)
        {

            try
            {
                if (id != null)
                {
                    //List<Digichamps.Exam_List> list = new List<Digichamps.Exam_List>();
                    string date = string.Empty;
                    string time = string.Empty;
                    string sec = string.Empty;
                    int Exam_ID = 0;
                    int Exam_Type = 0;
                    var data1 = db.tbl_DC_Registration.Where(x => x.Regd_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    var percent = db.SP_DC_Student_Profile_Progress(id).FirstOrDefault();
                    string[] getpercentage = Convert.ToString(percent).Split('.');
                    var test = getpercentage[0];
                    var data2 = db.tbl_DC_Registration_Dtl.Where(x => x.Regd_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    if (data2 != null)
                    {
                        var datelist = db.tbl_DC_Exam.Where(x => x.Is_Active == true && x.Is_Deleted == false && x.Exam_type == 4 && x.Class_Id == data2.Class_ID && x.Shedule_date >= today).OrderBy(x => x.Shedule_time).FirstOrDefault();
                        if (datelist != null)
                        {
                            var chapters = db.VW_DC_Package_Learn.Where(x => x.Regd_ID == id && x.Class_ID == data2.Class_ID).ToList();
                            if (chapters.Count > 0)
                            {
                                Exam_ID = datelist.Exam_ID;
                                Exam_Type = Convert.ToInt32(datelist.Exam_type);
                                date = Convert.ToDateTime(datelist.Shedule_date).ToString("yyyy-MM-dd");
                                time = datelist.Shedule_time + ":00";
                                string acctualtime = date + " " + time;
                                DateTime dt = Convert.ToDateTime(acctualtime);
                                int dt1 = (dt.Date - today.Date).Days;
                                if (dt1 <= 6)
                                {
                                    TimeSpan diff = dt - today;
                                    sec = diff.ToString();
                                }


                            }
                            else
                            {



                            }
                            //foreach (var item in datelist)
                            //{
                            //    date = Convert.ToDateTime(item.Shedule_date).ToString("MM/dd/yyyy");
                            //    time = item.Shedule_time + ":00";
                            //    string acctualtime = date + " " + time;
                            //    DateTime dt = Convert.ToDateTime(acctualtime);
                            //    int dt1 = (dt.Date - today.Date).Days;
                            //    if (dt1 <= 6)
                            //    {
                            //        TimeSpan diff = dt-today;
                            //        sec = diff.ToString();
                            //    }

                            //}
                        }
                    }


                    if (data1 != null)
                    {
                        var data = new Digichamps.profileget
                        {
                            success = (from c in db.tbl_DC_Registration.Where(x => x.Is_Active == true && x.Is_Deleted == false && x.Regd_ID == id)
                                       join d in db.tbl_DC_Registration_Dtl.Where(x => x.Is_Active == true && x.Is_Deleted == false) on c.Regd_ID equals d.Regd_ID

                                       join b in db.tbl_DC_Board.Where(x => x.Is_Active == true && x.Is_Deleted == false) on
                                       d.Board_ID equals b.Board_Id
                                       join l in db.tbl_DC_Class.Where(x => x.Is_Active == true && x.Is_Deleted == false) on
                                       d.Class_ID equals l.Class_Id
                                       select new Digichamps.Student_Profile
                                       {
                                           Regd_ID = c.Regd_ID,
                                           Customer_Name = c.Customer_Name,
                                           Email = c.Email,
                                           Board_ID = d.Board_ID,
                                           Class_ID = d.Class_ID,
                                           Board_Name = b.Board_Name,
                                           Class_Name = l.Class_Name,
                                           Mobile = c.Mobile,
                                           Image_Url = "/Images/Profile/" + c.Image,
                                           Profile_Status = test,
                                           testdate = date ,
                                           testtime = time ,
                                           remain_time = sec ,
                                           Exam_ID = Exam_ID ,
                                           Exam_Type = Exam_Type


                                       }).FirstOrDefault()


                        };

                        return Request.CreateResponse(HttpStatusCode.OK, data);
                    }
                    else
                    {
                        var obj = new Digichamps.errormessage
                        {
                            error = new Digichamps.Display
                            {
                                Message = "Invalid user details",
                            }
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, obj);
                    }

                }
                else
                {
                    var obj = new Digichamps.errormessage
                    {
                        error = new Digichamps.Display
                        {
                            Message = "Something Went Wrong.Please Try again",
                        }
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, obj);
                }
            }
            catch (Exception ex)
            {

               var obj = new Digichamps.errormessage
                    {
                        error = new Digichamps.Display
                        {
                            Message = "Error: "+ex.Message,
                        }
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, obj);
                }
            }

        

        [HttpGet]
        public HttpResponseMessage Getprofile(int? id)
        {

            try
            {
    
                if (id != null)
                {
                    var data1 = db.tbl_DC_Registration.Where(x => x.Regd_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                    var percent = db.SP_DC_Student_Profile_Progress(id).FirstOrDefault();
                    string[] getpercentage = Convert.ToString(percent).Split('.');
                    var test = getpercentage[0];
                    if (data1 != null)
                    {
                        var data = new Digichamps.profiledetails
                        {
                            success = (from c in db.tbl_DC_Registration.Where(x => x.Is_Active == true && x.Is_Deleted == false && x.Regd_ID == id)
                                       join d in db.tbl_DC_Registration_Dtl.Where(x => x.Is_Active == true && x.Is_Deleted == false) on c.Regd_ID equals d.Regd_ID
                                       join s in db.tbl_DC_Security_Question.Where(x => x.Is_Active == true && x.Is_Deleted == false) on d.Secure_Id equals s.Security_Question_ID
                                       join b in db.tbl_DC_Board.Where(x => x.Is_Active == true && x.Is_Deleted == false) on
                                       d.Board_ID equals b.Board_Id
                                       join l in db.tbl_DC_Class.Where(x => x.Is_Active == true && x.Is_Deleted == false) on
                                       d.Class_ID equals l.Class_Id
                                       select new Digichamps.Student_Edit
                                       {
                                           Regd_ID = c.Regd_ID,
                                           Customer_Name = c.Customer_Name,
                                           Email = c.Email,
                                           Phone = c.Phone,
                                           Board_ID = d.Board_ID,
                                           Class_ID = d.Class_ID,
                                           Board_Name = b.Board_Name,
                                           Class_Name = l.Class_Name,
                                           Mobile = c.Mobile,
                                           Organisation_Name = c.Organisation_Name == null ? "" : c.Organisation_Name,
                                           Pincode = c.Pincode == null ? "" : c.Pincode,
                                           Address = c.Address  == null ? "" : c.Address,
                                           Image_Url = "/Images/Profile/" + c.Image,
                                           Security_Question = s.Security_Question,
                                           Security_Question_ID = d.Secure_Id,
                                           Profile_Status = test,
                                           Security_Answer = d.Answer,

                                           securityquestions = (from q in db.tbl_DC_Security_Question.Where(x => x.Is_Active == true && x.Is_Deleted == false)
                                                                select new Digichamps.Security_Question
                                                                {
                                                                    Security_Questions = q.Security_Question,
                                                                    Security_Question_ID = q.Security_Question_ID
                                                                }).ToList()
                                       }).FirstOrDefault()


                        };

                        return Request.CreateResponse(HttpStatusCode.OK, data);
                    }
                    else
                    {
                        var obj = new Digichamps.errormessage
                        {
                            error = new Digichamps.Display
                            {
                                Message = "Invalid user details",
                            }
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, obj);
                    }

                }
                else
                {
                    var obj = new Digichamps.errormessage
                    {
                        error = new Digichamps.Display
                        {
                            Message = "Something Went Wrong.Please Try again",
                        }
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, obj);
                }
            }
            catch (Exception ex)
            {

                var obj = new Digichamps.errormessage
                {
                    error = new Digichamps.Display
                    {
                        Message = "Error"+ex.Message,
                    }
                };
                return Request.CreateResponse(HttpStatusCode.OK, obj);
            }

        }
        [HttpPost]
        public HttpResponseMessage postprofile([FromBody] Digichamps.Student_Edit obj)
        {
            try
            {
                if (obj.Customer_Name != null)
                {
                    if (obj.Board_ID != null)
                    {
                        if (obj.Class_ID != null)
                        {
                            if (obj.Email != null)
                            {
                                if (obj.Phone != null)
                                {
                                    if (obj.Organisation_Name != null)
                                    {
                                        if (obj.Security_Question_ID != null)
                                        {
                                            if (obj.Security_Answer != null)
                                            {
                                                tbl_DC_Registration_Dtl obj2 = db.tbl_DC_Registration_Dtl.Where(x => x.Regd_ID == obj.Regd_ID && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                                                tbl_DC_Registration obj3 = db.tbl_DC_Registration.Where(x => x.Regd_ID == obj.Regd_ID && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                                                if (obj2 != null)
                                                {
                                                    if (obj3 != null)
                                                    {
                                                        obj3.Customer_Name = obj.Customer_Name;
                                                        obj3.Email = obj.Email;
                                                        obj3.Phone = obj.Phone;
                                                        obj3.Organisation_Name = obj.Organisation_Name;
                                                        obj2.Board_ID = obj.Board_ID;
                                                        obj2.Class_ID = obj.Class_ID;
                                                        obj2.Secure_Id = obj.Security_Question_ID;
                                                        obj2.Answer = obj.Security_Answer;
                                                        db.Entry(obj3).State = EntityState.Modified;
                                                        db.SaveChanges();
                                                        db.Entry(obj2).State = EntityState.Modified;
                                                        db.SaveChanges();
                                                        var obj10 = new Digichamps.SuccessResult
                                                        {
                                                            success = new Digichamps.SuccessResponse
                                                            {
                                                                Message = "Profile Successfully Updated"
                                                            }
                                                        };
                                                        return Request.CreateResponse(HttpStatusCode.OK, obj10);
                                                    }
                                                    else
                                                    {
                                                        var obj20 = new Digichamps.errormessage
                                                        {
                                                            error = new Digichamps.Display
                                                            {
                                                                Message = "Invalid Details",
                                                            }
                                                        };
                                                        return Request.CreateResponse(HttpStatusCode.OK, obj20);
                                                    }
                                                }
                                                else
                                                {
                                                    var obj21 = new Digichamps.errormessage
                                                    {
                                                        error = new Digichamps.Display
                                                        {
                                                            Message = "Invalid Details",
                                                        }
                                                    };
                                                    return Request.CreateResponse(HttpStatusCode.OK, obj21);
                                                }

                                            }
                                            else
                                            {
                                                var obj10 = new Digichamps.errormessage
                                                {
                                                    error = new Digichamps.Display
                                                    {
                                                        Message = "Please Enter Security Answer",
                                                    }
                                                };
                                                return Request.CreateResponse(HttpStatusCode.OK, obj10);
                                            }
                                        }
                                        else
                                        {
                                            var obj11 = new Digichamps.errormessage
                                            {
                                                error = new Digichamps.Display
                                                {
                                                    Message = "Please Select Security Question",
                                                }
                                            };
                                            return Request.CreateResponse(HttpStatusCode.OK, obj11);
                                        }
                                    }
                                    else
                                    {
                                        var obj12 = new Digichamps.errormessage
                                        {
                                            error = new Digichamps.Display
                                            {
                                                Message = "Please Enter Organisation Name",
                                            }
                                        };
                                        return Request.CreateResponse(HttpStatusCode.OK, obj12);
                                    }
                                }
                                else
                                {
                                    var obj13 = new Digichamps.errormessage
                                    {
                                        error = new Digichamps.Display
                                        {
                                            Message = "Please Enter Mobile Number",
                                        }
                                    };
                                    return Request.CreateResponse(HttpStatusCode.OK, obj13);
                                }
                            }
                            else
                            {
                                var obj14 = new Digichamps.errormessage
                                {
                                    error = new Digichamps.Display
                                    {
                                        Message = "Please Enter Email",
                                    }
                                };
                                return Request.CreateResponse(HttpStatusCode.OK, obj14);
                            }

                        }
                        else
                        {
                            var obj14 = new Digichamps.errormessage
                            {
                                error = new Digichamps.Display
                                {
                                    Message = "Please Select Class",
                                }
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, obj14);
                        }

                    }
                    else
                    {
                        var obj14 = new Digichamps.errormessage
                        {
                            error = new Digichamps.Display
                            {
                                Message = "Please Select Board",
                            }
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, obj14);
                    }

                }
                else
                {
                    var obj15 = new Digichamps.errormessage
                    {
                        error = new Digichamps.Display
                        {
                            Message = "Please Enter Name",
                        }
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, obj15);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
            return Request.CreateResponse(HttpStatusCode.OK);
        }
        [HttpPost]
        public HttpResponseMessage ChangePassword(Digichamps_web_Api.changepassword obj)
        {
            try
            {
                var stuobj = db.tbl_DC_Registration.Where(x => x.Regd_ID == obj.id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();
                if (stuobj != null)
                {
                    string old_encrypt = DigiChampsModel.Encrypt_Password.HashPassword(obj.old_password).ToString();
                    var data = db.tbl_DC_USER_SECURITY.Where(x => x.USER_NAME == stuobj.Mobile && x.PASSWORD == old_encrypt && x.STATUS == "A").FirstOrDefault();

                    if (data != null)
                    {
                        if (obj.new_password == obj.confirm_password)
                        {
                            string new_pass_word = DigiChampsModel.Encrypt_Password.HashPassword(obj.new_password).ToString();

                            data.PASSWORD = new_pass_word;
                            db.Entry(data).State = EntityState.Modified;
                            db.SaveChanges();
                            var obj10 = new Digichamps_web_Api.chngpasswordresult
                            {
                                success = new Digichamps_web_Api.ChangepasswordResponse
                                {
                                    Message = "Password is changed successfully"
                                }
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, obj10);
                        }
                        else
                        {
                            var retobj = new Digichamps.errormessage
                            {
                                error = new Digichamps.Display
                                {
                                    Message = "The new password & confirm password are not matching",
                                }
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, retobj);
                        }
                    }
                    else
                    {
                        var retobj = new Digichamps.errormessage
                        {
                            error = new Digichamps.Display
                            {
                                Message = "current password is wrong",
                            }
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, retobj);
                    }
                }
                else {
                    var retobj = new Digichamps.errormessage
                    {
                        error = new Digichamps.Display
                        {
                            Message = "Invalid user details.",
                        }
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, retobj);
                }

            }
            catch
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }


        }

        [ActionName("PostUserImage")]
        [HttpPost]

        public async Task<HttpResponseMessage> PostUserImage()
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            try
            {

                var httpRequest = HttpContext.Current.Request;
                int regdid = Convert.ToInt32(httpRequest.Form["Regd_ID"]);
                var obj = db.tbl_DC_Registration.Where(x => x.Regd_ID == regdid).FirstOrDefault();
                foreach (string file in httpRequest.Files)
                {
                    HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created);

                    var postedFile = httpRequest.Files[file];
                    if (postedFile != null && postedFile.ContentLength > 0)
                    {

                        int MaxContentLength = 1024 * 1024 * 4; //Size = 4 MB

                        IList<string> AllowedFileExtensions = new List<string> { ".jpg", ".gif", ".png" };
                        var ext = postedFile.FileName.Substring(postedFile.FileName.LastIndexOf('.'));
                        var extension = ext.ToLower();
                        if (!AllowedFileExtensions.Contains(extension))
                        {

                            var message = string.Format("Please Upload image of type .jpg,.gif,.png.");

                            var obj2 = new DigiChamps.Models.Digichamps_web_Api.Errorresult
                            {
                                Error = new DigiChamps.Models.Digichamps_web_Api.Errorresponse
                                {

                                    Message = message,



                                }


                            };
                            return Request.CreateResponse(HttpStatusCode.OK, obj2);
                        }
                        else if (postedFile.ContentLength > MaxContentLength)
                        {



                            var obj2 = new DigiChamps.Models.Digichamps_web_Api.Errorresult
                            {
                                Error = new DigiChamps.Models.Digichamps_web_Api.Errorresponse
                                {

                                    Message = "Please Upload a file upto 4 mb",



                                }


                            };
                            return Request.CreateResponse(HttpStatusCode.OK, obj2);
                        }
                        else
                        {
                            if (obj != null)
                            {
                                string guid = Guid.NewGuid().ToString();
                                var filePath = HttpContext.Current.Server.MapPath("~/Images/Profile/" + guid + postedFile.FileName);
                                postedFile.SaveAs(filePath);

                                obj.Image = guid + postedFile.FileName;
                                db.SaveChanges();
                            }
                            else
                            {
                                var obj6 = new DigiChamps.Models.Digichamps_web_Api.Errorresult
                                {
                                    Error = new DigiChamps.Models.Digichamps_web_Api.Errorresponse
                                    {

                                        Message = "Regdid is not present in database",



                                    }


                                };
                                return Request.CreateResponse(HttpStatusCode.OK, obj6);

                            }
                        }
                    }
                    if (obj != null)
                    {
                        var obj10 = new DigiChamps.Models.Digichamps_web_Api.Successimageresult
                        {
                            Success = new DigiChamps.Models.Digichamps_web_Api.Successimageresponse
                            {

                                Message = "Images updated  successfully ",

                                image = "/Images/Profile/" + obj.Image,
                                Regd_ID = regdid

                            }


                        };
                        return Request.CreateResponse(HttpStatusCode.OK, obj10);
                    }
                }
                var obj11 = new DigiChamps.Models.Digichamps_web_Api.Errorresult
                {
                    Error = new DigiChamps.Models.Digichamps_web_Api.Errorresponse
                    {

                        Message = "Please Upload a Image",



                    }


                };
                return Request.CreateResponse(HttpStatusCode.OK, obj11);
            }
            catch (Exception ex)
            {
                var res = string.Format("Please try later");
                dict.Add("error", res);
                return Request.CreateResponse(HttpStatusCode.NotFound, dict);
            }
        }




        [HttpGet]
        public HttpResponseMessage globaltest(int? id)
        {

            try
            {
                if (id != null)
                {
                    //List<Digichamps.Exam_List> list = new List<Digichamps.Exam_List>();
                    string date = string.Empty;
                    string time = string.Empty;
                    string sec = string.Empty;
                   
                    var data1 = db.tbl_DC_Registration.Where(x => x.Regd_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();

                    var data2 = db.tbl_DC_Registration_Dtl.Where(x => x.Regd_ID == id && x.Is_Active == true && x.Is_Deleted == false).FirstOrDefault();


                    if (data1 != null)
                    {
                        var data = new Digichamps.globaltest
                        {
                            success = new Digichamps.globalexamlist
                            {
                               
                            
                         examlists =    (from c in db.tbl_DC_Exam.Where(x => x.Is_Active == true && x.Is_Deleted == false && x.Exam_type == 4 && x.Class_Id == data2.Class_ID && x.Shedule_date >= today).OrderBy(x => x.Shedule_time)
                                       select new Digichamps.Global_testdata
                                       {
                                           Regd_ID =data1.Regd_ID,
                                          
                                          
                                           
                                           testdate = c.Shedule_date,
                                           testtime = c.Shedule_time,
                                          
                                           Exam_ID = c.Exam_ID,
                                           Exam_Type = c.Exam_type,
                                           Examname = c.Exam_Name


                                       }).ToList()

                            }
                        };

                        return Request.CreateResponse(HttpStatusCode.OK, data);
                    }
                    else
                    {
                        var obj = new Digichamps.errormessage
                        {
                            error = new Digichamps.Display
                            {
                                Message = "Invalid user details",
                            }
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, obj);
                    }

                }
                else
                {
                    var obj = new Digichamps.errormessage
                    {
                        error = new Digichamps.Display
                        {
                            Message = "Something Went Wrong.Please Try again",
                        }
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, obj);
                }
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }

        }

        public string Exam_Name { get; set; }
        //[HttpPost]
        //public HttpResponseMessage mapping()
        //{
        //    try
        //    {
        //        var httprequest = HttpContext.Current.Request;
               
        //        string lattitude = Convert.ToString(httprequest.Form["Lattitude"]);
        //        string longitude = Convert.ToString(httprequest.Form["Longitude"]);
        //        Test obj = new Test();
        //        if (httprequest.Files.Count > 0)
        //        {
        //            string guid = Guid.NewGuid().ToString();
        //            //var docfile = new List<string>();
        //            foreach (string files in httprequest.Files)
        //            {
        //                var postedfile = httprequest.Files[files];
        //                var path = HttpContext.Current.Server.MapPath("~/Images/Profile/" + guid + postedfile.FileName);
        //                postedfile.SaveAs(path);
        //                //docfile.Add(path);
        //                obj.Image = guid + postedfile.FileName;
        //            }
        //        }
              
        //        obj.latt = Convert.ToDecimal(lattitude);
        //        obj.longitude = Convert.ToDecimal(longitude);
        //        db.Tests.Add(obj);
        //        db.SaveChanges();

        //        var obj1 = new SuccessMessage
        //        {
        //            Success = new Test_Demo
        //            {
        //                Image = obj.Image,
        //                Id = obj.id,
        //                Lattitude = obj.latt,
        //                Longitude = obj.longitude
        //            }
                   
        //    };
        //        return Request.CreateResponse(HttpStatusCode.OK, obj1);
        //    }
        //    catch (Exception ex)
        //    {
        //    return Request.CreateResponse(HttpStatusCode.OK, "Error"+ex.Message);
                
        //    }
            
        //}
        public class Test_Demo
        {
            public string Image { get; set; }
            public decimal? Lattitude { get; set; }
            public decimal? Longitude { get; set; }
            public int? Id { get; set; }
        }
        public class SuccessMessage
        {
            public Test_Demo Success { get; set; }
        }
    }
}
