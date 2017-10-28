using System.Web.Mvc;
using Reviews.Models.Db;

namespace Reviews.Controllers
{
    public abstract class BaseController : Controller
    {
        public ModelsMapping Db { get; }

        protected BaseController()
        {
            Db = new ModelsMapping();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Db.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}