using DigiChamps.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;

namespace DigiChamps
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801
    public class MvcApplication : System.Web.HttpApplication
    {
        string con = ConfigurationManager.ConnectionStrings["sqlConString"].ConnectionString;
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            // GlobalFilters.Filters.Add(new DigiChamps.Controllers.StudentController.EnforceLowercaseUrlAttribute());
            // SqlDependency.Start(con);
        }

        protected void Session_Start(object sender, EventArgs e)
        {
            NotificationComponent NC = new NotificationComponent();
            var currentTime = DigiChampsModel.datetoserver();
            HttpContext.Current.Session["LastUpdated"] = currentTime;
            NC.RegisterNotification(currentTime);
            NC.TicketNotification(currentTime);
        }
        protected void Application_End()
        {
            //here we will stop Sql Dependency
            SqlDependency.Stop(con);
        }
        protected void Application_Error(object sender, EventArgs e)
        {
            var application = (HttpApplication)sender;

            var context = application.Context;
            //var context = new HttpContextWrapper(HttpContext.Current);
            var ex = context.Server.GetLastError().GetBaseException();
            if (ex != null)
            {
                string hostUrl = context.Request.Url.Scheme + "://" + context.Request.Url.Authority;
                string errorurl = hostUrl + "/Admin/Login";



                context.Response.Redirect(errorurl);
                return;

            }
        }
    }
}