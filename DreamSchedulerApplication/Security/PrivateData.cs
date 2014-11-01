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
     * 1- when user log in : create FormsAuthentication.SetAuthCookie(user.Username, false);
     *  - this will create cookie containing the username, it will be destroyed when the user close his browser
     *  
     * 2- When ever a filter is processed , it will ask for the role of username
     *  - to find role, user must be authenticated, then it will search database 
     * 
     * 3- Unique account with unique student id 
     *  - before creation account : check if username exist and if that student id exist
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
                client.Connect();//connect to database
                var user = client
                          .Cypher
                          .Match("(n:User)")
                          .Where(((User n) => n.Username == username1.Name))
                          .Return(n => n.As<User>())
                          .Results.Single();

                string role = user.Role;
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

        //Find if user already exist in data
        public bool UserUnique(string username)
        {

            bool UserUnique;
            try
            {
                //Find user account
                //If not found, neo4j will throw an exception 
                client.Connect();//connect to database
                var test = client
                              .Cypher
                              .Match("(n:User)")
                              .Where(((User n) => n.Username == username))
                              .Return(n => n.As<User>())
                              .Results.Single();

                return UserUnique = false;
                
            }
            catch (InvalidOperationException)
            {
                //does not exist
                return UserUnique = true;
            }
        }
        //Find if student Id already exist in data
        public bool IdUnique(string sID)
        {

            bool idUnique;
            try
            {
                //Find user account
                //If not found, neo4j will throw an exception 
                client.Connect();//connect to database
                var test = client
                              .Cypher
                              .Match("(s:Student)")
                              .Where(((Student s) => s.StudentID == sID))
                              .Return(s => s.As<Student>())
                              .Results.Single();

                return idUnique = false;

            }
            catch (InvalidOperationException)
            {
                //does not exist
                return idUnique = true;
            }
        }



        // if required specific data quicly 
        //FIND STUDENT then return ( first name, last name, id, gpa)
        public string GetStudentID()
        {
            // find user from authenticated user
            //var username = HttpContext.Current.User.Identity;
            string username = GetUserName();

            if (username != null )
            {
                client.Connect();//connect to database
                var studentFound = client.Cypher
                    .Match("(u:User)-[]->(s:Student)")
                    .Where((User u) => u.Username == username)
                    .WithParam("username", username)
                    .Return((s) => s.As<Student>())
                    .Results.First();
                
                return studentFound.StudentID;
            }
            return null; // user isn't authenticated

        }
        public string GetStudentFN()
        {
            // find user from authenticated user
            string username = GetUserName();

            if (username != null)
            {
                var studentFound = client.Cypher
                    .Match("(u:User)-[]->(s:Student)")
                    .Where((User u) => u.Username == username)
                    .WithParam("username", username)
                    .Return((s) => s.As<Student>())
                    .Results.First();



                return studentFound.FirstName;
            }
            return null; // user isn't authenticated
        }

        public string GetStudentLN()
        {
            // find user from authenticated user
            string username = GetUserName();

            if (username!=null)
            {
                var studentFound = client.Cypher
                    .Match("(u:User)-[]->(s:Student)")
                    .Where((User u) => u.Username == username)
                    .WithParam("username", username)
                    .Return((s) => s.As<Student>())
                    .Results.First();



                return studentFound.LastName;
            }
            return null; // user isn't authenticated

        }
        public string GetStudentGPA()
        {
            // find user from authenticated user
            string username = GetUserName();

            if (username!=null)
            {
                client.Connect();//connect to database
                var studentFound = client.Cypher
                    .Match("(u:User)-[]->(s:Student)")
                    .Where((User u) => u.Username == username)
                    .WithParam("username", username)
                    .Return((s) => s.As<Student>())
                    .Results.First();



                return studentFound.GPA;
            }
            return null; // user isn't authenticated

        }



    }
}











