using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Neo4jClient;
using DreamSchedulerApplication.Models;
using DreamSchedulerApplication.Security;


namespace DreamSchedulerApplication.Controllers
{
    //everyone can access
    [AllowAnonymous]
    public class HomeController : Controller
    {
        private readonly IGraphClient client;

        public HomeController(IGraphClient graphClient)
        {
            client = graphClient;
        }

        public ActionResult Index()
        {
            return View();
        }


    }
}