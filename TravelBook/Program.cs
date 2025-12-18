using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.IdentityModel.Tokens.Jwt;
using TravelBook.Components;
using TravelBook.Services;

string ReadSecret(string name)
{
    var path = $"/run/secrets/{name}";
    if (!File.Exists(path))
        throw new Exception($"Docker secret not found: {path}");

    return File.ReadAllText(path).Trim();
}

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents()
    .AddAuthenticationStateSerialization();

builder.Services.AddRazorPages();

var useEntraID = builder.Configuration["UseEntraID"];

if (!string.IsNullOrEmpty(useEntraID) &&
    useEntraID.Equals("True", StringComparison.InvariantCultureIgnoreCase))
{
    builder.Configuration["AzureAd:ClientSecret"] = ReadSecret("travelbook_azure_client_secret");
}

var scopesToRequest = new string[] { "profile", "user.read", "user.readwrite.all", "device.read.all" };

builder.Services
    .AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"))
    .EnableTokenAcquisitionToCallDownstreamApi(scopesToRequest)
    .AddMicrosoftGraph(builder.Configuration.GetSection("MicrosoftGraph"))
    .AddInMemoryTokenCaches();

builder.Services.Configure<CookieAuthenticationOptions>(
    CookieAuthenticationDefaults.AuthenticationScheme,
    options => options.Events = new TravelBookCookieAuthenticationEvents());

//Configure OpenID Connect events
builder.Services.Configure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options =>
    {
        var existingRedirectHandler = options.Events.OnRedirectToIdentityProvider;
        var existingLogoutHandler = options.Events.OnRedirectToIdentityProviderForSignOut;

        //options.Events.OnTokenValidated = ctx =>
        //{
        //    if (ctx?.Principal is null)
        //        throw new Exception($"Principal is null");

        //    var principal = ctx.Principal;

        //    var neededClaims = new[] { "preferred_username", "oid", "tid", "login_hint" };

        //    foreach (var claim in neededClaims)
        //    {
        //        if (!principal.HasClaim(c => c.Type == claim))
        //            throw new Exception($"Missing claim: {claim}");
        //    }

        //    return Task.CompletedTask;
        //};

        options.Events.OnRedirectToIdentityProvider = context =>
        {
            existingRedirectHandler?.Invoke(context);

            var login_hint = context.HttpContext.User.Claims.Where(c => c.Type == "login_hint").FirstOrDefault();
            if (login_hint != null)
            {
                context.ProtocolMessage.SetParameter("login_hint", login_hint.Value);
            }

            return Task.CompletedTask;
        };

        options.Events.OnRedirectToIdentityProviderForSignOut = async context =>
        {
            existingLogoutHandler?.Invoke(context);

            var idToken = await context.HttpContext.GetTokenAsync("id_token");
            if (!string.IsNullOrEmpty(idToken))
            {
                context.ProtocolMessage.IdTokenHint = idToken;
            }

            var login_hint = context.HttpContext.User.Claims.Where(c => c.Type == "login_hint").FirstOrDefault();
            if (login_hint != null)
            {
                context.ProtocolMessage.SetParameter("logout_hint",  login_hint.Value);
            }

            context.Properties.RedirectUri = builder.Configuration["AzureAd:SignedOutCallbackPath"];
            context.ProtocolMessage.PostLogoutRedirectUri = builder.Configuration["AzureAd:SignedOutRedirectUri"];
        };
    });

builder.Services.ConfigureApplicationCookie(options =>
    {
        options.Cookie.Name = ".TravelBook.Auth";        // Nom personnalis�
        options.Cookie.SameSite = SameSiteMode.None;      // Recommand� pour OIDC
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo("/home/data-protection"))
    .SetApplicationName("TravelBook");

builder.Services.AddAuthorizationBuilder().SetFallbackPolicy(null);
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();
builder.Services.AddHttpContextAccessor();


// Pipeline
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

app.UseCookiePolicy(new CookiePolicyOptions
{
    MinimumSameSitePolicy = SameSiteMode.None,
    Secure = CookieSecurePolicy.Always,
});

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(TravelBook.Client._Imports).Assembly);

app.MapControllers();
app.MapAuthenticationService();
//app.MapUserService(builder.Configuration["AzureAd:TenantId"], builder.Configuration["AzureAd:ClientId"], builder.Configuration["AzureAd:ClientSecret"]);
app.MapRazorPages();


app.Run();
