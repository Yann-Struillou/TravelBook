using System.Net;
using TravelBook.Client.Services;
using TravelBookDto.Users;

namespace Test.TravelBook.Client
{
    public class UsersServiceTests
    {
        private UsersService CreateService(Func<HttpRequestMessage, HttpResponseMessage> handler)
        {
            var httpClient = new HttpClient(new FakeHttpMessageHandler(handler))
            {
                BaseAddress = new Uri("http://localhost")
            };
            return new UsersService(httpClient);
        }

        private async Task TestApiCallAsync<TDto, TResult>(
            Func<UsersService, TDto, Task<TResult?>> method,
            TDto dto,
            TResult expectedSuccess,
            string expectedError = "API Error",
            bool throwOnError = false)
        {
            // Success scenario
            var service = CreateService(_ =>
                FakeHttpMessageHandler.JsonResponse(HttpStatusCode.OK, expectedSuccess!)
            );

            var result = await method(service, dto);
            Assert.NotNull(result);
            Assert.Equal(expectedSuccess, result);

            // API error scenario
            service = CreateService(_ =>
                new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent("Bad request")
                });

            result = await method(service, dto);
            if (!throwOnError)
            {
                Assert.NotNull(result);
                var errorProp = result!.GetType().GetProperty("Error")?.GetValue(result) as string;
                Assert.Contains(expectedError, errorProp ?? "");
            }
            else
            {
                await Assert.ThrowsAsync<Exception>(async () => await method(service, dto));
            }

            // Exception scenario
            service = CreateService(_ => throw new HttpRequestException("Network failure"));
            result = await method(service, dto);
            if (!throwOnError)
            {
                Assert.NotNull(result);
                var errorProp = result!.GetType().GetProperty("Error")?.GetValue(result) as string;
                Assert.Contains("Application error", errorProp ?? "");
            }
        }

        [Fact]
        public async Task GetUserByIdAsync_AllScenarios()
        {
            await TestApiCallAsync(
                (svc, dto) => svc.GetUserByIdAsync(dto),
                new GetUserByIdDto("1"),
                new GetUserResponseDto(null, "1", "John", "Doe", "john@demo.com")
            );
        }

        [Fact]
        public async Task GetUserByPrincipalAsync_AllScenarios()
        {
            await TestApiCallAsync(
                (svc, dto) => svc.GetUserByPrincipalAsync(dto),
                new GetUserByPrincipalNameDto("alice@demo.com"),
                new GetUserResponseDto(null, "2", "Alice", "Smith", "alice@demo.com")
            );
        }

        [Fact]
        public async Task CreateUserAsync_AllScenarios()
        {
            await TestApiCallAsync(
                (svc, dto) => svc.CreateUserAsync(dto),
                new CreateUserDto("Bob", "Brown", "bob@demo.com"),
                new CreateUserResponseDto(null, null, "Bob", "Brown", "bob@demo.com"),
                throwOnError: true
            );
        }
    }
}
