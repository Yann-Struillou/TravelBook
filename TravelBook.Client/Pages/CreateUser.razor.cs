using Microsoft.AspNetCore.Components;
using TravelBook.Client.Services;
using TravelBook.Client.ViewModels.Users;
using TravelBookDto.Users;

namespace TravelBook.Client.Pages
{
    public partial class CreateUser
    {
        [Inject]
        public IUsersService UsersService { get; set; } = default!;

        private CreateUserFormModel userModel = new();
        private string resultMessage = string.Empty;

        private async Task HandleValidSubmit()
        {
            try
            {
                var createdUser = await UsersService.CreateUserAsync(new CreateUserDto(
                    userModel.UserPrincipalName,
                    userModel.DisplayName,
                    userModel.MailNickName
                ));

                resultMessage = $"User created: {createdUser?.UserDisplayName} ({createdUser?.UserPrincipalName})";
                userModel = new CreateUserFormModel(); // Reset du formulaire

                StateHasChanged(); // Force le re-render
            }
            catch (Exception ex)
            {
                resultMessage = $"Error : {ex.Message}";
                StateHasChanged(); // Force le re-render même en cas d'erreur
            }
        }
    }
}