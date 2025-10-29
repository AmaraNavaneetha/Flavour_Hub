using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace restapp.Models
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        [Required(ErrorMessage ="First name is required")]
        [MaxLength(50,ErrorMessage ="Invalid size")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [MaxLength(50, ErrorMessage = "Invalid size")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [MaxLength(50, ErrorMessage = "Invalid size")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Mobile number is required")]
        [MaxLength(10, ErrorMessage = "Invalid size")]
        public string Mobile { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [MaxLength(50, ErrorMessage = "Invalid size")]
        [DataType(DataType.Password)]
        [StringLength(15, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 15 characters")]

        public string Password { get; set; }

        [Required(ErrorMessage = "User id is required")]
        [MaxLength(50, ErrorMessage = "Invalid size")]
        public string UserId { get; set; }

        [Required]
        public bool Status { get; set; }

        //foreign key
        public int RoleId { get; set; }

        //navigation property - one user is mapped to only one role
        public Role? role { get; set; }
    }
}
