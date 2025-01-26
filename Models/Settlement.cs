using System.ComponentModel.DataAnnotations;

namespace Credit_Book_System.Models
{
    public class Settlement
    {
        public int Id { get; set; }
        [Required]
        public DateTime Date { get; set; }
        [Required]
        public decimal AmountPaid { get; set; }
        public decimal RemainingBalance { get; set; }
        public int CreditEntryId { get; set; }
    }
}
