using System.Web.Mvc;
using System.Web.Mvc.Filters;
using DreamSchedulerApplication;
using System.Web.Security;
using DreamSchedulerApplication.getCookieInfo;
using System;

namespace DreamSchedulerApplication.CustomAttributes
{
    public class AdminOrMemberAuthAttribute : ActionFilterAttribute, IAuthenticationFilter
    {
        public void OnAuthentication(AuthenticationContext filterContext)
        {
        }

        public void OnAuthenticationChallenge(AuthenticationChallengeContext filterContext)
        {
            var test = new SecurityInformation();
            string myname = test.CurrentUserName;
            string myrole = test.CurrentUserRole;



            var user = filterContext.HttpContext.User;

            if (user == null || myrole== null/*!user.Identity.IsAuthenticated*/) //null or not authenticated
            {
                // filterContext.Result = new HttpUnauthorizedResult();
                filterContext.Result = new RedirectResult(FormsAuthentication.LoginUrl);
            }
            //else user is authenticated : is admin or member
        }
    }
    public class AdminAttribute : ActionFilterAttribute, IAuthenticationFilter
    {
        
           

        public void OnAuthentication(AuthenticationContext filterContext)
        {
        }

        public void OnAuthenticationChallenge(AuthenticationChallengeContext filterContext)
        {
            var test = new SecurityInformation();
            string myname = test.CurrentUserName;
            string myrole = test.CurrentUserRole;


            var user = filterContext.HttpContext.User.Identity.Name;
            

            if (/*!user.Identity.IsAuthenticated*/ myrole == null || user == null)
            {
                filterContext.Result = new RedirectResult(FormsAuthentication.LoginUrl);
            }
            else if (/*user.Identity.IsAuthenticated && user.Identity.Name!= "Admin"*/ myrole != "Admin")
            {
                //for testing
                //bool trueorfalse = user.Identity.IsAuthenticated;
                //string name = user.Identity.Name;
                filterContext.Controller.TempData["message"] = "User is not a authenticated administator";
                filterContext.Result = new RedirectResult("~/Account/Login");
            }
            //else user is authenticated and is a admin
        }
    }
    public class MemberAttribute : ActionFilterAttribute, IAuthenticationFilter
    {
        public void OnAuthentication(AuthenticationContext filterContext)
        {
        }

        public void OnAuthenticationChallenge(AuthenticationChallengeContext filterContext)
        {
            var test = new SecurityInformation();
            string myname = test.CurrentUserName;
            string myrole = test.CurrentUserRole;


            var user = filterContext.HttpContext.User;
            
            if(/*!user.Identity.IsAuthenticated*/ myrole == null || user == null)
            {
                filterContext.Result = new RedirectResult(FormsAuthentication.LoginUrl);
            } else if (/*user.Identity.IsAuthenticated &&user.Identity.Name*/ myrole != "Member")
            {
                //for testing
                //bool trueorfalse = user.Identity.IsAuthenticated;
                //string name = user.Identity.Name;
                filterContext.Controller.TempData["message"] = "User is not a authenticated member";
                filterContext.Result = new RedirectResult("~/Account/Login");
            }
            //else user is authenticated and is a member
        }
    }
}