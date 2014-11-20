using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Neo4jClient;
using DreamSchedulerApplication.Models;
using System.Web.Mvc;
using System.Web.Mvc.Filters;

namespace DreamSchedulerApplication.Security
{

    /*
     * Security Measures
     * 
     * 
     * */
       
    public class PrivateData
    {

        GraphClient client = new GraphClient(new Uri("http://localhost:7474/db/data"));

        //FIND USER role
        public string GetUserRole()
        {
            var username1 = HttpContext.Current.User.Identity;

            if (username1.IsAuthenticated)
            {

                client.Connect();
                var user = client
                          .Cypher
                          .Match("(n:User)")
                          .Where(((User n) => n.Username == username1.Name))
                          .Return(n => n.As<User>())
                          .Results.Single();

                string role = user.Roles;
                return role;
            }
            else
                return null;//user is not authenticated
        }

        //FIND username
        public string GetUserName()
        {
            string username = "";
            if (HttpContext.Current.Request.IsAuthenticated)
            {
                username = HttpContext.Current.User.Identity.Name;
                return username;
            }

            return username =null;
        }


        //// if required specific data quicly 
        ////FIND STUDENT then return ( first name, last name, id, gpa)
        //public string GetStudentID()
        //{

        //    if(HttpContext.Current.User.Identity.Name != null )
        //    {
        //        client.Connect();
        //        var studentFound = client.Cypher
        //            .Match("(u:User)-[]->(s:Student)")
        //            .Where((User u) => u.Username == HttpContext.Current.User.Identity.Name)
        //            .WithParam("username", HttpContext.Current.User.Identity.Name)
        //            .Return((s) => s.As<Student>())
        //            .Results.First();
                
        //        return studentFound.StudentID;
        //    }
        //    return null; // user isn't authenticated

        //}
        //public string GetStudentFN()
        //{


        //    if (HttpContext.Current.User.Identity.Name != null)
        //    {
        //        client.Connect();
        //        var studentFound = client.Cypher
        //            .Match("(u:User)-[]->(s:Student)")
        //            .Where((User u) => u.Username == HttpContext.Current.User.Identity.Name)
        //            .WithParam("username", HttpContext.Current.User.Identity.Name)
        //            .Return((s) => s.As<Student>())
        //            .Results.First();

        //        return studentFound.FirstName;
        //    }
        //    return null; // user isn't authenticated
        //}

        //public string GetStudentLN()
        //{
        //    if (HttpContext.Current.User.Identity.Name != null)
        //    {
        //        client.Connect();
        //        var studentFound = client.Cypher
        //            .Match("(u:User)-[]->(s:Student)")
        //            .Where((User u) => u.Username == HttpContext.Current.User.Identity.Name)
        //            .WithParam("username", HttpContext.Current.User.Identity.Name)
        //            .Return((s) => s.As<Student>())
        //            .Results.First();

        //        return studentFound.LastName;
        //    }
        //    return null; // user isn't authenticated

        //}
        //public string GetStudentGPA()
        //{
        //    // find user from authenticated user
        //    string username = GetUserName();

        //    if (HttpContext.Current.User.Identity.Name != null)
        //    {
        //        client.Connect();
        //        var studentFound = client.Cypher
        //            .Match("(u:User)-[]->(s:Student)")
        //            .Where((User u) => u.Username == HttpContext.Current.User.Identity.Name)
        //            .WithParam("username", HttpContext.Current.User.Identity.Name)
        //            .Return((s) => s.As<Student>())
        //            .Results.First();

        //        return studentFound.GPA;
        //    }
        //    return null; // user isn't authenticated

        //}



    }
}











