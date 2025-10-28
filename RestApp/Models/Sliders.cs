using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace restapp.Models
{
    public class Sliders
    {
        [Key]
        public int SliderId { get; set; }

        [Required]
        public string? Name { get; set; }
        public string? DisplayText { get; set; }
        public string? LinkText { get; set; }
        public string? SliderImagePath { get; set; }

        [Required]
        public bool Status { get; set; }

        [Required]
        public int DisplayOrderNo { get; set; }

        [NotMapped]
        //Iformfile is a interface, because we are not
        //fixing the file because file can be text, word, image, audio
        public IFormFile? SliderImage { get; set; }

    }
}
