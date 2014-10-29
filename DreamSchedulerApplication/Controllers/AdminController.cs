using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DreamSchedulerApplication.CustomAttributes;

namespace DreamSchedulerApplication.Controllers
{
    //only admin can access
    [Admin]
    public class AdminController : Controller
    {
        // GET: Admin
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Database()
        {
            return View();
        }
    }
}