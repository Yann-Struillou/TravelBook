using Microsoft.Extensions.DependencyInjection;
using TravelBook.Client.Services;
using System.Net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace TravelBook.xUnit.TravelBook
{
    public class ProgramTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public ProgramTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, config) =>
                {
                    var settings = new Dictionary<string, string?>
                    {
                        ["UseEntraID"] = "False", // évite KeyVault
                        ["AzureAd:Instance"] = "https://login.microsoftonline.com/",
                        ["AzureAd:TenantId"] = "fake-tenant",
                        ["AzureAd:ClientId"] = "fake-client-id",
                        ["AzureAd:ClientSecret"] = "fake-secret",
                        ["AzureAd:CallbackPath"] = "/signin-oidc"
                    };

                    config.AddInMemoryCollection(settings);
                });
            });
        }

        // --------------------------------------------------------------------
        // 1. L'application démarre correctement
        // --------------------------------------------------------------------
        [Fact]
        public async Task Application_Starts_Successfully()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/");

            Assert.NotNull(response);
        }

        // --------------------------------------------------------------------
        // 2. Le service Authentication est enregistré
        // --------------------------------------------------------------------
        [Fact]
        public void Authentication_Is_Registered()
        {
            using var scope = _factory.Services.CreateScope();
            var authService = scope.ServiceProvider.GetService<IAuthenticationService>();

            Assert.NotNull(authService);
        }

        // --------------------------------------------------------------------
        // 3. OpenID Connect est bien configuré
        // --------------------------------------------------------------------
        [Fact]
        public void OpenIdConnect_Is_Configured()
        {
            using var scope = _factory.Services.CreateScope();
            var schemeProvider = scope.ServiceProvider.GetRequiredService<IAuthenticationSchemeProvider>();

            var scheme = schemeProvider.GetSchemeAsync(OpenIdConnectDefaults.AuthenticationScheme).Result;

            Assert.NotNull(scheme);
            Assert.Equal(OpenIdConnectDefaults.AuthenticationScheme, scheme.Name);
        }

        // --------------------------------------------------------------------
        // 4. Cookie d’authentification personnalisé
        // --------------------------------------------------------------------
        [Fact]
        public void Cookie_Authentication_Is_Configured()
        {
            using var scope = _factory.Services.CreateScope();
            var options = scope.ServiceProvider
                .GetRequiredService<Microsoft.Extensions.Options.IOptionsMonitor<Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationOptions>>()
                .Get(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme);
            Assert.Equal(".TravelBook.Auth", options.Cookie.Name);
            Assert.Equal(Microsoft.AspNetCore.Http.SameSiteMode.None, options.Cookie.SameSite);
        }

        // --------------------------------------------------------------------
        // 5. DataProtection est bien configuré
        // --------------------------------------------------------------------
        [Fact]
        public void DataProtection_Is_Configured()
        {
            using var scope = _factory.Services.CreateScope();
            var provider = scope.ServiceProvider.GetService<Microsoft.AspNetCore.DataProtection.IDataProtectionProvider>();

            Assert.NotNull(provider);
        }

        // --------------------------------------------------------------------
        // 6. Une route inexistante retourne bien une page NotFound
        // --------------------------------------------------------------------
        [Fact]
        public async Task Unknown_Route_Returns_NotFound()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/route-inexistante-123");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public void LoadClientServerServices_RegistersIUsersService()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.LoadClientServerServices();

            // Assert
            var usersServiceDescriptor = services.FirstOrDefault(s =>
                s.ServiceType == typeof(IUsersService));

            Assert.NotNull(usersServiceDescriptor);
            Assert.Equal(typeof(UsersService), usersServiceDescriptor.ImplementationType);
        }

        [Fact]
        public void LoadClientServerServices_RegistersUsersServiceWithScopedLifetime()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.LoadClientServerServices();

            // Assert
            var usersServiceDescriptor = services.FirstOrDefault(s =>
                s.ServiceType == typeof(IUsersService));

            Assert.NotNull(usersServiceDescriptor);
            Assert.Equal(ServiceLifetime.Scoped, usersServiceDescriptor.Lifetime);
        }

        [Fact]
        public void LoadClientServerServices_CanResolveIUsersService()
        {
            // Arrange
            var services = new ServiceCollection();

            // Ajoutez les dépendances nécessaires pour UsersService
            services.AddHttpClient(); // HttpClient nécessaire pour UsersService
            services.AddLogging();    // Si vos services utilisent ILogger

            // Act
            services.LoadClientServerServices();
            var serviceProvider = services.BuildServiceProvider();

            // Assert - Vérifiez que IUsersService peut être résolu
            var usersService = serviceProvider.GetService<IUsersService>();
            Assert.NotNull(usersService);
        }

        [Fact]
        public void LoadClientServerServices_DoesNotThrowException()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act & Assert - Vérifie que l'enregistrement ne lance pas d'exception
            var exception = Record.Exception(() => services.LoadClientServerServices());
            Assert.Null(exception);
        }

        [Fact]
        public void LoadClientServerServices_HandlesNullServiceCollection()
        {
            // Arrange
            IServiceCollection? services = null;

            // Act & Assert - Vérifie que la méthode gère null gracieusement
            var exception = Record.Exception(() => services!.LoadClientServerServices());
            Assert.Null(exception); // Ne devrait pas lancer d'exception grâce à services?
        }
    }
}