using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace restapp.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Required(ErrorMessage="The category name is required")]
        [MaxLength(30, ErrorMessage = "Invalid size")]
        public string CategoryName { get; set; } = string.Empty;

        [Required(ErrorMessage = "The category description is required")]
        [MaxLength(1500, ErrorMessage = "Invalid size")]
        public string CategoryDescription { get; set; } = string.Empty;

        public string CategoryImagePath { get; set; } = string.Empty;
        public bool CategoryStatus { get; set; }

        [Required(ErrorMessage = "Empty category discount is not allowed")]
        [Range(0,100,ErrorMessage ="Invalid value")]
        public int CategoryDiscount { get; set; }

        //navigation property - under one category many food items can come
        public List<FoodItem>? foodItems { get; set; }

        [NotMapped]
        public IFormFile? CategoryImage { get; set; }


    }
}
