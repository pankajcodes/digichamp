using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Collections.Specialized;
using System.Security.Cryptography;
using System.Web;
using System.Text.RegularExpressions;

namespace BotR.API {

    public class BotRAPI {

        private string _apiURL = "";

        private string _args = "";

        private NameValueCollection _queryString = null;
        
        public string Key { get; set; }

        public string Secret { get; set; }

        public string APIFormat { get; set; }

        public BotRAPI(string key, string secret) : this("http://api.bitsontherun.com", "v1", key, secret) { }

        public BotRAPI(string url, string version, string key, string secret) {

            Key = key;
            Secret = secret;

            _apiURL = string.Format("{0}/{1}", url, version);            
        }

        /// <summary>
     
        public string Call(string apiCall) {
            return Call(apiCall, null);
        }

      
     
        public string Call(string apiCall, NameValueCollection args) {

            _queryString = new NameValueCollection();

            //add the non-required args to the required args
            if (args != null)
            {
                foreach (string k in args.Keys)
                {
                    _queryString.Add(k, UrlEncodeUCase(args.Get(k), Encoding.UTF8));
                }
            }
            buildArgs();
            WebClient client = createWebClient();

            string callUrl = _apiURL + apiCall;

            try
            {
                return client.DownloadString(callUrl);
            }
            catch
            {
                return "";
            }                                                               
        }

      
        public string Upload(string uploadUrl, string filePath)
        {

            WebClient client = createWebClient();

            string callUrl = _apiURL + uploadUrl;

            try
            {
                byte[] response = client.UploadFile(callUrl, filePath);
                return Encoding.UTF8.GetString(response);
            }
            catch
            {
                return "";
            }
        }

      
        public string Upload(string uploadUrl, NameValueCollection args, string filePath)
        {

            _queryString = args; //no required args

            
            WebClient client = createWebClient();
            _queryString["api_format"] = APIFormat ?? "xml";                                 
            queryStringToArgs();

            string callUrl = uploadUrl + "?" + _args;

            try {
                byte[] response = client.UploadFile(callUrl, filePath);
                 return Encoding.UTF8.GetString(response);     
            } catch {
                return "";
            }   
        }

        /// <summary>
       
        private string signArgs() {

            queryStringToArgs();

            HashAlgorithm ha = HashAlgorithm.Create("SHA");
            byte[] hashed = ha.ComputeHash(Encoding.UTF8.GetBytes(_args + Secret));
            return BitConverter.ToString(hashed).Replace("-", "").ToLower();
        }

       
        private void queryStringToArgs() {

            Array.Sort(_queryString.AllKeys);
            StringBuilder sb = new StringBuilder();

            foreach (string key in _queryString.AllKeys) {
                sb.AppendFormat("{0}={1}&", key, _queryString[key]);
            }
            sb.Remove(sb.Length - 1, 1); //remove trailing &

            _args = sb.ToString();            

        }

     
        private void buildArgs() {

            _queryString["api_format"] = APIFormat ?? "xml"; 
            _queryString["api_key"] = Key;
            _queryString["api_kit"] = "dnet-1.0";
            _queryString["api_nonce"] = string.Format("{0:00000000}", new Random().Next(99999999));
            _queryString["api_timestamp"] = getUnixTime().ToString();
            _queryString["api_signature"] = signArgs();            

            _args = string.Concat(_args, "&api_signature=", _queryString["api_signature"]);            
        }

       
        private WebClient createWebClient() {

            ServicePointManager.Expect100Continue = false; 
            WebClient client = new WebClient();
            client.BaseAddress = _apiURL;
            client.QueryString = _queryString;
            client.Encoding = UTF8Encoding.UTF8;
            return client;
        }

        
        private int getUnixTime() {
            return (int)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;            
        }

      
        private string UrlEncodeUCase(string data, Encoding enc)
        {
            data = Regex.Replace(HttpUtility.UrlEncode(data), "(%[0-9a-f][0-9a-f])", c => c.Value.ToUpper());
           
            data = data.Replace("+", "%20");
            
            data = data.Replace("%7E", "~");

            return data;
        }

        public string getsignedurl(string path, int timeout, string domain, string secret)
        {
            string url = "";
            int expires = 0;
            string signature = null;

            if (timeout != null)
            {
                expires = getUnixTime() + timeout;
            }
            else
            {
                expires = getUnixTime() + 3600;
            }

            signature = getMd5Hash(path + ":" + expires + ":" + secret);

            if (domain != null)
            {
                url = "http://" + domain + "/" + path + "?exp=" + expires + "&sig=" + signature;
            }
            else
            {
                url = "http://content.jwplatform.com/" + path + "?exp=" + expires + "&sig=" + signature;
            }

            return url;
        }

        private static string getMd5Hash(string input)
        { // Create a new instance of the MD5CryptoServiceProvider object.
            MD5 md5Hasher = MD5.Create(); // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(input));
            // Create a new Stringbuilder to collect the bytes // and create a string.
            StringBuilder sBuilder = new StringBuilder(); // Loop through each byte of the hashed data // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            // Return the hexadecimal string.
            return sBuilder.ToString();
        }
    }
}
