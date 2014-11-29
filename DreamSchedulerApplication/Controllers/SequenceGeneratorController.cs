using DreamSchedulerApplication.Models;
using Neo4jClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DreamSchedulerApplication.Controllers
{

    [Authorize(Roles = "student")]
    public class SequenceGeneratorController : Controller
    {

        private readonly StudentSequenceGenerator studentSequenceGenerator;
        private readonly DefaultSequenceGenerator defaultSequenceGenerator;

        private readonly IGraphClient client;

        public SequenceGeneratorController(IGraphClient graphClient)
        {
            studentSequenceGenerator = new StudentSequenceGenerator(graphClient);
            defaultSequenceGenerator = new DefaultSequenceGenerator(graphClient);

            client = graphClient;
        }

        public ActionResult GenerateDefaultSequence()
        {
            return View("DefaultCourseSequence", defaultSequenceGenerator.GenerateDefaultSequence());
        }

        public ActionResult ViewDefaultSequence()
        {
            return View("DefaultCourseSequence", defaultSequenceGenerator.ViewDefaultSequence());
        }


        public ActionResult GenerateStudentSequence(List<Constraint> constraints)
        {
            return View("StudentCourseSequence", studentSequenceGenerator.GenerateStudentSequence(constraints));
        }

        public ActionResult ResetCustomSequence()
        {
            studentSequenceGenerator.ResetStudentSequence();
            return RedirectToAction("Constraints", "SequenceGenerator");
        }

        public ActionResult Constraints()
        {
            var sequence = studentSequenceGenerator.getStudentSequence();
            if (sequence.Count() != 0) return View("StudentCourseSequence", sequence);

            var previousSemesters = client.Cypher
                         .Match("(u:User)-->(:Student)-[r:Completed]->(:Course)")
                         .Where((User u) => u.Username == HttpContext.User.Identity.Name)
                         .Return(r => r.As<Completed>().Semester)
                         .OrderByDescending("r.semester")
                         .Results;

            var nextSemester = 1;
            if (previousSemesters.Count() != 0) nextSemester = previousSemesters.First() + 1;

            var constraints = new List<Constraint>();
            constraints.Add(new Constraint { Semester = nextSemester, NumberOfCourses = 5 });

            return View(constraints);
        }

        [HttpPost]
        public ActionResult Constraints(List<Constraint> constraints)
        {
            if (!ModelState.IsValid) return View(constraints);
        
            return GenerateStudentSequence(constraints);
        }


    }
}