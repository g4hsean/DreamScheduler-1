using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DreamSchedulerApplication.Models
{
    public class AcademicRecord
    {
        public Student Student;

        public IEnumerable<CourseEntry> CompletedCourses;

        public class CourseEntry
        {
            public Course Course { get; set; }
            public Completed Completed { get; set; }
        }
    }
}