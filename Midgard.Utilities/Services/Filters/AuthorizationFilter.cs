using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using System.Linq;
using System.Security.Claims;
using Midgard.Utilities.Models.Options;

namespace Midgard.Utilities.Services.Filters
{
    public class AuthorizationFilter : ActionFilterAttribute
    {
        private readonly RedirectToRouteResult _redirect = new RedirectToRouteResult(
                                                                new RouteValueDictionary {
                                                                    { "controller", "Home" },
                                                                    {"action", "Index" }
                                                                }
                                                            );
        private readonly string[] _requiredRoles;

        public AuthorizationFilter(string requiredRoles)
        {
            _requiredRoles = requiredRoles.Split(AuthOptions.Separator).Select(s => s.Trim()).ToArray();
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var currentUser = context.HttpContext.User;
            // TempData requires controller to be converted to a proper controller class,
            // rather than the object you get from context.Controller.
            var controller = context.Controller as Controller;
            if (currentUser == null || currentUser.Identity.IsAuthenticated == false)
            {
                controller.TempData["Alert"] = "Sign in to access.";
                context.Result = _redirect;
            } else
            {
                var roles = GetUserRoles(currentUser);
                if (roles != null && _requiredRoles.Intersect(roles).Count() > 0)
                {
                    base.OnActionExecuting(context);
                } else
                {
                    controller.TempData["Alert"] = "You do not have access to that resource.";
                    context.Result = _redirect;
                }
            }

        }

        private string[] GetUserRoles(ClaimsPrincipal currentUser)
        {
            var localIdentity = currentUser.Identities
                .Where(i => i.AuthenticationType == AuthOptions.Identity).First();
            var roleString = localIdentity.Claims
                .Where(c => c.Type == ClaimTypes.Role).First();
            return roleString == null ? null : roleString.Value.Split(AuthOptions.Separator);
        }
    }
}
