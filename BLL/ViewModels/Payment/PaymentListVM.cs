using DAL.Models;

namespace BLL.ViewModels.Payment
{
    public class PaymentListVM
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public PaymentMethodType PaymentMethod { get; set; }
        public bool IsIncoming { get; set; }
        public string PartyName { get; set; } = string.Empty;
        public string PartyType { get; set; } = string.Empty;
        public string ReferenceNumber { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
