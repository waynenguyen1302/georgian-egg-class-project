using System.ComponentModel.DataAnnotations;

namespace GeorgianEgg.Models

{
    public class CartLine
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        [Required]
        public String CustomerId { get; set; }
        public int ProductId { get; set; }

        public Product? Product { get; set; }
    }
}
