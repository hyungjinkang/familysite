using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using FamilySite.DATA.EF;
using Microsoft.AspNet.Identity;

namespace FamilySite.UI.MVC.Controllers
{
    [Authorize]
    public class ReservationsController : Controller
    {
        private FamilySiteEntities db = new FamilySiteEntities();

        // GET: Reservations
        public ActionResult Index(string filter)
        {
            //if a user is "Admin" or "Parent", it can see all reservations of all members.
            //Other users can see only their records.
            if (User.IsInRole("Admin") || User.IsInRole("Parent"))
            {
                var reservations = db.Reservations.Include(r => r.UserDetail).Include(r => r.Lesson);
                return View(reservations.ToList());
            }
            else
            {
                string currentUserID = User.Identity.GetUserId();
                var reservations = db.Reservations.Include(r => r.UserDetail).Include(r => r.Lesson).Where(x => x.UserID == currentUserID);
                return View(reservations.ToList());
            }

        }

        // GET: Reservations/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            string currentUserID = User.Identity.GetUserId();
            Reservation reservation = db.Reservations.Find(id);
            if (reservation == null)
            {
                return HttpNotFound();
            }
            else
            {
                if (User.IsInRole("Parent") || User.IsInRole("Admin") || reservation.UserID == currentUserID)
                {
                    return View(reservation);
                }
                else
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
            }
        }


        // GET: Reservations/Create
        public ActionResult Create(int? id, int? filter)
        {
            //original create()
            //ViewBag.LessonID = new SelectList(db.Lessons, "LessonID", "LessonTitle");
            //ViewBag.CourseID = new SelectList(db.Courses1, "CourseID", "CourseName");

            //if a course is selected, only lessons of the course are displayed. (hjkang)
            ViewBag.LessonID = new SelectList(db.Lessons, "LessonID", "LessonTitle");

            var coursesWithLessons = db.Courses1.Include(c => c.Lessons).Where(c => c.Lessons.Count > 0);

            ViewBag.filter = new SelectList(coursesWithLessons, "CourseID", "CourseName");

            if (filter != null)
            {
                var lessons = db.Lessons.Where(l => l.CourseID == filter);
                if (lessons == null)
                {
                    ViewBag.LessonID = null;
                }
                else
                {
                    ViewBag.LessonID = new SelectList(lessons, "LessonID", "LessonTitle");

                }

            }

            // hjkang: when a user makes a reservation on a lesson in lesson index with a lesson id.
            // it only shows the lesson connected to the lesson id from the lesson index.
            if (id != null)
            {
                var lesson = db.Lessons.Where(l => l.LessonID == id);

                ViewBag.LessonID = new SelectList(lesson, "LessonID", "LessonTitle");

                var course = lesson.SingleOrDefault().CourseID;
                //In order to get courseID of lesson, I wrote below code. It needs to be simplified without foreach. (hjkang)
                //var course = 0;
                //foreach (var x in lesson)
                //{
                //    course = x.CourseID;
                //}
                ViewBag.filter = new SelectList(db.Courses1.Where(c => c.CourseID == course), "CourseID", "CourseName", course);
            }
            //end hjkang's reservation from lesson index

            // Users is displayed differently according to a role. (hjkang)
            string currentUserID = User.Identity.GetUserId();
            ViewBag.UserID = new SelectList(db.UserDetails.Where(u => u.UserID == currentUserID), "UserID", "FirstName");

            if (User.IsInRole("Admin") || User.IsInRole("Parent"))
            {
                ViewBag.UserID = new SelectList(db.UserDetails, "UserID", "FirstName");
            }
            //end of diplaying differently according to a role

            return View();
        }


        // POST: Reservations/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ReservationID,ReservationDate,UserID,LessonID")] Reservation reservation)
        {
            if (ModelState.IsValid)
            {
                db.Reservations.Add(reservation);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.UserID = new SelectList(db.UserDetails, "UserID", "FirstName", reservation.UserID);
            ViewBag.LessonID = new SelectList(db.Lessons, "LessonID", "LessonTitle", reservation.LessonID);
            return View(reservation);
        }


        // GET: Reservations/Edit/5
        public ActionResult Edit(int? id, int? filter)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Reservation reservation = db.Reservations.Find(id);
            if (reservation == null)
            {
                return HttpNotFound();
            }

            string currentUserID = User.Identity.GetUserId();
            if (User.IsInRole("Parent") || User.IsInRole("Admin") || reservation.UserID == currentUserID)
            {
                #region if User is authorized to see reservations.
                ViewBag.UserID = new SelectList(db.UserDetails.Where(u => u.UserID == reservation.UserID), "UserID", "FirstName"); //, reservation.UserID);
                if (User.IsInRole("Admin") || User.IsInRole("Parent"))
                {
                    ViewBag.UserID = new SelectList(db.UserDetails, "UserID", "FirstName", reservation.UserID);
                }
                //end of diplaying users differently according to a role

                var coursesWithLessons = db.Courses1.Include(c => c.Lessons).Where(c => c.Lessons.Count > 0);

                ViewBag.filter = new SelectList(coursesWithLessons, "CourseID", "CourseName", reservation.Lesson.CourseID);
                ViewBag.LessonID = new SelectList(db.Lessons, "LessonID", "LessonTitle", reservation.LessonID);

                //the value of filter is null when "Edit" is loaded at the first time. the filter value is selected lessons showed are changed according to the selected course.
                if (filter != null)
                {
                    var lessons = db.Lessons.Where(l => l.CourseID == filter);
                    if (lessons == null) //no need any more because courses in the select list are courses with lessons.
                    {
                        ViewBag.LessonID = null;
                    }
                    else
                    {
                        ViewBag.LessonID = new SelectList(lessons, "LessonID", "LessonTitle", reservation.LessonID);
                    }
                }
                return View(reservation);
            }//end if
            #endregion
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            }//end else
            
        }//end Get







        // POST: Reservations/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ReservationID,ReservationDate,UserID,LessonID")] Reservation reservation)
        {
            if (ModelState.IsValid)
            {
                db.Entry(reservation).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.UserID = new SelectList(db.UserDetails, "UserID", "FirstName", reservation.UserID);
            ViewBag.LessonID = new SelectList(db.Lessons, "LessonID", "LessonTitle", reservation.LessonID);
            return View(reservation);
        }


        // GET: Reservations/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Reservation reservation = db.Reservations.Find(id);
            if (reservation == null)
            {
                return HttpNotFound();
            }
            return View(reservation);
        }


        // POST: Reservations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Reservation reservation = db.Reservations.Find(id);
            db.Reservations.Remove(reservation);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
