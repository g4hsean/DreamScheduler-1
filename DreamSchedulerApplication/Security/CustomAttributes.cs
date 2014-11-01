using System.Web.Mvc;
using System.Web.Mvc.Filters;
using DreamSchedulerApplication;
using System.Web.Security;
using System;
using DreamSchedulerApplication.Security;


namespace DreamSchedulerApplication.CustomAttributes
{

    /*
     * Security Measures
     * 
     * Filter 
     * 
     * [adminOrMemberAuth] : only admin or member can access these controller or methods
     * 
     * [admin] : "..." only admin
     * 
     * [member] : "..." only member (the student user)
     * 
     * 
     * if the user does not respect the filter it will redirect it to the login page and give a specific error message if necessary 
     * 
     * */


    public class AdminOrMemberAuthAttribute : ActionFilterAttribute, IAuthenticationFilter
    {
        public void OnAuthentication(AuthenticationContext filterContext)
        {
        }

        public void OnAuthenticationChallenge(AuthenticationChallengeContext filterContext)
        {
            var test = new PrivateData();
            //
            string myrole = test.GetUserRole();



            var user = filterContext.HttpContext.User;

            if (user == null || myrole== null || !user.Identity.IsAuthenticated) 
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
            var test = new PrivateData();
           
            string myrole = test.GetUserRole();


            var user = filterContext.HttpContext.User;
            

            if (!user.Identity.IsAuthenticated || myrole == null || user == null)
            {
                filterContext.Result = new RedirectResult(FormsAuthentication.LoginUrl);
            }
            else if (user.Identity.IsAuthenticated && myrole != "Admin")
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
            var test = new PrivateData();
            string myrole = test.GetUserRole();
           


            var user = filterContext.HttpContext.User;
            
            if(!user.Identity.IsAuthenticated || myrole == null || user == null)
            {
                filterContext.Result = new RedirectResult(FormsAuthentication.LoginUrl);
            } else if ( myrole != "Member")
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