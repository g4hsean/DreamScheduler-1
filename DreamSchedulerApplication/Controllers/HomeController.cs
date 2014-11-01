using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Neo4jClient;
using DreamSchedulerApplication.Models;
using DreamSchedulerApplication.CustomAttributes;
using DreamSchedulerApplication.Security;
	

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

            string username = new PrivateData().GetUserName();


            var student1 = client.Cypher
                .Match("(u:User)-[]->(s:Student)")
                .Where((User u) => u.Username == username)
                .WithParam("username", username)
                .Return((s) => s.As<Student>())
                .Results.First();


            academicRecord.Student = student1;

            return View(academicRecord);
        }
        [AdminOrMemberAuth]
        public ActionResult AccountEdit()
        {
            var academicRecord = new AcademicRecord();

            client.Connect();//connect to database

            string username = new PrivateData().GetUserName();

            var student1 = client.Cypher
                .Match("(u:User)-[]->(s:Student)")
                .Where((User u) => u.Username == username)
                .WithParam("username", username)
                .Return((s) => s.As<Student>())
                .Results.First();

            academicRecord.Student = student1;

            return View(academicRecord);
        }
        [AdminOrMemberAuth]
        [HttpPost]
        public ActionResult AccountEdit(AcademicRecord model)
        {

            var teststudent = new Student { FirstName = model.Student.FirstName, LastName = model.Student.LastName, StudentID = model.Student.StudentID, GPA = model.Student.GPA };
            
            
            client.Connect();//connect database

            string username = new PrivateData().GetUserName();

            //find the student node matched to that account
            var student1 = client.Cypher
               .Match("(u:User)-[]->(s:Student)")
               .Where((User u) => u.Username == username)
               .WithParam("username", username)
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