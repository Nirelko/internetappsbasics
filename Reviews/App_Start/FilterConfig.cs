using System.Web.Mvc;
using Reviews.Attributes;
using Reviews.Filters;

namespace Reviews
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new ReviewsAuthorizeFilter());
        }
    }
}
