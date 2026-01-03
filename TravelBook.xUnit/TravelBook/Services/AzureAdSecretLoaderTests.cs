using Azure;
using Azure.Identity;
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
        private readonly string? _secretName;
        private readonly string? _secretValue;

        public FakeAzureAdSecretLoaderWithSecret(string? secretName, string? secretValue)
        {
            _secretName = secretName;
            _secretValue = secretValue;
        }

        public Task LoadAsync(IConfigurationManager configurationManager)
        {
            if (string.IsNullOrEmpty(_secretName))
                return Task.CompletedTask;

            if (_secretValue is null)
                throw new InvalidOperationException("Secret is null");

            configurationManager["AzureAd:ClientSecret"] = _secretValue;

            return Task.CompletedTask;
        }
    }

    public class FakeAzureAdSecretLoaderWithSecretMocked : IAzureAdSecretLoader
    {
        private readonly string? _secretValue;

        public FakeAzureAdSecretLoaderWithSecretMocked(string? secretValue)
        {
            _secretValue = secretValue;
        }

        public async Task LoadAsync(IConfigurationManager configurationManager)
        {
            ArgumentNullException.ThrowIfNull(configurationManager);

            var vaultUri = configurationManager["KeyVault:VaultUri"];
            var secretName = configurationManager["KeyVault:AzureAdClientSecret"];

            if (string.IsNullOrEmpty(vaultUri) || string.IsNullOrEmpty(secretName))
                return;

            configurationManager["AzureAd:ClientSecret"]
                = await new FakeAzureKeyVaultSecretReader(_secretValue).ReadSecretAsync(vaultUri, secretName) ?? throw new InvalidOperationException();
        }
    }

    public class FakeAzureKeyVaultSecretReader : IKeyVaultSecretReader
    {
        private readonly string? _secretValue;

        public FakeAzureKeyVaultSecretReader(string? secretValue)
        {
            _secretValue = secretValue;
        }

        public async Task<string?> ReadSecretAsync(string vaultUri, string secretName)
        {
            return _secretValue;
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
            var configuration = CreateConfiguration([]);

            var loader = new AzureAdSecretLoader();

            // Act
            await loader.LoadAsync(configuration);

            // Assert
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

            var loader = new AzureAdSecretLoader();

            // Act
            await loader.LoadAsync(configuration);

            // Assert
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
            var loader = new FakeAzureAdSecretLoaderWithSecret(configuration["KeyVault:AzureAdClientSecret"], "super-secret");

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
                ["KeyVault:AzureAdClientSecret"] = "my-secret",
                ["AzureAd:ClientSecret"] = ""
            });

            // Utilisation du FakeAzureAdSecretLoader
            var loader = new FakeAzureAdSecretLoaderWithSecret(configuration["KeyVault:AzureAdClientSecret"], null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => loader.LoadAsync(configuration));
        }

        [Fact]
        public async Task LoadAsync_Returns_When_ClientSecretName_Is_Null()
        {
            var configuration = CreateConfiguration(new()
            {
                ["KeyVault:VaultUri"] = "https://fake-vault.vault.azure.net/"
                // PAS de KeyVault:AzureAdClientSecret
            });

            var loader = new FakeAzureAdSecretLoaderWithSecret(configuration["KeyVault:AzureAdClientSecret"], null);

            await loader.LoadAsync(configuration);

            Assert.Null(configuration["AzureAd:ClientSecret"]);
        }

        [Fact]
        public async Task LoadAsync_Returns_When_ClientSecretName_Is_Empty()
        {
            var configuration = CreateConfiguration(new()
            {
                ["KeyVault:VaultUri"] = "https://fake-vault.vault.azure.net/",
                ["KeyVault:AzureAdClientSecret"] = ""
            });

            var loader = new FakeAzureAdSecretLoaderWithSecret("", null);

            await loader.LoadAsync(configuration);

            Assert.Null(configuration["AzureAd:ClientSecret"]);
        }

        [Fact]
        public async Task LoadAsync_Returns_When_ClientSecretName_Is_Null_SecretMocked()
        {
            // Arrange
            var configuration = CreateConfiguration(new()
            {
                ["KeyVault:VaultUri"] = "https://fake.vault/"
                // AzureAdClientSecret absent
            });

            var loader = new FakeAzureAdSecretLoaderWithSecretMocked("super-secret");

            // Act
            await loader.LoadAsync(configuration);

            // Assert
            Assert.Null(configuration["AzureAd:ClientSecret"]);
        }

        [Fact]
        public async Task LoadAsync_Sets_ClientSecret_When_Secret_Found_SecretMocked()
        {
            // Arrange
            var configuration = CreateConfiguration(new()
            {
                ["KeyVault:VaultUri"] = "https://fake.vault/",
                ["KeyVault:AzureAdClientSecret"] = "my-secret"
            });

            var loader = new FakeAzureAdSecretLoaderWithSecretMocked("super-secret");

            // Act
            await loader.LoadAsync(configuration);

            // Assert
            Assert.Equal("super-secret", configuration["AzureAd:ClientSecret"]);
        }

        [Fact]
        public async Task LoadAsync_Throws_When_Secret_Value_Is_Null_SecretMocked()
        {
            // Arrange
            var configuration = CreateConfiguration(new()
            {
                ["KeyVault:VaultUri"] = "https://fake.vault/",
                ["KeyVault:AzureAdClientSecret"] = "my-secret"
            });

            var loader = new FakeAzureAdSecretLoaderWithSecretMocked(null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => loader.LoadAsync(configuration));
        }

    }
}

