using System.ComponentModel.DataAnnotations;

namespace BLL.ViewModels.PaymentMethod
{
    public class PaymentMethodFormVM
    {
        [Required]
        public string Type { get; set; } = "CashBox";
        public int Id { get; set; }

        [Required(ErrorMessage = "الاسم مطلوب")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(100)]
        public string? SecondaryName { get; set; }

        [StringLength(50)]
        public string? Code { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "الرصيد لا يمكن أن يكون سالباً")]
        public decimal Balance { get; set; }
    }
}
