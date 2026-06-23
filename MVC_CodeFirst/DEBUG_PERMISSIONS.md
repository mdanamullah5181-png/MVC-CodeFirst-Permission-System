# Permission Debugging Guide

## Step 1: Check Database Data

কমান্ড এ সম্পূর্ণ project path e চলান:

```sql
-- SQL Server এ এই queries run করুন:

-- Check if permissions exist
SELECT * FROM RolePermissions;

-- Check user roles
SELECT u.UserName, r.Name as RoleName FROM AspNetUsers u
INNER JOIN AspNetUserRoles ur ON u.Id = ur.UserId
INNER JOIN AspNetRoles r ON ur.RoleId = r.Id;

-- Check specific user's permissions
SELECT u.UserName, r.Name as RoleName, rp.ControllerName, rp.ActionName 
FROM AspNetUsers u
INNER JOIN AspNetUserRoles ur ON u.Id = ur.UserId
INNER JOIN AspNetRoles r ON ur.RoleId = r.Id
LEFT JOIN RolePermissions rp ON r.Id = rp.RoleId
WHERE u.UserName = 'admin@example.com'; -- আপনার admin username দিন
```

## Step 2: Verify in Application

Admin দিয়ে login করে:
1. Role Management এ যান
2. Admin role select করুন
3. **Manage Permission** বাটনে click করুন
4. সব checkboxes checked আছে কিনা দেখুন
5. Update Permissions বাটনে click করুন

## Step 3: Test Access

1. Product/Index এ যান
2. Details button click করুন
3. আগে যদি লগইন page আসতো, এখন details page আসা উচিত

## Common Issues

### Issue 1: SavePermission POST হচ্ছে না
- View form properly submit হচ্ছে কিনা check করুন
- ManagePermission.cshtml এ checkboxes properly selected আছে কিনা

### Issue 2: ControllerName case mismatch
- GetControllerActions() এ "Product" defined, কিন্তু database এ "product" saved হতে পারে
- Exact case matching চেক করুন

### Issue 3: ActionName case issue
- "Index" vs "index" - exact case দিতে হবে
