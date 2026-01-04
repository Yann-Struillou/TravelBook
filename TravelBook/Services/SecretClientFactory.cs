using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using System.Diagnostics.CodeAnalysis;

namespace TravelBook.Services
{
    [ExcludeFromCodeCoverage]
    public class SecretClientFactory : ISecretClientFactory
    {
        public SecretClient Create(Uri vaultUri)
        {
            return new SecretClient(vaultUri, new DefaultAzureCredential());
        }
    }
}
