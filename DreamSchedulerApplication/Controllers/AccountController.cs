using Neo4jClient;
using System;
using System.Linq;
using System.Web.Mvc;
using DreamSchedulerApplication.Models;
using DreamSchedulerApplication.Libraries;
using System.Web.Security;

namespace DreamSchedulerApplication.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {

        private readonly IGraphClient client;

        public AccountController(IGraphClient graphClient)
        {
            client = graphClient;
        }
        

        //
        // GET: /Account/Login
        public ActionResult Login()
        {
            ViewBag.IDENTITYERROR = TempData["message"];
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new User();

                try
                {
                    //Find user account
                    //If not found, neo4j will throw an exception 
                    user = client
                                  .Cypher
                                  .Match("(n:User)")
                                  .Where(((User n) => n.Username == model.Username))
                                  .Return(n => n.As<User>())
                                  .Results.Single();
                }
                catch (InvalidOperationException)
                {
                    //If account not found, display invalid user account error
                    ModelState.AddModelError("", "A user with this username does not exist");
                    return View("login");
                }

                if (PasswordHash.ValidatePassword(model.Password, user.Password))
                {
                    //if user is logged in, create an encrypted cookie
                    FormsAuthentication.SetAuthCookie(user.Username, false);
                    if (user.Roles.Contains("admin")) return RedirectToAction("Database", "Admin");
                    else if (user.Roles.Contains("student")) return RedirectToAction("Index", "Student");

                    ModelState.AddModelError("", "Your account has not been activated yet");
                    LogOff();
                    return View("login");
                }
                else
                {
                    ModelState.AddModelError("", "The user name or password provided is incorrect");
                    return View("login");
                }
            }

            // model is not valid
            return View(model);
        }

        //
        // GET: /Account/Register
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var encryptedPassword = PasswordHash.CreateHash(model.Password);
                var newUser = new User { Username = model.Username, Password = encryptedPassword, Roles = "student" };

                var newStudent = new Student { FirstName = model.FirstName, LastName = model.LastName, StudentID = model.StudentID, Entry = model.Entry, GPA = model.GPA };

                try
                {
                    client.Cypher
                                .Create("(u:User {newUser})-[:IsA]->(s:Student {newStudent})")
                                .WithParam("newUser", newUser)
                                .WithParam("newStudent", newStudent)
                                .ExecuteWithoutResults();
                }
                catch (Neo4jClient.NeoException exception)
                {
                    if (exception.Message.Contains("Username")) { ModelState.AddModelError("", "User with such username already exists"); return View("Register"); }
                    else if (exception.Message.Contains("StudentID")) { ModelState.AddModelError("", "Student with such student ID number already exists"); return View("Register"); }
                    else throw exception;
                }

                //Create new session
                FormsAuthentication.SetAuthCookie(newUser.Username, false);
                return RedirectToAction("Index", "Student");
            }

            // model is not valid
            return View(model);
        }

        // GET: /Account/RequestAdminAccount
        public ActionResult RequestAdminAccount()
        {
            return View();
        }

        //
        // POST: /Account/RequestAdminAccount
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RequestAdminAccount(RequestAdminAccountViewModel model)
        {
            if (ModelState.IsValid)
            {
                var encryptedPassword = PasswordHash.CreateHash(model.Password);
                var newUser = new User { Username = model.Username, Password = encryptedPassword, Roles = "" };

                var newAdmin = new Admin { FirstName = model.FirstName, LastName = model.LastName };

                try
                {
                    client.Cypher
                                .Create("(u:User {newUser})-[:IsA]->(a:Admin {newAdmin})")
                                .WithParam("newUser", newUser)
                                .WithParam("newAdmin", newAdmin)
                                .ExecuteWithoutResults();
                }
                catch (Neo4jClient.NeoException exception)
                {
                    if (exception.Message.Contains("Username")) { ModelState.AddModelError("", "User with such username already exists"); return View("Register"); }
                    else throw exception;
                }

                return View("RequestAdminAccountConfirmation");
            }

            // model is not valid
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/EditAccount
        public ActionResult EditAccount()
        {
            var academicRecord = new Student();


            var student = client.Cypher
                                        .Match("(u:User)-[]->(s:Student)")
                                        .Where((User u) => u.Username == HttpContext.User.Identity.Name)
                                        .Return((s) => s.As<Student>())
                                        .Results.First();

            return View(student);
        }

        // POST: /Account/EditAccount
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditAccount(Student model)
        {
            if (ModelState.IsValid)
            {
                var newStudent = new Student { FirstName = model.FirstName, LastName = model.LastName, StudentID = model.StudentID, GPA = model.GPA, Entry = model.Entry };

                client.Cypher
                                .Match("(u:User)-->(s:Student)")
                                .Where((User u) => u.Username == HttpContext.User.Identity.Name)
                                .Set("s = {newStudent}")
                                .WithParam("newStudent", newStudent)
                                .ExecuteWithoutResults();

                if (HttpContext.User.IsInRole("admin")) return RedirectToAction("Database", "Admin");
                return RedirectToAction("Index", "Student");
            }
            else return View(model);
        }

        //Get /Account/ChangePassword
        public ActionResult ChangePassword()
        {
            var User = client.Cypher
                                        .Match("(u:User)-[]->(s:Student)")
                                        .Where((User u) => u.Username == HttpContext.User.Identity.Name)
                                        .Return((u) => u.As<ChangePasswordViewModel>())
                                        .Results.First();

            return View(User);
        }

        //POST : /Account/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(ChangePasswordViewModel model)
        {
            if(ModelState.IsValid)
            {
                var encryptedPassword = PasswordHash.CreateHash(model.Password);
                var newUser = new User { };//intialize 
                if (HttpContext.User.IsInRole("admin"))
                {
                    newUser = new User { Username = HttpContext.User.Identity.Name, Password = encryptedPassword, Roles = "admin" };
                }
                else
                {
                    newUser = new User { Username = HttpContext.User.Identity.Name, Password = encryptedPassword, Roles = "student" };
                }
                client.Cypher
                                .Match("(u:User)-->(s:Student)")
                                .Where((User u) => u.Username == HttpContext.User.Identity.Name)
                                .Set("u = {newUser}")
                                .WithParam("newUser", newUser)
                                .ExecuteWithoutResults();

                if (HttpContext.User.IsInRole("admin")) return RedirectToAction("Database", "Admin");
                return RedirectToAction("Index", "Student");
            }
            else return View(model);
        }

    }
}