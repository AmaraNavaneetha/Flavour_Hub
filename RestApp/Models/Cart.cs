using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace restapp.Models
{
    public class Cart
    {
        [Key]
        public int CartId { get; set; } // Primary Key

        
        // [ForeignKey("User")] // Standard approach (references User navigation property)
        public string UserId { get; set; } // Foreign Key to Users table (Username)

        public int ItemId { get; set; } // Foreign Key to FoodItem table (ItemId)

        [Required]
        public int Quantity { get; set; }

        [Required]
        public decimal SellingPrice { get; set; } // Price at time of purchase

        // Navigation Properties
        public User? User { get; set; }
        public FoodItem? FoodItem { get; set; }
    }
}