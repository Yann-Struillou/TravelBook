using Azure.Identity;

namespace TravelBook.Services
{
    public class AzureAdSecretLoader() : IAzureAdSecretLoader
    {
        public async Task LoadAsync(IConfigurationManager configurationManager, IKeyVaultSecretReader keyVaultSecretReader)
        {
            ArgumentNullException.ThrowIfNull(configurationManager);

            var vaultUri = configurationManager["KeyVault:VaultUri"];
            var secretName = configurationManager["KeyVault:AzureAdClientSecret"];

            if (string.IsNullOrEmpty(vaultUri) || string.IsNullOrEmpty(secretName))
                return;

            configurationManager["AzureAd:ClientSecret"] 
                = await keyVaultSecretReader.ReadSecretAsync(vaultUri, secretName) ?? throw new InvalidOperationException();
        }
    }
}

