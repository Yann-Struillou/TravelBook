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
using TravelBook.Client.Services;
using TravelBook.Components;
using TravelBook.Services;

var builder = WebApplication.CreateBuilder(args);

async Task ReadSecretFromKeyVault()
{
    var keyVaultUri = builder.Configuration["KeyVault:VaultUri"];
    if (!string.IsNullOrEmpty(keyVaultUri))
    {
        // Ajout de KeyVault � la configuration
        builder.Configuration.AddAzureKeyVault(
            new Uri(keyVaultUri),
            new DefaultAzureCredential());

        var secretClient = new SecretClient(new Uri(keyVaultUri), new DefaultAzureCredential());

        // Lecture du secret Azure AD
        var clientSecretName = builder.Configuration["KeyVault:AzureAdClientSecret"];
        if (!string.IsNullOrEmpty(clientSecretName))
        {
            KeyVaultSecret clientSecret = await secretClient.GetSecretAsync(clientSecretName);
            string clientSecretValue = clientSecret.Value
                ?? throw new InvalidOperationException("Azure AD client secret not found in KeyVault.");

            // Injecte la valeur r�elle dans la configuration avant l'auth
            builder.Configuration["AzureAd:ClientSecret"] = clientSecretValue;
        }
    }
}

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents()
    .AddAuthenticationStateSerialization();

builder.Services.AddRazorPages();

var useEntraID = builder.Configuration["UseEntraID"];

if (!string.IsNullOrEmpty(useEntraID) &&
    useEntraID.Equals("True", StringComparison.InvariantCultureIgnoreCase))
    await ReadSecretFromKeyVault();

var scopesToRequest = builder.Configuration["MicrosoftGraph:Scopes"]?.Split(",", StringSplitOptions.TrimEntries);

builder.Services
    .AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"))
    .EnableTokenAcquisitionToCallDownstreamApi(scopesToRequest)
    .AddMicrosoftGraph(builder.Configuration.GetSection("MicrosoftGraph"))
    .AddInMemoryTokenCaches();

builder.Services.Configure<CookieAuthenticationOptions>(
    CookieAuthenticationDefaults.AuthenticationScheme,
    options => {
        options.Cookie.Name = ".TravelBook.Auth";        // custom name
        options.Cookie.SameSite = SameSiteMode.None;      // better for OIDC
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.Events = new TravelBookCookieAuthenticationEvents();
    });

//Configure OpenID Connect events
builder.Services.Configure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options =>
    {
        var existingRedirectHandler = options.Events.OnRedirectToIdentityProvider;
        var existingLogoutHandler = options.Events.OnRedirectToIdentityProviderForSignOut;

        options.Events.OnRedirectToIdentityProvider = context =>
        {
            existingRedirectHandler?.Invoke(context);

            var login_hint = context.HttpContext.User.Claims.FirstOrDefault(c => c.Type == "login_hint");
            if (login_hint != null)
                context.ProtocolMessage.SetParameter("login_hint", login_hint.Value);

            return Task.CompletedTask;
        };

        options.Events.OnRedirectToIdentityProviderForSignOut = async context =>
        {
            existingLogoutHandler?.Invoke(context);

            var idToken = await context.HttpContext.GetTokenAsync("id_token");
            if (!string.IsNullOrEmpty(idToken))
                context.ProtocolMessage.IdTokenHint = idToken;

            var login_hint = context.HttpContext.User.Claims.FirstOrDefault(c => c.Type == "login_hint");
            if (login_hint != null)
                context.ProtocolMessage.SetParameter("logout_hint",  login_hint.Value);

            context.Properties.RedirectUri = builder.Configuration["AzureAd:SignedOutCallbackPath"];
            context.ProtocolMessage.PostLogoutRedirectUri = builder.Configuration["AzureAd:SignedOutRedirectUri"];
        };
    });

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo("/data-protection"))
    .SetApplicationName("TravelBook");

builder.Services.AddAuthorizationBuilder().SetFallbackPolicy(null);
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();
builder.Services.AddHttpContextAccessor();

builder.Services.LoadClientServerServices();


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
app.MapRazorPages();


await app.RunAsync();
