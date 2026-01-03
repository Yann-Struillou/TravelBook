namespace TravelBook.Services
{
    public interface IKeyVaultSecretReader
    {
        Task<string?> ReadSecretAsync(string vaultUri, string secretName);
    }
}
