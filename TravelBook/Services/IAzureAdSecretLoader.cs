namespace TravelBook.Services
{
    public interface IAzureAdSecretLoader
    {
        Task LoadAsync(IConfigurationManager configurationManager);
    }
}
