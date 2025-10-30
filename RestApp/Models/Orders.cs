using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace restapp.Models
{
    public class Orders
    {
        [Key]
        public int Id { get; set; } // Primary Key for the Order

        // UserId is a simple string column here, not a formal FK object.
        [Required]
        [MaxLength(50)] // Ensure this matches the length in your User table
        public string UserId { get; set; }

        [Required]
        public DateTime OrderDate { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalAmount { get; set; }

        [Required]
        [MaxLength(20)]
        public string PaymentMethod { get; set; }

        [Required]
        [MaxLength(50)]
        public string OrderStatus { get; set; }

        // Remove the navigation property: public ICollection<OrderDetail>? OrderDetails { get; set; } 
        // to keep it simple, or keep it if OrderDetail still references Order.
    }
}