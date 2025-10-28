using System.ComponentModel.DataAnnotations;

namespace restapp.Models
{
    public class UserLogin
    {
        //there is no existing db table for this userlogin
        // just to take user id and password for creating a login view
        [Key]
        [Required(ErrorMessage ="Empty user id is not allowed")]
        public string UserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Empty password is not allowed")]
        public string Password { get; set; } = string.Empty;
    }
}
