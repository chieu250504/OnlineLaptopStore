using System.Linq;
using System.Web;
using System.Web.Mvc;
using Laptop88_3.Models;

namespace Laptop88_3.Filters
{
    public class AdminAuthorizeAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var session = HttpContext.Current.Session;
            if (session["UserID"] == null)
            {
                filterContext.Result = new RedirectToRouteResult(
                    new System.Web.Routing.RouteValueDictionary(
                        new { controller = "Account", action = "Login" }
                    )
                );
                return;
            }
            int userId = (int)session["UserID"];
            using (var db = new AppDbContext())
            {
                var user = db.Users.FirstOrDefault(u => u.UserID == userId);
                if (user == null || user.Role != "Admin")
                {
                    filterContext.Result = new RedirectToRouteResult(
                        new System.Web.Routing.RouteValueDictionary(
                            new { controller = "Account", action = "Login" }
                        )
                    );
                }
            }
            base.OnActionExecuting(filterContext);
        }
    }
}
