using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace TravelBook.Services
{
    public static class AuthenticationService
    {
        public static void MapAuthenticationService(this IEndpointRouteBuilder endpoints)
        {
            var group = endpoints.MapGroup("Authentication");

            group.MapGet("/LogIn", () =>
            {
                return Results.Challenge(new AuthenticationProperties
                {
                    RedirectUri = "/"
                },
                authenticationSchemes: [OpenIdConnectDefaults.AuthenticationScheme]);
            })
            .AllowAnonymous();

            group.MapPost("/LogOut", () =>
            {
                return Results.SignOut(authenticationSchemes: [CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme]);
            });
        }
    }
}
