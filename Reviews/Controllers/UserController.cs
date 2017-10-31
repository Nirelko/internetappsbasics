using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Reviews.Attributes;
using Reviews.Models;
using Reviews.ViewModels;

namespace Reviews.Controllers
{
    public class UserController : BaseController
    {
        [AdminRequired]
        public ActionResult Index()
        {
            return View(Db.Users.OrderByDescending(x => x.Username).ToList());
        }

        [AdminRequired]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var user = Db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            return View(user);
        }

        [AllowAnonymous]
        public ActionResult Create()
        {
            return View();
        }

        [AdminRequired]
        public ActionResult Add()
        {
            return View();
        }

        [AdminRequired]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var user = Db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            return View(user);
        }

        [AdminRequired]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var user = Db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            return View(user);
        }

        [AllowAnonymous]
        public ActionResult Logout()
        {
            Session.Clear();

            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        public ActionResult Login()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult FailedLogin()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult Stats()
        {
            var userReviewsViewModels =
                from user in Db.Users
                join review in Db.Reviews on user.Id equals review.User.Id
                select new UserReviewsViewModel
                {
                    UserName = user.Username,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Title = review.Title,
                    Id = user.Id
                };

            return View(userReviewsViewModels.ToList());
        }       

        #region API

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public ActionResult Create([Bind(Include = "ID,Gender,Username,FirstName,LastName,Password,isAdmin")] User user)
        {
            var oRegisterRedirect = SaveUser(user, RedirectToAction("Index", "Home"));

            Session.Add("User", user);

            return oRegisterRedirect;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminRequired]
        public ActionResult Add([Bind(Include = "ID,Gender,Username,FirstName,LastName,Password,isAdmin")] User user)
        {
            return SaveUser(user, RedirectToAction("Index", "User"));
        }

        private ActionResult SaveUser(User user, RedirectToRouteResult redirectOnSucess)
        {
            if (!ModelState.IsValid || Db.Users.Any(x => x.Username == user.Username))
            {
                return View(user);
            }

            Db.Users.Add(user);
            Db.SaveChanges();

            return redirectOnSucess;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminRequired]
        public ActionResult Edit([Bind(Include = "ID,Gender,Username,FirstName,LastName,Password,isAdmin")] User user)
        {
            if (!ModelState.IsValid)
            {
                return View(user);
            }

            Db.Entry(user).State = EntityState.Modified;
            Db.SaveChanges();

            return RedirectToAction("Index");
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [AdminRequired]
        public ActionResult DeleteConfirmed(int id)
        {
            var user = Db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            RemoveLinkedReviews(id);

            Db.Users.Remove(user);
            Db.SaveChanges();

            if (((User)Session["User"]).Id == id)
            {
                Session.Clear();
            }

            return RedirectToAction("Index");
        }

        private void RemoveLinkedReviews(int userId)
        {
            RemoveLinkedReviewsComments(userId);

            Db.Reviews.Where(x => x.User.Id == userId).ToList().ForEach(x => Db.Reviews.Remove(x));
        }

        private void RemoveLinkedReviewsComments(int userId)
        {
            Db.Comments.Where(x => x.User.Id == userId).ToList().ForEach(x => Db.Comments.Remove(x));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public ActionResult Login([Bind(Include = "Username,Password")] User loginCredentials)
        {
            var user = Db.Users.SingleOrDefault(u => u.Username.Equals(loginCredentials.Username) && u.Password.Equals(loginCredentials.Password));
            if (user == null)
            {
                return RedirectToAction("FailedLogin", "User");
            }

            Session.Add("User", user);

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [AdminRequired]
        public ActionResult Search(string username, string firstname, string lastname)
        {
            if (string.IsNullOrEmpty(username) && string.IsNullOrEmpty(firstname) && string.IsNullOrEmpty(lastname))
            {
                return View(Db.Users.ToList().OrderByDescending(x => x.Username));
            }

            return View(Db.Users.Where(user =>
                (!string.IsNullOrEmpty(username) && user.Username.Contains(username)) ||
                (!string.IsNullOrEmpty(firstname) && user.FirstName.Contains(firstname)) ||
                (!string.IsNullOrEmpty(lastname) && user.LastName.Contains(lastname))
            ).OrderByDescending(x => x.Username));
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult GetGroupByGender()
        {
            var genderToUsers = Db.Users.GroupBy(x => x.Gender, user => user, (gender, users) => new
            {
                Name = gender.ToString(),
                Count = users.Count()
            });

            return Json(genderToUsers, JsonRequestBehavior.AllowGet);
        }

        #endregion
    }
}
