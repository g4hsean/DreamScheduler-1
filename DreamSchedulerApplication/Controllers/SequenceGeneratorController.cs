using DreamSchedulerApplication.Models;
using Neo4jClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DreamSchedulerApplication.Controllers
{
    public class SequenceGeneratorController : Controller
    {
        // GET: SequenceGenerator
        private readonly IGraphClient client;
        public SequenceGeneratorController(IGraphClient graphClient)
        {
            client = graphClient;
        }

        //Get SequenceGenerator/Index
        public ActionResult Index()
        {
            return View(); //View for user input
        }

        public ActionResult GenerateSequence()
        {
            //Create new sequence
            var sequence = new Sequence();
            sequence.SequenceList = new List<Sequence.CourseEntry>();

            //Get Student Entry
            var entry = client.Cypher
                                    .Match("(u:User)-->(s:Student)")
                                    .Where((User u) => u.Username == HttpContext.User.Identity.Name)
                                    .Return((s) => s.As<Student>())
                                    .Results.First().Entry;

            //Get all courses
            List<Course> courseList = client.Cypher
                                  .Match("(c:Course)")
                                  .Return(c => c.As<Course>())
                                  .OrderBy("c.SemesterInSequence")
                                  .Results.ToList();

            //Strat scheduling by iterating over each semester
            for (int semester = 1; courseList.Count() != 0 && semester < 50; semester++) //should be changed after we fix scraping
            {
                //Get current semester season
                var semesterName = "";
                if (entry == "Fall")
                {
                    if (semester % 2 == 1) semesterName = "Fall";
                    else semesterName = "Winter";
                }
                else
                {
                    if (semester % 2 == 0) semesterName = "Fall";
                    else semesterName = "Winter";
                }

                //Find up to 5 courses for each semester
                for(int course = 1; course <=5; course++)
                {
                    foreach(var c in courseList)
                    {
                        //If prerequisite not satisfied, skip course
                        var prerequisites = client.Cypher
                                                .Match("(c1:Course)-[:PrerequisiteFor]->(c2:Course)")
                                                .Where((Course c2) => c2.Code == c.Code)
                                                .Return(c1 => c1.As<Course>())
                                                .Results;
                        bool notSatisfied = false;
                        foreach(var prerequisite in prerequisites)
                        {
                            //Found in non-scheduled course list
                            if (courseList.Find(x => x.Code == prerequisite.Code) != null)
                            {
                                notSatisfied = true;
                                break;
                            }
                            //Found in sequence with the same semester number
                            if (sequence.SequenceList.Find(x => x.Course.Code == prerequisite.Code && x.Semester == semester) != null)
                            {
                                notSatisfied = true;
                                break;
                            }
                        }
                        if (notSatisfied)
                        {
                            continue;
                        }

                        //If given in current semester remove from unscheduled courses and insert in sequence, break from the loop
                        var semesterGiven = client.Cypher
                                               .Match("(c1:Course)-->(s:Semester)")
                                               .Where((Course c1) => c1.Code == c.Code)
                                               .AndWhere((Course.Semester s) => s.Name == semesterName || s.Name == "Fall&Winter;")
                                               .Return(s => s.As<Course.Semester>())
                                               .Results;

                        if (semesterGiven.Count() != 0)
                        {
                            var newCourseEntry = new Sequence.CourseEntry()
                            {
                                Course = c,
                                Semester = semester,
                                SemesterName = semesterName + " " + (semester + 1)/2
                            };
                            sequence.SequenceList.Add(newCourseEntry);
                            courseList.Remove(c);
                            break;
                        }
                    }

                }
            }

            return View(sequence.SequenceList);
        }


        public ActionResult GenerateCustomSequence()
        {
            //Get Student Entry
            Student currentStudent = client.Cypher
                                    .Match("(u:User)-->(s:Student)")
                                    .Where((User u) => u.Username == HttpContext.User.Identity.Name)
                                    .Return((s) => s.As<Student>())
                                    .Results.First();

            var entry = currentStudent.Entry;

            //Get all courses
            List<Course> courseList = client.Cypher
                                  .Match("(s:Student), (c:Course)")
                                  .Where((Student s) => s.StudentID == currentStudent.StudentID)
                                  .AndWhere("NOT (s)-->(c)")
                                  .Return(c => c.As<Course>())
                                  .OrderBy("c.SemesterInSequence")
                                  .Results.ToList();

            for (int semester = 1; courseList.Count() != 0 && semester<50; semester++)
            {

                //Get semester
                var semesterName = "";
                if (entry == "Fall")
                {
                    if (semester % 2 == 1) semesterName = "Fall";
                    else semesterName = "Winter";
                }
                else
                {
                    if (semester % 2 == 0) semesterName = "Fall";
                    else semesterName = "Winter";
                }



                for (int course = 1; course <= 5; course++)
                {
                    foreach (var c in courseList)
                    {
                        //If prerequisite not satisfied, skip the course

                        //Not scheduled prerequisites
                        var prerequisitesNotSatisfied1 = client.Cypher
                                               .Match("(c1:Course)-[:PrerequisiteFor]->(c2:Course), (s:Student)")
                                               .Where((Student s) => s.StudentID == currentStudent.StudentID)
                                               .AndWhere((Course c2) => c2.Code == c.Code)
                                               .AndWhere("NOT (s)-->(c1)")
                                               .Return(c1 => c1.As<Course>())
                                               .Results;

                        //Prerequisites scheduled for the same semester
                        var prerequisitesNotSatisfied2 = client.Cypher
                                              .Match("(c1:Course)-[:PrerequisiteFor]->(c2:Course), (s:Student)-[r:Scheduled]->(c1)")
                                              .Where((Student s) => s.StudentID == currentStudent.StudentID)
                                              .AndWhere((Course c2) => c2.Code == c.Code)
                                              .AndWhere((Scheduled r) => r.SemesterNumber == semester)
                                              .Return(c1 => c1.As<Course>())
                                              .Results;

                        if (prerequisitesNotSatisfied1.Count() != 0 || prerequisitesNotSatisfied2.Count() != 0)
                        {
                            continue;
                        }


                        //If given in semester remove from unscheduled courses and create Scheduled relationship, then break from the loop
                        var semesterGiven = client.Cypher
                                               .Match("(c1:Course)-->(s:Semester)")
                                               .Where((Course c1) => c1.Code == c.Code)
                                               .AndWhere((Course.Semester s) => s.Name == semesterName || s.Name == "Fall&Winter;")
                                               .Return(s => s.As<Course.Semester>())
                                               .Results;
                       
                        if (semesterGiven.Count() != 0)
                        {
                            var scheduled = new Scheduled()
                            {
                                SemesterSeason = semesterName + " " + (semester+1)/2,
                                SemesterNumber = semester
                            };

                            client.Cypher
                                        .Match("(s:Student), (c1:Course)")
                                        .Where((Student s) => s.StudentID == currentStudent.StudentID)
                                        .AndWhere((Course c1) => c1.Code == c.Code)
                                        .Create("(s)-[r:Scheduled {scheduled}]->(c1)")
                                        .WithParam("scheduled", scheduled)
                                        .ExecuteWithoutResults();

                            courseList.Remove(c);

                            break;
                        }
                    }

                }
            }

            //Pass created schedule to the view
            var sequence = new Sequence();
            sequence.SequenceList = new List<Sequence.CourseEntry>();

            sequence.SequenceList = client.Cypher
                                    .Match("(s:Student)-[r:Scheduled]->(c:Course)")
                                    .Where((Student s) => s.StudentID == currentStudent.StudentID)
                                    .Return((r,c) => new Sequence.CourseEntry
                                    {
                                        Course = c.As<Course>(),
                                        Semester = r.As<Scheduled>().SemesterNumber,
                                        SemesterName = r.As<Scheduled>().SemesterSeason
                                    })
                                    .OrderBy("r.SemesterNumber")
                                    .Results.ToList();

            return View(sequence.SequenceList);
        }

        public ActionResult ResetCustomSequence()
        {
            client.Cypher
                        .Match("(u:User)-->(s:Student)-[r:Scheduled]->(c:Course)")
                        .Where((User u) => u.Username == HttpContext.User.Identity.Name)
                        .Delete("r")
                        .ExecuteWithoutResults();

            return RedirectToAction("Index", "Student");
        }

    }
}