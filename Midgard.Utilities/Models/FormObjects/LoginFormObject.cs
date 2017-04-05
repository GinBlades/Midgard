using System.ComponentModel.DataAnnotations;

namespace Midgard.Utilities.Models.FormObjects
{
    public class LoginFormObject
    {
        [Required, MaxLength(60)]
        public string UsernameOrEmail { get; set; }
        [Required, MaxLength(30)]
        public string Password { get; set; }
        public bool RememberMe { get; set; }
        [MaxLength(120)]
        public string ReturnUrl { get; set; }
    }
}
