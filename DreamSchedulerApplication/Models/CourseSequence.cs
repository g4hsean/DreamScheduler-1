using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DreamSchedulerApplication.Models
{
    public class CourseSequence
    {
        public IEnumerable<CourseEntry> CourseList;

        public class CourseEntry
        {
            public CourseData.CourseInfo Course { get; set; }
            public int Semester { get; set; }
        }
    }





}