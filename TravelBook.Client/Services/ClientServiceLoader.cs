namespace TravelBook.Client.Services
{
    public static class ClientServiceLoader
    {
        public static void LoadClientServerServices(this IServiceCollection services)
        { 
            services?.AddScoped<IUsersService, UsersService>();
        }
    }
}
