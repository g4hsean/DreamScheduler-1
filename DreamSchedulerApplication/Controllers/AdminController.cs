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
            return View();
        }

        public ActionResult DatabaseUpdate()
        {


            //execute  scrapper.py which output text file
            //scrapper.py must be the python27 folder 
            //var p = new Process();
            //p.StartInfo.FileName = @"Python.exe";
            //p.StartInfo.Arguments = "scrapper.py";
            //p.StartInfo.WorkingDirectory = @"C:\Python27";
            //p.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            //p.StartInfo.UseShellExecute = false;
            ////p.StartInfo.RedirectStandardOutput = true;
            //p.Start();
            //p.WaitForExit();


            //////JSON/////////// NEED to fix this so o.childen give only (prereq, credit, course desscr, course dates, restrictions

            //prereq, course date, restrtic, have their own childen 

            StreamReader reader = System.IO.File.OpenText(@"c:/Python27/JSONdata.txt");
            JObject o = (JObject)JToken.ReadFrom(new JsonTextReader(reader));

            //number of courses
            int nbElement = o.Count;


            //MATCH TO MODEL 
            var StoreCourse = new CourseData.CourseInfo();

            /////////////////////////////////////////////
            
            
            


            //get all course
            var courseList = o.Children();

            var loopCourses = courseList.ElementAt(0); //CHOOSE course

            var courseName = ((Newtonsoft.Json.Linq.JProperty)loopCourses).Name; // name from course object
            var courseInfo = loopCourses.ElementAt(0).ToObject<CourseData.CourseInfo>();// get credits, get prerequisites, get restrictions
            string[] prereq = courseInfo.Prerequisites;
            string credit = courseInfo.Credits;
            string description = (string)courseList.ElementAt(0).ElementAt(0).ElementAt(2).First; // course description 

            string[] restric = courseInfo.Restrictions; // show course restriction


            ////////////////////STORE IN MODEL///////////////////////


            StoreCourse.courseName = courseName;
            StoreCourse.Prerequisites = prereq;
            StoreCourse.Credits = credit;
            StoreCourse.CourseDescription = description;
            StoreCourse.Restrictions = restric;
            StoreCourse.SemesterAvailable = new List<CourseData.CourseInfo.Semester>();
            

            /////////////////////////////////////////////////////




            var sem = loopCourses.ElementAt(0).ElementAt(3);
            //var semester1 = (Newtonsoft.Json.Linq.JProperty)(((Newtonsoft.Json.Linq.JContainer)(sem.First)).First);

            int numberOfSem = ((Newtonsoft.Json.Linq.JContainer)(sem.ElementAt(0))).Count; //number of semester where this course is  available

            //need to loop if course available in more than 1 semester
            for (int x = 0; x < numberOfSem; x++)
            {
                var semester = ((Newtonsoft.Json.Linq.JToken)(sem.ElementAt(0))).ElementAt(x);
                string semesterName = ((Newtonsoft.Json.Linq.JProperty)(semester)).Name;


                //get lecture information
                var lecture = (((Newtonsoft.Json.Linq.JContainer)(semester)).First).First;
                int nbOfLecture = ((Newtonsoft.Json.Linq.JContainer)(((Newtonsoft.Json.Linq.JToken)(lecture)).First)).Count; //number of lecture



                var LectureList = new CourseData.CourseInfo.Semester().lecture;
                LectureList = new List<CourseData.CourseInfo.Semester.Lecture>();

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


                    ///STORE LECTURE IN MODEL//////////
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

                    LectureList.Add(lectureData);
                    

                }


                // get tutorial information
                var tutorial = (((Newtonsoft.Json.Linq.JContainer)(semester)).First).Last;
                int nbOfTutorial = ((Newtonsoft.Json.Linq.JContainer)(((Newtonsoft.Json.Linq.JToken)(tutorial)).First)).Count; //number of tutorial

                var TutorialList = new CourseData.CourseInfo.Semester().tutorial;
                TutorialList = new List<CourseData.CourseInfo.Semester.Tutorial>();


                //for every lectures
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


                    var tutorialInfo = new CourseData.CourseInfo.Semester.Tutorial
                    {
                        TutorialBuilding = Building,
                        TutorialDays = Days,
                        TutorialEnd = EndTime,
                        TutorialStart = StartTime,
                        TutorialRoom = Room,
                        TutorialSection = Section

                    };
                    TutorialList.Add(tutorialInfo);
                    
                }
                //StoreCourse.semester.Add(TutorialList);

                var testing = new CourseData.CourseInfo.Semester
                {
                    SemesterName = semesterName,
                    lecture = LectureList,
                    tutorial = TutorialList
                };

                StoreCourse.SemesterAvailable.Add(testing);
            }

            ///FOR TESTING ONLY////////////////// 
            var bob = StoreCourse;
            //put breakpoint to see the inside of storecourse   
            //JSON TO MODEL is working, but model to neo4j not working because or nested list,
                

            // create the professor in the database
            client.Cypher
                        .Create("(u:CourseTest {bob})")
                        .WithParam("bob", bob)
                        .ExecuteWithoutResults();
            /////////////////////////////////////


            

            return RedirectToAction("Index");

        }


        //working 
        public ActionResult JsonProfessors()
        {
         
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