using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Recipes.Models;
using Recipes.Models.Db;

namespace Recipes.Controllers
{
    public class CommentsController : Controller
    {
        private readonly ModelsMapping _db = new ModelsMapping();

        public ActionResult Index()
        {
            var comments = _db.Comments.Include(c => c.Client).Include(c => c.Recipe);

            return View(comments.ToList());
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var comment = _db.Comments.Find(id);

            if (comment == null)
            {
                return HttpNotFound();
            }

            return View(comment);
        }

        public ActionResult Create()
        {
            if (!AuthorizationMiddleware.Authorized(Session)) return RedirectToAction("Index", "Home");

            ViewBag.ClientID = new SelectList(_db.Clients, "ID", "ClientName");
            ViewBag.RecipeID = new SelectList(_db.Recipes, "ID", "Content");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,ClientID,RecipeID,Content,CreationDate")] Comment comment)
        {
            if (!AuthorizationMiddleware.Authorized(Session)) return RedirectToAction("Index", "Home");

            if (ModelState.IsValid)
            {
                _db.Comments.Add(comment);
                _db.SaveChanges();

                return RedirectToAction("Index");
            }

            ViewBag.ClientID = new SelectList(_db.Clients, "ID", "ClientName", comment.ClientId);
            ViewBag.RecipeID = new SelectList(_db.Recipes, "ID", "Content", comment.RecipeId);

            return View(comment);
        }

        public ActionResult Edit(int? id)
        {
            if (!AuthorizationMiddleware.Authorized(Session)) return RedirectToAction("Index", "Home");

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var comment = _db.Comments.Find(id);

            if (comment == null)
            {
                return HttpNotFound();
            }

            ViewBag.ClientID = new SelectList(_db.Clients, "ID", "ClientName", comment.ClientId);
            ViewBag.RecipeID = new SelectList(_db.Recipes, "ID", "Content", comment.RecipeId);

            return View(comment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,ClientID,RecipeID,Content,CreationDate")] Comment comment)
        {
            if (!AuthorizationMiddleware.Authorized(Session)) return RedirectToAction("Index", "Home");

            if (ModelState.IsValid)
            {
                _db.Entry(comment).State = EntityState.Modified;
                _db.SaveChanges();

                return RedirectToAction("Index");
            }

            ViewBag.ClientID = new SelectList(_db.Clients, "ID", "ClientName", comment.ClientId);
            ViewBag.RecipeID = new SelectList(_db.Recipes, "ID", "Content", comment.RecipeId);

            return View(comment);
        }

        public ActionResult Delete(int? id)
        {
            if (!AuthorizationMiddleware.Authorized(Session)) return RedirectToAction("Index", "Home");

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var comment = _db.Comments.Find(id);

            if (comment == null)
            {
                return HttpNotFound();
            }

            return View(comment);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (!AuthorizationMiddleware.Authorized(Session)) return RedirectToAction("Index", "Home");

            var comment = _db.Comments.Find(id);

            _db.Comments.Remove(comment);
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
