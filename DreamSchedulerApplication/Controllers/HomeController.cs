using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Neo4jClient;
using DreamSchedulerApplication.Models;
using DreamSchedulerApplication.CustomAttributes;

namespace DreamSchedulerApplication.Controllers
{
    //everyone can access
    [AllowAnonymous]
    public class HomeController : Controller
    {
        private GraphClient client = new GraphClient(new Uri("http://localhost:7474/db/data"));

        public ActionResult Index()
        {
            return View();
        }
        
        //only for users 
        [AdminOrMemberAuth]
        public ActionResult Account()
        {
            var academicRecord = new AcademicRecord();

            client.Connect();//connect to database
            var student = client.Cypher
                         .Match("(s:Student)")
                         .Where((Student s) => s.StudentID == "1941097")//testing
                         .Return((s) => s.As<Student>())
                         .Results.FirstOrDefault();

            academicRecord.Student = student;

            return View(academicRecord);
        }
        public ActionResult AccountEdit()
        {
            var academicRecord = new AcademicRecord();

            client.Connect();//connect to database
            var student = client.Cypher
                         .Match("(s:Student)")
                         .Where((Student s) => s.StudentID == "1941097")//testing
                         .Return((s) => s.As<Student>())
                         .Results.FirstOrDefault();
            academicRecord.Student = student;

            return View(academicRecord);
        }
        [HttpPost]
        public ActionResult AccountEdit(AcademicRecord model)
        {
            //BUG HERE
            var teststudent = new Student { FirstName = model.Student.FirstName, LastName = model.Student.LastName, StudentID = model.Student.StudentID, GPA = model.Student.GPA };
            
            
            client.Connect();//connect database
            client.Cypher
                .Match("(s:Student)")
                .Where((Student s) => s.StudentID == "1941097") //it was supposed to be model.Student.StudentID  but it couldn't find because all my value are null from POST... weird  When i used the Id, it replace all the value in database with null(empty)... so i know that query work lol but something is wrong with my HTML POST submit
                .Set("s = {student}")
                .WithParam("student", new Student { StudentID = teststudent.StudentID, FirstName = teststudent.FirstName, LastName = teststudent.LastName, GPA = teststudent.GPA })
                .ExecuteWithoutResults();

            return RedirectToAction("Account");
             
        }
        
    }
}