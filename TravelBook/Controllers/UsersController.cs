using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Identity.Web;
using TravelBookDto.Users;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TravelBook.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = "OpenIdConnect")]
    [AuthorizeForScopes(Scopes = new string[] { "user.read", "user.readwrite.all", "device.read.all" })]
    public class UsersController : ControllerBase
    {
        private readonly GraphServiceClient _graphServiceClient;
        private readonly string _defaultDomain;

        public UsersController(GraphServiceClient graphServiceClient, IConfiguration configuration)
        {
            _graphServiceClient = graphServiceClient;

            // Récupération du domaine depuis appsettings.json
            _defaultDomain = configuration["AzureAd:Domain"] ?? "";
            if (string.IsNullOrWhiteSpace(_defaultDomain))
            {
                throw new ArgumentException("Azure domain is not set in appsettings.json");
            }
        }

        [HttpPost("GetUserById")]      
        public async Task<ActionResult<GetUserResponseDto>> GetUserById([FromBody] GetUserByIdDto getUserByIdDto)
        {
            try
            {
                var users = await _graphServiceClient.Users.GetAsync((requestConfiguration) =>
                {
                    requestConfiguration.Options.WithAuthenticationScheme(OpenIdConnectDefaults.AuthenticationScheme);
                    requestConfiguration.QueryParameters.Select = ["id", "principal", "displayName", "mailnickname"];
                    requestConfiguration.QueryParameters.Filter = $"id eq '{getUserByIdDto.UserId}'";
                });

                var user = users?.Value?.FirstOrDefault();

                return user is null
                    ? throw new Exception($"Graph API error")
                    : (ActionResult<GetUserResponseDto>)Ok(new GetUserResponseDto(
                        "User found",
                        user.Id,
                        user.UserPrincipalName,
                        user.DisplayName,
                        user.MailNickname));
            }
            catch (ServiceException ex)
            {
                throw new ArgumentException($"Graph API error: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"TravelBook API error: {ex.Message}", ex);
            }
        }

        [HttpPost("GetUserByPrincipalName")]
        public async Task<ActionResult<GetUserResponseDto>> GetUserByPrincipalName([FromBody] GetUserByPrincipalNameDto getUserByPrincipalNameDto)
        {
            try
            {
                var users = await _graphServiceClient.Users.GetAsync((requestConfiguration) =>
                {
                    requestConfiguration.Options.WithAuthenticationScheme(OpenIdConnectDefaults.AuthenticationScheme);
                    requestConfiguration.QueryParameters.Select = ["id", "principal", "displayName", "mailnickname"];
                    requestConfiguration.QueryParameters.Filter = $"userPrincipalName eq '{getUserByPrincipalNameDto.UserPrincipalName}'";
                });

                var user = users?.Value?.FirstOrDefault();

                return user is null
                    ? throw new Exception($"Graph API error")
                    : (ActionResult<GetUserResponseDto>)Ok(new GetUserResponseDto(
                        "User found",
                        user.Id,
                        user.UserPrincipalName,
                        user.DisplayName,
                        user.MailNickname));
            }
            catch (ServiceException ex)
            {
                throw new ArgumentException($"Graph API error: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"TravelBook API error: {ex.Message}", ex);
            }
        }

        [HttpPost("CreateUser")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto createUserDto)
        {
            if (createUserDto == null
                || string.IsNullOrWhiteSpace(createUserDto.DisplayName)
                || string.IsNullOrWhiteSpace(createUserDto.MailNickName))
            {
                return BadRequest("DisplayName et MailNickName are mandatory.");
            }

            // Construction du UserPrincipalName avec le domaine configuré
            var userPrincipalName = $"{createUserDto.MailNickName}@{_defaultDomain}";

            var newUser = new User
            {
                AccountEnabled = true,
                DisplayName = createUserDto.DisplayName,
                MailNickname = createUserDto.MailNickName,
                UserPrincipalName = userPrincipalName,
                PasswordProfile = new PasswordProfile
                {
                    ForceChangePasswordNextSignIn = true,
                    Password = Guid.NewGuid().ToString("N") + "Aa1!"
                }
            };

            try
            {
                var createdUser = await _graphServiceClient.Users.PostAsync(newUser);

                if (createdUser == null)
                    return StatusCode(500, "The user registration failed.");

                Console.WriteLine($"User created: {createdUser.DisplayName}");

                return Ok(new CreateUserResponseDto(
                    "User created successfully",
                    createdUser.Id,
                    createdUser.UserPrincipalName,
                    createdUser.DisplayName,
                    createdUser.MailNickname
                ));
            }
            catch (ServiceException ex)
            {
                Console.WriteLine($"Erreur Graph API : {ex.Message}");
                return StatusCode(ex.ResponseStatusCode, new { error = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
