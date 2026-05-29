# CineScope Azure Authentication - Quick Reference

## 🎯 User Types & Roles

| Role | Access Level | Can Access |
|------|--------------|-----------|
| **Guest** | Authenticated | Login, Profile, Movies |
| **Member** | Authenticated | Guest + Member features |
| **Admin** | Administrator | Member + Admin Dashboard |

---

## 📁 File Structure

```
CineScope/
├── Pages/
│   ├── Account/
│   │   ├── Login.cshtml          (Public login page)
│   │   ├── Login.cshtml.cs       (Login model)
│   │   ├── Profile.cshtml        (User profile - requires auth)
│   │   ├── Profile.cshtml.cs     (Profile model)
│   │   └── Signout.cshtml.cs     (Sign-out handler)
│   │
│   └── Admin/
│       ├── Dashboard.cshtml      (Admin only - requires "Admin" role)
│       └── Dashboard.cshtml.cs   (Admin dashboard model)
│
├── Views/
│   └── Shared/
│       └── _Layout.cshtml        (Updated with auth UI)
│
├── Program.cs                    (Authentication configured)
└── appsettings.json              (Azure AD settings)

infra/
└── main.bicep                    (Infrastructure as Code)

deploy.ps1                        (Deployment script)
DEPLOYMENT_GUIDE.md              (Complete setup instructions)
```

---

## 🔐 Authorization Attributes

### Require Authentication
```csharp
[Authorize]
public class MyPageModel : PageModel { }
```

### Require Admin Role
```csharp
[Authorize(Policy = "Admin")]
public class AdminPageModel : PageModel { }
```

### Require Member or Admin Role
```csharp
[Authorize(Policy = "Member")]
public class MemberPageModel : PageModel { }
```

### Allow Any Authenticated User
```csharp
[Authorize(Policy = "Guest")]
public class UserPageModel : PageModel { }
```

---

## 🔗 Key Pages

| Page | URL | Access |
|------|-----|--------|
| Login | `/Account/Login` | Public |
| Profile | `/Account/Profile` | Authenticated Users |
| Admin Dashboard | `/Admin/Dashboard` | Admins Only |
| Signout | `/Account/Signout` | Authenticated Users |

---

## 🚀 Check User Info in Razor Pages

```csharp
// Check if authenticated
if (User?.Identity?.IsAuthenticated == true)
{
    // Get username
    var username = User.Identity.Name;
    
    // Get email
    var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
    
    // Check if user is admin
    var isAdmin = User.IsInRole("Admin");
    
    // Get all roles
    var roles = User.FindAll(System.Security.Claims.ClaimTypes.Role);
}
```

---

## 📊 Deployment Checklist

- [ ] Create Entra ID App Registration
- [ ] Configure redirect URI
- [ ] Create client secret
- [ ] Get Client ID
- [ ] Get Tenant ID
- [ ] Create app roles (Admin, Member, Guest)
- [ ] Run `.\deploy.ps1`
- [ ] Add secrets to Key Vault
- [ ] Assign roles to users in Azure Portal
- [ ] Publish application to App Service
- [ ] Test login flow
- [ ] Test admin access
- [ ] Test role-based access control

---

## 🆘 Common Issues

**Issue:** "AADSTS500113: No sign-in name identified"
- **Solution:** Ensure redirect URI matches exactly: `https://<app-name>.azurewebsites.net/signin-oidc`

**Issue:** "User is not assigned to a role"
- **Solution:** Go to Azure Portal → Enterprise Applications → Your App → Users and groups → Add user and select role

**Issue:** "Key Vault access denied"
- **Solution:** Check App Service has System Assigned Identity, and Key Vault grants access policy permissions

**Issue:** "Connection string failed"
- **Solution:** Verify SQL Server firewall allows Azure App Services and test connection string

---

## 🎨 Customization

### Add Custom Claims
```csharp
// In Program.cs
builder.Services.Configure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options =>
{
    options.Scope.Add("custom_scope");
});
```

### Add Custom Policies
```csharp
// In Program.cs
options.AddPolicy("CustomPolicy", policy =>
    policy.RequireAssertion(context =>
        context.User.HasClaim(c => c.Type == "custom_claim")));
```

### Protect API Endpoints
```csharp
[Authorize(Policy = "Admin")]
[ApiController]
[Route("api/[controller]")]
public class AdminApiController : ControllerBase { }
```

---

## 📞 Support

- Docs: DEPLOYMENT_GUIDE.md
- Issues: Check troubleshooting section
- Resources: See links in DEPLOYMENT_GUIDE.md

---

**Last Updated:** 2026-05-29
**Version:** 1.0
