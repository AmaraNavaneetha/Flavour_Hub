using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace restapp.Models
{
    public class OrdersDetail
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; } // Simple integer reference

        [Required]
        public int FoodItemId { get; set; } // Simple integer reference

        [Required]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal UnitPrice { get; set; }

        // Remove navigation properties: Order? Order { get; set; } and FoodItem? FoodItem { get; set; }
    }
}