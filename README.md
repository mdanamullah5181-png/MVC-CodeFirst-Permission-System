 # 🚀 MVC-CodeFirst-Permission-System

![License](https://img.shields.io/badge/License-MIT-blue.svg)
![.NET Framework](https://img.shields.io/badge/.NET-Framework%204.7.2-blueviolet)
![ASP.NET MVC](https://img.shields.io/badge/ASP.NET-MVC%205-green)

A professional **ASP.NET MVC 5** application with comprehensive **Role-Based Permission System** for managing Products, Orders, and Categories using Entity Framework Code First.

---

## ✨ Features

| Feature | Description |
|---------|-------------|
| 🔐 **Permission System** | Fine-grained role-based access control |
| 📦 **Product Management** | Create, read, update, delete products |
| 📋 **Order Management** | Complete order tracking and management |
| 👥 **User & Role Management** | Flexible role creation and assignment |
| 🛡️ **Server-side Security** | CustomAuthorize filter for protection |
| 🗄️ **Code First ORM** | Entity Framework with migrations |

---

## 🛠️ Tech Stack

<div align="center">

| Technology | Version |
|-----------|---------|
| ASP.NET MVC | 5.0 |
| Entity Framework | 6.0 |
| SQL Server | 2019+ |
| ASP.NET Identity | 2.2 |
| Bootstrap | 4.0 |

</div>

---

## 📥 Installation

### Prerequisites
- Visual Studio 2019+
- .NET Framework 4.7.2+
- SQL Server

### Setup Steps

```bash
# 1. Clone the repository
git clone https://github.com/yourusername/MVC-CodeFirst-Permission-System.git
cd MVC-CodeFirst

# 2. Open in Visual Studio
# File → Open → Project/Solution

# 3. Restore NuGet packages
Update-Package -Reinstall

# 4. Update database
Update-Database

# 5. Build solution
Ctrl + Shift + B
```

---

## 🚀 Quick Start

1. **Run the application** (Press `F5`)
2. **Login** with admin credentials:
   ```
   Email: admin@example.com
   Password: Admin@123
   ```
3. **Navigate** to Role Management
4. **Create roles** and assign permissions
5. **Manage users** and assign roles

---

## 📊 Permission System Architecture

```
┌────────────────────────────┐
│    User Request            │
└────────────┬───────────────┘
             │
             ▼
┌────────────────────────────┐
│ CustomAuthorize Filter     │
│ • Check if logged in       │
│ • Check role permissions   │
│ • Redirect if unauthorized│
└────────────┬───────────────┘
             │
             ▼
┌────────────────────────────┐
│ Load Permissions to View   │
└────────────┬───────────────┘
             │
             ▼
┌────────────────────────────┐
│ View Rendering             │
│ • Show/Hide buttons        │
│ • Enable/Disable actions   │
└────────────────────────────┘
```

---

## 📋 Permission Matrix Example

```
┌──────────────┬────────┬────────┬──────┬────────┬─────────┐
│ Module       │ Index  │ Create │ Edit │ Delete │ Details │
├──────────────┼────────┼────────┼──────┼────────┼─────────┤
│ Product      │   ✅   │   ❌   │  ❌  │   ❌   │    ✅   │
│ Order        │   ✅   │   ✅   │  ✅  │   ❌   │    ✅   │
│ Category     │   ✅   │   ❌   │  ❌  │   ❌   │    ✅   │
└──────────────┴────────┴────────┴──────┴────────┴─────────┘
```

---

## 📁 Project Structure

```
MVC_CodeFirst/
│
├── 🗂️ App_Start/
│   ├── BundleConfig.cs
│   ├── FilterConfig.cs
│   └── RouteConfig.cs
│
├── 🎮 Controllers/
│   ├── BaseController.cs          ⭐ Permission helpers
│   ├── AccountController.cs
│   ├── ProductController.cs
│   ├── OrderController.cs
│   └── RoleController.cs
│
├── 🔒 Filters/
│   └── CustomAuthorize.cs         ⭐ Authorization filter
│
├── 📦 Models/
│   ├── Product.cs
│   ├── Order.cs
│   ├── RolePermission.cs
│   └── ApplicationDbContext.cs
│
├── 🎨 Views/
│   ├── Product/
│   ├── Order/
│   ├── Role/
│   └── Account/
│
├── 🗄️ Migrations/
│   └── [Database migrations]
│
└── 📄 Configuration
    ├── Web.config
    └── packages.config
```

---

## 🔑 Key Components

### 1️⃣ CustomAuthorize Filter
```csharp
[CustomAuthorize]
public ActionResult Details(int? id)
{
    // Only accessible users with permission
}
```

### 2️⃣ BaseController Methods
```csharp
// Get user permissions
var permissions = GetUserPermissions();

// Check specific permission
if (HasPermission("Product", "Edit")) { }

// In View
@if (ViewBag.CanAccess("Product", "Delete")) { }
```

---

## 🔧 How to Use

### Create New Role
1. Login as Admin
2. Role Management → New Role
3. Assign Name (e.g., "Editor", "Viewer")
4. Save

### Assign Permissions to Role
1. Role Management → Your Role → Manage Permission
2. Check boxes for desired actions
3. Save

### Create User & Assign Role
1. User Management → New User
2. Enter details
3. Assign Role
4. User gets all permissions of that role

---

## 📝 Usage Examples

### Check Permission in View
```html
<!-- Show button only if user has permission -->
@if (ViewBag.CanAccess("Product", "Delete"))
{
    <a href="@Url.Action("Delete", "Product")" class="btn btn-danger">
        Delete
    </a>
}
```

### Add Permission Check to Controller
```csharp
[HttpPost]
[CustomAuthorize]
public ActionResult Create(Product product)
{
    // Only authorized users reach here
    db.Products.Add(product);
    db.SaveChanges();
    return RedirectToAction("Index");
}
```

---

## ⚙️ Configuration

### Database Connection
Edit `Web.config`:
```xml
<connectionStrings>
    <add name="DefaultConnection" 
         connectionString="Server=YOUR_SERVER;Database=MVC_CodeFirst_DB;Integrated Security=true;" 
         providerName="System.Data.SqlClient" />
</connectionStrings>
```

### Default Admin User
Change in `Startup.Auth.cs` if needed

---

## 🐛 Troubleshooting

| Issue | Solution |
|-------|----------|
| Database not connecting | Check connection string in Web.config |
| Cannot login | Run `Update-Database` to create tables |
| Permissions not working | Clear cookies, logout and re-login |
| NuGet errors | Run `Update-Package -Reinstall` |

---

## 📚 Documentation Files

- 📖 `PERMISSION_SYSTEM.md` - Complete system guide
- 📖 `IMPLEMENTATION_GUIDE.md` - Implementation examples
- 📖 `QUICK_REFERENCE.md` - Quick reference card

---

## 📄 License

MIT License - See LICENSE file for details

---

## 👤 Author

**Md Anamullah**  
📧 Email: mohammadsayem570@gmail.com  
🔗 GitHub: https://github.com/mdanamullah5181-png

---

## 💬 Support

For issues, questions, or contributions:
- 📝 Create an Issue
- 🔗 Submit a Pull Request
- 📧 Contact via email

---

<div align="center">

⭐ If this project helped you, please give it a star!

Made with ❤️ by Md Anamullah

</div>
