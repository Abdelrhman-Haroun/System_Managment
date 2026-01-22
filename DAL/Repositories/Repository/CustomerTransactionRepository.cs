using DAL.Data;
using DAL.Models;
using DAL.Repositories.IRepository;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories.Repository
{
    public class CustomerTransactionRepository : GenericRepository<CustomerTransaction>, ICustomerTransactionRepository
    {
        private readonly ApplicationDbContext _context;
        public CustomerTransactionRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CustomerTransaction>> GetByCustomerIdAsync(int customerId)
        {
            return await _context.CustomerTransactions.Where(ct => ct.CustomerId == customerId && !ct.IsDeleted)
                .OrderByDescending(ct => ct.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<CustomerTransaction>> GetByInvoiceIdAsync(int invoiceId)
        {
            return await _context.CustomerTransactions.Where(ct => ct.InvoiceId == invoiceId && !ct.IsDeleted)
                .OrderByDescending(ct => ct.CreatedAt)
                .ToListAsync();
        }
    }
}
