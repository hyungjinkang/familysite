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
using System.Drawing;
using FamiliySite.Domain.Services;
//using System.Drawing;

namespace FamilySite.UI.MVC.Controllers
{
    [Authorize]
    public class UserDetailsController : Controller
    {
        private FamilySiteEntities db = new FamilySiteEntities();

        // GET: UserDetails
        public ActionResult Index()
        {
            var userDetails = db.UserDetails.Include(u => u.AspNetUser);

            string currentUserID = User.Identity.GetUserId();

            if (currentUserID == null)
            {
                ViewBag.anonymousUser = "Please Resister or Log in to see members info.";
                return View();
            }

            ViewBag.currentUser = currentUserID;
            return View(userDetails.ToList());
        }


        // GET: UserDetails/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UserDetail userDetail = db.UserDetails.Find(id);
            if (userDetail == null)
            {
                return HttpNotFound();
            }
                return View(userDetail);
        }

        // GET: UserDetails/Create
        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            string currentUserID = User.Identity.GetUserId();

            ViewBag.UserID = new SelectList(db.AspNetUsers.Where(a => a.Id == currentUserID), "Id", "Email");


            if (User.IsInRole("Admin"))
            {
                ViewBag.UserID = new SelectList(db.AspNetUsers, "Id", "Email");
            }
                      
            return View();
        }

        // POST: UserDetails/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        
        [Authorize(Roles ="Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "UserID,FirstName,LastName,AvatarImage")] UserDetail userDetail)
        {
            if (db.UserDetails.Where(u => u.UserID == userDetail.UserID).Count() > 0)
            {
                ViewBag.alreadyExists = true;
                ViewBag.UserID = userDetail.UserID;
                return View();
            }
            
            if (ModelState.IsValid)
            {
                
                db.UserDetails.Add(userDetail);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.UserID = new SelectList(db.AspNetUsers, "Id", "Email", userDetail.UserID);
            return View(userDetail);
        }

        // GET: UserDetails/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            UserDetail userDetail = db.UserDetails.Find(id);
            if (userDetail == null)
            {
                return HttpNotFound();
            }

            string currentUserID = User.Identity.GetUserId();
            if (User.IsInRole("Admin") || userDetail.UserID == currentUserID)
            {
                //ViewBag.UserID = new SelectList(db.AspNetUsers, "Id", "Email", userDetail.UserID);
                return View(userDetail);
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            }//end else

        }

        // POST: UserDetails/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "UserID,FirstName,LastName,AvatarImage")] UserDetail userDetail, HttpPostedFileBase avatarImage)
        {
            if (ModelState.IsValid)
            {
                #region Simple File Upload
                //set a default image name (noImage.jpg)
            
                //if the input has a file (is not null)
                if (avatarImage != null)
                {
                    //store the new file name
                    string image = avatarImage.FileName;

                    //extract the extension and save it in a variable
                    string ext = image.Substring(image.LastIndexOf("."));//.jpg, .gif, etc.
                    //create a whitelist of valid extensions
                    string[] goodExts = new string[] { ".jpg", ".jpeg", ".gif", ".png" };

                    //check our extension vs the whitelist
                    if (goodExts.Contains(ext.ToLower()))
                    {
                        //if we have a good ext, creat a new Unique file name and add the extension
                        //Global Unique Identifier
                        image = Guid.NewGuid() + ext;
                        //save the new file to the webserver
                        //avatarImage.SaveAs(Server.MapPath("~/Content/Images/Users/" + image));

                        #region to generate full & thumnail images //! using FamilySite.Domain.Services
                        string savePath = Server.MapPath("~/Content/Images/Users/");

                        Image convertedImage = Image.FromStream(avatarImage.InputStream);
                        ImageUtilities.ResizeImage(savePath, image, convertedImage, 500, 100);
                        userDetail.AvatarImage = image;
                        #endregion

                        if (Session["currentImage"].ToString() != "noImage.jpg")
                        {
                            System.IO.File.Delete(Server.MapPath("~/Content/Images/Users/" + Session["currentImage"].ToString()));
                            System.IO.File.Delete(Server.MapPath("~/Content/Images/Users/t_" + Session["currentImage"].ToString()));
                        }
                    }
                         
                }

                //no matter what - add the image value back to the record.
                //save to DB //associating the uploaded file (name) to the model record being added
                #endregion

                db.Entry(userDetail).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            //ViewBag.UserID = new SelectList(db.AspNetUsers, "Id", "Email", userDetail.UserID);
            return View(userDetail);
        }

        // GET: UserDetails/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UserDetail userDetail = db.UserDetails.Find(id);
            if (userDetail == null)
            {
                return HttpNotFound();
            }
            //Handle any delete that would orphon an connected record but this delete function deletes only the profile image.
            //if (userDetail.Reservations.Count > 0 || userDetail.LessonViews.Count >0)
            //{
            //    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            //}
            
            return View(userDetail);
        }

        // POST: UserDetails/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            UserDetail userDetail = db.UserDetails.Find(id);
           
            
            if (userDetail.AvatarImage != "noImage.jpg")
            {
                System.IO.File.Delete(Server.MapPath("~/Content/Images/Users/" + userDetail.AvatarImage));

                //if you had a thumbnail, you would want to delete it as well.
                System.IO.File.Delete(Server.MapPath("~/Content/Images/Users/t_" + userDetail.AvatarImage));
            }
            userDetail.AvatarImage = "noImage.jpg";


            db.Entry(userDetail).State = EntityState.Modified;
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
