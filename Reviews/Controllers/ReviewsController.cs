using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Reviews.Models;
using Reviews.ViewModels;

namespace Reviews.Controllers
{
    public class ReviewsController : BaseController
    {
        public IQueryable<Review> LoadedQueryableReviewses { get; set; }
        public ReviewsController()
        {
            LoadedQueryableReviewses = Db.Reviews.Include(p => p.User).Include(p => p.Category);
        }

        [AllowAnonymous]
        public ActionResult Index()
        {
            ViewBag.RecommendedReview = GetRecommendedReview(LoadedQueryableReviewses);

            return View(LoadedQueryableReviewses.ToList());
        }

        /// <summary>
        /// Returns the most commented review in the most reviewed category by the user
        /// </summary>
        /// <param name="reviews"></param>
        /// <returns></returns>
        private Review GetRecommendedReview(IQueryable<Review> reviews)
        {
            var currentUser = (User)Session["User"];
            if (currentUser == null)
            {
                return null;
            }

            var currentUserReviews  = reviews.Where(x => x.UserID == currentUser.Id).ToList();
            if (!currentUserReviews.Any())
            {
                return null;
            }

            var userMostReviewedCategory = currentUserReviews
                .GroupBy(x => x.Category)
                .OrderByDescending(x => x.Key.Reviews.Count(review => review.User.Id == currentUser.Id))
                .First().Key;

            return reviews
                .Where(x => x.Category.Id == userMostReviewedCategory.Id)
                .OrderByDescending(x => x.Comments.Count)
                .FirstOrDefault();
        }

        [AllowAnonymous]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var review = Db.Reviews.Find(id);
            if (review == null)
            {
                return HttpNotFound();
            }

            return View(review);
        }

        [AllowAnonymous]
        public ActionResult RecommendedReview()
        {
            var recommendedReview = GetRecommendedReview(LoadedQueryableReviewses);
            if (recommendedReview == null)
            {
                return HttpNotFound();
            }

            return View("Details", recommendedReview);
        }

        public ActionResult Create()
        {
            ViewBag.UserID = new SelectList(Db.Users, "ID", "Username");
            ViewBag.CategoryID = new SelectList(Db.Categories, "ID", "Name");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,userId,CategoryID,Title,Content")] Review review)
        {
            if (review.Content == null || review.Title == null || review.CategoryID == 0)
            {
                return RedirectToAction("Index", "Home");
            }

            if (!ModelState.IsValid)
            {
                ReloadViewBag(review);

                return View(review);
            }

            review.CreationDate = DateTime.Now;
            Db.Reviews.Add(review);
            Db.SaveChanges();

            return RedirectToAction("Index");
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var review = Db.Reviews.Find(id);
            if (review == null)
            {
                return HttpNotFound();
            }

            ReloadViewBag(review);

            return View(review);
        }

        private void ReloadViewBag(Review review)
        {
            ViewBag.UserID = new SelectList(Db.Users, "ID", "Username", review.UserID);
            ViewBag.CategoryID = new SelectList(Db.Categories, "ID", "Name", review.CategoryID);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,userId,CategoryID,Title,Content")] Review review)
        {
            if (review.Content == null || review.Title == null)
            {
                return RedirectToAction("Index", "Home");
            }

            if (!ModelState.IsValid)
            {
                ReloadViewBag(review);

                return View(review);
            }

            review.CreationDate = DateTime.Now;
            Db.Entry(review).State = EntityState.Modified;
            Db.SaveChanges();

            return RedirectToAction("Index");
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var review = Db.Reviews.Find(id);
            if (review == null)
            {
                return HttpNotFound();
            }

            return View(review);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var review = Db.Reviews.Find(id);
            if (review == null)
            {
                return HttpNotFound();
            }

            Db.Comments.Where(x => x.Review.Id == id).ToList().ForEach(x => Db.Comments.Remove(x));

            Db.Reviews.Remove(review);

            Db.SaveChanges();

            return RedirectToAction("Index");
        }

        public ActionResult PostComment(int userId, int reviewId, string content)
        {
            Db.Comments.Add(new Comment
            {
                Content = content,
                UserID = userId,
                ReviewID = reviewId,
                CreationDate = DateTime.Now
            });

            Db.SaveChanges();

            return RedirectToAction("Index");
        }

        [AllowAnonymous]
        public ActionResult Stats()
        {
            var reviewCommentViewModels =
                from review in Db.Reviews
                join user in Db.Users on review.User.Id equals user.Id
                select new ReviewCommentViewModel
                {
                    Title = review.Title,
                    NumberOfComment = review.Comments.Count,
                    AuthorFullName = user.FirstName + " " + user.LastName
                };

            return View(reviewCommentViewModels.ToList());
        }

        [AllowAnonymous]
        public ActionResult StatsJson()
        {
            var reviewCommentViewModels =
                from review in Db.Reviews
                join user in Db.Users on review.User.Id equals user.Id
                select new ReviewCommentViewModel
                {
                    Title = review.Title,
                    NumberOfComment = review.Comments.Count,
                    AuthorFullName = user.FirstName + " " + user.LastName
                };

            return Json(reviewCommentViewModels.ToList(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult Search(string content, string title, DateTime? date)
        {
            var dayAfterDate = date?.AddDays(1);

            return View(Db.Reviews
                .Where(review =>
                    (!string.IsNullOrEmpty(content) && review.Content.ToLower().Contains(content.ToLower())) ||
                    (!string.IsNullOrEmpty(title) && review.Title.ToLower().Contains(title.ToLower())) ||
                    (date.HasValue && dayAfterDate.HasValue && date < review.CreationDate && review.CreationDate < dayAfterDate))
                .OrderByDescending(x => x.CreationDate)
                .ToList());
        }
    }
}
