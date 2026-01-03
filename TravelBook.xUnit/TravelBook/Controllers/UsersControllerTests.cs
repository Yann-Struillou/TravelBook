using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Users;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Serialization;
using Moq;
using System.Net;
using TravelBook.Controllers;
using TravelBook.Services;
using TravelBookDto.Users;

namespace TravelBook.xUnit.TravelBook.Controllers
{
    public class UsersControllerTests
    {
        private readonly Mock<IGraphUserService> _graphUserService;
        private readonly UsersController _controller;
        private const string TestDomain = "testdomain.com";

        private static (UsersController controller, Mock<IGraphUserService> graphUserService) CreateController
        {
            get
            {
                var newGraphUserService = new Mock<IGraphUserService>();
                var newMockConfiguration = new Mock<IConfiguration>();

                newMockConfiguration.Setup(c => c["AzureAd:Domain"]).Returns(TestDomain);

                var controller = new UsersController(newGraphUserService.Object, newMockConfiguration.Object);
                
                return (controller, newGraphUserService);
            }
        }

        public UsersControllerTests() =>
            // Créer un mock du RequestAdapter au lieu du GraphServiceClient directement
            (_controller, _graphUserService) = CreateController;

        #region Constructor Tests

        [Fact]
        public void Constructor_Should_Throw_When_Domain_Is_Null()
        {
            // Arrange
            var mockConfig = new Mock<IConfiguration>();
            mockConfig.Setup(c => c["AzureAd:Domain"]).Returns(value: null);

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                new UsersController(_graphUserService.Object, mockConfig.Object));
        }

        [Fact]
        public void Constructor_Should_Throw_When_Domain_Is_Empty()
        {
            // Arrange
            var mockConfig = new Mock<IConfiguration>();
            mockConfig.Setup(c => c["AzureAd:Domain"]).Returns("");

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                new UsersController(_graphUserService.Object, mockConfig.Object));
        }

        #endregion

        #region GetUserById Tests

        [Fact]
        public async Task GetUserById_Should_Return_User_When_Found()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var expectedUser = new User
            {
                Id = userId,
                UserPrincipalName = "test@testdomain.com",
                DisplayName = "Test User",
                MailNickname = "testuser"
            };

            var userCollectionResponse = new UserCollectionResponse
            {
                Value = [expectedUser]
            };

            _graphUserService
                .Setup(x => x.GetUsersAsync(It.IsAny<Action<RequestConfiguration<UsersRequestBuilder.UsersRequestBuilderGetQueryParameters>>>()))
                .ReturnsAsync(userCollectionResponse);

            var dto = new GetUserByIdDto(userId);

            // Act
            var result = await _controller.GetUserById(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<GetUserResponseDto>(okResult.Value);
            Assert.Equal("User found", response.Message);
            Assert.Equal(userId, response.UserId);
            Assert.Equal("test@testdomain.com", response.UserPrincipalName);
            Assert.Equal("Test User", response.DisplayName);
            Assert.Equal("testuser", response.MailNickname);
        }

        [Fact]
        public async Task GetUserById_Should_Throw_When_User_Not_Found()
        {
            // Arrange
            var userCollectionResponse = new UserCollectionResponse
            {
                Value = []
            };

            var dto = new GetUserByIdDto(Guid.NewGuid().ToString());

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _controller.GetUserById(dto));
        }

        [Fact]
        public async Task GetUserById_Should_Throw_When_ServiceException_Occurs()
        {
            var dto = new GetUserByIdDto(Guid.NewGuid().ToString());

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _controller.GetUserById(dto));
            Assert.Contains("Graph API error", exception.Message);
        }

        #endregion

        #region GetUserByPrincipalName Tests

        [Fact]
        public async Task GetUserByPrincipalName_Should_Return_User_When_Found()
        {
            // Arrange
            var principalName = "test@testdomain.com";
            var expectedUser = new User
            {
                Id = Guid.NewGuid().ToString(),
                UserPrincipalName = principalName,
                DisplayName = "Test User",
                MailNickname = "testuser"
            };

            var userCollectionResponse = new UserCollectionResponse
            {
                Value = [expectedUser]
            };

            _graphUserService
                .Setup(x => x.GetUsersAsync(It.IsAny<Action<RequestConfiguration<UsersRequestBuilder.UsersRequestBuilderGetQueryParameters>>>()))
                .ReturnsAsync(userCollectionResponse);

            var dto = new GetUserByPrincipalNameDto(principalName);

            // Act
            var result = await _controller.GetUserByPrincipalName(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<GetUserResponseDto>(okResult.Value);
            Assert.Equal("User found", response.Message);
            Assert.Equal(principalName, response.UserPrincipalName);
        }

        [Fact]
        public async Task GetUserByPrincipalName_Should_Throw_When_User_Not_Found()
        {
            // Arrange
            var userCollectionResponse = new UserCollectionResponse
            {
                Value = []
            };

            var dto = new GetUserByPrincipalNameDto("notfound@testdomain.com");

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _controller.GetUserByPrincipalName(dto));
        }

        [Fact]
        public async Task GetUserByPrincipalName_Should_Throw_When_ServiceException_Occurs()
        {
            var dto = new GetUserByPrincipalNameDto("test@testdomain.com");

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _controller.GetUserByPrincipalName(dto));
            Assert.Contains("Graph API error", exception.Message);
        }

        #endregion

        #region CreateUser Tests

        [Fact]
        public async Task CreateUser_Should_Return_BadRequest_When_DisplayName_Is_Empty()
        {
            // Arrange
            var createDto = new CreateUserDto("", "", "newuser");

            // Act
            var result = await _controller.CreateUser(createDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("DisplayName et MailNickName are mandatory.", badRequestResult.Value);
        }

        [Fact]
        public async Task CreateUser_Should_Return_BadRequest_When_MailNickName_Is_Empty()
        {
            // Arrange
            var createDto = new CreateUserDto("newuser@testdomain.com", "New User", "");

            // Act
            var result = await _controller.CreateUser(createDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("DisplayName et MailNickName are mandatory.", badRequestResult.Value);
        }

        [Fact]
        public async Task CreateUser_Builds_UserPrincipalName_Correctly()
        {
            // Arrange
            var graphMock = new Mock<IGraphUserService>();

            graphMock
                .Setup(g => g.CreateUserAsync(It.IsAny<User>()))
                .ReturnsAsync((User u) => u);

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["AzureAd:Domain"] = "contoso.com"
                })
                .Build();

            var controller = new UsersController(graphMock.Object, config);

            var dto = new CreateUserDto
            (
                UserPrincipalName : "jdoe@contoso.com",
                DisplayName : "John Doe",
                MailNickName : "jdoe"
            );

            // Act
            var result = await controller.CreateUser(dto);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<CreateUserResponseDto>(ok.Value);

            Assert.Equal("jdoe@contoso.com", response.UserPrincipalName);

            graphMock.Verify(g =>
                g.CreateUserAsync(It.Is<User>(u =>
                    u.UserPrincipalName == "jdoe@contoso.com" &&
                    u.DisplayName == "John Doe" &&
                    u.MailNickname == "jdoe" &&
                    u.AccountEnabled == true
                )),
                Times.Once);
        }

        [Fact]
        public async Task CreateUser_Returns_500_When_User_Is_Null()
        {
            var graphMock = new Mock<IGraphUserService>();
            graphMock.Setup(g => g.CreateUserAsync(It.IsAny<User>()))
                     .ReturnsAsync((User?)null);

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["AzureAd:Domain"] = "contoso.com"
                })
                .Build();

            var controller = new UsersController(graphMock.Object, config);

            var dto = new CreateUserDto
            (
                UserPrincipalName : "john@contoso.com",
                DisplayName : "John",
                MailNickName : "john"
            );

            var result = await controller.CreateUser(dto);

            var status = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, status.StatusCode);
        }

        [Fact]
        public async Task CreateUser_Returns_Graph_StatusCode_On_ServiceException()
        {
            var graphMock = new Mock<IGraphUserService>();

            graphMock.Setup(g => g.CreateUserAsync(It.IsAny<User>()))
                .ThrowsAsync(new ServiceException(
                    "Graph error",
                    null,
                    (int)HttpStatusCode.Forbidden));

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["AzureAd:Domain"] = "contoso.com"
                })
                .Build();

            var controller = new UsersController(graphMock.Object, config);

            var dto = new CreateUserDto
            (
                UserPrincipalName: "john@contoso.com",
                DisplayName: "John",
                MailNickName: "john"
            );

            var result = await controller.CreateUser(dto);

            var status = Assert.IsType<ObjectResult>(result);
            Assert.Equal(403, status.StatusCode);
        }

        #endregion
    }
}