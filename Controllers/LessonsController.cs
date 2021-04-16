using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using FamilySite.DATA.EF;
using Microsoft.AspNet.Identity;//User.Identity.GetUserId()
using System.Net.Mail;

namespace FamilySite.UI.MVC.Controllers
{
    public class LessonsController : Controller
    {
        private FamilySiteEntities db = new FamilySiteEntities();

        // GET: Lessons
        public ActionResult Index()
        {
            var lessons = db.Lessons.Include(l => l.Cours);
            return View(lessons.ToList());
        }

        // GET: Lessons/Details/5
        public ActionResult Details(int? id)//id = lesson id
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Lesson lesson = db.Lessons.Find(id);
            if (lesson == null)
            {
                return HttpNotFound();
            }

            #region lesson view record creation (coder: hjkang)
            string currentUserID = User.Identity.GetUserId();
            if (currentUserID != null)
            {
                //LessonView lessonview = new LessonView() {LessonID=lesson.LessonID, UserID=currentUserID, DateViewed=DateTime.Now };

                //lessonview.LessonID = lesson.LessonID;
                //lessonview.UserID = currentUserID;
                //lessonview.DateViewed = DateTime.Now;
                db.LessonViews.Add(new LessonView() { LessonID = lesson.LessonID, UserID = currentUserID, DateViewed = DateTime.Now });
                db.SaveChanges();

                #endregion

                #region record creation of course completion  (coder: hjkang)
                // if # of lessons in the course == # of lesson views by the user, then the course of the user is completed.

                //# of lessons in the course

                //# of lesson views for the course 
                var nbrLessonsViewed = 0;
                //var lessonViews = db.LessonViews.
                 //                 Where(x => (x.Lesson.CourseID == lesson.CourseID)
                 //                       && (x.UserID == currentUserID));
                var fc = db.LessonViews.Where(x => (x.Lesson.CourseID == lesson.CourseID)
                                        && (x.UserID == currentUserID)).OrderBy(x => x.LessonID).ToList();

                if (fc.Count() == 1)
                {
                    nbrLessonsViewed = 1;
                }
                else
                {
                    for (int i = 0; i < (fc.Count() - 1); i++)
                    {
                        var elementA = fc.ElementAt(i);
                        var elementB = fc.ElementAt(i + 1);
                        if (elementA.LessonID != elementB.LessonID)
                        {
                            nbrLessonsViewed++;
                        }
                    }
                    nbrLessonsViewed += 1;
                }
                //var nbrLessons = db.Lessons.Where(x => x.CourseID == lesson.CourseID).Count();

                if (nbrLessonsViewed == db.Lessons.Where(x => x.CourseID == lesson.CourseID).Count())
                {
                    //courseComplete.UserID = currentUserID;
                    //courseComplete.CourseID = lesson.CourseID;
                    //courseComplete.DateCompleted = DateTime.Now;

                    //CourseCompletion courseComplete = new CourseCompletion() { UserID = currentUserID, CourseID = lesson.CourseID, DateCompleted = DateTime.Now };

                    db.CourseCompletions.Add(new CourseCompletion() { UserID = currentUserID, CourseID = lesson.CourseID, DateCompleted = DateTime.Now });
                    db.SaveChanges();

                    //send email to the parent(a manager) to notify when a child completes a course
                    //var user = db.UserDetails.Where(u => u.UserID == currentUserID).SingleOrDefault();

                    string body = string.Format($"{db.UserDetails.Where(u => u.UserID == currentUserID).SingleOrDefault().FirstName}"
                        + " has completed " + $"the course of {lesson.Cours.CourseName}" + $" on {DateTime.Now}.");

                    MailMessage msg = new MailMessage(
                        "no-reply@hjkangweb.com",
                        "speedkhj@gmail.com",
                        "Your child completed a course.",
                        body);

                    msg.IsBodyHtml = true;
                    msg.CC.Add("postmaster@hjkangweb.com");

                    SmtpClient client = new SmtpClient("mail.hjkangweb.com");

                    client.Credentials = new NetworkCredential("no-reply@hjkangweb.com", "kang1226@@");
                    client.EnableSsl = false;
                    client.Port = 25;

                    using (client)
                    {
                        try
                        {
                            client.Send(msg);
                        }
                        catch
                        {
                            ViewBag.ErrorMessage = "There was an error sending complete notification. \n" + "Please notify your parent you have completed the course.";
                        }
                    }

                }

                #endregion
            }

            return View(lesson);
        }

        [Authorize(Roles = "Admin")]
        // GET: Lessons/Create
        public ActionResult Create()
        {
            ViewBag.CourseID = new SelectList(db.Courses1, "CourseID", "CourseName");
            return View();
        }

        [Authorize(Roles = "Admin")]
        // POST: Lessons/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "LessonID,LessonTitle,CourseID,Introduction,VideoURL,PdfFileName,IsActive,ReservationLimit")] Lesson lesson, HttpPostedFileBase pdfFile)
        {
            if (ModelState.IsValid)
            {
                string lessonMaterial = "noFile.jpg";
                if (pdfFile != null)
                {
                    lessonMaterial = pdfFile.FileName;
                    string ext = lessonMaterial.Substring(lessonMaterial.LastIndexOf("."));
                    //.pdf, .doc, .docx, .html, .xlsx, .jpg
                    string[] goodExts = new string[] { ".pdf", ".doc", ".docx", ".html", ".xlsx", ".xls", ".jpg", ".png", ".gif", ".jpeg" };
                    if (goodExts.Contains(ext.ToLower()))
                    {
                        lessonMaterial = Guid.NewGuid() + lessonMaterial;
                        pdfFile.SaveAs(Server.MapPath("~/Content/textfiles/Lessons/" + lessonMaterial));
                    }
                    else
                    {
                        lessonMaterial = "noFile.jpg";
                    }
                }
                lesson.PdfFileName = lessonMaterial;

                db.Lessons.Add(lesson);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CourseID = new SelectList(db.Courses1, "CourseID", "CourseName", lesson.CourseID);
            return View(lesson);
        }

        [Authorize(Roles = "Admin")]
        // GET: Lessons/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Lesson lesson = db.Lessons.Find(id);
            if (lesson == null)
            {
                return HttpNotFound();
            }
            ViewBag.CourseID = new SelectList(db.Courses1, "CourseID", "CourseName", lesson.CourseID);
            return View(lesson);
        }

        [Authorize(Roles = "Admin")]
        // POST: Lessons/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "LessonID,LessonTitle,CourseID,Introduction,VideoURL,PdfFileName,IsActive,ReservationLimit")] Lesson lesson, HttpPostedFileBase pdfFile)
        {
            if (ModelState.IsValid)
            {
               
                if (pdfFile != null)
                {
                    string lessonMaterial = pdfFile.FileName;
                    string ext = lessonMaterial.Substring(lessonMaterial.LastIndexOf("."));
                    //.pdf, .doc, .docx, .html, .xlsx, .jpg
                    string[] goodExts = new string[] { ".pdf", ".doc", ".docx", ".html", ".xlsx", ".xls", ".jpg", ".png", ".gif", ".jpeg" };
                    if (goodExts.Contains(ext.ToLower()))
                    {
                        lessonMaterial = Guid.NewGuid() + lessonMaterial;
                        pdfFile.SaveAs(Server.MapPath("~/Content/textfiles/Lessons/" + lessonMaterial));
                        lesson.PdfFileName = lessonMaterial;

                        if(Session["currentFile"].ToString() != "noFile.jpg") 
                        {
                            System.IO.File.Delete(Server.MapPath("~/Content/textfiles/Lessons/" + Session["currentFile"].ToString()));
                        }
                    }                   
                }

                db.Entry(lesson).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CourseID = new SelectList(db.Courses1, "CourseID", "CourseName", lesson.CourseID);
            return View(lesson);
        }

        [Authorize(Roles = "Admin")]
        // GET: Lessons/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Lesson lesson = db.Lessons.Find(id);
            if (lesson == null)
            {
                return HttpNotFound();
            }
            //Handle any delete that would orphon an connected record
            if (lesson.Reservations.Count > 0 || lesson.LessonViews.Count > 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            return View(lesson);
        }

        [Authorize(Roles = "Admin")]
        // POST: Lessons/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Lesson lesson = db.Lessons.Find(id);
            db.Lessons.Remove(lesson);
            if(lesson.PdfFileName != "noFile.jpg")
            {
                System.IO.File.Delete(Server.MapPath("~/Content/textfiles/Lessons/" + lesson.PdfFileName));
            }

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
