using net.openstack.Core.Domain;
using net.openstack.Providers.Rackspace;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace DigiChamps.Models
{
    public class FileUpload
    {

        public static string Upload(Stream stream, string fileName)
        {
            CloudFilesProvider cloudFilesProvider = getProvider();
            IEnumerable<string> containerObjectList =
                cloudFilesProvider.ListObjects("dgcmps_files", region: "DFW")
                .Select(o => o.Name);
            //IEnumerable<ContainerObject> containerObjectList = cloudFilesProvider.ListObjects("dgcmps_files");
            string extension = Path.GetExtension(fileName);
            string name = Path.GetFileNameWithoutExtension(fileName);

            stream.Position = 0;

            //name = name.GenerateSlug(); //my method
            fileName = name + extension;

            while (containerObjectList.Contains(fileName))
                fileName = name + Guid.NewGuid().ToString().Split('-')[0] + extension;

            cloudFilesProvider
                .CreateObject("dgcmps_files", stream, fileName, region: "DFW");

            ContainerCDN strCdnURL = cloudFilesProvider.GetContainerCDNHeader("dgcmps_files", region: "DFW");
            string returnURL = strCdnURL.CDNUri +"/"+ fileName;
            return returnURL;
        }

        private static CloudFilesProvider getProvider()
        {
            CloudIdentity cloudIdentity =
                new CloudIdentity() { APIKey = "33bfbc17de8d4425ae80620cb5a49b25", Username = "ntsplapi" };
            return new CloudFilesProvider(cloudIdentity);
        }
    }
}