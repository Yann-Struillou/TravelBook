using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using System.Diagnostics.CodeAnalysis;

namespace TravelBook.Services
{
    [ExcludeFromCodeCoverage]
    public class AzureKeyVaultSecretReader : IKeyVaultSecretReader
    {
        public async Task<string?> ReadSecretAsync(string vaultUri, string secretName)
        {
            var client = new SecretClient(new Uri(vaultUri), new DefaultAzureCredential());
            var secret = await client.GetSecretAsync(secretName);
            return secret.Value.Value;
        }
    }
}
