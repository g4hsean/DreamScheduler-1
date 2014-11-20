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

            var student = client.Cypher
                .Match("(u:User)-[]->(s:Student)")
                .Where((User u) => u.Username == HttpContext.User.Identity.Name)
                .WithParam("username", HttpContext.User.Identity.Name)
                .Return((s) => s.As<Student>())
                .Results.First();


            academicRecord.Student = student;

            return View(academicRecord);
        }

        public ActionResult AccountEdit()
        {
            var academicRecord = new AcademicRecord();


            var student = client.Cypher
                .Match("(u:User)-[]->(s:Student)")
                .Where((User u) => u.Username == HttpContext.User.Identity.Name)
                .WithParam("username", HttpContext.User.Identity.Name)
                .Return((s) => s.As<Student>())
                .Results.First();

            academicRecord.Student = student;

            return View(academicRecord);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AccountEdit(AcademicRecord model)
        {

            var newStudent = new Student { FirstName = model.Student.FirstName, LastName = model.Student.LastName, StudentID = model.Student.StudentID, GPA = model.Student.GPA };


            client.Cypher
               .Match("(u:User)-->(s:Student)")
               .Where((User u) => u.Username == HttpContext.User.Identity.Name)
               .AndWhere((Student s) => s.StudentID == model.Student.StudentID)
               .Set("s = {newStudent}")
               .WithParam("newStudent", newStudent)
               .ExecuteWithoutResults();

            return RedirectToAction("Index");

        }

    }
}