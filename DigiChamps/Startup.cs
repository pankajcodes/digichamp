using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using webApiTokenAuthentication;
using System.Web.Http;
using DigiChamps;
using Microsoft.Owin.Security.OAuth;

[assembly: OwinStartup(typeof(PushNotification.Startup))]

namespace PushNotification
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var myProvider = new MyAuthorizationServerProvider();
            OAuthAuthorizationServerOptions options = new OAuthAuthorizationServerOptions
            {
                AllowInsecureHttp = true,
                TokenEndpointPath = new PathString("/token"),
                AccessTokenExpireTimeSpan = TimeSpan.FromDays(10000),
                Provider = myProvider
            };
            app.UseOAuthAuthorizationServer(options);
            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions());


            HttpConfiguration config = new HttpConfiguration();
            WebApiConfig.Register(config);
        }
    }
}
