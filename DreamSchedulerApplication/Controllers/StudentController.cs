using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DreamSchedulerApplication.Models;
using Neo4jClient;
using DreamSchedulerApplication.Security;

namespace DreamSchedulerApplication.Controllers
{
    //only student can access
    [Authorize(Roles="student")]
    public class StudentController : Controller
    {
         private readonly IGraphClient client;

        public StudentController(IGraphClient graphClient)
        {
            client = graphClient;
        }
        

        // GET: Member
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult CourseSequence()
        {
            var courseSequence = new CourseSequence();
            
            courseSequence.CourseList = client.Cypher
                         .Match("(p:Program)-[r]->(c:Course)")
                         .Return((c, r) => new CourseSequence.CourseEntry
                         {
                             Course = c.As<Course>(),
                             Semester = r.As<ContainsCourse>().SemesterInSequence
                         })
                         .OrderBy("r.SemesterInSequence")
                         .Results;

            return View(courseSequence);
        }


        public ActionResult Professors()
        {
            return View();
        }
        public ActionResult Courses()
        {
            return View();
        }

    }
}