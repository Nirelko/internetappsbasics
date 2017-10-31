using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Reviews.Attributes;
using Reviews.Models;
using WebGrease.Css.Extensions;

namespace Reviews.Controllers
{
    [AdminRequired]
    public class CategoriesController : BaseController
    {
        public ActionResult Index()
        {
            return View(Db.Categories.ToList());
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var category = Db.Categories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }

            return View(category);
        }

        public ActionResult Create()
        {
                return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,Name")] Category category)
        {
            if (!ModelState.IsValid || Db.Categories.Any(x => x.Name == category.Name))
            {
                return View(category);
            }

            Db.Categories.Add(category);
            Db.SaveChanges();

            return RedirectToAction("Index");
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var category = Db.Categories.Find(id);
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
            if (!ModelState.IsValid)
            {
                return View(category);
            }

            Db.Entry(category).State = EntityState.Modified;
            Db.SaveChanges();

            return RedirectToAction("Index");
        }

        // GET: Categories/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var category = Db.Categories.Find(id);
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
            var category = Db.Categories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }

            RemoveCategoryReviews(id);

            Db.Categories.Remove(category);

            Db.SaveChanges();

            return RedirectToAction("Index");
        }

        private void RemoveCategoryReviews(int id)
        {
            var categoryReviews = Db.Reviews.Where(x => x.Category.Id == id).ToList();

            RemoveCategoryReviewsComments(categoryReviews);

            categoryReviews.ForEach(x => Db.Reviews.Remove(x));
        }

        private void RemoveCategoryReviewsComments(IEnumerable<Review> categoryReviews)
        {
            var categoryReviewsIds = categoryReviews.Select(x => x.Id);
            Db.Comments
                .Where(x => categoryReviewsIds.Any(y => y == x.Review.Id))
                .ForEach(x => Db.Comments.Remove(x));
        }
    }
}
