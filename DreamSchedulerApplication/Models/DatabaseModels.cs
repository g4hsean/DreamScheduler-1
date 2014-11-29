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
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Display(Name = "Student ID")]
        [RegularExpression(@"^\d{7}(?:\d{1})?$", ErrorMessage = "Invalid Student ID")]
        public int StudentID { get; set; }

        public string Entry  { get; set; }
        public int GPA { get; set; }
    }

    public class Admin
    {
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        public string LastName { get; set; }
    }


    public class Database
    {
        public string DatabaseName { get; set; }
        public string lastUpdate { get; set; }
    }

    public class Course
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Course Code is required")] 
        [RegularExpression(@"\b[A-Z]{4}\b\s\b[0-9]{3}\b", ErrorMessage = "Invalid Course Code")]
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
        [Required(ErrorMessage = "Your grade is required")]
        [RegularExpression(@"^[A-F][+-]?$", ErrorMessage = "Invalid Grade")]       
        public string Grade { get; set; }

        [Required(ErrorMessage = "Semester is required")]
        [Range(0, 20, ErrorMessage = "Invalid semester number")]
        public int Semester { get; set; }
    }

    public class Scheduled
    {
        public string SemesterSeason { get; set; }
        public int SemesterNumber { get; set; }
    }

}