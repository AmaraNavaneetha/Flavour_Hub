using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace restapp.Models
{
    public enum AvailableType
    {
        Breakfast,
        Lunch,
        Dinner
    }
    public class FoodItem
    {
        [Key]
        public int ItemId { get; set; }

        [Required(ErrorMessage ="Empty item name is not allowed")]
        [MaxLength(50,ErrorMessage ="Invalid size")]
        public string ItemName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Empty item description is not allowed")]
        [MaxLength(1500, ErrorMessage = "Invalid size")]
        public string ItemDescription { get; set; } = string.Empty;

        [Required(ErrorMessage = "Empty actual price is not allowed")]
        public int ActualPrice { get; set; }

        [Required(ErrorMessage = "Empty discount price is not allowed")]
        [Range(0,100,ErrorMessage="Invalid discount")]
        public int DiscountPer { get; set; }
        public int SellingPrice { get; set; }
        public string Rating { get; set; } = string.Empty;
        public int RatingCount { get; set; }
        public string ItemImagePath { get; set; } = string.Empty;

        public bool IsAvailable { get; set; }
        public bool IsBestSeller { get; set; }
        public bool IsFastMoving { get; set; }
        public bool IsBreakfast { get; set; }
        public bool IsLunch { get; set; }
        public bool IsDinner { get; set; }
        //[AllowedValues("Breakfast", "Lunch", "Dinner")]
        //public string AvailableType { get; set; } = string.Empty;
        //public AvailableType AvailableType { get; set; }


        //foreign keys
        public int CategoryId { get; set; }
        public int ItemTypeId { get; set; }
        //navigation property
        public Category? category { get; set; }
        public ItemType? itemType { get; set; }

        //not mapped property to handle image
        [NotMapped]
        public IFormFile? ItemImage { get; set; }

    }
}
