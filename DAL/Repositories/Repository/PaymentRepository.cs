using DAL.Data;
using DAL.Models;
using DAL.Repositories.IRepository;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories.Repository
{
    public class PaymentRepository : GenericRepository<Payment>, IPaymentRepository
    {
        private readonly ApplicationDbContext _context;

        public PaymentRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Payment>> GetAllWithDetailsAsync()
        {
            return await _context.Payments
                .Include(p => p.Customer)
                .Include(p => p.Supplier)
                .Include(p => p.Employee)
                .Include(p => p.BankAccount)
                .Include(p => p.CashBox)
                .Include(p => p.MobileWallet)
                .Where(p => !p.IsDeleted)
                .OrderByDescending(p => p.PaymentDate)
                .ThenByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<Payment?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Payments
                .Include(p => p.Customer)
                .Include(p => p.Supplier)
                .Include(p => p.Employee)
                .Include(p => p.BankAccount)
                .Include(p => p.CashBox)
                .Include(p => p.MobileWallet)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
        }

        public async Task<IEnumerable<Payment>> GetByPartyAsync(
            string partyType,
            int? customerId,
            int? supplierId,
            int? employeeId,
            string? partyName,
            int? excludePaymentId = null)
        {
            var query = _context.Payments
                .Where(p => !p.IsDeleted && p.PartyType == partyType);

            query = partyType switch
            {
                PaymentPartyTypes.Customer => query.Where(p => p.CustomerId == customerId),
                PaymentPartyTypes.Supplier => query.Where(p => p.SupplierId == supplierId),
                PaymentPartyTypes.Employee => query.Where(p => p.EmployeeId == employeeId),
                PaymentPartyTypes.Expense => query.Where(p => p.PartyName == partyName),
                _ => query.Where(p => false)
            };

            if (excludePaymentId.HasValue)
            {
                query = query.Where(p => p.Id != excludePaymentId.Value);
            }

            return await query
                .OrderBy(p => p.PaymentDate)
                .ThenBy(p => p.CreatedAt)
                .ThenBy(p => p.Id)
                .ToListAsync();
        }

        public async Task<IEnumerable<BankAccount>> GetBankAccountsAsync()
        {
            return await _context.BankAccounts
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.AccountName)
                .ToListAsync();
        }

        public async Task<IEnumerable<CashBox>> GetCashboxesAsync()
        {
            return await _context.CashBoxes
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<MobileWallet>> GetMobileWalletsAsync()
        {
            return await _context.MobileWallets
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.WalletName)
                .ToListAsync();
        }
    }
}
