using System.Web;
using Reviews.Models;

namespace Reviews.Controllers
{
    public static class AuthorizationMiddleware
    {
        public static bool AdminAuthorized(HttpSessionStateBase session)
        {
            return Authorized(session) && ((Client)session["Client"]).IsAdmin;
        }

        public static bool Authorized(HttpSessionStateBase session)
        {
            return (Client)session["Client"] != null;
        }
    }
}