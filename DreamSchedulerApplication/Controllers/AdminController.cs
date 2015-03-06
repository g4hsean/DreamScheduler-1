using System.Web.Mvc;
using DreamSchedulerApplication.Models;

using Neo4jClient;

namespace DreamSchedulerApplication.Controllers
{
    [Authorize(Roles = "admin")]
    public class AdminController : Controller
    {

        public AdminController(IGraphClient graphClient)
        {
            databaseManager = new DatabaseManager(graphClient);
        }

        private DatabaseManager databaseManager;

        //GET: Admin/Database
        public ActionResult Database()
        {
            var courseDatabase = databaseManager.getDatabase("CourseDatabase");

            if(courseDatabase != null)
            {
                ViewBag.FoundD = "true";
                ViewBag.UpdateD = courseDatabase.lastUpdate;
            }
            else ViewBag.FoundD = "false";

            var professorDatabase = databaseManager.getDatabase("ProfessorDatabase");

            if(professorDatabase != null)
            {
                ViewBag.FoundP = "true";
                ViewBag.UpdateP = professorDatabase.lastUpdate;
            }
            else ViewBag.FoundP = "false";

            return View();         
        }

        //GET: Admin/UpdateCourses
        public ActionResult UpdateCourses()
        {
            databaseManager.updateCourses();
            return RedirectToAction("Database");
        }

        //GET: Admin/UpdateProfessors
        public ActionResult UpdateProfessors()
        {
            databaseManager.updateProfessors();
            return RedirectToAction("Database");
        }

    }
}