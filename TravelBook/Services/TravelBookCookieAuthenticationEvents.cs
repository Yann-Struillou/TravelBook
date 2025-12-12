using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;

namespace TravelBook.Services
{
    internal class TravelBookCookieAuthenticationEvents : CookieAuthenticationEvents
    {
        public async override Task ValidatePrincipal(CookieValidatePrincipalContext context)
        {
            try
            {
                var tokenAcquisition = context.HttpContext.RequestServices.GetRequiredService<ITokenAcquisition>();
                string token = await tokenAcquisition.GetAccessTokenForUserAsync(
                    scopes: ["profile"],
                    user: context.Principal);
            }
            catch (MicrosoftIdentityWebChallengeUserException ex)
            when (AccountDoesNotExistInTokenCache(ex))
            {
                context.RejectPrincipal();
            }
        }

        /// <summary>
        /// Is the exception due to no account in the token cache?
        /// </summary>
        /// <param name="ex">Exception thrown by <see cref="ITokenAcquisition"/>.GetTokenForXX methods.</param>
        /// <returns>A boolean indicating if the exception relates to the absence of an account in the cache.</returns>
        private static bool AccountDoesNotExistInTokenCache(MicrosoftIdentityWebChallengeUserException ex)
        {
            if (ex is null || ex?.InnerException is null || ex?.InnerException is not MsalUiRequiredException)
                return false;

            return ex.InnerException is MsalUiRequiredException { ErrorCode: "user_null" };
        }
    }

}