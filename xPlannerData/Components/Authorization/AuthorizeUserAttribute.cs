using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Http;

namespace xPlannerData.Components.Authorization
{
    public class AuthorizeUserAttribute : AuthorizeAttribute
    {
        public const string secretKey = "bHwa9saNwB615FlIKEBKTk9oLR8vfmP6m8iQ5cZl"; // secret key to encrypt/decrypt the token
        public const string jwtTokenHeader = "Auth-Token";

        // override the OnAuthorization method
        public override void OnAuthorization(System.Web.Http.Controllers.HttpActionContext actionContext)
        {
            string token; // stores the token

            // verifies if the header contains a token
            if (actionContext.Request.Headers.Authorization != null)
            {

                if (actionContext.Request.Headers.Authorization.Scheme.Equals("Bearer") &&
                    actionContext.Request.Headers.Authorization.Parameter != null)
                {
                    // Get the tokens
                    token = actionContext.Request.Headers.Authorization.Parameter;

                    // Decrypt the token
                    try
                    {
                        var payload = JWT.JsonWebToken.DecodeToObject(token, secretKey) as IDictionary<string, object>;
                        string pId = payload["id"].ToString();
                        string username = payload["username"].ToString();
                        string domains = payload["domains"].ToString();
                        string roles = payload["roles"].ToString();
                        long expire = (long)payload["expireDate"];

                        // Verifies if the token has expired
                        var current_date = DateTime.Now;
                        if (expire >= current_date.Ticks)
                        {
                            // Verifiy the roles
                            string[] userRoles = roles.Replace(" ", "").Split(',');
                            string[] methodRoles = !Roles.Equals("") ? Roles.Replace(" ", "").Split(',') : null;

                            bool containRole = methodRoles == null;
                            int cont = 0;
                            while (!containRole && cont < userRoles.Count())
                            {
                                containRole = methodRoles.Contains(userRoles[cont]);
                                cont++;
                            }

                            if (!containRole)
                            {
                                actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Forbidden, "Role!"); // TODO
                                return;
                            }

                            // Generate a new token, with a new date
                            SetJwtTokenInHttpHeader(pId, username, roles, domains);
                        }
                        else // Authorization is not possible, token has expired
                        {
                            actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized, "expired");
                            return;
                        }

                    }
                    catch (Exception)
                    {
                        // Error in try Decrypt the token, authorization is not possible
                        actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized, "Valid authorization token not found in the HTTP header!");
                        return;

                    }
                } else
                {
                    actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized, "Valid authorization token not found in the HTTP header!");
                    return;
                }
            }
            else // No token found in the http header
            {
                actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized, "Valid authorization token not found in the HTTP header!");
                return;
            }
        }

        // Create a new token
        public static void SetJwtTokenInHttpHeader(string id, string username, string roles, string domains)
        {
            // Create the object with user's information and expiration time
            var payload = new Dictionary<string, object>() {
                {"id", id},
                {"username", username},
                {"roles", roles },
                {"domains", domains},
                {"expireDate", DateTime.Now.AddMinutes(10).Ticks} // 10 minutes to expire
            };

            // Encrypt the object
            string token = JWT.JsonWebToken.Encode(payload, AuthorizeUserAttribute.secretKey, JWT.JwtHashAlgorithm.HS512);
            // Sets the principal with the domains and id information
            var aux = new GenericIdentity(username);
            aux.AddClaim(new Claim("domains", string.Join(",", domains)));
            aux.AddClaim(new Claim("id", id));
            HttpContext.Current.User = new GenericPrincipal(aux, roles.Split(','));
            // Put the token in the response header
            HttpContext.Current.Response.AppendHeader(jwtTokenHeader, token);
        }

        // Return the domains of the current user
        public static int[] GetCurrentDomains()
        {
            return new int[] { 1 };
            //if (HttpContext.Current.User != null && HttpContext.Current.User.Identity.IsAuthenticated)
            //{
            //    var domains = ((ClaimsIdentity)HttpContext.Current.User.Identity).FindFirst("domains").Value;
            //    if (domains.Length == 0)
            //        return null;
            //    return ((ClaimsIdentity)HttpContext.Current.User.Identity).FindFirst("domains").Value.Split(',')
            //        .ToList().ConvertAll(s => Int32.Parse(s)).ToArray();
            //}
            
        }

        // Return the current user's id
        public static string GetCurrentId()
        {
            if (HttpContext.Current.User != null && HttpContext.Current.User.Identity.IsAuthenticated)
            {
                return ((ClaimsIdentity)HttpContext.Current.User.Identity).FindFirst("id").Value;
            }

            return null;
        }

        // Return the current user's email
        public static string GetCurrentEmail()
        {
            if (HttpContext.Current.User != null && HttpContext.Current.User.Identity.IsAuthenticated)
            {
                return ((ClaimsIdentity)HttpContext.Current.User.Identity).Name;
            }

            return null;
        }
    }
}