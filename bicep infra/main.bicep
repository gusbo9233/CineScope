// Parameters
param location string = resourceGroup().location
param appServicePlanName string = 'asp-cinescope-${uniqueString(resourceGroup().id)}'
param webAppName string = 'app-cinescope-${uniqueString(resourceGroup().id)}'
param keyVaultName string = 'kv-cinescope-${uniqueString(resourceGroup().id)}'

// App Service Plan
resource appServicePlan 'Microsoft.Web/serverfarms@2024-04-01' = {
  name: appServicePlanName
  location: location
  sku: {
    name: 'B2'
    capacity: 1
  }
  properties: {
    reserved: false
  }
}

// App Service
resource webApp 'Microsoft.Web/sites@2024-04-01' = {
  name: webAppName
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
  }

  // Application Settings
  resource appSettings 'config' = {
    name: 'appsettings'
    properties: {
      ASPNETCORE_ENVIRONMENT: 'Production'
      'Authentication:Schemes:OpenIdConnect:Authority': 'https://login.microsoftonline.com/${subscription().tenantId}/v2.0'
      'Authentication:Schemes:OpenIdConnect:ClientId': '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=AzureAdClientId)'
      'Authentication:Schemes:OpenIdConnect:ClientSecret': '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=AzureAdClientSecret)'
      'Authentication:Schemes:OpenIdConnect:CallbackPath': '/signin-oidc'
    }
  }

  // Connection Strings
  resource connectionStrings 'config' = {
    name: 'connectionstrings'
    properties: {
      MovieContext: {
        value: '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=MovieContextConnectionString)'
        type: 'SQLServer'
      }
    }
  }
}

// Key Vault
resource keyVault 'Microsoft.KeyVault/vaults@2024-04-01-preview' = {
  name: keyVaultName
  location: location
  properties: {
    tenantId: subscription().tenantId
    sku: {
      family: 'A'
      name: 'standard'
    }
    accessPolicies: [
      {
        tenantId: subscription().tenantId
        objectId: webApp.identity.principalId
        permissions: {
          secrets: ['get', 'list']
        }
      }
    ]
    enablePurgeProtection: true
    enableSoftDelete: true
    softDeleteRetentionInDays: 7
  }
}

// App Service Authentication Extension
resource appServiceAuth 'Microsoft.Web/sites/config@2024-04-01' = {
  parent: webApp
  name: 'authsettingsV2'
  properties: {
    platform: {
      enabled: true
    }
    identityProviders: {
      azureActiveDirectory: {
        enabled: true
        registration: {
          openIdIssuer: 'https://login.microsoftonline.com/${subscription().tenantId}/v2.0'
          clientId: '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=AzureAdClientId)'
          clientSecretSettingName: 'MICROSOFT_PROVIDER_AUTHENTICATION_SECRET'
        }
        login: {
          disableWWWAuthenticate: false
        }
        validation: {
          allowedAudiences: [
            '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=AzureAdClientId)'
          ]
        }
      }
    }
    login: {
      routes: {
        logoutEndpoint: '/.auth/logout'
      }
    }
    httpSettings: {
      requireHttps: true
      routes: {
        apiPrefix: '/.auth'
      }
      forwardProxy: {
        convention: 'NoProxy'
      }
    }
    responseOverrides: {
      authentication: {
        redirect: '/login'
      }
    }
  }
}

// Outputs
output appServiceUrl string = 'https://${webApp.properties.defaultHostName}'
output appServiceName string = webApp.name
output keyVaultUrl string = keyVault.properties.vaultUri