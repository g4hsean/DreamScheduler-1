using System.Collections.Generic;
using System.Web.Mvc;
using DreamSchedulerApplication.Models;
using Neo4jClient;
using System.Linq;

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

        //GET: Student/CourseDetails
        public ActionResult CourseDetails(string code)
        {
            var courseDetails = new CourseDetails();

            courseDetails.Course = client.Cypher
                                  .Match("(c:Course)")
                                  .Where((Course c) => c.Code == code)
                                  .Return(c => c.As<Course>())
                                  .Results.First();

            courseDetails.Lectures = client.Cypher
                                  .Match("(c:Course)-->(:Semester)-->(l:Lecture)")
                                  .Where((Course c) => c.Code == code)
                                  .Return(l => l.As<Course.Lecture>())
                                  .Results;

            courseDetails.Labs = client.Cypher
                                  .Match("(c:Course)-->(:Semester)-->(l:Lab)")
                                  .Where((Course c) => c.Code == code)
                                  .Return(l => l.As<Course.Lab>())
                                  .Results;

            courseDetails.Tutorials = client.Cypher
                                  .Match("(c:Course)-->(:Semester)-->(t:Tutorial)")
                                  .Where((Course c) => c.Code == code)
                                  .Return(t => t.As<Course.Tutorial>())
                                  .Results;
        
            return View(courseDetails);
        }

    }
}