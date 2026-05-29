# CineScope Azure Authentication Setup Guide

## Deployment Status: ✅ Complete

Your CineScope application is now configured for Azure App Service Authentication with three user types: **Guest, Member, and Admin**.

---

## 📋 What's Been Done

### 1. **Infrastructure Setup (Bicep)**
- ✅ Created `infra/main.bicep` - Infrastructure as Code for:
  - App Service Plan (B2)
  - App Service with System Managed Identity
  - Key Vault for secrets management
  - Configured for HTTPS only
  
### 2. **Application Configuration**
- ✅ Updated `Program.cs` with:
  - Microsoft Identity Web authentication
  - Role-based authorization policies (Admin, Member, Guest)
  - Proper middleware ordering
  
- ✅ Updated `appsettings.json` with Azure AD settings

### 3. **Razor Pages Created**
- ✅ `Pages/Account/Login.cshtml` - Login page
- ✅ `Pages/Account/Profile.cshtml` - User profile view with roles
- ✅ `Pages/Account/Signout.cshtml.cs` - Sign-out functionality

### 4. **NuGet Packages Installed**
- ✅ `Microsoft.Identity.Web` v2.21.0
- ✅ `Microsoft.Identity.Web.UI` v2.21.0

### 5. **Layout Updated**
- ✅ Added authentication UI to navbar
- ✅ User dropdown menu with Profile and Sign Out links
- ✅ Conditional display based on authentication status

---

## 🔐 Next Steps: Complete Azure Configuration

### Step 1: Create Entra ID (Azure AD) App Registration

1. Go to [Azure Portal](https://portal.azure.com)
2. Navigate to **Azure Active Directory** → **App registrations**
3. Click **+ New registration**
4. Fill in details:
   - **Name:** CineScope
   - **Supported account types:** Accounts in this organizational directory only
   - **Redirect URI:** Select "Web" and enter: `https://<your-app-name>.azurewebsites.net/signin-oidc`
5. Click **Register**

### Step 2: Get Client ID and Secret

1. In the app registration, go to **Overview**
2. Copy the **Application (client) ID** - you'll need this
3. Go to **Certificates & secrets** → **+ New client secret**
4. Add a secret with name "CineScope" (any duration)
5. Copy the **Value** - this is your client secret

### Step 3: Configure API Permissions

1. In the app registration, go to **API permissions**
2. Click **+ Add a permission**
3. Select **Microsoft Graph** → **Delegated permissions**
4. Search for and add:
   - `email`
   - `profile`
   - `openid`
5. Click **Grant admin consent**

### Step 4: Get Your Tenant ID

1. In Azure AD, go to **Properties**
2. Copy the **Tenant ID** (GUID)

### Step 5: Deploy Infrastructure

Run the deployment script in PowerShell:

```powershell
cd C:\Users\gusta\source\repos\CineScope
.\deploy.ps1
```

This will:
- Create a resource group: `rg-cinescope`
- Deploy App Service, App Service Plan, and Key Vault
- Output the resources created

**Note the Key Vault name from the output!**

### Step 6: Add Secrets to Key Vault

After deployment completes, add your secrets to Key Vault:

```powershell
$keyVaultName = "<from-deployment-output>"

# Add secrets
az keyvault secret set --vault-name $keyVaultName --name "AzureAdClientId" --value "<your-client-id>"
az keyvault secret set --vault-name $keyVaultName --name "AzureAdClientSecret" --value "<your-client-secret>"
az keyvault secret set --vault-name $keyVaultName --name "MovieContextConnectionString" --value "Server=tcp:glosify.database.windows.net,1433;Initial Catalog=CinescopeServer;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;Authentication=Active Directory Default;"
```

### Step 7: Configure App Roles (for Guest/Member/Admin)

1. Go back to your app registration
2. Go to **App roles** (under Manage section)
3. Click **+ Create app role**
4. Create three roles:

**Role 1: Admin**
- Display name: `Admin`
- Value: `Admin`
- Description: `Administrator access`
- Enabled: ✓

**Role 2: Member**
- Display name: `Member`
- Value: `Member`
- Description: `Regular member access`
- Enabled: ✓

**Role 3: Guest**
- Display name: `Guest`
- Value: `Guest`
- Description: `Guest user access`
- Enabled: ✓

### Step 8: Assign Roles to Users

1. Go to **Enterprise applications** → Find your app
2. Click **Users and groups**
3. For each user, click **+ Add user/group**
4. Select users and assign them roles

### Step 9: Publish to Azure

Option A - Using Visual Studio:
1. Right-click project → **Publish**
2. Select **Azure**
3. Select your App Service
4. Click **Publish**

Option B - Using Azure CLI:
```powershell
# Build the app
dotnet build CineScope\CineScope.csproj -c Release

# Create a deployment package
dotnet publish CineScope\CineScope.csproj -c Release -o ./publish

# Deploy to App Service
az webapp deployment source config-zip --resource-group rg-cinescope --name <your-app-name> --src ./publish.zip
```

### Step 10: Test Your App

1. Navigate to: `https://<your-app-name>.azurewebsites.net/Account/Login`
2. Click **Sign In with Microsoft**
3. Sign in with your Microsoft account
4. Check your profile to see assigned roles

---

## 🔑 Authorization in Your Pages

### Protected Pages - Admin Only

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

[Authorize(Policy = "Admin")]
public class AdminDashboardModel : PageModel
{
    public void OnGet()
    {
        // Only admins can access this
    }
}
```

### Protected Pages - Members + Admins

```csharp
[Authorize(Policy = "Member")]
public class MemberFeatureModel : PageModel
{
    // Accessible by members and admins
}
```

### Protected Pages - Any Authenticated User

```csharp
[Authorize]
public class ProfileModel : PageModel
{
    // Accessible by any logged-in user
}
```

---

## 📚 Resources

- [Microsoft Identity Web Documentation](https://github.com/AzureAD/microsoft-identity-web)
- [Azure App Service Authentication](https://learn.microsoft.com/en-us/azure/app-service/overview-authentication-authorization)
- [Entra ID App Roles](https://learn.microsoft.com/en-us/azure/active-directory/develop/howto-add-app-roles-in-apps)
- [Bicep Documentation](https://learn.microsoft.com/en-us/azure/azure-resource-manager/bicep/)

---

## 🆘 Troubleshooting

### "Invalid redirect URI"
- Make sure you registered the exact redirect URI in your app registration
- Format: `https://<your-app-name>.azurewebsites.net/signin-oidc`

### "Unauthorized" after login
- Verify user is assigned a role in "Enterprise applications"
- Check Key Vault secrets are set correctly

### Key Vault access denied
- Verify App Service has System Managed Identity enabled
- Check Key Vault access policy grants `Get` and `List` permissions

### Connection string issues
- Verify SQL Server allows Azure App Service connections
- Check firewall rules in SQL Database settings

---

## ✨ Architecture Overview

```
User Browser
    ↓
App Service (Razor Pages)
    ↓
Azure AD (Authentication)
    ↓
Entra ID Roles (Admin/Member/Guest)
    ↓
Authorize attributes check roles
    ↓
Key Vault (stores secrets securely)
    ↓
Azure SQL Database (CinescopeServer)
```

---

**Status:** Ready for deployment! 🚀
