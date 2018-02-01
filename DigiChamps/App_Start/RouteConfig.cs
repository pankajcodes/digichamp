using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace DigiChamps
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.LowercaseUrls = true;

            routes.MapRoute(
                name: "student",
                url: "{controller}/{action}/{id}/{eid}",
                defaults: new { controller = "student", action = "index", id = UrlParameter.Optional, eid = UrlParameter.Optional }
            ).DataTokens.Add("area", "Student"); 
            
            //routes.MapRoute(
            //    name: "Default",
            //    url: "{controller}/{action}/{id}",
            //    defaults: new { controller = "Student", action = "Index", id = UrlParameter.Optional }
            //);
        }
    }
}