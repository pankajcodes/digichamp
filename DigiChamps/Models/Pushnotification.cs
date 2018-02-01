using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;

namespace DigiChamps.Models
{
    public class Pushnotification
    {
        public Pushnotification(string bodys , string deviceId)
        {
        try
            {
                string applicationID = "AIzaSyD9lb6427EJTG0Frw61Bnc4fnwUHgk0JIs";

                string senderId = "226584391764";

                

                WebRequest tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send");
                tRequest.UseDefaultCredentials = true;

                tRequest.PreAuthenticate = true;

                tRequest.Credentials = CredentialCache.DefaultCredentials;

        
                tRequest.Method = "POST";

                tRequest.ContentType = "application/json";

                var data = new

                {

                    to = deviceId,

                    notification = new

                    {

                        body = bodys,

                        title = "",

                        icon = "myicon"

                    }    
                };       

                var serializer = new JavaScriptSerializer();

                var json = serializer.Serialize(data);

                Byte[] byteArray = Encoding.UTF8.GetBytes(json);

                tRequest.Headers.Add(string.Format("Authorization: key={0}", applicationID));

                tRequest.Headers.Add(string.Format("Sender: id={0}", senderId));

                tRequest.ContentLength = byteArray.Length; 


                using (Stream dataStream = tRequest.GetRequestStream())
                {

                    dataStream.Write(byteArray, 0, byteArray.Length);   


                    using (WebResponse tResponse = tRequest.GetResponse())
                    {

                        using (Stream dataStreamResponse = tResponse.GetResponseStream())
                        {

                            using (StreamReader tReader = new StreamReader(dataStreamResponse))
                            {

                                String sResponseFromServer = tReader.ReadToEnd();

                                string str = sResponseFromServer;

                            }    
                        }    
                    }    
                }    
            }        

            catch (Exception ex)
            {

                string str = ex.Message;

            }          
    }
    }
 public class pushnoti
    {
        public string  bodys { get; set; }
        public string deviceid { get; set; }

    }
}