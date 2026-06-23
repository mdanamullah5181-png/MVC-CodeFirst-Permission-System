# Permission System - Quick Reference

## 30 সেকেন্ডে বুঝুন

```
Admin বলল: "এই user শুধু Product Details দেখতে পারবে"
      ↓
এই user কি করতে পারবে?
  ✓ Product page এ যেতে পারবে
  ✓ Details button দেখতে পারবে এবং ক্লিক করতে পারবে
  ✗ Create button দেখতে পারবে না
  ✗ Edit button disabled থাকবে
  ✗ Delete button disabled থাকবে
  ✗ সরাসরি /Product/Create এ যেতে চাইলে → Login page

সারমর্ম: শুধুমাত্র permission থাকা কাজই করতে পারবে
```

---

## Admin Panel ব্যবহার

### Permission দেওয়ার ধাপ:

```
1. প্রথম login করুন (Admin account)
2. Navigation → Role Management
3. একটি Role খুঁজুন অথবা নতুন তৈরি করুন
4. Manage Permission ক্লিক করুন
5. Permission matrix দেখবেন:
   - বাম পাশে: Product, Order, ProductCategory
   - উপরে: Index, Create, Edit, Delete, Details
   - যা permit করতে চান তা ✓ করুন
6. Save করুন
7. User এর page এ যান এবং এই Role assign করুন
```

### Permission Matrix Example:

```
Module        │ Index │ Create │ Edit  │ Delete │ Details
──────────────┼───────┼────────┼───────┼────────┼─────────
Product       │  ✓    │   ✗    │   ✗   │   ✗    │    ✓
Order         │  ✓    │   ✓    │   ✓   │   ✗    │    ✓
ProductCat    │  ✓    │   ✗    │   ✗   │   ✗    │    ✓
```

এই setting এর মানে:
- User products এর list দেখতে পারে এবং details দেখতে পারে
- User নতুন order তৈরি, edit, দেখতে পারে কিন্তু delete করতে পারে না
- User শুধু category list এবং details দেখতে পারে

---

## Code এ Permission Check করা

### View এ (Razor):

```csharp
// Method 1: Direct check
@{
    bool canEdit = ViewBag.CanAccess("Product", "Edit");
}

// Method 2: Inline
@if (ViewBag.CanAccess("Product", "Delete"))
{
    <a href="...">Delete</a>
}

// Method 3: Loop through all permissions
@{
    var userPerms = ViewBag.UserPermissions as List<RolePermission>;
    foreach (var perm in userPerms)
    {
        // perm.ControllerName, perm.ActionName
    }
}
```

### Controller এ (C#):

```csharp
// Method 1: Check permission
if (HasPermission("Product", "Edit"))
{
    // User এর Edit permission আছে
}

// Method 2: Get all permissions
List<RolePermission> perms = GetUserPermissions();

// Method 3: Direct use [CustomAuthorize] attribute
[CustomAuthorize]
public ActionResult Edit(int id)
{
    // এই method এ unauthorized user access করতে পারবে না
}
```

---

## Common View Patterns

### 1. Button কে conditional দেখানো

```html
@{
    bool canEdit = ViewBag.CanAccess("Product", "Edit");
}

@if (canEdit)
{
    <a href="@Url.Action("Edit")" class="btn btn-primary">Edit</a>
}
else
{
    <button disabled class="btn btn-primary">Edit</button>
}
```

### 2. বহু buttons একসাথে

```html
@{
    bool canView = ViewBag.CanAccess("Product", "Details");
    bool canEdit = ViewBag.CanAccess("Product", "Edit");
    bool canDelete = ViewBag.CanAccess("Product", "Delete");
}

<div class="btn-group">
    @if (canView) { <a href="...">View</a> }
    @if (canEdit) { <a href="...">Edit</a> }
    @if (canDelete) { <a href="...">Delete</a> }
</div>

@if (!canView && !canEdit && !canDelete)
{
    <p>আপনার কাছে কোনো permission নেই</p>
}
```

### 3. Table rows এ buttons

```html
<table>
    @foreach (var item in Model)
    {
        <tr>
            <td>@item.Name</td>
            <td>
                @if (ViewBag.CanAccess("Product", "Details"))
                {
                    <a href="...">View</a>
                }
                @if (ViewBag.CanAccess("Product", "Edit"))
                {
                    <a href="...">Edit</a>
                }
            </td>
        </tr>
    }
</table>
```

---

## Database Tables

### AspNetRoles
```
Id          | Name
────────────┼──────────
1a2b3c...   | Admin
2b3c4d...   | Editor
3c4d5e...   | Viewer
```

### AspNetUserRoles (User ← Role mapping)
```
UserId      | RoleId
────────────┼──────────
user1...    | 2b3c4d... (Editor role)
user2...    | 3c4d5e... (Viewer role)
```

### RolePermissions (Role ← Permission mapping)
```
Id | RoleId      | ControllerName | ActionName
───┼─────────────┼────────────────┼─────────
1  | 2b3c4d...   | Product        | Index
2  | 2b3c4d...   | Product        | Details
3  | 2b3c4d...   | Product        | Edit
4  | 3c4d5e...   | Product        | Index
5  | 3c4d5e...   | Product        | Details
```

এই setup এ:
- **Editor** user: Product Index, Details, Edit করতে পারে
- **Viewer** user: শুধু Product Index, Details দেখতে পারে

---

## Error Messages

### Scenario 1: Permission নেই
```
ব্যবহারকারী যা দেখবে:
────────────────────
Unauthorized access! 
আপনার কাছে এই কাজ করার অনুমতি নেই। 
দয়া করে Admin এর সাথে যোগাযোগ করুন।

[Login button]
```

### Scenario 2: UI level (disabled button)
```html
<button disabled title="আপনার কাছে Edit অনুমতি নেই">
    Edit
</button>
```

---

## Troubleshooting

| সমস্যা | সম্ভাব্য কারণ | সমাধান |
|--------|---------|--------|
| Button দেখা যাচ্ছে কিন্তু disabled | Permission নেই | Admin panel এ permission যোগ করুন |
| Permission দিলেও button disabled | Controller নাম ভুল | "Product" দিন "product" নয় |
| সরাসরি URL access করলে logout | Permission check fail | RolePermission table check করুন |
| ViewBag.CanAccess null | BaseController inherit করছেন না | BaseController extend করুন |
| নতুন Controller যোগ করলেও Permission Management এ না দেখা | allowed list এ নেই | RoleController.cs এ নাম যোগ করুন |

---

## Security Points

✓ **Safe**: [CustomAuthorize] attribute দিয়ে controller action protect করা  
✓ **Safe**: ViewBag check করে UI permission control করা  
✓ **Safe**: Server session logout করা unauthorized attempt এ  

✗ **Unsafe**: শুধু UI hide করে backend protect না করা  
✗ **Unsafe**: Direct URL access এ permission check না করা  
✗ **Unsafe**: Session logout ছাড়া redirect করা  

---

## কিভাবে নতুন Controller permission enable করতে হয়

### 1 minute setup:

```
File: Controllers/RoleController.cs
Line: GetControllerActions() method

খুঁজুন: string[] allowedControllers = { "Product", ... };
যোগ করুন: "YourController" নাম
Save করুন

Done! এখন admin panel এ permission দিতে পারবেন
```

---

## Permission Levels (Examples)

### Level 1: Full Access
```
✓ Index, ✓ Create, ✓ Edit, ✓ Delete, ✓ Details
→ সম্পূর্ণ নিয়ন্ত্রণ
```

### Level 2: Editor
```
✓ Index, ✓ Edit, ✓ Details, ✗ Create, ✗ Delete
→ বিদ্যমান items edit করতে পারে, নতুন তৈরি/delete করতে পারে না
```

### Level 3: Viewer
```
✓ Index, ✓ Details, ✗ Create, ✗ Edit, ✗ Delete
→ শুধু দেখতে পারে, কোনো পরিবর্তন করতে পারে না
```

### Level 4: No Access
```
✗ Index, ✗ Create, ✗ Edit, ✗ Delete, ✗ Details
→ এই module access করতে পারে না, login page এ redirect
```

---

## Quick Checklist

নতুন feature implement করার সময়:

- [ ] Controller এ [CustomAuthorize] attribute আছে?
- [ ] View এ ViewBag.CanAccess() check করছি?
- [ ] Buttons disabled/enabled properly?
- [ ] RoleController.cs এ controller নাম আছে?
- [ ] Testing করেছি permission ছাড়া?
- [ ] Bengali error messages ঠিক আছে?

---

## একনজরে

```csharp
// Architecture এ তিনটি লেয়ার:

1. Authorization Layer (Server-side)
   └─ CustomAuthorize Filter
      └─ RolePermission চেক

2. Data Layer (Model)
   └─ RolePermission, Role, User tables

3. UI Layer (View-side)
   └─ ViewBag.CanAccess() function
   └─ Conditional buttons
```

```
Flow:
User Request
    ↓
CustomAuthorize Filter
    ├─ Is User logged in?
    ├─ Does User have permission?
    └─ If NO → Logout + Redirect Login
    └─ If YES → Execute Action
               └─ BaseController loads permissions
                  └─ View renders with conditional buttons
```

এটাই সব! 🎉
