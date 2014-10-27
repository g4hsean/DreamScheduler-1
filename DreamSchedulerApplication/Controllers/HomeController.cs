using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Neo4jClient;
using DreamSchedulerApplication.Models;

namespace DreamSchedulerApp.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            if (Session["User"] != null)
            {
                if ((string)Session["User"] == "admin")//if user is admin
                {
                    return RedirectToAction("HomeAdmin", "Home");
                }
                else
                {
                    return RedirectToAction("Home", "Home");
                }
            }
            return View();
        }

        //home for logged in user
        public ActionResult Home()
        {
            if (Session["User"] != null)
            {
                if((string)Session["User"] == "admin")
                {
                    return RedirectToAction("HomeAdmin", "Home");//redirect admin to correct home page
                }
                else
                    return View(); //normal user 
            }
            return RedirectToAction("Index"); //user tried to use direct url to Home but isn't logged in  
        }
        //admin home
        public ActionResult HomeAdmin()
        {
            if (Session["User"] != null)
            {
                if((string)Session["User"] == "admin")
                {
                    return View();
                }
                else
                    return RedirectToAction("Index", "home");
            }
            else
            {
                return RedirectToAction("Index", "home");
            }
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Account()
        {
            if (Session["User"] != null)
                return View();
            else
                return RedirectToAction("index");
        }

        ///////SCHEDULER////////////////////////////////////////////////////////////////////////////////
        public ActionResult Scheduler()
        {
            if (Session["User"] != null & (string)Session["User"] != "admin")
            {
                return View();
            }
            else
            {
                return RedirectToAction("index");
            }
        }




        //show sequence page
        public ActionResult Sequence()
        {
            if(Session["User"] != null & (string)Session["User"] != "admin")
            {
                return View();
            }
            else
            {
                return RedirectToAction("index");
            }
        }
        //show professor page
        public ActionResult Professors()
        {
            if (Session["User"] != null & (string)Session["User"] != "admin")
            {
                return View();
            }
            else
            {
                return RedirectToAction("index");
            }
        }
        //show courses page
        public ActionResult Courses()
        {
            if (Session["User"] != null & (string)Session["User"] != "admin")
            {
                return View();
            }
            else
            {
                return RedirectToAction("index");
            }
        }

        //show student record page
        public ActionResult StudentRecord()
        {
            
            //if user is  logged in and is not admin
            if(Session["User"] != null & (string)Session["User"] != "admin")
            {
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
        //show database page
        public ActionResult Database()
        {
            if((string)Session["User"] == "admin")
            {
                return View();
            }
            return RedirectToAction("index");
        }
        


        //Login & create account & logout /////////////////////////////////////////////////////////////////////////////
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public ActionResult LoginValidation(LoginTest test)
        {
            if (ModelState.IsValid)
            {
                //store input in strings
                string username = test.Username;
                string password = test.Password;

                GraphClient client = new GraphClient(new Uri("http://localhost:7474/db/data"));
                client.Connect();

                var query = client
                    .Cypher
                    .Match("(n:Account)")
                    .Where(((LoginTest n) => n.Username == username))
                     .Return(n => n.As<LoginTest>())
                        .Results;

                try
                {
                    var testing = query.Single();
                    if (testing.Password == test.Password)
                    {
                        //if user is logged in, create session
                        Session["User"] = query.Single().Username;
                        return RedirectToAction("index");
                    }
                    else
                    {
                        ViewBag.errorPassword = "password is wrong, try again";
                        return View("login");
                    }
                }//if we can't find the account = it doest not exist, neo4j will cause a error 
                catch (InvalidOperationException)
                {
                    //error invalid user account 
                    ViewBag.errorUsername = "this user does not exist ";
                    return View("login");
                }
            }
            // model is not valid
            return View("login", test);

        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CreateValidation(LoginTest test)
        {
            if (ModelState.IsValid)
            {
                // save to db, for instance
                GraphClient client = new GraphClient(new Uri("http://localhost:7474/db/data"));
                client.Connect();

                //create newaccount object with input data
                var newAccount = new LoginTest { Username = test.Username, Password = test.Password };

                // create the account in the database
                client.Cypher
                    .Create("(Account:Account {newAcount})")
                    .WithParam("newAcount", newAccount)//against cypher-injection 
                    .ExecuteWithoutResults();
                return RedirectToAction("Login");
            }
            // model is not valid
            return View("Create", test);
        }
        public ActionResult Logout()
        {
            Session.Clear();
            return Redirect("index");

        }
    }
}