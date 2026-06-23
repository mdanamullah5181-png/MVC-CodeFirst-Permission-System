using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MVC_CodeFirst.Models;
using MVC_CodeFirst.Models.ViewModels;
using MVC_CodeFirst.filters;

namespace MVC_CodeFirst.Controllers
{
    public class ProductController : BaseController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Product
        [CustomAuthorize]
        public async Task<ActionResult> Index(int? categoryId, string searchName)
        {
            var products = db.Products.Include(p => p.ProductCategory).AsQueryable();

            // Filter by Category
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                products = products.Where(p => p.ProductCategoryId == categoryId.Value);
            }

            // Filter by Product Name
            if (!string.IsNullOrEmpty(searchName))
            {
                products = products.Where(p => p.ProductName.Contains(searchName));
            }

            var productList = await products
                .Select(p => new ProductViewModel
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    Unit = p.Unit,
                    UnitPrice = p.UnitPrice,
                    AvailableQuantity = p.AvailableQuantity,
                    ProductImage = p.ProductImage,
                    ProductCategoryId = p.ProductCategoryId,
                    CategoryName = p.ProductCategory.CategoryName,
                })
                .ToListAsync();

            // Send categories to view
            ViewBag.Categories = new SelectList(db.ProductCategories, "ProductCategoryId", "CategoryName");
            ViewBag.SelectedCategory = categoryId;
            ViewBag.SearchName = searchName;

            return View(productList);
        }

        // GET: Product/Details/5
        [CustomAuthorize]
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var product = await db.Products
                .Include(p => p.ProductCategory)
                .FirstOrDefaultAsync(p => p.ProductId == id);
            
            if (product == null)
            {
                return HttpNotFound();
            }

            // Convert Product entity to ProductViewModel
            var model = new ProductViewModel
            {
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                Unit = product.Unit,
                UnitPrice = product.UnitPrice,
                AvailableQuantity = product.AvailableQuantity,
                ProductImage = product.ProductImage,
                ProductCategoryId = product.ProductCategoryId,
                CategoryName = product.ProductCategory?.CategoryName,
                IsActive = true // Default to active if not stored in Product entity
            };

            return View(model);
        }

        // GET: Product/Create
        [CustomAuthorize]
        public ActionResult Create()
        {
            ViewBag.ProductCategoryId = new SelectList(db.ProductCategories, "ProductCategoryId", "CategoryName");
            ViewBag.Units = new SelectList(UnitHelper.GetUnits());
            return View();
        }

        // POST: Product/Create
        [HttpPost]
        [CustomAuthorize]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(ProductViewModel model)
        {
            if (ModelState.IsValid)
            {
                var product = new Product
                {
                    ProductName = model.ProductName,
                    Unit = model.Unit,
                    UnitPrice = model.UnitPrice,
                    AvailableQuantity = model.AvailableQuantity,
                    ProductCategoryId = model.ProductCategoryId
                };

                // Handle image upload
                if (model.ImageFile != null && model.ImageFile.ContentLength > 0)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ImageFile.FileName);
                    string path = Path.Combine(Server.MapPath("~/Images"), fileName);
                    model.ImageFile.SaveAs(path);
                    product.ProductImage = fileName;
                }

                db.Products.Add(product);
                await db.SaveChangesAsync();
                TempData["SuccessMessage"] = "Product created successfully!";
                return RedirectToAction("Index");
            }

            ViewBag.ProductCategoryId = new SelectList(db.ProductCategories, "ProductCategoryId", "CategoryName", model.ProductCategoryId);
            ViewBag.Units = new SelectList(UnitHelper.GetUnits(), model.Unit);
            return View(model);
        }

        // GET: Product/Edit/5
        [CustomAuthorize]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var product = await db.Products.FindAsync(id);
            if (product == null)
            {
                return HttpNotFound();
            }

            var model = new ProductViewModel
            {
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                Unit = product.Unit,
                UnitPrice = product.UnitPrice,
                AvailableQuantity = product.AvailableQuantity,
                ProductImage = product.ProductImage,
                ProductCategoryId = product.ProductCategoryId,
            };

            ViewBag.ProductCategoryId = new SelectList(db.ProductCategories, "ProductCategoryId", "CategoryName", model.ProductCategoryId);
            ViewBag.Units = new SelectList(UnitHelper.GetUnits(), model.Unit);

            return View(model);
        }

        // POST: Product/Edit/5
        [HttpPost]
        [CustomAuthorize]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(ProductViewModel model)
        {
            if (ModelState.IsValid)
            {
                var product = await db.Products.FindAsync(model.ProductId);
                if (product == null)
                {
                    return HttpNotFound();
                }

                product.ProductName = model.ProductName;
                product.Unit = model.Unit;
                product.UnitPrice = model.UnitPrice;
                product.AvailableQuantity = model.AvailableQuantity;
                product.ProductCategoryId = model.ProductCategoryId.Value;

                // Handle image upload
                if (model.ImageFile != null && model.ImageFile.ContentLength > 0)
                {
                    // Delete old image if exists
                    if (!string.IsNullOrEmpty(product.ProductImage))
                    {
                        string oldImagePath = Path.Combine(Server.MapPath("~/Images"), product.ProductImage);
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }
                    // Save new image
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ImageFile.FileName);
                    string path = Path.Combine(Server.MapPath("~/Images"), fileName);
                    model.ImageFile.SaveAs(path);
                    product.ProductImage = fileName;
                }

                db.Entry(product).State = EntityState.Modified;
                await db.SaveChangesAsync();
                TempData["SuccessMessage"] = "Product updated successfully!";
                return RedirectToAction("Index");
            }

            ViewBag.ProductCategoryId = new SelectList(db.ProductCategories, "ProductCategoryId", "CategoryName", model.ProductCategoryId);
            ViewBag.Units = new SelectList(UnitHelper.GetUnits(), model.Unit);
            return View(model);
        }

        // GET: Product/Delete/5
        [CustomAuthorize]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var product = await db.Products
                .Include(p => p.ProductCategory)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
            {
                return HttpNotFound();
            }

            var model = new ProductViewModel
            {
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                Unit = product.Unit,
                UnitPrice = product.UnitPrice,
                AvailableQuantity = product.AvailableQuantity,
                ProductImage = product.ProductImage,
                ProductCategoryId = product.ProductCategoryId,
                CategoryName = product.ProductCategory.CategoryName
            };

            return View(model);
        }

        // POST: Product/Delete/5
        [HttpPost, ActionName("Delete")]
        [CustomAuthorize]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var product = await db.Products.FindAsync(id);
                if (product == null)
                {
                    return HttpNotFound();
                }

                var orderDetails = db.OrderDetails.Where(od => od.ProductId == id).ToList();
                
                if (orderDetails.Any())
                {
                    db.OrderDetails.RemoveRange(orderDetails);
                    await db.SaveChangesAsync();
                }

                if (!string.IsNullOrEmpty(product.ProductImage))
                {
                    string imagePath = Path.Combine(Server.MapPath("~/Images"), product.ProductImage);
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                db.Products.Remove(product);
                await db.SaveChangesAsync();
                TempData["SuccessMessage"] = "Product deleted successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Cannot delete this product. Error: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // AJAX: Get Products by Category
        [HttpGet]
        public async Task<JsonResult> GetProductsByCategory(int categoryId)
        {
            var products = await db.Products
                .Where(p => p.ProductCategoryId == categoryId)
                .Select(p => new
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    Unit = p.Unit,
                    UnitPrice = p.UnitPrice,
                    AvailableQuantity = p.AvailableQuantity
                })
                .ToListAsync();

            return Json(products, JsonRequestBehavior.AllowGet);
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