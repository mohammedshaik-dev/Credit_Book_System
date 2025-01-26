using System.ComponentModel.DataAnnotations;

namespace Credit_Book_System.Models
{
    public class CreditEntry
    {
        public int Id { get; set; }
        [Required]
        public DateTime Date { get; set; }
        [Required]
        public string ItemName { get; set; } = string.Empty;
        [Required]
        public int Quantity { get; set; }
        [Required]
        public decimal Rate { get; set; }
        public decimal TotalAmount => Quantity * Rate;
        public decimal RemainingBalance { get; set; }
    }
}
