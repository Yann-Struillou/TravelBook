using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using TravelBook.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(options =>
    {
        builder.Configuration.Bind("AzureAd", options);
        
        // Get the client secret from environnement variable
        var clientSecret = Environment.GetEnvironmentVariable("ENTRA_CLIENT_SECRET");

        if (string.IsNullOrEmpty(clientSecret))
        {
            throw new InvalidOperationException(
                "ENTRA_CLIENT_SECRET environment variable is not set. " +
                "The application cannot start without the client secret.");
        }
        Console.WriteLine($"ClientSecret loaded: {clientSecret.Length} characters");
        options.ClientSecret = clientSecret;

        options.SaveTokens = true;
    });

builder.Services.AddAuthorization();

builder.Services.AddControllersWithViews()
    .AddMicrosoftIdentityUI();

builder.Services.AddRazorPages()
    .AddMicrosoftIdentityUI();

builder.Services.AddCascadingAuthenticationState();

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
app.MapRazorPages();

app.Run();
