using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using TravelBook.Client.Services;
using Xunit;

namespace TravelBook.xUnit.TravelBook.Client
{
    public class ClientProgramTests
    {
        [Fact]
        public void ServiceConfiguration_RegistersAuthorizationCore()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging(); // Nécessaire pour IAuthorizationService

            // Act
            services.AddAuthorizationCore();

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var authService = serviceProvider.GetService<Microsoft.AspNetCore.Authorization.IAuthorizationService>();
            Assert.NotNull(authService);
        }

        [Fact]
        public void ServiceConfiguration_RegistersCascadingAuthenticationState()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddCascadingAuthenticationState();

            // Assert
            Assert.NotEmpty(services);
            Assert.NotNull(services?.FirstOrDefault()?.ServiceType);
            Assert.Equal("Microsoft.AspNetCore.Components.ICascadingValueSupplier", services?.FirstOrDefault()?.ServiceType.ToString());   
        }

        [Fact]
        public void ServiceConfiguration_RegistersAuthenticationStateDeserialization()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddAuthenticationStateDeserialization();

            // Assert - Vérifie que le service est enregistré
            Assert.NotEmpty(services);
        }

        [Fact]
        public void ServiceConfiguration_RegistersHttpClient()
        {
            // Arrange
            var services = new ServiceCollection();
            var baseAddress = "https://localhost:5001/";

            // Act
            services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(baseAddress) });

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var httpClient = scope.ServiceProvider.GetService<HttpClient>();

            Assert.NotNull(httpClient);
            Assert.Equal(new Uri(baseAddress), httpClient.BaseAddress);
        }

        [Fact]
        public void ServiceConfiguration_HttpClient_HasCorrectLifetime()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost/") });

            // Assert
            var descriptor = services.FirstOrDefault(s => s.ServiceType == typeof(HttpClient));
            Assert.NotNull(descriptor);
            Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
        }

        [Fact]
        public void ServiceConfiguration_LoadsClientServerServices()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddHttpClient(); // Dépendance pour UsersService

            // Act
            services.LoadClientServerServices();

            // Assert
            var usersServiceDescriptor = services.FirstOrDefault(s =>
                s.ServiceType == typeof(IUsersService));
            Assert.NotNull(usersServiceDescriptor);
        }

        [Fact]
        public void ServiceConfiguration_AllServicesCanBeResolved()
        {
            // Arrange
            var services = new ServiceCollection();

            // Simuler la configuration complète
            services.AddLogging(); // Nécessaire pour plusieurs services
            services.AddAuthorizationCore();
            services.AddCascadingAuthenticationState();
            services.AddAuthenticationStateDeserialization();
            services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost/") });
            services.AddHttpClient(); // Pour LoadClientServerServices
            services.LoadClientServerServices();

            // Act
            var serviceProvider = services.BuildServiceProvider();

            // Assert - Vérifie que les services critiques peuvent être résolus
            using var scope = serviceProvider.CreateScope();

            var authService = scope.ServiceProvider.GetService<Microsoft.AspNetCore.Authorization.IAuthorizationService>();
            Assert.NotNull(authService);

            var httpClient = scope.ServiceProvider.GetService<HttpClient>();
            Assert.NotNull(httpClient);

            var usersService = scope.ServiceProvider.GetService<IUsersService>();
            Assert.NotNull(usersService);
        }

        [Fact]
        public void ServiceConfiguration_HttpClient_UsesHostEnvironmentBaseAddress()
        {
            // Arrange
            var expectedBaseAddress = "https://myapp.com/";
            var services = new ServiceCollection();

            // Act - Simule ce que fait le Program.cs
            services.AddScoped(sp => new HttpClient
            {
                BaseAddress = new Uri(expectedBaseAddress)
            });

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var httpClient = scope.ServiceProvider.GetService<HttpClient>();

            Assert.NotNull(httpClient);
            Assert.Equal(expectedBaseAddress, httpClient.BaseAddress?.ToString());
        }

        [Fact]
        public void ServiceConfiguration_MultipleScopes_GetDifferentHttpClientInstances()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost/") });
            var serviceProvider = services.BuildServiceProvider();

            // Act
            HttpClient? client1;
            HttpClient? client2;

            using (var scope1 = serviceProvider.CreateScope())
            {
                client1 = scope1.ServiceProvider.GetService<HttpClient>();
            }

            using (var scope2 = serviceProvider.CreateScope())
            {
                client2 = scope2.ServiceProvider.GetService<HttpClient>();
            }

            // Assert - Vérifie que chaque scope obtient une instance différente (Scoped)
            Assert.NotNull(client1);
            Assert.NotNull(client2);
            Assert.NotSame(client1, client2);
        }

        [Fact]
        public void ServiceConfiguration_IntegrationTest_CompleteServiceCollection()
        {
            // Arrange & Act - Simule exactement la configuration du Program.cs
            var services = new ServiceCollection();

            services.AddAuthorizationCore();
            services.AddCascadingAuthenticationState();
            services.AddAuthenticationStateDeserialization();
            services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:5001/") });
            services.AddHttpClient();
            services.LoadClientServerServices();

            var serviceProvider = services.BuildServiceProvider();

            // Assert - Test d'intégration complet
            var exception = Record.Exception(() =>
            {
                using var scope = serviceProvider.CreateScope();
                var sp = scope.ServiceProvider;

                // Essaye de résoudre tous les services critiques
                _ = sp.GetRequiredService<Microsoft.AspNetCore.Authorization.IAuthorizationService>();
                _ = sp.GetRequiredService<HttpClient>();
                _ = sp.GetRequiredService<IUsersService>();
            });

            Assert.Null(exception);
        }
    }
}
