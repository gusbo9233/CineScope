# 🚀 CineScope - Deploy to Existing App Service

## Your Existing Setup

✅ **App Service Name:** `CineScope`  
✅ **Resource Group:** `cinema`  
✅ **Location:** Sweden Central  
✅ **URL:** `https://cinescope-dwf0b2dte5awg4g7.swedencentral-01.azurewebsites.net`  
✅ **App Service Plan:** `plan-glosify`

---

## 📋 Deployment Steps (4 Simple Steps)

### Step 1: Create Entra ID App Registration

1. Go to [Azure Portal](https://portal.azure.com)
2. **Azure Active Directory** → **App registrations** → **+ New registration**
3. Fill in:
   - **Name:** CineScope
   - **Supported account types:** "Accounts in this organizational directory only"
   - **Redirect URI:** Select **Web** and enter:
     ```
     https://cinescope-dwf0b2dte5awg4g7.swedencentral-01.azurewebsites.net/signin-oidc
     ```
   - Click **Register**

4. Go to **Overview** and copy:
   - **Application (client) ID** ← Save this
   - **Directory (tenant) ID** ← Save this

5. Go to **Certificates & secrets**:
   - Click **+ New client secret**
   - Name: "CineScope-Secret"
   - Click **Add**
   - Copy the **Value** ← Save this

6. Go to **API permissions**:
   - Click **+ Add a permission**
   - **Microsoft Graph** → **Delegated permissions**
   - Search and add: `email`, `profile`, `openid`
   - Click **Grant admin consent**

### Step 2: Create App Roles

1. In your app registration, go to **App roles**
2. Click **+ Create app role** and create three roles:

| Role | Value | Description |
|------|-------|---|
| Admin | Admin | Administrator access |
| Member | Member | Regular member access |
| Guest | Guest | Guest user access |

3. Click **Create app role** for each

### Step 3: Deploy Your Application

**Option A: Using Visual Studio (Easiest)**

```
1. Open CineScope project in Visual Studio
2. Right-click project → Publish
3. Target: Azure
4. Specific target: Azure App Service (Linux)
5. Select:
   - Subscription: [Your subscription]
   - Resource group: cinema
   - App Service: CineScope
6. Click Finish
7. Click Publish
```

**Option B: Using Azure CLI**

```powershell
# Build the application
cd C:\Users\gusta\source\repos\CineScope
dotnet publish CineScope\CineScope.csproj -c Release -o ./publish

# Create zip file
cd publish
Compress-Archive -Path * -DestinationPath ../cinescope-deploy.zip

# Deploy to App Service
cd ..
az webapp deployment source config-zip `
  --resource-group cinema `
  --name CineScope `
  --src cinescope-deploy.zip
```

**Option C: Using Git Deployment (Recommended)**

```powershell
# Push to your GitHub repo (assuming you have git remote configured)
git push origin master

# Enable App Service deployment from GitHub
# 1. Go to Azure Portal → Your App Service → Deployment → Deployment Center
# 2. Select GitHub, authorize, select your repo and branch
# 3. It will auto-deploy on each push
```

### Step 4: Configure App Service Settings

1. Go to [Azure Portal](https://portal.azure.com)
2. Search for "CineScope" → Your App Service
3. Go to **Settings** → **Configuration**
4. Click **+ New application setting** and add these:

```
Name: AzureAd:Instance
Value: https://login.microsoftonline.com/

Name: AzureAd:TenantId
Value: [Your Directory (tenant) ID from Step 1]

Name: AzureAd:ClientId
Value: [Your Application (client) ID from Step 1]

Name: AzureAd:ClientSecret
Value: [Your client secret Value from Step 1]

Name: AzureAd:CallbackPath
Value: /signin-oidc

Name: AzureAd:SignedOutCallbackPath
Value: /signout-callback-oidc
```

5. Click **Save**
6. Click **Continue** when prompted to restart the app

---

## 🔐 Assign Roles to Users

1. Go to Azure Portal → **Enterprise applications**
2. Search for and select "CineScope"
3. Go to **Users and groups**
4. Click **+ Add user/group**
5. Select your users and assign them roles:
   - Some as "Admin"
   - Some as "Member"
   - Some as "Guest"
6. Click **Assign**

---

## ✅ Test Your Deployment

1. Navigate to: `https://cinescope-dwf0b2dte5awg4g7.swedencentral-01.azurewebsites.net/Account/Login`
2. Click **Sign In with Microsoft**
3. Sign in with your Microsoft account
4. You should see your profile with assigned roles
5. If you're an Admin, visit: `/Admin/Dashboard`

---

## 🚨 If App Service Shows Errors

**Check logs:**
```powershell
# Stream live logs from App Service
az webapp log tail --resource-group cinema --name CineScope
```

**Common Issues:**

| Error | Solution |
|-------|----------|
| "Invalid redirect URI" | Check the Redirect URI matches exactly: `https://cinescope-dwf0b2dte5awg4g7.swedencentral-01.azurewebsites.net/signin-oidc` |
| "Client secret expired" | Go to Entra ID app registration → Certificates & secrets → Create new secret |
| "Unauthorized after login" | User not assigned a role in Enterprise applications |
| "Application not found" | Restart App Service (Azure Portal → Restart) |

---

## 📝 Your Application Settings Reference

```json
{
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "TenantId": "PASTE_YOUR_TENANT_ID",
    "ClientId": "PASTE_YOUR_CLIENT_ID",
    "ClientSecret": "PASTE_YOUR_CLIENT_SECRET",
    "CallbackPath": "/signin-oidc",
    "SignedOutCallbackPath": "/signout-callback-oidc"
  },
  "ConnectionStrings": {
    "MovieContext": "Server=tcp:glosify.database.windows.net,1433;Initial Catalog=CinescopeServer;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;Authentication=Active Directory Default;"
  }
}
```

---

## ✨ What's Already Done for You

✅ Code is ready - authentication pages created  
✅ NuGet packages installed  
✅ Program.cs configured  
✅ Authorization policies set up  
✅ Admin dashboard created  
✅ Navbar updated with auth UI  

**All you need to do:** Add your Entra ID credentials to App Service settings!

---

## 🎯 Quick Checklist

- [ ] Created Entra ID App Registration
- [ ] Saved: Client ID, Tenant ID, Client Secret
- [ ] Created app roles (Admin, Member, Guest)
- [ ] Deployed application to App Service
- [ ] Added application settings to App Service
- [ ] Assigned roles to test users
- [ ] Tested login at `/Account/Login`
- [ ] Verified admin access to `/Admin/Dashboard`

---

## 🎉 You're Done!

Once you complete these 4 steps, your CineScope app will have:
- ✅ Enterprise authentication (Microsoft/Entra ID)
- ✅ Role-based access control (Admin/Member/Guest)
- ✅ Secure token management
- ✅ User profile page
- ✅ Admin dashboard

**Current Status:**
- ✅ Code: Ready
- ✅ App Service: Deployed
- ⏳ Entra ID: Waiting for you to create it
- ⏳ Configuration: Waiting for credentials

**Next:** Go to Azure Portal and create your Entra ID app registration! 🚀
