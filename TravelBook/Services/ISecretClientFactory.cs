using Azure.Security.KeyVault.Secrets;

namespace TravelBook.Services
{
    public interface ISecretClientFactory
    {
        SecretClient Create(Uri vaultUri);
    }
}