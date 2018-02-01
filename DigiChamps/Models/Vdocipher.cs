using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Web;

namespace DigiChamps.Models
{
    public class Vdocipher
    {
        public string videoupload(string url, string key, string XAmzCredential, string XAmzAlgorithm, string XAmzDate, string Policy, string XAmzSignature, string success_action_status, string success_action_redirect, Stream filestream, string filename)
        {
            HttpContent stringContent0 = new StringContent(key);
            HttpContent stringContent7 = new StringContent(XAmzCredential);
            HttpContent stringContent1 = new StringContent(XAmzAlgorithm);
            HttpContent stringContent2 = new StringContent(XAmzDate);
            HttpContent stringContent3 = new StringContent(Policy);
            HttpContent stringContent4 = new StringContent(XAmzSignature);
            HttpContent stringContent5 = new StringContent(success_action_status);
            HttpContent stringContent6 = new StringContent(success_action_redirect);
            // examples of converting both Stream and byte [] to HttpContent objects
            // representing input type file
            HttpContent fileStreamContent = new StreamContent(filestream);
            //HttpContent bytesContent = new ByteArrayContent(file.ContentLength);

            // Submit the form using HttpClient and 
            // create form data as Multipart (enctype="multipart/form-data")

            using (var client = new HttpClient())
            using (var formData = new MultipartFormDataContent())
            {
                // Add the HttpContent objects to the form data

                // <input type="text" name="filename" />
                formData.Add(stringContent0, "key", key);
                formData.Add(stringContent7, "X-Amz-Credential", XAmzCredential);
                formData.Add(stringContent1, "X-Amz-Algorithm", XAmzAlgorithm);
                formData.Add(stringContent2, "X-Amz-Date", XAmzDate);
                formData.Add(stringContent3, "Policy", Policy);
                formData.Add(stringContent4, "X-Amz-Signature", XAmzSignature);
                formData.Add(stringContent5, "success_action_status", success_action_status);
                formData.Add(stringContent6, "success_action_redirect", success_action_redirect);
                // <input type="file" name="file1" />
                formData.Add(fileStreamContent, "file", filename);
                // <input type="file" name="file2" />


                // Actually invoke the request to the server

                // equivalent to (action="{url}" method="post")
                var response = client.PostAsync(url, formData).Result;

                // equivalent of pressing the submit button on the form

                return null;
            }


        }
    }
    
}