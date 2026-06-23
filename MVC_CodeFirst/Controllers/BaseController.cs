using Microsoft.AspNet.Identity;
using MVC_CodeFirst.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVC_CodeFirst.Controllers
{
    public class BaseController : Controller
    {
        protected List<RolePermission> GetUserPermissions()
        {
            var permissions = new List<RolePermission>();

            if (User.Identity.IsAuthenticated)
            {
                using (var db = new ApplicationDbContext())
                {
                    var userId = User.Identity.GetUserId();
                    var userObj = db.Users.Find(userId);

                    if (userObj != null)
                    {
                        var userRoles = userObj.Roles.Select(r => r.RoleId).ToList();

                        permissions = db.RolePermissions
                            .Where(p => userRoles.Contains(p.RoleId))
                            .ToList();
                    }
                }
            }

            return permissions;
        }

        protected bool HasPermission(string controllerName, string actionName)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return false;
            }

            using (var db = new ApplicationDbContext())
            {
                var userId = User.Identity.GetUserId();
                var userObj = db.Users.Find(userId);

                if (userObj == null)
                {
                    return false;
                }

                var userRoles = userObj.Roles.Select(r => r.RoleId).ToList();

                return db.RolePermissions.Any(p =>
                    userRoles.Contains(p.RoleId) &&
                    p.ControllerName == controllerName &&
                    p.ActionName == actionName);
            }
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            if (User.Identity.IsAuthenticated)
            {
                var permissions = GetUserPermissions();
                ViewBag.UserPermissions = permissions;

                ViewBag.CanAccess = new Func<string, string, bool>((controller, action) =>
                {
                    return permissions.Any(p => 
                        p.ControllerName == controller && 
                        p.ActionName == action);
                });
            }
        }
    }
}
