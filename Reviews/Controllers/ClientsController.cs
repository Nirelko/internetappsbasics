using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Reviews.Models;
using Reviews.Models.Db;
using Reviews.ViewModels;

namespace Reviews.Controllers
{
    public class ClientsController : Controller
    {
        private readonly ModelsMapping _db = new ModelsMapping();

        public ActionResult Index()
        {
            if (AuthorizationMiddleware.AdminAuthorized(Session))
            {
                return View(_db.Users.ToList());
            }

            return RedirectToAction("Index", "Home");
        }

        public ActionResult Details(int? id)
        {
            if (!AuthorizationMiddleware.AdminAuthorized(Session)) return RedirectToAction("Index", "Home");

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var client = _db.Users.Find(id);

            if (client == null)
            {
                return HttpNotFound();
            }

            return View(client);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,Gender,Username,FirstName,LastName,Password,isAdmin")] User user)
        {
            if (!ModelState.IsValid) return View(user);

            var requestedUser = _db.Users.FirstOrDefault(x => x.Username == user.Username);

            if (requestedUser != null) return View(user);

            _db.Users.Add(user);
            _db.SaveChanges();

            return RedirectToAction("RecipesLogin", "Clients");
        }

        public ActionResult Edit(int? id)
        {
            if (!AuthorizationMiddleware.AdminAuthorized(Session)) return RedirectToAction("Index", "Home");

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var client = _db.Users.Find(id);

            if (client == null)
            {
                return HttpNotFound();
            }

            return View(client);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,Gender,Username,FirstName,LastName,Password,isAdmin")] User user)
        {
            if (!AuthorizationMiddleware.AdminAuthorized(Session)) return RedirectToAction("Index", "Home");

            if (!ModelState.IsValid) return View(user);

            _db.Entry(user).State = EntityState.Modified;
            _db.SaveChanges();

            return RedirectToAction("Index");
        }

        public ActionResult Delete(int? id)
        {
            if (!AuthorizationMiddleware.AdminAuthorized(Session)) return RedirectToAction("Index", "Home");

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var client = _db.Users.Find(id);

            if (client == null)
            {
                return HttpNotFound();
            }

            return View(client);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login([Bind(Include = "Username,Password")] User user)
        {
            var pass = user.Password;
            var logonName = user.Username;

            var requestedClient = _db.Users.SingleOrDefault(u => u.Username.Equals(logonName) && u.Password.Equals(pass));

            if (requestedClient == null)
            {
                return RedirectToAction("FailedLogin", "Clients");
            }

            Session.Add("Client", requestedClient);

            return RedirectToAction("Index", "Home");
        }

        public ActionResult Logout()
        {
            Session.Clear();

            return RedirectToAction("Index", "Home");
        }

        public ActionResult RecipesLogin()
        {
            return View();
        }

        public ActionResult FailedLogin()
        {
            return View();
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (!AuthorizationMiddleware.AdminAuthorized(Session)) return RedirectToAction("Index", "Home");

            var client = _db.Users.Find(id);

            var recipes = _db.Recipes.Where(x => x.ClientId == id).ToList();

            foreach (var currComment in _db.Comments.Where(x => x.ClientId == id).ToList())
            {
                _db.Comments.Remove(currComment);
            }

            foreach (var currRecipe in recipes)
            {               
                _db.Recipes.Remove(currRecipe);
            }

            _db.Users.Remove(client);
            _db.SaveChanges();

            if (((User)Session["Client"]).Id == id)
            {
                Session.Clear();
            }

            return RedirectToAction("Index");
        }

        public ActionResult Stats()
        {
            // join select for users and their recipes
            var query =
                from client in _db.Users
                join recipe in _db.Recipes on client.Id equals recipe.ClientId
                select new UserRecipesViewModel
                {
                    UserName = client.Username,
                    FirstName = client.FirstName,
                    LastName = client.LastName,
                    Title = recipe.Title,
                    Id = client.Id
                };

            return View(query.ToList());
        }       

        [HttpGet]
        public ActionResult Search(string username, string firstname, string lastname)
        {
            if (!AuthorizationMiddleware.AdminAuthorized(Session)) return RedirectToAction("Index", "Home");

            var requestedClients = new List<User>();

            foreach (var client in _db.Users)
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
        public ActionResult GetGroupByGender()
        {
            var data = _db.Users.GroupBy(x => x.Gender, client => client, (gender, clients) => new
            {
                Name = gender.ToString(),
                Count = clients.Count()
            });

            return Json(data, JsonRequestBehavior.AllowGet);
        }

         protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
