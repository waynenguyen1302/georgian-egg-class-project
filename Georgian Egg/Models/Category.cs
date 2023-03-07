using System.ComponentModel.DataAnnotations;

namespace GeorgianEgg.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        public String? Name { get; set; }

        public List<Product>? Products { get; set; }
    }
}
