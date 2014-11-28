using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DreamSchedulerApplication.Models
{
    public class Sequence
    {
        public List<CourseEntry> CourseList { get; set; }

        public class CourseEntry
        {
            public Course Course { get; set; }
            public int Semester { get; set; }
            public string SemesterName { get; set; }
        }
    }
}