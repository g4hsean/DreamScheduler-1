using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DreamSchedulerApplication.Models;
using Neo4jClient;

namespace DreamSchedulerApplication.Controllers
{
    [Authorize]
    public class ManageController : Controller
    {
        private readonly IGraphClient client;
        public ManageController(IGraphClient graphClient)
        {
            client = graphClient;
        }

        // GET: Manage

        public ActionResult Index()
        {
            var academicRecord = new AcademicRecord();

            var student1 = client.Cypher
                .Match("(u:User)-[]->(s:Student)")
                .Where((User u) => u.Username == HttpContext.User.Identity.Name)
                .WithParam("username", HttpContext.User.Identity.Name)
                .Return((s) => s.As<Student>())
                .Results.First();


            academicRecord.Student = student1;

            return View(academicRecord);
        }

        public ActionResult AccountEdit()
        {
            var academicRecord = new AcademicRecord();


            var student1 = client.Cypher
                .Match("(u:User)-[]->(s:Student)")
                .Where((User u) => u.Username == HttpContext.User.Identity.Name)
                .WithParam("username", HttpContext.User.Identity.Name)
                .Return((s) => s.As<Student>())
                .Results.First();

            academicRecord.Student = student1;

            return View(academicRecord);
        }

        [HttpPost]
        public ActionResult AccountEdit(AcademicRecord model)
        {

            var teststudent = new Student { FirstName = model.Student.FirstName, LastName = model.Student.LastName, StudentID = model.Student.StudentID, GPA = model.Student.GPA };


            //NEED TO JOIN THESE TWO QUERY if possible

            //find the student node matched to that account
            var student1 = client.Cypher
               .Match("(u:User)-[]->(s:Student)")
               .Where((User u) => u.Username == HttpContext.User.Identity.Name)
               .WithParam("username", HttpContext.User.Identity.Name)
               .Return((s) => s.As<Student>())
               .Results.First();

            //update student node information
            client.Cypher
                .Match("(s:Student)")
                .Where((Student s) => s.StudentID == student1.StudentID)
                .Set("s = {student}")
                .WithParam("student", new Student { StudentID = teststudent.StudentID, FirstName = teststudent.FirstName, LastName = teststudent.LastName, GPA = teststudent.GPA })
                .ExecuteWithoutResults();

            return RedirectToAction("Account");

        }


    }
}