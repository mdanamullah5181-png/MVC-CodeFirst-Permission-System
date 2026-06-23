using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using MVC_CodeFirst.Models;
using MVC_CodeFirst;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace MVC_CodeFirst.filters
{
    public class CustomAuthorize : AuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (!filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary(new { controller = "Account", action = "Login" })
                );
                return;
            }

            var controller = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
            var action = filterContext.ActionDescriptor.ActionName;
            var user = filterContext.HttpContext.User;

            using (var db = new ApplicationDbContext())
            {
                var userId = user.Identity.GetUserId();
                
                var userObj = db.Users
                    .Include(u => u.Roles)
                    .FirstOrDefault(u => u.Id == userId);

                if (userObj == null)
                {
                    filterContext.Result = new RedirectToRouteResult(
                        new RouteValueDictionary(new { controller = "Account", action = "Login" })
                    );
                    return;
                }

                var userManager = filterContext.HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
                var userRoles = userManager.GetRoles(userId).ToList();

                if (userRoles.Count == 0)
                {
                    var authenticationManager = filterContext.HttpContext.GetOwinContext().Authentication;
                    authenticationManager.SignOut(Microsoft.AspNet.Identity.DefaultAuthenticationTypes.ApplicationCookie);
                    
                    filterContext.Controller.TempData["ErrorMessage"] = "Your account has no roles assigned. Please contact the Administrator.";
                    filterContext.Result = new RedirectToRouteResult(
                        new RouteValueDictionary(new { controller = "Account", action = "Login" })
                    );
                    return;
                }

                var roleIds = db.Roles
                    .Where(r => userRoles.Contains(r.Name))
                    .Select(r => r.Id)
                    .ToList();

                var adminRole = db.Roles.FirstOrDefault(r => r.Name == "Admin");
                bool isAdmin = (adminRole != null && roleIds.Contains(adminRole.Id));
                
                if (isAdmin)
                {
                    return;
                }

                bool hasPermission = false;
                
                if (roleIds.Count > 0)
                {
                    var allPermissions = db.RolePermissions.ToList();
                    
                    var matchingPermissions = allPermissions
                        .Where(p => roleIds.Contains(p.RoleId))
                        .ToList();
                    
                    System.Diagnostics.Debug.WriteLine($"[AUTH DEBUG] ========================================");
                    System.Diagnostics.Debug.WriteLine($"[AUTH DEBUG] Checking Permission");
                    System.Diagnostics.Debug.WriteLine($"[AUTH DEBUG] Controller: '{controller}'");
                    System.Diagnostics.Debug.WriteLine($"[AUTH DEBUG] Action: '{action}'");
                    System.Diagnostics.Debug.WriteLine($"[AUTH DEBUG] User: {userObj.UserName}");
                    System.Diagnostics.Debug.WriteLine($"[AUTH DEBUG] User Roles: {string.Join(", ", userRoles)}");
                    System.Diagnostics.Debug.WriteLine($"[AUTH DEBUG] Role IDs: {string.Join(", ", roleIds)}");
                    System.Diagnostics.Debug.WriteLine($"[AUTH DEBUG] Total Permissions in DB: {allPermissions.Count}");
                    System.Diagnostics.Debug.WriteLine($"[AUTH DEBUG] Matching Permissions for User Roles: {matchingPermissions.Count}");
                    
                    foreach (var perm in matchingPermissions)
                    {
                        var controllerMatch = string.Equals(perm.ControllerName, controller, StringComparison.OrdinalIgnoreCase);
                        var actionMatch = string.Equals(perm.ActionName, action, StringComparison.OrdinalIgnoreCase);
                        System.Diagnostics.Debug.WriteLine($"[AUTH DEBUG] Permission - RoleId: {perm.RoleId}, Controller: '{perm.ControllerName}' (Match: {controllerMatch}), Action: '{perm.ActionName}' (Match: {actionMatch})");
                    }
                    
                    hasPermission = allPermissions
                        .Any(p => roleIds.Contains(p.RoleId) &&
                                  string.Equals(p.ControllerName, controller, StringComparison.OrdinalIgnoreCase) &&
                                  string.Equals(p.ActionName, action, StringComparison.OrdinalIgnoreCase));
                    
                    System.Diagnostics.Debug.WriteLine($"[AUTH DEBUG] Final Result: HasPermission = {hasPermission}");
                    System.Diagnostics.Debug.WriteLine($"[AUTH DEBUG] ========================================");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[AUTH DEBUG] No Role IDs found for user!");
                }

                System.Diagnostics.Debug.WriteLine($"[AUTH] User: {userObj.UserName}, Controller: '{controller}', Action: '{action}', HasPermission: {hasPermission}, IsAdmin: {isAdmin}");

                if (!hasPermission)
                {
                    filterContext.Controller.TempData["ErrorMessage"] = $"Unauthorized access! You don't have permission to access {controller}/{action}. Please contact the Administrator.";

                    filterContext.Result = new RedirectToRouteResult(
                        new RouteValueDictionary(new { controller = "Home", action = "Index" })
                    );
                }
            }
        }
    }
}
