using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using Microsoft.Graph;
using Microsoft.Kiota.Abstractions.Authentication;
using BlazorWebAppMSIdentityWeb;
using BlazorWebAppMSIdentityWeb.Client.Weather;
using BlazorWebAppMSIdentityWeb.Components;
using BlazorWebAppMSIdentityWeb.Graph;
using BlazorWebAppMSIdentityWeb.Weather;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(options =>
    {
        builder.Configuration.Bind("AzureAd", options);

        options.Prompt = "select_account";

        options.Events.OnTokenValidated = async context =>
        {
            var tokenAcquisition = context.HttpContext.RequestServices
                .GetRequiredService<ITokenAcquisition>();

            var graphClient = new GraphServiceClient(
                new BaseBearerTokenAuthenticationProvider(
                    new TokenAcquisitionTokenProvider(
                        tokenAcquisition,
                        builder.Configuration.GetValue<string[]>("DownstreamApi:Scopes") ?? [ "User.Read" ],
                        context.Principal)));
            
            var memberOf = graphClient.Me.MemberOf;

            var graphDirectoryRoles = await memberOf.GraphDirectoryRole.GetAsync();

            if (context.Principal?.Identity is not ClaimsIdentity identity)
            {
                // Log missing Identity?
                return;
            }

            if (graphDirectoryRoles?.Value is not null)
            {
                foreach (var entry in graphDirectoryRoles.Value)
                {
                    if (entry.RoleTemplateId is not null)
                    {
                        identity.AddClaim(
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
                        identity.AddClaim(
                            new Claim("groups", entry.Id));
                    }
                }
            }
        };
    })
    .EnableTokenAcquisitionToCallDownstreamApi(builder.Configuration.GetValue<string[]>("DownstreamApi:Scopes"))
    .AddMicrosoftGraph(builder.Configuration.GetSection("DownstreamApi"))
    .AddInMemoryTokenCaches();

builder.Services.Configure<MicrosoftIdentityOptions>(OpenIdConnectDefaults.AuthenticationScheme, oidcOptions =>
{
    oidcOptions.MapInboundClaims = false;
    oidcOptions.TokenValidationParameters.NameClaimType = JwtRegisteredClaimNames.Name;
    oidcOptions.TokenValidationParameters.RoleClaimType = "roles";
});

builder.Services.AddAuthorization();

builder.Services.AddCascadingAuthenticationState();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddScoped<AuthenticationStateProvider, PersistingAuthenticationStateProvider>();

builder.Services.AddScoped<IWeatherForecaster, ServerWeatherForecaster>();

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseAntiforgery();

app.MapGet("/weather-forecast", ([FromServices] IWeatherForecaster WeatherForecaster) =>
{
    return WeatherForecaster.GetWeatherForecastAsync();
}).RequireAuthorization();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(BlazorWebAppMSIdentityWeb.Client._Imports).Assembly);

app.MapGroup("/authentication").MapLoginAndLogout();

app.Run();
