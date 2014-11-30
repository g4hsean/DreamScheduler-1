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
        public ActionResult CourseDetails(string code, string semesterName)
        {
            var courseDetails = new CourseDetails();

            if (semesterName.Contains("Fall")) semesterName = "Fall";
            else semesterName = "Winter";

            courseDetails.Course = client.Cypher
                                  .Match("(c:Course)")
                                  .Where((Course c) => c.Code == code)
                                  .Return(c => c.As<Course>())
                                  .Results.First();

            var lectures = client.Cypher
                                  .Match("(c:Course)-->(s:Semester)-->(l:Lecture)")
                                  .Where((Course c) => c.Code == code)
                                  .AndWhere((Course.Semester s) => s.Name == semesterName)
                                  .Return(l => l.As<Course.Lecture>())
                                  .Results;

            foreach (var lecture in lectures)
            {
                

                var labs = client.Cypher
                                      .Match("(c:Course)-->(s:Semester)-->(l:Lab)")
                                      .Where((Course c) => c.Code == code)
                                      .AndWhere((Course.Lab l) => l.ParentSection == lecture.Section)
                                      .Return(l => l.As<Course.Lab>())
                                      .Results;

                var tutorials = client.Cypher
                                      .Match("(c:Course)-->(s:Semester)-->(t:Tutorial)")
                                      .Where((Course c) => c.Code == code)
                                      .AndWhere((Course.Tutorial t) => t.ParentSection == lecture.Section)
                                      .Return(t => t.As<Course.Tutorial>())
                                      .Results;

                var section = new CourseDetails.Section() { Lecture = lecture, Labs = labs, Tutorials = tutorials };
                courseDetails.sections.Add(section);              
            }
        
            return View(courseDetails);
        }

    }
}