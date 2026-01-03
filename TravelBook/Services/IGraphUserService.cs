using Microsoft.Graph.Models;
using Microsoft.Kiota.Abstractions;
using Microsoft.Graph.Users;

namespace TravelBook.Services
{
    public interface IGraphUserService
    {
        Task<UserCollectionResponse?> GetUsersAsync(Action<RequestConfiguration<UsersRequestBuilder.UsersRequestBuilderGetQueryParameters>> requestConfiguration);
        Task<User?> CreateUserAsync(User user);
    }
}
