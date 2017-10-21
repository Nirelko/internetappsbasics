using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Recipes.Models;
using Recipes.Models.Db;

namespace Recipes.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ModelsMapping _db = new ModelsMapping();

        // GET: Catgories
        public ActionResult Index()
        {
            if (AuthorizationMiddleware.AdminAuthorized(Session))
            {
                return View(_db.Categories.ToList());
            }

            return RedirectToAction("Index", "Home");
        }

        public ActionResult Details(int? id)
        {
            if (!AuthorizationMiddleware.AdminAuthorized(Session))
            {
                return RedirectToAction("Index", "Home");
            }

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var category = _db.Categories.Find(id);

            if (category == null)
            {
                return HttpNotFound();
            }

            return View(category);
        }

        public ActionResult Create()
        {
            if (AuthorizationMiddleware.AdminAuthorized(Session))
            {
                return View();
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,Name")] Category category)
        {
            if (!AuthorizationMiddleware.AdminAuthorized(Session)) return RedirectToAction("Index", "Home");

            if (!ModelState.IsValid) return View(category);

            var requestedCategory = _db.Categories.FirstOrDefault(x => x.Name == category.Name);

            if (requestedCategory != null) return View(category);

            _db.Categories.Add(category);
            _db.SaveChanges();

            return RedirectToAction("Index");
        }

        public ActionResult Edit(int? id)
        {
            if (!AuthorizationMiddleware.AdminAuthorized(Session)) return RedirectToAction("Index", "Home");

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var category = _db.Categories.Find(id);

            if (category == null)
            {
                return HttpNotFound();
            }

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,Name")] Category category)
        {
            if (!AuthorizationMiddleware.AdminAuthorized(Session)) return RedirectToAction("Index", "Home");

            if (!ModelState.IsValid) return View(category);

            _db.Entry(category).State = EntityState.Modified;
            _db.SaveChanges();

            return RedirectToAction("Index");
        }

        // GET: Categories/Delete/5
        public ActionResult Delete(int? id)
        {
            if (!AuthorizationMiddleware.AdminAuthorized(Session)) return RedirectToAction("Index", "Home");

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var category = _db.Categories.Find(id);

            if (category == null)
            {
                return HttpNotFound();
            }

            return View(category);
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (!AuthorizationMiddleware.AdminAuthorized(Session)) return RedirectToAction("Index", "Home");

            var category = _db.Categories.Find(id);
            var recipes = _db.Recipes.Where(x => x.Category.Id == id).ToList();

            foreach (var currRecipe in recipes)
            {
                var recipe = _db.Recipes.Find(currRecipe.Id);

                var commentsToRemove = _db.Comments.Where(x => x.RecipeId == currRecipe.Id).ToList();
                    
                foreach (var currComment in commentsToRemove)
                {
                    _db.Comments.Remove(currComment);
                }

                _db.Recipes.Remove(recipe);
            }

            _db.Categories.Remove(category);
            _db.SaveChanges();

            return RedirectToAction("Index");
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
