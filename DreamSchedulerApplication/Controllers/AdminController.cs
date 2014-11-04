using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Diagnostics;
using DreamSchedulerApplication.Models;

//test
using Newtonsoft.Json;
using System.IO;
using Neo4jClient;
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
            var p = new Process();
            p.StartInfo.FileName = @"Python.exe";
            p.StartInfo.Arguments = "scrapper.py";
            p.StartInfo.WorkingDirectory = @"C:\Python27";
            p.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            p.StartInfo.UseShellExecute = false;
            //p.StartInfo.RedirectStandardOutput = true;
            p.Start();
            //p.WaitForExit();


            return RedirectToAction("Index");
			
        }

        public ActionResult Json()
        {
            //string file = System.IO.File.ReadAllText("C:/Python27/JSONdata.json");
            ////TextReader file2 = new TextReader();
            //StreamReader files = System.IO.File.OpenText(@"C:/Python27/JSONdata.txt");
            //JsonTextReader reader = new JsonTextReader(files);

            var files = System.IO.File.ReadAllText(@"c:/Python27/JSONdata.txt");
            if(!System.IO.File.Exists(files))
            {
                throw new System.InvalidOperationException("File cannot be read");
            }

            CourseData courseInfo = JsonConvert.DeserializeObject<CourseData>(files);
            
            
            for (int i = 0; i < 10; i++)
            {
                 // courseInfo.Courses.ElementAt(i)

                var newCourse = new CourseData.CourseInfo
                {
                    courseName = courseInfo.Courses.ElementAt(i).courseName,
                    coursePrerequisites = courseInfo.Courses.ElementAt(i).coursePrerequisites,
                    credit = courseInfo.Courses.ElementAt(i).credit,
                    Description = courseInfo.Courses.ElementAt(i).Description,
                    LectureBuilding = courseInfo.Courses.ElementAt(i).LectureBuilding,
                    LectureDays = courseInfo.Courses.ElementAt(i).LectureDays,
                    LectureEnd = courseInfo.Courses.ElementAt(i).LectureEnd,
                    LectureRoom = courseInfo.Courses.ElementAt(i).LectureRoom,
                    LectureSection = courseInfo.Courses.ElementAt(i).LectureSection,
                    LectureStart = courseInfo.Courses.ElementAt(i).LectureStart,
                    ProfessorName = courseInfo.Courses.ElementAt(i).ProfessorName,
                    restrictions = courseInfo.Courses.ElementAt(i).restrictions,
                    TutorialBuilding = courseInfo.Courses.ElementAt(i).TutorialBuilding,
                    TutorialDays = courseInfo.Courses.ElementAt(i).TutorialDays,
                    TutorialEnd = courseInfo.Courses.ElementAt(i).TutorialEnd,
                    TutorialRoom = courseInfo.Courses.ElementAt(i).TutorialRoom,
                    TutorialSection = courseInfo.Courses.ElementAt(i).TutorialSection,
                    TutorialStart = courseInfo.Courses.ElementAt(i).TutorialStart
                };
                
                GraphClient client = new GraphClient(new Uri("http://localhost:7474/db/data"));

                client.Connect();//connect to database
                client.Cypher
                            .Create("(u:Course {newCourse})")
                            .WithParam("newCourse", newCourse)
                            .ExecuteWithoutResults();

            }

            return RedirectToAction("index");
        }

    }
}