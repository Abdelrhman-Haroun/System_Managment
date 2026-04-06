using DAL.Models;
using System.ComponentModel.DataAnnotations;

namespace BLL.ViewModels.Payment
{
    public class PaymentFormVM
    {
        public int Id { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "المبلغ يجب أن يكون أكبر من صفر")]
        public decimal Amount { get; set; }

        [Required]
        public PaymentMethodType PaymentMethod { get; set; }

        [Required]
        public string PartyType { get; set; } = "Customer";

        public int? CustomerId { get; set; }
        public int? SupplierId { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }
    }
}
