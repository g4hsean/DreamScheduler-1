using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace DreamSchedulerApplication.Models
{
    //Node classes
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
        public int StudentID { get; set; }
        public int GPA { get; set; }
    }

    public class Database
    {
        public string DatabaseName { get; set; }
        public string lastUpdate { get; set; }
    }

    public class Course
    {
        public string Code { get; set; }
        public string Credits { get; set; }
        public string Title { get; set; }
        public int SemesterInSequence { get; set; }

        public class Semester
        {
            public string Name { get; set; }
        }

        public class Activity
        {
            public string Section { get; set; }
            public string StartTime { get; set; }
            public string EndTime { get; set; }
            public string[] Days { get; set; }
            public string Building { get; set; }
            public string Room { get; set; }
        }

        public class Lecture : Activity
        {
            public string Professor { get; set; }
        }

        public class Lab : Activity { }
        public class Tutorial : Activity { }
    }

    public class Professor
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Office { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Website { get; set; }
        public string Image { get; set; }
    }

    //Relationship classes
    public class Completed
    {
        public string Grade { get; set; }
        public int Semester { get; set; }
    }

}