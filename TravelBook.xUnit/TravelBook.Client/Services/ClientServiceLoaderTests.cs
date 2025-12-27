using Microsoft.Extensions.DependencyInjection;
using TravelBook.Client.Services;

namespace TravelBook.xUnit.TravelBook.Client.Services
{
    public class ClientServiceLoaderTests
    {
        [Fact]
        public void LoadClientServerServices_Should_Register_UsersService_As_Scoped()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.LoadClientServerServices();

            // Assert
            var descriptor = Assert.Single(
                services,
                s => s.ServiceType == typeof(IUsersService)
            );

            Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
            Assert.Equal(typeof(UsersService), descriptor.ImplementationType);
        }

        [Fact]
        public void LoadClientServerServices_Should_Not_Throw_When_Services_Is_Null()
        {
            // Arrange
            IServiceCollection? services = null;

            // Act & Assert
            var exception = Record.Exception(() =>
                ClientServiceLoader.LoadClientServerServices(services!)
            );

            Assert.Null(exception);
        }
    }
}
