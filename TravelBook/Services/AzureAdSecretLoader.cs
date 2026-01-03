using Azure.Identity;

namespace TravelBook.Services
{
    public class AzureAdSecretLoader(ISecretClientFactory secretClientFactory) : IAzureAdSecretLoader
    {
        public async Task LoadAsync(IConfigurationManager configurationManager)
        {
            ArgumentNullException.ThrowIfNull(configurationManager);

            var keyVaultUri = configurationManager["KeyVault:VaultUri"];
            if (string.IsNullOrEmpty(keyVaultUri))
                return;

            configurationManager.AddAzureKeyVault(new Uri(keyVaultUri), new DefaultAzureCredential());

            var clientSecretName = configurationManager["KeyVault:AzureAdClientSecret"];
            if (string.IsNullOrEmpty(clientSecretName))
                return;

            var secretClient = secretClientFactory.Create(new Uri(keyVaultUri));

            var secret = await secretClient.GetSecretAsync(clientSecretName);

            configurationManager["AzureAd:ClientSecret"] =
                secret.Value.Value ?? throw new InvalidOperationException();
        }
    }
}

