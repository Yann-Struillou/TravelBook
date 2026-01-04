using System.Net.Http.Json;
using TravelBookDto.Users;

namespace TravelBook.Client.Services
{
    public class UsersService : IUsersService
    {
        private readonly HttpClient _http;

        public UsersService(HttpClient http)
        {
            _http = http;
        }

        // POST: api/users/GetUserById
        public async Task<GetUserResponseDto?> GetUserByIdAsync(GetUserByIdDto dto)
        {
            try
            {
                var response = await _http.PostAsJsonAsync($"api/users/GetUserById", dto);

                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    return new GetUserResponseDto($"API Error: {errorMessage}", null, null, null, null);
                }
                return await response.Content.ReadFromJsonAsync<GetUserResponseDto>();
            }
            catch (Exception ex)
            {
                return new GetUserResponseDto($"Application error: {ex.Message}", null, null, null, null);
            }
        }

        // POST: api/users/GetUserByPrincipalName
        public async Task<GetUserResponseDto?> GetUserByPrincipalAsync(GetUserByPrincipalNameDto dto)
        {
            try
            {
                var response = await _http.PostAsJsonAsync($"api/users/GetUserByPrincipalName", dto);

                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    return new GetUserResponseDto($"API Error: {errorMessage}", null, null, null, null);
                }
                return await response.Content.ReadFromJsonAsync<GetUserResponseDto>();
            }
            catch (Exception ex)
            {
                return new GetUserResponseDto($"Application error: {ex.Message}", null, null, null, null);
            }
        }

        // POST: api/users/CreateUser
        public async Task<CreateUserResponseDto?> CreateUserAsync(CreateUserDto dto)
        {
            var response = await _http.PostAsJsonAsync("api/users/CreateUser", dto);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<CreateUserResponseDto>();
            }

            await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
            throw new ArgumentNullException(response.ReasonPhrase ?? "Could not read from Json");
        }
    }
}