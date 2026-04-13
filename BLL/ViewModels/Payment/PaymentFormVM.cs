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
        public string PartyType { get; set; } = PaymentPartyTypes.Customer;

        public int? CustomerId { get; set; }
        public int? SupplierId { get; set; }
        public int? EmployeeId { get; set; }
        public int? BankAccountId { get; set; }
        public int? CashboxId { get; set; }
        public int? MobileWalletId { get; set; }

        [StringLength(150)]
        public string? PartyName { get; set; }

        [Required(ErrorMessage = "سبب الدفع مطلوب")]
        [StringLength(150)]
        public string Reason { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        public DateTime PaymentDate { get; set; } = DateTime.Today;

        [StringLength(500)]
        public string? Notes { get; set; }
    }
}
