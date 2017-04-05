using System.ComponentModel.DataAnnotations;

namespace Midgard.Utilities.Models.FormObjects
{
    public class RegisterFormObject
    {
        [EmailAddress, Required, MaxLength(60)]
        public string Email { get; set; }
        [Required, MaxLength(30)]
        public string UserName { get; set; }
        [Required, MaxLength(30)]
        public string Password { get; set; }
        [Required, Compare("Password")]
        public string PasswordConfirmation { get; set; }
        [MaxLength(120)]
        public string ReturnUrl { get; set; }

        public User ToUser()
        {
            return new User()
            {
                Email = Email,
                UserName = UserName,
                Password = Password
            };
        }
    }
}
