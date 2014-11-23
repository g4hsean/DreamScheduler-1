using System.Collections.Generic;
using System.Web.Mvc;
using DreamSchedulerApplication.Models;
using Neo4jClient;

namespace DreamSchedulerApplication.Controllers
{
    [Authorize(Roles="student")]
    public class StudentController : Controller
    {
        private readonly IGraphClient client;

        public StudentController(IGraphClient graphClient)
        {
            client = graphClient;
        }

        //GET: Student/Index
        public ActionResult Index()
        {
            return View();
        }

        //GET: Student/CourseSequence
        public ActionResult CourseSequence()
        {
            IEnumerable<Course> courseSequence = client.Cypher
                         .Match("(c:Course)")
                         .Return(c => c.As<Course>())
                         .OrderBy("c.SemesterInSequence")
                         .Results;

            return View(courseSequence);
        }

        //GET: Student/Professors
        public ActionResult Professors()
        {
            IEnumerable<Professor> professorsList = client.Cypher
                                  .Match("(u:Professor)")
                                  .Return(u =>u.As<Professor>())
                                  .OrderBy("u.name")
                                  .Results;

            return View(professorsList);
        }

    }
}