using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using BlazorWebAppMSIdentityWeb.Client;
using Microsoft.Graph;
using System.Security.Claims;

namespace BlazorWebAppMSIdentityWeb;

// This is a server-side AuthenticationStateProvider that uses PersistentComponentState to flow the
// authentication state to the client which is then fixed for the lifetime of the WebAssembly application.
internal sealed class PersistingAuthenticationStateProvider : AuthenticationStateProvider, IHostEnvironmentAuthenticationStateProvider, IDisposable
{
    private readonly PersistentComponentState persistentComponentState;
    private readonly PersistingComponentStateSubscription subscription;
    private Task<AuthenticationState>? authenticationStateTask;

    private readonly GraphServiceClient graphServiceClient;

    public PersistingAuthenticationStateProvider(PersistentComponentState state, GraphServiceClient graphServiceClient)
    {
        this.graphServiceClient = graphServiceClient;
        persistentComponentState = state;
        subscription = state.RegisterOnPersisting(OnPersistingAsync, RenderMode.InteractiveWebAssembly);
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync() => authenticationStateTask ??
            throw new InvalidOperationException($"Do not call {nameof(GetAuthenticationStateAsync)} outside of the DI scope for a Razor component. Typically, this means you can call it only within a Razor component or inside another DI service that is resolved for a Razor component.");

    public void SetAuthenticationState(Task<AuthenticationState> task)
    {
        authenticationStateTask = task;
    }

    private async Task OnPersistingAsync()
    {
        var authenticationState = await GetAuthenticationStateAsync();
        var principal = authenticationState.User;

        if (principal.Identity?.IsAuthenticated == true)
        {
            var claimsIdentity = (ClaimsIdentity)principal.Identity;
            var memberOf = graphServiceClient.Me.MemberOf;

            var graphDirectoryRoles = await memberOf.GraphDirectoryRole.GetAsync();

            if (graphDirectoryRoles?.Value is not null)
            {
                foreach (var entry in graphDirectoryRoles.Value)
                {
                    if (entry.RoleTemplateId is not null)
                    {
                        claimsIdentity.AddClaim(
                            new Claim("directoryRole", entry.RoleTemplateId));
                    }
                }
            }

            var graphGroup = await memberOf.GraphGroup.GetAsync();

            if (graphGroup?.Value is not null)
            {
                foreach (var entry in graphGroup.Value)
                {
                    if (entry.Id is not null)
                    {
                        claimsIdentity.AddClaim(
                            new Claim("groups", entry.Id));
                    }
                }
            }

            principal = new ClaimsPrincipal(claimsIdentity);

            persistentComponentState.PersistAsJson(nameof(UserInfo), UserInfo.FromClaimsPrincipal(principal));
        }
    }

    public void Dispose()
    {
        subscription.Dispose();
    }
}
