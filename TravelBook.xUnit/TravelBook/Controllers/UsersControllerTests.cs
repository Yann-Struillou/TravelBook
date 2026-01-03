using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Serialization;
using Moq;
using TravelBook.Controllers;
using TravelBookDto.Users;

namespace TravelBook.xUnit.TravelBook.Controllers
{
    public class UsersControllerTests
    {
        private readonly Mock<IRequestAdapter> _mockRequestAdapter;
        private readonly GraphServiceClient _graphServiceClient;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly UsersController _controller;
        private const string TestDomain = "testdomain.com";

        private static (UsersController controller, Mock<IRequestAdapter> mockAdapter) CreateController
        {
            get
            {
                var mockRequestAdapter = new Mock<IRequestAdapter>();
                var graphServiceClient = new GraphServiceClient(mockRequestAdapter.Object);
                var mockConfiguration = new Mock<IConfiguration>();
                mockConfiguration.Setup(c => c["AzureAd:Domain"]).Returns(TestDomain);

                var controller = new UsersController(graphServiceClient, mockConfiguration.Object);
                return (controller, mockRequestAdapter);
            }
        }

        public UsersControllerTests()
        {
            // Créer un mock du RequestAdapter au lieu du GraphServiceClient directement
            _mockRequestAdapter = new Mock<IRequestAdapter>();
            _graphServiceClient = new GraphServiceClient(_mockRequestAdapter.Object);

            _mockConfiguration = new Mock<IConfiguration>();

            // Configuration du domaine par défaut
            _mockConfiguration.Setup(c => c["AzureAd:Domain"]).Returns(TestDomain);

            _controller = new UsersController(_graphServiceClient, _mockConfiguration.Object);
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_Should_Throw_When_Domain_Is_Null()
        {
            // Arrange
            var mockConfig = new Mock<IConfiguration>();
            mockConfig.Setup(c => c["AzureAd:Domain"]).Returns(value: null);

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                new UsersController(_graphServiceClient, mockConfig.Object));
        }

        [Fact]
        public void Constructor_Should_Throw_When_Domain_Is_Empty()
        {
            // Arrange
            var mockConfig = new Mock<IConfiguration>();
            mockConfig.Setup(c => c["AzureAd:Domain"]).Returns("");

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                new UsersController(_graphServiceClient, mockConfig.Object));
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

            _mockRequestAdapter
                .Setup(x => x.SendAsync(
                    It.IsAny<RequestInformation>(),
                    It.IsAny<ParsableFactory<UserCollectionResponse>>(),
                    It.IsAny<Dictionary<string, ParsableFactory<IParsable>>>(),
                    It.IsAny<CancellationToken>()))
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

            _mockRequestAdapter
                .Setup(x => x.SendAsync(
                    It.IsAny<RequestInformation>(),
                    It.IsAny<ParsableFactory<UserCollectionResponse>>(),
                    It.IsAny<Dictionary<string, ParsableFactory<IParsable>>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(userCollectionResponse);

            var dto = new GetUserByIdDto(Guid.NewGuid().ToString());

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _controller.GetUserById(dto));
        }

        [Fact]
        public async Task GetUserById_Should_Throw_When_ServiceException_Occurs()
        {
            // Arrange
            _mockRequestAdapter
                .Setup(x => x.SendAsync(
                    It.IsAny<RequestInformation>(),
                    It.IsAny<ParsableFactory<UserCollectionResponse>>(),
                    It.IsAny<Dictionary<string, ParsableFactory<IParsable>>>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ServiceException("Graph API Error"));

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

            _mockRequestAdapter
                .Setup(x => x.SendAsync(
                    It.IsAny<RequestInformation>(),
                    It.IsAny<ParsableFactory<UserCollectionResponse>>(),
                    It.IsAny<Dictionary<string, ParsableFactory<IParsable>>>(),
                    It.IsAny<CancellationToken>()))
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

            _mockRequestAdapter
                .Setup(x => x.SendAsync(
                    It.IsAny<RequestInformation>(),
                    It.IsAny<ParsableFactory<UserCollectionResponse>>(),
                    It.IsAny<Dictionary<string, ParsableFactory<IParsable>>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(userCollectionResponse);

            var dto = new GetUserByPrincipalNameDto("notfound@testdomain.com");

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _controller.GetUserByPrincipalName(dto));
        }

        [Fact]
        public async Task GetUserByPrincipalName_Should_Throw_When_ServiceException_Occurs()
        {
            // Arrange
            _mockRequestAdapter
                .Setup(x => x.SendAsync(
                    It.IsAny<RequestInformation>(),
                    It.IsAny<ParsableFactory<UserCollectionResponse>>(),
                    It.IsAny<Dictionary<string, ParsableFactory<IParsable>>>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ServiceException("Graph API Error"));

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

        //[Fact]
        //public async Task CreateUser_Should_Return_500_When_Creation_Returns_Null()
        //{
        //    // Arrange
        //    var createDto = new CreateUserDto("newuser@testdomain.com", "New User", "newuser");

        //    _mockRequestAdapter
        //        .Setup(x => x.SendAsync(
        //            It.IsAny<RequestInformation>(),
        //            It.IsAny<ParsableFactory<User>>(),
        //            It.IsAny<Dictionary<string, ParsableFactory<IParsable>>>(),
        //            It.IsAny<CancellationToken>()))
        //        .ReturnsAsync(value: null);

        //    // Act
        //    var result = await _controller.CreateUser(createDto);

        //    // Assert
        //    var statusCodeResult = Assert.IsType<ObjectResult>(result);
        //    Assert.Equal(500, statusCodeResult.StatusCode);
        //}

        //[Fact]
        //public async Task CreateUser_Should_Return_Error_When_ServiceException_Occurs()
        //{
        //    // Arrange
        //    var createDto = new CreateUserDto("newuser@testdomain.com", "New User", "newuser");

        //    var serviceException = new ServiceException("Conflict", null, 409);

        //    _mockRequestAdapter
        //        .Setup(x => x.SendAsync(
        //            It.IsAny<RequestInformation>(),
        //            It.IsAny<ParsableFactory<User>>(),
        //            It.IsAny<Dictionary<string, ParsableFactory<IParsable>>>(),
        //            It.IsAny<CancellationToken>()))
        //        .ThrowsAsync(serviceException);

        //    // Act
        //    var result = await _controller.CreateUser(createDto);

        //    // Assert
        //    var statusCodeResult = Assert.IsType<ObjectResult>(result);
        //    Assert.NotEqual(200, statusCodeResult.StatusCode);
        //}

        //[Fact]
        //public async Task CreateUser_Should_Return_500_When_Generic_Exception_Occurs()
        //{
        //    // Arrange
        //    var createDto = new CreateUserDto("newuser@testdomain.com", "New User", "newuser");

        //    _mockRequestAdapter
        //        .Setup(x => x.SendAsync(
        //            It.IsAny<RequestInformation>(),
        //            It.IsAny<ParsableFactory<User>>(),
        //            It.IsAny<Dictionary<string, ParsableFactory<IParsable>>>(),
        //            It.IsAny<CancellationToken>()))
        //        .ThrowsAsync(new Exception("Unexpected error"));

        //    // Act
        //    var result = await _controller.CreateUser(createDto);

        //    // Assert
        //    var statusCodeResult = Assert.IsType<ObjectResult>(result);
        //    Assert.Equal(500, statusCodeResult.StatusCode);
        //}

        #endregion
    }
}