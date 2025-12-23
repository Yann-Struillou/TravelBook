using Moq;
using TravelBook.Client.Pages;
using TravelBook.Client.Services;
using TravelBook.Client.ViewModels.Users;
using TravelBookDto.Users;
using Xunit;

namespace TravelBook.xUnit.TravelBook.Client.Pages
{
    public class CreateUserTests
    {
        private readonly Mock<IUsersService> _mockUsersService;
        private readonly CreateUser _component;

        public CreateUserTests()
        {
            _mockUsersService = new Mock<IUsersService>();
            _component = new CreateUser
            {
                UsersService = _mockUsersService.Object
            };
        }

        [Fact]
        public async Task HandleValidSubmit_CreatesUser_WhenDataIsValid()
        {
            // Arrange
            var userModel = new CreateUserFormModel
            {
                UserPrincipalName = "test@example.com",
                DisplayName = "Test User",
                MailNickName = "testuser"
            };

            var expectedUser = new CreateUserResponseDto
            (
                Message : "Message",
                UserId : Guid.NewGuid().ToString(),
                UserPrincipalName : "test@example.com",
                UserDisplayName : "Test User",
                UserMailNickname : "testuser"
            );

            _mockUsersService
                .Setup(s => s.CreateUserAsync(It.IsAny<CreateUserDto>()))
                .ReturnsAsync(expectedUser);

            // Utiliser la réflexion pour définir le modèle privé
            var userModelField = typeof(CreateUser).GetField("userModel",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            userModelField?.SetValue(_component, userModel);

            // Act
            await InvokeHandleValidSubmit();

            // Assert
            _mockUsersService.Verify(s => s.CreateUserAsync(It.Is<CreateUserDto>(dto =>
                dto.UserPrincipalName == "test@example.com" &&
                dto.DisplayName == "Test User" &&
                dto.MailNickName == "testuser"
            )), Times.Once);

            var resultMessage = GetResultMessage();
            Assert.Contains("Utilisateur créé", resultMessage);
            Assert.Contains("Test User", resultMessage);
            Assert.Contains("test@example.com", resultMessage);
        }

        [Fact]
        public async Task HandleValidSubmit_ResetsForm_AfterSuccessfulCreation()
        {
            // Arrange
            var userModel = new CreateUserFormModel
            {
                UserPrincipalName = "test@example.com",
                DisplayName = "Test User",
                MailNickName = "testuser"
            };

            var expectedUser = new CreateUserResponseDto
            (
                Message: "Message",
                UserId: Guid.NewGuid().ToString(),
                UserPrincipalName: "test@example.com",
                UserDisplayName: "Test User",
                UserMailNickname: "testuser"
            );

            _mockUsersService
                .Setup(s => s.CreateUserAsync(It.IsAny<CreateUserDto>()))
                .ReturnsAsync(expectedUser);

            var userModelField = typeof(CreateUser).GetField("userModel",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            userModelField?.SetValue(_component, userModel);

            // Act
            await InvokeHandleValidSubmit();

            // Assert
            var resetModel = userModelField?.GetValue(_component) as CreateUserFormModel;
            Assert.NotNull(resetModel);
            Assert.NotNull(resetModel.UserPrincipalName);
            Assert.NotNull(resetModel.DisplayName);
            Assert.NotNull(resetModel.MailNickName);
        }

        [Fact]
        public async Task HandleValidSubmit_DisplaysErrorMessage_WhenServiceThrowsException()
        {
            // Arrange
            var exceptionMessage = "Une erreur est survenue";
            _mockUsersService
                .Setup(s => s.CreateUserAsync(It.IsAny<CreateUserDto>()))
                .ThrowsAsync(new Exception(exceptionMessage));

            var userModel = new CreateUserFormModel
            {
                UserPrincipalName = "test@example.com",
                DisplayName = "Test User",
                MailNickName = "testuser"
            };

            var userModelField = typeof(CreateUser).GetField("userModel",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            userModelField?.SetValue(_component, userModel);

            // Act
            await InvokeHandleValidSubmit();

            // Assert
            var resultMessage = GetResultMessage();
            Assert.Contains("Erreur", resultMessage);
            Assert.Contains(exceptionMessage, resultMessage);
        }

        [Fact]
        public async Task HandleValidSubmit_DoesNotResetForm_WhenExceptionOccurs()
        {
            // Arrange
            var userModel = new CreateUserFormModel
            {
                UserPrincipalName = "test@example.com",
                DisplayName = "Test User",
                MailNickName = "testuser"
            };

            _mockUsersService
                .Setup(s => s.CreateUserAsync(It.IsAny<CreateUserDto>()))
                .ThrowsAsync(new Exception("Erreur"));

            var userModelField = typeof(CreateUser).GetField("userModel",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            userModelField?.SetValue(_component, userModel);

            // Act
            await InvokeHandleValidSubmit();

            // Assert
            var currentModel = userModelField?.GetValue(_component) as CreateUserFormModel;
            Assert.NotNull(currentModel);
            Assert.Equal("test@example.com", currentModel.UserPrincipalName);
            Assert.Equal("Test User", currentModel.DisplayName);
            Assert.Equal("testuser", currentModel.MailNickName);
        }

        [Fact]
        public async Task HandleValidSubmit_HandlesNullUserResponse()
        {
            // Arrange
            _mockUsersService
                .Setup(s => s.CreateUserAsync(It.IsAny<CreateUserDto>()))
                .ReturnsAsync((CreateUserResponseDto?)null);

            var userModel = new CreateUserFormModel
            {
                UserPrincipalName = "test@example.com",
                DisplayName = "Test User",
                MailNickName = "testuser"
            };

            var userModelField = typeof(CreateUser).GetField("userModel",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            userModelField?.SetValue(_component, userModel);

            // Act
            await InvokeHandleValidSubmit();

            // Assert
            var resultMessage = GetResultMessage();
            Assert.Contains("Utilisateur créé", resultMessage);
        }

        [Fact]
        public void UsersService_PropertyInjection_IsConfigured()
        {
            // Assert
            var property = typeof(CreateUser).GetProperty("UsersService");
            Assert.NotNull(property);

            var injectAttribute = property?.GetCustomAttributes(typeof(Microsoft.AspNetCore.Components.InjectAttribute), false);
            Assert.NotNull(injectAttribute);
            Assert.NotEmpty(injectAttribute);
        }

        // Méthodes helper privées
        private async Task InvokeHandleValidSubmit()
        {
            var method = typeof(CreateUser).GetMethod("HandleValidSubmit",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var task = method?.Invoke(_component, null) as Task;
            if (task != null)
            {
                await task;
            }
        }

        private string GetResultMessage()
        {
            var field = typeof(CreateUser).GetField("resultMessage",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return field?.GetValue(_component) as string ?? string.Empty;
        }
    }
}