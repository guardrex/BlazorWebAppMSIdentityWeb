using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using System.Runtime.CompilerServices;
using System.Security.Claims;

namespace BlazorWebAppMSIdentityWeb.Client;

// Add properties to this class and update the server and client AuthenticationStateProviders
// to expose more information about the authenticated user to the client.
public sealed class UserInfo()
{
    public required string UserId { get; init; }
    public required string Name { get; init; }
    public required string[] Roles { get; init; }
    public required string[] Groups { get; init; }
    public required string[] DirectoryRoles { get; init; }
    public required string City { get; init; }

    public const string UserIdClaimType = "sub";
    public const string NameClaimType = "name";
    private const string RoleClaimType = "roles";
    private const string GroupsClaimType = "groups";
    private const string DirectoryRoleClaimType = "directoryRole";
    private const string LocalityClaimType = "http://schemas.xmlsoap.org/claims/locality";

    public static UserInfo FromClaimsPrincipal(ClaimsPrincipal principal) =>
        new()
        {
            UserId = GetRequiredClaim(principal, UserIdClaimType),
            Name = GetRequiredClaim(principal, NameClaimType),
            Roles = principal.FindAll(RoleClaimType).Select(c => c.Value)
                .ToArray(),
            Groups = principal.FindAll(GroupsClaimType).Select(c => c.Value)
                .ToArray(),
            DirectoryRoles = principal.FindAll(DirectoryRoleClaimType).Select(c => c.Value)
                .ToArray(),
            City = GetRequiredClaim(principal, LocalityClaimType),
        };

    public ClaimsPrincipal ToClaimsPrincipal() =>
        new(new ClaimsIdentity(
            Roles.Select(role => new Claim(RoleClaimType, role))
                .Concat(Groups.Select(group => new Claim(GroupsClaimType, group)))
                .Concat(DirectoryRoles.Select(directoryRole => new Claim(DirectoryRoleClaimType, directoryRole)))
                .Concat([
                    new Claim(UserIdClaimType, UserId),
                    new Claim(NameClaimType, Name),
                    new Claim("city", City),
                ]),
            authenticationType: nameof(UserInfo),
            nameType: NameClaimType,
            roleType: RoleClaimType));

    private static string GetRequiredClaim(ClaimsPrincipal principal,
        string claimType) =>
            principal.FindFirst(claimType)?.Value ??
            throw new InvalidOperationException(
                $"Could not find required '{claimType}' claim.");

}
