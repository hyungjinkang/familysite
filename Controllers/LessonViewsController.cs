using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using FamilySite.DATA.EF;//Entity Framwork Logic
using Microsoft.AspNet.Identity;//Access to Identity Framework Classes and GetUserId()

namespace FamilySite.UI.MVC.Controllers
{
    [Authorize]
    public class LessonViewsController : Controller
    {
        private FamilySiteEntities db = new FamilySiteEntities();

        // GET: LessonViews
       
        public ActionResult Index()
        {
            if (User.IsInRole("Admin") || User.IsInRole("Parent"))
            {

                return RedirectToAction("IndexManager");

            }
            else
            {
                //displays the current user's lessons viewed.
                //1)Get the current user's ID
                //2)use the ID to filter the results from ALL items in the lesson views table.

                //1)
                string currentUserID = User.Identity.GetUserId();

                //2)

                var lessonViews = db.LessonViews.Include(l => l.Lesson).Include(l => l.UserDetail).Where(x => x.UserID == currentUserID);
                return View(lessonViews.ToList());
            }
            
        }

        [Authorize(Roles ="Admin, Parent")]
        public ActionResult IndexManager(string filter)
        {
            
            var lessonViews = db.LessonViews.Include(l => l.Lesson).Include(l => l.UserDetail);

            var members = db.UserDetails.ToList();

            foreach (var x in members)
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    if(x.UserID == filter)
                    {
                        lessonViews =
                            db.LessonViews.Where(l => l.UserDetail.FirstName == x.FirstName).Include(l => l.Lesson).Include(l => l.UserDetail);
                    }
                }
            }

            ViewBag.filter = new SelectList(db.UserDetails, "UserID", "FirstName");        

            return View(lessonViews.ToList());
        }


        

        // GET: LessonViews/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            string currentUserID = User.Identity.GetUserId();
            LessonView lessonView = db.LessonViews.Find(id);
            if (lessonView == null)
            {
                return HttpNotFound();
            }
             else
            {
                if (User.IsInRole("Parent") || User.IsInRole("Admin") || lessonView.UserID == currentUserID)
                {
                    return View(lessonView);
                }
                else
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
            }
        }

        [Authorize(Roles ="Admin")]
        // GET: LessonViews/Create
        public ActionResult Create()
        {
            ViewBag.LessonID = new SelectList(db.Lessons, "LessonID", "LessonTitle");
            ViewBag.UserID = new SelectList(db.UserDetails, "UserID", "FirstName");
            return View();
        }

        [Authorize(Roles = "Admin")]
        // POST: LessonViews/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "LessonViewID,UserID,LessonID,DateViewed")] LessonView lessonView)
        {
            if (ModelState.IsValid)
            {
                db.LessonViews.Add(lessonView);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.LessonID = new SelectList(db.Lessons, "LessonID", "LessonTitle", lessonView.LessonID);
            ViewBag.UserID = new SelectList(db.UserDetails, "UserID", "FirstName", lessonView.UserID);
            return View(lessonView);
        }

        [Authorize(Roles = "Admin")]
        // GET: LessonViews/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LessonView lessonView = db.LessonViews.Find(id);
            if (lessonView == null)
            {
                return HttpNotFound();
            }
            ViewBag.LessonID = new SelectList(db.Lessons, "LessonID", "LessonTitle", lessonView.LessonID);
            ViewBag.UserID = new SelectList(db.UserDetails, "UserID", "FirstName", lessonView.UserID);
            return View(lessonView);
        }

        [Authorize(Roles = "Admin")]
        // POST: LessonViews/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "LessonViewID,UserID,LessonID,DateViewed")] LessonView lessonView)
        {
            if (ModelState.IsValid)
            {
                db.Entry(lessonView).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.LessonID = new SelectList(db.Lessons, "LessonID", "LessonTitle", lessonView.LessonID);
            ViewBag.UserID = new SelectList(db.UserDetails, "UserID", "FirstName", lessonView.UserID);
            return View(lessonView);
        }

        [Authorize(Roles = "Admin")]
        // GET: LessonViews/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LessonView lessonView = db.LessonViews.Find(id);
            if (lessonView == null)
            {
                return HttpNotFound();
            }
            return View(lessonView);
        }

        [Authorize(Roles = "Admin")]
        // POST: LessonViews/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            LessonView lessonView = db.LessonViews.Find(id);
            db.LessonViews.Remove(lessonView);
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
