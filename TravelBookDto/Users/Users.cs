namespace TravelBookDto.Users
{
    public record GetUserByIdDto(string UserId);
    public record GetUserByPrincipalNameDto(string UserPrincipalName);
    public record GetUserResponseDto(string? Message, string? UserId, string? UserPrincipalName, string? DisplayName, string? MailNickName);
    public record CreateUserDto(string UserPrincipalName, string DisplayName, string MailNickName);
    public record CreateUserResponseDto(string? Message, string? UserId, string? UserPrincipalName, string? UserDisplayName, string? UserMailNickname);
}
