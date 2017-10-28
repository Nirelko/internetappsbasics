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
            return View(Db.Users.ToList());
        }

        [AdminRequired]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var client = Db.Users.Find(id);

            if (client == null)
            {
                return HttpNotFound();
            }

            return View(client);
        }

        [AllowAnonymous]
        public ActionResult Create()
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

            var client = Db.Users.Find(id);

            if (client == null)
            {
                return HttpNotFound();
            }

            return View(client);
        }

        [AdminRequired]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var client = Db.Users.Find(id);

            if (client == null)
            {
                return HttpNotFound();
            }

            return View(client);
        }

        [AllowAnonymous]
        public ActionResult Logout()
        {
            Session.Clear();

            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        public ActionResult RecipesLogin()
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
            // join select for users and their recipes
            var query =
                from client in Db.Users
                join recipe in Db.Recipes on client.Id equals recipe.User.Id
                select new UserReviewsViewModel
                {
                    UserName = client.Username,
                    FirstName = client.FirstName,
                    LastName = client.LastName,
                    Title = recipe.Title,
                    Id = client.Id
                };

            return View(query.ToList());
        }       

        #region API

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public ActionResult Create([Bind(Include = "ID,Gender,Username,FirstName,LastName,Password,isAdmin")] User user)
        {
            if (!ModelState.IsValid) return View(user);

            var requestedUser = Db.Users.FirstOrDefault(x => x.Username == user.Username);

            if (requestedUser != null) return View(user);

            Db.Users.Add(user);
            Db.SaveChanges();

            return RedirectToAction("RecipesLogin", "User");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminRequired]
        public ActionResult Edit([Bind(Include = "ID,Gender,Username,FirstName,LastName,Password,isAdmin")] User user)
        {
            if (!ModelState.IsValid) return View(user);

            Db.Entry(user).State = EntityState.Modified;
            Db.SaveChanges();

            return RedirectToAction("Index");
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [AdminRequired]
        public ActionResult DeleteConfirmed(int id)
        {
            var client = Db.Users.Find(id);

            var recipes = Db.Recipes.Where(x => x.User.Id == id).ToList();

            foreach (var currComment in Db.Comments.Where(x => x.User.Id == id).ToList())
            {
                Db.Comments.Remove(currComment);
            }

            foreach (var currRecipe in recipes)
            {
                Db.Recipes.Remove(currRecipe);
            }

            Db.Users.Remove(client);
            Db.SaveChanges();

            if (((User)Session["Client"]).Id == id)
            {
                Session.Clear();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public ActionResult Login([Bind(Include = "Username,Password")] User user)
        {
            var pass = user.Password;
            var logonName = user.Username;

            var requestedClient = Db.Users.SingleOrDefault(u => u.Username.Equals(logonName) && u.Password.Equals(pass));

            if (requestedClient == null)
            {
                return RedirectToAction("FailedLogin", "User");
            }

            Session.Add("Client", requestedClient);

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [AdminRequired]
        public ActionResult Search(string username, string firstname, string lastname)
        {
            var requestedClients = new List<User>();

            foreach (var client in Db.Users)
            {
                if (!string.IsNullOrEmpty(username) && client.Username.Contains(username))
                {
                    requestedClients.Add(client);
                }
                else if (!string.IsNullOrEmpty(firstname) && client.FirstName.Contains(firstname))
                {
                    requestedClients.Add(client);
                }
                else if (!string.IsNullOrEmpty(lastname) && client.LastName.Contains(lastname))
                {
                    requestedClients.Add(client);
                }
            }

            return View(requestedClients.OrderByDescending(x => x.Username));
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult GetGroupByGender() // TODO: Check if its redundent
        {
            var data = Db.Users.GroupBy(x => x.Gender, client => client, (gender, clients) => new
            {
                Name = gender.ToString(),
                Count = clients.Count()
            });

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        #endregion
    }
}
