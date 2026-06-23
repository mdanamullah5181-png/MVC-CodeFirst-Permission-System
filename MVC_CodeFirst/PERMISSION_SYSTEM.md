# Permission-Based Authorization System

## System Overview

This MVC application includes a complete **Permission-Based Access Control** system that allows Admins to assign specific permissions to each user.

### Key Features:

1. **Fine-grained Permissions**: Separate permissions for each Controller-Action
2. **Role-based Assignment**: Permissions are assigned to Roles first, then Users are assigned Roles
3. **Server-side Protection**: Unauthorized access directly redirects to Login page
4. **UI-level Control**: Buttons are hidden when user doesn't have permissions
5. **Session Management**: Automatic logout on unauthorized access attempts

---

## How to Use

### Step 1: Create a Role (Admin)

1. **Admin Dashboard** → **Role Management** → **Create Role**
2. Enter a Role name (e.g., `Editor`, `Viewer`, `Manager`)

### Step 2: Assign Permissions (Admin)

1. Find your created Role in **Role Management**
2. Click **Manage Permission** button
3. You'll see a **Permission Matrix** with:
   - **Rows**: Controllers (Product, Order, ProductCategory)
   - **Columns**: Actions (Index, Create, Edit, Delete, Details)

4. **Check** the actions you want to allow for this Role
5. **Save** the changes

**Example**:
- **Role**: "Product Viewer"
  - ✓ Product → Index
  - ✓ Product → Details
  - ✗ Product → Create
  - ✗ Product → Edit
  - ✗ Product → Delete

### Step 3: Assign Role to User (Admin)

1. Go to the User section in **Role Management**
2. **Select** a User
3. **Select** a Role and **Add Role**
4. User will now have all permissions of that Role

---

## সিস্টেম কীভাবে কাজ করে

### 1. **Server-side Authorization** (CustomAuthorize Filter)

প্রতিটি Controller action এ `[CustomAuthorize]` attribute থাকে।

```csharp
[CustomAuthorize]
public ActionResult Details(int? id)
{
    // Code...
}
```

**যখন User কোনো Action access করে:**

1. ✓ User logged in? → Continue
2. ✓ User এর Role এ এই Controller-Action এর permission আছে? → Continue
3. ✗ Permission নেই? → 
   - User এর session sign out করা হয়
   - ErrorMessage set করা হয়
   - Login page এ redirect করা হয়

### 2. **Client-side UI Control** (BaseController)

BaseController এ দুটি helper রয়েছে:

#### A. `ViewBag.UserPermissions`
User এর সকল permission এর list

#### B. `ViewBag.CanAccess(controller, action)`
একটি function যা specific permission check করে

**View এ ব্যবহার:**

```html
@{
    bool canEdit = ViewBag.CanAccess("Product", "Edit");
}

@if (canEdit)
{
    <a href="..." class="btn btn-primary">Edit</a>
}
else
{
    <button disabled class="btn btn-primary">Edit</button>
}
```

---

## সম্ভাব্য Scenarios এবং Behavior

### Scenario 1: User শুধু Details দেখতে পারে

**Admin দিয়েছে:**
- Product → Details ✓

**ফলাফল:**
- User Product page খুলতে পারে
- Details button দেখতে পারে এবং ক্লিক করতে পারে
- Edit, Delete, Create buttons disabled দেখায়
- Edit/Delete/Create URLs directly access করলে → Login page

### Scenario 2: User সম্পূর্ণ নিষিদ্ধ

**Admin দিয়েছে:**
- কোনো Product permission নেই

**ফলাফল:**
- Product Index page সরাসরি access করলে → Login page
- উপরে ErrorMessage: "Unauthorized access! আপনার কাছে এই কাজ করার অনুমতি নেই"
- User এর session logout হয়ে যায়

### Scenario 3: User Create, Edit, Delete করতে পারে

**Admin দিয়েছে:**
- Product → Index ✓
- Product → Create ✓
- Product → Edit ✓
- Product → Delete ✓

**ফলাফল:**
- সকল buttons visible এবং active
- সকল operations possible
- UI তে কোনো restriction নেই

---

## কোড গাইড

### BaseController Helper Methods

```csharp
// ব্যবহারকারীর সকল permission পান
List<RolePermission> permissions = GetUserPermissions();

// নির্দিষ্ট permission check করুন
bool hasAccess = HasPermission("Product", "Edit");

// View এ dynamic check
bool canEdit = ViewBag.CanAccess("Product", "Edit");
```

### View Example

**Product Card এ permission-based buttons:**

```html
@{
    bool canDetails = ViewBag.CanAccess("Product", "Details");
    bool canEdit = ViewBag.CanAccess("Product", "Edit");
    bool canDelete = ViewBag.CanAccess("Product", "Delete");
}

<div class="product-actions">
    @if (canDetails)
    {
        <a href="@Url.Action("Details", ...)" class="btn">Details</a>
    }
    
    @if (canEdit)
    {
        <a href="@Url.Action("Edit", ...)" class="btn">Edit</a>
    }
    
    @if (canDelete)
    {
        <a href="@Url.Action("Delete", ...)" class="btn">Delete</a>
    }
    
    @if (!canDetails && !canEdit && !canDelete)
    {
        <div>আপনার কাছে কোনো অনুমতি নেই</div>
    }
</div>
```

---

## Permission Matrix

### সমর্থিত Controllers

1. **Product**
2. **ProductCategory**
3. **Order**

### সমর্থিত Actions

1. **Index** - তালিকা দেখা
2. **Create** - নতুন item তৈরি
3. **Edit** - item সংশোধন
4. **Delete** - item মুছে ফেলা
5. **Details** - item বিস্তারিত দেখা

---

## Database Structure

### RolePermission Table

```sql
CREATE TABLE RolePermissions (
    Id INT PRIMARY KEY,
    RoleId NVARCHAR(128),
    ControllerName NVARCHAR(50),
    ActionName NVARCHAR(50)
)
```

**Example Data:**
```
Id | RoleId                           | ControllerName | ActionName
1  | fb3bcc42-0d4f-496e-8e8f-3f2c... | Product        | Index
2  | fb3bcc42-0d4f-496e-8e8f-3f2c... | Product        | Details
3  | 123abc45-0d4f-496e-8e8f-3f2c... | Order          | Index
```

---

## Workflow Diagram

```
User Tries to Access Action
        ↓
[CustomAuthorize Filter]
        ↓
    Is User Logged In?
    ├─ NO  → Redirect to Login
    └─ YES → Continue
             ↓
        Does User's Role Have Permission?
        ├─ NO  → Sign Out + Redirect to Login
        └─ YES → Execute Action
                 ↓
            BaseController Loads Permissions
                 ↓
            View Renders with Permission Checks
                 ↓
            Show/Hide Buttons Based on Permissions
```

---

## নিরাপত্তা বৈশিষ্ট্য

1. **Server-side Validation**: আপনি যতক্ষণ না permission পান, Action কখনো execute হবে না
2. **Session Logout**: Unauthorized attempt এ session automatically logout হয়
3. **Direct URL Protection**: Direct URL থেকেও protected
4. **No Silent Failures**: স্পষ্ট error message দেওয়া হয়

---

## সাধারণ সমস্যা এবং সমাধান

### সমস্যা 1: User সঠিক permission থাকলেও button disabled দেখা যাচ্ছে

**সমাধান:**
- Permissions এ সঠিক Controller এবং Action নাম আছে কিনা চেক করুন
- Case-sensitive: "Product" ✓, "product" ✗
- Controller name এ "Controller" suffix রাখবেন না: "Product" ✓, "ProductController" ✗

### সমস্যা 2: User button ক্লিক করলে logout হয়ে যাচ্ছে

**সমাধান:**
- Admin panel এ গিয়ে User এর Role চেক করুন
- সেই Action এর permission আছে কিনা দেখুন
- Permission যোগ করুন এবং পুনরায় চেষ্টা করুন

### সমস্যা 3: একটি নতুন Controller/Action যোগ করা হয়েছে, কিন্তু Permission Management এ দেখা যাচ্ছে না

**সমাধান:**
- নতুন Controller এর নাম `ManagePermission.cshtml` এর `GetControllerActions()` method এ যোগ করুন:

```csharp
string[] allowedControllers = { "ProductCategory", "Product", "Order", "YourNewController" };
```

---

## Best Practices

1. **Least Privilege Principle**: ব্যবহারকারীকে শুধুমাত্র প্রয়োজনীয় permissions দিন
2. **Role Naming**: সুন্দর নাম দিন যা permission বোঝায় (e.g., "Product Editor", "Order Viewer")
3. **Regular Audit**: নিয়মিত check করুন কার কাছে কি permission আছে
4. **Document Access**: কোন user কোন role পাবে তা ডকুমেন্ট করুন

---

## Testing

### Test Case 1: Permission-based UI visibility
```
1. Admin লগইন করুন
2. একটি User তৈরি করুন নাম "viewer"
3. "Viewer" Role তৈরি করুন
4. শুধু Product→Details permission দিন
5. "viewer" user কে "Viewer" Role assign করুন
6. "viewer" দিয়ে লগইন করুন
7. Product page খুলুন
   ✓ Details button দেখা যাওয়া উচিত
   ✗ Create, Edit, Delete button disabled
```

### Test Case 2: Direct URL access protection
```
1. "viewer" user দিয়ে লগইন করুন (শুধু Details permission)
2. সরাসরি URL এ /Product/Edit/1 access করুন
3. ফলাফল: Login page এ redirect হবে + Error message দেখাবে
```

---

## সংক্ষিপ্ত সংস্করণ

**কীভাবে কাজ করে:**
1. Admin → Role তৈরি → Permission দেয়
2. Admin → User কে Role assign করে
3. User → লগইন করে → শুধু যা permission আছে তা করতে পারে
4. নো permission → Automatic logout + Login page redirect

**মূলমন্ত্র:**
- ✓ Permission আছে → সবকিছু সম্ভব
- ✗ Permission নেই → Login page (session sign out হয়ে যায়)
