using System.Web;
using System.Web.Mvc;
using Reviews.Attributes;
using Reviews.Models;

namespace Reviews.Filters
{
    public class ReviewsAuthorizeFilter : AuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (!(filterContext.Controller is Controller))
            {
                return;
            }

            var controller = (Controller)filterContext.Controller;

            AuthorizeController(filterContext, controller);
        }

        private void AuthorizeController(AuthorizationContext filterContext, Controller controller)
        {
            if (filterContext.ActionDescriptor.ControllerDescriptor.IsDefined(typeof(AnonymousControllerRequired), true))
            {
                AuthorizeAction(filterContext, controller, true);
                return;
            }

            if (filterContext.ActionDescriptor.ControllerDescriptor.IsDefined(typeof(AdminRequiredAttribute), true) &&
                !AdminAuthorized(controller.Session))
            {
                HandleUnauthorizedRequest(filterContext);
            }

            AuthorizeAction(filterContext, controller, false);
        }

        private void AuthorizeAction(AuthorizationContext filterContext, Controller controller, bool isAnonymousController)
        {
            if (filterContext.ActionDescriptor.IsDefined(typeof(AllowAnonymousAttribute), true))
            {
                return;
            }

            if (filterContext.ActionDescriptor.IsDefined(typeof(AdminRequiredAttribute), true) && !AdminAuthorized(controller.Session))
            {
                HandleUnauthorizedRequest(filterContext);
            }

            if ((!isAnonymousController || filterContext.ActionDescriptor.IsDefined(typeof(LoginRequiredAttribute), true)) &&
                !Authorized(controller.Session))
            {
                HandleUnauthorizedRequest(filterContext);
            }
        }

        private bool AdminAuthorized(HttpSessionStateBase session)
        {
            return Authorized(session) && ((User)session["User"]).IsAdmin;
        }

        private bool Authorized(HttpSessionStateBase session)
        {
            return (User)session["User"] != null;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.HttpContext.Response.Redirect("~/");
        }
    }
}