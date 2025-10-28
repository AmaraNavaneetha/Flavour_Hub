using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace restapp.Models
{
    public class ItemType
    {
        [Key]
        public int ItemTypeId { get; set; }

        [Required(ErrorMessage = "Empty name is not allowed")]
        [MaxLength(20, ErrorMessage = "Invalid size")]
        public string ItemTypeName { get; set; } = string.Empty;

        // nav property
        public List<FoodItem>? foodItems { get; set; }

    }
}
