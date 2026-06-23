using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web.Mvc;
using MVC_CodeFirst.Models;
using MVC_CodeFirst.Models.ViewModels;
using MVC_CodeFirst.filters;

namespace MVC_CodeFirst.Controllers
{
    public class ProductCategoryController : BaseController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: ProductCategory
        [CustomAuthorize]
        public async Task<ActionResult> Index()
        {
            var categories = await db.ProductCategories
                .Select(c => new ProductCategoryViewModel
                {
                    ProductCategoryId = c.ProductCategoryId,
                    CategoryName = c.CategoryName,
                    CategoryDescription = c.CategoryDescription
                })
                .ToListAsync();

            return View(categories);
        }

        // GET: ProductCategory/Details/5
        [CustomAuthorize]
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var productCategory = await db.ProductCategories.FindAsync(id);
            if (productCategory == null)
            {
                return HttpNotFound();
            }
            return View(productCategory);
        }

        // GET: ProductCategory/Create
        [CustomAuthorize]
        public ActionResult Create()
        {
            return View();
        }

        // POST: ProductCategory/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize]
        public async Task<ActionResult> Create(ProductCategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                var category = new ProductCategory
                {
                    CategoryName = model.CategoryName,
                    CategoryDescription = model.CategoryDescription
                };

                db.ProductCategories.Add(category);
                await db.SaveChangesAsync();

                TempData["SuccessMessage"] = "Product Category created successfully!";
                return RedirectToAction("Index");
            }

            return View(model);
        }

        // GET: ProductCategory/Edit/5
        [CustomAuthorize]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var category = await db.ProductCategories.FindAsync(id);
            if (category == null)
            {
                return HttpNotFound();
            }

            var model = new ProductCategoryViewModel
            {
                ProductCategoryId = category.ProductCategoryId,
                CategoryName = category.CategoryName,
                CategoryDescription = category.CategoryDescription
            };

            return View(model);
        }

        // POST: ProductCategory/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize]
        public async Task<ActionResult> Edit(ProductCategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                var category = await db.ProductCategories.FindAsync(model.ProductCategoryId);
                if (category == null)
                {
                    return HttpNotFound();
                }

                category.CategoryName = model.CategoryName;
                category.CategoryDescription = model.CategoryDescription;

                db.Entry(category).State = EntityState.Modified;
                await db.SaveChangesAsync();

                TempData["SuccessMessage"] = "Product Category updated successfully!";
                return RedirectToAction("Index");
            }

            return View(model);
        }

        // GET: ProductCategory/Delete/5
        [CustomAuthorize]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var category = await db.ProductCategories.FindAsync(id);
            if (category == null)
            {
                return HttpNotFound();
            }

            var model = new ProductCategoryViewModel
            {
                ProductCategoryId = category.ProductCategoryId,
                CategoryName = category.CategoryName,
                CategoryDescription = category.CategoryDescription
            };

            return View(model);
        }

        // POST: ProductCategory/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [CustomAuthorize]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var category = await db.ProductCategories.FindAsync(id);
                if (category == null)
                {
                    return HttpNotFound();
                }

                var productsInCategory = db.Products.Where(p => p.ProductCategoryId == id).ToList();
                
                if (productsInCategory.Any())
                {
                    foreach (var product in productsInCategory)
                    {
                        var orderDetails = db.OrderDetails.Where(od => od.ProductId == product.ProductId).ToList();
                        db.OrderDetails.RemoveRange(orderDetails);
                    }
                    
                    db.Products.RemoveRange(productsInCategory);
                    await db.SaveChangesAsync();
                }

                db.ProductCategories.Remove(category);
                await db.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Product Category '{category.CategoryName}' and {productsInCategory.Count} related products deleted successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Cannot delete this category. Error: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}