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

            //var prof = new ProfessorView();

            //prof.pList = client.Cypher
            //                      .Match("(u:Professor)")
            //                      .Return((u) => new ProfessorView.Info
            //                      {
            //                          name = u.As<ProfessorsData.Professors>().name
            //                      })
            //                      .OrderBy("u.name")
            //                      .Results;
            //return View(prof);

            var prof = new ProfessorsData();

            prof.professorsList = client.Cypher
                                  .Match("(u:Professor)")
                                  .Return(u =>u.As<ProfessorsData.Professors>())
                                  .OrderBy("u.name")
                                  .Results;
            return View(prof);
        }


        //TESTING 
        public ActionResult ProfessorsInfo()
        {

            
            
            var prof1 = client.Cypher
                .Match("(u:Professor)")
                .Where((ProfessorsData.Professors u) => u.name == "Joey Paquet")
                .WithParam("name", "Joey Paquet")
                .Return((u) => u.As<ProfessorsData.Professors>())
                .Results.First();

            @ViewBag.Search = true;
            return RedirectToAction("Professors");
        }



        public ActionResult Courses()
        {
            return View();
        }

    }
}