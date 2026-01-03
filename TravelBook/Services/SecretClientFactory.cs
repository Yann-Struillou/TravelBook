using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace TravelBook.Services
{
    public class SecretClientFactory : ISecretClientFactory
    {
        public SecretClient Create(Uri vaultUri)
        {
            return new SecretClient(vaultUri, new DefaultAzureCredential());
        }
    }
}
