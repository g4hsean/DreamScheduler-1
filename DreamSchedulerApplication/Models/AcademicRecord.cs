using Neo4jClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DreamSchedulerApplication.Models
{
    public class AcademicRecord
    {

        protected readonly IGraphClient client;

        public AcademicRecord(IGraphClient client)
        {
            this.client = client;
        }

        public Student Student {get; set;}

        public IEnumerable<CourseEntry> CompletedCourses;

        public class CourseEntry
        {
            public Course Course { get; set; }
            public Completed Completed { get; set; }
        }

        public AcademicRecord getAcademicRecord()
        {
            var currentUserName = HttpContext.Current.User.Identity.Name;

            var currentStudent = client.Cypher
                        .Match("(u:User)-->(s:Student)")
                        .Where((User u) => u.Username == currentUserName)
                        .Return((s) => s.As<Student>())
                        .Results.First();

            this.Student = currentStudent;

            this.CompletedCourses = client.Cypher
                         .Match("(s:Student)-[r:Completed]->(c:Course)")
                         .Where((Student s) => s.StudentID == currentStudent.StudentID)
                         .Return((c, r) => new AcademicRecord.CourseEntry
                         {
                             Course = c.As<Course>(),
                             Completed = r.As<Completed>()
                         })
                         .OrderBy("r.semester")
                         .Results;

            return this;
        }

        public CourseEntry getCourseEntry(string code)
        {
            var currentUserName = HttpContext.Current.User.Identity.Name;

            return client.Cypher
                                .Match("(u:User)-->(s:Student)-[r:Completed]->(c:Course)")
                                .Where((User u) => u.Username == currentUserName)
                                .AndWhere((Course c) => c.Code == code)
                                .Return((c, r) => new AcademicRecord.CourseEntry
                                {
                                    Completed = r.As<Completed>(),
                                    Course = c.As<Course>()
                                })
                                .Results
                                .Single();
        }

        public IEnumerable<Course> addCourseEntry(CourseEntry courseEntry)
        {
            var currentUserName = HttpContext.Current.User.Identity.Name;

            return  client.Cypher
                                    .Match("(c:Course), (u:User)-->(s:Student)")
                                    .Where((Course c) => c.Code == courseEntry.Course.Code)
                                    .AndWhere((User u) => u.Username == currentUserName)
                                    .AndWhere("NOT (s)-[:Completed]->(c)")
                                    .Create("(s)-[r:Completed {completed}]->(c)")
                                    .WithParam("completed", courseEntry.Completed)
                                    .Return(c => c.As<Course>())
                                    .Results;
        }

        public void setCourseEntry(CourseEntry courseEntry)
        {
            var currentUserName = HttpContext.Current.User.Identity.Name;

            client.Cypher
                         .Match("(u:User)-->(s:Student)-[r:Completed]->(c:Course)")
                         .Where((User u) => u.Username == currentUserName)
                         .AndWhere((Course c) => c.Code == courseEntry.Course.Code)
                         .Set("r = {newRelationship}")
                         .WithParam("newRelationship", courseEntry.Completed)
                         .ExecuteWithoutResults();
        }

        public void removeCourseEntry(string code)
        {
            var currentUserName = HttpContext.Current.User.Identity.Name;

            client.Cypher
                         .Match("(u:User)-->(s:Student)-[r:Completed]->(c:Course)")
                         .Where((User u) => u.Username == currentUserName)
                         .AndWhere((Course c) => c.Code == code)
                         .Delete("r")
                         .ExecuteWithoutResults();
        }

    }
}