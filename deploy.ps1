# Variables
$resourceGroupName = "rg-cinescope"
$location = "eastus"
$deploymentName = "cinescope-deployment-$(Get-Date -Format 'yyyyMMddHHmmss')"

# Create resource group
Write-Host "Creating resource group: $resourceGroupName..." -ForegroundColor Green
az group create `
  --name $resourceGroupName `
  --location $location

# Validate deployment
Write-Host "Validating Bicep template..." -ForegroundColor Green
az deployment group what-if `
  --resource-group $resourceGroupName `
  --template-file infra/main.bicep `
  --deployment-name $deploymentName

# Deploy
Write-Host "Deploying infrastructure..." -ForegroundColor Green
$deployment = az deployment group create `
  --resource-group $resourceGroupName `
  --template-file infra/main.bicep `
  --deployment-name $deploymentName | ConvertFrom-Json

# Extract outputs
$appServiceUrl = $deployment.properties.outputs.appServiceUrl.value
$appServiceName = $deployment.properties.outputs.appServiceName.value
$keyVaultUrl = $deployment.properties.outputs.keyVaultUrl.value

Write-Host "Deployment completed!" -ForegroundColor Green
Write-Host "App Service URL: $appServiceUrl"
Write-Host "App Service Name: $appServiceName"
Write-Host "Key Vault URL: $keyVaultUrl"

# Next steps
Write-Host "`nNext steps:" -ForegroundColor Yellow
Write-Host "1. Install NuGet packages in Visual Studio Package Manager Console:`n"
Write-Host "   Install-Package Microsoft.Identity.Web"
Write-Host "   Install-Package Microsoft.Identity.Web.UI`n"
Write-Host "2. Update Key Vault secrets with your values:`n"
Write-Host "   az keyvault secret set --vault-name <keyvault-name> --name 'AzureAdClientId' --value '<your-client-id>'"
Write-Host "   az keyvault secret set --vault-name <keyvault-name> --name 'AzureAdClientSecret' --value '<your-client-secret>'"
Write-Host "   az keyvault secret set --vault-name <keyvault-name> --name 'MovieContextConnectionString' --value '<your-connection-string>'`n"
Write-Host "3. Create Entra ID app registration in Azure Portal"
Write-Host "4. Publish to App Service using Visual Studio or Azure CLI"