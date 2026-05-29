# Deploy to Existing CineScope App Service
# This script publishes the application to your existing App Service

$resourceGroup = "cinema"
$appServiceName = "CineScope"
$buildConfig = "Release"

Write-Host "CineScope - Deploy to Existing App Service" -ForegroundColor Green
Write-Host "========================================`n" -ForegroundColor Green

# Step 1: Build
Write-Host "Step 1: Building application..." -ForegroundColor Cyan
dotnet build CineScope\CineScope.csproj -c $buildConfig
if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed!" -ForegroundColor Red
    exit 1
}
Write-Host "✅ Build successful`n" -ForegroundColor Green

# Step 2: Publish
Write-Host "Step 2: Publishing application..." -ForegroundColor Cyan
dotnet publish CineScope\CineScope.csproj -c $buildConfig -o ./publish
if ($LASTEXITCODE -ne 0) {
    Write-Host "Publish failed!" -ForegroundColor Red
    exit 1
}
Write-Host "✅ Publish successful`n" -ForegroundColor Green

# Step 3: Create deployment package
Write-Host "Step 3: Creating deployment package..." -ForegroundColor Cyan
$zipPath = "cinescope-deploy.zip"
if (Test-Path $zipPath) {
    Remove-Item $zipPath -Force
}
Compress-Archive -Path publish\* -DestinationPath $zipPath
Write-Host "✅ Package created: $zipPath`n" -ForegroundColor Green

# Step 4: Deploy to App Service
Write-Host "Step 4: Deploying to App Service..." -ForegroundColor Cyan
Write-Host "Resource Group: $resourceGroup" -ForegroundColor Yellow
Write-Host "App Service: $appServiceName" -ForegroundColor Yellow

az webapp deployment source config-zip `
  --resource-group $resourceGroup `
  --name $appServiceName `
  --src $zipPath

if ($LASTEXITCODE -ne 0) {
    Write-Host "Deployment failed!" -ForegroundColor Red
    exit 1
}

Write-Host "`n✅ Deployment successful!`n" -ForegroundColor Green

# Get app URL
$appUrl = az webapp show `
  --resource-group $resourceGroup `
  --name $appServiceName `
  --query "defaultHostName" `
  -o tsv

Write-Host "Application URL: https://$appUrl" -ForegroundColor Green
Write-Host "Login page: https://$appUrl/Account/Login" -ForegroundColor Green
Write-Host "`n📚 Next steps:" -ForegroundColor Yellow
Write-Host "1. Add your Entra ID credentials to App Service Configuration"
Write-Host "2. Go to Azure Portal → App Service → Configuration → Application settings"
Write-Host "3. Add settings from DEPLOY_EXISTING_APPSERVICE.md (Step 4)"
Write-Host "4. Assign roles to users in Enterprise applications"
Write-Host "5. Test login at: https://$appUrl/Account/Login`n"

# Cleanup
Write-Host "Cleaning up publish folder..." -ForegroundColor Gray
Remove-Item -Path publish -Recurse -Force
Write-Host "✅ Done!" -ForegroundColor Green
