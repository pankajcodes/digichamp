using DigiChamps.Models;
using Microsoft.Owin.Security.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;

namespace webApiTokenAuthentication
{
    public class MyAuthorizationServerProvider : OAuthAuthorizationServerProvider
    {
        DigiChampsEntities dbContext = new DigiChampsEntities();
        public override async Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            context.Validated(); // 
        }
     
        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            //var authouser = dbContext.tbl_DC_USER_SECURITY.Where(x => x.ROLE_CODE == "A").FirstOrDefault();
            var identity = new ClaimsIdentity(context.Options.AuthenticationType);
            if (context.UserName == "admin" && context.Password == "admin")
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, "admin"));
                identity.AddClaim(new Claim("username", "admin"));
                identity.AddClaim(new Claim(ClaimTypes.Name, "Just Verified"));
                context.Validated(identity);
            }
            //else if (context.UserName == authouser.USER_NAME && context.Password == authouser.PASSWORD)
            //{
            //    identity.AddClaim(new Claim(ClaimTypes.Role, "user"));
            //    identity.AddClaim(new Claim("username", "user"));
            //    identity.AddClaim(new Claim(ClaimTypes.Name, "task ease"));
            //    context.Validated(identity);
            //}
            //else
            //{
            //    context.SetError("invalid_grant", "Provided username and password is incorrect");
            //    return;
            //}
        }
    }
}