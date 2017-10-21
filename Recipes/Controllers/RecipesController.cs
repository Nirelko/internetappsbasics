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
    public class RecipesController : Controller
    {
        private readonly ModelsMapping _db = new ModelsMapping();

        public ActionResult Index()
        {
            var recipes = _db.Recipes.Include(p => p.Client).Include(p => p.Category);

            ViewBag.RecommendedRecipe = GetRecommendedRecipe(recipes);

            return View(recipes.ToList());
        }

        private Recipe GetRecommendedRecipe(IQueryable<Recipe> allRecipes)
        {
            var currentUser = (Client)Session["Client"];

            if (currentUser == null) return null;

            var currentUserRecipes = _db.Clients.Where(x => x.Id == currentUser.Id).Include(x => x.Recipes).SingleOrDefault()?.Recipes;

            if (currentUserRecipes == null || !currentUserRecipes.Any()) return null;

            // Find the food category in which the current user wrote most of his recipes, and then get the
            // recipe with the biggest number of comments in this category and display it to the current user
            // as his recommended recipe
            Category currUserTopCategory = currentUserRecipes
                .GroupBy(x => x.Category)
                .OrderByDescending(x => x.Key.Recipes.Count(recipe => recipe.ClientId == currentUser.Id))
                .FirstOrDefault()?.Key;

            return allRecipes
                .Where(x => x.Category.Id == currUserTopCategory.Id)
                .OrderByDescending(x => x.Comments.Count)
                .FirstOrDefault(); ;
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var recipe = _db.Recipes.Find(id);

            if (recipe == null)
            {
                return HttpNotFound();
            }

            return View(recipe);
        }

        public ActionResult RecommendedRecipeDetails()
        {
            var recipes = _db.Recipes.Include(p => p.Client).Include(p => p.Category);
            var recommendedRecipe = GetRecommendedRecipe(recipes);

            if (recommendedRecipe == null)
            {
                return HttpNotFound();
            }

            return View("Details", recommendedRecipe);
        }

        public ActionResult DetailsByTitle(string title)
        {
            if (title == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var recipe = _db.Recipes.FirstOrDefault(x => x.Title == title);

            if (recipe == null)
            {
                return HttpNotFound();
            }

            return View("Details", recipe);
        }

        public ActionResult Create()
        {
            if (!AuthorizationMiddleware.Authorized(Session)) return RedirectToAction("Index", "Home");

            ViewBag.ClientID = new SelectList(_db.Clients, "ID", "ClientName");
            ViewBag.CategoryID = new SelectList(_db.Categories, "ID", "Name");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,clientId,CategoryID,Title,Content")] Recipe recipe)
        {
            if (recipe.Content == null || recipe.Title == null || recipe.CategoryId == 0)
            {
                return RedirectToAction("Index", "Home");
            }

            if (!AuthorizationMiddleware.Authorized(Session)) return RedirectToAction("Index", "Home");

            if (ModelState.IsValid)
            {
                recipe.CreationDate = DateTime.Now;
                _db.Recipes.Add(recipe);
                _db.SaveChanges();

                return RedirectToAction("Index");
            }

            ViewBag.ClientID = new SelectList(_db.Clients, "ID", "ClientName", recipe.ClientId);
            ViewBag.CategoryID = new SelectList(_db.Categories, "ID", "Name", recipe.CategoryId);

            return View(recipe);
        }

        public ActionResult Edit(int? id)
        {
            if (!AuthorizationMiddleware.Authorized(Session)) return RedirectToAction("Index", "Home");

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var recipe = _db.Recipes.Find(id);

            if (recipe == null)
            {
                return HttpNotFound();
            }

            ViewBag.ClientID = new SelectList(_db.Clients, "ID", "ClientName", recipe.ClientId);
            ViewBag.CategoryID = new SelectList(_db.Categories, "ID", "Name", recipe.CategoryId);

            return View(recipe);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,clientId,CategoryID,Title,Content")] Recipe recipe)
        {
            if (recipe.Content == null || recipe.Title == null)
            {
                return RedirectToAction("Index", "Home");
            }

            if (!AuthorizationMiddleware.Authorized(Session)) return RedirectToAction("Index", "Home");

            if (ModelState.IsValid)
            {
                recipe.CreationDate = DateTime.Now;
                _db.Entry(recipe).State = EntityState.Modified;
                _db.SaveChanges();

                return RedirectToAction("Index");
            }

            ViewBag.ClientID = new SelectList(_db.Clients, "ID", "ClientName", recipe.ClientId);
            ViewBag.CategoryID = new SelectList(_db.Categories, "ID", "Name", recipe.CategoryId);

            return View(recipe);
        }

        public ActionResult Delete(int? id)
        {
            if (!AuthorizationMiddleware.Authorized(Session)) return RedirectToAction("Index", "Home");

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var recipe = _db.Recipes.Find(id);

            if (recipe == null)
            {
                return HttpNotFound();
            }

            return View(recipe);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (!AuthorizationMiddleware.Authorized(Session)) return RedirectToAction("Index", "Home");

            var recipe = _db.Recipes.Find(id);
            var commentsToRemove = _db.Comments.Where(x => x.Recipe.Id == id).ToList();

            foreach (var commentToRemove in commentsToRemove)
            {
                var comment = _db.Comments.Find(commentToRemove.Id);
                _db.Comments.Remove(comment);
            }

            _db.Recipes.Remove(recipe);
            _db.SaveChanges();

            return RedirectToAction("Index");
        }

        public ActionResult PostComment(int clientId, int recipeId, string content)
        {
            if (!AuthorizationMiddleware.Authorized(Session)) return RedirectToAction("Index", "Home");

            var comment = new Comment
            {
                Content = content,
                ClientId = clientId,
                RecipeId = recipeId,
                CreationDate = DateTime.Now
            };

            _db.Comments.Add(comment);
            _db.SaveChanges();

            return RedirectToAction("Index");
        }

        public ActionResult Stats()
        {
            var query =
                from recipe in _db.Recipes
                join client in _db.Clients on recipe.ClientId equals client.Id
                select new RecipeCommentViewModel
                {
                    Title = recipe.Title,
                    NumberOfComment = recipe.Comments.Count,
                    AuthorFullName = client.FirstName + " " + client.LastName
                };

            return View(query.ToList());
        }

        public ActionResult StatsJson()
        {
            var query =
                from recipe in _db.Recipes
                join client in _db.Clients on recipe.ClientId equals client.Id
                select new RecipeCommentViewModel
                {
                    Title = recipe.Title,
                    NumberOfComment = recipe.Comments.Count,
                    AuthorFullName = client.FirstName + " " + client.LastName
                };

            var data = Json(query.ToList(), JsonRequestBehavior.AllowGet);

            return data;
        }

        [HttpGet]
        public ActionResult Search(string content, string title, DateTime? date)
        {
            var queryRecipes = new List<Recipe>();

            foreach (var recipe in _db.Recipes)
            {
                if (!string.IsNullOrEmpty(content) && recipe.Content.ToLower().Contains(content.ToLower()))
                {
                    queryRecipes.Add(recipe);
                }
                else if (!string.IsNullOrEmpty(title) && recipe.Title.ToLower().Contains(title.ToLower()))
                {
                    queryRecipes.Add(recipe);
                }
                else if (date != null)
                {
                    var formattedDateRecipe = recipe.CreationDate.ToString("MM/dd/yyyy");
                    var formattedDate = date.Value.ToString("MM/dd/yyyy");

                    if (formattedDateRecipe.Equals(formattedDate))
                    {
                        queryRecipes.Add(recipe);
                    }
                }
            }

            return View(queryRecipes.OrderByDescending(x => x.CreationDate));
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
