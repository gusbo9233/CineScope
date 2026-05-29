# 🎯 You Already Have an App Service! (Simplified)

## Your Setup
```
✅ App Service: CineScope
✅ Resource Group: cinema
✅ Region: Sweden Central
✅ URL: https://cinescope-dwf0b2dte5awg4g7.swedencentral-01.azurewebsites.net
```

---

## ⚡ Quick Deploy (3 Steps)

### Step 1: Create Entra ID App (5 min)
```
Azure Portal → Azure AD → App registrations → New registration
Name: CineScope
Redirect URI: https://cinescope-dwf0b2dte5awg4g7.swedencentral-01.azurewebsites.net/signin-oidc
```
**Save:** Client ID, Tenant ID, Client Secret

### Step 2: Deploy Your Code (3 min)
```powershell
cd C:\Users\gusta\source\repos\CineScope
.\deploy-to-existing.ps1
```

### Step 3: Configure App Service (2 min)
```
Azure Portal → App Service (CineScope) → Configuration → Application settings

Add these settings (use values from Step 1):
- AzureAd:Instance = https://login.microsoftonline.com/
- AzureAd:TenantId = [Your Tenant ID]
- AzureAd:ClientId = [Your Client ID]
- AzureAd:ClientSecret = [Your Client Secret]
- AzureAd:CallbackPath = /signin-oidc
- AzureAd:SignedOutCallbackPath = /signout-callback-oidc

Click Save → Restart App Service
```

---

## 🧪 Test It
```
https://cinescope-dwf0b2dte5awg4g7.swedencentral-01.azurewebsites.net/Account/Login
```

**That's it! 🎉**

---

## 📋 What You Get

✅ **Login with Microsoft** - SSO authentication  
✅ **Role-Based Access** - Admin/Member/Guest  
✅ **Admin Dashboard** - At `/Admin/Dashboard`  
✅ **User Profile** - At `/Account/Profile`  
✅ **Secure Setup** - No credentials in code  

---

## 📚 Documentation
- **`DEPLOY_EXISTING_APPSERVICE.md`** - Detailed step-by-step (this has all the screenshots/details)
- **`deploy-to-existing.ps1`** - One-command deploy script
- **`AUTHENTICATION_QUICK_REFERENCE.md`** - Code examples

---

## 🚀 Run Deploy Now!

```powershell
cd C:\Users\gusta\source\repos\CineScope
.\deploy-to-existing.ps1
```

Then follow Step 3 above in Azure Portal.

---

**Next:** Create your Entra ID app registration → Run deploy script → Add credentials to App Service! 🎬
