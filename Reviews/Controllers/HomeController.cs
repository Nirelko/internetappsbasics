using Reviews.Models;
using System.Linq;
using System.Web.Mvc;

namespace Reviews.Controllers
{
    public class HomeController : BaseController
    {
        [AllowAnonymous]
        public ActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult Contact()
        {

            if (!Db.Locations.Any())
            {
                Db.Locations.Add(new Location { Address = "450 Serra Mall, Stanford, CA 94305, USA" });
                Db.SaveChanges();
            }

            ViewBag.Location = Db.Locations.First().Address;

            return View();
        }
    }
}