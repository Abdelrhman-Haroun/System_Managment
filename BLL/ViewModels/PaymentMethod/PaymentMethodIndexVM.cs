using DAL.Models;

namespace BLL.ViewModels.PaymentMethod
{
    public class PaymentMethodIndexVM
    {
        public IEnumerable<BankAccount> BankAccounts { get; set; } = Enumerable.Empty<BankAccount>();
        public IEnumerable<CashBox> Cashboxes { get; set; } = Enumerable.Empty<CashBox>();
        public IEnumerable<MobileWallet> MobileWallets { get; set; } = Enumerable.Empty<MobileWallet>();
    }
}
