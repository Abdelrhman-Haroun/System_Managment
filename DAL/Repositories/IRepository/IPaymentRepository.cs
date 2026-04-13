using DAL.Models;

namespace DAL.Repositories.IRepository
{
    public interface IPaymentRepository : IGenericRepository<Payment>
    {
        Task<IEnumerable<Payment>> GetAllWithDetailsAsync();
        Task<Payment?> GetByIdWithDetailsAsync(int id);
        Task<IEnumerable<Payment>> GetByPartyAsync(string partyType, int? customerId, int? supplierId, int? employeeId, string? partyName, int? excludePaymentId = null);
        Task<IEnumerable<BankAccount>> GetBankAccountsAsync();
        Task<IEnumerable<CashBox>> GetCashboxesAsync();
        Task<IEnumerable<MobileWallet>> GetMobileWalletsAsync();
    }
}
