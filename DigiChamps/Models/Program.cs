using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BotR.API;
using System.Xml.Linq;
using System.IO;
using System.Collections.Specialized;
using System.Security.Cryptography;
using System.Xml;
using Newtonsoft.Json;


  public  class Program
    {

        public string Programs( string filename, Stream filestream, int fileData)
        {

            string uploadResponse = string.Empty;
            try
            {

                BotRAPI api = new  BotRAPI("rhhqK8sD", "M8sVGyMY0tpIftRBJ2PPVuzz");
                //botrapi.BotRAPI api = new botrapi.BotRAPI("rhhqK8sD", "M8sVGyMY0tpIftRBJ2PPVuzz") ;
               
                //test video listing
                Console.WriteLine(api.Call("/videos/list"));

                //params to store with a new video
                NameValueCollection col = new NameValueCollection() {
                    
                {"title", "New test video"},
                    {"tags", "new, test, video, upload"},
                    {"description", "New video2"},
                    {"link", "http://www.bitsontherun.com"},
                    {"author", "Bits on the Run"}
                };

                //create the new vidoe
                string xml = api.Call("/videos/create", col);

                Console.WriteLine(xml);

                XDocument doc = XDocument.Parse(xml);
                var result = (from d in doc.Descendants("status")
                              select new
                              {
                                  Status = d.Value
                              }).FirstOrDefault();

                //make sure the status was "ok" before trying to upload
                if (result.Status.Equals("ok", StringComparison.CurrentCultureIgnoreCase))
                {

                    var response = doc.Descendants("link").FirstOrDefault();
                    string url = string.Format("{0}://{1}{2}", response.Element("protocol").Value, response.Element("address").Value, response.Element("path").Value);

                   // string filePath = System.IO.Path.GetTempPath();
                  //  string filePath = "D:\\Ongoing\\jwplayer\\jwplayer\\jwplayer-7.9.3\\test.mp4";
                    col = new NameValueCollection();
                  
                    col["file_size"] = fileData.ToString();
                    col["file_md5"] = BitConverter.ToString(HashAlgorithm.Create("MD5").ComputeHash(filestream)).Replace("-", "").ToLower();
                    col["key"] = response.Element("query").Element("key").Value;
                    col["token"] = response.Element("query").Element("token").Value;

                   // fs.Dispose();
                   uploadResponse = api.Upload(url, col, filename);
                   XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(uploadResponse);
                    string s = JsonConvert.SerializeXmlNode(xmlDoc);
                    
                }
                //Console.ReadKey();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.GetBaseException().Message);
                Console.ReadKey();
            }
                   return uploadResponse;
        }

    }
    
