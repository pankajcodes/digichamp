using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Security;
using System.Text;
using System.Web.Hosting;
using Newtonsoft.Json;

namespace DigiChamps.Models
{
    public class LeadsSquaredAPI
    {
        public bool submitQueryAPI(string email, string mobile, string firstname, string lastname)
        {
            try
            {
                if (email != null)
                {


                    var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://api.leadsquared.com/v2/LeadManagement.svc/Lead.CreateOrUpdate?postUpdatedLead=false&accessKey=u$rae7ea41f3b4cf7df442def5028df8b28&secretKey=e4c7ec2b59c90251e9eb148d8c65115c86085354");
                    httpWebRequest.ContentType = "application/json";//; charset=utf-8;
                    httpWebRequest.Method = "POST";

                    using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                    {

                        string json = "";
                        json += "[{\"Attribute\": \"EmailAddress\",\"Value\": \"" + email + "\"},{\"Attribute\": \"FirstName\",\"Value\": \"" + firstname + "\"},{\"Attribute\": \"LastName\",\"Value\": \"" + lastname + "\"},{\"Attribute\": \"Phone\",\"Value\": \"" + mobile + "\" },{\"Attribute\": \"SearchBy\",\"Value\": \"EmailAddress\"}]";

                        json = json.Replace("\r\n", "");
                        streamWriter.Write(json);
                    }

                    var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        var result = streamReader.ReadToEnd();
                        //var ob5 = JsonConvert.DeserializeObject<JVtaskupdate.InsertResultClass>(result);
                        //if (ob5.Result.Count > 0)
                        //{
                        //    ViewBag.Total = ob5.Result.Count;
                        //    int count = 0;
                        //    for (int i = 0; i <= ob5.Result.Count - 1; i++)
                        //    {
                        //        int smaid = Convert.ToInt32(ob5.Result.ToList()[i].SMAID);
                        //        tbl_JV_Order_Job_Dtls_Sts ob6 = dbContext.tbl_JV_Order_Job_Dtls_Sts.Where(x => x.Job_Order_Dtl_ID == smaid).FirstOrDefault();

                        //        if (ob5.Result.ToList()[i].message == "Success")
                        //        {
                        //            count = count + 1;
                        //            ViewBag.success = count;
                        //            ob6.Job_Status = "verifying";
                        //            dbContext.Entry(ob6).State = EntityState.Modified;
                        //            dbContext.SaveChanges();
                        //        }
                        //    }
                        //    ViewBag.failed = ViewBag.Total - ViewBag.success;
                        //}


                    }
                    return true;
                }
            }
            catch (Exception ex)
            {

                return false;
            }
            return true;
        }

    }


}