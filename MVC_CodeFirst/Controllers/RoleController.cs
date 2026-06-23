using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity;
using MVC_CodeFirst.filters;
using MVC_CodeFirst.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace MVC_CodeFirst.Controllers
{
    [Authorize]

    public class RoleController : BaseController
    {

        // GET: Roles
        [Authorize(Roles = "Admin")]
        public ActionResult Index()
        {

            var context = new Models.ApplicationDbContext();

            var rolelist = context.Roles.OrderBy(r => r.Name).ToList().Select(rr =>
            new SelectListItem
            {

                Value = rr.Id.ToString(),
                Text = rr.Name
            }).ToList();

            ViewBag.Roles = rolelist;

            var userlist = context.Users.OrderBy(u => u.UserName).ToList().Select(uu =>
            new SelectListItem { Value = uu.UserName.ToString(), Text = uu.UserName }).ToList();
            ViewBag.Users = userlist;

            ViewBag.Message = "";

            return View();
        }

        // GET: /Roles/Create
        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            return View();
        }


        // POST: /Roles/Create
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                var context = new Models.ApplicationDbContext();
                context.Roles.Add(new Microsoft.AspNet.Identity.EntityFramework.IdentityRole()
                {
                    Name = collection["RoleName"]
                });
                context.SaveChanges();
                ViewBag.Message = "Role created successfully !";
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(string RoleName)
        {
            try
            {
                var context = new Models.ApplicationDbContext();
                var thisRole = context.Roles.Where(r => r.Name.Equals(RoleName, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
                
                if (thisRole == null)
                {
                    TempData["Error"] = "Role not found!";
                    return RedirectToAction("Index");
                }

                // Check if role is assigned to any user
                var usersWithRole = context.Users.Where(u => u.Roles.Any(r => r.RoleId == thisRole.Id)).ToList();
                if (usersWithRole.Count > 0)
                {
                    TempData["Error"] = $"Cannot delete role '{RoleName}' - it is assigned to {usersWithRole.Count} user(s). Please remove the role from all users first.";
                    return RedirectToAction("Index");
                }

                // Delete related permissions first
                var permissions = context.RolePermissions.Where(p => p.RoleId == thisRole.Id);
                context.RolePermissions.RemoveRange(permissions);

                // Then delete the role
                context.Roles.Remove(thisRole);
                context.SaveChanges();
                
                TempData["Message"] = $"Role '{RoleName}' deleted successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error deleting role: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        //
        // GET: /Roles/Edit/5
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(string roleName)
        {
            var context = new Models.ApplicationDbContext();
            var thisRole = context.Roles.Where(r => r.Name.Equals(roleName, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
            return View(thisRole);
        }


        // POST: /Roles/Edit/5
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Microsoft.AspNet.Identity.EntityFramework.IdentityRole role)
        {
            try
            {
                var context = new Models.ApplicationDbContext();
                context.Entry(role).State = System.Data.Entity.EntityState.Modified;
                context.SaveChanges();

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }


        //  Adding Roles to a user
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]

        public ActionResult RoleAddToUser(string UserName, string RoleName)
        {
            var context = new Models.ApplicationDbContext();
            var user = context.Users.Where(u => u.UserName.Equals(UserName, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();


            var role = context.Roles.Find(RoleName);
            string actualName = (role != null) ? role.Name : RoleName;

            var userStore = new UserStore<ApplicationUser>(context);
            var userManager = new UserManager<ApplicationUser>(userStore);

            userManager.AddToRole(user.Id, actualName);

            TempData["Message"] = "Role added to user successfully!";
            return RedirectToAction("Index");
        }


        //Getting a List of Roles for a User
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public ActionResult GetRoles(string UserName)
        {
            var context = new Models.ApplicationDbContext();

            // Always populate users and roles
            var userlist = context.Users.OrderBy(u => u.UserName).ToList().Select(uu =>
            new SelectListItem { Value = uu.UserName.ToString(), Text = uu.UserName }).ToList();
            ViewBag.Users = userlist;

            var rolelist = context.Roles.OrderBy(r => r.Name).ToList().Select(rr =>
            new SelectListItem { Value = rr.Name.ToString(), Text = rr.Name }).ToList();
            ViewBag.Roles = rolelist;

            if (!string.IsNullOrWhiteSpace(UserName))
            {
                ApplicationUser user = context.Users.Where(u => u.UserName.Equals(UserName, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();

                if (user != null)
                {
                    var userStore = new UserStore<ApplicationUser>(context);
                    var userManager = new UserManager<ApplicationUser>(userStore);
                    var userRoles = userManager.GetRoles(user.Id);
                    ViewBag.RolesForThisUser = userRoles;
                    ViewBag.SelectedUserName = UserName;
                    ViewBag.SelectedUserForDisplay = UserName;

                    // Create a list of only the roles assigned to this user for the Remove Role dropdown
                    var userRoleList = userRoles.ToList().Select(roleName => new SelectListItem { Value = roleName, Text = roleName }).ToList();
                    ViewBag.UserRoles = userRoleList;
                    ViewBag.Message = "Roles retrieved successfully !";
                }
            }

            return View("Index");
        }


        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteRoleForUser(string UserName, string RoleName)
        {
            var context = new Models.ApplicationDbContext();
            var user = context.Users.Where(u => u.UserName.Equals(UserName, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();

            var role = context.Roles.Find(RoleName);
            string actualName = (role != null) ? role.Name : RoleName;

            var userStore = new UserStore<ApplicationUser>(context);
            var userManager = new UserManager<ApplicationUser>(userStore);

            if (userManager.IsInRole(user.Id, actualName))
            {
                userManager.RemoveFromRole(user.Id, actualName);
                TempData["Message"] = "Role removed from user successfully!";
            }
            return RedirectToAction("Index");
        }

        private List<string> GetControllerActions()
        {
            List<string> actions = new List<string>();

            string[] allowedControllers = { "ProductCategory", "Product", "Order" };

            var types = System.Reflection.Assembly.GetExecutingAssembly().GetTypes()
                .Where(type => typeof(System.Web.Mvc.Controller).IsAssignableFrom(type)
                       && allowedControllers.Contains(type.Name.Replace("Controller", "")));

            foreach (var type in types)
            {
                var methods = type.GetMethods(System.Reflection.BindingFlags.Instance |
                                              System.Reflection.BindingFlags.DeclaredOnly |
                                              System.Reflection.BindingFlags.Public)
                    .Where(m => {
                        // Check for ActionResult or Task<ActionResult> return types
                        if (typeof(ActionResult).IsAssignableFrom(m.ReturnType))
                            return true;
                        
                        // Check for async methods returning Task<ActionResult>
                        if (m.ReturnType.IsGenericType && 
                            m.ReturnType.GetGenericTypeDefinition() == typeof(Task<>) &&
                            typeof(ActionResult).IsAssignableFrom(m.ReturnType.GetGenericArguments()[0]))
                            return true;
                        
                        return false;
                    })
                    .Where(m => {
                        // Exclude non-action methods
                        var actionName = m.Name;
                        if (actionName.StartsWith("get_") || actionName.StartsWith("set_"))
                            return false;
                        if (m.IsSpecialName)
                            return false;
                        // Exclude methods with [NonAction] attribute
                        if (m.GetCustomAttributes(typeof(System.Web.Mvc.NonActionAttribute), false).Length > 0)
                            return false;
                        return true;
                    });

                foreach (var method in methods)
                {
                    var controllerName = type.Name.Replace("Controller", "");
                    var actionName = method.Name;
                    actions.Add(controllerName + "-" + actionName);
                }
            }
            return actions;
        }

        [Authorize(Roles = "Admin")]
        public ActionResult ManagePermission(string roleId)
        {
            var context = new Models.ApplicationDbContext();
            var role = context.Roles.Find(roleId);

            if (role == null)
            {
                return RedirectToAction("Index");
            }

            ViewBag.RoleName = role.Name;
            ViewBag.RoleId = roleId;
            ViewBag.AllActions = GetControllerActions();



            var currentPermissions = context.RolePermissions.Where(p => p.RoleId == roleId).ToList() ?? new List<RolePermission>();

            return View(currentPermissions);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult SavePermission(string roleId, List<string> selectedActions)
        {
            try
            {
                var context = new Models.ApplicationDbContext();

                // Verify role exists
                var role = context.Roles.Find(roleId);
                if (role == null)
                {
                    TempData["Error"] = "Role not found!";
                    return RedirectToAction("Index");
                }

                // Remove old permissions
                var oldPermissions = context.RolePermissions.Where(p => p.RoleId == roleId).ToList();
                context.RolePermissions.RemoveRange(oldPermissions);

                // Add new permissions
                if (selectedActions != null && selectedActions.Count > 0)
                {
                    foreach (var action in selectedActions)
                    {
                        if (!string.IsNullOrWhiteSpace(action))
                        {
                            var split = action.Split('-');
                            if (split.Length == 2)
                            {
                                context.RolePermissions.Add(new RolePermission
                                {
                                    RoleId = roleId,
                                    ControllerName = split[0].Trim(),
                                    ActionName = split[1].Trim()
                                });
                            }
                        }
                    }
                }

                context.SaveChanges();
                TempData["Message"] = "Permissions updated successfully for role: " + role.Name;
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error saving permissions: " + ex.Message;
                return RedirectToAction("ManagePermission", new { roleId = roleId });
            }
        }

        // AJAX: Get roles for a specific user
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public JsonResult GetUserRolesAjax(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                return Json(new List<string>());
            }

            var context = new Models.ApplicationDbContext();
            ApplicationUser user = context.Users.Where(u => u.UserName.Equals(userName, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();

            if (user == null)
            {
                return Json(new List<string>());
            }

            var userStore = new UserStore<ApplicationUser>(context);
            var userManager = new UserManager<ApplicationUser>(userStore);
            var userRoles = userManager.GetRoles(user.Id).ToList();

            return Json(userRoles);
        }


    }
}