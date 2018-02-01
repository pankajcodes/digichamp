using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace DigiChamps
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
               routeTemplate: "api/{controller}/{action}/{id}/{eid}/{ClsId}/{SubId}",
                defaults: new { id = RouteParameter.Optional, eid = RouteParameter.Optional, ClsId = RouteParameter.Optional, SubId = RouteParameter.Optional }

            );
            config.Formatters.Remove(config.Formatters.XmlFormatter);
        }
        
    }
}
