using System.ComponentModel.DataAnnotations;

namespace TravelBook.Client.ViewModels.Users
{
    public class CreateUserFormModel
    {
        [Required(ErrorMessage = "Le nom complet est obligatoire.")]
        public string DisplayName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le surnom (MailNickname) est obligatoire.")]
        [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Le MailNickname ne doit contenir que des lettres et chiffres.")]
        public string MailNickName { get; set; } = string.Empty;

        [Required(ErrorMessage = "UserPrincipalName est obligatoire.")]
        [EmailAddress(ErrorMessage = "UserPrincipalName doit être un email valide.")]
        public string UserPrincipalName { get; set; } = string.Empty;
    }
}