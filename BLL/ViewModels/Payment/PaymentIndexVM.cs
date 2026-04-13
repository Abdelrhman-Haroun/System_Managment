namespace BLL.ViewModels.Payment
{
    public class PaymentIndexVM
    {
        public IEnumerable<PaymentListVM> Payments { get; set; } = Enumerable.Empty<PaymentListVM>();
        public IEnumerable<PaymentBalanceVM> Balances { get; set; } = Enumerable.Empty<PaymentBalanceVM>();
        public decimal TotalIncoming { get; set; }
        public decimal TotalOutgoing { get; set; }
        public int TotalCount { get; set; }
    }
}
