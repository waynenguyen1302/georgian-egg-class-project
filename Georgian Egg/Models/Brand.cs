using System.ComponentModel.DataAnnotations;

namespace GeorgianEgg.Models
{
    public class Brand
    {
        public int Id { get; set; }
        
        [Required]
        [MinLength(3)]
        public String? Name { get; set; }

        public List<Product>? Products { get; set; }
    }
}
