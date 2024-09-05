# Blazor Web App with Microsoft Identity Web

This sample features:

- A Blazor Web App with global Auto interactivity.
  - This adds a `PersistingAuthenticationStateProvider` and `PersistentAuthenticationStateProvider` services to the
    server and client Blazor apps respectively to capture authentication state and flow it between the server and client.
- OIDC authentication with Microsoft Entra without using Entra-specific packages.
  - The goal is that this sample can be used as a starting point for any OIDC authentication flow.
- Automatic non-interactive token refresh with the help of a custom `CookieOidcRefresher`.
- Microsoft Graph to obtain AD groups and built-in Azure Administrator Roles.

## Run the sample

### Azure portal

Make sure that the ME-ID app registration has `User.Read` and `RoleManagement.Read.Directory` permissions assigned.

### Visual Studio

1. Open the solution file in Visual Studio.
1. Configure the Identity provider in the `appsettings.json` file.
1. Select the server project in **Solution Explorer** and start the app with either Visual Studio's Run button or by selecting **Start Debugging** from the **Debug** menu.

### .NET CLI

1. Configure the Identity provider in the `appsettings.json` file.
1. In a command shell, navigate to the server project folder and use the `dotnet run` command to run the sample.
