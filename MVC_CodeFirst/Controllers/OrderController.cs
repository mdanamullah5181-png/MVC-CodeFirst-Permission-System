using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web.Mvc;
using MVC_CodeFirst.Models;
using MVC_CodeFirst.Models.ViewModels;
using System.Transactions;
using Microsoft.Ajax.Utilities;
using MVC_CodeFirst.filters;

namespace MVC_CodeFirst.Controllers
{
    public class OrderController : BaseController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Order
        [CustomAuthorize]
        public async Task<ActionResult> Index()
        {
            var orders = await db.Orders
                .Include(o => o.Customer)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new OrderMasterViewModel
                {
                    OrderId = o.OrderId,
                    CustomerName = o.Customer.CustomerName,
                    ContactNumber = o.Customer.ContactNumber,
                    ContactAddress = o.Customer.ContactAddress,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    CustomerId = o.CustomerId
                })
                .ToListAsync();

            return View(orders);
        }

        // GET: Order/Details/5
        [CustomAuthorize]
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var order = await db.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails.Select(od => od.Product))
                .Include(o => o.OrderDetails.Select(od => od.ProductCategory))
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                return HttpNotFound();
            }

            var model = new OrderMasterViewModel
            {
                OrderId = order.OrderId,
                CustomerName = order.Customer.CustomerName,
                ContactNumber = order.Customer.ContactNumber,
                ContactAddress = order.Customer.ContactAddress,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                OrderDetailsList = order.OrderDetails.Select(od => new OrderDetailsViewModel
                {
                    ProductName = od.Product.ProductName,
                    CategoryName = od.ProductCategory.CategoryName,
                    OrderQuantity = od.OrderQuantity,
                    OrderUnit = od.OrderUnit,
                    UnitPrice = od.UnitPrice,
                    Amount = od.Amount
                }).ToList()
            };

            return View("Details", model);
        }

        // GET: Order/Create
        [CustomAuthorize]
        public ActionResult Create()
        {
            ViewBag.ProductCategories = new SelectList(db.ProductCategories, "ProductCategoryId", "CategoryName");
            return View(new OrderMasterViewModel());
        }

        // POST: Order/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize]
        public async Task<ActionResult> Create(OrderMasterViewModel model, string OrderDetailsListJson)
        {
            // Deserialize JSON order details
            if (!string.IsNullOrEmpty(OrderDetailsListJson))
            {
                model.OrderDetailsList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<OrderDetailsViewModel>>(OrderDetailsListJson);
            }

            if (ModelState.IsValid)
            {
                if (model.OrderDetailsList == null || !model.OrderDetailsList.Any())
                {
                    TempData["ErrorMessage"] = "Please add at least one product to the order.";
                    ViewBag.ProductCategories = new SelectList(db.ProductCategories, "ProductCategoryId", "CategoryName");
                    return View(model);
                }

                using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    try
                    {
                        // Validate stock availability
                        foreach (var item in model.OrderDetailsList)
                        {
                            var product = await db.Products.FindAsync(item.ProductId);
                            if (product.AvailableQuantity < item.OrderQuantity)
                            {
                                TempData["ErrorMessage"] = $"Insufficient stock for {product.ProductName}. Available: {product.AvailableQuantity}";
                                ViewBag.ProductCategories = new SelectList(db.ProductCategories, "ProductCategoryId", "CategoryName");
                                return View(model);
                            }
                        }

                        // Create or find customer
                        var customer = await db.Customers.FirstOrDefaultAsync(c => c.ContactNumber == model.ContactNumber);
                        if (customer == null)
                        {
                            customer = new Customer
                            {
                                CustomerName = model.CustomerName,
                                ContactNumber = model.ContactNumber,
                                ContactAddress = model.ContactAddress
                            };
                            db.Customers.Add(customer);
                            await db.SaveChangesAsync();
                        }
                        else
                        {
                            // Update customer info
                            customer.CustomerName = model.CustomerName;
                            customer.ContactAddress = model.ContactAddress;
                            db.Entry(customer).State = EntityState.Modified;
                            await db.SaveChangesAsync();
                        }

                        // Create order
                        var order = new Order
                        {
                            CustomerId = customer.CustomerId,
                            OrderDate = DateTime.Now,
                            TotalAmount = model.OrderDetailsList.Sum(d => d.Amount)
                        };
                        db.Orders.Add(order);
                        await db.SaveChangesAsync();

                        // Create order details and update product quantity
                        foreach (var item in model.OrderDetailsList)
                        {
                            var orderDetail = new OrderDetail
                            {
                                OrderId = order.OrderId,
                                ProductCategoryId = item.ProductCategoryId,
                                ProductId = item.ProductId,
                                OrderQuantity = item.OrderQuantity,
                                OrderUnit = item.OrderUnit,
                                UnitPrice = item.UnitPrice,
                                Amount = item.Amount
                            };
                            db.OrderDetails.Add(orderDetail);

                            // Update product quantity
                            var product = await db.Products.FindAsync(item.ProductId);
                            product.AvailableQuantity -= item.OrderQuantity;
                            db.Entry(product).State = EntityState.Modified;
                        }

                        await db.SaveChangesAsync();
                        transaction.Complete();

                        TempData["SuccessMessage"] = "Order placed successfully!";
                        return RedirectToAction("Index");
                    }
                    catch (Exception ex)
                    {
                        TempData["ErrorMessage"] = "Error placing order: " + ex.Message;
                        ViewBag.ProductCategories = new SelectList(db.ProductCategories, "ProductCategoryId", "CategoryName");
                        return View(model);
                    }
                }
            }

            ViewBag.ProductCategories = new SelectList(db.ProductCategories, "ProductCategoryId", "CategoryName");
            return View(model);
        }

        // GET: Order/Edit/5
        [CustomAuthorize]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var order = await db.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails.Select(od => od.Product))
                .Include(o => o.OrderDetails.Select(od => od.ProductCategory))
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                return HttpNotFound();
            }

            var model = new OrderMasterViewModel
            {
                OrderId = order.OrderId,
                CustomerId = order.CustomerId,
                CustomerName = order.Customer.CustomerName,
                ContactNumber = order.Customer.ContactNumber,
                ContactAddress = order.Customer.ContactAddress,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                OrderDetailsList = order.OrderDetails.Select(od => new OrderDetailsViewModel
                {
                    OrderDetailsId = od.OrderDetailId,
                    OrderId = od.OrderId,
                    ProductCategoryId = od.ProductCategoryId,
                    ProductId = od.ProductId,
                    OrderQuantity = od.OrderQuantity,
                    OrderUnit = od.OrderUnit,
                    UnitPrice = od.UnitPrice,
                    Amount = od.Amount,
                    ProductName = od.Product.ProductName,
                    CategoryName = od.ProductCategory.CategoryName
                }).ToList()
            };

            ViewBag.ProductCategories = new SelectList(db.ProductCategories, "ProductCategoryId", "CategoryName");
            return View(model);
        }

        // POST: Order/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize]
        public async Task<ActionResult> Edit(OrderMasterViewModel model, string OrderDetailsListJson)
        {
            // Deserialize JSON order details
            if (!string.IsNullOrEmpty(OrderDetailsListJson))
            {
                model.OrderDetailsList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<OrderDetailsViewModel>>(OrderDetailsListJson);
            }

            if (ModelState.IsValid)
            {
                if (model.OrderDetailsList == null || !model.OrderDetailsList.Any())
                {
                    TempData["ErrorMessage"] = "Please add at least one product to the order.";
                    ViewBag.ProductCategories = new SelectList(db.ProductCategories, "ProductCategoryId", "CategoryName");
                    return View(model);
                }

                using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    try
                    {
                        var order = await db.Orders
                            .Include(o => o.OrderDetails)
                            .FirstOrDefaultAsync(o => o.OrderId == model.OrderId);

                        if (order == null)
                        {
                            return HttpNotFound();
                        }

                        // Restore original product quantities
                        foreach (var oldDetail in order.OrderDetails.ToList())
                        {
                            var product = await db.Products.FindAsync(oldDetail.ProductId);
                            product.AvailableQuantity += oldDetail.OrderQuantity;
                            db.Entry(product).State = EntityState.Modified;
                        }

                        await db.SaveChangesAsync();

                        // Validate stock availability for new quantities
                        foreach (var item in model.OrderDetailsList)
                        {
                            var product = await db.Products.FindAsync(item.ProductId);
                            if (product.AvailableQuantity < item.OrderQuantity)
                            {
                                // Restore quantities before returning error
                                foreach (var oldDetail in order.OrderDetails)
                                {
                                    var prod = await db.Products.FindAsync(oldDetail.ProductId);
                                    prod.AvailableQuantity -= oldDetail.OrderQuantity;
                                    db.Entry(prod).State = EntityState.Modified;
                                }
                                await db.SaveChangesAsync();

                                TempData["ErrorMessage"] = $"Insufficient stock for {product.ProductName}. Available: {product.AvailableQuantity}";
                                ViewBag.ProductCategories = new SelectList(db.ProductCategories, "ProductCategoryId", "CategoryName");
                                return View(model);
                            }
                        }

                        // Update customer
                        var customer = await db.Customers.FindAsync(order.CustomerId);
                        customer.CustomerName = model.CustomerName;
                        customer.ContactNumber = model.ContactNumber;
                        customer.ContactAddress = model.ContactAddress;
                        db.Entry(customer).State = EntityState.Modified;

                        // Delete old order details
                        db.OrderDetails.RemoveRange(order.OrderDetails);

                        // Create new order details and update product quantities
                        foreach (var item in model.OrderDetailsList)
                        {
                            var orderDetail = new OrderDetail
                            {
                                OrderId = order.OrderId,
                                ProductCategoryId = item.ProductCategoryId,
                                ProductId = item.ProductId,
                                OrderQuantity = item.OrderQuantity,
                                OrderUnit = item.OrderUnit,
                                UnitPrice = item.UnitPrice,
                                Amount = item.Amount
                            };
                            db.OrderDetails.Add(orderDetail);

                            // Update product quantity
                            var product = await db.Products.FindAsync(item.ProductId);
                            product.AvailableQuantity -= item.OrderQuantity;
                            db.Entry(product).State = EntityState.Modified;
                        }

                        // Update order
                        order.TotalAmount = model.OrderDetailsList.Sum(d => d.Amount);
                        db.Entry(order).State = EntityState.Modified;

                        await db.SaveChangesAsync();
                        transaction.Complete();

                        TempData["SuccessMessage"] = "Order updated successfully!";
                        return RedirectToAction("Index");
                    }
                    catch (Exception ex)
                    {
                        TempData["ErrorMessage"] = "Error updating order: " + ex.Message;
                        ViewBag.ProductCategories = new SelectList(db.ProductCategories, "ProductCategoryId", "CategoryName");
                        return View(model);
                    }
                }
            }

            ViewBag.ProductCategories = new SelectList(db.ProductCategories, "ProductCategoryId", "CategoryName");
            return View(model);
        }

        // GET: Order/Delete/5
        [CustomAuthorize]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var order = await db.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails.Select(od => od.Product))
                .Include(o => o.OrderDetails.Select(od => od.ProductCategory))
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                return HttpNotFound();
            }

            var model = new OrderMasterViewModel
            {
                OrderId = order.OrderId,
                CustomerName = order.Customer.CustomerName,
                ContactNumber = order.Customer.ContactNumber,
                ContactAddress = order.Customer.ContactAddress,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                OrderDetailsList = order.OrderDetails.Select(od => new OrderDetailsViewModel
                {
                    ProductName = od.Product.ProductName,
                    CategoryName = od.ProductCategory.CategoryName,
                    OrderQuantity = od.OrderQuantity,
                    OrderUnit = od.OrderUnit,
                    UnitPrice = od.UnitPrice,
                    Amount = od.Amount
                }).ToList()
            };

            return View(model);
        }

        // POST: Order/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [CustomAuthorize]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    var order = await db.Orders
                        .Include(o => o.OrderDetails)
                        .FirstOrDefaultAsync(o => o.OrderId == id);

                    if (order == null)
                    {
                        return HttpNotFound();
                    }

                    // Restore product quantities
                    foreach (var detail in order.OrderDetails)
                    {
                        var product = await db.Products.FindAsync(detail.ProductId);
                        product.AvailableQuantity += detail.OrderQuantity;
                        db.Entry(product).State = EntityState.Modified;
                    }
                    db.OrderDetails.RemoveRange(order.OrderDetails);
                    db.Orders.Remove(order);
                    await db.SaveChangesAsync();
                    transaction.Complete();

                    TempData["SuccessMessage"] = "Order deleted successfully!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Error deleting order: " + ex.Message;
                    return RedirectToAction("Index");
                }
            }
        }

        // AJAX: Get Product Details
        [HttpGet]
        public async Task<JsonResult> GetProductDetails(int productId)
        {
            var product = await db.Products.FindAsync(productId);
            if (product == null)
            {
                return Json(new { success = false, message = "Product not available" }, JsonRequestBehavior.AllowGet);
            }

            return Json(new
            {
                success = true,
                unit = product.Unit,
                unitPrice = product.UnitPrice,
                availableQuantity = product.AvailableQuantity
            }, JsonRequestBehavior.AllowGet);
        }

        // AJAX: Get Products by Category
        [HttpGet]
        public async Task<JsonResult> GetProductsByCategory(int categoryId)
        {
            var products = await db.Products
                .Where(p => p.ProductCategoryId == categoryId)
                .Select(p => new {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    Unit = p.Unit,
                    UnitPrice = p.UnitPrice,
                    AvailableQuantity = p.AvailableQuantity
                })
                .OrderBy(p => p.ProductName)
                .ToListAsync();

            return Json(products, JsonRequestBehavior.AllowGet);
        }


        private class SP_CustomerOrderSummary
        {
            // Customer Info (Result Set 1)
            public int CustomerId { get; set; }
            public string CustomerName { get; set; }
            public string ContactNumber { get; set; }
            public string ContactAddress { get; set; }
            public int TotalOrders { get; set; }
            public decimal? TotalSpent { get; set; }
            public DateTime? LastOrderDate { get; set; }

            // Order Info (Result Set 2)
            public int? OrderId { get; set; }
            public DateTime? OrderDate { get; set; }
            public decimal? OrderTotalAmount { get; set; }
            public int? NumberOfItems { get; set; }

            // Order Detail Info (Result Set 3)
            public int? Detail_OrderId { get; set; }
            public int? OrderDetailId { get; set; }
            public string ProductName { get; set; }
            public string CategoryName { get; set; }
            public int? OrderQuantity { get; set; }
            public string OrderUnit { get; set; }
            public decimal? UnitPrice { get; set; }
            public decimal? DetailAmount { get; set; }
            public int? CurrentStock { get; set; }
        }


        // GET: Order/CustomerSummary
        public ActionResult CustomerSummary()
        {
            // Get all customers for dropdown
            var customers = db.Customers
                .OrderBy(c => c.CustomerName)
                .Select(c => new SelectListItem
                {
                    Value = c.CustomerId.ToString(),
                    Text = c.CustomerName + " (" + c.ContactNumber + ")"
                })
                .ToList();

            ViewBag.CustomerId = new SelectList(customers, "Value", "Text");
            return View();
        }

        // POST: Order/CustomerSummary
        [HttpPost]
        public ActionResult CustomerSummary(int CustomerId)
        {
            return RedirectToAction("CustomerOrderDetails", new { id = CustomerId });
        }

        // GET: Order/CustomerOrderDetails/5
        public ActionResult CustomerOrderDetails(int id)
        {
            try
            {
                
                var sql = @"
            EXEC GetCustomerOrderSummary @CustomerId";

                var parameter = new System.Data.SqlClient.SqlParameter("@CustomerId", id);

                
                var results = db.Database.SqlQuery<SP_CustomerOrderSummary>(sql, parameter).ToList();

              
                var viewModel = new CustomerOrderSummaryViewModel();

                if (results.Count > 0)
                {
                     
                    var firstRow = results[0];
                    viewModel.CustomerId = firstRow.CustomerId;
                    viewModel.CustomerName = firstRow.CustomerName;
                    viewModel.ContactNumber = firstRow.ContactNumber;
                    viewModel.ContactAddress = firstRow.ContactAddress;
                    viewModel.TotalOrders = firstRow.TotalOrders;
                    viewModel.TotalSpent = firstRow.TotalSpent ?? 0;
                    viewModel.LastOrderDate = firstRow.LastOrderDate;

                    // Collect orders
                    viewModel.Orders = results
                        .Where(r => r.OrderId.HasValue)
                        .Select(r => new OrderSummaryVM
                        {
                            OrderId = r.OrderId.Value,
                            OrderDate = r.OrderDate.Value,
                            TotalAmount = r.OrderTotalAmount.Value,
                            NumberOfItems = r.NumberOfItems.Value
                        })
                        .DistinctBy(o => o.OrderId)  // Use .Distinct() if no DistinctBy
                        .ToList();

                    // Collect order details
                    viewModel.OrderDetails = results
                        .Where(r => r.OrderDetailId.HasValue)
                        .Select(r => new OrderDetailSummaryVM
                        {
                            OrderId = r.Detail_OrderId.Value,
                            OrderDetailId = r.OrderDetailId.Value,
                            ProductName = r.ProductName,
                            CategoryName = r.CategoryName,
                            OrderQuantity = r.OrderQuantity.Value,
                            OrderUnit = r.OrderUnit,
                            UnitPrice = r.UnitPrice.Value,
                            Amount = r.DetailAmount.Value,
                            CurrentStock = r.CurrentStock.Value
                        })
                        .ToList();
                }

                // Get customer name for display
                var customer = db.Customers.Find(id);
                if (customer != null)
                {
                    ViewBag.CustomerDisplayName = $"{customer.CustomerName} ({customer.ContactNumber})";
                }

                ViewBag.CustomerId = id;
                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error: " + ex.Message;
                return RedirectToAction("CustomerSummary");
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