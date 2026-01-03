using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Users;
using Microsoft.Kiota.Abstractions;

namespace TravelBook.Services
{
    public class GraphUserService : IGraphUserService
    {
        private readonly GraphServiceClient _client;

        public GraphUserService(GraphServiceClient client) => _client = client;

        public async Task<UserCollectionResponse?> GetUsersAsync(Action<RequestConfiguration<UsersRequestBuilder.UsersRequestBuilderGetQueryParameters>> requestConfiguration)
        {
            return await _client.Users.GetAsync(requestConfiguration);
        }

        public async Task<User?> CreateUserAsync(User user)
        {
            return await _client.Users.PostAsync(user);
        }
    }
}
