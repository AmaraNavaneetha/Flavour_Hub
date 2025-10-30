using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace restapp.Models
{
    // Define this class inside the restapp.Controllers namespace or in your Models folder
    [NotMapped] // <-- ADD THIS ATTRIBUTE
    public class CartItem
    {
        // Define this class inside the restapp.Controllers namespace or in your Models folder
            public int FoodItemId { get; set; }
            public string Name { get; set; }
            public decimal Price { get; set; }
            public int Quantity { get; set; } = 1; // Default quantity is 1
        
    }
}