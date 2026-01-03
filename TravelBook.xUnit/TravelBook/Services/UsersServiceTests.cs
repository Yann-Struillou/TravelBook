using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using TravelBook.Client.Services;
using TravelBookDto.Users;

namespace TravelBook.xUnit.TravelBook.Services
{
    public class UsersServiceTests
    {
        [Fact]
        public async Task GetUserByIdAsync_Returns_User_When_Success()
        {
            // Arrange
            var responseDto = new GetUserResponseDto(
                "User found",
                "id1",
                "user@domain.com",
                "User",
                "user");

            var handler = new FakeHttpMessageHandler(_ =>
                FakeHttpMessageHandler.JsonResponse(HttpStatusCode.OK, responseDto));

            var httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri("http://localhost/")
            };

            var service = new UsersService(httpClient);

            // Act
            var result = await service.GetUserByIdAsync(new GetUserByIdDto("id1"));

            // Assert
            Assert.NotNull(result);
            Assert.Equal("User found", result.Message);
            Assert.Equal("id1", result.UserId);
        }

        [Fact]
        public async Task GetUserByIdAsync_Returns_ApiError_When_NotSuccess()
        {
            var handler = new FakeHttpMessageHandler(_ =>
                new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent("Bad request")
                });

            var httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri("http://localhost/")
            };

            var service = new UsersService(httpClient);

            var result = await service.GetUserByIdAsync(new GetUserByIdDto("id"));

            Assert.NotNull(result);
            Assert.StartsWith("API Error", result.Message);
        }

        [Fact]
        public async Task GetUserByIdAsync_Returns_ApplicationError_On_Exception()
        {
            var handler = new FakeHttpMessageHandler(_ =>
                throw new HttpRequestException("network error"));

            var httpClient = new HttpClient(handler);

            var service = new UsersService(httpClient);

            var result = await service.GetUserByIdAsync(new GetUserByIdDto("id"));

            Assert.NotNull(result);
            Assert.StartsWith("Application error", result.Message);
        }

        [Fact]
        public async Task GetUserByPrincipalAsync_Returns_User_When_Success()
        {
            var responseDto = new GetUserResponseDto(
                "User found",
                "id2",
                "user@domain.com",
                "User",
                "user");

            var handler = new FakeHttpMessageHandler(_ =>
                FakeHttpMessageHandler.JsonResponse(HttpStatusCode.OK, responseDto));

            var httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri("http://localhost/")
            };

            var service = new UsersService(httpClient);

            var result = await service.GetUserByPrincipalAsync(
                new GetUserByPrincipalNameDto("user@domain.com"));

            Assert.NotNull(result);
            Assert.Equal("User found", result.Message);
        }

        [Fact]
        public async Task CreateUserAsync_Returns_Response_When_Success()
        {
            var responseDto = new CreateUserResponseDto(
                "User created",
                "id3",
                "user@domain.com",
                "User",
                "user");

            var handler = new FakeHttpMessageHandler(_ =>
                FakeHttpMessageHandler.JsonResponse(HttpStatusCode.OK, responseDto));

            var httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri("http://localhost/")
            };

            var service = new UsersService(httpClient);

            var result = await service.CreateUserAsync(
                new CreateUserDto("user@contoso.com", "User", "user"));

            Assert.NotNull(result);
            Assert.Equal("User created", result.Message);
        }

        [Fact]
        public async Task CreateUserAsync_Throws_When_Api_Error()
        {
            var handler = new FakeHttpMessageHandler(_ =>
                FakeHttpMessageHandler.JsonResponse(
                    HttpStatusCode.BadRequest,
                    new Dictionary<string, string>
                    {
                        ["Error"] = "Creation failed"
                    }));

            var httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri("http://localhost/")
            };

            var service = new UsersService(httpClient);

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                service.CreateUserAsync(new CreateUserDto("user@contoso.com","User", "user")));
        }

        [Fact]
        public async Task GetUserByIdAsync_Returns_ApiError_With_ErrorMessage_From_Response_Body()
        {
            // Arrange
            const string apiErrorMessage = "User not found";

            var handler = new FakeHttpMessageHandler(_ =>
                new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent(apiErrorMessage)
                });

            var httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri("http://localhost/")
            };

            var service = new UsersService(httpClient);

            // Act
            var result = await service.GetUserByIdAsync(
                new GetUserByIdDto("invalid-id"));

            // Assert
            Assert.NotNull(result);
            Assert.Equal($"API Error: {apiErrorMessage}", result.Message);
            Assert.Null(result.UserId);
            Assert.Null(result.UserPrincipalName);
        }

        [Fact]
        public async Task GetUserByPrincipalAsync_Returns_ApiError_With_ErrorMessage_From_Response_Body()
        {
            const string apiErrorMessage = "Invalid principal name";

            var handler = new FakeHttpMessageHandler(_ =>
                new HttpResponseMessage(HttpStatusCode.Forbidden)
                {
                    Content = new StringContent(apiErrorMessage)
                });

            var httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri("http://localhost/")
            };

            var service = new UsersService(httpClient);

            var result = await service.GetUserByPrincipalAsync(
                new GetUserByPrincipalNameDto("invalid@domain.com"));

            Assert.NotNull(result);
            Assert.Equal($"API Error: {apiErrorMessage}", result.Message);
        }

        [Fact]
        public async Task GetUserByIdAsync_Returns_ApplicationError_When_HttpClient_Throws()
        {
            // Arrange
            const string exceptionMessage = "Network failure";

            var handler = new FakeHttpMessageHandler(_ =>
                throw new HttpRequestException(exceptionMessage));

            var httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri("http://localhost/")
            };

            var service = new UsersService(httpClient);

            // Act
            var result = await service.GetUserByIdAsync(
                new GetUserByIdDto("any-id"));

            // Assert
            Assert.NotNull(result);
            Assert.Equal($"Application error: {exceptionMessage}", result.Message);
            Assert.Null(result.UserId);
            Assert.Null(result.DisplayName);
        }

        [Fact]
        public async Task GetUserByPrincipalAsync_Returns_ApplicationError_When_HttpClient_Throws()
        {
            const string exceptionMessage = "Timeout";

            var handler = new FakeHttpMessageHandler(_ =>
                throw new TaskCanceledException(exceptionMessage));

            var httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri("http://localhost/")
            };

            var service = new UsersService(httpClient);

            var result = await service.GetUserByPrincipalAsync(
                new GetUserByPrincipalNameDto("test@domain.com"));

            Assert.NotNull(result);
            Assert.Equal($"Application error: {exceptionMessage}", result.Message);
        }

    }
}
