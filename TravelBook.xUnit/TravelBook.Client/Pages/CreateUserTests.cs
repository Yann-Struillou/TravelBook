using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using TravelBook.Client.Pages;
using TravelBook.Client.Services;
using TravelBook.Client.ViewModels.Users;
using TravelBookDto.Users;

namespace TravelBook.xUnit.TravelBook.Client.Pages
{
    public class CreateUserTests
    {
        private readonly Mock<IUsersService> _mockUsersService;
        private readonly BunitContext _context;

        public CreateUserTests()
        {
            _mockUsersService = new Mock<IUsersService>();
            _context = new BunitContext();

            // Enregistrement du service via ServiceCollection
            var services = new ServiceCollection();
            services.Add(ServiceDescriptor.Singleton<IUsersService>(sp => _mockUsersService.Object));

            // Appliquer les services au contexte
            foreach (var service in services)
            {
                _context.Services.Add(service);
            }
        }

        [Fact]
        public void Component_RendersCorrectly()
        {
            // Act
            var cut = _context.Render<CreateUser>();

            // Assert
            Assert.NotNull(cut);
        }

        [Fact]
        public void UsersService_IsInjectedCorrectly()
        {
            // Act
            var cut = _context.Render<CreateUser>();

            // Assert
            Assert.NotNull(cut.Instance.UsersService);
            Assert.Same(_mockUsersService.Object, cut.Instance.UsersService);
        }

        [Fact]
        public async Task HandleValidSubmit_CreatesUser_WhenDataIsValid()
        {
            // Arrange
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

            var cut = _context.Render<CreateUser>();

            // Définir le modèle utilisateur via réflexion
            var userModelField = typeof(CreateUser).GetField("userModel",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var userModel = new CreateUserFormModel
            {
                UserPrincipalName = "test@example.com",
                DisplayName = "Test User",
                MailNickName = "testuser"
            };
            userModelField?.SetValue(cut.Instance, userModel);

            // Act
            await cut.InvokeAsync(async () => await InvokeHandleValidSubmit(cut.Instance));

            // Assert
            _mockUsersService.Verify(s => s.CreateUserAsync(It.Is<CreateUserDto>(dto =>
                dto.UserPrincipalName == "test@example.com" &&
                dto.DisplayName == "Test User" &&
                dto.MailNickName == "testuser"
            )), Times.Once);

            var resultMessage = GetResultMessage(cut.Instance);
            Assert.Contains("User created:", resultMessage);
            Assert.Contains("Test User", resultMessage);
            Assert.Contains("test@example.com", resultMessage);
        }

        [Fact]
        public async Task HandleValidSubmit_ResetsForm_AfterSuccessfulCreation()
        {
            // Arrange
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

            var cut = _context.Render<CreateUser>();

            var userModelField = typeof(CreateUser).GetField("userModel",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var userModel = new CreateUserFormModel
            {
                UserPrincipalName = "test@example.com",
                DisplayName = "Test User",
                MailNickName = "testuser"
            };
            userModelField?.SetValue(cut.Instance, userModel);

            // Act
            await cut.InvokeAsync(async () => await InvokeHandleValidSubmit(cut.Instance));

            // Assert
            var resetModel = userModelField?.GetValue(cut.Instance) as CreateUserFormModel;
            Assert.NotNull(resetModel);
            Assert.Empty(resetModel.UserPrincipalName);
            Assert.Empty(resetModel.DisplayName);
            Assert.Empty(resetModel.MailNickName);
        }

        [Fact]
        public async Task HandleValidSubmit_DisplaysErrorMessage_WhenServiceThrowsException()
        {
            // Arrange
            var exceptionMessage = "Error";
            _mockUsersService
                .Setup(s => s.CreateUserAsync(It.IsAny<CreateUserDto>()))
                .ThrowsAsync(new Exception(exceptionMessage));

            var cut = _context.Render<CreateUser>();

            var userModelField = typeof(CreateUser).GetField("userModel",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var userModel = new CreateUserFormModel
            {
                UserPrincipalName = "test@example.com",
                DisplayName = "Test User",
                MailNickName = "testuser"
            };
            userModelField?.SetValue(cut.Instance, userModel);

            // Act
            await cut.InvokeAsync(async () => await InvokeHandleValidSubmit(cut.Instance));

            // Assert
            var resultMessage = GetResultMessage(cut.Instance);
            Assert.Contains("Error", resultMessage);
            Assert.Contains(exceptionMessage, resultMessage);
        }

        [Fact]
        public async Task HandleValidSubmit_DoesNotResetForm_WhenExceptionOccurs()
        {
            // Arrange
            _mockUsersService
                .Setup(s => s.CreateUserAsync(It.IsAny<CreateUserDto>()))
                .ThrowsAsync(new Exception("Error"));

            var cut = _context.Render<CreateUser>();

            var userModelField = typeof(CreateUser).GetField("userModel",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var userModel = new CreateUserFormModel
            {
                UserPrincipalName = "test@example.com",
                DisplayName = "Test User",
                MailNickName = "testuser"
            };
            userModelField?.SetValue(cut.Instance, userModel);

            // Act
            await cut.InvokeAsync(async () => await InvokeHandleValidSubmit(cut.Instance));

            // Assert
            var currentModel = userModelField?.GetValue(cut.Instance) as CreateUserFormModel;
            Assert.NotNull(currentModel);
            var nonNullModel = Assert.IsType<CreateUserFormModel>(currentModel);
            Assert.Equal("test@example.com", nonNullModel.UserPrincipalName);
            Assert.Equal("Test User", nonNullModel.DisplayName);
            Assert.Equal("testuser", nonNullModel.MailNickName);
        }

        [Fact]
        public async Task HandleValidSubmit_HandlesNullUserResponse()
        {
            // Arrange
            _mockUsersService
                .Setup(s => s.CreateUserAsync(It.IsAny<CreateUserDto>()))
                .ReturnsAsync((CreateUserResponseDto?)null);

            var cut = _context.Render<CreateUser>();

            var userModelField = typeof(CreateUser).GetField("userModel",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var userModel = new CreateUserFormModel
            {
                UserPrincipalName = "test@example.com",
                DisplayName = "Test User",
                MailNickName = "testuser"
            };
            userModelField?.SetValue(cut.Instance, userModel);

            // Act
            await cut.InvokeAsync(async () => await InvokeHandleValidSubmit(cut.Instance));

            // Assert
            var resultMessage = GetResultMessage(cut.Instance);
            Assert.Contains("User created", resultMessage);
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
        private async Task InvokeHandleValidSubmit(CreateUser component)
        {
            var method = typeof(CreateUser).GetMethod("HandleValidSubmit",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var task = method?.Invoke(component, null) as Task;
            if (task != null)
            {
                await task;
            }
        }

        private string GetResultMessage(CreateUser component)
        {
            var field = typeof(CreateUser).GetField("resultMessage",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return field?.GetValue(component) as string ?? string.Empty;
        }
    }
}