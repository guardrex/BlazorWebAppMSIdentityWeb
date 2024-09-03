# BlazorWebAppMSIdentityWeb

Prototype :t-rex: ***HACKS*** :see_no_evil: ... :warning: USE AT YOUR OWN RISK! :warning:

This is a hacked version of the official BWA+OIDC app that switches over to MS Identity Web. The user's AD groups and AD built-in Admin Roles don't come in via the auth cookie. They're obtained via a separate Graph SDK call, so this won't explode when hosting on IIS with a long header for users with many AD groups/roles.

This app isn't recommended for production use. AFAIK, an official BWA + MS Identity Web sample app is in the works, and we'll add article coverage for it when we post it to the Blazor samples repo.

* Go into the Azure portal app registration and reset the `groupMembershipClaims` back to `null`:

  ```json
  "groupMembershipClaims": null,
  ```

  Hit the **Save** button to save the updated manifest.

* If you need to implement the AD built-in Administrator Roles piece, which is included in my sample, then the app is going to need ***delegated*** `RoleManagement.Read.Directory` permission in the app's registration. That one requires admin consent, so make sure you provide that after you add the scope.

  If you aren't doing anything with built-in Admin Roles, then you don't need that permission for the app in the portal. You can strip out the code in the sample app that tries to set the `directoryRole` claims.

The key parts of the sample to pay attention to are ...

* Packages: You need `Microsoft.Identity.Web` and `Microsoft.Identity.Web.GraphServiceClient`.

* App settings in the ***server project*** (not for the `.Client` project!). Place these settings into the `appsettings.json` file and configure it ............

  ```json
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "Domain": "... (example: contoso.onmicrosoft.com)",
    "TenantId": "...",
    "ClientId": "...",
    "ClientSecret": "...",
    "CallbackPath": "/signin-oidc"
  },
  "DownstreamApi": {
    "BaseUrl": "https://graph.microsoft.com/v1.0",
    "Scopes": "user.read"
  }
  ```

* Examine the `Program.cs` file changes ... and don't forget that the cookie policy must be enabled (`app.UseCookiePolicy();`), as my sample app does.

* One line changes in the `LoginLogoutEndpointRouteBuilderExtensions.cs` file in `MapLoginAndLogout`:

  ```diff
  - group.MapPost("/logout", ([FromForm] string? returnUrl) => TypedResults.SignOut(GetAuthProperties(returnUrl),
  -     ["Cookies", "MicrosoftOidc"]));
  + group.MapPost("/logout", ([FromForm] string? returnUrl) => 
  +     TypedResults.SignOut(GetAuthProperties(returnUrl), [ OpenIdConnectDefaults.AuthenticationScheme ]));
  ```

* See the updated `UserData.cs` file in the client app.

* See the updated `PersistingAuthenticationStateProvider.cs` file in the server app.

* I also don't have the user's name populated next to the **Logout** button in `LogInOrOut.razor` in a stable way between server and client, so I'm removing it for this sample ...

  ```diff
  - <span class="bi bi-arrow-bar-left-nav-menu" aria-hidden="true"></span> Logout @context.User.Identity?.Name
  + <span class="bi bi-arrow-bar-left-nav-menu" aria-hidden="true"></span> Logout
  ```
