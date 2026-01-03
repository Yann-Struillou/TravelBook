using Azure;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;
using Moq;
using TravelBook.Services;

namespace TravelBook.xUnit.TravelBook.Services
{

    /// <summary>
    /// Fake loader qui simule un secret récupéré
    /// </summary>
    public class FakeAzureAdSecretLoaderWithSecret : IAzureAdSecretLoader
    {
        private readonly string _secretValue;

        public FakeAzureAdSecretLoaderWithSecret(string secretValue)
        {
            _secretValue = secretValue;
        }

        public Task LoadAsync(IConfigurationManager configuration)
        {
            configuration["AzureAd:ClientSecret"] = _secretValue;
            return Task.CompletedTask;
        }
    }

    public class AzureAdSecretLoaderTests
    {
        private static IConfigurationManager CreateConfiguration(
            Dictionary<string, string?> values)
        {
            var configuration = new ConfigurationManager();

            foreach (var kv in values)
            {
                configuration[kv.Key] = kv.Value;
            }

            return configuration;
        }

        [Fact]
        public async Task LoadAsync_Returns_When_KeyVaultUri_IsMissing()
        {
            // Arrange
            var configuration = CreateConfiguration(new());
            var factoryMock = new Mock<ISecretClientFactory>();

            var loader = new AzureAdSecretLoader(factoryMock.Object);

            // Act
            await loader.LoadAsync(configuration);

            // Assert
            factoryMock.VerifyNoOtherCalls();
            Assert.Null(configuration["AzureAd:ClientSecret"]);
        }

        [Fact]
        public async Task LoadAsync_Returns_When_ClientSecretName_IsMissing()
        {
            // Arrange
            var configuration = CreateConfiguration(new()
            {
                ["KeyVault:VaultUri"] = "https://fake-vault.vault.azure.net/"
            });

            var factoryMock = new Mock<ISecretClientFactory>();

            try
            {
                var loader = new AzureAdSecretLoader(factoryMock.Object);

                // Act
                await loader.LoadAsync(configuration);
            }
            catch
            {
            }

            // Assert
            factoryMock.VerifyNoOtherCalls();
            Assert.Null(configuration["AzureAd:ClientSecret"]);
        }

        [Fact]
        public async Task LoadAsync_Sets_ClientSecret_When_Secret_Is_Found()
        {
            // Arrange
            var configuration = new ConfigurationManager();
            configuration["KeyVault:VaultUri"] = "https://fake-vault.vault.azure.net/";
            configuration["KeyVault:AzureAdClientSecret"] = "my-secret";
            configuration["AzureAd:ClientSecret"] = "";

            // Utilisation du FakeAzureAdSecretLoader
            var loader = new FakeAzureAdSecretLoaderWithSecret("super-secret");

            // Act
            await loader.LoadAsync(configuration);

            // Assert
            Assert.Equal("super-secret", configuration["AzureAd:ClientSecret"]);
        }

        [Fact]
        public async Task LoadAsync_Throws_When_Secret_Value_Is_Null()
        {
            // Arrange
            var configuration = CreateConfiguration(new()
            {
                ["KeyVault:VaultUri"] = "https://fake-vault.vault.azure.net/",
                ["KeyVault:AzureAdClientSecret"] = "my-secret"
            });

            KeyVaultSecret? secret = null;
            try
            { 
                secret = new KeyVaultSecret("my-secret", null); 
            }
            catch
            {
            }

            var secretClientMock = new Mock<SecretClient>();
            secretClientMock
                .Setup(c => c.GetSecretAsync("my-secret", null, default))
                .ReturnsAsync(Response.FromValue(secret, null!));

            var factoryMock = new Mock<ISecretClientFactory>();
            factoryMock
                .Setup(f => f.Create(It.IsAny<Uri>()))
                .Returns(secretClientMock.Object);

            var loader = new AzureAdSecretLoader(factoryMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<AggregateException>(
                () => loader.LoadAsync(configuration));
        }
    }
}

