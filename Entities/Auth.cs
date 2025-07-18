using System.ComponentModel.DataAnnotations;

namespace Products.Entities
{
    public class Auth
    {
        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
        public string UserName { get; set; }
        
        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Password must be at least 3 characters")]
        public string Password { get; set; }
    }
}
