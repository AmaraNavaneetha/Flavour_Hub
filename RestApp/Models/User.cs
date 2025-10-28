using System.ComponentModel.DataAnnotations;

namespace restapp.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string Mobile { get; set; }

        [Required]
        public string Password { get; set; }
        
        [Required]
        public string UserId { get; set; }

        public bool Status { get; set; }

        //foreign key
        [Required]
        public int RoleId { get; set; }

        //navigation property - one user is mapped to only one role
        public Role? role { get; set; }
    }
}
