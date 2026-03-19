using System.ComponentModel.DataAnnotations;

namespace RecipesAPI.DTOs
{
    public class RegisterDto
    {
        public string UserName { get; set; }
        [EmailAddress]
        public string Email { get; set; }
        public string Password { get; set; }
        public string CaptchaToken { get; set; }
    }
}
