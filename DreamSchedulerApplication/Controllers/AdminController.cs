using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Diagnostics;
using DreamSchedulerApplication.Models;

//test
using Newtonsoft.Json;
using Newtonsoft;
using System.IO;
using Neo4jClient;
using Newtonsoft.Json.Linq;
//test

namespace DreamSchedulerApplication.Controllers
{
    //only admin can access
    [Authorize(Roles = "admin")]
    public class AdminController : Controller
    {
        private readonly IGraphClient client;

        public AdminController(IGraphClient graphClient)
        {
            client = graphClient;
        }



        // GET: Admin
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

            //WHEN run first time 
            string time = DateTime.Now.ToString("yyyy-MM-ddThh:mm:sszzz");
            var newDatabase = new Database { DatabaseName = "DreamScheduler", lastUpdate = time };

            client.Cypher
                .Create("(n:Database {newDatabase})")
                .WithParam("newDatabase", newDatabase)
                .ExecuteWithoutResults();

            //will be used later for updates 
            //if(database !=null)
            //{
            //    string newTime = DateTime.Now.ToString("yyyy-MM-ddThh:mm:sszzz");
            //    client.Cypher
            //        .Match("(n:Database)")
            //        .Where((Database n) => n.DatabaseName == "DreamScheduler")
            //        .Set("n.lastUpdate = {newTime}")
            //        .WithParam("newTime", newTime)
            //        .ExecuteWithoutResults();
            //}

            /*    
             * ********************** WEB SCRAPPER **********************
             * 
             * execute  scrapper.py which output text file
             * scrapper.py must be the python27 folder
             * 
             * 
             * */

            var p = new Process();
            p.StartInfo.FileName = @"Python.exe";
            p.StartInfo.Arguments = "scrapper.py";
            p.StartInfo.WorkingDirectory = @"C:\Python27";
            p.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            p.StartInfo.UseShellExecute = false;
            //p.StartInfo.RedirectStandardOutput = true;
            p.Start();
            p.WaitForExit();


            /*
             * **************************** JSON TO MODEL => then stored in database     **************************************
             * 
             *  - system will open the json file 
             *  - it will match everything to each specific model : course, semester , lecture , tutorial 
             *  - after being matched, a node will be created in the database containing that information
             *              *
             * */

            //prereq, course date, restrtic, have their own childen 

            StreamReader reader = System.IO.File.OpenText(@"c:/Python27/JSONdata.txt");
            JObject o = (JObject)JToken.ReadFrom(new JsonTextReader(reader));

            //number of courses
            int nbElement = o.Count;



            for (int couseSelected = 0; couseSelected < nbElement; couseSelected++)
            {
                //MATCH TO MODEL 
                var StoreCourse = new CourseData.CourseInfo();


                //get all course
                var courseList = o.Children();

                var loopCourses = courseList.ElementAt(couseSelected); //CHOOSE course

                var courseName = ((Newtonsoft.Json.Linq.JProperty)loopCourses).Name; // name from course object

                var NotUsedCourseInfo = courseName.Substring(0, 4);

                if (NotUsedCourseInfo != "CWTC")
                {
                    var courseInfo = loopCourses.ElementAt(0).ToObject<CourseData.CourseInfo>();// get credits, get prerequisites, get restrictions
                    string[] prereq = courseInfo.Prerequisites;
                    string credit = courseInfo.Credits;
                    string description = (string)loopCourses.ElementAt(0).ElementAt(2).ElementAt(0); // course description 

                    string[] restric = courseInfo.Restrictions; // show course restriction


                    //matching to course model 
                    StoreCourse.courseName = courseName;
                    StoreCourse.Prerequisites = prereq;
                    StoreCourse.Credits = credit;
                    StoreCourse.CourseDescription = description;
                    StoreCourse.Restrictions = restric;


                    //create course node
                    client.Cypher
                           .Create("(u:Course {newCourse})")
                           .WithParam("newCourse", StoreCourse)
                           .ExecuteWithoutResults();




                    var sem = loopCourses.ElementAt(0).ElementAt(3);
                    int numberOfSem = ((Newtonsoft.Json.Linq.JContainer)(sem.ElementAt(0))).Count; //number of semester where this course is  available

                    //need to loop if course available in more than 1 semester
                    for (int x = 0; x < numberOfSem; x++)
                    {
                        var semester = ((Newtonsoft.Json.Linq.JToken)(sem.ElementAt(0))).ElementAt(x);
                        string semesterName = ((Newtonsoft.Json.Linq.JProperty)(semester)).Name;


                        var semesterNode = new CourseData.CourseInfo.Semester
                        {
                            SemesterName = semesterName
                        };


                        //create semester node -- create relation to the course 
                        client.Cypher
                                       .Match("(n:Course )")
                                       .Where((CourseData.CourseInfo n) => n.courseName == courseName)
                                       .Create("n-[:has]->(u:Semester {newSemester})")
                                       .WithParam("newSemester", semesterNode)
                                       .ExecuteWithoutResults();



                        //get lecture information
                        var lecture = ((Newtonsoft.Json.Linq.JContainer)(semester)).ElementAt(0).ElementAt(0);
                        int nbOfLecture = ((Newtonsoft.Json.Linq.JContainer)(((Newtonsoft.Json.Linq.JToken)(lecture)).First)).Count; //number of lecture


                        #region getLec
                        //for every lectures
                        for (int i = 0; i < nbOfLecture; i++)
                        {
                            var lec = lecture.ElementAt(0).ElementAt(i);
                            string ProfessorName = (string)((Newtonsoft.Json.Linq.JProperty)(lec.ElementAt(0))).Value;
                            string Section = (string)((Newtonsoft.Json.Linq.JProperty)(lec.ElementAt(1))).Value;

                            //DATES info
                            var Dates = lec.ElementAt(2);

                            string EndTime = (string)((Newtonsoft.Json.Linq.JProperty)(Dates.ElementAt(0).ElementAt(0))).Value;
                            string StartTime = (string)((Newtonsoft.Json.Linq.JProperty)(Dates.ElementAt(0).ElementAt(1))).Value;
                            string[] Days = ((Newtonsoft.Json.Linq.JArray)((Dates.ElementAt(0).ElementAt(2)).First)).ToObject<string[]>();


                            //location info

                            var location = lec.ElementAt(3);
                            string Building = (string)((Newtonsoft.Json.Linq.JProperty)(location.ElementAt(0).ElementAt(0))).Value;
                            string Room = (string)((Newtonsoft.Json.Linq.JProperty)(location.ElementAt(0).ElementAt(1))).Value;







                            // match to lecture model
                            var lectureData = new CourseData.CourseInfo.Semester.Lecture
                            {
                                LectureBuilding = Building,
                                LectureRoom = Room,
                                LectureDays = Days,
                                LectureStart = StartTime,
                                LectureEnd = EndTime,
                                Professor = ProfessorName,
                                Section = Section

                            };

                            //create lecture node -- create relation to the semester node 
                            client.Cypher
                                    .Match("(n:Course)-[has]->(s:Semester)")
                                    .Where((CourseData.CourseInfo n) => n.courseName == courseName)
                                    .AndWhere((CourseData.CourseInfo.Semester s) => s.SemesterName == semesterName)
                                    .Create("s-[:has]->(l:Lecture {newLecture})")
                                    .WithParam("newLecture", lectureData)
                                    .ExecuteWithoutResults();



                        }//end lecture
                        #endregion getLec
                        #region getLabTut
                        try
                        {
                            // get lab or tutorial 
                            var name1 = ((Newtonsoft.Json.Linq.JContainer)(semester)).ElementAt(0).ElementAt(1);
                            string itemName1 = ((Newtonsoft.Json.Linq.JProperty)(name1)).Name; //get name


                            if (itemName1 == "Lab")
                            {
                                var laboratory = name1;
                                //////////////////Lab //////////////////////////////////////////
                                int nbOfLab = ((Newtonsoft.Json.Linq.JContainer)(((Newtonsoft.Json.Linq.JToken)(laboratory)).First)).Count; //number of tutorial

                                //for every tutorial
                                for (int i = 0; i < nbOfLab; i++)
                                {
                                    var lab = laboratory.ElementAt(0).ElementAt(i);
                                    string Section = (string)((Newtonsoft.Json.Linq.JProperty)(lab.ElementAt(0))).Value;

                                    //DATES info
                                    var Dates = lab.ElementAt(1);

                                    string EndTime = (string)((Newtonsoft.Json.Linq.JProperty)(Dates.ElementAt(0).ElementAt(0))).Value;
                                    string StartTime = (string)((Newtonsoft.Json.Linq.JProperty)(Dates.ElementAt(0).ElementAt(1))).Value;
                                    string[] Days = ((Newtonsoft.Json.Linq.JArray)((Dates.ElementAt(0).ElementAt(2)).First)).ToObject<string[]>();


                                    //location info

                                    var location = lab.ElementAt(2);
                                    string Building = (string)((Newtonsoft.Json.Linq.JProperty)(location.ElementAt(0).ElementAt(0))).Value;
                                    string Room = (string)((Newtonsoft.Json.Linq.JProperty)(location.ElementAt(0).ElementAt(1))).Value;

                                    //match to tutorial model
                                    var LabInfo = new CourseData.CourseInfo.Semester.Lab
                                    {
                                        LabBuilding = Building,
                                        LabDays = Days,
                                        LabEnd = EndTime,
                                        LabStart = StartTime,
                                        LabRoom = Room,
                                        LabSection = Section

                                    };

                                    //create Tutorial node -- create relation to the semester 
                                    client.Cypher
                                            .Match("(n:Course)-[has]->(s:Semester)")
                                            .Where((CourseData.CourseInfo n) => n.courseName == courseName)
                                            .AndWhere((CourseData.CourseInfo.Semester s) => s.SemesterName == semesterName)
                                            .Create("s-[:has]->(l:Lab {newLab})")
                                            .WithParam("newLab", LabInfo)
                                            .ExecuteWithoutResults();


                                }//end lab
                                try
                                {
                                    //name2 exist , no out of bound 
                                    var name2 = ((Newtonsoft.Json.Linq.JContainer)(semester)).ElementAt(0).ElementAt(2);
                                    //////////////////TUTORIAL //////////////////////////////////////////
                                    var tutorial = name2;
                                    int nbOfTutorial = ((Newtonsoft.Json.Linq.JContainer)(((Newtonsoft.Json.Linq.JToken)(tutorial)).First)).Count; //number of tutorial

                                    //for every tutorial
                                    for (int i = 0; i < nbOfTutorial; i++)
                                    {
                                        var tut = tutorial.ElementAt(0).ElementAt(i);
                                        string Section = (string)((Newtonsoft.Json.Linq.JProperty)(tut.ElementAt(0))).Value;

                                        //DATES info
                                        var Dates = tut.ElementAt(1);

                                        string EndTime = (string)((Newtonsoft.Json.Linq.JProperty)(Dates.ElementAt(0).ElementAt(0))).Value;
                                        string StartTime = (string)((Newtonsoft.Json.Linq.JProperty)(Dates.ElementAt(0).ElementAt(1))).Value;
                                        string[] Days = ((Newtonsoft.Json.Linq.JArray)((Dates.ElementAt(0).ElementAt(2)).First)).ToObject<string[]>();


                                        //location info

                                        var location = tut.ElementAt(2);
                                        string Building = (string)((Newtonsoft.Json.Linq.JProperty)(location.ElementAt(0).ElementAt(0))).Value;
                                        string Room = (string)((Newtonsoft.Json.Linq.JProperty)(location.ElementAt(0).ElementAt(1))).Value;

                                        //match to tutorial model
                                        var tutorialInfo = new CourseData.CourseInfo.Semester.Tutorial
                                        {
                                            TutorialBuilding = Building,
                                            TutorialDays = Days,
                                            TutorialEnd = EndTime,
                                            TutorialStart = StartTime,
                                            TutorialRoom = Room,
                                            TutorialSection = Section

                                        };

                                        //create Tutorial node -- create relation to the semester 
                                        client.Cypher
                                                .Match("(n:Course)-[has]->(s:Semester)")
                                                .Where((CourseData.CourseInfo n) => n.courseName == courseName)
                                                .AndWhere((CourseData.CourseInfo.Semester s) => s.SemesterName == semesterName)
                                                .Create("s-[:has]->(l:Tutorial {newTutorial})")
                                                .WithParam("newTutorial", tutorialInfo)
                                                .ExecuteWithoutResults();


                                    }//end tutorial

                                }
                                catch (ArgumentOutOfRangeException)
                                {
                                    //name 2 does not exist 
                                }




                            }
                            else
                            {
                                //name1 is not lab, therefore it is turorial , there is no lab, only tutorials
                                //////////////////TUTORIAL //////////////////////////////////////////
                                var tutorial = name1;
                                int nbOfTutorial = ((Newtonsoft.Json.Linq.JContainer)(((Newtonsoft.Json.Linq.JToken)(tutorial)).First)).Count; //number of tutorial

                                //for every tutorial
                                for (int i = 0; i < nbOfTutorial; i++)
                                {
                                    var tut = tutorial.ElementAt(0).ElementAt(i);
                                    string Section = (string)((Newtonsoft.Json.Linq.JProperty)(tut.ElementAt(0))).Value;

                                    //DATES info
                                    var Dates = tut.ElementAt(1);

                                    string EndTime = (string)((Newtonsoft.Json.Linq.JProperty)(Dates.ElementAt(0).ElementAt(0))).Value;
                                    string StartTime = (string)((Newtonsoft.Json.Linq.JProperty)(Dates.ElementAt(0).ElementAt(1))).Value;
                                    string[] Days = ((Newtonsoft.Json.Linq.JArray)((Dates.ElementAt(0).ElementAt(2)).First)).ToObject<string[]>();


                                    //location info

                                    var location = tut.ElementAt(2);
                                    string Building = (string)((Newtonsoft.Json.Linq.JProperty)(location.ElementAt(0).ElementAt(0))).Value;
                                    string Room = (string)((Newtonsoft.Json.Linq.JProperty)(location.ElementAt(0).ElementAt(1))).Value;

                                    //match to tutorial model
                                    var tutorialInfo = new CourseData.CourseInfo.Semester.Tutorial
                                    {
                                        TutorialBuilding = Building,
                                        TutorialDays = Days,
                                        TutorialEnd = EndTime,
                                        TutorialStart = StartTime,
                                        TutorialRoom = Room,
                                        TutorialSection = Section

                                    };

                                    //create Tutorial node -- create relation to the semester 
                                    client.Cypher
                                            .Match("(n:Course)-[has]->(s:Semester)")
                                            .Where((CourseData.CourseInfo n) => n.courseName == courseName)
                                            .AndWhere((CourseData.CourseInfo.Semester s) => s.SemesterName == semesterName)
                                            .Create("s-[:has]->(l:Tutorial {newTutorial})")
                                            .WithParam("newTutorial", tutorialInfo)
                                            .ExecuteWithoutResults();


                                }//end tutorial
                            }
                        }
                        catch (ArgumentOutOfRangeException)
                        {
                            //no lab, no tutorial 
                        }
                        #endregion getLabTut
                    }//end semester loop
                }


            }//end of adding courses loop 





            return RedirectToAction("Index");

        }


        //working 
        public ActionResult JsonProfessors()
        {

            //WHEN run first time 
            string time = DateTime.Now.ToString("yyyy-MM-ddThh:mm:sszzz");
            var newDatabase = new Database { DatabaseName = "professors", lastUpdate = time };

            client.Cypher
                .Create("(n:Database {newDatabase})")
                .WithParam("newDatabase", newDatabase)
                .ExecuteWithoutResults();




            StreamReader reader = System.IO.File.OpenText(@"C:/Python27/professors.json");
            JObject o = (JObject)JToken.ReadFrom(new JsonTextReader(reader));


            int nbElement = o.Count; //number of professors

            var ProfList = o.Children(); //get all professors

            //find each prof and insert them in the database
            for (int i = 0; i < nbElement; i++)
            {

                var professor = ProfList.ElementAt(i).First.ToObject<ProfessorsData.Professors>();

                var newProfessor = new ProfessorsData.Professors
                {
                    name = professor.name,
                    description = professor.description,
                    email = professor.email,
                    image = professor.image,
                    office = professor.office,
                    phone = professor.phone,
                    website = professor.website
                };


                // create the professor in the database
                client.Cypher
                            .Create("(u:Professor {newProfessor})")
                            .WithParam("newProfessor", newProfessor)
                            .ExecuteWithoutResults();

            }
            return View();
        }

    }
}