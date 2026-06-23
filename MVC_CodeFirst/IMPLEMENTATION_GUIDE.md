# অন্যান্য Controllers এ Permission System প্রয়োগ করা

## দ্রুত গাইড

প্রতিটি নতুন Controller অথবা View এ permission-based authorization যোগ করতে নিচের ধাপগুলো অনুসরণ করুন।

---

## Controller Level

### 1. CustomAuthorize Attribute যোগ করুন

**প্রতিটি public action method এ:**

```csharp
using MVC_CodeFirst.filters;

public class OrderController : BaseController
{
    [CustomAuthorize]
    public ActionResult Index()
    {
        // Code...
        return View();
    }

    [CustomAuthorize]
    public ActionResult Details(int? id)
    {
        // Code...
        return View();
    }

    [CustomAuthorize]
    public ActionResult Create()
    {
        // Code...
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [CustomAuthorize]
    public ActionResult Create(Order model)
    {
        // Code...
        return View();
    }

    [CustomAuthorize]
    public ActionResult Edit(int? id)
    {
        // Code...
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [CustomAuthorize]
    public ActionResult Edit(int id, Order model)
    {
        // Code...
        return View();
    }

    [CustomAuthorize]
    public ActionResult Delete(int? id)
    {
        // Code...
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [CustomAuthorize]
    public ActionResult DeleteConfirmed(int id)
    {
        // Code...
        return RedirectToAction("Index");
    }
}
```

### 2. Controller কে ManagePermission এ রেজিস্টার করুন

**File**: `Controllers/RoleController.cs`

`GetControllerActions()` method এ যান এবং আপনার controller এর নাম যোগ করুন:

```csharp
string[] allowedControllers = { "ProductCategory", "Product", "Order", "YourNewController" };
```

---

## View Level

### 1. Index View এ Create Button এ Permission যোগ করুন

```html
<div class="row mb-3">
    <div class="col-md-12">
        @{
            bool canCreate = ViewBag.CanAccess("Order", "Create");
            if (canCreate)
            {
                @Html.ActionLink("Create New Order", "Create", null, new { @class = "btn btn-primary" })
            }
            else
            {
                <button class="btn btn-primary" disabled title="আপনার কাছে এই অনুমতি নেই">
                    Create New Order
                </button>
            }
        }
    </div>
</div>
```

### 2. Item List এ প্রতিটি Row এ Permission-based Buttons

```html
<table class="table table-striped">
    <thead>
        <tr>
            <th>Name</th>
            <th>Actions</th>
        </tr>
    </thead>
    <tbody>
        @foreach (var item in Model)
        {
            <tr>
                <td>@item.Name</td>
                <td>
                    @{
                        bool canView = ViewBag.CanAccess("Order", "Details");
                        bool canEdit = ViewBag.CanAccess("Order", "Edit");
                        bool canDelete = ViewBag.CanAccess("Order", "Delete");
                    }

                    @if (canView)
                    {
                        <a href="@Url.Action("Details", new { id = item.Id })" class="btn btn-sm btn-info">
                            <i class="fas fa-eye"></i> View
                        </a>
                    }

                    @if (canEdit)
                    {
                        <a href="@Url.Action("Edit", new { id = item.Id })" class="btn btn-sm btn-warning">
                            <i class="fas fa-edit"></i> Edit
                        </a>
                    }

                    @if (canDelete)
                    {
                        <a href="@Url.Action("Delete", new { id = item.Id })" class="btn btn-sm btn-danger">
                            <i class="fas fa-trash"></i> Delete
                        </a>
                    }

                    @if (!canView && !canEdit && !canDelete)
                    {
                        <span class="text-muted">কোনো অনুমতি নেই</span>
                    }
                </td>
            </tr>
        }
    </tbody>
</table>
```

### 3. Details/Edit View এ Action Buttons

```html
<div class="card-footer">
    @{
        bool canEdit = ViewBag.CanAccess("Order", "Edit");
        bool canDelete = ViewBag.CanAccess("Order", "Delete");
    }

    @if (canEdit)
    {
        <a href="@Url.Action("Edit", new { id = Model.Id })" class="btn btn-warning">
            <i class="fas fa-edit"></i> Edit
        </a>
    }
    else
    {
        <button class="btn btn-warning" disabled title="আপনার কাছে Edit অনুমতি নেই">
            <i class="fas fa-edit"></i> Edit
        </button>
    }

    @if (canDelete)
    {
        <a href="@Url.Action("Delete", new { id = Model.Id })" class="btn btn-danger">
            <i class="fas fa-trash"></i> Delete
        </a>
    }
    else
    {
        <button class="btn btn-danger" disabled title="আপনার কাছে Delete অনুমতি নেই">
            <i class="fas fa-trash"></i> Delete
        </button>
    }

    <a href="@Url.Action("Index")" class="btn btn-secondary">Back</a>
</div>
```

### 4. Partial View এ (যেমন Card Component)

```html
@model OrderViewModel

<div class="card">
    <!-- Card content -->
    <div class="card-footer">
        @{
            bool canDetails = ViewBag.CanAccess("Order", "Details");
            bool canEdit = ViewBag.CanAccess("Order", "Edit");
            bool canDelete = ViewBag.CanAccess("Order", "Delete");
        }

        <div class="btn-group w-100">
            @if (canDetails)
            {
                <a href="@Url.Action("Details", new { id = Model.OrderId })" 
                   class="btn btn-sm btn-info">Details</a>
            }

            @if (canEdit)
            {
                <a href="@Url.Action("Edit", new { id = Model.OrderId })" 
                   class="btn btn-sm btn-warning">Edit</a>
            }

            @if (canDelete)
            {
                <a href="@Url.Action("Delete", new { id = Model.OrderId })" 
                   class="btn btn-sm btn-danger">Delete</a>
            }
        </div>
    </div>
</div>
```

---

## Complete Example: Order Controller

### Controller Code

```csharp
using MVC_CodeFirst.filters;
using MVC_CodeFirst.Models;
using System.Web.Mvc;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace MVC_CodeFirst.Controllers
{
    public class OrderController : BaseController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Order
        [CustomAuthorize]
        public async Task<ActionResult> Index()
        {
            var orders = await db.Orders.Include(o => o.Customer).ToListAsync();
            return View(orders);
        }

        // GET: Order/Details/5
        [CustomAuthorize]
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);

            Order order = await db.Orders.FindAsync(id);
            if (order == null)
                return HttpNotFound();

            return View(order);
        }

        // GET: Order/Create
        [CustomAuthorize]
        public ActionResult Create()
        {
            ViewBag.CustomerId = new SelectList(db.Customers, "CustomerId", "CustomerName");
            return View();
        }

        // POST: Order/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize]
        public async Task<ActionResult> Create(Order order)
        {
            if (ModelState.IsValid)
            {
                db.Orders.Add(order);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.CustomerId = new SelectList(db.Customers, "CustomerId", "CustomerName", order.CustomerId);
            return View(order);
        }

        // GET: Order/Edit/5
        [CustomAuthorize]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);

            Order order = await db.Orders.FindAsync(id);
            if (order == null)
                return HttpNotFound();

            ViewBag.CustomerId = new SelectList(db.Customers, "CustomerId", "CustomerName", order.CustomerId);
            return View(order);
        }

        // POST: Order/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize]
        public async Task<ActionResult> Edit(int id, Order order)
        {
            if (id != order.OrderId)
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);

            if (ModelState.IsValid)
            {
                db.Entry(order).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.CustomerId = new SelectList(db.Customers, "CustomerId", "CustomerName", order.CustomerId);
            return View(order);
        }

        // GET: Order/Delete/5
        [CustomAuthorize]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);

            Order order = await db.Orders.FindAsync(id);
            if (order == null)
                return HttpNotFound();

            return View(order);
        }

        // POST: Order/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [CustomAuthorize]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Order order = await db.Orders.FindAsync(id);
            db.Orders.Remove(order);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();
            base.Dispose(disposing);
        }
    }
}
```

### View Code (Index.cshtml)

```html
@model IEnumerable<MVC_CodeFirst.Models.Order>

@{
    ViewBag.Title = "Orders";
}

<div class="container mt-4">
    <h2>Orders</h2>
    <hr />

    <!-- Create Button with Permission -->
    <div class="row mb-3">
        <div class="col-md-12">
            @{
                bool canCreate = ViewBag.CanAccess("Order", "Create");
                if (canCreate)
                {
                    @Html.ActionLink("Create New Order", "Create", null, new { @class = "btn btn-primary" })
                }
                else
                {
                    <button class="btn btn-primary" disabled title="আপনার কাছে এই অনুমতি নেই">
                        Create New Order
                    </button>
                }
            }
        </div>
    </div>

    <!-- Orders Table -->
    @if (Model.Any())
    {
        <div class="table-responsive">
            <table class="table table-striped table-hover">
                <thead class="table-dark">
                    <tr>
                        <th>Order ID</th>
                        <th>Customer</th>
                        <th>Order Date</th>
                        <th>Total</th>
                        <th>Actions</th>
                    </tr>
                </thead>
                <tbody>
                    @foreach (var order in Model)
                    {
                        <tr>
                            <td>@order.OrderId</td>
                            <td>@order.Customer.CustomerName</td>
                            <td>@order.OrderDate.ToString("dd/MM/yyyy")</td>
                            <td>@order.Total.ToString("N2")</td>
                            <td>
                                @{
                                    bool canView = ViewBag.CanAccess("Order", "Details");
                                    bool canEdit = ViewBag.CanAccess("Order", "Edit");
                                    bool canDelete = ViewBag.CanAccess("Order", "Delete");
                                }

                                @if (canView)
                                {
                                    <a href="@Url.Action("Details", new { id = order.OrderId })" 
                                       class="btn btn-sm btn-info">View</a>
                                }

                                @if (canEdit)
                                {
                                    <a href="@Url.Action("Edit", new { id = order.OrderId })" 
                                       class="btn btn-sm btn-warning">Edit</a>
                                }

                                @if (canDelete)
                                {
                                    <a href="@Url.Action("Delete", new { id = order.OrderId })" 
                                       class="btn btn-sm btn-danger">Delete</a>
                                }

                                @if (!canView && !canEdit && !canDelete)
                                {
                                    <span class="text-muted">কোনো অনুমতি নেই</span>
                                }
                            </td>
                        </tr>
                    }
                </tbody>
            </table>
        </div>
    }
    else
    {
        <div class="alert alert-warning">
            কোনো Order নেই।
        </div>
    }
</div>
```

---

## Checklist

নতুন Controller/Action যোগ করার সময়:

- [ ] Controller class `BaseController` থেকে inherit করে
- [ ] প্রতিটি public action এ `[CustomAuthorize]` attribute আছে
- [ ] Controller নাম `RoleController.cs` এর `allowedControllers` array এ আছে
- [ ] View এ buttons permission check করছে (`ViewBag.CanAccess()`)
- [ ] Testing করেছেন permission ছাড়া access করলে logout হয় কিনা
- [ ] UI তে permission না থাকলে buttons disabled দেখা যাচ্ছে কিনা

---

## সাধারণ ভুলত্রুটি

❌ **ভুল:**
```csharp
// [CustomAuthorize] নেই
public ActionResult Index()
{
    return View();
}
```

✓ **সঠিক:**
```csharp
[CustomAuthorize]
public ActionResult Index()
{
    return View();
}
```

---

❌ **ভুল:**
```html
<!-- Permission চেক নেই -->
<a href="@Url.Action("Edit", new { id = Model.Id })" class="btn">Edit</a>
```

✓ **সঠিক:**
```html
@{
    bool canEdit = ViewBag.CanAccess("Order", "Edit");
}
@if (canEdit)
{
    <a href="@Url.Action("Edit", new { id = Model.Id })" class="btn">Edit</a>
}
```

---

❌ **ভুল:**
```csharp
// "ordercontroller" (lowercase) বা "Order" ছাড়াই
string[] allowedControllers = { "order" };
```

✓ **সঠিক:**
```csharp
// Exact match (case-sensitive)
string[] allowedControllers = { "Order" };
```
