using System;
using System.Linq;
using System.Web.Mvc;
using System.Diagnostics;
using DreamSchedulerApplication.Models;

using Newtonsoft.Json;
using System.IO;
using Neo4jClient;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Text;

namespace DreamSchedulerApplication.Controllers
{
    [Authorize(Roles = "admin")]
    public class AdminController : Controller
    {
        private readonly IGraphClient client;

        public AdminController(IGraphClient graphClient)
        {
            client = graphClient;
        }

        // GET: Admin/Index
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Database()
        {

            try
            {
                var database = client.Cypher
                    .Match("(n:Database )")
                    .Where((Database n) => n.DatabaseName == "DreamScheduler")
                    .Return(n => n.As<Database>())
                    .Results.First();




                string[] dataFound = { "true", database.lastUpdate };
                //return RedirectToAction("Database", dataFound );
                try
                {
                    var database1 = client.Cypher
                    .Match("(n:Database )")
                    .Where((Database n) => n.DatabaseName == "professors")
                    .Return(n => n.As<Database>())
                    .Results.First();

                    ViewBag.FoundD = "true";
                    ViewBag.UpdateD = database.lastUpdate;
                    ViewBag.FoundP = "true";
                    ViewBag.UpdateP = database1.lastUpdate;
                    return View();

                }
                catch (InvalidOperationException)
                {
                    ViewBag.FoundD = "true";
                    ViewBag.UpdateD = database.lastUpdate;
                    ViewBag.FoundP = "false";
                    return View();
                }

            }
            catch (InvalidOperationException)
            {

                //error therefore can't find it 
                string[] dataFound = { "false", null };
                //return RedirectToAction("Database", dataFound);
                try
                {
                    var database1 = client.Cypher
                    .Match("(n:Database )")
                    .Where((Database n) => n.DatabaseName == "professors")
                    .Return(n => n.As<Database>())
                    .Results.First();

                    ViewBag.FoundD = "false";
                    ViewBag.FoundP = "true";
                    ViewBag.UpdateP = database1.lastUpdate;
                    return View();

                }
                catch (InvalidOperationException)
                {
                    ViewBag.FoundD = "false";
                    ViewBag.FoundP = "false";
                    return View();
                }

            }
        }

        public ActionResult DatabaseUpdate()
        {

            //When run first time
            //Create new Database nodes with timestamps
            //Only for creating new database, has to be modified to support updating

            //string time = DateTime.Now.ToString("yyyy-MM-ddThh:mm:sszzz");
            //var newDatabase = new Database { DatabaseName = "DreamScheduler", lastUpdate = time };

            //client.Cypher
             //   .Create("(n:Database {newDatabase})")
              //  .WithParam("newDatabase", newDatabase)
              //  .ExecuteWithoutResults();



            //Execute  scrapper.py to create JSON files with scraped data
            //scrapper.py file must be in the python27 folder

            var p = new Process();
            p.StartInfo.FileName = @"Python.exe";
            p.StartInfo.Arguments = "scrapper.py";
            p.StartInfo.WorkingDirectory = @"C:\Python27";
            p.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            p.StartInfo.UseShellExecute = false;
            p.Start();
            p.WaitForExit();


            //Import JSON objects containing course data from JSONdata.txt
            using (StreamReader reader = System.IO.File.OpenText(@"c:/Python27/JSONdata.txt"))
            {
                JObject courseFile = (JObject)JToken.ReadFrom(new JsonTextReader(reader));

                //Get all courses
                var courses = courseFile.Children();
                foreach (JProperty course in courses)
                {
                    //Create new course model object, load it with the JSON data and save it to database
                    
                    var newCourse = course.Value.ToObject<Course>();
                    newCourse.Code = course.Name;

                    if (newCourse.SemesterInSequence == -1) continue; //ignore courses that are not in the sequence

                    client.Cypher
                                .Create("(u:Course {newCourse})")
                                .WithParam("newCourse", newCourse)
                                .ExecuteWithoutResults();

                    //Load semester data
                    JToken semesters = course.Value["Course Dates"];

                    foreach (JProperty semester in semesters)
                    {

                        var newSemester = new Course.Semester { Name = semester.Name };

                        //Create semester node and relationship to the course
                        client.Cypher
                                       .Match("(n:Course )")
                                       .Where((Course n) => n.Code == newCourse.Code)
                                       .Create("n-[:has]->(u:Semester {newSemester})")
                                       .WithParam("newSemester", newSemester)
                                       .ExecuteWithoutResults();

                        //Load lectures, tutorials and labs
                        var lectures = semester.Value["Lecture"];
                        foreach (var lecture in lectures)
                        {
                            var newLecture = lecture.ToObject<Course.Lecture>();

                            newLecture.StartTime = (string) lecture["Dates"]["Start-Time"];
                            newLecture.EndTime = (string) lecture["Dates"]["End-Time"];
                            newLecture.Days = lecture["Dates"]["Days"].ToObject<string []>();
                            newLecture.Building = (string) lecture["Location"]["Building"];
                            newLecture.Room = (string) lecture["Location"]["Room"];

                            client.Cypher
                                            .Match("(n:Course)-[has]->(s:Semester)")
                                            .Where((Course n) => n.Code == newCourse.Code)
                                            .AndWhere((Course.Semester s) => s.Name == newSemester.Name)
                                            .Create("s-[:has]->(l:Lecture {newLecture})")
                                            .WithParam("newLecture", newLecture)
                                            .ExecuteWithoutResults();
                        }

                        var tutorials = semester.Value["Tutorial"];
                        if(tutorials != null)
                        {
                            foreach (var tutorial in tutorials)
                            {
                                var newTutorial = tutorial.ToObject<Course.Tutorial>();

                                newTutorial.StartTime = (string)tutorial["Dates"]["Start-Time"];
                                newTutorial.EndTime = (string)tutorial["Dates"]["End-Time"];
                                newTutorial.Days = tutorial["Dates"]["Days"].ToObject<string[]>();
                                newTutorial.Building = (string)tutorial["Location"]["Building"];
                                newTutorial.Room = (string)tutorial["Location"]["Room"];

                                client.Cypher
                                            .Match("(n:Course)-[has]->(s:Semester)")
                                            .Where((Course n) => n.Code == newCourse.Code)
                                            .AndWhere((Course.Semester s) => s.Name == newSemester.Name)
                                            .Create("s-[:has]->(l:Tutorial {newTutorial})")
                                            .WithParam("newTutorial", newTutorial)
                                            .ExecuteWithoutResults();

                            }
                        }

                        var labs = semester.Value["Lab"];
                        if (labs != null)
                        {
                            foreach (var lab in labs)
                            {
                                var newLab = lab.ToObject<Course.Tutorial>();

                                newLab.StartTime = (string)lab["Dates"]["Start-Time"];
                                newLab.EndTime = (string)lab["Dates"]["End-Time"];
                                newLab.Days = lab["Dates"]["Days"].ToObject<string[]>();
                                newLab.Building = (string)lab["Location"]["Building"];
                                newLab.Room = (string)lab["Location"]["Room"];

                                client.Cypher
                                            .Match("(n:Course)-[has]->(s:Semester)")
                                            .Where((Course n) => n.Code == newCourse.Code)
                                            .AndWhere((Course.Semester s) => s.Name == newSemester.Name)
                                            .Create("s-[:has]->(l:Lab {newLab})")
                                            .WithParam("newLab", newLab)
                                            .ExecuteWithoutResults();
                            }
                        }
                        
                    }
                }

                foreach (JProperty course in courses)
                {
                    var courseCode = course.Name;

                    var prerequisites = course.Value["Prerequisites"].ToObject<string[]>();

                    foreach (var prerequisite in prerequisites)
                    {
                        client.Cypher
                                           .Match("(c1:Course)", "(c2:Course)")
                                           .Where((Course c1) => c1.Code == prerequisite)
                                           .AndWhere((Course c2) => c2.Code == courseCode)
                                           .Create("(c1)-[:PrerequisiteFor]->(c2)")
                                           .ExecuteWithoutResults();
                    }

                }
            }

            return RedirectToAction("Index");
        }

        //working 
        public ActionResult JsonProfessors()
        {
            //When run first time
            //Create new Database nodes with timestamps
            //Only for creating new database, has to be modified to support updating

            string time = DateTime.Now.ToString("yyyy-MM-ddThh:mm:sszzz");
            var newDatabase = new Database { DatabaseName = "professors", lastUpdate = time };

            client.Cypher
                .Create("(n:Database {newDatabase})")
                .WithParam("newDatabase", newDatabase)
                .ExecuteWithoutResults();

            //Load professor data from JSON file
            StreamReader reader = System.IO.File.OpenText(@"C:/Python27/professors.json");
            JObject professorFile = (JObject)JToken.ReadFrom(new JsonTextReader(reader));

            var professors = professorFile.Children();

            foreach(var professor in professors)
            {
                var newProfessor = professor.First.ToObject<Professor>();
                //Create professor node in the database
                client.Cypher
                            .Create("(u:Professor {newProfessor})")
                            .WithParam("newProfessor", newProfessor)
                            .ExecuteWithoutResults();
            }

            return View();
        }

    }
}