using FamilySite.DATA.EF;
using Microsoft.AspNet.Identity;
using System.Net;
using System.Net.Mail;
using System.Web.Mvc;
using System.Linq;
using System.Data.Entity;


namespace FamilySite.UI.MVC.Controllers
{
    public class HomeController : Controller
    {
        private FamilySiteEntities db = new FamilySiteEntities();

        [HttpGet]
        public ActionResult Index()
        {
            var lessons = db.Lessons.Include(l => l.Cours);
            var courses = db.Courses1;
            var lessonViews = db.LessonViews;
            var courseCompletions = db.CourseCompletions;

            string currentUser = User.Identity.GetUserId();

            if (currentUser == null || User.IsInRole("Admin"))
            {
                ViewBag.lessonViewsNo = lessonViews.Count();
                ViewBag.courseCompletionsNo = courseCompletions.Count();
            }
            else
            {
                ViewBag.lessonViewsNo = lessonViews.Where(l => l.UserID == currentUser).Count();
                ViewBag.courseCompletionsNo = courseCompletions.Where(l => l.UserID == currentUser).Count();
            }
            ViewBag.currentUser = currentUser;
            ViewBag.lessonsNo = lessons.Count();
            ViewBag.coursesNo = courses.Count();
            ViewBag.lessons = lessons;

            #region find lessonID of top 3 lessonviews 
            //var top3 = lessonViews.Distinct().ToList();
            var top3 = lessonViews.GroupBy(x =>x.LessonID).OrderByDescending(x => x.Count()).ToList();
            
            if (top3.Count()>0)
            {
                var top1 = top3[0].Key;
                var top2 = top1;
                var top3_1 = top1;
                if (top3.Count() >= 2)
                {
                    top2 = top3[1].Key;
                    //top3_1 = top2;

                }
                if (top3.Count() >= 3)
                {
                    //top2 = top3[1].Key;
                    top3_1 = top3[2].Key;

                }

                ViewBag.lessonNo1 = top1;
                ViewBag.lessonNo2 = top2;
                ViewBag.lessonNo3 = top3_1;
            }
            else  
            {
                ViewBag.lessonNo1 = 1;
                ViewBag.lessonNo2 = 1;
                ViewBag.lessonNo3 = 1;
            }
            #endregion

            return View();
        }

        [HttpGet]
        [Authorize]
        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        [HttpGet]
        public ActionResult Contact()
        {
            //ViewBag.Message = "Your contact page.";

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Contact(Models.ContactViewModel contactInfo)
        {
            if (!ModelState.IsValid)
            {
                return View(contactInfo);
            }

            string body = string.Format(
                $"Name: {contactInfo.Name}<br />"
                + $"Email: {contactInfo.Email}<br />"
                + $"Subject: {contactInfo.Subject}<br />"
                + $"Message: {contactInfo.Message}");

            MailMessage msg = new MailMessage(
                "no-reply@hjkangweb.com",
                "speedkhj@gmail.com",
                contactInfo.Subject,
                body);

            msg.IsBodyHtml = true;
            msg.CC.Add("postmaster@hjkangweb.com");
            msg.Priority = MailPriority.High;

            SmtpClient client = new SmtpClient("mail.hjkangweb.com");
            client.Credentials = new NetworkCredential("no-reply@hjkangweb.com", "kang1226@@");
            client.EnableSsl = false;
            client.Port = 8889;

            using (client)
            {
                try
                {
                    client.Send(msg);
                }
                catch
                {
                    ViewBag.ErrorMessage = "There was an error sending your message.\n" + "Please try again";
                    return View();
                }
            }

            return View("ContactConfirmation", contactInfo);

        }
    }
}
