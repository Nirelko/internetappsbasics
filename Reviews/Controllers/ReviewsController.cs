﻿using System;
using System.Collections.Generic;
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
        [AllowAnonymous]
        public ActionResult Index()
        {
            var recipes = Db.Reviews.Include(p => p.User).Include(p => p.Category);

            ViewBag.RecommendedRecipe = GetRecommendedRecipe(recipes);

            return View(recipes.ToList());
        }

        private Review GetRecommendedRecipe(IQueryable<Review> allRecipes)
        {
            var currentUser = (User)Session["User"];

            if (currentUser == null) return null;

            var currentUserRecipes = Db.Users.Where(x => x.Id == currentUser.Id).Include(x => x.Reviews).SingleOrDefault()?.Reviews;

            if (currentUserRecipes == null || !currentUserRecipes.Any()) return null;

            // Find the food category in which the current user wrote most of his recipes, and then get the
            // review with the biggest number of comments in this category and display it to the current user
            // as his recommended review
            Category currUserTopCategory = currentUserRecipes
                .GroupBy(x => x.Category)
                .OrderByDescending(x => x.Key.Reviews.Count(recipe => recipe.User.Id == currentUser.Id))
                .FirstOrDefault()?.Key;

            return allRecipes
                .Where(x => x.Category.Id == currUserTopCategory.Id)
                .OrderByDescending(x => x.Comments.Count)
                .FirstOrDefault(); ;
        }

        [AllowAnonymous]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var recipe = Db.Reviews.Find(id);

            if (recipe == null)
            {
                return HttpNotFound();
            }

            return View(recipe);
        }

        [AllowAnonymous]
        public ActionResult RecommendedRecipeDetails()
        {
            var recipes = Db.Reviews.Include(p => p.User).Include(p => p.Category);
            var recommendedRecipe = GetRecommendedRecipe(recipes);

            if (recommendedRecipe == null)
            {
                return HttpNotFound();
            }

            return View("Details", recommendedRecipe);
        }

        [AllowAnonymous]
        public ActionResult DetailsByTitle(string title)
        {
            if (title == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var recipe = Db.Reviews.FirstOrDefault(x => x.Title == title);

            if (recipe == null)
            {
                return HttpNotFound();
            }

            return View("Details", recipe);
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

            if (ModelState.IsValid)
            {
                review.CreationDate = DateTime.Now;
                Db.Reviews.Add(review);
                Db.SaveChanges();

                return RedirectToAction("Index");
            }

            ReloadViewBag(review);

            return View(review);
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

            if (ModelState.IsValid)
            {
                review.CreationDate = DateTime.Now;
                Db.Entry(review).State = EntityState.Modified;
                Db.SaveChanges();

                return RedirectToAction("Index");
            }

            ReloadViewBag(review);

            return View(review);
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
            var commentsToRemove = Db.Comments.Where(x => x.Review.Id == id).ToList();

            foreach (var commentToRemove in commentsToRemove)
            {
                var comment = Db.Comments.Find(commentToRemove.Id);
                Db.Comments.Remove(comment);
            }

            Db.Reviews.Remove(review);
            Db.SaveChanges();

            return RedirectToAction("Index");
        }

        public ActionResult PostComment(int userId, int recipeId, string content)
        {
//            var comment = new Comment
//            {
//                Content = content,
//                UserId = userId,
//                RecipeId = recipeId,
//                CreationDate = DateTime.Now
//            };
//
//            Db.Comments.Add(comment);
//            Db.SaveChanges();
//
//            return RedirectToAction("Index");
            return null;
        }

        [AllowAnonymous]
        public ActionResult Stats()
        {
            var query =
                from review in Db.Reviews
                join user in Db.Users on review.User.Id equals user.Id
                select new ReviewCommentViewModel
                {
                    Title = review.Title,
                    NumberOfComment = review.Comments.Count,
                    AuthorFullName = user.FirstName + " " + user.LastName
                };

            return View(query.ToList());
        }

        [AllowAnonymous]
        public ActionResult StatsJson()
        {
            var query =
                from review in Db.Reviews
                join user in Db.Users on review.User.Id equals user.Id
                select new ReviewCommentViewModel
                {
                    Title = review.Title,
                    NumberOfComment = review.Comments.Count,
                    AuthorFullName = user.FirstName + " " + user.LastName
                };

            var data = Json(query.ToList(), JsonRequestBehavior.AllowGet);

            return data;
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult Search(string content, string title, DateTime? date)
        {
            var queryReviews = new List<Review>();

            foreach (var review in Db.Reviews)
            {
                if (!string.IsNullOrEmpty(content) && review.Content.ToLower().Contains(content.ToLower()))
                {
                    queryReviews.Add(review);
                }
                else if (!string.IsNullOrEmpty(title) && review.Title.ToLower().Contains(title.ToLower()))
                {
                    queryReviews.Add(review);
                }
                else if (date != null)
                {
                    var formattedDateRecipe = review.CreationDate.ToString("MM/dd/yyyy");
                    var formattedDate = date.Value.ToString("MM/dd/yyyy");

                    if (formattedDateRecipe.Equals(formattedDate))
                    {
                        queryReviews.Add(review);
                    }
                }
            }

            return View(queryReviews.OrderByDescending(x => x.CreationDate));
        }
    }
}
