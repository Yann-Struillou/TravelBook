using Microsoft.Extensions.DependencyInjection;
using TravelBook.Client.Services;
using Xunit;

namespace TravelBook.Tests;

public class LoadClientServerServicesTests
{
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
        var exception = Record.Exception(() => services.LoadClientServerServices());
        Assert.Null(exception); // Ne devrait pas lancer d'exception grâce à services?
    }
}