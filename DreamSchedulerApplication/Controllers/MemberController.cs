using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DreamSchedulerApplication.CustomAttributes;
using DreamSchedulerApplication.Models;
using Neo4jClient;

namespace DreamSchedulerApplication.Controllers
{
    //only member can access
    [Member]
    public class MemberController : Controller
    {
        private GraphClient client = new GraphClient(new Uri("http://localhost:7474/db/data"));
        

        // GET: Member
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult AcademicRecord()
        {
            var academicRecord = new AcademicRecord();
            client.Connect();//connect to database
            var student = client.Cypher
                         .Match("(s:Student)")
                         .Where((Student s) => s.StudentID == "123")
                         .Return((s) => s.As<Student>())
                         .Results.FirstOrDefault();

            academicRecord.Student = student;

            client.Connect();//connect to database
            academicRecord.CompletedCourses = client.Cypher
                         .Match("(s:Student)-[r:Completed]->(c:Course)")
                         .Where((Student s) => s.StudentID == "123")
                         .Return((c, r) => new AcademicRecord.CourseEntry
                         {
                             Course = c.As<Course>(),
                             Completed = r.As<Completed>()
                         })
                         .OrderBy("r.semester")
                         .Results;

            return View(academicRecord);
        }

        public ActionResult CourseSequence()
        {
            var courseSequence = new CourseSequence();
            client.Connect();//connect to database
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