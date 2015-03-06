using System.Web.Mvc;

namespace DreamSchedulerApplication.Controllers
{
    public class HomeController : Controller
    {
        //GET:: Home/Index
        public ActionResult Index()
        {
            return View();
        }

    }
}