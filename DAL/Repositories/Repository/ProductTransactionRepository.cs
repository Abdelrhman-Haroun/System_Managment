using DAL.Data;
using DAL.Models;
using DAL.Repositories.IRepository;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories.Repository
{
    public class ProductTransactionRepository : GenericRepository<ProductTransaction>, IProductTransactionRepository
    {
        private readonly ApplicationDbContext _context;
        public ProductTransactionRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ProductTransaction>> GetByProductIdAsync(int productId)
        {
            return await _context.ProductTransactions.Where(pt => pt.ProductId == productId && !pt.IsDeleted)
                .OrderByDescending(pt => pt.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProductTransaction>> GetByInvoiceIdAsync(int invoiceId)
        {
            return await _context.ProductTransactions.Where(pt => pt.InvoiceId == invoiceId && !pt.IsDeleted)
                .OrderByDescending(pt => pt.CreatedAt)
                .ToListAsync();
        }
    }
}
