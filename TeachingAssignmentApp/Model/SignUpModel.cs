using System.ComponentModel.DataAnnotations;

namespace TeachingAssignmentApp.Model
{
    public class SignUpModel
    {
        [Required]
        public string UserName { get; set; }
        [Required, EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string ConfirmPassword { get; set; }
    }
}
