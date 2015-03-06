﻿using DreamSchedulerApplication.Models;
using Neo4jClient;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace DreamSchedulerApplication.Controllers
{
    [Authorize(Roles = "student")]
    public class AcademicRecordController : Controller
    {

        public AcademicRecordController(IGraphClient graphClient)
        {
            academicRecord = new AcademicRecord(graphClient);
        }

        private AcademicRecord academicRecord;

        //GET: AcademicRecord/Index
        public ActionResult Index()
        {
            return View(academicRecord.getAcademicRecord());
        }

        // GET: AcademicRecord/CreateCourseEntry
        public ActionResult CreateCourseEntry()
        {
            return View();
        }

        // POST: AcademicRecord/CreateCourseEntry
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateCourseEntry(AcademicRecord.CourseEntry courseEntry)
        {
            if (ModelState.IsValid)
            {   
                var course = academicRecord.addCourseEntry(courseEntry);

                if (course.Count() == 0)
                {
                    ModelState.AddModelError("", "Course does not exists or already has been added to the academic record");
                    return View(courseEntry);
                }

                return RedirectToAction("Index");
            }

            return View(courseEntry);
        }

        // GET: AcademicRecord/EditCourseEntry
        public ActionResult EditCourseEntry(string code)
        {
            if (code == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            AcademicRecord.CourseEntry completedCourse = academicRecord.getCourseEntry(code);

            if (completedCourse == null)
            {
                return HttpNotFound();
            }

            return View(completedCourse);
        }

        // POST: AcademicRecord/EditCourseEntry
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditCourseEntry(AcademicRecord.CourseEntry completedCourse)
        {
            if (ModelState.IsValid)
            {
                academicRecord.setCourseEntry(completedCourse);

                return RedirectToAction("Index");
            }
            return View(completedCourse);
        }

        // GET: AcademicRecord/DeleteCourseEntry
        public ActionResult DeleteCourseEntry(string code)
        {
            if (code == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            AcademicRecord.CourseEntry completedCourse = academicRecord.getCourseEntry(code);

            if (completedCourse == null)
            {
                return HttpNotFound();
            }

            return View(completedCourse);
        }

        // POST: AcademicRecord/DeleteCourseEntry
        [HttpPost, ActionName("DeleteCourseEntry")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteCourse(string code)
        {
            if (ModelState.IsValid)
            {
                academicRecord.removeCourseEntry(code);

                return RedirectToAction("Index");
            }
            return View(code);
        }


    }
}