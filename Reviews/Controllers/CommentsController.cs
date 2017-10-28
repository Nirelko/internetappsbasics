using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Reviews.Models;

namespace Reviews.Controllers
{
    public class CommentsController : BaseController
    {
        [AllowAnonymous]
        public ActionResult Index()
        {
            var comments = Db.Comments.Include(c => c.User).Include(c => c.Review);

            return View(comments.ToList());
        }

        [AllowAnonymous]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var comment = Db.Comments.Find(id);

            if (comment == null)
            {
                return HttpNotFound();
            }

            return View(comment);
        }

        public ActionResult Create()
        {
            ViewBag.UserID = new SelectList(Db.Users, "ID", "Username");
            ViewBag.RecipeID = new SelectList(Db.Reviews, "ID", "Content");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,UserID,RecipeID,Content,CreationDate")] Comment comment)
        {
            if (ModelState.IsValid)
            {
                Db.Comments.Add(comment);
                Db.SaveChanges();

                return RedirectToAction("Index");
            }

            ViewBag.UserID = new SelectList(Db.Users, "ID", "Username", comment.User.Id);
            ViewBag.RecipeID = new SelectList(Db.Reviews, "ID", "Content", comment.Review.Id);

            return View(comment);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var comment = Db.Comments.Find(id);

            if (comment == null)
            {
                return HttpNotFound();
            }

            ViewBag.UserID = new SelectList(Db.Users, "ID", "Username", comment.User.Id);
            ViewBag.RecipeID = new SelectList(Db.Reviews, "ID", "Content", comment.User.Id);

            return View(comment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,UserID,RecipeID,Content,CreationDate")] Comment comment)
        {
            if (ModelState.IsValid)
            {
                Db.Entry(comment).State = EntityState.Modified;
                Db.SaveChanges();

                return RedirectToAction("Index");
            }

            ViewBag.UserID = new SelectList(Db.Users, "ID", "Username", comment.User.Id);
            ViewBag.RecipeID = new SelectList(Db.Reviews, "ID", "Content", comment.Review.Id);

            return View(comment);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var comment = Db.Comments.Find(id);

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
            var comment = Db.Comments.Find(id);

            Db.Comments.Remove(comment);
            Db.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}
