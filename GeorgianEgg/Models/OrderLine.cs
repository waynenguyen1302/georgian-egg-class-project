﻿using System.ComponentModel.DataAnnotations;

namespace GeorgianEgg.Models
{
    public class OrderLine
    {
        public int Id { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        [DisplayFormat(DataFormatString = "{0:c}")]
        public decimal Price { get; set; }


        [Required]
        public int ProductId { get; set; }

        [Required]
        public int OrderId { get; set; }

        public Product? Product { get; set; }
        public Order? Order { get; set; }
    }
}