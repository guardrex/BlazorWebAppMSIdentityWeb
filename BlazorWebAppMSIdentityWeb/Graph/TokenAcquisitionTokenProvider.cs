using System.Security.Claims;
using Microsoft.Identity.Web;
using Microsoft.Kiota.Abstractions.Authentication;

namespace BlazorWebAppMSIdentityWeb.Graph;

public class TokenAcquisitionTokenProvider(
    ITokenAcquisition tokenAcquisition,
    string[] scopes,
    ClaimsPrincipal? user) : IAccessTokenProvider
{
    private readonly string[] validHosts =
    [
        "graph.microsoft.com",
        "graph.microsoft.us",
        "dod-graph.microsoft.us",
        "graph.microsoft.de",
        "microsoftgraph.chinacloudapi.cn",
    ];

    private readonly ClaimsPrincipal user = user ?? throw new Exception("User claims principal is required.");

    public AllowedHostsValidator AllowedHostsValidator => new(validHosts);

    public async Task<string> GetAuthorizationTokenAsync(
        Uri uri,
        Dictionary<string, object>? additionalAuthenticationContext = null,
        CancellationToken cancellationToken = default)
    {
        if (!AllowedHostsValidator.IsUrlHostValid(uri))
        {
            return string.Empty;
        }

        if (uri.Scheme != "https")
        {
            throw new Exception("URL must use https.");
        }

        return await tokenAcquisition.GetAccessTokenForUserAsync(scopes, user: user);
    }
}
