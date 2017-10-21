using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Recipes.Models;
using Recipes.Models.Db;
using Recipes.ViewModels;

namespace Recipes.Controllers
{
    public class ClientsController : Controller
    {
        private readonly ModelsMapping _db = new ModelsMapping();

        public ActionResult Index()
        {
            if (AuthorizationMiddleware.AdminAuthorized(Session))
            {
                return View(_db.Clients.ToList());
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

            var client = _db.Clients.Find(id);

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
        public ActionResult Create([Bind(Include = "ID,Gender,ClientName,FirstName,LastName,Password,isAdmin")] Client client)
        {
            if (!ModelState.IsValid) return View(client);

            var requestedUser = _db.Clients.FirstOrDefault(x => x.ClientName == client.ClientName);

            if (requestedUser != null) return View(client);

            _db.Clients.Add(client);
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

            var client = _db.Clients.Find(id);

            if (client == null)
            {
                return HttpNotFound();
            }

            return View(client);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,Gender,ClientName,FirstName,LastName,Password,isAdmin")] Client client)
        {
            if (!AuthorizationMiddleware.AdminAuthorized(Session)) return RedirectToAction("Index", "Home");

            if (!ModelState.IsValid) return View(client);

            _db.Entry(client).State = EntityState.Modified;
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

            var client = _db.Clients.Find(id);

            if (client == null)
            {
                return HttpNotFound();
            }

            return View(client);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login([Bind(Include = "ClientName,Password")] Client client)
        {
            var pass = client.Password;
            var logonName = client.ClientName;

            var requestedClient = _db.Clients.SingleOrDefault(u => u.ClientName.Equals(logonName) && u.Password.Equals(pass));

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

            var client = _db.Clients.Find(id);

            var recipes = _db.Recipes.Where(x => x.ClientId == id).ToList();

            foreach (var currComment in _db.Comments.Where(x => x.ClientId == id).ToList())
            {
                _db.Comments.Remove(currComment);
            }

            foreach (var currRecipe in recipes)
            {               
                _db.Recipes.Remove(currRecipe);
            }

            _db.Clients.Remove(client);
            _db.SaveChanges();

            if (((Client)Session["Client"]).Id == id)
            {
                Session.Clear();
            }

            return RedirectToAction("Index");
        }

        public ActionResult Stats()
        {
            // join select for users and their recipes
            var query =
                from client in _db.Clients
                join recipe in _db.Recipes on client.Id equals recipe.ClientId
                select new UserRecipesViewModel
                {
                    UserName = client.ClientName,
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

            var requestedClients = new List<Client>();

            foreach (var client in _db.Clients)
            {
                if (!string.IsNullOrEmpty(username) && client.ClientName.Contains(username))
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

            return View(requestedClients.OrderByDescending(x => x.ClientName));
        }

        [HttpGet]
        public ActionResult GetGroupByGender()
        {
            var data = _db.Clients.GroupBy(x => x.Gender, client => client, (gender, clients) => new
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
