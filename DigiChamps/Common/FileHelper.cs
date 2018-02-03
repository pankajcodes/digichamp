using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using DigiChamps.Models;

namespace DigiChamps.Common
{
    public class FileHelper
    {

        public UploadFileDetailModel UploadDoc(HttpFileCollectionBase files, string moduleName, string uniquePath,string folderName, string browserType)
        {
            UploadFileDetailModel uploadFileDetailModel = new UploadFileDetailModel();
            
            string fname = null;

            for (int i = 0; i < files.Count; i++)
            {
                HttpPostedFileBase file = files[i];

                if (browserType == "IE" || browserType == "INTERNETEXPLORER")
                {
                    string[] testfiles = file.FileName.Split(new char[] { '\\' });
                    fname = testfiles[testfiles.Length - 1];
                }
                else
                {
                    fname = file.FileName;
                }
                
                if (!string.IsNullOrEmpty(fname) && file.ContentLength>0 &&( file.ContentType.ToLower() != "image/jpg" && file.ContentType.ToLower() != "image/jpeg" && file.ContentType.ToLower() != "image/pjpeg" && file.ContentType.ToLower() != "image/gif" && file.ContentType.ToLower() != "image/x-png" && file.ContentType.ToLower() != "image/png"))
                {
                    uploadFileDetailModel.VideoName=DateTime.Now.ToString("yyyyMMddHHmmssfff") + "_"+fname;
                    uploadFileDetailModel.VideoPath = "/Upload/" + moduleName + "/" + uniquePath.ToString() + "/Video";
                    string path = System.Web.HttpContext.Current.Server.MapPath("~/Upload/") + moduleName + "/" + uniquePath.ToString() + "/Video";
                    //string subPath = "~/Upload/" + uploadRoot;

                    bool exists = System.IO.Directory.Exists(path);

                    if (!exists)
                        System.IO.Directory.CreateDirectory(path);
                    // objimage.Add(Path.Combine("/Upload/" + "Product/" + productId + "/" + fname));

                    file.SaveAs(Path.Combine(path, uploadFileDetailModel.VideoName));
                    Stream strm = file.InputStream;
                    //objSchoolInformation.DocumentaryVideo = "~/Upload/School/Video/" + fname;
                    //CreateThumbnail(strm, fname);
                }
                else
                {
                    if (!string.IsNullOrEmpty(fname) && file.ContentLength > 0)
                    {
                        uploadFileDetailModel.ImageName = DateTime.Now.ToString("yyyyMMddHHmmssfff") + "_" + fname;
                        uploadFileDetailModel.ImagePath = "/Upload/" + moduleName + "/" + uniquePath.ToString() + "/" + folderName;

                        string path = System.Web.HttpContext.Current.Server.MapPath("~/Upload/") + moduleName + "/" + uniquePath.ToString() + "/" + folderName;
                        //string subPath = "~/Upload/" ;
                        bool exists = System.IO.Directory.Exists(path);

                        if (!exists)
                            System.IO.Directory.CreateDirectory(path);
                        // objimage.Add(Path.Combine("/Upload/" + "Product/" + productId + "/" + fname));

                        file.SaveAs(Path.Combine(path, uploadFileDetailModel.ImageName));
                    }
                    //objSchoolInformation.Logo = "~/Upload/School/Image/" + fname;
                    //objSchoolInformation.ThumbnailPath = path;
                }
               
            }
            return uploadFileDetailModel;
        }
    }
}