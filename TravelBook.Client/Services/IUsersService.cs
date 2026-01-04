using TravelBookDto.Users;

namespace TravelBook.Client.Services
{
    public interface IUsersService
    {
        // POST: api/users/GetUserById
        Task<GetUserResponseDto?> GetUserByIdAsync(GetUserByIdDto dto);

        // POST: api/users/GetUserByPrincipalName
        Task<GetUserResponseDto?> GetUserByPrincipalAsync(GetUserByPrincipalNameDto dto);

        // POST: api/users/CreateUser
        Task<CreateUserResponseDto?> CreateUserAsync(CreateUserDto dto);
    }
}