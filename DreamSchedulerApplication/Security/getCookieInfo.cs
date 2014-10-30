using System;
using DreamSchedulerApplication;
using System.Web;

namespace DreamSchedulerApplication.getCookieInfo
{
    public class SecurityInformation
    {
        public string CurrentUserRole
        {
            get
            {
                string userRole = string.Empty;

                if (HttpContext.Current.Request.IsAuthenticated)
                {
                    userRole = HttpContext.Current.User.Identity.Name.Split('|')[0];
                }

                return userRole;
            }
        }

        public string CurrentUserName
        {
            get
            {
                string userName = string.Empty;

                if (HttpContext.Current.Request.IsAuthenticated)
                {
                    userName = HttpContext.Current.User.Identity.Name.Split('|')[1];
                }

                return userName;
            }
        }
    }
    
}
