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

            string time = DateTime.Now.ToString("yyyy-MM-ddThh:mm:sszzz");
            var newDatabase = new Database { DatabaseName = "DreamScheduler", lastUpdate = time };

            client.Cypher
                .Create("(n:Database {newDatabase})")
                .WithParam("newDatabase", newDatabase)
                .ExecuteWithoutResults();

          
          
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
            StreamReader reader = System.IO.File.OpenText(@"c:/Python27/JSONdata.txt");
            JObject courseFile = (JObject)JToken.ReadFrom(new JsonTextReader(reader));

            //Get all courses
            var courses = courseFile.Children();
            foreach (var course in courses)
            {
                var prerequisites = ((Newtonsoft.Json.Linq.JArray)((course.ElementAt(0).ElementAt(2)).First)).ToObject<string[]>();
                //Create new course model object, load it with the JSON data and save it to database
                var newCourse = course.First.ToObject<Course>();
                newCourse.Code = ((Newtonsoft.Json.Linq.JProperty)course).Name;

                if (newCourse.SemesterInSequence == -1) continue; //ignore courses that are not in the sequence

                client.Cypher
                           .Create("(u:Course {newCourse})")
                           .WithParam("newCourse", newCourse)
                           .ExecuteWithoutResults();

                //Load semester data
                var semesters = course.ElementAt(0).ElementAt(4).ElementAt(0);
                foreach(var semester in semesters)
                {
                    
                   var semesterNode = new Course.Semester
                   {
                       Name = ((Newtonsoft.Json.Linq.JProperty)(semester)).Name
                   };

                   //Create semester node and relationship to the course
                   client.Cypher
                                  .Match("(n:Course )")
                                  .Where((Course n) => n.Code == newCourse.Code)
                                  .Create("n-[:has]->(u:Semester {newSemester})")
                                  .WithParam("newSemester", semesterNode)
                                  .ExecuteWithoutResults();

                   //Load lectures, tutorials and labs
                   var properties = ((Newtonsoft.Json.Linq.JContainer)(semester)).ElementAt(0);
                   
                   foreach(var property in properties)
                   {
                       var propertyName = ((Newtonsoft.Json.Linq.JProperty)(property)).Name;

                       if(propertyName == "Lecture")
                       {
                           var lectures = property.ElementAt(0);
                           
                           foreach (var lecture in lectures)
                           {
                               var dates = lecture.ElementAt(0);
                               var location = lecture.ElementAt(3);
                               var newLecture = new Course.Lecture
                               {
                                   Professor = (string)((Newtonsoft.Json.Linq.JProperty)(lecture.ElementAt(2))).Value,
                                   Section = (string)((Newtonsoft.Json.Linq.JProperty)(lecture.ElementAt(1))).Value,
                                   Days = ((Newtonsoft.Json.Linq.JArray)((dates.ElementAt(0).ElementAt(2)).First)).ToObject<string[]>(),
                                   StartTime = (string)((Newtonsoft.Json.Linq.JProperty)(dates.ElementAt(0).ElementAt(1))).Value,
                                   EndTime = (string)((Newtonsoft.Json.Linq.JProperty)(dates.ElementAt(0).ElementAt(0))).Value,                                 
                                   Building = (string)((Newtonsoft.Json.Linq.JProperty)(location.ElementAt(0).ElementAt(0))).Value,
                                   Room = (string)((Newtonsoft.Json.Linq.JProperty)(location.ElementAt(0).ElementAt(1))).Value
                               };
                             
                               //create lecture node -- create relationship to the semester node 
                               client.Cypher
                                       .Match("(n:Course)-[has]->(s:Semester)")
                                       .Where((Course n) => n.Code == newCourse.Code)
                                       .AndWhere((Course.Semester s) => s.Name == semesterNode.Name)
                                       .Create("s-[:has]->(l:Lecture {newLecture})")
                                       .WithParam("newLecture", newLecture)
                                       .ExecuteWithoutResults();
                           }
                       }

                       else if(propertyName == "Lab")
                       {
                            var labs = property.ElementAt(0);
                            foreach (var lab in labs)
                            {
                                var dates = lab.ElementAt(1);
                                var location = lab.ElementAt(2);
                                var newLab = new Course.Lab
                                {
                                    Section = (string)((Newtonsoft.Json.Linq.JProperty)(lab.ElementAt(0))).Value,
                                    Days = ((Newtonsoft.Json.Linq.JArray)((dates.ElementAt(0).ElementAt(2)).First)).ToObject<string[]>(),
                                    StartTime = (string)((Newtonsoft.Json.Linq.JProperty)(dates.ElementAt(0).ElementAt(1))).Value,
                                    EndTime = (string)((Newtonsoft.Json.Linq.JProperty)(dates.ElementAt(0).ElementAt(0))).Value,
                                    Building = (string)((Newtonsoft.Json.Linq.JProperty)(location.ElementAt(0).ElementAt(0))).Value,
                                    Room = (string)((Newtonsoft.Json.Linq.JProperty)(location.ElementAt(0).ElementAt(1))).Value
                                };                      

                                //create Tutorial node -- create relation to the semester 
                                client.Cypher
                                        .Match("(n:Course)-[has]->(s:Semester)")
                                        .Where((Course n) => n.Code == newCourse.Code)
                                        .AndWhere((Course.Semester s) => s.Name == semesterNode.Name)
                                        .Create("s-[:has]->(l:Lab {newLab})")
                                        .WithParam("newLab", newLab)
                                        .ExecuteWithoutResults();
                            }
                       }

                       else if(propertyName == "Tutorial")
                       {
                           var tutorials = property.ElementAt(0);
                           foreach (var tutorial in tutorials)
                           {
                               var dates = tutorial.ElementAt(1);
                               var location = tutorial.ElementAt(2);

                               var newTutorial = new Course.Tutorial
                               {
                                   Section = (string)((Newtonsoft.Json.Linq.JProperty)(tutorial.ElementAt(0))).Value,
                                   Days = ((Newtonsoft.Json.Linq.JArray)((dates.ElementAt(0).ElementAt(2)).First)).ToObject<string[]>(),
                                   StartTime = (string)((Newtonsoft.Json.Linq.JProperty)(dates.ElementAt(0).ElementAt(1))).Value,
                                   EndTime = (string)((Newtonsoft.Json.Linq.JProperty)(dates.ElementAt(0).ElementAt(0))).Value,
                                   Building = (string)((Newtonsoft.Json.Linq.JProperty)(location.ElementAt(0).ElementAt(0))).Value,
                                   Room = (string)((Newtonsoft.Json.Linq.JProperty)(location.ElementAt(0).ElementAt(1))).Value
                               };

                               //create Tutorial node -- create relation to the semester 
                               client.Cypher
                                       .Match("(n:Course)-[has]->(s:Semester)")
                                       .Where((Course n) => n.Code == newCourse.Code)
                                       .AndWhere((Course.Semester s) => s.Name == semesterNode.Name)
                                       .Create("s-[:has]->(l:Tutorial {newTutorial})")
                                       .WithParam("newTutorial", newTutorial)
                                       .ExecuteWithoutResults();
                           }
                       }
                   }   
                }
            }

            foreach (var course in courses)
            {
                var courseCode = ((Newtonsoft.Json.Linq.JProperty)course).Name;
                //var prerequisites = (string)((Newtonsoft.Json.Linq.JProperty)(course.ElementAt(0).ElementAt(0))).Value;

                var prerequisites = ((Newtonsoft.Json.Linq.JArray)((course.ElementAt(0).ElementAt(2)).First)).ToObject<string[]>();
                
                foreach(var prerequisite in prerequisites)
                {
                    client.Cypher
                                       .Match("(c1:Course)", "(c2:Course)")
                                       .Where((Course c1) => c1.Code == prerequisite)
                                       .AndWhere((Course c2) => c2.Code == courseCode)
                                       .Create("(c1)-[:PrerequisiteFor]->(c2)")
                                       .ExecuteWithoutResults();
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