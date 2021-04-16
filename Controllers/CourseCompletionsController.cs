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
    public class CourseCompletionsController : Controller
    {
        private FamilySiteEntities db = new FamilySiteEntities();

        // GET: CourseCompletions
        public ActionResult Index(string filter)
        {
            //if a user is "Admin" or "Parent", it can see all course completions of all members.
            //Other users can see only their records.
            if (User.IsInRole("Admin") || User.IsInRole("Parent"))
            {
                var courseCompletions = db.CourseCompletions.Include(c => c.Cours).Include(c => c.UserDetail);
                var members = db.UserDetails.ToList();

                foreach (var x in members)  // select list 
                {
                    if(!string.IsNullOrEmpty(filter))
                    {
                        if(x.UserID == filter)
                        {
                            courseCompletions = db.CourseCompletions.Where(c => c.UserDetail.FirstName == x.FirstName).Include(c => c.Cours).Include(c => c.UserDetail);
                        }
                    }
                }

                ViewBag.filter = new SelectList(db.UserDetails, "UserID", "FirstName");
                return View(courseCompletions.ToList());
            }
            else // a user is not a admin or a parent. can see only their records.
            {
                string currentUserID = User.Identity.GetUserId();
                var courseCompletions = db.CourseCompletions.Include(c => c.Cours).Include(c => c.UserDetail).Where(x => x.UserID == currentUserID);
                return View(courseCompletions.ToList());
            }
            
        }

        // GET: CourseCompletions/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            string currentUserID = User.Identity.GetUserId();
            CourseCompletion courseCompletion = db.CourseCompletions.Find(id);
            if (courseCompletion == null)
            {
                return HttpNotFound();
            }
            else
            {
                if (User.IsInRole("Parent") || User.IsInRole("Admin") || courseCompletion.UserID == currentUserID)
                {
                    return View(courseCompletion);
                }
                else
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
            }
        }

        [Authorize(Roles ="Admin")]
        // GET: CourseCompletions/Create
        public ActionResult Create()
        {
            ViewBag.CourseID = new SelectList(db.Courses1, "CourseID", "CourseName");
            ViewBag.UserID = new SelectList(db.UserDetails, "UserID", "FirstName");
            return View();
        }

        [Authorize(Roles = "Admin")]
        // POST: CourseCompletions/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "CourseCompletionID,UserID,CourseID,DateCompleted")] CourseCompletion courseCompletion)
        {

            if (ModelState.IsValid)
            {

                db.CourseCompletions.Add(courseCompletion);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CourseID = new SelectList(db.Courses1, "CourseID", "CourseName", courseCompletion.CourseID);
            ViewBag.UserID = new SelectList(db.UserDetails, "UserID", "FirstName", courseCompletion.UserID);
            return View(courseCompletion);
        }

        [Authorize(Roles = "Admin")]
        // GET: CourseCompletions/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CourseCompletion courseCompletion = db.CourseCompletions.Find(id);
            if (courseCompletion == null)
            {
                return HttpNotFound();
            }
            ViewBag.CourseID = new SelectList(db.Courses1, "CourseID", "CourseName", courseCompletion.CourseID);
            ViewBag.UserID = new SelectList(db.UserDetails, "UserID", "FirstName", courseCompletion.UserID);
            return View(courseCompletion);
        }

        [Authorize(Roles ="Admin")]
        // POST: CourseCompletions/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "CourseCompletionID,UserID,CourseID,DateCompleted")] CourseCompletion courseCompletion)
        {
            if (ModelState.IsValid)
            {
                db.Entry(courseCompletion).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CourseID = new SelectList(db.Courses1, "CourseID", "CourseName", courseCompletion.CourseID);
            ViewBag.UserID = new SelectList(db.UserDetails, "UserID", "FirstName", courseCompletion.UserID);
            return View(courseCompletion);
        }

        [Authorize(Roles = "Admin")]
        // GET: CourseCompletions/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CourseCompletion courseCompletion = db.CourseCompletions.Find(id);
            if (courseCompletion == null)
            {
                return HttpNotFound();
            }
            return View(courseCompletion);
        }

        [Authorize(Roles ="Admin")]
        // POST: CourseCompletions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            CourseCompletion courseCompletion = db.CourseCompletions.Find(id);
            db.CourseCompletions.Remove(courseCompletion);
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
