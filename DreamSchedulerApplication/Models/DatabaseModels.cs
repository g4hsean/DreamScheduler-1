using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DreamSchedulerApplication.Models
{
    public class User
    {
        public String Username { get; set; }
        public String Password { get; set; }

        public string Roles { get; set; }
    }

    public class Student
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string StudentID { get; set; }
        public string GPA { get; set; }
    }

    public class Course
    {
        public string Code { get; set; }
        public string Title { get; set; }
        public string Credits { get; set; }
    }

    public class Completed
    {
        public string Grade { get; set; }
        public int Semester { get; set; }
    }

    public class ContainsCourse
    {
        public int SemesterInSequence { get; set; }
    }
    //testing only
    public class CourseData
    {
        public IEnumerable<CourseInfo> Courses { get; set; }
        public class CourseInfo
        {
            public string courseName { get; set; }
            public string[] Prerequisites { get; set; }
            public string Credits { get; set; }
            public string CourseDescription { get; set; }

            public List<Semester> SemesterAvailable { get; set; }


            public class Semester
            {

                public string SemesterName { get; set; }

                public List<Lecture> lecture { get; set; }
                public class Lecture
                {
                    //lecture
                    public string Professor { get; set; }
                    public string Section { get; set; }
                    public string LectureStart { get; set; }
                    public string LectureEnd { get; set; }
                    public string[] LectureDays { get; set; }

                    public string LectureBuilding { get; set; }
                    public string LectureRoom { get; set; }
                }

                public List<Tutorial> tutorial { get; set; }


                public class Tutorial
                {
                    //tutorial
                    public string TutorialSection { get; set; }
                    public string TutorialStart { get; set; }
                    public string TutorialEnd { get; set; }
                    public string[] TutorialDays { get; set; }

                    public string TutorialBuilding { get; set; }
                    public string TutorialRoom { get; set; }
                }
            }
            //
            public string[] Restrictions { get; set; }
        }
    }

   
    //testing only
    public class ProfessorsData
    {
        public IEnumerable<Professors> professorsList { get; set; }
        public class Professors
        {
            public string name { get; set;}
            public string description { get; set; }
            public string office { get; set; }
            public string phone { get; set; }
            public string email { get; set; }
            public string website { get; set; }
            public string image { get; set; }
        }
    }

}