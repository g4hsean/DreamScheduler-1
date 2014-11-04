using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using DreamSchedulerApplication.Models;
using Neo4jClient;

namespace DreamSchedulerApplication
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
        protected void Application_PostAuthenticateRequest(Object sender, EventArgs e)
        {
            if (FormsAuthentication.CookiesSupported == true)
            {
                if (Request.Cookies[FormsAuthentication.FormsCookieName] != null)
                {
                    try
                    {
                        //let us take out the username now                
                        string username = FormsAuthentication.Decrypt(Request.Cookies[FormsAuthentication.FormsCookieName].Value).Name;
                        string roles = string.Empty;

                        var graphClient = new GraphClient(new Uri("http://localhost:7474/db/data"));
                        graphClient.Connect();

                        User user = new User();

                        try
                        {
                            user = graphClient
                                  .Cypher
                                  .Match("(n:User)")
                                  .Where(((User n) => n.Username == username))
                                  .Return(n => n.As<User>())
                                  .Results.FirstOrDefault();
                        }
                        catch (InvalidOperationException)
                        {
                            //  Will occur if 
                            //  1) the database has no users
                            //  2) User not found, should not happen
                        }


                        roles = user.Roles;

                        //Let us set the Pricipal with our user specific details
                        HttpContext.Current.User = new System.Security.Principal.GenericPrincipal(
                          new System.Security.Principal.GenericIdentity(username, "Forms"), roles.Split(';'));
                    }
                    catch (Exception ex)
                    {
                        throw ex; //something went wrong
                    }
                }
            }
        } 


    }
}
