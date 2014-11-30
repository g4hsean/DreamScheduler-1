using Neo4jClient;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using DreamSchedulerApplication.Models;

namespace DreamSchedulerApplication.Models
{
    public abstract class SequenceGenerators 
    {
        protected readonly IGraphClient client;

        protected SequenceGenerators(IGraphClient client)
        {
            this.client = client;
        }

        protected Student getCurrentStudent()
        {
            var currentUserName = HttpContext.Current.User.Identity.Name;

            Student currentStudent = client.Cypher
                                    .Match("(u:User)-->(s:Student)")
                                    .Where((User u) => u.Username == currentUserName)
                                    .Return((s) => s.As<Student>())
                                    .Results.First();

            return currentStudent;
        }

        protected abstract List<Course> getUnscheduledCourses(Student currentStudent);

        protected string getSemesterName(Student currentStudent, int semester)
        {
            if (currentStudent.Entry == "Fall")
            {
                if (semester % 2 == 1) return "Fall";
                else return "Winter";
            }
            else
            {
                if (semester % 2 == 0) return "Fall";
                else return "Winter";
            }
        }

        protected bool givenInSemester(Course course, string semesterName)
        {
            var semesterGiven = client.Cypher
                                               .Match("(c1:Course)-->(s:Semester)")
                                               .Where((Course c1) => c1.Code == course.Code)
                                               .AndWhere((Course.Semester s) => s.Name == semesterName || s.Name == "Fall&Winter;")
                                               .Return(s => s.As<Course.Semester>())
                                               .Results;

            return (semesterGiven.Count() != 0);
        }

    }

    public class StudentSequenceGenerator : SequenceGenerators
    {
        public StudentSequenceGenerator(IGraphClient client) : base(client) { }

        public List<Sequence.CourseEntry> GenerateStudentSequence(List<Constraint> constraints)
        {
            var currentStudent = getCurrentStudent();
            //Get courses that student has not yet taken
            List<Course> unscheduledCourses = getUnscheduledCourses(currentStudent);

            foreach (var constraint in constraints)
            {
                //Break if all courses have been scheduled
                if (unscheduledCourses.Count() == 0) break;
                
                //Get semester
                int semester = constraint.Semester;
                var semesterName = getSemesterName(currentStudent, semester);

                //Schedule specified number of courses for each semester
                for (int coursesToSchedule = constraint.NumberOfCourses; coursesToSchedule > 0; coursesToSchedule--)
                {
                    foreach (var course in unscheduledCourses)
                    {
                        //If not given in current semester, skip the course
                        if(!givenInSemester(course,semesterName)) continue;

                        //If prerequisites not satisfied, skip the course                                    
                        if(!prerequisitesSatisfied(currentStudent, course, semester)) continue;
    
                        //Schedule course
                        scheduleCourse(currentStudent, course, semester, semesterName);

                        //Remove scheduled course from unscheduled courses
                        unscheduledCourses.Remove(course);
                       
                        //Schedule next course
                        break;
                    }
                }
            }
          
            //Return generated schedule
            return getSequence(currentStudent);
        }

        public void ResetStudentSequence()
        {

            var currentUserName = HttpContext.Current.User.Identity.Name;

            client.Cypher
                        .Match("(u:User)-->(s:Student)-[r:Scheduled]->(c:Course)")
                        .Where((User u) => u.Username == currentUserName)
                        .Delete("r")
                        .ExecuteWithoutResults();
        }

        public List<Sequence.CourseEntry> getStudentSequence()
        {
            return getSequence(getCurrentStudent());
        }
     
        protected override List<Course> getUnscheduledCourses(Student currentStudent)
        {
            return client.Cypher
                                .Match("(s:Student), (c:Course)")
                                .Where((Student s) => s.StudentID == currentStudent.StudentID)
                                .AndWhere("NOT (s)-->(c)")
                                .Return(c => c.As<Course>())
                                .OrderBy("c.SemesterInSequence")
                                .Results.ToList();
        }    

        private bool prerequisitesSatisfied(Student currentStudent, Course course, int semester)
        {
            //Not scheduled and not taken prerequisites
            var prerequisitesNotSatisfied1 = client.Cypher
                                               .Match("(c1:Course)-[:PrerequisiteFor|CorequisiteFor]->(c2:Course), (s:Student)")
                                               .Where((Student s) => s.StudentID == currentStudent.StudentID)
                                               .AndWhere((Course c2) => c2.Code == course.Code)
                                               .AndWhere("NOT (s)-->(c1)")
                                               .Return(c1 => c1.As<Course>())
                                               .Results;

           //Prerequisites scheduled for the same semester
           var prerequisitesNotSatisfied2 = client.Cypher
                                              .Match("(c1:Course)-[:PrerequisiteFor]->(c2:Course), (s:Student)-[r:Scheduled]->(c1)")
                                              .Where((Student s) => s.StudentID == currentStudent.StudentID)
                                              .AndWhere((Course c2) => c2.Code == course.Code)
                                              .AndWhere((Scheduled r) => r.SemesterNumber == semester)
                                              .Return(c1 => c1.As<Course>())
                                              .Results;

           var prerequisitesSatisfied = !(prerequisitesNotSatisfied1.Count() != 0 || prerequisitesNotSatisfied2.Count() != 0);

           return prerequisitesSatisfied;
        }

        private void scheduleCourse(Student currentStudent, Course course, int semester, string semesterName)
        {
            var scheduled = new Scheduled()
            {
                SemesterSeason = semesterName + " " + (semester + 1) / 2,
                SemesterNumber = semester
            };

            client.Cypher
                         .Match("(s:Student), (c1:Course)")
                         .Where((Student s) => s.StudentID == currentStudent.StudentID)
                         .AndWhere((Course c1) => c1.Code == course.Code)
                         .Create("(s)-[r:Scheduled {scheduled}]->(c1)")
                         .WithParam("scheduled", scheduled)
                         .ExecuteWithoutResults();
        }

        private List<Sequence.CourseEntry> getSequence(Student currentStudent)
        {
            var sequence = new Sequence();
            sequence.CourseList = new List<Sequence.CourseEntry>();

            sequence.CourseList = client.Cypher
                                    .Match("(s:Student)-[r:Scheduled]->(c:Course)")
                                    .Where((Student s) => s.StudentID == currentStudent.StudentID)
                                    .Return((r, c) => new Sequence.CourseEntry
                                    {
                                        Course = c.As<Course>(),
                                        Semester = r.As<Scheduled>().SemesterNumber,
                                        SemesterName = r.As<Scheduled>().SemesterSeason
                                    })
                                    .OrderBy("r.SemesterNumber")
                                    .Results.ToList();

            return sequence.CourseList;
        }
    }

    public class DefaultSequenceGenerator : SequenceGenerators
    {
        public DefaultSequenceGenerator(IGraphClient client) : base(client) { }

        public List<Sequence.CourseEntry> GenerateDefaultSequence()
        {
            //Create new sequence
            var sequence = new Sequence();
            sequence.CourseList = new List<Sequence.CourseEntry>();

            var currentStudent = getCurrentStudent();

            //Get all courses
            List<Course> unscheduledCourses = getUnscheduledCourses(currentStudent);

            //Schedule courses for each semester
            for (int semester = 1; unscheduledCourses.Count() != 0; semester++)
            {
                var semesterName = getSemesterName(currentStudent, semester);

                //Schedule up to 5 courses for each semester
                for (int coursesToSchedule = 5; coursesToSchedule > 0; coursesToSchedule--)
                {
                    foreach (var course in unscheduledCourses)
                    {
                        //If not given in current semester, skip the course
                        if (!givenInSemester(course,semesterName)) continue;

                        //If prerequisites not satisfied, skip the course
                        if (!prerequisitesSatisfied(currentStudent, course, semester, unscheduledCourses, sequence)) continue;

                        //Schedule course
                        scheduleCourse(course, semester, semesterName, sequence);

                        //Remove scheduled course from unscheduled courses
                        unscheduledCourses.Remove(course);

                        //Schedule next course
                        break;               
                    }
                }
            }

            return sequence.CourseList;
        }

        protected override List<Course> getUnscheduledCourses(Student currentStudent)
        {
            return client.Cypher
                                .Match("(c:Course)")
                                .Return(c => c.As<Course>())
                                .OrderBy("c.SemesterInSequence")
                                .Results.ToList();
        }

        private bool prerequisitesSatisfied(Student currentStudent, Course course, int semester, List<Course> unscheduledCourses, Sequence sequence)
        {
            var prerequisites = client.Cypher
                                    .Match("(c1:Course)-[:PrerequisiteFor]->(c2:Course)")
                                    .Where((Course c2) => c2.Code == course.Code)
                                    .Return(c1 => c1.As<Course>())
                                    .Results;

            var corequisites = client.Cypher
                                    .Match("(c1:Course)-[:CorequisiteFor]->(c2:Course)")
                                    .Where((Course c2) => c2.Code == course.Code)
                                    .Return(c1 => c1.As<Course>())
                                    .Results;

            var prerequisitesSatisfied = true;
            foreach (var prerequisite in prerequisites)
            {
                //Prerequisite found in non-scheduled course list
                if (unscheduledCourses.Find(x => x.Code == prerequisite.Code) != null)
                {
                    prerequisitesSatisfied = false;
                    break;
                }
                //Prerequisite found in sequence with the same semester number
                if (sequence.CourseList.Find(x => x.Course.Code == prerequisite.Code && x.Semester == semester) != null)
                {
                    prerequisitesSatisfied = false;
                    break;
                }
            }
            foreach (var corequisite in corequisites)
            {
                //Prerequisite found in non-scheduled course list
                if (unscheduledCourses.Find(x => x.Code == corequisite.Code) != null)
                {
                    prerequisitesSatisfied = false;
                    break;
                }
            }

            return prerequisitesSatisfied;
      
        }

        private void scheduleCourse(Course course, int semester, string semesterName, Sequence sequence)
        {
            var newCourseEntry = new Sequence.CourseEntry()
            {
                Course = course,
                Semester = semester,
                SemesterName = semesterName + " " + (semester + 1) / 2
            };

            sequence.CourseList.Add(newCourseEntry);
        }
    }

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

    public class CourseDetails
    {

        public CourseDetails()
        {
            sections = new List<CourseDetails.Section>();
        }
        public Course Course {get; set;}

        public IList<CourseDetails.Section> sections;

        public class Section
        {
            public Course.Lecture Lecture { get; set; }
            public IEnumerable<Course.Lab> Labs { get; set; }
            public IEnumerable<Course.Tutorial> Tutorials { get; set; }
        }
    }

    public class Constraint
    {
        [Display(Name = "Semester")]    
        public int Semester { get; set; }

        [Display(Name = "Number of courses")]
        [Required(ErrorMessage = "Number of courses you want to take is required")]
        [Range(0, 6, ErrorMessage = "Number of courses must be between 0 and 6")] 
        public int NumberOfCourses { get; set; }
    }
}