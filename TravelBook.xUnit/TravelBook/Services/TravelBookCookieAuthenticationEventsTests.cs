using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using Moq;
using System.Security.Claims;
using TravelBook.Services;

namespace TravelBook.xUnit.TravelBook.Services
{
    public class TravelBookCookieAuthenticationEventsTests
    {
        [Fact]
        public async Task ValidatePrincipal_DoesNotReject_When_Token_Is_Acquired()
        {
            // Arrange
            var tokenAcquisitionMock = new Mock<ITokenAcquisition>();
            tokenAcquisitionMock
                .Setup(t => t.GetAccessTokenForUserAsync(
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(), 
                    It.IsAny<string?>(),
                    It.IsAny<ClaimsPrincipal?>(),
                    null))
                .ReturnsAsync("fake-token");

            var services = new ServiceCollection();
            services.AddSingleton(tokenAcquisitionMock.Object);

            var httpContext = new DefaultHttpContext
            {
                RequestServices = services.BuildServiceProvider()
            };

            var identity = new ClaimsIdentity("Cookies");
            identity.AddClaim(new Claim(ClaimTypes.Name, "test"));
            var principal = new ClaimsPrincipal(identity);
            var properties = new AuthenticationProperties();
            var ticket = new AuthenticationTicket(principal, properties, CookieAuthenticationDefaults.AuthenticationScheme);

            var context = new CookieValidatePrincipalContext(
                httpContext,
                new AuthenticationScheme("Cookies", null, typeof(CookieAuthenticationHandler)),
                new CookieAuthenticationOptions(),
                ticket);

            var events = new TravelBookCookieAuthenticationEvents();

            // Act
            await events.ValidatePrincipal(context);

            // Assert
            Assert.NotNull(context.Principal);
            Assert.False(context.ShouldRenew);
        }

        [Fact]
        public async Task ValidatePrincipal_Rejects_When_User_Not_In_TokenCache()
        {
            // Arrange
            var msalException = new MsalUiRequiredException("user_null", "User missing");

            var challengeException =
                new MicrosoftIdentityWebChallengeUserException(
                    msalException,
                    ["profile"]);

            var tokenAcquisitionMock = new Mock<ITokenAcquisition>();
            tokenAcquisitionMock
                .Setup(t => t.GetAccessTokenForUserAsync(
                    scopes : It.IsAny<IEnumerable<string>>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    user: It.IsAny<ClaimsPrincipal?>(), 
                    It.IsAny<TokenAcquisitionOptions?>()))
                .ThrowsAsync(challengeException);

            var services = new ServiceCollection();
            services.AddSingleton(tokenAcquisitionMock.Object);

            var httpContext = new DefaultHttpContext
            {
                RequestServices = services.BuildServiceProvider()
            };

            var principal = new ClaimsPrincipal(new ClaimsIdentity("Cookies"));
            var properties = new AuthenticationProperties();
            var ticket = new AuthenticationTicket(principal, properties, CookieAuthenticationDefaults.AuthenticationScheme);
            var context = new CookieValidatePrincipalContext(
                httpContext,
                new AuthenticationScheme("Cookies", null, typeof(CookieAuthenticationHandler)),
                new CookieAuthenticationOptions(),
                ticket);

            var events = new TravelBookCookieAuthenticationEvents();

            // Act
            await events.ValidatePrincipal(context);

            // Assert
            Assert.Null(context.Principal); // RejectPrincipal() appelé
        }

        [Fact]
        public async Task ValidatePrincipal_DoesNotReject_For_Other_MsalErrors()
        {
            var msalException = new MsalUiRequiredException("some_other_error", "Other error");

            var challengeException =
                new MicrosoftIdentityWebChallengeUserException(
                    msalException,
                    ["profile"]);

            var tokenAcquisitionMock = new Mock<ITokenAcquisition>();
            tokenAcquisitionMock
                .Setup(t => t.GetAccessTokenForUserAsync(
                    scopes: It.IsAny<IEnumerable<string>>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    user: It.IsAny<ClaimsPrincipal?>(),
                    It.IsAny<TokenAcquisitionOptions?>()))
                .ThrowsAsync(challengeException);

            var services = new ServiceCollection();
            services.AddSingleton(tokenAcquisitionMock.Object);

            var httpContext = new DefaultHttpContext
            {
                RequestServices = services.BuildServiceProvider()
            };

            var principal = new ClaimsPrincipal(new ClaimsIdentity("Cookies"));
            var properties = new AuthenticationProperties();
            var ticket = new AuthenticationTicket(principal, properties, CookieAuthenticationDefaults.AuthenticationScheme);
            var context = new CookieValidatePrincipalContext(
                httpContext,
                new AuthenticationScheme("Cookies", null, typeof(CookieAuthenticationHandler)),
                new CookieAuthenticationOptions(),
                ticket);

            var events = new TravelBookCookieAuthenticationEvents();

            // Act
            await events.ValidatePrincipal(context);

            // Assert
            Assert.NotNull(context.Principal);
        }
    }
}
