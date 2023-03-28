using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace GeorgianEgg.Models
{
    public class Order
    {
        public int Id { get; set; }

        [Required]
        public bool InProgress { get; set; }

        [Required]
        [DisplayName("Order Date")]
        public DateTime OrderDate { get; set; }

        [Required]
        [DisplayFormat(DataFormatString = "{0:c}")]
        public decimal Total { get; set; }

        [Required]
        [DisplayName("First Name")]
        public String FirstName { get; set; }

        [Required]
        [DisplayName("Last Name")]
        public String LastName { get; set; }

        [Required]
        [MaxLength(255)]
        public String Address { get; set; }

        [Required]
        [MaxLength(50)]
        public String City { get; set; }

        [Required]
        [MaxLength(2)]
        public String Province { get; set; }

        [Required]
        [MaxLength(10)]
        public String PostalCode { get; set; }

        [Required]
        [Phone]
        public String Phone { get; set; }

        [Required]
        [DisplayName("Customer")]
        public String CustomerId { get; set; }

        public List<OrderLine>? OrderLines { get; set; }
    }
}