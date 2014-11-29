using Neo4jClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;

namespace DreamSchedulerApplication.Models
{
    public class DatabaseManager
    {

        protected readonly IGraphClient client;

        public DatabaseManager(IGraphClient client)
        {
            this.client = client;
        }

        public Database getDatabase(string databaseName)
        {
            var database = client.Cypher
                                        .Match("(n:Database)")
                                        .Where((Database n) => n.DatabaseName == databaseName)                                       
                                        .Return(n => n.As<Database>())
                                        .Results;

            if (database.Count() != 0) return database.First();
            else return null;
        }

        public void updateCourses()
        {
            //When run first time
            //Create new Database nodes with timestamps
            //If database already exists, set new lastUpdate time

            string time = DateTime.Now.ToString("yyyy-MM-ddThh:mm:sszzz");
            var newDatabase = new Database { DatabaseName = "CourseDatabase", lastUpdate = time };

            client.Cypher
                        .Merge("(d:Database {DatabaseName: {newDatabase}.DatabaseName})")
                        .OnCreate()
                        .Set("d = {newDatabase}")
                        .OnMatch()
                        .Set("d.lastUpdate = {newDatabase}.lastUpdate")
                        .WithParam("newDatabase", newDatabase)
                        .ExecuteWithoutResults();

            //Remove all courses
            client.Cypher
                        .Match("(c:Course), (c1:Course)-[r]-()")
                        .Delete("r, c")
                        .ExecuteWithoutResults();

            //Execute  scrapper.py to create JSON files with scraped data
            //scrapper.py file must be in the python27 folderS
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

                            newLecture.StartTime = (string)lecture["Dates"]["Start-Time"];
                            newLecture.EndTime = (string)lecture["Dates"]["End-Time"];
                            newLecture.Days = lecture["Dates"]["Days"].ToObject<string[]>();
                            newLecture.Building = (string)lecture["Location"]["Building"];
                            newLecture.Room = (string)lecture["Location"]["Room"];

                            client.Cypher
                                            .Match("(n:Course)-[has]->(s:Semester)")
                                            .Where((Course n) => n.Code == newCourse.Code)
                                            .AndWhere((Course.Semester s) => s.Name == newSemester.Name)
                                            .Create("s-[:has]->(l:Lecture {newLecture})")
                                            .WithParam("newLecture", newLecture)
                                            .ExecuteWithoutResults();
                        }

                        var tutorials = semester.Value["Tutorial"];
                        if (tutorials != null)
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
        }

        public void updateProfessors()
        {
            //When run first time
            //Create new Database nodes with timestamps
            //If database already exists, set new lastUpdate time 

            string time = DateTime.Now.ToString("yyyy-MM-ddThh:mm:sszzz");
            var newDatabase = new Database { DatabaseName = "ProfessorDatabase", lastUpdate = time };

            client.Cypher
                        .Merge("(d:Database {DatabaseName: {newDatabase}.DatabaseName})")
                        .OnCreate()
                        .Set("d = {newDatabase}")
                        .OnMatch()
                        .Set("d.lastUpdate = {newDatabase}.lastUpdate")
                        .WithParam("newDatabase", newDatabase)
                        .ExecuteWithoutResults();

            //Load professor data from JSON file
            StreamReader reader = System.IO.File.OpenText(@"C:/Python27/professors.json");
            JObject professorFile = (JObject)JToken.ReadFrom(new JsonTextReader(reader));

            var professors = professorFile.Children();

            foreach (var professor in professors)
            {
                var newProfessor = professor.First.ToObject<Professor>();
                //Create professor node in the database
                client.Cypher
                            .Merge("(p:Professor {Name: {newProfessor}.Name})")
                            .OnCreate()
                            .Set("p = {newProfessor}")
                            .WithParam("newProfessor", newProfessor)
                            .ExecuteWithoutResults();
            }
        }

    }
}